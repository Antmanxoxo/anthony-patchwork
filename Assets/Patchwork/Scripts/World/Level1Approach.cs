using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The way in. The Pod now comes down well outside town, in a meadow in the south-east corner
    // of the (bigger) toybox, so the first thing you see is Toybox Town across the fields: the
    // wall and its towers, the keep and the chapel spire over the roofs. The King's Road winds
    // up to it, over the River Weft on a stone bridge, past a little hamlet of cottages and
    // stalls that has grown up outside the walls, to the Toybox Gate.
    //
    // Also here: the new land the bigger box opened up all round the old one (more fields to the
    // east, more wood to the west and south-west, meadow and orchard trees to the north).
    public static partial class Level1Builder
    {
        static Vector2[] roadIn;

        // From the Pod to the Toybox Gate.
        public static Vector2[] RoadIn
        {
            get
            {
                if (roadIn == null)
                    roadIn = Smooth(new[]
                    {
                        new Vector2(SpawnPoint.x, SpawnPoint.z + 3f), new Vector2(106f, -123f), new Vector2(102f, -113f), new Vector2(94f, -102f),
                        new Vector2(84f, -91f), new Vector2(72f, -80f), new Vector2(62f, -71.5f), new Vector2(MainGate.x, MainGate.z + 1.5f),
                    }, 3);
                return roadIn;
            }
        }

        // ---------------------------------------------------------------- Flat ribbons (roads, streets)

        // A strip of ground following a path, a few centimetres proud of the grass, solid (so
        // nothing grows through it). Its ends can be skipped where something else (a bridge)
        // takes over.
        static Transform Ribbon(LevelKit kit, Transform parent, string name, Vector2[] path, float width, float y, Material mat, System.Func<Vector2, bool> skip = null)
        {
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            bool open = false;
            for (int i = 0; i < path.Length; i++)
            {
                Vector2 p = path[i];
                if (skip != null && skip(p)) { open = false; continue; }
                Vector2 dir = (path[Mathf.Min(i + 1, path.Length - 1)] - path[Mathf.Max(i - 1, 0)]).normalized;
                Vector2 n = new Vector2(-dir.y, dir.x) * (width * 0.5f);
                Vector2 l = p + n, r = p - n;
                verts.Add(new Vector3(l.x, y, l.y)); verts.Add(new Vector3(r.x, y, r.y));
                uvs.Add(l / 2.5f); uvs.Add(r / 2.5f);
                int k = verts.Count - 2;
                if (open)
                {
                    tris.Add(k - 2); tris.Add(k); tris.Add(k - 1);
                    tris.Add(k - 1); tris.Add(k); tris.Add(k + 1);
                }
                open = true;
            }
            if (tris.Count == 0) return null;
            var root = parent != null ? parent : kit.Root;
            HillPiece(root, name, verts, uvs, tris, mat, false);
            return root.Find(name);
        }

        // ---------------------------------------------------------------- The approach

        static void Approach(LevelKit kit)
        {
            Physics.SyncTransforms();
            var root = kit.Group("KingsRoad", Vector3.zero);
            var earth = PatchworkAssets.Felt(new Color(0.84f, 0.72f, 0.52f));
            var verge = PatchworkAssets.Felt(new Color(0.72f, 0.6f, 0.42f));
            var road = RoadIn;

            // Where the road crosses the river.
            int cross = 0;
            float best = float.MaxValue;
            for (int i = 1; i < road.Length - 1; i++)
            {
                float t, d = PathDistance(road[i], RiverSouth, out t);
                if (d < best) { best = d; cross = i; }
            }
            Vector2 bc = road[cross];
            Vector2 bdir = (road[cross + 1] - road[cross - 1]).normalized;
            const float span = 23f;
            System.Func<Vector2, bool> onBridge = p => Vector2.Distance(p, bc) < span * 0.5f - 0.4f;
            Ribbon(kit, root, "RoadVerge", road, 6.6f, 0.025f, verge, onBridge);
            Ribbon(kit, root, "Road", road, 5f, 0.045f, earth, onBridge);
            StoneBridge(kit, root, new Vector3(bc.x, 0f, bc.y), Mathf.Atan2(bdir.x, bdir.y) * Mathf.Rad2Deg, span, 5.6f, 1.7f);
            kit.Info.MapPins.Add(new MapPin("Weft Bridge", new Vector3(bc.x, 0f, bc.y)));

            // Lantern posts and milestones along the way, on alternate sides.
            float run = 0f, next = 10f;
            int side = 1;
            for (int i = 1; i < road.Length; i++)
            {
                run += Vector2.Distance(road[i - 1], road[i]);
                if (run < next) continue;
                next = run + 15f;
                if (Vector2.Distance(road[i], bc) < span * 0.5f + 2f) continue;
                Vector2 dir = (road[Mathf.Min(i + 1, road.Length - 1)] - road[i - 1]).normalized;
                Vector2 q = road[i] + new Vector2(-dir.y, dir.x) * side * 3.9f;
                if (RiverEdgeDistance(new Vector3(q.x, 0f, q.y)) < 3f) continue;
                LanternPost(kit, new Vector3(q.x, 0f, q.y));
                side = -side;
            }
            // A signpost where you land, and another by the bridge.
            Signpost(kit, new Vector3(SpawnPoint.x - 3.5f, 0f, SpawnPoint.z + 6f), new[] { -38f, 70f }, new[] { LevelKit.Palette.Rose, LevelKit.Palette.Sky });
            Vector2 sb = bc - bdir * (span * 0.5f + 3f) + new Vector2(-bdir.y, bdir.x) * 4.2f;
            Signpost(kit, new Vector3(sb.x, 0f, sb.y), new[] { -30f, 150f }, new[] { LevelKit.Palette.Butter, LevelKit.Palette.Sage });

            Hamlet(kit, root, road);
            ApproachLife(kit, road, bc, bdir);
            GateTracks(kit, root, earth, verge);
            Fields(kit);
            NewLand(kit);
        }

        // Dirt tracks out of the East and North Gates, so every gate leads somewhere: east to the
        // Wildflower Meadow, north up through the orchard. (Fields and trees keep off them.)
        static readonly List<Vector2[]> tracks = new List<Vector2[]>();

        static void GateTracks(LevelKit kit, Transform root, Material earth, Material verge)
        {
            tracks.Clear();
            tracks.Add(Smooth(new[] { new Vector2(EastGateAt.x + 2.4f, EastGateAt.z), new Vector2(104f, 8f), new Vector2(109f, 13f), new Vector2(111f, 19f) }, 3));
            tracks.Add(Smooth(new[] { new Vector2(NorthGateAt.x, NorthGateAt.z + 2.6f), new Vector2(9f, 130f), new Vector2(12.5f, 138f), new Vector2(15f, 146f) }, 3));
            foreach (var t in tracks)
            {
                Ribbon(kit, root, "TrackVerge", t, 4.4f, 0.025f, verge);
                Ribbon(kit, root, "Track", t, 3.2f, 0.045f, earth);
                Vector2 end = t[t.Length - 1], d = (end - t[t.Length - 2]).normalized;
                Vector2 sp = end + new Vector2(-d.y, d.x) * 2.6f;
                Signpost(kit, new Vector3(sp.x, 0f, sp.y), new[] { Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg, Mathf.Atan2(-d.x, -d.y) * Mathf.Rad2Deg }, new[] { LevelKit.Palette.Sage, LevelKit.Palette.Rose });
            }
        }

        static bool OnTrack(Vector3 p, float pad)
        {
            foreach (var t in tracks)
            {
                float a;
                if (PathDistance(new Vector2(p.x, p.z), t, out a) < 2.2f + pad) return true;
            }
            return false;
        }

        // A humpbacked stone bridge: a deck that rises and falls, parapets with a coping and
        // little pillars at the ends, a stone arch over the water and cutwater piers.
        static void StoneBridge(LevelKit kit, Transform parent, Vector3 centre, float yaw, float length, float width, float rise)
        {
            var stone = PatchworkAssets.Cobble(new Color(0.8f, 0.77f, 0.72f));
            var coping = PatchworkAssets.Cobble(new Color(0.9f, 0.87f, 0.8f));
            var dark = PatchworkAssets.Cobble(new Color(0.58f, 0.55f, 0.52f));
            var rot = Quaternion.Euler(0f, yaw, 0f);
            const int segs = 14;
            for (int i = 0; i < segs; i++)
            {
                float t0 = -1f + 2f * i / segs, t1 = -1f + 2f * (i + 1) / segs;
                Vector3 p0 = centre + rot * new Vector3(0f, rise * (1f - t0 * t0), t0 * length * 0.5f);
                Vector3 p1 = centre + rot * new Vector3(0f, rise * (1f - t1 * t1), t1 * length * 0.5f);
                Vector3 mid = (p0 + p1) * 0.5f;
                float seg = Vector3.Distance(p0, p1) + 0.06f;
                var look = Quaternion.LookRotation((p1 - p0).normalized, Vector3.up);
                var deck = kit.Block(mid + look * new Vector3(0f, -0.25f, 0f), new Vector3(width, 0.5f, seg), TownKit.Stone, "BridgeDeck", stone);
                deck.rotation = look;
                deck.localScale = new Vector3(width, 0.5f, seg);
                for (int s = -1; s <= 1; s += 2)
                {
                    var wall = kit.Block(mid + look * new Vector3(s * (width * 0.5f + 0.25f), 0.45f, 0f), new Vector3(0.5f, 1.4f, seg), TownKit.Stone, "Parapet", stone);
                    wall.rotation = look;
                    wall.localScale = new Vector3(0.5f, 1.4f, seg);
                    kit.Deco(PrimitiveType.Cube, mid + look * new Vector3(s * (width * 0.5f + 0.25f), 1.2f, 0f), new Vector3(0.7f, 0.14f, seg), coping, look.eulerAngles, parent);
                }
            }
            // End pillars with stone balls.
            for (int e = -1; e <= 1; e += 2)
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 p = centre + rot * new Vector3(s * (width * 0.5f + 0.3f), 0f, e * (length * 0.5f + 0.2f));
                    kit.Block(p + Vector3.up * 0.75f, new Vector3(0.9f, 1.5f, 0.9f), TownKit.Stone, "BridgePillar", coping);
                    kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 1.75f, Vector3.one * 0.6f, coping, default(Vector3), parent);
                }
            // The arch and its spandrels, seen from the river.
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 face = centre + rot * new Vector3(s * (width * 0.5f + 0.22f), 0f, 0f);
                kit.Deco(PrimitiveType.Cylinder, face + Vector3.up * (rise - 4.6f), new Vector3(9.6f, 0.05f, length * 0.5f), dark, (rot * Quaternion.Euler(0f, 0f, 90f)).eulerAngles, parent);
            }
            var under = kit.Deco(PrimitiveType.Cube, centre + Vector3.up * (rise * 0.5f - 2.4f), new Vector3(width + 0.3f, rise + 2.6f, length * 0.62f), stone, new Vector3(0f, yaw, 0f), parent);
            under.name = "BridgeBody";
            // Cutwaters: pointed piers either side of the arch where it meets the water.
            for (int e = -1; e <= 1; e += 2)
            {
                Vector3 p = centre + rot * new Vector3(0f, 0f, e * length * 0.3f);
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Cube, p + rot * new Vector3(s * (width * 0.5f + 0.5f), CanalSurface - 0.4f, 0f), new Vector3(1.1f, 2.6f, 1.1f), dark, new Vector3(0f, yaw + 45f, 0f), parent);
            }
            ClearGrass(centre, length * 0.55f);
        }

        // Cottages, a smithy's lean-to, market stalls and a well, outside the gate along the road.
        static void Hamlet(LevelKit kit, Transform root, Vector2[] road)
        {
            var rng = new System.Random(404);
            System.Func<float> R = () => (float)rng.NextDouble();
            Color thatch = new Color(0.86f, 0.72f, 0.42f);
            Color[] stripes = { LevelKit.Palette.Rose, LevelKit.Palette.Sky, LevelKit.Palette.Butter, LevelKit.Palette.Sage };
            int mask = ~(1 << 2);
            int built = 0, stalls = 0;
            float total = 0f;
            for (int i = 0; i < road.Length - 1; i++) total += Vector2.Distance(road[i], road[i + 1]);
            float run = 0f, next = total * 0.62f;
            for (int i = 1; i < road.Length - 1 && run < total * 0.86f; i++)
            {
                run += Vector2.Distance(road[i - 1], road[i]);
                if (run < next) continue;
                next = run + 7.5f;
                Vector2 dir = (road[i + 1] - road[i - 1]).normalized, n = new Vector2(-dir.y, dir.x);
                for (int s = -1; s <= 1; s += 2)
                {
                    bool stall = (built + s) % 3 == 0;
                    Vector2 q = road[i] + n * s * (stall ? 5.2f : 8.6f + R() * 1.5f);
                    var at = new Vector3(q.x, 0f, q.y);
                    // Face the road (snapped to the nearest quarter-turn, as the houses are built square).
                    Vector2 face = -n * s;
                    float yaw = Mathf.Round(Mathf.Atan2(face.x, face.y) * Mathf.Rad2Deg / 90f) * 90f;
                    float r = stall ? 2.2f : 4.4f;
                    if (Physics.CheckBox(at + Vector3.up * 1.6f, new Vector3(r, 1.3f, r), Quaternion.identity, mask, QueryTriggerInteraction.Ignore)) continue;
                    if (Taken(at, r) || RiverEdgeDistance(at) < r + 2f) continue;
                    Take(at, r);
                    if (stall)
                    {
                        MarketStall(kit, at, Mathf.Atan2(face.x, face.y) * Mathf.Rad2Deg, stripes[stalls % stripes.Length], stalls % 4);
                        stalls++;
                        continue;
                    }
                    var size = new Vector3(5.6f + R() * 1.4f, 3.2f + (R() < 0.4f ? 3f : 0f), 4.8f + R());
                    TownKit.MakeHouse(kit, at, size, yaw, Walls[rng.Next(Walls.Length)], thatch, 300 + built, rng.Next(3) - 1, 0f);
                    // A little garden: a picket of flowers and a crate or a barrel.
                    var rot = Quaternion.Euler(0f, yaw, 0f);
                    for (int f = 0; f < 4; f++)
                        kit.Flower(at + rot * new Vector3(-size.x * 0.5f + 0.6f + f * 1.2f, 0f, size.z * 0.5f + 0.9f), FlowerColors[(built + f) % FlowerColors.Length]);
                    TownKit.Crate(kit, at + rot * new Vector3(size.x * 0.5f + 0.9f, 0f, size.z * 0.5f - 0.6f), 0.9f, yaw + 12f);
                    built++;
                }
            }
            // The hamlet's well, on the bend nearest the gate.
            Vector2 w = road[Mathf.Clamp(Mathf.RoundToInt(road.Length * 0.8f), 1, road.Length - 2)];
            Vector2 wd = (road[Mathf.Clamp(Mathf.RoundToInt(road.Length * 0.8f) + 1, 1, road.Length - 1)] - w).normalized;
            Vector2 wq = w + new Vector2(-wd.y, wd.x) * -5.5f;
            var wellAt = new Vector3(wq.x, 0f, wq.y);
            if (!Physics.CheckBox(wellAt + Vector3.up * 1.4f, new Vector3(1.6f, 1.2f, 1.6f), Quaternion.identity, mask, QueryTriggerInteraction.Ignore) && !Taken(wellAt, 2f))
            {
                Take(wellAt, 2f);
                TownWell(kit, wellAt);
            }
            kit.Info.MapPins.Add(new MapPin("Gate Hamlet", new Vector3(road[Mathf.RoundToInt(road.Length * 0.78f)].x, 0f, road[Mathf.RoundToInt(road.Length * 0.78f)].y), MapPin.Kind.Area));
        }

        // Hedged fields either side of the road and along the toybox's east side: soil with rows
        // of crops (cabbages, wheat, pumpkins) and a scarecrow keeping an eye on things.
        static void Fields(LevelKit kit)
        {
            var soil = PatchworkAssets.Felt(new Color(0.6f, 0.44f, 0.32f));
            var rng = new System.Random(515);
            var crops = new[] { new MeshBatch(), new MeshBatch(), new MeshBatch(), new MeshBatch() };
            Material[] cropMats =
            {
                PatchworkAssets.Felt(new Color(0.55f, 0.78f, 0.45f)),   // cabbages
                PatchworkAssets.Felt(new Color(0.95f, 0.82f, 0.45f)),   // wheat
                PatchworkAssets.Felt(new Color(0.98f, 0.6f, 0.28f)),    // pumpkins
                PatchworkAssets.Felt(new Color(0.4f, 0.62f, 0.34f)),    // leaves
            };
            // (x0, z0, x1, z1): kept clear of the road, the river and the town wall.
            var plots = new[]
            {
                new Vector4(112f, -104f, 138f, -86f), new Vector4(118f, -58f, 144f, -36f),
                new Vector4(130f, -30f, 146f, -6f), new Vector4(122f, 0f, 144f, 22f), new Vector4(70f, -146f, 96f, -134f),
                new Vector4(-30f, -146f, -4f, -132f), new Vector4(2f, -146f, 26f, -132f), new Vector4(80f, 118f, 104f, 140f),
                new Vector4(110f, 118f, 140f, 140f),
            };
            int field = 0;
            foreach (var f in plots)
            {
                var c = new Vector3((f.x + f.z) * 0.5f, 0f, (f.y + f.w) * 0.5f);
                var size = new Vector2(f.z - f.x, f.w - f.y);
                // Not if the river, the road or anything solid is in the way.
                bool blocked = false;
                for (float x = f.x; x <= f.z && !blocked; x += 3f)
                    for (float z = f.y; z <= f.w && !blocked; z += 3f)
                    {
                        var p = new Vector3(x, 0f, z);
                        if (RiverEdgeDistance(p) < 3f) blocked = true;
                        float t;
                        if (PathDistance(new Vector2(x, z), RoadIn, out t) < 5f || OnTrack(p, 1f)) blocked = true;
                    }
                if (blocked || Physics.CheckBox(c + Vector3.up * 1.6f, new Vector3(size.x * 0.5f, 1.3f, size.y * 0.5f), Quaternion.identity, ~(1 << 2), QueryTriggerInteraction.Ignore)) continue;
                kit.Block(c + Vector3.up * 0.02f, new Vector3(size.x, 0.1f, size.y), new Color(0.6f, 0.44f, 0.32f), "FieldSoil", soil);
                // Hedges round three sides, a gap in the fourth.
                Hedge(kit, new Vector3(f.x, 0f, f.y), new Vector3(f.z, 0f, f.y));
                Hedge(kit, new Vector3(f.x, 0f, f.w), new Vector3(f.z, 0f, f.w));
                Hedge(kit, new Vector3(f.x, 0f, f.y), new Vector3(f.x, 0f, f.w - 4f));
                int kind = field % 3;
                for (float z = f.y + 1.5f; z < f.w - 1f; z += 2.2f)
                    for (float x = f.x + 1.2f; x < f.z - 1f; x += kind == 1 ? 0.7f : 1.6f)
                    {
                        Vector3 p = new Vector3(x + ((float)rng.NextDouble() - 0.5f) * 0.3f, 0.07f, z);
                        if (kind == 0) crops[0].Ball(p + Vector3.up * 0.25f, new Vector3(0.7f, 0.5f, 0.7f), 3, 6);
                        else if (kind == 1) crops[1].Blade(p, 0.7f + (float)rng.NextDouble() * 0.3f, 0.12f, new Vector3(0.05f, 0f, 0.02f), (float)rng.NextDouble() * 360f);
                        else if (rng.NextDouble() < 0.4) { crops[2].Ball(p + Vector3.up * 0.3f, new Vector3(0.75f, 0.55f, 0.75f), 3, 7); crops[3].Ball(p + new Vector3(0.5f, 0.12f, 0.2f), new Vector3(0.5f, 0.15f, 0.4f), 2, 6); }
                        else crops[3].Ball(p + Vector3.up * 0.1f, new Vector3(0.45f, 0.18f, 0.4f), 2, 6);
                    }
                if (field % 2 == 0) Scarecrow(kit, c + new Vector3(size.x * 0.2f, 0.07f, 0f), 180f + field * 20f);
                field++;
            }
            var g = kit.Group("FieldCrops", Vector3.zero);
            for (int i = 0; i < crops.Length; i++) crops[i].Build(g, "Crops", cropMats[i], i != 1);
        }

        // The land the bigger toybox opened up all round the old one.
        static void NewLand(LevelKit kit)
        {
            var rng = new System.Random(616);
            System.Func<float> R = () => (float)rng.NextDouble();
            int mask = ~(1 << 2);
            System.Func<Vector3, float, bool> clear = (p, r) =>
                !Physics.CheckSphere(p + Vector3.up * (r + 0.3f), r, mask, QueryTriggerInteraction.Ignore) && !Taken(p, r) && RiverEdgeDistance(p) > r + 2f && !OnTrack(p, r);
            Color[] greens = { new Color(0.34f, 0.52f, 0.3f), new Color(0.4f, 0.58f, 0.32f), new Color(0.3f, 0.48f, 0.34f) };
            // The wood carries on west and south-west, right up to the toybox walls.
            for (int i = 0; i < 240; i++)
            {
                Vector3 p = R() < 0.55f ? new Vector3(-Half + 3f + R() * 34f, 0f, -Half + 3f + R() * (Half * 2f - 6f))
                                         : new Vector3(-Half + 3f + R() * (Half - 40f), 0f, -Half + 3f + R() * 33f);
                if (p.x > RavineWest - 4f || !clear(p, 2.2f)) continue;
                PomTree(kit, p, 6f + R() * 4f, greens[i % greens.Length]);
            }
            // Orchard rows north of town, and a scatter of blossom trees along the north wall.
            for (int row = 0; row < 4; row++)
                for (int k = 0; k < 9; k++)
                {
                    Vector3 p = new Vector3(-30f + k * 7f + (row % 2) * 3.5f, 0f, 126f + row * 6.5f);
                    if (!clear(p, 1.6f)) continue;
                    kit.YarnTree(p, 4.5f + R() * 1.5f, FlowerColors[(row + k) % FlowerColors.Length]);
                }
            // Wildflowers and the odd tree on the east side, between the fields.
            for (int i = 0; i < 160; i++)
            {
                Vector3 p = new Vector3(Half - 36f + R() * 33f, 0f, -Half + 4f + R() * (Half * 2f - 8f));
                if (RiverEdgeDistance(p) < 2f) continue;
                if (i % 8 == 0) { if (clear(p, 2f)) PomTree(kit, p, 5f + R() * 3f, greens[i % greens.Length]); }
                else if (!Physics.CheckSphere(p + Vector3.up * 0.5f, 0.3f, mask, QueryTriggerInteraction.Ignore)) kit.Flower(p, FlowerColors[i % FlowerColors.Length]);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The water of Toybox Town, now that the toybox is bigger.
    //   - Inside the town wall the Inkwell Canal is a straight cut between stone quays, from the
    //     south water gate to the north one.
    //   - Outside the wall it becomes the River Weft: a real winding river with soft grassy
    //     banks, a sandy shore, reeds, rocks and lily pads. South of town it bends away east,
    //     under the stone bridge on the road in from the Pod, and out under the east wall of the
    //     toybox; north of town it wanders up past the farm and out under the north wall.
    //   - Down in Thread Ravine the brook no longer runs ruler-straight: it meanders from bank to
    //     bank over a stony bed (pinned straight at the stepping stones, so they still line up).
    // The meadows either side of the river are one big gently-carved ground mesh, so the river
    // can bend anywhere; inside the walls the ground is still the flat town boxes.
    public static partial class Level1Builder
    {
        // Where the straight canal meets the river (the two water gates in the town wall).
        public const float CanalSouthEnd = -73f, CanalNorthEnd = 111f;
        const float RiverBed = -4.2f;

        static Vector2[] riverSouth, riverNorth;

        // The south river, from the water gate down past the orchard, then east under the road.
        public static Vector2[] RiverSouth
        {
            get
            {
                if (riverSouth == null)
                    riverSouth = Smooth(new[]
                    {
                        new Vector2(36f, CanalSouthEnd), new Vector2(36f, -86f), new Vector2(36.4f, -97f), new Vector2(39f, -110f),
                        new Vector2(50f, -122f), new Vector2(68f, -128f), new Vector2(88f, -126f), new Vector2(104f, -120f),
                        new Vector2(118f, -118f), new Vector2(134f, -114f), new Vector2(Half + 4f, -112f),
                    }, 4);
                return riverSouth;
            }
        }

        // The north river, from the north water gate up past the farm and out of the box.
        public static Vector2[] RiverNorth
        {
            get
            {
                if (riverNorth == null)
                    riverNorth = Smooth(new[]
                    {
                        new Vector2(36f, CanalNorthEnd), new Vector2(36f, 119f), new Vector2(32f, 128f), new Vector2(36f, 137f),
                        new Vector2(48f, 142f), new Vector2(62f, 144f), new Vector2(72f, Half + 4f),
                    }, 4);
                return riverNorth;
            }
        }

        // The river's half-width along its length (t = 0 at the water gate): it opens out from
        // the canal's width to a proper river within the first few bends.
        static float RiverHalfWidth(float t) { return Mathf.Lerp(4f, 5.6f, Mathf.SmoothStep(0f, 1f, t / 0.18f)); }

        // The brook down the ravine: wanders up to 2 m either side of the middle, but runs
        // straight through the stepping stones at z -30.
        public static float StreamX(float z)
        {
            float u = z + 30f;
            float pin = Mathf.Clamp01(Mathf.Abs(u) / 8f);
            return -44f + (1.5f * Mathf.Sin(u / 12f) + 0.5f * Mathf.Sin(u / 4.6f)) * pin;
        }

        // Chaikin corner-cutting: turns a few control points into a smooth curve.
        static Vector2[] Smooth(Vector2[] pts, int passes)
        {
            var list = new List<Vector2>(pts);
            for (int p = 0; p < passes; p++)
            {
                var next = new List<Vector2> { list[0] };
                for (int i = 0; i < list.Count - 1; i++)
                {
                    next.Add(Vector2.Lerp(list[i], list[i + 1], 0.25f));
                    next.Add(Vector2.Lerp(list[i], list[i + 1], 0.75f));
                }
                next.Add(list[list.Count - 1]);
                list = next;
            }
            return list.ToArray();
        }

        // Distance from p to a path, and how far along it (0..1) the nearest point is.
        static float PathDistance(Vector2 p, Vector2[] path, out float along)
        {
            float best = float.MaxValue, total = 0f, at = 0f, run = 0f;
            for (int i = 0; i < path.Length - 1; i++) total += Vector2.Distance(path[i], path[i + 1]);
            for (int i = 0; i < path.Length - 1; i++)
            {
                Vector2 a = path[i], b = path[i + 1], ab = b - a;
                float len = ab.magnitude;
                float t = len < 1e-4f ? 0f : Mathf.Clamp01(Vector2.Dot(p - a, ab) / (len * len));
                float d = Vector2.Distance(p, a + ab * t);
                if (d < best) { best = d; at = run + len * t; }
                run += len;
            }
            along = total > 0f ? at / total : 0f;
            return best;
        }

        // How far p is from the edge of the river water (negative inside it). Used for sounds and
        // for keeping props out of the water.
        public static float RiverEdgeDistance(Vector3 p)
        {
            var q = new Vector2(p.x, p.z);
            float t, best = float.MaxValue;
            float d = PathDistance(q, RiverSouth, out t);
            best = Mathf.Min(best, d - RiverHalfWidth(t));
            d = PathDistance(q, RiverNorth, out t);
            best = Mathf.Min(best, d - RiverHalfWidth(t));
            return best;
        }

        // The height of the meadow ground outside the walls: flat, except where a river has worn
        // its channel (a soft grassy bank down to a sandy shore, then down to the river bed).
        static float MeadowHeight(float x, float z)
        {
            var q = new Vector2(x, z);
            float h = 0f;
            foreach (var path in new[] { RiverSouth, RiverNorth })
            {
                float t;
                float d = PathDistance(q, path, out t);
                float hw = RiverHalfWidth(t);
                float y;
                if (d >= hw + 2.4f) y = 0f;
                else if (d >= hw - 0.4f) y = Mathf.Lerp(0f, -0.95f, Mathf.SmoothStep(0f, 1f, (hw + 2.4f - d) / 2.8f));
                else y = Mathf.Lerp(-0.95f, RiverBed, Mathf.SmoothStep(0f, 1f, (hw - 0.4f - d) / 2.4f));
                h = Mathf.Min(h, y);
            }
            return h;
        }

        // The ravine floor: cobbled banks at -5, the brook's channel down to -8.
        static float RavineFloorHeight(float x, float z)
        {
            float d = Mathf.Abs(x - StreamX(z));
            const float hw = 2f;
            if (d >= hw + 0.6f) return -5f;
            if (d >= hw) return Mathf.Lerp(-5f, -5.6f, (hw + 0.6f - d) / 0.6f);
            return Mathf.Lerp(-5.6f, -8f, Mathf.SmoothStep(0f, 1f, (hw - d) / 0.9f));
        }

        // ---------------------------------------------------------------- Ground meshes

        // A height-field of ground from (x0,z0) to (x1,z1), split in two: cells above `wetBelow`
        // get the dry material, cells below it the wet one. Both are solid.
        static void HeightField(LevelKit kit, string name, float x0, float z0, float x1, float z1, float step,
            System.Func<float, float, float> height, float wetBelow, Material dry, Material wet, float uvScale)
        {
            int nx = Mathf.CeilToInt((x1 - x0) / step), nz = Mathf.CeilToInt((z1 - z0) / step);
            float sx = (x1 - x0) / nx, sz = (z1 - z0) / nz;
            var hgt = new float[nx + 1, nz + 1];
            for (int i = 0; i <= nx; i++)
                for (int j = 0; j <= nz; j++)
                    hgt[i, j] = height(x0 + i * sx, z0 + j * sz);
            var parts = new[] { new GroundPart(), new GroundPart() };
            for (int i = 0; i < nx; i++)
                for (int j = 0; j < nz; j++)
                {
                    float lo = Mathf.Min(Mathf.Min(hgt[i, j], hgt[i + 1, j]), Mathf.Min(hgt[i, j + 1], hgt[i + 1, j + 1]));
                    var part = parts[lo < wetBelow ? 1 : 0];
                    Vector3 a = new Vector3(x0 + i * sx, hgt[i, j], z0 + j * sz);
                    Vector3 b = new Vector3(x0 + i * sx, hgt[i, j + 1], z0 + (j + 1) * sz);
                    Vector3 c = new Vector3(x0 + (i + 1) * sx, hgt[i + 1, j], z0 + j * sz);
                    Vector3 d = new Vector3(x0 + (i + 1) * sx, hgt[i + 1, j + 1], z0 + (j + 1) * sz);
                    part.Quad(a, b, c, d, uvScale);
                }
            var root = kit.Group(name, Vector3.zero);
            if (parts[0].Verts.Count > 0) HillPiece(root, name + "Dry", parts[0].Verts, parts[0].Uvs, parts[0].Tris, dry, false);
            if (parts[1].Verts.Count > 0) HillPiece(root, name + "Wet", parts[1].Verts, parts[1].Uvs, parts[1].Tris, wet, false);
        }

        class GroundPart
        {
            public readonly List<Vector3> Verts = new List<Vector3>();
            public readonly List<Vector2> Uvs = new List<Vector2>();
            public readonly List<int> Tris = new List<int>();
            // a-b along +z, a-c along +x: wound so the top faces up.
            public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float uv)
            {
                int k = Verts.Count;
                Verts.Add(a); Verts.Add(b); Verts.Add(c); Verts.Add(d);
                Uvs.Add(new Vector2(a.x, a.z) / uv); Uvs.Add(new Vector2(b.x, b.z) / uv); Uvs.Add(new Vector2(c.x, c.z) / uv); Uvs.Add(new Vector2(d.x, d.z) / uv);
                Tris.Add(k); Tris.Add(k + 1); Tris.Add(k + 2);
                Tris.Add(k + 2); Tris.Add(k + 1); Tris.Add(k + 3);
            }
        }

        // ---------------------------------------------------------------- Water along a path

        // A ribbon of water following a path: the satin surface (world-mapped, so it can bend),
        // a darker layer under it for depth, lace foam along both shores, and swimming volumes
        // all the way along. halfWidth(t) gives the water's half-width at t along the path.
        static void RibbonWater(LevelKit kit, string name, Vector2[] path, System.Func<float, float> halfWidth, float surface, float bottom, Vector2 flow)
        {
            var root = kit.Group(name, Vector3.zero);
            float total = 0f;
            for (int i = 0; i < path.Length - 1; i++) total += Vector2.Distance(path[i], path[i + 1]);
            // Points along the path with their sideways normal and width.
            var at = new List<Vector2>();
            var side = new List<Vector2>();
            var half = new List<float>();
            float run = 0f;
            for (int i = 0; i < path.Length; i++)
            {
                if (i > 0) run += Vector2.Distance(path[i - 1], path[i]);
                Vector2 dir = (path[Mathf.Min(i + 1, path.Length - 1)] - path[Mathf.Max(i - 1, 0)]).normalized;
                at.Add(path[i]);
                side.Add(new Vector2(-dir.y, dir.x));
                half.Add(halfWidth(total > 0f ? run / total : 0f));
            }
            System.Action<string, float, float, Material, bool> strip = (n, y, extra, mat, wobble) =>
            {
                var verts = new List<Vector3>();
                var uvs = new List<Vector2>();
                var tris = new List<int>();
                for (int i = 0; i < at.Count; i++)
                {
                    float w = half[i] + extra;
                    Vector2 l = at[i] + side[i] * w, r = at[i] - side[i] * w;
                    verts.Add(new Vector3(l.x, y, l.y)); verts.Add(new Vector3(r.x, y, r.y));
                    uvs.Add(l / 5f); uvs.Add(r / 5f);
                    if (i > 0)
                    {
                        int k = i * 2;
                        tris.Add(k - 2); tris.Add(k); tris.Add(k - 1);
                        tris.Add(k - 1); tris.Add(k); tris.Add(k + 1);
                    }
                }
                var mesh = new Mesh { name = n };
                if (verts.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.SetVertices(verts);
                mesh.SetUVs(0, uvs);
                mesh.SetTriangles(tris, 0);
                mesh.RecalculateNormals();
                if (mesh.normals.Length > 0 && mesh.normals[0].y < 0f)
                {
                    for (int i = 0; i < tris.Count; i += 3) { int t = tris[i]; tris[i] = tris[i + 1]; tris[i + 1] = t; }
                    mesh.SetTriangles(tris, 0);
                    mesh.RecalculateNormals();
                }
                mesh.RecalculateBounds();
                var go = new GameObject(n);
                go.transform.SetParent(root, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = mat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                go.AddComponent<OwnedMesh>().Mesh = mesh;
                if (wobble)
                {
                    var drift = go.AddComponent<PaintLake>();
                    drift.Drift = flow;
                    drift.Setup(mat);
                }
            };
            strip("WaterSurface", surface - 0.02f, 0.7f, PatchworkAssets.CraftWater(0.86f, Vector2.one), true);
            strip("WaterDepth", surface - 0.9f, 0.1f, PatchworkAssets.Bubble(new Color(0.12f, 0.3f, 0.6f, 0.5f)), false);

            // Lace foam scallops along both shores.
            var lace = PatchworkAssets.Felt(new Color(0.96f, 0.98f, 1f));
            var foam = new MeshBatch();
            float since = 0f;
            for (int i = 1; i < at.Count; i++)
            {
                since += Vector2.Distance(at[i - 1], at[i]);
                if (since < 0.6f) continue;
                since = 0f;
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector2 q = at[i] + side[i] * s * (half[i] + 0.45f);
                    foam.Ball(new Vector3(q.x, surface + 0.02f, q.y), new Vector3(0.3f, 0.05f, 0.3f), 2, 6);
                }
            }
            foam.Build(root, "FoamScallops", lace);

            // Swimming: a chain of boxes along the river, each a little longer than its piece so
            // there are no gaps at the bends. (The water only acts on you below the surface, so
            // the bits of bank inside a box don't matter.)
            float step = 0f;
            int from = 0;
            for (int i = 1; i < at.Count; i++)
            {
                step += Vector2.Distance(at[i - 1], at[i]);
                if (step < 3f && i < at.Count - 1) continue;
                float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
                for (int j = from; j <= i; j++)
                {
                    float w = half[j] + 0.3f;
                    minX = Mathf.Min(minX, at[j].x - w); maxX = Mathf.Max(maxX, at[j].x + w);
                    minZ = Mathf.Min(minZ, at[j].y - w); maxZ = Mathf.Max(maxZ, at[j].y + w);
                }
                var c = new Vector3((minX + maxX) * 0.5f, (bottom + surface + 0.6f) * 0.5f, (minZ + maxZ) * 0.5f);
                var size = new Vector3(maxX - minX, surface + 0.6f - bottom, maxZ - minZ);
                var body = kit.Volume<WaterBody>(c, size, "WaterBody");
                body.SurfaceY = surface;
                body.transform.SetParent(root, true);
                from = i;
                step = 0f;
            }
        }

        // ---------------------------------------------------------------- The meadow ground and rivers

        static void RiverGround(LevelKit kit)
        {
            var grass = PatchworkAssets.Grass(GrassCol);
            var sand = PatchworkAssets.Felt(new Color(0.86f, 0.8f, 0.62f));
            // South of the town (all the way to the toybox's south wall) and north of it.
            HeightField(kit, "SouthMeadow", RavineEast, -Half, Half, CanalSouthEnd, 1.5f, MeadowHeight, -0.62f, grass, sand, 2.5f);
            HeightField(kit, "NorthMeadow", RavineEast, CanalNorthEnd, Half, Half, 1.5f, MeadowHeight, -0.62f, grass, sand, 2.5f);
            // A darker bed under the sandy shallows, so the deep middle of the river reads as deep.
            var deepBed = PatchworkAssets.Felt(new Color(0.12f, 0.2f, 0.42f));
            foreach (var path in new[] { RiverSouth, RiverNorth })
            {
                var bed = new MeshBatch();
                for (int i = 0; i < path.Length; i += 2)
                    bed.Ball(new Vector3(path[i].x, RiverBed + 0.05f, path[i].y), new Vector3(6.2f, 0.2f, 6.2f), 2, 8);
                bed.Build(kit.Group("RiverBed", Vector3.zero), "DeepBed", deepBed);
            }
            RibbonWater(kit, "RiverWeftSouth", RiverSouth, RiverHalfWidth, CanalSurface, RiverBed - 0.2f, new Vector2(0.01f, -0.06f));
            RibbonWater(kit, "RiverWeftNorth", RiverNorth, RiverHalfWidth, CanalSurface, RiverBed - 0.2f, new Vector2(0.01f, 0.06f));
            // Stone headwalls where the canal's quays end and the soft banks begin, under the water gates.
            var stone = PatchworkAssets.Cobble(TownKit.Stone);
            foreach (float z in new[] { CanalSouthEnd, CanalNorthEnd })
            {
                float s = z < 0f ? -1f : 1f;
                for (int side = -1; side <= 1; side += 2)
                {
                    float x = side < 0 ? CanalWest - 1.4f : CanalEast + 1.4f;
                    kit.Block(new Vector3(x, -2.3f, z + s * 0.6f), new Vector3(3f, 4.8f, 1.2f), TownKit.Stone, "Headwall", stone);
                }
            }
            RiverDressing(kit);
            RavineFloor(kit);
        }

        // Reeds, rocks, lily pads and stepping stones along the river, so it doesn't read as a
        // painted strip: busier at the bends, quieter on the straights.
        static void RiverDressing(LevelKit kit)
        {
            var rng = new System.Random(88);
            System.Func<float> R = () => (float)rng.NextDouble();
            var reeds = new MeshBatch();
            var bulrush = new MeshBatch();
            var rocks = new MeshBatch();
            var pads = new MeshBatch();
            var blooms = new MeshBatch();
            foreach (var path in new[] { RiverSouth, RiverNorth })
            {
                float total = 0f;
                for (int i = 0; i < path.Length - 1; i++) total += Vector2.Distance(path[i], path[i + 1]);
                float run = 0f;
                for (int i = 1; i < path.Length - 1; i++)
                {
                    run += Vector2.Distance(path[i - 1], path[i]);
                    if (run < 14f) continue;   // keep the water gate clear
                    Vector2 a = path[i - 1], b = path[i], c = path[i + 1];
                    Vector2 dir = (c - a).normalized, n = new Vector2(-dir.y, dir.x);
                    // How sharply it's bending here, and which side is the outside of the bend.
                    float bend = Vector2.SignedAngle(b - a, c - b);
                    float hw = RiverHalfWidth(run / total);
                    for (int s = -1; s <= 1; s += 2)
                    {
                        bool inside = Mathf.Sign(bend) == s;
                        // Reed clumps on the shore, thicker on the inside of the bends.
                        if (R() < (inside ? 0.55f : 0.22f))
                        {
                            Vector2 q = b + n * s * (hw + 0.5f + R() * 1.2f) + dir * (R() - 0.5f) * 1.6f;
                            float y = MeadowHeight(q.x, q.y);
                            for (int k = 0; k < 7; k++)
                            {
                                float a2 = R() * Mathf.PI * 2f;
                                Vector3 root = new Vector3(q.x + Mathf.Cos(a2) * R() * 0.6f, y, q.y + Mathf.Sin(a2) * R() * 0.6f);
                                reeds.Blade(root, 1.1f + R() * 0.9f, 0.07f, new Vector3((R() - 0.5f) * 0.3f, 0f, (R() - 0.5f) * 0.3f), R() * 360f);
                                if (k == 0 && R() < 0.6f) bulrush.Ball(root + Vector3.up * (1.5f + R() * 0.4f), new Vector3(0.12f, 0.4f, 0.12f), 3, 5);
                            }
                        }
                        // Round river stones on the outside of the bends, half in the water.
                        if (!inside && R() < 0.35f)
                        {
                            Vector2 q = b + n * s * (hw + 0.2f) + dir * (R() - 0.5f) * 2f;
                            float sc = 0.6f + R() * 0.9f;
                            rocks.Ball(new Vector3(q.x, MeadowHeight(q.x, q.y) + sc * 0.2f, q.y), new Vector3(sc * 1.5f, sc * 0.8f, sc * 1.2f), 3, 6);
                        }
                    }
                    // Lily pads in the slack water on the inside of the bends.
                    if (Mathf.Abs(bend) > 2f && R() < 0.5f)
                    {
                        float s = Mathf.Sign(bend);
                        for (int k = 0; k < 3; k++)
                        {
                            Vector2 q = b + n * s * (hw - 1f - R() * 1.6f) + dir * (R() - 0.5f) * 3f;
                            float sc = 0.7f + R() * 0.5f;
                            pads.Cylinder(new Vector3(q.x, CanalSurface + 0.01f, q.y), sc * 0.6f, 0.03f, 10);
                            if (R() < 0.3f) blooms.Ball(new Vector3(q.x, CanalSurface + 0.12f, q.y), new Vector3(0.28f, 0.18f, 0.28f), 3, 6);
                        }
                    }
                }
            }
            var g = kit.Group("RiverDressing", Vector3.zero);
            reeds.Build(g, "Reeds", PatchworkAssets.Felt(new Color(0.45f, 0.62f, 0.32f)));
            bulrush.Build(g, "Bulrushes", PatchworkAssets.Felt(new Color(0.48f, 0.32f, 0.22f)));
            rocks.Build(g, "RiverStones", PatchworkAssets.Cobble(new Color(0.7f, 0.68f, 0.64f)), true);
            pads.Build(g, "LilyPads", PatchworkAssets.Felt(new Color(0.42f, 0.66f, 0.38f)));
            blooms.Build(g, "LilyFlowers", PatchworkAssets.Felt(new Color(1f, 0.78f, 0.84f)));
        }

        // The ravine's floor and its winding brook.
        static void RavineFloor(LevelKit kit)
        {
            var bankStone = PatchworkAssets.Cobble(new Color(0.6f, 0.58f, 0.55f));
            var deepBed = PatchworkAssets.Felt(new Color(0.12f, 0.2f, 0.42f));
            HeightField(kit, "RavineFloor", RavineWest, -Half, RavineEast, Half, 1f, RavineFloorHeight, -5.3f, bankStone, deepBed, 2.5f);
            var path = new List<Vector2>();
            for (float z = -Half; z <= Half + 0.01f; z += 1.5f) path.Add(new Vector2(StreamX(z), z));
            RibbonWater(kit, "Brook", path.ToArray(), t => 2.05f, StreamSurface, -8f, new Vector2(0f, 0.05f));
            // A few little riffles where the brook swings hardest: a line of foam across the water.
            var lace = PatchworkAssets.Felt(new Color(0.96f, 0.98f, 1f));
            var riffle = new MeshBatch();
            for (float z = -Half + 6f; z < Half - 6f; z += 1.5f)
            {
                float swing = Mathf.Abs(StreamX(z + 1f) - StreamX(z - 1f));
                if (swing < 0.55f || Mathf.Repeat(z, 9f) > 1.5f) continue;
                for (int k = -2; k <= 2; k++)
                    riffle.Ball(new Vector3(StreamX(z) + k * 0.7f, StreamSurface + 0.02f, z + Mathf.Sin(k) * 0.3f), new Vector3(0.45f, 0.05f, 0.3f), 2, 6);
            }
            riffle.Build(kit.Group("BrookRiffles", Vector3.zero), "Riffles", lace);
        }
    }
}

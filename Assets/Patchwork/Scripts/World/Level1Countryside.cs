using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Landmarks out in the countryside round the town, in the new land the bigger toybox opened:
    //   - the Lists: a proper tournament ground south of the orchard, with the tilt barrier,
    //     a stand under a striped awning, knights' pavilions, banners and a spinning quintain
    //   - the windmill on the fields by the Pod's meadow, the first thing you see turning
    //   - the Weft Mill, a little watermill on the river's long bend
    public static partial class Level1Builder
    {
        static readonly Vector3 ListsAt = new Vector3(-2f, 0f, -139f), WindmillAt = new Vector3(88f, 0f, -141f);

        static void Countryside(LevelKit kit)
        {
            Lists(kit, ListsAt);
            Windmill(kit, WindmillAt);
            WeftMill(kit, new Vector3(50f, 0f, -131f));
            Watchtower(kit, new Vector3(-134f, 0f, 42f));
            Ford(kit, new Vector2(44f, 141f));
            Smithy(kit);
        }

        // ---------------------------------------------------------------- The old watchtower

        // A tumbledown watchtower deep in the west wood: a round stone tower with a broken top,
        // steps winding up the outside, and a little cache of bubbles at the top for anyone who
        // climbs it. From up there you can see the whole town over the treetops.
        static void Watchtower(LevelKit kit, Vector3 feet)
        {
            var stone = PatchworkAssets.Cobble(new Color(0.72f, 0.7f, 0.66f));
            var moss = PatchworkAssets.Felt(new Color(0.42f, 0.58f, 0.34f));
            var g = kit.Group("OldWatchtower", feet);
            const float r = 3.2f, h = 9f;
            for (int i = 0; i < 5; i++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, h * (i + 0.5f) / 5f, 0f), new Vector3(r * 2f - i * 0.1f, h / 10f, r * 2f - i * 0.1f), stone, default(Vector3), g);
            var col = g.gameObject.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, (h - 0.3f) * 0.5f, 0f);
            col.radius = r - 0.1f;
            col.height = h - 0.3f;
            // The broken top: a ring of merlons with gaps, some fallen.
            var rng = new System.Random(9);
            for (int k = 0; k < 12; k++)
            {
                if (k % 4 == 1) continue;
                float a = k * 30f * Mathf.Deg2Rad;
                float mh = 0.6f + (float)rng.NextDouble() * 1.2f;
                kit.Deco(PrimitiveType.Cube, new Vector3(Mathf.Cos(a) * (r - 0.3f), h + mh * 0.5f, Mathf.Sin(a) * (r - 0.3f)), new Vector3(0.9f, mh, 0.7f), stone, new Vector3(0f, -a * Mathf.Rad2Deg, 0f), g);
            }
            for (int k = 0; k < 5; k++)
            {
                float a = (float)rng.NextDouble() * Mathf.PI * 2f, d = r + 1.2f + (float)rng.NextDouble() * 2.5f;
                kit.Deco(PrimitiveType.Cube, new Vector3(Mathf.Cos(a) * d, 0.3f, Mathf.Sin(a) * d), new Vector3(0.9f, 0.6f, 0.7f), stone, new Vector3((float)rng.NextDouble() * 30f, (float)rng.NextDouble() * 360f, 12f), g);
            }
            // Ivy patches up the side.
            for (int k = 0; k < 7; k++)
            {
                float a = (k * 47f + 20f) * Mathf.Deg2Rad;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * (r + 0.02f), 1.2f + k * 1.1f, Mathf.Sin(a) * (r + 0.02f)), new Vector3(1.4f, 1.6f, 0.3f), moss, new Vector3(0f, -a * Mathf.Rad2Deg + 90f, 0f), g);
            }
            // The steps: blocks spiralling up the outside, a full turn and a half to the top.
            const int steps = 26;
            for (int k = 0; k < steps; k++)
            {
                float a = k * (540f / steps) * Mathf.Deg2Rad;
                float y = (k + 1) * (h / steps);
                Vector3 p = feet + new Vector3(Mathf.Cos(a) * (r + 0.7f), y - 0.2f, Mathf.Sin(a) * (r + 0.7f));
                var step = kit.Block(p, new Vector3(1.5f, 0.4f, 1.1f), new Color(0.72f, 0.7f, 0.66f), "TowerStep", stone);
                step.rotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f);
            }
            // The floor at the top, and the cache.
            kit.Block(feet + Vector3.up * (h - 0.1f), new Vector3(r * 1.7f, 0.2f, r * 1.7f), new Color(0.6f, 0.45f, 0.3f), "TowerTop", PatchworkAssets.Planks(new Color(0.6f, 0.45f, 0.3f)));
            for (int k = 0; k < 5; k++)
            {
                float a = k * 72f * Mathf.Deg2Rad;
                ScoreBubbles.Place(kit, feet + new Vector3(Mathf.Cos(a) * 1.2f, h + 1f, Mathf.Sin(a) * 1.2f), 50);
            }
            Take(feet, r + 4f);
            ClearGrass(feet, r + 1f);
            kit.Info.MapPins.Add(new MapPin("Old Watchtower", feet));
        }

        // ---------------------------------------------------------------- The ford

        // Stepping stones across the River Weft north of town, on a straight stretch near
        // `near`: big flat-topped boulders a hop apart, with a little cairn on each bank.
        static void Ford(LevelKit kit, Vector2 near)
        {
            var path = RiverNorth;
            int ni = 1;
            for (int i = 1; i < path.Length - 1; i++)
                if ((path[i] - near).sqrMagnitude < (path[ni] - near).sqrMagnitude) ni = i;
            Vector2 p = path[ni], d = (path[ni + 1] - path[ni - 1]).normalized, n = new Vector2(-d.y, d.x);
            float t;
            PathDistance(p, path, out t);
            float hw = RiverHalfWidth(t);
            var stone = PatchworkAssets.Cobble(new Color(0.74f, 0.72f, 0.68f));
            int count = Mathf.CeilToInt((hw * 2f + 1f) / 1.9f);
            for (int k = 0; k < count; k++)
            {
                float u = -hw - 0.2f + (k + 0.5f) * ((hw * 2f + 0.4f) / count);
                Vector2 q = p + n * u + d * (k % 2 == 0 ? 0.35f : -0.35f);
                float top = 0.12f;
                float bottom = MeadowHeight(q.x, q.y) - 0.3f;
                var b = kit.Block(new Vector3(q.x, (top + bottom) * 0.5f, q.y), new Vector3(1.3f, top - bottom, 1.2f), new Color(0.74f, 0.72f, 0.68f), "FordStone", stone);
                b.rotation = Quaternion.Euler(0f, k * 23f, 0f);
                kit.Deco(PrimitiveType.Sphere, new Vector3(q.x, top, q.y), new Vector3(1.4f, 0.3f, 1.3f), stone, new Vector3(0f, k * 23f, 0f));
            }
            for (int s = -1; s <= 1; s += 2)
            {
                Vector2 c = p + n * s * (hw + 3.4f);
                for (int k = 0; k < 3; k++)
                    kit.Deco(PrimitiveType.Sphere, new Vector3(c.x, 0.25f + k * 0.4f, c.y), Vector3.one * (0.8f - k * 0.2f), stone, new Vector3(0f, k * 40f, 0f));
            }
            kit.Info.MapPins.Add(new MapPin("Weft Ford", new Vector3(p.x, 0f, p.y)));
        }

        // ---------------------------------------------------------------- The smithy

        // The keep's smithy in a corner of the bailey: a lean-to roof on posts over a glowing
        // forge, an anvil on its block, a rack of horseshoes and a water trough.
        static void Smithy(LevelKit kit)
        {
            int first = kit.Root.childCount;
            var feet = new Vector3(Bailey.z - 3.6f, 0.05f, Bailey.w - 3f);
            var g = kit.Group("Smithy", feet);
            var darkWood = PatchworkAssets.Wood(TownKit.DarkWood);
            var iron = PatchworkAssets.Plain(TownKit.Iron, 0.5f);
            var brick = PatchworkAssets.Brick(new Color(0.62f, 0.4f, 0.34f));
            for (int sx = -1; sx <= 1; sx += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(sx * 2.2f, 1.4f, -1.6f), new Vector3(0.2f, 2.8f, 0.2f), darkWood, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 3f, 0f), new Vector3(5f, 0.12f, 3.8f), PatchworkAssets.Shingle(new Color(0.45f, 0.36f, 0.3f)), new Vector3(-12f, 0f, 0f), g);
            // The forge: a brick hearth with coals that glow, and a chimney.
            kit.Deco(PrimitiveType.Cube, new Vector3(-1f, 0.55f, 1.1f), new Vector3(1.8f, 1.1f, 1.2f), brick, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(-1f, 1.12f, 1.1f), new Vector3(1.4f, 0.06f, 0.8f), PatchworkAssets.Glow(new Color(1f, 0.45f, 0.2f), 1.2f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(-1f, 2.6f, 1.4f), new Vector3(0.8f, 3f, 0.6f), brick, default(Vector3), g);
            var glow = new GameObject("ForgeGlow").AddComponent<Light>();
            glow.transform.SetParent(g, false);
            glow.transform.localPosition = new Vector3(-1f, 1.5f, 0.6f);
            glow.type = LightType.Point;
            glow.color = new Color(1f, 0.55f, 0.25f);
            glow.range = 5f;
            glow.intensity = 1.6f;
            // The anvil on a stump.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(1.2f, 0.35f, 0.2f), new Vector3(0.7f, 0.35f, 0.7f), PatchworkAssets.Wood(new Color(0.55f, 0.4f, 0.28f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(1.2f, 0.85f, 0.2f), new Vector3(0.9f, 0.3f, 0.4f), iron, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(1.75f, 0.88f, 0.2f), new Vector3(0.4f, 0.2f, 0.25f), iron, default(Vector3), g);
            TownKit.Solid(kit, feet + new Vector3(1.2f, 0.5f, 0.2f), new Vector3(0.8f, 1f, 0.8f), 0f);
            // A rack of horseshoes on the back post, and the trough.
            for (int k = 0; k < 4; k++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(2.1f, 1.2f + k * 0.35f, -1.45f), new Vector3(0.3f, 0.02f, 0.3f), iron, new Vector3(90f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.2f, 0.35f, -1.1f), new Vector3(1.6f, 0.7f, 0.7f), PatchworkAssets.Planks(new Color(0.55f, 0.4f, 0.28f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.2f, 0.62f, -1.1f), new Vector3(1.4f, 0.04f, 0.5f), PatchworkAssets.CraftWater(0.9f, Vector2.one), default(Vector3), g);
            TownKit.Solid(kit, feet + new Vector3(-1f, 0.55f, 1.1f), new Vector3(1.8f, 1.1f, 1.2f), 0f);
            // It's inside the keep's bailey, so it counts as part of the town (the tidy-up leaves it be).
            for (int i = first; i < kit.Root.childCount; i++) cityThings.Add(kit.Root.GetChild(i));
        }

        static readonly Color KnightRed = new Color(0.82f, 0.3f, 0.3f), KnightBlue = new Color(0.3f, 0.45f, 0.72f);

        // ---------------------------------------------------------------- The Lists

        static void Lists(LevelKit kit, Vector3 c)
        {
            var root = kit.Group("TheLists", c);
            var white = new Color(0.97f, 0.94f, 0.88f);
            var wood = PatchworkAssets.Wood(new Color(0.62f, 0.44f, 0.3f));
            var darkWood = PatchworkAssets.Wood(TownKit.DarkWood);
            const float halfL = 26f, halfW = 7f;
            // The ground: a raked sand lane down the middle, grass either side.
            var sand = kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.03f, 0f), new Vector3(halfL * 2f - 6f, 0.04f, 7f), PatchworkAssets.Felt(new Color(0.9f, 0.82f, 0.62f)), default(Vector3), root);
            sand.name = "TiltLane";
            ClearGrass(c, halfL);
            // The tilt barrier: a striped fence down the middle of the lane.
            for (int i = 0; i < 18; i++)
            {
                float x = -halfL + 6f + i * 2.2f;
                kit.Block(c + new Vector3(x, 0.6f, 0f), new Vector3(2.15f, 1.2f, 0.25f), white, "TiltBarrier", PatchworkAssets.Planks(i % 2 == 0 ? KnightRed : white));
                kit.Deco(PrimitiveType.Cube, new Vector3(x, 1.25f, 0f), new Vector3(2.2f, 0.12f, 0.35f), darkWood, default(Vector3), root);
            }
            // Low rails round the outside, open at both ends and in the middle of the south side.
            for (int s = -1; s <= 1; s += 2)
                for (float x = -halfL + 1f; x < halfL - 1f; x += 2.4f)
                {
                    if (s < 0 && Mathf.Abs(x) < 3f) continue;
                    kit.Deco(PrimitiveType.Cube, new Vector3(x, 0.45f, s * halfW), new Vector3(0.14f, 0.9f, 0.14f), darkWood, default(Vector3), root);
                    kit.Deco(PrimitiveType.Cube, new Vector3(x + 1.2f, 0.75f, s * halfW), new Vector3(2.4f, 0.1f, 0.08f), wood, default(Vector3), root);
                }
            // The stand on the north side: three tiers of benches you can climb, under a striped
            // awning on posts, with the herald's box in the middle.
            var stripes = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, white, KnightRed);
            for (int t = 0; t < 3; t++)
            {
                float th = 0.5f * (t + 1);
                kit.Block(c + new Vector3(-2f, th * 0.5f, halfW + 1.6f + t * 1.1f), new Vector3(20f, th, 1.1f), new Color(0.62f, 0.44f, 0.3f), "StandTier", wood);
            }
            for (int k = 0; k < 5; k++)
                for (int s = 0; s < 2; s++)
                    kit.Deco(PrimitiveType.Cube, new Vector3(-12f + k * 5f, 2.2f, halfW + 1f + s * 3.4f), new Vector3(0.2f, 4.4f, 0.2f), darkWood, default(Vector3), root);
            kit.Deco(PrimitiveType.Cube, new Vector3(-2f, 4.5f, halfW + 2.7f), new Vector3(21f, 0.1f, 4.2f), stripes, new Vector3(-8f, 0f, 0f), root);
            for (int k = 0; k < 14; k++)
                kit.Deco(PrimitiveType.Cube, new Vector3(-12.2f + k * 1.5f, 4.1f, halfW + 0.55f), new Vector3(0.75f, 0.6f, 0.04f), PatchworkAssets.Felt(k % 2 == 0 ? KnightRed : KnightBlue), new Vector3(0f, 0f, 45f), root);
            // Shields of the knights along the front of the stand.
            for (int k = 0; k < 4; k++)
            {
                var sh = new Vector3(-10f + k * 5.3f, 1.2f, halfW + 1.02f);
                kit.Deco(PrimitiveType.Cube, sh, new Vector3(0.9f, 1f, 0.06f), PatchworkAssets.Felt(k % 2 == 0 ? KnightRed : KnightBlue), default(Vector3), root);
                kit.Deco(PrimitiveType.Sphere, sh + new Vector3(0f, -0.45f, 0f), new Vector3(0.9f, 0.5f, 0.06f), PatchworkAssets.Felt(k % 2 == 0 ? KnightRed : KnightBlue), default(Vector3), root);
                kit.Deco(PrimitiveType.Cube, sh + new Vector3(0f, -0.05f, -0.04f), new Vector3(0.2f, 1f, 0.03f), gold, default(Vector3), root);
            }
            // The knights' pavilions at both ends: striped round tents with pointed tops and
            // pennants, one camp red and one blue.
            for (int e = -1; e <= 1; e += 2)
                for (int s = -1; s <= 1; s += 2)
                    Pavilion(kit, root, new Vector3(e * (halfL + 3.6f), 0f, s * 4.2f), e < 0 ? KnightRed : KnightBlue);
            // Banner poles along the south rail.
            for (int k = 0; k < 6; k++)
            {
                float x = -halfL + 4f + k * ((halfL * 2f - 8f) / 5f);
                if (Mathf.Abs(x) < 3f) continue;
                kit.Deco(PrimitiveType.Cylinder, new Vector3(x, 2.4f, -halfW - 0.8f), new Vector3(0.1f, 2.4f, 0.1f), darkWood, default(Vector3), root);
                kit.Deco(PrimitiveType.Sphere, new Vector3(x, 4.85f, -halfW - 0.8f), Vector3.one * 0.25f, gold, default(Vector3), root);
                var flag = kit.Group("ListsFlag", c + new Vector3(x, 4.3f, -halfW - 0.8f));
                flag.SetParent(root, true);
                kit.Deco(PrimitiveType.Cube, new Vector3(0.6f, 0f, 0f), new Vector3(1.2f, 0.7f, 0.04f), PatchworkAssets.Felt(k % 2 == 0 ? KnightRed : KnightBlue), default(Vector3), flag);
                flag.gameObject.AddComponent<FlagWave>();
            }
            // The quintain at the east end of the lane: a post with a spinning arm (a shield on
            // one end, a sandbag on the other). Run the lane to collect the bubbles.
            var post = c + new Vector3(halfL - 6.5f, 0f, -2.2f);
            kit.Block(post + Vector3.up * 1.2f, new Vector3(0.35f, 2.4f, 0.35f), new Color(0.36f, 0.24f, 0.17f), "Quintain", darkWood);
            var arm = kit.Group("QuintainArm", post + Vector3.up * 2.3f);
            arm.SetParent(root, true);
            kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(3.2f, 0.18f, 0.18f), wood, default(Vector3), arm);
            kit.Deco(PrimitiveType.Cube, new Vector3(1.6f, 0f, 0f), new Vector3(0.1f, 1f, 0.8f), PatchworkAssets.Felt(KnightBlue), default(Vector3), arm);
            kit.Deco(PrimitiveType.Sphere, new Vector3(-1.6f, -0.5f, 0f), new Vector3(0.6f, 0.8f, 0.6f), PatchworkAssets.Felt(new Color(0.75f, 0.62f, 0.45f)), default(Vector3), arm);
            var spin = arm.gameObject.AddComponent<Spinner>();
            spin.Speed = 40f;
            for (int k = 0; k < 9; k++)
                ScoreBubbles.Place(kit, c + new Vector3(-halfL + 8f + k * 4.2f, 1.2f, -2.2f), 10);
            // The herald and a knight between bouts.
            var herald = Toy(kit, c + new Vector3(-2f, 1.52f, halfW + 3.8f), 180f, ToyNpc.Kind.Bird, "Herald Fanfaronne", new Color(0.95f, 0.78f, 0.3f), KnightRed, ToyNpc.Instrument.Trumpet, 1.3f, new Color(0.85f, 0.65f, 0.25f));
            herald.Lines = () => new[] { "Hear ye, hear ye! Welcome to the Lists, where the bravest toys in the box ride at the tilt!",
                                         "Sir Thimble of the Red rides against Dame Bobbin of the Blue, at noon. Or whenever the Fray stops blowing their tents about.",
                                         "No horse? No matter! Run the lane to the quintain and catch every bubble. That's how the squires practise." };
            var knight = Toy(kit, c + new Vector3(-halfL - 0.6f, 0.05f, 0.6f), 90f, ToyNpc.Kind.TinSoldier, "Sir Thimble", KnightRed, new Color(0.85f, 0.86f, 0.9f), ToyNpc.Instrument.None, 0.72f, new Color(0.7f, 0.25f, 0.25f));
            knight.Lines = () => new[] { "Sir Thimble of the Red, champion of the Lists three years running! Well, two. Dame Bobbin had a very good year.",
                                         "A knight's lance is just a very long knitting needle, if you think about it. Which I try not to." };
            kit.Info.MapPins.Add(new MapPin("The Lists", c, MapPin.Kind.Area));
            Take(c, halfL + 6f);
        }

        // A round striped tent with a pointed top, a scalloped valance and a pennant, its door
        // flaps tied back.
        static void Pavilion(LevelKit kit, Transform parent, Vector3 local, Color col)
        {
            var g = kit.Group("Pavilion", parent.position + local);
            g.SetParent(parent, true);
            var white = new Color(0.97f, 0.94f, 0.88f);
            var cloth = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, white, col);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.3f, 0f), new Vector3(4f, 1.3f, 4f), cloth, default(Vector3), g);
            TowerCone(kit, g, new Vector3(0f, 2.6f, 0f), 2.4f, 2.6f, PatchworkAssets.Felt(col));
            for (int k = 0; k < 12; k++)
            {
                float a = k * 30f;
                var q = Quaternion.Euler(0f, a, 0f);
                kit.Deco(PrimitiveType.Sphere, q * new Vector3(0f, 2.55f, 2.04f), new Vector3(0.9f, 0.5f, 0.06f), PatchworkAssets.Felt(k % 2 == 0 ? white : col), new Vector3(0f, a, 0f), g);
            }
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.9f, 2.02f), new Vector3(1.1f, 1.8f, 0.04f), PatchworkAssets.Felt(new Color(0.3f, 0.22f, 0.2f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 5.6f, 0f), new Vector3(0.06f, 0.5f, 0.06f), PatchworkAssets.Wood(TownKit.DarkWood), default(Vector3), g);
            var flag = kit.Group("PavilionFlag", g.position + new Vector3(0f, 5.9f, 0f));
            flag.SetParent(g, true);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.45f, 0f, 0f), new Vector3(0.9f, 0.45f, 0.03f), PatchworkAssets.Felt(col), default(Vector3), flag);
            flag.gameObject.AddComponent<FlagWave>();
            var body = g.gameObject.AddComponent<CapsuleCollider>();
            body.center = new Vector3(0f, 1.6f, 0f);
            body.radius = 2f;
            body.height = 3.2f;
            // Face the lane.
            g.rotation = Quaternion.Euler(0f, local.x < 0f ? 90f : -90f, 0f);
        }

        // ---------------------------------------------------------------- The windmill

        // A stone tower mill with a boat-shaped cap and four latticed sails that turn in the
        // wind, a little door and a fence round its yard.
        static void Windmill(LevelKit kit, Vector3 feet)
        {
            var stone = PatchworkAssets.Cobble(new Color(0.88f, 0.84f, 0.76f));
            var wood = PatchworkAssets.Wood(new Color(0.62f, 0.44f, 0.3f));
            var darkWood = PatchworkAssets.Wood(TownKit.DarkWood);
            var sailCloth = PatchworkAssets.Felt(new Color(0.97f, 0.94f, 0.88f));
            var g = kit.Group("Windmill", feet);
            const float h = 11f;
            // The tower, a little narrower at the top.
            for (int i = 0; i < 4; i++)
            {
                float r = 3.2f - i * 0.25f;
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, h * (i + 0.5f) / 4f, 0f), new Vector3(r * 2f, h / 8f, r * 2f), stone, default(Vector3), g);
            }
            var body = g.gameObject.AddComponent<CapsuleCollider>();
            body.center = new Vector3(0f, h * 0.5f, 0f);
            body.radius = 3f;
            body.height = h;
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, h, 0f), new Vector3(5.6f, 0.15f, 5.6f), darkWood, default(Vector3), g);
            // The cap.
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, h + 0.6f, 0f), new Vector3(4.6f, 2.6f, 5.4f), PatchworkAssets.Shingle(new Color(0.42f, 0.32f, 0.28f)), default(Vector3), g);
            // Door, windows, a little gallery.
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.1f, 3.05f), new Vector3(1.2f, 2.2f, 0.12f), PatchworkAssets.Planks(new Color(0.5f, 0.32f, 0.24f)), default(Vector3), g);
            for (int k = 0; k < 2; k++)
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 4.6f + k * 3f, 2.95f - k * 0.25f), new Vector3(0.6f, 0.9f, 0.08f), PatchworkAssets.Glow(new Color(1f, 0.82f, 0.5f), 0.4f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 6f, 0f), new Vector3(7.2f, 0.1f, 7.2f), wood, default(Vector3), g);
            // The sails, on a hub at the front of the cap; they turn.
            var hub = kit.Group("WindmillSails", feet + new Vector3(0f, h + 0.7f, 2.9f));
            hub.SetParent(g, true);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.7f, 0.4f, 0.7f), darkWood, new Vector3(90f, 0f, 0f), hub);
            for (int k = 0; k < 4; k++)
            {
                var arm = kit.Group("Sail", hub.position);
                arm.SetParent(hub, true);
                arm.localRotation = Quaternion.Euler(0f, 0f, k * 90f + 20f);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 4.2f, 0.25f), new Vector3(0.22f, 8.4f, 0.14f), darkWood, default(Vector3), arm);
                kit.Deco(PrimitiveType.Cube, new Vector3(0.75f, 5f, 0.3f), new Vector3(1.4f, 6.2f, 0.05f), sailCloth, default(Vector3), arm);
                for (int b = 0; b < 6; b++)
                    kit.Deco(PrimitiveType.Cube, new Vector3(0.75f, 2.3f + b * 1.1f, 0.34f), new Vector3(1.5f, 0.06f, 0.06f), darkWood, default(Vector3), arm);
            }
            var turn = hub.gameObject.AddComponent<Spinner>();
            turn.Axis = Vector3.forward;
            turn.Speed = 22f;
            // A fence round the yard, sacks of flour by the door.
            for (int k = 0; k < 14; k++)
            {
                float a = k * Mathf.PI * 2f / 14f;
                if (k == 3) continue;   // the gate
                kit.Deco(PrimitiveType.Cube, new Vector3(Mathf.Cos(a) * 6.5f, 0.45f, Mathf.Sin(a) * 6.5f), new Vector3(0.14f, 0.9f, 0.14f), darkWood, default(Vector3), g);
                kit.Deco(PrimitiveType.Cube, new Vector3(Mathf.Cos(a + 0.22f) * 6.4f, 0.7f, Mathf.Sin(a + 0.22f) * 6.4f), new Vector3(2.9f, 0.08f, 0.08f), wood, new Vector3(0f, -(a + 0.22f) * Mathf.Rad2Deg + 90f, 0f), g);
            }
            for (int k = 0; k < 3; k++)
                kit.Deco(PrimitiveType.Sphere, new Vector3(-1.6f + k * 0.7f, 0.4f, 3.6f), new Vector3(0.6f, 0.8f, 0.55f), PatchworkAssets.Felt(new Color(0.95f, 0.92f, 0.84f)), default(Vector3), g);
            g.rotation = Quaternion.Euler(0f, 210f, 0f);   // sails towards the road and the Pod
            ClearGrass(feet, 3.5f);
            Take(feet, 7f);
            kit.Info.MapPins.Add(new MapPin("Windmill", feet));
        }

        // ---------------------------------------------------------------- The Weft Mill

        // A thatched watermill on the river's bank, its wheel turning in the stream.
        static void WeftMill(LevelKit kit, Vector3 near)
        {
            // Find the river point closest to `near`, and the bank on that side.
            var path = RiverSouth;
            int ni = 1;
            for (int i = 1; i < path.Length - 1; i++)
                if ((path[i] - new Vector2(near.x, near.z)).sqrMagnitude < (path[ni] - new Vector2(near.x, near.z)).sqrMagnitude) ni = i;
            Vector2 p = path[ni], d = (path[ni + 1] - path[ni - 1]).normalized, n = new Vector2(-d.y, d.x);
            if (Vector2.Dot(new Vector2(near.x, near.z) - p, n) < 0f) n = -n;
            float t;
            PathDistance(p, path, out t);
            float hw = RiverHalfWidth(t);
            // The house sits square on the bank, back from the edge; the wheel at the water.
            Vector2 hp = p + n * (hw + 6.4f);
            var house = new Vector3(hp.x, 0f, hp.y);
            if (Physics.CheckBox(house + Vector3.up * 2f, new Vector3(4f, 1.6f, 4f), Quaternion.identity, ~(1 << 2), QueryTriggerInteraction.Ignore) || Taken(house, 5f)) return;
            Take(house, 6f);
            Vector2 face = -n;
            float yaw = Mathf.Round(Mathf.Atan2(face.x, face.y) * Mathf.Rad2Deg / 90f) * 90f;
            TownKit.MakeHouse(kit, house, new Vector3(7f, 5f, 6f), yaw, Walls[0], new Color(0.86f, 0.72f, 0.42f), 777, 1, 1.5f);
            Vector2 wp = p + n * (hw - 0.2f);
            var wheel = kit.Group("WeftMillWheel", new Vector3(wp.x, CanalSurface + 1.6f, wp.y));
            wheel.rotation = Quaternion.LookRotation(new Vector3(n.x, 0f, n.y), Vector3.up);
            var wood = PatchworkAssets.Wood(new Color(0.55f, 0.4f, 0.28f));
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, s * 0.5f), new Vector3(4.6f, 0.06f, 4.6f), wood, new Vector3(90f, 0f, 0f), wheel);
            for (int k = 0; k < 12; k++)
            {
                float a = k * 30f * Mathf.Deg2Rad;
                kit.Deco(PrimitiveType.Cube, new Vector3(Mathf.Cos(a) * 2.2f, Mathf.Sin(a) * 2.2f, 0f), new Vector3(0.5f, 0.12f, 1.1f), wood, new Vector3(0f, 0f, a * Mathf.Rad2Deg), wheel);
            }
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.4f, 0.8f, 0.4f), PatchworkAssets.Wood(TownKit.DarkWood), new Vector3(90f, 0f, 0f), wheel);
            var spin = wheel.gameObject.AddComponent<Spinner>();
            spin.Axis = Vector3.forward;
            spin.Speed = -30f;
            // The axle beam back to the house, and a little race of foam where the wheel churns.
            Vector3 w3 = new Vector3(wp.x, CanalSurface + 1.6f, wp.y), h3 = new Vector3(hp.x, CanalSurface + 1.6f, hp.y);
            kit.Deco(PrimitiveType.Cube, (w3 + h3) * 0.5f, new Vector3(0.3f, 0.3f, Vector3.Distance(w3, h3)), PatchworkAssets.Wood(TownKit.DarkWood), Quaternion.LookRotation(h3 - w3).eulerAngles);
            var lace = PatchworkAssets.Felt(new Color(0.96f, 0.98f, 1f));
            var foam = new MeshBatch();
            for (int k = 0; k < 10; k++)
                foam.Ball(new Vector3(wp.x, CanalSurface + 0.03f, wp.y) + new Vector3(d.x, 0f, d.y) * ((k - 4.5f) * 0.6f) - new Vector3(n.x, 0f, n.y) * (0.4f + (k % 3) * 0.3f), new Vector3(0.45f, 0.05f, 0.35f), 2, 6);
            foam.Build(wheel.parent != null ? wheel.parent : kit.Root, "MillRace", lace);
            kit.Info.MapPins.Add(new MapPin("Weft Mill", house));
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Toybox Town's dressing: everything that isn't needed to play but makes it feel like a real
    // place. The nursery the toybox sits in, giant toys round the edges, market stalls and the
    // wedding cake in the square, hedges and railings, rubber ducks on the canal, a toy train,
    // a picnic in the park, a balloon and a cot mobile in the sky, butterflies, and finally the
    // yarn grass and felt patches over all the open ground.
    public static partial class Level1Builder
    {
        static MeshBatch ironBatch, whiteBatch, woodBatch, reedBatch, cattailBatch, railBatch, sleeperBatch;

        static void Details(LevelKit kit)
        {
            ironBatch = new MeshBatch();
            whiteBatch = new MeshBatch();
            woodBatch = new MeshBatch();
            reedBatch = new MeshBatch();
            cattailBatch = new MeshBatch();
            railBatch = new MeshBatch();
            sleeperBatch = new MeshBatch();

            Nursery(kit);
            GiantToys(kit);
            SquareDressing(kit);
            StreetDressing(kit);
            CanalDressing(kit);
            RavineDressing(kit);
            HillDressing(kit);
            NorthPark(kit);
            TrainLoop(kit);
            MillDressing(kit);
            SkyDressing(kit);
            Butterflies(kit);
            ToyFun(kit);

            var g = kit.Group("Batched", Vector3.zero);
            ironBatch.Build(g, "Iron", PatchworkAssets.Plain(TownKit.Iron, 0.5f), true);
            whiteBatch.Build(g, "WhitePaint", PatchworkAssets.Wood(new Color(0.97f, 0.95f, 0.9f)), true);
            woodBatch.Build(g, "Wood", PatchworkAssets.Wood(new Color(0.62f, 0.44f, 0.3f)), true);
            reedBatch.Build(g, "Reeds", PatchworkAssets.Yarn(new Color(0.45f, 0.6f, 0.35f)));
            cattailBatch.Build(g, "Cattails", PatchworkAssets.Felt(new Color(0.45f, 0.3f, 0.2f)));
            railBatch.Build(g, "Rails", PatchworkAssets.Plain(new Color(0.55f, 0.55f, 0.6f), 0.7f), true);
            sleeperBatch.Build(g, "Sleepers", PatchworkAssets.Planks(new Color(0.5f, 0.34f, 0.24f)));
        }

        // Yarn grass, clover, pebbles and sewn-on felt patches over all the open grass. Last of all,
        // so it only grows where nothing else is.
        static void GroundCover(LevelKit kit)
        {
            Color[] greens = { new Color(0.42f, 0.66f, 0.36f), new Color(0.5f, 0.74f, 0.4f), new Color(0.36f, 0.58f, 0.34f) };
            Color[] patchCols = { new Color(0.6f, 0.78f, 0.46f), new Color(0.46f, 0.66f, 0.42f), new Color(0.7f, 0.8f, 0.5f), new Color(0.55f, 0.72f, 0.6f), new Color(0.78f, 0.82f, 0.55f) };
            var area = new Vector2(Half - 0.5f, Half - 0.5f);
            YarnGrass.Patches(kit, -area, area, 0f, 70, patchCols, 5);
            YarnGrass.Patches(kit, new Vector2(-74f, 4f), new Vector2(-60f, 38f), 4f, 6, patchCols, 9);
            YarnGrass.Scatter(kit, -area, area, 0f, 1.6f, greens, FlowerColors, 3);
            YarnGrass.Scatter(kit, new Vector2(-74f, 4f), new Vector2(-60f, 38f), 4f, 1.8f, greens, FlowerColors, 4);
        }

        // ---------------------------------------------------------------- The intro's props

        // The things only the first-visit cutscene uses: the crack in the toybox lid, the Fray
        // itself (a big crackling knot of grey thread), and the golden lute string it flings away.
        static void IntroProps(LevelKit kit)
        {
            var intro = director.gameObject.AddComponent<ToyboxIntro>();
            intro.Director = director;

            var crack = kit.Group("LidCrack", new Vector3(0f, 40.8f, Half - 0.15f));
            var dark = PatchworkAssets.Felt(SnarlKnot.FrayDark);
            for (int i = 0; i < 7; i++)
                kit.Deco(PrimitiveType.Cube, new Vector3((i - 3) * 1.5f, Mathf.Sin(i * 2.1f) * 0.5f, 0f), new Vector3(1.8f, 0.5f + (i % 3) * 0.3f, 0.1f), dark, new Vector3(0f, 0f, (i % 2 == 0 ? 20f : -25f)), crack);
            AddGlow(crack, SnarlKnot.FrayGlow, 20f, 3f);
            crack.gameObject.SetActive(false);
            intro.Crack = crack;

            var blob = kit.Group("TheFray", Vector3.zero);
            KnotShape(kit, blob, 3.2f, 30);
            var grey = PatchworkAssets.Yarn(SnarlKnot.FrayGrey);
            for (int i = 0; i < 9; i++)
            {
                var strand = kit.Deco(PrimitiveType.Capsule, Vector3.zero, new Vector3(0.18f, 1.6f, 0.18f), grey, default(Vector3), blob);
                strand.localRotation = Quaternion.Euler(i * 40f, i * 73f, 0f);
                strand.localPosition = strand.localRotation * new Vector3(0f, 2f, 0f);
            }
            var spin = blob.gameObject.AddComponent<Spinner>();
            spin.Axis = new Vector3(0.3f, 1f, 0.2f);
            spin.Speed = 120f;
            AddGlow(blob, SnarlKnot.FrayGlow, 16f, 3f);
            blob.gameObject.SetActive(false);
            intro.Blob = blob;

            var streak = kit.Group("LuteString", Vector3.zero);
            var gold = PatchworkAssets.Glow(new Color(1f, 0.82f, 0.4f), 1.4f);
            for (int i = 0; i < 6; i++)
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, -i * 0.35f), Vector3.one * (0.4f - i * 0.05f), gold, default(Vector3), streak);
            AddGlow(streak, new Color(1f, 0.8f, 0.5f), 8f, 2f);
            streak.gameObject.SetActive(false);
            intro.Streak = streak;
        }

        static void AddGlow(Transform parent, Color c, float range, float intensity)
        {
            var go = new GameObject("Glow");
            go.transform.SetParent(parent, false);
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = c;
            l.range = range;
            l.intensity = intensity;
            l.shadows = LightShadows.None;
        }

        // ---------------------------------------------------------------- Helpers

        static Material Tiled(Material m, float sx, float sy)
        {
            var u = PatchworkAssets.Unique(m);
            u.SetTextureScale("_BaseMap", new Vector2(sx, sy));
            return u;
        }

        // An invisible box to bump into (for decorations made of several pieces).
        static void Solid(LevelKit kit, Vector3 center, Vector3 size, float yaw = 0f)
        {
            var go = new GameObject("Solid");
            go.transform.SetParent(kit.Root, false);
            go.transform.position = center;
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            go.AddComponent<BoxCollider>().size = size;
        }

        static void ClearGrass(Vector3 p, float radius)
        {
            YarnGrass.ClearZones.Add(new Vector3(p.x, p.z, radius));
        }

        // A lollipop tree: a striped felt trunk and a big round knitted pompom.
        static void PomTree(LevelKit kit, Vector3 feet, float height, Color leaves)
        {
            var g = kit.Group("PomTree", feet);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, height * 0.3f, 0f), new Vector3(0.32f, height * 0.3f, 0.32f), PatchworkAssets.Wood(new Color(0.55f, 0.38f, 0.26f)), default(Vector3), g);
            for (int i = 0; i < 3; i++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, height * (0.12f + i * 0.17f), 0f), new Vector3(0.34f, 0.05f, 0.34f), PatchworkAssets.Felt(LevelKit.Palette.Cream), default(Vector3), g);
            float r = height * 0.32f;
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, height * 0.72f, 0f), Vector3.one * r * 2f, PatchworkAssets.Knit(leaves), new Vector3(0f, feet.x * 7f, 0f), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(r * 0.5f, height * 0.72f + r * 0.6f, r * 0.3f), Vector3.one * r * 0.5f, PatchworkAssets.Felt(Color.Lerp(leaves, Color.white, 0.25f)), default(Vector3), g);
            for (int i = 0; i < 4; i++)
            {
                float a = i * 1.7f + feet.z;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * r * 0.95f, height * 0.72f + Mathf.Sin(i * 2.1f) * r * 0.5f, Mathf.Sin(a) * r * 0.95f), Vector3.one * 0.22f, PatchworkAssets.Felt(FlowerColors[i % FlowerColors.Length]), default(Vector3), g);
            }
            var col = g.gameObject.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, height * 0.4f, 0f);
            col.radius = 0.3f;
            col.height = height * 0.8f;
            ClearGrass(feet, 0.6f);
        }

        // A clipped knitted hedge along a line, with the odd pompom flower in it.
        static void Hedge(LevelKit kit, Vector3 a, Vector3 b, float height = 1.2f, float width = 0.9f)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            var col = new Color(0.32f, 0.52f, 0.32f);
            var h = kit.Block((a + b) * 0.5f + Vector3.up * height * 0.5f, new Vector3(width, height, len), col, "Hedge", PatchworkAssets.Knit(col));
            h.rotation = Quaternion.Euler(0f, yaw, 0f);
            h.localScale = new Vector3(width, height, len);
            int blooms = Mathf.FloorToInt(len / 2.5f);
            var rot = Quaternion.Euler(0f, yaw, 0f);
            for (int i = 0; i < blooms; i++)
            {
                float k = (i + 0.5f) / blooms;
                float side = (i % 2 == 0 ? 1f : -1f) * (width * 0.5f + 0.02f);
                kit.Deco(PrimitiveType.Sphere, Vector3.Lerp(a, b, k) + rot * new Vector3(side, height * (0.45f + (i % 3) * 0.15f), 0f), Vector3.one * 0.2f, PatchworkAssets.Felt(FlowerColors[i % FlowerColors.Length]));
            }
        }

        // A low iron railing (batched), solid so you can lean on it.
        static void Railing(LevelKit kit, Vector3 a, Vector3 b, float height = 0.95f)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            var rot = Quaternion.LookRotation(d / len, Vector3.up);
            int posts = Mathf.Max(1, Mathf.RoundToInt(len / 0.3f));
            for (int i = 0; i <= posts; i++)
            {
                Vector3 p = Vector3.Lerp(a, b, i / (float)posts);
                bool big = i % 8 == 0;
                ironBatch.Box(p + Vector3.up * height * 0.5f, new Vector3(big ? 0.12f : 0.04f, height, big ? 0.12f : 0.04f), rot);
                if (big) ironBatch.Ball(p + Vector3.up * (height + 0.06f), Vector3.one * 0.14f, 3, 6);
                else ironBatch.Box(p + Vector3.up * (height + 0.03f), new Vector3(0.07f, 0.07f, 0.07f), rot * Quaternion.Euler(0f, 0f, 45f));
            }
            ironBatch.Box((a + b) * 0.5f + Vector3.up * height, new Vector3(0.06f, 0.05f, len), rot);
            ironBatch.Box((a + b) * 0.5f + Vector3.up * 0.15f, new Vector3(0.05f, 0.04f, len), rot);
            Solid(kit, (a + b) * 0.5f + Vector3.up * height * 0.5f, new Vector3(0.12f, height, len), Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg);
        }

        // A white picket fence (batched).
        static void Pickets(LevelKit kit, Vector3 a, Vector3 b, float height = 0.9f)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            var rot = Quaternion.LookRotation(d / len, Vector3.up);
            int n = Mathf.Max(1, Mathf.RoundToInt(len / 0.28f));
            for (int i = 0; i <= n; i++)
            {
                Vector3 p = Vector3.Lerp(a, b, i / (float)n);
                whiteBatch.Box(p + Vector3.up * (height * 0.5f), new Vector3(0.1f, height, 0.04f), rot * Quaternion.Euler(0f, 90f, 0f));
                whiteBatch.Box(p + Vector3.up * height, new Vector3(0.07f, 0.07f, 0.03f), rot * Quaternion.Euler(0f, 90f, 45f));
            }
            whiteBatch.Box((a + b) * 0.5f + Vector3.up * height * 0.7f, new Vector3(0.05f, 0.08f, len), rot);
            whiteBatch.Box((a + b) * 0.5f + Vector3.up * height * 0.3f, new Vector3(0.05f, 0.08f, len), rot);
            Solid(kit, (a + b) * 0.5f + Vector3.up * height * 0.5f, new Vector3(0.1f, height, len), Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg);
        }

        static void FlowerPot(LevelKit kit, Vector3 feet, Color bloom)
        {
            var g = kit.Group("FlowerPot", feet);
            Solid(kit, feet + Vector3.up * 0.5f, new Vector3(0.7f, 1f, 0.7f));
            var clay = PatchworkAssets.Felt(new Color(0.82f, 0.48f, 0.34f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.3f, 0f), new Vector3(0.6f, 0.3f, 0.6f), clay, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.6f, 0f), new Vector3(0.7f, 0.05f, 0.7f), clay, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.85f, 0f), new Vector3(0.75f, 0.55f, 0.75f), PatchworkAssets.Knit(new Color(0.38f, 0.6f, 0.36f)), default(Vector3), g);
            for (int i = 0; i < 5; i++)
            {
                float a = i * 1.26f + feet.x;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.28f, 1f + (i % 2) * 0.08f, Mathf.Sin(a) * 0.28f), Vector3.one * 0.17f, PatchworkAssets.Felt(bloom), default(Vector3), g);
            }
        }

        static void Barrel(LevelKit kit, Vector3 feet, float yaw)
        {
            var g = kit.Group("Barrel", feet);
            g.rotation = Quaternion.Euler(0f, yaw, 0f);
            var staves = PatchworkAssets.Planks(new Color(0.6f, 0.4f, 0.26f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.55f, 0f), new Vector3(0.9f, 0.55f, 0.9f), staves, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.55f, 0f), new Vector3(1f, 0.9f, 1f), staves, default(Vector3), g);
            for (int i = 0; i < 2; i++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.25f + i * 0.6f, 0f), new Vector3(0.95f, 0.04f, 0.95f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), default(Vector3), g);
            var c = g.gameObject.AddComponent<CapsuleCollider>();
            c.center = new Vector3(0f, 0.55f, 0f);
            c.radius = 0.48f;
            c.height = 1.1f;
        }

        // ---------------------------------------------------------------- The nursery outside the box

        static void Nursery(LevelKit kit)
        {
            // A big soft rug round the toybox, and the box's painted wooden outside.
            const float R = 420f;   // the nursery's walls
            var rug = Tiled(PatchworkAssets.Knit(new Color(0.62f, 0.4f, 0.45f)), 50f, 20f);
            float o = Half + 2f;
            float span = R - o;
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -2.3f, o + span * 0.5f), new Vector3(R * 2f, 0.2f, span), rug);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -2.3f, -o - span * 0.5f), new Vector3(R * 2f, 0.2f, span), rug);
            kit.Deco(PrimitiveType.Cube, new Vector3(o + span * 0.5f, -2.3f, 0f), new Vector3(span, 0.2f, o * 2f), rug);
            kit.Deco(PrimitiveType.Cube, new Vector3(-o - span * 0.5f, -2.3f, 0f), new Vector3(span, 0.2f, o * 2f), rug);
            // The rug's fringed, stitched border, a few metres in from the walls.
            var border = PatchworkAssets.Felt(new Color(0.95f, 0.85f, 0.6f));
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, -2.19f, s * (R - 30f)), new Vector3(R * 2f - 60f, 0.02f, 6f), border);
                kit.Deco(PrimitiveType.Cube, new Vector3(s * (R - 30f), -2.185f, 0f), new Vector3(6f, 0.02f, R * 2f - 60f), border);
            }
            // The other walls, wainscot, skirting and the ceiling with its round lamp.
            var paper2 = Tiled(PatchworkAssets.Damask(new Color(0.75f, 0.82f, 0.9f)), 24f, 12f);
            var wains = Tiled(PatchworkAssets.Planks(new Color(0.96f, 0.93f, 0.86f)), 40f, 3f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 160f, -R), new Vector3(R * 2f, 330f, 2f), paper2);
            kit.Deco(PrimitiveType.Cube, new Vector3(R, 160f, 0f), new Vector3(2f, 330f, R * 2f), paper2);
            kit.Deco(PrimitiveType.Cube, new Vector3(-R, 160f, 0f), new Vector3(2f, 330f, R * 2f), paper2);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 25f, -R + 2f), new Vector3(R * 2f, 54f, 2f), wains);
            kit.Deco(PrimitiveType.Cube, new Vector3(R - 2f, 25f, 0f), new Vector3(2f, 54f, R * 2f), wains);
            kit.Deco(PrimitiveType.Cube, new Vector3(-R + 2f, 25f, 0f), new Vector3(2f, 54f, R * 2f), wains);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 25f, R - 2f), new Vector3(R * 2f, 54f, 2f), wains);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 325f, 0f), new Vector3(R * 2f, 2f, R * 2f), Tiled(PatchworkAssets.Plaster(new Color(0.97f, 0.94f, 0.9f)), 20f, 20f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 322f, 0f), new Vector3(60f, 1.5f, 60f), PatchworkAssets.Plaster(new Color(0.98f, 0.95f, 0.88f)));
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 300f, 0f), new Vector3(50f, 30f, 50f), PatchworkAssets.Glow(new Color(1f, 0.92f, 0.75f), 1.1f));
            // Pictures on the walls: a sailing ship, a hot air balloon and a moon.
            Color[] frames = { new Color(0.85f, 0.7f, 0.35f), new Color(0.6f, 0.4f, 0.3f), new Color(0.95f, 0.9f, 0.8f) };
            Vector3[] pics = { new Vector3(R - 3f, 140f, -140f), new Vector3(R - 3f, 150f, 120f), new Vector3(-R + 3f, 160f, -150f), new Vector3(0f, 150f, -R + 3f) };
            for (int i = 0; i < pics.Length; i++)
            {
                Vector3 n = pics[i].x > 100f ? Vector3.left : pics[i].x < -100f ? Vector3.right : Vector3.forward;
                var rot = Quaternion.LookRotation(n);
                var f = kit.Deco(PrimitiveType.Cube, pics[i], new Vector3(70f, 56f, 3f), PatchworkAssets.Plain(frames[i % 3], 0.6f));
                f.rotation = rot;
                var canvas = kit.Deco(PrimitiveType.Cube, pics[i] + n * 1.8f, new Vector3(60f, 46f, 1f), PatchworkAssets.Fabric(new Color(0.7f, 0.82f, 0.95f)));
                canvas.rotation = rot;
                var subject = kit.Deco(PrimitiveType.Sphere, pics[i] + n * 2.6f + Vector3.up * 4f, new Vector3(22f, i % 2 == 0 ? 14f : 24f, 1f), PatchworkAssets.Felt(i % 2 == 0 ? new Color(0.95f, 0.6f, 0.55f) : new Color(0.98f, 0.88f, 0.55f)));
                subject.rotation = rot;
            }
            var paint = Tiled(PatchworkAssets.Planks(new Color(0.85f, 0.35f, 0.32f)), 12f, 4f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 20f, o + 0.06f), new Vector3(o * 2f, 44f, 0.1f), paint);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 20f, -o - 0.06f), new Vector3(o * 2f, 44f, 0.1f), paint);
            kit.Deco(PrimitiveType.Cube, new Vector3(o + 0.06f, 20f, 0f), new Vector3(0.1f, 44f, o * 2f), paint);
            kit.Deco(PrimitiveType.Cube, new Vector3(-o - 0.06f, 20f, 0f), new Vector3(0.1f, 44f, o * 2f), paint);
            // Big painted letters on the box's front: a star, a heart and a moon in gold.
            var gold = PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f);
            var heart = kit.Group("BoxHeart", new Vector3(30f, 22f, -o - 0.5f));
            HeartShape(kit, heart, 14f, gold);
            heart.localScale = new Vector3(1f, 1f, 0.06f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-30f, 22f, -o - 0.2f), new Vector3(16f, 0.1f, 16f), gold, new Vector3(90f, 0f, 0f));

            // The nursery wall far behind, with a window full of sky, and the giant furniture.
            var paper = Tiled(PatchworkAssets.Damask(new Color(0.75f, 0.82f, 0.9f)), 24f, 12f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 160f, 421f), new Vector3(R * 2f, 330f, 2f), paper);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 170f, 418.5f), new Vector3(260f, 200f, 1f), PatchworkAssets.Glow(new Color(0.75f, 0.88f, 1f), 0.6f));
            var sash = PatchworkAssets.Wood(new Color(0.97f, 0.95f, 0.9f));
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 170f, 417.5f), new Vector3(276f, 12f, 3f), sash);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 170f, 417.5f), new Vector3(10f, 216f, 3f), sash);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 68f, 416f), new Vector3(290f, 8f, 8f), sash);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 133f, 170f, 417.5f), new Vector3(10f, 216f, 3f), sash);
                var curtain = Tiled(PatchworkAssets.Fabric(new Color(0.95f, 0.75f, 0.8f)), 6f, 12f);
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 160f, 160f, 412f), new Vector3(50f, 250f, 4f), curtain, new Vector3(0f, 0f, s * 2f));
            }
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 288f, 410f), new Vector3(3f, 200f, 3f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f), new Vector3(0f, 0f, 90f));
            // The bed to the west: a tall wooden frame and a quilted mattress.
            var bedWood = Tiled(PatchworkAssets.Wood(new Color(0.75f, 0.55f, 0.38f)), 4f, 8f);
            kit.Deco(PrimitiveType.Cube, new Vector3(-215f, 28f, 0f), new Vector3(12f, 60f, 260f), bedWood);
            kit.Deco(PrimitiveType.Cube, new Vector3(-280f, 64f, 0f), new Vector3(130f, 26f, 250f), Tiled(PatchworkAssets.Fabric(new Color(0.85f, 0.88f, 0.98f)), 10f, 18f));
            kit.Deco(PrimitiveType.Cube, new Vector3(-275f, 80f, 0f), new Vector3(136f, 10f, 254f), PatchworkAssets.BalloonQuilt());
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(-215f, 60f, s * 130f), new Vector3(16f, 62f, 16f), bedWood);
                kit.Deco(PrimitiveType.Sphere, new Vector3(-215f, 124f, s * 130f), Vector3.one * 20f, PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f));
            }
            // A chest of drawers to the north-east, and a lamp glowing to the south-west.
            var chest = Tiled(PatchworkAssets.Wood(new Color(0.9f, 0.86f, 0.78f)), 6f, 6f);
            kit.Deco(PrimitiveType.Cube, new Vector3(230f, 70f, 230f), new Vector3(150f, 145f, 80f), chest, new Vector3(0f, -30f, 0f));
            var dr = kit.Group("Drawers", new Vector3(230f, 0f, 230f));
            dr.rotation = Quaternion.Euler(0f, -30f, 0f);
            for (int i = 0; i < 3; i++)
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 25f + i * 45f, -41f), new Vector3(136f, 38f, 3f), PatchworkAssets.Wood(new Color(0.75f, 0.85f, 0.95f)), default(Vector3), dr);
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Sphere, new Vector3(s * 35f, 25f + i * 45f, -43f), Vector3.one * 7f, PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f), default(Vector3), dr);
            }
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-230f, 60f, -260f), new Vector3(6f, 62f, 6f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-230f, 2f, -260f), new Vector3(60f, 4f, 60f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-230f, 140f, -260f), new Vector3(90f, 36f, 90f), PatchworkAssets.Glow(new Color(1f, 0.9f, 0.7f), 0.8f));
        }

        // ---------------------------------------------------------------- Giant toys round the edges

        static void AlphabetBlock(LevelKit kit, Vector3 center, float size, float yaw, Color body, Color face, int shape)
        {
            var b = kit.Block(center, Vector3.one * size, body, "ToyBlock", PatchworkAssets.Wood(body));
            b.rotation = Quaternion.Euler(0f, yaw, 0f);
            var r = b.rotation;
            var inset = PatchworkAssets.Felt(face);
            var sym = PatchworkAssets.Felt(Color.Lerp(body, Color.white, 0.2f));
            float h = size * 0.5f;
            for (int f = 0; f < 5; f++)
            {
                Quaternion fr = f < 4 ? r * Quaternion.Euler(0f, f * 90f, 0f) : r * Quaternion.Euler(90f, 0f, 0f);
                Vector3 n = fr * Vector3.back;
                Vector3 c = center + n * (h + 0.03f);
                var panel = kit.Deco(PrimitiveType.Cube, c, new Vector3(size * 0.82f, size * 0.82f, 0.06f), inset);
                panel.rotation = fr;
                int s = (shape + f) % 4;
                Vector3 sc = center + n * (h + 0.1f);
                if (s == 0)
                {
                    var g = new GameObject("Heart").transform;
                    g.SetParent(kit.Root, false);
                    g.position = sc;
                    g.rotation = fr;
                    HeartShape(kit, g, size * 0.5f, sym);
                    g.localScale = new Vector3(1f, 1f, 0.15f);
                }
                else if (s == 1)
                {
                    var d = kit.Deco(PrimitiveType.Cylinder, sc, new Vector3(size * 0.5f, 0.05f, size * 0.5f), sym);
                    d.rotation = fr * Quaternion.Euler(90f, 0f, 0f);
                }
                else if (s == 2)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        var st = kit.Deco(PrimitiveType.Cube, sc, new Vector3(size * 0.38f, size * 0.38f, 0.1f), sym);
                        st.rotation = fr * Quaternion.Euler(0f, 0f, k * 45f);
                    }
                }
                else
                {
                    var dm = kit.Deco(PrimitiveType.Cube, sc, new Vector3(size * 0.4f, size * 0.4f, 0.1f), sym);
                    dm.rotation = fr * Quaternion.Euler(0f, 0f, 45f);
                }
            }
        }

        static void Crayon(LevelKit kit, Vector3 baseP, Vector3 tipP, Color col)
        {
            Vector3 d = tipP - baseP;
            float len = d.magnitude;
            var rot = Quaternion.FromToRotation(Vector3.up, d / len);
            float body = len * 0.8f;
            var wax = PatchworkAssets.Plain(col, 0.35f);
            var t = kit.Deco(PrimitiveType.Cylinder, baseP + d / len * body * 0.5f, new Vector3(1.4f, body * 0.5f, 1.4f), wax);
            t.rotation = rot;
            // Solid along its whole length.
            var solid = new GameObject("Solid");
            solid.transform.SetParent(kit.Root, false);
            solid.transform.position = baseP + d * 0.5f;
            solid.transform.rotation = rot;
            var cap = solid.AddComponent<CapsuleCollider>();
            cap.direction = 1;
            cap.radius = 0.72f;
            cap.height = len + 1.4f;
            var paper = kit.Deco(PrimitiveType.Cylinder, baseP + d / len * body * 0.5f, new Vector3(1.44f, body * 0.32f, 1.44f), PatchworkAssets.Felt(Color.Lerp(col, Color.white, 0.55f)));
            paper.rotation = rot;
            for (int i = 0; i < 2; i++)
            {
                var band = kit.Deco(PrimitiveType.Cylinder, baseP + d / len * body * (0.2f + i * 0.6f), new Vector3(1.47f, 0.1f, 1.47f), PatchworkAssets.Felt(Color.Lerp(col, Color.black, 0.3f)));
                band.rotation = rot;
            }
            // The tip: a stack of narrowing discs.
            for (int i = 0; i < 6; i++)
            {
                float k = i / 6f;
                var disc = kit.Deco(PrimitiveType.Cylinder, baseP + d / len * (body + (len - body) * (k + 0.08f)), new Vector3(1.4f * (1f - k * 0.9f), (len - body) / 12f, 1.4f * (1f - k * 0.9f)), wax);
                disc.rotation = rot;
            }
        }

        static void GiantToys(LevelKit kit)
        {
            // Alphabet blocks: a climbable stack in two corners, with threads on top.
            AlphabetBlock(kit, new Vector3(-82f, 2f, -72f), 4f, 10f, new Color(0.85f, 0.35f, 0.35f), new Color(0.98f, 0.9f, 0.75f), 0);
            AlphabetBlock(kit, new Vector3(-77.5f, 2f, -73f), 4f, -8f, new Color(0.35f, 0.55f, 0.85f), new Color(0.98f, 0.9f, 0.75f), 1);
            AlphabetBlock(kit, new Vector3(-80f, 6f, -72.3f), 4f, 25f, new Color(0.95f, 0.75f, 0.3f), new Color(0.98f, 0.9f, 0.75f), 2);
            kit.ThreadLine(new Vector3(-80.5f, 8.8f, -72.5f), new Vector3(-79.5f, 8.8f, -72f), 2);
            AlphabetBlock(kit, new Vector3(82f, 2.5f, 78f), 5f, 0f, new Color(0.45f, 0.7f, 0.45f), new Color(0.98f, 0.9f, 0.75f), 3);
            AlphabetBlock(kit, new Vector3(76.5f, 2.5f, 79f), 5f, 12f, new Color(0.75f, 0.5f, 0.85f), new Color(0.98f, 0.9f, 0.75f), 0);
            AlphabetBlock(kit, new Vector3(79.5f, 7.5f, 78.5f), 5f, -15f, new Color(0.95f, 0.55f, 0.4f), new Color(0.98f, 0.9f, 0.75f), 1);
            kit.ThreadLine(new Vector3(79f, 10.8f, 78.5f), new Vector3(80f, 10.8f, 78.5f), 2);
            AlphabetBlock(kit, new Vector3(-30f, 1.5f, 84f), 3f, 30f, new Color(0.35f, 0.55f, 0.85f), new Color(0.98f, 0.9f, 0.75f), 2);
            AlphabetBlock(kit, new Vector3(64f, 1.5f, -84f), 3f, -20f, new Color(0.85f, 0.35f, 0.35f), new Color(0.98f, 0.9f, 0.75f), 3);

            // Crayons leaning on the walls.
            Color[] crayons = { new Color(0.85f, 0.25f, 0.3f), new Color(0.3f, 0.5f, 0.9f), new Color(0.95f, 0.8f, 0.25f), new Color(0.4f, 0.7f, 0.4f), new Color(0.6f, 0.4f, 0.8f) };
            for (int i = 0; i < 5; i++)
            {
                float x = -20f + i * 3.4f;
                Crayon(kit, new Vector3(x, 0.7f, Half - 9f - (i % 2) * 1.5f), new Vector3(x + 1.5f, 15f + (i % 3) * 1.5f, Half - 0.9f), crayons[i]);
            }
            for (int i = 0; i < 3; i++)
            {
                float z = -40f + i * 3.2f;
                Crayon(kit, new Vector3(Half - 8f, 0.7f, z), new Vector3(Half - 0.9f, 14f + i, z - 1f), crayons[(i + 2) % 5]);
            }

            // A rocking horse in the north-west corner, rocking gently.
            var horseRoot = kit.Group("RockingHorse", new Vector3(-62f, 0f, 74f));
            horseRoot.rotation = Quaternion.Euler(0f, 70f, 0f);
            var horse = new GameObject("Rock").transform;
            horse.SetParent(horseRoot, false);
            horse.localPosition = Vector3.zero;
            var dapple = PatchworkAssets.Felt(new Color(0.96f, 0.93f, 0.88f));
            var red = PatchworkAssets.Wood(new Color(0.8f, 0.25f, 0.25f));
            var goldM = PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f);
            for (int i = 0; i < 9; i++)
            {
                float a = (i - 4) * 0.16f;
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.6f + (1f - Mathf.Cos(a)) * 14f, Mathf.Sin(a) * 14f) + new Vector3(-1.6f, 0f, 0f), new Vector3(0.4f, 0.4f, 2.4f), red, new Vector3(-a * Mathf.Rad2Deg, 0f, 0f), horse);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.6f + (1f - Mathf.Cos(a)) * 14f, Mathf.Sin(a) * 14f) + new Vector3(1.6f, 0f, 0f), new Vector3(0.4f, 0.4f, 2.4f), red, new Vector3(-a * Mathf.Rad2Deg, 0f, 0f), horse);
            }
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(sx * 1.4f, 3f, sz * 2.6f), new Vector3(0.5f, 2.4f, 0.5f), dapple, new Vector3(sz * 12f, 0f, -sx * 8f), horse);
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 6f, 0f), new Vector3(3.6f, 4f, 3.6f), dapple, new Vector3(90f, 0f, 0f), horse);
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 8.6f, 3.6f), new Vector3(1.6f, 2.2f, 1.6f), dapple, new Vector3(30f, 0f, 0f), horse);
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 9.4f, 5.2f), new Vector3(1.5f, 1.6f, 1.4f), dapple, new Vector3(100f, 0f, 0f), horse);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 7.6f, -0.3f), new Vector3(3.8f, 0.4f, 2.6f), red, default(Vector3), horse);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 8.6f, 4.4f), new Vector3(0.18f, 1.6f, 0.18f), goldM, new Vector3(0f, 0f, 90f), horse);
            for (int i = 0; i < 7; i++)
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 8.6f + i * 0.3f, 2.6f + i * 0.3f), new Vector3(0.6f, 0.8f, 0.5f), PatchworkAssets.Yarn(new Color(0.55f, 0.35f, 0.25f)), default(Vector3), horse);
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 5f, -4.2f), new Vector3(0.7f, 2.2f, 0.7f), PatchworkAssets.Yarn(new Color(0.55f, 0.35f, 0.25f)), new Vector3(-30f, 0f, 0f), horse);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.7f, 9.8f, 5.6f), Vector3.one * 0.3f, PatchworkAssets.Plain(UIKit.Ink, 0.6f), default(Vector3), horse);
            var rock = horse.gameObject.AddComponent<Rocker>();
            rock.Axis = Vector3.right;
            rock.Amount = 6f;
            rock.Speed = 0.9f;
            Solid(kit, new Vector3(-62f, 4f, 74f), new Vector3(4f, 8f, 8f), 70f);

            // A spinning top wobbling about on the hill.
            var topRoot = kit.Group("SpinningTop", new Vector3(-66f, 0f, -52f));
            var wobble = topRoot.gameObject.AddComponent<Rocker>();
            wobble.Axis = new Vector3(1f, 0f, 0.6f);
            wobble.Amount = 7f;
            wobble.Speed = 1.7f;
            var top = new GameObject("Top").transform;
            top.SetParent(topRoot, false);
            for (int i = 0; i < 6; i++)
            {
                float w = 3.2f * Mathf.Sin((i + 1) / 7f * Mathf.PI);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.4f + i * 0.55f, 0f), new Vector3(w, 0.28f, w), PatchworkAssets.Wood(i % 2 == 0 ? new Color(0.95f, 0.4f, 0.4f) : new Color(0.98f, 0.9f, 0.6f)), default(Vector3), top);
            }
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 4f, 0f), new Vector3(0.3f, 0.9f, 0.3f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f), default(Vector3), top);
            var spin = top.gameObject.AddComponent<Spinner>();
            spin.Speed = 160f;
            Solid(kit, new Vector3(-66f, 1.8f, -52f), new Vector3(2.6f, 3.6f, 2.6f));
            ClearGrass(new Vector3(-66f, 0f, -52f), 1.8f);

            // Marbles you can stand on, and toy jacks.
            Color[] swirl = { new Color(0.95f, 0.4f, 0.45f), new Color(0.35f, 0.6f, 0.95f), new Color(0.98f, 0.8f, 0.3f), new Color(0.45f, 0.8f, 0.5f) };
            Vector3[] marbles = { new Vector3(-74f, 0f, -60f), new Vector3(-58f, 0f, -66f), new Vector3(70f, 0f, 70f), new Vector3(86f, 0f, 60f), new Vector3(-16f, 0f, 63f), new Vector3(24f, 0f, -80f) };
            for (int i = 0; i < marbles.Length; i++)
            {
                float r = 1f + (i % 3) * 0.35f;
                var m = kit.Group("Marble", marbles[i] + Vector3.up * r);
                kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * r * 2f, PatchworkAssets.Bubble(new Color(0.85f, 0.95f, 1f, 0.45f)), default(Vector3), m);
                kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(r * 1.6f, r * 0.5f, r * 0.8f), PatchworkAssets.Plain(swirl[i % swirl.Length], 0.8f), new Vector3(30f, i * 50f, 20f), m);
                kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(r * 0.5f, r * 1.4f, r * 0.7f), PatchworkAssets.Plain(swirl[(i + 1) % swirl.Length], 0.8f), new Vector3(-20f, i * 30f, 40f), m);
                m.gameObject.AddComponent<SphereCollider>().radius = r;
                ClearGrass(marbles[i], r);
            }
            Vector3[] jacks = { new Vector3(-70f, 0f, -63f), new Vector3(74f, 0f, 66f), new Vector3(-26f, 0f, 76f) };
            foreach (var j in jacks)
            {
                Solid(kit, j + Vector3.up * 0.9f, new Vector3(1.8f, 1.8f, 1.8f));
                var g = kit.Group("Jack", j + Vector3.up * 0.9f);
                g.rotation = Quaternion.Euler(20f, j.x * 3f, 15f);
                var metal = PatchworkAssets.Plain(new Color(0.85f, 0.4f, 0.4f), 0.85f);
                Vector3[] axes = { Vector3.right, Vector3.up, Vector3.forward };
                foreach (var ax in axes)
                {
                    var arm = kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.22f, 1f, 0.22f), metal, default(Vector3), g);
                    arm.localRotation = Quaternion.FromToRotation(Vector3.up, ax);
                    kit.Deco(PrimitiveType.Sphere, ax * 1f, Vector3.one * 0.45f, metal, default(Vector3), g);
                    kit.Deco(PrimitiveType.Sphere, -ax * 1f, Vector3.one * 0.45f, metal, default(Vector3), g);
                }
            }
            // Big balls of yarn with knitting needles stuck in them.
            kit.YarnBall(new Vector3(-84f, 0f, 20f), 2.4f, LevelKit.Palette.Lavender);
            kit.KnittingNeedle(new Vector3(-84.5f, 2.2f, 19f), new Vector3(-86f, 7f, 15f), LevelKit.Palette.Rose);
            kit.YarnBall(new Vector3(84f, 0f, 30f), 2f, LevelKit.Palette.Butter);
            kit.KnittingNeedle(new Vector3(83.5f, 2f, 30.5f), new Vector3(86f, 6.5f, 34f), LevelKit.Palette.Sky);
            kit.YarnBall(new Vector3(20f, 0f, 86f), 1.8f, LevelKit.Palette.Peach);
        }

        // ---------------------------------------------------------------- The square

        static void SquareDressing(LevelKit kit)
        {
            // The wedding cake on a little round table by the arch.
            Vector3 t = new Vector3(7.6f, 0.06f, -3.2f);
            var white = PatchworkAssets.Felt(new Color(0.98f, 0.97f, 0.94f));
            kit.Deco(PrimitiveType.Cylinder, t + Vector3.up * 0.45f, new Vector3(0.18f, 0.45f, 0.18f), PatchworkAssets.Wood(TownKit.DarkWood));
            kit.Deco(PrimitiveType.Cylinder, t + Vector3.up * 0.86f, new Vector3(2.1f, 0.06f, 2.1f), white);
            kit.Deco(PrimitiveType.Cylinder, t + Vector3.up * 0.7f, new Vector3(2.2f, 0.18f, 2.2f), PatchworkAssets.Fabric(new Color(0.95f, 0.85f, 0.88f)));
            float y = 0.92f;
            for (int i = 0; i < 3; i++)
            {
                float r = 1.3f - i * 0.36f, h = 0.42f;
                kit.Deco(PrimitiveType.Cylinder, t + Vector3.up * (y + h * 0.5f), new Vector3(r, h * 0.5f, r), white);
                kit.Deco(PrimitiveType.Cylinder, t + Vector3.up * (y + 0.04f), new Vector3(r + 0.05f, 0.03f, r + 0.05f), PatchworkAssets.Felt(new Color(1f, 0.7f, 0.78f)));
                for (int k = 0; k < 8; k++)
                {
                    float a = k / 8f * Mathf.PI * 2f + i;
                    kit.Deco(PrimitiveType.Sphere, t + new Vector3(Mathf.Cos(a) * r * 0.5f, y + h, Mathf.Sin(a) * r * 0.5f), Vector3.one * 0.1f, PatchworkAssets.Felt(FlowerColors[(k + i) % FlowerColors.Length]));
                }
                y += h;
            }
            // Tiny bride and groom on top.
            kit.Deco(PrimitiveType.Capsule, t + new Vector3(-0.1f, y + 0.12f, 0f), new Vector3(0.1f, 0.12f, 0.1f), PatchworkAssets.Felt(new Color(0.62f, 0.42f, 0.28f)));
            kit.Deco(PrimitiveType.Capsule, t + new Vector3(0.1f, y + 0.12f, 0f), new Vector3(0.1f, 0.12f, 0.1f), white);
            Solid(kit, t + Vector3.up * 0.6f, new Vector3(2.1f, 1.2f, 2.1f));

            // White chairs for the guests behind them, and a garland on the arch posts.
            for (int row = 0; row < 2; row++)
                for (int s = -1; s <= 1; s += 2)
                    for (int c = 0; c < 3; c++)
                    {
                        Vector3 p = new Vector3(s * (2.8f + c * 1.1f), 0.06f, -12.2f - row * 1.4f);
                        whiteBatch.Box(p + Vector3.up * 0.45f, new Vector3(0.75f, 0.08f, 0.7f), Quaternion.identity);
                        whiteBatch.Box(p + new Vector3(0f, 0.85f, -0.32f), new Vector3(0.75f, 0.75f, 0.06f), Quaternion.identity);
                        for (int lx = -1; lx <= 1; lx += 2)
                            for (int lz = -1; lz <= 1; lz += 2)
                                whiteBatch.Box(p + new Vector3(lx * 0.32f, 0.22f, lz * 0.3f), new Vector3(0.06f, 0.44f, 0.06f), Quaternion.identity);
                        if (c == 1) Solid(kit, p + Vector3.up * 0.25f, new Vector3(3.1f, 0.5f, 0.75f));
                        // A satin bow on every chair back.
                        kit.Deco(PrimitiveType.Sphere, p + new Vector3(0f, 1.05f, -0.36f), new Vector3(0.3f, 0.16f, 0.06f), PatchworkAssets.Felt(new Color(1f, 0.72f, 0.8f)));
                    }

            // Market stalls on either side of the square.
            MarketStall(kit, new Vector3(-16.5f, 0.06f, 11f), 90f, new Color(0.85f, 0.3f, 0.35f), 0);
            MarketStall(kit, new Vector3(16.5f, 0.06f, 11f), -90f, new Color(0.3f, 0.5f, 0.85f), 1);

            // A red pillar post box, a signpost pointing to each part of town, flower pots by the doors.
            PostBox(kit, new Vector3(4.2f, 0.06f, -17.6f));
            Signpost(kit, new Vector3(-4.4f, 0.06f, -17.4f));
            var houses = new[] { new Vector4(-12f, 27f, 180f, 8f), new Vector4(12f, 27f, 180f, 8f), new Vector4(-24f, 13f, 90f, 9f), new Vector4(-24f, -14f, 90f, 9f),
                                 new Vector4(24f, 13f, -90f, 9f), new Vector4(24f, -13f, -90f, 9f), new Vector4(-13f, -26f, 0f, 8f), new Vector4(13f, -26f, 0f, 8f) };
            for (int i = 0; i < houses.Length; i++)
            {
                var h = houses[i];
                var r = Quaternion.Euler(0f, h.z, 0f);
                Vector3 c = new Vector3(h.x, 0.06f, h.y);
                FlowerPot(kit, c + r * new Vector3(-1.4f, 0f, h.w * 0.5f + 0.5f), FlowerColors[i % FlowerColors.Length]);
                FlowerPot(kit, c + r * new Vector3(1.4f, 0f, h.w * 0.5f + 0.5f), FlowerColors[(i + 2) % FlowerColors.Length]);
                if (i % 2 == 0) Barrel(kit, c + r * new Vector3(-3.8f, 0f, h.w * 0.5f + 0.7f), i * 40f);
            }
            // A penny-farthing leaning by the bakery.
            PennyFarthing(kit, new Vector3(-16.6f, 0f, 22.4f), 90f);
            // Puddles on the cobbles.
            var puddle = PatchworkAssets.Water(new Color(0.55f, 0.7f, 0.88f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-2.5f, 0.065f, -28f), new Vector3(1.6f, 0.005f, 1.1f), puddle);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(2.8f, 0.065f, -34f), new Vector3(1.1f, 0.005f, 0.8f), puddle);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-27f, 0.055f, 2f), new Vector3(1.3f, 0.005f, 0.9f), puddle);
        }

        static void MarketStall(LevelKit kit, Vector3 feet, float yaw, Color stripe, int kind)
        {
            var g = kit.Group("MarketStall", feet);
            g.rotation = Quaternion.Euler(0f, yaw, 0f);
            var wood = PatchworkAssets.Planks(new Color(0.7f, 0.5f, 0.34f));
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.5f, 0f), new Vector3(3.2f, 1f, 1.3f), wood, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.03f, 0.05f), new Vector3(3.4f, 0.06f, 1.5f), PatchworkAssets.Wood(TownKit.DarkWood), default(Vector3), g);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 1.55f, 1.4f, 0.65f), new Vector3(0.12f, 1.4f, 0.12f), PatchworkAssets.Wood(TownKit.DarkWood), default(Vector3), g);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 1.55f, 1.4f, -0.6f), new Vector3(0.12f, 1.4f, 0.12f), PatchworkAssets.Wood(TownKit.DarkWood), default(Vector3), g);
            }
            // Striped awning, with a scalloped trim.
            for (int i = 0; i < 6; i++)
            {
                var m = PatchworkAssets.Fabric(i % 2 == 0 ? stripe : new Color(0.98f, 0.95f, 0.88f));
                kit.Deco(PrimitiveType.Cube, new Vector3(-1.5f + i * 0.6f + 0.3f, 2.85f, 0.2f), new Vector3(0.6f, 0.05f, 2f), m, new Vector3(-14f, 0f, 0f), g);
                kit.Deco(PrimitiveType.Sphere, new Vector3(-1.5f + i * 0.6f + 0.3f, 2.55f, 1.2f), new Vector3(0.58f, 0.32f, 0.05f), m, default(Vector3), g);
            }
            // What's for sale.
            Color[] fruit = kind == 0 ? new[] { new Color(0.9f, 0.25f, 0.25f), new Color(0.98f, 0.6f, 0.2f), new Color(0.6f, 0.8f, 0.3f) }
                                      : new[] { LevelKit.Palette.Rose, LevelKit.Palette.Sky, LevelKit.Palette.Butter, LevelKit.Palette.Lavender };
            for (int c = 0; c < 3; c++)
            {
                Vector3 crate = new Vector3(-1f + c * 1f, 1.06f, 0.1f);
                kit.Deco(PrimitiveType.Cube, crate + Vector3.up * 0.15f, new Vector3(0.9f, 0.3f, 0.9f), PatchworkAssets.Planks(new Color(0.8f, 0.62f, 0.42f)), default(Vector3), g);
                for (int i = 0; i < 9; i++)
                {
                    Vector3 p = crate + new Vector3(((i % 3) - 1) * 0.27f, 0.36f + (i % 2) * 0.05f, ((i / 3) - 1) * 0.27f);
                    if (kind == 0) kit.Deco(PrimitiveType.Sphere, p, Vector3.one * 0.26f, PatchworkAssets.Felt(fruit[c % fruit.Length]), default(Vector3), g);
                    else kit.Deco(PrimitiveType.Cylinder, p, new Vector3(0.22f, 0.12f, 0.22f), PatchworkAssets.Yarn(fruit[(i + c) % fruit.Length]), default(Vector3), g);
                }
            }
            // A chalkboard sign.
            kit.Deco(PrimitiveType.Cube, new Vector3(1.9f, 0.55f, 1.1f), new Vector3(0.7f, 0.9f, 0.06f), PatchworkAssets.Plain(new Color(0.15f, 0.18f, 0.16f), 0.2f), new Vector3(-12f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(1.9f, 0.62f, 1.14f), new Vector3(0.4f, 0.04f, 0.02f), PatchworkAssets.Felt(Color.white), new Vector3(-12f, 0f, 0f), g);
            Solid(kit, feet + Vector3.up * 0.55f, new Vector3(3.3f, 1.1f, 1.4f), yaw);
        }

        static void PostBox(LevelKit kit, Vector3 feet)
        {
            var red = PatchworkAssets.Plain(new Color(0.85f, 0.15f, 0.15f), 0.6f);
            kit.Deco(PrimitiveType.Cylinder, feet + Vector3.up * 0.75f, new Vector3(0.7f, 0.75f, 0.7f), red);
            kit.Deco(PrimitiveType.Cylinder, feet + Vector3.up * 1.52f, new Vector3(0.82f, 0.05f, 0.82f), red);
            kit.Deco(PrimitiveType.Sphere, feet + Vector3.up * 1.6f, new Vector3(0.74f, 0.3f, 0.74f), red);
            kit.Deco(PrimitiveType.Cylinder, feet + Vector3.up * 0.08f, new Vector3(0.82f, 0.08f, 0.82f), red);
            kit.Deco(PrimitiveType.Cube, feet + new Vector3(0f, 1.2f, -0.36f), new Vector3(0.36f, 0.06f, 0.04f), PatchworkAssets.Plain(UIKit.Ink, 0.4f));
            kit.Deco(PrimitiveType.Cube, feet + new Vector3(0f, 0.95f, -0.355f), new Vector3(0.3f, 0.22f, 0.03f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f));
            Solid(kit, feet + Vector3.up * 0.85f, new Vector3(0.7f, 1.7f, 0.7f));
        }

        // A signpost with arrows to the three corners of town, each with a little picture:
        // a pink spool (Spindle Hill, west), a blue drop (Dye Works, east), a white swirl (rooftops, south).
        static void Signpost(LevelKit kit, Vector3 feet)
        {
            var post = PatchworkAssets.Wood(new Color(0.55f, 0.38f, 0.26f));
            kit.Deco(PrimitiveType.Cylinder, feet + Vector3.up * 1.4f, new Vector3(0.16f, 1.4f, 0.16f), post);
            Solid(kit, feet + Vector3.up * 1.4f, new Vector3(0.3f, 2.8f, 0.3f));
            kit.Deco(PrimitiveType.Sphere, feet + Vector3.up * 2.85f, Vector3.one * 0.22f, PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f));
            var board = PatchworkAssets.Planks(new Color(0.9f, 0.82f, 0.62f));
            float[] yaws = { -90f, 90f, 180f };
            Color[] icons = { LevelKit.Palette.AnchorPink, new Color(0.35f, 0.6f, 0.95f), Color.white };
            for (int i = 0; i < 3; i++)
            {
                var arm = kit.Group("SignArm", feet + Vector3.up * (2.45f - i * 0.45f));
                arm.rotation = Quaternion.Euler(0f, yaws[i], 0f);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, 0.65f), new Vector3(0.08f, 0.34f, 1.1f), board, default(Vector3), arm);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, 1.2f), new Vector3(0.08f, 0.24f, 0.24f), board, new Vector3(45f, 0f, 0f), arm);
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.05f, 0f, 0.6f), new Vector3(0.03f, 0.2f, 0.2f), PatchworkAssets.Plain(icons[i], 0.6f), default(Vector3), arm);
            }
        }

        static void PennyFarthing(LevelKit kit, Vector3 feet, float yaw)
        {
            var g = kit.Group("PennyFarthing", feet);
            g.rotation = Quaternion.Euler(0f, yaw, 8f);
            var iron = PatchworkAssets.Plain(UIKit.Ink, 0.5f);
            var tyre = PatchworkAssets.Plain(new Color(0.15f, 0.13f, 0.14f), 0.3f);
            // Big wheel and little wheel as rings of beads, with spokes.
            for (int w = 0; w < 2; w++)
            {
                float r = w == 0 ? 1.1f : 0.35f;
                Vector3 c = w == 0 ? new Vector3(0f, 1.1f, 0.4f) : new Vector3(0f, 0.35f, -1.3f);
                for (int i = 0; i < (w == 0 ? 28 : 12); i++)
                {
                    float a = i / (w == 0 ? 28f : 12f) * Mathf.PI * 2f;
                    kit.Deco(PrimitiveType.Sphere, c + new Vector3(0f, Mathf.Sin(a) * r, Mathf.Cos(a) * r), Vector3.one * 0.13f, tyre, default(Vector3), g);
                }
                for (int i = 0; i < 6; i++)
                    kit.Deco(PrimitiveType.Cube, c, new Vector3(0.02f, r * 2f, 0.02f), iron, new Vector3(i * 30f, 0f, 0f), g);
            }
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.6f, -0.4f), new Vector3(0.06f, 1.1f, 0.06f), iron, new Vector3(-55f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 2.3f, 0.1f), new Vector3(0.22f, 0.12f, 0.4f), PatchworkAssets.Felt(new Color(0.45f, 0.28f, 0.2f)), new Vector3(90f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 2.4f, 0.45f), new Vector3(0.05f, 0.45f, 0.05f), iron, new Vector3(0f, 0f, 90f), g);
            Solid(kit, feet + Vector3.up * 1.1f, new Vector3(0.4f, 2.2f, 2.6f), yaw);
        }

        // ---------------------------------------------------------------- Streets, hedges, railings

        static void StringLights(LevelKit kit, Vector3 a, Vector3 b, float sag)
        {
            var line = PatchworkAssets.Plain(UIKit.Ink, 0.4f);
            const int segs = 10;
            System.Func<float, Vector3> P = k => Vector3.Lerp(a, b, k) + Vector3.down * Mathf.Sin(k * Mathf.PI) * sag;
            Color[] bulbs = { new Color(1f, 0.85f, 0.55f), new Color(1f, 0.6f, 0.65f), new Color(0.7f, 0.85f, 1f), new Color(0.85f, 1f, 0.7f) };
            for (int i = 0; i < segs; i++)
            {
                kit.Rope(P(i / (float)segs), P((i + 1f) / segs), line, 0.03f);
                Vector3 p = P((i + 0.5f) / segs) + Vector3.down * 0.18f;
                kit.Deco(PrimitiveType.Sphere, p, new Vector3(0.18f, 0.24f, 0.18f), PatchworkAssets.Glow(bulbs[i % bulbs.Length], 1.4f));
            }
        }

        static void StreetDressing(LevelKit kit)
        {
            StringLights(kit, new Vector3(-8.4f, 5.4f, -23.5f), new Vector3(8.4f, 5.4f, -23.5f), 0.7f);
            StringLights(kit, new Vector3(-8.4f, 5.4f, -28.5f), new Vector3(8.4f, 5.4f, -28.5f), 0.7f);
            StringLights(kit, new Vector3(-28.4f, 5.4f, 8.05f), new Vector3(-28.4f, 5.4f, -8.95f), 0.8f);
            StringLights(kit, new Vector3(19.6f, 5.4f, 8.05f), new Vector3(19.6f, 5.4f, -8.05f), 0.8f);

            // Hedges along the ravine (leaving the swing and the steps open), and round the barracks.
            Hedge(kit, new Vector3(-36.2f, 0f, 16.5f), new Vector3(-36.2f, 0f, 60f));
            Hedge(kit, new Vector3(-36.2f, 0f, -24f), new Vector3(-36.2f, 0f, -70f));
            Hedge(kit, new Vector3(-17f, 0f, 33f), new Vector3(-17f, 0f, 57f));
            Hedge(kit, new Vector3(17f, 0f, 33f), new Vector3(17f, 0f, 57f));
            // Iron railings along the canal (gaps for the bridges and the steps).
            float[][] west = { new[] { 3f, 12.5f }, new[] { 18f, 37f }, new[] { 43f, 58.2f }, new[] { 61.8f, 88f }, new[] { -3f, -37f }, new[] { -43f, -88f } };
            foreach (var s in west) Railing(kit, new Vector3(CanalWest - 0.25f, 0f, s[0]), new Vector3(CanalWest - 0.25f, 0f, s[1]));
            float[][] east = { new[] { 3f, 37f }, new[] { 43f, 58.2f }, new[] { 61.8f, 88f }, new[] { -3f, -12.5f }, new[] { -18f, -37f }, new[] { -43f, -88f } };
            foreach (var s in east) Railing(kit, new Vector3(CanalEast + 0.25f, 0.05f, s[0]), new Vector3(CanalEast + 0.25f, 0.05f, s[1]));
            // Lollipop trees along the promenade and round the square's corners.
            Color[] leaves = { new Color(0.45f, 0.7f, 0.42f), new Color(0.95f, 0.65f, 0.7f), new Color(0.55f, 0.75f, 0.5f), new Color(0.98f, 0.82f, 0.5f) };
            float[] promenade = { -78f, -66f, -18f, -6f, 22f, 34f, 46f, 58f, 70f, 82f };
            for (int i = 0; i < promenade.Length; i++) PomTree(kit, new Vector3(48.6f, 0f, promenade[i]), 4.5f + (i % 3) * 0.6f, leaves[i % leaves.Length]);
            Vector3[] corners = { new Vector3(-20f, 0f, 21f), new Vector3(20f, 0f, 21f), new Vector3(-20f, 0f, -21f), new Vector3(20f, 0f, -21f), new Vector3(-30f, 0f, 26f), new Vector3(29f, 0f, 26f), new Vector3(-30f, 0f, -28f), new Vector3(29f, 0f, -28f) };
            for (int i = 0; i < corners.Length; i++) PomTree(kit, corners[i], 4.8f + (i % 2) * 0.8f, leaves[(i + 1) % leaves.Length]);
            // South end: hedged gardens behind the rooftop terraces.
            Hedge(kit, new Vector3(-34f, 0f, -66f), new Vector3(-22f, 0f, -66f));
            Hedge(kit, new Vector3(22f, 0f, -66f), new Vector3(34f, 0f, -66f));
            for (int i = 0; i < 4; i++) PomTree(kit, new Vector3(-30f + i * 20f, 0f, -84f), 5f, leaves[i % leaves.Length]);
        }

        // ---------------------------------------------------------------- Canal, ravine

        static void Duck(LevelKit kit, Vector3[] loop, float start, float scale)
        {
            var d = kit.Group("RubberDuck", loop[0]);
            var yellow = PatchworkAssets.Plain(new Color(1f, 0.85f, 0.2f), 0.75f);
            var orange = PatchworkAssets.Plain(new Color(1f, 0.55f, 0.15f), 0.75f);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.25f, 0f) * scale, new Vector3(0.9f, 0.7f, 1.2f) * scale, yellow, default(Vector3), d);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.25f, -0.55f) * scale, new Vector3(0.5f, 0.6f, 0.4f) * scale, yellow, new Vector3(-30f, 0f, 0f), d);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.8f, 0.35f) * scale, Vector3.one * 0.6f * scale, yellow, default(Vector3), d);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.72f, 0.7f) * scale, new Vector3(0.3f, 0.12f, 0.3f) * scale, orange, default(Vector3), d);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.2f, 0.9f, 0.55f) * scale, Vector3.one * 0.1f * scale, PatchworkAssets.Plain(UIKit.Ink, 0.7f), default(Vector3), d);
            var squeak = kit.Volume<SqueakToy>(d.position + Vector3.up * 0.5f * scale, Vector3.one * (1.1f * scale + 0.6f), "Squeak");
            squeak.transform.SetParent(d, true);
            squeak.Body = d.GetChild(0);
            squeak.Pitch = 1.6f / Mathf.Max(0.5f, scale);
            var m = d.gameObject.AddComponent<PathMover>();
            m.BobHeight = 0.05f;
            m.BobSpeed = 2.2f;
            m.Sway = 5f;
            m.Setup(loop, 0.9f, start);
        }

        static void CanalDressing(LevelKit kit)
        {
            float wy = CanalSurface;
            Vector3[] loop = { new Vector3(34f, wy, -85f), new Vector3(34f, wy, 85f), new Vector3(36.8f, wy, 85f), new Vector3(36.8f, wy, -85f) };
            for (int i = 0; i < 6; i++) Duck(kit, loop, i * 61f, i == 0 ? 1.6f : 0.8f);
            // Lily pads with pink flowers, reeds and cattails along both edges.
            var pad = PatchworkAssets.Felt(new Color(0.38f, 0.62f, 0.36f));
            for (int i = 0; i < 16; i++)
            {
                float z = -80f + i * 10.5f + (i % 3) * 1.7f;
                float x = i % 2 == 0 ? 32.8f + (i % 3) * 0.3f : 39.1f - (i % 3) * 0.3f;
                if (Mathf.Abs(z) < 4f || Mathf.Abs(Mathf.Abs(z) - 40f) < 3f) continue;
                kit.Deco(PrimitiveType.Cylinder, new Vector3(x, wy + 0.02f, z), new Vector3(1f, 0.01f, 1f), pad);
                if (i % 3 == 0) kit.Deco(PrimitiveType.Sphere, new Vector3(x + 0.15f, wy + 0.12f, z), new Vector3(0.3f, 0.2f, 0.3f), PatchworkAssets.Felt(new Color(1f, 0.7f, 0.8f)));
                for (int r = 0; r < 5; r++)
                {
                    float rx = (i % 2 == 0 ? CanalWest + 0.25f : CanalEast - 0.25f), rz = z + 2f + r * 0.3f;
                    // Reeds rise out of the water by the wall; each cattail sits on top of its reed.
                    float rh = 1.1f + (r % 3) * 0.25f;
                    Vector3 root = new Vector3(rx, wy - 0.3f, rz);
                    Vector3 lean = new Vector3(0.1f * (r - 2), 0f, 0.05f);
                    reedBatch.Blade(root, rh, 0.06f, lean, r * 40f);
                    if (r % 2 == 0) cattailBatch.Ball(root + Vector3.up * (rh - 0.12f) + lean * 0.9f, new Vector3(0.12f, 0.36f, 0.12f), 3, 6);
                }
            }
            // A rowboat tied up by the steps, with bollards on the promenade.
            var boat = kit.Group("Rowboat", new Vector3(38.6f, wy + 0.05f, -20f));
            var wood = PatchworkAssets.Planks(new Color(0.42f, 0.55f, 0.75f));
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(1.5f, 0.6f, 3.6f), wood, default(Vector3), boat);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.2f, 0f), new Vector3(1.3f, 0.08f, 0.35f), PatchworkAssets.Wood(new Color(0.8f, 0.6f, 0.4f)), default(Vector3), boat);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.5f, 0.3f, 0.6f), new Vector3(0.06f, 1f, 0.06f), PatchworkAssets.Wood(new Color(0.8f, 0.6f, 0.4f)), new Vector3(70f, 20f, 0f), boat);
            var bob = boat.gameObject.AddComponent<Spinner>();
            bob.Speed = 0f;
            bob.BobHeight = 0.05f;
            bob.BobSpeed = 1.3f;
            for (int i = 0; i < 4; i++)
            {
                Vector3 p = new Vector3(40.6f, 0.05f, -26f + i * 4.5f + (i > 1 ? 30f : 0f));
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.35f, new Vector3(0.35f, 0.35f, 0.35f), PatchworkAssets.Plain(UIKit.Ink, 0.5f));
                kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 0.7f, new Vector3(0.45f, 0.25f, 0.45f), PatchworkAssets.Plain(UIKit.Ink, 0.5f));
            }
            kit.Rope(new Vector3(40.6f, 0.6f, -21.5f), new Vector3(38.4f, -1f, -19f), PatchworkAssets.Yarn(LevelKit.Palette.Cream), 0.04f);
        }

        static void RavineDressing(LevelKit kit)
        {
            float bed = -5f, x0 = (RavineEast + RavineWest) * 0.5f;
            var rock = PatchworkAssets.Cobble(new Color(0.68f, 0.66f, 0.62f));
            var rocks = new MeshBatch();
            var rng = new System.Random(17);
            for (int i = 0; i < 60; i++)
            {
                float z = -86f + i * 2.9f + (float)rng.NextDouble();
                float side = rng.NextDouble() < 0.5 ? -1f : 1f;
                float x = x0 + side * (2.6f + (float)rng.NextDouble() * 3.6f);
                float s = 0.4f + (float)rng.NextDouble() * 0.9f;
                rocks.Ball(new Vector3(x, bed + s * 0.25f, z), new Vector3(s * 1.4f, s * 0.8f, s), 3, 6);
                if (i % 2 == 0)
                    for (int r = 0; r < 4; r++)
                        reedBatch.Blade(new Vector3(x + 0.5f, bed, z + r * 0.25f), 1.2f + r * 0.2f, 0.05f, new Vector3(0.1f, 0f, 0.05f * r), r * 50f);
            }
            rocks.Build(kit.Group("RavineRocks", Vector3.zero), "Rocks", rock, true);
            // A brick culvert at the north end where the stream comes in through the toybox wall.
            var brick = PatchworkAssets.Brick(new Color(0.7f, 0.45f, 0.38f));
            // (It sits over the deep stream channel, with the water pouring out at the surface.)
            kit.Deco(PrimitiveType.Cylinder, new Vector3(x0, StreamSurface + 0.6f, Half - 0.2f), new Vector3(4.6f, 0.3f, 4.6f), brick, new Vector3(90f, 0f, 0f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(x0, StreamSurface + 0.6f, Half - 0.55f), new Vector3(3.6f, 0.05f, 3.6f), PatchworkAssets.Plain(new Color(0.12f, 0.1f, 0.12f), 0.2f), new Vector3(90f, 0f, 0f));
            var fall = kit.Deco(PrimitiveType.Cube, new Vector3(x0, StreamSurface + 0.25f, Half - 0.9f), new Vector3(3.2f, 0.9f, 0.08f), PatchworkAssets.Bubble(new Color(0.65f, 0.85f, 1f, 0.6f)), new Vector3(-35f, 0f, 0f));
            fall.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // ---------------------------------------------------------------- Spindle Hill

        static void HillDressing(LevelKit kit)
        {
            // A fenced vegetable patch by the cottage, with a little scarecrow.
            Vector3 g0 = new Vector3(-76.5f, 0f, -14.5f), g1 = new Vector3(-70.5f, 0f, -7.5f);
            Pickets(kit, g0, new Vector3(g1.x, 0f, g0.z));
            Pickets(kit, new Vector3(g0.x, 0f, g1.z), g1);
            Pickets(kit, g0, new Vector3(g0.x, 0f, g1.z));
            Pickets(kit, new Vector3(g1.x, 0f, g0.z), new Vector3(g1.x, 0f, -10f));
            var soil = PatchworkAssets.Felt(new Color(0.42f, 0.3f, 0.22f));
            for (int row = 0; row < 3; row++)
            {
                float z = -13f + row * 2f;
                kit.Deco(PrimitiveType.Cube, new Vector3(-73.5f, 0.06f, z), new Vector3(5.2f, 0.12f, 1f), soil);
                ClearGrass(new Vector3(-73.5f, 0f, z), 1.2f);
                for (int i = 0; i < 5; i++)
                {
                    Vector3 p = new Vector3(-75.6f + i * 1.05f, 0.12f, z);
                    if (row == 0) kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 0.25f, new Vector3(0.6f, 0.45f, 0.6f), PatchworkAssets.Knit(new Color(0.55f, 0.75f, 0.45f)));
                    else if (row == 1)
                    {
                        kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 0.05f, new Vector3(0.25f, 0.15f, 0.25f), PatchworkAssets.Felt(new Color(0.98f, 0.55f, 0.2f)));
                        for (int l = 0; l < 3; l++) reedBatch.Blade(p, 0.45f, 0.05f, new Vector3((l - 1) * 0.12f, 0f, 0.05f), l * 60f);
                    }
                    else kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 0.2f, new Vector3(0.32f, 0.36f, 0.32f), PatchworkAssets.Felt(new Color(0.85f, 0.2f, 0.25f)));
                }
            }
            var crow = kit.Group("Scarecrow", new Vector3(-71.6f, 0f, -8.6f));
            Solid(kit, new Vector3(-71.6f, 1.3f, -8.6f), new Vector3(0.4f, 2.6f, 0.4f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.2f, 0f), new Vector3(0.1f, 1.2f, 0.1f), PatchworkAssets.Wood(TownKit.DarkWood), default(Vector3), crow);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.8f, 0f), new Vector3(0.08f, 0.8f, 0.08f), PatchworkAssets.Wood(TownKit.DarkWood), new Vector3(0f, 0f, 90f), crow);
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 1.6f, 0f), new Vector3(0.6f, 0.5f, 0.4f), PatchworkAssets.Fabric(new Color(0.45f, 0.55f, 0.75f)), default(Vector3), crow);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 2.35f, 0f), Vector3.one * 0.45f, PatchworkAssets.Felt(new Color(0.95f, 0.85f, 0.6f)), default(Vector3), crow);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 2.62f, 0f), new Vector3(0.7f, 0.03f, 0.7f), PatchworkAssets.Felt(new Color(0.75f, 0.6f, 0.35f)), default(Vector3), crow);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 2.75f, 0f), new Vector3(0.35f, 0.12f, 0.35f), PatchworkAssets.Felt(new Color(0.75f, 0.6f, 0.35f)), default(Vector3), crow);
            var sway = crow.gameObject.AddComponent<Rocker>();
            sway.Axis = Vector3.forward;
            sway.Amount = 2f;

            // Stepping-stone paths: from the swing to the cottage door and to the bounce pad.
            var stones = new MeshBatch();
            System.Action<Vector3, Vector3> path = (a, b) =>
            {
                int n = Mathf.RoundToInt(Vector3.Distance(a, b) / 1.1f);
                for (int i = 0; i <= n; i++)
                {
                    Vector3 p = Vector3.Lerp(a, b, i / (float)n) + new Vector3(Mathf.Sin(i * 1.9f) * 0.3f, 0f, Mathf.Cos(i * 1.3f) * 0.25f);
                    stones.Cylinder(p + Vector3.down * 0.04f, 0.38f + (i % 3) * 0.06f, 0.08f, 7);
                    ClearGrass(p, 0.5f);
                }
            };
            path(new Vector3(-53f, 0f, 1.5f), new Vector3(-63f, 0f, -6.8f));
            path(new Vector3(-53f, 0f, 1.5f), new Vector3(-65f, 0f, 1.5f));
            stones.Build(kit.Group("HillPath", Vector3.zero), "Stones", PatchworkAssets.Cobble(new Color(0.82f, 0.8f, 0.76f)), false);
            // Hay bales of yarn and a wheelbarrow.
            kit.YarnBall(new Vector3(-58f, 0f, -26f), 1.2f, new Color(0.95f, 0.82f, 0.45f));
            kit.YarnBall(new Vector3(-56.6f, 0f, -27.4f), 0.9f, new Color(0.95f, 0.82f, 0.45f));
            ClearGrass(new Vector3(-58f, 0f, -18f), 2f);
            Barrel(kit, new Vector3(-68.8f, 0f, -7.2f), 0f);
            FlowerPot(kit, new Vector3(-62.5f, 0f, -7.4f), LevelKit.Palette.Lavender);
            FlowerPot(kit, new Vector3(-65.5f, 0f, -7.4f), LevelKit.Palette.Butter);
            // Pompom trees around the hill.
            Color[] leaves = { new Color(0.55f, 0.75f, 0.45f), new Color(0.95f, 0.75f, 0.6f), new Color(0.75f, 0.65f, 0.9f) };
            Vector3[] trees = { new Vector3(-56f, 0f, 30f), new Vector3(-84f, 0f, 40f), new Vector3(-78f, 0f, 52f), new Vector3(-56f, 0f, 50f), new Vector3(-86f, 0f, -36f), new Vector3(-54f, 0f, -40f), new Vector3(-80f, 0f, -86f), new Vector3(-56f, 0f, -84f) };
            for (int i = 0; i < trees.Length; i++) PomTree(kit, trees[i], 5f + (i % 3) * 0.7f, leaves[i % leaves.Length]);
            // Ivy-knit stripes up the terrace and summit walls.
            var ivy = PatchworkAssets.Knit(new Color(0.42f, 0.62f, 0.38f));
            for (int i = 0; i < 6; i++)
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(-73f + i * 2.4f, 1.6f, 3.95f), new Vector3(0.7f, 3.2f - (i % 3) * 0.6f, 0.06f), ivy);
                kit.Deco(PrimitiveType.Cube, new Vector3(-73f + i * 2.4f, 1.4f, 25.95f), new Vector3(0.7f, 2.8f - (i % 2) * 0.8f, 0.06f), ivy);
            }
        }

        // ---------------------------------------------------------------- The park north of the barracks

        static void NorthPark(LevelKit kit)
        {
            // A gingham picnic blanket with a teapot, cups and a basket.
            Vector3 c = new Vector3(-12f, 0f, 72f);
            var gingham = Tiled(PatchworkAssets.Fabric(new Color(0.9f, 0.35f, 0.38f)), 3f, 3f);
            kit.Prism(new[] { new Vector2(c.x - 3f, c.z - 2.2f), new Vector2(c.x + 3f, c.z - 2.4f), new Vector2(c.x + 3.2f, c.z + 2.2f), new Vector2(c.x - 2.9f, c.z + 2.3f) }, -0.05f, 0.03f, gingham, "PicnicBlanket", 1f);
            var check = new MeshBatch();
            for (int i = -2; i <= 2; i++)
            {
                check.Box(new Vector3(c.x + i * 1.2f, 0.035f, c.z), new Vector3(0.35f, 0.01f, 4.4f), Quaternion.identity);
                check.Box(new Vector3(c.x, 0.04f, c.z + i * 0.9f), new Vector3(5.9f, 0.01f, 0.3f), Quaternion.identity);
            }
            check.Build(kit.Root, "Gingham", PatchworkAssets.Fabric(new Color(0.98f, 0.95f, 0.9f)));
            var china = PatchworkAssets.Plain(new Color(0.96f, 0.96f, 0.98f), 0.85f);
            var blue = PatchworkAssets.Plain(new Color(0.35f, 0.5f, 0.85f), 0.8f);
            kit.Deco(PrimitiveType.Sphere, c + new Vector3(0f, 0.45f, 0f), new Vector3(0.9f, 0.75f, 0.9f), china);
            kit.Deco(PrimitiveType.Cylinder, c + new Vector3(0f, 0.45f, 0f), new Vector3(0.92f, 0.04f, 0.92f), blue);
            kit.Deco(PrimitiveType.Sphere, c + new Vector3(0f, 0.86f, 0f), Vector3.one * 0.18f, blue);
            kit.Deco(PrimitiveType.Capsule, c + new Vector3(0.55f, 0.5f, 0f), new Vector3(0.14f, 0.3f, 0.14f), china, new Vector3(0f, 0f, -50f));
            for (int i = 0; i < 4; i++)
            {
                float a = i * 1.57f + 0.6f;
                Vector3 p = c + new Vector3(Mathf.Cos(a) * 1.6f, 0.04f, Mathf.Sin(a) * 1.3f);
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.02f, new Vector3(0.5f, 0.02f, 0.5f), china);
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.15f, new Vector3(0.24f, 0.11f, 0.24f), china);
                kit.Deco(PrimitiveType.Cube, p + new Vector3(0.3f, 0.08f, 0.2f), new Vector3(0.3f, 0.08f, 0.3f), PatchworkAssets.Felt(new Color(0.98f, 0.9f, 0.7f)), new Vector3(0f, 45f, 0f));
            }
            var basket = kit.Group("Basket", c + new Vector3(-2f, 0f, 1.2f));
            Solid(kit, c + new Vector3(-2f, 0.4f, 1.2f), new Vector3(1.2f, 0.8f, 0.8f));
            Solid(kit, c + new Vector3(0f, 0.45f, 0f), new Vector3(0.9f, 0.9f, 0.9f));
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.35f, 0f), new Vector3(1.2f, 0.7f, 0.8f), PatchworkAssets.Wicker(new Color(0.85f, 0.65f, 0.4f)), default(Vector3), basket);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.75f, 0f), new Vector3(1f, 0.04f, 0.5f), PatchworkAssets.Wicker(new Color(0.85f, 0.65f, 0.4f)), new Vector3(90f, 0f, 0f), basket);
            // A pond with stones round it, a little jetty and two ducklings.
            Vector3 pond = new Vector3(14f, 0f, 74f);
            var pondWater = kit.Deco(PrimitiveType.Cylinder, pond + Vector3.up * 0.03f, new Vector3(10f, 0.02f, 8f), PatchworkAssets.CraftWater(0.95f, new Vector2(2.5f, 2f)));
            var ripple = pondWater.gameObject.AddComponent<PaintLake>();
            ripple.Drift = new Vector2(0.02f, 0.01f);
            ripple.Setup(pondWater.GetComponent<Renderer>().sharedMaterial);
            // The stones round the edge keep everyone out of the pond (it's for the ducklings).
            for (int i = 0; i < 16; i++)
            {
                float a0 = i / 16f * Mathf.PI * 2f, a1 = (i + 1) / 16f * Mathf.PI * 2f;
                Vector3 p0 = pond + new Vector3(Mathf.Cos(a0) * 5.1f, 0f, Mathf.Sin(a0) * 4.1f);
                Vector3 p1 = pond + new Vector3(Mathf.Cos(a1) * 5.1f, 0f, Mathf.Sin(a1) * 4.1f);
                Vector3 d = p1 - p0;
                Solid(kit, (p0 + p1) * 0.5f + Vector3.up * 1.1f, new Vector3(0.7f, 2.2f, d.magnitude + 0.1f), Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg);
            }
            var edge = new MeshBatch();
            for (int i = 0; i < 26; i++)
            {
                float a = i / 26f * Mathf.PI * 2f;
                edge.Ball(pond + new Vector3(Mathf.Cos(a) * 5.1f, 0.1f, Mathf.Sin(a) * 4.1f), new Vector3(0.8f, 0.45f, 0.7f), 3, 6);
            }
            edge.Build(kit.Root, "PondStones", PatchworkAssets.Cobble(new Color(0.8f, 0.78f, 0.74f)), true);
            ClearGrass(pond, 5.6f);
            var ducklings = PathMover.Oval(pond + Vector3.up * 0.05f, 3f, 2.2f, 24);
            Duck(kit, ducklings, 0f, 0.5f);
            Duck(kit, ducklings, 1.5f, 0.45f);
            for (int i = 0; i < 4; i++)
            {
                float a = i * 1.4f + 0.3f;
                kit.Deco(PrimitiveType.Cylinder, pond + new Vector3(Mathf.Cos(a) * 2.8f, 0.05f, Mathf.Sin(a) * 2.2f), new Vector3(0.8f, 0.01f, 0.8f), PatchworkAssets.Felt(new Color(0.38f, 0.62f, 0.36f)));
            }
            for (int r = 0; r < 12; r++) reedBatch.Blade(pond + new Vector3(4.2f + (r % 3) * 0.2f, 0f, -1.5f + r * 0.25f), 1.3f + (r % 4) * 0.2f, 0.05f, new Vector3(0.15f, 0f, 0f), r * 30f);
            // A kite flying high on a long string, tied to a peg.
            Vector3 peg = new Vector3(2f, 0f, 66f);
            kit.Deco(PrimitiveType.Cylinder, peg + Vector3.up * 0.25f, new Vector3(0.1f, 0.25f, 0.1f), PatchworkAssets.Wood(TownKit.DarkWood));
            var kiteRoot = kit.Group("Kite", new Vector3(8f, 26f, 82f));
            var kite = new GameObject("KiteBody").transform;
            kite.SetParent(kiteRoot, false);
            kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(2.4f, 2.4f, 0.05f), PatchworkAssets.Fabric(new Color(0.95f, 0.5f, 0.55f)), new Vector3(0f, 0f, 45f), kite);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, -0.04f), new Vector3(1.2f, 1.2f, 0.05f), PatchworkAssets.Fabric(new Color(0.98f, 0.9f, 0.55f)), new Vector3(0f, 0f, 45f), kite);
            for (int i = 0; i < 6; i++)
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Sin(i * 0.9f) * 0.3f, -1.9f - i * 0.6f, 0f), new Vector3(0.4f, 0.2f, 0.05f), PatchworkAssets.Felt(FlowerColors[i % FlowerColors.Length]), default(Vector3), kite);
            var tug = kite.gameObject.AddComponent<Rocker>();
            tug.Axis = Vector3.forward;
            tug.Amount = 12f;
            tug.Speed = 1.1f;
            kit.Rope(peg + Vector3.up * 0.5f, new Vector3(8f, 25.4f, 82f), PatchworkAssets.Yarn(LevelKit.Palette.Cream), 0.025f);
            // Benches and trees round the park.
            TownKit.Bench(kit, new Vector3(4f, 0f, 70f), 180f);
            TownKit.Bench(kit, new Vector3(-24f, 0f, 70f), 90f);
            Color[] leaves = { new Color(0.45f, 0.7f, 0.42f), new Color(0.98f, 0.72f, 0.78f), new Color(0.55f, 0.75f, 0.5f) };
            Vector3[] trees = { new Vector3(-30f, 0f, 64f), new Vector3(-26f, 0f, 84f), new Vector3(-1f, 0f, 84f), new Vector3(24f, 0f, 64f), new Vector3(28f, 0f, 78f), new Vector3(-4f, 0f, 62f) };
            for (int i = 0; i < trees.Length; i++) PomTree(kit, trees[i], 5f + (i % 3) * 0.6f, leaves[i % leaves.Length]);
            TownKit.FlowerBed(kit, new Vector3(-24f, 0f, 80f), new Vector2(3f, 2f), 6);
            ClearGrass(new Vector3(-24f, 0f, 80f), 2f);
        }

        // ---------------------------------------------------------------- The toy train

        static void TrainLoop(LevelKit kit)
        {
            Vector3 center = new Vector3(70f, 0f, -63f);
            var pts = PathMover.Oval(center, 12f, 9f, 72);
            for (int i = 0; i < pts.Length; i++)
            {
                Vector3 a = pts[i], b = pts[(i + 1) % pts.Length];
                Vector3 d = b - a;
                var rot = Quaternion.LookRotation(d.normalized, Vector3.up);
                Vector3 side = rot * Vector3.right;
                Vector3 mid = (a + b) * 0.5f;
                sleeperBatch.Box(mid + Vector3.up * 0.04f, new Vector3(1.7f, 0.08f, 0.32f), rot);
                railBatch.Box(mid + side * 0.55f + Vector3.up * 0.12f, new Vector3(0.1f, 0.1f, d.magnitude + 0.05f), rot);
                railBatch.Box(mid - side * 0.55f + Vector3.up * 0.12f, new Vector3(0.1f, 0.1f, d.magnitude + 0.05f), rot);
                ClearGrass(a, 1.2f);
            }
            // The train: an engine and three wagons.
            var trackPts = new Vector3[pts.Length];
            for (int i = 0; i < pts.Length; i++) trackPts[i] = pts[(pts.Length - i) % pts.Length] + Vector3.up * 0.17f;
            var engine = kit.Group("Engine", trackPts[0]);
            BuildEngine(kit, engine);
            engine.gameObject.AddComponent<PathMover>().Setup(trackPts, 3f, 0f);
            Bumper.Add(engine, new Vector3(1.6f, 2.2f, 3.6f), new Vector3(0f, 1.25f, 0.1f));
            Color[] wagonCols = { new Color(0.35f, 0.55f, 0.85f), new Color(0.95f, 0.75f, 0.3f), new Color(0.45f, 0.7f, 0.45f) };
            for (int i = 0; i < 3; i++)
            {
                var w = kit.Group("Wagon", trackPts[0]);
                BuildWagon(kit, w, wagonCols[i], i);
                w.gameObject.AddComponent<PathMover>().Setup(trackPts, 3f, -3.3f - i * 2.9f);
                Bumper.Add(w, new Vector3(1.5f, 1.1f, 2.4f), new Vector3(0f, 0.75f, 0f));
            }
            // A station with a roof on posts, a bench, a clock and a stationmaster.
            Vector3 st = new Vector3(70f, 0f, -51.2f);
            var planks = PatchworkAssets.Planks(new Color(0.7f, 0.52f, 0.38f));
            kit.Block(st + new Vector3(0f, 0.2f, 0f), new Vector3(10f, 0.4f, 2.4f), new Color(0.7f, 0.52f, 0.38f), "Platform", planks);
            for (int i = -2; i <= 2; i++)
            {
                kit.Deco(PrimitiveType.Cylinder, st + new Vector3(i * 2.3f, 1.8f, 0.9f), new Vector3(0.14f, 1.4f, 0.14f), PatchworkAssets.Plain(new Color(0.25f, 0.4f, 0.3f), 0.5f));
                Solid(kit, st + new Vector3(i * 2.3f, 1.8f, 0.9f), new Vector3(0.25f, 2.8f, 0.25f));
            }
            TownKit.GableRoof(kit, st + new Vector3(0f, 3.2f, 0.4f), 11f, 3.4f, 1f, 0f, PatchworkAssets.Shingle(new Color(0.3f, 0.45f, 0.35f)), planks);
            TownKit.Bench(kit, st + new Vector3(-2.5f, 0.4f, 0.6f), 180f);
            kit.Deco(PrimitiveType.Cylinder, st + new Vector3(2.3f, 2.6f, -0.4f), new Vector3(0.7f, 0.06f, 0.7f), PatchworkAssets.Felt(LevelKit.Palette.Cream), new Vector3(90f, 0f, 0f));
            kit.Deco(PrimitiveType.Cylinder, st + new Vector3(2.3f, 2.6f, -0.36f), new Vector3(0.8f, 0.05f, 0.8f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f), new Vector3(90f, 0f, 0f));
            var master = Toy(kit, st + new Vector3(1.2f, 0.4f, 0.2f), 0f, ToyNpc.Kind.TinSoldier, "Stationmaster Cogg", new Color(0.25f, 0.4f, 0.3f), LevelKit.Palette.Gold, ToyNpc.Instrument.None, 0.9f, new Color(0.25f, 0.4f, 0.3f));
            master.Lines = () => new[] { "All aboard the Toybox Express! Next stop... this stop. Then this stop again.", "She goes round and round, never late once in forty years.",
                                         "If you're hunting golden buttons, look high and low. One hums a tune, they say. Up on the rooftops, I'd wager." };
            // A cardboard tunnel at the far end.
            var card = new Color(0.78f, 0.62f, 0.45f);
            Vector3 tun = center + new Vector3(12f, 0f, 0f);
            // (The track runs north-south here, so the tunnel does too.)
            kit.Cardboard(tun + new Vector3(1.45f, 1.4f, 0f), new Vector3(0.4f, 2.8f, 4.6f), card, "TunnelSide");
            kit.Cardboard(tun + new Vector3(-1.45f, 1.4f, 0f), new Vector3(0.4f, 2.8f, 4.6f), card, "TunnelSide");
            kit.Cardboard(tun + new Vector3(0f, 2.95f, 0f), new Vector3(3.5f, 0.3f, 4.8f), card, "TunnelTop");
            kit.Tape(tun + new Vector3(0f, 3.11f, 0f), new Vector3(3.6f, 0.04f, 0.6f));
            ClearGrass(tun, 2.6f);
            AmbientEmitter.Create(engine, engine.position + Vector3.up, Sfx.Ambient(Sfx.AmbientClip.Clock), 0.35f, 2f, 12f);
        }

        static void BuildEngine(LevelKit kit, Transform e)
        {
            var red = PatchworkAssets.Plain(new Color(0.85f, 0.2f, 0.22f), 0.7f);
            var black = PatchworkAssets.Plain(new Color(0.15f, 0.14f, 0.16f), 0.6f);
            var gold = PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f);
            var green = PatchworkAssets.Plain(new Color(0.25f, 0.5f, 0.35f), 0.7f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.45f, 0f), new Vector3(1.4f, 0.3f, 3.2f), black, default(Vector3), e);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.15f, 0.5f), new Vector3(1.1f, 1f, 1.1f), red, new Vector3(90f, 0f, 0f), e);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.15f, 1.52f), new Vector3(1f, 0.04f, 1f), gold, new Vector3(90f, 0f, 0f), e);
            for (int i = 0; i < 3; i++) kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.15f, -0.2f + i * 0.6f), new Vector3(1.14f, 0.04f, 1.14f), gold, new Vector3(90f, 0f, 0f), e);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 2f, 1.1f), new Vector3(0.36f, 0.4f, 0.36f), black, default(Vector3), e);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 2.42f, 1.1f), new Vector3(0.55f, 0.08f, 0.55f), black, default(Vector3), e);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.75f, 0.2f), new Vector3(0.5f, 0.45f, 0.5f), gold, default(Vector3), e);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.35f, -1f), new Vector3(1.4f, 1.5f, 1.1f), green, default(Vector3), e);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 2.15f, -1f), new Vector3(1.6f, 0.12f, 1.3f), black, default(Vector3), e);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.71f, 1.5f, -1f), new Vector3(0.02f, 0.5f, 0.6f), PatchworkAssets.Glow(new Color(1f, 0.86f, 0.6f), 0.5f), default(Vector3), e);
                for (int w = 0; w < 3; w++)
                {
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.72f, 0.38f, -1f + w * 1f), new Vector3(0.7f, 0.06f, 0.7f), red, new Vector3(0f, 0f, 90f), e);
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.76f, 0.38f, -1f + w * 1f), new Vector3(0.25f, 0.04f, 0.25f), gold, new Vector3(0f, 0f, 90f), e);
                }
            }
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.35f, 1.75f), new Vector3(1.3f, 0.4f, 0.4f), red, new Vector3(30f, 0f, 0f), e);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.15f, 1.6f), Vector3.one * 0.3f, PatchworkAssets.Glow(new Color(1f, 0.9f, 0.6f), 1.5f), default(Vector3), e);
            // Puffs of smoke trailing up and back from the chimney.
            for (int i = 0; i < 3; i++)
            {
                float s = 0.35f + i * 0.15f;
                var puff = kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 2.75f + i * 0.45f, 0.95f - i * 0.45f), Vector3.one * s, PatchworkAssets.Felt(new Color(0.95f, 0.94f, 0.92f)), default(Vector3), e);
                var bob = puff.gameObject.AddComponent<Spinner>();
                bob.Speed = 40f + i * 15f;
                bob.BobHeight = 0.1f + i * 0.05f;
                bob.BobSpeed = 4f - i * 0.8f;
            }
        }

        static void BuildWagon(LevelKit kit, Transform w, Color col, int cargo)
        {
            var black = PatchworkAssets.Plain(new Color(0.15f, 0.14f, 0.16f), 0.6f);
            var paint = PatchworkAssets.Plain(col, 0.65f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.45f, 0f), new Vector3(1.3f, 0.25f, 2.2f), black, default(Vector3), w);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.85f, 0f), new Vector3(1.4f, 0.6f, 2.3f), paint, default(Vector3), w);
            for (int s = -1; s <= 1; s += 2)
                for (int i = 0; i < 2; i++)
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.7f, 0.36f, -0.65f + i * 1.3f), new Vector3(0.6f, 0.05f, 0.6f), black, new Vector3(0f, 0f, 90f), w);
            if (cargo == 0)
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.5f, -0.4f), new Vector3(0.8f, 0.8f, 0.8f), PatchworkAssets.Wood(new Color(0.85f, 0.35f, 0.35f)), new Vector3(0f, 20f, 0f), w);
                kit.Deco(PrimitiveType.Cube, new Vector3(0.1f, 1.4f, 0.5f), new Vector3(0.7f, 0.7f, 0.7f), PatchworkAssets.Wood(new Color(0.95f, 0.8f, 0.35f)), new Vector3(0f, -15f, 0f), w);
            }
            else if (cargo == 1)
            {
                for (int i = 0; i < 3; i++)
                    kit.Deco(PrimitiveType.Sphere, new Vector3(-0.3f + (i % 2) * 0.6f, 1.35f + (i / 2) * 0.4f, -0.5f + i * 0.5f), Vector3.one * 0.6f, PatchworkAssets.Yarn(FlowerColors[i]), default(Vector3), w);
            }
            else
            {
                // A teddy passenger enjoying the ride.
                var fur = PatchworkAssets.Felt(new Color(0.75f, 0.55f, 0.35f));
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.4f, 0f), new Vector3(0.7f, 0.75f, 0.6f), fur, default(Vector3), w);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 2f, 0.05f), Vector3.one * 0.55f, fur, default(Vector3), w);
                for (int s = -1; s <= 1; s += 2)
                {
                    kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.22f, 2.25f, 0f), Vector3.one * 0.2f, fur, default(Vector3), w);
                    kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.1f, 2.05f, 0.27f), Vector3.one * 0.07f, PatchworkAssets.Plain(UIKit.Ink, 0.7f), default(Vector3), w);
                }
                var wave = kit.Deco(PrimitiveType.Capsule, new Vector3(0.4f, 1.8f, 0f), new Vector3(0.18f, 0.3f, 0.18f), fur, new Vector3(0f, 0f, -30f), w);
                var rock = wave.gameObject.AddComponent<Rocker>();
                rock.Axis = Vector3.forward;
                rock.Amount = 25f;
                rock.Speed = 5f;
            }
        }

        // ---------------------------------------------------------------- The Dye Works' yard

        static void MillDressing(LevelKit kit)
        {
            Vector3 o = Mill;
            // Lines of freshly dyed cloth drying behind the works.
            TownKit.Clothesline(kit, o + new Vector3(-6f, 0f, 9f), o + new Vector3(6f, 0f, 9f), 3.2f, 0.5f, new float[0]);
            TownKit.Clothesline(kit, o + new Vector3(-6f, 0f, 13f), o + new Vector3(6f, 0f, 13f), 3.2f, 0.5f, new float[0]);
            // Dye vats in the yard.
            Color[] dyes = { new Color(0.85f, 0.3f, 0.45f), new Color(0.3f, 0.55f, 0.9f), new Color(0.95f, 0.8f, 0.3f), new Color(0.5f, 0.75f, 0.4f) };
            var copper = PatchworkAssets.Plain(new Color(0.78f, 0.48f, 0.3f), 0.75f);
            for (int i = 0; i < 4; i++)
            {
                Vector3 p = o + new Vector3(10f, 0f, -4f + i * 3f);
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.55f, new Vector3(2f, 0.55f, 2f), copper);
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 1.11f, new Vector3(1.8f, 0.01f, 1.8f), PatchworkAssets.Water(dyes[i]));
                Solid(kit, p + Vector3.up * 0.55f, new Vector3(1.9f, 1.1f, 1.9f));
                ClearGrass(p, 1.2f);
            }
            Barrel(kit, o + new Vector3(-8.5f, 0f, -7.5f), 10f);
            Barrel(kit, o + new Vector3(-7.4f, 0f, -8.2f), 50f);
            // Inside: three small vats of dye on the ground floor.
            for (int i = 0; i < 3; i++)
            {
                Vector3 p = o + new Vector3(-2.6f + i * 1.4f, 0f, 5.2f - (i % 2) * 0.3f);
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.4f, new Vector3(1.1f, 0.4f, 1.1f), copper);
                Solid(kit, p + Vector3.up * 0.4f, new Vector3(1f, 0.8f, 1f));
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.81f, new Vector3(0.95f, 0.01f, 0.95f), PatchworkAssets.Water(dyes[i + 1]));
            }
        }

        // ---------------------------------------------------------------- Sky

        static void SkyDressing(LevelKit kit)
        {
            // A patchwork hot air balloon drifting slowly round the town.
            var balloon = kit.Group("Balloon", new Vector3(0f, 38f, 0f));
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 3.6f, 0f), new Vector3(7f, 8f, 7f), PatchworkAssets.BalloonQuilt(), default(Vector3), balloon);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -1.6f, 0f), new Vector3(1.6f, 1.2f, 1.6f), PatchworkAssets.Wicker(new Color(0.8f, 0.6f, 0.38f)), default(Vector3), balloon);
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    kit.Rope(new Vector3(sx * 0.75f, -1f, sz * 0.75f), new Vector3(sx * 1.6f, 0.6f, sz * 1.6f), PatchworkAssets.Yarn(LevelKit.Palette.Cream), 0.03f);
            var drift = balloon.gameObject.AddComponent<PathMover>();
            drift.BobHeight = 0.8f;
            drift.BobSpeed = 0.3f;
            drift.Setup(PathMover.Oval(new Vector3(0f, 40f, 0f), 62f, 55f, 40), 2.2f, 0f);
            // The cot mobile hanging over the toybox: felt moon, stars and clouds turning slowly.
            var mobile = kit.Group("Mobile", new Vector3(10f, 70f, -10f));
            var wood = PatchworkAssets.Wood(new Color(0.9f, 0.85f, 0.75f));
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.6f, 24f, 0.6f), wood, new Vector3(0f, 0f, 90f), mobile);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.6f, 24f, 0.6f), wood, new Vector3(90f, 0f, 0f), mobile);
            kit.Rope(new Vector3(10f, 70f, -10f), new Vector3(10f, 160f, -10f), PatchworkAssets.Yarn(LevelKit.Palette.Cream), 0.15f);
            var felt = PatchworkAssets.Felt(new Color(0.98f, 0.88f, 0.5f));
            for (int i = 0; i < 4; i++)
            {
                float a = i * 90f * Mathf.Deg2Rad;
                Vector3 end = new Vector3(Mathf.Cos(a) * 23f, 0f, Mathf.Sin(a) * 23f);
                float drop = 8f + (i % 2) * 5f;
                kit.Deco(PrimitiveType.Cylinder, end + Vector3.down * drop * 0.5f, new Vector3(0.12f, drop * 0.5f, 0.12f), PatchworkAssets.Yarn(LevelKit.Palette.Cream), default(Vector3), mobile);
                var charm = new GameObject("Charm").transform;
                charm.SetParent(mobile, false);
                charm.localPosition = end + Vector3.down * (drop + 3f);
                if (i == 0)
                {
                    kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(6f, 6f, 1.2f), felt, default(Vector3), charm);
                    kit.Deco(PrimitiveType.Sphere, new Vector3(1.8f, 0.6f, 0f), new Vector3(5f, 5.4f, 1.4f), PatchworkAssets.Felt(new Color(0.55f, 0.65f, 0.85f)), default(Vector3), charm);
                }
                else if (i == 2) { var c = kit.Cloud(Vector3.zero, 2.4f); c.SetParent(charm, false); c.localPosition = Vector3.zero; }
                else
                    for (int k = 0; k < 2; k++)
                        kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(4f, 4f, 1f), PatchworkAssets.Felt(i == 1 ? new Color(0.98f, 0.88f, 0.5f) : new Color(0.95f, 0.65f, 0.75f)), new Vector3(0f, 0f, k * 45f), charm);
                var turn = charm.gameObject.AddComponent<FlagWave>();
                turn.Amount = 30f;
                turn.Speed = 0.4f;
            }
            var spin = mobile.gameObject.AddComponent<Spinner>();
            spin.Speed = 5f;
        }

        // ---------------------------------------------------------------- Toys to play with

        static void ToyFun(LevelKit kit)
        {
            XylophoneBridge(kit, 60f);
            // Toy drums to bounce on: up the alphabet blocks, onto the bandstand roof, up to the rooftops.
            kit.AddDrumPad(new Vector3(-7.5f, 0.06f, 9.5f), 16f, new Color(0.85f, 0.25f, 0.3f));
            // An arc of threads over the bandstand's roof (clear of its tiers).
            kit.ThreadLine(new Vector3(-3.6f, 6f, 7f), new Vector3(-3.6f, 6f, 7f), 1);
            kit.ThreadLine(new Vector3(0f, 7f, 7f), new Vector3(0f, 7f, 7f), 1);
            kit.ThreadLine(new Vector3(3.6f, 6f, 7f), new Vector3(3.6f, 6f, 7f), 1);
            kit.AddDrumPad(new Vector3(77f, 0f, 72f), 16f, new Color(0.3f, 0.5f, 0.85f));
            kit.AddDrumPad(new Vector3(83.6f, 5f, 78f), 16f, new Color(0.95f, 0.75f, 0.3f));
            kit.AddDrumPad(new Vector3(-77f, 0f, -68.5f), 21f, new Color(0.45f, 0.7f, 0.45f));
            kit.AddDrumPad(new Vector3(-8.5f, 0f, -38f), 21f, new Color(0.75f, 0.5f, 0.85f));
            kit.Hint(new Vector3(-7.5f, 1.5f, 9.5f), new Vector3(3f, 3f, 3f), "Toy Drums",
                "Jump on a toy drum to bounce up high. There are drums all over town. Where do they lead?");
            ClearGrass(new Vector3(77f, 0f, 72f), 1.4f);
            ClearGrass(new Vector3(-77f, 0f, -68.5f), 1.4f);

            // Wind-up mice scurrying round the bandstand, the pond and the cottage.
            var round = PathMover.Oval(new Vector3(0f, 0.06f, 7f), 7f, 7.6f, 32);
            WindupMouse(kit, round, 0f, new Color(0.95f, 0.75f, 0.8f));
            WindupMouse(kit, round, 25f, new Color(0.75f, 0.8f, 0.95f));
            WindupMouse(kit, PathMover.Oval(new Vector3(14f, 0f, 74f), 6.6f, 5.6f, 28), 0f, new Color(0.98f, 0.9f, 0.6f));
            WindupMouse(kit, new[] { new Vector3(-69.6f, 0f, -16.8f), new Vector3(-58.4f, 0f, -16.8f), new Vector3(-58.4f, 0f, -5.5f), new Vector3(-69.6f, 0f, -5.5f) }, 0f, new Color(0.8f, 0.95f, 0.8f));
            // Tin soldiers drilling round the barracks yard.
            var drill = new[] { new Vector3(-9f, 0.05f, 37f), new Vector3(-9f, 0.05f, 54f), new Vector3(9f, 0.05f, 54f), new Vector3(9f, 0.05f, 37f) };
            for (int i = 0; i < 4; i++) MarchingSoldier(kit, drill, -i * 2.2f, i % 2 == 0 ? new Color(0.8f, 0.22f, 0.25f) : new Color(0.25f, 0.35f, 0.7f));
            Carousel(kit, new Vector3(-22f, 0f, 63f));

            DominoRun(kit);
            BubbleWand(kit, new Vector3(4f, 0f, 80f));
            // Wind chimes: on the bandstand, along the rooftop glide and on the spire.
            WindChimes(kit, Bandstand + new Vector3(4.6f, 3.9f, 0f), false);
            WindChimes(kit, Bandstand + new Vector3(-4.6f, 3.9f, 0f), false);
            WindChimes(kit, new Vector3(-3f, 11.3f, -46f), true);
            WindChimes(kit, new Vector3(3f, 10.3f, -58f), true);
            WindChimes(kit, new Vector3(2.4f, 14.6f, -68.8f), false);
            WindChimes(kit, new Vector3(52f, 9.4f, -26f), true);

            var pins = kit.Info.MapPins;
            pins.Add(new MapPin("Xylophone Bridge", new Vector3((CanalWest + CanalEast) * 0.5f, 0f, 60f)));
            pins.Add(new MapPin("Carousel", new Vector3(-22f, 0f, 63f)));
            pins.Add(new MapPin("Domino Run", new Vector3(24f, 0f, 44f)));
            pins.Add(new MapPin("Bubble Wand", new Vector3(4f, 0f, 80f)));
        }

        // Giant dominoes in a wiggly line up the east side of town. Walk into the first one to
        // knock them all down; the last one rings a bell and some threads appear.
        static void DominoRun(LevelKit kit)
        {
            const int count = 14;
            var pivots = new Transform[count];
            var white = PatchworkAssets.Plain(new Color(0.97f, 0.95f, 0.9f), 0.7f);
            var pip = PatchworkAssets.Plain(new Color(0.12f, 0.1f, 0.14f), 0.6f);
            Vector3 Pt(int i) => new Vector3(24f + Mathf.Sin(i * 0.45f) * 2.4f, 0f, 32f + i * 1.6f);
            for (int i = 0; i < count; i++)
            {
                Vector3 p = Pt(i), next = Pt(i + 1);
                var dir = (next - p).normalized;
                var pivot = kit.Group("Domino", p);
                pivot.rotation = Quaternion.LookRotation(dir, Vector3.up);
                var body = kit.Block(Vector3.zero, new Vector3(1.3f, 2.6f, 0.4f), Color.white, "Domino", white);
                body.SetParent(pivot, false);
                body.localPosition = new Vector3(0f, 1.3f, -0.2f);
                body.localRotation = Quaternion.identity;
                body.localScale = new Vector3(1.3f, 2.6f, 0.4f);
                // A line across the middle and pips on the face you walk up to.
                var line = kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.3f, -0.42f), new Vector3(1.1f, 0.06f, 0.04f), pip, default(Vector3), pivot);
                line.name = "Line";
                int top = 1 + i % 6, bottom = 1 + (i * 2 + 3) % 6;
                Pips(kit, pivot, top, 1.95f, pip);
                Pips(kit, pivot, bottom, 0.65f, pip);
                pivots[i] = pivot;
                ClearGrass(p, 0.9f);
            }
            // The bell at the end of the line.
            Vector3 end = Pt(count) + new Vector3(0f, 0f, 0.8f);
            kit.Deco(PrimitiveType.Cylinder, end + Vector3.up * 1.3f, new Vector3(0.18f, 1.3f, 0.18f), PatchworkAssets.Wood(TownKit.DarkWood));
            kit.Deco(PrimitiveType.Cube, end + Vector3.up * 2.6f, new Vector3(1.4f, 0.14f, 0.14f), PatchworkAssets.Wood(TownKit.DarkWood));
            var bell = kit.Group("DominoBell", end + Vector3.up * 2.55f);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.4f, 0f), new Vector3(0.7f, 0.8f, 0.7f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.85f), default(Vector3), bell);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.78f, 0f), new Vector3(0.8f, 0.04f, 0.8f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.85f), default(Vector3), bell);
            Solid(kit, end + Vector3.up * 1.3f, new Vector3(0.3f, 2.6f, 0.3f));
            var reward = new List<ThreadSpool>();
            for (int i = 0; i < 6; i++)
            {
                var th = kit.Thread(Pt(i * 2) + Vector3.up * 3.6f, LevelKit.Palette.Lavender);
                th.gameObject.SetActive(false);
                reward.Add(th);
            }
            Vector3 start = Pt(0) - (Pt(1) - Pt(0)).normalized * 0.9f;
            var run = kit.Volume<DominoRun>(start + Vector3.up * 1.2f, new Vector3(2f, 2.4f, 1.6f), "DominoStart");
            run.Setup(pivots, bell);
            run.Finished = () =>
            {
                foreach (var r in reward) if (r != null) r.gameObject.SetActive(true);
                var game = PatchworkGame.Instance;
                if (game != null) game.ShowToast("Ding! Threads tumbled out of the dominoes.");
            };
            kit.Hint(start + Vector3.up * 1.5f + new Vector3(0f, 0f, -3f), new Vector3(4f, 3f, 3f), "Domino Run", "Give the first domino a nudge and see what happens!");
        }

        static void Pips(LevelKit kit, Transform pivot, int n, float y, Material m)
        {
            Vector2[] spots;
            switch (n)
            {
                case 1: spots = new[] { Vector2.zero }; break;
                case 2: spots = new[] { new Vector2(-0.3f, 0.3f), new Vector2(0.3f, -0.3f) }; break;
                case 3: spots = new[] { new Vector2(-0.3f, 0.3f), Vector2.zero, new Vector2(0.3f, -0.3f) }; break;
                case 4: spots = new[] { new Vector2(-0.3f, 0.3f), new Vector2(0.3f, 0.3f), new Vector2(-0.3f, -0.3f), new Vector2(0.3f, -0.3f) }; break;
                case 5: spots = new[] { new Vector2(-0.3f, 0.3f), new Vector2(0.3f, 0.3f), Vector2.zero, new Vector2(-0.3f, -0.3f), new Vector2(0.3f, -0.3f) }; break;
                default: spots = new[] { new Vector2(-0.3f, 0.35f), new Vector2(0.3f, 0.35f), new Vector2(-0.3f, 0f), new Vector2(0.3f, 0f), new Vector2(-0.3f, -0.35f), new Vector2(0.3f, -0.35f) }; break;
            }
            foreach (var s in spots)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s.x, y + s.y, -0.42f), new Vector3(0.16f, 0.02f, 0.16f), m, new Vector3(90f, 0f, 0f), pivot);
        }

        // A giant bubble wand on a stand by a big bottle of bubble mix. It keeps blowing bubbles
        // that float up; hop from bubble to bubble to reach the sponge up high.
        static void BubbleWand(LevelKit kit, Vector3 feet)
        {
            var wood = PatchworkAssets.Wood(new Color(0.85f, 0.55f, 0.65f));
            kit.Deco(PrimitiveType.Cylinder, feet + new Vector3(0f, 1.2f, 0f), new Vector3(0.2f, 1.2f, 0.2f), wood);
            kit.Deco(PrimitiveType.Cylinder, feet + new Vector3(0f, 0.08f, 0f), new Vector3(1.2f, 0.08f, 1.2f), wood);
            Solid(kit, feet + Vector3.up * 1.2f, new Vector3(0.35f, 2.4f, 0.35f));
            Vector3 ring = feet + new Vector3(0f, 2.9f, 0f);
            var loop = PatchworkAssets.Plain(new Color(0.98f, 0.5f, 0.7f), 0.7f);
            for (int i = 0; i < 20; i++)
            {
                float a = i / 20f * Mathf.PI * 2f;
                kit.Deco(PrimitiveType.Sphere, ring + new Vector3(Mathf.Cos(a) * 0.7f, Mathf.Sin(a) * 0.7f, 0f), Vector3.one * 0.18f, loop);
            }
            kit.Deco(PrimitiveType.Cylinder, ring + new Vector3(0f, 0f, 0.02f), new Vector3(1.3f, 0.005f, 1.3f), PatchworkAssets.Bubble(new Color(0.85f, 0.9f, 1f, 0.3f)), new Vector3(90f, 0f, 0f));
            // The bottle of bubble mix.
            Vector3 bottle = feet + new Vector3(-1.8f, 0f, 0.6f);
            kit.Deco(PrimitiveType.Cylinder, bottle + Vector3.up * 0.9f, new Vector3(1f, 0.9f, 1f), PatchworkAssets.Bubble(new Color(0.8f, 0.9f, 1f, 0.45f)));
            kit.Deco(PrimitiveType.Cylinder, bottle + Vector3.up * 0.7f, new Vector3(0.9f, 0.68f, 0.9f), PatchworkAssets.Glow(new Color(1f, 0.6f, 0.8f), 0.25f));
            kit.Deco(PrimitiveType.Cylinder, bottle + Vector3.up * 1.95f, new Vector3(0.6f, 0.15f, 0.6f), PatchworkAssets.Plain(new Color(0.4f, 0.75f, 0.95f), 0.6f));
            Solid(kit, bottle + Vector3.up * 1f, new Vector3(1f, 2f, 1f));
            ClearGrass(feet, 1.4f);
            // The bubbles.
            var blower = kit.Group("BubbleBlower", feet).gameObject.AddComponent<BubbleBlower>();
            var pool = new Transform[9];
            var film = PatchworkAssets.Bubble(new Color(0.8f, 0.9f, 1f, 0.32f));
            var shine = PatchworkAssets.Glow(new Color(1f, 0.85f, 0.95f), 0.7f);
            for (int i = 0; i < pool.Length; i++)
            {
                var b = kit.Volume<BubbleBounce>(ring, new Vector3(1.8f, 1.8f, 1.8f), "Bubble");
                b.Owner = blower;
                kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 1.7f, film, default(Vector3), b.transform).GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                kit.Deco(PrimitiveType.Sphere, new Vector3(-0.38f, 0.42f, -0.4f), new Vector3(0.3f, 0.18f, 0.1f), shine, new Vector3(0f, 0f, 35f), b.transform);
                pool[i] = b.transform;
            }
            blower.Setup(ring, pool);
            // The sponge up top, with a Heartstring on it.
            Vector3 sponge = feet + new Vector3(2.8f, 12.4f, 2f);
            kit.Block(sponge, new Vector3(3f, 1.1f, 2f), new Color(1f, 0.85f, 0.35f), "Sponge", PatchworkAssets.Felt(new Color(1f, 0.85f, 0.35f)));
            for (int i = 0; i < 9; i++)
                kit.Deco(PrimitiveType.Sphere, sponge + new Vector3(-1.2f + (i % 5) * 0.6f, (i % 2 == 0 ? 0.3f : -0.2f), -1.01f), new Vector3(0.22f, 0.18f, 0.04f), PatchworkAssets.Felt(new Color(0.85f, 0.65f, 0.2f)));
            MakeHeartstring(kit, "L1_hs_bubbles", "Bubble Bath", sponge + new Vector3(0f, 1.8f, 0f), false);
            kit.ThreadLine(feet + new Vector3(0.3f, 4.5f, 0.3f), feet + new Vector3(1.8f, 10.5f, 1.4f), 4);
            kit.Hint(feet + new Vector3(0f, 1.5f, -2.5f), new Vector3(4f, 3f, 3f), "Bubble Wand", "Land on a bubble to bounce off it. Hop from bubble to bubble to reach the sponge!");
        }

        // A set of wind chimes on a little bracket. Brush through them to make them ring.
        static void WindChimes(LevelKit kit, Vector3 hang, bool onPost)
        {
            var wood = PatchworkAssets.Wood(new Color(0.6f, 0.42f, 0.3f));
            if (onPost) kit.Deco(PrimitiveType.Cylinder, hang + Vector3.down * 0.9f, new Vector3(0.1f, 0.9f, 0.1f), wood);
            kit.Deco(PrimitiveType.Cylinder, hang + Vector3.up * 0.05f, new Vector3(0.7f, 0.05f, 0.7f), wood);
            var tubes = new Transform[5];
            var metal = PatchworkAssets.Plain(new Color(0.85f, 0.85f, 0.92f), 0.9f);
            var gold = PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.85f);
            for (int i = 0; i < 5; i++)
            {
                float a = i / 5f * Mathf.PI * 2f;
                var pivot = kit.Group("ChimeTube", hang + new Vector3(Mathf.Cos(a) * 0.26f, 0f, Mathf.Sin(a) * 0.26f));
                float len = 0.5f + i * 0.12f;
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -len * 0.5f - 0.1f, 0f), new Vector3(0.07f, len * 0.5f, 0.07f), i % 2 == 0 ? metal : gold, default(Vector3), pivot);
                tubes[i] = pivot;
            }
            var sail = kit.Group("ChimeSail", hang);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -1.25f, 0f), new Vector3(0.22f, 0.3f, 0.02f), PatchworkAssets.Felt(LevelKit.Palette.Rose), default(Vector3), sail);
            var chime = kit.Volume<WindChime>(hang + Vector3.down * 0.6f, new Vector3(2.2f, 2.2f, 2.2f), "WindChime");
            var all = new Transform[6];
            for (int i = 0; i < 5; i++) all[i] = tubes[i];
            all[5] = sail;
            chime.Setup(all);
        }

        // A walk-on xylophone across the canal: run over it to play a scale. Play the whole scale
        // up in one go and some threads appear above it.
        static void XylophoneBridge(LevelKit kit, float z)
        {
            int[] scale = { 0, 2, 4, 5, 7, 9, 11, 12 };
            Color[] cols = { new Color(0.92f, 0.3f, 0.35f), new Color(0.98f, 0.55f, 0.25f), new Color(0.98f, 0.82f, 0.3f), new Color(0.5f, 0.78f, 0.4f),
                             new Color(0.3f, 0.72f, 0.75f), new Color(0.35f, 0.5f, 0.88f), new Color(0.58f, 0.42f, 0.85f), new Color(0.9f, 0.45f, 0.75f) };
            float x0 = CanalWest - 0.4f;
            var frame = PatchworkAssets.Wood(new Color(0.5f, 0.34f, 0.24f));
            for (int s = -1; s <= 1; s += 2)
                kit.Block(new Vector3(x0 + 4.4f, -0.025f, z + s * 1.3f), new Vector3(9.2f, 0.25f, 0.3f), new Color(0.5f, 0.34f, 0.24f), "XyloRail", frame);
            var reward = new List<ThreadSpool>();
            for (int i = 0; i < 6; i++)
            {
                var t = kit.Thread(new Vector3(x0 + 1.2f + i * 1.3f, 2.4f + Mathf.Sin(i / 5f * Mathf.PI) * 1.2f, z), LevelKit.Palette.Rose);
                t.gameObject.SetActive(false);
                reward.Add(t);
            }
            int chain = 0;
            float lastTime = -10f;
            bool paid = false;
            for (int i = 0; i < 8; i++)
            {
                float x = x0 + 0.55f + i * 1.1f;
                float len = 2.9f - i * 0.1f;
                var key = kit.Block(new Vector3(x, 0.17f, z), new Vector3(1f, 0.18f, len), cols[i], "XyloKey", PatchworkAssets.Plain(cols[i], 0.55f));
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.12f, 0.02f, 0.12f), PatchworkAssets.Plain(new Color(0.85f, 0.85f, 0.9f), 0.9f), default(Vector3), key).localPosition = new Vector3(0f, 0.55f, s * 0.43f);
                var vol = kit.Volume<XyloKey>(new Vector3(x, 0.95f, z), new Vector3(1f, 1.4f, len), "XyloNote");
                vol.Setup(key, i, scale[i]);
                vol.Struck = k =>
                {
                    float now = Time.time;
                    chain = (k.Index == 0 || (k.Index == chain && now - lastTime < 1.2f)) ? k.Index + 1 : 0;
                    lastTime = now;
                    if (chain >= 8 && !paid)
                    {
                        paid = true;
                        foreach (var r in reward) if (r != null) r.gameObject.SetActive(true);
                        Sfx.Play(SfxId.Complete, 0.5f);
                        FxBurst.Spawn(FxBurst.Kind.Sparkle, k.transform.position + Vector3.up * 2f, new Color(1f, 0.85f, 0.5f));
                        var game = PatchworkGame.Instance;
                        if (game != null) game.ShowToast("What a tune! Some threads popped out of the xylophone.");
                    }
                };
            }
            // A pair of giant mallets leaning at the end.
            for (int s = -1; s <= 1; s += 2)
            {
                var stick = kit.Deco(PrimitiveType.Cylinder, new Vector3(CanalEast + 1.2f, 1.1f, z + s * 1.9f), new Vector3(0.12f, 1.2f, 0.12f), PatchworkAssets.Wood(new Color(0.85f, 0.7f, 0.5f)), new Vector3(0f, 0f, -20f));
                stick.name = "Mallet";
                kit.Deco(PrimitiveType.Sphere, new Vector3(CanalEast + 1.6f, 2.2f, z + s * 1.9f), Vector3.one * 0.45f, PatchworkAssets.Felt(s < 0 ? new Color(0.92f, 0.3f, 0.35f) : new Color(0.35f, 0.5f, 0.88f)));
            }
            kit.Hint(new Vector3(x0 - 1.5f, 1.5f, z), new Vector3(3f, 3f, 4f), "Xylophone Bridge", "Run across to play a tune. Can you play the whole scale without stopping?");
        }

        static void WindupMouse(LevelKit kit, Vector3[] loop, float start, Color col)
        {
            var m = kit.Group("WindupMouse", loop[0]);
            var fur = PatchworkAssets.Felt(col);
            var pink = PatchworkAssets.Felt(new Color(1f, 0.7f, 0.75f));
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.22f, 0f), new Vector3(0.5f, 0.4f, 0.75f), fur, default(Vector3), m);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.28f, 0.36f), new Vector3(0.32f, 0.3f, 0.36f), fur, default(Vector3), m);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.13f, 0.45f, 0.33f), new Vector3(0.18f, 0.18f, 0.05f), pink, default(Vector3), m);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.07f, 0.33f, 0.52f), Vector3.one * 0.05f, PatchworkAssets.Plain(UIKit.Ink, 0.7f), default(Vector3), m);
            }
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.27f, 0.56f), Vector3.one * 0.07f, pink, default(Vector3), m);
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.18f, -0.5f), new Vector3(0.04f, 0.2f, 0.04f), pink, new Vector3(-70f, 0f, 0f), m);
            var key = new GameObject("Key").transform;
            key.SetParent(m, false);
            key.localPosition = new Vector3(0f, 0.42f, -0.12f);
            var gold = PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.06f, 0f), new Vector3(0.04f, 0.06f, 0.04f), gold, default(Vector3), key);
            kit.Deco(PrimitiveType.Sphere, new Vector3(-0.08f, 0.16f, 0f), new Vector3(0.13f, 0.1f, 0.03f), gold, default(Vector3), key);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.08f, 0.16f, 0f), new Vector3(0.13f, 0.1f, 0.03f), gold, default(Vector3), key);
            var spin = key.gameObject.AddComponent<Spinner>();
            spin.Speed = 220f;
            var mover = m.gameObject.AddComponent<PathMover>();
            mover.BobHeight = 0.03f;
            mover.BobSpeed = 14f;
            mover.Setup(loop, 1.3f, start);
        }

        static void MarchingSoldier(LevelKit kit, Vector3[] loop, float start, Color coat)
        {
            var g = kit.Group("TinSoldier", loop[0]);
            var white = PatchworkAssets.Plain(new Color(0.96f, 0.95f, 0.92f), 0.6f);
            var black = PatchworkAssets.Plain(new Color(0.12f, 0.1f, 0.12f), 0.5f);
            var gold = PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.12f, 0.35f, 0f), new Vector3(0.18f, 0.35f, 0.18f), white, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1f, 0f), new Vector3(0.52f, 0.32f, 0.42f), PatchworkAssets.Plain(coat, 0.6f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1f, 0.2f), new Vector3(0.08f, 0.6f, 0.04f), white, new Vector3(0f, 0f, 35f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.72f, 0f), new Vector3(0.54f, 0.04f, 0.44f), black, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.52f, 0f), Vector3.one * 0.34f, PatchworkAssets.Felt(new Color(0.98f, 0.82f, 0.7f)), default(Vector3), g);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.06f, 1.55f, 0.15f), Vector3.one * 0.05f, black, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.47f, 0.16f), new Vector3(0.12f, 0.08f, 0.04f), PatchworkAssets.Felt(new Color(0.95f, 0.5f, 0.55f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.95f, 0f), new Vector3(0.36f, 0.3f, 0.36f), black, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 2.24f, 0f), new Vector3(0.36f, 0.16f, 0.36f), black, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.66f, 0f), new Vector3(0.38f, 0.02f, 0.38f), gold, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.3f, 1.35f, -0.05f), new Vector3(0.05f, 0.55f, 0.05f), PatchworkAssets.Wood(new Color(0.5f, 0.32f, 0.2f)), new Vector3(-10f, 0f, 0f), g);
            var mover = g.gameObject.AddComponent<PathMover>();
            mover.BobHeight = 0.06f;
            mover.BobSpeed = 9f;
            mover.Sway = 4f;
            mover.Setup(loop, 1.4f, start);
            Bumper.Add(g, new Vector3(0.7f, 2.3f, 0.6f), new Vector3(0f, 1.15f, 0f));
        }

        // A merry-go-round turning slowly, its horses rising and falling.
        static void Carousel(LevelKit kit, Vector3 c)
        {
            var cream = PatchworkAssets.Plain(new Color(0.97f, 0.93f, 0.85f), 0.6f);
            var gold = PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f);
            kit.Prism(Octagon(c, 4.4f), -0.05f, 0.3f, PatchworkAssets.Planks(new Color(0.85f, 0.4f, 0.45f)), "CarouselBase", 1.5f);
            var spin = kit.Group("CarouselTurn", c);
            var turn = spin.gameObject.AddComponent<Spinner>();
            turn.Speed = 22f;
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 2.6f, 0f), new Vector3(0.5f, 2.3f, 0.5f), gold, default(Vector3), spin);
            var red = PatchworkAssets.Felt(new Color(0.88f, 0.3f, 0.35f));
            var white = PatchworkAssets.Felt(new Color(0.98f, 0.95f, 0.9f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 4.9f, 0f), new Vector3(9.4f, 0.12f, 9.4f), red, default(Vector3), spin);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 5.3f, 0f), new Vector3(7.2f, 0.3f, 7.2f), white, default(Vector3), spin);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 5.8f, 0f), new Vector3(4.6f, 0.25f, 4.6f), red, default(Vector3), spin);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 6.2f, 0f), new Vector3(2f, 0.2f, 2f), white, default(Vector3), spin);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 6.6f, 0f), Vector3.one * 0.6f, gold, default(Vector3), spin);
            for (int i = 0; i < 16; i++)
            {
                float a = i / 16f * Mathf.PI * 2f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 4.7f, 4.75f, Mathf.Sin(a) * 4.7f), new Vector3(0.6f, 0.4f, 0.6f), i % 2 == 0 ? red : white, default(Vector3), spin);
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 4.75f, 5f, Mathf.Sin(a) * 4.75f), Vector3.one * 0.16f, PatchworkAssets.Glow(new Color(1f, 0.85f, 0.55f), 1.2f), default(Vector3), spin);
            }
            Color[] horses = { new Color(0.98f, 0.95f, 0.9f), new Color(0.75f, 0.55f, 0.4f), new Color(0.6f, 0.62f, 0.68f), new Color(0.98f, 0.85f, 0.6f), new Color(0.8f, 0.75f, 0.9f), new Color(0.98f, 0.8f, 0.85f) };
            for (int i = 0; i < 6; i++)
            {
                float a = i / 6f * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Cos(a) * 3.1f, 0f, Mathf.Sin(a) * 3.1f);
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 2.6f, new Vector3(0.08f, 2.3f, 0.08f), gold, default(Vector3), spin);
                var horse = new GameObject("Horse").transform;
                horse.SetParent(spin, false);
                horse.localPosition = p + Vector3.up * 1.5f;
                horse.localRotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f);
                var coat = PatchworkAssets.Felt(horses[i]);
                kit.Deco(PrimitiveType.Capsule, Vector3.zero, new Vector3(0.45f, 0.6f, 0.45f), coat, new Vector3(90f, 0f, 0f), horse);
                kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.45f, 0.55f), new Vector3(0.28f, 0.35f, 0.28f), coat, new Vector3(25f, 0f, 0f), horse);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.25f, -0.05f), new Vector3(0.47f, 0.08f, 0.45f), PatchworkAssets.Felt(FlowerColors[i % FlowerColors.Length]), default(Vector3), horse);
                for (int l = 0; l < 4; l++)
                    kit.Deco(PrimitiveType.Cylinder, new Vector3((l % 2 == 0 ? -0.12f : 0.12f), -0.4f, (l < 2 ? -0.35f : 0.35f)), new Vector3(0.08f, 0.25f, 0.08f), coat, new Vector3((l < 2 ? -20f : 30f), 0f, 0f), horse);
                var bob = horse.gameObject.AddComponent<Spinner>();
                bob.Speed = 0f;
                bob.BobHeight = 0.35f;
                bob.BobSpeed = 1.4f + (i % 2) * 0.3f;
            }
            Solid(kit, c + Vector3.up * 2.5f, new Vector3(0.6f, 5f, 0.6f));
            ClearGrass(c, 4.6f);
        }

        // ---------------------------------------------------------------- Butterflies

        static void Butterflies(LevelKit kit)
        {
            Vector3[] spots = { new Vector3(-12f, 0.5f, 12f), new Vector3(12f, 0.5f, 12f), new Vector3(13f, 0.5f, -9f), new Vector3(-73f, 0.5f, -11f), new Vector3(-58f, 0.5f, -18f),
                                new Vector3(-12f, 0.5f, 72f), new Vector3(14f, 0.5f, 74f), new Vector3(-24f, 0.5f, 80f), new Vector3(52f, 0.5f, 20f), new Vector3(-67f, 4.5f, 33f),
                                new Vector3(0f, 0.5f, -4f), new Vector3(68f, 0.5f, -44f) };
            Color[] wings = { new Color(1f, 0.6f, 0.7f), new Color(0.6f, 0.8f, 1f), new Color(1f, 0.85f, 0.4f), new Color(0.8f, 0.65f, 1f), new Color(1f, 0.7f, 0.45f) };
            for (int i = 0; i < spots.Length; i++)
            {
                var b = kit.Group("Butterfly", spots[i]);
                kit.Deco(PrimitiveType.Capsule, Vector3.zero, new Vector3(0.05f, 0.1f, 0.05f), PatchworkAssets.Plain(UIKit.Ink, 0.4f), new Vector3(90f, 0f, 0f), b);
                var m = PatchworkAssets.Felt(wings[i % wings.Length]);
                var l = new GameObject("L").transform; l.SetParent(b, false);
                var r = new GameObject("R").transform; r.SetParent(b, false);
                kit.Deco(PrimitiveType.Sphere, new Vector3(-0.13f, 0f, 0.02f), new Vector3(0.24f, 0.01f, 0.2f), m, default(Vector3), l);
                kit.Deco(PrimitiveType.Sphere, new Vector3(-0.1f, 0f, -0.1f), new Vector3(0.16f, 0.01f, 0.14f), m, default(Vector3), l);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0.13f, 0f, 0.02f), new Vector3(0.24f, 0.01f, 0.2f), m, default(Vector3), r);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0.1f, 0f, -0.1f), new Vector3(0.16f, 0.01f, 0.14f), m, default(Vector3), r);
                b.gameObject.AddComponent<Butterfly>().Setup(l, r, 1.4f + (i % 3) * 0.6f);
            }
        }
    }
}

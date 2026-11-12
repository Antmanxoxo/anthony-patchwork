using UnityEngine;

namespace Patchwork
{
    // The second pass over the Oceanic Museum: everything that makes the halls feel lived in.
    // The atrium's enchanted ceiling (constellations that take turns to blaze, a turning
    // zodiac, paper stars on threads, star dust, shooting stars), the harbour round the quay,
    // and the furniture, pictures, plants and oddments of every hall.
    public static partial class Level2Builder
    {
        // ------------------------------------------------------------------ Small helpers

        // Builds a sheet of stars with its own glow that breathes in and out.
        static Transform Twinkling(MeshBatch mb, Transform parent, string name, Color tint, float low, float high, float speed, float phase)
        {
            var mat = PatchworkAssets.Unique(PatchworkAssets.Glow(tint, high));
            var t = mb.Build(parent, name, mat);
            if (t == null) return null;
            var tw = t.gameObject.AddComponent<StarTwinkle>();
            tw.Mat = mat;
            tw.Tint = tint;
            tw.Low = low;
            tw.High = high;
            tw.Speed = speed;
            tw.Phase = phase;
            return t;
        }

        // A four-pointed sparkle lying flat (facing down from a ceiling, or up from a floor).
        static void Sparkle(MeshBatch mb, Vector3 p, float size, float turn)
        {
            var r = Quaternion.Euler(0f, turn, 0f);
            mb.Box(p, new Vector3(size * 2f, 0.05f, size * 0.22f), r);
            mb.Box(p, new Vector3(size * 0.22f, 0.05f, size * 2f), r);
            mb.Box(p, new Vector3(size * 0.7f, 0.06f, size * 0.7f), r * Quaternion.Euler(0f, 45f, 0f));
        }

        // A cloud of drifting motes.
        static DriftMotes Motes(LevelKit kit, Vector3 center, Vector3 size, int count, Color col, float glow, float moteSize, Vector3 drift)
        {
            var g = kit.Group("Motes", center);
            var mat = PatchworkAssets.Glow(col, glow);
            var list = new Transform[count];
            for (int i = 0; i < count; i++) list[i] = NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * moteSize, mat, default(Vector3), g));
            var d = g.gameObject.AddComponent<DriftMotes>();
            d.Motes = list;
            d.Size = size;
            d.Drift = drift;
            d.MoteSize = moteSize;
            return d;
        }

        static void Shooting(LevelKit kit, Vector3 from, Vector3 to, Vector3 scatter, float every, float offset, float length, float thick, Color col)
        {
            var body = NoShadow(kit.Deco(PrimitiveType.Sphere, from, Vector3.zero, PatchworkAssets.Glow(col, 3.2f)));
            body.name = "ShootingStar";
            var s = body.gameObject.AddComponent<ShootingStar>();
            s.From = from;
            s.To = to;
            s.Scatter = scatter;
            s.Every = every;
            s.Offset = offset;
            s.Length = length;
            s.Thickness = thick;
        }

        // A picture in a gilt frame on a wall (yaw faces into the room): a little felt landscape.
        static void Painting(LevelKit kit, Vector3 center, float yaw, float w, float h, int seed)
        {
            var g = kit.Group("Painting", center);
            g.rotation = Rot(yaw);
            var rng = new System.Random(seed * 53 + 7);
            Color[] skies = { new Color(0.55f, 0.75f, 0.9f), new Color(0.95f, 0.7f, 0.5f), new Color(0.2f, 0.25f, 0.5f), new Color(0.75f, 0.85f, 0.8f) };
            Color[] lands = { new Color(0.25f, 0.5f, 0.6f), new Color(0.3f, 0.5f, 0.35f), new Color(0.6f, 0.45f, 0.3f), new Color(0.15f, 0.3f, 0.5f) };
            Color sky = skies[rng.Next(skies.Length)], land = lands[rng.Next(lands.Length)];
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, 0.03f), new Vector3(w + 0.36f, h + 0.36f, 0.08f), Brass, default(Vector3), g));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, 0.08f), new Vector3(w, h, 0.02f), PatchworkAssets.Felt(sky), default(Vector3), g));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, -h * 0.3f, 0.095f), new Vector3(w, h * 0.4f, 0.01f), PatchworkAssets.Felt(land), default(Vector3), g));
            int what = rng.Next(4);
            if (what == 0)   // a sail on the sea
            {
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(w * 0.1f, -h * 0.02f, 0.1f), new Vector3(h * 0.3f, h * 0.3f, 0.01f), PatchworkAssets.Felt(CreamCol), new Vector3(0f, 0f, 45f), g));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(w * 0.1f, -h * 0.16f, 0.105f), new Vector3(h * 0.5f, h * 0.07f, 0.01f), PatchworkAssets.Felt(WoodCol), default(Vector3), g));
            }
            else if (what == 1)   // hills and a sun
            {
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(-w * 0.2f, -h * 0.2f, 0.1f), new Vector3(w * 0.6f, h * 0.5f, 0.01f), PatchworkAssets.Felt(land * 0.8f), default(Vector3), g));
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(w * 0.25f, h * 0.2f, 0.1f), new Vector3(h * 0.24f, h * 0.24f, 0.01f), PatchworkAssets.Felt(new Color(1f, 0.85f, 0.4f)), default(Vector3), g));
            }
            else if (what == 2)   // a whale
            {
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -h * 0.05f, 0.1f), new Vector3(w * 0.6f, h * 0.3f, 0.01f), PatchworkAssets.Felt(Navy), default(Vector3), g));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(-w * 0.33f, h * 0.06f, 0.1f), new Vector3(w * 0.16f, h * 0.08f, 0.01f), PatchworkAssets.Felt(Navy), new Vector3(0f, 0f, -35f), g));
            }
            else   // a lighthouse
            {
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(w * 0.15f, 0f, 0.1f), new Vector3(h * 0.12f, h * 0.6f, 0.01f), PatchworkAssets.Felt(CreamCol), default(Vector3), g));
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(w * 0.15f, h * 0.33f, 0.105f), new Vector3(h * 0.14f, h * 0.14f, 0.01f), PatchworkAssets.Glow(new Color(1f, 0.85f, 0.4f), 1.5f), default(Vector3), g));
            }
        }

        // A palm in a brass tub. Solid at the trunk.
        static void Palm(LevelKit kit, Vector3 foot, float height, int seed)
        {
            var g = kit.Group("Palm", foot);
            g.rotation = Rot(seed * 67f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.45f, 0f), new Vector3(1.3f, 0.45f, 1.3f), Brass, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.88f, 0f), new Vector3(1.14f, 0.04f, 1.14f), PatchworkAssets.Felt(new Color(0.3f, 0.22f, 0.18f)), default(Vector3), g);
            var bark = PatchworkAssets.Yarn(new Color(0.5f, 0.38f, 0.26f));
            int segs = Mathf.RoundToInt(height / 0.7f);
            for (int i = 0; i < segs; i++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(Mathf.Sin(i * 0.25f) * 0.25f, 0.9f + i * 0.7f + 0.35f, 0f), new Vector3(0.34f - i * 0.012f, 0.38f, 0.34f - i * 0.012f), bark, default(Vector3), g);
            Vector3 top = new Vector3(Mathf.Sin(segs * 0.25f) * 0.25f, 0.9f + segs * 0.7f, 0f);
            var leaf = new MeshBatch();
            for (int i = 0; i < 9; i++)
            {
                var r = Quaternion.Euler(0f, i * 40f, 0f) * Quaternion.Euler(i % 2 == 0 ? 20f : -8f, 0f, 0f);
                for (int k = 0; k < 4; k++)
                {
                    var bend = r * Quaternion.Euler(k * 14f, 0f, 0f);
                    leaf.Box(top + r * new Vector3(0f, -k * k * 0.07f, 0.4f + k * 0.62f), new Vector3(0.5f - k * 0.09f, 0.04f, 0.7f), bend);
                }
            }
            leaf.Build(g, "Fronds", PatchworkAssets.Yarn(new Color(0.24f, 0.5f, 0.36f)));
            var col = g.gameObject.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 1.2f, 0f);
            col.radius = 0.6f;
            col.height = 2.4f;
        }

        // A wooden crate. Solid.
        static void Crate(LevelKit kit, Vector3 foot, float size, float yaw)
        {
            var b = kit.Block(foot + Vector3.up * size * 0.5f, Vector3.one * size, WoodCol, "Crate", PatchworkAssets.Planks(new Color(0.7f, 0.56f, 0.4f)));
            b.rotation = Rot(yaw);
            NoShadow(kit.Deco(PrimitiveType.Cube, foot + Vector3.up * size * 0.5f, new Vector3(size + 0.04f, size * 0.14f, size + 0.04f), DarkWood, new Vector3(0f, yaw, 0f)));
        }

        // A vine (or a strand of weed) hanging from "top" and swaying.
        static void Vine(LevelKit kit, Vector3 top, float length, Color col, int seed)
        {
            int segs = Mathf.Clamp(Mathf.RoundToInt(length / 2.2f), 2, 6);
            float segLen = length / segs;
            var mat = PatchworkAssets.Glow(col, 0.2f);
            var root = kit.Group("Vine", top);
            var chain = new Transform[segs];
            Transform parent = root;
            var rng = new System.Random(seed);
            for (int i = 0; i < segs; i++)
            {
                var seg = new GameObject("Seg").transform;
                seg.SetParent(parent, false);
                seg.localPosition = i == 0 ? Vector3.zero : new Vector3(0f, -segLen, 0f);
                var mb = new MeshBatch();
                mb.Cylinder(new Vector3(0f, -segLen, 0f), 0.05f, segLen, 4);
                for (int leaf = 0; leaf < 4; leaf++)
                {
                    var lr = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f) * Quaternion.Euler(50f, 0f, 0f);
                    mb.Ball(new Vector3(0f, -segLen * (leaf + 0.5f) / 4f, 0f) + lr * new Vector3(0f, 0f, 0.28f), new Vector3(0.3f, 0.05f, 0.5f), 2, 5);
                }
                mb.Build(seg, "VineMesh", mat);
                chain[i] = seg;
                parent = seg;
            }
            var sway = root.gameObject.AddComponent<KelpSway>();
            sway.Segments = chain;
            sway.Amount = 3f + (float)rng.NextDouble() * 3f;
            sway.Speed = 0.5f + (float)rng.NextDouble() * 0.4f;
            sway.Phase = (float)rng.NextDouble() * 6f;
        }

        // A little boat riding the swell, with a lantern in it.
        static void Boat(LevelKit kit, Vector3 at, float yaw, float len, Color paint, bool mast)
        {
            var g = kit.Group("Boat", at);
            g.rotation = Rot(yaw);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.1f, 0f), new Vector3(len * 0.38f, len * 0.22f, len), PatchworkAssets.Planks(paint), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.32f, 0f), new Vector3(len * 0.3f, 0.06f, len * 0.8f), PatchworkAssets.Planks(new Color(0.6f, 0.46f, 0.34f)), default(Vector3), g);
            for (int i = -1; i <= 1; i++)
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.42f, i * len * 0.24f), new Vector3(len * 0.33f, 0.05f, len * 0.07f), DarkWood, default(Vector3), g);
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.75f, len * 0.36f), Vector3.one * 0.3f, PatchworkAssets.Glow(LampGold, 2.6f), default(Vector3), g));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.5f, len * 0.36f), new Vector3(0.04f, 0.2f, 0.04f), IronMat, default(Vector3), g);
            if (mast)
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, len * 0.45f, 0f), new Vector3(0.1f, len * 0.45f, 0.1f), DarkWood, default(Vector3), g);
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, len * 0.5f, -len * 0.16f), new Vector3(0.03f, len * 0.6f, len * 0.34f), PatchworkAssets.Felt(CreamCol), new Vector3(-14f, 0f, 0f), g));
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, len * 0.92f, 0f), Vector3.one * 0.26f, PatchworkAssets.Glow(new Color(1f, 0.5f, 0.4f), 2.4f), default(Vector3), g));
            }
            var bob = g.gameObject.AddComponent<SeaBob>();
            bob.Phase = at.x * 0.3f + at.z * 0.17f;
            bob.Height = 0.14f;
            bob.Rock = 3.5f;
        }

        // ------------------------------------------------------------------ The enchanted ceiling

        // x,y pairs on a 10 x 10 card, then pairs of point numbers to join.
        static readonly float[][] FigurePoints =
        {
            // The Whale
            new[] { 1.5f, 4f, 3f, 5.6f, 5.5f, 6f, 7.8f, 5.2f, 9f, 3.8f, 8f, 2.6f, 5.5f, 2.2f, 3f, 2.8f, 0.2f, 5.6f, 0.2f, 2.6f, 6.4f, 7.6f, 5.6f, 8.6f, 7.2f, 8.6f, 7.8f, 4f },
            // The Turtle
            new[] { 3f, 3f, 5f, 2.2f, 7f, 3f, 7f, 5.4f, 5f, 6.2f, 3f, 5.4f, 9f, 4.2f, 1.8f, 1.4f, 1.8f, 7f, 7.6f, 1.2f, 7.6f, 7.2f, 1.4f, 4.2f, 5f, 4.2f },
            // The Ship
            new[] { 1.5f, 3f, 8f, 3f, 9.2f, 4.4f, 0.6f, 4.4f, 4.8f, 4.4f, 4.8f, 9f, 8f, 5.2f, 4.8f, 5.2f, 6f, 9.4f, 2.4f, 5.6f, 2.4f, 8f, 4.8f, 7.4f },
            // The Anchor
            new[] { 5f, 9f, 5f, 8f, 5f, 1.4f, 3.4f, 7f, 6.6f, 7f, 2.6f, 3f, 1.8f, 4.4f, 7.4f, 3f, 8.2f, 4.4f, 4.4f, 9.4f, 5.6f, 9.4f },
            // The Seahorse
            new[] { 5.2f, 9f, 6.4f, 8.2f, 6f, 7f, 5f, 6.4f, 5.6f, 5.2f, 6f, 4f, 5.6f, 2.8f, 4.6f, 2f, 3.8f, 2.6f, 4.2f, 3.4f, 7.6f, 8.4f, 4.4f, 9.4f, 4.2f, 5.6f },
            // The Lighthouse
            new[] { 3.6f, 0.6f, 6.4f, 0.6f, 5.8f, 6.4f, 4.2f, 6.4f, 3.8f, 7.8f, 6.2f, 7.8f, 5f, 9.2f, 9.4f, 8.8f, 9.4f, 6.4f, 0.6f, 8.8f, 0.6f, 6.4f },
        };

        static readonly int[][] FigureLines =
        {
            new[] { 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 0, 0, 8, 0, 9, 2, 10, 10, 11, 10, 12 },
            new[] { 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 0, 2, 6, 3, 6, 0, 7, 5, 8, 2, 9, 3, 10, 0, 11, 5, 11, 1, 12, 12, 4 },
            new[] { 0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 5, 8, 4, 9, 9, 10, 10, 11 },
            new[] { 0, 1, 1, 2, 3, 4, 2, 5, 5, 6, 2, 7, 7, 8, 0, 9, 0, 10, 9, 10 },
            new[] { 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 1, 10, 0, 11, 3, 12 },
            new[] { 0, 1, 1, 2, 2, 3, 3, 0, 3, 4, 4, 5, 5, 2, 4, 6, 6, 5, 5, 7, 5, 8, 4, 9, 4, 10 },
        };

        // The atrium's ceiling. Stars in five sheets that each twinkle to their own time; a
        // milky way across one corner; six constellations of the sea that light up one after
        // another; a zodiac turning slowly round the middle; paper stars on threads all the
        // way down the hall; star dust falling through the moonbeams; and now and then a
        // shooting star.
        static void StarCeiling(LevelKit kit, float H)
        {
            var root = kit.Group("StarCeiling", Vector3.zero);
            float y = H - 0.07f;
            var rng = new System.Random(2024);
            Color[] tints = { new Color(1f, 0.97f, 0.9f), new Color(0.7f, 0.82f, 1f), new Color(1f, 0.85f, 0.5f), new Color(0.8f, 0.7f, 1f), new Color(0.6f, 0.95f, 0.95f) };
            var sheets = new MeshBatch[tints.Length];
            for (int i = 0; i < sheets.Length; i++) sheets[i] = new MeshBatch();
            for (int i = 0; i < 620; i++)
            {
                Vector3 p = new Vector3(((float)rng.NextDouble() - 0.5f) * 59f, y, -78f + ((float)rng.NextDouble() - 0.5f) * 59f);
                float s = 0.1f + (float)rng.NextDouble() * (float)rng.NextDouble() * 0.5f;
                var sheet = sheets[rng.Next(sheets.Length)];
                if (s > 0.38f) Sparkle(sheet, p, s * 1.6f, (float)rng.NextDouble() * 90f);
                else sheet.Ball(p, new Vector3(s, 0.07f, s), 2, 5);
            }
            // The milky way: a river of dust from one corner to the other.
            for (int i = 0; i < 520; i++)
            {
                float k = (float)rng.NextDouble();
                float across = ((float)rng.NextDouble() + (float)rng.NextDouble() - 1f) * 5.5f;
                Vector3 p = new Vector3(-27f + k * 54f + across * 0.6f, y, -104f + k * 50f + Mathf.Sin(k * 5f) * 4f - across);
                float s = 0.05f + (float)rng.NextDouble() * 0.1f;
                sheets[i % 2 == 0 ? 1 : 3].Ball(p, new Vector3(s, 0.05f, s), 2, 4);
            }
            for (int i = 0; i < sheets.Length; i++)
                Twinkling(sheets[i], root, "CeilingStars", tints[i], 0.9f, 3f, 0.7f + i * 0.33f, i * 1.9f);

            // The constellations.
            Vector3[] spots = { new Vector3(-17f, y, -97f), new Vector3(17f, y, -99f), new Vector3(-19f, y, -64f), new Vector3(18f, y, -62f), new Vector3(-20f, y, -81f), new Vector3(20f, y, -80f) };
            float[] turn = { 20f, -15f, 160f, 200f, 80f, -80f };
            Color[] figureTints = { new Color(0.55f, 0.8f, 1f), new Color(0.6f, 1f, 0.75f), new Color(1f, 0.8f, 0.45f), new Color(1f, 0.6f, 0.7f), new Color(0.8f, 0.65f, 1f), new Color(1f, 0.92f, 0.6f) };
            var figures = new Material[FigurePoints.Length];
            for (int f = 0; f < FigurePoints.Length; f++)
            {
                var mb = new MeshBatch();
                var pts = FigurePoints[f];
                var r = Quaternion.Euler(0f, turn[f], 0f);
                const float scale = 1.15f;
                var world = new Vector3[pts.Length / 2];
                for (int i = 0; i < world.Length; i++)
                {
                    world[i] = spots[f] + r * new Vector3((pts[i * 2] - 5f) * scale, -0.03f, (pts[i * 2 + 1] - 5f) * scale);
                    Sparkle(mb, world[i], i == 0 ? 0.6f : 0.36f, i * 23f);
                }
                var lines = FigureLines[f];
                for (int i = 0; i + 1 < lines.Length; i += 2)
                    mb.Stitches(world[lines[i]], world[lines[i + 1]], 0.34f, 0.2f, 0.09f, 0.04f);
                figures[f] = PatchworkAssets.Unique(PatchworkAssets.Glow(figureTints[f], 1f));
                mb.Build(root, "Constellation", figures[f]);
            }
            var show = root.gameObject.AddComponent<ConstellationShow>();
            show.Figures = figures;
            show.Tints = figureTints;

            // The zodiac: two brass rings round the middle of the ceiling, turning opposite ways,
            // one set with twelve star-clusters and the other with the phases of the moon.
            Vector3 mid = new Vector3(0f, H - 0.5f, -78f);
            var outer = kit.Group("Zodiac", mid);
            Hoop(kit, outer, 13f, 0.16f, Brass, 60);
            var gold = PatchworkAssets.Glow(new Color(1f, 0.85f, 0.45f), 2.2f);
            for (int i = 0; i < 12; i++)
            {
                float a = i / 12f * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Cos(a) * 13f, -0.1f, Mathf.Sin(a) * 13f);
                var cluster = new MeshBatch();
                Sparkle(cluster, p, 0.5f, i * 30f);
                for (int k = 0; k < 2 + i % 4; k++)
                    cluster.Ball(p + Quaternion.Euler(0f, k * 97f + i * 31f, 0f) * new Vector3(0.8f + k * 0.25f, 0f, 0f), new Vector3(0.2f, 0.08f, 0.2f), 2, 5);
                cluster.Build(outer, "Sign", gold);
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(Mathf.Cos(a + 0.26f) * 13f, 0f, Mathf.Sin(a + 0.26f) * 13f), new Vector3(0.08f, 0.1f, 0.7f), Brass, new Vector3(0f, -(a + 0.26f) * Mathf.Rad2Deg + 90f, 0f), outer));
            }
            outer.gameObject.AddComponent<Spinner>().Speed = 2.4f;
            var inner = kit.Group("MoonRing", mid + Vector3.down * 0.3f);
            Hoop(kit, inner, 8.4f, 0.12f, PatchworkAssets.Plain(new Color(0.8f, 0.84f, 0.92f), 0.85f), 44);
            var moonLit = PatchworkAssets.Glow(new Color(0.95f, 0.95f, 0.85f), 2f);
            var moonDark = PatchworkAssets.Felt(new Color(0.12f, 0.14f, 0.3f));
            for (int i = 0; i < 8; i++)
            {
                float a = i / 8f * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Cos(a) * 8.4f, -0.1f, Mathf.Sin(a) * 8.4f);
                NoShadow(kit.Deco(PrimitiveType.Sphere, p, new Vector3(1.1f, 0.2f, 1.1f), moonDark, default(Vector3), inner));
                float lit = 1f - Mathf.Abs(i - 4) / 4f;   // new moon round to full and back
                if (lit > 0.01f)
                    NoShadow(kit.Deco(PrimitiveType.Sphere, p + new Vector3(0f, -0.05f, 0f) + Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f) * new Vector3(0f, 0f, (1f - lit) * 0.5f * (i < 4 ? 1f : -1f)),
                        new Vector3(1.1f, 0.2f, 1.1f * lit), moonLit, new Vector3(0f, -a * Mathf.Rad2Deg, 0f), inner));
            }
            inner.gameObject.AddComponent<Spinner>().Speed = -3.4f;

            // Paper stars on threads, in a slow double spiral down the length of the hall.
            var mobile = kit.Group("StarMobile", new Vector3(0f, 0f, -78f));
            var paper = new[] { PatchworkAssets.Glow(new Color(1f, 0.85f, 0.45f), 1.8f), PatchworkAssets.Glow(new Color(0.85f, 0.9f, 1f), 1.7f), PatchworkAssets.Glow(new Color(0.75f, 0.65f, 1f), 1.6f) };
            var thread = new MeshBatch();
            var hung = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < 64; i++)
            {
                float a = i * 0.83f;
                float rad = 8f + (i % 5) * 4.4f;
                float hy = 11.5f + ((i * 37) % 150) / 10f;
                Vector3 p = new Vector3(Mathf.Cos(a) * rad, hy, Mathf.Sin(a) * rad * 1.05f);
                if (Mathf.Abs(p.x) < 6.5f && hy > 12f && hy < 24f) continue;                      // leave the whale room
                if (p.x > 24f || Mathf.Abs(p.x) > 27f || Mathf.Abs(p.z) > 27f) continue;           // and keep off the walls
                var star = new GameObject("PaperStar").transform;
                star.SetParent(mobile, false);
                star.localPosition = p;
                float s = 0.45f + (i % 4) * 0.16f;
                var m = paper[i % 3];
                NoShadow(kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(s, s, 0.05f), m, new Vector3(0f, 0f, 45f), star));
                NoShadow(kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(s * 0.34f, s * 1.9f, 0.05f), m, default(Vector3), star));
                NoShadow(kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(s * 1.9f, s * 0.34f, 0.05f), m, default(Vector3), star));
                NoShadow(kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(0.05f, s, s), m, new Vector3(45f, 0f, 0f), star));
                thread.Cylinder(new Vector3(p.x, hy + s, p.z - 78f), 0.012f, H - hy - s, 3);
                if (i % 9 == 0) PointLight(kit, new Vector3(p.x, hy, p.z - 78f), i % 2 == 0 ? new Color(1f, 0.85f, 0.5f) : new Color(0.7f, 0.8f, 1f), 9f, 0.9f);
                hung.Add(star);
            }
            thread.Build(root, "StarThreads", PatchworkAssets.Glow(new Color(0.8f, 0.8f, 0.9f), 0.5f));
            mobile.gameObject.AddComponent<StarMobile>().Stars = hung.ToArray();

            // Star dust in the air, and shooting stars under the roof.
            Motes(kit, new Vector3(0f, 15f, -78f), new Vector3(54f, 26f, 54f), 90, new Color(1f, 0.92f, 0.7f), 2.4f, 0.13f, new Vector3(0.12f, -0.3f, 0.06f));
            Motes(kit, new Vector3(0f, 15f, -78f), new Vector3(50f, 26f, 50f), 50, new Color(0.7f, 0.85f, 1f), 2.4f, 0.1f, new Vector3(-0.08f, -0.2f, 0.1f));
            Shooting(kit, new Vector3(-26f, H - 0.9f, -100f), new Vector3(22f, H - 1.6f, -66f), new Vector3(8f, 0f, 16f), 9f, 0f, 6f, 0.2f, new Color(1f, 0.95f, 0.8f));
            Shooting(kit, new Vector3(24f, H - 0.9f, -54f), new Vector3(-20f, H - 1.8f, -84f), new Vector3(10f, 0f, 14f), 13f, 4f, 7f, 0.22f, new Color(0.8f, 0.9f, 1f));
            Shooting(kit, new Vector3(-10f, H - 0.8f, -52f), new Vector3(14f, H - 1.4f, -102f), new Vector3(16f, 0f, 6f), 17f, 9f, 5f, 0.18f, new Color(1f, 0.85f, 0.55f));
        }

        // ------------------------------------------------------------------ Everything else

        static void Details(LevelKit kit)
        {
            HarbourDetails(kit);
            AtriumDetails(kit);
            HistoryDetails(kit);
            KelpDetails(kit);
            DeepDetails(kit);
            RuinsDetails(kit);
            SkyDetails(kit);
        }

        // The quay and the sea round it.
        static void HarbourDetails(LevelKit kit)
        {
            // ----- Out at sea -----
            // A lighthouse on its rock to the west, sweeping its beam.
            Vector3 lh = new Vector3(-150f, -2.7f, -185f);
            var rock = new MeshBatch();
            var rr = new System.Random(5);
            for (int i = 0; i < 14; i++)
                rock.Ball(lh + new Vector3(((float)rr.NextDouble() - 0.5f) * 30f, (float)rr.NextDouble() * 2f, ((float)rr.NextDouble() - 0.5f) * 22f), new Vector3(8f + (float)rr.NextDouble() * 10f, 5f + (float)rr.NextDouble() * 6f, 8f + (float)rr.NextDouble() * 8f), 4, 7);
            rock.Build(kit.Root, "LighthouseRock", PatchworkAssets.Felt(new Color(0.16f, 0.18f, 0.3f)));
            var stripes = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.8f, 0.25f, 0.25f), CreamCol);
            for (int i = 0; i < 5; i++)
                kit.Deco(PrimitiveType.Cylinder, lh + Vector3.up * (6f + i * 6f), new Vector3(7f - i * 0.7f, 3f, 7f - i * 0.7f), i % 2 == 0 ? stripes : PatchworkAssets.Felt(CreamCol));
            kit.Deco(PrimitiveType.Cylinder, lh + Vector3.up * 33.4f, new Vector3(5.6f, 0.4f, 5.6f), IronMat);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, lh + Vector3.up * 35.6f, new Vector3(3.4f, 1.8f, 3.4f), PatchworkAssets.Glow(new Color(1f, 0.92f, 0.6f), 3f)));
            kit.Deco(PrimitiveType.Sphere, lh + Vector3.up * 38f, new Vector3(4.4f, 2.6f, 4.4f), PatchworkAssets.Shingle(Navy));
            // The lamp glows, and its sweep is a real spot light turning round out over the sea.
            var lamp = kit.Group("LighthouseBeam", lh + Vector3.up * 35.6f);
            var sweep = new GameObject("Sweep").AddComponent<Light>();
            sweep.transform.SetParent(lamp, false);
            sweep.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
            sweep.type = LightType.Spot;
            sweep.color = new Color(1f, 0.92f, 0.7f);
            sweep.range = 160f;
            sweep.spotAngle = 14f;
            sweep.intensity = 260f;
            sweep.shadows = LightShadows.None;
            lamp.gameObject.AddComponent<Spinner>().Speed = 16f;
            // An island with a few lit windows to the east, and far hills behind.
            var isle = new MeshBatch();
            for (int i = 0; i < 9; i++)
                isle.Ball(new Vector3(150f + i * 9f - (i % 3) * 6f, -3f, -200f + (i % 4) * 9f), new Vector3(26f, 9f + (i % 3) * 6f, 20f), 4, 8);
            for (int i = 0; i < 16; i++)
                isle.Ball(new Vector3(-320f + i * 44f, -3f, -420f - (i % 3) * 20f), new Vector3(90f, 20f + (i % 4) * 12f, 40f), 3, 8);
            isle.Build(kit.Root, "FarIslands", PatchworkAssets.Felt(new Color(0.1f, 0.13f, 0.26f)));
            var win = new MeshBatch();
            for (int i = 0; i < 18; i++)
                win.Box(new Vector3(146f + (i * 7) % 52, 1.5f + (i * 5) % 7, -196f + (i * 3) % 14), new Vector3(0.9f, 1.1f, 0.9f), Quaternion.identity);
            win.Build(kit.Root, "IslandWindows", PatchworkAssets.Glow(new Color(1f, 0.8f, 0.45f), 2.6f));
            // Boats riding at anchor beyond the railings.
            Boat(kit, new Vector3(-66f, -2.6f, -128f), 20f, 4.4f, new Color(0.3f, 0.5f, 0.6f), false);
            Boat(kit, new Vector3(-70f, -2.6f, -148f), -30f, 5f, new Color(0.7f, 0.3f, 0.3f), true);
            Boat(kit, new Vector3(67f, -2.6f, -138f), 160f, 4.6f, new Color(0.85f, 0.75f, 0.5f), false);
            Boat(kit, new Vector3(72f, -2.6f, -156f), 200f, 6f, new Color(0.25f, 0.35f, 0.6f), true);
            Boat(kit, new Vector3(-20f, -2.6f, -172f), 80f, 7.5f, new Color(0.3f, 0.3f, 0.5f), true);
            Boat(kit, new Vector3(34f, -2.6f, -176f), 110f, 4.4f, new Color(0.6f, 0.4f, 0.3f), false);
            // A ship a long way out, strung with lights.
            var ship = kit.Group("FarShip", new Vector3(40f, -2.6f, -300f));
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 2f, 0f), new Vector3(46f, 9f, 9f), PatchworkAssets.Felt(new Color(0.08f, 0.09f, 0.18f)), default(Vector3), ship);
            kit.Deco(PrimitiveType.Cube, new Vector3(-2f, 8f, 0f), new Vector3(22f, 5f, 6f), PatchworkAssets.Felt(new Color(0.12f, 0.13f, 0.24f)), default(Vector3), ship);
            for (int i = 0; i < 2; i++) kit.Deco(PrimitiveType.Cylinder, new Vector3(-6f + i * 8f, 13f, 0f), new Vector3(2.6f, 3f, 2.6f), PatchworkAssets.Felt(new Color(0.5f, 0.2f, 0.2f)), default(Vector3), ship);
            var ports = new MeshBatch();
            for (int i = 0; i < 16; i++)
            {
                ports.Ball(new Vector3(-17f + i * 2.3f, 4.6f, 4.4f), Vector3.one * 0.7f, 2, 5);
                if (i % 2 == 0) ports.Ball(new Vector3(-11f + i * 1.3f, 8.6f, 3.1f), Vector3.one * 0.7f, 2, 5);
            }
            ports.Build(ship, "Portholes", PatchworkAssets.Glow(new Color(1f, 0.85f, 0.5f), 3f));
            ship.gameObject.AddComponent<SeaBob>().Height = 0.3f;

            // ----- In the sky -----
            // The northern lights, low over the sea to the south, and a shooting star or two.
            Color[] aurora = { new Color(0.3f, 1f, 0.7f), new Color(0.5f, 0.6f, 1f), new Color(0.8f, 0.5f, 1f) };
            // (Soft glowing clouds of colour rather than flat sheets, so they never show an edge.)
            for (int band = 0; band < 3; band++)
            {
                var veil = Shaft(aurora[band], 0.035f);
                for (int i = 0; i < 12; i++)
                {
                    float k = i / 11f;
                    Vector3 p = new Vector3(-230f + k * 460f, 70f + band * 16f + Mathf.Sin(k * 7f + band) * 12f, -380f - band * 20f + Mathf.Cos(k * 5f + band * 2f) * 24f);
                    NoShadow(kit.Deco(PrimitiveType.Sphere, p, new Vector3(60f, 36f + band * 6f, 14f), veil, new Vector3(0f, Mathf.Sin(k * 9f + band) * 28f, 0f)));
                }
            }
            Shooting(kit, new Vector3(-180f, 190f, -260f), new Vector3(-40f, 120f, -330f), new Vector3(160f, 30f, 60f), 11f, 2f, 34f, 1.1f, new Color(1f, 0.97f, 0.85f));
            Shooting(kit, new Vector3(140f, 200f, -240f), new Vector3(30f, 130f, -300f), new Vector3(120f, 30f, 60f), 19f, 8f, 30f, 1f, new Color(0.8f, 0.9f, 1f));
            Shooting(kit, new Vector3(-60f, 150f, 30f), new Vector3(60f, 110f, 60f), new Vector3(90f, 20f, 20f), 23f, 13f, 30f, 1f, new Color(1f, 0.9f, 0.7f));
            // Thin night clouds catching the moon.
            var cloud = Shaft(new Color(0.7f, 0.75f, 0.95f), 0.06f);
            foreach (var p in new[] { new Vector3(-90f, 120f, -40f), new Vector3(70f, 135f, 10f), new Vector3(-20f, 150f, -330f), new Vector3(160f, 110f, -250f), new Vector3(-190f, 100f, -200f) })
                for (int i = 0; i < 4; i++)
                    NoShadow(kit.Deco(PrimitiveType.Sphere, p + new Vector3(i * 20f - 30f, (i % 2) * 5f, (i % 3) * 8f), new Vector3(44f - i * 4f, 9f, 22f), cloud));

            // ----- On the quay -----
            // Fireflies round the trees and fountains.
            Motes(kit, new Vector3(0f, 3f, -134f), new Vector3(104f, 5f, 46f), 70, new Color(0.9f, 1f, 0.5f), 3f, 0.11f, new Vector3(0.25f, 0.06f, 0.18f)).Flicker = 3.4f;
            // Strings of little lamps from post to post along the carpet.
            Color[] bulbs = { new Color(1f, 0.8f, 0.4f), new Color(1f, 0.5f, 0.5f), new Color(0.5f, 0.8f, 1f), new Color(0.7f, 1f, 0.6f) };
            var bulbBatch = new MeshBatch[bulbs.Length];
            for (int i = 0; i < bulbBatch.Length; i++) bulbBatch[i] = new MeshBatch();
            var wire = PatchworkAssets.Plain(new Color(0.1f, 0.1f, 0.14f), 0.3f);
            for (int s = -1; s <= 1; s += 2)
                for (int i = 0; i < 2; i++)
                {
                    Vector3 a = new Vector3(s * 5.5f, 3.6f, -139f + i * 8.4f), b = a + new Vector3(0f, 0f, 8.4f);
                    Vector3 prev = a;
                    for (int k = 1; k <= 8; k++)
                    {
                        float u = k / 8f;
                        Vector3 p = Vector3.Lerp(a, b, u) + Vector3.down * Mathf.Sin(u * Mathf.PI) * 0.9f;
                        kit.Rope(prev, p, wire, 0.03f);
                        if (k < 8) bulbBatch[(k + i) % bulbs.Length].Ball(p + Vector3.down * 0.14f, Vector3.one * 0.22f, 3, 6);
                        prev = p;
                    }
                }
            // And one long swag across the front of the portico.
            {
                Vector3 prev = new Vector3(-18f, 9f, -116.2f);
                for (int k = 1; k <= 30; k++)
                {
                    float u = k / 30f;
                    Vector3 p = new Vector3(-18f + u * 36f, 9f - Mathf.Abs(Mathf.Sin(u * Mathf.PI * 5f)) * 1.2f, -116.2f);
                    kit.Rope(prev, p, wire, 0.03f);
                    bulbBatch[k % bulbs.Length].Ball(p + Vector3.down * 0.16f, Vector3.one * 0.26f, 3, 6);
                    prev = p;
                }
            }
            for (int i = 0; i < bulbBatch.Length; i++) bulbBatch[i].Build(kit.Root, "FairyLights", PatchworkAssets.Glow(bulbs[i], 2.6f));
            // The ticket booth: a little striped kiosk with a lit window.
            Vector3 tb = new Vector3(13f, 0f, -121f);
            kit.Block(tb + Vector3.up * 1.4f, new Vector3(3f, 2.8f, 3f), WoodCol, "TicketBooth", PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, Navy, CreamCol));
            for (int i = 0; i < 4; i++)
                kit.Deco(PrimitiveType.Cube, tb + Vector3.up * (3f + i * 0.4f), new Vector3(3.8f - i * 0.9f, 0.4f, 3.8f - i * 0.9f), PatchworkAssets.Shingle(Wine));
            NoShadow(kit.Deco(PrimitiveType.Sphere, tb + Vector3.up * 4.8f, Vector3.one * 0.4f, Brass));
            NoShadow(kit.Deco(PrimitiveType.Cube, tb + new Vector3(0f, 1.7f, -1.52f), new Vector3(1.6f, 1f, 0.05f), PatchworkAssets.Glow(new Color(1f, 0.85f, 0.5f), 1.8f)));
            kit.Deco(PrimitiveType.Cube, tb + new Vector3(0f, 1.15f, -1.7f), new Vector3(2f, 0.1f, 0.5f), Brass);
            NoShadow(kit.Deco(PrimitiveType.Cube, tb + new Vector3(0f, 2.5f, -1.56f), new Vector3(2f, 0.4f, 0.05f), StarCloth));
            PointLight(kit, tb + new Vector3(0f, 1.8f, -2.4f), LampGold, 7f, 1.1f);
            // Poster boards either side of the door.
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 pb = new Vector3(s * 8.2f, 0f, -111.4f);
                kit.Block(pb + Vector3.up * 1.6f, new Vector3(2.4f, 3.2f, 0.3f), WoodCol, "PosterBoard", DarkWood);
                Painting(kit, pb + new Vector3(0f, 1.7f, -0.16f), 180f, 1.7f, 2.3f, s > 0 ? 2 : 10);
            }
            // Bronze sea-creatures on plinths: a whale's tail to the left, a seahorse to the right.
            var bronze = PatchworkAssets.Plain(new Color(0.3f, 0.5f, 0.45f), 0.7f);
            Vector3 wt = Plinth(kit, new Vector3(-14f, 0f, -125f), new Vector3(2.6f, 1f, 2.6f));
            kit.Deco(PrimitiveType.Capsule, wt + Vector3.up * 1.2f, new Vector3(0.8f, 1.3f, 0.6f), bronze, new Vector3(8f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Sphere, wt + new Vector3(s * 0.9f, 2.7f, 0.1f), new Vector3(1.9f, 0.5f, 0.8f), bronze, new Vector3(0f, s * -20f, s * 28f));
            TownKit.Solid(kit, wt + Vector3.up * 1.4f, new Vector3(1f, 2.8f, 0.8f), 0f);
            Vector3 sh = Plinth(kit, new Vector3(14f, 0f, -130f), new Vector3(2.4f, 1f, 2.4f));
            for (int i = 0; i < 9; i++)
            {
                float a = i * 0.5f;
                kit.Deco(PrimitiveType.Sphere, sh + new Vector3(Mathf.Sin(a * 1.2f) * 0.3f, 0.3f + i * 0.33f, Mathf.Cos(a) * 0.2f - 0.1f), Vector3.one * (0.4f + Mathf.Sin(i / 8f * Mathf.PI) * 0.36f), bronze);
            }
            kit.Deco(PrimitiveType.Cube, sh + new Vector3(0f, 3.1f, 0.4f), new Vector3(0.2f, 0.2f, 0.7f), bronze, new Vector3(18f, 0f, 0f));
            kit.Deco(PrimitiveType.Sphere, sh + new Vector3(0f, 3.2f, 0f), Vector3.one * 0.66f, bronze);
            TownKit.Solid(kit, sh + Vector3.up * 1.6f, new Vector3(0.9f, 3.2f, 0.9f), 0f);
            // A big brass anchor leaning on its plinth, bollards and chain along the south edge, crates.
            Vector3 an = new Vector3(-30f, 0f, -150f);
            Plinth(kit, an, new Vector3(4f, 0.5f, 3f));
            var anchor = kit.Group("Anchor", an + Vector3.up * 0.5f);
            anchor.rotation = Quaternion.Euler(0f, 30f, 24f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.7f, 0f), new Vector3(0.26f, 1.7f, 0.26f), Brass, default(Vector3), anchor);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 2.9f, 0f), new Vector3(0.2f, 0.9f, 0.2f), Brass, new Vector3(0f, 0f, 90f), anchor);
            var ringGroup = new GameObject("Ring").transform;
            ringGroup.SetParent(anchor, false);
            ringGroup.localPosition = new Vector3(0f, 3.7f, 0f);
            ringGroup.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Hoop(kit, ringGroup, 0.36f, 0.12f, Brass, 10);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.6f, 0.36f, 0f), new Vector3(1.4f, 0.26f, 0.26f), Brass, new Vector3(0f, 0f, s * 34f), anchor);
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 1.2f, 0.86f, 0f), new Vector3(0.5f, 0.5f, 0.2f), Brass, new Vector3(0f, 0f, 45f), anchor);
            }
            TownKit.Solid(kit, an + Vector3.up * 1.6f, new Vector3(2f, 2.4f, 1.4f), 30f);
            for (float x = -52f; x <= 52f; x += 13f)
            {
                if (Mathf.Abs(x) < 8f) continue;
                kit.Deco(PrimitiveType.Cylinder, new Vector3(x, 0.5f, -159.4f), new Vector3(0.7f, 0.5f, 0.7f), IronMat);
                kit.Deco(PrimitiveType.Sphere, new Vector3(x, 1.05f, -159.4f), new Vector3(0.9f, 0.5f, 0.9f), IronMat);
                TownKit.Solid(kit, new Vector3(x, 0.6f, -159.4f), new Vector3(0.7f, 1.2f, 0.7f), 0f);
            }
            Crate(kit, new Vector3(32f, 0f, -152f), 1.6f, 12f);
            Crate(kit, new Vector3(33.9f, 0f, -151.4f), 1.2f, -20f);
            Crate(kit, new Vector3(32.4f, 1.6f, -152f), 1f, 30f);
            foreach (var p in new[] { new Vector3(35.6f, 0f, -153.4f), new Vector3(-44f, 0f, -148f), new Vector3(-45.4f, 0f, -146.8f) })
            {
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.7f, new Vector3(1.1f, 0.7f, 1.1f), PatchworkAssets.Planks(new Color(0.6f, 0.42f, 0.28f)));
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.7f, new Vector3(1.16f, 0.06f, 1.16f), IronMat);
                TownKit.Solid(kit, p + Vector3.up * 0.7f, new Vector3(1f, 1.4f, 1f), 0f);
            }
            // Compass roses let into the cobbles each side.
            var gold = PatchworkAssets.Felt(new Color(1f, 0.82f, 0.4f));
            foreach (float x in new[] { 38f })   // (the carousel stands where the other one was)
            {
                Vector3 c = new Vector3(x, 0.02f, -134f);
                NoShadow(kit.Deco(PrimitiveType.Cylinder, c, new Vector3(9f, 0.02f, 9f), PatchworkAssets.Felt(Navy)));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.01f, new Vector3(6f, 0.02f, 6f), PatchworkAssets.Felt(new Color(0.2f, 0.25f, 0.5f))));
                for (int i = 0; i < 4; i++)
                    NoShadow(kit.Deco(PrimitiveType.Cube, c + Vector3.up * 0.03f, new Vector3(i % 2 == 0 ? 0.7f : 0.4f, 0.02f, i % 2 == 0 ? 8.4f : 5.4f), gold, new Vector3(0f, i * 45f, 0f)));
            }
            // Flower boxes under the windows, flags on the towers, a clock over the door.
            Color[] blooms = { new Color(1f, 0.6f, 0.7f), new Color(1f, 0.85f, 0.4f), new Color(0.7f, 0.6f, 1f) };
            foreach (float x in new[] { -48f, -37f, -26f, 26f, 37f, 48f })
            {
                kit.Block(new Vector3(x, 0.5f, -110.8f), new Vector3(5f, 1f, 1.4f), StoneCol, "FlowerBox", PaleStone);
                var flowers = new MeshBatch();
                var leaves = new MeshBatch();
                for (int i = 0; i < 12; i++)
                {
                    Vector3 p = new Vector3(x - 2.1f + i * 0.38f, 1.2f + (i % 3) * 0.14f, -110.8f + ((i * 7) % 5 - 2) * 0.2f);
                    leaves.Ball(p + Vector3.down * 0.12f, new Vector3(0.6f, 0.4f, 0.6f), 3, 6);
                    flowers.Ball(p + Vector3.up * 0.16f, Vector3.one * 0.28f, 3, 6);
                }
                leaves.Build(kit.Root, "BoxLeaves", PatchworkAssets.Yarn(new Color(0.22f, 0.45f, 0.36f)));
                flowers.Build(kit.Root, "BoxFlowers", PatchworkAssets.Glow(blooms[Mathf.Abs(Mathf.RoundToInt(x)) % 3], 0.6f));
            }
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 56f, 62f, -113f), new Vector3(0.2f, 4f, 0.2f), IronMat);
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 56f + 2f, 64.6f, -113f), new Vector3(4f, 2.2f, 0.06f), s > 0 ? StarCloth : PlumStars));
            }
            var clock = kit.Group("Clock", new Vector3(0f, 12.6f, -110.05f));
            clock.rotation = Quaternion.Euler(90f, 0f, 0f);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(3.2f, 0.08f, 3.2f), Brass, default(Vector3), clock));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.06f, 0f), new Vector3(2.7f, 0.04f, 2.7f), PatchworkAssets.Glow(new Color(1f, 0.95f, 0.8f), 1.2f), default(Vector3), clock));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.1f, -0.4f), new Vector3(0.12f, 0.03f, 0.9f), IronMat, default(Vector3), clock));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0.36f, 0.1f, 0.26f), new Vector3(0.1f, 0.03f, 1.1f), IronMat, new Vector3(0f, 55f, 0f), clock));
            for (int i = 0; i < 12; i++)
                NoShadow(kit.Deco(PrimitiveType.Cube, Quaternion.Euler(0f, i * 30f, 0f) * new Vector3(0f, 0.1f, 1.15f), new Vector3(0.08f, 0.03f, i % 3 == 0 ? 0.3f : 0.16f), IronMat, new Vector3(0f, i * 30f, 0f), clock));
        }

        // The Grand Atrium's furniture.
        static void AtriumDetails(LevelKit kit)
        {
            // Palms in the corners and by the stairs.
            int n = 0;
            foreach (var p in new[] { new Vector3(-21f, 0f, -61f), new Vector3(10f, 0f, -63.5f), new Vector3(-10f, 0f, -63.5f) })
                Palm(kit, p, 3.2f + (n % 3) * 0.7f, n++);
            Palm(kit, new Vector3(-27.5f, 8f, -51f), 2.4f, 7);
            Palm(kit, new Vector3(28f, 8f, -52.6f), 2.4f, 8);
            // Brass turnstiles just inside the doors (with room to walk between).
            foreach (float x in new[] { -6.5f, -3.4f, 3.4f, 6.5f })
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(x, 0.6f, -105f), new Vector3(0.3f, 0.6f, 0.3f), Brass);
                kit.Deco(PrimitiveType.Sphere, new Vector3(x, 1.26f, -105f), Vector3.one * 0.4f, Brass);
                TownKit.Solid(kit, new Vector3(x, 0.7f, -105f), new Vector3(0.4f, 1.4f, 0.4f), 0f);
            }
            foreach (float x in new[] { -4.95f, 4.95f })
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(x, 0.95f, -105f), new Vector3(2.8f, 0.08f, 0.08f), Brass);
                TownKit.Solid(kit, new Vector3(x, 0.6f, -105f), new Vector3(2.8f, 1.2f, 0.2f), 0f);
            }
            // Pictures along the south wall and up on the west gallery.
            int seed = 0;
            foreach (float x in new[] { -24f, -18f, -12f, 12f, 18f, 24f })
                Painting(kit, new Vector3(x, 4.2f + (seed % 2) * 0.5f, -107.9f), 0f, 3.4f, 2.4f, seed++);
            foreach (float z in new[] { -95f, -72f, -62f })
                Painting(kit, new Vector3(-29.9f, 11f, z), 90f, 3f, 2.2f, seed++);
            foreach (float z in new[] { -98f, -86f, -70f, -60f })
                Painting(kit, new Vector3(-29.9f, 3.6f, z), 90f, 2.6f, 2f, seed++);
            // A grand clock on the north wall over the landing.
            var clock = kit.Group("AtriumClock", new Vector3(0f, 9.4f, -47.95f));
            clock.rotation = Quaternion.Euler(-90f, 0f, 0f);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(4.6f, 0.1f, 4.6f), Brass, default(Vector3), clock));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.08f, 0f), new Vector3(4f, 0.04f, 4f), StarCloth, default(Vector3), clock));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.12f, 0f), new Vector3(2.4f, 0.03f, 2.4f), PatchworkAssets.Glow(new Color(1f, 0.95f, 0.8f), 1f), default(Vector3), clock));
            var hands = new GameObject("Hands").transform;
            hands.SetParent(clock, false);
            hands.localPosition = new Vector3(0f, 0.16f, 0f);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, 0.8f), new Vector3(0.12f, 0.03f, 1.7f), IronMat, default(Vector3), hands));
            NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.3f, 0.1f, 0.3f), Brass, default(Vector3), hands));
            hands.gameObject.AddComponent<Spinner>().Speed = 6f;
            // A cart of plush sea creatures by the stairs.
            Vector3 cart = new Vector3(15f, 0f, -63f);
            kit.Block(cart + Vector3.up * 0.9f, new Vector3(3.2f, 0.9f, 1.8f), WoodCol, "GiftCart", PatchworkAssets.Planks(new Color(0.75f, 0.5f, 0.4f)));
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cylinder, cart + new Vector3(s * 1.2f, 0.45f, 0.95f), new Vector3(0.9f, 0.06f, 0.9f), DarkWood, new Vector3(90f, 0f, 0f));
                kit.Deco(PrimitiveType.Cylinder, cart + new Vector3(s * 1.5f, 2.2f, 0f), new Vector3(0.06f, 0.9f, 0.06f), Brass);
            }
            NoShadow(kit.Deco(PrimitiveType.Cube, cart + Vector3.up * 3.15f, new Vector3(3.6f, 0.1f, 2.2f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, Wine, CreamCol)));
            Color[] plush = { new Color(1f, 0.6f, 0.3f), new Color(0.5f, 0.75f, 1f), new Color(1f, 0.55f, 0.75f), new Color(0.6f, 0.9f, 0.6f), new Color(0.8f, 0.65f, 1f) };
            for (int i = 0; i < 7; i++)
            {
                Vector3 p = cart + new Vector3(-1.2f + i * 0.4f, 1.55f + (i % 2) * 0.08f, (i % 3 - 1) * 0.4f);
                var m = PatchworkAssets.Felt(plush[i % plush.Length]);
                kit.Deco(PrimitiveType.Sphere, p, new Vector3(0.5f, 0.36f, 0.3f), m, new Vector3(0f, i * 40f, 0f));
                kit.Deco(PrimitiveType.Cube, p + Quaternion.Euler(0f, i * 40f, 0f) * new Vector3(-0.3f, 0f, 0f), new Vector3(0.16f, 0.26f, 0.04f), m, new Vector3(0f, i * 40f, 0f));
            }
            PointLight(kit, cart + Vector3.up * 2.6f, LampGold, 7f, 1f);
            // A narwhal's tusk in a tall case.
            Vector3 nt = DisplayCase(kit, new Vector3(-14f, 0f, -90f), new Vector3(1.8f, 4f, 1.8f));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, nt + Vector3.up * 1.4f, new Vector3(0.14f, 1.4f, 0.14f), PatchworkAssets.Felt(BoneCol), new Vector3(0f, 0f, 4f)));
            for (int i = 0; i < 9; i++)
                NoShadow(kit.Deco(PrimitiveType.Cylinder, nt + Vector3.up * (0.2f + i * 0.3f), new Vector3(0.17f - i * 0.012f, 0.02f, 0.17f - i * 0.012f), PatchworkAssets.Felt(new Color(0.8f, 0.74f, 0.6f)), new Vector3(8f, i * 30f, 4f)));
            Plaque(kit, new Vector3(-14f, 0f, -88.1f), 0f, "The Unicorn's Horn",
                "For hundreds of years, kings paid fortunes for unicorn horns like this one, and drank from cups made of them to be safe from poison.",
                "It's a tooth. It belongs to the narwhal, a small Arctic whale, and it grows straight out through the upper lip in a spiral, up to three metres long.");
            // Velvet benches round the armillary, and telescopes on the east balcony.
            TownKit.Bench(kit, new Vector3(-9.5f, 0f, -86f), 90f);
            TownKit.Bench(kit, new Vector3(9.5f, 0f, -86f), 270f);
            for (int i = 0; i < 2; i++)
            {
                Vector3 t = new Vector3(21f + i * 4.6f, 8f, -52.8f);
                kit.Deco(PrimitiveType.Cylinder, t + Vector3.up * 0.7f, new Vector3(0.1f, 0.7f, 0.1f), Brass);
                kit.Deco(PrimitiveType.Cylinder, t + new Vector3(0f, 1.5f, -0.2f), new Vector3(0.2f, 0.6f, 0.2f), Brass, new Vector3(80f, 20f - i * 40f, 0f));
            }
            // A second ring of lamplight: sconces on the piers.
            foreach (float z in new[] { -104f, -91f, -65f, -52f })
                Sconce(kit, new Vector3(-28.8f, 5f, z), 90f, false);
        }

        // The Hall of Deep Time.
        static void HistoryDetails(LevelKit kit)
        {
            // A flying reptile hung over the sands.
            Vector3 pt = new Vector3(-66f, 13f, -92f);
            var bone = Bone;
            var g = kit.Group("Pterosaur", pt);
            g.rotation = Quaternion.Euler(8f, 100f, -6f);
            NoShadow(kit.Deco(PrimitiveType.Capsule, Vector3.zero, new Vector3(0.6f, 0.9f, 0.6f), bone, new Vector3(90f, 0f, 0f), g));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.2f, 1.7f), new Vector3(0.24f, 0.3f, 2f), bone, default(Vector3), g));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.6f, 0.7f), new Vector3(0.08f, 0.5f, 1f), bone, new Vector3(-30f, 0f, 0f), g));
            for (int s = -1; s <= 1; s += 2)
            {
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 1.6f, 0.3f, 0.2f), new Vector3(3.2f, 0.12f, 0.16f), bone, new Vector3(0f, 0f, s * 10f), g));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 4.8f, 0.3f, -0.4f), new Vector3(3.6f, 0.09f, 0.12f), bone, new Vector3(0f, s * 20f, s * -6f), g));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 3.2f, 0.26f, -0.9f), new Vector3(6f, 0.02f, 1.8f), PatchworkAssets.Bubble(new Color(0.9f, 0.82f, 0.6f, 0.35f)), new Vector3(0f, s * 10f, s * 4f), g));
            }
            Beam(kit, pt + Vector3.up * 0.4f, new Vector3(pt.x, HistTop, pt.z), 0.05f, IronMat);
            // The sea dragon in its slab of rock, on the west wall.
            Vector3 slab = new Vector3(-87.7f, 4f, -72f);
            NoShadow(kit.Deco(PrimitiveType.Cube, slab, new Vector3(0.5f, 3.4f, 7f), PatchworkAssets.Cobble(new Color(0.34f, 0.34f, 0.4f))));
            NoShadow(kit.Deco(PrimitiveType.Cube, slab, new Vector3(0.6f, 3.8f, 7.4f), DarkWood));
            Spine(kit, slab + new Vector3(0.34f, -0.2f, -2.8f), slab + new Vector3(0.34f, 0.3f, 2f), 0.3f, 0.22f, 0.5f, 0.3f, 0.7f);
            NoShadow(kit.Deco(PrimitiveType.Cube, slab + new Vector3(0.34f, 0.3f, 2.7f), new Vector3(0.1f, 0.36f, 1.4f), bone));
            NoShadow(kit.Deco(PrimitiveType.Sphere, slab + new Vector3(0.36f, 0.4f, 2.2f), new Vector3(0.08f, 0.36f, 0.36f), PatchworkAssets.Felt(new Color(0.3f, 0.25f, 0.22f))));
            PointLight(kit, slab + new Vector3(2.4f, 0.6f, 0f), LampGold, 7f, 1.1f);
            Plaque(kit, new Vector3(-85.4f, 0f, -72f), 90f, "Ichthyosaur",
                "A reptile shaped like a dolphin, with eyes as big as dinner plates for hunting in the deep. Mary Anning was twelve when she and her brother Joseph dug the first complete one out of a cliff, in 1811.",
                "Nobody had a word for it. The idea that whole kinds of animal could die out and vanish was brand new, and skeletons like this one were the proof.");
            // Glowing minerals.
            Vector3 mc = DisplayCase(kit, new Vector3(-84.6f, 0f, -40f), new Vector3(2.2f, 2.4f, 3f));
            Color[] gems = { new Color(0.7f, 0.4f, 1f), new Color(0.4f, 1f, 0.8f), new Color(1f, 0.5f, 0.6f), new Color(1f, 0.85f, 0.4f) };
            for (int i = 0; i < 4; i++)
                for (int k = 0; k < 4; k++)
                    NoShadow(kit.Deco(PrimitiveType.Cube, mc + new Vector3((k % 2 - 0.5f) * 0.3f, 0.3f + k * 0.08f, -1f + i * 0.66f + (k / 2) * 0.18f), new Vector3(0.16f, 0.5f + k * 0.14f, 0.16f), PatchworkAssets.Glow(gems[i], 1.3f),
                        new Vector3(k * 14f - 20f, k * 40f, i * 9f - 12f)));
            Plaque(kit, new Vector3(-82.2f, 0f, -40f), 90f, "Crystals",
                "Nobody carved these. A crystal builds itself, one layer of atoms at a time, always at exactly the same angles. Given a few million years and a quiet crack in the rock, this is what salt water and patience can do.");
            // Footprints across the floor, heading for the door (somebody large came this way).
            var mud = PatchworkAssets.Felt(new Color(0.42f, 0.3f, 0.26f));
            for (int i = 0; i < 9; i++)
            {
                Vector3 p = new Vector3(-53.5f + (i % 2) * 1.6f, 0.03f, -48f + i * 3.6f);
                NoShadow(kit.Deco(PrimitiveType.Sphere, p, new Vector3(0.9f, 0.02f, 1.1f), mud));
                for (int t = -1; t <= 1; t++)
                    NoShadow(kit.Deco(PrimitiveType.Sphere, p + new Vector3(t * 0.36f, 0f, 0.8f - Mathf.Abs(t) * 0.14f), new Vector3(0.3f, 0.02f, 0.6f), mud, new Vector3(0f, t * 18f, 0f)));
            }
            // Pictures and maps along the walls, benches, crates by the scaffold, palms.
            int seed = 20;
            foreach (float x in new[] { -80f, -70f, -52f, -44f })
                Painting(kit, new Vector3(x, 5f, -103.9f), 0f, 3.6f, 2.6f, seed++);
            foreach (float z in new[] { -60f, -24f })
                Painting(kit, new Vector3(-87.9f, 3.4f, z), 90f, 2.8f, 2f, seed++);
            TownKit.Bench(kit, new Vector3(-42f, 0f, -62f), 270f);
            TownKit.Bench(kit, new Vector3(-70f, 0f, -45.5f), 0f);
            TownKit.Bench(kit, new Vector3(-60f, 0f, -88.6f), 180f);
            Crate(kit, new Vector3(-42.6f, 0f, -25.6f), 1.5f, 10f);
            Crate(kit, new Vector3(-41f, 0f, -24.6f), 1.1f, 40f);
            Crate(kit, new Vector3(-42.4f, 1.5f, -25.6f), 1f, -15f);
            Palm(kit, new Vector3(-85.5f, 0f, -90f), 3.4f, 3);
            Palm(kit, new Vector3(-36f, 0f, -12f), 3f, 5);
            // A row of ship models on the gallery, and a great hanging map of the winds.
            for (int i = 0; i < 3; i++)
            {
                Vector3 sc = DisplayCase(kit, new Vector3(-33f, 8f, -24f + i * 4.6f), new Vector3(1.6f, 1.9f, 2.6f), 0f, i == 1);
                NoShadow(kit.Deco(PrimitiveType.Sphere, sc + Vector3.up * 0.22f, new Vector3(0.4f, 0.3f, 1.6f), DarkWood));
                NoShadow(kit.Deco(PrimitiveType.Cube, sc + Vector3.up * 0.7f, new Vector3(0.03f, 0.8f, 0.03f), DarkWood));
                NoShadow(kit.Deco(PrimitiveType.Cube, sc + new Vector3(0f, 0.66f, 0f), new Vector3(0.02f, 0.5f, 0.8f - i * 0.16f), PatchworkAssets.Felt(i == 1 ? Wine : CreamCol)));
            }
            Painting(kit, new Vector3(-60f, 12f, -103.9f), 0f, 9f, 4f, 99);
            // A stitched timeline down the middle of the floor: a gold thread with a knot every so often.
            var gold = new MeshBatch();
            gold.Stitches(new Vector3(-54f, 0.03f, -100f), new Vector3(-54f, 0.03f, -14f), 0.7f, 0.5f, 0.12f, 0.02f);
            for (float z = -98f; z < -14f; z += 12f) gold.Ball(new Vector3(-54f, 0.04f, z), new Vector3(0.7f, 0.05f, 0.7f), 2, 8);
            gold.Build(kit.Root, "Timeline", PatchworkAssets.Felt(new Color(1f, 0.82f, 0.4f)));
            // Dust in the lamplight.
            Motes(kit, new Vector3(-60f, 8f, -57f), new Vector3(50f, 14f, 90f), 70, new Color(1f, 0.9f, 0.7f), 1.6f, 0.07f, new Vector3(0.05f, -0.08f, 0.04f));
        }

        // The Kelp Galleries.
        static void KelpDetails(LevelKit kit)
        {
            // Two sea otters afloat on their backs, holding paws.
            var otters = kit.Group("Otters", new Vector3(-48f, PoolSurface + 0.1f, 25f));
            otters.rotation = Rot(30f);
            var fur = PatchworkAssets.Yarn(new Color(0.5f, 0.36f, 0.26f));
            var face = PatchworkAssets.Yarn(new Color(0.85f, 0.75f, 0.6f));
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Capsule, new Vector3(s * 0.5f, 0f, 0f), new Vector3(0.5f, 0.7f, 0.4f), fur, new Vector3(90f, 0f, 0f), otters);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.5f, 0.1f, 0.8f), Vector3.one * 0.46f, face, default(Vector3), otters);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.5f, 0.16f, 1.02f), Vector3.one * 0.1f, PatchworkAssets.Plain(new Color(0.1f, 0.08f, 0.08f), 0.7f), default(Vector3), otters);
                for (int e = -1; e <= 1; e += 2)
                    kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.5f + e * 0.2f, 0.3f, 0.74f), Vector3.one * 0.12f, fur, default(Vector3), otters);
                kit.Deco(PrimitiveType.Capsule, new Vector3(s * 0.5f, 0f, -0.9f), new Vector3(0.2f, 0.4f, 0.14f), fur, new Vector3(90f, 0f, 0f), otters);
                kit.Deco(PrimitiveType.Capsule, new Vector3(s * 0.14f, 0.16f, 0.2f), new Vector3(0.14f, 0.2f, 0.14f), fur, new Vector3(0f, 0f, 90f), otters);
            }
            var ob = otters.gameObject.AddComponent<SeaBob>();
            ob.Height = 0.05f;
            ob.Rock = 2f;
            // The keepers' rowing boat, glass floats and nets on the far wall.
            Boat(kit, new Vector3(-83f, PoolSurface - 0.1f, 55f), 80f, 4f, new Color(0.3f, 0.55f, 0.5f), false);
            Color[] floats = { new Color(0.4f, 0.9f, 0.8f), new Color(0.5f, 0.7f, 1f), new Color(0.9f, 0.8f, 0.4f) };
            // (On the foyer's south wall, to the west of the doorway.)
            for (int i = 0; i < 7; i++)
            {
                Vector3 p = new Vector3(-85f + i * 2.6f + (i % 2) * 0.5f, 7f + (i % 3) * 1.6f, -3.5f);
                NoShadow(kit.Deco(PrimitiveType.Sphere, p, Vector3.one * (0.9f + (i % 3) * 0.3f), PatchworkAssets.Glow(floats[i % 3], 0.9f)));
                kit.Rope(p + Vector3.up * 0.5f, new Vector3(p.x, 13f, -3.9f), PatchworkAssets.Yarn(new Color(0.6f, 0.5f, 0.35f)), 0.05f);
            }
            var net = new MeshBatch();
            for (int i = 0; i <= 8; i++)
            {
                net.Stitches(new Vector3(-87f + i * 2f, 13f, -3.8f), new Vector3(-85f + i * 2f, 5f + Mathf.Sin(i * 0.9f) * 1.2f, -3.8f), 0.4f, 0.1f, 0.05f, 0.03f);
                net.Stitches(new Vector3(-85f + i * 2f, 13f, -3.8f), new Vector3(-87f + i * 2f, 5f + Mathf.Sin(i * 0.9f) * 1.2f, -3.8f), 0.4f, 0.1f, 0.05f, 0.03f);
            }
            net.Build(kit.Root, "Nets", PatchworkAssets.Yarn(new Color(0.6f, 0.5f, 0.35f)));
            // A touch pool on the foyer: a low round basin of starfish and urchins.
            Vector3 tp = new Vector3(-74f, 0f, 4f);
            RoundPlatform(kit, tp + Vector3.up * 0.9f, 2.6f, 0.9f, PatchworkAssets.Glow(new Color(0.2f, 0.6f, 0.65f), 0.5f), PaleStone, false);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, tp + Vector3.up * 0.9f, new Vector3(5.5f, 0.1f, 5.5f), Brass));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, tp + Vector3.up * 0.93f, new Vector3(4.9f, 0.1f, 4.9f), PatchworkAssets.Glow(new Color(0.2f, 0.6f, 0.65f), 0.5f)));
            Color[] stars = { new Color(1f, 0.5f, 0.3f), new Color(0.9f, 0.4f, 0.7f), new Color(1f, 0.8f, 0.3f) };
            for (int i = 0; i < 6; i++)
            {
                Vector3 p = tp + new Vector3(Mathf.Cos(i * 1.1f) * (0.6f + (i % 3) * 0.6f), 1f, Mathf.Sin(i * 1.1f) * (0.6f + (i % 3) * 0.6f));
                var m = PatchworkAssets.Felt(stars[i % 3]);
                if (i % 3 == 2) NoShadow(kit.Deco(PrimitiveType.Sphere, p, new Vector3(0.5f, 0.3f, 0.5f), PatchworkAssets.Yarn(new Color(0.3f, 0.2f, 0.4f))));
                else
                    for (int arm = 0; arm < 5; arm++)
                        NoShadow(kit.Deco(PrimitiveType.Sphere, p + Quaternion.Euler(0f, arm * 72f + i * 20f, 0f) * new Vector3(0f, 0f, 0.24f), new Vector3(0.2f, 0.1f, 0.6f), m, new Vector3(0f, arm * 72f + i * 20f, 0f)));
            }
            PointLight(kit, tp + Vector3.up * 2.4f, new Color(0.4f, 0.95f, 0.9f), 8f, 1.3f);
            Plaque(kit, new Vector3(-74f, 0f, 7.6f), 0f, "The Touch Pool",
                "A starfish has no brain and no blood: it pumps sea water round its body instead. If it loses an arm, it grows another. Some kinds can grow a whole new starfish from the arm.");
            // Two tall columns of seahorses on the far terrace.
            for (int i = 0; i < 2; i++)
            {
                Vector3 c = new Vector3(-80f + i * 10f, 0f, 71f);
                kit.Block(c + Vector3.up * 0.5f, new Vector3(2.4f, 1f, 2.4f), StoneCol, "SeahorseBase", DarkWood);
                NoShadow(kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 3.2f, new Vector3(2f, 2.2f, 2f), PatchworkAssets.Glow(new Color(0.12f, 0.5f, 0.55f), 0.8f)));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 3.2f, new Vector3(2.14f, 2.2f, 2.14f), GlassMat));
                kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 5.5f, new Vector3(2.4f, 0.12f, 2.4f), Brass);
                kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 1.05f, new Vector3(2.4f, 0.08f, 2.4f), Brass);
                TownKit.Solid(kit, c + Vector3.up * 3f, new Vector3(2f, 5f, 2f), 0f);
                for (int k = 0; k < 3; k++)
                {
                    var horse = kit.Group("Seahorse", c + new Vector3(Mathf.Cos(k * 2.1f) * 0.55f, 2f + k * 1f, Mathf.Sin(k * 2.1f) * 0.55f - 0.5f));
                    var hm = PatchworkAssets.Glow(k % 2 == 0 ? new Color(1f, 0.8f, 0.3f) : new Color(1f, 0.55f, 0.6f), 0.9f);
                    for (int b = 0; b < 6; b++)
                        NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Sin(b * 0.9f) * 0.08f, b * 0.13f, Mathf.Cos(b * 0.8f) * 0.06f), Vector3.one * (0.12f + Mathf.Sin(b / 5f * Mathf.PI) * 0.1f), hm, default(Vector3), horse));
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.74f, 0.12f), new Vector3(0.06f, 0.06f, 0.22f), hm, default(Vector3), horse));
                    var jb = horse.gameObject.AddComponent<JellyBob>();
                    jb.Rise = 0.25f;
                    jb.Drift = 0.08f;
                    jb.Phase = k * 1.7f + i;
                }
                PointLight(kit, c + new Vector3(0f, 3f, -2f), new Color(0.3f, 0.9f, 0.85f), 8f, 1.2f);
            }
            Plaque(kit, new Vector3(-75f, 0f, 68.8f), 180f, "Seahorses",
                "A fish that swims standing up, holds on to the weed with its tail so it isn't swept away, and can look two ways at once. And it is the father, not the mother, who carries the babies: up to a thousand at a time.");
            // Weed hanging from the roof beams, plankton in the air.
            for (int i = 0; i < 16; i++)
                Vine(kit, new Vector3(-84f + (i * 13) % 50, KelpTop - 1f, 8f + (i * 7) % 64), 4f + (i % 4) * 1.6f, KelpCols[i % KelpCols.Length], 40 + i);
            Motes(kit, new Vector3(-60f, 7f, 38f), new Vector3(52f, 14f, 42f), 80, new Color(0.6f, 1f, 0.85f), 2.2f, 0.09f, new Vector3(0.1f, 0.14f, 0.05f));
            // Lily-lamps afloat on the pool.
            for (int i = 0; i < 7; i++)
            {
                var lily = kit.Group("LilyLamp", new Vector3(-84f + (i * 17) % 50, PoolSurface + 0.06f, 21f + (i * 11) % 34));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.9f, 0.03f, 0.9f), PatchworkAssets.Felt(new Color(0.2f, 0.5f, 0.4f)), default(Vector3), lily));
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.16f, 0f), new Vector3(0.34f, 0.3f, 0.34f), PatchworkAssets.Glow(i % 2 == 0 ? new Color(1f, 0.8f, 0.9f) : new Color(1f, 0.95f, 0.7f), 2f), default(Vector3), lily));
                var lb = lily.gameObject.AddComponent<SeaBob>();
                lb.Height = 0.04f;
                lb.Phase = i * 1.3f;
            }
            Palm(kit, new Vector3(-86f, 0f, -1.5f), 3f, 11);
            Palm(kit, new Vector3(-34.5f, 0f, -1.5f), 3f, 12);
        }

        // The Great Deep and its tunnel.
        static void DeepDetails(LevelKit kit)
        {
            // A bathysphere resting on the sea bed, its porthole lit.
            Vector3 bs = new Vector3(18f, DeepFloor + 2.2f, 54f);
            var steel = PatchworkAssets.Plain(new Color(0.75f, 0.78f, 0.82f), 0.7f);
            kit.Deco(PrimitiveType.Sphere, bs, Vector3.one * 4.2f, steel);
            for (int i = 0; i < 3; i++)
            {
                var r = Quaternion.Euler(0f, i * 120f + 20f, 0f);
                kit.Deco(PrimitiveType.Cylinder, bs + r * new Vector3(0f, -1.7f, 1.5f), new Vector3(0.3f, 0.6f, 0.3f), IronMat);
                NoShadow(kit.Deco(PrimitiveType.Cylinder, bs + r * new Vector3(0f, 0.3f, 2.02f), new Vector3(1.1f, 0.06f, 1.1f), i == 0 ? PatchworkAssets.Glow(new Color(1f, 0.9f, 0.6f), 2.6f) : PatchworkAssets.Glow(new Color(0.5f, 0.8f, 1f), 1.2f), new Vector3(90f, i * 120f + 20f, 0f)));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, bs + r * new Vector3(0f, 0.3f, 1.98f), new Vector3(1.4f, 0.06f, 1.4f), Brass, new Vector3(90f, i * 120f + 20f, 0f)));
            }
            kit.Deco(PrimitiveType.Cylinder, bs + Vector3.up * 2.2f, new Vector3(0.9f, 0.2f, 0.9f), IronMat);
            Beam(kit, bs + Vector3.up * 2.3f, bs + new Vector3(1.5f, DeepTop - bs.y, 2f), 0.1f, IronMat);
            PointLight(kit, bs + Rot(20f) * new Vector3(0f, 0.6f, 4f), new Color(1f, 0.9f, 0.6f), 13f, 1.8f);
            Plaque(kit, new Vector3(22f, 0f, TunnelZ - 3.1f), 0f, "The Bathysphere",
                "A hollow steel ball on the end of a cable. In 1934 William Beebe and Otis Barton squeezed inside one, were bolted in, and were lowered half a mile down into the dark off Bermuda.",
                "Beebe described what he saw by telephone to the ship above: fish with lights along their sides, like liners passing in the night. Nobody believed half of it until much later, when it all turned out to be true.");
            // A giant squid in the far corner, watching, its arms stirring.
            Vector3 sq = new Vector3(32f, 6f, 93f);
            var squid = kit.Group("GiantSquid", sq);
            squid.rotation = Quaternion.Euler(-20f, 215f, 0f);
            var red = PatchworkAssets.Glow(new Color(0.85f, 0.3f, 0.3f), 0.35f);
            var pale = PatchworkAssets.Glow(new Color(0.95f, 0.75f, 0.7f), 0.35f);
            NoShadow(kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0f, -3.4f), new Vector3(2.4f, 3.6f, 2.4f), red, new Vector3(90f, 0f, 0f), squid));
            for (int s = -1; s <= 1; s += 2)
            {
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 1.5f, 0f, -6f), new Vector3(2f, 0.2f, 2.6f), red, new Vector3(0f, 0f, s * 20f), squid));
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 1.05f, 0.3f, 0.3f), new Vector3(0.5f, 1.1f, 1.1f), PatchworkAssets.Glow(new Color(1f, 0.95f, 0.8f), 1.2f), default(Vector3), squid));
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 1.25f, 0.3f, 0.36f), new Vector3(0.2f, 0.56f, 0.56f), PatchworkAssets.Plain(new Color(0.05f, 0.05f, 0.08f), 0.9f), default(Vector3), squid));
            }
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, 0.4f), new Vector3(2.2f, 2f, 2.2f), red, default(Vector3), squid));
            for (int i = 0; i < 8; i++)
            {
                float a = i / 8f * Mathf.PI * 2f;
                var armRoot = new GameObject("Arm").transform;
                armRoot.SetParent(squid, false);
                armRoot.localPosition = new Vector3(Mathf.Cos(a) * 0.7f, Mathf.Sin(a) * 0.7f, 1.2f);
                armRoot.localRotation = Quaternion.Euler(Mathf.Sin(a) * -22f + 90f, 0f, Mathf.Cos(a) * -22f);
                bool longArm = i % 4 == 0;
                int segs = longArm ? 6 : 4;
                var chain = new Transform[segs];
                Transform parent = armRoot;
                for (int k = 0; k < segs; k++)
                {
                    var seg = new GameObject("Seg").transform;
                    seg.SetParent(parent, false);
                    seg.localPosition = k == 0 ? Vector3.zero : new Vector3(0f, 1.9f, 0f);
                    float w = 0.5f - k * (longArm ? 0.06f : 0.1f);
                    NoShadow(kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.95f, 0f), new Vector3(w, 1.05f, w), k % 2 == 0 ? red : pale, default(Vector3), seg));
                    chain[k] = seg;
                    parent = seg;
                }
                var sway = armRoot.gameObject.AddComponent<KelpSway>();
                sway.Segments = chain;
                sway.Amount = longArm ? 10f : 7f;
                sway.Speed = 0.5f + (i % 3) * 0.12f;
                sway.Phase = i * 0.9f;
            }
            PointLight(kit, sq + new Vector3(-4f, 1f, -5f), new Color(1f, 0.5f, 0.5f), 14f, 1.2f);
            // Black smokers: chimneys on the sea bed, glowing at the root and streaming bubbles.
            foreach (var p in new[] { new Vector3(-22f, DeepFloor, 90f), new Vector3(-18f, DeepFloor, 94f), new Vector3(34f, DeepFloor, 32f) })
            {
                var chimney = new MeshBatch();
                for (int k = 0; k < 5; k++)
                    chimney.Ball(p + new Vector3(Mathf.Sin(k * 1.9f) * 0.3f, 0.7f + k * 1.1f, Mathf.Cos(k * 2.3f) * 0.3f), new Vector3(2f - k * 0.3f, 1.6f, 2f - k * 0.3f), 3, 7);
                chimney.Build(kit.Root, "Smoker", PatchworkAssets.Felt(new Color(0.14f, 0.13f, 0.18f)));
                NoShadow(kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 0.3f, new Vector3(3.4f, 0.8f, 3.4f), PatchworkAssets.Glow(new Color(1f, 0.4f, 0.2f), 1.6f)));
                Bubbles(kit, p + Vector3.up * 6f, 16f, 10);
                PointLight(kit, p + Vector3.up * 1.5f, new Color(1f, 0.45f, 0.25f), 10f, 1.5f);
                // Tube worms round it.
                var worms = new MeshBatch();
                var tips = new MeshBatch();
                for (int k = 0; k < 12; k++)
                {
                    Vector3 w = p + Quaternion.Euler(0f, k * 30f, 0f) * new Vector3(2f + (k % 3) * 0.5f, 0f, 0f);
                    float h = 0.8f + (k % 4) * 0.4f;
                    worms.Cylinder(w, 0.09f, h, 4);
                    tips.Ball(w + Vector3.up * (h + 0.12f), new Vector3(0.2f, 0.36f, 0.2f), 2, 5);
                }
                worms.Build(kit.Root, "TubeWorms", PatchworkAssets.Felt(CreamCol));
                tips.Build(kit.Root, "WormTips", PatchworkAssets.Glow(new Color(1f, 0.3f, 0.3f), 1f));
            }
            // An old anchor and its chain, a fallen statue, crabs.
            var anchor = kit.Group("SeaAnchor", new Vector3(-6f, DeepFloor + 0.3f, 50f));
            anchor.rotation = Quaternion.Euler(70f, 40f, 0f);
            var rust = PatchworkAssets.Felt(new Color(0.45f, 0.3f, 0.25f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.8f, 0f), new Vector3(0.3f, 1.8f, 0.3f), rust, default(Vector3), anchor);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.7f, 0.4f, 0f), new Vector3(1.6f, 0.3f, 0.3f), rust, new Vector3(0f, 0f, s * 34f), anchor);
            for (int i = 0; i < 12; i++)
                kit.Deco(PrimitiveType.Sphere, new Vector3(-6f + i * 0.5f + 2.6f, DeepFloor + 0.2f + (i > 8 ? (i - 8) * 0.9f : 0f), 53f + Mathf.Sin(i * 0.7f) * 0.6f), new Vector3(0.5f, 0.3f, 0.36f), rust, new Vector3(0f, i * 40f, 0f));
            Color[] crabCols = { new Color(1f, 0.45f, 0.3f), new Color(0.9f, 0.3f, 0.4f) };
            for (int i = 0; i < 5; i++)
            {
                Vector3 c = new Vector3(-20f + i * 11f, DeepFloor + 0.3f, i % 2 == 0 ? 58f : 74.5f);
                var crab = kit.Group("Crab", c);
                var cm = PatchworkAssets.Glow(crabCols[i % 2], 0.5f);
                NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.6f, 0.3f, 0.9f), cm, default(Vector3), crab));
                for (int s = -1; s <= 1; s += 2)
                {
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.2f, 0.1f, 0.6f), Vector3.one * 0.26f, cm, default(Vector3), crab));
                    for (int l = 0; l < 3; l++)
                        NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.4f, -0.1f, -0.2f + l * 0.2f), new Vector3(0.4f, 0.05f, 0.05f), cm, new Vector3(0f, 0f, s * -30f), crab));
                }
                var walk = crab.gameObject.AddComponent<FishSwim>();
                walk.Center = c;
                walk.RadiusX = 2.5f + i * 0.4f;
                walk.RadiusZ = 0.8f;
                walk.Speed = (i % 2 == 0 ? 1f : -1f) * 22f;
                walk.Bob = 0f;
                walk.Wiggle = 10f;
                walk.Bank = 0f;
                walk.Angle = i * 70f;
            }
            // Marine snow, drifting down past the glass.
            Motes(kit, new Vector3(6f, 4f, TunnelZ), new Vector3(70f, 14f, 30f), 120, new Color(0.75f, 0.9f, 1f), 2f, 0.08f, new Vector3(0.12f, -0.22f, 0.04f));
            // Signs of the deep along the tunnel: little brass depth markers on the ribs.
            for (float x = -26f; x <= 38f; x += 9f)
            {
                if (x > -4f && x < 16f) continue;
                NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(x, 0.5f, TunnelZ + 3.66f), new Vector3(0.5f, 0.03f, 0.5f), Brass, new Vector3(90f, 0f, 0f)));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(x, 0.5f, TunnelZ + 3.62f), new Vector3(0.36f, 0.03f, 0.36f), PatchworkAssets.Glow(new Color(0.4f, 0.9f, 1f), 1.6f), new Vector3(90f, 0f, 0f)));
            }
        }

        // The Sunken Ruins.
        static void RuinsDetails(LevelKit kit)
        {
            // Vines hanging from the broken roof.
            for (int i = 0; i < 26; i++)
                Vine(kit, new Vector3(47f + (i * 17) % 50, RuinTop - 0.6f, 9f + (i * 29) % 88), 5f + (i % 5) * 2f, i % 3 == 0 ? new Color(0.45f, 0.7f, 0.4f) : MossCol, 90 + i);
            // Two lion-headed spouts pouring into the pool.
            foreach (var w in new[] { new Vector4(99.9f, 9f, 76f, 270f), new Vector4(74f, 9f, 99.9f, 180f), new Vector4(99.9f, 9f, 24f + 5f, 270f) })
            {
                Vector3 at = new Vector3(w.x, w.y, w.z);
                var r = Rot(w.w);
                var head = kit.Group("LionSpout", at);
                head.rotation = r;
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, 0.4f), new Vector3(2.4f, 2.4f, 1.6f), RuinStone, default(Vector3), head);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.3f, 1.1f), new Vector3(1.2f, 0.9f, 0.9f), RuinStone, default(Vector3), head);
                for (int i = 0; i < 9; i++)
                    kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(i * 0.7f + 0.3f) * 1.3f, Mathf.Sin(i * 0.7f + 0.3f) * 1.3f + 0.1f, 0.2f), Vector3.one * 0.9f, RuinDark, default(Vector3), head);
                for (int s = -1; s <= 1; s += 2)
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.5f, 0.36f, 1.1f), Vector3.one * 0.24f, PatchworkAssets.Glow(new Color(0.5f, 1f, 0.8f), 1.6f), default(Vector3), head));
                // The falling water: a pale ribbon, bubbles tumbling down it, foam where it lands.
                Vector3 lip = at + r * new Vector3(0f, -0.7f, 1.7f);
                float drop = lip.y - RuinSurface;
                NoShadow(kit.Deco(PrimitiveType.Cube, lip + Vector3.down * drop * 0.5f + r * new Vector3(0f, 0f, 0.3f), new Vector3(1f, drop, 0.2f), Shaft(new Color(0.7f, 1f, 0.95f), 0.22f), new Vector3(0f, w.w, 0f)));
                var falls = kit.Group("Falls", lip + r * new Vector3(0f, 0f, 0.3f));
                var fm = PatchworkAssets.Glow(new Color(0.85f, 1f, 1f), 1f);
                var drops = new Transform[10];
                for (int i = 0; i < drops.Length; i++) drops[i] = NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.2f, fm, default(Vector3), falls));
                var fs = falls.gameObject.AddComponent<BubbleStream>();
                fs.Bubbles = drops;
                fs.Height = -drop;
                fs.Speed = -7f;   // (falling: the stream runs downwards)
                fs.Wobble = 0.3f;
                NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(lip.x, RuinSurface + 0.03f, lip.z) + r * new Vector3(0f, 0f, 0.3f), new Vector3(3f, 0.02f, 3f), PatchworkAssets.Glow(new Color(0.85f, 1f, 1f), 0.8f)));
                AmbientEmitter.Create(kit.Root, lip, Sfx.Ambient(Sfx.AmbientClip.Fountain), 0.5f, 5f, 26f);
            }
            // A great stone hand reaching up out of the water.
            Vector3 hand = new Vector3(90f, RuinSurface - 0.6f, 88f);
            var hg = kit.Group("StoneHand", hand);
            hg.rotation = Quaternion.Euler(-12f, 30f, 10f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.6f, 0f), new Vector3(2.6f, 3.2f, 1f), RuinStone, default(Vector3), hg);
            for (int i = 0; i < 4; i++)
            {
                kit.Deco(PrimitiveType.Capsule, new Vector3(-0.96f + i * 0.64f, 4.2f + (i == 1 || i == 2 ? 0.3f : 0f), 0.1f), new Vector3(0.56f, 1.1f, 0.6f), RuinStone, new Vector3(10f + i * 4f, 0f, 0f), hg);
                kit.Deco(PrimitiveType.Capsule, new Vector3(-0.96f + i * 0.64f, 5.6f + (i == 1 || i == 2 ? 0.3f : 0f), 0.5f), new Vector3(0.5f, 0.7f, 0.54f), RuinStone, new Vector3(34f + i * 5f, 0f, 0f), hg);
            }
            kit.Deco(PrimitiveType.Capsule, new Vector3(1.7f, 2.6f, 0.3f), new Vector3(0.6f, 1f, 0.64f), RuinStone, new Vector3(10f, 0f, -40f), hg);
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0.4f, 2.4f, -0.5f), new Vector3(1.6f, 1.2f, 0.2f), PatchworkAssets.Felt(MossCol), default(Vector3), hg));
            TownKit.Solid(kit, hand + Vector3.up * 3f, new Vector3(2.6f, 6f, 1.4f), 30f);
            // Floating candles and lotus flowers on the pool.
            for (int i = 0; i < 14; i++)
            {
                Vector3 p = new Vector3(47f + (i * 23) % 50, RuinSurface + 0.05f, 27f + (i * 31) % 70);
                if (Vector3.Distance(p, new Vector3(75f, p.y, 58f)) < 12f) p.x += 16f;
                if (p.x > 98f) p.x -= 40f;
                var fl = kit.Group("Lotus", p);
                bool candle = i % 3 == 0;
                if (candle)
                {
                    NoShadow(kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.6f, 0.04f, 0.6f), PatchworkAssets.Felt(new Color(0.5f, 0.38f, 0.26f)), default(Vector3), fl));
                    NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.14f, 0f), new Vector3(0.16f, 0.12f, 0.16f), PatchworkAssets.Felt(CreamCol), default(Vector3), fl));
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.36f, 0f), new Vector3(0.12f, 0.22f, 0.12f), PatchworkAssets.Glow(new Color(1f, 0.7f, 0.3f), 3f), default(Vector3), fl));
                }
                else
                {
                    NoShadow(kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.1f, 0.03f, 1.1f), PatchworkAssets.Felt(new Color(0.22f, 0.5f, 0.38f)), default(Vector3), fl));
                    for (int k = 0; k < 6; k++)
                        NoShadow(kit.Deco(PrimitiveType.Sphere, Quaternion.Euler(0f, k * 60f, 0f) * new Vector3(0f, 0.14f, 0.16f), new Vector3(0.16f, 0.1f, 0.36f), PatchworkAssets.Glow(new Color(1f, 0.75f, 0.9f), 0.9f), new Vector3(-30f, k * 60f, 0f), fl));
                }
                var lb = fl.gameObject.AddComponent<SeaBob>();
                lb.Height = 0.04f;
                lb.Phase = i * 1.9f;
            }
            // A mosaic on the temple floor, runes of light on its columns, jars on the terrace.
            var gold = PatchworkAssets.Felt(new Color(0.9f, 0.78f, 0.45f));
            var mosaic = kit.Group("Mosaic", new Vector3(75f, 1.23f, 58f));
            Hoop(kit, mosaic, 5.6f, 0.12f, gold, 40);
            Hoop(kit, mosaic, 3.4f, 0.1f, PatchworkAssets.Felt(new Color(0.3f, 0.6f, 0.6f)), 30);
            for (int i = 0; i < 8; i++)
                NoShadow(kit.Deco(PrimitiveType.Cube, Quaternion.Euler(0f, i * 45f, 0f) * new Vector3(0f, 0f, 4.5f), new Vector3(0.5f, 0.02f, 1.6f), i % 2 == 0 ? gold : PatchworkAssets.Felt(new Color(0.3f, 0.6f, 0.6f)), new Vector3(0f, i * 45f, 0f), mosaic));
            var rune = PatchworkAssets.Glow(new Color(0.5f, 1f, 0.85f), 1.6f);
            foreach (var c in new[] { new Vector3(69.2f, 1.2f, 51.2f), new Vector3(75f, 1.2f, 51.2f), new Vector3(80.8f, 1.2f, 58f), new Vector3(75f, 1.2f, 64.8f) })
                for (int k = 0; k < 5; k++)
                    NoShadow(kit.Deco(PrimitiveType.Cube, c + new Vector3(0f, 2f + k * 0.7f, -0.72f), new Vector3(k % 2 == 0 ? 0.5f : 0.3f, 0.1f, 0.04f), rune, new Vector3(0f, 0f, k * 30f)));
            foreach (var p in new[] { new Vector3(96f, 0f, 20f), new Vector3(97.2f, 0f, 18.4f), new Vector3(47f, 0f, 21f), new Vector3(84f, 0f, 21.4f) })
            {
                var clay = PatchworkAssets.Felt(new Color(0.78f, 0.5f, 0.36f));
                kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 0.7f, new Vector3(1.1f, 1.4f, 1.1f), clay);
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 1.5f, new Vector3(0.5f, 0.2f, 0.5f), clay);
                TownKit.Solid(kit, p + Vector3.up * 0.8f, new Vector3(0.9f, 1.6f, 0.9f), 0f);
            }
            Palm(kit, new Vector3(97f, 0f, 8.6f), 3.4f, 21);
            // On the wreck: a net over the side, a ship's lantern, an anchor on the bow.
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(92f, 3.6f, 51.4f), Vector3.one * 0.5f, PatchworkAssets.Glow(LampGold, 2.6f)));
            PointLight(kit, new Vector3(92f, 3.6f, 50.6f), LampGold, 10f, 1.3f);
            var net = new MeshBatch();
            for (int i = 0; i <= 8; i++)
            {
                net.Stitches(new Vector3(86.9f, 2.4f, 40f + i * 1.2f), new Vector3(86.6f, -0.4f, 41.2f + i * 1.2f), 0.3f, 0.1f, 0.05f, 0.03f);
                net.Stitches(new Vector3(86.9f, 2.4f, 41.2f + i * 1.2f), new Vector3(86.6f, -0.4f, 40f + i * 1.2f), 0.3f, 0.1f, 0.05f, 0.03f);
            }
            net.Build(kit.Root, "WreckNet", PatchworkAssets.Yarn(new Color(0.6f, 0.52f, 0.36f)));
            // Mist over the water.
            Motes(kit, new Vector3(72f, 3f, 60f), new Vector3(54f, 7f, 76f), 90, new Color(0.7f, 1f, 0.85f), 1.8f, 0.09f, new Vector3(0.14f, 0.05f, 0.1f));
        }

        // The Orrery Bridge and the Observatory.
        static void SkyDetails(LevelKit kit)
        {
            // ----- The bridge -----
            // Planets hung from the ceiling the whole length of the hall, each slowly turning.
            Color[] worlds = { new Color(0.8f, 0.6f, 0.4f), new Color(0.95f, 0.8f, 0.5f), new Color(0.4f, 0.6f, 1f), new Color(1f, 0.5f, 0.35f), new Color(0.9f, 0.75f, 0.55f), new Color(0.95f, 0.85f, 0.6f), new Color(0.6f, 0.9f, 0.95f), new Color(0.4f, 0.5f, 1f) };
            float[] sizes = { 0.7f, 1.1f, 1.2f, 0.9f, 2.6f, 2.2f, 1.6f, 1.5f };
            var planets = kit.Group("HangingPlanets", Vector3.zero);
            var list = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < worlds.Length; i++)
            {
                Vector3 p = new Vector3(-2f + i * 5.6f, 23.5f + (i % 3) * 1.4f, 9f + (i % 2) * 6f);
                var body = new GameObject("Planet").transform;
                body.SetParent(planets, false);
                body.localPosition = p;
                NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * sizes[i], PatchworkAssets.Glow(worlds[i], 0.8f), default(Vector3), body));
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, sizes[i] * 0.12f, 0f), new Vector3(sizes[i] * 1.02f, sizes[i] * 0.16f, sizes[i] * 1.02f), PatchworkAssets.Glow(Color.Lerp(worlds[i], Color.white, 0.4f), 0.8f), default(Vector3), body));
                if (i == 5)
                {
                    var ring = new GameObject("Ring").transform;
                    ring.SetParent(body, false);
                    Hoop(kit, ring, sizes[i] * 0.9f, 0.14f, Brass, 20);
                }
                Beam(kit, p + Vector3.up * sizes[i] * 0.5f, new Vector3(p.x, 30f, p.z), 0.03f, Brass);
                list.Add(body);
            }
            var pm = planets.gameObject.AddComponent<StarMobile>();
            pm.Stars = list.ToArray();
            pm.Turn = 10f;
            pm.Bob = 0.1f;
            pm.Tilt = 3f;
            // The Moon in the corner of the west landing, with a plaque.
            Vector3 mn = new Vector3(-3.6f, DeckY, 15.2f);
            Plinth(kit, mn, new Vector3(2.4f, 0.6f, 2.4f), 45f);
            NoShadow(kit.Deco(PrimitiveType.Sphere, mn + Vector3.up * 2.6f, Vector3.one * 3.4f, PatchworkAssets.Glow(new Color(0.9f, 0.9f, 0.85f), 0.7f)));
            var crater = PatchworkAssets.Glow(new Color(0.7f, 0.7f, 0.68f), 0.5f);
            Vector3[] dents = { new Vector3(0.4f, 0.5f, -0.76f), new Vector3(-0.6f, 0.1f, -0.8f), new Vector3(0.1f, -0.4f, -0.9f), new Vector3(0.8f, -0.2f, -0.56f), new Vector3(-0.3f, 0.8f, -0.5f), new Vector3(-0.8f, -0.5f, -0.3f) };
            foreach (var d in dents)
                NoShadow(kit.Deco(PrimitiveType.Sphere, mn + Vector3.up * 2.6f + d.normalized * 1.62f, new Vector3(0.7f, 0.7f, 0.2f), crater)).rotation = Quaternion.LookRotation(d.normalized);
            TownKit.Solid(kit, mn + Vector3.up * 2.4f, new Vector3(2.6f, 3.8f, 2.6f), 0f);
            PointLight(kit, mn + new Vector3(2f, 2.6f, -3f), new Color(0.9f, 0.92f, 1f), 9f, 1.2f);
            Plaque(kit, new Vector3(-0.6f, DeckY, 15.2f), 90f, "The Moon",
                "In July 1969 two people climbed down a ladder and walked about on it. There is no wind there and no rain, so their footprints are still exactly as they left them, and will be for millions of years.",
                "The Moon is also why this museum is where it is. Twice a day it pulls the whole sea up towards itself, and lets it go again.");
            // Star-chart tables on the east landing, and a comet under the roof.
            foreach (var tpos in new[] { new Vector3(40.4f, DeckY, 16.7f), new Vector3(36f, DeckY, 6.9f) })
            {
                kit.Block(tpos + Vector3.up * 0.6f, new Vector3(3f, 1.2f, 1.6f), WoodCol, "ChartTable", DarkWood);
                NoShadow(kit.Deco(PrimitiveType.Cube, tpos + Vector3.up * 1.22f, new Vector3(2.4f, 0.03f, 1.2f), StarCloth, new Vector3(0f, 6f, 0f)));
                NoShadow(kit.Deco(PrimitiveType.Sphere, tpos + new Vector3(1f, 1.6f, 0.2f), Vector3.one * 0.5f, PatchworkAssets.Glow(LampGold, 2.2f)));
            }
            Shooting(kit, new Vector3(-4f, 28.6f, 8f), new Vector3(42f, 27.6f, 16f), new Vector3(0f, 0.6f, 6f), 12f, 3f, 5f, 0.16f, new Color(0.8f, 0.9f, 1f));
            Motes(kit, new Vector3(20f, 20f, 12f), new Vector3(44f, 16f, 11f), 60, new Color(0.85f, 0.85f, 1f), 2.4f, 0.09f, new Vector3(0.08f, -0.12f, 0.03f));

            // ----- The Observatory -----
            Vector3 C = Observatory;
            // Armchairs, a rug and a tea table by the east windows.
            Vector3 nook = C + new Vector3(15f, 0f, 6f);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, nook + Vector3.up * 0.03f, new Vector3(7f, 0.02f, 7f), PlumStars));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, nook + Vector3.up * 0.04f, new Vector3(4.4f, 0.02f, 4.4f), StarCloth));
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 ch = nook + new Vector3(s * 1.9f, 0f, 0f);
                var velvet = PatchworkAssets.Fabric(s > 0 ? Wine : new Color(0.2f, 0.36f, 0.4f));
                kit.Block(ch + Vector3.up * 0.4f, new Vector3(1.5f, 0.8f, 1.5f), WoodCol, "Armchair", velvet);
                kit.Deco(PrimitiveType.Cube, ch + new Vector3(s * 0.66f, 1.1f, 0f), new Vector3(0.36f, 1.5f, 1.5f), velvet);
                for (int a = -1; a <= 1; a += 2)
                    kit.Deco(PrimitiveType.Capsule, ch + new Vector3(0f, 0.95f, a * 0.66f), new Vector3(0.34f, 0.7f, 0.34f), velvet, new Vector3(0f, 0f, 90f));
            }
            kit.Deco(PrimitiveType.Cylinder, nook + Vector3.up * 0.4f, new Vector3(0.14f, 0.4f, 0.14f), Brass);
            kit.Deco(PrimitiveType.Cylinder, nook + Vector3.up * 0.82f, new Vector3(1.1f, 0.04f, 1.1f), DarkWood);
            NoShadow(kit.Deco(PrimitiveType.Sphere, nook + new Vector3(0.1f, 1.06f, 0f), new Vector3(0.4f, 0.36f, 0.4f), PatchworkAssets.Plain(CreamCol, 0.8f)));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, nook + new Vector3(-0.3f, 0.92f, 0.2f), new Vector3(0.2f, 0.06f, 0.2f), PatchworkAssets.Plain(CreamCol, 0.8f)));
            TownKit.Solid(kit, nook + Vector3.up * 0.45f, new Vector3(1f, 0.9f, 1f), 0f);
            // A star globe, a tall clock, stacks of books, palms.
            Vector3 sg = C + new Vector3(-14f, 0f, -12f);
            Plinth(kit, sg, new Vector3(2f, 0.6f, 2f), 45f);
            NoShadow(kit.Deco(PrimitiveType.Sphere, sg + Vector3.up * 2.2f, Vector3.one * 2.4f, StarCloth));
            var sgRing = kit.Group("StarGlobeRing", sg + Vector3.up * 2.2f);
            sgRing.rotation = Quaternion.Euler(66f, 0f, 20f);
            Hoop(kit, sgRing, 1.4f, 0.1f, Brass, 22);
            TownKit.Solid(kit, sg + Vector3.up * 1.8f, new Vector3(2f, 3.6f, 2f), 0f);
            Color[] covers = { Wine, Navy, Plum, new Color(0.25f, 0.45f, 0.4f) };
            foreach (var p in new[] { C + new Vector3(8f, 0f, -14f), C + new Vector3(-18f, 0f, 9f), C + new Vector3(18.5f, 0f, -4f) })
                for (int k = 0; k < 5; k++)
                    kit.Deco(PrimitiveType.Cube, p + Vector3.up * (0.16f + k * 0.3f), new Vector3(1.2f - (k % 2) * 0.2f, 0.28f, 0.9f), PatchworkAssets.Fabric(covers[(k + Mathf.RoundToInt(p.x)) % 4 < 0 ? 0 : Mathf.Abs(k + Mathf.RoundToInt(p.x)) % 4]), new Vector3(0f, k * 23f, 0f));
            Palm(kit, C + new Vector3(17f, 0f, 12f), 3f, 31);
            Palm(kit, C + new Vector3(-15f, 0f, 11f), 3f, 32);
            Palm(kit, C + new Vector3(10f, 0f, -15f), 2.6f, 33);
            // Constellations drawn in light on the inside of the dome, and marine snow outside it.
            Vector3 domeBase = C + Vector3.up * 9.4f;
            var domeStars = new MeshBatch();
            var rng = new System.Random(77);
            for (int i = 0; i < 9; i++)
            {
                float az = i * 40f * Mathf.Deg2Rad, el = (24f + (i % 3) * 16f) * Mathf.Deg2Rad;
                Vector3 prev = Vector3.zero;
                for (int k = 0; k < 5; k++)
                {
                    float a2 = az + ((float)rng.NextDouble() - 0.5f) * 0.4f, e2 = el + ((float)rng.NextDouble() - 0.5f) * 0.3f;
                    Vector3 p = domeBase + new Vector3(Mathf.Cos(a2) * Mathf.Cos(e2) * (ObsRadius - 0.5f), Mathf.Sin(e2) * 14.6f, Mathf.Sin(a2) * Mathf.Cos(e2) * (ObsRadius - 0.5f));
                    domeStars.Ball(p, Vector3.one * (0.3f + (float)rng.NextDouble() * 0.3f), 2, 5);
                    if (k > 0) domeStars.Stitches(prev, p, 0.5f, 0.3f, 0.08f, 0.08f);
                    prev = p;
                }
            }
            Twinkling(domeStars, kit.Root, "DomeConstellations", new Color(1f, 0.9f, 0.6f), 1f, 2.8f, 0.9f, 0f);
            Motes(kit, C + Vector3.up * 26f, new Vector3(50f, 30f, 50f), 110, new Color(0.8f, 0.92f, 1f), 2.2f, 0.14f, new Vector3(0.1f, -0.3f, 0.06f));
            Motes(kit, C + Vector3.up * 5f, new Vector3(44f, 9f, 44f), 60, new Color(1f, 0.9f, 0.65f), 2f, 0.07f, new Vector3(0.05f, 0.06f, 0.04f));
            Shooting(kit, C + new Vector3(-18f, 34f, 10f), C + new Vector3(16f, 30f, -6f), new Vector3(10f, 4f, 16f), 14f, 5f, 5f, 0.18f, new Color(0.8f, 1f, 1f));
            // A small brass telescope on the balcony, for looking at the whale's bones.
            Vector3 bt = new Vector3(-6.5f, DeckY, -50.6f);
            kit.Deco(PrimitiveType.Cylinder, bt + Vector3.up * 0.7f, new Vector3(0.1f, 0.7f, 0.1f), Brass);
            kit.Deco(PrimitiveType.Cylinder, bt + new Vector3(0f, 1.5f, -0.3f), new Vector3(0.22f, 0.7f, 0.22f), Brass, new Vector3(110f, 0f, 0f));
        }
    }
}

using UnityEngine;

namespace Patchwork
{
    // The Fray looks different in every world: it's always grey, unravelling and glowing violet,
    // but it takes on whatever the world is made of.
    //   Thread - the original: grey tangles of frayed yarn (the Skiff, the practice room).
    //   Toy    - Toybox Town: toys with the colour drained out of them. Knots stuffed with gears,
    //            springs and wind-up keys; Snarlings are grey wind-up tin beetles; tendrils are
    //            sprung jack-in-the-boxes; tears are holes ripped in wrapping paper.
    public enum FrayStyle { Thread, Toy }

    public static class FrayTheme
    {
        public static FrayStyle Style = FrayStyle.Thread;

        static Material Dark { get { return PatchworkAssets.Yarn(SnarlKnot.FrayDark); } }
        static Material Grey { get { return PatchworkAssets.Yarn(SnarlKnot.FrayGrey); } }
        static Material GlowM { get { return PatchworkAssets.Glow(SnarlKnot.FrayGlow, 1.4f); } }
        // Drained toy colours: tin, and paint that has gone grey.
        static Material Tin { get { return PatchworkAssets.Plain(new Color(0.56f, 0.54f, 0.62f), 0.72f); } }
        static Material DarkTin { get { return PatchworkAssets.Plain(new Color(0.32f, 0.3f, 0.37f), 0.65f); } }
        static Material DullRed { get { return PatchworkAssets.Plain(new Color(0.52f, 0.4f, 0.45f), 0.45f); } }
        static Material DullBlue { get { return PatchworkAssets.Plain(new Color(0.4f, 0.43f, 0.55f), 0.45f); } }
        static Material DullPaper { get { return PatchworkAssets.Felt(new Color(0.6f, 0.56f, 0.64f)); } }

        // ------------------------------------------------------------------ Knots

        public static void Knot(LevelKit kit, Transform parent, float size, int strands)
        {
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 1.1f * size, Dark, default(Vector3), parent);
            int n = Style == FrayStyle.Toy ? Mathf.Max(5, strands * 2 / 3) : strands;
            for (int i = 0; i < n; i++)
            {
                Vector3 e = new Vector3((i * 47) % 360, (i * 83) % 360, (i * 29) % 360);
                var strand = kit.Deco(PrimitiveType.Capsule, Vector3.zero, new Vector3(0.16f, 0.7f, 0.16f) * size, i % 3 == 0 ? Dark : Grey, e, parent);
                strand.localPosition = Quaternion.Euler(e) * new Vector3(0f, 0f, 0.3f * size);
            }
            for (int i = 0; i < 3; i++)
                kit.Deco(PrimitiveType.Sphere, Quaternion.Euler(i * 120f, i * 70f, 0f) * new Vector3(0f, 0f, 0.5f * size), Vector3.one * 0.18f * size, GlowM, default(Vector3), parent);
            if (Style != FrayStyle.Toy) return;

            // Broken toy bits caught in the tangle, poking out all round.
            int bits = Mathf.Clamp(Mathf.RoundToInt(3 + size * 2f), 4, 10);
            for (int i = 0; i < bits; i++)
            {
                // Spread evenly over the ball (golden-angle spiral).
                float y = 1f - (i + 0.5f) / bits * 2f;
                float r = Mathf.Sqrt(1f - y * y);
                float a = i * 2.39996f;
                Vector3 dir = new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r);
                Vector3 at = dir * 0.55f * size;
                var look = Quaternion.FromToRotation(Vector3.up, dir);
                switch (i % 5)
                {
                    case 0: Gear(kit, parent, at, 0.32f * size, look * Quaternion.Euler(90f, 0f, 0f), i % 2 == 0 ? Tin : DullRed); break;
                    case 1: Key(kit, parent, at, 0.9f * size, look, Tin); break;
                    case 2: Spring(parent, at, dir, 0.12f * size, 0.6f * size, 4f, Tin); break;
                    case 3:
                        var block = kit.Deco(PrimitiveType.Cube, at, Vector3.one * 0.38f * size, i % 2 == 0 ? DullBlue : DullRed, default(Vector3), parent);
                        block.localRotation = look * Quaternion.Euler(20f, 35f, 10f);
                        break;
                    default: ButtonEye(kit, parent, at, 0.3f * size, look); break;
                }
            }
        }

        // ------------------------------------------------------------------ Tears in the ground

        public static Transform Tear(LevelKit kit, Vector3 pos, float size)
        {
            var g = kit.Group("FrayTear", pos);
            if (Style != FrayStyle.Toy)
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.04f, 0f), new Vector3(size, 0.03f, size * 0.7f), PatchworkAssets.Felt(SnarlKnot.FrayDark), new Vector3(0f, (pos.x * 13f) % 360f, 0f), g);
                for (int i = 0; i < 9; i++)
                {
                    float a = i / 9f * Mathf.PI * 2f;
                    var t = kit.Deco(PrimitiveType.Capsule, new Vector3(Mathf.Cos(a) * size * 0.45f, 0.12f, Mathf.Sin(a) * size * 0.32f), new Vector3(0.06f, 0.22f + (i % 3) * 0.08f, 0.06f), Grey, default(Vector3), g);
                    t.localRotation = Quaternion.LookRotation(new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a))) * Quaternion.Euler(60f + (i % 2) * 15f, 0f, 0f);
                }
                return g;
            }
            // A hole ripped in grey wrapping paper, with jagged flaps curling up, springs and a gear
            // poking out of the dark, and a faint violet glow from inside.
            float yaw = (pos.x * 13f + pos.z * 7f) % 360f;
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.035f, 0f), new Vector3(size, 0.02f, size * 0.75f), PatchworkAssets.Felt(SnarlKnot.FrayDark), new Vector3(0f, yaw, 0f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.05f, 0f), new Vector3(size * 0.5f, 0.01f, size * 0.38f), PatchworkAssets.Glow(SnarlKnot.FrayGlow, 0.6f), new Vector3(0f, yaw, 0f), g);
            for (int i = 0; i < 8; i++)
            {
                float a = i / 8f * Mathf.PI * 2f + yaw * Mathf.Deg2Rad;
                Vector3 rim = new Vector3(Mathf.Cos(a) * size * 0.5f, 0.1f, Mathf.Sin(a) * size * 0.38f);
                var flap = kit.Deco(PrimitiveType.Cube, rim, new Vector3(0.35f + (i % 3) * 0.12f, 0.03f, 0.3f), i % 2 == 0 ? DullPaper : DullRed, default(Vector3), g);
                flap.localRotation = Quaternion.LookRotation(new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a))) * Quaternion.Euler(-35f - (i % 3) * 12f, 0f, (i % 2 == 0 ? 15f : -10f));
            }
            Spring(g, new Vector3(size * 0.15f, 0.02f, 0f), new Vector3(0.2f, 1f, 0.1f).normalized, 0.08f, 0.55f, 5f, Tin);
            Spring(g, new Vector3(-size * 0.12f, 0.02f, size * 0.08f), new Vector3(-0.3f, 1f, 0.2f).normalized, 0.07f, 0.4f, 4f, Tin);
            Gear(kit, g, new Vector3(-size * 0.1f, 0.12f, -size * 0.12f), 0.18f, Quaternion.Euler(70f, yaw, 0f), DarkTin);
            return g;
        }

        // ------------------------------------------------------------------ Tendrils

        public static Transform Tendril(LevelKit kit, Vector3 pos, float height)
        {
            var g = kit.Group("FrayTendril", pos);
            if (Style != FrayStyle.Toy)
            {
                Vector3 p = Vector3.zero;
                int segs = Mathf.Max(3, Mathf.RoundToInt(height / 0.7f));
                for (int i = 0; i < segs; i++)
                {
                    Vector3 next = p + new Vector3(Mathf.Sin(i * 1.7f) * 0.3f, height / segs, Mathf.Cos(i * 1.3f) * 0.3f);
                    var seg = kit.Deco(PrimitiveType.Cylinder, (p + next) * 0.5f, new Vector3(0.12f, (next - p).magnitude * 0.5f, 0.12f), i % 2 == 0 ? Dark : Grey, default(Vector3), g);
                    seg.localRotation = Quaternion.FromToRotation(Vector3.up, (next - p).normalized);
                    p = next;
                }
                kit.Deco(PrimitiveType.Sphere, p, Vector3.one * 0.3f, PatchworkAssets.Glow(SnarlKnot.FrayGlow, 1.5f), default(Vector3), g);
                for (int i = 0; i < 4; i++)
                {
                    var mote = kit.Deco(PrimitiveType.Cube, new Vector3(Mathf.Cos(i * 1.6f) * 0.8f, height * (0.3f + i * 0.15f), Mathf.Sin(i * 1.6f) * 0.8f), Vector3.one * 0.12f, Dark, new Vector3(i * 30f, i * 45f, 0f), g);
                    var sp = mote.gameObject.AddComponent<Spinner>();
                    sp.Axis = new Vector3(1f, 1f, 0f);
                    sp.Speed = 120f;
                    sp.BobHeight = 0.2f;
                    sp.BobSpeed = 2f + i * 0.4f;
                }
            }
            else
            {
                // A grey jack-in-the-box sprung loose: the box, its lid hanging open, a long wobbly
                // spring and a drooping jester head with glowing button eyes.
                float box = Mathf.Clamp(height * 0.28f, 0.5f, 0.9f);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, box * 0.5f, 0f), Vector3.one * box, DullBlue, default(Vector3), g);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, box + 0.02f, 0f), new Vector3(box * 1.04f, 0.04f, box * 1.04f), DarkTin, default(Vector3), g);
                var lid = kit.Deco(PrimitiveType.Cube, new Vector3(0f, box * 1.2f, -box * 0.5f), new Vector3(box, 0.06f, box), DullRed, new Vector3(-110f, 0f, 0f), g);
                lid.name = "Lid";
                kit.Deco(PrimitiveType.Sphere, new Vector3(box * 0.52f, box * 0.5f, 0f), new Vector3(0.05f, box * 0.3f, box * 0.3f), DullRed, default(Vector3), g);
                float springH = Mathf.Max(0.6f, height - box - 0.5f);
                Spring(g, new Vector3(0f, box, 0f), new Vector3(0.12f, 1f, 0.05f).normalized, 0.14f, springH, Mathf.Max(4f, springH * 3f), Tin);
                Vector3 top = new Vector3(0.12f, box, 0.05f).normalized * 0f + new Vector3(0f, box, 0f) + new Vector3(0.12f, 1f, 0.05f).normalized * springH;
                var head = kit.Group("JesterHead", Vector3.zero);
                head.SetParent(g, false);
                head.localPosition = top + Vector3.up * 0.25f;
                kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.5f, 0.48f, 0.45f), DullPaper, default(Vector3), head);
                kit.Deco(PrimitiveType.Capsule, new Vector3(-0.22f, 0.28f, 0f), new Vector3(0.16f, 0.32f, 0.16f), DullRed, new Vector3(0f, 0f, 55f), head);
                kit.Deco(PrimitiveType.Capsule, new Vector3(0.22f, 0.28f, 0f), new Vector3(0.16f, 0.32f, 0.16f), DullBlue, new Vector3(0f, 0f, -55f), head);
                kit.Deco(PrimitiveType.Sphere, new Vector3(-0.44f, 0.18f, 0f), Vector3.one * 0.12f, GlowM, default(Vector3), head);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0.44f, 0.18f, 0f), Vector3.one * 0.12f, GlowM, default(Vector3), head);
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.11f, 0.04f, 0.2f), new Vector3(0.12f, 0.02f, 0.12f), GlowM, new Vector3(90f, 0f, 0f), head);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.1f, 0.21f), new Vector3(0.22f, 0.03f, 0.02f), DarkTin, new Vector3(0f, 0f, 8f), head);
                var nod = head.gameObject.AddComponent<Rocker>();
                nod.Axis = new Vector3(1f, 0f, 0.4f);
                nod.Amount = 14f;
                nod.Speed = 1.3f;
                // Little gears orbiting it, spinning.
                for (int i = 0; i < 3; i++)
                {
                    var gear = Gear(kit, g, new Vector3(Mathf.Cos(i * 2.1f) * 0.8f, height * (0.35f + i * 0.18f), Mathf.Sin(i * 2.1f) * 0.8f), 0.14f, Quaternion.Euler(i * 30f, i * 50f, 0f), i == 1 ? DullRed : DarkTin);
                    var sp = gear.gameObject.AddComponent<Spinner>();
                    sp.Axis = Vector3.up;
                    sp.Speed = 140f;
                    sp.BobHeight = 0.18f;
                    sp.BobSpeed = 1.6f + i * 0.4f;
                }
            }
            var sway = g.gameObject.AddComponent<FlagWave>();
            sway.Amount = 25f;
            sway.Speed = 0.8f;
            return g;
        }

        // ------------------------------------------------------------------ Snarlings

        // Builds a toy-world Snarling: a grey wind-up tin beetle with a key turning in its back.
        public static void ToySnarling(Transform root, Transform body, out Transform eyeL, out Transform eyeR, out Transform key)
        {
            Part(PrimitiveType.Sphere, body, Vector3.zero, new Vector3(1.15f, 0.85f, 1.3f), Tin, Vector3.zero);
            Part(PrimitiveType.Cylinder, body, new Vector3(0f, -0.05f, 0f), new Vector3(1.18f, 0.05f, 1.33f), DullRed, Vector3.zero);
            Part(PrimitiveType.Cube, body, new Vector3(0f, 0.36f, 0f), new Vector3(0.06f, 0.06f, 1.1f), DarkTin, new Vector3(-8f, 0f, 0f));
            for (int i = 0; i < 6; i++)
            {
                float a = i / 6f * Mathf.PI * 2f;
                Part(PrimitiveType.Sphere, body, new Vector3(Mathf.Cos(a) * 0.42f, 0.22f, Mathf.Sin(a) * 0.5f), Vector3.one * 0.07f, DarkTin, Vector3.zero);
            }
            Part(PrimitiveType.Sphere, body, new Vector3(0f, -0.04f, 0.62f), new Vector3(0.72f, 0.6f, 0.55f), DarkTin, Vector3.zero);
            var glow = PatchworkAssets.Glow(SnarlKnot.FrayGlow, 1.4f);
            eyeL = Part(PrimitiveType.Sphere, body, new Vector3(-0.17f, 0.06f, 0.86f), new Vector3(0.18f, 0.24f, 0.1f), glow, Vector3.zero);
            eyeR = Part(PrimitiveType.Sphere, body, new Vector3(0.17f, 0.06f, 0.86f), new Vector3(0.18f, 0.24f, 0.1f), glow, Vector3.zero);
            // Bent antennae with glowing tips.
            for (int s = -1; s <= 1; s += 2)
            {
                Part(PrimitiveType.Capsule, body, new Vector3(s * 0.18f, 0.32f, 0.72f), new Vector3(0.04f, 0.2f, 0.04f), DarkTin, new Vector3(30f, 0f, -s * 25f));
                Part(PrimitiveType.Sphere, body, new Vector3(s * 0.28f, 0.5f, 0.84f), Vector3.one * 0.09f, glow, Vector3.zero);
            }
            // The wind-up key on its back.
            key = new GameObject("Key").transform;
            key.SetParent(body, false);
            key.localPosition = new Vector3(0f, 0.38f, -0.42f);
            key.localRotation = Quaternion.Euler(-40f, 0f, 0f);
            Part(PrimitiveType.Cylinder, key, new Vector3(0f, 0.15f, 0f), new Vector3(0.08f, 0.15f, 0.08f), DarkTin, Vector3.zero);
            Part(PrimitiveType.Sphere, key, new Vector3(-0.16f, 0.36f, 0f), new Vector3(0.26f, 0.2f, 0.06f), Tin, Vector3.zero);
            Part(PrimitiveType.Sphere, key, new Vector3(0.16f, 0.36f, 0f), new Vector3(0.26f, 0.2f, 0.06f), Tin, Vector3.zero);
            // Clockwork wheels instead of legs.
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * 0.42f, z = (i < 2 ? -1f : 1f) * 0.32f;
                Part(PrimitiveType.Cylinder, root, new Vector3(x, 0.17f, z), new Vector3(0.32f, 0.04f, 0.32f), DarkTin, new Vector3(0f, 0f, 90f));
                Part(PrimitiveType.Cylinder, root, new Vector3(x * 1.1f, 0.17f, z), new Vector3(0.12f, 0.03f, 0.12f), DullRed, new Vector3(0f, 0f, 90f));
            }
        }

        public static Transform Part(PrimitiveType type, Transform parent, Vector3 p, Vector3 s, Material m, Vector3 e)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.Destroy(go.GetComponent<Collider>());
            go.layer = 2;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = p;
            go.transform.localRotation = Quaternion.Euler(e);
            go.transform.localScale = s;
            go.GetComponent<Renderer>().sharedMaterial = m;
            return go.transform;
        }

        // ------------------------------------------------------------------ Toy bits

        // A cog: a disc with square teeth round it (lying in its local x-z plane).
        static Transform Gear(LevelKit kit, Transform parent, Vector3 at, float r, Quaternion rot, Material m)
        {
            var g = new GameObject("Gear").transform;
            g.SetParent(parent, false);
            g.localPosition = at;
            g.localRotation = rot;
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(r * 2f, r * 0.18f, r * 2f), m, default(Vector3), g);
            for (int i = 0; i < 8; i++)
            {
                float a = i / 8f * 360f;
                var tooth = kit.Deco(PrimitiveType.Cube, Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0f, r * 1.05f), new Vector3(r * 0.42f, r * 0.34f, r * 0.4f), m, new Vector3(0f, a, 0f), g);
                tooth.name = "Tooth";
            }
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(r * 0.6f, r * 0.24f, r * 0.6f), DarkTin, default(Vector3), g);
            return g;
        }

        // A wind-up key sticking out along its local up.
        static void Key(LevelKit kit, Transform parent, Vector3 at, float s, Quaternion rot, Material m)
        {
            var k = new GameObject("WindKey").transform;
            k.SetParent(parent, false);
            k.localPosition = at;
            k.localRotation = rot;
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.18f * s, 0f), new Vector3(0.08f * s, 0.18f * s, 0.08f * s), m, default(Vector3), k);
            kit.Deco(PrimitiveType.Sphere, new Vector3(-0.15f * s, 0.42f * s, 0f), new Vector3(0.26f, 0.2f, 0.06f) * s, m, default(Vector3), k);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.15f * s, 0.42f * s, 0f), new Vector3(0.26f, 0.2f, 0.06f) * s, m, default(Vector3), k);
        }

        // A big four-holed button with violet light shining through the holes, like an eye.
        static void ButtonEye(LevelKit kit, Transform parent, Vector3 at, float r, Quaternion rot)
        {
            var b = new GameObject("ButtonEye").transform;
            b.SetParent(parent, false);
            b.localPosition = at;
            b.localRotation = rot;
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(r * 2f, r * 0.2f, r * 2f), DullBlue, default(Vector3), b);
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * r * 0.3f, z = (i < 2 ? -1f : 1f) * r * 0.3f;
                kit.Deco(PrimitiveType.Cylinder, new Vector3(x, r * 0.11f, z), new Vector3(r * 0.32f, r * 0.02f, r * 0.32f), GlowM, default(Vector3), b);
            }
        }

        // A coiled spring along "dir" from "start" (in parent space), built as one mesh.
        static void Spring(Transform parent, Vector3 start, Vector3 dir, float radius, float length, float turns, Material m)
        {
            var batch = new MeshBatch();
            var rot = Quaternion.FromToRotation(Vector3.up, dir.normalized);
            int steps = Mathf.Max(8, Mathf.RoundToInt(turns * 10f));
            for (int i = 0; i <= steps; i++)
            {
                float k = i / (float)steps;
                float a = k * turns * Mathf.PI * 2f;
                Vector3 p = start + rot * new Vector3(Mathf.Cos(a) * radius, k * length, Mathf.Sin(a) * radius);
                batch.Ball(p, Vector3.one * radius * 0.55f, 3, 5);
            }
            batch.Build(parent, "Spring", m, true);
        }
    }
}

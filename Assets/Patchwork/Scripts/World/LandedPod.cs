using UnityEngine;

namespace Patchwork
{
    // Your Pod, landed (a bit hard!) outside a level. It's the same egg-shaped cardboard pod you
    // live in, tipped over at a jaunty angle in the crater it dug, with a long skid trench behind
    // it, torn-up turf and scattered bits. It has a pill-shaped polka-dot hatch, a riveted brass
    // porthole glowing from inside, three striped tail fins, springy landing legs (one of them
    // bent), a bent antenna still blinking, a patch stitched over a dent, sticky tape over a crack,
    // a steaming nozzle and a fizzing loose wire. Walk up to the hatch to fly home.
    public class LandedPod
    {
        public Transform Root, Door;
        public Vector3 Inside, DoorStep;

        const float Cy = 3.9f, Rx = 2.5f, Ry = 3.1f;
        static readonly Color Kraft = new Color(0.8f, 0.62f, 0.42f), KraftDark = new Color(0.62f, 0.45f, 0.3f);

        // A point on the egg's surface (angle round from the front, height) and its outward normal.
        static Vector3 Surface(float deg, float y, out Vector3 normal)
        {
            float k = Mathf.Clamp((y - Cy) / Ry, -0.999f, 0.999f);
            float r = Rx * Mathf.Sqrt(1f - k * k);
            float a = deg * Mathf.Deg2Rad;
            var p = new Vector3(Mathf.Sin(a) * r, y, Mathf.Cos(a) * r);
            normal = new Vector3(p.x / (Rx * Rx), (y - Cy) / (Ry * Ry), p.z / (Rx * Rx)).normalized;
            return p;
        }

        public static LandedPod Build(LevelKit kit, Vector3 feet, float yaw)
        {
            var pod = new LandedPod();
            Crater(kit, feet, yaw);

            // The pod itself, tipped back and to one side, and sunk a little into the dirt.
            var g = kit.Group("LandedPod", feet + Vector3.down * 0.25f);
            g.rotation = Quaternion.Euler(0f, yaw, 0f) * Quaternion.Euler(-6f, 0f, 9f);
            pod.Root = g;
            var shell = PatchworkAssets.Cardboard(Kraft);
            var seam = PatchworkAssets.Cardboard(KraftDark);
            var ink = PatchworkAssets.Plain(new Color(0.12f, 0.09f, 0.14f), 0.2f);
            var brass = PatchworkAssets.Plain(new Color(0.95f, 0.75f, 0.35f), 0.85f);
            var metal = PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.9f), 0.7f);
            Normal(kit, g, PrimitiveType.Sphere, new Vector3(0f, Cy, 0f), new Vector3(Rx * 2f, Ry * 2f, Rx * 2f), shell);

            // Cardboard panel seams (slim, so they read as joins, not rings) round the back.
            foreach (float r in new[] { 45f, 90f, 135f, 180f, 225f, 270f, 315f })
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, Cy, 0f), new Vector3(Rx * 2f + 0.06f, Ry * 2f + 0.04f, 0.07f), seam, new Vector3(0f, r, 0f), g);
            // A zigzag band round its middle, like the box art, with a brass trim either side.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, Cy, 0f), new Vector3(Rx * 2f + 0.1f, 0.22f, Rx * 2f + 0.1f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, new Color(0.2f, 0.25f, 0.55f), new Color(1f, 0.8f, 0.28f)), default(Vector3), g);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, Cy + s * 0.25f, 0f), new Vector3(Rx * 2f + 0.06f, 0.035f, Rx * 2f + 0.06f), brass, default(Vector3), g);

            // The porthole: a brass ring with rivets, glass glowing warm from inside.
            Vector3 wn;
            Vector3 wp = Surface(0f, 5.05f, out wn);
            var win = kit.Group("Porthole", Vector3.zero);
            win.SetParent(g, false);
            win.localPosition = wp + wn * 0.02f;
            win.localRotation = Quaternion.LookRotation(wn, Vector3.up);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.55f, 0.07f, 1.55f), brass, new Vector3(90f, 0f, 0f), win);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.03f), new Vector3(1.22f, 0.05f, 1.22f), PatchworkAssets.Glow(new Color(1f, 0.85f, 0.55f), 1.4f), new Vector3(90f, 0f, 0f), win);
            kit.Deco(PrimitiveType.Sphere, new Vector3(-0.25f, 0.22f, 0.06f), new Vector3(0.32f, 0.12f, 0.03f), PatchworkAssets.Glow(Color.white, 1.8f), new Vector3(0f, 0f, 30f), win);
            for (int k = 0; k < 10; k++)
            {
                float a = k * Mathf.PI * 2f / 10f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.7f, Mathf.Sin(a) * 0.7f, 0.04f), Vector3.one * 0.08f, brass, default(Vector3), win);
            }

            // The hatch: a pill-shaped frame flush with the shell, a dark doorway, and the
            // polka-dot door hinged on the left with its own little porthole and a brass handle.
            Vector3 dn;
            Vector3 dp = Surface(0f, 2.15f, out dn);
            var hatch = kit.Group("Hatch", Vector3.zero);
            hatch.SetParent(g, false);
            hatch.localPosition = dp;
            hatch.localRotation = Quaternion.LookRotation(dn, Vector3.up);
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0f, -0.08f), new Vector3(1.95f, 1.35f, 0.42f), PatchworkAssets.Felt(new Color(0.9f, 0.3f, 0.35f)), default(Vector3), hatch);
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0f, 0.06f), new Vector3(1.6f, 1.2f, 0.2f), ink, default(Vector3), hatch);
            for (int k = 0; k < 14; k++)
            {
                float a = k * Mathf.PI * 2f / 14f;
                kit.Deco(PrimitiveType.Cube, new Vector3(Mathf.Cos(a) * 0.92f, Mathf.Sin(a) * 1.25f, 0.14f), new Vector3(0.05f, 0.14f, 0.02f), PatchworkAssets.Felt(new Color(1f, 0.9f, 0.7f)), new Vector3(0f, 0f, a * Mathf.Rad2Deg), hatch);
            }
            var hinge = new GameObject("DoorHinge").transform;
            hinge.SetParent(hatch, false);
            hinge.localPosition = new Vector3(-0.78f, 0f, 0.2f);
            var doorMat = PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(0.9f, 0.3f, 0.35f), new Color(0.99f, 0.95f, 0.88f));
            kit.Deco(PrimitiveType.Capsule, new Vector3(0.78f, 0f, 0f), new Vector3(1.5f, 1.12f, 0.14f), doorMat, default(Vector3), hinge);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.78f, 0.45f, 0.08f), new Vector3(0.5f, 0.03f, 0.5f), brass, new Vector3(90f, 0f, 0f), hinge);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.78f, 0.45f, 0.1f), new Vector3(0.38f, 0.03f, 0.38f), PatchworkAssets.Glow(new Color(1f, 0.85f, 0.55f), 1.2f), new Vector3(90f, 0f, 0f), hinge);
            kit.Deco(PrimitiveType.Capsule, new Vector3(1.3f, -0.1f, 0.12f), new Vector3(0.1f, 0.18f, 0.1f), brass, default(Vector3), hinge);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, s * 0.75f, 0f), new Vector3(0.12f, 0.12f, 0.12f), brass, default(Vector3), hinge);
            pod.Door = hinge;
            // A little felt step under the hatch, sitting flat on the ground.
            var stepAt = feet + Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, 0f, 2.55f);
            kit.Block(stepAt + Vector3.up * 0.22f, new Vector3(1.7f, 0.44f, 0.9f), Color.white, "PodStep", PatchworkAssets.Felt(new Color(1f, 0.8f, 0.3f))).rotation = Quaternion.Euler(0f, yaw, 0f);

            // A fabric patch stitched over a dent, and sticky tape over a crack.
            Vector3 pn;
            Vector3 pp = Surface(-58f, 4.6f, out pn);
            var patch = kit.Group("Patch", Vector3.zero);
            patch.SetParent(g, false);
            patch.localPosition = pp + pn * 0.02f;
            patch.localRotation = Quaternion.LookRotation(pn, Vector3.up) * Quaternion.Euler(0f, 0f, 12f);
            kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(0.95f, 0.85f, 0.03f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, new Color(0.99f, 0.96f, 0.9f), new Color(0.35f, 0.55f, 0.9f)), default(Vector3), patch);
            for (int k = 0; k < 16; k++)
            {
                float u = (k % 4) / 3f - 0.5f;
                int side = k / 4;
                Vector3 at = side == 0 ? new Vector3(u * 0.8f, 0.44f, 0.02f) : side == 1 ? new Vector3(u * 0.8f, -0.44f, 0.02f) : side == 2 ? new Vector3(-0.49f, u * 0.7f, 0.02f) : new Vector3(0.49f, u * 0.7f, 0.02f);
                kit.Deco(PrimitiveType.Cube, at, side < 2 ? new Vector3(0.03f, 0.12f, 0.02f) : new Vector3(0.12f, 0.03f, 0.02f), PatchworkAssets.Felt(new Color(0.9f, 0.3f, 0.35f)), default(Vector3), patch);
            }
            Vector3 tn;
            Vector3 tp = Surface(52f, 3.0f, out tn);
            for (int s = -1; s <= 1; s += 2)
            {
                var tape = kit.Deco(PrimitiveType.Cube, tp + tn * 0.03f, new Vector3(1.1f, 0.22f, 0.02f), PatchworkAssets.Plain(new Color(0.95f, 0.9f, 0.7f), 0.3f), default(Vector3), g);
                tape.localRotation = Quaternion.LookRotation(tn, Vector3.up) * Quaternion.Euler(0f, 0f, s * 35f);
            }
            // A scorched, sooty smudge low down on one side.
            Vector3 sn;
            Vector3 sp = Surface(115f, 1.5f, out sn);
            var soot = kit.Deco(PrimitiveType.Sphere, sp, new Vector3(1.4f, 0.9f, 0.08f), PatchworkAssets.Felt(new Color(0.32f, 0.26f, 0.24f)), default(Vector3), g);
            soot.localRotation = Quaternion.LookRotation(sn, Vector3.up);

            // Three striped tail fins round the bottom.
            var fin = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.9f, 0.3f, 0.35f), new Color(0.99f, 0.95f, 0.88f));
            foreach (float a in new[] { 120f, 180f, 240f })
            {
                Vector3 fn;
                Vector3 fp = Surface(a, 1.6f, out fn);
                var f = kit.Deco(PrimitiveType.Cube, fp + fn * 0.45f + Vector3.down * 0.2f, new Vector3(0.12f, 1.6f, 1.1f), fin, default(Vector3), g);
                f.localRotation = Quaternion.LookRotation(new Vector3(fn.x, 0f, fn.z).normalized, Vector3.up) * Quaternion.Euler(-28f, 0f, 0f);
                f.localScale = new Vector3(0.12f, 1.6f, 1.1f);
            }

            // Springy landing legs: struts with a coiled spring and round felt feet. One buckled.
            for (int k = 0; k < 3; k++)
            {
                float deg = k * 120f + 60f;
                bool bent = k == 1;
                Vector3 top = Surface(deg, 1.7f, out Vector3 ln) - ln * 0.1f;
                Vector3 dir = new Vector3(ln.x, 0f, ln.z).normalized;
                Vector3 knee = top + dir * 0.7f + Vector3.down * 0.7f;
                Vector3 foot = bent ? knee + dir * 0.9f + Vector3.down * 0.1f : top + dir * 1.3f + Vector3.down * 1.6f;
                Rod(kit, top, knee, metal, 0.18f, g);
                Rod(kit, knee, foot, metal, 0.15f, g);
                for (int c = 0; c < 4; c++)
                {
                    var coil = kit.Deco(PrimitiveType.Cylinder, Vector3.Lerp(knee, foot, 0.25f + c * 0.12f), new Vector3(0.3f, 0.025f, 0.3f), brass, default(Vector3), g);
                    coil.localRotation = Quaternion.FromToRotation(Vector3.up, (foot - knee).normalized);
                }
                kit.Deco(PrimitiveType.Sphere, knee, Vector3.one * 0.24f, brass, default(Vector3), g);
                var pad = kit.Deco(PrimitiveType.Cylinder, foot, new Vector3(0.85f, 0.1f, 0.85f), PatchworkAssets.Felt(new Color(0.9f, 0.3f, 0.35f)), default(Vector3), g);
                if (bent) pad.localRotation = Quaternion.Euler(0f, 0f, 60f);
            }

            // The engine nozzle underneath (half buried), still steaming.
            Normal(kit, g, PrimitiveType.Cylinder, new Vector3(0f, 0.75f, 0f), new Vector3(1.4f, 0.35f, 1.4f), PatchworkAssets.Plain(new Color(0.35f, 0.33f, 0.38f), 0.6f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.42f, 0f), new Vector3(1.15f, 0.03f, 1.15f), PatchworkAssets.Glow(new Color(1f, 0.5f, 0.2f), 1.6f), default(Vector3), g);
            var steam = new GameObject("Steam");
            steam.transform.SetParent(g, false);
            steam.transform.localPosition = new Vector3(1.2f, 0.5f, -0.6f);
            steam.AddComponent<Smoker>().Every = 2.6f;

            // The antenna, bent in the landing, its light still blinking, and a little chimney
            // puffing smoke.
            Vector3 aTop = new Vector3(0.45f, Cy + Ry - 0.05f, 0f);
            Vector3 aMid = aTop + new Vector3(0.1f, 0.7f, 0f);
            Vector3 aEnd = aMid + new Vector3(0.55f, 0.25f, 0.1f);
            Rod(kit, aTop, aMid, metal, 0.07f, g);
            Rod(kit, aMid, aEnd, metal, 0.06f, g);
            var blink = kit.Deco(PrimitiveType.Sphere, aEnd, Vector3.one * 0.26f, PatchworkAssets.Glow(new Color(1f, 0.35f, 0.4f), 2f), default(Vector3), g);
            blink.gameObject.AddComponent<Blinker>();
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-0.6f, Cy + Ry - 0.35f, -0.4f), new Vector3(0.6f, 0.4f, 0.6f), shell, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-0.6f, Cy + Ry + 0.06f, -0.4f), new Vector3(0.7f, 0.05f, 0.7f), seam, default(Vector3), g);
            var puff = new GameObject("ChimneySmoke");
            puff.transform.SetParent(g, false);
            puff.transform.localPosition = new Vector3(-0.6f, Cy + Ry + 0.2f, -0.4f);
            var ps = puff.AddComponent<Smoker>();
            ps.Every = 3.4f;
            ps.Kind = FxBurst.Kind.Puff;
            // A loose wire fizzing with sparks.
            Vector3 wn2;
            Vector3 wire = Surface(-100f, 2.6f, out wn2);
            Rod(kit, wire, wire + wn2 * 0.5f + Vector3.down * 0.4f, PatchworkAssets.Yarn(new Color(0.95f, 0.8f, 0.2f)), 0.03f, g);
            var spark = kit.Deco(PrimitiveType.Sphere, wire + wn2 * 0.5f + Vector3.down * 0.4f, Vector3.one * 0.12f, PatchworkAssets.Glow(new Color(1f, 0.9f, 0.5f), 3f), default(Vector3), g);
            var sb = spark.gameObject.AddComponent<Blinker>();
            sb.Period = 0.35f;

            // Solid (but not in front of the door, where Patch waits).
            var solid = kit.Block(feet + g.rotation * new Vector3(0f, 3.6f, -0.5f), new Vector3(4f, 5.2f, 3.4f), Color.white, "PodSolid");
            solid.rotation = g.rotation;
            solid.GetComponent<Renderer>().enabled = false;
            // (The box sits inside the egg, so the camera is told about the egg itself too.)
            PlayerCamera.AddBlocker(g, new Vector3(0f, Cy, 0f), new Vector3(Rx + 0.25f, Ry + 0.25f, Rx + 0.25f));
            pod.Inside = feet + Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, 0.9f, 1.55f);
            pod.DoorStep = feet + Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, 0.95f, 2.7f);
            return pod;
        }

        static Transform Normal(LevelKit kit, Transform parent, PrimitiveType type, Vector3 pos, Vector3 scale, Material mat)
        {
            return kit.Deco(type, pos, scale, mat, default(Vector3), parent);
        }

        static void Rod(LevelKit kit, Vector3 a, Vector3 b, Material mat, float thick, Transform parent)
        {
            Vector3 d = b - a;
            var t = kit.Deco(PrimitiveType.Cylinder, (a + b) * 0.5f, new Vector3(thick, d.magnitude * 0.5f, thick), mat, default(Vector3), parent);
            t.localRotation = Quaternion.FromToRotation(Vector3.up, d.normalized);
        }

        // The crater: a ring of churned-up earth (walkable, gentle slopes in and out), a scorched
        // floor, turf clods flung about, and a long skid trench the pod ploughed on the way in.
        static void Crater(LevelKit kit, Vector3 c, float yaw)
        {
            var root = kit.Group("Crater", Vector3.zero);   // children use world positions
            var dirt = PatchworkAssets.Felt(new Color(0.55f, 0.38f, 0.26f));
            var dirtDark = PatchworkAssets.Felt(new Color(0.4f, 0.27f, 0.2f));
            var turf = PatchworkAssets.Grass(new Color(0.48f, 0.68f, 0.38f));
            const float inner = 6.2f, crest = 7.8f, outer = 9.8f, h = 0.7f;
            const int segs = 30;
            for (int i = 0; i < segs; i++)
            {
                float a = (i + 0.5f) * 360f / segs;
                var q = Quaternion.Euler(0f, a, 0f);
                // Inner slope (dirt) and outer slope (grass), as tilted slabs.
                Slope(kit, c, q, inner, crest, 0f, h, 2f * Mathf.PI * crest / segs * 1.12f, dirt);
                Slope(kit, c, q, outer, crest, 0f, h, 2f * Mathf.PI * outer / segs * 1.12f, turf);
                // Lumpy clods along the crest.
                if (i % 2 == 0)
                {
                    Vector3 lump = c + q * new Vector3(0f, h - 0.05f, crest + Mathf.Sin(i * 1.7f) * 0.3f);
                    kit.Deco(PrimitiveType.Sphere, lump, new Vector3(1.1f, 0.35f, 0.8f), i % 4 == 0 ? dirtDark : dirt, new Vector3(0f, a + 20f, 0f), root);
                }
            }
            // The scorched floor, with a darker burn right under the pod.
            kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.02f, new Vector3(inner * 2.05f, 0.01f, inner * 2.05f), dirt, default(Vector3), root);
            kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.03f, new Vector3(6f, 0.01f, 6f), PatchworkAssets.Felt(new Color(0.3f, 0.24f, 0.22f)), default(Vector3), root);
            // Cracks radiating out of the burn.
            for (int k = 0; k < 7; k++)
            {
                float a = k * 51f + 10f;
                kit.Deco(PrimitiveType.Cube, c + Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0.04f, 3.6f), new Vector3(0.12f, 0.01f, 1.8f + (k % 3) * 0.5f), PatchworkAssets.Felt(new Color(0.22f, 0.17f, 0.16f)), new Vector3(0f, a, 0f), root);
            }
            // Turf clods and pebbles flung out round the rim.
            var rng = new System.Random(7);
            for (int k = 0; k < 22; k++)
            {
                float a = (float)rng.NextDouble() * 360f;
                float r = outer + 0.5f + (float)rng.NextDouble() * 5f;
                Vector3 p = c + Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0f, r);
                float s = 0.25f + (float)rng.NextDouble() * 0.45f;
                kit.Deco(PrimitiveType.Cube, p + Vector3.up * s * 0.3f, new Vector3(s, s * 0.6f, s * 0.8f), k % 3 == 0 ? dirt : turf, new Vector3((float)rng.NextDouble() * 40f, a, (float)rng.NextDouble() * 30f), root);
                if (k % 2 == 0) kit.Deco(PrimitiveType.Sphere, p + new Vector3(0.5f, 0.06f, 0.3f), new Vector3(0.22f, 0.12f, 0.18f), PatchworkAssets.Felt(new Color(0.75f, 0.72f, 0.68f)), default(Vector3), root);
            }
            // Bits that fell off: a bent cardboard panel, a brass bolt, a lost spring.
            Vector3 debris = c + Quaternion.Euler(0f, yaw + 130f, 0f) * new Vector3(0f, 0f, 4.6f);
            kit.Deco(PrimitiveType.Cube, debris + Vector3.up * 0.15f, new Vector3(1.2f, 0.06f, 0.8f), PatchworkAssets.Cardboard(Kraft), new Vector3(12f, 40f, 8f), root);
            kit.Deco(PrimitiveType.Cylinder, debris + new Vector3(0.9f, 0.08f, 0.5f), new Vector3(0.18f, 0.08f, 0.18f), PatchworkAssets.Plain(new Color(0.95f, 0.75f, 0.35f), 0.85f), new Vector3(90f, 0f, 30f), root);
            for (int k = 0; k < 4; k++)
                kit.Deco(PrimitiveType.Cylinder, debris + new Vector3(-0.8f, 0.05f, 0.4f + k * 0.07f), new Vector3(0.25f, 0.02f, 0.25f), PatchworkAssets.Plain(new Color(0.95f, 0.75f, 0.35f), 0.85f), new Vector3(80f, 0f, 0f), root);

            // The skid trench it ploughed coming in from the side: flattened grass between two
            // ridges of thrown-up earth, getting shallower the further back it goes.
            float trench = yaw + 96f;
            var back = Quaternion.Euler(0f, trench, 0f);
            for (int k = 0; k < 9; k++)
            {
                float d = outer + 1f + k * 2.1f;
                float fade = 1f - k / 9f;
                Vector3 mid = c + back * new Vector3(0f, 0f, d);
                kit.Deco(PrimitiveType.Cube, mid + Vector3.up * 0.015f, new Vector3(2.4f * (0.6f + fade * 0.4f), 0.01f, 2.3f), PatchworkAssets.Felt(new Color(0.42f, 0.55f, 0.32f)), new Vector3(0f, trench, 0f), root);
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Capsule, mid + back * new Vector3(s * 1.45f * (0.6f + fade * 0.4f), 0.12f * fade, 0f), new Vector3(0.6f * fade + 0.2f, 1.2f, 0.5f * fade + 0.15f), dirt, new Vector3(90f, trench, 0f), root);
            }
        }

        // One slab of the rim: a tilted box whose top runs from (r0, y0) to (r1, y1), facing out
        // along the given rotation.
        static void Slope(LevelKit kit, Vector3 c, Quaternion q, float r0, float r1, float y0, float y1, float width, Material mat)
        {
            const float thick = 0.6f;
            Vector3 a = new Vector3(0f, y0, r0), b = new Vector3(0f, y1, r1);
            Vector3 along = (b - a);
            float len = along.magnitude;
            Vector3 up = Vector3.Cross(along.normalized, Vector3.right);
            if (up.y < 0f) up = -up;
            Vector3 mid = (a + b) * 0.5f - up * thick * 0.5f;
            var t = kit.Block(c + q * mid, new Vector3(width, thick, len + 0.25f), Color.white, "CraterRim", mat);
            t.rotation = q * Quaternion.LookRotation(along.normalized, up);
        }
    }

    // Blinks something on and off (the Pod's antenna light).
    public class Blinker : MonoBehaviour
    {
        public float Period = 1.2f;
        Vector3 scale;
        void Start() { scale = transform.localScale; }
        void Update() { transform.localScale = Mathf.Repeat(Time.time, Period) < Period * 0.5f ? scale : scale * 0.4f; }
    }
}

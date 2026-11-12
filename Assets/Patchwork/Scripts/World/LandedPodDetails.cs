using UnityEngine;

namespace Patchwork
{
    // Everything that makes the landed Pod look lived-in and well-travelled, and the little yard
    // Patch has made round it since it came down.
    //
    // On the Pod itself:
    //   rivets down every seam, two more little portholes (with curtains, and a plant on the sill
    //   of the big one), a window box of flowers, a brass name plate over the door saying PATCH,
    //   a doorbell, a lantern on a hook with moths round it, a string of fairy lights round its
    //   middle, a shipping label with a barcode and a stamp, FRAGILE, THIS WAY UP (upside down,
    //   since it landed on its side), a tally of landings chalked by the door, stickers, dents,
    //   a sticking plaster, side exhausts glowing, a pressure gauge, a landing light on every
    //   leg, a splint tied on the bent one, a pinwheel and a little dish on the roof, and a snail
    //   who is very slowly climbing up the side.
    // Round it, in the crater:
    //   a doormat saying WELCOME and small footprints leading off into the level, a pot plant
    //   either side of the step, a letterbox with its flag up, a signpost (HOME, and THAT WAY),
    //   a washing line from a pole to the Pod with socks and a hanky drying, the repair kit by the
    //   bent leg, a picnic blanket, and a bucket and sponge for the soot.
    public partial class LandedPod
    {
        // Parts gathered by material, in a frame of their own, built as one mesh each.
        sealed class Bits
        {
            readonly System.Collections.Generic.List<Material> mats = new System.Collections.Generic.List<Material>();
            readonly System.Collections.Generic.List<MeshBatch> batches = new System.Collections.Generic.List<MeshBatch>();
            public readonly Transform Root;
            public Bits(Transform root) { Root = root; }
            public MeshBatch this[Material m]
            {
                get
                {
                    int i = mats.IndexOf(m);
                    if (i < 0) { mats.Add(m); batches.Add(new MeshBatch()); i = mats.Count - 1; }
                    return batches[i];
                }
            }
            public void Box(Material m, Vector3 c, Vector3 s, Vector3 euler = default(Vector3)) { this[m].Box(c, s, Quaternion.Euler(euler)); }
            public void Ball(Material m, Vector3 c, Vector3 s, int rings = 3, int segs = 7) { this[m].Ball(c, s, rings, segs); }
            public void Cyl(Material m, Vector3 bottom, float r, float h, int segs = 10) { this[m].Cylinder(bottom, r, h, segs); }
            // A rod from a to b, as a thin square bar.
            public void Bar(Material m, Vector3 a, Vector3 b, float thick)
            {
                Vector3 d = b - a;
                if (d.sqrMagnitude < 1e-6f) return;
                this[m].Box((a + b) * 0.5f, new Vector3(thick, thick, d.magnitude), Quaternion.LookRotation(d.normalized, Mathf.Abs(d.normalized.y) > 0.95f ? Vector3.forward : Vector3.up));
            }
            // A ring of short pieces in the frame's x-y plane (turned by euler), from..to degrees.
            public void Ring(Material m, Vector3 c, Vector3 euler, float r, float thick, int segs = 14, float from = 0f, float to = 360f)
            {
                var q = Quaternion.Euler(euler);
                float step = (to - from) * Mathf.Deg2Rad / segs;
                float piece = 2f * r * Mathf.Sin(step * 0.5f) * 1.1f;
                for (int i = 0; i < segs; i++)
                {
                    float a = from * Mathf.Deg2Rad + step * (i + 0.5f);
                    this[m].Box(c + q * new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f), new Vector3(thick, piece, thick), q * Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg));
                }
            }
            public void Done(string name, bool shadows = false)
            {
                for (int i = 0; i < mats.Count; i++) batches[i].Build(Root, name, mats[i], shadows);
                mats.Clear();
                batches.Clear();
            }
        }

        // A frame sitting on the shell at (degrees round from the front, height), lifted `lift`
        // off it, its +z pointing straight out and its +y as near to up as the curve allows.
        static Transform OnShell(LevelKit kit, Transform g, string name, float deg, float y, float lift, float roll = 0f)
        {
            Vector3 n;
            Vector3 p = Surface(deg, y, out n);
            var t = kit.Group(name, Vector3.zero);
            t.SetParent(g, false);
            t.localPosition = p + n * lift;
            t.localRotation = Quaternion.LookRotation(n, Vector3.up) * Quaternion.Euler(0f, 0f, roll);
            return t;
        }

        static Material Felt(Color c) { return PatchworkAssets.Felt(c); }
        static Material Shine(Color c, float s) { return PatchworkAssets.Plain(c, s); }
        static Material Glow(Color c, float k) { return PatchworkAssets.Glow(c, k); }

        static readonly Color Cream = new Color(0.99f, 0.95f, 0.88f), Red = new Color(0.9f, 0.3f, 0.35f), NavyInk = new Color(0.2f, 0.25f, 0.55f), Gold = new Color(1f, 0.8f, 0.28f);

        static void Details(LevelKit kit, Transform g, Vector3 feet, float yaw)
        {
            var brass = Shine(new Color(0.95f, 0.75f, 0.35f), 0.85f);
            var metal = Shine(new Color(0.85f, 0.86f, 0.9f), 0.7f);
            ShellRivets(g, brass);
            ShellMarks(kit, g);
            SidePortholes(kit, g, brass);
            DoorDetails(kit, g, brass);
            Doorway(kit, g, brass);
            FairyLights(kit, g);
            Plumbing(kit, g, brass, metal);
            LegDetails(kit, g, brass);
            Roof(kit, g, brass, metal);
            Snail(kit, g);

            // The yard: flat on the ground (not tipped like the Pod), facing the way the door does.
            var yard = kit.Group("PodYard", feet);
            yard.rotation = Quaternion.Euler(0f, yaw, 0f);
            Doorstep(kit, yard);
            Letterbox(kit, yard);
            Signpost(kit, yard);
            WashingLine(kit, g, yard);
            RepairKit(kit, yard, brass);
            Picnic(kit, yard);
            Bucket(kit, yard);
            Windsock(kit, yard);
            Supplies(kit, yard);
        }

        // ------------------------------------------------------------------ The shell

        static void ShellRivets(Transform g, Material brass)
        {
            // A rivet every hand's width down every seam, and round the top and bottom of the band.
            var mb = new MeshBatch();
            foreach (float r in new[] { 45f, 90f, 135f, 180f, 225f, 270f, 315f })
                for (float y = 1.1f; y <= Cy + Ry - 0.5f; y += 0.32f)
                {
                    if (Mathf.Abs(y - Cy) < 0.35f) continue;
                    Vector3 n;
                    Vector3 p = Surface(r + 3.2f, y, out n);
                    mb.Ball(p + n * 0.03f, Vector3.one * 0.07f, 2, 5);
                }
            for (int k = 0; k < 48; k++)
                foreach (int s in new[] { -1, 1 })
                {
                    Vector3 n;
                    Vector3 p = Surface(k * 7.5f + 3.75f, Cy + s * 0.31f, out n);
                    mb.Ball(p + n * 0.05f, Vector3.one * 0.055f, 2, 4);
                }
            mb.Build(g, "Rivets", brass);
        }

        static void ShellMarks(LevelKit kit, Transform g)
        {
            var ink = Felt(new Color(0.15f, 0.12f, 0.18f));
            var red = Felt(new Color(0.78f, 0.2f, 0.22f));

            // A shipping label: cream with a striped airmail border, an address scribble, a barcode
            // and a stamp in the corner.
            var label = OnShell(kit, g, "ShippingLabel", -150f, 3.3f, 0.02f, 6f);
            var b = new Bits(label);
            b.Box(Felt(Cream), Vector3.zero, new Vector3(1f, 0.66f, 0.01f));
            for (int i = 0; i < 12; i++)
            {
                var m = Felt(i % 2 == 0 ? new Color(0.85f, 0.2f, 0.25f) : new Color(0.25f, 0.35f, 0.75f));
                b.Box(m, new Vector3(-0.46f + i * 0.084f, 0.3f, 0.008f), new Vector3(0.06f, 0.04f, 0.004f), new Vector3(0f, 0f, 30f));
                b.Box(m, new Vector3(-0.46f + i * 0.084f, -0.3f, 0.008f), new Vector3(0.06f, 0.04f, 0.004f), new Vector3(0f, 0f, 30f));
            }
            for (int i = 0; i < 4; i++) b.Box(ink, new Vector3(-0.12f, 0.15f - i * 0.08f, 0.009f), new Vector3(0.5f - (i % 2) * 0.12f, 0.018f, 0.004f));
            // The barcode: thick and thin bars.
            float x = 0.12f;
            for (int i = 0; i < 18; i++)
            {
                float w = (i % 3 == 0 ? 0.016f : 0.007f);
                b.Box(ink, new Vector3(x, -0.15f, 0.009f), new Vector3(w, 0.16f, 0.004f));
                x += w + 0.008f;
            }
            b.Box(Felt(new Color(0.95f, 0.85f, 0.5f)), new Vector3(0.36f, 0.14f, 0.01f), new Vector3(0.17f, 0.2f, 0.005f));
            b.Ball(Felt(new Color(0.85f, 0.3f, 0.35f)), new Vector3(0.36f, 0.14f, 0.014f), new Vector3(0.08f, 0.08f, 0.004f), 2, 6);
            b.Ring(Felt(new Color(0.3f, 0.3f, 0.35f)), new Vector3(0.28f, 0.1f, 0.016f), Vector3.zero, 0.09f, 0.008f, 12);
            b.Done("ShippingLabel");

            // FRAGILE in big red letters with a broken glass, and THIS WAY UP, the arrows pointing
            // at the ground (it is lying on its side, after all).
            var fragile = OnShell(kit, g, "Fragile", 152f, 4.7f, 0.02f, -4f);
            var fb = new Bits(fragile);
            Level2Builder.Letters(fb[red], "FRAGILE", new Vector3(0f, 0f, 0.01f), Vector3.forward, 0.045f, 0.01f);
            fb.Ring(red, new Vector3(0f, -0.3f, 0.01f), Vector3.zero, 0.18f, 0.03f, 10, 180f, 360f);
            fb.Box(red, new Vector3(0f, -0.62f, 0.01f), new Vector3(0.03f, 0.3f, 0.01f));
            fb.Box(red, new Vector3(0f, -0.78f, 0.01f), new Vector3(0.22f, 0.03f, 0.01f));
            fb.Box(red, new Vector3(0.05f, -0.28f, 0.012f), new Vector3(0.02f, 0.12f, 0.01f), new Vector3(0f, 0f, 30f));
            fb.Done("Fragile");
            var up = OnShell(kit, g, "ThisWayUp", 98f, 2.5f, 0.02f, 180f);
            var ub = new Bits(up);
            Level2Builder.Letters(ub[ink], "THIS WAY UP", new Vector3(0f, -0.3f, 0.01f), Vector3.forward, 0.022f, 0.008f);
            foreach (int s in new[] { -1, 1 })
            {
                ub.Box(ink, new Vector3(s * 0.25f, 0.05f, 0.01f), new Vector3(0.05f, 0.35f, 0.008f));
                ub.Box(ink, new Vector3(s * 0.25f - 0.06f, 0.2f, 0.01f), new Vector3(0.04f, 0.16f, 0.008f), new Vector3(0f, 0f, -40f));
                ub.Box(ink, new Vector3(s * 0.25f + 0.06f, 0.2f, 0.01f), new Vector3(0.04f, 0.16f, 0.008f), new Vector3(0f, 0f, 40f));
            }
            ub.Done("ThisWayUp");

            // A tally of landings chalked beside the door: four and a cross-stroke, and one more.
            var tally = OnShell(kit, g, "LandingTally", 33f, 1.9f, 0.02f);
            var tb = new Bits(tally);
            var chalk = Felt(new Color(0.97f, 0.97f, 0.95f));
            for (int i = 0; i < 4; i++) tb.Box(chalk, new Vector3(-0.15f + i * 0.08f, 0f, 0.01f), new Vector3(0.02f, 0.26f, 0.005f), new Vector3(0f, 0f, (i % 2) * 4f - 2f));
            tb.Box(chalk, new Vector3(-0.03f, 0f, 0.012f), new Vector3(0.36f, 0.02f, 0.005f), new Vector3(0f, 0f, 28f));
            tb.Box(chalk, new Vector3(0.22f, 0f, 0.01f), new Vector3(0.02f, 0.26f, 0.005f));
            tb.Done("LandingTally");

            // Stickers: a smiling sun, two stars, a little rainbow.
            var stick = OnShell(kit, g, "Stickers", -32f, 4.9f, 0.02f, 10f);
            var sb = new Bits(stick);
            sb.Ball(Felt(new Color(1f, 0.82f, 0.3f)), Vector3.zero, new Vector3(0.3f, 0.3f, 0.01f), 2, 10);
            for (int i = 0; i < 8; i++) sb.Box(Felt(new Color(1f, 0.7f, 0.2f)), new Vector3(Mathf.Cos(i * 0.785f) * 0.21f, Mathf.Sin(i * 0.785f) * 0.21f, 0.002f), new Vector3(0.08f, 0.03f, 0.008f), new Vector3(0f, 0f, i * 45f));
            sb.Ball(ink, new Vector3(-0.05f, 0.04f, 0.008f), new Vector3(0.03f, 0.03f, 0.005f), 2, 4);
            sb.Ball(ink, new Vector3(0.05f, 0.04f, 0.008f), new Vector3(0.03f, 0.03f, 0.005f), 2, 4);
            sb.Ring(ink, new Vector3(0f, -0.01f, 0.008f), Vector3.zero, 0.07f, 0.015f, 8, 200f, 340f);
            for (int k = 0; k < 2; k++)
                for (int i = 0; i < 5; i++)
                    sb.Box(Glow(new Color(1f, 0.9f, 0.5f), 0.8f), new Vector3(0.38f + k * 0.2f, -0.25f + k * 0.35f, 0.006f), new Vector3(0.025f, 0.12f, 0.006f), new Vector3(0f, 0f, i * 36f));
            Color[] bow = { new Color(0.9f, 0.3f, 0.3f), new Color(1f, 0.7f, 0.25f), new Color(0.4f, 0.75f, 0.4f), new Color(0.35f, 0.5f, 0.9f) };
            for (int i = 0; i < bow.Length; i++) sb.Ring(Felt(bow[i]), new Vector3(-0.42f, -0.35f, 0.006f + i * 0.001f), Vector3.zero, 0.16f - i * 0.03f, 0.03f, 10, 0f, 180f);
            sb.Done("Stickers");

            // Dents knocked into the back, a sticking plaster over one, and a darned bit.
            foreach (var d in new[] { new Vector3(-170f, 4.4f, 0.55f), new Vector3(-130f, 5.6f, 0.4f), new Vector3(200f - 360f + 360f, 2.6f, 0.45f) })
            {
                var dent = OnShell(kit, g, "Dent", d.x, d.y, -0.02f);
                NoShadowDeco(kit, PrimitiveType.Sphere, Vector3.zero, new Vector3(d.z, d.z * 0.8f, 0.06f), PatchworkAssets.Cardboard(KraftDark), dent);
                NoShadowDeco(kit, PrimitiveType.Sphere, new Vector3(0.04f, 0.04f, 0.015f), new Vector3(d.z * 0.6f, d.z * 0.45f, 0.04f), PatchworkAssets.Cardboard(new Color(0.88f, 0.72f, 0.52f)), dent);
            }
            var plaster = OnShell(kit, g, "Plaster", -170f, 4.4f, 0.03f, 30f);
            var pb = new Bits(plaster);
            foreach (int s in new[] { 0, 1 })
            {
                var e = new Vector3(0f, 0f, s == 0 ? 35f : -35f);
                pb.Box(Felt(new Color(0.95f, 0.78f, 0.62f)), Vector3.zero, new Vector3(0.62f, 0.16f, 0.012f), e);
                pb.Box(Felt(new Color(0.98f, 0.92f, 0.85f)), new Vector3(0f, 0f, 0.004f), new Vector3(0.18f, 0.12f, 0.012f), e);
            }
            pb.Done("Plaster");
            var darn = OnShell(kit, g, "Darn", 75f, 1.8f, 0.02f);
            var db = new Bits(darn);
            var wool = PatchworkAssets.Yarn(new Color(0.35f, 0.6f, 0.4f));
            for (int i = 0; i < 7; i++)
            {
                db.Box(wool, new Vector3(-0.18f + i * 0.06f, 0f, 0.01f), new Vector3(0.025f, 0.42f, 0.02f));
                db.Box(wool, new Vector3(0f, -0.18f + i * 0.06f, 0.022f), new Vector3(0.42f, 0.025f, 0.02f));
            }
            db.Done("Darn");
        }

        static void NoShadowDeco(LevelKit kit, PrimitiveType t, Vector3 pos, Vector3 scale, Material m, Transform parent, Vector3 euler = default(Vector3))
        {
            var d = kit.Deco(t, pos, scale, m, euler, parent);
            var r = d.GetComponent<Renderer>();
            if (r != null) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // A small round window: brass rim, rivets, warm glass, and curtains drawn to either side.
        static void SmallPorthole(LevelKit kit, Transform g, float deg, float y, Material brass, Color curtain)
        {
            var w = OnShell(kit, g, "SmallPorthole", deg, y, 0.02f);
            var b = new Bits(w);
            b.Ring(brass, Vector3.zero, Vector3.zero, 0.36f, 0.09f, 20);
            for (int k = 0; k < 8; k++) b.Ball(brass, new Vector3(Mathf.Cos(k * 0.785f) * 0.36f, Mathf.Sin(k * 0.785f) * 0.36f, 0.05f), Vector3.one * 0.05f, 2, 4);
            b.Ball(Glow(new Color(1f, 0.82f, 0.5f), 1.2f), Vector3.zero, new Vector3(0.64f, 0.64f, 0.02f), 2, 14);
            var cloth = PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, curtain, Cream);
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(cloth, new Vector3(s * 0.19f, 0.02f, 0.03f), new Vector3(0.14f, 0.56f, 0.01f), new Vector3(0f, 0f, s * -6f));
                b.Box(Felt(Red), new Vector3(s * 0.2f, -0.06f, 0.04f), new Vector3(0.16f, 0.025f, 0.012f));
            }
            b.Box(Felt(curtain), new Vector3(0f, 0.25f, 0.035f), new Vector3(0.62f, 0.08f, 0.012f));
            b.Box(Shine(new Color(0.3f, 0.35f, 0.4f), 0.95f), new Vector3(-0.08f, 0.1f, 0.04f), new Vector3(0.12f, 0.04f, 0.006f), new Vector3(0f, 0f, 30f));
            b.Done("SmallPorthole");
        }

        static void SidePortholes(LevelKit kit, Transform g, Material brass)
        {
            SmallPorthole(kit, g, 70f, 4.6f, brass, new Color(0.35f, 0.55f, 0.9f));
            SmallPorthole(kit, g, -118f, 3.8f, brass, new Color(0.45f, 0.7f, 0.45f));

            // In the big porthole: curtains tied back, and a little plant on the sill inside.
            Vector3 wn;
            Vector3 wp = Surface(0f, 5.05f, out wn);
            var win = kit.Group("PortholeInside", Vector3.zero);
            win.SetParent(g, false);
            win.localPosition = wp + wn * 0.02f;
            win.localRotation = Quaternion.LookRotation(wn, Vector3.up);
            var b = new Bits(win);
            var cloth = PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(0.95f, 0.75f, 0.3f), Cream);
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(cloth, new Vector3(s * 0.44f, 0.05f, 0.05f), new Vector3(0.2f, 0.9f, 0.01f), new Vector3(0f, 0f, s * -8f));
                b.Box(Felt(Red), new Vector3(s * 0.45f, -0.1f, 0.06f), new Vector3(0.24f, 0.035f, 0.012f));
            }
            b.Box(Felt(new Color(0.95f, 0.75f, 0.3f)), new Vector3(0f, 0.42f, 0.055f), new Vector3(1.0f, 0.12f, 0.012f));
            // A silhouette of a pot plant on the sill, and the edge of a lampshade.
            var shade = Felt(new Color(0.35f, 0.25f, 0.2f));
            b.Box(shade, new Vector3(0.12f, -0.42f, 0.045f), new Vector3(0.16f, 0.14f, 0.006f));
            for (int i = 0; i < 5; i++) b.Ball(shade, new Vector3(0.12f + (i - 2) * 0.05f, -0.28f + Mathf.Abs(i - 2) * -0.03f, 0.045f), new Vector3(0.07f, 0.12f, 0.006f), 2, 5);
            b.Box(shade, new Vector3(-0.22f, -0.2f, 0.045f), new Vector3(0.22f, 0.12f, 0.006f), new Vector3(0f, 0f, 8f));
            b.Box(Felt(Cream), new Vector3(0f, -0.52f, 0.055f), new Vector3(0.9f, 0.04f, 0.02f));
            b.Done("PortholeInside");
        }

        // ------------------------------------------------------------------ The door

        static void DoorDetails(LevelKit kit, Transform g, Material brass)
        {
            // The name plate over the door.
            var plate = OnShell(kit, g, "NamePlate", 0f, 3.5f, 0.03f);
            var pb = new Bits(plate);
            pb.Box(brass, Vector3.zero, new Vector3(0.86f, 0.26f, 0.03f));
            pb.Box(Felt(NavyInk), new Vector3(0f, 0f, 0.016f), new Vector3(0.78f, 0.19f, 0.01f));
            Level2Builder.Letters(pb[Felt(Gold)], "PATCH", new Vector3(0f, 0f, 0.022f), Vector3.forward, 0.024f, 0.008f);
            foreach (int s in new[] { -1, 1 }) pb.Ball(brass, new Vector3(s * 0.38f, 0f, 0.025f), Vector3.one * 0.05f, 2, 5);
            pb.Done("NamePlate", true);

            // A window box under the porthole: cream slats, bright felt flowers and leaves.
            var box = OnShell(kit, g, "WindowBox", 0f, 4.02f, 0.02f);
            var bb = new Bits(box);
            var wood = PatchworkAssets.Planks(new Color(0.95f, 0.9f, 0.8f));
            bb.Box(wood, new Vector3(0f, 0f, 0.14f), new Vector3(1.1f, 0.2f, 0.26f));
            bb.Box(Felt(new Color(0.35f, 0.25f, 0.18f)), new Vector3(0f, 0.1f, 0.14f), new Vector3(1.02f, 0.02f, 0.2f));
            foreach (int s in new[] { -1, 1 }) bb.Box(brass, new Vector3(s * 0.4f, -0.12f, 0.08f), new Vector3(0.04f, 0.12f, 0.14f), new Vector3(30f, 0f, 0f));
            Color[] petals = { new Color(1f, 0.55f, 0.65f), new Color(1f, 0.85f, 0.35f), new Color(0.75f, 0.6f, 1f), Color.white, new Color(1f, 0.5f, 0.3f) };
            for (int i = 0; i < 9; i++)
            {
                float x = -0.44f + i * 0.11f;
                Vector3 head = new Vector3(x, 0.26f + (i % 3) * 0.05f, 0.18f + (i % 2) * 0.05f);
                bb.Bar(Felt(new Color(0.3f, 0.55f, 0.3f)), new Vector3(x, 0.1f, 0.16f), head, 0.015f);
                bb.Ball(Felt(petals[i % petals.Length]), head, new Vector3(0.1f, 0.1f, 0.05f), 2, 7);
                bb.Ball(Felt(new Color(1f, 0.9f, 0.4f)), head + new Vector3(0f, 0f, 0.025f), Vector3.one * 0.035f, 2, 4);
                bb.Ball(Felt(new Color(0.35f, 0.6f, 0.35f)), new Vector3(x + 0.04f, 0.15f, 0.24f), new Vector3(0.09f, 0.03f, 0.06f), 2, 5);
            }
            bb.Done("WindowBox", true);

            // A doorbell: a brass button on a round plate, with a tiny label.
            var bell = OnShell(kit, g, "Doorbell", -27f, 2.4f, 0.02f);
            var dbb = new Bits(bell);
            dbb.Ball(brass, Vector3.zero, new Vector3(0.22f, 0.22f, 0.03f), 2, 10);
            dbb.Ball(Felt(Red), new Vector3(0f, 0f, 0.04f), new Vector3(0.09f, 0.09f, 0.06f), 2, 7);
            dbb.Box(Felt(Cream), new Vector3(0f, -0.18f, 0.01f), new Vector3(0.2f, 0.07f, 0.01f));
            Level2Builder.Letters(dbb[Felt(new Color(0.2f, 0.2f, 0.25f))], "RING", new Vector3(0f, -0.18f, 0.016f), Vector3.forward, 0.008f, 0.004f);
            dbb.Done("Doorbell", true);

            // The lantern on a hook, glowing, with two moths going round it.
            var hook = OnShell(kit, g, "DoorLantern", 28f, 3.05f, 0.02f);
            var hb = new Bits(hook);
            var iron = Shine(new Color(0.15f, 0.14f, 0.16f), 0.5f);
            hb.Box(iron, Vector3.zero, new Vector3(0.12f, 0.2f, 0.04f));
            hb.Bar(iron, new Vector3(0f, 0.05f, 0.02f), new Vector3(0f, 0.12f, 0.4f), 0.03f);
            hb.Bar(iron, new Vector3(0f, 0.12f, 0.4f), new Vector3(0f, 0.02f, 0.42f), 0.02f);
            Vector3 lamp = new Vector3(0f, -0.2f, 0.42f);
            hb.Box(iron, lamp + Vector3.up * 0.17f, new Vector3(0.2f, 0.04f, 0.2f));
            hb.Box(iron, lamp + Vector3.up * 0.22f, new Vector3(0.12f, 0.06f, 0.12f), new Vector3(0f, 45f, 0f));
            hb.Box(iron, lamp - Vector3.up * 0.17f, new Vector3(0.18f, 0.04f, 0.18f));
            foreach (int s in new[] { -1, 1 }) foreach (int t in new[] { -1, 1 }) hb.Box(iron, lamp + new Vector3(s * 0.08f, 0f, t * 0.08f), new Vector3(0.02f, 0.34f, 0.02f));
            hb.Ball(Glow(new Color(1f, 0.78f, 0.4f), 2.6f), lamp, new Vector3(0.13f, 0.22f, 0.13f), 3, 7);
            hb.Done("DoorLantern", true);
            var light = new GameObject("DoorLanternLight").AddComponent<Light>();
            light.transform.SetParent(hook, false);
            light.transform.localPosition = lamp + Vector3.forward * 0.1f;
            light.type = LightType.Point;
            light.color = new Color(1f, 0.78f, 0.45f);
            light.range = 5f;
            light.intensity = 1.2f;
            light.shadows = LightShadows.None;
            var moths = kit.Group("Moths", hook.TransformPoint(lamp));
            moths.SetParent(hook, true);
            moths.localRotation = Quaternion.identity;
            for (int m = 0; m < 2; m++)
            {
                var moth = kit.Group("Moth", moths.TransformPoint(new Vector3(m == 0 ? 0.28f : -0.2f, m * 0.08f, m == 0 ? 0f : 0.18f)));
                moth.SetParent(moths, true);
                var mb = new Bits(moth);
                mb.Ball(Felt(new Color(0.85f, 0.8f, 0.7f)), Vector3.zero, new Vector3(0.03f, 0.03f, 0.06f), 2, 4);
                foreach (int s in new[] { -1, 1 }) mb.Box(Felt(new Color(0.95f, 0.92f, 0.85f)), new Vector3(s * 0.04f, 0.01f, 0f), new Vector3(0.06f, 0.005f, 0.05f), new Vector3(0f, 0f, s * 25f));
                mb.Done("Moth");
                var flap = moth.gameObject.AddComponent<RmSway>();
                flap.Amount = 25f;
                flap.Speed = 14f + m * 3f;
            }
            var spin = moths.gameObject.AddComponent<Spinner>();
            spin.Axis = Vector3.up;
            spin.Speed = 120f;
        }

        // Just inside the hatch, seen when the door swings open: a warm lamplit hallway, a coat
        // and a scarf on a hook, a picture on the wall and the end of a rag rug.
        static void Doorway(LevelKit kit, Transform g, Material brass)
        {
            Vector3 dn;
            Vector3 dp = Surface(0f, 2.15f, out dn);
            var hall = kit.Group("Doorway", Vector3.zero);
            hall.SetParent(g, false);
            hall.localPosition = dp;
            hall.localRotation = Quaternion.LookRotation(dn, Vector3.up);
            var b = new Bits(hall);
            // (Everything sits between the dark doorway, at 0.16, and the back of the closed door, at 0.13: hidden inside the door until it swings open.)
            b.Ball(Glow(new Color(1f, 0.7f, 0.4f), 0.7f), new Vector3(0f, 0.05f, 0.165f), new Vector3(1.3f, 1.9f, 0.01f), 2, 16);
            b.Ball(Glow(new Color(1f, 0.85f, 0.6f), 1.2f), new Vector3(0f, 0.3f, 0.17f), new Vector3(0.7f, 0.9f, 0.01f), 2, 14);
            var dark = Felt(new Color(0.3f, 0.2f, 0.18f));
            b.Box(dark, new Vector3(-0.42f, 0.5f, 0.18f), new Vector3(0.18f, 0.05f, 0.01f));
            b.Box(Felt(new Color(0.3f, 0.45f, 0.7f)), new Vector3(-0.42f, 0.25f, 0.18f), new Vector3(0.24f, 0.5f, 0.01f));
            b.Box(PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, Red, Cream), new Vector3(-0.33f, 0.2f, 0.185f), new Vector3(0.06f, 0.55f, 0.01f));
            b.Box(brass, new Vector3(0.38f, 0.45f, 0.18f), new Vector3(0.26f, 0.22f, 0.01f));
            b.Box(Felt(new Color(0.55f, 0.75f, 0.95f)), new Vector3(0.38f, 0.45f, 0.185f), new Vector3(0.2f, 0.16f, 0.01f));
            b.Ball(Felt(new Color(0.45f, 0.7f, 0.4f)), new Vector3(0.38f, 0.41f, 0.19f), new Vector3(0.18f, 0.06f, 0.005f), 2, 6);
            Color[] rag = { Red, Gold, new Color(0.4f, 0.65f, 0.9f), new Color(0.45f, 0.75f, 0.45f) };
            for (int i = 0; i < 4; i++) b.Box(Felt(rag[i]), new Vector3(0f, -0.58f + i * 0.035f, 0.18f), new Vector3(0.9f - i * 0.12f, 0.03f, 0.01f));
            b.Done("Doorway");
        }

        // ------------------------------------------------------------------ Lights round the middle

        static void FairyLights(LevelKit kit, Transform g)
        {
            // A string of little bulbs looped round the Pod's middle, sagging between hooks,
            // twinkling in four colours, each colour on its own slow beat.
            Color[] cols = { new Color(1f, 0.5f, 0.45f), new Color(1f, 0.85f, 0.35f), new Color(0.45f, 0.85f, 0.6f), new Color(0.5f, 0.65f, 1f) };
            var bulbs = new MeshBatch[cols.Length];
            for (int i = 0; i < bulbs.Length; i++) bulbs[i] = new MeshBatch();
            var wire = new MeshBatch();
            const int hooks = 16, per = 4;
            int n = 0;
            for (int h = 0; h < hooks; h++)
            {
                float a0 = h * 360f / hooks, a1 = (h + 1) * 360f / hooks;
                Vector3 prev = Vector3.zero;
                for (int k = 0; k <= per; k++)
                {
                    float t = k / (float)per;
                    float a = Mathf.Lerp(a0, a1, t) * Mathf.Deg2Rad;
                    float sag = Mathf.Sin(t * Mathf.PI) * 0.22f;
                    float r = Rx * Mathf.Sqrt(1f - Mathf.Pow((Cy - 0.42f - Cy) / Ry, 2f)) + 0.07f;
                    Vector3 p = new Vector3(Mathf.Sin(a) * r, Cy - 0.42f - sag, Mathf.Cos(a) * r);
                    if (k > 0)
                    {
                        Vector3 d = p - prev;
                        wire.Box((p + prev) * 0.5f, new Vector3(0.012f, 0.012f, d.magnitude), Quaternion.LookRotation(d.normalized));
                    }
                    if (k > 0 && k < per)
                    {
                        bulbs[n % cols.Length].Ball(p + Vector3.down * 0.06f, new Vector3(0.07f, 0.1f, 0.07f), 2, 6);
                        n++;
                    }
                    prev = p;
                }
            }
            wire.Build(g, "FairyWire", PatchworkAssets.Plain(new Color(0.15f, 0.3f, 0.18f), 0.4f));
            var twinkle = new GameObject("FairyLights").transform;
            twinkle.SetParent(g, false);
            var mats = new Material[cols.Length];
            for (int i = 0; i < cols.Length; i++)
            {
                mats[i] = PatchworkAssets.Unique(PatchworkAssets.Glow(cols[i], 1.8f));
                bulbs[i].Build(twinkle, "Bulbs", mats[i]);
            }
            var tw = twinkle.gameObject.AddComponent<PodTwinkle>();
            tw.Mats = mats;
            tw.Cols = cols;
        }

        // ------------------------------------------------------------------ Pipes, gauge, exhausts

        static void Plumbing(LevelKit kit, Transform g, Material brass, Material metal)
        {
            // A pressure gauge low down by the door, its needle never quite settling, on a curl of pipe.
            var gauge = OnShell(kit, g, "PressureGauge", -40f, 1.45f, 0.03f);
            var b = new Bits(gauge);
            b.Ring(brass, new Vector3(0f, 0f, 0.05f), Vector3.zero, 0.17f, 0.05f, 16);
            b.Ball(Felt(Cream), new Vector3(0f, 0f, 0.045f), new Vector3(0.3f, 0.3f, 0.01f), 2, 12);
            for (int i = 0; i < 7; i++)
            {
                float a = (-60f + i * 20f) * Mathf.Deg2Rad;
                b.Box(Felt(i >= 5 ? new Color(0.85f, 0.2f, 0.2f) : new Color(0.15f, 0.15f, 0.2f)), new Vector3(Mathf.Sin(a) * 0.11f, Mathf.Cos(a) * 0.11f, 0.055f), new Vector3(0.012f, 0.035f, 0.004f), new Vector3(0f, 0f, -(-60f + i * 20f)));
            }
            b.Bar(brass, new Vector3(0f, -0.17f, 0.04f), new Vector3(0f, -0.35f, 0.04f), 0.05f);
            b.Bar(brass, new Vector3(0f, -0.35f, 0.04f), new Vector3(0.3f, -0.42f, 0.02f), 0.05f);
            b.Ball(brass, new Vector3(0f, -0.35f, 0.04f), Vector3.one * 0.08f, 2, 5);
            b.Done("PressureGauge", true);
            var needle = new GameObject("Needle").transform;
            needle.SetParent(gauge, false);
            needle.localPosition = new Vector3(0f, 0f, 0.06f);
            NoShadowDeco(kit, PrimitiveType.Cube, new Vector3(0f, 0.05f, 0f), new Vector3(0.014f, 0.1f, 0.004f), Felt(new Color(0.8f, 0.1f, 0.1f)), needle);
            needle.gameObject.AddComponent<GaugeWaver>();

            // Two little exhaust pipes out of the back, still glowing at the tips.
            foreach (float deg in new[] { 152f, 208f })
            {
                Vector3 n;
                Vector3 p = Surface(deg, 1.25f, out n);
                Vector3 outward = new Vector3(n.x, 0f, n.z).normalized;
                var pipe = kit.Group("Exhaust", Vector3.zero);
                pipe.SetParent(g, false);
                pipe.localPosition = p;
                pipe.localRotation = Quaternion.LookRotation((outward + Vector3.down * 0.45f).normalized, Vector3.up);
                var pb = new Bits(pipe);
                for (int k = 0; k < 4; k++) pb.Ring(metal, new Vector3(0f, 0f, 0.1f + k * 0.12f), Vector3.zero, 0.12f, 0.07f, 12);
                pb.Ring(brass, new Vector3(0f, 0f, 0.56f), Vector3.zero, 0.14f, 0.05f, 12);
                pb.Ball(Glow(new Color(1f, 0.45f, 0.2f), 1.8f), new Vector3(0f, 0f, 0.55f), new Vector3(0.2f, 0.2f, 0.03f), 2, 8);
                pb.Done("Exhaust", true);
                var heat = new GameObject("ExhaustSmoke");
                heat.transform.SetParent(pipe, false);
                heat.transform.localPosition = new Vector3(0f, 0f, 0.7f);
                var sm = heat.AddComponent<Smoker>();
                sm.Every = 4.5f + deg * 0.01f;
                sm.Kind = FxBurst.Kind.Puff;
            }
        }

        // ------------------------------------------------------------------ The legs

        static void LegDetails(LevelKit kit, Transform g, Material brass)
        {
            for (int k = 0; k < 3; k++)
            {
                float deg = k * 120f + 60f;
                bool bent = k == 1;
                Vector3 ln;
                Vector3 top = Surface(deg, 1.7f, out ln) - ln * 0.1f;
                Vector3 dir = new Vector3(ln.x, 0f, ln.z).normalized;
                Vector3 knee = top + dir * 0.7f + Vector3.down * 0.7f;
                Vector3 foot = bent ? knee + dir * 0.9f + Vector3.down * 0.1f : top + dir * 1.3f + Vector3.down * 1.6f;
                var b = new Bits(g);
                // A mounting plate with bolts where each leg meets the shell, and a landing light.
                var plate = OnShell(kit, g, "LegMount", deg, 1.85f, 0.02f);
                var mb = new Bits(plate);
                mb.Box(brass, Vector3.zero, new Vector3(0.5f, 0.34f, 0.04f));
                foreach (int s in new[] { -1, 1 }) foreach (int t in new[] { -1, 1 }) mb.Ball(Shine(new Color(0.6f, 0.6f, 0.65f), 0.8f), new Vector3(s * 0.18f, t * 0.11f, 0.03f), Vector3.one * 0.06f, 2, 5);
                mb.Done("LegMount", true);
                var lamp = NoShadowDecoReturn(kit, PrimitiveType.Sphere, Surface(deg + 14f, 1.55f, out Vector3 lmn) + lmn * 0.06f, Vector3.one * 0.13f, Glow(k == 1 ? new Color(1f, 0.35f, 0.3f) : new Color(0.4f, 1f, 0.55f), 2.2f), g);
                var blink = lamp.gameObject.AddComponent<Blinker>();
                blink.Period = 0.9f + k * 0.37f;
                // A grease nipple on each knee, and tufts of grass caught in the feet.
                b.Ball(Shine(new Color(0.9f, 0.85f, 0.4f), 0.8f), knee + dir * 0.12f + Vector3.up * 0.1f, Vector3.one * 0.07f, 2, 5);
                var grass = Felt(new Color(0.45f, 0.65f, 0.35f));
                for (int t = 0; t < 4; t++)
                {
                    Vector3 side = Vector3.Cross(dir, Vector3.up);
                    Vector3 root = foot + side * ((t - 1.5f) * 0.14f) + dir * 0.3f;
                    b.Bar(grass, root, root + Vector3.up * 0.16f + dir * 0.06f, 0.025f);
                }
                if (bent)
                {
                    // A splint tied on the buckled leg: two wooden slats and wraps of yarn.
                    Vector3 side = Vector3.Cross((foot - knee).normalized, Vector3.up).normalized;
                    var slat = PatchworkAssets.Planks(new Color(0.75f, 0.6f, 0.4f));
                    var tie = PatchworkAssets.Yarn(new Color(0.9f, 0.3f, 0.35f));
                    Vector3 a = Vector3.Lerp(knee, top, 0.35f), c = Vector3.Lerp(knee, foot, 0.75f);
                    foreach (int s in new[] { -1, 1 })
                        b.Bar(slat, a + side * s * 0.13f, c + side * s * 0.13f, 0.07f);
                    for (int w = 0; w < 4; w++)
                    {
                        Vector3 at = Vector3.Lerp(a, c, 0.15f + w * 0.24f);
                        b.Bar(tie, at - side * 0.18f, at + side * 0.18f, 0.06f);
                    }
                    b.Bar(tie, Vector3.Lerp(a, c, 0.9f) + side * 0.18f, Vector3.Lerp(a, c, 0.9f) + side * 0.3f + Vector3.down * 0.2f, 0.03f);
                }
                b.Done("LegBits", true);
            }
        }

        static Transform NoShadowDecoReturn(LevelKit kit, PrimitiveType t, Vector3 pos, Vector3 scale, Material m, Transform parent)
        {
            var d = kit.Deco(t, pos, scale, m, default(Vector3), parent);
            var r = d.GetComponent<Renderer>();
            if (r != null) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return d;
        }

        // ------------------------------------------------------------------ The roof

        static void Roof(LevelKit kit, Transform g, Material brass, Material metal)
        {
            // A pinwheel on a stick, turning in the breeze.
            Vector3 stick = new Vector3(0.05f, Cy + Ry * Mathf.Sqrt(1f - 0.7f * 0.7f / (Rx * Rx)) - 0.05f, 0.7f);
            var b = new Bits(g);
            b.Bar(PatchworkAssets.Planks(new Color(0.9f, 0.85f, 0.75f)), stick, stick + Vector3.up * 1.1f, 0.05f);
            var hub = kit.Group("Pinwheel", Vector3.zero);
            hub.SetParent(g, false);
            hub.localPosition = stick + Vector3.up * 1.1f + Vector3.forward * 0.06f;
            hub.localRotation = Quaternion.identity;
            var pb = new Bits(hub);
            Color[] cols = { Red, Gold, new Color(0.35f, 0.65f, 0.95f), new Color(0.45f, 0.8f, 0.45f) };
            for (int i = 0; i < 4; i++)
            {
                var q = Quaternion.Euler(0f, 0f, i * 90f);
                pb.Box(Felt(cols[i]), q * new Vector3(0.15f, 0.07f, 0f), new Vector3(0.3f, 0.14f, 0.01f), new Vector3(0f, 20f, i * 90f));
                pb.Box(Felt(cols[i]), q * new Vector3(0.22f, 0.13f, 0.02f), new Vector3(0.12f, 0.1f, 0.01f), new Vector3(30f, 0f, i * 90f + 45f));
            }
            pb.Ball(brass, Vector3.zero, Vector3.one * 0.07f, 2, 5);
            pb.Done("Pinwheel");
            var spin = hub.gameObject.AddComponent<Spinner>();
            spin.Axis = Vector3.forward;
            spin.Speed = 140f;

            // A little dish on the back of the roof, listening for home.
            Vector3 dishFoot = new Vector3(-0.3f, Cy + Ry * Mathf.Sqrt(1f - 1.3f * 1.3f / (Rx * Rx)) - 0.05f, -1.3f);
            b.Bar(metal, dishFoot, dishFoot + new Vector3(0f, 0.55f, -0.1f), 0.06f);
            var dish = kit.Group("Dish", Vector3.zero);
            dish.SetParent(g, false);
            dish.localPosition = dishFoot + new Vector3(0f, 0.6f, -0.12f);
            dish.localRotation = Quaternion.Euler(-50f, 200f, 0f);
            var dbits = new Bits(dish);
            for (int k = 0; k < 4; k++) dbits.Ring(metal, new Vector3(0f, 0f, k * 0.05f), Vector3.zero, 0.1f + k * 0.1f, 0.05f, 12 + k * 4);
            dbits.Bar(metal, Vector3.zero, new Vector3(0f, 0f, 0.4f), 0.025f);
            dbits.Ball(Glow(new Color(0.5f, 0.85f, 1f), 1.5f), new Vector3(0f, 0f, 0.42f), Vector3.one * 0.06f, 2, 5);
            dbits.Done("Dish", true);

            // A rain gutter of brass round the top seam of the chimney, and a weathercock arrow.
            Vector3 chim = new Vector3(-0.6f, Cy + Ry + 0.1f, -0.4f);
            b.Bar(Shine(new Color(0.15f, 0.14f, 0.16f), 0.5f), chim + new Vector3(0.45f, -0.05f, 0f), chim + new Vector3(0.45f, 0.75f, 0f), 0.03f);
            var vane = kit.Group("WeatherVane", Vector3.zero);
            vane.SetParent(g, false);
            vane.localPosition = chim + new Vector3(0.45f, 0.75f, 0f);
            vane.localRotation = Quaternion.identity;
            var vb = new Bits(vane);
            var black = Shine(new Color(0.15f, 0.14f, 0.16f), 0.5f);
            vb.Box(black, Vector3.zero, new Vector3(0.03f, 0.03f, 0.6f));
            vb.Box(black, new Vector3(0f, 0f, 0.3f), new Vector3(0.1f, 0.1f, 0.02f), new Vector3(0f, 0f, 45f));
            vb.Box(Felt(Gold), new Vector3(0f, 0.06f, -0.26f), new Vector3(0.02f, 0.16f, 0.14f));
            vb.Done("WeatherVane", true);
            var turn = vane.gameObject.AddComponent<Spinner>();
            turn.Axis = Vector3.up;
            turn.Speed = 9f;
            b.Done("RoofBits", true);
        }

        // ------------------------------------------------------------------ A snail

        static void Snail(LevelKit kit, Transform g)
        {
            // A snail who is climbing up the side of the Pod, very, very slowly, and has left a
            // shiny trail behind it.
            Vector3 n0, n1;
            Vector3 a = Surface(-28f, 1.25f, out n0) + n0 * 0.06f;
            Vector3 c = Surface(-24f, 2.05f, out n1) + n1 * 0.06f;
            var trail = new MeshBatch();
            for (int i = 0; i < 10; i++)
            {
                Vector3 n;
                Vector3 p = Surface(-28f - Mathf.Sin(i * 0.8f) * 1.5f, 0.55f + i * 0.07f, out n) + n * 0.015f;
                trail.Box(p, new Vector3(0.05f, 0.08f, 0.005f), Quaternion.LookRotation(n, Vector3.up));
            }
            trail.Build(g, "SnailTrail", PatchworkAssets.Plain(new Color(0.85f, 0.9f, 0.95f), 0.98f));
            var snail = kit.Group("Snail", g.TransformPoint(a));
            snail.rotation = g.rotation * Quaternion.LookRotation(c - a, n0);
            var b = new Bits(snail);
            var body = Felt(new Color(0.8f, 0.7f, 0.55f));
            b.Ball(body, new Vector3(0f, 0.02f, 0f), new Vector3(0.08f, 0.05f, 0.24f), 2, 6);
            b.Ball(body, new Vector3(0f, 0.04f, 0.12f), new Vector3(0.06f, 0.06f, 0.07f), 2, 5);
            foreach (int s in new[] { -1, 1 })
            {
                b.Bar(body, new Vector3(s * 0.015f, 0.06f, 0.14f), new Vector3(s * 0.035f, 0.13f, 0.17f), 0.012f);
                b.Ball(Felt(new Color(0.15f, 0.12f, 0.12f)), new Vector3(s * 0.035f, 0.135f, 0.17f), Vector3.one * 0.02f, 2, 4);
            }
            var shell = Felt(new Color(0.85f, 0.5f, 0.35f));
            var band = Felt(new Color(0.65f, 0.35f, 0.25f));
            for (int k = 0; k < 5; k++)
            {
                float r = 0.08f - k * 0.013f;
                b.Ball(k % 2 == 0 ? shell : band, new Vector3(0f, 0.09f + k * 0.012f, -0.03f + k * 0.012f), new Vector3(0.07f, r * 2f, r * 2f), 2, 7);
            }
            b.Done("Snail", true);
            var climb = snail.gameObject.AddComponent<Glider>();
            climb.A = g.TransformPoint(a);
            climb.B = g.TransformPoint(c);
            climb.Travel = 60f;
            climb.Rest = 10f;
        }

        // ------------------------------------------------------------------ The yard

        static void Doorstep(LevelKit kit, Transform yard)
        {
            var b = new Bits(yard);
            // A doormat: coir-coloured with a red border, WELCOME in dark letters.
            Vector3 mat = new Vector3(0f, 0.04f, 3.45f);
            b.Box(Felt(new Color(0.75f, 0.58f, 0.35f)), mat, new Vector3(1.4f, 0.03f, 0.8f));
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(Felt(Red), mat + new Vector3(s * 0.66f, 0.003f, 0f), new Vector3(0.06f, 0.032f, 0.78f));
                b.Box(Felt(Red), mat + new Vector3(0f, 0.003f, s * 0.36f), new Vector3(1.38f, 0.032f, 0.06f));
            }
            var lettersAt = mat + Vector3.up * 0.02f;
            var text = new MeshBatch();
            Level2Builder.Letters(text, "WELCOME", Vector3.zero, Vector3.forward, 0.03f, 0.01f);
            var tg = kit.Group("WelcomeLetters", yard.TransformPoint(lettersAt));
            // (Lying face up, the tops of the letters towards the Pod, so it reads as you walk up.)
            tg.rotation = yard.rotation * Quaternion.Euler(-90f, 0f, 0f);
            text.Build(tg, "Welcome", Felt(new Color(0.3f, 0.2f, 0.12f)));
            // Two pot plants either side of the step: a round bush and a tall fern.
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 pot = new Vector3(s * 1.3f, 0f, 2.7f);
                var clay = Felt(new Color(0.78f, 0.45f, 0.32f));
                for (int i = 0; i < 4; i++) b.Ring(clay, pot + Vector3.up * (0.06f + i * 0.09f), new Vector3(90f, 0f, 0f), 0.2f + i * 0.02f, 0.09f, 14);
                b.Cyl(clay, pot, 0.19f, 0.06f, 12);
                b.Cyl(Felt(new Color(0.3f, 0.22f, 0.16f)), pot + Vector3.up * 0.33f, 0.27f, 0.03f, 12);
                b.Ring(Felt(new Color(0.95f, 0.85f, 0.6f)), pot + Vector3.up * 0.38f, new Vector3(90f, 0f, 0f), 0.29f, 0.06f, 14);
                if (s < 0)
                {
                    b.Ball(Felt(new Color(0.3f, 0.55f, 0.32f)), pot + Vector3.up * 0.65f, new Vector3(0.62f, 0.55f, 0.62f), 3, 8);
                    for (int f = 0; f < 6; f++) b.Ball(Felt(new Color(1f, 0.6f, 0.7f)), pot + new Vector3(Mathf.Cos(f) * 0.25f, 0.7f + Mathf.Sin(f * 2.3f) * 0.12f, Mathf.Sin(f) * 0.25f), Vector3.one * 0.08f, 2, 5);
                }
                else
                {
                    for (int f = 0; f < 9; f++)
                    {
                        float a = f * 0.7f;
                        Vector3 tip = pot + new Vector3(Mathf.Cos(a) * 0.4f, 0.95f + (f % 3) * 0.1f, Mathf.Sin(a) * 0.4f);
                        b.Bar(Felt(new Color(0.35f, 0.6f, 0.3f)), pot + Vector3.up * 0.38f, tip, 0.05f);
                        b.Ball(Felt(new Color(0.4f, 0.68f, 0.35f)), tip, new Vector3(0.12f, 0.04f, 0.12f), 2, 5);
                    }
                }
                TownKit.Solid(kit, yard.TransformPoint(pot + Vector3.up * 0.3f), new Vector3(0.5f, 0.6f, 0.5f), yard.eulerAngles.y);
            }
            // Small footprints leading out from the step into the level.
            var print = Felt(new Color(0.35f, 0.26f, 0.2f));
            for (int i = 0; i < 8; i++)
            {
                float side = i % 2 == 0 ? -0.13f : 0.13f;
                Vector3 p = new Vector3(side + Mathf.Sin(i * 0.5f) * 0.1f, 0.035f, 4.0f + i * 0.45f);
                b.Ball(print, p, new Vector3(0.13f, 0.012f, 0.22f), 2, 6);
                b.Ball(print, p + new Vector3(0f, 0f, 0.16f), new Vector3(0.1f, 0.012f, 0.08f), 2, 5);
            }
            b.Done("Doorstep", true);
        }

        static void Letterbox(LevelKit kit, Transform yard)
        {
            Vector3 at = new Vector3(3.3f, 0f, 3.1f);
            var lb = kit.Group("Letterbox", yard.TransformPoint(at));
            lb.rotation = yard.rotation * Quaternion.Euler(0f, -30f, 0f);
            var b = new Bits(lb);
            var post = PatchworkAssets.Planks(new Color(0.55f, 0.4f, 0.28f));
            b.Box(post, new Vector3(0f, 0.55f, 0f), new Vector3(0.12f, 1.1f, 0.12f));
            var blue = Shine(new Color(0.25f, 0.4f, 0.7f), 0.6f);
            b.Box(blue, new Vector3(0f, 1.25f, 0f), new Vector3(0.36f, 0.3f, 0.55f));
            for (int i = 0; i < 7; i++)
            {
                float a = i / 6f * Mathf.PI;
                b.Box(blue, new Vector3(Mathf.Cos(a) * 0.15f, 1.4f + Mathf.Sin(a) * 0.15f, 0f), new Vector3(0.08f, 0.08f, 0.55f), new Vector3(0f, 0f, a * Mathf.Rad2Deg));
            }
            b.Box(Felt(Cream), new Vector3(0f, 1.3f, 0.28f), new Vector3(0.3f, 0.36f, 0.01f));
            b.Ball(Shine(new Color(0.95f, 0.75f, 0.35f), 0.85f), new Vector3(0f, 1.3f, 0.29f), Vector3.one * 0.05f, 2, 5);
            // The flag up: there's post! A letter is sticking out of the top of the lid.
            b.Box(Shine(new Color(0.85f, 0.2f, 0.2f), 0.6f), new Vector3(0.2f, 1.45f, -0.1f), new Vector3(0.02f, 0.4f, 0.04f));
            b.Box(Shine(new Color(0.85f, 0.2f, 0.2f), 0.6f), new Vector3(0.2f, 1.6f, -0.02f), new Vector3(0.02f, 0.12f, 0.16f));
            b.Box(Felt(Color.white), new Vector3(-0.02f, 1.32f, 0.3f), new Vector3(0.22f, 0.15f, 0.01f), new Vector3(-20f, 0f, 5f));
            b.Box(Felt(new Color(0.85f, 0.3f, 0.35f)), new Vector3(-0.02f, 1.34f, 0.31f), new Vector3(0.05f, 0.05f, 0.005f), new Vector3(-20f, 0f, 5f));
            b.Done("Letterbox", true);
            TownKit.Solid(kit, yard.TransformPoint(at + Vector3.up * 0.7f), new Vector3(0.4f, 1.4f, 0.6f), lb.eulerAngles.y);
        }

        static void Signpost(LevelKit kit, Transform yard)
        {
            Vector3 at = new Vector3(-4.6f, 0f, 3.4f);
            var sg = kit.Group("Signpost", yard.TransformPoint(at));
            sg.rotation = yard.rotation;
            var b = new Bits(sg);
            var wood = PatchworkAssets.Planks(new Color(0.6f, 0.45f, 0.3f));
            b.Box(wood, new Vector3(0f, 1.1f, 0f), new Vector3(0.14f, 2.2f, 0.14f));
            b.Ball(Shine(new Color(0.95f, 0.75f, 0.35f), 0.85f), new Vector3(0f, 2.25f, 0f), Vector3.one * 0.14f, 2, 6);
            // HOME, pointing back at the Pod, and THAT WAY, pointing off into the level.
            Vector3 toPod = -at;
            float homeYaw = Mathf.Atan2(toPod.x, toPod.z) * Mathf.Rad2Deg;
            Arrow(kit, b, new Vector3(0f, 1.85f, 0f), homeYaw, Red, "HOME");
            Arrow(kit, b, new Vector3(0f, 1.45f, 0f), 20f, NavyInk, "THAT WAY");
            b.Done("Signpost", true);
            TownKit.Solid(kit, yard.TransformPoint(at + Vector3.up * 1.1f), new Vector3(0.2f, 2.2f, 0.2f), 0f);
        }

        // A pointed board, pointing along `yaw` (in the frame's own degrees), with its word
        // written on both faces.
        static void Arrow(LevelKit kit, Bits b, Vector3 at, float yaw, Color paint, string word)
        {
            var q = Quaternion.Euler(0f, yaw, 0f);
            Vector3 mid = at + q * new Vector3(0f, 0f, 0.45f);
            b.Box(Felt(paint), mid, new Vector3(0.05f, 0.28f, 0.9f), new Vector3(0f, yaw, 0f));
            b.Box(Felt(paint), at + q * new Vector3(0f, 0f, 0.94f), new Vector3(0.05f, 0.2f, 0.2f), new Vector3(45f, yaw, 0f));
            float px = Mathf.Min(0.022f, 0.7f / (word.Length * 6f));
            foreach (int s in new[] { -1, 1 })
            {
                Vector3 side = q * new Vector3(s, 0f, 0f);
                var face = kit.Group("SignWord", b.Root.TransformPoint(mid + side * 0.03f));
                face.rotation = b.Root.rotation * Quaternion.LookRotation(side, Vector3.up);
                face.SetParent(b.Root, true);
                var text = new MeshBatch();
                Level2Builder.Letters(text, word, Vector3.zero, Vector3.forward, px, 0.01f);
                text.Build(face, "SignWord", Felt(Cream));
            }
        }

        static void WashingLine(LevelKit kit, Transform g, Transform yard)
        {
            // A washing line from a pole in the yard to a hook on the Pod, with socks, a little
            // jumper and a hanky pegged on it, all swaying.
            Vector3 pole = new Vector3(-4.2f, 0f, 0.6f);
            Vector3 hn;
            Vector3 hookLocal = Surface(-62f, 3.2f, out hn) + hn * 0.12f;
            Vector3 hook = yard.InverseTransformPoint(g.TransformPoint(hookLocal));
            var b = new Bits(yard);
            var wood = PatchworkAssets.Planks(new Color(0.6f, 0.45f, 0.3f));
            Vector3 poleTop = pole + Vector3.up * 2.6f;
            b.Box(wood, pole + Vector3.up * 1.3f, new Vector3(0.12f, 2.6f, 0.12f));
            b.Box(wood, poleTop, new Vector3(0.5f, 0.08f, 0.08f));
            var rope = PatchworkAssets.Yarn(Cream);
            const int segs = 12;
            var pts = new Vector3[segs + 1];
            for (int i = 0; i <= segs; i++)
            {
                float t = i / (float)segs;
                pts[i] = Vector3.Lerp(poleTop, hook, t) + Vector3.down * Mathf.Sin(t * Mathf.PI) * 0.45f;
                if (i > 0) b.Bar(rope, pts[i - 1], pts[i], 0.025f);
            }
            var ghb = new Bits(g);
            ghb.Ball(Shine(new Color(0.15f, 0.14f, 0.16f), 0.5f), hookLocal, Vector3.one * 0.08f, 2, 5);
            ghb.Done("LineHook", true);
            b.Done("WashingLine", true);
            TownKit.Solid(kit, yard.TransformPoint(pole + Vector3.up * 1.3f), new Vector3(0.2f, 2.6f, 0.2f), 0f);

            // The washing.
            Color[] socks = { new Color(0.9f, 0.3f, 0.35f), new Color(0.35f, 0.55f, 0.9f) };
            int[] at = { 3, 5, 7, 9 };
            for (int k = 0; k < at.Length; k++)
            {
                Vector3 p = pts[at[k]];
                var item = kit.Group("Washing", yard.TransformPoint(p));
                Vector3 along = (pts[at[k] + 1] - pts[at[k] - 1]).normalized;
                item.rotation = yard.rotation * Quaternion.LookRotation(Vector3.Cross(along, Vector3.up), Vector3.up);
                var ib = new Bits(item);
                var peg = PatchworkAssets.Planks(new Color(0.85f, 0.75f, 0.55f));
                if (k == 0 || k == 3)
                {
                    var sock = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, socks[k == 0 ? 0 : 1], Cream);
                    ib.Box(sock, new Vector3(0f, -0.22f, 0f), new Vector3(0.12f, 0.4f, 0.04f));
                    ib.Box(sock, new Vector3(0.07f, -0.43f, 0f), new Vector3(0.22f, 0.12f, 0.045f));
                    ib.Box(Felt(Cream), new Vector3(0f, -0.04f, 0f), new Vector3(0.13f, 0.06f, 0.05f));
                }
                else if (k == 1)
                {
                    // A little jumper with a patch on it.
                    var knit = Felt(new Color(0.45f, 0.75f, 0.5f));
                    ib.Box(knit, new Vector3(0f, -0.25f, 0f), new Vector3(0.4f, 0.42f, 0.04f));
                    foreach (int s in new[] { -1, 1 }) ib.Box(knit, new Vector3(s * 0.28f, -0.17f, 0f), new Vector3(0.18f, 0.12f, 0.04f), new Vector3(0f, 0f, s * -30f));
                    ib.Box(Felt(Gold), new Vector3(0.08f, -0.3f, 0.025f), new Vector3(0.12f, 0.12f, 0.01f), new Vector3(0f, 0f, 10f));
                    ib.Box(Felt(new Color(0.35f, 0.6f, 0.42f)), new Vector3(0f, -0.43f, 0f), new Vector3(0.42f, 0.05f, 0.045f));
                }
                else
                {
                    var hanky = PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, Cream, Red);
                    ib.Box(hanky, new Vector3(0f, -0.17f, 0f), new Vector3(0.32f, 0.32f, 0.01f), new Vector3(0f, 0f, 45f));
                }
                ib.Box(peg, new Vector3(-0.04f, 0f, 0f), new Vector3(0.025f, 0.1f, 0.03f));
                ib.Box(peg, new Vector3(0.04f, 0f, 0f), new Vector3(0.025f, 0.1f, 0.03f));
                ib.Done("Washing", true);
                var sway = item.gameObject.AddComponent<RmSway>();
                sway.Amount = 7f;
                sway.Speed = 1.1f + k * 0.17f;
                sway.Phase = k * 1.3f;
            }
        }

        static void RepairKit(LevelKit kit, Transform yard, Material brass)
        {
            // By the buckled leg at the back: an open red toolbox, a spanner, a screwdriver, a
            // hammer, a spare spring, a can of oil and a dirty rag.
            Vector3 at = new Vector3(1.4f, 0f, -4.1f);
            var t = kit.Group("RepairKit", yard.TransformPoint(at));
            t.rotation = yard.rotation * Quaternion.Euler(0f, 25f, 0f);
            var b = new Bits(t);
            var red = Shine(new Color(0.8f, 0.2f, 0.2f), 0.6f);
            var steel = Shine(new Color(0.78f, 0.8f, 0.84f), 0.85f);
            b.Box(red, new Vector3(0f, 0.14f, 0f), new Vector3(0.7f, 0.28f, 0.34f));
            b.Box(red, new Vector3(0f, 0.32f, -0.2f), new Vector3(0.7f, 0.04f, 0.34f), new Vector3(-70f, 0f, 0f));
            b.Box(steel, new Vector3(0f, 0.31f, 0f), new Vector3(0.5f, 0.03f, 0.03f));
            foreach (int s in new[] { -1, 1 }) b.Box(steel, new Vector3(s * 0.25f, 0.34f, 0f), new Vector3(0.03f, 0.08f, 0.03f));
            for (int i = 0; i < 4; i++) b.Box(Shine(new Color(0.6f, 0.6f, 0.65f), 0.8f), new Vector3(-0.2f + i * 0.12f, 0.3f, 0.08f), new Vector3(0.03f, 0.08f, 0.03f));
            // Laid out on the dirt.
            b.Box(steel, new Vector3(0.6f, 0.02f, 0.2f), new Vector3(0.05f, 0.02f, 0.4f), new Vector3(0f, 30f, 0f));
            b.Ring(steel, new Vector3(0.7f, 0.02f, 0.37f), new Vector3(90f, 0f, 0f), 0.05f, 0.025f, 8, -60f, 240f);
            b.Box(Felt(new Color(0.95f, 0.8f, 0.2f)), new Vector3(-0.55f, 0.03f, 0.3f), new Vector3(0.05f, 0.05f, 0.18f), new Vector3(0f, -20f, 0f));
            b.Box(steel, new Vector3(-0.58f, 0.03f, 0.47f), new Vector3(0.015f, 0.015f, 0.18f), new Vector3(0f, -20f, 0f));
            b.Box(PatchworkAssets.Planks(new Color(0.6f, 0.45f, 0.3f)), new Vector3(0.2f, 0.03f, 0.55f), new Vector3(0.04f, 0.04f, 0.4f), new Vector3(0f, 70f, 0f));
            b.Box(steel, new Vector3(0.02f, 0.05f, 0.48f), new Vector3(0.16f, 0.07f, 0.07f), new Vector3(0f, 70f, 0f));
            for (int c = 0; c < 6; c++) b.Ring(brass, new Vector3(-0.3f + c * 0.04f, 0.08f, -0.45f), new Vector3(0f, 90f, 0f), 0.08f, 0.02f, 10);
            b.Cyl(brass, new Vector3(0.55f, 0f, -0.3f), 0.09f, 0.14f, 10);
            for (int i = 0; i < 3; i++) b.Cyl(brass, new Vector3(0.55f, 0.14f + i * 0.04f, -0.3f), 0.08f - i * 0.022f, 0.04f, 10);
            b.Bar(brass, new Vector3(0.55f, 0.25f, -0.3f), new Vector3(0.75f, 0.33f, -0.3f), 0.015f);
            b.Ball(Felt(new Color(0.6f, 0.55f, 0.5f)), new Vector3(-0.1f, 0.04f, 0.45f), new Vector3(0.3f, 0.07f, 0.22f), 2, 6);
            b.Ball(Felt(new Color(0.15f, 0.12f, 0.12f)), new Vector3(-0.05f, 0.075f, 0.45f), new Vector3(0.1f, 0.02f, 0.08f), 2, 5);
            b.Done("RepairKit", true);
        }

        static void Picnic(LevelKit kit, Transform yard)
        {
            // A picnic blanket on the crater floor round the side: a thermos, a mug, a tin of
            // biscuits with the lid off, and a book of maps left open.
            Vector3 at = new Vector3(-3.4f, 0f, -2.4f);
            var p = kit.Group("Picnic", yard.TransformPoint(at));
            p.rotation = yard.rotation * Quaternion.Euler(0f, -20f, 0f);
            var b = new Bits(p);
            b.Box(PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, Red, Cream), new Vector3(0f, 0.04f, 0f), new Vector3(1.6f, 0.02f, 1.3f));
            b.Ball(PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, Red, Cream), new Vector3(0.72f, 0.05f, 0.5f), new Vector3(0.3f, 0.06f, 0.25f), 2, 6);
            b.Cyl(Shine(new Color(0.25f, 0.5f, 0.45f), 0.7f), new Vector3(-0.45f, 0.05f, 0.3f), 0.07f, 0.32f, 10);
            b.Cyl(Shine(new Color(0.8f, 0.8f, 0.82f), 0.9f), new Vector3(-0.45f, 0.37f, 0.3f), 0.075f, 0.06f, 10);
            b.Cyl(Felt(new Color(1f, 0.85f, 0.4f)), new Vector3(-0.2f, 0.05f, 0.35f), 0.06f, 0.12f, 10);
            b.Ring(Felt(new Color(1f, 0.85f, 0.4f)), new Vector3(-0.13f, 0.11f, 0.35f), Vector3.zero, 0.035f, 0.015f, 8, -90f, 90f);
            b.Cyl(Shine(new Color(0.3f, 0.5f, 0.85f), 0.7f), new Vector3(0.25f, 0.05f, -0.2f), 0.2f, 0.12f, 14);
            b.Cyl(Shine(new Color(0.3f, 0.5f, 0.85f), 0.7f), new Vector3(0.6f, 0.05f, -0.35f), 0.21f, 0.02f, 14);
            for (int i = 0; i < 6; i++) b.Cyl(Felt(new Color(0.85f, 0.65f, 0.4f)), new Vector3(0.25f + Mathf.Cos(i) * 0.1f, 0.17f, -0.2f + Mathf.Sin(i) * 0.1f), 0.045f, 0.015f, 8);
            b.Box(Felt(NavyInk), new Vector3(-0.3f, 0.06f, -0.3f), new Vector3(0.3f, 0.02f, 0.22f), new Vector3(0f, 0f, 6f));
            b.Box(Felt(NavyInk), new Vector3(-0.0f, 0.06f, -0.3f), new Vector3(0.3f, 0.02f, 0.22f), new Vector3(0f, 0f, -6f));
            b.Box(Felt(Cream), new Vector3(-0.15f, 0.075f, -0.3f), new Vector3(0.54f, 0.01f, 0.2f));
            for (int i = 0; i < 4; i++) b.Box(Felt(new Color(0.4f, 0.55f, 0.35f)), new Vector3(-0.3f + (i % 2) * 0.28f, 0.082f, -0.34f + (i / 2) * 0.08f), new Vector3(0.12f, 0.004f, 0.05f), new Vector3(0f, i * 25f, 0f));
            b.Done("Picnic", true);
        }

        static void Bucket(LevelKit kit, Transform yard)
        {
            // A bucket of soapy water and a sponge by the sooty side, for the scrubbing that
            // hasn't been done yet.
            Vector3 at = new Vector3(3.9f, 0f, -0.9f);
            var t = kit.Group("SootBucket", yard.TransformPoint(at));
            t.rotation = yard.rotation;
            var b = new Bits(t);
            var tin = Shine(new Color(0.65f, 0.68f, 0.72f), 0.8f);
            for (int i = 0; i < 4; i++) b.Ring(tin, new Vector3(0f, 0.05f + i * 0.1f, 0f), new Vector3(90f, 0f, 0f), 0.2f + i * 0.02f, 0.035f, 14);
            b.Cyl(tin, Vector3.zero, 0.2f, 0.03f, 12);
            b.Cyl(Shine(new Color(0.55f, 0.7f, 0.8f), 0.95f), new Vector3(0f, 0.02f, 0f), 0.24f, 0.3f, 14);
            for (int i = 0; i < 7; i++) b.Ball(Shine(new Color(0.95f, 0.97f, 1f), 0.9f), new Vector3(Mathf.Cos(i * 0.9f) * 0.12f, 0.34f, Mathf.Sin(i * 0.9f) * 0.12f), Vector3.one * (0.06f + (i % 3) * 0.02f), 2, 5);
            b.Ring(Shine(new Color(0.3f, 0.3f, 0.32f), 0.6f), new Vector3(0f, 0.42f, 0f), Vector3.zero, 0.23f, 0.015f, 10, 0f, 180f);
            b.Box(Felt(new Color(1f, 0.85f, 0.3f)), new Vector3(0.35f, 0.06f, 0.15f), new Vector3(0.22f, 0.12f, 0.14f), new Vector3(0f, 20f, 0f));
            b.Box(Felt(new Color(0.35f, 0.65f, 0.4f)), new Vector3(0.35f, 0.125f, 0.15f), new Vector3(0.22f, 0.02f, 0.14f), new Vector3(0f, 20f, 0f));
            b.Done("SootBucket", true);
            TownKit.Solid(kit, yard.TransformPoint(at + Vector3.up * 0.22f), new Vector3(0.5f, 0.44f, 0.5f), 0f);
        }

        static void Windsock(LevelKit kit, Transform yard)
        {
            // A windsock on a pole, as every proper landing place has: orange and white rings,
            // lifting and falling with the breeze.
            Vector3 at = new Vector3(5f, 0f, 0.8f);
            var post = kit.Group("Windsock", yard.TransformPoint(at));
            post.rotation = yard.rotation;
            var b = new Bits(post);
            var white = Shine(new Color(0.92f, 0.92f, 0.9f), 0.5f);
            b.Box(white, new Vector3(0f, 1.5f, 0f), new Vector3(0.08f, 3f, 0.08f));
            for (int i = 0; i < 3; i++) b.Box(Shine(new Color(0.85f, 0.25f, 0.2f), 0.5f), new Vector3(0f, 0.6f + i * 0.9f, 0f), new Vector3(0.09f, 0.2f, 0.09f));
            b.Ball(Shine(new Color(0.95f, 0.75f, 0.35f), 0.85f), new Vector3(0f, 3.05f, 0f), Vector3.one * 0.12f, 2, 6);
            b.Ring(Shine(new Color(0.6f, 0.6f, 0.65f), 0.8f), new Vector3(0f, 2.85f, 0.25f), new Vector3(0f, 0f, 0f), 0.2f, 0.03f, 12);
            b.Done("WindsockPole", true);
            var sock = kit.Group("Sock", post.TransformPoint(new Vector3(0f, 2.85f, 0.05f)));
            sock.SetParent(post, true);
            sock.localRotation = Quaternion.identity;
            var sb = new Bits(sock);
            for (int i = 0; i < 5; i++)
            {
                float r = 0.2f - i * 0.025f;
                var m = Felt(i % 2 == 0 ? new Color(1f, 0.5f, 0.15f) : new Color(0.98f, 0.96f, 0.92f));
                for (int k = 0; k < 10; k++)
                {
                    float a = k / 10f * Mathf.PI * 2f;
                    sb.Box(m, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0.2f + i * 0.24f), new Vector3(0.13f, 0.02f, 0.25f), new Vector3(0f, 0f, a * Mathf.Rad2Deg + 90f));
                }
            }
            sb.Done("Sock", true);
            post.gameObject.AddComponent<PodWindsock>().Sock = sock;
            TownKit.Solid(kit, yard.TransformPoint(at + Vector3.up * 1.5f), new Vector3(0.2f, 3f, 0.2f), 0f);
        }

        static void Supplies(LevelKit kit, Transform yard)
        {
            // Supplies unloaded for the stay: cardboard boxes, one marked BUTTONS and one YARN,
            // with a ball of wool rolled out of the top one, under a tarpaulin tied down with rope.
            Vector3 at = new Vector3(-1.6f, 0f, -4.9f);
            var t = kit.Group("Supplies", yard.TransformPoint(at));
            t.rotation = yard.rotation * Quaternion.Euler(0f, 15f, 0f);
            var b = new Bits(t);
            var box = PatchworkAssets.Cardboard(new Color(0.78f, 0.6f, 0.42f));
            var tape = Shine(new Color(0.9f, 0.85f, 0.6f), 0.4f);
            b.Box(box, new Vector3(0f, 0.3f, 0f), new Vector3(0.8f, 0.6f, 0.6f));
            b.Box(box, new Vector3(0.75f, 0.25f, 0.1f), new Vector3(0.6f, 0.5f, 0.5f), new Vector3(0f, 12f, 0f));
            b.Box(box, new Vector3(0.1f, 0.8f, 0.05f), new Vector3(0.55f, 0.4f, 0.45f), new Vector3(0f, -10f, 0f));
            b.Box(tape, new Vector3(0f, 0.605f, 0f), new Vector3(0.12f, 0.01f, 0.62f));
            b.Box(tape, new Vector3(0.75f, 0.505f, 0.1f), new Vector3(0.12f, 0.01f, 0.52f), new Vector3(0f, 12f, 0f));
            // The flaps of the top box open, and the wool coming out.
            b.Box(box, new Vector3(0.1f, 1.02f, 0.3f), new Vector3(0.5f, 0.02f, 0.2f), new Vector3(-50f, -10f, 0f));
            b.Box(box, new Vector3(0.1f, 1.02f, -0.2f), new Vector3(0.5f, 0.02f, 0.2f), new Vector3(50f, -10f, 0f));
            b.Ball(PatchworkAssets.Yarn(new Color(0.55f, 0.4f, 0.8f)), new Vector3(0.05f, 1.02f, 0.05f), Vector3.one * 0.26f, 3, 8);
            b.Ball(PatchworkAssets.Yarn(new Color(0.95f, 0.6f, 0.3f)), new Vector3(-0.55f, 0.12f, 0.4f), Vector3.one * 0.22f, 3, 8);
            b.Bar(PatchworkAssets.Yarn(new Color(0.95f, 0.6f, 0.3f)), new Vector3(-0.55f, 0.02f, 0.4f), new Vector3(-1.1f, 0.02f, 0.9f), 0.02f);
            var ink = Felt(new Color(0.25f, 0.2f, 0.18f));
            var label = kit.Group("SupplyLabels", t.TransformPoint(new Vector3(0f, 0.3f, 0.305f)));
            label.rotation = t.rotation;
            label.SetParent(t, true);
            var lt = new MeshBatch();
            Level2Builder.Letters(lt, "BUTTONS", Vector3.zero, Vector3.forward, 0.018f, 0.006f);
            lt.Build(label, "BoxLabel", ink);
            var label2 = kit.Group("SupplyLabels", t.TransformPoint(new Vector3(0.75f, 0.25f, 0.36f)));
            label2.rotation = t.rotation * Quaternion.Euler(0f, 12f, 0f);
            label2.SetParent(t, true);
            var lt2 = new MeshBatch();
            Level2Builder.Letters(lt2, "YARN", Vector3.zero, Vector3.forward, 0.02f, 0.006f);
            lt2.Build(label2, "BoxLabel", ink);
            // A folded tarpaulin over the big box, and a rope tied round.
            b.Box(Felt(new Color(0.3f, 0.45f, 0.4f)), new Vector3(0f, 0.62f, -0.1f), new Vector3(0.84f, 0.03f, 0.42f), new Vector3(0f, 0f, 2f));
            b.Box(Felt(new Color(0.3f, 0.45f, 0.4f)), new Vector3(0f, 0.45f, -0.31f), new Vector3(0.84f, 0.34f, 0.03f));
            b.Box(PatchworkAssets.Yarn(Cream), new Vector3(-0.2f, 0.3f, 0f), new Vector3(0.03f, 0.62f, 0.62f));
            b.Done("Supplies", true);
            TownKit.Solid(kit, yard.TransformPoint(at + new Vector3(0.35f, 0.4f, 0.05f)), new Vector3(1.5f, 0.8f, 0.7f), t.eulerAngles.y);
        }
    }

    // (See LandedPod.Windsock.)
    public class PodWindsock : MonoBehaviour
    {
        public Transform Sock;
        float phase;
        void Start() { phase = transform.position.x; }
        void Update()
        {
            if (Sock == null) return;
            float t = Time.time + phase;
            float gust = 0.5f + 0.5f * Mathf.PerlinNoise(t * 0.25f, 0.4f);
            Sock.localRotation = Quaternion.Euler(Mathf.Lerp(55f, 8f, gust) + Mathf.Sin(t * 3.1f) * 3f, Mathf.Sin(t * 0.7f) * 25f, Mathf.Sin(t * 2.3f) * 4f);
        }
    }

    // Fairy lights: each colour of bulb brightening and dimming on its own slow beat.
    public class PodTwinkle : MonoBehaviour
    {
        public Material[] Mats;
        public Color[] Cols;

        void Update()
        {
            if (Mats == null) return;
            for (int i = 0; i < Mats.Length; i++)
            {
                if (Mats[i] == null) continue;
                float k = 0.55f + 0.45f * Mathf.Sin(Time.time * (1.3f + i * 0.37f) + i * 1.7f);
                Mats[i].SetColor("_EmissionColor", Cols[i] * (0.6f + k * 1.8f));
            }
        }
    }
}

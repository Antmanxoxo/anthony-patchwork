using UnityEngine;

namespace Patchwork
{
    // Six more things in each of the middle rooms (see Level2Rooms for how they're fitted in):
    //   The Deepsea Tunnel  - sea pigs, Venus' flower baskets, a tripod fish, bamboo coral, an
    //                         oarfish, and the squashed cups
    //   The Sunken Ruins    - the archaeologists' table, a survey grid on the sea bed, a model
    //                         of Pavlopetri, cuttlefish, a fallen bronze bell, and sea grass
    //   The Hall of Arms    - the armourer's anvil, an archery butt and the Mary Rose's bows,
    //                         painted shields, a model trebuchet, a shirt of mail, a cat in a helmet
    //   The Orrery Bridge   - its winding key, MIND THE GAP, the oil can, lights on the stairs,
    //                         a window seat, and Kepler's ellipse
    //   The Star Corridor   - a cuckoo clock, a planisphere, a meteorite to touch, the phases of
    //                         the Moon, comet dust in aerogel, and a stargazer's deck chair
    //   The Observatory     - Foucault's pendulum, a cabinet of instruments, the library ladder,
    //                         a red torch, Voyager, and the moonflowers
    public static partial class Level2Builder
    {
        // ================================================================ The Deepsea Tunnel

        static void RoomsTunnel(LevelKit kit)
        {
            TnSeaPigs(kit);
            TnFlowerBaskets(kit);
            TnTripodFish(kit);
            TnBambooCoral(kit);
            TnOarfish(kit);
            TnCups(kit);
        }

        static void TnSeaPigs(LevelKit kit)
        {
            // Sea pigs: plump pink sea cucumbers that walk the deep sea floor in herds, on legs
            // that are really little tubes they pump full of water.
            Vector3 f;
            if (!RmFloor("the sea pigs", new Vector3(24f, DeepFloor, 44f), new Vector3(3.2f, 0.6f, 2.6f), 0f, 9f, out f, 1.2f)) return;
            var pink = RmFelt(new Color(0.95f, 0.62f, 0.68f));
            var pale = RmFelt(new Color(1f, 0.82f, 0.85f));
            for (int i = 0; i < 4; i++)
            {
                var g = kit.Group("SeaPig", f);
                var b = new RmBits(kit, g);
                float s = 0.8f + (i % 3) * 0.15f;
                b.Ball(pink, new Vector3(0f, 0.14f * s, 0f), new Vector3(0.24f, 0.2f, 0.5f) * s, 4, 8);
                b.Ball(pink, new Vector3(0f, 0.15f * s, 0.24f * s), new Vector3(0.18f, 0.16f, 0.16f) * s, 3, 7);
                for (int l = 0; l < 4; l++)
                    foreach (int side in new[] { -1, 1 })
                        b.Cyl(pale, new Vector3(side * 0.1f * s, 0f, (-0.18f + l * 0.12f) * s), 0.025f * s, 0.08f * s, 5);
                foreach (int side in new[] { -1, 1 })
                    for (int k = 0; k < 2; k++)
                    {
                        Vector3 root = new Vector3(side * 0.05f * s, 0.26f * s, (0.08f - k * 0.12f) * s);
                        b.Tube(pale, root, root + new Vector3(side * 0.03f, 0.16f, 0.04f) * s, 0.015f * s, 5);
                        b.Ball(pale, root + new Vector3(side * 0.03f, 0.16f, 0.04f) * s, Vector3.one * 0.04f * s, 2, 5);
                    }
                for (int k = 0; k < 6; k++)
                    b.Ball(pale, new Vector3(Mathf.Cos(k * 1.05f) * 0.07f * s, 0.12f * s, (0.32f + Mathf.Sin(k * 1.05f) * 0.02f) * s), Vector3.one * 0.035f * s, 2, 4);
                b.Done("SeaPig");
                var walk = g.gameObject.AddComponent<FishSwim>();
                walk.Center = f + new Vector3(0f, 0.02f, 0f);
                walk.RadiusX = 1.1f;
                walk.RadiusZ = 0.8f;
                walk.Speed = 7f + i;
                walk.Angle = i * 90f;
                walk.Bob = 0.02f;
                walk.BobSpeed = 3f;
                walk.Wiggle = 5f;
                walk.WiggleSpeed = 2f;
                walk.Bank = 0f;
            }
        }

        static void TnFlowerBaskets(LevelKit kit)
        {
            Vector3 f;
            if (RmFloor("the Venus' flower baskets", new Vector3(-18f, DeepFloor, 40f), new Vector3(2.4f, 2.6f, 2.4f), 0f, 9f, out f, 1.2f))
            {
                var g = kit.Group("FlowerBaskets", f);
                var glass = new MeshBatch();
                Vector3[] at = { new Vector3(0f, 0f, 0f), new Vector3(0.6f, 0f, 0.35f), new Vector3(-0.45f, 0f, 0.5f) };
                float[] tall = { 2.2f, 1.5f, 1.1f };
                for (int k = 0; k < at.Length; k++)
                {
                    float h = tall[k];
                    System.Func<float, float> R = y => 0.1f + Mathf.Sin(Mathf.Clamp01(y / h) * Mathf.PI * 0.85f) * 0.16f;
                    for (int i = 0; i <= 8; i++)
                    {
                        float y = i / 8f * h;
                        for (int s = 0; s < 12; s++)
                        {
                            float a0 = s / 12f * Mathf.PI * 2f, a1 = (s + 1) / 12f * Mathf.PI * 2f;
                            RmTube(glass, at[k] + new Vector3(Mathf.Cos(a0) * R(y), y, Mathf.Sin(a0) * R(y)), at[k] + new Vector3(Mathf.Cos(a1) * R(y), y, Mathf.Sin(a1) * R(y)), 0.008f, 4);
                        }
                    }
                    for (int s = 0; s < 12; s++)
                    {
                        float a = s / 12f * Mathf.PI * 2f;
                        for (int i = 0; i < 8; i++)
                        {
                            float y0 = i / 8f * h, y1 = (i + 1) / 8f * h;
                            RmTube(glass, at[k] + new Vector3(Mathf.Cos(a) * R(y0), y0, Mathf.Sin(a) * R(y0)), at[k] + new Vector3(Mathf.Cos(a) * R(y1), y1, Mathf.Sin(a) * R(y1)), 0.008f, 4);
                            // The diagonal braces that make it so strong.
                            float b2 = a + Mathf.PI / 12f;
                            RmTube(glass, at[k] + new Vector3(Mathf.Cos(a) * R(y0), y0, Mathf.Sin(a) * R(y0)), at[k] + new Vector3(Mathf.Cos(b2) * R(y1), y1, Mathf.Sin(b2) * R(y1)), 0.005f, 4);
                        }
                    }
                    // The sieve plate on top, and the tuft of glass threads at the root.
                    for (int s = 0; s < 6; s++) glass.Box(at[k] + Vector3.up * h, new Vector3(R(h) * 2f, 0.01f, 0.01f), Quaternion.Euler(0f, s * 30f, 0f));
                    for (int s = 0; s < 10; s++) RmTube(glass, at[k], at[k] + new Vector3(Mathf.Cos(s * 0.63f) * 0.3f, -0.05f, Mathf.Sin(s * 0.63f) * 0.3f), 0.006f, 4);
                }
                Twinkling(glass, g, "FlowerBaskets", new Color(0.75f, 0.9f, 1f), 0.5f, 1.3f, 0.5f, 0f);
                // The pair of shrimp who live inside the tallest one.
                var b = new RmBits(kit, g);
                foreach (int s in new[] { -1, 1 })
                {
                    Vector3 p = new Vector3(s * 0.05f, 1.1f + s * 0.15f, 0f);
                    b.Ball(RmGlow(new Color(1f, 0.6f, 0.5f), 0.6f), p, new Vector3(0.05f, 0.05f, 0.12f), 3, 6);
                    b.Box(RmGlow(new Color(1f, 0.6f, 0.5f), 0.6f), p + new Vector3(0f, 0.03f, 0.08f), new Vector3(0.005f, 0.005f, 0.1f), new Vector3(-20f, s * 15f, 0f));
                }
                b.Done("BasketShrimp");
            }
            RmPlaque(kit, new Vector3(-22f, 0f, TunnelZ - 3.1f), 0f, 4f, "The Venus' Flower Basket",
                "A deep-sea sponge that builds itself a lattice out of glass, finer than lace and stronger than it looks, standing up on the sea floor on a tuft of glass threads.",
                "A pair of tiny shrimp swim in when they're young, grow too big to get out again, and spend their whole lives inside it together. In Japan, one has been given as a wedding present.");
        }

        static void TnTripodFish(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the tripod fish", new Vector3(31f, DeepFloor, 41f), new Vector3(1.6f, 1.4f, 1.4f), 300f, 9f, out f, 1.2f)) return;
            var g = RmGroup(kit, "TripodFish", f, 300f);
            var b = new RmBits(kit, g);
            var body = RmFelt(new Color(0.55f, 0.55f, 0.6f));
            var ray = RmFelt(new Color(0.75f, 0.75f, 0.78f));
            Vector3 c = new Vector3(0f, 0.95f, 0f);
            // Standing tall on its three stilts, facing into the current, waiting for supper to drift by.
            b.Ball(body, c, new Vector3(0.16f, 0.16f, 0.6f), 4, 8);
            b.Ball(body, c + new Vector3(0f, 0.01f, 0.28f), new Vector3(0.14f, 0.14f, 0.18f), 3, 7);
            b.Box(body, c + new Vector3(0f, 0.03f, -0.4f), new Vector3(0.03f, 0.16f, 0.22f));
            b.Tube(ray, c + new Vector3(0.05f, -0.06f, 0.12f), new Vector3(0.35f, 0f, 0.4f), 0.012f, 4);
            b.Tube(ray, c + new Vector3(-0.05f, -0.06f, 0.12f), new Vector3(-0.35f, 0f, 0.4f), 0.012f, 4);
            b.Tube(ray, c + new Vector3(0f, -0.04f, -0.42f), new Vector3(0f, 0f, -0.75f), 0.012f, 4);
            // The long feelers held up and forward, like arms waiting for a hug.
            foreach (int s in new[] { -1, 1 })
            {
                b.Tube(ray, c + new Vector3(s * 0.07f, 0.04f, 0.18f), c + new Vector3(s * 0.32f, 0.38f, 0.5f), 0.008f, 4);
                b.Tube(ray, c + new Vector3(s * 0.32f, 0.38f, 0.5f), c + new Vector3(s * 0.38f, 0.42f, 0.75f), 0.006f, 4);
            }
            RmEyes(b, c + new Vector3(0f, 0.05f, 0.32f), 0.09f, 0.02f);
            b.Done("TripodFish");
        }

        static void TnBambooCoral(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the bamboo coral", new Vector3(-21f, DeepFloor, 89f), new Vector3(2.8f, 3.4f, 2.8f), 0f, 9f, out f, 1.2f)) return;
            var g = kit.Group("BambooCoral", f);
            var white = new MeshBatch();
            var dark = new MeshBatch();
            var tips = new MeshBatch();
            var rng = new System.Random(4401);
            System.Action<Vector3, Vector3, int> grow = null;
            grow = (from, dir, depth) =>
            {
                float len = depth == 0 ? 0.9f : 0.75f - depth * 0.12f;
                int joints = 3;
                Vector3 p = from;
                for (int j = 0; j < joints; j++)
                {
                    Vector3 q = p + dir * (len / joints);
                    RmTube(white, p, q, 0.05f - depth * 0.01f, 6);
                    dark.Ball(q, Vector3.one * (0.12f - depth * 0.02f), 3, 6);
                    p = q;
                    dir = (dir + new Vector3((float)rng.NextDouble() - 0.5f, 0.15f, (float)rng.NextDouble() - 0.5f) * 0.25f).normalized;
                }
                if (depth >= 3) { tips.Ball(p, Vector3.one * 0.07f, 2, 5); return; }
                int n = depth == 0 ? 3 : 2;
                for (int k = 0; k < n; k++)
                {
                    float a = (k / (float)n) * Mathf.PI * 2f + (float)rng.NextDouble();
                    grow(p, (Vector3.up * 1.2f + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a))).normalized, depth + 1);
                }
            };
            grow(Vector3.zero, Vector3.up, 0);
            white.Build(g, "BambooCoral", RmFelt(new Color(0.92f, 0.9f, 0.82f)), true);
            dark.Build(g, "BambooCoralJoints", RmFelt(new Color(0.3f, 0.22f, 0.18f)));
            Twinkling(tips, g, "BambooCoralPolyps", new Color(0.45f, 0.7f, 1f), 0.4f, 1.6f, 0.6f, 1f);
            var foot = new RmBits(kit, g);
            foot.Ball(RmFelt(new Color(0.3f, 0.3f, 0.34f)), Vector3.zero, new Vector3(0.6f, 0.3f, 0.5f), 3, 7);
            foot.Done("BambooCoralRock", true);
        }

        static void TnOarfish(LevelKit kit)
        {
            // An oarfish: the longest bony fish in the sea, a ribbon of silver with a red crest,
            // swimming slowly round high over the sea bed.
            Vector3 c = new Vector3(6f, 9.5f, 85f);
            var g = kit.Group("Oarfish", c);
            var silver = RmShine(new Color(0.82f, 0.86f, 0.92f), 0.9f);
            var crest = RmFelt(new Color(0.9f, 0.2f, 0.25f));
            var b = new RmBits(kit, g);
            const int n = 30;
            const float len = 7f;
            Vector3 prev = Vector3.zero;
            for (int i = 0; i <= n; i++)
            {
                float t = i / (float)n;
                float z = len * 0.5f - t * len;
                Vector3 p = new Vector3(Mathf.Sin(t * Mathf.PI * 2.4f) * 0.35f * t, Mathf.Sin(t * Mathf.PI * 1.3f) * 0.12f, z);
                float h = Mathf.Lerp(0.36f, 0.06f, t * t);
                if (i > 0)
                {
                    b.Box(silver, (p + prev) * 0.5f, new Vector3(0.06f, h, (p - prev).magnitude + 0.02f), Quaternion.LookRotation(p - prev).eulerAngles);
                    b.Box(crest, (p + prev) * 0.5f + Vector3.up * (h * 0.5f + 0.04f), new Vector3(0.02f, 0.08f, (p - prev).magnitude), Quaternion.LookRotation(p - prev).eulerAngles);
                }
                prev = p;
            }
            // The crown of long red rays on its head, and its eyes.
            for (int k = 0; k < 6; k++)
                b.Tube(crest, new Vector3(0f, 0.18f, len * 0.5f - 0.1f), new Vector3((k - 2.5f) * 0.06f, 0.6f + (k % 2) * 0.15f, len * 0.5f - 0.45f - k * 0.08f), 0.015f, 4);
            foreach (int s in new[] { -1, 1 }) b.Tube(crest, new Vector3(s * 0.03f, -0.1f, len * 0.5f - 0.3f), new Vector3(s * 0.08f, -0.9f, len * 0.5f - 0.9f), 0.012f, 4);
            RmEyes(b, new Vector3(0f, 0.05f, len * 0.5f - 0.08f), 0.07f, 0.04f);
            b.Done("Oarfish", true);
            var swim = g.gameObject.AddComponent<FishSwim>();
            swim.Center = c;
            swim.RadiusX = 13f;
            swim.RadiusZ = 6f;
            swim.Speed = -4f;
            swim.Angle = 120f;
            swim.Bob = 0.7f;
            swim.BobSpeed = 0.25f;
            swim.Wiggle = 3f;
            swim.WiggleSpeed = 0.8f;
            swim.Bank = 3f;
        }

        static void TnCups(LevelKit kit)
        {
            Transform g;
            if (!RmExhibit(kit, "the squashed cups", new Vector3(34f, 0f, TunnelZ + 3f), 180f, 5f, new Vector3(0.9f, 1.5f, 0.7f), out g, "Squashed Cups",
                "These two foam cups were the same size once. One stayed up here. The other went down to the deep sea floor, tied to a scientist's instruments.",
                "Down there, the weight of all the water overhead presses in from every side at once, hard enough to squeeze the air out of every bubble in the foam. So it came back up small: just the same shape, only tiny.")) return;
            Vector3 inside = RmCase(kit, g, new Vector3(0.9f, 1.5f, 0.7f), false);
            var b = new RmBits(kit, g);
            var foam = RmFelt(new Color(0.97f, 0.97f, 0.94f));
            System.Action<Vector3, float> cup = (at, s) =>
            {
                for (int i = 0; i < 6; i++)
                    b.Ring(foam, at + Vector3.up * (0.02f + i * 0.05f) * s, new Vector3(90f, 0f, 0f), (0.07f + i * 0.012f) * s, 0.025f * s, 14);
                b.Cyl(foam, at, 0.07f * s, 0.01f, 12);
                // Somebody drew a fish on each one before it went down.
                b.Box(RmFelt(new Color(0.2f, 0.45f, 0.85f)), at + new Vector3(0f, 0.16f * s, 0.105f * s), new Vector3(0.08f * s, 0.035f * s, 0.005f), new Vector3(0f, 0f, 0f));
                b.Box(RmFelt(new Color(0.2f, 0.45f, 0.85f)), at + new Vector3(-0.05f * s, 0.16f * s, 0.1f * s), new Vector3(0.03f * s, 0.04f * s, 0.005f), new Vector3(0f, 0f, 45f));
            };
            cup(inside + new Vector3(-0.17f, 0f, 0f), 1f);
            cup(inside + new Vector3(0.2f, 0f, 0.05f), 0.38f);
            b.Done("SquashedCups", true);
        }

        // ================================================================ The Sunken Ruins

        static void RoomsRuins(LevelKit kit)
        {
            RuDigTable(kit);
            RuSurveyGrid(kit);
            RuPavlopetri(kit);
            RuCuttlefish(kit, new Vector3(60f, -2.8f, 50f), 2.4f, 2.8f, 0f);
            RuCuttlefish(kit, new Vector3(88f, -3f, 60f), 2.4f, 1.8f, 180f);
            RuFallenBell(kit);
            RuSeaGrass(kit);
        }

        static void RuDigTable(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the archaeologists' table", new Vector3(83f, 0f, 19f), new Vector3(3.4f, 2.1f, 2.4f), 180f, 7f, out f)) return;
            var g = RmGroup(kit, "DigTable", f, 180f);
            var b = new RmBits(kit, g);
            var wood = PatchworkAssets.Planks(new Color(0.6f, 0.45f, 0.3f));
            // A trestle table, its top covered in drawings.
            b.Box(wood, new Vector3(0f, 0.82f, 0f), new Vector3(2.2f, 0.06f, 1f));
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(wood, new Vector3(s * 0.85f, 0.4f, 0.3f), new Vector3(0.06f, 0.84f, 0.06f), new Vector3(-12f, 0f, 0f));
                b.Box(wood, new Vector3(s * 0.85f, 0.4f, -0.3f), new Vector3(0.06f, 0.84f, 0.06f), new Vector3(12f, 0f, 0f));
                b.Box(wood, new Vector3(s * 0.85f, 0.75f, 0f), new Vector3(0.06f, 0.06f, 0.8f));
            }
            var paper = RmFelt(new Color(0.95f, 0.92f, 0.82f));
            var ink = RmFelt(new Color(0.25f, 0.3f, 0.5f));
            b.Box(paper, new Vector3(-0.3f, 0.86f, 0f), new Vector3(1f, 0.01f, 0.75f), new Vector3(0f, 4f, 0f));
            // The plan of the temple, drawn out square by square.
            for (int i = 0; i <= 6; i++)
            {
                b.Box(ink, new Vector3(-0.75f + i * 0.15f, 0.867f, 0f), new Vector3(0.006f, 0.004f, 0.62f), new Vector3(0f, 4f, 0f));
                b.Box(ink, new Vector3(-0.3f, 0.867f, -0.3f + i * 0.1f), new Vector3(0.9f, 0.004f, 0.006f), new Vector3(0f, 4f, 0f));
            }
            b.Ring(RmFelt(new Color(0.8f, 0.25f, 0.25f)), new Vector3(-0.25f, 0.869f, 0.05f), new Vector3(90f, 0f, 0f), 0.14f, 0.012f, 12);
            foreach (var p in new[] { new Vector3(-0.78f, 0.89f, 0.33f), new Vector3(0.18f, 0.89f, -0.33f) })
                b.Ball(RmFelt(new Color(0.55f, 0.55f, 0.58f)), p, new Vector3(0.1f, 0.06f, 0.08f), 3, 6);
            // A tray of finds: broken pots, a little lamp, a coin.
            b.Box(wood, new Vector3(0.65f, 0.88f, 0.05f), new Vector3(0.6f, 0.05f, 0.45f));
            var clay = RmFelt(new Color(0.78f, 0.48f, 0.32f));
            for (int i = 0; i < 6; i++)
                b.Box(clay, new Vector3(0.48f + (i % 3) * 0.17f, 0.92f, -0.08f + (i / 3) * 0.2f), new Vector3(0.12f, 0.02f, 0.08f), new Vector3(0f, i * 37f, 4f));
            b.Ball(clay, new Vector3(0.8f, 0.93f, 0.16f), new Vector3(0.12f, 0.05f, 0.08f), 3, 6);
            b.Cyl(RmShine(new Color(0.9f, 0.75f, 0.35f), 0.9f), new Vector3(0.5f, 0.91f, 0.18f), 0.03f, 0.008f, 10);
            // A lamp, a ranging pole leant on the end, a stool and a canvas bag.
            b.Cyl(IronMat, new Vector3(-0.85f, 0.85f, -0.3f), 0.07f, 0.04f, 8);
            b.Ball(RmGlow(new Color(1f, 0.8f, 0.45f), 2f), new Vector3(-0.85f, 0.98f, -0.3f), new Vector3(0.1f, 0.16f, 0.1f), 3, 6);
            b.Ring(IronMat, new Vector3(-0.85f, 1.08f, -0.3f), Vector3.zero, 0.06f, 0.01f, 8, 0f, 180f);
            for (int i = 0; i < 4; i++)
                b.Box(RmFelt(i % 2 == 0 ? new Color(0.85f, 0.2f, 0.2f) : Color.white), new Vector3(1.2f, 0.25f + i * 0.5f, 0.35f + i * 0.04f), new Vector3(0.05f, 0.5f, 0.05f), new Vector3(-5f, 0f, 0f));
            b.Cyl(RmFelt(new Color(0.35f, 0.4f, 0.3f)), new Vector3(0f, 0f, -0.85f), 0.2f, 0.45f, 10);
            b.Ball(RmFelt(new Color(0.75f, 0.68f, 0.5f)), new Vector3(-1.05f, 0.18f, 0.55f), new Vector3(0.4f, 0.36f, 0.3f), 3, 7);
            b.Done("DigTable", true);
            PointLight(kit, g.TransformPoint(new Vector3(-0.85f, 1.2f, -0.3f)), new Color(1f, 0.8f, 0.5f), 5f, 0.9f);
            RmSolid(kit, g, new Vector3(0f, 0.45f, 0f), new Vector3(2.2f, 0.9f, 1f));
            RmSolid(kit, g, new Vector3(0f, 0.22f, -0.85f), new Vector3(0.4f, 0.45f, 0.4f));
        }

        static void RuSurveyGrid(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the survey grid", new Vector3(66f, RuinBed, 34f), new Vector3(4.6f, 0.6f, 4.6f), 12f, 8f, out f, 1f)) return;
            var g = RmGroup(kit, "SurveyGrid", f, 12f);
            var b = new RmBits(kit, g);
            var yellow = RmShine(new Color(0.95f, 0.8f, 0.2f), 0.6f);
            var line = RmFelt(new Color(1f, 0.95f, 0.85f));
            const float h = 0.4f, half = 2f;
            // A square frame of yellow poles on little legs, strung into one-metre squares.
            foreach (int s in new[] { -1, 1 })
            {
                b.Tube(yellow, new Vector3(-half, h, s * half), new Vector3(half, h, s * half), 0.04f, 6);
                b.Tube(yellow, new Vector3(s * half, h, -half), new Vector3(s * half, h, half), 0.04f, 6);
                foreach (int t in new[] { -1, 1 }) b.Tube(yellow, new Vector3(s * half, 0f, t * half), new Vector3(s * half, h, t * half), 0.035f, 6);
            }
            for (int i = 1; i < 4; i++)
            {
                float x = -half + i;
                b.Box(line, new Vector3(x, h, 0f), new Vector3(0.012f, 0.012f, half * 2f));
                b.Box(line, new Vector3(0f, h, x), new Vector3(half * 2f, 0.012f, 0.012f));
            }
            // Little tags on the corners of the squares, and pots half-cleared in two of them.
            for (int i = 0; i < 4; i++)
                for (int k = 0; k < 4; k++)
                    if ((i + k) % 3 == 0)
                        b.Box(RmFelt(new Color(0.9f, 0.3f, 0.3f)), new Vector3(-half + i + 0.5f, 0.03f, -half + k + 0.5f), new Vector3(0.1f, 0.03f, 0.07f));
            var clay = RmFelt(new Color(0.7f, 0.45f, 0.3f));
            b.Ball(clay, new Vector3(-0.5f, 0.15f, 0.5f), new Vector3(0.36f, 0.5f, 0.36f), 4, 8);
            b.Ball(clay, new Vector3(0.7f, 0.08f, -0.6f), new Vector3(0.6f, 0.22f, 0.3f), 3, 7);
            b.Ball(clay, new Vector3(0.95f, 0.06f, -0.45f), new Vector3(0.2f, 0.1f, 0.16f), 3, 6);
            // The airlift: a hose that sucks the sand away, rising up to a float at the surface.
            var hose = RmShine(new Color(0.2f, 0.25f, 0.3f), 0.4f);
            Vector3 mouth = new Vector3(1.4f, 0.1f, 1.3f);
            b.Tube(hose, mouth, mouth + new Vector3(0.1f, 1.4f, 0.1f), 0.07f, 8);
            b.Tube(hose, mouth + new Vector3(0.1f, 1.4f, 0.1f), new Vector3(1.8f, RuinSurface - f.y - 0.2f, 1.8f), 0.07f, 8);
            b.Done("SurveyGrid", true);
            // The marker buoy over the dig, bobbing, its rope going down to the frame.
            var buoy = kit.Group("DigBuoy", g.TransformPoint(new Vector3(1.8f, RuinSurface - f.y, 1.8f)));
            buoy.SetParent(g, true);
            var bb = new RmBits(kit, buoy);
            bb.Ball(RmFelt(new Color(1f, 0.5f, 0.15f)), Vector3.zero, new Vector3(0.6f, 0.5f, 0.6f), 4, 10);
            bb.Box(RmFelt(Color.white), Vector3.zero, new Vector3(0.61f, 0.1f, 0.61f));
            bb.Box(DarkWood, Vector3.up * 0.6f, new Vector3(0.04f, 0.9f, 0.04f));
            bb.Box(RmFelt(new Color(0.85f, 0.15f, 0.15f)), new Vector3(0.18f, 0.9f, 0f), new Vector3(0.36f, 0.24f, 0.01f));
            bb.Box(RmFelt(Color.white), new Vector3(0.18f, 0.9f, 0f), new Vector3(0.36f, 0.04f, 0.012f), new Vector3(0f, 0f, 33f));
            bb.Done("DigBuoy", true);
            var bob = buoy.gameObject.AddComponent<SeaBob>();
            bob.Height = 0.08f;
            bob.Rock = 5f;
        }

        static void RuPavlopetri(LevelKit kit)
        {
            Transform g;
            if (!RmExhibit(kit, "the model of Pavlopetri", new Vector3(56f, 0f, 19.5f), 180f, 7f, new Vector3(1.6f, 1.3f, 1.1f), out g, "Pavlopetri",
                "Off the south coast of Greece, under three or four metres of clear water, there's a whole town: streets, houses, courtyards and tombs, about five thousand years old.",
                "It's thought to be the oldest drowned town ever found. A diver spotted it in 1967, and since then it has been mapped stone by stone, the way this little model shows it.")) return;
            var b = new RmBits(kit, g);
            b.Box(DarkWood, new Vector3(0f, 0.45f, 0f), new Vector3(1.6f, 0.9f, 1.1f));
            b.Box(RmFelt(new Color(0.82f, 0.74f, 0.55f)), new Vector3(0f, 0.92f, 0f), new Vector3(1.5f, 0.04f, 1f));
            var wall = RmFelt(new Color(0.6f, 0.58f, 0.52f));
            var rng = new System.Random(1967);
            // Streets crossing at angles, and the low walls of houses either side of them.
            for (int i = 0; i < 26; i++)
            {
                float x = -0.65f + (float)rng.NextDouble() * 1.3f, z = -0.42f + (float)rng.NextDouble() * 0.84f;
                if (Mathf.Abs(z - x * 0.3f) < 0.07f || Mathf.Abs(x + 0.15f) < 0.05f) continue;
                float w = 0.08f + (float)rng.NextDouble() * 0.12f, d = 0.08f + (float)rng.NextDouble() * 0.1f;
                foreach (int s in new[] { -1, 1 })
                {
                    b.Box(wall, new Vector3(x + s * w * 0.5f, 0.955f, z), new Vector3(0.012f, 0.03f, d), new Vector3(0f, 16f, 0f));
                    b.Box(wall, new Vector3(x, 0.955f, z + s * d * 0.5f), new Vector3(w, 0.03f, 0.012f), new Vector3(0f, 16f, 0f));
                }
            }
            // The water over it all: a sheet of blue glass on brass posts.
            foreach (int s in new[] { -1, 1 })
                foreach (int t in new[] { -1, 1 })
                    b.Box(Brass, new Vector3(s * 0.72f, 1.07f, t * 0.47f), new Vector3(0.03f, 0.3f, 0.03f));
            b.Done("Pavlopetri", true);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.2f, 0f), new Vector3(1.48f, 0.02f, 0.98f), PatchworkAssets.Bubble(new Color(0.4f, 0.75f, 0.9f, 0.3f)), default(Vector3), g));
            RmSolid(kit, g, new Vector3(0f, 0.6f, 0f), new Vector3(1.6f, 1.2f, 1.1f));
        }

        static void RuCuttlefish(LevelKit kit, Vector3 c, float rx, float rz, float start)
        {
            // A cuttlefish, rippling the frill round its edge and rolling colours over its skin.
            var g = kit.Group("Cuttlefish", c);
            var b = new RmBits(kit, g);
            var skin = PatchworkAssets.Unique(RmFelt(new Color(0.75f, 0.55f, 0.4f)));
            b.Ball(skin, Vector3.zero, new Vector3(0.36f, 0.22f, 0.7f), 4, 10);
            var frill = RmFelt(new Color(0.85f, 0.8f, 0.7f));
            for (int k = 0; k < 14; k++)
            {
                float t = k / 13f, z = 0.28f - t * 0.56f;
                float w = 0.18f * Mathf.Sqrt(Mathf.Max(0.05f, 1f - (z / 0.36f) * (z / 0.36f)));
                foreach (int s in new[] { -1, 1 })
                    b.Box(frill, new Vector3(s * (w + 0.04f), Mathf.Sin(k * 1.3f) * 0.015f, z), new Vector3(0.08f, 0.008f, 0.05f), new Vector3(0f, 0f, s * Mathf.Sin(k * 1.3f) * 20f));
            }
            for (int a = 0; a < 8; a++)
            {
                float x = (a % 4 - 1.5f) * 0.05f, y = (a < 4 ? 0.03f : -0.03f);
                b.Tube(skin, new Vector3(x, y, 0.32f), new Vector3(x * 1.6f, y * 1.5f - 0.02f, 0.5f), 0.022f, 5);
            }
            RmEyes(b, new Vector3(0f, 0.08f, 0.26f), 0.24f, 0.05f);
            b.Done("Cuttlefish");
            var shift = g.gameObject.AddComponent<ColourShift>();
            shift.Mat = skin;
            shift.Colours = new[] { new Color(0.75f, 0.55f, 0.4f), new Color(0.95f, 0.9f, 0.8f), new Color(0.45f, 0.3f, 0.25f), new Color(0.85f, 0.45f, 0.35f), new Color(0.6f, 0.65f, 0.5f) };
            shift.Offset = c.x;
            var swim = g.gameObject.AddComponent<FishSwim>();
            swim.Center = c;
            swim.RadiusX = rx;
            swim.RadiusZ = rz;
            swim.Speed = 9f;
            swim.Angle = start;
            swim.Bob = 0.2f;
            swim.BobSpeed = 0.5f;
            swim.Wiggle = 3f;
            swim.WiggleSpeed = 1.5f;
            swim.Bank = 4f;
        }

        static void RuFallenBell(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the fallen bell", new Vector3(86f, RuinBed, 88f), new Vector3(2.4f, 2f, 2.4f), 30f, 7f, out f, 1f)) return;
            var g = RmGroup(kit, "FallenBell", f, 30f);
            var b = new RmBits(kit, g);
            var bronze = RmShine(new Color(0.35f, 0.6f, 0.5f), 0.55f);
            var bright = RmShine(new Color(0.75f, 0.58f, 0.32f), 0.85f);
            // Lying on its side, sunk a little into the sand, mouth to the east.
            for (int i = 0; i < 9; i++)
            {
                float t = i / 8f, r = Mathf.Lerp(0.42f, 0.85f, t * t);
                b.Ring(i == 6 ? bright : bronze, new Vector3(-0.7f + t * 1.4f, 0.75f, 0f), new Vector3(0f, 90f, 0f), r, 0.18f, 22);
            }
            b.Ball(bronze, new Vector3(-0.72f, 0.75f, 0f), new Vector3(0.2f, 0.84f, 0.84f), 4, 10);
            b.Box(bronze, new Vector3(-0.95f, 0.75f, 0f), new Vector3(0.3f, 0.25f, 0.5f));
            b.Ring(bronze, new Vector3(-1.08f, 0.75f, 0f), new Vector3(90f, 0f, 0f), 0.12f, 0.06f, 10);
            b.Ball(IronMat, new Vector3(0.45f, 0.35f, 0.1f), Vector3.one * 0.26f, 3, 7);
            // Sand drifted into its mouth, weed on top, and a starfish who has moved in.
            b.Ball(RmFelt(new Color(0.75f, 0.68f, 0.5f)), new Vector3(0.35f, 0.05f, 0f), new Vector3(1.4f, 0.3f, 1.4f), 3, 8);
            for (int i = 0; i < 8; i++)
                b[RmFelt(new Color(0.3f, 0.55f, 0.35f))].Strand(new Vector3(-0.5f + i * 0.14f, 1.45f + Mathf.Sin(i) * 0.08f, (i % 3 - 1) * 0.12f), 0.3f + (i % 3) * 0.1f, 0.05f, new Vector3(0.05f, 0f, 0.02f), i * 40f);
            for (int k = 0; k < 5; k++)
                b.Box(RmFelt(new Color(1f, 0.55f, 0.3f)), new Vector3(0.6f, 0.26f, -0.4f), new Vector3(0.05f, 0.03f, 0.22f), new Vector3(0f, k * 72f, 0f));
            b.Done("FallenBell", true);
            var fish = Swim(kit, Swimmer.Fish, g.TransformPoint(new Vector3(0.7f, 0.8f, 0f)), 0.4f, 0.35f, 30f, 0.35f, new Color(0.95f, 0.85f, 0.3f), new Color(0.2f, 0.2f, 0.35f), 0f);
            fish.Bob = 0.1f;
        }

        static void RuSeaGrass(LevelKit kit)
        {
            Vector3 f;
            if (RmFloor("the sea grass meadow", new Vector3(79f, RuinBed, 31f), new Vector3(5f, 1.4f, 4f), 0f, 7f, out f, 1f))
            {
                var rng = new System.Random(311);
                var grass = RmFelt(new Color(0.3f, 0.55f, 0.3f));
                var young = RmFelt(new Color(0.45f, 0.7f, 0.35f));
                for (int clump = 0; clump < 8; clump++)
                {
                    Vector3 at = f + new Vector3(-2f + (clump % 4) * 1.3f + (float)rng.NextDouble() * 0.4f, 0f, -1.2f + (clump / 4) * 2f + (float)rng.NextDouble() * 0.4f);
                    var g = kit.Group("SeaGrass", at);
                    var b = new RmBits(kit, g);
                    for (int i = 0; i < 16; i++)
                    {
                        float a = (float)rng.NextDouble() * Mathf.PI * 2f, r = (float)rng.NextDouble() * 0.45f;
                        b[i % 4 == 0 ? young : grass].Blade(new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r), 0.7f + (float)rng.NextDouble() * 0.6f, 0.05f, new Vector3(0.06f, 0f, 0.03f), a * Mathf.Rad2Deg);
                    }
                    b.Done("SeaGrass");
                    var sway = g.gameObject.AddComponent<RmSway>();
                    sway.Amount = 7f;
                    sway.Speed = 0.7f;
                    sway.Phase = clump * 0.9f;
                }
                Shoal(kit, f + Vector3.up * 1.6f, 2.2f, 1.6f, 16f, 14, 0.22f, new Color(0.8f, 0.85f, 0.7f), 3110);
            }
            RmPlaque(kit, new Vector3(79f, 0f, 22.4f), 180f, 4f, "Sea Grass",
                "Not a seaweed at all: sea grass is a real flowering plant, with roots and tiny flowers, whose ancestors left the land and went back into the sea.",
                "Its meadows hold the sea bed together and hide baby fish. One meadow of it, off the island of Ibiza, is thought by some scientists to be a hundred thousand years old.");
        }

        // ================================================================ The Hall of Arms

        static void RoomsArms(LevelKit kit)
        {
            ArAnvil(kit);
            ArArchery(kit);
            ArShields(kit);
            ArTrebuchet(kit);
            ArMail(kit);
            ArCatInHelmet(kit);
        }

        static void ArAnvil(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the armourer's anvil", new Vector3(63.6f, 0f, -2.5f), new Vector3(2.4f, 1.6f, 1.6f), 90f, 7f, out f)) return;
            var g = RmGroup(kit, "ArmourersBench", f, 90f);
            var b = new RmBits(kit, g);
            var iron = RmShine(new Color(0.22f, 0.22f, 0.26f), 0.6f);
            // The anvil on its tree-stump block.
            b.Cyl(PatchworkAssets.Planks(new Color(0.45f, 0.32f, 0.22f)), new Vector3(-0.5f, 0f, 0f), 0.3f, 0.55f, 12);
            b.Ring(RmFelt(new Color(0.62f, 0.48f, 0.32f)), new Vector3(-0.5f, 0.555f, 0f), new Vector3(90f, 0f, 0f), 0.2f, 0.02f, 12);
            b.Box(iron, new Vector3(-0.5f, 0.62f, 0f), new Vector3(0.28f, 0.14f, 0.2f));
            b.Box(iron, new Vector3(-0.5f, 0.74f, 0f), new Vector3(0.5f, 0.12f, 0.18f));
            b.Box(iron, new Vector3(-0.15f, 0.76f, 0f), new Vector3(0.26f, 0.08f, 0.1f), new Vector3(0f, 0f, 8f));
            // The bench, with a breastplate laid on it being mended, the hammer, tongs and rivets.
            var wood = PatchworkAssets.Planks(new Color(0.42f, 0.3f, 0.22f));
            b.Box(wood, new Vector3(0.45f, 0.8f, 0f), new Vector3(1.1f, 0.08f, 0.7f));
            foreach (int s in new[] { -1, 1 }) foreach (int t in new[] { -1, 1 }) b.Box(wood, new Vector3(0.45f + s * 0.48f, 0.4f, t * 0.28f), new Vector3(0.07f, 0.8f, 0.07f));
            var steel = RmShine(new Color(0.78f, 0.8f, 0.84f), 0.85f);
            b.Ball(steel, new Vector3(0.4f, 0.88f, 0f), new Vector3(0.44f, 0.12f, 0.5f), 4, 10);
            b.Box(RmShine(new Color(0.6f, 0.45f, 0.25f), 0.5f), new Vector3(0.4f, 0.9f, -0.24f), new Vector3(0.3f, 0.02f, 0.04f));
            b.Box(DarkWood, new Vector3(-0.5f, 0.84f, 0.06f), new Vector3(0.04f, 0.04f, 0.34f), new Vector3(0f, 25f, 0f));
            b.Box(iron, new Vector3(-0.44f, 0.86f, 0.22f), new Vector3(0.12f, 0.06f, 0.06f), new Vector3(0f, 25f, 0f));
            foreach (int s in new[] { -1, 1 }) b.Box(iron, new Vector3(0.85f, 0.85f, 0.15f + s * 0.02f), new Vector3(0.36f, 0.015f, 0.015f), new Vector3(0f, s * 6f, 0f));
            b.Cyl(DarkWood, new Vector3(0.75f, 0.84f, -0.18f), 0.08f, 0.04f, 10);
            for (int i = 0; i < 7; i++) b.Ball(iron, new Vector3(0.75f + Mathf.Sin(i) * 0.04f, 0.885f, -0.18f + Mathf.Cos(i * 1.7f) * 0.04f), Vector3.one * 0.02f, 2, 4);
            // A bucket of water for cooling, and a leather apron hung on the bench's end.
            b.Cyl(PatchworkAssets.Planks(new Color(0.5f, 0.36f, 0.24f)), new Vector3(-0.5f, 0f, 0.55f), 0.16f, 0.32f, 10);
            b.Cyl(RmShine(new Color(0.2f, 0.35f, 0.45f), 0.95f), new Vector3(-0.5f, 0.05f, 0.55f), 0.14f, 0.24f, 10);
            b.Box(RmShine(new Color(0.42f, 0.25f, 0.15f), 0.3f), new Vector3(1.02f, 0.5f, 0f), new Vector3(0.02f, 0.6f, 0.4f));
            b.Done("ArmourersBench", true);
            RmSolid(kit, g, new Vector3(0f, 0.45f, 0f), new Vector3(2.1f, 0.9f, 0.8f));
        }

        static void ArArchery(LevelKit kit)
        {
            Vector3 f;
            if (RmFloor("the archery butt", new Vector3(97.4f, 0f, -26.5f), new Vector3(1.6f, 2.4f, 1.3f), 270f, 7f, out f))
            {
                var g = RmGroup(kit, "ArcheryButt", f, 270f);
                var b = new RmBits(kit, g);
                // A boss of plaited straw on a wooden easel, painted in rings.
                foreach (int s in new[] { -1, 1 })
                    b.Box(DarkWood, new Vector3(s * 0.55f, 0.85f, -0.15f), new Vector3(0.07f, 1.7f, 0.07f), new Vector3(-10f, 0f, s * -8f));
                b.Box(DarkWood, new Vector3(0f, 0.85f, -0.55f), new Vector3(0.07f, 1.75f, 0.07f), new Vector3(30f, 0f, 0f));
                Vector3 c = new Vector3(0f, 1.25f, -0.05f);
                var straw = PatchworkAssets.Wicker(new Color(0.85f, 0.72f, 0.42f));
                var bq = Quaternion.Euler(-10f, 0f, 0f);
                b.Prim(PrimitiveType.Cylinder, straw, c, new Vector3(1.1f, 0.1f, 1.1f), new Vector3(80f, 0f, 0f));
                Color[] rings = { Color.white, new Color(0.15f, 0.15f, 0.18f), new Color(0.3f, 0.5f, 0.9f), new Color(0.85f, 0.2f, 0.2f), new Color(1f, 0.8f, 0.2f) };
                for (int i = 0; i < rings.Length; i++)
                    b.Prim(PrimitiveType.Cylinder, RmFelt(rings[i]), c + bq * new Vector3(0f, 0f, 0.105f + i * 0.004f), new Vector3(1f - i * 0.2f, 0.004f, 1f - i * 0.2f), new Vector3(80f, 0f, 0f));
                // Three arrows in it, none of them quite in the gold.
                var shaft = PatchworkAssets.Planks(new Color(0.75f, 0.6f, 0.4f));
                Vector2[] hits = { new Vector2(0.12f, 0.08f), new Vector2(-0.22f, -0.1f), new Vector2(0.05f, -0.3f) };
                foreach (var hp in hits)
                {
                    Vector3 tip = c + bq * new Vector3(hp.x, hp.y, 0.05f);
                    Vector3 tail = tip + bq * new Vector3(hp.x * 0.3f, 0.06f, 0.75f);
                    b.Tube(shaft, tip, tail, 0.012f, 5);
                    for (int k = 0; k < 3; k++)
                        b.Box(RmFelt(new Color(0.95f, 0.95f, 0.95f)), Vector3.Lerp(tip, tail, 0.9f), new Vector3(0.005f, 0.05f, 0.12f), Quaternion.LookRotation(tail - tip).eulerAngles + new Vector3(0f, 0f, k * 120f));
                }
                // The longbow, leaning on the easel: as tall as a man.
                var yew = PatchworkAssets.Planks(new Color(0.7f, 0.5f, 0.3f));
                Vector3 bowFoot = new Vector3(0.75f, 0f, 0.1f), bowTop = new Vector3(0.6f, 1.85f, -0.05f);
                for (int k = 0; k < 8; k++)
                {
                    float t0 = k / 8f, t1 = (k + 1) / 8f;
                    Vector3 bend = new Vector3(0f, 0f, 0.12f);
                    b.Tube(yew, Vector3.Lerp(bowFoot, bowTop, t0) + bend * Mathf.Sin(t0 * Mathf.PI), Vector3.Lerp(bowFoot, bowTop, t1) + bend * Mathf.Sin(t1 * Mathf.PI), 0.02f, 5);
                }
                b.Box(RmFelt(CreamCol), Vector3.Lerp(bowFoot, bowTop, 0.5f), new Vector3(0.006f, 1.86f, 0.006f), new Vector3(-4.6f, 0f, -4.6f));
                b.Done("ArcheryButt", true);
                RmSolid(kit, g, new Vector3(0f, 0.9f, -0.2f), new Vector3(1.2f, 1.8f, 0.8f));
            }
            RmPlaque(kit, new Vector3(97.2f, 0f, -20f), 270f, 5f, "The Mary Rose's Longbows",
                "In 1545 the warship Mary Rose sank in front of the English fleet, with her crew and everything they had with them. In 1982 what was left of her was lifted out of the mud.",
                "Inside were 137 longbows of yew and thousands of arrows: the only longbows from those days that have survived anywhere. The archers' bones show it too: years of drawing bows that strong had changed the shape of their shoulders.");
        }

        static void ArShields(LevelKit kit)
        {
            // A painted shield high on each of the stone piers, a coat of arms nobody has ever had
            // before: a star, a wave, a fish, a key, a crescent, a lantern, a bell, a button.
            Color[,] fields =
            {
                { Navy, new Color(1f, 0.82f, 0.4f) }, { new Color(0.2f, 0.45f, 0.6f), CreamCol }, { Wine, new Color(1f, 0.82f, 0.4f) }, { new Color(0.25f, 0.45f, 0.3f), CreamCol },
                { Plum, new Color(0.95f, 0.9f, 0.75f) }, { new Color(0.6f, 0.2f, 0.18f), new Color(1f, 0.82f, 0.4f) }, { new Color(0.15f, 0.2f, 0.35f), new Color(0.85f, 0.85f, 0.9f) }, { new Color(0.7f, 0.55f, 0.2f), Navy }
            };
            int n = 0;
            foreach (float z in new[] { -36f, -27f, -18f, -9f })
                foreach (int side in new[] { -1, 1 })
                {
                    Vector3 facing = side < 0 ? Vector3.right : Vector3.left;
                    float x = side < 0 ? 61f : 99f;
                    Vector3 at;
                    int k = n++;
                    if (!RmWall("a painted shield", new Vector3(x, 8.6f, z), facing, new Vector3(1.1f, 1.5f, 0.2f), 0.4f, out at)) continue;
                    var g = RmGroup(kit, "Shield", at, Quaternion.LookRotation(facing));
                    var b = new RmBits(kit, g);
                    var field = RmFelt(fields[k, 0]);
                    var charge = RmFelt(fields[k, 1]);
                    var rim = Brass;
                    // The shape: square at the top, curving to a point at the bottom.
                    b.Box(field, new Vector3(0f, 0.25f, 0.06f), new Vector3(0.9f, 0.7f, 0.06f));
                    b.Box(field, new Vector3(0f, -0.25f, 0.06f), new Vector3(0.62f, 0.62f, 0.06f), new Vector3(0f, 0f, 45f));
                    b.Box(rim, new Vector3(0f, 0.6f, 0.07f), new Vector3(0.94f, 0.05f, 0.08f));
                    foreach (int s in new[] { -1, 1 })
                    {
                        b.Box(rim, new Vector3(s * 0.45f, 0.25f, 0.07f), new Vector3(0.05f, 0.72f, 0.08f));
                        b.Box(rim, new Vector3(s * 0.22f, -0.37f, 0.07f), new Vector3(0.05f, 0.64f, 0.08f), new Vector3(0f, 0f, s * 45f));
                    }
                    // Half of each shield in the second colour, then the charge in the middle.
                    b.Box(charge, new Vector3(0.22f, 0.25f, 0.095f), new Vector3(0.44f, 0.66f, 0.01f));
                    Vector3 m = new Vector3(0f, 0.12f, 0.11f);
                    var c2 = RmFelt(Color.Lerp(fields[k, 1], fields[k, 0], 0.15f));
                    var c1 = RmFelt(Color.Lerp(fields[k, 0], fields[k, 1], 0.85f));
                    switch (k)
                    {
                        case 0: for (int i = 0; i < 5; i++) b.Box(c1, m, new Vector3(0.06f, 0.42f, 0.01f), new Vector3(0f, 0f, i * 36f)); break;
                        case 1: for (int i = 0; i < 3; i++) b.Ring(c1, m + new Vector3(-0.2f + i * 0.2f, -0.05f, 0f), Vector3.zero, 0.1f, 0.04f, 8, 0f, 180f); break;
                        case 2: b.Ball(c1, m, new Vector3(0.34f, 0.16f, 0.02f), 3, 8); b.Box(c1, m + new Vector3(-0.22f, 0f, 0f), new Vector3(0.12f, 0.12f, 0.02f), new Vector3(0f, 0f, 45f)); break;
                        case 3: b.Ring(c1, m + Vector3.up * 0.12f, Vector3.zero, 0.08f, 0.04f, 10); b.Box(c1, m - Vector3.up * 0.06f, new Vector3(0.04f, 0.28f, 0.02f)); b.Box(c1, m - Vector3.up * 0.16f + Vector3.right * 0.05f, new Vector3(0.08f, 0.04f, 0.02f)); break;
                        case 4: b.Ring(c1, m, Vector3.zero, 0.14f, 0.07f, 12, 60f, 300f); break;
                        case 5: b.Box(c1, m, new Vector3(0.16f, 0.24f, 0.02f)); b.Box(c1, m + Vector3.up * 0.15f, new Vector3(0.22f, 0.04f, 0.02f)); b.Ring(c1, m + Vector3.up * 0.2f, Vector3.zero, 0.05f, 0.02f, 8, 0f, 180f); break;
                        case 6: b.Ball(c1, m + Vector3.up * 0.04f, new Vector3(0.26f, 0.3f, 0.02f), 3, 8); b.Box(c1, m - Vector3.up * 0.1f, new Vector3(0.34f, 0.05f, 0.02f)); break;
                        default: b.Disc(c1, m, 0.32f, 0.02f); for (int i = 0; i < 4; i++) b.Ball(c2, m + new Vector3((i % 2 - 0.5f) * 0.1f, (i / 2 - 0.5f) * 0.1f, 0.015f), Vector3.one * 0.05f, 2, 5); break;
                    }
                    // Hung on a strap from an iron peg.
                    b.Box(IronMat, new Vector3(0f, 0.72f, 0.04f), new Vector3(0.06f, 0.06f, 0.08f));
                    b.Box(RmShine(new Color(0.42f, 0.26f, 0.16f), 0.3f), new Vector3(0f, 0.66f, 0.06f), new Vector3(0.5f, 0.04f, 0.02f), new Vector3(0f, 0f, 0f));
                    b.Done("Shield", true);
                }
        }

        static void ArTrebuchet(LevelKit kit)
        {
            Transform g;
            if (!RmExhibit(kit, "the model trebuchet", new Vector3(96.6f, 0f, -8.6f), 270f, 7f, new Vector3(1.8f, 2.2f, 1.2f), out g, "The Trebuchet",
                "A siege engine: a long throwing arm on a tall frame, with a great box of stones hung on its short end. Let the box drop and the long end whips up and over, and flings a boulder from a sling, further than any bow can shoot.",
                "In 1304 King Edward the First had the biggest one ever built in Britain dragged to Stirling Castle. He called it the Warwolf. The castle offered to surrender before it was even finished, and he made them stay inside until he'd tried it out.")) return;
            var b = new RmBits(kit, g);
            var wood = PatchworkAssets.Planks(new Color(0.55f, 0.4f, 0.28f));
            b.Box(DarkWood, new Vector3(0f, 0.42f, 0f), new Vector3(1.7f, 0.84f, 1.1f));
            b.Box(RmFelt(new Color(0.35f, 0.5f, 0.3f)), new Vector3(0f, 0.86f, 0f), new Vector3(1.6f, 0.03f, 1f));
            Vector3 top = new Vector3(0f, 0.88f, 0f);
            // The base frame and its A-frame uprights.
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(wood, top + new Vector3(s * 0.16f, 0.03f, 0f), new Vector3(0.05f, 0.05f, 0.8f));
                b.Box(wood, top + new Vector3(s * 0.16f, 0.3f, -0.14f), new Vector3(0.04f, 0.62f, 0.04f), new Vector3(-24f, 0f, 0f));
                b.Box(wood, top + new Vector3(s * 0.16f, 0.3f, 0.14f), new Vector3(0.04f, 0.62f, 0.04f), new Vector3(24f, 0f, 0f));
            }
            foreach (float z in new[] { -0.36f, 0.36f }) b.Box(wood, top + new Vector3(0f, 0.03f, z), new Vector3(0.36f, 0.05f, 0.05f));
            b.Tube(IronMat, top + new Vector3(-0.2f, 0.58f, 0f), top + new Vector3(0.2f, 0.58f, 0f), 0.015f, 6);
            // The arm, cocked back, with its counterweight box and the sling trailing on the ground.
            Vector3 pivot = top + new Vector3(0f, 0.58f, 0f);
            Vector3 longEnd = pivot + Quaternion.Euler(-35f, 0f, 0f) * new Vector3(0f, 0f, -0.75f);
            Vector3 shortEnd = pivot + Quaternion.Euler(-35f, 0f, 0f) * new Vector3(0f, 0f, 0.25f);
            b.Tube(wood, longEnd, shortEnd, 0.025f, 6);
            b.Box(wood, shortEnd + Vector3.down * 0.14f, new Vector3(0.2f, 0.16f, 0.16f));
            for (int i = 0; i < 4; i++) b.Ball(RmFelt(new Color(0.55f, 0.55f, 0.58f)), shortEnd + new Vector3(-0.05f + (i % 2) * 0.1f, -0.04f, -0.03f + (i / 2) * 0.06f), Vector3.one * 0.06f, 2, 5);
            b.Box(RmFelt(CreamCol), (longEnd + top + new Vector3(0f, 0.05f, -0.5f)) * 0.5f, new Vector3(0.005f, (longEnd - (top + new Vector3(0f, 0.05f, -0.5f))).magnitude, 0.005f), Quaternion.FromToRotation(Vector3.up, longEnd - (top + new Vector3(0f, 0.05f, -0.5f))).eulerAngles);
            b.Ball(RmShine(new Color(0.42f, 0.26f, 0.16f), 0.3f), top + new Vector3(0f, 0.07f, -0.5f), new Vector3(0.08f, 0.04f, 0.1f), 2, 5);
            b.Ball(RmFelt(new Color(0.6f, 0.6f, 0.62f)), top + new Vector3(0f, 0.1f, -0.5f), Vector3.one * 0.06f, 2, 5);
            // A little model castle wall at the far end, with a hole in it.
            b.Box(PaleStone, top + new Vector3(-0.5f, 0.14f, 0.32f), new Vector3(0.5f, 0.28f, 0.1f));
            b.Box(PaleStone, top + new Vector3(-0.5f, 0.3f, 0.32f), new Vector3(0.08f, 0.06f, 0.1f));
            b.Ball(RmFelt(new Color(0.15f, 0.12f, 0.12f)), top + new Vector3(-0.5f, 0.15f, 0.36f), new Vector3(0.1f, 0.1f, 0.04f), 2, 6);
            b.Done("Trebuchet", true);
            RmSolid(kit, g, new Vector3(0f, 0.55f, 0f), new Vector3(1.7f, 1.1f, 1.1f));
        }

        static void ArMail(LevelKit kit)
        {
            Transform g;
            if (!RmExhibit(kit, "the shirt of mail", new Vector3(63.6f, 0f, -48f), 90f, 7f, new Vector3(1.1f, 1.9f, 0.8f), out g, "A Shirt of Mail",
                "Mail is made of iron rings, each one linked through four others and closed with its own tiny rivet, one at a time, by hand. A single shirt can have tens of thousands of them.",
                "It's heavy, but it hangs from the shoulders like a coat, and you can move freely in it. Under it a soldier wore a thick padded jacket, to soften the blows that the rings stopped.")) return;
            var b = new RmBits(kit, g);
            // A wooden stand with a padded torso, and the mail hung on it.
            b.Box(DarkWood, new Vector3(0f, 0.04f, 0f), new Vector3(0.7f, 0.08f, 0.6f));
            b.Box(DarkWood, new Vector3(0f, 0.5f, 0f), new Vector3(0.06f, 1f, 0.06f));
            b.Box(RmFelt(new Color(0.75f, 0.68f, 0.55f)), new Vector3(0f, 1.35f, 0f), new Vector3(0.5f, 0.8f, 0.3f));
            b.Box(DarkWood, new Vector3(0f, 1.82f, 0f), new Vector3(0.12f, 0.14f, 0.12f));
            var iron = RmShine(new Color(0.55f, 0.56f, 0.6f), 0.8f);
            for (int row = 0; row < 14; row++)
                for (int col = 0; col < 10; col++)
                {
                    float y = 1.05f + row * 0.05f, x = -0.24f + col * 0.052f + (row % 2) * 0.026f;
                    if (x > 0.25f) continue;
                    b.Ring(iron, new Vector3(x, y, 0.165f), new Vector3(0f, 0f, 0f), 0.022f, 0.007f, 6);
                    if (col == 0 || col == 9) b.Ring(iron, new Vector3(x < 0f ? -0.26f : 0.26f, y, 0f), new Vector3(0f, 90f, 0f), 0.022f, 0.007f, 6);
                }
            // Its hem, hanging below the torso, and one sleeve.
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < 11; col++)
                    b.Ring(iron, new Vector3(-0.26f + col * 0.052f, 0.98f - row * 0.05f, 0.16f - row * 0.01f), Vector3.zero, 0.022f, 0.007f, 6);
            for (int row = 0; row < 8; row++)
                b.Ring(iron, new Vector3(0.32f + row * 0.015f, 1.62f - row * 0.06f, 0.05f), new Vector3(0f, 90f, 0f), 0.06f, 0.012f, 10);
            // A loose handful of rings and the pliers on the base, to show how it's made.
            for (int i = 0; i < 9; i++) b.Ring(iron, new Vector3(-0.2f + (i % 3) * 0.04f, 0.09f, 0.15f + (i / 3) * 0.04f), new Vector3(90f, 0f, i * 20f), 0.02f, 0.006f, 6);
            b.Box(IronMat, new Vector3(0.18f, 0.09f, 0.18f), new Vector3(0.02f, 0.015f, 0.16f), new Vector3(0f, 10f, 0f));
            b.Box(IronMat, new Vector3(0.2f, 0.09f, 0.18f), new Vector3(0.02f, 0.015f, 0.16f), new Vector3(0f, -10f, 0f));
            b.Done("Mail", true);
            RmSolid(kit, g, new Vector3(0f, 0.9f, 0f), new Vector3(0.6f, 1.8f, 0.5f));
        }

        static void ArCatInHelmet(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the cat in the helmet", new Vector3(63.8f, 0f, -25f), new Vector3(1.1f, 0.8f, 1.1f), 60f, 7f, out f)) return;
            var g = RmGroup(kit, "CatInHelmet", f, 60f);
            var b = new RmBits(kit, g);
            var steel = RmShine(new Color(0.72f, 0.74f, 0.78f), 0.85f);
            // A great helm lying on its side, its open end facing out, with a cat asleep in it.
            for (int i = 0; i < 6; i++)
                b.Ring(steel, new Vector3(0f, 0.33f, -0.3f + i * 0.09f), Vector3.zero, 0.31f, 0.06f, 18);
            b.Ball(steel, new Vector3(0f, 0.33f, -0.36f), new Vector3(0.62f, 0.62f, 0.2f), 4, 10);
            b.Box(RmShine(new Color(0.05f, 0.05f, 0.07f), 0.4f), new Vector3(0.31f, 0.4f, -0.05f), new Vector3(0.02f, 0.03f, 0.4f));
            for (int i = 0; i < 6; i++) b.Ball(RmShine(new Color(0.05f, 0.05f, 0.07f), 0.4f), new Vector3(0.31f, 0.22f + (i % 2) * 0.05f, -0.15f + (i / 2) * 0.08f), Vector3.one * 0.02f, 2, 4);
            b.Box(Brass, new Vector3(0f, 0.65f, -0.05f), new Vector3(0.06f, 0.02f, 0.5f));
            b.Done("GreatHelm", true);
            SleepingCat(kit, g.TransformPoint(new Vector3(0f, 0.04f, 0.28f)), 60f + 180f, new Color(0.85f, 0.55f, 0.3f));
            Snores(kit, g.TransformPoint(new Vector3(0f, 0.6f, 0.4f)));
            RmSolid(kit, g, new Vector3(0f, 0.32f, -0.05f), new Vector3(0.66f, 0.64f, 0.6f));
        }

        // ================================================================ The Orrery Bridge

        static void RoomsBridge(LevelKit kit)
        {
            BrWindingKey(kit);
            BrMindTheGap(kit, new Vector3(29.2f, DeckY, 7.2f), 90f);
            BrMindTheGap(kit, new Vector3(13.1f, DeckY, 16.8f), 270f);
            BrOilCan(kit);
            BrStepLights(kit);
            BrWindowSeat(kit);
            BrKepler(kit);
        }

        static void BrWindingKey(LevelKit kit)
        {
            Vector3 at;
            if (!RmWall("the orrery's winding key", new Vector3(30f, DeckY + 2.6f, 6f), Vector3.forward, new Vector3(1.7f, 1.7f, 0.7f), 4f, out at)) return;
            var g = RmGroup(kit, "WindingKey", at, Quaternion.LookRotation(Vector3.forward));
            var b = new RmBits(kit, g);
            b.Disc(Brass, new Vector3(0f, 0f, 0.03f), 0.5f, 0.06f);
            for (int i = 0; i < 8; i++) b.Ball(Brass, new Vector3(Mathf.Cos(i * 0.785f) * 0.2f, Mathf.Sin(i * 0.785f) * 0.2f, 0.06f), Vector3.one * 0.04f, 2, 4);
            b.Done("KeyPlate", true);
            // A great brass key in the wall, as if the whole bridge were clockwork, turning slowly.
            var key = kit.Group("Key", g.TransformPoint(new Vector3(0f, 0f, 0.06f)));
            key.SetParent(g, true);
            key.localRotation = Quaternion.identity;
            var kb = new RmBits(kit, key);
            kb.Tube(Brass, Vector3.zero, new Vector3(0f, 0f, 0.4f), 0.06f, 10);
            kb.Box(Brass, new Vector3(0f, 0f, 0.46f), new Vector3(0.18f, 0.12f, 0.12f));
            foreach (int s in new[] { -1, 1 })
            {
                kb.Ring(Brass, new Vector3(s * 0.38f, 0f, 0.5f), Vector3.zero, 0.26f, 0.08f, 16);
                kb.Disc(RmFelt(Navy), new Vector3(s * 0.38f, 0f, 0.49f), 0.44f, 0.02f);
                kb.Ball(RmGlow(new Color(1f, 0.82f, 0.45f), 1.4f), new Vector3(s * 0.38f, 0f, 0.5f), new Vector3(0.12f, 0.12f, 0.05f), 2, 6);
            }
            kb.Done("WindingKey", true);
            var spin = key.gameObject.AddComponent<Spinner>();
            spin.Axis = Vector3.forward;
            spin.Speed = 10f;
        }

        static void BrMindTheGap(LevelKit kit, Vector3 want, float yaw)
        {
            Vector3 f;
            if (!RmFloor("a MIND THE GAP sign", want, new Vector3(0.8f, 1.6f, 0.3f), yaw, 1.6f, out f)) return;
            var g = RmGroup(kit, "MindTheGap", f, yaw);
            var b = new RmBits(kit, g);
            b.Cyl(Brass, Vector3.zero, 0.14f, 0.05f, 10);
            b.Box(Brass, new Vector3(0f, 0.6f, 0f), new Vector3(0.04f, 1.2f, 0.04f));
            b.Box(Brass, new Vector3(0f, 1.36f, 0f), new Vector3(0.8f, 0.42f, 0.04f));
            b.Box(RmFelt(Navy), new Vector3(0f, 1.36f, 0.022f), new Vector3(0.72f, 0.34f, 0.01f));
            var cream = RmFelt(new Color(1f, 0.92f, 0.7f));
            Letters(b[cream], "MIND", new Vector3(0.05f, 1.44f, 0.028f), Vector3.forward, 0.022f, 0.008f);
            Letters(b[cream], "THE GAP", new Vector3(0.05f, 1.27f, 0.028f), Vector3.forward, 0.016f, 0.008f);
            b.Ball(RmGlow(new Color(0.4f, 0.7f, 1f), 1f), new Vector3(-0.27f, 1.36f, 0.03f), new Vector3(0.1f, 0.1f, 0.03f), 2, 6);
            b.Ring(Brass, new Vector3(-0.27f, 1.36f, 0.035f), new Vector3(0f, 0f, 20f), 0.08f, 0.01f, 10);
            b.Done("MindTheGap", true);
            RmSolid(kit, g, new Vector3(0f, 0.7f, 0f), new Vector3(0.12f, 1.4f, 0.12f));
        }

        static void BrOilCan(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the orrery keeper's oil can", new Vector3(42.4f, DeckY, 16.6f), new Vector3(1.3f, 0.6f, 0.9f), 0f, 4f, out f)) return;
            var g = RmGroup(kit, "OrreryOil", f, 200f);
            var b = new RmBits(kit, g);
            // A wooden tool box with a great spanner, the oil can with its long spout, a rag and polish.
            var wood = PatchworkAssets.Planks(new Color(0.55f, 0.4f, 0.28f));
            b.Box(wood, new Vector3(-0.25f, 0.14f, 0f), new Vector3(0.6f, 0.28f, 0.32f));
            b.Box(wood, new Vector3(-0.25f, 0.4f, 0f), new Vector3(0.04f, 0.24f, 0.04f));
            b.Box(wood, new Vector3(-0.25f, 0.52f, 0f), new Vector3(0.5f, 0.04f, 0.04f));
            b.Box(IronMat, new Vector3(-0.25f, 0.3f, 0.06f), new Vector3(0.5f, 0.04f, 0.06f), new Vector3(0f, 0f, 12f));
            b.Ring(IronMat, new Vector3(-0.49f, 0.35f, 0.06f), Vector3.zero, 0.05f, 0.02f, 8, -60f, 240f);
            b.Cyl(Brass, new Vector3(0.3f, 0f, 0.05f), 0.11f, 0.06f, 12);
            for (int i = 0; i < 4; i++) b.Cyl(Brass, new Vector3(0.3f, 0.06f + i * 0.04f, 0.05f), 0.11f - i * 0.025f, 0.04f, 12);
            b.Tube(Brass, new Vector3(0.3f, 0.22f, 0.05f), new Vector3(0.62f, 0.34f, 0.05f), 0.012f, 5);
            b.Ring(Brass, new Vector3(0.18f, 0.16f, 0.05f), Vector3.zero, 0.07f, 0.02f, 8, 90f, 270f);
            b.Ball(RmFelt(new Color(0.85f, 0.8f, 0.7f)), new Vector3(0.15f, 0.04f, -0.25f), new Vector3(0.28f, 0.07f, 0.2f), 3, 7);
            b.Cyl(RmShine(new Color(0.75f, 0.2f, 0.2f), 0.7f), new Vector3(0.45f, 0f, -0.22f), 0.07f, 0.05f, 10);
            b.Done("OrreryOil", true);
            RmSolid(kit, g, new Vector3(-0.25f, 0.15f, 0f), new Vector3(0.6f, 0.3f, 0.32f));
        }

        static void BrStepLights(LevelKit kit)
        {
            // A little glowing star let into the wall beside every step of both stairs.
            var dots = new MeshBatch();
            for (int i = 0; i < 16; i++)
            {
                float top = 10f + 0.25f * (i + 1);
                dots.Ball(new Vector3(20f + 0.5f * (i + 0.5f), top + 0.32f, 6.03f), new Vector3(0.1f, 0.1f, 0.04f), 2, 6);
                dots.Ball(new Vector3(22f - 0.5f * (i + 0.5f), top + 0.32f, 17.97f), new Vector3(0.1f, 0.1f, 0.04f), 2, 6);
            }
            Twinkling(dots, kit.Root, "StairStars", new Color(1f, 0.85f, 0.5f), 1f, 2.2f, 1.1f, 0f);
            rmPlaced++;
        }

        static void BrWindowSeat(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the window seat", new Vector3(39.4f, DeckY, 16.9f), new Vector3(2.4f, 1.4f, 0.8f), 0f, 4f, out f)) return;
            TownKit.Bench(kit, f, 0f);
            var g = RmGroup(kit, "WindowSeat", f, 0f);
            var b = new RmBits(kit, g);
            // Cushions to sit on and look out at the deep, a folded blanket and a mug.
            b.Ball(RmFelt(Plum), new Vector3(-0.5f, 0.6f, 0f), new Vector3(0.6f, 0.14f, 0.45f), 3, 8);
            b.Ball(RmFelt(Teal), new Vector3(0.4f, 0.6f, 0f), new Vector3(0.6f, 0.14f, 0.45f), 3, 8);
            b.Ball(RmFelt(new Color(0.95f, 0.8f, 0.4f)), new Vector3(-0.85f, 0.85f, -0.18f), new Vector3(0.36f, 0.34f, 0.14f), 3, 8);
            var tartan = PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, Wine, CreamCol);
            b.Box(tartan, new Vector3(0.82f, 0.62f, 0.02f), new Vector3(0.4f, 0.1f, 0.4f));
            b.Box(tartan, new Vector3(0.82f, 0.69f, 0.02f), new Vector3(0.38f, 0.04f, 0.38f), new Vector3(0f, 8f, 0f));
            b.Cyl(RmFelt(CreamCol), new Vector3(0.05f, 0.55f, 0.15f), 0.05f, 0.11f, 10);
            b.Ring(RmFelt(CreamCol), new Vector3(0.11f, 0.61f, 0.15f), new Vector3(0f, 0f, 0f), 0.03f, 0.012f, 8, -90f, 90f);
            b.Done("WindowSeat", true);
        }

        static void BrKepler(LevelKit kit)
        {
            Transform g;
            if (!RmExhibit(kit, "Kepler's ellipse", new Vector3(5f, DeckY, 16.4f), 180f, 6f, new Vector3(1.3f, 1.5f, 1.1f), out g, "Kepler's Rule",
                "About four hundred years ago Johannes Kepler worked out that the planets don't go round the Sun in perfect circles. Each one goes round in an ellipse, a slightly squashed circle, with the Sun a little to one side of the middle.",
                "And a planet speeds up as it swings in close to the Sun, and slows down again as it climbs away. Watch the little one on the table go round.")) return;
            var b = new RmBits(kit, g);
            b.Box(DarkWood, new Vector3(0f, 0.45f, 0f), new Vector3(1.3f, 0.9f, 1.1f));
            b.Box(RmFelt(new Color(0.06f, 0.06f, 0.16f)), new Vector3(0f, 0.915f, 0f), new Vector3(1.22f, 0.03f, 1.02f));
            // The ellipse, drawn in brass, with the Sun at one focus.
            const float A = 0.45f, E = 0.55f;
            float B = A * Mathf.Sqrt(1f - E * E);
            Vector3 sun = new Vector3(0.12f, 0.95f, 0f);
            for (int i = 0; i < 40; i++)
            {
                float e0 = i / 40f * Mathf.PI * 2f, e1 = (i + 1) / 40f * Mathf.PI * 2f;
                b.Tube(Brass, sun + new Vector3(A * (Mathf.Cos(e0) - E), 0f, B * Mathf.Sin(e0)), sun + new Vector3(A * (Mathf.Cos(e1) - E), 0f, B * Mathf.Sin(e1)), 0.008f, 4);
            }
            b.Ball(RmGlow(new Color(1f, 0.78f, 0.35f), 2.4f), sun + Vector3.up * 0.06f, Vector3.one * 0.12f, 3, 8);
            b.Box(Brass, sun + Vector3.up * 0.0f, new Vector3(0.03f, 0.03f, 0.03f));
            b.Done("KeplerTable", true);
            var planet = kit.Group("KeplerPlanet", g.TransformPoint(sun + new Vector3(0f, 0.05f, 0f)));
            planet.SetParent(g, true);
            var pivot = kit.Group("KeplerSun", g.TransformPoint(sun));
            pivot.SetParent(g, true);
            pivot.localRotation = Quaternion.identity;
            planet.SetParent(pivot, true);
            NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.07f, RmGlow(new Color(0.45f, 0.7f, 1f), 1.4f), default(Vector3), planet));
            var orbit = planet.gameObject.AddComponent<KeplerOrbit>();
            orbit.A = A;
            orbit.E = E;
            orbit.Period = 8f;
            PointLight(kit, g.TransformPoint(sun + Vector3.up * 0.4f), new Color(1f, 0.82f, 0.5f), 3f, 0.8f);
            RmSolid(kit, g, new Vector3(0f, 0.47f, 0f), new Vector3(1.3f, 0.94f, 1.1f));
        }

        // ================================================================ The Star Corridor

        static void RoomsCorridor(LevelKit kit)
        {
            float Y = DeckY;
            CrCuckoo(kit, Y);
            CrPlanisphere(kit, Y);
            CrMeteorite(kit, Y);
            CrMoonPhases(kit, Y);
            CrAerogel(kit, Y);
            CrDeckChair(kit, Y);
        }

        static void CrCuckoo(LevelKit kit, float Y)
        {
            Vector3 at;
            if (!RmWall("the cuckoo clock", new Vector3(46.4f, Y + 2.6f, -35f), Vector3.back, new Vector3(0.9f, 1.6f, 0.5f), 3f, out at)) return;
            var g = RmGroup(kit, "CuckooClock", at, Quaternion.LookRotation(Vector3.back));
            var b = new RmBits(kit, g);
            var wood = PatchworkAssets.Planks(new Color(0.45f, 0.3f, 0.2f));
            var dark = PatchworkAssets.Planks(new Color(0.3f, 0.2f, 0.14f));
            // A little wooden house with a steep roof, carved leaves, and a door under the eaves.
            b.Box(wood, new Vector3(0f, 0.1f, 0.18f), new Vector3(0.56f, 0.6f, 0.34f));
            foreach (int s in new[] { -1, 1 })
                b.Box(dark, new Vector3(s * 0.17f, 0.52f, 0.2f), new Vector3(0.42f, 0.05f, 0.42f), new Vector3(0f, 0f, s * -40f));
            b.Box(dark, new Vector3(0f, 0.66f, 0.2f), new Vector3(0.06f, 0.06f, 0.44f));
            for (int i = 0; i < 6; i++)
                b.Ball(RmFelt(new Color(0.35f, 0.5f, 0.3f)), new Vector3(-0.24f + i * 0.096f, -0.22f + Mathf.Sin(i * 1.4f) * 0.03f, 0.36f), new Vector3(0.12f, 0.07f, 0.03f), 2, 6);
            b.Box(dark, new Vector3(0f, 0.36f, 0.355f), new Vector3(0.16f, 0.14f, 0.01f));
            b.Disc(RmFelt(CreamCol), new Vector3(0f, 0.08f, 0.355f), 0.28f, 0.01f);
            b.Ring(Brass, new Vector3(0f, 0.08f, 0.36f), Vector3.zero, 0.14f, 0.015f, 14);
            for (int i = 0; i < 12; i++) b.Box(RmFelt(new Color(0.15f, 0.1f, 0.1f)), new Vector3(Mathf.Sin(i * 0.5236f) * 0.11f, 0.08f + Mathf.Cos(i * 0.5236f) * 0.11f, 0.365f), new Vector3(0.008f, 0.02f, 0.004f), new Vector3(0f, 0f, -i * 30f));
            b.Box(RmFelt(new Color(0.1f, 0.08f, 0.08f)), new Vector3(0f, 0.11f, 0.37f), new Vector3(0.01f, 0.07f, 0.004f));
            b.Box(RmFelt(new Color(0.1f, 0.08f, 0.08f)), new Vector3(0.03f, 0.08f, 0.372f), new Vector3(0.06f, 0.008f, 0.004f), new Vector3(0f, 0f, 20f));
            // The chains and pine-cone weights hanging under it.
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(Brass, new Vector3(s * 0.08f, -0.5f, 0.2f), new Vector3(0.008f, 0.6f, 0.008f));
                b.Ball(PatchworkAssets.Planks(new Color(0.55f, 0.38f, 0.2f)), new Vector3(s * 0.08f, -0.85f + s * 0.06f, 0.2f), new Vector3(0.07f, 0.18f, 0.07f), 3, 6);
            }
            b.Done("CuckooClock", true);
            var pend = kit.Group("Pendulum", g.TransformPoint(new Vector3(0f, -0.2f, 0.36f)));
            pend.SetParent(g, true);
            pend.localRotation = Quaternion.identity;
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.2f, 0f), new Vector3(0.012f, 0.4f, 0.006f), Brass, default(Vector3), pend));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.42f, 0f), new Vector3(0.09f, 0.008f, 0.09f), Brass, new Vector3(90f, 0f, 0f), pend));
            pend.gameObject.AddComponent<RmTick>();
            // The door and the bird behind it, who comes out to call now and then.
            var door = kit.Group("CuckooDoor", g.TransformPoint(new Vector3(-0.08f, 0.36f, 0.36f)));
            door.SetParent(g, true);
            door.localRotation = Quaternion.identity;
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0.08f, 0f, 0f), new Vector3(0.15f, 0.13f, 0.01f), wood, default(Vector3), door));
            var bird = kit.Group("Cuckoo", g.TransformPoint(new Vector3(0f, 0.36f, 0.25f)));
            bird.SetParent(g, true);
            bird.localRotation = Quaternion.identity;
            var bb = new RmBits(kit, bird);
            bb.Ball(RmFelt(new Color(0.55f, 0.5f, 0.45f)), Vector3.zero, new Vector3(0.08f, 0.08f, 0.12f), 3, 6);
            bb.Ball(RmFelt(new Color(0.55f, 0.5f, 0.45f)), new Vector3(0f, 0.04f, 0.06f), Vector3.one * 0.06f, 3, 6);
            bb.Box(RmFelt(new Color(1f, 0.75f, 0.2f)), new Vector3(0f, 0.04f, 0.1f), new Vector3(0.015f, 0.015f, 0.04f));
            bb.Done("Cuckoo");
            var pop = bird.gameObject.AddComponent<PopOut>();
            pop.Out = 0.16f;
            pop.Every = 15f;
            pop.Hold = 1.8f;
            pop.Door = door;
            Mark("Secret_CuckooClock", at);
        }

        static void CrPlanisphere(LevelKit kit, float Y)
        {
            Vector3 at;
            if (!RmWall("the planisphere", new Vector3(39f, Y + 2.2f, -29f), Vector3.left, new Vector3(1.3f, 1.3f, 0.2f), 8f, out at)) return;
            var g = RmGroup(kit, "Planisphere", at, Quaternion.LookRotation(Vector3.left));
            var b = new RmBits(kit, g);
            b.Ring(Brass, new Vector3(0f, 0f, 0.12f), Vector3.zero, 0.6f, 0.06f, 28);
            // The ring of months round the edge, a mark for each.
            for (int i = 0; i < 12; i++)
                b.Box(RmFelt(i % 3 == 0 ? new Color(1f, 0.82f, 0.4f) : CreamCol), new Vector3(Mathf.Sin(i * 0.5236f) * 0.56f, Mathf.Cos(i * 0.5236f) * 0.56f, 0.13f), new Vector3(0.015f, 0.06f, 0.01f), new Vector3(0f, 0f, -i * 30f));
            // The cream cover with its oval window onto tonight's sky.
            b.Ring(RmFelt(new Color(0.92f, 0.86f, 0.7f)), new Vector3(0f, 0f, 0.115f), Vector3.zero, 0.42f, 0.16f, 24);
            b.Done("Planisphere", true);
            // The star wheel underneath, turning once a minute instead of once a day.
            var wheel = kit.Group("StarWheel", g.TransformPoint(new Vector3(0f, 0f, 0.06f)));
            wheel.SetParent(g, true);
            wheel.localRotation = Quaternion.identity;
            NoShadow(kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.12f, 0.02f, 1.12f), RmFelt(new Color(0.06f, 0.08f, 0.22f)), new Vector3(90f, 0f, 0f), wheel));
            var stars = new MeshBatch();
            var rng = new System.Random(88);
            for (int i = 0; i < 70; i++)
            {
                float a = (float)rng.NextDouble() * Mathf.PI * 2f, r = Mathf.Sqrt((float)rng.NextDouble()) * 0.52f;
                stars.Ball(new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0.025f), Vector3.one * (0.012f + (float)rng.NextDouble() * 0.02f), 2, 4);
            }
            // The Plough, picked out bigger.
            Vector2[] plough = { new Vector2(-0.2f, 0.1f), new Vector2(-0.1f, 0.12f), new Vector2(0f, 0.08f), new Vector2(0.08f, 0.02f), new Vector2(0.12f, -0.08f), new Vector2(0.24f, -0.06f), new Vector2(0.22f, 0.04f) };
            foreach (var p in plough) stars.Ball(new Vector3(p.x, p.y, 0.03f), Vector3.one * 0.035f, 2, 5);
            Twinkling(stars, wheel, "PlanisphereStars", new Color(1f, 0.92f, 0.75f), 0.8f, 1.8f, 0.9f, 0.4f);
            var spin = wheel.gameObject.AddComponent<Spinner>();
            spin.Axis = Vector3.forward;
            spin.Speed = 6f;
        }

        static void CrMeteorite(LevelKit kit, float Y)
        {
            Transform g;
            if (!RmExhibit(kit, "the meteorite", new Vector3(43f, Y, -11.4f), 180f, 5f, new Vector3(1f, 1.1f, 0.8f), out g, "Touch a Shooting Star",
                "This lump of iron has come further than anything else in the museum: it fell out of the sky. It's a piece of the inside of an asteroid that broke up long, long ago.",
                "The biggest ever found is still lying where it landed, on a farm in Namibia: the Hoba meteorite, about sixty tonnes, too heavy for anyone to move. Go on, touch this one. Everybody does. That's why that bit is shiny.")) return;
            var b = new RmBits(kit, g);
            b.Box(DarkStone, new Vector3(0f, 0.35f, 0f), new Vector3(0.9f, 0.7f, 0.7f));
            b.Box(PaleStone, new Vector3(0f, 0.71f, 0f), new Vector3(0.96f, 0.04f, 0.76f));
            var iron = RmShine(new Color(0.3f, 0.28f, 0.27f), 0.45f);
            b.Ball(iron, new Vector3(0f, 0.86f, 0f), new Vector3(0.6f, 0.34f, 0.46f), 4, 9);
            b.Ball(iron, new Vector3(0.16f, 0.9f, 0.05f), new Vector3(0.34f, 0.3f, 0.3f), 3, 7);
            b.Ball(iron, new Vector3(-0.18f, 0.84f, -0.06f), new Vector3(0.3f, 0.24f, 0.26f), 3, 7);
            // Thumbprint dents all over it, from burning through the air, and the polished patch.
            for (int i = 0; i < 9; i++) b.Ball(RmShine(new Color(0.2f, 0.19f, 0.19f), 0.4f), new Vector3(Mathf.Sin(i * 2.1f) * 0.24f, 0.92f + Mathf.Cos(i * 1.3f) * 0.06f, Mathf.Cos(i * 2.1f) * 0.18f), new Vector3(0.08f, 0.05f, 0.07f), 2, 5);
            b.Ball(RmShine(new Color(0.85f, 0.85f, 0.88f), 0.97f), new Vector3(0.02f, 0.98f, 0.14f), new Vector3(0.16f, 0.08f, 0.1f), 3, 7);
            b.Done("Meteorite", true);
            RmSolid(kit, g, new Vector3(0f, 0.5f, 0f), new Vector3(0.9f, 1f, 0.7f));
        }

        static void CrMoonPhases(LevelKit kit, float Y)
        {
            Vector3 at;
            if (!RmWall("the phases of the Moon", new Vector3(48f, Y + 3.2f, -6.5f), Vector3.right, new Vector3(4.2f, 0.7f, 0.12f), 1.5f, out at)) return;
            var g = RmGroup(kit, "MoonPhases", at, Quaternion.LookRotation(Vector3.right));
            var lit = new MeshBatch();
            var dark = new MeshBatch();
            var b = new RmBits(kit, g);
            b.Box(DarkWood, new Vector3(0f, 0f, 0.02f), new Vector3(4.2f, 0.66f, 0.04f));
            // Eight moons in a row, new to full and back, each stitched from little squares.
            const int px = 11;
            const float r = 0.22f;
            for (int m = 0; m < 8; m++)
            {
                float th = m / 8f * Mathf.PI * 2f;
                // (Local x runs to the viewer's left on this frame, so the row reads right to left.)
                Vector3 c = new Vector3(1.75f - m * 0.5f, 0f, 0.05f);
                float cell = r * 2f / px;
                for (int i = 0; i < px; i++)
                    for (int k = 0; k < px; k++)
                    {
                        float x = -r + (i + 0.5f) * cell, y = -r + (k + 0.5f) * cell;
                        if (x * x + y * y > r * r) continue;
                        float s = Mathf.Sqrt(Mathf.Max(0f, r * r - y * y));
                        // (The viewer's right is -x here.)
                        float vx = -x;
                        bool on = th <= Mathf.PI ? vx > s * Mathf.Cos(th) : vx < -s * Mathf.Cos(th);
                        (on ? lit : dark).Box(c + new Vector3(x, y, 0f), new Vector3(cell * 0.94f, cell * 0.94f, 0.02f), Quaternion.identity);
                    }
            }
            Twinkling(lit, g, "MoonLit", new Color(0.95f, 0.92f, 0.8f), 0.9f, 1.3f, 0.4f, 0f);
            dark.Build(g, "MoonDark", RmFelt(new Color(0.18f, 0.18f, 0.24f)));
            b.Box(Brass, new Vector3(0f, 0.31f, 0.045f), new Vector3(4.2f, 0.03f, 0.03f));
            b.Box(Brass, new Vector3(0f, -0.31f, 0.045f), new Vector3(4.2f, 0.03f, 0.03f));
            b.Done("MoonPhasesFrame", true);
        }

        static void CrAerogel(LevelKit kit, float Y)
        {
            Transform g;
            if (!RmExhibit(kit, "the comet dust", new Vector3(47f, Y, -2.2f), 0f, 5f, new Vector3(1f, 1.6f, 0.8f), out g, "Comet Dust",
                "In 2004 a spacecraft called Stardust flew through the cloud of dust round a comet, holding out a grid like a tennis racket, filled with aerogel: a foam of glass so light it's nearly all air.",
                "The dust hit it about six times faster than a rifle bullet, and stopped, gently, leaving long thin tracks like little carrots. In 2006 the capsule parachuted back down into the desert in Utah, and for the first time there was real comet dust to look at.")) return;
            Vector3 inside = RmCase(kit, g, new Vector3(1f, 1.6f, 0.8f), false);
            var b = new RmBits(kit, g);
            // A piece of the collecting grid: aerogel tiles in a metal frame.
            var frame = RmShine(new Color(0.75f, 0.76f, 0.8f), 0.85f);
            for (int i = 0; i <= 3; i++)
            {
                b.Box(frame, inside + new Vector3(-0.24f + i * 0.16f, 0.2f, 0f), new Vector3(0.015f, 0.34f, 0.08f));
                b.Box(frame, inside + new Vector3(0f, 0.04f + i * 0.107f, 0f), new Vector3(0.5f, 0.015f, 0.08f));
            }
            var gel = RmGlow(new Color(0.55f, 0.7f, 1f), 0.35f);
            for (int i = 0; i < 3; i++)
                for (int k = 0; k < 3; k++)
                    b.Box(gel, inside + new Vector3(-0.16f + i * 0.16f, 0.093f + k * 0.107f, 0f), new Vector3(0.14f, 0.09f, 0.06f));
            // The carrot tracks the dust made, going in.
            for (int i = 0; i < 6; i++)
                b.Box(RmFelt(new Color(0.55f, 0.35f, 0.2f)), inside + new Vector3(-0.2f + i * 0.08f, 0.1f + (i % 3) * 0.1f, 0.035f), new Vector3(0.005f, 0.005f, 0.04f), new Vector3(10f, i * 9f, 0f));
            b.Box(frame, inside + new Vector3(0f, 0.2f, -0.06f), new Vector3(0.04f, 0.04f, 0.1f));
            b.Done("CometDust", true);
            PointLight(kit, g.TransformPoint(inside + new Vector3(0f, 0.5f, 0.3f)), new Color(0.7f, 0.8f, 1f), 2.5f, 0.9f);
        }

        static void CrDeckChair(LevelKit kit, float Y)
        {
            Vector3 f;
            if (!RmFloor("the stargazer's deck chair", new Vector3(34.4f, Y, -36.8f), new Vector3(0.8f, 1.1f, 1.7f), 0f, 6f, out f)) return;
            var g = RmGroup(kit, "DeckChair", f, 0f);
            var b = new RmBits(kit, g);
            var wood = PatchworkAssets.Planks(new Color(0.75f, 0.6f, 0.4f));
            var stripes = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, Navy, new Color(1f, 0.82f, 0.45f));
            // Leaning right back, so you can lie and look at the painted sky.
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(wood, new Vector3(s * 0.3f, 0.4f, -0.3f), new Vector3(0.04f, 0.04f, 1.2f), new Vector3(-40f, 0f, 0f));
                b.Box(wood, new Vector3(s * 0.3f, 0.25f, 0.35f), new Vector3(0.04f, 0.04f, 0.8f), new Vector3(35f, 0f, 0f));
            }
            b.Box(wood, new Vector3(0f, 0.78f, -0.76f), new Vector3(0.64f, 0.04f, 0.04f));
            b.Box(wood, new Vector3(0f, 0.04f, 0.65f), new Vector3(0.64f, 0.04f, 0.04f));
            b.Box(stripes, new Vector3(0f, 0.45f, -0.3f), new Vector3(0.56f, 0.02f, 1.1f), new Vector3(-30f, 0f, 0f));
            // A tartan blanket over the end, a flask, and a star book open face down.
            b.Box(PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, new Color(0.2f, 0.4f, 0.3f), new Color(0.85f, 0.3f, 0.3f)), new Vector3(0f, 0.25f, 0.18f), new Vector3(0.62f, 0.03f, 0.4f), new Vector3(-14f, 0f, 0f));
            b.Cyl(RmShine(new Color(0.25f, 0.4f, 0.6f), 0.7f), new Vector3(0.5f, 0f, 0.4f), 0.06f, 0.28f, 10);
            b.Cyl(RmShine(new Color(0.8f, 0.8f, 0.82f), 0.9f), new Vector3(0.5f, 0.28f, 0.4f), 0.065f, 0.05f, 10);
            b.Box(RmFelt(Wine), new Vector3(-0.5f, 0.03f, 0.3f), new Vector3(0.3f, 0.04f, 0.22f), new Vector3(0f, 0f, 8f));
            b.Box(RmFelt(Wine), new Vector3(-0.5f, 0.03f, 0.3f), new Vector3(0.3f, 0.04f, 0.22f), new Vector3(0f, 0f, -8f));
            b.Done("DeckChair", true);
            RmSolid(kit, g, new Vector3(0f, 0.35f, 0f), new Vector3(0.68f, 0.7f, 1.4f));
        }

        // ================================================================ The Observatory

        static void RoomsObservatory(LevelKit kit)
        {
            ObPendulum(kit);
            ObInstruments(kit);
            ObLadder(kit);
            ObRedTorch(kit);
            ObVoyager(kit);
            ObMoonflowers(kit);
        }

        // How high the Observatory's dome is, `r` metres out from the middle.
        static float ObDomeY(float r)
        {
            float k = Mathf.Clamp01(r / ObsRadius);
            return Observatory.y + 9.4f + 15f * Mathf.Sqrt(Mathf.Max(0f, 1f - k * k));
        }

        // Turned so the group's +z points in towards the middle of the Observatory.
        static float ObFacingIn(Vector3 at)
        {
            Vector3 d = Observatory - at;
            return Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
        }

        static Vector3 ObRim(float deg, float r)
        {
            float a = deg * Mathf.Deg2Rad;
            return Observatory + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * r;
        }

        static void ObPendulum(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("Foucault's pendulum", Observatory + new Vector3(-8f, 0f, 9f), new Vector3(3f, 2.6f, 3f), 0f, 6f, out f)) return;
            float r = new Vector2(f.x - Observatory.x, f.z - Observatory.z).magnitude;
            float pivotY = ObDomeY(r + 1f) - 1.2f;
            float bobY = f.y + 1.1f, len = pivotY - bobY;
            var g = kit.Group("FoucaultPendulum", f);
            var b = new RmBits(kit, g);
            // A brass ring on the floor, with a compass star in it and a ring of little pegs.
            b.Ring(Brass, new Vector3(0f, 0.05f, 0f), new Vector3(90f, 0f, 0f), 1.4f, 0.1f, 36);
            b.Ring(Brass, new Vector3(0f, 0.05f, 0f), new Vector3(90f, 0f, 0f), 0.9f, 0.04f, 30);
            for (int i = 0; i < 8; i++) b.Box(RmFelt(new Color(1f, 0.82f, 0.4f)), new Vector3(0f, 0.035f, 0f), new Vector3(0.05f, 0.02f, i % 2 == 0 ? 1.7f : 1.1f), new Vector3(0f, i * 22.5f, 0f));
            for (int i = 0; i < 24; i++)
            {
                float a = i / 24f * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Cos(a) * 1.22f, 0f, Mathf.Sin(a) * 1.22f);
                if (i % 5 == 1) b.Box(RmFelt(new Color(0.85f, 0.25f, 0.25f)), p + Vector3.up * 0.04f, new Vector3(0.04f, 0.04f, 0.16f), new Vector3(0f, -a * Mathf.Rad2Deg + 20f, 0f));
                else
                {
                    b.Cyl(RmFelt(new Color(0.85f, 0.25f, 0.25f)), p, 0.022f, 0.16f, 6);
                    b.Ball(RmFelt(new Color(1f, 0.82f, 0.4f)), p + Vector3.up * 0.17f, Vector3.one * 0.05f, 2, 4);
                }
            }
            b.Done("PendulumRing", true);
            // The pivot up under the dome, and the bob swinging below it on its long wire.
            var mount = new RmBits(kit, g);
            mount.Box(Brass, new Vector3(0f, pivotY - f.y + 0.1f, 0f), new Vector3(0.3f, 0.2f, 0.3f));
            mount.Box(Brass, new Vector3(0f, pivotY - f.y + 0.7f, 0f), new Vector3(0.06f, 1.2f, 0.06f));
            mount.Done("PendulumMount", true);
            var pivot = kit.Group("PendulumPivot", new Vector3(f.x, pivotY, f.z));
            pivot.SetParent(g, true);
            var pb = new RmBits(kit, pivot);
            pb.Box(RmShine(new Color(0.8f, 0.8f, 0.82f), 0.9f), new Vector3(0f, -len * 0.5f, 0f), new Vector3(0.015f, len, 0.015f));
            pb.Ball(Brass, new Vector3(0f, -len, 0f), new Vector3(0.5f, 0.42f, 0.5f), 5, 12);
            pb.Ring(Brass, new Vector3(0f, -len, 0f), new Vector3(90f, 0f, 0f), 0.26f, 0.04f, 16);
            pb.Box(Brass, new Vector3(0f, -len - 0.28f, 0f), new Vector3(0.03f, 0.16f, 0.03f));
            pb.Done("PendulumBob", true);
            var swing = pivot.gameObject.AddComponent<PendulumSwing>();
            swing.Angle = Mathf.Atan2(1f, len) * Mathf.Rad2Deg;
            swing.Period = 2f * Mathf.PI * Mathf.Sqrt(len / 9.81f);
            swing.Creep = 3f;
            RmPlaque(kit, f + new Vector3(2.4f, 0f, -1.4f), ObFacingIn(f) + 180f, 3f, "Foucault's Pendulum",
                "In 1851 Leon Foucault hung a heavy ball on a very long wire from the dome of the Pantheon in Paris, set it swinging, and invited everybody in to watch.",
                "The ball keeps swinging the same way. It's the floor that moves underneath it: the whole Earth turning, carrying the room round with it, so the pegs get knocked over one by one. (This one has been sped up a good deal, so you can see it happen.)");
        }

        static void ObInstruments(LevelKit kit)
        {
            Vector3 want = ObRim(300f, ObsRadius - 1.7f);
            float yaw = ObFacingIn(want);
            Vector3 f;
            if (!RmFloor("the cabinet of instruments", want, new Vector3(2.2f, 1.3f, 0.8f), yaw, 3f, out f)) return;
            var g = RmGroup(kit, "InstrumentCabinet", f, yaw);
            var b = new RmBits(kit, g);
            b.Box(DarkWood, new Vector3(0f, 0.45f, 0f), new Vector3(2f, 0.9f, 0.7f));
            for (int i = 0; i < 3; i++) b.Box(PatchworkAssets.Planks(new Color(0.35f, 0.24f, 0.17f)), new Vector3(-0.66f + i * 0.66f, 0.45f, 0.351f), new Vector3(0.6f, 0.75f, 0.01f));
            for (int i = 0; i < 3; i++) b.Ball(Brass, new Vector3(-0.66f + i * 0.66f, 0.5f, 0.36f), Vector3.one * 0.05f, 2, 5);
            b.Box(RmFelt(Navy), new Vector3(0f, 0.92f, 0f), new Vector3(1.9f, 0.03f, 0.6f));
            foreach (int s in new[] { -1, 1 }) foreach (int t in new[] { -1, 1 }) b.Box(Brass, new Vector3(s * 0.97f, 1.04f, t * 0.32f), new Vector3(0.04f, 0.24f, 0.04f));
            b.Box(Brass, new Vector3(0f, 1.16f, 0f), new Vector3(2f, 0.03f, 0.68f));
            // A sextant: its arc, its arm and its little telescope.
            Vector3 sx = new Vector3(-0.6f, 0.95f, 0f);
            b.Ring(Brass, sx + new Vector3(0f, 0.01f, 0.08f), new Vector3(90f, 0f, 0f), 0.22f, 0.02f, 10, 240f, 300f);
            b.Box(Brass, sx + new Vector3(-0.05f, 0.01f, -0.04f), new Vector3(0.02f, 0.01f, 0.26f), new Vector3(0f, 20f, 0f));
            b.Box(Brass, sx + new Vector3(0.05f, 0.01f, -0.04f), new Vector3(0.02f, 0.01f, 0.26f), new Vector3(0f, -20f, 0f));
            b.Tube(Brass, sx + new Vector3(-0.08f, 0.04f, 0f), sx + new Vector3(0.1f, 0.04f, 0.02f), 0.02f, 6);
            // A compass in its box, a ship's chronometer, a pair of dividers and a rolled chart.
            Vector3 cp = new Vector3(0f, 0.95f, 0f);
            b.Box(PatchworkAssets.Planks(new Color(0.55f, 0.38f, 0.25f)), cp + Vector3.up * 0.04f, new Vector3(0.26f, 0.08f, 0.26f));
            b.Cyl(Brass, cp + Vector3.up * 0.08f, 0.1f, 0.01f, 12);
            b.Cyl(RmFelt(CreamCol), cp + Vector3.up * 0.085f, 0.085f, 0.01f, 12);
            b.Box(RmFelt(new Color(0.8f, 0.15f, 0.15f)), cp + Vector3.up * 0.1f, new Vector3(0.012f, 0.004f, 0.14f), new Vector3(0f, 25f, 0f));
            Vector3 ch = new Vector3(0.45f, 0.95f, 0.05f);
            b.Box(PatchworkAssets.Planks(new Color(0.45f, 0.3f, 0.2f)), ch + Vector3.up * 0.07f, new Vector3(0.26f, 0.14f, 0.26f));
            b.Cyl(Brass, ch + Vector3.up * 0.14f, 0.09f, 0.02f, 12);
            b.Cyl(RmFelt(CreamCol), ch + Vector3.up * 0.16f, 0.075f, 0.006f, 12);
            b.Box(Brass, new Vector3(0.8f, 0.96f, -0.1f), new Vector3(0.01f, 0.01f, 0.2f), new Vector3(0f, 12f, 0f));
            b.Box(Brass, new Vector3(0.82f, 0.96f, -0.1f), new Vector3(0.01f, 0.01f, 0.2f), new Vector3(0f, -12f, 0f));
            b.Tube(RmFelt(new Color(0.92f, 0.86f, 0.7f)), new Vector3(0.2f, 0.98f, -0.2f), new Vector3(0.7f, 0.98f, -0.22f), 0.035f, 8);
            b.Done("InstrumentCabinet", true);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.04f, 0f), new Vector3(1.9f, 0.22f, 0.6f), CaseGlass, default(Vector3), g));
            RmSolid(kit, g, new Vector3(0f, 0.6f, 0f), new Vector3(2f, 1.2f, 0.7f));
        }

        static void ObLadder(LevelKit kit)
        {
            // The library ladder, leaning on a bookcase up on the gallery.
            const float deg = 193f;
            Vector3 foot = ObRim(deg, 23.15f) + Vector3.up * 5f;
            float yaw = ObFacingIn(foot);
            Vector3 c;
            var rot = Rot(yaw);
            if (!RmAir("the library ladder", foot + Vector3.up * 1.05f + rot * new Vector3(0f, 0f, 0.15f), new Vector3(0.7f, 1.9f, 0.5f), yaw, 1f, out c, 0f)) return;
            var g = RmGroup(kit, "LibraryLadder", c - Vector3.up * 1.05f - rot * new Vector3(0f, 0f, 0.15f), yaw);
            var b = new RmBits(kit, g);
            var wood = PatchworkAssets.Planks(new Color(0.55f, 0.38f, 0.24f));
            Vector3 lean = new Vector3(0f, 2.45f, -0.72f);
            foreach (int s in new[] { -1, 1 }) b.Tube(wood, new Vector3(s * 0.24f, 0f, 0f), new Vector3(s * 0.24f, 0f, 0f) + lean, 0.03f, 6);
            for (int i = 1; i <= 7; i++)
            {
                Vector3 p = lean * (i / 8f);
                b.Tube(wood, p + new Vector3(-0.24f, 0f, 0f), p + new Vector3(0.24f, 0f, 0f), 0.02f, 6);
            }
            foreach (int s in new[] { -1, 1 })
            {
                b.Ring(Brass, new Vector3(s * 0.24f, 2.48f, -0.72f), new Vector3(0f, 90f, 0f), 0.05f, 0.015f, 8);
                b.Ball(RmShine(new Color(0.1f, 0.1f, 0.1f), 0.5f), new Vector3(s * 0.24f, 0.03f, 0f), Vector3.one * 0.06f, 2, 5);
            }
            // A book left on the third rung.
            b.Box(RmFelt(Teal), lean * (3f / 8f) + new Vector3(0.05f, 0.05f, 0.02f), new Vector3(0.18f, 0.05f, 0.24f), new Vector3(0f, 14f, 0f));
            b.Done("LibraryLadder", true);
        }

        static void ObRedTorch(LevelKit kit)
        {
            Vector3 want = Observatory + new Vector3(-5f, 0f, -13f);
            Transform g;
            if (!RmExhibit(kit, "the red torch", want, ObFacingIn(want) + 180f, 6f, new Vector3(0.8f, 1.2f, 0.8f), out g, "Why the Torch Is Red",
                "Your eyes take about half an hour to get properly used to the dark, and one flash of white light undoes it all in a moment.",
                "Red light hardly troubles them at all. So astronomers read their charts and write up their notebooks by the light of a red torch, and keep their night eyes.")) return;
            var b = new RmBits(kit, g);
            var wood = PatchworkAssets.Planks(new Color(0.55f, 0.4f, 0.28f));
            b.Cyl(wood, new Vector3(0f, 0.6f, 0f), 0.32f, 0.06f, 14);
            for (int i = 0; i < 3; i++)
            {
                float a = i * 2.094f;
                b.Box(wood, new Vector3(Mathf.Cos(a) * 0.22f, 0.3f, Mathf.Sin(a) * 0.22f), new Vector3(0.05f, 0.62f, 0.05f), new Vector3(Mathf.Sin(a) * 8f, 0f, -Mathf.Cos(a) * 8f));
            }
            // The notebook, open at a page of comet sketches, a pencil and the torch.
            var paper = RmFelt(new Color(0.96f, 0.93f, 0.84f));
            b.Box(paper, new Vector3(-0.08f, 0.67f, 0f), new Vector3(0.2f, 0.01f, 0.28f), new Vector3(0f, 0f, 4f));
            b.Box(paper, new Vector3(0.12f, 0.67f, 0f), new Vector3(0.2f, 0.01f, 0.28f), new Vector3(0f, 0f, -4f));
            var pencilInk = RmFelt(new Color(0.3f, 0.3f, 0.35f));
            for (int i = 0; i < 3; i++)
            {
                b.Ball(pencilInk, new Vector3(-0.12f, 0.678f, -0.08f + i * 0.08f), Vector3.one * 0.025f, 2, 4);
                b.Box(pencilInk, new Vector3(-0.06f, 0.678f, -0.08f + i * 0.08f + 0.01f), new Vector3(0.1f, 0.003f, 0.012f), new Vector3(0f, 12f, 0f));
            }
            for (int i = 0; i < 5; i++) b.Box(pencilInk, new Vector3(0.12f, 0.678f, -0.1f + i * 0.045f), new Vector3(0.14f, 0.003f, 0.006f));
            b.Tube(RmFelt(new Color(1f, 0.8f, 0.2f)), new Vector3(0.05f, 0.68f, 0.18f), new Vector3(0.22f, 0.68f, 0.12f), 0.008f, 5);
            b.Tube(RmShine(new Color(0.15f, 0.15f, 0.18f), 0.6f), new Vector3(-0.2f, 0.7f, -0.2f), new Vector3(-0.05f, 0.7f, -0.22f), 0.03f, 8);
            b.Disc(RmGlow(new Color(1f, 0.12f, 0.08f), 2.4f), new Vector3(-0.04f, 0.7f, -0.222f), 0.05f, 0.01f);
            b.Done("RedTorch", true);
            var lamp = PointLight(kit, g.TransformPoint(new Vector3(0.1f, 0.75f, -0.15f)), new Color(1f, 0.15f, 0.1f), 2.2f, 1.2f);
            lamp.transform.SetParent(g, true);
            RmSolid(kit, g, new Vector3(0f, 0.32f, 0f), new Vector3(0.6f, 0.64f, 0.6f));
        }

        static void ObVoyager(LevelKit kit)
        {
            Vector3 want = Observatory + new Vector3(9f, 8.5f, 7f);
            Vector3 c;
            if (!RmAir("Voyager", want, new Vector3(3.4f, 2.2f, 3.4f), 0f, 5f, out c, 0.8f)) return;
            float r = new Vector2(c.x - Observatory.x, c.z - Observatory.z).magnitude;
            float top = ObDomeY(r + 1f) - 0.6f;
            var hang = kit.Group("VoyagerWire", c);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, (top - c.y) * 0.5f + 0.3f, 0f), new Vector3(0.012f, top - c.y - 0.6f, 0.012f), RmShine(new Color(0.8f, 0.8f, 0.82f), 0.9f), default(Vector3), hang));
            var g = kit.Group("Voyager", c);
            g.SetParent(hang, true);
            var b = new RmBits(kit, g);
            var white = RmFelt(new Color(0.94f, 0.94f, 0.92f));
            var foil = RmShine(new Color(0.85f, 0.7f, 0.3f), 0.9f);
            var dark = RmShine(new Color(0.15f, 0.15f, 0.18f), 0.6f);
            // The body: a ten-sided box wrapped in gold foil.
            for (int i = 0; i < 10; i++) b.Box(foil, new Vector3(Mathf.Cos(i * 0.628f) * 0.3f, 0f, Mathf.Sin(i * 0.628f) * 0.3f), new Vector3(0.2f, 0.26f, 0.04f), new Vector3(0f, -i * 36f + 90f, 0f));
            b.Cyl(dark, new Vector3(0f, -0.13f, 0f), 0.3f, 0.26f, 10);
            // The great dish on top, pointing back home.
            for (int k = 0; k < 4; k++) b.Ring(white, new Vector3(0f, 0.26f + k * 0.06f, 0f), new Vector3(90f, 0f, 0f), 0.25f + k * 0.2f, 0.06f, 22 + k * 4);
            b.Cyl(white, new Vector3(0f, 0.13f, 0f), 0.2f, 0.14f, 12);
            b.Tube(white, new Vector3(0f, 0.3f, 0f), new Vector3(0f, 0.75f, 0f), 0.02f, 6);
            b.Ball(white, new Vector3(0f, 0.78f, 0f), Vector3.one * 0.08f, 2, 6);
            // Its long arms: the magnetometer boom, the science boom, and the power supply.
            b.Tube(dark, new Vector3(0.3f, 0f, 0f), new Vector3(1.6f, -0.05f, 0.1f), 0.015f, 5);
            b.Box(white, new Vector3(1.62f, -0.05f, 0.1f), new Vector3(0.06f, 0.06f, 0.06f));
            b.Tube(dark, new Vector3(-0.25f, 0f, 0.15f), new Vector3(-1f, -0.05f, 0.5f), 0.025f, 6);
            b.Box(white, new Vector3(-1.05f, -0.05f, 0.52f), new Vector3(0.22f, 0.16f, 0.16f));
            b.Box(dark, new Vector3(-1.05f, -0.05f, 0.65f), new Vector3(0.08f, 0.08f, 0.12f));
            b.Tube(dark, new Vector3(-0.2f, -0.05f, -0.2f), new Vector3(-0.8f, -0.1f, -0.7f), 0.025f, 6);
            for (int k = 0; k < 3; k++) b.Cyl(dark, new Vector3(-0.8f - k * 0.08f, -0.18f, -0.7f - k * 0.08f), 0.07f, 0.12f, 8);
            // The golden record, on the side of the body.
            b.Disc(RmGlow(new Color(1f, 0.78f, 0.35f), 0.9f), new Vector3(0f, 0f, 0.33f), 0.24f, 0.01f);
            b.Disc(dark, new Vector3(0f, 0f, 0.337f), 0.05f, 0.006f);
            b.Done("Voyager", true);
            var spin = g.gameObject.AddComponent<Spinner>();
            spin.Speed = 4f;
            RmPlaque(kit, new Vector3(c.x, Observatory.y, c.z) + new Vector3(1.8f, 0f, -1.2f), ObFacingIn(c) + 180f, 4f, "Voyager",
                "In 1977 two little spacecraft, Voyager 1 and Voyager 2, set off to visit the giant planets. They are still going: further from home than anything people have ever made, out past the edge of the Sun's own wind, in between the stars.",
                "Each one carries a golden record, with music, the sound of the sea, greetings in fifty-five languages and pictures of us, in case somebody out there ever finds it.");
        }

        static void ObMoonflowers(LevelKit kit)
        {
            Vector3 want = ObRim(40f, ObsRadius - 1.8f);
            float yaw = ObFacingIn(want);
            Vector3 f;
            if (!RmFloor("the moonflowers", want, new Vector3(1.4f, 2.2f, 1.4f), yaw, 4f, out f)) return;
            var g = RmGroup(kit, "Moonflowers", f, yaw);
            var b = new RmBits(kit, g);
            var clay = RmFelt(new Color(0.75f, 0.42f, 0.3f));
            for (int i = 0; i < 6; i++) b.Ring(clay, new Vector3(0f, 0.06f + i * 0.1f, 0f), new Vector3(90f, 0f, 0f), 0.32f + i * 0.025f, 0.1f, 18);
            b.Cyl(clay, Vector3.zero, 0.3f, 0.06f, 14);
            b.Cyl(RmFelt(new Color(0.3f, 0.22f, 0.16f)), new Vector3(0f, 0.5f, 0f), 0.42f, 0.1f, 14);
            // A trellis, a vine winding up it, and its white trumpets wide open, as they only are at night.
            var cane = PatchworkAssets.Planks(new Color(0.7f, 0.58f, 0.4f));
            foreach (int s in new[] { -1, 1 })
                b.Box(cane, new Vector3(s * 0.25f, 1.3f, -0.1f), new Vector3(0.025f, 1.6f, 0.025f));
            for (int k = 0; k < 4; k++) b.Box(cane, new Vector3(0f, 0.8f + k * 0.38f, -0.1f), new Vector3(0.55f, 0.02f, 0.02f));
            var leaf = RmFelt(new Color(0.28f, 0.5f, 0.3f));
            var vine = RmFelt(new Color(0.35f, 0.55f, 0.3f));
            var flowers = new MeshBatch();
            Vector3 prev = new Vector3(0f, 0.55f, 0f);
            for (int i = 1; i <= 20; i++)
            {
                float t = i / 20f;
                Vector3 p = new Vector3(Mathf.Sin(t * 12f) * 0.24f, 0.55f + t * 1.55f, -0.1f + Mathf.Cos(t * 12f) * 0.08f);
                b.Tube(vine, prev, p, 0.012f, 5);
                if (i % 2 == 0) b.Ball(leaf, p + new Vector3(0.06f, 0f, 0.05f), new Vector3(0.14f, 0.03f, 0.12f), 2, 6);
                if (i % 4 == 1)
                {
                    Vector3 fc = p + new Vector3(0f, 0.02f, 0.1f);
                    flowers.Ball(fc, new Vector3(0.22f, 0.22f, 0.05f), 3, 10);
                    RmTube(flowers, fc + Vector3.back * 0.02f, fc + new Vector3(0f, -0.04f, -0.16f), 0.025f, 6);
                }
                prev = p;
            }
            Twinkling(flowers, g, "Moonflowers", new Color(0.95f, 0.97f, 1f), 0.6f, 1.4f, 0.25f, 0f);
            b.Box(cane, new Vector3(0.3f, 0.75f, 0.2f), new Vector3(0.02f, 0.5f, 0.02f));
            b.Box(RmFelt(CreamCol), new Vector3(0.3f, 0.98f, 0.21f), new Vector3(0.36f, 0.1f, 0.01f));
            Letters(b[RmFelt(new Color(0.25f, 0.3f, 0.5f))], "MOONFLOWER", new Vector3(0.3f, 0.98f, 0.216f), Vector3.forward, 0.006f, 0.004f);
            b.Done("Moonflowers", true);
            RmSolid(kit, g, new Vector3(0f, 0.32f, 0f), new Vector3(0.8f, 0.64f, 0.8f));
        }
    }

    // A clock's pendulum: ticks back and forth.
    public class RmTick : MonoBehaviour
    {
        public float Angle = 12f, Period = 1.2f;
        void Update() { transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * Mathf.PI * 2f / Period) * Angle); }
    }
}

using UnityEngine;

namespace Patchwork
{
    // The Star Corridor: the long way round from the top of the cardboard lift to the Orrery
    // Bridge, winding through five rooms, each a little further out into the sky:
    //   The Hall of Clocks     - clocks all telling different times, and John Harrison's sea
    //                            clock (the lift opens here)
    //   The Night Sky Gallery  - a long gallery under painted stars, with tall night windows,
    //                            star charts, celestial globes, shooting stars, and Professor Hoot
    //   The Planet Walk        - the planets hung in order down the corridor, slowly turning
    //   The Moon Steps         - a moon overhead, moon dust and craters underfoot, a lander
    //   The Comet Gallery      - a great comet with a long glittering tail, and the door out
    //                            onto the bridge
    public static partial class Level2Builder
    {
        const float CorH = 6.5f;   // floor to ceiling

        static void StarCorridor(LevelKit kit)
        {
            float Y = DeckY;
            var stone = Sandstone;
            var ceil = PatchworkAssets.Felt(new Color(0.06f, 0.06f, 0.2f));

            // ----- Floors (one per room, none overlapping) and ceilings -----
            float[][] rooms =
            {
                new[] { 39f, 47.8f, -41f, -35f },   // Hall of Clocks
                new[] { 33f, 39f, -41f, -10f },     // Night Sky Gallery
                new[] { 39f, 54f, -16f, -10f },     // Planet Walk
                new[] { 48f, 54f, -10f, -3f },      // Moon Steps
                new[] { 33f, 54f, -3f, 3f },        // Comet Gallery
            };
            Color[][] quilts = { WarmQuilt, FloorQuilt, FloorQuilt, SeaQuilt, FloorQuilt };
            for (int i = 0; i < rooms.Length; i++)
            {
                var r = rooms[i];
                Floor(kit, r[0], r[1], r[2], r[3], Y, quilts[i], 3f, "CorridorFloor");
                kit.Block(new Vector3((r[0] + r[1]) * 0.5f, Y + CorH + 0.4f, (r[2] + r[3]) * 0.5f), new Vector3(r[1] - r[0], 0.8f, r[3] - r[2]), Navy, "CorridorCeiling", ceil);
            }
            // The step through the wall onto the bridge.
            Floor(kit, 34f, 38f, 3f, 5.6f, Y, WarmQuilt, 3f, "CorridorDoorstep");

            // ----- Walls (each with its wood panelling and a gilt line, on both faces: only
            // along real walls, never across the openings between rooms) -----
            float w0 = Y - 1f, w1 = Y + CorH + 0.8f;
            var dado = new MeshBatch();
            var gilt = new MeshBatch();
            var cove = new MeshBatch();     // a moulded cornice where the walls meet the ceiling
            var rail = new MeshBatch();     // the rail along the top of the panelling
            System.Action<Vector3, Vector3, float, float> W = (a0, b0, y0, y1) =>
            {
                Wall(kit, a0, b0, y0, y1, 1f, stone);
                bool alongX = Mathf.Abs(b0.x - a0.x) > Mathf.Abs(b0.z - a0.z);
                Vector3 mid = (a0 + b0) * 0.5f;
                float len = alongX ? Mathf.Abs(b0.x - a0.x) : Mathf.Abs(b0.z - a0.z);
                for (int s2 = -1; s2 <= 1; s2 += 2)
                {
                    Vector3 off = alongX ? new Vector3(0f, 0f, s2 * 0.54f) : new Vector3(s2 * 0.54f, 0f, 0f);
                    Vector3 size = alongX ? new Vector3(len, 1f, 0.08f) : new Vector3(0.08f, 1f, len);
                    if (y0 <= Y)
                    {
                        dado.Box(new Vector3(mid.x, Y + 0.55f, mid.z) + off, new Vector3(size.x, 1.1f, size.z), Quaternion.identity);
                        rail.Box(new Vector3(mid.x, Y + 1.14f, mid.z) + off * 1.04f, new Vector3(size.x, 0.08f, size.z * 1.6f), Quaternion.identity);
                    }
                    gilt.Box(new Vector3(mid.x, Y + CorH - 0.5f, mid.z) + off * 1.02f, new Vector3(size.x, 0.1f, size.z * 1.2f), Quaternion.identity);
                    cove.Box(new Vector3(mid.x, Y + CorH - 0.13f, mid.z) + off * 1.12f, new Vector3(size.x, 0.26f, size.z * 3f), Quaternion.identity);
                    cove.Box(new Vector3(mid.x, Y + CorH - 0.3f, mid.z) + off * 1.05f, new Vector3(size.x, 0.1f, size.z * 1.8f), Quaternion.identity);
                }
            };
            // (Laid end to end without overlapping, each corner belonging to just one wall, so no
            // two faces ever sit in the same place and flicker.)
            W(new Vector3(32.5f, 0f, -42f), new Vector3(32.5f, 0f, 4f), w0, w1);         // the long west wall
            W(new Vector3(33f, 0f, -41.5f), new Vector3(47.8f, 0f, -41.5f), w0, w1);     // Gallery and Clocks, south
            W(new Vector3(40f, 0f, -34.5f), new Vector3(47.8f, 0f, -34.5f), w0, w1);     // Clocks, north
            W(new Vector3(39.5f, 0f, -35f), new Vector3(39.5f, 0f, -16f), w0, w1);       // Gallery, east
            W(new Vector3(40f, 0f, -16.5f), new Vector3(54f, 0f, -16.5f), w0, w1);       // Planets, south
            W(new Vector3(54.5f, 0f, -17f), new Vector3(54.5f, 0f, 4f), w0, w1);         // the east wall
            W(new Vector3(33f, 0f, -9.5f), new Vector3(48f, 0f, -9.5f), w0, w1);         // Gallery and Planets, north
            W(new Vector3(47.5f, 0f, -9f), new Vector3(47.5f, 0f, -4f), w0, w1);         // Moon Steps, west
            W(new Vector3(33f, 0f, -3.5f), new Vector3(48f, 0f, -3.5f), w0, w1);         // Comets, south
            W(new Vector3(33f, 0f, 3.5f), new Vector3(34f, 0f, 3.5f), w0, w1);           // Comets, north (with the door)
            W(new Vector3(38f, 0f, 3.5f), new Vector3(54f, 0f, 3.5f), w0, w1);
            W(new Vector3(34f, 0f, 3.5f), new Vector3(38f, 0f, 3.5f), Y + 5.5f, w1);
            // Over the lift's doors (the lift is lower than the corridor's ceiling).
            Wall(kit, new Vector3(48.2f, 0f, -41f), new Vector3(48.2f, 0f, -35f), Y + CarHigh + 0.4f, w1, 0.8f, stone);
            dado.Build(kit.Root, "CorridorDado", DarkWood);
            rail.Build(kit.Root, "CorridorRail", Brass);
            cove.Build(kit.Root, "CorridorCornice", PaleStone);
            // Where one room opens into the next: a beam across the ceiling, carried on a pair
            // of pale pilasters, so each room reads as its own space.
            var beams = new MeshBatch();
            var pil = new MeshBatch();
            System.Action<Vector3, Vector3> Opening = (a0, b0) =>
            {
                bool alongX = Mathf.Abs(b0.x - a0.x) > Mathf.Abs(b0.z - a0.z);
                Vector3 mid = (a0 + b0) * 0.5f;
                float len = alongX ? Mathf.Abs(b0.x - a0.x) : Mathf.Abs(b0.z - a0.z);
                beams.Box(new Vector3(mid.x, Y + CorH - 0.4f, mid.z), alongX ? new Vector3(len, 0.8f, 0.7f) : new Vector3(0.7f, 0.8f, len), Quaternion.identity);
                foreach (var end in new[] { a0, b0 })
                {
                    Vector3 inward = (mid - end).normalized;
                    Vector3 c = new Vector3(end.x, 0f, end.z) + inward * 0.17f;
                    Vector3 sz = alongX ? new Vector3(0.34f, CorH - 0.8f, 0.8f) : new Vector3(0.8f, CorH - 0.8f, 0.34f);
                    pil.Box(new Vector3(c.x, Y + (CorH - 0.8f) * 0.5f, c.z), sz, Quaternion.identity);
                    pil.Box(new Vector3(c.x, Y + CorH - 0.9f, c.z) + inward * 0.04f, sz * 1f + new Vector3(0.1f, -(CorH - 1f), 0.1f), Quaternion.identity);
                    pil.Box(new Vector3(c.x, Y + 0.15f, c.z) + inward * 0.04f, new Vector3(sz.x + 0.1f, 0.3f, sz.z + 0.1f), Quaternion.identity);
                }
            };
            Opening(new Vector3(39f, 0f, -41f), new Vector3(39f, 0f, -35f));     // Clocks into the Gallery
            Opening(new Vector3(39f, 0f, -16f), new Vector3(39f, 0f, -10f));     // Gallery into the Planets
            Opening(new Vector3(48f, 0f, -10f), new Vector3(54f, 0f, -10f));     // Planets onto the Moon Steps
            Opening(new Vector3(48f, 0f, -3f), new Vector3(54f, 0f, -3f));       // Moon Steps into the Comets
            beams.Build(kit.Root, "CorridorBeams", DarkWood, true);
            pil.Build(kit.Root, "CorridorPilasters", PaleStone, true);
            gilt.Build(kit.Root, "CorridorGilt", PatchworkAssets.Glow(new Color(1f, 0.82f, 0.45f), 0.9f));

            HallOfClocks(kit, Y);
            NightSkyGallery(kit, Y);
            PlanetWalk(kit, Y);
            MoonSteps(kit, Y);
            CometGallery(kit, Y);
            CorridorTouches(kit, Y);

            // ----- Getting round -----
            kit.AddCheckpoint(new Vector3(45.6f, Y, -38f), 270f);
            Trail(kit, 10, new Vector3(44f, Y + 1.2f, -38f), new Vector3(40f, Y + 1.2f, -38f), new Vector3(36f, Y + 1.2f, -36f));
            Trail(kit, 10, new Vector3(36f, Y + 1.2f, -32f), new Vector3(36f, Y + 1.2f, -26f), new Vector3(36f, Y + 1.2f, -20f), new Vector3(37f, Y + 1.2f, -14f));
            Trail(kit, 10, new Vector3(42f, Y + 1.2f, -13f), new Vector3(47f, Y + 1.2f, -13f), new Vector3(51f, Y + 1.2f, -11f), new Vector3(51f, Y + 1.2f, -6f));
            Trail(kit, 10, new Vector3(50f, Y + 1.2f, 0f), new Vector3(45f, Y + 1.2f, 0f), new Vector3(40f, Y + 1.2f, 0.5f));
            kit.ThreadLine(new Vector3(36f, Y + 3.4f, -39f), new Vector3(36f, Y + 3.4f, -12f), 7);
            kit.Hint(new Vector3(44.5f, Y + 1.5f, -38f), new Vector3(5f, 4f, 6f), "The Star Corridor",
                "The lift came out at the far end of the museum's Star Corridor. Follow it all the way round: it ends at the Orrery Bridge, and the bridge leads to the Observatory.");
            var shot = Shot(kit, "corridor", new Vector3(43f, Y + 1.5f, -38f), new Vector3(3f, 4f, 5f), 9f,
                new[] { new Vector3(46.5f, Y + 2.6f, -38f), new Vector3(40.5f, Y + 3.4f, -36.6f), new Vector3(36f, Y + 4.4f, -39.5f), new Vector3(44f, Y + 2.4f, -38f) },
                new[] { new Vector3(39f, Y + 2.4f, -38f), new Vector3(36f, Y + 3.4f, -26f), new Vector3(36f, Y + 3f, -11f), new Vector3(47f, Y + 1.4f, -38f) },
                "The Star Corridor.", "The long way round to the Observatory...", "...through every sky there's ever been.");
            shot.Blend = 1.2f;
        }

        // ================================================================ The Hall of Clocks

        static void HallOfClocks(LevelKit kit, float Y)
        {
            Rug(kit, new Vector3(43.4f, Y, -38f), new Vector2(8f, 2.6f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, Wine, new Color(1f, 0.82f, 0.45f)), 0f, Brass);
            // Clocks all down both walls, every one telling a different time, and all ticking.
            int n = 0;
            foreach (float x in new[] { 41f, 44f, 46.8f })
            {
                WallClock(kit, new Vector3(x, Y + 3.6f + (n % 2) * 0.5f, -35.05f), 180f, 0.55f + (n % 3) * 0.12f, n++);
                WallClock(kit, new Vector3(x - 1.4f, Y + 3.8f - (n % 2) * 0.4f, -40.95f), 0f, 0.5f + (n % 3) * 0.14f, n++);
            }
            PointLight(kit, new Vector3(43.5f, Y + 5f, -38f), LampGold, 12f, 1.2f);
            HangingLantern(kit, new Vector3(43.5f, Y + 4.6f, -38f), Y + CorH, 0f, 0f, false);
            Plaque(kit, new Vector3(42.6f, Y, -35.9f), 180f, "Harrison's Sea Clock",
                "Sailors could tell how far north they were from the stars, but not how far east or west, and ships were lost because of it. You can work it out if you know the time back home, but no clock would keep time at sea.",
                "John Harrison, a carpenter, spent most of his life making one that would. In 1761 his little H4 went all the way to Jamaica and lost only about five seconds.");
        }

        // A clock on the wall: a round face with a brass rim, twelve marks, and two hands going
        // round (much faster than they should; it's that kind of museum).
        static void WallClock(LevelKit kit, Vector3 at, float yaw, float radius, int seed)
        {
            var g = kit.Group("WallClock", at);
            g.rotation = Rot(yaw);
            Color[] faces = { CreamCol, new Color(0.95f, 0.9f, 0.75f), new Color(0.85f, 0.9f, 1f) };
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.04f), new Vector3(radius * 2.2f, 0.04f, radius * 2.2f), Brass, new Vector3(90f, 0f, 0f), g));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.1f), new Vector3(radius * 1.9f, 0.03f, radius * 1.9f), PatchworkAssets.Felt(faces[seed % faces.Length]), new Vector3(90f, 0f, 0f), g));
            var marks = new MeshBatch();
            for (int i = 0; i < 12; i++)
            {
                float a = i * 30f * Mathf.Deg2Rad;
                marks.Box(new Vector3(Mathf.Sin(a) * radius * 0.78f, Mathf.Cos(a) * radius * 0.78f, 0.15f), new Vector3(0.05f, i % 3 == 0 ? 0.16f : 0.08f, 0.03f), Quaternion.Euler(0f, 0f, -i * 30f));
            }
            marks.Build(g, "ClockMarks", PatchworkAssets.Felt(new Color(0.2f, 0.16f, 0.2f)));
            for (int h = 0; h < 2; h++)
            {
                var hand = new GameObject(h == 0 ? "HourHand" : "MinuteHand").transform;
                hand.SetParent(g, false);
                hand.localPosition = new Vector3(0f, 0f, 0.18f + h * 0.03f);
                hand.localRotation = Quaternion.Euler(0f, 0f, seed * 47f + h * 130f);
                float len = radius * (h == 0 ? 0.45f : 0.7f);
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, len * 0.5f, 0f), new Vector3(h == 0 ? 0.08f : 0.05f, len, 0.02f), PatchworkAssets.Felt(new Color(0.15f, 0.12f, 0.18f)), default(Vector3), hand));
                var sp = hand.gameObject.AddComponent<Spinner>();
                sp.Axis = Vector3.forward;
                sp.Speed = -(h == 0 ? 3f + seed % 3 : 36f + seed * 7 % 30);
            }
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, 0.22f), Vector3.one * 0.1f, Brass, default(Vector3), g));
        }

        // ================================================================ The Night Sky Gallery

        static void NightSkyGallery(LevelKit kit, float Y)
        {
            PaintedStars(kit, new Vector3(36f, Y + CorH - 0.06f, -25.5f), 5f, 30f, 70, 77);
            Rug(kit, new Vector3(36f, Y, -25.5f), new Vector2(2.4f, 28f), StarCloth, 0f, PatchworkAssets.Felt(new Color(1f, 0.82f, 0.4f)));
            // Tall night windows down the west wall.
            foreach (float z in new[] { -36f, -29.5f, -23f, -16.5f, -12f })
                NightWindow(kit, new Vector3(33.02f, Y + 1.3f, z), 90f, 2.2f, 4.6f, new Color(0.25f, 0.3f, 0.8f), 0.85f);
            // Star charts down the east wall, and celestial globes beneath them.
            int seed = 40;
            foreach (float z in new[] { -31f, -25f, -19f })
                Painting(kit, new Vector3(38.95f, Y + 3.6f, z), 270f, 2.6f, 2f, seed++);
            foreach (float z in new[] { -28f, -22f })
                CelestialGlobe(kit, new Vector3(37.8f, Y, z), z * 3f);
            // A grandfather clock at the far end, its pendulum swinging behind the glass.
            GrandfatherClock(kit, new Vector3(36f, Y, -40.7f), 0f);
            // Shooting stars across the painted sky.
            Shooting(kit, new Vector3(36f, Y + CorH - 0.4f, -40f), new Vector3(36f, Y + CorH - 0.5f, -11f), new Vector3(2f, 0.15f, 2f), 5.5f, 0f, 3f, 0.08f, new Color(1f, 0.95f, 0.8f));
            Shooting(kit, new Vector3(35f, Y + CorH - 0.5f, -12f), new Vector3(37f, Y + CorH - 0.4f, -38f), new Vector3(1.5f, 0.15f, 2f), 7f, 2.7f, 2.4f, 0.07f, new Color(0.8f, 0.9f, 1f));
            foreach (float z in new[] { -35f, -25.5f, -15f })
                PointLight(kit, new Vector3(36f, Y + 5f, z), new Color(0.8f, 0.8f, 1f), 12f, 1.1f);
            Plaque(kit, new Vector3(33.9f, Y, -33f), 90f, "The First Star Catalogue",
                "More than two thousand years ago a Greek astronomer called Hipparchus wrote down the places of about eight hundred and fifty stars, and how bright each one looked.",
                "Comparing his notes with much older ones, he found the stars had all shifted a little: the whole sky slowly wobbles, once round every twenty-six thousand years.");
            Plaque(kit, new Vector3(33.9f, Y, -14.2f), 90f, "A New Planet",
                "In 1781 William Herschel, a music teacher in Bath who built his own telescopes, found a new planet: the first anyone had found since ancient times. We call it Uranus.",
                "His sister Caroline swept the sky beside him, and found eight comets of her own.");
            Person(kit, new Vector3(34.4f, Y, -20.5f), 120f, ToyNpc.Kind.Bird, "Professor Hoot", new Color(0.55f, 0.42f, 0.3f), new Color(0.95f, 0.85f, 0.55f), 0.9f, () => new[]
            {
                "Hoo! A visitor, this late? Splendid. Nobody else ever comes the long way round.",
                "Every star you see out of those windows is a sun. Some of them have planets. Some of those planets might have owls.",
                "The Observatory's at the end, past the comet. Do say hello to the whale for me."
            });
        }

        // A celestial globe: the stars on a dark blue ball, in a brass ring, on a little stand.
        static void CelestialGlobe(LevelKit kit, Vector3 foot, float spin)
        {
            kit.Deco(PrimitiveType.Cylinder, foot + Vector3.up * 0.05f, new Vector3(0.9f, 0.05f, 0.9f), DarkWood);
            kit.Deco(PrimitiveType.Cylinder, foot + Vector3.up * 0.55f, new Vector3(0.14f, 0.5f, 0.14f), Brass);
            var g = kit.Group("CelestialGlobe", foot + Vector3.up * 1.6f);
            g.rotation = Quaternion.Euler(23f, spin, 0f);
            NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 1.1f, StarCloth, default(Vector3), g));
            Hoop(kit, g, 0.62f, 0.05f, Brass, 24);
            var axle = g.gameObject.AddComponent<Spinner>();
            axle.Speed = 9f;
            TownKit.Solid(kit, foot + Vector3.up * 1f, new Vector3(1f, 2f, 1f), 0f);
        }

        static void GrandfatherClock(LevelKit kit, Vector3 foot, float yaw)
        {
            var r = Rot(yaw);
            var g = kit.Group("GrandfatherClock", foot);
            g.rotation = r;
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.3f, 0f), new Vector3(1.1f, 2.6f, 0.6f), DarkWood, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 3f, 0f), new Vector3(1.3f, 0.9f, 0.7f), DarkWood, default(Vector3), g);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 3f, 0.36f), new Vector3(0.8f, 0.02f, 0.8f), PatchworkAssets.Felt(CreamCol), new Vector3(90f, 0f, 0f), g));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 3.5f, 0f), new Vector3(1.4f, 0.12f, 0.76f), Brass, default(Vector3), g));
            // The window in the case, and the pendulum behind it.
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.4f, 0.31f), new Vector3(0.7f, 1.7f, 0.02f), PatchworkAssets.Felt(new Color(0.12f, 0.08f, 0.1f)), default(Vector3), g));
            var pend = new GameObject("Pendulum").transform;
            pend.SetParent(g, false);
            pend.localPosition = new Vector3(0f, 2.15f, 0.34f);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.6f, 0f), new Vector3(0.04f, 1.2f, 0.02f), Brass, default(Vector3), pend));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -1.25f, 0f), new Vector3(0.36f, 0.02f, 0.36f), Brass, new Vector3(90f, 0f, 0f), pend));
            var swing = pend.gameObject.AddComponent<ClockPendulum>();
            swing.Angle = 14f;
            swing.Period = 2f;
            TownKit.Solid(kit, foot + Vector3.up * 1.75f, new Vector3(1.3f, 3.5f, 0.7f), yaw);
            for (int h = 0; h < 2; h++)
            {
                var hand = new GameObject("Hand").transform;
                hand.SetParent(g, false);
                hand.localPosition = new Vector3(0f, 3f, 0.38f + h * 0.02f);
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.15f, 0f), new Vector3(0.04f, 0.3f - h * 0.08f, 0.01f), PatchworkAssets.Felt(new Color(0.15f, 0.12f, 0.18f)), default(Vector3), hand));
                var sp = hand.gameObject.AddComponent<Spinner>();
                sp.Axis = Vector3.forward;
                sp.Speed = h == 0 ? -30f : -2.5f;
            }
        }

        // ================================================================ The Planet Walk

        static void PlanetWalk(LevelKit kit, float Y)
        {
            PaintedStars(kit, new Vector3(46.5f, Y + CorH - 0.06f, -13f), 14f, 5f, 40, 78);
            // A brass line down the floor, with a stud under each planet.
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(46.6f, Y + 0.04f, -13f), new Vector3(13.6f, 0.04f, 0.14f), Brass));
            // The sun at the start, then the planets in order, roughly to size (Jupiter's the
            // big one). Each hangs from the ceiling on a brass rod and slowly turns.
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(40.4f, Y + 3.4f, -13f), Vector3.one * 2.4f, PatchworkAssets.Glow(new Color(1f, 0.75f, 0.35f), 2.6f)));
            PointLight(kit, new Vector3(41.6f, Y + 3.4f, -13f), new Color(1f, 0.78f, 0.45f), 12f, 1.4f);
            string[] names = { "Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune" };
            float[] size = { 0.32f, 0.55f, 0.58f, 0.42f, 1.5f, 1.25f, 0.85f, 0.82f };
            Color[] col =
            {
                new Color(0.7f, 0.65f, 0.6f), new Color(0.95f, 0.8f, 0.5f), new Color(0.35f, 0.6f, 1f), new Color(0.9f, 0.45f, 0.3f),
                new Color(0.9f, 0.7f, 0.5f), new Color(0.95f, 0.85f, 0.55f), new Color(0.6f, 0.9f, 0.95f), new Color(0.35f, 0.5f, 1f)
            };
            float x = 41.8f;
            for (int i = 0; i < names.Length; i++)
            {
                x += size[i] * 0.5f + 0.35f;
                Vector3 p = new Vector3(x, Y + 3.3f + Mathf.Sin(i * 1.7f) * 0.4f, -13f + (i % 2 == 0 ? -0.6f : 0.6f));
                x += size[i] * 0.5f + 0.35f;
                Beam(kit, p + Vector3.up * size[i] * 0.5f, new Vector3(p.x, Y + CorH, p.z), 0.04f, Brass);
                var g = kit.Group("Planet_" + names[i], p);
                var body = PatchworkAssets.Glow(col[i], 0.55f);
                NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * size[i], body, default(Vector3), g));
                // Bands on the gas giants, a ring on Saturn, a moon for the Earth.
                if (i == 4 || i == 5)
                    for (int b = -1; b <= 1; b++)
                        NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, b * size[i] * 0.2f, 0f), new Vector3(size[i] * 1.01f * Mathf.Sqrt(1f - b * b * 0.16f), 0.03f, size[i] * 1.01f * Mathf.Sqrt(1f - b * b * 0.16f)), PatchworkAssets.Glow(col[i] * 0.8f, 0.6f), default(Vector3), g));
                if (i == 5)
                {
                    var ring = kit.Group("SaturnRing", p);
                    ring.rotation = Quaternion.Euler(18f, 0f, 10f);
                    Hoop(kit, ring, size[i] * 0.95f, 0.08f, PatchworkAssets.Glow(new Color(1f, 0.9f, 0.65f), 0.7f), 28);
                    Hoop(kit, ring, size[i] * 1.12f, 0.05f, PatchworkAssets.Glow(new Color(0.95f, 0.85f, 0.6f), 0.6f), 30);
                }
                if (i == 2) NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0.55f, 0.2f, 0f), Vector3.one * 0.16f, PatchworkAssets.Felt(new Color(0.85f, 0.85f, 0.82f)), default(Vector3), g));
                var turn = g.gameObject.AddComponent<Spinner>();
                turn.Speed = 20f - i * 1.5f;
                NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(p.x, Y + 0.09f, -13f), new Vector3(0.3f, 0.02f, 0.3f), Brass));
            }
            PointLight(kit, new Vector3(50f, Y + 5f, -13f), new Color(0.8f, 0.8f, 1f), 11f, 1f);
            Plaque(kit, new Vector3(52.6f, Y, -10.8f), 180f, "The Planet Found by Sums",
                "Uranus never moved quite where it should, as if something unseen were tugging at it. In 1846 Urbain Le Verrier worked out with pen and paper where that something must be.",
                "He posted his answer to Johann Galle in Berlin, who looked that very night, and found Neptune almost exactly on the spot.");
        }

        // ================================================================ The Moon Steps

        static void MoonSteps(LevelKit kit, float Y)
        {
            var dust = PatchworkAssets.Felt(new Color(0.62f, 0.62f, 0.66f));
            Rug(kit, new Vector3(51f, Y, -6.5f), new Vector2(5.4f, 6.4f), dust, 0f, PatchworkAssets.Felt(new Color(0.45f, 0.45f, 0.5f)));
            // Craters in the moon dust, each a clear step above the rug.
            var rng = new System.Random(19);
            for (int i = 0; i < 6; i++)
            {
                float r = 0.35f + (float)rng.NextDouble() * 0.5f;
                Vector3 p = new Vector3(49.2f + (float)rng.NextDouble() * 3.6f, Y + 0.15f, -9f + (float)rng.NextDouble() * 5f);
                NoShadow(kit.Deco(PrimitiveType.Cylinder, p, new Vector3(r * 2f, 0.01f, r * 2f), PatchworkAssets.Felt(new Color(0.5f, 0.5f, 0.55f))));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.03f, new Vector3(r * 1.4f, 0.01f, r * 1.4f), PatchworkAssets.Felt(new Color(0.42f, 0.42f, 0.47f))));
            }
            // Footprints across it.
            var steps = new MeshBatch();
            for (int i = 0; i < 7; i++)
                steps.Box(new Vector3(50.2f + (i % 2) * 0.5f, Y + 0.2f, -9.4f + i * 0.85f), new Vector3(0.22f, 0.02f, 0.4f), Quaternion.identity);
            steps.Build(kit.Root, "MoonFootprints", PatchworkAssets.Felt(new Color(0.36f, 0.36f, 0.42f)));
            // The moon overhead.
            Vector3 moon = new Vector3(51f, Y + 4.6f, -6.5f);
            NoShadow(kit.Deco(PrimitiveType.Sphere, moon, Vector3.one * 2.6f, PatchworkAssets.Glow(new Color(0.92f, 0.92f, 0.85f), 1.2f)));
            for (int i = 0; i < 7; i++)
            {
                float a = i * 2.1f, b = i * 0.9f;
                Vector3 d = new Vector3(Mathf.Cos(a) * Mathf.Cos(b), Mathf.Sin(b) * 0.8f - 0.3f, Mathf.Sin(a) * Mathf.Cos(b)).normalized;
                NoShadow(kit.Deco(PrimitiveType.Sphere, moon + d * 1.22f, new Vector3(0.5f, 0.5f, 0.12f) * (0.6f + i % 3 * 0.25f), PatchworkAssets.Glow(new Color(0.75f, 0.75f, 0.72f), 0.9f), Quaternion.LookRotation(d).eulerAngles));
            }
            Beam(kit, moon + Vector3.up * 1.3f, new Vector3(moon.x, Y + CorH, moon.z), 0.05f, Brass);
            PointLight(kit, moon + Vector3.down * 1.6f, new Color(0.9f, 0.9f, 1f), 9f, 1.1f);
            // A little model of the lander, on a plinth by the wall, with its flag.
            Vector3 top = Plinth(kit, new Vector3(53f, Y, -7f), new Vector3(1.4f, 1f, 1.4f));
            var g = kit.Group("Lander", top);
            g.rotation = Rot(200f);
            var foil = PatchworkAssets.Felt(new Color(0.95f, 0.78f, 0.35f));
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.35f, 0f), new Vector3(0.7f, 0.4f, 0.7f), foil, new Vector3(0f, 45f, 0f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.75f, 0f), new Vector3(0.55f, 0.4f, 0.5f), PatchworkAssets.Felt(new Color(0.8f, 0.8f, 0.82f)), default(Vector3), g);
            for (int i = 0; i < 4; i++)
            {
                float a = (45f + i * 90f) * Mathf.Deg2Rad;
                Beam(kit, top + g.rotation * new Vector3(Mathf.Cos(a) * 0.3f, 0.3f, Mathf.Sin(a) * 0.3f), top + g.rotation * new Vector3(Mathf.Cos(a) * 0.55f, 0.03f, Mathf.Sin(a) * 0.55f), 0.04f, Brass);
            }
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.5f, 0.45f, 0.3f), new Vector3(0.02f, 0.45f, 0.02f), PatchworkAssets.Felt(CreamCol), default(Vector3), g);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0.65f, 0.78f, 0.3f), new Vector3(0.28f, 0.18f, 0.01f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.85f, 0.25f, 0.25f), CreamCol), default(Vector3), g));
            Plaque(kit, new Vector3(48.9f, Y, -8.6f), 90f, "Footprints on the Moon",
                "On the twentieth of July 1969, Neil Armstrong and Buzz Aldrin landed on the Moon in a lander called Eagle, while Michael Collins circled overhead.",
                "There's no wind or rain up there, so their footprints are probably still there today, exactly as they left them.");
        }

        // ================================================================ The Comet Gallery

        static void CometGallery(LevelKit kit, float Y)
        {
            PaintedStars(kit, new Vector3(43.5f, Y + CorH - 0.06f, 0f), 20f, 5f, 60, 79);
            // The comet: a bright head and a long tail of glittering beads, streaming away from the
            // sun (which, as anyone in the museum will tell you, is back that way).
            Vector3 head = new Vector3(50.5f, Y + 4.2f, 0.3f);
            NoShadow(kit.Deco(PrimitiveType.Sphere, head, Vector3.one * 1.1f, PatchworkAssets.Glow(new Color(0.85f, 0.95f, 1f), 2.6f)));
            PointLight(kit, head, new Color(0.7f, 0.85f, 1f), 12f, 1.4f);
            var tail = new MeshBatch();
            var rng = new System.Random(1758);
            for (int i = 0; i < 160; i++)
            {
                float t = i / 160f;
                float spread = 0.2f + t * 1.6f;
                Vector3 p = head + new Vector3(-t * 15f, (float)(rng.NextDouble() - 0.5) * spread + t * 0.6f, (float)(rng.NextDouble() - 0.5) * spread);
                tail.Ball(p, Vector3.one * Mathf.Lerp(0.16f, 0.05f, t), 2, 4);
            }
            Twinkling(tail, kit.Root, "CometTail", new Color(0.75f, 0.9f, 1f), 0.9f, 2.4f, 1.6f, 0.4f);
            Motes(kit, new Vector3(43f, Y + 3.6f, 0f), new Vector3(18f, 3f, 4f), 30, new Color(0.8f, 0.92f, 1f), 1.8f, 0.06f, new Vector3(-0.2f, 0.02f, 0f));
            // Night windows down the north wall, star charts on the south.
            foreach (float x in new[] { 41.5f, 46f, 50.5f })
                NightWindow(kit, new Vector3(x, Y + 1.3f, 2.98f), 180f, 2.2f, 4.6f, new Color(0.3f, 0.35f, 0.85f), 0.85f);
            Painting(kit, new Vector3(37f, Y + 3.4f, -2.95f), 0f, 2.4f, 1.9f, 46);
            Painting(kit, new Vector3(43f, Y + 3.4f, -2.95f), 0f, 2.4f, 1.9f, 47);
            PointLight(kit, new Vector3(38f, Y + 5f, 0f), LampGold, 11f, 1.1f);
            Plaque(kit, new Vector3(44.5f, Y, 2.2f), 180f, "Halley's Comet",
                "Edmond Halley noticed that the comets seen in 1531, 1607 and 1682 followed the same path, and guessed they were one comet, coming back every seventy-six years or so.",
                "He said it would return in 1758. He didn't live to see it, but on Christmas night 1758 it came back, right on time. It's due again in 2061.");
            // The way out, with a lamp each side.
            foreach (float x in new[] { 33.4f, 38.6f })
                Sconce(kit, new Vector3(x, Y + 3.6f, 2.96f), 180f, false);
        }
    }

    // A clock's pendulum: a steady tick-tock swing (no wandering, unlike the museum's great
    // Foucault pendulum).
    public class ClockPendulum : MonoBehaviour
    {
        public float Angle = 14f, Period = 2f;

        void Update()
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * Mathf.PI * 2f / Period) * Angle);
        }
    }
}

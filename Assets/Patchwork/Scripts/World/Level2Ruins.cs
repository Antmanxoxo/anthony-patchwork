using UnityEngine;

namespace Patchwork
{
    // The last three parts of the museum.
    //   The Sunken Ruins   - a drowned temple hall: more water than floor. Broken columns to
    //                        hop, a fallen one to walk along, a wreck against the far wall, and
    //                        the Observatory lift in the dry corner at the south end.
    //   The Orrery Bridge  - a high gallery with a gap in the middle, and three planets going
    //                        round on brass arms to cross it by (or take the stairs underneath).
    //   The Observatory    - a round room under a glass dome on the sea floor: the great
    //                        telescope, the star library, and a balcony over the atrium.
    public static partial class Level2Builder
    {
        const float RuinSurface = -0.6f, RuinBed = -5f;
        static readonly Color MossCol = new Color(0.3f, 0.55f, 0.45f);

        static Material RuinStone { get { return PatchworkAssets.Plaster(new Color(0.82f, 0.84f, 0.78f)); } }
        static Material RuinDark { get { return PatchworkAssets.Cobble(new Color(0.5f, 0.56f, 0.54f)); } }

        // A broken column standing up out of the water, flat on top to land on.
        // A great stone fish just under the water, its mouth open to the sky: the spring's jet
        // rises out of its mouth. (No colliders: Water Form swims straight into the jet.)
        static void SpringMouth(LevelKit kit, Vector3 at, Material stone, float yaw)
        {
            var g = kit.Group("SpringFish", at);
            g.rotation = Rot(yaw);
            var dark = PatchworkAssets.Felt(new Color(0.05f, 0.1f, 0.12f));
            // The lips: a ring of rounded stones round the jet, just breaking the surface.
            for (int i = 0; i < 12; i++)
            {
                float a = i / 12f * Mathf.PI * 2f;
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 1.35f, 0.05f, Mathf.Sin(a) * 1.35f), new Vector3(0.75f, 0.5f, 0.75f), stone, default(Vector3), g));
            }
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.5f, 0f), new Vector3(2.3f, 0.4f, 2.3f), dark, default(Vector3), g));
            // The head and body slope away under the water, with eyes and a fin or two.
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -1.4f, -1.6f), new Vector3(3.6f, 2.6f, 4.2f), stone, default(Vector3), g));
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -2.6f, -4.6f), new Vector3(2.6f, 2f, 4f), stone, default(Vector3), g));
            for (int s = -1; s <= 1; s += 2)
            {
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 1.45f, -0.2f, -1.1f), Vector3.one * 0.5f, PatchworkAssets.Glow(new Color(0.6f, 1f, 0.85f), 1.5f), default(Vector3), g));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 1.9f, -1.6f, -2.6f), new Vector3(0.3f, 1.2f, 1.8f), stone, new Vector3(0f, 0f, s * 35f), g));
            }
        }

        static void Stump(LevelKit kit, float x, float z, float top, float radius, bool mossy = false)
        {
            RoundPlatform(kit, new Vector3(x, top, z), radius, top - RuinBed, RuinStone, RuinStone, false);
            for (float y = top - 0.5f; y > RuinSurface - 0.4f; y -= 0.9f)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(x, y, z), new Vector3(radius * 2.12f, 0.07f, radius * 2.12f), RuinDark);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(x, top - 0.12f, z), new Vector3(radius * 2.2f, 0.12f, radius * 2.2f), RuinDark);
            if (mossy)
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(x + radius * 0.4f, top + 0.01f, z - radius * 0.3f), new Vector3(radius * 1.1f, 0.1f, radius * 0.9f), PatchworkAssets.Felt(MossCol)));
        }

        // A tall fluted column (whole, or snapped off at "height"), for looks. Solid.
        static void RuinColumn(LevelKit kit, Vector3 foot, float height, float radius, bool broken)
        {
            var g = kit.Group("RuinColumn", foot);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.25f, 0f), new Vector3(radius * 2.8f, 0.5f, radius * 2.8f), RuinDark, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, height * 0.5f, 0f), new Vector3(radius * 2f, height * 0.5f, radius * 2f), RuinStone, default(Vector3), g);
            for (float y = 1.2f; y < height - 0.3f; y += 1.5f)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, y, 0f), new Vector3(radius * 2.1f, 0.05f, radius * 2.1f), RuinDark, default(Vector3), g);
            if (broken)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(radius * 0.2f, height + 0.1f, 0f), new Vector3(radius * 1.7f, 0.22f, radius * 1.5f), RuinStone, new Vector3(9f, 30f, 6f), g);
            else
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, height - 0.2f, 0f), new Vector3(radius * 2.5f, 0.2f, radius * 2.5f), RuinStone, default(Vector3), g);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, height + 0.15f, 0f), new Vector3(radius * 3f, 0.3f, radius * 3f), RuinDark, default(Vector3), g);
            }
            var col = g.gameObject.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, height * 0.5f, 0f);
            col.radius = radius;
            col.height = height;
        }

        // ================================================================ The Sunken Ruins

        static void Ruins(LevelKit kit)
        {
            var stone = DarkStone;
            float T = RuinTop;
            Color green = new Color(0.35f, 0.9f, 0.7f);

            // ----- The shell of the hall -----
            // West wall: the upper doorway from the lift, the tunnel's doorway, and a window onto the Great Deep.
            Wall(kit, new Vector3(43f, 0f, 4f), new Vector3(43f, 0f, 7f), -8f, T + 8f, 2f, stone);
            // (Solid here now: the lift's top car is out at the end of the Star Corridor.)
            Wall(kit, new Vector3(43f, 0f, 7f), new Vector3(43f, 0f, 13f), -8f, T + 8f, 2f, stone);
            ArchTrim(kit, new Vector3(43f, DeckY, 10f), 90f, 6f, 6.5f, 2f, stone, PaleStone);
            WallWithDoor(kit, new Vector3(43f, 0f, 13f), new Vector3(43f, 0f, 76f), -8f, T + 8f, 2f, stone, TunnelZ - 13f, 8f, 7f);
            Wall(kit, new Vector3(43f, 0f, 76f), new Vector3(43f, 0f, 96f), -8f, 0.6f, 2f, stone);
            Wall(kit, new Vector3(43f, 0f, 76f), new Vector3(43f, 0f, 96f), 12.6f, T + 8f, 2f, stone);
            GlassWall(kit, new Vector3(43f, 0.6f, 86f), 90f, 20f, 12f, 4, 2);
            Wall(kit, new Vector3(43f, 0f, 96f), new Vector3(43f, 0f, 102f), -8f, T + 8f, 2f, stone);
            Wall(kit, new Vector3(42f, 0f, 101f), new Vector3(102f, 0f, 101f), -8f, T + 8f, 2f, stone);
            Wall(kit, new Vector3(101f, 0f, 4f), new Vector3(101f, 0f, 102f), -8f, T + 8f, 2f, stone);
            // South wall, with the doorway through to the Hall of Arms.
            WallWithDoor(kit, new Vector3(42f, 0f, 5f), new Vector3(102f, 0f, 5f), -8f, T + 8f, 2f, stone, 38f, 8f, 7.5f);
            kit.Block(new Vector3(72f, T + 0.5f, 53f), new Vector3(60f, 1f, 98f), Navy, "RuinsCeiling", PatchworkAssets.Felt(new Color(0.05f, 0.14f, 0.13f)));
            // Light falling through cracks in the roof.
            var crack = PatchworkAssets.Glow(new Color(0.6f, 1f, 0.85f), 1.2f);
            foreach (var p in new[] { new Vector3(75f, 0f, 58f), new Vector3(60f, 0f, 84f), new Vector3(90f, 0f, 42f), new Vector3(62f, 0f, 36f), new Vector3(88f, 0f, 80f) })
            {
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(p.x + 4f, T - 0.04f, p.z), new Vector3(5f, 0.1f, 2.2f), crack, new Vector3(0f, p.x * 7f, 0f)));
                PointLight(kit, new Vector3(p.x, 7f, p.z), green, 24f, 1.5f);
            }

            // ----- Water, and the three dry places -----
            kit.Block(new Vector3(72f, RuinBed - 0.5f, 62f), new Vector3(58f, 1f, 80f), StoneCol, "RuinBed", PatchworkAssets.Felt(new Color(0.26f, 0.36f, 0.32f)));
            Pool(kit, RuinX0, RuinX1, 24f, RuinZ1, RuinSurface, RuinBed, new Color(0.2f, 0.65f, 0.5f), "RuinPool");
            // The entry terrace, at the mouth of the tunnel.
            Floor(kit, 42f, 54f, 59f, 73f, 0f, SeaQuilt, 4f, "RuinEntry");
            kit.Block(new Vector3(49f, -3.25f, 66f), new Vector3(10f, 4.5f, 14f), StoneCol, "RuinEntryBase", RuinDark);
            // The south terrace, with the lift in its corner.
            Floor(kit, RuinX0, RuinX1, RuinZ0, 24f, 0f, SeaQuilt, 4f, "RuinTerrace");
            kit.Block(new Vector3(72f, -3.5f, 15f), new Vector3(56f, 5f, 18f), StoneCol, "RuinTerraceBase", RuinDark);
            foreach (float z in new[] { 24.25f })
                kit.Deco(PrimitiveType.Cube, new Vector3(72f, 0.1f, z), new Vector3(56f, 0.2f, 0.5f), RuinStone);
            // The sea bed, seen through the water: rubble, weed and a few fish.
            var bed = new TankSpace { Kit = kit, Origin = new Vector3(72f, 0f, 24f), Yaw = 180f, Width = 56f, Height = 20f, Depth = 76f, FloorY = RuinBed, Rng = new System.Random(515) };
            Reef(bed, 30, 3f, 73f);
            for (int i = 0; i < 6; i++)
                Swim(kit, i % 2 == 0 ? Swimmer.Fish : Swimmer.Tall, new Vector3(50f + i * 8.5f, -2.4f - (i % 3) * 0.6f, 32f + (i * 23) % 60), 3f + i % 3, 2f + i % 2, (i % 2 == 0 ? 1f : -1f) * (15f + i * 3f), 0.9f,
                    i % 2 == 0 ? new Color(1f, 0.7f, 0.25f) : new Color(0.5f, 0.95f, 0.75f), CreamCol, i * 60f);
            foreach (var p in new[] { new Vector3(58f, RuinBed, 46f), new Vector3(84f, RuinBed, 66f), new Vector3(66f, RuinBed, 92f), new Vector3(94f, RuinBed, 28f) })
                Bubbles(kit, p, 4.4f, 6);
            PointLight(kit, new Vector3(64f, -3f, 44f), new Color(0.3f, 1f, 0.7f), 24f, 1.4f);
            PointLight(kit, new Vector3(84f, -3f, 78f), new Color(0.3f, 1f, 0.7f), 24f, 1.4f);

            Temple(kit);
            Wreck(kit);
            StoneHead(kit, new Vector3(68f, -1.6f, 87f));

            // ----- Stepping stones: entry terrace -> temple -----
            Stump(kit, 56.6f, 65.2f, 0.5f, 1.5f, true);
            Stump(kit, 60.3f, 62.4f, 0.8f, 1.4f);
            Stump(kit, 63.6f, 59.4f, 0.5f, 1.5f, true);
            Trail(kit, 10, new Vector3(56.6f, 1.8f, 65.2f), new Vector3(60.3f, 2.1f, 62.4f), new Vector3(63.6f, 1.8f, 59.4f), new Vector3(66.6f, 2f, 58f));
            // ----- Temple -> south terrace: along the fallen column, then three more stumps -----
            var fallen = kit.Group("FallenColumn", new Vector3(78f, 0.1f, 43.4f));
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(2f, 6.2f, 2f), RuinStone, new Vector3(90f, 0f, 0f), fallen);
            for (float z = -5.5f; z <= 5.5f; z += 1.6f)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, z), new Vector3(2.1f, 0.06f, 2.1f), RuinDark, new Vector3(90f, 0f, 0f), fallen);
            var fcol = fallen.gameObject.AddComponent<BoxCollider>();
            fcol.center = new Vector3(0f, 0.1f, 0f);
            fcol.size = new Vector3(1.6f, 1.8f, 12.4f);
            Stump(kit, 76.4f, 34.4f, 0.8f, 1.5f);
            Stump(kit, 72.6f, 30.8f, 0.6f, 1.4f, true);
            Stump(kit, 68.8f, 27.2f, 0.4f, 1.5f);
            Trail(kit, 10, new Vector3(78f, 2.3f, 47f), new Vector3(78f, 2.3f, 43f), new Vector3(78f, 2.3f, 39f), new Vector3(76.4f, 2.1f, 34.4f), new Vector3(72.6f, 1.9f, 30.8f), new Vector3(68.8f, 1.7f, 27.2f));
            // More ruins standing about in the water, for the look of the place.
            RuinColumn(kit, new Vector3(52f, RuinBed, 44f), 12f, 1f, false);
            RuinColumn(kit, new Vector3(52f, RuinBed, 36f), 8f, 1f, true);
            RuinColumn(kit, new Vector3(58f, RuinBed, 78f), 13f, 1f, false);
            RuinColumn(kit, new Vector3(58f, RuinBed, 94f), 13f, 1f, false);
            RuinColumn(kit, new Vector3(80f, RuinBed, 94f), 9f, 1f, true);
            RuinColumn(kit, new Vector3(86f, RuinBed, 70f), 11f, 1f, false);
            RuinColumn(kit, new Vector3(94f, RuinBed, 64f), 7f, 1f, true);
            RuinColumn(kit, new Vector3(60f, RuinBed, 30f), 7.5f, 0.9f, true);
            Arc(kit, new Vector3(58f, 8.2f, 78f), new Vector3(58f, 8.2f, 94f), 4f, 0.9f, 1.4f, RuinStone, 14);
            // A walkway along the top of that arch's wall: Water Form's reward for riding the spring.
            kit.Block(new Vector3(58f, 7.95f, 86f), new Vector3(2.4f, 0.5f, 14f), StoneCol, "ArchTop", RuinStone);
            kit.Fountain(new Vector3(61.6f, RuinSurface, 82f), 9.6f, LevelKit.FountainStyle.Spring);
            SpringMouth(kit, new Vector3(61.6f, RuinSurface, 82f), RuinStone, 200f);
            kit.ThreadLine(new Vector3(58f, 9.4f, 81f), new Vector3(58f, 9.4f, 91f), 6);
            kit.Hint(new Vector3(52f, 1.5f, 70f), new Vector3(6f, 4f, 6f), "Springs in the Ruins", "Water Form can swim the whole hall. A spring by the tall arch to the north lifts you to the top of it.", HintTheme.Water);
            // Warm vents for Wind Form: one on the entry terrace, one on the temple.
            kit.Updraft(new Vector3(45.6f, 0f, 61f), 12f, LevelKit.UpdraftStyle.Steam);
            // The pump house on its rock in the north-west corner: only Water Form fits through the drain.
            PumpHouse(kit, new Vector3(50f, 0f, 92f));

            // ----- The entry terrace -----
            kit.AddCheckpoint(new Vector3(48f, 0f, 66f), 90f);
            Plaque(kit, new Vector3(51f, 0f, 71.4f), 180f, "Drowned Cities",
                "The city of Thonis-Heracleion was once the greatest port in Egypt. Then the ground beneath it turned to soup, and it slid into the sea and was forgotten for over a thousand years.",
                "Divers found it again in the year 2000, under ten metres of water and a blanket of sand: temples, statues, sixty-four ships, all still there.");
            kit.Hint(new Vector3(49f, 1.5f, 66f), new Vector3(8f, 4f, 10f), "The Sunken Ruins",
                "Hop the broken columns to the temple in the middle, then along the fallen column to the dry terrace at the south end. The Observatory lift is in the corner there.");
            HangingLantern(kit, new Vector3(49f, 9f, 66f), T, 18f, 1.2f);

            // ----- The south terrace and the lift -----
            kit.AddCheckpoint(new Vector3(62f, 0f, 15f), 270f);
            foreach (var p in new[] { new Vector3(60f, 10f, 14f), new Vector3(84f, 10f, 14f) }) HangingLantern(kit, p, T, 22f, 1.4f);
            Rug(kit, new Vector3(60f, 0f, 12f), new Vector2(16f, 5f), StarCloth, 0f, PatchworkAssets.Felt(new Color(0.4f, 0.85f, 0.7f)));
            Lift(kit);
            Vector3 amph = DisplayCase(kit, new Vector3(74f, 0f, 9f), new Vector3(3.4f, 2.6f, 2.2f));
            for (int i = 0; i < 3; i++)
            {
                Vector3 p = amph + new Vector3(-1f + i, 0f, 0f);
                var clay = PatchworkAssets.Felt(i == 1 ? new Color(0.3f, 0.5f, 0.55f) : new Color(0.78f, 0.5f, 0.36f));
                NoShadow(kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 0.5f, new Vector3(0.6f, 0.9f, 0.6f), clay));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 1f, new Vector3(0.22f, 0.2f, 0.22f), clay));
                NoShadow(kit.Deco(PrimitiveType.Sphere, p + new Vector3(0.1f, 0.3f, 0.26f), new Vector3(0.24f, 0.1f, 0.1f), PatchworkAssets.Felt(CreamCol)));
            }
            Plaque(kit, new Vector3(74f, 0f, 11.2f), 0f, "Amphorae",
                "The cardboard boxes of the ancient world: clay jars for carrying oil, wine, olives and fish sauce. The pointed bottom let them be stacked in the hold of a ship, row upon row.",
                "When divers find a wreck, the wood has usually rotted away. What's left is the cargo: a ship-shaped heap of jars on the sea bed.");
            Vector3 lh = Plinth(kit, new Vector3(90f, 0f, 10f), new Vector3(3f, 0.6f, 3f));
            for (int i = 0; i < 3; i++)
                kit.Deco(PrimitiveType.Cube, lh + Vector3.up * (0.9f + i * 1.7f), new Vector3(1.9f - i * 0.5f, 1.8f, 1.9f - i * 0.5f), RuinStone, new Vector3(0f, i == 1 ? 45f : 0f, 0f));
            NoShadow(kit.Deco(PrimitiveType.Sphere, lh + Vector3.up * 5.6f, Vector3.one * 0.8f, PatchworkAssets.Glow(new Color(1f, 0.85f, 0.5f), 3f)));
            PointLight(kit, lh + Vector3.up * 5.6f, LampGold, 14f, 1.4f);
            TownKit.Solid(kit, lh + Vector3.up * 2.6f, new Vector3(1.9f, 5.2f, 1.9f), 0f);
            Plaque(kit, new Vector3(90f, 0f, 12.6f), 0f, "The Lighthouse of Alexandria",
                "For sixteen hundred years the tallest lighthouse ever built stood at the mouth of Alexandria's harbour, with a fire at the top and a mirror to throw its light out to sea.",
                "Earthquakes brought it down in the end. In 1994 divers found its great stone blocks lying on the harbour floor, right where they fell.");
            Person(kit, new Vector3(66f, 0f, 20f), 20f, ToyNpc.Kind.JackInBox, "Dr. Trowel", new Color(0.75f, 0.6f, 0.35f), new Color(0.3f, 0.6f, 0.55f), 1.1f, () => new[]
            {
                "Careful! Every one of those stones is priceless. ...You hopped on all of them, didn't you.",
                "Never mind. They've had worse. They've had the whole sea sitting on them.",
                "The lift's that big cardboard box in the corner. Don't laugh, it's perfectly safe. It opens for three Heartstrings. There's one on the temple altar, if you're short."
            });
            foreach (float x in new[] { 56f, 68f, 96f }) Sconce(kit, new Vector3(x, 4.4f, 6f), 0f, x == 68f || x == 96f);
            Banner(kit, new Vector3(90f, 18f, 6.1f), 0f, 3.2f, 8f, StarCloth, 3);
            Banner(kit, new Vector3(74f, 18f, 6.1f), 0f, 3.2f, 8f, PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, Wine, new Color(1f, 0.82f, 0.4f)), 0);
            Banner(kit, new Vector3(66f, 17f, 6.1f), 0f, 3.2f, 9f, PlumStars, 1);
            TrailLine(kit, new Vector3(64f, 1.2f, 22f), new Vector3(53f, 1.2f, 12f), 6);
        }

        // The temple in the middle of the water: a stepped platform, a ring of columns (half of
        // them fallen), and the altar with its Heartstring.
        static void Temple(LevelKit kit)
        {
            Color[] pale = { new Color(0.8f, 0.82f, 0.76f), new Color(0.72f, 0.78f, 0.72f), new Color(0.86f, 0.86f, 0.8f), new Color(0.66f, 0.74f, 0.7f) };
            kit.Block(new Vector3(75f, -2.3f, 58f), new Vector3(15f, 5.4f, 17f), StoneCol, "TempleBase", RuinDark);
            Floor(kit, 68f, 82f, 50f, 66f, 1.2f, pale, 3.5f, "TempleFloor");
            kit.Deco(PrimitiveType.Cube, new Vector3(75f, 0.5f, 58f), new Vector3(15.6f, 0.4f, 17.6f), RuinStone);
            // Steps up out of the water on the west side.
            Steps(kit, new Vector3(64.4f, -0.3f, 58f), 90f, 6, 0.25f, 0.6f, 6f, RuinStone, RuinBed);
            float[] heights = { 9f, 9f, 4.5f, 9f, 2.6f, 9f, 6f, 9f };
            Vector3[] at = { new Vector3(69.2f, 1.2f, 51.2f), new Vector3(75f, 1.2f, 51.2f), new Vector3(80.8f, 1.2f, 51.2f), new Vector3(80.8f, 1.2f, 58f),
                             new Vector3(80.8f, 1.2f, 64.8f), new Vector3(75f, 1.2f, 64.8f), new Vector3(69.2f, 1.2f, 64.8f), new Vector3(69.2f, 1.2f, 61.4f) };
            for (int i = 0; i < at.Length; i++) RuinColumn(kit, at[i], heights[i], 0.7f, heights[i] < 9f);
            // What's left of the roof: two beams still up.
            kit.Deco(PrimitiveType.Cube, new Vector3(72.1f, 10.9f, 51.2f), new Vector3(7.6f, 1f, 1.8f), RuinStone);
            kit.Deco(PrimitiveType.Cube, new Vector3(69.2f, 10.9f, 63.1f), new Vector3(1.8f, 1f, 5.2f), RuinStone);
            kit.Deco(PrimitiveType.Cube, new Vector3(79f, 1.9f, 55f), new Vector3(1.6f, 1.2f, 5f), RuinStone, new Vector3(0f, 24f, 8f));
            TownKit.Solid(kit, new Vector3(79f, 1.8f, 55f), new Vector3(1.6f, 1.2f, 5f), 24f);
            // The altar.
            Vector3 altar = Plinth(kit, new Vector3(75f, 1.2f, 58f), new Vector3(3f, 1f, 2f), 0f, RuinStone);
            Level1Builder.MakeHeartstring(kit, "L2_hs_temple", "The Drowned Temple", altar + Vector3.up * 1.6f, false);
            for (int s = -1; s <= 1; s += 2)
            {
                // Braziers either side: the only warm light in the hall.
                Vector3 b = new Vector3(75f + s * 3.4f, 1.2f, 58f);
                kit.Deco(PrimitiveType.Cylinder, b + Vector3.up * 0.6f, new Vector3(0.24f, 0.6f, 0.24f), IronMat);
                kit.Deco(PrimitiveType.Sphere, b + Vector3.up * 1.3f, new Vector3(1f, 0.5f, 1f), IronMat);
                NoShadow(kit.Deco(PrimitiveType.Sphere, b + Vector3.up * 1.6f, new Vector3(0.7f, 0.8f, 0.7f), PatchworkAssets.Glow(new Color(1f, 0.6f, 0.25f), 3f)));
                PointLight(kit, b + Vector3.up * 2f, new Color(1f, 0.65f, 0.35f), 13f, 1.6f);
                TownKit.Solid(kit, b + Vector3.up * 0.8f, new Vector3(0.6f, 1.6f, 0.6f), 0f);
            }
            Plaque(kit, new Vector3(71.6f, 1.2f, 54.4f), 200f, "Saved by the Sea",
                "On land, old bronze statues were melted down for coins and old marble was burned to make lime. Almost none survived. The ones we still have mostly spent two thousand years on the sea bed, where nobody could get at them.",
                "Two of the finest were spotted in 1972 by a man out snorkelling on his holiday. He saw an arm sticking out of the sand.");
            kit.Updraft(new Vector3(78.6f, 1.2f, 62.6f), 12f, LevelKit.UpdraftStyle.Steam);
            // Moss and weed on the stones.
            var moss = PatchworkAssets.Felt(MossCol);
            foreach (var p in new[] { new Vector3(70f, 1.22f, 52.6f), new Vector3(80f, 1.22f, 60f), new Vector3(73f, 1.22f, 64f), new Vector3(77.6f, 1.22f, 52f) })
                NoShadow(kit.Deco(PrimitiveType.Sphere, p, new Vector3(1.8f, 0.08f, 1.3f), moss, new Vector3(0f, p.x * 31f, 0f)));
        }

        // A wrecked ship lying against the east wall: a main deck, a quarterdeck, and a mast
        // with a crow's nest. The way over from the temple is a stump, or a swing on the lantern.
        static void Wreck(LevelKit kit)
        {
            var hull = PatchworkAssets.Planks(new Color(0.36f, 0.3f, 0.26f));
            var deck = PatchworkAssets.Planks(new Color(0.55f, 0.46f, 0.36f));
            // Decks (solid) and the hull under them.
            kit.Block(new Vector3(92f, 2f, 45f), new Vector3(10f, 0.4f, 14f), WoodCol, "WreckDeck", deck);
            kit.Block(new Vector3(92f, 4.2f, 35f), new Vector3(10f, 0.4f, 6f), WoodCol, "WreckQuarterdeck", deck);
            kit.Block(new Vector3(92f, 1f, 35f), new Vector3(10f, 6f, 6f), WoodCol, "WreckStern", hull);
            kit.Block(new Vector3(92f, -1.6f, 45f), new Vector3(9.4f, 6.8f, 14f), WoodCol, "WreckHull", hull);
            kit.Deco(PrimitiveType.Cube, new Vector3(92f, -0.6f, 54.6f), new Vector3(6.6f, 5f, 5.4f), hull, new Vector3(14f, 0f, 0f));
            kit.Deco(PrimitiveType.Cube, new Vector3(92f, 0.2f, 58.6f), new Vector3(3f, 3.6f, 4f), hull, new Vector3(26f, 0f, 0f));
            kit.Deco(PrimitiveType.Cube, new Vector3(92f, 1.9f, 61f), new Vector3(0.5f, 0.5f, 6f), hull, new Vector3(-24f, 0f, 0f));
            // Rails with gaps in them, and a few ribs showing where the planking has gone.
            foreach (float x in new[] { 87.1f, 96.9f })
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(x, 2.7f, 41f), new Vector3(0.2f, 1f, 5f), hull);
                kit.Deco(PrimitiveType.Cube, new Vector3(x, 4.9f, 35f), new Vector3(0.2f, 1f, 6f), hull);
                for (int i = 0; i < 4; i++)
                    kit.Deco(PrimitiveType.Cube, new Vector3(x + (x < 92f ? -0.2f : 0.2f), 1f, 40f + i * 3.4f), new Vector3(0.3f, 3.6f, 0.3f), hull, new Vector3(0f, 0f, x < 92f ? 8f : -8f));
            }
            // Two crates as steps up to the quarterdeck.
            kit.Block(new Vector3(89f, 2.75f, 39.2f), new Vector3(1.6f, 1.1f, 1.6f), WoodCol, "Crate", PatchworkAssets.Planks(new Color(0.7f, 0.56f, 0.4f)));
            kit.Block(new Vector3(95f, 2.75f, 39.2f), new Vector3(1.6f, 1.1f, 1.6f), WoodCol, "Crate", PatchworkAssets.Planks(new Color(0.7f, 0.56f, 0.4f)));
            // The mast, leaning a little, the rags of a sail, and little round steps up it to the nest.
            Vector3 mast = new Vector3(92f, 2.2f, 45f);
            kit.Deco(PrimitiveType.Cylinder, mast + Vector3.up * 7.4f, new Vector3(0.5f, 7.4f, 0.5f), DarkWood);
            TownKit.Solid(kit, mast + Vector3.up * 5f, new Vector3(0.5f, 10f, 0.5f), 0f);
            kit.Deco(PrimitiveType.Cylinder, mast + Vector3.up * 8f, new Vector3(0.22f, 4.4f, 0.22f), DarkWood, new Vector3(0f, 0f, 84f));
            NoShadow(kit.Deco(PrimitiveType.Cube, mast + new Vector3(-2f, 6.3f, 0.1f), new Vector3(3.2f, 3.2f, 0.05f), PatchworkAssets.Felt(new Color(0.8f, 0.78f, 0.66f)), new Vector3(0f, 0f, 5f)));
            NoShadow(kit.Deco(PrimitiveType.Cube, mast + new Vector3(2.4f, 6.9f, 0.1f), new Vector3(2f, 2f, 0.05f), PatchworkAssets.Felt(new Color(0.74f, 0.72f, 0.6f)), new Vector3(0f, 0f, -12f)));
            float[] stepY = { 5.8f, 7.4f, 9f };
            for (int i = 0; i < 3; i++)
            {
                float a = (200f + i * 105f) * Mathf.Deg2Rad;
                Vector3 p = new Vector3(92f + Mathf.Cos(a) * 1.9f, stepY[i], 45f + Mathf.Sin(a) * 1.9f);
                // (The first one is nearest the quarterdeck.)
                if (i == 0) p = new Vector3(92f, stepY[i], 40.4f);
                if (i == 2) p = new Vector3(92f + Mathf.Cos(a) * 2.7f, stepY[i], 45f + Mathf.Sin(a) * 2.7f);
                RoundPlatform(kit, p, 1.15f, 0.3f, PatchworkAssets.Planks(new Color(0.7f, 0.56f, 0.4f)), DarkWood, false);
                ScoreBubbles.Place(kit, p + Vector3.up * 1.2f, 10);
            }
            RoundPlatform(kit, mast + Vector3.up * 8.4f, 1.9f, 0.4f, deck, DarkWood, false);
            for (int i = 0; i < 10; i++)
            {
                float a = i / 10f * Mathf.PI * 2f;
                kit.Deco(PrimitiveType.Cube, mast + new Vector3(Mathf.Cos(a) * 1.85f, 8.9f, Mathf.Sin(a) * 1.85f), new Vector3(0.12f, 1f, 0.12f), DarkWood);
            }
            var hoop = kit.Group("NestRail", mast + Vector3.up * 9.4f);
            Hoop(kit, hoop, 1.85f, 0.14f, DarkWood, 16);
            kit.GoldenButton("L2_b_crowsnest", mast + new Vector3(1.05f, 9.9f, 0f));
            // From the temple: one stump to hop, or the lantern to swing on.
            Stump(kit, 84.5f, 52f, 1.7f, 1.3f, true);
            kit.Deco(PrimitiveType.Cube, new Vector3(85f, RuinTop - 1f, 47f), new Vector3(0.5f, 0.5f, 5f), IronMat);
            kit.Anchor(new Vector3(85f, 8.4f, 47f), 13.5f, LevelKit.AnchorStyle.Lantern, RuinTop - 1.3f - 8.4f - 0.6f);
            Level2Light(kit, new Vector3(92f, 6f, 44f));
            Plaque(kit, new Vector3(89.4f, 2.2f, 49.6f), 220f, "The Vasa",
                "In 1628 the grandest warship in Sweden set out on her very first voyage, to cheers and cannon salutes. She had sailed about as far as you could throw a ball twenty times when a puff of wind tipped her over, and down she went.",
                "She had too many guns up top and not enough weight below. She lay in the cold mud for 333 years, was lifted out nearly whole in 1961, and you can go and see her today.");
            kit.Hint(new Vector3(88.5f, 3.5f, 44f), new Vector3(6f, 4f, 8f), "The Crow's Nest", "Up the crates to the quarterdeck, then the little round steps up the mast. Wind Form can ride the temple's vent and glide across instead.");
            // Barrels and a chest on deck.
            foreach (var p in new[] { new Vector3(95.4f, 2.2f, 49.6f), new Vector3(94.6f, 2.2f, 50.8f), new Vector3(89f, 4.4f, 33.4f) })
            {
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.55f, new Vector3(0.9f, 0.55f, 0.9f), PatchworkAssets.Planks(new Color(0.6f, 0.42f, 0.28f)));
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.55f, new Vector3(0.96f, 0.05f, 0.96f), Brass);
                TownKit.Solid(kit, p + Vector3.up * 0.55f, new Vector3(0.8f, 1.1f, 0.8f), 0f);
            }
            kit.ThreadLine(new Vector3(90f, 5.6f, 33.4f), new Vector3(95.4f, 5.6f, 36.4f), 4);
        }

        static void Level2Light(LevelKit kit, Vector3 p)
        {
            PointLight(kit, p, new Color(0.5f, 1f, 0.85f), 16f, 1.2f);
        }

        // The great stone face, tipped over and half under water, asleep.
        static void StoneHead(LevelKit kit, Vector3 c)
        {
            var stone = RuinStone;
            var g = kit.Group("StoneHead", c);
            g.rotation = Quaternion.Euler(-18f, 150f, 24f);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(8f, 10f, 8.6f), stone, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.3f, 4.2f), new Vector3(1.1f, 2.6f, 1.2f), stone, new Vector3(-12f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -1.5f, 4.3f), new Vector3(1.7f, 0.8f, 1f), stone, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.3f, 3.9f), new Vector3(5.4f, 0.5f, 0.8f), stone, default(Vector3), g);
            var line = RuinDark;
            for (int s = -1; s <= 1; s += 2)
            {
                // Closed eyes and the corners of a small smile.
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 1.6f, 0.6f, 3.7f), new Vector3(1.7f, 0.8f, 0.7f), stone, default(Vector3), g);
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 1.6f, 0.45f, 4.06f), new Vector3(1.4f, 0.1f, 0.1f), line, new Vector3(0f, 0f, s * -6f), g));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.7f, -2.5f, 3.8f), new Vector3(0.9f, 0.1f, 0.1f), line, new Vector3(0f, s * 14f, s * 10f), g));
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 4f, 0f, 0f), new Vector3(0.8f, 2.4f, 1.6f), stone, default(Vector3), g);
            }
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, -2.6f, 4.05f), new Vector3(1f, 0.1f, 0.1f), line, default(Vector3), g));
            // Curls.
            for (int i = 0; i < 16; i++)
            {
                float a = i / 16f * Mathf.PI * 2f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 3.3f, 3.6f + Mathf.Sin(i * 2.4f) * 0.5f, Mathf.Sin(a) * 3.5f), Vector3.one * 2f, stone, default(Vector3), g);
            }
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 4.6f, 0f), new Vector3(5f, 2.4f, 5f), stone, default(Vector3), g);
            // Moss on the upward cheek, and a button for a beauty spot.
            var moss = PatchworkAssets.Felt(MossCol);
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(-2.8f, 1.6f, 2.4f), new Vector3(2.6f, 1.8f, 1.4f), moss, default(Vector3), g));
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(-3.6f, -1f, 1f), new Vector3(1.4f, 2.2f, 1.6f), moss, default(Vector3), g));
            var col = g.gameObject.AddComponent<SphereCollider>();
            col.radius = 4.4f;
            PointLight(kit, c + new Vector3(4f, 4f, -6f), new Color(0.5f, 1f, 0.8f), 16f, 1.3f);
            ScoreBubbles.Place(kit, c + new Vector3(-1f, 6f, -0.6f), 50);
        }

        // A little stone pump house on a rock, with a drain for a door.
        static void PumpHouse(LevelKit kit, Vector3 foot)
        {
            // The rock: a low shelf to climb out onto, and the hut on top of it.
            kit.Block(foot + new Vector3(0f, -2.45f, 0f), new Vector3(9f, 5.1f, 9f), StoneCol, "PumpRock", RuinDark);
            Vector3 f = foot + Vector3.up * 0.1f;
            var wall = RuinStone;
            kit.Block(f + new Vector3(0f, 1.5f, 2.2f), new Vector3(5.4f, 3f, 0.4f), StoneCol, "PumpWall", wall);
            kit.Block(f + new Vector3(-2.5f, 1.5f, 0f), new Vector3(0.4f, 3f, 4.8f), StoneCol, "PumpWall", wall);
            kit.Block(f + new Vector3(2.5f, 1.5f, 0f), new Vector3(0.4f, 3f, 4.8f), StoneCol, "PumpWall", wall);
            // Front wall with the drain in the middle of it.
            kit.Block(f + new Vector3(-1.75f, 1.5f, -2.2f), new Vector3(1.9f, 3f, 0.4f), StoneCol, "PumpWall", wall);
            kit.Block(f + new Vector3(1.75f, 1.5f, -2.2f), new Vector3(1.9f, 3f, 0.4f), StoneCol, "PumpWall", wall);
            kit.Block(f + new Vector3(0f, 2.2f, -2.2f), new Vector3(1.6f, 1.6f, 0.4f), StoneCol, "PumpWall", wall);
            kit.Grate(f + new Vector3(0f, 0.7f, -2.2f), new Vector3(1.6f, 1.4f, 0.3f));
            kit.Block(f + new Vector3(0f, 3.2f, 0f), new Vector3(6f, 0.4f, 5.4f), StoneCol, "PumpRoof", RuinDark);
            // Inside: a brass pump, a lamp, and the marble.
            kit.Deco(PrimitiveType.Cylinder, f + new Vector3(1.3f, 0.7f, 1.2f), new Vector3(0.9f, 0.7f, 0.9f), Brass);
            kit.Deco(PrimitiveType.Sphere, f + new Vector3(1.3f, 1.5f, 1.2f), Vector3.one * 0.7f, Brass);
            kit.Deco(PrimitiveType.Cylinder, f + new Vector3(1.3f, 2.2f, 1.2f), new Vector3(0.16f, 0.9f, 0.16f), Brass);
            PointLight(kit, f + new Vector3(0f, 2.4f, 0f), new Color(0.5f, 0.9f, 1f), 7f, 1.6f);
            SecretMarble.Place(kit, f + new Vector3(-1f, 1f, 0.6f), "L2_marble_seabed", new Color(0.5f, 0.9f, 0.5f));
            kit.Hint(foot + new Vector3(0f, 1.5f, -4.6f), new Vector3(7f, 4f, 3f), "The Pump House", "Only Water Form can slip through the drain.", HintTheme.Water);
        }

        // ================================================================ The Orrery Bridge

        static void OrreryBridge(LevelKit kit)
        {
            var stone = Sandstone;
            const float x0 = -6f, x1 = 42f, z0 = 6f, z1 = 18f, low = 10f, top = 30f;
            // Walls: the north one has a long window onto the Great Deep.
            Wall(kit, new Vector3(-8f, 0f, 19f), new Vector3(42f, 0f, 19f), -8f, DeckY + 0.9f, 2f, stone);
            Wall(kit, new Vector3(-8f, 0f, 19f), new Vector3(42f, 0f, 19f), DeckY + 7.9f, top, 2f, stone);
            Wall(kit, new Vector3(-8f, 0f, 19f), new Vector3(3f, 0f, 19f), DeckY + 0.9f, DeckY + 7.9f, 2f, stone);
            Wall(kit, new Vector3(39f, 0f, 19f), new Vector3(42f, 0f, 19f), DeckY + 0.9f, DeckY + 7.9f, 2f, stone);
            GlassWall(kit, new Vector3(21f, DeckY + 0.9f, 19f), 180f, 36f, 7f, 8, 2);
            // The south wall, with the doorway in from the Star Corridor near its east end.
            Wall(kit, new Vector3(8f, 0f, 5f), new Vector3(34f, 0f, 5f), 0f, top, 2f, stone);
            Wall(kit, new Vector3(38f, 0f, 5f), new Vector3(42f, 0f, 5f), 0f, top, 2f, stone);
            Wall(kit, new Vector3(34f, 0f, 5f), new Vector3(38f, 0f, 5f), 0f, DeckY, 2f, stone);
            Wall(kit, new Vector3(34f, 0f, 5f), new Vector3(38f, 0f, 5f), DeckY + 5.5f, top, 2f, stone);
            ArchTrim(kit, new Vector3(36f, DeckY, 5f), 0f, 4f, 5.5f, 2f, stone, PaleStone);
            Wall(kit, new Vector3(-7f, 0f, 3f), new Vector3(-7f, 0f, 20f), 0f, top, 2f, stone);
            kit.Block(new Vector3(18f, top + 0.5f, 12f), new Vector3(52f, 1f, 16f), Navy, "BridgeCeiling", PatchworkAssets.Felt(new Color(0.06f, 0.06f, 0.2f)));
            PaintedStars(kit, new Vector3(18f, top - 0.06f, 12f), 46f, 11f, 120, 9);
            // Floors: the starry lower floor, and a landing at each end of the gap.
            kit.Block(new Vector3(18f, low * 0.5f - 0.5f, 12f), new Vector3(50f, low - 1f, 14f), StoneCol, "BridgeBase", DarkStone);
            Floor(kit, x0 - 1f, x1, z0 - 0.4f, z1, low, FloorQuilt, 3f, "BridgeLower");
            Floor(kit, 28f, 44.2f, z0 - 0.4f, z1, DeckY, WarmQuilt, 3f, "BridgeEast");
            kit.Block(new Vector3(36.1f, (low + DeckY - 1f) * 0.5f, 12f), new Vector3(16.2f, DeckY - 1f - low, 12.2f), StoneCol, "BridgeEastBase", stone);
            Floor(kit, x0 - 1f, 14f, z0 - 0.4f, z1, DeckY, WarmQuilt, 3f, "BridgeWest");
            kit.Block(new Vector3(3.5f, (low + DeckY - 1f) * 0.5f, 12f), new Vector3(21f, DeckY - 1f - low, 12.2f), StoneCol, "BridgeWestBase", stone);
            // Stairs up from the lower floor: one flight along each wall.
            Steps(kit, new Vector3(20f, low, 7.2f), 90f, 16, 0.25f, 0.5f, 2.4f, PaleStone, low - 0.5f);
            Steps(kit, new Vector3(22f, low, 16.8f), 270f, 16, 0.25f, 0.5f, 2.4f, PaleStone, low - 0.5f);
            Balustrade(kit, new Vector3(28f, DeckY, 8.6f), new Vector3(28f, DeckY, 10.4f));
            Balustrade(kit, new Vector3(28f, DeckY, 14.8f), new Vector3(28f, DeckY, 17.7f));
            Balustrade(kit, new Vector3(14f, DeckY, 6f), new Vector3(14f, DeckY, 10.4f));
            Balustrade(kit, new Vector3(14f, DeckY, 14.8f), new Vector3(14f, DeckY, 15.4f));

            // ----- The orrery -----
            Vector3 c = new Vector3(21f, DeckY, 12.6f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(c.x, (low + DeckY) * 0.5f, c.z), new Vector3(1f, (DeckY - low) * 0.5f, 1f), Brass);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(c.x, low + 0.2f, c.z), new Vector3(3f, 0.2f, 3f), Brass);
            // The hub: a little fixed platform round the column, with the sun over it.
            RoundPlatform(kit, c, 1.25f, 0.4f, PatchworkAssets.Felt(new Color(1f, 0.85f, 0.45f)), Brass, false);
            kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 4.6f, new Vector3(0.14f, 0.9f, 0.14f), Brass);
            NoShadow(kit.Deco(PrimitiveType.Sphere, c + Vector3.up * 6.4f, Vector3.one * 2.4f, PatchworkAssets.Glow(new Color(1f, 0.8f, 0.4f), 2.6f)));
            PointLight(kit, c + Vector3.up * 5f, new Color(1f, 0.82f, 0.5f), 22f, 1.8f);
            Beam(kit, c + Vector3.up * 7.4f, new Vector3(c.x, top, c.z), 0.1f, Brass);
            Level1Builder.MakeHeartstring(kit, "L2_hs_orrery", "The Orrery", c + Vector3.up * 2.2f, false);
            // The arms: three planets to stand on, going slowly round.
            var arms = kit.Group("OrreryArms", c);
            Color[] planet = { new Color(0.4f, 0.7f, 1f), new Color(1f, 0.55f, 0.35f), new Color(0.75f, 0.6f, 1f) };
            for (int i = 0; i < 3; i++)
            {
                float a = i * 120f * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var p = RoundPlatform(kit, c + dir * 3.75f, 1.55f, 0.36f, i == 1 ? PlumStars : StarCloth, Brass);
                p.SetParent(arms, true);
                var arm = kit.Deco(PrimitiveType.Cube, c + dir * 1.9f + Vector3.down * 0.5f, new Vector3(0.2f, 0.2f, 3.8f), Brass);
                arm.rotation = Quaternion.LookRotation(dir, Vector3.up);
                arm.SetParent(arms, true);
                var ball = NoShadow(kit.Deco(PrimitiveType.Sphere, c + dir * 3.75f + Vector3.down * 1.5f, Vector3.one * (1.5f - i * 0.2f), PatchworkAssets.Glow(planet[i], 0.9f)));
                ball.SetParent(arms, true);
                if (i == 2)
                {
                    // One of them has a ring.
                    var ring = kit.Group("PlanetRing", c + dir * 3.75f + Vector3.down * 1.5f);
                    ring.rotation = Quaternion.Euler(20f, 0f, 12f);
                    Hoop(kit, ring, 1f, 0.1f, Brass, 14);
                    ring.SetParent(arms, true);
                }
            }
            var spin = arms.gameObject.AddComponent<Spinner>();
            spin.Speed = 16f;
            arms.gameObject.AddComponent<Carrier>();
            var body = arms.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            // The track the planets run on, drawn on the lower floor.
            var track = kit.Group("OrreryTrack", new Vector3(c.x, low + 0.06f, c.z));
            Hoop(kit, track, 3.75f, 0.12f, Brass, 30);
            var outer = kit.Group("OrreryTrackOuter", new Vector3(c.x, low + 0.06f, c.z));
            Hoop(kit, outer, 5.3f, 0.08f, Brass, 36);

            // ----- Along the way -----
            kit.AddCheckpoint(new Vector3(39f, DeckY, 12f), 270f);
            Trail(kit, 10, new Vector3(40f, DeckY + 1.2f, 11f), new Vector3(36f, DeckY + 1.2f, 12f), new Vector3(32f, DeckY + 1.2f, 12.6f));
            Trail(kit, 10, new Vector3(11f, DeckY + 1.2f, 12.6f), new Vector3(7f, DeckY + 1.2f, 11.6f), new Vector3(3f, DeckY + 1.2f, 10f), new Vector3(0f, DeckY + 1.2f, 7.6f));
            kit.ThreadLine(new Vector3(15.5f, low + 1.3f, 10.2f), new Vector3(26.5f, low + 1.3f, 10.2f), 6);
            kit.Hint(new Vector3(36f, DeckY + 1.5f, 12f), new Vector3(10f, 4f, 10f), "The Orrery Bridge",
                "Step onto a planet as it comes round, ride it to the hub in the middle, and take another across to the far side. If you fall, the stairs are at either end of the floor below.");
            Plaque(kit, new Vector3(33f, DeckY, 16.4f), 180f, "The Orrery",
                "A clockwork model of the sun and its planets. Turn the handle and they all go round at the right speeds: years in seconds.",
                "It's named after the Earl of Orrery, who had one made in about 1704. The clockmaker who actually invented it, George Graham, got rather less of the credit.");
            foreach (float x in new[] { 34f, 6f }) HangingLantern(kit, new Vector3(x, DeckY + 8f, 12f), top, 18f, 1.2f);
            foreach (float x in new[] { 12f, 22f, 32f })
                NightWindow(kit, new Vector3(x, low + 1.4f, 6f), 0f, 2.6f, 6f, new Color(0.3f, 0.3f, 0.8f), 0.8f);
            Banner(kit, new Vector3(38f, DeckY + 12f, 6.1f), 0f, 3f, 8f, StarCloth, 1);
            Banner(kit, new Vector3(4f, DeckY + 12f, 18f - 0.1f), 180f, 3f, 8f, PlumStars, 0);
        }

        // ================================================================ The Observatory

        static void ObservatoryHall(LevelKit kit)
        {
            Vector3 C = Observatory;
            const float R = ObsRadius;
            var stone = Sandstone;
            // ----- The deck: a great drum standing on the sea floor -----
            const int sides = 24;
            var corners = new Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float a = (i + 0.5f) / sides * Mathf.PI * 2f;
                corners[i] = new Vector2(C.x + Mathf.Cos(a) * (R + 0.9f), C.z + Mathf.Sin(a) * (R + 0.9f));
            }
            kit.Prism(corners, -1f, DeckY, PatchworkAssets.Fabric(new Color(0.14f, 0.16f, 0.36f)), "ObservatoryDeck", 3f);
            // The star map on the floor: rings of cloth, brass circles and a compass star.
            NoShadow(kit.Deco(PrimitiveType.Cylinder, C + Vector3.up * 0.02f, new Vector3(R * 1.84f, 0.02f, R * 1.84f), StarCloth));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, C + Vector3.up * 0.03f, new Vector3(R * 1.3f, 0.02f, R * 1.3f), PlumStars));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, C + Vector3.up * 0.04f, new Vector3(R * 0.8f, 0.02f, R * 0.8f), StarCloth));
            var gold = PatchworkAssets.Felt(new Color(1f, 0.82f, 0.4f));
            foreach (float rr in new[] { R * 0.92f, R * 0.65f, R * 0.4f })
            {
                var ring = kit.Group("FloorRing", C + Vector3.up * 0.07f);
                Hoop(kit, ring, rr, 0.12f, Brass, 48);
            }
            for (int i = 0; i < 8; i++)
                NoShadow(kit.Deco(PrimitiveType.Cube, C + Vector3.up * 0.055f, new Vector3(i % 2 == 0 ? 0.9f : 0.5f, 0.02f, i % 2 == 0 ? R * 1.28f : R * 0.8f), gold, new Vector3(0f, i * 22.5f, 0f)));

            // ----- The wall: stone below, glass between, stone above, a column at every joint -----
            float segW = 2f * R * Mathf.Tan(Mathf.PI / sides) + 0.06f;
            for (int i = 0; i < sides; i++)
            {
                float deg = i * 360f / sides;
                float a = deg * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var rot = Quaternion.LookRotation(dir, Vector3.up);
                bool door = i == 6;                           // north: the way in from the bridge
                bool overlook = i >= 17 && i <= 19;           // south: the balcony over the atrium
                Vector3 jointDir = new Vector3(Mathf.Cos(a + Mathf.PI / sides), 0f, Mathf.Sin(a + Mathf.PI / sides));
                if (!(overlook && i < 19))
                    Column(kit, C + jointDir * (R - 0.5f), 9f, 0.45f);
                if (overlook) continue;
                if (door)
                {
                    var lintel = kit.Block(C + dir * R + Vector3.up * 7.75f, new Vector3(segW, 2.5f, 1f), StoneCol, "ObsLintel", stone);
                    lintel.rotation = rot;
                    continue;
                }
                var dado = kit.Block(C + dir * R + Vector3.up * 0.75f, new Vector3(segW, 1.5f, 1f), StoneCol, "ObsWall", stone);
                dado.rotation = rot;
                var glass = kit.Block(C + dir * R + Vector3.up * 4.5f, new Vector3(segW, 6f, 0.3f), Color.white, "ObsGlass", GlassMat);
                glass.rotation = rot;
                NoShadow(glass);
                var frieze = kit.Block(C + dir * R + Vector3.up * 8.25f, new Vector3(segW, 1.5f, 1f), StoneCol, "ObsWall", PaleStone);
                frieze.rotation = rot;
                // Lead bars across the glass, and a lamp on every third pier.
                NoShadow(kit.Deco(PrimitiveType.Cube, C + dir * R + Vector3.up * 4.5f, new Vector3(0.14f, 6f, 0.4f), IronMat, new Vector3(0f, 90f - deg, 0f)));
                NoShadow(kit.Deco(PrimitiveType.Cube, C + dir * R + Vector3.up * 5.5f, new Vector3(segW, 0.14f, 0.4f), IronMat, new Vector3(0f, 90f - deg, 0f)));
                if (i % 3 == 0) Sconce(kit, C + jointDir * (R - 1.1f) + Vector3.up * 4f, 270f - (deg + 180f / sides), true, 15f);
            }
            // A stone ring on top of the wall for the dome to sit on.
            var ringTop = kit.Group("DomeRing", C + Vector3.up * 9.3f);
            Hoop(kit, ringTop, R, 0.7f, PaleStone, 48);

            // ----- The dome, and the sea beyond it -----
            Vector3 domeBase = C + Vector3.up * 9.4f;
            Dome(kit, domeBase, new Vector3(R, 15f, R), DomeGlass(new Color(0.55f, 0.8f, 1f, 0.09f)), false, "ObservatoryDome", 10, 36);
            for (int i = 0; i < 12; i++)
            {
                float az = i / 12f * Mathf.PI * 2f;
                Vector3 prev = domeBase + new Vector3(Mathf.Cos(az) * R, 0f, Mathf.Sin(az) * R);
                for (int k = 1; k <= 9; k++)
                {
                    float el = k / 9f * Mathf.PI * 0.5f * 0.94f;
                    Vector3 p = domeBase + new Vector3(Mathf.Cos(az) * Mathf.Cos(el) * R, Mathf.Sin(el) * 15f, Mathf.Sin(az) * Mathf.Cos(el) * R);
                    Beam(kit, prev, p, 0.3f, Brass);
                    prev = p;
                }
            }
            foreach (float el in new[] { 30f, 60f })
            {
                var hoop = kit.Group("DomeHoop", domeBase + Vector3.up * Mathf.Sin(el * Mathf.Deg2Rad) * 15f);
                Hoop(kit, hoop, Mathf.Cos(el * Mathf.Deg2Rad) * R, 0.22f, Brass, 40);
            }
            var oculus = kit.Group("Oculus", domeBase + Vector3.up * 14.9f);
            Hoop(kit, oculus, 2.5f, 0.4f, Brass, 18);
            // The sea outside: a tall shell of deep blue, lighter towards the top, left open on
            // the south side where the balcony looks out over the atrium.
            Dome(kit, C + Vector3.down * 6f, new Vector3(R + 4.6f, 62f, R + 4.6f), Backdrop(new Color(0.01f, 0.04f, 0.16f), new Color(0.16f, 0.5f, 0.85f), 0.95f), true, "ObservatorySea", 14, 40, -38f, 218f);
            // The whale, going round and round over the glass.
            var whale = Swim(kit, Swimmer.Whale, C + Vector3.up * 30f, 12.5f, 12.5f, 6.5f, 1.05f, new Color(0.16f, 0.24f, 0.5f), new Color(0.75f, 0.85f, 0.95f), 200f);
            whale.Bob = 1.2f;
            var stars = new MeshBatch();
            var srng = new System.Random(9);
            for (int i = 0; i < 22; i++)
            {
                float a = (float)srng.NextDouble() * Mathf.PI - Mathf.PI * 0.5f, z = ((float)srng.NextDouble() - 0.45f) * 7f;
                float wide = 1.2f * Mathf.Sqrt(Mathf.Max(0.05f, 1f - (z / 4.6f) * (z / 4.6f)));
                stars.Ball(new Vector3(Mathf.Sin(a) * wide, 0.14f + Mathf.Cos(a) * wide * 0.92f, z), Vector3.one * (0.1f + (float)srng.NextDouble() * 0.14f), 2, 5);
            }
            stars.Build(whale.transform, "WhaleStars", PatchworkAssets.Glow(new Color(1f, 0.9f, 0.55f), 1.8f));
            Swim(kit, Swimmer.Ray, C + Vector3.up * 24.5f, 17f, 17f, -9f, 1.5f, new Color(0.2f, 0.24f, 0.46f), CreamCol, 60f);
            Shoal(kit, C + Vector3.up * 27f, 14f, 14f, 13f, 26, 0.4f, new Color(0.75f, 0.88f, 1f), 91, 300f);
            // Jellyfish drifting past the windows.
            Color[] jc = { new Color(1f, 0.55f, 0.85f), new Color(0.55f, 0.75f, 1f), new Color(0.8f, 0.6f, 1f), new Color(0.5f, 1f, 0.9f) };
            for (int i = 0; i < 20; i++)
            {
                float deg = -30f + i * 12.6f;
                if (deg > 208f) break;
                float a = deg * Mathf.Deg2Rad;
                Jelly(kit, C + new Vector3(Mathf.Cos(a) * (R + 2.4f), 2.6f + (i % 4) * 1.5f, Mathf.Sin(a) * (R + 2.4f)), 0.7f + (i % 3) * 0.25f, jc[i % jc.Length], i * 1.7f, i % 5 == 0);
            }

            // ----- The great telescope -----
            RoundPlatform(kit, C + Vector3.up * 0.5f, 6.4f, 0.5f, PlumStars, PaleStone, false);
            RoundPlatform(kit, C + Vector3.up * 1f, 4.4f, 0.5f, StarCloth, PaleStone, true);
            Vector3 mount = C + new Vector3(0f, 1f, 0.6f);
            kit.Block(mount + Vector3.up * 0.4f, new Vector3(2.2f, 0.8f, 2.2f), StoneCol, "TelescopeBase", DarkWood);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, mount + new Vector3(s * 0.95f, 2.8f, 0f), new Vector3(0.3f, 4.2f, 0.9f), Brass);
            var tube = kit.Group("TelescopeTube", mount + Vector3.up * 4.6f);
            tube.rotation = Quaternion.Euler(-52f, 0f, 0f);   // looking up and to the north
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 2.2f), new Vector3(1.4f, 4.4f, 1.4f), PatchworkAssets.Plain(new Color(0.16f, 0.18f, 0.36f), 0.7f), new Vector3(90f, 0f, 0f), tube);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, -2.8f), new Vector3(0.9f, 0.9f, 0.9f), Brass, new Vector3(90f, 0f, 0f), tube);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, -3.9f), new Vector3(0.4f, 0.3f, 0.4f), Brass, new Vector3(90f, 0f, 0f), tube);
            foreach (float z in new[] { -1.9f, 0.6f, 3.4f, 6.5f })
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, z), new Vector3(1.52f, 0.1f, 1.52f), Brass, new Vector3(90f, 0f, 0f), tube);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 6.62f), new Vector3(1.3f, 0.02f, 1.3f), PatchworkAssets.Glow(new Color(0.6f, 0.85f, 1f), 1.4f), new Vector3(90f, 0f, 0f), tube));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.5f, 0.7f, 1f), new Vector3(0.3f, 1.2f, 0.3f), Brass, new Vector3(90f, 0f, 0f), tube);
            TownKit.Solid(kit, mount + Vector3.up * 1.6f, new Vector3(2.2f, 3.2f, 1.6f), 0f);
            var look = kit.Volume<UseSpot>(C + new Vector3(0f, 2.2f, -2.4f), new Vector3(3.6f, 3f, 2.6f), "TelescopeSpot");
            look.Label = "Look through the telescope";
            look.PromptPoint = C + new Vector3(0f, 3.6f, -1.8f);
            // (Not until its three lenses are back in it: see Level2Quest.)
            look.Used = p => { if (Level2Quest.Instance == null || Level2Quest.Instance.TelescopeReady()) director.LookThroughTelescope(); };
            director.TelescopeEye = mount + new Vector3(0f, 4.4f, -1.4f);
            director.TelescopeLook = C + new Vector3(0f, 30f, 12.5f);
            Plaque(kit, C + new Vector3(3.2f, 1f, -2.6f), 200f, "The Great Telescope",
                "In the winter of 1609 Galileo pointed a telescope at the night sky, which almost nobody had thought to do. He saw mountains on the Moon. He saw that the Milky Way is made of stars. And he saw four little moons going round Jupiter.",
                "That last one changed everything. If moons could circle Jupiter, then not everything in the heavens went round the Earth.");
            kit.Hint(C + new Vector3(0f, 1.5f, 14f), new Vector3(14f, 4f, 10f), "The Observatory",
                "You made it! Climb the steps to the great telescope and take a look. The balcony on the far side looks down over the whole atrium: hop the rail for a quick way back to the Pod.");

            // ----- The orrery chandelier -----
            var chand = kit.Group("ObservatoryOrrery", C + Vector3.up * 15.5f);
            Beam(kit, C + Vector3.up * 15.5f, domeBase + Vector3.up * 14.6f, 0.1f, Brass);
            NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 2.2f, PatchworkAssets.Glow(new Color(1f, 0.8f, 0.45f), 2.4f), default(Vector3), chand));
            float[] orbit = { 4f, 6.4f, 9f };
            Color[] pc = { new Color(0.5f, 0.75f, 1f), new Color(1f, 0.6f, 0.4f), new Color(0.8f, 0.65f, 1f) };
            for (int i = 0; i < 3; i++)
            {
                var ring = kit.Group("OrreryRing", C + Vector3.up * 15.5f);
                ring.rotation = Quaternion.Euler(i * 7f - 6f, i * 50f, i * 5f);
                Hoop(kit, ring, orbit[i], 0.12f, Brass, 30 + i * 6);
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(orbit[i], 0f, 0f), Vector3.one * (1f + i * 0.25f), PatchworkAssets.Glow(pc[i], 1.2f), default(Vector3), ring));
                var spin = ring.gameObject.AddComponent<Spinner>();
                spin.Speed = 22f - i * 6f;
            }
            PointLight(kit, C + Vector3.up * 13f, new Color(1f, 0.85f, 0.6f), 30f, 1.7f);
            PointLight(kit, C + new Vector3(0f, 6f, -16f), new Color(0.7f, 0.75f, 1f), 20f, 1.1f);

            // ----- The star library: a gallery round the west side, up a stair of giant books -----
            Color[] covers = { Wine, Navy, new Color(0.25f, 0.45f, 0.4f), Plum, new Color(0.65f, 0.45f, 0.2f) };
            for (int i = 0; i < 5; i++)
            {
                float top = 1f + i;
                Vector3 at = C + new Vector3(-12.6f - i * 1.9f, 0f, 0f);
                kit.Block(at + Vector3.up * top * 0.5f, new Vector3(2f, top, 3.4f), StoneCol, "BookStep", PatchworkAssets.Felt(CreamCol));
                kit.Deco(PrimitiveType.Cube, at + Vector3.up * (top - 0.1f), new Vector3(2.16f, 0.22f, 3.56f), PatchworkAssets.Fabric(covers[i]));
                kit.Deco(PrimitiveType.Cube, at + new Vector3(0f, top * 0.5f, 1.72f), new Vector3(2.16f, top, 0.14f), PatchworkAssets.Fabric(covers[i]));
                NoShadow(kit.Deco(PrimitiveType.Cube, at + new Vector3(0f, top - 0.5f, 1.8f), new Vector3(1.2f, 0.16f, 0.03f), gold));
            }
            for (int i = 0; i < 7; i++)
            {
                float deg = 144f + i * 12f;
                float a = deg * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var slab = kit.Block(C + dir * 22.9f + Vector3.up * 4.8f, new Vector3(2f * 22.9f * Mathf.Tan(6f * Mathf.Deg2Rad) + 0.7f, 0.4f, 5f), StoneCol, "LibraryGallery", DarkWood);
                slab.rotation = Quaternion.LookRotation(dir, Vector3.up);
                NoShadow(kit.Deco(PrimitiveType.Cube, C + dir * 22.9f + Vector3.up * 5.02f, new Vector3(3.4f, 0.03f, 3.6f), i % 2 == 0 ? StarCloth : PlumStars, new Vector3(0f, 90f - deg, 0f)));
                kit.Deco(PrimitiveType.Cylinder, C + dir * 20.5f + Vector3.up * 5.5f, new Vector3(0.1f, 0.5f, 0.1f), Brass);
                
            }
            // Bookcases between the columns (which stand every 15 degrees, from 142.5).
            int bc = 0;
            foreach (float deg in new[] { 150f, 165f, 195f, 210f })
            {
                float a = deg * Mathf.Deg2Rad;
                Bookcase(kit, C + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 24.6f + Vector3.up * 5f, 270f - deg, 3.6f, 2.6f, 20 + bc++);
            }
            kit.GoldenButton("L2_b_library", C + new Vector3(Mathf.Cos(204f * Mathf.Deg2Rad) * 22.6f, 6.6f, Mathf.Sin(204f * Mathf.Deg2Rad) * 22.6f));
            kit.ThreadLine(C + new Vector3(Mathf.Cos(150f * Mathf.Deg2Rad) * 22.6f, 6.3f, Mathf.Sin(150f * Mathf.Deg2Rad) * 22.6f), C + new Vector3(Mathf.Cos(170f * Mathf.Deg2Rad) * 22.6f, 6.3f, Mathf.Sin(170f * Mathf.Deg2Rad) * 22.6f), 4);
            Plaque(kit, C + new Vector3(-12.6f, 0f, 3.4f), 0f, "The Moon and the Sea",
                "Twice a day, every day, the whole ocean bulges up towards the Moon and follows it round the world. That's the tide. The sea and the sky have been holding hands all along.",
                "It's why this museum keeps its telescope at the bottom of the sea. It seemed the proper place.");

            // ----- People, and things to sit on -----
            Person(kit, C + new Vector3(9f, 0f, 9f), 220f, ToyNpc.Kind.Bird, "Stella", new Color(0.3f, 0.3f, 0.6f), new Color(1f, 0.85f, 0.45f), 1.4f, () => new[]
            {
                "Shh! ...Oh, it's all right, she's gone round the back again.",
                "I've watched that whale every night for eleven years. She has stars on her back. I've counted them. I've named most of them.",
                "Go on, have a look through the big telescope. It's the best seat in the house."
            });
            Vector3 desk = C + new Vector3(13f, 0f, -9f);
            kit.Block(desk + Vector3.up * 0.55f, new Vector3(3.4f, 1.1f, 1.6f), WoodCol, "ChartDesk", DarkWood);
            NoShadow(kit.Deco(PrimitiveType.Cube, desk + Vector3.up * 1.12f, new Vector3(2.6f, 0.03f, 1.2f), PatchworkAssets.Felt(new Color(0.9f, 0.84f, 0.66f)), new Vector3(0f, 8f, 0f)));
            NoShadow(kit.Deco(PrimitiveType.Sphere, desk + new Vector3(1.2f, 1.5f, 0.2f), Vector3.one * 0.5f, PatchworkAssets.Glow(LampGold, 2.2f)));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, desk + new Vector3(-1.1f, 1.3f, -0.2f), new Vector3(0.1f, 0.2f, 0.1f), Brass, new Vector3(0f, 0f, 60f)));
            kit.AddCheckpoint(C + new Vector3(0f, 0f, 22f), 180f);
            Trail(kit, 10, C + new Vector3(0f, 1.2f, 21f), C + new Vector3(0f, 1.2f, 17f), C + new Vector3(0f, 1.2f, 13f), C + new Vector3(0f, 1.2f, 9f));
            Banner(kit, C + new Vector3(Mathf.Cos(60f * Mathf.Deg2Rad) * (R - 1.2f), 8.6f, Mathf.Sin(60f * Mathf.Deg2Rad) * (R - 1.2f)), 270f - 60f, 2.6f, 6f, StarCloth, 1);
            Banner(kit, C + new Vector3(Mathf.Cos(120f * Mathf.Deg2Rad) * (R - 1.2f), 8.6f, Mathf.Sin(120f * Mathf.Deg2Rad) * (R - 1.2f)), 270f - 120f, 2.6f, 6f, PlumStars, 2);

            // ----- The balcony over the atrium -----
            kit.Block(new Vector3(0f, DeckY - 0.5f, -49f), new Vector3(20f, 1f, 7f), StoneCol, "OverlookBalcony", PaleStone);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, DeckY + 0.02f, -48.6f), new Vector3(17f, 0.03f, 5.4f), PlumStars));
            Balustrade(kit, new Vector3(-9.8f, DeckY, -52.3f), new Vector3(9.8f, DeckY, -52.3f));
            Balustrade(kit, new Vector3(-9.8f, DeckY, -52.3f), new Vector3(-9.8f, DeckY, -48.2f));
            Balustrade(kit, new Vector3(9.8f, DeckY, -52.3f), new Vector3(9.8f, DeckY, -48.2f));
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 6f, DeckY - 2.2f, -49.6f), new Vector3(1f, 2.6f, 4.6f), PaleStone, new Vector3(-28f, 0f, 0f));
                // (Stone cheeks closing the slit between the round wall and the atrium's arch.)
                kit.Block(new Vector3(s * 10.5f, DeckY + 4.5f, -45f), new Vector3(1.2f, 9f, 2.8f), StoneCol, "OverlookCheek", stone);
            }
            Plaque(kit, new Vector3(6f, DeckY, -49.2f), 0f, "The Whole Museum",
                "From up here you can see all of it at once: the whale, the armillary, the Great Window. Every museum is really just somebody saying, \"Look. Look at this. Isn't it wonderful?\"");
            Trail(kit, 50, new Vector3(-4f, DeckY + 1.3f, -50f), new Vector3(0f, DeckY + 1.3f, -50.4f), new Vector3(4f, DeckY + 1.3f, -50f));
        }
    }
}

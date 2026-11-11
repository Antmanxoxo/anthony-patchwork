using UnityEngine;

namespace Patchwork
{
    // The aquarium half of the museum.
    //   The Kelp Galleries - a dim hall whose whole middle is an open pool with a forest of
    //                        kelp growing up out of it to the roof. Every form has its own way
    //                        over: star-patch platforms, lanterns to swing on, warm vents to
    //                        glide between, or just swim it.
    //   The Great Deep     - the big tank. You don't go in it: you walk through it, along a
    //                        glass tunnel on stilts above the sea bed, with the Bell (a glass
    //                        dome) half way along.
    public static partial class Level2Builder
    {
        const float PoolZ0 = 18f, PoolZ1 = 58f, PoolSurface = -0.6f, PoolBed = -6f;
        static readonly Color[] KelpCols = { new Color(0.5f, 0.62f, 0.22f), new Color(0.72f, 0.62f, 0.2f), new Color(0.36f, 0.56f, 0.3f), new Color(0.62f, 0.5f, 0.18f) };

        // A wall of glass in an iron frame (no tank behind it: whatever's there shows through).
        // "foot" is the middle of its bottom edge; yaw is the way it faces.
        static void GlassWall(LevelKit kit, Vector3 foot, float yaw, float width, float height, int panesX, int panesY)
        {
            var r = Rot(yaw);
            var pane = kit.Block(foot + Vector3.up * height * 0.5f, new Vector3(width, height, 0.3f), Color.white, "GlassWall", GlassMat);
            pane.rotation = r;
            NoShadow(pane);
            var iron = IronMat;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i <= panesX; i++)
                {
                    float x = -width * 0.5f + width * i / panesX;
                    bool edge = i == 0 || i == panesX;
                    NoShadow(kit.Deco(PrimitiveType.Cube, foot + r * new Vector3(x, height * 0.5f, side * 0.1f), new Vector3(edge ? 0.5f : 0.26f, height, 0.3f), iron, new Vector3(0f, yaw, 0f)));
                }
                for (int j = 0; j <= panesY; j++)
                {
                    float y = height * j / panesY;
                    bool edge = j == 0 || j == panesY;
                    NoShadow(kit.Deco(PrimitiveType.Cube, foot + r * new Vector3(0f, y, side * 0.1f), new Vector3(width + 0.5f, edge ? 0.5f : 0.22f, 0.3f), iron, new Vector3(0f, yaw, 0f)));
                }
            }
        }

        // A pool of deep water you can see down into. Water Form swims it; nobody else can.
        static WaterBody Pool(LevelKit kit, float x0, float x1, float z0, float z1, float surface, float bottom, Color tint, string name = "Pool")
        {
            float w = x1 - x0, d = z1 - z0;
            Vector3 c = new Vector3((x0 + x1) * 0.5f, surface, (z0 + z1) * 0.5f);
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = name + "Surface";
            go.transform.SetParent(kit.Root, false);
            go.transform.position = c + Vector3.down * 0.03f;
            go.transform.localScale = new Vector3(w, 0.06f, d);
            var mat = PatchworkAssets.CraftWater(0.7f, new Vector2(Mathf.Max(1f, w / 6f), Mathf.Max(1f, d / 6f)));
            mat.SetColor("_BaseColor", new Color(Mathf.Lerp(1f, tint.r, 0.5f), Mathf.Lerp(1f, tint.g, 0.5f), Mathf.Lerp(1f, tint.b, 0.5f), 0.7f));
            mat.SetColor("_EmissionColor", tint * 0.3f);
            var rend = go.GetComponent<Renderer>();
            rend.sharedMaterial = mat;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var drift = go.AddComponent<PaintLake>();
            drift.Drift = new Vector2(0.015f, 0.025f);
            drift.Setup(mat);
            NoShadow(kit.Deco(PrimitiveType.Cube, c + Vector3.down * 1.2f, new Vector3(w, 0.05f, d), PatchworkAssets.Bubble(new Color(tint.r * 0.5f, tint.g * 0.5f, tint.b * 0.6f, 0.4f))));
            var body = kit.Volume<WaterBody>(new Vector3(c.x, (bottom + surface + 0.6f) * 0.5f, c.z), new Vector3(w, surface + 0.6f - bottom, d), name);
            body.SurfaceY = surface;
            body.Divable = true;
            return body;
        }

        // ================================================================ The Kelp Galleries

        static void KelpGalleries(LevelKit kit)
        {
            var stone = DarkStone;
            float T = KelpTop;
            Color teal = new Color(0.25f, 0.8f, 0.75f);

            // ----- Floors: a foyer, the pool, and the far terrace -----
            Floor(kit, KelpX0, -32f, KelpZ0, PoolZ0, 0f, SeaQuilt, 4.5f, "KelpFoyer");
            Floor(kit, KelpX0, -30f, PoolZ1, KelpZ1, 0f, SeaQuilt, 4.5f, "KelpTerrace");
            kit.Block(new Vector3(-60f, -4f, (KelpZ0 + PoolZ0) * 0.5f), new Vector3(56f, 6f, PoolZ0 - KelpZ0), StoneCol, "FoyerBase", stone);
            kit.Block(new Vector3(-60f, -4f, (PoolZ1 + KelpZ1) * 0.5f), new Vector3(56f, 6f, KelpZ1 - PoolZ1), StoneCol, "TerraceBase", stone);
            kit.Block(new Vector3(-60f, PoolBed - 0.5f, (PoolZ0 + PoolZ1) * 0.5f), new Vector3(60f, 1f, PoolZ1 - PoolZ0 + 2f), StoneCol, "PoolBed", PatchworkAssets.Felt(new Color(0.3f, 0.36f, 0.34f)));
            Pool(kit, KelpX0, KelpX1, PoolZ0, PoolZ1, PoolSurface, PoolBed, new Color(0.15f, 0.6f, 0.6f), "KelpPool");
            // A brass kerb along each edge of the pool.
            foreach (float z in new[] { PoolZ0 - 0.25f, PoolZ1 + 0.25f })
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(-60f, 0.12f, z), new Vector3(56f, 0.24f, 0.5f), PaleStone);
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(-60f, 0.26f, z), new Vector3(56f, 0.05f, 0.2f), Brass));
            }

            // ----- Walls -----
            // West: stone at each end, and a kelp tank between.
            Wall(kit, new Vector3(-89f, 0f, -8f), new Vector3(-89f, 0f, 20f), -8f, T + 4f, 2f, stone);
            Wall(kit, new Vector3(-89f, 0f, 56f), new Vector3(-89f, 0f, 78f), -8f, T + 4f, 2f, stone);
            Wall(kit, new Vector3(-89f, 0f, 20f), new Vector3(-89f, 0f, 56f), 18f, T + 4f, 2f, stone);
            Wall(kit, new Vector3(-88.6f, 0f, 20f), new Vector3(-88.6f, 0f, 56f), -8f, 0f, 1.2f, stone);
            // North.
            Wall(kit, new Vector3(-90f, 0f, 77f), new Vector3(-30f, 0f, 77f), -8f, T + 4f, 2f, stone);
            // East: stone, a long window onto the Great Deep, and the door to the tunnel.
            Wall(kit, new Vector3(-31f, 0f, -4f), new Vector3(-31f, 0f, 22f), -8f, T + 4f, 2f, stone);
            Wall(kit, new Vector3(-31f, 0f, 22f), new Vector3(-31f, 0f, 56f), 16f, T + 4f, 2f, stone);
            Wall(kit, new Vector3(-31f, 0f, 22f), new Vector3(-31f, 0f, 56f), -8f, 0f, 2f, stone);
            WallWithDoor(kit, new Vector3(-31f, 0f, 56f), new Vector3(-31f, 0f, 78f), -8f, T + 4f, 2f, stone, TunnelZ - 56f, 8f, 7f);
            GlassWall(kit, new Vector3(-31f, 0f, 39f), 270f, 34f, 16f, 6, 3);
            // The ceiling, with long skylights of "surface" over the pool.
            kit.Block(new Vector3(-60f, T + 0.5f, 36f), new Vector3(60f, 1f, 84f), Navy, "KelpCeiling", PatchworkAssets.Felt(new Color(0.04f, 0.12f, 0.16f)));
            var skin = PatchworkAssets.Glow(new Color(0.55f, 0.95f, 0.9f), 1.1f);
            var beam = Shaft(new Color(0.6f, 1f, 0.9f), 0.06f);
            for (int i = 0; i < 4; i++)
            {
                float x = -80f + i * 13.5f;
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(x, T - 0.04f, 38f), new Vector3(7f, 0.1f, 36f), skin));
                for (int k = 0; k < 3; k++)
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(x + 2f, T * 0.5f - 3f, 25f + k * 13f), new Vector3(2.4f, T + 7f, 0.04f), beam, new Vector3(0f, 30f + k * 40f, 14f)));
            }
            foreach (float z in new[] { 10f, 30f, 46f, 66f })
                kit.Deco(PrimitiveType.Cube, new Vector3(-60f, T - 0.5f, z), new Vector3(56f, 1f, 1f), IronMat);

            // ----- The west tank -----
            var tank = Tank(kit, new Vector3(-88f, 0f, 38f), 90f, 36f, 18f, 20f, 6, 3, new Color(0.02f, 0.14f, 0.2f), new Color(0.3f, 0.85f, 0.7f), 4f, true, true, 23);
            Reef(tank, 18);
            for (int i = 0; i < 10; i++) Kelp(tank, tank.R(-16f, 16f), tank.R(3f, 18f), tank.R(14f, 21f), KelpCols[i % KelpCols.Length]);
            TankShoal(tank, 0f, 12f, 10f, 11f, 5f, 12f, 22, 0.45f, new Color(1f, 0.6f, 0.25f));
            TankSwim(tank, Swimmer.Turtle, -6f, 8f, 8f, 8f, 3f, 8f, 1.3f, new Color(0.4f, 0.6f, 0.45f), new Color(0.85f, 0.8f, 0.55f));
            TankSwim(tank, Swimmer.Ray, 4f, 15f, 11f, 11f, 5f, -7f, 1.6f, new Color(0.3f, 0.3f, 0.45f), CreamCol, 120f);
            for (int i = 0; i < 8; i++)
                TankSwim(tank, i % 2 == 0 ? Swimmer.Tall : Swimmer.Fish, tank.R(-14f, 14f), tank.R(5f, 16f), tank.R(5f, 12f), tank.R(2f, 5f), tank.R(1f, 3f), (i % 2 == 0 ? 1f : -1f) * tank.R(14f, 30f),
                    tank.R(0.6f, 1f), i % 3 == 0 ? new Color(1f, 0.5f, 0.15f) : i % 3 == 1 ? new Color(0.95f, 0.9f, 0.4f) : new Color(0.5f, 0.75f, 1f), CreamCol, i * 47f);
            for (int i = 0; i < 3; i++) Bubbles(kit, tank.At(tank.R(-14f, 14f), 0.5f, tank.R(3f, 14f)), 20f);

            // ----- The forest in the pool -----
            var pool = new TankSpace { Kit = kit, Origin = new Vector3(-60f, 0f, PoolZ0), Yaw = 180f, Width = 56f, Height = T - PoolBed, Depth = PoolZ1 - PoolZ0, FloorY = PoolBed, Rng = new System.Random(4242) };
            Reef(pool, 26, 2f, 38f);
            // (x here is mirrored: +x in the pool's own space is west.)
            float[,] kelpAt =
            {
                { 20f, 6f }, { 17f, 12f }, { 21f, 21f }, { 13f, 31f }, { 20f, 35f }, { 8f, 7f }, { 9f, 19f }, { 4f, 13f }, { -9f, 9f }, { -7f, 27f },
                { -3f, 30f }, { -12f, 17f }, { -22f, 6f }, { -25f, 20f }, { -21f, 33f }, { -13f, 36f }, { 19f, 28f }, { -24f, 13f }, { 14f, 3f }, { -3f, 37f }
            };
            for (int i = 0; i < kelpAt.GetLength(0); i++)
                Kelp(pool, kelpAt[i, 0], kelpAt[i, 1], pool.R(19f, 26.5f), KelpCols[i % KelpCols.Length]);
            for (int i = 0; i < 6; i++)
                Swim(kit, i % 2 == 0 ? Swimmer.Fish : Swimmer.Tall, new Vector3(-80f + i * 8f, -2.6f - (i % 3) * 0.8f, 24f + (i * 11) % 30), 3f + i % 3, 2f + i % 2, (i % 2 == 0 ? 1f : -1f) * (16f + i * 3f), 0.9f,
                    i % 2 == 0 ? new Color(1f, 0.65f, 0.2f) : new Color(0.4f, 0.9f, 0.8f), CreamCol, i * 60f);
            foreach (var p in new[] { new Vector3(-74f, PoolBed, 26f), new Vector3(-48f, PoolBed, 34f), new Vector3(-66f, PoolBed, 50f), new Vector3(-40f, PoolBed, 52f) })
                Bubbles(kit, p, 5.4f, 6);
            foreach (var p in new[] { new Vector3(-74f, 8f, 30f), new Vector3(-48f, 8f, 38f), new Vector3(-66f, 8f, 48f) })
                PointLight(kit, p, teal, 24f, 1.5f);
            PointLight(kit, new Vector3(-60f, -3f, 38f), new Color(0.3f, 1f, 0.8f), 26f, 1.6f);

            // ----- Ways across -----
            // Down the middle: star patches, two of them gliding from side to side, and a big
            // island half way with the Heartstring over it.
            var cloth = StarCloth;
            RoundPlatform(kit, new Vector3(-60f, 0.5f, 22.4f), 2.3f, 1.4f, cloth);
            RoundPlatform(kit, new Vector3(-55.5f, 0.5f, 27.4f), 2f, 1.4f, PlumStars);
            var g1 = RoundPlatform(kit, new Vector3(-54f, 0.5f, 33f), 2f, 1.2f, cloth);
            var gl1 = g1.gameObject.AddComponent<Glider>();
            gl1.A = new Vector3(-51f, 0.5f, 33f);
            gl1.B = new Vector3(-58.5f, 0.5f, 33f);
            gl1.Travel = 3.2f;
            gl1.Rest = 1.1f;
            g1.gameObject.AddComponent<Carrier>();
            RoundPlatform(kit, new Vector3(-60f, 0.5f, 39.4f), 4.3f, 1.6f, cloth);
            RoundPlatform(kit, new Vector3(-65.5f, 0.5f, 46.2f), 2f, 1.4f, PlumStars);
            var g2 = RoundPlatform(kit, new Vector3(-66f, 0.5f, 51.6f), 2f, 1.2f, cloth);
            var gl2 = g2.gameObject.AddComponent<Glider>();
            gl2.A = new Vector3(-69f, 0.5f, 51.6f);
            gl2.B = new Vector3(-62f, 0.5f, 51.6f);
            gl2.Travel = 3.2f;
            gl2.Rest = 1.1f;
            gl2.Offset = 2f;
            g2.gameObject.AddComponent<Carrier>();
            RoundPlatform(kit, new Vector3(-60f, 0.5f, 55.4f), 2.2f, 1.4f, cloth);
            // Posts holding the fixed ones up out of the water.
            foreach (var p in new[] { new Vector3(-60f, 0f, 22.4f), new Vector3(-55.5f, 0f, 27.4f), new Vector3(-60f, 0f, 39.4f), new Vector3(-65.5f, 0f, 46.2f), new Vector3(-60f, 0f, 55.4f) })
                kit.Deco(PrimitiveType.Cylinder, new Vector3(p.x, (PoolBed - 0.9f) * 0.5f, p.z), new Vector3(0.7f, (-0.9f - PoolBed) * 0.5f, 0.7f), IronMat);
            Level1Builder.MakeHeartstring(kit, "L2_hs_kelp", "The Kelp Pool", new Vector3(-60f, 2.5f, 39.4f), false);
            Trail(kit, 10, new Vector3(-60f, 1.8f, 22.4f), new Vector3(-55.5f, 1.8f, 27.4f), new Vector3(-56f, 2f, 36f), new Vector3(-64f, 2f, 43f), new Vector3(-65.5f, 1.8f, 46.2f), new Vector3(-60f, 1.8f, 55.4f));

            // East side: lanterns on long chains for Yarn Form, with one stump to rest on.
            kit.Deco(PrimitiveType.Cube, new Vector3(-42f, T - 1.1f, 38f), new Vector3(0.6f, 0.6f, 44f), IronMat);
            foreach (float z in new[] { 24.5f, 33.5f, 46.5f, 54f })
                kit.Anchor(new Vector3(-42f, 7.2f, z), 13.5f, LevelKit.AnchorStyle.Lantern, T - 1.4f - 7.2f - 0.6f);
            RoundPlatform(kit, new Vector3(-42f, 0.9f, 40f), 1.9f, 1.6f, PlumStars);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-42f, (PoolBed - 0.7f) * 0.5f, 40f), new Vector3(0.7f, (-0.7f - PoolBed) * 0.5f, 0.7f), IronMat);
            kit.ThreadLine(new Vector3(-42f, 4f, 22f), new Vector3(-42f, 4f, 56f), 8);

            // West side: warm vents for Wind Form, one on the foyer and one on a rock in the pool.
            kit.Updraft(new Vector3(-76f, 0f, 14f), 13f, LevelKit.UpdraftStyle.Steam);
            RoundPlatform(kit, new Vector3(-76f, 0.4f, 38f), 2.4f, 1.6f, PatchworkAssets.Felt(new Color(0.3f, 0.36f, 0.4f)), PatchworkAssets.Felt(new Color(0.24f, 0.28f, 0.34f)), false);
            kit.Deco(PrimitiveType.Sphere, new Vector3(-76f, -3.2f, 38f), new Vector3(5f, 7f, 5f), PatchworkAssets.Felt(new Color(0.24f, 0.28f, 0.34f)));
            kit.Updraft(new Vector3(-76f, 0.4f, 38f), 13f, LevelKit.UpdraftStyle.Steam);
            Trail(kit, 10, new Vector3(-76f, 11f, 20f), new Vector3(-76f, 9f, 26f), new Vector3(-76f, 7f, 32f), new Vector3(-76f, 10f, 44f), new Vector3(-76f, 7.5f, 50f), new Vector3(-76f, 5f, 56f));

            // For Water Form: swim it, and ride the spring by the tank up to the keepers' catwalk.
            kit.Block(new Vector3(-86f, 9.85f, 38f), new Vector3(3f, 0.3f, 32f), WoodCol, "Catwalk", PatchworkAssets.Planks(new Color(0.45f, 0.5f, 0.52f)));
            for (float z = 22f; z <= 54f; z += 4f)
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(-84.6f, 10.5f, z), new Vector3(0.08f, 0.5f, 0.08f), Brass);
                Beam(kit, new Vector3(-87.4f, 9.7f, z), new Vector3(-87.4f, 18f, z), 0.07f, IronMat);
            }
            kit.Rope(new Vector3(-84.6f, 11f, 28.4f), new Vector3(-84.6f, 11f, 54f), Brass, 0.07f);
            TownKit.Solid(kit, new Vector3(-84.6f, 10.5f, 41.2f), new Vector3(0.2f, 1f, 25.6f), 0f);
            kit.Fountain(new Vector3(-82.6f, PoolSurface, 25f), 11.6f, LevelKit.FountainStyle.Spring);
            kit.GoldenButton("L2_b_catwalk", new Vector3(-86f, 11.4f, 52f));
            kit.ThreadLine(new Vector3(-86f, 11.1f, 30f), new Vector3(-86f, 11.1f, 48f), 5);

            // ----- The foyer -----
            kit.AddCheckpoint(new Vector3(-60f, 0f, 0f), 0f);
            foreach (var p in new[] { new Vector3(-72f, 12f, 6f), new Vector3(-48f, 12f, 6f) }) HangingLantern(kit, p, T, 20f, 1.2f);
            foreach (var p in new[] { new Vector3(-72f, 12f, 68f), new Vector3(-48f, 12f, 68f) }) HangingLantern(kit, p, T, 20f, 1.2f);
            Rug(kit, new Vector3(-60f, 0f, 7f), new Vector2(7f, 18f), StarCloth, 0f, PatchworkAssets.Felt(new Color(0.4f, 0.85f, 0.8f)));
            Rug(kit, new Vector3(-60f, 0f, 66f), new Vector2(7f, 13f), StarCloth, 0f, PatchworkAssets.Felt(new Color(0.4f, 0.85f, 0.8f)));
            Plaque(kit, new Vector3(-55f, 0f, 15.4f), 180f, "The Kelp Forest",
                "Kelp isn't a plant: it's an enormous seaweed. It has no roots, only a knot called a holdfast that grips the rock, and little gas-filled floats that hold it up towards the light.",
                "Sea otters wrap themselves in it before they go to sleep, so the tide doesn't carry them away. Sometimes they hold each other's paws as well, to be sure.");
            Vector3 aq = DisplayCase(kit, new Vector3(-44f, 0f, 4f), new Vector3(2.2f, 2.6f, 2.2f));
            for (int s = -1; s <= 1; s += 2)
            {
                NoShadow(kit.Deco(PrimitiveType.Capsule, aq + new Vector3(s * 0.26f, 0.7f, 0f), new Vector3(0.42f, 0.6f, 0.42f), PatchworkAssets.Plain(new Color(0.95f, 0.8f, 0.25f), 0.7f)));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, aq + new Vector3(s * 0.26f, 1.36f, 0f), new Vector3(0.12f, 0.08f, 0.12f), Brass));
            }
            NoShadow(kit.Deco(PrimitiveType.Cube, aq + new Vector3(0f, 0.7f, 0f), new Vector3(0.9f, 0.08f, 0.44f), IronMat));
            NoShadow(kit.Deco(PrimitiveType.Sphere, aq + new Vector3(0f, 1.5f, 0.3f), Vector3.one * 0.26f, IronMat));
            Plaque(kit, new Vector3(-44f, 0f, 6.3f), 0f, "The Aqualung",
                "Until 1943, a diver was tied to the surface by an air hose. Then Jacques Cousteau and Émile Gagnan built this: air in a bottle on your back, and a valve that gives you a breath whenever you ask for one.",
                "For the first time, people could swim about under the sea as freely as the fish. Most of what we know about kelp forests, we learned after that.");
            Person(kit, new Vector3(-66f, 0f, 14.5f), 150f, ToyNpc.Kind.Pirate, "Old Barnacle", new Color(0.3f, 0.4f, 0.55f), new Color(0.9f, 0.3f, 0.3f), 0.7f, () => new[]
            {
                "Arr. Don't mind me. I'm not fishin'. I'm just holdin' a rod near some water.",
                "There's four ways over, by my count. Hop the star patches up the middle. Swing the lanterns on the east side. Ride the warm vents on the west.",
                "Or do what any sensible sailor'd do, and go as Water. Ye can swim the lot. There's even a spring by the big tank that'll throw ye up to the keepers' catwalk."
            });
            kit.Hint(new Vector3(-60f, 1.5f, 13f), new Vector3(16f, 4f, 8f), "The Kelp Pool",
                "Only Water Form can swim. Everyone else: hop the star patches (two of them glide from side to side), swing the lanterns on the right, or ride the vents on the left.");
            TrailLine(kit, new Vector3(-60f, 1.2f, -2f), new Vector3(-60f, 1.2f, 16f), 6);
            foreach (float x in new[] { -80f, -70f, -50f, -40f }) TownKit.Bench(kit, new Vector3(x, 0f, 12f), 0f);

            // ----- The far terrace -----
            kit.AddCheckpoint(new Vector3(-60f, 0f, 61f), 0f);
            TownKit.Bench(kit, new Vector3(-37f, 0f, 60f), 90f);
            Plaque(kit, new Vector3(-36f, 0f, 58.6f), 270f, "The Great Deep",
                "Through this glass is the biggest tank in the museum: seven storeys of sea water. The way on is the tunnel, through the arch to your left.");
            Person(kit, new Vector3(-50f, 0f, 70f), 200f, ToyNpc.Kind.Teddy, "Button", new Color(0.8f, 0.6f, 0.4f), new Color(0.4f, 0.75f, 0.7f), 1.35f, () => new[]
            {
                "I counted the fish in the big tank. There's a hundred and... lots.",
                "The whale goes right over the tunnel. If you wait in the Bell you can see her tummy!"
            });
            Banner(kit, new Vector3(-60f, 17f, 75.9f), 180f, 3.4f, 9f, StarCloth, 2);
            Banner(kit, new Vector3(-74f, 17f, 75.9f), 180f, 3f, 8f, PlumStars, 3);
            Banner(kit, new Vector3(-46f, 17f, 75.9f), 180f, 3f, 8f, PlumStars, 0);
            Trail(kit, 10, new Vector3(-58f, 1.2f, 62f), new Vector3(-52f, 1.2f, 64f), new Vector3(-46f, 1.2f, 65.5f), new Vector3(-40f, 1.2f, 66f), new Vector3(-34f, 1.2f, 66f));
        }

        // ================================================================ The Great Deep

        static void GreatDeep(LevelKit kit)
        {
            Color deep = new Color(0.01f, 0.05f, 0.2f), shallow = new Color(0.12f, 0.45f, 0.8f);
            float W = DeepX1 - DeepX0, D = DeepZ1 - DeepZ0, cx = (DeepX0 + DeepX1) * 0.5f, cz = (DeepZ0 + DeepZ1) * 0.5f;
            float h = DeepTop - DeepFloor;
            // The sea bed, the glowing far walls and the bright surface a long way up.
            kit.Block(new Vector3(cx, DeepFloor - 0.5f, cz), new Vector3(W + 4f, 1f, D + 4f), StoneCol, "SeaBed", PatchworkAssets.Felt(new Color(0.2f, 0.26f, 0.36f)));
            var far = Backdrop(deep, shallow, 0.9f);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(cx, DeepFloor + h * 0.5f, DeepZ1 + 0.4f), new Vector3(W + 4f, h, 0.2f), far));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(-15f, DeepFloor + h * 0.5f, DeepZ0 - 0.4f), new Vector3(34f, h, 0.2f), far));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(cx, DeepTop + 0.05f, cz), new Vector3(W + 4f, 0.1f, D + 4f), PatchworkAssets.Glow(new Color(0.45f, 0.75f, 1f), 1.1f)));
            var shaft = Shaft(new Color(0.55f, 0.8f, 1f), 0.055f);
            var rng = new System.Random(61);
            for (int i = 0; i < 12; i++)
            {
                Vector3 p = new Vector3(DeepX0 + 6f + (float)rng.NextDouble() * (W - 12f), DeepFloor + h * 0.52f, DeepZ0 + 6f + (float)rng.NextDouble() * (D - 12f));
                if (Mathf.Abs(p.z - TunnelZ) < 7f) p.z += 14f;
                NoShadow(kit.Deco(PrimitiveType.Cube, p, new Vector3(2f + (float)rng.NextDouble() * 2.5f, h * 1.06f, 0.04f), shaft, new Vector3(0f, (float)rng.NextDouble() * 180f, 10f + (float)rng.NextDouble() * 10f)));
            }
            foreach (var p in new[] { new Vector3(-10f, 12f, 44f), new Vector3(24f, 12f, 46f), new Vector3(-8f, 12f, 86f), new Vector3(26f, 12f, 86f) })
                PointLight(kit, p, new Color(0.3f, 0.55f, 1f), 42f, 1.7f);

            // ----- The sea bed -----
            var bed = new TankSpace { Kit = kit, Origin = new Vector3(cx, 0f, DeepZ0), Yaw = 180f, Width = W, Height = h, Depth = D, FloorY = DeepFloor, Rng = new System.Random(977) };
            Reef(bed, 30, 3f, 40f);
            Reef(bed, 26, 52f, D - 3f);
            for (int i = 0; i < 14; i++)
            {
                float z = i % 2 == 0 ? bed.R(4f, 36f) : bed.R(56f, D - 4f);
                Kelp(bed, bed.R(-W * 0.46f, W * 0.46f), z, bed.R(12f, 22f), KelpCols[i % KelpCols.Length]);
            }
            // A whale fall: the bones of an old giant, with the sea bed growing over them.
            Spine(kit, new Vector3(32f, DeepFloor + 0.7f, 84f), new Vector3(14f, DeepFloor + 0.9f, 92f), 0.6f, 0.7f, 2.4f, 0.3f, 0.75f);
            // A sunken chest and a lost helmet, glinting.
            var chest = kit.Group("Chest", new Vector3(-12f, DeepFloor, 54f));
            chest.rotation = Quaternion.Euler(6f, 35f, -8f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.5f, 0f), new Vector3(2f, 1f, 1.3f), DarkWood, default(Vector3), chest);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1f, -0.3f), new Vector3(1.3f, 1f, 1.3f), DarkWood, new Vector3(0f, 0f, 90f), chest);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.05f, 0.1f), new Vector3(1.7f, 0.2f, 1f), PatchworkAssets.Glow(new Color(1f, 0.8f, 0.3f), 1.6f), default(Vector3), chest));
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.7f, 0.5f, 0f), new Vector3(0.14f, 1.04f, 1.34f), Brass, default(Vector3), chest);
            PointLight(kit, new Vector3(-12f, DeepFloor + 2.2f, 54.5f), new Color(1f, 0.8f, 0.4f), 9f, 1.4f);
            for (int i = 0; i < 9; i++) Bubbles(kit, bed.At(bed.R(-W * 0.44f, W * 0.44f), 0.3f, i % 2 == 0 ? bed.R(5f, 38f) : bed.R(54f, D - 5f)), h * 0.8f, 8);

            // ----- Who lives here -----
            var whale = Swim(kit, Swimmer.Whale, new Vector3(cx, 15.5f, 60f), 25f, 30f, 4.6f, 1.5f, new Color(0.16f, 0.24f, 0.5f), new Color(0.75f, 0.85f, 0.95f), 40f);
            whale.Bob = 1.6f;
            // Stars on her back.
            var starry = new MeshBatch();
            var srng = new System.Random(3);
            for (int i = 0; i < 26; i++)
            {
                float a = (float)srng.NextDouble() * Mathf.PI - Mathf.PI * 0.5f, z = ((float)srng.NextDouble() - 0.45f) * 10f;
                float wide = 1.7f * Mathf.Sqrt(Mathf.Max(0.05f, 1f - (z / 6.6f) * (z / 6.6f)));
                starry.Ball(new Vector3(Mathf.Sin(a) * wide, 0.2f + Mathf.Cos(a) * wide * 0.92f, z), Vector3.one * (0.12f + (float)srng.NextDouble() * 0.2f), 2, 5);
            }
            starry.Build(whale.transform, "WhaleStars", PatchworkAssets.Glow(new Color(1f, 0.9f, 0.55f), 1.8f));
            Swim(kit, Swimmer.Shark, new Vector3(-6f, 10.8f, 62f), 17f, 22f, 10f, 2.2f, new Color(0.38f, 0.46f, 0.62f), CreamCol, 200f);
            Swim(kit, Swimmer.Shark, new Vector3(20f, 11f, 70f), 15f, 20f, -8f, 1.8f, new Color(0.46f, 0.44f, 0.58f), CreamCol, 20f);
            Swim(kit, Swimmer.Shark, new Vector3(8f, 7.5f, 44f), 22f, 9f, 9f, 1.5f, new Color(0.34f, 0.42f, 0.52f), CreamCol, 110f);
            Swim(kit, Swimmer.Ray, new Vector3(4f, 12f, 66f), 21f, 14f, -7f, 2.6f, new Color(0.16f, 0.2f, 0.4f), CreamCol, 300f);
            Swim(kit, Swimmer.Ray, new Vector3(18f, 6.5f, 84f), 12f, 7f, 9f, 1.8f, new Color(0.3f, 0.28f, 0.44f), CreamCol, 80f);
            Swim(kit, Swimmer.Turtle, new Vector3(-14f, 5f, 82f), 9f, 8f, 7f, 1.6f, new Color(0.36f, 0.56f, 0.42f), new Color(0.82f, 0.76f, 0.5f), 10f);
            Shoal(kit, new Vector3(-8f, 10f, 80f), 12f, 9f, 12f, 30, 0.5f, new Color(0.8f, 0.9f, 1f), 71);
            Shoal(kit, new Vector3(24f, 6f, 50f), 10f, 8f, -14f, 24, 0.45f, new Color(1f, 0.75f, 0.35f), 72, 100f);
            Shoal(kit, new Vector3(6f, 13f, 40f), 18f, 7f, 10f, 28, 0.45f, new Color(0.6f, 0.85f, 1f), 73, 200f);
            Shoal(kit, new Vector3(-16f, 3f, 48f), 8f, 6f, -16f, 20, 0.4f, new Color(1f, 0.55f, 0.6f), 74, 30f);
            Color[] cols = { new Color(1f, 0.55f, 0.2f), new Color(0.4f, 0.65f, 1f), new Color(1f, 0.85f, 0.3f), new Color(0.75f, 0.5f, 1f), new Color(0.45f, 0.9f, 0.7f) };
            for (int i = 0; i < 16; i++)
            {
                bool north = i % 2 == 0;
                Vector3 c = new Vector3(DeepX0 + 8f + (float)rng.NextDouble() * (W - 16f), 1.5f + (float)rng.NextDouble() * 6f, north ? TunnelZ + 8.6f + (float)rng.NextDouble() * 12f : TunnelZ - 8.6f - (float)rng.NextDouble() * 12f);
                Swim(kit, i % 3 == 0 ? Swimmer.Tall : Swimmer.Fish, c, 3f + (float)rng.NextDouble() * 5f, 1.4f + (float)rng.NextDouble() * 1.6f, (i % 4 < 2 ? 1f : -1f) * (14f + (float)rng.NextDouble() * 18f), 0.7f + (float)rng.NextDouble() * 0.7f,
                    cols[i % cols.Length], i % 2 == 0 ? CreamCol : new Color(0.12f, 0.14f, 0.3f), i * 41f);
            }
            // Anglers down in the dark by the sea bed, each with its little lamp.
            foreach (var p in new[] { new Vector3(30f, DeepFloor + 2.6f, 57f), new Vector3(-20f, DeepFloor + 2.2f, 75.5f) })
            {
                var ang = Swim(kit, Swimmer.Angler, p, 3.5f, 1.2f, 7f, 1.3f, new Color(0.26f, 0.2f, 0.34f), CreamCol, p.x);
                ang.Bob = 0.3f;
                var lure = NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.38f, 0.76f), Vector3.one * 0.34f, PatchworkAssets.Glow(new Color(0.6f, 1f, 0.8f), 3f), default(Vector3), ang.transform));
                var lamp = PointLight(kit, p, new Color(0.5f, 1f, 0.8f), 9f, 1.6f);
                lamp.transform.SetParent(lure, false);
                lamp.transform.localPosition = Vector3.zero;
                var pulse = ang.gameObject.AddComponent<GlowPulse>();
                pulse.Lamp = lamp;
                pulse.Low = 0.7f;
                pulse.High = 2f;
                pulse.Phase = p.x;
            }

            Tunnel(kit, DeepX0 - 1f, -2f);
            Tunnel(kit, 14f, DeepX1 + 1f);
            Bell(kit, new Vector3(6f, 0f, TunnelZ));

            // ----- Along the way -----
            kit.AddCheckpoint(new Vector3(-27f, 0f, TunnelZ), 90f);
            kit.AddCheckpoint(new Vector3(17f, 0f, TunnelZ), 90f);
            TrailLine(kit, new Vector3(-27f, 1.2f, TunnelZ), new Vector3(-5f, 1.2f, TunnelZ), 8);
            TrailLine(kit, new Vector3(17f, 1.2f, TunnelZ), new Vector3(39f, 1.2f, TunnelZ), 8);
            kit.ThreadLine(new Vector3(-24f, 1.3f, TunnelZ + 2.6f), new Vector3(-8f, 1.3f, TunnelZ + 2.6f), 5);
            kit.ThreadLine(new Vector3(20f, 1.3f, TunnelZ - 2.6f), new Vector3(36f, 1.3f, TunnelZ - 2.6f), 5);
            Plaque(kit, new Vector3(-14f, 0f, TunnelZ + 3.1f), 180f, "The Twilight Zone",
                "Sunlight only reaches about two hundred metres down. Below that the sea is dusk, and below a thousand metres it is black: the biggest dark place on Earth.",
                "So the animals down there make their own light. Most creatures of the deep can glow. Light is how they hunt, hide, and say hello.");
            Plaque(kit, new Vector3(28f, 0f, TunnelZ - 3.1f), 0f, "The Deepest Dive",
                "In 1960, Jacques Piccard and Don Walsh climbed into a steel ball called the Trieste and sank for nearly five hours, to the bottom of the Challenger Deep: almost eleven kilometres down.",
                "On the way a window cracked with a bang. They kept going. At the very bottom, in the light of their lamp, they saw a flatfish looking back at them.");
            kit.Hint(new Vector3(-24f, 1.5f, TunnelZ), new Vector3(8f, 4f, 8f), "The Deepsea Tunnel",
                "Take your time. Half way along is the Bell: bounce on the jelly cushion for the Heartstring at the top of the dome.");
        }

        // One run of the glass tunnel along z = TunnelZ, from x0 to x1: a walkway on stone
        // legs above the sea bed, arched over with glass on brass ribs.
        static void Tunnel(LevelKit kit, float x0, float x1)
        {
            const float half = 4f, rise = 5.4f;
            const int panes = 12;
            float len = x1 - x0, mx = (x0 + x1) * 0.5f;
            kit.Block(new Vector3(mx, -0.5f, TunnelZ), new Vector3(len, 1f, half * 2f + 0.6f), Navy, "TunnelFloor", PatchworkAssets.Fabric(new Color(0.1f, 0.14f, 0.3f)));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(mx, 0.02f, TunnelZ), new Vector3(len, 0.04f, 3.4f), StarCloth));
            for (float x = x0 + 3f; x < x1; x += 9f)
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(x, (DeepFloor - 1f) * 0.5f, TunnelZ), new Vector3(2.2f, -1f - DeepFloor, 7f), DarkStone);
                kit.Deco(PrimitiveType.Cube, new Vector3(x, -1.3f, TunnelZ), new Vector3(3f, 0.6f, 9f), DarkStone);
            }
            // Glass, in flat panes round a half-oval.
            System.Func<float, Vector3> P = a => new Vector3(0f, 0.15f + Mathf.Sin(a) * rise, TunnelZ + Mathf.Cos(a) * half);
            for (int i = 0; i < panes; i++)
            {
                Vector3 p0 = P(Mathf.PI * i / panes), p1 = P(Mathf.PI * (i + 1) / panes);
                Vector3 tangent = (p1 - p0).normalized;
                Vector3 normal = Vector3.Cross(Vector3.right, tangent).normalized;
                Vector3 mid = (p0 + p1) * 0.5f;
                var pane = kit.Block(new Vector3(mx, mid.y, mid.z), new Vector3(len, 0.16f, (p1 - p0).magnitude + 0.05f), Color.white, "TunnelGlass", GlassMat);
                pane.rotation = Quaternion.LookRotation(tangent, normal);
                NoShadow(pane);
            }
            // Brass ribs, and a rail along the foot of the glass each side.
            for (float x = x0 + 1f; x <= x1 - 0.5f; x += 4.5f)
                for (int i = 0; i < panes; i++)
                {
                    Vector3 p0 = P(Mathf.PI * i / panes), p1 = P(Mathf.PI * (i + 1) / panes);
                    p0.x = p1.x = x;
                    Beam(kit, p0, p1, 0.24f, Brass);
                }
            for (int s = -1; s <= 1; s += 2)
            {
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(mx, 0.14f, TunnelZ + s * (half - 0.05f)), new Vector3(len, 0.28f, 0.3f), Brass));
                // Little floor lights.
                var dots = new MeshBatch();
                for (float x = x0 + 1f; x < x1; x += 1.5f) dots.Ball(new Vector3(x, 0.06f, TunnelZ + s * 2.3f), new Vector3(0.2f, 0.08f, 0.2f), 2, 5);
                dots.Build(kit.Root, "TunnelLights", PatchworkAssets.Glow(new Color(0.4f, 0.9f, 1f), 2.2f));
            }
            for (float x = x0 + 5f; x < x1; x += 9f)
                PointLight(kit, new Vector3(x, 1.2f, TunnelZ), new Color(0.3f, 0.7f, 1f), 8f, 0.9f);
        }

        // The Bell: a round room under a glass dome in the middle of the tunnel, with
        // jellyfish drifting all round it.
        static void Bell(LevelKit kit, Vector3 c)
        {
            const float R = 9f;
            var corners = new Vector2[20];
            for (int i = 0; i < corners.Length; i++)
            {
                float a = i / (float)corners.Length * Mathf.PI * 2f;
                corners[i] = new Vector2(c.x + Mathf.Cos(a) * (R + 0.4f), c.z + Mathf.Sin(a) * (R + 0.4f));
            }
            kit.Prism(corners, -1f, 0f, PatchworkAssets.Fabric(new Color(0.1f, 0.14f, 0.3f)), "BellFloor");
            kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * (DeepFloor - 1f) * 0.5f, new Vector3(6f, (-1f - DeepFloor) * 0.5f, 6f), DarkStone);
            kit.Deco(PrimitiveType.Cylinder, c + Vector3.down * 1.3f, new Vector3(R * 2f + 1.6f, 0.3f, R * 2f + 1.6f), DarkStone);
            // A compass rose in cloth on the floor.
            NoShadow(kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.02f, new Vector3(R * 1.5f, 0.02f, R * 1.5f), StarCloth));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.03f, new Vector3(R * 0.9f, 0.02f, R * 0.9f), PlumStars));
            var gold = PatchworkAssets.Felt(new Color(1f, 0.82f, 0.4f));
            for (int i = 0; i < 4; i++)
                NoShadow(kit.Deco(PrimitiveType.Cube, c + Vector3.up * 0.045f, new Vector3(0.5f, 0.02f, R * 1.36f), gold, new Vector3(0f, i * 45f, 0f)));
            // The dome, its brass ribs, and a low glass wall round the foot of it (open at each
            // end where the tunnel comes in).
            Dome(kit, c, new Vector3(R, 7.6f, R), DomeGlass(new Color(0.6f, 0.85f, 0.95f, 0.1f)), false, "BellDome", 8, 28);
            for (int i = 0; i < 12; i++)
            {
                float az = (i + 0.5f) / 12f * Mathf.PI * 2f;
                Vector3 prev = c + new Vector3(Mathf.Cos(az) * R, 0f, Mathf.Sin(az) * R);
                for (int k = 1; k <= 8; k++)
                {
                    float el = k / 8f * Mathf.PI * 0.5f;
                    Vector3 p = c + new Vector3(Mathf.Cos(az) * Mathf.Cos(el) * R, Mathf.Sin(el) * 7.6f, Mathf.Sin(az) * Mathf.Cos(el) * R);
                    Beam(kit, prev, p, 0.18f, Brass);
                    prev = p;
                }
            }
            var crown = kit.Group("BellCrown", c + Vector3.up * 7.5f);
            Hoop(kit, crown, 1.2f, 0.2f, Brass, 14);
            float open = Mathf.Asin(4.4f / R) * Mathf.Rad2Deg;
            for (int side = 0; side < 2; side++)
            {
                float fromDeg = side * 180f + open, toDeg = side * 180f + 180f - open;
                const int segs = 8;
                for (int i = 0; i < segs; i++)
                {
                    float a = Mathf.Lerp(fromDeg, toDeg, (i + 0.5f) / segs) * Mathf.Deg2Rad;
                    float width = 2f * R * Mathf.Tan((toDeg - fromDeg) / segs * 0.5f * Mathf.Deg2Rad) + 0.1f;
                    var seg = kit.Block(c + new Vector3(Mathf.Cos(a) * R, 1.6f, Mathf.Sin(a) * R), new Vector3(width, 3.2f, 0.2f), Color.white, "BellWall", GlassMat);
                    seg.rotation = Quaternion.LookRotation(new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)), Vector3.up);
                    NoShadow(seg);
                    NoShadow(kit.Deco(PrimitiveType.Cube, c + new Vector3(Mathf.Cos(a) * R, 0.16f, Mathf.Sin(a) * R), new Vector3(width, 0.32f, 0.36f), Brass, new Vector3(0f, 90f - a * Mathf.Rad2Deg, 0f)));
                }
            }
            // Inside: the jelly cushion and the Heartstring over it, benches, and someone to talk to.
            kit.AddBouncePad(c, 16f);
            Level1Builder.MakeHeartstring(kit, "L2_hs_bell", "The Bell", c + Vector3.up * 5f, false);
            ScoreBubbles.Place(kit, c + Vector3.up * 2.6f, 50);
            for (int i = 0; i < 4; i++)
            {
                float a = 45f + i * 90f;
                TownKit.Bench(kit, c + Rot(a) * new Vector3(0f, 0f, R - 1.5f), a + 180f);
            }
            PointLight(kit, c + Vector3.up * 3f, new Color(0.45f, 0.6f, 1f), 13f, 1.1f);
            Plaque(kit, c + new Vector3(-3.4f, 0f, 5.6f), 150f, "Jellyfish",
                "No brain, no bones, no heart, and nearly all water: and yet jellyfish have drifted through the sea for more than five hundred million years. They are older than trees. They are older than fish.",
                "One kind can even grow young again when it's hurt, and start its life over from the beginning.");
            Person(kit, c + new Vector3(4.2f, 0f, -5.2f), 320f, ToyNpc.Kind.Teddy, "Bubbles", new Color(0.92f, 0.75f, 0.4f), new Color(0.3f, 0.5f, 0.8f), 0.95f, () => new[]
            {
                "Shh. Listen. ...That's her. That's the whale.",
                "I'm the diver. I go in on Tuesdays and wipe the inside of the glass. The sharks don't mind. The turtle supervises.",
                "The Sunken Ruins are at the far end of the tunnel. Mind your step in there: it's more puddle than floor."
            });
            // Outside: jellyfish, all round and above.
            Color[] jc = { new Color(1f, 0.5f, 0.85f), new Color(0.55f, 0.75f, 1f), new Color(0.75f, 0.55f, 1f), new Color(0.5f, 1f, 0.9f) };
            for (int i = 0; i < 22; i++)
            {
                float a = i * 137.5f * Mathf.Deg2Rad;
                float r = R + 2.5f + (i % 5) * 1.5f;
                Vector3 p = c + new Vector3(Mathf.Cos(a) * r, 1f + (i % 7) * 1.5f, Mathf.Sin(a) * r);
                if (Mathf.Abs(p.z - c.z) < 5.5f && Mathf.Abs(p.x - c.x) > R) p.y += 6.5f;   // not through the tunnel
                Jelly(kit, p, 0.8f + (i % 3) * 0.35f, jc[i % jc.Length], i * 1.3f, i % 6 == 0);
            }
            for (int i = 0; i < 5; i++)
                Jelly(kit, c + new Vector3(Mathf.Cos(i * 1.26f) * 4f, 10f + i * 0.8f, Mathf.Sin(i * 1.26f) * 4f), 1.2f, jc[(i + 1) % jc.Length], i * 2.1f, i == 0);
        }
    }
}

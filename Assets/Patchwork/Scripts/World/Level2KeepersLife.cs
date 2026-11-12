using UnityEngine;

namespace Patchwork
{
    // The second, finer layer of Behind the Glass: the things that make it a place people
    // work in, and the tanks places things live in.
    //   The Keepers' Room - tiled walls, a clock, a sink with a dripping tap, raincoats on
    //                       hooks, a first-aid box and the fuse box, the tide tables, pipes
    //   The Tank Hall     - drains and puddles, a hose left running into the reef, a hoist
    //                       on its rail with a net of fish, crates of live fish, the big pump
    //                       thumping away on the north walk, filters pouring back into the
    //                       tanks, banners over each tank, and two more keepers at work
    //   The tanks         - garden eels that duck into the sand when you come near, the
    //                       octopus in its pot (with the missing biscuits), a beluga and a
    //                       shoal of cod under the ice, a manatee in the Mangrove, a giant
    //                       isopod and glowing sea pens in the Lantern Deep
    // Also out in the Grand Atrium: a bust of Philip Henry Gosse, the man who named the
    // aquarium, with the visitors' book and a donation box near the doors.
    public static partial class Level2Builder
    {
        static void KwLife(LevelKit kit)
        {
            KwRoomLife(kit);
            KwHallLife(kit);
            KwTankLife(kit);
            KwSounds(kit);
            KwAir(kit);
            KwTankWalls(kit);
            KwNoticeBoard(kit);
            KwUndercroftLife(kit);
            AtriumFounder(kit);
            HallAir(kit);
        }

        // ================================================================ Light and air

        static void KwAir(LevelKit kit)
        {
            // Ripples of light thrown up off each tank's surface onto the roof, the way the
            // ceiling over a real pool always dances.
            foreach (var t in new[] { CoralTank, LanternTank, FrozenTank, MangroveTank })
            {
                float cx = (t[0] + t[1]) * 0.5f, cz = (t[2] + t[3]) * 0.5f;
                bool dark = t == LanternTank;
                Color c = t == CoralTank ? new Color(0.5f, 0.9f, 1f) : t == FrozenTank ? new Color(0.75f, 0.88f, 1f) : t == MangroveTank ? new Color(0.65f, 0.95f, 0.6f) : new Color(0.35f, 0.45f, 0.9f);
                SpotLight(kit, new Vector3(cx, -0.2f, cz), new Vector3(cx, KwTop, cz), c, 30f, 95f, dark ? 70f : 190f, false, true);
            }
            // And through the Undercroft's windows: the reef's ripples on one wall, the deep's
            // faint blue on the other.
            SpotLight(kit, new Vector3(-71f, -5.6f, 116.5f), new Vector3(-58f, -6.2f, 116.5f), new Color(0.5f, 0.9f, 1f), 16f, 80f, 45f, false, true);
            SpotLight(kit, new Vector3(-49f, -5.6f, 116.5f), new Vector3(-62f, -6.2f, 116.5f), new Color(0.35f, 0.5f, 1f), 16f, 80f, 18f, false, true);
            // Specks of dust in the lamplight over the hall and the Keepers' Room.
            Motes(kit, new Vector3(-60f, 9f, 136f), new Vector3(50f, 14f, 78f), 120, new Color(1f, 0.9f, 0.7f), 0.8f, 0.05f, new Vector3(0.03f, -0.02f, 0.02f));
            Motes(kit, new Vector3(-60f, 3.5f, 86f), new Vector3(50f, 6f, 15f), 40, new Color(1f, 0.9f, 0.7f), 0.8f, 0.04f, new Vector3(0.02f, -0.01f, 0.02f));
        }

        // Dust in the air of the bigger rooms that didn't have any.
        static void HallAir(LevelKit kit)
        {
            Motes(kit, new Vector3(80f, 8f, -24f), new Vector3(38f, 14f, 54f), 90, new Color(1f, 0.88f, 0.7f), 0.8f, 0.05f, new Vector3(0.03f, -0.02f, 0.02f));   // the Hall of Arms
            Motes(kit, new Vector3(36f, DeckY + 3.4f, -25.5f), new Vector3(5f, 5f, 30f), 30, new Color(0.8f, 0.85f, 1f), 1.2f, 0.04f, new Vector3(0.01f, -0.02f, 0.03f));   // the Night Sky Gallery
            Motes(kit, new Vector3(46.5f, DeckY + 3.4f, -13f), new Vector3(14f, 5f, 5f), 20, new Color(1f, 0.9f, 0.7f), 1.2f, 0.04f, new Vector3(0.03f, -0.01f, 0.01f));    // the Planet Walk
        }

        // ================================================================ Dressing the tanks' walls

        static void KwTankWalls(LevelKit kit)
        {
            // The Frozen Sea: rock along the foot of every wall, and a crust of frost and hanging
            // ice along the top, just under the water.
            {
                var T = FrozenTank;
                float x0 = T[0], x1 = T[1], z0 = T[2], z1 = T[3], bed = T[4];
                var rng = new System.Random(7171);
                var rock = new MeshBatch();
                var frost = new MeshBatch();
                for (float z = z0 + 1f; z < z1 - 1f; z += 2.2f)
                    foreach (float x in new[] { x0 + 0.5f, x1 - 0.5f })
                    {
                        if (x > -64f && z > 152.5f && z < 157.5f) continue;    // (the pipe to the Mangrove)
                        if (x > -71f && z > 167f) continue;                     // (the ice cave)
                        KwRocks(rock, new Vector3(x, bed, z), 0.9f + (float)rng.NextDouble() * 0.9f, rng);
                        frost.Ball(new Vector3(x + (x < -70f ? -0.15f : 0.15f), -1.7f, z), new Vector3(0.7f, 0.9f + (float)rng.NextDouble() * 0.8f, 1.6f), 3, 6);
                    }
                for (float x = x0 + 1f; x < x1 - 1f; x += 2.2f)
                    foreach (float z in new[] { z0 + 0.5f, z1 - 0.5f })
                    {
                        if (z < 140f && Mathf.Abs(x + 75f) < 2.5f) continue;   // (the pipe to the Coral Nursery)
                        if (z > 170f && x > -71f) continue;
                        KwRocks(rock, new Vector3(x, bed, z), 0.9f + (float)rng.NextDouble() * 0.9f, rng);
                        frost.Ball(new Vector3(x, -1.7f, z + (z < 150f ? -0.15f : 0.15f)), new Vector3(1.6f, 0.9f + (float)rng.NextDouble() * 0.8f, 0.7f), 3, 6);
                    }
                rock.Build(kit.Root, "FrozenWallRocks", PatchworkAssets.Felt(new Color(0.38f, 0.42f, 0.52f)));
                Twinkling(frost, kit.Root, "FrostCrust", new Color(0.82f, 0.93f, 1f), 0.4f, 0.7f, 0.3f, 0.7f);
            }
            // The Mangrove: dark mud banked up the walls, roots worming down into the water,
            // and green weed on the mud.
            {
                var T = MangroveTank;
                float x0 = T[0], x1 = T[1], z0 = T[2], z1 = T[3], bed = T[4];
                var rng = new System.Random(7272);
                var mud = new MeshBatch();
                var weed = new MeshBatch();
                var rootlets = new MeshBatch();
                for (float z = z0 + 1f; z < z1 - 1f; z += 2f)
                    foreach (float x in new[] { x0 + 0.5f, x1 - 0.5f })
                    {
                        if (x < -55f && z > 152.5f && z < 157.5f) continue;    // (the pipe to the Frozen Sea)
                        if (x < -55f && z > 165.5f) continue;                   // (the bank and its hollow)
                        if (x > -37f && z < 141.5f) continue;                   // (the other bank)
                        mud.Ball(new Vector3(x, bed + 0.4f, z), new Vector3(1.4f, 1.6f + (float)rng.NextDouble(), 2.2f), 3, 6);
                        if (rng.NextDouble() < 0.7) weed.Ball(new Vector3(x + (x < -46f ? 0.5f : -0.5f), bed + 1.4f, z), new Vector3(0.4f, 0.6f, 0.8f), 2, 5);
                        for (int k = 0; k < 2; k++)
                        {
                            float zz = z + ((float)rng.NextDouble() - 0.5f) * 1.6f, len = 1.5f + (float)rng.NextDouble() * 3.5f;
                            rootlets.Box(new Vector3(x + (x < -46f ? 0.06f : -0.06f), -0.8f - len * 0.5f, zz), new Vector3(0.06f, len, 0.06f), Quaternion.Euler(((float)rng.NextDouble() - 0.5f) * 10f, 0f, ((float)rng.NextDouble() - 0.5f) * 10f));
                        }
                    }
                for (float x = x0 + 1f; x < x1 - 1f; x += 2f)
                    foreach (float z in new[] { z0 + 0.5f, z1 - 0.5f })
                    {
                        if (z < 140f && Mathf.Abs(x + 46.5f) < 2.5f) continue;   // (the pipe to the Lantern Deep)
                        if (z > 170f && x < -49.5f) continue;
                        if (z < 140f && x > -42.5f) continue;
                        mud.Ball(new Vector3(x, bed + 0.4f, z), new Vector3(2.2f, 1.6f + (float)rng.NextDouble(), 1.4f), 3, 6);
                        if (rng.NextDouble() < 0.7) weed.Ball(new Vector3(x, bed + 1.4f, z + (z < 150f ? 0.5f : -0.5f)), new Vector3(0.8f, 0.6f, 0.4f), 2, 5);
                    }
                mud.Build(kit.Root, "MangroveWallMud", PatchworkAssets.Felt(new Color(0.3f, 0.25f, 0.18f)));
                Twinkling(weed, kit.Root, "MangroveWeed", new Color(0.4f, 0.65f, 0.3f), 0.25f, 0.45f, 0.3f, 0.2f);
                rootlets.Build(kit.Root, "WallRoots", PatchworkAssets.Wood(new Color(0.42f, 0.32f, 0.24f)));
            }
            // The Lantern Deep: a few long cracks in the rock glowing faintly blue, high and low.
            {
                var T = LanternTank;
                var cracks = new MeshBatch();
                var rng = new System.Random(7373);
                for (int i = 0; i < 14; i++)
                {
                    bool eastWall = i % 2 == 0;
                    float z = T[2] + 2f + (float)rng.NextDouble() * (T[3] - T[2] - 4f);
                    float y = T[4] + 2f + (float)rng.NextDouble() * 14f;
                    float x = eastWall ? T[1] - 0.04f : T[0] + 0.04f;
                    if (!eastWall && y > -8f && z > 108f && z < 125f) continue;   // (the Undercroft windows)
                    float len = 1.2f + (float)rng.NextDouble() * 2.6f;
                    cracks.Box(new Vector3(x, y, z), new Vector3(0.04f, len, 0.08f), Quaternion.Euler(((float)rng.NextDouble() - 0.5f) * 60f, 0f, 0f));
                }
                Twinkling(cracks, kit.Root, "DeepCracks", new Color(0.35f, 0.75f, 1f), 0.2f, 1.6f, 0.6f, 0.9f);
            }
        }

        // The Undercroft: handrails down the stair, pipes along the ceiling, and a framed chart
        // of who's who between each pair of windows.
        static void KwUndercroftLife(LevelKit kit)
        {
            foreach (float x in new[] { -61.9f, -58.1f })
            {
                Vector3 a = new Vector3(x, 0.9f, 98.4f), b = new Vector3(x, -7.1f, 108.2f);
                Beam(kit, a, b, 0.08f, Brass);
                for (int i = 0; i <= 4; i++)
                {
                    Vector3 p = Vector3.Lerp(a, b, i / 4f);
                    Beam(kit, p, p + new Vector3(x < -60f ? -0.1f : 0.1f, -0.15f, 0f), 0.05f, IronMat);
                }
                // and on along the corridor at the same height
                Beam(kit, new Vector3(x, -7.1f, 108.2f), new Vector3(x, -7.1f, 109.2f), 0.08f, Brass);
            }
            var pipe = PatchworkAssets.Plain(new Color(0.55f, 0.42f, 0.3f), 0.6f);
            foreach (float x in new[] { -61.5f, -58.5f })
            {
                NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(x, -3.8f, 117f), new Vector3(0.2f, 9f, 0.2f), pipe, new Vector3(90f, 0f, 0f)));
                for (float z = 109f; z < 126f; z += 3f)
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(x, -3.62f, z), new Vector3(0.06f, 0.24f, 0.06f), IronMat));
            }
            // The charts: a cream card in a dark frame with little felt fish on it.
            Color[][] fish =
            {
                new[] { new Color(1f, 0.5f, 0.12f), new Color(0.25f, 0.45f, 1f), new Color(1f, 0.88f, 0.25f), new Color(0.3f, 0.8f, 0.6f) },
                new[] { new Color(0.35f, 0.75f, 1f), new Color(0.2f, 0.18f, 0.25f), new Color(0.45f, 0.08f, 0.12f), new Color(1f, 0.7f, 0.7f) },
            };
            int n = 0;
            foreach (float x in new[] { -61.97f, -58.03f })
                foreach (float z in new[] { 113.75f, 119.25f })
                {
                    float into = x < -60f ? 1f : -1f;
                    var cols = fish[x < -60f ? 0 : 1];
                    Vector3 c = new Vector3(x, -6.4f, z);
                    NoShadow(kit.Deco(PrimitiveType.Cube, c, new Vector3(0.04f, 0.95f, 1.3f), DarkWood));
                    NoShadow(kit.Deco(PrimitiveType.Cube, c + Vector3.right * into * 0.025f, new Vector3(0.02f, 0.82f, 1.16f), PatchworkAssets.Felt(CreamCol)));
                    for (int i = 0; i < 4; i++)
                    {
                        Vector3 fp = c + Vector3.right * into * 0.04f + new Vector3(0f, 0.18f - (i / 2) * 0.36f, -0.28f + (i % 2) * 0.56f);
                        NoShadow(kit.Deco(PrimitiveType.Sphere, fp, new Vector3(0.02f, 0.14f, 0.3f), PatchworkAssets.Felt(cols[(i + n) % 4])));
                        NoShadow(kit.Deco(PrimitiveType.Cube, fp + new Vector3(0f, 0f, -0.18f), new Vector3(0.02f, 0.12f, 0.08f), PatchworkAssets.Felt(cols[(i + n) % 4]), new Vector3(45f, 0f, 0f)));
                        NoShadow(kit.Deco(PrimitiveType.Cube, fp + new Vector3(0f, -0.12f, 0f), new Vector3(0.01f, 0.015f, 0.26f), PatchworkAssets.Felt(new Color(0.3f, 0.3f, 0.35f))));
                    }
                    n++;
                }
        }

        // A corkboard in the Keepers' Room with snapshots of the residents, and "keeper of the
        // month" (who is, again, Gerald the penguin).
        static void KwNoticeBoard(LevelKit kit)
        {
            Vector3 c = new Vector3(-54f, 2.9f, 78.06f);
            kit.Deco(PrimitiveType.Cube, c, new Vector3(3f, 2f, 0.08f), DarkWood);
            NoShadow(kit.Deco(PrimitiveType.Cube, c + new Vector3(0f, 0f, 0.05f), new Vector3(2.8f, 1.8f, 0.03f), PatchworkAssets.Felt(new Color(0.7f, 0.55f, 0.38f))));
            Color[] snaps = { new Color(0.3f, 0.7f, 0.85f), new Color(0.2f, 0.25f, 0.5f), new Color(0.85f, 0.9f, 1f), new Color(0.35f, 0.6f, 0.35f), new Color(0.95f, 0.55f, 0.3f), new Color(0.55f, 0.5f, 0.75f) };
            var white = PatchworkAssets.Felt(new Color(0.97f, 0.96f, 0.92f));
            var pinMat = PatchworkAssets.Felt(new Color(0.9f, 0.25f, 0.25f));
            for (int i = 0; i < 6; i++)
            {
                Vector3 p = c + new Vector3(-1.05f + (i % 3) * 1.05f, i < 3 ? 0.4f : -0.45f, 0.075f);
                float tilt = (i * 37 % 13) - 6f;
                NoShadow(kit.Deco(PrimitiveType.Cube, p, new Vector3(0.62f, 0.7f, 0.01f), white, new Vector3(0f, 0f, tilt)));
                NoShadow(kit.Deco(PrimitiveType.Cube, p + new Vector3(0f, 0.06f, 0.006f), new Vector3(0.52f, 0.46f, 0.01f), PatchworkAssets.Felt(snaps[i]), new Vector3(0f, 0f, tilt)));
                // A little shape of who's in it: a fish, a penguin, a jelly...
                NoShadow(kit.Deco(PrimitiveType.Sphere, p + new Vector3(0f, 0.06f, 0.012f), new Vector3(i % 2 == 0 ? 0.26f : 0.16f, i % 2 == 0 ? 0.14f : 0.26f, 0.01f), PatchworkAssets.Felt(Color.Lerp(snaps[(i + 3) % 6], Color.white, 0.3f)), new Vector3(0f, 0f, tilt)));
                NoShadow(kit.Deco(PrimitiveType.Sphere, p + new Vector3(0f, 0.32f, 0.015f), Vector3.one * 0.06f, pinMat));
            }
            // Keeper of the Month: a rosette, and Gerald.
            Vector3 r = c + new Vector3(1.75f, 0.2f, 0.07f);
            for (int k = 0; k < 2; k++)
                NoShadow(kit.Deco(PrimitiveType.Cube, r + new Vector3((k - 0.5f) * 0.1f, -0.3f, 0f), new Vector3(0.1f, 0.4f, 0.01f), PatchworkAssets.Felt(new Color(0.25f, 0.4f, 0.85f)), new Vector3(0f, 0f, (k - 0.5f) * 20f)));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, r, new Vector3(0.34f, 0.01f, 0.34f), PatchworkAssets.Felt(new Color(1f, 0.8f, 0.3f)), new Vector3(90f, 0f, 0f)));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, r + new Vector3(0f, 0f, 0.01f), new Vector3(0.22f, 0.01f, 0.22f), PatchworkAssets.Felt(new Color(0.25f, 0.4f, 0.85f)), new Vector3(90f, 0f, 0f)));
            var g = KwPenguin(kit, c + new Vector3(1.75f, -0.75f, 0.12f), 180f, 0.32f, false);
            g.rotation = Quaternion.identity;
            var lbl = new MeshBatch();
            Letters(lbl, "KEEPER OF THE MONTH", c + new Vector3(0f, 1.15f, 0.04f), Vector3.forward, 0.026f, 0.006f);
            lbl.Build(kit.Root, "BoardTitle", PatchworkAssets.Felt(new Color(0.95f, 0.92f, 0.85f)));
        }

        // ================================================================ The Keepers' Room

        static void KwRoomLife(LevelKit kit)
        {
            // White tiles up to shoulder height on the long walls, with a teal edge on top.
            var tile = PatchworkAssets.Plain(new Color(0.9f, 0.92f, 0.9f), 0.7f);
            var grout = new MeshBatch();
            var edge = new MeshBatch();
            System.Action<float, float, float, bool> tiles = (a, b, plane, alongX) =>
            {
                // plane: the wall's face (z for walls along x, x for walls along z); the tiles
                // sit 2 cm in front of it, facing into the room.
                float mid = (a + b) * 0.5f, len = b - a;
                float inward = alongX ? (plane < 86f ? 1f : -1f) : (plane < -60f ? 1f : -1f);
                float face = plane + inward * 0.02f;
                Vector3 c = alongX ? new Vector3(mid, 0.65f, face) : new Vector3(face, 0.65f, mid);
                NoShadow(kit.Deco(PrimitiveType.Cube, c, alongX ? new Vector3(len, 1.3f, 0.03f) : new Vector3(0.03f, 1.3f, len), tile));
                for (float y = 0.3f; y < 1.3f; y += 0.3f)
                    grout.Box(alongX ? new Vector3(mid, y, face + inward * 0.018f) : new Vector3(face + inward * 0.018f, y, mid), alongX ? new Vector3(len, 0.02f, 0.01f) : new Vector3(0.01f, 0.02f, len), Quaternion.identity);
                for (float u = a + 0.3f; u < b - 0.05f; u += 0.3f)
                    grout.Box(alongX ? new Vector3(u, 0.65f, face + inward * 0.018f) : new Vector3(face + inward * 0.018f, 0.65f, u), alongX ? new Vector3(0.02f, 1.3f, 0.01f) : new Vector3(0.01f, 1.3f, 0.02f), Quaternion.identity);
                edge.Box(alongX ? new Vector3(mid, 1.34f, face + inward * 0.03f) : new Vector3(face + inward * 0.03f, 1.34f, mid), alongX ? new Vector3(len, 0.1f, 0.07f) : new Vector3(0.07f, 0.1f, len), Quaternion.identity);
            };
            tiles(KwX0 + 1.2f, -62.9f, KwZ0, true);     // the south wall, either side of the door in from the Kelp Galleries
            tiles(-57.1f, KwX1, KwZ0, true);
            tiles(KwX0 + 1.2f, -63.4f, 94f, true);      // the north wall, either side of the door to the Tank Hall
            tiles(-56.6f, KwX1, 94f, true);
            tiles(KwZ0, 94f, KwX1, false);              // the east wall
            grout.Build(kit.Root, "TileGrout", PatchworkAssets.Plain(new Color(0.62f, 0.66f, 0.66f), 0.2f));
            edge.Build(kit.Root, "TileEdge", PatchworkAssets.Plain(new Color(0.2f, 0.5f, 0.5f), 0.7f));

            // A clock over the north wall, and the tide tables pinned beside the door.
            WallClock(kit, new Vector3(-70f, 4.4f, 93.95f), 180f, 0.6f, 7);
            Vector3 tide = new Vector3(-48f, 3.1f, 93.96f);
            NoShadow(kit.Deco(PrimitiveType.Cube, tide, new Vector3(1.8f, 2.2f, 0.03f), PatchworkAssets.Felt(new Color(0.94f, 0.92f, 0.84f))));
            var lines = new MeshBatch();
            Letters(lines, "TIDES", tide + new Vector3(0f, 0.85f, -0.03f), Vector3.back, 0.04f, 0.005f);
            for (int i = 0; i < 8; i++) lines.Box(tide + new Vector3(0f, 0.5f - i * 0.17f, -0.025f), new Vector3(1.5f, 0.015f, 0.005f), Quaternion.identity);
            for (int i = 0; i < 4; i++) lines.Box(tide + new Vector3(-0.6f + i * 0.4f, -0.1f, -0.025f), new Vector3(0.015f, 1.4f, 0.005f), Quaternion.identity);
            lines.Build(kit.Root, "TideTable", PatchworkAssets.Felt(new Color(0.25f, 0.3f, 0.45f)));
            foreach (var pin in new[] { new Vector3(-0.8f, 1f, 0f), new Vector3(0.8f, 1f, 0f), new Vector3(-0.8f, -1f, 0f), new Vector3(0.8f, -1f, 0f) })
                NoShadow(kit.Deco(PrimitiveType.Sphere, tide + pin + new Vector3(0f, 0f, -0.03f), Vector3.one * 0.07f, PatchworkAssets.Felt(new Color(0.9f, 0.25f, 0.25f))));

            // Yellow raincoats and sou'westers on hooks by the door.
            var yellow = PatchworkAssets.Felt(new Color(1f, 0.82f, 0.2f));
            for (int i = 0; i < 3; i++)
            {
                float x = -67.4f + i * 1.3f;
                var g = kit.Group("Raincoat", new Vector3(x, 0f, 93.9f));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 3.1f, -0.06f), new Vector3(0.06f, 0.06f, 0.14f), Brass, default(Vector3), g));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 2.3f, -0.14f), new Vector3(0.75f, 1.4f, 0.12f), yellow, new Vector3(-3f, 0f, (i - 1) * 3f), g));
                for (int s = -1; s <= 1; s += 2)
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.45f, 2.35f, -0.16f), new Vector3(0.2f, 1.2f, 0.1f), yellow, new Vector3(0f, 0f, s * 8f), g));
                if (i != 1)
                {
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 3.25f, -0.2f), new Vector3(0.5f, 0.24f, 0.5f), yellow, default(Vector3), g));
                    NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 3.17f, -0.2f), new Vector3(0.7f, 0.02f, 0.7f), yellow, default(Vector3), g));
                }
                // Wellies under it.
                for (int s = -1; s <= 1; s += 2)
                {
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.16f, 0.3f, -0.4f), new Vector3(0.2f, 0.3f, 0.24f), PatchworkAssets.Felt(new Color(0.15f, 0.35f, 0.2f)), default(Vector3), g);
                    kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.16f, 0.05f, -0.48f), new Vector3(0.2f, 0.1f, 0.36f), PatchworkAssets.Felt(new Color(0.15f, 0.35f, 0.2f)), default(Vector3), g);
                }
            }

            // A deep sink on the south wall, with a mirror and a tap that won't stop dripping.
            Vector3 sk = new Vector3(-67.5f, 0f, 78f);
            var ceramic = PatchworkAssets.Plain(new Color(0.95f, 0.95f, 0.93f), 0.8f);
            kit.Block(sk + new Vector3(0f, 0.95f, 0.45f), new Vector3(1.5f, 0.35f, 0.85f), StoneCol, "Sink", ceramic);
            NoShadow(kit.Deco(PrimitiveType.Cube, sk + new Vector3(0f, 1.1f, 0.45f), new Vector3(1.3f, 0.08f, 0.65f), PatchworkAssets.Plain(new Color(0.3f, 0.4f, 0.42f), 0.6f)));
            kit.Deco(PrimitiveType.Cube, sk + new Vector3(0f, 0.4f, 0.45f), new Vector3(0.4f, 0.8f, 0.4f), ceramic);
            var chrome = PatchworkAssets.Plain(new Color(0.85f, 0.87f, 0.9f), 0.9f);
            kit.Deco(PrimitiveType.Cylinder, sk + new Vector3(0f, 1.45f, 0.12f), new Vector3(0.08f, 0.3f, 0.08f), chrome);
            kit.Deco(PrimitiveType.Cube, sk + new Vector3(0f, 1.72f, 0.3f), new Vector3(0.07f, 0.07f, 0.4f), chrome);
            kit.Deco(PrimitiveType.Cube, sk + new Vector3(0f, 1.64f, 0.48f), new Vector3(0.07f, 0.12f, 0.07f), chrome);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, sk + new Vector3(s * 0.22f, 1.75f, 0.15f), new Vector3(0.14f, 0.04f, 0.14f), PatchworkAssets.Felt(s < 0 ? new Color(0.85f, 0.2f, 0.2f) : new Color(0.25f, 0.45f, 0.9f)));
            var drop = NoShadow(kit.Deco(PrimitiveType.Sphere, sk + new Vector3(0f, 1.55f, 0.48f), Vector3.one * 0.06f, PatchworkAssets.Glow(new Color(0.7f, 0.9f, 1f), 0.8f)));
            var dl = drop.gameObject.AddComponent<DripLoop>();
            dl.Fall = 0.42f;
            dl.Period = 2.6f;
            kit.Deco(PrimitiveType.Cube, sk + new Vector3(0f, 2.6f, 0.04f), new Vector3(1.1f, 1.2f, 0.05f), DarkWood);
            NoShadow(kit.Deco(PrimitiveType.Cube, sk + new Vector3(0f, 2.6f, 0.07f), new Vector3(0.95f, 1.05f, 0.02f), PatchworkAssets.Plain(new Color(0.55f, 0.65f, 0.72f), 0.95f)));
            NoShadow(kit.Deco(PrimitiveType.Cube, sk + new Vector3(0.8f, 2.1f, 0.06f), new Vector3(0.05f, 0.05f, 0.08f), Brass));
            NoShadow(kit.Deco(PrimitiveType.Cube, sk + new Vector3(0.8f, 1.8f, 0.1f), new Vector3(0.36f, 0.6f, 0.03f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.9f, 0.88f, 0.8f), new Color(0.3f, 0.5f, 0.8f))));

            // The first-aid box and the fuse box on the east wall.
            Vector3 fa = new Vector3(-34.08f, 2.6f, 80.2f);
            kit.Deco(PrimitiveType.Cube, fa, new Vector3(0.16f, 0.7f, 0.9f), PatchworkAssets.Plain(new Color(0.95f, 0.95f, 0.95f), 0.6f));
            NoShadow(kit.Deco(PrimitiveType.Cube, fa + new Vector3(-0.09f, 0f, 0f), new Vector3(0.02f, 0.42f, 0.12f), PatchworkAssets.Felt(new Color(0.85f, 0.15f, 0.15f))));
            NoShadow(kit.Deco(PrimitiveType.Cube, fa + new Vector3(-0.09f, 0f, 0f), new Vector3(0.02f, 0.12f, 0.42f), PatchworkAssets.Felt(new Color(0.85f, 0.15f, 0.15f))));
            Vector3 fb = new Vector3(-34.12f, 3f, 84f);
            kit.Deco(PrimitiveType.Cube, fb, new Vector3(0.24f, 1.2f, 0.9f), PatchworkAssets.Plain(new Color(0.35f, 0.38f, 0.36f), 0.4f));
            NoShadow(kit.Deco(PrimitiveType.Cube, fb + new Vector3(-0.2f, 0.1f, 0.2f), new Vector3(0.16f, 0.06f, 0.06f), PatchworkAssets.Felt(new Color(0.15f, 0.15f, 0.15f)), new Vector3(0f, 0f, -35f)));
            NoShadow(kit.Deco(PrimitiveType.Sphere, fb + new Vector3(-0.27f, 0.15f, 0.2f), Vector3.one * 0.09f, PatchworkAssets.Felt(new Color(0.85f, 0.15f, 0.15f))));
            NoShadow(kit.Deco(PrimitiveType.Cube, fb + new Vector3(-0.13f, -0.35f, -0.15f), new Vector3(0.02f, 0.2f, 0.36f), PatchworkAssets.Felt(new Color(1f, 0.85f, 0.2f))));
            NoShadow(kit.Deco(PrimitiveType.Sphere, fb + new Vector3(-0.14f, 0.45f, -0.25f), Vector3.one * 0.06f, PatchworkAssets.Glow(new Color(0.3f, 1f, 0.4f), 2f)));
            for (int i = 0; i < 3; i++)
                NoShadow(kit.Deco(PrimitiveType.Cylinder, fb + new Vector3(0f, 0.6f + i * 0.9f, 0.3f - i * 0.15f), new Vector3(0.05f, 0.5f, 0.05f), IronMat));

            // Pipes along the ceiling, on brackets.
            var pipe = PatchworkAssets.Plain(new Color(0.55f, 0.42f, 0.3f), 0.6f);
            foreach (float z in new[] { 80f, 92.6f })
            {
                NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(-61f, 7.4f, z), new Vector3(0.24f, 26.5f, 0.24f), pipe, new Vector3(0f, 0f, 90f)));
                for (float x = KwX0 + 2f; x < KwX1; x += 4f)
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(x, 7.7f, z), new Vector3(0.08f, 0.6f, 0.08f), IronMat));
            }
        }

        // ================================================================ The Tank Hall

        static void KwHallLife(LevelKit kit)
        {
            // Floor drains with grates, and puddles by the tanks.
            var dark = PatchworkAssets.Plain(new Color(0.08f, 0.09f, 0.1f), 0.3f);
            var bars = new MeshBatch();
            foreach (var d in new[] { new Vector3(-82f, 0f, 96.4f), new Vector3(-53f, 0f, 96.2f), new Vector3(-82f, 0f, 130.6f), new Vector3(-50f, 0f, 127.6f), new Vector3(-70f, 0f, 176.6f), new Vector3(-45f, 0f, 176.4f) })
            {
                NoShadow(kit.Deco(PrimitiveType.Cylinder, d + Vector3.up * 0.02f, new Vector3(0.7f, 0.006f, 0.7f), dark));
                for (int i = -2; i <= 2; i++) bars.Box(d + new Vector3(i * 0.12f, 0.032f, 0f), new Vector3(0.04f, 0.012f, 0.6f), Quaternion.identity);
                NoShadow(kit.Deco(PrimitiveType.Cylinder, d + Vector3.up * 0.026f, new Vector3(0.76f, 0.004f, 0.76f), IronMat));
            }
            bars.Build(kit.Root, "DrainGrates", IronMat);
            var puddles = new MeshBatch();
            var prng = new System.Random(9191);
            foreach (var p in new[] { new Vector3(-70f, 0f, 97.3f), new Vector3(-79.5f, 0f, 96.8f), new Vector3(-43f, 0f, 97.2f), new Vector3(-70f, 0f, 127f), new Vector3(-40f, 0f, 133f), new Vector3(-82.6f, 0f, 133f), new Vector3(-60f, 0f, 175.8f), new Vector3(-87f, 0f, 150f) })
                for (int k = 0; k < 3; k++)
                    puddles.Ball(p + new Vector3(((float)prng.NextDouble() - 0.5f) * 1.6f, 0.022f, ((float)prng.NextDouble() - 0.5f) * 0.6f), new Vector3(0.6f + (float)prng.NextDouble() * 1.2f, 0.008f, 0.4f + (float)prng.NextDouble() * 0.6f), 2, 8);
            puddles.Build(kit.Root, "Puddles", PatchworkAssets.Glow(new Color(0.35f, 0.5f, 0.6f), 0.25f));

            // A hose from the tap on the west wall, left running into the Coral Nursery.
            var hose = PatchworkAssets.Felt(new Color(0.2f, 0.55f, 0.3f));
            Vector3 tap = new Vector3(KwX0 + 0.1f, 1f, 96.2f);
            NoShadow(kit.Deco(PrimitiveType.Cube, tap, new Vector3(0.2f, 0.2f, 0.2f), Brass));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, tap + new Vector3(0.12f, 0.18f, 0f), new Vector3(0.24f, 0.03f, 0.24f), PatchworkAssets.Felt(new Color(0.8f, 0.2f, 0.2f))));
            Vector3[] path =
            {
                tap + new Vector3(0.15f, -0.1f, 0f), new Vector3(-87.4f, 0.06f, 96.4f), new Vector3(-86.2f, 0.06f, 95.8f), new Vector3(-84.6f, 0.06f, 96.6f),
                new Vector3(-83f, 0.06f, 96.1f), new Vector3(-81.8f, 0.06f, 97.1f), new Vector3(-81.2f, 0.3f, 97.75f), new Vector3(-81f, 0.1f, 98.2f), new Vector3(-80.9f, -0.5f, 98.5f)
            };
            for (int i = 0; i + 1 < path.Length; i++) Beam(kit, path[i], path[i + 1], 0.1f, hose);
            Bubbles(kit, new Vector3(-80.9f, -1.2f, 98.6f), 0.6f, 5);
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(-80.9f, -0.5f, 98.55f), new Vector3(0.6f, 0.04f, 0.6f), PatchworkAssets.Glow(new Color(0.8f, 0.95f, 1f), 0.6f)));

            // The hoist: a trolley on a rail under the roof, with a net of fish on its hook.
            Vector3 rail0 = new Vector3(-86f, 13f, 96.6f), rail1 = new Vector3(-36f, 13f, 96.6f);
            kit.Deco(PrimitiveType.Cube, (rail0 + rail1) * 0.5f, new Vector3(50f, 0.4f, 0.3f), IronMat);
            for (float x = -84f; x <= -38f; x += 9.2f) Beam(kit, new Vector3(x, 13.2f, 96.6f), new Vector3(x, KwTop - 0.2f, 96.6f), 0.08f, IronMat);
            Vector3 trolley = new Vector3(-51f, 12.6f, 96.6f);
            kit.Deco(PrimitiveType.Cube, trolley, new Vector3(0.8f, 0.5f, 0.6f), PatchworkAssets.Plain(new Color(0.9f, 0.7f, 0.2f), 0.5f));
            var hook = kit.Group("HoistHook", trolley + Vector3.down * 0.3f);
            var chain = new MeshBatch();
            for (int i = 0; i < 26; i++) chain.Box(new Vector3(0f, -0.15f - i * 0.2f, 0f), new Vector3(0.08f, 0.2f, 0.025f), Quaternion.Euler(0f, i % 2 == 0 ? 0f : 90f, 0f));
            chain.Build(hook, "HoistChain", IronMat);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, -5.45f, 0f), new Vector3(0.1f, 0.3f, 0.1f), IronMat, default(Vector3), hook));
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -6.1f, 0f), new Vector3(1f, 1.1f, 1f), PatchworkAssets.Bubble(new Color(0.55f, 0.65f, 0.6f, 0.5f)), default(Vector3), hook));
            var catch_ = new MeshBatch();
            var crng = new System.Random(77);
            for (int i = 0; i < 9; i++)
                catch_.Ball(new Vector3(((float)crng.NextDouble() - 0.5f) * 0.6f, -6.2f + ((float)crng.NextDouble() - 0.5f) * 0.6f, ((float)crng.NextDouble() - 0.5f) * 0.6f), new Vector3(0.14f, 0.18f, 0.42f), 2, 6);
            catch_.Build(hook, "NetOfFish", PatchworkAssets.Plain(new Color(0.75f, 0.8f, 0.85f), 0.85f));
            var sway = hook.gameObject.AddComponent<SeaBob>();
            sway.Height = 0f; sway.Rock = 3f; sway.Speed = 0.5f;

            // Crates of live fish on the middle walk, and a sack truck.
            Crate(kit, new Vector3(-85f, 0f, 128.2f), 1.4f, 8f);
            Crate(kit, new Vector3(-85f, 0f, 129.9f), 1.3f, -6f);
            Crate(kit, new Vector3(-85f, 1.4f, 128.3f), 1.1f, 20f);
            var stencil = new MeshBatch();
            Letters(stencil, "LIVE FISH", new Vector3(-84.27f, 0.8f, 128.2f), Rot(8f) * Vector3.right, 0.028f, 0.005f);
            Letters(stencil, "THIS WAY UP", new Vector3(-84.33f, 0.6f, 128.2f), Rot(8f) * Vector3.right, 0.022f, 0.005f);
            stencil.Build(kit.Root, "CrateStencil", PatchworkAssets.Felt(new Color(0.2f, 0.15f, 0.12f)));
            Vector3 st = new Vector3(-82.4f, 0f, 129.4f);
            Beam(kit, st + new Vector3(-0.3f, 0.1f, 0f), st + new Vector3(-0.3f, 1.5f, -0.3f), 0.06f, IronMat);
            Beam(kit, st + new Vector3(0.3f, 0.1f, 0f), st + new Vector3(0.3f, 1.5f, -0.3f), 0.06f, IronMat);
            kit.Deco(PrimitiveType.Cube, st + new Vector3(0f, 0.06f, 0.15f), new Vector3(0.7f, 0.04f, 0.4f), IronMat);
            foreach (int s in new[] { -1, 1 })
                kit.Deco(PrimitiveType.Cylinder, st + new Vector3(s * 0.38f, 0.18f, -0.1f), new Vector3(0.36f, 0.04f, 0.36f), PatchworkAssets.Felt(new Color(0.12f, 0.12f, 0.12f)), new Vector3(0f, 0f, 90f));
            kit.Deco(PrimitiveType.Sphere, st + new Vector3(0f, 0.5f, -0.05f), new Vector3(0.6f, 0.7f, 0.4f), PatchworkAssets.Felt(new Color(0.78f, 0.68f, 0.5f)));

            // The big pump on the north walk, thumping away, with its flywheel turning.
            Vector3 pump = new Vector3(-65f, 0f, 176.6f);
            var green = PatchworkAssets.Plain(new Color(0.22f, 0.4f, 0.32f), 0.55f);
            kit.Deco(PrimitiveType.Cube, pump + Vector3.up * 0.2f, new Vector3(3.2f, 0.4f, 1.6f), DarkStone);
            kit.Deco(PrimitiveType.Cylinder, pump + Vector3.up * 1.4f, new Vector3(1.6f, 1f, 1.6f), green);
            kit.Deco(PrimitiveType.Sphere, pump + Vector3.up * 2.4f, new Vector3(1.6f, 0.8f, 1.6f), green);
            for (int b = 0; b < 10; b++)
            {
                float a = b / 10f * Mathf.PI * 2f;
                NoShadow(kit.Deco(PrimitiveType.Sphere, pump + new Vector3(Mathf.Cos(a) * 0.82f, 1.05f, Mathf.Sin(a) * 0.82f), Vector3.one * 0.12f, Brass));
            }
            NoShadow(kit.Deco(PrimitiveType.Cylinder, pump + new Vector3(0f, 2.9f, 0f), new Vector3(0.4f, 0.3f, 0.4f), Brass));
            var piston = kit.Group("PumpPiston", pump + new Vector3(0f, 3.2f, 0f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.4f, 0f), new Vector3(0.18f, 0.4f, 0.18f), PatchworkAssets.Plain(new Color(0.8f, 0.82f, 0.85f), 0.9f), default(Vector3), piston);
            var thump = piston.gameObject.AddComponent<SeaBob>();
            thump.Height = 0.25f; thump.Rock = 0f; thump.Speed = 3.2f;
            var wheel = kit.Group("Flywheel", pump + new Vector3(1.9f, 1.4f, 0f));
            wheel.rotation = Quaternion.Euler(0f, 0f, 90f);
            Hoop(kit, wheel, 0.9f, 0.12f, PatchworkAssets.Plain(new Color(0.75f, 0.15f, 0.15f), 0.4f), 24);
            for (int k = 0; k < 4; k++)
                NoShadow(kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(1.8f, 0.08f, 0.1f), IronMat, new Vector3(0f, k * 45f, 0f), wheel));
            var spin = wheel.gameObject.AddComponent<Spinner>();
            spin.Axis = Vector3.up;
            spin.Speed = 90f;
            Beam(kit, pump + new Vector3(0.8f, 1.4f, 0f), pump + new Vector3(1.9f, 1.4f, 0f), 0.14f, IronMat);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, pump + new Vector3(0f, 1.2f, 1.1f), new Vector3(0.5f, 0.6f, 0.5f), PatchworkAssets.Plain(new Color(0.55f, 0.42f, 0.3f), 0.6f), new Vector3(90f, 0f, 0f)));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, pump + new Vector3(-1.2f, 0.6f, 0f), new Vector3(0.45f, 0.6f, 0.45f), PatchworkAssets.Plain(new Color(0.55f, 0.42f, 0.3f), 0.6f)));
            Motes(kit, pump + new Vector3(0f, 4.6f, 0f), new Vector3(0.8f, 2.4f, 0.8f), 12, new Color(0.95f, 0.95f, 0.95f), 0.25f, 0.22f, new Vector3(0.02f, 0.9f, 0.01f));
            TownKit.Solid(kit, pump + Vector3.up * 1.6f, new Vector3(3.6f, 3.2f, 1.6f), 0f);
            // And its gauge.
            var gauge = kit.Group("PumpGauge", pump + new Vector3(-0.9f, 2.2f, -0.62f));
            gauge.rotation = Rot(180f);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.6f, 0.05f, 0.6f), Brass, new Vector3(90f, 0f, 0f), gauge));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.04f), new Vector3(0.5f, 0.02f, 0.5f), PatchworkAssets.Felt(CreamCol), new Vector3(90f, 0f, 0f), gauge));
            var needle = kit.Group("GaugeNeedle", gauge.position + gauge.rotation * new Vector3(0f, 0f, 0.07f));
            needle.SetParent(gauge, true);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.1f, 0f), new Vector3(0.03f, 0.2f, 0.01f), PatchworkAssets.Felt(new Color(0.75f, 0.1f, 0.1f)), default(Vector3), needle));
            needle.gameObject.AddComponent<GaugeWaver>();

            // The filters pouring clean water back into three of the tanks.
            foreach (var f in new[] { (new Vector3(KwX0 + 0.2f, 2.4f, 116f), 1f), (new Vector3(KwX1 - 0.2f, 2.4f, 104f), -1f), (new Vector3(KwX1 - 0.2f, 2.4f, 148f), -1f) })
            {
                Vector3 wallP = f.Item1;
                float dir = f.Item2;
                var copper = PatchworkAssets.Plain(new Color(0.7f, 0.45f, 0.3f), 0.7f);
                Vector3 spout = wallP + new Vector3(dir * 2.4f, 0.2f, 0f);
                Beam(kit, wallP, wallP + new Vector3(dir * 1.2f, 0.5f, 0f), 0.36f, copper);
                Beam(kit, wallP + new Vector3(dir * 1.2f, 0.5f, 0f), spout, 0.36f, copper);
                NoShadow(kit.Deco(PrimitiveType.Cylinder, wallP, new Vector3(0.6f, 0.08f, 0.6f), Brass, new Vector3(0f, 0f, 90f)));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, spout, new Vector3(0.48f, 0.08f, 0.48f), Brass));
                // The falling water: a glassy column, a bright splash ring, spray and bubbles.
                float fallTop = spout.y - 0.1f, fallBot = WaterTop;
                NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(spout.x, (fallTop + fallBot) * 0.5f, spout.z), new Vector3(0.28f, (fallTop - fallBot) * 0.5f, 0.28f), PatchworkAssets.Bubble(new Color(0.75f, 0.92f, 1f, 0.5f))));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(spout.x, WaterTop + 0.02f, spout.z), new Vector3(1.4f, 0.01f, 1.4f), PatchworkAssets.Glow(new Color(0.85f, 0.95f, 1f), 0.6f)));
                Motes(kit, new Vector3(spout.x, WaterTop + 0.5f, spout.z), new Vector3(1.4f, 1f, 1.4f), 14, new Color(0.9f, 0.97f, 1f), 0.8f, 0.07f, new Vector3(0.05f, 0.6f, 0.04f));
                Bubbles(kit, new Vector3(spout.x, WaterTop - 3f, spout.z), 2.4f, 8);
                AmbientEmitter.Create(kit.Root, spout, Sfx.Ambient(Sfx.AmbientClip.Fountain), 0.35f, 2f, 14f);
            }

            // Banners hung from the roof trusses over each tank.
            Banner(kit, new Vector3(-74.5f, KwTop - 1f, 117f), 180f, 3f, 7f, PatchworkAssets.Print(PatchworkAssets.PrintKind.Stars, Teal, CreamCol), 0);
            Banner(kit, new Vector3(-46.5f, KwTop - 1f, 117f), 180f, 3f, 7f, StarCloth, 1);
            Banner(kit, new Vector3(-74.5f, KwTop - 1f, 162f), 180f, 3f, 7f, PatchworkAssets.Print(PatchworkAssets.PrintKind.Stars, new Color(0.55f, 0.7f, 0.85f), Color.white), 2);
            Banner(kit, new Vector3(-46.5f, KwTop - 1f, 162f), 180f, 3f, 7f, PatchworkAssets.Print(PatchworkAssets.PrintKind.Stars, new Color(0.25f, 0.45f, 0.3f), new Color(1f, 0.9f, 0.5f)), 3);
            KwSign(kit, new Vector3(-82.4f, 0f, 133.2f), Vector3.back, "NO CHIPS FOR PENGUINS", 4.4f);

            // Two more keepers at work: June up on the catwalk, Pippa out on the ice.
            Person(kit, new Vector3(-68.5f, CatY, 112f), 200f, ToyNpc.Kind.RagDoll, "Keeper June", new Color(0.2f, 0.4f, 0.55f), new Color(1f, 0.8f, 0.25f), 1.1f, () => new[]
            {
                "Hello! Careful up here, the rail's only there to make you feel better.",
                "I dropped my torch in the Lantern Deep this morning. Marlow thinks I'm making it up about the anglerfish. I'm not.",
                "If you go down there, look for the light on the ledge halfway down. That's mine. It's waterproof, so it'll still be going.",
                "The diving boards are for feeding. Mostly. Nobody's watching."
            });
            Person(kit, new Vector3(-79.2f, 0.37f, 163.4f), 10f, ToyNpc.Kind.Bunny, "Keeper Pippa", new Color(0.85f, 0.85f, 0.9f), new Color(0.3f, 0.55f, 0.85f), 1.3f, () => new[]
            {
                "Shh, don't startle them. Well, you can startle the big one. He's had three breakfasts.",
                "Emperor penguins can dive deeper than five hundred metres. This tank's not that deep, but don't tell them.",
                "The little grey ones are the chicks. That fluff isn't waterproof yet, so they stay up here with me.",
                "Somebody keeps leaving chips on the ice. Penguins don't eat chips. Except Gerald. Gerald eats everything."
            });
            Vector3 pb = new Vector3(-78.2f, 0.37f, 163.1f);
            kit.Deco(PrimitiveType.Cylinder, pb + Vector3.up * 0.3f, new Vector3(0.5f, 0.3f, 0.5f), PatchworkAssets.Plain(new Color(0.7f, 0.72f, 0.75f), 0.6f));
            var fishes = new MeshBatch();
            for (int i = 0; i < 5; i++) fishes.Ball(pb + new Vector3((i % 3 - 1) * 0.12f, 0.6f + (i / 3) * 0.06f, (i % 2 - 0.5f) * 0.14f), new Vector3(0.1f, 0.12f, 0.36f), 2, 5);
            fishes.Build(kit.Root, "BucketOfFish", PatchworkAssets.Plain(new Color(0.75f, 0.8f, 0.85f), 0.85f));
            Mark("Narration_June", new Vector3(-68.5f, CatY + 1f, 112f));
            Mark("Narration_Pippa", new Vector3(-79.2f, 1.4f, 163.4f));
        }

        // ================================================================ More life in the tanks

        static void KwTankLife(LevelKit kit)
        {
            // ----- Garden eels in the Coral Nursery: they duck into the sand when you get close -----
            float cb = CoralTank[4];
            var erng = new System.Random(4242);
            var eelBody = PatchworkAssets.Felt(new Color(0.85f, 0.82f, 0.7f));
            var eelSpot = PatchworkAssets.Felt(new Color(0.25f, 0.22f, 0.2f));
            for (int i = 0; i < 16; i++)
            {
                Vector3 p = new Vector3(-81f + (float)erng.NextDouble() * 5f, cb, 100.2f + (float)erng.NextDouble() * 3f);
                float h = 0.6f + (float)erng.NextDouble() * 0.5f;
                NoShadow(kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.015f, new Vector3(0.14f, 0.01f, 0.14f), PatchworkAssets.Felt(new Color(0.6f, 0.55f, 0.42f))));
                var eel = kit.Group("GardenEel", p);
                eel.rotation = Rot((float)erng.NextDouble() * 360f);
                var stalk = new MeshBatch();
                for (int k = 0; k < 5; k++)
                {
                    float t = k / 4f;
                    stalk.Ball(new Vector3(0f, t * h, Mathf.Sin(t * 2f) * 0.06f), new Vector3(0.07f, h * 0.3f, 0.07f), 2, 5);
                }
                stalk.Ball(new Vector3(0f, h + 0.02f, 0.1f), new Vector3(0.08f, 0.08f, 0.13f), 2, 6);
                stalk.Build(eel, "EelBody", eelBody);
                var spots = new MeshBatch();
                for (int k = 0; k < 3; k++) spots.Ball(new Vector3(0f, h * (0.3f + k * 0.2f), 0.035f), new Vector3(0.05f, 0.05f, 0.03f), 2, 4);
                foreach (int s in new[] { -1, 1 }) spots.Ball(new Vector3(s * 0.035f, h + 0.05f, 0.15f), Vector3.one * 0.03f, 2, 4);
                spots.Build(eel, "EelSpots", eelSpot);
                var ge = eel.gameObject.AddComponent<GardenEel>();
                ge.Shy = 3.4f;
            }
            Plaque(kit, new Vector3(-75.4f, cb, 100.6f), 270f, "Garden Eels",
                "Each garden eel lives in its own burrow in the sand, with just its front half poking out into the current to catch drifting food. A whole colony sways together like grass.",
                "Swim too close and they all slide down into their holes. Keep still, and they'll come back up.");

            // ----- The octopus, in its pot, with the biscuits -----
            Vector3 pot = new Vector3(-72.6f, cb, 108.6f);
            var potG = kit.Group("OctopusPot", pot);
            potG.rotation = Quaternion.Euler(0f, 60f, 0f);
            var clay = PatchworkAssets.Felt(new Color(0.75f, 0.42f, 0.28f));
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.55f, 0f), new Vector3(1.1f, 1.1f, 1.5f), clay, new Vector3(0f, 0f, 0f), potG);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.55f, 0.85f), new Vector3(0.56f, 0.2f, 0.56f), clay, new Vector3(90f, 0f, 0f), potG);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.55f, 1.06f), new Vector3(0.44f, 0.02f, 0.44f), PatchworkAssets.Felt(new Color(0.06f, 0.04f, 0.04f)), new Vector3(90f, 0f, 0f), potG));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.95f, 0.1f), new Vector3(0.3f, 0.06f, 0.9f), PatchworkAssets.Felt(new Color(0.85f, 0.6f, 0.4f)), default(Vector3), potG));
            TownKit.Solid(kit, pot + Vector3.up * 0.55f, new Vector3(1.2f, 1.1f, 1.6f), 60f);
            var octo = PatchworkAssets.Felt(new Color(0.85f, 0.42f, 0.35f));
            var sucker = PatchworkAssets.Felt(new Color(1f, 0.75f, 0.65f));
            Vector3 mouthLocal = new Vector3(0f, 0.55f, 1.1f);
            NoShadow(kit.Deco(PrimitiveType.Sphere, mouthLocal + new Vector3(0f, 0.15f, 0.02f), new Vector3(0.5f, 0.45f, 0.4f), octo, default(Vector3), potG));
            foreach (int s in new[] { -1, 1 })
            {
                NoShadow(kit.Deco(PrimitiveType.Sphere, mouthLocal + new Vector3(s * 0.15f, 0.28f, 0.16f), Vector3.one * 0.13f, PatchworkAssets.Glow(new Color(1f, 0.92f, 0.6f), 0.4f), default(Vector3), potG));
                NoShadow(kit.Deco(PrimitiveType.Cube, mouthLocal + new Vector3(s * 0.16f, 0.28f, 0.22f), new Vector3(0.08f, 0.03f, 0.02f), PatchworkAssets.Plain(new Color(0.04f, 0.04f, 0.04f), 0.8f), default(Vector3), potG));
            }
            for (int a = 0; a < 5; a++)
            {
                var root = new GameObject("Arm").transform;
                root.SetParent(potG, false);
                root.localPosition = mouthLocal + new Vector3((a - 2f) * 0.1f, -0.05f, 0.05f);
                root.localRotation = Quaternion.Euler(70f + (a % 2) * 15f, (a - 2f) * 28f, 0f);
                var segs = new Transform[6];
                Transform parent = root;
                for (int k = 0; k < segs.Length; k++)
                {
                    var sg = new GameObject("Seg").transform;
                    sg.SetParent(parent, false);
                    sg.localPosition = k == 0 ? Vector3.zero : new Vector3(0f, 0.2f - k * 0.012f, 0f);
                    float r = 0.11f - k * 0.014f;
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.1f, 0f), new Vector3(r * 2f, 0.24f, r * 2f), octo, default(Vector3), sg));
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.1f, -r * 0.8f), Vector3.one * r * 0.8f, sucker, default(Vector3), sg));
                    segs[k] = sg;
                    parent = sg;
                }
                var curl = root.gameObject.AddComponent<KelpSway>();
                curl.Segments = segs;
                curl.Amount = 18f;
                curl.Speed = 0.7f + a * 0.1f;
                curl.Phase = a * 1.3f;
            }
            // The biscuits, stacked neatly, and the lid of the tin from the Keepers' Room.
            var bisc = new MeshBatch();
            Vector3 stash = pot + new Vector3(1.4f, 0f, 0.8f);
            for (int i = 0; i < 7; i++) bisc.Cylinder(stash + new Vector3((i / 4) * 0.36f, 0.02f + (i % 4) * 0.06f, 0f), 0.14f, 0.05f, 10);
            bisc.Build(kit.Root, "Biscuits", PatchworkAssets.Felt(new Color(0.85f, 0.65f, 0.38f)));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, stash + new Vector3(-0.5f, 0.04f, 0.5f), new Vector3(0.7f, 0.03f, 0.7f), PatchworkAssets.Plain(new Color(0.7f, 0.2f, 0.25f), 0.6f), new Vector3(12f, 0f, 6f)));
            Plaque(kit, new Vector3(-70.4f, cb, 107.2f), 300f, "The Octopus",
                "An octopus has three hearts and blue blood, can change its colour in a blink, and can squeeze through any gap bigger than its beak.",
                "They're famous for getting out of their tanks at night. One in New Zealand slipped out, slid across the floor and escaped down a drainpipe to the sea. This one only goes as far as the biscuit tin.");
            Mark("Secret_Biscuits", stash);

            // ----- The Lantern Deep: a giant isopod on its rounds, and glowing sea pens -----
            float lb = LanternTank[4];
            {
                Vector3 c = new Vector3(-45f, lb + 0.25f, 113.5f);
                var iso = kit.Group("GiantIsopod", c);
                var body = new GameObject("Body").transform;
                body.SetParent(iso, false);
                var shell = PatchworkAssets.Felt(new Color(0.72f, 0.66f, 0.78f));
                for (int k = 0; k < 7; k++)
                {
                    float t = (k - 3f) / 3f;
                    kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.12f * (1f - t * t), t * 0.55f), new Vector3(0.9f - Mathf.Abs(t) * 0.3f, 0.36f, 0.26f), shell, default(Vector3), body);
                }
                for (int k = 0; k < 7; k++)
                    foreach (int s in new[] { -1, 1 })
                        NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.42f, -0.1f, -0.5f + k * 0.17f), new Vector3(0.2f, 0.03f, 0.03f), shell, new Vector3(0f, 0f, s * -30f), body));
                foreach (int s in new[] { -1, 1 })
                {
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.22f, 0.12f, 0.78f), new Vector3(0.12f, 0.08f, 0.1f), PatchworkAssets.Glow(new Color(0.9f, 0.7f, 0.3f), 0.6f), default(Vector3), body));
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.1f, 0.05f, 0.95f), new Vector3(0.02f, 0.02f, 0.4f), shell, new Vector3(0f, s * 25f, 0f), body));
                }
                var f = iso.gameObject.AddComponent<FishSwim>();
                f.Center = c; f.RadiusX = 3.2f; f.RadiusZ = 2.4f; f.Speed = 7f; f.Bob = 0f; f.Wiggle = 3f; f.WiggleSpeed = 2f; f.Bank = 0f;
            }
            var pens = new MeshBatch();
            var prng = new System.Random(5353);
            for (int i = 0; i < 9; i++)
            {
                Vector3 p = new Vector3(-43f + (float)prng.NextDouble() * 5f, lb, 99.5f + (float)prng.NextDouble() * 4f);
                float h = 0.8f + (float)prng.NextDouble() * 0.8f;
                pens.Box(p + Vector3.up * h * 0.5f, new Vector3(0.05f, h, 0.05f), Quaternion.identity);
                for (int k = 0; k < 7; k++)
                {
                    float y = h * (0.4f + k * 0.09f);
                    foreach (int s in new[] { -1, 1 })
                        pens.Box(p + new Vector3(s * 0.12f, y, 0f), new Vector3(0.22f, 0.03f, 0.05f), Quaternion.Euler(0f, 0f, s * 25f));
                }
            }
            Twinkling(pens, kit.Root, "SeaPens", new Color(0.9f, 0.6f, 1f), 0.4f, 2.4f, 0.8f, 0.3f);

            // ----- The Frozen Sea: a beluga, and a shoal of cod under the ice -----
            float fb = FrozenTank[4];
            var beluga = Swim(kit, Swimmer.Whale, new Vector3(-74.5f, fb + 2.6f, 154.5f), 6f, 13f, -6f, 0.34f, new Color(0.95f, 0.96f, 0.98f), new Color(0.88f, 0.9f, 0.95f), 200f);
            beluga.Bob = 0.8f;
            Shoal(kit, new Vector3(-75f, -3.8f, 150f), 6f, 10f, 15f, 26, 0.3f, new Color(0.78f, 0.82f, 0.86f), 6161);
            Plaque(kit, new Vector3(-84.6f, fb, 156f), 90f, "The Beluga",
                "Belugas are the white whales of the Arctic. They can turn their heads and pull faces, and they chatter, whistle and click so much that sailors called them sea canaries.",
                "They're born grey, and take years to turn white.");

            // ----- The Mangrove: a manatee, grazing slowly along the bottom -----
            float mb = MangroveTank[4];
            var manatee = Swim(kit, Swimmer.Whale, new Vector3(-40f, mb + 1.3f, 156f), 2.5f, 6f, 4f, 0.36f, new Color(0.5f, 0.5f, 0.46f), new Color(0.62f, 0.6f, 0.55f), 90f);
            manatee.Bob = 0.3f;
            manatee.Wiggle = 2f;
            Plaque(kit, new Vector3(-38.2f, mb, 145.6f), 270f, "The Manatee",
                "Manatees are big, slow, gentle plant-eaters that graze on sea grass like underwater cows. Their nearest relatives on land are elephants.",
                "Sailors long ago who glimpsed them in the water are said to have told stories about mermaids.");
        }

        // ================================================================ Sounds

        static void KwSounds(LevelKit kit)
        {
            AmbientEmitter.Create(kit.Root, new Vector3(-60f, 4f, 86f), Sfx.Ambient(Sfx.AmbientClip.Clock), 0.25f, 2f, 12f);
            AmbientEmitter.Create(kit.Root, new Vector3(-65f, 2f, 176.6f), Sfx.Ambient(Sfx.AmbientClip.Vent), 0.4f, 3f, 24f);
            AmbientEmitter.Create(kit.Root, new Vector3(-60f, 8f, 136f), Sfx.Ambient(Sfx.AmbientClip.Hall), 0.35f, 10f, 60f);
            AmbientEmitter.Create(kit.Root, new Vector3(-46.5f, -14f, 112f), Sfx.Ambient(Sfx.AmbientClip.Deep), 0.5f, 6f, 26f);
            AmbientEmitter.Create(kit.Root, new Vector3(-60f, -6f, 117f), Sfx.Ambient(Sfx.AmbientClip.Deep), 0.3f, 3f, 14f);
            foreach (var p in new[] { new Vector3(-75f, -6f, 130f), new Vector3(-46.5f, -5.5f, 130f), new Vector3(-60f, -5f, 155f) })
                AmbientEmitter.Create(kit.Root, p, Sfx.Ambient(Sfx.AmbientClip.Vent), 0.3f, 2f, 10f);
        }

        // ================================================================ The Grand Atrium

        // A bust of Philip Henry Gosse on a plinth, and the visitors' book and a donation box
        // near the doors.
        static void AtriumFounder(LevelKit kit)
        {
            Vector3 top = Plinth(kit, new Vector3(-14f, 0f, -84f), new Vector3(1.4f, 1.6f, 1.4f), 0f, PaleStone);
            var bust = kit.Group("GosseBust", top);
            bust.rotation = Rot(100f);
            var bronze = PatchworkAssets.Plain(new Color(0.45f, 0.34f, 0.22f), 0.75f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.1f, 0f), new Vector3(0.6f, 0.2f, 0.5f), PaleStone, default(Vector3), bust);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.55f, 0f), new Vector3(1f, 0.7f, 0.55f), bronze, default(Vector3), bust);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.95f, 0f), new Vector3(0.22f, 0.12f, 0.22f), bronze, default(Vector3), bust);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.32f, 0.02f), new Vector3(0.48f, 0.6f, 0.52f), bronze, default(Vector3), bust);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.05f, 0.12f), new Vector3(0.42f, 0.42f, 0.36f), bronze, default(Vector3), bust);   // a grand Victorian beard
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.36f, 0.26f), new Vector3(0.08f, 0.14f, 0.08f), bronze, new Vector3(-10f, 0f, 0f), bust);
            foreach (int s in new[] { -1, 1 })
            {
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.1f, 1.42f, 0.24f), new Vector3(0.08f, 0.05f, 0.04f), PatchworkAssets.Plain(new Color(0.3f, 0.22f, 0.14f), 0.7f), default(Vector3), bust));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.1f, 1.48f, 0.24f), new Vector3(0.12f, 0.03f, 0.04f), bronze, new Vector3(0f, 0f, s * -8f), bust));
            }
            // A little bronze rock pool at his shoulder: a starfish and a sea anemone.
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0.42f, 0.95f, 0.05f), new Vector3(0.22f, 0.1f, 0.22f), bronze, default(Vector3), bust));
            for (int arm = 0; arm < 5; arm++)
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0.42f, 1.02f, 0.05f) + Rot(arm * 72f) * new Vector3(0f, 0f, 0.07f), new Vector3(0.04f, 0.02f, 0.12f), bronze, new Vector3(0f, arm * 72f, 0f), bust));
            SpotLight(kit, top + new Vector3(2.5f, 5f, 0f), top + Vector3.up * 1.2f, new Color(1f, 0.88f, 0.7f), 9f, 30f, 40f, false, false);
            Plaque(kit, new Vector3(-11.8f, 0f, -84f), 100f, "Philip Henry Gosse",
                "A naturalist who loved rock pools. In 1853 he helped stock the Fish House at London Zoo: the first public aquarium in the world.",
                "The next year he wrote a book about keeping sea creatures in glass tanks at home, and gave them the name we still use: the aquarium.");

            // The visitors' book on its lectern, and a glass donation box with a few coins in it.
            Vector3 lec = new Vector3(14f, 0f, -103.4f);
            var lg = kit.Group("VisitorsBook", lec);
            lg.rotation = Rot(200f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.06f, 0f), new Vector3(0.7f, 0.06f, 0.7f), DarkWood, default(Vector3), lg);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.6f, 0f), new Vector3(0.12f, 0.55f, 0.12f), Brass, default(Vector3), lg);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.2f, 0f), new Vector3(0.9f, 0.06f, 0.6f), DarkWood, new Vector3(-20f, 0f, 0f), lg);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(-0.19f, 1.25f, 0.02f), new Vector3(0.36f, 0.03f, 0.46f), PatchworkAssets.Felt(CreamCol), new Vector3(-20f, 0f, 4f), lg));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0.19f, 1.25f, 0.02f), new Vector3(0.36f, 0.03f, 0.46f), PatchworkAssets.Felt(CreamCol), new Vector3(-20f, 0f, -4f), lg));
            var ink = new MeshBatch();
            for (int i = 0; i < 5; i++)
                foreach (int s in new[] { -1, 1 })
                    ink.Box(new Vector3(s * 0.19f, 1.27f + 0.06f - i * 0.04f, 0.18f - i * 0.08f), new Vector3(0.26f - (i % 2) * 0.06f, 0.006f, 0.012f), Quaternion.Euler(-20f, 0f, 0f));
            ink.Build(lg, "Signatures", PatchworkAssets.Felt(new Color(0.15f, 0.2f, 0.45f)));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0.36f, 1.3f, -0.1f), new Vector3(0.02f, 0.02f, 0.3f), PatchworkAssets.Felt(new Color(0.9f, 0.9f, 0.95f)), new Vector3(-20f, 30f, 0f), lg));
            TownKit.Solid(kit, lec + Vector3.up * 0.65f, new Vector3(0.8f, 1.3f, 0.8f), 0f);
            Vector3 box = new Vector3(16.6f, 0f, -103.6f);
            kit.Deco(PrimitiveType.Cylinder, box + Vector3.up * 0.45f, new Vector3(0.3f, 0.45f, 0.3f), Brass);
            NoShadow(kit.Deco(PrimitiveType.Cube, box + Vector3.up * 1.25f, new Vector3(0.6f, 0.7f, 0.6f), CaseGlass));
            kit.Deco(PrimitiveType.Cube, box + Vector3.up * 1.62f, new Vector3(0.66f, 0.06f, 0.66f), Brass);
            NoShadow(kit.Deco(PrimitiveType.Cube, box + Vector3.up * 1.66f, new Vector3(0.24f, 0.02f, 0.04f), PatchworkAssets.Plain(new Color(0.05f, 0.05f, 0.05f), 0.5f)));
            var coins = new MeshBatch();
            var rng = new System.Random(17);
            for (int i = 0; i < 18; i++) coins.Cylinder(box + new Vector3(((float)rng.NextDouble() - 0.5f) * 0.44f, 0.92f + (float)rng.NextDouble() * 0.08f, ((float)rng.NextDouble() - 0.5f) * 0.44f), 0.06f, 0.015f, 8);
            Twinkling(coins, kit.Root, "DonationCoins", new Color(1f, 0.82f, 0.4f), 0.4f, 1f, 1.2f, 0f);
            TownKit.Solid(kit, box + Vector3.up * 0.8f, new Vector3(0.66f, 1.6f, 0.66f), 0f);
        }
    }

    // A drop of water that swells at the tap, falls, and swells again.
    public class DripLoop : MonoBehaviour
    {
        public float Fall = 0.5f, Period = 2.2f;
        Vector3 home;
        float t;

        void Start()
        {
            home = transform.localPosition;
            t = Mathf.Repeat(transform.position.x * 0.37f, Period);
        }

        void Update()
        {
            t += Time.deltaTime;
            float p = Mathf.Repeat(t, Period);
            float fallTime = Mathf.Sqrt(2f * Fall / 9.8f);
            if (p < fallTime)
            {
                transform.localPosition = home + Vector3.down * (4.9f * p * p);
                transform.localScale = Vector3.one * 0.06f;
            }
            else
            {
                transform.localPosition = home;
                transform.localScale = Vector3.one * 0.06f * Mathf.Clamp01((p - fallTime) / (Period - fallTime));
            }
        }
    }

    // A garden eel: up out of its burrow swaying in the current, until Patch comes too close,
    // then it slides back down into the sand and waits.
    public class GardenEel : MonoBehaviour
    {
        public float Shy = 3f;
        float k = 1f, phase, yaw;

        void Start()
        {
            phase = transform.position.x * 1.7f + transform.position.z * 0.9f;
            yaw = transform.localEulerAngles.y;
        }

        void Update()
        {
            float target = 1f;
            var g = PatchworkGame.Instance;
            if (g != null && g.Player != null && (g.Player.transform.position - transform.position).sqrMagnitude < Shy * Shy) target = 0.04f;
            k = Mathf.MoveTowards(k, target, Time.deltaTime * (target < k ? 5f : 0.5f));
            transform.localScale = new Vector3(1f, Mathf.Max(0.04f, k), 1f);
            float t = Time.time;
            transform.localRotation = Quaternion.Euler(0f, yaw, 0f) * Quaternion.Euler(Mathf.Sin(t * 1.2f + phase) * 9f * k, 0f, Mathf.Cos(t * 0.9f + phase) * 7f * k);
        }
    }

    // A pressure gauge's needle, never quite settling.
    public class GaugeWaver : MonoBehaviour
    {
        float phase;
        void Start() { phase = transform.position.x + transform.position.z; }
        void Update()
        {
            float t = Time.time + phase;
            transform.localRotation = Quaternion.Euler(0f, 0f, -30f + Mathf.Sin(t * 0.7f) * 20f + Mathf.Sin(t * 5.3f) * 3f);
        }
    }
}

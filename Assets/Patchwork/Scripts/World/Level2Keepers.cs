using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Behind the Glass: the Keepers' Wing, the back of the aquarium, through a door at the far
    // end of the Kelp Galleries marked KEEPERS ONLY.
    //
    //   The Keepers' Room   - lockers, wetsuits on pegs, a brass diving helmet, the kettle on,
    //                         the feeding times chalked up, and old Keeper Marlow
    //   The Tank Hall       - a great dim hall with four tanks sunk into its floor, open at the
    //                         top so Water Form can dive straight in, catwalks and feeding
    //                         platforms (with diving boards) slung over them
    //   The Undercroft      - a dry corridor down a stair between the first two tanks, with
    //                         windows into the Coral Nursery on one side and the Lantern Deep
    //                         on the other
    //   The four tanks, each its own little sea, joined underwater by the old keepers' pipes:
    //     The Coral Nursery - a sunlit reef, the keepers' coral "trees", seagrass and seahorses,
    //                         clownfish, a moray in its crevice, a cleaning station, a rock arch
    //                         and, behind a curtain of sea fans, a little cave
    //     The Lantern Deep  - a deep dark shaft where everything makes its own light: comb
    //                         jellies, a long siphonophore, anglerfish, a vampire squid, black
    //                         smokers at the bottom and the old Bathysphere
    //     The Frozen Sea    - ice floes with a penguin colony and a seal, brinicles and a
    //                         narwhal under the ice, and an ice cave at the far end
    //     The Mangrove      - trees standing on their roots in the water, mudbanks with
    //                         mudskippers and fiddler crabs, baby sharks and upside-down jellyfish
    //                         among the roots, and a hollow under the bank
    //
    // There are empty markers ("KeepersWing_Markers") at the places the Storyteller will speak
    // and where the wing's trouble will happen, for the parts of the level still to come.
    public static partial class Level2Builder
    {
        const float KwX0 = -88f, KwX1 = -34f, KwZ0 = 78f, KwZ1 = 178f, KwTop = 24f, KwHall = 95f;
        const float KwFill = -26f;      // the bottom of the rock the tanks are cut into
        const float WaterTop = -0.6f;
        const float CatY = 8f;          // the catwalks
        // The four tanks: x0, x1, z0, z1, bed.
        static readonly float[] CoralTank = { -86f, -63f, 98f, 126f, -9f };
        static readonly float[] LanternTank = { -57f, -36f, 98f, 126f, -24f };
        static readonly float[] FrozenTank = { -86f, -63f, 134f, 175f, -12f };
        static readonly float[] MangroveTank = { -57f, -36f, 134f, 175f, -7f };
        public const string CoralMarble = "L2_marble_coral", AbyssMarble = "L2_marble_abyss", IceMarble = "L2_marble_ice", MangroveMarble = "L2_marble_mangrove";

        static Transform kwMarkers;
        static Material KwRock { get { return PatchworkAssets.Plaster(new Color(0.2f, 0.3f, 0.33f)); } }
        static Material Grating { get { return PatchworkAssets.Plain(new Color(0.2f, 0.22f, 0.25f), 0.45f); } }

        static void KeepersWing(LevelKit kit)
        {
            kwMarkers = kit.Group("KeepersWing_Markers", new Vector3(-60f, 0f, 120f));
            KwShell(kit);
            KwWorkroom(kit);
            KwTankHall(kit);
            KwUndercroft(kit);
            KwCatwalks(kit);
            CoralNursery(kit);
            LanternDeep(kit);
            FrozenSea(kit);
            Mangrove(kit);
            KwPipes(kit);
            KwKelpSide(kit);
        }

        // On the Kelp Galleries' side of the door: a doormat, a mop in its bucket, and wet
        // footprints (flipper-prints, really) coming out under the door.
        static void KwKelpSide(LevelKit kit)
        {
            Rug(kit, new Vector3(-60f, 0f, 74.6f), new Vector2(4.2f, 1.6f), PatchworkAssets.Felt(new Color(0.45f, 0.32f, 0.22f)), 0f, PatchworkAssets.Felt(new Color(0.3f, 0.22f, 0.16f)));
            Vector3 b = new Vector3(-56.4f, 0f, 75f);
            kit.Deco(PrimitiveType.Cylinder, b + Vector3.up * 0.35f, new Vector3(0.7f, 0.35f, 0.7f), PatchworkAssets.Plain(new Color(0.85f, 0.75f, 0.2f), 0.5f));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, b + Vector3.up * 0.66f, new Vector3(0.6f, 0.02f, 0.6f), PatchworkAssets.Bubble(new Color(0.5f, 0.6f, 0.55f, 0.6f))));
            Beam(kit, b + new Vector3(0.05f, 0.4f, 0f), b + new Vector3(0.5f, 2.3f, 0.6f), 0.07f, DarkWood);
            NoShadow(kit.Deco(PrimitiveType.Sphere, b + new Vector3(0f, 0.55f, 0f), new Vector3(0.5f, 0.3f, 0.5f), PatchworkAssets.Felt(new Color(0.9f, 0.88f, 0.8f))));
            TownKit.Solid(kit, b + Vector3.up * 0.4f, new Vector3(0.7f, 0.8f, 0.7f), 0f);
            var prints = new MeshBatch();
            // (Off along the terrace, clear of the big rug.)
            for (int i = 0; i < 7; i++)
            {
                Vector3 p = new Vector3(-57f + i * 1.15f, 0.025f, 73.9f + (i % 2 == 0 ? -0.3f : 0.3f));
                prints.Ball(p, new Vector3(0.8f, 0.01f, 0.45f), 2, 7);
            }
            prints.Ball(new Vector3(-52.5f, 0.024f, 74.3f), new Vector3(1.4f, 0.01f, 0.9f), 2, 8);
            prints.Build(kit.Root, "WetPrints", PatchworkAssets.Glow(new Color(0.45f, 0.75f, 0.8f), 0.25f));
        }

        // An empty marker for something still to come (a line for the Storyteller, a patrol).
        static void Mark(string name, Vector3 at)
        {
            var g = new GameObject(name).transform;
            g.SetParent(kwMarkers, false);
            g.position = at;
        }

        // A solid block from (x0,y0,z0) to (x1,y1,z1).
        static Transform KwBox(LevelKit kit, float x0, float x1, float y0, float y1, float z0, float z1, Material mat, string name = "KeepersRock")
        {
            if (x1 - x0 < 0.01f || y1 - y0 < 0.01f || z1 - z0 < 0.01f) return null;
            return kit.Block(new Vector3((x0 + x1) * 0.5f, (y0 + y1) * 0.5f, (z0 + z1) * 0.5f), new Vector3(x1 - x0, y1 - y0, z1 - z0), StoneCol, name, mat);
        }

        // A solid block with square tunnels through it (for the pipes between the tanks). The
        // block is cut into slices along x (alongX) or z; each hole is {from, to, bottom, top}.
        static void KwBoxWithHoles(LevelKit kit, float x0, float x1, float y0, float y1, float z0, float z1, bool alongX, float[][] holes, Material mat)
        {
            var edges = new List<float> { alongX ? x0 : z0, alongX ? x1 : z1 };
            foreach (var h in holes) { edges.Add(h[0]); edges.Add(h[1]); }
            edges.Sort();
            for (int i = 0; i + 1 < edges.Count; i++)
            {
                float a = edges[i], b = edges[i + 1];
                if (b - a < 0.01f) continue;
                float[] hole = null;
                foreach (var h in holes) if (a >= h[0] - 0.001f && b <= h[1] + 0.001f) hole = h;
                if (hole == null)
                {
                    if (alongX) KwBox(kit, a, b, y0, y1, z0, z1, mat); else KwBox(kit, x0, x1, y0, y1, a, b, mat);
                }
                else
                {
                    if (alongX) { KwBox(kit, a, b, y0, hole[2], z0, z1, mat); KwBox(kit, a, b, hole[3], y1, z0, z1, mat); }
                    else { KwBox(kit, x0, x1, y0, hole[2], a, b, mat); KwBox(kit, x0, x1, hole[3], y1, a, b, mat); }
                }
            }
        }

        // A pile of rocks against a wall or on a tank floor.
        static void KwRocks(MeshBatch mb, Vector3 p, float s, System.Random rng)
        {
            int n = 3 + rng.Next(3);
            for (int i = 0; i < n; i++)
            {
                float a = (float)rng.NextDouble() * Mathf.PI * 2f, d = (float)rng.NextDouble() * s * 0.6f;
                float k = s * (0.5f + (float)rng.NextDouble() * 0.6f);
                mb.Ball(p + new Vector3(Mathf.Cos(a) * d, k * 0.3f, Mathf.Sin(a) * d), new Vector3(k * 1.3f, k * 0.85f, k * 1.1f), 4, 7);
            }
        }

        // A low name board at the edge of a tank, gilt letters on navy, on two short brass legs
        // (low enough to see over). It faces "facing".
        static void KwSign(LevelKit kit, Vector3 foot, Vector3 facing, string text, float width)
        {
            var q = Quaternion.LookRotation(-facing, Vector3.up);
            Vector3 c = foot + Vector3.up * 0.7f;
            kit.Deco(PrimitiveType.Cube, c, new Vector3(width, 0.62f, 0.1f), PatchworkAssets.Felt(Navy), q.eulerAngles);
            NoShadow(kit.Deco(PrimitiveType.Cube, c + Vector3.up * 0.33f, new Vector3(width + 0.12f, 0.06f, 0.14f), Brass, q.eulerAngles));
            NoShadow(kit.Deco(PrimitiveType.Cube, c + Vector3.down * 0.33f, new Vector3(width + 0.12f, 0.06f, 0.14f), Brass, q.eulerAngles));
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, foot + q * new Vector3(s * (width * 0.5f - 0.3f), 0.2f, 0f), new Vector3(0.1f, 0.2f, 0.1f), Brass);
            var mb = new MeshBatch();
            Letters(mb, text, c + facing * 0.055f, facing, Mathf.Min(0.065f, (width - 0.4f) / (text.Length * 6f)), 0.02f);
            mb.Build(kit.Root, "SignLetters", PatchworkAssets.Glow(new Color(1f, 0.82f, 0.45f), 0.9f));
        }

        // ================================================================ The shell

        static void KwShell(LevelKit kit)
        {
            var stone = Sandstone;
            Wall(kit, new Vector3(-89f, 0f, 78f), new Vector3(-89f, 0f, 180f), KwFill - 1f, KwTop + 1f, 2f, stone);
            Wall(kit, new Vector3(-33f, 0f, 78f), new Vector3(-33f, 0f, 180f), KwFill - 1f, KwTop + 1f, 2f, stone);
            Wall(kit, new Vector3(-88f, 0f, 179f), new Vector3(-34f, 0f, 179f), KwFill - 1f, KwTop + 1f, 2f, stone);
            // Between the Keepers' Room and the Tank Hall.
            WallWithDoor(kit, new Vector3(-88f, 0f, 94.5f), new Vector3(-34f, 0f, 94.5f), 0f, KwTop, 1f, stone, 28f, 6f, 5.5f);
            // The roof, with long moonlit roof-lights over the tanks and iron trusses.
            kit.Block(new Vector3(-61f, KwTop + 0.5f, 129f), new Vector3(58f, 1f, 102f), Navy, "KeepersRoof", PatchworkAssets.Felt(new Color(0.05f, 0.08f, 0.12f)));
            var moon = PatchworkAssets.Glow(new Color(0.55f, 0.68f, 0.95f), 0.9f);
            foreach (var t in new[] { CoralTank, LanternTank, FrozenTank, MangroveTank })
            {
                float cx = (t[0] + t[1]) * 0.5f, cz = (t[2] + t[3]) * 0.5f;
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(cx, KwTop - 0.05f, cz), new Vector3(8f, 0.08f, (t[3] - t[2]) * 0.7f), moon));
                // The glazing bars across it.
                for (float z = t[2] + 4f; z < t[3] - 3f; z += 3.2f)
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(cx, KwTop - 0.12f, z), new Vector3(8.2f, 0.1f, 0.16f), IronMat));
            }
            for (float z = KwHall + 4f; z < KwZ1; z += 9f)
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(-61f, KwTop - 0.4f, z), new Vector3(54f, 0.8f, 0.6f), IronMat);
                for (int s = -1; s <= 1; s += 2)
                    Beam(kit, new Vector3(-61f + s * 26.5f, KwTop - 3.5f, z), new Vector3(-61f + s * 12f, KwTop - 0.8f, z), 0.3f, IronMat);
            }
            // Big lamps on chains down the middle of the hall.
            foreach (float z in new[] { 104f, 118f, 130f, 145f, 162f })
                HangingLantern(kit, new Vector3(-60f, 12f, z), KwTop, 26f, 1.2f);
            // Pipes along both long walls, with valve wheels and gauges.
            var pipe = PatchworkAssets.Plain(new Color(0.55f, 0.42f, 0.3f), 0.6f);
            foreach (float x in new[] { KwX0 + 0.45f, KwX1 - 0.45f })
                foreach (float y in new[] { 3.2f, 4.1f })
                {
                    NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(x, y, (KwHall + KwZ1) * 0.5f), new Vector3(0.34f, (KwZ1 - KwHall) * 0.5f, 0.34f), pipe, new Vector3(90f, 0f, 0f)));
                    for (float z = KwHall + 3f; z < KwZ1; z += 6f)
                        NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(x, y, z), new Vector3(0.46f, 0.08f, 0.46f), Brass, new Vector3(90f, 0f, 0f)));
                }
            foreach (float z in new[] { 100f, 112f, 140f, 160f })
                foreach (float x in new[] { KwX0 + 0.3f, KwX1 - 0.3f })
                {
                    float yaw = x < -60f ? 90f : 270f;
                    var wheel = kit.Group("ValveWheel", new Vector3(x, 5.2f, z));
                    wheel.rotation = Rot(yaw);
                    Hoop(kit, wheel, 0.42f, 0.06f, PatchworkAssets.Plain(new Color(0.75f, 0.15f, 0.15f), 0.4f), 18);
                    wheel.localRotation = Rot(yaw) * Quaternion.Euler(90f, 0f, 0f);
                    for (int k = 0; k < 3; k++)
                        NoShadow(kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(0.84f, 0.05f, 0.05f), IronMat, new Vector3(0f, k * 60f, 0f), wheel));
                    // A pressure gauge beside it.
                    var g = kit.Group("Gauge", new Vector3(x, 6.4f, z + 1.2f));
                    g.rotation = Rot(yaw);
                    NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.06f), new Vector3(0.6f, 0.05f, 0.6f), Brass, new Vector3(90f, 0f, 0f), g));
                    NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.1f), new Vector3(0.5f, 0.02f, 0.5f), PatchworkAssets.Felt(CreamCol), new Vector3(90f, 0f, 0f), g));
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0.06f, 0.06f, 0.13f), new Vector3(0.03f, 0.2f, 0.01f), PatchworkAssets.Felt(new Color(0.7f, 0.1f, 0.1f)), new Vector3(0f, 0f, -40f), g));
                }
            // Wall lamps.
            foreach (float z in new[] { 106f, 120f, 138f, 152f, 168f })
            {
                Sconce(kit, new Vector3(KwX0, 6f, z), 90f, true, 14f);
                Sconce(kit, new Vector3(KwX1, 6f, z), 270f, true, 14f);
            }
            Mark("Narration_TankHall", new Vector3(-60f, 1f, 97f));
            Mark("EnemyArena_CrossAisle", new Vector3(-60f, 0f, 130f));
        }

        // ================================================================ The Keepers' Room

        static void KwWorkroom(LevelKit kit)
        {
            const float z0 = KwZ0, z1 = 94f, ceil = 8f;
            Floor(kit, KwX0, KwX1, z0, z1 + 1f, 0f, WarmQuilt, 3f, "KeepersRoomFloor");
            kit.Block(new Vector3(-61f, ceil + 0.5f, (z0 + z1) * 0.5f), new Vector3(54f, 1f, z1 - z0), WoodCol, "KeepersRoomCeiling", DarkWood);
            for (float x = KwX0 + 3f; x < KwX1; x += 4.5f)
                kit.Deco(PrimitiveType.Cube, new Vector3(x, ceil - 0.25f, (z0 + z1) * 0.5f), new Vector3(0.4f, 0.5f, z1 - z0), DarkWood);
            foreach (var p in new[] { new Vector3(-74f, 6f, 86f), new Vector3(-60f, 6f, 86f), new Vector3(-46f, 6f, 86f) })
                HangingLantern(kit, p, ceil, 14f, 1.3f);
            Rug(kit, new Vector3(-60f, 0f, 86f), new Vector2(4f, 14f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, Teal, CreamCol), 0f, Brass);

            // ----- West: the lockers, wetsuits and the diving helmet -----
            var green = PatchworkAssets.Plain(new Color(0.3f, 0.45f, 0.4f), 0.35f);
            var vent = PatchworkAssets.Plain(new Color(0.15f, 0.2f, 0.2f), 0.3f);
            string[] names = { "MARLOW", "PIPPA", "OSKAR", "JUNE", "BEN", "AMIRA" };
            for (int i = 0; i < 6; i++)
            {
                float z = 80.2f + i * 1.9f;
                kit.Block(new Vector3(-87.45f, 1.2f, z), new Vector3(1.1f, 2.4f, 1.8f), StoneCol, "Locker", green);
                if (i == 3)
                {
                    // June's locker's open, with her wetsuit and torch inside.
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(-86.9f, 1.25f, z), new Vector3(0.04f, 2.2f, 1.6f), PatchworkAssets.Felt(new Color(0.08f, 0.1f, 0.1f))));
                    var door = kit.Group("LockerDoor", new Vector3(-86.85f, 1.2f, z - 0.8f));
                    door.rotation = Rot(-70f);
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, 0.8f), new Vector3(0.05f, 2.3f, 1.6f), green, default(Vector3), door);
                    NoShadow(kit.Deco(PrimitiveType.Capsule, new Vector3(-86.6f, 1.3f, z), new Vector3(0.5f, 0.8f, 0.3f), PatchworkAssets.Felt(new Color(0.15f, 0.2f, 0.45f))));
                    continue;
                }
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(-86.88f, 1.2f, z), new Vector3(0.02f, 2.25f, 0.04f), vent));
                for (int v = 0; v < 4; v++)
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(-86.88f, 2f - v * 0.1f, z), new Vector3(0.02f, 0.03f, 0.9f), vent));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(-86.88f, 1.55f, z + 0.5f), new Vector3(0.03f, 0.2f, 0.06f), Brass));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(-86.88f, 2.25f, z), new Vector3(0.02f, 0.16f, 0.9f), PatchworkAssets.Felt(CreamCol)));
            }
            // The names on the lockers (on a strip above them, so you can read them).
            for (int i = 0; i < 6; i++)
            {
                var nm = new MeshBatch();
                Letters(nm, names[i], new Vector3(-86.86f, 2.6f, 80.2f + i * 1.9f), Vector3.right, 0.035f, 0.01f);
                nm.Build(kit.Root, "LockerName", PatchworkAssets.Felt(new Color(0.15f, 0.12f, 0.1f)));
            }
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(-86.88f, 2.6f, 85f), new Vector3(0.02f, 0.3f, 11.4f), PatchworkAssets.Felt(CreamCol)));
            TownKit.Bench(kit, new Vector3(-84.6f, 0f, 85f), 90f);

            // Wetsuits on pegs along the south wall, flippers on the floor under them.
            Color[] suit = { new Color(0.1f, 0.12f, 0.2f), new Color(0.12f, 0.2f, 0.3f), new Color(0.2f, 0.12f, 0.18f), new Color(0.1f, 0.25f, 0.22f) };
            for (int i = 0; i < 4; i++)
            {
                float x = -83f + i * 3f;
                var g = kit.Group("Wetsuit", new Vector3(x, 0f, 78.3f));
                var m = PatchworkAssets.Felt(suit[i]);
                NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 3.3f, 0.1f), new Vector3(0.08f, 0.15f, 0.08f), Brass, new Vector3(90f, 0f, 0f), g));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 2.5f, 0.2f), new Vector3(0.7f, 1.3f, 0.1f), m, default(Vector3), g));
                for (int s = -1; s <= 1; s += 2)
                {
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.48f, 2.4f, 0.2f), new Vector3(0.2f, 1.1f, 0.08f), m, new Vector3(0f, 0f, s * 12f), g));
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.18f, 1.35f, 0.2f), new Vector3(0.26f, 1.1f, 0.08f), m, default(Vector3), g));
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.3f, 0.04f, 0.5f), new Vector3(0.3f, 0.04f, 0.8f), PatchworkAssets.Felt(new Color(1f, 0.7f, 0.2f)), new Vector3(0f, s * 8f, 0f), g));
                }
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 2.9f, 0.26f), new Vector3(0.05f, 0.5f, 0.02f), PatchworkAssets.Felt(new Color(0.9f, 0.9f, 0.9f)), default(Vector3), g));
            }

            // The brass diving helmet on its stand.
            Vector3 hf = new Vector3(-79f, 0f, 89.5f);
            var helm = kit.Group("DivingHelmet", hf);
            helm.rotation = Rot(120f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.05f, 0f), new Vector3(0.9f, 0.05f, 0.9f), DarkWood, default(Vector3), helm);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.65f, 0f), new Vector3(0.16f, 0.6f, 0.16f), DarkWood, default(Vector3), helm);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.35f, 0f), new Vector3(1.2f, 0.18f, 0.9f), Brass, default(Vector3), helm);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.95f, 0f), new Vector3(1.05f, 1.1f, 1.05f), Brass, default(Vector3), helm);
            var porthole = PatchworkAssets.Glow(new Color(0.3f, 0.45f, 0.5f), 0.25f);
            foreach (var d in new[] { Vector3.forward, Vector3.left, Vector3.right })
            {
                var pr = Quaternion.LookRotation(d) * Quaternion.Euler(90f, 0f, 0f);
                NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.95f, 0f) + d * 0.5f, new Vector3(0.42f, 0.04f, 0.42f), Brass, pr.eulerAngles, helm));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.95f, 0f) + d * 0.53f, new Vector3(0.32f, 0.02f, 0.32f), porthole, pr.eulerAngles, helm));
                for (int b = 0; b < 3; b++)
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.95f, 0f) + d * 0.56f + Vector3.up * (-0.1f + b * 0.1f), new Vector3(Mathf.Abs(d.x) > 0.5f ? 0.02f : 0.3f, 0.02f, Mathf.Abs(d.x) > 0.5f ? 0.3f : 0.02f), Brass, default(Vector3), helm));
            }
            for (int b = 0; b < 8; b++)
            {
                float a = b * 45f * Mathf.Deg2Rad;
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.55f, 1.46f, Mathf.Sin(a) * 0.42f), Vector3.one * 0.09f, Brass, default(Vector3), helm));
            }
            TownKit.Solid(kit, hf + Vector3.up * 1.2f, new Vector3(1.2f, 2.4f, 1.2f), 0f);

            // ----- East: the work table, the brine-shrimp tank, the chalkboard -----
            Vector3 tf = new Vector3(-46f, 0f, 86f);
            kit.Block(tf + Vector3.up * 0.95f, new Vector3(8f, 0.14f, 2.4f), WoodCol, "WorkTable", DarkWood);
            foreach (var l in new[] { new Vector3(-3.7f, 0f, -1f), new Vector3(3.7f, 0f, -1f), new Vector3(-3.7f, 0f, 1f), new Vector3(3.7f, 0f, 1f) })
                kit.Deco(PrimitiveType.Cube, tf + l + Vector3.up * 0.45f, new Vector3(0.16f, 0.9f, 0.16f), DarkWood);
            TownKit.Solid(kit, tf + Vector3.up * 0.5f, new Vector3(8f, 1f, 2.4f), 0f);
            float ty = 1.02f;
            // Buckets of fish food, tins, scales, the kettle and mugs, a newspaper, a clipboard.
            var tin = PatchworkAssets.Plain(new Color(0.7f, 0.72f, 0.75f), 0.7f);
            foreach (float x in new[] { -3f, -2.2f })
            {
                kit.Deco(PrimitiveType.Cylinder, tf + new Vector3(x, ty + 0.3f, -0.4f), new Vector3(0.6f, 0.3f, 0.6f), tin);
                NoShadow(kit.Deco(PrimitiveType.Cylinder, tf + new Vector3(x, ty + 0.58f, -0.4f), new Vector3(0.52f, 0.02f, 0.52f), PatchworkAssets.Felt(new Color(0.7f, 0.5f, 0.3f))));
                NoShadow(kit.Deco(PrimitiveType.Cube, tf + new Vector3(x, ty + 0.75f, -0.4f), new Vector3(0.6f, 0.04f, 0.04f), IronMat, new Vector3(0f, 0f, 0f)));
            }
            Color[] tins = { new Color(0.85f, 0.3f, 0.3f), new Color(0.3f, 0.5f, 0.85f), new Color(0.95f, 0.75f, 0.3f) };
            for (int i = 0; i < 3; i++)
                kit.Deco(PrimitiveType.Cylinder, tf + new Vector3(-1.2f + i * 0.45f, ty + 0.2f, 0.6f), new Vector3(0.32f, 0.2f, 0.32f), PatchworkAssets.Felt(tins[i]));
            kit.Deco(PrimitiveType.Cube, tf + new Vector3(0.6f, ty + 0.08f, -0.3f), new Vector3(0.9f, 0.16f, 0.6f), Brass);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, tf + new Vector3(0.6f, ty + 0.2f, -0.3f), new Vector3(0.7f, 0.03f, 0.7f), Brass));
            // The kettle (still steaming) and three mugs.
            Vector3 kettle = tf + new Vector3(2.2f, ty, 0.3f);
            kit.Deco(PrimitiveType.Sphere, kettle + Vector3.up * 0.3f, new Vector3(0.6f, 0.55f, 0.6f), PatchworkAssets.Plain(new Color(0.8f, 0.3f, 0.25f), 0.6f));
            kit.Deco(PrimitiveType.Cube, kettle + new Vector3(0.38f, 0.38f, 0f), new Vector3(0.3f, 0.08f, 0.08f), PatchworkAssets.Plain(new Color(0.8f, 0.3f, 0.25f), 0.6f), new Vector3(0f, 0f, 30f));
            NoShadow(kit.Deco(PrimitiveType.Cube, kettle + new Vector3(0f, 0.66f, 0f), new Vector3(0.4f, 0.06f, 0.08f), IronMat));
            Motes(kit, kettle + new Vector3(0.6f, 1.1f, 0f), new Vector3(0.4f, 1f, 0.4f), 8, new Color(0.95f, 0.95f, 0.95f), 0.2f, 0.12f, new Vector3(0.02f, 0.5f, 0f));
            Color[] mugs = { new Color(0.9f, 0.85f, 0.7f), new Color(0.4f, 0.6f, 0.8f), new Color(0.8f, 0.5f, 0.6f) };
            for (int i = 0; i < 3; i++)
            {
                Vector3 mp = tf + new Vector3(2.9f + (i % 2) * 0.4f, ty + 0.15f, -0.6f + i * 0.35f);
                kit.Deco(PrimitiveType.Cylinder, mp, new Vector3(0.22f, 0.15f, 0.22f), PatchworkAssets.Felt(mugs[i]));
                NoShadow(kit.Deco(PrimitiveType.Cube, mp + new Vector3(0.14f, 0f, 0f), new Vector3(0.06f, 0.14f, 0.04f), PatchworkAssets.Felt(mugs[i])));
            }
            NoShadow(kit.Deco(PrimitiveType.Cube, tf + new Vector3(-0.4f, ty + 0.03f, 0.55f), new Vector3(0.9f, 0.02f, 0.6f), PatchworkAssets.Felt(new Color(0.9f, 0.88f, 0.82f)), new Vector3(0f, 14f, 0f)));
            NoShadow(kit.Deco(PrimitiveType.Cube, tf + new Vector3(1.4f, ty + 0.03f, 0.6f), new Vector3(0.5f, 0.03f, 0.7f), DarkWood, new Vector3(0f, -8f, 0f)));
            NoShadow(kit.Deco(PrimitiveType.Cube, tf + new Vector3(1.4f, ty + 0.05f, 0.6f), new Vector3(0.42f, 0.01f, 0.55f), PatchworkAssets.Felt(CreamCol), new Vector3(0f, -8f, 0f)));
            for (int i = 0; i < 3; i++)
                TownKit.Bench(kit, tf + new Vector3(-2.6f + i * 2.6f, 0f, 1.9f), 0f);

            // The brine shrimp tank: little pink sparks of baby shrimp, for feeding the seahorses.
            Vector3 bt = new Vector3(-37.2f, 0f, 81.5f);
            kit.Deco(PrimitiveType.Cube, bt + Vector3.up * 0.5f, new Vector3(2.4f, 1f, 1.3f), DarkWood);
            var bglass = kit.Block(bt + Vector3.up * 1.6f, new Vector3(2.2f, 1.2f, 1.1f), Color.white, "BrineTank", GlassMat);
            NoShadow(bglass);
            NoShadow(kit.Deco(PrimitiveType.Cube, bt + Vector3.up * 1.4f, new Vector3(2.1f, 0.8f, 1f), PatchworkAssets.Bubble(new Color(0.4f, 0.7f, 0.75f, 0.3f))));
            Motes(kit, bt + Vector3.up * 1.45f, new Vector3(1.9f, 0.7f, 0.8f), 30, new Color(1f, 0.55f, 0.6f), 1.4f, 0.05f, new Vector3(0.05f, 0.02f, 0.03f));
            Bubbles(kit, bt + new Vector3(0.8f, 1.05f, 0f), 0.75f, 6);
            PointLight(kit, bt + Vector3.up * 2.2f, new Color(1f, 0.7f, 0.75f), 5f, 1f);
            TownKit.Solid(kit, bt + Vector3.up * 1.1f, new Vector3(2.4f, 2.2f, 1.3f), 0f);

            // The chalkboard of feeding times.
            Vector3 cb = new Vector3(-46f, 3.3f, 78.08f);
            kit.Deco(PrimitiveType.Cube, cb, new Vector3(6.4f, 2.6f, 0.1f), DarkWood);
            NoShadow(kit.Deco(PrimitiveType.Cube, cb + new Vector3(0f, 0f, 0.06f), new Vector3(6f, 2.2f, 0.04f), PatchworkAssets.Felt(new Color(0.12f, 0.16f, 0.14f))));
            var chalk = new MeshBatch();
            Letters(chalk, "FEEDING TIMES", cb + new Vector3(0f, 0.8f, 0.085f), Vector3.forward, 0.055f, 0.01f);
            for (int i = 0; i < 4; i++)
                chalk.Box(cb + new Vector3(-1.2f + (i % 2) * 0.3f, 0.25f - i * 0.36f, 0.085f), new Vector3(2.6f - (i % 3) * 0.5f, 0.05f, 0.01f), Quaternion.identity);
            // A little chalk fish in the corner.
            chalk.Ball(cb + new Vector3(2.1f, -0.5f, 0.085f), new Vector3(0.7f, 0.36f, 0.01f), 2, 8);
            chalk.Box(cb + new Vector3(2.6f, -0.5f, 0.085f), new Vector3(0.3f, 0.3f, 0.01f), Quaternion.Euler(0f, 0f, 45f));
            chalk.Build(kit.Root, "Chalk", PatchworkAssets.Felt(new Color(0.92f, 0.92f, 0.88f)));
            Plaque(kit, new Vector3(-50.5f, 0f, 79.4f), 0f, "The Keepers' Notice Board",
                "FEEDING TIMES. Coral Nursery: half past eight, and again at four. Lantern Deep: whenever they can find it. Frozen Sea: fish, lots. Mangrove: the sharks first, then everybody else.",
                "PLEASE do not tap on the glass. PLEASE do not feed the octopus after midnight. PLEASE stop asking the octopus where the biscuits went.");

            // Nets on long poles leaning on the east wall, a hose on its reel, sacks of fish food.
            for (int i = 0; i < 4; i++)
            {
                Vector3 foot = new Vector3(-35f, 0f, 86f + i * 1.1f);
                Vector3 top = new Vector3(-34.2f, 4.6f, 86.3f + i * 1.1f);
                Beam(kit, foot, top, 0.08f, DarkWood);
                var net = kit.Group("Net", top + new Vector3(-0.1f, 0.4f, 0f));
                net.rotation = Quaternion.Euler(0f, 90f, 80f);
                Hoop(kit, net, 0.45f, 0.04f, IronMat, 16);
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.25f, 0f), new Vector3(0.8f, 0.6f, 0.8f), PatchworkAssets.Bubble(new Color(0.6f, 0.7f, 0.65f, 0.35f)), default(Vector3), net));
            }
            var reel = kit.Group("HoseReel", new Vector3(-34.3f, 2.2f, 92f));
            reel.rotation = Rot(270f);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.25f), new Vector3(1.2f, 0.25f, 1.2f), PatchworkAssets.Felt(new Color(0.25f, 0.5f, 0.3f)), new Vector3(90f, 0f, 0f), reel));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.25f), new Vector3(1.4f, 0.04f, 1.4f), PatchworkAssets.Plain(new Color(0.75f, 0.15f, 0.15f), 0.4f), new Vector3(90f, 0f, 0f), reel));
            for (int i = 0; i < 5; i++)
                kit.Deco(PrimitiveType.Sphere, new Vector3(-38.5f + (i % 3) * 1f, 0.35f + (i / 3) * 0.6f, 92.6f - (i % 2) * 0.3f), new Vector3(1f, 0.7f, 0.7f), PatchworkAssets.Felt(new Color(0.78f, 0.68f, 0.5f)), new Vector3(0f, i * 23f, 0f));
            TownKit.Solid(kit, new Vector3(-37.5f, 0.6f, 92.4f), new Vector3(3f, 1.2f, 1.2f), 0f);

            // Signs: KEEPERS ONLY on the Kelp Galleries' side of the door, TANK HALL over the next.
            var gold = PatchworkAssets.Glow(new Color(1f, 0.82f, 0.45f), 0.9f);
            foreach (var s in new[] { (new Vector3(-60f, 7f, 75.94f), Vector3.back, "KEEPERS ONLY"), (new Vector3(-60f, 6.6f, 93.94f), Vector3.back, "TANK HALL") })
            {
                var q = Quaternion.LookRotation(-s.Item2, Vector3.up);
                NoShadow(kit.Deco(PrimitiveType.Cube, s.Item1 - s.Item2 * 0.03f, new Vector3(s.Item3.Length * 0.42f + 0.6f, 0.8f, 0.06f), PatchworkAssets.Felt(Navy), q.eulerAngles));
                var mb = new MeshBatch();
                Letters(mb, s.Item3, s.Item1 + s.Item2 * 0.01f, s.Item2, 0.07f, 0.02f);
                mb.Build(kit.Root, "DoorLetters", gold);
            }

            // Old Keeper Marlow, and a checkpoint by the door.
            Person(kit, new Vector3(-55.2f, 0f, 90.5f), 210f, ToyNpc.Kind.Bird, "Keeper Marlow", new Color(0.3f, 0.36f, 0.5f), new Color(0.95f, 0.75f, 0.35f), 0.85f, () => new[]
            {
                "Mind the floor, it's always wet in here. That's how you know it's working.",
                "Out there's the front of the glass. In here's the back. Everybody wants to see the front. The back's where the fish actually live.",
                "Four tanks through that door, all joined up by the old pipes underneath. A good swimmer could go round the lot without coming up once.",
                "If you find June's torch down the Deep, she'd like it back. She says the anglerfish took it. I have my doubts."
            });
            kit.AddCheckpoint(new Vector3(-60f, 0f, 84f), 0f);
            kit.Hint(new Vector3(-60f, 1.5f, 80.5f), new Vector3(8f, 4f, 4f), "Behind the Glass",
                "This is where the museum's keepers look after the tanks. Past the next door the tanks are open at the top: in Water Form you can dive straight in, and swim from one to the next through the old pipes underneath.", HintTheme.Water);
            Mark("Narration_KeepersDoor", new Vector3(-60f, 1f, 79f));
            Mark("Narration_Marlow", new Vector3(-55f, 1f, 90f));

            // The way in: first sight of the hall.
            var shot = Shot(kit, "keepers", new Vector3(-60f, 1.5f, 96.6f), new Vector3(6f, 4f, 2f), 10f,
                new[] { new Vector3(-60f, 2.6f, 96f), new Vector3(-66f, 10f, 108f), new Vector3(-74f, 14f, 138f), new Vector3(-48f, 11f, 150f), new Vector3(-60f, 3f, 97.5f) },
                new[] { new Vector3(-60f, 1f, 112f), new Vector3(-74f, -4f, 114f), new Vector3(-76f, 0f, 165f), new Vector3(-46f, -2f, 162f), new Vector3(-60f, 1.5f, 108f) },
                "Behind the Glass.", "Every aquarium has a back door...", "...and four little seas behind it.");
            shot.Blend = 1.1f;
        }

        // ================================================================ The Tank Hall

        static void KwTankHall(LevelKit kit)
        {
            var rock = KwRock;
            const float top = -1f;
            // The rock the tanks are sunk into, under the walkways (the spine between the first
            // two tanks is the Undercroft, built separately).
            KwBox(kit, KwX0, KwX1, KwFill, top, KwHall, 98f, rock);
            KwBox(kit, KwX0, -86f, KwFill, top, 98f, 126f, rock);
            KwBox(kit, KwX0, -86f, KwFill, top, 134f, 175f, rock);
            KwBox(kit, -36f, KwX1, KwFill, top, 98f, 126f, rock);
            KwBox(kit, -36f, KwX1, KwFill, top, 134f, 175f, rock);
            KwBoxWithHoles(kit, KwX0, KwX1, KwFill, top, 126f, 134f, true, new[] { new[] { -76.5f, -73.5f, -7.5f, -4.5f }, new[] { -48f, -45f, -7f, -4f } }, rock);
            KwBoxWithHoles(kit, -63f, -57f, KwFill, top, 134f, 175f, false, new[] { new[] { 153.5f, 156.5f, -6.5f, -3.5f } }, rock);
            KwBox(kit, KwX0, KwX1, KwFill, top, 175f, KwZ1, rock);
            // The walkways.
            Color[] deck = { new Color(0.22f, 0.27f, 0.31f), new Color(0.27f, 0.31f, 0.34f), new Color(0.2f, 0.25f, 0.29f), new Color(0.29f, 0.33f, 0.35f) };
            Floor(kit, KwX0, KwX1, KwHall, 98f, 0f, deck, 3f, "KeepersDeck");
            Floor(kit, KwX0, -86f, 98f, 126f, 0f, deck, 3f, "KeepersDeck");
            Floor(kit, KwX0, -86f, 134f, 175f, 0f, deck, 3f, "KeepersDeck");
            Floor(kit, -36f, KwX1, 98f, 126f, 0f, deck, 3f, "KeepersDeck");
            Floor(kit, -36f, KwX1, 134f, 175f, 0f, deck, 3f, "KeepersDeck");
            Floor(kit, KwX0, KwX1, 126f, 134f, 0f, deck, 3f, "KeepersDeck");
            Floor(kit, -63f, -57f, 134f, 175f, 0f, deck, 3f, "KeepersDeck");
            Floor(kit, KwX0, KwX1, 175f, KwZ1, 0f, deck, 3f, "KeepersDeck");

            // Each tank: its floor, its water, a brass-capped kerb round it, and its name.
            var kerb = PaleStone;
            foreach (var t in new[] { CoralTank, LanternTank, FrozenTank, MangroveTank })
            {
                float x0 = t[0], x1 = t[1], z0 = t[2], z1 = t[3];
                float w = x1 - x0, d = z1 - z0;
                foreach (float z in new[] { z0 - 0.25f, z1 + 0.25f })
                {
                    kit.Deco(PrimitiveType.Cube, new Vector3((x0 + x1) * 0.5f, 0.12f, z), new Vector3(w + 1f, 0.24f, 0.5f), kerb);
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3((x0 + x1) * 0.5f, 0.26f, z), new Vector3(w + 1f, 0.05f, 0.2f), Brass));
                }
                foreach (float x in new[] { x0 - 0.25f, x1 + 0.25f })
                {
                    kit.Deco(PrimitiveType.Cube, new Vector3(x, 0.115f, (z0 + z1) * 0.5f), new Vector3(0.5f, 0.23f, d), kerb);
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(x, 0.255f, (z0 + z1) * 0.5f), new Vector3(0.2f, 0.05f, d), Brass));
                }
            }
            KwSign(kit, new Vector3(-74.5f, 0f, 97.2f), Vector3.back, "CORAL NURSERY", 6f);
            KwSign(kit, new Vector3(-46.5f, 0f, 97.2f), Vector3.back, "LANTERN DEEP", 5.6f);
            KwSign(kit, new Vector3(-74.5f, 0f, 133.2f), Vector3.back, "FROZEN SEA", 4.8f);
            KwSign(kit, new Vector3(-46.5f, 0f, 133.2f), Vector3.back, "MANGROVE", 4.2f);

            // Keepers' things about the deck: feeding buckets, a trolley, coiled hose, a life ring,
            // and a depth gauge on a post beside each tank.
            var bucket = PatchworkAssets.Plain(new Color(0.7f, 0.72f, 0.75f), 0.6f);
            foreach (var p in new[] { new Vector3(-62f, 0f, 97f), new Vector3(-35f, 0f, 128f), new Vector3(-87f, 0f, 131f), new Vector3(-58f, 0f, 176.5f) })
            {
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.3f, new Vector3(0.55f, 0.3f, 0.55f), bucket);
                NoShadow(kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.55f, new Vector3(0.46f, 0.02f, 0.46f), PatchworkAssets.Felt(new Color(0.55f, 0.62f, 0.6f))));
            }
            Vector3 trolley = new Vector3(-66f, 0f, 129.5f);
            kit.Deco(PrimitiveType.Cube, trolley + Vector3.up * 0.8f, new Vector3(1.8f, 0.08f, 1f), IronMat);
            kit.Deco(PrimitiveType.Cube, trolley + Vector3.up * 0.3f, new Vector3(1.8f, 0.06f, 1f), IronMat);
            foreach (var o in new[] { new Vector3(-0.8f, 0f, -0.42f), new Vector3(0.8f, 0f, -0.42f), new Vector3(-0.8f, 0f, 0.42f), new Vector3(0.8f, 0f, 0.42f) })
            {
                kit.Deco(PrimitiveType.Cube, trolley + o + Vector3.up * 0.55f, new Vector3(0.05f, 0.9f, 0.05f), IronMat);
                kit.Deco(PrimitiveType.Sphere, trolley + o + Vector3.up * 0.08f, Vector3.one * 0.16f, PatchworkAssets.Felt(new Color(0.15f, 0.15f, 0.15f)));
            }
            for (int i = 0; i < 3; i++)
                kit.Deco(PrimitiveType.Cylinder, trolley + new Vector3(-0.5f + i * 0.5f, 1.05f, 0f), new Vector3(0.38f, 0.22f, 0.38f), PatchworkAssets.Felt(new[] { new Color(0.85f, 0.3f, 0.3f), new Color(0.3f, 0.5f, 0.85f), new Color(0.95f, 0.75f, 0.3f) }[i]));
            TownKit.Solid(kit, trolley + Vector3.up * 0.6f, new Vector3(1.8f, 1.2f, 1f), 0f);
            // A life ring on the cross aisle's north side.
            var ring = kit.Group("LifeRing", new Vector3(-60f, 1.6f, 133.7f));
            ring.rotation = Quaternion.Euler(90f, 0f, 0f);
            for (int i = 0; i < 8; i++)
            {
                var seg = kit.Group("RingSeg", ring.position);
                seg.SetParent(ring, true);
                seg.localRotation = Quaternion.Euler(0f, i * 45f, 0f);
                NoShadow(kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0f, 0.5f), new Vector3(0.22f, 0.24f, 0.22f), PatchworkAssets.Felt(i % 2 == 0 ? new Color(0.9f, 0.2f, 0.2f) : new Color(0.96f, 0.96f, 0.92f)), new Vector3(0f, 0f, 90f), seg));
            }
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(-60f, 1f, 133.9f), new Vector3(0.12f, 2f, 0.12f), DarkWood));

            // Snow, falling softly over the Frozen Sea.
            Motes(kit, new Vector3(-74.5f, 6f, 154.5f), new Vector3(23f, 12f, 41f), 90, new Color(0.95f, 0.97f, 1f), 0.5f, 0.12f, new Vector3(0.05f, -0.55f, 0.03f));
            // Fireflies in the Mangrove.
            Motes(kit, new Vector3(-46.5f, 4f, 157f), new Vector3(20f, 7f, 36f), 40, new Color(1f, 0.9f, 0.4f), 2.4f, 0.09f, new Vector3(0.06f, 0.02f, -0.05f));
            // A second checkpoint in the middle of the hall.
            kit.AddCheckpoint(new Vector3(-58.5f, 0f, 130f), 90f);
        }

        // ================================================================ The Undercroft

        static void KwUndercroft(LevelKit kit)
        {
            var rock = KwRock;
            const float x0 = -63f, x1 = -57f, z0 = 98f, z1 = 126f, floorY = -8f, ceil = -3.5f, top = -1f;
            const float wy0 = -7.2f, wy1 = -4.6f, ww = 3.2f;
            float[] windows = { 111f, 116.5f, 122f };
            KwBox(kit, x0, x1, KwFill, floorY - 1f, z0, z1, rock);
            // Its two walls, a metre thick, with windows into the tanks either side.
            foreach (float wx in new[] { x0 + 0.5f, x1 - 0.5f })
            {
                float a = wx - 0.5f, b = wx + 0.5f;
                float z = z0;
                foreach (float wc in windows)
                {
                    KwBox(kit, a, b, floorY - 1f, top, z, wc - ww * 0.5f, rock);
                    KwBox(kit, a, b, floorY - 1f, wy0, wc - ww * 0.5f, wc + ww * 0.5f, rock);
                    KwBox(kit, a, b, wy1, top, wc - ww * 0.5f, wc + ww * 0.5f, rock);
                    NoShadow(kit.Block(new Vector3(wx, (wy0 + wy1) * 0.5f, wc), new Vector3(0.3f, wy1 - wy0, ww), Color.white, "TankGlass", GlassMat));
                    // A riveted brass frame on the dry side.
                    float face = wx < -60f ? b + 0.03f : a - 0.03f;
                    var brass = Brass;
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(face, wy1 + 0.12f, wc), new Vector3(0.08f, 0.24f, ww + 0.48f), brass));
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(face, wy0 - 0.12f, wc), new Vector3(0.08f, 0.24f, ww + 0.48f), brass));
                    for (int s = -1; s <= 1; s += 2)
                        NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(face, (wy0 + wy1) * 0.5f, wc + s * (ww * 0.5f + 0.12f)), new Vector3(0.08f, wy1 - wy0, 0.24f), brass));
                    for (int r = 0; r < 10; r++)
                    {
                        float u = r / 9f;
                        NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(face + (wx < -60f ? 0.05f : -0.05f), wy1 + 0.12f, wc - ww * 0.5f + ww * u), Vector3.one * 0.07f, brass));
                        NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(face + (wx < -60f ? 0.05f : -0.05f), wy0 - 0.12f, wc - ww * 0.5f + ww * u), Vector3.one * 0.07f, brass));
                    }
                    z = wc + ww * 0.5f;
                }
                KwBox(kit, a, b, floorY - 1f, top, z, z1, rock);
            }
            // The stair down from the south walk, its floor, its ceiling, and the deck over it.
            Steps(kit, new Vector3(-60f, floorY, 108f), 180f, 28, 8f / 28f, 10f / 28f, 4f, DarkStone, floorY - 1f);
            Floor(kit, -62f, -58f, 108f, z1, floorY, SeaQuilt, 3f, "UndercroftFloor");
            KwBox(kit, -62f, -58f, ceil, top, 108f, z1, rock, "UndercroftCeiling");
            Color[] deck = { new Color(0.22f, 0.27f, 0.31f), new Color(0.27f, 0.31f, 0.34f) };
            Floor(kit, x0, x1, 108f, z1, 0f, deck, 3f, "KeepersDeck");
            Floor(kit, x0, -62f, z0, 108f, 0f, deck, 3f, "KeepersDeck");
            Floor(kit, -58f, x1, z0, 108f, 0f, deck, 3f, "KeepersDeck");
            Balustrade(kit, new Vector3(-62.2f, 0f, 98.6f), new Vector3(-62.2f, 0f, 108.4f), 1.1f, PaleStone);
            Balustrade(kit, new Vector3(-57.8f, 0f, 98.6f), new Vector3(-57.8f, 0f, 108.4f), 1.1f, PaleStone);
            Balustrade(kit, new Vector3(-62.2f, 0f, 108.4f), new Vector3(-57.8f, 0f, 108.4f), 1.1f, PaleStone);
            // A sign over the head of the stair, on two tall brass posts.
            foreach (float x in new[] { -62.2f, -57.8f })
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(x, 2f, 98.6f), new Vector3(0.14f, 2f, 0.14f), Brass);
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(x, 4.1f, 98.6f), Vector3.one * 0.3f, Brass));
            }
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(-60f, 3.5f, 98.6f), new Vector3(4.3f, 0.7f, 0.1f), PatchworkAssets.Felt(Navy)));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(-60f, 3.88f, 98.6f), new Vector3(4.4f, 0.06f, 0.14f), Brass));
            var ul = new MeshBatch();
            Letters(ul, "UNDERCROFT", new Vector3(-60f, 3.5f, 98.54f), Vector3.back, 0.06f, 0.02f);
            ul.Build(kit.Root, "SignLetters", PatchworkAssets.Glow(new Color(1f, 0.82f, 0.45f), 0.9f));

            // Down below: a runner, lamps, stools by the windows, a painting of the tanks at the end.
            Rug(kit, new Vector3(-60f, floorY, 117f), new Vector2(1.8f, 16f), StarCloth, 0f, Brass);
            foreach (float z in new[] { 109f, 119.3f, 125.4f })
                PointLight(kit, new Vector3(-60f, -4.4f, z), LampGold, 7f, 0.9f);
            foreach (float z in new[] { 113.75f, 119.25f })
                foreach (float x in new[] { -61.95f, -58.05f })
                    Sconce(kit, new Vector3(x, -4.9f, z), x < -60f ? 90f : 270f, false);
            foreach (float wc in windows)
                foreach (float x in new[] { -61.1f, -58.9f })
                {
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(x, floorY + 0.3f, wc), new Vector3(0.6f, 0.3f, 0.6f), DarkWood);
                    NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(x, floorY + 0.62f, wc), new Vector3(0.62f, 0.03f, 0.62f), PatchworkAssets.Felt(Wine)));
                }
            Painting(kit, new Vector3(-60f, -5.6f, 125.94f), 180f, 3f, 2f, 901);
            Plaque(kit, new Vector3(-60f, floorY, 123.6f), 180f, "The Undercroft",
                "You're standing between two tanks. On your left, the Coral Nursery, as bright and busy as a reef in the shallows. On your right, the Lantern Deep, as dark as the sea a kilometre down.",
                "The keepers built this corridor so they could check on both without getting wet. They mostly come down here to eat their sandwiches.");
            Mark("Narration_Undercroft", new Vector3(-60f, floorY + 1f, 116f));
        }

        // ================================================================ Catwalks

        static void KwCatwalks(LevelKit kit)
        {
            var grate = Grating;
            float y = CatY;
            // Pieces of walkway (x0, x1, z0, z1), none overlapping: two across the hall and one
            // down the middle, with a wider feeding platform over the middle of each tank.
            float[][] walk =
            {
                new[] { KwX0, -76.5f, 111f, 113f }, new[] { -76.5f, -72.5f, 110f, 114f }, new[] { -72.5f, -48.5f, 111f, 113f }, new[] { -48.5f, -44.5f, 110f, 114f }, new[] { -44.5f, KwX1, 111f, 113f },
                new[] { KwX0, -76.5f, 154f, 156f }, new[] { -76.5f, -72.5f, 153f, 157f }, new[] { -72.5f, -48.5f, 154f, 156f }, new[] { -48.5f, -44.5f, 153f, 157f }, new[] { -44.5f, KwX1, 154f, 156f },
                new[] { -61f, -59f, 113f, 154f },
            };
            foreach (var w in walk)
            {
                KwBox(kit, w[0], w[1], y - 0.2f, y, w[2], w[3], grate, "Catwalk");
                // The grating's slats.
                var slats = new MeshBatch();
                bool alongX = w[1] - w[0] > w[3] - w[2];
                if (alongX) for (float x = w[0] + 0.25f; x < w[1]; x += 0.5f) slats.Box(new Vector3(x, y + 0.012f, (w[2] + w[3]) * 0.5f), new Vector3(0.06f, 0.02f, w[3] - w[2] - 0.1f), Quaternion.identity);
                else for (float z = w[2] + 0.25f; z < w[3]; z += 0.5f) slats.Box(new Vector3((w[0] + w[1]) * 0.5f, y + 0.012f, z), new Vector3(w[1] - w[0] - 0.1f, 0.02f, 0.06f), Quaternion.identity);
                slats.Build(kit.Root, "CatwalkSlats", IronMat);
            }
            // Handrails, left open where a walkway joins, at the stairs and at each diving board.
            float[][] rails =
            {
                // along the south edge of the south catwalk (gap at the west stair)
                new[] { -86f, 111f, -76.5f, 111f }, new[] { -76.5f, 110f, -75f, 110f }, new[] { -74f, 110f, -72.5f, 110f }, new[] { -72.5f, 111f, -48.5f, 111f },
                new[] { -48.5f, 110f, -47f, 110f }, new[] { -46f, 110f, -44.5f, 110f }, new[] { -44.5f, 111f, KwX1, 111f },
                // its north edge (gap where the middle walkway joins)
                new[] { KwX0, 113f, -76.5f, 113f }, new[] { -76.5f, 114f, -72.5f, 114f }, new[] { -72.5f, 113f, -61f, 113f }, new[] { -59f, 113f, -48.5f, 113f },
                new[] { -48.5f, 114f, -44.5f, 114f }, new[] { -44.5f, 113f, KwX1, 113f },
                // the north catwalk's south edge (gap at the middle walkway)
                new[] { KwX0, 154f, -76.5f, 154f }, new[] { -76.5f, 153f, -72.5f, 153f }, new[] { -72.5f, 154f, -61f, 154f }, new[] { -59f, 154f, -48.5f, 154f },
                new[] { -48.5f, 153f, -44.5f, 153f }, new[] { -44.5f, 154f, KwX1, 154f },
                // its north edge (diving boards out over the Frozen Sea and the Mangrove; the east stair)
                new[] { KwX0, 156f, -76.5f, 156f }, new[] { -76.5f, 157f, -75f, 157f }, new[] { -74f, 157f, -72.5f, 157f }, new[] { -72.5f, 156f, -48.5f, 156f },
                new[] { -48.5f, 157f, -47f, 157f }, new[] { -46f, 157f, -44.5f, 157f }, new[] { -44.5f, 156f, -36f, 156f },
                // the platform ends
                new[] { -76.5f, 110f, -76.5f, 111f }, new[] { -72.5f, 110f, -72.5f, 111f }, new[] { -48.5f, 110f, -48.5f, 111f }, new[] { -44.5f, 110f, -44.5f, 111f },
                new[] { -76.5f, 113f, -76.5f, 114f }, new[] { -72.5f, 113f, -72.5f, 114f }, new[] { -48.5f, 113f, -48.5f, 114f }, new[] { -44.5f, 113f, -44.5f, 114f },
                new[] { -76.5f, 153f, -76.5f, 154f }, new[] { -72.5f, 153f, -72.5f, 154f }, new[] { -48.5f, 153f, -48.5f, 154f }, new[] { -44.5f, 153f, -44.5f, 154f },
                new[] { -76.5f, 156f, -76.5f, 157f }, new[] { -72.5f, 156f, -72.5f, 157f }, new[] { -48.5f, 156f, -48.5f, 157f }, new[] { -44.5f, 156f, -44.5f, 157f },
                // the middle walkway
                new[] { -61f, 113f, -61f, 154f }, new[] { -59f, 113f, -59f, 154f },
            };
            var rail = Brass;
            foreach (var r in rails)
            {
                Vector3 a = new Vector3(r[0], y, r[1]), b = new Vector3(r[2], y, r[3]);
                Vector3 d = b - a;
                float len = d.magnitude;
                if (len < 0.1f) continue;
                float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
                Vector3 mid = (a + b) * 0.5f;
                NoShadow(kit.Deco(PrimitiveType.Cube, mid + Vector3.up * 1.05f, new Vector3(0.07f, 0.07f, len), rail, new Vector3(0f, yaw, 0f)));
                NoShadow(kit.Deco(PrimitiveType.Cube, mid + Vector3.up * 0.55f, new Vector3(0.04f, 0.04f, len), IronMat, new Vector3(0f, yaw, 0f)));
                int posts = Mathf.Max(1, Mathf.RoundToInt(len / 1.5f));
                for (int i = 0; i <= posts; i++)
                    NoShadow(kit.Deco(PrimitiveType.Cube, a + d * (i / (float)posts) + Vector3.up * 0.52f, new Vector3(0.06f, 1.05f, 0.06f), IronMat));
                TownKit.Solid(kit, mid + Vector3.up * 0.6f, new Vector3(0.12f, 1.2f, len), yaw);
            }
            // Diving boards: a springy plank out over each tank.
            var board = PatchworkAssets.Planks(new Color(0.75f, 0.6f, 0.42f));
            foreach (var bd in new[] { new Vector4(-74.5f, 110f, -1f, 0f), new Vector4(-46.5f, 110f, -1f, 0f), new Vector4(-74.5f, 157f, 1f, 0f), new Vector4(-46.5f, 157f, 1f, 0f) })
            {
                float zEnd = bd.y + bd.z * 3f;
                KwBox(kit, bd.x - 0.45f, bd.x + 0.45f, y - 0.1f, y + 0.08f, Mathf.Min(bd.y, zEnd), Mathf.Max(bd.y, zEnd), board, "DivingBoard");
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(bd.x, y + 0.1f, zEnd - bd.z * 0.25f), new Vector3(0.8f, 0.02f, 0.4f), PatchworkAssets.Felt(new Color(0.9f, 0.3f, 0.3f))));
                Mark("Dive_" + (bd.y < 120f ? (bd.x < -60f ? "Coral" : "Lantern") : (bd.x < -60f ? "Frozen" : "Mangrove")), new Vector3(bd.x, y, zEnd));
            }
            // Feeding buckets on each platform.
            foreach (var p in new[] { new Vector3(-75.8f, y, 113.3f), new Vector3(-45.2f, y, 113.3f), new Vector3(-75.8f, y, 153.7f), new Vector3(-45.2f, y, 153.7f) })
            {
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.3f, new Vector3(0.55f, 0.3f, 0.55f), PatchworkAssets.Plain(new Color(0.7f, 0.72f, 0.75f), 0.6f));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.56f, new Vector3(0.46f, 0.02f, 0.46f), PatchworkAssets.Felt(new Color(0.85f, 0.55f, 0.45f))));
            }
            // Hangers up to the roof trusses, and cage lamps along the walkways.
            foreach (var w in walk)
            {
                if (Mathf.Abs((w[1] - w[0]) - (w[3] - w[2])) < 0.5f) continue;   // (not on the platforms)
                bool alongX = w[1] - w[0] > w[3] - w[2];
                float len = alongX ? w[1] - w[0] : w[3] - w[2];
                for (float u = 2f; u < len - 1f; u += 6f)
                {
                    float hx = alongX ? w[0] + u : (w[0] + w[1]) * 0.5f, hz = alongX ? (w[2] + w[3]) * 0.5f : w[2] + u;
                    foreach (int s in new[] { -1, 1 })
                    {
                        Vector3 at = alongX ? new Vector3(hx, y + 1.1f, hz + s * 1.05f) : new Vector3(hx + s * 1.05f, y + 1.1f, hz);
                        Beam(kit, at, new Vector3(at.x, KwTop - 0.8f, at.z), 0.05f, IronMat);
                    }
                }
            }
            foreach (var p in new[] { new Vector3(-82f, y, 112f), new Vector3(-66f, y, 112f), new Vector3(-54f, y, 112f), new Vector3(-38f, y, 112f), new Vector3(-82f, y, 155f), new Vector3(-66f, y, 155f), new Vector3(-54f, y, 155f), new Vector3(-38f, y, 155f), new Vector3(-60f, y, 122f), new Vector3(-60f, y, 144f) })
            {
                NoShadow(kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 2.4f, new Vector3(0.36f, 0.44f, 0.36f), PatchworkAssets.Glow(LampGold, 2f)));
                for (int k = 0; k < 2; k++)
                    NoShadow(kit.Deco(PrimitiveType.Cube, p + Vector3.up * 2.4f, new Vector3(0.44f, 0.5f, 0.04f), IronMat, new Vector3(0f, k * 90f + 45f, 0f)));
                PointLight(kit, p + Vector3.up * 2.1f, LampGold, 9f, 0.8f);
            }
            // The stairs up: one on the west side by the Coral Nursery, one on the east by the Mangrove.
            Steps(kit, new Vector3(-87f, 0f, 98f), 0f, 28, CatY / 28f, 13f / 28f, 2f, DarkStone, 0f);
            Steps(kit, new Vector3(-35f, 0f, 169f), 180f, 28, CatY / 28f, 13f / 28f, 2f, DarkStone, 0f);
            Mark("EnemyPatrol_Catwalk_South", new Vector3(-60f, CatY, 112f));
            Mark("EnemyPatrol_Catwalk_North", new Vector3(-60f, CatY, 155f));
            Mark("EnemyPatrol_Catwalk_Middle", new Vector3(-60f, CatY, 133f));
        }

        // ================================================================ The Coral Nursery

        static void CoralNursery(LevelKit kit)
        {
            var T = CoralTank;
            float x0 = T[0], x1 = T[1], z0 = T[2], z1 = T[3], bed = T[4];
            float cx = (x0 + x1) * 0.5f, cz = (z0 + z1) * 0.5f;
            var rng = new System.Random(8101);
            System.Func<float, float, float> R = (a, b) => a + (float)rng.NextDouble() * (b - a);
            KwBox(kit, x0, x1, bed - 1f, bed, z0, z1, PatchworkAssets.Felt(new Color(0.88f, 0.8f, 0.62f)), "CoralSand");
            Pool(kit, x0, x1, z0, z1, WaterTop, bed, new Color(0.25f, 0.78f, 0.88f), "CoralNursery");
            // Sunlight: a rippling caustic spot from over the water, and shafts down through it.
            SpotLight(kit, new Vector3(cx, 6f, cz), new Vector3(cx, bed, cz), new Color(0.78f, 0.96f, 1f), 26f, 100f, 140f, false, true);
            PointLight(kit, new Vector3(cx, bed + 4f, cz), new Color(0.4f, 0.85f, 0.95f), 20f, 1.3f);
            PointLight(kit, new Vector3(-65f, bed + 3f, 116.5f), new Color(0.5f, 0.9f, 1f), 9f, 1.2f);   // (in front of the Undercroft windows)
            for (int i = 0; i < 6; i++)
            {
                Vector3 topP = new Vector3(R(x0 + 2f, x1 - 2f), WaterTop - 0.1f, R(z0 + 2f, z1 - 2f));
                LightBeam.Make(kit.Root, topP, topP + new Vector3(1.2f, bed + 0.5f - WaterTop, 0.8f), R(1.4f, 2.4f), new Color(0.75f, 1f, 1f), 0.45f);
            }
            // Ripples in the sand.
            var ripples = new MeshBatch();
            for (int i = 0; i < 16; i++)
                ripples.Box(new Vector3(R(x0 + 3f, x1 - 3f), bed + 0.02f, z0 + 1.5f + i * 1.6f), new Vector3(R(4f, 9f), 0.04f, 0.28f), Rot(R(-10f, 10f)));
            ripples.Build(kit.Root, "SandRipples", PatchworkAssets.Felt(new Color(0.78f, 0.7f, 0.54f)));

            // ----- Rock and coral banked up against the walls (not the Undercroft's side) -----
            var rock = new MeshBatch();
            Color[] coralCols = { new Color(1f, 0.45f, 0.5f), new Color(1f, 0.7f, 0.3f), new Color(0.75f, 0.5f, 1f), new Color(0.4f, 0.9f, 0.8f), new Color(1f, 0.55f, 0.8f), new Color(0.95f, 0.85f, 0.4f) };
            var coral = new MeshBatch[coralCols.Length];
            for (int i = 0; i < coral.Length; i++) coral[i] = new MeshBatch();
            System.Action<Vector3, float> Head = (p, s) =>
            {
                var mb = coral[rng.Next(coral.Length)];
                switch (rng.Next(6))
                {
                    case 0: mb.Ball(p + Vector3.up * s * 0.35f, new Vector3(s * 1.1f, s * 0.8f, s * 1.1f), 4, 8); break;            // brain coral
                    case 1:                                                                                                         // staghorn
                        for (int b = 0; b < 7; b++)
                        {
                            var br = Quaternion.Euler(R(-40f, 40f), R(0f, 360f), R(-40f, 40f));
                            mb.Box(p + br * new Vector3(0f, s * 0.5f, 0f), new Vector3(0.12f, s, 0.12f), br);
                            mb.Ball(p + br * new Vector3(0f, s, 0f), Vector3.one * 0.2f, 2, 5);
                        }
                        break;
                    case 2:                                                                                                         // table coral
                        mb.Cylinder(p, 0.18f, s * 0.6f, 6);
                        mb.Ball(p + Vector3.up * s * 0.65f, new Vector3(s * 2f, 0.16f, s * 1.8f), 3, 9);
                        break;
                    case 3:                                                                                                         // sea fan
                        mb.Ball(p + Vector3.up * s * 0.8f, new Vector3(s * 1.4f, s * 1.4f, 0.06f), 4, 9);
                        mb.Box(p + Vector3.up * s * 0.15f, new Vector3(0.1f, s * 0.4f, 0.1f), Quaternion.identity);
                        break;
                    case 4:                                                                                                         // tube sponges
                        for (int b = 0; b < 4; b++) mb.Cylinder(p + new Vector3(R(-0.3f, 0.3f), 0f, R(-0.3f, 0.3f)), R(0.12f, 0.2f), s * R(0.6f, 1.2f), 7);
                        break;
                    default:                                                                                                        // finger coral
                        for (int b = 0; b < 9; b++) mb.Ball(p + new Vector3(R(-0.4f, 0.4f), s * 0.3f, R(-0.4f, 0.4f)), new Vector3(0.14f, s * 0.6f, 0.14f), 3, 5);
                        break;
                }
            };
            for (float z = z0 + 1.5f; z < z1 - 1f; z += 2.6f)
            {
                if (z > 119f) break;   // (the little cave's in the far corner)
                Vector3 p = new Vector3(x0 + R(0.5f, 1.6f), bed, z);
                KwRocks(rock, p, R(1.2f, 2.2f), rng);
                Head(p + new Vector3(R(0.5f, 1.2f), R(0.8f, 1.8f), 0f), R(0.7f, 1.3f));
            }
            for (float x = x0 + 9f; x < x1 - 2f; x += 2.8f)
            {
                Vector3 p = new Vector3(x, bed, z1 - R(0.6f, 1.5f));
                if (Mathf.Abs(x + 75f) < 3f) continue;   // (the pipe to the Frozen Sea)
                KwRocks(rock, p, R(1.2f, 2.4f), rng);
                Head(p + new Vector3(0f, R(0.9f, 2f), -R(0.5f, 1f)), R(0.7f, 1.4f));
            }
            for (float x = x0 + 2f; x < x1 - 1f; x += 3f)
            {
                Vector3 p = new Vector3(x, bed, z0 + R(0.6f, 1.4f));
                KwRocks(rock, p, R(1f, 2f), rng);
                Head(p + new Vector3(0f, R(0.7f, 1.4f), R(0.4f, 0.8f)), R(0.6f, 1.2f));
            }
            // Low rocks along the Undercroft's wall, under and between its windows.
            for (float z = z0 + 2f; z < z1 - 1f; z += 2.2f)
            {
                Vector3 p = new Vector3(x1 - R(0.6f, 1.2f), bed, z);
                KwRocks(rock, p, R(0.5f, 0.9f), rng);
                if (rng.NextDouble() < 0.6) Head(p + Vector3.up * 0.4f, R(0.4f, 0.7f));
            }
            // Coral heads scattered across the floor.
            for (int i = 0; i < 22; i++)
            {
                Vector3 p = new Vector3(R(x0 + 4f, x1 - 4f), bed, R(z0 + 3f, z1 - 3f));
                if (p.x > -69f && p.z < 110f) continue;     // (the seagrass)
                if (Mathf.Abs(p.x + 78f) < 4f && Mathf.Abs(p.z - 106f) < 2.5f) continue;   // (the arch)
                if (p.x < -78f && p.z > 117f) continue;     // (the cave)
                KwRocks(rock, p, R(0.6f, 1.4f), rng);
                Head(p + Vector3.up * R(0.4f, 0.9f), R(0.6f, 1.3f));
            }

            // ----- The coral arch, to swim through -----
            Vector3 arch = new Vector3(-78f, bed, 106f);
            foreach (float dx in new[] { -2.8f, 2.8f })
            {
                Vector3 f = arch + new Vector3(dx, 0f, 0f);
                for (int k = 0; k < 4; k++) rock.Ball(f + new Vector3(R(-0.2f, 0.2f), 0.6f + k * 1.05f, R(-0.2f, 0.2f)), new Vector3(1.6f - k * 0.12f, 1.3f, 1.6f - k * 0.1f), 4, 7);
                TownKit.Solid(kit, f + Vector3.up * 2.1f, new Vector3(1.4f, 4.2f, 1.4f), 0f);
            }
            for (int k = 0; k < 6; k++) rock.Ball(arch + new Vector3(-3.2f + k * 1.3f, 4.6f + Mathf.Sin(k / 5f * Mathf.PI) * 0.5f, 0f), new Vector3(1.6f, 1.1f, 1.5f), 4, 7);
            TownKit.Solid(kit, arch + Vector3.up * 4.8f, new Vector3(7f, 1f, 1.4f), 0f);
            for (int k = 0; k < 5; k++) Head(arch + new Vector3(-2.6f + k * 1.3f, 5.3f, R(-0.3f, 0.3f)), R(0.5f, 0.9f));
            Trail(kit, 10, arch + new Vector3(0f, 2f, -4f), arch + new Vector3(0f, 2.1f, -1.5f), arch + new Vector3(0f, 2.2f, 1f), arch + new Vector3(0f, 2.1f, 3.5f), arch + new Vector3(0f, 2f, 6f));

            rock.Build(kit.Root, "NurseryRocks", PatchworkAssets.Felt(new Color(0.42f, 0.42f, 0.5f)), true);
            for (int i = 0; i < coral.Length; i++) Twinkling(coral[i], kit.Root, "NurseryCoral", coralCols[i], 0.4f, 0.65f, 0.35f + i * 0.07f, i * 1.3f);

            // ----- The coral "trees" of the nursery -----
            var pole = PatchworkAssets.Plain(new Color(0.92f, 0.92f, 0.9f), 0.4f);
            var frags = new MeshBatch[3];
            for (int i = 0; i < 3; i++) frags[i] = new MeshBatch();
            foreach (var tp in new[] { new Vector3(-71f, bed, 118f), new Vector3(-67.5f, bed, 122.5f), new Vector3(-70f, bed, 112.5f) })
            {
                kit.Deco(PrimitiveType.Cylinder, tp + Vector3.up * 2.6f, new Vector3(0.14f, 2.6f, 0.14f), pole);
                kit.Deco(PrimitiveType.Cylinder, tp + Vector3.up * 0.1f, new Vector3(0.8f, 0.1f, 0.8f), PatchworkAssets.Plain(new Color(0.5f, 0.5f, 0.52f), 0.3f));
                TownKit.Solid(kit, tp + Vector3.up * 2.6f, new Vector3(0.3f, 5.2f, 0.3f), 0f);
                for (int tier = 0; tier < 4; tier++)
                {
                    float ty = 1.4f + tier * 1.1f;
                    float yaw = tier * 45f + tp.z * 7f;
                    kit.Deco(PrimitiveType.Cube, tp + Vector3.up * ty, new Vector3(0.08f, 0.08f, 2.6f - tier * 0.3f), pole, new Vector3(0f, yaw, 0f));
                    for (int s = -1; s <= 1; s += 2)
                        for (int k = 0; k < 2; k++)
                        {
                            Vector3 hang = tp + Vector3.up * ty + Rot(yaw) * new Vector3(0f, 0f, s * (0.5f + k * 0.6f));
                            Beam(kit, hang, hang + Vector3.down * 0.35f, 0.015f, pole);
                            var fb = frags[(tier + k + (s > 0 ? 1 : 0)) % 3];
                            for (int b = 0; b < 4; b++) fb.Box(hang + Vector3.down * 0.45f + new Vector3(R(-0.1f, 0.1f), R(-0.05f, 0.05f), R(-0.1f, 0.1f)), new Vector3(0.05f, 0.3f, 0.05f), Quaternion.Euler(R(-40f, 40f), R(0f, 360f), R(-40f, 40f)));
                        }
                }
            }
            Color[] fragCols = { new Color(1f, 0.6f, 0.4f), new Color(0.8f, 0.55f, 1f), new Color(0.5f, 1f, 0.8f) };
            for (int i = 0; i < 3; i++) Twinkling(frags[i], kit.Root, "CoralFragments", fragCols[i], 0.5f, 0.9f, 0.5f, i);
            Plaque(kit, new Vector3(-69f, bed, 115.5f), 270f, "The Coral Nursery",
                "When a reef is damaged, keepers can help it grow back. Little broken pieces of living coral are hung on frames like these, where each one keeps growing.",
                "Once they're big enough, the pieces are planted back onto the reef, one by one, like a garden.");

            // ----- The seagrass meadow and its seahorses -----
            var grass = new MeshBatch();
            for (int i = 0; i < 320; i++)
            {
                Vector3 p = new Vector3(R(-68.5f, x1 - 1.5f), bed, R(z0 + 1f, 109.5f));
                float h = R(0.6f, 1.6f);
                var q = Quaternion.Euler(R(-14f, 14f), R(0f, 180f), R(-14f, 14f));
                grass.Box(p + q * new Vector3(0f, h * 0.5f, 0f), new Vector3(0.07f, h, 0.015f), q);
            }
            Twinkling(grass, kit.Root, "Seagrass", new Color(0.45f, 0.8f, 0.35f), 0.25f, 0.4f, 0.3f, 0f);
            Color[] horseCols = { new Color(1f, 0.75f, 0.3f), new Color(0.95f, 0.45f, 0.5f), new Color(0.7f, 0.85f, 0.45f) };
            for (int i = 0; i < 3; i++) KwSeahorse(kit, new Vector3(-66.5f + i * 1.2f, bed + 1.2f + i * 0.3f, 101.5f + i * 2.6f), 200f + i * 50f, horseCols[i]);

            // ----- Clownfish in their anemones -----
            foreach (var ap in new[] { new Vector3(-72f, bed, 101.5f), new Vector3(-79f, bed, 113f), new Vector3(-75f, bed, 120.5f) })
            {
                var arms = new MeshBatch();
                arms.Ball(ap + Vector3.up * 0.2f, new Vector3(1.1f, 0.4f, 1.1f), 3, 8);
                for (int t = 0; t < 22; t++)
                {
                    float a = t / 22f * Mathf.PI * 2f, rr = 0.2f + (t % 3) * 0.15f;
                    var q = Quaternion.Euler(Mathf.Sin(a) * 30f, 0f, -Mathf.Cos(a) * 30f);
                    arms.Ball(ap + new Vector3(Mathf.Cos(a) * rr, 0.55f, Mathf.Sin(a) * rr) + q * Vector3.up * 0.2f, new Vector3(0.1f, 0.55f, 0.1f), 2, 5);
                }
                Twinkling(arms, kit.Root, "Anemone", new Color(0.95f, 0.55f, 0.8f), 0.6f, 1.1f, 0.7f, ap.x);
                for (int f = 0; f < 2; f++)
                {
                    var fish = Swim(kit, Swimmer.Fish, ap + Vector3.up * 1f, 0.55f, 0.45f, f == 0 ? 60f : -50f, 0.26f, new Color(1f, 0.5f, 0.12f), new Color(1f, 1f, 1f), f * 160f);
                    fish.Bob = 0.15f;
                }
            }
            Plaque(kit, new Vector3(-76.5f, bed, 111f), 0f, "Clownfish",
                "A clownfish lives in among the stinging arms of an anemone, where nothing else dares to follow it. A coat of slime keeps it from getting stung.",
                "Every clownfish is born a boy. The biggest in each anemone turns into a girl, and becomes the boss.");

            // ----- The moray in its crevice -----
            Vector3 cre = new Vector3(x0 + 0.6f, bed, 112.5f);
            var crev = new MeshBatch();
            crev.Ball(cre + new Vector3(0.3f, 2.6f, 0f), new Vector3(2.4f, 1.6f, 3f), 4, 8);
            crev.Ball(cre + new Vector3(0.4f, 0.6f, -1.1f), new Vector3(2f, 1.4f, 1.6f), 4, 8);
            crev.Ball(cre + new Vector3(0.4f, 0.6f, 1.1f), new Vector3(2f, 1.4f, 1.6f), 4, 8);
            crev.Build(kit.Root, "Crevice", PatchworkAssets.Felt(new Color(0.36f, 0.36f, 0.44f)));
            NoShadow(kit.Deco(PrimitiveType.Cube, cre + new Vector3(0.2f, 1.45f, 0f), new Vector3(0.6f, 1.1f, 1.1f), PatchworkAssets.Felt(new Color(0.05f, 0.05f, 0.08f))));
            var eel = kit.Group("Moray", cre + new Vector3(0.5f, 1.45f, 0f));
            eel.rotation = Rot(90f);
            var eelMat = PatchworkAssets.Felt(new Color(0.42f, 0.55f, 0.2f));
            var spot = PatchworkAssets.Felt(new Color(0.85f, 0.85f, 0.5f));
            var head = new GameObject("Head").transform;
            head.SetParent(eel, false);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, -0.6f), new Vector3(0.5f, 0.6f, 1.8f), eelMat, default(Vector3), head);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.04f, 0.3f), new Vector3(0.42f, 0.48f, 0.7f), eelMat, default(Vector3), head);
            for (int i = 0; i < 6; i++)
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3((i % 2 == 0 ? 1f : -1f) * 0.22f, 0.05f + (i % 3) * 0.06f, -0.9f + i * 0.22f), Vector3.one * 0.09f, spot, default(Vector3), head));
            for (int s = -1; s <= 1; s += 2)
            {
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.17f, 0.16f, 0.45f), Vector3.one * 0.1f, PatchworkAssets.Glow(new Color(1f, 0.95f, 0.7f), 0.5f), default(Vector3), head));
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.2f, 0.16f, 0.47f), Vector3.one * 0.05f, PatchworkAssets.Plain(new Color(0.05f, 0.05f, 0.05f), 0.8f), default(Vector3), head));
            }
            var jaw = new GameObject("Jaw").transform;
            jaw.SetParent(head, false);
            jaw.localPosition = new Vector3(0f, -0.12f, 0.1f);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.05f, 0.25f), new Vector3(0.36f, 0.16f, 0.6f), eelMat, default(Vector3), jaw);
            var peek = eel.gameObject.AddComponent<MorayPeek>();
            peek.Head = head;
            peek.Jaw = jaw;

            // ----- A cleaning station in front of the Undercroft's middle window -----
            Vector3 cs = new Vector3(-65.6f, bed, 116.5f);
            var csRock = new MeshBatch();
            KwRocks(csRock, cs, 1.2f, rng);
            csRock.Build(kit.Root, "CleaningRock", PatchworkAssets.Felt(new Color(0.5f, 0.45f, 0.5f)));
            for (int i = 0; i < 4; i++)
            {
                var sh = kit.Group("CleanerShrimp", cs + new Vector3(R(-0.5f, 0.5f), 1.15f + R(0f, 0.3f), R(-0.5f, 0.5f)));
                sh.rotation = Rot(R(0f, 360f));
                kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.14f, 0.14f, 0.45f), PatchworkAssets.Felt(new Color(0.95f, 0.3f, 0.25f)), default(Vector3), sh);
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.07f, 0f), new Vector3(0.04f, 0.02f, 0.44f), PatchworkAssets.Felt(Color.white), default(Vector3), sh));
                for (int s = -1; s <= 1; s += 2)
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.08f, 0.15f, 0.45f), new Vector3(0.01f, 0.01f, 0.6f), PatchworkAssets.Felt(Color.white), new Vector3(-25f, s * 15f, 0f), sh));
                var b = sh.gameObject.AddComponent<SeaBob>();
                b.Height = 0.05f; b.Rock = 10f; b.Speed = 2f + i * 0.4f;
            }
            var grouper = Swim(kit, Swimmer.Fish, cs + new Vector3(0.4f, 2.4f, 0f), 0.35f, 0.5f, 6f, 1.7f, new Color(0.55f, 0.4f, 0.3f), new Color(0.85f, 0.75f, 0.55f), 0f);
            grouper.Bob = 0.2f;
            grouper.Wiggle = 2f;
            Plaque(kit, new Vector3(-67.4f, bed, 119.4f), 200f, "Cleaner Shrimp",
                "Big fish come to this rock and wait their turn, then hold very still while the little shrimp pick them clean, even inside their mouths. Nobody gets eaten.",
                "It's a fair deal: the big fish get rid of their parasites, and the shrimp get their dinner.");

            // ----- Who else lives here -----
            Shoal(kit, new Vector3(cx, bed + 4.5f, cz), 6f, 9f, 14f, 26, 0.34f, new Color(0.25f, 0.45f, 1f), 8111);
            Shoal(kit, new Vector3(cx - 2f, bed + 2.6f, cz + 3f), 5f, 6f, -18f, 18, 0.3f, new Color(1f, 0.88f, 0.25f), 8112, 90f);
            Swim(kit, Swimmer.Tall, new Vector3(cx, bed + 3.4f, cz - 3f), 7f, 8f, 9f, 0.9f, new Color(0.3f, 0.8f, 0.6f), new Color(1f, 0.5f, 0.65f), 30f);
            Swim(kit, Swimmer.Tall, new Vector3(cx + 2f, bed + 2.2f, cz + 4f), 5f, 6f, -11f, 0.7f, new Color(1f, 0.85f, 0.3f), new Color(0.15f, 0.15f, 0.3f), 200f);
            var turtle = Swim(kit, Swimmer.Turtle, new Vector3(cx, bed + 5.4f, cz), 7.5f, 10.5f, 6f, 1.3f, new Color(0.4f, 0.6f, 0.42f), new Color(0.85f, 0.78f, 0.52f), 120f);
            turtle.Bob = 0.8f;
            Swim(kit, Swimmer.Shark, new Vector3(cx, bed + 3f, cz), 8f, 11f, -8f, 0.9f, new Color(0.5f, 0.55f, 0.6f), CreamCol, 250f);
            Motes(kit, new Vector3(cx, (bed + WaterTop) * 0.5f, cz), new Vector3(x1 - x0 - 1f, -WaterTop - bed - 1f, z1 - z0 - 1f), 50, new Color(0.85f, 1f, 0.95f), 1.4f, 0.05f, new Vector3(0.04f, 0.03f, 0.05f));
            for (int i = 0; i < 4; i++) Bubbles(kit, new Vector3(R(x0 + 3f, x1 - 3f), bed + 0.3f, R(z0 + 3f, z1 - 3f)), -WaterTop - bed - 0.6f, 7);
            Trail(kit, 10, new Vector3(-66f, bed + 2.2f, 101f), new Vector3(-66f, bed + 2.2f, 104f), new Vector3(-66.5f, bed + 2.4f, 107f), new Vector3(-68f, bed + 2.6f, 110f));

            // ----- The little cave behind the sea fans (the far corner) -----
            float cb = bed;
            var cave = PatchworkAssets.Felt(new Color(0.38f, 0.38f, 0.46f));
            KwBox(kit, -81f, -80f, cb, cb + 3.6f, 120f, z1, cave, "CoralCave");
            KwBox(kit, x0, -80f, cb + 3.6f, cb + 4.4f, 120f, z1, cave, "CoralCave");
            KwBox(kit, x0, -84.8f, cb, cb + 3.6f, 120f, 121f, cave, "CoralCave");
            KwBox(kit, -82.4f, -81f, cb, cb + 3.6f, 120f, 121f, cave, "CoralCave");
            KwBox(kit, -84.8f, -82.4f, cb + 2.5f, cb + 3.6f, 120f, 121f, cave, "CoralCave");
            // (Three batches, roof, side and mouth, so none of their outlines take in the inside.)
            var caveRock = new MeshBatch();
            var caveSide = new MeshBatch();
            var caveMouth = new MeshBatch();
            for (int i = 0; i < 12; i++) caveRock.Ball(new Vector3(R(x0 + 0.5f, -80f), cb + 4.4f + R(-0.2f, 0.4f), R(120.5f, z1 - 0.5f)), new Vector3(R(1.4f, 2.4f), R(0.8f, 1.3f), R(1.4f, 2.2f)), 4, 7);
            for (int i = 0; i < 6; i++) caveSide.Ball(new Vector3(-80f + R(-0.1f, 0.6f), cb + R(0.5f, 3.4f), R(120.5f, z1 - 0.5f)), new Vector3(R(0.9f, 1.5f), R(0.8f, 1.4f), R(1.2f, 2f)), 4, 7);
            foreach (float x in new[] { -85.4f, -81.7f })
                caveMouth.Ball(new Vector3(x, cb + R(1f, 3f), 119.8f), new Vector3(1.4f, R(2f, 3f), 0.9f), 4, 7);
            caveRock.Build(kit.Root, "CaveRocks", cave, true);
            caveSide.Build(kit.Root, "CaveRocks", cave, true);
            caveMouth.Build(kit.Root, "CaveRocks", cave, true);
            for (int i = 0; i < 5; i++) Head(new Vector3(R(x0 + 1f, -80.5f), cb + 4.8f, R(121f, z1 - 1f)), R(0.6f, 1f));
            // The curtain of sea fans over the mouth (swim straight through them).
            var fans = new MeshBatch();
            for (int i = 0; i < 4; i++)
                fans.Ball(new Vector3(-84.6f + i * 0.75f, cb + 1.2f + (i % 2) * 0.5f, 119.6f - (i % 2) * 0.2f), new Vector3(1.3f, 1.8f, 0.05f), 4, 9);
            Twinkling(fans, kit.Root, "SeaFanCurtain", new Color(0.8f, 0.4f, 0.95f), 0.5f, 0.8f, 0.4f, 2f);
            // Inside: a lost toy submarine, glow-worms on the roof, and a marble.
            var glow = new MeshBatch();
            for (int i = 0; i < 26; i++) glow.Ball(new Vector3(R(x0 + 0.3f, -81.2f), cb + 3.55f, R(121.2f, z1 - 0.2f)), Vector3.one * R(0.05f, 0.1f), 2, 4);
            Twinkling(glow, kit.Root, "CaveGlow", new Color(0.5f, 1f, 0.9f), 0.4f, 2.4f, 1.3f, 0.6f);
            var sub = kit.Group("ToySubmarine", new Vector3(-84.6f, cb + 0.45f, 124.6f));
            sub.rotation = Quaternion.Euler(8f, 35f, 14f);
            var yel = PatchworkAssets.Plain(new Color(1f, 0.8f, 0.2f), 0.6f);
            kit.Deco(PrimitiveType.Capsule, Vector3.zero, new Vector3(0.5f, 0.6f, 0.5f), yel, new Vector3(90f, 0f, 0f), sub);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.3f, 0.05f), new Vector3(0.18f, 0.25f, 0.35f), yel, default(Vector3), sub);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0.24f, 0.04f, 0.2f), new Vector3(0.14f, 0.02f, 0.14f), PatchworkAssets.Glow(new Color(0.5f, 0.8f, 1f), 0.8f), new Vector3(0f, 0f, 90f), sub));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, -0.62f), new Vector3(0.32f, 0.04f, 0.08f), Brass, default(Vector3), sub));
            SecretMarble.Place(kit, new Vector3(-83.2f, cb + 1.3f, 123.8f), CoralMarble, new Color(1f, 0.55f, 0.75f));
            PointLight(kit, new Vector3(-83f, cb + 2.6f, 123.5f), new Color(0.7f, 1f, 0.9f), 6f, 1.3f);

            // ----- A message in a bottle, half-buried by the Undercroft wall -----
            var bottle = kit.Group("MessageBottle", new Vector3(-65f, bed + 0.12f, 124.6f));
            bottle.rotation = Quaternion.Euler(0f, 30f, 72f);
            NoShadow(kit.Deco(PrimitiveType.Capsule, Vector3.zero, new Vector3(0.28f, 0.32f, 0.28f), PatchworkAssets.Bubble(new Color(0.4f, 0.8f, 0.5f, 0.55f)), default(Vector3), bottle));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.38f, 0f), new Vector3(0.1f, 0.1f, 0.1f), PatchworkAssets.Bubble(new Color(0.4f, 0.8f, 0.5f, 0.55f)), default(Vector3), bottle));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.5f, 0f), new Vector3(0.09f, 0.04f, 0.09f), PatchworkAssets.Felt(new Color(0.65f, 0.45f, 0.3f)), default(Vector3), bottle));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.12f, 0.2f, 0.12f), PatchworkAssets.Felt(new Color(0.96f, 0.92f, 0.8f)), default(Vector3), bottle));
            Plaque(kit, new Vector3(-66.6f, bed, 123.4f), 135f, "A Message in a Bottle",
                "The note inside says: Dear Dorothea, we hope you like the big window. Don't let the sharks tell you what to do. Love, the Keepers.",
                "Dorothea is the turtle in the Great Window, out in the Atrium. Nobody knows how the bottle got in here.");
            Mark("Secret_CoralCave", new Vector3(-83f, bed + 1f, 123.5f));
            Mark("EnemyLair_CoralNursery", new Vector3(cx, bed + 2f, cz));
        }

        // A seahorse: a curled tail, a long snout and a little fin fluttering on its back.
        static void KwSeahorse(LevelKit kit, Vector3 at, float yaw, Color col)
        {
            var g = kit.Group("Seahorse", at);
            g.rotation = Rot(yaw);
            var m = PatchworkAssets.Felt(col);
            var dark = PatchworkAssets.Felt(col * 0.75f);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.38f, 0.04f), new Vector3(0.2f, 0.22f, 0.24f), m, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.36f, 0.22f), new Vector3(0.06f, 0.06f, 0.2f), m, default(Vector3), g);
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.22f - t * 0.42f, 0.02f + Mathf.Sin(t * Mathf.PI) * 0.08f), Vector3.one * (0.22f - t * 0.08f), i % 2 == 0 ? m : dark, default(Vector3), g);
            }
            for (int i = 0; i < 7; i++)
            {
                float a = i / 7f * Mathf.PI * 1.6f;
                float r = 0.12f - i * 0.012f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.28f - Mathf.Sin(a) * r, -0.02f - (1f - Mathf.Cos(a)) * r * 0.9f), Vector3.one * (0.1f - i * 0.009f), m, default(Vector3), g);
            }
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.05f, -0.1f), new Vector3(0.02f, 0.14f, 0.1f), PatchworkAssets.Glow(Color.Lerp(col, Color.white, 0.4f), 0.6f), default(Vector3), g));
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0.08f, 0.42f, 0.1f), Vector3.one * 0.05f, PatchworkAssets.Plain(new Color(0.05f, 0.05f, 0.05f), 0.8f), default(Vector3), g));
            var b = g.gameObject.AddComponent<SeaBob>();
            b.Height = 0.12f; b.Rock = 7f; b.Speed = 0.9f + at.z * 0.01f;
        }

        // ================================================================ The Lantern Deep

        static void LanternDeep(LevelKit kit)
        {
            var T = LanternTank;
            float x0 = T[0], x1 = T[1], z0 = T[2], z1 = T[3], bed = T[4];
            float cx = (x0 + x1) * 0.5f, cz = (z0 + z1) * 0.5f;
            var rng = new System.Random(8202);
            System.Func<float, float, float> R = (a, b) => a + (float)rng.NextDouble() * (b - a);
            KwBox(kit, x0, x1, bed - 1f, bed, z0, z1, PatchworkAssets.Felt(new Color(0.16f, 0.16f, 0.22f)), "DeepSilt");
            Pool(kit, x0, x1, z0, z1, WaterTop, bed, new Color(0.06f, 0.12f, 0.35f), "LanternDeep");
            // A faint blue near the top, and nothing at all further down but what glows.
            PointLight(kit, new Vector3(cx, -3f, cz), new Color(0.2f, 0.35f, 0.8f), 18f, 0.9f);
            PointLight(kit, new Vector3(-55.5f, -5.8f, 116.5f), new Color(0.35f, 0.5f, 1f), 9f, 0.9f);   // (by the Undercroft windows)
            // Dark rock walls, studded with glowing sponges.
            var rock = new MeshBatch();
            var sponge = new MeshBatch();
            for (int i = 0; i < 70; i++)
            {
                int side = rng.Next(4);
                float y = R(bed + 0.5f, -2f);
                Vector3 p = side == 0 ? new Vector3(x0 + 0.3f, y, R(z0, z1)) : side == 1 ? new Vector3(x1 - 0.3f, y, R(z0, z1)) : side == 2 ? new Vector3(R(x0, x1), y, z0 + 0.3f) : new Vector3(R(x0, x1), y, z1 - 0.3f);
                if (side == 0 && y > -8f && p.z > 108f && p.z < 125f) continue;   // (keep the Undercroft windows clear)
                if (side == 2 && Mathf.Abs(p.x + 46.5f) < 2.5f && y > -7.5f) continue;
                rock.Ball(p, new Vector3(R(0.8f, 2f), R(0.6f, 1.4f), R(0.8f, 2f)), 3, 6);
                if (rng.NextDouble() < 0.6) sponge.Ball(p + new Vector3(R(-0.4f, 0.4f), R(0.3f, 0.6f), R(-0.4f, 0.4f)), Vector3.one * R(0.1f, 0.22f), 2, 5);
            }
            // Ledges down the walls, to rest on on the way down (and for whatever lurks).
            var ledge = PatchworkAssets.Felt(new Color(0.18f, 0.18f, 0.24f));
            float[][] ledges = { new[] { x0, x0 + 3f, 100f, 104f, -6f }, new[] { x1 - 3f, x1, 103f, 109f, -10f }, new[] { -50f, -43f, z1 - 3f, z1, -13.5f }, new[] { x0, x0 + 4f, 112f, 120f, -17f }, new[] { x1 - 4f, x1, 116f, 124f, -20f } };
            foreach (var l in ledges)
            {
                KwBox(kit, l[0], l[1], l[4] - 0.6f, l[4], l[2], l[3], ledge, "DeepLedge");
                for (int k = 0; k < 4; k++) rock.Ball(new Vector3(R(l[0], l[1]), l[4] - 0.8f, R(l[2], l[3])), new Vector3(R(1f, 2f), R(0.6f, 1f), R(1f, 2f)), 3, 6);
                for (int k = 0; k < 5; k++) sponge.Ball(new Vector3(R(l[0] + 0.3f, l[1] - 0.3f), l[4] + 0.1f, R(l[2] + 0.3f, l[3] - 0.3f)), new Vector3(0.18f, R(0.2f, 0.6f), 0.18f), 2, 5);
            }
            rock.Build(kit.Root, "DeepRocks", PatchworkAssets.Felt(new Color(0.12f, 0.12f, 0.18f)));
            Twinkling(sponge, kit.Root, "GlowSponges", new Color(0.35f, 0.9f, 1f), 0.3f, 1.8f, 0.5f, 0f);

            // ----- Comb jellies, rippling rainbows down their sides -----
            for (int i = 0; i < 7; i++)
            {
                Vector3 p = new Vector3(R(x0 + 3f, x1 - 3f), R(bed + 4f, -3f), R(z0 + 3f, z1 - 3f));
                if (i < 2) p = new Vector3(-53.5f, -5.6f, 113f + i * 6f);   // two where the Undercroft can see them
                var j = kit.Group("CombJelly", p);
                float s = R(0.7f, 1.2f);
                NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.8f, 1.2f, 0.8f) * s, PatchworkAssets.Bubble(new Color(0.7f, 0.85f, 1f, 0.22f)), default(Vector3), j));
                var ribsA = new MeshBatch();
                var ribsB = new MeshBatch();
                for (int r = 0; r < 8; r++)
                {
                    float a = r / 8f * Mathf.PI * 2f;
                    var q = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f);
                    for (int k = 0; k < 5; k++)
                    {
                        float t = (k - 2f) / 2.4f;
                        float rad = 0.4f * Mathf.Sqrt(Mathf.Max(0.05f, 1f - t * t)) * s;
                        ((k + r) % 2 == 0 ? ribsA : ribsB).Box(new Vector3(Mathf.Cos(a) * rad, t * 0.55f * s, Mathf.Sin(a) * rad), new Vector3(0.05f, 0.16f * s, 0.06f), q);
                    }
                }
                Twinkling(ribsA, j, "CombRows", new Color(0.4f, 1f, 0.9f), 0.2f, 2.6f, 2.2f, i);
                Twinkling(ribsB, j, "CombRows", new Color(1f, 0.45f, 0.9f), 0.2f, 2.6f, 2.2f, i + 1.6f);
                var b = j.gameObject.AddComponent<SeaBob>();
                b.Height = R(0.4f, 0.9f); b.Rock = 8f; b.Speed = R(0.2f, 0.4f); b.Phase = i;
            }
            Plaque(kit, new Vector3(x0 + 1.5f, -6f, 102f), 90f, "Comb Jellies",
                "They look like jellyfish, but they aren't. They swim by beating rows of tiny hairs, and the hairs split the light into moving rainbows.",
                "Most of the animals that live this deep make their own light: to find each other, to hide, or to catch their dinner.");

            // ----- A siphonophore: a chain of many little animals living as one, hanging in the dark -----
            {
                var root = kit.Group("Siphonophore", new Vector3(-40.5f, -2.5f, 106f));
                var segs = new Transform[9];
                Transform parent = root;
                var mat = PatchworkAssets.Glow(new Color(0.55f, 0.8f, 1f), 1.6f);
                var red = PatchworkAssets.Glow(new Color(1f, 0.45f, 0.55f), 1.4f);
                for (int i = 0; i < segs.Length; i++)
                {
                    var sg = new GameObject("Seg").transform;
                    sg.SetParent(parent, false);
                    sg.localPosition = i == 0 ? Vector3.zero : new Vector3(0f, -1.4f, 0f);
                    var mb = new MeshBatch();
                    mb.Box(new Vector3(0f, -0.7f, 0f), new Vector3(0.03f, 1.4f, 0.03f), Quaternion.identity);
                    for (int k = 0; k < 3; k++)
                    {
                        mb.Ball(new Vector3(0f, -k * 0.45f, 0f), new Vector3(0.22f, 0.16f, 0.22f), 2, 6);
                        mb.Box(new Vector3(0.12f, -k * 0.45f - 0.25f, 0f), new Vector3(0.01f, 0.5f, 0.01f), Quaternion.Euler(0f, k * 70f, 12f));
                    }
                    mb.Build(sg, "Zooids", mat);
                    if (i % 2 == 1) NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.25f, 0f), Vector3.one * 0.12f, red, default(Vector3), sg));
                    segs[i] = sg;
                    parent = sg;
                }
                var sway = root.gameObject.AddComponent<KelpSway>();
                sway.Segments = segs;
                sway.Amount = 4f;
                sway.Speed = 0.3f;
            }

            // ----- Lanternfish, anglerfish and a vampire squid -----
            Shoal(kit, new Vector3(cx, -9f, cz), 6f, 9f, 11f, 30, 0.3f, new Color(0.45f, 0.75f, 1f), 8211);
            Shoal(kit, new Vector3(cx + 2f, -15f, cz - 2f), 5f, 7f, -9f, 20, 0.28f, new Color(0.6f, 1f, 0.85f), 8212, 120f);
            for (int i = 0; i < 2; i++)
            {
                float s = i == 0 ? 1.1f : 0.8f;
                var angler = Swim(kit, Swimmer.Angler, new Vector3(cx + (i == 0 ? -2f : 3f), bed + (i == 0 ? 4.5f : 8f), cz + (i == 0 ? 1f : -4f)), 5f + i * 2f, 7f, i == 0 ? 7f : -9f, s, new Color(0.18f, 0.16f, 0.22f), new Color(0.85f, 0.8f, 0.75f), i * 140f);
                angler.Bob = 0.5f;
                Vector3 lure = new Vector3(0f, 1.08f, 0.6f) * s;
                NoShadow(kit.Deco(PrimitiveType.Sphere, lure, Vector3.one * 0.2f * s, PatchworkAssets.Glow(new Color(0.65f, 1f, 0.9f), 3.6f), default(Vector3), angler.transform));
                var l = new GameObject("Lure").AddComponent<Light>();
                l.transform.SetParent(angler.transform, false);
                l.transform.localPosition = lure;
                l.type = LightType.Point;
                l.color = new Color(0.55f, 1f, 0.85f);
                l.range = 5f;
                l.intensity = 1.6f;
                l.shadows = LightShadows.None;
            }
            {
                var squid = kit.Group("VampireSquid", new Vector3(cx, -14f, cz));
                var model = new GameObject("Body").transform;
                model.SetParent(squid, false);
                var red = PatchworkAssets.Felt(new Color(0.45f, 0.08f, 0.12f));
                var web = PatchworkAssets.Felt(new Color(0.28f, 0.05f, 0.1f));
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, 0.5f), new Vector3(0.8f, 0.8f, 1.3f), red, default(Vector3), model);
                for (int s = -1; s <= 1; s += 2)
                {
                    kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.45f, 0.15f, 0.9f), new Vector3(0.45f, 0.05f, 0.3f), red, new Vector3(0f, s * -20f, s * 25f), model);
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.3f, 0.15f, 0.2f), Vector3.one * 0.26f, PatchworkAssets.Glow(new Color(0.35f, 0.65f, 1f), 2.6f), default(Vector3), model));
                }
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, -0.35f), new Vector3(1.6f, 0.5f, 1.1f), web, default(Vector3), model);
                for (int a = 0; a < 8; a++)
                {
                    float ang = a / 8f * 360f;
                    var q = Quaternion.Euler(0f, 0f, ang) * Quaternion.Euler(-12f, 0f, 0f);
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, -0.75f) + q * new Vector3(0f, 0.55f, 0f), new Vector3(0.08f, 0.6f, 0.08f), red, q.eulerAngles, model));
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, -0.75f) + q * new Vector3(0f, 0.85f, 0f), Vector3.one * 0.09f, PatchworkAssets.Glow(new Color(0.4f, 0.7f, 1f), 2f), default(Vector3), model));
                }
                var f = squid.gameObject.AddComponent<FishSwim>();
                f.Center = squid.position;
                f.RadiusX = 6f; f.RadiusZ = 9f; f.Speed = -5f; f.Angle = 90f; f.Bob = 1.2f; f.BobSpeed = 0.25f; f.Wiggle = 3f; f.WiggleSpeed = 1f; f.Bank = 4f;
            }
            // A dumbo octopus near the bottom, flapping its ear-fins.
            {
                var dumbo = kit.Group("DumboOctopus", new Vector3(-52f, bed + 2.6f, 108f));
                var pink = PatchworkAssets.Felt(new Color(1f, 0.7f, 0.7f));
                kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.7f, 0.8f, 0.7f), pink, default(Vector3), dumbo);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.4f, 0f), new Vector3(1.2f, 0.3f, 1.2f), pink, default(Vector3), dumbo);
                for (int s = -1; s <= 1; s += 2)
                {
                    kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.42f, 0.25f, 0f), new Vector3(0.4f, 0.12f, 0.3f), pink, new Vector3(0f, 0f, s * 30f), dumbo);
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.2f, 0.05f, 0.3f), Vector3.one * 0.1f, PatchworkAssets.Plain(new Color(0.05f, 0.05f, 0.05f), 0.8f), default(Vector3), dumbo));
                }
                var b = dumbo.gameObject.AddComponent<SeaBob>();
                b.Height = 0.6f; b.Rock = 14f; b.Speed = 0.8f;
            }

            // ----- The vent field at the bottom: black smokers and their villages -----
            var stack = PatchworkAssets.Plain(new Color(0.12f, 0.1f, 0.1f), 0.2f);
            var cracks = new MeshBatch();
            var tubes = new MeshBatch();
            var plumes = new MeshBatch();
            var crabs = new MeshBatch();
            foreach (var vp in new[] { new Vector3(-52.5f, bed, 103f), new Vector3(-41f, bed, 108.5f), new Vector3(-45f, bed, 121.5f) })
            {
                float h = 0f;
                for (int k = 0; k < 5; k++)
                {
                    float r = 1.4f - k * 0.2f, sh = R(0.9f, 1.4f);
                    kit.Deco(PrimitiveType.Cylinder, vp + new Vector3(R(-0.1f, 0.1f), h + sh * 0.5f, R(-0.1f, 0.1f)), new Vector3(r, sh * 0.5f, r), stack);
                    for (int c = 0; c < 3; c++)
                    {
                        float a = R(0f, Mathf.PI * 2f);
                        cracks.Box(vp + new Vector3(Mathf.Cos(a) * r * 0.5f, h + R(0.1f, sh), Mathf.Sin(a) * r * 0.5f), new Vector3(0.05f, R(0.3f, 0.7f), 0.05f), Quaternion.Euler(R(-20f, 20f), -a * Mathf.Rad2Deg, R(-20f, 20f)));
                    }
                    h += sh;
                }
                TownKit.Solid(kit, vp + Vector3.up * h * 0.5f, new Vector3(2.2f, h, 2.2f), 0f);
                // The smoke: dark specks pouring up out of the top.
                Motes(kit, vp + Vector3.up * (h + 4f), new Vector3(1.6f, 8f, 1.6f), 26, new Color(0.25f, 0.22f, 0.22f), 0.05f, 0.35f, new Vector3(0.02f, 1.1f, 0.02f));
                PointLight(kit, vp + Vector3.up * 1.5f, new Color(1f, 0.5f, 0.2f), 9f, 1.8f);
                // Giant tube worms round its foot, red plumes out of white tubes; and little white crabs.
                for (int w = 0; w < 14; w++)
                {
                    float a = R(0f, Mathf.PI * 2f), d = R(1.4f, 2.4f);
                    Vector3 wp = vp + new Vector3(Mathf.Cos(a) * d, 0f, Mathf.Sin(a) * d);
                    float th = R(0.6f, 1.6f);
                    tubes.Cylinder(wp, 0.07f, th, 6);
                    plumes.Ball(wp + Vector3.up * (th + 0.08f), new Vector3(0.2f, 0.26f, 0.2f), 2, 6);
                }
                for (int c = 0; c < 4; c++)
                {
                    float a = R(0f, Mathf.PI * 2f), d = R(2.2f, 3f);
                    Vector3 cp = vp + new Vector3(Mathf.Cos(a) * d, 0.12f, Mathf.Sin(a) * d);
                    crabs.Ball(cp, new Vector3(0.36f, 0.16f, 0.3f), 2, 6);
                    for (int lg = 0; lg < 6; lg++)
                        crabs.Box(cp + new Vector3((lg < 3 ? -1f : 1f) * 0.22f, -0.04f, (lg % 3 - 1) * 0.1f), new Vector3(0.24f, 0.03f, 0.03f), Quaternion.Euler(0f, (lg % 3 - 1) * 25f, (lg < 3 ? 1f : -1f) * 30f));
                }
            }
            Twinkling(cracks, kit.Root, "VentCracks", new Color(1f, 0.5f, 0.18f), 0.6f, 2.6f, 1.4f, 0f);
            tubes.Build(kit.Root, "TubeWorms", PatchworkAssets.Felt(new Color(0.92f, 0.9f, 0.85f)));
            Twinkling(plumes, kit.Root, "WormPlumes", new Color(1f, 0.25f, 0.25f), 0.7f, 1.2f, 0.6f, 1f);
            crabs.Build(kit.Root, "VentCrabs", PatchworkAssets.Felt(new Color(0.9f, 0.88f, 0.86f)));
            Plaque(kit, new Vector3(-48f, bed, 104.6f), 300f, "Black Smokers",
                "On the deep sea floor, water heated by the Earth gushes out of chimneys of rock, black with minerals. No sunlight ever gets this far down.",
                "But whole villages live round them anyway: tube worms, crabs, shrimp, fed by bacteria that make their food from the chemicals in the water instead of from the sun.");

            // ----- The Bathysphere -----
            Vector3 bs = new Vector3(-49.5f, bed + 1.45f, 116f);
            var ball = kit.Group("Bathysphere", bs);
            ball.rotation = Quaternion.Euler(0f, 0f, 8f);
            var steel = PatchworkAssets.Plain(new Color(0.3f, 0.36f, 0.42f), 0.55f);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 2.9f, steel, default(Vector3), ball);
            foreach (var d in new[] { new Vector3(0.35f, 0.2f, 1f), new Vector3(0.35f, 0.2f, -1f), new Vector3(1f, 0.35f, 0f) })
            {
                var dn = d.normalized;
                var q = Quaternion.LookRotation(dn) * Quaternion.Euler(90f, 0f, 0f);
                NoShadow(kit.Deco(PrimitiveType.Cylinder, dn * 1.42f, new Vector3(0.62f, 0.1f, 0.62f), steel, q.eulerAngles, ball));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, dn * 1.5f, new Vector3(0.38f, 0.04f, 0.38f), PatchworkAssets.Glow(new Color(0.55f, 0.85f, 0.9f), 0.9f), q.eulerAngles, ball));
            }
            // The hatch, standing open on its hinge (on the west side).
            var hatchRim = Quaternion.LookRotation(Vector3.left) * Quaternion.Euler(90f, 0f, 0f);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, Vector3.left * 1.38f, new Vector3(1.2f, 0.12f, 1.2f), steel, hatchRim.eulerAngles, ball));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, Vector3.left * 1.46f, new Vector3(0.95f, 0.04f, 0.95f), PatchworkAssets.Plain(new Color(0.03f, 0.03f, 0.05f), 0.3f), hatchRim.eulerAngles, ball));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(-1.75f, 0f, 0.65f), new Vector3(1.1f, 0.08f, 1.1f), steel, new Vector3(0f, 70f, 90f), ball));
            for (int b = 0; b < 12; b++)
            {
                float a = b / 12f * Mathf.PI * 2f;
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(-1.43f, Mathf.Sin(a) * 0.55f, Mathf.Cos(a) * 0.55f), Vector3.one * 0.1f, steel, default(Vector3), ball));
            }
            NoShadow(kit.Deco(PrimitiveType.Cylinder, Vector3.up * 1.5f, new Vector3(0.5f, 0.1f, 0.5f), steel, default(Vector3), ball));
            Beam(kit, bs + Vector3.up * 1.6f, new Vector3(bs.x - 0.3f, WaterTop - 0.2f, bs.z + 0.4f), 0.09f, IronMat);
            TownKit.Solid(kit, bs, new Vector3(2.5f, 2.5f, 2.5f), 0f);
            var silt = new MeshBatch();
            for (int i = 0; i < 6; i++) silt.Ball(bs + new Vector3(R(-1.2f, 1.2f), -1.4f, R(-1.2f, 1.2f)), new Vector3(R(1f, 1.8f), 0.4f, R(1f, 1.8f)), 3, 6);
            silt.Build(kit.Root, "Silt", PatchworkAssets.Felt(new Color(0.18f, 0.18f, 0.24f)));
            SecretMarble.Place(kit, bs + new Vector3(-2.1f, -0.15f, -0.8f), AbyssMarble, new Color(0.4f, 0.8f, 1f));
            PointLight(kit, bs + new Vector3(-2.6f, 0.4f, 0f), new Color(0.5f, 0.85f, 1f), 5f, 1.4f);
            Plaque(kit, new Vector3(-53f, bed, 112.5f), 140f, "The Bathysphere",
                "In 1934 William Beebe and Otis Barton squeezed into a hollow steel ball, hardly bigger than this one, and were lowered into the sea off Bermuda on a single cable.",
                "They went down 923 metres, deeper than anybody had ever been, and saw with their own eyes the lights of the animals that live there. Beebe talked to the ship above the whole way down, by telephone.");

            // ----- June's torch, dropped on a ledge, still shining -----
            Vector3 torch = new Vector3(x0 + 1.4f, -17f + 0.12f, 116f);
            var tg = kit.Group("JunesTorch", torch);
            tg.rotation = Rot(80f);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.16f, 0.25f, 0.16f), PatchworkAssets.Plain(new Color(0.8f, 0.2f, 0.2f), 0.6f), new Vector3(90f, 0f, 0f), tg);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.27f), new Vector3(0.22f, 0.04f, 0.22f), PatchworkAssets.Glow(new Color(1f, 0.95f, 0.75f), 3f), new Vector3(90f, 0f, 0f), tg));
            SpotLight(kit, torch + Rot(80f) * new Vector3(0f, 0.05f, 0.35f), torch + Rot(80f) * new Vector3(0f, -0.2f, 8f), new Color(1f, 0.92f, 0.7f), 12f, 30f, 30f, false, false);
            LightBeam.Make(kit.Root, torch + Rot(80f) * new Vector3(0f, 0.05f, 0.4f), torch + Rot(80f) * new Vector3(0f, -0.1f, 5f), 0.7f, new Color(1f, 0.95f, 0.8f), 0.35f);
            Motes(kit, new Vector3(cx, (bed + WaterTop) * 0.5f, cz), new Vector3(x1 - x0 - 1f, -WaterTop - bed - 1f, z1 - z0 - 1f), 70, new Color(0.5f, 0.8f, 1f), 1.8f, 0.05f, new Vector3(0.02f, -0.06f, 0.03f));
            Trail(kit, 10, new Vector3(cx, -4f, cz), new Vector3(cx + 1f, -8f, cz + 1f), new Vector3(cx, -12f, cz + 2f), new Vector3(cx - 1f, -16f, cz + 1f), new Vector3(cx, -20f, cz));
            Mark("Secret_Bathysphere", bs + Vector3.left * 2f);
            Mark("Secret_JunesTorch", torch);
            Mark("EnemyLair_LanternDeep", new Vector3(cx, bed + 3f, cz));
        }

        // ================================================================ The Frozen Sea

        static void FrozenSea(LevelKit kit)
        {
            var T = FrozenTank;
            float x0 = T[0], x1 = T[1], z0 = T[2], z1 = T[3], bed = T[4];
            float cx = (x0 + x1) * 0.5f, cz = (z0 + z1) * 0.5f;
            var rng = new System.Random(8303);
            System.Func<float, float, float> R = (a, b) => a + (float)rng.NextDouble() * (b - a);
            KwBox(kit, x0, x1, bed - 1f, bed, z0, z1, PatchworkAssets.Felt(new Color(0.55f, 0.6f, 0.68f)), "IceSeaBed");
            Pool(kit, x0, x1, z0, z1, WaterTop, bed, new Color(0.55f, 0.8f, 0.95f), "FrozenSea");
            SpotLight(kit, new Vector3(cx, 7f, cz), new Vector3(cx, bed, cz), new Color(0.7f, 0.85f, 1f), 28f, 105f, 120f, false, true);
            PointLight(kit, new Vector3(cx, bed + 5f, cz), new Color(0.5f, 0.75f, 1f), 26f, 1.2f);

            // ----- The ice floes: solid to walk on, snow on top, icicles under -----
            var ice = PatchworkAssets.Glow(new Color(0.7f, 0.88f, 1f), 0.3f);
            var snow = PatchworkAssets.Felt(new Color(0.96f, 0.97f, 1f));
            var icicles = new MeshBatch();
            var mounds = new MeshBatch();
            float[][] floes = { new[] { x0, -77f, 162f, z1 }, new[] { -73f, -67f, 149f, 155f }, new[] { -85f, -80f, 139f, 145f }, new[] { -71f, -64f, 165f, 170f }, new[] { -78f, -75f, 136f, 139f } };
            foreach (var f in floes)
            {
                KwBox(kit, f[0], f[1], -1.3f, 0.25f, f[2], f[3], ice, "IceFloe");
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3((f[0] + f[1]) * 0.5f, 0.31f, (f[2] + f[3]) * 0.5f), new Vector3(f[1] - f[0] - 0.2f, 0.12f, f[3] - f[2] - 0.2f), snow));
                float area = (f[1] - f[0]) * (f[3] - f[2]);
                for (int i = 0; i < area / 3f; i++)
                {
                    Vector3 p = new Vector3(R(f[0] + 0.3f, f[1] - 0.3f), -1.3f, R(f[2] + 0.3f, f[3] - 0.3f));
                    float h = R(0.4f, 1.6f);
                    icicles.Ball(p + Vector3.down * h * 0.45f, new Vector3(R(0.12f, 0.3f), h, R(0.12f, 0.3f)), 3, 5);
                }
                for (int i = 0; i < area / 12f; i++)
                    mounds.Ball(new Vector3(R(f[0] + 0.8f, f[1] - 0.8f), 0.35f, R(f[2] + 0.8f, f[3] - 0.8f)), new Vector3(R(0.8f, 1.8f), R(0.3f, 0.7f), R(0.8f, 1.8f)), 3, 7);
            }
            Twinkling(icicles, kit.Root, "Icicles", new Color(0.75f, 0.92f, 1f), 0.4f, 0.9f, 0.4f, 0f);
            mounds.Build(kit.Root, "SnowMounds", snow);
            // Light coming down through the gaps between the floes.
            for (int i = 0; i < 6; i++)
            {
                Vector3 topP = new Vector3(R(-76f, -66f), WaterTop - 0.1f, R(140f, 162f));
                LightBeam.Make(kit.Root, topP, topP + new Vector3(0.6f, bed + 0.5f - WaterTop, 0.4f), R(1.2f, 2.2f), new Color(0.8f, 0.92f, 1f), 0.4f);
            }
            // Brinicles: icy fingers reaching down from the floes to the sea bed.
            var brin = new MeshBatch();
            foreach (var bp in new[] { new Vector3(-70f, 0f, 152f), new Vector3(-82.5f, 0f, 142f), new Vector3(-80f, 0f, 168f) })
                for (int k = 0; k < 9; k++)
                {
                    float y = -1.3f - k * ((-1.3f - bed) / 9f) - 0.5f;
                    brin.Ball(new Vector3(bp.x + Mathf.Sin(k * 1.3f) * 0.15f, y, bp.z + Mathf.Cos(k * 0.9f) * 0.15f), new Vector3(0.5f - k * 0.03f, 1.6f, 0.5f - k * 0.03f), 3, 6);
                }
            Twinkling(brin, kit.Root, "Brinicles", new Color(0.85f, 0.95f, 1f), 0.5f, 1.1f, 0.3f, 1f);

            // ----- The penguin colony on the big floe, and a seal on the little one -----
            for (int i = 0; i < 9; i++)
                KwPenguin(kit, new Vector3(R(x0 + 1.2f, -78.5f), 0.37f, R(164f, z1 - 1f)), R(120f, 240f), R(0.9f, 1.15f), false);
            KwPenguin(kit, new Vector3(-81.5f, 0.37f, 166f), 170f, 0.75f, true);
            KwPenguin(kit, new Vector3(-82.6f, 0.37f, 165.6f), 150f, 0.7f, true);
            KwPenguin(kit, new Vector3(-67.6f, 0.37f, 152f), 90f, 1f, false);   // one at the edge, working up its nerve
            {
                Vector3 sp = new Vector3(-82.5f, 0.37f, 142f);
                var seal = kit.Group("Seal", sp);
                seal.rotation = Rot(30f);
                var grey = PatchworkAssets.Felt(new Color(0.52f, 0.52f, 0.56f));
                var spots = PatchworkAssets.Felt(new Color(0.35f, 0.35f, 0.4f));
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.45f, 0f), new Vector3(1.1f, 0.9f, 2.6f), grey, default(Vector3), seal);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.65f, 1.35f), new Vector3(0.7f, 0.65f, 0.75f), grey, default(Vector3), seal);
                for (int i = 0; i < 6; i++) NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(R(-0.4f, 0.4f), 0.75f, R(-0.9f, 0.9f)), new Vector3(0.18f, 0.06f, 0.22f), spots, default(Vector3), seal));
                for (int s = -1; s <= 1; s += 2)
                {
                    kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.55f, 0.15f, 0.5f), new Vector3(0.4f, 0.06f, 0.3f), grey, new Vector3(0f, s * 20f, s * -10f), seal);
                    kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.2f, 0.25f, -1.4f), new Vector3(0.3f, 0.06f, 0.5f), grey, new Vector3(0f, s * 25f, 0f), seal);
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.2f, 0.78f, 1.62f), Vector3.one * 0.1f, PatchworkAssets.Plain(new Color(0.03f, 0.03f, 0.03f), 0.9f), default(Vector3), seal));
                    for (int w = 0; w < 3; w++)
                        NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.22f, 0.55f + w * 0.04f, 1.72f), new Vector3(0.3f, 0.01f, 0.01f), PatchworkAssets.Felt(Color.white), new Vector3(0f, s * -15f, w * 8f - 8f), seal));
                }
                var b = seal.gameObject.AddComponent<SeaBob>();
                b.Height = 0.03f; b.Rock = 1.5f; b.Speed = 0.6f;
                TownKit.Solid(kit, sp + Vector3.up * 0.5f, new Vector3(1.2f, 1f, 3f), 30f);
            }
            // Penguins flying through the water underneath.
            for (int i = 0; i < 4; i++)
            {
                Vector3 c = new Vector3(cx + R(-2f, 2f), R(-7f, -3f), cz + R(-6f, 6f));
                var holder = kit.Group("SwimmingPenguin", c);
                var model = KwPenguin(kit, c, 0f, 0.85f, false);
                model.SetParent(holder, false);
                model.localPosition = new Vector3(0f, 0f, -0.6f);
                model.localRotation = Quaternion.Euler(90f, 0f, 0f);
                var f = holder.gameObject.AddComponent<FishSwim>();
                f.Center = c; f.RadiusX = R(4f, 8f); f.RadiusZ = R(8f, 14f); f.Speed = (i % 2 == 0 ? 1f : -1f) * R(26f, 40f); f.Angle = i * 90f; f.Bob = 1.4f; f.BobSpeed = 0.9f; f.Wiggle = 8f; f.WiggleSpeed = 4f; f.Bank = 18f;
            }
            Plaque(kit, new Vector3(-80f, 0.37f, 170.5f), 0f, "Emperor Penguins",
                "All through the Antarctic winter, in the dark and the bitter wind, the emperor penguin fathers stand huddled together, each keeping an egg warm on his feet.",
                "They go about two months without eating anything at all, until the chicks hatch and the mothers come back from the sea.");

            // ----- The narwhal -----
            {
                Vector3 c = new Vector3(cx, -7.2f, cz);
                var holder = kit.Group("Narwhal", c);
                var body = new GameObject("Body").transform;
                body.SetParent(holder, false);
                var skin = PatchworkAssets.Felt(new Color(0.62f, 0.64f, 0.66f));
                var mottle = PatchworkAssets.Felt(new Color(0.36f, 0.38f, 0.42f));
                var ivory = PatchworkAssets.Felt(new Color(0.96f, 0.93f, 0.84f));
                kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(1.2f, 1.1f, 3.8f), skin, default(Vector3), body);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.05f, 1.45f), new Vector3(1f, 0.95f, 1.2f), skin, default(Vector3), body);
                for (int i = 0; i < 14; i++) NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(R(-0.5f, 0.5f), R(0.25f, 0.5f), R(-1.6f, 1.4f)), new Vector3(0.2f, 0.05f, 0.25f), mottle, default(Vector3), body));
                for (int s = -1; s <= 1; s += 2)
                {
                    kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.6f, -0.25f, 0.9f), new Vector3(0.6f, 0.05f, 0.3f), skin, new Vector3(0f, s * -25f, s * -20f), body);
                    kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.4f, 0f, -2.05f), new Vector3(0.8f, 0.06f, 0.4f), skin, new Vector3(0f, s * 28f, 0f), body);
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.4f, 0.12f, 1.75f), Vector3.one * 0.08f, PatchworkAssets.Plain(new Color(0.03f, 0.03f, 0.03f), 0.9f), default(Vector3), body));
                }
                // The tusk: a long spiral tooth.
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0.12f, 0.08f, 3.3f), new Vector3(0.12f, 1.3f, 0.12f), ivory, new Vector3(90f, 0f, 0f), body);
                for (int k = 0; k < 18; k++)
                {
                    float a = k * 0.9f, t = k / 17f;
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0.12f + Mathf.Cos(a) * 0.065f * (1f - t * 0.6f), 0.08f + Mathf.Sin(a) * 0.065f * (1f - t * 0.6f), 2.1f + t * 2.4f), new Vector3(0.03f, 0.03f, 0.16f), PatchworkAssets.Felt(new Color(0.8f, 0.76f, 0.66f)), new Vector3(0f, 0f, a * Mathf.Rad2Deg), body));
                }
                var f = holder.gameObject.AddComponent<FishSwim>();
                f.Center = c; f.RadiusX = 6.5f; f.RadiusZ = 14f; f.Speed = 8f; f.Bob = 1f; f.BobSpeed = 0.3f; f.Wiggle = 3f; f.WiggleSpeed = 1.6f; f.Bank = 6f;
            }
            Plaque(kit, new Vector3(-65f, bed, 146f), 270f, "The Narwhal",
                "The narwhal's tusk isn't a horn at all: it's a tooth, growing straight out through its lip in a long left-handed spiral. It can be nearly three metres long.",
                "Sailors used to sell narwhal tusks in Europe as unicorn horns, for a great deal of money.");

            // ----- The sea bed: rocks, white anemones, starfish, giant sea spiders, krill -----
            var rock = new MeshBatch();
            var anem = new MeshBatch();
            var stars = new MeshBatch();
            var spider = new MeshBatch();
            for (int i = 0; i < 26; i++)
            {
                Vector3 p = new Vector3(R(x0 + 1.5f, x1 - 1.5f), bed, R(z0 + 1.5f, z1 - 1.5f));
                if (p.x > -71f && p.z > 166.5f) continue;   // (the ice cave)
                if (i % 2 == 0) KwRocks(rock, p, R(0.7f, 1.6f), rng);
                else
                {
                    for (int t = 0; t < 12; t++)
                    {
                        float a = t / 12f * Mathf.PI * 2f;
                        anem.Ball(p + new Vector3(Mathf.Cos(a) * 0.2f, 0.45f, Mathf.Sin(a) * 0.2f), new Vector3(0.06f, 0.4f, 0.06f), 2, 4);
                    }
                    anem.Ball(p + Vector3.up * 0.15f, new Vector3(0.5f, 0.35f, 0.5f), 3, 6);
                }
            }
            for (int i = 0; i < 14; i++)
            {
                Vector3 p = new Vector3(R(x0 + 1f, x1 - 1f), bed + 0.03f, R(z0 + 1f, z1 - 1f));
                for (int arm = 0; arm < 5; arm++) stars.Box(p + Rot(arm * 72f + i * 17f) * new Vector3(0f, 0f, 0.2f), new Vector3(0.12f, 0.05f, 0.4f), Rot(arm * 72f + i * 17f));
            }
            for (int i = 0; i < 4; i++)
            {
                Vector3 p = new Vector3(R(x0 + 3f, x1 - 3f), bed + 0.6f, R(z0 + 3f, 160f));
                spider.Ball(p, new Vector3(0.25f, 0.15f, 0.4f), 2, 5);
                for (int lg = 0; lg < 8; lg++)
                {
                    float a = (lg / 8f) * 360f + 22f;
                    var q = Rot(a);
                    Vector3 knee = p + q * new Vector3(0f, 0.25f, 0.6f);
                    Vector3 foot = p + q * new Vector3(0f, -0.6f, 1.2f);
                    spider.Box((p + knee) * 0.5f, new Vector3(0.04f, 0.04f, Vector3.Distance(p, knee)), Quaternion.LookRotation(knee - p));
                    spider.Box((knee + foot) * 0.5f, new Vector3(0.04f, 0.04f, Vector3.Distance(knee, foot)), Quaternion.LookRotation(foot - knee));
                }
            }
            rock.Build(kit.Root, "IceRocks", PatchworkAssets.Felt(new Color(0.42f, 0.46f, 0.54f)));
            Twinkling(anem, kit.Root, "WhiteAnemones", new Color(0.95f, 0.95f, 1f), 0.4f, 0.9f, 0.5f, 0f);
            stars.Build(kit.Root, "IceStarfish", PatchworkAssets.Felt(new Color(1f, 0.55f, 0.35f)));
            spider.Build(kit.Root, "SeaSpiders", PatchworkAssets.Felt(new Color(0.9f, 0.6f, 0.45f)));
            Motes(kit, new Vector3(cx, -6f, cz), new Vector3(16f, 7f, 30f), 80, new Color(1f, 0.6f, 0.55f), 1.2f, 0.06f, new Vector3(0.12f, 0.02f, 0.18f));   // krill
            Plaque(kit, new Vector3(-80f, bed, 150f), 90f, "The Endurance",
                "In 1915 the ice of the Weddell Sea crushed Ernest Shackleton's ship, the Endurance. Her crew camped on the floating ice for months, then rowed and sailed to safety. Every one of them came home alive.",
                "In 2022 the wreck was found again, three kilometres down at the bottom of the sea, still standing upright, with her name still there on her stern.");
            for (int i = 0; i < 3; i++) Bubbles(kit, new Vector3(R(x0 + 3f, x1 - 3f), bed + 0.3f, R(z0 + 3f, 160f)), -WaterTop - bed - 2f, 6);
            Trail(kit, 10, new Vector3(-74f, -3.5f, 140f), new Vector3(-73f, -4f, 145f), new Vector3(-71f, -4.5f, 150f), new Vector3(-72f, -5f, 156f), new Vector3(-74f, -5f, 161f));

            // ----- The ice cave at the far end -----
            var caveIce = PatchworkAssets.Glow(new Color(0.6f, 0.8f, 1f), 0.4f);
            KwBox(kit, -70f, x1, -6.8f, -6f, 168f, z1, caveIce, "IceCave");
            KwBox(kit, -70f, -69.2f, bed, -6.8f, 168.8f, z1, caveIce, "IceCave");
            KwBox(kit, -70f, -67.8f, bed, -6.8f, 168f, 168.8f, caveIce, "IceCave");
            KwBox(kit, -65.4f, x1, bed, -6.8f, 168f, 168.8f, caveIce, "IceCave");
            KwBox(kit, -67.8f, -65.4f, -9f, -6.8f, 168f, 168.8f, caveIce, "IceCave");
            var chunks = new MeshBatch();
            var chunkSide = new MeshBatch();
            var chunkMouth = new MeshBatch();
            for (int i = 0; i < 14; i++) chunks.Ball(new Vector3(R(-70.5f, x1 - 0.3f), R(-6.2f, -5.4f), R(168f, z1 - 0.3f)), new Vector3(R(1f, 2f), R(0.7f, 1.2f), R(1f, 2f)), 3, 6);
            for (int i = 0; i < 8; i++) chunkSide.Ball(new Vector3(R(-70.6f, -69.8f), R(bed + 0.5f, -7f), R(169f, z1 - 0.5f)), new Vector3(R(0.8f, 1.4f), R(1f, 2f), R(1f, 1.6f)), 3, 6);
            foreach (float x in new[] { -68.4f, -64.8f })
                chunkMouth.Ball(new Vector3(x, R(-11f, -8f), 167.8f), new Vector3(1.4f, R(2.4f, 3.4f), 0.9f), 3, 6);
            Twinkling(chunks, kit.Root, "IceCaveChunks", new Color(0.65f, 0.85f, 1f), 0.3f, 0.6f, 0.3f, 0.5f);
            Twinkling(chunkSide, kit.Root, "IceCaveChunks", new Color(0.65f, 0.85f, 1f), 0.3f, 0.6f, 0.3f, 0.9f);
            Twinkling(chunkMouth, kit.Root, "IceCaveChunks", new Color(0.65f, 0.85f, 1f), 0.3f, 0.6f, 0.3f, 1.3f);
            // Inside: an explorer's old sledge, a lantern still lit, frozen crystals and the marble.
            Vector3 sl = new Vector3(-66.8f, bed + 0.1f, 172.6f);
            var sledge = kit.Group("Sledge", sl);
            sledge.rotation = Quaternion.Euler(0f, 20f, 6f);
            var plank = PatchworkAssets.Planks(new Color(0.55f, 0.4f, 0.28f));
            for (int i = 0; i < 5; i++) kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.5f, -0.9f + i * 0.45f), new Vector3(1.1f, 0.06f, 0.35f), plank, default(Vector3), sledge);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.5f, 0.1f, 0f), new Vector3(0.06f, 0.06f, 2.4f), plank, default(Vector3), sledge);
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.5f, 0.3f, 1.25f), new Vector3(0.06f, 0.4f, 0.06f), plank, new Vector3(-35f, 0f, 0f), sledge);
                for (int k = 0; k < 3; k++) kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.5f, 0.3f, -0.8f + k * 0.8f), new Vector3(0.05f, 0.4f, 0.05f), plank, default(Vector3), sledge);
            }
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.75f, -0.3f), new Vector3(0.8f, 0.4f, 0.6f), PatchworkAssets.Felt(new Color(0.55f, 0.35f, 0.25f)), new Vector3(0f, 8f, 0f), sledge);
            var lantern = kit.Group("IceLantern", new Vector3(-68.4f, bed + 0.3f, 174.1f));
            NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.up * 0.25f, new Vector3(0.3f, 0.4f, 0.3f), PatchworkAssets.Glow(new Color(1f, 0.8f, 0.45f), 2.6f), default(Vector3), lantern));
            kit.Deco(PrimitiveType.Cylinder, Vector3.up * 0.5f, new Vector3(0.36f, 0.04f, 0.36f), IronMat, default(Vector3), lantern);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.36f, 0.04f, 0.36f), IronMat, default(Vector3), lantern);
            PointLight(kit, new Vector3(-67.5f, bed + 1.2f, 172.5f), new Color(1f, 0.82f, 0.5f), 7f, 1.6f);
            var crystals = new MeshBatch();
            for (int i = 0; i < 18; i++)
            {
                var q = Quaternion.Euler(R(-30f, 30f), R(0f, 360f), R(-30f, 30f));
                crystals.Box(new Vector3(R(-69f, x1 - 0.4f), -6.9f, R(169.2f, z1 - 0.3f)) + q * Vector3.down * 0.3f, new Vector3(0.12f, R(0.4f, 0.9f), 0.12f), q);
            }
            Twinkling(crystals, kit.Root, "IceCrystals", new Color(0.7f, 0.95f, 1f), 0.5f, 2.4f, 1.1f, 0.2f);
            SecretMarble.Place(kit, new Vector3(-65.4f, bed + 1.3f, 171.6f), IceMarble, new Color(0.7f, 0.9f, 1f));
            Mark("Secret_IceCave", new Vector3(-66.5f, bed + 1f, 172f));
            Mark("EnemyLair_FrozenSea", new Vector3(cx, -6f, cz));
        }

        // An emperor penguin (or its fluffy grey chick), standing on its own feet at "feet".
        static Transform KwPenguin(LevelKit kit, Vector3 feet, float yaw, float s, bool chick)
        {
            var g = kit.Group(chick ? "PenguinChick" : "Penguin", feet);
            g.rotation = Rot(yaw);
            var black = PatchworkAssets.Felt(new Color(0.12f, 0.13f, 0.18f));
            var white = PatchworkAssets.Felt(new Color(0.96f, 0.96f, 0.94f));
            var eye = PatchworkAssets.Plain(new Color(0.03f, 0.03f, 0.03f), 0.9f);
            if (chick)
            {
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.38f, 0f) * s, new Vector3(0.62f, 0.72f, 0.58f) * s, PatchworkAssets.Felt(new Color(0.62f, 0.62f, 0.66f)), default(Vector3), g);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.86f, 0.02f) * s, new Vector3(0.44f, 0.42f, 0.42f) * s, black, default(Vector3), g);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.84f, 0.12f) * s, new Vector3(0.34f, 0.26f, 0.3f) * s, white, default(Vector3), g);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.84f, 0.29f) * s, new Vector3(0.05f, 0.05f, 0.12f) * s, black, default(Vector3), g);
                for (int k = -1; k <= 1; k += 2)
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(k * 0.08f, 0.9f, 0.24f) * s, Vector3.one * 0.05f * s, eye, default(Vector3), g));
            }
            else
            {
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.62f, 0f) * s, new Vector3(0.62f, 1.25f, 0.56f) * s, black, default(Vector3), g);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.58f, 0.1f) * s, new Vector3(0.5f, 1.05f, 0.42f) * s, white, default(Vector3), g);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.32f, 0.02f) * s, new Vector3(0.42f, 0.4f, 0.42f) * s, black, default(Vector3), g);
                for (int k = -1; k <= 1; k += 2)
                {
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(k * 0.17f, 1.24f, 0.06f) * s, new Vector3(0.08f, 0.16f, 0.1f) * s, PatchworkAssets.Felt(new Color(1f, 0.72f, 0.25f)), default(Vector3), g));
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(k * 0.11f, 1.37f, 0.17f) * s, Vector3.one * 0.05f * s, eye, default(Vector3), g));
                    kit.Deco(PrimitiveType.Cube, new Vector3(k * 0.32f, 0.75f, 0f) * s, new Vector3(0.06f, 0.62f, 0.2f) * s, black, new Vector3(0f, 0f, k * 12f), g);
                    kit.Deco(PrimitiveType.Cube, new Vector3(k * 0.12f, 0.03f, 0.12f) * s, new Vector3(0.14f, 0.05f, 0.22f) * s, PatchworkAssets.Felt(new Color(0.25f, 0.22f, 0.22f)), default(Vector3), g);
                }
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.3f, 0.27f) * s, new Vector3(0.07f, 0.07f, 0.24f) * s, black, new Vector3(8f, 0f, 0f), g);
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.27f, 0.25f) * s, new Vector3(0.075f, 0.03f, 0.18f) * s, PatchworkAssets.Felt(new Color(0.95f, 0.5f, 0.2f)), new Vector3(8f, 0f, 0f), g));
            }
            var b = g.gameObject.AddComponent<SeaBob>();
            b.Height = 0.015f; b.Rock = 4f; b.Speed = 1.2f + Mathf.Repeat(feet.x * 0.37f + feet.z * 0.19f, 1f);
            b.Phase = feet.x + feet.z;
            return g;
        }

        // ================================================================ The Mangrove

        static void Mangrove(LevelKit kit)
        {
            var T = MangroveTank;
            float x0 = T[0], x1 = T[1], z0 = T[2], z1 = T[3], bed = T[4];
            float cx = (x0 + x1) * 0.5f, cz = (z0 + z1) * 0.5f;
            var rng = new System.Random(8404);
            System.Func<float, float, float> R = (a, b) => a + (float)rng.NextDouble() * (b - a);
            KwBox(kit, x0, x1, bed - 1f, bed, z0, z1, PatchworkAssets.Felt(new Color(0.42f, 0.36f, 0.26f)), "MangroveMud");
            Pool(kit, x0, x1, z0, z1, WaterTop, bed, new Color(0.35f, 0.55f, 0.35f), "Mangrove");
            SpotLight(kit, new Vector3(cx, 7f, cz), new Vector3(cx, bed, cz), new Color(0.85f, 1f, 0.75f), 24f, 105f, 100f, false, true);
            PointLight(kit, new Vector3(cx, bed + 3f, cz), new Color(0.5f, 0.85f, 0.5f), 18f, 1.1f);
            PointLight(kit, new Vector3(cx, 4f, cz), new Color(1f, 0.85f, 0.55f), 22f, 1f);
            for (int i = 0; i < 5; i++)
            {
                Vector3 topP = new Vector3(R(x0 + 3f, x1 - 3f), WaterTop - 0.1f, R(z0 + 4f, z1 - 4f));
                LightBeam.Make(kit.Root, topP, topP + new Vector3(0.8f, bed + 0.5f - WaterTop, 0.5f), R(1.2f, 2f), new Color(0.85f, 1f, 0.7f), 0.35f);
            }

            // ----- The mudbanks -----
            var mud = PatchworkAssets.Felt(new Color(0.4f, 0.32f, 0.22f));
            KwBox(kit, x0, -50f, -1.2f, 0.3f, 166f, z1, mud, "Mudbank");
            KwBox(kit, -42f, x1, -1.2f, 0.3f, z0, 141f, mud, "Mudbank");
            var lumps = new MeshBatch();
            for (int i = 0; i < 12; i++) lumps.Ball(new Vector3(R(x0 + 0.5f, -50.5f), 0.32f, R(166.5f, z1 - 0.5f)), new Vector3(R(1f, 2.2f), R(0.2f, 0.4f), R(1f, 2.2f)), 3, 7);
            for (int i = 0; i < 8; i++) lumps.Ball(new Vector3(R(-41.5f, x1 - 0.5f), 0.32f, R(z0 + 0.5f, 140.5f)), new Vector3(R(1f, 2f), R(0.2f, 0.4f), R(1f, 2f)), 3, 7);
            lumps.Build(kit.Root, "MudLumps", PatchworkAssets.Felt(new Color(0.36f, 0.28f, 0.2f)));

            // ----- The trees, standing up on their roots -----
            var bark = PatchworkAssets.Wood(new Color(0.42f, 0.32f, 0.24f));
            var leaves = new MeshBatch[3];
            for (int i = 0; i < 3; i++) leaves[i] = new MeshBatch();
            var oysters = new MeshBatch();
            foreach (var tr in new[] { new Vector4(-53.5f, 0.3f, 170.5f, 7f), new Vector4(-44f, bed, 163f, 6.5f), new Vector4(-39f, 0.3f, 137.5f, 6.8f), new Vector4(-51f, bed, 152.5f, 5.6f) })
            {
                Vector3 foot = new Vector3(tr.x, tr.y, tr.z);
                float topY = tr.w;
                float trunkFoot = Mathf.Max(foot.y, 1.2f);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(foot.x, (trunkFoot + topY) * 0.5f, foot.z), new Vector3(0.8f, (topY - trunkFoot) * 0.5f, 0.8f), bark);
                TownKit.Solid(kit, new Vector3(foot.x, (foot.y + topY) * 0.5f, foot.z), new Vector3(0.8f, topY - foot.y, 0.8f), 0f);
                // Prop roots arching out and down from the trunk into the mud.
                int roots = 11;
                for (int r = 0; r < roots; r++)
                {
                    float a = r / (float)roots * Mathf.PI * 2f + R(-0.2f, 0.2f);
                    float reach = R(2.6f, 4.2f);
                    Vector3 from = new Vector3(foot.x, trunkFoot + R(0.2f, 1.6f), foot.z);
                    Vector3 to = new Vector3(foot.x + Mathf.Cos(a) * reach, foot.y < -1f ? bed : 0.3f, foot.z + Mathf.Sin(a) * reach);
                    if (foot.y < -1f && to.y < bed + 0.1f) to.y = bed;
                    // (Roots on the banks stop at the bank's edge; past it they go into the water.)
                    bool overWater = !(to.x < -50f && to.z > 166f) && !(to.x > -42f && to.z < 141f);
                    if (overWater) to.y = bed;
                    Vector3 mid = Vector3.Lerp(from, to, 0.45f) + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 0.6f + Vector3.up * 0.8f;
                    Beam(kit, from, mid, 0.16f, bark);
                    Beam(kit, mid, to, 0.14f, bark);
                    if (overWater && rng.NextDouble() < 0.7)
                        for (int o = 0; o < 3; o++) oysters.Ball(Vector3.Lerp(mid, to, R(0.4f, 0.9f)) + new Vector3(R(-0.1f, 0.1f), 0f, R(-0.1f, 0.1f)), new Vector3(0.2f, 0.12f, 0.24f), 2, 5);
                }
                // A crown of leaves.
                for (int k = 0; k < 16; k++)
                {
                    float a = R(0f, Mathf.PI * 2f), d = R(0f, 3.2f);
                    leaves[k % 3].Ball(new Vector3(foot.x + Mathf.Cos(a) * d, topY + R(-0.6f, 0.4f) - d * 0.15f, foot.z + Mathf.Sin(a) * d), new Vector3(R(1.4f, 2.4f), R(0.9f, 1.4f), R(1.4f, 2.4f)), 3, 7);
                }
                for (int k = 0; k < 6; k++)
                {
                    float a = R(0f, Mathf.PI * 2f);
                    Beam(kit, new Vector3(foot.x, topY - 1f, foot.z), new Vector3(foot.x + Mathf.Cos(a) * 2.6f, topY - 0.2f, foot.z + Mathf.Sin(a) * 2.6f), 0.12f, bark);
                }
            }
            Color[] leafCols = { new Color(0.25f, 0.5f, 0.28f), new Color(0.32f, 0.58f, 0.3f), new Color(0.22f, 0.44f, 0.32f) };
            for (int i = 0; i < 3; i++) leaves[i].Build(kit.Root, "MangroveLeaves", PatchworkAssets.Yarn(leafCols[i]));
            oysters.Build(kit.Root, "Oysters", PatchworkAssets.Felt(new Color(0.55f, 0.55f, 0.52f)));

            // ----- A boardwalk out from the middle walk to a little platform among the trees -----
            var boards = PatchworkAssets.Planks(new Color(0.6f, 0.46f, 0.32f));
            KwBox(kit, x0, -48f, 0.1f, 0.3f, 145.2f, 146.8f, boards, "Boardwalk");
            KwBox(kit, -48f, -44.5f, 0.1f, 0.3f, 143.5f, 148.5f, boards, "Boardwalk");
            for (float x = x0 + 1f; x < -44.5f; x += 2f)
                foreach (float z in new[] { 145.3f, 146.7f })
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(x, (bed + 0.1f) * 0.5f, z), new Vector3(0.18f, (0.1f - bed) * 0.5f, 0.18f), boards);
            Balustrade(kit, new Vector3(-44.7f, 0.3f, 143.7f), new Vector3(-44.7f, 0.3f, 148.3f), 0.9f, boards);
            Plaque(kit, new Vector3(-45.6f, 0.3f, 147.6f), 200f, "Mangroves",
                "Mangroves are trees that grow in the salty water where the land meets the sea, standing up on their roots so the tide can come and go underneath.",
                "Down among the roots is a nursery: young fish, baby sharks and little crabs hide there, safe from the big hungry things outside, until they're big enough for the open sea.");

            // ----- Mudskippers and fiddler crabs on the banks -----
            for (int i = 0; i < 5; i++)
            {
                Vector3 p = new Vector3(R(x0 + 0.8f, -51f), 0.36f, 166.6f + R(0f, 1.2f));
                var g = kit.Group("Mudskipper", p);
                g.rotation = Rot(R(150f, 210f));
                var m = PatchworkAssets.Felt(new Color(0.55f, 0.5f, 0.38f));
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.12f, 0f), new Vector3(0.22f, 0.22f, 0.6f), m, default(Vector3), g);
                for (int s = -1; s <= 1; s += 2)
                {
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.06f, 0.26f, 0.18f), Vector3.one * 0.1f, PatchworkAssets.Glow(new Color(0.9f, 0.95f, 1f), 0.3f), default(Vector3), g));
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.07f, 0.28f, 0.21f), Vector3.one * 0.05f, PatchworkAssets.Plain(new Color(0.03f, 0.03f, 0.03f), 0.9f), default(Vector3), g));
                    kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.13f, 0.04f, 0.08f), new Vector3(0.12f, 0.03f, 0.1f), m, new Vector3(0f, s * 30f, 0f), g);
                }
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.28f, -0.05f), new Vector3(0.02f, 0.14f, 0.22f), PatchworkAssets.Felt(new Color(0.4f, 0.55f, 0.9f)), default(Vector3), g));
                var b = g.gameObject.AddComponent<SeaBob>();
                b.Height = 0.08f; b.Rock = 6f; b.Speed = 2.6f + i * 0.5f; b.Phase = i * 1.7f;
            }
            for (int i = 0; i < 6; i++)
            {
                Vector3 p = new Vector3(R(-41.5f, x1 - 0.6f), 0.4f, R(z0 + 0.6f, 140.4f));
                var g = kit.Group("FiddlerCrab", p);
                g.rotation = Rot(R(0f, 360f));
                var shell = PatchworkAssets.Felt(new Color(0.95f, 0.55f, 0.25f));
                kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.36f, 0.14f, 0.26f), shell, default(Vector3), g);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0.24f, 0.06f, 0.14f), new Vector3(0.3f, 0.14f, 0.16f), PatchworkAssets.Felt(new Color(1f, 0.85f, 0.55f)), new Vector3(0f, -30f, 20f), g);
                kit.Deco(PrimitiveType.Sphere, new Vector3(-0.18f, 0.02f, 0.12f), new Vector3(0.08f, 0.05f, 0.06f), shell, default(Vector3), g);
                for (int s = -1; s <= 1; s += 2)
                {
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.06f, 0.12f, 0.1f), new Vector3(0.02f, 0.1f, 0.02f), shell, default(Vector3), g));
                    for (int lg = 0; lg < 3; lg++)
                        NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.2f, -0.05f, -0.06f + lg * 0.06f), new Vector3(0.16f, 0.02f, 0.02f), shell, new Vector3(0f, 0f, s * -30f), g));
                }
                var b = g.gameObject.AddComponent<SeaBob>();
                b.Height = 0.02f; b.Rock = 8f; b.Speed = 3f + i * 0.4f;
            }

            // ----- Under the water: baby sharks, upside-down jellies, snappers, archerfish, a horseshoe crab -----
            for (int i = 0; i < 3; i++)
                Swim(kit, Swimmer.Shark, new Vector3(cx + R(-3f, 3f), bed + R(1.5f, 3.5f), cz + R(-6f, 6f)), R(4f, 7f), R(8f, 13f), (i % 2 == 0 ? 1f : -1f) * R(10f, 16f), 0.5f, new Color(0.55f, 0.55f, 0.5f), new Color(0.95f, 0.95f, 0.9f), i * 120f);
            Shoal(kit, new Vector3(cx, bed + 2.6f, cz + 4f), 5f, 9f, 10f, 22, 0.32f, new Color(0.9f, 0.75f, 0.55f), 8411);
            for (int i = 0; i < 3; i++)
            {
                var af = Swim(kit, Swimmer.Fish, new Vector3(cx + R(-4f, 4f), WaterTop - 0.8f, cz + R(-8f, 8f)), R(2f, 4f), R(3f, 6f), (i % 2 == 0 ? 1f : -1f) * 18f, 0.45f, new Color(0.85f, 0.9f, 0.85f), new Color(0.15f, 0.15f, 0.15f), i * 90f);
                af.Bob = 0.1f;
            }
            var bells = new MeshBatch();
            var arms = new MeshBatch();
            for (int i = 0; i < 12; i++)
            {
                Vector3 p = new Vector3(R(x0 + 2f, x1 - 2f), bed + 0.08f, R(142f, 165f));
                bells.Ball(p, new Vector3(0.7f, 0.12f, 0.7f), 3, 8);
                for (int k = 0; k < 8; k++)
                {
                    float a = k / 8f * Mathf.PI * 2f;
                    arms.Ball(p + new Vector3(Mathf.Cos(a) * 0.15f, 0.16f + (k % 2) * 0.05f, Mathf.Sin(a) * 0.15f), new Vector3(0.14f, 0.18f, 0.14f), 2, 5);
                }
            }
            bells.Build(kit.Root, "UpsideDownJellies", PatchworkAssets.Felt(new Color(0.75f, 0.68f, 0.5f)));
            Twinkling(arms, kit.Root, "JellyArms", new Color(0.45f, 0.8f, 0.75f), 0.4f, 1f, 0.6f, 0f);
            {
                Vector3 hc = new Vector3(-41.5f, bed + 0.12f, 150f);
                var crab = kit.Group("HorseshoeCrab", hc);
                var model = new GameObject("Shell").transform;
                model.SetParent(crab, false);
                var shell = PatchworkAssets.Felt(new Color(0.45f, 0.36f, 0.25f));
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.08f, 0.2f), new Vector3(0.9f, 0.3f, 0.8f), shell, default(Vector3), model);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.06f, -0.35f), new Vector3(0.6f, 0.22f, 0.5f), shell, default(Vector3), model);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.05f, -0.9f), new Vector3(0.05f, 0.05f, 0.8f), shell, new Vector3(-6f, 0f, 0f), model);
                var f = crab.gameObject.AddComponent<FishSwim>();
                f.Center = hc; f.RadiusX = 2.5f; f.RadiusZ = 4f; f.Speed = 6f; f.Bob = 0f; f.Wiggle = 2f; f.WiggleSpeed = 1f; f.Bank = 0f;
            }
            Plaque(kit, new Vector3(-54f, bed, 150f), 90f, "The Horseshoe Crab",
                "Horseshoe crabs were crawling about on the sea floor long before the first dinosaur, and they've hardly changed since. They aren't really crabs at all: their nearest relatives are spiders and scorpions.",
                "Their blood is blue.");
            Plaque(kit, new Vector3(-38.6f, 0.3f, 139.6f), 300f, "Archerfish",
                "Archerfish hunt insects sitting on the leaves above the water. They spit a jet of water hard enough to knock their dinner off its leaf, then catch it as it lands.",
                "They even allow for the way the water bends the light, which makes things look as if they're somewhere they aren't.");
            Motes(kit, new Vector3(cx, (bed + WaterTop) * 0.5f, cz), new Vector3(x1 - x0 - 1f, -WaterTop - bed - 0.5f, z1 - z0 - 1f), 60, new Color(0.8f, 0.9f, 0.6f), 1f, 0.06f, new Vector3(0.03f, 0.02f, 0.04f));
            Trail(kit, 10, new Vector3(-54f, bed + 1.5f, 141f), new Vector3(-51f, bed + 1.6f, 144f), new Vector3(-48f, bed + 1.7f, 148f), new Vector3(-46f, bed + 1.8f, 153f), new Vector3(-46f, bed + 1.8f, 158f));

            // ----- The hollow under the far bank, behind a curtain of roots -----
            var curtain = new MeshBatch();
            for (float x = x0 + 0.4f; x < -50.3f; x += 0.35f)
            {
                float len = R(3f, 5.4f);
                curtain.Box(new Vector3(x, -1.2f - len * 0.5f, 166f + R(-0.2f, 0.2f)), new Vector3(0.08f, len, 0.08f), Quaternion.Euler(R(-6f, 6f), 0f, R(-6f, 6f)));
            }
            curtain.Build(kit.Root, "RootCurtain", bark);
            var tinG = kit.Group("KeepersTin", new Vector3(-55.6f, bed + 0.2f, 173.6f));
            tinG.rotation = Rot(25f);
            kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(0.7f, 0.3f, 0.5f), PatchworkAssets.Plain(new Color(0.7f, 0.2f, 0.25f), 0.6f), default(Vector3), tinG);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.16f, 0f), new Vector3(0.72f, 0.03f, 0.52f), Brass, default(Vector3), tinG));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.18f, 0f), new Vector3(0.3f, 0.01f, 0.2f), PatchworkAssets.Felt(CreamCol), default(Vector3), tinG));
            var glowWorms = new MeshBatch();
            for (int i = 0; i < 24; i++) glowWorms.Ball(new Vector3(R(x0 + 0.3f, -50.5f), -1.25f, R(166.4f, z1 - 0.3f)), Vector3.one * R(0.05f, 0.1f), 2, 4);
            Twinkling(glowWorms, kit.Root, "HollowGlow", new Color(0.7f, 1f, 0.6f), 0.3f, 2.2f, 1.2f, 0.4f);
            SecretMarble.Place(kit, new Vector3(-54.4f, bed + 1.6f, 172f), MangroveMarble, new Color(0.6f, 1f, 0.55f));
            PointLight(kit, new Vector3(-54.4f, bed + 2.4f, 171.5f), new Color(0.7f, 1f, 0.6f), 6f, 1.2f);
            Mark("Secret_MangroveHollow", new Vector3(-54.5f, bed + 1f, 172f));
            Mark("EnemyLair_Mangrove", new Vector3(cx, bed + 2f, cz));
        }

        // ================================================================ The pipes between the tanks

        static void KwPipes(LevelKit kit)
        {
            // {x0, x1, y0, y1, z0, z1}: Coral to Frozen, Lantern to Mangrove, Frozen to Mangrove.
            float[][] pipes =
            {
                new[] { -76.5f, -73.5f, -7.5f, -4.5f, 126f, 134f },
                new[] { -48f, -45f, -7f, -4f, 126f, 134f },
                new[] { -63f, -57f, -6.5f, -3.5f, 153.5f, 156.5f },
            };
            string[][] labels = { new[] { "FROZEN SEA", "CORAL NURSERY" }, new[] { "MANGROVE", "LANTERN DEEP" }, new[] { "MANGROVE", "FROZEN SEA" } };
            var brass = Brass;
            var lining = PatchworkAssets.Plain(new Color(0.42f, 0.3f, 0.2f), 0.55f);
            var label = PatchworkAssets.Glow(new Color(1f, 0.82f, 0.45f), 0.7f);
            for (int i = 0; i < pipes.Length; i++)
            {
                var p = pipes[i];
                bool alongZ = p[5] - p[4] > p[1] - p[0] - 0.01f && i < 2;
                Vector3 c = new Vector3((p[0] + p[1]) * 0.5f, (p[2] + p[3]) * 0.5f, (p[4] + p[5]) * 0.5f);
                Vector3 size = new Vector3(p[1] - p[0], p[3] - p[2], p[5] - p[4]);
                var water = kit.Volume<WaterBody>(c, size, "PipeWater");
                water.SurfaceY = WaterTop;
                water.Divable = true;
                // The copper lining: a thin skin over each inner face, and brass rings every so often.
                float len = alongZ ? size.z : size.x;
                float w = alongZ ? size.x : size.z, h = size.y;
                var ribs = new MeshBatch();
                var skin = new MeshBatch();
                Vector3 along = alongZ ? Vector3.forward : Vector3.right;
                Vector3 across = alongZ ? Vector3.right : Vector3.forward;
                skin.Box(c + Vector3.up * (h * 0.5f - 0.03f), (alongZ ? new Vector3(w, 0.04f, len) : new Vector3(len, 0.04f, w)), Quaternion.identity);
                skin.Box(c + Vector3.down * (h * 0.5f - 0.03f), (alongZ ? new Vector3(w, 0.04f, len) : new Vector3(len, 0.04f, w)), Quaternion.identity);
                for (int s = -1; s <= 1; s += 2)
                    skin.Box(c + across * s * (w * 0.5f - 0.03f), (alongZ ? new Vector3(0.04f, h, len) : new Vector3(len, h, 0.04f)), Quaternion.identity);
                for (float u = -len * 0.5f + 0.8f; u < len * 0.5f - 0.4f; u += 1.6f)
                {
                    Vector3 rc = c + along * u;
                    ribs.Box(rc + Vector3.up * (h * 0.5f - 0.1f), (alongZ ? new Vector3(w, 0.12f, 0.2f) : new Vector3(0.2f, 0.12f, w)), Quaternion.identity);
                    ribs.Box(rc + Vector3.down * (h * 0.5f - 0.1f), (alongZ ? new Vector3(w, 0.12f, 0.2f) : new Vector3(0.2f, 0.12f, w)), Quaternion.identity);
                    for (int s = -1; s <= 1; s += 2)
                        ribs.Box(rc + across * s * (w * 0.5f - 0.1f), (alongZ ? new Vector3(0.12f, h, 0.2f) : new Vector3(0.2f, h, 0.12f)), Quaternion.identity);
                }
                skin.Build(kit.Root, "PipeLining", lining);
                ribs.Build(kit.Root, "PipeRings", brass);
                // A strip of glow along the top, so it's never pitch dark in there.
                var strip = new MeshBatch();
                strip.Box(c + Vector3.up * (h * 0.5f - 0.07f), (alongZ ? new Vector3(0.12f, 0.02f, len - 0.4f) : new Vector3(len - 0.4f, 0.02f, 0.12f)), Quaternion.identity);
                Twinkling(strip, kit.Root, "PipeGlow", new Color(0.6f, 0.95f, 1f), 0.6f, 1.6f, 0.8f, i);
                PointLight(kit, c, new Color(0.55f, 0.85f, 1f), 6f, 0.9f);
                // A heavy flange round each mouth, and the name of where it goes above it.
                for (int e = -1; e <= 1; e += 2)
                {
                    Vector3 mouth = c + along * e * (len * 0.5f + 0.06f);
                    var fl = new MeshBatch();
                    fl.Box(mouth + Vector3.up * (h * 0.5f + 0.2f), (alongZ ? new Vector3(w + 0.8f, 0.4f, 0.12f) : new Vector3(0.12f, 0.4f, w + 0.8f)), Quaternion.identity);
                    fl.Box(mouth + Vector3.down * (h * 0.5f + 0.2f), (alongZ ? new Vector3(w + 0.8f, 0.4f, 0.12f) : new Vector3(0.12f, 0.4f, w + 0.8f)), Quaternion.identity);
                    for (int s = -1; s <= 1; s += 2)
                        fl.Box(mouth + across * s * (w * 0.5f + 0.2f), (alongZ ? new Vector3(0.4f, h, 0.12f) : new Vector3(0.12f, h, 0.4f)), Quaternion.identity);
                    for (int b = 0; b < 8; b++)
                    {
                        float a = b / 8f * Mathf.PI * 2f;
                        fl.Ball(mouth + along * e * 0.07f + Vector3.up * Mathf.Sin(a) * (h * 0.5f + 0.2f) + across * Mathf.Cos(a) * (w * 0.5f + 0.2f), Vector3.one * 0.12f, 2, 5);
                    }
                    fl.Build(kit.Root, "PipeFlange", brass);
                    var txt = new MeshBatch();
                    string name = labels[i][e < 0 ? 0 : 1];
                    Letters(txt, name, mouth + along * e * 0.04f + Vector3.up * (h * 0.5f + 0.75f), along * e, Mathf.Min(0.07f, (w + 2f) / (name.Length * 6f)), 0.02f);
                    txt.Build(kit.Root, "PipeLabel", label);
                }
                var pts = new Vector3[5];
                for (int k = 0; k < 5; k++) pts[k] = c + along * (-len * 0.5f + len * (k + 0.5f) / 5f);
                Trail(kit, 10, pts);
            }
            Mark("Pipe_CoralToFrozen", new Vector3(-75f, -6f, 130f));
            Mark("Pipe_LanternToMangrove", new Vector3(-46.5f, -5.5f, 130f));
            Mark("Pipe_FrozenToMangrove", new Vector3(-60f, -5f, 155f));
        }

        // ================================================================ Moods

        static void KwMoods(List<Level2Mood.Zone> Z)
        {
            Z.Add(new Level2Mood.Zone
            {
                Name = "The Undercroft", Line = "The Undercroft. A corridor with a sea on either side of it.",
                Box = Box(-63f, -57f, -9.5f, -2.5f, 98f, 126f),
                AmbientSky = new Color(0.4f, 0.42f, 0.5f), AmbientSide = new Color(0.32f, 0.32f, 0.4f), AmbientGround = new Color(0.18f, 0.18f, 0.22f),
                Fog = new Color(0.06f, 0.12f, 0.18f), Backdrop = new Color(0.02f, 0.05f, 0.1f), SunColor = new Color(0.8f, 0.85f, 1f), FogStart = 20f, FogEnd = 90f, Sun = 0.3f, Depth = 0.4f
            });
            Z.Add(new Level2Mood.Zone
            {
                Name = "The Coral Nursery", Line = "The Coral Nursery. The keepers grow new reefs in here, one little piece at a time.",
                Box = Box(-86f, -63f, -10f, -0.3f, 98f, 126f),
                AmbientSky = new Color(0.4f, 0.75f, 0.8f), AmbientSide = new Color(0.3f, 0.6f, 0.65f), AmbientGround = new Color(0.4f, 0.4f, 0.3f),
                Fog = new Color(0.12f, 0.45f, 0.55f), Backdrop = new Color(0.05f, 0.3f, 0.4f), SunColor = new Color(0.8f, 1f, 1f), FogStart = 4f, FogEnd = 60f, Sun = 0.3f, Depth = 0.6f,
                UnderFog = new Color(0.12f, 0.48f, 0.58f), UnderFogEnd = 46f
            });
            Z.Add(new Level2Mood.Zone
            {
                Name = "The Lantern Deep", Line = "The Lantern Deep. No sun has ever come down here. Everything that shines, shines all by itself.",
                Box = Box(-57f, -36f, -26f, -0.3f, 98f, 126f),
                AmbientSky = new Color(0.06f, 0.08f, 0.2f), AmbientSide = new Color(0.04f, 0.05f, 0.14f), AmbientGround = new Color(0.02f, 0.02f, 0.06f),
                Fog = new Color(0.01f, 0.02f, 0.07f), Backdrop = new Color(0.005f, 0.01f, 0.04f), SunColor = new Color(0.3f, 0.4f, 0.8f), FogStart = 2f, FogEnd = 30f, Sun = 0.05f, Depth = 1f,
                UnderFog = new Color(0.01f, 0.025f, 0.08f), UnderFogEnd = 24f
            });
            Z.Add(new Level2Mood.Zone
            {
                Name = "The Frozen Sea", Line = "The Frozen Sea. The coldest water in the museum, and the warmest-hearted penguins.",
                Box = Box(-86f, -63f, -13f, -0.3f, 134f, 175f),
                AmbientSky = new Color(0.6f, 0.72f, 0.85f), AmbientSide = new Color(0.45f, 0.55f, 0.7f), AmbientGround = new Color(0.3f, 0.35f, 0.45f),
                Fog = new Color(0.4f, 0.55f, 0.7f), Backdrop = new Color(0.2f, 0.3f, 0.45f), SunColor = new Color(0.85f, 0.92f, 1f), FogStart = 4f, FogEnd = 50f, Sun = 0.35f, Depth = 0.7f,
                UnderFog = new Color(0.36f, 0.56f, 0.72f), UnderFogEnd = 40f
            });
            Z.Add(new Level2Mood.Zone
            {
                Name = "The Mangrove", Line = "The Mangrove. Half forest, half sea, and a nursery for nearly everybody.",
                Box = Box(-57f, -36f, -8f, -0.3f, 134f, 175f),
                AmbientSky = new Color(0.45f, 0.6f, 0.4f), AmbientSide = new Color(0.32f, 0.45f, 0.3f), AmbientGround = new Color(0.22f, 0.2f, 0.14f),
                Fog = new Color(0.15f, 0.28f, 0.16f), Backdrop = new Color(0.06f, 0.14f, 0.08f), SunColor = new Color(0.9f, 1f, 0.75f), FogStart = 3f, FogEnd = 30f, Sun = 0.3f, Depth = 0.6f,
                UnderFog = new Color(0.14f, 0.27f, 0.15f), UnderFogEnd = 20f
            });
            Z.Add(new Level2Mood.Zone
            {
                Name = "The Keepers' Room", Line = "The Keepers' Room. Somebody's left the kettle on.",
                Box = Box(-90f, -32f, -1f, 9f, KwZ0, KwHall),
                AmbientSky = new Color(0.62f, 0.52f, 0.46f), AmbientSide = new Color(0.52f, 0.42f, 0.36f), AmbientGround = new Color(0.3f, 0.24f, 0.2f),
                Fog = new Color(0.2f, 0.15f, 0.12f), Backdrop = new Color(0.05f, 0.04f, 0.04f), SunColor = new Color(1f, 0.85f, 0.65f), FogStart = 40f, FogEnd = 160f, Sun = 0.5f
            });
            Z.Add(new Level2Mood.Zone
            {
                Name = "Behind the Glass", Line = "Behind the Glass. Every aquarium has a back door, and this is the museum's.",
                Box = Box(-90f, -32f, -27f, KwTop + 2f, KwHall, KwZ1 + 2f),
                AmbientSky = new Color(0.36f, 0.46f, 0.55f), AmbientSide = new Color(0.26f, 0.34f, 0.42f), AmbientGround = new Color(0.14f, 0.18f, 0.22f),
                Fog = new Color(0.06f, 0.12f, 0.16f), Backdrop = new Color(0.03f, 0.06f, 0.1f), SunColor = new Color(0.7f, 0.85f, 1f), FogStart = 30f, FogEnd = 150f, Sun = 0.35f, Depth = 0.3f
            });
        }
    }

    // A moray eel in its crevice: it slides out, gapes for a while (that's how it breathes),
    // then slides back in again.
    public class MorayPeek : MonoBehaviour
    {
        public Transform Head, Jaw;
        public float Out = 0.9f, Cycle = 7f;

        void Update()
        {
            if (Head == null) return;
            float t = Mathf.Repeat(Time.time / Cycle + transform.position.z * 0.1f, 1f);
            float k = t < 0.2f ? 0f : t < 0.35f ? Mathf.SmoothStep(0f, 1f, (t - 0.2f) / 0.15f) : t < 0.8f ? 1f : 1f - Mathf.SmoothStep(0f, 1f, (t - 0.8f) / 0.2f);
            Head.localPosition = new Vector3(Mathf.Sin(Time.time * 0.8f) * 0.05f * k, Mathf.Sin(Time.time * 1.1f) * 0.04f * k, -Out + k * Out);
            Head.localRotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * 0.6f) * 12f * k, 0f);
            if (Jaw != null) Jaw.localRotation = Quaternion.Euler((Mathf.Sin(Time.time * 2.2f) * 0.5f + 0.5f) * 22f * k, 0f, 0f);
        }
    }
}

using UnityEngine;

namespace Patchwork
{
    // Six more things in each part of Behind the Glass (see Level2Rooms for how they're fitted in):
    //   The Keepers' Room   - the night keeper's camp bed, a sick pufferfish resting in quarantine,
    //                         the tool board (one tool missing), a barometer, the towel horse,
    //                         and the wireless on its shelf
    //   The Tank Hall       - the board of tank temperatures, children's thank-you drawings, the
    //                         keepers' skiff on the wall, a crate of glass, two pigeons, nets drying
    //   The Undercroft      - a speaking tube, binoculars on a hook, a shelf of field guides, a
    //                         leak and its bucket, the fire bucket, and a sign about the glass
    //   The Coral Nursery   - parrotfish, Christmas tree worms, the corals spawning, a brain coral,
    //                         a pufferfish, and a keeper's slate
    //   The Lantern Deep    - a gulper eel, a barreleye, hatchetfish, a chain of salps, brittle
    //                         stars, and the keepers' drop camera
    //   The Frozen Sea      - icefish, sea butterflies, an Arctic tern, an ice drill, the algae
    //                         under the ice, and Pippa's snowman
    //   The Mangrove        - a kingfisher on a snag, floating seedlings, mud lobster mounds, a
    //                         spider's web, tree-climbing crabs, and a crocodile being a log
    public static partial class Level2Builder
    {
        // ================================================================ The Keepers' Room

        static void RoomsKeepersRoom(LevelKit kit)
        {
            KrCampBed(kit);
            KrQuarantine(kit);
            KrToolBoard(kit);
            KrBarometer(kit);
            KrTowelHorse(kit);
            KrWireless(kit);
        }

        static void KrCampBed(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the night keeper's camp bed", new Vector3(-80f, 0f, 92.6f), new Vector3(0.95f, 1f, 2.2f), 90f, 8f, out f)) return;
            var g = RmGroup(kit, "CampBed", f, 90f);
            var b = new RmBits(kit, g);
            var frame = RmShine(new Color(0.35f, 0.4f, 0.3f), 0.5f);
            var canvas = RmFelt(new Color(0.55f, 0.58f, 0.45f));
            foreach (int s in new[] { -1, 1 })
            {
                b.Tube(frame, new Vector3(s * 0.38f, 0.42f, -1f), new Vector3(s * 0.38f, 0.42f, 1f), 0.025f, 6);
                foreach (float z in new[] { -0.85f, 0.85f })
                {
                    b.Tube(frame, new Vector3(s * 0.38f, 0.42f, z - 0.15f), new Vector3(s * 0.38f, 0f, z + 0.15f), 0.018f, 6);
                    b.Tube(frame, new Vector3(s * 0.38f, 0.42f, z + 0.15f), new Vector3(s * 0.38f, 0f, z - 0.15f), 0.018f, 6);
                }
            }
            b.Box(canvas, new Vector3(0f, 0.43f, 0f), new Vector3(0.74f, 0.03f, 1.98f));
            // A pillow, a blanket kicked half off, and the alarm clock on the floor by it.
            b.Ball(RmFelt(new Color(0.95f, 0.93f, 0.88f)), new Vector3(0f, 0.52f, -0.8f), new Vector3(0.55f, 0.16f, 0.34f), 3, 8);
            var blanket = PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, Teal, CreamCol);
            b.Box(blanket, new Vector3(0.05f, 0.47f, 0.25f), new Vector3(0.78f, 0.05f, 1.2f), new Vector3(0f, 4f, 0f));
            b.Ball(blanket, new Vector3(0.1f, 0.5f, -0.2f), new Vector3(0.7f, 0.14f, 0.4f), 3, 8);
            b.Box(blanket, new Vector3(0.46f, 0.25f, 0.4f), new Vector3(0.04f, 0.46f, 0.6f), new Vector3(0f, 0f, 8f));
            Vector3 clock = new Vector3(-0.6f, 0f, -0.75f);
            b.Cyl(RmShine(new Color(0.75f, 0.2f, 0.2f), 0.7f), clock + Vector3.up * 0.02f, 0.05f, 0.04f, 8);
            b.Prim(PrimitiveType.Cylinder, RmShine(new Color(0.75f, 0.2f, 0.2f), 0.7f), clock + Vector3.up * 0.14f, new Vector3(0.18f, 0.04f, 0.18f), new Vector3(90f, 90f, 0f));
            b.Prim(PrimitiveType.Cylinder, RmFelt(CreamCol), clock + new Vector3(-0.045f, 0.14f, 0f), new Vector3(0.15f, 0.005f, 0.15f), new Vector3(90f, 90f, 0f));
            foreach (int s in new[] { -1, 1 }) b.Ball(Brass, clock + new Vector3(0f, 0.25f, s * 0.06f), new Vector3(0.07f, 0.05f, 0.07f), 3, 6);
            // Slippers, and a torch.
            foreach (int s in new[] { -1, 1 })
                b.Ball(RmFelt(new Color(0.4f, 0.3f, 0.55f)), new Vector3(-0.62f, 0.04f, 0.1f + s * 0.12f), new Vector3(0.12f, 0.08f, 0.26f), 3, 6);
            b.Tube(RmShine(new Color(0.2f, 0.2f, 0.25f), 0.6f), new Vector3(-0.55f, 0.04f, 0.55f), new Vector3(-0.55f, 0.04f, 0.8f), 0.035f, 8);
            b.Done("CampBed", true);
            RmSolid(kit, g, new Vector3(0f, 0.25f, 0f), new Vector3(0.8f, 0.5f, 2f));
            Mark("Story_NightKeepersBed", f);
        }

        static void KrQuarantine(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the quarantine tub", new Vector3(-71f, 0f, 80.4f), new Vector3(1.3f, 1.6f, 1f), 0f, 7f, out f)) return;
            var g = RmGroup(kit, "QuarantineTub", f, 0f);
            var b = new RmBits(kit, g);
            var steel = RmShine(new Color(0.7f, 0.72f, 0.75f), 0.85f);
            foreach (int s in new[] { -1, 1 }) foreach (int t in new[] { -1, 1 }) b.Box(steel, new Vector3(s * 0.45f, 0.4f, t * 0.32f), new Vector3(0.04f, 0.8f, 0.04f));
            b.Box(steel, new Vector3(0f, 0.8f, 0f), new Vector3(1f, 0.04f, 0.74f));
            b.Box(steel, new Vector3(0f, 0.15f, 0f), new Vector3(0.9f, 0.03f, 0.64f));
            // The tub: water, gravel, an air stone, a thermometer clipped to the side.
            b.Cyl(RmShine(new Color(0.35f, 0.65f, 0.75f), 0.95f), new Vector3(0f, 0.83f, 0f), 0.34f, 0.5f, 18);
            b.Cyl(RmFelt(new Color(0.85f, 0.8f, 0.7f)), new Vector3(0f, 0.83f, 0f), 0.34f, 0.05f, 18);
            b.Ring(RmShine(new Color(0.2f, 0.5f, 0.6f), 0.9f), new Vector3(0f, 1.38f, 0f), new Vector3(90f, 0f, 0f), 0.37f, 0.03f, 22);
            b.Ball(RmFelt(new Color(0.4f, 0.4f, 0.45f)), new Vector3(0.15f, 0.9f, 0.1f), new Vector3(0.08f, 0.05f, 0.08f), 2, 5);
            b.Box(RmShine(Color.white, 0.9f), new Vector3(-0.36f, 1.2f, 0f), new Vector3(0.02f, 0.3f, 0.03f));
            b.Box(RmGlow(new Color(1f, 0.2f, 0.2f), 1f), new Vector3(-0.37f, 1.15f, 0f), new Vector3(0.01f, 0.16f, 0.012f));
            // Its chart on a clipboard, and a sign: SHH, RESTING.
            b.Box(PatchworkAssets.Planks(new Color(0.6f, 0.45f, 0.3f)), new Vector3(0.48f, 1.05f, 0.37f), new Vector3(0.26f, 0.34f, 0.02f), new Vector3(0f, 0f, -6f));
            b.Box(RmFelt(CreamCol), new Vector3(0.48f, 1.03f, 0.385f), new Vector3(0.22f, 0.26f, 0.01f), new Vector3(0f, 0f, -6f));
            for (int i = 0; i < 5; i++) b.Box(RmFelt(new Color(0.3f, 0.3f, 0.35f)), new Vector3(0.48f + Mathf.Sin(-6f * Mathf.Deg2Rad) * (0.1f - i * 0.045f), 1.13f - i * 0.045f, 0.392f), new Vector3(0.16f - (i % 2) * 0.05f, 0.012f, 0.005f), new Vector3(0f, 0f, -6f));
            b.Box(DarkWood, new Vector3(-0.3f, 1.55f, 0.32f), new Vector3(0.03f, 0.35f, 0.03f));
            b.Box(RmFelt(new Color(0.95f, 0.85f, 0.4f)), new Vector3(-0.3f, 1.72f, 0.33f), new Vector3(0.4f, 0.14f, 0.02f));
            Letters(b[RmFelt(new Color(0.3f, 0.2f, 0.1f))], "SHH RESTING", new Vector3(-0.3f, 1.72f, 0.342f), Vector3.forward, 0.0055f, 0.004f);
            b.Done("QuarantineTub", true);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.11f, 0f), new Vector3(0.7f, 0.28f, 0.7f), CaseGlass, default(Vector3), g));
            // The patient: a little pufferfish with a bandage round his middle, puffing now and then.
            var fish = kit.Group("SickPuffer", g.TransformPoint(new Vector3(0f, 1.12f, 0f)));
            fish.SetParent(g, true);
            fish.localRotation = Quaternion.Euler(0f, 30f, 0f);
            var body = kit.Group("Body", fish.position);
            body.SetParent(fish, true);
            body.localRotation = Quaternion.identity;
            var pb = new RmBits(kit, body);
            pb.Ball(RmFelt(new Color(0.85f, 0.75f, 0.45f)), Vector3.zero, new Vector3(0.14f, 0.12f, 0.18f), 4, 8);
            pb.Ball(RmFelt(Color.white), Vector3.zero, new Vector3(0.145f, 0.04f, 0.185f), 3, 8);
            for (int i = 0; i < 10; i++)
                pb.Box(RmFelt(new Color(0.6f, 0.5f, 0.3f)), new Vector3(Mathf.Cos(i * 0.63f) * 0.07f, Mathf.Sin(i * 1.3f) * 0.05f, Mathf.Sin(i * 0.63f) * 0.08f), new Vector3(0.008f, 0.008f, 0.03f), new Vector3(i * 30f, i * 63f, 0f));
            RmEyes(pb, new Vector3(0f, 0.025f, 0.07f), 0.07f, 0.025f);
            pb.Done("SickPuffer");
            var puff = fish.gameObject.AddComponent<Puffer>();
            puff.Body = body;
            puff.Every = 11f;
            puff.Swell = 1.5f;
            var bob = fish.gameObject.AddComponent<SeaBob>();
            bob.Height = 0.03f;
            bob.Rock = 6f;
            Bubbles(kit, g.TransformPoint(new Vector3(0.15f, 0.92f, 0.1f)), 0.42f, 4);
            RmSolid(kit, g, new Vector3(0f, 0.7f, 0f), new Vector3(1f, 1.4f, 0.74f));
        }

        static void KrToolBoard(LevelKit kit)
        {
            Vector3 at;
            if (!RmWall("the tool board", new Vector3(-49f, 2f, 94f), Vector3.back, new Vector3(2.2f, 1.3f, 0.3f), 8f, out at)) return;
            var g = RmGroup(kit, "ToolBoard", at, Quaternion.LookRotation(Vector3.back));
            var b = new RmBits(kit, g);
            b.Box(PatchworkAssets.Planks(new Color(0.75f, 0.62f, 0.45f)), new Vector3(0f, 0f, 0.02f), new Vector3(2.1f, 1.2f, 0.04f));
            for (int i = 0; i < 20; i++)
                for (int k = 0; k < 11; k++)
                    b.Box(RmFelt(new Color(0.45f, 0.35f, 0.25f)), new Vector3(-0.95f + i * 0.1f, -0.5f + k * 0.1f, 0.042f), new Vector3(0.012f, 0.012f, 0.005f));
            var outline = RmFelt(new Color(0.95f, 0.95f, 0.92f));
            var iron = RmShine(new Color(0.55f, 0.56f, 0.6f), 0.8f);
            var handle = RmFelt(new Color(0.8f, 0.25f, 0.2f));
            // A shape painted round each tool, so you can see where it goes back.
            System.Action<Vector3, Vector3, float, Material, bool> tool = (c, size, turn, m, there) =>
            {
                b.Box(outline, c + new Vector3(0f, 0f, 0.046f), size + new Vector3(0.04f, 0.04f, 0f), new Vector3(0f, 0f, turn));
                if (there) b.Box(m, c + new Vector3(0f, 0f, 0.06f), size, new Vector3(0f, 0f, turn));
            };
            tool(new Vector3(-0.8f, 0.1f, 0f), new Vector3(0.06f, 0.5f, 0.02f), 0f, iron, true);
            b.Box(iron, new Vector3(-0.8f, 0.38f, 0.06f), new Vector3(0.14f, 0.08f, 0.02f));
            tool(new Vector3(-0.55f, 0.05f, 0f), new Vector3(0.05f, 0.42f, 0.03f), 0f, PatchworkAssets.Planks(new Color(0.55f, 0.4f, 0.28f)), true);
            b.Box(iron, new Vector3(-0.55f, 0.3f, 0.07f), new Vector3(0.18f, 0.07f, 0.05f));
            tool(new Vector3(-0.3f, 0.05f, 0f), new Vector3(0.04f, 0.3f, 0.03f), 0f, iron, true);
            b.Box(handle, new Vector3(-0.3f, -0.15f, 0.07f), new Vector3(0.06f, 0.14f, 0.05f));
            tool(new Vector3(-0.05f, 0.1f, 0f), new Vector3(0.05f, 0.4f, 0.02f), 20f, iron, true);
            tool(new Vector3(0.05f, 0.1f, 0f), new Vector3(0.05f, 0.4f, 0.02f), -20f, iron, true);
            // (This one isn't there. Somebody's borrowed the big spanner again.)
            tool(new Vector3(0.38f, 0.05f, 0f), new Vector3(0.07f, 0.62f, 0.02f), 0f, iron, false);
            b.Ring(outline, new Vector3(0.38f, 0.42f, 0.047f), Vector3.zero, 0.08f, 0.03f, 10, -40f, 220f);
            tool(new Vector3(0.7f, 0.1f, 0f), new Vector3(0.2f, 0.2f, 0.05f), 0f, RmShine(new Color(0.95f, 0.8f, 0.2f), 0.6f), true);
            b.Disc(RmShine(new Color(0.2f, 0.2f, 0.22f), 0.5f), new Vector3(0.7f, 0.1f, 0.09f), 0.08f, 0.01f);
            // Hooks along the bottom, with a reel of line and a coil of rope.
            for (int i = 0; i < 4; i++) b.Box(iron, new Vector3(-0.75f + i * 0.5f, -0.4f, 0.08f), new Vector3(0.02f, 0.02f, 0.1f));
            b.Ring(PatchworkAssets.Yarn(CreamCol), new Vector3(-0.25f, -0.5f, 0.12f), Vector3.zero, 0.09f, 0.03f, 12);
            b.Disc(RmFelt(new Color(0.3f, 0.6f, 0.4f)), new Vector3(0.25f, -0.48f, 0.12f), 0.12f, 0.05f);
            b.Done("ToolBoard", true);
        }

        static void KrBarometer(LevelKit kit)
        {
            Vector3 at;
            if (!RmWall("the barometer", new Vector3(-68f, 2.3f, 94f), Vector3.back, new Vector3(0.55f, 1.2f, 0.15f), 7f, out at)) return;
            var g = RmGroup(kit, "Barometer", at, Quaternion.LookRotation(Vector3.back));
            var b = new RmBits(kit, g);
            var wood = PatchworkAssets.Planks(new Color(0.4f, 0.25f, 0.16f));
            // A banjo barometer: a dial below, a thermometer above, all in carved wood.
            b.Disc(wood, new Vector3(0f, -0.25f, 0.04f), 0.52f, 0.06f);
            b.Box(wood, new Vector3(0f, 0.25f, 0.03f), new Vector3(0.16f, 0.6f, 0.05f));
            b.Ball(wood, new Vector3(0f, 0.6f, 0.03f), new Vector3(0.2f, 0.12f, 0.05f), 3, 6);
            b.Disc(RmFelt(CreamCol), new Vector3(0f, -0.25f, 0.075f), 0.4f, 0.01f);
            b.Ring(Brass, new Vector3(0f, -0.25f, 0.08f), Vector3.zero, 0.21f, 0.02f, 20);
            for (int i = 0; i < 11; i++)
            {
                float a = (-60f + i * 12f) * Mathf.Deg2Rad;
                b.Box(RmFelt(new Color(0.2f, 0.15f, 0.12f)), new Vector3(Mathf.Sin(a) * 0.16f, -0.25f + Mathf.Cos(a) * 0.16f, 0.083f), new Vector3(0.006f, 0.03f, 0.003f), new Vector3(0f, 0f, -(-60f + i * 12f)));
            }
            var ink = RmFelt(new Color(0.25f, 0.2f, 0.18f));
            Letters(b[ink], "RAIN", new Vector3(0.1f, -0.33f, 0.083f), Vector3.forward, 0.006f, 0.003f);
            Letters(b[ink], "FAIR", new Vector3(-0.1f, -0.33f, 0.083f), Vector3.forward, 0.006f, 0.003f);
            b.Box(RmShine(Color.white, 0.9f), new Vector3(0f, 0.27f, 0.06f), new Vector3(0.025f, 0.46f, 0.02f));
            b.Box(RmGlow(new Color(0.9f, 0.15f, 0.15f), 0.8f), new Vector3(0f, 0.16f, 0.072f), new Vector3(0.012f, 0.25f, 0.01f));
            b.Done("Barometer", true);
            var needle = kit.Group("Needle", g.TransformPoint(new Vector3(0f, -0.25f, 0.09f)));
            needle.SetParent(g, true);
            needle.localRotation = Quaternion.identity;
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.08f, 0f), new Vector3(0.012f, 0.16f, 0.004f), RmShine(new Color(0.1f, 0.1f, 0.12f), 0.7f), default(Vector3), needle));
            needle.gameObject.AddComponent<GaugeWaver>();
        }

        static void KrTowelHorse(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the towel horse", new Vector3(-45f, 0f, 80.4f), new Vector3(1.7f, 1.4f, 0.7f), 0f, 7f, out f)) return;
            var g = RmGroup(kit, "TowelHorse", f, 0f);
            var b = new RmBits(kit, g);
            var wood = PatchworkAssets.Planks(new Color(0.75f, 0.62f, 0.45f));
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(wood, new Vector3(s * 0.75f, 0.6f, -0.12f), new Vector3(0.04f, 1.22f, 0.04f), new Vector3(-10f, 0f, 0f));
                b.Box(wood, new Vector3(s * 0.75f, 0.6f, 0.12f), new Vector3(0.04f, 1.22f, 0.04f), new Vector3(10f, 0f, 0f));
            }
            foreach (float y in new[] { 0.45f, 0.85f, 1.18f }) b.Tube(wood, new Vector3(-0.77f, y, 0f), new Vector3(0.77f, y, 0f), 0.018f, 6);
            // Towels hung over the rails, flippers pegged up, and a snorkel.
            Color[] towels = { new Color(0.95f, 0.5f, 0.45f), new Color(0.45f, 0.7f, 0.95f), new Color(0.95f, 0.85f, 0.4f) };
            for (int i = 0; i < 3; i++)
            {
                var t = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, towels[i], CreamCol);
                float x = -0.45f + i * 0.45f, y = i == 1 ? 1.18f : 0.85f;
                b.Box(t, new Vector3(x, y - 0.25f, 0.05f), new Vector3(0.38f, 0.5f, 0.015f), new Vector3(3f, 0f, 0f));
                b.Box(t, new Vector3(x, y - 0.15f, -0.05f), new Vector3(0.38f, 0.3f, 0.015f), new Vector3(-3f, 0f, 0f));
                b.Ball(t, new Vector3(x, y + 0.005f, 0f), new Vector3(0.38f, 0.04f, 0.1f), 2, 6);
            }
            var rubber = RmShine(new Color(0.15f, 0.45f, 0.55f), 0.5f);
            foreach (int s in new[] { -1, 1 })
                b.Box(rubber, new Vector3(0.5f + s * 0.12f, 0.2f, 0.03f), new Vector3(0.18f, 0.5f, 0.02f), new Vector3(0f, 0f, s * 6f));
            b.Tube(RmShine(new Color(1f, 0.85f, 0.2f), 0.6f), new Vector3(-0.7f, 1.18f, 0.02f), new Vector3(-0.68f, 0.82f, 0.05f), 0.02f, 6);
            b.Done("TowelHorse", true);
            RmSolid(kit, g, new Vector3(0f, 0.6f, 0f), new Vector3(1.6f, 1.2f, 0.4f));
        }

        static void KrWireless(LevelKit kit)
        {
            Vector3 at;
            if (!RmWall("the wireless on its shelf", new Vector3(-34f, 1.7f, 87f), Vector3.left, new Vector3(1.1f, 0.8f, 0.42f), 7f, out at)) return;
            var g = RmGroup(kit, "Wireless", at, Quaternion.LookRotation(Vector3.left));
            var b = new RmBits(kit, g);
            var wood = PatchworkAssets.Planks(new Color(0.5f, 0.34f, 0.22f));
            b.Box(wood, new Vector3(0f, -0.2f, 0.2f), new Vector3(1.05f, 0.04f, 0.38f));
            foreach (int s in new[] { -1, 1 }) b.Box(IronMat, new Vector3(s * 0.4f, -0.3f, 0.12f), new Vector3(0.03f, 0.2f, 0.2f), new Vector3(45f, 0f, 0f));
            // An old valve wireless, its dial glowing, playing quietly to the fish.
            b.Box(wood, new Vector3(-0.15f, 0.04f, 0.2f), new Vector3(0.5f, 0.44f, 0.3f));
            b.Ring(wood, new Vector3(-0.15f, 0.26f, 0.2f), new Vector3(0f, 90f, 0f), 0.25f, 0.06f, 10, 0f, 180f);
            b.Disc(RmFelt(new Color(0.65f, 0.55f, 0.4f)), new Vector3(-0.15f, 0.1f, 0.355f), 0.24f, 0.01f);
            for (int i = 0; i < 5; i++) b.Box(wood, new Vector3(-0.15f, 0.1f, 0.362f), new Vector3(0.01f, 0.26f, 0.008f), new Vector3(0f, 0f, i * 36f));
            b.Box(RmGlow(new Color(1f, 0.78f, 0.4f), 1.4f), new Vector3(-0.15f, -0.08f, 0.356f), new Vector3(0.3f, 0.06f, 0.01f));
            foreach (int s in new[] { -1, 1 }) b.Cyl(RmShine(new Color(0.15f, 0.1f, 0.08f), 0.6f), new Vector3(-0.15f + s * 0.18f, -0.1f, 0.36f), 0.035f, 0.005f, 8);
            b.Tube(IronMat, new Vector3(0f, 0.27f, 0.15f), new Vector3(0.1f, 0.6f, 0.1f), 0.006f, 4);
            // A cactus in a pot, and a mug of cold tea.
            b.Cyl(RmFelt(new Color(0.75f, 0.42f, 0.3f)), new Vector3(0.3f, -0.18f, 0.2f), 0.07f, 0.1f, 10);
            b.Ball(RmFelt(new Color(0.35f, 0.6f, 0.35f)), new Vector3(0.3f, -0.0f, 0.2f), new Vector3(0.09f, 0.2f, 0.09f), 3, 7);
            b.Ball(RmFelt(new Color(1f, 0.55f, 0.7f)), new Vector3(0.3f, 0.11f, 0.2f), Vector3.one * 0.04f, 2, 5);
            b.Cyl(RmFelt(new Color(0.3f, 0.45f, 0.75f)), new Vector3(0.44f, -0.18f, 0.18f), 0.04f, 0.1f, 10);
            b.Done("Wireless", true);
        }

        // ================================================================ The Tank Hall

        static void RoomsTankHall(LevelKit kit)
        {
            ThTemperatures(kit);
            ThDrawings(kit);
            ThSkiff(kit);
            ThGlassCrate(kit);
            ThPigeons(kit);
            ThDryingNet(kit);
        }

        static Transform thBoard;

        static void ThTemperatures(LevelKit kit)
        {
            thBoard = null;
            Vector3 at;
            if (!RmWall("the board of tank temperatures", new Vector3(-76f, 3.4f, 95f), Vector3.forward, new Vector3(3.6f, 1.6f, 0.25f), 8f, out at)) return;
            var g = RmGroup(kit, "TankTemperatures", at, Quaternion.LookRotation(Vector3.forward));
            thBoard = g;
            var b = new RmBits(kit, g);
            b.Box(PatchworkAssets.Planks(new Color(0.2f, 0.28f, 0.3f)), new Vector3(0f, 0f, 0.03f), new Vector3(3.5f, 1.5f, 0.06f));
            b.Box(Brass, new Vector3(0f, 0.72f, 0.065f), new Vector3(3.5f, 0.05f, 0.04f));
            b.Box(Brass, new Vector3(0f, -0.72f, 0.065f), new Vector3(3.5f, 0.05f, 0.04f));
            // One dial for each tank, from cold blue round to warm red, and where its needle sits.
            string[] names = { "CORAL", "DEEP", "FROZEN", "MANGROVE" };
            float[] warm = { 0.85f, 0.25f, 0.05f, 0.95f };
            Color cold = new Color(0.3f, 0.55f, 1f), hot = new Color(1f, 0.35f, 0.25f);
            for (int i = 0; i < 4; i++)
            {
                // (Local x runs to the viewer's left here, so the first tank goes on the right.)
                Vector3 c = new Vector3(1.29f - i * 0.86f, 0.12f, 0.07f);
                b.Disc(Brass, c, 0.62f, 0.03f);
                b.Disc(RmFelt(CreamCol), c + Vector3.forward * 0.02f, 0.54f, 0.01f);
                for (int k = 0; k < 9; k++)
                {
                    float a = (-120f + k * 30f) * Mathf.Deg2Rad;
                    var tick = RmFelt(Color.Lerp(cold, hot, k / 8f));
                    b.Box(tick, c + new Vector3(Mathf.Sin(a) * 0.21f, Mathf.Cos(a) * 0.21f, 0.03f), new Vector3(0.05f, 0.03f, 0.005f), new Vector3(0f, 0f, -(-120f + k * 30f) + 90f));
                }
                Letters(b[RmFelt(new Color(1f, 0.9f, 0.65f))], names[i], c + new Vector3(0f, -0.48f, 0.0f), Vector3.forward, 0.018f, 0.008f);
                // The needle: set to that tank's warmth (from the viewer's side, clockwise is warmer).
                var hub = kit.Group("DialHub", g.TransformPoint(c + new Vector3(0f, 0f, 0.04f)));
                hub.SetParent(g, true);
                hub.localRotation = Quaternion.Euler(0f, 0f, -(-120f + warm[i] * 240f));
                var needle = kit.Group("Needle", hub.position);
                needle.SetParent(hub, true);
                needle.localRotation = Quaternion.identity;
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.09f, 0f), new Vector3(0.014f, 0.18f, 0.005f), RmFelt(new Color(0.12f, 0.12f, 0.14f)), default(Vector3), needle));
                needle.gameObject.AddComponent<RmTick>().Angle = 2f;
                b.Ball(Brass, c + new Vector3(0f, 0f, 0.05f), Vector3.one * 0.04f, 2, 5);
            }
            b.Done("TankTemperatures", true);
        }

        static void ThDrawings(LevelKit kit)
        {
            Vector3 at;
            if (!RmWall("the children's drawings", new Vector3(-45f, 2.1f, 95f), Vector3.forward, new Vector3(3.2f, 1.3f, 0.08f), 8f, out at)) return;
            var g = RmGroup(kit, "ThankYouDrawings", at, Quaternion.LookRotation(Vector3.forward));
            var b = new RmBits(kit, g);
            b.Box(RmFelt(new Color(0.95f, 0.85f, 0.35f)), new Vector3(0f, 0.55f, 0.01f), new Vector3(1.4f, 0.16f, 0.01f));
            Letters(b[RmFelt(new Color(0.8f, 0.25f, 0.3f))], "THANK YOU", new Vector3(0f, 0.55f, 0.016f), Vector3.forward, 0.02f, 0.006f);
            var paper = RmFelt(new Color(0.97f, 0.95f, 0.9f));
            var rng = new System.Random(2024);
            for (int i = 0; i < 6; i++)
            {
                Vector3 c = new Vector3(1.25f - i * 0.5f, (i % 2) * 0.12f - 0.12f, 0.012f);
                float tilt = ((float)rng.NextDouble() - 0.5f) * 12f;
                b.Box(paper, c, new Vector3(0.42f, 0.32f, 0.006f), new Vector3(0f, 0f, tilt));
                b.Ball(RmFelt(new Color(0.85f, 0.2f, 0.2f)), c + new Vector3(0f, 0.15f, 0.006f), Vector3.one * 0.025f, 2, 4);
                var q = Quaternion.Euler(0f, 0f, tilt);
                // Crayon pictures: a fish, a penguin, a whale, a starfish, an octopus, the sun over the sea.
                Color[] cr = { new Color(1f, 0.55f, 0.15f), new Color(0.15f, 0.15f, 0.2f), new Color(0.3f, 0.45f, 0.9f), new Color(1f, 0.4f, 0.5f), new Color(0.6f, 0.3f, 0.75f), new Color(1f, 0.8f, 0.2f) };
                var ink = RmFelt(cr[i]);
                switch (i)
                {
                    case 0: b.Ball(ink, c + q * new Vector3(0.03f, 0f, 0.008f), new Vector3(0.18f, 0.1f, 0.004f), 3, 8); b.Box(ink, c + q * new Vector3(-0.1f, 0f, 0.008f), new Vector3(0.07f, 0.07f, 0.004f), new Vector3(0f, 0f, tilt + 45f)); break;
                    case 1: b.Ball(ink, c + q * new Vector3(0f, -0.02f, 0.008f), new Vector3(0.1f, 0.18f, 0.004f), 3, 8); b.Ball(RmFelt(Color.white), c + q * new Vector3(0f, -0.03f, 0.01f), new Vector3(0.06f, 0.12f, 0.004f), 3, 8); b.Box(RmFelt(new Color(1f, 0.6f, 0.1f)), c + q * new Vector3(0f, 0.05f, 0.012f), new Vector3(0.03f, 0.015f, 0.004f)); break;
                    case 2: b.Ball(ink, c + q * new Vector3(0f, 0f, 0.008f), new Vector3(0.28f, 0.12f, 0.004f), 3, 8); b.Box(ink, c + q * new Vector3(-0.15f, 0.03f, 0.008f), new Vector3(0.06f, 0.06f, 0.004f), new Vector3(0f, 0f, tilt + 45f)); for (int k = 0; k < 3; k++) b.Box(RmFelt(new Color(0.5f, 0.75f, 1f)), c + q * new Vector3(0.06f + k * 0.015f, 0.1f, 0.008f), new Vector3(0.006f, 0.05f, 0.004f), new Vector3(0f, 0f, tilt + (k - 1) * 25f)); break;
                    case 3: for (int k = 0; k < 5; k++) b.Box(ink, c + q * new Vector3(0f, 0f, 0.008f), new Vector3(0.03f, 0.2f, 0.004f), new Vector3(0f, 0f, tilt + k * 72f)); break;
                    case 4: b.Ball(ink, c + q * new Vector3(0f, 0.04f, 0.008f), new Vector3(0.12f, 0.12f, 0.004f), 3, 8); for (int k = 0; k < 6; k++) b.Box(ink, c + q * new Vector3(-0.06f + k * 0.024f, -0.06f, 0.008f), new Vector3(0.012f, 0.08f, 0.004f), new Vector3(0f, 0f, tilt + (k - 2.5f) * 8f)); break;
                    default: b.Ball(ink, c + q * new Vector3(0.08f, 0.06f, 0.008f), new Vector3(0.1f, 0.1f, 0.004f), 3, 8); b.Box(RmFelt(new Color(0.3f, 0.5f, 0.95f)), c + q * new Vector3(0f, -0.08f, 0.008f), new Vector3(0.38f, 0.1f, 0.004f), new Vector3(0f, 0f, tilt)); break;
                }
            }
            b.Done("ThankYouDrawings", true);
        }

        static void ThSkiff(LevelKit kit)
        {
            Vector3 at;
            if (!RmWall("the keepers' skiff", new Vector3(-88f, 4.4f, 141f), Vector3.right, new Vector3(4.4f, 1.3f, 1.4f), 10f, out at)) return;
            var g = RmGroup(kit, "KeepersSkiff", at, Quaternion.LookRotation(Vector3.right));
            var b = new RmBits(kit, g);
            // Two iron brackets out of the wall, and the little boat sitting in them.
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(IronMat, new Vector3(s * 1.1f, -0.45f, 0.6f), new Vector3(0.06f, 0.06f, 1.2f));
                b.Box(IronMat, new Vector3(s * 1.1f, -0.1f, 0.3f), new Vector3(0.05f, 0.05f, 0.9f), new Vector3(-50f, 0f, 0f));
                b.Box(IronMat, new Vector3(s * 1.1f, -0.45f, 0.02f), new Vector3(0.14f, 0.22f, 0.04f));
            }
            var paint = PatchworkAssets.Planks(new Color(0.2f, 0.45f, 0.55f));
            var inside = PatchworkAssets.Planks(new Color(0.7f, 0.58f, 0.42f));
            var rim = PatchworkAssets.Planks(new Color(0.85f, 0.85f, 0.8f));
            const float len = 4f, beam = 1.1f, deep = 0.5f;
            Vector3 mid = new Vector3(0f, -0.15f, 0.65f);
            // Planks round the hull, narrowing to a point at each end.
            for (int i = 0; i < 16; i++)
            {
                float t0 = i / 16f, t1 = (i + 1) / 16f;
                float x0 = -len * 0.5f + t0 * len, x1 = -len * 0.5f + t1 * len;
                float w0 = Mathf.Sin(t0 * Mathf.PI) * beam * 0.5f + 0.02f, w1 = Mathf.Sin(t1 * Mathf.PI) * beam * 0.5f + 0.02f;
                float xm = (x0 + x1) * 0.5f, wm = (w0 + w1) * 0.5f;
                for (int k = 0; k < 4; k++)
                {
                    float a = k / 3f * Mathf.PI * 0.5f;
                    foreach (int s in new[] { -1, 1 })
                        b.Box(paint, mid + new Vector3(xm, -Mathf.Cos(a) * deep * 0.9f + deep * 0.35f - (1f - Mathf.Sin(a)) * 0.1f, s * Mathf.Sin(a) * wm), new Vector3(x1 - x0 + 0.02f, 0.14f, 0.03f), new Vector3(s * (90f - a * Mathf.Rad2Deg), 0f, 0f));
                }
                b.Box(inside, mid + new Vector3(xm, -deep * 0.45f, 0f), new Vector3(x1 - x0 + 0.02f, 0.02f, wm * 1.5f));
                foreach (int s in new[] { -1, 1 })
                    b.Box(rim, mid + new Vector3(xm, deep * 0.4f, s * wm), new Vector3(x1 - x0 + 0.03f, 0.05f, 0.05f));
            }
            foreach (float x in new[] { -0.6f, 0.5f }) b.Box(inside, mid + new Vector3(x, 0.05f, 0f), new Vector3(0.25f, 0.04f, beam * 0.95f));
            // Its name on the bow, and the oars crossed over it on two pegs.
            Letters(b[RmFelt(Color.white)], "SALTY", mid + new Vector3(1.2f, 0.05f, beam * 0.5f + 0.01f), Vector3.forward, 0.03f, 0.01f);
            var oar = PatchworkAssets.Planks(new Color(0.75f, 0.6f, 0.4f));
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(oar, new Vector3(0f, 0.75f, 0.12f), new Vector3(2.6f, 0.05f, 0.05f), new Vector3(0f, 0f, s * 12f));
                b.Box(oar, new Vector3(s * 1.35f, 0.75f + s * 0.28f, 0.12f), new Vector3(0.45f, 0.14f, 0.02f), new Vector3(0f, 0f, s * 12f));
                b.Box(IronMat, new Vector3(s * 0.5f, 0.76f + s * 0.1f, 0.05f), new Vector3(0.04f, 0.04f, 0.12f));
            }
            b.Done("KeepersSkiff", true);
        }

        static void ThGlassCrate(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the crate of glass", new Vector3(-37f, 0f, 130f), new Vector3(1.8f, 1.9f, 1f), 90f, 7f, out f)) return;
            var g = RmGroup(kit, "GlassCrate", f, 90f);
            var b = new RmBits(kit, g);
            var slat = PatchworkAssets.Planks(new Color(0.78f, 0.65f, 0.45f));
            const float w = 1.6f, h = 1.8f, d = 0.45f;
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(slat, new Vector3(s * w * 0.5f, h * 0.5f, 0f), new Vector3(0.1f, h, d + 0.08f));
                for (int k = 0; k < 4; k++)
                    b.Box(slat, new Vector3(0f, 0.15f + k * 0.5f, s * d * 0.5f), new Vector3(w, 0.14f, 0.03f));
                b.Box(slat, new Vector3(0f, h * 0.5f, s * d * 0.5f), new Vector3(Mathf.Sqrt(w * w + h * h) * 0.92f, 0.12f, 0.03f), new Vector3(0f, 0f, s * Mathf.Atan2(h, w) * Mathf.Rad2Deg));
            }
            b.Box(slat, new Vector3(0f, h, 0f), new Vector3(w + 0.1f, 0.1f, d + 0.08f));
            b.Box(slat, new Vector3(0f, 0.04f, 0f), new Vector3(w + 0.1f, 0.08f, d + 0.08f));
            // Straw poking out between the slats, and the pane inside.
            var straw = RmFelt(new Color(0.9f, 0.78f, 0.45f));
            for (int i = 0; i < 24; i++)
                b.Box(straw, new Vector3(-0.7f + (i % 12) * 0.13f, 0.38f + (i / 12) * 1f, (i % 2 == 0 ? 1f : -1f) * (d * 0.5f + 0.02f)), new Vector3(0.008f, 0.12f, 0.06f), new Vector3(i * 23f, 0f, i * 17f));
            // Stencils: GLASS, and THIS WAY UP with arrows.
            var stencil = RmFelt(new Color(0.75f, 0.15f, 0.15f));
            Letters(b[stencil], "GLASS", new Vector3(0f, 0.9f, d * 0.5f + 0.02f), Vector3.forward, 0.04f, 0.006f);
            Letters(b[stencil], "THIS WAY UP", new Vector3(0f, 0.55f, d * 0.5f + 0.02f), Vector3.forward, 0.018f, 0.006f);
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(stencil, new Vector3(s * 0.55f, 1.25f, d * 0.5f + 0.02f), new Vector3(0.04f, 0.3f, 0.006f));
                b.Box(stencil, new Vector3(s * 0.55f - 0.04f, 1.38f, d * 0.5f + 0.02f), new Vector3(0.03f, 0.12f, 0.006f), new Vector3(0f, 0f, -40f));
                b.Box(stencil, new Vector3(s * 0.55f + 0.04f, 1.38f, d * 0.5f + 0.02f), new Vector3(0.03f, 0.12f, 0.006f), new Vector3(0f, 0f, 40f));
            }
            b.Done("GlassCrate", true);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.95f, 0f), new Vector3(1.4f, 1.6f, 0.04f), CaseGlass, default(Vector3), g));
            RmSolid(kit, g, new Vector3(0f, h * 0.5f, 0f), new Vector3(w + 0.1f, h, d + 0.1f));
        }

        // A pigeon, sitting, facing +z.
        static void RmPigeon(LevelKit kit, Vector3 at, float yaw, bool asleep)
        {
            var g = RmGroup(kit, "Pigeon", at, yaw);
            var b = new RmBits(kit, g);
            var grey = RmFelt(new Color(0.55f, 0.58f, 0.65f));
            var dark = RmFelt(new Color(0.35f, 0.37f, 0.45f));
            b.Ball(grey, new Vector3(0f, 0.12f, 0f), new Vector3(0.2f, 0.2f, 0.3f), 4, 8);
            b.Ball(RmShine(new Color(0.4f, 0.6f, 0.5f), 0.8f), new Vector3(0f, 0.2f, 0.09f), new Vector3(0.15f, 0.12f, 0.12f), 3, 7);
            foreach (int s in new[] { -1, 1 }) b.Box(dark, new Vector3(s * 0.09f, 0.14f, -0.04f), new Vector3(0.03f, 0.1f, 0.24f), new Vector3(-8f, 0f, 0f));
            b.Box(dark, new Vector3(0f, 0.1f, -0.2f), new Vector3(0.1f, 0.02f, 0.14f), new Vector3(-12f, 0f, 0f));
            foreach (int s in new[] { -1, 1 }) b.Box(RmFelt(new Color(0.9f, 0.45f, 0.5f)), new Vector3(s * 0.04f, 0.01f, 0.03f), new Vector3(0.02f, 0.03f, 0.06f));
            if (asleep)
            {
                b.Ball(grey, new Vector3(0f, 0.22f, 0.07f), new Vector3(0.11f, 0.08f, 0.1f), 3, 6);
                b.Done("Pigeon", true);
                return;
            }
            b.Done("Pigeon", true);
            var head = kit.Group("Head", g.TransformPoint(new Vector3(0f, 0.26f, 0.13f)));
            head.SetParent(g, true);
            head.localRotation = Quaternion.identity;
            var hb = new RmBits(kit, head);
            hb.Ball(grey, Vector3.zero, new Vector3(0.1f, 0.1f, 0.11f), 3, 7);
            hb.Box(RmFelt(new Color(0.85f, 0.75f, 0.6f)), new Vector3(0f, -0.01f, 0.07f), new Vector3(0.02f, 0.02f, 0.05f));
            RmEyes(hb, new Vector3(0f, 0.015f, 0.04f), 0.08f, 0.018f);
            hb.Done("PigeonHead", true);
            var nod = head.gameObject.AddComponent<RmSway>();
            nod.Amount = 9f;
            nod.Speed = 2.6f;
        }

        static void ThPigeons(LevelKit kit)
        {
            // Two pigeons who got in through a roof-light, sitting on top of the temperature board
            // (or, if that isn't there, on the south catwalk's rail).
            if (thBoard != null)
            {
                RmPigeon(kit, thBoard.TransformPoint(new Vector3(0.9f, 0.75f, 0.14f)), 82f, false);
                RmPigeon(kit, thBoard.TransformPoint(new Vector3(0.35f, 0.75f, 0.14f)), 262f, true);
                rmPlaced++;
                return;
            }
            float railTop = CatY + 1.09f;
            foreach (float x in new[] { -67.5f, -55.5f, -51f, -79f, -41f })
            {
                Vector3 c = new Vector3(x, railTop + 0.2f, 111f);
                if (!RmTry(c, new Vector3(1.1f, 0.36f, 0.36f), 0f)) continue;
                RmPigeon(kit, new Vector3(x - 0.3f, railTop, 111f), 170f, false);
                RmPigeon(kit, new Vector3(x + 0.25f, railTop, 111f), 200f, true);
                rmPlaced++;
                return;
            }
            RmMiss("the pigeons", new Vector3(-67.5f, railTop, 111f));
        }

        static void ThDryingNet(LevelKit kit)
        {
            float railTop = CatY + 1.09f;
            foreach (float x in new[] { -68.5f, -52.5f, -81f })
            {
                Vector3 c = new Vector3(x, railTop - 0.62f, 156.18f);
                if (!RmTry(c, new Vector3(3f, 1.3f, 0.22f), 0f)) continue;
                var g = kit.Group("DryingNet", new Vector3(x, railTop, 156f));
                var b = new RmBits(kit, g);
                var net = RmFelt(new Color(0.5f, 0.62f, 0.55f));
                // Hung over the rail on the outside, sagging, with cork floats along its edge.
                const int cols = 15, rows = 7;
                for (int i = 0; i <= cols; i++)
                {
                    float u = i / (float)cols, xx = -1.4f + u * 2.8f;
                    float sag = Mathf.Sin(u * Mathf.PI) * 0.25f;
                    b.Box(net, new Vector3(xx, -0.6f - sag * 0.5f, 0.1f), new Vector3(0.012f, 1.2f + sag, 0.012f), new Vector3(4f, 0f, 0f));
                }
                for (int k = 0; k <= rows; k++)
                {
                    float v = k / (float)rows;
                    b.Box(net, new Vector3(0f, -v * 1.15f - Mathf.Pow(v, 2f) * 0.2f, 0.06f + v * 0.1f), new Vector3(2.8f, 0.012f, 0.012f));
                }
                b.Box(net, new Vector3(0f, 0.02f, 0f), new Vector3(2.9f, 0.04f, 0.06f));
                for (int i = 0; i < 7; i++)
                    b.Ball(RmFelt(new Color(0.85f, 0.7f, 0.45f)), new Vector3(-1.3f + i * 0.43f, -1.3f - Mathf.Sin(i / 6f * Mathf.PI) * 0.15f, 0.16f), new Vector3(0.12f, 0.08f, 0.08f), 3, 6);
                b.Done("DryingNet", true);
                for (int i = 0; i < 3; i++)
                {
                    var drop = NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(-0.9f + i * 0.9f, -1.36f, 0.16f), Vector3.one * 0.06f, RmShine(new Color(0.6f, 0.8f, 1f), 0.95f), default(Vector3), g));
                    var drip = drop.gameObject.AddComponent<DripLoop>();
                    drip.Fall = railTop - 1.36f - WaterTop;
                    drip.Period = 2.2f + i * 0.7f;
                }
                rmPlaced++;
                return;
            }
            RmMiss("the drying net", new Vector3(-68.5f, railTop, 156f));
        }

        // ================================================================ The Undercroft

        static void RoomsUndercroft(LevelKit kit)
        {
            UcSpeakingTube(kit);
            UcBinoculars(kit);
            UcFieldGuides(kit);
            UcLeak(kit);
            UcFireBucket(kit);
            UcGlassSign(kit);
        }

        static void UcSpeakingTube(LevelKit kit)
        {
            Vector3 at;
            if (!RmWall("the speaking tube", new Vector3(-58f, -5.1f, 108.7f), Vector3.left, new Vector3(0.36f, 2.9f, 0.3f), 0.6f, out at, false)) return;
            var g = RmGroup(kit, "SpeakingTube", at, Quaternion.LookRotation(Vector3.left));
            var b = new RmBits(kit, g);
            // A brass pipe up through the ceiling to the deck above, so you can shout up for more tea.
            float top = -3.5f - at.y;
            b.Tube(Brass, new Vector3(0f, -0.95f, 0.1f), new Vector3(0f, top, 0.1f), 0.04f, 8);
            for (int i = 0; i < 3; i++) b.Box(Brass, new Vector3(0f, -0.6f + i * 0.9f, 0.05f), new Vector3(0.12f, 0.03f, 0.1f));
            b.Tube(Brass, new Vector3(0f, -0.95f, 0.1f), new Vector3(0f, -1.12f, 0.18f), 0.05f, 8);
            b.Ring(Brass, new Vector3(0f, -1.18f, 0.22f), new Vector3(-30f, 0f, 0f), 0.08f, 0.03f, 12);
            b.Disc(RmShine(new Color(0.1f, 0.1f, 0.1f), 0.4f), new Vector3(0f, -1.18f, 0.21f), 0.13f, 0.01f);
            // Its whistle cap, hanging on a little chain.
            b.Cyl(Brass, new Vector3(0.12f, -1.32f, 0.22f), 0.05f, 0.07f, 8);
            b.Box(Brass, new Vector3(0.07f, -1.24f, 0.22f), new Vector3(0.006f, 0.14f, 0.006f), new Vector3(0f, 0f, 30f));
            b.Done("SpeakingTube", true);
        }

        static void UcBinoculars(LevelKit kit)
        {
            Vector3 at;
            if (!RmWall("the binoculars", new Vector3(-62f, -4.7f, 113.8f), Vector3.right, new Vector3(0.36f, 0.5f, 0.18f), 1f, out at)) return;
            var g = RmGroup(kit, "Binoculars", at, Quaternion.LookRotation(Vector3.right));
            var b = new RmBits(kit, g);
            b.Box(Brass, new Vector3(0f, 0.18f, 0.03f), new Vector3(0.03f, 0.03f, 0.06f));
            var strap = RmShine(new Color(0.35f, 0.22f, 0.14f), 0.3f);
            b.Box(strap, new Vector3(-0.06f, 0.08f, 0.05f), new Vector3(0.012f, 0.22f, 0.006f), new Vector3(0f, 0f, -25f));
            b.Box(strap, new Vector3(0.06f, 0.08f, 0.05f), new Vector3(0.012f, 0.22f, 0.006f), new Vector3(0f, 0f, 25f));
            var body = RmShine(new Color(0.12f, 0.12f, 0.14f), 0.5f);
            foreach (int s in new[] { -1, 1 })
            {
                b.Tube(body, new Vector3(s * 0.06f, -0.02f, 0.07f), new Vector3(s * 0.06f, -0.2f, 0.07f), 0.045f, 8);
                b.Disc(RmGlow(new Color(0.5f, 0.75f, 1f), 0.5f), new Vector3(s * 0.06f, -0.21f, 0.07f), 0.07f, 0.004f);
            }
            b.Box(Brass, new Vector3(0f, -0.08f, 0.07f), new Vector3(0.1f, 0.03f, 0.03f));
            b.Done("Binoculars", true);
        }

        static void UcFieldGuides(LevelKit kit)
        {
            Vector3 at;
            if (!RmWall("the shelf of field guides", new Vector3(-58f, -4.8f, 119.3f), Vector3.left, new Vector3(1f, 0.45f, 0.3f), 1f, out at)) return;
            var g = RmGroup(kit, "FieldGuides", at, Quaternion.LookRotation(Vector3.left));
            var b = new RmBits(kit, g);
            b.Box(DarkWood, new Vector3(0f, -0.2f, 0.14f), new Vector3(0.95f, 0.03f, 0.26f));
            foreach (int s in new[] { -1, 1 }) b.Box(IronMat, new Vector3(s * 0.38f, -0.28f, 0.08f), new Vector3(0.025f, 0.15f, 0.15f), new Vector3(45f, 0f, 0f));
            Color[] spines = { Teal, Wine, Navy, new Color(0.65f, 0.5f, 0.2f), Plum, new Color(0.3f, 0.5f, 0.35f), new Color(0.75f, 0.35f, 0.25f) };
            float x = -0.42f;
            for (int i = 0; i < 9; i++)
            {
                float w = 0.05f + (i % 3) * 0.015f, h = 0.2f + (i % 4) * 0.025f;
                bool lean = i == 8;
                b.Box(RmFelt(spines[i % spines.Length]), new Vector3(x + w * 0.5f + (lean ? 0.04f : 0f), -0.185f + h * 0.5f, 0.14f), new Vector3(w, h, 0.18f), new Vector3(0f, 0f, lean ? -18f : 0f));
                b.Box(RmGlow(new Color(1f, 0.82f, 0.4f), 0.5f), new Vector3(x + w * 0.5f + (lean ? 0.04f : 0f), -0.185f + h * 0.72f, 0.232f), new Vector3(w * 0.6f, 0.012f, 0.004f), new Vector3(0f, 0f, lean ? -18f : 0f));
                x += w + 0.008f;
            }
            // A seashell bookend, and a magnifying glass.
            b.Ball(RmFelt(new Color(0.98f, 0.85f, 0.75f)), new Vector3(0.36f, -0.12f, 0.14f), new Vector3(0.12f, 0.13f, 0.1f), 3, 7);
            for (int k = 0; k < 5; k++) b.Box(RmFelt(new Color(0.9f, 0.65f, 0.55f)), new Vector3(0.36f, -0.12f, 0.19f), new Vector3(0.008f, 0.12f, 0.004f), new Vector3(0f, 0f, (k - 2) * 18f));
            b.Ring(Brass, new Vector3(0.22f, -0.175f, 0.16f), new Vector3(90f, 0f, 0f), 0.05f, 0.012f, 10);
            b.Box(DarkWood, new Vector3(0.22f, -0.178f, 0.23f), new Vector3(0.02f, 0.015f, 0.09f));
            b.Done("FieldGuides", true);
        }

        static void UcLeak(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the leak bucket", new Vector3(-61.2f, -8f, 125.2f), new Vector3(0.5f, 0.45f, 0.5f), 0f, 1.5f, out f)) return;
            var g = kit.Group("LeakBucket", f);
            var b = new RmBits(kit, g);
            // A tin bucket set under a drip from the ceiling, a damp ring on the floor round it.
            var tin = RmShine(new Color(0.65f, 0.68f, 0.72f), 0.8f);
            for (int i = 0; i < 4; i++) b.Ring(tin, new Vector3(0f, 0.04f + i * 0.1f, 0f), new Vector3(90f, 0f, 0f), 0.16f + i * 0.015f, 0.03f, 14);
            b.Cyl(tin, Vector3.zero, 0.16f, 0.02f, 12);
            b.Cyl(RmShine(new Color(0.3f, 0.5f, 0.6f), 0.95f), new Vector3(0f, 0.02f, 0f), 0.17f, 0.24f, 14);
            b.Ring(IronMat, new Vector3(0f, 0.4f, 0f), Vector3.zero, 0.2f, 0.012f, 10, 0f, 180f);
            b.Done("LeakBucket", true);
            float ceiling = -3.5f;
            RaycastHit hit;
            if (Physics.Raycast(f + Vector3.up * 0.6f, Vector3.up, out hit, 6f, RmMask, QueryTriggerInteraction.Ignore)) ceiling = hit.point.y;
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(f.x, ceiling - 0.01f, f.z), new Vector3(0.5f, 0.004f, 0.5f), RmFelt(new Color(0.18f, 0.2f, 0.22f))));
            var drop = NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, ceiling - f.y - 0.05f, 0f), Vector3.one * 0.06f, RmShine(new Color(0.6f, 0.8f, 1f), 0.95f), default(Vector3), g));
            var drip = drop.gameObject.AddComponent<DripLoop>();
            drip.Fall = ceiling - f.y - 0.3f;
            drip.Period = 1.8f;
            Mark("Story_UndercroftLeak", f);
        }

        static void UcFireBucket(LevelKit kit)
        {
            Vector3 at;
            if (!RmWall("the fire bucket", new Vector3(-62f, -6.5f, 124.8f), Vector3.right, new Vector3(0.6f, 1.1f, 0.4f), 1f, out at)) return;
            var g = RmGroup(kit, "FireBucket", at, Quaternion.LookRotation(Vector3.right));
            var b = new RmBits(kit, g);
            var red = RmShine(new Color(0.8f, 0.15f, 0.15f), 0.7f);
            b.Box(IronMat, new Vector3(0f, 0.38f, 0.04f), new Vector3(0.05f, 0.05f, 0.08f));
            b.Ring(IronMat, new Vector3(0f, 0.2f, 0.2f), new Vector3(0f, 0f, 0f), 0.16f, 0.012f, 10, 0f, 180f);
            for (int i = 0; i < 4; i++) b.Ring(red, new Vector3(0f, 0.04f + i * 0.08f - 0.15f, 0.2f), new Vector3(90f, 0f, 0f), 0.12f + i * 0.015f, 0.03f, 14);
            b.Cyl(RmFelt(new Color(0.9f, 0.8f, 0.55f)), new Vector3(0f, 0.08f, 0.2f), 0.16f, 0.02f, 12);
            Letters(b[RmFelt(Color.white)], "FIRE", new Vector3(0f, -0.02f, 0.37f), Vector3.forward, 0.014f, 0.005f);
            // And an emergency lantern beside it, ready.
            b.Box(IronMat, new Vector3(0.32f, 0.38f, 0.04f), new Vector3(0.05f, 0.05f, 0.08f));
            b.Cyl(IronMat, new Vector3(0.32f, -0.05f, 0.16f), 0.08f, 0.04f, 8);
            b.Ball(RmGlow(new Color(1f, 0.75f, 0.4f), 0.6f), new Vector3(0.32f, 0.07f, 0.16f), new Vector3(0.11f, 0.16f, 0.11f), 3, 7);
            for (int k = 0; k < 4; k++) b.Box(IronMat, new Vector3(0.32f + Mathf.Cos(k * 1.57f) * 0.06f, 0.07f, 0.16f + Mathf.Sin(k * 1.57f) * 0.06f), new Vector3(0.012f, 0.18f, 0.012f));
            b.Cyl(IronMat, new Vector3(0.32f, 0.16f, 0.16f), 0.07f, 0.03f, 8);
            b.Ring(IronMat, new Vector3(0.32f, 0.27f, 0.16f), Vector3.zero, 0.05f, 0.01f, 8, 0f, 180f);
            b.Done("FireBucket", true);
        }

        static void UcGlassSign(LevelKit kit)
        {
            Vector3 at;
            if (!RmWall("the sign about the glass", new Vector3(-58f, -4.4f, 113.75f), Vector3.left, new Vector3(0.95f, 0.42f, 0.06f), 1f, out at)) return;
            var g = RmGroup(kit, "GlassSign", at, Quaternion.LookRotation(Vector3.left));
            var b = new RmBits(kit, g);
            b.Box(Brass, new Vector3(0f, 0f, 0.015f), new Vector3(0.92f, 0.4f, 0.03f));
            b.Box(RmFelt(CreamCol), new Vector3(0f, 0f, 0.032f), new Vector3(0.86f, 0.34f, 0.006f));
            var ink = RmFelt(new Color(0.2f, 0.3f, 0.45f));
            Letters(b[ink], "PLEASE DON'T", new Vector3(0.08f, 0.07f, 0.035f), Vector3.forward, 0.01f, 0.004f);
            Letters(b[ink], "TAP THE GLASS", new Vector3(0.08f, -0.06f, 0.035f), Vector3.forward, 0.01f, 0.004f);
            // A little fish, looking cross about it.
            Vector3 c = new Vector3(-0.33f, 0f, 0.036f);
            b.Ball(RmFelt(new Color(1f, 0.6f, 0.2f)), c, new Vector3(0.12f, 0.08f, 0.006f), 3, 8);
            b.Box(RmFelt(new Color(1f, 0.6f, 0.2f)), c + new Vector3(0.08f, 0f, 0f), new Vector3(0.05f, 0.05f, 0.006f), new Vector3(0f, 0f, 45f));
            b.Ball(RmFelt(Color.white), c + new Vector3(-0.03f, 0.01f, 0.004f), new Vector3(0.025f, 0.025f, 0.004f), 2, 5);
            b.Ball(RmFelt(new Color(0.05f, 0.05f, 0.05f)), c + new Vector3(-0.032f, 0.008f, 0.006f), new Vector3(0.012f, 0.012f, 0.004f), 2, 4);
            b.Box(RmFelt(new Color(0.05f, 0.05f, 0.05f)), c + new Vector3(-0.03f, 0.03f, 0.006f), new Vector3(0.03f, 0.005f, 0.004f), new Vector3(0f, 0f, -20f));
            b.Done("GlassSign", true);
        }

        // ================================================================ The Coral Nursery

        static void RoomsCoral(LevelKit kit)
        {
            var T = CoralTank;
            CnParrotfish(kit, T);
            CnTreeWorms(kit, T);
            CnSpawning(kit, T);
            CnBrainCoral(kit, T);
            CnPufferfish(kit, T);
            CnSlate(kit, T);
        }

        static void CnParrotfish(LevelKit kit, float[] T)
        {
            // Parrotfish crunch on dead coral with their beaks, and what comes out the other end
            // is sand: a lot of the white sand on a reef has been through a parrotfish.
            var male = Swim(kit, Swimmer.Fish, new Vector3(-76f, T[4] + 3.4f, 109f), 3.2f, 2.4f, 13f, 1.15f, new Color(0.25f, 0.72f, 0.65f), new Color(1f, 0.55f, 0.75f), 40f);
            var female = Swim(kit, Swimmer.Fish, new Vector3(-75f, T[4] + 2.6f, 116f), 2.6f, 2f, -11f, 0.95f, new Color(0.7f, 0.35f, 0.3f), new Color(0.95f, 0.8f, 0.6f), 210f);
            foreach (var f in new[] { male, female })
            {
                var b = new RmBits(kit, f.transform);
                // The beak: two white plates where the lips should be.
                b.Box(RmFelt(new Color(0.95f, 0.95f, 0.9f)), new Vector3(0f, -0.04f, 0.55f), new Vector3(0.16f, 0.06f, 0.1f), new Vector3(10f, 0f, 0f));
                b.Box(RmFelt(new Color(0.95f, 0.95f, 0.9f)), new Vector3(0f, -0.1f, 0.53f), new Vector3(0.14f, 0.05f, 0.09f), new Vector3(-10f, 0f, 0f));
                b.Done("ParrotBeak");
            }
            rmPlaced++;
        }

        static void CnTreeWorms(LevelKit kit, float[] T)
        {
            Vector3 f;
            if (!RmFloor("the Christmas tree worms", new Vector3(-70f, T[4], 120f), new Vector3(1.6f, 1.4f, 1.6f), 0f, 7f, out f, 1.5f)) return;
            var g = kit.Group("TreeWormRock", f);
            var b = new RmBits(kit, g);
            b.Ball(RmFelt(new Color(0.75f, 0.68f, 0.55f)), new Vector3(0f, 0.35f, 0f), new Vector3(1.4f, 0.8f, 1.3f), 4, 9);
            b.Ball(RmFelt(new Color(0.65f, 0.6f, 0.5f)), new Vector3(0.3f, 0.6f, 0.1f), new Vector3(0.7f, 0.5f, 0.6f), 3, 8);
            b.Done("TreeWormRock", true);
            // Little spiral fir trees in every colour, two to a worm; they pop back into their
            // tubes in a blink if anything comes too close.
            Color[] cols = { new Color(1f, 0.55f, 0.1f), new Color(0.3f, 0.55f, 1f), new Color(1f, 0.9f, 0.3f), new Color(0.95f, 0.25f, 0.3f), new Color(0.95f, 0.95f, 0.95f), new Color(0.6f, 0.35f, 0.9f) };
            for (int w = 0; w < 9; w++)
            {
                float a = w * 2.3f, r = 0.25f + (w % 3) * 0.14f;
                Vector3 p = new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
                p.y = 0.35f + 0.4f * Mathf.Sqrt(Mathf.Max(0f, 1f - (p.x * p.x) / 0.49f - (p.z * p.z) / 0.42f)) + (w % 2) * 0.05f;
                var worm = kit.Group("TreeWorm", g.TransformPoint(p));
                worm.SetParent(g, true);
                worm.localRotation = Quaternion.identity;
                var wb = new RmBits(kit, worm);
                var col = RmGlow(cols[w % cols.Length], 0.35f);
                foreach (int s in new[] { -1, 1 })
                    for (int k = 0; k < 6; k++)
                    {
                        float rr = 0.06f - k * 0.009f;
                        wb.Ring(col, new Vector3(s * 0.045f, 0.02f + k * 0.03f, 0f), new Vector3(90f, k * 40f, 0f), rr, 0.014f, 8, 0f, 300f);
                    }
                wb.Done("TreeWorm");
                worm.gameObject.AddComponent<GardenEel>().Shy = 2.4f;
            }
        }

        static void CnSpawning(LevelKit kit, float[] T)
        {
            // Once a year, a few nights after a full moon, the whole reef lets go of its eggs at
            // once: little pink beads floating up towards the surface. It's happening tonight.
            Vector3 c = new Vector3((T[0] + T[1]) * 0.5f, (T[4] + WaterTop) * 0.5f, (T[2] + T[3]) * 0.5f);
            Motes(kit, c, new Vector3(T[1] - T[0] - 3f, WaterTop - T[4] - 1f, T[3] - T[2] - 3f), 90, new Color(1f, 0.55f, 0.65f), 1.3f, 0.07f, new Vector3(0.02f, 0.2f, 0.015f));
            Motes(kit, c, new Vector3(T[1] - T[0] - 3f, WaterTop - T[4] - 1f, T[3] - T[2] - 3f), 50, new Color(1f, 0.85f, 0.6f), 1.2f, 0.05f, new Vector3(-0.015f, 0.16f, 0.02f));
            rmPlaced++;
        }

        static void CnBrainCoral(LevelKit kit, float[] T)
        {
            Vector3 f;
            if (!RmFloor("the brain coral", new Vector3(-81f, T[4], 103.5f), new Vector3(2.2f, 1.3f, 2f), 0f, 7f, out f, 1.5f)) return;
            var g = kit.Group("BrainCoral", f);
            var b = new RmBits(kit, g);
            b.Ball(RmFelt(new Color(0.6f, 0.62f, 0.4f)), new Vector3(0f, 0.3f, 0f), new Vector3(1.9f, 1.3f, 1.7f), 6, 12);
            // Its ridges, wandering every which way over the dome like the folds of a brain.
            var ridge = RmFelt(new Color(0.78f, 0.8f, 0.55f));
            var rng = new System.Random(5150);
            for (int path = 0; path < 14; path++)
            {
                float th = (float)rng.NextDouble() * 1.2f, ph = (float)rng.NextDouble() * Mathf.PI * 2f;
                float dth = 0f, dph = 0.12f;
                for (int step = 0; step < 14; step++)
                {
                    Vector3 p = new Vector3(Mathf.Sin(th) * Mathf.Cos(ph) * 0.95f, 0.3f + Mathf.Cos(th) * 0.65f, Mathf.Sin(th) * Mathf.Sin(ph) * 0.85f);
                    if (p.y < 0.05f) break;
                    b.Ball(ridge, p, new Vector3(0.09f, 0.07f, 0.09f), 2, 5);
                    dth = Mathf.Clamp(dth + ((float)rng.NextDouble() - 0.5f) * 0.12f, -0.1f, 0.1f);
                    th = Mathf.Clamp(th + dth, 0.05f, 1.45f);
                    ph += dph * (path % 2 == 0 ? 1f : -1f);
                }
            }
            b.Done("BrainCoral", true);
            // A goby who lives in one of its grooves, and a small shoal round the top.
            var goby = Swim(kit, Swimmer.Fish, f + new Vector3(0f, 1.2f, 0f), 0.6f, 0.5f, 20f, 0.25f, new Color(1f, 0.85f, 0.3f), new Color(0.3f, 0.5f, 1f), 0f);
            goby.Bob = 0.08f;
        }

        static void CnPufferfish(LevelKit kit, float[] T)
        {
            Vector3 c = new Vector3(-70f, T[4] + 4f, 104.5f);
            var g = kit.Group("Pufferfish", c);
            var body = kit.Group("Body", c);
            body.SetParent(g, true);
            var b = new RmBits(kit, body);
            b.Ball(RmFelt(new Color(0.85f, 0.78f, 0.5f)), Vector3.zero, new Vector3(0.36f, 0.3f, 0.48f), 4, 9);
            b.Ball(RmFelt(new Color(0.97f, 0.95f, 0.88f)), new Vector3(0f, -0.06f, 0f), new Vector3(0.34f, 0.2f, 0.44f), 3, 8);
            for (int i = 0; i < 14; i++)
                b.Ball(RmFelt(new Color(0.4f, 0.32f, 0.2f)), new Vector3(Mathf.Cos(i * 1.3f) * 0.17f, 0.06f + Mathf.Sin(i * 0.7f) * 0.05f, Mathf.Sin(i * 1.3f) * 0.22f), Vector3.one * 0.04f, 2, 4);
            for (int i = 0; i < 20; i++)
            {
                float a = i * 2.39996f, z = -0.2f + (i / 19f) * 0.4f;
                b.Box(RmFelt(new Color(0.7f, 0.62f, 0.4f)), new Vector3(Mathf.Cos(a) * 0.17f, Mathf.Sin(a) * 0.14f, z), new Vector3(0.012f, 0.012f, 0.05f), new Vector3(Mathf.Sin(a) * 60f, -Mathf.Cos(a) * 60f, 0f));
            }
            foreach (int s in new[] { -1, 1 }) b.Box(RmFelt(new Color(0.9f, 0.8f, 0.55f)), new Vector3(s * 0.18f, 0f, 0.06f), new Vector3(0.08f, 0.06f, 0.012f), new Vector3(0f, s * 60f, 0f));
            b.Box(RmFelt(new Color(0.9f, 0.8f, 0.55f)), new Vector3(0f, 0f, -0.27f), new Vector3(0.02f, 0.14f, 0.1f));
            RmEyes(b, new Vector3(0f, 0.06f, 0.17f), 0.2f, 0.07f);
            b.Box(RmFelt(new Color(0.4f, 0.3f, 0.2f)), new Vector3(0f, -0.02f, 0.24f), new Vector3(0.05f, 0.03f, 0.02f));
            b.Done("Pufferfish");
            var puff = g.gameObject.AddComponent<Puffer>();
            puff.Body = body;
            puff.Every = 13f;
            puff.Swell = 1.8f;
            var swim = g.gameObject.AddComponent<FishSwim>();
            swim.Center = c;
            swim.RadiusX = 1.8f;
            swim.RadiusZ = 1.3f;
            swim.Speed = 8f;
            swim.Bob = 0.3f;
            swim.BobSpeed = 0.5f;
            swim.Wiggle = 4f;
            swim.WiggleSpeed = 2f;
            swim.Bank = 2f;
            rmPlaced++;
        }

        static void CnSlate(LevelKit kit, float[] T)
        {
            Vector3 f;
            if (!RmFloor("the keeper's slate", new Vector3(-68f, T[4], 102.4f), new Vector3(1f, 0.6f, 0.8f), 30f, 7f, out f, 1.5f)) return;
            var g = RmGroup(kit, "KeepersSlate", f, 30f);
            var b = new RmBits(kit, g);
            // A white plastic slate for writing on underwater, left propped on a stone, its pencil
            // tied on with string, and a net bag of coral pieces waiting to be planted.
            b.Ball(RmFelt(new Color(0.5f, 0.5f, 0.52f)), new Vector3(0f, 0.08f, -0.1f), new Vector3(0.36f, 0.18f, 0.28f), 3, 7);
            b.Box(RmFelt(new Color(0.95f, 0.95f, 0.93f)), new Vector3(0f, 0.18f, 0.02f), new Vector3(0.4f, 0.3f, 0.02f), new Vector3(-62f, 0f, 0f));
            var ink = RmFelt(new Color(0.3f, 0.3f, 0.35f));
            for (int i = 0; i < 5; i++) b.Box(ink, new Vector3(-0.04f + (i % 2) * 0.03f, 0.13f + i * 0.025f, 0.02f - i * 0.013f), new Vector3(0.24f - (i % 3) * 0.06f, 0.008f, 0.005f), new Vector3(-62f, 0f, 0f));
            b.Box(RmFelt(new Color(0.95f, 0.8f, 0.2f)), new Vector3(0.24f, 0.02f, 0.12f), new Vector3(0.015f, 0.015f, 0.14f), new Vector3(0f, 40f, 0f));
            b.Box(RmFelt(CreamCol), new Vector3(0.21f, 0.09f, 0.07f), new Vector3(0.005f, 0.17f, 0.005f), new Vector3(30f, 0f, 20f));
            var mesh = RmFelt(new Color(0.85f, 0.4f, 0.2f));
            Vector3 bag = new Vector3(-0.32f, 0f, 0.15f);
            for (int k = 0; k < 4; k++) b.Ring(mesh, bag + Vector3.up * (0.04f + k * 0.06f), new Vector3(90f, 0f, 0f), 0.12f - k * 0.015f, 0.012f, 10);
            Color[] frags = { new Color(1f, 0.5f, 0.6f), new Color(0.6f, 0.8f, 0.4f), new Color(0.95f, 0.75f, 0.3f) };
            for (int k = 0; k < 6; k++) b.Box(RmFelt(frags[k % 3]), bag + new Vector3(Mathf.Cos(k) * 0.05f, 0.12f + (k % 2) * 0.05f, Mathf.Sin(k) * 0.05f), new Vector3(0.025f, 0.12f, 0.025f), new Vector3(k * 20f, 0f, k * 15f));
            b.Done("KeepersSlate", true);
            Mark("Secret_KeepersSlate", f);
        }

        // ================================================================ The Lantern Deep

        static void RoomsLantern(LevelKit kit)
        {
            var T = LanternTank;
            LdGulper(kit, T);
            LdBarreleye(kit, T);
            Shoal(kit, new Vector3(-41.5f, -10f, 118f), 2.6f, 3.6f, 14f, 18, 0.28f, new Color(0.78f, 0.85f, 0.95f), 6111);
            Shoal(kit, new Vector3(-52f, -13f, 108f), 2.2f, 3f, -12f, 14, 0.24f, new Color(0.7f, 0.8f, 0.95f), 6112, 90f);
            rmPlaced++;
            LdSalps(kit, T);
            LdBrittleStars(kit, T);
            LdDropCamera(kit, T);
        }

        static void LdGulper(LevelKit kit, float[] T)
        {
            // A gulper eel: nearly all mouth, a whip of a tail with a pink light at the very end.
            Vector3 c = new Vector3(-46.5f, -15f, 112f);
            var g = kit.Group("GulperEel", c);
            var b = new RmBits(kit, g);
            var skin = RmFelt(new Color(0.12f, 0.1f, 0.14f));
            b.Ball(skin, new Vector3(0f, 0f, 0.15f), new Vector3(0.14f, 0.14f, 0.3f), 3, 7);
            // The jaws, hanging wide open, and the great black pouch between them.
            b.Box(skin, new Vector3(0f, 0.12f, 0.5f), new Vector3(0.08f, 0.04f, 0.6f), new Vector3(-12f, 0f, 0f));
            b.Box(skin, new Vector3(0f, -0.25f, 0.45f), new Vector3(0.08f, 0.04f, 0.6f), new Vector3(30f, 0f, 0f));
            b.Ball(RmFelt(new Color(0.05f, 0.04f, 0.06f)), new Vector3(0f, -0.12f, 0.35f), new Vector3(0.36f, 0.5f, 0.6f), 4, 9);
            b.Ball(RmGlow(new Color(0.7f, 0.9f, 1f), 0.8f), new Vector3(0.05f, 0.11f, 0.22f), Vector3.one * 0.03f, 2, 4);
            b.Ball(RmGlow(new Color(0.7f, 0.9f, 1f), 0.8f), new Vector3(-0.05f, 0.11f, 0.22f), Vector3.one * 0.03f, 2, 4);
            Vector3 prev = Vector3.zero;
            for (int i = 1; i <= 18; i++)
            {
                float t = i / 18f;
                Vector3 p = new Vector3(Mathf.Sin(t * 7f) * 0.25f * t, Mathf.Sin(t * 4f) * 0.08f, -t * 2.6f);
                b.Tube(skin, prev, p, Mathf.Lerp(0.06f, 0.01f, t), 5);
                prev = p;
            }
            b.Done("GulperEel");
            var tip = NoShadow(kit.Deco(PrimitiveType.Sphere, prev, Vector3.one * 0.08f, PatchworkAssets.Unique(RmGlow(new Color(1f, 0.4f, 0.6f), 2.5f)), default(Vector3), g));
            var tw = tip.gameObject.AddComponent<StarTwinkle>();
            tw.Mat = tip.GetComponent<Renderer>().sharedMaterial;
            tw.Tint = new Color(1f, 0.4f, 0.6f);
            tw.Low = 0.6f;
            tw.High = 3f;
            tw.Speed = 2.2f;
            var swim = g.gameObject.AddComponent<FishSwim>();
            swim.Center = c;
            swim.RadiusX = 4.5f;
            swim.RadiusZ = 7.5f;
            swim.Speed = 7f;
            swim.Bob = 1.2f;
            swim.BobSpeed = 0.3f;
            swim.Wiggle = 9f;
            swim.WiggleSpeed = 2.5f;
            swim.Bank = 5f;
            rmPlaced++;
        }

        static void LdBarreleye(LevelKit kit, float[] T)
        {
            // A barreleye: a little fish whose head is a clear dome, with its two green eyes
            // inside, looking straight up for the shadows of things to eat.
            Vector3 c = new Vector3(-50.5f, -19f, 106f);
            var g = kit.Group("Barreleye", c);
            var b = new RmBits(kit, g);
            var body = RmFelt(new Color(0.35f, 0.25f, 0.22f));
            b.Ball(body, Vector3.zero, new Vector3(0.22f, 0.22f, 0.6f), 4, 8);
            b.Box(body, new Vector3(0f, 0f, -0.36f), new Vector3(0.02f, 0.2f, 0.16f));
            foreach (int s in new[] { -1, 1 }) b.Box(RmFelt(new Color(0.5f, 0.4f, 0.35f)), new Vector3(s * 0.14f, -0.03f, 0.05f), new Vector3(0.18f, 0.01f, 0.12f), new Vector3(0f, 0f, s * -25f));
            foreach (int s in new[] { -1, 1 })
            {
                b.Tube(RmGlow(new Color(0.4f, 1f, 0.5f), 2.2f), new Vector3(s * 0.04f, 0.1f, 0.12f), new Vector3(s * 0.04f, 0.2f, 0.12f), 0.03f, 8);
                b.Ball(RmGlow(new Color(0.6f, 1f, 0.7f), 2.6f), new Vector3(s * 0.04f, 0.21f, 0.12f), Vector3.one * 0.06f, 3, 6);
            }
            b.Ball(body, new Vector3(0f, 0f, 0.3f), new Vector3(0.08f, 0.06f, 0.06f), 2, 5);
            b.Done("Barreleye");
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.12f, 0.12f), new Vector3(0.2f, 0.22f, 0.28f), CaseGlass, default(Vector3), g));
            var swim = g.gameObject.AddComponent<FishSwim>();
            swim.Center = c;
            swim.RadiusX = 2f;
            swim.RadiusZ = 2.6f;
            swim.Speed = 9f;
            swim.Bob = 0.4f;
            swim.BobSpeed = 0.5f;
            swim.Wiggle = 5f;
            swim.WiggleSpeed = 3f;
            rmPlaced++;
        }

        static void LdSalps(LevelKit kit, float[] T)
        {
            // A chain of salps: clear little barrels, each its own animal, linked nose to tail and
            // drifting along together, a glowing speck of a stomach in every one.
            Vector3 c = new Vector3(-47f, -5.6f, 104.5f);
            var g = kit.Group("SalpChain", c);
            var barrels = new MeshBatch();
            var guts = new MeshBatch();
            for (int i = 0; i < 14; i++)
            {
                Vector3 p = new Vector3(Mathf.Sin(i * 0.6f) * 0.15f, Mathf.Sin(i * 0.9f) * 0.06f, 1.6f - i * 0.25f);
                barrels.Ball(p, new Vector3(0.16f, 0.16f, 0.24f), 3, 8);
                guts.Ball(p + new Vector3(0f, -0.03f, -0.04f), Vector3.one * 0.04f, 2, 4);
            }
            barrels.Build(g, "Salps", PatchworkAssets.Bubble(new Color(0.75f, 0.9f, 1f, 0.3f)));
            Twinkling(guts, g, "SalpGuts", new Color(1f, 0.6f, 0.35f), 0.8f, 2f, 1.5f, 0f);
            var swim = g.gameObject.AddComponent<FishSwim>();
            swim.Center = c;
            swim.RadiusX = 3.8f;
            swim.RadiusZ = 2.4f;
            swim.Speed = 5f;
            swim.Bob = 0.3f;
            swim.BobSpeed = 0.3f;
            swim.Wiggle = 4f;
            swim.WiggleSpeed = 0.7f;
            swim.Bank = 2f;
            rmPlaced++;
        }

        static void LdBrittleStars(LevelKit kit, float[] T)
        {
            Vector3 f;
            if (!RmFloor("the brittle stars", new Vector3(-42f, T[4], 102.5f), new Vector3(3f, 0.4f, 3f), 0f, 8f, out f, 2f)) return;
            var g = kit.Group("BrittleStars", f);
            var b = new RmBits(kit, g);
            var rng = new System.Random(7101);
            Color[] cols = { new Color(0.9f, 0.75f, 0.75f), new Color(0.75f, 0.72f, 0.8f), new Color(0.95f, 0.85f, 0.7f) };
            for (int s = 0; s < 9; s++)
            {
                Vector3 c = new Vector3(((float)rng.NextDouble() - 0.5f) * 2.6f, 0.04f, ((float)rng.NextDouble() - 0.5f) * 2.6f);
                var m = RmFelt(cols[s % cols.Length]);
                b.Ball(m, c, new Vector3(0.12f, 0.04f, 0.12f), 2, 6);
                float turn = (float)rng.NextDouble() * 360f;
                // Five long thin arms, wriggling out across the mud, one or two held up to feed.
                for (int a = 0; a < 5; a++)
                {
                    float ang = (turn + a * 72f) * Mathf.Deg2Rad;
                    Vector3 dir = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang));
                    Vector3 side = new Vector3(-dir.z, 0f, dir.x);
                    bool up = (a + s) % 4 == 0;
                    Vector3 prev = c;
                    for (int k = 1; k <= 6; k++)
                    {
                        float t = k / 6f;
                        Vector3 p = c + dir * (t * 0.32f) + side * Mathf.Sin(t * 4f + s) * 0.04f + Vector3.up * (up ? t * t * 0.2f : 0f);
                        RmTube(b[m], prev, p, Mathf.Lerp(0.012f, 0.005f, t), 4);
                        prev = p;
                    }
                }
            }
            b.Done("BrittleStars");
        }

        static void LdDropCamera(LevelKit kit, float[] T)
        {
            Vector3 f;
            if (!RmFloor("the drop camera", new Vector3(-52f, T[4], 121f), new Vector3(1.6f, 2.4f, 1.6f), 200f, 8f, out f, 2f)) return;
            var g = RmGroup(kit, "DropCamera", f, 200f);
            var b = new RmBits(kit, g);
            var alu = RmShine(new Color(0.75f, 0.77f, 0.8f), 0.8f);
            // A tripod frame let down on a rope from the catwalk, with a camera, a lamp and a bait bag,
            // to see who comes to visit in the dark.
            for (int i = 0; i < 3; i++)
            {
                float a = i * 2.094f;
                b.Tube(alu, new Vector3(Mathf.Cos(a) * 0.65f, 0f, Mathf.Sin(a) * 0.65f), new Vector3(0f, 1.5f, 0f), 0.025f, 6);
                b.Ball(alu, new Vector3(Mathf.Cos(a) * 0.65f, 0.03f, Mathf.Sin(a) * 0.65f), new Vector3(0.12f, 0.05f, 0.12f), 2, 6);
            }
            b.Box(alu, new Vector3(0f, 1.52f, 0f), new Vector3(0.2f, 0.06f, 0.2f));
            var housing = RmShine(new Color(0.9f, 0.55f, 0.15f), 0.6f);
            b.Tube(housing, new Vector3(0f, 1.1f, -0.15f), new Vector3(0f, 1.1f, 0.25f), 0.1f, 10);
            b.Disc(RmShine(new Color(0.1f, 0.12f, 0.2f), 0.97f), new Vector3(0f, 1.1f, 0.26f), 0.16f, 0.01f);
            b.Tube(alu, new Vector3(0.2f, 1.25f, -0.05f), new Vector3(0.2f, 1.25f, 0.18f), 0.06f, 8);
            b.Disc(RmGlow(new Color(1f, 0.95f, 0.8f), 3f), new Vector3(0.2f, 1.25f, 0.19f), 0.1f, 0.01f);
            // The bait bag, hanging below on a short arm.
            b.Tube(alu, new Vector3(0f, 0.9f, 0f), new Vector3(0f, 0.5f, 0.55f), 0.015f, 5);
            var bag = RmFelt(new Color(0.85f, 0.6f, 0.3f));
            for (int k = 0; k < 4; k++) b.Ring(bag, new Vector3(0f, 0.35f + k * 0.06f, 0.55f), new Vector3(90f, 0f, 0f), 0.08f - Mathf.Abs(k - 1.5f) * 0.02f, 0.01f, 8);
            b.Ball(RmFelt(new Color(0.75f, 0.78f, 0.82f)), new Vector3(0f, 0.43f, 0.55f), new Vector3(0.08f, 0.05f, 0.12f), 2, 6);
            // Its rope, all the way up to the surface, and the keepers' tag.
            b.Box(PatchworkAssets.Yarn(new Color(0.95f, 0.85f, 0.3f)), new Vector3(0f, 1.55f + (WaterTop - f.y - 1.55f) * 0.5f, 0f), new Vector3(0.025f, WaterTop - f.y - 1.55f, 0.025f));
            b.Box(RmFelt(new Color(0.95f, 0.95f, 0.9f)), new Vector3(0.12f, 1.35f, 0.0f), new Vector3(0.01f, 0.08f, 0.16f));
            b.Done("DropCamera", true);
            var led = NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(-0.07f, 1.22f, 0.1f), Vector3.one * 0.03f, PatchworkAssets.Unique(RmGlow(new Color(1f, 0.15f, 0.1f), 3f)), default(Vector3), g));
            var tw = led.gameObject.AddComponent<StarTwinkle>();
            tw.Mat = led.GetComponent<Renderer>().sharedMaterial;
            tw.Tint = new Color(1f, 0.15f, 0.1f);
            tw.Low = 0f;
            tw.High = 4f;
            tw.Speed = 4f;
            SpotLight(kit, g.TransformPoint(new Vector3(0.2f, 1.25f, 0.2f)), g.TransformPoint(new Vector3(0f, 0f, 2.4f)), new Color(1f, 0.95f, 0.85f), 9f, 50f, 2.2f, false, false);
            Mark("Secret_DropCamera", f);
        }

        // ================================================================ The Frozen Sea

        static void RoomsFrozen(LevelKit kit)
        {
            var T = FrozenTank;
            FzIcefish(kit, T);
            FzSeaButterflies(kit, T);
            FzTern(kit, T);
            FzIceDrill(kit, T);
            FzIceAlgae(kit, T);
            FzSnowman(kit, T);
        }

        static void FzIcefish(LevelKit kit, float[] T)
        {
            // Icefish: their blood has no red in it at all. It's clear, like the water they swim in.
            for (int n = 0; n < 2; n++)
            {
                Vector3 c = new Vector3(-74f + n * 3f, T[4] + 3.5f + n * 1.2f, 165f - n * 6f);
                var g = kit.Group("Icefish", c);
                var b = new RmBits(kit, g);
                var skin = PatchworkAssets.Bubble(new Color(0.9f, 0.95f, 1f, 0.55f));
                b.Ball(skin, Vector3.zero, new Vector3(0.2f, 0.2f, 0.8f), 4, 8);
                b.Box(skin, new Vector3(0f, -0.02f, 0.5f), new Vector3(0.12f, 0.06f, 0.3f));
                b.Box(skin, new Vector3(0f, -0.07f, 0.48f), new Vector3(0.11f, 0.03f, 0.28f), new Vector3(6f, 0f, 0f));
                b.Box(skin, new Vector3(0f, 0.12f, -0.05f), new Vector3(0.015f, 0.14f, 0.36f));
                b.Box(skin, new Vector3(0f, 0f, -0.45f), new Vector3(0.015f, 0.2f, 0.12f));
                foreach (int s in new[] { -1, 1 }) b.Box(skin, new Vector3(s * 0.12f, -0.04f, 0.12f), new Vector3(0.2f, 0.01f, 0.12f), new Vector3(0f, 0f, s * -15f));
                b.Ball(RmFelt(new Color(0.85f, 0.88f, 0.92f)), new Vector3(0f, -0.02f, 0f), new Vector3(0.06f, 0.08f, 0.5f), 3, 6);
                RmEyes(b, new Vector3(0f, 0.04f, 0.36f), 0.14f, 0.06f);
                b.Done("Icefish");
                var swim = g.gameObject.AddComponent<FishSwim>();
                swim.Center = c;
                swim.RadiusX = 2.6f;
                swim.RadiusZ = 3.2f;
                swim.Speed = n == 0 ? 9f : -8f;
                swim.Angle = n * 150f;
                swim.Bob = 0.3f;
                swim.BobSpeed = 0.4f;
                swim.Wiggle = 5f;
                swim.WiggleSpeed = 2.4f;
            }
            rmPlaced++;
        }

        static void FzSeaButterflies(LevelKit kit, float[] T)
        {
            // Sea butterflies: snails no bigger than a lentil, who swim by flapping two little wings.
            var rng = new System.Random(4477);
            var shell = PatchworkAssets.Bubble(new Color(0.85f, 0.9f, 1f, 0.5f));
            var wing = RmGlow(new Color(0.75f, 0.85f, 1f), 0.6f);
            int made = 0;
            for (int i = 0; i < 40 && made < 14; i++)
            {
                Vector3 p = new Vector3(-80f + (float)rng.NextDouble() * 12f, -5.5f + (float)rng.NextDouble() * 3.5f, 140f + (float)rng.NextDouble() * 20f);
                if (!RmTry(p, new Vector3(0.3f, 0.2f, 0.2f), 0f)) continue;
                var g = kit.Group("SeaButterfly", p);
                var b = new RmBits(kit, g);
                b.Ball(shell, Vector3.zero, new Vector3(0.06f, 0.09f, 0.06f), 3, 6);
                b.Ball(RmFelt(new Color(0.6f, 0.4f, 0.55f)), new Vector3(0f, -0.02f, 0f), Vector3.one * 0.03f, 2, 4);
                b.Done("SeaButterfly");
                Transform[] wings = new Transform[2];
                for (int s = 0; s < 2; s++)
                {
                    var hinge = kit.Group("Wing", p + Vector3.up * 0.02f);
                    hinge.SetParent(g, true);
                    hinge.localRotation = Quaternion.identity;
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s == 0 ? -0.07f : 0.07f, 0f, 0f), new Vector3(0.12f, 0.012f, 0.08f), wing, default(Vector3), hinge));
                    wings[s] = hinge;
                }
                var flap = g.gameObject.AddComponent<WingFlap>();
                flap.Left = wings[0];
                flap.Right = wings[1];
                flap.Angle = 45f;
                flap.Speed = 9f + (float)rng.NextDouble() * 4f;
                flap.Phase = i * 0.7f;
                var bob = g.gameObject.AddComponent<SeaBob>();
                bob.Height = 0.25f;
                bob.Rock = 12f;
                bob.Speed = 0.9f + (float)rng.NextDouble() * 0.6f;
                bob.Phase = i;
                made++;
            }
            if (made > 0) rmPlaced++; else RmMiss("the sea butterflies", new Vector3(-74f, -4f, 150f));
        }

        static void FzTern(LevelKit kit, float[] T)
        {
            // An Arctic tern: it flies from the top of the world to the bottom and back every year,
            // the longest journey of any animal there is. It's stopped here for a rest. Nearly.
            Vector3 c = new Vector3((T[0] + T[1]) * 0.5f, 4.6f, (T[2] + T[3]) * 0.5f);
            var g = kit.Group("ArcticTern", c);
            var b = new RmBits(kit, g);
            var white = RmFelt(new Color(0.97f, 0.97f, 0.97f));
            var grey = RmFelt(new Color(0.78f, 0.8f, 0.85f));
            var black = RmFelt(new Color(0.08f, 0.08f, 0.1f));
            b.Ball(white, Vector3.zero, new Vector3(0.16f, 0.15f, 0.42f), 4, 8);
            b.Ball(white, new Vector3(0f, 0.05f, 0.2f), Vector3.one * 0.12f, 3, 7);
            b.Ball(black, new Vector3(0f, 0.09f, 0.2f), new Vector3(0.125f, 0.07f, 0.12f), 3, 7);
            b.Box(RmFelt(new Color(0.9f, 0.2f, 0.15f)), new Vector3(0f, 0.04f, 0.32f), new Vector3(0.025f, 0.025f, 0.12f));
            foreach (int s in new[] { -1, 1 }) b.Box(white, new Vector3(s * 0.04f, 0f, -0.3f), new Vector3(0.02f, 0.01f, 0.22f), new Vector3(0f, s * 14f, 0f));
            b.Done("ArcticTern");
            var wings = new Transform[2];
            for (int s = 0; s < 2; s++)
            {
                var hinge = kit.Group("Wing", c + Vector3.up * 0.04f);
                hinge.SetParent(g, true);
                hinge.localRotation = Quaternion.identity;
                float side = s == 0 ? -1f : 1f;
                var wb = new RmBits(kit, hinge);
                wb.Box(grey, new Vector3(side * 0.28f, 0f, 0f), new Vector3(0.56f, 0.015f, 0.14f), new Vector3(0f, side * -8f, 0f));
                wb.Box(black, new Vector3(side * 0.56f, 0f, -0.02f), new Vector3(0.1f, 0.016f, 0.1f), new Vector3(0f, side * -8f, 0f));
                wb.Done("TernWing");
                wings[s] = hinge;
            }
            var flap = g.gameObject.AddComponent<WingFlap>();
            flap.Left = wings[0];
            flap.Right = wings[1];
            flap.Angle = 32f;
            flap.Speed = 8f;
            var swim = g.gameObject.AddComponent<FishSwim>();
            swim.Center = c;
            swim.RadiusX = 6f;
            swim.RadiusZ = 11f;
            swim.Speed = 18f;
            swim.Bob = 0.6f;
            swim.BobSpeed = 0.7f;
            swim.Wiggle = 2f;
            swim.WiggleSpeed = 1f;
            swim.Bank = 22f;
            rmPlaced++;
        }

        static void FzIceDrill(LevelKit kit, float[] T)
        {
            Vector3 f;
            if (!RmFloor("the ice drill", new Vector3(-82.5f, 0.25f, 142f), new Vector3(1.4f, 2.4f, 1.4f), 30f, 3f, out f, 0.4f)) return;
            var g = RmGroup(kit, "IceDrill", f, 30f);
            var b = new RmBits(kit, g);
            // A hand drill standing in its hole, the ice core it brought up laid out on the snow,
            // and a flag so the next keeper can find the spot.
            b.Disc(RmShine(new Color(0.2f, 0.35f, 0.5f), 0.95f), new Vector3(0f, 0.14f, 0f), 0.16f, 0.01f).localRotation = Quaternion.identity;
            var steel = RmShine(new Color(0.75f, 0.77f, 0.8f), 0.85f);
            b.Tube(steel, new Vector3(0f, 0.1f, 0f), new Vector3(0f, 1.3f, 0f), 0.025f, 6);
            for (int k = 0; k < 10; k++)
            {
                float y = 0.15f + k * 0.07f;
                b.Box(steel, new Vector3(0f, y, 0f), new Vector3(0.14f, 0.01f, 0.03f), new Vector3(-12f, k * 36f, 0f));
            }
            b.Tube(steel, new Vector3(-0.3f, 1.3f, 0f), new Vector3(0.3f, 1.3f, 0f), 0.022f, 6);
            foreach (int s in new[] { -1, 1 }) b.Tube(RmFelt(new Color(0.85f, 0.25f, 0.2f)), new Vector3(s * 0.2f, 1.3f, 0f), new Vector3(s * 0.32f, 1.3f, 0f), 0.035f, 6);
            var core = RmGlow(new Color(0.8f, 0.92f, 1f), 0.4f);
            var dust = RmFelt(new Color(0.65f, 0.7f, 0.75f));
            for (int i = 0; i < 6; i++)
                b.Tube(i % 2 == 0 ? core : dust, new Vector3(0.35f + i * 0.1f, 0.18f, 0.35f), new Vector3(0.44f + i * 0.1f, 0.18f, 0.35f), 0.05f, 8);
            b.Box(DarkWood, new Vector3(-0.5f, 0.75f, -0.3f), new Vector3(0.03f, 1.5f, 0.03f));
            b.Box(RmFelt(new Color(1f, 0.5f, 0.1f)), new Vector3(-0.5f, 1.32f, -0.12f), new Vector3(0.01f, 0.24f, 0.36f), new Vector3(0f, 0f, 3f));
            b.Cyl(RmShine(new Color(0.25f, 0.4f, 0.6f), 0.7f), new Vector3(0.3f, 0.12f, -0.3f), 0.06f, 0.26f, 10);
            b.Done("IceDrill", true);
            RmSolid(kit, g, new Vector3(0f, 0.7f, 0f), new Vector3(0.2f, 1.2f, 0.2f));
        }

        static void FzIceAlgae(LevelKit kit, float[] T)
        {
            // A brown-green fuzz of algae growing on the underside of every floe: it's what the
            // krill graze on, and so, in the end, what everything else in this tank lives on.
            float[][] floes = { new[] { T[0], -77f, 162f, T[3] }, new[] { -73f, -67f, 149f, 155f }, new[] { -85f, -80f, 139f, 145f }, new[] { -71f, -64f, 165f, 170f }, new[] { -78f, -75f, 136f, 139f } };
            var mb = new MeshBatch();
            var pale = new MeshBatch();
            var rng = new System.Random(9071);
            foreach (var fl in floes)
            {
                float area = (fl[1] - fl[0]) * (fl[3] - fl[2]);
                for (int i = 0; i < area * 0.8f; i++)
                {
                    Vector3 p = new Vector3(fl[0] + 0.4f + (float)rng.NextDouble() * (fl[1] - fl[0] - 0.8f), -1.34f, fl[2] + 0.4f + (float)rng.NextDouble() * (fl[3] - fl[2] - 0.8f));
                    float s = 0.3f + (float)rng.NextDouble() * 0.5f;
                    (i % 4 == 0 ? pale : mb).Ball(p, new Vector3(s, 0.06f, s * 0.8f), 2, 6);
                }
            }
            mb.Build(kit.Root, "IceAlgae", RmFelt(new Color(0.45f, 0.42f, 0.22f)));
            pale.Build(kit.Root, "IceAlgae", RmFelt(new Color(0.6f, 0.58f, 0.32f)));
            rmPlaced++;
        }

        static void FzSnowman(LevelKit kit, float[] T)
        {
            Vector3 f;
            if (!RmFloor("Pippa's snowman", new Vector3(-67f, 0.25f, 167.5f), new Vector3(1f, 1.9f, 1f), 200f, 3f, out f, 0.4f)) return;
            var g = RmGroup(kit, "Snowman", f, 200f);
            var b = new RmBits(kit, g);
            var snow = RmFelt(new Color(0.97f, 0.98f, 1f));
            b.Ball(snow, new Vector3(0f, 0.36f, 0f), new Vector3(0.8f, 0.72f, 0.8f), 5, 10);
            b.Ball(snow, new Vector3(0f, 0.95f, 0f), new Vector3(0.58f, 0.54f, 0.58f), 5, 10);
            b.Ball(snow, new Vector3(0f, 1.42f, 0f), new Vector3(0.42f, 0.4f, 0.42f), 4, 10);
            var coal = RmShine(new Color(0.08f, 0.08f, 0.1f), 0.5f);
            foreach (int s in new[] { -1, 1 }) b.Ball(coal, new Vector3(s * 0.08f, 1.48f, 0.19f), Vector3.one * 0.05f, 2, 5);
            for (int i = 0; i < 3; i++) b.Ball(coal, new Vector3(0f, 0.8f + i * 0.13f, 0.28f - Mathf.Abs(i - 1) * 0.02f), Vector3.one * 0.05f, 2, 5);
            for (int i = 0; i < 5; i++) b.Ball(coal, new Vector3((i - 2) * 0.05f, 1.36f - Mathf.Abs(i - 2) * -0.01f, 0.19f), Vector3.one * 0.03f, 2, 4);
            b.Tube(RmFelt(new Color(1f, 0.5f, 0.15f)), new Vector3(0f, 1.43f, 0.2f), new Vector3(0f, 1.41f, 0.38f), 0.03f, 6);
            // A knitted scarf with the ends flying, a bobble hat, and two twig arms.
            var scarf = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.85f, 0.25f, 0.3f), CreamCol);
            b.Ring(scarf, new Vector3(0f, 1.2f, 0f), new Vector3(90f, 0f, 0f), 0.22f, 0.08f, 14);
            b.Box(scarf, new Vector3(0.12f, 1.02f, 0.22f), new Vector3(0.1f, 0.34f, 0.03f), new Vector3(10f, 0f, 14f));
            var hat = RmFelt(new Color(0.25f, 0.45f, 0.75f));
            b.Ball(hat, new Vector3(0f, 1.58f, 0f), new Vector3(0.36f, 0.24f, 0.36f), 3, 8);
            b.Ring(RmFelt(CreamCol), new Vector3(0f, 1.55f, 0f), new Vector3(90f, 0f, 0f), 0.18f, 0.05f, 12);
            b.Ball(RmFelt(CreamCol), new Vector3(0f, 1.73f, 0f), Vector3.one * 0.1f, 2, 6);
            var twig = PatchworkAssets.Planks(new Color(0.4f, 0.28f, 0.18f));
            foreach (int s in new[] { -1, 1 })
            {
                b.Tube(twig, new Vector3(s * 0.25f, 1.0f, 0f), new Vector3(s * 0.62f, 1.22f, 0.05f), 0.015f, 5);
                b.Tube(twig, new Vector3(s * 0.5f, 1.15f, 0.04f), new Vector3(s * 0.58f, 1.3f, 0.06f), 0.01f, 4);
            }
            b.Done("Snowman", true);
            RmSolid(kit, g, new Vector3(0f, 0.8f, 0f), new Vector3(0.7f, 1.6f, 0.7f));
            Mark("Story_PippasSnowman", f);
        }

        // ================================================================ The Mangrove

        static void RoomsMangrove(LevelKit kit)
        {
            var T = MangroveTank;
            MgKingfisher(kit, T);
            MgPropagules(kit, T);
            MgMudLobster(kit, T);
            MgSpiderWeb(kit, T);
            MgClimbingCrabs(kit, T);
            MgCrocodile(kit, T);
        }

        static void MgKingfisher(LevelKit kit, float[] T)
        {
            Vector3 f;
            if (!RmFloor("the kingfisher's snag", new Vector3(-46.5f, T[4], 157f), new Vector3(0.6f, 8.6f, 0.6f), 0f, 6f, out f, 1.5f)) return;
            var g = kit.Group("KingfisherSnag", f);
            var b = new RmBits(kit, g);
            var wood = PatchworkAssets.Planks(new Color(0.32f, 0.26f, 0.22f));
            float top = 1.3f - f.y;
            // A dead branch sticking up out of the water: the best fishing perch in the tank.
            b.Tube(wood, Vector3.zero, new Vector3(0.05f, top, 0.02f), 0.06f, 7);
            b.Tube(wood, new Vector3(0.04f, top - 0.35f, 0.01f), new Vector3(0.32f, top + 0.05f, 0.05f), 0.03f, 6);
            b.Tube(wood, new Vector3(0.05f, top, 0.02f), new Vector3(-0.08f, top + 0.25f, 0f), 0.025f, 5);
            for (int i = 0; i < 5; i++) b.Ball(RmFelt(new Color(0.85f, 0.82f, 0.75f)), new Vector3(Mathf.Sin(i) * 0.06f, (WaterTop - f.y) + 0.05f + i * 0.08f, Mathf.Cos(i) * 0.06f), new Vector3(0.06f, 0.04f, 0.06f), 2, 5);
            b.Done("KingfisherSnag", true);
            // The kingfisher: electric blue on top, orange underneath, a dagger of a beak.
            var bird = kit.Group("Kingfisher", g.TransformPoint(new Vector3(0.28f, top + 0.06f, 0.05f)));
            bird.SetParent(g, true);
            bird.localRotation = Quaternion.Euler(0f, 210f, 0f);
            var kb = new RmBits(kit, bird);
            var blue = RmGlow(new Color(0.1f, 0.6f, 0.9f), 0.25f);
            var orange = RmFelt(new Color(1f, 0.55f, 0.2f));
            kb.Ball(orange, new Vector3(0f, 0.1f, 0f), new Vector3(0.13f, 0.16f, 0.15f), 3, 7);
            kb.Ball(blue, new Vector3(0f, 0.13f, -0.03f), new Vector3(0.135f, 0.13f, 0.17f), 3, 7);
            kb.Box(blue, new Vector3(0f, 0.1f, -0.14f), new Vector3(0.05f, 0.02f, 0.12f), new Vector3(-30f, 0f, 0f));
            foreach (int s in new[] { -1, 1 }) kb.Box(RmFelt(new Color(1f, 0.45f, 0.2f)), new Vector3(s * 0.025f, 0.01f, 0f), new Vector3(0.015f, 0.04f, 0.02f));
            kb.Done("Kingfisher", true);
            var head = kit.Group("Head", bird.TransformPoint(new Vector3(0f, 0.22f, 0.03f)));
            head.SetParent(bird, true);
            head.localRotation = Quaternion.identity;
            var hb = new RmBits(kit, head);
            hb.Ball(blue, Vector3.zero, Vector3.one * 0.1f, 3, 7);
            hb.Ball(orange, new Vector3(0f, -0.02f, 0.02f), new Vector3(0.105f, 0.05f, 0.08f), 2, 6);
            hb.Box(RmFelt(Color.white), new Vector3(0f, -0.025f, 0.03f), new Vector3(0.106f, 0.02f, 0.03f));
            hb.Box(RmShine(new Color(0.1f, 0.1f, 0.1f), 0.6f), new Vector3(0f, -0.005f, 0.11f), new Vector3(0.022f, 0.022f, 0.13f));
            RmEyes(hb, new Vector3(0f, 0.015f, 0.035f), 0.085f, 0.018f);
            hb.Done("KingfisherHead", true);
            var watch = head.gameObject.AddComponent<WatchPlayer>();
            watch.Range = 9f;
            watch.MaxTurn = 70f;
        }

        static void MgPropagules(LevelKit kit, float[] T)
        {
            // Mangrove seeds start growing while they're still on the tree, then drop off as long
            // green spears and float, for months if need be, until they wash up somewhere muddy.
            var rng = new System.Random(6601);
            var green = RmFelt(new Color(0.35f, 0.55f, 0.25f));
            var brown = RmFelt(new Color(0.45f, 0.35f, 0.2f));
            int afloat = 0, planted = 0;
            for (int i = 0; i < 60 && afloat < 8; i++)
            {
                Vector3 p = new Vector3(T[0] + 2f + (float)rng.NextDouble() * (T[1] - T[0] - 4f), WaterTop + 0.02f, 141f + (float)rng.NextDouble() * 24f);
                float yaw = (float)rng.NextDouble() * 360f;
                if (!RmTry(p, new Vector3(0.12f, 0.12f, 0.6f), yaw)) continue;
                var g = RmGroup(kit, "Propagule", p, yaw);
                var b = new RmBits(kit, g);
                b.Tube(green, new Vector3(0f, 0f, -0.28f), new Vector3(0f, 0.02f, 0.24f), 0.025f, 6);
                b.Ball(brown, new Vector3(0f, 0.02f, 0.27f), new Vector3(0.05f, 0.05f, 0.06f), 2, 5);
                b.Done("Propagule");
                var bob = g.gameObject.AddComponent<SeaBob>();
                bob.Height = 0.03f;
                bob.Rock = 4f;
                bob.Speed = 0.6f + (float)rng.NextDouble() * 0.4f;
                bob.Phase = i;
                afloat++;
            }
            // And a few that have landed and taken root on the banks: a spear in the mud, two leaves on top.
            float[][] banks = { new[] { T[0] + 0.6f, -51f, 167f, T[3] - 0.6f }, new[] { -41f, T[1] - 0.6f, T[2] + 0.6f, 140f } };
            for (int i = 0; i < 40 && planted < 7; i++)
            {
                var bk = banks[i % 2];
                Vector3 p = new Vector3(bk[0] + (float)rng.NextDouble() * (bk[1] - bk[0]), 2f, bk[2] + (float)rng.NextDouble() * (bk[3] - bk[2]));
                RaycastHit hit;
                if (!Physics.Raycast(p, Vector3.down, out hit, 3f, RmMask, QueryTriggerInteraction.Ignore) || hit.point.y < 0f || hit.point.y > 0.6f) continue;
                if (!RmTry(hit.point + Vector3.up * 0.3f, new Vector3(0.25f, 0.5f, 0.25f), 0f)) continue;
                var g = RmGroup(kit, "Seedling", hit.point, (float)rng.NextDouble() * 360f);
                var b = new RmBits(kit, g);
                b.Tube(green, new Vector3(0f, -0.05f, 0f), new Vector3(0.02f, 0.4f, 0f), 0.018f, 6);
                foreach (int s in new[] { -1, 1 }) b.Ball(RmFelt(new Color(0.3f, 0.6f, 0.3f)), new Vector3(s * 0.06f, 0.43f, 0f), new Vector3(0.12f, 0.02f, 0.06f), 2, 5);
                b.Done("Seedling", true);
                planted++;
            }
            if (afloat + planted > 0) rmPlaced++; else RmMiss("the mangrove seedlings", new Vector3(-46f, WaterTop, 152f));
        }

        static void MgMudLobster(LevelKit kit, float[] T)
        {
            Vector3 f;
            if (!RmFloor("the mud lobster's mound", new Vector3(-55f, 0.3f, 173f), new Vector3(1.4f, 1.2f, 1.4f), 0f, 4f, out f, 0.5f)) return;
            var g = kit.Group("MudLobsterMound", f);
            var b = new RmBits(kit, g);
            var mud = RmFelt(new Color(0.32f, 0.25f, 0.18f));
            var wet = RmShine(new Color(0.26f, 0.2f, 0.15f), 0.7f);
            // A chimney of mud, dug up and piled by a mud lobster who lives at the bottom of it.
            for (int i = 0; i < 6; i++)
                b.Ball(i % 2 == 0 ? mud : wet, new Vector3(Mathf.Sin(i * 1.3f) * 0.05f, 0.12f + i * 0.15f, Mathf.Cos(i * 1.3f) * 0.05f), new Vector3(0.9f - i * 0.11f, 0.28f, 0.85f - i * 0.1f), 3, 8);
            b.Cyl(RmShine(new Color(0.05f, 0.04f, 0.03f), 0.5f), new Vector3(0f, 0.92f, 0f), 0.1f, 0.03f, 10);
            for (int i = 0; i < 8; i++) b.Ball(mud, new Vector3(Mathf.Cos(i * 0.8f) * 0.6f, 0.04f, Mathf.Sin(i * 0.8f) * 0.55f), new Vector3(0.14f, 0.07f, 0.12f), 2, 5);
            // Its claw, just showing at the top.
            b.Ball(RmFelt(new Color(0.55f, 0.35f, 0.25f)), new Vector3(0.03f, 0.95f, 0.02f), new Vector3(0.06f, 0.04f, 0.1f), 2, 5);
            b.Done("MudLobsterMound", true);
            RmSolid(kit, g, new Vector3(0f, 0.45f, 0f), new Vector3(0.6f, 0.9f, 0.6f));
        }

        static void MgSpiderWeb(LevelKit kit, float[] T)
        {
            // Strung between the far tree and the edge of the tank, beaded with drops that catch the light.
            // (Up above the arching roots, where there's room to spin.)
            foreach (var hub in new[] { new Vector3(-37.3f, 3.5f, 137.5f), new Vector3(-37.3f, 3.1f, 136.3f), new Vector3(-37.3f, 3.1f, 138.7f), new Vector3(-37.3f, 4f, 137.5f) })
            {
                if (!RmTry(hub, new Vector3(1.9f, 1.8f, 0.06f), 0f)) continue;
                var g = kit.Group("SpiderWeb", hub);
                var silk = new MeshBatch();
                var dew = new MeshBatch();
                const int spokes = 14;
                for (int i = 0; i < spokes; i++)
                {
                    float a = i / (float)spokes * Mathf.PI * 2f;
                    Vector3 rim = new Vector3(Mathf.Cos(a) * 1f, Mathf.Sin(a) * 0.9f, 0f);
                    RmTube(silk, Vector3.zero, rim, 0.004f, 3);
                }
                for (int ring = 1; ring <= 9; ring++)
                    for (int i = 0; i < spokes; i++)
                    {
                        float a0 = i / (float)spokes * Mathf.PI * 2f, a1 = (i + 1) / (float)spokes * Mathf.PI * 2f, r = ring * 0.1f;
                        Vector3 p0 = new Vector3(Mathf.Cos(a0) * r, Mathf.Sin(a0) * r * 0.9f, 0f), p1 = new Vector3(Mathf.Cos(a1) * r, Mathf.Sin(a1) * r * 0.9f, 0f);
                        RmTube(silk, p0, p1, 0.003f, 3);
                        if ((i + ring) % 3 == 0) dew.Ball((p0 + p1) * 0.5f, Vector3.one * 0.025f, 2, 4);
                    }
                // Anchor threads out to the trunk and the wall.
                RmTube(silk, new Vector3(-1f, 0f, 0f), new Vector3(-1.3f, 0.1f, 0f), 0.004f, 3);
                RmTube(silk, new Vector3(1f, 0f, 0f), new Vector3(1.4f, -1.5f, 0f), 0.004f, 3);
                silk.Build(g, "SpiderWeb", RmGlow(new Color(0.85f, 0.9f, 0.95f), 0.5f));
                Twinkling(dew, g, "WebDew", new Color(0.8f, 0.95f, 1f), 0.6f, 2f, 0.8f, 0f);
                var b = new RmBits(kit, g);
                b.Ball(RmFelt(new Color(0.85f, 0.7f, 0.2f)), new Vector3(0f, 0f, 0.02f), new Vector3(0.08f, 0.1f, 0.05f), 3, 6);
                b.Ball(RmFelt(new Color(0.2f, 0.2f, 0.2f)), new Vector3(0f, 0.07f, 0.02f), Vector3.one * 0.05f, 2, 5);
                for (int k = 0; k < 4; k++)
                    foreach (int s in new[] { -1, 1 })
                        b.Box(RmFelt(new Color(0.2f, 0.2f, 0.2f)), new Vector3(s * 0.07f, 0.04f - k * 0.03f, 0.02f), new Vector3(0.1f, 0.008f, 0.008f), new Vector3(0f, 0f, s * (30f - k * 20f)));
                b.Done("Spider");
                rmPlaced++;
                return;
            }
            RmMiss("the spider's web", new Vector3(-37.3f, 3.5f, 137.5f));
        }

        static void MgClimbingCrabs(LevelKit kit, float[] T)
        {
            // Little crabs that live up the trees, scuttling up when the tide comes in and down
            // again when it goes out.
            Vector3[] trunks = { new Vector3(-53.5f, 0f, 170.5f), new Vector3(-44f, 0f, 163f), new Vector3(-39f, 0f, 137.5f), new Vector3(-51f, 0f, 152.5f) };
            var shell = RmFelt(new Color(0.55f, 0.3f, 0.35f));
            var legs = RmFelt(new Color(0.45f, 0.4f, 0.3f));
            int n = 0;
            for (int t = 0; t < trunks.Length; t++)
                for (int k = 0; k < 2; k++)
                {
                    float a = (t * 97f + k * 160f) * Mathf.Deg2Rad;
                    Vector3 outward = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                    // (On the bark itself, above where the prop roots spring out: no need to ask
                    // for room, they're meant to be touching it.)
                    float y0 = 1.6f + k * 0.7f, y1 = y0 + 1.1f;
                    Vector3 low = trunks[t] + outward * 0.43f + Vector3.up * y0;
                    var g = RmGroup(kit, "TreeCrab", low, Quaternion.LookRotation(Vector3.up, outward));
                    var b = new RmBits(kit, g);
                    b.Ball(shell, new Vector3(0f, 0.02f, 0f), new Vector3(0.16f, 0.06f, 0.14f), 3, 7);
                    foreach (int s in new[] { -1, 1 })
                    {
                        for (int l = 0; l < 3; l++) b.Box(legs, new Vector3(s * 0.1f, 0f, -0.04f + l * 0.04f), new Vector3(0.1f, 0.012f, 0.012f), new Vector3(0f, s * (l - 1) * 20f, 0f));
                        b.Ball(RmFelt(new Color(0.85f, 0.45f, 0.35f)), new Vector3(s * 0.05f, 0.02f, 0.1f), new Vector3(0.04f, 0.03f, 0.05f), 2, 5);
                    }
                    RmEyes(b, new Vector3(0f, 0.05f, 0.06f), 0.05f, 0.018f);
                    b.Done("TreeCrab");
                    var climb = g.gameObject.AddComponent<Glider>();
                    climb.A = low;
                    climb.B = trunks[t] + outward * 0.43f + Vector3.up * y1;
                    climb.Travel = 6f + k * 2f;
                    climb.Rest = 3f;
                    climb.Offset = t * 2.3f + k * 4f;
                    n++;
                }
            if (n > 0) rmPlaced++; else RmMiss("the tree-climbing crabs", trunks[0]);
        }

        static void MgCrocodile(LevelKit kit, float[] T)
        {
            // A crocodile, very still, hoping you'll take it for a log. (It's a soft toy one.)
            foreach (var want in new[] { new Vector3(-48f, WaterTop - 0.18f, 158.5f), new Vector3(-41f, WaterTop - 0.18f, 150f), new Vector3(-52f, WaterTop - 0.18f, 141.5f) })
            {
                if (!RmTry(want, new Vector3(3.4f, 0.5f, 1f), 35f)) continue;
                var g = RmGroup(kit, "Crocodile", want, 35f + 90f);
                var b = new RmBits(kit, g);
                var hide = RmFelt(new Color(0.3f, 0.36f, 0.24f));
                var belly = RmFelt(new Color(0.75f, 0.72f, 0.5f));
                // Nose at +z: snout, head, a long body and tail, almost all of it under the water.
                b.Ball(hide, new Vector3(0f, -0.08f, 0f), new Vector3(0.6f, 0.32f, 1.6f), 4, 10);
                b.Ball(belly, new Vector3(0f, -0.18f, 0f), new Vector3(0.55f, 0.2f, 1.5f), 3, 9);
                b.Ball(hide, new Vector3(0f, -0.04f, 0.95f), new Vector3(0.36f, 0.2f, 0.5f), 3, 8);
                b.Box(hide, new Vector3(0f, -0.04f, 1.35f), new Vector3(0.22f, 0.1f, 0.5f));
                for (int i = 0; i < 8; i++)
                {
                    float t = i / 7f;
                    b.Ball(hide, new Vector3(Mathf.Sin(t * 3f) * 0.15f * t, -0.1f, -0.7f - t * 1.2f), new Vector3(0.36f - t * 0.28f, 0.2f - t * 0.12f, 0.3f), 3, 7);
                }
                // The bits showing above the water: the bumps down its back, its eyes, its nostrils.
                for (int i = 0; i < 9; i++) b.Ball(hide, new Vector3((i % 2 == 0 ? 0.1f : -0.1f), 0.08f, 0.5f - i * 0.2f), new Vector3(0.08f, 0.06f, 0.1f), 2, 5);
                foreach (int s in new[] { -1, 1 })
                {
                    b.Ball(hide, new Vector3(s * 0.1f, 0.1f, 0.95f), new Vector3(0.1f, 0.1f, 0.12f), 3, 6);
                    b.Ball(RmGlow(new Color(1f, 0.8f, 0.2f), 1.2f), new Vector3(s * 0.1f, 0.15f, 1f), new Vector3(0.05f, 0.03f, 0.05f), 2, 5);
                    b.Ball(RmShine(new Color(0.05f, 0.05f, 0.05f), 0.8f), new Vector3(s * 0.1f, 0.165f, 1.02f), new Vector3(0.012f, 0.012f, 0.03f), 2, 4);
                    b.Ball(hide, new Vector3(s * 0.04f, 0.06f, 1.55f), Vector3.one * 0.05f, 2, 5);
                }
                foreach (int s in new[] { -1, 1 }) foreach (float z in new[] { 0.5f, -0.5f }) b.Ball(hide, new Vector3(s * 0.34f, -0.22f, z), new Vector3(0.14f, 0.1f, 0.3f), 2, 6);
                b.Done("Crocodile");
                var bob = g.gameObject.AddComponent<SeaBob>();
                bob.Height = 0.02f;
                bob.Rock = 1f;
                bob.Speed = 0.3f;
                rmPlaced++;
                return;
            }
            RmMiss("the crocodile", new Vector3(-48f, WaterTop, 158.5f));
        }
    }
}

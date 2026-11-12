using UnityEngine;

namespace Patchwork
{
    // Six more things in each of the front rooms (see Level2Rooms for how they're fitted in):
    //   The Forecourt        - bicycles at the rack, lobster pots, a chalk hopscotch, a viewing
    //                          telescope, the tide board, and the harbour bell
    //   The Grand Atrium     - an umbrella stand, a rack of maps, the whale's baleen, a whale's
    //                          earplug, a mobile of paper whales, and a satchel left on a bench
    //   The Hall of Deep Time- a children's dig, amber, a Marshall Islands stick chart, an
    //                          Egyptian water clock, Megalodon's jaws, and a coprolite
    //   The Kelp Galleries   - the kelp growth chart, the otters' urchins, a garibaldi and his
    //                          garden, a leafy sea dragon, a diver's air pump, and a sea-glass window
    public static partial class Level2Builder
    {
        // ================================================================ The Forecourt

        static void RoomsForecourt(LevelKit kit)
        {
            FcBicycles(kit);
            FcLobsterPots(kit);
            FcHopscotch(kit);
            FcViewer(kit);
            FcTideBoard(kit);
            FcHarbourBell(kit);
        }

        // A bicycle, length along z, wheels on the ground, in group `g` at `at`.
        static void RmBicycle(LevelKit kit, Transform parent, Vector3 at, float lean, Color paint, bool basket)
        {
            var g = kit.Group("Bicycle", parent.TransformPoint(at));
            g.SetParent(parent, true);
            g.localRotation = Quaternion.Euler(0f, 0f, lean);
            var b = new RmBits(kit, g);
            var frame = RmShine(paint, 0.65f);
            var tyre = RmShine(new Color(0.08f, 0.08f, 0.1f), 0.3f);
            var steel = RmShine(new Color(0.75f, 0.76f, 0.8f), 0.85f);
            Vector3 rear = new Vector3(0f, 0.33f, -0.52f), front = new Vector3(0f, 0.33f, 0.52f);
            Vector3 crank = new Vector3(0f, 0.3f, -0.05f), seat = new Vector3(0f, 0.78f, -0.2f), head = new Vector3(0f, 0.8f, 0.36f), bar = new Vector3(0f, 0.97f, 0.33f);
            foreach (var hub in new[] { rear, front })
            {
                b.Ring(tyre, hub, new Vector3(0f, 90f, 0f), 0.31f, 0.045f, 20);
                b.Ring(steel, hub, new Vector3(0f, 90f, 0f), 0.27f, 0.02f, 20);
                for (int s = 0; s < 6; s++)
                    b.Box(steel, hub, new Vector3(0.006f, 0.54f, 0.006f), new Vector3(s * 30f, 0f, 0f));
                b.Ball(steel, hub, new Vector3(0.07f, 0.05f, 0.05f), 2, 6);
            }
            b.Tube(frame, rear, crank, 0.018f, 6);
            b.Tube(frame, rear, seat, 0.016f, 6);
            b.Tube(frame, crank, seat + Vector3.down * 0.04f, 0.022f, 6);
            b.Tube(frame, crank, head, 0.024f, 6);
            b.Tube(frame, seat + Vector3.down * 0.06f, head, 0.022f, 6);
            b.Tube(frame, head, front, 0.02f, 6);
            b.Tube(steel, head, bar, 0.016f, 6);
            b.Tube(steel, bar + new Vector3(-0.26f, 0f, -0.04f), bar + new Vector3(0.26f, 0f, -0.04f), 0.014f, 6);
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(tyre, bar + new Vector3(s * 0.24f, 0f, -0.06f), new Vector3(0.1f, 0.035f, 0.035f));
                b.Box(steel, crank + new Vector3(s * 0.06f, -s * 0.12f, s * 0.04f), new Vector3(0.012f, 0.24f, 0.02f), new Vector3(s * 20f, 0f, 0f));
                b.Box(tyre, crank + new Vector3(s * 0.11f, -s * 0.24f, s * 0.08f), new Vector3(0.09f, 0.02f, 0.05f));
            }
            b.Ring(steel, crank + Vector3.right * 0.03f, new Vector3(0f, 90f, 0f), 0.09f, 0.02f, 12);
            b.Box(RmShine(new Color(0.2f, 0.12f, 0.1f), 0.5f), seat + Vector3.up * 0.04f, new Vector3(0.13f, 0.05f, 0.25f));
            b.Box(frame, rear + new Vector3(0f, 0.36f, 0.02f), new Vector3(0.12f, 0.012f, 0.42f), new Vector3(-8f, 0f, 0f));   // the mudguard
            b.Ball(Brass, bar + new Vector3(0.16f, 0.03f, -0.02f), Vector3.one * 0.05f, 3, 6);   // the bell
            b.Ball(RmGlow(new Color(1f, 0.92f, 0.7f), 1.4f), head + new Vector3(0f, -0.05f, 0.07f), new Vector3(0.07f, 0.07f, 0.05f), 3, 6);
            if (basket)
            {
                var wicker = PatchworkAssets.Wicker(new Color(0.8f, 0.62f, 0.38f));
                b.Box(wicker, new Vector3(0f, 0.85f, 0.56f), new Vector3(0.32f, 0.2f, 0.26f));
                Color[] petals = { new Color(1f, 0.55f, 0.65f), new Color(1f, 0.85f, 0.35f), new Color(0.75f, 0.6f, 1f), Color.white };
                for (int i = 0; i < 7; i++)
                {
                    Vector3 p = new Vector3(-0.1f + (i % 4) * 0.07f, 0.98f + (i % 2) * 0.04f, 0.5f + (i / 4) * 0.1f);
                    b.Ball(RmFelt(petals[i % petals.Length]), p, Vector3.one * 0.07f, 3, 6);
                    b.Box(RmFelt(new Color(0.3f, 0.55f, 0.3f)), p + new Vector3(0.02f, -0.03f, 0.02f), new Vector3(0.05f, 0.012f, 0.03f), new Vector3(0f, i * 40f, 0f));
                }
            }
            b.Done("Bicycle", true);
        }

        static void FcBicycles(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the bicycle rack", new Vector3(-14f, 0f, -111.6f), new Vector3(3.4f, 1.3f, 1.9f), 180f, 7f, out f)) return;
            var g = RmGroup(kit, "BicycleRack", f, 180f);
            var b = new RmBits(kit, g);
            // Four iron hoops, set into a stone kerb.
            b.Box(PaleStone, new Vector3(0f, 0.05f, 0.25f), new Vector3(3.2f, 0.1f, 0.3f));
            for (int i = 0; i < 4; i++)
            {
                float x = -1.2f + i * 0.8f;
                b.Ring(IronMat, new Vector3(x, 0.45f, 0.25f), new Vector3(0f, 90f, 0f), 0.3f, 0.04f, 12, 0f, 180f);
                b.Box(IronMat, new Vector3(x, 0.24f, -0.05f), new Vector3(0.04f, 0.42f, 0.04f));
                b.Box(IronMat, new Vector3(x, 0.24f, 0.55f), new Vector3(0.04f, 0.42f, 0.04f));
            }
            b.Done("BicycleRack", true);
            RmBicycle(kit, g, new Vector3(-0.8f, 0f, 0.15f), 5f, new Color(0.2f, 0.55f, 0.6f), true);
            RmBicycle(kit, g, new Vector3(0.8f, 0f, 0.05f), -6f, new Color(0.75f, 0.22f, 0.25f), false);
            RmSolid(kit, g, new Vector3(0f, 0.55f, 0.1f), new Vector3(3.2f, 1.1f, 1.5f));
        }

        // A lobster pot: a flat wooden base with net hooped over it, and a funnel in each end.
        static void RmCreel(RmBits b, Vector3 at, float yaw)
        {
            var q = Quaternion.Euler(0f, yaw, 0f);
            var wood = PatchworkAssets.Planks(new Color(0.55f, 0.42f, 0.3f));
            var hoop = RmShine(new Color(0.22f, 0.18f, 0.16f), 0.4f);
            var net = RmFelt(new Color(0.55f, 0.65f, 0.55f));
            b.Box(wood, at + q * new Vector3(0f, 0.03f, 0f), new Vector3(0.62f, 0.06f, 0.9f), new Vector3(0f, yaw, 0f));
            foreach (float z in new[] { -0.38f, 0f, 0.38f })
                b.Ring(hoop, at + q * new Vector3(0f, 0.06f, z), new Vector3(0f, yaw, 0f), 0.3f, 0.025f, 10, 0f, 180f);
            for (int k = 1; k < 8; k++)
            {
                float a = k / 8f * Mathf.PI;
                b.Box(net, at + q * new Vector3(Mathf.Cos(a) * 0.3f, 0.06f + Mathf.Sin(a) * 0.3f, 0f), new Vector3(0.012f, 0.012f, 0.78f), new Vector3(0f, yaw, 0f));
            }
            foreach (int s in new[] { -1, 1 })
                b.Ball(net, at + q * new Vector3(0f, 0.2f, s * 0.36f), new Vector3(0.34f, 0.26f, 0.06f), 3, 8);
        }

        static void FcLobsterPots(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the lobster pots", new Vector3(52.5f, 0f, -121f), new Vector3(2.6f, 1.4f, 2.2f), 270f, 6f, out f)) return;
            var g = RmGroup(kit, "LobsterPots", f, 270f);
            var b = new RmBits(kit, g);
            RmCreel(b, new Vector3(-0.45f, 0f, 0.1f), 0f);
            RmCreel(b, new Vector3(0.3f, 0f, -0.1f), 8f);
            RmCreel(b, new Vector3(-0.1f, 0.37f, 0f), -5f);
            // A coil of rope and a striped float.
            var rope = PatchworkAssets.Yarn(new Color(0.78f, 0.66f, 0.45f));
            for (int i = 0; i < 4; i++) b.Ring(rope, new Vector3(0.95f, 0.04f + i * 0.05f, 0.6f), new Vector3(90f, 0f, 0f), 0.26f - i * 0.02f, 0.05f, 14);
            b.Ball(RmFelt(new Color(0.95f, 0.45f, 0.2f)), new Vector3(0.9f, 0.2f, -0.65f), new Vector3(0.32f, 0.36f, 0.32f), 4, 8);
            b.Box(RmFelt(CreamCol), new Vector3(0.9f, 0.2f, -0.65f), new Vector3(0.33f, 0.08f, 0.33f));
            b.Box(DarkWood, new Vector3(0.9f, 0.5f, -0.65f), new Vector3(0.04f, 0.4f, 0.04f));
            // A crab, who has found the pot quite comfortable, thank you.
            var shell = RmFelt(new Color(0.85f, 0.35f, 0.25f));
            Vector3 crab = new Vector3(0.3f, 0.12f, -0.1f);
            b.Ball(shell, crab, new Vector3(0.22f, 0.08f, 0.17f), 3, 7);
            foreach (int s in new[] { -1, 1 })
            {
                b.Ball(shell, crab + new Vector3(s * 0.13f, 0.01f, 0.1f), new Vector3(0.08f, 0.05f, 0.1f), 2, 5);
                for (int l = 0; l < 3; l++) b.Box(shell, crab + new Vector3(s * 0.12f, -0.04f, -0.05f + l * 0.05f), new Vector3(0.11f, 0.015f, 0.015f), new Vector3(0f, 0f, s * -25f));
            }
            RmEyes(b, crab + new Vector3(0f, 0.06f, 0.07f), 0.06f, 0.025f);
            b.Done("LobsterPots", true);
            RmSolid(kit, g, new Vector3(0f, 0.4f, 0f), new Vector3(2.2f, 0.8f, 1.8f));
        }

        static void FcHopscotch(LevelKit kit)
        {
            Vector3 f;
            if (!RmFlat("the hopscotch", new Vector3(-13f, 0f, -141f), new Vector3(1.8f, 0.02f, 5.4f), 0f, 7f, out f)) return;
            var g = RmGroup(kit, "Hopscotch", f, 0f);
            var b = new RmBits(kit, g);
            var chalk = RmShine(new Color(0.95f, 0.95f, 0.9f), 0.05f);
            // Squares: one, two, three; four and five; six; seven and eight; then home.
            float[][] cells = { new[] { 0f, 0f }, new[] { 0f, 1f }, new[] { 0f, 2f }, new[] { -0.4f, 3f }, new[] { 0.4f, 3f }, new[] { 0f, 4f }, new[] { -0.4f, 5f }, new[] { 0.4f, 5f } };
            const float s = 0.8f, y = 0.006f;
            for (int i = 0; i < cells.Length; i++)
            {
                Vector3 c = new Vector3(cells[i][0], y, -2.3f + cells[i][1] * s);
                foreach (int k in new[] { -1, 1 })
                {
                    b.Box(chalk, c + new Vector3(k * s * 0.5f, 0f, 0f), new Vector3(0.04f, 0.01f, s));
                    b.Box(chalk, c + new Vector3(0f, 0f, k * s * 0.5f), new Vector3(s, 0.01f, 0.04f));
                }
                // The number as dots, since it's for little ones.
                for (int d = 0; d <= i; d++)
                    b.Ball(chalk, c + new Vector3(((d % 4) - 1.5f) * 0.12f, 0f, (d / 4 - 0.5f) * 0.14f), new Vector3(0.07f, 0.01f, 0.07f), 2, 6);
            }
            b.Ring(chalk, new Vector3(0f, y, -2.3f + 5.5f * s), new Vector3(90f, 0f, 0f), 0.55f, 0.04f, 12, 0f, 180f);
            // A pebble on number three, and the chalk left lying by it.
            b.Ball(RmFelt(new Color(0.6f, 0.6f, 0.66f)), new Vector3(0.1f, 0.05f, -2.3f + 2f * s), new Vector3(0.14f, 0.08f, 0.11f), 3, 6);
            b.Box(RmFelt(new Color(1f, 0.6f, 0.7f)), new Vector3(0.75f, 0.03f, -2.5f), new Vector3(0.05f, 0.05f, 0.2f), new Vector3(0f, 30f, 0f));
            b.Box(RmFelt(new Color(0.5f, 0.75f, 1f)), new Vector3(0.82f, 0.03f, -2.32f), new Vector3(0.05f, 0.05f, 0.16f), new Vector3(0f, -12f, 0f));
            // And a chalk star at the top, for whoever gets home.
            for (int k = 0; k < 5; k++)
                b.Box(RmShine(new Color(1f, 0.86f, 0.4f), 0.05f), new Vector3(0f, y, 2.55f), new Vector3(0.04f, 0.01f, 0.5f), new Vector3(0f, k * 36f, 0f));
            b.Done("Hopscotch");
        }

        static void FcViewer(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the viewing telescope", new Vector3(14f, 0f, -160.2f), new Vector3(1f, 1.8f, 1f), 180f, 5f, out f)) return;
            var g = RmGroup(kit, "ViewingTelescope", f, 180f);
            var b = new RmBits(kit, g);
            var paint = RmShine(new Color(0.22f, 0.45f, 0.42f), 0.7f);
            b.Cyl(paint, Vector3.zero, 0.3f, 0.12f, 12);
            b.Cyl(paint, Vector3.up * 0.12f, 0.1f, 1.0f, 10);
            b.Cyl(Brass, Vector3.up * 1.08f, 0.16f, 0.06f, 12);
            // The head, tipped up a little towards the lighthouse out at sea.
            var head = kit.Group("Head", g.TransformPoint(new Vector3(0f, 1.35f, 0f)));
            head.SetParent(g, true);
            head.localRotation = Quaternion.Euler(-8f, 0f, 0f);
            var hb = new RmBits(kit, head);
            hb.Box(paint, Vector3.zero, new Vector3(0.52f, 0.36f, 0.5f));
            hb.Box(paint, new Vector3(0f, 0.2f, 0f), new Vector3(0.44f, 0.06f, 0.44f));
            foreach (int s in new[] { -1, 1 })
            {
                hb.Tube(Brass, new Vector3(s * 0.12f, 0.04f, 0.25f), new Vector3(s * 0.12f, 0.04f, 0.46f), 0.09f, 10);
                hb.Disc(RmGlow(new Color(0.55f, 0.75f, 1f), 0.8f), new Vector3(s * 0.12f, 0.04f, 0.47f), 0.15f, 0.01f);
                hb.Tube(RmShine(new Color(0.1f, 0.1f, 0.12f), 0.4f), new Vector3(s * 0.1f, 0.04f, -0.25f), new Vector3(s * 0.1f, 0.04f, -0.34f), 0.05f, 8);
                hb.Box(paint, new Vector3(s * 0.33f, -0.05f, -0.1f), new Vector3(0.14f, 0.05f, 0.05f));
            }
            hb.Box(Brass, new Vector3(0.18f, -0.06f, -0.26f), new Vector3(0.08f, 0.12f, 0.03f));
            hb.Box(RmShine(new Color(0.05f, 0.05f, 0.05f), 0.5f), new Vector3(0.18f, -0.03f, -0.28f), new Vector3(0.04f, 0.008f, 0.01f));
            hb.Done("ViewingTelescope", true);
            b.Done("ViewingTelescope", true);
            RmSolid(kit, g, new Vector3(0f, 0.8f, 0f), new Vector3(0.4f, 1.6f, 0.4f));
        }

        static void FcTideBoard(LevelKit kit)
        {
            Vector3 f;
            // (It stands in the sea just past the railings, the way a real one does.)
            if (!RmFloor("the tide board", new Vector3(46f, -2.7f, -163.5f), new Vector3(0.6f, 4.4f, 0.3f), 0f, 4f, out f)) return;
            var g = RmGroup(kit, "TideBoard", f, 0f);
            var b = new RmBits(kit, g);
            b.Box(DarkWood, new Vector3(0f, 2.1f, -0.1f), new Vector3(0.14f, 4.2f, 0.14f));
            b.Box(RmFelt(new Color(0.95f, 0.95f, 0.92f)), new Vector3(0f, 2.2f, 0.01f), new Vector3(0.42f, 3.9f, 0.04f));
            var black = RmFelt(new Color(0.08f, 0.08f, 0.1f));
            var red = RmFelt(new Color(0.8f, 0.15f, 0.15f));
            for (int i = 0; i <= 38; i++)
            {
                float y = 0.3f + i * 0.1f;
                bool metre = i % 10 == 0, half = i % 5 == 0;
                b.Box(metre ? red : black, new Vector3(metre ? 0f : -0.08f, y, 0.035f), new Vector3(metre ? 0.4f : half ? 0.24f : 0.12f, 0.03f, 0.01f));
            }
            // Weed and barnacles round its foot, where the water is.
            for (int i = 0; i < 10; i++)
                b.Ball(RmFelt(i % 3 == 0 ? new Color(0.8f, 0.8f, 0.75f) : new Color(0.2f, 0.4f, 0.25f)), new Vector3(Mathf.Sin(i * 2.3f) * 0.12f, 0.1f + i * 0.06f, 0.04f + Mathf.Cos(i * 1.7f) * 0.06f), new Vector3(0.08f, 0.07f, 0.05f), 2, 5);
            // A little lamp on top, so the boats can read it at night.
            b.Box(IronMat, new Vector3(0f, 4.25f, -0.1f), new Vector3(0.2f, 0.06f, 0.2f));
            b.Ball(RmGlow(new Color(1f, 0.55f, 0.3f), 2.2f), new Vector3(0f, 4.38f, -0.1f), new Vector3(0.14f, 0.18f, 0.14f), 3, 6);
            b.Done("TideBoard", true);
        }

        static void FcHarbourBell(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the harbour bell", new Vector3(-52f, 0f, -151f), new Vector3(2f, 3.2f, 1.2f), 90f, 6f, out f)) return;
            var g = RmGroup(kit, "HarbourBell", f, 90f);
            var b = new RmBits(kit, g);
            var wood = PatchworkAssets.Planks(new Color(0.36f, 0.25f, 0.18f));
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(wood, new Vector3(s * 0.8f, 1.4f, 0f), new Vector3(0.2f, 2.8f, 0.2f));
                b.Box(wood, new Vector3(s * 0.8f, 0.1f, 0f), new Vector3(0.36f, 0.2f, 0.9f));
                b.Box(wood, new Vector3(s * 0.62f, 2.55f, 0f), new Vector3(0.36f, 0.08f, 0.1f), new Vector3(0f, 0f, s * 40f));
            }
            b.Box(wood, new Vector3(0f, 2.86f, 0f), new Vector3(1.9f, 0.22f, 0.24f));
            b.Box(Brass, new Vector3(0f, 3f, 0f), new Vector3(1.94f, 0.04f, 0.26f));
            b.Done("BellFrame", true);
            // The bell, swinging very slightly in the sea breeze.
            var pivot = kit.Group("BellPivot", g.TransformPoint(new Vector3(0f, 2.74f, 0f)));
            pivot.SetParent(g, true);
            pivot.localRotation = Quaternion.identity;
            var bb = new RmBits(kit, pivot);
            var bronze = RmShine(new Color(0.78f, 0.58f, 0.3f), 0.85f);
            bb.Box(IronMat, new Vector3(0f, -0.04f, 0f), new Vector3(0.14f, 0.12f, 0.14f));
            bb.Ball(bronze, new Vector3(0f, -0.3f, 0f), new Vector3(0.5f, 0.56f, 0.5f), 5, 12);
            bb.Cyl(bronze, new Vector3(0f, -0.66f, 0f), 0.32f, 0.12f, 14);
            bb.Ring(bronze, new Vector3(0f, -0.62f, 0f), new Vector3(90f, 0f, 0f), 0.33f, 0.06f, 16);
            bb.Ball(IronMat, new Vector3(0f, -0.78f, 0f), Vector3.one * 0.1f, 3, 6);
            var rope = PatchworkAssets.Yarn(CreamCol);
            bb.Box(rope, new Vector3(0f, -1.15f, 0f), new Vector3(0.03f, 0.75f, 0.03f));
            bb.Ball(rope, new Vector3(0f, -1.55f, 0f), new Vector3(0.09f, 0.14f, 0.09f), 3, 6);
            bb.Done("HarbourBell", true);
            var swing = pivot.gameObject.AddComponent<PendulumSwing>();
            swing.Angle = 4f;
            swing.Period = 3.6f;
            swing.Creep = 0f;
            foreach (int s in new[] { -1, 1 }) RmSolid(kit, g, new Vector3(s * 0.8f, 1.4f, 0f), new Vector3(0.3f, 2.8f, 0.3f));
        }

        // ================================================================ The Grand Atrium

        static void RoomsAtrium(LevelKit kit)
        {
            AtUmbrellas(kit);
            AtMapRack(kit);
            AtBaleen(kit);
            AtEarplug(kit);
            AtPaperWhales(kit);
            AtSatchel(kit);
        }

        static void AtUmbrellas(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the umbrella stand", new Vector3(7.6f, 0f, -106.6f), new Vector3(0.8f, 1.3f, 0.8f), 0f, 4f, out f)) return;
            var g = RmGroup(kit, "UmbrellaStand", f, 0f);
            var b = new RmBits(kit, g);
            b.Cyl(Brass, Vector3.zero, 0.24f, 0.62f, 14);
            b.Ring(Brass, new Vector3(0f, 0.6f, 0f), new Vector3(90f, 0f, 0f), 0.25f, 0.04f, 16);
            b.Ring(Brass, new Vector3(0f, 0.08f, 0f), new Vector3(90f, 0f, 0f), 0.25f, 0.04f, 16);
            b.Cyl(RmFelt(Navy), new Vector3(0f, 0.6f, 0f), 0.2f, 0.01f, 12);
            Color[] cloth = { Plum, new Color(0.95f, 0.8f, 0.3f), new Color(0.2f, 0.45f, 0.4f) };
            for (int i = 0; i < 3; i++)
            {
                float a = i * 2.1f;
                Vector3 baseP = new Vector3(Mathf.Cos(a) * 0.09f, 0.3f, Mathf.Sin(a) * 0.09f);
                Vector3 lean = new Vector3(Mathf.Cos(a) * 0.12f, 0f, Mathf.Sin(a) * 0.12f);
                Vector3 tip = baseP + Vector3.up * 0.78f + lean;
                b.Tube(RmFelt(cloth[i]), baseP + Vector3.up * 0.2f + lean * 0.25f, tip, 0.07f, 8);
                b.Ball(RmFelt(cloth[i]), baseP + Vector3.up * 0.52f + lean * 0.6f, new Vector3(0.16f, 0.36f, 0.16f), 3, 8);
                b.Tube(DarkWood, tip, tip + Vector3.up * 0.18f + lean * 0.15f, 0.018f, 6);
                // The crook of the handle.
                Vector3 h = tip + Vector3.up * 0.18f + lean * 0.15f;
                b.Ring(i == 1 ? Brass : DarkWood, h + new Vector3(lean.x, 0f, lean.z).normalized * 0.06f, new Vector3(0f, -a * Mathf.Rad2Deg, 0f), 0.06f, 0.03f, 8, 0f, 200f);
            }
            b.Done("UmbrellaStand", true);
            RmSolid(kit, g, new Vector3(0f, 0.4f, 0f), new Vector3(0.5f, 0.8f, 0.5f));
        }

        static void AtMapRack(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the rack of maps", new Vector3(-7.6f, 0f, -106.6f), new Vector3(1f, 1.7f, 0.7f), 0f, 4f, out f)) return;
            var g = RmGroup(kit, "MapRack", f, 0f);
            var b = new RmBits(kit, g);
            foreach (int s in new[] { -1, 1 }) b.Box(DarkWood, new Vector3(s * 0.42f, 0.75f, -0.1f), new Vector3(0.06f, 1.5f, 0.3f), new Vector3(-8f, 0f, 0f));
            b.Box(DarkWood, new Vector3(0f, 0.05f, 0f), new Vector3(0.95f, 0.1f, 0.45f));
            Color[] covers = { Navy, Wine, Teal, Plum, new Color(0.75f, 0.55f, 0.25f) };
            for (int row = 0; row < 3; row++)
            {
                float y = 0.45f + row * 0.38f, z = 0.02f - row * 0.05f;
                b.Box(Brass, new Vector3(0f, y - 0.14f, z + 0.08f), new Vector3(0.84f, 0.03f, 0.05f));
                for (int k = 0; k < 4; k++)
                {
                    var cover = RmFelt(covers[(row * 4 + k) % covers.Length]);
                    b.Box(cover, new Vector3(-0.3f + k * 0.2f, y, z + 0.04f - k * 0.01f), new Vector3(0.17f, 0.3f, 0.02f), new Vector3(-14f, 0f, 0f));
                    b.Box(RmGlow(new Color(1f, 0.82f, 0.4f), 0.6f), new Vector3(-0.3f + k * 0.2f, y + 0.05f, z + 0.055f - k * 0.01f), new Vector3(0.1f, 0.02f, 0.005f), new Vector3(-14f, 0f, 0f));
                }
            }
            // A little brass sign on top.
            b.Box(Brass, new Vector3(0f, 1.6f, -0.12f), new Vector3(0.7f, 0.2f, 0.03f));
            Letters(b[RmFelt(Navy)], "MAPS", new Vector3(0f, 1.6f, -0.1f), Vector3.forward, 0.022f, 0.01f);
            b.Done("MapRack", true);
            RmSolid(kit, g, new Vector3(0f, 0.8f, 0f), new Vector3(0.9f, 1.6f, 0.5f));
        }

        static void AtBaleen(LevelKit kit)
        {
            Transform g;
            if (!RmExhibit(kit, "the baleen", new Vector3(-14f, 0f, -84f), 0f, 6f, new Vector3(1.9f, 2.8f, 1f), out g, "Baleen",
                "A blue whale has no teeth at all. Instead, hundreds of plates of baleen hang down from the roof of its mouth, made of keratin, the same stuff as your fingernails.",
                "It takes a huge gulp of sea, pushes the water out again through the bristly fringe on the baleen, and swallows what's caught: krill, by the tonne. On a good day's feeding, as much as four tonnes of them.")) return;
            Vector3 inside = RmCase(kit, g, new Vector3(1.9f, 2.8f, 1f), false);
            var b = new RmBits(kit, g);
            var gum = RmFelt(new Color(0.78f, 0.6f, 0.58f));
            var plate = RmFelt(new Color(0.22f, 0.2f, 0.22f));
            var fringe = RmFelt(new Color(0.72f, 0.68f, 0.6f));
            Vector3 top = inside + Vector3.up * 1.65f;
            b.Box(gum, top, new Vector3(1.6f, 0.14f, 0.3f));
            for (int i = 0; i < 18; i++)
            {
                float x = -0.72f + i * 0.085f;
                float len = 1.2f + Mathf.Sin(i / 17f * Mathf.PI) * 0.3f;
                b.Box(plate, top + new Vector3(x, -len * 0.5f, 0f), new Vector3(0.02f, len, 0.24f), new Vector3(0f, 0f, 3f));
                for (int k = 0; k < 4; k++)
                    b.Box(fringe, top + new Vector3(x + 0.01f, -len - 0.04f - k * 0.03f, -0.08f + k * 0.05f), new Vector3(0.008f, 0.12f, 0.008f));
            }
            // A dish of krill on the felt, to show what it's for.
            b.Cyl(RmShine(new Color(0.85f, 0.88f, 0.9f), 0.9f), inside + new Vector3(0f, 0f, 0.15f), 0.2f, 0.04f, 12);
            for (int i = 0; i < 12; i++)
                b.Box(RmFelt(new Color(1f, 0.45f, 0.4f)), inside + new Vector3(Mathf.Sin(i * 2.4f) * 0.12f, 0.055f, 0.15f + Mathf.Cos(i * 1.9f) * 0.1f), new Vector3(0.05f, 0.015f, 0.015f), new Vector3(0f, i * 47f, 0f));
            b.Done("Baleen", true);
            PointLight(kit, g.TransformPoint(new Vector3(0f, 2.6f, 0.6f)), new Color(1f, 0.9f, 0.75f), 4f, 0.8f);
        }

        static void AtEarplug(LevelKit kit)
        {
            Transform g;
            if (!RmExhibit(kit, "the whale's earplug", new Vector3(14f, 0f, -72f), 0f, 6f, new Vector3(1.2f, 2f, 0.9f), out g, "The Whale's Earplug",
                "A big whale's ear canal fills up, little by little, with a plug of wax: a layer every season of its life, light and dark, light and dark, like the rings in a tree.",
                "Count the layers and you know how old the whale was. Scientists have even read what was in the sea round it, year by year, in the wax. Nobody ever has to clean a whale's ears.")) return;
            Vector3 inside = RmCase(kit, g, new Vector3(1.2f, 2f, 0.9f), false);
            var b = new RmBits(kit, g);
            b.Box(Brass, inside + new Vector3(0f, 0.06f, 0f), new Vector3(0.06f, 0.12f, 0.06f));
            var light = RmFelt(new Color(0.9f, 0.78f, 0.55f));
            var dark = RmFelt(new Color(0.45f, 0.32f, 0.2f));
            // Lying along a little brass cradle, banded all the way down.
            for (int i = 0; i < 22; i++)
            {
                float t = i / 21f, r = Mathf.Lerp(0.07f, 0.02f, t);
                b.Ball(i % 2 == 0 ? light : dark, inside + new Vector3(-0.38f + t * 0.76f, 0.2f + Mathf.Sin(t * Mathf.PI) * 0.04f, 0f), new Vector3(0.05f, r * 2f, r * 2f), 3, 7);
            }
            b.Box(Brass, inside + new Vector3(-0.2f, 0.14f, 0f), new Vector3(0.03f, 0.06f, 0.14f));
            b.Box(Brass, inside + new Vector3(0.2f, 0.14f, 0f), new Vector3(0.03f, 0.06f, 0.14f));
            b.Box(RmFelt(CreamCol), inside + new Vector3(0f, 0.02f, 0.28f), new Vector3(0.36f, 0.01f, 0.14f), new Vector3(0f, 6f, 0f));
            b.Done("Earplug", true);
        }

        static void AtPaperWhales(LevelKit kit)
        {
            // Hung from the underside of the west gallery, over the walk along the wall.
            Vector3 want = new Vector3(-27.4f, 5.2f, -88f);
            RaycastHit hit;
            float ceiling = 7.4f;
            if (Physics.Raycast(want, Vector3.up, out hit, 4f, RmMask, QueryTriggerInteraction.Ignore)) ceiling = hit.point.y;
            Vector3 c;
            if (!RmAir("the paper whales", new Vector3(want.x, (ceiling + 4.4f) * 0.5f, want.z), new Vector3(2.2f, ceiling - 4.4f, 2.2f), 0f, 6f, out c, 0f)) return;
            var g = RmGroup(kit, "PaperWhales", new Vector3(c.x, ceiling, c.z), 0f);
            var b = new RmBits(kit, g);
            b.Box(Brass, new Vector3(0f, -0.04f, 0f), new Vector3(0.16f, 0.08f, 0.16f));
            b.Box(PatchworkAssets.Yarn(CreamCol), new Vector3(0f, -0.4f, 0f), new Vector3(0.015f, 0.7f, 0.015f));
            b.Done("MobileHook");
            var spin = kit.Group("Mobile", g.TransformPoint(new Vector3(0f, -0.75f, 0f)));
            spin.SetParent(g, true);
            var mb = new RmBits(kit, spin);
            mb.Ring(Brass, Vector3.zero, new Vector3(90f, 0f, 0f), 0.9f, 0.03f, 20);
            mb.Box(Brass, Vector3.zero, new Vector3(1.8f, 0.02f, 0.02f));
            mb.Box(Brass, Vector3.zero, new Vector3(0.02f, 0.02f, 1.8f));
            Color[] papers = { new Color(0.95f, 0.93f, 0.86f), new Color(0.6f, 0.75f, 0.95f), new Color(0.98f, 0.8f, 0.55f), new Color(0.8f, 0.7f, 0.95f), new Color(0.65f, 0.9f, 0.85f) };
            for (int i = 0; i < 5; i++)
            {
                float a = i / 5f * Mathf.PI * 2f;
                float drop = 0.35f + (i % 3) * 0.3f;
                Vector3 at = new Vector3(Mathf.Cos(a) * 0.9f, 0f, Mathf.Sin(a) * 0.9f);
                mb.Box(PatchworkAssets.Yarn(CreamCol), at + Vector3.down * drop * 0.5f, new Vector3(0.01f, drop, 0.01f));
                // A folded paper whale: a body of two creased halves, a tail, and a fin.
                var paper = RmFelt(papers[i]);
                Vector3 w = at + Vector3.down * (drop + 0.12f);
                float yaw = -a * Mathf.Rad2Deg;
                var q = Quaternion.Euler(0f, yaw, 0f);
                mb.Box(paper, w + q * new Vector3(0f, 0.04f, 0.04f), new Vector3(0.14f, 0.12f, 0.42f), new Vector3(0f, yaw, 22f));
                mb.Box(paper, w + q * new Vector3(0f, 0.04f, -0.04f), new Vector3(0.14f, 0.12f, 0.42f), new Vector3(0f, yaw, -22f));
                mb.Box(paper, w + q * new Vector3(0f, 0.1f, -0.26f), new Vector3(0.24f, 0.02f, 0.1f), new Vector3(20f, yaw, 0f));
                mb.Box(paper, w + q * new Vector3(0f, 0.12f, 0.06f), new Vector3(0.02f, 0.08f, 0.08f), new Vector3(0f, yaw, 0f));
                mb.Ball(RmShine(new Color(0.1f, 0.1f, 0.15f), 0.8f), w + q * new Vector3(0.06f, 0.05f, 0.16f), Vector3.one * 0.025f, 2, 4);
            }
            mb.Done("PaperWhales");
            var s = spin.gameObject.AddComponent<Spinner>();
            s.Speed = 7f;
        }

        static void AtSatchel(LevelKit kit)
        {
            // Left on the middle bench by the Great Window. (Somebody is going to be in trouble.)
            Vector3 c;
            if (!RmAir("the forgotten satchel", new Vector3(22.05f, 0.75f, -64.5f), new Vector3(0.42f, 0.36f, 0.26f), 90f, 1.2f, out c, 0f)) return;
            var g = RmGroup(kit, "Satchel", new Vector3(c.x, c.y - 0.18f, c.z), 90f);
            var b = new RmBits(kit, g);
            var leather = RmShine(new Color(0.5f, 0.3f, 0.18f), 0.35f);
            b.Box(leather, new Vector3(0f, 0.14f, 0f), new Vector3(0.38f, 0.28f, 0.12f));
            b.Box(leather, new Vector3(0f, 0.22f, 0.065f), new Vector3(0.38f, 0.14f, 0.02f), new Vector3(-8f, 0f, 0f));
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(RmShine(new Color(0.35f, 0.2f, 0.12f), 0.35f), new Vector3(s * 0.1f, 0.16f, 0.078f), new Vector3(0.03f, 0.14f, 0.008f));
                b.Box(Brass, new Vector3(s * 0.1f, 0.1f, 0.08f), new Vector3(0.04f, 0.03f, 0.01f));
            }
            // The strap, flopped over the seat.
            b.Ring(leather, new Vector3(0f, 0.28f, -0.1f), new Vector3(0f, 0f, 0f), 0.18f, 0.03f, 10, 20f, 160f);
            // A rolled-up drawing of the whale poking out of the top, and a pencil.
            b.Tube(RmFelt(new Color(0.96f, 0.94f, 0.88f)), new Vector3(-0.12f, 0.2f, -0.02f), new Vector3(-0.17f, 0.52f, -0.02f), 0.035f, 8);
            b.Ring(RmFelt(new Color(0.9f, 0.3f, 0.3f)), new Vector3(-0.15f, 0.42f, -0.02f), new Vector3(90f, 0f, 0f), 0.037f, 0.01f, 8);
            b.Tube(RmFelt(new Color(1f, 0.8f, 0.2f)), new Vector3(0.1f, 0.24f, -0.03f), new Vector3(0.14f, 0.4f, -0.02f), 0.01f, 6);
            b.Done("Satchel", true);
        }

        // ================================================================ The Hall of Deep Time

        static void RoomsDeepTime(LevelKit kit)
        {
            DtDig(kit);
            DtAmber(kit);
            DtStickChart(kit);
            DtWaterClock(kit);
            DtMegalodon(kit);
            DtCoprolite(kit);
        }

        static void DtDig(LevelKit kit)
        {
            Vector3 f;
            if (!RmFloor("the children's dig", new Vector3(-78f, 0f, -97.5f), new Vector3(4.4f, 1.2f, 3.6f), 0f, 7f, out f)) return;
            var g = RmGroup(kit, "LittleDig", f, 0f);
            var b = new RmBits(kit, g);
            var plank = PatchworkAssets.Planks(new Color(0.62f, 0.46f, 0.3f));
            const float w = 3.8f, d = 3f, h = 0.3f;
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(plank, new Vector3(s * w * 0.5f, h * 0.5f, 0f), new Vector3(0.12f, h, d + 0.12f));
                b.Box(plank, new Vector3(0f, h * 0.5f, s * d * 0.5f), new Vector3(w, h, 0.12f));
            }
            var sand = RmFelt(new Color(0.88f, 0.74f, 0.5f));
            b.Box(sand, new Vector3(0f, h * 0.5f - 0.02f, 0f), new Vector3(w - 0.1f, h - 0.04f, d - 0.1f));
            for (int i = 0; i < 9; i++)
                b.Ball(sand, new Vector3(Mathf.Sin(i * 1.9f) * 1.4f, h - 0.04f, Mathf.Cos(i * 2.6f) * 1.1f), new Vector3(0.7f, 0.16f, 0.6f), 3, 7);
            // What's been found so far: a skull and a curl of backbone, coming out of the sand.
            var bone = Bone;
            Vector3 skull = new Vector3(0.6f, h, 0.3f);
            b.Ball(bone, skull, new Vector3(0.7f, 0.22f, 0.4f), 4, 8);
            b.Box(bone, skull + new Vector3(0.42f, -0.02f, 0f), new Vector3(0.36f, 0.1f, 0.22f), new Vector3(0f, 0f, -6f));
            for (int k = 0; k < 4; k++) b.Box(RmFelt(new Color(0.98f, 0.96f, 0.9f)), skull + new Vector3(0.3f + k * 0.08f, -0.06f, 0.1f), new Vector3(0.02f, 0.06f, 0.02f));
            b.Ball(RmShine(new Color(0.12f, 0.1f, 0.1f), 0.5f), skull + new Vector3(0.1f, 0.06f, 0.14f), new Vector3(0.12f, 0.06f, 0.08f), 3, 6);
            for (int v = 0; v < 9; v++)
            {
                float a = v * 0.33f;
                Vector3 p = new Vector3(0.1f - Mathf.Sin(a) * 1f, h - 0.01f, 0.1f - (1f - Mathf.Cos(a)) * 1f);
                b.Ball(bone, p, new Vector3(0.16f, 0.1f, 0.16f), 3, 6);
                b.Box(bone, p + Vector3.up * 0.04f, new Vector3(0.04f, 0.1f, 0.04f));
            }
            // Brushes, a trowel, a sieve, a bucket, and a kneeling mat.
            b.Box(RmFelt(new Color(0.3f, 0.5f, 0.75f)), new Vector3(-1.1f, h + 0.01f, 0.8f), new Vector3(0.7f, 0.03f, 0.5f));
            b.Box(Brass, new Vector3(-0.6f, h + 0.02f, -0.6f), new Vector3(0.12f, 0.01f, 0.2f), new Vector3(0f, 30f, 0f));
            b.Box(DarkWood, new Vector3(-0.67f, h + 0.04f, -0.78f), new Vector3(0.04f, 0.04f, 0.16f), new Vector3(0f, 30f, 0f));
            foreach (int k in new[] { 0, 1 })
            {
                b.Box(DarkWood, new Vector3(-1.3f + k * 0.25f, h + 0.03f, -0.3f), new Vector3(0.03f, 0.03f, 0.22f), new Vector3(0f, k * 20f - 10f, 0f));
                b.Box(RmFelt(new Color(0.85f, 0.75f, 0.55f)), new Vector3(-1.3f + k * 0.25f + Mathf.Sin((k * 20f - 10f) * Mathf.Deg2Rad) * 0.14f, h + 0.03f, -0.3f + 0.14f), new Vector3(0.05f, 0.04f, 0.06f), new Vector3(0f, k * 20f - 10f, 0f));
            }
            b.Ring(plank, new Vector3(-1.95f + 2.4f, h + 0.06f, -1.0f), new Vector3(90f, 0f, 0f), 0.22f, 0.05f, 14);
            for (int k = -2; k <= 2; k++)
            {
                b.Box(IronMat, new Vector3(0.45f + k * 0.07f, h + 0.06f, -1.0f), new Vector3(0.005f, 0.005f, 0.4f));
                b.Box(IronMat, new Vector3(0.45f, h + 0.06f, -1.0f + k * 0.07f), new Vector3(0.4f, 0.005f, 0.005f));
            }
            b.Cyl(RmShine(new Color(0.95f, 0.75f, 0.2f), 0.6f), new Vector3(2.35f, 0f, 1.2f), 0.18f, 0.3f, 12);
            b.Ring(IronMat, new Vector3(2.35f, 0.34f, 1.2f), new Vector3(0f, 0f, 0f), 0.17f, 0.015f, 10, 0f, 180f);
            // Rope round three sides, on little posts, and a sign.
            var rope = PatchworkAssets.Yarn(new Color(0.85f, 0.3f, 0.3f));
            Vector3[] posts = { new Vector3(-2.1f, 0f, -1.7f), new Vector3(2.1f, 0f, -1.7f), new Vector3(2.1f, 0f, 1.7f), new Vector3(-2.1f, 0f, 1.7f) };
            foreach (var p in posts)
            {
                b.Box(DarkWood, p + Vector3.up * 0.45f, new Vector3(0.08f, 0.9f, 0.08f));
                b.Ball(Brass, p + Vector3.up * 0.92f, Vector3.one * 0.1f, 3, 6);
            }
            for (int i = 0; i < 3; i++)
            {
                Vector3 a = posts[i] + Vector3.up * 0.8f, c = posts[i + 1] + Vector3.up * 0.8f;
                for (int k = 0; k < 6; k++)
                {
                    float t0 = k / 6f, t1 = (k + 1) / 6f;
                    b.Tube(rope, Vector3.Lerp(a, c, t0) + Vector3.down * Mathf.Sin(t0 * Mathf.PI) * 0.12f, Vector3.Lerp(a, c, t1) + Vector3.down * Mathf.Sin(t1 * Mathf.PI) * 0.12f, 0.02f, 6);
                }
            }
            b.Box(DarkWood, new Vector3(-2.1f, 0.6f, -0.3f), new Vector3(0.05f, 1.2f, 0.05f));
            b.Box(RmFelt(CreamCol), new Vector3(-2.06f, 1.1f, -0.3f), new Vector3(0.03f, 0.3f, 0.7f));
            Letters(b[RmFelt(Wine)], "DIG", new Vector3(-2.04f, 1.1f, -0.3f), Vector3.right, 0.035f, 0.01f);
            b.Done("LittleDig", true);
            // (Low enough to step up onto and walk about on the sand; not through the posts.)
            RmSolid(kit, g, new Vector3(0f, 0.13f, 0f), new Vector3(w + 0.1f, 0.26f, d + 0.1f));
            foreach (var p in posts) RmSolid(kit, g, p + Vector3.up * 0.45f, new Vector3(0.12f, 0.9f, 0.12f));
        }

        static void DtAmber(LevelKit kit)
        {
            Transform g;
            if (!RmExhibit(kit, "the amber", new Vector3(-44f, 0f, -95f), 270f, 7f, new Vector3(1f, 1.8f, 0.8f), out g, "Amber",
                "Amber is the resin of ancient trees, gone as hard as stone. Before it set, now and then, a fly or an ant or a little spider got stuck in it, and has been there ever since.",
                "Some amber is more than a hundred million years old. The creatures inside are kept so perfectly you can still see the hairs on their legs.")) return;
            Vector3 inside = RmCase(kit, g, new Vector3(1f, 1.8f, 0.8f), false);
            var b = new RmBits(kit, g);
            var amber = RmGlow(new Color(1f, 0.62f, 0.15f), 1.2f);
            b.Ball(amber, inside + new Vector3(0f, 0.16f, 0f), new Vector3(0.44f, 0.3f, 0.32f), 4, 8);
            b.Ball(amber, inside + new Vector3(0.15f, 0.12f, 0.06f), new Vector3(0.22f, 0.2f, 0.2f), 3, 7);
            // The fly inside, wings and all.
            var fly = RmShine(new Color(0.08f, 0.06f, 0.05f), 0.6f);
            Vector3 c = inside + new Vector3(-0.02f, 0.18f, 0.14f);
            b.Ball(fly, c, new Vector3(0.05f, 0.035f, 0.08f), 3, 6);
            b.Box(RmFelt(new Color(0.7f, 0.6f, 0.45f)), c + new Vector3(-0.04f, 0.02f, -0.01f), new Vector3(0.06f, 0.004f, 0.03f), new Vector3(0f, 20f, 15f));
            b.Box(RmFelt(new Color(0.7f, 0.6f, 0.45f)), c + new Vector3(0.04f, 0.02f, -0.01f), new Vector3(0.06f, 0.004f, 0.03f), new Vector3(0f, -20f, -15f));
            // A magnifying glass on a little brass arm in front of it.
            b.Box(Brass, inside + new Vector3(0.3f, 0.12f, 0.25f), new Vector3(0.02f, 0.24f, 0.02f));
            b.Ring(Brass, inside + new Vector3(0.18f, 0.28f, 0.26f), new Vector3(0f, 20f, 0f), 0.1f, 0.02f, 14);
            b.Disc(CaseGlass, inside + new Vector3(0.18f, 0.28f, 0.26f), 0.19f, 0.01f);
            b.Done("Amber", true);
            PointLight(kit, g.TransformPoint(inside + new Vector3(0f, 0.4f, 0.1f)), new Color(1f, 0.65f, 0.3f), 2.5f, 1f);
        }

        static void DtStickChart(LevelKit kit)
        {
            Vector3 at;
            if (!RmWall("the stick chart", new Vector3(-88f, 3.3f, -63.5f), Vector3.right, new Vector3(2.2f, 1.8f, 0.2f), 5f, out at)) return;
            var g = RmGroup(kit, "StickChart", at, Quaternion.LookRotation(Vector3.right));
            var b = new RmBits(kit, g);
            var stick = RmFelt(new Color(0.86f, 0.74f, 0.52f));
            var tie = RmFelt(new Color(0.55f, 0.4f, 0.28f));
            // The frame of straight sticks, criss-crossed.
            for (int i = -2; i <= 2; i++)
            {
                b.Box(stick, new Vector3(i * 0.45f, 0f, 0.06f), new Vector3(0.025f, 1.6f, 0.025f));
                b.Box(stick, new Vector3(0f, i * 0.36f, 0.08f), new Vector3(1.95f, 0.025f, 0.025f));
            }
            b.Box(stick, new Vector3(0f, 0f, 0.1f), new Vector3(0.025f, 2.2f, 0.025f), new Vector3(0f, 0f, 48f));
            b.Box(stick, new Vector3(0f, 0f, 0.1f), new Vector3(0.025f, 2.2f, 0.025f), new Vector3(0f, 0f, -48f));
            // The swells: curved sticks bending round the islands.
            Vector2[] isles = { new Vector2(-0.55f, 0.3f), new Vector2(0.35f, 0.45f), new Vector2(0.1f, -0.35f), new Vector2(-0.7f, -0.5f), new Vector2(0.75f, -0.15f) };
            foreach (var p in isles)
            {
                b.Ring(stick, new Vector3(p.x, p.y, 0.12f), Vector3.zero, 0.2f, 0.022f, 8, 200f, 340f);
                b.Ring(stick, new Vector3(p.x, p.y, 0.12f), Vector3.zero, 0.3f, 0.02f, 10, 30f, 150f);
                b.Ball(RmFelt(new Color(0.97f, 0.94f, 0.86f)), new Vector3(p.x, p.y, 0.14f), new Vector3(0.09f, 0.07f, 0.05f), 3, 6);
                b.Box(tie, new Vector3(p.x, p.y, 0.1f), new Vector3(0.05f, 0.05f, 0.03f), new Vector3(0f, 0f, 45f));
            }
            for (int i = -2; i <= 2; i++)
                for (int k = -2; k <= 2; k++)
                    if ((i + k) % 2 == 0) b.Box(tie, new Vector3(i * 0.45f, k * 0.36f, 0.1f), new Vector3(0.04f, 0.04f, 0.03f), new Vector3(0f, 0f, 45f));
            // Hung on a cord from a peg.
            b.Box(DarkWood, new Vector3(0f, 1.15f, 0.04f), new Vector3(0.06f, 0.06f, 0.08f));
            b.Box(tie, new Vector3(-0.45f, 0.98f, 0.05f), new Vector3(0.012f, 0.6f, 0.012f), new Vector3(0f, 0f, -62f));
            b.Box(tie, new Vector3(0.45f, 0.98f, 0.05f), new Vector3(0.012f, 0.6f, 0.012f), new Vector3(0f, 0f, 62f));
            b.Done("StickChart", true);
            RmPlaque(kit, at + new Vector3(1.8f, -at.y, 0f), 90f, 3f, "The Stick Chart",
                "Sailors of the Marshall Islands, out in the middle of the Pacific, made maps from palm sticks and shells. The shells are islands. The curved sticks are swells: the way the sea bends and bounces as it rolls round the land.",
                "The charts were left at home. A navigator learned his by heart, then out at sea he read the swells through the hull of his canoe, by feel, and found his way.");
        }

        static void DtWaterClock(LevelKit kit)
        {
            Transform g;
            if (!RmExhibit(kit, "the water clock", new Vector3(-44f, 0f, -44f), 270f, 7f, new Vector3(1.2f, 1.7f, 1.2f), out g, "The Water Clock",
                "The oldest clock of its kind ever found was made in Egypt about three and a half thousand years ago: a stone bowl with a tiny hole near the bottom, and rings of marks inside.",
                "Fill it at sunset and let it drip. The lower the water, the later the night. With one of these, the priests could tell the hours in the dark, when a sundial was no use at all.")) return;
            var b = new RmBits(kit, g);
            Vector3 top = new Vector3(0f, 0.9f, 0f);
            b.Box(Ashlar, new Vector3(0f, 0.45f, 0f), new Vector3(1f, 0.9f, 1f));
            b.Box(PaleStone, new Vector3(0f, 0.88f, 0f), new Vector3(1.1f, 0.06f, 1.1f));
            var alabaster = RmFelt(new Color(0.94f, 0.9f, 0.8f));
            // The bowl, raised on its stand, wider at the top.
            b.Cyl(alabaster, top + Vector3.up * 0.3f, 0.2f, 0.06f, 14);
            for (int i = 0; i < 6; i++)
                b.Ring(alabaster, top + Vector3.up * (0.36f + i * 0.08f), new Vector3(90f, 0f, 0f), 0.22f + i * 0.028f, 0.06f, 18);
            b.Cyl(RmShine(new Color(0.35f, 0.6f, 0.85f), 0.95f), top + Vector3.up * 0.38f, 0.26f, 0.22f, 16);
            for (int i = 0; i < 5; i++) b.Box(RmFelt(new Color(0.45f, 0.35f, 0.25f)), top + new Vector3(0f, 0.44f + i * 0.07f, -(0.23f + i * 0.025f)), new Vector3(0.12f, 0.015f, 0.01f));
            foreach (int s in new[] { -1, 1 }) b.Box(alabaster, top + new Vector3(s * 0.12f, 0.15f, 0f), new Vector3(0.06f, 0.3f, 0.06f));
            // The basin below, and a drop on its way down.
            b.Cyl(alabaster, top + new Vector3(0f, 0f, 0.34f), 0.16f, 0.1f, 14);
            b.Cyl(RmShine(new Color(0.35f, 0.6f, 0.85f), 0.95f), top + new Vector3(0f, 0f, 0.34f), 0.13f, 0.105f, 14);
            b.Box(Brass, top + new Vector3(0f, 0.36f, 0.27f), new Vector3(0.03f, 0.03f, 0.08f));
            b.Done("WaterClock", true);
            var drop = NoShadow(kit.Deco(PrimitiveType.Sphere, top + new Vector3(0f, 0.32f, 0.32f), new Vector3(0.03f, 0.04f, 0.03f), RmShine(new Color(0.6f, 0.8f, 1f), 0.95f), default(Vector3), g));
            var drip = drop.gameObject.AddComponent<DripLoop>();
            drip.Fall = 0.2f;
            drip.Period = 1.6f;
            RmSolid(kit, g, new Vector3(0f, 0.6f, 0f), new Vector3(1f, 1.2f, 1f));
        }

        static void DtMegalodon(LevelKit kit)
        {
            Vector3 at;
            if (!RmWall("Megalodon's jaws", new Vector3(-88f, 4.2f, -37.5f), Vector3.right, new Vector3(3.2f, 2.8f, 0.7f), 5f, out at)) return;
            var g = RmGroup(kit, "MegalodonJaws", at, Quaternion.LookRotation(Vector3.right));
            var b = new RmBits(kit, g);
            var gum = RmFelt(new Color(0.62f, 0.5f, 0.48f));
            var tooth = RmFelt(new Color(0.9f, 0.86f, 0.74f));
            var board = PatchworkAssets.Planks(new Color(0.25f, 0.17f, 0.13f));
            b.Box(board, new Vector3(0f, 0f, 0.03f), new Vector3(0.5f, 0.3f, 0.06f));
            // The jaws, wide open: two big arches, the upper one deeper, rows of teeth round each.
            for (int jaw = 0; jaw < 2; jaw++)
            {
                bool upper = jaw == 0;
                float rx = 1.45f, ry = upper ? 1.15f : 0.95f;
                int n = 26;
                for (int i = 0; i < n; i++)
                {
                    float t0 = i / (float)n, t1 = (i + 1f) / n;
                    float a0 = Mathf.PI * t0, a1 = Mathf.PI * t1;
                    Vector3 p0 = new Vector3(Mathf.Cos(a0) * rx, (upper ? 1f : -1f) * Mathf.Sin(a0) * ry, 0.35f - Mathf.Sin(a0) * 0.18f);
                    Vector3 p1 = new Vector3(Mathf.Cos(a1) * rx, (upper ? 1f : -1f) * Mathf.Sin(a1) * ry, 0.35f - Mathf.Sin(a1) * 0.18f);
                    b.Tube(gum, p0, p1, 0.1f, 6);
                    if (i == 0 || i == n - 1) continue;
                    // Each tooth a flat point, pointing in to the middle of the mouth.
                    Vector3 mid = (p0 + p1) * 0.5f;
                    Vector3 inward = -new Vector3(mid.x / rx, mid.y / ry, 0f).normalized;
                    float ang = Mathf.Atan2(inward.y, inward.x) * Mathf.Rad2Deg - 90f;
                    float big = 0.18f + Mathf.Sin(t0 * Mathf.PI) * 0.1f;
                    b.Box(tooth, mid + inward * (0.09f + big * 0.25f), new Vector3(big * 0.55f, big * 0.55f, 0.025f), new Vector3(0f, 0f, ang + 45f));
                    b.Box(tooth, mid + inward * 0.06f + new Vector3(0f, 0f, -0.05f), new Vector3(big * 0.35f, big * 0.35f, 0.02f), new Vector3(0f, 0f, ang + 45f));
                }
            }
            // Hung from two chains.
            foreach (int s in new[] { -1, 1 })
                for (int k = 0; k < 6; k++)
                    b.Ring(IronMat, new Vector3(s * 1.2f, 0.9f + k * 0.13f, 0.12f), new Vector3(0f, k % 2 == 0 ? 0f : 90f, 0f), 0.05f, 0.015f, 6);
            b.Done("MegalodonJaws", true);
            RmPlaque(kit, at + new Vector3(2.4f, -at.y, 0f), 90f, 3f, "Megalodon",
                "This is how wide the jaws of Megalodon may have opened: the biggest shark there has ever been, which hunted whales until about three and a half million years ago.",
                "Nobody has ever found a whole one. A shark's skeleton is soft cartilage, which hardly ever turns to stone. What it left behind was teeth, thousands and thousands of them, some as big as your hand.");
        }

        static void DtCoprolite(LevelKit kit)
        {
            Transform g;
            if (!RmExhibit(kit, "the coprolite", new Vector3(-74f, 0f, -14.5f), 180f, 7f, new Vector3(1f, 1.6f, 0.8f), out g, "A Coprolite",
                "That is a coprolite: a fossilised dropping. Yes, really. It's been stone for a very, very long time, and it doesn't smell of anything at all.",
                "They're some of the most useful fossils there are. Cut one open and you find out what an animal had for its dinner millions of years ago: crunched-up bones, fish scales, seeds.")) return;
            Vector3 inside = RmCase(kit, g, new Vector3(1f, 1.6f, 0.8f), false);
            var b = new RmBits(kit, g);
            var stone = RmFelt(new Color(0.5f, 0.4f, 0.3f));
            for (int i = 0; i < 4; i++)
                b.Ball(stone, inside + new Vector3(-0.15f + i * 0.1f, 0.1f + (i % 2) * 0.02f, (i % 2) * 0.04f), new Vector3(0.2f - i * 0.02f, 0.16f, 0.17f), 3, 7);
            for (int i = 0; i < 6; i++)
                b.Ball(RmFelt(new Color(0.85f, 0.8f, 0.68f)), inside + new Vector3(-0.15f + i * 0.06f, 0.16f, 0.08f), new Vector3(0.025f, 0.02f, 0.02f), 2, 4);
            b.Box(RmFelt(CreamCol), inside + new Vector3(0.25f, 0.01f, 0.22f), new Vector3(0.22f, 0.01f, 0.1f), new Vector3(0f, -8f, 0f));
            b.Done("Coprolite", true);
        }

        // ================================================================ The Kelp Galleries

        static void RoomsKelp(LevelKit kit)
        {
            KgGrowthChart(kit);
            KgUrchins(kit);
            KgGaribaldi(kit);
            KgSeaDragon(kit);
            KgAirPump(kit);
            KgSeaGlass(kit);
        }

        static void KgGrowthChart(LevelKit kit)
        {
            Vector3 at;
            if (!RmWall("the kelp growth chart", new Vector3(-32f, 3.1f, 6f), Vector3.left, new Vector3(1.4f, 5.4f, 0.4f), 8f, out at, false)) return;
            var g = RmGroup(kit, "KelpGrowthChart", at, Quaternion.LookRotation(Vector3.left));
            var b = new RmBits(kit, g);
            // A tall cream measuring board, a mark every ten centimetres.
            b.Box(DarkWood, new Vector3(0f, 0f, 0.03f), new Vector3(0.62f, 5.3f, 0.06f));
            b.Box(RmFelt(CreamCol), new Vector3(0f, 0f, 0.065f), new Vector3(0.5f, 5.1f, 0.02f));
            var ink = RmFelt(new Color(0.15f, 0.2f, 0.25f));
            for (int i = 0; i <= 50; i++)
                b.Box(ink, new Vector3(0.18f - (i % 5 == 0 ? 0.08f : 0f), -2.55f + i * 0.1f, 0.078f), new Vector3(i % 10 == 0 ? 0.26f : i % 5 == 0 ? 0.18f : 0.08f, 0.015f, 0.005f));
            // Half a metre picked out in gold, near the top: what it can grow in a day.
            b.Box(RmGlow(new Color(1f, 0.8f, 0.35f), 0.9f), new Vector3(-0.18f, 2.05f, 0.08f), new Vector3(0.06f, 0.5f, 0.01f));
            b.Box(RmGlow(new Color(1f, 0.8f, 0.35f), 0.9f), new Vector3(-0.1f, 1.8f, 0.08f), new Vector3(0.2f, 0.02f, 0.01f));
            b.Box(RmGlow(new Color(1f, 0.8f, 0.35f), 0.9f), new Vector3(-0.1f, 2.3f, 0.08f), new Vector3(0.2f, 0.02f, 0.01f));
            Letters(b[RmFelt(Wine)], "ONE DAY", new Vector3(0f, 2.5f, 0.07f), Vector3.forward, 0.016f, 0.008f);
            // A real frond pinned up beside it: the stalk, its blades, and a float at each one's root.
            var stipe = RmFelt(new Color(0.45f, 0.35f, 0.15f));
            var blade = RmFelt(new Color(0.55f, 0.5f, 0.2f));
            var bulb = RmFelt(new Color(0.65f, 0.55f, 0.25f));
            for (int i = 0; i < 18; i++)
            {
                float y = -2.5f + i * 0.28f;
                float x = 0.42f + Mathf.Sin(i * 0.6f) * 0.04f;
                b.Box(stipe, new Vector3(x, y + 0.14f, 0.1f), new Vector3(0.025f, 0.3f, 0.02f), new Vector3(0f, 0f, Mathf.Cos(i * 0.6f) * 8f));
                int s = i % 2 == 0 ? 1 : -1;
                b.Ball(bulb, new Vector3(x + s * 0.05f, y + 0.06f, 0.11f), new Vector3(0.06f, 0.07f, 0.04f), 3, 6);
                b.Box(blade, new Vector3(x + s * 0.2f, y + 0.18f, 0.11f), new Vector3(0.32f, 0.08f, 0.008f), new Vector3(0f, 0f, s * 35f));
            }
            for (int k = 0; k < 5; k++) b.Ball(Brass, new Vector3(0.42f, -2.4f + k * 1.2f, 0.12f), Vector3.one * 0.03f, 2, 4);
            b.Done("KelpGrowthChart", true);
            RmPlaque(kit, at + new Vector3(-1.6f, -at.y, 2.2f), 270f, 3f, "How Fast Kelp Grows",
                "Giant kelp is one of the fastest-growing things there is. In a good summer, one frond can grow half a metre in a single day: look at the gold on the board.",
                "Every blade has a little gas-filled float at its root, to hold it up towards the light. It's a forest of trees with no wood in them, held up by bubbles.");
        }

        static void KgUrchins(LevelKit kit)
        {
            Transform g;
            if (!RmExhibit(kit, "the otters' urchins", new Vector3(-46f, 0f, 15.6f), 180f, 6f, new Vector3(1.2f, 1.1f, 0.9f), out g, "Who Looks After the Forest",
                "Sea urchins eat kelp. Left to themselves they can chew a whole forest down to bare rock.",
                "Sea otters eat sea urchins, cracking them open on a stone balanced on their tummies. Where the otters came back, so did the kelp. And an otter often keeps a favourite stone for years, tucked in a pocket of loose skin under its arm.")) return;
            var b = new RmBits(kit, g);
            b.Box(DarkWood, new Vector3(0f, 0.4f, 0f), new Vector3(1f, 0.8f, 0.7f));
            b.Box(Brass, new Vector3(0f, 0.81f, 0f), new Vector3(1.06f, 0.03f, 0.76f));
            // A wide shallow dish of urchin shells, and the otter's cracking stone.
            b.Cyl(RmShine(new Color(0.85f, 0.85f, 0.88f), 0.85f), new Vector3(-0.1f, 0.82f, 0f), 0.36f, 0.06f, 16);
            var purple = RmFelt(new Color(0.45f, 0.2f, 0.5f));
            var spine = RmFelt(new Color(0.55f, 0.28f, 0.6f));
            for (int i = 0; i < 6; i++)
            {
                Vector3 c = new Vector3(-0.1f + Mathf.Cos(i * 1.05f) * (i == 0 ? 0f : 0.2f), 0.93f, Mathf.Sin(i * 1.05f) * (i == 0 ? 0f : 0.16f));
                b.Ball(purple, c, new Vector3(0.14f, 0.09f, 0.14f), 3, 7);
                for (int k = 0; k < 8; k++)
                {
                    float a = k / 8f * Mathf.PI * 2f;
                    b.Box(spine, c + new Vector3(Mathf.Cos(a) * 0.07f, 0.02f, Mathf.Sin(a) * 0.07f), new Vector3(0.008f, 0.008f, 0.1f), new Vector3(-30f, -a * Mathf.Rad2Deg + 90f, 0f));
                }
            }
            b.Ball(RmFelt(new Color(0.5f, 0.5f, 0.55f)), new Vector3(0.36f, 0.88f, 0.05f), new Vector3(0.18f, 0.1f, 0.14f), 3, 7);
            b.Done("Urchins", true);
            RmSolid(kit, g, new Vector3(0f, 0.45f, 0f), new Vector3(1f, 0.9f, 0.7f));
        }

        static void KgGaribaldi(LevelKit kit)
        {
            // A garibaldi: bright orange, and very fussy about his garden of red weed on a rock,
            // which he keeps weeded and chases everyone else away from.
            Vector3 f;
            if (!RmFloor("the garibaldi's garden", new Vector3(-74f, PoolBed, 31f), new Vector3(1.6f, 0.8f, 1.6f), 0f, 8f, out f, 1f)) return;
            var g = RmGroup(kit, "GaribaldiGarden", f, 0f);
            var b = new RmBits(kit, g);
            b.Ball(RmFelt(new Color(0.4f, 0.4f, 0.42f)), new Vector3(0f, 0.2f, 0f), new Vector3(1.4f, 0.5f, 1.2f), 4, 8);
            var weed = RmFelt(new Color(0.75f, 0.15f, 0.2f));
            for (int i = 0; i < 26; i++)
            {
                float a = i * 2.39996f, r = Mathf.Sqrt(i / 26f) * 0.5f;
                b[weed].Strand(new Vector3(Mathf.Cos(a) * r, 0.42f - r * 0.3f, Mathf.Sin(a) * r), 0.18f + (i % 3) * 0.06f, 0.04f, new Vector3(Mathf.Cos(a) * 0.05f, 0f, Mathf.Sin(a) * 0.05f), a * 30f);
            }
            b.Done("GaribaldiGarden", true);
            var fish = Swim(kit, Swimmer.Fish, f + Vector3.up * 1.1f, 1.3f, 1f, 26f, 0.7f, new Color(1f, 0.45f, 0.08f), new Color(1f, 0.65f, 0.2f), 30f);
            fish.Bob = 0.25f;
            var wife = Swim(kit, Swimmer.Fish, f + new Vector3(2.2f, 1.6f, -1.2f), 2.4f, 1.6f, -14f, 0.6f, new Color(1f, 0.5f, 0.12f), new Color(1f, 0.7f, 0.3f), 200f);
            wife.Bob = 0.3f;
        }

        static void KgSeaDragon(LevelKit kit)
        {
            // A leafy sea dragon: a little seahorse cousin dressed all over in leaves, drifting so
            // slowly it looks like a bit of weed that's come loose.
            Vector3 c = new Vector3(-55f, -3.2f, 47f);
            var g = kit.Group("LeafySeaDragon", c);
            var b = new RmBits(kit, g);
            var skin = RmFelt(new Color(0.85f, 0.65f, 0.3f));
            var leaf = RmGlow(new Color(0.7f, 0.75f, 0.3f), 0.3f);
            var spot = RmFelt(new Color(0.95f, 0.9f, 0.6f));
            // The body: a gentle S of beads, nose forward (+z), tail curling down behind.
            Vector3[] spine = { new Vector3(0f, 0.05f, 0.55f), new Vector3(0f, 0.08f, 0.42f), new Vector3(0f, 0.06f, 0.28f), new Vector3(0f, 0f, 0.14f), new Vector3(0f, -0.04f, 0f), new Vector3(0f, -0.06f, -0.14f), new Vector3(0f, -0.04f, -0.28f), new Vector3(0f, -0.08f, -0.4f), new Vector3(0f, -0.18f, -0.48f), new Vector3(0f, -0.28f, -0.46f) };
            for (int i = 0; i < spine.Length; i++)
            {
                float r = i < 2 ? 0.05f : Mathf.Lerp(0.1f, 0.03f, (i - 2) / 7f);
                b.Ball(skin, spine[i], new Vector3(r * 1.3f, r * 1.6f, r * 1.4f), 3, 7);
                if (i % 2 == 0) b.Ball(spot, spine[i] + new Vector3(0.02f, r * 0.5f, 0f), Vector3.one * r * 0.5f, 2, 4);
            }
            b.Tube(skin, new Vector3(0f, 0.06f, 0.55f), new Vector3(0f, 0.03f, 0.78f), 0.02f, 6);
            RmEyes(b, new Vector3(0f, 0.11f, 0.5f), 0.08f, 0.03f);
            // The leaves: clumps sprouting from its back, belly and tail, each a few flat blades.
            int[] at = { 1, 2, 3, 4, 5, 6, 7, 8 };
            foreach (int i in at)
                for (int k = 0; k < 3; k++)
                {
                    float up = (k == 0 ? 1f : -0.7f);
                    float side = k == 2 ? -1f : k == 1 ? 1f : 0f;
                    Vector3 root = spine[i];
                    float len = 0.16f + (i % 3) * 0.05f;
                    Vector3 dir = new Vector3(side * 0.6f, up, -0.25f).normalized;
                    for (int l = 0; l < 3; l++)
                    {
                        Vector3 tip = root + dir * len + new Vector3((l - 1) * 0.04f, 0f, (l - 1) * 0.04f);
                        b.Box(leaf, (root + tip) * 0.5f, new Vector3(0.05f, len, 0.012f), Quaternion.FromToRotation(Vector3.up, (tip - root).normalized).eulerAngles + new Vector3(0f, l * 30f, 0f));
                    }
                }
            b.Done("LeafySeaDragon");
            var swim = g.gameObject.AddComponent<FishSwim>();
            swim.Center = c;
            swim.RadiusX = 2.4f;
            swim.RadiusZ = 1.6f;
            swim.Speed = 5f;
            swim.Bob = 0.25f;
            swim.BobSpeed = 0.4f;
            swim.Wiggle = 2f;
            swim.WiggleSpeed = 1.2f;
            swim.Bank = 2f;
        }

        static void KgAirPump(LevelKit kit)
        {
            Transform g;
            if (!RmExhibit(kit, "the diver's air pump", new Vector3(-80f, 0f, 71f), 180f, 7f, new Vector3(1.6f, 1.6f, 1.1f), out g, "The Air Pump",
                "Before divers carried tanks of air on their backs, a diver in a brass helmet breathed air pumped down a hose from the boat, by people turning handles like these for as long as he was down.",
                "The pumpers worked in shifts and never stopped. The diver talked to them by tugging on a rope: one tug for \"all well\", and other signals for \"more air\" or \"pull me up\".")) return;
            var b = new RmBits(kit, g);
            var wood = PatchworkAssets.Planks(new Color(0.5f, 0.32f, 0.2f));
            b.Box(wood, new Vector3(0f, 0.45f, 0f), new Vector3(1f, 0.9f, 0.6f));
            b.Box(Brass, new Vector3(0f, 0.91f, 0f), new Vector3(1.04f, 0.04f, 0.64f));
            foreach (int s in new[] { -1, 1 })
            {
                b.Cyl(Brass, new Vector3(s * 0.22f, 0.92f, 0f), 0.13f, 0.38f, 12);
                b.Cyl(Brass, new Vector3(s * 0.22f, 1.3f, 0f), 0.15f, 0.04f, 12);
                b.Box(IronMat, new Vector3(s * 0.22f, 1.42f, 0f), new Vector3(0.03f, 0.22f, 0.03f));
            }
            b.Box(IronMat, new Vector3(0f, 1.53f, 0f), new Vector3(0.6f, 0.04f, 0.04f));
            // The dial on the front.
            b.Disc(Brass, new Vector3(0f, 0.6f, 0.31f), 0.26f, 0.03f);
            b.Disc(RmFelt(CreamCol), new Vector3(0f, 0.6f, 0.325f), 0.22f, 0.01f);
            b.Done("AirPump", true);
            var needle = kit.Group("Needle", g.TransformPoint(new Vector3(0f, 0.6f, 0.335f)));
            needle.SetParent(g, true);
            needle.localRotation = Quaternion.identity;
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.05f, 0f), new Vector3(0.012f, 0.1f, 0.005f), RmFelt(new Color(0.7f, 0.1f, 0.1f)), default(Vector3), needle));
            needle.gameObject.AddComponent<GaugeWaver>();
            // A great flywheel each side, with its handle, still turning a little.
            foreach (int s in new[] { -1, 1 })
            {
                var wheel = kit.Group("Flywheel", g.TransformPoint(new Vector3(s * 0.56f, 0.55f, 0f)));
                wheel.SetParent(g, true);
                wheel.localRotation = Quaternion.identity;
                var wb = new RmBits(kit, wheel);
                wb.Ring(IronMat, Vector3.zero, new Vector3(0f, 90f, 0f), 0.42f, 0.05f, 20);
                for (int k = 0; k < 6; k++) wb.Box(IronMat, Vector3.zero, new Vector3(0.025f, 0.84f, 0.025f), new Vector3(k * 30f, 0f, 0f));
                wb.Box(IronMat, new Vector3(s * 0.06f, 0f, 0f), new Vector3(0.12f, 0.08f, 0.08f));
                wb.Tube(DarkWood, new Vector3(s * 0.02f, 0.38f, 0f), new Vector3(s * 0.2f, 0.38f, 0f), 0.03f, 6);
                wb.Done("Flywheel", true);
                var spin = wheel.gameObject.AddComponent<Spinner>();
                spin.Axis = Vector3.right;
                spin.Speed = s * 14f;
            }
            // The hose, coiled on the floor beside it, and its brass end.
            var hose = RmShine(new Color(0.15f, 0.15f, 0.17f), 0.4f);
            var hb = new RmBits(kit, g);
            for (int i = 0; i < 4; i++) hb.Ring(hose, new Vector3(0.05f, 0.04f + i * 0.06f, -0.75f), new Vector3(90f, 0f, 0f), 0.38f - i * 0.03f, 0.06f, 18);
            hb.Tube(hose, new Vector3(0.3f, 0.3f, -0.3f), new Vector3(0.3f, 0.7f, -0.05f), 0.035f, 6);
            hb.Tube(Brass, new Vector3(-0.3f, 0.06f, -0.4f), new Vector3(-0.42f, 0.06f, -0.3f), 0.045f, 8);
            hb.Done("PumpHose", true);
            RmSolid(kit, g, new Vector3(0f, 0.75f, 0f), new Vector3(1.3f, 1.5f, 0.6f));
        }

        static void KgSeaGlass(LevelKit kit)
        {
            Vector3 at;
            if (!RmWall("the sea-glass window", new Vector3(-73f, 9f, 76f), Vector3.back, new Vector3(3.3f, 3.3f, 0.25f), 8f, out at)) return;
            var g = RmGroup(kit, "SeaGlassWindow", at, Quaternion.LookRotation(Vector3.back));
            var b = new RmBits(kit, g);
            b.Ring(IronMat, new Vector3(0f, 0f, 0.1f), Vector3.zero, 1.55f, 0.12f, 32);
            b.Ring(Brass, new Vector3(0f, 0f, 0.14f), Vector3.zero, 1.62f, 0.05f, 32);
            b.Disc(RmFelt(new Color(0.05f, 0.08f, 0.12f)), new Vector3(0f, 0f, 0.04f), 3.1f, 0.04f);
            // Rings of frosted glass, worn smooth by the sea: greens, blues, a little amber.
            Color[] glass = { new Color(0.35f, 0.75f, 0.6f), new Color(0.45f, 0.65f, 0.95f), new Color(0.75f, 0.9f, 0.85f), new Color(1f, 0.7f, 0.35f), new Color(0.3f, 0.55f, 0.75f) };
            var mbs = new MeshBatch[glass.Length];
            for (int i = 0; i < mbs.Length; i++) mbs[i] = new MeshBatch();
            var rng = new System.Random(77);
            for (int ring = 0; ring < 5; ring++)
            {
                float r = 0.25f + ring * 0.28f;
                int n = 6 + ring * 7;
                for (int k = 0; k < n; k++)
                {
                    float a = (k + ring * 0.5f) / n * Mathf.PI * 2f;
                    float sz = 0.13f + (float)rng.NextDouble() * 0.07f;
                    mbs[rng.Next(glass.Length)].Ball(new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0.1f), new Vector3(sz * 1.3f, sz, 0.05f), 2, 6);
                }
            }
            mbs[3].Ball(new Vector3(0f, 0f, 0.1f), new Vector3(0.3f, 0.3f, 0.06f), 3, 8);
            for (int i = 0; i < mbs.Length; i++) Twinkling(mbs[i], g, "SeaGlass", glass[i], 0.5f, 1.1f, 0.3f + i * 0.07f, i * 1.3f);
            // The lead between them: spokes out from the middle.
            for (int k = 0; k < 8; k++) b.Box(IronMat, new Vector3(0f, 0f, 0.13f), new Vector3(0.03f, 3f, 0.02f), new Vector3(0f, 0f, k * 22.5f));
            b.Done("SeaGlassWindow", true);
            PointLight(kit, g.TransformPoint(new Vector3(0f, 0f, 1.2f)), new Color(0.5f, 0.85f, 0.8f), 6f, 0.7f);
        }
    }
}

using UnityEngine;

namespace Patchwork
{
    // Toybox Town's playground layer: score bubble trails along every route, prize bubbles in
    // hard-to-reach spots, sticker boards that want a particular sticker, the Toy Workshop yard
    // (a crate puzzle and a see-saw catapult), and loose toys to kick, push and blow around.
    public static partial class Level1Builder
    {
        static void Fun(LevelKit kit)
        {
            ScoreBubbles.Ensure(kit);
            LooseToys(kit);
            Workshop(kit, new Vector3(60f, 0f, 42f));
            StickerBoards(kit);
            PrizeSpots(kit);
            kit.Info.MapPins.Add(new MapPin("Toy Workshop", new Vector3(60f, 0f, 40f)));
        }

        // Score bubbles go in last, once every roof and platform exists, so trails can follow them.
        static void ScoreTrails(LevelKit kit)
        {
            Physics.SyncTransforms();
            // From the spawn to the bandstand, round the bandstand, and up the drum.
            Trail(kit, new Vector3(0f, 1.1f, -11f), new Vector3(0f, 1.1f, -3f), 5);
            Ring(kit, Bandstand, 8.5f, 16, 1.1f);
            for (int i = 0; i < 3; i++) ScoreBubbles.Place(kit, new Vector3(-7.5f, 2.5f + i * 1.5f, 9.5f), 10);
            Ring(kit, Vector3.zero, 17.5f, 24, 1.1f);
            // The streets either side of the square.
            Trail(kit, new Vector3(-28.6f, 1.1f, -20f), new Vector3(-28.6f, 1.1f, 20f), 9);
            Trail(kit, new Vector3(28.2f, 1.1f, -20f), new Vector3(28.2f, 1.1f, 20f), 9);
            // A zigzag through the barracks yard, and along the domino run.
            Trail(kit, new Vector3(-8f, 1.1f, 36f), new Vector3(8f, 1.1f, 44f), 5);
            Trail(kit, new Vector3(8f, 1.1f, 46f), new Vector3(-8f, 1.1f, 54f), 5);
            Trail(kit, new Vector3(20.5f, 1.1f, 32f), new Vector3(20.5f, 1.1f, 54f), 8);
            // Over the xylophone bridge (following its keys), and down the canal for swimmers.
            Trail(kit, new Vector3(33f, 0f, 60f), new Vector3(39f, 0f, 60f), 5, 1.2f);
            Trail(kit, new Vector3(36f, -0.25f, -40f), new Vector3(36f, -0.25f, 40f), 17);
            // Round the duck pond and the carousel, and through the picnic park.
            Ring(kit, new Vector3(14f, 0f, 74f), 7.5f, 14, 1.1f);
            Ring(kit, new Vector3(-22f, 0f, 63f), 6.5f, 14, 1.1f);
            Trail(kit, new Vector3(-12f, 1.1f, 68f), new Vector3(-12f, 1.1f, 80f), 5);
            // Down in the ravine, and a gold one between the stepping stones.
            Trail(kit, new Vector3(-40f, -3.9f, -20f), new Vector3(-40f, -3.9f, 20f), 9);
            ScoreBubbles.Place(kit, new Vector3(StreamWest + 1f, -3.8f, -30f), 10);
            ScoreBubbles.Place(kit, new Vector3(StreamEast - 1f, -3.8f, -30f), 10);
            ScoreBubbles.Place(kit, new Vector3((StreamWest + StreamEast) * 0.5f, -2.6f, -30f), 100);
            // Along the top of Spindle Hill.
            Trail(kit, new Vector3(-67f, 5.2f, 8f), new Vector3(-67f, 5.2f, 30f), 6);
            // Over the rooftops of Chimney Lane (they follow the roofs up and down).
            Trail(kit, new Vector3(-28f, 0f, -46f), new Vector3(28f, 0f, -46f), 15, 1.2f);
            Trail(kit, new Vector3(-14f, 0f, -58f), new Vector3(16f, 0f, -60f), 9, 1.2f);
            // Round the top of the spire and inside the belfry (big ones up there).
            Ring(kit, new Vector3(0f, 16f, -72f), 2.1f, 6, 0.8f, 50);
            ScoreBubbles.Place(kit, new Vector3(0f, 17.8f, -72f), 100);
            Ring(kit, Tower + Vector3.up * 23.5f, 2.6f, 6, 0.8f, 50);
            // Over the Dye Works, along the station platform and on the tunnel roof.
            Trail(kit, new Vector3(56f, 0f, 8f), new Vector3(72f, 0f, 8f), 8, 1.2f);
            Trail(kit, new Vector3(66f, 1.5f, -51.2f), new Vector3(74f, 1.5f, -51.2f), 4);
            ScoreBubbles.Place(kit, new Vector3(82f, 3.9f, -61.4f), 50);
            ScoreBubbles.Place(kit, new Vector3(82f, 3.9f, -64.6f), 50);
            // The Toy Workshop: inside the pen, and up on the shelf.
            Ring(kit, new Vector3(58f, 0f, 50.5f), 2.2f, 8, 1.1f);
            Trail(kit, new Vector3(66.2f, 8.9f, 47f), new Vector3(69.8f, 8.9f, 47f), 4, -1f, 50);
        }

        // A line of bubbles (collect every one of them for a "perfect trail" bonus).
        static void Trail(LevelKit kit, Vector3 a, Vector3 b, int n, float lift = -1f, int value = 10)
        {
            ScoreBubbles.BeginSet(kit);
            for (int i = 0; i < n; i++)
            {
                Vector3 p = Vector3.Lerp(a, b, n == 1 ? 0.5f : i / (float)(n - 1));
                if (lift >= 0f) p = Snap(p, lift);
                ScoreBubbles.Place(kit, p, value);
            }
            ScoreBubbles.EndSet();
        }

        static void Ring(LevelKit kit, Vector3 c, float radius, int n, float height, int value = 10)
        {
            ScoreBubbles.BeginSet(kit);
            for (int i = 0; i < n; i++)
            {
                float a = i / (float)n * Mathf.PI * 2f;
                ScoreBubbles.Place(kit, c + new Vector3(Mathf.Cos(a) * radius, height, Mathf.Sin(a) * radius), value);
            }
            ScoreBubbles.EndSet();
        }

        // Drops a point onto whatever's below it (a roof, a bridge) and lifts it a little.
        static Vector3 Snap(Vector3 p, float lift)
        {
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(p.x, p.y + 25f, p.z), Vector3.down, out hit, 60f, ~0, QueryTriggerInteraction.Ignore))
                return hit.point + Vector3.up * lift;
            return p + Vector3.up * lift;
        }

        // ---------------------------------------------------------------- Loose toys

        static void LooseToys(LevelKit kit)
        {
            // Beach balls to kick about.
            Grabbable.BeachBall(kit, new Vector3(9f, 0f, -6f), 0.75f);
            Grabbable.BeachBall(kit, new Vector3(-1f, 0f, 64f), 0.9f);
            Grabbable.BeachBall(kit, new Vector3(56f, 0f, 35f), 0.75f);
            Grabbable.BeachBall(kit, new Vector3(-62f, 4f, 20f), 0.7f);
            // A tower of building blocks in the park, just asking to be knocked over.
            Color[] cols = { new Color(0.9f, 0.35f, 0.35f), new Color(0.35f, 0.6f, 0.9f), new Color(1f, 0.8f, 0.3f), new Color(0.45f, 0.75f, 0.45f), new Color(0.75f, 0.5f, 0.85f) };
            Vector3 t = new Vector3(-24f, 0f, 80f);
            int k = 0;
            for (int row = 0; row < 3; row++)
                for (int i = 0; i < 3 - row; i++)
                    Grabbable.ToyBlock(kit, t + new Vector3((i - (2 - row) * 0.5f) * 1.02f, row * 1.0f, 0f), 1f, cols[k++ % cols.Length], 0f);
            ClearGrass(t, 2.5f);
            kit.Hint(t + new Vector3(0f, 1.5f, -3f), new Vector3(6f, 3f, 3f), "Building Blocks",
                "Run into the blocks to knock them over, or hold {Grab} to push and pull them about.");
        }

        // ---------------------------------------------------------------- The Toy Workshop yard

        static void Workshop(LevelKit kit, Vector3 c)
        {
            var card = new Color(0.78f, 0.62f, 0.45f);
            var red = new Color(0.85f, 0.3f, 0.32f);
            kit.Hint(c + new Vector3(0f, 1.5f, -6f), new Vector3(10f, 3f, 4f), "The Toy Workshop",
                "Hold {Grab} next to a crate to take hold of it. Walk to push it, back away to pull it. Let go of {Grab} to drop it.");

            // A sign on two posts.
            Vector3 sign = c + new Vector3(-6f, 0f, -6f);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cylinder, sign + new Vector3(s * 1.2f, 1.2f, 0f), new Vector3(0.14f, 1.2f, 0.14f), PatchworkAssets.Wood(TownKit.DarkWood));
                Solid(kit, sign + new Vector3(s * 1.2f, 1.2f, 0f), new Vector3(0.2f, 2.4f, 0.2f));
            }
            kit.Deco(PrimitiveType.Cube, sign + new Vector3(0f, 2.3f, 0f), new Vector3(2.9f, 0.9f, 0.12f), PatchworkAssets.Planks(new Color(0.95f, 0.85f, 0.6f)));
            kit.Deco(PrimitiveType.Cube, sign + new Vector3(0f, 2.3f, -0.07f), new Vector3(2.5f, 0.12f, 0.02f), PatchworkAssets.Felt(red));
            kit.Deco(PrimitiveType.Cylinder, sign + new Vector3(-0.9f, 2.45f, -0.08f), new Vector3(0.3f, 0.02f, 0.3f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f), new Vector3(90f, 0f, 0f));

            // 1) The crate pen: a crate on the pad opens the gate.
            float x0 = c.x - 6f, x1 = c.x + 2f, z0 = c.z + 4f, z1 = c.z + 12f;
            const float wallH = 2.6f;
            // The side walls run the full depth; the back and front fit between them, a touch
            // lower, so no two faces sit exactly on top of each other.
            kit.Cardboard(new Vector3(x0, wallH * 0.5f, (z0 + z1) * 0.5f), new Vector3(0.4f, wallH, z1 - z0 + 0.4f), card, "PenWall");
            kit.Cardboard(new Vector3(x1, wallH * 0.5f, (z0 + z1) * 0.5f), new Vector3(0.4f, wallH, z1 - z0 + 0.4f), card, "PenWall");
            kit.Cardboard(new Vector3((x0 + x1) * 0.5f, (wallH - 0.04f) * 0.5f, z1), new Vector3(x1 - x0 - 0.4f, wallH - 0.04f, 0.38f), card, "PenWall");
            float gx = (x0 + x1) * 0.5f;
            kit.Cardboard(new Vector3((x0 + 0.2f + gx - 1.2f) * 0.5f, (wallH - 0.04f) * 0.5f, z0), new Vector3(gx - 1.2f - x0 - 0.2f, wallH - 0.04f, 0.38f), card, "PenWall");
            kit.Cardboard(new Vector3((gx + 1.2f + x1 - 0.2f) * 0.5f, (wallH - 0.04f) * 0.5f, z0), new Vector3(x1 - 0.2f - gx - 1.2f, wallH - 0.04f, 0.38f), card, "PenWall");
            kit.Tape(new Vector3((x0 + x1) * 0.5f, wallH + 0.02f, z1), new Vector3(x1 - x0, 0.04f, 0.5f));
            var gateGo = new GameObject("PenGate");
            gateGo.transform.SetParent(kit.Root, false);
            gateGo.transform.position = new Vector3(gx, 0f, z0);
            var gate = gateGo.AddComponent<Gate>();
            var door = kit.Block(new Vector3(gx, wallH * 0.5f, z0), new Vector3(2.4f, wallH, 0.3f), red, "GateDoor", PatchworkAssets.Planks(red));
            door.SetParent(gateGo.transform, true);
            for (int i = 0; i < 2; i++)
            {
                var stripe = kit.Deco(PrimitiveType.Cube, Vector3.zero, Vector3.one, PatchworkAssets.Felt(Color.white), default(Vector3), door);
                stripe.localPosition = new Vector3(0f, -0.2f + i * 0.4f, -0.52f);
                stripe.localScale = new Vector3(1f, 0.08f, 0.05f);
            }
            gate.Door = door;
            gate.Drop = wallH + 0.1f;

            Vector3 padPos = new Vector3(gx, 0f, z0 - 3f);
            var pad = PadAt(kit, padPos, 1.1f, 20f);
            var lampPost = new Vector3(gx + 1.6f, 0f, z0 - 0.5f);
            kit.Deco(PrimitiveType.Cylinder, lampPost + Vector3.up * 1.2f, new Vector3(0.12f, 1.2f, 0.12f), PatchworkAssets.Plain(TownKit.Iron, 0.5f));
            Solid(kit, lampPost + Vector3.up * 1.2f, new Vector3(0.2f, 2.4f, 0.2f));
            pad.Lamp = kit.Deco(PrimitiveType.Sphere, lampPost + Vector3.up * 2.55f, Vector3.one * 0.4f, PatchworkAssets.Glow(new Color(0.95f, 0.4f, 0.35f), 0.8f)).GetComponent<Renderer>();
            pad.Changed = open => gate.SetOpen(open);

            Grabbable.Crate(kit, c + new Vector3(-8f, 0f, -1f), 1.6f, new Color(0.75f, 0.55f, 0.35f));
            Grabbable.Crate(kit, c + new Vector3(-10f, 0f, 2f), 1.2f, new Color(0.6f, 0.45f, 0.65f));
            PrizeBubble.Place(kit, new Vector3(gx, 1.5f, z1 - 2.5f), "prize_fish");
            ClearGrass(new Vector3(gx, 0f, (z0 + z1) * 0.5f), 5f);

            // 2) The see-saw catapult: stand on the low end, pull the lever, fly up to the shelf.
            Vector3 pivotPos = c + new Vector3(8f, 0.6f, -6f);
            var wood = PatchworkAssets.Planks(new Color(0.85f, 0.65f, 0.4f));
            kit.Deco(PrimitiveType.Cube, pivotPos + Vector3.down * 0.3f, new Vector3(1.2f, 0.6f, 1.2f), PatchworkAssets.Wood(TownKit.DarkWood), new Vector3(0f, 0f, 0f));
            Solid(kit, pivotPos + Vector3.down * 0.3f, new Vector3(1.2f, 0.6f, 1.2f));
            var board = kit.Group("SeesawBoard", pivotPos);
            var plank = kit.Block(pivotPos + Vector3.up * 0.15f, new Vector3(1.6f, 0.3f, 8f), new Color(0.85f, 0.65f, 0.4f), "SeesawPlank", wood);
            plank.SetParent(board, true);
            for (int s = -1; s <= 1; s += 2)
            {
                var stripe = kit.Deco(PrimitiveType.Cube, Vector3.zero, Vector3.one, PatchworkAssets.Felt(red), default(Vector3), plank);
                stripe.localPosition = new Vector3(0f, 0.52f, s * 0.42f);
                stripe.localScale = new Vector3(1.01f, 0.05f, 0.04f);
            }
            var saw = board.gameObject.AddComponent<Seesaw>();
            saw.Board = board;
            saw.LowEnd = pivotPos + new Vector3(0f, -0.5f, 3.6f);
            saw.LaunchSpeed = 22f;

            // The weight over the raised end, hung from a little gantry.
            Vector3 raised = pivotPos + new Vector3(0f, 0f, -3.6f);
            var iron = PatchworkAssets.Plain(TownKit.Iron, 0.5f);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(raised.x + s * 1.5f, 3.4f, raised.z), new Vector3(0.25f, 6.8f, 0.25f), PatchworkAssets.Wood(TownKit.DarkWood));
                Solid(kit, new Vector3(raised.x + s * 1.5f, 3.4f, raised.z), new Vector3(0.3f, 6.8f, 0.3f));
            }
            kit.Deco(PrimitiveType.Cube, new Vector3(raised.x, 6.85f, raised.z), new Vector3(3.3f, 0.3f, 0.35f), PatchworkAssets.Wood(TownKit.DarkWood));
            var weight = kit.Group("Weight", new Vector3(raised.x, 5.4f, raised.z));
            kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(1.4f, 1.1f, 1.4f), iron, default(Vector3), weight);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.62f, 0f), new Vector3(0.9f, 0.18f, 0.9f), iron, default(Vector3), weight);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, -0.71f), new Vector3(1f, 0.4f, 0.02f), PatchworkAssets.Felt(Color.white), default(Vector3), weight);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 2.5f, 0f), new Vector3(0.06f, 1.8f, 0.06f), PatchworkAssets.Yarn(new Color(0.95f, 0.9f, 0.8f)), default(Vector3), weight);
            saw.Weight = weight;

            // The lever beside the low end.
            Vector3 lv = pivotPos + new Vector3(2.2f, -0.6f, 3.6f);
            kit.Deco(PrimitiveType.Cube, lv + Vector3.up * 0.3f, new Vector3(0.8f, 0.6f, 0.8f), PatchworkAssets.Wood(TownKit.DarkWood));
            Solid(kit, lv + Vector3.up * 0.3f, new Vector3(0.8f, 0.6f, 0.8f));
            var handle = kit.Group("LeverHandle", lv + Vector3.up * 0.6f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.7f, 0f), new Vector3(0.12f, 0.7f, 0.12f), PatchworkAssets.Wood(new Color(0.85f, 0.7f, 0.5f)), default(Vector3), handle);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.45f, 0f), Vector3.one * 0.32f, PatchworkAssets.Plain(red, 0.7f), default(Vector3), handle);
            var lever = kit.Volume<Lever>(lv + Vector3.up * 1.2f, new Vector3(4.4f, 3f, 3.4f), "SeesawLever");
            lever.Handle = handle;
            lever.Pulled = () => saw.Drop();

            // The shelf it flings you up to.
            Vector3 shelf = pivotPos + new Vector3(0f, 0f, 11f);
            shelf.y = 0f;
            kit.Block(shelf + new Vector3(0f, 7.7f, 0f), new Vector3(5f, 0.6f, 4f), new Color(0.95f, 0.85f, 0.6f), "ToyShelf", PatchworkAssets.Planks(new Color(0.95f, 0.85f, 0.6f)));
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    Vector3 leg = shelf + new Vector3(sx * 2.2f, 3.7f, sz * 1.7f);
                    kit.Deco(PrimitiveType.Cylinder, leg, new Vector3(0.3f, 3.7f, 0.3f), PatchworkAssets.Wood(new Color(0.75f, 0.55f, 0.35f)));
                    Solid(kit, leg, new Vector3(0.35f, 7.4f, 0.35f));
                }
            saw.Target = shelf + new Vector3(0f, 8f, 0f);
            PrizeBubble.Place(kit, shelf + new Vector3(0f, 9.3f, 0.6f), "deco_pinwheels");
            kit.Hint(lv + new Vector3(-1f, 1.5f, -2f), new Vector3(5f, 3f, 4f), "The Catapult",
                "Stand on the low end of the see-saw, then {Grab} the lever. Hold on to your hat!");
            ClearGrass(pivotPos, 5f);
            ClearGrass(shelf, 3f);
        }

        static PressurePad PadAt(LevelKit kit, Vector3 feet, float radius, float minMass)
        {
            var g = kit.Group("PressurePad", feet);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.06f, 0f), new Vector3(radius * 2.3f, 0.06f, radius * 2.3f), PatchworkAssets.Plain(new Color(0.35f, 0.33f, 0.38f), 0.5f), default(Vector3), g);
            var top = kit.Group("Top", feet + Vector3.up * 0.16f);
            top.SetParent(g, true);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(radius * 2f, 0.08f, radius * 2f), PatchworkAssets.Plain(new Color(0.95f, 0.75f, 0.3f), 0.7f), default(Vector3), top);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.05f, 0f), new Vector3(radius * 1.2f, 0.06f, radius * 1.2f), PatchworkAssets.Plain(new Color(0.95f, 0.45f, 0.4f), 0.7f), default(Vector3), top);
            var pad = g.gameObject.AddComponent<PressurePad>();
            pad.Top = top;
            pad.Radius = radius;
            pad.MinMass = minMass;
            ClearGrass(feet, radius + 0.5f);
            return pad;
        }

        // ---------------------------------------------------------------- Sticker boards

        static void StickerBoards(LevelKit kit)
        {
            // In the square, near where you start: the first one, with a hint about stickers.
            var a = StickerBoard(kit, new Vector3(7f, 0f, -9f), new Vector3(-0.45f, 0f, -0.9f), StickerId.Star, "L1_sticker_star", true);
            var aPrize = PrizeBubble.Place(kit, new Vector3(7f, 1.6f, -9f) + new Vector3(-0.45f, 0f, -0.9f).normalized * 2.2f, "prize_moon", true);
            a.Done = fresh => aPrize.Reveal();
            kit.Hint(new Vector3(6f, 1.5f, -11f), new Vector3(5f, 3f, 4f), "Sticker Board",
                "Press {Sticker} to open your stickers. Aim at the outline, pick the Gold Star and stick it on!");

            // On the Toy Shop's blank east wall, facing the domino run.
            var b = StickerPlaque(kit, new Vector3(16.56f, 2.2f, 27f), Vector3.right, StickerId.Heart, "L1_sticker_heart");
            var bPrize = PrizeBubble.Place(kit, new Vector3(18.8f, 1.6f, 27f), "prize_partyhat", true);
            b.Done = fresh => bPrize.Reveal();

            // In the picnic park.
            var c = StickerBoard(kit, new Vector3(-5f, 0f, 70f), Vector3.back, StickerId.Flower, "L1_sticker_flower", false);
            var cPrize = PrizeBubble.Place(kit, new Vector3(-5f, 1.6f, 67.8f), "deco_snowglobe", true);
            c.Done = fresh => cPrize.Reveal();

            // On the barracks' back wall: wants the Sleepy Moon, which the first board gives you.
            var d = StickerPlaque(kit, new Vector3(6f, 1.7f, 57.94f), Vector3.back, StickerId.Moon, "L1_sticker_moon");
            var dPrize = PrizeBubble.Place(kit, new Vector3(4f, 1.6f, 55.5f), "prize_piratehat", true);
            d.Done = fresh => dPrize.Reveal();
        }

        // A cork board on two posts with a dashed outline of the sticker it wants.
        static StickerSwitch StickerBoard(LevelKit kit, Vector3 feet, Vector3 facing, StickerId wanted, string saveId, bool big)
        {
            facing.y = 0f;
            facing.Normalize();
            float yaw = Mathf.Atan2(facing.x, facing.z) * Mathf.Rad2Deg;
            var rot = Quaternion.Euler(0f, yaw, 0f);
            Vector3 side = rot * Vector3.right;
            var wood = PatchworkAssets.Wood(TownKit.DarkWood);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cylinder, feet + side * s * 1.05f + Vector3.up * 1.1f, new Vector3(0.14f, 1.1f, 0.14f), wood);
                Solid(kit, feet + side * s * 1.05f + Vector3.up * 1.1f, new Vector3(0.2f, 2.2f, 0.2f));
            }
            Vector3 centre = feet + Vector3.up * 1.7f;
            var cork = kit.Block(centre, new Vector3(2f, 1.6f, 0.16f), new Color(0.8f, 0.62f, 0.42f), "StickerBoard", PatchworkAssets.Felt(new Color(0.8f, 0.62f, 0.42f)));
            cork.rotation = rot;
            var frame = kit.Deco(PrimitiveType.Cube, centre - facing * 0.01f, new Vector3(2.2f, 1.8f, 0.12f), wood);
            frame.rotation = rot;
            // A couple of drawing pins.
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Sphere, centre + side * s * 0.8f + Vector3.up * 0.6f + facing * 0.1f, Vector3.one * 0.1f, PatchworkAssets.Plain(s < 0 ? LevelKit.Palette.Rose : LevelKit.Palette.Sky, 0.8f));
            ClearGrass(feet, 1.4f);
            return OutlineAt(kit, centre + facing * 0.09f, facing, wanted, saveId);
        }

        // A wooden plaque fixed to a wall, with the outline on it.
        static StickerSwitch StickerPlaque(LevelKit kit, Vector3 centre, Vector3 facing, StickerId wanted, string saveId)
        {
            float yaw = Mathf.Atan2(facing.x, facing.z) * Mathf.Rad2Deg;
            var plaque = kit.Block(centre, new Vector3(1.5f, 1.5f, 0.1f), new Color(0.95f, 0.88f, 0.7f), "StickerPlaque", PatchworkAssets.Planks(new Color(0.95f, 0.88f, 0.7f)));
            plaque.rotation = Quaternion.Euler(0f, yaw, 0f);
            return OutlineAt(kit, centre + facing * 0.06f, facing, wanted, saveId);
        }

        static StickerSwitch OutlineAt(LevelKit kit, Vector3 pos, Vector3 facing, StickerId wanted, string saveId)
        {
            var q = kit.Deco(PrimitiveType.Quad, pos, Vector3.one * 1.15f, PatchworkAssets.Sticker(StickerArt.Outline(wanted)));
            q.rotation = Quaternion.LookRotation(-facing, Vector3.up);
            q.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var go = new GameObject("StickerSwitch");
            go.transform.SetParent(kit.Root, false);
            go.transform.position = pos;
            var sw = go.AddComponent<StickerSwitch>();
            sw.Wanted = wanted;
            sw.SaveId = saveId;
            return sw;
        }

        // ---------------------------------------------------------------- Prize bubbles in tricky spots

        static void PrizeSpots(LevelKit kit)
        {
            // Under the xylophone bridge, only reachable by swimming.
            PrizeBubble.Place(kit, new Vector3((CanalWest + CanalEast) * 0.5f, 0.2f, 57f), "deco_duck");
            // On top of the alphabet block stacks.
            PrizeBubble.Place(kit, new Vector3(79.5f, 12.2f, 78.5f), "prize_rainbow");
            PrizeBubble.Place(kit, new Vector3(-80f, 10.2f, -72.3f), "prize_paw");
            // On the train tunnel's roof: ride the Toybox Express and hop off on top.
            PrizeBubble.Place(kit, new Vector3(82f, 4.4f, -63f), "prize_propeller");
            // On top of the weathervane spire.
            PrizeBubble.Place(kit, new Vector3(2f, 17.4f, -74f), "deco_lanterns");
            // On the spool tower on Spindle Hill.
            PrizeBubble.Place(kit, new Vector3(-84f, 7.6f, -20f), "prize_crown");
        }
    }
}

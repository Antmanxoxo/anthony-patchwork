using UnityEngine;

namespace Patchwork
{
    // Level 1 - The Sewing Basket Garden. A felt garden inside a giant wicker basket.
    // Goes in a straight line and each part teaches one thing:
    // 1. garden start with threads and a bouncy pin-cushion
    // 2. wicker wall with a drain grate (Water)
    // 3. dye pond with a fountain up to a golden button (Water)
    // 4. button bridge - two sliding buttons over a pit
    // 5. 15m gap with a swing anchor (Yarn)
    // 6. 7m gap with pinwheels (Wind)
    // 7. updraft up to the last golden button (Wind), then the goal
    // Gap sizes are based on the movement numbers: normal jump ~5m, Wind glide alone ~8m,
    // Wind glide + gust ~13.4m (so the 15m Yarn gap still needs Yarn).
    public static class Level1Builder
    {
        public const string SceneName = "Level1";
        public static readonly string[] CollectibleIds = { "L1_fountain", "L1_swing", "L1_updraft" };

        // Garden palette
        static readonly Color[] Lawn = { new Color(0.55f, 0.75f, 0.45f), new Color(0.48f, 0.68f, 0.42f), new Color(0.62f, 0.8f, 0.5f), new Color(0.5f, 0.72f, 0.5f) };
        static readonly Color[] Meadow = { new Color(0.6f, 0.78f, 0.48f), new Color(0.72f, 0.82f, 0.5f), new Color(0.52f, 0.7f, 0.46f), new Color(0.95f, 0.88f, 0.6f) };
        static readonly Color[] Pond = { new Color(0.6f, 0.78f, 0.9f), new Color(0.55f, 0.72f, 0.88f), new Color(0.7f, 0.85f, 0.92f), new Color(0.62f, 0.8f, 0.6f) };

        public static LevelInfo Build(LevelKit kit)
        {
            var info = kit.Info;
            info.Id = SceneName;
            info.Title = "Level 1 · The Sewing Basket Garden";
            info.Subtitle = "A felt garden inside a giant sewing basket. Water, Yarn and Wind all lend a hand.";
            info.Spawn = new Vector3(0f, 0.1f, -6f);
            info.SpawnYaw = 0f;
            info.KillY = -12f;
            info.Sky = new Color(0.78f, 0.88f, 0.95f);

            kit.Backdrop(-40f);
            // A giant wicker basket around the whole course, with a soft blue felt "sky" lid.
            kit.Enclosure(new Vector3(-26f, -45f, -24f), new Vector3(26f, 32f, 124f),
                new[] { new Color(0.62f, 0.45f, 0.28f), new Color(0.75f, 0.56f, 0.34f), new Color(0.82f, 0.64f, 0.4f), new Color(0.88f, 0.72f, 0.48f) },
                new Color(0.62f, 0.78f, 0.95f), true);
            kit.FeltSun(new Vector3(0f, 22f, 123.6f), 5f, Vector3.back);
            for (int i = 0; i < 6; i++)
                kit.Cloud(new Vector3(-16f + (i % 3) * 16f, 26f - (i % 2) * 2f, 4f + i * 20f), 1.6f + (i % 2) * 0.5f);

            Section1_Garden(kit);
            Section2_WickerWall(kit);
            Section3_DyePond(kit);
            Section4_ButtonBridge(kit);
            Section5_YarnSwing(kit);
            Section6_WindGap(kit);
            Section7_Finale(kit);
            return info;
        }

        // ---------------------------------------------------------------- 1. Garden start  (z -10 .. 12)
        static void Section1_Garden(LevelKit kit)
        {
            kit.Quilt(new Vector3(0f, -0.5f, 1f), new Vector3(14f, 1f, 22f), Lawn, 3.5f, "GardenLawn", true);
            kit.Hint(new Vector3(0f, 1.5f, -5f), new Vector3(14f, 4f, 9f), "The Sewing Basket Garden",
                "Collect the floating thread spools, bounce on pin-cushions, and use your forms to reach the end of the basket!");
            kit.ThreadLine(new Vector3(0f, 1f, -2f), new Vector3(0f, 1f, 6f), 4);

            // Pin-cushion bounce up to a little ledge with treats.
            kit.AddBouncePad(new Vector3(-4f, 0f, 3f));
            kit.Block(new Vector3(-4.5f, 3f, 8.5f), new Vector3(4f, 1f, 3f), LevelKit.Palette.Peach, "BounceLedge");
            kit.ThreadLine(new Vector3(-5.5f, 4.3f, 8.5f), new Vector3(-3.5f, 4.3f, 8.5f), 2);
            kit.Thread(new Vector3(-4f, 4.8f, 3f), LevelKit.Palette.Gold);

            // Garden dressing.
            kit.YarnBall(new Vector3(5f, 0f, -6f), 1.4f, new Color(0.95f, 0.55f, 0.6f));
            kit.YarnBall(new Vector3(5.2f, 0f, 4f), 1f, new Color(0.55f, 0.7f, 0.95f));
            kit.Mushroom(new Vector3(-5.5f, 0f, -7f), 1.4f, new Color(0.95f, 0.4f, 0.4f));
            kit.Mushroom(new Vector3(-6f, 0f, -4.5f), 0.9f, new Color(1f, 0.7f, 0.3f));
            kit.Thimble(new Vector3(4.6f, 0f, 9.5f), 1.2f);
            kit.Flower(new Vector3(-2.5f, 0f, -1f), LevelKit.Palette.Rose);
            kit.Flower(new Vector3(2.2f, 0f, 1.5f), LevelKit.Palette.Butter);
            kit.Flower(new Vector3(2.8f, 0f, -3.5f), LevelKit.Palette.Lavender);
            kit.Flower(new Vector3(-1.8f, 0f, 9f), LevelKit.Palette.Sky);
            kit.KnittingNeedle(new Vector3(-9f, -2f, -12f), new Vector3(-6.5f, 7f, -9f), LevelKit.Palette.Rose);
            kit.KnittingNeedle(new Vector3(9f, -2f, -11f), new Vector3(6.8f, 6.5f, -8.5f), LevelKit.Palette.Sky);
        }

        // ---------------------------------------------------------------- 2. Wicker wall with drain tunnel (z 12 .. 16)
        static void Section2_WickerWall(LevelKit kit)
        {
            var wicker = PatchworkAssets.Wicker(new Color(0.8f, 0.6f, 0.38f));
            kit.Block(new Vector3(0f, -0.5f, 14f), new Vector3(14f, 1f, 4f), LevelKit.Palette.Walnut, "TunnelFloor", PatchworkAssets.Wood(new Color(0.6f, 0.45f, 0.3f)));
            // Wall halves stretch far past the floor so it can't be walked or glided around.
            kit.Block(new Vector3(-7.4f, 4f, 14f), new Vector3(13.2f, 9f, 4f), LevelKit.Palette.Walnut, "WickerWallL", wicker);
            kit.Block(new Vector3(7.4f, 4f, 14f), new Vector3(13.2f, 9f, 4f), LevelKit.Palette.Walnut, "WickerWallR", wicker);
            kit.Block(new Vector3(0f, 4.75f, 14f), new Vector3(1.6f, 7.5f, 4f), LevelKit.Palette.Walnut, "WickerWallTop", wicker);
            kit.Block(new Vector3(0f, 8.6f, 14f), new Vector3(28.2f, 0.4f, 4.4f), LevelKit.Palette.Rose, "WallRim", PatchworkAssets.Yarn(new Color(0.85f, 0.4f, 0.5f)));
            kit.Grate(new Vector3(0f, 0.5f, 11.9f), new Vector3(1.6f, 1f, 0.2f));
            kit.Hint(new Vector3(0f, 1.5f, 9.5f), new Vector3(14f, 4f, 4f), "A Tiny Drain",
                "The wicker wall is too tall to climb, but there's a little blue drain grate. Become Water and slip right through!", HintTheme.Water);
            // Reward for squeezing through.
            kit.Thread(new Vector3(0f, 0.45f, 13.2f), LevelKit.Palette.Sky);
            kit.Thread(new Vector3(0f, 0.45f, 15f), LevelKit.Palette.Sky);
        }

        // ---------------------------------------------------------------- 3. Dye pond (z 16 .. 34)
        static void Section3_DyePond(LevelKit kit)
        {
            kit.Quilt(new Vector3(0f, -0.5f, 25f), new Vector3(14f, 1f, 18f), Pond, 3.5f, "DyePond", true);
            kit.AddCheckpoint(new Vector3(0f, 0f, 18.5f), 0f);
            kit.Hint(new Vector3(0f, 1.5f, 20f), new Vector3(14f, 4f, 4f), "The Dye Fountain",
                "Blue fountains carry Water Form up and up. A golden button waits on the high shelf!", HintTheme.Water);
            kit.Fountain(new Vector3(4.5f, 0f, 26f), 10f);
            kit.Block(new Vector3(4.5f, 7.5f, 30f), new Vector3(4f, 1f, 4f), LevelKit.Palette.Sky, "FountainShelf");
            kit.Block(new Vector3(4.5f, 3.5f, 31.2f), new Vector3(1.2f, 7f, 1.2f), LevelKit.Palette.Denim, "ShelfPillar", PatchworkAssets.Wood(new Color(0.55f, 0.42f, 0.32f)));
            kit.GoldenButton(CollectibleIds[0], new Vector3(4.5f, 9.1f, 30.5f));
            // Threads up the jet: only Water gets these.
            kit.Thread(new Vector3(4.5f, 3f, 26f), LevelKit.Palette.Sky);
            kit.Thread(new Vector3(4.5f, 5.5f, 26f), LevelKit.Palette.Sky);
            kit.Thread(new Vector3(4.5f, 8f, 26f), LevelKit.Palette.Sky);

            // Felt pond with lily-pad buttons.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-3.5f, 0.02f, 26f), new Vector3(5f, 0.02f, 7f), PatchworkAssets.Water(new Color(0.35f, 0.6f, 0.9f)));
            kit.SewingButton(new Vector3(-4.2f, 0.06f, 24.5f), 1.2f, new Color(0.45f, 0.7f, 0.4f), Vector3.zero);
            kit.SewingButton(new Vector3(-2.8f, 0.06f, 27.5f), 0.9f, new Color(0.5f, 0.75f, 0.45f), Vector3.zero);
            kit.Mushroom(new Vector3(-6f, 0f, 31f), 1.2f, new Color(0.8f, 0.45f, 0.9f));
            kit.YarnBall(new Vector3(-5.5f, 0f, 19f), 1f, new Color(0.95f, 0.85f, 0.4f));
        }

        // ---------------------------------------------------------------- 4. Button Bridge (pit z 34 .. 46, landing 46 .. 52)
        static void Section4_ButtonBridge(LevelKit kit)
        {
            kit.Hint(new Vector3(0f, 1.5f, 32.5f), new Vector3(14f, 4f, 3f), "Button Bridge",
                "Hop onto a sliding felt button and ride it across. Any form works, just time your jump!");
            kit.MovingButton(new Vector3(-2f, 0f, 36.4f), new Vector3(-2f, 0f, 43.6f), 0f, new Color(0.95f, 0.5f, 0.55f));
            kit.MovingButton(new Vector3(2f, 0f, 36.4f), new Vector3(2f, 0f, 43.6f), 0.5f, new Color(0.5f, 0.65f, 0.95f));
            kit.ThreadLine(new Vector3(-2f, 1.2f, 37.5f), new Vector3(-2f, 1.2f, 42.5f), 3);
            kit.ThreadLine(new Vector3(2f, 1.2f, 37.5f), new Vector3(2f, 1.2f, 42.5f), 3);
            kit.Cloud(new Vector3(0f, -5f, 40f), 1.8f);

            kit.Quilt(new Vector3(0f, -0.5f, 49f), new Vector3(14f, 1f, 6f), Meadow, 3f, "BridgeLanding", true);
            kit.AddCheckpoint(new Vector3(0f, 0f, 47.5f), 0f);
        }

        // ---------------------------------------------------------------- 5. Yarn swing (gap z 52 .. 67)
        static void Section5_YarnSwing(LevelKit kit)
        {
            kit.Hint(new Vector3(0f, 1.5f, 50.5f), new Vector3(14f, 4f, 3f), "Swing Across",
                "Too far to jump! As Yarn, run toward the glowing pink button and hold Ability to throw your yarn arm. Let go to fly.", HintTheme.Yarn);
            kit.Anchor(new Vector3(0f, 8f, 59.5f), 13.5f);
            // Pull the camera back over the whole gap so the full swing arc and the landing fit on screen.
            kit.AddCameraZone(new Vector3(0f, 2f, 59.5f), new Vector3(16f, 24f, 22f), 2.5f, 6f);
            kit.GoldenButton(CollectibleIds[1], new Vector3(0f, -0.4f, 59.5f));
            // Threads along the swing arc.
            float rope = 8.4f;
            float[] angles = { -50f, -28f, 28f, 50f };
            foreach (float a in angles)
            {
                float r = a * Mathf.Deg2Rad;
                kit.Thread(new Vector3(0f, 8f - rope * Mathf.Cos(r), 59.5f + rope * Mathf.Sin(r)), LevelKit.Palette.Rose);
            }
            kit.KnittingNeedle(new Vector3(-12f, -10f, 55f), new Vector3(-7f, 6f, 60f), LevelKit.Palette.Gold);
            kit.KnittingNeedle(new Vector3(12f, -10f, 64f), new Vector3(7.5f, 5f, 59f), LevelKit.Palette.Lavender);
            kit.Cloud(new Vector3(-9f, 2f, 61f), 1.4f);
        }

        // ---------------------------------------------------------------- 6. Wind gap (landing 67 .. 84, gap 84 .. 91)
        static void Section6_WindGap(LevelKit kit)
        {
            kit.Quilt(new Vector3(0f, -0.5f, 75.5f), new Vector3(14f, 1f, 17f), Lawn, 3.5f, "SwingLanding", true);
            kit.AddCheckpoint(new Vector3(0f, 0f, 70f), 0f);

            // Bounce tower: bounce straight up through a column of threads.
            kit.AddBouncePad(new Vector3(-4f, 0f, 73f), 16f);
            kit.Thread(new Vector3(-4f, 2.5f, 73f), LevelKit.Palette.Gold);
            kit.Thread(new Vector3(-4f, 4f, 73f), LevelKit.Palette.Gold);
            kit.Thread(new Vector3(-4f, 5.5f, 73f), LevelKit.Palette.Gold);

            kit.Hint(new Vector3(0f, 1.5f, 78.5f), new Vector3(14f, 4f, 7f), "Ride the Breeze",
                "Pinwheels mean wind! As Wind, hold Jump while falling to glide, and tap Ability for a gust.", HintTheme.Wind);
            kit.Pinwheel(new Vector3(-6f, 0f, 81f), LevelKit.Palette.Sky, LevelKit.Palette.Cream);
            kit.Pinwheel(new Vector3(6f, 0f, 81f), LevelKit.Palette.Rose, LevelKit.Palette.Cream);
            kit.Pinwheel(new Vector3(-6f, 0f, 92f), LevelKit.Palette.Sage, LevelKit.Palette.Cream);
            kit.Pinwheel(new Vector3(6f, 0f, 92f), LevelKit.Palette.Butter, LevelKit.Palette.Cream);
            // Threads along the glide.
            kit.Thread(new Vector3(0f, 1.6f, 85.4f), LevelKit.Palette.Cream);
            kit.Thread(new Vector3(0f, 1.2f, 87.6f), LevelKit.Palette.Cream);
            kit.Thread(new Vector3(0f, 0.9f, 89.8f), LevelKit.Palette.Cream);
            kit.Cloud(new Vector3(0f, -4f, 87.5f), 1.6f);
            kit.Mushroom(new Vector3(5.5f, 0f, 69f), 1.3f, new Color(0.4f, 0.6f, 0.95f));
            kit.YarnBall(new Vector3(5f, 0f, 76f), 1.2f, new Color(0.6f, 0.85f, 0.6f));
        }

        // ---------------------------------------------------------------- 7. Updraft shelf + goal (z 91 .. 110)
        static void Section7_Finale(LevelKit kit)
        {
            kit.Quilt(new Vector3(0f, -0.5f, 100.5f), new Vector3(14f, 1f, 19f), Meadow, 3.5f, "FinalMeadow", true);
            kit.AddCheckpoint(new Vector3(0f, 0f, 93.5f), 0f);
            kit.Updraft(new Vector3(4.5f, 0f, 96f), 12f);
            kit.Hint(new Vector3(4.5f, 1.5f, 96f), new Vector3(4f, 3f, 4f), "Updraft",
                "White updrafts lift Wind Form. Float up and glide onto the high shelf for a golden button.", HintTheme.Wind);
            kit.Block(new Vector3(4.5f, 9.5f, 101f), new Vector3(4f, 1f, 4f), LevelKit.Palette.Cream, "UpdraftShelf");
            kit.Block(new Vector3(4.5f, 4.5f, 102.4f), new Vector3(1.2f, 9f, 1.2f), LevelKit.Palette.Sage, "ShelfPillar", PatchworkAssets.Wood(new Color(0.55f, 0.42f, 0.32f)));
            kit.GoldenButton(CollectibleIds[2], new Vector3(4.5f, 11.1f, 101.5f));
            kit.Thread(new Vector3(4.5f, 4f, 96f), LevelKit.Palette.Cream);
            kit.Thread(new Vector3(4.5f, 7f, 96f), LevelKit.Palette.Cream);
            kit.Thread(new Vector3(4.5f, 10f, 96f), LevelKit.Palette.Cream);

            // A pair of celebratory bounce pads near the goal, just for fun.
            kit.AddBouncePad(new Vector3(-4f, 0f, 99f), 12f);
            kit.Thread(new Vector3(-4f, 3.2f, 99f), LevelKit.Palette.Lavender);
            kit.ThreadLine(new Vector3(0f, 1f, 95f), new Vector3(0f, 1f, 103f), 4);

            kit.Thimble(new Vector3(-5f, 0f, 94f), 1.4f);
            kit.YarnBall(new Vector3(-5.2f, 0f, 104f), 1.3f, new Color(0.9f, 0.5f, 0.8f));
            kit.Flower(new Vector3(-2.5f, 0f, 96f), LevelKit.Palette.Rose);
            kit.Flower(new Vector3(-1.5f, 0f, 104f), LevelKit.Palette.Butter);
            kit.Flower(new Vector3(2f, 0f, 104.5f), LevelKit.Palette.Lavender);
            kit.Goal(new Vector3(0f, 0f, 106.5f));
        }
    }
}

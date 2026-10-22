using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// Level 1 — "The Quilted Meadow". A straight, readable course along +Z that teaches one form at a time:
    ///   1. Start meadow  →  tall wall with a low drain-grate tunnel (Water)
    ///   2. Fountain room →  optional fountain to a high shelf (Water, golden button)
    ///   3. Big 15 m gap with a pink anchor (Yarn; optional button under the swing)
    ///   4. 9 m gap with pinwheels (Wind) and an optional updraft to a high shelf (Wind, golden button)
    ///   5. Goal archway
    /// Gap sizes are tuned against the movement numbers: normal jump ≈ 5 m, Wind glide+gust ≈ 12 m,
    /// so each challenge needs its intended form. Checkpoints sit before every challenge.
    /// </summary>
    public static class Level1Builder
    {
        public const string SceneName = "Level1";
        public static readonly string[] CollectibleIds = { "L1_fountain", "L1_swing", "L1_updraft" };

        public static LevelInfo Build(LevelKit kit)
        {
            var P = LevelKit.Palette.Quilt;
            var meadow = LevelKit.Palette.Meadow;
            var info = kit.Info;
            info.Id = SceneName;
            info.Title = "Level 1 · The Quilted Meadow";
            info.Subtitle = "Water, Yarn and Wind: you'll need all three.";
            info.Spawn = new Vector3(0f, 0.1f, -5f);
            info.SpawnYaw = 0f;
            info.KillY = -12f;
            info.Sky = new Color(0.8f, 0.87f, 0.96f);

            kit.Backdrop(-40f);
            // Enclose the course in a big quilted room so it feels cosy and focused.
            kit.Enclosure(new Vector3(-26f, -45f, -22f), new Vector3(26f, 32f, 108f),
                new[] { LevelKit.Palette.Denim, LevelKit.Palette.Sage, LevelKit.Palette.Butter, LevelKit.Palette.Sky },
                new Color(0.62f, 0.55f, 0.78f));

            // ---------------------------------------------------------------- 1. Start meadow
            kit.Quilt(new Vector3(0f, -0.5f, 2f), new Vector3(14f, 1f, 20f), meadow, 3.5f, "StartMeadow");
            kit.Hint(new Vector3(0f, 1.5f, 0f), new Vector3(14f, 4f, 16f), "Patch's First Stitch",
                "Open the Form Wheel to change shape. The wall ahead only has a tiny drain grate... what could slip through?");
            kit.YarnTree(new Vector3(-5f, 0f, -6f), 3.2f, LevelKit.Palette.Sage);
            kit.YarnTree(new Vector3(5.2f, 0f, -3f), 2.6f, LevelKit.Palette.Rose);
            kit.Spool(new Vector3(4.6f, 0f, 6.5f), 1.4f, LevelKit.Palette.Sky);
            kit.Flower(new Vector3(-3f, 0f, -1f), LevelKit.Palette.Rose);
            kit.Flower(new Vector3(-2.2f, 0f, 1.5f), LevelKit.Palette.Butter);
            kit.Flower(new Vector3(2.5f, 0f, 3f), LevelKit.Palette.Lavender);
            kit.SewingButton(new Vector3(-4.5f, 0.05f, 4f), 1.6f, LevelKit.Palette.Peach, Vector3.zero);

            // ---------------------------------------------------------------- 2. The grate wall (Water)
            // Floor under the wall/tunnel.
            kit.Block(new Vector3(0f, -0.5f, 14f), new Vector3(14f, 1f, 4f), LevelKit.Palette.Walnut, "TunnelFloor");
            // Wall halves extend far past the floor so it can't be walked or glided around.
            kit.Block(new Vector3(-7.4f, 4f, 14f), new Vector3(13.2f, 9f, 4f), LevelKit.Palette.Lavender, "WallLeft");
            kit.Block(new Vector3(7.4f, 4f, 14f), new Vector3(13.2f, 9f, 4f), LevelKit.Palette.Lavender, "WallRight");
            // Above the 1.6 m × 1.0 m tunnel.
            kit.Block(new Vector3(0f, 4.75f, 14f), new Vector3(1.6f, 7.5f, 4f), LevelKit.Palette.Peach, "WallTop");
            kit.Grate(new Vector3(0f, 0.5f, 11.9f), new Vector3(1.6f, 1f, 0.2f));
            // Stitched trim along the top of the wall.
            kit.Block(new Vector3(0f, 8.6f, 14f), new Vector3(28.2f, 0.3f, 4.3f), LevelKit.Palette.Rose, "WallTrim");

            // ---------------------------------------------------------------- 3. Fountain room (Water, optional)
            kit.Quilt(new Vector3(0f, -0.5f, 25f), new Vector3(14f, 1f, 18f), P, 3.5f, "FountainRoom");
            kit.AddCheckpoint(new Vector3(0f, 0f, 18.5f), 0f);
            kit.Hint(new Vector3(0f, 1.5f, 20f), new Vector3(14f, 4f, 5f), "Fountains",
                "Blue fountains carry Water Form upward. A golden button waits on the high shelf to the right.");
            kit.Fountain(new Vector3(4.5f, 0f, 26f), 10f);
            kit.Block(new Vector3(4.5f, 7.5f, 30f), new Vector3(4f, 1f, 4f), LevelKit.Palette.Sky, "FountainShelf");
            kit.Block(new Vector3(4.5f, 3.5f, 31.2f), new Vector3(1.2f, 7f, 1.2f), LevelKit.Palette.Denim, "ShelfPillar");
            kit.GoldenButton(CollectibleIds[0], new Vector3(4.5f, 9.1f, 30.5f));
            kit.Spool(new Vector3(-5f, 0f, 24f), 1.6f, LevelKit.Palette.Butter);
            kit.YarnTree(new Vector3(-5.5f, 0f, 29f), 3f, LevelKit.Palette.Lavender);

            // ---------------------------------------------------------------- 4. Yarn gap (15 m)
            kit.AddCheckpoint(new Vector3(0f, 0f, 31f), 0f);
            kit.Hint(new Vector3(0f, 1.5f, 31.5f), new Vector3(14f, 4f, 5f), "Swing Across",
                "Too far to jump! In Yarn Form, run toward the glowing pink button and hold Ability to swing. Let go to fly.");
            kit.Anchor(new Vector3(0f, 8f, 41.5f), 13.5f);
            kit.GoldenButton(CollectibleIds[1], new Vector3(0f, -0.4f, 41.5f));
            kit.Cloud(new Vector3(-9f, 3f, 42f), 1.4f);
            kit.Cloud(new Vector3(10f, 6f, 38f), 1.1f);

            // ---------------------------------------------------------------- 5. Wind gap (9 m) and updraft
            kit.Quilt(new Vector3(0f, -0.5f, 56.5f), new Vector3(14f, 1f, 15f), meadow, 3.5f, "SwingLanding");
            kit.AddCheckpoint(new Vector3(0f, 0f, 52f), 0f);
            kit.Hint(new Vector3(0f, 1.5f, 60f), new Vector3(14f, 4f, 7f), "Ride the Breeze",
                "Pinwheels mean wind! In Wind Form hold Jump while falling to glide, and tap Ability for a gust.");
            kit.Pinwheel(new Vector3(-6f, 0f, 63f), LevelKit.Palette.Sky, LevelKit.Palette.Cream);
            kit.Pinwheel(new Vector3(6f, 0f, 63f), LevelKit.Palette.Rose, LevelKit.Palette.Cream);
            kit.Pinwheel(new Vector3(-6f, 0f, 74f), LevelKit.Palette.Sage, LevelKit.Palette.Cream);
            kit.Pinwheel(new Vector3(6f, 0f, 74f), LevelKit.Palette.Butter, LevelKit.Palette.Cream);
            kit.Cloud(new Vector3(0f, -4f, 68.5f), 1.6f);

            kit.Quilt(new Vector3(0f, -0.5f, 82.5f), new Vector3(14f, 1f, 19f), P, 3.5f, "FinalMeadow");
            kit.AddCheckpoint(new Vector3(0f, 0f, 75.5f), 0f);
            kit.Updraft(new Vector3(4.5f, 0f, 78f), 12f);
            kit.Hint(new Vector3(4.5f, 1.5f, 78f), new Vector3(4f, 3f, 4f), "Updraft",
                "White updrafts lift Wind Form. Glide onto the high shelf for a golden button.");
            kit.Block(new Vector3(4.5f, 9.5f, 83f), new Vector3(4f, 1f, 4f), LevelKit.Palette.Cream, "UpdraftShelf");
            kit.Block(new Vector3(4.5f, 4.5f, 84.4f), new Vector3(1.2f, 9f, 1.2f), LevelKit.Palette.Sage, "ShelfPillar");
            kit.GoldenButton(CollectibleIds[2], new Vector3(4.5f, 11.1f, 83.5f));
            kit.YarnTree(new Vector3(-5f, 0f, 80f), 3.4f, LevelKit.Palette.Peach);
            kit.Flower(new Vector3(-3f, 0f, 77f), LevelKit.Palette.Lavender);
            kit.Flower(new Vector3(-1.5f, 0f, 84f), LevelKit.Palette.Rose);

            // ---------------------------------------------------------------- 6. Goal
            kit.Goal(new Vector3(0f, 0f, 88.5f));

            return info;
        }
    }
}

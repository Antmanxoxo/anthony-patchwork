using UnityEngine;

namespace Patchwork
{
    // Level 1 - The Sewing Basket Garden. A felt garden inside a giant wicker basket, climbing all
    // the way out over the basket's rim. Built as "introduce, develop, twist, finish":
    //  1. Garden Start      - stretch your legs, pin-cushion bounce, first threads
    //  2. The Tiny Drain    - Water through a grate in the wicker wall
    //  3. Dye Pond          - Water up a fountain; a drain hut on the shelf hides a golden button
    //  4. Button Bridge     - ride sliding buttons over a pit (any form)
    //  5. Pincushion Canyon - two Yarn swings in a row, with a pincushion island between them
    //  6. Breezy Meadow     - Wind: a 7 m glide, then an 11 m gap that needs the gust;
    //                         a secret updraft up to a spool tower hides the third button
    //  7. Over the Wall     - an updraft lifts Wind up and over a tall wicker wall
    //  8. The Basket Rim    - the finale uses all three: fountain (Water), glide down to a giant
    //                         thimble (Wind), swing to the rim (Yarn)
    // Gap sizes come from the movement numbers: normal jump ~5 m, Wind glide alone ~8 m,
    // Wind glide + gust ~13.4 m, a Yarn swing crosses 15 m.
    // All floors are at y = 0 unless they're a raised ledge; pits drop away below y = -3.
    public static class Level1Builder
    {
        public const string SceneName = "Level1";
        // Same ids as before so old saves keep their buttons.
        //   L1_fountain: inside the drain hut on the fountain shelf (stay Water at the top)
        //   L1_swing:    at the bottom of the first swing (teaches you to look down)
        //   L1_updraft:  on top of the spool tower in the Breezy Meadow (off the main path, it hums)
        public static readonly string[] CollectibleIds = { "L1_fountain", "L1_swing", "L1_updraft" };

        // Garden palettes
        static readonly Color[] Lawn = { new Color(0.55f, 0.75f, 0.45f), new Color(0.48f, 0.68f, 0.42f), new Color(0.62f, 0.8f, 0.5f), new Color(0.5f, 0.72f, 0.5f) };
        static readonly Color[] Meadow = { new Color(0.6f, 0.78f, 0.48f), new Color(0.72f, 0.82f, 0.5f), new Color(0.52f, 0.7f, 0.46f), new Color(0.95f, 0.88f, 0.6f) };
        static readonly Color[] Pond = { new Color(0.6f, 0.78f, 0.9f), new Color(0.55f, 0.72f, 0.88f), new Color(0.7f, 0.85f, 0.92f), new Color(0.62f, 0.8f, 0.6f) };
        static readonly Color[] Canyon = { new Color(0.86f, 0.55f, 0.55f), new Color(0.92f, 0.7f, 0.6f), new Color(0.8f, 0.6f, 0.65f), new Color(0.95f, 0.82f, 0.68f) };
        static readonly Color[] Breezy = { new Color(0.75f, 0.88f, 0.78f), new Color(0.82f, 0.9f, 0.72f), new Color(0.7f, 0.84f, 0.8f), new Color(0.92f, 0.9f, 0.74f) };
        static readonly Color[] Rim = { new Color(0.95f, 0.85f, 0.6f), new Color(0.9f, 0.75f, 0.55f), new Color(0.98f, 0.9f, 0.7f), new Color(0.85f, 0.7f, 0.5f) };
        static readonly Color[] SpoolThreads = { LevelKit.Palette.Rose, LevelKit.Palette.Sky, LevelKit.Palette.Butter, LevelKit.Palette.Sage, LevelKit.Palette.Lavender };

        // Key z positions.
        const float PitStart = 34f, BridgeLanding = 46f;
        const float SwingAStart = 52f, IslandStart = 67f, SwingBStart = 73f, MeadowStart = 88f;
        const float GlideGapStart = 104f, PlatformB = 111f, GustGapStart = 121f, PlatformC = 132f;
        const float WallZ = 143f, PlatformD = 144f, RimLedgeStart = 154f;
        const float ThimbleZ = 173f, ThimbleSize = 6f, GoalLedgeStart = 190.5f;

        public static LevelInfo Build(LevelKit kit)
        {
            var info = kit.Info;
            info.Id = SceneName;
            info.Title = "Level 1 · The Sewing Basket Garden";
            info.Subtitle = "Climb out of the sewing basket: across the dye pond, through Pincushion Canyon, over the Breezy Meadow and up to the rim.";
            info.Spawn = new Vector3(0f, 0.1f, -6f);
            info.SpawnYaw = 0f;
            info.KillY = -14f;
            info.Sky = new Color(0.78f, 0.88f, 0.95f);

            // Below the course: a lake of swirling paint and ink (the gloopy sounds you hear).
            PaintLake(kit);
            // A giant wicker basket around the whole course, with a soft blue felt "sky" lid.
            kit.Enclosure(new Vector3(-26f, -45f, -24f), new Vector3(26f, 34f, 214f),
                new[] { new Color(0.62f, 0.45f, 0.28f), new Color(0.75f, 0.56f, 0.34f), new Color(0.82f, 0.64f, 0.4f), new Color(0.88f, 0.72f, 0.48f) },
                new Color(0.62f, 0.78f, 0.95f), true);
            kit.FeltSun(new Vector3(0f, 24f, 213.6f), 6f, Vector3.back);
            for (int i = 0; i < 11; i++)
                kit.Cloud(new Vector3(-16f + (i % 3) * 16f, 27f - (i % 2) * 2.5f, 4f + i * 19f), 1.6f + (i % 2) * 0.5f);

            Section1_Garden(kit);
            Section2_WickerWall(kit);
            Section3_DyePond(kit);
            Section4_ButtonBridge(kit);
            Section5_PincushionCanyon(kit);
            Section6_BreezyMeadow(kit);
            Section7_OverTheWall(kit);
            Section8_BasketRim(kit);
            YarnBirds(kit);
            return info;
        }

        // ---------------------------------------------------------------- The paint lake

        static void PaintLake(LevelKit kit)
        {
            kit.PaintLake(new Vector3(0f, -8f, 95f), new Vector2(52f, 238f), 13f);

            // Art supplies bobbing in the paint.
            PaintTube(kit, new Vector3(3.5f, -8f, 40f), new Color(0.95f, 0.45f, 0.55f), 30f);
            InkBottle(kit, new Vector3(-5f, -8f, 38f), -12f);
            Brush(kit, new Vector3(-4f, -8f, 60f), new Color(0.35f, 0.6f, 0.95f), 40f);
            PaintTube(kit, new Vector3(5f, -8f, 76f), new Color(0.98f, 0.82f, 0.35f), -50f);
            InkBottle(kit, new Vector3(-3f, -8f, 84f), 8f);
            PaintTube(kit, new Vector3(2f, -8f, 107f), new Color(0.4f, 0.75f, 0.6f), 75f);
            InkBottle(kit, new Vector3(4.5f, -8f, 126f), -20f);
            Brush(kit, new Vector3(-3f, -8f, 167f), new Color(0.95f, 0.5f, 0.6f), -30f);
            InkBottle(kit, new Vector3(-5f, -8f, 179f), 15f);
            PaintTube(kit, new Vector3(4f, -8f, 185f), new Color(0.6f, 0.5f, 0.9f), 10f);
            // Along the sides of the basket.
            PaintTube(kit, new Vector3(-17f, -8f, 8f), new Color(0.35f, 0.6f, 0.95f), 100f);
            Brush(kit, new Vector3(16f, -8f, 24f), new Color(0.98f, 0.82f, 0.35f), 150f);
            InkBottle(kit, new Vector3(18f, -8f, 55f), 0f);
            PaintTube(kit, new Vector3(-16f, -8f, 70f), new Color(0.95f, 0.45f, 0.55f), -140f);
            Brush(kit, new Vector3(-18f, -8f, 100f), new Color(0.4f, 0.75f, 0.6f), 60f);
            PaintTube(kit, new Vector3(17f, -8f, 115f), new Color(0.98f, 0.6f, 0.3f), 20f);
            InkBottle(kit, new Vector3(-17f, -8f, 150f), -30f);
            Brush(kit, new Vector3(16f, -8f, 160f), new Color(0.6f, 0.5f, 0.9f), -100f);
            PaintTube(kit, new Vector3(-15f, -8f, 196f), new Color(0.4f, 0.75f, 0.6f), 160f);
        }

        // A squeezed tube of paint floating on its side, with a blob coming out of it.
        static void PaintTube(LevelKit kit, Vector3 pos, Color paint, float yaw)
        {
            var g = kit.Group("PaintTube", pos + Vector3.up * 0.5f);
            g.rotation = Quaternion.Euler(0f, yaw, 0f);
            var metal = PatchworkAssets.Plain(new Color(0.88f, 0.88f, 0.92f), 0.85f);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.3f, 2f, 1.1f), metal, new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.36f, 1.1f, 1.16f), PatchworkAssets.Plain(paint, 0.4f), new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(-2.1f, 0f, 0f), new Vector3(0.3f, 0.3f, 1.4f), metal, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(2.15f, 0f, 0f), new Vector3(0.55f, 0.25f, 0.55f), PatchworkAssets.Plain(new Color(0.15f, 0.12f, 0.15f), 0.4f), new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(2.9f, -0.2f, 0f), new Vector3(1.4f, 0.5f, 1.1f), PatchworkAssets.Plain(paint, 0.9f), default(Vector3), g);
            var bob = g.gameObject.AddComponent<Spinner>();
            bob.Speed = 2f;
            bob.BobHeight = 0.18f;
            bob.BobSpeed = 0.7f;
        }

        // A bottle of ink, tilted and half sunk.
        static void InkBottle(LevelKit kit, Vector3 pos, float tilt)
        {
            var g = kit.Group("InkBottle", pos);
            g.rotation = Quaternion.Euler(tilt, tilt * 2f, tilt * 0.5f);
            var glass = PatchworkAssets.Plain(new Color(0.12f, 0.13f, 0.3f), 0.95f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.6f, 0f), new Vector3(2.4f, 1.6f, 2.4f), glass, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 2.5f, 0f), new Vector3(1.1f, 0.4f, 1.1f), glass, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 3.05f, 0f), new Vector3(0.95f, 0.25f, 0.95f), PatchworkAssets.Felt(new Color(0.65f, 0.48f, 0.32f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1f, 0f), new Vector3(2.46f, 0.5f, 2.46f), PatchworkAssets.Felt(LevelKit.Palette.Cream), default(Vector3), g);
            var bob = g.gameObject.AddComponent<Spinner>();
            bob.Speed = 0f;
            bob.BobHeight = 0.12f;
            bob.BobSpeed = 0.5f;
        }

        // A big paintbrush sticking up out of the paint at an angle.
        static void Brush(LevelKit kit, Vector3 pos, Color paint, float yaw)
        {
            var g = kit.Group("Paintbrush", pos);
            g.rotation = Quaternion.Euler(0f, yaw, 0f) * Quaternion.Euler(0f, 0f, 30f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 4f, 0f), new Vector3(0.45f, 3.5f, 0.45f), PatchworkAssets.Plain(new Color(0.85f, 0.3f, 0.3f), 0.6f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.3f, 0f), new Vector3(0.6f, 0.6f, 0.6f), PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.9f), 0.9f), default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.4f, 0f), new Vector3(0.7f, 1.2f, 0.7f), PatchworkAssets.Plain(paint, 0.85f), default(Vector3), g);
        }

        // ---------------------------------------------------------------- Yarn birds

        static void YarnBirds(LevelKit kit)
        {
            Color[] c = { LevelKit.Palette.Rose, LevelKit.Palette.Sky, LevelKit.Palette.Butter, LevelKit.Palette.Lavender, LevelKit.Palette.Peach };
            // Garden
            kit.YarnBird(new Vector3(2.5f, 0f, -2f), c[0], false);
            kit.YarnBird(new Vector3(3.5f, 0f, 5f), c[1], false);
            kit.YarnBird(new Vector3(-4f, 3.5f, 9.2f), c[2], false);
            kit.YarnBird(new Vector3(0f, 7f, 2f), c[3], true, 5f);
            // On the wicker wall's rim
            kit.YarnBird(new Vector3(-3.5f, 8.8f, 14f), c[4], false);
            kit.YarnBird(new Vector3(4.5f, 8.8f, 14.6f), c[0], false);
            // Dye pond: one on the hut roof (a hint to look up there), one circling
            kit.YarnBird(new Vector3(4.2f, 10.9f, 34.5f), c[1], false);
            kit.YarnBird(new Vector3(-3f, 6f, 26f), c[2], true, 4f);
            // Canyon
            kit.YarnBird(new Vector3(0f, 12f, 62f), c[3], true, 6f);
            kit.YarnBird(new Vector3(2.5f, 0f, 70.5f), c[4], false);
            kit.YarnBird(new Vector3(-3f, 11f, 80f), c[0], true, 5f);
            // Meadow, and one on top of the spool tower beside the secret button
            kit.YarnBird(new Vector3(4f, 0f, 96f), c[1], false);
            kit.YarnBird(new Vector3(-2f, 9f, 108f), c[2], true, 6f);
            kit.YarnBird(new Vector3(-10.2f, 12f, 118.2f), c[3], false);
            // Over the wall
            kit.YarnBird(new Vector3(-6f, 9.3f, 143f), c[4], false);
            kit.YarnBird(new Vector3(7.5f, 9.3f, 143.3f), c[0], false);
            // The rim
            kit.YarnBird(new Vector3(3f, 6f, 198f), c[1], false);
            kit.YarnBird(new Vector3(-3f, 6f, 192f), c[2], false);
            kit.YarnBird(new Vector3(0f, 14f, 196f), c[3], true, 5f);
            kit.YarnBird(new Vector3(-6f, 18f, 175f), c[4], true, 7f);
        }

        // ---------------------------------------------------------------- 1. Garden start  (z -10 .. 12)
        static void Section1_Garden(LevelKit kit)
        {
            kit.Quilt(new Vector3(0f, -0.5f, 1f), new Vector3(14f, 1f, 22f), Lawn, 3.5f, "GardenLawn", true);
            kit.Hint(new Vector3(0f, 1.5f, -5f), new Vector3(14f, 4f, 9f), "The Sewing Basket Garden",
                "Climb out of the basket! Grab threads and golden buttons on the way. Bounce on pin-cushions with {Jump}.");
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
            kit.Flower(new Vector3(6f, 0f, -1f), LevelKit.Palette.Peach);
            kit.Flower(new Vector3(-6.2f, 0f, 1.5f), LevelKit.Palette.Butter);
            kit.KnittingNeedle(new Vector3(-9f, -2f, -12f), new Vector3(-6.5f, 7f, -9f), LevelKit.Palette.Rose);
            kit.KnittingNeedle(new Vector3(9f, -2f, -11f), new Vector3(6.8f, 6.5f, -8.5f), LevelKit.Palette.Sky);
            kit.SewingPin(new Vector3(6.4f, 0f, -9f), 3.5f, LevelKit.Palette.Gold, new Vector3(-15f, 0f, 20f));
            kit.SewingButton(new Vector3(-3.5f, 0.05f, -8.5f), 1.6f, LevelKit.Palette.Lavender, Vector3.zero);
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
            kit.Block(new Vector3(0f, 8.6f, 14f), new Vector3(28.2f, 0.4f, 4.4f), LevelKit.Palette.Rose, "WallRim", PatchworkAssets.Yarn(new Color(0.85f, 0.5f, 0.55f)));
            kit.Grate(new Vector3(0f, 0.5f, 11.9f), new Vector3(1.6f, 1f, 0.2f));
            kit.Hint(new Vector3(0f, 1.5f, 9.5f), new Vector3(14f, 4f, 4f), "A Tiny Drain",
                "Too tall to climb! Open the Form Wheel with {Wheel}, become Water, and slip through the blue drain.", HintTheme.Water);
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
                "Blue fountains carry Water Form up and up. What's hiding on the high shelf?", HintTheme.Water);
            kit.Fountain(new Vector3(4.5f, 0f, 26f), 10f);
            // The shelf (top at 8 m) runs back over the pit, with a little stitched hut on it.
            kit.Block(new Vector3(4.5f, 7.5f, 32f), new Vector3(4f, 1f, 8f), LevelKit.Palette.Sky, "FountainShelf");
            kit.Block(new Vector3(4.5f, 3.5f, 31f), new Vector3(1.2f, 7f, 1.2f), LevelKit.Palette.Denim, "ShelfPillar", PatchworkAssets.Wood(new Color(0.55f, 0.42f, 0.32f)));
            DrainHut(kit, new Vector3(4.5f, 8f, 32.2f));
            kit.GoldenButton(CollectibleIds[0], new Vector3(4.5f, 9.1f, 34.2f));
            // Threads up the jet and along the shelf point the way to the hut.
            kit.Thread(new Vector3(4.5f, 3f, 26f), LevelKit.Palette.Sky);
            kit.Thread(new Vector3(4.5f, 5.5f, 26f), LevelKit.Palette.Sky);
            kit.Thread(new Vector3(4.5f, 8f, 26f), LevelKit.Palette.Sky);
            kit.Thread(new Vector3(4.5f, 8.5f, 29f), LevelKit.Palette.Sky);
            kit.Thread(new Vector3(4.5f, 8.5f, 30.6f), LevelKit.Palette.Sky);

            // Felt pond with lily-pad buttons.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-3.5f, 0.02f, 26f), new Vector3(5f, 0.02f, 7f), PatchworkAssets.Water(new Color(0.35f, 0.6f, 0.9f)));
            kit.SewingButton(new Vector3(-4.2f, 0.06f, 24.5f), 1.2f, new Color(0.45f, 0.7f, 0.4f), Vector3.zero);
            kit.SewingButton(new Vector3(-2.8f, 0.06f, 27.5f), 0.9f, new Color(0.5f, 0.75f, 0.45f), Vector3.zero);
            kit.Mushroom(new Vector3(-6f, 0f, 31f), 1.2f, new Color(0.8f, 0.45f, 0.9f));
            kit.YarnBall(new Vector3(-5.5f, 0f, 19f), 1f, new Color(0.95f, 0.85f, 0.4f));
            kit.Flower(new Vector3(-1f, 0f, 30.5f), LevelKit.Palette.Lavender);
            kit.Flower(new Vector3(1.5f, 0f, 21.5f), LevelKit.Palette.Rose);
        }

        // A tiny stitched hut on the fountain shelf. Its only door is a drain grate, so you have to
        // still be Water when you reach the top. The front wall faces -z at frontBase.z.
        static void DrainHut(LevelKit kit, Vector3 frontBase)
        {
            float x = frontBase.x, y = frontBase.y, z = frontBase.z;
            var felt = LevelKit.Palette.Peach;
            kit.Block(new Vector3(x - 1.35f, y + 1.25f, z), new Vector3(1.3f, 2.5f, 0.4f), felt, "HutFrontL");
            kit.Block(new Vector3(x + 1.35f, y + 1.25f, z), new Vector3(1.3f, 2.5f, 0.4f), felt, "HutFrontR");
            kit.Block(new Vector3(x, y + 1.75f, z), new Vector3(1.4f, 1.5f, 0.4f), felt, "HutFrontTop");
            kit.Grate(new Vector3(x, y + 0.5f, z), new Vector3(1.4f, 1f, 0.2f));
            kit.Block(new Vector3(x - 1.8f, y + 1.25f, z + 1.9f), new Vector3(0.4f, 2.5f, 3.8f), felt, "HutSide");
            kit.Block(new Vector3(x + 1.8f, y + 1.25f, z + 1.9f), new Vector3(0.4f, 2.5f, 3.8f), felt, "HutSide");
            kit.Block(new Vector3(x, y + 1.25f, z + 3.6f), new Vector3(3.6f, 2.5f, 0.4f), felt, "HutBack");
            kit.Block(new Vector3(x, y + 2.7f, z + 1.8f), new Vector3(4.4f, 0.4f, 4.4f), LevelKit.Palette.Rose, "HutRoof");
        }

        // ---------------------------------------------------------------- 4. Button Bridge (pit 34 .. 46, landing 46 .. 52)
        static void Section4_ButtonBridge(LevelKit kit)
        {
            kit.Hint(new Vector3(0f, 1.5f, 32.5f), new Vector3(14f, 4f, 3f), "Button Bridge",
                "Hop onto a sliding felt button and ride it across. Any form works, just time your {Jump}!");
            kit.MovingButton(new Vector3(-2f, 0f, PitStart + 2.4f), new Vector3(-2f, 0f, BridgeLanding - 2.4f), 0f, new Color(0.95f, 0.5f, 0.55f));
            kit.MovingButton(new Vector3(2f, 0f, PitStart + 2.4f), new Vector3(2f, 0f, BridgeLanding - 2.4f), 0.5f, new Color(0.5f, 0.65f, 0.95f));
            kit.ThreadLine(new Vector3(-2f, 1.2f, 37.5f), new Vector3(-2f, 1.2f, 42.5f), 3);
            kit.ThreadLine(new Vector3(2f, 1.2f, 37.5f), new Vector3(2f, 1.2f, 42.5f), 3);
            kit.Cloud(new Vector3(0f, -5f, 40f), 1.8f);
            // Giant pins stuck in the pit walls.
            kit.SewingPin(new Vector3(-7f, -6f, 37f), 9f, LevelKit.Palette.Rose, new Vector3(10f, 0f, -18f));
            kit.SewingPin(new Vector3(7f, -6f, 43f), 9f, LevelKit.Palette.Gold, new Vector3(-10f, 0f, 18f));

            kit.Quilt(new Vector3(0f, -0.5f, (BridgeLanding + SwingAStart) * 0.5f), new Vector3(14f, 1f, SwingAStart - BridgeLanding), Meadow, 3f, "BridgeLanding", true);
            kit.AddCheckpoint(new Vector3(0f, 0f, 47.5f), 0f);
            kit.Flower(new Vector3(-5.5f, 0f, 50f), LevelKit.Palette.Butter);
            kit.Flower(new Vector3(5.8f, 0f, 47f), LevelKit.Palette.Lavender);
        }

        // ---------------------------------------------------------------- 5. Pincushion Canyon (two swings, 52 .. 88)
        static void Section5_PincushionCanyon(LevelKit kit)
        {
            kit.Hint(new Vector3(0f, 1.5f, 50.5f), new Vector3(14f, 4f, 3f), "Pincushion Canyon",
                "Too far to jump! As Yarn, run at the glowing pink button and hold {Ability} to swing. Let go to fly.", HintTheme.Yarn);

            // Swing A over the first gap.
            float anchorA = (SwingAStart + IslandStart) * 0.5f;
            kit.Anchor(new Vector3(0f, 8f, anchorA), 13.5f);
            kit.AddCameraZone(new Vector3(0f, 2f, anchorA), new Vector3(16f, 24f, 22f), 2.5f, 6f);
            kit.GoldenButton(CollectibleIds[1], new Vector3(0f, -0.4f, anchorA));
            SwingArcThreads(kit, 8f, anchorA);

            // The pincushion island between the swings.
            kit.Quilt(new Vector3(0f, -0.5f, (IslandStart + SwingBStart) * 0.5f), new Vector3(10f, 1f, SwingBStart - IslandStart), Canyon, 2.5f, "PincushionIsland");
            kit.AddCheckpoint(new Vector3(-2.5f, 0f, 69f), 0f);
            kit.Pincushion(new Vector3(9.5f, -1f, 70f), 3.5f, new Color(0.9f, 0.35f, 0.35f));
            kit.Hint(new Vector3(0f, 1.5f, 71.5f), new Vector3(10f, 4f, 3f), "Again!",
                "Chain your swings: let go, land, and grab the next pink button with {Ability}.", HintTheme.Yarn);

            // Swing B over the second gap.
            float anchorB = (SwingBStart + MeadowStart) * 0.5f;
            kit.Anchor(new Vector3(0f, 8f, anchorB), 13.5f);
            kit.AddCameraZone(new Vector3(0f, 2f, anchorB), new Vector3(16f, 24f, 22f), 2.5f, 6f);
            SwingArcThreads(kit, 8f, anchorB);

            // Canyon dressing: knitting needles and giant pins poking out of the depths.
            kit.KnittingNeedle(new Vector3(-12f, -10f, 55f), new Vector3(-7f, 6f, 60f), LevelKit.Palette.Gold);
            kit.KnittingNeedle(new Vector3(12f, -10f, 64f), new Vector3(7.5f, 5f, 59f), LevelKit.Palette.Lavender);
            kit.KnittingNeedle(new Vector3(-12f, -10f, 84f), new Vector3(-7.5f, 5.5f, 79f), LevelKit.Palette.Sky);
            kit.SewingPin(new Vector3(8f, -8f, 82f), 12f, LevelKit.Palette.Rose, new Vector3(-8f, 0f, 14f));
            kit.SewingPin(new Vector3(-8.5f, -8f, 66f), 10f, LevelKit.Palette.Butter, new Vector3(6f, 0f, -12f));
            kit.Cloud(new Vector3(-9f, 2f, 61f), 1.4f);
            kit.Cloud(new Vector3(9f, 1f, 81f), 1.2f);
        }

        static void SwingArcThreads(LevelKit kit, float anchorY, float anchorZ)
        {
            const float rope = 8.4f;
            float[] angles = { -50f, -28f, 28f, 50f };
            foreach (float a in angles)
            {
                float r = a * Mathf.Deg2Rad;
                kit.Thread(new Vector3(0f, anchorY - rope * Mathf.Cos(r), anchorZ + rope * Mathf.Sin(r)), LevelKit.Palette.Rose);
            }
        }

        // ---------------------------------------------------------------- 6. Breezy Meadow (Wind, 88 .. 132)
        static void Section6_BreezyMeadow(LevelKit kit)
        {
            // Landing meadow after the canyon.
            kit.Quilt(new Vector3(0f, -0.5f, (MeadowStart + GlideGapStart) * 0.5f), new Vector3(14f, 1f, GlideGapStart - MeadowStart), Lawn, 3.5f, "SwingLanding", true);
            kit.AddCheckpoint(new Vector3(0f, 0f, 91f), 0f);
            kit.AddBouncePad(new Vector3(-4f, 0f, 94f), 16f);
            kit.Thread(new Vector3(-4f, 2.5f, 94f), LevelKit.Palette.Gold);
            kit.Thread(new Vector3(-4f, 4f, 94f), LevelKit.Palette.Gold);
            kit.Thread(new Vector3(-4f, 5.5f, 94f), LevelKit.Palette.Gold);
            kit.Mushroom(new Vector3(5.5f, 0f, 92f), 1.3f, new Color(0.4f, 0.6f, 0.95f));
            kit.YarnBall(new Vector3(5f, 0f, 98f), 1.2f, new Color(0.6f, 0.85f, 0.6f));

            kit.Hint(new Vector3(0f, 1.5f, 100.5f), new Vector3(14f, 4f, 6f), "Ride the Breeze",
                "Pinwheels mean wind! As Wind, hold {Jump} while falling to glide across.", HintTheme.Wind);
            kit.Pinwheel(new Vector3(-6f, 0f, 102.5f), LevelKit.Palette.Sky, LevelKit.Palette.Cream);
            kit.Pinwheel(new Vector3(6f, 0f, 102.5f), LevelKit.Palette.Rose, LevelKit.Palette.Cream);
            // 7 m glide gap.
            kit.Thread(new Vector3(0f, 1.6f, 105.4f), LevelKit.Palette.Cream);
            kit.Thread(new Vector3(0f, 1.2f, 107.6f), LevelKit.Palette.Cream);
            kit.Thread(new Vector3(0f, 0.9f, 109.8f), LevelKit.Palette.Cream);
            kit.Cloud(new Vector3(0f, -4f, 107.5f), 1.6f);

            // Platform B, then an 11 m gap that needs the gust.
            kit.Quilt(new Vector3(0f, -0.5f, (PlatformB + GustGapStart) * 0.5f), new Vector3(14f, 1f, GustGapStart - PlatformB), Breezy, 3.5f, "BreezyB", true);
            kit.AddCheckpoint(new Vector3(0f, 0f, 112.5f), 0f);
            kit.Hint(new Vector3(0f, 1.5f, 118.5f), new Vector3(14f, 4f, 5f), "Gust!",
                "Too far to glide! Jump, glide, then tap {Ability} for a burst of speed.", HintTheme.Wind);
            kit.Pinwheel(new Vector3(-6f, 0f, 120f), LevelKit.Palette.Sage, LevelKit.Palette.Cream);
            kit.Pinwheel(new Vector3(6f, 0f, 120f), LevelKit.Palette.Butter, LevelKit.Palette.Cream);
            kit.ThreadLine(new Vector3(0f, 1.6f, 123f), new Vector3(0f, 0.8f, 130f), 4);
            kit.MeasuringTape(new Vector3(6.2f, 0f, 89f), new Vector3(6.2f, 0f, 103.5f));

            // Secret: a side updraft up to a tall spool tower. The button up there hums.
            kit.Updraft(new Vector3(-5f, 0f, 117.5f), 14f);
            kit.SpoolTower(new Vector3(-11f, -6f, 119f), 18f, SpoolThreads);
            var secret = kit.GoldenButton(CollectibleIds[2], new Vector3(-11f, 13.3f, 119f));
            secret.Hums = true;
            kit.Thread(new Vector3(-5f, 6f, 117.5f), LevelKit.Palette.Cream);
            kit.Thread(new Vector3(-5f, 10f, 117.5f), LevelKit.Palette.Cream);
        }

        // ---------------------------------------------------------------- 7. Over the Wall (132 .. 144)
        static void Section7_OverTheWall(LevelKit kit)
        {
            kit.Quilt(new Vector3(0f, -0.5f, (PlatformC + PlatformD) * 0.5f), new Vector3(14f, 1f, PlatformD - PlatformC), Meadow, 3.5f, "MeadowC", true);
            kit.AddCheckpoint(new Vector3(0f, 0f, 134f), 0f);
            kit.Hint(new Vector3(0f, 1.5f, 136f), new Vector3(14f, 4f, 4f), "Over the Wall",
                "White updrafts lift Wind Form. Float up, then glide over the wicker wall.", HintTheme.Wind);
            // A tall wicker wall right across the basket. Only Wind gets over it.
            var wicker = PatchworkAssets.Wicker(new Color(0.78f, 0.58f, 0.36f));
            kit.Block(new Vector3(0f, 4.5f, WallZ), new Vector3(40f, 9f, 2f), LevelKit.Palette.Walnut, "TallWickerWall", wicker);
            kit.Block(new Vector3(0f, 9.15f, WallZ), new Vector3(40.4f, 0.3f, 2.4f), LevelKit.Palette.Rose, "TallWallRim", PatchworkAssets.Yarn(new Color(0.85f, 0.5f, 0.55f)));
            kit.Updraft(new Vector3(0f, 0f, 140f), 13f);
            kit.AddCameraZone(new Vector3(0f, 6f, 141f), new Vector3(14f, 16f, 10f), 2f, 4f);
            kit.Thread(new Vector3(0f, 5f, 140f), LevelKit.Palette.Cream);
            kit.Thread(new Vector3(0f, 9f, 140f), LevelKit.Palette.Cream);
            kit.Thread(new Vector3(0f, 11.5f, 143f), LevelKit.Palette.Gold);
            kit.Flower(new Vector3(-5f, 0f, 133f), LevelKit.Palette.Rose);
            kit.Flower(new Vector3(5f, 0f, 137f), LevelKit.Palette.Sky);
            kit.Thimble(new Vector3(-5f, 0f, 139.5f), 1.3f);
        }

        // ---------------------------------------------------------------- 8. The Basket Rim (finale, 144 .. 204)
        static void Section8_BasketRim(LevelKit kit)
        {
            // Behind the wall: the last checkpoint on the ground.
            kit.Quilt(new Vector3(0f, -0.5f, PlatformD + 7f), new Vector3(14f, 1f, 14f), Rim, 3.5f, "RimApproach", true);
            kit.AddCheckpoint(new Vector3(0f, 0f, 146.5f), 0f);
            kit.Hint(new Vector3(0f, 1.5f, 148.5f), new Vector3(14f, 4f, 4f), "The Basket Rim",
                "Use everything! Ride the fountain as Water, switch to Wind and glide to the thimble, then swing to the rim as Yarn.");

            // Step 1 (Water): fountain up to the rim ledge, 10 m up.
            kit.Fountain(new Vector3(0f, 0f, 152f), 12f);
            kit.Block(new Vector3(0f, 9.5f, RimLedgeStart + 4f), new Vector3(6f, 1f, 8f), LevelKit.Palette.Butter, "RimLedge");
            var wood = PatchworkAssets.Wood(new Color(0.55f, 0.42f, 0.32f));
            kit.Block(new Vector3(-2.2f, 4.5f, RimLedgeStart + 4.5f), new Vector3(0.8f, 9f, 0.8f), LevelKit.Palette.Walnut, "RimLedgePost", wood);
            kit.Block(new Vector3(2.2f, 4.5f, RimLedgeStart + 4.5f), new Vector3(0.8f, 9f, 0.8f), LevelKit.Palette.Walnut, "RimLedgePost", wood);
            kit.Thread(new Vector3(0f, 4f, 152f), LevelKit.Palette.Sky);
            kit.Thread(new Vector3(0f, 8f, 152f), LevelKit.Palette.Sky);
            kit.Thread(new Vector3(0f, 11f, 157f), LevelKit.Palette.Gold);
            kit.AddCameraZone(new Vector3(0f, 8f, 166f), new Vector3(16f, 20f, 26f), 3f, 8f);

            // Step 2 (Wind): glide down from the ledge (top 10) to a giant thimble (top 6), 8 m away.
            kit.Quilt(new Vector3(0f, 5.5f, ThimbleZ), new Vector3(ThimbleSize, 1f, ThimbleSize), Rim, 3f, "ThimbleTop");
            var metal = PatchworkAssets.Plain(new Color(0.8f, 0.8f, 0.86f), 0.85f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -2f, ThimbleZ), new Vector3(ThimbleSize * 0.95f, 7.5f, ThimbleSize * 0.95f), metal);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 4.8f, ThimbleZ), new Vector3(ThimbleSize * 1.05f, 0.25f, ThimbleSize * 1.05f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f));
            for (int i = 0; i < 4; i++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 3.5f - i * 1.4f, ThimbleZ), new Vector3(ThimbleSize * 0.97f, 0.05f, ThimbleSize * 0.97f), PatchworkAssets.Plain(new Color(0.65f, 0.65f, 0.72f), 0.6f));
            kit.AddCheckpoint(new Vector3(-1.5f, 6f, ThimbleZ - 1f), 0f);
            kit.ThreadLine(new Vector3(0f, 10.5f, 163.5f), new Vector3(0f, 7.5f, 169f), 3);

            // Step 3 (Yarn): swing from the thimble to the rim, about 15 m.
            float anchorZ = (ThimbleZ + ThimbleSize * 0.5f + GoalLedgeStart) * 0.5f;
            kit.Anchor(new Vector3(0f, 14f, anchorZ), 13.5f);
            kit.AddCameraZone(new Vector3(0f, 8f, anchorZ), new Vector3(16f, 24f, 22f), 2.5f, 6f);
            SwingArcThreads(kit, 14f, anchorZ);

            // The rim: the goal, with a celebration set.
            kit.Quilt(new Vector3(0f, 5.5f, GoalLedgeStart + 6.5f), new Vector3(14f, 1f, 13f), Rim, 3.25f, "BasketRim");
            kit.Goal(new Vector3(0f, 6f, 199f));
            kit.Scissors(new Vector3(-4.5f, 6f, 193f), 1.6f, -30f);
            kit.YarnBall(new Vector3(5f, 6f, 194f), 1.2f, new Color(0.95f, 0.55f, 0.6f));
            kit.YarnBall(new Vector3(-5.2f, 6f, 201f), 1f, new Color(0.55f, 0.7f, 0.95f));
            kit.Flower(new Vector3(-2f, 6f, 195.5f), LevelKit.Palette.Rose);
            kit.Flower(new Vector3(2.5f, 6f, 196f), LevelKit.Palette.Butter);
            kit.Flower(new Vector3(5.5f, 6f, 200.5f), LevelKit.Palette.Lavender);
            kit.Bunting(new Vector3(-7f, 11f, 196f), new Vector3(7f, 11f, 196f), LevelKit.Palette.Quilt);
            kit.SewingPin(new Vector3(6.5f, 6f, 202.5f), 4f, LevelKit.Palette.Rose, new Vector3(-12f, 0f, 15f));
            kit.SewingPin(new Vector3(-6.5f, 6f, 202.5f), 4f, LevelKit.Palette.Gold, new Vector3(-12f, 0f, -15f));
        }
    }
}

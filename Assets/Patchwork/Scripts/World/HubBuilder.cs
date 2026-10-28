using UnityEngine;

namespace Patchwork
{
    // The hub - "The Sewing Room". A small, safe quilt with a practice spot for each form and the
    // stitched doorway to Level 1.
    public static class HubBuilder
    {
        public const string SceneName = "Hub";

        public static LevelInfo Build(LevelKit kit)
        {
            var info = kit.Info;
            info.Id = SceneName;
            info.IsHub = true;
            info.Title = "The Sewing Room";
            info.Subtitle = "Practice your forms, then step through the doorway.";
            info.Spawn = new Vector3(0f, 0.1f, -7f);
            info.SpawnYaw = 0f;
            info.KillY = -10f;
            info.Sky = new Color(0.86f, 0.82f, 0.95f);

            kit.Backdrop(-35f);
            // The Sewing Room really is a room: quilted walls, a ceiling with bunting and lamps.
            kit.Enclosure(new Vector3(-34f, -40f, -24f), new Vector3(34f, 26f, 30f),
                new[] { LevelKit.Palette.Walnut, LevelKit.Palette.Rose, LevelKit.Palette.Cream, LevelKit.Palette.Lavender },
                new Color(0.85f, 0.7f, 0.6f));

            // Main floor.
            kit.Quilt(new Vector3(0f, -0.5f, 0f), new Vector3(24f, 1f, 24f), LevelKit.Palette.Quilt, 4f, "HubFloor");
            kit.AddCheckpoint(new Vector3(0f, 0f, -7f), 0f);
            kit.Hint(new Vector3(0f, 1.5f, -7.5f), new Vector3(10f, 4f, 7f), "Welcome to Patchwork",
                "Move, {Jump} to jump, and press {Wheel} to switch between Water, Yarn and Wind. {Satchel} opens the Satchel to swap hats.");

            // Portal to Level 1.
            kit.AddPortal(new Vector3(0f, 0f, 10f), 0f, Level1Builder.SceneName, GameState.IsLevelComplete(Level1Builder.SceneName));
            int threadTotal;
            int bestThreads = GameState.BestThreads(Level1Builder.SceneName, out threadTotal);
            string record = "Golden buttons: " + GameState.GoldenButtonCount() + " / " + Level1Builder.CollectibleIds.Length +
                            (threadTotal > 0 ? ".   Best threads: " + bestThreads + " / " + threadTotal + "." : ".");
            kit.Hint(new Vector3(0f, 1.5f, 7f), new Vector3(6f, 4f, 3f), "Level 1: The Sewing Basket Garden",
                "Walk through the doorway to begin. " + record);

            ButtonBox(kit);

            // Yarn practice: an anchor over a gap to a little island (west).
            kit.Quilt(new Vector3(-21f, -0.5f, 0f), new Vector3(6f, 1f, 6f), LevelKit.Palette.Meadow, 3f, "SwingIsland");
            kit.Anchor(new Vector3(-15f, 6.5f, 0f), 13f);
            kit.AddCameraZone(new Vector3(-16f, 2f, 0f), new Vector3(14f, 20f, 10f), 2f, 5f);
            kit.Hint(new Vector3(-10f, 1.5f, 0f), new Vector3(4f, 4f, 6f), "Practice: Yarn Swing",
                "In Yarn Form, hold {Ability} near the pink button to swing over to the island. Swing back the same way.", HintTheme.Yarn);
            kit.YarnTree(new Vector3(-22f, 0f, 1.5f), 2.6f, LevelKit.Palette.Rose);

            // Water practice: fountain to a shelf (east, front).
            kit.Fountain(new Vector3(8.5f, 0f, 4f), 7f);
            kit.Block(new Vector3(8.5f, 4.5f, 7.6f), new Vector3(3f, 1f, 3f), LevelKit.Palette.Sky, "FountainShelf");
            kit.Hint(new Vector3(8.5f, 1.5f, 1.2f), new Vector3(4f, 3f, 3f), "Practice: Water",
                "Water Form rides blue fountains and slips through blue drain grates.", HintTheme.Water);

            // Wind practice: updraft (east, back).
            kit.Updraft(new Vector3(8.5f, 0f, -5f), 8f);
            kit.Block(new Vector3(8.5f, 5.5f, -9f), new Vector3(3f, 1f, 3f), LevelKit.Palette.Cream, "UpdraftShelf");
            kit.Hint(new Vector3(8.5f, 1.5f, -2.2f), new Vector3(4f, 3f, 3f), "Practice: Wind",
                "Wind Form floats up white updrafts. Hold {Jump} to glide, tap {Ability} for a gust.", HintTheme.Wind);

            // Mini grate nook (west, back): a little box you can only enter as Water.
            kit.Block(new Vector3(-7f, 1.25f, -9.75f), new Vector3(4f, 2.5f, 0.5f), LevelKit.Palette.Peach, "NookFront");
            kit.Block(new Vector3(-9.25f, 1.25f, -8f), new Vector3(0.5f, 2.5f, 4f), LevelKit.Palette.Peach, "NookSide");
            kit.Block(new Vector3(-4.75f, 1.25f, -8f), new Vector3(0.5f, 2.5f, 4f), LevelKit.Palette.Peach, "NookSide");
            kit.Block(new Vector3(-7f, 2.75f, -8f), new Vector3(5f, 0.5f, 4.5f), LevelKit.Palette.Rose, "NookRoof");
            kit.Block(new Vector3(-8.3f, 1.25f, -6.25f), new Vector3(1.4f, 2.5f, 0.5f), LevelKit.Palette.Peach, "NookBack");
            kit.Block(new Vector3(-5.7f, 1.25f, -6.25f), new Vector3(1.4f, 2.5f, 0.5f), LevelKit.Palette.Peach, "NookBack");
            kit.Block(new Vector3(-7f, 1.75f, -6.25f), new Vector3(1.2f, 1.5f, 0.5f), LevelKit.Palette.Peach, "NookBackTop");
            kit.Grate(new Vector3(-7f, 0.5f, -6.25f), new Vector3(1.2f, 1f, 0.2f));
            kit.SewingButton(new Vector3(-7f, 0.05f, -8f), 1.2f, LevelKit.Palette.Gold, Vector3.zero);

            // A pin-cushion to bounce on, with a few threads to grab.
            kit.AddBouncePad(new Vector3(3f, 0f, -2f));
            kit.Thread(new Vector3(3f, 3f, -2f), LevelKit.Palette.Gold);
            kit.ThreadLine(new Vector3(-3f, 1f, 1f), new Vector3(3f, 1f, 1f), 3);
            kit.Thread(new Vector3(-21f, 1f, 0f), LevelKit.Palette.Rose);

            // Decorations.
            kit.Spool(new Vector3(-9f, 0f, 8f), 1.8f, LevelKit.Palette.Rose);
            kit.Spool(new Vector3(-6.5f, 0f, 9.5f), 1.2f, LevelKit.Palette.Sky);
            kit.YarnTree(new Vector3(9.5f, 0f, 10f), 3.4f, LevelKit.Palette.Sage);
            kit.YarnTree(new Vector3(-10f, 0f, 3.5f), 2.8f, LevelKit.Palette.Butter);
            kit.Flower(new Vector3(3f, 0f, -4f), LevelKit.Palette.Rose);
            kit.Flower(new Vector3(-3f, 0f, -2f), LevelKit.Palette.Butter);
            kit.Flower(new Vector3(4f, 0f, 2f), LevelKit.Palette.Lavender);
            kit.SewingButton(new Vector3(3.5f, 0.05f, 6f), 1.5f, LevelKit.Palette.Rose, Vector3.zero);
            kit.Cloud(new Vector3(-14f, 9f, 16f), 1.6f);
            kit.Cloud(new Vector3(15f, 11f, 6f), 1.3f);
            kit.Cloud(new Vector3(4f, 13f, 22f), 1.8f);

            return info;
        }

        // The Button Box: a little stitched room off the Sewing Room. Its door stays sewn shut until
        // all of Level 1's golden buttons are found; inside is the Golden Crown.
        static void ButtonBox(LevelKit kit)
        {
            int found = GameState.GoldenButtonCount();
            int needed = Level1Builder.CollectibleIds.Length;
            bool open = found >= needed;
            var walls = LevelKit.Palette.Lavender;

            kit.Quilt(new Vector3(8f, -0.5f, -17f), new Vector3(8f, 1f, 10f), new[] { LevelKit.Palette.Butter, LevelKit.Palette.Peach, LevelKit.Palette.Cream }, 2.5f, "ButtonBoxFloor");
            kit.Block(new Vector3(4.2f, 2f, -17f), new Vector3(0.4f, 4f, 10f), walls, "ButtonBoxWall");
            kit.Block(new Vector3(11.8f, 2f, -17f), new Vector3(0.4f, 4f, 10f), walls, "ButtonBoxWall");
            kit.Block(new Vector3(8f, 2f, -21.8f), new Vector3(8f, 4f, 0.4f), walls, "ButtonBoxWall");
            kit.Block(new Vector3(8f, 4.2f, -17f), new Vector3(8.4f, 0.4f, 10.4f), LevelKit.Palette.Rose, "ButtonBoxRoof");
            kit.Block(new Vector3(5.4f, 2f, -12.2f), new Vector3(2.8f, 4f, 0.4f), walls, "ButtonBoxFront");
            kit.Block(new Vector3(10.6f, 2f, -12.2f), new Vector3(2.8f, 4f, 0.4f), walls, "ButtonBoxFront");
            kit.Block(new Vector3(8f, 3.5f, -12.2f), new Vector3(2.4f, 1f, 0.4f), walls, "ButtonBoxLintel");

            // Three button sockets over the door, gold for each one found.
            var gold = PatchworkAssets.Glow(LevelKit.Palette.Gold, 0.5f);
            var empty = PatchworkAssets.Felt(new Color(0.35f, 0.3f, 0.4f));
            for (int i = 0; i < needed; i++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(7.2f + i * 0.8f, 3.5f, -12.45f), new Vector3(0.6f, 0.05f, 0.6f), i < found ? gold : empty, new Vector3(90f, 0f, 0f));

            if (!open)
            {
                // Sewn shut: a fabric panel with a big cross-stitch.
                kit.Block(new Vector3(8f, 1.5f, -12.2f), new Vector3(2.4f, 3f, 0.3f), LevelKit.Palette.Rose, "ButtonBoxDoor");
                var thread = PatchworkAssets.Yarn(new Color(0.95f, 0.9f, 0.8f));
                kit.Deco(PrimitiveType.Cube, new Vector3(8f, 1.5f, -12.4f), new Vector3(0.12f, 3.4f, 0.05f), thread, new Vector3(0f, 0f, 35f));
                kit.Deco(PrimitiveType.Cube, new Vector3(8f, 1.5f, -12.4f), new Vector3(0.12f, 3.4f, 0.05f), thread, new Vector3(0f, 0f, -35f));
            }
            kit.Hint(new Vector3(8f, 1.5f, -10.6f), new Vector3(4f, 3f, 2.4f), "The Button Box",
                open ? "Unstitched! All the golden buttons are home. Step inside."
                     : "Stitched shut. It opens when you've found all " + needed + " golden buttons in Level 1. Found so far: " + found + ".");

            // Inside: the Golden Crown on a pedestal.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(8f, 0.4f, -18f), new Vector3(1.4f, 0.4f, 1.4f), PatchworkAssets.Felt(LevelKit.Palette.Cream));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(8f, 0.82f, -18f), new Vector3(1.6f, 0.04f, 1.6f), gold);
            var pedestal = kit.Volume<HatPedestal>(new Vector3(8f, 1.4f, -18f), new Vector3(2f, 2.8f, 2f), "CrownPedestal");
            var vis = new GameObject("Crown").transform;
            vis.SetParent(pedestal.transform, false);
            var crownGold = PatchworkAssets.Glow(new Color(1f, 0.8f, 0.25f), 0.35f);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.9f, 0.16f, 0.9f), crownGold, default(Vector3), vis);
            for (int i = 0; i < 5; i++)
            {
                float a = i * Mathf.PI * 2f / 5f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Sin(a) * 0.4f, 0.25f, Mathf.Cos(a) * 0.4f), new Vector3(0.18f, 0.36f, 0.18f), crownGold, default(Vector3), vis);
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Sin(a) * 0.4f, 0.46f, Mathf.Cos(a) * 0.4f), Vector3.one * 0.12f, crownGold, default(Vector3), vis);
            }
            pedestal.Setup(4, FormVisuals.HatNames[4], vis);
            kit.Lamp(new Vector3(8f, 3.3f, -17f), 4f);
        }
    }
}

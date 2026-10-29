using UnityEngine;

namespace Patchwork
{
    // Every place you can go, with the text the globe, the loading screen and the HUD show for it.
    public class LevelEntry
    {
        public string Scene;
        public string Number;      // "Level 1", or "Home" for the Skiff
        public string Title;
        public string Blurb;
        public Color Tint;         // loading screen and card colour
        public bool Playable;
        public string[] Collectibles = new string[0];
        public string[] Heartstrings = new string[0];
        // Where its pin sits on the Skiff's globe (degrees).
        public float Longitude;
        public float Latitude;
    }

    public static class LevelCatalog
    {
        public static readonly LevelEntry Skiff = new LevelEntry
        {
            Scene = HubBuilder.SceneName,
            Number = "Home",
            Title = "The Cardboard Skiff",
            Blurb = "Your little cardboard ship, sailing over the patchwork world.",
            Tint = new Color(0.78f, 0.6f, 0.42f),
            Playable = true
        };

        // The levels on the globe, in order. The last two are still being made.
        public static readonly LevelEntry[] Levels =
        {
            new LevelEntry
            {
                Scene = Level1Builder.SceneName,
                Number = "Level 1",
                Title = "The Sewing Basket Garden",
                Blurb = "The Fray has tangled the garden's heart. Find Heartstrings in Pincushion Peaks, Dye Falls, Breezy Bluffs and the Spoolwork Ruins, then ride the golden thread up to the Fray's knot.",
                Tint = new Color(0.55f, 0.72f, 0.48f),
                Playable = true,
                Collectibles = Level1Builder.CollectibleIds,
                Heartstrings = Level1Builder.HeartstringIds,
                Longitude = 20f,
                Latitude = 18f
            },
            new LevelEntry
            {
                Scene = "",
                Number = "Level 2",
                Title = "???",
                Blurb = "Still being stitched. Check back soon!",
                Tint = new Color(0.55f, 0.6f, 0.85f),
                Playable = false,
                Longitude = 95f,
                Latitude = -12f
            },
            new LevelEntry
            {
                Scene = "",
                Number = "Level 3",
                Title = "???",
                Blurb = "Still being stitched. Check back soon!",
                Tint = new Color(0.85f, 0.6f, 0.7f),
                Playable = false,
                Longitude = 170f,
                Latitude = 30f
            }
        };

        // Little tips for the loading screen.
        public static readonly string[] Tips =
        {
            "Golden buttons like to hide off the main path. Look up, look down, and listen.",
            "The Fray's knots pop when you land on top of them. Snarlings too!",
            "Homeward hoops at the end of a climb whisk you back to the middle of the level.",
            "You don't need every Heartstring to reach the Fray's knot. But Captain Yarny will be impressed.",
            "Wind Form gets one gust per jump. Save it for the end of a long glide.",
            "Let go of a swing near the top of the arc to fly the furthest.",
            "Water Form can still slip through drains after riding a fountain. Don't switch too soon!",
            "Spam the D-pad faces and see what happens...",
            "Hats stay on in every form. Find them all to show off on the Skiff.",
            "Threads come in rows. Follow them and they'll usually show you the way.",
            "Fell off? Checkpoint pins stitch your progress as you pass them."
        };

        public static LevelEntry ForScene(string scene)
        {
            if (scene == Skiff.Scene) return Skiff;
            foreach (var e in Levels) if (e.Scene == scene) return e;
            return new LevelEntry { Scene = scene, Number = "", Title = scene, Blurb = "", Tint = new Color(0.7f, 0.6f, 0.8f), Playable = true };
        }

        public static int FoundHeartstrings(LevelEntry e)
        {
            int n = 0;
            foreach (var id in e.Heartstrings) if (GameState.IsCollected(id)) n++;
            return n;
        }

        public static int FoundButtons(LevelEntry e)
        {
            int n = 0;
            foreach (var id in e.Collectibles) if (GameState.IsCollected(id)) n++;
            return n;
        }
    }
}

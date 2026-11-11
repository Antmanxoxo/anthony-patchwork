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
            Title = "The Pod",
            Blurb = "Your cosy cardboard pod, floating high above the patchwork world.",
            Tint = new Color(0.78f, 0.6f, 0.42f),
            Playable = true
        };

        // The levels on the globe, in order. The last one is still being made.
        public static readonly LevelEntry[] Levels =
        {
            new LevelEntry
            {
                Scene = Level1Builder.SceneName,
                Number = "Level 1",
                Title = "Toybox Town",
                Blurb = "It's the Mayor's wedding day, but the Fray has spoiled the town band. Find the lute's string on Spindle Hill, valve dew in the Dye Works, and rescue Wisp the piper from the rooftops.",
                Tint = new Color(0.55f, 0.72f, 0.48f),
                Playable = true,
                Collectibles = Level1Builder.CollectibleIds,
                Heartstrings = Level1Builder.HeartstringIds,
                Longitude = 20f,
                Latitude = 18f
            },
            new LevelEntry
            {
                Scene = Level2Builder.SceneName,
                Number = "Level 2",
                Title = "The Oceanic Museum",
                Blurb = "A grand old museum built round an aquarium the size of a sea. Wander the fossil halls, cross the kelp galleries, brave the deep tunnels and the sunken ruins, and ride the lift up to the Observatory.",
                Tint = new Color(0.3f, 0.42f, 0.75f),
                Playable = true,
                Collectibles = Level2Builder.CollectibleIds,
                Heartstrings = Level2Builder.HeartstringIds,
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
            "Heartstrings in Toybox Town are bonus finds. Captain Yarny will be impressed, though.",
            "Yarn Form can swing from anything wrapped in pink yarn: pegs, lanterns, knots.",
            "Chimney smoke and steam vents lift Wind Form just like updrafts do.",
            "Talk to the toys of Toybox Town. They always know where to look.",
            "Wind Form gets one gust per jump. Save it for the end of a long glide.",
            "Let go of a swing near the top of the arc to fly the furthest.",
            "Water Form can still slip through drains after riding a fountain. Don't switch too soon!",
            "Spam the D-pad faces and see what happens...",
            "Hats stay on in every form. Find them all to show off in your Pod.",
            "Threads come in rows. Follow them and they'll usually show you the way.",
            "Fell off? Checkpoint pins stitch your progress as you pass them.",
            "In the Oceanic Museum, stand by a brass plaque and press Ability to read it. Every word is true.",
            "Only Water Form can swim. Everyone else needs something to stand on.",
            "The music in the museum grows as you find Heartstrings. Listen for the flute."
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

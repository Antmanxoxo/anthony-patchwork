using UnityEditor;
using UnityEngine;

namespace Patchwork
{
    // Patchwork > Replay Toybox Town Opening: the opening chapter normally only plays the very
    // first time you visit. Tick this and it plays again the next time Level 1 loads (once).
    // Patchwork > Replay Level 2 Moments / Reset Level 2: the same idea for the museum.
    [InitializeOnLoad]
    public static class PatchworkStoryMenu
    {
        // If scripts change while the game is playing, Unity normally swaps the new code in
        // mid-game, and the whole level (built in code when it loads) is left half-forgotten:
        // a burst of red errors in the console. This sets Unity to wait until you stop playing
        // before it recompiles. (Preferences > General > Script Changes While Playing.)
        const string DuringPlay = "ScriptCompilationDuringPlay";

        static PatchworkStoryMenu()
        {
            if (EditorPrefs.GetInt(DuringPlay, 0) == 0) EditorPrefs.SetInt(DuringPlay, 1);   // 1 = recompile after finished playing
        }

        [MenuItem("Patchwork/Replay Toybox Town Opening (next visit)", false, 30)]
        static void ReplayOpening()
        {
            EditorPrefs.SetBool(ToyboxIntro.ReplayPref, true);
            Debug.Log("Patchwork: the Toybox Town opening will play the next time Level 1 loads.");
        }

        // Everything Level 2 remembers about your visit is saved under these.
        static readonly string[] Level2Keys = { "L2_", Level2Builder.SceneName + "_", "map_cp_" + Level2Builder.SceneName + "_" };

        // The Oceanic Museum's camera moments, the Storyteller's lines, the cards and the map,
        // all as if you'd never been, but keeping the Heartstrings, buttons and marbles found.
        [MenuItem("Patchwork/Replay Level 2 Moments (keep what you found)", false, 31)]
        static void ReplayLevel2Moments()
        {
            if (!StoppedFirst()) return;
            GameState.ForgetLevel(Level2Builder.SceneName, Level2Keys, true);
            Debug.Log("Patchwork: Level 2's cutscenes, cards and map will all play fresh next time (finds kept).");
        }

        // Level 2 exactly like the very first visit: nothing found, nothing seen, not finished.
        [MenuItem("Patchwork/Reset Level 2 (first visit)", false, 32)]
        static void ResetLevel2()
        {
            if (!StoppedFirst()) return;
            if (!EditorUtility.DisplayDialog("Reset Level 2?",
                "Level 2 will be like the first time you went there: every cutscene, card and Storyteller line plays again, the map is blank, and every Heartstring, golden button and marble in the museum is back where it started. Other levels aren't touched.", "Reset Level 2", "Cancel"))
                return;
            GameState.ForgetLevel(Level2Builder.SceneName, Level2Keys, false);
            Debug.Log("Patchwork: Level 2 reset to a first visit.");
        }

        // Which piece the museum plays. Works while playing too (it swaps over straight away).
        const string SkywardMenu = "Patchwork/Level 2 Music/The Skyward Waltz (new)";
        const string CarouselMenu = "Patchwork/Level 2 Music/The Star Carousel (the first waltz)";
        const string TideMenu = "Patchwork/Level 2 Music/The Lantern Tide (the second piece)";

        [MenuItem(SkywardMenu, false, 39)]
        static void UseSkyward()
        {
            MusicPlayer.MuseumSong = 0;
            Debug.Log("Patchwork: the museum will play The Skyward Waltz.");
        }

        [MenuItem(SkywardMenu, true)]
        static bool UseSkywardCheck()
        {
            Menu.SetChecked(SkywardMenu, MusicPlayer.MuseumSong == 0);
            return true;
        }

        [MenuItem(TideMenu, false, 42)]
        static void UseTide()
        {
            MusicPlayer.MuseumSong = 2;
            Debug.Log("Patchwork: the museum will play The Lantern Tide.");
        }

        [MenuItem(TideMenu, true)]
        static bool UseTideCheck()
        {
            Menu.SetChecked(TideMenu, MusicPlayer.MuseumSong == 2);
            return true;
        }

        [MenuItem(CarouselMenu, false, 41)]
        static void UseCarousel()
        {
            MusicPlayer.MuseumSong = 1;
            Debug.Log("Patchwork: the museum will play The Star Carousel.");
        }

        [MenuItem(CarouselMenu, true)]
        static bool UseCarouselCheck()
        {
            Menu.SetChecked(CarouselMenu, MusicPlayer.MuseumSong == 1);
            return true;
        }

        static bool StoppedFirst()
        {
            if (!EditorApplication.isPlaying) return true;
            EditorUtility.DisplayDialog("Stop the game first", "Press Play again to stop the game, then use this menu.", "OK");
            return false;
        }
    }
}

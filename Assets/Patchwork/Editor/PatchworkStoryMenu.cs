using UnityEditor;
using UnityEngine;

namespace Patchwork
{
    // Patchwork > Replay Toybox Town Opening: the opening chapter normally only plays the very
    // first time you visit. Tick this and it plays again the next time Level 1 loads (once).
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
    }
}

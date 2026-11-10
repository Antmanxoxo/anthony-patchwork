using UnityEditor;
using UnityEngine;

namespace Patchwork
{
    // Patchwork > Replay Toybox Town Opening: the opening chapter normally only plays the very
    // first time you visit. Tick this and it plays again the next time Level 1 loads (once).
    public static class PatchworkStoryMenu
    {
        [MenuItem("Patchwork/Replay Toybox Town Opening (next visit)", false, 30)]
        static void ReplayOpening()
        {
            EditorPrefs.SetBool(ToyboxIntro.ReplayPref, true);
            Debug.Log("Patchwork: the Toybox Town opening will play the next time Level 1 loads.");
        }
    }
}

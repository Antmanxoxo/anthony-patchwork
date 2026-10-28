using UnityEngine;

namespace Patchwork
{
    // Play mode starts without reloading scripts (faster), so static values from the last
    // Play press would stick around. This clears them before anything else runs.
    static class StaticReset
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            PatchworkGame.ResetStatics();
            Sfx.ResetStatics();
            Haptics.ResetStatics();
            GameState.ResetStatics();
            LoadingScreen.ResetStatics();
            TitleSequence.ResetStatics();
            LevelSelectMenu.ResetStatics();
            PatchModel.CurrentFace = PatchModel.Face.Happy;
        }
    }
}

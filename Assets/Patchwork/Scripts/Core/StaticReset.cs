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
            Narrator.ResetStatics();
            GlobeDetail.ResetStatics();
            ToyNpc.Focus = null;
            FrayBodies.ResetStatics();
            GreyStain.ResetStatics();
            PatchworkHUD.ResetStatics();
            TitleSequence.ResetStatics();
            LevelSelectMenu.ResetStatics();
            UseSpot.ResetStatics();
            CardboardLift.ResetStatics();
            CinematicShot.ResetStatics();
            CausticLight.ResetStatics();
            LightBeam.ResetStatics();
            PodTheme.ResetStatics();
            PodLaunch.ResetStatics();
            PodDecor.ResetStatics();
            TapeBall.ResetStatics();
            Monsters.ResetStatics();
            Merits.ResetStatics();
            ScriptedHop.ResetStatics();
            Grumblewool.ResetStatics();
            Level1Director.ResetStatics();
            ThreadRace.ResetStatics();
            WindChime.FirstRung = null;
            XyloKey.AnyStruck = null;
            PatchModel.CurrentFace = PatchModel.Face.Happy;
            PatchModel.Riding = false;
            YarnHorse.ResetStatics();
        }
    }
}

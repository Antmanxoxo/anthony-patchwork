using UnityEngine;

namespace Patchwork
{
    // The only object saved in each Patchwork scene. On play it builds the whole level, player,
    // camera and HUD from code, so scenes never get out of sync with the scripts.
    public class LevelBootstrap : MonoBehaviour
    {
        public LevelId Level = LevelId.Hub;

        void Awake()
        {
            var level = Level;
#if UNITY_EDITOR
            // Patchwork > Play Test Gym sets this just before entering Play mode.
            if (UnityEditor.SessionState.GetBool("Patchwork.PlayGym", false))
            {
                UnityEditor.SessionState.SetBool("Patchwork.PlayGym", false);
                level = LevelId.Gym;
            }
#endif
            PatchworkGame.Boot(level);
        }
    }
}

using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// The only object saved in each Patchwork scene. On play it builds the whole level, player,
    /// camera and HUD from code, so scenes never get out of sync with the scripts.
    /// </summary>
    public class LevelBootstrap : MonoBehaviour
    {
        public LevelId Level = LevelId.Hub;

        void Awake()
        {
            PatchworkGame.Boot(Level);
        }
    }
}

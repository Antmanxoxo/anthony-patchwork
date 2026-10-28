using UnityEngine;

namespace Patchwork
{
    // The spot behind the Skiff's helm. Standing here lets you open the globe and pick a level.
    public class HelmStation : PlayerVolume
    {
        public LevelGlobe Globe;
        // Where the "Set course" prompt floats.
        public Vector3 PromptPoint;

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            player.Game.SetHelm(this);
        }

        protected override void OnPlayerExit(PatchworkPlayer player)
        {
            player.Game.ClearHelm(this);
        }
    }
}

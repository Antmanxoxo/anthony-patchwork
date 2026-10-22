using UnityEngine;

namespace Patchwork
{
    /// <summary>A little flag on a pin. Touching it makes it the respawn point.</summary>
    public class Checkpoint : PlayerVolume
    {
        public Vector3 SpawnPoint;
        public float SpawnYaw;
        Renderer flag;
        bool active;

        public void Setup(Renderer flagRenderer)
        {
            flag = flagRenderer;
            SetActiveVisual(false);
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (active) return;
            player.Game.SetCheckpoint(this);
        }

        public void SetActiveVisual(bool on)
        {
            active = on;
            if (flag != null)
                flag.sharedMaterial = on ? PatchworkAssets.Glow(new Color(1f, 0.8f, 0.25f), 0.6f)
                                         : PatchworkAssets.Fabric(new Color(0.7f, 0.7f, 0.75f));
        }
    }
}

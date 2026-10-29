using UnityEngine;

namespace Patchwork
{
    // A little embroidery hoop at the end of a climb. Hop through it to be whisked back to the
    // middle of the level (and it becomes your checkpoint there).
    public class HomeHoop : PlayerVolume
    {
        public Vector3 Target;
        public float TargetYaw;
        Transform ring;

        public void Setup(Transform spinning) { ring = spinning; }

        void Update()
        {
            if (ring != null) ring.localRotation = Quaternion.Euler(0f, Time.time * 90f, 0f);
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            Sfx.Play(SfxId.PortalWarp, 0.6f);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, player.Motor.Center, new Color(0.85f, 0.7f, 1f));
            player.Game.WarpPlayer(Target, TargetYaw);
        }
    }
}

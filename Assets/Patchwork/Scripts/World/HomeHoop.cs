using UnityEngine;

namespace Patchwork
{
    // An ink hole at the end of a climb. Step in (and say yes) to be whisked back to the middle
    // of the level.
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

        // Asks first, so you can't fall through one by accident. Say No and step out; walking
        // back in asks again.
        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            var game = player.Game;
            game.Ask("Dive into the ink hole?", "It'll whisk you back to the town square.", () =>
            {
                Sfx.Play(SfxId.PortalWarp, 0.6f);
                FxBurst.Spawn(FxBurst.Kind.Splash, player.Motor.Center, new Color(0.15f, 0.1f, 0.3f));
                FxBurst.Spawn(FxBurst.Kind.Sparkle, player.Motor.Center, new Color(0.65f, 0.5f, 1f));
                game.WarpPlayer(Target, TargetYaw);
            });
        }
    }
}

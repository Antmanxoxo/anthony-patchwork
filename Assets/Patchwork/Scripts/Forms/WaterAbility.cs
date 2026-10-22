using UnityEngine;

namespace Patchwork
{
    // Water: smaller body that can pass through drain grates.
    // Fountains check for this form themselves. FormController won't let you change back
    // while you're inside a grate or somewhere too small.
    public class WaterAbility : FormAbility
    {
        bool ignoring;

        public override void OnEnter()
        {
            SetGrateCollision(true);
        }

        public override void OnExit()
        {
            SetGrateCollision(false);
        }

        public override void ForceCancel()
        {
            // Re-apply after a teleport: disabling a CharacterController can reset ignore pairs.
            if (ignoring) SetGrateCollision(true);
        }

        public override void PreMove(PlayerInputFrame input, float dt)
        {
            // Just re-apply the ignore every frame - cheap, and it covers teleports
            // and grates made after you transformed.
            SetGrateCollision(true);
        }

        void SetGrateCollision(bool passThrough)
        {
            ignoring = passThrough;
            var controller = Motor.Controller;
            if (controller == null) return;
            for (int i = 0; i < WaterGrate.All.Count; i++)
            {
                var g = WaterGrate.All[i];
                if (g != null && g.Collider != null)
                    Physics.IgnoreCollision(controller, g.Collider, passThrough);
            }
        }
    }
}

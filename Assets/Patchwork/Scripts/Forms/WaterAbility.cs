using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// Water Form: a smaller collision capsule plus permission to pass through WaterGrate colliders.
    /// Fountains (WaterCurrent volumes) check for this form and lift the player.
    /// Leaving the form is refused while inside a grate or a space too small for the other forms
    /// (FormController checks room before switching).
    /// </summary>
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
            // Cheap and robust: keep the ignore pairs applied every frame (covers teleports
            // and grates that were created after the player transformed).
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

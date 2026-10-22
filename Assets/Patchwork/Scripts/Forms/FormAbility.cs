using UnityEngine;

namespace Patchwork
{
    // One frame of gameplay input, already converted to camera-relative world space.
    public struct PlayerInputFrame
    {
        public Vector2 Move;
        public Vector3 MoveWorld;
        public bool JumpPressed;
        public bool JumpHeld;
        public bool AbilityPressed;
        public bool AbilityHeld;

        public static PlayerInputFrame Empty
        {
            get { return new PlayerInputFrame(); }
        }
    }

    // Base class for a form's special behaviour. Movement itself stays in PlayerMotor;
    // abilities only add to it (or, for the tether, briefly take it over).
    public abstract class FormAbility
    {
        protected PatchworkPlayer Player;
        protected PlayerMotor Motor { get { return Player.Motor; } }

        public void Init(PatchworkPlayer player)
        {
            Player = player;
            OnInit();
        }

        protected virtual void OnInit() { }

        // Called when the player becomes this form.
        public virtual void OnEnter() { }

        // Called when the player leaves this form. Must leave no ability active.
        public virtual void OnExit() { }

        // Runs every gameplay frame before the motor moves the player.
        public virtual void PreMove(PlayerInputFrame input, float dt) { }

        // Runs every gameplay frame after the motor has moved the player.
        public virtual void PostMove(float dt) { }

        // Cancel everything immediately (respawn / scene change). No launch, no effects.
        public virtual void ForceCancel() { }

        // While true, transformation requests are deferred until it becomes false.
        public virtual bool BlocksTransform { get { return false; } }

        // Message shown when a transformation is deferred because of this ability.
        public virtual string BlockReason { get { return ""; } }
    }
}

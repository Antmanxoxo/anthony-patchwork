using UnityEngine;

namespace Patchwork
{
    /// <summary>One frame of gameplay input, already converted to camera-relative world space.</summary>
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

    /// <summary>
    /// Base class for a form's special behaviour. Movement itself stays in PlayerMotor;
    /// abilities only add to it (or, for the tether, briefly take it over).
    /// </summary>
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

        /// <summary>Called when the player becomes this form.</summary>
        public virtual void OnEnter() { }

        /// <summary>Called when the player leaves this form. Must leave no ability active.</summary>
        public virtual void OnExit() { }

        /// <summary>Runs every gameplay frame before the motor moves the player.</summary>
        public virtual void PreMove(PlayerInputFrame input, float dt) { }

        /// <summary>Runs every gameplay frame after the motor has moved the player.</summary>
        public virtual void PostMove(float dt) { }

        /// <summary>Cancel everything immediately (respawn / scene change). No launch, no effects.</summary>
        public virtual void ForceCancel() { }

        /// <summary>While true, transformation requests are deferred until it becomes false.</summary>
        public virtual bool BlocksTransform { get { return false; } }

        /// <summary>Message shown when a transformation is deferred because of this ability.</summary>
        public virtual string BlockReason { get { return ""; } }
    }
}

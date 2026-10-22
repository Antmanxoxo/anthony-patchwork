using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// Shared movement for every form: camera-relative running, jumping (with coyote time and
    /// jump buffering), gravity and landing. Forms change behaviour only through
    /// <see cref="Params"/> and the per-frame modifiers below, so there is one controller, not three.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        public const float BaseGravity = 25f;
        const float CoyoteTime = 0.12f;
        const float JumpBufferTime = 0.14f;
        const float GroundStick = -2f;

        public CharacterController Controller { get; private set; }
        public FormDefinition Params;

        public Vector3 Velocity;
        public bool Grounded { get; private set; }
        public bool JustLanded { get; private set; }
        public bool JustJumped { get; private set; }

        /// <summary>When true another system (the yarn tether) moves the controller this frame.</summary>
        public bool ExternalControl;

        // ---- Per-frame modifiers, set by abilities/volumes before Step() and cleared after ----
        /// <summary>Caps downward speed this frame (glide). Negative = unused.</summary>
        [System.NonSerialized] public float FallSpeedCap = -1f;
        /// <summary>If set, vertical velocity is pushed towards this value this frame (currents, updrafts).</summary>
        [System.NonSerialized] public bool HasLift;
        [System.NonSerialized] public float LiftTargetSpeed;
        [System.NonSerialized] public float LiftAcceleration;

        // ---- Timed overrides ----
        float dashTimer;
        Vector3 dashVelocity;
        float coyoteTimer;
        float jumpBufferTimer;
        bool allowJumpCut;
        float airTime;


        /// <summary>The collider Patch is standing on (for moving platforms). Null when airborne.</summary>
        public Collider GroundCollider { get; private set; }
        Collider groundHit;

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.normal.y > 0.6f) groundHit = hit.collider;
        }

        /// <summary>Launches straight up (bounce pads). Keeps horizontal speed.</summary>
        public void Bounce(float upSpeed)
        {
            Velocity = new Vector3(Velocity.x, upSpeed, Velocity.z);
            Grounded = false;
            dashTimer = 0f;
            MarkLaunched();
        }
        public bool IsDashing { get { return dashTimer > 0f; } }

        void Awake()
        {
            Controller = GetComponent<CharacterController>();
        }

        public void ApplyControllerShape(FormDefinition def)
        {
            Controller.height = def.ControllerHeight;
            Controller.radius = def.ControllerRadius;
            Controller.center = new Vector3(0f, def.ControllerHeight * 0.5f, 0f);
        }

        /// <summary>Starts a short horizontal burst (Wind gust).</summary>
        public void StartDash(Vector3 velocity, float duration)
        {
            dashVelocity = velocity;
            dashTimer = duration;
            Velocity = new Vector3(velocity.x, Mathf.Max(Velocity.y, 1.5f), velocity.z);
        }

        /// <summary>Clears every temporary movement state (used on transform, respawn and tether release).</summary>
        public void ResetTransientState()
        {
            dashTimer = 0f;
            FallSpeedCap = -1f;
            HasLift = false;
            jumpBufferTimer = 0f;
        }

        /// <summary>Marks the current upward motion as a launch (tether/gust) so releasing Jump won't cut it short.</summary>
        public void MarkLaunched()
        {
            allowJumpCut = false;
            coyoteTimer = 0f;
        }

        public void Teleport(Vector3 feetPosition, float yaw)
        {
            Controller.enabled = false;
            transform.position = feetPosition;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Controller.enabled = true;
            Velocity = Vector3.zero;
            ResetTransientState();
        }

        /// <param name="moveWorld">Desired move direction on the ground plane (magnitude 0..1).</param>
        public void Step(Vector3 moveWorld, bool jumpPressed, bool jumpHeld, float dt)
        {
            JustLanded = false;
            JustJumped = false;

            if (ExternalControl || Params == null)
            {
                ClearFrameModifiers();
                return;
            }

            // ---------------- Horizontal ----------------
            Vector3 horizontal = new Vector3(Velocity.x, 0f, Velocity.z);
            if (dashTimer > 0f)
            {
                dashTimer -= dt;
                horizontal = new Vector3(dashVelocity.x, 0f, dashVelocity.z);
            }
            else
            {
                Vector3 target = moveWorld * Params.MoveSpeed;
                float accel = Grounded ? Params.GroundAcceleration : Params.AirAcceleration;
                horizontal = Vector3.MoveTowards(horizontal, target, accel * dt);
            }

            // ---------------- Jump ----------------
            if (Grounded) coyoteTimer = CoyoteTime; else coyoteTimer -= dt;
            if (jumpPressed) jumpBufferTimer = JumpBufferTime; else jumpBufferTimer -= dt;

            float gravity = BaseGravity * Params.GravityScale;
            float vy = Velocity.y;

            if (jumpBufferTimer > 0f && coyoteTimer > 0f)
            {
                vy = Mathf.Sqrt(2f * gravity * Params.JumpHeight);
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
                allowJumpCut = true;
                JustJumped = true;
                Grounded = false;
            }

            // ---------------- Gravity ----------------
            bool dashing = dashTimer > 0f;
            if (!Grounded || vy > 0f)
            {
                float g = gravity;
                // Short hop when jump is released early
                if (vy > 0f && !jumpHeld && allowJumpCut && !dashing) { g *= 2.2f; }
                if (dashing) g *= 0.15f;
                vy -= g * dt;
            }

            if (HasLift)
            {
                vy = Mathf.MoveTowards(vy, LiftTargetSpeed, LiftAcceleration * dt);
            }

            float maxFall = Params.MaxFallSpeed;
            if (FallSpeedCap > 0f) maxFall = Mathf.Min(maxFall, FallSpeedCap);
            if (vy < -maxFall) vy = Mathf.MoveTowards(vy, -maxFall, 60f * dt);

            Velocity = new Vector3(horizontal.x, vy, horizontal.z);

            // ---------------- Move ----------------
            bool wasGrounded = Grounded;
            groundHit = null;
            CollisionFlags flags = Controller.Move(Velocity * dt);
            Grounded = Controller.isGrounded;
            GroundCollider = Grounded ? groundHit : null;

            if ((flags & CollisionFlags.Above) != 0 && Velocity.y > 0f)
                Velocity = new Vector3(Velocity.x, 0f, Velocity.z);

            if ((flags & CollisionFlags.Sides) != 0 && dashing)
                dashTimer = 0f;

            if (Grounded)
            {
                if (Velocity.y < 0f) Velocity = new Vector3(Velocity.x, GroundStick, Velocity.z);
                if (!wasGrounded && airTime > 0.15f) JustLanded = true;
                allowJumpCut = false;
                airTime = 0f;
            }
            else
            {
                airTime += dt;
            }

            // ---------------- Facing ----------------
            Vector3 face = new Vector3(Velocity.x, 0f, Velocity.z);
            if (moveWorld.sqrMagnitude > 0.01f) face = moveWorld;
            if (face.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(face.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 720f * dt);
            }

            ClearFrameModifiers();
        }

        /// <summary>Called by the tether while it owns movement, to keep grounded/air state truthful.</summary>
        public void SetAirborneFromExternal(bool grounded)
        {
            Grounded = grounded;
            if (!grounded) airTime += Time.deltaTime; else airTime = 0f;
        }

        void ClearFrameModifiers()
        {
            FallSpeedCap = -1f;
            HasLift = false;
        }

        /// <summary>Centre of the capsule in world space.</summary>
        public Vector3 Center
        {
            get { return transform.position + Vector3.up * (Controller != null ? Controller.height * 0.5f : 0.7f); }
        }
    }
}

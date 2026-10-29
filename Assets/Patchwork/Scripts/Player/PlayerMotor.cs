using UnityEngine;

namespace Patchwork
{
    // Movement shared by every form: running, jumping (coyote time + jump buffer),
    // gravity and landing. Forms only change the numbers in Params and a few per-frame
    // tweaks, so there's one controller instead of three.
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        public const float BaseGravity = 25f;
        const float CoyoteTime = 0.12f;
        const float JumpBufferTime = 0.14f;
        const float GroundStick = -2f;
        // Below this upward/downward speed counts as "the top of the jump" for the hang.
        const float ApexSpeed = 1.5f;

        public CharacterController Controller { get; private set; }
        public FormDefinition Params;

        public Vector3 Velocity;
        public bool Grounded { get; private set; }
        public bool JustLanded { get; private set; }
        public bool JustJumped { get; private set; }

        // When true another system (the yarn tether) moves the controller this frame.
        public bool ExternalControl;

        // ---- Per-frame modifiers, set by abilities/volumes before Step() and cleared after ----
        // Caps downward speed this frame (glide). Negative = unused.
        [System.NonSerialized] public float FallSpeedCap = -1f;
        // If set, vertical velocity is pushed towards this value this frame (currents, updrafts).
        [System.NonSerialized] public bool HasLift;
        [System.NonSerialized] public float LiftTargetSpeed;
        [System.NonSerialized] public float LiftAcceleration;
        // Set each frame by deep water while Water Form is swimming: faster moves, and Jump works
        // from the surface. Both reset every frame.
        [System.NonSerialized] public float SpeedBoost;
        [System.NonSerialized] public bool Swimming;
        [System.NonSerialized] public float SwimSurface;
        // Whether Patch was swimming last frame (Swimming itself resets every frame), for the camera.
        public bool WasSwimming { get; private set; }

        // ---- Timed overrides ----
        float dashTimer;
        Vector3 dashVelocity;
        float coyoteTimer;
        float jumpBufferTimer;
        bool allowJumpCut;
        float airTime;
        bool launched;

        // Extra horizontal speed kept in the air (e.g. jumping off a moving platform). Cleared on landing.
        [System.NonSerialized] public Vector3 Carry;


        // The collider Patch is standing on (for moving platforms). Null when airborne.
        public Collider GroundCollider { get; private set; }
        Collider groundHit;

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.normal.y > 0.6f) groundHit = hit.collider;
        }

        // Launches straight up (bounce pads). Keeps horizontal speed.
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

        // Starts a short horizontal burst (Wind gust).
        public void StartDash(Vector3 velocity, float duration)
        {
            dashVelocity = velocity;
            dashTimer = duration;
            // Only a small fixed lift. Keeping a bigger upward speed (or adding lift to a fall) let
            // each gust re-launch the glide, so Wind could reach about 15-17 m and skip the Yarn gap.
            float lift = Grounded ? 1.5f : Mathf.Clamp(Velocity.y, 0f, 1.5f);
            Velocity = new Vector3(velocity.x, lift, velocity.z);
        }

        // Clears every temporary movement state (used on transform, respawn and tether release).
        public void ResetTransientState()
        {
            dashTimer = 0f;
            FallSpeedCap = -1f;
            HasLift = false;
            jumpBufferTimer = 0f;
        }

        // Marks the current upward motion as a launch (tether/gust) so releasing Jump won't cut it short.
        public void MarkLaunched()
        {
            allowJumpCut = false;
            coyoteTimer = 0f;
            launched = true;
        }

        public void Teleport(Vector3 feetPosition, float yaw)
        {
            Controller.enabled = false;
            transform.position = feetPosition;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Controller.enabled = true;
            Velocity = Vector3.zero;
            Carry = Vector3.zero;
            launched = false;
            ResetTransientState();
        }

        // moveWorld: Desired move direction on the ground plane (magnitude 0..1).
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
                Vector3 target = moveWorld * Params.MoveSpeed * (1f + SpeedBoost) + (Grounded ? Vector3.zero : Carry);
                float accel = Grounded || Swimming ? Params.GroundAcceleration * (1f + SpeedBoost) : Params.AirAcceleration;
                horizontal = Vector3.MoveTowards(horizontal, target, accel * dt);
            }

            // ---------------- Jump ----------------
            if (Grounded || Swimming) coyoteTimer = CoyoteTime; else coyoteTimer -= dt;
            if (jumpPressed) jumpBufferTimer = JumpBufferTime; else jumpBufferTimer -= dt;

            float gravity = BaseGravity * Params.GravityScale;
            float vy = Velocity.y;

            if (jumpBufferTimer > 0f && coyoteTimer > 0f)
            {
                vy = Mathf.Sqrt(2f * gravity * Params.JumpHeight);
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
                allowJumpCut = true;
                launched = false;
                JustJumped = true;
                Grounded = false;
            }

            // ---------------- Gravity ----------------
            float vyStart = vy;
            bool dashing = dashTimer > 0f;
            if (!Grounded || vy > 0f)
            {
                float g = gravity;
                // Short hop when jump is released early
                if (vy > 0f && !jumpHeld && allowJumpCut && !dashing) { g *= 2.2f; }
                else if (!launched && !dashing && !HasLift)
                {
                    // A little hang at the top while Jump is held, then a quicker drop.
                    if (jumpHeld && Mathf.Abs(vy) < ApexSpeed) g *= Params.ApexHangMultiplier;
                    else if (vy < 0f) g *= Params.FallGravityMultiplier;
                }
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
            // Move by the average of this frame's start and end vertical speed. With plain
            // "speed after gravity x time" the jump height changed with the frame rate.
            bool wasGrounded = Grounded;
            groundHit = null;
            Vector3 step = new Vector3(horizontal.x, (vyStart + vy) * 0.5f, horizontal.z) * dt;
            CollisionFlags flags = Controller.Move(step);
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
                launched = false;
                Carry = Vector3.zero;
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

        // Called by the tether while it owns movement, to keep grounded/air state truthful.
        public void SetAirborneFromExternal(bool grounded)
        {
            Grounded = grounded;
            if (!grounded) airTime += Time.deltaTime; else airTime = 0f;
        }

        void ClearFrameModifiers()
        {
            WasSwimming = Swimming;
            FallSpeedCap = -1f;
            HasLift = false;
            SpeedBoost = 0f;
            Swimming = false;
        }

        // Centre of the capsule in world space.
        public Vector3 Center
        {
            get { return transform.position + Vector3.up * (Controller != null ? Controller.height * 0.5f : 0.7f); }
        }
    }
}

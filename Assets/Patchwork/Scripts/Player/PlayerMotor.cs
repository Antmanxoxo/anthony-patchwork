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
        // Sliding down a slope too steep to stand on (see NoClimb).
        public bool Sliding { get; private set; }
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
        // Holding Run (set each frame by the player). Keeps going through a jump, so a running
        // jump goes further.
        [System.NonSerialized] public bool Running;
        // Full sprint: after holding Run on the ground for a moment he kicks up into a faster,
        // showier sprint (each form has its own). Sprint eases 0..1 for the animation;
        // Sprinting is the on/off. The extra speed is only on the ground, so jump distances
        // (tuned to running jumps) stay the same.
        public bool Sprinting { get; private set; }
        public float Sprint { get; private set; }
        public bool JustSprinted { get; private set; }
        public const float SprintBoost = 1.2f;
        const float SprintDelay = 0.45f;
        float sprintHold;

        // Wearing the God Mode Halo (the testing hat): over three times the speed, much higher
        // jumps, jump again in mid-air as often as you like, hold Jump to fly upward, a
        // gentler fall, and any form can swim.
        public static bool GodMode
        {
            get { return GameState.HatIndex == FormVisuals.GodHat && GameState.IsHatUnlocked(FormVisuals.GodHat); }
        }
        const float GodSpeed = 3.2f, GodJump = 1.8f, GodFly = 11f, GodMaxFall = 16f;
        [System.NonSerialized] public bool Swimming;
        [System.NonSerialized] public float SwimSurface;
        // Whether Patch was swimming last frame (Swimming itself resets every frame), for the camera.
        public bool WasSwimming { get; private set; }
        // Diving: Patch is right under the water (Water Form, in water that's deep enough to
        // dive in). Resets every frame like Swimming.
        [System.NonSerialized] public bool Submerged;
        public bool WasSubmerged { get; private set; }
        // Face this way this frame instead of the way he's moving (holding a crate). Zero = unused.
        [System.NonSerialized] public Vector3 FacingOverride;
        // Sideways speed kept through the air after a launch (see-saw), on top of steering.
        // Cleared on landing.
        [System.NonSerialized] public Vector3 AirCarry;

        // ---- Timed overrides ----
        float dashTimer;
        Vector3 dashVelocity;
        float coyoteTimer;
        float jumpBufferTimer;
        bool allowJumpCut;
        float airTime;
        bool launched;



        // The collider Patch is standing on (for moving platforms). Null when airborne.
        public Collider GroundCollider { get; private set; }
        Collider groundHit;

        // The most upward-facing thing touched this move, to tell standing on a slope too steep
        // to climb (a NoClimb surface, like Spindle Hill's sides) from standing on the ground.
        float bestUp;
        Collider bestUpCollider;

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.normal.y > 0.6f) groundHit = hit.collider;
            if (hit.normal.y > bestUp) { bestUp = hit.normal.y; bestUpCollider = hit.collider; }
            // Running into a ball or a toy block knocks it along.
            var rb = hit.rigidbody;
            if (rb == null || rb.isKinematic || hit.moveDirection.y < -0.5f) return;
            var toy = rb.GetComponent<Grabbable>();
            if (toy == null || !toy.KickOnWalk || toy.Held) return;
            Vector3 push = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
            if (push.sqrMagnitude < 0.01f) return;
            push.Normalize();
            Vector3 v = rb.linearVelocity;
            if (toy.Rolls)
            {
                // Rolling it along: a steady push at about running speed, no hop.
                float roll = Mathf.Max(2.2f, new Vector2(Velocity.x, Velocity.z).magnitude * 1.1f);
                if (Vector3.Dot(new Vector3(v.x, 0f, v.z), push) < roll * 0.9f)
                    rb.linearVelocity = new Vector3(push.x * roll, v.y, push.z * roll);
                return;
            }
            float speed = Mathf.Max(2.5f, new Vector2(Velocity.x, Velocity.z).magnitude * 1.3f);
            if (Vector3.Dot(new Vector3(v.x, 0f, v.z), push) < speed * 0.8f)
            {
                rb.linearVelocity = new Vector3(push.x * speed, Mathf.Max(v.y, 1.5f), push.z * speed);
                Sfx.Step(SfxId.Boing, 0.2f);
            }
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
            sprintHold = 0f;
            Sprinting = false;
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
            AirCarry = Vector3.zero;
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
                bool godSpeed = GodMode;
                float run = Running && !Swimming ? Params.RunMultiplier : 1f;
                if (Sprinting && Grounded) run *= SprintBoost;
                Vector3 target = moveWorld * Params.MoveSpeed * run * (1f + SpeedBoost) * (godSpeed ? GodSpeed : 1f) + (Grounded ? Vector3.zero : AirCarry);
                float accel = Grounded || Swimming || godSpeed ? Params.GroundAcceleration * (1f + SpeedBoost) * (godSpeed ? 3f : 1f) * (run > 1f ? 1.15f : 1f) : Params.AirAcceleration * (run > 1f ? 1.3f : 1f);
                horizontal = Vector3.MoveTowards(horizontal, target, accel * dt);
            }

            // ---------------- Jump ----------------
            if (Grounded || Swimming) coyoteTimer = CoyoteTime; else coyoteTimer -= dt;
            if (jumpPressed) jumpBufferTimer = JumpBufferTime; else jumpBufferTimer -= dt;
            bool god = GodMode;
            if (god && jumpPressed) coyoteTimer = CoyoteTime;   // jump again, any time

            float gravity = BaseGravity * Params.GravityScale;
            float vy = Velocity.y;

            if (jumpBufferTimer > 0f && coyoteTimer > 0f)
            {
                vy = Mathf.Sqrt(2f * gravity * Params.JumpHeight * (god ? GodJump : 1f));
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

            // God Mode: keep holding Jump in the air and Patch flies straight up.
            if (god && !Grounded && jumpHeld && airTime > 0.3f && !HasLift)
                vy = Mathf.MoveTowards(vy, GodFly, 45f * dt);

            float maxFall = Params.MaxFallSpeed;
            if (god) maxFall = Mathf.Min(maxFall, GodMaxFall);
            if (FallSpeedCap > 0f) maxFall = Mathf.Min(maxFall, FallSpeedCap);
            if (vy < -maxFall) vy = Mathf.MoveTowards(vy, -maxFall, 60f * dt);

            Velocity = new Vector3(horizontal.x, vy, horizontal.z);

            // ---------------- Move ----------------
            // Move by the average of this frame's start and end vertical speed. With plain
            // "speed after gravity x time" the jump height changed with the frame rate.
            bool wasGrounded = Grounded;
            groundHit = null;
            bestUp = -1f;
            bestUpCollider = null;
            Vector3 step = new Vector3(horizontal.x, (vyStart + vy) * 0.5f, horizontal.z) * dt;
            CollisionFlags flags = Controller.Move(step);
            Grounded = Controller.isGrounded;
            Sliding = false;
            // Too steep to stand on: he slides back down it, and can't jump up it.
            if (Grounded && bestUp < 0.62f && bestUpCollider != null && bestUpCollider.GetComponent<NoClimb>() != null)
            {
                Grounded = false;
                Sliding = true;
                if (Velocity.y < -14f) Velocity = new Vector3(Velocity.x, -14f, Velocity.z);
            }
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
                AirCarry = Vector3.zero;
                airTime = 0f;
            }
            else
            {
                airTime += dt;
            }

            // ---------------- Sprint ----------------
            JustSprinted = false;
            float flatSpeed = new Vector2(Velocity.x, Velocity.z).magnitude;
            bool canSprint = Running && !Swimming && !dashing && moveWorld.sqrMagnitude > 0.5f && flatSpeed > Params.MoveSpeed * 0.9f;
            if (canSprint && Grounded) sprintHold += dt;
            else if (!canSprint) sprintHold = 0f;
            bool was = Sprinting;
            Sprinting = canSprint && sprintHold >= SprintDelay;
            if (Sprinting && !was) JustSprinted = true;
            Sprint = Mathf.MoveTowards(Sprint, Sprinting ? 1f : 0f, dt * (Sprinting ? 3.5f : 5f));

            // ---------------- Facing ----------------
            Vector3 face = new Vector3(Velocity.x, 0f, Velocity.z);
            if (moveWorld.sqrMagnitude > 0.01f) face = moveWorld;
            if (FacingOverride.sqrMagnitude > 0.01f) face = FacingOverride;
            if (face.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(face.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 720f * dt);
            }

            ClearFrameModifiers();
        }

        // For the models in the Store: standing still on the ground (nothing moves them).
        public void HoldForDisplay()
        {
            Grounded = true;
            Velocity = Vector3.zero;
            Sprinting = false;
            Sprint = 0f;
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
            WasSubmerged = Submerged;
            Submerged = false;
            FallSpeedCap = -1f;
            HasLift = false;
            SpeedBoost = 0f;
            Swimming = false;
            FacingOverride = Vector3.zero;
        }

        // Centre of the capsule in world space.
        public Vector3 Center
        {
            get { return transform.position + Vector3.up * (Controller != null ? Controller.height * 0.5f : 0.7f); }
        }
    }
}

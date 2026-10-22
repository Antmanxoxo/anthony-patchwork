using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// Yarn Form: hold Ability near a YarnAnchor to tether and swing; release Ability (or press Jump)
    /// to let go and launch. The swing is a simple constrained pendulum (a rope that can go slack but
    /// never stretch) moved through the CharacterController, so walls and floors still collide.
    /// </summary>
    public class YarnAbility : FormAbility
    {
        const float PumpAcceleration = 9f;
        const float ReelSpeed = 4f;
        const float ReelFraction = 0.8f;
        const float MinRope = 3f;
        const float MaxSwingSpeed = 22f;
        const float ReattachDelay = 0.25f;

        public bool Attached { get; private set; }
        public YarnAnchor Target { get; private set; }

        YarnAnchor attachedAnchor;
        float ropeLength;
        float targetRopeLength;
        float reattachTimer;
        float failToastTimer;
        LineRenderer line;

        public override bool BlocksTransform { get { return Attached; } }
        public override string BlockReason { get { return "You'll transform as soon as you let go of the yarn."; } }

        protected override void OnInit()
        {
            var go = new GameObject("YarnTether");
            go.transform.SetParent(Player.transform, false);
            go.layer = 2;
            line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.widthMultiplier = 0.1f;
            line.numCapVertices = 3;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.sharedMaterial = PatchworkAssets.Glow(FormLibrary.Get(FormType.Yarn).PrimaryColor, 0.4f);
            line.enabled = false;
        }

        public override void OnEnter()
        {
            reattachTimer = 0f;
        }

        public override void OnExit()
        {
            if (Attached) Detach(false, false);
            SetTarget(null);
        }

        public override void ForceCancel()
        {
            if (Attached) Detach(false, false);
            SetTarget(null);
        }

        public override void PreMove(PlayerInputFrame input, float dt)
        {
            reattachTimer -= dt;
            failToastTimer -= dt;

            if (!Attached)
            {
                SetTarget(reattachTimer <= 0f ? FindBestAnchor() : null);

                if (input.AbilityPressed)
                {
                    if (Target != null) Attach(Target);
                    else if (failToastTimer <= 0f)
                    {
                        Sfx.Play(SfxId.TransformFail, 0.25f);
                        Player.Game.ShowToast("No anchor in reach. Look for the glowing pink buttons.");
                        failToastTimer = 1.2f;
                    }
                }
            }

            if (Attached)
            {
                if (attachedAnchor == null) { Detach(false, false); return; }
                if (input.JumpPressed) { Detach(true, true); return; }
                if (!input.AbilityHeld) { Detach(true, false); return; }
                Swing(input, dt);
            }
        }

        void Attach(YarnAnchor anchor)
        {
            attachedAnchor = anchor;
            Attached = true;
            SetTarget(null);
            Motor.ResetTransientState();
            Motor.ExternalControl = true;

            Vector3 c = Motor.Center;
            ropeLength = Vector3.Distance(c, anchor.Position);
            targetRopeLength = Mathf.Max(MinRope, ropeLength * ReelFraction);

            // A small hop so a tether from the ground starts swinging instead of dragging.
            if (Motor.Grounded && Motor.Velocity.y < 2f)
                Motor.Velocity = new Vector3(Motor.Velocity.x, 2f, Motor.Velocity.z);

            line.enabled = true;
            Player.Visuals.SetTether(true, anchor.Position);
            UpdateLine();
            anchor.Pulse();
            Sfx.Play(SfxId.TetherAttach, 0.6f);
            Player.Visuals.PunchScale(new Vector3(0.85f, 1.2f, 0.85f));
        }

        void Detach(bool launch, bool jumpBoost)
        {
            Attached = false;
            Motor.ExternalControl = false;
            line.enabled = false;
            Player.Visuals.SetTether(false, Vector3.zero);
            attachedAnchor = null;
            reattachTimer = ReattachDelay;

            if (launch)
            {
                Vector3 v = Motor.Velocity;
                Vector3 h = new Vector3(v.x, 0f, v.z) * 1.1f;
                float vy = jumpBoost ? Mathf.Max(v.y + 3f, 7f) : v.y + 2f;
                Motor.Velocity = new Vector3(h.x, vy, h.z);
                Motor.MarkLaunched();
                Sfx.Play(SfxId.TetherRelease, 0.5f);
                Player.Visuals.PunchScale(new Vector3(0.85f, 1.2f, 0.85f));
            }
        }

        void Swing(PlayerInputFrame input, float dt)
        {
            var controller = Motor.Controller;
            Vector3 anchorPos = attachedAnchor.Position;
            Vector3 c = Motor.Center;
            Vector3 vel = Motor.Velocity;

            vel += Vector3.down * PlayerMotor.BaseGravity * dt;

            // Pump: push along the swing, perpendicular to the rope.
            Vector3 ropeDir = (c - anchorPos).normalized;
            Vector3 pump = input.MoveWorld - ropeDir * Vector3.Dot(input.MoveWorld, ropeDir);
            vel += pump * PumpAcceleration * dt;
            vel *= 1f - 0.08f * dt;

            ropeLength = Mathf.MoveTowards(ropeLength, targetRopeLength, ReelSpeed * dt);

            Vector3 next = c + vel * dt;
            Vector3 offset = next - anchorPos;
            float d = offset.magnitude;
            if (d > ropeLength && d > 0.001f)
            {
                Vector3 n = offset / d;
                next = anchorPos + n * ropeLength;
                float radial = Vector3.Dot(vel, n);
                if (radial > 0f) vel -= n * radial;
            }
            vel = Vector3.ClampMagnitude(vel, MaxSwingSpeed);

            CollisionFlags flags = controller.Move(next - c);
            if ((flags & CollisionFlags.Sides) != 0) vel *= 0.6f;
            if ((flags & CollisionFlags.Below) != 0 && vel.y < 0f) vel.y = 0f;
            if ((flags & CollisionFlags.Above) != 0 && vel.y > 0f) vel.y = 0f;

            Motor.Velocity = vel;
            Motor.SetAirborneFromExternal(controller.isGrounded);

            Vector3 face = new Vector3(vel.x, 0f, vel.z);
            if (face.sqrMagnitude > 0.5f)
                Player.transform.rotation = Quaternion.RotateTowards(Player.transform.rotation,
                    Quaternion.LookRotation(face.normalized), 540f * dt);

            UpdateLine();
        }

        public override void PostMove(float dt)
        {
            if (Attached) UpdateLine();
        }

        void UpdateLine()
        {
            if (attachedAnchor == null) return;
            line.SetPosition(0, Player.Visuals.HandPosition);
            line.SetPosition(1, attachedAnchor.Position);
        }

        void SetTarget(YarnAnchor anchor)
        {
            if (Target == anchor) return;
            if (Target != null) Target.Highlighted = false;
            Target = anchor;
            if (Target != null) Target.Highlighted = true;
        }

        YarnAnchor FindBestAnchor()
        {
            Vector3 c = Motor.Center;
            Vector3 facing = Player.transform.forward;
            Vector3 camForward = Player.Game.CameraForwardFlat;
            YarnAnchor best = null;
            float bestScore = float.MaxValue;
            int mask = ~(1 << 2);

            for (int i = 0; i < YarnAnchor.All.Count; i++)
            {
                var a = YarnAnchor.All[i];
                if (a == null) continue;
                Vector3 to = a.Position - c;
                float dist = to.magnitude;
                if (dist > a.Range || to.y < 1f) continue;

                Vector3 flat = new Vector3(to.x, 0f, to.z);
                float facingScore = 0f;
                if (flat.sqrMagnitude > 0.01f)
                {
                    Vector3 fn = flat.normalized;
                    facingScore = Mathf.Max(Vector3.Dot(fn, facing), Vector3.Dot(fn, camForward));
                }
                float score = dist - facingScore * 4f;
                if (score >= bestScore) continue;

                // Line of sight (stop just short of the anchor itself).
                Vector3 end = a.Position - to.normalized * 0.8f;
                if (Physics.Linecast(c, end, mask, QueryTriggerInteraction.Ignore)) continue;

                best = a;
                bestScore = score;
            }
            return best;
        }
    }
}

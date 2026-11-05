using UnityEngine;

namespace Patchwork
{
    // Wind Form: hold Jump while falling to glide (capped fall speed), press Ability for one
    // short horizontal gust per jump. It extends jumps; it never flies indefinitely.
    public class WindAbility : FormAbility
    {
        public const float GlideFallSpeed = 2.0f;
        public const float GustSpeed = 14f;
        public const float GustDuration = 0.22f;
        const float GustCooldown = 0.45f;

        public bool GustReady { get; private set; }
        public bool Gliding { get; private set; }

        float cooldown;
        float failToastTimer;

        protected override void OnInit()
        {
            GustReady = true;
        }

        public override void OnEnter()
        {
            GustReady = Motor.Grounded || GustReady;
            Gliding = false;
        }

        public override void OnExit()
        {
            Gliding = false;
            Player.Visuals.SetGliding(false);
        }

        public override void ForceCancel()
        {
            Gliding = false;
            GustReady = true;
            Player.Visuals.SetGliding(false);
        }

        static readonly Collider[] blown = new Collider[24];

        // The gust shoves loose toys (balls, blocks, crates) in front of Wind.
        void BlowToys(Vector3 dir)
        {
            int n = Physics.OverlapSphereNonAlloc(Motor.Center + dir * 2f, 3.2f, blown, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                var rb = blown[i].attachedRigidbody;
                if (rb == null || rb.isKinematic || rb.GetComponent<Grabbable>() == null) continue;
                float push = 14f / Mathf.Sqrt(Mathf.Max(1f, rb.mass));
                rb.AddForce(dir * push + Vector3.up * push * 0.35f, ForceMode.VelocityChange);
            }
        }

        public override void PreMove(PlayerInputFrame input, float dt)
        {
            if (Motor.Grounded) GustReady = true;
            cooldown -= dt;
            failToastTimer -= dt;

            Gliding = !Motor.Grounded && input.JumpHeld && Motor.Velocity.y < 0.5f && !Motor.IsDashing;
            if (Gliding)
            {
                Motor.FallSpeedCap = GlideFallSpeed;
                Sfx.RequestLoop(LoopId.Glide);
            }
            Player.Visuals.SetGliding(Gliding);

            if (input.AbilityPressed)
            {
                if (GustReady && cooldown <= 0f)
                {
                    Vector3 dir = input.MoveWorld.sqrMagnitude > 0.04f ? input.MoveWorld.normalized : Player.transform.forward;
                    dir.y = 0f;
                    dir.Normalize();
                    Motor.StartDash(dir * GustSpeed, GustDuration);
                    Motor.MarkLaunched();
                    if (!Motor.Grounded) GustReady = false;
                    cooldown = GustCooldown;
                    Sfx.Play(SfxId.Gust, 0.6f);
                    Haptics.Pulse(0f, 0.25f, 0.1f);
                    FxBurst.Spawn(FxBurst.Kind.Puff, Motor.Center - dir * 0.5f, new Color(1f, 1f, 1f), -dir);
                    Player.Visuals.PunchScale(new Vector3(1.3f, 0.8f, 1.3f));
                    BlowToys(dir);
                    Monsters.Gust(Player, Motor.Center, dir);
                }
                else if (failToastTimer <= 0f)
                {
                    Sfx.Play(SfxId.TransformFail, 0.3f);
                    if (!GustReady) Player.Game.ShowToast("Gust recharges when you land.");
                    failToastTimer = 0.8f;
                }
            }
        }
    }
}

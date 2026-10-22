using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// Wind Form: hold Jump while falling to glide (capped fall speed), press Ability for one
    /// short horizontal gust per jump. It extends jumps; it never flies indefinitely.
    /// </summary>
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
                    FxBurst.Spawn(FxBurst.Kind.Puff, Motor.Center - dir * 0.5f, new Color(1f, 1f, 1f), -dir);
                    Player.Visuals.PunchScale(new Vector3(1.3f, 0.8f, 1.3f));
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

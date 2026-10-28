using UnityEngine;

namespace Patchwork
{
    // The player root ("Patch"). Holds the motor, form controller and visuals, and runs them
    // in a fixed order each frame so abilities, volumes and movement never fight each other.
    public class PatchworkPlayer : MonoBehaviour
    {
        public PlayerMotor Motor { get; private set; }
        public FormController Forms { get; private set; }
        public FormVisuals Visuals { get; private set; }
        public PatchworkGame Game { get; private set; }

        public static PatchworkPlayer Create(PatchworkGame game, Vector3 feet, float yaw)
        {
            var go = new GameObject("Patch (Player)");
            go.layer = 2; // Ignore Raycast: camera and anchor checks skip the player.
            go.transform.SetPositionAndRotation(feet, Quaternion.Euler(0f, yaw, 0f));

            var cc = go.AddComponent<CharacterController>();
            cc.slopeLimit = 50f;
            cc.stepOffset = 0.3f;
            cc.skinWidth = 0.06f;
            cc.minMoveDistance = 0f;
            cc.height = 1.4f;
            cc.radius = 0.45f;
            cc.center = new Vector3(0f, 0.7f, 0f);

            var player = go.AddComponent<PatchworkPlayer>();
            player.Game = game;
            player.Motor = go.AddComponent<PlayerMotor>();
            player.Visuals = new FormVisuals(go.transform);
            player.Forms = new FormController(player);
            player.Forms.Initialise(GameState.CurrentForm);
            BlobShadow.Create(player);
            return player;
        }

        public void Tick(PlayerInputFrame input, float dt)
        {
            // Ride moving platforms: move with whatever we're standing on.
            Vector3 platformVelocity = Vector3.zero;
            if (Motor.Grounded && !Motor.ExternalControl && Motor.GroundCollider != null)
            {
                var platform = Motor.GroundCollider.GetComponentInParent<MovingPlatform>();
                if (platform != null && platform.Delta.sqrMagnitude > 0f)
                {
                    Motor.Controller.Move(platform.Delta);
                    if (dt > 0f) platformVelocity = platform.Delta / dt;
                }
            }

            Forms.Tick(dt);
            Forms.Ability.PreMove(input, dt);
            PlayerVolume.UpdateAll(this, dt);
            Motor.Step(input.MoveWorld, input.JumpPressed, input.JumpHeld, dt);
            Forms.Ability.PostMove(dt);

            if (Motor.JustJumped)
            {
                // Jumping off a moving platform keeps its speed instead of stopping dead.
                if (platformVelocity.sqrMagnitude > 0f)
                {
                    Motor.Carry = new Vector3(platformVelocity.x, 0f, platformVelocity.z);
                    Motor.Velocity += new Vector3(platformVelocity.x, Mathf.Max(0f, platformVelocity.y), platformVelocity.z);
                }
                Visuals.PunchScale(new Vector3(0.8f, 1.25f, 0.8f));
                Sfx.Play(SfxId.Jump, 0.35f);
                // A little "hup" on about every other jump, so it never gets tiring.
                if (Random.value < 0.45f) Sfx.Voice(SfxId.VoiceHup, Forms.Current, 0.28f);
                Haptics.Pulse(0f, 0.1f, 0.05f);
            }
            if (Motor.JustLanded)
            {
                Visuals.PunchScale(new Vector3(1.25f, 0.75f, 1.25f));
                Sfx.Play(SfxId.Land, 0.4f);
                Haptics.Pulse(0.28f, 0f, 0.08f);
            }

            Footsteps(dt);
            Visuals.Tick(Motor, dt);
        }

        // ----- Footsteps -----
        float stepDistance;
        Collider lastGround;
        SfxId lastGroundStep = SfxId.StepFelt;

        void Footsteps(float dt)
        {
            var form = Forms.Current;
            if (form == FormType.Wind || !Motor.Grounded || Motor.ExternalControl) { stepDistance = 0.3f; return; }
            Vector3 v = Motor.Velocity;
            float speed = new Vector2(v.x, v.z).magnitude;
            if (speed < 0.8f) { stepDistance = 0.3f; return; }
            float stride = form == FormType.Water ? 0.6f : Mathf.Lerp(0.7f, 1.05f, Mathf.Clamp01(speed / 7f));
            stepDistance += speed * dt;
            if (stepDistance < stride) return;
            stepDistance -= stride;
            float vol = Mathf.Lerp(0.12f, 0.22f, Mathf.Clamp01(speed / 7f));
            if (form == FormType.Water) { Sfx.Step(SfxId.StepSquelch, vol * 1.2f); return; }
            Sfx.Step(SurfaceStep(Motor.GroundCollider), vol);
        }

        // Works out the footstep sound from the material of whatever Patch is standing on.
        SfxId SurfaceStep(Collider ground)
        {
            if (ground == null) return SfxId.StepFelt;
            if (ground == lastGround) return lastGroundStep;
            lastGround = ground;
            var r = ground.GetComponent<Renderer>();
            string name = r != null && r.sharedMaterial != null ? r.sharedMaterial.name : "";
            if (name.Contains("wicker")) lastGroundStep = SfxId.StepWicker;
            else if (name.Contains("wood")) lastGroundStep = SfxId.StepWood;
            else if (name.Contains("grass")) lastGroundStep = SfxId.StepGrass;
            else lastGroundStep = SfxId.StepFelt;
            return lastGroundStep;
        }

        // Safe respawn: cancels abilities, restores collision, puts the player on the checkpoint.
        public void Respawn(Vector3 feet, float yaw)
        {
            Forms.CancelAll();
            Motor.Teleport(feet, yaw);
            Forms.Ability.ForceCancel();
            Visuals.PunchScale(new Vector3(0.5f, 1.4f, 0.5f));
        }
    }
}

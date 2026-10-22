using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// The player root ("Patch"). Holds the motor, form controller and visuals, and runs them
    /// in a fixed order each frame so abilities, volumes and movement never fight each other.
    /// </summary>
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
            return player;
        }

        public void Tick(PlayerInputFrame input, float dt)
        {
            Forms.Tick(dt);
            Forms.Ability.PreMove(input, dt);
            PlayerVolume.UpdateAll(this, dt);
            Motor.Step(input.MoveWorld, input.JumpPressed, input.JumpHeld, dt);
            Forms.Ability.PostMove(dt);

            if (Motor.JustJumped)
            {
                Visuals.PunchScale(new Vector3(0.8f, 1.25f, 0.8f));
                Sfx.Play(SfxId.Jump, 0.35f);
            }
            if (Motor.JustLanded)
            {
                Visuals.PunchScale(new Vector3(1.25f, 0.75f, 1.25f));
                Sfx.Play(SfxId.Land, 0.4f);
            }

            Visuals.Tick(Motor, dt);
        }

        /// <summary>Safe respawn: cancels abilities, restores collision, puts the player on the checkpoint.</summary>
        public void Respawn(Vector3 feet, float yaw)
        {
            Forms.CancelAll();
            Motor.Teleport(feet, yaw);
            Forms.Ability.ForceCancel();
            Visuals.PunchScale(new Vector3(0.5f, 1.4f, 0.5f));
        }
    }
}

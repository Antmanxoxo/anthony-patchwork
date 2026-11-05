using UnityEngine;

namespace Patchwork
{
    // A spot you stand on and press Ability to use something (flip the pod's pattern book, and so
    // on). The HUD floats the label over it while you're standing there.
    public class UseSpot : PlayerVolume
    {
        public static UseSpot Active { get; private set; }
        public string Label = "Use";
        public Vector3 PromptPoint;
        public System.Action<PatchworkPlayer> Used;

        public static void ResetStatics() { Active = null; }

        protected override void OnPlayerStay(PatchworkPlayer player, float dt)
        {
            Active = this;
            var game = player.Game;
            if (game != null && game.Mode == UIMode.Gameplay && game.Input.Ability.WasPressedThisFrame() && player.Motor.Grounded && Used != null)
                Used(player);
        }

        protected override void OnPlayerExit(PatchworkPlayer player) { if (Active == this) Active = null; }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (Active == this) Active = null;
        }
    }
}

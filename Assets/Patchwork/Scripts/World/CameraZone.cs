namespace Patchwork
{
    // While Patch is inside this box, the camera backs out and/or tilts down a bit more,
    // e.g. so a whole swing arc fits on screen.
    public class CameraZone : PlayerVolume
    {
        public float ExtraDistance;
        public float ExtraPitch;

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (player.Game.CameraRig != null) player.Game.CameraRig.EnterZone(this);
        }

        protected override void OnPlayerExit(PatchworkPlayer player)
        {
            if (player.Game.CameraRig != null) player.Game.CameraRig.ExitZone(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (PatchworkGame.Instance != null && PatchworkGame.Instance.CameraRig != null)
                PatchworkGame.Instance.CameraRig.ExitZone(this);
        }
    }
}

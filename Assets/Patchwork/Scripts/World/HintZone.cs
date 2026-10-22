namespace Patchwork
{
    /// <summary>Shows a tutorial card at the top of the screen while the player stands inside.</summary>
    public class HintZone : PlayerVolume
    {
        public string Title;
        public string Body;

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            player.Game.ShowHint(this);
        }

        protected override void OnPlayerExit(PatchworkPlayer player)
        {
            player.Game.ClearHint(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (PatchworkGame.Instance != null) PatchworkGame.Instance.ClearHint(this);
        }
    }
}

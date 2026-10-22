namespace Patchwork
{
    /// <summary>Which element a tutorial card belongs to (sets its colours, art and sound).</summary>
    public enum HintTheme
    {
        Neutral,
        Water,
        Yarn,
        Wind
    }

    /// <summary>Shows a themed tutorial card when the player walks in; it slides away after a few seconds.</summary>
    public class HintZone : PlayerVolume
    {
        public string Title;
        public string Body;
        public HintTheme Theme = HintTheme.Neutral;

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

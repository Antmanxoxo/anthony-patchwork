namespace Patchwork
{
    /// <summary>The end of a level.</summary>
    public class GoalZone : PlayerVolume
    {
        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            player.Game.CompleteLevel();
        }
    }
}

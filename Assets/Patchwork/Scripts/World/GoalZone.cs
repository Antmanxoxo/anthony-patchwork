namespace Patchwork
{
    // The end of a level.
    public class GoalZone : PlayerVolume
    {
        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            player.Game.CompleteLevel();
        }
    }
}

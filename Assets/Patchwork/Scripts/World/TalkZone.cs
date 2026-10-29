using UnityEngine;

namespace Patchwork
{
    // The area around an NPC where you can press Ability to talk to them.
    public class TalkZone : PlayerVolume
    {
        public Npc Npc;

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            player.Game.SetTalker(this);
            if (Npc != null) Npc.Greet();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            var game = PatchworkGame.Instance;
            if (game != null) game.ClearTalker(this);
        }

        protected override void OnPlayerExit(PatchworkPlayer player)
        {
            player.Game.ClearTalker(this);
        }
    }
}

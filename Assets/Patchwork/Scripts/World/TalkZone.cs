using UnityEngine;

namespace Patchwork
{
    // The area around an NPC where you can press Ability to talk to them. When two zones
    // overlap, the nearer NPC is the one you talk to.
    public class TalkZone : PlayerVolume
    {
        public Npc Npc;

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (Nearer(player)) player.Game.SetTalker(this);
            if (Npc != null) Npc.Greet();
        }

        protected override void OnPlayerStay(PatchworkPlayer player, float dt)
        {
            if (player.Game.Talker != this && Nearer(player)) player.Game.SetTalker(this);
        }

        bool Nearer(PatchworkPlayer player)
        {
            var current = player.Game.Talker;
            if (current == null || current.Npc == null || current == this) return true;
            Vector3 p = player.transform.position;
            float mine = (Flat(Npc != null ? Npc.transform.position : transform.position) - Flat(p)).sqrMagnitude;
            float theirs = (Flat(current.Npc.transform.position) - Flat(p)).sqrMagnitude;
            return mine + 0.25f < theirs;
        }

        static Vector3 Flat(Vector3 v) { return new Vector3(v.x, 0f, v.z); }

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

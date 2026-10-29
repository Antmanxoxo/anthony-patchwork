using UnityEngine;

namespace Patchwork
{
    // One bubble from a BubbleBlower: land on top and it pops, bouncing you up.
    public class BubbleBounce : PlayerVolume
    {
        public BubbleBlower Owner;
        public float LaunchSpeed = 12f;

        protected override void OnPlayerStay(PatchworkPlayer player, float dt)
        {
            var m = player.Motor;
            if (m.ExternalControl || m.Velocity.y > 1f) return;
            if (m.Center.y < transform.position.y - 0.2f) return;   // only from above
            m.Bounce(LaunchSpeed);
            player.Visuals.PunchScale(new Vector3(1.3f, 0.7f, 1.3f));
            if (Owner != null) Owner.Pop(Owner.IndexOf(transform), true);
        }
    }
}

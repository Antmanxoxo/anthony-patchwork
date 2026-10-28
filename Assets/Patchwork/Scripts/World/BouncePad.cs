using UnityEngine;

namespace Patchwork
{
    // A squishy pin-cushion. Landing on it (in any form) bounces Patch high into the air.
    public class BouncePad : PlayerVolume
    {
        public float LaunchSpeed = 14f;
        Transform cushion;
        Vector3 cushionScale;
        float squash;
        float cooldown;

        public void Setup(Transform cushionVisual)
        {
            cushion = cushionVisual;
            cushionScale = cushion.localScale;
        }

        void Update()
        {
            cooldown -= Time.deltaTime;
            squash = Mathf.MoveTowards(squash, 0f, Time.deltaTime * 3f);
            if (cushion != null)
            {
                float wob = Mathf.Sin(squash * 20f) * squash;
                cushion.localScale = new Vector3(cushionScale.x * (1f + wob * 0.25f), cushionScale.y * (1f - wob * 0.45f), cushionScale.z * (1f + wob * 0.25f));
            }
        }

        protected override void OnPlayerStay(PatchworkPlayer player, float dt)
        {
            if (cooldown > 0f) return;
            var m = player.Motor;
            if (m.ExternalControl || m.Velocity.y > 1f) return;
            m.Bounce(LaunchSpeed);
            cooldown = 0.3f;
            squash = 1f;
            player.Visuals.PunchScale(new Vector3(1.35f, 0.65f, 1.35f));
            Sfx.Play(SfxId.Boing, 0.6f);
            Haptics.Pulse(0.45f, 0.25f, 0.14f);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position, new Color(1f, 0.6f, 0.7f));
        }
    }
}

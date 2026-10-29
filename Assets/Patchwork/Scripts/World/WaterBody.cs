using UnityEngine;

namespace Patchwork
{
    // A body of deep water (canal, stream, pond). Water Form bobs at the surface and swims fast
    // (and can jump out); every other form can't swim, so falling in means a splash and back to
    // the last checkpoint. The volume's box covers the water from the bottom up to the surface.
    public class WaterBody : PlayerVolume
    {
        public float SurfaceY;
        public float SwimBoost = 0.9f;   // +90% speed while swimming
        float splashCooldown, trail;

        protected override void OnPlayerStay(PatchworkPlayer player, float dt)
        {
            var m = player.Motor;
            if (m.ExternalControl) return;
            float feet = player.transform.position.y;
            if (feet > SurfaceY + 0.05f) return;   // above the water (e.g. on a bridge or jumping out)
            splashCooldown -= dt;

            if (player.Forms.Current == FormType.Water)
            {
                // Float with the surface at about the middle of the body; let jumps leave the water.
                float centreTarget = SurfaceY - 0.1f;
                float c = m.Center.y;
                if (m.Velocity.y < 3f)
                {
                    m.HasLift = true;
                    m.LiftTargetSpeed = Mathf.Clamp((centreTarget - c) * 5f, -4f, 5f);
                    m.LiftAcceleration = 40f;
                }
                m.Swimming = true;
                m.SpeedBoost = SwimBoost;
                // A little wake while moving.
                Vector3 flat = new Vector3(m.Velocity.x, 0f, m.Velocity.z);
                trail -= dt;
                if (flat.sqrMagnitude > 4f && trail <= 0f)
                {
                    trail = 0.18f;
                    FxBurst.Spawn(FxBurst.Kind.Splash, new Vector3(m.Center.x, SurfaceY + 0.05f, m.Center.z), new Color(0.75f, 0.9f, 1f));
                }
                return;
            }

            // Anyone else: splash, and back to the last checkpoint.
            if (splashCooldown > 0f || feet > SurfaceY - 0.2f) return;
            splashCooldown = 1f;
            FxBurst.Spawn(FxBurst.Kind.Splash, new Vector3(m.Center.x, SurfaceY + 0.1f, m.Center.z), new Color(0.75f, 0.9f, 1f));
            Sfx.Play(SfxId.Splash, 0.7f);
            var game = player.Game;
            game.Respawn(true);
            game.ShowToast("Splash! Only Water Form can swim. Switch forms on the Form Wheel.");
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (player.Forms.Current == FormType.Water && player.transform.position.y <= SurfaceY + 0.05f)
            {
                Sfx.Play(SfxId.Splash, 0.45f);
                FxBurst.Spawn(FxBurst.Kind.Splash, new Vector3(player.Motor.Center.x, SurfaceY + 0.05f, player.Motor.Center.z), new Color(0.75f, 0.9f, 1f));
            }
        }
    }
}

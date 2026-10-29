using UnityEngine;

namespace Patchwork
{
    // A column of rising ribbons. Lifts Wind Form only.
    public class WindUpdraft : PlayerVolume
    {
        public float LiftSpeed = 7f;
        Transform[] ribbons;
        float height;

        Transform[] puffs;

        public void SetupRibbons(Transform[] r, float h)
        {
            ribbons = r;
            height = h;
        }

        // Smoke or steam puffs that rise and spread out (chimneys and steam vents).
        public void SetupPuffs(Transform[] p)
        {
            puffs = p;
        }

        void Update()
        {
            if (ribbons == null) return;
            float t = Time.time;
            for (int i = 0; i < ribbons.Length; i++)
            {
                float k = Mathf.Repeat(t * 0.6f + i / (float)ribbons.Length, 1f);
                float a = t * 2f + i * 2.1f;
                ribbons[i].localPosition = new Vector3(Mathf.Cos(a) * 0.9f, k * height - height * 0.5f, Mathf.Sin(a) * 0.9f);
                ribbons[i].localRotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 20f);
            }
            if (puffs == null) return;
            for (int i = 0; i < puffs.Length; i++)
            {
                float k = Mathf.Repeat(t * 0.22f + i / (float)puffs.Length, 1f);
                float drift = Mathf.Sin(t * 0.5f + i) * 0.6f * k;
                puffs[i].localPosition = new Vector3(drift, k * height - height * 0.5f + 0.4f, Mathf.Cos(t * 0.4f + i) * 0.4f * k);
                float s = Mathf.Lerp(0.5f, 2.2f, k) * (1f - Mathf.SmoothStep(0.75f, 1f, k));
                puffs[i].localScale = new Vector3(s, s * 0.8f, s);
            }
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (player.Forms.Current != FormType.Wind)
                player.Game.ShowToast("A breezy updraft... Wind Form could ride this.");
            else
                Sfx.Play(SfxId.Gust, 0.4f);
        }

        protected override void OnPlayerStay(PatchworkPlayer player, float dt)
        {
            if (player.Forms.Current != FormType.Wind) return;
            var m = player.Motor;
            m.HasLift = true;
            m.LiftTargetSpeed = LiftSpeed;
            m.LiftAcceleration = 60f;
        }
    }
}

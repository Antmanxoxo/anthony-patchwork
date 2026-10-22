using UnityEngine;

namespace Patchwork
{
    /// <summary>A column of rising ribbons. Lifts Wind Form only.</summary>
    public class WindUpdraft : PlayerVolume
    {
        public float LiftSpeed = 7f;
        Transform[] ribbons;
        float height;

        public void SetupRibbons(Transform[] r, float h)
        {
            ribbons = r;
            height = h;
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

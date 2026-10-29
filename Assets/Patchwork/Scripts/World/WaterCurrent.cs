using UnityEngine;

namespace Patchwork
{
    // A blue fountain. Water Form is carried upward; other forms are told what would work.
    // The visible rings rise continuously so the route reads as "flowing up".
    public class WaterCurrent : PlayerVolume
    {
        public float LiftSpeed = 9f;
        Transform column, jet;
        Transform[] bubbles, crown, drops;
        float height;
        float spread;
        bool splashed;
        float t;

        public void SetupVisuals(Transform columnT, Transform jetT, Transform[] bubbleTs, Transform[] crownTs, Transform[] dropTs, float h, float dropSpread)
        {
            column = columnT;
            jet = jetT;
            bubbles = bubbleTs;
            crown = crownTs;
            drops = dropTs;
            height = h;
            spread = dropSpread;
            t = Random.value * 10f;
        }

        void Update()
        {
            if (column == null) return;
            t += Time.deltaTime;
            float half = height * 0.5f;
            // The column wobbles a little, like water pushing up.
            float wob = 1f + Mathf.Sin(t * 7f) * 0.05f;
            column.localScale = new Vector3(1.24f * wob, half, 1.24f * (2f - wob));
            jet.localScale = new Vector3(0.45f * (2f - wob), half, 0.45f * wob);

            for (int i = 0; i < bubbles.Length; i++)
            {
                float k = Mathf.Repeat(t * 0.55f + i / (float)bubbles.Length, 1f);
                float a = i * 2.4f + t * 1.5f;
                bubbles[i].localPosition = new Vector3(Mathf.Cos(a) * 0.32f, k * height - half, Mathf.Sin(a) * 0.32f);
                bubbles[i].localScale = Vector3.one * Mathf.Lerp(0.12f, 0.34f, k) * (1f - Mathf.SmoothStep(0.85f, 1f, k));
            }
            // A foamy crown that churns at the top.
            for (int i = 0; i < crown.Length; i++)
            {
                float a = i / (float)crown.Length * Mathf.PI * 2f + t * 1.2f;
                float r = 0.55f + Mathf.Sin(t * 5f + i) * 0.12f;
                crown[i].localPosition = new Vector3(Mathf.Cos(a) * r, half + 0.1f + Mathf.Abs(Mathf.Sin(t * 6f + i * 1.7f)) * 0.3f, Mathf.Sin(a) * r);
                crown[i].localScale = Vector3.one * (0.38f + Mathf.Sin(t * 4f + i * 2f) * 0.08f);
            }
            // Droplets thrown out from the top that fall back down around it.
            for (int i = 0; i < drops.Length; i++)
            {
                float k = Mathf.Repeat(t * 0.8f + i / (float)drops.Length, 1f);
                float a = i * 2.39f;
                float fall = Mathf.Min(height, 4f + spread * 2f);
                float y = half + 0.4f + 1.2f * k - (fall + 1.2f) * k * k;
                float r = 0.5f + spread * k;
                drops[i].localPosition = new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r);
                drops[i].localScale = Vector3.one * 0.16f * (1f - k * 0.5f);
            }
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            splashed = false;
            if (player.Forms.Current != FormType.Water)
                player.Game.ShowToast("Only Water Form can ride the fountain.");
        }

        protected override void OnPlayerStay(PatchworkPlayer player, float dt)
        {
            if (player.Forms.Current != FormType.Water) return;
            var m = player.Motor;
            m.HasLift = true;
            m.LiftTargetSpeed = LiftSpeed;
            m.LiftAcceleration = 70f;
            Sfx.RequestLoop(LoopId.Bubbles);
            if (!splashed)
            {
                splashed = true;
                Sfx.Play(SfxId.Splash, 0.6f);
            }
        }
    }
}

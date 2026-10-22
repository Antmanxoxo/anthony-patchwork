using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// A blue fountain. Water Form is carried upward; other forms are told what would work.
    /// The visible rings rise continuously so the route reads as "flowing up".
    /// </summary>
    public class WaterCurrent : PlayerVolume
    {
        public float LiftSpeed = 9f;
        public Transform[] Rings;
        float ringHeight;
        bool splashed;

        public void SetupRings(Transform[] rings, float height)
        {
            Rings = rings;
            ringHeight = height;
        }

        void Update()
        {
            if (Rings == null) return;
            float t = Time.time;
            for (int i = 0; i < Rings.Length; i++)
            {
                float k = Mathf.Repeat(t * 0.45f + i / (float)Rings.Length, 1f);
                Rings[i].localPosition = new Vector3(0f, k * ringHeight - ringHeight * 0.5f, 0f);
                float s = Mathf.Lerp(1f, 0.6f, k);
                Rings[i].localScale = new Vector3(s * 2.2f, 0.04f, s * 2.2f);
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

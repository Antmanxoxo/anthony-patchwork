using UnityEngine;

namespace Patchwork
{
    // The golden thread that shoots up from the Heart Spool once enough Heartstrings are threaded.
    // Lifts every form, like a gentle updraft.
    public class HeartLift : PlayerVolume
    {
        public float LiftSpeed = 9f;
        Transform[] strands;
        float height;

        public void Setup(Transform[] s, float h)
        {
            strands = s;
            height = h;
        }

        void Update()
        {
            if (strands == null) return;
            float t = Time.time;
            for (int i = 0; i < strands.Length; i++)
            {
                float k = Mathf.Repeat(t * 0.5f + i / (float)strands.Length, 1f);
                float a = t * 2.5f + i * 1.7f;
                strands[i].localPosition = new Vector3(Mathf.Cos(a) * 1.2f, k * height - height * 0.5f, Mathf.Sin(a) * 1.2f);
            }
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            Sfx.Play(SfxId.Gust, 0.4f);
        }

        protected override void OnPlayerStay(PatchworkPlayer player, float dt)
        {
            var m = player.Motor;
            m.HasLift = true;
            m.LiftTargetSpeed = LiftSpeed;
            m.LiftAcceleration = 50f;
        }
    }
}

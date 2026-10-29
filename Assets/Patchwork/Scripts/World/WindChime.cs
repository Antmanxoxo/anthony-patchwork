using UnityEngine;

namespace Patchwork
{
    // Wind chimes: brush through them (gliding as Wind is best) and they ring and swing.
    public class WindChime : PlayerVolume
    {
        static readonly float[] Notes = { 0f, 2f, 4f, 7f, 9f, 12f, 14f };   // a pentatonic scale
        Transform[] tubes;
        float swing, noteTimer;
        int step;

        public void Setup(Transform[] tubeVisuals) { tubes = tubeVisuals; }

        protected override void OnPlayerEnter(PatchworkPlayer player) { Ring(); }

        protected override void OnPlayerStay(PatchworkPlayer player, float dt)
        {
            noteTimer -= dt;
            if (noteTimer <= 0f && player.Motor.Velocity.sqrMagnitude > 4f) Ring();
        }

        void Ring()
        {
            noteTimer = 0.18f;
            swing = 1f;
            float semis = Notes[(step + Random.Range(0, 3)) % Notes.Length];
            step = (step + 2) % Notes.Length;
            Sfx.PlayPitched(SfxId.Chime, 0.35f, Mathf.Pow(2f, semis / 12f) * 0.8f);
        }

        void Update()
        {
            swing = Mathf.MoveTowards(swing, 0.08f, Time.deltaTime * 0.5f);
            if (tubes == null) return;
            for (int i = 0; i < tubes.Length; i++)
                if (tubes[i] != null)
                    tubes[i].localRotation = Quaternion.Euler(Mathf.Sin(Time.time * (3f + i * 0.4f) + i) * 20f * swing, 0f, Mathf.Cos(Time.time * (2.6f + i * 0.3f) + i * 2f) * 20f * swing);
        }
    }
}

using UnityEngine;

namespace Patchwork
{
    // A line of giant dominoes. Bump the first one and they topple one after another with a
    // clatter, ending with a ding on the bell at the far end (and a reward). They stand back up
    // when you come back to the level.
    public class DominoRun : PlayerVolume
    {
        Transform[] pivots;
        Transform bell;
        public System.Action Finished;
        bool running, done;
        float t;
        int clacked;
        const float Gap = 0.16f, Fall = 0.32f;

        public void Setup(Transform[] dominoPivots, Transform bellVisual)
        {
            pivots = dominoPivots;
            bell = bellVisual;
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (running || done) return;
            running = true;
            t = 0f;
            clacked = 0;
        }

        void Update()
        {
            if (bell != null && done)
                bell.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 6f) * 18f * Mathf.Exp(-(Time.time - ringTime) * 0.8f));
            if (!running || pivots == null) return;
            t += Time.deltaTime;
            for (int i = 0; i < pivots.Length; i++)
            {
                float k = Mathf.Clamp01((t - i * Gap) / Fall);
                pivots[i].localRotation = Quaternion.Euler(k * k * 74f, 0f, 0f);
                if (k >= 1f && clacked <= i)
                {
                    clacked = i + 1;
                    Sfx.PlayPitched(SfxId.Clack, 0.55f, 0.8f + i * 0.04f);
                }
            }
            if (t > pivots.Length * Gap + Fall + 0.1f)
            {
                running = false;
                done = true;
                ringTime = Time.time;
                Sfx.Play(SfxId.BigBell, 0.7f);
                if (bell != null) FxBurst.Spawn(FxBurst.Kind.Sparkle, bell.position, new Color(1f, 0.85f, 0.5f));
                if (Finished != null) Finished();
            }
        }

        float ringTime;
    }
}

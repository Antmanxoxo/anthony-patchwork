using UnityEngine;

namespace Patchwork
{
    // The embroidery-hoop portal. Rings spin, sparkles spiral inward and the light pulses.
    // Walking in starts a warp: Patch is pulled to the centre, spins and shrinks away, then the next scene loads.
    public class Portal : PlayerVolume
    {
        public string TargetScene;

        Transform visual;
        Transform[] rings;
        float[] speeds;
        Transform[] motes;
        float[] motePhase;
        Light glow;
        float excite;   // 0 normal -> 1 warping
        float time;

        // World position of the vortex centre (where the player is pulled to).
        public Vector3 Center
        {
            get { return visual != null ? visual.position : transform.position + Vector3.up; }
        }

        public void Setup(Transform visualRoot, Transform[] ringTransforms, float[] ringSpeeds, Transform[] moteTransforms, Light light)
        {
            visual = visualRoot;
            rings = ringTransforms;
            speeds = ringSpeeds;
            motes = moteTransforms;
            glow = light;
            motePhase = new float[motes.Length];
            for (int i = 0; i < motes.Length; i++) motePhase[i] = i / (float)motes.Length;
        }

        // Spin everything up for the warp.
        public void Excite()
        {
            excite = 0.01f;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            time += dt;
            if (excite > 0f) excite = Mathf.MoveTowards(excite, 1f, dt * 1.5f);
            float boost = 1f + excite * 5f;

            if (rings != null)
            {
                for (int i = 0; i < rings.Length; i++)
                {
                    rings[i].Rotate(0f, speeds[i] * boost * dt, 0f, Space.Self);
                    float pulse = 1f + Mathf.Sin(time * 2.5f + i) * 0.03f + excite * 0.15f;
                    Vector3 s = rings[i].localScale;
                    float baseSize = Mathf.Abs(s.x) / Mathf.Max(0.0001f, PrevPulse(i));
                    SetPulse(i, pulse);
                    rings[i].localScale = new Vector3(baseSize * pulse, s.y, baseSize * pulse);
                }
            }

            if (motes != null)
            {
                for (int i = 0; i < motes.Length; i++)
                {
                    motePhase[i] = Mathf.Repeat(motePhase[i] + dt * (0.35f + excite * 1.5f), 1f);
                    float k = motePhase[i];
                    float r = Mathf.Lerp(2.4f, 0.1f, k);
                    float a = k * 6f + i * 2.4f;
                    motes[i].localPosition = new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, -0.15f - k * 0.3f);
                    motes[i].localScale = Vector3.one * Mathf.Lerp(0.14f, 0.03f, k);
                }
            }

            if (glow != null) glow.intensity = 1.8f + Mathf.Sin(time * 3f) * 0.4f + excite * 6f;
        }

        // Ring pulse bookkeeping (so pulsing never drifts the base size).
        float[] pulses;
        float PrevPulse(int i)
        {
            if (pulses == null) { pulses = new float[rings.Length]; for (int j = 0; j < pulses.Length; j++) pulses[j] = 1f; }
            return pulses[i];
        }
        void SetPulse(int i, float p) { pulses[i] = p; }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            player.Game.EnterPortal(this);
        }
    }
}

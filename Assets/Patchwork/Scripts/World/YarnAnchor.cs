using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // A big pink felt button hanging from a thread. Yarn Form can tether to it.
    public class YarnAnchor : MonoBehaviour
    {
        public static readonly List<YarnAnchor> All = new List<YarnAnchor>();

        public float Range = 13f;
        public bool Highlighted;

        Renderer[] glowRenderers;
        Material glowMat;
        Transform visual;
        float pulse;
        float time;
        Color baseEmission;

        public Vector3 Position { get { return transform.position; } }

        public void Setup(Transform visualRoot, Renderer[] glowParts, Color color)
        {
            visual = visualRoot;
            glowRenderers = glowParts;
            glowMat = PatchworkAssets.Unique(PatchworkAssets.Glow(color, 0.25f));
            baseEmission = color;
            foreach (var r in glowRenderers) r.sharedMaterial = glowMat;
        }

        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }

        public void Pulse()
        {
            pulse = 1f;
        }

        void Update()
        {
            time += Time.deltaTime;
            pulse = Mathf.MoveTowards(pulse, 0f, Time.deltaTime * 2.5f);

            float glow = Highlighted ? 0.9f + Mathf.Sin(time * 8f) * 0.35f : 0.2f + Mathf.Sin(time * 2f) * 0.05f;
            glow += pulse;
            if (glowMat != null) glowMat.SetColor("_EmissionColor", baseEmission * glow);

            if (visual != null)
            {
                float s = 1f + (Highlighted ? 0.12f + Mathf.Sin(time * 8f) * 0.05f : 0f) + pulse * 0.3f;
                visual.localScale = Vector3.one * s;
                visual.localRotation = Quaternion.Euler(0f, Mathf.Sin(time * 0.8f) * 25f, Mathf.Sin(time * 1.3f) * 6f);
            }
        }
    }
}

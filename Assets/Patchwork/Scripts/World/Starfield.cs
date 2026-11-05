using UnityEngine;

namespace Patchwork
{
    // Tiny stars drifting slowly towards the Pod's window and wrapping round, so it feels like
    // the pod is flying through space. A few twinkle.
    public class Starfield : MonoBehaviour
    {
        const float MinZ = 18f, Depth = 140f;
        Transform[] stars;
        float[] twinkle;
        Vector3[] baseScale;
        public float Speed = 2.5f;

        public void Build(LevelKit kit, int count)
        {
            var rng = new System.Random(42);
            Material[] mats =
            {
                PatchworkAssets.Glow(new Color(1f, 0.97f, 0.9f), 1.6f),
                PatchworkAssets.Glow(new Color(1f, 0.85f, 0.5f), 1.6f),
                PatchworkAssets.Glow(new Color(0.65f, 0.8f, 1f), 1.6f),
                PatchworkAssets.Glow(new Color(1f, 0.65f, 0.8f), 1.4f),
            };
            stars = new Transform[count];
            twinkle = new float[count];
            baseScale = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                float x = (float)(rng.NextDouble() * 2.0 - 1.0) * 80f;
                float y = (float)(rng.NextDouble() * 2.0 - 1.0) * 55f + 6f;
                float z = MinZ + (float)rng.NextDouble() * Depth;
                float s = 0.12f + (float)rng.NextDouble() * 0.35f;
                var t = kit.Deco(PrimitiveType.Sphere, new Vector3(x, y, z), Vector3.one * s, mats[i % mats.Length], default(Vector3), transform);
                t.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                stars[i] = t;
                baseScale[i] = t.localScale;
                twinkle[i] = i % 5 == 0 ? (float)rng.NextDouble() * 10f : -1f;
            }
        }

        void Update()
        {
            if (stars == null) return;
            float dt = Time.deltaTime, time = Time.time;
            for (int i = 0; i < stars.Length; i++)
            {
                var t = stars[i];
                Vector3 p = t.localPosition;
                p.z -= Speed * dt;
                if (p.z < MinZ) p.z += Depth;
                t.localPosition = p;
                // Fade in from far away, and twinkle.
                float k = Mathf.Clamp01((MinZ + Depth - p.z) / 12f) * Mathf.Clamp01((p.z - MinZ) / 4f);
                if (twinkle[i] >= 0f) k *= 0.6f + 0.4f * Mathf.Sin(time * 3f + twinkle[i]);
                t.localScale = baseScale[i] * k;
            }
        }
    }
}

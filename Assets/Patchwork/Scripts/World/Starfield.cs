using UnityEngine;

namespace Patchwork
{
    // Tiny stars drifting slowly towards the Pod's window and wrapping round, so it feels like
    // the pod is flying through space. A few twinkle.
    // Tiny glowing stars all the way round the Pod (above, below, behind, not just out of the
    // window), in a shell well clear of the hull. The whole field turns very slowly, so the Pod
    // feels like it's drifting through space, and some of the stars twinkle.
    public class Starfield : MonoBehaviour
    {
        const float Inner = 28f, Outer = 150f;
        Transform[] stars;
        float[] twinkle;
        Vector3[] baseScale;
        public float Speed = 1.2f;   // degrees per second the field turns

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
                // An even spread over a sphere, at a random depth in the shell.
                float y = (float)(rng.NextDouble() * 2.0 - 1.0);
                float a = (float)(rng.NextDouble() * Mathf.PI * 2.0);
                float r = Mathf.Sqrt(1f - y * y);
                float d = Mathf.Lerp(Inner, Outer, Mathf.Pow((float)rng.NextDouble(), 0.7f));
                Vector3 p = new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r) * d + Vector3.up * 5f;
                float s = (0.12f + (float)rng.NextDouble() * 0.35f) * Mathf.Lerp(0.8f, 1.8f, (d - Inner) / (Outer - Inner));
                var t = kit.Deco(PrimitiveType.Sphere, p, Vector3.one * s, mats[i % mats.Length], default(Vector3), transform);
                t.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                stars[i] = t;
                baseScale[i] = t.localScale;
                twinkle[i] = i % 4 == 0 ? (float)rng.NextDouble() * 10f : -1f;
            }
        }

        void Update()
        {
            if (stars == null) return;
            float time = Time.time;
            transform.rotation = Quaternion.Euler(time * Speed * 0.3f, time * Speed, 0f);
            for (int i = 0; i < stars.Length; i++)
            {
                if (twinkle[i] < 0f) continue;
                stars[i].localScale = baseScale[i] * (0.55f + 0.45f * Mathf.Sin(time * 3f + twinkle[i]));
            }
        }
    }
}

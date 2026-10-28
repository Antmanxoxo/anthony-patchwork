using UnityEngine;

namespace Patchwork
{
    // Lightweight particle bursts built from primitives (no particle-system assets needed).
    // Each kind has its own motion so transformations can read as soaking, steaming, raining, unravelling...
    public class FxBurst : MonoBehaviour
    {
        public enum Kind
        {
            Scraps,   // fabric squares flying out
            Puff,     // soft cloud puffs pushed along a direction
            Drips,    // water dripping down (soaking)
            Steam,    // wisps rising and growing (evaporating)
            Rain,     // raindrops falling (condensing)
            Threads,  // yarn strands spiralling (unravelling / spinning up)
            Splash,   // droplets thrown out in a ring
            Sparkle,  // tiny glowing motes
            Poof      // big soft cloud that hides the body while a form change swaps it
        }

        Transform[] bits;
        Vector3[] vel;
        Vector3[] spin;
        Vector3[] baseScale;
        float gravity;
        float drag;
        float grow;
        float life;
        float age;
        float swirl;

        public static void Spawn(Kind kind, Vector3 position, Color color, Vector3 direction = default(Vector3))
        {
            var go = new GameObject("Fx_" + kind);
            go.transform.position = position;
            go.AddComponent<FxBurst>().Build(kind, color, direction);
        }

        void Build(Kind kind, Color color, Vector3 dir)
        {
            int count;
            PrimitiveType prim = PrimitiveType.Sphere;
            Material matA, matB;
            Vector2 size;

            switch (kind)
            {
                case Kind.Puff:
                    count = 9; life = 0.6f; gravity = -0.6f; drag = 3f; grow = 1.6f; size = new Vector2(0.22f, 0.42f);
                    matA = matB = PatchworkAssets.Felt(color);
                    break;
                case Kind.Drips:
                    count = 14; life = 0.7f; gravity = 11f; drag = 0.5f; grow = 0f; size = new Vector2(0.07f, 0.12f);
                    matA = matB = PatchworkAssets.Water(color);
                    break;
                case Kind.Steam:
                    count = 12; life = 1.0f; gravity = -1.5f; drag = 1.5f; grow = 2.2f; size = new Vector2(0.18f, 0.32f);
                    matA = matB = PatchworkAssets.Felt(color);
                    break;
                case Kind.Rain:
                    count = 20; life = 0.55f; gravity = 9f; drag = 0f; grow = 0f; size = new Vector2(0.03f, 0.04f);
                    prim = PrimitiveType.Capsule;
                    matA = matB = PatchworkAssets.Water(color);
                    break;
                case Kind.Threads:
                    count = 14; life = 0.75f; gravity = -1f; drag = 1f; grow = 0f; size = new Vector2(0.03f, 0.04f);
                    prim = PrimitiveType.Cylinder;
                    matA = matB = PatchworkAssets.Yarn(color);
                    swirl = 9f;
                    break;
                case Kind.Splash:
                    count = 16; life = 0.65f; gravity = 12f; drag = 0.8f; grow = 0f; size = new Vector2(0.08f, 0.15f);
                    matA = matB = PatchworkAssets.Water(color);
                    break;
                case Kind.Sparkle:
                    count = 10; life = 0.9f; gravity = -0.5f; drag = 1f; grow = 0f; size = new Vector2(0.05f, 0.1f);
                    matA = matB = PatchworkAssets.Glow(color, 1.5f);
                    break;
                case Kind.Poof:
                    count = 14; life = 0.5f; gravity = -1.2f; drag = 5f; grow = 0.5f; size = new Vector2(0.38f, 0.62f);
                    matA = PatchworkAssets.Felt(new Color(0.98f, 0.96f, 0.92f));
                    matB = PatchworkAssets.Felt(Color.Lerp(color, Color.white, 0.45f));
                    break;
                default: // Scraps
                    count = 16; life = 0.85f; gravity = 9f; drag = 2.5f; grow = 0f; size = new Vector2(0.08f, 0.2f);
                    prim = PrimitiveType.Cube;
                    matA = PatchworkAssets.Fabric(color);
                    matB = PatchworkAssets.Felt(Color.Lerp(color, Color.white, 0.6f));
                    break;
            }

            bits = new Transform[count];
            vel = new Vector3[count];
            spin = new Vector3[count];
            baseScale = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                float s = Random.Range(size.x, size.y);
                Vector3 local = Vector3.zero;
                Vector3 v;
                Vector3 scale = Vector3.one * s;
                Vector3 euler = Vector3.zero;
                Vector2 disc = Random.insideUnitCircle;

                switch (kind)
                {
                    case Kind.Puff:
                        v = (dir.sqrMagnitude > 0.01f ? dir.normalized * Random.Range(2f, 4f) : Vector3.zero) + Random.insideUnitSphere * 1.5f;
                        break;
                    case Kind.Drips:
                        local = new Vector3(disc.x * 0.45f, 0.6f + Random.value * 0.5f, disc.y * 0.45f);
                        v = Vector3.down * Random.Range(0.2f, 1.2f);
                        scale = new Vector3(s, s * 1.6f, s);
                        break;
                    case Kind.Steam:
                        local = new Vector3(disc.x * 0.5f, Random.value * 0.6f, disc.y * 0.5f);
                        v = Vector3.up * Random.Range(1.2f, 2.6f) + new Vector3(disc.x, 0f, disc.y) * 0.6f;
                        break;
                    case Kind.Rain:
                        local = new Vector3(disc.x * 0.8f, 1.4f + Random.value * 0.6f, disc.y * 0.8f);
                        v = Vector3.down * Random.Range(4f, 6.5f);
                        scale = new Vector3(s, s * 5f, s);
                        break;
                    case Kind.Threads:
                    {
                        float a = Random.value * Mathf.PI * 2f;
                        local = new Vector3(Mathf.Cos(a) * 0.4f, Random.value * 0.8f, Mathf.Sin(a) * 0.4f);
                        v = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * Random.Range(1f, 2f) + Vector3.up * Random.Range(0.5f, 2f);
                        scale = new Vector3(s, Random.Range(0.12f, 0.25f), s);
                        euler = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), 0f);
                        break;
                    }
                    case Kind.Splash:
                    {
                        float a = Random.value * Mathf.PI * 2f;
                        v = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * Random.Range(2f, 4f) + Vector3.up * Random.Range(2f, 4.5f);
                        break;
                    }
                    case Kind.Poof:
                    {
                        Vector3 o = Random.onUnitSphere;
                        o.y *= 0.8f;
                        local = o * Random.Range(0.25f, 0.55f);
                        v = o * Random.Range(1.5f, 3f);
                        break;
                    }
                    case Kind.Sparkle:
                        local = Random.insideUnitSphere * 0.6f;
                        v = Random.insideUnitSphere * 1.2f;
                        break;
                    default:
                    {
                        v = Random.onUnitSphere;
                        v.y = Mathf.Abs(v.y) * 0.8f + 0.2f;
                        v *= Random.Range(3f, 6.5f);
                        scale = new Vector3(s, s, s * 0.3f);
                        euler = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), 0f);
                        break;
                    }
                }

                bits[i] = FormVisuals.Prim(prim, transform, local, scale, i % 2 == 0 ? matA : matB, euler);
                bits[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                vel[i] = v;
                spin[i] = (kind == Kind.Scraps || kind == Kind.Threads) ? Random.insideUnitSphere * 720f : Vector3.zero;
                baseScale[i] = scale;
            }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            age += dt;
            float k = Mathf.Clamp01(age / life);
            float fade = 1f - Mathf.SmoothStep(0.65f, 1f, k);
            float g = 1f + grow * k;
            for (int i = 0; i < bits.Length; i++)
            {
                vel[i] += Vector3.down * gravity * dt;
                vel[i] *= Mathf.Max(0f, 1f - drag * dt);
                Vector3 p = bits[i].localPosition;
                if (swirl > 0f)
                {
                    // Spiral around the vertical axis.
                    Vector3 flat = new Vector3(p.x, 0f, p.z);
                    Vector3 tangent = Vector3.Cross(Vector3.up, flat).normalized;
                    p += tangent * swirl * 0.25f * dt;
                }
                bits[i].localPosition = p + vel[i] * dt;
                if (spin[i] != Vector3.zero) bits[i].Rotate(spin[i] * dt, Space.Self);
                bits[i].localScale = baseScale[i] * g * fade;
            }
            if (age >= life) Destroy(gameObject);
        }
    }
}

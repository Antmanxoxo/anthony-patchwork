using UnityEngine;

namespace Patchwork
{
    /// <summary>A puff of little fabric scraps that fly out and shrink away. Used for transforms, gusts and pickups.</summary>
    public class TransformBurst : MonoBehaviour
    {
        Transform[] bits;
        Vector3[] velocities;
        Vector3[] spins;
        float life;
        float age;

        public static void Spawn(Vector3 position, Color a, Color b, int count, float lifetime)
        {
            var go = new GameObject("Burst");
            go.transform.position = position;
            var burst = go.AddComponent<TransformBurst>();
            burst.Build(a, b, count, lifetime);
        }

        void Build(Color a, Color b, int count, float lifetime)
        {
            life = lifetime;
            bits = new Transform[count];
            velocities = new Vector3[count];
            spins = new Vector3[count];
            var ma = PatchworkAssets.Fabric(a);
            var mb = PatchworkAssets.Felt(b);
            for (int i = 0; i < count; i++)
            {
                float s = Random.Range(0.08f, 0.2f);
                bits[i] = FormVisuals.Prim(PrimitiveType.Cube, transform, Vector3.zero, new Vector3(s, s, s * 0.3f), i % 2 == 0 ? ma : mb,
                    new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), 0f));
                Vector3 dir = Random.onUnitSphere;
                dir.y = Mathf.Abs(dir.y) * 0.8f + 0.2f;
                velocities[i] = dir * Random.Range(3f, 6.5f);
                spins[i] = Random.insideUnitSphere * 720f;
            }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            age += dt;
            float k = Mathf.Clamp01(age / life);
            for (int i = 0; i < bits.Length; i++)
            {
                velocities[i] += Vector3.down * 9f * dt;
                velocities[i] *= 1f - 2.5f * dt;
                bits[i].localPosition += velocities[i] * dt;
                bits[i].Rotate(spins[i] * dt, Space.Self);
                bits[i].localScale *= 1f - 1.8f * dt * k;
            }
            if (age >= life) Destroy(gameObject);
        }
    }
}

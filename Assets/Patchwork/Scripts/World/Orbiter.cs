using UnityEngine;

namespace Patchwork
{
    // Sails something slowly round a point, facing the way it's going, with a gentle bob and
    // lean (the little Skiff circling the pod).
    public class Orbiter : MonoBehaviour
    {
        public Vector3 Center;
        public float Radius = 20f, RadiusZ = 20f, Height, Speed = 6f, Bob = 0.4f, Lean = 6f;
        public float Angle;
        // A little jitter, like an engine rattling (metres).
        public float Shake;

        void Update()
        {
            Angle += Speed * Time.deltaTime;
            float a = Angle * Mathf.Deg2Rad;
            Vector3 p = Center + new Vector3(Mathf.Sin(a) * Radius, Height + Mathf.Sin(Time.time * 0.7f) * Bob, Mathf.Cos(a) * RadiusZ);
            Vector3 next = Center + new Vector3(Mathf.Sin(a + 0.01f) * Radius, Height, Mathf.Cos(a + 0.01f) * RadiusZ);
            Vector3 fwd = next - new Vector3(p.x, Center.y + Height, p.z);
            if (Shake > 0f)
            {
                float t = Time.time * 9f;
                p += new Vector3(Mathf.PerlinNoise(t, 1.3f) - 0.5f, Mathf.PerlinNoise(4.1f, t) - 0.5f, Mathf.PerlinNoise(t, 7.7f) - 0.5f) * Shake * 2f;
            }
            transform.position = p;
            if (fwd.sqrMagnitude > 1e-6f)
                transform.rotation = Quaternion.LookRotation(fwd.normalized, Vector3.up) * Quaternion.Euler(0f, 0f, -Lean * Mathf.Sign(Speed) + Mathf.Sin(Time.time * 0.5f) * 2f);
        }
    }
}

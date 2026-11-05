using UnityEngine;

namespace Patchwork
{
    // Felt petals and dandelion fluff drifting on the breeze around the camera. They live in a
    // box that follows the camera and wrap round its edges, so there are always a few in view
    // without placing thousands across the level. Purely for looks: no colliders, no shadows.
    public class AmbientFluff : MonoBehaviour
    {
        const float Half = 16f, HalfY = 7f;

        struct Bit
        {
            public Transform T;
            public Vector3 Pos, Spin;
            public float Phase, Fall, Sway;
            public bool Fluff;
        }

        Bit[] bits;
        Vector3 wind = new Vector3(0.7f, 0f, 0.35f);
        float minY = -999f;
        bool shown = true;
        // Secret rooms are built far off to the side of the level: no breeze in there.
        public float IndoorsBeyondX = 200f;

        public static AmbientFluff Create(Transform parent, int count, float floorY)
        {
            var go = new GameObject("AmbientFluff");
            go.transform.SetParent(parent, false);
            var f = go.AddComponent<AmbientFluff>();
            f.minY = floorY;
            f.Build(count);
            return f;
        }

        void Build(int count)
        {
            Color[] petal =
            {
                new Color(1f, 0.72f, 0.8f), new Color(1f, 0.86f, 0.55f),
                new Color(0.8f, 0.7f, 1f), new Color(1f, 0.95f, 0.9f),
            };
            var fluffMat = PatchworkAssets.Glow(new Color(1f, 0.98f, 0.92f), 0.35f);
            bits = new Bit[count];
            Vector3 c = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            for (int i = 0; i < count; i++)
            {
                bool fluff = i % 3 == 0;
                var go = GameObject.CreatePrimitive(fluff ? PrimitiveType.Sphere : PrimitiveType.Cube);
                go.name = fluff ? "Fluff" : "Petal";
                Object.Destroy(go.GetComponent<Collider>());
                go.transform.SetParent(transform, false);
                go.transform.localScale = fluff ? Vector3.one * Random.Range(0.07f, 0.11f)
                    : new Vector3(Random.Range(0.12f, 0.18f), 0.015f, Random.Range(0.08f, 0.12f));
                var r = go.GetComponent<Renderer>();
                r.sharedMaterial = fluff ? fluffMat : PatchworkAssets.Felt(petal[i % petal.Length]);
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
                bits[i] = new Bit
                {
                    T = go.transform,
                    Pos = c + new Vector3(Random.Range(-Half, Half), Random.Range(-HalfY, HalfY), Random.Range(-Half, Half)),
                    Spin = new Vector3(Random.Range(-90f, 90f), Random.Range(-140f, 140f), Random.Range(-90f, 90f)),
                    Phase = Random.value * 10f,
                    Fall = fluff ? Random.Range(-0.05f, 0.12f) : Random.Range(0.25f, 0.45f),
                    Sway = Random.Range(0.3f, 0.7f),
                    Fluff = fluff,
                };
            }
        }

        void Update()
        {
            var cam = Camera.main;
            if (cam == null || bits == null) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return; // frozen in pause and Photo Mode, which looks lovely in photos
            float t = Time.time;
            Vector3 c = cam.transform.position;
            bool want = c.x < IndoorsBeyondX;
            if (want != shown)
            {
                shown = want;
                for (int i = 0; i < bits.Length; i++) bits[i].T.gameObject.SetActive(want);
            }
            if (!shown) return;
            // The breeze slowly swings round.
            Vector3 w = Quaternion.Euler(0f, Mathf.Sin(t * 0.05f) * 40f, 0f) * wind;
            for (int i = 0; i < bits.Length; i++)
            {
                ref Bit b = ref bits[i];
                float s = Mathf.Sin(t * 1.3f + b.Phase);
                Vector3 v = w * (b.Fluff ? 1.3f : 1f) + new Vector3(s * b.Sway, -b.Fall + Mathf.Cos(t * 0.9f + b.Phase) * 0.12f, Mathf.Cos(t * 1.1f + b.Phase) * b.Sway * 0.6f);
                b.Pos += v * dt;
                // Wrap round the box that follows the camera.
                Vector3 d = b.Pos - c;
                if (d.x > Half) b.Pos.x -= Half * 2f; else if (d.x < -Half) b.Pos.x += Half * 2f;
                if (d.z > Half) b.Pos.z -= Half * 2f; else if (d.z < -Half) b.Pos.z += Half * 2f;
                if (d.y > HalfY) b.Pos.y -= HalfY * 2f; else if (d.y < -HalfY) b.Pos.y += HalfY * 2f;
                if (b.Pos.y < minY) b.Pos.y += HalfY * 2f;
                b.T.position = b.Pos;
                b.T.rotation = Quaternion.Euler(b.Spin * (t + b.Phase));
            }
        }
    }
}

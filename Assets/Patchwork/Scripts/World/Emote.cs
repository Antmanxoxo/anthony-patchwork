using UnityEngine;

namespace Patchwork
{
    // A little felt shape that pops up over somebody's head to show how they feel: a heart, a
    // music note, a star or an exclamation mark. It bounces in, bobs for a moment, turns to
    // face the camera, and shrinks away.
    public class Emote : MonoBehaviour
    {
        public enum Kind { Heart, Note, Star, Surprise }

        float age;
        const float Life = 1.5f;
        Vector3 start;

        public static void Show(Vector3 at, Kind kind)
        {
            var go = new GameObject("Emote");
            go.transform.position = at;
            go.AddComponent<Emote>().Build(kind);
        }

        void Build(Kind kind)
        {
            start = transform.position;
            var t = transform;
            switch (kind)
            {
                case Kind.Heart:
                {
                    var m = PatchworkAssets.Felt(new Color(0.96f, 0.4f, 0.52f));
                    Bit(PrimitiveType.Sphere, new Vector3(-0.085f, 0.06f, 0f), new Vector3(0.2f, 0.2f, 0.1f), m, 0f);
                    Bit(PrimitiveType.Sphere, new Vector3(0.085f, 0.06f, 0f), new Vector3(0.2f, 0.2f, 0.1f), m, 0f);
                    Bit(PrimitiveType.Cube, new Vector3(0f, -0.04f, 0f), new Vector3(0.19f, 0.19f, 0.08f), m, 45f);
                    break;
                }
                case Kind.Note:
                {
                    var m = PatchworkAssets.Felt(new Color(0.35f, 0.55f, 0.95f));
                    Bit(PrimitiveType.Sphere, new Vector3(-0.06f, -0.1f, 0f), new Vector3(0.18f, 0.14f, 0.1f), m, 20f);
                    Bit(PrimitiveType.Cube, new Vector3(0.02f, 0.06f, 0f), new Vector3(0.035f, 0.32f, 0.05f), m, 0f);
                    Bit(PrimitiveType.Cube, new Vector3(0.08f, 0.18f, 0f), new Vector3(0.14f, 0.07f, 0.05f), m, -25f);
                    break;
                }
                case Kind.Star:
                {
                    var m = PatchworkAssets.Felt(new Color(1f, 0.82f, 0.3f));
                    Bit(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.18f, 0.18f, 0.09f), m, 0f);
                    for (int i = 0; i < 5; i++)
                    {
                        float a = i * 72f * Mathf.Deg2Rad;
                        Bit(PrimitiveType.Sphere, new Vector3(Mathf.Sin(a), Mathf.Cos(a), 0f) * 0.11f, new Vector3(0.09f, 0.2f, 0.07f), m, -i * 72f);
                    }
                    break;
                }
                default:
                {
                    var m = PatchworkAssets.Felt(new Color(0.98f, 0.55f, 0.25f));
                    Bit(PrimitiveType.Cube, new Vector3(0f, 0.07f, 0f), new Vector3(0.08f, 0.24f, 0.07f), m, 0f);
                    Bit(PrimitiveType.Sphere, new Vector3(0f, -0.13f, 0f), Vector3.one * 0.09f, m, 0f);
                    break;
                }
            }
            t.localScale = Vector3.zero;
        }

        void Bit(PrimitiveType type, Vector3 pos, Vector3 scale, Material m, float roll)
        {
            var b = FormVisuals.Prim(type, transform, pos, scale, m, new Vector3(0f, 0f, roll));
            var r = b.GetComponent<Renderer>();
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        void Update()
        {
            age += Time.deltaTime;
            float k = age / Life;
            if (k >= 1f) { Destroy(gameObject); return; }
            // Bounce in (overshooting a little), hold, shrink away.
            float s = k < 0.18f ? UIKit.EaseOutBack(k / 0.18f) : k > 0.8f ? 1f - (k - 0.8f) / 0.2f : 1f;
            transform.localScale = Vector3.one * Mathf.Max(0f, s) * 1.5f;
            transform.position = start + Vector3.up * (0.35f * Mathf.Clamp01(k * 3f) + Mathf.Sin(age * 7f) * 0.04f);
            var cam = Camera.main;
            if (cam != null) transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position, Vector3.up) * Quaternion.Euler(0f, 0f, Mathf.Sin(age * 9f) * 8f);
        }
    }
}

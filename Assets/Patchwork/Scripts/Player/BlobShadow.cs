using UnityEngine;

namespace Patchwork
{
    // A soft dark disc on the ground straight below Patch, for every form. The sun's shadow lands off
    // to the side (and Water casts none), so this is what tells you where you're going to land.
    // It shrinks and fades the higher Patch is.
    public class BlobShadow : MonoBehaviour
    {
        const float MaxDistance = 30f;
        const float FadeHeight = 10f;
        const float NearAlpha = 0.55f;
        const float FarAlpha = 0.15f;
        const float SizeScale = 2.2f;

        static Texture2D mask;

        PatchworkPlayer player;
        Transform disc;
        Renderer discRenderer;
        MaterialPropertyBlock block;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        public static BlobShadow Create(PatchworkPlayer player)
        {
            var s = player.gameObject.AddComponent<BlobShadow>();
            s.player = player;
            s.Build();
            return s;
        }

        void Build()
        {
            if (mask == null) mask = MakeMask(64);

            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "Patch Shadow";
            go.layer = 2;
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            disc = go.transform;
            discRenderer = go.GetComponent<Renderer>();
            discRenderer.sharedMaterial = PatchworkAssets.SoftShadow(mask);
            discRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            discRenderer.receiveShadows = false;
            block = new MaterialPropertyBlock();
        }

        void LateUpdate()
        {
            if (disc == null || player == null) return;
            var game = player.Game;
            bool hidden = game == null || game.Mode == UIMode.Warping || game.Mode == UIMode.Loading
                          || !player.Motor.Controller.enabled;

            RaycastHit hit;
            Vector3 origin = player.Motor.Center;
            if (hidden || !Physics.Raycast(origin, Vector3.down, out hit, MaxDistance, ~(1 << 2), QueryTriggerInteraction.Ignore))
            {
                discRenderer.enabled = false;
                return;
            }

            float height = Mathf.Max(0f, player.transform.position.y - hit.point.y);
            float k = Mathf.Clamp01(height / FadeHeight);
            float size = player.Motor.Controller.radius * SizeScale * Mathf.Lerp(1f, 0.5f, k);

            disc.position = hit.point + hit.normal * 0.02f;
            disc.rotation = Quaternion.LookRotation(-hit.normal); // a Quad faces -Z, so this points it up
            disc.localScale = new Vector3(size, size, 1f);

            block.SetColor(BaseColorId, new Color(0.12f, 0.08f, 0.14f, Mathf.Lerp(NearAlpha, FarAlpha, k)));
            discRenderer.SetPropertyBlock(block);
            discRenderer.enabled = true;
        }

        void OnDestroy()
        {
            if (disc != null) Destroy(disc.gameObject);
        }

        // White with a soft round alpha falloff.
        static Texture2D MakeMask(int size)
        {
            var t = new Texture2D(size, size, TextureFormat.RGBA32, true);
            t.name = "PW_ShadowMask";
            t.wrapMode = TextureWrapMode.Clamp;
            t.filterMode = FilterMode.Bilinear;
            var px = new Color32[size * size];
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half, dy = (y - half) / half;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = 1f - Mathf.SmoothStep(0.35f, 1f, d);
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(a) * 255f));
                }
            t.SetPixels32(px);
            t.Apply(true, true);
            return t;
        }
    }
}

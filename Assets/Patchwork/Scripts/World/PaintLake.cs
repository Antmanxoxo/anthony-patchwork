using UnityEngine;

namespace Patchwork
{
    // Slowly swirls the marbled paint-and-ink texture on a lake so it looks like it's flowing.
    public class PaintLake : MonoBehaviour
    {
        Material mat;
        Vector2 offset;
        public Vector2 Drift = new Vector2(0.004f, 0.0025f);

        public void Setup(Material material)
        {
            mat = material;
        }

        void Update()
        {
            if (mat == null) return;
            float t = Time.time;
            offset += Drift * Time.deltaTime;
            // A gentle wobble on top of the drift, so the swirls seem to stir.
            Vector2 wobble = new Vector2(Mathf.Sin(t * 0.13f), Mathf.Cos(t * 0.11f)) * 0.01f;
            mat.SetTextureOffset("_BaseMap", offset + wobble);
        }

        void OnDestroy()
        {
            if (mat != null) Destroy(mat);
        }
    }
}

using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// Procedural surface textures with matching normal maps, so materials have real tactile
    /// bumps: knitted stitches, fuzzy felt, woven cloth, wicker, stitched grass and wood grain.
    /// Each pattern is described by a height function; albedo shading and normals come from it.
    /// </summary>
    public static class PatchworkTextures
    {
        public class Pair
        {
            public Texture2D Albedo;
            public Texture2D Normal;
        }

        delegate float HeightFn(float u, float v, int x, int y);

        public static Pair Knit, Felt, Weave, Wicker, Grass, Wood, Wrap;

        static bool built;

        public static void EnsureBuilt()
        {
            if (built) return;
            built = true;
            Knit = Build(128, KnitHeight, 0.62f, 0.38f, 3.5f);
            Felt = Build(64, FeltHeight, 0.84f, 0.16f, 1.6f);
            Weave = Build(128, WeaveHeight, 0.8f, 0.2f, 2.2f);
            Wicker = Build(128, WickerHeight, 0.55f, 0.45f, 4f);
            Grass = Build(128, GrassHeight, 0.72f, 0.28f, 2.5f);
            Wood = Build(64, WoodHeight, 0.78f, 0.22f, 1.5f);
            Wrap = Build(128, WrapHeight, 0.68f, 0.32f, 3f);
        }

        // ------------------------------------------------------------------ Builder

        static Pair Build(int size, HeightFn height, float baseShade, float shadeRange, float strength)
        {
            var h = new float[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    h[y * size + x] = Mathf.Clamp01(height(x / (float)size, y / (float)size, x, y));

            var albedo = new Texture2D(size, size, TextureFormat.RGBA32, true, false);
            var normal = new Texture2D(size, size, TextureFormat.RGBA32, true, true);
            albedo.wrapMode = normal.wrapMode = TextureWrapMode.Repeat;
            albedo.filterMode = normal.filterMode = FilterMode.Trilinear;
            albedo.anisoLevel = normal.anisoLevel = 4;

            var ca = new Color[size * size];
            var cn = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float hv = h[y * size + x];
                    // Ambient-occlusion-ish: low points are darker.
                    float shade = baseShade + shadeRange * hv + (Hash(x * 7, y * 13) - 0.5f) * 0.05f;
                    ca[y * size + x] = new Color(shade, shade, shade, 1f);

                    float hl = h[y * size + (x - 1 + size) % size];
                    float hr = h[y * size + (x + 1) % size];
                    float hd = h[((y - 1 + size) % size) * size + x];
                    float hu = h[((y + 1) % size) * size + x];
                    Vector3 n = new Vector3((hl - hr) * strength, (hd - hu) * strength, 1f).normalized;
                    cn[y * size + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                }
            albedo.SetPixels(ca);
            albedo.Apply(true);
            normal.SetPixels(cn);
            normal.Apply(true);
            return new Pair { Albedo = albedo, Normal = normal };
        }

        static float Hash(int x, int y)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263;
                h = (h ^ (h >> 13)) * 1274126177;
                return ((h ^ (h >> 16)) & 0xFFFF) / 65535f;
            }
        }

        static float ValueNoise(float u, float v, int cells, int seed)
        {
            float fx = u * cells, fy = v * cells;
            int x0 = Mathf.FloorToInt(fx), y0 = Mathf.FloorToInt(fy);
            float tx = fx - x0, ty = fy - y0;
            tx = tx * tx * (3f - 2f * tx);
            ty = ty * ty * (3f - 2f * ty);
            int x1 = (x0 + 1) % cells, y1 = (y0 + 1) % cells;
            x0 %= cells; y0 %= cells;
            float a = Hash(x0 + seed, y0), b = Hash(x1 + seed, y0), c = Hash(x0 + seed, y1), d = Hash(x1 + seed, y1);
            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
        }

        static float SegDist(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 pa = p - a, ba = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * t).magnitude;
        }

        // ------------------------------------------------------------------ Patterns

        /// <summary>Stockinette knitting: rows of little "V" stitches.</summary>
        static float KnitHeight(float u, float v, int x, int y)
        {
            const int cells = 8;
            float cu = u * cells, cv = v * cells;
            Vector2 p = new Vector2(cu - Mathf.Floor(cu), cv - Mathf.Floor(cv));
            float left = SegDist(p, new Vector2(0.08f, 0.95f), new Vector2(0.48f, 0.1f));
            float right = SegDist(p, new Vector2(0.92f, 0.95f), new Vector2(0.52f, 0.1f));
            float d = Mathf.Min(left, right);
            float strand = Mathf.Clamp01(1f - d / 0.24f);
            strand = Mathf.Sqrt(strand);
            // Fibre twist along each leg.
            float twist = 0.85f + 0.15f * Mathf.Sin((p.y * 6f + (left < right ? 0f : 0.5f)) * Mathf.PI * 2f);
            return strand * twist + ValueNoise(u, v, 32, 3) * 0.08f;
        }

        /// <summary>Soft fuzzy felt.</summary>
        static float FeltHeight(float u, float v, int x, int y)
        {
            return ValueNoise(u, v, 8, 1) * 0.5f + ValueNoise(u, v, 16, 2) * 0.3f + Hash(x, y) * 0.2f;
        }

        /// <summary>Plain-weave cloth with a raised running stitch around the patch.</summary>
        static float WeaveHeight(float u, float v, int x, int y)
        {
            int size = 128;
            bool over = ((x / 2) + (y / 2)) % 2 == 0;
            float wx = Mathf.Sin(x * Mathf.PI * 0.5f) * 0.5f + 0.5f;
            float wy = Mathf.Sin(y * Mathf.PI * 0.5f) * 0.5f + 0.5f;
            float h = (over ? wx : wy) * 0.5f + 0.2f;
            int edge = Mathf.Min(Mathf.Min(x, y), Mathf.Min(size - 1 - x, size - 1 - y));
            if (edge < 2) h *= 0.4f;
            int inset = 8;
            bool onH = (y >= inset - 1 && y <= inset) || (y >= size - 1 - inset && y <= size - inset);
            bool onV = (x >= inset - 1 && x <= inset) || (x >= size - 1 - inset && x <= size - inset);
            bool spanX = x >= inset && x <= size - 1 - inset;
            bool spanY = y >= inset && y <= size - 1 - inset;
            if ((onH && spanX && (x % 12) < 7) || (onV && spanY && (y % 12) < 7)) h = 1f;
            return h;
        }

        /// <summary>Basket weave: horizontal reeds passing over and under vertical stakes.</summary>
        static float WickerHeight(float u, float v, int x, int y)
        {
            const int rows = 8, stakes = 4;
            float rv = v * rows;
            int row = Mathf.FloorToInt(rv);
            float across = rv - row;                       // 0..1 across the reed
            float reed = Mathf.Sin(across * Mathf.PI);     // rounded profile
            float su = u * stakes;
            int stake = Mathf.FloorToInt(su);
            float sAcross = su - stake;
            bool reedOnTop = (row + stake) % 2 == 0;
            float stakeProfile = Mathf.Clamp01(1f - Mathf.Abs(sAcross - 0.5f) / 0.12f);
            float reedH = reed * (reedOnTop ? 1f : 0.55f + 0.45f * (1f - stakeProfile));
            float stakeH = stakeProfile * (reedOnTop ? 0.4f : 1f);
            float fibre = ValueNoise(u * 4f, v, 64, 5) * 0.12f;
            return Mathf.Max(reedH, stakeH) * 0.9f + fibre;
        }

        /// <summary>Felt lawn with little stitched grass tufts.</summary>
        static float GrassHeight(float u, float v, int x, int y)
        {
            float h = ValueNoise(u, v, 8, 7) * 0.35f + Hash(x, y) * 0.12f;
            const int cells = 6;
            float cu = u * cells, cv = v * cells;
            int ix = Mathf.FloorToInt(cu), iy = Mathf.FloorToInt(cv);
            Vector2 jitter = new Vector2(Hash(ix, iy * 3) * 0.4f + 0.3f, Hash(ix * 5, iy) * 0.4f + 0.3f);
            Vector2 p = new Vector2(cu - ix, cv - iy);
            // Three little stitched blades fanning out from a point.
            Vector2 root = jitter - new Vector2(0f, 0.2f);
            float d = Mathf.Min(SegDist(p, root, root + new Vector2(-0.12f, 0.3f)),
                      Mathf.Min(SegDist(p, root, root + new Vector2(0f, 0.36f)), SegDist(p, root, root + new Vector2(0.12f, 0.3f))));
            if (d < 0.035f) h = Mathf.Max(h, 1f - d / 0.035f * 0.3f);
            return h;
        }

        static float WoodHeight(float u, float v, int x, int y)
        {
            float grain = Mathf.Sin((u * 18f + ValueNoise(u, v, 4, 9) * 3f) * Mathf.PI * 2f) * 0.5f + 0.5f;
            return grain * 0.6f + ValueNoise(u, v, 16, 4) * 0.4f;
        }

        /// <summary>Strands wound around a ball of yarn.</summary>
        static float WrapHeight(float u, float v, int x, int y)
        {
            float a = Mathf.Sin((v * 22f + Mathf.Sin(u * Mathf.PI * 4f) * 1.6f) * Mathf.PI * 2f) * 0.5f + 0.5f;
            float b = Mathf.Sin(((u + v) * 18f) * Mathf.PI * 2f) * 0.5f + 0.5f;
            return Mathf.Max(a, b * 0.75f) * 0.9f + Hash(x, y) * 0.1f;
        }
    }
}

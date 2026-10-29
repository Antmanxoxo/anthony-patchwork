using UnityEngine;

namespace Patchwork
{
    // Makes the bumpy textures (knit, felt, cloth, wicker, grass, wood, wound yarn).
    // Each one is just a height function - colour shading and the normal map come from it.
    public static class PatchworkTextures
    {
        public class Pair
        {
            public Texture2D Albedo;
            public Texture2D Normal;
        }

        delegate float HeightFn(float u, float v, int x, int y);

        public static Pair Knit, Felt, Weave, Wicker, Grass, Wood, Wrap, Cardboard;
        // Town textures: cobblestones, brick, plaster, fish-scale roof shingles, wooden planks and
        // damask (the swirly Victorian wallpaper / upholstery pattern).
        public static Pair Cobble, Brick, Plaster, Shingle, Planks, Damask;
        // Crafted water: rippling blue satin with stitched white wave lines and sequin sparkles
        // (full colour; tint it white).
        public static Pair CraftWater;
        // Full-colour textures (not tinted by the material colour).
        public static Texture2D PaintMarble;   // swirled paint and ink for the lake under Level 1
        public static Texture2D GlobeMap;      // the quilted world map on the Skiff's globe
        public static Texture2D BalloonQuilt;  // the Skiff's patched balloon

        static bool built;

        public static void EnsureBuilt()
        {
            // Also rebuild if Unity has thrown the textures away (Play pressed again).
            if (built && Wrap != null && Wrap.Albedo != null) return;
            built = true;
            // Deeper relief than before: stronger normals and a bit more shading in the dips.
            // Each surface: height pattern, shading, relief, then the painterly extras:
            // what colour the crevices go, how much the raised edges catch the light, and how
            // much big soft colour variation there is so repeats don't show.
            var none = new Color(1f, 1f, 1f);
            Knit = Build(256, KnitHeight, 0.56f, 0.44f, 5f, new Color(0.82f, 0.8f, 0.86f), 0.5f, 0.08f, 4);
            Felt = Build(256, FeltHeight, 0.8f, 0.2f, 2.4f, none, 0.25f, 0.1f, 3);
            Weave = Build(256, WeaveHeight2, 0.74f, 0.26f, 3.2f, new Color(0.85f, 0.83f, 0.88f), 0.35f, 0.07f, 4);
            Wicker = Build(256, WickerHeight, 0.5f, 0.5f, 5.5f, new Color(0.78f, 0.7f, 0.62f), 0.6f, 0.08f, 3);
            // Grass: blue-green shade in the dips, sunny yellow-green tips, clumpy patches.
            Grass = Build(512, GrassHeight, 0.64f, 0.36f, 3.8f, new Color(0.72f, 0.85f, 0.82f), 0.55f, 0.16f, 2, new Color(1.0f, 1.0f, 0.8f));
            Wood = Build(256, WoodHeight, 0.72f, 0.28f, 2.4f, new Color(0.78f, 0.68f, 0.6f), 0.4f, 0.1f, 3);
            Wrap = Build(256, WrapHeight, 0.6f, 0.4f, 4.2f, new Color(0.84f, 0.82f, 0.88f), 0.45f, 0.06f, 4);
            Cardboard = Build(256, CardboardHeight, 0.78f, 0.22f, 2.4f, new Color(0.85f, 0.78f, 0.7f), 0.3f, 0.12f, 3);
            // Stone and brick: mossy green in the joints and worn, sunlit edges.
            Cobble = Build(256, CobbleHeight, 0.45f, 0.55f, 5f, new Color(0.62f, 0.74f, 0.5f), 0.9f, 0.14f, 3);
            Brick = Build(256, BrickHeight, 0.5f, 0.5f, 4.5f, new Color(0.7f, 0.76f, 0.6f), 0.8f, 0.12f, 3);
            Plaster = Build(256, PlasterHeight, 0.82f, 0.18f, 1.6f, new Color(0.92f, 0.88f, 0.82f), 0.2f, 0.12f, 2);
            Shingle = Build(256, ShingleHeight, 0.5f, 0.5f, 4.5f, new Color(0.7f, 0.72f, 0.66f), 0.8f, 0.12f, 3);
            Planks = Build(256, PlankHeight, 0.6f, 0.4f, 3.5f, new Color(0.62f, 0.5f, 0.42f), 0.6f, 0.12f, 3);
            Damask = Build(256, DamaskHeight, 0.72f, 0.28f, 2.2f, new Color(0.86f, 0.82f, 0.9f), 0.3f, 0.06f, 4);
            CraftWater = BuildCraftWater(256);
            PaintMarble = BuildMarble(256);
            GlobeMap = BuildGlobe(256, 128);
            BalloonQuilt = BuildBalloon(256, 128);
        }

        // ------------------------------------------------------------------ Builder

        // cavity: colour multiplied into the low points; edge: how much convex ridges brighten;
        // macro: strength of big, soft colour and value patches; macroCells: how big they are
        // (cells across the texture); tip: colour the highest points lean towards.
        static Pair Build(int size, HeightFn height, float baseShade, float shadeRange, float strength,
            Color cavity, float edge, float macro, int macroCells, Color tip = default(Color))
        {
            if (tip.a <= 0f) tip = Color.white;
            var h = new float[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    h[y * size + x] = Mathf.Clamp01(height(x / (float)size, y / (float)size, x, y));
            // Height functions were written for their own pixel grid; normals scale with resolution.
            float relief = strength * Mathf.Sqrt(size / 128f);

            var albedo = new Texture2D(size, size, TextureFormat.RGBA32, true, false);
            var normal = new Texture2D(size, size, TextureFormat.RGBA32, true, true);
            albedo.wrapMode = normal.wrapMode = TextureWrapMode.Repeat;
            albedo.filterMode = normal.filterMode = FilterMode.Trilinear;
            albedo.anisoLevel = normal.anisoLevel = 8;

            var ca = new Color[size * size];
            var cn = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float hv = h[y * size + x];
                    float hl = h[y * size + (x - 1 + size) % size];
                    float hr = h[y * size + (x + 1) % size];
                    float hd = h[((y - 1 + size) % size) * size + x];
                    float hu = h[((y + 1) % size) * size + x];
                    float u = x / (float)size, v = y / (float)size;

                    // Ambient-occlusion-ish: low points are darker (and take the cavity colour).
                    float shade = baseShade + shadeRange * hv + (Hash(x * 7, y * 13) - 0.5f) * 0.05f;
                    // Curvature: ridges catch the light, creases go darker, like painted highlights.
                    float lap = (hl + hr + hd + hu) * 0.25f - hv;
                    shade += Mathf.Clamp(-lap * 6f, -0.5f, 0.5f) * edge * 0.5f;
                    // Big soft patches of lighter/darker, warmer/cooler colour (tile seamlessly).
                    float m1 = ValueNoise(u, v, macroCells, 101) - 0.5f;
                    float m2 = ValueNoise(u, v, macroCells * 2, 202) - 0.5f;
                    shade *= 1f + (m1 * 0.8f + m2 * 0.4f) * macro * 2f;
                    float low = Mathf.Clamp01(1f - hv * 1.6f);
                    Color c = new Color(shade, shade, shade, 1f);
                    c = Color.Lerp(c, new Color(shade * cavity.r, shade * cavity.g, shade * cavity.b, 1f), low);
                    c = Color.Lerp(c, new Color(c.r * tip.r, c.g * tip.g, c.b * tip.b, 1f), Mathf.Clamp01((hv - 0.6f) * 2.5f));
                    float warm = m2 * macro * 0.6f;
                    c.r = Mathf.Clamp01(c.r * (1f + warm));
                    c.b = Mathf.Clamp01(c.b * (1f - warm));
                    c.g = Mathf.Clamp01(c.g);
                    c.a = 1f;
                    ca[y * size + x] = c;

                    Vector3 n = new Vector3((hl - hr) * relief, (hd - hu) * relief, 1f).normalized;
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

        // Stockinette knitting: rows of little "V" stitches.
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

        // Soft fuzzy felt.
        static float FeltHeight(float u, float v, int x, int y)
        {
            return ValueNoise(u, v, 8, 1) * 0.5f + ValueNoise(u, v, 16, 2) * 0.3f + Hash(x, y) * 0.2f;
        }

        // Plain-weave cloth with a raised running stitch around the patch.
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

        // Plain-weave cloth at any resolution (maps onto the 128-pixel pattern).
        static float WeaveHeight2(float u, float v, int x, int y)
        {
            int px = Mathf.FloorToInt(u * 128f), py = Mathf.FloorToInt(v * 128f);
            return WeaveHeight(u, v, px, py) + (Hash(x, y) - 0.5f) * 0.04f;
        }

        // Basket weave: horizontal reeds passing over and under vertical stakes.
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

        // Felt lawn with little stitched grass tufts.
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

        // ----- Town patterns -----

        // Rounded cobblestones with sunken mortar between them (jittered cells).
        static float CobbleHeight(float u, float v, int x, int y)
        {
            const int cells = 6;
            float cu = u * cells, cv = v * cells;
            int ix = Mathf.FloorToInt(cu), iy = Mathf.FloorToInt(cv);
            float best = 9f, second = 9f;
            for (int oy = -1; oy <= 1; oy++)
                for (int ox = -1; ox <= 1; ox++)
                {
                    int gx = ((ix + ox) % cells + cells) % cells, gy = ((iy + oy) % cells + cells) % cells;
                    Vector2 c = new Vector2(ix + ox + 0.2f + Hash(gx * 3, gy * 7) * 0.6f, iy + oy + 0.2f + Hash(gx * 11, gy * 5) * 0.6f);
                    float d = Vector2.Distance(new Vector2(cu, cv), c);
                    if (d < best) { second = best; best = d; } else if (d < second) second = d;
                }
            float edge = Mathf.Clamp01((second - best) * 3.2f);
            float dome = Mathf.Sqrt(edge);
            return dome * 0.85f + ValueNoise(u, v, 32, 13) * 0.1f + Hash(x, y) * 0.05f;
        }

        // Brick courses with offset joints.
        static float BrickHeight(float u, float v, int x, int y)
        {
            const int rows = 8, cols = 4;
            float rv = v * rows;
            int row = Mathf.FloorToInt(rv);
            float ru = u * cols + (row % 2 == 0 ? 0f : 0.5f);
            float fu = ru - Mathf.Floor(ru), fv = rv - row;
            float mortar = Mathf.Min(Mathf.Min(fu, 1f - fu) * 3.5f, Mathf.Min(fv, 1f - fv) * 1.8f);
            float face = Mathf.Clamp01(mortar / 0.12f);
            face = Mathf.SmoothStep(0f, 1f, face);
            int bx = Mathf.FloorToInt(ru);
            return face * (0.75f + Hash(bx * 7, row * 13) * 0.2f) + ValueNoise(u, v, 32, 9) * 0.08f * face + Hash(x, y) * 0.04f;
        }

        // Smooth plaster with a soft trowelled texture.
        static float PlasterHeight(float u, float v, int x, int y)
        {
            return ValueNoise(u, v, 4, 21) * 0.4f + ValueNoise(u, v, 16, 22) * 0.35f + Hash(x, y) * 0.15f;
        }

        // Fish-scale roof shingles: rows of rounded tiles, each row overlapping the one below.
        static float ShingleHeight(float u, float v, int x, int y)
        {
            const int rows = 8, cols = 6;
            float rv = v * rows;
            int row = Mathf.FloorToInt(rv);
            float fv = rv - row;
            float ru = u * cols + (row % 2 == 0 ? 0f : 0.5f);
            float fu = ru - Mathf.Floor(ru);
            // Each tile's bottom edge is a semicircle.
            float cx = fu - 0.5f;
            float bottom = 0.5f - Mathf.Sqrt(Mathf.Max(0f, 0.25f - cx * cx));
            float inTile = fv - bottom * 0.9f;
            float h = inTile > 0f ? 0.35f + 0.6f * (1f - fv) : 0.1f;
            return Mathf.Clamp01(h) + ValueNoise(u, v, 32, 17) * 0.08f;
        }

        // Wooden planks with grain, gaps and nail heads.
        static float PlankHeight(float u, float v, int x, int y)
        {
            const int planks = 5;
            float pu = u * planks;
            int p = Mathf.FloorToInt(pu);
            float fu = pu - p;
            float gap = Mathf.Clamp01(Mathf.Min(fu, 1f - fu) / 0.04f);
            float grain = Mathf.Sin((v * 9f + Hash(p, 3) * 5f + ValueNoise(u, v, 8, 31 + p) * 1.5f) * Mathf.PI * 2f) * 0.5f + 0.5f;
            float nail = 0f;
            float nv = Mathf.Repeat(v * 2f + Hash(p, 9) * 0.3f, 1f);
            if (Mathf.Abs(fu - 0.5f) < 0.06f && Mathf.Abs(nv - 0.5f) < 0.03f) nail = 0.3f;
            return gap * (0.55f + grain * 0.35f + Hash(p * 5, 1) * 0.1f) + nail;
        }

        // Damask: mirrored swirls in a diamond repeat, slightly raised like woven brocade.
        static float DamaskHeight(float u, float v, int x, int y)
        {
            float a = u * 2f, b = v * 2f;
            float fu = a - Mathf.Floor(a), fv = b - Mathf.Floor(b);
            float mu = Mathf.Abs(fu - 0.5f) * 2f, mv = Mathf.Abs(fv - 0.5f) * 2f;   // mirror both ways
            float motif = Mathf.Sin(mu * 6f + Mathf.Sin(mv * 5f) * 1.5f) * Mathf.Cos(mv * 4f - mu * 2f);
            float diamond = 1f - Mathf.Clamp01(Mathf.Abs(mu + mv - 0.9f) * 6f);
            float pattern = Mathf.Max(Mathf.Clamp01(motif * 2.2f - 0.6f), diamond * 0.8f);
            float weave = ((x + y) % 2 == 0 ? 0.04f : 0f);
            return pattern * 0.75f + 0.15f + weave;
        }

        // Corrugated cardboard: faint ridges, paper fibres and the odd crease.
        static float CardboardHeight(float u, float v, int x, int y)
        {
            float ridges = Mathf.Sin(u * 16f * Mathf.PI * 2f) * 0.5f + 0.5f;
            float fibres = ValueNoise(u, v, 32, 17) * 0.5f + Hash(x, y) * 0.25f;
            float crease = Mathf.Abs(Mathf.Sin((v * 2f + u * 0.5f + ValueNoise(u, v, 4, 5) * 0.3f) * Mathf.PI * 2f));
            float c = crease > 0.985f ? -0.3f : 0f;
            return 0.35f + ridges * 0.18f + fibres * 0.4f + c;
        }

        // Satin water: soft ripple bands in two blues, three wavy rows of white running stitch,
        // and the odd sequin glinting. The ripples and stitches are raised in the normal map.
        static Pair BuildCraftWater(int size)
        {
            var h = new float[size * size];
            var col = new Color[size * size];
            Color deep = new Color(0.16f, 0.42f, 0.78f), mid = new Color(0.3f, 0.6f, 0.9f), light = new Color(0.62f, 0.85f, 0.98f);
            Color stitch = new Color(0.96f, 0.98f, 1f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size, v = y / (float)size;
                    float tau = Mathf.PI * 2f;
                    float bend = 0.035f * Mathf.Sin(u * tau * 2f) + 0.02f * Mathf.Sin(u * tau * 5f + v * tau);
                    float band = 0.5f + 0.5f * Mathf.Sin((v * 6f + bend * 6f) * tau);
                    float soft = ValueNoise(u, v, 8, 61) * 0.25f;
                    float hv = band * 0.7f + soft;
                    Color c = Color.Lerp(deep, mid, Mathf.Clamp01(band * 1.2f));
                    c = Color.Lerp(c, light, Mathf.Pow(Mathf.Clamp01(band - 0.55f) * 2.2f, 2f) * 0.8f);
                    c *= 0.92f + soft * 0.4f;
                    // Rows of white running stitch following the waves.
                    for (int k = 0; k < 3; k++)
                    {
                        float line = (k + 0.5f) / 3f + 0.03f * Mathf.Sin(u * tau * 3f + k * 2f);
                        float d = Mathf.Abs(Mathf.Repeat(v - line + 0.5f, 1f) - 0.5f);
                        bool dash = Mathf.Repeat(u * 24f + k * 0.33f, 1f) < 0.55f;
                        if (dash && d < 0.007f) { c = stitch; hv = 1f; }
                    }
                    // Sequins.
                    if (Hash(x * 5 + 3, y * 7 + 1) > 0.9975f) { c = Color.Lerp(c, Color.white, 0.85f); hv = 0.9f; }
                    c.a = 1f;
                    col[y * size + x] = c;
                    h[y * size + x] = hv;
                }
            var albedo = new Texture2D(size, size, TextureFormat.RGBA32, true, false);
            var normal = new Texture2D(size, size, TextureFormat.RGBA32, true, true);
            albedo.wrapMode = normal.wrapMode = TextureWrapMode.Repeat;
            albedo.filterMode = normal.filterMode = FilterMode.Trilinear;
            albedo.anisoLevel = normal.anisoLevel = 8;
            var cn = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float hl = h[y * size + (x - 1 + size) % size], hr = h[y * size + (x + 1) % size];
                    float hd = h[((y - 1 + size) % size) * size + x], hu = h[((y + 1) % size) * size + x];
                    Vector3 n = new Vector3((hl - hr) * 4f, (hd - hu) * 4f, 1f).normalized;
                    cn[y * size + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                }
            albedo.SetPixels(col);
            albedo.Apply(true);
            normal.SetPixels(cn);
            normal.Apply(true);
            return new Pair { Albedo = albedo, Normal = normal };
        }

        // ------------------------------------------------------------------ Colour textures

        static Texture2D NewColourTex(int w, int h)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, true, false);
            t.wrapMode = TextureWrapMode.Repeat;
            t.filterMode = FilterMode.Trilinear;
            t.anisoLevel = 4;
            return t;
        }

        static Color Ramp(Color[] stops, float t)
        {
            t = Mathf.Repeat(t, 1f) * stops.Length;
            int i = Mathf.FloorToInt(t);
            float f = t - i;
            f = f * f * (3f - 2f * f);
            return Color.Lerp(stops[i % stops.Length], stops[(i + 1) % stops.Length], f);
        }

        // Marbled paint floating on ink: bands of colour pulled into swirls (tiles seamlessly).
        static Texture2D BuildMarble(int size)
        {
            Color ink = new Color(0.1f, 0.12f, 0.3f);
            Color indigo = new Color(0.22f, 0.2f, 0.48f);
            Color teal = new Color(0.16f, 0.52f, 0.6f);
            Color pink = new Color(0.95f, 0.47f, 0.62f);
            Color butter = new Color(0.98f, 0.83f, 0.4f);
            Color cream = new Color(0.96f, 0.92f, 0.85f);
            Color[] stops = { ink, indigo, ink, teal, cream, pink, ink, indigo, butter, ink };
            var t = NewColourTex(size, size);
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size, v = y / (float)size;
                    float wu = ValueNoise(u, v, 4, 11) * 0.6f + ValueNoise(u, v, 8, 12) * 0.25f;
                    float wv = ValueNoise(u, v, 4, 21) * 0.6f + ValueNoise(u, v, 8, 22) * 0.25f;
                    float band = (u * 2f + v * 1f) + wu * 1.6f + wv * 1.1f + ValueNoise(u, v, 16, 31) * 0.08f;
                    Color c = Ramp(stops, band);
                    // Little bright specks of paint.
                    if (Hash(x * 3, y * 5) > 0.997f) c = Color.Lerp(c, cream, 0.7f);
                    px[y * size + x] = c;
                }
            t.SetPixels(px);
            t.Apply(true);
            return t;
        }

        // A quilted world map: sea patches and land patches, each square edged with stitches.
        static Texture2D BuildGlobe(int w, int h)
        {
            Color[] sea = { new Color(0.45f, 0.62f, 0.85f), new Color(0.5f, 0.68f, 0.88f), new Color(0.4f, 0.56f, 0.8f) };
            Color[] land = { new Color(0.62f, 0.78f, 0.5f), new Color(0.75f, 0.84f, 0.52f), new Color(0.98f, 0.86f, 0.55f),
                             new Color(0.95f, 0.7f, 0.6f), new Color(0.78f, 0.7f, 0.9f), new Color(0.55f, 0.72f, 0.48f) };
            var t = NewColourTex(w, h);
            t.wrapMode = TextureWrapMode.Clamp;
            var px = new Color[w * h];
            const int cellsX = 24, cellsY = 12;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float u = x / (float)w, v = y / (float)h;
                    int cx = Mathf.FloorToInt(u * cellsX), cy = Mathf.FloorToInt(v * cellsY);
                    float landness = ValueNoise((cx + 0.5f) / cellsX, (cy + 0.5f) / cellsY, 6, 41);
                    bool isLand = landness > 0.52f && cy > 0 && cy < cellsY - 1;
                    Color c = isLand ? land[(cx * 7 + cy * 3) % land.Length] : sea[(cx + cy) % sea.Length];
                    if (cy == 0 || cy == cellsY - 1) c = new Color(0.96f, 0.93f, 0.88f); // felt ice caps
                    c *= 0.94f + Hash(x, y) * 0.08f;
                    // Stitches around each patch: dashes just inside the edge.
                    float fu = u * cellsX - cx, fv = v * cellsY - cy;
                    bool nearV = fu < 0.08f || fu > 0.92f, nearH = fv < 0.08f || fv > 0.92f;
                    bool dashV = Mathf.Repeat(fv * 6f, 1f) < 0.55f, dashH = Mathf.Repeat(fu * 6f, 1f) < 0.55f;
                    if ((nearV && dashV && !nearH) || (nearH && dashH && !nearV)) c = Color.Lerp(c, new Color(1f, 0.97f, 0.9f), 0.75f);
                    if (fu < 0.02f || fv < 0.02f) c *= 0.75f; // seam shadow
                    c.a = 1f;
                    px[y * w + x] = c;
                }
            t.SetPixels(px);
            t.Apply(true);
            return t;
        }

        // Long patchwork panels (like balloon gores) in soft colours, with stitched seams.
        static Texture2D BuildBalloon(int w, int h)
        {
            Color[] cols = { new Color(0.93f, 0.6f, 0.62f), new Color(0.98f, 0.86f, 0.55f), new Color(0.64f, 0.74f, 0.88f),
                             new Color(0.96f, 0.9f, 0.8f), new Color(0.78f, 0.7f, 0.9f), new Color(0.65f, 0.8f, 0.62f), new Color(0.98f, 0.72f, 0.6f) };
            var t = NewColourTex(w, h);
            var px = new Color[w * h];
            const int gores = 14, rows = 5;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float u = x / (float)w, v = y / (float)h;
                    int gx = Mathf.FloorToInt(u * gores);
                    // Each gore is split into a few patches at staggered heights.
                    float off = Hash(gx, 7) * 0.5f;
                    int gy = Mathf.FloorToInt(v * rows + off);
                    Color c = cols[(gx * 3 + gy * 5) % cols.Length];
                    c *= 0.95f + Hash(x, y) * 0.07f + (WeaveHeight(Mathf.Repeat(u * 8f, 1f), Mathf.Repeat(v * 4f, 1f), (x * 4) % 128, (y * 4) % 128) - 0.4f) * 0.1f;
                    float fu = u * gores - gx, fv = v * rows + off - gy;
                    bool nearV = fu < 0.07f || fu > 0.93f, nearH = fv < 0.06f || fv > 0.94f;
                    bool dashV = Mathf.Repeat(v * 40f, 1f) < 0.55f, dashH = Mathf.Repeat(u * 60f, 1f) < 0.55f;
                    if ((nearV && dashV) || (nearH && dashH && !nearV)) c = Color.Lerp(c, new Color(1f, 0.97f, 0.9f), 0.8f);
                    if (fu < 0.015f || fv < 0.015f) c *= 0.8f;
                    c.a = 1f;
                    px[y * w + x] = c;
                }
            t.SetPixels(px);
            t.Apply(true);
            return t;
        }

        // Strands wound around a ball of yarn.
        static float WrapHeight(float u, float v, int x, int y)
        {
            float a = Mathf.Sin((v * 22f + Mathf.Sin(u * Mathf.PI * 4f) * 1.6f) * Mathf.PI * 2f) * 0.5f + 0.5f;
            float b = Mathf.Sin(((u + v) * 18f) * Mathf.PI * 2f) * 0.5f + 0.5f;
            return Mathf.Max(a, b * 0.75f) * 0.9f + Hash(x, y) * 0.1f;
        }
    }
}

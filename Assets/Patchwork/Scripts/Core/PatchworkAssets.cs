using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// Creates every material, texture and UI icon at runtime so the prototype needs no imported art.
    /// The look is "handmade": woven fabric with stitched borders, wrapped yarn, soft pastel colours.
    /// </summary>
    public static class PatchworkAssets
    {
        public const string LitMaterialResource = "PW_Lit";

        static Shader litShader;
        static Material litTemplate;
        static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();

        public static Texture2D FabricTex { get; private set; }
        public static Texture2D YarnTex { get; private set; }
        public static Texture2D WaterTex { get; private set; }
        public static Texture2D WindTex { get; private set; }
        public static Texture2D FeltTex { get; private set; }

        // UI
        public static Texture2D GlyphCross { get; private set; }
        public static Texture2D GlyphCircle { get; private set; }
        public static Texture2D GlyphSquare { get; private set; }
        public static Texture2D GlyphTriangle { get; private set; }
        public static Texture2D IconWater { get; private set; }
        public static Texture2D IconYarn { get; private set; }
        public static Texture2D IconWind { get; private set; }
        public static Texture2D IconButton { get; private set; }
        public static Texture2D IconLock { get; private set; }
        public static Texture2D SoftCircle { get; private set; }

        static bool initialised;

        public static void EnsureInit()
        {
            if (initialised) return;
            initialised = true;

            // Prefer the material asset the editor setup creates (keeps the shader in builds);
            // fall back to finding the URP Lit shader directly.
            litTemplate = Resources.Load<Material>(LitMaterialResource);
            if (litTemplate != null) litShader = litTemplate.shader;
            if (litShader == null) litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            FabricTex = MakeFabric(128);
            FeltTex = MakeFelt(64);
            YarnTex = MakeYarn(128);
            WaterTex = MakeWater(64);
            WindTex = MakeWind(64);

            GlyphCross = MakeGlyph(0, new Color(0.49f, 0.70f, 0.91f));
            GlyphCircle = MakeGlyph(1, new Color(1f, 0.42f, 0.45f));
            GlyphSquare = MakeGlyph(2, new Color(0.90f, 0.62f, 0.90f));
            GlyphTriangle = MakeGlyph(3, new Color(0.25f, 0.89f, 0.65f));
            IconWater = MakeIcon(0);
            IconYarn = MakeIcon(1);
            IconWind = MakeIcon(2);
            IconButton = MakeIcon(3);
            IconLock = MakeIcon(4);
            SoftCircle = MakeIcon(5);
        }

        public static Texture2D IconFor(FormType form)
        {
            switch (form)
            {
                case FormType.Water: return IconWater;
                case FormType.Wind: return IconWind;
                default: return IconYarn;
            }
        }

        // ------------------------------------------------------------------ Materials

        public static Material Fabric(Color color)
        {
            return Get("fabric", color, FabricTex, 0.08f, Color.black);
        }

        public static Material Felt(Color color)
        {
            return Get("felt", color, FeltTex, 0.02f, Color.black);
        }

        public static Material Yarn(Color color)
        {
            return Get("yarn", color, YarnTex, 0.05f, Color.black);
        }

        public static Material Water(Color color)
        {
            return Get("water", color, WaterTex, 0.92f, color * 0.25f);
        }

        public static Material Wind(Color color)
        {
            return Get("wind", color, WindTex, 0.3f, color * 0.2f);
        }

        /// <summary>See-through, glossy material (URP Lit set to Transparent) for the water bubble.</summary>
        public static Material Bubble(Color color)
        {
            EnsureInit();
            string key = "bubble_" + ColorUtility.ToHtmlStringRGBA(color);
            Material m;
            if (cache.TryGetValue(key, out m) && m != null) return m;

            m = litTemplate != null ? new Material(litTemplate) : new Material(litShader);
            m.name = "PW_" + key;
            m.SetColor("_BaseColor", color);
            m.SetColor("_Color", color);
            m.SetTexture("_BaseMap", WaterTex);
            m.SetFloat("_Smoothness", 0.95f);
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
            m.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.DisableKeyword("_ALPHATEST_ON");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            m.SetShaderPassEnabled("ShadowCaster", false);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", new Color(color.r, color.g, color.b) * 0.15f);
            cache[key] = m;
            return m;
        }

        public static Material Glow(Color color, float intensity)
        {
            return Get("glow" + intensity.ToString("0.0"), color, FeltTex, 0.2f, color * intensity);
        }

        public static Material Plain(Color color, float smoothness)
        {
            return Get("plain" + smoothness.ToString("0.00"), color, null, smoothness, Color.black);
        }

        static Material Get(string kind, Color color, Texture2D tex, float smoothness, Color emission)
        {
            EnsureInit();
            string key = kind + "_" + ColorUtility.ToHtmlStringRGBA(color);
            Material m;
            if (cache.TryGetValue(key, out m) && m != null) return m;

            m = litTemplate != null ? new Material(litTemplate) : new Material(litShader);
            m.name = "PW_" + key;
            m.SetColor("_BaseColor", color);
            m.SetColor("_Color", color);
            if (tex != null)
            {
                m.SetTexture("_BaseMap", tex);
                m.SetTexture("_MainTex", tex);
            }
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Glossiness", smoothness);
            if (emission.maxColorComponent > 0.001f)
            {
                m.EnableKeyword("_EMISSION");
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                m.SetColor("_EmissionColor", emission);
            }
            cache[key] = m;
            return m;
        }

        /// <summary>Creates a unique (non-cached) material, for things that animate their colour.</summary>
        public static Material Unique(Material source)
        {
            return new Material(source);
        }

        // ------------------------------------------------------------------ Textures

        static Texture2D NewTex(int size, bool mips)
        {
            var t = new Texture2D(size, size, TextureFormat.RGBA32, mips);
            t.wrapMode = TextureWrapMode.Repeat;
            t.filterMode = FilterMode.Bilinear;
            t.anisoLevel = 4;
            return t;
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

        /// <summary>Woven fabric with a dashed running stitch around the border (a quilt patch).</summary>
        static Texture2D MakeFabric(int size)
        {
            var t = NewTex(size, true);
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float weaveX = Mathf.Sin(x * Mathf.PI * 0.5f) * 0.5f + 0.5f;
                    float weaveY = Mathf.Sin(y * Mathf.PI * 0.5f) * 0.5f + 0.5f;
                    bool over = ((x / 2) + (y / 2)) % 2 == 0;
                    float weave = over ? weaveX : weaveY;
                    float v = 0.84f + weave * 0.08f + (Hash(x, y) - 0.5f) * 0.07f;

                    // Seam shadow at the very edge
                    int edge = Mathf.Min(Mathf.Min(x, y), Mathf.Min(size - 1 - x, size - 1 - y));
                    if (edge < 2) v *= 0.78f;

                    Color c = new Color(v, v, v, 1f);

                    // Running stitch: dashes 7px long, 5px gap, 8px in from the edge
                    int inset = 8;
                    bool onH = (y >= inset - 1 && y <= inset) || (y >= size - 1 - inset && y <= size - inset);
                    bool onV = (x >= inset - 1 && x <= inset) || (x >= size - 1 - inset && x <= size - inset);
                    bool inSpanX = x >= inset && x <= size - 1 - inset;
                    bool inSpanY = y >= inset && y <= size - 1 - inset;
                    if ((onH && inSpanX && (x % 12) < 7) || (onV && inSpanY && (y % 12) < 7))
                        c = new Color(1f, 0.97f, 0.9f, 1f);

                    px[y * size + x] = c;
                }
            }
            t.SetPixels(px);
            t.Apply(true);
            return t;
        }

        static Texture2D MakeFelt(int size)
        {
            var t = NewTex(size, true);
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float v = 0.9f + (Hash(x * 3, y * 7) - 0.5f) * 0.12f;
                    px[y * size + x] = new Color(v, v, v, 1f);
                }
            t.SetPixels(px);
            t.Apply(true);
            return t;
        }

        /// <summary>Wrapped strands for a ball of yarn (sphere UVs).</summary>
        static Texture2D MakeYarn(int size)
        {
            var t = NewTex(size, true);
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size, w = y / (float)size;
                    float a = Mathf.Sin((w * 22f + Mathf.Sin(u * Mathf.PI * 4f) * 1.6f) * Mathf.PI * 2f);
                    float b = Mathf.Sin(((u + w) * 18f) * Mathf.PI * 2f);
                    float strand = Mathf.Max(a, b * 0.7f);
                    float v = 0.72f + 0.28f * Mathf.SmoothStep(-0.2f, 1f, strand);
                    v += (Hash(x, y) - 0.5f) * 0.05f;
                    px[y * size + x] = new Color(v, v, v, 1f);
                }
            t.SetPixels(px);
            t.Apply(true);
            return t;
        }

        static Texture2D MakeWater(int size)
        {
            var t = NewTex(size, true);
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.09f, y * 0.09f);
                    float v = 0.82f + n * 0.18f;
                    px[y * size + x] = new Color(v, v, 1f, 1f);
                }
            t.SetPixels(px);
            t.Apply(true);
            return t;
        }

        static Texture2D MakeWind(int size)
        {
            var t = NewTex(size, true);
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float s = Mathf.Sin((y / (float)size * 6f + Mathf.Sin(x / (float)size * Mathf.PI * 2f) * 0.5f) * Mathf.PI * 2f);
                    float v = 0.88f + Mathf.Max(0f, s) * 0.12f;
                    px[y * size + x] = new Color(v, v, v, 1f);
                }
            t.SetPixels(px);
            t.Apply(true);
            return t;
        }

        // ------------------------------------------------------------------ UI icons

        static float Smooth(float dist, float aa)
        {
            // 1 inside (dist < 0), 0 outside, anti-aliased
            return Mathf.Clamp01(0.5f - dist / aa);
        }

        static float SegDist(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 pa = p - a, ba = b - a;
            float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude;
        }

        static Color Over(Color under, Color over, float alpha)
        {
            float a = over.a * alpha;
            float outA = a + under.a * (1f - a);
            if (outA < 0.0001f) return new Color(0, 0, 0, 0);
            Color c = (over * a + under * under.a * (1f - a)) / outA;
            c.a = outA;
            return c;
        }

        /// <summary>PlayStation face-button glyphs: 0 cross, 1 circle, 2 square, 3 triangle.</summary>
        static Texture2D MakeGlyph(int kind, Color symbol)
        {
            const int size = 64;
            var t = NewTex(size, false);
            t.wrapMode = TextureWrapMode.Clamp;
            var px = new Color[size * size];
            Color face = new Color(0.13f, 0.11f, 0.19f, 1f);
            Color rim = new Color(1f, 1f, 1f, 0.9f);
            float aa = 1.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x - size / 2f + 0.5f, y - size / 2f + 0.5f);
                    float r = p.magnitude;
                    Color c = new Color(0, 0, 0, 0);
                    c = Over(c, rim, Smooth(r - 31f, aa));
                    c = Over(c, face, Smooth(r - 28.5f, aa));

                    float d;
                    switch (kind)
                    {
                        case 0:
                            d = Mathf.Min(SegDist(p, new Vector2(-11, -11), new Vector2(11, 11)),
                                          SegDist(p, new Vector2(-11, 11), new Vector2(11, -11))) - 3f;
                            break;
                        case 1:
                            d = Mathf.Abs(r - 12f) - 3f;
                            break;
                        case 2:
                            d = Mathf.Abs(Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.y)) - 11f) - 2.8f;
                            break;
                        default:
                            Vector2 a = new Vector2(0, 13), b = new Vector2(-12.5f, -9), cc = new Vector2(12.5f, -9);
                            d = Mathf.Min(SegDist(p, a, b), Mathf.Min(SegDist(p, b, cc), SegDist(p, cc, a))) - 2.8f;
                            break;
                    }
                    c = Over(c, symbol, Smooth(d, aa));
                    px[y * size + x] = c;
                }
            t.SetPixels(px);
            t.Apply(false);
            return t;
        }

        /// <summary>0 water drop, 1 yarn ball, 2 wind swirl, 3 sewing button, 4 lock, 5 soft circle.</summary>
        static Texture2D MakeIcon(int kind)
        {
            const int size = 128;
            var t = NewTex(size, false);
            t.wrapMode = TextureWrapMode.Clamp;
            var px = new Color[size * size];
            float aa = 2f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    // Normalised coordinates, -1..1, y up
                    Vector2 p = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f);
                    float px1 = 2f / size; // one pixel in normalised units
                    Color c = new Color(0, 0, 0, 0);
                    switch (kind)
                    {
                        case 0: // water droplet
                        {
                            Vector2 cen = new Vector2(0f, -0.22f);
                            float circle = (p - cen).magnitude - 0.55f;
                            float tri = 1f;
                            if (p.y > -0.22f && p.y < 0.85f)
                            {
                                float halfW = 0.55f * (0.85f - p.y) / 1.07f;
                                tri = Mathf.Abs(p.x) - halfW;
                            }
                            float d = Mathf.Min(circle, tri);
                            c = Over(c, new Color(0.15f, 0.35f, 0.65f, 1f), Smooth((d - 0.06f) / px1, aa));
                            c = Over(c, new Color(0.40f, 0.70f, 0.98f, 1f), Smooth(d / px1, aa));
                            float hl = (p - new Vector2(-0.2f, -0.05f)).magnitude - 0.14f;
                            c = Over(c, new Color(1f, 1f, 1f, 0.85f), Smooth(hl / px1, aa));
                            break;
                        }
                        case 1: // yarn ball
                        {
                            float r = p.magnitude;
                            float d = r - 0.7f;
                            c = Over(c, new Color(0.55f, 0.18f, 0.3f, 1f), Smooth((d - 0.06f) / px1, aa));
                            c = Over(c, new Color(0.93f, 0.45f, 0.58f, 1f), Smooth(d / px1, aa));
                            if (d < 0f)
                            {
                                float s1 = Mathf.Abs(Mathf.Sin((p.x * 0.8f + p.y * 0.6f) * 9f));
                                float s2 = Mathf.Abs(Mathf.Sin((p.x * -0.5f + p.y * 0.85f) * 7f));
                                float stripe = Mathf.Min(s1, s2 + 0.3f);
                                c = Over(c, new Color(0.7f, 0.25f, 0.4f, 1f), Smooth((stripe - 0.18f) / 0.1f, 1f) * Smooth(d / px1, aa));
                            }
                            float tail = SegDist(p, new Vector2(0.5f, -0.5f), new Vector2(0.85f, -0.82f)) - 0.05f;
                            c = Over(c, new Color(0.93f, 0.45f, 0.58f, 1f), Smooth(tail / px1, aa));
                            break;
                        }
                        case 2: // wind swirls
                        {
                            Color stroke = new Color(0.55f, 0.85f, 0.8f, 1f);
                            float d1 = Mathf.Abs(p.y - 0.35f - 0.08f * Mathf.Sin(p.x * 5f)) - 0.07f;
                            if (p.x < -0.75f || p.x > 0.45f) d1 = 1f;
                            float d2 = Mathf.Abs(p.y + 0.0f - 0.08f * Mathf.Sin(p.x * 5f + 1f)) - 0.07f;
                            if (p.x < -0.85f || p.x > 0.75f) d2 = 1f;
                            float d3 = Mathf.Abs(p.y + 0.38f - 0.08f * Mathf.Sin(p.x * 5f + 2f)) - 0.07f;
                            if (p.x < -0.6f || p.x > 0.3f) d3 = 1f;
                            float curl = Mathf.Abs((p - new Vector2(0.45f, 0.5f)).magnitude - 0.16f) - 0.07f;
                            if (p.y < 0.5f && p.x < 0.45f) curl = 1f;
                            float d = Mathf.Min(Mathf.Min(d1, d2), Mathf.Min(d3, curl));
                            c = Over(c, new Color(1f, 1f, 1f, 1f), Smooth((d - 0.06f) / px1, aa));
                            c = Over(c, stroke, Smooth(d / px1, aa));
                            break;
                        }
                        case 3: // sewing button
                        {
                            float r = p.magnitude;
                            float d = r - 0.8f;
                            c = Over(c, new Color(0.6f, 0.4f, 0.05f, 1f), Smooth((d - 0.06f) / px1, aa));
                            c = Over(c, new Color(1f, 0.8f, 0.25f, 1f), Smooth(d / px1, aa));
                            float ring = Mathf.Abs(r - 0.6f) - 0.04f;
                            c = Over(c, new Color(0.8f, 0.55f, 0.1f, 1f), Smooth(ring / px1, aa));
                            for (int i = 0; i < 4; i++)
                            {
                                Vector2 hp = new Vector2((i % 2 == 0 ? -1f : 1f) * 0.2f, (i < 2 ? -1f : 1f) * 0.2f);
                                float hd = (p - hp).magnitude - 0.1f;
                                c = Over(c, new Color(0.45f, 0.28f, 0.05f, 1f), Smooth(hd / px1, aa));
                            }
                            break;
                        }
                        case 4: // lock
                        {
                            float body = Mathf.Max(Mathf.Abs(p.x) - 0.5f, Mathf.Abs(p.y + 0.25f) - 0.38f);
                            float shackle = Mathf.Abs((p - new Vector2(0f, 0.15f)).magnitude - 0.32f) - 0.08f;
                            if (p.y < 0.15f) shackle = 1f;
                            float d = Mathf.Min(body, shackle);
                            c = Over(c, new Color(0.85f, 0.85f, 0.9f, 1f), Smooth(d / px1, aa));
                            break;
                        }
                        default: // soft circle
                        {
                            float r = p.magnitude;
                            c = new Color(1f, 1f, 1f, Mathf.Clamp01(1f - r) * Mathf.Clamp01(1f - r));
                            break;
                        }
                    }
                    px[y * size + x] = c;
                }
            t.SetPixels(px);
            t.Apply(false);
            return t;
        }
    }
}

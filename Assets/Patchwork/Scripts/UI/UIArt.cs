using UnityEngine;

namespace Patchwork
{
    // Generated UI art: themed card patterns (waves, knit, swirls) and the pause-menu icons.
    // Icons are white silhouettes so they can be tinted.
    public static class UIArt
    {
        public static Texture2D PatternWaves { get; private set; }
        public static Texture2D PatternKnit { get; private set; }
        public static Texture2D PatternSwirl { get; private set; }
        public static Texture2D PatternDots { get; private set; }

        public static Texture2D IconPlay { get; private set; }
        public static Texture2D IconRestart { get; private set; }
        public static Texture2D IconHome { get; private set; }
        public static Texture2D IconPad { get; private set; }
        public static Texture2D IconNote { get; private set; }
        public static Texture2D IconDoor { get; private set; }
        public static Texture2D IconNeedle { get; private set; }

        static bool built;

        public static void EnsureBuilt()
        {
            if (built) return;
            built = true;
            PatternWaves = Pattern(0);
            PatternKnit = Pattern(1);
            PatternSwirl = Pattern(2);
            PatternDots = Pattern(3);
            IconPlay = Icon(0);
            IconRestart = Icon(1);
            IconHome = Icon(2);
            IconPad = Icon(3);
            IconNote = Icon(4);
            IconDoor = Icon(5);
            IconNeedle = Icon(6);
        }

        static float Smooth(float d, float aa) { return Mathf.Clamp01(0.5f - d / aa); }

        static float Seg(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 pa = p - a, ba = b - a;
            float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude;
        }

        // Tileable 64x64 white-on-transparent patterns.
        static Texture2D Pattern(int kind)
        {
            const int size = 64;
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
            t.wrapMode = TextureWrapMode.Repeat;
            t.filterMode = FilterMode.Bilinear;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size, v = y / (float)size;
                    float a;
                    switch (kind)
                    {
                        case 0: // gentle waves
                        {
                            float w = Mathf.Abs(Mathf.Repeat(v * 4f + Mathf.Sin(u * Mathf.PI * 4f) * 0.18f, 1f) - 0.5f);
                            a = Smooth(w - 0.06f, 0.04f);
                            break;
                        }
                        case 1: // knit Vs
                        {
                            float cu = u * 4f, cv = v * 4f;
                            Vector2 p = new Vector2(cu - Mathf.Floor(cu), cv - Mathf.Floor(cv));
                            float d = Mathf.Min(Seg(p, new Vector2(0.1f, 0.9f), new Vector2(0.48f, 0.15f)),
                                                Seg(p, new Vector2(0.9f, 0.9f), new Vector2(0.52f, 0.15f)));
                            a = Smooth(d - 0.1f, 0.05f);
                            break;
                        }
                        case 2: // swirls
                        {
                            float cu = u * 2f, cv = v * 2f;
                            Vector2 p = new Vector2(cu - Mathf.Floor(cu), cv - Mathf.Floor(cv)) - new Vector2(0.5f, 0.5f);
                            float r = p.magnitude;
                            float ang = Mathf.Atan2(p.y, p.x);
                            float spiral = Mathf.Abs(Mathf.Repeat(r * 5f - ang / (Mathf.PI * 2f), 1f) - 0.5f);
                            a = r < 0.42f ? Smooth(spiral - 0.08f, 0.06f) : 0f;
                            break;
                        }
                        default: // polka dots
                        {
                            float cu = u * 4f, cv = v * 4f;
                            Vector2 p = new Vector2(cu - Mathf.Floor(cu), cv - Mathf.Floor(cv)) - new Vector2(0.5f, 0.5f);
                            a = Smooth(p.magnitude - 0.16f, 0.05f);
                            break;
                        }
                    }
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            t.SetPixels(px);
            t.Apply(false);
            return t;
        }

        // 128x128 white icons (y up).
        static Texture2D Icon(int kind)
        {
            const int size = 128;
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
            t.wrapMode = TextureWrapMode.Clamp;
            t.filterMode = FilterMode.Bilinear;
            var px = new Color[size * size];
            float pixel = 2f / size;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f);
                    float d;
                    switch (kind)
                    {
                        case 0: // play triangle
                        {
                            Vector2 a = new Vector2(-0.45f, 0.6f), b = new Vector2(-0.45f, -0.6f), c = new Vector2(0.62f, 0f);
                            float e = Mathf.Min(Seg(p, a, b), Mathf.Min(Seg(p, b, c), Seg(p, c, a)));
                            bool inside = p.x > -0.45f && Mathf.Abs(p.y) < 0.6f * (0.62f - p.x) / 1.07f;
                            d = inside ? -e : e;
                            d -= 0.06f;
                            break;
                        }
                        case 1: // circular arrow
                        {
                            float r = p.magnitude;
                            float ang = Mathf.Atan2(p.y, p.x) * Mathf.Rad2Deg;
                            float ring = Mathf.Abs(r - 0.55f) - 0.12f;
                            if (ang > 60f && ang < 120f) ring = 1f; // gap
                            Vector2 tip = new Vector2(Mathf.Cos(60f * Mathf.Deg2Rad), Mathf.Sin(60f * Mathf.Deg2Rad)) * 0.55f;
                            float head = Mathf.Min(Seg(p, tip + new Vector2(-0.3f, 0.05f), tip), Seg(p, tip + new Vector2(0.05f, -0.3f), tip)) - 0.09f;
                            d = Mathf.Min(ring, head);
                            break;
                        }
                        case 2: // house
                        {
                            float roof = Mathf.Min(Seg(p, new Vector2(-0.75f, 0.05f), new Vector2(0f, 0.75f)), Seg(p, new Vector2(0f, 0.75f), new Vector2(0.75f, 0.05f))) - 0.11f;
                            float body = Mathf.Max(Mathf.Abs(p.x) - 0.5f, Mathf.Abs(p.y + 0.35f) - 0.4f);
                            float door = Mathf.Max(Mathf.Abs(p.x) - 0.14f, Mathf.Abs(p.y + 0.52f) - 0.24f);
                            d = Mathf.Min(roof, Mathf.Max(body, -door));
                            break;
                        }
                        case 3: // game controller
                        {
                            float bodyD = Mathf.Max(Mathf.Abs(p.x) - 0.6f, Mathf.Abs(p.y) - 0.25f);
                            float l = (p - new Vector2(-0.55f, -0.15f)).magnitude - 0.32f;
                            float r = (p - new Vector2(0.55f, -0.15f)).magnitude - 0.32f;
                            float shape = Mathf.Min(bodyD, Mathf.Min(l, r));
                            float cross = Mathf.Min(Mathf.Max(Mathf.Abs(p.x + 0.45f) - 0.04f, Mathf.Abs(p.y) - 0.15f),
                                                    Mathf.Max(Mathf.Abs(p.x + 0.45f) - 0.15f, Mathf.Abs(p.y) - 0.04f));
                            float b1 = (p - new Vector2(0.42f, 0.06f)).magnitude - 0.06f;
                            float b2 = (p - new Vector2(0.55f, -0.07f)).magnitude - 0.06f;
                            d = Mathf.Max(shape, -Mathf.Min(cross, Mathf.Min(b1, b2)));
                            break;
                        }
                        case 4: // music note
                        {
                            float head1 = (new Vector2((p.x + 0.38f) / 1.3f, p.y + 0.5f)).magnitude - 0.18f;
                            float head2 = (new Vector2((p.x - 0.4f) / 1.3f, p.y + 0.35f)).magnitude - 0.18f;
                            float stem1 = Seg(p, new Vector2(-0.16f, -0.5f), new Vector2(-0.16f, 0.55f)) - 0.05f;
                            float stem2 = Seg(p, new Vector2(0.62f, -0.35f), new Vector2(0.62f, 0.7f)) - 0.05f;
                            float beam = Seg(p, new Vector2(-0.16f, 0.55f), new Vector2(0.62f, 0.7f)) - 0.1f;
                            d = Mathf.Min(Mathf.Min(head1, head2), Mathf.Min(Mathf.Min(stem1, stem2), beam));
                            break;
                        }
                        case 5: // door with knob
                        {
                            float frame = Mathf.Max(Mathf.Abs(p.x) - 0.45f, Mathf.Abs(p.y) - 0.75f);
                            float inner = Mathf.Max(Mathf.Abs(p.x) - 0.33f, Mathf.Abs(p.y + 0.05f) - 0.62f);
                            float knob = (p - new Vector2(0.18f, -0.05f)).magnitude - 0.07f;
                            d = Mathf.Min(Mathf.Max(frame, -inner), knob);
                            float panel = Mathf.Max(Mathf.Abs(p.x) - 0.33f, Mathf.Abs(p.y + 0.05f) - 0.62f);
                            d = Mathf.Min(d, Mathf.Max(panel, -Mathf.Max(Mathf.Abs(p.x) - 0.2f, Mathf.Abs(p.y - 0.25f) - 0.2f)) + 0.0f);
                            break;
                        }
                        default: // sewing needle with thread (pointing right)
                        {
                            float shaft = Seg(p, new Vector2(-0.75f, 0f), new Vector2(0.8f, 0f)) - 0.07f * Mathf.Clamp01((0.85f - p.x) * 1.2f);
                            float eye = Mathf.Abs((new Vector2((p.x + 0.6f) / 2.2f, p.y)).magnitude - 0.035f) - 0.012f;
                            d = Mathf.Max(shaft, -((new Vector2((p.x + 0.6f) / 2.2f, p.y)).magnitude - 0.03f));
                            d = Mathf.Min(d, Mathf.Max(eye, 1f));
                            break;
                        }
                    }
                    px[y * size + x] = new Color(1f, 1f, 1f, Smooth(d / pixel, 2f));
                }
            t.SetPixels(px);
            t.Apply(false);
            return t;
        }
    }
}

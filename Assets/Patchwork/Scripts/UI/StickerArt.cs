using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    public enum StickerId { Star, Heart, Flower, Smiley, Moon, Music, Crown, Fish, Rainbow, Paw }

    // The sticker pictures, drawn in code: a shape, a thick white die-cut border round it, a soft
    // shadow and a glossy shine, like a real sheet of puffy stickers. Also draws the dashed
    // outline used on sticker boards ("stick a star here").
    public static class StickerArt
    {
        const int Size = 128;
        static readonly Dictionary<int, Texture2D> cache = new Dictionary<int, Texture2D>();
        static readonly Dictionary<int, Texture2D> outlines = new Dictionary<int, Texture2D>();

        public static readonly StickerId[] Starters = { StickerId.Star, StickerId.Heart, StickerId.Flower, StickerId.Smiley };

        public static string Name(StickerId id)
        {
            switch (id)
            {
                case StickerId.Star: return "Gold Star";
                case StickerId.Heart: return "Puffy Heart";
                case StickerId.Flower: return "Button Flower";
                case StickerId.Smiley: return "Smiley";
                case StickerId.Moon: return "Sleepy Moon";
                case StickerId.Music: return "Music Note";
                case StickerId.Crown: return "Little Crown";
                case StickerId.Fish: return "Goldfish";
                case StickerId.Rainbow: return "Rainbow";
                default: return "Paw Print";
            }
        }

        public static Texture2D Get(StickerId id)
        {
            Texture2D t;
            if (cache.TryGetValue((int)id, out t) && t != null) return t;
            t = Draw(id, false);
            cache[(int)id] = t;
            return t;
        }

        public static Texture2D Outline(StickerId id)
        {
            Texture2D t;
            if (outlines.TryGetValue((int)id, out t) && t != null) return t;
            t = Draw(id, true);
            outlines[(int)id] = t;
            return t;
        }

        static Texture2D Draw(StickerId id, bool outlineOnly)
        {
            var colour = new Color[Size * Size];
            var mask = new float[Size * Size];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    // -1..1 with y up, inset so the border fits.
                    float u = (x + 0.5f) / Size * 2f - 1f, v = (y + 0.5f) / Size * 2f - 1f;
                    u *= 1.22f; v *= 1.22f;
                    Color c;
                    float a = Shape(id, u, v, out c);
                    mask[y * Size + x] = a;
                    colour[y * Size + x] = c;
                }

            // Grow the shape for the white border (distance in pixels to the nearest shape pixel).
            const int R = 7;
            var border = new float[Size * Size];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    float best = 99f;
                    for (int dy = -R; dy <= R; dy++)
                    {
                        int yy = y + dy;
                        if (yy < 0 || yy >= Size) continue;
                        for (int dx = -R; dx <= R; dx++)
                        {
                            int xx = x + dx;
                            if (xx < 0 || xx >= Size) continue;
                            if (mask[yy * Size + xx] < 0.5f) continue;
                            float d = dx * dx + dy * dy;
                            if (d < best) best = d;
                        }
                    }
                    border[y * Size + x] = Mathf.Clamp01(R + 0.5f - Mathf.Sqrt(best));
                }

            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, true);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Trilinear;
            var px = new Color[Size * Size];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    int i = y * Size + x;
                    if (outlineOnly)
                    {
                        // Dashed line along the edge of the shape, on a transparent background.
                        float edge = border[i] * (1f - Mathf.Clamp01(mask[i] * 1.6f));
                        float ang = Mathf.Atan2(y - Size / 2f, x - Size / 2f);
                        float dash = Mathf.Sin(ang * 22f) > -0.2f ? 1f : 0f;
                        float a = Mathf.Clamp01(edge * 3f) * dash * 0.9f;
                        px[i] = new Color(0.25f, 0.2f, 0.3f, a);
                        continue;
                    }
                    float m = mask[i];
                    Color c = Color.Lerp(new Color(1f, 1f, 0.98f, 1f), colour[i], m);
                    // Puffy shading: lighter top-left, darker bottom-right, plus a glossy streak.
                    float u = x / (float)Size, v = y / (float)Size;
                    float shade = 1f + (v - u) * 0.18f * m;
                    c = new Color(c.r * shade, c.g * shade, c.b * shade, 1f);
                    float gloss = Mathf.Clamp01(1f - Mathf.Abs((u + (1f - v)) - 0.62f) * 9f) * m * 0.35f;
                    c = Color.Lerp(c, Color.white, gloss);
                    c.a = Mathf.Max(border[i], m);
                    px[i] = c;
                }
            tex.SetPixels(px);
            tex.Apply(true);
            return tex;
        }

        // Coverage (0..1) of the picture at u,v in -1..1, and its colour there.
        static float Shape(StickerId id, float u, float v, out Color c)
        {
            switch (id)
            {
                case StickerId.Star:
                {
                    c = new Color(1f, 0.8f, 0.2f);
                    // Point-in-polygon on a ten-corner star.
                    bool inside = false;
                    int j = 9;
                    for (int k = 0; k < 10; k++)
                    {
                        float ak = Mathf.PI / 2f + k * Mathf.PI / 5f, aj = Mathf.PI / 2f + j * Mathf.PI / 5f;
                        float rk = k % 2 == 0 ? 0.95f : 0.42f, rj = j % 2 == 0 ? 0.95f : 0.42f;
                        float xi = Mathf.Cos(ak) * rk, yi = Mathf.Sin(ak) * rk, xj = Mathf.Cos(aj) * rj, yj = Mathf.Sin(aj) * rj;
                        if ((yi > v) != (yj > v) && u < (xj - xi) * (v - yi) / (yj - yi) + xi) inside = !inside;
                        j = k;
                    }
                    return inside ? 1f : 0f;
                }
                case StickerId.Heart:
                {
                    c = new Color(0.95f, 0.35f, 0.5f);
                    float x = u * 1.1f, y = v * 1.1f + 0.25f;
                    float q = x * x + y * y - 0.55f;
                    return q * q * q - x * x * y * y * y < 0f ? 1f : 0f;
                }
                case StickerId.Flower:
                {
                    float r = Mathf.Sqrt(u * u + v * v);
                    if (r < 0.3f)
                    {
                        // A sewing button for the middle, with four holes.
                        c = new Color(1f, 0.82f, 0.3f);
                        for (int i = 0; i < 4; i++)
                        {
                            float a = i * Mathf.PI / 2f + Mathf.PI / 4f;
                            float hx = u - Mathf.Cos(a) * 0.1f, hy = v - Mathf.Sin(a) * 0.1f;
                            if (hx * hx + hy * hy < 0.0016f) c = new Color(0.6f, 0.4f, 0.15f);
                        }
                        return 1f;
                    }
                    c = new Color(0.6f, 0.75f, 1f);
                    for (int i = 0; i < 6; i++)
                    {
                        float a = i * Mathf.PI / 3f;
                        float px = u - Mathf.Cos(a) * 0.52f, py = v - Mathf.Sin(a) * 0.52f;
                        if (px * px + py * py < 0.1f) return 1f;
                    }
                    return 0f;
                }
                case StickerId.Smiley:
                {
                    float r = Mathf.Sqrt(u * u + v * v);
                    c = new Color(1f, 0.85f, 0.25f);
                    if (r > 0.85f) return 0f;
                    var ink = new Color(0.25f, 0.18f, 0.2f);
                    for (int s = -1; s <= 1; s += 2)
                    {
                        float ex = u - s * 0.28f, ey = v - 0.22f;
                        if (ex * ex / 0.01f + ey * ey / 0.03f < 1f) c = ink;
                    }
                    float mr = Mathf.Sqrt(u * u + (v + 0.05f) * (v + 0.05f));
                    if (v < -0.08f && Mathf.Abs(mr - 0.48f) < 0.06f) c = ink;
                    if (Mathf.Abs(u) > 0.42f && Mathf.Abs(u) < 0.62f && v < -0.05f && v > -0.25f) c = Color.Lerp(c, new Color(1f, 0.55f, 0.55f), 0.6f);
                    return 1f;
                }
                case StickerId.Moon:
                {
                    c = new Color(0.98f, 0.9f, 0.55f);
                    bool inA = u * u + v * v < 0.72f;
                    float bx = u - 0.38f, by = v - 0.22f;
                    bool inB = bx * bx + by * by < 0.5f;
                    if (inA && !inB)
                    {
                        // A closed sleepy eye.
                        float ex = u + 0.38f, ey = v - 0.05f;
                        if (Mathf.Abs(ex * ex + ey * ey - 0.012f) < 0.008f && ey < 0f) c = new Color(0.35f, 0.3f, 0.3f);
                        return 1f;
                    }
                    return 0f;
                }
                case StickerId.Music:
                {
                    c = new Color(0.45f, 0.35f, 0.85f);
                    float hx = (u + 0.2f) * 1.3f, hy = (v + 0.48f) * 1.8f;
                    float hx2 = hx * 0.85f - hy * 0.5f, hy2 = hx * 0.5f + hy * 0.85f;
                    if (hx2 * hx2 + hy2 * hy2 < 0.16f) return 1f;
                    if (u > 0.05f && u < 0.17f && v > -0.45f && v < 0.75f) return 1f;
                    if (u > 0.1f && v > 0.35f && v < 0.75f && u < 0.17f + (0.75f - v) * 1.1f && u < 0.6f) return 1f;
                    return 0f;
                }
                case StickerId.Crown:
                {
                    c = new Color(1f, 0.78f, 0.25f);
                    if (v < -0.5f || v > 0.6f || Mathf.Abs(u) > 0.8f) return 0f;
                    if (v < -0.1f) { if (Mathf.Abs(u) < 0.12f && Mathf.Abs(v + 0.3f) < 0.1f) c = new Color(0.9f, 0.3f, 0.5f); return 1f; }
                    float tooth = Mathf.Abs(Mathf.Repeat(u + 0.8f, 0.5333f) - 0.2667f) / 0.2667f;   // 0 at the points
                    float top = 0.6f - tooth * 0.7f;
                    return v < top ? 1f : 0f;
                }
                case StickerId.Fish:
                {
                    c = new Color(1f, 0.55f, 0.2f);
                    float bx = (u + 0.1f) / 0.62f, by = v / 0.42f;
                    if (bx * bx + by * by < 1f)
                    {
                        float ex = u + 0.38f, ey = v - 0.1f;
                        if (ex * ex + ey * ey < 0.006f) c = new Color(0.2f, 0.15f, 0.2f);
                        if (Mathf.Abs(u - 0.05f) < 0.04f && Mathf.Abs(v) < 0.3f) c = new Color(1f, 0.85f, 0.6f);
                        return 1f;
                    }
                    if (u > 0.4f && u < 0.9f && Mathf.Abs(v) < (u - 0.4f) * 1.1f) return 1f;
                    return 0f;
                }
                case StickerId.Rainbow:
                {
                    float r = Mathf.Sqrt(u * u + (v + 0.35f) * (v + 0.35f));
                    c = Color.white;
                    if (v < -0.35f) return 0f;
                    Color[] bands = { new Color(0.95f, 0.35f, 0.35f), new Color(1f, 0.65f, 0.25f), new Color(1f, 0.88f, 0.3f), new Color(0.45f, 0.8f, 0.45f), new Color(0.4f, 0.6f, 0.95f) };
                    for (int i = 0; i < bands.Length; i++)
                    {
                        float outer = 0.95f - i * 0.11f;
                        if (r < outer && r > outer - 0.11f) { c = bands[i]; return 1f; }
                    }
                    return 0f;
                }
                default:
                {
                    c = new Color(0.55f, 0.38f, 0.3f);
                    float px = u / 0.45f, py = (v + 0.3f) / 0.36f;
                    if (px * px + py * py < 1f) return 1f;
                    float[] tx = { -0.5f, -0.18f, 0.18f, 0.5f };
                    float[] ty = { 0.2f, 0.5f, 0.5f, 0.2f };
                    for (int i = 0; i < 4; i++)
                    {
                        float dx = (u - tx[i]) / 0.17f, dy = (v - ty[i]) / 0.21f;
                        if (dx * dx + dy * dy < 1f) return 1f;
                    }
                    return 0f;
                }
            }
        }
    }
}

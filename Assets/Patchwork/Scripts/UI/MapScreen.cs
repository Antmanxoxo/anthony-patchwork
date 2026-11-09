using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Patchwork
{
    // A landmark on a level's map.
    public class MapPin
    {
        public enum Kind { Place, Band, Shop, Area }   // Area: a whole region, named in big letters with no pin
        public string Name;
        public Vector3 Position;
        public Kind Type;
        public string Id;        // for Band pins: the collected id
        public Color Tint = new Color(0.9f, 0.25f, 0.25f);

        public MapPin(string name, Vector3 pos, Kind type = Kind.Place, string id = null)
        {
            Name = name;
            Position = pos;
            Type = type;
            Id = id;
        }
    }

    // The pull-up map (Map button: D-pad left / M). A felt tourist map stitched onto a quilt: a
    // bird's-eye picture of the level with region names on felt tags, numbered badges for the
    // places (named in the key beside it), the Town Band's buttons and Patch's arrow, and one
    // tidy panel with the key and what's left to find.
    // The painting is taken once (from a camera high overhead) the first time the map opens.
    public class MapScreen
    {
        readonly PatchworkGame game;
        Texture2D painting, arrowTex, starTex;
        float openTime;
        Bounds area;

        public MapScreen(PatchworkGame game)
        {
            this.game = game;
        }

        public static bool Available(LevelInfo level) { return level != null && level.MapPins.Count > 0; }

        public void Open()
        {
            openTime = 0f;
            if (painting == null) Prepare();
        }

        public void Update(float udt)
        {
            openTime += udt;
            var input = game.Input;
            if (openTime < 0.15f) return;
            if (input.Map.WasPressedThisFrame() || input.Cancel.WasPressedThisFrame() || input.Pause.WasPressedThisFrame())
                game.CloseMap();
            else if (CanTravel() && input.Confirm.WasPressedThisFrame())
            {
                // Hop straight back to where the Pod landed.
                var player = game.Player;
                if (YarnHorse.Ridden != null) YarnHorse.Ridden.Dismount(false);
                game.CloseMap();
                FxBurst.Spawn(FxBurst.Kind.Poof, player.Motor.Center, Color.white);
                player.Motor.Teleport(game.Level.Spawn, game.Level.SpawnYaw);
                FxBurst.Spawn(FxBurst.Kind.Sparkle, game.Level.Spawn + Vector3.up, new Color(1f, 0.85f, 0.5f));
                Sfx.Play(SfxId.PodRocket, 0.45f);
                game.ShowToast("Back at the Pod!");
            }
        }

        bool CanTravel()
        {
            var level = game.Level;
            return level != null && !level.IsHub && level.Spawn != Vector3.zero && game.Player != null && !game.Player.Motor.ExternalControl;
        }

        // ------------------------------------------------------------------ Painting the map
        // A camera high overhead renders the level into a texture for a few frames (started when
        // the level loads), then the picture is read back and turned into a watercolour.

        Camera capCam;
        RenderTexture capRT;
        int capFrames;

        public void Prepare()
        {
            if (painting != null || capCam != null) return;
            var level = game.Level;
            area = level.MapArea.size.sqrMagnitude > 1f ? level.MapArea : level.Area;
            float half = Mathf.Max(area.extents.x, area.extents.z) + 2f;
            const int size = 1024;
            capRT = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32);
            capRT.Create();
            var camGo = new GameObject("MapCamera");
            capCam = camGo.AddComponent<Camera>();
            capCam.orthographic = true;
            capCam.orthographicSize = half;
            capCam.transform.position = new Vector3(area.center.x, level.MapCameraHeight, area.center.z);
            capCam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            capCam.nearClipPlane = 0.3f;
            capCam.farClipPlane = level.MapCameraHeight + 30f;
            capCam.clearFlags = CameraClearFlags.SolidColor;
            capCam.backgroundColor = new Color(0.93f, 0.88f, 0.76f);
            // Leave out the grass and tiny bits so the picture is clean shapes: roads, roofs, water, trees.
            capCam.cullingMask = ~((1 << 2) | (1 << RenderOptimizer.GrassLayer) | (1 << RenderOptimizer.SmallLayer) | (1 << RenderOptimizer.MediumLayer));
            capCam.depth = -10f;
            capCam.targetTexture = capRT;
            var data = capCam.GetUniversalAdditionalCameraData();
            if (data != null) data.renderPostProcessing = false;
            capFrames = 0;
        }

        // Called every frame by the game; finishes the painting once the camera has drawn.
        public void Tick()
        {
            if (capCam == null) return;
            capFrames++;
            if (capFrames < 4) return;
            try
            {
                int size = capRT.width;
                var raw = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var prev = RenderTexture.active;
                RenderTexture.active = capRT;
                raw.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                raw.Apply(false);
                RenderTexture.active = prev;
                painting = Stylize(raw);
                Object.Destroy(raw);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Map painting failed: " + e.Message);
            }
            capCam.targetTexture = null;
            Object.Destroy(capCam.gameObject);
            capCam = null;
            capRT.Release();
            Object.Destroy(capRT);
            capRT = null;
        }

        // Turns the snapshot into a clean illustrated map, like one cut from coloured felt: the
        // picture is smoothed, every pixel is matched to one of a few flat map colours (meadow,
        // wood, water, path, stone, and soft pastels for roofs and toys), and a thin darker line
        // is drawn wherever two colours meet.
        static Texture2D Stylize(Texture2D src)
        {
            int w = src.width, h = src.height;
            var px = Blur(Blur(src.GetPixels(), w, h, 2), w, h, 2);
            var kind = new int[w * h];
            var flat = new Color[w * h];
            for (int i = 0; i < px.Length; i++) flat[i] = MapColour(px[i], out kind[i]);
            // Tidy stray specks: a pixel takes the commonest kind around it.
            var tidy = new int[w * h];
            var counts = new int[64];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    System.Array.Clear(counts, 0, counts.Length);
                    int best = kind[y * w + x], bestN = 0;
                    for (int oy = -1; oy <= 1; oy++)
                        for (int ox = -1; ox <= 1; ox++)
                        {
                            int k = kind[Mathf.Clamp(y + oy, 0, h - 1) * w + Mathf.Clamp(x + ox, 0, w - 1)];
                            if (++counts[k] > bestN) { bestN = counts[k]; best = k; }
                        }
                    tidy[y * w + x] = best;
                }
            var outPx = new Color[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x, k = tidy[i];
                    Color c = KindColour(k);
                    int r = tidy[y * w + Mathf.Min(x + 1, w - 1)], d = tidy[Mathf.Max(y - 1, 0) * w + x];
                    int r2 = tidy[y * w + Mathf.Min(x + 2, w - 1)], d2 = tidy[Mathf.Max(y - 2, 0) * w + x];
                    if (r != k || d != k) c = Color.Lerp(c, new Color(0.3f, 0.24f, 0.26f), 0.42f);          // the outline
                    else if (r2 != k || d2 != k) c = Color.Lerp(c, new Color(0.3f, 0.24f, 0.26f), 0.16f);
                    float grain = (Hash(x, y) - 0.5f) * 0.02f;                                             // a breath of felt
                    c.r += grain; c.g += grain; c.b += grain;
                    c.a = 1f;
                    outPx[i] = c;
                }
            var t = new Texture2D(w, h, TextureFormat.RGBA32, true);
            t.wrapMode = TextureWrapMode.Clamp;
            t.filterMode = FilterMode.Trilinear;
            t.SetPixels(outPx);
            t.Apply(true);
            return t;
        }

        static Color[] Blur(Color[] src, int w, int h, int radius)
        {
            var tmp = new Color[src.Length];
            var dst = new Color[src.Length];
            float n = radius * 2 + 1;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    Color sum = Color.clear;
                    for (int o = -radius; o <= radius; o++) sum += src[y * w + Mathf.Clamp(x + o, 0, w - 1)];
                    tmp[y * w + x] = sum / n;
                }
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    Color sum = Color.clear;
                    for (int o = -radius; o <= radius; o++) sum += tmp[Mathf.Clamp(y + o, 0, h - 1) * w + x];
                    dst[y * w + x] = sum / n;
                }
            return dst;
        }

        // Map colours: 0-5 are the land, 6+ are twelve pastel hues (roofs, toys, tents) in a light
        // and a deeper shade each.
        static readonly Color[] Land =
        {
            new Color(0.74f, 0.87f, 0.6f),    // 0 meadow
            new Color(0.62f, 0.8f, 0.54f),    // 1 shady grass
            new Color(0.44f, 0.66f, 0.47f),   // 2 wood
            new Color(0.6f, 0.8f, 0.94f),     // 3 water
            new Color(0.96f, 0.91f, 0.78f),   // 4 paths and paving
            new Color(0.76f, 0.73f, 0.72f),   // 5 stone and shadow
        };

        static Color KindColour(int k)
        {
            if (k < Land.Length) return Land[k];
            int hue = (k - Land.Length) / 2;
            bool deep = (k - Land.Length) % 2 == 1;
            return Color.HSVToRGB(hue / 12f, deep ? 0.5f : 0.36f, deep ? 0.8f : 0.96f);
        }

        static Color MapColour(Color c, out int kind)
        {
            float hh, s, v;
            Color.RGBToHSV(c, out hh, out s, out v);
            float deg = hh * 360f;
            if (s < 0.16f) kind = v > 0.7f ? 4 : 5;
            else if (deg >= 62f && deg < 170f) kind = v > 0.6f ? 0 : v > 0.42f ? 1 : 2;
            else if (deg >= 170f && deg < 255f && s > 0.22f) kind = 3;
            else if (deg >= 20f && deg < 62f && s < 0.42f) kind = v > 0.55f ? 4 : 5;     // sandy paths, wood, cobbles
            else kind = Land.Length + (Mathf.RoundToInt(hh * 12f) % 12) * 2 + (v < 0.62f ? 1 : 0);
            return KindColour(kind);
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

        void MakeIcons()
        {
            if (arrowTex != null) return;
            const int n = 64;
            arrowTex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            starTex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            var a = new Color[n * n];
            var s = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float u = (x + 0.5f) / n * 2f - 1f, v = (y + 0.5f) / n * 2f - 1f;
                    // Arrow pointing up (+v): a chunky triangle with a notch.
                    float inTri = (v > -0.7f && Mathf.Abs(u) < (0.9f - v) * 0.5f && v < 0.9f && !(v < -0.25f && Mathf.Abs(u) < (-0.25f - v) * 0.9f)) ? 1f : 0f;
                    a[y * n + x] = new Color(1f, 1f, 1f, inTri);
                    // Five-pointed star.
                    float ang = Mathf.Atan2(v, u), r = Mathf.Sqrt(u * u + v * v);
                    float k = Mathf.Cos(5f * (ang - Mathf.PI * 0.5f));
                    float edge = Mathf.Lerp(0.45f, 0.95f, k * 0.5f + 0.5f);
                    s[y * n + x] = new Color(1f, 1f, 1f, Mathf.Clamp01((edge - r) * 12f));
                }
            arrowTex.SetPixels(a);
            arrowTex.Apply();
            starTex.SetPixels(s);
            starTex.Apply();
        }

        public void Release()
        {
            if (painting != null) Object.Destroy(painting);
            if (arrowTex != null) Object.Destroy(arrowTex);
            if (starTex != null) Object.Destroy(starTex);
        }

        // ------------------------------------------------------------------ Drawing

        Vector2 ToMap(Rect m, Vector3 world)
        {
            float half = Mathf.Max(area.extents.x, area.extents.z) + 2f;
            float u = (world.x - (area.center.x - half)) / (half * 2f);
            float v = (world.z - (area.center.z - half)) / (half * 2f);
            return new Vector2(m.x + u * m.width, m.yMax - v * m.height);
        }

        // A dashed running stitch along a rectangle's edge (for small things the panel helper
        // is too big for).
        static void RunningStitch(Rect r, Color c, float dash = 7f, float gap = 5f, float thick = 2f)
        {
            for (float x = r.x + 2f; x < r.xMax - dash * 0.5f; x += dash + gap)
            {
                float w = Mathf.Min(dash, r.xMax - x);
                UIKit.Fill(new Rect(x, r.y, w, thick), c, 1f);
                UIKit.Fill(new Rect(x, r.yMax - thick, w, thick), c, 1f);
            }
            for (float y = r.y + 2f; y < r.yMax - dash * 0.5f; y += dash + gap)
            {
                float hh = Mathf.Min(dash, r.yMax - y);
                UIKit.Fill(new Rect(r.x, y, thick, hh), c, 1f);
                UIKit.Fill(new Rect(r.xMax - thick, y, thick, hh), c, 1f);
            }
        }

        // A sewing button: a round disc with a rim and four thread holes.
        static void Button(Vector2 c, float d, Color col, float k)
        {
            UIKit.Circle(c + new Vector2(1.5f, 2.5f), d, new Color(0f, 0f, 0f, 0.28f * k));
            UIKit.Circle(c, d, new Color(col.r, col.g, col.b, k));
            UIKit.CircleOutline(c, d * 0.78f, new Color(col.r * 0.7f, col.g * 0.7f, col.b * 0.7f, k), Mathf.Max(1.5f, d * 0.05f));
            float o = d * 0.12f, hd = d * 0.13f;
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sy = -1; sy <= 1; sy += 2)
                    UIKit.Circle(c + new Vector2(sx * o, sy * o), hd, new Color(0.2f, 0.12f, 0.1f, 0.8f * k));
        }

        static bool Overlaps(Rect r, List<Rect> placed)
        {
            foreach (var p in placed) if (r.Overlaps(p)) return true;
            return false;
        }

        static readonly Color Cream = new Color(0.99f, 0.96f, 0.88f), Ink = new Color(0.3f, 0.18f, 0.12f);
        static readonly Color PlaceRed = new Color(0.86f, 0.24f, 0.24f), ShopBlue = new Color(0.3f, 0.55f, 0.92f);

        // The numbered places, in reading order (top row of the map first, left to right), so
        // the numbers are easy to find on the map and the key reads 1, 2, 3...
        List<MapPin> Numbered(LevelInfo level)
        {
            var list = new List<MapPin>();
            foreach (var pin in level.MapPins)
                if (pin.Type == MapPin.Kind.Place || pin.Type == MapPin.Kind.Shop) list.Add(pin);
            float half = Mathf.Max(area.extents.x, area.extents.z) + 2f, band = half * 2f / 6f;
            list.Sort((x, y) =>
            {
                int rx = Mathf.FloorToInt((area.center.z + half - x.Position.z) / band), ry = Mathf.FloorToInt((area.center.z + half - y.Position.z) / band);
                return rx != ry ? rx.CompareTo(ry) : x.Position.x.CompareTo(y.Position.x);
            });
            return list;
        }

        // A turn about a point, in the menu's own coordinates (Unity's own helper turns about a
        // screen pixel, which lands in the wrong place once the menu is scaled to fit the window).
        static Matrix4x4 Turn(float degrees, Vector2 pivot)
        {
            var p3 = new Vector3(pivot.x, pivot.y, 0f);
            return Matrix4x4.Translate(p3) * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, degrees)) * Matrix4x4.Translate(-p3);
        }

        // A felt tag with a stitched edge: every name on the map sits on one of these.
        static void Tag(Rect r, Color stitch, float k)
        {
            UIKit.Fill(new Rect(r.x + 2f, r.y + 3f, r.width, r.height), new Color(0f, 0f, 0f, 0.22f * k), 7f);
            UIKit.Fill(r, new Color(Cream.r, Cream.g, Cream.b, 0.96f * k), 7f);
            RunningStitch(new Rect(r.x + 3f, r.y + 3f, r.width - 6f, r.height - 6f), new Color(stitch.r, stitch.g, stitch.b, 0.7f * k), 5f, 3f, 1.4f);
        }

        // A round numbered badge.
        static void Badge(Vector2 c, float d, Color col, string text, float k)
        {
            UIKit.Circle(c + new Vector2(1.5f, 2f), d, new Color(0f, 0f, 0f, 0.3f * k));
            UIKit.Circle(c, d, new Color(1f, 1f, 1f, k));
            UIKit.Circle(c, d - 5f, new Color(col.r, col.g, col.b, k));
            UIKit.Label(new Rect(c.x - d, c.y - d * 0.5f, d * 2f, d), text, UIKit.Style(Mathf.RoundToInt(d * 0.52f), true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, k)));
        }

        // On the map: region names on felt tags, then the Town Band buttons, then a numbered badge
        // for every place (nudged apart if two would sit on top of each other, with a little
        // thread back to the real spot). The names of the numbered places are in the key.
        void DrawPins(Rect mapR, LevelInfo level, float k)
        {
            var placed = new List<Rect>();
            var areaStyle = UIKit.Style(17, true, TextAnchor.MiddleCenter, new Color(Ink.r, Ink.g, Ink.b, k));
            var areaStitch = new Color(0.45f, 0.62f, 0.4f);
            foreach (var pin in level.MapPins)
            {
                if (pin.Type != MapPin.Kind.Area) continue;
                Vector2 p = ToMap(mapR, pin.Position);
                string text = pin.Name.ToUpperInvariant();
                float w = UIKit.TextWidth(text, areaStyle) + 26f, h = 30f;
                bool tall = pin.Name.Contains("Ravine") || pin.Name.Contains("Canal");
                if (tall)
                {
                    var box = new Rect(p.x - h * 0.5f, p.y - w * 0.5f, h, w);
                    // Keep the whole tag on the map (a long one near the edge used to hang off it).
                    box.x = Mathf.Clamp(box.x, mapR.x + 6f, mapR.xMax - box.width - 6f);
                    box.y = Mathf.Clamp(box.y, mapR.y + 6f, mapR.yMax - box.height - 6f);
                    placed.Add(box);
                    Matrix4x4 saved = GUI.matrix;
                    GUI.matrix = saved * Turn(-90f, box.center);
                    var lr = new Rect(box.center.x - w * 0.5f, box.center.y - h * 0.5f, w, h);
                    Tag(lr, areaStitch, k);
                    UIKit.Label(lr, text, areaStyle);
                    GUI.matrix = saved;
                    continue;
                }
                // Try the spot itself, then a little above or below it.
                float[] dy = { 0f, -34f, 34f, -68f, 68f };
                foreach (float o in dy)
                {
                    var r = new Rect(p.x - w * 0.5f, p.y - h * 0.5f + o, w, h);
                    r.x = Mathf.Clamp(r.x, mapR.x + 6f, mapR.xMax - r.width - 6f);
                    r.y = Mathf.Clamp(r.y, mapR.y + 6f, mapR.yMax - r.height - 6f);
                    if (Overlaps(r, placed) && o != dy[dy.Length - 1]) continue;
                    placed.Add(r);
                    Tag(r, areaStitch, k);
                    UIKit.Label(r, text, areaStyle);
                    break;
                }
            }

            // The Town Band: gold star buttons once found, grey until then.
            foreach (var pin in level.MapPins)
            {
                if (pin.Type != MapPin.Kind.Band) continue;
                Vector2 p = ToMap(mapR, pin.Position);
                bool got = !string.IsNullOrEmpty(pin.Id) && GameState.IsCollected(pin.Id);
                Button(p, 34f, got ? new Color(1f, 0.8f, 0.25f) : new Color(0.62f, 0.58f, 0.7f), k);
                UIKit.IconTransformed(new Rect(p.x - 11f, p.y - 11f, 22f, 22f), starTex, new Color(1f, 1f, 1f, 0.9f * k), 0f, 1f);
                placed.Add(new Rect(p.x - 17f, p.y - 17f, 34f, 34f));
            }

            // Numbered places.
            var list = Numbered(level);
            const float d = 28f;
            for (int i = 0; i < list.Count; i++)
            {
                Vector2 p = ToMap(mapR, list[i].Position);
                Vector2 at = p;
                for (int ring = 0; ring < 5; ring++)
                {
                    bool found = false;
                    int steps = ring == 0 ? 1 : 8;
                    for (int s = 0; s < steps && !found; s++)
                    {
                        float a = s * Mathf.PI * 0.25f;
                        Vector2 c = p + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * ring * 16f;
                        var r = new Rect(c.x - d * 0.5f, c.y - d * 0.5f, d, d);
                        if (r.x < mapR.x + 2f || r.xMax > mapR.xMax - 2f || r.y < mapR.y + 2f || r.yMax > mapR.yMax - 2f) continue;
                        if (Overlaps(r, placed)) continue;
                        at = c;
                        found = true;
                    }
                    if (found) break;
                }
                if ((at - p).sqrMagnitude > 4f)
                {
                    // A little thread from the badge to the real spot, and a dot there.
                    Vector2 dlt = at - p;
                    Matrix4x4 saved = GUI.matrix;
                    GUI.matrix = saved * Turn(Mathf.Atan2(dlt.y, dlt.x) * Mathf.Rad2Deg, p);
                    UIKit.Fill(new Rect(p.x, p.y - 1f, dlt.magnitude, 2f), new Color(Ink.r, Ink.g, Ink.b, 0.7f * k));
                    GUI.matrix = saved;
                    UIKit.Circle(p, 7f, new Color(Ink.r, Ink.g, Ink.b, k));
                }
                placed.Add(new Rect(at.x - d * 0.5f, at.y - d * 0.5f, d, d));
                Badge(at, d, list[i].Type == MapPin.Kind.Shop ? ShopBlue : PlaceRed, (i + 1).ToString(), k);
            }

            // North, in the corner.
            var nc = new Vector2(mapR.xMax - 30f, mapR.y + 34f);
            UIKit.Circle(nc + new Vector2(1.5f, 2f), 40f, new Color(0f, 0f, 0f, 0.25f * k));
            UIKit.Circle(nc, 40f, new Color(Cream.r, Cream.g, Cream.b, k));
            UIKit.Label(new Rect(nc.x - 20f, nc.y - 20f, 40f, 40f), "N", UIKit.Style(22, true, TextAnchor.MiddleCenter, new Color(0.85f, 0.3f, 0.3f, k)));
        }

        public void Draw(bool kb)
        {
            MakeIcons();
            float W = UIKit.Width, H = UIKit.Height;
            float k = Mathf.Clamp01(openTime / 0.2f);
            float pop = UIKit.EaseOutBack(Mathf.Clamp01(openTime / 0.35f));
            UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.12f, 0.08f, 0.14f, 0.55f * k));
            var level = game.Level;
            var thread = new Color(0.99f, 0.95f, 0.85f, 0.9f * k);

            // The quilt: a backing of patchwork squares, each stitched round, with a binding.
            float pw = 1560f, ph = 960f;
            var quilt = new Rect((W - pw) * 0.5f, (H - ph) * 0.5f + (1f - pop) * 60f, pw, ph);
            UIKit.Fill(new Rect(quilt.x + 8f, quilt.y + 12f, pw, ph), new Color(0f, 0f, 0f, 0.3f * k), 26f);
            UIKit.Fill(quilt, new Color(0.62f, 0.32f, 0.36f, k), 26f);
            // One calm piece of fabric inside the binding (a busy patchwork fought with the map).
            var cloth = new Rect(quilt.x + 18f, quilt.y + 18f, pw - 36f, ph - 36f);
            var clothCol = new Color(0.93f, 0.86f, 0.74f);
            UIKit.Fill(cloth, new Color(clothCol.r, clothCol.g, clothCol.b, k), 14f);
            if (UIArt.PatternKnit != null) UIKit.PatternFill(cloth, UIArt.PatternKnit, new Color(clothCol.r * 0.82f, clothCol.g * 0.82f, clothCol.b * 0.82f, 0.25f * k), 40f, Vector2.zero);
            RunningStitch(new Rect(cloth.x + 8f, cloth.y + 8f, cloth.width - 16f, cloth.height - 16f), new Color(1f, 1f, 1f, 0.6f * k), 10f, 7f, 2.5f);

            // The map: a linen panel appliquéd onto the quilt, the felt picture inside it.
            float ms = ph - 190f;
            var mapR = new Rect(quilt.x + 70f, quilt.y + 100f, ms, ms);
            var linen = new Rect(mapR.x - 22f, mapR.y - 22f, mapR.width + 44f, mapR.height + 44f);
            UIKit.Fill(new Rect(linen.x + 5f, linen.y + 7f, linen.width, linen.height), new Color(0f, 0f, 0f, 0.25f * k), 16f);
            UIKit.Fill(linen, new Color(0.97f, 0.93f, 0.84f, k), 16f);
            RunningStitch(new Rect(linen.x + 8f, linen.y + 8f, linen.width - 16f, linen.height - 16f), new Color(0.85f, 0.35f, 0.4f, 0.9f * k), 10f, 6f, 2.5f);
            if (painting != null) GUI.DrawTexture(mapR, painting, ScaleMode.StretchToFill, false, 0f, new Color(1f, 1f, 1f, k), 0f, 8f);
            else UIKit.Label(mapR, "Stitching the map...", UIKit.Style(30, true, TextAnchor.MiddleCenter, new Color(0.36f, 0.22f, 0.16f, 0.6f * k)));
            UIKit.Outline(mapR, new Color(0.45f, 0.3f, 0.25f, 0.8f * k), 3f, 8f);

            // The title on a felt ribbon with notched ends.
            var titleStyle = UIKit.Style(50, true, TextAnchor.MiddleCenter, new Color(1f, 0.97f, 0.9f, k));
            float tw = UIKit.TextWidth(level.MapTitle, titleStyle) + 90f;
            var ribbon = new Rect(mapR.x + (mapR.width - tw) * 0.5f, quilt.y + 22f, tw, 64f);
            for (int s = -1; s <= 1; s += 2)
            {
                var tail = new Rect(s < 0 ? ribbon.x - 34f : ribbon.xMax - 16f, ribbon.y + 12f, 50f, 50f);
                UIKit.Fill(tail, new Color(0.62f, 0.16f, 0.2f, k), 6f);
                Vector2 notch = new Vector2(s < 0 ? tail.x : tail.xMax, tail.center.y);
                Matrix4x4 saved = GUI.matrix;
                GUI.matrix = saved * Turn(45f, notch);
                UIKit.Fill(new Rect(notch.x - 18f, notch.y - 18f, 36f, 36f), new Color(0.62f, 0.32f, 0.36f, k));
                GUI.matrix = saved;
            }
            UIKit.Fill(new Rect(ribbon.x + 3f, ribbon.y + 5f, ribbon.width, ribbon.height), new Color(0f, 0f, 0f, 0.25f * k), 10f);
            UIKit.Fill(ribbon, new Color(0.86f, 0.26f, 0.3f, k), 10f);
            RunningStitch(new Rect(ribbon.x + 8f, ribbon.y + 7f, ribbon.width - 16f, ribbon.height - 14f), thread);
            UIKit.Label(ribbon, level.MapTitle, titleStyle, true);

            DrawPins(mapR, level, k);

            // Patch: a little ball of pink yarn with an arrow showing which way he's facing.
            if (game.Player != null)
            {
                Vector2 p = ToMap(mapR, game.Player.transform.position);
                float yaw = game.Player.transform.eulerAngles.y;
                float pulse = 1f + Mathf.Sin(Time.unscaledTime * 6f) * 0.12f;
                UIKit.Circle(p, 58f * pulse, new Color(1f, 1f, 1f, 0.3f * k));
                UIKit.IconTransformed(new Rect(p.x - 26f, p.y - 26f, 52f, 52f), arrowTex, new Color(0.15f, 0.1f, 0.2f, k), yaw, 1.2f);
                UIKit.Circle(p, 30f, new Color(1f, 0.55f, 0.68f, k));
                for (int i = 0; i < 3; i++)
                {
                    Matrix4x4 saved = GUI.matrix;
                    GUI.matrix = saved * Turn(i * 60f + 20f, p);
                    UIKit.Fill(new Rect(p.x - 13f, p.y - 1.5f + (i - 1) * 6f, 26f, 2.5f), new Color(0.85f, 0.35f, 0.5f, k), 1.2f);
                    GUI.matrix = saved;
                }
                var you = new Rect(p.x - 26f, p.y + 22f, 52f, 26f);
                Tag(you, new Color(0.85f, 0.35f, 0.5f), k);
                UIKit.Label(you, "You", UIKit.Style(16, true, TextAnchor.MiddleCenter, new Color(Ink.r, Ink.g, Ink.b, k)));
            }

            // The key, on one felt panel stitched to the quilt: every numbered place, then the Town
            // Band and what's left to find. Every row the same height, on the same cream strip.
            float lx = linen.xMax + 34f, lw = quilt.xMax - lx - 34f;
            var pocket = new Rect(lx, linen.y, lw, linen.height);
            UIKit.Panel(pocket, new Color(0.99f, 0.95f, 0.88f, 0.97f * k), new Color(0.85f, 0.35f, 0.4f, 0.8f * k), 20f);
            float px = pocket.x + 22f, pwid = pocket.width - 44f, ly = pocket.y + 20f;
            var head = UIKit.Style(26, true, TextAnchor.MiddleLeft, new Color(0.85f, 0.3f, 0.3f, k));
            var row = UIKit.Style(17, true, TextAnchor.MiddleLeft, new Color(Ink.r, Ink.g, Ink.b, k));
            var body = UIKit.Style(19, true, TextAnchor.MiddleLeft, new Color(Ink.r, Ink.g, Ink.b, k));

            var places = Numbered(level);
            if (places.Count > 0)
            {
                UIKit.Label(new Rect(px, ly, pwid, 34f), "Places", head);
                ly += 38f;
                int perCol = (places.Count + 1) / 2;
                float colW = (pwid - 12f) * 0.5f, rowH = Mathf.Min(30f, (pocket.height * 0.55f - 40f) / Mathf.Max(1, perCol));
                for (int i = 0; i < places.Count; i++)
                {
                    int col = i / perCol, r = i % perCol;
                    var strip = new Rect(px + col * (colW + 12f), ly + r * rowH, colW, rowH - 4f);
                    UIKit.Fill(strip, new Color(1f, 0.99f, 0.95f, k), 6f);
                    float bd = Mathf.Min(24f, rowH - 4f);
                    Badge(new Vector2(strip.x + bd * 0.5f + 3f, strip.center.y), bd, places[i].Type == MapPin.Kind.Shop ? ShopBlue : PlaceRed, (i + 1).ToString(), k);
                    UIKit.Label(new Rect(strip.x + bd + 10f, strip.y, strip.width - bd - 12f, strip.height), places[i].Name, row);
                }
                ly += perCol * rowH + 14f;
            }

            int bands = 0;
            foreach (var pin in level.MapPins) if (pin.Type == MapPin.Kind.Band) bands++;
            if (bands > 0)
            {
                UIKit.Fill(new Rect(px, ly, pwid, 2f), new Color(0.85f, 0.35f, 0.4f, 0.4f * k));
                ly += 10f;
                UIKit.Label(new Rect(px, ly, pwid, 34f), "Town Band", head);
                ly += 38f;
                foreach (var pin in level.MapPins)
                {
                    if (pin.Type != MapPin.Kind.Band) continue;
                    bool got = GameState.IsCollected(pin.Id);
                    var strip = new Rect(px, ly, pwid, 30f);
                    UIKit.Fill(strip, new Color(1f, 0.99f, 0.95f, k), 6f);
                    Button(new Vector2(strip.x + 15f, strip.center.y), 24f, got ? new Color(1f, 0.8f, 0.25f) : new Color(0.62f, 0.58f, 0.7f), k);
                    UIKit.Label(new Rect(strip.x + 34f, strip.y, strip.width - 36f, strip.height), got ? pin.Name + "  - found!" : "??? (not found yet)", row);
                    ly += 34f;
                }
                ly += 8f;
            }

            UIKit.Fill(new Rect(px, ly, pwid, 2f), new Color(0.85f, 0.35f, 0.4f, 0.4f * k));
            ly += 12f;
            if (level.HeartstringIds.Count > 0)
            {
                UIKit.Heart(new Vector2(px + 14f, ly + 16f), 26f, new Color(1f, 0.72f, 0.3f, k));
                UIKit.Label(new Rect(px + 36f, ly, pwid - 36f, 32f), "Heartstrings  " + level.HeartstringCount() + " / " + level.HeartstringIds.Count, body);
                ly += 36f;
            }
            if (level.CollectibleIds.Count > 0)
            {
                UIKit.Icon(new Rect(px + 1f, ly + 3f, 26f, 26f), PatchworkAssets.IconButton, new Color(1f, 1f, 1f, k));
                UIKit.Label(new Rect(px + 36f, ly, pwid - 36f, 32f), "Golden Buttons  " + level.CollectedCount() + " / " + level.CollectibleIds.Count, body);
                ly += 36f;
            }
            if (level.SecretIds.Count > 0)
            {
                UIKit.Circle(new Vector2(px + 14f, ly + 16f), 24f, new Color(0.55f, 0.75f, 1f, k));
                UIKit.Circle(new Vector2(px + 10f, ly + 12f), 8f, new Color(1f, 1f, 1f, 0.8f * k));
                UIKit.Label(new Rect(px + 36f, ly, pwid - 36f, 32f), "Secret Marbles  " + level.SecretCount() + " / " + level.SecretIds.Count, body);
                ly += 36f;
            }

            UIKit.PromptLabel(quilt.xMax - 250f, quilt.yMax - 50f, 36f, PromptAction.Map, "Close", kb, Ink, k);
            if (CanTravel())
                UIKit.PromptLabel(linen.x + 10f, quilt.yMax - 50f, 36f, PromptAction.Confirm, "Travel back to the Pod", kb, Ink, k);
        }
    }
}

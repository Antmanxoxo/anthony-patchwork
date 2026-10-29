using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Patchwork
{
    // A landmark on a level's map.
    public class MapPin
    {
        public enum Kind { Place, Band, Shop }
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

    // The pull-up map (Map button: D-pad left / M). A hand-drawn tourist map on squared paper,
    // in the spirit of an island brochure: a painted bird's-eye view of the level, red pins with
    // place names, Patch's arrow, little doodles round the edge and a legend of what's left to find.
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
            capCam.cullingMask = ~(1 << 2);
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

        // Turns the snapshot into a soft watercolour on cream paper with brown ink outlines.
        static Texture2D Stylize(Texture2D src)
        {
            int w = src.width, h = src.height;
            var px = src.GetPixels();
            var lum = new float[w * h];
            for (int i = 0; i < px.Length; i++) lum[i] = px[i].r * 0.3f + px[i].g * 0.59f + px[i].b * 0.11f;
            var paper = new Color(0.96f, 0.91f, 0.78f);
            var ink = new Color(0.36f, 0.22f, 0.16f);
            var outPx = new Color[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    Color c = px[i];
                    // Brighter, a touch more saturated, then washed toward the paper colour.
                    float l = lum[i];
                    c = Color.LerpUnclamped(new Color(l, l, l), c, 1.25f);
                    c = Color.Lerp(c, paper * (0.6f + l * 0.6f), 0.28f);
                    // Gentle posterise, like flat watercolour washes.
                    for (int k = 0; k < 3; k++)
                    {
                        float v = c[k];
                        c[k] = Mathf.Lerp(v, Mathf.Round(v * 7f) / 7f, 0.45f);
                    }
                    // Ink lines where the picture changes sharply (Sobel on brightness).
                    int xl = Mathf.Max(0, x - 1), xr = Mathf.Min(w - 1, x + 1), yd = Mathf.Max(0, y - 1), yu = Mathf.Min(h - 1, y + 1);
                    float gx = (lum[yd * w + xr] + 2f * lum[y * w + xr] + lum[yu * w + xr]) - (lum[yd * w + xl] + 2f * lum[y * w + xl] + lum[yu * w + xl]);
                    float gy = (lum[yu * w + xl] + 2f * lum[yu * w + x] + lum[yu * w + xr]) - (lum[yd * w + xl] + 2f * lum[yd * w + x] + lum[yd * w + xr]);
                    float edge = Mathf.Clamp01((Mathf.Sqrt(gx * gx + gy * gy) - 0.18f) * 2.2f);
                    c = Color.Lerp(c, ink, edge * 0.75f);
                    // Paper grain.
                    float grain = (Hash(x, y) - 0.5f) * 0.05f;
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

        public void Draw(bool kb)
        {
            MakeIcons();
            float W = UIKit.Width, H = UIKit.Height;
            float k = Mathf.Clamp01(openTime / 0.2f);
            float pop = UIKit.EaseOutBack(Mathf.Clamp01(openTime / 0.35f));
            UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.12f, 0.08f, 0.14f, 0.55f * k));
            var level = game.Level;
            var inkC = new Color(0.36f, 0.22f, 0.16f, k);
            var red = new Color(0.86f, 0.22f, 0.2f, k);

            // The paper: cream with a faint grid, a torn-ish dark edge and a drop shadow.
            float pw = 1560f, ph = 960f;
            var paperR = new Rect((W - pw) * 0.5f, (H - ph) * 0.5f + (1f - pop) * 60f, pw, ph);
            UIKit.Fill(new Rect(paperR.x + 8f, paperR.y + 12f, pw, ph), new Color(0f, 0f, 0f, 0.3f * k), 18f);
            UIKit.Fill(paperR, new Color(0.97f, 0.93f, 0.82f, k), 18f);
            for (float gx = paperR.x + 40f; gx < paperR.xMax - 10f; gx += 40f)
                UIKit.Fill(new Rect(gx, paperR.y + 10f, 1.5f, ph - 20f), new Color(0.6f, 0.68f, 0.72f, 0.25f * k));
            for (float gy = paperR.y + 40f; gy < paperR.yMax - 10f; gy += 40f)
                UIKit.Fill(new Rect(paperR.x + 10f, gy, pw - 20f, 1.5f), new Color(0.6f, 0.68f, 0.72f, 0.25f * k));
            UIKit.Outline(paperR, new Color(0.6f, 0.45f, 0.3f, 0.8f * k), 4f, 18f);

            // The painting, framed with a wobbly double ink line.
            float ms = ph - 120f;
            var mapR = new Rect(paperR.x + 60f, paperR.y + 80f, ms, ms);
            UIKit.Fill(new Rect(mapR.x - 10f, mapR.y - 10f, mapR.width + 20f, mapR.height + 20f), new Color(0.9f, 0.84f, 0.68f, k), 14f);
            if (painting != null) GUI.DrawTexture(mapR, painting, ScaleMode.StretchToFill, false, 0f, new Color(1f, 1f, 1f, k), 0f, 10f);
            else UIKit.Label(mapR, "Painting the map...", UIKit.Style(30, true, TextAnchor.MiddleCenter, new Color(0.36f, 0.22f, 0.16f, 0.6f * k)));
            UIKit.Outline(new Rect(mapR.x - 6f, mapR.y - 6f, mapR.width + 12f, mapR.height + 12f), inkC, 3f, 12f);
            UIKit.Outline(new Rect(mapR.x - 13f, mapR.y - 13f, mapR.width + 26f, mapR.height + 26f), new Color(inkC.r, inkC.g, inkC.b, 0.5f * k), 2f, 16f);

            // Title banner, top left, with a doodled sun.
            var titleStyle = UIKit.Style(54, true, TextAnchor.MiddleLeft, new Color(0.85f, 0.3f, 0.3f, k));
            UIKit.Label(new Rect(mapR.x + 10f, paperR.y + 12f, 700f, 66f), level.MapTitle, titleStyle);
            Vector2 sun = new Vector2(paperR.xMax - 90f, paperR.y + 80f);
            for (int i = 0; i < 12; i++)
            {
                float a = i / 12f * 360f + Time.unscaledTime * 10f;
                Matrix4x4 saved = GUI.matrix;
                GUIUtility.RotateAroundPivot(a, sun);
                UIKit.Fill(new Rect(sun.x - 4f, sun.y - 58f, 8f, 22f), new Color(1f, 0.6f, 0.2f, k), 4f);
                GUI.matrix = saved;
            }
            UIKit.Circle(sun, 64f, new Color(1f, 0.78f, 0.25f, k));
            UIKit.Circle(sun + new Vector2(-10f, -6f), 8f, new Color(0.36f, 0.22f, 0.16f, k));
            UIKit.Circle(sun + new Vector2(10f, -6f), 8f, new Color(0.36f, 0.22f, 0.16f, k));
            UIKit.Fill(new Rect(sun.x - 10f, sun.y + 8f, 20f, 5f), new Color(0.36f, 0.22f, 0.16f, k), 2.5f);

            // Lollipop-tree doodles in the bottom corner.
            for (int i = 0; i < 3; i++)
            {
                Vector2 b = new Vector2(paperR.xMax - 300f + i * 60f, paperR.yMax - 40f);
                UIKit.Fill(new Rect(b.x - 3f, b.y - 50f, 6f, 50f), new Color(0.55f, 0.38f, 0.26f, k));
                UIKit.Circle(b + new Vector2(0f, -62f), 46f, i == 1 ? new Color(0.98f, 0.65f, 0.72f, k) : new Color(0.5f, 0.75f, 0.45f, k));
            }

            // Pins: red dots with names; band pieces as stars (gold if found).
            var labelStyle = UIKit.Style(22, true, TextAnchor.MiddleLeft, new Color(0.3f, 0.18f, 0.12f, k));
            foreach (var pin in level.MapPins)
            {
                Vector2 p = ToMap(mapR, pin.Position);
                if (pin.Type == MapPin.Kind.Band)
                {
                    bool got = !string.IsNullOrEmpty(pin.Id) && GameState.IsCollected(pin.Id);
                    float bob = Mathf.Sin(Time.unscaledTime * 3f + p.x) * 3f;
                    var r = new Rect(p.x - 22f, p.y - 22f + bob, 44f, 44f);
                    UIKit.IconTransformed(new Rect(r.x + 2f, r.y + 3f, r.width, r.height), starTex, new Color(0f, 0f, 0f, 0.35f * k), 0f, 1f);
                    UIKit.IconTransformed(r, starTex, got ? new Color(1f, 0.8f, 0.25f, k) : new Color(0.62f, 0.58f, 0.7f, k), 0f, 1f);
                    UIKit.Label(new Rect(p.x + 24f, p.y - 16f + bob, 240f, 32f), got ? pin.Name : "???", UIKit.Style(20, true, TextAnchor.MiddleLeft, new Color(1f, 1f, 1f, k)), true);
                    continue;
                }
                UIKit.Circle(p + new Vector2(1f, 2f), 20f, new Color(0f, 0f, 0f, 0.3f * k));
                UIKit.Circle(p, 20f, pin.Type == MapPin.Kind.Shop ? new Color(0.25f, 0.5f, 0.85f, k) : red);
                UIKit.CircleOutline(p, 20f, new Color(1f, 1f, 1f, 0.9f * k), 3f);
                float tw = UIKit.TextWidth(pin.Name, labelStyle) + 18f;
                var lr = new Rect(p.x + 14f, p.y - 15f, tw, 30f);
                UIKit.Fill(lr, new Color(0.99f, 0.96f, 0.88f, 0.88f * k), 8f);
                UIKit.Label(new Rect(lr.x + 9f, lr.y, tw, 30f), pin.Name, labelStyle);
            }

            // Patch: a bobbing arrow pointing the way he's facing.
            if (game.Player != null)
            {
                Vector2 p = ToMap(mapR, game.Player.transform.position);
                float yaw = game.Player.transform.eulerAngles.y;
                float pulse = 1f + Mathf.Sin(Time.unscaledTime * 6f) * 0.12f;
                UIKit.Circle(p, 54f * pulse, new Color(1f, 1f, 1f, 0.35f * k));
                UIKit.IconTransformed(new Rect(p.x - 22f, p.y - 22f, 44f, 44f), arrowTex, new Color(0.15f, 0.1f, 0.2f, k), yaw, 1.15f);
                UIKit.IconTransformed(new Rect(p.x - 22f, p.y - 22f, 44f, 44f), arrowTex, new Color(1f, 0.55f, 0.65f, k), yaw, 0.9f);
                UIKit.Label(new Rect(p.x - 40f, p.y + 22f, 80f, 26f), "You", UIKit.Style(18, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, k)), true);
            }

            // The legend: what's left to find here.
            float lx = mapR.xMax + 50f, ly = mapR.y + 10f, lw = paperR.xMax - lx - 40f;
            var head = UIKit.Style(30, true, TextAnchor.MiddleLeft, new Color(0.85f, 0.3f, 0.3f, k));
            var body = UIKit.Style(24, true, TextAnchor.MiddleLeft, new Color(0.3f, 0.18f, 0.12f, k));
            int bands = 0;
            foreach (var pin in level.MapPins) if (pin.Type == MapPin.Kind.Band) bands++;
            if (bands > 0)
            {
                UIKit.Label(new Rect(lx, ly, lw, 40f), "Town Band", head);
                ly += 48f;
                foreach (var pin in level.MapPins)
                {
                    if (pin.Type != MapPin.Kind.Band) continue;
                    bool got = GameState.IsCollected(pin.Id);
                    UIKit.IconTransformed(new Rect(lx, ly + 2f, 30f, 30f), starTex, got ? new Color(1f, 0.8f, 0.25f, k) : new Color(0.62f, 0.58f, 0.7f, k), 0f, 1f);
                    UIKit.Label(new Rect(lx + 40f, ly, lw - 40f, 34f), got ? pin.Name + "  - found!" : pin.Name, body);
                    ly += 40f;
                }
                ly += 20f;
            }
            if (level.HeartstringIds.Count > 0)
            {
                UIKit.Heart(new Vector2(lx + 15f, ly + 18f), 30f, new Color(1f, 0.72f, 0.3f, k));
                UIKit.Label(new Rect(lx + 40f, ly, lw - 40f, 36f), "Heartstrings  " + level.HeartstringCount() + " / " + level.HeartstringIds.Count, body);
                ly += 44f;
            }
            if (level.CollectibleIds.Count > 0)
            {
                UIKit.Icon(new Rect(lx, ly + 2f, 30f, 30f), PatchworkAssets.IconButton, new Color(1f, 1f, 1f, k));
                UIKit.Label(new Rect(lx + 40f, ly, lw - 40f, 36f), "Golden Buttons  " + level.CollectedCount() + " / " + level.CollectibleIds.Count, body);
                ly += 44f;
            }
            ly += 16f;
            var note = UIKit.Style(20, false, TextAnchor.UpperLeft, new Color(0.3f, 0.18f, 0.12f, 0.85f * k), true);
            UIKit.Label(new Rect(lx, ly, lw, 160f), level.MapNote, note);
            // A little compass rose.
            Vector2 cp = new Vector2(lx + 70f, paperR.yMax - 150f);
            UIKit.CircleOutline(cp, 110f, inkC, 3f);
            UIKit.Fill(new Rect(cp.x - 2f, cp.y - 50f, 4f, 100f), inkC);
            UIKit.Fill(new Rect(cp.x - 50f, cp.y - 2f, 100f, 4f), inkC);
            UIKit.IconTransformed(new Rect(cp.x - 14f, cp.y - 52f, 28f, 28f), arrowTex, red, 0f, 1f);
            UIKit.Label(new Rect(cp.x - 20f, cp.y - 92f, 40f, 34f), "N", UIKit.Style(28, true, TextAnchor.MiddleCenter, new Color(0.85f, 0.3f, 0.3f, k)));

            UIKit.PromptLabel(paperR.xMax - 260f, paperR.yMax - 62f, 40f, PromptAction.Map, "Close", kb, new Color(0.3f, 0.18f, 0.12f), k);
        }
    }
}

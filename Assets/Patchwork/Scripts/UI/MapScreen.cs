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

        // Turns the snapshot into felt appliqué: flat, bright patches of fabric with darker
        // stitched seams where one thing meets another, a cream running stitch along the seams,
        // and a woven texture over everything.
        static Texture2D Stylize(Texture2D src)
        {
            int w = src.width, h = src.height;
            var px = src.GetPixels();
            var lum = new float[w * h];
            for (int i = 0; i < px.Length; i++) lum[i] = px[i].r * 0.3f + px[i].g * 0.59f + px[i].b * 0.11f;
            var cream = new Color(0.98f, 0.94f, 0.84f);
            var outPx = new Color[w * h];
            System.Func<int, int, float> edgeAt = (x, y) =>
            {
                x = Mathf.Clamp(x, 1, w - 2);
                y = Mathf.Clamp(y, 1, h - 2);
                float gx = (lum[(y - 1) * w + x + 1] + 2f * lum[y * w + x + 1] + lum[(y + 1) * w + x + 1]) - (lum[(y - 1) * w + x - 1] + 2f * lum[y * w + x - 1] + lum[(y + 1) * w + x - 1]);
                float gy = (lum[(y + 1) * w + x - 1] + 2f * lum[(y + 1) * w + x] + lum[(y + 1) * w + x + 1]) - (lum[(y - 1) * w + x - 1] + 2f * lum[(y - 1) * w + x] + lum[(y - 1) * w + x + 1]);
                return Mathf.Clamp01((Mathf.Sqrt(gx * gx + gy * gy) - 0.16f) * 2.4f);
            };
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    Color c = px[i];
                    float l = lum[i];
                    // Richer, brighter colour, then flattened into a few fabric tones.
                    c = Color.LerpUnclamped(new Color(l, l, l), c, 1.4f);
                    c = Color.Lerp(c, cream * (0.55f + l * 0.6f), 0.18f);
                    for (int k = 0; k < 3; k++) c[k] = Mathf.Lerp(c[k], Mathf.Round(c[k] * 6f) / 6f, 0.75f);
                    // Seams: a dark fold where things meet, a light edge just beside it (like the
                    // raised edge of a felt patch), and a dashed cream running stitch along it.
                    float seam = edgeAt(x, y);
                    float lift = edgeAt(x - 2, y + 2) - seam;
                    c = Color.Lerp(c, c * 0.5f, seam * 0.75f);
                    if (lift > 0.2f) c = Color.Lerp(c, Color.Lerp(c, Color.white, 0.35f), lift);
                    if (seam > 0.55f && ((x + y) / 4) % 3 == 0) c = Color.Lerp(c, cream, 0.7f);
                    // Woven fabric texture and a little fuzz.
                    float weave = Mathf.Sin(x * 1.5708f) * Mathf.Sin(y * 1.5708f) * 0.025f + (Hash(x, y) - 0.5f) * 0.06f;
                    c.r += weave; c.g += weave; c.b += weave;
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

        public void Draw(bool kb)
        {
            MakeIcons();
            float W = UIKit.Width, H = UIKit.Height;
            float k = Mathf.Clamp01(openTime / 0.2f);
            float pop = UIKit.EaseOutBack(Mathf.Clamp01(openTime / 0.35f));
            UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.12f, 0.08f, 0.14f, 0.55f * k));
            var level = game.Level;
            var thread = new Color(0.99f, 0.95f, 0.85f, 0.9f * k);
            var brown = new Color(0.36f, 0.22f, 0.16f, k);
            var red = new Color(0.86f, 0.22f, 0.2f, k);

            // The quilt: a backing of patchwork squares, each stitched round, with a binding.
            float pw = 1560f, ph = 960f;
            var quilt = new Rect((W - pw) * 0.5f, (H - ph) * 0.5f + (1f - pop) * 60f, pw, ph);
            UIKit.Fill(new Rect(quilt.x + 8f, quilt.y + 12f, pw, ph), new Color(0f, 0f, 0f, 0.3f * k), 26f);
            UIKit.Fill(quilt, new Color(0.62f, 0.32f, 0.36f, k), 26f);
            Color[] squares = { new Color(0.96f, 0.86f, 0.74f), new Color(0.78f, 0.86f, 0.92f), new Color(0.98f, 0.8f, 0.82f), new Color(0.84f, 0.9f, 0.76f), new Color(0.98f, 0.9f, 0.68f), new Color(0.86f, 0.8f, 0.94f) };
            Texture[] prints = { UIArt.PatternDots, UIArt.PatternKnit, UIArt.PatternSwirl, UIArt.PatternWaves };
            int cols = 8, rows = 5;
            float sw = (pw - 36f) / cols, sh = (ph - 36f) / rows;
            for (int cx = 0; cx < cols; cx++)
                for (int cy = 0; cy < rows; cy++)
                {
                    var sq = new Rect(quilt.x + 18f + cx * sw + 2f, quilt.y + 18f + cy * sh + 2f, sw - 4f, sh - 4f);
                    Color sc = squares[(cx + cy * 3) % squares.Length];
                    UIKit.Fill(sq, new Color(sc.r, sc.g, sc.b, k), 8f);
                    var pr = prints[(cx * 2 + cy) % prints.Length];
                    if (pr != null) UIKit.PatternFill(sq, pr, new Color(sc.r * 0.75f, sc.g * 0.75f, sc.b * 0.75f, 0.35f * k), 34f, new Vector2(cx * 13f, cy * 7f));
                    RunningStitch(new Rect(sq.x + 6f, sq.y + 6f, sq.width - 12f, sq.height - 12f), new Color(1f, 1f, 1f, 0.55f * k));
                }
            RunningStitch(new Rect(quilt.x + 8f, quilt.y + 8f, pw - 16f, ph - 16f), thread, 12f, 8f, 3f);

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
            // A tape measure along the bottom of the map.
            var tape = new Rect(mapR.x, linen.yMax + 4f, mapR.width, 22f);
            UIKit.Fill(tape, new Color(1f, 0.84f, 0.35f, k), 4f);
            for (float tx = tape.x + 6f; tx < tape.xMax - 4f; tx += 10f)
            {
                bool big = Mathf.RoundToInt((tx - tape.x) / 10f) % 5 == 0;
                UIKit.Fill(new Rect(tx, tape.y, 2f, big ? 12f : 6f), new Color(0.3f, 0.2f, 0.12f, 0.8f * k));
            }

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
                GUIUtility.RotateAroundPivot(45f, notch);
                UIKit.Fill(new Rect(notch.x - 18f, notch.y - 18f, 36f, 36f), new Color(0.62f, 0.32f, 0.36f, k));
                GUI.matrix = saved;
            }
            UIKit.Fill(new Rect(ribbon.x + 3f, ribbon.y + 5f, ribbon.width, ribbon.height), new Color(0f, 0f, 0f, 0.25f * k), 10f);
            UIKit.Fill(ribbon, new Color(0.86f, 0.26f, 0.3f, k), 10f);
            RunningStitch(new Rect(ribbon.x + 8f, ribbon.y + 7f, ribbon.width - 16f, ribbon.height - 14f), thread);
            UIKit.Label(ribbon, level.MapTitle, titleStyle, true);

            // Pins: sewing pins stuck in the map (red), buttons for shops, star buttons for the band.
            var labelStyle = UIKit.Style(21, true, TextAnchor.MiddleLeft, new Color(0.3f, 0.18f, 0.12f, k));
            foreach (var pin in level.MapPins)
            {
                Vector2 p = ToMap(mapR, pin.Position);
                if (pin.Type == MapPin.Kind.Band)
                {
                    bool got = !string.IsNullOrEmpty(pin.Id) && GameState.IsCollected(pin.Id);
                    float bob = Mathf.Sin(Time.unscaledTime * 3f + p.x) * 3f;
                    Button(new Vector2(p.x, p.y + bob), 40f, got ? new Color(1f, 0.8f, 0.25f) : new Color(0.62f, 0.58f, 0.7f), k);
                    UIKit.IconTransformed(new Rect(p.x - 13f, p.y - 13f + bob, 26f, 26f), starTex, new Color(1f, 1f, 1f, 0.9f * k), 0f, 1f);
                    UIKit.Label(new Rect(p.x + 24f, p.y - 16f + bob, 240f, 32f), got ? pin.Name : "???", UIKit.Style(20, true, TextAnchor.MiddleLeft, new Color(1f, 1f, 1f, k)), true);
                    continue;
                }
                if (pin.Type == MapPin.Kind.Shop) Button(p, 30f, new Color(0.35f, 0.6f, 0.95f), k);
                else
                {
                    // The pin's needle, slanting in, and its round glass head.
                    Matrix4x4 saved = GUI.matrix;
                    GUIUtility.RotateAroundPivot(-30f, p);
                    UIKit.Fill(new Rect(p.x - 1.5f, p.y - 2f, 3f, 20f), new Color(0.75f, 0.76f, 0.8f, k), 1.5f);
                    GUI.matrix = saved;
                    Vector2 pinHead = p + new Vector2(-9f, -14f);
                    UIKit.Circle(pinHead + new Vector2(1.5f, 2.5f), 22f, new Color(0f, 0f, 0f, 0.3f * k));
                    UIKit.Circle(pinHead, 22f, red);
                    UIKit.Circle(pinHead + new Vector2(-3.5f, -3.5f), 7f, new Color(1f, 1f, 1f, 0.75f * k));
                }
                // A little fabric name tag with a stitched edge.
                float lw2 = UIKit.TextWidth(pin.Name, labelStyle) + 20f;
                var lr = new Rect(p.x + 12f, p.y - 30f, lw2, 30f);
                UIKit.Fill(new Rect(lr.x + 2f, lr.y + 3f, lr.width, lr.height), new Color(0f, 0f, 0f, 0.2f * k), 6f);
                UIKit.Fill(lr, new Color(0.99f, 0.96f, 0.88f, 0.94f * k), 6f);
                RunningStitch(new Rect(lr.x + 3f, lr.y + 3f, lr.width - 6f, lr.height - 6f), new Color(0.86f, 0.4f, 0.45f, 0.6f * k), 4f, 3f, 1.5f);
                UIKit.Label(new Rect(lr.x + 10f, lr.y, lw2, 30f), pin.Name, labelStyle);
            }

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
                    GUIUtility.RotateAroundPivot(i * 60f + 20f, p);
                    UIKit.Fill(new Rect(p.x - 13f, p.y - 1.5f + (i - 1) * 6f, 26f, 2.5f), new Color(0.85f, 0.35f, 0.5f, k), 1.2f);
                    GUI.matrix = saved;
                }
                UIKit.Label(new Rect(p.x - 40f, p.y + 22f, 80f, 26f), "You", UIKit.Style(18, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, k)), true);
            }

            // The legend, on a felt pocket stitched to the quilt.
            float lx = linen.xMax + 40f, lw = quilt.xMax - lx - 36f;
            var pocket = new Rect(lx - 14f, mapR.y - 10f, lw + 28f, 520f);
            UIKit.Panel(pocket, new Color(0.99f, 0.95f, 0.88f, 0.97f * k), new Color(0.85f, 0.35f, 0.4f, 0.8f * k), 20f);
            float ly = pocket.y + 30f;
            lx += 12f;
            lw -= 24f;
            var head = UIKit.Style(30, true, TextAnchor.MiddleLeft, new Color(0.85f, 0.3f, 0.3f, k));
            var body = UIKit.Style(23, true, TextAnchor.MiddleLeft, new Color(0.3f, 0.18f, 0.12f, k));
            int bands = 0;
            foreach (var pin in level.MapPins) if (pin.Type == MapPin.Kind.Band) bands++;
            if (bands > 0)
            {
                UIKit.Label(new Rect(lx, ly, lw, 40f), "Town Band", head);
                ly += 46f;
                foreach (var pin in level.MapPins)
                {
                    if (pin.Type != MapPin.Kind.Band) continue;
                    bool got = GameState.IsCollected(pin.Id);
                    Button(new Vector2(lx + 15f, ly + 17f), 28f, got ? new Color(1f, 0.8f, 0.25f) : new Color(0.62f, 0.58f, 0.7f), k);
                    UIKit.Label(new Rect(lx + 40f, ly, lw - 40f, 34f), got ? pin.Name + "  - found!" : pin.Name, body);
                    ly += 40f;
                }
                ly += 14f;
            }
            if (level.HeartstringIds.Count > 0)
            {
                UIKit.Heart(new Vector2(lx + 15f, ly + 18f), 30f, new Color(1f, 0.72f, 0.3f, k));
                UIKit.Label(new Rect(lx + 40f, ly, lw - 40f, 36f), "Heartstrings  " + level.HeartstringCount() + " / " + level.HeartstringIds.Count, body);
                ly += 42f;
            }
            if (level.CollectibleIds.Count > 0)
            {
                UIKit.Icon(new Rect(lx, ly + 2f, 30f, 30f), PatchworkAssets.IconButton, new Color(1f, 1f, 1f, k));
                UIKit.Label(new Rect(lx + 40f, ly, lw - 40f, 36f), "Golden Buttons  " + level.CollectedCount() + " / " + level.CollectibleIds.Count, body);
                ly += 42f;
            }
            if (level.SecretIds.Count > 0)
            {
                UIKit.Circle(new Vector2(lx + 15f, ly + 18f), 28f, new Color(0.55f, 0.75f, 1f, k));
                UIKit.Circle(new Vector2(lx + 11f, ly + 14f), 9f, new Color(1f, 1f, 1f, 0.8f * k));
                UIKit.Label(new Rect(lx + 40f, ly, lw - 40f, 36f), "Secret Marbles  " + level.SecretCount() + " / " + level.SecretIds.Count, body);
                ly += 42f;
            }
            ly += 10f;
            var note = UIKit.Style(19, false, TextAnchor.UpperLeft, new Color(0.3f, 0.18f, 0.12f, 0.85f * k), true);
            UIKit.Label(new Rect(lx, ly, lw, pocket.yMax - ly - 16f), level.MapNote, note);

            // The compass: a big button with the directions cross-stitched on it.
            Vector2 cp = new Vector2(lx + lw * 0.5f - 60f, quilt.yMax - 150f);
            Button(cp, 150f, new Color(0.95f, 0.85f, 0.65f), k);
            for (int i = 0; i < 4; i++)
            {
                Matrix4x4 saved = GUI.matrix;
                GUIUtility.RotateAroundPivot(i * 90f, cp);
                for (int s = 0; s < 4; s++)
                    UIKit.Fill(new Rect(cp.x - 2f, cp.y - 62f + s * 11f, 4f, 7f), i == 0 ? red : brown, 2f);
                GUI.matrix = saved;
            }
            UIKit.Label(new Rect(cp.x - 20f, cp.y - 112f, 40f, 34f), "N", UIKit.Style(28, true, TextAnchor.MiddleCenter, new Color(0.85f, 0.3f, 0.3f, k)), true);
            // A spool of thread and a pincushion, sitting on the quilt.
            Vector2 spool = new Vector2(cp.x + 140f, cp.y + 10f);
            UIKit.Fill(new Rect(spool.x - 30f, spool.y - 46f, 60f, 12f), new Color(0.85f, 0.7f, 0.5f, k), 5f);
            UIKit.Fill(new Rect(spool.x - 24f, spool.y - 36f, 48f, 72f), new Color(0.45f, 0.6f, 0.95f, k), 4f);
            for (int i = 0; i < 6; i++) UIKit.Fill(new Rect(spool.x - 24f, spool.y - 30f + i * 11f, 48f, 2f), new Color(0.35f, 0.48f, 0.8f, k));
            UIKit.Fill(new Rect(spool.x - 30f, spool.y + 34f, 60f, 12f), new Color(0.85f, 0.7f, 0.5f, k), 5f);
            Vector2 cush = new Vector2(spool.x + 74f, spool.y + 14f);
            UIKit.Circle(cush, 70f, new Color(0.9f, 0.28f, 0.32f, k));
            Color[] heads = { new Color(1f, 0.85f, 0.3f), new Color(0.5f, 0.75f, 1f), Color.white, new Color(0.6f, 0.9f, 0.5f) };
            for (int i = 0; i < 4; i++)
            {
                Vector2 hp = cush + new Vector2(-18f + i * 12f, -26f - (i % 2) * 8f);
                UIKit.Fill(new Rect(hp.x - 1f, hp.y, 2f, 18f), new Color(0.75f, 0.76f, 0.8f, k));
                UIKit.Circle(hp, 12f, new Color(heads[i].r, heads[i].g, heads[i].b, k));
            }

            UIKit.PromptLabel(quilt.xMax - 260f, quilt.yMax - 62f, 40f, PromptAction.Map, "Close", kb, new Color(0.99f, 0.95f, 0.85f), k);
            if (CanTravel())
                UIKit.PromptLabel(linen.x + 10f, quilt.yMax - 44f, 34f, PromptAction.Confirm, "Travel back to the Pod", kb, new Color(0.99f, 0.95f, 0.85f), k);
        }
    }
}

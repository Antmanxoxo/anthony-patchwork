using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

    // The pull-up map (Map button: R3 / M). A big illustrated map of the level on the left, and
    // a stitched sidebar on the right with where you are, what's left to find, and the key.
    //  - The picture is painted once from a camera high overhead (a felt look for outdoor
    //    levels; a brighter "lamplit plan" for dark indoor ones).
    //  - Places you haven't been yet stay under a soft cloud of parchment until you walk there,
    //    and the map remembers where you've explored between visits.
    //  - Move the cursor with the stick / WASD / the mouse, zoom with L1 / R1 (Z / X, or the
    //    mouse wheel), and Triangle / F puts you back on Patch.
    //  - Point at the Pod or a checkpoint flag you've reached and press Confirm to travel there.
    public class MapScreen
    {
        readonly PatchworkGame game;
        Texture2D painting, arrowTex, starTex, flagTex, fogTex, podTex;
        float openTime;
        Bounds area;
        float half;

        // The view: zoom (1 = the whole map) and the middle of the view, in map space (0..1).
        float zoom = 1.5f, zoomWant = 1.5f;
        Vector2 view = new Vector2(0.5f, 0.5f), cursor = new Vector2(0.5f, 0.5f);
        Vector2 lastMouse;
        bool mouseMode;
        const float MinZoom = 1f, MaxZoom = 4.5f;

        public MapScreen(PatchworkGame game)
        {
            this.game = game;
        }

        public static bool Available(LevelInfo level) { return level != null && level.MapPins.Count > 0; }

        void SetArea()
        {
            var level = game.Level;
            area = level.MapArea.size.sqrMagnitude > 1f ? level.MapArea : level.Area;
            half = Mathf.Max(area.extents.x, area.extents.z) + 2f;
        }

        public void Open()
        {
            openTime = 0f;
            SetArea();
            if (painting == null) Prepare();
            LoadFog();
            RevealAround(game.Player != null ? game.Player.transform.position : game.Level.Spawn);
            SaveFog();
            // Open on Patch, a little zoomed in.
            Vector2 me = game.Player != null ? ToUV(game.Player.transform.position) : new Vector2(0.5f, 0.5f);
            cursor = me;
            zoom = zoomWant = 1.6f;
            view = me;
            ClampView();
            mouseMode = false;
            if (Mouse.current != null) lastMouse = Mouse.current.position.ReadValue();
        }

        // ------------------------------------------------------------------ Controls

        public void Update(float udt)
        {
            openTime += udt;
            var input = game.Input;
            if (openTime < 0.15f) return;
            if (input.Map.WasPressedThisFrame() || input.Cancel.WasPressedThisFrame() || input.Pause.WasPressedThisFrame())
            {
                SaveFog();
                game.CloseMap();
                return;
            }

            // Zoom, towards the cursor.
            float scroll = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
            if (input.RotateRight.WasPressedThisFrame() || scroll > 0.1f) ZoomBy(1.5f);
            if (input.RotateLeft.WasPressedThisFrame() || scroll < -0.1f) ZoomBy(1f / 1.5f);
            float before = zoom;
            zoom = Mathf.Lerp(zoom, zoomWant, 1f - Mathf.Exp(-12f * udt));
            if (Mathf.Abs(zoom - before) > 0.0001f)
            {
                // Keep the point under the cursor where it is on screen.
                Vector2 off = (cursor - view) * before;
                view = cursor - off / zoom;
            }

            // The cursor: stick and keys move it; the mouse takes over when it moves.
            Vector2 move = input.Move.ReadValue<Vector2>();
            if (move.sqrMagnitude > 0.02f)
            {
                mouseMode = false;
                cursor += move * (0.42f / zoom) * udt;
            }
            if (Mouse.current != null)
            {
                Vector2 m = Mouse.current.position.ReadValue();
                if ((m - lastMouse).sqrMagnitude > 4f) mouseMode = true;
                lastMouse = m;
                if (mouseMode)
                {
                    Vector2 g = new Vector2(m.x / UIKit.Scale, (Screen.height - m.y) / UIKit.Scale);
                    Rect mr = MapRect();
                    if (mr.Contains(g)) cursor = FromScreen(mr, g);
                }
            }
            cursor.x = Mathf.Clamp01(cursor.x);
            cursor.y = Mathf.Clamp01(cursor.y);
            if (input.Sticker.WasPressedThisFrame() && game.Player != null)
            {
                cursor = ToUV(game.Player.transform.position);
                view = cursor;
                Sfx.Play(SfxId.MenuMove, 0.4f);
            }
            // The view follows the cursor once it gets near the edge.
            float span = 0.5f / zoom, edge = span * 0.7f;
            Vector2 d = cursor - view;
            if (d.x > edge) view.x += d.x - edge;
            if (d.x < -edge) view.x += d.x + edge;
            if (d.y > edge) view.y += d.y - edge;
            if (d.y < -edge) view.y += d.y + edge;
            ClampView();

            bool click = Mouse.current != null && mouseMode && Mouse.current.leftButton.wasPressedThisFrame;
            if (input.Confirm.WasPressedThisFrame() || click)
            {
                var t = Hovered();
                if (t != null && t.Travel) TravelTo(t);
            }
        }

        void ZoomBy(float f)
        {
            zoomWant = Mathf.Clamp(zoomWant * f, MinZoom, MaxZoom);
            Sfx.Play(SfxId.MenuMove, 0.3f);
        }

        void ClampView()
        {
            float span = 0.5f / Mathf.Max(MinZoom, zoom);
            view.x = Mathf.Clamp(view.x, span, 1f - span);
            view.y = Mathf.Clamp(view.y, span, 1f - span);
        }

        bool CanTravel()
        {
            var level = game.Level;
            return level != null && !level.IsHub && level.Spawn != Vector3.zero && game.Player != null && !game.Player.Motor.ExternalControl;
        }

        void TravelTo(Target t)
        {
            var player = game.Player;
            if (YarnHorse.Ridden != null) YarnHorse.Ridden.Dismount(false);
            SaveFog();
            game.CloseMap();
            FxBurst.Spawn(FxBurst.Kind.Poof, player.Motor.Center, Color.white);
            if (t.Checkpoint != null)
            {
                player.Motor.Teleport(t.Checkpoint.SpawnPoint, t.Checkpoint.SpawnYaw);
                game.SetCheckpoint(t.Checkpoint);
                game.ShowToast("Off to " + t.Name + "!");
            }
            else
            {
                player.Motor.Teleport(game.Level.Spawn, game.Level.SpawnYaw);
                game.ShowToast("Back at the Pod!");
            }
            FxBurst.Spawn(FxBurst.Kind.Sparkle, player.transform.position + Vector3.up, new Color(1f, 0.85f, 0.5f));
            Sfx.Play(SfxId.PodRocket, 0.45f);
        }

        // ------------------------------------------------------------------ Exploring
        // A grid over the map: a cell is uncovered once Patch has been near it. Remembered
        // between visits (per level).

        const int FogN = 96;
        bool[] seen;
        bool fogDirty;
        string fogLevel;
        float revealTimer;

        string FogKey { get { return "pw_map_" + game.Level.Id; } }

        void LoadFog()
        {
            if (game.Level == null) return;
            if (seen != null && fogLevel == game.Level.Id) return;
            fogLevel = game.Level.Id;
            seen = new bool[FogN * FogN];
            fogDirty = true;
            string s = PlayerPrefs.GetString(FogKey, "");
            if (string.IsNullOrEmpty(s)) return;
            try
            {
                var bytes = System.Convert.FromBase64String(s);
                for (int i = 0; i < seen.Length && i / 8 < bytes.Length; i++) seen[i] = (bytes[i / 8] & (1 << (i % 8))) != 0;
            }
            catch (System.Exception) { }
        }

        void SaveFog()
        {
            if (seen == null || game.Level == null || fogLevel != game.Level.Id) return;
            var bytes = new byte[(seen.Length + 7) / 8];
            for (int i = 0; i < seen.Length; i++) if (seen[i]) bytes[i / 8] |= (byte)(1 << (i % 8));
            PlayerPrefs.SetString(FogKey, System.Convert.ToBase64String(bytes));
        }

        public static void ForgetAll(IEnumerable<string> levelIds)
        {
            foreach (var id in levelIds) PlayerPrefs.DeleteKey("pw_map_" + id);
        }

        void RevealAround(Vector3 p)
        {
            if (seen == null) return;
            float cell = half * 2f / FogN;
            float r = Mathf.Max(16f, cell * 4f);
            Vector2 uv = ToUV(p);
            int cx = Mathf.FloorToInt(uv.x * FogN), cy = Mathf.FloorToInt(uv.y * FogN);
            int rc = Mathf.CeilToInt(r / cell);
            for (int y = cy - rc; y <= cy + rc; y++)
                for (int x = cx - rc; x <= cx + rc; x++)
                {
                    if (x < 0 || y < 0 || x >= FogN || y >= FogN) continue;
                    float dx = (x + 0.5f) / FogN - uv.x, dy = (y + 0.5f) / FogN - uv.y;
                    if ((dx * dx + dy * dy) * (half * 2f) * (half * 2f) > r * r) continue;
                    int i = y * FogN + x;
                    if (!seen[i]) { seen[i] = true; fogDirty = true; }
                }
        }

        bool Explored(Vector2 uv)
        {
            if (seen == null) return true;
            int x = Mathf.Clamp(Mathf.FloorToInt(uv.x * FogN), 0, FogN - 1), y = Mathf.Clamp(Mathf.FloorToInt(uv.y * FogN), 0, FogN - 1);
            // Count a spot as found if it or a neighbour has been seen (pins sit on edges).
            for (int oy = -1; oy <= 1; oy++)
                for (int ox = -1; ox <= 1; ox++)
                {
                    int xx = x + ox, yy = y + oy;
                    if (xx >= 0 && yy >= 0 && xx < FogN && yy < FogN && seen[yy * FogN + xx]) return true;
                }
            return false;
        }

        float ExploredShare()
        {
            if (seen == null) return 0f;
            int n = 0;
            foreach (bool b in seen) if (b) n++;
            return n / (float)seen.Length;
        }

        void BuildFog()
        {
            if (!fogDirty || seen == null) return;
            fogDirty = false;
            if (fogTex == null)
            {
                fogTex = new Texture2D(FogN, FogN, TextureFormat.RGBA32, false);
                fogTex.wrapMode = TextureWrapMode.Clamp;
                fogTex.filterMode = FilterMode.Bilinear;
            }
            var px = new Color[FogN * FogN];
            for (int y = 0; y < FogN; y++)
                for (int x = 0; x < FogN; x++)
                {
                    // Softened: a cell's cloud thins where its neighbours are uncovered.
                    float s = 0f, w = 0f;
                    for (int oy = -1; oy <= 1; oy++)
                        for (int ox = -1; ox <= 1; ox++)
                        {
                            int xx = Mathf.Clamp(x + ox, 0, FogN - 1), yy = Mathf.Clamp(y + oy, 0, FogN - 1);
                            float k = ox == 0 && oy == 0 ? 2f : 1f;
                            s += seen[yy * FogN + xx] ? 0f : k;
                            w += k;
                        }
                    float a = s / w;
                    float grain = Hash(x, y) * 0.06f;
                    px[y * FogN + x] = new Color(0.9f - grain, 0.84f - grain, 0.72f - grain, Mathf.SmoothStep(0f, 1f, a) * 0.93f);
                }
            fogTex.SetPixels(px);
            fogTex.Apply(false);
        }

        // Checkpoints you've actually stood at (they become places you can travel to).
        string CheckpointKey(Checkpoint cp)
        {
            return "map_cp_" + game.Level.Id + "_" + Mathf.RoundToInt(cp.SpawnPoint.x) + "_" + Mathf.RoundToInt(cp.SpawnPoint.z);
        }

        static readonly List<Checkpoint> checkpoints = new List<Checkpoint>();

        List<Checkpoint> Checkpoints()
        {
            checkpoints.Clear();
            foreach (var v in PlayerVolume.All)
            {
                var cp = v as Checkpoint;
                if (cp != null && cp.isActiveAndEnabled) checkpoints.Add(cp);
            }
            return checkpoints;
        }

        // ------------------------------------------------------------------ Painting the map
        // A camera high overhead renders the level into a texture for a few frames (started when
        // the level loads), then the picture is read back and painted over.

        Camera capCam;
        RenderTexture capRT;
        int capFrames;

        public void Prepare()
        {
            if (painting != null || capCam != null) return;
            var level = game.Level;
            SetArea();
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

        // Called every frame by the game: finishes the painting once the camera has drawn, and
        // uncovers the map around Patch as he goes.
        public void Tick()
        {
            if (game.Level != null && game.Player != null && game.Mode == UIMode.Gameplay && Available(game.Level))
            {
                revealTimer -= Time.unscaledDeltaTime;
                if (revealTimer <= 0f)
                {
                    revealTimer = 0.3f;
                    if (seen == null || fogLevel != game.Level.Id) { SetArea(); LoadFog(); }
                    Vector3 p = game.Player.transform.position;
                    RevealAround(p);
                    foreach (var cp in Checkpoints())
                        if ((cp.SpawnPoint - p).sqrMagnitude < 36f) GameState.SetHintSeen(CheckpointKey(cp));
                    saveTimer -= 0.3f;
                    if (saveTimer <= 0f) { saveTimer = 20f; SaveFog(); }
                }
            }
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
                bool indoor = game.Level.MapCameraHeight < 20f;
                painting = indoor ? Lamplit(raw) : Stylize(raw);
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

        float saveTimer = 20f;

        // Outdoor levels: a clean illustrated map, like one cut from coloured felt. The picture
        // is smoothed, every pixel is matched to one of a few flat map colours (meadow, wood,
        // water, path, stone, and soft pastels for roofs and toys), and a thin darker line is
        // drawn wherever two colours meet.
        static Texture2D Stylize(Texture2D src)
        {
            int w = src.width, h = src.height;
            var px = Blur(Blur(src.GetPixels(), w, h, 2), w, h, 2);
            var kind = new int[w * h];
            for (int i = 0; i < px.Length; i++) MapColour(px[i], out kind[i]);
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
            var line = new Color(0.3f, 0.24f, 0.26f);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x, k = tidy[i];
                    Color c = KindColour(k);
                    int r = tidy[y * w + Mathf.Min(x + 1, w - 1)], d = tidy[Mathf.Max(y - 1, 0) * w + x];
                    int r2 = tidy[y * w + Mathf.Min(x + 2, w - 1)], d2 = tidy[Mathf.Max(y - 2, 0) * w + x];
                    if (r != k || d != k) c = Color.Lerp(c, line, 0.42f);          // the outline
                    else if (r2 != k || d2 != k) c = Color.Lerp(c, line, 0.16f);
                    // A soft drop shadow down-right of every edge, so things sit up off the cloth.
                    int ul = tidy[Mathf.Min(y + 3, h - 1) * w + Mathf.Max(x - 3, 0)];
                    if (ul != k && k <= 1) c *= 0.9f;
                    float grain = (Hash(x, y) - 0.5f) * 0.02f;                         // a breath of felt
                    c.r += grain; c.g += grain; c.b += grain;
                    c.a = 1f;
                    outPx[i] = c;
                }
            return Finish(outPx, w, h);
        }

        // Indoor levels (dark, lamplit halls): the real colours kept, but brightened and
        // levelled so the floor plan reads, flattened into a few soft bands like a hand-tinted
        // plan, with an ink line round every wall and a little paper grain.
        static Texture2D Lamplit(Texture2D src)
        {
            int w = src.width, h = src.height;
            var raw = src.GetPixels();
            var bg = new Color(0.93f, 0.88f, 0.76f);
            var px = Blur(raw, w, h, 1);
            // Find the dark and bright ends of the picture (leaving out the paper behind it).
            var hist = new int[256];
            int count = 0;
            var lum = new float[px.Length];
            var empty = new bool[px.Length];
            for (int i = 0; i < px.Length; i++)
            {
                Color c = raw[i];
                empty[i] = Mathf.Abs(c.r - bg.r) < 0.02f && Mathf.Abs(c.g - bg.g) < 0.02f && Mathf.Abs(c.b - bg.b) < 0.02f;
                lum[i] = px[i].r * 0.3f + px[i].g * 0.55f + px[i].b * 0.15f;
                if (empty[i]) continue;
                hist[Mathf.Clamp(Mathf.RoundToInt(lum[i] * 255f), 0, 255)]++;
                count++;
            }
            float lo = 0f, hi = 1f;
            int acc = 0;
            for (int b = 0; b < 256; b++) { acc += hist[b]; if (acc > count * 0.02f) { lo = b / 255f; break; } }
            acc = 0;
            for (int b = 255; b >= 0; b--) { acc += hist[b]; if (acc > count * 0.02f) { hi = b / 255f; break; } }
            float range = Mathf.Max(0.05f, hi - lo);
            var outPx = new Color[px.Length];
            var band = new float[px.Length];
            for (int i = 0; i < px.Length; i++)
            {
                if (empty[i]) { band[i] = -1f; continue; }
                Color c = px[i];
                float hh, s, v;
                Color.RGBToHSV(c, out hh, out s, out v);
                float vl = Mathf.Clamp01((v - lo) / range);
                vl = Mathf.Pow(vl, 0.7f);
                // Flatten into five soft bands of light, so it looks painted rather than photographed.
                float q = Mathf.Round(vl * 4f) / 4f;
                vl = Mathf.Lerp(vl, q, 0.65f);
                vl = Mathf.Lerp(0.34f, 0.97f, vl);
                s = Mathf.Clamp01(s * 0.9f + 0.06f);
                band[i] = q;
                outPx[i] = Color.HSVToRGB(hh, s, vl);
            }
            var ink = new Color(0.22f, 0.17f, 0.22f);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    Color c;
                    if (empty[i])
                    {
                        // The paper outside the building, with a soft shadow round the walls.
                        c = bg;
                        bool near = false;
                        for (int o = 2; o <= 8 && !near; o += 3)
                        {
                            int xx = Mathf.Max(0, x - o), yy = Mathf.Min(h - 1, y + o);
                            if (!empty[yy * w + xx]) near = true;
                        }
                        if (near) c *= 0.88f;
                    }
                    else
                    {
                        c = outPx[i];
                        // Ink where the bands change, and a heavy line round the outside.
                        int r = Mathf.Min(x + 1, w - 1), d = Mathf.Max(y - 1, 0);
                        bool edgeOut = empty[y * w + r] || empty[d * w + x] || empty[y * w + Mathf.Max(x - 1, 0)] || empty[Mathf.Min(y + 1, h - 1) * w + x];
                        float dl = Mathf.Abs(lum[i] - lum[y * w + r]) + Mathf.Abs(lum[i] - lum[d * w + x]);
                        if (edgeOut) c = Color.Lerp(c, ink, 0.85f);
                        else if (band[i] != band[y * w + r] || band[i] != band[d * w + x] || dl > 0.09f) c = Color.Lerp(c, ink, 0.32f);
                    }
                    float grain = (Hash(x, y) - 0.5f) * 0.035f;
                    c.r += grain; c.g += grain; c.b += grain * 0.8f;
                    c.a = 1f;
                    outPx[i] = c;
                }
            return Finish(outPx, w, h);
        }

        static Texture2D Finish(Color[] px, int w, int h)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, true);
            t.wrapMode = TextureWrapMode.Clamp;
            t.filterMode = FilterMode.Trilinear;
            t.anisoLevel = 4;
            t.SetPixels(px);
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
            flagTex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            podTex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            var a = new Color[n * n];
            var s = new Color[n * n];
            var f = new Color[n * n];
            var p = new Color[n * n];
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
                    // An embroidery hoop with a heart stitched in it (the checkpoints).
                    float hr = Mathf.Sqrt(u * u + (v - 0.08f) * (v - 0.08f));
                    bool ring = hr > 0.66f && hr < 0.86f;
                    bool screw = Mathf.Abs(u) < 0.12f && v > 0.8f && v < 1f;
                    float hx = Mathf.Abs(u) / 0.42f, hy = (v - 0.02f) / 0.42f;   // a little heart
                    float hh2 = hx * hx + Mathf.Pow(hy - Mathf.Sqrt(hx), 2f);
                    bool heartIn = hh2 < 1f;
                    f[y * n + x] = new Color(1f, 1f, 1f, ring || screw || heartIn ? 1f : 0f);
                    // The Pod: a round capsule with a porthole and two little fins.
                    float body = (u * u) / 0.35f + (v * v) / 0.72f;
                    bool fin = v < -0.45f && v > -0.9f && Mathf.Abs(u) > 0.35f && Mathf.Abs(u) < 0.75f && Mathf.Abs(u) - 0.35f < (v + 0.95f) * 0.8f;
                    bool hole = u * u + (v - 0.15f) * (v - 0.15f) < 0.06f;
                    p[y * n + x] = new Color(1f, 1f, 1f, (body < 1f && !hole) || fin ? 1f : 0f);
                }
            arrowTex.SetPixels(a); arrowTex.Apply();
            starTex.SetPixels(s); starTex.Apply();
            flagTex.SetPixels(f); flagTex.Apply();
            podTex.SetPixels(p); podTex.Apply();
        }

        public void Release()
        {
            SaveFog();
            foreach (var t in new[] { painting, arrowTex, starTex, flagTex, fogTex, podTex })
                if (t != null) Object.Destroy(t);
            painting = arrowTex = starTex = flagTex = fogTex = podTex = null;
            seen = null;
        }

        // ------------------------------------------------------------------ Space

        Vector2 ToUV(Vector3 world)
        {
            return new Vector2((world.x - (area.center.x - half)) / (half * 2f), (world.z - (area.center.z - half)) / (half * 2f));
        }

        Rect MapRect()
        {
            float W = UIKit.Width;
            float side = Mathf.Clamp(W * 0.3f, 420f, 560f);
            float s = Mathf.Min(1080f - 150f, W - side - 150f);
            return new Rect(60f, (1080f - s) * 0.5f + 6f, s, s);
        }

        Vector2 ToScreen(Rect m, Vector2 uv)
        {
            float span = 0.5f / zoom;
            float u = (uv.x - (view.x - span)) / (span * 2f), v = (uv.y - (view.y - span)) / (span * 2f);
            return new Vector2(m.x + u * m.width, m.yMax - v * m.height);
        }

        Vector2 FromScreen(Rect m, Vector2 g)
        {
            float span = 0.5f / zoom;
            float u = (g.x - m.x) / m.width, v = (m.yMax - g.y) / m.height;
            return new Vector2(view.x - span + u * span * 2f, view.y - span + v * span * 2f);
        }

        // ------------------------------------------------------------------ Things on the map

        class Target
        {
            public Vector2 UV;
            public string Name, Note;
            public bool Travel;
            public Checkpoint Checkpoint;
        }

        readonly List<Target> targets = new List<Target>();

        // The numbered places, in reading order (top row of the map first, left to right).
        List<MapPin> Numbered(LevelInfo level)
        {
            var list = new List<MapPin>();
            foreach (var pin in level.MapPins)
                if (pin.Type == MapPin.Kind.Place || pin.Type == MapPin.Kind.Shop) list.Add(pin);
            float band = half * 2f / 6f;
            list.Sort((x, y) =>
            {
                int rx = Mathf.FloorToInt((area.center.z + half - x.Position.z) / band), ry = Mathf.FloorToInt((area.center.z + half - y.Position.z) / band);
                return rx != ry ? rx.CompareTo(ry) : x.Position.x.CompareTo(y.Position.x);
            });
            return list;
        }

        // The name of the region nearest a spot (the Area pins).
        string RegionNear(Vector3 p, out float dist)
        {
            string best = null;
            dist = float.MaxValue;
            foreach (var pin in game.Level.MapPins)
            {
                if (pin.Type != MapPin.Kind.Area) continue;
                float d = new Vector2(pin.Position.x - p.x, pin.Position.z - p.z).magnitude;
                if (d < dist) { dist = d; best = pin.Name; }
            }
            return best;
        }

        string PlaceNear(Vector3 p)
        {
            string best = null;
            float bd = float.MaxValue;
            foreach (var pin in game.Level.MapPins)
            {
                float d = new Vector2(pin.Position.x - p.x, pin.Position.z - p.z).sqrMagnitude;
                if (d < bd) { bd = d; best = pin.Name; }
            }
            return best;
        }

        void GatherTargets(LevelInfo level)
        {
            targets.Clear();
            bool travel = CanTravel();
            if (!level.IsHub && level.Spawn != Vector3.zero)
                targets.Add(new Target { UV = ToUV(level.Spawn), Name = "The Pod", Note = travel ? "Travel back to the Pod" : null, Travel = travel });
            foreach (var cp in Checkpoints())
            {
                bool reached = GameState.IsHintSeen(CheckpointKey(cp));
                Vector2 uv = ToUV(cp.SpawnPoint);
                if (!reached && !Explored(uv)) continue;
                string near = PlaceNear(cp.SpawnPoint);
                targets.Add(new Target
                {
                    UV = uv,
                    Name = near != null ? near : "Checkpoint",
                    Note = reached ? (travel ? "Travel to this checkpoint" : "Checkpoint") : "Checkpoint (not reached yet)",
                    Travel = travel && reached,
                    Checkpoint = cp
                });
            }
            foreach (var pin in Numbered(level))
            {
                Vector2 uv = ToUV(pin.Position);
                targets.Add(new Target { UV = uv, Name = Explored(uv) ? pin.Name : "???", Note = Explored(uv) ? null : "Not explored yet" });
            }
            foreach (var pin in level.MapPins)
            {
                if (pin.Type != MapPin.Kind.Band) continue;
                bool got = !string.IsNullOrEmpty(pin.Id) && GameState.IsCollected(pin.Id);
                targets.Add(new Target { UV = ToUV(pin.Position), Name = got ? pin.Name : "???", Note = got ? "Found!" : "Someone's waiting here" });
            }
        }

        Target Hovered()
        {
            if (game.Level == null) return null;
            GatherTargets(game.Level);
            Rect m = MapRect();
            Vector2 c = ToScreen(m, cursor);
            Target best = null;
            float bd = 30f * 30f;
            foreach (var t in targets)
            {
                float d = (ToScreen(m, t.UV) - c).sqrMagnitude;
                // Travel spots win ties (they're the ones you can do something with).
                if (t.Travel) d *= 0.6f;
                if (d < bd) { bd = d; best = t; }
            }
            return best;
        }

        // ------------------------------------------------------------------ Drawing helpers

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

        static readonly Color Cream = new Color(0.99f, 0.96f, 0.88f), Ink = new Color(0.3f, 0.18f, 0.12f);
        static readonly Color PlaceRed = new Color(0.86f, 0.24f, 0.24f), ShopBlue = new Color(0.3f, 0.55f, 0.92f);
        static readonly Color Rose = new Color(0.85f, 0.35f, 0.4f), Gold = new Color(1f, 0.8f, 0.25f);

        // A turn about a point, in the menu's own coordinates.
        static Matrix4x4 Turn(float degrees, Vector2 pivot)
        {
            var p3 = new Vector3(pivot.x, pivot.y, 0f);
            return Matrix4x4.Translate(p3) * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, degrees)) * Matrix4x4.Translate(-p3);
        }

        // A felt tag with a stitched edge.
        static void Tag(Rect r, Color stitch, float k)
        {
            UIKit.Fill(new Rect(r.x + 2f, r.y + 3f, r.width, r.height), new Color(0f, 0f, 0f, 0.22f * k), 7f);
            UIKit.Fill(r, new Color(Cream.r, Cream.g, Cream.b, 0.96f * k), 7f);
            RunningStitch(new Rect(r.x + 3f, r.y + 3f, r.width - 6f, r.height - 6f), new Color(stitch.r, stitch.g, stitch.b, 0.7f * k), 5f, 3f, 1.4f);
        }

        static void Badge(Vector2 c, float d, Color col, string text, float k)
        {
            UIKit.Circle(c + new Vector2(1.5f, 2f), d, new Color(0f, 0f, 0f, 0.3f * k));
            UIKit.Circle(c, d, new Color(1f, 1f, 1f, k));
            UIKit.Circle(c, d - 5f, new Color(col.r, col.g, col.b, k));
            UIKit.Label(new Rect(c.x - d, c.y - d * 0.5f, d * 2f, d), text, UIKit.Style(Mathf.RoundToInt(d * 0.52f), true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, k)));
        }

        static bool Inside(Rect r, Vector2 p, float pad) { return p.x > r.x - pad && p.x < r.xMax + pad && p.y > r.y - pad && p.y < r.yMax + pad; }

        // ------------------------------------------------------------------ Drawing

        public void Draw(bool kb)
        {
            MakeIcons();
            BuildFog();
            var level = game.Level;
            float W = UIKit.Width, H = UIKit.Height;
            float k = Mathf.Clamp01(openTime / 0.2f);
            float pop = UIKit.EaseOutBack(Mathf.Clamp01(openTime / 0.35f));
            float drop = (1f - pop) * 50f;
            UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.1f, 0.07f, 0.13f, 0.72f * k));
            if (UIArt.PatternKnit != null) UIKit.PatternFill(new Rect(0f, 0f, W, H), UIArt.PatternKnit, new Color(1f, 1f, 1f, 0.03f * k), 60f, Vector2.zero);

            Rect mr = MapRect();
            mr.y += drop;

            // ----- The map, in a quilted frame -----
            var frame = new Rect(mr.x - 26f, mr.y - 26f, mr.width + 52f, mr.height + 52f);
            UIKit.Fill(new Rect(frame.x + 8f, frame.y + 12f, frame.width, frame.height), new Color(0f, 0f, 0f, 0.35f * k), 24f);
            UIKit.Fill(frame, new Color(0.62f, 0.32f, 0.36f, k), 24f);
            RunningStitch(new Rect(frame.x + 9f, frame.y + 9f, frame.width - 18f, frame.height - 18f), new Color(1f, 0.93f, 0.82f, 0.8f * k), 10f, 7f, 2.5f);
            UIKit.Fill(mr, new Color(0.93f, 0.88f, 0.76f, k));
            float span = 0.5f / zoom;
            var tc = new Rect(view.x - span, view.y - span, span * 2f, span * 2f);
            if (painting != null) GUI.DrawTextureWithTexCoords(mr, painting, tc, true);
            else UIKit.Label(mr, "Stitching the map...", UIKit.Style(30, true, TextAnchor.MiddleCenter, new Color(0.36f, 0.22f, 0.16f, 0.6f * k)));
            if (fogTex != null) GUI.DrawTextureWithTexCoords(mr, fogTex, tc, true);
            // A faint grid, like a surveyor's plan.
            float gridStep = 1f / 12f;
            for (float g = Mathf.Ceil((view.x - span) / gridStep) * gridStep; g < view.x + span; g += gridStep)
                UIKit.Fill(new Rect(ToScreen(mr, new Vector2(g, 0f)).x, mr.y, 1.2f, mr.height), new Color(0.35f, 0.25f, 0.2f, 0.08f * k));
            for (float g = Mathf.Ceil((view.y - span) / gridStep) * gridStep; g < view.y + span; g += gridStep)
                UIKit.Fill(new Rect(mr.x, ToScreen(mr, new Vector2(0f, g)).y, mr.width, 1.2f), new Color(0.35f, 0.25f, 0.2f, 0.08f * k));

            DrawRegions(mr, level, k);
            DrawMarkers(mr, level, k);
            DrawPatch(mr, k);
            var hover = DrawCursor(mr, k, kb);

            // Edge of the map (over anything poking out), and the compass.
            UIKit.Outline(mr, new Color(0.35f, 0.22f, 0.2f, 0.9f * k), 4f, 2f);
            var nc = new Vector2(mr.xMax - 44f, mr.y + 46f);
            UIKit.Circle(nc + new Vector2(2f, 3f), 64f, new Color(0f, 0f, 0f, 0.25f * k));
            UIKit.Circle(nc, 64f, new Color(Cream.r, Cream.g, Cream.b, 0.95f * k));
            UIKit.CircleOutline(nc, 54f, new Color(Rose.r, Rose.g, Rose.b, 0.7f * k), 2f);
            UIKit.IconTransformed(new Rect(nc.x - 13f, nc.y - 26f, 26f, 26f), arrowTex, new Color(0.85f, 0.3f, 0.3f, k), 0f, 1f);
            UIKit.Label(new Rect(nc.x - 20f, nc.y - 2f, 40f, 26f), "N", UIKit.Style(20, true, TextAnchor.MiddleCenter, new Color(Ink.r, Ink.g, Ink.b, k)));
            // Zoom pips, bottom left of the map.
            for (int i = 0; i < 5; i++)
            {
                float zl = Mathf.Lerp(MinZoom, MaxZoom, i / 4f);
                bool on = zoomWant >= zl - 0.01f;
                UIKit.Circle(new Vector2(mr.x + 26f + i * 22f, mr.yMax - 24f), on ? 14f : 10f, on ? new Color(Rose.r, Rose.g, Rose.b, k) : new Color(Ink.r, Ink.g, Ink.b, 0.35f * k));
            }

            DrawSidebar(new Rect(frame.xMax + 34f, frame.y, W - frame.xMax - 34f - 50f, frame.height), level, k, kb, hover);
        }

        // Region names on felt tags (only for regions you've found).
        void DrawRegions(Rect mr, LevelInfo level, float k)
        {
            var style = UIKit.Style(Mathf.RoundToInt(Mathf.Lerp(16f, 22f, Mathf.InverseLerp(1f, 3f, zoom))), true, TextAnchor.MiddleCenter, new Color(Ink.r, Ink.g, Ink.b, k));
            var stitch = new Color(0.45f, 0.62f, 0.4f);
            var placed = new List<Rect>();
            foreach (var pin in level.MapPins)
            {
                if (pin.Type != MapPin.Kind.Area) continue;
                Vector2 uv = ToUV(pin.Position);
                if (!Explored(uv)) continue;
                Vector2 p = ToScreen(mr, uv);
                if (!Inside(mr, p, 60f)) continue;
                string text = pin.Name.ToUpperInvariant();
                float w = UIKit.TextWidth(text, style) + 28f, h = style.fontSize + 16f;
                bool tall = pin.Name.Contains("Ravine") || pin.Name.Contains("Canal");
                if (tall)
                {
                    var box = new Rect(p.x - h * 0.5f, p.y - w * 0.5f, h, w);
                    Matrix4x4 saved = GUI.matrix;
                    GUI.matrix = saved * Turn(-90f, box.center);
                    var lr = new Rect(box.center.x - w * 0.5f, box.center.y - h * 0.5f, w, h);
                    Tag(lr, stitch, k * 0.92f);
                    UIKit.Label(lr, text, style);
                    GUI.matrix = saved;
                    placed.Add(box);
                    continue;
                }
                var r = new Rect(p.x - w * 0.5f, p.y - h * 0.5f, w, h);
                foreach (var other in placed) if (r.Overlaps(other)) r.y = other.yMax + 4f;
                placed.Add(r);
                Tag(r, stitch, k * 0.92f);
                UIKit.Label(r, text, style);
            }
        }

        // The Pod, checkpoints, numbered places and the Town Band.
        void DrawMarkers(Rect mr, LevelInfo level, float k)
        {
            // Checkpoint flags.
            foreach (var cp in Checkpoints())
            {
                Vector2 uv = ToUV(cp.SpawnPoint);
                bool reached = GameState.IsHintSeen(CheckpointKey(cp));
                if (!reached && !Explored(uv)) continue;
                Vector2 p = ToScreen(mr, uv);
                if (!Inside(mr, p, -8f)) continue;
                UIKit.Circle(p + new Vector2(1.5f, 2f), 34f, new Color(0f, 0f, 0f, 0.25f * k));
                UIKit.Circle(p, 34f, new Color(1f, 1f, 1f, k));
                UIKit.Circle(p, 28f, reached ? new Color(0.98f, 0.92f, 0.78f, k) : new Color(0.8f, 0.78f, 0.82f, k));
                UIKit.Icon(new Rect(p.x - 12f, p.y - 13f, 24f, 24f), flagTex, reached ? new Color(Gold.r * 0.95f, Gold.g * 0.7f, 0.15f, k) : new Color(0.5f, 0.48f, 0.55f, k));
            }
            // The Pod.
            if (!level.IsHub && level.Spawn != Vector3.zero)
            {
                Vector2 p = ToScreen(mr, ToUV(level.Spawn));
                if (Inside(mr, p, -8f))
                {
                    UIKit.Circle(p + new Vector2(1.5f, 2.5f), 44f, new Color(0f, 0f, 0f, 0.3f * k));
                    UIKit.Circle(p, 44f, new Color(1f, 1f, 1f, k));
                    UIKit.Circle(p, 37f, new Color(0.45f, 0.62f, 0.9f, k));
                    UIKit.Icon(new Rect(p.x - 14f, p.y - 15f, 28f, 28f), podTex, new Color(1f, 1f, 1f, k));
                }
            }
            // Numbered places (a hidden "?" until you've been near).
            var list = Numbered(level);
            float d = Mathf.Lerp(26f, 32f, Mathf.InverseLerp(1f, 3f, zoom));
            for (int i = 0; i < list.Count; i++)
            {
                Vector2 uv = ToUV(list[i].Position);
                Vector2 p = ToScreen(mr, uv);
                if (!Inside(mr, p, -6f)) continue;
                bool known = Explored(uv);
                Badge(p, d, !known ? new Color(0.62f, 0.58f, 0.66f) : list[i].Type == MapPin.Kind.Shop ? ShopBlue : PlaceRed, known ? (i + 1).ToString() : "?", k);
                // Zoomed in far enough, the names are written beside the badges.
                if (known && zoom > 2.4f)
                {
                    var st = UIKit.Style(15, true, TextAnchor.MiddleLeft, new Color(Ink.r, Ink.g, Ink.b, k));
                    float tw = UIKit.TextWidth(list[i].Name, st) + 16f;
                    var r = new Rect(p.x + d * 0.5f + 4f, p.y - 12f, tw, 24f);
                    UIKit.Fill(r, new Color(Cream.r, Cream.g, Cream.b, 0.9f * k), 6f);
                    UIKit.Label(new Rect(r.x + 8f, r.y, r.width - 8f, r.height), list[i].Name, st);
                }
            }
            // The Town Band: gold buttons once found, grey until then.
            foreach (var pin in level.MapPins)
            {
                if (pin.Type != MapPin.Kind.Band) continue;
                Vector2 p = ToScreen(mr, ToUV(pin.Position));
                if (!Inside(mr, p, -8f)) continue;
                bool got = !string.IsNullOrEmpty(pin.Id) && GameState.IsCollected(pin.Id);
                Button(p, 34f, got ? Gold : new Color(0.62f, 0.58f, 0.7f), k);
                UIKit.IconTransformed(new Rect(p.x - 11f, p.y - 11f, 22f, 22f), starTex, new Color(1f, 1f, 1f, 0.9f * k), 0f, 1f);
            }
        }

        // Patch: a ball of pink yarn with an arrow showing which way he's facing. When he's off
        // the edge of the view, a little arrow at the edge points the way.
        void DrawPatch(Rect mr, float k)
        {
            if (game.Player == null) return;
            Vector2 p = ToScreen(mr, ToUV(game.Player.transform.position));
            float yaw = game.Player.transform.eulerAngles.y;
            if (!Inside(mr, p, -10f))
            {
                Vector2 c = mr.center, dir = (p - c).normalized;
                float tx = dir.x != 0f ? (mr.width * 0.5f - 22f) / Mathf.Abs(dir.x) : float.MaxValue;
                float ty = dir.y != 0f ? (mr.height * 0.5f - 22f) / Mathf.Abs(dir.y) : float.MaxValue;
                Vector2 e = c + dir * Mathf.Min(tx, ty);
                UIKit.Circle(e, 36f, new Color(1f, 0.55f, 0.68f, k));
                UIKit.IconTransformed(new Rect(e.x - 14f, e.y - 14f, 28f, 28f), arrowTex, new Color(1f, 1f, 1f, k), Mathf.Atan2(dir.x, -dir.y) * Mathf.Rad2Deg, 1f);
                return;
            }
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 5f) * 0.1f;
            UIKit.Circle(p, 70f * pulse, new Color(1f, 0.55f, 0.68f, 0.22f * k));
            UIKit.CircleOutline(p, 70f * pulse, new Color(1f, 1f, 1f, 0.55f * k), 2f);
            UIKit.IconTransformed(new Rect(p.x - 28f, p.y - 28f, 56f, 56f), arrowTex, new Color(0.15f, 0.1f, 0.2f, k), yaw, 1.25f);
            UIKit.Circle(p, 30f, new Color(1f, 1f, 1f, k));
            UIKit.Circle(p, 25f, new Color(1f, 0.55f, 0.68f, k));
            for (int i = 0; i < 3; i++)
            {
                Matrix4x4 saved = GUI.matrix;
                GUI.matrix = saved * Turn(i * 60f + 20f, p);
                UIKit.Fill(new Rect(p.x - 11f, p.y - 1.25f + (i - 1) * 5f, 22f, 2.5f), new Color(0.85f, 0.35f, 0.5f, k), 1.2f);
                GUI.matrix = saved;
            }
        }

        // The cursor: a stitched ring; over something it snaps on and says what it is.
        Target DrawCursor(Rect mr, float k, bool kb)
        {
            var hover = Hovered();
            Vector2 c = ToScreen(mr, cursor);
            if (hover != null) c = Vector2.Lerp(c, ToScreen(mr, hover.UV), 0.85f);
            float ring = hover != null ? 52f : 40f;
            float spin = Time.unscaledTime * 40f;
            for (int i = 0; i < 4; i++)
            {
                Matrix4x4 saved = GUI.matrix;
                GUI.matrix = saved * Turn(spin + i * 90f, c);
                UIKit.Fill(new Rect(c.x + ring * 0.5f - 10f, c.y - 2f, 12f, 4f), new Color(1f, 1f, 1f, k), 2f);
                UIKit.Fill(new Rect(c.x + ring * 0.5f - 10f, c.y - 1f, 12f, 2f), new Color(Rose.r, Rose.g, Rose.b, k), 1f);
                GUI.matrix = saved;
            }
            UIKit.CircleOutline(c, ring, new Color(1f, 1f, 1f, 0.85f * k), 2.5f);
            if (hover == null) return null;
            // The tooltip.
            var nameStyle = UIKit.Style(22, true, TextAnchor.MiddleLeft, new Color(Ink.r, Ink.g, Ink.b, k));
            var noteStyle = UIKit.Style(16, true, TextAnchor.MiddleLeft, new Color(0.55f, 0.3f, 0.32f, k));
            float w = Mathf.Max(UIKit.TextWidth(hover.Name, nameStyle), hover.Note != null ? UIKit.TextWidth(hover.Note, noteStyle) + (hover.Travel ? 50f : 0f) : 0f) + 32f;
            float h = hover.Note != null ? 70f : 44f;
            var r = new Rect(c.x + 34f, c.y - h - 10f, w, h);
            if (r.xMax > mr.xMax - 6f) r.x = c.x - 34f - w;
            if (r.y < mr.y + 6f) r.y = c.y + 30f;
            Tag(r, Rose, k);
            UIKit.Label(new Rect(r.x + 16f, r.y + 6f, w - 20f, 32f), hover.Name, nameStyle);
            if (hover.Note != null)
            {
                float x = r.x + 16f;
                if (hover.Travel) x += UIKit.Prompt(x, r.y + 38f, 26f, PromptAction.Confirm, kb, k) + 8f;
                UIKit.Label(new Rect(x, r.y + 36f, r.xMax - x - 6f, 30f), hover.Note, noteStyle);
            }
            return hover;
        }

        // The sidebar: the level's name, where you are, what's left, the key, and the controls.
        void DrawSidebar(Rect sb, LevelInfo level, float k, bool kb, Target hover)
        {
            UIKit.Panel(sb, new Color(0.99f, 0.95f, 0.88f, 0.97f * k), new Color(Rose.r, Rose.g, Rose.b, 0.8f * k), 22f);
            float px = sb.x + 30f, pw = sb.width - 60f, y = sb.y + 26f;
            var head = UIKit.Style(22, true, TextAnchor.MiddleLeft, new Color(0.85f, 0.3f, 0.3f, k));
            var body = UIKit.Style(19, true, TextAnchor.MiddleLeft, new Color(Ink.r, Ink.g, Ink.b, k));
            var small = UIKit.Style(16, false, TextAnchor.UpperLeft, new Color(Ink.r, Ink.g, Ink.b, 0.85f * k), true);

            // The title, on a ribbon.
            var titleStyle = UIKit.Style(Mathf.RoundToInt(Mathf.Clamp(pw / Mathf.Max(8, level.MapTitle.Length) * 1.7f, 26f, 40f)), true, TextAnchor.MiddleCenter, new Color(1f, 0.97f, 0.9f, k));
            var ribbon = new Rect(sb.x - 14f, y, sb.width + 28f, 62f);
            UIKit.Fill(new Rect(ribbon.x + 3f, ribbon.y + 5f, ribbon.width, ribbon.height), new Color(0f, 0f, 0f, 0.25f * k), 8f);
            UIKit.Fill(ribbon, new Color(0.86f, 0.26f, 0.3f, k), 8f);
            RunningStitch(new Rect(ribbon.x + 8f, ribbon.y + 7f, ribbon.width - 16f, ribbon.height - 14f), new Color(1f, 0.95f, 0.85f, 0.9f * k));
            UIKit.Label(ribbon, level.MapTitle, titleStyle, true);
            y += 80f;

            // You are here.
            if (game.Player != null)
            {
                float dist;
                string where = RegionNear(game.Player.transform.position, out dist);
                if (where != null)
                {
                    UIKit.Label(new Rect(px, y, pw, 24f), "YOU ARE IN", UIKit.Style(15, true, TextAnchor.MiddleLeft, new Color(0.55f, 0.3f, 0.32f, k)));
                    UIKit.Label(new Rect(px, y + 22f, pw, 36f), where, UIKit.Style(28, true, TextAnchor.MiddleLeft, new Color(Ink.r, Ink.g, Ink.b, k)));
                    y += 66f;
                }
            }
            if (!string.IsNullOrEmpty(level.MapNote))
            {
                float nh = small.CalcHeight(new GUIContent(level.MapNote), pw);
                UIKit.Label(new Rect(px, y, pw, nh), level.MapNote, small);
                y += nh + 14f;
            }
            UIKit.Fill(new Rect(px, y, pw, 2f), new Color(Rose.r, Rose.g, Rose.b, 0.4f * k));
            y += 14f;

            // What's left to find, with little progress bars.
            UIKit.Label(new Rect(px, y, pw, 30f), "Found so far", head);
            y += 34f;
            if (level.HeartstringIds.Count > 0)
            {
                Progress(px, y, pw, level.HeartstringCount(), level.HeartstringIds.Count, "Heartstrings", new Color(1f, 0.6f, 0.35f), k, 0);
                y += 40f;
            }
            if (level.CollectibleIds.Count > 0)
            {
                Progress(px, y, pw, level.CollectedCount(), level.CollectibleIds.Count, "Golden Buttons", Gold, k, 1);
                y += 40f;
            }
            if (level.SecretIds.Count > 0)
            {
                Progress(px, y, pw, level.SecretCount(), level.SecretIds.Count, "Secret Marbles", new Color(0.55f, 0.75f, 1f), k, 2);
                y += 40f;
            }
            int bands = 0, bandGot = 0;
            foreach (var pin in level.MapPins)
                if (pin.Type == MapPin.Kind.Band) { bands++; if (GameState.IsCollected(pin.Id)) bandGot++; }
            if (bands > 0)
            {
                Progress(px, y, pw, bandGot, bands, "Town Band", new Color(0.75f, 0.6f, 0.95f), k, 3);
                y += 40f;
            }
            float share = ExploredShare();
            Progress(px, y, pw, Mathf.RoundToInt(share * 100f), 100, "Explored", new Color(0.45f, 0.7f, 0.55f), k, 4);
            y += 48f;
            UIKit.Fill(new Rect(px, y, pw, 2f), new Color(Rose.r, Rose.g, Rose.b, 0.4f * k));
            y += 14f;

            // The places, numbered as on the map (two columns).
            var places = Numbered(level);
            float controlsTop = sb.yMax - 150f;
            if (places.Count > 0 && y < controlsTop - 60f)
            {
                UIKit.Label(new Rect(px, y, pw, 30f), "Places", head);
                y += 34f;
                int perCol = (places.Count + 1) / 2;
                float colW = (pw - 12f) * 0.5f;
                float rowH = Mathf.Clamp((controlsTop - y - 10f) / Mathf.Max(1, perCol), 18f, 30f);
                var row = UIKit.Style(Mathf.RoundToInt(Mathf.Clamp(rowH * 0.58f, 12f, 17f)), true, TextAnchor.MiddleLeft, new Color(Ink.r, Ink.g, Ink.b, k));
                var rowDim = UIKit.Style(row.fontSize, true, TextAnchor.MiddleLeft, new Color(Ink.r, Ink.g, Ink.b, 0.45f * k));
                for (int i = 0; i < places.Count; i++)
                {
                    int col = i / perCol, r = i % perCol;
                    var strip = new Rect(px + col * (colW + 12f), y + r * rowH, colW, rowH - 3f);
                    bool known = Explored(ToUV(places[i].Position));
                    bool lit = hover != null && hover.Name == places[i].Name;
                    UIKit.Fill(strip, lit ? new Color(1f, 0.88f, 0.86f, k) : new Color(1f, 0.99f, 0.95f, k), 6f);
                    float bd = Mathf.Min(22f, rowH - 4f);
                    Badge(new Vector2(strip.x + bd * 0.5f + 3f, strip.center.y), bd, !known ? new Color(0.62f, 0.58f, 0.66f) : places[i].Type == MapPin.Kind.Shop ? ShopBlue : PlaceRed, known ? (i + 1).ToString() : "?", k);
                    UIKit.Label(new Rect(strip.x + bd + 10f, strip.y, strip.width - bd - 12f, strip.height), known ? places[i].Name : "???", known ? row : rowDim);
                }
            }

            // The key and the controls, at the bottom.
            float cy = sb.yMax - 140f;
            UIKit.Fill(new Rect(px, cy - 10f, pw, 2f), new Color(Rose.r, Rose.g, Rose.b, 0.4f * k));
            var keyStyle = UIKit.Style(15, true, TextAnchor.MiddleLeft, new Color(Ink.r, Ink.g, Ink.b, k));
            float kx = px;
            UIKit.Circle(new Vector2(kx + 10f, cy + 12f), 22f, new Color(0.45f, 0.62f, 0.9f, k));
            UIKit.Icon(new Rect(kx + 2f, cy + 4f, 16f, 16f), podTex, new Color(1f, 1f, 1f, k));
            UIKit.Label(new Rect(kx + 26f, cy, 60f, 24f), "Pod", keyStyle);
            kx += 82f;
            UIKit.Circle(new Vector2(kx + 10f, cy + 12f), 22f, new Color(0.98f, 0.92f, 0.78f, k));
            UIKit.Icon(new Rect(kx + 2f, cy + 3f, 17f, 17f), flagTex, new Color(Gold.r * 0.95f, Gold.g * 0.7f, 0.15f, k));
            UIKit.Label(new Rect(kx + 26f, cy, 110f, 24f), "Checkpoint", keyStyle);
            kx += 136f;
            UIKit.Circle(new Vector2(kx + 10f, cy + 12f), 22f, PlaceRed * new Color(1f, 1f, 1f, k));
            UIKit.Label(new Rect(kx + 26f, cy, 60f, 24f), "Place", keyStyle);
            kx += 84f;
            if (kx + 80f < px + pw)
            {
                UIKit.Circle(new Vector2(kx + 10f, cy + 12f), 22f, ShopBlue * new Color(1f, 1f, 1f, k));
                UIKit.Label(new Rect(kx + 26f, cy, 80f, 24f), "Special", keyStyle);
            }
            cy += 36f;
            float h = 30f;
            float x0 = px;
            x0 += UIKit.PromptLabel(x0, cy, h, PromptAction.Rotate, "Zoom", kb, Ink, k) + 22f;
            x0 += UIKit.PromptLabel(x0, cy, h, PromptAction.Sticker, "Find Patch", kb, Ink, k) + 22f;
            cy += 40f;
            x0 = px;
            x0 += UIKit.PromptLabel(x0, cy, h, PromptAction.Confirm, "Travel", kb, Ink, k) + 22f;
            UIKit.PromptLabel(x0, cy, h, PromptAction.Map, "Close", kb, Ink, k);
        }

        // One row of the "found so far" list: an icon, the name, a stitched bar and the count.
        void Progress(float x, float y, float w, int got, int of, string label, Color col, float k, int icon)
        {
            var c = new Color(col.r, col.g, col.b, k);
            Vector2 ic = new Vector2(x + 14f, y + 14f);
            switch (icon)
            {
                case 0: UIKit.Heart(ic, 24f, c); break;
                case 1: UIKit.Icon(new Rect(ic.x - 13f, ic.y - 13f, 26f, 26f), PatchworkAssets.IconButton, new Color(1f, 1f, 1f, k)); break;
                case 2: UIKit.Circle(ic, 22f, c); UIKit.Circle(ic + new Vector2(-4f, -4f), 7f, new Color(1f, 1f, 1f, 0.8f * k)); break;
                case 3: Button(ic, 24f, c, k); break;
                default: UIKit.IconTransformed(new Rect(ic.x - 11f, ic.y - 11f, 22f, 22f), starTex, c, 0f, 1f); break;
            }
            var style = UIKit.Style(17, true, TextAnchor.MiddleLeft, new Color(Ink.r, Ink.g, Ink.b, k));
            UIKit.Label(new Rect(x + 34f, y, w * 0.45f, 28f), label, style);
            string count = icon == 4 ? got + "%" : got + " / " + of;
            var cs = UIKit.Style(17, true, TextAnchor.MiddleRight, new Color(Ink.r, Ink.g, Ink.b, k));
            UIKit.Label(new Rect(x + w - 80f, y, 80f, 28f), count, cs);
            var bar = new Rect(x + 34f + w * 0.45f, y + 9f, w - 34f - w * 0.45f - 90f, 11f);
            if (bar.width > 20f)
            {
                UIKit.Fill(bar, new Color(0.3f, 0.2f, 0.2f, 0.12f * k), 5f);
                float f = of > 0 ? Mathf.Clamp01(got / (float)of) : 0f;
                if (f > 0f) UIKit.Fill(new Rect(bar.x, bar.y, Mathf.Max(10f, bar.width * f), bar.height), c, 5f);
                if (f >= 1f) UIKit.IconTransformed(new Rect(bar.xMax - 9f, bar.y - 5f, 20f, 20f), starTex, new Color(1f, 1f, 1f, k), 0f, 1f);
            }
        }
    }
}

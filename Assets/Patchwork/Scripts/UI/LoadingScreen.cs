using UnityEngine;
using UnityEngine.SceneManagement;

namespace Patchwork
{
    // The between-levels screen. Quilt patches pop in across the screen, the level's name gets
    // embroidered across the middle by a needle while a ball of yarn rolls along the bottom as the
    // loading bar. When the level is ready the patches pop away again to reveal it.
    // It lives across the scene change (DontDestroyOnLoad) and removes itself when done.
    public class LoadingScreen : MonoBehaviour
    {
        static LoadingScreen instance;

        // True from the moment a load starts until the new level is fully revealed.
        public static bool Busy { get { return instance != null; } }
        // True while the screen is completely covered (the new level shouldn't show its own fade).
        public static bool Covered { get { return instance != null && instance.cover >= 0.999f; } }

        enum Phase { CoverIn, Loading, Activating, Reveal }

        const float CoverTime = 0.6f;
        const float RevealTime = 0.7f;
        const float MinShowTime = 3.2f;      // long enough to read the card (and enjoy the stitching)
        const float ReloadShowTime = 1.2f;   // restarting the same level is quicker

        Phase phase;
        float cover;          // 0 = nothing covered, 1 = fully covered
        float shownTime;      // time the card has been fully up
        float display;        // smoothed progress for the yarn line
        float minTime;
        AsyncOperation op;
        string scene;
        LevelEntry entry;
        string tip;
        int framesAfterLoad;
        Color[] patchColors;

        public static void ResetStatics() { instance = null; }

        public static void Begin(string sceneName)
        {
            if (instance != null) return;
            var go = new GameObject("Loading Screen");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<LoadingScreen>();
            instance.Setup(sceneName);
        }

        void Setup(string sceneName)
        {
            scene = sceneName;
            entry = LevelCatalog.ForScene(sceneName);
            tip = LevelCatalog.Tips[Random.Range(0, LevelCatalog.Tips.Length)];
            minTime = SceneManager.GetActiveScene().name == sceneName ? ReloadShowTime : MinShowTime;
            phase = Phase.CoverIn;
            useGUILayout = false;

            // A quilt of shades around the level's colour.
            Color[] mix = { LevelKit.Palette.Cream, LevelKit.Palette.Butter, LevelKit.Palette.Peach, LevelKit.Palette.Lavender, LevelKit.Palette.Sage };
            patchColors = new Color[24];
            for (int i = 0; i < patchColors.Length; i++)
            {
                Color c = Color.Lerp(entry.Tint, mix[i % mix.Length], 0.25f + 0.2f * ((i * 7) % 3));
                c *= 0.92f + 0.08f * ((i * 5) % 3);
                c.a = 1f;
                patchColors[i] = c;
            }
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        void Update()
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            switch (phase)
            {
                case Phase.CoverIn:
                    cover = Mathf.MoveTowards(cover, 1f, dt / CoverTime);
                    if (cover >= 1f)
                    {
                        Time.timeScale = 1f;
                        op = SceneManager.LoadSceneAsync(scene);
                        if (op == null) { Destroy(gameObject); return; }
                        op.allowSceneActivation = false;
                        phase = Phase.Loading;
                    }
                    break;

                case Phase.Loading:
                    shownTime += dt;
                    float real = Mathf.Clamp01(op.progress / 0.9f);
                    float target = Mathf.Min(real, shownTime / minTime);
                    display = Mathf.MoveTowards(display, target, dt * 1.5f);
                    if (op.progress >= 0.9f && shownTime >= minTime && display >= 0.999f)
                    {
                        op.allowSceneActivation = true;
                        phase = Phase.Activating;
                    }
                    break;

                case Phase.Activating:
                    shownTime += dt;
                    // Wait until the new level has built itself, plus a couple of frames for it to settle.
                    if (op.isDone && PatchworkGame.Instance != null)
                    {
                        framesAfterLoad++;
                        if (framesAfterLoad > 2) phase = Phase.Reveal;
                    }
                    break;

                case Phase.Reveal:
                    cover = Mathf.MoveTowards(cover, 0f, dt / RevealTime);
                    if (cover <= 0f) Destroy(gameObject);
                    break;
            }
        }

        // ------------------------------------------------------------------ Drawing

        void OnGUI()
        {
            if (Event.current.type != EventType.Repaint) return;
            GUI.depth = -1000; // on top of everything
            UIKit.Begin();
            float W = UIKit.Width, H = UIKit.Height;

            DrawPatches(W, H);

            // The card only shows once the patches have covered the screen.
            float content = phase == Phase.CoverIn ? 0f : phase == Phase.Reveal ? Mathf.Clamp01((cover - 0.6f) / 0.4f) : Mathf.Clamp01(shownTime / 0.35f);
            if (content > 0.001f) DrawCard(W, H, content);
        }

        // Quilt squares pop in along a diagonal (and pop away the same way, the other direction).
        void DrawPatches(float W, float H)
        {
            const int rows = 7;
            float cell = H / rows;
            int cols = Mathf.CeilToInt(W / cell) + 1;
            float maxIndex = rows + cols;
            bool revealing = phase == Phase.Reveal;
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    float order = (revealing ? (r + c) : (maxIndex - (r + c))) / maxIndex;
                    // Each patch grows during its own slice of the cover amount.
                    float k = Mathf.Clamp01((cover * 1.6f - order * 0.6f));
                    if (k <= 0f) continue;
                    float s = revealing ? Mathf.SmoothStep(0f, 1f, k) : UIKit.EaseOutBack(k);
                    float size = (cell + 2f) * s;
                    var rect = new Rect(c * cell + (cell - size) * 0.5f, r * cell + (cell - size) * 0.5f, size, size);
                    Color col = patchColors[(r * 5 + c * 3) % patchColors.Length];
                    UIKit.Fill(rect, col, 12f * s);
                    if (s > 0.6f)
                        UIKit.Stitches(new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, rect.height - 16f),
                            new Color(1f, 0.97f, 0.9f, 0.55f * Mathf.Clamp01((s - 0.6f) / 0.4f)));
                }
        }

        void DrawCard(float W, float H, float a)
        {
            var ink = UIKit.Ink;
            var card = new Rect((W - 1180f) * 0.5f, H * 0.17f, 1180f, 560f);
            UIKit.Panel(card, new Color(0.99f, 0.96f, 0.9f, 0.95f * a), new Color(entry.Tint.r * 0.7f, entry.Tint.g * 0.7f, entry.Tint.b * 0.7f, 0.9f * a));

            // "Level 1" tag.
            if (!string.IsNullOrEmpty(entry.Number))
            {
                var tagStyle = UIKit.Style(24, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, a));
                float tw = UIKit.TextWidth(entry.Number, tagStyle) + 48f;
                var tag = new Rect(card.center.x - tw * 0.5f, card.y - 22f, tw, 46f);
                UIKit.Fill(tag, new Color(entry.Tint.r * 0.75f, entry.Tint.g * 0.75f, entry.Tint.b * 0.75f, a), 23f);
                UIKit.Label(tag, entry.Number, tagStyle);
            }

            // The title is "embroidered" left to right, with a needle leading the thread.
            var titleStyle = UIKit.Style(64, true, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, a));
            float titleW = UIKit.TextWidth(entry.Title, titleStyle);
            float tx = card.center.x - titleW * 0.5f, ty = card.y + 60f;
            float sew = Mathf.Clamp01(shownTime / 1.4f);
            float shown = titleW * sew;
            GUI.BeginGroup(new Rect(tx, ty - 10f, shown + 2f, 110f));
            GUI.Label(new Rect(0f, 10f, titleW + 10f, 90f), entry.Title, titleStyle);
            GUI.EndGroup();
            // Running stitch under the title.
            Color thread = new Color(entry.Tint.r * 0.8f, entry.Tint.g * 0.8f, entry.Tint.b * 0.8f, a);
            for (float x = 0f; x < shown; x += 26f)
                UIKit.Fill(new Rect(tx + x, ty + 100f, Mathf.Min(16f, shown - x), 4f), thread, 2f);
            if (sew < 1f) DrawNeedle(new Vector2(tx + shown, ty + 55f), a);

            // Blurb.
            var blurbStyle = UIKit.Style(26, false, TextAnchor.UpperCenter, new Color(ink.r, ink.g, ink.b, 0.85f * a), true);
            UIKit.Label(new Rect(card.x + 90f, card.y + 180f, card.width - 180f, 100f), entry.Blurb, blurbStyle);

            // Golden buttons and best threads.
            float rowY = card.y + 300f;
            if (entry.Collectibles.Length > 0)
            {
                int found = LevelCatalog.FoundButtons(entry);
                float total = entry.Collectibles.Length * 70f;
                float bx = card.center.x - total * 0.5f;
                for (int i = 0; i < entry.Collectibles.Length; i++)
                {
                    var c = new Vector2(bx + 35f + i * 70f, rowY + 30f);
                    if (i < found)
                    {
                        UIKit.Circle(c, 56f, new Color(1f, 0.8f, 0.25f, a));
                        UIKit.CircleOutline(c, 56f, new Color(0.75f, 0.55f, 0.15f, a), 3f);
                        for (int h = 0; h < 4; h++)
                            UIKit.Circle(c + new Vector2(h % 2 == 0 ? -7f : 7f, h < 2 ? -7f : 7f), 8f, new Color(0.6f, 0.42f, 0.1f, a));
                    }
                    else UIKit.CircleOutline(c, 56f, new Color(ink.r, ink.g, ink.b, 0.35f * a), 4f);
                }
                int threadTotal;
                int best = GameState.BestThreads(entry.Scene, out threadTotal);
                string line = "Golden buttons " + found + " / " + entry.Collectibles.Length +
                              (threadTotal > 0 ? "      Best threads " + best + " / " + threadTotal : "");
                UIKit.Label(new Rect(card.x, rowY + 70f, card.width, 36f), line, UIKit.Style(24, true, TextAnchor.MiddleCenter, new Color(ink.r, ink.g, ink.b, 0.8f * a)));
            }

            // Tip.
            var tipRect = new Rect(card.x + 120f, card.yMax - 120f, card.width - 240f, 80f);
            UIKit.Fill(tipRect, new Color(entry.Tint.r, entry.Tint.g, entry.Tint.b, 0.22f * a), 18f);
            UIKit.Label(new Rect(tipRect.x + 24f, tipRect.y, tipRect.width - 48f, tipRect.height), "Tip: " + tip,
                UIKit.Style(22, false, TextAnchor.MiddleCenter, new Color(ink.r, ink.g, ink.b, 0.9f * a), true));

            DrawYarnBar(W, H, a);
        }

        // A little silver needle with an eye, pointing right.
        void DrawNeedle(Vector2 tip, float a)
        {
            float bob = Mathf.Sin(Time.unscaledTime * 18f) * 6f;
            Vector2 p = tip + new Vector2(0f, bob);
            UIKit.Fill(new Rect(p.x - 70f, p.y - 4f, 70f, 8f), new Color(0.82f, 0.84f, 0.9f, a), 4f);
            UIKit.Fill(new Rect(p.x - 8f, p.y - 2f, 14f, 4f), new Color(0.9f, 0.92f, 0.96f, a), 2f);
            UIKit.CircleOutline(new Vector2(p.x - 60f, p.y), 10f, new Color(0.5f, 0.52f, 0.6f, a), 2f);
            // Thread trailing from the eye back to the stitches.
            UIKit.Fill(new Rect(p.x - 100f, p.y + 2f, 40f, 3f), new Color(entry.Tint.r * 0.8f, entry.Tint.g * 0.8f, entry.Tint.b * 0.8f, a), 1.5f);
        }

        // The loading bar: a ball of yarn rolling to the right, unrolling a strand behind it.
        void DrawYarnBar(float W, float H, float a)
        {
            float x0 = W * 0.2f, x1 = W * 0.8f, y = H - 120f;
            float k = phase == Phase.CoverIn ? 0f : phase == Phase.Loading ? display : 1f;
            float ball = 70f * (1f - 0.35f * k);  // the ball gets smaller as it unrolls
            float x = Mathf.Lerp(x0, x1, k);
            UIKit.Fill(new Rect(x0, y - 2f, x1 - x0, 4f), new Color(1f, 1f, 1f, 0.25f * a), 2f);
            UIKit.Fill(new Rect(x0, y - 3f, x - x0, 6f), new Color(entry.Tint.r * 0.75f, entry.Tint.g * 0.75f, entry.Tint.b * 0.75f, a), 3f);
            var r = new Rect(x - ball * 0.5f, y - ball + 6f, ball, ball);
            float angle = (x - x0) / (ball * 0.5f) * Mathf.Rad2Deg;
            UIKit.IconTransformed(r, PatchworkAssets.IconYarn, new Color(1f, 1f, 1f, a), angle, 1f);
            string dots = new string('.', 1 + Mathf.FloorToInt(Time.unscaledTime * 2.5f) % 3);
            UIKit.Label(new Rect(x0, y + 16f, x1 - x0, 40f), (k >= 1f ? "Ready" : "Stitching") + dots,
                UIKit.Style(24, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, a)), true);
        }
    }
}

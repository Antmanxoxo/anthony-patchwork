using UnityEngine;

namespace Patchwork
{
    // On-screen HUD: current form badge with ability hints, golden buttons, control reminders,
    // tutorial cards, toasts, the anchor prompt, plus whichever menu is open.
    public class PatchworkHUD : MonoBehaviour
    {
        public PatchworkGame Game;

        // Form panel auto-hide
        const float BadgeShowTime = 4.5f;
        float badgeTimer = BadgeShowTime;
        float badgeSlide;
        FormType lastForm;
        bool firstFrame = true;

        void Update()
        {
            if (Game == null || Game.Player == null) return;
            float dt = Time.unscaledDeltaTime;
            var forms = Game.Player.Forms;
            if (firstFrame) { lastForm = forms.Current; firstFrame = false; }
            if (forms.Current != lastForm) { lastForm = forms.Current; badgeTimer = BadgeShowTime; }

            // Keep it up while it's telling you something useful right now.
            bool relevant = forms.HasPending || Game.Mode == UIMode.FormWheel;
            if (forms.Current == FormType.Yarn)
            {
                var yarn = forms.Get<YarnAbility>();
                relevant |= yarn.Attached || yarn.Target != null;
            }
            else if (forms.Current == FormType.Wind)
            {
                var wind = forms.Get<WindAbility>();
                relevant |= wind.Gliding || !wind.GustReady;
            }
            if (relevant) badgeTimer = Mathf.Max(badgeTimer, 1.5f);

            threadPop = Mathf.MoveTowards(threadPop, 0f, dt * 2f);
            badgeTimer -= dt;
            float target = badgeTimer > 0f ? 1f : 0f;
            badgeSlide = Mathf.MoveTowards(badgeSlide, target, dt * (target > badgeSlide ? 4f : 2.5f));
        }

        // Pop the form panel back up (e.g. after a gust or when the Satchel closes).
        public void PokeBadge()
        {
            badgeTimer = BadgeShowTime;
        }

        void Awake()
        {
            useGUILayout = false; // everything is drawn with fixed rects, so skip the layout pass
        }

        void OnGUI()
        {
            // All input goes through the Input System, so only the draw event matters here.
            if (Event.current.type != EventType.Repaint) return;
            if (Game == null || Game.Player == null) return;
            UIKit.Begin();
            bool kb = Game.Input.UsingKeyboard;

            if (Game.Mode == UIMode.Intro)
            {
                Game.Title.Draw(kb);
                return;
            }

            DrawLetterbox(kb);
            if (Game.Mode != UIMode.Loading && Game.Mode != UIMode.Warping && Game.Mode != UIMode.Dance && Game.Mode != UIMode.LevelSelect)
            {
                DrawHelmPrompt(kb);
                DrawAnchorPrompt(kb);
                DrawFormBadge(kb);
                DrawButtons();
                DrawControls(kb);
                DrawHintCard();
                DrawToast();
                DrawTitle();
                DrawMeter();
            }

            switch (Game.Mode)
            {
                case UIMode.FormWheel: Game.Selector.Draw(); break;
                case UIMode.Satchel: Game.Satchel.Draw(); break;
                case UIMode.Paused: Game.Pause.Draw(); break;
                case UIMode.Complete: DrawComplete(kb); break;
                case UIMode.LevelSelect: Game.LevelSelect.Draw(kb); break;
            }

            if (Game.Fade > 0.001f)
            {
                Color fc = Game.FadeColor;
                UIKit.Fill(new Rect(0f, 0f, UIKit.Width, UIKit.Height), new Color(fc.r, fc.g, fc.b, Game.Fade));
            }
        }

        // Dance easter egg: cinema bars slide in, with a little "keep tapping" note at the bottom.
        void DrawLetterbox(bool kb)
        {
            float k = Game.CameraRig != null ? Game.CameraRig.CutsceneBlend : 0f;
            if (k <= 0.001f) return;
            float e = Mathf.SmoothStep(0f, 1f, k);
            float bar = 110f * e;
            var black = new Color(0.06f, 0.04f, 0.08f, 1f);
            UIKit.Fill(new Rect(0f, 0f, UIKit.Width, bar), black);
            UIKit.Fill(new Rect(0f, UIKit.Height - bar, UIKit.Width, bar), black);
            if (Game.Mode != UIMode.Dance) return;

            float t = Time.unscaledTime;
            string title;
            switch (Game.Dance.Form)
            {
                case FormType.Water: title = "Bubble Calypso"; break;
                case FormType.Wind: title = "Breezy Waltz"; break;
                default: title = "Yarnball Hoedown"; break;
            }
            float bob = Mathf.Sin(Game.Dance.Beat * Mathf.PI * 2f) * 3f;
            UIKit.Label(new Rect(0f, bar - 86f + bob, UIKit.Width, 60f), title,
                UIKit.Style(40, true, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.6f, e)));
            UIKit.Label(new Rect(0f, UIKit.Height - bar + 30f, UIKit.Width, 50f),
                kb ? "Keep tapping 1 / 2 / 3 to keep dancing!" : "Keep tapping the D-pad to keep dancing!",
                UIKit.Style(24, false, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.85f * e * (0.75f + 0.25f * Mathf.Sin(t * 4f)))));
        }

        void DrawFormBadge(bool kb)
        {
            var forms = Game.Player.Forms;
            var def = forms.Definition;
            var ink = UIKit.Ink;
            if (badgeSlide <= 0.001f) return;
            float slide = badgeSlide < 1f ? (target01(badgeSlide)) : 1f;
            var panel = new Rect(28f - (1f - slide) * 720f, UIKit.Height - 206f, 640f, 178f);
            UIKit.Panel(panel, UIKit.Paper, UIKit.Thread);

            var iconCenter = new Vector2(panel.x + 92f, panel.y + 89f);
            UIKit.Circle(iconCenter, 132f, def.PrimaryColor);
            UIKit.CircleOutline(iconCenter, 116f, new Color(1f, 1f, 1f, 0.85f), 3f);
            UIKit.Icon(new Rect(iconCenter.x - 42f, iconCenter.y - 42f, 84f, 84f), PatchworkAssets.IconFor(forms.Current), Color.white);

            float x = panel.x + 176f;
            UIKit.Label(new Rect(x, panel.y + 20f, 440f, 40f), def.DisplayName, UIKit.Style(32, true, TextAnchor.MiddleLeft, ink));

            float y = panel.y + 70f;
            float h = 40f;
            switch (forms.Current)
            {
                case FormType.Water:
                    UIKit.Label(new Rect(x, y, 440f, 90f), "Slip through blue drain grates.\nRide blue fountains upward.",
                        UIKit.Style(23, false, TextAnchor.UpperLeft, ink, true));
                    break;

                case FormType.Yarn:
                {
                    var yarn = forms.Get<YarnAbility>();
                    if (yarn.Attached)
                    {
                        UIKit.PromptLabel(x, y, h, PromptAction.Ability, GameSettings.SwingToggle ? "Tap to launch" : "Release to launch", kb, ink);
                        UIKit.PromptLabel(x, y + 48f, h, PromptAction.Jump, "Jump off", kb, ink);
                    }
                    else
                    {
                        bool ready = yarn.Target != null;
                        UIKit.PromptLabel(x, y, h, PromptAction.Ability, ready ? (GameSettings.SwingToggle ? "Tap to swing!" : "Hold to swing!")
                            : (GameSettings.SwingToggle ? "Tap near a pink anchor to swing" : "Hold near a pink anchor to swing"), kb, ink, ready ? 1f : 0.55f);
                    }
                    break;
                }

                case FormType.Wind:
                {
                    var wind = forms.Get<WindAbility>();
                    UIKit.PromptLabel(x, y, h, PromptAction.Jump, wind.Gliding ? "Gliding..." : "Hold in the air to glide", kb, ink);
                    UIKit.PromptLabel(x, y + 48f, h, PromptAction.Ability, wind.GustReady ? "Gust" : "Gust (land to recharge)", kb, ink, wind.GustReady ? 1f : 0.45f);
                    break;
                }
            }

            if (forms.HasPending)
            {
                var r = new Rect(panel.x + 20f, panel.y - 46f, 600f, 38f);
                UIKit.Fill(r, new Color(0.13f, 0.11f, 0.19f, 0.85f), 14f);
                UIKit.Label(r, "Becoming " + FormLibrary.Get(forms.Pending).DisplayName + " after you let go...",
                    UIKit.Style(18, true, TextAnchor.MiddleCenter, Color.white));
            }
        }

        static float target01(float t)
        {
            return UIKit.EaseOutBack(t);
        }

        float threadPop;

        // Little bounce on the thread counter when a spool is picked up.
        public void PokeThreads()
        {
            threadPop = 1f;
        }

        void DrawButtons()
        {
            var level = Game.Level;
            int total = level.CollectibleIds.Count;
            float y = 24f;
            if (total > 0)
            {
                var r = new Rect(28f, y, 230f, 84f);
                UIKit.Panel(r, UIKit.Paper, UIKit.Thread, 20f);
                UIKit.Icon(new Rect(r.x + 18f, r.y + 14f, 56f, 56f), PatchworkAssets.IconButton, Color.white);
                UIKit.Label(new Rect(r.x + 88f, r.y + 14f, 130f, 56f), level.CollectedCount() + " / " + total,
                    UIKit.Style(34, true, TextAnchor.MiddleLeft, UIKit.Ink));
                y += 96f;
            }
            if (level.ThreadTotal > 0)
            {
                float pop = 1f + Mathf.Sin(threadPop * Mathf.PI) * 0.15f;
                var r = new Rect(28f, y, 230f * pop, 70f * pop);
                UIKit.Panel(r, new Color(1f, 0.93f, 0.95f, 0.94f), new Color(0.85f, 0.4f, 0.55f, 0.9f), 20f);
                UIKit.IconTransformed(new Rect(r.x + 18f, r.y + 11f, 48f, 48f), PatchworkAssets.IconYarn, Color.white, threadPop * 360f, 1f);
                UIKit.Label(new Rect(r.x + 80f, r.y + 10f, 140f, 50f), Game.ThreadsCollected + " / " + level.ThreadTotal,
                    UIKit.Style(28, true, TextAnchor.MiddleLeft, new Color(0.5f, 0.15f, 0.3f)));
            }
        }

        void DrawControls(bool kb)
        {
            if (Game.Mode != UIMode.Gameplay) return;
            float h = 36f;
            float y = UIKit.Height - 64f;
            var c = new Color(1f, 1f, 1f, 0.95f);
            // Measure right-to-left so the strip hugs the right edge.
            string[] labels = { "Form Wheel", "Satchel", "Pause" };
            PromptAction[] actions = { PromptAction.FormWheel, PromptAction.Satchel, PromptAction.Pause };
            float total = 0f;
            var style = UIKit.Style(Mathf.RoundToInt(h * 0.55f), true, TextAnchor.MiddleLeft, c);
            for (int i = 0; i < labels.Length; i++)
                total += EstimatePromptWidth(actions[i], kb, h) + 10f + UIKit.TextWidth(labels[i], style) + 30f;
            var bg = new Rect(UIKit.Width - total - 50f, y - 12f, total + 30f, h + 24f);
            UIKit.Pill(bg);
            float x = bg.x + 16f;
            for (int i = 0; i < labels.Length; i++)
                x += UIKit.PromptLabel(x, y, h, actions[i], labels[i], kb, c) + 30f;
        }

        static float EstimatePromptWidth(PromptAction a, bool kb, float h)
        {
            string text;
            if (kb) text = a == PromptAction.FormWheel ? "Q" : a == PromptAction.Satchel ? "Tab" : "Esc";
            else text = a == PromptAction.FormWheel ? PatchworkInput.FormWheelPadLabel : a == PromptAction.Satchel ? "TOUCHPAD" : "OPTIONS";
            var s = UIKit.Style(Mathf.RoundToInt(h * 0.42f), true, TextAnchor.MiddleCenter, Color.white);
            return Mathf.Max(h, UIKit.TextWidth(text, s) + h * 0.55f);
        }

        void DrawHintCard()
        {
            var hint = Game.ActiveHint;
            if (hint == null || Game.Mode != UIMode.Gameplay) return;
            float age = Game.HintAge;
            float D = PatchworkGame.HintDuration;
            float tin = UIKit.EaseOutBack(age / 0.45f);
            float tout = age > D - 0.45f ? UIKit.EaseOutCubic((age - (D - 0.45f)) / 0.45f) : 0f;

            Color bg, pattern, stitch, title, iconBg;
            Texture2D pat, icon;
            switch (hint.Theme)
            {
                case HintTheme.Water:
                    bg = new Color(0.86f, 0.95f, 1f, 0.97f); pattern = new Color(0.45f, 0.7f, 0.95f, 0.28f);
                    stitch = new Color(0.35f, 0.6f, 0.9f, 0.9f); title = new Color(0.1f, 0.3f, 0.55f); iconBg = new Color(0.36f, 0.66f, 0.95f);
                    pat = UIArt.PatternWaves; icon = PatchworkAssets.IconWater;
                    break;
                case HintTheme.Yarn:
                    bg = new Color(1f, 0.92f, 0.94f, 0.97f); pattern = new Color(0.93f, 0.45f, 0.58f, 0.2f);
                    stitch = new Color(0.85f, 0.4f, 0.55f, 0.9f); title = new Color(0.5f, 0.15f, 0.3f); iconBg = new Color(0.93f, 0.45f, 0.58f);
                    pat = UIArt.PatternKnit; icon = PatchworkAssets.IconYarn;
                    break;
                case HintTheme.Wind:
                    bg = new Color(0.93f, 0.99f, 0.97f, 0.97f); pattern = new Color(0.45f, 0.8f, 0.75f, 0.25f);
                    stitch = new Color(0.4f, 0.75f, 0.7f, 0.9f); title = new Color(0.12f, 0.4f, 0.38f); iconBg = new Color(0.55f, 0.85f, 0.8f);
                    pat = UIArt.PatternSwirl; icon = PatchworkAssets.IconWind;
                    break;
                default:
                    bg = UIKit.Paper; pattern = new Color(0.85f, 0.6f, 0.4f, 0.14f);
                    stitch = UIKit.Thread; title = UIKit.Ink; iconBg = new Color(1f, 0.8f, 0.3f);
                    pat = UIArt.PatternDots; icon = PatchworkAssets.IconButton;
                    break;
            }

            float w = 940f, h = 182f;
            float y = Mathf.LerpUnclamped(-h - 20f, 24f, tin) - tout * (h + 60f);
            var r = new Rect((UIKit.Width - w) * 0.5f, y, w, h);
            float t = Time.unscaledTime;

            UIKit.Fill(new Rect(r.x + 4f, r.y + 7f, r.width, r.height), new Color(0f, 0f, 0f, 0.18f), 24f);
            UIKit.Fill(r, bg, 24f);
            UIKit.PatternFill(new Rect(r.x + 16f, r.y + 16f, r.width - 32f, r.height - 32f), pat, pattern, 64f, new Vector2(t * 0.05f, t * 0.03f));

            // Theme decorations on the right edge.
            switch (hint.Theme)
            {
                case HintTheme.Water:
                    for (int i = 0; i < 4; i++)
                    {
                        float k = Mathf.Repeat(t * 0.35f + i * 0.25f, 1f);
                        float bx = r.xMax - 60f + Mathf.Sin(t * 2f + i) * 10f - i * 14f;
                        float by = r.yMax - 20f - k * (h - 30f);
                        float size = 10f + i * 4f;
                        UIKit.CircleOutline(new Vector2(bx, by), size, new Color(0.35f, 0.6f, 0.9f, 0.6f * (1f - k)), 2f);
                    }
                    break;
                case HintTheme.Wind:
                    for (int i = 0; i < 3; i++)
                    {
                        float k = Mathf.Repeat(t * 0.15f + i * 0.33f, 1f);
                        float cx = r.xMax - 150f + k * 120f;
                        float cy = r.y + 36f + i * 34f;
                        float a = Mathf.Sin(k * Mathf.PI) * 0.7f;
                        UIKit.Circle(new Vector2(cx, cy), 22f, new Color(1f, 1f, 1f, a));
                        UIKit.Circle(new Vector2(cx + 14f, cy - 4f), 28f, new Color(1f, 1f, 1f, a));
                        UIKit.Circle(new Vector2(cx + 30f, cy), 20f, new Color(1f, 1f, 1f, a));
                    }
                    break;
                case HintTheme.Yarn:
                {
                    // A dangling strand of yarn swinging from the top-right corner.
                    float sway = Mathf.Sin(t * 2f) * 10f;
                    for (int i = 0; i < 10; i++)
                    {
                        float k = i / 9f;
                        float sx = r.xMax - 70f + sway * k * k;
                        float sy = r.y + 8f + k * 70f;
                        UIKit.Circle(new Vector2(sx, sy), 7f, new Color(0.93f, 0.45f, 0.58f, 0.85f));
                    }
                    UIKit.Icon(new Rect(r.xMax - 70f + sway - 18f, r.y + 70f, 36f, 36f), PatchworkAssets.IconYarn, Color.white);
                    break;
                }
            }

            UIKit.Stitches(new Rect(r.x + 10f, r.y + 10f, r.width - 20f, r.height - 20f), stitch);

            // Icon medallion.
            float bob = Mathf.Sin(t * 3f) * 3f;
            var ic = new Vector2(r.x + 78f, r.y + h * 0.5f + bob);
            UIKit.Circle(ic, 92f, iconBg);
            UIKit.CircleOutline(ic, 80f, new Color(1f, 1f, 1f, 0.85f), 3f);
            UIKit.Icon(new Rect(ic.x - 30f, ic.y - 30f, 60f, 60f), icon, Color.white);

            float tx = r.x + 140f;
            float tw = w - 140f - (hint.Theme == HintTheme.Neutral ? 40f : 110f);
            UIKit.Label(new Rect(tx, r.y + 22f, tw, 40f), hint.Title, UIKit.Style(29, true, TextAnchor.MiddleLeft, title));
            UIKit.RichText(new Rect(tx, r.y + 64f, tw, 104f), hint.Body, 24, false,
                new Color(title.r * 0.6f + 0.1f, title.g * 0.6f + 0.1f, title.b * 0.6f + 0.1f, 0.95f), Game.Input.UsingKeyboard);

            // Thread "timer" along the bottom: full while you're in the zone, runs down once you leave.
            float remain = Game.HintHeld ? 1f : Mathf.Clamp01((D - 0.45f - age) / 1.5f);
            UIKit.Fill(new Rect(r.x + 24f, r.yMax - 8f, (r.width - 48f) * remain, 3f), new Color(stitch.r, stitch.g, stitch.b, 0.5f), 1.5f);
        }

        void DrawToast()
        {
            if (Game.ToastTimer <= 0f || string.IsNullOrEmpty(Game.ToastText)) return;
            float a = Mathf.Clamp01(Game.ToastTimer / 0.4f);
            var style = UIKit.Style(24, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, a));
            float w = UIKit.TextWidth(Game.ToastText, style) + 60f;
            var r = new Rect((UIKit.Width - w) * 0.5f, UIKit.Height * 0.68f, w, 54f);
            UIKit.Fill(r, new Color(0.13f, 0.11f, 0.19f, 0.82f * a), 27f);
            UIKit.Label(r, Game.ToastText, style);
        }

        // Test Gym readout: last jump's distance and height, and the best distance so far.
        void DrawMeter()
        {
            var m = Game.Meter;
            if (m == null) return;
            var r = new Rect(UIKit.Width - 420f, 28f, 390f, 132f);
            UIKit.Panel(r, UIKit.Paper, UIKit.Thread);
            var ink = UIKit.Ink;
            string form = Game.Player.Forms.Definition.DisplayName;
            UIKit.Label(new Rect(r.x + 26f, r.y + 16f, r.width - 52f, 30f), form, UIKit.Style(24, true, TextAnchor.MiddleLeft, ink));
            string last = m.HasJump ? m.LastDistance.ToString("0.0") + " m far,  " + m.LastHeight.ToString("0.0") + " m high" : "Jump to measure";
            UIKit.Label(new Rect(r.x + 26f, r.y + 52f, r.width - 52f, 30f), last, UIKit.Style(22, false, TextAnchor.MiddleLeft, ink));
            UIKit.Label(new Rect(r.x + 26f, r.y + 86f, r.width - 52f, 30f), "Best: " + m.BestDistance.ToString("0.0") + " m",
                UIKit.Style(22, false, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, 0.75f)));
        }

        void DrawTitle()
        {
            if (Game.TitleTimer <= 0f) return;
            float a = Mathf.Clamp01(Game.TitleTimer / 1f) * Mathf.Clamp01((4.5f - Game.TitleTimer) / 0.6f);
            var title = UIKit.Style(54, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, a));
            var sub = UIKit.Style(24, false, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, a));
            float y = UIKit.Height * 0.3f;
            float tw = Mathf.Max(UIKit.TextWidth(Game.Level.Title, title), UIKit.TextWidth(Game.Level.Subtitle, sub)) + 90f;
            UIKit.Pill(new Rect((UIKit.Width - tw) * 0.5f, y - 14f, tw, 136f), a);
            UIKit.Label(new Rect(0f, y, UIKit.Width, 70f), Game.Level.Title, title, true);
            UIKit.Label(new Rect(0f, y + 66f, UIKit.Width, 40f), Game.Level.Subtitle, sub, true);
        }

        // Floating "Set course" prompt over the Skiff's helm while you're standing at it.
        void DrawHelmPrompt(bool kb)
        {
            if (Game.Mode != UIMode.Gameplay || Game.Helm == null) return;
            var cam = Game.CameraRig.Cam;
            Vector3 sp = cam.WorldToScreenPoint(Game.Helm.PromptPoint);
            if (sp.z <= 0f) return;
            float gx = sp.x / UIKit.Scale;
            float gy = (Screen.height - sp.y) / UIKit.Scale;
            float bob = Mathf.Sin(Time.unscaledTime * 4f) * 5f;
            var label = UIKit.Style(26, true, TextAnchor.MiddleLeft, Color.white);
            const string text = "Set course";
            float w = 56f + 12f + UIKit.TextWidth(text, label) + 36f;
            var r = new Rect(gx - w * 0.5f, gy - 34f + bob, w, 68f);
            UIKit.Pill(r);
            UIKit.Prompt(r.x + 14f, r.y + 6f, 56f, PromptAction.Ability, kb);
            UIKit.Label(new Rect(r.x + 82f, r.y, w - 82f, r.height), text, label);
        }

        void DrawAnchorPrompt(bool kb)
        {
            if (Game.Mode != UIMode.Gameplay || Game.Player.Forms.Current != FormType.Yarn) return;
            var yarn = Game.Player.Forms.Get<YarnAbility>();
            if (yarn == null || yarn.Attached || yarn.Target == null) return;
            var cam = Game.CameraRig.Cam;
            Vector3 sp = cam.WorldToScreenPoint(yarn.Target.Position + Vector3.up * 1.4f);
            if (sp.z <= 0f) return;
            float gx = sp.x / UIKit.Scale;
            float gy = (Screen.height - sp.y) / UIKit.Scale;
            float bob = Mathf.Sin(Time.unscaledTime * 6f) * 6f;
            float s = 64f;
            UIKit.Circle(new Vector2(gx, gy + bob), s + 18f, new Color(1f, 0.55f, 0.7f, 0.45f));
            UIKit.Prompt(gx - s * 0.5f, gy - s * 0.5f + bob, s, PromptAction.Ability, kb);
            var holdRect = new Rect(gx - 42f, gy + s * 0.5f + bob + 6f, 84f, 30f);
            UIKit.Pill(holdRect);
            UIKit.Label(holdRect, "HOLD", UIKit.Style(20, true, TextAnchor.MiddleCenter, Color.white));
        }

        void DrawComplete(bool kb)
        {
            float W = UIKit.Width, H = UIKit.Height;
            UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.12f, 0.08f, 0.18f, 0.6f));
            var panel = new Rect((W - 640f) * 0.5f, (H - 440f) * 0.5f, 640f, 440f);
            UIKit.Panel(panel, new Color(0.99f, 0.95f, 0.88f, 0.97f), UIKit.Thread);
            var ink = UIKit.Ink;
            UIKit.Label(new Rect(panel.x, panel.y + 40f, panel.width, 60f), "Level Complete!", UIKit.Style(48, true, TextAnchor.MiddleCenter, ink));
            UIKit.Label(new Rect(panel.x, panel.y + 100f, panel.width, 36f), Game.Level.Title, UIKit.Style(22, false, TextAnchor.MiddleCenter, ink));
            UIKit.Icon(new Rect(panel.x + 200f, panel.y + 150f, 90f, 90f), PatchworkAssets.IconButton, Color.white);
            UIKit.Label(new Rect(panel.x + 305f, panel.y + 150f, 200f, 90f),
                Game.Level.CollectedCount() + " / " + Game.Level.CollectibleIds.Count, UIKit.Style(44, true, TextAnchor.MiddleLeft, ink));
            if (Game.Level.ThreadTotal > 0)
            {
                UIKit.Icon(new Rect(panel.x + 210f, panel.y + 236f, 70f, 70f), PatchworkAssets.IconYarn, Color.white);
                UIKit.Label(new Rect(panel.x + 305f, panel.y + 236f, 260f, 70f),
                    Game.ThreadsCollected + " / " + Game.Level.ThreadTotal + " threads", UIKit.Style(30, true, TextAnchor.MiddleLeft, ink));
            }
            float x = panel.x + 150f;
            UIKit.PromptLabel(x, panel.yMax - 90f, 44f, PromptAction.Confirm, "Back to the Skiff", kb, ink);
        }
    }
}

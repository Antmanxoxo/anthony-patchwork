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

        // The HUD keeps out of the way: the counters slide in when something changes, or when
        // Patch stands still for a moment, then tuck away again.
        float stackA, scoreA, controlsA, heartsA;
        float stackTimer, scoreTimer, heartsTimer, idleTime;
        int lastSig = int.MinValue, lastScore = int.MinValue, lastHearts = -1;

        void UpdateVisibility(float dt)
        {
            var level = Game.Level;
            int sig = level.CollectedCount() * 131 + level.HeartstringCount() * 31 + level.SecretCount() * 1031 + Game.ThreadsCollected * 7919
                      + (level.Id == Level1Builder.SceneName ? Level1Director.BandCount() * 7 : 0);
            if (sig != lastSig) { if (lastSig != int.MinValue) stackTimer = 4f; lastSig = sig; }
            int score = Game.Score != null ? Game.Score.Total : 0;
            if (score != lastScore) { if (lastScore != int.MinValue) scoreTimer = 3f; lastScore = score; }
            if (Game.Hearts != lastHearts) { if (lastHearts >= 0) heartsTimer = 3.5f; lastHearts = Game.Hearts; }
            var m = Game.Player.Motor;
            bool still = new Vector2(m.Velocity.x, m.Velocity.z).sqrMagnitude < 0.09f && m.Grounded && Game.Mode == UIMode.Gameplay;
            idleTime = still ? idleTime + dt : 0f;
            bool idle = idleTime > 1.5f;
            bool menu = Game.Mode == UIMode.Paused || Game.Mode == UIMode.Satchel || Game.Mode == UIMode.Complete;
            stackTimer -= dt; scoreTimer -= dt; heartsTimer -= dt;
            stackA = Mathf.MoveTowards(stackA, stackTimer > 0f || idle || menu ? 1f : 0f, dt * 3f);
            scoreA = Mathf.MoveTowards(scoreA, scoreTimer > 0f || idle || menu || (Game.Score != null && Game.Score.ChainAlive && Game.Score.Multiplier > 1) ? 1f : 0f, dt * 3f);
            controlsA = Mathf.MoveTowards(controlsA, idleTime > 2.5f ? 1f : 0f, dt * 3f);
            bool hurt = Game.Hearts < PatchworkGame.MaxHearts;
            heartsA = Mathf.MoveTowards(heartsA, !level.IsHub && (hurt || heartsTimer > 0f || idle || menu) ? 1f : 0f, dt * 4f);
        }

        // Draws with everything shifted (to slide a panel on and off screen).
        void Shifted(float dx, float dy, System.Action draw)
        {
            var m = GUI.matrix;
            GUI.matrix = m * Matrix4x4.Translate(new Vector3(dx, dy, 0f));
            draw();
            GUI.matrix = m;
        }

        void DrawHearts()
        {
            if (heartsA <= 0.001f) return;
            float shake = Game.HeartsShake;
            var r = new Rect(28f, 24f, 214f, 70f);
            UIKit.Panel(r, new Color(1f, 0.95f, 0.93f, 0.95f), new Color(0.9f, 0.35f, 0.45f, 0.9f), 20f);
            for (int i = 0; i < PatchworkGame.MaxHearts; i++)
            {
                bool full = i < Game.Hearts;
                float wob = shake > 0f ? Mathf.Sin(Time.unscaledTime * 40f + i) * shake * 4f : 0f;
                var c = new Vector2(r.x + 46f + i * 62f + wob, r.y + 37f);
                UIKit.Heart(c + new Vector2(0f, 3f), 50f, new Color(0f, 0f, 0f, 0.15f));
                UIKit.Heart(c, 50f, full ? new Color(0.95f, 0.3f, 0.42f) : new Color(0.8f, 0.76f, 0.8f, 0.7f));
                if (full) UIKit.Circle(c + new Vector2(-8f, -9f), 9f, new Color(1f, 1f, 1f, 0.6f));
            }
        }

        void Update()
        {
            if (Game == null || Game.Player == null) return;
            float dt = Time.unscaledDeltaTime;
            UpdateVisibility(dt);
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
            if (Game.Mode == UIMode.Photo)
            {
                Game.Photo.Draw(kb);
                return;
            }

            DrawLetterbox(kb);
            if (Game.Mode != UIMode.Loading && Game.Mode != UIMode.Dance && Game.Mode != UIMode.LevelSelect && Game.Mode != UIMode.Talk && Game.Mode != UIMode.Cutscene)
            {
                DrawHelmPrompt(kb);
                DrawTalkPrompt(kb);
                DrawAnchorPrompt(kb);
                DrawFormBadge(kb);
                if (heartsA > 0.001f) Shifted(-(1f - UIKit.EaseOutCubic(heartsA)) * 280f, 0f, DrawHearts);
                if (stackA > 0.001f) Shifted(-(1f - UIKit.EaseOutCubic(stackA)) * 420f, 0f, DrawButtons);
                if (controlsA > 0.001f) Shifted(0f, (1f - UIKit.EaseOutCubic(controlsA)) * 120f, () => DrawControls(kb));
                DrawHintCard();
                DrawGrabPrompt(kb);
                if (scoreA > 0.001f) Shifted((1f - UIKit.EaseOutCubic(scoreA)) * 340f, 0f, DrawScore);
                DrawPopups();
                DrawPrizeCard();
                DrawToast();
                DrawTitle();
                DrawRaceTimer();
                DrawTapeBall();
                DrawBossBar();
            }
            if (Game.Mode != UIMode.Loading) Merits.DrawToast();

            switch (Game.Mode)
            {
                case UIMode.FormWheel: Game.Selector.Draw(); break;
                case UIMode.Satchel: Game.Satchel.Draw(); break;
                case UIMode.Paused: Game.Pause.Draw(); break;
                case UIMode.Complete: DrawComplete(kb); break;
                case UIMode.LevelSelect: Game.LevelSelect.Draw(kb); break;
                case UIMode.Talk: Game.Dialogue.Draw(kb); break;
                case UIMode.Ask: Game.Asker.Draw(kb); break;
                case UIMode.Map: Game.Map.Draw(kb); break;
                case UIMode.Sticker: DrawStickerMode(kb); break;
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
            if (Game.Mode == UIMode.Cutscene && Game.CameraRig != null) k = Mathf.Max(k, Game.CameraRig.ShowcaseBlend);
            if (k <= 0.001f) return;
            float e = Mathf.SmoothStep(0f, 1f, k);
            float bar = 110f * e;
            var black = new Color(0.06f, 0.04f, 0.08f, 1f);
            UIKit.Fill(new Rect(0f, 0f, UIKit.Width, bar), black);
            UIKit.Fill(new Rect(0f, UIKit.Height - bar, UIKit.Width, bar), black);
            if (Game.Mode == UIMode.Cutscene && Game.CutsceneSkippable)
            {
                float sx = UIKit.Width - 330f, sy = bar - 78f;
                UIKit.PromptLabel(sx, sy, 40f, PromptAction.Confirm, "Hold to skip", kb, new Color(1f, 0.92f, 0.7f), e * 0.85f);
                float fill = Mathf.Clamp01(Game.CutsceneSkipFill);
                if (fill > 0f)
                {
                    UIKit.Fill(new Rect(sx, sy + 48f, 290f, 8f), new Color(1f, 1f, 1f, 0.2f * e), 4f);
                    UIKit.Fill(new Rect(sx, sy + 48f, 290f * fill, 8f), new Color(1f, 0.85f, 0.5f, e), 4f);
                }
            }
            if (Game.Mode == UIMode.Cutscene && !string.IsNullOrEmpty(Game.CutsceneCaption))
            {
                UIKit.Label(new Rect(0f, UIKit.Height - bar + 26f, UIKit.Width, 60f), Game.CutsceneCaption,
                    UIKit.Style(30, true, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.7f, e)));
                return;
            }
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
                kb ? "Keep tapping 4 to keep dancing!" : "Keep tapping D-pad down to keep dancing!",
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
            float y = level.IsHub ? 24f : 106f;   // below the hearts
            if (level.Id == Level1Builder.SceneName)
            {
                // Toybox Town: the town band is the goal.
                int band = Level1Director.BandCount();
                var br = new Rect(28f, y, 330f, 112f);
                UIKit.Panel(br, new Color(1f, 0.95f, 0.88f, 0.95f), new Color(0.75f, 0.32f, 0.36f, 0.9f), 20f);
                UIKit.Label(new Rect(br.x + 20f, br.y + 8f, 290f, 36f), "Town Band  " + band + " / 3",
                    UIKit.Style(24, true, TextAnchor.MiddleLeft, new Color(0.45f, 0.2f, 0.18f)));
                string[] names = { "Lute", "Trumpet", "Piper" };
                for (int i = 0; i < 3; i++)
                {
                    bool got = Level1Director.Has(Level1Director.BandIds[i]);
                    var chip = new Rect(br.x + 16f + i * 102f, br.y + 52f, 94f, 44f);
                    UIKit.Fill(chip, got ? new Color(1f, 0.78f, 0.3f) : new Color(0.62f, 0.6f, 0.66f, 0.55f), 22f);
                    UIKit.Label(chip, names[i], UIKit.Style(19, true, TextAnchor.MiddleCenter, got ? new Color(0.4f, 0.22f, 0.08f) : new Color(1f, 1f, 1f, 0.9f)));
                }
                y += 124f;
            }
            if (level.HeartstringIds.Count > 0)
            {
                // Heartstrings first: they're what the level is about.
                int got = level.HeartstringCount();
                var hr = new Rect(28f, y, 260f, 92f);
                UIKit.Panel(hr, new Color(1f, 0.95f, 0.86f, 0.95f), new Color(0.9f, 0.6f, 0.3f, 0.9f), 20f);
                UIKit.Heart(new Vector2(hr.x + 50f, hr.y + 48f), 58f, new Color(1f, 0.72f, 0.3f));
                UIKit.Label(new Rect(hr.x + 92f, hr.y + 10f, 160f, 50f), got + " / " + level.HeartstringIds.Count,
                    UIKit.Style(36, true, TextAnchor.MiddleLeft, new Color(0.45f, 0.25f, 0.1f)));
                string sub = level.HeartstringsNeeded <= 0 ? "Bonus finds" : got >= level.HeartstringsNeeded ? "The lift is open!" : "Need " + level.HeartstringsNeeded + " for the lift";
                UIKit.Label(new Rect(hr.x + 92f, hr.y + 54f, 170f, 28f), sub, UIKit.Style(17, true, TextAnchor.MiddleLeft, new Color(0.45f, 0.25f, 0.1f, 0.8f)));
                y += 104f;
            }
            if (total > 0)
            {
                var r = new Rect(28f, y, 230f, 84f);
                UIKit.Panel(r, UIKit.Paper, UIKit.Thread, 20f);
                UIKit.Icon(new Rect(r.x + 18f, r.y + 14f, 56f, 56f), PatchworkAssets.IconButton, Color.white);
                UIKit.Label(new Rect(r.x + 88f, r.y + 14f, 130f, 56f), level.CollectedCount() + " / " + total,
                    UIKit.Style(34, true, TextAnchor.MiddleLeft, UIKit.Ink));
                y += 96f;
            }
            if (level.SecretIds.Count > 0)
            {
                var mr = new Rect(28f, y, 230f, 70f);
                UIKit.Panel(mr, new Color(0.93f, 0.97f, 1f, 0.94f), new Color(0.4f, 0.65f, 0.95f, 0.9f), 20f);
                DrawMarbleIcon(new Vector2(mr.x + 42f, mr.y + 35f), 48f, Time.unscaledTime * 0.3f);
                UIKit.Label(new Rect(mr.x + 80f, mr.y + 10f, 140f, 50f), level.SecretCount() + " / " + level.SecretIds.Count,
                    UIKit.Style(28, true, TextAnchor.MiddleLeft, new Color(0.2f, 0.3f, 0.55f)));
                y += 82f;
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
            bool map = MapScreen.Available(Game.Level);
            string[] labels = map ? new[] { "Form Wheel", "Stickers", "Map", "Satchel", "Pause" } : new[] { "Form Wheel", "Stickers", "Satchel", "Pause" };
            PromptAction[] actions = map ? new[] { PromptAction.FormWheel, PromptAction.Sticker, PromptAction.Map, PromptAction.Satchel, PromptAction.Pause }
                                         : new[] { PromptAction.FormWheel, PromptAction.Sticker, PromptAction.Satchel, PromptAction.Pause };
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
            if (kb) text = a == PromptAction.FormWheel ? "Q" : a == PromptAction.Satchel ? "Tab" : a == PromptAction.Map ? "M" : a == PromptAction.Sticker ? "F" : "Esc";
            else text = a == PromptAction.FormWheel ? PatchworkInput.FormWheelPadLabel : a == PromptAction.Satchel ? "TOUCHPAD" : a == PromptAction.Map ? "R3" : a == PromptAction.Sticker ? "" : "OPTIONS";
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

        // Thread Race countdown at the top of the screen.
        // The Grumblewool's health, while you're fighting it.
        float bossA;
        void DrawBossBar()
        {
            var boss = Grumblewool.Active;
            bossA = Mathf.MoveTowards(bossA, boss != null ? 1f : 0f, Time.unscaledDeltaTime * 3f);
            if (bossA <= 0.001f || boss == null) return;
            float a = bossA;
            float W = UIKit.Width;
            var r = new Rect((W - 520f) * 0.5f, 24f - (1f - a) * 40f, 520f, 96f);
            UIKit.Panel(r, new Color(0.22f, 0.2f, 0.26f, 0.95f * a), new Color(0.95f, 0.4f, 0.45f, 0.95f * a), 22f);
            UIKit.Label(new Rect(r.x, r.y + 8f, r.width, 38f), boss.Name, UIKit.Style(28, true, TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.9f, a)));
            for (int i = 0; i < Grumblewool.MaxHp; i++)
            {
                bool full = i < boss.Hp;
                var c = new Vector2(r.x + r.width * 0.5f + (i - 1) * 64f, r.y + 66f);
                UIKit.Circle(c, 46f, full ? new Color(0.62f, 0.6f, 0.7f, a) : new Color(0.35f, 0.33f, 0.4f, 0.6f * a));
                if (full)
                {
                    UIKit.CircleOutline(c, 34f, new Color(0.85f, 0.85f, 0.92f, a), 3f);
                    UIKit.Circle(c + new Vector2(-6f, -6f), 10f, new Color(1f, 1f, 1f, 0.5f * a));
                }
            }
        }

        // The Tape Ball's size, while you're near it.
        float tapeAlpha;
        void DrawTapeBall()
        {
            var ball = TapeBall.Active;
            bool show = ball != null && TimedChallenge.Running == null && Grumblewool.Active == null && Game.Mode == UIMode.Gameplay;
            tapeAlpha = Mathf.MoveTowards(tapeAlpha, show ? 1f : 0f, Time.unscaledDeltaTime * 4f);
            if (tapeAlpha <= 0.001f || ball == null) return;
            float a = tapeAlpha;
            float W = UIKit.Width;
            var r = new Rect((W - 420f) * 0.5f, 24f - (1f - a) * 30f, 420f, 92f);
            UIKit.Panel(r, new Color(0.99f, 0.96f, 0.9f, 0.95f * a), new Color(1f, 0.45f, 0.6f, 0.9f * a), 20f);
            var ink = UIKit.Ink;
            string size = (ball.Radius * 2f).ToString("0.0") + " m";
            UIKit.Label(new Rect(r.x + 24f, r.y + 8f, 200f, 40f), "Tape Ball", UIKit.Style(26, true, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, a)));
            UIKit.Label(new Rect(r.xMax - 164f, r.y + 8f, 140f, 40f), size, UIKit.Style(30, true, TextAnchor.MiddleRight, new Color(0.85f, 0.3f, 0.45f, a)));
            var bar = new Rect(r.x + 24f, r.y + 54f, r.width - 48f, 14f);
            UIKit.Fill(bar, new Color(ink.r, ink.g, ink.b, 0.15f * a), 7f);
            UIKit.Fill(new Rect(bar.x, bar.y, Mathf.Max(14f, bar.width * ball.Progress), bar.height), new Color(1f, 0.55f, 0.7f, a), 7f);
            UIKit.Label(new Rect(r.x, r.yMax - 4f, r.width, 26f), ball.NextGoal, UIKit.Style(18, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, a)), true);
        }

        void DrawRaceTimer()
        {
            var ch = TimedChallenge.Running;
            if (ch != null)
            {
                float W2 = UIKit.Width;
                var cr = new Rect((W2 - 380f) * 0.5f, 24f, 380f, 86f);
                bool hurry2 = ch.TimeLeft < 5f;
                UIKit.Panel(cr, new Color(0.96f, 0.92f, 1f, 0.95f), hurry2 ? new Color(0.95f, 0.4f, 0.4f, 0.9f) : new Color(0.65f, 0.45f, 0.9f, 0.9f), 20f);
                float pulse2 = hurry2 ? 1f + Mathf.Abs(Mathf.Sin(Time.unscaledTime * 8f)) * 0.08f : 1f;
                UIKit.Label(new Rect(cr.x, cr.y + 6f, cr.width, 48f), Mathf.CeilToInt(Mathf.Max(0f, ch.TimeLeft)).ToString(),
                    UIKit.Style(Mathf.RoundToInt(40 * pulse2), true, TextAnchor.MiddleCenter, hurry2 ? new Color(0.8f, 0.2f, 0.25f) : UIKit.Ink));
                string sub = ch.Total > 1 ? ch.Title + "  " + ch.Got + " / " + ch.Total : ch.Title;
                UIKit.Label(new Rect(cr.x, cr.y + 50f, cr.width, 30f), sub,
                    UIKit.Style(20, true, TextAnchor.MiddleCenter, new Color(UIKit.Ink.r, UIKit.Ink.g, UIKit.Ink.b, 0.8f)));
                return;
            }
            var race = ThreadRace.Running;
            if (race == null) return;
            float W = UIKit.Width;
            var r = new Rect((W - 360f) * 0.5f, 24f, 360f, 86f);
            bool hurry = race.TimeLeft < 5f;
            UIKit.Panel(r, new Color(0.92f, 0.98f, 1f, 0.95f), hurry ? new Color(0.95f, 0.4f, 0.4f, 0.9f) : new Color(0.3f, 0.65f, 0.85f, 0.9f), 20f);
            float pulse = hurry ? 1f + Mathf.Abs(Mathf.Sin(Time.unscaledTime * 8f)) * 0.08f : 1f;
            UIKit.Label(new Rect(r.x, r.y + 6f, r.width, 48f), Mathf.CeilToInt(race.TimeLeft).ToString(),
                UIKit.Style(Mathf.RoundToInt(40 * pulse), true, TextAnchor.MiddleCenter, hurry ? new Color(0.8f, 0.2f, 0.25f) : UIKit.Ink));
            UIKit.Label(new Rect(r.x, r.y + 50f, r.width, 30f), "Rings " + race.Got + " / " + race.Total,
                UIKit.Style(20, true, TextAnchor.MiddleCenter, new Color(UIKit.Ink.r, UIKit.Ink.g, UIKit.Ink.b, 0.8f)));
        }

        // Floating "Talk" prompt over an NPC you're standing next to.
        void DrawTalkPrompt(bool kb)
        {
            if (Game.Mode != UIMode.Gameplay || Game.Talker == null || Game.Talker.Npc == null || Game.Helm != null) return;
            DrawWorldPrompt(Game.Talker.Npc.HeadPosition + Vector3.up * 1.3f, "Talk", kb, PromptAction.Ability);
        }

        // Floating prompt over the level-select station (the Pod's controller) while you're at it.
        void DrawHelmPrompt(bool kb)
        {
            if (Game.Mode != UIMode.Gameplay || Game.Helm == null) return;
            DrawWorldPrompt(Game.Helm.PromptPoint, Game.Helm.Label, kb, PromptAction.Ability);
        }

        void DrawWorldPrompt(Vector3 world, string text, bool kb, PromptAction action)
        {
            var cam = Game.CameraRig.Cam;
            Vector3 sp = cam.WorldToScreenPoint(world);
            if (sp.z <= 0f) return;
            float gx = sp.x / UIKit.Scale;
            float gy = (Screen.height - sp.y) / UIKit.Scale;
            float bob = Mathf.Sin(Time.unscaledTime * 4f) * 5f;
            var label = UIKit.Style(26, true, TextAnchor.MiddleLeft, Color.white);
            float w = 56f + 12f + UIKit.TextWidth(text, label) + 36f;
            var r = new Rect(gx - w * 0.5f, gy - 34f + bob, w, 68f);
            UIKit.Pill(r);
            UIKit.Prompt(r.x + 14f, r.y + 6f, 56f, action, kb);
            UIKit.Label(new Rect(r.x + 82f, r.y, w - 82f, r.height), text, label);
        }

        // "Grab" over a toy you can hold, "Pull" over a lever.
        void DrawGrabPrompt(bool kb)
        {
            if (Game.Mode != UIMode.Gameplay) return;
            if (Warp.Active != null)
            {
                DrawWorldPrompt(Warp.Active.transform.position + Vector3.up * 1.6f, Warp.Active.Label, kb, PromptAction.Ability);
                return;
            }
            if (UseSpot.Active != null)
            {
                DrawWorldPrompt(UseSpot.Active.PromptPoint, UseSpot.Active.Label, kb, PromptAction.Ability);
                return;
            }
            if (DigSpot.Active != null)
            {
                DrawWorldPrompt(DigSpot.Active.transform.position + Vector3.up * 1.2f, "Dig", kb, PromptAction.Grab);
                return;
            }
            var hand = Game.Player.Hand;
            if (Lever.Active != null && hand.Held == null)
            {
                DrawWorldPrompt(Lever.Active.transform.position + Vector3.up * 1.4f, Lever.Active.Label, kb, PromptAction.Grab);
                return;
            }
            if (hand.Held != null || hand.Target == null) return;
            var b = hand.Target.Body;
            DrawWorldPrompt(b.worldCenterOfMass + Vector3.up * 1.3f, "Grab", kb, PromptAction.Grab);
        }

        // ----- Score -----
        static string Num(float v)
        {
            return Mathf.RoundToInt(v).ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
        }

        void DrawScore()
        {
            var s = Game.Score;
            if (s == null) return;
            float pop = 1f + Mathf.Sin(s.Pop * Mathf.PI) * 0.12f;
            float w = 270f, h = 76f;
            var r = new Rect(UIKit.Width - w - 28f, 24f, w, h);
            UIKit.Panel(r, new Color(0.99f, 0.96f, 0.9f, 0.94f), new Color(0.35f, 0.6f, 0.9f, 0.9f), 20f);
            UIKit.Circle(new Vector2(r.x + 40f, r.y + h * 0.5f), 44f * pop, new Color(0.6f, 0.85f, 1f));
            UIKit.Circle(new Vector2(r.x + 34f, r.y + h * 0.5f - 7f), 12f, new Color(1f, 1f, 1f, 0.85f));
            UIKit.Label(new Rect(r.x + 72f, r.y + 6f, w - 86f, h - 12f), Num(s.Shown),
                UIKit.Style(Mathf.RoundToInt(34 * pop), true, TextAnchor.MiddleRight, UIKit.Ink));
            if (s.Multiplier > 1 && s.ChainAlive)
            {
                var chip = new Rect(r.xMax - 118f, r.yMax + 8f, 118f, 44f);
                float wob = Mathf.Sin(Time.unscaledTime * 10f) * 2f;
                UIKit.Fill(new Rect(chip.x, chip.y + wob, chip.width, chip.height), new Color(1f, 0.72f, 0.25f, 0.95f), 22f);
                UIKit.Label(new Rect(chip.x, chip.y + wob, chip.width, chip.height), "x" + s.Multiplier + " CHAIN",
                    UIKit.Style(20, true, TextAnchor.MiddleCenter, new Color(0.4f, 0.18f, 0.05f)));
            }
        }

        void DrawPopups()
        {
            var s = Game.Score;
            if (s == null || Game.CameraRig == null) return;
            var cam = Game.CameraRig.Cam;
            foreach (var p in s.Popups)
            {
                Vector3 sp = cam.WorldToScreenPoint(p.World + Vector3.up * (0.6f + p.Age * 1.4f));
                if (sp.z <= 0f) continue;
                float gx = sp.x / UIKit.Scale, gy = (Screen.height - sp.y) / UIKit.Scale;
                float a = Mathf.Clamp01((1.1f - p.Age) / 0.35f);
                float grow = 1f + Mathf.Clamp01(1f - p.Age / 0.15f) * 0.4f;
                var c = p.Color;
                c.a = a;
                UIKit.Label(new Rect(gx - 100f, gy - 24f, 200f, 48f), p.Text, UIKit.Style(Mathf.RoundToInt(28 * grow), true, TextAnchor.MiddleCenter, c), true);
            }
        }

        // ----- Prize card -----
        void DrawSecretCard()
        {
            float t = Game.SecretTimer;
            if (t <= 0f) return;
            float age = 3.6f - t;
            float tin = UIKit.EaseOutBack(Mathf.Clamp01(age / 0.4f));
            float tout = Mathf.Clamp01((0.4f - t) / 0.4f);
            float y = 130f - (1f - tin) * 260f - tout * 260f;
            var r = new Rect((UIKit.Width - 560f) * 0.5f, y, 560f, 150f);
            UIKit.Panel(r, new Color(0.93f, 0.97f, 1f, 0.97f), new Color(0.4f, 0.65f, 0.95f, 0.9f), 24f);
            DrawMarbleIcon(new Vector2(r.x + 80f, r.y + 75f), 104f, age);
            var ink = UIKit.Ink;
            UIKit.Label(new Rect(r.x + 160f, r.y + 18f, 380f, 34f), "SECRET!", UIKit.Style(24, true, TextAnchor.MiddleLeft, new Color(0.3f, 0.5f, 0.85f)));
            UIKit.Label(new Rect(r.x + 160f, r.y + 52f, 380f, 46f), "Secret Marble", UIKit.Style(34, true, TextAnchor.MiddleLeft, ink));
            UIKit.Label(new Rect(r.x + 160f, r.y + 98f, 380f, 30f), Game.Level.SecretCount() + " of " + Game.Level.SecretIds.Count + " found",
                UIKit.Style(20, false, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, 0.75f)));
        }

        // A glass marble with a swirl, drawn flat.
        static void DrawMarbleIcon(Vector2 c, float d, float spin)
        {
            UIKit.Circle(c, d, new Color(0.75f, 0.88f, 1f));
            UIKit.Circle(c + new Vector2(Mathf.Cos(spin * 3f) * d * 0.12f, Mathf.Sin(spin * 3f) * d * 0.12f), d * 0.55f, new Color(1f, 0.55f, 0.7f, 0.9f));
            UIKit.Circle(c - new Vector2(Mathf.Cos(spin * 3f) * d * 0.14f, Mathf.Sin(spin * 3f) * d * 0.14f), d * 0.35f, new Color(0.45f, 0.8f, 1f, 0.9f));
            UIKit.Circle(c + new Vector2(-d * 0.2f, -d * 0.22f), d * 0.2f, new Color(1f, 1f, 1f, 0.85f));
            UIKit.CircleOutline(c, d, new Color(1f, 1f, 1f, 0.7f), 3f);
        }

        void DrawPrizeCard()
        {
            DrawSecretCard();
            var prize = Game.PrizeShown;
            float t = Game.PrizeTimer;
            if (prize == null || t <= 0f) return;
            float age = 3.6f - t;
            float tin = UIKit.EaseOutBack(Mathf.Clamp01(age / 0.4f));
            float tout = Mathf.Clamp01((0.4f - t) / 0.4f);
            float y = 130f - (1f - tin) * 260f - tout * 260f;
            var r = new Rect((UIKit.Width - 560f) * 0.5f, y, 560f, 150f);
            UIKit.Panel(r, new Color(1f, 0.96f, 0.98f, 0.97f), new Color(0.85f, 0.45f, 0.75f, 0.9f), 24f);
            var icon = new Rect(r.x + 26f, r.y + 20f, 110f, 110f);
            UIKit.Circle(icon.center, 120f, new Color(0.95f, 0.85f, 1f));
            if (prize.Kind == PrizeKind.Sticker)
                UIKit.IconTransformed(icon, StickerArt.Get((StickerId)prize.Value), Color.white, Mathf.Sin(age * 3f) * 8f, 1f);
            else
            {
                var box = new Rect(icon.x + 22f, icon.y + 26f, 66f, 60f);
                UIKit.Fill(box, prize.Kind == PrizeKind.Hat ? new Color(0.45f, 0.65f, 0.95f) : new Color(0.95f, 0.45f, 0.55f), 8f);
                UIKit.Fill(new Rect(box.center.x - 6f, box.y, 12f, box.height), new Color(1f, 0.85f, 0.35f), 3f);
                UIKit.Fill(new Rect(box.x, box.center.y - 6f, box.width, 12f), new Color(1f, 0.85f, 0.35f), 3f);
                UIKit.Circle(new Vector2(box.center.x - 10f, box.y - 4f), 20f, new Color(1f, 0.85f, 0.35f));
                UIKit.Circle(new Vector2(box.center.x + 10f, box.y - 4f), 20f, new Color(1f, 0.85f, 0.35f));
            }
            var ink = UIKit.Ink;
            UIKit.Label(new Rect(r.x + 160f, r.y + 18f, 380f, 34f), "PRIZE!", UIKit.Style(24, true, TextAnchor.MiddleLeft, new Color(0.8f, 0.35f, 0.6f)));
            UIKit.Label(new Rect(r.x + 160f, r.y + 52f, 380f, 46f), prize.Name, UIKit.Style(34, true, TextAnchor.MiddleLeft, ink));
            UIKit.Label(new Rect(r.x + 160f, r.y + 98f, 380f, 30f), prize.KindText, UIKit.Style(20, false, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, 0.75f)));
        }

        // ----- Sticker mode -----
        void DrawStickerMode(bool kb)
        {
            var st = Game.Stickers;
            float W = UIKit.Width, H = UIKit.Height;
            var ink = UIKit.Ink;
            // Crosshair.
            Vector2 c = new Vector2(W * 0.5f, H * 0.5f);
            Color cross = st.HasAim ? new Color(1f, 1f, 1f, 0.9f) : new Color(1f, 0.6f, 0.6f, 0.8f);
            UIKit.CircleOutline(c, 46f, new Color(0f, 0f, 0f, 0.3f), 5f);
            UIKit.CircleOutline(c, 44f, cross, 3f);
            UIKit.Circle(c, 8f, cross);
            if (!st.HasAim)
                UIKit.Label(new Rect(c.x - 200f, c.y + 34f, 400f, 36f), "Too far away", UIKit.Style(22, true, TextAnchor.MiddleCenter, cross), true);

            // The sticker book along the bottom.
            var owned = st.Owned;
            float cell = 96f;
            float total = owned.Count * (cell + 12f) + 40f;
            var strip = new Rect((W - total) * 0.5f, H - 220f, total, cell + 60f);
            UIKit.Panel(strip, new Color(0.99f, 0.96f, 0.9f, 0.95f), UIKit.Thread, 24f);
            for (int i = 0; i < owned.Count; i++)
            {
                var r = new Rect(strip.x + 20f + i * (cell + 12f), strip.y + 14f, cell, cell);
                bool sel = i == st.Selected;
                if (sel) UIKit.Fill(new Rect(r.x - 6f, r.y - 6f, r.width + 12f, r.height + 12f), new Color(1f, 0.8f, 0.35f, 0.9f), 18f);
                float bob = sel ? Mathf.Sin(Time.unscaledTime * 6f) * 3f : 0f;
                UIKit.IconTransformed(new Rect(r.x, r.y + bob, r.width, r.height), StickerArt.Get(owned[i]), Color.white, sel ? -st.Spin : 0f, sel ? 1f : 0.85f);
            }
            if (owned.Count > 0)
                UIKit.Label(new Rect(strip.x, strip.yMax - 44f, strip.width, 36f), StickerArt.Name(owned[Mathf.Clamp(st.Selected, 0, owned.Count - 1)]),
                    UIKit.Style(22, true, TextAnchor.MiddleCenter, ink));

            // Prompts.
            float y = H - 64f;
            float x = 40f;
            var white = new Color(1f, 1f, 1f, 0.95f);
            var bg = new Rect(20f, y - 12f, W - 40f, 60f);
            UIKit.Fill(bg, new Color(0.13f, 0.11f, 0.19f, 0.6f), 30f);
            x += UIKit.PromptLabel(x, y, 36f, PromptAction.Confirm, "Stick", kb, white) + 34f;
            x += UIKit.PromptLabel(x, y, 36f, PromptAction.Navigate, "Choose", kb, white) + 34f;
            x += UIKit.PromptLabel(x, y, 36f, PromptAction.Rotate, "Turn", kb, white) + 34f;
            if (st.PeelTarget) x += UIKit.PromptLabel(x, y, 36f, PromptAction.Grab, "Peel off", kb, white) + 34f;
            UIKit.PromptLabel(x, y, 36f, PromptAction.Cancel, "Done", kb, white);
            UIKit.Label(new Rect(0f, 30f, W, 40f), "Stickers: aim with the camera", UIKit.Style(26, true, TextAnchor.MiddleCenter, white), true);
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
            var panel = new Rect((W - 640f) * 0.5f, (H - 600f) * 0.5f, 640f, 600f);
            UIKit.Panel(panel, new Color(0.99f, 0.95f, 0.88f, 0.97f), UIKit.Thread);
            var ink = UIKit.Ink;
            UIKit.Label(new Rect(panel.x, panel.y + 40f, panel.width, 60f), "Level Complete!", UIKit.Style(48, true, TextAnchor.MiddleCenter, ink));
            UIKit.Label(new Rect(panel.x, panel.y + 100f, panel.width, 36f), Game.Level.Title, UIKit.Style(22, false, TextAnchor.MiddleCenter, ink));
            UIKit.Icon(new Rect(panel.x + 200f, panel.y + 150f, 90f, 90f), PatchworkAssets.IconButton, Color.white);
            UIKit.Label(new Rect(panel.x + 305f, panel.y + 150f, 200f, 90f),
                Game.Level.CollectedCount() + " / " + Game.Level.CollectibleIds.Count, UIKit.Style(44, true, TextAnchor.MiddleLeft, ink));
            if (Game.Level.HeartstringIds.Count > 0)
            {
                UIKit.Heart(new Vector2(panel.x + 140f, panel.y + 195f), 70f, new Color(1f, 0.72f, 0.3f));
                UIKit.Label(new Rect(panel.x + 90f, panel.y + 240f, 110f, 40f), Game.Level.HeartstringCount() + "/" + Game.Level.HeartstringIds.Count,
                    UIKit.Style(28, true, TextAnchor.MiddleCenter, ink));
            }
            if (Game.Level.ThreadTotal > 0)
            {
                UIKit.Icon(new Rect(panel.x + 210f, panel.y + 236f, 70f, 70f), PatchworkAssets.IconYarn, Color.white);
                UIKit.Label(new Rect(panel.x + 305f, panel.y + 236f, 260f, 70f),
                    Game.ThreadsCollected + " / " + Game.Level.ThreadTotal + " threads", UIKit.Style(30, true, TextAnchor.MiddleLeft, ink));
            }
            if (Game.Level.SecretIds.Count > 0)
            {
                DrawMarbleIcon(new Vector2(panel.xMax - 110f, panel.y + 195f), 70f, Time.unscaledTime * 0.3f);
                UIKit.Label(new Rect(panel.xMax - 170f, panel.y + 240f, 120f, 40f), Game.Level.SecretCount() + "/" + Game.Level.SecretIds.Count,
                    UIKit.Style(28, true, TextAnchor.MiddleCenter, ink));
            }
            // Score: counts up, then the medal stamps on.
            float ct = Game.CompleteTime;
            float shown = Mathf.Lerp(0f, Game.Score.Total, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((ct - 0.3f) / 1.4f)));
            var scoreRow = new Rect(panel.x + 40f, panel.y + 320f, panel.width - 80f, 110f);
            UIKit.Fill(scoreRow, new Color(1f, 1f, 1f, 0.55f), 18f);
            UIKit.Label(new Rect(scoreRow.x + 24f, scoreRow.y + 10f, 300f, 30f), "Score", UIKit.Style(22, true, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, 0.75f)));
            UIKit.Label(new Rect(scoreRow.x + 24f, scoreRow.y + 40f, 320f, 60f), Num(shown), UIKit.Style(46, true, TextAnchor.MiddleLeft, ink));
            if (ct > 1.8f)
            {
                var medal = ScoreKeeper.MedalFor(Game.Score.Total, Game.Level.ScorePossible);
                float stamp = UIKit.EaseOutBack(Mathf.Clamp01((ct - 1.8f) / 0.35f));
                Vector2 mc = new Vector2(scoreRow.xMax - 90f, scoreRow.center.y);
                if (medal != ScoreKeeper.Medal.None)
                {
                    UIKit.Circle(mc, 96f * stamp, ScoreKeeper.MedalColor(medal));
                    UIKit.CircleOutline(mc, 80f * stamp, new Color(1f, 1f, 1f, 0.8f), 3f);
                    UIKit.Label(new Rect(mc.x - 60f, mc.y - 18f, 120f, 36f), medal.ToString().ToUpperInvariant(), UIKit.Style(Mathf.RoundToInt(18 * stamp + 1), true, TextAnchor.MiddleCenter, new Color(0.3f, 0.2f, 0.1f)));
                }
                if (Game.NewBestScore)
                {
                    var tag = new Rect(scoreRow.x + 250f, scoreRow.y + 12f, 130f, 30f);
                    UIKit.Fill(tag, new Color(0.95f, 0.45f, 0.55f), 15f);
                    UIKit.Label(tag, "NEW BEST!", UIKit.Style(17, true, TextAnchor.MiddleCenter, Color.white));
                }
            }
            float x = panel.x + 150f;
            UIKit.PromptLabel(x, panel.yMax - 80f, 44f, PromptAction.Confirm, "Back to the Pod", kb, ink);
        }
    }
}

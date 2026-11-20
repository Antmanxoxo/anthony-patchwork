using UnityEngine;

namespace Patchwork
{
    // Choosing a level at the Skiff's helm. The camera moves in on the globe, which turns to show
    // the chosen level's pin; a card beside it shows the level's name, what you've found there and
    // your best thread count. Confirm sets sail (the loading screen takes over from there).
    public class LevelSelectMenu
    {
        readonly PatchworkGame game;
        int index;
        float openTime;
        float stickCooldown;
        float changeTime;     // time since the selection changed (for the card's little pop)
        float shake;          // wobble when you try a locked level
        bool sailing;
        float sailTime;
        static int lastIndex;
        // Picking up the controller first offers a choice: set sail (the globe), or the Store.
        bool choosing;
        int choice;
        float choiceTime;
        static readonly string[] Choices = { "Set Sail", "The Store" };
        static readonly string[] ChoiceInfo = { "Turn the globe and pick where to go next.", "Spend your points on colours, fabrics and outfits, modelled by the cast." };

        public LevelSelectMenu(PatchworkGame game)
        {
            this.game = game;
        }

        public static void ResetStatics() { lastIndex = 0; }

        LevelGlobe Globe { get { return game.Level.Globe; } }

        Vector3 ViewPoint
        {
            get
            {
                var g = Globe;
                Vector3 c = g != null ? g.transform.position : Vector3.zero;
                return c + (g != null ? g.ViewOffset : new Vector3(0.4f, 0.9f, -6.4f));
            }
        }

        public void Open()
        {
            index = Mathf.Clamp(lastIndex, 0, LevelCatalog.Levels.Length - 1);
            openTime = 0f;
            stickCooldown = 0.2f;
            changeTime = 0f;
            sailing = false;
            choosing = true;
            choice = 0;
            choiceTime = 0f;
            var cam = game.CameraRig;
            Vector3 c = Globe != null ? Globe.transform.position : Vector3.zero;
            cam.ShowcasePosition = ViewPoint;
            // Look a little to the right of the globe so it sits on the left of the screen, beside the card.
            cam.ShowcaseLookAt = c + new Vector3(2.3f, 0.1f, 0f);
            cam.ShowcaseBlendSpeed = 1.4f;
            cam.Showcase = true;
        }

        // Back from the Store: the choice again, on the Store.
        public void BackFromStore()
        {
            choosing = true;
            choice = 1;
            choiceTime = 0.3f;
            openTime = 0.2f;
        }

        public void Close()
        {
            game.CameraRig.ShowcaseBlendSpeed = 1.4f;
            game.CameraRig.Showcase = false;
            if (Globe != null) Globe.Focus(-1, Vector3.zero);
        }

        public void Update(float udt)
        {
            openTime += udt;
            changeTime += udt;
            shake = Mathf.MoveTowards(shake, 0f, udt * 3f);
            var input = game.Input;
            if (sailing)
            {
                sailTime += udt;
                if (sailTime > 0.5f)
                {
                    sailing = false;
                    game.SetSail(LevelCatalog.Levels[index].Scene);
                }
                return;
            }
            if (openTime < 0.15f) return;

            if (choosing) { UpdateChoice(udt); return; }

            if (input.Cancel.WasPressedThisFrame())
            {
                // Back to the controller's choice.
                choosing = true;
                choiceTime = 0f;
                if (Globe != null) Globe.Focus(-1, Vector3.zero);
                Sfx.Play(SfxId.MenuCancel, 0.4f);
                return;
            }

            stickCooldown -= udt;
            int dir = 0;
            if (input.NavLeft.WasPressedThisFrame()) dir = -1;
            else if (input.NavRight.WasPressedThisFrame()) dir = 1;
            else
            {
                Vector2 stick = input.Move.ReadValue<Vector2>();
                if (Mathf.Abs(stick.x) > 0.6f && stickCooldown <= 0f) dir = stick.x > 0f ? 1 : -1;
                if (Mathf.Abs(stick.x) < 0.3f) stickCooldown = 0f;
            }
            if (dir != 0)
            {
                stickCooldown = 0.32f;
                int n = LevelCatalog.Levels.Length;
                index = ((index + dir) % n + n) % n;
                lastIndex = index;
                changeTime = 0f;
                if (Globe != null) Globe.Focus(index, ViewPoint);
                Sfx.Play(SfxId.PageTurn, 0.5f);
            }

            if (input.Confirm.WasPressedThisFrame())
            {
                var e = LevelCatalog.Levels[index];
                if (e.Playable)
                {
                    sailing = true;
                    sailTime = 0f;
                    Sfx.Play(SfxId.MenuConfirm, 0.6f);
                    Sfx.Play(SfxId.PodRocket, 0.7f);
                    MusicPlayer.Hush();   // the song fades away into the loading screen
                    game.CameraRig.Kick(0.25f);
                    Haptics.Pulse(0.25f, 0.15f, 0.5f);
                }
                else
                {
                    shake = 1f;
                    Sfx.Play(SfxId.TransformFail, 0.4f);
                }
            }
        }

        void UpdateChoice(float udt)
        {
            choiceTime += udt;
            var input = game.Input;
            if (input.Cancel.WasPressedThisFrame())
            {
                game.CloseLevelSelect();
                return;
            }
            stickCooldown -= udt;
            int dir = 0;
            if (input.NavUp.WasPressedThisFrame() || input.NavLeft.WasPressedThisFrame()) dir = -1;
            else if (input.NavDown.WasPressedThisFrame() || input.NavRight.WasPressedThisFrame()) dir = 1;
            else
            {
                Vector2 stick = input.Move.ReadValue<Vector2>();
                float m = Mathf.Abs(stick.y) > Mathf.Abs(stick.x) ? -stick.y : stick.x;
                if (Mathf.Abs(m) > 0.6f && stickCooldown <= 0f) dir = m > 0f ? 1 : -1;
                if (stick.magnitude < 0.3f) stickCooldown = 0f;
            }
            if (dir != 0 && choice != Mathf.Clamp(choice + dir, 0, Choices.Length - 1))
            {
                stickCooldown = 0.3f;
                choice = Mathf.Clamp(choice + dir, 0, Choices.Length - 1);
                choiceTime = 0f;
                Sfx.Play(SfxId.MenuMove, 0.5f);
            }
            if (!input.Confirm.WasPressedThisFrame()) return;
            Sfx.Play(SfxId.MenuConfirm, 0.55f);
            if (choice == 0)
            {
                choosing = false;
                changeTime = 0f;
                if (Globe != null) Globe.Focus(index, ViewPoint);
            }
            else game.OpenStore();
        }

        // The controller's own menu: two big felt buttons on a card, the purse underneath.
        void DrawChoice(bool kb)
        {
            float W = UIKit.Width, H = UIKit.Height, time = Time.unscaledTime;
            float a = Mathf.Clamp01(openTime / 0.4f);
            var ink = UIKit.Ink;
            float cw = 640f, ch = 640f;
            var card = new Rect(W - cw - 110f + (1f - a) * 200f, (H - ch) * 0.5f - 10f, cw, ch);
            UIKit.Panel(card, new Color(0.99f, 0.96f, 0.9f, 0.96f * a), new Color(0.45f, 0.35f, 0.6f, 0.9f * a));
            UIKit.Label(new Rect(card.x + 40f, card.y + 30f, card.width - 80f, 64f), "Patch's Controller", UIKit.Style(44, true, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, a)));
            Color[] tints = { new Color(0.45f, 0.65f, 0.95f), new Color(0.9f, 0.45f, 0.62f) };
            for (int i = 0; i < Choices.Length; i++)
            {
                bool on = i == choice;
                float pop = on ? UIKit.EaseOutBack(Mathf.Clamp01(choiceTime / 0.3f)) : 0f;
                float grow = on ? 8f * pop + Mathf.Sin(time * 4f) * 2f : 0f;
                var r = new Rect(card.x + 40f - grow, card.y + 120f + i * 200f - grow, card.width - 80f + grow * 2f, 176f + grow * 2f);
                var c = tints[i];
                UIKit.Fill(new Rect(r.x + 4f, r.y + 8f, r.width, r.height), new Color(0.3f, 0.2f, 0.25f, 0.15f * a), 26f);
                UIKit.Fill(r, on ? new Color(c.r, c.g, c.b, a) : new Color(1f, 0.99f, 0.97f, a), 26f);
                if (on) PauseMenu.MarchingStitches(new Rect(r.x + 8f, r.y + 8f, r.width - 16f, r.height - 16f), new Color(1f, 1f, 1f, 0.85f * a), time * 40f);
                else UIKit.Outline(r, new Color(ink.r, ink.g, ink.b, 0.25f * a), 2f, 26f);
                // The picture: a little globe, or a golden button.
                var icon = new Vector2(r.x + 88f, r.center.y);
                if (i == 0)
                {
                    UIKit.Circle(icon, 104f, new Color(0.35f, 0.6f, 0.9f, a));
                    UIKit.Circle(icon + new Vector2(-14f, -10f), 46f, new Color(0.45f, 0.8f, 0.45f, a));
                    UIKit.Circle(icon + new Vector2(22f, 18f), 30f, new Color(0.45f, 0.8f, 0.45f, a));
                    UIKit.CircleOutline(icon, 104f, new Color(1f, 1f, 1f, 0.8f * a), 4f);
                }
                else
                {
                    UIKit.IconTransformed(new Rect(icon.x - 50f, icon.y - 50f, 100f, 100f), PatchworkAssets.IconButton, new Color(1f, 1f, 1f, a), on ? Mathf.Sin(time * 3f) * 10f : 0f, 1f);
                }
                Color tc = on ? new Color(1f, 1f, 1f, a) : new Color(ink.r, ink.g, ink.b, a);
                UIKit.Label(new Rect(r.x + 160f, r.y + 26f, r.width - 190f, 56f), Choices[i], UIKit.Style(38, true, TextAnchor.MiddleLeft, tc));
                UIKit.Label(new Rect(r.x + 160f, r.y + 84f, r.width - 190f, 80f), ChoiceInfo[i], UIKit.Style(20, false, TextAnchor.UpperLeft, new Color(tc.r, tc.g, tc.b, 0.85f * a), true));
            }
            // The purse.
            string purse = GameState.Purse.ToString("N0") + " points to spend";
            UIKit.Icon(new Rect(card.x + 40f, card.yMax - 132f, 40f, 40f), PatchworkAssets.IconButton, new Color(1f, 1f, 1f, a));
            UIKit.Label(new Rect(card.x + 92f, card.yMax - 132f, card.width - 130f, 40f), purse, UIKit.Style(22, true, TextAnchor.MiddleLeft, new Color(0.5f, 0.32f, 0.08f, a)));
            float px = card.x + 40f, py = card.yMax - 76f;
            px += UIKit.PromptLabel(px, py, 44f, PromptAction.Confirm, "Choose", kb, ink, a) + 36f;
            UIKit.PromptLabel(px, py, 44f, PromptAction.Cancel, "Put it down", kb, ink, a);
            UIKit.Label(new Rect(0f, 40f, W, 60f), "What shall we do?", UIKit.Style(44, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, a)), true);
        }

        public void Draw(bool kb)
        {
            if (choosing) { DrawChoice(kb); return; }
            float W = UIKit.Width, H = UIKit.Height;
            float a = Mathf.Clamp01(openTime / 0.4f);
            var e = LevelCatalog.Levels[index];
            var ink = UIKit.Ink;

            // Pin marker on the globe.
            DrawPinMarker(e, a);

            // The card on the right.
            float pop = UIKit.EaseOutBack(Mathf.Clamp01(changeTime / 0.3f));
            float sx = Mathf.Sin(shake * 40f) * 14f * shake;
            float cw = 640f, ch = 680f;
            var card = new Rect(W - cw - 110f + sx + (1f - a) * 200f, (H - ch) * 0.5f - 20f, cw, ch);
            Color tint = e.Tint;
            UIKit.Panel(card, new Color(0.99f, 0.96f, 0.9f, 0.96f * a), new Color(tint.r * 0.7f, tint.g * 0.7f, tint.b * 0.7f, 0.9f * a));

            // Number tag.
            var tagStyle = UIKit.Style(24, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, a));
            float tw = UIKit.TextWidth(e.Number, tagStyle) + 48f;
            var tag = new Rect(card.x + 40f, card.y - 22f, tw, 46f);
            UIKit.Fill(tag, new Color(tint.r * 0.75f, tint.g * 0.75f, tint.b * 0.75f, a), 23f);
            UIKit.Label(tag, e.Number, tagStyle);

            // Title + blurb.
            float s = Mathf.Lerp(0.9f, 1f, pop);
            UIKit.Label(new Rect(card.x + 40f, card.y + 46f, card.width - 80f, 130f), e.Title,
                UIKit.Style(Mathf.RoundToInt(50 * s), true, TextAnchor.UpperLeft, new Color(ink.r, ink.g, ink.b, a), true));
            UIKit.Label(new Rect(card.x + 40f, card.y + 190f, card.width - 80f, 140f), e.Blurb,
                UIKit.Style(24, false, TextAnchor.UpperLeft, new Color(ink.r, ink.g, ink.b, 0.85f * a), true));

            float y = card.y + 340f;
            if (e.Playable)
            {
                // Golden buttons.
                int found = LevelCatalog.FoundButtons(e);
                UIKit.Label(new Rect(card.x + 40f, y, 300f, 36f), "Golden buttons", UIKit.Style(22, true, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, 0.8f * a)));
                for (int i = 0; i < e.Collectibles.Length; i++)
                {
                    var c = new Vector2(card.x + 270f + i * 58f, y + 18f);
                    if (i < found) UIKit.Circle(c, 44f, new Color(1f, 0.8f, 0.25f, a));
                    else UIKit.CircleOutline(c, 44f, new Color(ink.r, ink.g, ink.b, 0.3f * a), 3f);
                }
                // Heartstrings.
                if (e.Heartstrings.Length > 0)
                {
                    UIKit.Heart(new Vector2(card.x + 52f, y + 112f), 36f, new Color(1f, 0.72f, 0.3f, a));
                    UIKit.Label(new Rect(card.x + 80f, y + 94f, card.width - 120f, 36f), "Heartstrings   " + LevelCatalog.FoundHeartstrings(e) + " / " + e.Heartstrings.Length,
                        UIKit.Style(22, true, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, 0.8f * a)));
                }
                // Threads.
                int total;
                int best = GameState.BestThreads(e.Scene, out total);
                UIKit.Label(new Rect(card.x + 40f, y + 52f, card.width - 80f, 36f),
                    total > 0 ? "Best threads   " + best + " / " + total : "Best threads   -",
                    UIKit.Style(22, true, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, 0.8f * a)));
                // Best score and its medal, on the right of the threads line.
                int possible;
                int bestScore = GameState.BestScore(e.Scene, out possible);
                if (bestScore > 0)
                {
                    var medal = ScoreKeeper.MedalFor(bestScore, possible);
                    UIKit.Label(new Rect(card.x + 40f, y + 52f, card.width - 130f, 36f),
                        "Best score  " + bestScore.ToString("N0", System.Globalization.CultureInfo.InvariantCulture),
                        UIKit.Style(22, true, TextAnchor.MiddleRight, new Color(ink.r, ink.g, ink.b, 0.8f * a)));
                    var mc = ScoreKeeper.MedalColor(medal);
                    if (medal != ScoreKeeper.Medal.None)
                        UIKit.Circle(new Vector2(card.xMax - 64f, y + 70f), 40f, new Color(mc.r, mc.g, mc.b, a));
                }
                if (GameState.IsLevelComplete(e.Scene))
                {
                    var ribbon = new Rect(card.xMax - 210f, card.y + 18f, 180f, 40f);
                    UIKit.Fill(ribbon, new Color(1f, 0.8f, 0.25f, a), 20f);
                    UIKit.Label(ribbon, "Completed!", UIKit.Style(20, true, TextAnchor.MiddleCenter, new Color(0.35f, 0.22f, 0.05f, a)));
                }
            }
            else
            {
                UIKit.Icon(new Rect(card.x + 40f, y - 4f, 64f, 64f), PatchworkAssets.IconLock, new Color(1f, 1f, 1f, 0.8f * a));
                UIKit.Label(new Rect(card.x + 120f, y, card.width - 160f, 56f), "Coming soon",
                    UIKit.Style(30, true, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, 0.6f * a)));
            }

            // Arrows to show you can turn the globe.
            float arrowY = card.yMax - 150f;
            UIKit.Label(new Rect(card.x + 30f, arrowY, 80f, 60f), "<", UIKit.Style(44, true, TextAnchor.MiddleCenter, new Color(tint.r * 0.6f, tint.g * 0.6f, tint.b * 0.6f, a)));
            UIKit.Label(new Rect(card.xMax - 110f, arrowY, 80f, 60f), ">", UIKit.Style(44, true, TextAnchor.MiddleCenter, new Color(tint.r * 0.6f, tint.g * 0.6f, tint.b * 0.6f, a)));
            // Dots for each level.
            int n = LevelCatalog.Levels.Length;
            for (int i = 0; i < n; i++)
            {
                var c = new Vector2(card.center.x + (i - (n - 1) * 0.5f) * 30f, arrowY + 30f);
                UIKit.Circle(c, i == index ? 18f : 12f, new Color(ink.r, ink.g, ink.b, (i == index ? 0.8f : 0.3f) * a));
            }

            // Prompts.
            float py = card.yMax - 76f;
            float px = card.x + 40f;
            px += UIKit.PromptLabel(px, py, 44f, PromptAction.Confirm, e.Playable ? "Let's go!" : "Locked", kb, ink, a) + 36f;
            UIKit.PromptLabel(px, py, 44f, PromptAction.Cancel, "Back", kb, ink, a);

            // Header across the top.
            UIKit.Label(new Rect(0f, 40f, W, 60f), "Where to, Captain?", UIKit.Style(44, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, a)), true);

            if (sailing) UIKit.Fill(new Rect(0f, 0f, W, H), new Color(1f, 0.97f, 0.9f, Mathf.Clamp01(sailTime / 0.5f) * 0.4f));
        }

        void DrawPinMarker(LevelEntry e, float a)
        {
            var g = Globe;
            if (g == null) return;
            var cam = game.CameraRig.Cam;
            Vector3 world = g.PinWorldPosition(index);
            Vector3 sp = cam.WorldToScreenPoint(world + (world - g.transform.position).normalized * 0.9f);
            if (sp.z <= 0f) return;
            float gx = sp.x / UIKit.Scale, gy = (Screen.height - sp.y) / UIKit.Scale;
            float settle = Mathf.Clamp01(changeTime / 0.5f);
            if (settle < 0.6f) return; // wait until the globe has mostly turned
            float b = a * Mathf.Clamp01((settle - 0.6f) / 0.4f);
            float ringPulse = 1f + 0.15f * Mathf.Sin(Time.unscaledTime * 5f);
            UIKit.CircleOutline(new Vector2(gx, gy), 70f * ringPulse, new Color(1f, 1f, 1f, 0.8f * b), 4f);
        }
    }
}

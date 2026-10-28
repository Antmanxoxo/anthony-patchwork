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
                return c + new Vector3(0.4f, 0.9f, -6.4f);
            }
        }

        public void Open()
        {
            index = Mathf.Clamp(lastIndex, 0, LevelCatalog.Levels.Length - 1);
            openTime = 0f;
            stickCooldown = 0.2f;
            changeTime = 0f;
            sailing = false;
            var cam = game.CameraRig;
            Vector3 c = Globe != null ? Globe.transform.position : Vector3.zero;
            cam.ShowcasePosition = ViewPoint;
            // Look a little to the right of the globe so it sits on the left of the screen, beside the card.
            cam.ShowcaseLookAt = c + new Vector3(2.3f, 0.1f, 0f);
            cam.ShowcaseBlendSpeed = 1.4f;
            cam.Showcase = true;
            if (Globe != null) Globe.Focus(index, ViewPoint);
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

            if (input.Cancel.WasPressedThisFrame())
            {
                game.CloseLevelSelect();
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
                    Sfx.Play(SfxId.PortalWarp, 0.5f);
                }
                else
                {
                    shake = 1f;
                    Sfx.Play(SfxId.TransformFail, 0.4f);
                }
            }
        }

        public void Draw(bool kb)
        {
            float W = UIKit.Width, H = UIKit.Height;
            float a = Mathf.Clamp01(openTime / 0.4f);
            var e = LevelCatalog.Levels[index];
            var ink = UIKit.Ink;

            // Pin marker on the globe.
            DrawPinMarker(e, a);

            // The card on the right.
            float pop = UIKit.EaseOutBack(Mathf.Clamp01(changeTime / 0.3f));
            float sx = Mathf.Sin(shake * 40f) * 14f * shake;
            float cw = 640f, ch = 600f;
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
                // Threads.
                int total;
                int best = GameState.BestThreads(e.Scene, out total);
                UIKit.Label(new Rect(card.x + 40f, y + 52f, card.width - 80f, 36f),
                    total > 0 ? "Best threads   " + best + " / " + total : "Best threads   -",
                    UIKit.Style(22, true, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, 0.8f * a)));
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
            px += UIKit.PromptLabel(px, py, 44f, PromptAction.Confirm, e.Playable ? "Set sail" : "Locked", kb, ink, a) + 36f;
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

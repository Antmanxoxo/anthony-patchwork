using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// On-screen HUD: current form badge with ability hints, golden buttons, control reminders,
    /// tutorial cards, toasts, the anchor prompt, plus whichever menu is open.
    /// </summary>
    public class PatchworkHUD : MonoBehaviour
    {
        public PatchworkGame Game;

        void OnGUI()
        {
            if (Game == null || Game.Player == null) return;
            UIKit.Begin();
            bool kb = Game.Input.UsingKeyboard;

            if (Game.Mode != UIMode.Loading && Game.Mode != UIMode.Warping)
            {
                DrawAnchorPrompt(kb);
                DrawFormBadge(kb);
                DrawButtons();
                DrawControls(kb);
                DrawHintCard();
                DrawToast();
                DrawTitle();
            }

            switch (Game.Mode)
            {
                case UIMode.FormWheel: Game.Selector.Draw(); break;
                case UIMode.Satchel: Game.Satchel.Draw(); break;
                case UIMode.Paused: Game.Pause.Draw(); break;
                case UIMode.Complete: DrawComplete(kb); break;
            }

            if (Game.Fade > 0.001f)
            {
                Color fc = Game.FadeColor;
                UIKit.Fill(new Rect(0f, 0f, UIKit.Width, UIKit.Height), new Color(fc.r, fc.g, fc.b, Game.Fade));
            }
        }

        void DrawFormBadge(bool kb)
        {
            var forms = Game.Player.Forms;
            var def = forms.Definition;
            var ink = UIKit.Ink;
            var panel = new Rect(28f, UIKit.Height - 206f, 640f, 178f);
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
                        UIKit.Style(20, false, TextAnchor.UpperLeft, ink, true));
                    break;

                case FormType.Yarn:
                {
                    var yarn = forms.Get<YarnAbility>();
                    if (yarn.Attached)
                    {
                        UIKit.PromptLabel(x, y, h, PromptAction.Ability, "Release to launch", kb, ink);
                        UIKit.PromptLabel(x, y + 48f, h, PromptAction.Jump, "Jump off", kb, ink);
                    }
                    else
                    {
                        bool ready = yarn.Target != null;
                        UIKit.PromptLabel(x, y, h, PromptAction.Ability, ready ? "Hold to swing!" : "Hold near a pink anchor to swing", kb, ink, ready ? 1f : 0.55f);
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

        void DrawButtons()
        {
            var level = Game.Level;
            int total = level.CollectibleIds.Count;
            if (total == 0) return;
            var r = new Rect(28f, 24f, 230f, 84f);
            UIKit.Panel(r, UIKit.Paper, UIKit.Thread, 20f);
            UIKit.Icon(new Rect(r.x + 18f, r.y + 14f, 56f, 56f), PatchworkAssets.IconButton, Color.white);
            UIKit.Label(new Rect(r.x + 88f, r.y + 14f, 130f, 56f), level.CollectedCount() + " / " + total,
                UIKit.Style(34, true, TextAnchor.MiddleLeft, UIKit.Ink));
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
            else text = a == PromptAction.FormWheel ? "L1" : a == PromptAction.Satchel ? "TOUCHPAD" : "OPTIONS";
            var s = UIKit.Style(Mathf.RoundToInt(h * 0.42f), true, TextAnchor.MiddleCenter, Color.white);
            return Mathf.Max(h, UIKit.TextWidth(text, s) + h * 0.55f);
        }

        void DrawHintCard()
        {
            var hint = Game.ActiveHint;
            if (hint == null || Game.Mode != UIMode.Gameplay) return;
            float w = 760f;
            var r = new Rect((UIKit.Width - w) * 0.5f, 26f, w, 132f);
            UIKit.Panel(r, UIKit.Paper, UIKit.Thread);
            UIKit.Label(new Rect(r.x + 36f, r.y + 20f, w - 72f, 34f), hint.Title, UIKit.Style(26, true, TextAnchor.MiddleLeft, UIKit.Ink));
            UIKit.Label(new Rect(r.x + 36f, r.y + 56f, w - 72f, 70f), hint.Body,
                UIKit.Style(19, false, TextAnchor.UpperLeft, new Color(UIKit.Ink.r, UIKit.Ink.g, UIKit.Ink.b, 0.9f), true));
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
            var panel = new Rect((W - 640f) * 0.5f, (H - 380f) * 0.5f, 640f, 380f);
            UIKit.Panel(panel, new Color(0.99f, 0.95f, 0.88f, 0.97f), UIKit.Thread);
            var ink = UIKit.Ink;
            UIKit.Label(new Rect(panel.x, panel.y + 40f, panel.width, 60f), "Level Complete!", UIKit.Style(48, true, TextAnchor.MiddleCenter, ink));
            UIKit.Label(new Rect(panel.x, panel.y + 100f, panel.width, 36f), Game.Level.Title, UIKit.Style(22, false, TextAnchor.MiddleCenter, ink));
            UIKit.Icon(new Rect(panel.x + 200f, panel.y + 150f, 90f, 90f), PatchworkAssets.IconButton, Color.white);
            UIKit.Label(new Rect(panel.x + 305f, panel.y + 150f, 200f, 90f),
                Game.Level.CollectedCount() + " / " + Game.Level.CollectibleIds.Count, UIKit.Style(44, true, TextAnchor.MiddleLeft, ink));
            float x = panel.x + 150f;
            UIKit.PromptLabel(x, panel.yMax - 90f, 44f, PromptAction.Confirm, "Back to the Sewing Room", kb, ink);
        }
    }
}

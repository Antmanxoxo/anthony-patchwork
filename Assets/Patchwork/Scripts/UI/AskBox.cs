using UnityEngine;

namespace Patchwork
{
    // A small Yes / No card in the middle of the screen. No is picked to start with, so a stray
    // button press never sends you anywhere. Left / right to choose, Confirm to answer, Back for No.
    public class AskBox
    {
        readonly PatchworkGame game;
        string title, body;
        System.Action onYes;
        bool yes;
        float time, sideCooldown;

        public AskBox(PatchworkGame game)
        {
            this.game = game;
        }

        public void Open(string title, string body, System.Action onYes)
        {
            this.title = title;
            this.body = body;
            this.onYes = onYes;
            yes = false;
            time = 0f;
            sideCooldown = 0f;
        }

        public System.Action Close()
        {
            var a = onYes;
            onYes = null;
            return a;
        }

        public void Update(float udt)
        {
            time += udt;
            var input = game.Input;
            // Ignore the first moment, so the jump that carried you in can't answer for you.
            if (time < 0.35f) return;

            if (input.Cancel.WasPressedThisFrame() || input.Pause.WasPressedThisFrame())
            {
                Sfx.Play(SfxId.MenuCancel, 0.4f);
                game.CloseAsk(false);
                return;
            }

            float sx = input.Move.ReadValue<Vector2>().x;
            bool left = input.NavLeft.WasPressedThisFrame() || input.NavUp.WasPressedThisFrame();
            bool right = input.NavRight.WasPressedThisFrame() || input.NavDown.WasPressedThisFrame();
            if (Mathf.Abs(sx) > 0.6f && sideCooldown <= 0f) { if (sx < 0f) left = true; else right = true; }
            if (Mathf.Abs(sx) < 0.3f) sideCooldown = 0f;
            sideCooldown -= udt;
            if (left || right)
            {
                sideCooldown = 0.25f;
                bool want = right;   // No on the left, Yes on the right
                if (want != yes)
                {
                    yes = want;
                    Sfx.Play(SfxId.MenuMove, 0.5f);
                }
            }

            if (input.Confirm.WasPressedThisFrame())
            {
                Sfx.Play(yes ? SfxId.MenuConfirm : SfxId.MenuCancel, 0.45f);
                game.CloseAsk(yes);
            }
        }

        public void Draw(bool kb)
        {
            float W = UIKit.Width, H = UIKit.Height;
            var ink = UIKit.Ink;
            float k = Mathf.Clamp01(time / 0.15f);
            UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.1f, 0.07f, 0.15f, 0.3f * k));

            float pw = 720f, ph = 330f;
            float pop = UIKit.EaseOutBack(time / 0.3f);
            var panel = new Rect((W - pw) * 0.5f, (H - ph) * 0.5f + (1f - pop) * 40f, pw, ph);
            UIKit.Panel(panel, new Color(0.99f, 0.95f, 0.88f, 0.98f * k), UIKit.Thread);
            UIKit.Label(new Rect(panel.x + 40f, panel.y + 32f, pw - 80f, 50f), title,
                UIKit.Style(32, true, TextAnchor.MiddleCenter, new Color(ink.r, ink.g, ink.b, k)));
            UIKit.Label(new Rect(panel.x + 60f, panel.y + 92f, pw - 120f, 70f), body,
                UIKit.Style(24, false, TextAnchor.UpperCenter, new Color(ink.r, ink.g, ink.b, 0.85f * k), true));

            float bw = 220f, bh = 64f, gap = 40f;
            float bx = panel.x + (pw - bw * 2f - gap) * 0.5f, by = panel.y + 176f;
            for (int i = 0; i < 2; i++)
            {
                bool isYes = i == 1;
                bool sel = isYes == yes;
                var r = new Rect(bx + i * (bw + gap), by, bw, bh);
                Color c = isYes ? new Color(0.55f, 0.45f, 0.85f) : new Color(0.35f, 0.7f, 0.5f);
                if (sel)
                {
                    float grow = 4f + Mathf.Sin(Time.unscaledTime * 4f) * 1.5f;
                    var g = new Rect(r.x - grow, r.y - grow * 0.5f, r.width + grow * 2f, r.height + grow);
                    UIKit.Fill(g, new Color(c.r, c.g, c.b, k), 20f);
                    PauseMenu.MarchingStitches(new Rect(g.x + 6f, g.y + 6f, g.width - 12f, g.height - 12f), new Color(1f, 1f, 1f, 0.85f * k), Time.unscaledTime * 40f);
                }
                else
                {
                    UIKit.Fill(r, new Color(1f, 1f, 1f, 0.7f * k), 18f);
                    UIKit.Outline(r, new Color(ink.r, ink.g, ink.b, 0.25f * k), 2f, 18f);
                }
                UIKit.Label(r, isYes ? "Yes" : "No", UIKit.Style(26, true, TextAnchor.MiddleCenter,
                    sel ? new Color(1f, 1f, 1f, k) : new Color(ink.r, ink.g, ink.b, k)));
            }

            float py = panel.yMax - 60f;
            float px = panel.x + 150f;
            px += UIKit.PromptLabel(px, py, 38f, PromptAction.Confirm, "Choose", kb, ink, k) + 30f;
            UIKit.PromptLabel(px, py, 38f, PromptAction.Cancel, "Stay here", kb, ink, k);
        }
    }
}

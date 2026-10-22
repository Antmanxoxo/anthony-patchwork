using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    /// <summary>Pause menu (Options / Esc). D-pad or stick to move, Cross to choose, Circle to resume.</summary>
    public class PauseMenu
    {
        readonly PatchworkGame game;
        readonly List<string> items = new List<string>();
        int index;
        float openTime;
        float stickCooldown;

        public PauseMenu(PatchworkGame game)
        {
            this.game = game;
        }

        public void Open()
        {
            items.Clear();
            items.Add("Resume");
            items.Add("Restart from Checkpoint");
            if (!game.Level.IsHub) items.Add("Return to the Sewing Room");
            items.Add("Quit");
            index = 0;
            openTime = 0f;
            stickCooldown = 0f;
        }

        public void Update(float udt)
        {
            openTime += udt;
            stickCooldown -= udt;
            var input = game.Input;

            if (openTime > 0.05f && (input.Pause.WasPressedThisFrame() || input.Cancel.WasPressedThisFrame()))
            {
                game.ClosePause();
                return;
            }

            int dir = 0;
            if (input.NavUp.WasPressedThisFrame()) dir = -1;
            if (input.NavDown.WasPressedThisFrame()) dir = 1;
            float sy = input.Move.ReadValue<Vector2>().y;
            if (dir == 0 && Mathf.Abs(sy) > 0.6f && stickCooldown <= 0f) dir = sy > 0f ? -1 : 1;
            if (Mathf.Abs(sy) < 0.3f) stickCooldown = 0f;
            if (dir != 0)
            {
                stickCooldown = 0.25f;
                index = (index + dir + items.Count) % items.Count;
                Sfx.Play(SfxId.MenuMove, 0.5f);
            }

            if (input.Confirm.WasPressedThisFrame())
            {
                Sfx.Play(SfxId.MenuConfirm, 0.5f);
                string choice = items[index];
                if (choice == "Resume") game.ClosePause();
                else if (choice == "Restart from Checkpoint")
                {
                    game.ClosePause();
                    game.Respawn(false);
                }
                else if (choice == "Return to the Sewing Room") game.LoadScene(HubBuilder.SceneName);
                else if (choice == "Quit") game.QuitGame();
            }
        }

        public void Draw()
        {
            bool kb = game.Input.UsingKeyboard;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(openTime / 0.12f));
            float W = UIKit.Width, H = UIKit.Height;
            UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.12f, 0.08f, 0.18f, 0.5f * k));

            float pw = 560f, ph = 140f + items.Count * 72f + 70f;
            var panel = new Rect((W - pw) * 0.5f, (H - ph) * 0.5f, pw, ph);
            UIKit.Panel(panel, new Color(0.99f, 0.95f, 0.88f, 0.97f), UIKit.Thread);
            var ink = UIKit.Ink;
            UIKit.Label(new Rect(panel.x, panel.y + 34f, pw, 56f), "Paused", UIKit.Style(44, true, TextAnchor.MiddleCenter, ink));

            float y = panel.y + 120f;
            for (int i = 0; i < items.Count; i++)
            {
                var r = new Rect(panel.x + 50f, y, pw - 100f, 58f);
                bool sel = i == index;
                UIKit.Fill(r, sel ? new Color(0.93f, 0.45f, 0.58f, 0.9f) : new Color(1f, 1f, 1f, 0.55f), 16f);
                UIKit.Label(r, items[i], UIKit.Style(24, true, TextAnchor.MiddleCenter, sel ? Color.white : ink));
                y += 72f;
            }

            float py = panel.yMax - 62f;
            float px = panel.x + 60f;
            px += UIKit.PromptLabel(px, py, 38f, PromptAction.Confirm, "Select", kb, ink) + 30f;
            UIKit.PromptLabel(px, py, 38f, PromptAction.Cancel, "Resume", kb, ink);
        }
    }
}

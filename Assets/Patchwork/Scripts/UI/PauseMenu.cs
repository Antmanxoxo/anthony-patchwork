using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    /// <summary>Pause menu (Options / Esc). D-pad or stick to move, Cross to choose, Circle to resume.</summary>
    public class PauseMenu
    {
        enum Item { Resume, Restart, ReturnToHub, Prompts, Music, Quit }

        readonly PatchworkGame game;
        readonly List<Item> items = new List<Item>();
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
            items.Add(Item.Resume);
            items.Add(Item.Restart);
            if (!game.Level.IsHub) items.Add(Item.ReturnToHub);
            items.Add(Item.Prompts);
            items.Add(Item.Music);
            items.Add(Item.Quit);
            index = 0;
            openTime = 0f;
            stickCooldown = 0f;
        }

        string LabelFor(Item item)
        {
            switch (item)
            {
                case Item.Resume: return "Resume";
                case Item.Restart: return "Restart from Checkpoint";
                case Item.ReturnToHub: return "Return to the Sewing Room";
                case Item.Prompts:
                {
                    var mode = game.Input.Prompts;
                    string m = mode == PatchworkInput.PromptMode.Auto ? "Auto" : mode == PatchworkInput.PromptMode.PlayStation ? "PlayStation" : "Keyboard";
                    return "Button Prompts:  " + m;
                }
                case Item.Music: return "Music:  " + (MusicPlayer.Enabled ? "On" : "Off");
                default: return "Quit";
            }
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
                switch (items[index])
                {
                    case Item.Resume:
                        game.ClosePause();
                        break;
                    case Item.Restart:
                        game.ClosePause();
                        game.Respawn(false);
                        break;
                    case Item.ReturnToHub:
                        game.LoadScene(HubBuilder.SceneName);
                        break;
                    case Item.Prompts:
                        input.Prompts = (PatchworkInput.PromptMode)(((int)input.Prompts + 1) % 3);
                        break;
                    case Item.Music:
                        MusicPlayer.Enabled = !MusicPlayer.Enabled;
                        break;
                    case Item.Quit:
                        game.QuitGame();
                        break;
                }
            }
        }

        public void Draw()
        {
            bool kb = game.Input.UsingKeyboard;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(openTime / 0.12f));
            float W = UIKit.Width, H = UIKit.Height;
            UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.1f, 0.07f, 0.15f, 0.62f * k));

            float pw = 600f, ph = 140f + items.Count * 68f + 70f;
            var panel = new Rect((W - pw) * 0.5f, (H - ph) * 0.5f, pw, ph);
            UIKit.Panel(panel, new Color(0.99f, 0.95f, 0.88f, 0.98f), UIKit.Thread);
            var ink = UIKit.Ink;
            UIKit.Label(new Rect(panel.x, panel.y + 34f, pw, 56f), "Paused", UIKit.Style(44, true, TextAnchor.MiddleCenter, ink));

            float y = panel.y + 116f;
            for (int i = 0; i < items.Count; i++)
            {
                var r = new Rect(panel.x + 50f, y, pw - 100f, 56f);
                bool sel = i == index;
                UIKit.Fill(r, sel ? new Color(0.8f, 0.3f, 0.45f, 1f) : new Color(1f, 1f, 1f, 0.7f), 16f);
                if (!sel) UIKit.Outline(r, new Color(ink.r, ink.g, ink.b, 0.25f), 2f, 16f);
                UIKit.Label(r, LabelFor(items[i]), UIKit.Style(23, true, TextAnchor.MiddleCenter, sel ? Color.white : ink));
                y += 68f;
            }

            float py = panel.yMax - 62f;
            float px = panel.x + 70f;
            px += UIKit.PromptLabel(px, py, 38f, PromptAction.Confirm, "Select", kb, ink) + 30f;
            UIKit.PromptLabel(px, py, 38f, PromptAction.Cancel, "Resume", kb, ink);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// Pause menu (Options / Esc). Drops in with a bounce, options slide in one after another,
    /// and the highlighted option glows, grows and plays its own little icon animation:
    /// Resume pulses, Restart spins, the house hops, the controller wiggles, the music note dances,
    /// the door swings. D-pad/stick to move, Cross to choose, Circle to resume.
    /// </summary>
    public class PauseMenu
    {
        enum Item { Resume, Restart, ReturnToHub, Prompts, Music, Quit }

        readonly PatchworkGame game;
        readonly List<Item> items = new List<Item>();
        int index;
        float openTime;
        float stickCooldown;
        float highlightPos;     // animated position of the highlight (in item slots)
        float selectTime;       // time since the current item became selected
        float confirmFlash;

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
            highlightPos = 0f;
            selectTime = 0f;
            openTime = 0f;
            stickCooldown = 0f;
            confirmFlash = 0f;
            Sfx.Play(SfxId.PauseOpen, 0.45f);
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

        static Texture2D IconFor(Item item)
        {
            switch (item)
            {
                case Item.Resume: return UIArt.IconPlay;
                case Item.Restart: return UIArt.IconRestart;
                case Item.ReturnToHub: return UIArt.IconHome;
                case Item.Prompts: return UIArt.IconPad;
                case Item.Music: return UIArt.IconNote;
                default: return UIArt.IconDoor;
            }
        }

        static Color ColorFor(Item item)
        {
            switch (item)
            {
                case Item.Resume: return new Color(0.35f, 0.7f, 0.5f);
                case Item.Restart: return new Color(0.95f, 0.6f, 0.3f);
                case Item.ReturnToHub: return new Color(0.85f, 0.4f, 0.55f);
                case Item.Prompts: return new Color(0.4f, 0.55f, 0.9f);
                case Item.Music: return new Color(0.65f, 0.45f, 0.85f);
                default: return new Color(0.6f, 0.5f, 0.45f);
            }
        }

        public void Update(float udt)
        {
            openTime += udt;
            selectTime += udt;
            stickCooldown -= udt;
            confirmFlash = Mathf.MoveTowards(confirmFlash, 0f, udt * 3f);
            highlightPos = Mathf.Lerp(highlightPos, index, 1f - Mathf.Exp(-16f * udt));
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
                selectTime = 0f;
                Sfx.Play(SfxId.MenuMove, 0.5f);
            }

            if (input.Confirm.WasPressedThisFrame())
            {
                Sfx.Play(SfxId.MenuConfirm, 0.5f);
                confirmFlash = 1f;
                selectTime = 0f;
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
            float W = UIKit.Width, H = UIKit.Height;
            float t = Time.unscaledTime;
            float fade = Mathf.Clamp01(openTime / 0.2f);
            UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.1f, 0.07f, 0.15f, 0.62f * fade));

            // Panel drops in from above with a bounce.
            const float rowH = 66f, rowGap = 10f;
            float pw = 640f, ph = 150f + items.Count * (rowH + rowGap) + 64f;
            float drop = UIKit.EaseOutBack(openTime / 0.4f);
            float py0 = (H - ph) * 0.5f;
            var panel = new Rect((W - pw) * 0.5f, Mathf.LerpUnclamped(-ph - 40f, py0, drop), pw, ph);
            UIKit.Panel(panel, new Color(0.99f, 0.95f, 0.88f, 0.98f), UIKit.Thread);
            UIKit.PatternFill(new Rect(panel.x + 20f, panel.y + 20f, panel.width - 40f, panel.height - 40f), UIArt.PatternDots,
                new Color(0.85f, 0.6f, 0.4f, 0.08f), 48f, new Vector2(t * 0.02f, 0f));
            var ink = UIKit.Ink;

            // Title with a little sewing-needle bob.
            UIKit.Label(new Rect(panel.x, panel.y + 30f, pw, 60f), "Paused", UIKit.Style(46, true, TextAnchor.MiddleCenter, ink));
            float needleX = panel.x + pw * 0.5f + 110f + Mathf.Sin(t * 3f) * 6f;
            UIKit.IconTransformed(new Rect(needleX, panel.y + 42f, 60f, 36f), UIArt.IconNeedle, new Color(0.6f, 0.6f, 0.68f), -20f + Mathf.Sin(t * 2f) * 6f, 1f);

            float top = panel.y + 118f;
            float rowX = panel.x + 44f, rowW = pw - 88f;

            // Sliding highlight behind the selected row.
            var selItem = items[index];
            Color selColor = ColorFor(selItem);
            float hy = top + highlightPos * (rowH + rowGap);
            float pop = UIKit.EaseOutBack(selectTime / 0.25f);
            float grow = 6f * pop + Mathf.Sin(t * 4f) * 1.5f + confirmFlash * 8f;
            var hl = new Rect(rowX - grow, hy - grow * 0.5f, rowW + grow * 2f, rowH + grow);
            UIKit.Fill(new Rect(hl.x + 3f, hl.y + 6f, hl.width, hl.height), new Color(0f, 0f, 0f, 0.2f * fade), 20f);
            UIKit.Fill(hl, new Color(selColor.r, selColor.g, selColor.b, fade), 20f);
            UIKit.Fill(new Rect(hl.x, hl.y, hl.width, hl.height * 0.45f), new Color(1f, 1f, 1f, 0.12f * fade), 20f);
            MarchingStitches(new Rect(hl.x + 7f, hl.y + 7f, hl.width - 14f, hl.height - 14f), new Color(1f, 1f, 1f, 0.85f * fade), t * 40f);

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                bool sel = i == index;
                // Staggered slide-in from the left.
                float appear = UIKit.EaseOutBack((openTime - 0.12f - i * 0.05f) / 0.3f);
                float offset = (1f - appear) * -60f + (sel ? 14f * pop : 0f);
                float alpha = Mathf.Clamp01(appear) * fade;
                float y = top + i * (rowH + rowGap);
                var r = new Rect(rowX + offset, y, rowW, rowH);

                if (!sel)
                {
                    UIKit.Fill(r, new Color(1f, 1f, 1f, 0.7f * alpha), 18f);
                    UIKit.Outline(r, new Color(ink.r, ink.g, ink.b, 0.2f * alpha), 2f, 18f);
                }

                // Icon (animated when selected).
                Color c = ColorFor(item);
                var iconRect = new Rect(r.x + 18f, r.y + 11f, 44f, 44f);
                float angle = 0f, scale = 1f;
                Vector2 shift = Vector2.zero;
                if (sel) AnimateIcon(item, selectTime, ref angle, ref scale, ref shift);
                iconRect.position += shift;
                if (!sel) UIKit.Circle(iconRect.center, 52f, new Color(c.r, c.g, c.b, 0.9f * alpha));
                UIKit.IconTransformed(iconRect, IconFor(item), new Color(1f, 1f, 1f, alpha), angle, scale);
                if (sel && item == Item.Music) DrawFloatingNotes(iconRect, alpha);

                var style = UIKit.Style(sel ? 25 : 23, true, TextAnchor.MiddleLeft, sel ? new Color(1f, 1f, 1f, alpha) : new Color(ink.r, ink.g, ink.b, alpha));
                UIKit.Label(new Rect(r.x + 82f, r.y, r.width - 100f, r.height), LabelFor(item), style, sel);
            }

            // Prompt row.
            float py = panel.yMax - 58f;
            float px = panel.x + 70f;
            px += UIKit.PromptLabel(px, py, 38f, PromptAction.Confirm, "Select", kb, ink, fade) + 30f;
            UIKit.PromptLabel(px, py, 38f, PromptAction.Cancel, "Back to game", kb, ink, fade);
        }

        static void AnimateIcon(Item item, float s, ref float angle, ref float scale, ref Vector2 shift)
        {
            switch (item)
            {
                case Item.Resume:       // pulse and nudge forward
                    scale = 1f + Mathf.Sin(s * 7f) * 0.12f;
                    shift = new Vector2(Mathf.Abs(Mathf.Sin(s * 3.5f)) * 5f, 0f);
                    break;
                case Item.Restart:      // spin
                    angle = -s * 220f;
                    break;
                case Item.ReturnToHub:  // hop
                    shift = new Vector2(0f, -Mathf.Abs(Mathf.Sin(s * 6f)) * 9f);
                    scale = 1f + Mathf.Max(0f, -Mathf.Sin(s * 6f)) * 0.06f;
                    break;
                case Item.Prompts:      // wiggle like it's being played
                    angle = Mathf.Sin(s * 18f) * 12f * Mathf.Clamp01(1.2f - Mathf.Repeat(s, 1.5f));
                    break;
                case Item.Music:        // dance
                    angle = Mathf.Sin(s * 5f) * 15f;
                    shift = new Vector2(0f, Mathf.Sin(s * 10f) * 4f);
                    break;
                default:                // door swings open and shut
                    scale = 1f;
                    angle = 0f;
                    shift = new Vector2(Mathf.Sin(s * 4f) * 3f, 0f);
                    break;
            }
        }

        static void DrawFloatingNotes(Rect iconRect, float alpha)
        {
            float t = Time.unscaledTime;
            for (int i = 0; i < 3; i++)
            {
                float k = Mathf.Repeat(t * 0.8f + i * 0.33f, 1f);
                var r = new Rect(iconRect.xMax - 6f + k * 18f + Mathf.Sin(t * 4f + i) * 4f, iconRect.y - k * 40f, 20f, 20f);
                UIKit.IconTransformed(r, UIArt.IconNote, new Color(1f, 1f, 1f, alpha * (1f - k)), Mathf.Sin(t * 3f + i) * 20f, 1f);
            }
        }

        /// <summary>A running-stitch border whose dashes march around the rectangle.</summary>
        static void MarchingStitches(Rect r, Color c, float phase)
        {
            const float dash = 12f, gap = 8f, thick = 3f;
            float period = dash + gap;
            float per = (r.width + r.height) * 2f;
            for (float d = -Mathf.Repeat(phase, period); d < per; d += period)
            {
                float a = Mathf.Max(0f, d), b = Mathf.Min(per, d + dash);
                if (b <= a) continue;
                DrawAlongPerimeter(r, a, b, c, thick);
            }
        }

        static void DrawAlongPerimeter(Rect r, float a, float b, Color c, float thick)
        {
            float w = r.width, h = r.height;
            float[] edges = { w, h, w, h };
            float start = 0f;
            for (int e = 0; e < 4; e++)
            {
                float s0 = Mathf.Max(a, start), s1 = Mathf.Min(b, start + edges[e]);
                if (s1 > s0)
                {
                    float p0 = s0 - start, p1 = s1 - start;
                    switch (e)
                    {
                        case 0: UIKit.Fill(new Rect(r.x + p0, r.y, p1 - p0, thick), c, 1.5f); break;
                        case 1: UIKit.Fill(new Rect(r.xMax - thick, r.y + p0, thick, p1 - p0), c, 1.5f); break;
                        case 2: UIKit.Fill(new Rect(r.xMax - p1, r.yMax - thick, p1 - p0, thick), c, 1.5f); break;
                        default: UIKit.Fill(new Rect(r.x, r.yMax - p1, thick, p1 - p0), c, 1.5f); break;
                    }
                }
                start += edges[e];
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Pause menu. Drops in with a bounce and each option has its own little icon animation
    // when it's highlighted. D-pad/stick to move, X to pick, Circle to go back.
    // Settings live on their own page (sound, rumble, camera, controls, VSync).
    public class PauseMenu
    {
        enum Item { Resume, Restart, ReturnToHub, Settings, Quit,
                    Music, Sounds, Rumble, CameraSpeed, InvertY, InvertX, SwingMode, Prompts, WheelButton, VSync, Back }

        readonly PatchworkGame game;
        readonly List<Item> items = new List<Item>();
        int index;
        float openTime;
        float stickCooldown;
        float highlightPos;     // animated position of the highlight (in item slots)
        float selectTime;       // time since the current item became selected
        float confirmFlash;
        float sideCooldown;
        bool settingsPage;

        // "Are you sure?" card for options that lose progress. Starts on No.
        bool confirming;
        Item confirmItem;
        bool confirmYes;
        float confirmTime;

        public PauseMenu(PatchworkGame game)
        {
            this.game = game;
        }

        public void Open()
        {
            ShowMainPage(0);
            highlightPos = 0f;
            selectTime = 0f;
            openTime = 0f;
            stickCooldown = 0f;
            confirmFlash = 0f;
            confirming = false;
            Sfx.Play(SfxId.PauseOpen, 0.45f);
        }

        void ShowMainPage(int select)
        {
            settingsPage = false;
            items.Clear();
            items.Add(Item.Resume);
            items.Add(Item.Restart);
            if (!game.Level.IsHub) items.Add(Item.ReturnToHub);
            items.Add(Item.Settings);
            items.Add(Item.Quit);
            index = Mathf.Clamp(select, 0, items.Count - 1);
            selectTime = 0f;
        }

        void ShowSettingsPage()
        {
            settingsPage = true;
            items.Clear();
            items.Add(Item.Music);
            items.Add(Item.Sounds);
            items.Add(Item.Rumble);
            items.Add(Item.CameraSpeed);
            items.Add(Item.InvertY);
            items.Add(Item.InvertX);
            items.Add(Item.SwingMode);
            items.Add(Item.Prompts);
            items.Add(Item.WheelButton);
            items.Add(Item.VSync);
            items.Add(Item.Back);
            index = 0;
            highlightPos = 0f;
            selectTime = 0f;
            openTime = 0.12f; // replay the row slide-in
        }

        void BackToMainPage()
        {
            ShowMainPage(items.Count > 0 ? 0 : 0);
            index = items.IndexOf(Item.Settings);
            highlightPos = index;
            Sfx.Play(SfxId.MenuCancel, 0.4f);
        }

        static string OnOff(bool on) { return on ? "On" : "Off"; }

        string LabelFor(Item item)
        {
            switch (item)
            {
                case Item.Settings: return "Settings";
                case Item.CameraSpeed: return "Camera Speed";
                case Item.InvertY: return "Invert Camera Up/Down:  " + OnOff(GameSettings.InvertY);
                case Item.InvertX: return "Invert Camera Left/Right:  " + OnOff(GameSettings.InvertX);
                case Item.SwingMode: return "Yarn Swing:  " + (GameSettings.SwingToggle ? "Tap on / tap off" : "Hold button");
                case Item.VSync: return "VSync:  " + OnOff(GameSettings.VSync);
                case Item.Back: return "Back";
                case Item.Resume: return "Resume";
                case Item.Restart: return "Restart from Checkpoint";
                case Item.ReturnToHub: return "Return to the Sewing Room";
                case Item.Prompts:
                {
                    var mode = game.Input.Prompts;
                    string m = mode == PatchworkInput.PromptMode.Auto ? "Auto" : mode == PatchworkInput.PromptMode.PlayStation ? "PlayStation" : "Keyboard";
                    return "Button Prompts:  " + m;
                }
                case Item.WheelButton: return "Form Wheel Button:  " + (game.Input.FormWheelOnR1 ? "R1" : "L1");
                case Item.Music: return "Music Volume";
                case Item.Sounds: return "Sound Volume";
                case Item.Rumble: return "Rumble";
                default: return "Quit";
            }
        }

        static Texture2D IconFor(Item item)
        {
            switch (item)
            {
                case Item.Resume:
                case Item.Back: return UIArt.IconPlay;
                case Item.Settings: return UIArt.IconNeedle;
                case Item.CameraSpeed:
                case Item.InvertY:
                case Item.InvertX:
                case Item.SwingMode:
                case Item.VSync: return UIArt.IconPad;
                case Item.Restart: return UIArt.IconRestart;
                case Item.ReturnToHub: return UIArt.IconHome;
                case Item.Prompts:
                case Item.WheelButton: return UIArt.IconPad;
                case Item.Music:
                case Item.Sounds: return UIArt.IconNote;
                case Item.Rumble: return UIArt.IconPad;
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
                case Item.WheelButton: return new Color(0.3f, 0.65f, 0.75f);
                case Item.Music: return new Color(0.65f, 0.45f, 0.85f);
                case Item.Sounds: return new Color(0.85f, 0.45f, 0.65f);
                case Item.Rumble: return new Color(0.45f, 0.6f, 0.4f);
                case Item.Settings: return new Color(0.55f, 0.5f, 0.8f);
                case Item.CameraSpeed: return new Color(0.35f, 0.6f, 0.85f);
                case Item.InvertY:
                case Item.InvertX: return new Color(0.4f, 0.5f, 0.8f);
                case Item.SwingMode: return new Color(0.9f, 0.45f, 0.58f);
                case Item.VSync: return new Color(0.5f, 0.65f, 0.6f);
                case Item.Back: return new Color(0.35f, 0.7f, 0.5f);
                default: return new Color(0.6f, 0.5f, 0.45f);
            }
        }

        static bool IsSlider(Item item)
        {
            return item == Item.Music || item == Item.Sounds || item == Item.Rumble || item == Item.CameraSpeed;
        }

        // On/off and multiple-choice settings: X or left/right flips them.
        static bool IsToggle(Item item)
        {
            return item == Item.InvertY || item == Item.InvertX || item == Item.SwingMode || item == Item.VSync
                || item == Item.Prompts || item == Item.WheelButton;
        }

        void Toggle(Item item, int dir)
        {
            var input = game.Input;
            switch (item)
            {
                case Item.InvertY: GameSettings.InvertY = !GameSettings.InvertY; break;
                case Item.InvertX: GameSettings.InvertX = !GameSettings.InvertX; break;
                case Item.SwingMode: GameSettings.SwingToggle = !GameSettings.SwingToggle; break;
                case Item.VSync: GameSettings.VSync = !GameSettings.VSync; GameSettings.ApplyVSync(); break;
                case Item.Prompts: input.Prompts = (PatchworkInput.PromptMode)(((int)input.Prompts + (dir < 0 ? 2 : 1)) % 3); break;
                case Item.WheelButton: input.FormWheelOnR1 = !input.FormWheelOnR1; break;
            }
            PlayerPrefs.Save();
            Sfx.Play(SfxId.MenuMove, 0.5f);
            selectTime = 0f;
        }

        static int SliderValue(Item item)
        {
            switch (item)
            {
                case Item.Music: return MusicPlayer.Volume;
                case Item.Sounds: return Sfx.Volume;
                case Item.CameraSpeed: return GameSettings.CameraSpeed;
                default: return Haptics.Strength;
            }
        }

        // Change a 0..10 setting and give a taste of the new level straight away. The little tick
        // climbs in pitch as the value goes up and drops as it goes down.
        void SetSlider(Item item, int value)
        {
            value = Mathf.Clamp(value, item == Item.CameraSpeed ? 1 : 0, 10);
            switch (item)
            {
                case Item.Music: MusicPlayer.Volume = value; break;
                case Item.Sounds: Sfx.Volume = value; break;
                case Item.CameraSpeed: GameSettings.CameraSpeed = value; break;
                default: Haptics.Strength = value; Haptics.Preview(); break;
            }
            PlayerPrefs.Save();
            float pitch = Mathf.Lerp(0.7f, 1.6f, value / 10f);
            // The sound slider's tick also gets louder or quieter with it; the others stay at a
            // fixed loudness (and still make a quiet tick at 0 so you know it registered).
            Sfx.PlayPitched(SfxId.MenuMove, item == Item.Sounds ? 0.7f : 0.5f, pitch);
        }

        public void Update(float udt)
        {
            openTime += udt;
            selectTime += udt;
            stickCooldown -= udt;
            confirmFlash = Mathf.MoveTowards(confirmFlash, 0f, udt * 3f);
            highlightPos = Mathf.Lerp(highlightPos, index, 1f - Mathf.Exp(-16f * udt));
            var input = game.Input;

            if (confirming)
            {
                UpdateConfirm(udt);
                return;
            }

            if (openTime > 0.05f && settingsPage && input.Cancel.WasPressedThisFrame())
            {
                BackToMainPage();
                return;
            }
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

            // Left / right change the sliders and flip the on/off settings.
            var cur = items[index];
            if (IsSlider(cur) || IsToggle(cur))
            {
                int side = 0;
                if (input.NavLeft.WasPressedThisFrame()) side = -1;
                if (input.NavRight.WasPressedThisFrame()) side = 1;
                float sx = input.Move.ReadValue<Vector2>().x;
                if (side == 0 && Mathf.Abs(sx) > 0.6f && sideCooldown <= 0f) side = sx > 0f ? 1 : -1;
                if (Mathf.Abs(sx) < 0.3f) sideCooldown = 0f;
                sideCooldown -= udt;
                if (side != 0)
                {
                    sideCooldown = IsToggle(cur) ? 0.3f : 0.18f;
                    if (IsToggle(cur)) Toggle(cur, side);
                    else SetSlider(cur, SliderValue(cur) + side);
                    selectTime = 0f;
                }
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
                    case Item.ReturnToHub:
                    case Item.Quit:
                        OpenConfirm(items[index]);
                        break;
                    case Item.Settings:
                        ShowSettingsPage();
                        break;
                    case Item.Back:
                        BackToMainPage();
                        break;
                    case Item.Music:
                    case Item.Sounds:
                    case Item.Rumble:
                    case Item.CameraSpeed:
                        // Pressing X steps up and wraps back round to the bottom.
                        int v = SliderValue(items[index]);
                        SetSlider(items[index], v >= 10 ? 0 : v + 1);
                        break;
                    default:
                        if (IsToggle(items[index])) Toggle(items[index], 1);
                        break;
                }
            }
        }

        void OpenConfirm(Item item)
        {
            confirming = true;
            confirmItem = item;
            confirmYes = false;
            confirmTime = 0f;
        }

        void UpdateConfirm(float udt)
        {
            confirmTime += udt;
            var input = game.Input;
            if (confirmTime < 0.05f) return;

            if (input.Cancel.WasPressedThisFrame() || input.Pause.WasPressedThisFrame())
            {
                confirming = false;
                Sfx.Play(SfxId.MenuCancel, 0.4f);
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
                bool want = right; // No on the left, Yes on the right
                if (want != confirmYes)
                {
                    confirmYes = want;
                    Sfx.Play(SfxId.MenuMove, 0.5f);
                }
            }

            if (input.Confirm.WasPressedThisFrame())
            {
                confirming = false;
                if (!confirmYes)
                {
                    Sfx.Play(SfxId.MenuCancel, 0.4f);
                    return;
                }
                Sfx.Play(SfxId.MenuConfirm, 0.5f);
                switch (confirmItem)
                {
                    case Item.Restart:
                        game.ClosePause();
                        game.Respawn(false);
                        break;
                    case Item.ReturnToHub:
                        game.LoadScene(HubBuilder.SceneName);
                        break;
                    case Item.Quit:
                        game.QuitGame();
                        break;
                }
            }
        }

        void ConfirmText(out string title, out string body)
        {
            switch (confirmItem)
            {
                case Item.Restart:
                    title = "Back to the last checkpoint?";
                    body = "You'll start again from the last pin you touched.";
                    break;
                case Item.ReturnToHub:
                    title = "Head back to the Sewing Room?";
                    body = "Golden buttons are saved, but thread spools from this visit will be lost.";
                    break;
                default:
                    title = "Quit Patchwork?";
                    body = "Golden buttons are saved, but thread spools from this visit will be lost.";
                    break;
            }
        }

        void DrawConfirm(bool kb, Color ink)
        {
            float W = UIKit.Width, H = UIKit.Height;
            float k = Mathf.Clamp01(confirmTime / 0.15f);
            UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.1f, 0.07f, 0.15f, 0.45f * k));

            string title, body;
            ConfirmText(out title, out body);
            float pw = 720f, ph = 340f;
            float pop = UIKit.EaseOutBack(confirmTime / 0.3f);
            var panel = new Rect((W - pw) * 0.5f, (H - ph) * 0.5f + (1f - pop) * 40f, pw, ph);
            UIKit.Panel(panel, new Color(0.99f, 0.95f, 0.88f, 0.98f * k), UIKit.Thread);
            UIKit.Label(new Rect(panel.x + 40f, panel.y + 34f, pw - 80f, 50f), title,
                UIKit.Style(32, true, TextAnchor.MiddleCenter, new Color(ink.r, ink.g, ink.b, k)));
            UIKit.Label(new Rect(panel.x + 60f, panel.y + 94f, pw - 120f, 80f), body,
                UIKit.Style(24, false, TextAnchor.UpperCenter, new Color(ink.r, ink.g, ink.b, 0.85f * k), true));

            // No / Yes buttons.
            float bw = 220f, bh = 64f, gap = 40f;
            float bx = panel.x + (pw - bw * 2f - gap) * 0.5f, by = panel.y + 186f;
            for (int i = 0; i < 2; i++)
            {
                bool isYes = i == 1;
                bool sel = isYes == confirmYes;
                var r = new Rect(bx + i * (bw + gap), by, bw, bh);
                Color c = isYes ? new Color(0.9f, 0.45f, 0.45f) : new Color(0.35f, 0.7f, 0.5f);
                if (sel)
                {
                    float grow = 4f + Mathf.Sin(Time.unscaledTime * 4f) * 1.5f;
                    var g = new Rect(r.x - grow, r.y - grow * 0.5f, r.width + grow * 2f, r.height + grow);
                    UIKit.Fill(g, new Color(c.r, c.g, c.b, k), 20f);
                    MarchingStitches(new Rect(g.x + 6f, g.y + 6f, g.width - 12f, g.height - 12f), new Color(1f, 1f, 1f, 0.85f * k), Time.unscaledTime * 40f);
                }
                else
                {
                    UIKit.Fill(r, new Color(1f, 1f, 1f, 0.7f * k), 18f);
                    UIKit.Outline(r, new Color(ink.r, ink.g, ink.b, 0.25f * k), 2f, 18f);
                }
                UIKit.Label(r, isYes ? "Yes" : "No", UIKit.Style(26, true, TextAnchor.MiddleCenter,
                    sel ? new Color(1f, 1f, 1f, k) : new Color(ink.r, ink.g, ink.b, k)));
            }

            float py = panel.yMax - 62f;
            float px = panel.x + 150f;
            px += UIKit.PromptLabel(px, py, 38f, PromptAction.Confirm, "Choose", kb, ink, k) + 30f;
            UIKit.PromptLabel(px, py, 38f, PromptAction.Cancel, "Back", kb, ink, k);
        }

        public void Draw()
        {
            bool kb = game.Input.UsingKeyboard;
            float W = UIKit.Width, H = UIKit.Height;
            float t = Time.unscaledTime;
            float fade = Mathf.Clamp01(openTime / 0.2f);
            UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.1f, 0.07f, 0.15f, 0.62f * fade));

            // Panel drops in from above with a bounce.
            // The settings page has more rows, so they're a little tighter.
            float rowH = settingsPage ? 58f : 66f, rowGap = settingsPage ? 8f : 10f;
            float pw = 640f, ph = 150f + items.Count * (rowH + rowGap) + 64f;
            float drop = UIKit.EaseOutBack(openTime / 0.4f);
            float py0 = (H - ph) * 0.5f;
            var panel = new Rect((W - pw) * 0.5f, Mathf.LerpUnclamped(-ph - 40f, py0, drop), pw, ph);
            UIKit.Panel(panel, new Color(0.99f, 0.95f, 0.88f, 0.98f), UIKit.Thread);
            UIKit.PatternFill(new Rect(panel.x + 20f, panel.y + 20f, panel.width - 40f, panel.height - 40f), UIArt.PatternDots,
                new Color(0.85f, 0.6f, 0.4f, 0.08f), 48f, new Vector2(t * 0.02f, 0f));
            var ink = UIKit.Ink;

            // Title with a little sewing-needle bob.
            UIKit.Label(new Rect(panel.x, panel.y + 30f, pw, 60f), settingsPage ? "Settings" : "Paused", UIKit.Style(46, true, TextAnchor.MiddleCenter, ink));
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
                var iconRect = new Rect(r.x + 18f, r.y + (rowH - 44f) * 0.5f, 44f, 44f);
                float angle = 0f, scale = 1f;
                Vector2 shift = Vector2.zero;
                if (sel) AnimateIcon(item, selectTime, ref angle, ref scale, ref shift);
                if (item == Item.Back) angle += 180f; // the play arrow, pointing back
                iconRect.position += shift;
                if (!sel) UIKit.Circle(iconRect.center, 52f, new Color(c.r, c.g, c.b, 0.9f * alpha));
                UIKit.IconTransformed(iconRect, IconFor(item), new Color(1f, 1f, 1f, alpha), angle, scale);
                if (sel && (item == Item.Music || item == Item.Sounds)) DrawFloatingNotes(iconRect, alpha);

                var style = UIKit.Style(sel ? 25 : 23, true, TextAnchor.MiddleLeft, sel ? new Color(1f, 1f, 1f, alpha) : new Color(ink.r, ink.g, ink.b, alpha));
                UIKit.Label(new Rect(r.x + 82f, r.y, r.width - 100f, r.height), LabelFor(item), style, sel);
                if (IsSlider(item)) DrawSlider(new Rect(r.xMax - 236f, r.y + (rowH - 22f) * 0.5f, 210f, 22f), SliderValue(item), sel, alpha, ink);
            }

            // Prompt row.
            float py = panel.yMax - 58f;
            float px = panel.x + 70f;
            px += UIKit.PromptLabel(px, py, 38f, PromptAction.Confirm, "Select", kb, ink, fade) + 30f;
            UIKit.PromptLabel(px, py, 38f, PromptAction.Cancel, settingsPage ? "Back" : "Back to game", kb, ink, fade);

            if (confirming) DrawConfirm(kb, ink);
        }

        // Ten little stitched pills, filled up to the current value.
        static void DrawSlider(Rect r, int value, bool sel, float alpha, Color ink)
        {
            const int Steps = 10;
            float gap = 4f;
            float w = (r.width - gap * (Steps - 1)) / Steps;
            for (int i = 0; i < Steps; i++)
            {
                var pill = new Rect(r.x + i * (w + gap), r.y, w, r.height);
                bool on = i < value;
                Color fill = sel
                    ? new Color(1f, 1f, 1f, (on ? 0.95f : 0.25f) * alpha)
                    : new Color(ink.r, ink.g, ink.b, (on ? 0.7f : 0.15f) * alpha);
                UIKit.Fill(pill, fill, 5f);
            }
            if (value == 0)
            {
                var off = UIKit.Style(16, true, TextAnchor.MiddleRight, sel ? new Color(1f, 1f, 1f, alpha) : new Color(ink.r, ink.g, ink.b, alpha));
                UIKit.Label(new Rect(r.x - 60f, r.y - 2f, 50f, r.height + 4f), "OFF", off);
            }
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
                case Item.Settings:     // the needle wobbles like it's sewing
                    angle = Mathf.Sin(s * 6f) * 18f;
                    shift = new Vector2(0f, Mathf.Sin(s * 12f) * 3f);
                    break;
                case Item.Prompts:      // wiggle like it's being played
                case Item.WheelButton:
                case Item.Rumble:
                    angle = Mathf.Sin(s * 18f) * 12f * Mathf.Clamp01(1.2f - Mathf.Repeat(s, 1.5f));
                    break;
                case Item.Music:        // dance
                case Item.Sounds:
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

        // A running-stitch border whose dashes march around the rectangle.
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

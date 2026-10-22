using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// "Patch's Satchel" — the character-held HUD. Shows the three forms and what each does,
    /// golden buttons found, and the cosmetic hat (changed here, independent of form).
    /// Transforming is NOT done here; the Form Wheel stays the fast path.
    /// </summary>
    public class SatchelMenu
    {
        readonly PatchworkGame game;
        float openTime;
        float stickCooldown;

        public SatchelMenu(PatchworkGame game)
        {
            this.game = game;
        }

        public void Open()
        {
            openTime = 0f;
            stickCooldown = 0f;
        }

        public void Update(float udt)
        {
            openTime += udt;
            stickCooldown -= udt;
            var input = game.Input;

            if (input.Cancel.WasPressedThisFrame() || input.Satchel.WasPressedThisFrame())
            {
                game.CloseSatchel();
                return;
            }

            int dir = 0;
            if (input.NavLeft.WasPressedThisFrame()) dir = -1;
            if (input.NavRight.WasPressedThisFrame()) dir = 1;
            float sx = input.Move.ReadValue<Vector2>().x;
            if (dir == 0 && Mathf.Abs(sx) > 0.6f && stickCooldown <= 0f) dir = sx > 0f ? 1 : -1;
            if (Mathf.Abs(sx) < 0.3f) stickCooldown = 0f;

            if (dir != 0)
            {
                stickCooldown = 0.3f;
                int count = FormVisuals.HatNames.Length;
                int hat = ((GameState.HatIndex + dir) % count + count) % count;
                GameState.HatIndex = hat;
                GameState.Save();
                game.Player.Visuals.SetHat(hat);
                Sfx.Play(SfxId.MenuMove, 0.5f);
            }
        }

        public void Draw()
        {
            bool kb = game.Input.UsingKeyboard;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(openTime / 0.15f));
            float W = UIKit.Width, H = UIKit.Height;
            UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.12f, 0.08f, 0.18f, 0.4f * k));

            float pw = 1040f, ph = 660f;
            var panel = new Rect((W - pw) * 0.5f, (H - ph) * 0.5f + (1f - k) * 40f, pw, ph);
            UIKit.Panel(panel, new Color(0.99f, 0.95f, 0.88f, 0.97f), UIKit.Thread);
            var ink = UIKit.Ink;

            UIKit.Label(new Rect(panel.x + 50f, panel.y + 36f, 600f, 50f), "Patch's Satchel", UIKit.Style(40, true, TextAnchor.MiddleLeft, ink));
            UIKit.Label(new Rect(panel.x + 50f, panel.y + 84f, 700f, 30f), "Your forms, your finds, your hat. (Change form any time with the Form Wheel.)",
                UIKit.Style(18, false, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, 0.75f)));

            // Forms column.
            float y = panel.y + 135f;
            var current = game.Player.Forms.Current;
            foreach (var form in FormLibrary.AllForms)
            {
                var def = FormLibrary.Get(form);
                bool unlocked = GameState.IsFormUnlocked(form);
                var row = new Rect(panel.x + 40f, y, 600f, 128f);
                UIKit.Fill(row, form == current ? new Color(def.PrimaryColor.r, def.PrimaryColor.g, def.PrimaryColor.b, 0.28f) : new Color(1f, 1f, 1f, 0.5f), 18f);
                UIKit.Circle(new Vector2(row.x + 62f, row.y + 64f), 96f, unlocked ? def.PrimaryColor : new Color(0.6f, 0.6f, 0.62f));
                UIKit.Icon(new Rect(row.x + 30f, row.y + 32f, 64f, 64f), unlocked ? PatchworkAssets.IconFor(form) : PatchworkAssets.IconLock, Color.white);
                UIKit.Label(new Rect(row.x + 128f, row.y + 12f, 300f, 34f), def.DisplayName, UIKit.Style(26, true, TextAnchor.MiddleLeft, ink));
                string status = !unlocked ? "LOCKED" : form == current ? "WEARING" : "READY";
                var tag = new Rect(row.xMax - 130f, row.y + 16f, 110f, 28f);
                UIKit.Fill(tag, form == current ? ink : new Color(ink.r, ink.g, ink.b, 0.45f), 14f);
                UIKit.Label(tag, status, UIKit.Style(15, true, TextAnchor.MiddleCenter, Color.white));
                UIKit.Label(new Rect(row.x + 128f, row.y + 48f, 455f, 76f), def.AbilityHint,
                    UIKit.Style(16, false, TextAnchor.UpperLeft, new Color(ink.r, ink.g, ink.b, 0.85f), true));
                y += 140f;
            }

            // Right column: finds + hat.
            float rx = panel.x + 680f;
            float ry = panel.y + 135f;
            var box = new Rect(rx, ry, 320f, 190f);
            UIKit.Fill(box, new Color(1f, 1f, 1f, 0.5f), 18f);
            UIKit.Label(new Rect(rx + 24f, ry + 14f, 280f, 30f), "Golden Buttons", UIKit.Style(22, true, TextAnchor.MiddleLeft, ink));
            UIKit.Icon(new Rect(rx + 24f, ry + 60f, 90f, 90f), PatchworkAssets.IconButton, Color.white);
            var level = game.Level;
            string count;
            if (level.CollectibleIds.Count > 0) count = level.CollectedCount() + " / " + level.CollectibleIds.Count;
            else
            {
                int n = 0;
                foreach (var id in Level1Builder.CollectibleIds) if (GameState.IsCollected(id)) n++;
                count = n + " / " + Level1Builder.CollectibleIds.Length;
            }
            UIKit.Label(new Rect(rx + 130f, ry + 62f, 180f, 50f), count, UIKit.Style(40, true, TextAnchor.MiddleLeft, ink));
            UIKit.Label(new Rect(rx + 130f, ry + 110f, 180f, 30f), level.IsHub ? "in Level 1" : "in this level",
                UIKit.Style(16, false, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, 0.7f)));

            var hatBox = new Rect(rx, ry + 210f, 320f, 190f);
            UIKit.Fill(hatBox, new Color(1f, 1f, 1f, 0.5f), 18f);
            UIKit.Label(new Rect(rx + 24f, ry + 224f, 280f, 30f), "Cosmetic: Hat", UIKit.Style(22, true, TextAnchor.MiddleLeft, ink));
            string hatName = FormVisuals.HatNames[Mathf.Clamp(GameState.HatIndex, 0, FormVisuals.HatNames.Length - 1)];
            UIKit.Label(new Rect(rx + 20f, ry + 266f, 280f, 50f), "<   " + hatName + "   >", UIKit.Style(26, true, TextAnchor.MiddleCenter, ink));
            UIKit.Label(new Rect(rx + 24f, ry + 320f, 280f, 60f), "Hats stay on in every form.",
                UIKit.Style(16, false, TextAnchor.UpperCenter, new Color(ink.r, ink.g, ink.b, 0.7f), true));

            // Prompts
            float py = panel.yMax - 66f;
            float px = panel.x + 50f;
            px += UIKit.PromptLabel(px, py, 40f, PromptAction.Navigate, "Change hat", kb, ink) + 36f;
            UIKit.PromptLabel(px, py, 40f, PromptAction.Cancel, "Close", kb, ink);
        }
    }
}

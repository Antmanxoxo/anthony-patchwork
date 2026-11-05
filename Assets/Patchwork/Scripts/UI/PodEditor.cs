using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The Pod's decorating menu (from the pattern book by the window). Tabs along the top:
    // Walls, Floor, Rug, Light, Decorations, Furniture. Up / down picks (and previews straight
    // away), L1 / R1 or left / right switches tab, Cross toggles things on and off, Circle closes.
    // The camera pulls back so you can see the whole pod while you choose.
    public class PodEditor
    {
        readonly PatchworkGame game;
        static readonly string[] Tabs = { "Walls", "Floor", "Rug", "Light", "Decorations", "Furniture" };
        int tab, row;
        float open, flash;
        readonly List<string> decoKeys = new List<string>();
        readonly List<string> decoNames = new List<string>();

        public PodEditor(PatchworkGame game) { this.game = game; }

        public void Open()
        {
            tab = 0;
            row = PodTheme.Current;
            open = 0f;
            decoKeys.Clear();
            decoNames.Clear();
            for (int i = 0; i < Prizes.DecorationNames.Length; i++)
                if (PodDecor.Exists("deco" + i)) { decoKeys.Add("deco" + i); decoNames.Add(Prizes.DecorationNames[i]); }
            var cam = game.CameraRig;
            cam.ShowcasePosition = CamSpot;
            cam.ShowcaseLookAt = new Vector3(1.2f, 1.6f, 2.4f);
            cam.ShowcaseBlendSpeed = 2.5f;
            cam.Showcase = true;
        }

        static readonly Vector3 CamSpot = new Vector3(-2.2f, 6.4f, -5.4f);

        public void Close()
        {
            var cam = game.CameraRig;
            cam.ShowcaseBlendSpeed = 2.5f;
            cam.Showcase = false;
        }

        int Rows()
        {
            switch (tab)
            {
                case 0: return PodTheme.Names.Length;
                case 1: return PodDecor.FloorNames.Length;
                case 2: return PodDecor.RugNames.Length;
                case 3: return PodDecor.LightNames.Length;
                case 4: return decoKeys.Count;
                default: return PodDecor.Furniture.Length;
            }
        }

        int CurrentChoice()
        {
            switch (tab)
            {
                case 0: return PodTheme.Current;
                case 1: return PodDecor.Floor;
                case 2: return PodDecor.Rug;
                case 3: return PodDecor.LightMode;
                default: return 0;
            }
        }

        void Choose(int i)
        {
            switch (tab)
            {
                case 0: PodTheme.Current = i; PodTheme.Apply(); break;
                case 1: PodDecor.Floor = i; break;
                case 2: PodDecor.Rug = i; break;
                case 3: PodDecor.LightMode = i; break;
            }
            Merits.Earn(Merits.Id.HomeMaker);
        }

        // Returns false when it's closed.
        public bool Update(float udt)
        {
            var input = game.Input;
            open += udt;
            flash = Mathf.MoveTowards(flash, 0f, udt * 3f);
            // A slow, gentle drift so the room feels alive while you choose.
            game.CameraRig.ShowcasePosition = CamSpot + new Vector3(Mathf.Sin(open * 0.35f) * 0.5f, Mathf.Sin(open * 0.5f) * 0.12f, Mathf.Cos(open * 0.35f) * 0.25f);
            if (open < 0.2f) return true;
            if (input.Cancel.WasPressedThisFrame() || input.Pause.WasPressedThisFrame()) return false;

            int t = 0;
            if (input.NavLeft.WasPressedThisFrame() || input.RotateLeft.WasPressedThisFrame()) t = -1;
            if (input.NavRight.WasPressedThisFrame() || input.RotateRight.WasPressedThisFrame()) t = 1;
            if (t != 0)
            {
                tab = (tab + t + Tabs.Length) % Tabs.Length;
                row = tab <= 3 ? CurrentChoice() : 0;
                Sfx.Play(SfxId.PageTurn, 0.45f);
            }
            int n = Rows();
            int d = 0;
            if (input.NavUp.WasPressedThisFrame()) d = -1;
            if (input.NavDown.WasPressedThisFrame()) d = 1;
            if (d != 0 && n > 0)
            {
                row = (row + d + n) % n;
                Sfx.Play(SfxId.MenuMove, 0.45f);
                // Pattern choices preview straight away.
                if (tab <= 3) Choose(row);
            }
            if (input.Confirm.WasPressedThisFrame() && n > 0)
            {
                if (tab <= 3)
                {
                    Choose(row);
                    Sfx.Play(SfxId.MenuConfirm, 0.5f);
                }
                else
                {
                    string key = tab == 4 ? decoKeys[row] : PodDecor.Furniture[row].Key;
                    bool def = tab == 4 || PodDecor.Furniture[row].DefaultOn;
                    bool on = !PodDecor.IsOn(key, def);
                    if (on) PodDecor.Set(key, true);
                    var spot = PodDecor.Find(key);   // measured while it's showing
                    if (!on) PodDecor.Set(key, false);
                    Merits.Earn(Merits.Id.HomeMaker);
                    if (spot != null) FxBurst.Spawn(on ? FxBurst.Kind.Sparkle : FxBurst.Kind.Puff, spot.Value, on ? new Color(1f, 0.85f, 0.5f) : Color.white);
                    Sfx.Play(on ? SfxId.Pop : SfxId.MenuCancel, 0.5f);
                }
                flash = 1f;
            }
            return true;
        }

        public void Draw(bool kb)
        {
            float a = Mathf.Clamp01(open / 0.25f);
            float W = UIKit.Width, H = UIKit.Height;
            float pw = 640f, slide = (1f - UIKit.EaseOutCubic(a)) * (pw + 60f);
            var p = new Rect(W - pw - 40f + slide, 40f, pw, H - 80f);
            UIKit.Panel(p, new Color(0.99f, 0.95f, 0.88f, 0.97f), UIKit.Thread);
            UIKit.PatternFill(new Rect(p.x + 20f, p.y + 20f, p.width - 40f, p.height - 40f), UIArt.PatternDots, new Color(0.85f, 0.6f, 0.4f, 0.07f), 44f, Vector2.zero);
            var ink = UIKit.Ink;
            UIKit.Label(new Rect(p.x, p.y + 24f, pw, 56f), "Decorate the Pod", UIKit.Style(40, true, TextAnchor.MiddleCenter, ink));

            // Tabs.
            float tx = p.x + 30f, ty = p.y + 96f;
            for (int i = 0; i < Tabs.Length; i++)
            {
                var st = UIKit.Style(19, true, TextAnchor.MiddleCenter, i == tab ? Color.white : ink);
                float w = UIKit.TextWidth(Tabs[i], st) + 28f;
                if (tx + w > p.xMax - 30f) { tx = p.x + 30f; ty += 50f; }
                var r = new Rect(tx, ty, w, 40f);
                UIKit.Fill(r, i == tab ? new Color(0.9f, 0.4f, 0.5f) : new Color(ink.r, ink.g, ink.b, 0.08f), 20f);
                UIKit.Label(r, Tabs[i], st);
                tx += w + 10f;
            }

            // Rows.
            float y = ty + 64f;
            int n = Rows();
            if (n == 0)
            {
                UIKit.Label(new Rect(p.x + 40f, y, pw - 80f, 120f), "No decorations yet! Pop prize bubbles in the levels to win some.",
                    UIKit.Style(22, true, TextAnchor.UpperCenter, new Color(ink.r, ink.g, ink.b, 0.7f)));
            }
            float rowH = Mathf.Min(54f, (p.yMax - 120f - y) / Mathf.Max(1, n));
            for (int i = 0; i < n; i++)
            {
                var r = new Rect(p.x + 36f, y + i * rowH, pw - 72f, rowH - 6f);
                bool sel = i == row;
                if (sel) UIKit.Fill(r, new Color(1f, 0.85f, 0.5f, 0.55f + flash * 0.3f), 18f);
                string name;
                bool chosen;
                if (tab <= 3)
                {
                    name = tab == 0 ? PodTheme.Names[i] : tab == 1 ? PodDecor.FloorNames[i] : tab == 2 ? PodDecor.RugNames[i] : PodDecor.LightNames[i];
                    chosen = i == CurrentChoice();
                    // A little swatch dot.
                    UIKit.Circle(new Vector2(r.x + 26f, r.y + r.height * 0.5f), 26f, chosen ? new Color(0.9f, 0.4f, 0.5f) : new Color(ink.r, ink.g, ink.b, 0.15f));
                    if (chosen) UIKit.Circle(new Vector2(r.x + 26f, r.y + r.height * 0.5f), 10f, Color.white);
                }
                else
                {
                    string key = tab == 4 ? decoKeys[i] : PodDecor.Furniture[i].Key;
                    name = tab == 4 ? decoNames[i] : PodDecor.Furniture[i].Name;
                    chosen = PodDecor.IsOn(key, tab == 4 || PodDecor.Furniture[i].DefaultOn);
                    var chip = new Rect(r.xMax - 92f, r.y + (r.height - 34f) * 0.5f, 80f, 34f);
                    UIKit.Fill(chip, chosen ? new Color(0.45f, 0.75f, 0.45f) : new Color(ink.r, ink.g, ink.b, 0.2f), 17f);
                    UIKit.Label(chip, chosen ? "On" : "Off", UIKit.Style(18, true, TextAnchor.MiddleCenter, Color.white));
                }
                UIKit.Label(new Rect(r.x + (tab <= 3 ? 56f : 20f), r.y, r.width - 120f, r.height), name, UIKit.Style(23, sel, TextAnchor.MiddleLeft, ink));
            }

            // Prompts.
            float px = p.x + 36f, py = p.yMax - 64f;
            px += UIKit.PromptLabel(px, py, 38f, PromptAction.Navigate, "Choose", kb, ink) + 26f;
            px += UIKit.PromptLabel(px, py, 38f, PromptAction.Rotate, "Tab", kb, ink) + 26f;
            if (tab > 3) px += UIKit.PromptLabel(px, py, 38f, PromptAction.Confirm, "On / Off", kb, ink) + 26f;
            UIKit.PromptLabel(px, py, 38f, PromptAction.Cancel, "Done", kb, ink);
        }
    }
}

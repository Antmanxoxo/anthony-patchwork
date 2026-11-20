using UnityEngine;

namespace Patchwork
{
    // The Store (Pause > Store). Points buy looks, the way a shop works in a craft game: a row
    // of tabs along the top (Colours, Fabrics, Outfits, Hats, flicked with L1 / R1), a grid of
    // cards on the left, and on the right a little stage where Water, Yarn and Wind all model
    // whatever card you're on, with a hop and a twirl. The right stick spins them round.
    //   X on something new: buy it (with a "are you sure?"), and they dance.
    //   X on something you own: wear it (X again on an outfit or hat takes it off).
    // Hats aren't for sale: you find them out in the world. Locked ones show as a shadow.
    public class StoreMenu
    {
        const int Hats = 3;
        static readonly string[] TabNames = { "Colours", "Fabrics", "Outfits", "Hats" };
        static readonly Color[] TabColors = { new Color(0.95f, 0.55f, 0.6f), new Color(0.55f, 0.7f, 0.95f), new Color(0.95f, 0.7f, 0.35f), new Color(0.6f, 0.8f, 0.5f) };
        const int Cols = 4;

        readonly PatchworkGame game;
        StoreStage stage;
        int tab;
        readonly int[] sel = new int[4];
        float openTime, selTime, stickCooldown, scroll, buyFlash, failFlash, tagShake;
        bool confirming, confirmYes;
        float confirmTime;
        string message;
        float messageTime;

        public StoreMenu(PatchworkGame game) { this.game = game; }

        public void Open()
        {
            stage = new StoreStage();
            tab = 0;
            for (int c = 0; c < 3; c++) sel[c] = Wardrobe.Current(c);
            sel[Hats] = Mathf.Clamp(GameState.HatIndex, 0, HatCount - 1);
            openTime = 0f;
            selTime = 0f;
            scroll = 0f;
            confirming = false;
            message = null;
            ShowSelected();
            Sfx.Play(SfxId.MenuOpen, 0.5f);
        }

        public void Close()
        {
            if (stage != null) { stage.Destroy(); stage = null; }
            // Put Patch himself back in what he's actually wearing.
            if (game.Player != null)
            {
                game.Player.Visuals.ApplyLook();
                game.Player.Visuals.SetHat(GameState.HatIndex);
            }
            GameState.Save();
        }

        // ------------------------------------------------------------------ What's on sale

        static int HatCount { get { return FormVisuals.GodHat; } }   // (the testing halo isn't in the shop)

        static int Count(int cat) { return cat == Hats ? HatCount : Wardrobe.Names(cat).Length; }
        static string Name(int cat, int i) { return cat == Hats ? FormVisuals.HatNames[i] : Wardrobe.Names(cat)[i]; }
        static bool Owned(int cat, int i) { return cat == Hats ? GameState.IsHatUnlocked(i) : Wardrobe.Has(cat, i); }
        static int Price(int cat, int i) { return cat == Hats ? 0 : Wardrobe.Prices(cat)[i]; }
        static bool Wearing(int cat, int i) { return cat == Hats ? GameState.HatIndex == i : Wardrobe.Current(cat) == i; }

        static string Describe(int cat, int i)
        {
            if (cat != Hats) return Wardrobe.Describe(cat, i);
            if (i == 0) return "Just Patch, hatless.";
            return GameState.IsHatUnlocked(i) ? "Found on your travels. It fits every form." : "Not for sale: this one's hidden somewhere out in the world.";
        }

        // Everything he's wearing now, with the card you're on swapped in: {colour, fabric, outfit, hat}.
        int[] LookWith(int cat, int i)
        {
            return new[]
            {
                cat == Wardrobe.Colours ? i : GameState.ColourScheme,
                cat == Wardrobe.Fabrics ? i : GameState.Fabric,
                cat == Wardrobe.Outfits ? i : GameState.Outfit,
                cat == Hats ? i : GameState.HatIndex,
            };
        }

        void ShowSelected()
        {
            if (stage == null) return;
            var look = LookWith(tab, sel[tab]);
            stage.Preview(look[0], look[1], look[2], look[3]);
            // The outfit and hat cards get pictures (taken one a frame).
            if (tab == Wardrobe.Outfits || tab == Hats)
                for (int i = 0; i < Count(tab); i++)
                    stage.WantThumb(tab, i, new[] { GameState.ColourScheme, GameState.Fabric, tab == Wardrobe.Outfits ? i : 0, tab == Hats ? i : 0 });
        }

        void Say(string text) { message = text; messageTime = 0f; }

        // ------------------------------------------------------------------ Input

        // Returns false once it's closed.
        public bool Update(float udt)
        {
            openTime += udt;
            selTime += udt;
            stickCooldown -= udt;
            messageTime += udt;
            buyFlash = Mathf.MoveTowards(buyFlash, 0f, udt * 1.5f);
            failFlash = Mathf.MoveTowards(failFlash, 0f, udt * 2.5f);
            tagShake = Mathf.MoveTowards(tagShake, 0f, udt * 3f);
            if (stage != null)
            {
                float spin = game.Input.LookStick.ReadValue<Vector2>().x;
                if (Mathf.Abs(spin) > 0.2f) stage.Turn(spin);
                stage.Tick(udt);
            }
            var input = game.Input;
            if (openTime < 0.15f) return true;

            if (confirming) { UpdateConfirm(udt); return true; }

            if (input.Cancel.WasPressedThisFrame() || input.Pause.WasPressedThisFrame())
            {
                Sfx.Play(SfxId.MenuCancel, 0.4f);
                Close();
                return false;
            }

            int t = 0;
            if (input.RotateLeft.WasPressedThisFrame()) t = -1;
            if (input.RotateRight.WasPressedThisFrame()) t = 1;
            if (t != 0)
            {
                tab = (tab + t + TabNames.Length) % TabNames.Length;
                scroll = 0f;
                selTime = 0f;
                Sfx.Play(SfxId.PageTurn, 0.45f);
                ShowSelected();
                return true;
            }

            int dx = 0, dy = 0;
            if (input.NavLeft.WasPressedThisFrame()) dx = -1;
            if (input.NavRight.WasPressedThisFrame()) dx = 1;
            if (input.NavUp.WasPressedThisFrame()) dy = -1;
            if (input.NavDown.WasPressedThisFrame()) dy = 1;
            var stick = input.Move.ReadValue<Vector2>();
            if (dx == 0 && dy == 0 && stickCooldown <= 0f)
            {
                if (Mathf.Abs(stick.x) > 0.6f) dx = stick.x > 0f ? 1 : -1;
                else if (Mathf.Abs(stick.y) > 0.6f) dy = stick.y > 0f ? -1 : 1;
            }
            if (stick.magnitude < 0.3f) stickCooldown = 0f;
            if (dx != 0 || dy != 0)
            {
                stickCooldown = 0.2f;
                int n = Count(tab), cur = sel[tab];
                int next = cur;
                if (dx != 0) next = Mathf.Clamp(cur + dx, 0, n - 1);
                if (dy != 0) { next = cur + dy * Cols; if (next < 0 || next >= n) next = cur; }
                if (next != cur)
                {
                    sel[tab] = next;
                    selTime = 0f;
                    Sfx.Play(SfxId.MenuMove, 0.45f);
                    ShowSelected();
                }
            }

            if (input.Confirm.WasPressedThisFrame()) Choose();
            return true;
        }

        void Choose()
        {
            int i = sel[tab];
            if (Owned(tab, i))
            {
                bool off = Wearing(tab, i) && i != 0 && (tab == Wardrobe.Outfits || tab == Hats);
                Put(tab, off ? 0 : i);
                Say(off ? "Taken off." : "Wearing the " + Name(tab, i) + "!");
                return;
            }
            if (tab == Hats)
            {
                failFlash = 1f;
                Sfx.Play(SfxId.TransformFail, 0.4f);
                Say("Hats can't be bought: keep exploring and you'll find this one.");
                return;
            }
            if (GameState.Purse < Price(tab, i))
            {
                failFlash = 1f;
                tagShake = 1f;
                Sfx.Play(SfxId.TransformFail, 0.4f);
                Say("Not enough points yet: " + (Price(tab, i) - GameState.Purse).ToString("N0") + " more to go.");
                return;
            }
            confirming = true;
            confirmYes = false;
            confirmTime = 0f;
            Sfx.Play(SfxId.MenuConfirm, 0.45f);
        }

        void Put(int cat, int i)
        {
            if (cat == Hats) { GameState.HatIndex = i; GameState.Save(); }
            else Wardrobe.Wear(cat, i);
            Sfx.PlayPitched(SfxId.Pop, 0.4f, 1.1f);
            ShowSelected();
        }

        void UpdateConfirm(float udt)
        {
            confirmTime += udt;
            var input = game.Input;
            if (confirmTime < 0.08f) return;
            if (input.Cancel.WasPressedThisFrame() || input.Pause.WasPressedThisFrame()) { confirming = false; Sfx.Play(SfxId.MenuCancel, 0.4f); return; }
            if (input.NavLeft.WasPressedThisFrame() || input.NavRight.WasPressedThisFrame())
            {
                confirmYes = !confirmYes;
                Sfx.Play(SfxId.MenuMove, 0.45f);
            }
            float sx = input.Move.ReadValue<Vector2>().x;
            if (Mathf.Abs(sx) > 0.6f && stickCooldown <= 0f) { confirmYes = sx > 0f; stickCooldown = 0.25f; }
            if (Mathf.Abs(sx) < 0.3f) stickCooldown = 0f;
            if (!input.Confirm.WasPressedThisFrame()) return;
            confirming = false;
            if (!confirmYes) { Sfx.Play(SfxId.MenuCancel, 0.4f); return; }
            int i = sel[tab];
            if (Wardrobe.Buy(tab, i))
            {
                Wardrobe.Wear(tab, i);
                buyFlash = 1f;
                Sfx.Play(SfxId.PrizeFanfare, 0.55f);
                Say("It's yours! " + Name(tab, i) + ", wearing it now.");
                ShowSelected();
                if (stage != null) stage.Celebrate();
            }
            else { failFlash = 1f; Sfx.Play(SfxId.TransformFail, 0.4f); }
        }

        // ------------------------------------------------------------------ Drawing

        public void Draw()
        {
            bool kb = game.Input.UsingKeyboard;
            float W = UIKit.Width, H = UIKit.Height, time = Time.unscaledTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(openTime / 0.25f));
            var ink = UIKit.Ink;
            var tc = TabColors[tab];

            // The shop: a full screen of felt in the tab's colour, with polka dots drifting by.
            UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.98f, 0.93f, 0.86f, k));
            UIKit.PatternFill(new Rect(0f, 0f, W, H), UIArt.PatternDots, new Color(tc.r, tc.g, tc.b, 0.16f * k), 64f, new Vector2(time * 0.03f, time * 0.01f));
            float drop = UIKit.EaseOutBack(openTime / 0.45f);

            // Header: the name over the door, and the purse.
            var header = new Rect(40f, 24f + (1f - drop) * -120f, W - 80f, 100f);
            UIKit.Panel(header, new Color(1f, 0.99f, 0.96f, 0.97f * k), new Color(tc.r, tc.g, tc.b, 0.9f * k), 26f);
            UIKit.Label(new Rect(header.x + 36f, header.y + 6f, 600f, 64f), "The Patchwork Store", UIKit.Style(44, true, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, k)));
            UIKit.Label(new Rect(header.x + 38f, header.y + 62f, 700f, 30f), "Everything modelled by the cast. Points from every level end up here.", UIKit.Style(18, false, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, 0.7f * k)));
            string purse = GameState.Purse.ToString("N0");
            var ps = UIKit.Style(34, true, TextAnchor.MiddleRight, new Color(0.45f, 0.28f, 0.08f, k));
            float pwid = UIKit.TextWidth(purse, ps) + 110f;
            var pr = new Rect(header.xMax - pwid - 30f, header.y + 20f, pwid, 60f);
            UIKit.Fill(pr, new Color(1f, 0.86f, 0.42f, k), 30f);
            UIKit.IconTransformed(new Rect(pr.x + 12f, pr.y + 8f, 44f, 44f), PatchworkAssets.IconButton, new Color(1f, 1f, 1f, k), buyFlash * 360f, 1f + buyFlash * 0.2f);
            UIKit.Label(new Rect(pr.x + 60f, pr.y, pr.width - 80f, pr.height), purse, ps);
            UIKit.Label(new Rect(pr.x - 90f, pr.y, 84f, pr.height), "points", UIKit.Style(18, true, TextAnchor.MiddleRight, new Color(ink.r, ink.g, ink.b, 0.6f * k)));

            // Tabs.
            float tx = 60f, ty = header.yMax + 18f;
            UIKit.PromptLabel(tx, ty + 6f, 36f, PromptAction.Rotate, "", kb, ink, k);
            tx += 120f;
            for (int i = 0; i < TabNames.Length; i++)
            {
                bool on = i == tab;
                var ts = UIKit.Style(24, true, TextAnchor.MiddleCenter, on ? Color.white : new Color(ink.r, ink.g, ink.b, 0.8f * k));
                float w = UIKit.TextWidth(TabNames[i], ts) + 56f;
                var r = new Rect(tx, ty - (on ? 4f : 0f), w, on ? 56f : 48f);
                UIKit.Fill(r, on ? new Color(TabColors[i].r, TabColors[i].g, TabColors[i].b, k) : new Color(1f, 1f, 1f, 0.85f * k), 24f);
                if (!on) UIKit.Outline(r, new Color(ink.r, ink.g, ink.b, 0.25f * k), 2f, 24f);
                else PauseMenu.MarchingStitches(new Rect(r.x + 5f, r.y + 5f, r.width - 10f, r.height - 10f), new Color(1f, 1f, 1f, 0.75f * k), time * 30f);
                UIKit.Label(r, TabNames[i], ts);
                tx += w + 12f;
            }

            float top = ty + 76f, bottom = H - 90f;
            var grid = new Rect(40f, top, Mathf.Min(820f, W * 0.46f), bottom - top);
            var show = new Rect(grid.xMax + 30f, top, W - grid.xMax - 70f, bottom - top);
            DrawGrid(grid, k, ink, time);
            DrawShowcase(show, k, ink, time);

            // Prompts.
            float py = H - 70f, px = 60f;
            int cur = sel[tab];
            string act = Owned(tab, cur) ? (Wearing(tab, cur) && cur != 0 && (tab == Wardrobe.Outfits || tab == Hats) ? "Take off" : Wearing(tab, cur) ? "Wearing" : "Wear") : tab == Hats ? "Locked" : "Buy";
            px += UIKit.PromptLabel(px, py, 40f, PromptAction.Confirm, act, kb, ink, k) + 34f;
            px += UIKit.PromptLabel(px, py, 40f, PromptAction.Rotate, "Tabs", kb, ink, k) + 34f;
            if (!kb) px += UIKit.PromptLabel(px, py, 40f, PromptAction.Navigate, "Browse", kb, ink, k) + 34f;
            UIKit.PromptLabel(px, py, 40f, PromptAction.Cancel, "Back", kb, ink, k);
            if (!kb) UIKit.Label(new Rect(W - 520f, py, 480f, 40f), "Right stick: spin them round", UIKit.Style(18, true, TextAnchor.MiddleRight, new Color(ink.r, ink.g, ink.b, 0.6f * k)));

            if (confirming) DrawConfirm(k, ink, kb);
        }

        void DrawGrid(Rect area, float k, Color ink, float time)
        {
            int n = Count(tab);
            float gap = 16f;
            float cw = (area.width - gap * (Cols - 1)) / Cols, ch = cw * 1.22f;
            int rows = (n + Cols - 1) / Cols;
            float full = rows * (ch + gap);
            int selRow = sel[tab] / Cols;
            float want = Mathf.Clamp(selRow * (ch + gap) - area.height * 0.35f, 0f, Mathf.Max(0f, full - area.height));
            scroll = Mathf.Lerp(scroll, want, 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));
            var tc = TabColors[tab];

            for (int i = 0; i < n; i++)
            {
                int row = i / Cols, col = i % Cols;
                float appear = UIKit.EaseOutBack((openTime - 0.1f - i * 0.025f) / 0.3f);
                var r = new Rect(area.x + col * (cw + gap), area.y + row * (ch + gap) - scroll, cw, ch);
                if (r.yMax < area.y - 4f || r.y > area.yMax + 4f) continue;
                bool on = i == sel[tab], owned = Owned(tab, i), wearing = Wearing(tab, i);
                float a = Mathf.Clamp01(appear) * k;
                // Cards that would poke out past the top or bottom of the grid fade.
                if (r.y < area.y) a *= Mathf.Clamp01(1f - (area.y - r.y) / (ch * 0.6f));
                if (r.yMax > area.yMax) a *= Mathf.Clamp01(1f - (r.yMax - area.yMax) / (ch * 0.6f));
                float grow = on ? 6f + Mathf.Sin(time * 4f) * 2f : 0f;
                var cr = new Rect(r.x - grow, r.y - grow, r.width + grow * 2f, r.height + grow * 2f);
                UIKit.Fill(new Rect(cr.x + 3f, cr.y + 6f, cr.width, cr.height), new Color(0.3f, 0.2f, 0.2f, 0.15f * a), 18f);
                UIKit.Fill(cr, on ? new Color(tc.r, tc.g, tc.b, a) : new Color(1f, 0.995f, 0.98f, a), 18f);
                if (on) PauseMenu.MarchingStitches(new Rect(cr.x + 6f, cr.y + 6f, cr.width - 12f, cr.height - 12f), new Color(1f, 1f, 1f, 0.85f * a), time * 40f);
                else UIKit.Outline(cr, new Color(ink.r, ink.g, ink.b, 0.2f * a), 2f, 18f);

                // The picture.
                var pic = new Rect(cr.x + 12f, cr.y + 12f, cr.width - 24f, cr.width - 24f);
                UIKit.Fill(pic, new Color(1f, 0.97f, 0.92f, a), 14f);
                DrawPicture(tab, i, pic, a, time);

                // Name, and price or status.
                var ns = UIKit.Style(16, true, TextAnchor.UpperCenter, on ? new Color(1f, 1f, 1f, a) : new Color(ink.r, ink.g, ink.b, a), true);
                UIKit.Label(new Rect(cr.x + 8f, pic.yMax + 6f, cr.width - 16f, 40f), Name(tab, i), ns);
                string tag = wearing ? "Wearing" : owned ? "Owned" : tab == Hats ? "Hidden" : Price(tab, i).ToString("N0");
                Color tagCol = wearing ? new Color(0.45f, 0.75f, 0.5f) : owned ? new Color(0.55f, 0.6f, 0.75f) : tab == Hats ? new Color(0.55f, 0.5f, 0.6f) : GameState.Purse >= Price(tab, i) ? new Color(0.95f, 0.7f, 0.25f) : new Color(0.85f, 0.45f, 0.45f);
                var tagStyle = UIKit.Style(15, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, a));
                float tw = UIKit.TextWidth(tag, tagStyle) + 26f;
                var tr = new Rect(cr.center.x - tw * 0.5f, cr.yMax - 36f, tw, 26f);
                UIKit.Fill(tr, new Color(tagCol.r, tagCol.g, tagCol.b, a), 13f);
                UIKit.Label(tr, tag, tagStyle);
            }
        }

        // The little picture on a card: colour swatches, fabric samples, or a photo of Patch.
        void DrawPicture(int cat, int i, Rect pic, float a, float time)
        {
            if (cat == Wardrobe.Colours)
            {
                Color one = i == 0 ? new Color(0.96f, 0.74f, 0.3f) : Wardrobe.ColourTints[i];
                Color two = i == 0 ? new Color(0.25f, 0.62f, 0.65f) : Wardrobe.SecondTints[i];
                Vector2 c = pic.center;
                float d = pic.width * 0.78f;
                UIKit.Circle(c + new Vector2(2f, 4f), d, new Color(0f, 0f, 0f, 0.12f * a));
                UIKit.Circle(c, d, new Color(one.r, one.g, one.b, a));
                UIKit.Circle(c + new Vector2(d * 0.2f, d * 0.2f), d * 0.42f, new Color(two.r, two.g, two.b, a));
                UIKit.CircleOutline(c, d - 10f, new Color(1f, 1f, 1f, 0.55f * a), 2f);
                UIKit.Circle(c + new Vector2(-d * 0.2f, -d * 0.22f), d * 0.16f, new Color(1f, 1f, 1f, 0.35f * a));
                return;
            }
            if (cat == Wardrobe.Fabrics)
            {
                Color tint = Wardrobe.ColourTints[Mathf.Clamp(GameState.ColourScheme, 0, Wardrobe.ColourTints.Length - 1)];
                Color baseCol = Color.Lerp(new Color(0.95f, 0.72f, 0.35f), tint, GameState.ColourScheme == 0 ? 0f : 0.7f);
                var sw = new Rect(pic.x + 14f, pic.y + 14f, pic.width - 28f, pic.height - 28f);
                UIKit.Fill(sw, new Color(baseCol.r, baseCol.g, baseCol.b, a), 12f);
                var weave = Wardrobe.WeaveTexture(i);
                if (weave != null) UIKit.PatternFill(sw, weave, new Color(baseCol.r * 0.8f, baseCol.g * 0.8f, baseCol.b * 0.8f, 0.6f * a), 40f, Vector2.zero);
                else if (i == 0) UIKit.PatternFill(sw, UIArt.PatternKnit, new Color(1f, 1f, 1f, 0.35f * a), 34f, Vector2.zero);
                // A shine as glossy as the fabric.
                float shine = Wardrobe.FabricSmooth[i];
                UIKit.Fill(new Rect(sw.x + sw.width * 0.12f, sw.y + sw.height * 0.1f, sw.width * 0.5f, sw.height * 0.12f), new Color(1f, 1f, 1f, shine * 0.6f * a), 8f);
                if (i == Wardrobe.FabricNames.Length - 1)
                    for (int s = 0; s < 9; s++)
                    {
                        float tw = Mathf.Abs(Mathf.Sin(time * 3f + s * 1.7f));
                        var sp = new Vector2(sw.x + Mathf.Repeat(s * 0.37f, 1f) * sw.width, sw.y + Mathf.Repeat(s * 0.61f + 0.2f, 1f) * sw.height);
                        UIKit.Circle(sp, 4f + tw * 7f, new Color(1f, 0.97f, 0.8f, tw * a));
                    }
                UIKit.Outline(sw, new Color(1f, 1f, 1f, 0.6f * a), 2f, 12f);
                return;
            }
            var photo = stage != null ? stage.Thumb(cat, i) : null;
            if (photo != null)
            {
                bool locked = cat == Hats && !Owned(cat, i);
                GUI.color = locked ? new Color(0.15f, 0.12f, 0.2f, 0.9f * a) : new Color(1f, 1f, 1f, a);
                GUI.DrawTexture(new Rect(pic.x + 4f, pic.y + 4f, pic.width - 8f, pic.height - 8f), photo, ScaleMode.ScaleAndCrop);
                GUI.color = Color.white;
                if (locked) UIKit.Label(pic, "?", UIKit.Style(54, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.9f * a)));
            }
            else
            {
                // Still having its picture taken: a little spinning needle.
                UIKit.IconTransformed(new Rect(pic.center.x - 24f, pic.center.y - 24f, 48f, 48f), UIArt.IconNeedle, new Color(0.6f, 0.55f, 0.65f, 0.7f * a), time * 240f, 1f);
            }
        }

        void DrawShowcase(Rect r, float k, Color ink, float time)
        {
            int i = sel[tab];
            var tc = TabColors[tab];
            float slide = UIKit.EaseOutBack(openTime / 0.5f);
            r.x += (1f - slide) * 200f;
            UIKit.Panel(r, new Color(1f, 0.99f, 0.96f, 0.97f * k), new Color(tc.r, tc.g, tc.b, 0.9f * k), 26f);

            // The stage, filmed live.
            float vw = r.width - 48f, vh = Mathf.Min(r.height - 230f, vw * 760f / 1100f);
            var view = new Rect(r.x + 24f, r.y + 24f, vw, vh);
            UIKit.Fill(new Rect(view.x - 6f, view.y - 6f, view.width + 12f, view.height + 12f), new Color(0.55f, 0.36f, 0.24f, k), 16f);
            if (stage != null && stage.View != null)
            {
                stage.FitTo(view.width, view.height);
                GUI.color = new Color(1f, 1f, 1f, k);
                GUI.DrawTexture(view, stage.View, ScaleMode.StretchToFill);
                GUI.color = Color.white;
            }
            PauseMenu.MarchingStitches(new Rect(view.x - 2f, view.y - 2f, view.width + 4f, view.height + 4f), new Color(1f, 0.85f, 0.5f, 0.8f * k), time * 25f);
            if (buyFlash > 0f) UIKit.Fill(view, new Color(1f, 0.95f, 0.7f, buyFlash * 0.35f * k));

            // The card: name, what it is, and the price tag.
            float y = view.yMax + 22f;
            UIKit.Label(new Rect(r.x + 32f, y, r.width - 300f, 50f), Name(tab, i), UIKit.Style(36, true, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, k)));
            UIKit.Label(new Rect(r.x + 34f, y + 50f, r.width - 320f, 60f), Describe(tab, i), UIKit.Style(20, false, TextAnchor.UpperLeft, new Color(ink.r, ink.g, ink.b, 0.8f * k), true));
            bool owned = Owned(tab, i), wearing = Wearing(tab, i);
            string tag = wearing ? "Wearing" : owned ? "Owned" : tab == Hats ? "Hidden" : Price(tab, i).ToString("N0") + " pts";
            var tagStyle = UIKit.Style(28, true, TextAnchor.MiddleCenter, Color.white);
            float tw = UIKit.TextWidth(tag, tagStyle) + 50f;
            float shake = Mathf.Sin(time * 50f) * 8f * tagShake;
            var tr = new Rect(r.xMax - tw - 30f + shake, y + 6f, tw, 56f);
            Color tagCol = wearing ? new Color(0.45f, 0.75f, 0.5f) : owned ? new Color(0.55f, 0.6f, 0.75f) : tab == Hats ? new Color(0.55f, 0.5f, 0.6f) : GameState.Purse >= Price(tab, i) ? new Color(0.95f, 0.65f, 0.2f) : new Color(0.85f, 0.4f, 0.4f);
            UIKit.Fill(new Rect(tr.x + 3f, tr.y + 5f, tr.width, tr.height), new Color(0f, 0f, 0f, 0.15f * k), 28f);
            UIKit.Fill(tr, new Color(tagCol.r, tagCol.g, tagCol.b, k), 28f);
            UIKit.Circle(new Vector2(tr.x + 18f, tr.center.y), 12f, new Color(1f, 1f, 1f, 0.85f * k));
            UIKit.Label(tr, tag, tagStyle);

            // What just happened.
            if (message != null && messageTime < 3.5f)
            {
                float ma = Mathf.Clamp01((3.5f - messageTime) / 0.5f) * Mathf.Clamp01(messageTime / 0.15f) * k;
                var ms = UIKit.Style(20, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, ma), true);
                var mr = new Rect(view.x + 40f, view.yMax - 70f, view.width - 80f, 50f);
                UIKit.Fill(mr, new Color(failFlash > 0f ? 0.7f : 0.3f, 0.2f, failFlash > 0f ? 0.25f : 0.35f, 0.85f * ma), 25f);
                UIKit.Label(mr, message, ms);
            }
        }

        void DrawConfirm(float k, Color ink, bool kb)
        {
            float W = UIKit.Width, H = UIKit.Height;
            float c = Mathf.Clamp01(confirmTime / 0.15f);
            UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.1f, 0.07f, 0.15f, 0.45f * c));
            int i = sel[tab];
            float pw = 700f, ph = 320f, pop = UIKit.EaseOutBack(confirmTime / 0.3f);
            var panel = new Rect((W - pw) * 0.5f, (H - ph) * 0.5f + (1f - pop) * 40f, pw, ph);
            UIKit.Panel(panel, new Color(0.99f, 0.95f, 0.88f, 0.98f * c), UIKit.Thread);
            UIKit.Label(new Rect(panel.x + 30f, panel.y + 30f, pw - 60f, 50f), "Buy the " + Name(tab, i) + "?", UIKit.Style(32, true, TextAnchor.MiddleCenter, new Color(ink.r, ink.g, ink.b, c)));
            UIKit.Label(new Rect(panel.x + 30f, panel.y + 86f, pw - 60f, 60f), Price(tab, i).ToString("N0") + " points. You'll have " + (GameState.Purse - Price(tab, i)).ToString("N0") + " left.", UIKit.Style(22, false, TextAnchor.UpperCenter, new Color(ink.r, ink.g, ink.b, 0.85f * c), true));
            float bw = 220f, bh = 64f, gap = 40f, bx = panel.x + (pw - bw * 2f - gap) * 0.5f, by = panel.y + 170f;
            for (int b = 0; b < 2; b++)
            {
                bool yes = b == 1, on = yes == confirmYes;
                var r = new Rect(bx + b * (bw + gap), by, bw, bh);
                Color col = yes ? new Color(0.95f, 0.65f, 0.25f) : new Color(0.55f, 0.6f, 0.75f);
                if (on)
                {
                    UIKit.Fill(r, new Color(col.r, col.g, col.b, c), 20f);
                    PauseMenu.MarchingStitches(new Rect(r.x + 6f, r.y + 6f, r.width - 12f, r.height - 12f), new Color(1f, 1f, 1f, 0.85f * c), Time.unscaledTime * 40f);
                }
                else
                {
                    UIKit.Fill(r, new Color(1f, 1f, 1f, 0.7f * c), 18f);
                    UIKit.Outline(r, new Color(ink.r, ink.g, ink.b, 0.25f * c), 2f, 18f);
                }
                UIKit.Label(r, yes ? "Buy it!" : "Not yet", UIKit.Style(26, true, TextAnchor.MiddleCenter, on ? new Color(1f, 1f, 1f, c) : new Color(ink.r, ink.g, ink.b, c)));
            }
        }
    }
}

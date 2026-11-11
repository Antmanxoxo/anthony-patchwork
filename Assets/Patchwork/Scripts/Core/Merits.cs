using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Merit Patches: little felt badges for trying everything the game has (stomping creatures,
    // finding marbles, the Big Top, Photo Mode, decorating the Pod...). Earning one pops up a
    // card; they're listed in the pause menu and stitched onto the Pod's wall. Saved across plays.
    public static class Merits
    {
        public struct Def
        {
            public string Name, How;
            public StickerId Icon;
            public Color Col;
            public Def(string name, string how, StickerId icon, Color col) { Name = name; How = how; Icon = icon; Col = col; }
        }

        public enum Id
        {
            FirstStitch, SnarlStomper, ComboCrafter, BandTogether, WeddingBells, Heartstrung, MarbleHunter, MarbleMaster,
            RollingStar, Ringmaster, WhackChampion, TrapezeArtist, StickerArtist, Shutterbug, HomeMaker, DanceFloor, ShapeShifter, BigScore,
            KingOfTheCastle, SkyGlider
        }

        public static readonly Def[] All =
        {
            new Def("First Stitch", "Unravel a creature of the Fray.", StickerId.Smiley, new Color(0.95f, 0.45f, 0.5f)),
            new Def("Snarl Stomper", "Unravel 15 creatures of the Fray.", StickerId.Paw, new Color(0.6f, 0.45f, 0.85f)),
            new Def("Combo Crafter", "Stomp three creatures in a row without landing.", StickerId.Rainbow, new Color(1f, 0.6f, 0.3f)),
            new Def("Band Together", "Bring the whole Toybox Town band home.", StickerId.Music, new Color(0.4f, 0.7f, 0.95f)),
            new Def("Wedding Bells", "Finish Toybox Town.", StickerId.Heart, new Color(0.95f, 0.55f, 0.7f)),
            new Def("Heartstrung", "Find every Heartstring in Toybox Town.", StickerId.Heart, new Color(1f, 0.75f, 0.3f)),
            new Def("Marble Hunter", "Find 5 secret marbles.", StickerId.Moon, new Color(0.45f, 0.75f, 0.95f)),
            new Def("Marble Master", "Find every secret marble in Toybox Town.", StickerId.Star, new Color(0.35f, 0.55f, 0.95f)),
            new Def("Rolling Star", "Roll the Tape Ball up into a star.", StickerId.Star, new Color(1f, 0.85f, 0.3f)),
            new Def("Ringmaster", "Beat the Grumblewool at the Big Top.", StickerId.Crown, new Color(0.9f, 0.3f, 0.3f)),
            new Def("Whack Champion", "Whack 15 Snarls in Whack-a-Snarl.", StickerId.Paw, new Color(0.95f, 0.5f, 0.3f)),
            new Def("Trapeze Artist", "Swing across the Big Top's trapeze.", StickerId.Rainbow, new Color(0.55f, 0.85f, 0.55f)),
            new Def("Sticker Artist", "Stick 5 stickers anywhere.", StickerId.Flower, new Color(0.95f, 0.6f, 0.8f)),
            new Def("Shutterbug", "Take a photo in Photo Mode.", StickerId.Smiley, new Color(0.5f, 0.8f, 0.9f)),
            new Def("Home Maker", "Change the Pod's wallpaper.", StickerId.Flower, new Color(0.85f, 0.7f, 0.45f)),
            new Def("Dance Floor", "Tap Patch's faces fast enough to dance.", StickerId.Music, new Color(0.75f, 0.5f, 0.95f)),
            new Def("Shape Shifter", "Try all three forms.", StickerId.Fish, new Color(0.45f, 0.85f, 0.8f)),
            new Def("Big Score", "Score 20,000 in one trip to Toybox Town.", StickerId.Crown, new Color(1f, 0.8f, 0.25f)),
            new Def("King of the Castle", "Bounce to the top of the Bouncy Castle's keep.", StickerId.Crown, new Color(1f, 0.55f, 0.72f)),
            new Def("Sky Glider", "Glide through all eight Sky Hoops in time.", StickerId.Rainbow, new Color(0.4f, 0.7f, 1f)),
        };

        const string Prefix = "merit_";
        static readonly Queue<int> toastQueue = new Queue<int>();
        static int toastId = -1;
        static float toastTime;
        static float poll;

        public static void ResetStatics() { toastQueue.Clear(); toastId = -1; toastTime = 0f; poll = 0f; }

        public static bool Has(Id id) { return GameState.IsHintSeen(Prefix + id); }

        public static int Count()
        {
            int n = 0;
            for (int i = 0; i < All.Length; i++) if (Has((Id)i)) n++;
            return n;
        }

        public static void Earn(Id id)
        {
            if (Has(id)) return;
            GameState.SetHintSeen(Prefix + id);
            GameState.Save();
            toastQueue.Enqueue((int)id);
        }

        // Counters that build up over time (creatures popped, stickers stuck).
        public static void Add(string counter, int amount = 1)
        {
            string key = "pw_mc_" + counter;
            int v = PlayerPrefs.GetInt(key, 0) + amount;
            PlayerPrefs.SetInt(key, v);
            if (counter == "pops")
            {
                Earn(Id.FirstStitch);
                if (v >= 15) Earn(Id.SnarlStomper);
            }
            else if (counter == "stickers" && v >= 5) Earn(Id.StickerArtist);
        }

        public static void FormUsed(FormType f)
        {
            int bits = PlayerPrefs.GetInt("pw_mc_forms", 0) | (1 << (int)f);
            PlayerPrefs.SetInt("pw_mc_forms", bits);
            if (bits == 7) Earn(Id.ShapeShifter);
        }

        // Checked a couple of times a second for the things that are easiest to just look at.
        public static void Tick(PatchworkGame game, float udt)
        {
            if (toastId >= 0)
            {
                toastTime += udt;
                if (toastTime > 3.6f) toastId = -1;
            }
            if (toastId < 0 && toastQueue.Count > 0)
            {
                toastId = toastQueue.Dequeue();
                toastTime = 0f;
                Sfx.Play(SfxId.PrizeFanfare, 0.55f);
            }

            poll -= udt;
            if (poll > 0f || game == null || game.Level == null || game.Player == null) return;
            poll = 0.5f;
            var level = game.Level;
            FormUsed(game.Player.Forms.Current);
            if (level.Id == Level1Builder.SceneName)
            {
                if (Level1Director.BandCount() >= 3) Earn(Id.BandTogether);
                if (level.HeartstringIds.Count > 0 && level.HeartstringCount() >= level.HeartstringIds.Count) Earn(Id.Heartstrung);
                int marbles = level.SecretCount();
                if (marbles >= 5) Earn(Id.MarbleHunter);
                if (level.SecretIds.Count > 0 && marbles >= level.SecretIds.Count) Earn(Id.MarbleMaster);
                if (game.Score != null && game.Score.Total >= 20000) Earn(Id.BigScore);
            }
        }

        // ------------------------------------------------------------------ Drawing

        // A round felt patch with a stitched edge and its emblem.
        public static void DrawPatch(Vector2 c, float size, int i, bool earned, float alpha = 1f)
        {
            var d = All[i];
            Color body = earned ? d.Col : new Color(0.72f, 0.7f, 0.76f);
            body.a = alpha;
            UIKit.Circle(c + new Vector2(0f, size * 0.05f), size, new Color(0f, 0f, 0f, 0.18f * alpha));
            UIKit.Circle(c, size, body);
            UIKit.CircleOutline(c, size * 0.84f, new Color(1f, 1f, 1f, 0.85f * alpha), 3f);
            if (earned) UIKit.Icon(new Rect(c.x - size * 0.32f, c.y - size * 0.32f, size * 0.64f, size * 0.64f), StickerArt.Get(d.Icon), new Color(1f, 1f, 1f, alpha));
            else UIKit.Label(new Rect(c.x - size * 0.5f, c.y - size * 0.5f, size, size), "?", UIKit.Style(Mathf.RoundToInt(size * 0.45f), true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.9f * alpha)));
        }

        // The "Merit Patch earned!" card.
        // top: up at the top of the screen (over the Level Complete card it would sit on its
        // buttons), instead of near the bottom.
        public static void DrawToast(bool top = false)
        {
            if (toastId < 0) return;
            float t = toastTime;
            float a = Mathf.Clamp01(t / 0.25f) * Mathf.Clamp01((3.6f - t) / 0.4f);
            float slide = UIKit.EaseOutBack(Mathf.Clamp01(t / 0.45f));
            float W = UIKit.Width;
            var r = top ? new Rect((W - 560f) * 0.5f, 36f - (1f - slide) * 80f, 560f, 120f)
                        : new Rect((W - 560f) * 0.5f, UIKit.Height - 250f + (1f - slide) * 80f, 560f, 120f);
            UIKit.Panel(r, new Color(1f, 0.97f, 0.9f, 0.96f * a), new Color(d(toastId).r, d(toastId).g, d(toastId).b, 0.95f * a), 24f);
            float spin = Mathf.Sin(t * 6f) * 6f * Mathf.Clamp01(1f - t);
            DrawPatch(new Vector2(r.x + 66f, r.y + 60f + spin), 88f, toastId, true, a);
            var ink = UIKit.Ink;
            UIKit.Label(new Rect(r.x + 126f, r.y + 14f, 420f, 34f), "Merit Patch earned!", UIKit.Style(20, true, TextAnchor.MiddleLeft, new Color(0.85f, 0.35f, 0.45f, a)));
            UIKit.Label(new Rect(r.x + 126f, r.y + 44f, 420f, 40f), All[toastId].Name, UIKit.Style(30, true, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, a)));
            UIKit.Label(new Rect(r.x + 126f, r.y + 82f, 420f, 28f), All[toastId].How, UIKit.Style(17, false, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, 0.75f * a)));
        }

        static Color d(int i) { return All[i].Col; }

        // The full board (from the pause menu). "selected" is the highlighted patch.
        public static void DrawBoard(int selected, float open)
        {
            float W = UIKit.Width, H = UIKit.Height;
            float a = Mathf.Clamp01(open / 0.2f);
            UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.1f, 0.07f, 0.15f, 0.55f * a));
            float pw = 1180f, ph = 760f;
            float drop = UIKit.EaseOutBack(Mathf.Clamp01(open / 0.4f));
            var p = new Rect((W - pw) * 0.5f, Mathf.LerpUnclamped(-ph, (H - ph) * 0.5f, drop), pw, ph);
            UIKit.Panel(p, new Color(0.99f, 0.95f, 0.88f, 0.98f), UIKit.Thread);
            UIKit.PatternFill(new Rect(p.x + 20f, p.y + 20f, p.width - 40f, p.height - 40f), UIArt.PatternDots, new Color(0.85f, 0.6f, 0.4f, 0.08f), 48f, Vector2.zero);
            var ink = UIKit.Ink;
            UIKit.Label(new Rect(p.x, p.y + 26f, pw, 56f), "Merit Patches  " + Count() + " / " + All.Length, UIKit.Style(42, true, TextAnchor.MiddleCenter, ink));
            const int cols = 7;
            float cell = 156f, top = p.y + 110f, left = p.x + (pw - cols * cell) * 0.5f;
            for (int i = 0; i < All.Length; i++)
            {
                int cx = i % cols, cy = i / cols;
                var c = new Vector2(left + cx * cell + cell * 0.5f, top + cy * 150f + 56f);
                bool sel = i == selected;
                float s = sel ? 96f + Mathf.Sin(Time.unscaledTime * 6f) * 4f : 86f;
                if (sel) UIKit.Circle(c, s + 16f, new Color(1f, 1f, 1f, 0.9f));
                DrawPatch(c, s, i, Has((Id)i));
                bool earned = Has((Id)i);
                UIKit.Label(new Rect(c.x - cell * 0.5f, c.y + 50f, cell, 30f), earned ? All[i].Name : "???", UIKit.Style(16, true, TextAnchor.MiddleCenter, new Color(ink.r, ink.g, ink.b, earned ? 1f : 0.5f)));
            }
            // What the highlighted one is for.
            var info = new Rect(p.x + 60f, p.yMax - 110f, pw - 120f, 50f);
            UIKit.Fill(info, new Color(All[selected].Col.r, All[selected].Col.g, All[selected].Col.b, 0.25f), 25f);
            UIKit.Label(info, (Has((Id)selected) ? All[selected].Name + ": " : "") + All[selected].How, UIKit.Style(22, true, TextAnchor.MiddleCenter, ink));
            bool kb = PatchworkGame.Instance != null && PatchworkGame.Instance.Input.UsingKeyboard;
            UIKit.PromptLabel(p.x + pw * 0.5f - 60f, p.yMax - 52f, 36f, PromptAction.Cancel, "Back", kb, ink);
        }
    }
}

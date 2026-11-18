using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The quest menu (Pause > Quests). Three tabs, flicked with L1 / R1:
    //   Quests   - the story, the jobs people have given you and the things to collect, on the
    //              left; the one you're on, in full, on the right (who gave it, what it's about,
    //              and every step, ticked off). X tracks it: the gold pointer follows that quest
    //              instead of the story until it's done.
    //   Finished - everything you've completed.
    //   Options  - the pointer on or off, the "Next:" line on the quest card, and the "Next:"
    //              pop-ups when the step changes.
    // In the Pod there's no story running, so it lists the places you've been and how they went.
    public class QuestMenu
    {
        enum Tab { Quests, Finished, Options }
        static readonly string[] TabNames = { "Quests", "Finished", "Options" };
        static readonly string[] OptionNames = { "Quest Marker", "Next Step on Screen", "Next Step Pop-ups" };
        static readonly string[] OptionInfo =
        {
            "The gold pointer that shows where to go next, with how far away it is.",
            "The \"Next:\" line on the quest card in the top corner.",
            "The little \"Next: ...\" note that pops up when the next thing to do changes.",
        };

        readonly PatchworkGame game;
        public bool IsOpen { get; private set; }
        Tab tab;
        int sel;
        float openTime, selTime, stickCooldown, scroll;
        readonly List<QuestEntry> shown = new List<QuestEntry>();

        public QuestMenu(PatchworkGame game) { this.game = game; }

        public void Open()
        {
            IsOpen = true;
            tab = Tab.Quests;
            sel = 0;
            scroll = 0f;
            openTime = 0f;
            selTime = 0f;
            Refresh();
            // Start on whatever's being tracked.
            string tracked = QuestGuide.Tracked;
            for (int i = 0; i < shown.Count; i++) if (shown[i].Id == tracked) sel = i;
            Sfx.Play(SfxId.MenuOpen, 0.45f);
        }

        void Refresh()
        {
            shown.Clear();
            var log = QuestGuide.Current != null ? QuestGuide.Log(true) : PodLog();
            // Story first, then side quests, then collections (the log is already in that order
            // within each kind).
            for (int kind = 0; kind < 3; kind++)
                foreach (var q in log)
                    if ((int)q.Type == kind && (tab == Tab.Finished ? q.Done : !q.Done))
                        shown.Add(q);
            sel = Mathf.Clamp(sel, 0, Mathf.Max(0, shown.Count - 1));
        }

        // In the Pod: one entry per place on the globe.
        static List<QuestEntry> PodLog()
        {
            var log = new List<QuestEntry>();
            foreach (var l in LevelCatalog.Levels)
            {
                if (!l.Playable) continue;
                bool done = GameState.IsLevelComplete(l.Scene);
                int got = 0;
                foreach (var id in l.Collectibles) if (GameState.IsCollected(id)) got++;
                int hearts = 0;
                foreach (var id in l.Heartstrings) if (GameState.IsCollected(id)) hearts++;
                var e = new QuestEntry { Id = l.Scene, Type = QuestEntry.Kind.Main, Title = l.Number + ": " + l.Title, Giver = "The globe", Summary = l.Blurb, Done = done };
                e.Step("Mend it", done);
                if (l.Collectibles.Length > 0) e.Step("Golden buttons  " + got + " / " + l.Collectibles.Length, got >= l.Collectibles.Length);
                if (l.Heartstrings.Length > 0) e.Step("Heartstrings  " + hearts + " / " + l.Heartstrings.Length, hearts >= l.Heartstrings.Length);
                log.Add(e);
            }
            return log;
        }

        static bool Option(int i)
        {
            return i == 0 ? GameSettings.QuestMarker : i == 1 ? GameSettings.QuestObjective : GameSettings.QuestPopups;
        }

        static void FlipOption(int i)
        {
            if (i == 0) GameSettings.QuestMarker = !GameSettings.QuestMarker;
            else if (i == 1) GameSettings.QuestObjective = !GameSettings.QuestObjective;
            else GameSettings.QuestPopups = !GameSettings.QuestPopups;
            PlayerPrefs.Save();
        }

        int Count { get { return tab == Tab.Options ? OptionNames.Length : shown.Count; } }

        public void Update(float udt)
        {
            if (!IsOpen) return;
            openTime += udt;
            selTime += udt;
            stickCooldown -= udt;
            var input = game.Input;
            if (openTime < 0.12f) return;

            if (input.Cancel.WasPressedThisFrame() || input.Pause.WasPressedThisFrame())
            {
                IsOpen = false;
                Sfx.Play(SfxId.MenuCancel, 0.4f);
                return;
            }

            // Tabs.
            int t = 0;
            if (input.RotateLeft.WasPressedThisFrame()) t = -1;
            if (input.RotateRight.WasPressedThisFrame()) t = 1;
            if (t != 0)
            {
                tab = (Tab)(((int)tab + t + 3) % 3);
                sel = 0;
                scroll = 0f;
                selTime = 0f;
                Refresh();
                Sfx.Play(SfxId.MenuMove, 0.5f);
            }

            // Up and down the list.
            int dir = 0;
            if (input.NavUp.WasPressedThisFrame()) dir = -1;
            if (input.NavDown.WasPressedThisFrame()) dir = 1;
            float sy = input.Move.ReadValue<Vector2>().y;
            if (dir == 0 && Mathf.Abs(sy) > 0.6f && stickCooldown <= 0f) dir = sy > 0f ? -1 : 1;
            if (Mathf.Abs(sy) < 0.3f) stickCooldown = 0f;
            if (dir != 0 && Count > 0)
            {
                stickCooldown = 0.22f;
                sel = (sel + dir + Count) % Count;
                selTime = 0f;
                Sfx.Play(SfxId.MenuMove, 0.45f);
            }

            bool side = input.NavLeft.WasPressedThisFrame() || input.NavRight.WasPressedThisFrame();
            if (input.Confirm.WasPressedThisFrame() || tab == Tab.Options && side)
            {
                if (tab == Tab.Options)
                {
                    FlipOption(sel);
                    Sfx.Play(SfxId.MenuConfirm, 0.5f);
                    selTime = 0f;
                }
                else if (tab == Tab.Quests && sel < shown.Count && QuestGuide.Current != null)
                {
                    var q = shown[sel];
                    if (q.Target == null && q.Id != "main")
                    {
                        Sfx.Play(SfxId.TransformFail, 0.4f);
                        game.ShowToast("The marker can't point at this one. Keep your eyes open!");
                    }
                    else
                    {
                        bool already = QuestGuide.Tracked == q.Id || q.Id == "main" && QuestGuide.Tracked == "";
                        QuestGuide.Tracked = already || q.Id == "main" ? "" : q.Id;
                        Sfx.Play(SfxId.MenuConfirm, 0.5f);
                        if (!GameSettings.QuestMarker) game.ShowToast("Tracking " + q.Title + ". (The Quest Marker is off: switch it on under Options.)");
                    }
                    selTime = 0f;
                }
            }
        }

        static bool IsTracked(QuestEntry q)
        {
            string t = QuestGuide.Tracked;
            if (QuestGuide.Current == null) return false;
            if (q.Id == "main") return t == "" || QuestGuide.Find(t) == null || QuestGuide.Find(t).Done;
            return t == q.Id;
        }

        static Color KindColor(QuestEntry.Kind k)
        {
            return k == QuestEntry.Kind.Main ? new Color(0.93f, 0.62f, 0.25f) : k == QuestEntry.Kind.Side ? new Color(0.45f, 0.62f, 0.9f) : new Color(0.55f, 0.75f, 0.5f);
        }

        static string KindName(QuestEntry.Kind k)
        {
            return k == QuestEntry.Kind.Main ? "Story" : k == QuestEntry.Kind.Side ? "Side Quests" : "Collections";
        }

        public void Draw()
        {
            if (!IsOpen) return;
            bool kb = game.Input.UsingKeyboard;
            float W = UIKit.Width, H = UIKit.Height, time = Time.unscaledTime;
            float k = Mathf.Clamp01(openTime / 0.2f);
            var ink = UIKit.Ink;
            UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.1f, 0.07f, 0.15f, 0.55f * k));

            float pw = Mathf.Min(1500f, W - 80f), ph = 860f;
            float pop = UIKit.EaseOutBack(openTime / 0.35f);
            var panel = new Rect((W - pw) * 0.5f, (H - ph) * 0.5f + (1f - pop) * 60f, pw, ph);
            UIKit.Panel(panel, new Color(0.99f, 0.95f, 0.88f, 0.98f * k), UIKit.Thread);
            UIKit.PatternFill(new Rect(panel.x + 20f, panel.y + 20f, panel.width - 40f, panel.height - 40f), UIArt.PatternDots, new Color(0.85f, 0.6f, 0.4f, 0.07f * k), 48f, new Vector2(time * 0.02f, 0f));

            // Title and tabs.
            string place = game.Level != null ? (game.Level.IsHub ? "Your Adventures" : "Quests") : "Quests";
            UIKit.Label(new Rect(panel.x + 40f, panel.y + 24f, 500f, 56f), place, UIKit.Style(44, true, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, k)));
            float tx = panel.xMax - 40f;
            for (int i = TabNames.Length - 1; i >= 0; i--)
            {
                var ts = UIKit.Style(22, true, TextAnchor.MiddleCenter, (int)tab == i ? Color.white : new Color(ink.r, ink.g, ink.b, 0.8f * k));
                float w = UIKit.TextWidth(TabNames[i], ts) + 44f;
                var r = new Rect(tx - w, panel.y + 32f, w, 44f);
                if ((int)tab == i) UIKit.Fill(r, new Color(0.85f, 0.45f, 0.55f, k), 22f);
                else UIKit.Outline(r, new Color(ink.r, ink.g, ink.b, 0.3f * k), 2f, 22f);
                UIKit.Label(r, TabNames[i], ts);
                tx -= w + 12f;
            }
            UIKit.PromptLabel(tx - 150f, panel.y + 36f, 36f, PromptAction.Rotate, "", kb, ink, k);

            var list = new Rect(panel.x + 36f, panel.y + 100f, pw * 0.42f, ph - 190f);
            var detail = new Rect(list.xMax + 28f, list.y, panel.xMax - list.xMax - 64f, list.height);

            if (tab == Tab.Options) DrawOptions(list, detail, k, ink, time);
            else DrawQuests(list, detail, k, ink, time);

            // Prompts.
            float py = panel.yMax - 66f, px = panel.x + 46f;
            if (tab == Tab.Options) px += UIKit.PromptLabel(px, py, 38f, PromptAction.Confirm, "Switch", kb, ink, k) + 30f;
            else if (tab == Tab.Quests && QuestGuide.Current != null && shown.Count > 0)
                px += UIKit.PromptLabel(px, py, 38f, PromptAction.Confirm, IsTracked(shown[Mathf.Min(sel, shown.Count - 1)]) ? "Tracking" : "Track", kb, ink, k) + 30f;
            px += UIKit.PromptLabel(px, py, 38f, PromptAction.Rotate, "Tabs", kb, ink, k) + 30f;
            UIKit.PromptLabel(px, py, 38f, PromptAction.Cancel, "Back", kb, ink, k);
        }

        void DrawQuests(Rect list, Rect detail, float k, Color ink, float time)
        {
            if (shown.Count == 0)
            {
                string empty = tab == Tab.Finished ? "Nothing finished yet. It's early days!" : QuestGuide.Current == null && game.Level != null && !game.Level.IsHub ? "No quests here." : "All done here. Well stitched!";
                UIKit.Label(list, empty, UIKit.Style(24, false, TextAnchor.MiddleCenter, new Color(ink.r, ink.g, ink.b, 0.7f * k), true));
                return;
            }
            // The list, with a heading over each kind. Scrolls to keep the pick in view.
            const float rowH = 62f, headH = 40f;
            float y = 0f, selY = 0f;
            QuestEntry.Kind? last = null;
            for (int i = 0; i < shown.Count; i++)
            {
                if (last != shown[i].Type) { y += headH; last = shown[i].Type; }
                if (i == sel) selY = y;
                y += rowH + 8f;
            }
            float want = Mathf.Clamp(selY - list.height * 0.4f, 0f, Mathf.Max(0f, y - list.height));
            scroll = Mathf.Lerp(scroll, want, 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));

            // (Drawn in place rather than in a GUI group, so the rotated tick marks line up; rows
            // outside the list's box are skipped.)
            y = list.y - scroll;
            last = null;
            for (int i = 0; i < shown.Count; i++)
            {
                var q = shown[i];
                if (last != q.Type)
                {
                    last = q.Type;
                    var kc = KindColor(q.Type);
                    if (y >= list.y - 4f && y + headH <= list.yMax)
                        UIKit.Label(new Rect(list.x + 8f, y + 4f, list.width, headH - 6f), KindName(q.Type), UIKit.Style(19, true, TextAnchor.MiddleLeft, new Color(kc.r * 0.75f, kc.g * 0.75f, kc.b * 0.75f, k)));
                    y += headH;
                }
                if (y < list.y - 10f || y + rowH > list.yMax + 10f) { y += rowH + 8f; continue; }
                bool on = i == sel;
                float grow = on ? 4f + Mathf.Sin(time * 4f) * 1.2f : 0f;
                var r = new Rect(list.x + (on ? 14f : 4f), y - grow * 0.5f, list.width - 18f, rowH + grow);
                var c = KindColor(q.Type);
                if (on)
                {
                    UIKit.Fill(r, new Color(c.r, c.g, c.b, k), 18f);
                    PauseMenu.MarchingStitches(new Rect(r.x + 6f, r.y + 6f, r.width - 12f, r.height - 12f), new Color(1f, 1f, 1f, 0.8f * k), time * 40f);
                }
                else
                {
                    UIKit.Fill(r, new Color(1f, 0.995f, 0.98f, 0.95f * k), 18f);
                    UIKit.Outline(r, new Color(ink.r, ink.g, ink.b, 0.22f * k), 2f, 18f);
                }
                // A tick when done, the steps so far otherwise.
                var badge = new Vector2(r.x + 34f, r.y + r.height * 0.5f);
                UIKit.Circle(badge, 38f, on ? new Color(1f, 1f, 1f, 0.95f * k) : new Color(c.r, c.g, c.b, 0.9f * k));
                Color markCol = on ? new Color(c.r * 0.6f, c.g * 0.6f, c.b * 0.6f, k) : new Color(1f, 1f, 1f, k);
                if (q.Done) Tick(badge, 22f, markCol);
                else if (q.Steps.Count > 1) UIKit.Label(new Rect(badge.x - 20f, badge.y - 19f, 40f, 38f), q.StepsDone + "/" + q.Steps.Count, UIKit.Style(14, true, TextAnchor.MiddleCenter, markCol));
                else UIKit.Circle(badge, 10f, markCol);
                var ts = UIKit.Style(22, true, TextAnchor.MiddleLeft, on ? new Color(1f, 1f, 1f, k) : new Color(ink.r, ink.g, ink.b, k));
                UIKit.Label(new Rect(r.x + 66f, r.y, r.width - 120f, r.height), q.Title, ts, on);
                // The gold pin on the tracked one.
                if (IsTracked(q) && GameSettings.QuestMarker && !q.Done)
                {
                    var pin = new Vector2(r.xMax - 30f, r.y + r.height * 0.5f + Mathf.Sin(time * 3f) * 3f);
                    UIKit.Circle(pin, 26f, new Color(1f, 0.82f, 0.32f, k));
                    UIKit.Circle(pin, 10f, new Color(1f, 0.97f, 0.88f, k));
                }
                y += rowH + 8f;
            }

            DrawDetail(shown[Mathf.Clamp(sel, 0, shown.Count - 1)], detail, k, ink, time);
        }

        void DrawDetail(QuestEntry q, Rect r, float k, Color ink, float time)
        {
            var c = KindColor(q.Type);
            float slide = UIKit.EaseOutCubic(Mathf.Clamp01(selTime / 0.25f));
            r.x += (1f - slide) * 24f;
            float a = k * slide;
            UIKit.Panel(r, new Color(1f, 0.99f, 0.96f, 0.97f * a), new Color(c.r, c.g, c.b, 0.9f * a), 22f);
            float x = r.x + 34f, w = r.width - 68f, y = r.y + 26f;
            UIKit.Label(new Rect(x, y, w, 50f), q.Title, UIKit.Style(34, true, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, a)));
            y += 52f;
            string sub = KindName(q.Type).TrimEnd('s') + (string.IsNullOrEmpty(q.Giver) ? "" : "  ·  " + q.Giver) + (q.Done ? "  ·  Done!" : IsTracked(q) && GameSettings.QuestMarker ? "  ·  Tracking" : "");
            UIKit.Label(new Rect(x, y, w, 30f), sub, UIKit.Style(18, true, TextAnchor.MiddleLeft, new Color(c.r * 0.7f, c.g * 0.7f, c.b * 0.7f, a)));
            y += 42f;
            if (!string.IsNullOrEmpty(q.Summary))
            {
                var ss = UIKit.Style(21, false, TextAnchor.UpperLeft, new Color(ink.r, ink.g, ink.b, 0.9f * a), true);
                float sh = ss.CalcHeight(new GUIContent(q.Summary), w);
                UIKit.Label(new Rect(x, y, w, sh), q.Summary, ss);
                y += sh + 22f;
            }
            // The steps, ticked off. The first one not done yet is the one to do next.
            bool nextShown = false;
            foreach (var s in q.Steps)
            {
                if (y > r.yMax - 40f) break;
                bool next = !s.Done && !nextShown && !q.Done;
                if (next) nextShown = true;
                var box = new Rect(x, y + 4f, 26f, 26f);
                if (s.Done)
                {
                    UIKit.Fill(box, new Color(0.45f, 0.75f, 0.5f, a), 7f);
                    Tick(box.center, 18f, new Color(1f, 1f, 1f, a));
                }
                else UIKit.Outline(box, next ? new Color(0.93f, 0.62f, 0.25f, a) : new Color(ink.r, ink.g, ink.b, 0.4f * a), next ? 3f : 2f, 7f);
                var st = UIKit.Style(20, next, TextAnchor.UpperLeft, s.Done ? new Color(ink.r, ink.g, ink.b, 0.5f * a) : new Color(ink.r, ink.g, ink.b, a), true);
                float h = Mathf.Max(32f, st.CalcHeight(new GUIContent(s.Text), w - 44f) + 6f);
                UIKit.Label(new Rect(x + 42f, y + 3f, w - 44f, h), s.Text, st);
                if (s.Done) UIKit.Fill(new Rect(x + 42f, y + 17f, Mathf.Min(w - 44f, UIKit.TextWidth(s.Text, st)), 2f), new Color(ink.r, ink.g, ink.b, 0.35f * a));
                y += h + 6f;
            }
            if (q.Target == null && q.Id != "main" && !q.Done && QuestGuide.Current != null)
                UIKit.Label(new Rect(x, r.yMax - 44f, w, 30f), "The marker can't point at this one.", UIKit.Style(16, false, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, 0.55f * a)));
        }

        // A tick mark, drawn as two strokes (so it doesn't depend on the font having one).
        static void Tick(Vector2 c, float s, Color col)
        {
            Vector2 a = c + new Vector2(-0.38f, 0.02f) * s, b = c + new Vector2(-0.1f, 0.3f) * s, d = c + new Vector2(0.4f, -0.3f) * s;
            Stroke(a, b, s * 0.17f, col);
            Stroke(b, d, s * 0.17f, col);
        }

        static void Stroke(Vector2 a, Vector2 b, float thick, Color col)
        {
            Vector2 d = b - a;
            float len = d.magnitude + thick * 0.5f, ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            var saved = GUI.matrix;
            var pivot = new Vector3(a.x, a.y, 0f);
            GUI.matrix = saved * Matrix4x4.TRS(pivot, Quaternion.Euler(0f, 0f, ang), Vector3.one) * Matrix4x4.Translate(-pivot);
            UIKit.Fill(new Rect(a.x - thick * 0.25f, a.y - thick * 0.5f, len, thick), col, thick * 0.5f);
            GUI.matrix = saved;
        }

        void DrawOptions(Rect list, Rect detail, float k, Color ink, float time)
        {
            for (int i = 0; i < OptionNames.Length; i++)
            {
                bool on = i == sel, val = Option(i);
                var r = new Rect(list.x + (on ? 10f : 0f), list.y + 20f + i * 84f, list.width - 10f, 70f);
                var c = new Color(0.85f, 0.45f, 0.55f);
                if (on)
                {
                    UIKit.Fill(r, new Color(c.r, c.g, c.b, k), 18f);
                    PauseMenu.MarchingStitches(new Rect(r.x + 6f, r.y + 6f, r.width - 12f, r.height - 12f), new Color(1f, 1f, 1f, 0.8f * k), time * 40f);
                }
                else
                {
                    UIKit.Fill(r, new Color(1f, 0.995f, 0.98f, 0.95f * k), 18f);
                    UIKit.Outline(r, new Color(ink.r, ink.g, ink.b, 0.22f * k), 2f, 18f);
                }
                UIKit.Label(new Rect(r.x + 26f, r.y, r.width - 160f, r.height), OptionNames[i], UIKit.Style(23, true, TextAnchor.MiddleLeft, on ? Color.white : new Color(ink.r, ink.g, ink.b, k)), on);
                // A little stitched switch.
                var sw = new Rect(r.xMax - 118f, r.y + 17f, 96f, 36f);
                UIKit.Fill(sw, val ? new Color(0.45f, 0.75f, 0.5f, k) : new Color(0.6f, 0.58f, 0.62f, k), 18f);
                float knob = val ? sw.xMax - 18f : sw.x + 18f;
                UIKit.Circle(new Vector2(knob, sw.center.y), 30f, new Color(1f, 1f, 1f, k));
                UIKit.Label(new Rect(val ? sw.x + 8f : sw.x + 36f, sw.y, 52f, sw.height), val ? "On" : "Off", UIKit.Style(15, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, k)));
            }
            UIKit.Panel(detail, new Color(1f, 0.99f, 0.96f, 0.97f * k), new Color(0.85f, 0.45f, 0.55f, 0.9f * k), 22f);
            UIKit.Label(new Rect(detail.x + 34f, detail.y + 26f, detail.width - 68f, 50f), OptionNames[sel], UIKit.Style(32, true, TextAnchor.MiddleLeft, new Color(ink.r, ink.g, ink.b, k)));
            UIKit.Label(new Rect(detail.x + 34f, detail.y + 90f, detail.width - 68f, 200f), OptionInfo[sel] + "\n\nIt's " + (Option(sel) ? "on" : "off") + " now.", UIKit.Style(22, false, TextAnchor.UpperLeft, new Color(ink.r, ink.g, ink.b, 0.9f * k), true));
        }
    }
}

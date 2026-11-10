using UnityEngine;

namespace Patchwork
{
    // The Storyteller: the voice of the storybook the game is told in. Lines appear on a strip
    // of cream paper along the bottom of the screen, written out a letter at a time, with a
    // warm low murmur under them as if somebody were reading aloud. Used for cutscene captions
    // and for the odd remark as Patch explores. It can also show a chapter card (the big title
    // at the start of a story) and a place name tag.
    public static class Narrator
    {
        const float LettersPerSecond = 34f;

        static string line, lastShown;
        static float written, lineTimer, lineAlpha;
        static int lastLetter;
        // Chapter card and place tag (set by whoever's telling the story; fade themselves out).
        static string cardKicker, cardTitle, place, lastPlace;
        static float cardAlpha, cardWant, placePop, placeAlpha;

        public static void ResetStatics()
        {
            line = lastShown = cardKicker = cardTitle = place = lastPlace = null;
            written = lineTimer = lineAlpha = cardAlpha = cardWant = placePop = placeAlpha = 0f;
        }

        // Say something while Patch is playing. It stays up long enough to read, then fades.
        public static void Say(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            line = text;
            lineTimer = 2.6f + text.Length * 0.062f;
        }

        public static bool Speaking { get { return lineTimer > 0f; } }

        // Show (or, with null, put away) the chapter card.
        public static void Card(string kicker, string title)
        {
            if (title != null) { cardKicker = kicker; cardTitle = title; }
            cardWant = title != null ? 1f : 0f;
        }

        // Show (or, with null, put away) the name of the place the story has got to.
        public static void Place(string name)
        {
            place = name;
        }

        // Draws whatever the Storyteller has to say this frame. caption is the cutscene's
        // current line (or null when there's no cutscene); k fades the whole thing.
        public static void Draw(string caption, float k, bool cutscene)
        {
            float udt = Time.unscaledDeltaTime;
            string text = cutscene ? caption : (lineTimer > 0f ? line : null);
            if (!cutscene && lineTimer > 0f) lineTimer -= udt;

            if (text != lastShown)
            {
                // A new line: start writing from the top (after the last one has faded a little).
                if (text != null) { lastShown = text; written = 0f; lastLetter = 0; }
                else if (lineAlpha <= 0.01f) lastShown = null;
            }
            lineAlpha = Mathf.MoveTowards(lineAlpha, text != null ? 1f : 0f, udt * (text != null ? 5f : 2.2f));
            if (lastShown != null && lineAlpha > 0.01f)
            {
                written = Mathf.Min(lastShown.Length, written + udt * LettersPerSecond);
                Murmur();
                DrawStrip(lastShown.Substring(0, Mathf.FloorToInt(written)), lastShown, lineAlpha * (cutscene ? k : 1f), cutscene);
            }

            cardAlpha = Mathf.MoveTowards(cardAlpha, cutscene ? cardWant : 0f, udt * 1.6f);
            if (cardAlpha > 0.01f && cardTitle != null) DrawCard(cardAlpha * k);

            string wantPlace = cutscene ? place : null;
            if (wantPlace != lastPlace && placeAlpha <= 0.01f) { lastPlace = wantPlace; placePop = 0f; }
            placeAlpha = Mathf.MoveTowards(placeAlpha, wantPlace != null && wantPlace == lastPlace ? 1f : 0f, udt * 4f);
            if (lastPlace != null && placeAlpha > 0.01f)
            {
                placePop = Mathf.MoveTowards(placePop, 1f, udt * 2.6f);
                DrawPlace(lastPlace, placeAlpha * k);
            }
        }

        // The murmur: a soft, low syllable every few letters as the line is written.
        static void Murmur()
        {
            int at = Mathf.FloorToInt(written);
            if (at <= lastLetter || at > lastShown.Length) return;
            for (int i = lastLetter; i < at; i++)
            {
                char c = lastShown[i];
                if (!char.IsLetter(c) || i % 3 != 0) continue;
                float lift = i + 1 < lastShown.Length && lastShown[lastShown.Length - 1] == '?' && i > lastShown.Length - 8 ? 1.12f : 1f;
                Sfx.PlayPitched(SfxId.Babble0 + char.ToLowerInvariant(c) % 6, 0.2f, (0.6f + (c % 5) * 0.022f) * lift);
            }
            lastLetter = at;
        }

        static readonly Color Paper = new Color(0.99f, 0.96f, 0.88f), Ink = new Color(0.27f, 0.17f, 0.13f), Rose = new Color(0.82f, 0.36f, 0.42f);

        static void DrawStrip(string shown, string whole, float a, bool cutscene)
        {
            var style = UIKit.Style(27, true, TextAnchor.MiddleCenter, new Color(Ink.r, Ink.g, Ink.b, a), true);
            float w = Mathf.Min(UIKit.Width - 260f, 1180f);
            // Tall enough for the whole line from the start, so the strip doesn't jump as it's written.
            float h = Mathf.Max(64f, style.CalcHeight(new GUIContent(whole), w - 120f) + 30f);
            var r = new Rect((UIKit.Width - w) * 0.5f, UIKit.Height - (cutscene ? 46f : 150f) - h, w, h);
            UIKit.Fill(new Rect(r.x + 4f, r.y + 6f, r.width, r.height), new Color(0f, 0f, 0f, 0.28f * a), 18f);
            UIKit.Panel(r, new Color(Paper.r, Paper.g, Paper.b, 0.97f * a), new Color(Rose.r, Rose.g, Rose.b, 0.75f * a), 18f);
            // The Storyteller's mark: a round rose seal with a quotation mark, on the corner.
            var seal = new Vector2(r.x + 6f, r.y + 6f);
            UIKit.Circle(seal + new Vector2(2f, 3f), 54f, new Color(0f, 0f, 0f, 0.25f * a));
            UIKit.Circle(seal, 54f, new Color(Rose.r, Rose.g, Rose.b, a));
            UIKit.Circle(seal, 44f, new Color(0.9f, 0.46f, 0.5f, a));
            UIKit.Label(new Rect(seal.x - 27f, seal.y - 20f, 54f, 54f), "“", UIKit.Style(44, true, TextAnchor.MiddleCenter, new Color(1f, 0.97f, 0.9f, a)));
            UIKit.Label(new Rect(r.x + 60f, r.y, r.width - 120f, r.height), shown, style);
        }

        static void DrawCard(float a)
        {
            float cx = UIKit.Width * 0.5f, cy = UIKit.Height * 0.4f;
            var title = UIKit.Style(72, true, TextAnchor.MiddleCenter, new Color(Paper.r, Paper.g, Paper.b, a));
            float w = Mathf.Max(620f, UIKit.TextWidth(cardTitle, title) + 150f);
            float rise = (1f - a) * 24f;
            var r = new Rect(cx - w * 0.5f, cy - 110f + rise, w, 220f);
            UIKit.Fill(new Rect(r.x + 5f, r.y + 8f, r.width, r.height), new Color(0f, 0f, 0f, 0.3f * a), 26f);
            UIKit.Panel(r, new Color(0.3f, 0.2f, 0.34f, 0.9f * a), new Color(1f, 0.85f, 0.5f, 0.85f * a), 26f);
            if (!string.IsNullOrEmpty(cardKicker))
                UIKit.Label(new Rect(r.x, r.y + 22f, r.width, 44f), cardKicker.ToUpperInvariant(), UIKit.Style(26, true, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.5f, a)));
            UIKit.Label(new Rect(r.x, r.y + 62f, r.width, 110f), cardTitle, title, true);
            // A row of little stitches under the title.
            for (int i = -5; i <= 5; i++)
                UIKit.Fill(new Rect(cx + i * 30f - 9f, r.y + 180f, 18f, 5f), new Color(1f, 0.85f, 0.5f, a), 2.5f);
        }

        static void DrawPlace(string name, float a)
        {
            var style = UIKit.Style(34, true, TextAnchor.MiddleCenter, new Color(Ink.r, Ink.g, Ink.b, a));
            float w = UIKit.TextWidth(name, style) + 90f;
            float pop = UIKit.EaseOutBack(placePop);
            Matrix4x4 saved = GUI.matrix;
            var pivot = new Vector3(70f + w * 0.5f, 170f, 0f);
            GUI.matrix = saved * Matrix4x4.TRS(pivot, Quaternion.Euler(0f, 0f, -4f), new Vector3(pop, pop, 1f)) * Matrix4x4.Translate(-pivot);
            var r = new Rect(70f, 138f, w, 64f);
            UIKit.Fill(new Rect(r.x + 4f, r.y + 6f, r.width, r.height), new Color(0f, 0f, 0f, 0.28f * a), 14f);
            UIKit.Panel(r, new Color(Paper.r, Paper.g, Paper.b, a), new Color(0.35f, 0.6f, 0.45f, 0.85f * a), 14f);
            // A button sewn on at the left end, like a luggage tag.
            UIKit.Circle(new Vector2(r.x + 30f, r.center.y), 26f, new Color(0.35f, 0.6f, 0.45f, a));
            UIKit.Circle(new Vector2(r.x + 30f, r.center.y), 8f, new Color(Paper.r, Paper.g, Paper.b, a));
            UIKit.Label(new Rect(r.x + 44f, r.y, r.width - 54f, r.height), name, style);
            GUI.matrix = saved;
        }
    }

    // Somewhere the Storyteller has something to say. The first time Patch walks in, the line
    // is read out; never again after that (it's remembered in the save).
    public class NarrationZone : PlayerVolume
    {
        public string Key, Line;

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (string.IsNullOrEmpty(Key) || GameState.IsHintSeen(Key)) return;
            if (player.Game.Mode != UIMode.Gameplay || Narrator.Speaking) return;   // try again next time he passes
            GameState.SetHintSeen(Key);
            Narrator.Say(Line);
        }
    }
}

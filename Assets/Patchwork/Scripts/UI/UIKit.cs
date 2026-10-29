using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    public enum PromptAction
    {
        Jump,
        Ability,
        FormWheel,
        Confirm,
        Cancel,
        Satchel,
        Pause,
        Navigate
    }

    // Immediate-mode UI helpers. Everything is drawn in a virtual 1080p-tall space, so it scales to any
    // resolution. Panels are soft rounded "felt" cards with stitched borders.
    public static class UIKit
    {
        public const float RefHeight = 1080f;
        public static float Scale { get; private set; }
        public static float Width { get; private set; }
        public static float Height { get { return RefHeight; } }

        public static readonly Color Ink = new Color(0.22f, 0.17f, 0.26f);
        public static readonly Color Paper = new Color(0.99f, 0.96f, 0.9f, 0.94f);
        public static readonly Color Thread = new Color(0.85f, 0.45f, 0.55f, 0.9f);

        // One style per size/weight/alignment/wrap; the colour is set each time it's handed out.
        // (Keying on colour made a new style for every fade step and built strings every frame.)
        static readonly Dictionary<int, GUIStyle> styles = new Dictionary<int, GUIStyle>();

        public static void Begin()
        {
            Scale = Mathf.Max(0.1f, Screen.height / RefHeight);
            Width = Screen.width / Scale;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Scale, Scale, 1f));
        }

        // Crafted fonts (Resources/Fonts): Fredoka for everyday text, Sniglet's puffy cut-felt letters
        // for big titles. If they're missing, Unity's default font is used so nothing breaks.
        static Font bodyFont, bodyBoldFont, titleFont;
        static bool fontsLoaded;
        const int TitleSize = 30;

        static void LoadFonts()
        {
            if (fontsLoaded && (bodyFont != null || bodyBoldFont != null || titleFont != null)) return;
            fontsLoaded = true;
            bodyFont = Resources.Load<Font>("Fonts/Fredoka-Medium");
            bodyBoldFont = Resources.Load<Font>("Fonts/Fredoka-SemiBold");
            titleFont = Resources.Load<Font>("Fonts/Sniglet-ExtraBold");
            if (bodyBoldFont == null) bodyBoldFont = bodyFont;
            if (titleFont == null) titleFont = bodyBoldFont;
        }

        static Font FontFor(int size, bool bold)
        {
            LoadFonts();
            if (bold && size >= TitleSize && titleFont != null) return titleFont;
            return bold ? bodyBoldFont : bodyFont;
        }

        public static GUIStyle Style(int size, bool bold, TextAnchor anchor, Color color, bool wrap = false)
        {
            int key = (size & 0xFF) | (bold ? 1 << 8 : 0) | ((int)anchor << 9) | (wrap ? 1 << 13 : 0);
            GUIStyle s;
            if (styles.TryGetValue(key, out s) && s != null)
            {
                s.normal.textColor = color;
                return s;
            }
            s = new GUIStyle(GUI.skin.label);
            s.fontSize = size;
            var font = FontFor(size, bold);
            s.font = font;
            // The crafted fonts have real bold cuts, so don't fake-bold them on top.
            s.fontStyle = bold && font == null ? FontStyle.Bold : FontStyle.Normal;
            s.alignment = anchor;
            s.wordWrap = wrap;
            s.richText = false;
            s.normal.textColor = color;
            s.padding = new RectOffset(0, 0, 0, 0);
            s.margin = new RectOffset(0, 0, 0, 0);
            s.clipping = TextClipping.Overflow;
            styles[key] = s;
            return s;
        }

        public static void Fill(Rect r, Color c, float radius = 0f)
        {
            GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, c, 0f, radius);
        }

        public static void Outline(Rect r, Color c, float width, float radius)
        {
            GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, c, width, radius);
        }

        public static void Circle(Vector2 center, float diameter, Color c)
        {
            Fill(new Rect(center.x - diameter * 0.5f, center.y - diameter * 0.5f, diameter, diameter), c, diameter * 0.5f);
        }

        public static void CircleOutline(Vector2 center, float diameter, Color c, float width)
        {
            Outline(new Rect(center.x - diameter * 0.5f, center.y - diameter * 0.5f, diameter, diameter), c, width, diameter * 0.5f);
        }

        public static void Icon(Rect r, Texture tex, Color tint)
        {
            if (tex == null) return;
            GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit, true, 0f, tint, 0f, 0f);
        }

        // A felt card with a dashed running-stitch border.
        public static void Panel(Rect r, Color fill, Color stitch, float radius = 22f)
        {
            Fill(new Rect(r.x + 4f, r.y + 6f, r.width, r.height), new Color(0f, 0f, 0f, 0.18f * fill.a), radius);
            Fill(r, fill, radius);
            Stitches(new Rect(r.x + 10f, r.y + 10f, r.width - 20f, r.height - 20f), stitch);
        }

        public static void Stitches(Rect r, Color c)
        {
            const float dash = 12f, gap = 8f, thick = 3f;
            float inset = 10f;
            for (float x = r.x + inset; x < r.xMax - inset - dash * 0.5f; x += dash + gap)
            {
                float w = Mathf.Min(dash, r.xMax - inset - x);
                Fill(new Rect(x, r.y, w, thick), c, 1.5f);
                Fill(new Rect(x, r.yMax - thick, w, thick), c, 1.5f);
            }
            for (float y = r.y + inset; y < r.yMax - inset - dash * 0.5f; y += dash + gap)
            {
                float h = Mathf.Min(dash, r.yMax - inset - y);
                Fill(new Rect(r.x, y, thick, h), c, 1.5f);
                Fill(new Rect(r.xMax - thick, y, thick, h), c, 1.5f);
            }
        }

        // Draws text. With outline=true the text gets a dark outline so it reads on any background.
        public static void Label(Rect r, string text, GUIStyle style, bool outline = false)
        {
            if (outline)
            {
                var prev = style.normal.textColor;
                style.normal.textColor = new Color(0.08f, 0.05f, 0.12f, 0.85f * prev.a);
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                        if (dx != 0 || dy != 0)
                            GUI.Label(new Rect(r.x + dx * 2f, r.y + dy * 2f, r.width, r.height), text, style);
                style.normal.textColor = prev;
            }
            GUI.Label(r, text, style);
        }

        // Tiles a pattern texture across a rect (tileSize in virtual pixels), with a scrolling offset.
        public static void PatternFill(Rect r, Texture tex, Color tint, float tileSize, Vector2 offset)
        {
            if (tex == null) return;
            var prev = GUI.color;
            GUI.color = tint;
            var coords = new Rect(offset.x, offset.y, r.width / tileSize, r.height / tileSize);
            GUI.DrawTextureWithTexCoords(r, tex, coords, true);
            GUI.color = prev;
        }

        // Draws an icon rotated (degrees) and scaled around its centre.
        public static void IconTransformed(Rect r, Texture tex, Color tint, float angle, float scale)
        {
            if (tex == null) return;
            Matrix4x4 saved = GUI.matrix;
            Vector3 c = new Vector3(r.center.x, r.center.y, 0f);
            GUI.matrix = saved * Matrix4x4.TRS(c, Quaternion.Euler(0f, 0f, angle), new Vector3(scale, scale, 1f)) * Matrix4x4.TRS(-c, Quaternion.identity, Vector3.one);
            GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit, true, 0f, tint, 0f, 0f);
            GUI.matrix = saved;
        }

        public static float EaseOutBack(float t)
        {
            t = Mathf.Clamp01(t);
            const float c1 = 1.70158f, c3 = c1 + 1f;
            float x = t - 1f;
            return 1f + c3 * x * x * x + c1 * x * x;
        }

        public static float EaseOutCubic(float t)
        {
            t = Mathf.Clamp01(t);
            float x = 1f - t;
            return 1f - x * x * x;
        }

        // A little heart (two circles and a turned square), for Heartstrings.
        public static void Heart(Vector2 c, float size, Color col)
        {
            float r = size * 0.3f;
            Circle(new Vector2(c.x - r * 0.95f, c.y - r * 0.55f), r * 2f, col);
            Circle(new Vector2(c.x + r * 0.95f, c.y - r * 0.55f), r * 2f, col);
            float sq = size * 0.56f;
            IconTransformed(new Rect(c.x - sq * 0.5f, c.y - sq * 0.35f, sq, sq), Texture2D.whiteTexture, col, 45f, 1f);
        }

        // Dark rounded backing for white text.
        public static void Pill(Rect r, float alpha = 1f)
        {
            Fill(r, new Color(0.13f, 0.1f, 0.19f, 0.88f * alpha), Mathf.Min(r.height * 0.5f, 26f));
        }

        public static float TextWidth(string text, GUIStyle style)
        {
            return style.CalcSize(new GUIContent(text)).x;
        }

        // ------------------------------------------------------------------ Button prompts

        static string KeyName(PromptAction a)
        {
            switch (a)
            {
                case PromptAction.Jump: return "Space";
                case PromptAction.Ability: return "E";
                case PromptAction.FormWheel: return "Q";
                case PromptAction.Confirm: return "Enter";
                case PromptAction.Cancel: return "Esc";
                case PromptAction.Satchel: return "Tab";
                case PromptAction.Pause: return "Esc";
                default: return "Arrows";
            }
        }

        static Texture2D PadGlyph(PromptAction a)
        {
            switch (a)
            {
                case PromptAction.Jump:
                case PromptAction.Confirm: return PatchworkAssets.GlyphCross;
                case PromptAction.Ability: return PatchworkAssets.GlyphSquare;
                case PromptAction.Cancel: return PatchworkAssets.GlyphCircle;
                default: return null;
            }
        }

        static string PadLabel(PromptAction a)
        {
            switch (a)
            {
                case PromptAction.FormWheel: return PatchworkInput.FormWheelPadLabel;
                case PromptAction.Satchel: return "TOUCHPAD";
                case PromptAction.Pause: return "OPTIONS";
                case PromptAction.Navigate: return "D-PAD";
                default: return "";
            }
        }

        // Draws a PlayStation glyph or a key cap. Returns the width used.
        public static float Prompt(float x, float y, float h, PromptAction action, bool keyboard, float alpha = 1f)
        {
            Color tint = new Color(1f, 1f, 1f, alpha);
            if (!keyboard)
            {
                var glyph = PadGlyph(action);
                if (glyph != null)
                {
                    Icon(new Rect(x, y, h, h), glyph, tint);
                    return h;
                }
                return KeyCap(x, y, h, PadLabel(action), alpha);
            }
            return KeyCap(x, y, h, KeyName(action), alpha);
        }

        static float KeyCap(float x, float y, float h, string text, float alpha)
        {
            var style = Style(Mathf.RoundToInt(h * 0.42f), true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, alpha));
            float w = Mathf.Max(h, TextWidth(text, style) + h * 0.55f);
            var r = new Rect(x, y, w, h);
            Fill(new Rect(x, y + 3f, w, h), new Color(0f, 0f, 0f, 0.3f * alpha), h * 0.28f);
            Fill(r, new Color(0.13f, 0.11f, 0.19f, alpha), h * 0.28f);
            Outline(r, new Color(1f, 1f, 1f, 0.85f * alpha), 2f, h * 0.28f);
            GUI.Label(r, text, style);
            return w;
        }

        static float PromptWidth(PromptAction action, float h, bool keyboard)
        {
            if (!keyboard && PadGlyph(action) != null) return h;
            string text = keyboard ? KeyName(action) : PadLabel(action);
            var style = Style(Mathf.RoundToInt(h * 0.42f), true, TextAnchor.MiddleCenter, Color.white);
            return Mathf.Max(h, TextWidth(text, style) + h * 0.55f);
        }

        static bool TokenAction(string name, out PromptAction action)
        {
            switch (name)
            {
                case "Jump": action = PromptAction.Jump; return true;
                case "Ability": action = PromptAction.Ability; return true;
                case "Wheel": action = PromptAction.FormWheel; return true;
                case "Satchel": action = PromptAction.Satchel; return true;
                case "Pause": action = PromptAction.Pause; return true;
                default: action = PromptAction.Jump; return false;
            }
        }

        // Word-wrapped text where {Jump}, {Ability}, {Wheel}, {Satchel} and {Pause} are drawn as the
        // actual button for the controller (or key) being used, so hints always match the controls.
        public static void RichText(Rect r, string text, int size, bool bold, Color color, bool keyboard)
        {
            var style = Style(size, bold, TextAnchor.MiddleLeft, color);
            float lineH = size * 1.4f;
            float iconH = size * 1.3f;
            float space = Mathf.Max(4f, TextWidth("a a", style) - TextWidth("aa", style));
            float x = r.x, y = r.y;
            foreach (var raw in text.Split(' '))
            {
                if (raw.Length == 0) continue;
                // A word can be "{Ability}" with punctuation after it, e.g. "{Jump}!".
                PromptAction action;
                bool hasToken = false;
                string after = raw;
                if (raw.StartsWith("{"))
                {
                    int close = raw.IndexOf('}');
                    if (close > 1 && TokenAction(raw.Substring(1, close - 1), out action))
                    {
                        hasToken = true;
                        after = raw.Substring(close + 1);
                        float iw = PromptWidth(action, iconH, keyboard);
                        float aw = after.Length > 0 ? TextWidth(after, style) : 0f;
                        if (x + iw + aw > r.xMax && x > r.x) { x = r.x; y += lineH; }
                        Prompt(x, y + (lineH - iconH) * 0.5f, iconH, action, keyboard, color.a);
                        style = Style(size, bold, TextAnchor.MiddleLeft, color);
                        x += iw;
                        if (aw > 0f) { GUI.Label(new Rect(x, y, aw + 4f, lineH), after, style); x += aw; }
                        x += space;
                        continue;
                    }
                }
                if (hasToken) continue;
                float w = TextWidth(raw, style);
                if (x + w > r.xMax && x > r.x) { x = r.x; y += lineH; }
                // Prompt() can hand out the same style with another colour, so set ours again.
                style = Style(size, bold, TextAnchor.MiddleLeft, color);
                GUI.Label(new Rect(x, y, w + 4f, lineH), raw, style);
                x += w + space;
            }
        }

        // Prompt + label pair. Returns the total width.
        public static float PromptLabel(float x, float y, float h, PromptAction action, string label, bool keyboard, Color textColor, float alpha = 1f)
        {
            float w = Prompt(x, y, h, action, keyboard, alpha);
            var style = Style(Mathf.RoundToInt(h * 0.55f), true, TextAnchor.MiddleLeft, new Color(textColor.r, textColor.g, textColor.b, textColor.a * alpha));
            float tw = TextWidth(label, style);
            bool light = textColor.r + textColor.g + textColor.b > 2f;
            Label(new Rect(x + w + 10f, y, tw + 4f, h), label, style, light);
            return w + 10f + tw;
        }
    }
}

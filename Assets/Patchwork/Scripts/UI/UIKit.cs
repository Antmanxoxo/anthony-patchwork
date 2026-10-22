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

    /// <summary>
    /// Immediate-mode UI helpers. Everything is drawn in a virtual 1080p-tall space, so it scales to any
    /// resolution. Panels are soft rounded "felt" cards with stitched borders.
    /// </summary>
    public static class UIKit
    {
        public const float RefHeight = 1080f;
        public static float Scale { get; private set; }
        public static float Width { get; private set; }
        public static float Height { get { return RefHeight; } }

        public static readonly Color Ink = new Color(0.22f, 0.17f, 0.26f);
        public static readonly Color Paper = new Color(0.99f, 0.96f, 0.9f, 0.94f);
        public static readonly Color Thread = new Color(0.85f, 0.45f, 0.55f, 0.9f);

        static readonly Dictionary<string, GUIStyle> styles = new Dictionary<string, GUIStyle>();

        public static void Begin()
        {
            Scale = Mathf.Max(0.1f, Screen.height / RefHeight);
            Width = Screen.width / Scale;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Scale, Scale, 1f));
        }

        public static GUIStyle Style(int size, bool bold, TextAnchor anchor, Color color, bool wrap = false)
        {
            string key = size + "_" + bold + "_" + anchor + "_" + ColorUtility.ToHtmlStringRGBA(color) + "_" + wrap;
            GUIStyle s;
            if (styles.TryGetValue(key, out s)) return s;
            s = new GUIStyle(GUI.skin.label);
            s.fontSize = size;
            s.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
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

        /// <summary>A felt card with a dashed running-stitch border.</summary>
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

        public static void Label(Rect r, string text, GUIStyle style, bool shadow = false)
        {
            if (shadow)
            {
                var prev = style.normal.textColor;
                style.normal.textColor = new Color(0f, 0f, 0f, 0.35f * prev.a);
                GUI.Label(new Rect(r.x + 2f, r.y + 2f, r.width, r.height), text, style);
                style.normal.textColor = prev;
            }
            GUI.Label(r, text, style);
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
                case PromptAction.FormWheel: return "L1";
                case PromptAction.Satchel: return "TOUCHPAD";
                case PromptAction.Pause: return "OPTIONS";
                case PromptAction.Navigate: return "D-PAD";
                default: return "";
            }
        }

        /// <summary>Draws a PlayStation glyph or a key cap. Returns the width used.</summary>
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

        /// <summary>Prompt + label pair. Returns the total width.</summary>
        public static float PromptLabel(float x, float y, float h, PromptAction action, string label, bool keyboard, Color textColor, float alpha = 1f)
        {
            float w = Prompt(x, y, h, action, keyboard, alpha);
            var style = Style(Mathf.RoundToInt(h * 0.55f), true, TextAnchor.MiddleLeft, new Color(textColor.r, textColor.g, textColor.b, textColor.a * alpha));
            float tw = TextWidth(label, style);
            GUI.Label(new Rect(x + w + 10f, y, tw + 4f, h), label, style);
            return w + 10f + tw;
        }
    }
}

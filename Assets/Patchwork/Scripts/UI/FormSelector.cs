using UnityEngine;

namespace Patchwork
{
    // The Form Wheel. Opens with L1 (or Q), slows time down, and you point a stick at a form.
    // Yarn is at the top, Water bottom-left, Wind bottom-right. X picks, Circle cancels.
    public class FormSelector
    {
        static readonly FormType[] Slots = { FormType.Yarn, FormType.Water, FormType.Wind };
        static readonly float[] SlotAngles = { -90f, 150f, 30f }; // screen space, degrees (y down)

        readonly PatchworkGame game;
        public FormType Highlight { get; private set; }
        float openTime;

        public FormSelector(PatchworkGame game)
        {
            this.game = game;
        }

        public void Open()
        {
            Highlight = game.Player.Forms.Current;
            openTime = 0f;
        }

        public void Update(float udt)
        {
            openTime += udt;
            var input = game.Input;

            // Point with either stick (WASD also feeds the Move stick).
            Vector2 s = input.Move.ReadValue<Vector2>();
            Vector2 r = input.LookStick.ReadValue<Vector2>();
            if (r.sqrMagnitude > s.sqrMagnitude) s = r;
            if (s.magnitude > 0.5f)
            {
                float ang = Mathf.Atan2(-s.y, s.x) * Mathf.Rad2Deg;
                int best = 0;
                float bestDiff = 999f;
                for (int i = 0; i < Slots.Length; i++)
                {
                    float d = Mathf.Abs(Mathf.DeltaAngle(ang, SlotAngles[i]));
                    if (d < bestDiff) { bestDiff = d; best = i; }
                }
                SetHighlight(Slots[best]);
            }

            if (input.NavUp.WasPressedThisFrame()) SetHighlight(FormType.Yarn);
            if (input.NavLeft.WasPressedThisFrame()) SetHighlight(FormType.Water);
            if (input.NavRight.WasPressedThisFrame()) SetHighlight(FormType.Wind);
            if (input.Pick1.WasPressedThisFrame()) SetHighlight(FormType.Water);
            if (input.Pick2.WasPressedThisFrame()) SetHighlight(FormType.Yarn);
            if (input.Pick3.WasPressedThisFrame()) SetHighlight(FormType.Wind);

            if (input.Confirm.WasPressedThisFrame())
            {
                if (!game.Player.Forms.IsAvailable(Highlight))
                {
                    Sfx.Play(SfxId.TransformFail, 0.5f);
                    game.ShowToast(FormLibrary.Get(Highlight).DisplayName + " isn't sewn yet.");
                    return;
                }
                Sfx.Play(SfxId.MenuConfirm, 0.5f);
                game.CloseWheel(true, Highlight);
            }
            else if (input.Cancel.WasPressedThisFrame() || input.FormWheel.WasPressedThisFrame())
            {
                game.CloseWheel(false, Highlight);
            }
        }

        void SetHighlight(FormType f)
        {
            if (f == Highlight) return;
            Highlight = f;
            Sfx.Play(SfxId.MenuMove, 0.4f);
        }

        public void Draw()
        {
            bool kb = game.Input.UsingKeyboard;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(openTime / 0.12f));
            float W = UIKit.Width, H = UIKit.Height;

            UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.1f, 0.07f, 0.15f, 0.62f * k));

            Vector2 c = new Vector2(W * 0.5f, H * 0.47f);
            float ringD = 640f * Mathf.Lerp(0.85f, 1f, k);
            UIKit.Circle(c + new Vector2(4f, 8f), ringD, new Color(0f, 0f, 0f, 0.2f * k));
            UIKit.Circle(c, ringD, new Color(0.99f, 0.95f, 0.88f, 0.92f * k));
            UIKit.CircleOutline(c, ringD - 26f, new Color(0.85f, 0.45f, 0.55f, 0.55f * k), 3f);

            var current = game.Player.Forms.Current;
            for (int i = 0; i < Slots.Length; i++)
            {
                var form = Slots[i];
                var def = FormLibrary.Get(form);
                bool hi = form == Highlight;
                bool unlocked = game.Player.Forms.IsAvailable(form);
                float a = SlotAngles[i] * Mathf.Deg2Rad;
                Vector2 p = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 205f * k;
                float d = hi ? 190f : 150f;

                Color bg = unlocked ? def.PrimaryColor : new Color(0.6f, 0.6f, 0.62f);
                if (!hi) bg = Color.Lerp(bg, Color.white, 0.25f);
                bg.a = k;
                if (hi) UIKit.Circle(p, d + 26f, new Color(1f, 0.85f, 0.4f, 0.55f * k));
                UIKit.Circle(p, d, bg);
                UIKit.CircleOutline(p, d - 14f, new Color(1f, 1f, 1f, 0.8f * k), hi ? 4f : 2f);
                float iconSize = d * 0.62f;
                UIKit.Icon(new Rect(p.x - iconSize * 0.5f, p.y - iconSize * 0.5f, iconSize, iconSize),
                    unlocked ? PatchworkAssets.IconFor(form) : PatchworkAssets.IconLock, new Color(1f, 1f, 1f, k));

                var nameStyle = UIKit.Style(hi ? 24 : 21, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, k));
                float nw = UIKit.TextWidth(def.DisplayName, nameStyle) + 30f;
                var nameRect = new Rect(p.x - nw * 0.5f, p.y + d * 0.5f + 4f, nw, 36f);
                UIKit.Pill(nameRect, k);
                UIKit.Label(nameRect, def.DisplayName, nameStyle);

                if (form == current)
                {
                    var tag = new Rect(p.x - 46f, p.y - d * 0.5f - 18f, 92f, 28f);
                    UIKit.Fill(tag, new Color(UIKit.Ink.r, UIKit.Ink.g, UIKit.Ink.b, 0.9f * k), 14f);
                    UIKit.Label(tag, "CURRENT", UIKit.Style(16, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, k)));
                }
            }

            // Only the name sits in the middle, in the gap between the three pictures. The longer
            // description goes in its own card above the wheel so it never runs into them.
            var hdef = FormLibrary.Get(Highlight);
            UIKit.Label(new Rect(c.x - 110f, c.y - 34f, 220f, 44f), hdef.DisplayName,
                UIKit.Style(30, true, TextAnchor.MiddleCenter, new Color(UIKit.Ink.r, UIKit.Ink.g, UIKit.Ink.b, k)));

            var tagStyle = UIKit.Style(24, false, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, k), true);
            const float tagW = 860f, tagH = 84f;
            var tagRect = new Rect(c.x - tagW * 0.5f, c.y - ringD * 0.5f - tagH - 34f, tagW, tagH);
            UIKit.Pill(tagRect, k);
            UIKit.Label(new Rect(tagRect.x + 30f, tagRect.y + 6f, tagRect.width - 60f, tagRect.height - 12f), hdef.Tagline, tagStyle);

            // Prompts.
            float y = c.y + ringD * 0.5f + 28f;
            var white = new Color(1f, 1f, 1f, k);
            string transformLabel = Highlight == current ? "Keep form" : "Transform";
            float w1 = 44f + 10f + UIKit.TextWidth(transformLabel, UIKit.Style(24, true, TextAnchor.MiddleLeft, white));
            float w2 = 44f + 10f + UIKit.TextWidth("Cancel", UIKit.Style(24, true, TextAnchor.MiddleLeft, white));
            float x = c.x - (w1 + w2 + 40f) * 0.5f;
            UIKit.Pill(new Rect(x - 24f, y - 12f, w1 + w2 + 40f + 48f, 68f), k);
            x += UIKit.PromptLabel(x, y, 44f, PromptAction.Confirm, transformLabel, kb, Color.white, k) + 40f;
            UIKit.PromptLabel(x, y, 44f, PromptAction.Cancel, "Cancel", kb, Color.white, k);
            string pointHint = kb ? "WASD / Arrows / 1-2-3 to choose" : "Left stick or D-pad to choose";
            var hintStyle = UIKit.Style(18, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, k));
            float hw = UIKit.TextWidth(pointHint, hintStyle) + 40f;
            var hintRect = new Rect(c.x - hw * 0.5f, y + 64f, hw, 34f);
            UIKit.Pill(hintRect, k);
            UIKit.Label(hintRect, pointHint, hintStyle);
        }
    }
}

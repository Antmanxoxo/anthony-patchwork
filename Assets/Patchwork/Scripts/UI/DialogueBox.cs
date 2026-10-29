using UnityEngine;

namespace Patchwork
{
    // Talking to an NPC: a felt speech card at the bottom of the screen with their name on a tag.
    // Text types out a letter at a time with little voice blips; Confirm (or Ability) shows the
    // whole line, then moves on. Back ends the chat early.
    public class DialogueBox
    {
        readonly PatchworkGame game;
        NpcYarny npc;
        string[] lines;
        int line;
        float shown;          // characters shown so far
        float openTime;
        int lastBlipChar;
        const float CharsPerSecond = 42f;

        public DialogueBox(PatchworkGame game)
        {
            this.game = game;
        }

        public void Open(NpcYarny speaker)
        {
            npc = speaker;
            lines = speaker.NextLines();
            line = 0;
            shown = 0f;
            lastBlipChar = 0;
            openTime = 0f;
            npc.Talking = true;

            // Over Patch's shoulder, looking at the captain.
            var cam = game.CameraRig;
            Vector3 p = game.Player.transform.position;
            Vector3 n = npc.transform.position;
            Vector3 away = p - n;
            away.y = 0f;
            if (away.sqrMagnitude < 0.01f) away = Vector3.back;
            away.Normalize();
            Vector3 side = Vector3.Cross(Vector3.up, away);
            cam.ShowcasePosition = p + away * 3.2f + side * 1.6f + Vector3.up * 1.9f;
            cam.ShowcaseLookAt = Vector3.Lerp(p + Vector3.up * 1f, npc.HeadPosition, 0.7f);
            cam.ShowcaseBlendSpeed = 1.8f;
            cam.Showcase = true;

            // Patch faces the captain.
            game.Player.Motor.Teleport(game.Player.transform.position, Mathf.Atan2(-away.x, -away.z) * Mathf.Rad2Deg);
        }

        public void Close()
        {
            if (npc != null) npc.Talking = false;
            game.CameraRig.ShowcaseBlendSpeed = 1.6f;
            game.CameraRig.Showcase = false;
        }

        string Current { get { return lines != null && line < lines.Length ? lines[line] : ""; } }

        public void Update(float udt)
        {
            openTime += udt;
            string text = Current;
            if (shown < text.Length)
            {
                shown = Mathf.Min(text.Length, shown + udt * CharsPerSecond);
                int c = Mathf.FloorToInt(shown);
                // A blip every few letters (not on spaces).
                if (c - lastBlipChar >= 3 && c > 0 && c <= text.Length && text[c - 1] != ' ')
                {
                    lastBlipChar = c;
                    npc.Blip();
                }
            }

            var input = game.Input;
            if (openTime < 0.2f) return;
            if (input.Cancel.WasPressedThisFrame())
            {
                game.EndTalk();
                return;
            }
            if (input.Confirm.WasPressedThisFrame() || input.Ability.WasPressedThisFrame())
            {
                if (shown < text.Length)
                {
                    shown = text.Length;
                }
                else
                {
                    line++;
                    shown = 0f;
                    lastBlipChar = 0;
                    if (line >= lines.Length) game.EndTalk();
                    else Sfx.Play(SfxId.MenuMove, 0.35f);
                }
            }
        }

        public void Draw(bool kb)
        {
            float W = UIKit.Width, H = UIKit.Height;
            float a = Mathf.Clamp01(openTime / 0.25f);
            float rise = (1f - UIKit.EaseOutCubic(a)) * 60f;
            var ink = UIKit.Ink;
            var box = new Rect((W - 1240f) * 0.5f, H - 300f + rise, 1240f, 240f);
            UIKit.Panel(box, new Color(0.99f, 0.96f, 0.9f, 0.97f * a), new Color(0.66f, 0.3f, 0.32f, 0.85f * a));

            // Name tag.
            var nameStyle = UIKit.Style(28, true, TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.85f, a));
            float nw = UIKit.TextWidth(NpcYarny.DisplayName, nameStyle) + 56f;
            var tag = new Rect(box.x + 46f, box.y - 28f, nw, 54f);
            UIKit.Fill(new Rect(tag.x + 3f, tag.y + 5f, tag.width, tag.height), new Color(0f, 0f, 0f, 0.2f * a), 27f);
            UIKit.Fill(tag, new Color(0.62f, 0.27f, 0.3f, a), 27f);
            UIKit.Label(tag, NpcYarny.DisplayName, nameStyle);

            string text = Current;
            int count = Mathf.Clamp(Mathf.FloorToInt(shown), 0, text.Length);
            UIKit.Label(new Rect(box.x + 60f, box.y + 46f, box.width - 120f, box.height - 80f), text.Substring(0, count),
                UIKit.Style(30, false, TextAnchor.UpperLeft, new Color(ink.r, ink.g, ink.b, a), true));

            if (count >= text.Length && text.Length > 0)
            {
                float bob = Mathf.Sin(Time.unscaledTime * 6f) * 5f;
                bool last = line >= lines.Length - 1;
                UIKit.PromptLabel(box.xMax - 250f, box.yMax - 64f + bob, 44f, PromptAction.Confirm, last ? "Bye!" : "Next", kb, ink, a);
            }
        }
    }
}

using UnityEngine;

namespace Patchwork
{
    // What you see when the game starts: the Little Colossus studio logo, then the Patchwork title
    // over a slow fly-around of the Cardboard Skiff. Any button starts the game and the camera
    // swoops down to Patch. Only plays the first time the Skiff loads after starting the game.
    public class TitleSequence
    {
        enum Stage { Studio, Title, Done }

        // Put the real logo at Assets/Patchwork/Resources/Branding/StudioLogo.png and it's used
        // automatically; until then a placeholder badge is drawn.
        const string LogoResource = "Branding/StudioLogo";
        const string StudioName = "LITTLE COLOSSUS";

        static bool playedThisSession;

        readonly PatchworkGame game;
        Stage stage;
        float time;          // time in the current stage
        float studioFade;    // the dark studio background (1 = fully dark)
        float titleAlpha;
        float orbit;
        bool stingPlayed;
        Texture2D logo;

        public bool Active { get { return stage != Stage.Done; } }

        public static void ResetStatics() { playedThisSession = false; }

        // Should the game open with this? (First Skiff load since the game started.)
        public static bool ShouldPlay()
        {
            return !playedThisSession;
        }

        public TitleSequence(PatchworkGame game)
        {
            this.game = game;
            stage = Stage.Done;
        }

        public void Start()
        {
            playedThisSession = true;
            stage = Stage.Studio;
            time = 0f;
            studioFade = 1f;
            titleAlpha = 0f;
            orbit = 200f;
            logo = Resources.Load<Texture2D>(LogoResource);
            MusicPlayer.SetMuffle(22000f, 0f);
            PlaceCamera(0f);
            game.CameraRig.Showcase = true;
            game.CameraRig.SnapShowcase();
        }

        bool AnyButton()
        {
            var i = game.Input;
            return i.Confirm.WasPressedThisFrame() || i.Jump.WasPressedThisFrame() || i.Pause.WasPressedThisFrame() || i.Ability.WasPressedThisFrame();
        }

        // Returns true on the frame the player starts the game.
        public bool Update(float udt)
        {
            if (stage == Stage.Done) return false;
            time += udt;
            PlaceCamera(udt);

            if (stage == Stage.Studio)
            {
                if (!stingPlayed && time > 0.35f) { stingPlayed = true; Sfx.Play(SfxId.StudioSting, 0.7f); }
                bool skip = time > 0.6f && AnyButton();
                if (time > 4.4f || skip)
                {
                    stage = Stage.Title;
                    time = 0f;
                    MusicPlayer.SetMuffle(22000f, 1f);
                }
                return false;
            }

            // Title.
            studioFade = Mathf.MoveTowards(studioFade, 0f, udt * 1.2f);
            titleAlpha = Mathf.MoveTowards(titleAlpha, 1f, udt * 0.9f);
            if (time > 0.8f && AnyButton())
            {
                stage = Stage.Done;
                game.CameraRig.ShowcaseBlendSpeed = 0.75f;
                game.CameraRig.Showcase = false;
                Sfx.Play(SfxId.MenuConfirm, 0.6f);
                return true;
            }
            return false;
        }

        // A slow circle around the Skiff, a little above deck height.
        void PlaceCamera(float udt)
        {
            orbit += udt * 7f;
            float a = orbit * Mathf.Deg2Rad;
            Vector3 center = new Vector3(0f, 5f, 2f);
            game.CameraRig.ShowcasePosition = center + new Vector3(Mathf.Sin(a) * 36f, 9f + Mathf.Sin(orbit * 0.02f) * 2f, Mathf.Cos(a) * 36f);
            game.CameraRig.ShowcaseLookAt = center + Vector3.up * 3f;
        }

        // ------------------------------------------------------------------ Drawing

        public void Draw(bool kb)
        {
            if (stage == Stage.Done) return;
            float W = UIKit.Width, H = UIKit.Height;

            if (stage == Stage.Title) DrawTitle(W, H, kb);

            float dark = stage == Stage.Studio ? 1f : studioFade;
            if (dark > 0.001f)
            {
                UIKit.Fill(new Rect(0f, 0f, W, H), new Color(0.09f, 0.06f, 0.1f, dark));
                if (stage == Stage.Studio)
                {
                    float a = Mathf.Clamp01((time - 0.4f) / 0.8f) * Mathf.Clamp01((4.2f - time) / 0.7f);
                    float grow = 0.94f + 0.06f * Mathf.Clamp01((time - 0.4f) / 3.5f);
                    DrawStudioLogo(W, H, a, grow);
                }
            }
        }

        void DrawStudioLogo(float W, float H, float a, float scale)
        {
            if (a <= 0f) return;
            Vector2 c = new Vector2(W * 0.5f, H * 0.44f);
            if (logo != null)
            {
                float max = 560f * scale;
                float aspect = logo.width / (float)Mathf.Max(1, logo.height);
                float w = aspect >= 1f ? max : max * aspect;
                float h = aspect >= 1f ? max / aspect : max;
                UIKit.Icon(new Rect(c.x - w * 0.5f, c.y - h * 0.5f, w, h), logo, new Color(1f, 1f, 1f, a));
                return;
            }

            // Placeholder: a felt badge with a big "LC" and the studio name under it.
            float d = 300f * scale;
            UIKit.Circle(c + new Vector2(0f, 8f), d, new Color(0f, 0f, 0f, 0.35f * a));
            UIKit.Circle(c, d, new Color(0.95f, 0.88f, 0.76f, a));
            UIKit.CircleOutline(c, d - 34f, new Color(0.62f, 0.42f, 0.3f, 0.8f * a), 4f);
            // A little figure with a giant shadow behind it: small person, colossal ambition.
            UIKit.Fill(new Rect(c.x - 60f * scale, c.y - 100f * scale, 120f * scale, 160f * scale), new Color(0.62f, 0.42f, 0.3f, 0.35f * a), 60f * scale);
            UIKit.Circle(c + new Vector2(0f, -10f) * scale, 34f * scale, new Color(0.25f, 0.17f, 0.22f, a));
            UIKit.Fill(new Rect(c.x - 16f * scale, c.y + 8f * scale, 32f * scale, 52f * scale), new Color(0.25f, 0.17f, 0.22f, a), 14f * scale);
            var mono = UIKit.Style(40, true, TextAnchor.MiddleCenter, new Color(0.62f, 0.42f, 0.3f, a));
            UIKit.Label(new Rect(c.x - 100f, c.y + d * 0.5f - 76f, 200f, 50f), "LC", mono);

            var name = UIKit.Style(64, true, TextAnchor.MiddleCenter, new Color(0.97f, 0.92f, 0.84f, a));
            UIKit.Label(new Rect(0f, c.y + d * 0.5f + 40f, W, 80f), Spaced(StudioName), name);
            UIKit.Fill(new Rect(W * 0.5f - 180f, c.y + d * 0.5f + 128f, 360f, 3f), new Color(0.97f, 0.92f, 0.84f, 0.5f * a), 1.5f);
        }

        static string Spaced(string s)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                sb.Append(s[i]);
                if (i < s.Length - 1) sb.Append(' ');
            }
            return sb.ToString();
        }

        void DrawTitle(float W, float H, bool kb)
        {
            float a = titleAlpha;
            if (a <= 0f) return;
            // Soft vignette at the top so the title reads over the sky.
            UIKit.Fill(new Rect(0f, 0f, W, H * 0.45f), new Color(0.12f, 0.08f, 0.18f, 0.18f * a));

            float bob = Mathf.Sin(Time.unscaledTime * 1.3f) * 6f;
            float y = H * 0.16f + bob;
            var big = UIKit.Style(150, true, TextAnchor.MiddleCenter, new Color(1f, 0.97f, 0.9f, a));
            UIKit.Label(new Rect(0f, y, W, 190f), "Patchwork", big, true);
            // Stitched underline in yarn pink.
            float lw = UIKit.TextWidth("Patchwork", big) * 0.8f;
            for (float x = 0f; x < lw; x += 34f)
                UIKit.Fill(new Rect(W * 0.5f - lw * 0.5f + x, y + 178f, 22f, 7f), new Color(0.95f, 0.45f, 0.62f, a), 3.5f);
            UIKit.Label(new Rect(0f, y + 200f, W, 44f), "A little toy with three forms and a whole world to stitch together.",
                UIKit.Style(26, false, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.95f * a)), true);

            // Press to start (pulses).
            float pulse = 0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * 3f);
            if (time > 0.8f)
            {
                var label = UIKit.Style(30, true, TextAnchor.MiddleLeft, new Color(1f, 1f, 1f, a * pulse));
                const string text = "to start";
                float pw = 60f + 14f + UIKit.TextWidth(text, label);
                float px = (W - pw) * 0.5f, py = H * 0.78f;
                UIKit.Pill(new Rect(px - 26f, py - 10f, pw + 52f, 80f), a * 0.8f);
                UIKit.Prompt(px, py, 60f, PromptAction.Confirm, kb, a * pulse);
                UIKit.Label(new Rect(px + 74f, py, pw, 60f), text, label);
            }

            UIKit.Label(new Rect(30f, H - 60f, 600f, 40f), "© Little Colossus",
                UIKit.Style(20, false, TextAnchor.MiddleLeft, new Color(1f, 1f, 1f, 0.75f * a)), true);
        }
    }
}

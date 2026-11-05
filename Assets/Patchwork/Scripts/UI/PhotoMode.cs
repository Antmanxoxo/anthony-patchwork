using System.IO;
using UnityEngine;

namespace Patchwork
{
    // Photo Mode (from the pause menu). Time stops and the camera floats free around Patch:
    // look to orbit, move to slide the focus, L1 / R1 (Z / X) to zoom. Left / right on the D-pad
    // (arrow keys) swaps the screen look, up / down changes Patch's face. Cross / Enter takes the
    // picture (saved as a PNG in the game's Photos folder), Square / E hides the controls.
    public class PhotoMode
    {
        readonly PatchworkGame game;
        Vector3 focus;
        float yaw, pitch, distance;
        bool hideUI;
        float flash, savedTimer, captureDelay = -1f;
        int hiddenFrames;
        string savedName;

        public bool Capturing { get { return captureDelay >= 0f; } }
        public bool HideUI { get { return hideUI || Capturing || hiddenFrames > 0; } }

        public PhotoMode(PatchworkGame game) { this.game = game; }

        public static string Folder { get { return Path.Combine(Application.persistentDataPath, "Photos"); } }

        public void Open()
        {
            var cam = game.CameraRig;
            Transform t = cam.transform;
            focus = game.Player.Motor.Center + Vector3.up * 0.3f;
            Vector3 off = t.position - focus;
            distance = Mathf.Clamp(off.magnitude, 2f, 12f);
            yaw = Mathf.Atan2(off.x, off.z) * Mathf.Rad2Deg;
            pitch = Mathf.Asin(Mathf.Clamp(off.y / Mathf.Max(0.01f, off.magnitude), -1f, 1f)) * Mathf.Rad2Deg;
            hideUI = false;
            flash = 0f;
            savedTimer = 0f;
            captureDelay = -1f;
            Place();
            cam.ShowcaseBlendSpeed = 100f;
            cam.Showcase = true;
            cam.SnapShowcase();
        }

        public void Close()
        {
            var cam = game.CameraRig;
            cam.ShowcaseBlendSpeed = 2.5f;
            cam.Showcase = false;
        }

        // Returns false when the player backs out.
        public bool Update(float udt)
        {
            var input = game.Input;
            if (flash > 0f) flash -= udt * 2.5f;
            if (savedTimer > 0f) savedTimer -= udt;
            if (hiddenFrames > 0)
            {
                // Keep the screen clean until the picture has been taken, then flash.
                if (--hiddenFrames == 0) { flash = 1f; savedTimer = 2.6f; }
                return true;
            }

            if (captureDelay >= 0f)
            {
                // The UI is hidden for this frame; grab the screen once it has drawn without it.
                captureDelay -= 1f;
                if (captureDelay < 0f) Capture();
                return true;
            }

            if (input.Cancel.WasPressedThisFrame() || input.Pause.WasPressedThisFrame()) return false;

            Vector2 stick = input.LookStick.ReadValue<Vector2>();
            Vector2 mouse = Cursor.lockState == CursorLockMode.Locked ? input.LookMouse.ReadValue<Vector2>() : Vector2.zero;
            float speed = GameSettings.CameraSpeedMultiplier;
            yaw += (stick.x * 120f * udt + mouse.x * 0.12f) * speed * (GameSettings.InvertX ? -1f : 1f);
            pitch -= (stick.y * 90f * udt + mouse.y * 0.1f) * speed * (GameSettings.InvertY ? -1f : 1f);
            pitch = Mathf.Clamp(pitch, -35f, 80f);

            Vector2 move = Vector2.ClampMagnitude(input.Move.ReadValue<Vector2>(), 1f);
            Quaternion flat = Quaternion.Euler(0f, yaw + 180f, 0f);
            focus += (flat * new Vector3(move.x, 0f, move.y)) * 4f * udt;
            // Don't let the focus wander off: stay within 8m of Patch.
            Vector3 home = game.Player.Motor.Center;
            Vector3 d = focus - home;
            if (d.magnitude > 8f) focus = home + d.normalized * 8f;
            if (input.Jump.IsPressed()) focus.y += 2.5f * udt;

            float zoom = (input.RotateRight.IsPressed() ? 1f : 0f) - (input.RotateLeft.IsPressed() ? 1f : 0f);
            float wheel = UnityEngine.InputSystem.Mouse.current != null ? UnityEngine.InputSystem.Mouse.current.scroll.ReadValue().y : 0f;
            distance = Mathf.Clamp(distance - zoom * 5f * udt - wheel * 0.004f, 1.2f, 14f);

            if (input.NavLeft.WasPressedThisFrame()) SetLook(-1);
            if (input.NavRight.WasPressedThisFrame()) SetLook(1);
            if (input.NavUp.WasPressedThisFrame() || input.NavDown.WasPressedThisFrame())
            {
                var next = (PatchModel.Face)(((int)PatchModel.CurrentFace + (input.NavUp.WasPressedThisFrame() ? 1 : 2)) % 3);
                game.SetFace(next);
            }
            if (input.Ability.WasPressedThisFrame())
            {
                hideUI = !hideUI;
                Sfx.Play(SfxId.MenuMove, 0.4f);
            }
            if (input.Confirm.WasPressedThisFrame())
            {
                captureDelay = 1f;
            }

            Place();
            return true;
        }

        void SetLook(int dir)
        {
            GameSettings.Look = (GameSettings.Look + (dir < 0 ? 2 : 1)) % 3;
            GameSettings.ApplyLook();
            Sfx.Play(SfxId.MenuMove, 0.45f);
        }

        void Place()
        {
            var cam = game.CameraRig;
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 want = focus + rot * new Vector3(0f, 0f, 1f) * distance;
            // Keep the lens out of walls.
            Vector3 dir = want - focus;
            RaycastHit hit;
            if (Physics.SphereCast(focus, 0.25f, dir.normalized, out hit, dir.magnitude, ~(1 << 2), QueryTriggerInteraction.Ignore))
                want = focus + dir.normalized * Mathf.Max(0.4f, hit.distance - 0.1f);
            cam.ShowcasePosition = want;
            cam.ShowcaseLookAt = focus;
        }

        void Capture()
        {
            try
            {
                Directory.CreateDirectory(Folder);
                savedName = "Patchwork_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
                ScreenCapture.CaptureScreenshot(Path.Combine(Folder, savedName));
                Merits.Earn(Merits.Id.Shutterbug);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Patchwork: couldn't save the photo (" + e.Message + ")");
                savedName = null;
            }
            hiddenFrames = 2;
            Sfx.Play(SfxId.MenuConfirm, 0.7f);
            Sfx.Play(SfxId.Pop, 0.4f);
            Haptics.Pulse(0.2f, 0.1f, 0.08f);
        }

        public void Draw(bool kb)
        {
            if (flash > 0f && hiddenFrames == 0) UIKit.Fill(new Rect(0f, 0f, UIKit.Width, UIKit.Height), new Color(1f, 1f, 1f, Mathf.Clamp01(flash) * 0.85f));
            if (HideUI) return;

            // A little viewfinder: corner brackets.
            Color frame = new Color(1f, 1f, 1f, 0.55f);
            float m = 40f, L = 70f, w = 5f;
            float W = UIKit.Width, H = UIKit.Height;
            UIKit.Fill(new Rect(m, m, L, w), frame); UIKit.Fill(new Rect(m, m, w, L), frame);
            UIKit.Fill(new Rect(W - m - L, m, L, w), frame); UIKit.Fill(new Rect(W - m - w, m, w, L), frame);
            UIKit.Fill(new Rect(m, H - m - w, L, w), frame); UIKit.Fill(new Rect(m, H - m - L, w, L), frame);
            UIKit.Fill(new Rect(W - m - L, H - m - w, L, w), frame); UIKit.Fill(new Rect(W - m - w, H - m - L, w, L), frame);

            var title = UIKit.Style(30, true, TextAnchor.MiddleLeft, Color.white);
            UIKit.Label(new Rect(m + 30f, m + 20f, 600f, 44f), "Photo Mode", title, true);
            var sub = UIKit.Style(22, true, TextAnchor.MiddleLeft, new Color(1f, 0.92f, 0.8f));
            UIKit.Label(new Rect(m + 30f, m + 62f, 600f, 34f), "Look: " + GameSettings.LookNames[GameSettings.Look] + "   <  >", sub, true);

            float h = 40f, y = H - m - 70f, x = m + 30f;
            Color tc = Color.white;
            var bar = new Rect(x - 18f, y - 10f, kb ? 1240f : 1120f, h + 20f);
            UIKit.Fill(bar, new Color(0.13f, 0.11f, 0.19f, 0.6f), 30f);
            x += UIKit.PromptLabel(x, y, h, PromptAction.Confirm, "Snap", kb, tc) + 40f;
            x += UIKit.PromptLabel(x, y, h, PromptAction.Rotate, "Zoom", kb, tc) + 40f;
            x += UIKit.PromptLabel(x, y, h, PromptAction.Navigate, "Look / Face", kb, tc) + 40f;
            x += UIKit.PromptLabel(x, y, h, PromptAction.Ability, "Hide", kb, tc) + 40f;
            UIKit.PromptLabel(x, y, h, PromptAction.Cancel, "Done", kb, tc);

            if (savedTimer > 0f)
            {
                float a = Mathf.Clamp01(savedTimer / 0.4f);
                string msg = savedName != null ? "Photo saved!" : "Couldn't save the photo.";
                var st = UIKit.Style(24, true, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, a));
                float tw = UIKit.TextWidth(msg, st) + 60f;
                var r = new Rect((W - tw) * 0.5f, H * 0.14f, tw, 54f);
                UIKit.Fill(r, new Color(0.13f, 0.11f, 0.19f, 0.82f * a), 27f);
                UIKit.Label(r, msg, st);
            }
        }
    }
}

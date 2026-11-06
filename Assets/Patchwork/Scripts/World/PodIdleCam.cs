using UnityEngine;

namespace Patchwork
{
    // Leave Patch alone in the Pod for a while and the camera drifts outside: the whole Pod
    // floating in space, stars all round, turning very slowly, like a screensaver. Touch any
    // button or stick and it glides straight back inside.
    // The Pod's outside (an egg of cardboard with a glowing porthole, a zigzag band, fins and an
    // antenna) is only switched on while you're looking at it, so it never blocks the window.
    public class PodIdleCam : MonoBehaviour
    {
        const float WaitSeconds = 40f;
        static readonly Vector3 Centre = new Vector3(0f, 5f, 0f);

        public GameObject Exterior;
        float idle, angle, active;
        bool on;
        readonly System.Collections.Generic.List<Renderer> backRoom = new System.Collections.Generic.List<Renderer>();

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) return;
            var input = game.Input;
            bool touched =
                input.Move.ReadValue<Vector2>().sqrMagnitude > 0.01f ||
                input.LookStick.ReadValue<Vector2>().sqrMagnitude > 0.01f ||
                input.LookMouse.ReadValue<Vector2>().sqrMagnitude > 0.5f ||
                input.Jump.IsPressed() || input.Ability.IsPressed() || input.Confirm.IsPressed() || input.Cancel.IsPressed() ||
                input.Pause.IsPressed() || input.Map.IsPressed() || input.FormWheel.IsPressed() || input.Satchel.IsPressed() ||
                input.Grab.IsPressed() || input.NavUp.IsPressed() || input.NavDown.IsPressed() || input.NavLeft.IsPressed() || input.NavRight.IsPressed();
            bool inGame = game.Mode == UIMode.Gameplay && !game.Player.Motor.ExternalControl;

            if (on && (touched || !inGame)) Stop(game);
            if (on)
            {
                active += Time.unscaledDeltaTime;
                angle += Time.unscaledDeltaTime * 4f;   // degrees a second: very slow
                var cam = game.CameraRig;
                float a = angle * Mathf.Deg2Rad;
                float dist = Mathf.Lerp(24f, 34f, Mathf.Clamp01(active / 6f));
                cam.ShowcasePosition = Centre + new Vector3(Mathf.Sin(a) * dist, 7f + Mathf.Sin(a * 0.5f) * 4f, Mathf.Cos(a) * dist);
                cam.ShowcaseLookAt = Centre;
                return;
            }

            if (touched || !inGame || Flat(game.Player.Motor.Velocity) > 0.05f) { idle = 0f; return; }
            idle += Time.unscaledDeltaTime;
            if (idle >= WaitSeconds) Begin(game);
        }

        // Sideways speed only: standing still on the floor the motor still presses down a little.
        static float Flat(Vector3 v) { return v.x * v.x + v.z * v.z; }

        void Begin(PatchworkGame game)
        {
            on = true;
            active = 0f;
            // Start from behind the camera's current view so it glides out smoothly.
            Vector3 from = game.CameraRig.transform.position - Centre;
            angle = Mathf.Atan2(from.x, from.z) * Mathf.Rad2Deg;
            if (Exterior != null) Exterior.SetActive(true);
            // The Button Box room sticks out of the back of the Pod: hide it while we're outside.
            backRoom.Clear();
            var root = game.LevelRoot != null ? game.LevelRoot : transform;
            foreach (var r in root.GetComponentsInChildren<Renderer>())
            {
                if (!r.enabled || (Exterior != null && r.transform.IsChildOf(Exterior.transform))) continue;
                Vector3 c = r.bounds.center;
                if (c.z < -HubBuilder.RadiusAt(Mathf.Clamp(c.y, 0f, 13.4f)) + 0.1f && Mathf.Abs(c.x) < 3.6f && c.y < 7f && c.z > -16f)
                {
                    r.enabled = false;
                    backRoom.Add(r);
                }
            }
            var cam = game.CameraRig;
            cam.ShowcaseBlendSpeed = 0.35f;
            cam.ShowcasePosition = Centre + new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad) * 24f, 7f, Mathf.Cos(angle * Mathf.Deg2Rad) * 24f);
            cam.ShowcaseLookAt = Centre;
            cam.Showcase = true;
        }

        void Stop(PatchworkGame game)
        {
            on = false;
            idle = 0f;
            var cam = game.CameraRig;
            cam.ShowcaseBlendSpeed = 3f;
            cam.Showcase = false;
            if (Exterior != null) Exterior.SetActive(false);
            foreach (var r in backRoom) if (r != null) r.enabled = true;
            backRoom.Clear();
        }

        // ------------------------------------------------------------------ The Pod's outside

        public static GameObject BuildExterior(LevelKit kit)
        {
            var g = kit.Group("PodExterior", Vector3.zero);
            var kraft = PatchworkAssets.Cardboard(new Color(0.8f, 0.62f, 0.42f));
            var seam = PatchworkAssets.Cardboard(new Color(0.62f, 0.45f, 0.3f));
            var brass = PatchworkAssets.Plain(new Color(0.95f, 0.75f, 0.35f), 0.85f);
            const float cy = 5f, rx = 11f, ry = 10f;
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, cy, 0f), new Vector3(rx * 2f, ry * 2f, rx * 2f), kraft, default(Vector3), g);
            foreach (float r in new[] { 30f, 75f, 120f, 165f, 210f, 255f, 300f, 345f })
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, cy, 0f), new Vector3(rx * 2f + 0.1f, ry * 2f + 0.06f, 0.12f), seam, new Vector3(0f, r, 0f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, cy, 0f), new Vector3(rx * 2f + 0.15f, 0.4f, rx * 2f + 0.15f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, new Color(0.2f, 0.25f, 0.55f), new Color(1f, 0.8f, 0.28f)), default(Vector3), g);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, cy + s * 0.45f, 0f), new Vector3(rx * 2f + 0.1f, 0.06f, rx * 2f + 0.1f), brass, default(Vector3), g);

            // The big porthole at the front, glowing warm from the room inside.
            float wy = 4.3f, k = (wy - cy) / ry, wz = rx * Mathf.Sqrt(1f - k * k);
            var win = kit.Group("Porthole", new Vector3(0f, wy, wz - 0.1f));
            win.SetParent(g, true);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(7.6f, 0.2f, 7.6f), brass, new Vector3(90f, 0f, 0f), win);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.12f), new Vector3(6.6f, 0.15f, 6.6f), PatchworkAssets.Glow(new Color(1f, 0.85f, 0.6f), 1.3f), new Vector3(90f, 0f, 0f), win);
            kit.Deco(PrimitiveType.Sphere, new Vector3(-1.4f, 1.3f, 0.25f), new Vector3(1.8f, 0.6f, 0.1f), PatchworkAssets.Glow(Color.white, 1.6f), new Vector3(0f, 0f, 30f), win);
            for (int i = 0; i < 14; i++)
            {
                float a = i * Mathf.PI * 2f / 14f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 3.55f, Mathf.Sin(a) * 3.55f, 0.15f), Vector3.one * 0.3f, brass, default(Vector3), win);
            }
            // Three striped tail fins.
            var fin = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.9f, 0.3f, 0.35f), new Color(0.99f, 0.95f, 0.88f));
            foreach (float a in new[] { 60f, 180f, 300f })
            {
                Vector3 d = new Vector3(Mathf.Sin(a * Mathf.Deg2Rad), 0f, Mathf.Cos(a * Mathf.Deg2Rad));
                var f = kit.Deco(PrimitiveType.Cube, d * 9.2f + Vector3.up * -1.5f, new Vector3(0.4f, 6f, 4f), fin, default(Vector3), g);
                f.rotation = Quaternion.LookRotation(d, Vector3.up) * Quaternion.Euler(-30f, 0f, 0f);
            }
            // The engine nozzle glowing underneath.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -4.8f, 0f), new Vector3(5f, 0.8f, 5f), PatchworkAssets.Plain(new Color(0.35f, 0.33f, 0.38f), 0.6f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -5.6f, 0f), new Vector3(4f, 0.05f, 4f), PatchworkAssets.Glow(new Color(1f, 0.55f, 0.25f), 2f), default(Vector3), g);
            // The antenna on top, with a blinking light.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, cy + ry + 1.3f, 0f), new Vector3(0.18f, 1.6f, 0.18f), PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.9f), 0.7f), default(Vector3), g);
            var blink = kit.Deco(PrimitiveType.Sphere, new Vector3(0f, cy + ry + 3.1f, 0f), Vector3.one * 0.7f, PatchworkAssets.Glow(new Color(1f, 0.35f, 0.4f), 2.4f), default(Vector3), g);
            blink.gameObject.AddComponent<Blinker>();
            // Stitched patches here and there, like it's been mended after every trip.
            Color[] patch = { new Color(0.45f, 0.65f, 0.95f), new Color(0.95f, 0.6f, 0.78f), new Color(0.6f, 0.85f, 0.5f) };
            float[] pa = { 110f, 230f, 330f };
            float[] py = { 8f, 2f, 9.5f };
            for (int i = 0; i < pa.Length; i++)
            {
                float kk = (py[i] - cy) / ry, rr = rx * Mathf.Sqrt(Mathf.Max(0.01f, 1f - kk * kk));
                Vector3 d = new Vector3(Mathf.Sin(pa[i] * Mathf.Deg2Rad), 0f, Mathf.Cos(pa[i] * Mathf.Deg2Rad));
                Vector3 n = new Vector3(d.x * rr / (rx * rx), (py[i] - cy) / (ry * ry), d.z * rr / (rx * rx)).normalized;
                var p = kit.Deco(PrimitiveType.Cube, d * rr + Vector3.up * py[i] + n * 0.05f, new Vector3(2.2f, 1.8f, 0.06f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, new Color(0.99f, 0.96f, 0.9f), patch[i]), default(Vector3), g);
                p.rotation = Quaternion.LookRotation(n, Vector3.up) * Quaternion.Euler(0f, 0f, i * 17f);
            }
            g.gameObject.SetActive(false);
            return g.gameObject;
        }
    }
}

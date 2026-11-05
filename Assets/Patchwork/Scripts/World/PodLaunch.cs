using UnityEngine;

namespace Patchwork
{
    // Arriving in the Pod: Patch waits inside the launch tube, then gets popped out of it and
    // sails across the pod to land on the rug.
    public class PodLaunch : MonoBehaviour
    {
        public static bool Busy { get; private set; }
        public Vector3 Inside, Mouth, Land;
        public float Apex = 4.5f, Duration = 1.2f;
        public Transform Tube;
        public Vector3 CameraFrom, CameraAt;
        // A door that swings open first (the landed Pod), and a softer hop instead of a launch.
        public Transform Door;
        public float DoorAngle = -105f;
        public bool Gentle;
        float doorT;

        enum State { Waiting, Flying, Done }
        State state;
        float wait, t, punch;
        Vector3 tubeScale;
        float flightYaw;

        public static void ResetStatics() { Busy = false; }

        void Start()
        {
            if (Tube != null) tubeScale = Tube.localScale;
            Vector3 d = Land - Mouth;
            flightYaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
        }

        void OnDisable() { Busy = false; }

        Vector3 Path(float k)
        {
            // A quick shove out of the tube (first 15%), then a lovely arc to the ground.
            if (k < 0.15f) return Vector3.Lerp(Inside, Mouth, k / 0.15f);
            float u = (k - 0.15f) / 0.85f;
            Vector3 mid = (Mouth + Land) * 0.5f + Vector3.up * Apex * 2f;
            return Vector3.Lerp(Vector3.Lerp(Mouth, mid, u), Vector3.Lerp(mid, Land, u), u);
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null || state == State.Done) return;
            var motor = game.Player.Motor;
            float dt = Time.deltaTime;

            if (Tube != null && punch > 0f)
            {
                punch = Mathf.MoveTowards(punch, 0f, dt * 2.5f);
                float w = Mathf.Sin(punch * 30f) * punch * 0.25f;
                Tube.localScale = new Vector3(tubeScale.x * (1f + w), tubeScale.y * (1f - w * 0.5f), tubeScale.z * (1f + w));
            }

            if (state == State.Waiting)
            {
                Busy = true;
                motor.ExternalControl = true;
                motor.Teleport(Inside, flightYaw);
                if (game.Mode != UIMode.Gameplay) { wait = 0f; return; }
                var cam = game.CameraRig;
                cam.ShowcasePosition = CameraFrom;
                cam.ShowcaseLookAt = CameraAt;
                cam.ShowcaseBlendSpeed = 2.5f;
                cam.Showcase = true;
                if (LoadingScreen.Busy || game.Fade > 0.2f) return;
                wait += dt;
                if (Door != null && wait > 0.2f)
                {
                    if (doorT == 0f) Sfx.Play(SfxId.Creak, 0.5f);
                    doorT = Mathf.MoveTowards(doorT, 1f, dt * 2.5f);
                    float e = 1f - (1f - doorT) * (1f - doorT);
                    Door.localRotation = Quaternion.Euler(0f, DoorAngle * e, 0f);
                }
                if (wait > (Door != null ? 0.85f : 0.6f))
                {
                    state = State.Flying;
                    t = 0f;
                    punch = 1f;
                    if (Gentle)
                    {
                        Sfx.Voice(SfxId.VoiceYay, game.Player.Forms.Current, 0.5f);
                        Sfx.Play(SfxId.Jump, 0.5f);
                    }
                    else
                    {
                        // A cheerful cork-pop and slide-whistle (no whoosh).
                        Sfx.Play(SfxId.PodPop, 0.75f);
                        FxBurst.Spawn(FxBurst.Kind.Puff, Mouth + Vector3.up * 0.7f, Color.white, (Land - Mouth).normalized);
                    }
                    Haptics.Pulse(0.3f, 0.5f, 0.15f);
                }
                return;
            }

            // Flying.
            t += dt / Duration;
            float k = Mathf.Clamp01(t);
            motor.ExternalControl = true;
            motor.Teleport(Path(k), flightYaw);
            if (k >= 1f)
            {
                state = State.Done;
                Busy = false;
                motor.ExternalControl = false;
                motor.Teleport(Land, flightYaw);
                game.Player.Visuals.PunchScale(new Vector3(1.35f, 0.65f, 1.35f));
                FxBurst.Spawn(FxBurst.Kind.Scraps, Land + Vector3.up * 0.3f, new Color(1f, 0.85f, 0.6f));
                Sfx.Play(SfxId.Land, 0.8f);
                Haptics.Pulse(0.4f, 0.3f, 0.12f);
                var cam = game.CameraRig;
                cam.ShowcaseBlendSpeed = 1.3f;
                cam.Showcase = false;
                cam.SnapBehind(flightYaw);
            }
        }
    }
}

using UnityEngine;

namespace Patchwork
{
    // The Observatory Express: a cardboard lift you step into and ride. There are really two
    // cars, one at the bottom of the tower and one at the top, built exactly alike. When you
    // ride, the doors close, the camera sits in the corner of the car, the little dial ticks
    // round, the lift music plays, and halfway through Patch is quietly moved to the other
    // car. Then: ding, and the doors on the far side open onto somewhere else entirely.
    public class CardboardLift : MonoBehaviour
    {
        // 0 = at the bottom, 1 = at the top. Every floor dial points at this.
        public static float Dial;
        public static void ResetStatics() { Dial = 0f; Riding = false; ShowOff = false; MusicPlayer.LiftHush = false; }

        void OnDestroy() { Riding = false; Dial = 0f; MusicPlayer.LiftHush = false; }
        public static bool Riding { get; private set; }
        // Set while the "lift wakes up" shot plays: the bottom doors open for the camera.
        public static bool ShowOff;

        public Vector3 LowerCenter, UpperCenter;     // the middle of each car's floor
        public Vector3 CameraOffset, LookOffset;     // the camera's corner of the car
        public Vector3 LowerDoorPoint, UpperDoorPoint;
        public LiftDoor[] LowerDoors, UpperDoors;
        public AudioSource Music;
        public int HeartsNeeded = 3;

        bool atTop, goingUp, swapped, dinged, arrived;
        float t, dialFrom, dialTo;
        int caption;
        float dialShown;

        static readonly string[] UpLines =
        {
            "Going up!",
            "Still going up...",
            "Quite a long way up, this.",
            "Nearly there. Probably.",
        };
        static readonly string[] DownLines =
        {
            "Going down!",
            "Down, down, down...",
            "Mind the fish on the way past.",
            "Almost at the bottom. Hold on to your yarn.",
        };

        const float CloseTime = 1.1f, RideTime = 8.6f, OpenTime = 1.2f;

        bool Unlocked()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Level == null) return false;
            return game.Level.HeartstringCount() >= HeartsNeeded || GameState.IsLevelComplete(Level2Builder.SceneName);
        }

        public void Ride(bool fromTop)
        {
            var game = PatchworkGame.Instance;
            if (Riding || game == null || game.Player == null || game.Mode != UIMode.Gameplay) return;
            if (!fromTop && !Unlocked()) return;
            Riding = true;
            atTop = fromTop;
            goingUp = !fromTop;
            swapped = dinged = arrived = false;
            t = 0f;
            caption = -1;
            dialFrom = fromTop ? 1f : 0f;
            dialTo = fromTop ? 0f : 1f;
            Vector3 c = fromTop ? UpperCenter : LowerCenter;
            if (YarnHorse.Ridden != null) YarnHorse.Ridden.Dismount(false);
            game.Player.Motor.Teleport(c, 0f);
            game.CutsceneSkippable = false;
            game.CutsceneCaption = null;
            game.StartScriptedCutscene(c + CameraOffset, c + LookOffset, 3.5f);
            SetDoors(fromTop ? UpperDoors : LowerDoors, false);
            Sfx.PlayPitched(SfxId.PlatformThunk, 0.35f, 0.8f);
        }

        static void SetDoors(LiftDoor[] doors, bool open)
        {
            if (doors == null) return;
            foreach (var d in doors) if (d != null) d.WantOpen = open;
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) return;
            float dt = Time.deltaTime;
            if (!Riding)
            {
                // Doors open for you as you walk up (the bottom ones only once the lift works).
                Vector3 p = game.Player.Motor.Center;
                bool nearLow = (p - LowerDoorPoint).sqrMagnitude < 30f || Inside(p, LowerCenter);
                bool nearHigh = (p - UpperDoorPoint).sqrMagnitude < 30f || Inside(p, UpperCenter);
                SetDoors(LowerDoors, (nearLow && Unlocked()) || ShowOff);
                SetDoors(UpperDoors, nearHigh);
                dialShown = Mathf.MoveTowards(dialShown, atTop ? 1f : 0f, dt * 0.5f);
                Dial = dialShown;
                MusicPlayer.LiftHush = false;
                if (Music != null && Music.isPlaying)
                {
                    Music.volume = Mathf.MoveTowards(Music.volume, 0f, dt * 0.6f);
                    if (Music.volume <= 0.001f) Music.Stop();
                }
                return;
            }

            t += dt;
            Vector3 from = atTop ? UpperCenter : LowerCenter;
            Vector3 to = atTop ? LowerCenter : UpperCenter;
            Vector3 here = swapped ? to : from;

            // The ride itself: music, the dial, a gentle wobble, and the captions.
            float ride = Mathf.Clamp01((t - CloseTime) / RideTime);
            if (t > CloseTime * 0.6f && Music != null)
            {
                if (!Music.isPlaying) { Music.volume = 0f; Music.time = 0f; Music.Play(); }
                float want = 0.55f * MusicPlayer.Volume / 10f;
                Music.volume = Mathf.MoveTowards(Music.volume, dinged ? 0f : want, dt * (dinged ? 0.5f : 0.8f));
            }
            MusicPlayer.LiftHush = !dinged;
            float eased = Mathf.SmoothStep(0f, 1f, ride);
            // The dial dawdles: it overshoots a little, has second thoughts, then gets there.
            float dither = Mathf.Sin(ride * Mathf.PI * 3f) * 0.06f * (1f - ride);
            dialShown = Mathf.Lerp(dialFrom, dialTo, eased) + (dialTo > dialFrom ? dither : -dither);
            Dial = dialShown;

            float shake = 0f;
            if (t > CloseTime && !dinged) shake = 0.018f + 0.03f * Mathf.Max(0f, 1f - Mathf.Abs(t - CloseTime - 0.2f) * 4f);
            Vector3 wob = new Vector3(Mathf.PerlinNoise(t * 9f, 0.3f) - 0.5f, Mathf.PerlinNoise(0.7f, t * 11f) - 0.5f, Mathf.PerlinNoise(t * 7f, 5.1f) - 0.5f) * shake * 2f;
            game.SetCutsceneCamera(here + CameraOffset + wob, here + LookOffset + wob * 0.5f);

            var lines = goingUp ? UpLines : DownLines;
            if (t > CloseTime)
            {
                int c = Mathf.Min(lines.Length - 1, Mathf.FloorToInt((t - CloseTime) / (RideTime / lines.Length)));
                if (c != caption && !dinged)
                {
                    caption = c;
                    game.CutsceneCaption = lines[c];
                }
            }

            // Halfway, with the camera locked in the corner, swap cars. Nobody will ever know.
            if (!swapped && t > CloseTime + RideTime * 0.5f)
            {
                swapped = true;
                Vector3 offset = game.Player.transform.position - from;
                float yaw = game.Player.transform.eulerAngles.y;
                game.Player.Motor.Teleport(to + offset, yaw);
                // The arrival car's doors are shut, of course (they were never open).
                SetDoors(atTop ? LowerDoors : UpperDoors, false);
                foreach (var d in atTop ? LowerDoors : UpperDoors) if (d != null) d.Snap();
            }

            if (!dinged && t > CloseTime + RideTime)
            {
                dinged = true;
                game.CutsceneCaption = goingUp ? "Ding! Top floor: the Observatory." : "Ding! Ground floor: the Sunken Ruins.";
                var ding = MusicPlayer.LiftDing();
                if (ding != null && Music != null) Music.PlayOneShot(ding, Mathf.Max(0.25f, MusicPlayer.Volume / 10f) * 0.9f);
                SetDoors(atTop ? LowerDoors : UpperDoors, true);
            }

            if (!arrived && t > CloseTime + RideTime + OpenTime)
            {
                arrived = true;
                Riding = false;
                atTop = !atTop;
                game.CutsceneCaption = null;
                game.EndCutscene();
            }
        }

        static bool Inside(Vector3 p, Vector3 car)
        {
            Vector3 d = p - car;
            return Mathf.Abs(d.x) < 4.2f && Mathf.Abs(d.z) < 4f && d.y > -0.5f && d.y < 4.5f;
        }
    }

    // One leaf of a sliding lift door.
    public class LiftDoor : MonoBehaviour
    {
        public Vector3 Closed, Slide;
        public bool WantOpen;
        float k;
        bool wasOpen;

        public void Snap()
        {
            k = WantOpen ? 1f : 0f;
            transform.localPosition = Closed + Slide * k;
        }

        void Update()
        {
            float want = WantOpen ? 1f : 0f;
            if (Mathf.Approximately(k, want)) return;
            k = Mathf.MoveTowards(k, want, Time.deltaTime / 0.9f);
            float e = Mathf.SmoothStep(0f, 1f, k);
            transform.localPosition = Closed + Slide * e;
            if (WantOpen != wasOpen && k > 0.02f && k < 0.98f)
            {
                wasOpen = WantOpen;
                if (Slide.z > 0f || Slide.x > 0f) Sfx.PlayNear(SfxId.PlatformThunk, transform.position, 0.12f, 12f);
            }
        }
    }

    // A floor dial's needle: swings from "ground" (left) to "top" (right).
    public class LiftDial : MonoBehaviour
    {
        public float Swing = 70f;

        void Update()
        {
            float a = Mathf.Lerp(Swing, -Swing, Mathf.Clamp01(CardboardLift.Dial));
            a += Mathf.Sin(Time.time * 23f) * (CardboardLift.Riding ? 2.5f : 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, a);
        }
    }
}

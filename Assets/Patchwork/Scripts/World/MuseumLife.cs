using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The moving parts of the Oceanic Museum: fish, kelp, jellyfish and bubbles behind the glass,
    // platforms that glide and lifts that ride, the pendulum, the plaques you can read, and the
    // light and sound changing as you walk from the warm halls down to the sea bed.

    // A fish (or shark, ray, turtle, whale) swimming round and round an oval, nose first, with
    // a little wiggle and a slow rise and fall.
    public class FishSwim : MonoBehaviour
    {
        public Vector3 Center;
        public float RadiusX = 6f, RadiusZ = 3f;
        public float Speed = 20f;        // degrees per second (negative swims the other way)
        public float Angle;
        public float Bob = 0.4f, BobSpeed = 0.6f;
        public float Wiggle = 6f, WiggleSpeed = 5f;
        public float Bank = 8f;
        public Transform Tail;
        public float TailSwing = 22f;
        float phase;

        void Start()
        {
            phase = Angle * 0.13f + Center.x * 0.07f;
            Place(0f);
        }

        void Update() { Place(Time.deltaTime); }

        void Place(float dt)
        {
            Angle += Speed * dt;
            float a = Angle * Mathf.Deg2Rad;
            float t = Time.time + phase;
            Vector3 p = Center + new Vector3(Mathf.Sin(a) * RadiusX, Mathf.Sin(t * BobSpeed) * Bob, Mathf.Cos(a) * RadiusZ);
            Vector3 fwd = new Vector3(Mathf.Cos(a) * RadiusX, Mathf.Cos(t * BobSpeed) * Bob * BobSpeed * 0.5f, -Mathf.Sin(a) * RadiusZ) * Mathf.Sign(Speed);
            transform.position = p;
            if (fwd.sqrMagnitude > 1e-6f)
                transform.rotation = Quaternion.LookRotation(fwd.normalized, Vector3.up)
                    * Quaternion.Euler(0f, Mathf.Sin(t * WiggleSpeed) * Wiggle, -Bank * Mathf.Sign(Speed));
            if (Tail != null) Tail.localRotation = Quaternion.Euler(0f, Mathf.Sin(t * WiggleSpeed - 1.2f) * TailSwing, 0f);
        }
    }

    // Kelp (or a hanging banner, or seaweed on a wreck): a chain of pieces, each leaning a
    // little further than the one below, so the whole strand sways like it's in a slow current.
    public class KelpSway : MonoBehaviour
    {
        public Transform[] Segments;
        public float Amount = 5f, Speed = 0.7f, Phase;

        void Update()
        {
            if (Segments == null) return;
            float t = Time.time * Speed + Phase;
            for (int i = 0; i < Segments.Length; i++)
            {
                if (Segments[i] == null) continue;
                float k = t - i * 0.55f;
                Segments[i].localRotation = Quaternion.Euler(Mathf.Sin(k) * Amount, 0f, Mathf.Cos(k * 0.83f + 1f) * Amount * 0.8f);
            }
        }
    }

    // A jellyfish: drifts up and down on the spot, its bell squeezing as it rises.
    public class JellyBob : MonoBehaviour
    {
        public Transform Bell;
        public float Rise = 0.8f, Speed = 0.6f, Phase, Drift = 0.3f;
        Vector3 home, bellScale;

        void Start()
        {
            home = transform.localPosition;
            if (Bell != null) bellScale = Bell.localScale;
        }

        void Update()
        {
            float t = Time.time * Speed + Phase;
            float s = Mathf.Sin(t);
            transform.localPosition = home + new Vector3(Mathf.Sin(t * 0.37f) * Drift, s * Rise, Mathf.Cos(t * 0.29f) * Drift);
            if (Bell != null)
            {
                float squeeze = Mathf.Cos(t) * 0.12f;
                Bell.localScale = new Vector3(bellScale.x * (1f - squeeze), bellScale.y * (1f + squeeze), bellScale.z * (1f - squeeze));
            }
        }
    }

    // A string of bubbles wobbling up from one spot and starting again at the bottom.
    public class BubbleStream : MonoBehaviour
    {
        public Transform[] Bubbles;
        public float Height = 8f, Speed = 1.2f, Wobble = 0.25f;

        void Update()
        {
            if (Bubbles == null) return;
            float time = Time.time;
            for (int i = 0; i < Bubbles.Length; i++)
            {
                if (Bubbles[i] == null) continue;
                float k = Mathf.Repeat(time * Speed / Height + i / (float)Bubbles.Length + i * 0.37f, 1f);
                float grow = 0.6f + k * 0.8f;
                Bubbles[i].localPosition = new Vector3(Mathf.Sin(time * 2.1f + i * 1.7f) * Wobble * k, k * Height, Mathf.Cos(time * 1.7f + i * 2.3f) * Wobble * k);
                float fade = Mathf.Clamp01(k * 8f) * Mathf.Clamp01((1f - k) * 6f);
                Bubbles[i].localScale = Vector3.one * (0.12f + (i % 3) * 0.05f) * grow * fade;
            }
        }
    }

    // A platform (or a lift) gliding smoothly back and forth between two points, resting a
    // moment at each end. Put a Carrier on the same object so Patch rides along.
    [DefaultExecutionOrder(-50)]
    public class Glider : MonoBehaviour
    {
        public Vector3 A, B;
        public float Travel = 4f, Rest = 1.5f, Offset;

        void Start() { Place(); }
        void Update() { Place(); }

        void Place()
        {
            float cycle = (Travel + Rest) * 2f;
            float t = Mathf.Repeat(Time.time + Offset, cycle);
            float k;
            if (t < Rest) k = 0f;
            else if (t < Rest + Travel) k = (t - Rest) / Travel;
            else if (t < Rest * 2f + Travel) k = 1f;
            else k = 1f - (t - Rest * 2f - Travel) / Travel;
            transform.position = Vector3.Lerp(A, B, Mathf.SmoothStep(0f, 1f, k));
        }
    }

    // The great pendulum: swings slowly from its pivot, and the plane it swings in creeps
    // round (very much faster than the real one, so you can see it happen).
    public class PendulumSwing : MonoBehaviour
    {
        public float Angle = 14f, Period = 7f, Creep = 4f;

        void Update()
        {
            float t = Time.time;
            transform.localRotation = Quaternion.Euler(0f, t * Creep, 0f) * Quaternion.Euler(Mathf.Sin(t * Mathf.PI * 2f / Period) * Angle, 0f, 0f);
        }
    }

    // Something that glows softly brighter and dimmer (a lantern fish's lure, the lenses).
    public class GlowPulse : MonoBehaviour
    {
        public Light Lamp;
        public float Low = 0.6f, High = 1.4f, Speed = 1.3f, Phase;

        void Update()
        {
            if (Lamp != null) Lamp.intensity = Mathf.Lerp(Low, High, 0.5f + 0.5f * Mathf.Sin(Time.time * Speed + Phase));
        }
    }

    // A brass plaque beside an exhibit. Stand by it and press Ability to read what it says.
    public class ExhibitPlaque : Npc
    {
        public string Title = "Exhibit";
        public string[] Text = { "..." };
        public Vector3 Head;

        public override string DisplayNameText { get { return Title; } }
        public override Color NameColor { get { return new Color(0.5f, 0.36f, 0.14f); } }
        public override Vector3 HeadPosition { get { return Head; } }
        public override string PromptVerb { get { return "Read"; } }
        public override string[] NextLines() { return Text; }
        public override void Blip() { }
    }

    // The light, the haze and the sound of each part of the museum. Each zone is a box; the one
    // Patch is standing in sets what everything eases towards, so walking from the golden
    // atrium down into the tunnels on the sea bed dims the lamps and turns the air deep blue.
    public class Level2Mood : MonoBehaviour
    {
        public class Zone
        {
            public string Name;
            public string Line;              // what the Storyteller says the first time you walk in
            public Bounds Box;
            public Color AmbientSky, AmbientSide, AmbientGround;
            public Color Fog, Backdrop, SunColor;
            public float FogStart = 40f, FogEnd = 300f, Sun = 0.5f;
            public float Depth;              // how "underwater" the music sounds (0-1)
        }

        public readonly List<Zone> Zones = new List<Zone>();
        public Zone Outside;
        public Zone Current { get; private set; }
        public System.Action<Zone> Entered;
        Color sky, side, ground, fog, back, sunColor;
        float fogStart, fogEnd, sun;
        bool started;
        readonly HashSet<string> spoken = new HashSet<string>();

        Zone Find(Vector3 p)
        {
            for (int i = 0; i < Zones.Count; i++) if (Zones[i].Box.Contains(p)) return Zones[i];
            return Outside;
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null || Outside == null) return;
            // During a camera-led moment (the telescope), follow the camera instead of Patch.
            Vector3 at = game.Mode == UIMode.Cutscene && game.CameraRig != null ? game.CameraRig.transform.position : game.Player.Motor.Center;
            var z = Find(at);
            if (z != Current)
            {
                Current = z;
                if (Entered != null) Entered(z);
                if (started && game.Mode == UIMode.Gameplay && !string.IsNullOrEmpty(z.Line) && spoken.Add(z.Name))
                {
                    string id = game.Level.Id + "_place_" + z.Name;
                    if (!GameState.IsHintSeen(id))
                    {
                        GameState.SetHintSeen(id);
                        Narrator.Say(z.Line);
                    }
                }
            }
            float k = started ? 1f - Mathf.Exp(-1.4f * Time.unscaledDeltaTime) : 1f;
            started = true;
            sky = Color.Lerp(sky, z.AmbientSky, k);
            side = Color.Lerp(side, z.AmbientSide, k);
            ground = Color.Lerp(ground, z.AmbientGround, k);
            fog = Color.Lerp(fog, z.Fog, k);
            back = Color.Lerp(back, z.Backdrop, k);
            sunColor = Color.Lerp(sunColor, z.SunColor, k);
            fogStart = Mathf.Lerp(fogStart, z.FogStart, k);
            fogEnd = Mathf.Lerp(fogEnd, z.FogEnd, k);
            sun = Mathf.Lerp(sun, z.Sun, k);

            RenderSettings.ambientSkyColor = sky;
            RenderSettings.ambientEquatorColor = side;
            RenderSettings.ambientGroundColor = ground;
            RenderSettings.fogColor = fog;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
            if (RenderSettings.sun != null)
            {
                RenderSettings.sun.intensity = sun;
                RenderSettings.sun.color = sunColor;
            }
            if (game.CameraRig != null)
            {
                var cam = game.CameraRig.GetComponent<Camera>();
                if (cam != null) cam.backgroundColor = back;
            }
            MusicPlayer.SetMuseumDepth(z.Depth);
        }
    }

    // Runs the museum: opens the Observatory lift when enough Heartstrings are found, brings
    // the music's layers in as you get further, and ends the level at the great telescope.
    public class Level2Director : MonoBehaviour
    {
        public Level2Mood Mood;
        public GameObject LiftGate;
        public Vector3 LiftSpot;
        public string ObservatoryZone = "The Observatory";
        public Vector3 TelescopeEye, TelescopeLook;
        float check;
        bool liftOpen, gazing, reachedAquarium;
        float gazeTime;

        const string AquariumKey = "Level2_reached_aquarium";

        void Start()
        {
            reachedAquarium = GameState.IsHintSeen(AquariumKey);
            if (Mood != null) Mood.Entered += OnZone;
            Refresh(true);
        }

        void OnZone(Level2Mood.Zone z)
        {
            if (z == null || reachedAquarium) return;
            if (z.Depth > 0f || z.Name == "The Kelp Galleries")
            {
                reachedAquarium = true;
                GameState.SetHintSeen(AquariumKey);
            }
        }

        void Refresh(bool quiet)
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Level == null) return;
            int hearts = game.Level.HeartstringCount();
            bool open = hearts >= game.Level.HeartstringsNeeded;
            if (open && !liftOpen)
            {
                liftOpen = true;
                if (LiftGate != null) LiftGate.SetActive(false);
                if (!quiet)
                {
                    game.ShowToast("The Observatory lift is open! Find it at the far end of the Sunken Ruins.");
                    Sfx.Play(SfxId.PrizeFanfare, 0.5f);
                }
            }
            bool inObservatory = Mood != null && Mood.Current != null && Mood.Current.Name == ObservatoryZone;
            MusicPlayer.SetMuseumLayers(hearts >= 1 || reachedAquarium, open, inObservatory || GameState.IsLevelComplete(Level2Builder.SceneName));
        }

        void Update()
        {
            check -= Time.unscaledDeltaTime;
            if (check <= 0f) { check = 0.5f; Refresh(false); }
            if (gazing) Gaze();
        }

        // Looking through the great telescope: the camera drifts up to the eyepiece and out
        // through the dome to the whale going by, and the level is done.
        public void LookThroughTelescope()
        {
            var game = PatchworkGame.Instance;
            if (game == null || gazing || game.Mode != UIMode.Gameplay) return;
            gazing = true;
            gazeTime = 0f;
            game.CutsceneSkippable = false;
            game.CutsceneCaption = "And there, far above the glass, went the oldest thing in the whole museum, singing.";
            game.StartScriptedCutscene(TelescopeEye, TelescopeLook, 1.2f);
            Sfx.Play(SfxId.PrizeFanfare, 0.5f);
        }

        void Gaze()
        {
            var game = PatchworkGame.Instance;
            if (game == null) { gazing = false; return; }
            gazeTime += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(gazeTime / 9f);
            Vector3 look = TelescopeLook + new Vector3(Mathf.Sin(k * 2.2f) * 10f, k * 4f, 0f);
            game.SetCutsceneCamera(TelescopeEye + Vector3.up * k * 1.5f, look);
            if (gazeTime > 9f)
            {
                gazing = false;
                game.CutsceneCaption = null;
                game.EndCutscene();
                game.CompleteLevel();
            }
        }
    }
}

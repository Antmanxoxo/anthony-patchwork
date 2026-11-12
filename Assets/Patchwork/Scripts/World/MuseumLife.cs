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

    // A whole sheet of stars breathing brighter and dimmer together (several sheets, each at its
    // own pace, make a sky that twinkles).
    public class StarTwinkle : MonoBehaviour
    {
        public Material Mat;
        public Color Tint = Color.white;
        public float Low = 1f, High = 2.6f, Speed = 1f, Phase;

        void Update()
        {
            if (Mat == null) return;
            float k = 0.5f + 0.5f * Mathf.Sin(Time.time * Speed + Phase);
            Mat.SetColor("_EmissionColor", Tint * Mathf.Lerp(Low, High, k * k));
        }
    }

    // The constellations on the atrium's ceiling take turns to blaze: one lights up, holds,
    // and fades as the next begins.
    public class ConstellationShow : MonoBehaviour
    {
        public Material[] Figures;
        public Color[] Tints;
        public float Dim = 0.45f, Bright = 3.4f, Each = 6f;
        float[] level;

        void Update()
        {
            if (Figures == null || Figures.Length == 0) return;
            if (level == null || level.Length != Figures.Length) level = new float[Figures.Length];
            float t = Time.time / Each;
            int now = Mathf.FloorToInt(t) % Figures.Length;
            float within = t - Mathf.Floor(t);
            for (int i = 0; i < Figures.Length; i++)
            {
                float want = i == now ? Mathf.Sin(within * Mathf.PI) : 0f;
                level[i] = Mathf.MoveTowards(level[i], want, Time.deltaTime * 1.5f);
                if (Figures[i] == null) continue;
                float shimmer = 1f + 0.12f * Mathf.Sin(Time.time * 7f + i);
                Figures[i].SetColor("_EmissionColor", Tints[i % Tints.Length] * Mathf.Lerp(Dim, Bright, level[i]) * shimmer);
            }
        }
    }

    // A shooting star: every so often it streaks across from one side to the other, a little
    // differently each time, and is gone.
    public class ShootingStar : MonoBehaviour
    {
        public Vector3 From, To, Scatter;
        public float Every = 8f, Take = 0.8f, Offset, Length = 5f, Thickness = 0.14f;

        void Update()
        {
            float clock = Time.time + Offset;
            float t = Mathf.Repeat(clock, Every);
            if (t > Take)
            {
                transform.localScale = Vector3.zero;
                return;
            }
            int turn = Mathf.FloorToInt(clock / Every);
            float ra = Mathf.Repeat(Mathf.Sin(turn * 12.9898f) * 43758.5f, 1f) - 0.5f;
            float rb = Mathf.Repeat(Mathf.Sin(turn * 78.233f) * 12543.1f, 1f) - 0.5f;
            Vector3 shift = new Vector3(Scatter.x * ra, Scatter.y * rb, Scatter.z * rb);
            Vector3 a = From + shift, b = To + new Vector3(Scatter.x * rb, Scatter.y * ra, Scatter.z * ra);
            float k = t / Take;
            Vector3 dir = (b - a).normalized;
            transform.position = Vector3.Lerp(a, b, k * k * (3f - 2f * k));
            transform.rotation = Quaternion.LookRotation(dir, Mathf.Abs(dir.y) > 0.95f ? Vector3.forward : Vector3.up);
            float fade = Mathf.Sin(k * Mathf.PI);
            transform.localScale = new Vector3(Thickness * fade, Thickness * fade, Length * (0.3f + fade));
        }
    }

    // Motes of light drifting through a box of air (or water), wrapping round when they leave
    // it: star dust in the atrium, fireflies on the quay, marine snow outside the tunnel.
    public class DriftMotes : MonoBehaviour
    {
        public Transform[] Motes;
        public Vector3 Size = new Vector3(20f, 10f, 20f);
        public Vector3 Drift = new Vector3(0.1f, -0.25f, 0.05f);
        public float Wander = 0.6f, MoteSize = 0.12f, Flicker = 2f;

        void Update()
        {
            if (Motes == null) return;
            float time = Time.time;
            for (int i = 0; i < Motes.Length; i++)
            {
                if (Motes[i] == null) continue;
                float sx = Mathf.Repeat(i * 0.6180339f, 1f), sy = Mathf.Repeat(i * 0.3819660f + 0.37f, 1f), sz = Mathf.Repeat(i * 0.7548777f + 0.11f, 1f);
                float speed = 0.6f + sx * 0.8f;
                Vector3 p = new Vector3(
                    Mathf.Repeat(sx * Size.x + Drift.x * time * speed, Size.x) - Size.x * 0.5f,
                    Mathf.Repeat(sy * Size.y + Drift.y * time * speed, Size.y) - Size.y * 0.5f,
                    Mathf.Repeat(sz * Size.z + Drift.z * time * speed, Size.z) - Size.z * 0.5f);
                p.x += Mathf.Sin(time * 0.5f + i * 1.3f) * Wander;
                p.z += Mathf.Cos(time * 0.4f + i * 2.1f) * Wander;
                Motes[i].localPosition = p;
                // Fade in and out near the top and bottom of the box, and glimmer.
                float edge = Mathf.Clamp01((Size.y * 0.5f - Mathf.Abs(p.y)) / (Size.y * 0.15f));
                float glim = 0.55f + 0.45f * Mathf.Sin(time * Flicker * (0.6f + sy) + i * 4.7f);
                Motes[i].localScale = Vector3.one * MoteSize * (0.6f + sz * 0.8f) * edge * glim;
            }
        }
    }

    // Paper stars (or planets, or lamps) hung on threads: each turns slowly and bobs a little.
    public class StarMobile : MonoBehaviour
    {
        public Transform[] Stars;
        public float Turn = 18f, Bob = 0.14f, Tilt = 7f;
        Vector3[] home;

        void Start()
        {
            if (Stars == null) return;
            home = new Vector3[Stars.Length];
            for (int i = 0; i < Stars.Length; i++) if (Stars[i] != null) home[i] = Stars[i].localPosition;
        }

        void Update()
        {
            if (Stars == null || home == null) return;
            float t = Time.time;
            for (int i = 0; i < Stars.Length; i++)
            {
                if (Stars[i] == null) continue;
                Stars[i].localPosition = home[i] + Vector3.up * Mathf.Sin(t * 0.6f + i * 1.7f) * Bob;
                Stars[i].localRotation = Quaternion.Euler(Mathf.Sin(t * 0.5f + i) * Tilt, t * Turn * (0.6f + (i % 4) * 0.25f) * (i % 2 == 0 ? 1f : -1f) + i * 47f, 0f);
            }
        }
    }

    // Something afloat: bobs and rocks gently on the swell.
    public class SeaBob : MonoBehaviour
    {
        public float Height = 0.12f, Rock = 3f, Speed = 0.8f, Phase;
        Vector3 home;
        Quaternion homeRot;

        void Start()
        {
            home = transform.localPosition;
            homeRot = transform.localRotation;
        }

        void Update()
        {
            float t = Time.time * Speed + Phase;
            transform.localPosition = home + Vector3.up * Mathf.Sin(t) * Height;
            transform.localRotation = homeRot * Quaternion.Euler(Mathf.Sin(t * 0.8f + 1f) * Rock, 0f, Mathf.Cos(t * 0.65f) * Rock);
        }
    }

    // A suit of armour's helmet (or a statue's head) that turns, slowly, to follow Patch when
    // he comes close, and turns back to the front when he's gone.
    public class WatchPlayer : MonoBehaviour
    {
        public float Range = 11f, MaxTurn = 55f, Speed = 1.6f;
        Quaternion home;
        float yaw;

        void Start() { home = transform.localRotation; }

        void Update()
        {
            var game = PatchworkGame.Instance;
            float want = 0f;
            if (game != null && game.Player != null)
            {
                Vector3 to = game.Player.Motor.Center - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude < Range * Range && to.sqrMagnitude > 0.5f)
                {
                    Vector3 fwd = (transform.parent != null ? transform.parent.rotation : Quaternion.identity) * home * Vector3.forward;
                    fwd.y = 0f;
                    want = Mathf.Clamp(Vector3.SignedAngle(fwd, to, Vector3.up), -MaxTurn, MaxTurn);
                }
            }
            yaw = Mathf.Lerp(yaw, want, 1f - Mathf.Exp(-Speed * Time.deltaTime));
            transform.localRotation = home * Quaternion.Euler(0f, yaw, 0f);
        }
    }

    // A seal (or a fish) that every so often leaps out of the water in an arc and dives back
    // in, with a splash at each end. Between leaps it's out of sight under the surface.
    public class Leaper : MonoBehaviour
    {
        public Vector3 From, To;
        public float Height = 2f, Every = 7f, Take = 1.3f, Offset, Splash = 0.3f;
        bool wasOut;

        void Update()
        {
            float t = Mathf.Repeat(Time.time + Offset, Every);
            bool isOut = t < Take;
            if (!isOut)
            {
                if (wasOut) Splosh(To);
                wasOut = false;
                transform.localScale = Vector3.zero;
                return;
            }
            if (!wasOut) Splosh(From);
            wasOut = true;
            float k = t / Take;
            Vector3 p = Vector3.Lerp(From, To, k) + Vector3.up * Height * 4f * k * (1f - k);
            Vector3 v = (To - From) + Vector3.up * Height * 4f * (1f - 2f * k);
            transform.position = p;
            if (v.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.LookRotation(v.normalized, Vector3.up);
            transform.localScale = Vector3.one;
        }

        void Splosh(Vector3 at)
        {
            if (Splash <= 0f) return;
            FxBurst.Spawn(FxBurst.Kind.Puff, at + Vector3.up * 0.2f, new Color(0.85f, 0.97f, 1f));
            Sfx.PlayNear(SfxId.Splash, at, Splash, 22f);
        }
    }

    // The horses (well, the sea creatures) of a carousel, rising and falling on their poles,
    // each a little out of step with the next.
    public class CarouselBob : MonoBehaviour
    {
        public Transform[] Mounts;
        public float Height = 0.45f, Speed = 1.7f;
        Vector3[] home;

        void Start()
        {
            if (Mounts == null) return;
            home = new Vector3[Mounts.Length];
            for (int i = 0; i < Mounts.Length; i++) if (Mounts[i] != null) home[i] = Mounts[i].localPosition;
        }

        void Update()
        {
            if (Mounts == null || home == null) return;
            float t = Time.time * Speed;
            for (int i = 0; i < Mounts.Length; i++)
                if (Mounts[i] != null) Mounts[i].localPosition = home[i] + Vector3.up * Mathf.Sin(t + i * 2.1f) * Height;
        }
    }

    // A bird's wings beating, with a long glide now and then.
    public class WingFlap : MonoBehaviour
    {
        public Transform Left, Right;
        public float Angle = 30f, Speed = 7f, Phase;

        void Update()
        {
            float t = Time.time + Phase;
            // Flap for a few seconds, then hold the wings out and glide.
            float gliding = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(Mathf.Sin(t * 0.45f) * 3f));
            float a = Mathf.Sin(t * Speed) * Angle * (1f - gliding) + 4f * gliding;
            if (Left != null) Left.localRotation = Quaternion.Euler(0f, 0f, a);
            if (Right != null) Right.localRotation = Quaternion.Euler(0f, 0f, -a);
        }
    }

    // Someone having a quiet nap. Stand by them and press Ability to give them a nudge.
    public class SleepyNpc : ExhibitPlaque
    {
        public override string PromptVerb { get { return "Nudge"; } }
        public override Color NameColor { get { return new Color(0.3f, 0.35f, 0.6f); } }
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
            public Color UnderFog;           // the colour of the water when you dive here (none: the usual blue-green)
            public float UnderFogEnd;        // how far you can see under it (0: the usual)
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
            // Under the water (Water Form diving, with the camera gone down after him): a deep
            // blue-green, the far side lost in the murk, the music far away.
            bool camUnder = false;
            if (game.CameraRig != null)
            {
                Vector3 cp = game.CameraRig.transform.position;
                foreach (var v in PlayerVolume.All)
                {
                    var wb = v as WaterBody;
                    if (wb != null && wb.Divable && cp.y < wb.SurfaceY && wb.Contains(cp)) { camUnder = true; break; }
                }
            }
            under = Mathf.MoveTowards(under, camUnder ? 1f : 0f, Time.unscaledDeltaTime * 2.5f);
            if (under > 0.001f)
            {
                float u = Mathf.SmoothStep(0f, 1f, under);
                bool own = z.UnderFog.a > 0.01f;
                RenderSettings.fogColor = Color.Lerp(fog, own ? z.UnderFog : new Color(0.05f, 0.26f, 0.3f), u);
                RenderSettings.fogStartDistance = Mathf.Lerp(fogStart, 0.5f, u);
                RenderSettings.fogEndDistance = Mathf.Lerp(fogEnd, z.UnderFogEnd > 0f ? z.UnderFogEnd : 34f, u);
                RenderSettings.ambientSkyColor = Color.Lerp(sky, new Color(0.3f, 0.6f, 0.65f), u * 0.7f);
                RenderSettings.ambientEquatorColor = Color.Lerp(side, new Color(0.18f, 0.42f, 0.48f), u * 0.7f);
            }
            if (under != lastUnder) LevelLook.SetUnderwater(under);
            lastUnder = under;
            MusicPlayer.SetMuseumDepth(Mathf.Max(z.Depth, under * 0.85f));
        }

        float under, lastUnder = -1f;
    }

    // Runs the museum: opens the Observatory lift when enough Heartstrings are found, brings
    // the music's layers in as you get further, and ends the level at the great telescope.
    public class Level2Director : MonoBehaviour
    {
        public Level2Mood Mood;
        public GameObject LiftGate;
        public CinematicShot LiftShot;
        bool gateFalling;
        Vector3 gateHome;
        Quaternion gateRot;
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
                if (LiftGate != null && (quiet || LiftShot == null)) LiftGate.SetActive(false);
                else if (LiftGate != null) WakeTheLift();
                if (!quiet)
                {
                    game.ShowToast("The Observatory lift is open! Find it at the far end of the Sunken Ruins.");
                    Sfx.Play(SfxId.PrizeFanfare, 0.5f);
                }
            }
            bool inObservatory = Mood != null && Mood.Current != null && Mood.Current.Name == ObservatoryZone;
            MusicPlayer.SetMuseumLayers(true, hearts >= 1 || reachedAquarium, open || inObservatory || GameState.IsLevelComplete(Level2Builder.SceneName));
        }

        // The third Heartstring: a "meanwhile" shot of the lift, the "out of order" sign
        // dropping off, the lamps coming up and the doors sliding open with a ding.
        void WakeTheLift()
        {
            gateHome = LiftGate.transform.position;
            gateRot = LiftGate.transform.rotation;
            bool dinged = false;
            LiftShot.Tick = k =>
            {
                float fall = Mathf.Clamp01((k - 0.3f) / 0.22f);
                if (LiftGate != null && LiftGate.activeSelf)
                {
                    float drop = fall * fall;
                    LiftGate.transform.position = gateHome + new Vector3(0.5f * fall, -1.7f * drop, 0f);
                    LiftGate.transform.rotation = gateRot * Quaternion.Euler(fall * 25f, 0f, -fall * 75f);
                    if (fall >= 1f)
                    {
                        LiftGate.SetActive(false);
                        Sfx.PlayNear(SfxId.PlatformThunk, gateHome, 0.5f, 40f);
                    }
                }
                CardboardLift.ShowOff = k > 0.62f && k < 0.97f;
                if (!dinged && k > 0.66f)
                {
                    dinged = true;
                    var ding = MusicPlayer.LiftDing();
                    if (ding != null) AudioSource.PlayClipAtPoint(ding, PatchworkGame.Instance != null && PatchworkGame.Instance.CameraRig != null ? PatchworkGame.Instance.CameraRig.transform.position : gateHome, Mathf.Max(0.3f, MusicPlayer.Volume / 10f));
                }
            };
            LiftShot.Done = () =>
            {
                CardboardLift.ShowOff = false;
                if (LiftGate != null) LiftGate.SetActive(false);
            };
            LiftShot.Trigger();
            // If the shot has been seen before (an old save), just take the sign down.
            if (GameState.IsHintSeen(LiftShot.Key)) LiftGate.SetActive(false);
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

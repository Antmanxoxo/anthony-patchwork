using UnityEngine;

namespace Patchwork
{
    // A short camera move the first time you walk somewhere special: the bars slide in, the
    // camera glides along a few points (a smooth curve through them), captions come and go,
    // and it hands the camera back. Each one plays once (remembered in the save). Hold
    // Confirm to skip.
    // Shots with a zero Size never trigger by walking in: something calls Trigger() instead
    // (like the lift waking up).
    public class CinematicShot : PlayerVolume
    {
        public string Key;
        public Vector3[] Cams, Looks;
        public float Duration = 8f, Blend = 1.4f;
        public string[] Captions;
        // Called every frame while it plays, with how far through it is (0..1).
        public System.Action<float> Tick;
        public System.Action Done;

        static CinematicShot current;
        public static bool Playing { get { return current != null; } }
        public static void ResetStatics() { current = null; }

        bool pending, playing;
        float t, skipHold, wait;

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (!string.IsNullOrEmpty(Key) && !GameState.IsHintSeen(Key)) pending = true;
        }

        public void Trigger()
        {
            if (!string.IsNullOrEmpty(Key) && GameState.IsHintSeen(Key)) return;
            pending = true;
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) return;
            if (playing)
            {
                Play(game);
                return;
            }
            if (!pending || current != null) return;
            bool ready = game.Mode == UIMode.Gameplay && !LoadingScreen.Busy && !PodLaunch.Busy && !CardboardLift.Riding && game.Fade < 0.02f;
            wait = ready ? wait + Time.unscaledDeltaTime : 0f;
            if (wait < 0.35f) return;
            pending = false;
            playing = true;
            current = this;
            t = 0f;
            skipHold = 0f;
            if (!string.IsNullOrEmpty(Key)) GameState.SetHintSeen(Key);
            game.CutsceneSkippable = true;
            game.CutsceneCaption = Captions != null && Captions.Length > 0 ? Captions[0] : null;
            game.StartScriptedCutscene(Cams[0], Looks[0], Blend);
            if (Tick != null) Tick(0f);
        }

        void Play(PatchworkGame game)
        {
            // Something else took the camera away (a menu, the level ending): just stop.
            if (game.Mode != UIMode.Cutscene && game.Mode != UIMode.Paused)
            {
                playing = false;
                current = null;
                return;
            }
            if (game.Mode == UIMode.Paused) return;
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / Duration);
            float e = k * k * (3f - 2f * k);
            game.SetCutsceneCamera(Spline(Cams, e), Spline(Looks, e));
            if (Captions != null && Captions.Length > 0)
            {
                int c = Mathf.Min(Captions.Length - 1, Mathf.FloorToInt(k * Captions.Length));
                game.CutsceneCaption = Captions[c];
            }
            if (Tick != null) Tick(k);
            bool held = t > 0.5f && (game.Input.Confirm.IsPressed() || game.Input.Pause.IsPressed());
            skipHold = held ? skipHold + Time.unscaledDeltaTime : Mathf.Max(0f, skipHold - Time.unscaledDeltaTime * 2f);
            game.CutsceneSkipFill = skipHold / 0.9f;
            if (k >= 1f || skipHold >= 0.9f)
            {
                if (Tick != null) Tick(1f);
                playing = false;
                current = null;
                game.CutsceneCaption = null;
                game.CutsceneSkipFill = 0f;
                game.EndCutscene();
                if (Done != null) Done();
            }
        }

        // A smooth curve through the points (Catmull-Rom), from the first at 0 to the last at 1.
        static Vector3 Spline(Vector3[] p, float u)
        {
            if (p == null || p.Length == 0) return Vector3.zero;
            if (p.Length == 1) return p[0];
            float f = u * (p.Length - 1);
            int i = Mathf.Min(p.Length - 2, Mathf.FloorToInt(f));
            float s = f - i;
            Vector3 p0 = p[Mathf.Max(0, i - 1)], p1 = p[i], p2 = p[i + 1], p3 = p[Mathf.Min(p.Length - 1, i + 2)];
            return 0.5f * ((2f * p1) + (-p0 + p2) * s + (2f * p0 - 5f * p1 + 4f * p2 - p3) * s * s + (-p0 + 3f * p1 - 3f * p2 + p3) * s * s * s);
        }

        void OnDestroy() { if (current == this) current = null; }
    }

    // A spot light whose cookie is moving water: the dancing net of light you see on the
    // floor of a pool, or on the walls next to a big aquarium window.
    public class CausticLight : MonoBehaviour
    {
        public Light Lamp;
        public float Fps = 11f, Turn = 2.5f, Phase;
        static Texture2D[] frames;
        const int Size = 96, Frames = 12;

        public static void ResetStatics() { frames = null; }

        void Start()
        {
            if (Lamp == null) Lamp = GetComponent<Light>();
            if (frames == null) frames = Make();
        }

        void Update()
        {
            if (Lamp == null || frames == null) return;
            int f = Mathf.FloorToInt((Time.time + Phase) * Fps) % Frames;
            Lamp.cookie = frames[f];
            transform.Rotate(Vector3.forward, Turn * Time.deltaTime, Space.Self);
        }

        // A looping set of caustic patterns: bright threads where the cells of a slowly
        // wandering pattern meet (it tiles, and the last frame flows back into the first).
        static Texture2D[] Make()
        {
            const int pts = 14;
            var rng = new System.Random(77);
            var baseP = new Vector2[pts];
            var orbit = new Vector2[pts];
            var speed = new int[pts];
            for (int i = 0; i < pts; i++)
            {
                baseP[i] = new Vector2((float)rng.NextDouble(), (float)rng.NextDouble());
                orbit[i] = new Vector2(0.04f + (float)rng.NextDouble() * 0.05f, 0.04f + (float)rng.NextDouble() * 0.05f);
                speed[i] = rng.Next(0, 2) == 0 ? 1 : -1;
            }
            var result = new Texture2D[Frames];
            var px = new Color[Size * Size];
            var cur = new Vector2[pts];
            for (int f = 0; f < Frames; f++)
            {
                float a = f / (float)Frames * Mathf.PI * 2f;
                for (int i = 0; i < pts; i++)
                    cur[i] = baseP[i] + new Vector2(Mathf.Cos(a * speed[i] + i) * orbit[i].x, Mathf.Sin(a * speed[i] + i * 1.7f) * orbit[i].y);
                for (int y = 0; y < Size; y++)
                    for (int x = 0; x < Size; x++)
                    {
                        Vector2 p = new Vector2((x + 0.5f) / Size, (y + 0.5f) / Size);
                        float d1 = 9f, d2 = 9f;
                        for (int i = 0; i < pts; i++)
                            for (int oy = -1; oy <= 1; oy++)
                                for (int ox = -1; ox <= 1; ox++)
                                {
                                    float dx = cur[i].x + ox - p.x, dy = cur[i].y + oy - p.y;
                                    float d = dx * dx + dy * dy;
                                    if (d < d1) { d2 = d1; d1 = d; }
                                    else if (d < d2) d2 = d;
                                }
                        float edge = Mathf.Sqrt(d2) - Mathf.Sqrt(d1);
                        float v = Mathf.Pow(Mathf.Clamp01(1f - edge * 9f), 3f);
                        v = 0.22f + v * 0.95f;
                        // Fade to nothing towards the rim, so the pool of light has a soft edge.
                        float r = Vector2.Distance(p, new Vector2(0.5f, 0.5f)) * 2f;
                        v *= Mathf.Clamp01((1f - r) * 3f);
                        px[y * Size + x] = new Color(v, v, v, v);
                    }
                var t = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
                t.wrapMode = TextureWrapMode.Clamp;
                t.filterMode = FilterMode.Bilinear;
                t.SetPixels(px);
                t.Apply(false);
                result[f] = t;
            }
            return result;
        }
    }
}

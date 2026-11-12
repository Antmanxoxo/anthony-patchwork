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

    // A spot light whose cookie is moving water: the net of light you see on the floor of a
    // pool, or on the walls next to a big aquarium window. One pattern, turning and swaying
    // slowly and smoothly (swapping between patterns made everything it lit flicker).
    public class CausticLight : MonoBehaviour
    {
        public Light Lamp;
        public float Turn = 3f, Phase;
        Quaternion home;
        float baseIntensity = -1f;
        static Texture2D[] frames;
        const int Size = 96, Frames = 12;

        public static void ResetStatics() { frames = null; }

        void Start()
        {
            if (Lamp == null) Lamp = GetComponent<Light>();
            if (frames == null) frames = Make();
            if (Lamp != null && frames != null) Lamp.cookie = frames[Mathf.Abs(Mathf.RoundToInt(Phase * 7f)) % Frames];
            home = transform.localRotation;
            if (Lamp != null) baseIntensity = Lamp.intensity;
        }

        void Update()
        {
            if (Lamp == null) return;
            float t = Time.time + Phase * 10f;
            // A light that throws shadows stays perfectly still: if it moved, every shadow it
            // casts would crawl and shimmer. The others turn and sway slowly, like light
            // through a moving surface.
            if (Lamp.shadows == LightShadows.None)
                transform.localRotation = home * Quaternion.Euler(Mathf.Sin(t * 0.37f) * 1.6f, Mathf.Cos(t * 0.29f) * 1.6f, t * Turn);
            if (baseIntensity > 0f) Lamp.intensity = baseIntensity * (0.96f + Mathf.Sin(t * 0.5f) * 0.04f);
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

    // A soft beam of light (sunlight through water, moonlight through a skylight): a glowing
    // strip with feathered edges that fades out towards the bottom, always turned to face
    // the camera round its own length, so from any side it reads as a shaft of light and
    // never as a flat sheet. It adds light rather than covering what's behind it, so beams
    // crossing each other can't flicker.
    public class LightBeam : MonoBehaviour
    {
        public Vector3 Axis = Vector3.down;   // from the top of the beam to the bottom
        // If set, the beam swings round with whatever it's fixed to (like a lighthouse's lamp),
        // and Axis is in that thing's own terms.
        public bool AxisFollowsParent;
        public float Shimmer = 0.12f, Phase;
        Renderer rend;
        MaterialPropertyBlock mpb;
        Color baseCol;
        static Texture2D tex;
        static readonly System.Collections.Generic.Dictionary<string, Material> mats = new System.Collections.Generic.Dictionary<string, Material>();
        static Camera cam;

        public static void ResetStatics() { tex = null; mats.Clear(); cam = null; }

        // Makes a beam from top to bottom, topWidth wide where it starts (it spreads a little).
        public static LightBeam Make(Transform parent, Vector3 top, Vector3 bottom, float width, Color c, float strength)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = "LightBeam";
            go.layer = 2;
            go.transform.SetParent(parent, false);
            go.transform.position = (top + bottom) * 0.5f;
            float len = Vector3.Distance(top, bottom);
            go.transform.localScale = new Vector3(width, len, 1f);
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = Mat(c, strength);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.sortingOrder = 20;   // adding light: drawn after everything else see-through
            var b = go.AddComponent<LightBeam>();
            b.Axis = (bottom - top).normalized;
            b.Phase = top.x * 0.37f + top.z * 0.21f;
            return b;
        }

        static Material Mat(Color c, float strength)
        {
            string key = ColorUtility.ToHtmlStringRGB(c) + strength.ToString("0.00");
            Material m;
            if (mats.TryGetValue(key, out m) && m != null) return m;
            if (tex == null) tex = MakeTexture();
            m = PatchworkAssets.Unique(PatchworkAssets.Bubble(new Color(1f, 1f, 1f, 1f)));
            m.name = "PW_beam" + key;
            m.SetTexture("_BaseMap", tex);
            m.SetColor("_BaseColor", new Color(0f, 0f, 0f, 1f));
            m.SetColor("_EmissionColor", c * strength);
            m.SetTexture("_EmissionMap", tex);
            m.EnableKeyword("_EMISSION");
            m.SetFloat("_Smoothness", 0f);
            m.SetFloat("_SpecularHighlights", 0f);
            m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            m.SetFloat("_EnvironmentReflections", 0f);
            m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            m.SetFloat("_Cull", 0f);
            // Additive: light added on top, nothing darkened, no sorting to get wrong.
            m.SetFloat("_Blend", 2f);
            m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            m.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.Zero);
            m.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
            m.SetFloat("_ZWrite", 0f);
            m.renderQueue = 3100;
            mats[key] = m;
            return m;
        }

        // Soft across (a gentle hump with feathered edges), narrower at the top, fading in at
        // the top and away to nothing towards the bottom, with a few faint streaks.
        static Texture2D MakeTexture()
        {
            const int w = 64, h = 128;
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            t.wrapMode = TextureWrapMode.Clamp;
            t.filterMode = FilterMode.Bilinear;
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                float v = (y + 0.5f) / h;   // 0 bottom, 1 top
                float halfW = Mathf.Lerp(0.5f, 0.28f, v);
                float along = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - v) / 0.08f)) * Mathf.Pow(Mathf.Clamp01(v / 0.9f), 0.8f);
                for (int x = 0; x < w; x++)
                {
                    float u = (x + 0.5f) / w - 0.5f;
                    float across = Mathf.Clamp01(1f - Mathf.Abs(u) / halfW);
                    across = across * across * (3f - 2f * across);
                    float streak = 0.82f + 0.18f * Mathf.Sin(u * 41f + Mathf.Sin(u * 13f) * 2f);
                    float a = across * along * streak;
                    px[y * w + x] = new Color(a, a, a, a);
                }
            }
            t.SetPixels(px);
            t.Apply(false);
            return t;
        }

        void Start()
        {
            rend = GetComponent<Renderer>();
            mpb = new MaterialPropertyBlock();
            if (rend != null && rend.sharedMaterial != null) baseCol = rend.sharedMaterial.GetColor("_EmissionColor");
        }

        void LateUpdate()
        {
            if (cam == null) cam = Camera.main != null ? Camera.main : Object.FindAnyObjectByType<Camera>();
            if (cam == null) return;
            Vector3 axis = AxisFollowsParent && transform.parent != null ? transform.parent.TransformDirection(Axis).normalized : Axis;
            Vector3 toCam = cam.transform.position - transform.position;
            Vector3 flat = toCam - Vector3.Dot(toCam, axis) * axis;
            if (flat.sqrMagnitude < 1e-4f) return;
            // The quad's face looks down its -Z: point +Z away from the camera, +Y up the beam.
            transform.rotation = Quaternion.LookRotation(-flat.normalized, -axis);
            if (rend != null && Shimmer > 0f)
            {
                float k = 1f + Mathf.Sin(Time.time * 0.6f + Phase) * Shimmer + Mathf.Sin(Time.time * 1.7f + Phase * 2f) * Shimmer * 0.4f;
                mpb.SetColor("_EmissionColor", baseCol * k);
                rend.SetPropertyBlock(mpb);
            }
        }
    }
}

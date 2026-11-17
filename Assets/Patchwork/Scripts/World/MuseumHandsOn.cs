using UnityEngine;

namespace Patchwork
{
    // The museum's hands-on exhibits: five machines you're allowed (encouraged, even) to
    // touch. Each one does something when used, and tells you a true thing while it does it.
    //   The Whale-Song Gramophone  (Grand Atrium)     - crank it: a humpback's song fills the hall
    //   The Archimedes Screw       (Hall of Deep Time)- turn the handle: water climbs a spiral
    //   The Lighthouse Lens        (Kelp Galleries)   - wind it up: a beam sweeps the room
    //   The Ship's Bell            (Sunken Ruins)     - pull the rope: eight bells
    //   The Message Tube           (Keepers' Room)    - post a capsule: whoosh, across the ceiling
    // Each also stamps the Museum Passport (Ticket-master Tock's idea). All five is a secret.
    public static partial class Level2Builder
    {
        static void HandsOn(LevelKit kit)
        {
            var root = kit.Group("HandsOn", Vector3.zero);
            var passport = root.gameObject.AddComponent<MuseumPassport>();
            passport.Stations = new HandsOnExhibit[MuseumPassport.Stamps];

            Station(kit, passport, 0, new Vector3(6f, 0f, -70f), "The Whale-Song Gramophone", "Crank the gramophone",
                "Male humpback whales sing songs that can last for half an hour, and they sing them over and over, sometimes all day.",
                "Every male in the same stretch of ocean sings the same song, and the song slowly changes as the season goes on: they all change it together.");
            Station(kit, passport, 1, new Vector3(-60f, 0f, -40f), "The Archimedes Screw", "Turn the handle",
                "Wind a spiral round a pole, tilt it with its foot in the water, and turn it: the water climbs the spiral, a scoop at a time, all the way to the top.",
                "It is named after Archimedes, who lived in Syracuse over two thousand years ago. Screws just like it still pump water today.");
            Station(kit, passport, 2, new Vector3(-70f, 0f, 68f), "The Lighthouse Lens", "Wind the lens",
                "In 1822 a French engineer, Augustin Fresnel, built a lens out of rings of glass prisms instead of one great lump of glass.",
                "It was far thinner and lighter, and it gathered the lamp's light into a beam that could be seen for over twenty miles out at sea.");
            Station(kit, passport, 3, new Vector3(80f, 0f, 22f), "The Ship's Bell", "Ring the bell",
                "On a sailing ship the bell was struck every half hour, one more stroke each time, to keep count of the watch.",
                "A watch lasted four hours. Eight bells meant it was over, and time for the next lot of sailors to go on deck.");
            Station(kit, passport, 4, new Vector3(-66f, 0f, 88f), "The Message Tube", "Post a message",
                "In 1853 a pneumatic tube was laid under the streets of London: a pipe that blew small cases of messages along it with air.",
                "Soon cities all over the world had them. A letter could cross Paris in less than an hour without ever seeing daylight.");
        }

        static void Station(LevelKit kit, MuseumPassport passport, int which, Vector3 want, string title, string verb, string factA, string factB)
        {
            Vector3 foot;
            // (Room for the machine, its card beside it, and somewhere to stand in front.)
            Vector3 room = which == 2 ? new Vector3(5f, 5f, 5f) : new Vector3(4.4f, 3.6f, 4.4f);
            if (!RmFloor(title, want, room, 0f, 18f, out foot)) foot = want;
            // Back to the nearest wall, front to the room.
            float yaw = FaceTheRoom(foot, 0f);
            var g = kit.Group("HandsOn_" + which, foot);
            g.rotation = Rot(yaw);
            var ex = g.gameObject.AddComponent<HandsOnExhibit>();
            ex.Kind = which;
            ex.Passport = passport;
            ex.Title = title;
            ex.Fact = factA + " " + factB;
            ex.Build(kit);
            TownKit.Solid(kit, foot + Vector3.up * 0.8f, which == 2 ? new Vector3(1.8f, 1.6f, 1.8f) : new Vector3(1.3f, 1.6f, 1.3f), yaw);
            passport.Stations[which] = ex;

            var spot = kit.Volume<UseSpot>(foot + Rot(yaw) * new Vector3(0f, 1.2f, 1.6f), new Vector3(2.6f, 2.6f, 2.2f), "Use_" + title);
            spot.Label = verb;
            spot.PromptPoint = foot + Vector3.up * 2.4f;
            spot.Used = p => ex.Use();
            Plaque(kit, foot + Rot(yaw) * new Vector3(1.6f, 0f, 0.9f), yaw - 20f, title, factA, factB);
        }
    }

    // Counts the stamps, and hands out the reward.
    public class MuseumPassport : MonoBehaviour
    {
        public static MuseumPassport Instance { get; private set; }
        public static void ResetStatics() { Instance = null; }
        public const int Stamps = 5;
        public HandsOnExhibit[] Stations;

        static string Key(int i) { return "L2_q_stamp" + i; }
        public bool Has(int i) { return GameState.IsHintSeen(Key(i)); }
        public int Count { get { int n = 0; for (int i = 0; i < Stamps; i++) if (Has(i)) n++; return n; } }

        void Awake() { Instance = this; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Stamp(int which, string title)
        {
            if (Has(which)) return;
            GameState.SetHintSeen(Key(which));
            int n = Count;
            PatchworkHUD.ShowGet("Passport Stamp: " + title, n + " of " + Stamps + " hands-on exhibits", new Color(0.85f, 0.4f, 0.35f), 0);
            Sfx.Play(SfxId.StickerSlap, 0.5f);
            if (n >= Stamps && Level2Quest.Instance != null) Level2Quest.Instance.Secret("passport");
        }
    }

    // One hands-on exhibit. Each does its own thing for a few seconds after it's used.
    public class HandsOnExhibit : MonoBehaviour
    {
        public int Kind;
        public MuseumPassport Passport;
        public string Title, Fact;

        float run = -1f, t;
        int strokes;
        Transform crank, disc, horn, screw, lens, bell, clapper, capsule;
        Transform[] drops, rings;
        Light beam;
        Transform beamCone;

        static Material Mat(Color c) { return PatchworkAssets.Plain(c, 0.6f); }

        Transform D(LevelKit kit, PrimitiveType type, Vector3 p, Vector3 s, Material m, Vector3 e = default(Vector3), Transform parent = null)
        {
            return kit.Deco(type, p, s, m, e, parent != null ? parent : transform);
        }

        static Transform Pivot(Transform parent, Vector3 at)
        {
            var p = new GameObject("Pivot").transform;
            p.SetParent(parent, false);
            p.localPosition = at;
            return p;
        }

        public void Build(LevelKit kit)
        {
            var brass = PatchworkAssets.Plain(new Color(0.85f, 0.65f, 0.3f), 0.8f);
            var wood = PatchworkAssets.Wood(new Color(0.42f, 0.27f, 0.17f));
            var iron = Mat(new Color(0.22f, 0.22f, 0.26f));
            var red = PatchworkAssets.Felt(new Color(0.75f, 0.2f, 0.22f));
            switch (Kind)
            {
                case 0:   // The gramophone: a wooden cabinet, a turntable, a great brass horn
                    D(kit, PrimitiveType.Cube, new Vector3(0f, 0.5f, 0f), new Vector3(1f, 1f, 1f), wood);
                    D(kit, PrimitiveType.Cube, new Vector3(0f, 1.03f, 0f), new Vector3(1.08f, 0.06f, 1.08f), wood);
                    disc = D(kit, PrimitiveType.Cylinder, new Vector3(0f, 1.08f, 0.05f), new Vector3(0.78f, 0.015f, 0.78f), Mat(new Color(0.08f, 0.07f, 0.09f)));
                    D(kit, PrimitiveType.Cylinder, new Vector3(0f, 0.002f, 0f), new Vector3(0.3f, 1f, 0.3f), red, default(Vector3), disc);
                    horn = Pivot(transform, new Vector3(0.25f, 1.15f, -0.3f));
                    D(kit, PrimitiveType.Cylinder, new Vector3(0f, 0.25f, 0f), new Vector3(0.06f, 0.25f, 0.06f), brass, default(Vector3), horn);
                    for (int i = 0; i < 6; i++)
                        D(kit, PrimitiveType.Cylinder, new Vector3(0f, 0.55f + i * 0.1f, 0.12f + i * 0.06f), new Vector3(0.14f + i * 0.13f, 0.06f, 0.14f + i * 0.13f), brass, new Vector3(-40f - i * 6f, 0f, 0f), horn);
                    crank = Pivot(transform, new Vector3(0.52f, 0.6f, 0f));
                    D(kit, PrimitiveType.Cube, new Vector3(0.05f, 0f, 0f), new Vector3(0.1f, 0.05f, 0.05f), brass, default(Vector3), crank);
                    D(kit, PrimitiveType.Cube, new Vector3(0.1f, -0.12f, 0f), new Vector3(0.04f, 0.26f, 0.04f), brass, default(Vector3), crank);
                    D(kit, PrimitiveType.Cylinder, new Vector3(0.16f, -0.24f, 0f), new Vector3(0.06f, 0.06f, 0.06f), wood, new Vector3(0f, 0f, 90f), crank);
                    rings = new Transform[3];
                    for (int i = 0; i < 3; i++)
                    {
                        rings[i] = D(kit, PrimitiveType.Cylinder, new Vector3(0.25f, 2.2f, 0.3f), new Vector3(1f, 0.01f, 1f), PatchworkAssets.Glow(new Color(0.55f, 0.8f, 1f), 1.2f), new Vector3(70f, 0f, 0f));
                        rings[i].gameObject.SetActive(false);
                    }
                    break;

                case 1:   // The Archimedes screw: a tilted spiral from a trough up to a basin
                    D(kit, PrimitiveType.Cube, new Vector3(0f, 0.15f, 0.6f), new Vector3(1.2f, 0.3f, 0.8f), wood);
                    D(kit, PrimitiveType.Cube, new Vector3(0f, 0.29f, 0.6f), new Vector3(1f, 0.02f, 0.6f), PatchworkAssets.Glow(new Color(0.3f, 0.6f, 0.85f), 0.7f));
                    D(kit, PrimitiveType.Cylinder, new Vector3(0f, 1.5f, -0.65f), new Vector3(0.9f, 0.18f, 0.9f), wood);
                    D(kit, PrimitiveType.Cylinder, new Vector3(0f, 1.6f, -0.65f), new Vector3(0.74f, 0.02f, 0.74f), PatchworkAssets.Glow(new Color(0.3f, 0.6f, 0.85f), 0.7f));
                    D(kit, PrimitiveType.Cube, new Vector3(0f, 0.7f, -0.65f), new Vector3(0.16f, 1.4f, 0.16f), wood);
                    screw = Pivot(transform, new Vector3(0f, 0.85f, 0f));
                    screw.localRotation = Quaternion.Euler(-48f, 0f, 0f);   // tilted: low end in the trough, high end over the basin
                    D(kit, PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.1f, 0.95f, 0.1f), wood, default(Vector3), screw);
                    for (int i = 0; i < 18; i++)
                        D(kit, PrimitiveType.Cube, new Vector3(0f, -0.85f + i * 0.1f, 0f), new Vector3(0.36f, 0.03f, 0.1f), brass, new Vector3(0f, i * 40f, 0f), screw);
                    crank = Pivot(transform, new Vector3(0.3f, 1.5f, -0.65f));
                    D(kit, PrimitiveType.Cube, new Vector3(0.12f, 0f, 0f), new Vector3(0.24f, 0.05f, 0.05f), iron, default(Vector3), crank);
                    D(kit, PrimitiveType.Cylinder, new Vector3(0.24f, 0.1f, 0f), new Vector3(0.06f, 0.1f, 0.06f), wood, default(Vector3), crank);
                    drops = new Transform[6];
                    for (int i = 0; i < drops.Length; i++)
                    {
                        drops[i] = D(kit, PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.12f, PatchworkAssets.Glow(new Color(0.45f, 0.75f, 1f), 1.1f));
                        drops[i].gameObject.SetActive(false);
                    }
                    break;

                case 2:   // The lighthouse lens: a beehive of glass rings on an iron table
                    D(kit, PrimitiveType.Cylinder, new Vector3(0f, 0.4f, 0f), new Vector3(1.4f, 0.4f, 1.4f), iron);
                    D(kit, PrimitiveType.Cylinder, new Vector3(0f, 0.82f, 0f), new Vector3(1.5f, 0.03f, 1.5f), brass);
                    lens = Pivot(transform, new Vector3(0f, 0.85f, 0f));
                    var glass = PatchworkAssets.Bubble(new Color(0.75f, 0.9f, 0.95f, 0.45f));
                    for (int i = 0; i < 7; i++)
                    {
                        float r = 0.55f - Mathf.Abs(i - 3) * 0.08f;
                        D(kit, PrimitiveType.Cylinder, new Vector3(0f, 0.12f + i * 0.14f, 0f), new Vector3(r * 2f, 0.06f, r * 2f), glass, default(Vector3), lens);
                        D(kit, PrimitiveType.Cylinder, new Vector3(0f, 0.17f + i * 0.14f, 0f), new Vector3(r * 2f + 0.03f, 0.012f, r * 2f + 0.03f), brass, default(Vector3), lens);
                    }
                    D(kit, PrimitiveType.Sphere, new Vector3(0f, 1.1f, 0f), Vector3.one * 0.6f, PatchworkAssets.Bubble(new Color(0.9f, 0.95f, 1f, 0.3f)), default(Vector3), lens);
                    D(kit, PrimitiveType.Cylinder, new Vector3(0f, 1.95f, 0f), new Vector3(0.5f, 0.05f, 0.5f), brass, default(Vector3), lens);
                    var flame = D(kit, PrimitiveType.Sphere, new Vector3(0f, 1.1f, 0f), new Vector3(0.18f, 0.26f, 0.18f), PatchworkAssets.Glow(new Color(1f, 0.85f, 0.5f), 2.2f), default(Vector3), lens);
                    flame.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    beamCone = D(kit, PrimitiveType.Cylinder, new Vector3(0f, 1.1f, 3.5f), new Vector3(0.9f, 3.4f, 0.9f), PatchworkAssets.Bubble(new Color(1f, 0.92f, 0.65f, 0.12f)), new Vector3(90f, 0f, 0f), lens);
                    beamCone.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    beamCone.gameObject.SetActive(false);
                    var lg = new GameObject("Beam");
                    lg.transform.SetParent(lens, false);
                    lg.transform.localPosition = new Vector3(0f, 1.1f, 0.3f);
                    beam = lg.AddComponent<Light>();
                    beam.type = LightType.Spot;
                    beam.spotAngle = 26f;
                    beam.range = 30f;
                    beam.color = new Color(1f, 0.9f, 0.65f);
                    beam.intensity = 6f;
                    beam.shadows = LightShadows.None;
                    beam.enabled = false;
                    crank = Pivot(transform, new Vector3(0.72f, 0.5f, 0f));
                    D(kit, PrimitiveType.Cube, new Vector3(0.05f, 0f, 0f), new Vector3(0.1f, 0.05f, 0.05f), brass, default(Vector3), crank);
                    D(kit, PrimitiveType.Cube, new Vector3(0.1f, -0.12f, 0f), new Vector3(0.04f, 0.26f, 0.04f), brass, default(Vector3), crank);
                    break;

                case 3:   // The ship's bell: a bronze bell in a wooden frame, with a rope
                    for (int s = -1; s <= 1; s += 2)
                        D(kit, PrimitiveType.Cube, new Vector3(s * 0.55f, 1.1f, 0f), new Vector3(0.14f, 2.2f, 0.14f), wood);
                    D(kit, PrimitiveType.Cube, new Vector3(0f, 2.2f, 0f), new Vector3(1.4f, 0.16f, 0.2f), wood);
                    D(kit, PrimitiveType.Cube, new Vector3(0f, 0.08f, 0f), new Vector3(1.5f, 0.16f, 0.6f), wood);
                    bell = Pivot(transform, new Vector3(0f, 2.1f, 0f));
                    var bronze = PatchworkAssets.Plain(new Color(0.75f, 0.52f, 0.28f), 0.85f);
                    D(kit, PrimitiveType.Cylinder, new Vector3(0f, -0.08f, 0f), new Vector3(0.2f, 0.06f, 0.2f), bronze, default(Vector3), bell);
                    for (int i = 0; i < 5; i++)
                        D(kit, PrimitiveType.Cylinder, new Vector3(0f, -0.2f - i * 0.1f, 0f), new Vector3(0.32f + i * 0.07f, 0.06f, 0.32f + i * 0.07f), bronze, default(Vector3), bell);
                    D(kit, PrimitiveType.Cylinder, new Vector3(0f, -0.66f, 0f), new Vector3(0.7f, 0.03f, 0.7f), bronze, default(Vector3), bell);
                    clapper = Pivot(bell, new Vector3(0f, -0.2f, 0f));
                    D(kit, PrimitiveType.Cylinder, new Vector3(0f, -0.25f, 0f), new Vector3(0.03f, 0.25f, 0.03f), iron, default(Vector3), clapper);
                    D(kit, PrimitiveType.Sphere, new Vector3(0f, -0.52f, 0f), Vector3.one * 0.1f, iron, default(Vector3), clapper);
                    D(kit, PrimitiveType.Cylinder, new Vector3(0f, -0.95f, 0f), new Vector3(0.035f, 0.4f, 0.035f), PatchworkAssets.Knit(new Color(0.9f, 0.85f, 0.7f)), default(Vector3), clapper);
                    D(kit, PrimitiveType.Sphere, new Vector3(0f, -1.35f, 0f), new Vector3(0.09f, 0.14f, 0.09f), PatchworkAssets.Knit(new Color(0.9f, 0.85f, 0.7f)), default(Vector3), clapper);
                    break;

                default:  // The message tube: a brass pipe up from a desk, along the ceiling, out of sight
                    D(kit, PrimitiveType.Cube, new Vector3(0f, 0.45f, 0f), new Vector3(1.1f, 0.9f, 0.7f), wood);
                    D(kit, PrimitiveType.Cylinder, new Vector3(0f, 1.15f, -0.15f), new Vector3(0.24f, 0.26f, 0.24f), brass);
                    D(kit, PrimitiveType.Cylinder, new Vector3(0f, 1.42f, -0.15f), new Vector3(0.3f, 0.02f, 0.3f), brass);
                    D(kit, PrimitiveType.Cylinder, new Vector3(0f, 3.4f, -0.15f), new Vector3(0.18f, 2f, 0.18f), brass);
                    D(kit, PrimitiveType.Sphere, new Vector3(0f, 5.4f, -0.15f), Vector3.one * 0.18f, brass);
                    D(kit, PrimitiveType.Cylinder, new Vector3(0f, 5.4f, -3.15f), new Vector3(0.18f, 3f, 0.18f), brass, new Vector3(90f, 0f, 0f));
                    // A little rack of message capsules, and a gauge.
                    for (int i = 0; i < 3; i++)
                        D(kit, PrimitiveType.Capsule, new Vector3(-0.35f + i * 0.14f, 1.0f, 0.18f), new Vector3(0.1f, 0.12f, 0.1f), PatchworkAssets.Felt(new Color(0.55f + i * 0.1f, 0.4f, 0.3f)), new Vector3(90f, 0f, 0f));
                    D(kit, PrimitiveType.Cylinder, new Vector3(0.36f, 1.1f, 0.2f), new Vector3(0.22f, 0.02f, 0.22f), PatchworkAssets.Felt(new Color(0.95f, 0.93f, 0.85f)), new Vector3(90f, 0f, 0f));
                    crank = Pivot(transform, new Vector3(0.36f, 1.1f, 0.22f));
                    D(kit, PrimitiveType.Cube, new Vector3(0f, 0.04f, 0f), new Vector3(0.012f, 0.08f, 0.005f), iron, default(Vector3), crank);
                    capsule = D(kit, PrimitiveType.Capsule, new Vector3(0f, 1.3f, -0.15f), new Vector3(0.13f, 0.16f, 0.13f), PatchworkAssets.Felt(new Color(0.75f, 0.45f, 0.3f)));
                    capsule.gameObject.SetActive(false);
                    break;
            }
        }

        public void Use()
        {
            if (run >= 0f) return;
            run = 0f;
            strokes = 0;
            Sfx.Play(SfxId.Lever, 0.4f);
            Narrator.Say(Fact);
            if (Passport != null) Passport.Stamp(Kind, Title);
            if (Kind == 2) { beam.enabled = true; beamCone.gameObject.SetActive(true); }
        }

        float Duration { get { return Kind == 0 ? 9f : Kind == 2 ? 10f : Kind == 3 ? 6f : 3.2f; } }

        void Update()
        {
            float dt = Time.deltaTime;
            t += dt;
            // A little idle life, even when nobody's using it.
            if (Kind == 2 && lens != null && run < 0f) lens.localRotation = Quaternion.Euler(0f, Mathf.Sin(t * 0.3f) * 3f, 0f);
            if (run < 0f) return;
            run += dt;
            float k = run / Duration;
            switch (Kind)
            {
                case 0:
                    crank.localRotation = Quaternion.Euler(run < 1.2f ? -run * 720f : 0f, 0f, 0f);
                    disc.localRotation = Quaternion.Euler(0f, run * 200f, 0f);
                    horn.localScale = Vector3.one * (1f + Mathf.Sin(run * 9f) * 0.02f);
                    // The song: long low swoops, rising and falling.
                    if (strokes < 6 && run > 1.2f + strokes * 1.3f)
                    {
                        Sfx.PlayNear(SfxId.Chime, horn.position, 0.5f, 40f);
                        Sfx.PlayPitched(SfxId.Steam, 0.25f, 0.28f + (strokes % 3) * 0.06f);
                        strokes++;
                    }
                    for (int i = 0; i < rings.Length; i++)
                    {
                        float rk = Mathf.Repeat(run * 0.45f + i / 3f, 1f);
                        bool on = run > 1.2f && k < 0.95f;
                        rings[i].gameObject.SetActive(on);
                        rings[i].localPosition = new Vector3(0.25f, 2.2f + rk * 0.8f, 0.3f + rk * 2.2f);
                        rings[i].localScale = new Vector3(0.6f + rk * 3f, 0.01f, 0.6f + rk * 3f);
                    }
                    break;
                case 1:
                    crank.localRotation = Quaternion.Euler(0f, 0f, -run * 260f);
                    screw.localRotation = Quaternion.Euler(-48f, run * 260f, 0f);
                    // Drops climbing the spiral, then falling into the basin.
                    for (int i = 0; i < drops.Length; i++)
                    {
                        float dk = Mathf.Repeat(run * 0.4f + i / (float)drops.Length, 1f);
                        bool on = k < 0.92f;
                        drops[i].gameObject.SetActive(on);
                        if (!on) continue;
                        Vector3 along = screw.localPosition + screw.localRotation * new Vector3(0f, -0.85f + dk * 1.7f, 0f);
                        float a = dk * 30f;
                        along += screw.localRotation * new Vector3(Mathf.Cos(a) * 0.14f, 0f, Mathf.Sin(a) * 0.14f);
                        drops[i].localPosition = along;
                    }
                    if (strokes < 4 && run > 1f + strokes * 1.4f) { Sfx.PlayNear(SfxId.Splash, transform.position + Vector3.up * 1.6f, 0.25f, 20f); strokes++; }
                    break;
                case 2:
                    crank.localRotation = Quaternion.Euler(run * 400f, 0f, 0f);
                    lens.localRotation = Quaternion.Euler(0f, run * 70f, 0f);
                    float fade = Mathf.Clamp01(Mathf.Min(run / 0.6f, (Duration - run) / 1.2f));
                    beam.intensity = 6f * fade;
                    beamCone.localScale = new Vector3(0.9f, 3.4f, 0.9f) * Mathf.Max(0.01f, fade);
                    if (k >= 1f) { beam.enabled = false; beamCone.gameObject.SetActive(false); }
                    break;
                case 3:
                    // Eight bells: four pairs, ding-ding.
                    float beat = run / 0.65f;
                    int stroke = Mathf.FloorToInt(beat);
                    if (stroke < 8 && stroke >= strokes)
                    {
                        strokes = stroke + 1;
                        Sfx.PlayNear(SfxId.BigBell, bell.position, 0.5f, 50f);
                    }
                    float swing = stroke < 8 ? Mathf.Sin(beat * Mathf.PI) * 18f : 0f;
                    bell.localRotation = Quaternion.Euler(swing, 0f, 0f);
                    clapper.localRotation = Quaternion.Euler(-swing * 1.6f, 0f, 0f);
                    break;
                default:
                    crank.localRotation = Quaternion.Euler(0f, 0f, -Mathf.Min(run, 0.5f) * 400f);
                    capsule.gameObject.SetActive(run < 1.6f);
                    if (run < 0.5f) capsule.localPosition = new Vector3(0f, 1.6f - run * 0.6f, -0.15f);
                    else if (run < 0.9f) capsule.localPosition = new Vector3(0f, 1.3f + (run - 0.5f) * 10f, -0.15f);
                    else capsule.localPosition = new Vector3(0f, 5.4f, -0.15f - (run - 0.9f) * 12f);
                    if (strokes == 0 && run > 0.5f) { strokes = 1; Sfx.PlayNear(SfxId.Whoosh, transform.position + Vector3.up * 2f, 0.6f, 30f); }
                    if (strokes == 1 && run > 2.6f) { strokes = 2; Sfx.PlayNear(SfxId.PadClick, transform.position + Vector3.up * 5f, 0.4f, 40f); }
                    break;
            }
            if (k >= 1f) run = -1f;
        }
    }
}

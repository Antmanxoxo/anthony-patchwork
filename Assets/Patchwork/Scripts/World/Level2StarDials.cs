using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The last job before the whale: aiming the Night Glass. Stella tells the old rhyme the
    // keepers set its three star dials by, and the answers are written on three of the
    // museum's own plaques (a gold star floats over each once you've heard the rhyme):
    //   "Moons of the giant, as Galileo saw them."   -> 4    (The Great Telescope, in the Observatory)
    //   "The year the comet kept its promise."        -> 1758 (Halley's Comet, in the Comet Gallery)
    //   "The stars of the first catalogue, in hundreds." -> 8 (The First Star Catalogue, Night Sky Gallery)
    // Turn each dial (press Ability at it) to the right number, and the telescope finds her.
    public class StarDials : MonoBehaviour
    {
        public Level2Quest Quest;
        public Transform[] Pointers;
        public UseSpot[] Spots;
        public ExhibitPlaque[] Plaques;
        public Transform Glow;
        public Vector3 Centre;

        static readonly string[] Names = { "Moons dial", "Comet dial", "Stars dial" };
        static readonly int[][] Options =
        {
            new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            new[] { 1531, 1607, 1682, 1758, 1781, 1846 },
            new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
        };
        static readonly int[] Answer = { 4, 1758, 8 };
        static readonly string[] ClueLine =
        {
            "That's one of Stella's answers: Galileo saw FOUR moons going round Jupiter. (The Moons dial.)",
            "That's one of Stella's answers: the comet came back, right on time, in 1758. (The Comet dial.)",
            "That's one of Stella's answers: about eight hundred and fifty stars, so 8 hundreds. (The Stars dial.)",
        };
        public static StarDials Instance { get; private set; }
        readonly int[] at = new int[3];
        readonly bool[] clued = new bool[3];
        Transform[] stars;

        void Awake() { Instance = this; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        // Remembered, so a reload doesn't send you back round the plaques you've read.
        static string ClueKey(int i) { return "L2_q_clue" + i; }

        // The nearest of Stella's answer plaques not read yet (for the pointer on screen).
        public bool NearestClue(Vector3 from, out Vector3 where)
        {
            where = Vector3.zero;
            float best = float.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                if (clued[i] || Plaques == null || i >= Plaques.Length || Plaques[i] == null) continue;
                Vector3 p = Plaques[i].transform.position;
                float d = (p - from).sqrMagnitude;
                if (d < best) { best = d; where = p; }
            }
            return best < float.MaxValue;
        }

        void Start()
        {
            stars = new Transform[3];
            for (int i = 0; i < 3; i++) clued[i] = GameState.IsHintSeen(ClueKey(i));
            for (int i = 0; i < 3; i++)
            {
                int which = i;
                if (Spots != null && Spots[i] != null) Spots[i].Used = p => Turn(which);
                if (Plaques != null && i < Plaques.Length && Plaques[i] != null)
                {
                    Plaques[i].Read += () => Clue(which);
                    stars[i] = BuildStar(Plaques[i].Head + Vector3.up * 1.35f);
                }
            }
            Refresh();
        }

        static Transform BuildStar(Vector3 at)
        {
            var root = new GameObject("ClueStar").transform;
            root.position = at;
            var gold = PatchworkAssets.Glow(new Color(1f, 0.85f, 0.35f), 2f);
            for (int arm = 0; arm < 5; arm++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(go.GetComponent<Collider>());
                go.transform.SetParent(root, false);
                var r = Quaternion.Euler(0f, 0f, arm * 72f);
                go.transform.localRotation = r;
                go.transform.localPosition = r * new Vector3(0f, 0.16f, 0f);
                go.transform.localScale = new Vector3(0.1f, 0.34f, 0.06f);
                go.GetComponent<Renderer>().sharedMaterial = gold;
            }
            root.gameObject.SetActive(false);
            return root;
        }

        bool Ready { get { return Quest != null && Quest.Defended && !Quest.DialsSet; } }

        void Turn(int i)
        {
            var game = PatchworkGame.Instance;
            if (Quest == null || game == null) return;
            if (Quest.DialsSet) { game.ShowToast("The dials are set. She's in the glass: go and look!"); return; }
            if (!Quest.Defended) { game.ShowToast("A brass star dial. It won't turn while the Night Glass is blind."); return; }
            at[i] = (at[i] + 1) % Options[i].Length;
            Sfx.PlayPitched(SfxId.Clack, 0.5f, 1.2f + at[i] * 0.03f);
            Refresh();
            bool all = true;
            for (int k = 0; k < 3; k++) if (Options[k][at[k]] != Answer[k]) all = false;
            if (all)
            {
                Quest.SetDials();
                if (Glow != null) Glow.gameObject.SetActive(true);
                FxBurst.Spawn(FxBurst.Kind.Sparkle, Centre + Vector3.up * 1.5f, new Color(1f, 0.85f, 0.4f));
                Refresh();
            }
        }

        void Clue(int i)
        {
            if (!Ready || Quest == null || !Quest.RiddleGiven || clued[i]) return;
            clued[i] = true;
            GameState.SetHintSeen(ClueKey(i));
            var game = PatchworkGame.Instance;
            if (game != null) game.ShowToast(ClueLine[i]);
            Sfx.PlayPitched(SfxId.Twinkle, 0.4f, 1.1f);
        }

        void Refresh()
        {
            for (int i = 0; i < 3; i++)
            {
                if (Pointers != null && Pointers[i] != null)
                    Pointers[i].localRotation = Quaternion.Euler(0f, 0f, -at[i] * (360f / Options[i].Length));
                if (Spots != null && Spots[i] != null)
                    Spots[i].Label = Quest != null && Quest.DialsSet ? Names[i] + ": " + Answer[i] : Names[i] + ": " + Options[i][at[i]] + "   (turn it)";
            }
            if (Glow != null) Glow.gameObject.SetActive(Quest != null && Quest.DialsSet);
        }

        void Update()
        {
            bool show = Ready && Quest.RiddleGiven;
            for (int i = 0; i < 3; i++)
            {
                if (stars[i] == null) continue;
                bool want = show && !clued[i];
                if (stars[i].gameObject.activeSelf != want) stars[i].gameObject.SetActive(want);
                if (want) stars[i].rotation = Quaternion.Euler(0f, Time.time * 90f, 0f);
            }
        }
    }

    // When Patch wins a lens: everything stops, the camera swings round in front of him, and he
    // holds it up over his head while it spins and shines, to a fanfare. Then on with the night.
    public class LensCelebration : MonoBehaviour
    {
        public static bool Busy { get; private set; }
        string title, kicker;
        float t;
        Transform lens, rays;
        Vector3 eye, look;

        Color glassCol = new Color(0.55f, 0.85f, 1f);

        public static void Play(string name, int count)
        {
            PlayItem(name + "!", count >= 3 ? "All three lenses" : count + " of 3 lenses", new Color(0.55f, 0.85f, 1f));
        }

        // The same moment for anything else that matters as much (Toybox Town's band pieces).
        public static void PlayItem(string title, string kicker, Color glow)
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null || game.Mode != UIMode.Gameplay || Busy) return;
            var go = new GameObject("ItemCelebration");
            var c = go.AddComponent<LensCelebration>();
            c.title = title;
            c.kicker = kicker;
            c.glassCol = glow;
            c.Begin(game);
        }

        void Begin(PatchworkGame game)
        {
            Busy = true;
            var p = game.Player.transform;
            Vector3 fwd = game.CameraRig != null ? game.CameraRig.transform.position - p.position : -p.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.01f) fwd = -p.forward;
            fwd.Normalize();
            eye = p.position + fwd * 4.2f + Vector3.up * 1.9f;
            look = p.position + Vector3.up * 1.5f;
            game.CutsceneSkippable = false;
            game.CutsceneCaption = null;
            game.StartScriptedCutscene(eye, look, 3f);
            // The lens, up over his head, with a sunburst behind it.
            lens = new GameObject("HeldLens").transform;
            lens.position = p.position + Vector3.up * 2.5f;
            var glass = PatchworkAssets.Glow(glassCol, 2.4f);
            var rim = PatchworkAssets.Plain(new Color(0.95f, 0.78f, 0.4f), 0.8f);
            Part(lens, PrimitiveType.Cylinder, new Vector3(0.8f, 0.05f, 0.8f), glass, new Vector3(90f, 0f, 0f), Vector3.zero);
            Part(lens, PrimitiveType.Cylinder, new Vector3(0.92f, 0.04f, 0.92f), rim, new Vector3(90f, 0f, 0f), Vector3.zero);
            rays = new GameObject("Rays").transform;
            rays.SetParent(lens, false);
            var gold = PatchworkAssets.Glow(new Color(1f, 0.88f, 0.5f), 1.6f);
            for (int i = 0; i < 10; i++)
            {
                var r = Quaternion.Euler(0f, 0f, i * 36f);
                Part(rays, PrimitiveType.Cube, new Vector3(0.07f, 1.1f, 0.02f), gold, r.eulerAngles, r * new Vector3(0f, 0.9f, -0.05f));
            }
            Sfx.Play(SfxId.PrizeFanfare, 0.7f);
            Sfx.Voice(SfxId.VoiceYay, game.Player.Forms.Current, 0.5f);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, lens.position, glassCol);
            Haptics.Pulse(0.5f, 0.6f, 0.4f);
        }

        static void Part(Transform parent, PrimitiveType type, Vector3 s, Material m, Vector3 e, Vector3 p)
        {
            var go = GameObject.CreatePrimitive(type);
            Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localScale = s;
            go.transform.localRotation = Quaternion.Euler(e);
            go.transform.localPosition = p;
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = m;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) { Done(game); return; }
            t += Time.unscaledDeltaTime;
            var p = game.Player.transform;
            // He hops for joy (twice), then holds it up.
            if (t > 0.1f && t - Time.unscaledDeltaTime <= 0.1f) game.Player.Visuals.PunchScale(new Vector3(0.75f, 1.35f, 0.75f));
            if (t > 0.9f && t - Time.unscaledDeltaTime <= 0.9f) game.Player.Visuals.PunchScale(new Vector3(1.25f, 0.8f, 1.25f));
            float rise = UIKit.EaseOutBack(Mathf.Clamp01(t / 0.6f));
            if (lens != null)
            {
                lens.position = p.position + Vector3.up * (1.6f + rise * 1.2f + Mathf.Sin(t * 3f) * 0.06f);
                lens.rotation = Quaternion.LookRotation((eye - lens.position).normalized, Vector3.up) * Quaternion.Euler(0f, Mathf.Sin(t * 2f) * 25f, 0f);
                if (rays != null) { rays.localRotation = Quaternion.Euler(0f, 0f, t * 60f); rays.localScale = Vector3.one * (0.8f + Mathf.Sin(t * 5f) * 0.12f) * rise; }
                if (Mathf.Repeat(t, 0.5f) < Time.unscaledDeltaTime) FxBurst.Spawn(FxBurst.Kind.Sparkle, lens.position, new Color(1f, 0.9f, 0.6f));
            }
            Narrator.Card(kicker, title);
            // A slow push in.
            game.SetCutsceneCamera(Vector3.Lerp(eye, look + (eye - look) * 0.75f, Mathf.SmoothStep(0f, 1f, t / 3.4f)), Vector3.Lerp(look, lens != null ? lens.position : look, 0.4f));
            if (t > 3.4f) Done(game);
        }

        void Done(PatchworkGame game)
        {
            Busy = false;
            Narrator.Card(null, null);
            if (lens != null) Destroy(lens.gameObject);
            if (game != null && game.Mode == UIMode.Cutscene) game.EndCutscene();
            Destroy(gameObject);
        }

        void OnDestroy() { Busy = false; }
    }

    public static partial class Level2Builder
    {
        // The three plaques that hold the answers to Stella's rhyme (set where they're built).
        static readonly ExhibitPlaque[] riddlePlaques = new ExhibitPlaque[3];

        // The star dials: three brass dials on a little console behind the great telescope.
        static void StarDialsAt(LevelKit kit, Level2Quest quest)
        {
            Vector3 C = Observatory;
            Vector3 foot;
            if (!RmFloor("the star dials", C + new Vector3(0f, 1.5f, 3.7f), new Vector3(5f, 1.8f, 1.2f), 0f, 3f, out foot)) foot = C + new Vector3(0f, 1.5f, 3.7f);
            var go = new GameObject("StarDials");
            go.transform.SetParent(kit.Root, false);
            var dials = go.AddComponent<StarDials>();
            dials.Quest = quest;
            dials.Centre = foot;
            dials.Plaques = new[] { riddlePlaques[0], riddlePlaques[1], riddlePlaques[2] };
            dials.Pointers = new Transform[3];
            dials.Spots = new UseSpot[3];
            var navy = PatchworkAssets.Felt(new Color(0.12f, 0.14f, 0.32f));
            var gold = PatchworkAssets.Glow(new Color(1f, 0.82f, 0.4f), 1.4f);
            kit.Deco(PrimitiveType.Cube, foot + new Vector3(0f, 0.45f, 0f), new Vector3(4.6f, 0.9f, 0.7f), DarkWood);
            NoShadow(kit.Deco(PrimitiveType.Cube, foot + new Vector3(0f, 0.92f, 0f), new Vector3(4.7f, 0.06f, 0.8f), Brass));
            TownKit.Solid(kit, foot + new Vector3(0f, 0.8f, 0f), new Vector3(4.6f, 1.6f, 0.7f), 0f);
            for (int i = 0; i < 3; i++)
            {
                Vector3 d = foot + new Vector3(-1.5f + i * 1.5f, 1.45f, 0.1f);
                // A round brass dial standing up on the console, its face to the north side.
                NoShadow(kit.Deco(PrimitiveType.Cylinder, d, new Vector3(0.95f, 0.05f, 0.95f), Brass, new Vector3(90f, 0f, 0f)));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, d + Vector3.forward * 0.04f, new Vector3(0.82f, 0.02f, 0.82f), navy, new Vector3(90f, 0f, 0f)));
                for (int k = 0; k < 8; k++)
                {
                    float a = k / 8f * Mathf.PI * 2f;
                    NoShadow(kit.Deco(PrimitiveType.Sphere, d + new Vector3(Mathf.Sin(a) * 0.33f, Mathf.Cos(a) * 0.33f, 0.07f), Vector3.one * 0.05f, gold));
                }
                var pointer = new GameObject("DialPointer").transform;
                pointer.SetParent(go.transform, false);
                pointer.position = d + Vector3.forward * 0.08f;
                pointer.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.15f, 0f), new Vector3(0.06f, 0.32f, 0.02f), gold, default(Vector3), pointer));
                dials.Pointers[i] = pointer;
                var spot = kit.Volume<UseSpot>(d + new Vector3(0f, -0.3f, 0.9f), new Vector3(1.3f, 2.4f, 1.8f), "StarDial" + i);
                spot.Label = "Star dial";
                spot.PromptPoint = d + Vector3.up * 0.9f;
                dials.Spots[i] = spot;
            }
            var glow = kit.Group("DialsGlow", foot + Vector3.up * 1.45f);
            glow.SetParent(go.transform, true);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.6f, 0f), new Vector3(4.4f, 0.06f, 0.06f), gold, default(Vector3), glow));
            dials.Glow = glow;
            quest.DialsSpot = foot;
        }

        // More of the Fray about the place: grey tears and tendrils in the corners of the big
        // rooms (they wither away once the Night Glass is safe), and little patrols of chessmen
        // and rollers waiting in the halls.
        static void MoreFray(LevelKit kit, Level2Quest quest)
        {
            var root = kit.Group("MoreFray", Vector3.zero);
            Vector3[] tears =
            {
                new Vector3(-24f, 0f, -100f), new Vector3(24f, 0f, -62f), new Vector3(-80f, 0f, -96f), new Vector3(-38f, 0f, -20f),
                new Vector3(-82f, 0f, 8f), new Vector3(-38f, 0f, 70f), new Vector3(52f, 0f, 92f), new Vector3(94f, 0f, 16f),
                new Vector3(64f, 0f, -48f), new Vector3(36f, DeckY, -40f), new Vector3(52f, DeckY, -14f), new Vector3(-84f, 0f, 90f)
            };
            int placed = 0;
            for (int i = 0; i < tears.Length; i++)
            {
                Vector3 foot;
                if (!RmFloor("a Fray tear", tears[i], new Vector3(2.4f, 2.6f, 2.4f), 0f, 6f, out foot)) continue;
                FrayTheme.Tear(kit, foot, 1.8f).SetParent(root, true);
                FrayTheme.Tendril(kit, foot + new Vector3(0.6f, 0f, 0.3f), 1.8f + (i % 3) * 0.5f).SetParent(root, true);
                if (i % 2 == 0) FrayTheme.Tendril(kit, foot + new Vector3(-0.5f, 0f, -0.4f), 1.2f).SetParent(root, true);
                placed++;
            }
            quest.FrayDressing = root;
            // Where the patrols wait.
            var spots = new List<Vector3>();
            Vector3[] want =
            {
                new Vector3(-14f, 0f, -70f), new Vector3(16f, 0f, -88f), new Vector3(-60f, 0f, -42f), new Vector3(-70f, 0f, -80f),
                new Vector3(-48f, 0f, 14f), new Vector3(-72f, 0f, 56f), new Vector3(60f, 0f, 44f), new Vector3(84f, 0f, 74f),
                new Vector3(44f, DeckY, -13f), new Vector3(-60f, 0f, 100f)
            };
            foreach (var w in want)
            {
                Vector3 foot;
                if (RmFloor("a Fray patrol", w, new Vector3(3f, 1.6f, 3f), 0f, 6f, out foot)) spots.Add(foot);
            }
            quest.PatrolSpots = spots.ToArray();
            Debug.Log("[Level2] More Fray: " + placed + " tears, " + spots.Count + " patrols.");
        }
    }
}

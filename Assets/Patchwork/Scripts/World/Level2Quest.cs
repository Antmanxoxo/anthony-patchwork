using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Chapter Two: The Night Glass.
    //
    // Every night at midnight the keepers of the Oceanic Museum point the great telescope in
    // the Observatory (the Night Glass) up through the dome and find the Great Whale, the
    // oldest thing in the museum, as she goes by. She sings when she's seen, and her song is
    // what keeps the colour in the place: in the tanks, the tapestries, the paint on the
    // rocking horses. It has been done every night for longer than anyone can remember.
    //
    // Tonight, at dusk, the Fray came up through the floor and took the telescope's three
    // lenses. Without them the Night Glass is blind; if midnight passes and nobody sees her,
    // the whale goes by unsung and the museum goes grey by morning. Already the exhibits are
    // waking up wrong.
    //
    // So Patch has three lenses to win back, and each is held a different way:
    //   The Object Glass - the Hollow Captain wears it on his chest, in the Hall of Arms. The
    //                      portcullis drops behind you and the armours step down in three waves.
    //   The Field Lens   - the Fray has knotted grey clots round the hearts of the keepers' four
    //                      tanks. Dive in, break all four; the last one coughs it up.
    //   The Eye Lens     - a brass diving helmet has grown legs and run off with it. It bolts
    //                      twice (Atrium, Kelp Galleries) and turns to fight in the Sunken Ruins.
    // Then up the lift (which still wants three Heartstrings), where Stella fits them, the
    // moths come for the light, and at last Patch can look.
    //
    // Round the edges of that:
    //   - Six pages of Wick's night book are scattered about. Each is an hour of the evening
    //     in his own words, and together they say who took the lenses.
    //   - The museum clock (on the panel) starts at eleven and creeps toward midnight with
    //     every lens.
    //   - Three lamps by Wick's post light one by one as the lenses come home.
    //   - Each clot in the tanks is held by three anchor knots that have to go first.
    //   - The Helmet Crab sits tight in the Kelp Galleries until its chessmen are dealt with.
    //   - The Unpicker himself turns up, twice, to see how it's going.
    //
    // Everything saved here starts "L2_q_".
    public static partial class Level2Builder
    {
        static readonly string[] TankNames = { "The Coral Nursery", "The Lantern Deep", "The Frozen Sea", "The Mangrove Shallows" };

        static void Quest(LevelKit kit)
        {
            var root = kit.Group("Level2Quest", Vector3.zero);
            var quest = root.gameObject.AddComponent<Level2Quest>();
            quest.Director = director;
            quest.FoeRoot = root;
            if (kit.Root.GetComponent<GreyStain>() == null) kit.Root.gameObject.AddComponent<GreyStain>();

            // ----- Wick, the night watchman, by the atrium checkpoint -----
            Vector3 wickAt = new Vector3(-3.6f, 0f, -108f), foot;
            if (RmFloor("the night watchman", wickAt, new Vector3(1.6f, 2.2f, 1.6f), 0f, 5f, out foot)) wickAt = foot;
            var wick = Person(kit, wickAt, 150f, ToyNpc.Kind.TinSoldier, "Wick", new Color(0.2f, 0.24f, 0.4f), new Color(1f, 0.8f, 0.35f), 0.78f, () => quest.WickLines());
            wick.TalkEnded = quest.WickTalked;
            // His lantern, set down beside him.
            var lamp = kit.Group("WicksLantern", wickAt + new Vector3(0.9f, 0f, 0.3f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.04f, 0f), new Vector3(0.34f, 0.04f, 0.34f), Brass, default(Vector3), lamp);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.3f, 0f), new Vector3(0.26f, 0.22f, 0.26f), PatchworkAssets.Glow(new Color(1f, 0.78f, 0.4f), 2.2f), default(Vector3), lamp));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.55f, 0f), new Vector3(0.36f, 0.04f, 0.36f), Brass, default(Vector3), lamp);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.66f, 0f), new Vector3(0.16f, 0.2f, 0.16f), Brass, default(Vector3), lamp);
            PointLight(kit, wickAt + new Vector3(0.9f, 0.6f, 0.3f), new Color(1f, 0.75f, 0.4f), 7f, 1.6f);
            // The three lens lamps on a stand behind him: one lights for each lens that comes home.
            var stand = kit.Group("LensLamps", wickAt + new Vector3(-1.3f, 0f, -0.5f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.05f, 0f), new Vector3(0.7f, 0.05f, 0.7f), IronMat, default(Vector3), stand);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.2f, 0f), new Vector3(0.09f, 1.2f, 0.09f), IronMat, default(Vector3), stand);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 2.3f, 0f), new Vector3(1.9f, 0.08f, 0.08f), IronMat, default(Vector3), stand);
            quest.LampGlows = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                float lx = (i - 1) * 0.8f;
                kit.Deco(PrimitiveType.Cylinder, new Vector3(lx, 2.12f, 0f), new Vector3(0.02f, 0.16f, 0.02f), IronMat, default(Vector3), stand);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(lx, 1.96f, 0f), new Vector3(0.3f, 0.03f, 0.3f), Brass, default(Vector3), stand);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(lx, 1.56f, 0f), new Vector3(0.3f, 0.03f, 0.3f), Brass, default(Vector3), stand);
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(lx, 1.76f, 0f), new Vector3(0.3f, 0.36f, 0.3f), PatchworkAssets.Bubble(new Color(0.5f, 0.55f, 0.65f, 0.35f)), default(Vector3), stand));
                quest.LampGlows[i] = NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(lx, 1.76f, 0f), new Vector3(0.22f, 0.28f, 0.22f), PatchworkAssets.Glow(new Color(0.55f, 0.85f, 1f), 2.4f), default(Vector3), stand));
                quest.LampGlows[i].gameObject.SetActive(false);
            }

            // ----- Six pages of his night book, blown about the museum -----
            Vector3[] pageAt = { wickAt + new Vector3(3f, 0f, 5f), new Vector3(-48f, 0f, -77f), new Vector3(-63f, 0f, 17.5f), new Vector3(-47f, 0f, 73f), new Vector3(-52f, 0f, 88f), new Vector3(77f, 0f, 10f) };
            for (int i = 0; i < pageAt.Length; i++)
                if (RmFloor("a page of the night book", pageAt[i], new Vector3(1.2f, 2f, 1.2f), 0f, 5f, out foot)) pageAt[i] = foot;
            quest.PageSpots = pageAt;

            // ----- The Hall of Arms: the portcullis and the Hollow Guard -----
            var fight = root.gameObject.AddComponent<HallOfArmsFight>();
            fight.Quest = quest;
            quest.Fight = fight;
            var gate = kit.Group("ArmsPortcullis", new Vector3(80f, 0f, 5f));
            for (int i = 0; i < 9; i++)
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(-3.6f + i * 0.9f, 4f, 0f), new Vector3(0.16f, 4f, 0.16f), IronMat, default(Vector3), gate);
                kit.Deco(PrimitiveType.Cube, new Vector3(-3.6f + i * 0.9f, 0.12f, 0f), new Vector3(0.2f, 0.2f, 0.2f), IronMat, new Vector3(45f, 0f, 45f), gate);
            }
            for (int j = 0; j < 4; j++)
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.9f + j * 2.2f, 0f), new Vector3(8.3f, 0.16f, 0.22f), IronMat, default(Vector3), gate);
            var bar = gate.gameObject.AddComponent<BoxCollider>();
            bar.center = new Vector3(0f, 4f, 0f);
            bar.size = new Vector3(8.6f, 8f, 0.7f);
            gate.gameObject.SetActive(false);
            fight.Gate = gate;

            float[] zs = { -4f, -13f, -22f, -31f };
            var kinds = new[] { HollowGuard.Suit.Knight, HollowGuard.Suit.Hoplite, HollowGuard.Suit.Legionary, HollowGuard.Suit.Viking,
                HollowGuard.Suit.Samurai, HollowGuard.Suit.Terracotta, HollowGuard.Suit.Mongol, HollowGuard.Suit.Eagle };
            fight.Suits = new Transform[8];
            fight.Guards = new HollowGuard[8];
            fight.Landings = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                bool west = i < 4;
                Vector3 plinth = new Vector3(west ? 67f : 93f, 0.6f, zs[i % 4]);
                fight.Suits[i] = kit.Root.Find("Armour" + kinds[i]);
                fight.Guards[i] = HollowGuard.Create(root, plinth, west ? 90f : 270f, kinds[i], i == 0);
                fight.Landings[i] = new Vector3(west ? 71.4f : 88.6f, 0f, zs[i % 4]);
            }

            // ----- The four tanks: a grey clot in each, and the jellies round it -----
            Vector3[] lair = { new Vector3(-74.5f, -6.5f, 112f), new Vector3(-46.5f, -17f, 112f), new Vector3(-74.5f, -6.5f, 154.5f), new Vector3(-46.5f, -4.4f, 154.5f) };
            float[] bed = { -9f, -24f, -12f, -7f };
            float[] tankX0 = { -86f, -57f, -86f, -57f }, tankX1 = { -63f, -36f, -63f, -36f }, tankZ0 = { 98f, 98f, 134f, 134f }, tankZ1 = { 126f, 126f, 175f, 175f };
            quest.TankNames = TankNames;
            quest.ClotSpots = lair;
            quest.Clots = new GreyClot[4];
            for (int i = 0; i < 4; i++)
            {
                if (GameState.IsHintSeen(Level2Quest.ClotKey(i))) continue;
                // Somewhere in open water near the middle of the tank.
                Vector3 at;
                if (RmAir("a grey clot", lair[i], Vector3.one * 2.6f, 0f, 5f, out at, 1.5f)) lair[i] = at;
                var clot = GreyClot.Create(root, lair[i], i * 1.7f);
                quest.Clots[i] = clot;
                // Three anchor knots, out toward the corners of the tank.
                for (int k = 0; k < 3; k++)
                {
                    float ang = k * Mathf.PI * 2f / 3f + i * 0.9f + 0.5f;
                    Vector3 a = lair[i] + new Vector3(Mathf.Cos(ang) * 7f, 0f, Mathf.Sin(ang) * 9f);
                    a.x = Mathf.Clamp(a.x, tankX0[i] + 2.2f, tankX1[i] - 2.2f);
                    a.z = Mathf.Clamp(a.z, tankZ0[i] + 2.2f, tankZ1[i] - 2.2f);
                    a.y = Mathf.Clamp(lair[i].y + (k - 1) * 1.6f, bed[i] + 1.6f, -2.2f);
                    Vector3 clear;
                    if (RmAir("an anchor knot", a, Vector3.one * 1.4f, 0f, 3f, out clear, 1.2f)) a = clear;
                    GreyAnchor.Create(root, a, clot);
                }
                int jellies = i == 1 ? 4 : 3;
                for (int j = 0; j < jellies; j++)
                {
                    float a = j * Mathf.PI * 2f / jellies + i;
                    Vector3 p = lair[i] + new Vector3(Mathf.Cos(a) * 5f, (j % 2) * 1.2f - 0.4f, Mathf.Sin(a) * 5f);
                    p.y = Mathf.Clamp(p.y, bed[i] + 1.5f, -1.8f);
                    InkJelly.Create(root, p, bed[i] + 1.2f, -1.6f);
                }
            }

            // ----- The Helmet Crab, and the three places it turns up -----
            Vector3[] want = { new Vector3(2f, 0f, -90f), new Vector3(-46f, 0f, 66f), new Vector3(72f, 0f, 16f) };
            for (int i = 0; i < 3; i++)
                if (RmFloor("the Helmet Crab", want[i], new Vector3(2.6f, 2.2f, 2.6f), 0f, 8f, out foot)) want[i] = foot;
            quest.CrabSpots = want;
            quest.Crab = HelmetCrab.Create(root);
        }
    }

    // Runs the story of the three lenses: who has been met, what's been won, what the
    // watchman and the keepers say about it, and whether the Night Glass can see yet.
    public class Level2Quest : MonoBehaviour
    {
        public static Level2Quest Instance { get; private set; }
        public static void ResetStatics() { Instance = null; }

        public Level2Director Director;
        public Transform FoeRoot;
        public HallOfArmsFight Fight;
        public HelmetCrab Crab;
        public Vector3[] CrabSpots, ClotSpots;
        public GreyClot[] Clots;
        public string[] TankNames;
        public Vector3[] PageSpots;
        public Transform[] LampGlows;

        const string K = "L2_q_";
        public static string ClotKey(int i) { return K + "clot" + i; }
        static readonly string[] LensNames = { "The Object Glass", "The Field Lens", "The Eye Lens" };
        static readonly string[] CrabPlaces = { "the Atrium", "the Kelp Galleries", "the Sunken Ruins" };
        static readonly Color LensBlue = new Color(0.35f, 0.7f, 0.95f);
        static readonly Vector3 ObservatoryCentre = new Vector3(0f, 14f, -20f);

        readonly List<StaticMoth> moths = new List<StaticMoth>();
        readonly List<Chessling> escorts = new List<Chessling>();
        UnpickerVisit visitor;
        bool defending, toldHeld;
        float tick;

        // Wick's night book: an hour of the evening on each page.
        const int Pages = 6;
        static readonly string[] PageText =
        {
            "\"Four o'clock. Lit the lamps. Counted the exhibits: all present. Whale due at midnight, same as every night. Stella says the dome wants oiling.\"",
            "\"Five. A loose thread on the floor of Deep Time. Grey, a yard long, and cold. Put it in my pocket to throw away. It is not in my pocket now.\"",
            "\"Half past five. The paint on the old ship has gone pale down one side, as if something had licked it. Barnacle says he heard scissors.\"",
            "\"Six. Chess case in the Kelp Galleries standing open. Counted the chessmen: seven short. Something in the corner went tick, tick, tick, like claws on tile.\"",
            "\"Seven. Marlow sent for me. Water in the Coral Nursery gone cloudy. Told him it was the filters. I did not think it was the filters.\"",
            "\"Dusk. A tall fellow on the Observatory stair, a thimble for a hat, a lens under each arm and one in his teeth. He bowed to me. Then the lamps went out.\""
        };
        public int PagesFound { get { int n = 0; for (int i = 0; i < Pages; i++) if (Seen("page" + i)) n++; return n; } }

        // The museum clock: eleven when Patch arrives, and later with every lens.
        public string Clock()
        {
            if (Defended) return "Midnight";
            if (Fitted) return "11:58";
            int n = LensCount;
            return n >= 3 ? "11:50" : n == 2 ? "11:35" : n == 1 ? "11:20" : "11:00";
        }

        static bool Seen(string key) { return GameState.IsHintSeen(K + key); }
        static void Mark(string key) { GameState.SetHintSeen(K + key); }

        public bool Met { get { return Seen("met"); } }
        public bool HasLens(int i) { return Seen("lens" + i); }
        public int LensCount { get { return (HasLens(0) ? 1 : 0) + (HasLens(1) ? 1 : 0) + (HasLens(2) ? 1 : 0); } }
        public bool ArmsWon { get { return Seen("arms"); } }
        public bool Fitted { get { return Seen("fitted"); } }
        public bool Defended { get { return Seen("defended"); } }
        public int ClotsCleared { get { int n = 0; for (int i = 0; i < 4; i++) if (GameState.IsHintSeen(ClotKey(i))) n++; return n; } }
        int CrabStage { get { return Seen("crab2") ? 2 : Seen("crab1") ? 1 : 0; } }
        bool CrabBeaten { get { return Seen("crab"); } }

        void Awake() { Instance = this; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        void Start()
        {
            // The clots that are still there.
            for (int i = 0; i < 4; i++)
            {
                if (Clots == null || Clots[i] == null) continue;
                int which = i;
                Clots[i].Beaten = f => ClotBeaten(which, f.Center);
                Clots[i].Held = c => ClotHeld();
                Clots[i].Loosed = c =>
                {
                    var g = PatchworkGame.Instance;
                    if (g != null) g.ShowToast("The last anchor's gone. The clot hangs loose: hit it!");
                };
            }
            for (int i = 0; i < Pages && PageSpots != null; i++)
            {
                if (Seen("page" + i)) continue;
                int page = i;
                NightBookPage.Spawn(FoeRoot, PageSpots[i] + Vector3.up * 1.2f).Taken = () => TakePage(page);
            }
            RefreshLamps();
            // Anything won on an earlier visit but not picked up is waiting where it fell.
            if (ArmsWon && !HasLens(0)) DropLens(0, new Vector3(80f, 1.3f, -10f), false);
            if (ClotsCleared >= 4 && !HasLens(1)) DropLens(1, ClotSpots[0], false);
            if (CrabBeaten && !HasLens(2)) DropLens(2, CrabSpots[2] + Vector3.up * 1.3f, false);

            if (Crab != null)
            {
                Crab.Fled = c => CrabFled();
                Crab.Beaten = f => CrabFell(f.Center);
                if (!CrabBeaten) PlaceCrab(CrabStage > 0);
            }
            if (Fitted && !Defended) StartDefence(false);

            WrapNpc("Keeper Marlow", MarlowLines, null);
            WrapNpc("The Castellan", CastellanLines, null);
            WrapNpc("Stella", StellaLines, StellaTalked);
            WrapNpc("Button", () => CrabGossip(1, "Button"), null);
            WrapNpc("Dr. Trowel", () => CrabGossip(2, "Trowel"), null);
            WrapNpc("Thimble", () => CrabGossip(0, "Thimble"), null);
        }

        // ------------------------------------------------------------------ The lenses

        void DropLens(int which, Vector3 at, bool hop)
        {
            var lens = LensPickup.Spawn(FoeRoot, at, hop);
            lens.Taken = () => TakeLens(which);
        }

        void TakeLens(int which)
        {
            if (HasLens(which)) return;
            Mark("lens" + which);
            int n = LensCount;
            PatchworkHUD.ShowGet(LensNames[which], n >= 3 ? "All three lenses of the Night Glass!" : n + " of 3 lenses of the Night Glass", LensBlue, 0);
            Sfx.Play(SfxId.PrizeFanfare, 0.6f);
            Haptics.Pulse(0.4f, 0.5f, 0.3f);
            RefreshLamps();
            // The first lens Patch wins, somebody notices.
            if (n == 1 && which == 0 && visitor == null)
            {
                visitor = UnpickerVisit.Show(FoeRoot, new Vector3(80f, 18.5f, -44f), 19f, 16f, 13f);
                if (visitor != null)
                {
                    Narrator.Say("The Object Glass, still warm from the Captain's chest. And at the far end of the hall, up on the toy castle, something very tall unfolded itself, looked at Patch with all its eyes, and folded away again.");
                    return;
                }
            }
            if (n >= 3) Narrator.Say("Three lenses, safe in Patch's satchel, clinking. Now for the top of the house, and Stella, and the time: which was getting on.");
            else if (which == 0) Narrator.Say("The Object Glass: the big one, the one that gathers the light. It was still warm from the Captain's chest.");
            else if (which == 1) Narrator.Say("The Field Lens, washed clean. All down the hall the tanks were turning blue again, one after another, like lamps being lit.");
            else Narrator.Say("The Eye Lens: the little one you put your eye to. The helmet lay on its side, quite empty, and only a helmet.");
        }

        // ------------------------------------------------------------------ The Hall of Arms

        public void ArmsBeaten(Vector3 at)
        {
            Mark("arms");
            DropLens(0, new Vector3(at.x, 1.3f, at.z), true);
        }

        // ------------------------------------------------------------------ The tanks

        void ClotBeaten(int which, Vector3 at)
        {
            GameState.SetHintSeen(ClotKey(which));
            int n = ClotsCleared;
            var game = PatchworkGame.Instance;
            if (n >= 4)
            {
                DropLens(1, at, true);
                if (game != null) game.ShowToast("All four tanks run clear! Something bright is turning in the water...");
            }
            else
            {
                if (game != null) game.ShowToast(TankNames[which] + " runs clear again!  (" + n + " of 4)");
                if (n == 1) Narrator.Say("The knot came apart all at once, and the colour came back into the water the way a blush comes into a face.");
            }
        }

        void ClotHeld()
        {
            if (toldHeld) return;
            toldHeld = true;
            var game = PatchworkGame.Instance;
            if (game != null) game.ShowToast("It's held fast! Follow its three ropes and break the anchor knots first.");
        }

        // ------------------------------------------------------------------ The night book

        void TakePage(int which)
        {
            if (Seen("page" + which)) return;
            Mark("page" + which);
            int n = PagesFound;
            PatchworkHUD.ShowGet("Wick's Night Book", "Page " + n + " of " + Pages, new Color(0.95f, 0.85f, 0.55f), 0);
            Sfx.Play(SfxId.Checkpoint, 0.35f);
            Narrator.Say(PageText[which]);
            if (n >= Pages)
            {
                ScoreKeeper.Bonus(3000, PatchworkGame.Instance != null && PatchworkGame.Instance.Player != null ? PatchworkGame.Instance.Player.Motor.Center : Vector3.zero);
                var game = PatchworkGame.Instance;
                if (game != null) game.ShowToast("The whole night book! Wick will want to see this.");
            }
        }

        void RefreshLamps()
        {
            if (LampGlows == null) return;
            for (int i = 0; i < LampGlows.Length; i++)
                if (LampGlows[i] != null) LampGlows[i].gameObject.SetActive(HasLens(i));
        }

        // ------------------------------------------------------------------ The Helmet Crab

        void PlaceCrab(bool escorted)
        {
            int stage = CrabStage;
            Vector3 at = CrabSpots[stage];
            at.y = Monsters.Ground(at + Vector3.up * 0.5f);
            Crab.Guarded = false;
            escorts.Clear();
            Crab.Appear(at, stage >= 2);
            if (!escorted) return;
            // It doesn't travel alone any more: chessmen, out of their case.
            int n = stage >= 2 ? 4 : 3;
            for (int i = 0; i < n; i++)
            {
                float a = i * Mathf.PI * 2f / n + 0.6f;
                Vector3 p = at + new Vector3(Mathf.Cos(a) * 3.2f, 0f, Mathf.Sin(a) * 3.2f);
                float g = Monsters.Ground(p + Vector3.up * 0.5f);
                if (Mathf.Abs(g - at.y) > 1f || Physics.CheckSphere(new Vector3(p.x, g + 0.9f, p.z), 0.5f, ~(1 << 2), QueryTriggerInteraction.Ignore)) continue;
                escorts.Add(Chessling.Create(FoeRoot, new Vector3(p.x, g, p.z), i % 2 == 1));
            }
            // In the Kelp Galleries it sits tight inside its helmet till the chessmen are gone.
            Crab.Guarded = stage == 1 && escorts.Count > 0;
        }

        void CrabFled()
        {
            int stage = CrabStage;
            if (stage >= 2) return;
            Mark(stage == 0 ? "crab1" : "crab2");
            var game = PatchworkGame.Instance;
            if (game != null) game.ShowToast("The Helmet Crab slipped through a seam! It's making for " + CrabPlaces[stage + 1] + ".");
            if (stage == 0) Narrator.Say("A diving helmet, on legs, with something bright in its claw. It looked at Patch, and Patch looked at it, and then there was only a loose thread where it had been.");
            else Narrator.Say("Gone again. But it was running out of museum.");
            PlaceCrab(true);
        }

        void CrabFell(Vector3 at)
        {
            Mark("crab");
            DropLens(2, new Vector3(at.x, at.y + 0.4f, at.z), true);
        }

        // ------------------------------------------------------------------ The Observatory

        void StellaTalked()
        {
            if (LensCount < 3 || Fitted) return;
            Mark("fitted");
            PatchworkHUD.ShowGet("The Night Glass", "Stella has fitted all three lenses.", LensBlue, 0);
            Sfx.Play(SfxId.Lever, 0.5f);
            if (visitor == null) visitor = UnpickerVisit.Show(FoeRoot, ObservatoryCentre + new Vector3(-11f, 7f, -11f), 12f, 600f, 7f);
            Narrator.Say(visitor != null
                ? "Click, click, click: three lenses home. And on the far side of the dome a tall shape with a thimble for a hat lifted one long arm, and pointed. Out of the dark came the moths."
                : "Click, click, click: three lenses home. And the moment the Night Glass could see again, everything grey in the museum knew it, and came.");
            StartDefence(true);
        }

        void StartDefence(bool loud)
        {
            defending = true;
            moths.Clear();
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3f + 0.4f;
                Vector3 p = ObservatoryCentre + new Vector3(Mathf.Cos(a) * 9f, 0f, Mathf.Sin(a) * 9f);
                p.y = Monsters.Ground(p + Vector3.up * 2f);
                moths.Add(StaticMoth.Create(FoeRoot, p));
            }
            if (loud)
            {
                var game = PatchworkGame.Instance;
                if (game != null) game.ShowToast("Moths! Keep them off the telescope.");
            }
        }

        // The telescope's "Look" spot asks this first.
        public bool TelescopeReady()
        {
            if (Defended || GameState.IsLevelComplete(Level2Builder.SceneName)) return true;
            var game = PatchworkGame.Instance;
            if (game == null) return false;
            if (!Fitted)
            {
                int n = LensCount;
                game.ShowToast(n >= 3 ? "The telescope is blind without its lenses. Stella knows how they go in." :
                    "Nothing but grey. The Night Glass is missing " + (3 - n) + " of its three lenses.");
                Sfx.Play(SfxId.TransformFail, 0.4f);
                return false;
            }
            game.ShowToast("Not with moths all over it!");
            return false;
        }

        void Update()
        {
            tick -= Time.deltaTime;
            if (tick > 0f) return;
            tick = 0.3f;
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) return;

            if (defending)
            {
                bool any = false;
                foreach (var m in moths) if (m != null && m.gameObject.activeSelf) any = true;
                if (!any)
                {
                    defending = false;
                    Mark("defended");
                    game.ShowToast("The Night Glass is ready. Go and look!");
                    Sfx.Play(SfxId.PrizeFanfare, 0.5f);
                    if (visitor != null)
                    {
                        visitor.Leave();
                        Narrator.Say("The last moth went out like a candle. The tall shape took off its thimble, which may have been a bow, and stepped backward into a seam. Somewhere below, a clock began to strike. Midnight.");
                    }
                    else Narrator.Say("The last moth went out like a candle. Somewhere below, a clock began to strike. Midnight; and the telescope was pointing the right way.");
                }
            }

            // Once its chessmen are gone, the crab in the Kelp Galleries comes out of its helmet.
            if (Crab != null && Crab.Guarded)
            {
                bool any = false;
                foreach (var c in escorts) if (c != null && !c.IsBeaten) any = true;
                if (!any)
                {
                    Crab.Guarded = false;
                    game.ShowToast("No chessmen left to hide behind. The helmet's lifting...");
                }
            }

            // The first sight of a grey tank.
            if (!Seen("say_tanks") && ClotsCleared < 4)
            {
                Vector3 pp = game.Player.transform.position;
                for (int i = 0; i < 4; i++)
                {
                    if (Clots == null || Clots[i] == null || Clots[i].IsBeaten) continue;
                    Vector3 d = ClotSpots[i] - pp;
                    if (new Vector2(d.x, d.z).magnitude > 16f) continue;
                    Mark("say_tanks");
                    Narrator.Say("The water in the tank had gone the colour of dishwater, and in the middle of it hung a knot of grey thread as big as a barrel, with one eye, which opened.");
                    break;
                }
            }
        }

        // ------------------------------------------------------------------ What everyone says

        void WrapNpc(string name, System.Func<string[]> mine, System.Action talked)
        {
            ToyNpc npc = null;
            foreach (var n in FindObjectsByType<ToyNpc>(FindObjectsInactive.Exclude))
                if (n != null && n.Name == name) { npc = n; break; }
            if (npc == null) return;
            var old = npc.Lines;
            string heard = null;
            npc.Lines = () =>
            {
                var q = mine();
                // (Each new thing is said once; after that they go back to their own chatter.)
                if (q == null || q.Length == 0 || heard == q[0]) return old != null ? old() : new[] { "..." };
                heard = q[0];
                return q;
            };
            if (talked == null) return;
            var was = npc.TalkEnded;
            npc.TalkEnded = () => { if (was != null) was(); talked(); };
        }

        public void WickTalked()
        {
            if (Met) return;
            Mark("met");
            Narrator.Card("Chapter Two", "The Night Glass");
            Sfx.Play(SfxId.Checkpoint, 0.4f);
        }

        public string[] WickLines()
        {
            if (!Met) return new[]
            {
                "Halt! Who goes... oh. You're not grey. Thank the stars. Wick, night watchman. Forty years and I never lost so much as a teaspoon, until tonight.",
                "You know what we do here at midnight? We point the Night Glass, the great telescope, up through the dome, and we find the Great Whale as she goes over.",
                "She only sings if somebody sees her. And her song is what keeps the colour in this place. Every tank, every tapestry. Every night, for longer than the building's stood.",
                "At dusk the Fray came up through the floor and took the Night Glass's three lenses. Unscrewed them, neat as you like. If midnight goes by and nobody's looking, she goes over unsung, and by morning...",
                "Well. Look at the suits of armour. Look at the tanks. It's started.",
                "One lens went into the Hall of Arms, in the Sunken Ruins: I heard the clanking. One's somewhere in the keepers' tanks: ask Marlow, through the door behind the big aquarium. And one...",
                "One I saw carried off by a diving helmet. On legs. I am not a fanciful man.",
                "You've a good arm on you. Three quick hits, and make the third one count: it's the only thing that shifts a shield. Bring the lenses up to Stella in the Observatory. I'll hold the door."
            };
            int n = LensCount;
            if (Defended) return new[] { "It's ready? Then what are you doing down here talking to me? Go and look, before she's gone over!" };
            if (n >= 3) return new[] { "All three! Up to Stella with them, quick as you can. The lift's at the far end of the Sunken Ruins." };
            var lines = new List<string>();
            int pages = PagesFound;
            if (pages >= Pages && !Seen("pages_told"))
            {
                Mark("pages_told");
                return new[]
                {
                    "My night book! Every page! I thought the draught had them for good.",
                    "Read me the last one. ...A thimble for a hat. Yes. I remember now. I'd made myself forget.",
                    "The toys down in Toybox Town have a name for him. The Unpicker. The Fray goes where he points, and the colour it takes ends up in his jars.",
                    "He bowed to me. Forty years a watchman and the thief bowed to me. You get those lenses back, and you look through that glass, and you let him watch you do it."
                };
            }
            if (pages > 0 && pages < Pages) lines.Add("You've found " + pages + " of the six pages out of my night book. Keep an eye out for the rest; I wrote down everything I saw, and I can't for the life of me remember the end of it.");
            lines.Add(n == 0 ? "No lenses yet? No shame in it. It's a big museum and they've a head start." : n == 1 ? "One lens back. Two to find. You're doing better than I did." : "Two! One more and she can see again.");
            if (!HasLens(0)) lines.Add(ArmsWon ? "The Hall of Arms has gone quiet. Whatever the Captain dropped is still lying in there." : "The Hall of Arms is through the Sunken Ruins, south side. Mind the door. I don't like the look of that portcullis.");
            if (!HasLens(1)) lines.Add("The tanks are behind the big aquarium: the door marked KEEPERS ONLY. You'll want to be Water to go in. Marlow's count is " + ClotsCleared + " of 4 clear.");
            if (!HasLens(2)) lines.Add("That helmet with legs was last seen in " + CrabPlaces[CrabStage] + ". Nothing dents it but a really good wallop, the third of three. Then it's on its back, and it's yours.");
            return lines.ToArray();
        }

        string[] MarlowLines()
        {
            if (HasLens(1)) return null;
            int n = ClotsCleared;
            if (n >= 4) return new[] { "Clear! All four! And there's something shining in the water where the last knot was. Go on, it's yours." };
            if (n == 0) return new[]
            {
                "You've seen them? The tanks? Forty years of my life in those tanks, and tonight they look like washing-up water.",
                "It's the Fray. There's a great grey knot tied round the heart of each one, drinking the colour out. Coral Nursery, Lantern Deep, Frozen Sea, Mangrove Shallows. All four.",
                "I can't go in. I'm a bird; I'd never get back out. But you can be Water, can't you? Go in from the catwalk and dive.",
                "Hit the knot. It'll swell up before it lets off a shock: you'll see it. Back away, then go in again. And mind the jellies. They used to be such gentle things.",
                "And look: one of those knots is wrapped round something that glints. I saw it. I'd bet my whistle it's off the telescope."
            };
            return new[] { n + " of four tanks clear. I can see the colour coming back from here. Keep going: the one with the glint in it is still knotted up." };
        }

        string[] CastellanLines()
        {
            if (HasLens(0)) return new[] { "They're back on their plinths. A little dented. I shall be up all night with the polish, and I shan't mind one bit. Thank you." };
            if (ArmsWon) return new[] { "It's over? It's over. He dropped something, the Captain. Round and blue. It's lying in the aisle; I didn't dare touch it." };
            return new[]
            {
                "Stay back from the aisle! They've been turning their heads all evening. Not the way they're meant to. All the way round.",
                "There's grey thread coming out of the gauntlets. And the Knight by the door has something on his chest that he didn't have this morning. Round. Blue. Glass.",
                "If you go down there the portcullis will drop. It always did have a mind of its own. Three quick blows, mind, and the third one's the one that moves a shield."
            };
        }

        string[] StellaLines()
        {
            if (Defended) return new[] { "It's ready. Every lens where it belongs. Go on, up the steps. You earned the first look." };
            if (Fitted) return new[] { "Moths! Get them off! Shoo! They'll have the lenses out again!" };
            int n = LensCount;
            if (n >= 3) return new[]
            {
                "You have them? All three? Oh, hold them up. Hold them up to the light.",
                "The Object Glass at the top. The Field Lens in the middle. The Eye Lens where your eye goes. There. There. And... there.",
                "Listen. Do you hear that? That's a telescope that can see."
            };
            if (n == 0) return new[]
            {
                "Don't look through it. There's nothing to see. They took the lenses, all three, while I was winding the dome.",
                "It's a tube without them. A very expensive tube, pointing at a whale it can't see. And it's nearly midnight.",
                "The watchman's downstairs in the Atrium, if you want to help. Wick. He saw where they went."
            };
            return new[] { n == 1 ? "One lens! One's not enough to see by, but it's one more than I had. Two to go." : "Two! Just one more. Hurry: I can feel her getting close. The whole dome hums when she's close." };
        }

        string[] CrabGossip(int place, string who)
        {
            if (HasLens(2) || CrabBeaten) return null;
            int stage = CrabStage;
            if (place == 0)
            {
                if (stage == 0) return new[] { "Did you see it? A helmet! With legs! It's been sitting in the middle of the Atrium all evening, looking at people. It runs if you get close." };
                return new[] { "The helmet thing went that way. Through the floor. I'm not making it up." };
            }
            if (place == 1)
            {
                if (stage == 1) return new[] { "There's a brass helmet in the Kelp Galleries and it's got the chess pieces with it. They hop. I don't like the hopping." };
                if (stage == 2) return new[] { "It went through a seam in the carpet. Toward the Sunken Ruins, I think. It can't have many places left." };
                return null;
            }
            if (stage == 2) return new[]
            {
                "Fascinating specimen. A standard diving helmet, brass, about 1900, and six legs, which is not standard.",
                "It's stopped running. I think it's run out of museum. When it rears up, it means to charge. Step aside, let it tire itself, then hit it properly."
            };
            return null;
        }

        // What to do next, for the panel at the top left of the screen.
        public string Objective()
        {
            var game = PatchworkGame.Instance;
            if (Defended) return "Look through the Night Glass.";
            if (Fitted) return "Drive the moths off the telescope!";
            if (LensCount >= 3)
            {
                bool lift = game != null && game.Level != null && game.Level.HeartstringCount() >= game.Level.HeartstringsNeeded;
                return lift ? "Take the lenses up to Stella in the Observatory." : "Take the lenses to Stella. (The lift still needs Heartstrings.)";
            }
            if (!Met && LensCount == 0) return "Find Wick, the night watchman, in the Atrium.";
            if (Fight != null && Fight.Busy) return "Beat the Hollow Guard!  (Wave " + Fight.WaveNumber + " of 3)";
            if (!HasLens(0)) return ArmsWon ? "Pick up the Object Glass in the Hall of Arms." : "Hall of Arms: something is clanking in there.";
            if (!HasLens(1)) return ClotsCleared >= 4 ? "Pick up the Field Lens in the tank." : "Keepers' tanks: cut each clot's three anchors, then break it.  (" + ClotsCleared + " of 4)";
            if (Crab != null && Crab.Guarded) return "Kelp Galleries: pop the chessmen guarding the Helmet Crab.";
            return CrabBeaten ? "Pick up the Eye Lens." : "Catch the Helmet Crab: last seen in " + CrabPlaces[CrabStage] + ".";
        }
    }

    // The fight in the Hall of Arms. The first time Patch walks down the aisle the portcullis
    // drops behind him, and the armours come down off their plinths in three waves; the last
    // is the Knight by the door, the Hollow Captain, with the Object Glass on his chest.
    // Beaten armours go back on their plinths, empty. If Patch unravels, the wave he was on
    // starts again (not the whole fight).
    public class HallOfArmsFight : MonoBehaviour
    {
        public Level2Quest Quest;
        public Transform Gate;
        public Transform[] Suits;        // the display armours on the plinths
        public HollowGuard[] Guards;     // the same eight, walking
        public Vector3[] Landings;

        enum State { Idle, Closing, Fighting, Between, Won }
        static readonly int[][] Waves = { new[] { 2, 1, 6 }, new[] { 3, 7, 5 }, new[] { 4, 0 } };

        State state;
        int wave;
        float timer;
        bool fell;
        Vector3 lastFall = new Vector3(80f, 0f, -12f);

        public bool Busy { get { return state == State.Closing || state == State.Fighting || state == State.Between; } }
        public int WaveNumber { get { return wave + 1; } }

        void Start()
        {
            for (int i = 0; i < Guards.Length; i++)
            {
                Guards[i].Fence = Fence;
                Guards[i].Beaten = OnBeaten;
            }
            if (Quest != null && Quest.ArmsWon) state = State.Won;
        }

        // Keeps the guards in the open middle of the hall: between the rows of plinths, and
        // round the horseman.
        static Vector3 Fence(Vector3 p)
        {
            p.x = Mathf.Clamp(p.x, 70.8f, 89.2f);
            p.z = Mathf.Clamp(p.z, -38f, -2.5f);
            Vector3 d = p - new Vector3(80f, p.y, -27f);
            d.y = 0f;
            float m = d.magnitude;
            if (m < 3.4f) p += (m > 0.01f ? d / m : Vector3.forward) * (3.4f - m);
            return p;
        }

        static bool Inside(Vector3 p, float margin)
        {
            return p.x > 60f - margin && p.x < 100f + margin && p.z > -52f - margin && p.z < 4f + margin && p.y > -2f && p.y < 19f;
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null || state == State.Won) return;
            float dt = Time.deltaTime;
            Vector3 pp = game.Player.transform.position;

            switch (state)
            {
                case State.Idle:
                    if (game.Mode == UIMode.Gameplay && Inside(pp, 0f) && pp.z < 0.5f && !game.Unravelling) Begin(game);
                    break;

                case State.Closing:
                    // Down comes the portcullis.
                    timer += dt;
                    float k = Mathf.Clamp01(timer / 0.4f);
                    Gate.position = new Vector3(80f, Mathf.Lerp(8.2f, 0f, k * k), 5f);
                    if (timer >= 0.4f && timer - dt < 0.4f)
                    {
                        Sfx.Play(SfxId.PlatformThunk, 0.8f);
                        if (game.CameraRig != null) game.CameraRig.Kick(0.4f);
                        FxBurst.Spawn(FxBurst.Kind.Poof, new Vector3(80f, 0.3f, 4.4f), new Color(0.8f, 0.78f, 0.74f));
                    }
                    if (timer > 1.7f) StartWave();
                    break;

                case State.Fighting:
                    if (Restarting(game, pp)) break;
                    bool left = false;
                    foreach (int i in Waves[wave]) if (!Guards[i].IsBeaten) left = true;
                    if (left) break;
                    if (wave >= Waves.Length - 1) { Win(game); break; }
                    state = State.Between;
                    timer = 0f;
                    if (game.Hearts < PatchworkGame.MaxHearts) HeartPickup.Spawn(new Vector3(80f, 1f, -12f), true);
                    game.ShowToast(wave == 0 ? "Listen... more of them are stirring." : "The Knight by the door has turned his head.");
                    break;

                case State.Between:
                    if (Restarting(game, pp)) break;
                    timer += dt;
                    if (timer > 2.4f) { wave++; StartWave(); }
                    break;
            }
        }

        void Begin(PatchworkGame game)
        {
            state = State.Closing;
            timer = 0f;
            fell = false;
            Gate.gameObject.SetActive(true);
            Gate.position = new Vector3(80f, 8.2f, 5f);
            Sfx.Play(SfxId.GateRumble, 0.6f);
            game.ShowToast(wave == 0 ? "The portcullis has dropped! Beat the Hollow Guard to raise it." : "The portcullis drops again...");
            if (wave == 0) Narrator.Say("The portcullis came down behind Patch like a full stop. And down the hall, one after another, the helmets turned.");
        }

        void StartWave()
        {
            state = State.Fighting;
            foreach (int i in Waves[wave])
            {
                if (Suits[i] != null) Suits[i].gameObject.SetActive(false);
                Guards[i].Wake(Landings[i]);
            }
            if (wave == 2) Narrator.Say("Last of all the Knight stepped down: the Hollow Captain, with a round blue glass shining on his chest where a heart would go.");
        }

        // Patch unravelled (or got out somehow): the armours go back, and the wave starts over
        // when he comes back down the aisle.
        bool Restarting(PatchworkGame game, Vector3 pp)
        {
            if (game.Unravelling) { fell = true; return true; }
            if (!fell && Inside(pp, 3f)) return false;
            fell = false;
            foreach (int i in Waves[wave])
            {
                Guards[i].Sleep();
                if (Suits[i] != null) Suits[i].gameObject.SetActive(true);
            }
            Gate.gameObject.SetActive(false);
            state = State.Idle;
            return true;
        }

        void OnBeaten(MuseumFoe foe)
        {
            lastFall = foe.Feet;
            // Its armour is back where it belongs: empty, and only armour.
            for (int i = 0; i < Guards.Length; i++)
            {
                if (Guards[i] != foe || Suits[i] == null) continue;
                Suits[i].gameObject.SetActive(true);
                FxBurst.Spawn(FxBurst.Kind.Poof, Suits[i].position + Vector3.up * 1.4f, new Color(0.85f, 0.85f, 0.9f));
            }
        }

        void Win(PatchworkGame game)
        {
            state = State.Won;
            Gate.gameObject.SetActive(false);
            Sfx.Play(SfxId.GateRumble, 0.5f);
            Sfx.Play(SfxId.PrizeFanfare, 0.5f);
            game.ShowToast("The Hollow Guard is beaten! The portcullis is up.");
            Narrator.Say("Then it was quiet. Eight suits of armour stood on eight plinths, looking as if they had never moved, and would thank you not to mention it.");
            if (Quest != null) Quest.ArmsBeaten(lastFall);
        }
    }
}

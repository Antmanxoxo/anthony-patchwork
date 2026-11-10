using UnityEngine;

namespace Patchwork
{
    // The moving parts of Toybox Town's three band stories and its wedding presents.
    // (Where everything is put, and what it looks like, is in Level1Quests.)

    // ------------------------------------------------------------------ The lute: loot!
    //
    // Captain Buttonbeard has found the lute string and won't give it up: he's sure "lute" means
    // "loot". He'll swap it for his real treasure, buried where his toy boat is wrecked in the
    // sandpit, right across the map. Dig it up (in any order: you can find it before you meet
    // him), go back, and the string's yours.
    public class PirateQuest : MonoBehaviour
    {
        public const string MetKey = "L1_pirate_met", TreasureKey = "L1_pirate_treasure", PaidKey = "L1_pirate_paid";

        public ToyNpc Pirate;
        public BandPiece Piece;
        public DigSpot Dig;
        public Transform Chest, Lid, Coins;
        public LevelInfo Info;
        public Vector3 TreasureAt;

        MapPin pin;
        float rise = -1f;
        Vector3 chestRest;

        public static bool Met { get { return GameState.IsHintSeen(MetKey); } }
        public static bool HasTreasure { get { return GameState.IsHintSeen(TreasureKey); } }
        public static bool Paid { get { return GameState.IsHintSeen(PaidKey); } }

        void Start()
        {
            if (Chest != null) chestRest = Chest.localPosition;
            if (Piece != null && !Piece.Taken) Piece.SetLocked(!Paid);
            if (HasTreasure)
            {
                if (Dig != null) Dig.MarkDug();
                ShowChest(1f);
            }
            else if (Chest != null) Chest.gameObject.SetActive(false);
            if (Met && !HasTreasure) AddPin();
        }

        void AddPin()
        {
            if (pin != null || Info == null) return;
            pin = new MapPin("X Marks the Spot", TreasureAt);
            pin.Tint = new Color(0.95f, 0.7f, 0.2f);
            Info.MapPins.Add(pin);
        }

        void RemovePin()
        {
            if (pin == null || Info == null) return;
            Info.MapPins.Remove(pin);
            pin = null;
        }

        public string[] Lines()
        {
            bool done = Piece == null || Piece.Taken;
            if (done)
                return new[] { "Arr, that Pip plays a fine tune on me old loot. Lute. Whichever.", "I'll be at the wedding, mind. Pirates love a cake." };
            if (Paid)
                return new[] { "The string's right there, matey. A deal's a deal.", "Off ye go. And if ye find any more treasure, ye know where I be." };
            if (HasTreasure)
                return new[] { "Shiver me stitches... is that me TREASURE? Ye dug it up!", "Every last button, all present and correct. Ye're a fine wee swab.",
                               "A deal's a deal. Here: one coil of loot. Lute. Whatever ye call it." };
            if (Met)
                return new[] { "No treasure yet? Me ship's wrecked in a sea of sand, out east on the way to the Sky Hoops.", "Dig where the X be stitched. Then the string's yours." };
            return new[] { "Arr! Captain Buttonbeard, terror of the Inkwell Canal! Come for me LOOT, have ye?",
                           "...A LUTE string? Never heard of such a thing. This shiny coil be LOOT, and the only loot I've got since me ship ran aground.",
                           "Tell ye what. Me real treasure's buried where the ship went down: wrecked in a sea of sand, out east on the way to the Sky Hoops.",
                           "Dig it up and bring it here, and ye can have the coil. Look for the X!" };
        }

        public void TalkEnded()
        {
            if (Piece == null || Piece.Taken || Paid) return;
            var game = PatchworkGame.Instance;
            if (HasTreasure)
            {
                GameState.SetHintSeen(PaidKey);
                GameState.Save();
                Piece.SetLocked(false);
                Sfx.Play(SfxId.Checkpoint, 0.6f);
                if (Pirate != null) Emote.Show(Pirate.HeadPosition + Vector3.up * 0.4f, Emote.Kind.Heart);
                if (game != null) game.ShowToast("Captain Buttonbeard hands over the lute string. Grab it!");
                return;
            }
            if (!Met)
            {
                GameState.SetHintSeen(MetKey);
                GameState.Save();
                AddPin();
                if (game != null) game.ShowToast("New on the map: X Marks the Spot.");
                Narrator.Say("Loot. Lute. One can see how it happened. A shipwreck in a sea of sand, he said. Now where would a toy keep one of those?");
            }
        }

        // Called by the dig spot on the X.
        public void Dug()
        {
            if (HasTreasure) return;
            GameState.SetHintSeen(TreasureKey);
            GameState.Save();
            RemovePin();
            rise = 0f;
            if (Chest != null) Chest.gameObject.SetActive(true);
            ShowChest(0f);
            Sfx.Play(SfxId.GateRumble, 0.4f);
            var game = PatchworkGame.Instance;
            if (game != null)
                game.ShowToast(Met ? "Captain Buttonbeard's treasure! Take the news back to him on Spindle Hill."
                                   : "A buried treasure chest! Somebody must be missing this...");
            if (!Met) Narrator.Say("Buried treasure, and nobody to claim it. One hears there's a pirate moping on top of Spindle Hill.");
        }

        // k: 0 = still under the sand, 1 = up, with the lid thrown open.
        void ShowChest(float k)
        {
            if (Chest == null) return;
            float up = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(k / 0.7f));
            Chest.localPosition = chestRest + Vector3.down * (1f - up) * 0.95f;
            float open = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((k - 0.7f) / 0.3f));
            if (Lid != null) Lid.localRotation = Quaternion.Euler(-105f * open, 0f, 0f);
            if (Coins != null) Coins.gameObject.SetActive(k > 0.75f);
        }

        void Update()
        {
            if (rise < 0f) return;
            float before = rise;
            rise += Time.deltaTime / 1.5f;
            ShowChest(Mathf.Min(1f, rise));
            if (Chest != null && rise < 0.7f && Random.value < Time.deltaTime * 8f)
                FxBurst.Spawn(FxBurst.Kind.Scraps, Chest.position + Vector3.up * 0.2f, new Color(0.96f, 0.86f, 0.62f));
            if (before < 0.72f && rise >= 0.72f && Chest != null)
            {
                // The lid flies open: gold everywhere.
                Vector3 p = Chest.position + Vector3.up * 0.8f;
                FxBurst.Spawn(FxBurst.Kind.Sparkle, p, new Color(1f, 0.85f, 0.35f));
                FxBurst.Spawn(FxBurst.Kind.Pompoms, p, new Color(1f, 0.8f, 0.3f));
                ScoreBubbles.BurstAt(p, 24);
                ScoreKeeper.Bonus(500, p);
                Sfx.Play(SfxId.Complete, 0.6f);
                Haptics.Pulse(0.5f, 0.4f, 0.3f);
            }
            if (rise >= 1f) rise = -1f;
        }
    }

    // ------------------------------------------------------------------ The piper: scared of everything
    //
    // Wisp is stuck on the weathervane, but he's so jumpy that when Patch gets close he squeaks
    // and flaps off to the next cloud up. Follow him from cloud to cloud until there's no more
    // sky to run to: then he finally sees who it is, and he'll come home.
    public class ScaredPiper : MonoBehaviour
    {
        public ToyNpc Bird;
        public BandPiece Piece;
        public Vector3[] Perches;     // where he sits, lowest first (the last is where he gives up)

        static readonly string[] Squeaks =
        {
            "Wisp: \"EEP! A grey thing! Don't unravel me!\"  He's bolted for the clouds.",
            "Wisp: \"It's FOLLOWING me! Higher! Higher!\"",
            "Wisp: \"Oh dear, oh dear, oh dear, oh dear...\"",
            "Wisp: \"Up, up, up! It can't possibly climb this high!\"",
            "Wisp: \"Go away, Fray! I've nothing worth fraying!\"",
        };

        int at;
        float fly = -1f, flyTime = 1.5f, calm;
        Vector3 from, to;
        bool cornered;

        void Start()
        {
            if (Piece != null && !Piece.Taken) Piece.SetLocked(true);
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null || Bird == null || Perches == null || Perches.Length == 0) return;
            if (!Bird.gameObject.activeInHierarchy || (Piece != null && Piece.Taken)) return;
            float dt = Time.deltaTime;

            if (fly >= 0f)
            {
                fly += dt / flyTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(fly));
                Vector3 p = Vector3.Lerp(from, to, k);
                // A panicky, flappy arc.
                p.y += Mathf.Sin(k * Mathf.PI) * 3.2f + Mathf.Sin(Time.time * 24f) * 0.12f * Mathf.Sin(k * Mathf.PI);
                Bird.transform.position = p;
                if (fly >= 1f)
                {
                    fly = -1f;
                    Bird.transform.position = to;
                    Landed(game);
                }
                return;
            }
            if (cornered || game.Mode != UIMode.Gameplay) return;
            calm -= dt;
            if (calm > 0f) return;
            Vector3 d = game.Player.Motor.Center - (Bird.transform.position + Vector3.up * 0.8f);
            if (d.x * d.x + d.z * d.z > 5.2f * 5.2f || Mathf.Abs(d.y) > 4f) return;
            Flee(game);
        }

        void Flee(PatchworkGame game)
        {
            if (at >= Perches.Length - 1) return;
            from = Bird.transform.position;
            at++;
            to = Perches[at];
            flyTime = Mathf.Clamp((to - from).magnitude / 9f, 1.2f, 2.2f);
            fly = 0f;
            Emote.Show(Bird.HeadPosition + Vector3.up * 0.4f, Emote.Kind.Surprise);
            FxBurst.Spawn(FxBurst.Kind.Puff, from + Vector3.up, new Color(0.7f, 0.85f, 1f));
            Sfx.PlayPitched(SfxId.Squeak, 0.45f, 1.5f);
            Sfx.Play(SfxId.Gust, 0.4f);
            game.ShowToast(Squeaks[Mathf.Min(at - 1, Squeaks.Length - 1)]);
            if (at == 1) Narrator.Say("Ah. Wisp, it seems, is afraid of heights, of the Fray, and at present of you. After him. Gently.");
        }

        void Landed(PatchworkGame game)
        {
            if (at < Perches.Length - 1) { calm = 0.5f; return; }
            // Nowhere higher to go.
            cornered = true;
            if (Piece != null)
            {
                Piece.transform.position = to + Vector3.up * 1f;
                Piece.SetLocked(false);
            }
            Emote.Show(Bird.HeadPosition + Vector3.up * 0.4f, Emote.Kind.Surprise);
            game.ShowToast("Wisp: \"There's no more sky! I'm doomed!\"  Go to him. Show him it's only you.");
        }
    }

    // ------------------------------------------------------------------ The trumpet: the dye taps
    //
    // The valve dew only gathers in the reservoir while all three dye taps are running, and the
    // Fray shut every one. Two are inside the works; the third is the intake in the canal wall.
    public class DyeTap : PlayerVolume
    {
        public string Key, Dye;
        public Color Tint;
        public Transform Wheel, Stream;
        public DyeWorksQuest Quest;
        public bool On { get; private set; }
        float spin;

        public void TurnOn()
        {
            On = true;
            if (Stream != null) Stream.gameObject.SetActive(true);
        }

        void Update()
        {
            if (Wheel == null) return;
            spin = Mathf.MoveTowards(spin, On ? 200f : 0f, Time.deltaTime * 260f);
            if (spin > 0.5f) Wheel.Rotate(0f, spin * Time.deltaTime, 0f, Space.Self);
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (On) return;
            TurnOn();
            GameState.SetHintSeen(Key);
            GameState.Save();
            spin = 520f;   // a good hard yank
            Sfx.Play(SfxId.Lever, 0.6f);
            Sfx.Play(SfxId.Splash, 0.5f);
            FxBurst.Spawn(FxBurst.Kind.Splash, transform.position, Tint);
            FxBurst.Spawn(FxBurst.Kind.Drips, transform.position + Vector3.up * 0.5f, Tint);
            Haptics.Pulse(0.3f, 0.3f, 0.2f);
            if (Quest != null) Quest.TapOpened(this);
        }
    }

    public class DyeWorksQuest : MonoBehaviour
    {
        public DyeTap[] Taps;
        public BandPiece Piece;
        public Spinner MillWheel;

        public int Count
        {
            get
            {
                int n = 0;
                if (Taps != null) foreach (var t in Taps) if (t != null && t.On) n++;
                return n;
            }
        }

        void Start()
        {
            if (Taps != null)
                foreach (var t in Taps)
                    if (t != null && GameState.IsHintSeen(t.Key)) t.TurnOn();
            Apply();
        }

        void Apply()
        {
            bool all = Taps != null && Count >= Taps.Length;
            if (Piece != null && !Piece.Taken) Piece.SetLocked(!all);
            // The more dye running, the faster the old waterwheel turns.
            if (MillWheel != null) MillWheel.Speed = 30f + 40f * Count;
        }

        public void TapOpened(DyeTap tap)
        {
            Apply();
            var game = PatchworkGame.Instance;
            int n = Count, total = Taps.Length;
            if (n < total)
            {
                if (game != null) game.ShowToast(tap.Dye + " dye is running!  (Taps " + n + "/" + total + ")");
                if (n == 1) Narrator.Say("One tap running. The works gives a hopeful sort of gurgle. Two more, and there'll be dew.");
                return;
            }
            Sfx.Play(SfxId.Checkpoint, 0.6f);
            if (game != null) game.ShowToast(tap.Dye + " dye is running! All three taps are on: the valve dew is gathering in the reservoir.");
            Narrator.Say("Rose, butter and sky, all running at once. And up in the reservoir, drop by drop: valve dew.");
        }
    }

    // ------------------------------------------------------------------ Wedding presents
    //
    // A lost wedding present, hidden somewhere out of the way. Each one found adds something to
    // the wedding (see Level1Director).
    public class WeddingPresent : PlayerVolume
    {
        public int Index;
        Transform visual;
        float t, glint = 2f;
        bool taken;

        public void Setup(int index, Transform visualRoot)
        {
            Index = index;
            visual = visualRoot;
            if (GameState.IsCollected(Level1Director.GiftIds[index]))
            {
                taken = true;
                if (visual != null) visual.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            if (taken || visual == null) return;
            float dt = Time.deltaTime;
            t += dt;
            visual.localRotation = Quaternion.Euler(0f, t * 40f, Mathf.Sin(t * 1.3f) * 6f);
            visual.localPosition = new Vector3(0f, Mathf.Sin(t * 1.8f) * 0.12f, 0f);
            // A glint now and then, so a sharp eye can spot it from a way off.
            glint -= dt;
            if (glint > 0f) return;
            glint = Random.Range(2.5f, 4f);
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null || (game.Player.transform.position - transform.position).sqrMagnitude > 45f * 45f) return;
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position + Vector3.up * 0.5f, new Color(1f, 0.9f, 0.6f));
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (taken) return;
            taken = true;
            GameState.SetCollected(Level1Director.GiftIds[Index]);
            GameState.Save();
            Vector3 p = transform.position;
            ScoreKeeper.Bonus(500, p);
            ScoreBubbles.BurstAt(p, 12);
            Sfx.Play(SfxId.Complete, 0.6f);
            Sfx.Voice(SfxId.VoiceYay, player.Forms.Current, 0.5f);
            Haptics.Pulse(0.5f, 0.4f, 0.25f);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, p, new Color(1f, 0.85f, 0.4f));
            FxBurst.Spawn(FxBurst.Kind.Scraps, p, new Color(1f, 0.6f, 0.75f));
            if (visual != null) visual.gameObject.SetActive(false);
            if (Level1Director.Instance != null) Level1Director.Instance.OnGift(Index);
        }
    }

    // A bunch of balloons tugging at their strings.
    public class BalloonBunch : MonoBehaviour
    {
        public Transform[] Balloons;
        Vector3[] rest;
        float t;

        void Update()
        {
            if (Balloons == null) return;
            if (rest == null)
            {
                rest = new Vector3[Balloons.Length];
                for (int i = 0; i < rest.Length; i++) rest[i] = Balloons[i].localPosition;
                t = Random.value * 10f;
            }
            t += Time.deltaTime;
            for (int i = 0; i < Balloons.Length; i++)
                Balloons[i].localPosition = rest[i] + new Vector3(Mathf.Sin(t * 0.9f + i * 1.7f) * 0.12f, Mathf.Sin(t * 1.3f + i * 2.3f) * 0.08f, Mathf.Cos(t * 0.8f + i * 1.1f) * 0.12f);
        }
    }
}

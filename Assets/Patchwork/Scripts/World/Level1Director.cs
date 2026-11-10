using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Runs Toybox Town's story. The Fray has spoiled the town band's instruments on the morning of
    // Mayor Bramblebutton's wedding. Patch finds the lute's lost string (Spindle Hill), a drop of
    // valve dew for the trumpet (the Dye Works) and the band's best piper (blown up onto the
    // weathervane). Each one brings a new part of the town's music back, and peels the Fray off
    // that corner of town. With the whole band home, the Mayor can be married; the music
    // unravels the Fray's knot on the clock tower, and the town is mended.
    //
    // How grand the wedding is depends on how much of the toybox Patch has explored. Five lost
    // wedding presents are hidden in out-of-the-way corners, and each one adds something of its
    // own to the day (fireworks, confetti cannons, balloons, paper doves, bunting). The more of
    // the town's other secrets he's found (Heartstrings, golden buttons, marbles), the more
    // guests turn up and the thicker the confetti. Presents found after the wedding still
    // arrive in the square, late.
    public class Level1Director : MonoBehaviour
    {
        public static Level1Director Instance { get; private set; }

        public const string LuteId = "L1_band_lute", TrumpetId = "L1_band_trumpet", PiperId = "L1_band_piper";
        public static readonly string[] BandIds = { LuteId, TrumpetId, PiperId };

        public string[] Ids;   // optional Heartstrings
        public readonly Dictionary<string, FrayRegion> Regions = new Dictionary<string, FrayRegion>();
        public FrayRegion SquareRegion;          // mended at the wedding
        public ToyNpc Lute, Trumpet, Piper, Drummer, Mayor, Bride;
        public readonly List<ToyNpc> Crowd = new List<ToyNpc>();
        public Transform TowerKnot;
        public ToyNpc StrandedPiper;             // Wisp up on the weathervane, waiting to be rescued
        public Vector3 ArchCenter;
        public Vector3 MayorSpot, BrideSpot;

        // ----- Wedding presents -----
        public const int Fireworks = 0, Cannons = 1, Balloons = 2, Doves = 3, Bunting = 4;
        public static readonly string[] GiftIds = { "L1_gift_fireworks", "L1_gift_cannons", "L1_gift_balloons", "L1_gift_doves", "L1_gift_bunting" };
        public static readonly string[] GiftNames = { "a box of fireworks", "a crate of confetti cannons", "a bag of balloons", "a basket of paper doves", "the good bunting" };
        public static readonly string[] GiftHints =
        {
            "They say a box of fireworks was left out west, by a soldier's campfire at the edge of the wood.",
            "A crate of confetti cannons was last seen floating off into the clouds, high above the weathervane.",
            "A bag of balloons fell in the canal and drifted off south, down towards the meadow where you landed.",
            "A basket of paper doves tumbled into the ravine. The north end, on the far bank, I heard.",
            "And the good bunting was sent to the farm by mistake. Try the far north-east corner of the toybox.",
        };
        // What each present adds to the square (nothing to show for the fireworks: they go off).
        public readonly GameObject[] GiftScenes = new GameObject[5];
        public readonly List<ToyNpc> ExtraGuests = new List<ToyNpc>();
        public LevelInfo Info;

        public static int GiftCount()
        {
            int n = 0;
            foreach (var id in GiftIds) if (GameState.IsCollected(id)) n++;
            return n;
        }

        // How much of the town's hidden things Patch has found, 0 (none) to 1 (the lot).
        public float Explored()
        {
            int found = GiftCount() + GameState.GoldenButtonCount(), total = GiftIds.Length + Level1Builder.CollectibleIds.Length;
            if (Ids != null) { found += Count; total += Ids.Length; }
            if (Info != null) { found += Info.SecretCount(); total += Info.SecretIds.Count; }
            return total > 0 ? Mathf.Clamp01(found / (float)total) : 0f;
        }

        float weddingT = -1f;
        int stage;
        float fireworkTimer, lateFirework = 8f;
        Vector3 knotScale;
        readonly List<Transform> notes = new List<Transform>();
        float confettiTimer;

        public static bool Has(string id) { return GameState.IsCollected(id); }

        public static int BandCount()
        {
            int n = 0;
            foreach (var id in BandIds) if (GameState.IsCollected(id)) n++;
            return n;
        }

        public int Count
        {
            get
            {
                int n = 0;
                foreach (var id in Ids) if (GameState.IsCollected(id)) n++;
                return n;
            }
        }

        public static void ResetStatics() { Instance = null; }

        void Awake() { Instance = this; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        bool Done { get { return GameState.IsLevelComplete(Level1Builder.SceneName); } }

        public void ApplySavedState()
        {
            if (Lute != null) Lute.SetInstrumentFixed(Has(LuteId));
            if (Trumpet != null) Trumpet.SetInstrumentFixed(Has(TrumpetId));
            if (Piper != null) Piper.gameObject.SetActive(Has(PiperId));
            foreach (var kv in Regions) if (Has(kv.Key)) kv.Value.Mend(true);
            if (TowerKnot != null) knotScale = TowerKnot.localScale;
            if (Done)
            {
                // The wedding's already happened: the town is mended and the band plays on.
                if (SquareRegion != null) SquareRegion.Mend(true);
                if (TowerKnot != null) TowerKnot.gameObject.SetActive(false);
                SetBandPlaying(true);
                if (Mayor != null) Mayor.transform.position = Vector3.Lerp(MayorSpot, BrideSpot, 0.35f);
                if (Bride != null) Bride.transform.position = Vector3.Lerp(BrideSpot, MayorSpot, 0.35f);
            }
            ApplyExtras();
            UpdateMusic();
            MusicPlayer.SnapBand();
        }

        // The wedding's extras: there afterwards if they were there on the day (or came late).
        public void ApplyExtras()
        {
            for (int i = 0; i < GiftScenes.Length; i++)
                if (GiftScenes[i] != null) GiftScenes[i].SetActive(Done && GameState.IsCollected(GiftIds[i]));
            int guests = Done ? GuestsFor(Explored()) : 0;
            for (int i = 0; i < ExtraGuests.Count; i++)
                if (ExtraGuests[i] != null) ExtraGuests[i].gameObject.SetActive(i < guests);
        }

        void SetBandPlaying(bool on)
        {
            if (Drummer != null) Drummer.Playing = on || true;   // the drummer never stopped
            if (Lute != null) Lute.Playing = on && Has(LuteId);
            if (Trumpet != null) Trumpet.Playing = on && Has(TrumpetId);
            if (Piper != null) Piper.Playing = on && Has(PiperId);
        }

        void UpdateMusic()
        {
            MusicPlayer.SetBand(Has(LuteId), Has(TrumpetId), Has(PiperId));
            // Musicians play along once they have their instrument back.
            if (Lute != null) Lute.Playing = Has(LuteId);
            if (Trumpet != null) Trumpet.Playing = Has(TrumpetId);
            if (Piper != null) Piper.Playing = Has(PiperId);
            if (Drummer != null) Drummer.Playing = true;
        }

        public void OnHeartstring(Heartstring h)
        {
            var game = PatchworkGame.Instance;
            if (game != null) game.ShowToast("Heartstring found: " + h.Title + "  (" + Count + "/" + Ids.Length + ")");
        }

        public void OnBandPiece(BandPiece piece)
        {
            var game = PatchworkGame.Instance;
            FrayRegion region;
            if (Regions.TryGetValue(piece.Id, out region)) region.Mend(false);
            string msg;
            switch (piece.Kind)
            {
                case BandPiece.Part.Lute:
                    if (Lute != null) Lute.SetInstrumentFixed(true);
                    msg = "The lute string! Pip can play again.";
                    break;
                case BandPiece.Part.Trumpet:
                    if (Trumpet != null) Trumpet.SetInstrumentFixed(true);
                    msg = "Valve dew! The trumpet's shining again.";
                    break;
                default:
                    if (Piper != null)
                    {
                        Piper.gameObject.SetActive(true);
                        FxBurst.Spawn(FxBurst.Kind.Puff, Piper.transform.position + Vector3.up, Color.white);
                    }
                    msg = "Wisp is safe and flying home to the bandstand!";
                    break;
            }
            UpdateMusic();
            int n = BandCount();
            if (game != null)
            {
                if (n >= BandIds.Length) game.ShowToast(msg + " The whole band is ready! Go and tell the Mayor.");
                if (n >= BandIds.Length && GiftCount() < GiftIds.Length)
                    Narrator.Say("The band is whole again. Though if one fancied a grander wedding, there are lost presents about. Lady Lacewing keeps count.");
                else game.ShowToast(msg + "  (Band " + n + "/" + BandIds.Length + ")");
            }
        }

        // ------------------------------------------------------------------ Wedding presents

        int GuestsFor(float explored) { return Mathf.RoundToInt(ExtraGuests.Count * Mathf.Clamp01(explored * 1.15f)); }

        void ShowGift(int i)
        {
            if (i < 0 || i >= GiftScenes.Length || GiftScenes[i] == null || GiftScenes[i].activeSelf) return;
            GiftScenes[i].SetActive(true);
            FxBurst.Spawn(FxBurst.Kind.Poof, GiftScenes[i].transform.position + Vector3.up * 2f, Color.white);
        }

        public void OnGift(int i)
        {
            var game = PatchworkGame.Instance;
            int n = GiftCount();
            if (Done)
            {
                // Late, but welcome: it goes straight to the square.
                ShowGift(i);
                if (i == Fireworks) lateFirework = 1f;
                if (game != null) game.ShowToast("A late wedding present: " + GiftNames[i] + "! It's gone straight to the square.  (" + n + "/" + GiftIds.Length + ")");
            }
            else if (game != null) game.ShowToast("A lost wedding present: " + GiftNames[i] + "!  (" + n + "/" + GiftIds.Length + ")");
            if (n >= GiftIds.Length) Narrator.Say("That's all five presents. Nobody could say the Bramblebuttons were short-changed now.");
            else if (n == 1) Narrator.Say("A wedding present, a long way from the wedding. I dare say there are more of those about.");
        }

        static readonly Color[] FireworkColours =
        {
            new Color(1f, 0.45f, 0.55f), new Color(1f, 0.85f, 0.35f), new Color(0.45f, 0.8f, 1f), new Color(0.7f, 0.5f, 1f), new Color(0.5f, 1f, 0.65f),
        };

        void Firework(float volume)
        {
            Vector3 p = ArchCenter + new Vector3(Random.Range(-13f, 13f), Random.Range(9f, 15f), Random.Range(2f, 16f));
            Color c = FireworkColours[Random.Range(0, FireworkColours.Length)];
            FxBurst.Spawn(FxBurst.Kind.Sparkle, p, c);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, p + Random.insideUnitSphere * 1.2f, Color.Lerp(c, Color.white, 0.5f));
            FxBurst.Spawn(FxBurst.Kind.Scraps, p, c);
            Sfx.PlayNear(SfxId.Pop, p, volume, 70f);
            Sfx.PlayNear(SfxId.Twinkle, p, volume * 0.6f, 70f);
        }

        // Once the town's married, anyone who found the fireworks gets one now and then over
        // the square, for as long as they're near enough to see it.
        void LateFireworks(float dt)
        {
            if (!Done || !GameState.IsCollected(GiftIds[Fireworks])) return;
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null || game.Mode != UIMode.Gameplay) return;
            Vector3 d = game.Player.transform.position - ArchCenter;
            if (d.x * d.x + d.z * d.z > 45f * 45f) return;
            lateFirework -= dt;
            if (lateFirework > 0f) return;
            lateFirework = Random.Range(7f, 13f);
            Firework(0.35f);
        }

        // ------------------------------------------------------------------ The wedding

        public bool CanStartWedding { get { return BandCount() >= BandIds.Length && weddingT < 0f; } }

        public void StartWedding()
        {
            if (!CanStartWedding) return;
            var game = PatchworkGame.Instance;
            if (game == null) return;
            weddingT = 0f;
            stage = 0;
            fireworkTimer = 0f;
            confettiTimer = 0f;
            // The more of the toybox Patch has explored, the more toys have heard and come.
            int guests = GuestsFor(Explored());
            for (int i = 0; i < ExtraGuests.Count && i < guests; i++)
            {
                var g = ExtraGuests[i];
                if (g == null) continue;
                g.gameObject.SetActive(true);
                g.Playing = true;
                FxBurst.Spawn(FxBurst.Kind.Puff, g.transform.position + Vector3.up, Color.white);
            }
            game.CutsceneCaption = "The Toybox Wedding";
            game.StartCutscene(ArchCenter + new Vector3(0f, 0f, 3f), 15f, 5f, 180f);
            SetBandPlaying(true);
            foreach (var c in Crowd) if (c != null) c.Playing = true;
            Sfx.Play(SfxId.Checkpoint, 0.6f);
        }

        // True the first time the wedding passes a moment, so each thing happens once.
        bool Reached(int bit, float time)
        {
            if (weddingT < time || (stage & (1 << bit)) != 0) return false;
            stage |= 1 << bit;
            return true;
        }

        static bool Gift(int i) { return GameState.IsCollected(GiftIds[i]); }

        void Update()
        {
            if (weddingT < 0f) { LateFireworks(Time.deltaTime); return; }
            float dt = Time.deltaTime;
            weddingT += dt;
            var game = PatchworkGame.Instance;
            float explored = Explored();

            // ----- What the presents add -----
            if (Reached(0, 1.2f) && Gift(Bunting)) { ShowGift(Bunting); Sfx.Play(SfxId.Whoosh, 0.4f); }
            if (Reached(1, 2.6f) && Gift(Balloons)) { ShowGift(Balloons); Sfx.Play(SfxId.Pop, 0.4f); }
            if (Reached(2, 3.6f) && Gift(Cannons)) ShowGift(Cannons);
            if (Gift(Cannons) && (Reached(3, 4.6f) || Reached(4, 6.2f) || Reached(5, 8.4f)))
            {
                // Both cannons go off over the aisle.
                Color[] cc = { LevelKit.Palette.Rose, LevelKit.Palette.Butter, LevelKit.Palette.Sky, LevelKit.Palette.Lavender };
                for (int s = -1; s <= 1; s += 2)
                    for (int i = 0; i < 4; i++)
                        FxBurst.Spawn(i % 2 == 0 ? FxBurst.Kind.Scraps : FxBurst.Kind.Pompoms, ArchCenter + new Vector3(s * (4.2f - i * 0.9f), 2.2f + i * 0.9f, -4.6f + i * 0.5f), cc[(i + (s > 0 ? 1 : 0)) % cc.Length]);
                Sfx.Play(SfxId.Pop, 0.7f);
                Sfx.Play(SfxId.Boing, 0.3f);
            }
            if (Reached(6, 5.4f) && Gift(Doves)) { ShowGift(Doves); Sfx.Play(SfxId.Glide, 0.5f); }
            if (Gift(Fireworks) && weddingT > 5f && weddingT < 13.6f)
            {
                fireworkTimer -= dt;
                if (fireworkTimer <= 0f) { fireworkTimer = Random.Range(0.35f, 0.7f); Firework(0.5f); }
            }

            // The Mayor and his bride meet under the flower arch.
            float walk = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((weddingT - 1f) / 3f));
            if (Mayor != null) Mayor.transform.position = Vector3.Lerp(MayorSpot, Vector3.Lerp(MayorSpot, BrideSpot, 0.35f), walk);
            if (Bride != null) Bride.transform.position = Vector3.Lerp(BrideSpot, Vector3.Lerp(BrideSpot, MayorSpot, 0.35f), walk);

            if (weddingT > 4.5f && weddingT < 4.5f + dt * 1.5f)
            {
                if (game != null) game.CutsceneCaption = "\"I do!\"";
                Sfx.Play(SfxId.Complete, 0.8f);
                FxBurst.Spawn(FxBurst.Kind.Sparkle, ArchCenter + Vector3.up * 3f, new Color(1f, 0.6f, 0.75f));
            }
            // Confetti!
            if (weddingT > 4.5f && weddingT < 11f)
            {
                confettiTimer -= dt;
                if (confettiTimer <= 0f)
                {
                    confettiTimer = Mathf.Lerp(0.35f, 0.1f, explored);
                    Color[] cols = { LevelKit.Palette.Rose, LevelKit.Palette.Butter, LevelKit.Palette.Sky, LevelKit.Palette.Lavender, LevelKit.Palette.Sage };
                    Vector3 p = ArchCenter + new Vector3(Random.Range(-7f, 7f), Random.Range(3f, 6f), Random.Range(-4f, 6f));
                    FxBurst.Spawn(FxBurst.Kind.Scraps, p, cols[Random.Range(0, cols.Length)]);
                }
            }
            // Golden notes rise from the band to the knot on the clock tower.
            if (weddingT > 5.5f && weddingT < 8.5f && TowerKnot != null && Random.value < dt * 8f)
            {
                var n = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(n.GetComponent<Collider>());
                n.transform.position = ArchCenter + new Vector3(Random.Range(-3f, 3f), 2.5f, 6f);
                n.transform.localScale = Vector3.one * 0.35f;
                n.GetComponent<Renderer>().sharedMaterial = PatchworkAssets.Glow(LevelKit.Palette.Gold, 1.2f);
                notes.Add(n.transform);
                if (game != null) game.CutsceneCaption = "The music is unravelling the Fray!";
            }
            for (int i = notes.Count - 1; i >= 0; i--)
            {
                var n = notes[i];
                if (n == null || TowerKnot == null) { notes.RemoveAt(i); continue; }
                n.position = Vector3.MoveTowards(n.position, TowerKnot.position, dt * 14f) + Vector3.right * Mathf.Sin(Time.time * 6f + i) * dt * 2f;
                if ((n.position - TowerKnot.position).sqrMagnitude < 1f) { Destroy(n.gameObject); notes.RemoveAt(i); }
            }
            // The knot comes apart.
            if (TowerKnot != null && TowerKnot.gameObject.activeSelf && weddingT > 8f)
            {
                float k = Mathf.Clamp01((weddingT - 8f) / 1.5f);
                TowerKnot.localScale = knotScale * (1f - k);
                TowerKnot.Rotate(0f, 500f * dt, 0f, Space.World);
                if (k >= 1f)
                {
                    TowerKnot.gameObject.SetActive(false);
                    FxBurst.Spawn(FxBurst.Kind.Threads, TowerKnot.position, SnarlKnot.FrayDark);
                    FxBurst.Spawn(FxBurst.Kind.Sparkle, TowerKnot.position, new Color(1f, 0.85f, 0.5f));
                    Sfx.Play(SfxId.PortalWarp, 0.7f);
                    if (SquareRegion != null) SquareRegion.Mend(false);
                    foreach (var kv in Regions) kv.Value.Mend(false);
                    if (game != null) game.CutsceneCaption = "Toybox Town is mended!";
                }
            }
            // The Storyteller's verdict on the day.
            if (Reached(7, 11.4f) && game != null)
            {
                int gifts = GiftCount();
                game.CutsceneCaption = gifts >= GiftIds.Length ? "And with every present found, it was the grandest wedding the toybox had ever seen."
                    : gifts > 0 ? "A fine wedding, with " + gifts + " of its five lost presents. One wonders what the others would have added."
                    : "A fine wedding, if a simple one. Five presents went missing on the way, you see.";
            }
            if (weddingT > 14.6f)
            {
                weddingT = -1f;
                foreach (var g in ExtraGuests) if (g != null) g.Playing = false;
                foreach (var c in Crowd) if (c != null) c.Playing = false;
                if (game != null)
                {
                    game.CutsceneCaption = null;
                    game.EndCutscene();
                    game.CompleteLevel();
                }
            }
        }
    }
}

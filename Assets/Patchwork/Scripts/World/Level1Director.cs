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
        public Vector3 ArchCenter;
        public Vector3 MayorSpot, BrideSpot;

        float weddingT = -1f;
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
            UpdateMusic();
            MusicPlayer.SnapBand();
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
                else game.ShowToast(msg + "  (Band " + n + "/" + BandIds.Length + ")");
            }
        }

        // ------------------------------------------------------------------ The wedding

        public bool CanStartWedding { get { return BandCount() >= BandIds.Length && weddingT < 0f; } }

        public void StartWedding()
        {
            if (!CanStartWedding) return;
            var game = PatchworkGame.Instance;
            if (game == null) return;
            weddingT = 0f;
            confettiTimer = 0f;
            game.CutsceneCaption = "The Toybox Wedding";
            game.StartCutscene(ArchCenter + new Vector3(0f, 0f, 3f), 15f, 5f, 180f);
            SetBandPlaying(true);
            foreach (var c in Crowd) if (c != null) c.Playing = true;
            Sfx.Play(SfxId.Checkpoint, 0.6f);
        }

        void Update()
        {
            if (weddingT < 0f) return;
            float dt = Time.deltaTime;
            weddingT += dt;
            var game = PatchworkGame.Instance;

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
                    confettiTimer = 0.35f;
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
            if (weddingT > 13f)
            {
                weddingT = -1f;
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

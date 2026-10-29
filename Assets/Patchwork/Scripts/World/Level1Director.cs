using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Runs Level 1's story while you play: counts Heartstrings, peels the Fray off each corner of
    // the garden as its Heartstring is found, unwinds the snarls around the Heart Spool, opens the
    // golden lift once enough are threaded, and breaks the Fray's knot in the sky.
    public class Level1Director : MonoBehaviour
    {
        public static Level1Director Instance { get; private set; }

        public string[] Ids;
        public int Needed = 4;

        // Built by Level1Builder.
        public readonly List<Transform> HeartRings = new List<Transform>();   // snarls around the spool, one per Heartstring needed
        public readonly List<Transform> SpoolPegs = new List<Transform>();    // a peg per Heartstring, lit when found
        public readonly Dictionary<string, FrayRegion> Regions = new Dictionary<string, FrayRegion>();
        public GameObject LiftRoot;         // the golden lift (hidden until it opens)
        public readonly List<SnarlKnot> CoreShell = new List<SnarlKnot>();
        public readonly List<Transform> CoreTethers = new List<Transform>();
        public Transform Core;              // the Fray's knot over the sky island
        public Transform GardenHeart;       // what's left when the knot breaks
        public GameObject CoreGoal;         // touch the heart to finish
        Material pegLit;

        float liftOpening = -1f;
        float coreBreaking = -1f;
        Vector3 coreScale;
        readonly List<Transform> ringsLeaving = new List<Transform>();
        readonly List<Vector3> ringsLeavingScale = new List<Vector3>();

        public int Count
        {
            get
            {
                int n = 0;
                foreach (var id in Ids) if (GameState.IsCollected(id)) n++;
                return n;
            }
        }

        public bool LiftOpen { get { return Count >= Needed; } }

        public static void ResetStatics() { Instance = null; }

        void Awake() { Instance = this; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        // Puts everything into the state saved from earlier visits (no animations).
        public void ApplySavedState()
        {
            pegLit = PatchworkAssets.Glow(LevelKit.Palette.Gold, 0.8f);
            int count = Count;
            for (int i = 0; i < HeartRings.Count; i++) HeartRings[i].gameObject.SetActive(i >= count);
            for (int i = 0; i < SpoolPegs.Count && i < count; i++) LightPeg(i);
            foreach (var kv in Regions) if (GameState.IsCollected(kv.Key)) kv.Value.Mend(true);
            if (LiftRoot != null) LiftRoot.SetActive(count >= Needed);
            if (Core != null) coreScale = Core.localScale;
            if (GardenHeart != null) GardenHeart.gameObject.SetActive(false);
            if (CoreGoal != null) CoreGoal.SetActive(false);
            foreach (var k in CoreShell) k.Popped += ShellPopped;
        }

        void LightPeg(int i)
        {
            if (i < 0 || i >= SpoolPegs.Count) return;
            foreach (var r in SpoolPegs[i].GetComponentsInChildren<Renderer>()) r.sharedMaterial = pegLit;
        }

        public void OnHeartstring(Heartstring h)
        {
            int count = Count;
            var game = PatchworkGame.Instance;
            LightPeg(count - 1);
            if (count - 1 < HeartRings.Count)
            {
                var ring = HeartRings[count - 1];
                ringsLeaving.Add(ring);
                ringsLeavingScale.Add(ring.localScale);
            }
            FrayRegion region;
            if (Regions.TryGetValue(h.Id, out region)) region.Mend(false);

            if (game != null)
            {
                if (count == Needed)
                {
                    game.ShowToast("Heartstring " + count + "/" + Ids.Length + "! The Heart Spool is glowing... a golden thread rises!");
                    liftOpening = 0f;
                    if (LiftRoot != null) { LiftRoot.SetActive(true); LiftRoot.transform.localScale = new Vector3(1f, 0.01f, 1f); }
                }
                else if (count == Ids.Length)
                    game.ShowToast("Every Heartstring in the garden! (" + count + "/" + Ids.Length + ")");
                else
                    game.ShowToast("Heartstring mended: " + h.Title + "  (" + count + "/" + Ids.Length + ")");
            }
        }

        void ShellPopped(SnarlKnot k)
        {
            int left = 0;
            for (int i = 0; i < CoreShell.Count; i++)
            {
                if (!CoreShell[i].IsPopped) left++;
                else if (i < CoreTethers.Count && CoreTethers[i] != null) CoreTethers[i].gameObject.SetActive(false);
            }
            var game = PatchworkGame.Instance;
            if (left > 0)
            {
                if (game != null) game.ShowToast(left + " knot" + (left == 1 ? "" : "s") + " left holding the Fray together!");
                return;
            }
            coreBreaking = 0f;
            if (game != null) game.ShowToast("The Fray's knot is coming undone!");
            Sfx.Play(SfxId.PortalWarp, 0.7f);
        }

        void Update()
        {
            float dt = Time.deltaTime;

            // Snarls around the Heart Spool unwind and shrink away.
            for (int i = ringsLeaving.Count - 1; i >= 0; i--)
            {
                var r = ringsLeaving[i];
                if (r == null) { ringsLeaving.RemoveAt(i); ringsLeavingScale.RemoveAt(i); continue; }
                r.localScale = Vector3.MoveTowards(r.localScale, Vector3.zero, dt * 2f);
                r.Rotate(0f, 400f * dt, 0f, Space.World);
                if (r.localScale.sqrMagnitude < 0.0001f)
                {
                    r.gameObject.SetActive(false);
                    FxBurst.Spawn(FxBurst.Kind.Threads, r.position, SnarlKnot.FrayGrey);
                    ringsLeaving.RemoveAt(i);
                    ringsLeavingScale.RemoveAt(i);
                }
            }

            // The golden lift grows up from the spool.
            if (liftOpening >= 0f && LiftRoot != null)
            {
                liftOpening += dt / 1.5f;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(liftOpening));
                LiftRoot.transform.localScale = new Vector3(1f, Mathf.Max(0.01f, k), 1f);
                if (liftOpening >= 1f) liftOpening = -1f;
            }

            // The knot in the sky shudders, shrinks and bursts, leaving the garden's heart.
            if (Core != null && Core.gameObject.activeSelf)
            {
                Core.Rotate(0f, (coreBreaking >= 0f ? 300f : 20f) * dt, 0f, Space.World);
                if (coreBreaking >= 0f)
                {
                    coreBreaking += dt / 1.6f;
                    float shake = Mathf.Sin(Time.time * 60f) * 0.15f * (1f - coreBreaking);
                    Core.localScale = coreScale * Mathf.Max(0f, 1f - coreBreaking) * (1f + shake);
                    if (coreBreaking >= 1f)
                    {
                        Core.gameObject.SetActive(false);
                        FxBurst.Spawn(FxBurst.Kind.Threads, Core.position, SnarlKnot.FrayDark);
                        FxBurst.Spawn(FxBurst.Kind.Sparkle, Core.position, new Color(1f, 0.85f, 0.5f));
                        FxBurst.Spawn(FxBurst.Kind.Scraps, Core.position, new Color(1f, 0.55f, 0.65f));
                        Sfx.Play(SfxId.Complete, 0.8f);
                        if (GardenHeart != null) GardenHeart.gameObject.SetActive(true);
                        if (CoreGoal != null) CoreGoal.SetActive(true);
                        var game = PatchworkGame.Instance;
                        if (game != null) game.ShowToast("The garden's heart is free! Go and give it a hug.");
                    }
                }
            }
            if (GardenHeart != null && GardenHeart.gameObject.activeSelf)
            {
                GardenHeart.localRotation = Quaternion.Euler(0f, Time.time * 50f, 0f);
                GardenHeart.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 4f) * 0.06f);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The Whispering Wood's secret: three spirit lanterns hidden among the trees. Touch each one
    // to light it (its little ghost-flame wakes up and giggles a chime). With all three lit, the
    // old owl in the hollow tree hoots, the tree's door creaks open and the secret marble (and a
    // prize for the Pod) float out.
    public class WoodSecret : MonoBehaviour
    {
        public readonly List<SpiritLantern> Lanterns = new List<SpiritLantern>();
        public Transform Door;
        public float DoorAngle = -110f;
        public SecretMarble Marble;
        public PrizeBubble Prize;
        public Transform OwlEyes;
        bool open;
        float doorT, owlT = -1f;

        void Start()
        {
            // Already solved on an earlier visit: everything's lit and the door stands open.
            if (Marble != null && GameState.IsCollected(Marble.Id))
            {
                foreach (var l in Lanterns) l.Light(true);
                open = true;
                doorT = 1f;
                if (Door != null) Door.localRotation = Quaternion.Euler(0f, DoorAngle, 0f);
                Marble.Reveal();
                if (Prize != null) Prize.Reveal();
            }
        }

        public void LanternLit()
        {
            int lit = 0;
            foreach (var l in Lanterns) if (l.Lit) lit++;
            var game = PatchworkGame.Instance;
            if (lit < Lanterns.Count)
            {
                if (game != null) game.ShowToast(lit + " of " + Lanterns.Count + " spirit lanterns lit...");
                return;
            }
            if (open) return;
            open = true;
            owlT = 0f;
            if (game != null) game.ShowToast("Hoo-hoo! Something creaks open deep in the wood...");
        }

        void Update()
        {
            if (owlT >= 0f)
            {
                owlT += Time.deltaTime;
                if (owlT > 0.1f && owlT < 0.1f + Time.deltaTime * 1.5f) Sfx.Play(SfxId.Hoot, 0.6f);
                if (OwlEyes != null) OwlEyes.localScale = Vector3.one * (1f + Mathf.Sin(Mathf.Clamp01(owlT / 0.8f) * Mathf.PI) * 0.6f);
                if (owlT > 1.2f)
                {
                    owlT = -1f;
                    Sfx.Play(SfxId.GateRumble, 0.5f);
                }
            }
            if (open && doorT < 1f && owlT < 0f)
            {
                doorT = Mathf.MoveTowards(doorT, 1f, Time.deltaTime * 0.8f);
                float e = 1f - (1f - doorT) * (1f - doorT);
                if (Door != null) Door.localRotation = Quaternion.Euler(0f, DoorAngle * e, 0f);
                if (doorT >= 1f)
                {
                    if (Marble != null) Marble.Reveal();
                    if (Prize != null) Prize.Reveal();
                }
            }
        }
    }

    // One spirit lantern: a paper lantern on a crooked stick. Dark until Patch touches it, then a
    // little wobbling ghost-flame with a face glows inside.
    public class SpiritLantern : PlayerVolume
    {
        public WoodSecret Secret;
        public Renderer Paper;
        public Transform Flame;
        public Material DarkMat, LitMat;
        public float Pitch = 1f;
        public bool Lit { get; private set; }
        float t;

        public void Light(bool quiet)
        {
            if (Lit) return;
            Lit = true;
            if (Paper != null) Paper.sharedMaterial = LitMat;
            if (Flame != null) Flame.gameObject.SetActive(true);
            if (quiet) return;
            Sfx.PlayPitched(SfxId.Chime, 0.5f, Pitch);
            Sfx.PlayPitched(SfxId.Twinkle, 0.35f, Pitch);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position + Vector3.up * 0.6f, new Color(0.6f, 1f, 0.85f));
            if (Secret != null) Secret.LanternLit();
        }

        protected override void OnPlayerEnter(PatchworkPlayer player) { Light(false); }

        void Update()
        {
            if (!Lit || Flame == null) return;
            t += Time.deltaTime;
            Flame.localScale = new Vector3(1f + Mathf.Sin(t * 7f) * 0.08f, 1f + Mathf.Sin(t * 5.3f) * 0.12f, 1f);
            Flame.localRotation = Quaternion.Euler(0f, t * 40f, Mathf.Sin(t * 3f) * 8f);
        }
    }

    // Two glowing eyes in the dark that blink every so often (and sometimes look around).
    public class PeekingEyes : MonoBehaviour
    {
        float next, blink;
        Vector3 baseScale, basePos;

        void Start()
        {
            baseScale = transform.localScale;
            basePos = transform.localPosition;
            next = Random.Range(1f, 5f);
        }

        void Update()
        {
            next -= Time.deltaTime;
            if (next <= 0f) { blink = 0.18f; next = Random.Range(2f, 6f); }
            blink -= Time.deltaTime;
            float k = blink > 0f ? 0.08f : 1f;
            transform.localScale = new Vector3(baseScale.x, baseScale.y * k, baseScale.z);
            transform.localPosition = basePos + new Vector3(Mathf.Sin(Time.time * 0.4f + basePos.x) * 0.05f, 0f, 0f);
        }
    }

    // Fireflies (or will-o'-the-wisps): little glowing motes drifting in slow loops round where
    // they were placed. One component moves all its children.
    public class FireflySwarm : MonoBehaviour
    {
        public bool Pulse = true;          // fireflies glow on and off; butterflies don't
        public float Wander = 1.2f;
        Transform[] motes;
        Vector3[] homes;
        float[] seeds;

        void Start()
        {
            int n = transform.childCount;
            motes = new Transform[n];
            homes = new Vector3[n];
            seeds = new float[n];
            for (int i = 0; i < n; i++)
            {
                motes[i] = transform.GetChild(i);
                homes[i] = motes[i].localPosition;
                seeds[i] = Random.value * 20f;
            }
        }

        void Update()
        {
            float t = Time.time;
            for (int i = 0; i < motes.Length; i++)
            {
                float s = seeds[i];
                Vector3 before = motes[i].localPosition;
                motes[i].localPosition = homes[i] + new Vector3(Mathf.Sin(t * 0.37f + s) * Wander, Mathf.Sin(t * 0.61f + s * 1.3f) * 0.5f, Mathf.Cos(t * 0.29f + s) * Wander);
                if (Pulse)
                {
                    float pulse = 0.6f + 0.4f * Mathf.Sin(t * 2.3f + s * 3f);
                    motes[i].localScale = Vector3.one * 0.12f * pulse;
                }
                else
                {
                    // Face the way it's fluttering.
                    Vector3 v = motes[i].localPosition - before;
                    v.y = 0f;
                    if (v.sqrMagnitude > 1e-8f) motes[i].localRotation = Quaternion.Slerp(motes[i].localRotation, Quaternion.LookRotation(v.normalized, Vector3.up), 0.2f);
                }
            }
        }
    }

    // Puffs a little smoke or steam every so often (the landed Pod's hot nozzle).
    public class Smoker : MonoBehaviour
    {
        public float Every = 2.2f;
        public FxBurst.Kind Kind = FxBurst.Kind.Steam;
        public Color Tint = new Color(0.9f, 0.9f, 0.92f);
        float t;

        void Start() { t = Random.value * Every; }

        void Update()
        {
            t -= Time.deltaTime;
            if (t > 0f) return;
            t = Every * Random.Range(0.7f, 1.3f);
            var game = PatchworkGame.Instance;
            if (game != null && game.Player != null && (game.Player.Motor.Center - transform.position).sqrMagnitude > 60f * 60f) return;
            FxBurst.Spawn(Kind, transform.position, Tint, transform.up);
        }
    }

    // The forest's darker, bluer look, faded in with the spooky music.
    public class SpookyLook : MonoBehaviour
    {
        UnityEngine.Rendering.Volume vol;

        public static void Create(Transform parent)
        {
            var go = new GameObject("SpookyLook");
            go.transform.SetParent(parent, false);
            var look = go.AddComponent<SpookyLook>();
            look.vol = go.AddComponent<UnityEngine.Rendering.Volume>();
            look.vol.isGlobal = true;
            look.vol.priority = 11f;
            look.vol.weight = 0f;
            var p = ScriptableObject.CreateInstance<UnityEngine.Rendering.VolumeProfile>();
            p.name = "SpookyLook";
            var adjust = p.Add<UnityEngine.Rendering.Universal.ColorAdjustments>(true);
            adjust.postExposure.Override(-0.55f);
            adjust.saturation.Override(-18f);
            adjust.colorFilter.Override(new Color(0.78f, 0.82f, 1f));
            var vig = p.Add<UnityEngine.Rendering.Universal.Vignette>(true);
            vig.intensity.Override(0.42f);
            vig.smoothness.Override(0.55f);
            vig.color.Override(new Color(0.05f, 0.04f, 0.12f));
            look.vol.profile = p;
            go.AddComponent<OwnedProfile>().Profile = p;
        }

        void Update()
        {
            if (vol != null) vol.weight = Mathf.SmoothStep(0f, 1f, MusicPlayer.SpookyMix);
        }
    }
}

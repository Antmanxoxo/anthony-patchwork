using UnityEngine;

namespace Patchwork
{
    // A checkpoint: an embroidery hoop on a little wooden easel, with plain linen stretched in
    // it and a needle dangling on its thread underneath. Touch it and it stitches itself: a
    // ring of gold running stitches races round the hoop, then a glowing heart fills in stitch
    // by stitch, the needle hops up and tucks itself into the cloth, a lamp warms up and the
    // whole thing gives a happy bounce. When another checkpoint takes over, the heart quietly
    // unpicks itself and the needle drops back down to dangle. Touching it makes it the
    // respawn point.
    public class Checkpoint : PlayerVolume
    {
        public Vector3 SpawnPoint;
        public float SpawnYaw;

        Transform look, hoop, needle;
        Renderer[] stitches;
        Transform[] heart;
        Vector3[] heartScale;
        Light lamp;
        Material lit, unlit;
        bool active;
        float k, pop = 1f, time;

        static readonly Color Gold = new Color(1f, 0.8f, 0.32f);

        // The visual's parts (made by LevelKit.AddCheckpoint).
        public void Setup(Transform root, Transform hoopPart, Transform needlePart, Renderer[] stitchParts, Transform[] heartParts, Light glow)
        {
            look = root;
            hoop = hoopPart;
            needle = needlePart;
            stitches = stitchParts;
            heart = heartParts;
            heartScale = new Vector3[heart.Length];
            for (int i = 0; i < heart.Length; i++) heartScale[i] = heart[i].localScale;
            lamp = glow;
            lit = PatchworkAssets.Glow(Gold, 1.1f);
            unlit = PatchworkAssets.Felt(new Color(0.72f, 0.7f, 0.68f));
            k = 0f;
            Apply();
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (active) return;
            player.Game.SetCheckpoint(this);
        }

        public void SetActiveVisual(bool on)
        {
            bool was = active;
            active = on;
            if (on && !was && look != null)
            {
                pop = 0f;
                FxBurst.Spawn(FxBurst.Kind.Sparkle, (hoop != null ? hoop.position : look.position + Vector3.up * 1.5f), Gold);
            }
        }

        void Update()
        {
            if (look == null) return;
            float dt = Time.deltaTime;
            k = Mathf.MoveTowards(k, active ? 1f : 0f, dt * (active ? 0.75f : 0.5f));
            time += dt;
            pop = Mathf.Min(1f, pop + dt * 2f);
            Apply();
        }

        void Apply()
        {
            // The running stitches light up one after another round the hoop (first 40%).
            if (stitches != null)
                for (int i = 0; i < stitches.Length; i++)
                {
                    bool on = k > 0.02f + 0.38f * i / stitches.Length;
                    var m = on ? lit : unlit;
                    if (stitches[i] != null && stitches[i].sharedMaterial != m) stitches[i].sharedMaterial = m;
                }
            // Then the heart fills in from the point upwards, each stitch popping in.
            if (heart != null)
                for (int i = 0; i < heart.Length; i++)
                {
                    float start = 0.4f + 0.55f * i / heart.Length;
                    float s = Mathf.Clamp01((k - start) / 0.08f);
                    float over = s < 1f ? 1f + Mathf.Sin(s * Mathf.PI) * 0.35f : 1f;
                    heart[i].localScale = heartScale[i] * (s * over);
                    heart[i].gameObject.SetActive(s > 0.001f);
                }
            float e = Mathf.SmoothStep(0f, 1f, k);
            // The needle: dangling and swinging on its thread asleep, tucked into the cloth awake.
            if (needle != null)
            {
                float swing = Mathf.Sin(time * 1.6f) * 9f * (1f - e);
                needle.localPosition = Vector3.Lerp(new Vector3(0f, -0.86f, 0.03f), new Vector3(0.34f, 0.42f, 0.06f), e);
                needle.localRotation = Quaternion.Slerp(Quaternion.Euler(0f, 0f, swing), Quaternion.Euler(0f, 0f, -48f), e);
            }
            // The hoop leans in a little and breathes while it's awake.
            if (hoop != null)
                hoop.localRotation = Quaternion.Euler(-10f + Mathf.Sin(time * 1.3f) * 1.5f * e, 0f, Mathf.Sin(time * 0.9f) * 1.2f * e);
            // A happy bounce when it's stitched.
            float bounce = pop < 1f ? Mathf.Sin(pop * Mathf.PI) * (1f - pop) * 0.3f : 0f;
            look.localScale = new Vector3(1f - bounce * 0.35f, 1f + bounce, 1f - bounce * 0.35f);
            if (lamp != null)
            {
                lamp.enabled = e > 0.02f;
                lamp.intensity = e * (1.3f + Mathf.Sin(time * 2.4f) * 0.1f);
            }
        }
    }
}

using UnityEngine;

namespace Patchwork
{
    // A checkpoint: a sewing station. A big tomato pincushion bristling with pins, with a
    // wooden cotton reel standing on it and a needle lying asleep across the top of the reel.
    // Touch it and it's "stitched": the needle springs up and stands to attention, the thread
    // on the reel lights up gold, the reel starts slowly winding, a little lamp glows and the
    // whole thing gives a happy bounce. When another checkpoint takes over it settles back
    // down to sleep. Touching it makes it the respawn point.
    public class Checkpoint : PlayerVolume
    {
        public Vector3 SpawnPoint;
        public float SpawnYaw;

        Transform look, reel, needle;
        Renderer[] thread;
        Light lamp;
        bool active;
        float k, pop = 1f, wake;

        static readonly Color Gold = new Color(1f, 0.8f, 0.3f);
        static readonly Color Sleepy = new Color(0.62f, 0.6f, 0.68f);

        // The visual's parts (made by LevelKit.AddCheckpoint).
        public void Setup(Transform root, Transform reelPart, Transform needlePart, Renderer[] threadParts, Light glow)
        {
            look = root;
            reel = reelPart;
            needle = needlePart;
            thread = threadParts;
            lamp = glow;
            k = 0f;
            Apply();
            SetActiveVisual(false);
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
            if (thread != null)
                foreach (var r in thread)
                    if (r != null) r.sharedMaterial = on ? PatchworkAssets.Glow(Gold, 0.8f) : PatchworkAssets.Fabric(Sleepy);
            if (on && !was && look != null)
            {
                pop = 0f;
                FxBurst.Spawn(FxBurst.Kind.Sparkle, look.position + Vector3.up * 1.4f, Gold);
            }
        }

        void Update()
        {
            if (look == null) return;
            float dt = Time.deltaTime;
            k = Mathf.MoveTowards(k, active ? 1f : 0f, dt * (active ? 3f : 1.2f));
            wake += dt;
            pop = Mathf.Min(1f, pop + dt * 2.2f);
            Apply();
        }

        void Apply()
        {
            float e = Mathf.SmoothStep(0f, 1f, k);
            // The needle: lying across the reel asleep, standing up (with a little sway) awake.
            if (needle != null)
            {
                float sway = Mathf.Sin(wake * 2.2f) * 6f * e;
                needle.localRotation = Quaternion.Euler(Mathf.Lerp(84f, 10f, e) + sway * 0.4f, 0f, sway);
            }
            // The reel winds the thread on while it's awake.
            if (reel != null && k > 0.01f)
                reel.Rotate(Vector3.up, 55f * e * Time.deltaTime, Space.Self);
            // A happy bounce when it's stitched.
            float bounce = pop < 1f ? Mathf.Sin(pop * Mathf.PI) * (1f - pop) * 0.35f : 0f;
            look.localScale = new Vector3(1f - bounce * 0.4f, 1f + bounce, 1f - bounce * 0.4f);
            if (lamp != null)
            {
                lamp.enabled = e > 0.02f;
                lamp.intensity = e * (1.1f + Mathf.Sin(wake * 3f) * 0.12f);
            }
        }
    }
}

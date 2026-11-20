using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The background sound of a level, chosen by where Patch is: the streets sound like a
    // town, the canal laps, the ravine babbles, the farm clucks, and the open country has its
    // breeze and birds. Each bed is a soft 2D loop; as you walk from one area to the next they
    // crossfade over a couple of seconds. (Spot sounds, like the clock or the mill, still come
    // from AmbientEmitters where they are.)
    public class AmbientZones : MonoBehaviour
    {
        // How much an area counts at a spot: 1 inside, 0 outside, soft in between.
        public delegate float Weigh(Vector3 at);

        class Bed
        {
            public AudioSource Src;
            public AudioLowPassFilter Filter;
            public float Volume;
            public Weigh Weight;
            public float Level, Want;
        }

        readonly List<Bed> beds = new List<Bed>();
        Bed fallback;
        float fade;

        public static AmbientZones Create(Transform parent)
        {
            var go = new GameObject("AreaSounds");
            go.transform.SetParent(parent, false);
            return go.AddComponent<AmbientZones>();
        }

        // An area and its sound. A null clip means "quiet here" (indoors, say).
        public AmbientZones Add(AudioClip clip, float volume, Weigh weight)
        {
            beds.Add(MakeBed(clip, volume, weight));
            return this;
        }

        // What you hear anywhere that isn't one of the areas.
        public AmbientZones Otherwise(AudioClip clip, float volume)
        {
            fallback = MakeBed(clip, volume, null);
            return this;
        }

        Bed MakeBed(AudioClip clip, float volume, Weigh weight)
        {
            var b = new Bed { Volume = volume, Weight = weight };
            if (clip == null) return b;
            var go = new GameObject("Bed_" + clip.name);
            go.transform.SetParent(transform, false);
            b.Src = go.AddComponent<AudioSource>();
            b.Src.clip = clip;
            b.Src.loop = true;
            b.Src.playOnAwake = false;
            b.Src.spatialBlend = 0f;
            b.Src.volume = 0f;
            b.Src.time = Random.value * clip.length * 0.9f;
            b.Src.Play();
            b.Filter = go.AddComponent<AudioLowPassFilter>();
            b.Filter.cutoffFrequency = 22000f;
            return b;
        }

        // ---- Shapes for areas

        // A box on the ground (x and z), fading out over `feather` metres outside it.
        public static float Box(Vector3 p, float x0, float x1, float z0, float z1, float feather)
        {
            float dx = Mathf.Max(x0 - p.x, p.x - x1, 0f);
            float dz = Mathf.Max(z0 - p.z, p.z - z1, 0f);
            return Soft(Mathf.Sqrt(dx * dx + dz * dz), feather);
        }

        // A circle on the ground.
        public static float Round(Vector3 p, Vector3 centre, float radius, float feather)
        {
            float d = new Vector2(p.x - centre.x, p.z - centre.z).magnitude - radius;
            return Soft(Mathf.Max(0f, d), feather);
        }

        static float Soft(float outside, float feather)
        {
            if (outside <= 0f) return 1f;
            float k = Mathf.Clamp01(1f - outside / Mathf.Max(0.01f, feather));
            return k * k * (3f - 2f * k);
        }

        Vector3 Ear()
        {
            var game = PatchworkGame.Instance;
            if (game != null && game.Player != null) return game.Player.Motor.Center;
            var cam = Camera.main;
            return cam != null ? cam.transform.position : transform.position;
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            bool leaving = game != null && game.Mode == UIMode.Loading;
            float dt = Time.unscaledDeltaTime;
            fade = Mathf.MoveTowards(fade, leaving ? 0f : 1f, dt * (leaving ? 2f : 0.5f));

            Vector3 at = Ear();
            float sum = 0f;
            foreach (var b in beds) { b.Want = Mathf.Clamp01(b.Weight(at)); sum += b.Want; }
            if (sum > 1f) foreach (var b in beds) b.Want /= sum;
            if (fallback != null) fallback.Want = Mathf.Clamp01(1f - sum);

            float master = fade * Sfx.Volume / 10f;
            foreach (var b in beds) Drive(b, dt, master);
            if (fallback != null) Drive(fallback, dt, master);
        }

        static void Drive(Bed b, float dt, float master)
        {
            b.Level = Mathf.MoveTowards(b.Level, b.Want, dt * 0.45f);
            if (b.Src == null) return;
            b.Src.volume = b.Volume * b.Level * master;
            b.Filter.cutoffFrequency = Sfx.LoopCutoff;
        }
    }
}

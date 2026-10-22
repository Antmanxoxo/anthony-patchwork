using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    public enum SfxId
    {
        Transform,
        TransformFail,
        Jump,
        Land,
        TetherAttach,
        TetherRelease,
        Gust,
        Splash,
        Collect,
        Checkpoint,
        MenuOpen,
        MenuMove,
        MenuConfirm,
        MenuCancel,
        Complete,
        Respawn,
        PortalWarp,
        Soak,
        Steam,
        Unravel
    }

    /// <summary>Continuous sounds that fade in while something requests them each frame.</summary>
    public enum LoopId
    {
        Bubbles,   // Water Form rising in a fountain
        Glide      // Wind Form gliding
    }

    /// <summary>
    /// Tiny synthesised sound kit (no audio files needed). Everything is soft and rounded:
    /// sines and low-passed noise only, no harsh square waves or bright white noise.
    /// </summary>
    public static class Sfx
    {
        const int Rate = 44100;
        static AudioSource source;
        static readonly Dictionary<SfxId, AudioClip> clips = new Dictionary<SfxId, AudioClip>();

        // Loops
        static AudioSource[] loopSources;
        static float[] loopTargets;
        static readonly float[] LoopVolumes = { 0.45f, 0.3f };

        public static void Init(GameObject host)
        {
            source = host.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            if (clips.Count == 0) Build();

            int n = System.Enum.GetValues(typeof(LoopId)).Length;
            loopSources = new AudioSource[n];
            loopTargets = new float[n];
            for (int i = 0; i < n; i++)
            {
                var s = host.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.loop = true;
                s.spatialBlend = 0f;
                s.volume = 0f;
                s.clip = i == (int)LoopId.Bubbles ? BubbleLoop() : AirLoop();
                loopSources[i] = s;
            }
        }

        public static void Play(SfxId id, float volume = 0.6f)
        {
            if (source == null) return;
            AudioClip clip;
            if (clips.TryGetValue(id, out clip) && clip != null)
                source.PlayOneShot(clip, volume);
        }

        /// <summary>Ask for a loop to be audible this frame. Call every frame while it should play.</summary>
        public static void RequestLoop(LoopId id)
        {
            if (loopTargets != null) loopTargets[(int)id] = 1f;
        }

        /// <summary>Fades loops in/out. Called once per frame by the game after gameplay has run.</summary>
        public static void TickLoops(float dt)
        {
            if (loopSources == null) return;
            for (int i = 0; i < loopSources.Length; i++)
            {
                var s = loopSources[i];
                if (s == null) continue;
                float target = loopTargets[i] * LoopVolumes[i];
                s.volume = Mathf.MoveTowards(s.volume, target, dt * (target > s.volume ? 2.5f : 1.5f));
                if (s.volume > 0.001f && !s.isPlaying) s.Play();
                if (s.volume <= 0.001f && s.isPlaying) s.Stop();
                loopTargets[i] = 0f;
            }
        }

        public static void StopLoops()
        {
            if (loopSources == null) return;
            for (int i = 0; i < loopSources.Length; i++)
            {
                if (loopSources[i] == null) continue;
                loopSources[i].volume = 0f;
                loopSources[i].Stop();
                loopTargets[i] = 0f;
            }
        }

        // ------------------------------------------------------------------ Clip design

        delegate float Wave(float t, float dur);

        static void Build()
        {
            // Warm "poof + chime" for transforming.
            clips[SfxId.Transform] = Make("transform", 0.55f, (t, d) =>
                Bell(t, Mathf.Lerp(392f, 523f, Mathf.Clamp01(t / 0.25f)), d) * 0.45f +
                Bell(t - 0.08f, 659f, d) * 0.25f, 3500f);
            clips[SfxId.TransformFail] = Make("fail", 0.25f, (t, d) =>
                Sine(t, Mathf.Lerp(260f, 180f, t / d)) * Env(t, d, 0.01f) * 0.4f, 2000f);
            clips[SfxId.Jump] = Make("jump", 0.18f, (t, d) =>
                Sine(t, Mathf.Lerp(280f, 460f, t / d)) * Env(t, d, 0.008f) * 0.45f, 2500f);
            clips[SfxId.Land] = Make("land", 0.12f, (t, d) =>
                Sine(t, Mathf.Lerp(140f, 90f, t / d)) * Env(t, d, 0.003f) * 0.6f, 800f);
            clips[SfxId.TetherAttach] = Make("attach", 0.25f, (t, d) =>
                (Sine(t, 330f) * 0.5f + Sine(t, 495f) * 0.25f) * Env(t, d, 0.004f), 2500f);
            clips[SfxId.TetherRelease] = Make("release", 0.22f, (t, d) =>
                Sine(t, Mathf.Lerp(520f, 330f, t / d)) * Env(t, d, 0.006f) * 0.4f, 2500f);
            clips[SfxId.Gust] = Whoosh("gust", 0.55f, 250f, 1300f, 0.8f);
            clips[SfxId.Splash] = Make("splash", 0.3f, (t, d) =>
                Bloop(t, 180f, 0.12f) * 0.6f + Bloop(t - 0.07f, 260f, 0.12f) * 0.4f, 2500f);
            clips[SfxId.Collect] = Make("collect", 0.6f, (t, d) =>
                Bell(t, 784f, d) * 0.4f + Bell(t - 0.1f, 1047f, d) * 0.3f, 4000f);
            clips[SfxId.Checkpoint] = Make("checkpoint", 0.7f, (t, d) =>
                Bell(t, 523f, d) * 0.35f + Bell(t - 0.12f, 659f, d) * 0.3f + Bell(t - 0.24f, 784f, d) * 0.3f, 4000f);
            clips[SfxId.MenuOpen] = Make("menuopen", 0.16f, (t, d) =>
                Sine(t, Mathf.Lerp(392f, 587f, t / d)) * Env(t, d, 0.006f) * 0.35f, 3000f);
            clips[SfxId.MenuMove] = Make("menumove", 0.07f, (t, d) =>
                Sine(t, 660f) * Env(t, d, 0.003f) * 0.3f, 3000f);
            clips[SfxId.MenuConfirm] = Make("menuconfirm", 0.25f, (t, d) =>
                Bell(t, 587f, d) * 0.4f + Bell(t - 0.05f, 880f, d) * 0.2f, 4000f);
            clips[SfxId.MenuCancel] = Make("menucancel", 0.16f, (t, d) =>
                Sine(t, Mathf.Lerp(440f, 294f, t / d)) * Env(t, d, 0.006f) * 0.35f, 2500f);
            clips[SfxId.Complete] = Make("complete", 1.6f, (t, d) =>
                Bell(t, 523f, d) * 0.3f + Bell(t - 0.18f, 659f, d) * 0.3f + Bell(t - 0.36f, 784f, d) * 0.3f +
                Bell(t - 0.54f, 1047f, d) * 0.3f, 4000f);
            clips[SfxId.Respawn] = Make("respawn", 0.45f, (t, d) =>
                Sine(t, Mathf.Lerp(262f, 523f, t / d)) * Env(t, d, 0.03f) * 0.35f, 2500f);
            clips[SfxId.PortalWarp] = PortalWarp();
            clips[SfxId.Soak] = Make("soak", 0.45f, (t, d) =>
                Bloop(t, 220f, 0.1f) * 0.4f + Bloop(t - 0.12f, 300f, 0.1f) * 0.35f + Bloop(t - 0.24f, 200f, 0.12f) * 0.35f, 2200f);
            clips[SfxId.Steam] = Whoosh("steam", 0.5f, 900f, 2200f, 0.35f);
            clips[SfxId.Unravel] = Make("unravel", 0.4f, (t, d) =>
                Sine(t, 440f + Mathf.Sin(t * 60f) * 60f) * Env(t, d, 0.02f) * 0.3f, 2500f);
        }

        static AudioClip Make(string name, float duration, Wave wave, float lowpassHz)
        {
            int n = Mathf.CeilToInt(duration * Rate);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                data[i] = wave(t, duration);
            }
            LowPass(data, lowpassHz);
            FadeEdges(data, 0.004f);
            return ToClip(name, data);
        }

        /// <summary>Soft airy whoosh: low-passed noise with a cutoff that swells and settles.</summary>
        static AudioClip Whoosh(string name, float duration, float lowCut, float highCut, float volume)
        {
            int n = Mathf.CeilToInt(duration * Rate);
            var data = new float[n];
            float y = 0f, y2 = 0f;
            var rng = new System.Random(7);
            for (int i = 0; i < n; i++)
            {
                float k = i / (float)n;
                float swell = Mathf.Sin(Mathf.PI * Mathf.Pow(k, 0.7f));
                float cutoff = Mathf.Lerp(lowCut, highCut, swell);
                float a = Alpha(cutoff);
                float x = (float)(rng.NextDouble() * 2.0 - 1.0);
                y += a * (x - y);
                y2 += a * (y - y2);
                data[i] = y2 * swell * volume * 3f;
            }
            FadeEdges(data, 0.01f);
            return ToClip(name, data);
        }

        static AudioClip PortalWarp()
        {
            float duration = 1.2f;
            int n = Mathf.CeilToInt(duration * Rate);
            var data = new float[n];
            var rng = new System.Random(11);
            float y = 0f, phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float k = t / duration;
                float f = Mathf.Lerp(180f, 720f, k * k);
                phase += 2f * Mathf.PI * f / Rate;
                float a = Alpha(Mathf.Lerp(300f, 2500f, k));
                y += a * ((float)(rng.NextDouble() * 2.0 - 1.0) - y);
                float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(k * 1.1f));
                data[i] = (Mathf.Sin(phase) * 0.3f + Mathf.Sin(phase * 1.5f) * 0.15f + y * 0.6f) * env * 0.6f;
            }
            FadeEdges(data, 0.01f);
            return ToClip("portal", data);
        }

        /// <summary>2 s seamless loop of soft bubbles rising (for Water Form in fountains).</summary>
        static AudioClip BubbleLoop()
        {
            float duration = 2f;
            int n = Mathf.CeilToInt(duration * Rate);
            var data = new float[n];
            var rng = new System.Random(3);
            int bubbles = 26;
            for (int b = 0; b < bubbles; b++)
            {
                float start = (float)rng.NextDouble() * duration;
                float f0 = 220f + (float)rng.NextDouble() * 380f;
                float len = 0.06f + (float)rng.NextDouble() * 0.07f;
                float vol = 0.15f + (float)rng.NextDouble() * 0.2f;
                int s0 = (int)(start * Rate);
                int count = (int)(len * Rate);
                float phase = 0f;
                for (int i = 0; i < count; i++)
                {
                    float t = i / (float)Rate;
                    float f = f0 * (1f + 0.8f * (t / len));
                    phase += 2f * Mathf.PI * f / Rate;
                    float env = Mathf.Clamp01(t / 0.005f) * Mathf.Pow(1f - t / len, 2f);
                    int idx = (s0 + i) % n; // wrap so the loop is seamless
                    data[idx] += Mathf.Sin(phase) * env * vol;
                }
            }
            // Gentle low rumble of moving water underneath.
            float y = 0f, y2 = 0f;
            float a = Alpha(260f);
            for (int i = 0; i < n; i++)
            {
                y += a * ((float)(rng.NextDouble() * 2.0 - 1.0) - y);
                y2 += a * (y - y2);
                data[i] += y2 * 0.5f;
            }
            LowPass(data, 2500f);
            return ToClip("bubbles", data);
        }

        /// <summary>Soft airy loop for gliding.</summary>
        static AudioClip AirLoop()
        {
            float duration = 2f;
            int n = Mathf.CeilToInt(duration * Rate);
            var data = new float[n];
            var rng = new System.Random(5);
            float y = 0f, y2 = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float cutoff = 500f + 250f * Mathf.Sin(2f * Mathf.PI * t / duration);
                float a = Alpha(cutoff);
                y += a * ((float)(rng.NextDouble() * 2.0 - 1.0) - y);
                y2 += a * (y - y2);
                data[i] = y2 * 1.6f;
            }
            // Crossfade the end into the start for a seamless loop.
            int fade = Rate / 10;
            for (int i = 0; i < fade; i++)
            {
                float k = i / (float)fade;
                data[i] = data[i] * k + data[n - fade + i] * (1f - k);
            }
            return ToClip("air", data);
        }

        // ------------------------------------------------------------------ DSP helpers

        static float Alpha(float cutoffHz)
        {
            float rc = 1f / (2f * Mathf.PI * cutoffHz);
            float dt = 1f / Rate;
            return dt / (rc + dt);
        }

        static void LowPass(float[] data, float cutoffHz)
        {
            float a = Alpha(cutoffHz);
            float y = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                y += a * (data[i] - y);
                data[i] = y;
            }
        }

        static void FadeEdges(float[] data, float seconds)
        {
            int f = Mathf.Min(data.Length / 2, Mathf.CeilToInt(seconds * Rate));
            for (int i = 0; i < f; i++)
            {
                float k = i / (float)f;
                data[i] *= k;
                data[data.Length - 1 - i] *= k;
            }
        }

        static AudioClip ToClip(string name, float[] data)
        {
            for (int i = 0; i < data.Length; i++) data[i] = Mathf.Clamp(data[i], -1f, 1f);
            var clip = AudioClip.Create("pw_" + name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float Sine(float t, float f) { return t < 0f ? 0f : Mathf.Sin(2f * Mathf.PI * f * t); }

        static float Env(float t, float d, float attack)
        {
            if (t < 0f) return 0f;
            float a = Mathf.Clamp01(t / attack);
            float r = Mathf.Clamp01(1f - t / d);
            return a * r * r;
        }

        /// <summary>Soft bell / music-box tone.</summary>
        static float Bell(float t, float f, float d)
        {
            if (t < 0f) return 0f;
            float env = Mathf.Clamp01(t / 0.005f) * Mathf.Exp(-t * 5f) * Mathf.Clamp01((d - t) / 0.05f);
            return (Sine(t, f) + 0.25f * Sine(t, f * 2f) + 0.08f * Sine(t, f * 3f)) * env;
        }

        /// <summary>A single water "bloop": quick upward pitch bend with a fast decay.</summary>
        static float Bloop(float t, float f0, float len)
        {
            if (t < 0f || t > len) return 0f;
            float k = t / len;
            float f = f0 * (1f + 1.2f * k);
            return Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Clamp01(t / 0.004f) * (1f - k) * (1f - k);
        }
    }
}

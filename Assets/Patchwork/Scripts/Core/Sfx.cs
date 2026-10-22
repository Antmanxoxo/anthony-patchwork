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
        Glide,
        Splash,
        Collect,
        Checkpoint,
        MenuOpen,
        MenuMove,
        MenuConfirm,
        MenuCancel,
        Complete,
        Respawn
    }

    /// <summary>
    /// Tiny synthesised sound kit (no audio files needed). Plinky, soft, toy-like tones.
    /// </summary>
    public static class Sfx
    {
        const int Rate = 44100;
        static AudioSource source;
        static readonly Dictionary<SfxId, AudioClip> clips = new Dictionary<SfxId, AudioClip>();

        public static void Init(GameObject host)
        {
            source = host.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            if (clips.Count == 0) Build();
        }

        public static void Play(SfxId id, float volume = 0.6f)
        {
            if (source == null) return;
            AudioClip clip;
            if (clips.TryGetValue(id, out clip) && clip != null)
            {
                source.pitch = 1f;
                source.PlayOneShot(clip, volume);
            }
        }

        delegate float Wave(float t, float dur);

        static void Build()
        {
            clips[SfxId.Transform] = Make("transform", 0.45f, (t, d) =>
                Chime(t, Mathf.Lerp(520f, 1040f, t / d), d) * 0.6f + Chime(t, Mathf.Lerp(780f, 1560f, t / d), d) * 0.3f + Noise(t) * Env(t, d, 0.01f) * 0.15f);
            clips[SfxId.TransformFail] = Make("fail", 0.25f, (t, d) =>
                Square(t, Mathf.Lerp(220f, 160f, t / d)) * Env(t, d, 0.01f) * 0.3f);
            clips[SfxId.Jump] = Make("jump", 0.16f, (t, d) =>
                Sine(t, Mathf.Lerp(330f, 620f, t / d)) * Env(t, d, 0.005f) * 0.5f);
            clips[SfxId.Land] = Make("land", 0.08f, (t, d) =>
                (Noise(t) * 0.5f + Sine(t, 120f)) * Env(t, d, 0.002f) * 0.35f);
            clips[SfxId.TetherAttach] = Make("attach", 0.22f, (t, d) =>
                (Sine(t, 880f) * 0.4f + Noise(t) * 0.2f) * Env(t, d, 0.003f) + Sine(t, 440f) * Env(t, d, 0.01f) * 0.4f);
            clips[SfxId.TetherRelease] = Make("release", 0.18f, (t, d) =>
                Sine(t, Mathf.Lerp(700f, 400f, t / d)) * Env(t, d, 0.005f) * 0.4f);
            clips[SfxId.Gust] = Make("gust", 0.45f, (t, d) =>
                Noise(t) * Mathf.Sin(Mathf.PI * t / d) * 0.45f);
            clips[SfxId.Glide] = Make("glide", 0.3f, (t, d) =>
                Noise(t) * Mathf.Sin(Mathf.PI * t / d) * 0.15f);
            clips[SfxId.Splash] = Make("splash", 0.35f, (t, d) =>
                (Noise(t) * 0.5f + Sine(t, Mathf.Lerp(900f, 300f, t / d)) * 0.4f) * Env(t, d, 0.005f));
            clips[SfxId.Collect] = Make("collect", 0.5f, (t, d) =>
                Chime(t, t < 0.12f ? 988f : 1319f, d) * 0.6f);
            clips[SfxId.Checkpoint] = Make("checkpoint", 0.6f, (t, d) =>
                Chime(t, t < 0.15f ? 659f : (t < 0.3f ? 784f : 1047f), d) * 0.5f);
            clips[SfxId.MenuOpen] = Make("menuopen", 0.15f, (t, d) =>
                Sine(t, Mathf.Lerp(400f, 800f, t / d)) * Env(t, d, 0.005f) * 0.35f);
            clips[SfxId.MenuMove] = Make("menumove", 0.05f, (t, d) =>
                Sine(t, 1200f) * Env(t, d, 0.002f) * 0.3f);
            clips[SfxId.MenuConfirm] = Make("menuconfirm", 0.18f, (t, d) =>
                Chime(t, 1047f, d) * 0.4f);
            clips[SfxId.MenuCancel] = Make("menucancel", 0.15f, (t, d) =>
                Sine(t, Mathf.Lerp(600f, 300f, t / d)) * Env(t, d, 0.005f) * 0.35f);
            clips[SfxId.Complete] = Make("complete", 1.2f, (t, d) =>
                Chime(t, t < 0.2f ? 523f : t < 0.4f ? 659f : t < 0.6f ? 784f : 1047f, d) * 0.5f);
            clips[SfxId.Respawn] = Make("respawn", 0.4f, (t, d) =>
                Sine(t, Mathf.Lerp(300f, 700f, t / d)) * Env(t, d, 0.02f) * 0.35f);
        }

        static AudioClip Make(string name, float duration, Wave wave)
        {
            int n = Mathf.CeilToInt(duration * Rate);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                data[i] = Mathf.Clamp(wave(t, duration), -1f, 1f);
            }
            var clip = AudioClip.Create("pw_" + name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float Sine(float t, float f) { return Mathf.Sin(2f * Mathf.PI * f * t); }
        static float Square(float t, float f) { return Mathf.Sign(Mathf.Sin(2f * Mathf.PI * f * t)); }

        static float Noise(float t)
        {
            int i = (int)(t * Rate);
            unchecked
            {
                int h = i * 1103515245 + 12345;
                h ^= h >> 15;
                return ((h & 0xFFFF) / 32767.5f) - 1f;
            }
        }

        /// <summary>Quick attack, exponential-ish decay.</summary>
        static float Env(float t, float d, float attack)
        {
            float a = Mathf.Clamp01(t / attack);
            float r = Mathf.Clamp01(1f - t / d);
            return a * r * r;
        }

        static float Chime(float t, float f, float d)
        {
            return (Sine(t, f) + 0.3f * Sine(t, f * 2f)) * Env(t, d, 0.004f);
        }
    }
}

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
        Unravel,
        HintWater,
        HintYarn,
        HintWind,
        HintNeutral,
        Boing,
        Thread,
        PauseOpen,
        PlatformThunk
    }

    // Continuous sounds that fade in while something requests them each frame.
    public enum LoopId
    {
        Bubbles,   // Water Form rising in a fountain
        Glide      // Wind Form gliding
    }

    // Tiny synthesised sound kit (no audio files needed). Everything is soft and rounded:
    // sines and low-passed noise only, no harsh square waves or bright white noise.
    public static class Sfx
    {
        const int Rate = 44100;
        const int VoiceCount = 8;
        const string VolumeKey = "pw_sfx_volume";

        static AudioSource[] voices;
        static int nextVoice;
        static readonly Dictionary<SfxId, AudioClip> clips = new Dictionary<SfxId, AudioClip>();

        // Loops
        static AudioSource[] loopSources;
        static AudioLowPassFilter[] loopFilters;
        static float[] loopTargets;
        static readonly float[] LoopVolumes = { 0.45f, 0.3f };
        static AudioClip bubbleClip, airClip;
        static float loopCutoff = 22000f, loopCutoffTarget = 22000f;

        // Pickups in quick succession climb a pentatonic scale, so a row of spools plays a little tune.
        static readonly float[] PickupSteps = { 1f, 9f / 8f, 5f / 4f, 3f / 2f, 5f / 3f, 2f };
        const float PickupWindow = 1.5f;
        static float lastPickupTime = -10f;
        static int pickupStep;

        // Sound effects volume, 0..10 (saved).
        public static int Volume
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(VolumeKey, 10), 0, 10); }
            set { PlayerPrefs.SetInt(VolumeKey, Mathf.Clamp(value, 0, 10)); }
        }

        public static void Init(GameObject host)
        {
            voices = new AudioSource[VoiceCount];
            for (int i = 0; i < VoiceCount; i++)
            {
                var v = host.AddComponent<AudioSource>();
                v.playOnAwake = false;
                v.spatialBlend = 0f;
                voices[i] = v;
            }
            nextVoice = 0;
            AudioClip probe;
            if (clips.Count == 0 || !clips.TryGetValue(SfxId.Jump, out probe) || probe == null) Build();
            if (bubbleClip == null) bubbleClip = BubbleLoop();
            if (airClip == null) airClip = AirLoop();

            int n = System.Enum.GetValues(typeof(LoopId)).Length;
            loopSources = new AudioSource[n];
            loopFilters = new AudioLowPassFilter[n];
            loopTargets = new float[n];
            for (int i = 0; i < n; i++)
            {
                // Each loop gets its own object so it can have its own muffle filter.
                var go = new GameObject("Loop " + (LoopId)i);
                go.transform.SetParent(host.transform, false);
                var s = go.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.loop = true;
                s.spatialBlend = 0f;
                s.volume = 0f;
                s.clip = i == (int)LoopId.Bubbles ? bubbleClip : airClip;
                loopSources[i] = s;
                loopFilters[i] = go.AddComponent<AudioLowPassFilter>();
                loopFilters[i].cutoffFrequency = 22000f;
            }
            loopCutoff = loopCutoffTarget = 22000f;
        }

        // Forget the audio sources from the last Play session (they belonged to the old camera).
        public static void ResetStatics()
        {
            voices = null;
            loopSources = null;
            loopFilters = null;
            loopTargets = null;
            lastPickupTime = -10f;
            pickupStep = 0;
        }

        public static void Play(SfxId id, float volume = 0.6f)
        {
            PlayPitched(id, volume, 1f + Random.Range(-1f, 1f) * JitterFor(id));
        }

        // Thread spools and golden buttons: each pickup soon after the last one is a step higher.
        public static void PlayPickup(SfxId id, float volume)
        {
            float now = Time.unscaledTime;
            pickupStep = now - lastPickupTime <= PickupWindow ? Mathf.Min(pickupStep + 1, PickupSteps.Length - 1) : 0;
            lastPickupTime = now;
            PlayPitched(id, volume, PickupSteps[pickupStep]);
        }

        static void PlayPitched(SfxId id, float volume, float pitch)
        {
            if (voices == null) return;
            AudioClip clip;
            if (!clips.TryGetValue(id, out clip) || clip == null) return;
            float vol = volume * Volume / 10f;
            if (vol <= 0f) return;

            // Use a free voice if there is one, otherwise cut off the oldest.
            AudioSource v = null;
            for (int i = 0; i < VoiceCount; i++)
            {
                var c = voices[(nextVoice + i) % VoiceCount];
                if (c != null && !c.isPlaying) { v = c; nextVoice = (nextVoice + i + 1) % VoiceCount; break; }
            }
            if (v == null)
            {
                v = voices[nextVoice];
                nextVoice = (nextVoice + 1) % VoiceCount;
            }
            if (v == null) return;
            v.Stop();
            v.clip = clip;
            v.volume = vol;
            v.pitch = pitch;
            v.Play();
        }

        // Small random pitch changes on sounds you hear constantly, so they don't sound copy-pasted.
        // Important cues (pickups, checkpoints, hints) always sound the same so they're recognisable.
        static float JitterFor(SfxId id)
        {
            switch (id)
            {
                case SfxId.Jump:
                case SfxId.Land:
                case SfxId.MenuMove:
                case SfxId.Boing:
                case SfxId.Splash:
                case SfxId.TetherAttach:
                    return 0.04f;
                default:
                    return 0f;
            }
        }

        // Muffle the gameplay loops (used while paused or in the Form Wheel). 22000 = normal.
        public static void SetLoopCutoff(float hz)
        {
            loopCutoffTarget = hz;
        }

        // Ask for a loop to be audible this frame. Call every frame while it should play.
        public static void RequestLoop(LoopId id)
        {
            if (loopTargets != null) loopTargets[(int)id] = 1f;
        }

        // Fades loops in/out. Called once per frame by the game after gameplay has run.
        public static void TickLoops(float dt)
        {
            if (loopSources == null) return;
            loopCutoff = Mathf.Lerp(loopCutoff, loopCutoffTarget, 1f - Mathf.Exp(-12f * dt));
            float master = Volume / 10f;
            for (int i = 0; i < loopSources.Length; i++)
            {
                var s = loopSources[i];
                if (s == null) continue;
                if (loopFilters[i] != null) loopFilters[i].cutoffFrequency = loopCutoff;
                float target = loopTargets[i] * LoopVolumes[i] * master;
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
            // Pop-up chimes, one per element.
            clips[SfxId.HintWater] = Make("hintwater", 0.6f, (t, d) =>
                Bloop(t, 260f, 0.13f) * 0.5f + Bloop(t - 0.1f, 350f, 0.13f) * 0.45f + Bloop(t - 0.2f, 470f, 0.15f) * 0.4f +
                Bell(t - 0.25f, 784f, d) * 0.12f, 3000f);
            clips[SfxId.HintYarn] = Pluck("hintyarn", new float[] { 392f, 523f, 659f }, 0.09f, 0.9f);
            clips[SfxId.HintWind] = WindChime();
            clips[SfxId.HintNeutral] = Make("hintneutral", 0.5f, (t, d) =>
                Bell(t, 523f, d) * 0.3f + Bell(t - 0.09f, 784f, d) * 0.25f, 3500f);
            clips[SfxId.Boing] = Make("boing", 0.45f, (t, d) =>
                Sine(t, 180f + 140f * Mathf.Sin(t * 38f) * Mathf.Exp(-t * 6f) + 160f * (t / d)) * Env(t, d, 0.005f) * 0.55f, 2000f);
            clips[SfxId.Thread] = Make("thread", 0.22f, (t, d) =>
                Bell(t, 1175f, d) * 0.25f + Bell(t - 0.04f, 1568f, d) * 0.15f, 5000f);
            clips[SfxId.PauseOpen] = Pluck("pause", new float[] { 523f, 659f, 784f, 1047f }, 0.05f, 0.8f);
            clips[SfxId.PlatformThunk] = Make("thunk", 0.2f, (t, d) =>
                Sine(t, Mathf.Lerp(120f, 70f, t / d)) * Env(t, d, 0.002f) * 0.4f, 900f);
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

        // Plucked-yarn notes (Karplus-Strong string), played as a little arpeggio.
        static AudioClip Pluck(string name, float[] notes, float gap, float duration)
        {
            int n = Mathf.CeilToInt(duration * Rate);
            var data = new float[n];
            var rng = new System.Random(17);
            for (int k = 0; k < notes.Length; k++)
            {
                int period = Mathf.Max(2, Mathf.RoundToInt(Rate / notes[k]));
                var buf = new float[period];
                for (int i = 0; i < period; i++) buf[i] = (float)(rng.NextDouble() * 2.0 - 1.0);
                int start = Mathf.RoundToInt(k * gap * Rate);
                int idx = 0;
                for (int i = start; i < n; i++)
                {
                    int next = (idx + 1) % period;
                    float v = buf[idx];
                    buf[idx] = 0.497f * (buf[idx] + buf[next]); // soft decay = woolly tone
                    idx = next;
                    data[i] += v * 0.35f * (1f - (i - start) / (float)(n - start + 1));
                }
            }
            LowPass(data, 2600f);
            FadeEdges(data, 0.004f);
            return ToClip(name, data);
        }

        // Breeze + a few soft wind-chime tings.
        static AudioClip WindChime()
        {
            float duration = 0.9f;
            int n = Mathf.CeilToInt(duration * Rate);
            var data = new float[n];
            var rng = new System.Random(23);
            float y = 0f, y2 = 0f;
            float[] tings = { 1319f, 1760f, 1568f };
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float k = t / duration;
                float a = Alpha(Mathf.Lerp(400f, 1100f, Mathf.Sin(Mathf.PI * k)));
                y += a * ((float)(rng.NextDouble() * 2.0 - 1.0) - y);
                y2 += a * (y - y2);
                float breeze = y2 * Mathf.Sin(Mathf.PI * k) * 1.6f;
                float chime = 0f;
                for (int c = 0; c < tings.Length; c++) chime += Bell(t - 0.08f - c * 0.13f, tings[c], duration) * 0.1f;
                data[i] = breeze + chime;
            }
            FadeEdges(data, 0.01f);
            return ToClip("windchime", data);
        }

        // Soft airy whoosh: low-passed noise with a cutoff that swells and settles.
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

        // 2 second loop of little bubbles for riding fountains as Water.
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
                    int idx = (s0 + i) % n; // wrap around so the loop has no gap
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

        // Soft airy loop for gliding.
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
            // Blend the end into the start so the loop doesn't click.
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

        // Soft bell / music-box tone.
        static float Bell(float t, float f, float d)
        {
            if (t < 0f) return 0f;
            float env = Mathf.Clamp01(t / 0.005f) * Mathf.Exp(-t * 5f) * Mathf.Clamp01((d - t) / 0.05f);
            return (Sine(t, f) + 0.25f * Sine(t, f * 2f) + 0.08f * Sine(t, f * 3f)) * env;
        }

        // A single water "bloop": quick upward pitch bend with a fast decay.
        static float Bloop(float t, float f0, float len)
        {
            if (t < 0f || t > len) return 0f;
            float k = t / len;
            float f = f0 * (1f + 1.2f * k);
            return Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Clamp01(t / 0.004f) * (1f - k) * (1f - k);
        }
    }
}

using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// Chill background music, synthesised once at start-up and looped across scenes:
    /// warm electric-piano chords, a round bass, a soft music-box melody and a light brushed beat.
    /// 72 BPM, 8 bars (≈ 27 s) — Fmaj7 · Em7 · Dm9 · Cmaj7 · Fmaj7 · Em7 · Dm7 · G6sus.
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        const string PrefKey = "pw_music_on";
        const int Rate = 32000;
        const float Bpm = 72f;
        const float MaxVolume = 0.32f;

        static MusicPlayer instance;
        static AudioClip clip;

        AudioSource source;
        float targetVolume;

        public static bool Enabled
        {
            get { return PlayerPrefs.GetInt(PrefKey, 1) == 1; }
            set
            {
                PlayerPrefs.SetInt(PrefKey, value ? 1 : 0);
                PlayerPrefs.Save();
                if (instance != null) instance.targetVolume = value ? MaxVolume : 0f;
            }
        }

        /// <summary>Starts the music if it isn't already playing. Survives scene loads.</summary>
        public static void EnsurePlaying()
        {
            if (instance != null) return;
            var go = new GameObject("Patchwork Music");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<MusicPlayer>();
        }

        void Awake()
        {
            if (clip == null) clip = Compose();
            source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0f;
            source.ignoreListenerPause = true;
            source.Play();
            targetVolume = Enabled ? MaxVolume : 0f;
        }

        void Update()
        {
            source.volume = Mathf.MoveTowards(source.volume, targetVolume, Time.unscaledDeltaTime * 0.25f);
        }

        // ------------------------------------------------------------------ Composition

        // MIDI note numbers. Chords voiced low-mid for warmth.
        static readonly int[][] Chords =
        {
            new[] { 53, 57, 60, 64 },   // Fmaj7
            new[] { 52, 55, 59, 62 },   // Em7
            new[] { 50, 53, 57, 60, 64 }, // Dm9
            new[] { 48, 52, 55, 59 },   // Cmaj7
            new[] { 53, 57, 60, 64 },   // Fmaj7
            new[] { 52, 55, 59, 62 },   // Em7
            new[] { 50, 53, 57, 60 },   // Dm7
            new[] { 55, 60, 62, 64 },   // G6sus
        };
        static readonly int[] Bass = { 41, 40, 38, 36, 41, 40, 38, 43 };

        // Melody: (bar, beat, midi, beats). C major pentatonic, sparse and lazy.
        static readonly float[,] Melody =
        {
            { 0, 0.0f, 76, 1.5f }, { 0, 2.0f, 72, 1.0f }, { 0, 3.0f, 74, 1.0f },
            { 1, 0.5f, 71, 1.5f }, { 1, 2.5f, 67, 1.5f },
            { 2, 0.0f, 69, 1.0f }, { 2, 1.0f, 72, 1.0f }, { 2, 2.0f, 76, 2.0f },
            { 3, 1.0f, 74, 1.0f }, { 3, 2.0f, 72, 2.0f },
            { 4, 0.0f, 81, 1.0f }, { 4, 1.0f, 79, 1.0f }, { 4, 2.0f, 76, 2.0f },
            { 5, 0.5f, 74, 1.0f }, { 5, 1.5f, 76, 1.0f }, { 5, 2.5f, 71, 1.5f },
            { 6, 0.0f, 72, 1.5f }, { 6, 2.0f, 69, 1.0f }, { 6, 3.0f, 72, 1.0f },
            { 7, 0.0f, 74, 2.5f }, { 7, 3.0f, 67, 1.0f },
        };

        static float Freq(float midi)
        {
            return 440f * Mathf.Pow(2f, (midi - 69f) / 12f);
        }

        static AudioClip Compose()
        {
            float beat = 60f / Bpm;
            float bar = beat * 4f;
            int bars = Chords.Length;
            float total = bar * bars;
            int n = Mathf.CeilToInt(total * Rate);
            var data = new float[n];

            for (int b = 0; b < bars; b++)
            {
                float barStart = b * bar;

                // Electric-piano chord: hit on beat 1, softer re-hit on the "and" of 3.
                var chord = Chords[b];
                for (int c = 0; c < chord.Length; c++)
                {
                    AddEPiano(data, barStart, bar * 0.95f, Freq(chord[c]), 0.075f);
                    AddEPiano(data, barStart + beat * 2.5f, beat * 1.4f, Freq(chord[c]), 0.04f);
                }

                // Round bass on 1 and 3.
                AddBass(data, barStart, beat * 1.8f, Freq(Bass[b]), 0.22f);
                AddBass(data, barStart + beat * 2f, beat * 1.6f, Freq(Bass[b] + (b % 2 == 0 ? 7 : 0)), 0.16f);

                // Brushed beat: soft "tss" on 2 and 4, very quiet shaker on off-beats.
                for (int q = 0; q < 4; q++)
                {
                    float t0 = barStart + q * beat;
                    if (q == 1 || q == 3) AddBrush(data, t0, 0.18f, 0.05f, 1000 + b * 10 + q);
                    AddBrush(data, t0 + beat * 0.5f, 0.06f, 0.018f, 2000 + b * 10 + q);
                }
                // A soft felt "kick" on 1.
                AddKick(data, barStart, 0.12f);
            }

            // Music-box melody.
            for (int i = 0; i < Melody.GetLength(0); i++)
            {
                float t0 = Melody[i, 0] * bar + Melody[i, 1] * beat;
                AddMusicBox(data, t0, Melody[i, 3] * beat + 0.6f, Freq(Melody[i, 2]), 0.07f);
            }

            // Warm it up: gentle low-pass + soft saturation, then wrap the tail into the start.
            LowPass(data, 4200f);
            float peak = 0.0001f;
            for (int i = 0; i < n; i++) peak = Mathf.Max(peak, Mathf.Abs(data[i]));
            float gain = 0.85f / peak;
            for (int i = 0; i < n; i++)
            {
                float x = data[i] * gain;
                data[i] = x / (1f + Mathf.Abs(x) * 0.3f);
            }

            var result = AudioClip.Create("pw_music", n, 1, Rate, false);
            result.SetData(data, 0);
            return result;
        }

        static void AddEPiano(float[] data, float start, float dur, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt((dur + 0.5f) * Rate);
            float detune = 1.003f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.01f) * Mathf.Exp(-t * 1.6f) * Mathf.Clamp01((dur + 0.5f - t) / 0.5f);
                float w = Mathf.Sin(2f * Mathf.PI * f * t) * 0.7f
                        + Mathf.Sin(2f * Mathf.PI * f * detune * t) * 0.3f
                        + Mathf.Sin(2f * Mathf.PI * f * 2f * t) * 0.12f * Mathf.Exp(-t * 4f);
                // Slow tremolo for that lo-fi wobble.
                w *= 1f + 0.08f * Mathf.Sin(2f * Mathf.PI * 4.2f * t);
                Add(data, s0 + i, w * env * vol);
            }
        }

        static void AddBass(float[] data, float start, float dur, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(dur * Rate);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.015f) * Mathf.Exp(-t * 1.2f) * Mathf.Clamp01((dur - t) / 0.1f);
                float w = Mathf.Sin(2f * Mathf.PI * f * t) + 0.2f * Mathf.Sin(2f * Mathf.PI * f * 2f * t);
                Add(data, s0 + i, w * env * vol);
            }
        }

        static void AddMusicBox(float[] data, float start, float dur, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(dur * Rate);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.003f) * Mathf.Exp(-t * 2.8f) * Mathf.Clamp01((dur - t) / 0.1f);
                float w = Mathf.Sin(2f * Mathf.PI * f * t) + 0.35f * Mathf.Sin(2f * Mathf.PI * f * 3f * t) * Mathf.Exp(-t * 6f);
                Add(data, s0 + i, w * env * vol);
            }
        }

        static void AddBrush(float[] data, float start, float dur, float vol, int seed)
        {
            var rng = new System.Random(seed);
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(dur * Rate);
            float lp = 0f, prev = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float x = (float)(rng.NextDouble() * 2.0 - 1.0);
                lp += 0.35f * (x - lp);
                float hp = lp - prev; // remove rumble
                prev = lp;
                float env = Mathf.Clamp01(t / 0.004f) * Mathf.Pow(1f - t / dur, 3f);
                Add(data, s0 + i, hp * env * vol * 2.5f);
            }
        }

        static void AddKick(float[] data, float start, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(0.25f * Rate);
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float f = Mathf.Lerp(110f, 50f, Mathf.Clamp01(t / 0.08f));
                phase += 2f * Mathf.PI * f / Rate;
                float env = Mathf.Clamp01(t / 0.003f) * Mathf.Exp(-t * 14f);
                Add(data, s0 + i, Mathf.Sin(phase) * env * vol);
            }
        }

        static void Add(float[] data, int index, float v)
        {
            // Wrap so notes ringing past the end blend into the loop start.
            int n = data.Length;
            index %= n;
            if (index < 0) index += n;
            data[index] += v;
        }

        static void LowPass(float[] data, float cutoffHz)
        {
            float rc = 1f / (2f * Mathf.PI * cutoffHz);
            float dt = 1f / Rate;
            float a = dt / (rc + dt);
            float y = 0f;
            // Two passes over the loop so the filter state is continuous at the seam.
            for (int pass = 0; pass < 2; pass++)
                for (int i = 0; i < data.Length; i++)
                {
                    y += a * (data[i] - y);
                    if (pass == 1) data[i] = y;
                }
        }
    }
}

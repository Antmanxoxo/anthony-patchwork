using UnityEngine;

namespace Patchwork
{
    // Background music. Built once when the game starts and loops between scenes.
    // Lo-fi style: e-piano chords, bass and soft brushes underneath, with the tune played by a
    // different instrument for each form, crossfading when Patch changes:
    //   Water = music box with little bubble blips, Yarn = plucked strings, Wind = breathy flute and air.
    // 72 BPM, 8 bars: Fmaj7 Em7 Dm9 Cmaj7 Fmaj7 Em7 Dm7 G6sus
    public class MusicPlayer : MonoBehaviour
    {
        const string OldOnKey = "pw_music_on";
        const string VolumeKey = "pw_music_volume";
        const int Rate = 32000;
        const float Bpm = 72f;
        const float MaxVolume = 0.32f;
        const float LayerFade = 1.5f;   // seconds to crossfade between form layers

        enum Stem { Base, Water, Yarn, Wind }
        const int StemCount = 4;

        static MusicPlayer instance;
        static AudioClip[] stems;

        readonly AudioSource[] sources = new AudioSource[StemCount];
        readonly AudioLowPassFilter[] filters = new AudioLowPassFilter[StemCount];
        readonly float[] layerMix = { 1f, 0f, 1f, 0f };   // 0..1 crossfade position per stem
        FormType form = FormType.Yarn;
        float master;
        float duck = 1f, duckTarget = 1f;
        float cutoff = 22000f, cutoffTarget = 22000f;

        // Music volume, 0..10 (saved). 0 means off.
        public static int Volume
        {
            get
            {
                // Older saves only had an on/off switch.
                int fallback = PlayerPrefs.GetInt(OldOnKey, 1) == 1 ? 10 : 0;
                return Mathf.Clamp(PlayerPrefs.GetInt(VolumeKey, fallback), 0, 10);
            }
            set
            {
                PlayerPrefs.SetInt(VolumeKey, Mathf.Clamp(value, 0, 10));
                PlayerPrefs.Save();
            }
        }

        // Paused / Satchel: quieter and "behind glass". Form Wheel: muffled. Normal: clear.
        public static void SetMuffle(float cutoffHz, float volumeScale)
        {
            if (instance == null) return;
            instance.cutoffTarget = cutoffHz;
            instance.duckTarget = volumeScale;
        }

        // Which form's tune layer should be playing. Safe to call every frame.
        public static void SetForm(FormType f)
        {
            if (instance != null) instance.form = f;
        }

        // Starts the music if it isn't already playing. Survives scene loads.
        public static void EnsurePlaying()
        {
            if (instance != null) return;
            var go = new GameObject("Patchwork Music");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<MusicPlayer>();
        }

        void Awake()
        {
            if (stems == null || stems[0] == null) stems = Compose();
            // Start straight on the saved form's layer (no fade-in from another one).
            form = GameState.CurrentForm;
            for (int i = 1; i < StemCount; i++) layerMix[i] = StemFor(form) == i ? 1f : 0f;
            // All layers start on the same audio tick so they stay perfectly in time.
            double start = AudioSettings.dspTime + 0.2;
            for (int i = 0; i < StemCount; i++)
            {
                var go = new GameObject("Music " + (Stem)i);
                go.transform.SetParent(transform, false);
                var src = go.AddComponent<AudioSource>();
                src.clip = stems[i];
                src.loop = true;
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                src.volume = 0f;
                src.ignoreListenerPause = true;
                filters[i] = go.AddComponent<AudioLowPassFilter>();
                filters[i].cutoffFrequency = 22000f;
                sources[i] = src;
                src.PlayScheduled(start);
            }
        }

        void Update()
        {
            float udt = Time.unscaledDeltaTime;
            // Ease the muffle over about 0.3 s, in real time so it works while the game is paused.
            float k = 1f - Mathf.Exp(-10f * udt);
            cutoff = Mathf.Lerp(cutoff, cutoffTarget, k);
            duck = Mathf.Lerp(duck, duckTarget, k);

            float target = MaxVolume * Volume / 10f * duck;
            master = Mathf.MoveTowards(master, target, udt * 0.6f);

            for (int i = 0; i < StemCount; i++)
            {
                if (sources[i] == null) continue;
                if (i != (int)Stem.Base)
                {
                    bool on = StemFor(form) == i;
                    layerMix[i] = Mathf.MoveTowards(layerMix[i], on ? 1f : 0f, udt / LayerFade);
                }
                // Equal-power curve so the overall loudness stays even mid-crossfade.
                float gain = i == (int)Stem.Base ? 1f : Mathf.Sin(layerMix[i] * Mathf.PI * 0.5f);
                sources[i].volume = master * gain;
                filters[i].cutoffFrequency = cutoff;
            }
        }

        static int StemFor(FormType f)
        {
            switch (f)
            {
                case FormType.Water: return (int)Stem.Water;
                case FormType.Wind: return (int)Stem.Wind;
                default: return (int)Stem.Yarn;
            }
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

        static AudioClip[] Compose()
        {
            float beat = 60f / Bpm;
            float bar = beat * 4f;
            int bars = Chords.Length;
            float total = bar * bars;
            int n = Mathf.CeilToInt(total * Rate);
            var data = new float[StemCount][];
            for (int i = 0; i < StemCount; i++) data[i] = new float[n];
            var bed = data[(int)Stem.Base];

            for (int b = 0; b < bars; b++)
            {
                float barStart = b * bar;

                // Electric-piano chord: hit on beat 1, softer re-hit on the "and" of 3.
                var chord = Chords[b];
                for (int c = 0; c < chord.Length; c++)
                {
                    AddEPiano(bed, barStart, bar * 0.95f, Freq(chord[c]), 0.075f);
                    AddEPiano(bed, barStart + beat * 2.5f, beat * 1.4f, Freq(chord[c]), 0.04f);
                }

                // Round bass on 1 and 3.
                AddBass(bed, barStart, beat * 1.8f, Freq(Bass[b]), 0.22f);
                AddBass(bed, barStart + beat * 2f, beat * 1.6f, Freq(Bass[b] + (b % 2 == 0 ? 7 : 0)), 0.16f);

                // Brushed beat: soft "tss" on 2 and 4, very quiet shaker on off-beats.
                for (int q = 0; q < 4; q++)
                {
                    float t0 = barStart + q * beat;
                    if (q == 1 || q == 3) AddBrush(bed, t0, 0.18f, 0.05f, 1000 + b * 10 + q);
                    AddBrush(bed, t0 + beat * 0.5f, 0.06f, 0.018f, 2000 + b * 10 + q);
                }
                // A soft felt "kick" on 1.
                AddKick(bed, barStart, 0.12f);

                // Wind: a breathy air pad that swells on each chord.
                AddAirPad(data[(int)Stem.Wind], barStart, bar, 0.035f, 3000 + b);
            }

            // The tune, played three ways.
            for (int i = 0; i < Melody.GetLength(0); i++)
            {
                float t0 = Melody[i, 0] * bar + Melody[i, 1] * beat;
                float f = Freq(Melody[i, 2]);
                float len = Melody[i, 3] * beat;
                // Water: music box, plus a tiny rising "bloop" an octave up.
                AddMusicBox(data[(int)Stem.Water], t0, len + 0.6f, f, 0.07f);
                AddBloop(data[(int)Stem.Water], t0 + beat * 0.5f, f * 2f, 0.02f);
                // Yarn: plucked string, like a little ukulele.
                AddPluck(data[(int)Stem.Yarn], t0, len + 0.8f, f, 0.12f, 4000 + i);
                // Wind: soft flute with a gentle vibrato.
                AddFlute(data[(int)Stem.Wind], t0, len + 0.15f, f, 0.06f);
            }

            // Warm everything with the same filter and one shared gain, so the layers sit
            // at the same level as each other and as the old single track.
            for (int i = 0; i < StemCount; i++) LowPass(data[i], 4200f);
            float peak = 0.0001f;
            for (int s = 0; s < n; s++)
            {
                float loudest = 0f;
                for (int i = 1; i < StemCount; i++) loudest = Mathf.Max(loudest, Mathf.Abs(data[i][s]));
                peak = Mathf.Max(peak, Mathf.Abs(bed[s]) + loudest);
            }
            float gain = 0.85f / peak;
            var result = new AudioClip[StemCount];
            for (int i = 0; i < StemCount; i++)
            {
                var d = data[i];
                for (int s = 0; s < n; s++)
                {
                    float x = d[s] * gain;
                    d[s] = i == (int)Stem.Base ? x / (1f + Mathf.Abs(x) * 0.3f) : x;
                }
                result[i] = AudioClip.Create("pw_music_" + ((Stem)i).ToString().ToLower(), n, 1, Rate, false);
                result[i].SetData(d, 0);
            }
            return result;
        }

        // Plucked string (Karplus-Strong): a burst of noise fed round a short, damped delay line.
        static void AddPluck(float[] data, float start, float dur, float f, float vol, int seed)
        {
            var rng = new System.Random(seed);
            int period = Mathf.Max(2, Mathf.RoundToInt(Rate / f));
            var ring = new float[period];
            for (int i = 0; i < period; i++) ring[i] = (float)(rng.NextDouble() * 2.0 - 1.0);
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(dur * Rate);
            int idx = 0;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                int next = (idx + 1) % period;
                float y = ring[idx];
                ring[idx] = 0.5f * (ring[idx] + ring[next]) * 0.996f;
                idx = next;
                float env = Mathf.Clamp01((dur - t) / 0.08f);
                Add(data, s0 + i, y * env * vol);
            }
        }

        // Soft flute: a sine with a slow-starting vibrato and a little breath noise.
        static void AddFlute(float[] data, float start, float dur, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(dur * Rate);
            float phase = 0f;
            uint noise = 22222u;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float vib = 1f + 0.006f * Mathf.Sin(2f * Mathf.PI * 5f * t) * Mathf.Clamp01(t / 0.3f);
                phase += 2f * Mathf.PI * f * vib / Rate;
                noise = noise * 1664525u + 1013904223u;
                float breath = ((noise >> 9) / 4194304f - 1f) * 0.04f;
                float env = Mathf.Clamp01(t / 0.08f) * Mathf.Clamp01((dur - t) / 0.12f);
                float w = Mathf.Sin(phase) + 0.15f * Mathf.Sin(phase * 2f) + breath;
                Add(data, s0 + i, w * env * vol);
            }
        }

        // Quiet wash of filtered noise that swells in and out over a bar.
        static void AddAirPad(float[] data, float start, float dur, float vol, int seed)
        {
            var rng = new System.Random(seed);
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(dur * Rate);
            float lp = 0f, lp2 = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float x = (float)(rng.NextDouble() * 2.0 - 1.0);
                lp += 0.05f * (x - lp);
                lp2 += 0.05f * (lp - lp2);
                float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / dur));
                Add(data, s0 + i, lp2 * env * vol * 6f);
            }
        }

        // Tiny upward "bloop", like a bubble rising.
        static void AddBloop(float[] data, float start, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(0.09f * Rate);
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                phase += 2f * Mathf.PI * f * Mathf.Lerp(0.8f, 1.25f, t / 0.09f) / Rate;
                float env = Mathf.Clamp01(t / 0.004f) * Mathf.Exp(-t * 35f);
                Add(data, s0 + i, Mathf.Sin(phase) * env * vol);
            }
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

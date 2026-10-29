using UnityEngine;

namespace Patchwork
{
    public enum MusicTrack { Skiff, Toybox }

    // Toybox Town's own soundtrack: a Renaissance-style galliard in D major, 3/4 at 108 BPM.
    // It's built in layers that match the town band:
    //   Bed     - harpsichord "oom-pah-pah" and the drummer's tabor and tambourine (always on;
    //             the drummer is the only one whose instrument still works)
    //   Lute    - rippling lute arpeggios (on once the lute's string is found)
    //   Trumpet - a brassy fanfare and harmony (on once the trumpet's valves are oiled)
    //   Pipe    - the melody on a sweet wooden pipe (on once the piper is home)
    public partial class MusicPlayer
    {
        enum ToyStem { Bed, Lute, Trumpet, Pipe }
        const int ToyStemCount = 4;
        const float ToyBpm = 108f;

        static AudioClip[] toyStems;
        AudioSource[] toySources;
        AudioLowPassFilter[] toyFilters;
        MusicTrack track = MusicTrack.Skiff;
        float toyMix;
        readonly float[] toyLayer = { 1f, 0f, 0f, 0f };
        readonly bool[] toyWant = { true, false, false, false };

        public static void SetTrack(MusicTrack t)
        {
            if (instance == null) return;
            instance.track = t;
            if (t == MusicTrack.Toybox) instance.StartToybox();
        }

        // Which band layers are playing in Toybox Town.
        public static void SetBand(bool lute, bool trumpet, bool pipe)
        {
            if (instance == null) return;
            instance.toyWant[(int)ToyStem.Lute] = lute;
            instance.toyWant[(int)ToyStem.Trumpet] = trumpet;
            instance.toyWant[(int)ToyStem.Pipe] = pipe;
        }

        // Jump the band layers straight to their state (no fade), e.g. when the level loads.
        public static void SnapBand()
        {
            if (instance == null) return;
            for (int i = 1; i < ToyStemCount; i++) instance.toyLayer[i] = instance.toyWant[i] ? 1f : 0f;
        }

        void StartToybox()
        {
            if (toySources != null) return;
            if (toyStems == null || toyStems[0] == null) toyStems = ComposeToybox();
            toySources = new AudioSource[ToyStemCount];
            toyFilters = new AudioLowPassFilter[ToyStemCount];
            double start = AudioSettings.dspTime + 0.15;
            for (int i = 0; i < ToyStemCount; i++)
            {
                var go = new GameObject("Music Toybox " + (ToyStem)i);
                go.transform.SetParent(transform, false);
                var src = go.AddComponent<AudioSource>();
                src.clip = toyStems[i];
                src.loop = true;
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                src.volume = 0f;
                src.ignoreListenerPause = true;
                toyFilters[i] = go.AddComponent<AudioLowPassFilter>();
                toySources[i] = src;
                src.PlayScheduled(start);
            }
        }

        // Fades between the Skiff music and the level track. Returns how loud the Skiff music should be.
        float UpdateLevelTrack(float udt)
        {
            float want = track == MusicTrack.Toybox ? 1f : 0f;
            toyMix = Mathf.MoveTowards(toyMix, want, udt / 1.5f);
            if (toySources != null)
            {
                for (int i = 0; i < ToyStemCount; i++)
                {
                    if (toySources[i] == null) continue;
                    toyLayer[i] = Mathf.MoveTowards(toyLayer[i], toyWant[i] ? 1f : 0f, udt / 2f);
                    float gain = Mathf.Sin(toyLayer[i] * Mathf.PI * 0.5f);
                    toySources[i].volume = master * gain * Mathf.Sin(toyMix * Mathf.PI * 0.5f);
                    toyFilters[i].cutoffFrequency = cutoff;
                }
                // Back on the Skiff and fully faded: stop the town band.
                if (toyMix <= 0f && track != MusicTrack.Toybox)
                {
                    foreach (var s in toySources) if (s != null) Destroy(s.gameObject);
                    toySources = null;
                }
            }
            return Mathf.Cos(toyMix * Mathf.PI * 0.5f);
        }

        // ------------------------------------------------------------------ Composition

        static readonly int[][] ToyChords =
        {
            new[] { 62, 66, 69 }, new[] { 62, 67, 71 }, new[] { 62, 66, 69 }, new[] { 61, 64, 69 },   // D G D A
            new[] { 62, 66, 71 }, new[] { 62, 67, 71 }, new[] { 61, 64, 69 }, new[] { 62, 66, 69 },   // Bm G A D
            new[] { 62, 67, 71 }, new[] { 62, 66, 69 }, new[] { 64, 67, 71 }, new[] { 61, 64, 69 },   // G D Em A
            new[] { 62, 66, 69 }, new[] { 62, 66, 71 }, new[] { 61, 64, 69 }, new[] { 62, 66, 69 },   // D Bm A D
        };
        static readonly int[] ToyBass = { 50, 43, 50, 45, 47, 43, 45, 50, 43, 50, 40, 45, 50, 47, 45, 50 };

        // (bar, beat, midi, beats)
        static readonly float[,] ToyMelody =
        {
            { 0, 0f, 74, 1.5f }, { 0, 1.5f, 76, 0.5f }, { 0, 2f, 78, 1f },
            { 1, 0f, 79, 1f }, { 1, 1f, 78, 1f }, { 1, 2f, 76, 1f },
            { 2, 0f, 74, 1.5f }, { 2, 1.5f, 73, 0.5f }, { 2, 2f, 74, 1f },
            { 3, 0f, 76, 2f }, { 3, 2f, 69, 1f },
            { 4, 0f, 71, 1.5f }, { 4, 1.5f, 73, 0.5f }, { 4, 2f, 74, 1f },
            { 5, 0f, 76, 1f }, { 5, 1f, 74, 1f }, { 5, 2f, 71, 1f },
            { 6, 0f, 73, 1f }, { 6, 1f, 76, 1f }, { 6, 2f, 79, 1f },
            { 7, 0f, 78, 2f }, { 7, 2f, 74, 1f },
            { 8, 0f, 79, 1.5f }, { 8, 1.5f, 78, 0.5f }, { 8, 2f, 76, 1f },
            { 9, 0f, 74, 1f }, { 9, 1f, 78, 1f }, { 9, 2f, 81, 1f },
            { 10, 0f, 79, 1.5f }, { 10, 1.5f, 78, 0.5f }, { 10, 2f, 76, 1f },
            { 11, 0f, 73, 2f }, { 11, 2f, 69, 1f },
            { 12, 0f, 74, 1.5f }, { 12, 1.5f, 76, 0.5f }, { 12, 2f, 78, 1f },
            { 13, 0f, 81, 1f }, { 13, 1f, 78, 1f }, { 13, 2f, 74, 1f },
            { 14, 0f, 76, 1f }, { 14, 1f, 73, 1f }, { 14, 2f, 69, 1f },
            { 15, 0f, 74, 3f },
        };

        static AudioClip[] ComposeToybox()
        {
            float beat = 60f / ToyBpm;
            float bar = beat * 3f;
            int bars = ToyChords.Length;
            int n = Mathf.CeilToInt(bar * bars * Rate);
            var data = new float[ToyStemCount][];
            for (int i = 0; i < ToyStemCount; i++) data[i] = new float[n];
            var bed = data[(int)ToyStem.Bed];
            var lute = data[(int)ToyStem.Lute];
            var trumpet = data[(int)ToyStem.Trumpet];
            var pipe = data[(int)ToyStem.Pipe];

            for (int b = 0; b < bars; b++)
            {
                float t0 = b * bar;
                var ch = ToyChords[b];
                // Harpsichord: bass on 1, chord on 2 and 3, each chord quickly strummed.
                AddHarpsichord(bed, t0, beat * 1.2f, Freq(ToyBass[b]), 0.1f, 9000 + b);
                AddHarpsichord(bed, t0, beat * 1.2f, Freq(ToyBass[b] - 12), 0.06f, 9100 + b);
                for (int q = 1; q < 3; q++)
                    for (int c = 0; c < ch.Length; c++)
                        AddHarpsichord(bed, t0 + q * beat + c * 0.012f, beat * 0.8f, Freq(ch[c] - 12), 0.045f, 9200 + b * 8 + q * 3 + c);
                // The drummer: a tabor thump on 1, a lighter one on 3, a tambourine shake on 2.
                AddKick(bed, t0, 0.16f);
                AddKick(bed, t0 + beat * 2f, 0.08f);
                AddBrush(bed, t0 + beat, 0.16f, 0.05f, 9300 + b);
                AddBrush(bed, t0 + beat * 2.5f, 0.08f, 0.025f, 9400 + b);

                // Lute: six-note arpeggio per bar, up and back down the chord.
                int[] order = { 0, 1, 2, 1, 2, 1 };
                for (int e = 0; e < 6; e++)
                    AddPluck(lute, t0 + e * beat * 0.5f, beat * 1.4f, Freq(ch[order[e]] - (e == 0 ? 12 : 0)), 0.08f, 9500 + b * 6 + e);

                // Trumpet: a fanfare at the start of each phrase, otherwise a held harmony note.
                if (b % 4 == 0)
                {
                    AddBrass(trumpet, t0, beat * 0.7f, Freq(ch[0]), 0.07f);
                    AddBrass(trumpet, t0 + beat * 0.75f, beat * 0.25f, Freq(ch[0]), 0.06f);
                    AddBrass(trumpet, t0 + beat, beat * 1.9f, Freq(ch[2]), 0.075f);
                }
                else
                    AddBrass(trumpet, t0, beat * 2.6f, Freq(ch[1] - 12), 0.045f);
            }

            // Pipe: the tune.
            for (int i = 0; i < ToyMelody.GetLength(0); i++)
            {
                float t0 = ToyMelody[i, 0] * bar + ToyMelody[i, 1] * beat;
                float len = ToyMelody[i, 3] * beat;
                AddFlute(pipe, t0, len + 0.08f, Freq(ToyMelody[i, 2]), 0.08f);
                // A quick grace note before long notes, like a Renaissance recorder.
                if (ToyMelody[i, 3] >= 1.5f) AddFlute(pipe, t0 - 0.06f, 0.06f, Freq(ToyMelody[i, 2] + 2), 0.04f);
            }

            for (int i = 0; i < ToyStemCount; i++) LowPass(data[i], 5200f);
            float peak = 0.0001f;
            for (int s = 0; s < n; s++)
            {
                float sum = 0f;
                for (int i = 0; i < ToyStemCount; i++) sum += Mathf.Abs(data[i][s]);
                peak = Mathf.Max(peak, sum);
            }
            float gain = 0.9f / peak;
            var result = new AudioClip[ToyStemCount];
            for (int i = 0; i < ToyStemCount; i++)
            {
                var d = data[i];
                for (int s = 0; s < n; s++) d[s] *= gain;
                result[i] = AudioClip.Create("pw_toybox_" + ((ToyStem)i).ToString().ToLower(), n, 1, Rate, false);
                result[i].SetData(d, 0);
            }
            return result;
        }

        // Harpsichord: a bright plucked string with a quick buzzy second partial.
        static void AddHarpsichord(float[] data, float start, float dur, float f, float vol, int seed)
        {
            AddPluck(data, start, dur, f, vol, seed);
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(Mathf.Min(dur, 0.4f) * Rate);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float w = Mathf.Sin(2f * Mathf.PI * f * 2f * t) * 0.5f + Mathf.Sin(2f * Mathf.PI * f * 3f * t) * 0.3f;
                Add(data, s0 + i, w * Mathf.Exp(-t * 14f) * vol * 0.5f);
            }
        }

        // Brass: a stack of harmonics that brighten as the note swells, with a little vibrato.
        static void AddBrass(float[] data, float start, float dur, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt((dur + 0.08f) * Rate);
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float att = Mathf.Clamp01(t / 0.05f);
                float rel = Mathf.Clamp01((dur + 0.08f - t) / 0.08f);
                float vib = 1f + 0.006f * Mathf.Sin(2f * Mathf.PI * 5.5f * t) * Mathf.Clamp01((t - 0.2f) * 3f);
                phase += 2f * Mathf.PI * f * vib / Rate;
                float bright = 0.4f + 0.6f * att;
                float w = 0f;
                for (int h = 1; h <= 8; h++) w += Mathf.Sin(phase * h) * Mathf.Pow(bright, h - 1) / h;
                Add(data, s0 + i, w * att * rel * vol);
            }
        }
    }
}

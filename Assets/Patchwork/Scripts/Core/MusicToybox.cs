using UnityEngine;

namespace Patchwork
{
    public enum MusicTrack { Skiff, Toybox, Pod }

    // Toybox Town's own soundtrack: a sweeping orchestral adventure theme (see the notes on
    // ComposeToybox). It's built in layers that match the town band:
    //   Bed     - always on (the drummer's the only one whose instrument still works)
    //   Lute    - on once the lute's string is found
    //   Trumpet - on once the trumpet's valves are oiled
    //   Pipe    - on once the piper is home
    public partial class MusicPlayer
    {
        enum ToyStem { Bed, Lute, Trumpet, Pipe }
        const int ToyStemCount = 4;
        public const float ToyBpm = 132f;

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
            if (t == MusicTrack.Pod) instance.StartPod();
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
        //
        // A sweeping orchestral adventure theme in D major, 4/4 at 132 BPM, 40 bars:
        //   Intro (1-4)    harp and celesta twinkling, strings swelling, a timpani roll in
        //   A     (5-12)   the main tune: big upward leaps and long held notes over a
        //                  falling bass line
        //   A'    (13-20)  again, lifting through two borrowed chords (Bb, C) into...
        //   B     (21-28)  the big soaring section: the tune an octave up, horns singing
        //                  underneath, timpani on every bar
        //   C     (29-36)  a quiet, starry interlude: celesta, harp and low flute, no drums
        //   Coda  (37-40)  the head of the tune once more, grandly, and round again
        // The bed (always playing) is already the whole piece: bass, a pattering string
        // ostinato, soft orchestral drums, held strings, and the tune on strings with a
        // music box, glockenspiel or celesta on top. Each band member found adds a layer:
        //   Lute    - harp arpeggios running up and down, and glissandos into each section
        //   Trumpet - horn pads, fanfares, and the countermelody under the soaring section
        //   Pipe    - the flute singing the tune, with trills into each new section

        static readonly string[] ToyChords =
        {
            "D", "G", "D", "A7",
            "D", "A", "Bm", "F#m", "G", "D", "Em7", "A7",
            "D", "A", "Bm", "F#m", "G", "D", "Bb", "C",
            "G", "A", "F#m", "Bm", "G", "A", "D", "D",
            "Bm", "G", "D", "A", "Bm", "G", "Em7", "A7",
            "D", "G", "D A7", "D",
        };

        // One bar per entry, lengths in beats (4 a bar).
        static readonly string[] ToyTune =
        {
            "r:4", "r:4", "r:2 A4:1 D5:1", "E5:1 G5:1 A5:1 C#6:1",

            "A4:1 D5:1 F#5:1.5 A5:0.5", "A5:2 G5:0.5 F#5:0.5 E5:1", "D5:1 F#5:1 B5:1.5 A5:0.5", "A5:2 F#5:1 C#5:1",
            "B4:1 D5:1 G5:1.5 B5:0.5", "A5:2 F#5:1 D5:1", "E5:1 G5:1 B5:1 D6:1", "C#6:1.5 B5:0.5 A5:1 E5:1",

            "A4:1 D5:1 F#5:1.5 A5:0.5", "A5:2 G5:0.5 F#5:0.5 E5:1", "D5:1 F#5:1 B5:1.5 A5:0.5", "A5:2 F#5:1 C#5:1",
            "B4:1 D5:1 G5:1.5 B5:0.5", "A5:2 F#5:1 D5:1", "D5:1 F5:1 Bb5:1 D6:1", "E6:1.5 D6:0.5 C6:1 E6:1",

            "D6:1.5 B5:0.5 G5:1 B5:1", "C#6:1.5 A5:0.5 E5:1 A5:1", "A5:1 C#6:1 F#6:2", "F#6:1 E6:0.5 D6:0.5 B5:2",
            "D6:1.5 B5:0.5 G5:1 B5:1", "C#6:1 E6:1 A6:1.5 G6:0.5", "F#6:3 D6:1", "A5:1 D6:1 F#6:1 A6:1",

            "F#5:1 B5:1 D6:2", "D6:1 B5:1 G5:2", "F#5:1 A5:1 D6:1.5 E6:0.5", "E6:2 C#6:1 A5:1",
            "F#5:1 B5:1 D6:1 F#6:1", "G6:1.5 F#6:0.5 D6:1 B5:1", "G5:1 B5:1 E6:1 G6:1", "A6:2 G6:1 E6:1",

            "A5:1 D6:1 F#6:1.5 A6:0.5", "B6:2 G6:1 D6:1", "F#6:1 D6:1 E6:1 C#6:1", "D6:3 r:1",
        };

        // The horns' countermelody under the soaring section (bars 21-28).
        static readonly string[] ToyBrassC =
        {
            "B4:4", "A4:2 C#5:2", "C#5:2 A4:2", "D5:2 B4:2", "B4:2 D5:2", "E5:2 C#5:2", "D5:2 A4:2", "F#4:2 A4:2",
        };

        // Which part of the piece a bar belongs to: 0 intro, 1 A, 2 A', 3 B (soaring), 4 C (quiet), 5 coda.
        static int ToySection(int bar)
        {
            return bar < 4 ? 0 : bar < 12 ? 1 : bar < 20 ? 2 : bar < 28 ? 3 : bar < 36 ? 4 : 5;
        }

        static AudioClip[] ComposeToybox()
        {
            float beat = 60f / ToyBpm;
            float bar = beat * 4f;
            int bars = ToyChords.Length;
            int n = Mathf.CeilToInt(bar * bars * Rate);
            var data = new float[ToyStemCount][];
            for (int i = 0; i < ToyStemCount; i++) data[i] = new float[n];
            var bed = data[(int)ToyStem.Bed];
            var lute = data[(int)ToyStem.Lute];
            var brass = data[(int)ToyStem.Trumpet];
            var pipe = data[(int)ToyStem.Pipe];

            for (int b = 0; b < bars; b++)
            {
                int section = ToySection(b);
                bool quiet = section == 0 || section == 4;
                bool big = section == 3 || section == 5;
                bool sectionStart = b == 4 || b == 12 || b == 20 || b == 28 || b == 36;
                bool sectionEnd = b == 3 || b == 11 || b == 19 || b == 27 || b == 35 || b == bars - 1;
                float b0 = b * 4f;
                var names = ToyChords[b].Split(' ');
                string first = names[0], second = names.Length > 1 ? names[1] : names[0];

                // ----- Bed -----
                // Bass: root on 1, fifth on 3 (pizzicato in the quiet parts, bowed otherwise).
                int r1 = Root(first, 38), r2 = Root(second, 38);
                if (quiet)
                {
                    AddPizz(bed, b0 * beat, Freq(r1), 0.1f, 1000 + b * 4);
                    AddPizz(bed, (b0 + 2f) * beat, Freq(first == second ? r1 + 7 : r2), 0.08f, 1001 + b * 4);
                }
                else
                {
                    AddBass(bed, b0 * beat, beat * 1.8f, Freq(r1), 0.1f);
                    AddBass(bed, (b0 + 2f) * beat, beat * 1.8f, Freq(first == second ? r1 + 7 : r2), 0.085f);
                    AddPizz(bed, (b0 + 3.5f) * beat, Freq(r2 + 7), 0.05f, 1002 + b * 4);
                }

                for (int half = 0; half < 2; half++)
                {
                    string name = half == 0 ? first : second;
                    var ch = Ch(name, 57);
                    float h0 = b0 + half * 2f;
                    // Held strings: the warm cushion under everything (fuller in the big parts).
                    foreach (int c in ch) AddPad(bed, h0 * beat, beat * 2f, Freq(c - 12), big ? 0.022f : 0.016f);
                    if (big) AddPad(bed, h0 * beat, beat * 2f, Freq(ch[0] - 24), 0.018f);
                    if (!quiet)
                    {
                        // The pattering string ostinato: light plucked off-beats that keep it galloping.
                        for (int e8 = 0; e8 < 4; e8++)
                        {
                            if (e8 % 2 == 0) AddPizz(bed, (h0 + e8 * 0.5f + 0.25f) * beat, Freq(ch[(e8 / 2) % ch.Length]), 0.028f, 1500 + b * 16 + half * 8 + e8);
                            else foreach (int c in ch) AddPizz(bed, (h0 + e8 * 0.5f) * beat, Freq(c), 0.022f, 1600 + b * 16 + half * 8 + e8 + c);
                        }
                    }
                    else
                    {
                        // Celesta rippling up and back down the chord, like stars coming out.
                        int[] order = { 0, 1, 2, 1 };
                        for (int e8 = 0; e8 < 4; e8++) AddCelesta(bed, (h0 + e8 * 0.5f) * beat, beat * 0.7f, Freq(ch[order[e8] % ch.Length] + 12), 0.018f);
                    }
                }

                // Drums: soft and orchestral. A little march on the snare, brushes, and timpani
                // marking the big moments. None at all in the quiet interlude.
                if (!quiet)
                {
                    AddKick(bed, b0 * beat, 0.09f);
                    AddKick(bed, (b0 + 2f) * beat, 0.05f);
                    AddSnare(bed, (b0 + 1f) * beat, 0.03f, 1100 + b);
                    AddSnare(bed, (b0 + 3f) * beat, 0.035f, 1200 + b);
                    AddSnare(bed, (b0 + 3.75f) * beat, 0.014f, 1250 + b);
                    for (int e8 = 0; e8 < 8; e8++) AddBrush(bed, (b0 + e8 * 0.5f) * beat, 0.05f, e8 % 2 == 0 ? 0.009f : 0.014f, 1300 + b * 8 + e8);
                }
                if (big) AddTimpani(bed, b0 * beat, Freq(Root(first, 38)), 0.1f);
                if (sectionStart && !quiet) AddTimpani(bed, b0 * beat, Freq(Root(first, 38)), 0.13f);
                if (sectionEnd)
                {
                    // A snare roll swelling into the next section, with a timpani roll under it.
                    for (int s16 = 0; s16 < 8; s16++) AddSnare(bed, (b0 + 2f + s16 * 0.25f) * beat, 0.012f + s16 * 0.004f, 1400 + b * 8 + s16);
                    int roll = Root(ToyChords[(b + 1) % bars].Split(' ')[0], 38);
                    for (int s16 = 0; s16 < 6; s16++) AddTimpani(bed, (b0 + 2.5f + s16 * 0.25f) * beat, Freq(roll), 0.03f + s16 * 0.012f);
                }
                // Glockenspiel sparkles high above, every other bar.
                if (b % 2 == 1 && !quiet)
                {
                    var g = Ch(second, 84);
                    for (int i = 0; i < 3; i++) AddGlock(bed, (b0 + 2f + i * 0.25f) * beat, Freq(g[i % g.Length]), 0.012f);
                }

                // ----- Lute (harp) -----
                var hc = Ch(first, 50);
                var hc2 = Ch(second, 50);
                int[] order8 = big ? new[] { 0, 1, 2, 3, 4, 5, 4, 3 } : new[] { 0, 1, 2, 3, 2, 1, 2, 3 };
                for (int e8 = 0; e8 < 8; e8++)
                {
                    var c = e8 < 4 ? hc : hc2;
                    int idx = order8[e8];
                    int note = c[idx % 3] + 12 * (idx / 3) + (big ? 12 : 0);
                    AddPluck(lute, (b0 + e8 * 0.5f) * beat, beat * 1.3f, Freq(note), quiet ? 0.06f : 0.05f, 2100 + b * 8 + e8);
                }
                if (sectionEnd)
                {
                    var into = Ch(ToyChords[(b + 1) % bars].Split(' ')[0], 62);
                    var run = new int[14];
                    for (int i = 0; i < run.Length; i++) run[i] = into[i % 3] + 12 * (i / 3) - 12;
                    AddGliss(lute, (b0 + 3f) * beat, run, beat * 0.95f, 0.05f, 2500 + b * 14);
                }

                // ----- Trumpet (horns and brass) -----
                if (section == 1 || section == 2)
                {
                    // Horns holding a soft open fifth, and a little fanfare every four bars.
                    int r = Root(first, 50);
                    AddBrass(brass, b0 * beat, beat * 3.7f, Freq(r), 0.022f);
                    AddBrass(brass, b0 * beat, beat * 3.7f, Freq(r + 7), 0.016f);
                    if (b % 4 == 0)
                    {
                        var bc = Ch(first, 62);
                        foreach (int c in bc) AddBrass(brass, b0 * beat, beat * 0.3f, Freq(c), 0.028f);
                        foreach (int c in bc) AddBrass(brass, (b0 + 0.75f) * beat, beat * 0.2f, Freq(c), 0.02f);
                        foreach (int c in bc) AddBrass(brass, (b0 + 1f) * beat, beat * 0.8f, Freq(c), 0.026f);
                    }
                    if (b == 18 || b == 19)
                    {
                        // The lift through the borrowed chords: brass swelling up.
                        foreach (int c in Ch(first, 58)) AddBrass(brass, b0 * beat, beat * 3.8f, Freq(c), 0.03f);
                    }
                }
                else if (section == 3)
                {
                    foreach (var nt in Line(ToyBrassC[b - 20], b0)) AddBrass(brass, nt.Beat * beat, nt.Len * beat * 0.96f, Freq(nt.Midi), 0.055f);
                    if (b == 20)
                        foreach (int c in Ch(first, 62)) AddBrass(brass, b0 * beat, beat * 0.5f, Freq(c), 0.035f);
                }
                else if (section == 5)
                {
                    foreach (int c in Ch(first, 58)) AddBrass(brass, b0 * beat, beat * (first == second ? 3.6f : 1.8f), Freq(c), 0.034f);
                    if (first != second) foreach (int c in Ch(second, 58)) AddBrass(brass, (b0 + 2f) * beat, beat * 1.8f, Freq(c), 0.034f);
                }
                else if (section == 4 && b >= 34)
                {
                    // Horns creeping back in under the end of the interlude.
                    AddBrass(brass, b0 * beat, beat * 3.8f, Freq(Root(first, 50)), 0.018f + (b - 34) * 0.008f);
                }
            }

            // ----- The tune -----
            for (int b = 0; b < bars; b++)
            {
                int section = ToySection(b);
                foreach (var nt in Line(ToyTune[b], b * 4f))
                {
                    float t = nt.Beat * beat;
                    float len = nt.Len * beat;
                    float f = Freq(nt.Midi);
                    // In the bed: strings carry the long notes, with something bright on top
                    // of every note so the tune speaks clearly.
                    if (section == 4 || section == 0)
                    {
                        AddCelesta(bed, t, len + 0.4f, f, 0.03f);
                        AddMusicBox(bed, t, len + 0.3f, f * 0.5f, 0.012f);
                    }
                    else
                    {
                        if (nt.Len >= 1f) AddStrings(bed, t - 0.06f, len, f, section == 3 || section == 5 ? 0.034f : 0.028f);
                        if (section == 3 || section == 5)
                        {
                            if (nt.Len >= 1f) AddStrings(bed, t - 0.06f, len, f * 0.5f, 0.024f);
                            AddGlock(bed, t, f, 0.024f);
                        }
                        else AddMusicBox(bed, t, len + 0.3f, f, 0.024f);
                    }
                    // The pipe: the flute sings the tune (an octave down, soft and breathy, in
                    // the quiet interlude), with a grace note into the long notes.
                    if (section == 4) AddFlute(pipe, t, len * 0.95f + 0.05f, f * 0.5f, 0.07f);
                    else if (section != 0)
                    {
                        AddFlute(pipe, t, len * 0.95f + 0.05f, f, 0.065f);
                        if (nt.Len >= 1.5f) AddFlute(pipe, t - 0.05f, 0.05f, Freq(nt.Midi + 2), 0.03f);
                    }
                }
            }
            // Piccolo trills fluttering into the soaring section and the coda.
            foreach (int tb in new[] { 19, 35 })
                for (int i = 0; i < 8; i++) AddFlute(pipe, (tb * 4f + 3f + i * 0.125f) * beat, beat * 0.13f, Freq(i % 2 == 0 ? 86 : 88), 0.035f);

            return FinishStems(data, "pw_toybox_", new[] { "bed", "lute", "trumpet", "pipe" }, 7500f);
        }

        // A soft string pad for held chords: like AddStrings but with a simpler tone, so a whole
        // piece of it is quick to build.
        static void AddPad(float[] data, float start, float dur, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt((dur + 0.3f) * Rate);
            float pa = 0f, pb = 0f;
            float da = 2f * Mathf.PI * f * 1.004f / Rate, db = 2f * Mathf.PI * f * 0.996f / Rate;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.3f) * Mathf.Clamp01((dur + 0.3f - t) / 0.3f);
                pa += da;
                pb += db;
                float w = Mathf.Sin(pa) + Mathf.Sin(pb) + 0.35f * (Mathf.Sin(pa * 2f) + Mathf.Sin(pb * 2f)) + 0.12f * Mathf.Sin(pa * 3f);
                Add(data, s0 + i, w * 0.4f * env * vol);
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
                float amp = 1f;
                for (int h = 1; h <= 8; h++)
                {
                    w += Mathf.Sin(phase * h) * amp / h;
                    amp *= bright;
                }
                Add(data, s0 + i, w * att * rel * vol);
            }
        }
    }
}

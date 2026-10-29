using UnityEngine;

namespace Patchwork
{
    public enum MusicTrack { Skiff, Toybox }

    // Toybox Town's own soundtrack: a bouncy, swung toy-town march (see the notes on
    // ComposeToybox). It's built in layers that match the town band:
    //   Bed     - always on (the drummer's the only one whose instrument still works)
    //   Lute    - on once the lute's string is found
    //   Trumpet - on once the trumpet's valves are oiled
    //   Pipe    - on once the piper is home
    public partial class MusicPlayer
    {
        enum ToyStem { Bed, Lute, Trumpet, Pipe }
        const int ToyStemCount = 4;
        public const float ToyBpm = 112f;

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
        //
        // A bouncy, swung toy-town march in F major, 4/4 at 112 BPM, 32 bars:
        //   A  (1-8)   the main tune - catchy and singable
        //   A' (9-16)  again, with a wistful borrowed chord and a new ending
        //   B  (17-24) D minor, a little mysterious - a soft chiptune lead takes the tune
        //   C  (25-32) the big lift: long soaring notes, strings and brass, then back round
        // The bed (always playing) is already a whole song: walking pizzicato bass, brushed
        // drums, toy-piano off-beats, and the tune on music box / chip lead / glockenspiel.
        // Each band member found adds a layer on top:
        //   Lute    - harp arpeggios and glissandos into each section
        //   Trumpet - brass fanfares, little answering fills, horn pads and a countermelody
        //   Pipe    - the flute singing the tune (a clarinet-ish low flute in the minor section)

        static readonly string[] ToyChords =
        {
            "F", "F", "Bb", "F", "Gm7", "C7", "F", "C7",
            "F", "F", "Bb", "Bbm", "F", "D7", "Gm7 C7", "F",
            "Dm", "Dm", "Bb", "F", "Gm", "A7", "Dm", "C7",
            "Bb", "C", "Am", "Dm", "Gm7", "C", "F", "C7",
        };

        // One bar per entry, lengths in beats (4 a bar).
        static readonly string[] ToyTune =
        {
            "C5:0.5 F5:0.5 A5:1 G5:0.5 F5:0.5 C5:1", "D5:0.5 F5:0.5 A5:0.5 C6:0.5 A5:2",
            "Bb5:1 A5:0.5 G5:0.5 F5:1 D5:1", "C5:0.5 D5:0.5 F5:0.5 A5:0.5 G5:2",
            "G5:0.5 Bb5:0.5 D6:1 C6:0.5 Bb5:0.5 G5:1", "E5:0.5 G5:0.5 C6:1 Bb5:0.5 G5:0.5 E5:1",
            "F5:1 A5:0.5 C6:0.5 F6:1 E6:0.5 D6:0.5", "C6:1.5 Bb5:0.5 A5:0.5 G5:0.5 E5:1",

            "C5:0.5 F5:0.5 A5:1 G5:0.5 F5:0.5 C5:1", "D5:0.5 F5:0.5 A5:0.5 C6:0.5 A5:2",
            "Bb5:1 A5:0.5 G5:0.5 F5:1 D5:1", "Db6:1 C6:0.5 Bb5:0.5 F5:2",
            "A5:0.5 C6:0.5 F6:1 E6:0.5 D6:0.5 C6:1", "F#5:0.5 A5:0.5 D6:1 C6:0.5 A5:0.5 F#5:1",
            "Bb5:0.5 A5:0.5 G5:1 E5:0.5 G5:0.5 Bb5:1", "A5:2 F5:1 r:1",

            "D5:1 F5:0.5 A5:0.5 D6:1 A5:1", "C6:0.5 Bb5:0.5 A5:1 G5:0.5 F5:0.5 E5:1",
            "F5:1 D5:0.5 F5:0.5 Bb5:1 A5:0.5 G5:0.5", "A5:2 F5:1 C5:1",
            "Bb4:0.5 D5:0.5 G5:1 F5:0.5 G5:0.5 Bb5:1", "A5:0.5 C#6:0.5 E6:1 D6:0.5 C#6:0.5 A5:1",
            "D6:1 C6:0.5 Bb5:0.5 A5:1 F5:1", "E5:1 G5:0.5 Bb5:0.5 C6:1 r:1",

            "D6:2 F6:2", "E6:2 C6:1 G5:1", "A5:1.5 C6:0.5 E6:2", "D6:3 A5:1",
            "Bb5:2 D6:1 F6:1", "E6:2 D6:1 C6:1", "C6:1 A5:1 F5:1 A5:1", "G5:1 Bb5:1 C6:0.5 Bb5:0.5 G5:1",
        };

        // The brass countermelody under the soaring section (bars 25-32).
        static readonly string[] ToyBrassC =
        {
            "F4:4", "G4:4", "E4:2 C5:2", "F4:4", "D5:2 Bb4:2", "C5:2 G4:2", "A4:2 C5:2", "Bb4:2 E4:2",
        };

        const float ToySwing = 0.12f;   // how late the "and"s land (in beats)

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
            System.Func<float, float> T = bt => Swing(bt, ToySwing) * beat;

            for (int b = 0; b < bars; b++)
            {
                int section = b / 8;   // 0 A, 1 A', 2 B, 3 C
                float b0 = b * 4f;
                var names = ToyChords[b].Split(' ');
                string first = names[0], second = names.Length > 1 ? names[1] : names[0];

                // ----- Bed -----
                // Walking pizzicato bass: root, third/fifth, fifth/octave, then a step towards the next bar.
                int r1 = Root(first, 36), r2 = Root(second, 36);
                string next = ToyChords[(b + 1) % bars].Split(' ')[0];
                int nextRoot = Root(next, 36);
                int[] walk = first == second
                    ? new[] { r1, r1 + (first.Contains("m") ? 3 : 4), r1 + 7, nextRoot + (nextRoot > r1 ? -1 : 1) }
                    : new[] { r1, r1 + 7, r2, r2 + 7 };
                for (int q = 0; q < 4; q++) AddPizz(bed, (b0 + q) * beat, Freq(walk[q]), 0.13f, 1000 + b * 4 + q);
                // Drums: soft kick on 1 and 3, brushed snare on 2 and 4, a swung shaker.
                AddKick(bed, b0 * beat, 0.12f);
                AddKick(bed, (b0 + 2f) * beat, 0.07f);
                AddSnare(bed, (b0 + 1f) * beat, 0.045f, 1100 + b);
                AddSnare(bed, (b0 + 3f) * beat, 0.05f, 1200 + b);
                for (int e8 = 0; e8 < 8; e8++) AddBrush(bed, T(b0 + e8 * 0.5f), 0.05f, e8 % 2 == 0 ? 0.012f : 0.018f, 1300 + b * 8 + e8);
                if (b == bars - 1 || b == 15 || b == 23)
                    for (int s16 = 0; s16 < 8; s16++) AddSnare(bed, (b0 + 2f + s16 * 0.25f) * beat, 0.02f + s16 * 0.004f, 1400 + b * 8 + s16);
                if (b == 24) AddTimpani(bed, b0 * beat, Freq(Root("Bb", 34)), 0.14f);
                if (b == 16) AddTimpani(bed, b0 * beat, Freq(Root("D", 38)), 0.12f);

                for (int half = 0; half < 2; half++)
                {
                    string name = half == 0 ? first : second;
                    var ch = Ch(name, 60);
                    float h0 = b0 + half * 2f;
                    if (section == 0 || section == 1)
                    {
                        // Toy piano on the swung "and" of each half bar.
                        foreach (int c in ch) AddToyPiano(bed, T(h0 + 1.5f), Freq(c), 0.022f);
                        foreach (int c in ch) AddToyPiano(bed, T(h0 + 0.5f), Freq(c), 0.012f);
                    }
                    else if (section == 2)
                    {
                        // Celesta ripples, up and back down the chord.
                        int[] order = { 0, 1, 2, 1 };
                        for (int e8 = 0; e8 < 4; e8++) AddCelesta(bed, T(h0 + e8 * 0.5f), beat * 0.6f, Freq(ch[order[e8] % ch.Length] + 12), 0.02f);
                    }
                    else
                    {
                        // Warm strings holding the chords.
                        foreach (int c in ch) AddStrings(bed, h0 * beat, beat * 2f, Freq(c - 12), 0.016f);
                    }
                }

                // ----- Lute (harp) -----
                var hc = Ch(first, 55);
                int[] up = { hc[0], hc[1], hc[2], hc[0] + 12, hc[1] + 12, hc[2] + 12 };
                if (section == 2)
                {
                    int[] order = { 0, 2, 1, 2 };
                    for (int q = 0; q < 4; q++)
                    {
                        var c = Ch(q < 2 ? first : second, 48);
                        AddPluck(lute, (b0 + q) * beat, beat * 1.6f, Freq(c[order[q] % c.Length]), 0.075f, 2000 + b * 4 + q);
                    }
                }
                else
                {
                    int[] order = section == 3 ? new[] { 0, 1, 2, 3, 4, 5, 4, 3 } : new[] { 0, 1, 2, 3, 2, 1, 2, 3 };
                    for (int e8 = 0; e8 < 8; e8++)
                    {
                        var c = e8 < 4 ? up : (first == second ? up : new[] { Ch(second, 55)[0], Ch(second, 55)[1], Ch(second, 55)[2], Ch(second, 55)[0] + 12, Ch(second, 55)[1] + 12, Ch(second, 55)[2] + 12 });
                        AddPluck(lute, T(b0 + e8 * 0.5f), beat * 1.2f, Freq(c[order[e8]] + (section == 3 ? 12 : 0)), 0.055f, 2100 + b * 8 + e8);
                    }
                }
                // Glissandos sweeping into each new section.
                if (b % 8 == 7)
                {
                    var into = Ch(ToyChords[(b + 1) % bars].Split(' ')[0], 60);
                    var run = new int[12];
                    for (int i = 0; i < run.Length; i++) run[i] = into[i % into.Length] + 12 * (i / into.Length) - 12;
                    AddGliss(lute, (b0 + 3f) * beat, run, beat * 0.9f, 0.05f, 2500 + b * 12);
                }

                // ----- Trumpet (brass) -----
                if (section == 0 || section == 1)
                {
                    if (b % 4 == 0)
                    {
                        // Fanfare: a short stab, a pickup, and a held chord.
                        var bc = Ch(first, 58);
                        foreach (int c in bc) AddBrass(brass, b0 * beat, beat * 0.35f, Freq(c), 0.03f);
                        foreach (int c in bc) AddBrass(brass, T(b0 + 1.5f), beat * 0.4f, Freq(c), 0.025f);
                    }
                    string fill = null;
                    float fillAt = 2f;
                    switch (b)
                    {
                        case 3: fill = "F4:0.5 A4:0.5 C5:1"; break;
                        case 7: fill = "E4:0.5 G4:0.5 C5:0.5 Bb4:0.5"; break;
                        case 11: fill = "Db5:1 F5:1"; break;
                        case 15: fill = "C5:0.5 A4:0.5 F4:1"; break;
                    }
                    if (fill != null)
                        foreach (var nt in Line(fill, b0 + fillAt)) AddBrass(brass, T(nt.Beat), nt.Len * beat * 0.9f, Freq(nt.Midi), 0.06f);
                }
                else if (section == 2)
                {
                    // Horns: soft held fifths underneath.
                    int r = Root(first, 46);
                    AddBrass(brass, b0 * beat, beat * 3.8f, Freq(r), 0.03f);
                    AddBrass(brass, b0 * beat, beat * 3.8f, Freq(r + 7), 0.022f);
                }
                else
                {
                    foreach (var nt in Line(ToyBrassC[b - 24], b0)) AddBrass(brass, nt.Beat * beat, nt.Len * beat * 0.95f, Freq(nt.Midi), 0.05f);
                }
            }

            // ----- The tune -----
            for (int b = 0; b < bars; b++)
            {
                int section = b / 8;
                foreach (var nt in Line(ToyTune[b], b * 4f))
                {
                    float t = T(nt.Beat);
                    float len = nt.Len * beat;
                    float f = Freq(nt.Midi);
                    // In the bed: music box (A), chiptune lead (B), glockenspiel (C).
                    if (section < 2) AddMusicBox(bed, t, len + 0.3f, f, 0.026f);
                    else if (section == 2) AddChip(bed, t, len * 0.92f, f, 0.018f);
                    else AddGlock(bed, t, f, 0.028f);
                    // The pipe: flute, dropping an octave to a mellow low flute in the minor section.
                    if (section == 2) AddFlute(pipe, t, len * 0.95f + 0.05f, f * 0.5f, 0.075f);
                    else
                    {
                        AddFlute(pipe, t, len * 0.95f + 0.05f, f, 0.07f);
                        if (nt.Len >= 1.5f) AddFlute(pipe, t - 0.05f, 0.05f, Freq(nt.Midi + 2), 0.035f);
                    }
                }
            }
            // A little piccolo trill flourish before the soaring section.
            for (int i = 0; i < 8; i++) AddFlute(pipe, (23f * 4f + 3f + i * 0.125f) * beat, beat * 0.13f, Freq(i % 2 == 0 ? 84 : 86), 0.04f);

            return FinishStems(data, "pw_toybox_", new[] { "bed", "lute", "trumpet", "pipe" }, 7000f);
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

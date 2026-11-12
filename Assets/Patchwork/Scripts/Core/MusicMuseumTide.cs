using UnityEngine;

namespace Patchwork
{
    // "The Lantern Tide": the Oceanic Museum's piece. A gentle, glowing walk through an
    // aquarium at night, in F major: a harp rippling in quavers, a marimba bobbing like
    // something afloat, plucked basses, and now and then a bubble rising through it all.
    //   A  - the flute sings the tune over the harp and marimba; a soft heartbeat of drums
    //   B  - down into D minor, deeper and darker: the cellos take the tune, the drums stop,
    //        only the brushes and the bubbles keep time
    //   C  - the tide rising: the violins climb up high, the horns answer them
    //   A' - the tune comes home with everything under it, and settles
    // (An original piece, written in the spirit of gentle orchestral adventure music.)
    //
    // The same four layers as the old waltz, so the museum turns them on and off just the same:
    //   Bed (harp, marimba, basses, bubbles), Strings, Winds (flute, bells), Grand (horns, drums).
    //
    // 4/4 at 92 BPM, 64 bars:
    //   Intro (1-4)  A (5-20)  B (21-36)  C (37-48)  A' (49-64)
    public partial class MusicPlayer
    {
        const float TideBpm = 92f;

        static readonly string[] TideChords =
        {
            "Fmaj7", "C", "Bb", "C",

            "F", "C", "Dm", "Bb", "F", "Bb", "Gm", "C",
            "F", "C", "Dm", "Bb", "Gm", "C", "F", "F",

            "Dm", "Bb", "F", "C", "Dm", "Bb", "Gm", "A7",
            "Dm", "Bb", "F", "C", "Gm", "A7", "Dm", "C7",

            "Bb", "C", "Am", "Dm", "Bb", "C", "F", "Dm", "Gm", "Am", "Bb", "C7",

            "F", "C", "Dm", "Bb", "F", "Bb", "Gm", "C",
            "F", "C", "Dm", "Bb", "Gm", "C", "F", "F",
        };

        // One bar per entry, lengths in beats (4 a bar).
        static readonly string[] TideTune =
        {
            "r:4", "r:4", "r:4", "r:3 C5:1",

            "C5:1.5 A4:0.5 C5:1 F5:1", "E5:2 D5:1 C5:1", "D5:1.5 E5:0.5 F5:1 A5:1", "G5:3 F5:1",
            "A5:1.5 G5:0.5 F5:1 C5:1", "D5:1.5 C5:0.5 Bb4:1 D5:1", "C5:1 Bb4:1 A4:1 G4:1", "E5:3 C5:1",
            "C5:1.5 A4:0.5 C5:1 F5:1", "E5:2 D5:1 E5:1", "F5:1.5 G5:0.5 A5:1 C6:1", "Bb5:2 A5:1 G5:1",
            "Bb5:1.5 A5:0.5 G5:1 F5:1", "E5:1 G5:1 C6:1 Bb5:1", "A5:4", "r:3 A4:1",

            "D5:2 E5:1 F5:1", "F5:2 E5:1 D5:1", "C5:2 A4:1 C5:1", "E5:3 r:1",
            "D5:1 F5:1 A5:1 D6:1", "C6:2 Bb5:1 A5:1", "G5:2 F5:1 E5:1", "C#5:2 E5:1 A5:1",
            "F5:2 E5:1 D5:1", "D5:2 C5:1 Bb4:1", "A4:1 C5:1 F5:1 A5:1", "G5:3 E5:1",
            "D5:1 G5:1 Bb5:1 A5:1", "G5:1 E5:1 C#5:1 E5:1", "D5:4", "r:1 E5:1 G5:1 Bb5:1",

            "D6:2 C6:1 Bb5:1", "E6:2 D6:1 C6:1", "C6:1.5 Bb5:0.5 A5:1 E5:1", "F5:2 A5:1 D6:1",
            "D6:2 C6:1 Bb5:1", "E6:2 F6:1 G6:1", "A6:2 F6:1 C6:1", "D6:2 A5:1 F5:1",
            "G5:1 Bb5:1 D6:1 G6:1", "E6:2 C6:1 A5:1", "D6:1.5 C6:0.5 Bb5:1 G5:1", "G5:1 A5:1 Bb5:1 B5:1",

            "C6:1.5 A5:0.5 C6:1 F6:1", "E6:2 D6:1 C6:1", "D6:1.5 E6:0.5 F6:1 A6:1", "G6:3 F6:1",
            "A6:1.5 G6:0.5 F6:1 C6:1", "D6:1.5 C6:0.5 Bb5:1 D6:1", "C6:1 Bb5:1 A5:1 G5:1", "E6:3 C6:1",
            "C6:1.5 A5:0.5 C6:1 F6:1", "E6:2 D6:1 E6:1", "F6:1.5 G6:0.5 A6:1 C7:1", "Bb6:2 A6:1 G6:1",
            "Bb6:1.5 A6:0.5 G6:1 F6:1", "E6:1 G6:1 C7:1 Bb6:1", "A6:4", "F6:4",
        };

        // The horns' answer in the rising tide and the homecoming: long notes, one or two a bar.
        static readonly string[] TideHorn =
        {
            "F4:4", "E4:4", "E4:4", "F4:4", "F4:4", "G4:4", "A4:4", "A4:4", "Bb4:4", "C5:4", "D5:2 C5:2", "Bb4:2 E4:2",
        };

        static readonly string[] TideHornHome =
        {
            "A4:4", "G4:4", "F4:4", "F4:4", "A4:4", "F4:4", "G4:4", "E4:4",
            "A4:4", "G4:4", "A4:4", "Bb4:4", "Bb4:4", "G4:2 E4:2", "F4:4", "F4:4",
        };

        // 0 intro, 1 A, 2 B (the deep), 3 C (the rising tide), 4 A' (home).
        static int TideSection(int bar)
        {
            return bar < 4 ? 0 : bar < 20 ? 1 : bar < 36 ? 2 : bar < 48 ? 3 : 4;
        }

        static AudioClip[] ComposeTide()
        {
            float beat = 60f / TideBpm;
            float bar = beat * 4f;
            int bars = TideChords.Length;
            int n = Mathf.CeilToInt(bar * bars * Rate);
            var data = new float[MuseumStemCount][];
            for (int i = 0; i < MuseumStemCount; i++) data[i] = new float[n];
            var bed = data[(int)MuseumStem.Bed];
            var strings = data[(int)MuseumStem.Strings];
            var winds = data[(int)MuseumStem.Winds];
            var grand = data[(int)MuseumStem.Grand];
            var rng = new System.Random(9292);

            for (int b = 0; b < bars; b++)
            {
                int section = TideSection(b);
                bool intro = section == 0, deep = section == 2, rising = section == 3, home = section == 4;
                bool sectionStart = b == 4 || b == 20 || b == 36 || b == 48;
                bool sectionEnd = b == 3 || b == 19 || b == 35 || b == 47;
                float b0 = b * 4f;
                string name = TideChords[b];
                string next = TideChords[(b + 1) % bars];
                var ch = Ch(name, 53);
                int root = Root(name, 38);   // between D2 and C#3
                int fifth = root + 7;

                // ----- Bed -----
                // The harp: quavers rippling up two octaves of the chord and back down.
                int[] shape = deep ? new[] { 0, 2, 4, 5, 4, 2, 1, 2 } : new[] { 0, 1, 2, 3, 4, 5, 4, 2 };
                for (int e8 = 0; e8 < 8; e8++)
                {
                    int step = shape[(e8 + (b % 2 == 1 && !intro ? 1 : 0)) % 8];
                    int note = ch[step % ch.Length] + 12 * (step / ch.Length);
                    float v = (e8 == 0 ? 0.046f : 0.032f) * (intro ? 1.2f : deep ? 0.85f : 1f);
                    AddPluck(bed, (b0 + e8 * 0.5f) * beat, beat * 1.5f, Freq(note), v, 7000 + b * 8 + e8);
                }
                // The marimba, bobbing on a dotted rhythm (not in the deep, where it's too dark for it).
                if (!intro && !deep)
                {
                    float[] at = { 0f, 1.5f, 3f };
                    for (int k = 0; k < at.Length; k++)
                    {
                        int note = ch[(k + b) % ch.Length] + 12;
                        AddMarimba(bed, (b0 + at[k]) * beat, Freq(note), rising || home ? 0.07f : 0.06f);
                    }
                    if (b % 2 == 1) AddMarimba(bed, (b0 + 3.5f) * beat, Freq(ch[0] + 24), 0.04f);
                }
                // Plucked basses: the root, then up the octave, then the fifth to lead on.
                if (!intro)
                {
                    AddPizz(bed, b0 * beat, Freq(root), 0.18f, 7100 + b * 3);
                    AddPizz(bed, (b0 + 2f) * beat, Freq(root + 12), 0.13f, 7101 + b * 3);
                    AddPizz(bed, (b0 + 3.5f) * beat, Freq(fifth), 0.09f, 7102 + b * 3);
                    AddBass(bed, b0 * beat, bar * 0.9f, Freq(root), deep ? 0.06f : 0.045f);
                }
                else AddBass(bed, b0 * beat, bar * 0.95f, Freq(root), 0.03f);
                // Bubbles rising through it: a few soft bloops, never in the same place twice.
                int bloops = intro || deep ? 3 : 1;
                for (int k = 0; k < bloops; k++)
                {
                    float when = (float)rng.NextDouble() * 3.5f;
                    AddBloop(bed, (b0 + when) * beat, Freq(ch[rng.Next(ch.Length)] + 24 + (rng.NextDouble() < 0.5 ? 12 : 0)), 0.024f);
                }
                foreach (int c in ch) AddPad(bed, b0 * beat, bar, Freq(c - 12), intro ? 0.009f : 0.011f);

                // ----- Strings -----
                foreach (int c in ch) AddPad(strings, b0 * beat, bar, Freq(c), rising || home ? 0.016f : 0.011f);
                if (section == 1)
                {
                    // Soft long violins on the top of each chord.
                    AddViolins(strings, b0 * beat - 0.04f, bar * 0.98f, Freq(ch[ch.Length - 1] + 12), 0.016f, 3);
                }
                if (rising || home)
                {
                    // Cellos stepping along with the bass an octave up.
                    AddViolins(strings, b0 * beat - 0.04f, bar * 0.97f, Freq(root + 12), 0.03f, 3);
                    AddViolins(strings, b0 * beat - 0.04f, bar * 0.97f, Freq(ch[1] + 12), 0.016f, 3);
                }
                if (deep)
                {
                    // A low drone under the deep, and a slow swell through every other bar.
                    AddViolins(strings, b0 * beat - 0.04f, bar, Freq(root), 0.02f, 3);
                    if (b % 2 == 0) AddAirPad(strings, b0 * beat, bar * 2f, 0.03f, 7300 + b);
                }

                // ----- Winds: bells and sparkle -----
                if (intro || b % 2 == 1)
                {
                    var g = Ch(name, 79);
                    AddMusicBox(winds, (b0 + 1f) * beat, beat * 1.4f, Freq(g[(b / 2) % g.Length]), 0.022f);
                    AddMusicBox(winds, (b0 + 2.5f) * beat, beat * 1.4f, Freq(g[(b / 2 + 2) % g.Length]), 0.018f);
                }
                if (sectionEnd)
                {
                    // A harp glissando sweeping up into each new part.
                    var into = Ch(next, 55);
                    var run = new int[16];
                    for (int i = 0; i < run.Length; i++) run[i] = into[i % into.Length] + 12 * (i / into.Length);
                    AddGliss(winds, (b0 + 2f) * beat, run, beat * 1.9f, 0.05f, 7600 + b * 16);
                }
                if (home && b % 2 == 0)
                {
                    var g = Ch(name, 84);
                    for (int i = 0; i < 4; i++) AddGlock(winds, (b0 + i) * beat, Freq(g[i % g.Length]), 0.011f);
                }

                // ----- Grand: horns and drums -----
                if (rising || home)
                {
                    string horn = rising ? TideHorn[b - 36] : TideHornHome[b - 48];
                    foreach (var nt in Line(horn, b0))
                        AddBrass(grand, nt.Beat * beat - BrassLead, nt.Len * beat * 0.95f, Freq(nt.Midi), home ? 0.034f : 0.03f);
                    if (home) AddBrass(grand, b0 * beat - BrassLead, bar * 0.95f, Freq(root + 12), 0.022f);
                }
                if (!intro && !deep)
                {
                    // A soft heartbeat: kick on one and three, brushes on two and four.
                    AddKick(grand, b0 * beat, 0.05f);
                    AddKick(grand, (b0 + 2f) * beat, 0.04f);
                    AddBrush(grand, (b0 + 1f) * beat, beat * 0.6f, 0.012f, 7800 + b * 2);
                    AddBrush(grand, (b0 + 3f) * beat, beat * 0.6f, 0.012f, 7801 + b * 2);
                }
                if (deep)
                {
                    AddBrush(grand, (b0 + 1f) * beat, beat * 0.8f, 0.008f, 7810 + b * 2);
                    AddBrush(grand, (b0 + 3f) * beat, beat * 0.8f, 0.008f, 7811 + b * 2);
                }
                if (sectionStart) AddTimpani(grand, b0 * beat, Freq(root), section == 2 ? 0.07f : 0.1f);
                if (sectionEnd && b != 3)
                {
                    // A drum roll swelling into the next part, and a breath of cymbal.
                    int roll = Root(next, 38);
                    for (int s16 = 0; s16 < 8; s16++) AddTimpani(grand, (b0 + 2f + s16 * 0.25f) * beat, Freq(roll), 0.02f + s16 * 0.01f);
                    AddAirPad(grand, (b0 + 3f) * beat, beat * 2.5f, 0.04f, 7900 + b);
                }
                if (b == bars - 1)
                    foreach (int c in ch) AddBrass(grand, b0 * beat - BrassLead, bar * 0.9f, Freq(c - 12), 0.018f);
            }

            // ----- The tune -----
            for (int b = 0; b < bars; b++)
            {
                int section = TideSection(b);
                foreach (var nt in Line(TideTune[b], b * 4f))
                {
                    float t = nt.Beat * beat;
                    float len = nt.Len * beat;
                    float f = Freq(nt.Midi);
                    switch (section)
                    {
                        case 0:
                            // (The pickup into the tune.)
                            AddFlute(winds, t - FluteLead, len * 0.95f, f, 0.04f);
                            AddCelesta(bed, t, len + 0.3f, f, 0.03f);
                            break;
                        case 1:
                            // The flute has it, with a celesta shining on it and bells on the long notes.
                            AddFlute(winds, t - FluteLead, len * 0.95f, f, 0.05f);
                            AddCelesta(bed, t, len + 0.4f, f, 0.035f);
                            AddViolins(strings, t - 0.04f, len * 0.98f, f, 0.018f, 3);
                            if (nt.Len >= 2f) AddGlock(winds, t, f * 2f, 0.012f);
                            break;
                        case 2:
                            // The deep: the cellos take it an octave down, a celesta far above.
                            AddViolins(strings, t - 0.05f, len * 0.98f, f * 0.5f, 0.05f, 4);
                            AddCelesta(bed, t, len + 0.5f, f * 2f, 0.02f);
                            if (nt.Len >= 2f) AddFlute(winds, t - FluteLead, len * 0.9f, f, 0.022f);
                            break;
                        case 3:
                            // The rising tide: violins up high, the flute an octave under.
                            AddViolins(strings, t - 0.04f, len * 0.98f, f, 0.055f);
                            AddViolins(strings, t - 0.04f, len * 0.98f, f * 0.5f, 0.025f, 3);
                            AddFlute(winds, t - FluteLead, len * 0.95f, f * 0.5f, 0.035f);
                            AddPiano(bed, t, len, f * 0.5f, 0.05f);
                            break;
                        default:
                            // Home: violins an octave up, the flute on the tune as it first was,
                            // the piano and celesta glinting, horns under the long notes.
                            AddViolins(strings, t - 0.04f, len * 0.98f, f, 0.05f);
                            AddViolins(strings, t - 0.04f, len * 0.98f, f * 0.5f, 0.03f, 3);
                            AddFlute(winds, t - FluteLead, len * 0.95f, f * 0.5f, 0.045f);
                            AddCelesta(bed, t, len + 0.4f, f * 0.5f, 0.03f);
                            AddPiano(bed, t, len, f * 0.5f, 0.06f);
                            if (nt.Len >= 2f) AddBrass(grand, t - BrassLead, len * 0.9f, f * 0.25f, 0.026f);
                            break;
                    }
                }
            }

            return FinishStems(data, "pw_tide_", new[] { "bed", "strings", "winds", "grand" }, 7500f);
        }
    }
}

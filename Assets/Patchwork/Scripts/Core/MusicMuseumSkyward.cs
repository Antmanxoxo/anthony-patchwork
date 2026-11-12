using UnityEngine;

namespace Patchwork
{
    // "The Skyward Waltz": the Oceanic Museum's piece. A waltz that takes off: it starts on
    // its toes in G minor, bouncing along like a walk through crowded streets, then lifts
    // off its feet into the sky and climbs up among the stars.
    //   Intro - harp and celesta, glittering, the chords turning slowly
    //   A     - G minor, on its toes: the accordion has the tune, the bass steps down a
    //           half-step at a time, the piano goes "pah-pah" on two and three
    //   A2    - the violins take the tune, a flute weaving round it, and it turns for home
    //           to B flat
    //   B     - lift-off: B flat major, long soaring notes on the violins with the cellos
    //           singing underneath, the horns opening out; one dark chord (E flat minor) for
    //           the moment you look down
    //   C     - among the stars: E flat major, broad and glowing, harp running up and down,
    //           bells and celesta, a choir-like hush, the horns calling; then a turn back
    //           towards G minor and a drum roll
    //   A'    - the tune again, with the whole orchestra, flying now
    //   Coda  - it lands in G major, bright, and settles
    // (An original piece, written in the spirit of the great soaring film and game waltzes.)
    //
    // The same four layers as the other museum pieces, so the museum turns them on and off
    // just the same:
    //   Bed (harp, piano, plucked basses, celesta), Strings, Winds (accordion, flute, bells),
    //   Grand (horns, timpani, cymbals).
    //
    // 3/4 at 156 BPM (felt one-in-a-bar), 96 bars:
    //   Intro (1-8)  A (9-24)  A2 (25-40)  B (41-56)  C (57-72)  A' (73-88)  Coda (89-96)
    public partial class MusicPlayer
    {
        const float SkyBpm = 156f;

        // Each bar: its chord, and the bass note under it (which isn't always the chord's root:
        // in the A tune the bass walks down by half-steps under a chord that stays put).
        static readonly string[] SkyChords =
        {
            "Gm", "Ebmaj7", "Cm", "D7", "Gm", "Ebmaj7", "Cm", "D7",

            "Gm", "Gm", "Gm", "C", "Cm", "Gm", "A7", "D7",
            "Gm", "Gm", "Cm", "F7", "Bbmaj7", "Ebmaj7", "Cm", "D7",

            "Gm", "Gm", "Gm", "C", "Cm", "Gm", "A7", "D7",
            "Gm", "Gm", "Cm", "F7", "Ebmaj7", "Cm7", "F7", "F7",

            "Bb", "Dm", "Eb", "F", "Gm", "Eb", "Cm7", "F7",
            "Bb", "Dm", "Eb", "Ebm", "Bb", "G7", "F7", "Bb7",

            "Ebmaj7", "Bb", "Cm", "Ab", "Ebmaj7", "Gm", "Abmaj7", "Bb7",
            "Ebmaj7", "Bb", "Cm", "Gm", "Abmaj7", "Fm7", "Bb7", "D7",

            "Gm", "Gm", "Gm", "C", "Cm", "Gm", "A7", "D7",
            "Gm", "Gm", "Cm", "F7", "Bbmaj7", "Ebmaj7", "Cm", "D7",

            "G", "Em", "C", "D", "G", "C", "D7", "G",
        };

        static readonly string[] SkyBass =
        {
            "G2", "Eb2", "C2", "D2", "G2", "Eb2", "A2", "D2",

            "G2", "F#2", "F2", "E2", "Eb2", "D2", "C#2", "D2",
            "G2", "Bb2", "C2", "F2", "Bb2", "Eb2", "A2", "D2",

            "G2", "F#2", "F2", "E2", "Eb2", "D2", "C#2", "D2",
            "G2", "Bb2", "C2", "F2", "Eb2", "C2", "F2", "F2",

            "Bb2", "D2", "Eb2", "F2", "G2", "Eb2", "C2", "F2",
            "Bb2", "D2", "Eb2", "Eb2", "Bb2", "B2", "F2", "Bb2",

            "Eb2", "D2", "C2", "Ab2", "G2", "G2", "Ab2", "Bb2",
            "Eb2", "D2", "C2", "Bb2", "Ab2", "F2", "Bb2", "D2",

            "G2", "F#2", "F2", "E2", "Eb2", "D2", "C#2", "D2",
            "G2", "Bb2", "C2", "F2", "Bb2", "Eb2", "A2", "D2",

            "G2", "E2", "C2", "D2", "G2", "C2", "D2", "G2",
        };

        // One bar per entry, lengths in beats (3 a bar).
        static readonly string[] SkyTune =
        {
            "r:3", "r:3", "r:3", "r:3", "D6:3", "Eb6:3", "C6:3", "A5:2 D5:1",

            // A: on its toes.
            "D5:1 G5:1 A5:1", "Bb5:2 A5:1", "G5:1 F5:0.5 G5:0.5 A5:1", "G5:2 E5:1",
            "Eb5:1 G5:1 C6:1", "Bb5:1.5 A5:0.5 G5:1", "A5:1 C#5:1 E5:1", "D5:2 D5:1",
            "D5:1 G5:1 A5:1", "Bb5:1 C6:1 D6:1", "Eb6:2 D6:0.5 C6:0.5", "A5:2 F5:1",
            "D6:1 C6:1 Bb5:1", "G5:1.5 F5:0.5 G5:1", "A5:1 C6:1 Eb6:1", "D6:2 D5:1",

            // A2: the violins, turning for B flat at the end.
            "D5:1 G5:1 A5:1", "Bb5:2 A5:1", "G5:1 F5:0.5 G5:0.5 A5:1", "G5:2 E5:1",
            "Eb5:1 G5:1 C6:1", "Bb5:1.5 A5:0.5 G5:1", "A5:1 C#5:1 E5:1", "D5:2 D5:1",
            "D5:1 G5:1 A5:1", "Bb5:1 C6:1 D6:1", "Eb6:2 D6:0.5 C6:0.5", "A5:2 F5:1",
            "G5:1 Bb5:1 D6:1", "Eb6:1.5 D6:0.5 C6:1", "A5:2 C6:1", "C6:1 A5:1 F5:1",

            // B: lift-off.
            "F5:3", "F5:1 A5:1 D6:1", "Bb5:2 G5:1", "A5:2 C6:1",
            "D6:3", "Eb6:1 D6:1 Bb5:1", "C6:2 G5:1", "F5:1 A5:1 C6:1",
            "D6:3", "F6:1 E6:1 D6:1", "Eb6:2 Bb5:1", "Gb5:2 Bb5:1",
            "F5:1 Bb5:1 D6:1", "B5:2 D6:1", "Eb6:1.5 D6:0.5 C6:1", "Ab5:2 F5:1",

            // C: among the stars.
            "G5:2 Bb5:1", "D6:1.5 C6:0.5 Bb5:1", "C6:2 Eb6:1", "Eb6:2 C6:1",
            "Bb5:1 D6:1 G6:1", "F6:1.5 D6:0.5 Bb5:1", "C6:2 Eb6:1", "D6:3",
            "G5:2 Bb5:1", "F6:2 D6:1", "Eb6:2 G6:1", "D6:2 Bb5:1",
            "C6:1 Eb6:1 G6:1", "Ab6:1.5 G6:0.5 F6:1", "F6:1 D6:1 Ab5:1", "F#5:1 A5:1 C6:1",

            // A': flying.
            "D5:1 G5:1 A5:1", "Bb5:2 A5:1", "G5:1 F5:0.5 G5:0.5 A5:1", "G5:2 E5:1",
            "Eb5:1 G5:1 C6:1", "Bb5:1.5 A5:0.5 G5:1", "A5:1 C#5:1 E5:1", "D5:2 D5:1",
            "D5:1 G5:1 A5:1", "Bb5:1 C6:1 D6:1", "Eb6:2 D6:0.5 C6:0.5", "A5:2 F5:1",
            "D6:1 C6:1 Bb5:1", "G5:1.5 F5:0.5 G5:1", "A5:1 C6:1 Eb6:1", "D6:2 F#5:1",

            // Coda: landing in G major.
            "B5:3", "G5:1 B5:1 E6:1", "E6:2 D6:1", "D6:1.5 C6:0.5 A5:1",
            "G6:3", "E6:1 C6:1 G5:1", "F#5:1 A5:1 C6:1", "G5:3",
        };

        // The horns: long notes under the lift-off, the stars and the flight.
        static readonly string[] SkyHornB =
        {
            "D4:3", "D4:3", "Eb4:3", "C4:3", "D4:3", "Eb4:3", "Eb4:3", "Eb4:3",
            "F4:3", "F4:3", "G4:3", "Gb4:3", "F4:3", "F4:3", "Eb4:3", "D4:3",
        };

        static readonly string[] SkyHornC =
        {
            "Bb3:1 Eb4:1 G4:1", "F4:3", "G4:2 Eb4:1", "Ab4:3", "G4:1 Bb4:1 D5:1", "D5:3", "C5:3", "Bb4:2 Ab4:1",
            "G4:3", "F4:3", "G4:3", "Bb4:3", "C5:3", "Ab4:3", "Ab4:3", "A4:2 F#4:1",
        };

        static readonly string[] SkyHornA =
        {
            "Bb3:3", "A3:3", "Bb3:3", "G3:3", "G3:3", "Bb3:3", "A3:3", "F#3:3",
            "Bb3:3", "D4:3", "Eb4:3", "Eb4:3", "D4:3", "Eb4:3", "Eb4:3", "F#4:3",
        };

        // 0 intro, 1 A, 2 A2, 3 B (lift-off), 4 C (the stars), 5 A' (flying), 6 coda.
        static int SkySection(int bar)
        {
            return bar < 8 ? 0 : bar < 24 ? 1 : bar < 40 ? 2 : bar < 56 ? 3 : bar < 72 ? 4 : bar < 88 ? 5 : 6;
        }

        static AudioClip[] ComposeSkyward()
        {
            float beat = 60f / SkyBpm;
            float bar = beat * 3f;
            int bars = SkyChords.Length;
            int n = Mathf.CeilToInt(bar * bars * Rate);
            var data = new float[MuseumStemCount][];
            for (int i = 0; i < MuseumStemCount; i++) data[i] = new float[n];
            var bed = data[(int)MuseumStem.Bed];
            var strings = data[(int)MuseumStem.Strings];
            var winds = data[(int)MuseumStem.Winds];
            var grand = data[(int)MuseumStem.Grand];

            for (int b = 0; b < bars; b++)
            {
                int section = SkySection(b);
                bool intro = section == 0, walking = section == 1 || section == 2, lift = section == 3, stars = section == 4, flying = section == 5, coda = section == 6;
                bool sectionStart = b == 8 || b == 24 || b == 40 || b == 56 || b == 72 || b == 88;
                bool sectionEnd = b == 7 || b == 23 || b == 39 || b == 55 || b == 71 || b == 87;
                float b0 = b * 3f;
                string name = SkyChords[b];
                string next = SkyChords[(b + 1) % bars];
                var ch = Ch(name, 55);
                int bass = NoteNum(SkyBass[b]);
                int root = Root(name, 43);

                // ----- Bed -----
                if (walking || flying)
                {
                    // On its toes: a plucked bass on one, the piano's chord on two and three.
                    AddPizz(bed, b0 * beat, Freq(bass), 0.2f, 9000 + b * 3);
                    AddBass(bed, b0 * beat, beat * 0.9f, Freq(bass), 0.05f);
                    for (int k = 1; k <= 2; k++)
                        foreach (int c in ch) AddPiano(bed, (b0 + k) * beat, beat * 0.45f, Freq(c), k == 1 ? 0.03f : 0.025f, false);
                    // A little harp turn on the "and" of three every other bar, to keep it light.
                    if (b % 2 == 1)
                    {
                        AddPluck(bed, (b0 + 2.5f) * beat, beat, Freq(ch[ch.Length - 1] + 12), 0.03f, 9100 + b);
                        AddPluck(bed, (b0 + 2.75f) * beat, beat, Freq(ch[0] + 24), 0.026f, 9101 + b);
                    }
                }
                else
                {
                    // Harp rippling in quavers up the chord and back, the bass held under it.
                    int[] shape = stars ? new[] { 0, 1, 2, 3, 4, 5 } : new[] { 0, 2, 4, 5, 4, 2 };
                    for (int e8 = 0; e8 < 6; e8++)
                    {
                        int step = shape[(e8 + (stars && b % 2 == 1 ? 3 : 0)) % 6];
                        int note = ch[step % ch.Length] + 12 * (step / ch.Length);
                        float v = (e8 == 0 ? 0.05f : 0.034f) * (intro ? 1.1f : 1f);
                        AddPluck(bed, (b0 + e8 * 0.5f) * beat, beat * 1.6f, Freq(note), v, 9200 + b * 8 + e8);
                    }
                    AddPizz(bed, b0 * beat, Freq(bass), intro ? 0.12f : 0.17f, 9300 + b);
                    AddBass(bed, b0 * beat, bar * 0.95f, Freq(bass), intro ? 0.035f : 0.05f);
                    if (lift || coda)
                        foreach (int c in ch) AddPiano(bed, (b0 + 1f) * beat, beat * 1.6f, Freq(c), 0.018f, false);
                }
                foreach (int c in ch) AddPad(bed, b0 * beat, bar, Freq(c - 12), intro ? 0.008f : 0.01f);
                // The celesta's twinkle on top of the intro and the stars.
                if (intro || stars)
                {
                    var hi = Ch(name, 79);
                    AddCelesta(bed, (b0 + 1f) * beat, beat * 1.2f, Freq(hi[b % hi.Length]), 0.022f);
                    AddCelesta(bed, (b0 + 2f) * beat, beat * 1.2f, Freq(hi[(b + 1) % hi.Length]), 0.018f);
                }

                // ----- Strings -----
                foreach (int c in ch) AddPad(strings, b0 * beat, bar, Freq(c), lift || stars || flying ? 0.016f : 0.011f);
                if (walking)
                {
                    // Soft strings holding the top of each chord, off the beat like a breath.
                    AddViolins(strings, (b0 + 1f) * beat, beat * 1.9f, Freq(ch[ch.Length - 1] + 12), 0.012f, 3);
                }
                if (lift || stars)
                {
                    // The cellos singing under the soaring tune, a step behind the bass.
                    AddViolins(strings, b0 * beat - 0.04f, bar * 0.98f, Freq(root), 0.03f, 4);
                    AddViolins(strings, b0 * beat - 0.04f, bar * 0.98f, Freq(ch[1]), 0.016f, 3);
                }
                if (flying || coda)
                {
                    AddViolins(strings, b0 * beat - 0.04f, bar * 0.97f, Freq(bass + 12), 0.026f, 3);
                    AddViolins(strings, (b0 + 1f) * beat, beat * 1.9f, Freq(ch[ch.Length - 1] + 12), 0.016f, 3);
                }

                // ----- Winds: bells and sparkle -----
                if (intro)
                {
                    var g = Ch(name, 79);
                    AddMusicBox(winds, b0 * beat, beat * 2f, Freq(g[0] + 12), 0.02f);
                    AddMusicBox(winds, (b0 + 1.5f) * beat, beat * 1.5f, Freq(g[g.Length - 1]), 0.016f);
                }
                if (stars)
                {
                    var g = Ch(name, 84);
                    for (int k = 0; k < 3; k++) AddGlock(winds, (b0 + k) * beat, Freq(g[(b + k) % g.Length]), 0.01f);
                }
                if (section == 2 || flying)
                {
                    // The flute's counter-tune: it rises into the top of each chord as the bar starts,
                    // and floats there.
                    var g = Ch(name, 72);
                    AddFlute(winds, b0 * beat - FluteLead, beat * 0.95f, Freq(g[b % g.Length]), 0.022f);
                    AddFlute(winds, (b0 + 1f) * beat - FluteLead, beat * 1.9f, Freq(g[(b + 1) % g.Length]), 0.026f);
                }
                if (sectionEnd)
                {
                    // A harp glissando sweeping up into each new part.
                    var into = Ch(next, 55);
                    var run = new int[18];
                    for (int i = 0; i < run.Length; i++) run[i] = into[i % into.Length] + 12 * (i / into.Length);
                    AddGliss(winds, (b0 + 1.2f) * beat, run, beat * 1.7f, 0.05f, 9600 + b * 18);
                }

                // ----- Grand: horns, timpani and cymbals -----
                string horn = lift ? SkyHornB[b - 40] : stars ? SkyHornC[b - 56] : flying ? SkyHornA[b - 72] : null;
                if (horn != null)
                    foreach (var nt in Line(horn, b0))
                        AddBrass(grand, nt.Beat * beat - BrassLead, nt.Len * beat * 0.95f, Freq(nt.Midi), stars ? 0.032f : 0.028f);
                if (flying)
                {
                    // The oom-pah-pah again, but with the timpani on one and the horns on two and three.
                    AddTimpani(grand, b0 * beat, Freq(bass + 12), 0.05f);
                    for (int k = 1; k <= 2; k++) AddBrass(grand, (b0 + k) * beat - BrassLead, beat * 0.4f, Freq(ch[1] - 12), 0.012f);
                    AddBrush(grand, (b0 + 1f) * beat, beat * 0.4f, 0.008f, 9700 + b * 2);
                    AddBrush(grand, (b0 + 2f) * beat, beat * 0.4f, 0.008f, 9701 + b * 2);
                }
                if (lift && b % 4 == 0) AddTimpani(grand, b0 * beat, Freq(root), 0.06f);
                if (stars && b % 2 == 0) AddAirPad(grand, b0 * beat, bar * 2f, 0.022f, 9800 + b);
                if (sectionStart) AddTimpani(grand, b0 * beat, Freq(root), section == 1 ? 0.06f : 0.1f);
                if (sectionEnd && b != 7)
                {
                    // A drum roll swelling into the next part, and a breath of cymbal.
                    int roll = Root(next, 43);
                    for (int s16 = 0; s16 < 8; s16++) AddTimpani(grand, (b0 + 1f + s16 * 0.25f) * beat, Freq(roll), 0.02f + s16 * 0.01f);
                    AddAirPad(grand, (b0 + 2f) * beat, beat * 2.5f, 0.045f, 9900 + b);
                }
                if (coda)
                {
                    AddTimpani(grand, b0 * beat, Freq(root), 0.06f);
                    foreach (int c in ch) AddBrass(grand, b0 * beat - BrassLead, bar * 0.95f, Freq(c - 12), b == bars - 1 ? 0.02f : 0.012f);
                }
            }

            // ----- The tune -----
            for (int b = 0; b < bars; b++)
            {
                int section = SkySection(b);
                foreach (var nt in Line(SkyTune[b], b * 3f))
                {
                    float t = nt.Beat * beat;
                    float len = nt.Len * beat;
                    float f = Freq(nt.Midi);
                    switch (section)
                    {
                        case 0:
                            // A glimpse of things to come, on the celesta and the music box.
                            AddCelesta(bed, t, len + 0.4f, f, 0.03f);
                            AddMusicBox(winds, t, len, f, 0.02f);
                            break;
                        case 1:
                            // On its toes: the accordion, the piano doubling it lightly.
                            AddAccordion(winds, t, len * 0.92f, f, 0.05f);
                            AddPiano(bed, t, len * 0.8f, f, 0.05f);
                            AddViolins(strings, t - 0.04f, len * 0.95f, f, 0.012f, 3);
                            break;
                        case 2:
                            // The violins take it, the celesta glinting on it.
                            AddViolins(strings, t - 0.04f, len * 0.97f, f, 0.05f);
                            AddCelesta(bed, t, len + 0.3f, f, 0.025f);
                            AddPiano(bed, t, len * 0.8f, f * 0.5f, 0.03f);
                            break;
                        case 3:
                            // Lift-off: violins up high, an octave of violas under, the flute on top.
                            AddViolins(strings, t - 0.04f, len * 0.98f, f, 0.055f);
                            AddViolins(strings, t - 0.04f, len * 0.98f, f * 0.5f, 0.025f, 3);
                            AddFlute(winds, t - FluteLead, len * 0.95f, f, 0.03f);
                            AddPiano(bed, t, len, f * 0.5f, 0.04f);
                            break;
                        case 4:
                            // Among the stars: violins and celesta, bells on the long notes.
                            AddViolins(strings, t - 0.04f, len * 0.98f, f, 0.05f);
                            AddViolins(strings, t - 0.04f, len * 0.98f, f * 0.5f, 0.022f, 3);
                            AddCelesta(bed, t, len + 0.4f, f, 0.03f);
                            AddFlute(winds, t - FluteLead, len * 0.95f, f * 0.5f, 0.028f);
                            if (nt.Len >= 2f) AddGlock(winds, t, f * 2f, 0.012f);
                            break;
                        case 5:
                            // Flying: violins an octave up, the accordion on the tune as it first was,
                            // the piano under it, horns on the long notes.
                            AddViolins(strings, t - 0.04f, len * 0.98f, f * 2f, 0.042f);
                            AddViolins(strings, t - 0.04f, len * 0.98f, f, 0.035f, 3);
                            AddAccordion(winds, t, len * 0.92f, f, 0.04f);
                            AddPiano(bed, t, len * 0.8f, f, 0.045f);
                            if (nt.Len >= 2f) AddBrass(grand, t - BrassLead, len * 0.9f, f * 0.5f, 0.024f);
                            break;
                        default:
                            // Landing: everyone, warm and bright.
                            AddViolins(strings, t - 0.04f, len * 0.98f, f, 0.05f);
                            AddViolins(strings, t - 0.04f, len * 0.98f, f * 0.5f, 0.03f, 3);
                            AddFlute(winds, t - FluteLead, len * 0.95f, f, 0.03f);
                            AddCelesta(bed, t, len + 0.5f, f, 0.03f);
                            AddPiano(bed, t, len, f * 0.5f, 0.05f);
                            if (nt.Len >= 2f) AddBrass(grand, t - BrassLead, len * 0.9f, f * 0.5f, 0.026f);
                            break;
                    }
                }
            }

            return FinishStems(data, "pw_sky_", new[] { "bed", "strings", "winds", "grand" }, 7800f);
        }
    }
}

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
    // It's played on its own light, clear set of instruments (the Sky... ones at the bottom),
    // in a soft room echo, and the whole song is there from the moment you walk in: the bed
    // has the harp, piano, accordion, flute, bells, horns and drums, the strings layer has
    // every violin and cello. The two later layers only add a little extra on top: Winds a
    // sparkle of bells and music box, Grand a deeper swell of horns and cymbals.
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
            int n = Mathf.CeilToInt((bar * bars + 4f) * Rate);
            var data = new float[MuseumStemCount][];
            for (int i = 0; i < MuseumStemCount; i++) data[i] = new float[n];
            var bed = data[(int)MuseumStem.Bed];
            var strings = data[(int)MuseumStem.Strings];
            var winds = data[(int)MuseumStem.Winds];
            var grand = data[(int)MuseumStem.Grand];
            skyNoise = new System.Random(156);

            for (int b = 0; b < bars; b++)
            {
                int section = SkySection(b);
                float b0 = b * 3f;
                var ch = Ch(SkyChords[b], 55);
                int bass = NoteNum(SkyBass[b]);
                int root = Root(SkyChords[b], 43);
                string next = SkyChords[(b + 1) % bars];
                System.Func<float, float> T = x => (b0 + x) * beat;

                if (section == 1 || section == 2 || section == 5)
                {
                    // On its toes: the plucked bass on one, the piano's chord on two and three.
                    SkyPluck(bed, T(0f), Freq(bass), 0.25f, 4f);
                    for (int k = 1; k <= 2; k++)
                        foreach (int c in ch) SkyPianoNote(bed, T(k), beat * 0.45f, Freq(c), 0.03f);
                }
                else
                {
                    // The harp rippling up the chord in quavers, the bass held under it.
                    int[] shape = section == 4 ? new[] { 0, 1, 2, 3, 4, 5 } : new[] { 0, 2, 4, 5, 4, 2 };
                    for (int e = 0; e < 6; e++)
                    {
                        int st = shape[(e + (section == 4 && b % 2 == 1 ? 3 : 0)) % 6];
                        SkyPluck(bed, T(e * 0.5f), Freq(ch[st % ch.Length] + 12 * (st / ch.Length)), e == 0 ? 0.05f : 0.035f, 3f);
                    }
                    SkyPluck(bed, T(0f), Freq(bass), 0.18f, 3f);
                    SkyPadNote(bed, T(0f), 3f * beat, Freq(bass), 0.05f);
                }
                foreach (int c in ch) SkyPadNote(bed, T(0f), 3f * beat, Freq(c), section >= 3 && section <= 5 ? 0.012f : 0.008f);
                if (section == 3 || section == 4) SkyBowed(strings, T(0f), 3f * beat * 0.98f, Freq(root), 0.03f, 4);
                if (section == 5 || section == 6) SkyBowed(strings, T(0f), 3f * beat * 0.97f, Freq(bass + 12), 0.025f, 3);
                if (section == 4)
                {
                    var g = Ch(SkyChords[b], 84);
                    for (int k = 0; k < 3; k++) SkyBell(bed, T(k), Freq(g[(b + k) % g.Length]), 0.012f, 2.5f);
                }
                if (section == 0 || section == 4)
                {
                    var h = Ch(SkyChords[b], 79);
                    SkyBell(bed, T(1f), Freq(h[b % h.Length]), 0.025f, 4f);
                    SkyBell(bed, T(2f), Freq(h[(b + 1) % h.Length]), 0.02f, 4f);
                }
                if (section == 2 || section == 5)
                {
                    // The flute's counter-tune, floating up into the top of each chord.
                    var g = Ch(SkyChords[b], 72);
                    SkyFlute(bed, T(0f), beat * 0.95f, Freq(g[b % g.Length]), 0.02f);
                    SkyFlute(bed, T(1f), beat * 1.9f, Freq(g[(b + 1) % g.Length]), 0.024f);
                }
                string horn = section == 3 ? SkyHornB[b - 40] : section == 4 ? SkyHornC[b - 56] : section == 5 ? SkyHornA[b - 72] : null;
                if (horn != null)
                    foreach (var nt in Line(horn, b0)) SkyBrass(bed, nt.Beat * beat, nt.Len * beat * 0.95f, Freq(nt.Midi), 0.03f);
                if (section == 5) SkyTimp(bed, T(0f), Freq(bass + 12), 0.05f);
                bool start = b == 8 || b == 24 || b == 40 || b == 56 || b == 72 || b == 88;
                bool end = b == 23 || b == 39 || b == 55 || b == 71 || b == 87;
                if (start) SkyTimp(bed, T(0f), Freq(root), 0.1f);
                if (end)
                {
                    // A drum roll and a swell of cymbal into the next part.
                    int nr = Root(next, 43);
                    for (int k = 0; k < 8; k++) SkyTimp(bed, T(1f + k * 0.25f), Freq(nr), 0.02f + k * 0.01f);
                    SkyCymbal(bed, T(2f), beat * 2.5f, 0.05f);
                }
                if (end || b == 7)
                {
                    // And a harp glissando sweeping up into it.
                    var into = Ch(next, 55);
                    for (int i = 0; i < 18; i++) SkyPluck(bed, T(1.2f) + i * beat * 1.7f / 18f, Freq(into[i % into.Length] + 12 * (i / into.Length)), 0.04f, 3f);
                }

                // ----- The extra layers -----
                // Winds: a music box and bells glinting on the off-beats.
                var sp = Ch(SkyChords[b], 84);
                SkyBell(winds, T(1.5f), Freq(sp[b % sp.Length]), 0.012f, 3.5f);
                if (b % 2 == 0) SkyBell(winds, T(2.5f), Freq(sp[(b + 2) % sp.Length]), 0.009f, 3.5f);
                // Grand: a low swell of horns under every bar, and a cymbal on each new part.
                if (section >= 3) SkyBrass(grand, T(0f), 3f * beat * 0.95f, Freq(root), 0.016f);
                if (start) SkyCymbal(grand, T(0f) - beat * 0.5f, beat * 1.2f, 0.06f);
            }

            // ----- The tune -----
            for (int b = 0; b < bars; b++)
            {
                int section = SkySection(b);
                foreach (var nt in Line(SkyTune[b], b * 3f))
                {
                    float t = nt.Beat * beat, len = nt.Len * beat, f = Freq(nt.Midi);
                    switch (section)
                    {
                        case 0: SkyBell(bed, t, f, 0.035f, 3f); break;
                        case 1: SkyAccordion(bed, t, len * 0.92f, f, 0.05f); SkyPianoNote(bed, t, len * 0.8f, f, 0.04f); break;
                        case 2: SkyBowed(strings, t, len * 0.97f, f, 0.05f, 4); SkyBell(bed, t, f, 0.02f, 3f); break;
                        case 3: SkyBowed(strings, t, len * 0.98f, f, 0.055f, 4); SkyBowed(strings, t, len * 0.98f, f * 0.5f, 0.025f, 3); SkyFlute(bed, t, len * 0.95f, f, 0.03f); break;
                        case 4: SkyBowed(strings, t, len * 0.98f, f, 0.05f, 4); SkyBell(bed, t, f, 0.03f, 3f); SkyFlute(bed, t, len * 0.95f, f * 0.5f, 0.028f); break;
                        case 5: SkyBowed(strings, t, len * 0.98f, f * 2f, 0.04f, 4); SkyBowed(strings, t, len * 0.98f, f, 0.035f, 3); SkyAccordion(bed, t, len * 0.92f, f, 0.04f); SkyPianoNote(bed, t, len * 0.8f, f, 0.04f); break;
                        default: SkyBowed(strings, t, len * 0.98f, f, 0.05f, 4); SkyFlute(bed, t, len * 0.95f, f, 0.03f); SkyBell(bed, t, f, 0.03f, 3f); SkyPianoNote(bed, t, len, f * 0.5f, 0.05f); break;
                    }
                }
            }

            // The room: a soft echo on everything, the same on every layer.
            for (int i = 0; i < MuseumStemCount; i++) SkyRoom(data[i]);
            // (Only the faintest softening at the very top, as in the preview.)
            int loopLen = Mathf.CeilToInt(bar * bars * Rate);
            for (int i = 0; i < MuseumStemCount; i++)
            {
                // Fold the echo that rings past the end back onto the start, so the loop is seamless.
                var d = data[i];
                for (int s = loopLen; s < n; s++) d[s - loopLen] += d[s];
                System.Array.Resize(ref data[i], loopLen);
            }
            return FinishStems(data, "pw_sky_", new[] { "bed", "strings", "winds", "grand" }, 10000f);
        }

        // ------------------------------------------------------------------ The Skyward instruments
        // Simple, clear voices: a few sine partials each, with gentle envelopes.

        static System.Random skyNoise = new System.Random(156);

        static float SkyRand() { return (float)(skyNoise.NextDouble() * 2.0 - 1.0) * 1.7f; }

        static void SkyPut(float[] d, float t0, int i, float v)
        {
            int k = Mathf.FloorToInt(t0 * Rate) + i;
            if (k >= 0 && k < d.Length) d[k] += v;
        }

        // A harp string (or a plucked bass): bright for a moment, then ringing away.
        static void SkyPluck(float[] d, float t0, float f, float v, float dec)
        {
            SkyRing(d, t0, f, 2f, 0.4f, 6f, v, dec);
        }

        // Two sine partials (the note, and another at `ratio` times it that fades `fade` times
        // faster), dying away at `dec` per second. Worked out by turning a pair of phasors
        // rather than calling Sin every sample, and stopped once it's too quiet to hear.
        static void SkyRing(float[] d, float t0, float f, float ratio, float amp2, float fade, float v, float dec)
        {
            int start = Mathf.FloorToInt(t0 * Rate);
            int count = Mathf.Min(2 * Rate, Mathf.CeilToInt(Mathf.Log(v * 2e4f + 1f) / dec * Rate) + 1);
            double w1 = 2.0 * System.Math.PI * f / Rate, w2 = w1 * ratio;
            double c1 = System.Math.Cos(w1), s1 = System.Math.Sin(w1), c2 = System.Math.Cos(w2), s2 = System.Math.Sin(w2);
            double x1 = 1.0, y1 = 0.0, x2 = 1.0, y2 = 0.0;
            double e = v, ek = System.Math.Exp(-dec / Rate), e2 = amp2, ek2 = System.Math.Exp(-fade / Rate);
            for (int i = 0; i < count; i++)
            {
                int k = start + i;
                if (k >= 0 && k < d.Length) d[k] += (float)((y1 + y2 * e2) * e);
                double nx = x1 * c1 - y1 * s1; y1 = x1 * s1 + y1 * c1; x1 = nx;
                nx = x2 * c2 - y2 * s2; y2 = x2 * s2 + y2 * c2; x2 = nx;
                e *= ek; e2 *= ek2;
            }
        }

        static void SkyPianoNote(float[] d, float t0, float dur, float f, float v)
        {
            int count = Mathf.CeilToInt((dur + 0.6f) * Rate);
            float w = 2f * Mathf.PI * f / Rate, dec = 1.3f + f / 600f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Exp(-t * dec);
                if (t > dur) env *= Mathf.Exp(-(t - dur) * 8f);
                float s = (Mathf.Sin(w * i) + 0.5f * Mathf.Sin(2f * w * i) * env + 0.25f * Mathf.Sin(3f * w * i) * env) * env;
                SkyPut(d, t0, i, s * v * Mathf.Clamp01(t / 0.003f));
            }
        }

        // Violins (or cellos): a bowed note, its vibrato warming up once it's settled.
        static void SkyBowed(float[] d, float t0, float dur, float f, float v, int harmonics)
        {
            int count = Mathf.CeilToInt((dur + 0.25f) * Rate);
            float ph = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.09f) * Mathf.Clamp01((dur + 0.25f - t) / 0.25f);
                float vib = 1f + 0.0042f * Mathf.Clamp01((t - 0.12f) / 0.25f) * Mathf.Sin(2f * Mathf.PI * 5.6f * t);
                ph += 2f * Mathf.PI * f * vib / Rate;
                float s = 0f;
                for (int k = 1; k <= harmonics; k++) s += (Mathf.Sin(ph * k) + Mathf.Sin(ph * k * 1.0045f)) / (k * 1.15f);
                SkyPut(d, t0, i, s * 0.45f * env * v);
            }
        }

        static void SkyAccordion(float[] d, float t0, float dur, float f, float v)
        {
            int count = Mathf.CeilToInt((dur + 0.08f) * Rate);
            float w = 2f * Mathf.PI * f / Rate;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.03f) * Mathf.Clamp01((dur + 0.08f - t) / 0.08f);
                float s = 0f;
                for (int k = 1; k <= 6; k++)
                {
                    float a = 1f / Mathf.Pow(k, 0.8f);
                    s += Mathf.Sin(w * i * k) * a + 0.8f * Mathf.Sin(w * i * k * 1.006f) * a;
                }
                SkyPut(d, t0, i, s * 0.3f * env * v * (1f + 0.1f * Mathf.Sin(2f * Mathf.PI * 6f * t)));
            }
        }

        static void SkyFlute(float[] d, float t0, float dur, float f, float v)
        {
            int count = Mathf.CeilToInt((dur + 0.1f) * Rate);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.06f) * Mathf.Clamp01((dur + 0.1f - t) / 0.1f);
                float ph = 2f * Mathf.PI * f * (t + 0.0025f * Mathf.Sin(2f * Mathf.PI * 5f * t) / (2f * Mathf.PI * 5f));
                float s = Mathf.Sin(ph) + 0.2f * Mathf.Sin(2f * ph) + 0.02f * SkyRand();
                SkyPut(d, t0, i, s * env * v);
            }
        }

        static void SkyBrass(float[] d, float t0, float dur, float f, float v)
        {
            int count = Mathf.CeilToInt((dur + 0.15f) * Rate);
            float w = 2f * Mathf.PI * f / Rate;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.08f) * Mathf.Clamp01((dur + 0.15f - t) / 0.15f);
                float s = 0f, a = 1f;
                for (int k = 1; k <= 7; k++) { a *= 0.9f; s += Mathf.Sin(w * i * k) * a; }
                SkyPut(d, t0, i, s * 0.35f * env * v);
            }
        }

        // A bell, a celesta or a music box, depending on how quickly it dies away.
        static void SkyBell(float[] d, float t0, float f, float v, float dec)
        {
            SkyRing(d, t0, f, 3.01f, 0.3f, 4f, v, dec);
        }

        static void SkyTimp(float[] d, float t0, float f, float v)
        {
            int count = Mathf.CeilToInt(1.5f * Rate);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float s = Mathf.Sin(2f * Mathf.PI * f * t * (1f + 0.05f * Mathf.Exp(-t * 20f))) * Mathf.Exp(-t * 3f) + 0.3f * SkyRand() * Mathf.Exp(-t * 30f);
                SkyPut(d, t0, i, s * v);
            }
        }

        static void SkyPadNote(float[] d, float t0, float dur, float f, float v)
        {
            int count = Mathf.CeilToInt(dur * Rate);
            int start = Mathf.FloorToInt(t0 * Rate);
            double w = 2.0 * System.Math.PI * f / Rate, c = System.Math.Cos(w), sn = System.Math.Sin(w);
            double x = 1.0, y = 0.0;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.3f) * Mathf.Clamp01((dur - t) / 0.3f);
                // (sin 2a = 2 sin a cos a)
                int k = start + i;
                if (k >= 0 && k < d.Length) d[k] += (float)((y + 0.3 * 2.0 * y * x) * env * v);
                double nx = x * c - y * sn; y = x * sn + y * c; x = nx;
            }
        }

        static void SkyCymbal(float[] d, float t0, float dur, float v)
        {
            int count = Mathf.CeilToInt(dur * Rate);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / (dur * 0.7f)) * Mathf.Clamp01((dur - t) / 0.2f);
                SkyPut(d, t0, i, SkyRand() * env * v * 0.3f);
            }
        }

        // A soft room: sixty faint echoes scattered through the next second and three quarters.
        static void SkyRoom(float[] d)
        {
            var rng = new System.Random(1800);
            const int taps = 60;
            var at = new int[taps];
            var gain = new float[taps];
            for (int k = 0; k < taps; k++)
            {
                at[k] = (int)(0.02f * Rate) + rng.Next((int)(1.78f * Rate));
                gain[k] = Mathf.Exp(-k / 18f) * (0.3f + 0.7f * (float)rng.NextDouble()) * 0.25f;
            }
            var dry = (float[])d.Clone();
            int n = d.Length;
            for (int k = 0; k < taps; k++)
            {
                int o = at[k];
                float g = gain[k];
                for (int s = o; s < n; s++) d[s] += dry[s - o] * g;
            }
        }
    }
}

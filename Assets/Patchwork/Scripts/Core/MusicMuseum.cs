using UnityEngine;

namespace Patchwork
{
    // The Oceanic Museum's soundtrack: a sweeping carousel waltz for piano and orchestra. It
    // begins wistful, in a minor key, the piano turning its little tune over on its own; the
    // orchestra takes it up; then the key lifts into the major and the strings soar, the way
    // the roof of a great hall opens over you; and it comes home to the minor at full sail.
    // (An original piece, written in the spirit of the grand film and game waltzes.)
    //
    // The layers:
    //   Bed     - always on: the piano, the plucked oom-pah-pah, harp, and a soft cushion
    //   Strings - always on in the halls: violins singing the tune, cellos answering
    //   Winds   - accordion, flute and glockenspiel, once you've found a Heartstring (or
    //             reached the aquarium)
    //   Grand   - horns, timpani and the big swells, once the lift is open (and always in the
    //             Observatory)
    // In the glass tunnels on the sea bed the whole thing goes soft and far away, as if you
    // were hearing it through the water.
    //
    // 3/4 at 138 BPM, 72 bars:
    //   Intro (1-8)    the piano alone, G minor
    //   A  (9-24)      the carousel tune
    //   A' (25-40)     again, violins on top, turning towards the major at the end
    //   B  (41-56)     B flat major: long soaring notes, harp running underneath
    //   A" (57-72)     the tune once more with everything, and home
    public partial class MusicPlayer
    {
        enum MuseumStem { Bed, Strings, Winds, Grand }
        const int MuseumStemCount = 4;
        const float MuseumBpm = 138f;

        static readonly string[] MuseumChords =
        {
            "Gm", "Gm", "Cm", "Cm", "D7", "D7", "Gm", "D7",

            "Gm", "Gm", "Cm", "Cm", "F", "F", "Bb", "Bb",
            "Eb", "Eb", "Cm", "Cm", "D7", "D7", "Gm", "D7",

            "Gm", "Gm", "Cm", "Cm", "F", "F", "Bb", "Bb",
            "Eb", "Eb", "Cm", "Cm", "D7", "D7", "Gm", "F7",

            "Bb", "Dm", "Eb", "F", "Gm", "Eb", "Cm7", "F7",
            "Bb", "Dm", "Eb", "F", "Eb", "Ebm", "Bb", "D7",

            "Gm", "Gm", "Cm", "Cm", "F", "F", "Bb", "Bb",
            "Eb", "Eb", "Cm", "Cm", "D7", "D7", "Gm", "Gm",
        };

        // One bar per entry, lengths in beats (3 a bar).
        static readonly string[] MuseumTune =
        {
            "Bb5:2 A5:1", "G5:3", "Eb6:2 D6:1", "C6:3", "D6:1 C6:1 A5:1", "F#5:3", "G5:3", "r:1 D5:1 F#5:1",

            "Bb5:2 A5:1", "G5:1 D5:1 G5:1", "Eb6:2 D6:1", "C6:1 G5:1 C6:1", "A5:2 G5:1", "F5:1 C5:1 F5:1", "D6:2 C6:1", "Bb5:3",
            "G6:2 F6:1", "Eb6:1 Bb5:1 Eb6:1", "F6:1.5 Eb6:0.5 D6:1", "C6:2 Eb6:1", "D6:1 F#5:1 A5:1", "C6:2 A5:1", "Bb5:1 A5:1 G5:1", "F#5:2 D5:1",

            "Bb5:2 A5:1", "G5:1 D5:1 G5:1", "Eb6:2 D6:1", "C6:1 G5:1 C6:1", "A5:2 G5:1", "F5:1 C5:1 F5:1", "D6:2 C6:1", "Bb5:3",
            "G6:2 F6:1", "Eb6:1 Bb5:1 Eb6:1", "F6:1.5 Eb6:0.5 D6:1", "C6:2 Eb6:1", "D6:1 F#5:1 A5:1", "C6:1 Bb5:1 A5:1", "G5:3", "r:1 F5:1 A5:1",

            "F5:1 Bb5:1 D6:1", "F6:3", "G6:2 F6:1", "Eb6:1 D6:1 C6:1", "D6:2 Bb5:1", "G5:1 Bb5:1 Eb6:1", "D6:2 C6:1", "A5:1 C6:1 Eb6:1",
            "D6:1 F6:1 Bb6:1", "A6:3", "G6:1.5 F6:0.5 Eb6:1", "F6:2 A6:1", "Bb6:2 G6:1", "Gb6:2 Eb6:1", "D6:3", "C6:1 A5:1 F#5:1",

            "Bb5:2 A5:1", "G5:1 D5:1 G5:1", "Eb6:2 D6:1", "C6:1 G5:1 C6:1", "A5:2 G5:1", "F5:1 C5:1 F5:1", "D6:2 C6:1", "Bb5:3",
            "G6:2 F6:1", "Eb6:1 Bb5:1 Eb6:1", "F6:1.5 Eb6:0.5 D6:1", "C6:2 Eb6:1", "D6:1 F#5:1 A5:1", "C6:1 Bb5:1 A5:1", "G5:3", "r:3",
        };

        // 0 intro, 1 A, 2 A', 3 B, 4 A".
        static int MuseumSection(int bar)
        {
            return bar < 8 ? 0 : bar < 24 ? 1 : bar < 40 ? 2 : bar < 56 ? 3 : 4;
        }

        // A piano: a struck string. Bright at the hammer, then each overtone dies away at its
        // own pace (the high ones first), and the damper comes down when the note is let go.
        static void AddPiano(float[] data, float start, float dur, float f, float vol, bool full = true)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            float ring = Mathf.Lerp(1.1f, 0.5f, Mathf.InverseLerp(100f, 2000f, f));
            if (!full) ring *= 0.6f;
            int count = Mathf.CeilToInt((dur + ring) * Rate);
            float decay = 1.3f + f / 600f;
            float p1 = 0f, p2 = 0f, p3 = 0f, p4 = 0f, p5 = 0f, p6 = 0f;
            float w = 2f * Mathf.PI * f / Rate;
            // Real strings are a touch stiff, so the overtones run slightly sharp.
            float w2 = w * 2f * 1.0008f, w3 = w * 3f * 1.002f, w4 = w * 4f * 1.0035f, w5 = w * 5f * 1.0055f, w6 = w * 6f * 1.008f;
            float bright = Mathf.Lerp(1f, 0.45f, Mathf.InverseLerp(200f, 1800f, f));
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.003f);
                if (t > dur) env *= Mathf.Exp(-(t - dur) * (5f / ring));
                p1 += w; p2 += w2; p3 += w3; p4 += w4; p5 += w5; p6 += w6;
                float d = Mathf.Exp(-t * decay);
                float d2 = d * d;
                float v = Mathf.Sin(p1) * d
                    + Mathf.Sin(p2) * 0.52f * bright * d * Mathf.Sqrt(d)
                    + Mathf.Sin(p3) * 0.3f * bright * d2;
                // (Accompaniment notes make do with three overtones: nobody can tell, and the
                // tune takes half as long to write out.)
                if (full)
                    v += Mathf.Sin(p4) * 0.17f * bright * d2 * Mathf.Sqrt(d)
                        + Mathf.Sin(p5) * 0.1f * bright * d2 * d
                        + Mathf.Sin(p6) * 0.06f * bright * d2 * d2;
                // The knock of the hammer.
                if (t < 0.02f) v += Mathf.Sin(p1 * 7.3f) * (1f - t / 0.02f) * 0.12f;
                Add(data, s0 + i, v * env * vol);
            }
        }

        // A section of violins singing: quick to speak, with a vibrato that warms up once the
        // note has settled.
        static void AddViolins(float[] data, float start, float dur, float f, float vol, int harmonics = 4)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            const float release = 0.22f;
            int count = Mathf.CeilToInt((dur + release) * Rate);
            float pa = 0f, pb = 0f;
            float top = Mathf.Lerp(1f, 0.5f, Mathf.InverseLerp(400f, 2200f, f));
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.09f) * Mathf.Clamp01((dur + release - t) / release);
                // A little swell through the note, like a bow leaning in.
                env *= 0.82f + 0.18f * Mathf.Sin(Mathf.Clamp01(t / Mathf.Max(0.2f, dur)) * Mathf.PI);
                float vib = 1f + 0.0042f * Mathf.Clamp01((t - 0.12f) / 0.25f) * Mathf.Sin(2f * Mathf.PI * 5.6f * t);
                float w = 2f * Mathf.PI * f * vib / Rate;
                pa += w; pb += w * 1.0045f;
                float v = 0f;
                for (int h = 1; h <= harmonics; h++)
                {
                    float a = (h == 1 ? 1f : top) / (h * 1.15f);
                    v += (Mathf.Sin(pa * h) + Mathf.Sin(pb * h)) * a;
                }
                Add(data, s0 + i, v * 0.45f * env * vol);
            }
        }

        static AudioClip[] museumStems;
        AudioSource[] museumSources;
        AudioLowPassFilter[] museumFilters;
        float museumMix;
        readonly float[] museumLayer = { 1f, 1f, 0f, 0f };
        readonly bool[] museumWant = { true, true, false, false };
        float museumDeep, museumDeepWant;

        // Which layers of the waltz are playing.
        public static void SetMuseumLayers(bool strings, bool winds, bool grand)
        {
            if (instance == null) return;
            instance.museumWant[(int)MuseumStem.Strings] = strings;
            instance.museumWant[(int)MuseumStem.Winds] = winds;
            instance.museumWant[(int)MuseumStem.Grand] = grand;
        }

        // How far "under the water" the music sounds: 0 clear, 1 soft and far away.
        public static void SetMuseumDepth(float depth)
        {
            if (instance != null) instance.museumDeepWant = Mathf.Clamp01(depth);
        }

        void StartMuseum()
        {
            museumDeep = museumDeepWant = 0f;
            for (int i = 2; i < MuseumStemCount; i++) { museumWant[i] = false; museumLayer[i] = 0f; }
            museumWant[1] = true;
            museumLayer[1] = 1f;
            if (museumSources != null) return;
            if (museumStems == null || museumStems[0] == null) museumStems = ComposeMuseum();
            museumSources = new AudioSource[MuseumStemCount];
            museumFilters = new AudioLowPassFilter[MuseumStemCount];
            for (int i = 0; i < MuseumStemCount; i++)
            {
                var go = new GameObject("Music Museum " + (MuseumStem)i);
                go.transform.SetParent(transform, false);
                var src = go.AddComponent<AudioSource>();
                src.clip = museumStems[i];
                src.loop = true;
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                src.volume = SilentVolume;
                src.priority = 0;
                src.ignoreListenerPause = true;
                museumFilters[i] = go.AddComponent<AudioLowPassFilter>();
                museumSources[i] = src;
            }
            StartTogether(museumSources, 0);
        }

        float museumStepCheck = 1.3f;
        int museumStepFixes;

        // Fades the museum's waltz in or out. Returns how loud the other music should be.
        float UpdateMuseumTrack(float udt)
        {
            float want = track == MusicTrack.Museum ? 1f : 0f;
            museumMix = Mathf.MoveTowards(museumMix, want, udt / 1.5f);
            if (museumSources != null)
            {
                museumDeep = Mathf.MoveTowards(museumDeep, museumDeepWant, udt / 1.8f);
                // Under the water: the top comes off the sound and it drops back a little.
                float deepCut = Mathf.Min(cutoff, 22000f * Mathf.Pow(0.05f, museumDeep));
                float deepVol = 1f - 0.18f * museumDeep;
                for (int i = 0; i < MuseumStemCount; i++)
                {
                    if (museumSources[i] == null) continue;
                    museumLayer[i] = Mathf.MoveTowards(museumLayer[i], museumWant[i] ? 1f : 0f, udt / 2.5f);
                    float gain = Mathf.Sin(museumLayer[i] * Mathf.PI * 0.5f);
                    museumSources[i].volume = Mathf.Max(SilentVolume, master * deepVol * gain * Mathf.Sin(museumMix * Mathf.PI * 0.5f));
                    museumFilters[i].cutoffFrequency = deepCut;
                }
                // The layers must stay on exactly the same sample (see "Keeping layers in step").
                museumStepCheck -= udt;
                if (museumStepCheck <= 0f && museumStepFixes < 4)
                {
                    museumStepCheck = 1.3f;
                    if (OutOfStep(museumSources))
                    {
                        museumStepFixes++;
                        StartTogether(museumSources, museumSources[0].timeSamples);
                    }
                }
                if (museumMix <= 0f && track != MusicTrack.Museum)
                {
                    foreach (var s in museumSources) if (s != null) Destroy(s.gameObject);
                    museumSources = null;
                    museumStepFixes = 0;
                }
            }
            return Mathf.Cos(museumMix * Mathf.PI * 0.5f);
        }

        static AudioClip[] ComposeMuseum()
        {
            float beat = 60f / MuseumBpm;
            float bar = beat * 3f;
            int bars = MuseumChords.Length;
            int n = Mathf.CeilToInt(bar * bars * Rate);
            var data = new float[MuseumStemCount][];
            for (int i = 0; i < MuseumStemCount; i++) data[i] = new float[n];
            var bed = data[(int)MuseumStem.Bed];
            var strings = data[(int)MuseumStem.Strings];
            var winds = data[(int)MuseumStem.Winds];
            var grand = data[(int)MuseumStem.Grand];

            for (int b = 0; b < bars; b++)
            {
                int section = MuseumSection(b);
                bool intro = section == 0, major = section == 3, last = section == 4;
                bool sectionEnd = b == 7 || b == 23 || b == 39 || b == 55 || b == bars - 1;
                float b0 = b * 3f;
                string name = MuseumChords[b];
                string next = MuseumChords[(b + 1) % bars];
                bool sameAsBefore = b > 0 && MuseumChords[b - 1] == name;
                var ch = Ch(name, 55);
                int root = Root(name, 31);
                int bassNote = sameAsBefore ? root + 7 : root;

                // ----- Bed: the waltz -----
                if (intro)
                {
                    // The piano on its own: a low note, and the chord spread gently after it.
                    AddPiano(bed, b0 * beat, bar * 0.95f, Freq(root + 12), 0.11f);
                    for (int c = 0; c < 3; c++)
                        AddPiano(bed, (b0 + 1f + c * 0.5f) * beat, beat * 1.4f, Freq(ch[c % ch.Length] + (c == 2 ? 12 : 0)), 0.05f, false);
                }
                else
                {
                    // Oom: plucked basses and the piano's left hand.
                    AddPizz(bed, b0 * beat, Freq(bassNote + 12), 0.2f, 5000 + b * 3);
                    AddPiano(bed, b0 * beat, beat * 0.9f, Freq(bassNote + 12), 0.085f, false);
                    AddBass(bed, b0 * beat, beat * 0.8f, Freq(bassNote), 0.05f);
                    // Pah-pah: the chord, short, on two and three.
                    for (int k = 1; k <= 2; k++)
                    {
                        for (int c = 0; c < 3; c++)
                            AddPiano(bed, (b0 + k) * beat + c * 0.006f, beat * 0.42f, Freq(ch[c % ch.Length]), major ? 0.03f : 0.038f, false);
                        AddPluck(bed, (b0 + k) * beat, beat * 0.8f, Freq(ch[(k + b) % 3] + 12), 0.034f, 5200 + b * 4 + k);
                    }
                }
                // In the major section the harp runs up and down the chord in quavers.
                if (major)
                    for (int e8 = 0; e8 < 6; e8++)
                    {
                        int step = e8 < 4 ? e8 : 6 - e8;
                        int note = ch[step % ch.Length] + 12 * (step / ch.Length) + 12;
                        AddPluck(bed, (b0 + e8 * 0.5f) * beat, beat * 1.2f, Freq(note), 0.04f, 5400 + b * 8 + e8);
                    }
                foreach (int c in ch) AddPad(bed, b0 * beat, bar, Freq(c - 12), intro ? 0.008f : 0.011f);

                // ----- Strings: the cushion, and the cellos' slow answer under the tune -----
                foreach (int c in ch) AddPad(strings, b0 * beat, bar, Freq(c), major || last ? 0.015f : 0.011f);
                if (section >= 2)
                {
                    // One long note a bar, walking through the thirds of the chords.
                    int third = ch[1] - 12;
                    AddViolins(strings, b0 * beat - 0.03f, bar * 0.96f, Freq(third), 0.026f, 3);
                    if (major || last) AddViolins(strings, b0 * beat - 0.03f, bar * 0.96f, Freq(ch[0] - 12), 0.02f, 3);
                }
                if (!intro) AddBass(strings, b0 * beat, bar * 0.9f, Freq(root + 12), 0.04f);

                // ----- Winds: sparkle -----
                if (major && b % 2 == 0)
                {
                    var g = Ch(name, 84);
                    for (int i = 0; i < 3; i++) AddCelesta(winds, (b0 + 0.5f + i) * beat, beat * 0.8f, Freq(g[i % g.Length]), 0.02f);
                }
                if (sectionEnd)
                {
                    // A harp glissando sweeping into each new section.
                    var into = Ch(next, 55);
                    var run = new int[14];
                    for (int i = 0; i < run.Length; i++) run[i] = into[i % 3] + 12 * (i / 3);
                    AddGliss(winds, (b0 + 1.5f) * beat, run, beat * 1.45f, 0.055f, 5600 + b * 14);
                }

                // ----- Grand: horns, drums, swells -----
                if (major || last)
                {
                    int r = Root(name, 43);
                    AddBrass(grand, b0 * beat - BrassLead, bar * 0.94f, Freq(r), 0.034f);
                    AddBrass(grand, b0 * beat - BrassLead, bar * 0.94f, Freq(r + 7), 0.024f);
                    if (last || b >= 48) AddBrass(grand, b0 * beat - BrassLead, bar * 0.94f, Freq(ch[1]), 0.022f);
                    AddTimpani(grand, b0 * beat, Freq(Root(name, 36)), last ? 0.12f : 0.07f);
                }
                else if (section == 2 && b % 2 == 0)
                    AddTimpani(grand, b0 * beat, Freq(Root(name, 36)), 0.06f);
                if (!intro)
                {
                    // A soft brushed lilt on two and three.
                    AddBrush(grand, (b0 + 1f) * beat, beat * 0.5f, 0.012f, 5800 + b * 2);
                    AddBrush(grand, (b0 + 2f) * beat, beat * 0.5f, 0.012f, 5801 + b * 2);
                }
                if (sectionEnd && b != 7)
                {
                    // A drum roll and a breath of cymbal into the next section.
                    int roll = Root(next, 36);
                    for (int s16 = 0; s16 < 9; s16++) AddTimpani(grand, (b0 + 0.75f + s16 * 0.25f) * beat, Freq(roll), 0.02f + s16 * 0.011f);
                    AddAirPad(grand, (b0 + 1f) * beat, beat * 2.6f, 0.05f, 5900 + b);
                }
            }

            // ----- The tune -----
            for (int b = 0; b < bars; b++)
            {
                int section = MuseumSection(b);
                bool major = section == 3, last = section == 4;
                foreach (var nt in Line(MuseumTune[b], b * 3f))
                {
                    float t = nt.Beat * beat;
                    float len = nt.Len * beat;
                    float f = Freq(nt.Midi);
                    // The piano always has it (in octaves once the orchestra is in).
                    AddPiano(bed, t, len, f, section == 0 ? 0.15f : 0.12f);
                    if (section >= 1) AddPiano(bed, t, len, f * 0.5f, 0.06f, false);
                    if (section == 0) AddCelesta(bed, t, len + 0.3f, f * 2f, 0.012f);
                    // Violins from the second time round; an octave higher as well where it soars.
                    if (section >= 2)
                    {
                        AddViolins(strings, t - 0.04f, len * 0.98f, f, major || last ? 0.06f : 0.05f);
                        if (major || last) AddViolins(strings, t - 0.04f, len * 0.98f, f * 2f, 0.03f, 3);
                    }
                    else if (section == 1 && nt.Len >= 2f)
                        AddViolins(strings, t - 0.04f, len * 0.98f, f, 0.024f);   // just leaning on the long notes
                    // Accordion in the minor sections (the carousel); flute and bells in the major.
                    if (section == 1 || section == 2 || last) AddAccordion(winds, t, len * 0.92f, f, section == 1 ? 0.03f : 0.038f);
                    if (major || last) AddFlute(winds, t - FluteLead, len * 0.95f, f * (major ? 2f : 1f), 0.05f);
                    if (major) AddGlock(winds, t, f * 2f, 0.016f);
                    // Horns join the tune for the last time through.
                    if (last && nt.Len >= 1f) AddBrass(grand, t - BrassLead, len * 0.9f, f * 0.5f, 0.03f);
                }
            }

            return FinishStems(data, "pw_museum_", new[] { "bed", "strings", "winds", "grand" }, 7500f);
        }
    }
}

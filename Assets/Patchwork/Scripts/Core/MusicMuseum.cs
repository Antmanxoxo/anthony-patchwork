using UnityEngine;

namespace Patchwork
{
    // The Oceanic Museum's soundtrack: a dreamy orchestral waltz, the kind you'd hear drifting
    // through a grand hall under the stars. It swells as you go deeper into the museum:
    //   Bed     - always on: harp, plucked bass, a soft pad, and the tune on celesta
    //   Strings - join once you've found your first Heartstring (or reached the aquarium)
    //   Winds   - flute on the tune and glockenspiel sparkles, once the lift is open
    //   Grand   - horns, timpani and a choir-like swell, up in the Observatory
    // In the glass tunnels on the sea bed the whole thing goes soft and far away, as if you
    // were hearing it through the water.
    //
    // F major, a lilting 3/4 at 108 BPM, 44 bars:
    //   Intro (1-4)    harp rippling, a pickup into...
    //   A     (5-20)   the tune, twice: big leaps up and long held notes
    //   B     (21-36)  two borrowed chords (Db and Eb) lift it somewhere far away, then it
    //                  climbs home to F at the top of its voice
    //   A'    (37-44)  the tune once more, and a wistful minor chord to turn it round
    public partial class MusicPlayer
    {
        enum MuseumStem { Bed, Strings, Winds, Grand }
        const int MuseumStemCount = 4;
        const float MuseumBpm = 108f;

        static readonly string[] MuseumChords =
        {
            "F", "F", "Bb", "C7",
            "F", "Am", "Bb", "C7", "Dm", "Bb", "Gm7", "C7",
            "F", "Am", "Bb", "C7", "Dm", "Gm7", "C7", "F",
            "Db", "Db", "Eb", "Eb", "F", "F", "Bb", "C7",
            "Db", "Eb", "F", "Dm", "Bb", "Gm7", "Csus4", "C7",
            "F", "Am", "Bb", "C7", "Dm", "Bbm", "C7", "F",
        };

        // One bar per entry, lengths in beats (3 a bar).
        static readonly string[] MuseumTune =
        {
            "r:3", "r:3", "r:2 C5:1", "D5:1 E5:1 G5:1",

            "A5:2 C6:1", "E6:3", "D6:1.5 C6:0.5 Bb5:1", "C6:2 G5:1",
            "A5:1 D6:1 F6:1", "F6:2 D6:1", "Bb5:1 D6:1 G6:1", "E6:2 C6:1",

            "A5:2 C6:1", "E6:2 G6:1", "F6:1.5 D6:0.5 Bb5:1", "C6:2 E6:1",
            "F6:1 A6:1 F6:1", "D6:2 Bb5:1", "G5:1 C6:1 E6:1", "F6:3",

            "Ab5:1 Db6:1 F6:1", "Ab6:2 F6:1", "G6:1.5 F6:0.5 Eb6:1", "Bb5:2 Eb6:1",
            "C6:1 F6:1 A6:1", "C7:3", "Bb6:1 A6:1 F6:1", "G6:2 E6:1",

            "F6:2 Ab6:1", "G6:2 Bb6:1", "A6:1.5 G6:0.5 F6:1", "D6:2 F6:1",
            "D6:1 F6:1 Bb6:1", "A6:1 G6:1 D6:1", "F6:2 G6:1", "E6:1.5 D6:0.5 C6:1",

            "A5:2 C6:1", "E6:3", "D6:1.5 C6:0.5 Bb5:1", "C6:1 E6:1 G6:1",
            "A6:2 F6:1", "Db6:2 F6:1", "G5:1 Bb5:1 E6:1", "F6:3",
        };

        // The horns' slow line under the far-away section (bars 21-36), one note a bar.
        static readonly string[] MuseumHorns =
        {
            "F4", "Ab4", "G4", "Bb4", "A4", "C5", "D5", "C5",
            "Ab4", "Bb4", "C5", "A4", "Bb4", "Bb4", "C5", "Bb4",
        };

        // 0 intro, 1 A, 2 A again, 3 B, 4 B again, 5 A'.
        static int MuseumSection(int bar)
        {
            return bar < 4 ? 0 : bar < 12 ? 1 : bar < 20 ? 2 : bar < 28 ? 3 : bar < 36 ? 4 : 5;
        }

        static AudioClip[] museumStems;
        AudioSource[] museumSources;
        AudioLowPassFilter[] museumFilters;
        float museumMix;
        readonly float[] museumLayer = { 1f, 0f, 0f, 0f };
        readonly bool[] museumWant = { true, false, false, false };
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
            for (int i = 1; i < MuseumStemCount; i++) { museumWant[i] = false; museumLayer[i] = 0f; }
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
                bool far = section == 3 || section == 4;
                bool sectionEnd = b == 3 || b == 11 || b == 19 || b == 27 || b == 35 || b == bars - 1;
                float b0 = b * 3f;
                string name = MuseumChords[b];
                string next = MuseumChords[(b + 1) % bars];
                var ch = Ch(name, 53);
                int root = Root(name, 36);

                // ----- Bed -----
                // The waltz itself: plucked bass on one, harp answering on two and three.
                AddPizz(bed, b0 * beat, Freq(root), 0.15f, 3000 + b * 3);
                if (b % 2 == 1) AddPizz(bed, (b0 + 2f) * beat, Freq(root + 7), 0.06f, 3001 + b * 3);
                if (section == 0)
                {
                    // Intro: the harp runs up the chord and over the top.
                    for (int e8 = 0; e8 < 6; e8++)
                    {
                        int note = ch[e8 % ch.Length] + 12 * (e8 / ch.Length);
                        AddPluck(bed, (b0 + e8 * 0.5f) * beat, beat * 1.6f, Freq(note), 0.06f, 3100 + b * 8 + e8);
                    }
                }
                else
                {
                    for (int k = 1; k <= 2; k++)
                        for (int c = 0; c < ch.Length && c < 3; c++)
                            AddPluck(bed, (b0 + k) * beat + c * 0.012f, beat * 1.1f, Freq(ch[c] + (k == 2 ? 12 : 0)), k == 1 ? 0.04f : 0.03f, 3200 + b * 16 + k * 4 + c);
                }
                // A soft warm cushion under everything.
                foreach (int c in ch) AddPad(bed, b0 * beat, bar, Freq(c - 12), 0.014f);
                // Celesta twinkling up the chord on the off-beats of every other bar.
                if (b % 2 == 0 && section != 0)
                    for (int i = 0; i < 3; i++) AddCelesta(bed, (b0 + 1.5f + i * 0.5f) * beat, beat * 0.6f, Freq(ch[i % ch.Length] + 24), 0.012f);

                // ----- Strings -----
                // Held chords, fuller and higher, with the basses bowing the root.
                foreach (int c in ch) AddPad(strings, b0 * beat, bar, Freq(c), far ? 0.02f : 0.016f);
                AddBass(strings, b0 * beat, bar * 0.95f, Freq(root), 0.06f);
                if (far) AddPad(strings, b0 * beat, bar, Freq(ch[0] + 12), 0.014f);

                // ----- Winds -----
                // A harp glissando sweeping into each new section.
                if (sectionEnd)
                {
                    var into = Ch(next, 60);
                    var run = new int[12];
                    for (int i = 0; i < run.Length; i++) run[i] = into[i % 3] + 12 * (i / 3) - 12;
                    AddGliss(winds, (b0 + 2f) * beat, run, beat * 0.95f, 0.05f, 3600 + b * 12);
                }
                // Glockenspiel stars high above, now and then.
                if (b % 4 == 2 && section != 0)
                {
                    var g = Ch(name, 84);
                    for (int i = 0; i < 3; i++) AddGlock(winds, (b0 + i) * beat, Freq(g[(i * 2) % g.Length]), 0.014f);
                }

                // ----- Grand -----
                if (section != 0)
                {
                    // Horns holding an open fifth, swelling a touch in the far-away part.
                    int r = Root(name, 48);
                    AddBrass(grand, b0 * beat - BrassLead, bar * 0.95f, Freq(r), far ? 0.03f : 0.02f);
                    AddBrass(grand, b0 * beat - BrassLead, bar * 0.95f, Freq(r + 7), far ? 0.022f : 0.014f);
                    // A voice-like "aah" two octaves over the root.
                    AddPad(grand, b0 * beat, bar, Freq(ch[ch.Length > 2 ? 2 : 0] + 12), 0.012f);
                    AddPad(grand, b0 * beat, bar, Freq(ch[0] + 24), 0.009f);
                }
                if (far)
                {
                    AddBrass(grand, b0 * beat - BrassLead, bar * 0.95f, Freq(NoteNum(MuseumHorns[b - 20])), 0.045f);
                    AddTimpani(grand, b0 * beat, Freq(Root(name, 38)), 0.11f);
                }
                if (b == 4 || b == 12 || b == 36) AddTimpani(grand, b0 * beat, Freq(Root(name, 38)), 0.1f);
                if (sectionEnd && b != 3)
                {
                    int roll = Root(next, 38);
                    for (int s16 = 0; s16 < 8; s16++) AddTimpani(grand, (b0 + 1f + s16 * 0.25f) * beat, Freq(roll), 0.02f + s16 * 0.01f);
                }
            }

            // ----- The tune -----
            for (int b = 0; b < bars; b++)
            {
                int section = MuseumSection(b);
                bool far = section == 3 || section == 4;
                foreach (var nt in Line(MuseumTune[b], b * 3f))
                {
                    float t = nt.Beat * beat;
                    float len = nt.Len * beat;
                    float f = Freq(nt.Midi);
                    // Always there: celesta, with a music box an octave under it.
                    AddCelesta(bed, t, len + 0.4f, f, far ? 0.034f : 0.04f);
                    AddMusicBox(bed, t, len + 0.3f, f * 0.5f, 0.014f);
                    // Strings sing it (and double it an octave down where it soars).
                    if (section != 0)
                    {
                        AddStrings(strings, t - 0.06f, len, f, far ? 0.04f : 0.034f);
                        if (far || section == 5) AddStrings(strings, t - 0.06f, len, f * 0.5f, 0.026f);
                    }
                    // The flute takes it from the second time round, with a grace note into
                    // the long notes.
                    if (section >= 2)
                    {
                        AddFlute(winds, t - FluteLead, len * 0.95f + 0.05f, f, 0.06f);
                        if (nt.Len >= 2f) AddFlute(winds, t - FluteLead - 0.05f, 0.05f, Freq(nt.Midi + 2), 0.028f);
                    }
                    if (far) AddGlock(winds, t, f, 0.018f);
                }
            }

            return FinishStems(data, "pw_museum_", new[] { "bed", "strings", "winds", "grand" }, 7000f);
        }
    }
}

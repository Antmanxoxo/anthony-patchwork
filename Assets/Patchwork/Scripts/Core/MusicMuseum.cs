using UnityEngine;

namespace Patchwork
{
    // The Oceanic Museum has three pieces to choose from (Patchwork > Level 2 Music in the menu):
    //   "The Skyward Waltz" (MusicMuseumSkyward.cs) - the one it plays now
    //   "The Star Carousel" (below)                 - the first waltz, kept so it can come back
    //   "The Lantern Tide" (MusicMuseumTide.cs)     - the second piece, kept too
    //
    // "The Star Carousel": a waltz in two moods that keep
    // turning into each other.
    //   The first is the night sky over the atrium: D major, a harp rippling under a long,
    //   wide-stepping tune for celesta and violins, the bass walking gently down the scale.
    //   The second is the merry-go-round on the quay: the same pulse slips into B minor and
    //   the piano and accordion whirl through a ring of chords, faster and faster on its feet.
    //   Then a drum roll, and the first tune comes back with the whole orchestra under it.
    // (An original piece, written in the spirit of the grand film and game waltzes.)
    //
    // The layers:
    //   Bed     - always on: harp, celesta, piano, plucked basses and a soft cushion
    //   Strings - always on in the halls: violins singing the tune, cellos underneath
    //   Winds   - flute, accordion and glockenspiel, once you've found a Heartstring (or
    //             reached the aquarium)
    //   Grand   - horns, timpani and the big swells, once the lift is open (and always in the
    //             Observatory)
    // In the glass tunnels on the sea bed the whole thing goes soft and far away, as if you
    // were hearing it through the water.
    //
    // 3/4 at 152 BPM (felt one-in-a-bar), 80 bars:
    //   Intro (1-8)     harp and celesta alone
    //   A  (9-24)       the star tune, D major
    //   A' (25-40)      again, violins an octave up, a horn answering; turns to the minor
    //   B  (41-64)      the carousel, B minor, round the ring of chords and climbing
    //   A" (65-80)      the star tune with everything, and home
    public partial class MusicPlayer
    {
        enum MuseumStem { Bed, Strings, Winds, Grand }
        const int MuseumStemCount = 4;
        const float MuseumBpm = 152f;

        static readonly string[] MuseumChords =
        {
            "D", "D", "G", "G", "D", "D", "Asus4", "A7",

            "D", "A", "Bm", "F#m", "G", "D", "Em7", "A7",
            "D", "A", "Bm", "F#m", "G", "Gm", "D", "A7",

            "D", "A", "Bm", "F#m", "G", "D", "Em7", "A7",
            "D", "A", "Bm", "F#m", "G", "Gm", "D", "F#7",

            "Bm", "Bm", "Em", "Em", "A7", "A7", "D", "D",
            "G", "G", "Em", "C#dim", "F#7", "F#7", "Bm", "F#7",
            "Bm", "Em", "A7", "D", "G", "Em", "F#7", "A7",

            "D", "A", "Bm", "F#m", "G", "D", "Em7", "A7",
            "D", "A", "Bm", "F#m", "G", "Gm", "D", "D",
        };

        // The note the basses play under each bar (the star tune's bass walks down the scale).
        static readonly string[] MuseumBass =
        {
            "D", "D", "D", "D", "D", "D", "A", "A",

            "D", "C#", "B", "A", "G", "F#", "E", "A",
            "D", "C#", "B", "A", "G", "G", "A", "A",

            "D", "C#", "B", "A", "G", "F#", "E", "A",
            "D", "C#", "B", "A", "G", "G", "A", "F#",

            "B", "B", "E", "E", "A", "A", "D", "D",
            "G", "G", "E", "C#", "F#", "F#", "B", "F#",
            "B", "E", "A", "D", "G", "E", "F#", "A",

            "D", "C#", "B", "A", "G", "F#", "E", "A",
            "D", "C#", "B", "A", "G", "G", "A", "D",
        };

        // One bar per entry, lengths in beats (3 a bar).
        static readonly string[] MuseumTune =
        {
            "r:1 A5:1 D6:1", "F#6:3", "r:1 B5:1 D6:1", "G6:3", "r:1 A5:1 D6:1", "F#6:2 A6:1", "E6:3", "r:2 E5:1",

            "A5:2 F#5:1", "E5:2 A5:1", "D6:2 C#6:1", "A5:3", "B5:2 G5:1", "F#5:2 A5:1", "G5:1 F#5:1 E5:1", "E5:2 E5:1",
            "A5:2 F#5:1", "E5:2 A5:1", "D6:2 E6:1", "F#6:2 C#6:1", "D6:2 B5:1", "Bb5:2 D6:1", "A5:1.5 G5:0.5 F#5:1", "E5:2 C#5:1",

            "A5:2 F#5:1", "E5:2 A5:1", "D6:2 C#6:1", "A5:3", "B5:2 G5:1", "F#5:2 A5:1", "G5:1 F#5:1 E5:1", "E5:2 E5:1",
            "A5:2 F#5:1", "E5:2 A5:1", "D6:2 E6:1", "F#6:2 C#6:1", "D6:2 B5:1", "Bb5:2 D6:1", "F#5:1 A5:1 D6:1", "C#6:2 A#5:1",

            "F#5:1 B5:1 D6:1", "C#6:0.5 D6:0.5 C#6:1 B5:1", "G5:1 B5:1 E6:1", "D6:0.5 E6:0.5 D6:1 B5:1", "C#6:1 E6:1 G6:1", "F#6:0.5 G6:0.5 F#6:1 E6:1", "F#6:2 D6:1", "A5:1 D6:1 F#6:1",
            "G6:2 F#6:1", "E6:1 D6:1 B5:1", "E6:2 D6:1", "C#6:1 B5:1 G5:1", "A#5:1 C#6:1 E6:1", "F#6:2 E6:1", "D6:1.5 C#6:0.5 B5:1", "F#5:1 A#5:1 C#6:1",
            "D6:1 F#6:1 B6:1", "G6:2 E6:1", "E6:1 G6:1 A6:1", "F#6:3", "B6:2 G6:1", "G6:1 F#6:1 E6:1", "F#6:2 C#6:1", "E6:1 C#6:1 A5:1",

            "A5:2 F#5:1", "E5:2 A5:1", "D6:2 C#6:1", "A5:3", "B5:2 G5:1", "F#5:2 A5:1", "G5:1 F#5:1 E5:1", "E5:2 E5:1",
            "A5:2 F#5:1", "E5:2 A5:1", "D6:2 E6:1", "F#6:2 C#6:1", "D6:2 B5:1", "Bb5:2 D6:1", "A5:1.5 G5:0.5 F#5:1", "D5:3",
        };

        // The horn's answer under the star tune: one long note a bar.
        static readonly string[] MuseumHorn =
        {
            "F#4", "E4", "F#4", "C#4", "D4", "D4", "B3", "C#4",
            "F#4", "E4", "F#4", "A4", "B4", "Bb4", "F#4", "E4",
        };

        // 0 intro, 1 A, 2 A', 3 B (the carousel), 4 A".
        static int MuseumSection(int bar)
        {
            return bar < 8 ? 0 : bar < 24 ? 1 : bar < 40 ? 2 : bar < 64 ? 3 : 4;
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
        static int museumStemsSong = -1;

        // Which piece the museum plays: 0 "The Skyward Waltz", 1 "The Star Carousel",
        // 2 "The Lantern Tide".
        public const string MuseumSongPref = "pw_l2_song";
        public static int MuseumSong
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(MuseumSongPref, 0), 0, 2); }
            set { PlayerPrefs.SetInt(MuseumSongPref, Mathf.Clamp(value, 0, 2)); PlayerPrefs.Save(); ReloadMuseum(); }
        }
        public static readonly string[] MuseumSongNames = { "The Skyward Waltz", "The Star Carousel", "The Lantern Tide" };

        // Swaps the piece over straight away if the museum's music is playing.
        public static void ReloadMuseum()
        {
            if (instance == null || instance.museumSources == null) return;
            foreach (var s in instance.museumSources) if (s != null) Destroy(s.gameObject);
            instance.museumSources = null;
            instance.museumStepFixes = 0;
            instance.StartMuseum();
        }
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
            if (museumStems == null || museumStems[0] == null || museumStemsSong != MuseumSong)
            {
                museumStemsSong = MuseumSong;
                museumStems = museumStemsSong == 1 ? ComposeCarousel() : museumStemsSong == 2 ? ComposeTide() : ComposeSkyward();
            }
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

        static AudioClip[] ComposeCarousel()
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
                bool intro = section == 0, whirl = section == 3, last = section == 4;
                bool climb = whirl && b >= 56;
                bool sectionEnd = b == 7 || b == 23 || b == 39 || b == 63 || b == bars - 1;
                int inSection = b - (section == 0 ? 0 : section == 1 ? 8 : section == 2 ? 24 : section == 3 ? 40 : 64);
                float b0 = b * 3f;
                string name = MuseumChords[b];
                string next = MuseumChords[(b + 1) % bars];
                var ch = Ch(name, 57);
                int bassNote = NoteNum(MuseumBass[b] + "2");
                if (bassNote < 38) bassNote += 12;   // keep the basses between D2 and C#3

                // ----- Bed -----
                if (!whirl)
                {
                    // The night sky: a harp rippling up the chord and back in quavers, each bar
                    // a little different so it never sounds like a machine.
                    for (int e8 = 0; e8 < 6; e8++)
                    {
                        int step = (b % 2 == 0) ? (e8 < 4 ? e8 : 6 - e8) : (e8 < 3 ? e8 + 1 : 5 - e8);
                        int note = ch[step % ch.Length] + 12 * (step / ch.Length);
                        float v = (e8 == 0 ? 0.05f : 0.036f) * (intro ? 1.15f : 1f);
                        AddPluck(bed, (b0 + e8 * 0.5f) * beat, beat * 1.6f, Freq(note), v, 5000 + b * 8 + e8);
                    }
                    // A plucked bass on the one, and a deep note under it once the tune is in.
                    AddPluck(bed, b0 * beat, bar * 0.9f, Freq(bassNote + 12), 0.06f, 5090 + b);
                    if (!intro) AddBass(bed, b0 * beat, bar * 0.85f, Freq(bassNote), 0.05f);
                    // Celesta stars, high up, on the off beats of every other bar.
                    if (b % 2 == 1 || intro)
                    {
                        var g = Ch(name, 81);
                        AddCelesta(bed, (b0 + 1f) * beat, beat, Freq(g[(b / 2) % g.Length]), 0.014f);
                        AddCelesta(bed, (b0 + 2f) * beat, beat, Freq(g[(b / 2 + 1) % g.Length] + (b % 4 == 3 ? 12 : 0)), 0.012f);
                    }
                }
                else
                {
                    // The carousel: oom on the basses and the piano's left hand...
                    AddPizz(bed, b0 * beat, Freq(bassNote + 12), 0.2f, 5100 + b * 3);
                    AddPiano(bed, b0 * beat, beat * 0.9f, Freq(bassNote + 12), 0.09f, false);
                    AddPiano(bed, b0 * beat, beat * 0.9f, Freq(bassNote), 0.05f, false);
                    AddBass(bed, b0 * beat, beat * 0.8f, Freq(bassNote), 0.055f);
                    // ...pah-pah on the chord, short and bright.
                    for (int k = 1; k <= 2; k++)
                        for (int c = 0; c < 3; c++)
                            AddPiano(bed, (b0 + k) * beat + c * 0.005f, beat * 0.4f, Freq(ch[c % ch.Length]), climb ? 0.044f : 0.038f, false);
                    // While it climbs the harp runs under it in quavers.
                    if (climb)
                        for (int e8 = 0; e8 < 6; e8++)
                        {
                            int note = ch[e8 % ch.Length] + 12 * (e8 / ch.Length);
                            AddPluck(bed, (b0 + e8 * 0.5f) * beat, beat * 1.2f, Freq(note), 0.034f, 5300 + b * 8 + e8);
                        }
                }
                foreach (int c in ch) AddPad(bed, b0 * beat, bar, Freq(c - 12), intro ? 0.008f : 0.011f);

                // ----- Strings: the cushion, and the cellos -----
                foreach (int c in ch) AddPad(strings, b0 * beat, bar, Freq(c), last || climb ? 0.016f : 0.011f);
                if (!intro)
                {
                    // Cellos hold the walking bass an octave up, so you hear it stepping down.
                    AddViolins(strings, b0 * beat - 0.03f, bar * 0.97f, Freq(bassNote + 12), whirl ? 0.022f : 0.03f, 3);
                    if (section == 2 || last) AddViolins(strings, b0 * beat - 0.03f, bar * 0.97f, Freq(ch[1] - 12), 0.02f, 3);
                }
                if (whirl)
                {
                    // Short bowed chords on two and three, pushing the waltz along.
                    for (int k = 1; k <= 2; k++)
                        AddViolins(strings, (b0 + k) * beat, beat * 0.35f, Freq(ch[(k + b) % 3]), 0.022f, 3);
                }

                // ----- Winds: sparkle -----
                if (sectionEnd)
                {
                    // A harp glissando sweeping into each new section.
                    var into = Ch(next, 57);
                    var run = new int[14];
                    for (int i = 0; i < run.Length; i++) run[i] = into[i % 3] + 12 * (i / 3);
                    AddGliss(winds, (b0 + 1.5f) * beat, run, beat * 1.45f, 0.055f, 5600 + b * 14);
                }
                if (last && b % 2 == 0)
                {
                    var g = Ch(name, 86);
                    for (int i = 0; i < 3; i++) AddGlock(winds, (b0 + i) * beat, Freq(g[i % g.Length]), 0.012f);
                }

                // ----- Grand: horns, drums, swells -----
                if (section == 2 || last)
                {
                    // The horn's long answer under the tune.
                    float hf = Freq(NoteNum(MuseumHorn[inSection % MuseumHorn.Length]));
                    AddBrass(grand, b0 * beat - BrassLead, bar * 0.95f, hf, last ? 0.04f : 0.03f);
                    if (last) AddBrass(grand, b0 * beat - BrassLead, bar * 0.95f, Freq(bassNote + 12), 0.03f);
                }
                if (climb)
                {
                    AddBrass(grand, b0 * beat - BrassLead, bar * 0.94f, Freq(bassNote + 12), 0.034f);
                    AddBrass(grand, b0 * beat - BrassLead, bar * 0.94f, Freq(ch[1]), 0.024f);
                }
                if (last || climb) AddTimpani(grand, b0 * beat, Freq(bassNote), last ? 0.11f : 0.08f);
                else if ((whirl || section == 2) && b % 2 == 0) AddTimpani(grand, b0 * beat, Freq(bassNote), 0.055f);
                if (whirl || last)
                {
                    // A soft brushed lilt on two and three.
                    AddBrush(grand, (b0 + 1f) * beat, beat * 0.5f, 0.012f, 5800 + b * 2);
                    AddBrush(grand, (b0 + 2f) * beat, beat * 0.5f, 0.012f, 5801 + b * 2);
                }
                if (sectionEnd && b != 7)
                {
                    // A drum roll and a breath of cymbal into the next section.
                    int roll = NoteNum(MuseumBass[(b + 1) % bars] + "2");
                    if (roll < 38) roll += 12;
                    for (int s16 = 0; s16 < 9; s16++) AddTimpani(grand, (b0 + 0.75f + s16 * 0.25f) * beat, Freq(roll), 0.02f + s16 * 0.011f);
                    AddAirPad(grand, (b0 + 1f) * beat, beat * 2.6f, 0.05f, 5900 + b);
                }
            }

            // ----- The tune -----
            for (int b = 0; b < bars; b++)
            {
                int section = MuseumSection(b);
                bool whirl = section == 3, last = section == 4;
                bool climb = whirl && b >= 56;
                foreach (var nt in Line(MuseumTune[b], b * 3f))
                {
                    float t = nt.Beat * beat;
                    float len = nt.Len * beat;
                    float f = Freq(nt.Midi);
                    if (section == 0)
                    {
                        // The intro's rising figure: celesta, with a harp note under it.
                        AddCelesta(bed, t, len + 0.4f, f, 0.05f);
                        AddPluck(bed, t, len + 0.3f, f * 0.5f, 0.03f, 6000 + b * 4 + (int)(nt.Beat * 2f));
                        continue;
                    }
                    if (whirl)
                    {
                        // The carousel belongs to the piano (in octaves) and the accordion.
                        AddPiano(bed, t, len, f, 0.13f);
                        AddPiano(bed, t, len, f * 0.5f, 0.06f, false);
                        AddAccordion(winds, t, len * 0.92f, f, climb ? 0.042f : 0.036f);
                        if (b >= 48) AddViolins(strings, t - 0.04f, len * 0.98f, f, climb ? 0.06f : 0.045f);
                        if (climb)
                        {
                            AddFlute(winds, t - FluteLead, len * 0.95f, f * 2f, 0.04f);
                            if (nt.Len >= 1f) AddBrass(grand, t - BrassLead, len * 0.9f, f * 0.5f, 0.026f);
                        }
                        continue;
                    }
                    // The star tune: celesta and a quiet piano always have it...
                    AddCelesta(bed, t, len + 0.4f, f, 0.045f);
                    AddPiano(bed, t, len, f, 0.07f);
                    // ...the violins sing it (an octave up the second time and at the end)...
                    AddViolins(strings, t - 0.04f, len * 0.98f, f, section == 1 ? 0.05f : 0.058f);
                    if (section == 2 || last) AddViolins(strings, t - 0.04f, len * 0.98f, f * 2f, 0.03f, 3);
                    // ...a flute doubles it, and bells pick out the long notes.
                    AddFlute(winds, t - FluteLead, len * 0.95f, f * (section == 1 ? 1f : 2f), 0.045f);
                    if (nt.Len >= 2f) AddGlock(winds, t, f * 2f, 0.014f);
                    if (last && nt.Len >= 2f) AddBrass(grand, t - BrassLead, len * 0.9f, f * 0.5f, 0.028f);
                }
            }

            return FinishStems(data, "pw_museum_", new[] { "bed", "strings", "winds", "grand" }, 7500f);
        }
    }
}

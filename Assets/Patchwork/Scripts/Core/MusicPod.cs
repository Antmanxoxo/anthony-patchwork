using UnityEngine;

namespace Patchwork
{
    // The Pod's song, heard while you hold the controller and choose a level. Dreamy and
    // weightless, like floating in a calm sea of stars: harp and celesta rippling up and down
    // wide, soft chords, a warm pad underneath, a glassy wavering voice taking the tune in the
    // middle, bubbles rising now and then, and no drums at all.
    //
    // D major, a slow lilting 3/4 at 104 BPM, 32 bars:
    //   A   (1-8)    the tune on celesta and music box, lots of long held notes
    //   A'  (9-16)   again, a soft flute joining in, climbing at the end into...
    //   B   (17-24)  two borrowed chords (Bb and C) for a "somewhere far away" feeling; the
    //                wavering voice sings, glockenspiel sparkling above
    //   A'' (25-32)  home again on warm strings, with one wistful minor chord, and round
    public partial class MusicPlayer
    {
        const float PodBpm = 104f;

        static readonly string[] PodChords =
        {
            "Dmaj7", "Dmaj7", "Gmaj7", "Gmaj7", "Bm7", "F#m7", "Gmaj7", "A7",
            "Dmaj7", "Dmaj7", "Gmaj7", "Gmaj7", "Bm7", "F#m7", "Em7", "A7",
            "Bbmaj7", "Bbmaj7", "Cmaj7", "Cmaj7", "Bbmaj7", "Bbmaj7", "Asus4", "A7",
            "Dmaj7", "Dmaj7", "Gmaj7", "Gm7", "F#m7", "Bm7", "Em7", "A7",
        };

        // One bar per entry, lengths in beats (3 a bar).
        static readonly string[] PodTune =
        {
            "F#5:2 A5:1", "C#6:3", "B5:2 D6:1", "F#6:2 D6:1",
            "D6:1.5 B5:0.5 A5:1", "A5:2 E5:1", "G5:1 B5:1 D6:1", "E6:3",

            "F#5:2 A5:1", "C#6:2 E6:1", "D6:2 B5:1", "F#6:3",
            "F#6:1 D6:1 B5:1", "C#6:2 A5:1", "G5:1 B5:1 E6:1", "C#6:1.5 E6:0.5 G6:1",

            "F6:2 D6:1", "A5:3", "G5:1 C6:1 E6:1", "G6:3",
            "F6:1.5 D6:0.5 Bb5:1", "D6:2 F6:1", "E6:2 D6:1", "C#6:3",

            "F#5:2 A5:1", "C#6:2 D6:1", "B5:2 D6:1", "Bb5:2 D6:1",
            "C#6:1 A5:1 F#5:1", "D5:1 F#5:1 B5:1", "G5:2 E5:1", "E5:1 A5:1 C#6:1",
        };

        static AudioClip podClip;
        AudioSource podSource;
        AudioLowPassFilter podFilter;
        float podMix;

        void StartPod()
        {
            if (podSource != null) return;
            if (podClip == null) podClip = ComposePod();
            var go = new GameObject("Music Pod");
            go.transform.SetParent(transform, false);
            podSource = go.AddComponent<AudioSource>();
            podSource.clip = podClip;
            podSource.loop = true;
            podSource.playOnAwake = false;
            podSource.spatialBlend = 0f;
            podSource.volume = 0f;
            podSource.ignoreListenerPause = true;
            podFilter = go.AddComponent<AudioLowPassFilter>();
            podSource.Play();
        }

        // Start the Pod song from the top if it's currently silent (picking up the controller).
        public static void CuePod()
        {
            if (instance == null || instance.podSource == null) return;
            if (instance.master < 0.01f) instance.podSource.time = 0f;
        }

        // Fades the Pod song in or out. Returns how loud the other music should be.
        float UpdatePodTrack(float udt)
        {
            float want = track == MusicTrack.Pod ? 1f : 0f;
            podMix = Mathf.MoveTowards(podMix, want, udt / 1.5f);
            if (podSource != null)
            {
                podSource.volume = master * Mathf.Sin(podMix * Mathf.PI * 0.5f);
                podFilter.cutoffFrequency = cutoff;
                if (podMix <= 0f && track != MusicTrack.Pod)
                {
                    Destroy(podSource.gameObject);
                    podSource = null;
                }
            }
            return Mathf.Cos(podMix * Mathf.PI * 0.5f);
        }

        // A "theremin": a pure, wavering sci-fi voice that slides gently into each note.
        static void AddTheremin(float[] data, float start, float dur, float f, float fromF, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt((dur + 0.25f) * Rate);
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                int k = s0 + i;
                if (k < 0 || k >= data.Length) continue;
                float t = i / (float)Rate;
                float glide = Mathf.Clamp01(t / 0.09f);
                float freq = Mathf.Lerp(fromF, f, glide * glide * (3f - 2f * glide));
                freq *= 1f + 0.018f * Mathf.Sin(2f * Mathf.PI * 5.6f * t) * Mathf.Clamp01(t / 0.25f);
                phase += 2f * Mathf.PI * freq / Rate;
                float env = Mathf.Clamp01(t / 0.06f) * Mathf.Clamp01((dur + 0.2f - t) / 0.2f);
                data[k] += (Mathf.Sin(phase) + 0.12f * Mathf.Sin(phase * 2f)) * env * vol;
            }
        }

        // See the notes at the top: everything flows in gentle eighth notes, nothing hits hard.
        static AudioClip ComposePod()
        {
            float beat = 60f / PodBpm;
            float bar = beat * 3f;
            int bars = PodChords.Length;
            int n = Mathf.CeilToInt(bar * bars * Rate);
            var d = new float[n];
            int[] stars = { 86, 88, 90, 93, 95, 98 };   // a high pentatonic for the twinkles

            for (int b = 0; b < bars; b++)
            {
                int section = b / 8;
                float b0 = b * 3f;
                string chord = PodChords[b];
                var low = Ch(chord, 50);
                int root = Root(chord, 38);

                // The warm pad: the whole chord held softly, plus a breath of air.
                foreach (int c in low) AddPad(d, b0 * beat, bar, Freq(c), section == 2 ? 0.02f : 0.016f);
                AddPad(d, b0 * beat, bar, Freq(root), 0.03f);
                if (b % 2 == 0) AddAirPad(d, b0 * beat, bar * 2f, 0.006f, 4800 + b);

                // Harp rippling up through the chord and back: six flowing notes a bar, the
                // shape rising higher in the far-away section.
                int[] shape = section == 2 ? new[] { 0, 2, 4, 5, 4, 2 } : (b % 2 == 0 ? new[] { 0, 1, 2, 4, 2, 1 } : new[] { 0, 2, 3, 4, 3, 2 });
                for (int e8 = 0; e8 < 6; e8++)
                {
                    int idx = shape[e8];
                    int note = low[idx % low.Length] + 12 * (idx / low.Length);
                    AddPluck(d, (b0 + e8 * 0.5f) * beat, beat * 2.2f, Freq(note), e8 == 0 ? 0.05f : 0.036f, 4100 + b * 6 + e8);
                }
                // Celesta answering an octave above, on the off-beats, very quietly.
                if (section != 0)
                    for (int e8 = 1; e8 < 6; e8 += 2)
                        AddCelesta(d, (b0 + e8 * 0.5f) * beat, beat, Freq(low[(e8 / 2 + b) % low.Length] + 24), 0.008f);

                // A long glissando, like a wave washing in, at the start of each section.
                if (b % 8 == 7)
                {
                    var into = Ch(PodChords[(b + 1) % bars], 62);
                    var run = new int[12];
                    for (int k = 0; k < run.Length; k++) run[k] = into[k % into.Length] + 12 * (k / into.Length) - 12;
                    AddGliss(d, (b0 + 1.5f) * beat, run, beat * 1.5f, 0.03f, 4300 + b * 12);
                }

                // Stars twinkling: one or two tiny high notes somewhere in most bars.
                int twinkles = (b * 7) % 3;
                for (int k = 0; k < twinkles; k++)
                {
                    float at = b0 + 0.25f + ((b * 5 + k * 3) % 5) * 0.5f;
                    AddGlock(d, at * beat, Freq(stars[(b * 3 + k * 2) % stars.Length]), 0.007f);
                }
                // Bubbles drifting up at the end of every other phrase.
                if (b % 8 == 3 || b % 8 == 6)
                {
                    AddBloop(d, (b0 + 2f) * beat, Freq(low[0] + 12), 0.03f);
                    AddBloop(d, (b0 + 2.33f) * beat, Freq(low[1 % low.Length] + 12), 0.026f);
                    AddBloop(d, (b0 + 2.66f) * beat, Freq(low[2 % low.Length] + 12), 0.022f);
                }
            }

            // The tune.
            float prev = Freq(78);
            for (int b = 0; b < bars; b++)
            {
                int section = b / 8;
                foreach (var nt in Line(PodTune[b], b * 3f))
                {
                    float t = nt.Beat * beat;
                    float len = nt.Len * beat;
                    float f = Freq(nt.Midi);
                    if (section == 0 || section == 1)
                    {
                        AddCelesta(d, t, len + 0.6f, f, 0.034f);
                        AddMusicBox(d, t, len + 0.5f, f * 0.5f, 0.016f);
                        if (section == 1) AddFlute(d, t, len * 0.97f + 0.05f, f, 0.03f);
                    }
                    else if (section == 2)
                    {
                        // The glassy voice, sliding from note to note, with a sparkle on each.
                        AddTheremin(d, t, len * 0.97f, f, prev, 0.042f);
                        AddGlock(d, t, f, 0.012f);
                    }
                    else
                    {
                        if (nt.Len >= 1f) AddStrings(d, t - 0.08f, len, f, 0.03f);
                        AddCelesta(d, t, len + 0.6f, f, 0.026f);
                        AddTheremin(d, t, len * 0.95f, f * 0.5f, prev * 0.5f, 0.014f);
                    }
                    prev = f;
                }
            }

            return FinishStems(new[] { d }, "pw_pod_", new[] { "song" }, 7000f)[0];
        }
    }
}

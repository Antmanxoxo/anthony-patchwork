using UnityEngine;

namespace Patchwork
{
    // The Pod's song, heard while you hold the controller and choose a level. Whimsical and
    // spacey: toy piano, music box and marimba over a bouncy pizzicato bass, a wavering theremin
    // in the middle, glockenspiel and chiptune star-blips, bloops between phrases.
    //
    // D major, 4/4 at 84 BPM with a swing, 24 bars:
    //   A (1-8)   the tune on music box and toy piano over marimba, pizzicato and brushes
    //   B (9-16)  the theremin takes over; a borrowed G minor chord for a wistful turn
    //   C (17-24) glockenspiel with a low theremin shadow, soft strings underneath
    public partial class MusicPlayer
    {
        const float PodBpm = 84f;
        const float PodSwing = 0.1f;

        static readonly string[] PodChords =
        {
            "Dmaj7", "Bm7", "Gmaj7", "A7", "Dmaj7", "F#m7", "Gmaj7", "A7",
            "Em7", "A7", "F#m7", "Bm7", "Gmaj7", "Gm7", "Dmaj7", "A7",
            "Bm7", "Gmaj7", "Dmaj7", "A7", "Bm7", "Em7", "Gmaj7", "A7",
        };

        static readonly string[] PodTune =
        {
            "F#5:1.5 E5:0.5 D5:1 A4:1", "B4:0.5 C#5:0.5 D5:0.5 F#5:0.5 E5:2",
            "D5:1.5 B4:0.5 G4:1 B4:1", "C#5:0.5 E5:0.5 A5:1 G5:0.5 E5:0.5 C#5:1",
            "F#5:1.5 E5:0.5 D5:1 A5:1", "C#6:0.5 A5:0.5 F#5:1 E5:1 C#5:1",
            "B4:0.5 D5:0.5 G5:1 F#5:0.5 E5:0.5 D5:1", "E5:3 r:1",

            "G5:1 E5:0.5 B4:0.5 D5:1 E5:1", "C#5:1.5 E5:0.5 A5:2",
            "A5:1 F#5:0.5 C#5:0.5 E5:1 F#5:1", "D5:1 B4:0.5 F#4:0.5 B4:2",
            "B5:1.5 A5:0.5 G5:0.5 F#5:0.5 D5:1", "Bb5:1.5 G5:0.5 D5:1 Bb4:1",
            "A5:2 F#5:1 D5:1", "E5:1 G5:0.5 E5:0.5 C#5:1 A4:1",

            "D5:0.5 F#5:0.5 B5:2 A5:1", "G5:0.5 F#5:0.5 D5:1 B4:2",
            "A4:0.5 D5:0.5 F#5:1 A5:1 F#5:1", "E5:2 G5:1 E5:1",
            "D5:1 F#5:0.5 A5:0.5 B5:2", "G5:1 E5:0.5 B4:0.5 E5:2",
            "F#5:1 G5:1 B5:1 A5:1", "A5:2 r:2",
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

        // Spacey and whimsical: plinky toy piano and music box, marimba ripples, pizzicato bass,
        // a wavering theremin taking the tune in the middle, glockenspiel and chiptune blips
        // twinkling like stars, bubbly bloops, and sparkly harp-like runs at each new section.
        static AudioClip ComposePod()
        {
            float beat = 60f / PodBpm;
            float bar = beat * 4f;
            int bars = PodChords.Length;
            int n = Mathf.CeilToInt(bar * bars * Rate);
            var d = new float[n];
            System.Func<float, float> T = bt => Swing(bt, PodSwing) * beat;
            int[] blips = { 86, 88, 90, 93, 95, 98 };   // a high pentatonic for the star blips

            for (int b = 0; b < bars; b++)
            {
                int section = b / 8;
                float b0 = b * 4f;
                string chord = PodChords[b];
                int bassNote = Root(chord, 40);
                var tones = Ch(chord, 60);

                // A sparkly run up at the start of each section.
                if (b % 8 == 0)
                {
                    var run = new int[8];
                    for (int k = 0; k < 8; k++) run[k] = tones[k % tones.Length] + 12 * (k / tones.Length);
                    AddGliss(d, b0 * beat, run, beat, 0.03f, 4100 + b);
                }

                // Marimba ripples on the off-beats.
                int[] order = { 0, 2, 1, 3, 2, 1, 3, 2 };
                for (int e8 = 1; e8 < 8; e8 += 2)
                {
                    int note = tones[order[e8] % tones.Length] + (order[e8] >= tones.Length ? 12 : 0);
                    AddMarimba(d, T(b0 + e8 * 0.5f), Freq(note), 0.045f);
                }
                // Toy piano chords on 1 and 3.
                foreach (int c in Ch(chord, 64))
                {
                    AddToyPiano(d, b0 * beat, Freq(c), 0.012f);
                    AddToyPiano(d, (b0 + 2f) * beat, Freq(c), 0.009f);
                }

                // Pizzicato bass, bouncy.
                AddPizz(d, b0 * beat, Freq(bassNote), 0.1f, 4200 + b);
                AddPizz(d, T(b0 + 1.5f), Freq(bassNote + 7), 0.07f, 4300 + b);
                AddPizz(d, (b0 + 3f) * beat, Freq(bassNote + (b % 2 == 0 ? 12 : 5)), 0.06f, 4400 + b);

                // Soft drums: kick, rim clicks, brushes.
                AddKick(d, b0 * beat, 0.07f);
                AddSnare(d, (b0 + 1f) * beat, 0.016f, 4500 + b);
                AddSnare(d, (b0 + 3f) * beat, 0.02f, 4600 + b);
                for (int e8 = 0; e8 < 8; e8++) AddBrush(d, T(b0 + e8 * 0.5f), 0.05f, e8 % 2 == 0 ? 0.006f : 0.01f, 4700 + b * 8 + e8);

                // A starry wash, and soft strings later on.
                if (b % 2 == 0) AddAirPad(d, b0 * beat, bar * 2f, 0.005f, 4800 + b);
                if (section >= 1)
                    foreach (int c in Ch(chord, 55)) AddStrings(d, b0 * beat, bar, Freq(c), section == 2 ? 0.008f : 0.005f);

                // Star blips: little chiptune twinkles dotted about.
                int blipCount = 1 + (b * 7 % 3);
                for (int k = 0; k < blipCount; k++)
                {
                    float at = b0 + 0.75f + ((b * 5 + k * 3) % 7) * 0.5f;
                    AddChip(d, at * beat, 0.07f, Freq(blips[(b * 3 + k * 2) % blips.Length]), 0.008f);
                }
                // Bubbly bloops at the end of each phrase.
                if (b % 4 == 3)
                {
                    AddBloop(d, (b0 + 3f) * beat, Freq(tones[0] + 12), 0.045f);
                    AddBloop(d, (b0 + 3.33f) * beat, Freq(tones[1 % tones.Length] + 12), 0.04f);
                    AddBloop(d, (b0 + 3.66f) * beat, Freq(tones[2 % tones.Length] + 12), 0.035f);
                }
            }

            // The tune.
            float prev = Freq(66);
            for (int b = 0; b < bars; b++)
            {
                int section = b / 8;
                foreach (var nt in Line(PodTune[b], b * 4f))
                {
                    float t = T(nt.Beat);
                    float len = nt.Len * beat;
                    float f = Freq(nt.Midi);
                    if (section == 0)
                    {
                        AddMusicBox(d, t, len + 0.4f, f, 0.04f);
                        AddToyPiano(d, t, f * 2f, 0.01f);
                    }
                    else if (section == 1)
                    {
                        // The theremin sings the middle section, sliding between notes.
                        AddTheremin(d, t, len * 0.95f, f, prev, 0.05f);
                        AddCelesta(d, t, len, f * 2f, 0.006f);
                    }
                    else
                    {
                        AddGlock(d, t, f, 0.03f);
                        AddTheremin(d, t, len * 0.9f, f * 0.5f, prev * 0.5f, 0.022f);
                    }
                    prev = f;
                }
            }

            return FinishStems(new[] { d }, "pw_pod_", new[] { "song" }, 8000f)[0];
        }
    }
}

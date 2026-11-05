using UnityEngine;

namespace Patchwork
{
    // The Pod's song, heard while you hold the controller and choose a level. Mellow and a little
    // quirky: a fingerpicked nylon guitar, a round bass, brushes and a rim click, a music box
    // singing the tune, a breathy flute answering in the middle, glockenspiel sparkle at the end,
    // and little bloops between phrases.
    //
    // D major, 4/4 at 88 BPM with a light swing, 24 bars:
    //   A (1-8)   the tune on music box over guitar, bass and brushes
    //   B (9-16)  the flute takes over; a borrowed G minor chord for a wistful turn
    //   C (17-24) glockenspiel and whistle, soft strings underneath, then back round
    public partial class MusicPlayer
    {
        const float PodBpm = 88f;
        const float PodSwing = 0.08f;

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

        static AudioClip ComposePod()
        {
            float beat = 60f / PodBpm;
            float bar = beat * 4f;
            int bars = PodChords.Length;
            int n = Mathf.CeilToInt(bar * bars * Rate);
            var d = new float[n];
            System.Func<float, float> T = bt => Swing(bt, PodSwing) * beat;

            for (int b = 0; b < bars; b++)
            {
                int section = b / 8;
                float b0 = b * 4f;
                string chord = PodChords[b];

                // Fingerpicked guitar: bass note on 1, then the chord tones rolling up and down.
                int bassNote = Root(chord, 40);
                var tones = Ch(chord, 52);
                AddPluck(d, b0 * beat, beat * 2f, Freq(bassNote + 12), 0.07f, 3000 + b * 9);
                int[] order = { 0, 2, 1, 3, 2, 1, 3, 2 };
                for (int e8 = 1; e8 < 8; e8++)
                {
                    int note = tones[order[e8] % tones.Length] + (order[e8] >= tones.Length ? 12 : 0);
                    AddPluck(d, T(b0 + e8 * 0.5f), beat * 1.4f, Freq(note), e8 % 2 == 0 ? 0.05f : 0.04f, 3001 + b * 9 + e8);
                }

                // Round bass: root on 1, the fifth on the "and" of 2, a passing note at the end.
                AddBass(d, b0 * beat, beat * 1.4f, Freq(bassNote), 0.11f);
                AddBass(d, T(b0 + 1.5f), beat * 0.9f, Freq(bassNote + 7), 0.08f);
                AddBass(d, (b0 + 3f) * beat, beat * 0.9f, Freq(bassNote + (b % 2 == 0 ? 12 : 5)), 0.07f);

                // Drums: soft kick, rim click on 2 and 4, swung brushes.
                AddKick(d, b0 * beat, 0.09f);
                AddKick(d, T(b0 + 2.5f), 0.05f);
                AddSnare(d, (b0 + 1f) * beat, 0.022f, 3500 + b);
                AddSnare(d, (b0 + 3f) * beat, 0.026f, 3600 + b);
                for (int e8 = 0; e8 < 8; e8++) AddBrush(d, T(b0 + e8 * 0.5f), 0.05f, e8 % 2 == 0 ? 0.008f : 0.013f, 3700 + b * 8 + e8);

                // Pads: a quiet airy wash, and soft strings in the later sections.
                if (b % 2 == 0) AddAirPad(d, b0 * beat, bar * 2f, 0.004f, 3900 + b);
                if (section >= 1)
                    foreach (int c in Ch(chord, 55)) AddStrings(d, b0 * beat, bar, Freq(c), section == 2 ? 0.009f : 0.006f);

                // Little bloops at the ends of phrases, like bubbles popping.
                if (b % 4 == 3)
                {
                    AddBloop(d, (b0 + 3f) * beat, Freq(tones[0] + 24), 0.04f);
                    AddBloop(d, (b0 + 3.5f) * beat, Freq(tones[1 % tones.Length] + 24), 0.035f);
                }
            }

            // The tune.
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
                        AddCelesta(d, t, len, f * 2f, 0.008f);
                    }
                    else if (section == 1)
                    {
                        AddFlute(d, t, len * 0.95f + 0.05f, f, 0.06f);
                        AddMusicBox(d, t, len + 0.3f, f, 0.012f);
                    }
                    else
                    {
                        AddGlock(d, t, f, 0.032f);
                        AddWhistle(d, t, len * 0.9f + 0.05f, f, 0.022f);
                    }
                }
            }

            return FinishStems(new[] { d }, "pw_pod_", new[] { "song" }, 7500f)[0];
        }
    }
}

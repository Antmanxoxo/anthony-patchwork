using UnityEngine;

namespace Patchwork
{
    // The Whispering Wood's music: only inside the forest, crossfading over whatever else is
    // playing. Spooky but still a toy: tiptoeing pizzicato, a wobbly detuned music box, a
    // ghostly theremin, a low string drone and soft timpani, like a haunted nursery rhyme.
    //
    // D minor, 4/4 at 76 BPM, 16 bars. An Eb chord (the "Neapolitan") gives it the creeps.
    public partial class MusicPlayer
    {
        const float SpookyBpm = 76f;

        static readonly string[] SpookyChords =
        {
            "Dm", "Dm", "Bb", "A7", "Dm", "Gm", "Eb", "A7",
            "Gm", "Dm", "Bb", "A7", "Dm", "Gm", "A7", "Dm",
        };

        static readonly string[] SpookyTune =
        {
            "D5:1 F5:0.5 E5:0.5 D5:1 A4:1", "C#5:1.5 D5:0.5 E5:2",
            "F5:1 D5:0.5 Bb4:0.5 F5:1 Eb5:1", "E5:0.5 F5:0.5 E5:0.5 C#5:0.5 A4:2",
            "D5:1 F5:0.5 A5:0.5 G#5:1 A5:1", "Bb5:1 A5:0.5 G5:0.5 D5:2",
            "Eb5:1 G5:0.5 Bb5:0.5 A5:1 G5:1", "E5:1 C#5:1 A4:2",
            "G4:1 Bb4:0.5 D5:0.5 G5:1.5 F5:0.5", "F5:1 E5:0.5 D5:0.5 A4:2",
            "Bb4:1 D5:0.5 F5:0.5 Bb5:1 A5:1", "G#5:1 A5:1 E5:2",
            "D5:0.5 E5:0.5 F5:0.5 G5:0.5 A5:1 D6:1", "C6:1 Bb5:0.5 G5:0.5 D5:2",
            "C#5:1 E5:1 G5:1 Bb5:1", "A5:2 r:2",
        };

        static AudioClip spookyClip;
        static Bounds spookyArea;
        static string spookyLevel;
        AudioSource spookySource;
        AudioLowPassFilter spookyFilter;
        float spookyMix;

        // How far into the spooky music we are (0 = not at all, 1 = fully). The forest's
        // darker look follows this too.
        public static float SpookyMix { get { return instance != null ? instance.spookyMix : 0f; } }

        // Called by a level that has a spooky area: composes the song (during loading) and
        // remembers where it plays.
        public static void SetSpookyArea(string levelId, Bounds area)
        {
            spookyLevel = levelId;
            spookyArea = area;
            if (spookyClip == null) spookyClip = ComposeSpooky();
        }

        static bool InSpookyArea()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Level == null || game.Player == null || spookyLevel == null) return false;
            if (game.Level.Id != spookyLevel) return false;
            if (game.Mode == UIMode.Loading || game.Mode == UIMode.Complete) return false;
            return spookyArea.Contains(game.Player.Motor.Center);
        }

        // Fades the forest song in or out. Returns how loud everything else should be.
        float UpdateSpookyTrack(float udt)
        {
            bool want = InSpookyArea();
            spookyMix = Mathf.MoveTowards(spookyMix, want ? 1f : 0f, udt / (want ? 2.2f : 1.6f));
            if (spookyMix > 0f && spookySource == null && spookyClip != null)
            {
                var go = new GameObject("Music Spooky");
                go.transform.SetParent(transform, false);
                spookySource = go.AddComponent<AudioSource>();
                spookySource.clip = spookyClip;
                spookySource.loop = true;
                spookySource.playOnAwake = false;
                spookySource.spatialBlend = 0f;
                spookySource.volume = 0f;
                spookySource.ignoreListenerPause = true;
                spookyFilter = go.AddComponent<AudioLowPassFilter>();
                spookySource.Play();
            }
            if (spookySource != null)
            {
                spookySource.volume = master * Mathf.Sin(spookyMix * Mathf.PI * 0.5f);
                spookyFilter.cutoffFrequency = cutoff;
                if (spookyMix <= 0f && !want)
                {
                    Destroy(spookySource.gameObject);
                    spookySource = null;
                }
            }
            return Mathf.Cos(spookyMix * Mathf.PI * 0.5f);
        }

        static AudioClip ComposeSpooky()
        {
            float beat = 60f / SpookyBpm;
            float bar = beat * 4f;
            int bars = SpookyChords.Length;
            int n = Mathf.CeilToInt(bar * bars * Rate);
            var d = new float[n];

            for (int b = 0; b < bars; b++)
            {
                float b0 = b * 4f;
                string chord = SpookyChords[b];
                int root = Root(chord, 38);
                var tones = Ch(chord, 69);

                // Tiptoe pizzicato: root, fifth, octave, fifth, short and sneaky.
                int[] walk = { root, root + 7, root + 12, root + 7 };
                for (int q = 0; q < 4; q++) AddPizz(d, (b0 + q) * beat, Freq(walk[q]), q == 0 ? 0.09f : 0.06f, 5100 + b * 4 + q);
                // A low string drone on D (and A), the whole way through.
                AddStrings(d, b0 * beat, bar, Freq(38), 0.012f);
                AddStrings(d, b0 * beat, bar, Freq(45), 0.006f);
                // Ghostly celesta ripples up the chord, high and quiet.
                for (int k = 0; k < tones.Length; k++)
                    AddCelesta(d, (b0 + 2f + k * 0.33f) * beat, beat, Freq(tones[k] + 12), 0.006f);
                // A creaky wind every other bar.
                if (b % 2 == 0) AddAirPad(d, b0 * beat, bar * 2f, 0.007f, 5200 + b);
                // Soft timpani at the end of each phrase, and a low "boo" bloop.
                if (b % 4 == 3)
                {
                    AddTimpani(d, (b0 + 3f) * beat, Freq(38), 0.05f);
                    AddTimpani(d, (b0 + 3.5f) * beat, Freq(45), 0.035f);
                    AddBloop(d, (b0 + 2.5f) * beat, Freq(50), 0.03f);
                }
                // A tick-tock like a grandfather clock in an empty house.
                AddSnare(d, (b0 + 1f) * beat, 0.008f, 5300 + b);
                AddSnare(d, (b0 + 3f) * beat, 0.006f, 5400 + b);
            }

            // The tune: a wobbly music box (two voices a hair apart), and the theremin wailing it
            // an octave down in the second half.
            float prev = Freq(62);
            for (int b = 0; b < bars; b++)
            {
                foreach (var nt in Line(SpookyTune[b], b * 4f))
                {
                    float t = nt.Beat * beat;
                    float len = nt.Len * beat;
                    float f = Freq(nt.Midi);
                    AddMusicBox(d, t, len + 0.5f, f, 0.032f);
                    AddMusicBox(d, t + 0.012f, len + 0.5f, f * 1.011f, 0.018f);
                    if (b >= 8) AddTheremin(d, t, len * 0.95f, f * 0.5f, prev * 0.5f, 0.03f);
                    prev = f;
                }
            }

            return FinishStems(new[] { d }, "pw_spooky_", new[] { "song" }, 6200f)[0];
        }
    }
}

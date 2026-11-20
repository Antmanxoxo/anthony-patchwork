using UnityEngine;

namespace Patchwork
{
    // The Store's music: a cousin of the lift music (the same easy bossa nova feel), but a
    // little brighter and bouncier for shopping: a marimba tune doubled by a toy piano, soft
    // electric piano and a plucked guitar on the off-beats, a walking bass, a shaker and rim
    // clicks, and a glockenspiel sparkle at the top of each phrase. Sixteen bars that loop
    // round without a seam, with a "ding-ding" like a shop bell at the end of each eight.
    // (An original tune.)
    public partial class MusicPlayer
    {
        static AudioClip storeMusic;

        const float StoreBpm = 118f;

        static readonly string[] StoreChords =
        {
            "Bbmaj7", "Gm7", "Cm7", "F7", "Dm7", "G7", "Cm7", "F7",
            "Ebmaj7", "Edim", "Bbmaj7", "G7", "Cm7", "F7", "Bbmaj7", "F7",
        };

        // Two bars a line (4/4).
        static readonly string[] StoreTune =
        {
            "F5:0.5 D5:0.5 F5:0.5 Bb5:1 A5:0.5 G5:1   F5:1 D5:0.5 r:0.5 Bb4:1 r:1",
            "Eb5:0.5 G5:0.5 C6:0.5 Bb5:1 G5:0.5 Eb5:1   F5:1.5 A5:0.5 C6:1 r:1",
            "F5:0.5 A5:0.5 D6:0.5 C6:1 A5:0.5 F5:1   G5:1 B5:0.5 D6:0.5 F6:1 r:1",
            "Eb6:1 D6:0.5 C6:0.5 Bb5:1 A5:1   C6:1 Bb5:0.5 A5:0.5 F5:1 r:1",
            "G5:1.5 Bb5:0.5 D6:1 Eb6:1   Db6:1 Bb5:0.5 G5:0.5 E5:1 r:1",
            "F5:0.5 D5:0.5 F5:0.5 Bb5:1 D6:0.5 C6:1   B5:1 G5:0.5 F5:0.5 D5:1 r:1",
            "Eb5:0.5 G5:0.5 C6:0.5 Eb6:1 D6:0.5 C6:1   A5:1 C6:0.5 Eb6:0.5 D6:1 C6:1",
            "Bb5:1.5 F5:0.5 D5:1 Bb4:1   r:2 C5:0.5 D5:0.5 Eb5:0.5 E5:0.5",
        };

        public static AudioClip StoreMusic()
        {
            if (storeMusic != null) return storeMusic;
            float beat = 60f / StoreBpm;
            int bars = StoreChords.Length;
            int n = Mathf.CeilToInt(beat * 4f * bars * Rate);
            var d = new float[n];
            for (int b = 0; b < bars; b++)
            {
                float b0 = b * 4f;
                string name = StoreChords[b];
                var ch = Ch(name, 53);
                int root = Root(name, 34);
                // A walking bass: root, third or fifth, fifth, and a step towards the next chord.
                int next = Root(StoreChords[(b + 1) % bars], 34);
                int[] walk = { root, root + (ch.Length > 1 ? ch[1] - ch[0] : 4), root + 7, next + (next > root ? -1 : 1) };
                for (int q = 0; q < 4; q++)
                    AddBass(d, (b0 + q) * beat, beat * 0.85f, Freq(walk[q]), q == 0 ? 0.12f : 0.085f);
                // Electric piano on the bossa beats.
                foreach (float at in new[] { 0f, 1.5f, 2.5f })
                    for (int c = 0; c < ch.Length; c++)
                        AddPiano(d, (b0 + at) * beat + c * 0.004f, beat * 0.5f, Freq(ch[c]), 0.024f, false);
                // Guitar on the off-beats, a shaker in quavers, rim clicks, and a soft kick.
                for (int e8 = 1; e8 < 8; e8 += 2)
                    AddPluck(d, (b0 + e8 * 0.5f) * beat, beat * 0.5f, Freq(ch[(e8 / 2) % ch.Length] + 12), 0.026f, 8000 + b * 8 + e8);
                for (int e8 = 0; e8 < 8; e8++)
                    AddBrush(d, (b0 + e8 * 0.5f) * beat, beat * 0.22f, e8 % 2 == 0 ? 0.007f : 0.013f, 8100 + b * 8 + e8);
                foreach (float at in new[] { 0f, 1.5f, 3f })
                    AddSnare(d, (b0 + at) * beat, 0.009f, 8200 + b * 3 + (int)at);
                AddKick(d, b0 * beat, 0.05f);
                AddKick(d, (b0 + 2f) * beat, 0.035f);
                foreach (int c in ch) AddPad(d, b0 * beat, beat * 4f, Freq(c), 0.005f);
                // A glockenspiel sparkle at the top of every two bars.
                if (b % 2 == 0) AddGlock(d, b0 * beat, Freq(ch[ch.Length - 1] + 24), 0.025f);
            }
            for (int line = 0; line < StoreTune.Length; line++)
                foreach (var nt in Line(StoreTune[line], line * 8f))
                {
                    float t = nt.Beat * beat, f = Freq(nt.Midi);
                    AddMarimba(d, t, f, 0.085f);
                    AddToyPiano(d, t + 0.006f, f * 2f, 0.022f);
                    if (nt.Len >= 1f) AddCelesta(d, t, nt.Len * beat, f, 0.025f);
                }
            // The shop bell at the end of each eight bars: ding-ding.
            foreach (float at in new[] { 8f * 4f - 1f, 16f * 4f - 1f })
            {
                AddGlock(d, at * beat, Freq(88), 0.05f);
                AddGlock(d, (at + 0.5f) * beat, Freq(84), 0.05f);
            }
            storeMusic = FinishStems(new[] { d }, "pw_store_", new[] { "music" }, 7000f)[0];
            return storeMusic;
        }
    }

    // Plays the Store's music while it's open: fades in, ducks the level's own music (as the
    // lift does), and fades away when you leave. It keeps going with the game paused.
    public class StoreMusicPlayer : MonoBehaviour
    {
        static StoreMusicPlayer instance;
        AudioSource src;
        bool on;

        public static void Play()
        {
            if (instance == null)
            {
                var go = new GameObject("Store Music");
                instance = go.AddComponent<StoreMusicPlayer>();
                instance.src = go.AddComponent<AudioSource>();
                instance.src.clip = MusicPlayer.StoreMusic();
                instance.src.loop = true;
                instance.src.playOnAwake = false;
                instance.src.spatialBlend = 0f;
                instance.src.ignoreListenerPause = true;
                instance.src.volume = 0f;
            }
            instance.on = true;
            if (!instance.src.isPlaying) { instance.src.time = 0f; instance.src.Play(); }
            MusicPlayer.LiftHush = true;
        }

        public static void Stop()
        {
            if (instance == null) return;
            instance.on = false;
            MusicPlayer.LiftHush = false;
        }

        void Update()
        {
            float want = on ? 0.6f * MusicPlayer.Volume / 10f : 0f;
            src.volume = Mathf.MoveTowards(src.volume, want, Time.unscaledDeltaTime * (on ? 0.8f : 1.5f));
            if (!on && src.volume <= 0.001f) Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}

using UnityEngine;

namespace Patchwork
{
    // Lift music for the museum's cardboard lift: a sleepy little bossa nova on vibraphone,
    // soft electric piano, nylon guitar and a shaker, the sort of tune that has played in
    // every lift since lifts were invented. (An original tune.) And the "ding".
    public partial class MusicPlayer
    {
        static AudioClip liftMusic, liftDing;

        const float LiftBpm = 112f;

        static readonly string[] LiftChords = { "Fmaj7", "Dm7", "Gm7", "C7", "Am7", "D7", "Gm7", "C7" };

        // Two bars a line (4/4).
        static readonly string[] LiftTune =
        {
            "C6:1.5 A5:0.5 r:0.5 G5:0.5 A5:1   F5:3 r:1",
            "Bb5:1.5 A5:0.5 r:0.5 G5:0.5 F5:1  E5:2 G5:1 C6:1",
            "C6:1.5 A5:0.5 r:0.5 G5:0.5 A5:1   E6:2 D6:1 C6:1",
            "D6:1 C6:0.5 Bb5:0.5 A5:1 G5:1     F5:3 r:1",
        };

        public static AudioClip LiftMusic()
        {
            if (liftMusic != null) return liftMusic;
            float beat = 60f / LiftBpm;
            int bars = LiftChords.Length;
            int n = Mathf.CeilToInt(beat * 4f * bars * Rate);
            var d = new float[n];
            for (int b = 0; b < bars; b++)
            {
                float b0 = b * 4f;
                string name = LiftChords[b];
                var ch = Ch(name, 53);
                int root = Root(name, 36);
                // Bass: root on one, fifth on three, with a little pickup.
                AddBass(d, b0 * beat, beat * 1.4f, Freq(root), 0.12f);
                AddBass(d, (b0 + 2f) * beat, beat * 1.2f, Freq(root + 7), 0.1f);
                AddBass(d, (b0 + 3.5f) * beat, beat * 0.4f, Freq(root + (b % 2 == 0 ? 5 : 10)), 0.06f);
                // Electric piano comping the bossa rhythm: 1, the "and" of 2, the "and" of 3.
                foreach (float at in new[] { 0f, 1.5f, 2.5f })
                    for (int c = 0; c < ch.Length; c++)
                        AddPiano(d, (b0 + at) * beat + c * 0.004f, beat * 0.55f, Freq(ch[c]), 0.028f, false);
                // Guitar picking the chord's top notes on the off-beats.
                for (int e8 = 1; e8 < 8; e8 += 2)
                    AddPluck(d, (b0 + e8 * 0.5f) * beat, beat * 0.6f, Freq(ch[(e8 / 2 + 1) % ch.Length] + 12), 0.03f, 7000 + b * 8 + e8);
                // A shaker in quavers, and a rim click on the bossa beats.
                for (int e8 = 0; e8 < 8; e8++)
                    AddBrush(d, (b0 + e8 * 0.5f) * beat, beat * 0.25f, e8 % 2 == 0 ? 0.008f : 0.014f, 7100 + b * 8 + e8);
                foreach (float at in new[] { 0f, 1.5f, 3f })
                    AddSnare(d, (b0 + at) * beat, 0.01f, 7200 + b * 3 + (int)at);
                foreach (int c in ch) AddPad(d, b0 * beat, beat * 4f, Freq(c), 0.007f);
            }
            for (int line = 0; line < LiftTune.Length; line++)
                foreach (var nt in Line(LiftTune[line], line * 8f))
                {
                    // Vibraphone: a soft bell with a slow shimmer.
                    float t = nt.Beat * beat, len = nt.Len * beat, f = Freq(nt.Midi);
                    AddCelesta(d, t, len + 0.3f, f, 0.07f);
                    AddCelesta(d, t + 0.012f, len + 0.3f, f * 1.003f, 0.03f);
                    AddFlute(d, t - FluteLead, len * 0.9f, f, 0.018f);
                }
            liftMusic = FinishStems(new[] { d }, "pw_lift_", new[] { "music" }, 6000f)[0];
            return liftMusic;
        }

        // The ding: two soft bells, high then low.
        public static AudioClip LiftDing()
        {
            if (liftDing != null) return liftDing;
            int n = Mathf.CeilToInt(1.8f * Rate);
            var d = new float[n];
            AddCelesta(d, 0f, 0.6f, Freq(84), 0.4f);
            AddGlock(d, 0f, Freq(84), 0.12f);
            AddCelesta(d, 0.32f, 1f, Freq(79), 0.4f);
            AddGlock(d, 0.32f, Freq(79), 0.12f);
            liftDing = FinishStems(new[] { d }, "pw_lift_", new[] { "ding" }, 9000f)[0];
            return liftDing;
        }
    }
}

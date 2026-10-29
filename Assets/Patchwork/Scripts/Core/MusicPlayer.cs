using UnityEngine;

namespace Patchwork
{
    // Background music. Built once when the game starts and loops between scenes.
    // On the Skiff it's a sea shanty, with the tune played by a different instrument for each
    // form, crossfading when Patch changes: Yarn = fiddle, Water = marimba, Wind = tin whistle.
    // Levels can have their own soundtrack (see MusicToybox).
    public partial class MusicPlayer : MonoBehaviour
    {
        const string OldOnKey = "pw_music_on";
        const string VolumeKey = "pw_music_volume";
        const int Rate = 32000;
        const float MaxVolume = 0.32f;
        const float LayerFade = 1.5f;   // seconds to crossfade between form layers

        enum Stem { Base, Water, Yarn, Wind }
        const int StemCount = 4;

        static MusicPlayer instance;
        static AudioClip[] stems;

        readonly AudioSource[] sources = new AudioSource[StemCount];
        readonly AudioLowPassFilter[] filters = new AudioLowPassFilter[StemCount];
        readonly float[] layerMix = { 1f, 0f, 1f, 0f };   // 0..1 crossfade position per stem
        FormType form = FormType.Yarn;
        float master;
        float duck = 1f, duckTarget = 1f;
        float cutoff = 22000f, cutoffTarget = 22000f;

        // Music volume, 0..10 (saved). 0 means off.
        public static int Volume
        {
            get
            {
                // Older saves only had an on/off switch.
                int fallback = PlayerPrefs.GetInt(OldOnKey, 1) == 1 ? 10 : 0;
                return Mathf.Clamp(PlayerPrefs.GetInt(VolumeKey, fallback), 0, 10);
            }
            set
            {
                PlayerPrefs.SetInt(VolumeKey, Mathf.Clamp(value, 0, 10));
                PlayerPrefs.Save();
            }
        }

        // Paused / Satchel: quieter and "behind glass". Form Wheel: muffled. Normal: clear.
        public static void SetMuffle(float cutoffHz, float volumeScale)
        {
            if (instance == null) return;
            instance.cutoffTarget = cutoffHz;
            instance.duckTarget = volumeScale;
        }

        // Which form's tune layer should be playing. Safe to call every frame.
        public static void SetForm(FormType f)
        {
            if (instance != null) instance.form = f;
        }

        // Starts the music if it isn't already playing. Survives scene loads.
        public static void EnsurePlaying()
        {
            if (instance != null) return;
            var go = new GameObject("Patchwork Music");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<MusicPlayer>();
        }

        void Awake()
        {
            if (stems == null || stems[0] == null) stems = Compose();
            // Start straight on the saved form's layer (no fade-in from another one).
            form = GameState.CurrentForm;
            for (int i = 1; i < StemCount; i++) layerMix[i] = StemFor(form) == i ? 1f : 0f;
            // All layers start on the same audio tick so they stay perfectly in time.
            double start = AudioSettings.dspTime + 0.2;
            for (int i = 0; i < StemCount; i++)
            {
                var go = new GameObject("Music " + (Stem)i);
                go.transform.SetParent(transform, false);
                var src = go.AddComponent<AudioSource>();
                src.clip = stems[i];
                src.loop = true;
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                src.volume = 0f;
                src.ignoreListenerPause = true;
                filters[i] = go.AddComponent<AudioLowPassFilter>();
                filters[i].cutoffFrequency = 22000f;
                sources[i] = src;
                src.PlayScheduled(start);
            }
        }

        void Update()
        {
            float udt = Time.unscaledDeltaTime;
            // Ease the muffle over about 0.3 s, in real time so it works while the game is paused.
            float k = 1f - Mathf.Exp(-10f * udt);
            cutoff = Mathf.Lerp(cutoff, cutoffTarget, k);
            duck = Mathf.Lerp(duck, duckTarget, k);

            float target = MaxVolume * Volume / 10f * duck;
            master = Mathf.MoveTowards(master, target, udt * 0.6f);
            // Level soundtracks (like Toybox Town) crossfade with the Skiff's music.
            float skiff = UpdateLevelTrack(udt);

            for (int i = 0; i < StemCount; i++)
            {
                if (sources[i] == null) continue;
                if (i != (int)Stem.Base)
                {
                    bool on = StemFor(form) == i;
                    layerMix[i] = Mathf.MoveTowards(layerMix[i], on ? 1f : 0f, udt / LayerFade);
                }
                // Equal-power curve so the overall loudness stays even mid-crossfade.
                float gain = i == (int)Stem.Base ? 1f : Mathf.Sin(layerMix[i] * Mathf.PI * 0.5f);
                sources[i].volume = master * gain * skiff;
                filters[i].cutoffFrequency = cutoff;
            }
        }

        static int StemFor(FormType f)
        {
            switch (f)
            {
                case FormType.Water: return (int)Stem.Water;
                case FormType.Wind: return (int)Stem.Wind;
                default: return (int)Stem.Yarn;
            }
        }

        // ------------------------------------------------------------------ Composition

        // ------------------------------------------------------------------ The Skiff's shanty
        //
        // A sea shanty in D minor, 6/8 (two big swaying beats a bar). 24 bars: a verse, the verse
        // again with a new ending, then a brighter "sailing" chorus in F major.
        //   Base: accordion "oom-cha-cha", plucked double bass, bodhran and the ship's bell.
        //   Yarn: a fiddle plays the tune, with a mandolin chop on the off-beats.
        //   Water: a marimba plays it, rolling on long notes, with bubbles at each phrase end.
        //   Wind: a tin whistle plays it, with little grace-note flicks, over a breezy wash.

        const float ShantyBpm = 70f;   // dotted quarters per minute

        static readonly string[] ShantyChords =
        {
            "Dm", "Dm", "C", "C", "Bb", "Bb A7", "Dm", "A7",
            "Dm", "F", "C", "Am", "Bb", "C", "A7", "Dm",
            "F", "C", "Dm", "Bb", "F", "C", "Gm A7", "Dm",
        };

        // One bar per line, lengths in eighth notes (6 a bar).
        static readonly string[] ShantyTune =
        {
            "A4:2 D5:1 D5:2 E5:1", "F5:2 E5:1 D5:3", "G5:2 E5:1 C5:2 E5:1", "G5:3 E5:3",
            "F5:2 G5:1 A5:2 F5:1", "D6:3 C#6:3", "D6:2 A5:1 F5:2 D5:1", "E5:3 A4:3",
            "A4:2 D5:1 F5:2 A5:1", "C6:3 A5:2 F5:1", "G5:2 E5:1 G5:2 C6:1", "A5:3 E5:3",
            "F5:2 D5:1 F5:2 Bb5:1", "A5:2 G5:1 E5:3", "C#5:2 E5:1 A5:2 G5:1", "D5:6",
            "C5:1 F5:1 A5:1 C6:3", "E5:1 G5:1 C6:1 E6:3", "D6:2 C6:1 A5:2 F5:1", "D6:3 Bb5:3",
            "A5:2 C6:1 F6:2 C6:1", "E6:2 D6:1 C6:2 G5:1", "Bb5:3 C#6:3", "D6:3 A5:2 r:1",
        };

        static AudioClip[] Compose()
        {
            float e = 60f / (ShantyBpm * 3f);   // one eighth note
            float bar = e * 6f;
            int bars = ShantyChords.Length;
            int n = Mathf.CeilToInt(bar * bars * Rate);
            var data = new float[StemCount][];
            for (int i = 0; i < StemCount; i++) data[i] = new float[n];
            var bed = data[(int)Stem.Base];
            var yarn = data[(int)Stem.Yarn];
            var water = data[(int)Stem.Water];
            var wind = data[(int)Stem.Wind];

            for (int b = 0; b < bars; b++)
            {
                float t0 = b * bar;
                var names = ShantyChords[b].Split(' ');
                string first = names[0], second = names.Length > 1 ? names[1] : names[0];
                bool chorus = b >= 16;
                for (int half = 0; half < 2; half++)
                {
                    string name = half == 0 ? first : second;
                    float h0 = t0 + half * e * 3f;
                    var ch = Ch(name, 55);
                    // Bass: the root on the big beat (the fifth on the second one if the chord stays).
                    int root = Root(name, 38);
                    int bassNote = half == 1 && first == second ? root + 7 : root;
                    AddPizz(bed, h0, Freq(bassNote), 0.17f, 100 + b * 4 + half);
                    // Accordion: "cha-cha" in the verses, long swells in the chorus.
                    if (chorus)
                        foreach (int c in ch) AddAccordion(bed, h0, e * 2.9f, Freq(c), 0.022f);
                    else
                        for (int k = 1; k <= 2; k++)
                            foreach (int c in ch) AddAccordion(bed, h0 + e * k, e * 0.75f, Freq(c), k == 1 ? 0.026f : 0.02f);
                    // Yarn's mandolin chop on the off-beats.
                    for (int c = 0; c < ch.Length; c++)
                        AddPluck(yarn, h0 + e * 2f + c * 0.01f, e * 0.8f, Freq(ch[c] + 12), 0.022f, 300 + b * 16 + half * 4 + c);
                }
                // Bodhran: big on 1, medium on 4, a light tap on 6; a little fill at phrase ends.
                AddBodhran(bed, t0, 0.13f, 400 + b);
                AddBodhran(bed, t0 + e * 3f, 0.08f, 500 + b);
                AddBrush(bed, t0 + e * 5f, 0.06f, 0.025f, 600 + b);
                if (b % 8 == 7)
                    for (int k = 3; k < 6; k++) AddBodhran(bed, t0 + e * k, 0.05f + k * 0.008f, 700 + b * 8 + k);
                // The ship's bell, "ding-ding", at the top of each section.
                if (b == 0 || b == 16)
                {
                    AddGlock(bed, t0, Freq(81), 0.05f);
                    AddGlock(bed, t0 + e * 2f, Freq(81), 0.045f);
                }
                // Wind: a breezy wash that swells over each bar.
                AddAirPad(wind, t0, bar, 0.03f, 800 + b);
            }

            // The tune, three ways.
            for (int b = 0; b < bars; b++)
            {
                foreach (var note in Line(ShantyTune[b], b * 6f))
                {
                    float t = note.Beat * e;
                    float len = note.Len * e;
                    float f = Freq(note.Midi);
                    AddFiddle(yarn, t, len * 0.96f, f, 0.075f);
                    AddMarimba(water, t, f, 0.11f);
                    // The marimba rolls on long notes.
                    for (float r = e; r < len - e * 0.5f; r += e) AddMarimba(water, t + r, f, 0.06f);
                    AddWhistle(wind, t, len * 0.95f, f, 0.065f);
                    if (note.Len >= 2f) AddWhistle(wind, t - 0.05f, 0.05f, Freq(note.Midi + 2), 0.04f);
                }
                if (b % 4 == 3) AddBloop(water, b * bar + e * 4.5f, Freq(86), 0.03f);
            }

            // Shared filter, then one gain so (base + any one tune) never clips.
            for (int i = 0; i < StemCount; i++) LowPass(data[i], 6000f);
            float peak = 0.0001f;
            for (int s = 0; s < n; s++)
            {
                float loudest = 0f;
                for (int i = 1; i < StemCount; i++) loudest = Mathf.Max(loudest, Mathf.Abs(data[i][s]));
                peak = Mathf.Max(peak, Mathf.Abs(bed[s]) + loudest);
            }
            float gain = 0.88f / peak;
            var result = new AudioClip[StemCount];
            for (int i = 0; i < StemCount; i++)
            {
                var d = data[i];
                for (int s = 0; s < n; s++)
                {
                    float x = d[s] * gain;
                    d[s] = x / (1f + Mathf.Abs(x) * 0.25f);
                }
                result[i] = AudioClip.Create("pw_music_" + ((Stem)i).ToString().ToLower(), n, 1, Rate, false);
                result[i].SetData(d, 0);
            }
            return result;
        }

        // ------------------------------------------------------------------ Dance songs (easter egg)

        static AudioClip[] danceSongs;

        // Beats per minute and beats per bar for each form's dance song.
        public static float DanceBpm(FormType f)
        {
            switch (f)
            {
                case FormType.Water: return 112f;
                case FormType.Wind: return 96f;
                default: return 132f;
            }
        }

        // Yarn: a plucky hoedown. Water: a steel-drum calypso. Wind: a floaty waltz. 8 bars each, looping.
        public static AudioClip DanceSong(FormType f)
        {
            if (danceSongs == null) danceSongs = new AudioClip[3];
            int i = (int)f;
            if (danceSongs[i] == null)
            {
                switch (f)
                {
                    case FormType.Water: danceSongs[i] = ComposeCalypso(); break;
                    case FormType.Wind: danceSongs[i] = ComposeWaltz(); break;
                    default: danceSongs[i] = ComposeHoedown(); break;
                }
            }
            return danceSongs[i];
        }

        static int[] Triad(int root, bool minor)
        {
            return new[] { root, root + (minor ? 3 : 4), root + 7 };
        }

        static AudioClip Finish(float[] data, string name)
        {
            LowPass(data, 6500f);
            float peak = 0.0001f;
            for (int i = 0; i < data.Length; i++) peak = Mathf.Max(peak, Mathf.Abs(data[i]));
            float gain = 0.8f / peak;
            for (int i = 0; i < data.Length; i++) data[i] *= gain;
            var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip ComposeHoedown()
        {
            float beat = 60f / DanceBpm(FormType.Yarn);
            int[] roots = { 55, 48, 55, 50, 55, 48, 50, 55 };   // G C G D G C D G
            var data = new float[Mathf.CeilToInt(beat * 4f * roots.Length * Rate)];
            // Banjo roll: eight notes a bar going round the chord.
            int[] roll = { 12, 16, 19, 24, 19, 16, 24, 19 };
            int[][] tune = { new[] { 19, 16, 24, 19 }, new[] { 16, 19, 24, 28 }, new[] { 19, 24, 28, 24 }, new[] { 26, 23, 19, 21 } };
            for (int b = 0; b < roots.Length; b++)
            {
                float t0 = b * beat * 4f;
                int r = roots[b];
                for (int e = 0; e < 8; e++)
                    AddPluck(data, t0 + e * beat * 0.5f, beat * 0.9f, Freq(r + roll[e]), 0.05f, 7000 + b * 8 + e);
                var line = tune[b % tune.Length];
                for (int q = 0; q < 4; q++)
                    AddPluck(data, t0 + q * beat, beat * 1.2f, Freq(r + line[q] + 12), 0.09f, 7500 + b * 4 + q);
                // Boom-chick: bass on 1 and 3 (root then fifth), brush and a soft kick.
                AddBass(data, t0, beat * 0.9f, Freq(r - 12), 0.24f);
                AddBass(data, t0 + beat * 2f, beat * 0.9f, Freq(r - 5), 0.2f);
                AddKick(data, t0, 0.1f);
                AddKick(data, t0 + beat * 2f, 0.08f);
                AddBrush(data, t0 + beat, 0.12f, 0.06f, 7900 + b);
                AddBrush(data, t0 + beat * 3f, 0.12f, 0.06f, 7950 + b);
            }
            return Finish(data, "pw_dance_yarn");
        }

        static AudioClip ComposeCalypso()
        {
            float beat = 60f / DanceBpm(FormType.Water);
            int[] roots = { 48, 53, 55, 48, 48, 53, 55, 48 };   // C F G C
            var data = new float[Mathf.CeilToInt(beat * 4f * roots.Length * Rate)];
            // 3 + 3 + 2 rhythm (in eighths): hits on 0, 1.5 and 3 beats.
            float[] hits = { 0f, 1.5f, 3f };
            int[][] tune = { new[] { 24, 28, 31, 28 }, new[] { 24, 21, 28, 24 }, new[] { 26, 23, 31, 26 }, new[] { 28, 24, 19, 24 } };
            for (int b = 0; b < roots.Length; b++)
            {
                float t0 = b * beat * 4f;
                int r = roots[b];
                var triad = Triad(r, false);
                foreach (var h in hits)
                {
                    AddBass(data, t0 + h * beat, beat * 1.1f, Freq(r - 12 + (h == 1.5f ? 7 : 0)), 0.22f);
                    for (int c = 0; c < 3; c++) AddSteelDrum(data, t0 + h * beat, Freq(triad[c] + 12), 0.035f);
                }
                var line = tune[b % tune.Length];
                for (int q = 0; q < 4; q++)
                {
                    float at = t0 + q * beat + (q % 2 == 1 ? beat * 0.5f : 0f); // pushed off-beats
                    AddSteelDrum(data, at, Freq(r + line[q]), 0.08f);
                }
                // Shaker on every eighth, a bubble blip at the end of the bar.
                for (int e = 0; e < 8; e++) AddBrush(data, t0 + e * beat * 0.5f, 0.05f, e % 2 == 0 ? 0.03f : 0.02f, 8100 + b * 8 + e);
                AddKick(data, t0, 0.1f);
                AddBloop(data, t0 + beat * 3.5f, Freq(r + 36), 0.03f);
            }
            return Finish(data, "pw_dance_water");
        }

        static AudioClip ComposeWaltz()
        {
            float beat = 60f / DanceBpm(FormType.Wind);
            int[] roots = { 57, 53, 48, 55, 57, 53, 55, 57 };   // Am F C G Am F G Am
            bool[] minor = { true, false, false, false, true, false, false, true };
            var data = new float[Mathf.CeilToInt(beat * 3f * roots.Length * Rate)];
            int[] tune = { 12, 9, 7, 11, 12, 16, 14, 12 };
            for (int b = 0; b < roots.Length; b++)
            {
                float t0 = b * beat * 3f;
                int r = roots[b];
                var triad = Triad(r, minor[b]);
                // Oom-pah-pah: bass on 1, harp chord notes on 2 and 3.
                AddBass(data, t0, beat * 1.4f, Freq(r - 12), 0.22f);
                for (int q = 1; q < 3; q++)
                    for (int c = 0; c < 3; c++) AddMusicBox(data, t0 + q * beat, beat * 1.2f, Freq(triad[c] + 12), 0.03f);
                // Flute: one long note a bar, with a little pickup into the next.
                AddFlute(data, t0, beat * 2.2f, Freq(r + tune[b]), 0.08f);
                AddFlute(data, t0 + beat * 2.4f, beat * 0.5f, Freq(r + tune[b] + 2), 0.05f);
                AddAirPad(data, t0, beat * 3f, 0.03f, 8300 + b);
            }
            return Finish(data, "pw_dance_wind");
        }

        // Steel drum: a bright, quickly fading tone with slightly out-of-tune upper partials.
        static void AddSteelDrum(float[] data, float start, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(0.6f * Rate);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.003f) * Mathf.Exp(-t * 6f);
                float w = Mathf.Sin(2f * Mathf.PI * f * t)
                        + 0.5f * Mathf.Sin(2f * Mathf.PI * f * 2.01f * t) * Mathf.Exp(-t * 10f)
                        + 0.25f * Mathf.Sin(2f * Mathf.PI * f * 3.02f * t) * Mathf.Exp(-t * 16f);
                Add(data, s0 + i, w * env * vol);
            }
        }

        // Plucked string (Karplus-Strong): a burst of noise fed round a short, damped delay line.
        static void AddPluck(float[] data, float start, float dur, float f, float vol, int seed)
        {
            var rng = new System.Random(seed);
            int period = Mathf.Max(2, Mathf.RoundToInt(Rate / f));
            var ring = new float[period];
            for (int i = 0; i < period; i++) ring[i] = (float)(rng.NextDouble() * 2.0 - 1.0);
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(dur * Rate);
            int idx = 0;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                int next = (idx + 1) % period;
                float y = ring[idx];
                ring[idx] = 0.5f * (ring[idx] + ring[next]) * 0.996f;
                idx = next;
                float env = Mathf.Clamp01((dur - t) / 0.08f);
                Add(data, s0 + i, y * env * vol);
            }
        }

        // Soft flute: a sine with a slow-starting vibrato and a little breath noise.
        static void AddFlute(float[] data, float start, float dur, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(dur * Rate);
            float phase = 0f;
            uint noise = 22222u;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float vib = 1f + 0.006f * Mathf.Sin(2f * Mathf.PI * 5f * t) * Mathf.Clamp01(t / 0.3f);
                phase += 2f * Mathf.PI * f * vib / Rate;
                noise = noise * 1664525u + 1013904223u;
                float breath = ((noise >> 9) / 4194304f - 1f) * 0.04f;
                float env = Mathf.Clamp01(t / 0.08f) * Mathf.Clamp01((dur - t) / 0.12f);
                float w = Mathf.Sin(phase) + 0.15f * Mathf.Sin(phase * 2f) + breath;
                Add(data, s0 + i, w * env * vol);
            }
        }

        // Quiet wash of filtered noise that swells in and out over a bar.
        static void AddAirPad(float[] data, float start, float dur, float vol, int seed)
        {
            var rng = new System.Random(seed);
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(dur * Rate);
            float lp = 0f, lp2 = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float x = (float)(rng.NextDouble() * 2.0 - 1.0);
                lp += 0.05f * (x - lp);
                lp2 += 0.05f * (lp - lp2);
                float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / dur));
                Add(data, s0 + i, lp2 * env * vol * 6f);
            }
        }

        // Tiny upward "bloop", like a bubble rising.
        static void AddBloop(float[] data, float start, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(0.09f * Rate);
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                phase += 2f * Mathf.PI * f * Mathf.Lerp(0.8f, 1.25f, t / 0.09f) / Rate;
                float env = Mathf.Clamp01(t / 0.004f) * Mathf.Exp(-t * 35f);
                Add(data, s0 + i, Mathf.Sin(phase) * env * vol);
            }
        }

        static void AddBass(float[] data, float start, float dur, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(dur * Rate);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.015f) * Mathf.Exp(-t * 1.2f) * Mathf.Clamp01((dur - t) / 0.1f);
                float w = Mathf.Sin(2f * Mathf.PI * f * t) + 0.2f * Mathf.Sin(2f * Mathf.PI * f * 2f * t);
                Add(data, s0 + i, w * env * vol);
            }
        }

        static void AddMusicBox(float[] data, float start, float dur, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(dur * Rate);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.003f) * Mathf.Exp(-t * 2.8f) * Mathf.Clamp01((dur - t) / 0.1f);
                float w = Mathf.Sin(2f * Mathf.PI * f * t) + 0.35f * Mathf.Sin(2f * Mathf.PI * f * 3f * t) * Mathf.Exp(-t * 6f);
                Add(data, s0 + i, w * env * vol);
            }
        }

        static void AddBrush(float[] data, float start, float dur, float vol, int seed)
        {
            var rng = new System.Random(seed);
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(dur * Rate);
            float lp = 0f, prev = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float x = (float)(rng.NextDouble() * 2.0 - 1.0);
                lp += 0.35f * (x - lp);
                float hp = lp - prev; // remove rumble
                prev = lp;
                float env = Mathf.Clamp01(t / 0.004f) * Mathf.Pow(1f - t / dur, 3f);
                Add(data, s0 + i, hp * env * vol * 2.5f);
            }
        }

        static void AddKick(float[] data, float start, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(0.25f * Rate);
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float f = Mathf.Lerp(110f, 50f, Mathf.Clamp01(t / 0.08f));
                phase += 2f * Mathf.PI * f / Rate;
                float env = Mathf.Clamp01(t / 0.003f) * Mathf.Exp(-t * 14f);
                Add(data, s0 + i, Mathf.Sin(phase) * env * vol);
            }
        }

        static void Add(float[] data, int index, float v)
        {
            // Wrap so notes ringing past the end blend into the loop start.
            int n = data.Length;
            index %= n;
            if (index < 0) index += n;
            data[index] += v;
        }

        static void LowPass(float[] data, float cutoffHz)
        {
            float rc = 1f / (2f * Mathf.PI * cutoffHz);
            float dt = 1f / Rate;
            float a = dt / (rc + dt);
            float y = 0f;
            // Two passes over the loop so the filter state is continuous at the seam.
            for (int pass = 0; pass < 2; pass++)
                for (int i = 0; i < data.Length; i++)
                {
                    y += a * (data[i] - y);
                    if (pass == 1) data[i] = y;
                }
        }
    }
}

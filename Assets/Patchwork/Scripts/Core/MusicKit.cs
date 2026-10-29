using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Patchwork
{
    // Tools for writing the soundtrack: tunes written as text ("C5:0.5 F5:0.5 A5:1 r:1 ..."),
    // chord names ("Gm7", "D7", "Bbm"), and the instruments the songs are played on. Every sound
    // is synthesised when the song is first needed, so there are no audio files.
    public partial class MusicPlayer
    {
        protected struct Note
        {
            public float Beat;   // when it starts, in beats from the start of the song
            public int Midi;
            public float Len;    // in beats
        }

        // MIDI note number to frequency (A4 = 69 = 440 Hz).
        static float Freq(float midi)
        {
            return 440f * Mathf.Pow(2f, (midi - 69f) / 12f);
        }

        // Parses a tune: space-separated "Name:beats" tokens, "r" for a rest, "|" ignored.
        // Names are like C5, F#4, Bb5 (C4 is middle C).
        protected static List<Note> Line(string text, float startBeat)
        {
            var list = new List<Note>();
            float b = startBeat;
            foreach (var raw in text.Split(new[] { ' ', '\n', '\r', '\t' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                if (raw == "|") continue;
                int colon = raw.IndexOf(':');
                string name = colon >= 0 ? raw.Substring(0, colon) : raw;
                float len = colon >= 0 ? float.Parse(raw.Substring(colon + 1), CultureInfo.InvariantCulture) : 1f;
                if (name != "r") list.Add(new Note { Beat = b, Midi = NoteNum(name), Len = len });
                b += len;
            }
            return list;
        }

        protected static int NoteNum(string s)
        {
            int pc;
            switch (s[0])
            {
                case 'C': pc = 0; break;
                case 'D': pc = 2; break;
                case 'E': pc = 4; break;
                case 'F': pc = 5; break;
                case 'G': pc = 7; break;
                case 'A': pc = 9; break;
                default: pc = 11; break;
            }
            int i = 1;
            while (i < s.Length && (s[i] == '#' || s[i] == 'b'))
            {
                pc += s[i] == '#' ? 1 : -1;
                i++;
            }
            int octave = int.Parse(s.Substring(i), CultureInfo.InvariantCulture);
            return 12 * (octave + 1) + pc;
        }

        // Chord tones (low to high) for a chord name, voiced with the root at or above "low".
        protected static int[] Ch(string name, int low)
        {
            int pc = NoteNum(RootName(name) + "4") - 60;
            string q = name.Substring(RootName(name).Length);
            int[] iv;
            switch (q)
            {
                case "m": iv = new[] { 0, 3, 7 }; break;
                case "7": iv = new[] { 0, 4, 7, 10 }; break;
                case "m7": iv = new[] { 0, 3, 7, 10 }; break;
                case "maj7": iv = new[] { 0, 4, 7, 11 }; break;
                case "dim": iv = new[] { 0, 3, 6 }; break;
                case "sus4": iv = new[] { 0, 5, 7 }; break;
                default: iv = new[] { 0, 4, 7 }; break;
            }
            int root = pc;
            while (root < low) root += 12;
            var notes = new int[iv.Length];
            for (int i = 0; i < iv.Length; i++) notes[i] = root + iv[i];
            return notes;
        }

        // The chord's root note at or above "low".
        protected static int Root(string name, int low)
        {
            int r = NoteNum(RootName(name) + "4") - 60;
            while (r < low) r += 12;
            return r;
        }

        static string RootName(string name)
        {
            int i = 1;
            while (i < name.Length && (name[i] == '#' || name[i] == 'b')) i++;
            return name.Substring(0, i);
        }

        // Swing: notes on the "and" of a beat land a little late, for a bouncy, skipping feel.
        protected static float Swing(float beat, float amount)
        {
            float frac = beat - Mathf.Floor(beat);
            return Mathf.Abs(frac - 0.5f) < 0.01f ? beat + amount : beat;
        }

        // ------------------------------------------------------------------ Instruments

        // Accordion / concertina: two slightly detuned reeds, bright odd partials, bellows wobble.
        protected static void AddAccordion(float[] data, float start, float dur, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt((dur + 0.06f) * Rate);
            float p1 = 0f, p2 = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.03f) * Mathf.Clamp01((dur + 0.06f - t) / 0.06f) * (0.85f + 0.15f * Mathf.Exp(-t * 4f));
                p1 += 2f * Mathf.PI * f / Rate;
                p2 += 2f * Mathf.PI * f * 1.004f / Rate;
                float w = 0f;
                for (int h = 1; h <= 5; h++)
                {
                    float amp = (h % 2 == 1 ? 1f : 0.45f) / h;
                    w += (Mathf.Sin(p1 * h) + Mathf.Sin(p2 * h)) * amp;
                }
                w *= 1f + 0.06f * Mathf.Sin(2f * Mathf.PI * 5.5f * t);
                Add(data, s0 + i, w * 0.5f * env * vol);
            }
        }

        // Fiddle: a bowed string (sawtooth-ish), slow-starting vibrato and a little bow hiss.
        protected static void AddFiddle(float[] data, float start, float dur, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt((dur + 0.05f) * Rate);
            float phase = 0f;
            uint noise = 777u;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float vib = 1f + 0.008f * Mathf.Sin(2f * Mathf.PI * 5.8f * t) * Mathf.Clamp01((t - 0.12f) * 4f);
                phase += 2f * Mathf.PI * f * vib / Rate;
                float w = 0f;
                for (int h = 1; h <= 6; h++) w += Mathf.Sin(phase * h) / h;
                noise = noise * 1664525u + 1013904223u;
                w += ((noise >> 9) / 4194304f - 1f) * 0.06f;
                float env = Mathf.Clamp01(t / 0.05f) * Mathf.Clamp01((dur + 0.05f - t) / 0.05f) * (0.8f + 0.2f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / Mathf.Max(0.05f, dur))));
                Add(data, s0 + i, w * env * vol);
            }
        }

        // Marimba: a warm wooden bar - fundamental plus a quick fourth partial.
        protected static void AddMarimba(float[] data, float start, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(0.9f * Rate);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.002f) * Mathf.Exp(-t * 4.5f);
                float w = Mathf.Sin(2f * Mathf.PI * f * t) + 0.35f * Mathf.Sin(2f * Mathf.PI * f * 3.95f * t) * Mathf.Exp(-t * 18f);
                Add(data, s0 + i, w * env * vol);
            }
        }

        // Tin whistle: a bright breathy flute.
        protected static void AddWhistle(float[] data, float start, float dur, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(dur * Rate);
            float phase = 0f;
            uint noise = 4242u;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float vib = 1f + 0.005f * Mathf.Sin(2f * Mathf.PI * 6f * t) * Mathf.Clamp01((t - 0.15f) * 4f);
                phase += 2f * Mathf.PI * f * vib / Rate;
                noise = noise * 1664525u + 1013904223u;
                float breath = ((noise >> 9) / 4194304f - 1f) * 0.07f;
                float env = Mathf.Clamp01(t / 0.03f) * Mathf.Clamp01((dur - t) / 0.06f);
                float w = Mathf.Sin(phase) + 0.25f * Mathf.Sin(phase * 2f) + 0.08f * Mathf.Sin(phase * 3f) + breath;
                Add(data, s0 + i, w * env * vol);
            }
        }

        // Bodhran (frame drum): a low thump with a skin slap.
        protected static void AddBodhran(float[] data, float start, float vol, int seed)
        {
            AddKick(data, start, vol);
            AddBrush(data, start, 0.07f, vol * 0.35f, seed);
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(0.3f * Rate);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                Add(data, s0 + i, Mathf.Sin(2f * Mathf.PI * 145f * t) * Mathf.Exp(-t * 18f) * vol * 0.4f);
            }
        }

        // Celesta: a soft glassy bell, rounder than a music box.
        protected static void AddCelesta(float[] data, float start, float dur, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt((dur + 0.4f) * Rate);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.003f) * Mathf.Exp(-t * 3f) * Mathf.Clamp01((dur + 0.4f - t) / 0.2f);
                float w = Mathf.Sin(2f * Mathf.PI * f * t) + 0.2f * Mathf.Sin(2f * Mathf.PI * f * 2f * t) * Mathf.Exp(-t * 5f)
                        + 0.12f * Mathf.Sin(2f * Mathf.PI * f * 4.1f * t) * Mathf.Exp(-t * 12f);
                Add(data, s0 + i, w * env * vol);
            }
        }

        // Glockenspiel: a bright metal bar with a ringing, slightly sour overtone.
        protected static void AddGlock(float[] data, float start, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(1.4f * Rate);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.001f) * Mathf.Exp(-t * 2.6f);
                float w = Mathf.Sin(2f * Mathf.PI * f * t) + 0.3f * Mathf.Sin(2f * Mathf.PI * f * 2.76f * t) * Mathf.Exp(-t * 6f);
                Add(data, s0 + i, w * env * vol);
            }
        }

        // Toy piano: a struck metal rod - plinky, with clangy inharmonic partials.
        protected static void AddToyPiano(float[] data, float start, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(0.8f * Rate);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.001f) * Mathf.Exp(-t * 5f);
                float w = Mathf.Sin(2f * Mathf.PI * f * t) + 0.45f * Mathf.Sin(2f * Mathf.PI * f * 2.92f * t) * Mathf.Exp(-t * 9f)
                        + 0.2f * Mathf.Sin(2f * Mathf.PI * f * 5.4f * t) * Mathf.Exp(-t * 20f);
                Add(data, s0 + i, w * env * vol);
            }
        }

        // A soft, rounded square-wave lead - a little chiptune voice, kept gentle for background play.
        protected static void AddChip(float[] data, float start, float dur, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt((dur + 0.04f) * Rate);
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float vib = 1f + 0.01f * Mathf.Sin(2f * Mathf.PI * 6f * t) * Mathf.Clamp01((t - 0.18f) * 3f);
                phase += 2f * Mathf.PI * f * vib / Rate;
                // Band-limited square from odd harmonics.
                float w = Mathf.Sin(phase) + Mathf.Sin(phase * 3f) / 3f + Mathf.Sin(phase * 5f) / 5f + Mathf.Sin(phase * 7f) / 7f;
                float env = Mathf.Clamp01(t / 0.008f) * Mathf.Clamp01((dur + 0.04f - t) / 0.04f) * (0.75f + 0.25f * Mathf.Exp(-t * 6f));
                Add(data, s0 + i, w * env * vol);
            }
        }

        // Pizzicato double bass: a short, round plucked note with a low thump.
        protected static void AddPizz(float[] data, float start, float f, float vol, int seed)
        {
            AddPluck(data, start, 0.45f, f, vol * 0.7f, seed);
            AddBass(data, start, 0.4f, f, vol);
        }

        // Brushed snare: a hiss with a little drum tone under it.
        protected static void AddSnare(float[] data, float start, float vol, int seed)
        {
            AddBrush(data, start, 0.16f, vol, seed);
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(0.12f * Rate);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                Add(data, s0 + i, Mathf.Sin(2f * Mathf.PI * 190f * t) * Mathf.Exp(-t * 30f) * vol * 0.5f);
            }
        }

        // Strings: a soft, slow-swelling ensemble (three detuned voices).
        protected static void AddStrings(float[] data, float start, float dur, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt((dur + 0.3f) * Rate);
            float pa = 0f, pb = 0f, pc = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.35f) * Mathf.Clamp01((dur + 0.3f - t) / 0.3f);
                pa += 2f * Mathf.PI * f / Rate;
                pb += 2f * Mathf.PI * f * 1.005f / Rate;
                pc += 2f * Mathf.PI * f * 0.995f / Rate;
                float w = 0f;
                for (int h = 1; h <= 4; h++) w += (Mathf.Sin(pa * h) + Mathf.Sin(pb * h) + Mathf.Sin(pc * h)) / (h * h * 0.8f + 0.2f);
                Add(data, s0 + i, w * 0.33f * env * vol);
            }
        }

        // Timpani: a deep tuned drum.
        protected static void AddTimpani(float[] data, float start, float f, float vol)
        {
            int s0 = Mathf.FloorToInt(start * Rate);
            int count = Mathf.CeilToInt(1.2f * Rate);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.004f) * Mathf.Exp(-t * 3.2f);
                float w = Mathf.Sin(2f * Mathf.PI * f * t) + 0.4f * Mathf.Sin(2f * Mathf.PI * f * 1.5f * t) * Mathf.Exp(-t * 6f);
                Add(data, s0 + i, w * env * vol);
            }
        }

        // A harp glissando: a quick run up through the given notes.
        protected static void AddGliss(float[] data, float start, int[] notes, float span, float vol, int seed)
        {
            for (int i = 0; i < notes.Length; i++)
                AddPluck(data, start + span * i / notes.Length, 1.2f, Freq(notes[i]), vol * (0.6f + 0.4f * i / notes.Length), seed + i);
        }

        // Peak-normalises a set of stems together and turns them into looping clips.
        protected static AudioClip[] FinishStems(float[][] data, string prefix, string[] names, float cutoff)
        {
            int n = data[0].Length;
            for (int i = 0; i < data.Length; i++) LowPass(data[i], cutoff);
            float peak = 0.0001f;
            for (int s = 0; s < n; s++)
            {
                float sum = 0f;
                for (int i = 0; i < data.Length; i++) sum += Mathf.Abs(data[i][s]);
                peak = Mathf.Max(peak, sum);
            }
            float gain = 0.92f / peak;
            var result = new AudioClip[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                var d = data[i];
                for (int s = 0; s < n; s++)
                {
                    float x = d[s] * gain;
                    // Gentle soft-clip so busy moments don't poke out.
                    d[s] = x / (1f + Mathf.Abs(x) * 0.25f);
                }
                result[i] = AudioClip.Create(prefix + names[i], n, 1, Rate, false);
                result[i].SetData(d, 0);
            }
            return result;
        }
    }
}

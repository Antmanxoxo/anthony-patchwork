using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // This visit's score. Picking things up quickly one after another builds a chain, and the
    // chain raises the multiplier (x2 after 5, up to x5). Every gain also makes a little "+30"
    // that floats up from where it happened, and every bubble sends a spark flying into the
    // score counter. Long chains get a shout ("Nice!" at 10, and so on), and the chain's
    // timer shows under the counter so you can see how long you've got to the next one.
    public class ScoreKeeper
    {
        public const float ChainWindow = 1.6f;
        // Pentatonic steps for the rising pickup notes (as pitch multipliers).
        static readonly float[] Steps = { 1f, 1.122f, 1.26f, 1.498f, 1.682f, 2f, 2.245f, 2.52f, 2.997f, 3.364f, 4f };

        public int Total { get; private set; }
        public int Chain { get; private set; }
        public float Shown { get; private set; }     // the number on screen, counting up to Total
        public float Pop { get; private set; }       // bounce on the counter when it grows
        float lastGain = -10f;

        public struct Popup
        {
            public Vector3 World;
            public string Text;
            public float Age;
            public Color Color;
        }
        public readonly List<Popup> Popups = new List<Popup>();

        // A spark flying from where something was picked up into the score counter.
        public struct Spark
        {
            public Vector3 World;
            public float Age;
            public Color Color;
        }
        public readonly List<Spark> Sparks = new List<Spark>();
        public const float SparkTime = 0.55f;

        // The big shout for a long chain (or a full trail): words, and how long it's been up.
        public string Callout { get; private set; }
        public float CalloutAge { get; private set; }
        public Color CalloutColor { get; private set; }

        public void Shout(string words, Color c)
        {
            Callout = words;
            CalloutAge = 0f;
            CalloutColor = c;
        }

        static readonly int[] Milestones = { 10, 25, 50, 100 };
        static readonly string[] Shouts = { "NICE!", "GREAT CHAIN!", "STITCHTASTIC!", "UNSTOPPABLE!" };

        // How much of the chain's time is left, 1 (just added to) to 0 (gone).
        public float ChainLeft { get { return Chain > 0 ? Mathf.Clamp01(1f - (Time.time - lastGain) / ChainWindow) : 0f; } }

        public int Multiplier
        {
            get { return Mathf.Min(5, 1 + Chain / 5); }
        }

        public bool ChainAlive
        {
            get { return Time.time - lastGain <= ChainWindow; }
        }

        // A score bubble (or anything else that should build the chain).
        public void AddChained(int baseValue, Vector3 at)
        {
            if (!ChainAlive) Chain = 0;
            Chain++;
            lastGain = Time.time;
            int value = baseValue * Multiplier;
            Total += value;
            Pop = 1f;
            Sfx.PlayPitched(SfxId.ScoreBubble, 0.32f, Steps[(Chain - 1) % Steps.Length]);
            AddPopup(at, "+" + value, Multiplier > 1 ? new Color(1f, 0.82f, 0.3f) : Color.white);
            if (Sparks.Count >= 40) Sparks.RemoveAt(0);
            Sparks.Add(new Spark { World = at, Age = 0f, Color = Multiplier > 1 ? new Color(1f, 0.82f, 0.3f) : new Color(0.6f, 0.85f, 1f) });
            for (int i = 0; i < Milestones.Length; i++)
                if (Chain == Milestones[i])
                {
                    int bonus = Milestones[i] * 10;
                    Total += bonus;
                    AddPopup(at + Vector3.up * 0.6f, "CHAIN " + Chain + "  +" + bonus, new Color(1f, 0.55f, 0.75f));
                    Shout(Shouts[i], i < 2 ? new Color(1f, 0.82f, 0.3f) : new Color(1f, 0.55f, 0.8f));
                    Sfx.Play(i < 2 ? SfxId.Complete : SfxId.PrizeFanfare, 0.45f);
                }
        }

        // Points for doing something (popping a Snarling, ringing a bell). Doesn't build the chain.
        public void AddBonus(int value, Vector3 at)
        {
            Total += value;
            Pop = 1f;
            AddPopup(at, "+" + value, new Color(0.65f, 0.95f, 1f));
        }

        void AddPopup(Vector3 at, string text, Color c)
        {
            if (Popups.Count >= 14) Popups.RemoveAt(0);
            Popups.Add(new Popup { World = at, Text = text, Age = 0f, Color = c });
        }

        public void Tick(float dt)
        {
            Shown = Mathf.MoveTowards(Shown, Total, Mathf.Max(60f, (Total - Shown) * 6f) * dt);
            Pop = Mathf.MoveTowards(Pop, 0f, dt * 3f);
            if (!ChainAlive && Chain > 0) Chain = 0;
            if (Callout != null)
            {
                CalloutAge += dt;
                if (CalloutAge > 1.6f) Callout = null;
            }
            for (int i = Sparks.Count - 1; i >= 0; i--)
            {
                var s = Sparks[i];
                s.Age += dt;
                if (s.Age >= SparkTime) { Sparks.RemoveAt(i); Pop = 1f; }
                else Sparks[i] = s;
            }
            for (int i = Popups.Count - 1; i >= 0; i--)
            {
                var p = Popups[i];
                p.Age += dt;
                if (p.Age > 1.1f) Popups.RemoveAt(i);
                else Popups[i] = p;
            }
        }

        // ----- Medals -----
        public enum Medal { None, Bronze, Silver, Gold }

        // Thresholds come from how many points of bubbles the level has (bursts and bonuses
        // are extra, and chains multiply), so gold means collecting most of it in long chains.
        public static Medal MedalFor(int score, int possible)
        {
            if (possible <= 0) return Medal.None;
            if (score >= possible * 1.6f) return Medal.Gold;
            if (score >= possible * 1.0f) return Medal.Silver;
            if (score >= possible * 0.5f) return Medal.Bronze;
            return Medal.None;
        }

        public static Color MedalColor(Medal m)
        {
            switch (m)
            {
                case Medal.Gold: return new Color(1f, 0.8f, 0.25f);
                case Medal.Silver: return new Color(0.82f, 0.85f, 0.9f);
                case Medal.Bronze: return new Color(0.85f, 0.55f, 0.32f);
                default: return new Color(0.6f, 0.6f, 0.62f);
            }
        }

        // Shortcut used by things in the world: award points through the running game, if any.
        public static void Bonus(int value, Vector3 at)
        {
            var game = PatchworkGame.Instance;
            if (game != null && game.Score != null) game.Score.AddBonus(value, at);
        }
    }
}

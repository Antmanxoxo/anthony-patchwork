using UnityEngine;

namespace Patchwork
{
    // Keeping Toybox Town's story clear, the same way as the museum's: the next thing to do (on
    // the band card), a gold pointer to it, a gold "!" over anyone with news, and remarks as
    // Patch goes by.
    public class Level1Guide : MonoBehaviour, IQuestGuide
    {
        public Level1Director Director;
        string lastObjective;
        float tick;
        BandPiece[] pieces;

        void Awake() { QuestGuide.Current = this; }
        void OnDestroy() { if (QuestGuide.Current == (IQuestGuide)this) QuestGuide.Current = null; }

        static bool Seen(string k) { return GameState.IsHintSeen("L1_g_" + k); }
        static void Mark(string k) { GameState.SetHintSeen("L1_g_" + k); }
        static bool Done { get { return GameState.IsLevelComplete(Level1Builder.SceneName); } }

        void Start()
        {
            pieces = FindObjectsByType<BandPiece>(FindObjectsInactive.Include);
            if (Director == null) return;
            // News: the Mayor once the band is whole; the bride and the band till you've asked.
            News(Director.Mayor, () => !Done && Level1Director.BandCount() >= 3, null);
            News(Director.Bride, () => !Seen("talk_bride") && Level1Director.GiftCount() < Level1Director.GiftIds.Length, () => Mark("talk_bride"));
            News(Director.Lute, () => !Seen("talk_lute") && !Level1Director.Has(Level1Director.LuteId), () => Mark("talk_lute"));
            News(Director.Trumpet, () => !Seen("talk_trumpet") && !Level1Director.Has(Level1Director.TrumpetId), () => Mark("talk_trumpet"));
            // What they say as he goes by.
            Bark(Director.Lute, () => Level1Director.Has(Level1Director.LuteId) ? "Listen to her sing! Well. Twang." : "My lute's lost its only string. Up on Spindle Hill, they say.");
            Bark(Director.Trumpet, () => Level1Director.Has(Level1Director.TrumpetId) ? "Ta-raaa! Shiny as a new button." : "Gummed up solid. Only valve dew from the Dye Works will shift it.");
            Bark(Director.Drummer, () => Level1Director.BandCount() >= 3 ? "One, two, one two three four!" : "Can't play a wedding with just a drum. Well. I could. Nobody'd dance.");
            Bark(Director.Mayor, () => Done ? "Best day of my life. Mind the confetti." : Level1Director.BandCount() >= 3 ? "The band's back? Then come here, come here!" : "No band, no wedding. No wedding, no cake. Think of the cake!");
            Bark(Director.Bride, () => Pick("Five presents went astray this morning. Five!", "Do keep an eye out for anything wedding-shaped."));
            if (Director.StrandedPiper != null) Bark(Director.StrandedPiper, () => "Help! I'm being very brave! Up here!");
            foreach (var n in Director.Crowd) if (n != null && n.Bark == null) Bark(n, () => Pick("Is the wedding still on?", "Grey, grey everywhere. Ugh.", "Have you seen the band? I mean, seen them PLAY?", "Lovely day for it. Apart from everything."));
        }

        static string Pick(params string[] lines) { return lines[Random.Range(0, lines.Length)]; }

        static void News(ToyNpc npc, System.Func<bool> has, System.Action told)
        {
            if (npc == null) return;
            npc.HasNews = has;
            if (told == null) return;
            var was = npc.TalkEnded;
            npc.TalkEnded = () => { if (was != null) was(); told(); };
        }

        static void Bark(ToyNpc npc, System.Func<string> bark) { if (npc != null) npc.Bark = bark; }

        // The nearest band piece still to find.
        BandPiece NextPiece()
        {
            if (pieces == null) return null;
            var game = PatchworkGame.Instance;
            Vector3 at = game != null && game.Player != null ? game.Player.transform.position : Vector3.zero;
            BandPiece best = null;
            float bestD = float.MaxValue;
            foreach (var p in pieces)
            {
                if (p == null || Level1Director.Has(p.Id)) continue;
                float d = (p.transform.position - at).sqrMagnitude;
                if (d < bestD) { bestD = d; best = p; }
            }
            return best;
        }

        public string Objective()
        {
            if (Done) return "Toybox Town is mended! Lost wedding presents found: " + Level1Director.GiftCount() + " of " + Level1Director.GiftIds.Length + ".";
            int n = Level1Director.BandCount();
            if (n >= 3) return "The whole band is back. Go and tell Mayor Bramblebutton!";
            var p = NextPiece();
            if (p == null) return "Find the band's three missing pieces.  (" + n + " of 3)";
            switch (p.Kind)
            {
                case BandPiece.Part.Lute: return "Find the lute's lost string, up on Spindle Hill.  (Band " + n + " of 3)";
                case BandPiece.Part.Trumpet: return "Fetch a drop of valve dew from the Dye Works, for the trumpet.  (Band " + n + " of 3)";
                default: return "Rescue Wisp the piper from the weathervane.  (Band " + n + " of 3)";
            }
        }

        public bool ObjectiveTarget(out Vector3 at, out string label)
        {
            at = Vector3.zero;
            label = null;
            if (Done || Director == null) return false;
            if (Level1Director.BandCount() >= 3)
            {
                if (Director.Mayor == null) return false;
                at = Director.Mayor.transform.position;
                label = "The Mayor";
                return true;
            }
            var p = NextPiece();
            if (p == null) return false;
            at = p.transform.position;
            label = p.Kind == BandPiece.Part.Lute ? "The lute string" : p.Kind == BandPiece.Part.Trumpet ? "Valve dew" : "Wisp";
            return true;
        }

        void Update()
        {
            tick -= Time.deltaTime;
            if (tick > 0f) return;
            tick = 0.3f;
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) return;
            string obj = Objective();
            if (lastObjective != null && obj != lastObjective && game.Mode == UIMode.Gameplay) game.ShowToast("Next: " + obj);
            lastObjective = obj;
        }
    }
}

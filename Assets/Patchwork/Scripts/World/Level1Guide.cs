using System.Collections.Generic;
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

        // ------------------------------------------------------------------ The quest menu

        public void BuildLog(List<QuestEntry> log)
        {
            var game = PatchworkGame.Instance;
            var level = game != null ? game.Level : null;
            bool lute = Level1Director.Has(Level1Director.LuteId), trumpet = Level1Director.Has(Level1Director.TrumpetId), piper = Level1Director.Has(Level1Director.PiperId);
            int band = Level1Director.BandCount();

            log.Add(new QuestEntry
            {
                Id = "main", Type = QuestEntry.Kind.Main, Title = "The Wedding Band", Giver = "Mayor Bramblebutton", Done = Done,
                Summary = "Today Mayor Bramblebutton marries Lady Lacewing, but the Fray have scattered the town band, and he can't be married without music. Bring all three players back.",
            }.Step("Find the lute's lost string", lute)
             .Step("Fetch valve dew for the trumpet", trumpet)
             .Step("Bring Wisp the piper home", piper)
             .Step("Tell the Mayor the band is back", Done));

            var dye = FindAnyObjectByType<DyeWorksQuest>();
            int taps = dye != null ? dye.Count : 0;
            log.Add(new QuestEntry
            {
                Id = "lute", Type = QuestEntry.Kind.Main, Title = "The Lute's String", Giver = "The lute player", Done = lute,
                Summary = "A pirate up on Spindle Hill is sitting on the lute's only string. He thinks a lute is \"loot\", and wants his own treasure back first.",
                Target = () => lute ? (Vector3?)null : PieceAt(BandPiece.Part.Lute), TargetLabel = "The lute string",
            }.Step("Talk to the pirate on Spindle Hill", PirateQuest.Met || lute)
             .Step("Dig up his treasure in the sandpit, by his wrecked boat", PirateQuest.HasTreasure || lute)
             .Step("Give it back, and take the string", lute));
            log.Add(new QuestEntry
            {
                Id = "trumpet", Type = QuestEntry.Kind.Main, Title = "Valve Dew", Giver = "The trumpeter", Done = trumpet,
                Summary = "The trumpet's gummed up solid. Only valve dew from the Dye Works will shift it, and the dew only gathers while all three dye taps run.",
                Target = () => trumpet ? (Vector3?)null : PieceAt(BandPiece.Part.Trumpet), TargetLabel = "The Dye Works",
            }.Step("Open the three dye taps  (" + taps + "/3: two in the Dye Works, one at the canal's waterline)", taps >= 3 || trumpet)
             .Step("Collect the valve dew", trumpet));
            log.Add(new QuestEntry
            {
                Id = "piper", Type = QuestEntry.Kind.Main, Title = "Wisp the Piper", Giver = "The drummer", Done = piper,
                Summary = "Wisp is so jumpy he bolts for the next cloud up whenever anyone comes near. Chase him up the clouds until he runs out of sky.",
                Target = () => piper ? (Vector3?)null : PieceAt(BandPiece.Part.Piper), TargetLabel = "Wisp",
            }.Step("Chase Wisp up the clouds and bring him home", piper));

            if (Seen("talk_bride") || Level1Director.GiftCount() > 0)
            {
                var e = new QuestEntry
                {
                    Id = "gifts", Type = QuestEntry.Kind.Side, Title = "Lost Wedding Presents", Giver = "Lady Lacewing",
                    Done = Level1Director.GiftCount() >= Level1Director.GiftIds.Length,
                    Summary = "Five wedding presents were sent this morning, and not one has arrived. Every one found makes the day a little grander.",
                    Target = NearestGift, TargetLabel = "A wedding present",
                };
                for (int i = 0; i < Level1Director.GiftIds.Length; i++)
                {
                    bool got = GameState.IsCollected(Level1Director.GiftIds[i]);
                    string name = Level1Director.GiftNames[i];
                    e.Step(char.ToUpper(name[0]) + name.Substring(1) + (got ? "" : "   " + Level1Director.GiftHints[i]), got);
                }
                log.Add(e);
            }

            if (level != null)
            {
                int buttons = level.CollectedCount(), allButtons = level.CollectibleIds.Count;
                if (allButtons > 0)
                    log.Add(new QuestEntry
                    {
                        Id = "buttons", Type = QuestEntry.Kind.Collection, Title = "Golden Buttons", Giver = "All over Toybox Town",
                        Summary = "Shiny golden buttons hidden round the toybox. Find them all for the Golden Crown.",
                        Done = buttons >= allButtons,
                    }.Step(buttons + " of " + allButtons + " found", buttons >= allButtons));
                if (level.HeartstringIds.Count > 0)
                    log.Add(new QuestEntry
                    {
                        Id = "hearts", Type = QuestEntry.Kind.Collection, Title = "Heartstrings", Giver = "Toybox Town",
                        Summary = "Golden threads that hold the town together. Bonus finds here.",
                        Done = level.HeartstringCount() >= level.HeartstringIds.Count,
                    }.Step(level.HeartstringCount() + " of " + level.HeartstringIds.Count + " found", level.HeartstringCount() >= level.HeartstringIds.Count));
                if (level.SecretIds.Count > 0)
                    log.Add(new QuestEntry
                    {
                        Id = "marbles", Type = QuestEntry.Kind.Collection, Title = "Secret Marbles", Giver = "Tucked away",
                        Summary = "Glass marbles hidden in the places people don't usually look.",
                        Done = level.SecretCount() >= level.SecretIds.Count,
                    }.Step(level.SecretCount() + " of " + level.SecretIds.Count + " found", level.SecretCount() >= level.SecretIds.Count));
            }
        }

        Vector3? PieceAt(BandPiece.Part kind)
        {
            if (pieces == null) return null;
            foreach (var p in pieces) if (p != null && p.Kind == kind && !Level1Director.Has(p.Id)) return p.transform.position;
            return null;
        }

        static Vector3? NearestGift()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) return null;
            Vector3 from = game.Player.transform.position;
            Vector3? best = null;
            float bestD = float.MaxValue;
            foreach (var g in FindObjectsByType<WeddingPresent>(FindObjectsInactive.Exclude))
            {
                if (g == null || GameState.IsCollected(Level1Director.GiftIds[g.Index])) continue;
                float d = (g.transform.position - from).sqrMagnitude;
                if (d < bestD) { bestD = d; best = g.transform.position; }
            }
            return best;
        }

        void Update()
        {
            tick -= Time.deltaTime;
            if (tick > 0f) return;
            tick = 0.3f;
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) return;
            string obj = Objective();
            if (lastObjective != null && obj != lastObjective && game.Mode == UIMode.Gameplay) QuestGuide.NextToast(game, obj);
            lastObjective = obj;
        }
    }
}

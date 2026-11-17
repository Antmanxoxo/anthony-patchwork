using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Keeping the story clear: what each lens still needs, where to go next (for the pointer
    // on screen and the panel), who has news (a gold "!" over their heads), and what everybody
    // says as Patch goes by.
    public partial class Level2Quest
    {
        readonly Vector3?[] lensDropped = new Vector3?[3];
        static readonly string[] LensHomes = { "the Hall of Arms", "the keepers' tanks", "the Helmet Crab" };
        static readonly Vector3 HallDoor = new Vector3(80f, 0f, 1f), KeepersRoom = new Vector3(-60f, 0f, 86f), LiftDoor = new Vector3(48f, 0f, 10f);
        string lastObjective;
        ToyNpc wickNpc, stellaNpc;

        // Where the work on one lens is right now.
        Vector3 LensGoal(int i)
        {
            if (lensDropped[i].HasValue) return lensDropped[i].Value;
            switch (i)
            {
                case 0:
                    return Fight != null && Fight.Busy ? new Vector3(80f, 0f, -20f) : HallDoor;
                case 1:
                    for (int c = 0; c < 4 && ClotSpots != null && c < ClotSpots.Length; c++)
                        if (!GameState.IsHintSeen(ClotKey(c))) return ClotSpots[c];
                    return KeepersRoom;
                default:
                    if (Crab != null && Crab.gameObject.activeInHierarchy) return Crab.transform.position;
                    return CrabSpots != null && CrabSpots.Length > 2 ? CrabSpots[CrabStage] : new Vector3(0f, 0f, -80f);
            }
        }

        // The lens to go for next: the nearest one still to win (whichever you've started on,
        // if you're in the middle of one).
        int NextLens()
        {
            if (Fight != null && Fight.Busy && !HasLens(0)) return 0;
            var game = PatchworkGame.Instance;
            Vector3 at = game != null && game.Player != null ? game.Player.transform.position : Vector3.zero;
            int best = -1;
            float bestD = float.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                if (HasLens(i)) continue;
                // A lens lying on the floor waiting to be picked up always comes first.
                float d = lensDropped[i].HasValue ? -1f : (LensGoal(i) - at).sqrMagnitude;
                if (d < bestD) { bestD = d; best = i; }
            }
            return best;
        }

        // One line for each lens on the panel.
        public string LensStatus(int i)
        {
            if (HasLens(i)) return "Found";
            if (lensDropped[i].HasValue) return "Pick it up!";
            switch (i)
            {
                case 0:
                    if (Fight != null && Fight.Busy) return "Fight! Wave " + Fight.WaveNumber + " of 3";
                    return "Hall of Arms (Sunken Ruins)";
                case 1:
                    return "Keepers' tanks: clots " + ClotsCleared + "/4";
                default:
                    if (Crab != null && Crab.Guarded) return "Chessmen guard the Crab";
                    return "Helmet Crab, in " + CrabPlaces[CrabStage];
            }
        }

        // Where the pointer on screen should point, and what to call it.
        public bool ObjectiveTarget(out Vector3 at, out string label)
        {
            at = Vector3.zero;
            label = null;
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null || GameState.IsLevelComplete(Level2Builder.SceneName)) return false;
            bool upstairs = game.Player.transform.position.y > 10f;
            if (Defended || Fitted)
            {
                if (!upstairs) { at = LiftDoor; label = "The lift"; return true; }
                if (Defended && !DialsSet && !RiddleGiven)
                {
                    if (stellaNpc == null) stellaNpc = FindNpc("Stella");
                    at = stellaNpc != null ? stellaNpc.transform.position : ObservatoryCentre;
                    label = "Stella";
                    return true;
                }
                if (Defended && !DialsSet) { at = DialsSpot; label = "The star dials"; return true; }
                at = ObservatoryCentre + new Vector3(0f, 1f, -2.4f);
                label = Defended ? "The Night Glass" : "The Great Seam";
                return true;
            }
            if (LensCount >= 3)
            {
                if (!upstairs) { at = LiftDoor; label = "The lift"; return true; }
                if (stellaNpc == null) stellaNpc = FindNpc("Stella");
                at = stellaNpc != null ? stellaNpc.transform.position : ObservatoryCentre;
                label = "Stella";
                return true;
            }
            if (!Met && LensCount == 0)
            {
                if (wickNpc == null) wickNpc = FindNpc("Wick");
                at = wickNpc != null ? wickNpc.transform.position : new Vector3(11f, 0f, -106f);
                label = "Wick";
                return true;
            }
            int next = NextLens();
            if (next < 0) return false;
            at = LensGoal(next);
            label = lensDropped[next].HasValue ? LensNames[next] : next == 0 ? "Hall of Arms" : next == 1 ? (ClotsCleared >= 4 ? "Keepers' tanks" : TankNameFor(at)) : "Helmet Crab";
            return true;
        }

        string TankNameFor(Vector3 at)
        {
            if (ClotSpots == null || TankNames == null) return "Keepers' tanks";
            for (int i = 0; i < ClotSpots.Length && i < TankNames.Length; i++)
                if ((ClotSpots[i] - at).sqrMagnitude < 1f) return TankNames[i];
            return "Keepers' tanks";
        }

        static ToyNpc FindNpc(string name)
        {
            foreach (var n in FindObjectsByType<ToyNpc>(FindObjectsInactive.Exclude))
                if (n != null && n.Name == name) return n;
            return null;
        }

        // The Heartstrings (and the lift they open) only come into it once somebody has said
        // what they're for: Wick does, or you find one first.
        public bool LiftKnown { get { return Seen("liftknown"); } }

        void CheckLift(PatchworkGame game)
        {
            if (LiftKnown || game.Mode != UIMode.Gameplay || Narrator.Speaking) return;
            bool found = game.Level != null && game.Level.HeartstringCount() > 0;
            if (!Met && !found) return;
            Mark("liftknown");
            Narrator.Say(found && !Met
                ? "A Heartstring: one of the golden threads the museum keeps in its big halls. The old lift up to the Observatory won't move without three of them. Hoo. Worth remembering."
                : "About that lift: it's the only way up to the Observatory, and it's old and particular. It won't move an inch without three Heartstrings, the golden threads hidden in the museum's big halls.");
        }

        // Tells you when the next thing to do changes (so you never wonder what's next).
        void CheckObjective(PatchworkGame game)
        {
            CheckLift(game);
            string obj = Objective();
            if (lastObjective != null && obj != lastObjective && game.Mode == UIMode.Gameplay)
                game.ShowToast("Next: " + obj);
            lastObjective = obj;
        }

        // ------------------------------------------------------------------ News and remarks

        void WireGuide()
        {
            // The gold "!" for news: Wick when there's a lens he hasn't heard about, Stella when
            // there's work for her, and the people with a job for you until you've asked them.
            News("Wick", () => !Met || LensCount > 0 && !Seen("wickheard" + LensCount), () => Mark("wickheard" + LensCount));
            News("Stella", () => LensCount >= 3 && !Fitted || Defended && !GameState.IsLevelComplete(Level2Builder.SceneName) || !Seen("talk_stella"), () => Mark("talk_stella"));
            News("Keeper Marlow", () => !HasLens(1) && !Seen("talk_marlow"), () => Mark("talk_marlow"));
            News("The Castellan", () => !ArmsWon && !Seen("talk_castellan"), () => Mark("talk_castellan"));
            News("Professor Plume", () => !HasSecret("audit") && !Seen("talk_plume"), () => Mark("talk_plume"));
            News("Mrs. Sudsbury", () => !HasSecret("dusties") && !Seen("talk_sudsbury"), () => Mark("talk_sudsbury"));
            News("Old Barnacle", () => !HasSecret("jar") && !Seen("talk_barnacle"), () => Mark("talk_barnacle"));
            News("Professor Hoot", () => !HasSecret("hall") && !Seen("talk_hoot"), () => Mark("talk_hoot"));
            News("Ticket-master Tock", () => !Seen("talk_tock"), () => Mark("talk_tock"));

            // What people say as Patch goes by.
            Barks("Wick", () => !Met ? "Halt! ...Oh, you're not grey. Come here, come here!"
                : LensCount >= 3 ? "All three! Up to Stella with them, quick!"
                : Pick("Eleven o'clock and all's... well, eleven o'clock.", (3 - LensCount) + " lenses still out there. I'm counting.", "I'm holding the door. Nobody gets past. Except you."));
            Barks("Stella", () => Fitted && !Defended ? "Mind the seam! Shut it, shut it!"
                : LensCount >= 3 ? "Are those my lenses? Bring them here!"
                : Pick("She's due at midnight. She's never once been late.", "Shh. Listen. Can you hear her? No? Me neither. Not yet."));
            Barks("Keeper Marlow", () => HasLens(1) ? "Look at that water. Blue again. I could cry." : Pick("Four tanks, four clots. Cut the anchors first!", "Don't mind the bubbles. They're mine."));
            Barks("The Castellan", () => ArmsWon ? "All quiet in the Hall. As it should be." : Pick("The Hall stands to arms! The armour does, I mean. Not me.", "Something in there keeps saying 'halt'. It isn't me."));
            Barks("Button", () => HasSecret("king") ? "The king says hello. He doesn't. But he would." : "Have you seen a very small king? Asking for a friend.");
            Barks("Dr. Trowel", () => HasLens(2) ? "Was it crab-shaped? Write it down. Write everything down." : "If you see the crab, don't startle it. I want notes.");
            Barks("Thimble", () => Pick("Shh! I'm not here. I'm a coat.", "I've got the WHOLE museum. Except the scary bits."));
            Barks("Ticket-master Tock", () => Pick("Don't wind that little clock tower in the corner of the forecourt. Wick wound it once. Very long night.", "Tickets! ...Nobody? Right. Habit."));
            Barks("Professor Plume", () => HasSecret("audit") ? "Five fakes! I shall write a paper." : "Read the card. Then read it again.");
            Barks("Mrs. Sudsbury", () => Pick("Mind my clean floor. Well. Cleanish.", "That grey comes back worse every time. It isn't dirt."));
            Barks("Sergeant Stanchion", () => Pick("Left, right, left... who goes there? Carry on.", "Stand aside when they rock. Hit them when they fall. That is the drill."));
            Barks("Old Barnacle", () => HasSecret("jar") ? "Red as the day she was run up. Arr." : "Arr. Smells like snow somewhere. Odd, that.");
            Barks("Squire Pim", () => ArmsWon ? "Is it over? I had my eyes shut." : "I'm guarding. Very hard. From here.");
            Barks("Keeper June", () => HasSecret("torch") ? "Got my torch back! Still on, too." : "Has anyone seen a torch? Says JUNE on it.");
            Barks("Keeper Oskar", () => "Lights in the Lantern Deep. Not ours. Swim sideways.");
            Barks("Keeper Pippa", () => "Brr. The Frozen Sea's colder than usual tonight.");
            Barks("Professor Hoot", () => HasSecret("hall") ? "Hoo! Is the cake for anyone in particular?" : "Hoo! A visitor! Mind the portrait. It's crooked.");
            Barks("Pip", () => Pick("Boing! ...Sorry. Habit.", "Pop goes the... no. Not tonight."));
        }

        // Reading the museum's cards is worth something: each one read for the first time is a
        // little sparkle and a few points, and the pause card keeps the tally.
        int plaquesTotal;
        public int PlaquesTotal { get { return plaquesTotal; } }
        public int PlaquesRead { get { int n = 0; foreach (var p in plaques) if (p != null && Seen("read_" + p.Title)) n++; return n; } }
        readonly List<ExhibitPlaque> plaques = new List<ExhibitPlaque>();

        void WirePlaques()
        {
            foreach (var p in FindObjectsByType<ExhibitPlaque>(FindObjectsInactive.Include))
            {
                if (p == null) continue;
                plaques.Add(p);
                var plaque = p;
                p.Read += () =>
                {
                    if (Seen("read_" + plaque.Title)) return;
                    Mark("read_" + plaque.Title);
                    var game = PatchworkGame.Instance;
                    int n = PlaquesRead;
                    if (game != null && game.Player != null)
                    {
                        ScoreKeeper.Bonus(150, plaque.Head + Vector3.up);
                        FxBurst.Spawn(FxBurst.Kind.Sparkle, plaque.Head + Vector3.up * 0.6f, new Color(1f, 0.85f, 0.45f));
                        if (n % 5 == 0 || n == plaquesTotal) game.ShowToast("Facts read: " + n + " of " + plaquesTotal + (n == plaquesTotal ? ". Every card in the museum!" : ""));
                    }
                };
            }
            plaquesTotal = plaques.Count;
        }

        // The little Fray patrols waiting in the halls (until the Night Glass is safe).
        void SpawnPatrols()
        {
            if (PatrolSpots == null || Defended || GameState.IsLevelComplete(Level2Builder.SceneName)) return;
            for (int i = 0; i < PatrolSpots.Length; i++)
            {
                Vector3 p = PatrolSpots[i];
                if (i % 3 == 2) { AmmoniteRoller.Create(FoeRoot, p); continue; }
                Chessling.Create(FoeRoot, p, i % 2 == 0);
                Chessling.Create(FoeRoot, p + new Vector3(1.6f, 0f, 1.1f), false, i % 4 == 1);
            }
        }

        static string Pick(params string[] lines) { return lines[Random.Range(0, lines.Length)]; }

        void News(string name, System.Func<bool> has, System.Action told)
        {
            var npc = FindNpc(name);
            if (npc == null) return;
            npc.HasNews = has;
            var was = npc.TalkEnded;
            npc.TalkEnded = () => { if (was != null) was(); if (told != null) told(); };
        }

        static void Barks(string name, System.Func<string> bark)
        {
            var npc = FindNpc(name);
            if (npc != null) npc.Bark = bark;
        }
    }
}

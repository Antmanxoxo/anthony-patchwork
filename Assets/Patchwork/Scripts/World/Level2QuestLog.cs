using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The Oceanic Museum's page of the quest menu: the Night Glass and its three lenses, the
    // jobs people give you (once they've asked), and the things there are to collect.
    public partial class Level2Quest
    {
        static readonly string[] StampNames = { "The Whale-Song Gramophone (the Atrium)", "The Archimedes Screw (Deep Time)", "The Lighthouse Lens (the Kelp Galleries)", "The Ship's Bell (the Sunken Ruins)", "The Message Tube (the Keepers' Room)" };

        public void BuildLog(List<QuestEntry> log)
        {
            var game = PatchworkGame.Instance;
            var level = game != null ? game.Level : null;
            bool done = GameState.IsLevelComplete(Level2Builder.SceneName);
            bool liftOpen = level != null && level.HeartstringCount() >= level.HeartstringsNeeded;

            // ----- The story -----
            var main = new QuestEntry
            {
                Id = "main", Type = QuestEntry.Kind.Main, Title = "The Night Glass", Giver = "Wick and Stella",
                Summary = "The Great Whale passes over the museum at midnight, and the Night Glass is the only telescope that can see her. The Fray have taken it apart. Put it back together before the clock strikes twelve.",
                Done = done,
            };
            main.Step("Find Wick, the night watchman, in the Atrium", Met)
                .Step("Win the Object Glass in the Hall of Arms", HasLens(0))
                .Step("Clear the keepers' tanks for the Field Lens  (" + ClotsCleared + "/4 clots)", HasLens(1))
                .Step("Catch the Helmet Crab for the Eye Lens", HasLens(2))
                .Step("Find " + (level != null ? level.HeartstringsNeeded : 3) + " Heartstrings to wake the lift", liftOpen)
                .Step("Take the three lenses up to Stella", Fitted || Defended)
                .Step("Stitch the Great Seam shut and clear the moths", Defended)
                .Step("Hear Stella's rhyme and set the three star dials", DialsSet)
                .Step("Look through the Night Glass", done);
            log.Add(main);

            // Each lens on its own, so you can point the marker at one in particular.
            for (int i = 0; i < 3; i++)
            {
                if (!Met && LensCount == 0) break;
                int lens = i;
                var e = new QuestEntry
                {
                    Id = "lens" + i, Type = QuestEntry.Kind.Main, Title = LensNames[i], Done = HasLens(i),
                    Giver = i == 0 ? "The Castellan" : i == 1 ? "Keeper Marlow" : "Dr. Trowel",
                    Target = () => HasLens(lens) ? (Vector3?)null : LensGoal(lens),
                    TargetLabel = LensNames[i],
                };
                if (i == 0)
                {
                    e.Summary = "The Hollow Guard have shut themselves in the Hall of Arms with the Object Glass. Go in through the Sunken Ruins and beat all three waves.";
                    e.Step("Get into the Hall of Arms (through the Sunken Ruins)", ArmsWon || Fight != null && Fight.Busy || HasLens(0))
                     .Step("Beat the Hollow Guard: three waves", ArmsWon)
                     .Step("Pick up the Object Glass", HasLens(0));
                }
                else if (i == 1)
                {
                    e.Summary = "Grey clots have grown in four of the keepers' tanks. Cut each clot's three anchor knots, then break it. The Field Lens is in the last one.";
                    for (int c = 0; c < 4; c++) e.Step("Break the clot in " + TankNames[c], GameState.IsHintSeen(ClotKey(c)));
                    e.Step("Pick up the Field Lens", HasLens(1));
                }
                else
                {
                    e.Summary = "The Helmet Crab has the Eye Lens stuck in its shell and runs off through the seams. Chase it from the Atrium to the Kelp Galleries to the Sunken Ruins.";
                    e.Step("Catch the Helmet Crab  (last seen in " + CrabPlaces[Mathf.Clamp(CrabStage, 0, CrabPlaces.Length - 1)] + ")", CrabBeaten || HasLens(2))
                     .Step("Pick up the Eye Lens", HasLens(2));
                }
                log.Add(e);
            }

            // ----- Side quests (once somebody has asked) -----
            if (Seen("talk_plume") || HasSecret("audit"))
            {
                var audit = ExhibitAudit.Instance;
                int found = audit != null ? audit.Found : 0;
                log.Add(new QuestEntry
                {
                    Id = "audit", Type = QuestEntry.Kind.Side, Title = "The Curator's Audit", Giver = "Professor Plume", Done = HasSecret("audit"),
                    Summary = "Five of the twelve small exhibits on plinths are the Fray, sitting very still with cards they wrote themselves. Read each card: if it's nonsense, hit the thing once to rattle it and again to be sure.",
                    Target = () => QuestGuide.NpcAt("Professor Plume"), TargetLabel = "Professor Plume",
                }.Step("Find the five fakes  (" + found + "/" + ExhibitAudit.Fakes + ")", found >= ExhibitAudit.Fakes)
                 .Step("Tell Professor Plume", HasSecret("audit")));
            }
            if (Seen("talk_sudsbury") || HasSecret("dusties"))
                log.Add(new QuestEntry
                {
                    Id = "dusties", Type = QuestEntry.Kind.Side, Title = "The Dust Sheet Five", Giver = "Mrs. Sudsbury", Done = HasSecret("dusties"),
                    Summary = "Something keeps giggling under five dust sheets in the History hall. Lift them with a puff of wind, and catch what comes out.",
                    Target = () => new Vector3(-60f, 0f, -57f), TargetLabel = "The History hall",
                }.Step("Find whatever's under the dust sheets and bring it to Mrs. Sudsbury", HasSecret("dusties")));
            if (Seen("talk_barnacle") || HasSecret("jar"))
                log.Add(new QuestEntry
                {
                    Id = "jar", Type = QuestEntry.Kind.Side, Title = "A Jar of Red", Giver = "Old Barnacle", Done = HasSecret("jar"),
                    Summary = "Somebody snipped the red clean out of Old Barnacle's ensign. He reckons it's been put somewhere cold: colour keeps better in the cold.",
                    Target = () => QuestGuide.NpcAt("Old Barnacle"), TargetLabel = "Old Barnacle",
                }.Step("Find the missing red, somewhere cold", HasSecret("jar"))
                 .Step("Give it back to Old Barnacle", HasSecret("jar")));
            if (Seen("talk_tock") || HasSecret("passport"))
            {
                var passport = MuseumPassport.Instance;
                var e = new QuestEntry
                {
                    Id = "passport", Type = QuestEntry.Kind.Side, Title = "The Museum Passport", Giver = "Ticket-master Tock", Done = HasSecret("passport"),
                    Summary = "Five hands-on exhibits, and a stamp for every one you try. They're for touching: turn things, ring things, post things.",
                    Target = () => NearestStation(), TargetLabel = "A hands-on exhibit",
                };
                for (int i = 0; i < MuseumPassport.Stamps; i++) e.Step(StampNames[i], passport != null && passport.Has(i));
                e.Step("Show Tock the full passport", HasSecret("passport"));
                log.Add(e);
            }
            if (Seen("talk_june") || HasSecret("torch"))
                log.Add(new QuestEntry
                {
                    Id = "torch", Type = QuestEntry.Kind.Side, Title = "June's Torch", Giver = "Keeper June", Done = HasSecret("torch"),
                    Summary = "Keeper June dropped her torch in the Lantern Deep. It's waterproof, so it'll still be on: look for a light on the ledge halfway down.",
                    Target = () => QuestGuide.NpcAt("Keeper June"), TargetLabel = "Keeper June",
                }.Step("Find June's torch in the Lantern Deep and bring it back", HasSecret("torch")));
            if (Seen("talk_button") || HasSecret("king"))
                log.Add(new QuestEntry
                {
                    Id = "king", Type = QuestEntry.Kind.Side, Title = "A Very Small King", Giver = "Button", Done = HasSecret("king"),
                    Summary = "Button has lost a very small king from the chess set. He might not have been taken at all. Where would a small king hide?",
                    Target = () => QuestGuide.NpcAt("Button"), TargetLabel = "Button",
                }.Step("Find the little chess king", HasSecret("king")));
            if (Met || SeamsShut > 0)
            {
                var e = new QuestEntry
                {
                    Id = "seams", Type = QuestEntry.Kind.Side, Title = "The Five Seams", Giver = "Wick", Done = SeamsShut >= Seams,
                    Summary = "The Fray are picking their way through from the Underside at five seams round the museum. Three good thumps shuts one, and fewer things climb out after.",
                    Target = () => NearestSpot(SeamSpots, "seam"), TargetLabel = "A seam",
                };
                for (int i = 0; i < Seams; i++) e.Step("The seam in " + SeamPlaces[i], Seen("seam" + i));
                log.Add(e);
            }

            // ----- Things to collect -----
            if (level != null && level.HeartstringIds.Count > 0 && LiftKnown)
                log.Add(new QuestEntry
                {
                    Id = "hearts", Type = QuestEntry.Kind.Collection, Title = "Heartstrings", Giver = "The museum's big halls",
                    Summary = "Golden threads that hold the museum together. The old lift won't move without three of them.",
                    Done = level.HeartstringCount() >= level.HeartstringIds.Count,
                }.Step(level.HeartstringCount() + " of " + level.HeartstringIds.Count + " found", level.HeartstringCount() >= level.HeartstringIds.Count));
            if (PagesFound > 0 || Met)
                log.Add(new QuestEntry
                {
                    Id = "pages", Type = QuestEntry.Kind.Collection, Title = "The Night-Book", Giver = "Pages blown about the museum",
                    Summary = "Torn-out pages of the night keepers' book, each one a little more of the story.",
                    Done = PagesFound >= Pages,
                    Target = () => NearestSpot(PageSpots, "page"), TargetLabel = "A night-book page",
                }.Step(PagesFound + " of " + Pages + " pages found", PagesFound >= Pages));
            log.Add(new QuestEntry
            {
                Id = "secrets", Type = QuestEntry.Kind.Collection, Title = "Secrets of the Night Glass", Giver = "All over the museum",
                Summary = "Little things that happen if you look, listen, or do something nobody expected. They go on the lost property shelf by Wick's post.",
                Done = SecretsFound >= Secrets,
            }.Step(SecretsFound + " of " + Secrets + " secrets", SecretsFound >= Secrets));
            if (PlaquesTotal > 0)
                log.Add(new QuestEntry
                {
                    Id = "facts", Type = QuestEntry.Kind.Collection, Title = "Facts", Giver = "The museum's plaques",
                    Summary = "Every plaque read for the first time is worth a few points. Professor Plume swears every word is true.",
                    Done = PlaquesRead >= PlaquesTotal,
                }.Step(PlaquesRead + " of " + PlaquesTotal + " plaques read", PlaquesRead >= PlaquesTotal));
        }

        // The nearest spot in a list that hasn't been done yet ("seam0", "page3"...).
        Vector3? NearestSpot(Vector3[] spots, string key)
        {
            if (spots == null) return null;
            var game = PatchworkGame.Instance;
            Vector3 from = game != null && game.Player != null ? game.Player.transform.position : Vector3.zero;
            Vector3? best = null;
            float bestD = float.MaxValue;
            for (int i = 0; i < spots.Length; i++)
            {
                if (Seen(key + i)) continue;
                float d = (spots[i] - from).sqrMagnitude;
                if (d < bestD) { bestD = d; best = spots[i]; }
            }
            return best;
        }

        static Vector3? NearestStation()
        {
            var passport = MuseumPassport.Instance;
            var game = PatchworkGame.Instance;
            if (passport == null || passport.Stations == null || game == null || game.Player == null) return null;
            Vector3 from = game.Player.transform.position;
            Vector3? best = null;
            float bestD = float.MaxValue;
            for (int i = 0; i < passport.Stations.Length; i++)
            {
                var s = passport.Stations[i];
                if (s == null || passport.Has(i)) continue;
                float d = (s.transform.position - from).sqrMagnitude;
                if (d < bestD) { bestD = d; best = s.transform.position; }
            }
            return best;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // What everybody in the Oceanic Museum says about the Night Glass: who is given new lines
    // (WireTalk), how those lines take turns with their ordinary chatter (Says), and the lines
    // themselves. The story's rules and state are in Level2Quest.cs.
    public partial class Level2Quest
    {
        // Called once, from Start. For each person, the sources are listed most important first.
        void WireTalk()
        {
            Says("Keeper Marlow", null,
                MarlowLines);
            Says("The Castellan", null,
                CastellanLines);
            Says("Stella", StellaTalked,
                StellaLines);
            Says("Button", null,
                () => Tell("king") ? new[]
                {
                    "The king! You found the king! He wasn't taken at all, he was HIDING!",
                    "In the biscuits. Of course in the biscuits. That's where I'd go.",
                    "He says thank you. Well, he doesn't say anything, he's a chess piece. But he's stopped shutting his eyes."
                } : CrabGossip(1));
            Says("Dr. Trowel", null,
                () => Tell("crab", "trowel") ? new[]
                {
                    "You caught it in one go? Before it reached the Ruins? Oh. Well. Good. Very good.",
                    "It's only that I had a chair set up. And a notebook. I was rather looking forward to observing it."
                } : CrabGossip(2));
            Says("Thimble", null,
                () => CrabGossip(0));
            Says("Ticket-master Tock", null,
                PassportLines,
                Mood(() => Seen("seam0"),
                    "No tickets tonight. Museum's closed, on account of the floor has a hole in it and things keep coming out. Three good thumps closes it, Wick says. I am a ticket-master. I do not thump.",
                    "You stitched it! Lovely needlework. I'd give you a free ticket, but you're already in."));
            // The Curator's Audit: Professor Plume sets it, keeps count, and marks it at the end.
            Says("Professor Plume", null,
                () => Tell("larder") ? new[]
                {
                    "Jars? Labelled? With the tanks' own colours in them? Oh, that is very interesting. That is very interesting indeed.",
                    "He isn't destroying it, you see. He's COLLECTING it. One doesn't label what one means to throw away.",
                    "Which raises the question of what he wants it all for. I have a theory. I don't like my theory."
                } : null,
                () =>
                {
                    var audit = ExhibitAudit.Instance;
                    if (audit == null) return null;
                    if (Tell("connoisseur")) return new[]
                    {
                        "Five fakes found, and not one real exhibit so much as dusted in error. Do you know how rare that is?",
                        "I have a doctorate, and I hit the nautilus. Twice. In my defence it was looking at me.",
                        "A perfect eye. I shall say so in the annual report, and the annual report is read by as many as four people."
                    };
                    if (Tell("audit")) return new[]
                    {
                        "All five! The compass, the Moon, the shark, the starfish and the seahorse. Every one of them a fraud with a card it wrote itself.",
                        audit.Mistakes == 0 ? "And never a wrong guess." : "And only " + audit.Mistakes + (audit.Mistakes == 1 ? " real exhibit" : " real exhibits") + " walloped by mistake, which I shall overlook. They're sturdier than they look.",
                        "That's the trouble with the Fray, you see. They can copy the look of a thing. They cannot be bothered to find out what's true about it."
                    };
                    if (HasSecret("audit")) return null;
                    int found = audit.Found;
                    if (found == 0 && audit.Mistakes == 0) return new[]
                    {
                        "Ah. You. You have the look of somebody who reads the cards. Good. I need somebody who reads the cards.",
                        "There are twelve small exhibits on plinths about the museum: the Atrium, the History hall, the Kelp Galleries, the Sunken Ruins. I catalogued seven. I did not catalogue the other five.",
                        "Five of them are the Fray, sitting very still. And they have written their own cards, and the Fray never check their facts.",
                        "Read each card. If what it says is true, leave the thing alone. If it's nonsense, hit it: once to make it rattle, and again to say you're sure.",
                        "Be sure. Wallop a real one and the alarm goes, and it'll cost you. If you truly can't tell from the card, watch the thing itself from a little way off. A fake cannot keep perfectly still."
                    };
                    return new[]
                    {
                        found + " of the five fakes found" + (audit.Mistakes > 0 ? ", and " + audit.Mistakes + " real " + (audit.Mistakes == 1 ? "exhibit" : "exhibits") + " hit by mistake. Read the cards twice." : ". No mistakes yet. Keep reading."),
                        "Remember: once to rattle it, twice to accuse it. And a fake blinks, when it thinks you're not close."
                    };
                },
                Mood(() => false,
                    "The Great Whale? She has gone over this spot every midnight since before there was a roof on it. The museum was built round the telescope, you know, not the other way about. She sings when she's seen. Nobody knows why. I have a theory that it's simply manners.", null));
            Says("Mrs. Sudsbury", null,
                () =>
                {
                    if (Tell("dusties")) return new[]
                    {
                        "My dust bunnies! All five! I've been after those since spring cleaning.",
                        "Oh, don't look at me like that, I'm not going to throw them out. They go under the bed in the Bedroom exhibit. It doesn't look lived-in without.",
                        "Hoovered them, did you? With the wind? Well. I never thought of being the wind."
                    };
                    if (HasSecret("dusties")) return null;
                    return new[]
                    {
                        "And another thing. Something keeps giggling under the dust sheets in the History hall. Five of them.",
                        "I'm not lifting those sheets. You lift them. You'd want a good puff of wind to do it, and another to catch whatever comes out."
                    };
                },
                Mood(() => LensCount >= 2,
                    "I've been scrubbing grey off the skirting boards since six o'clock and it comes back worse. It isn't dirt. Dirt I understand.",
                    "It's lifting! Look, that's blue under there. I'd forgotten that skirting board was blue."));
            Says("Sergeant Stanchion", null,
                () => Tell("skittles") ? new[]
                {
                    "Three with one blow! I saw that. That was not in the drill. I am putting it in the drill.",
                    "Henceforth it shall be known as the Patch Manoeuvre. Dismissed. Well done. Dismissed."
                } : null,
                Mood(() => Seen("seam1"),
                    "The big spiral fossils are out of their case and they are ROLLING. Stand aside when they rock. Hit them when they fall over. That is the whole of the drill.",
                    "Deep Time is secure. The fossils are fossils again. Carry on."));
            Says("Old Barnacle", null,
                () =>
                {
                    if (Tell("jar")) return new[]
                    {
                        "What've ye got there? A jam jar? ...That's red. That's MY red. That's the red off the ensign, I'd know it anywhere, I've saluted it every morning for sixty years.",
                        "He bottled it. Like plums. Unscrew it, lad. Go on. Stand back.",
                        "...There she is. Look at that. Red as the day she was run up. Ye can keep the jar."
                    };
                    if (HasSecret("jar") || GameState.IsLevelComplete(Level2Builder.SceneName)) return null;
                    return new[] { "Scissors. I heard scissors. Snip, snip, up in the rigging, and when I looked, the red had gone clean out of the ensign. If ye find it, it'll be somewhere cold. Colour keeps better in the cold. He'd know that." };
                });
            Says("Squire Pim", null,
                Mood(() => ArmsWon,
                    "I'm not hiding. I'm guarding this corner. From behind this plinth.",
                    "Did you see the Captain call the others back up? I did. I had my eyes shut, but I saw it."));
            Says("Keeper June", null,
                () => Tell("torch") ? new[]
                {
                    "My torch! It's still ON. That's a week of batteries, that is.",
                    "I put it down to tie a knot and the knot took longer than I thought. They always do. Thank you. I'll write my name on it bigger."
                } : null,
                Mood(() => HasLens(1),
                    "From up here you can see the ropes. Three to a knot, going off toward the corners. Cut those first, or you'll never so much as dent it.", null));
            Says("Keeper Oskar", null,
                Mood(() => HasLens(1),
                    "Mind the Lantern Deep. Something down there has a light on, and it isn't one of ours. When the light flares, swim sideways.", null));
            Says("Keeper Pippa", null,
                Mood(() => HasLens(1),
                    "The Frozen Sea's got one too. I saw teeth. I've never been so glad to be on this side of the glass.", null));
            // The two Easter eggs: a hint from whoever's nearest, and what they say afterwards.
            Says("Professor Hoot", null,
                Mood(() => SeamsShut >= Seams,
                    "The seams? The Fray lives on the back of everything. The Underside, where all the knots are. A seam is where it has picked its way through to the front. Stitch it and it has to find another way round.",
                    "Five seams shut. Do you know, I can hear the building settle. Like a quilt when you've finished the border."),
                () => Tell("tidy", "hoot") ? new[]
                {
                    "All five seams, before a single lens? Most people run after the shiny thing first. You mended the hole in the bag before you picked up the marbles.",
                    "I shall put that in the lecture. I don't have a lecture. I shall write a lecture and put that in it."
                } : null,
                () =>
                {
                    if (Tell("hall", "hoot")) return new[]
                    {
                        "Hoo! There you are! You walked into that wall nine times. I counted. I didn't like to say anything.",
                        "And there's a coat stand in my gallery now, in a party hat, with a cake. I shall say nothing about that either.",
                        "...Is the cake for anyone in particular?"
                    };
                    if (HasSecret("hall")) return null;
                    return new[]
                    {
                        "One thing. The Founder's portrait, between the windows. It hangs crooked. Leave it crooked.",
                        "I have straightened it twice. Each time I turned my back, crooked again. I have never straightened it a third time, and I am not going to tell you why, because I don't know why, and that is worse."
                    };
                });
        }

        // Gives somebody in the museum new things to say about the quest. Each source is asked in
        // turn; the first with something they haven't already said gets said (once), and when
        // none has anything new they go back to their own chatter. "talked" (if any) runs
        // whenever a conversation with them ends.
        void Says(string name, System.Action talked, params System.Func<string[]>[] sources)
        {
            ToyNpc npc = null;
            foreach (var n in FindObjectsByType<ToyNpc>(FindObjectsInactive.Exclude))
                if (n != null && n.Name == name) { npc = n; break; }
            if (npc == null) return;
            var old = npc.Lines;
            var heard = new string[sources.Length];
            npc.Lines = () =>
            {
                for (int i = 0; i < sources.Length; i++)
                {
                    var q = sources[i]();
                    if (q == null || q.Length == 0 || heard[i] == q[0]) continue;
                    heard[i] = q[0];
                    return q;
                }
                return old != null ? old() : new[] { "..." };
            };
            if (talked == null) return;
            var was = npc.TalkEnded;
            npc.TalkEnded = () => { if (was != null) was(); talked(); };
        }

        // A source for somebody who isn't part of the quest but can see it going on: one thing
        // to say while the trouble lasts, another (if any) once it's over.
        static System.Func<string[]> Mood(System.Func<bool> over, string during, string after)
        {
            return () =>
            {
                if (GameState.IsLevelComplete(Level2Builder.SceneName)) return null;
                string line = over() ? after : during;
                return line == null ? null : new[] { line };
            };
        }

        public void WickTalked()
        {
            if (Met) return;
            Mark("met");
            Narrator.Card("Chapter Two", "The Night Glass");
            Sfx.Play(SfxId.Checkpoint, 0.4f);
        }

        public string[] WickLines()
        {
            if (!Met && LensCount > 0) return new[]
            {
                "Halt! Who goes... is that... that's one of the LENSES. Where did you... who ARE you?",
                "No, don't tell me. I don't want to know. You walked in off the street and started putting my museum right without so much as a how-d'you-do. Forty years a watchman. Nobody tells me anything.",
                "Wick. Night watchman. There are three of those, they go in the telescope at the top of the house, and they need to be there by midnight or the Great Whale goes over unsung and we all go grey. But I expect you'd worked that out.",
                "Stella's up in the Observatory. The only way up is the old lift at the far end of the Sunken Ruins, and it won't budge without three Heartstrings: the golden threads hidden in the big halls. Pick them up as you go.",
                "Carry on, then. As you were. Whoever you are."
            };
            if (!Met) return new[]
            {
                "Halt! Who goes... oh. You're not grey. Thank the stars. Wick, night watchman. Forty years and I never lost so much as a teaspoon, until tonight.",
                "You know what we do here at midnight? We point the Night Glass, the great telescope, up through the dome, and we find the Great Whale as she goes over.",
                "She only sings if somebody sees her. And her song is what keeps the colour in this place. Every tank, every tapestry. Every night, for longer than the building's stood.",
                "At dusk the Fray came up through the floor and took the Night Glass's three lenses. Unscrewed them, neat as you like. If midnight goes by and nobody's looking, she goes over unsung, and by morning...",
                "Well. Look at the suits of armour. Look at the tanks. It's started.",
                "One lens went into the Hall of Arms, in the Sunken Ruins: I heard the clanking. One's somewhere in the keepers' tanks: ask Marlow, through the door behind the big aquarium. And one...",
                "One I saw carried off by a diving helmet. On legs. I am not a fanciful man.",
                "You've a good arm on you. Three quick hits, and make the third one count: it's the only thing that shifts a shield.",
                "Bring the lenses up to Stella in the Observatory. The only way up is the old lift at the far end of the Sunken Ruins, and it won't budge without three Heartstrings: golden threads, hidden in the big halls. Pick them up as you go. I'll hold the door."
            };
            int n = LensCount;
            if (Tell("thimble", "wick")) return new[]
            {
                "You HIT him? The tall one? With the thimble?",
                "Forty years I've walked these halls and I never so much as raised my voice to a burglar. And you knocked his hat off.",
                "I'd like it for the lost property cupboard, when you've done with it. Just to know it's in there."
            };
            if (Defended) return new[] { "It's ready? Then what are you doing down here talking to me? Go and look, before she's gone over!" };
            if (n >= 3) return new[] { "All three! Up to Stella with them, quick as you can. The lift's at the far end of the Sunken Ruins." };
            var lines = new List<string>();
            // Things he's heard about that he wasn't expecting to.
            if (Tell("rounds")) return new[]
            {
                "Every lamp on my round. Every one. I could see them coming on from here, one after another, like somebody walking home.",
                "That's my job you've done. I ought to mind. I find I don't.",
                "Have a look at the shelf behind me, by the by. Lost property. Things keep turning up on it tonight, and I don't put them there."
            };
            if (Lamps != null && LampsLit > 0 && LampsLit < LampsTotal && !Seen("lamps_said")) { Mark("lamps_said"); lines.Add("Somebody's been lighting my lamps. There's eight on the round, and they all went out at dusk. Every one you turn up, the grey backs off a bit. I'd do it myself, but I'm holding this door."); }
            if (Tell("flawless", "wick")) lines.Add("The Castellan says you went through the whole Hollow Guard and they never laid a gauntlet on you. He's told me four times. He's started acting it out.");
            if (Tell("crab")) lines.Add("You caught the helmet before it could run? Flat on its back, first go? I chased that thing round the Atrium for an hour. I'm not jealous. I'm a little jealous.");
            if (Tell("tidy", "wick")) lines.Add("All five seams stitched, and not a lens in your pocket yet. You're a tidy sort. Shut the doors before you chase the burglars. My mother was the same.");
            int pages = PagesFound;
            if (pages >= Pages && !Seen("pages_told"))
            {
                Mark("pages_told");
                return new[]
                {
                    "My night book! Every page! I thought the draught had them for good.",
                    "Read me the last one. ...A thimble for a hat. Yes. I remember now. I'd made myself forget.",
                    "The toys down in Toybox Town have a name for him. The Unravelled King. The Fray goes where he points, and the colour it takes ends up in his jars.",
                    "He bowed to me. Forty years a watchman and the thief bowed to me. You get those lenses back, and you look through that glass, and you let him watch you do it."
                };
            }
            int shut = SeamsShut;
            if (shut < Seams) lines.Add(shut == 0 ? "And mind the seams. Rips in the floor, five of them: that's how they came up. Things keep climbing out till somebody stitches them shut. Three hits does it." : shut + " of the five seams stitched. Every one you shut is one less way in for them, and it'll tell at the end, mark my words.");
            if (pages > 0 && pages < Pages) lines.Add("You've found " + pages + " of the six pages out of my night book. Keep an eye out for the rest; I wrote down everything I saw, and I can't for the life of me remember the end of it.");
            lines.Add(n == 0 ? "No lenses yet? No shame in it. It's a big museum and they've a head start." : n == 1 ? "One lens back. Two to find. You're doing better than I did." : "Two! One more and she can see again.");
            if (!HasLens(0)) lines.Add(ArmsWon ? "The Hall of Arms has gone quiet. Whatever the Captain dropped is still lying in there." : "The Hall of Arms is through the Sunken Ruins, south side. Mind the door. I don't like the look of that portcullis.");
            if (!HasLens(1)) lines.Add("The tanks are behind the big aquarium: the door marked KEEPERS ONLY. You'll want to be Water to go in. Marlow's count is " + ClotsCleared + " of 4 clear.");
            if (!HasLens(2)) lines.Add("That helmet with legs was last seen in " + CrabPlaces[CrabStage] + ". Nothing dents it but a really good wallop, the third of three. Then it's on its back, and it's yours.");
            return lines.ToArray();
        }

        // Tock and the Museum Passport: five hands-on machines, a stamp for each.
        string[] PassportLines()
        {
            var passport = MuseumPassport.Instance;
            if (passport == null) return null;
            if (Tell("passport")) return new[]
            {
                "Five stamps! A full passport! Do you know, in all my years, nobody has ever filled one in?",
                "Mostly because I only had the idea this evening. But still. First ever.",
                "Here: the museum's honorary membership, which entitles you to come in whenever you like, free, for ever. ...You already do that. Well. Now it's official."
            };
            if (HasSecret("passport")) return null;
            int n = passport.Count;
            if (n == 0) return new[]
            {
                "Tickets? No. But! I do have this. A Museum Passport. Five hands-on exhibits, a stamp for every one you try.",
                "The whale-song gramophone in here, the screw in Deep Time, the lighthouse lens in the Kelp Galleries, the ship's bell in the Ruins, and the message tube in the Keepers' Room.",
                "They're for touching. That's the whole point of them. Most people are too shy."
            };
            return new[] { n + " of five stamps. " + (n >= 4 ? "Just the one to go!" : "Have you tried them all? Turn things. Ring things. Post things.") };
        }

        string[] MarlowLines()
        {
            if (Tell("whistle")) return new[]
            {
                "My whistle! Where on earth... the cave in the Coral Nursery? That's where I dropped it. Nineteen years ago. I was feeding the wrasse.",
                "I've been betting it ever since. \"I'd bet my whistle.\" I never had a whistle to bet. Don't tell June.",
                "Listen. ...Still works. The fish used to come to that. I wonder if they remember."
            };
            if (HasLens(1)) return null;
            int n = ClotsCleared;
            if (n >= 4) return new[] { "Clear! All four! And there's something shining in the water where the last knot was. Go on, it's yours." };
            if (n == 0) return new[]
            {
                "You've seen them? The tanks? Forty years of my life in those tanks, and tonight they look like washing-up water.",
                "It's the Fray. There's a great grey knot tied round the heart of each one, drinking the colour out. Coral Nursery, Lantern Deep, Frozen Sea, Mangrove Shallows. All four.",
                "I can't go in. I'm a bird; I'd never get back out. But you can be Water, can't you? Go in from the catwalk and dive.",
                "Hit the knot. It'll swell up before it lets off a shock: you'll see it. Back away, then go in again. And mind the jellies. They used to be such gentle things.",
                "And look: one of those knots is wrapped round something that glints. I saw it. I'd bet my whistle it's off the telescope."
            };
            return new[] { n + " of four tanks clear. I can see the colour coming back from here. Keep going: the one with the glint in it is still knotted up." };
        }

        string[] CastellanLines()
        {
            if (Tell("flawless", "castellan")) return new[]
            {
                "Not a scratch. Not ONE. I watched from behind the cabinet. Eleven armed opponents, counting the two that got up again, and you came out the far end without a thread out of place.",
                "I have been Castellan of this hall for thirty years and I have never seen it done. I have never seen it attempted. Strictly speaking there has never been any call.",
                "I shall have a small brass plate made. \"On this spot.\" It will not say what happened on this spot. People will have to ask."
            };
            if (HasLens(0)) return new[] { "They're back on their plinths. A little dented. I shall be up all night with the polish, and I shan't mind one bit. Thank you." };
            if (ArmsWon) return new[] { "It's over? It's over. He dropped something, the Captain. Round and blue. It's lying in the aisle; I didn't dare touch it." };
            return new[]
            {
                "Stay back from the aisle! They've been turning their heads all evening. Not the way they're meant to. All the way round.",
                "There's grey thread coming out of the gauntlets. And the Knight by the door has something on his chest that he didn't have this morning. Round. Blue. Glass.",
                "If you go down there the portcullis will drop. It always did have a mind of its own. Three quick blows, mind, and the third one's the one that moves a shield."
            };
        }

        bool riddleTurn;

        string[] StellaLines()
        {
            if (Defended && Tell("nomoth")) return new[]
            {
                "Not one. Not one moth got out. You had that seam shut before it could so much as cough.",
                "I had a broom ready and everything. I feel a bit silly holding it now. Go on. Go and look."
            };
            if (Tell("thimble", "stella")) return new[]
            {
                "Is that HIS? The thimble? You knocked it off his head?",
                "Put it under the telescope. No, really. It's exactly the right height for a footstool, and I have wanted a footstool up here for years."
            };
            if (Defended && !DialsSet)
            {
                // The rhyme for the star dials (two ways of telling it, so she'll always say it again).
                Mark("riddle");
                riddleTurn = !riddleTurn;
                if (riddleTurn) return new[]
                {
                    "Every lens where it belongs. But she's not in the glass yet: it's pointing at empty sea. The Fray spun the star dials.",
                    "There are three dials on the console behind the telescope, and every keeper sets them by the old rhyme:",
                    "\"Moons of the giant, as Galileo saw them. The year the comet kept its promise. And the stars of the first catalogue, in hundreds.\"",
                    "My memory is all stars and no numbers, I'm afraid. But the answers are written on the museum's own cards: the telescope's, one in the Comet Gallery, and one in the Night Sky Gallery. I've put a gold star over each."
                };
                return new[]
                {
                    "The rhyme again? \"Moons of the giant, as Galileo saw them. The year the comet kept its promise. The stars of the first catalogue, in hundreds.\"",
                    "The telescope's own card, the Comet Gallery, the Night Sky Gallery. Read them, then turn the dials. Gold stars over all three."
                };
            }
            if (Defended) return new[] { "She's in the glass. Every lens where it belongs, every dial right. Go on, up the steps. You earned the first look." };
            if (Fitted) return new[] { "The floor! It's come open! Stitch it, stitch it shut, or the moths will have the lenses out again!" };
            int n = LensCount;
            if (n >= 3) return new[]
            {
                "You have them? All three? Oh, hold them up. Hold them up to the light.",
                "The Object Glass at the top. The Field Lens in the middle. The Eye Lens where your eye goes. There. There. And... there.",
                "Listen. Do you hear that? That's a telescope that can see."
            };
            if (n == 0) return new[]
            {
                "Don't look through it. There's nothing to see. They took the lenses, all three, while I was winding the dome.",
                "It's a tube without them. A very expensive tube, pointing at a whale it can't see. And it's nearly midnight.",
                "The watchman's downstairs in the Atrium, if you want to help. Wick. He saw where they went."
            };
            return new[] { n == 1 ? "One lens! One's not enough to see by, but it's one more than I had. Two to go." : "Two! Just one more. Hurry: I can feel her getting close. The whole dome hums when she's close." };
        }

        string[] CrabGossip(int place)
        {
            if (HasLens(2) || CrabBeaten) return null;
            int stage = CrabStage;
            if (place == 0)
            {
                if (stage == 0) return new[] { "Did you see it? A helmet! With legs! It's been sitting in the middle of the Atrium all evening, looking at people. It runs if you get close." };
                return new[] { "The helmet thing went that way. Through the floor. I'm not making it up." };
            }
            if (place == 1)
            {
                if (stage == 1) return new[] { "There's a brass helmet in the Kelp Galleries and it's got the chess pieces with it. They hop. I don't like the hopping." };
                if (stage == 2) return new[] { "It went through a seam in the carpet. Toward the Sunken Ruins, I think. It can't have many places left." };
                return null;
            }
            if (stage == 2) return new[]
            {
                "Fascinating specimen. A standard diving helmet, brass, about 1900, and six legs, which is not standard.",
                "It's stopped running. I think it's run out of museum. When it rears up, it means to charge. Step aside, let it tire itself, then hit it properly."
            };
            return null;
        }
    }
}

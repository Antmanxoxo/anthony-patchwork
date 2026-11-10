using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Toybox Town's three band stories, each with a twist of its own, and the lost wedding
    // presents hidden round the map. (The moving parts are in QuestBits.cs; how the wedding uses
    // them is in Level1Director.)
    //
    //   The lute    - a pirate on Spindle Hill is sitting on the string. He thinks "lute" is
    //                 "loot", and wants his own treasure back first: buried in the sandpit, by
    //                 his wrecked toy boat, right across the map.
    //   The trumpet - valve dew only gathers while all three dye taps run: two inside the Dye
    //                 Works, and the intake down at the waterline in the canal wall.
    //   The piper   - Wisp is so jumpy that he bolts to the next cloud up whenever Patch gets
    //                 near. Chase him up a staircase of clouds until he runs out of sky.
    public static partial class Level1Builder
    {
        static readonly Dictionary<string, BandPiece> bandPieces = new Dictionary<string, BandPiece>();
        static Spinner millWheel;
        static Vector3 sandpitAt;
        static Vector3 cloudGiftAt;

        static void Quests(LevelKit kit)
        {
            director.Info = kit.Info;
            PirateAndTreasure(kit);
            ScaredWisp(kit);
            DyeTaps(kit);
            WeddingPresents(kit);
            WeddingExtras(kit);
            director.ApplyExtras();
        }

        static BandPiece Piece(string id)
        {
            BandPiece p;
            return bandPieces.TryGetValue(id, out p) ? p : null;
        }

        // What Lady Lacewing has to say about the lost presents (how many, and where to look next).
        static string[] WithPresents(string[] lines, bool married)
        {
            var all = new List<string>(lines);
            int n = Level1Director.GiftCount(), total = Level1Director.GiftIds.Length;
            if (n >= total)
            {
                all.Add(married ? "And every one of our lost presents found! You do spoil us, dear." : "And all five of our lost presents found already! It's going to be quite a day.");
                return all.ToArray();
            }
            if (married) all.Add(n == 0 ? "Our five wedding presents never did arrive, you know. Late ones are still welcome!" : "We've had " + n + " of our five lost presents now. Late ones are still welcome!");
            else all.Add(n == 0 ? "Five wedding presents were sent, and not one has arrived. Lost all over the toybox, no doubt." : n + " of our five lost presents have turned up. So kind! Every one makes the day a little grander.");
            for (int i = 0; i < total; i++)
                if (!GameState.IsCollected(Level1Director.GiftIds[i])) { all.Add(Level1Director.GiftHints[i]); break; }
            return all.ToArray();
        }

        // ================================================================ The lute: loot!

        static void PirateAndTreasure(LevelKit kit)
        {
            var piece = Piece(Level1Director.LuteId);
            var go = new GameObject("PirateQuest");
            go.transform.SetParent(kit.Root, false);
            var quest = go.AddComponent<PirateQuest>();
            quest.Info = kit.Info;
            quest.Piece = piece;
            if (piece != null)
            {
                // The coil's in plain sight, but it's his until he's paid.
                piece.HideWhenLocked = false;
                piece.LockedToast = "That's Captain Buttonbeard's \"loot\". Better have a word with him first.";
            }

            // The Captain, on the summit beside the old spinning wheel, with his flag planted.
            var pirate = Toy(kit, new Vector3(-64.4f, 4.02f, 32.4f), 200f, ToyNpc.Kind.Pirate, "Captain Buttonbeard",
                new Color(0.2f, 0.3f, 0.5f), new Color(0.85f, 0.25f, 0.3f), ToyNpc.Instrument.None, 0.62f, new Color(0.2f, 0.28f, 0.45f));
            pirate.Lines = quest.Lines;
            pirate.TalkEnded = quest.TalkEnded;
            quest.Pirate = pirate;
            Vector3 flag = new Vector3(-61.6f, 4f, 34.6f);
            var pole = PatchworkAssets.Wood(new Color(0.6f, 0.42f, 0.28f));
            kit.Deco(PrimitiveType.Cylinder, flag + Vector3.up * 1.6f, new Vector3(0.09f, 1.6f, 0.09f), pole);
            kit.Deco(PrimitiveType.Sphere, flag + Vector3.up * 3.25f, Vector3.one * 0.2f, PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.75f));
            var cloth = kit.Group("PirateFlag", flag + Vector3.up * 2.75f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.6f, 0f, 0f), new Vector3(1.2f, 0.8f, 0.03f), PatchworkAssets.Felt(new Color(0.13f, 0.11f, 0.15f)), default(Vector3), cloth);
            // (A button for a skull, and two crossed needles.)
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.6f, 0.08f, 0.025f), new Vector3(0.32f, 0.012f, 0.32f), PatchworkAssets.Felt(new Color(0.97f, 0.93f, 0.85f)), new Vector3(90f, 0f, 0f), cloth);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.6f, 0.08f, -0.025f), new Vector3(0.32f, 0.012f, 0.32f), PatchworkAssets.Felt(new Color(0.97f, 0.93f, 0.85f)), new Vector3(90f, 0f, 0f), cloth);
            for (int s = -1; s <= 1; s += 2)
                foreach (float side in new[] { 0.03f, -0.03f })
                    kit.Deco(PrimitiveType.Cube, new Vector3(0.6f, -0.18f, side), new Vector3(0.62f, 0.05f, 0.012f), PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.9f), 0.8f), new Vector3(0f, 0f, s * 28f), cloth);
            var wave = cloth.gameObject.AddComponent<Rocker>();
            wave.Axis = Vector3.up;
            wave.Amount = 14f;
            wave.Speed = 2.2f;

            // The treasure: under a stitched red X in the sandpit, beside the wrecked toy boat.
            Vector3 x = sandpitAt + new Vector3(-1.8f, 0.2f, -1.9f);
            quest.TreasureAt = x;
            var red = PatchworkAssets.Felt(new Color(0.86f, 0.2f, 0.22f));
            kit.Deco(PrimitiveType.Cube, x + Vector3.up * 0.015f, new Vector3(1.5f, 0.03f, 0.22f), red, new Vector3(0f, 45f, 0f));
            kit.Deco(PrimitiveType.Cube, x + Vector3.up * 0.02f, new Vector3(1.5f, 0.03f, 0.22f), red, new Vector3(0f, -45f, 0f));
            var dig = DigSpot.Place(kit, x, null);
            dig.OnDug = quest.Dug;
            quest.Dig = dig;

            var chest = kit.Group("TreasureChest", x);
            chest.rotation = Quaternion.Euler(0f, 25f, 0f);
            var planks = PatchworkAssets.Planks(new Color(0.55f, 0.36f, 0.24f));
            var gold = PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.3f, 0f), new Vector3(0.95f, 0.6f, 0.62f), planks, default(Vector3), chest);
            foreach (float bx in new[] { -0.3f, 0.3f })
                kit.Deco(PrimitiveType.Cube, new Vector3(bx, 0.3f, 0f), new Vector3(0.09f, 0.62f, 0.64f), gold, default(Vector3), chest);
            var lid = new GameObject("Lid").transform;
            lid.SetParent(chest, false);
            lid.localPosition = new Vector3(0f, 0.6f, -0.31f);   // hinged along the back edge
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.1f, 0.31f), new Vector3(0.95f, 0.2f, 0.62f), planks, default(Vector3), lid);
            foreach (float bx in new[] { -0.3f, 0.3f })
                kit.Deco(PrimitiveType.Cube, new Vector3(bx, 0.1f, 0.31f), new Vector3(0.09f, 0.22f, 0.64f), gold, default(Vector3), lid);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.02f, 0.63f), new Vector3(0.16f, 0.2f, 0.05f), gold, default(Vector3), lid);
            // Inside: gold buttons heaped up, glowing.
            var coins = new GameObject("Coins").transform;
            coins.SetParent(chest, false);
            coins.localPosition = new Vector3(0f, 0.56f, 0f);
            var shine = PatchworkAssets.Glow(new Color(1f, 0.82f, 0.3f), 0.7f);
            var rng = new System.Random(11);
            for (int i = 0; i < 14; i++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3((float)rng.NextDouble() * 0.7f - 0.35f, (float)rng.NextDouble() * 0.12f, (float)rng.NextDouble() * 0.4f - 0.2f),
                    new Vector3(0.16f, 0.014f, 0.16f), i % 3 == 0 ? shine : gold, new Vector3((float)rng.NextDouble() * 50f - 25f, (float)rng.NextDouble() * 180f, (float)rng.NextDouble() * 50f - 25f), coins);
            quest.Chest = chest;
            quest.Lid = lid;
            quest.Coins = coins;
        }

        // ================================================================ The piper: up the clouds

        static void ScaredWisp(LevelKit kit)
        {
            var piece = Piece(Level1Director.PiperId);
            var bird = director.StrandedPiper;
            // A staircase of clouds climbing away from the weathervane spire (its top is at 16).
            Vector3[] clouds = { new Vector3(9f, 20.5f, -80f), new Vector3(20f, 25f, -88f), new Vector3(12f, 29.5f, -99f), new Vector3(0f, 34f, -104f) };
            float[] radius = { 3.4f, 3.2f, 3.6f, 4f };
            cloudGiftAt = clouds[2] + new Vector3(-0.3f, 1.3f, 2.2f);
            if (bird == null || piece == null) return;

            var perches = new List<Vector3> { bird.transform.position };
            for (int i = 0; i < clouds.Length; i++)
            {
                QuestCloud(kit, clouds[i], radius[i]);
                if (i < clouds.Length - 1)
                {
                    // A puff of steam on each cloud lifts Wind up to glide to the next one.
                    Vector3 to = clouds[i + 1] - clouds[i];
                    to.y = 0f;
                    to.Normalize();
                    Vector3 vent = clouds[i] + to * 1.6f;
                    kit.Updraft(vent, clouds[i + 1].y - clouds[i].y + 4.5f, LevelKit.UpdraftStyle.Steam);
                    kit.ThreadLine(vent + Vector3.up * (clouds[i + 1].y - clouds[i].y + 3f) + to * 2f, clouds[i + 1] - to * (radius[i + 1] + 1f) + Vector3.up * 2f, 3);
                    // He perches on the far side from where Patch arrives.
                    Vector3 side = new Vector3(-to.z, 0f, to.x);
                    perches.Add(clouds[i] + side * (radius[i] - 1.6f) - to * 0.6f);
                }
                else perches.Add(clouds[i] + new Vector3(-1.2f, 0f, -1.4f));
            }
            kit.AddCameraZone(new Vector3(9f, 28f, -91f), new Vector3(46f, 26f, 42f), 3f, 6f);
            kit.Hint(clouds[0] + Vector3.up * 1.5f, new Vector3(6f, 3f, 6f), "Cloud Hopping",
                "Every cloud has a puff of steam on it. Ride it up as Wind, then glide across to the next cloud. Fall, and you'll only land in the lane.", HintTheme.Wind);

            var go = new GameObject("ScaredPiper");
            go.transform.SetParent(kit.Root, false);
            var scared = go.AddComponent<ScaredPiper>();
            scared.Bird = bird;
            scared.Piece = piece;
            scared.Perches = perches.ToArray();
        }

        // A walkable cloud: a soft flat top ringed and underpinned with puffs.
        static void QuestCloud(LevelKit kit, Vector3 c, float r)
        {
            var fluff = PatchworkAssets.Felt(new Color(0.99f, 0.97f, 0.94f));
            kit.Prism(Octagon(c, r), c.y - 1f, c.y, fluff, "CloudStep");
            int ring = Mathf.RoundToInt(r * 2.6f);
            for (int i = 0; i < ring; i++)
            {
                float a = i / (float)ring * Mathf.PI * 2f;
                float s = 1.9f + (i % 3) * 0.35f;
                kit.Deco(PrimitiveType.Sphere, c + new Vector3(Mathf.Cos(a) * (r - 0.3f), -0.55f, Mathf.Sin(a) * (r - 0.3f)), new Vector3(s, s * 0.6f, s), fluff);
            }
            for (int i = 0; i < 5; i++)
            {
                float a = i / 5f * Mathf.PI * 2f + 0.4f;
                kit.Deco(PrimitiveType.Sphere, c + new Vector3(Mathf.Cos(a) * r * 0.45f, -1.3f, Mathf.Sin(a) * r * 0.45f), new Vector3(r * 0.8f, r * 0.4f, r * 0.8f), fluff);
            }
        }

        // ================================================================ The trumpet: three taps

        static void DyeTaps(LevelKit kit)
        {
            var piece = Piece(Level1Director.TrumpetId);
            var go = new GameObject("DyeWorksQuest");
            go.transform.SetParent(kit.Root, false);
            var quest = go.AddComponent<DyeWorksQuest>();
            quest.Piece = piece;
            quest.MillWheel = millWheel;
            if (piece != null) piece.LockedToast = "The reservoir's dry. No valve dew until all three dye taps are running.";

            Vector3 o = Mill;
            Vector3 vat = o + new Vector3(4.6f, 11.44f, 3.2f);
            Color rose = new Color(0.95f, 0.4f, 0.55f), butter = new Color(1f, 0.82f, 0.3f), sky = new Color(0.35f, 0.7f, 1f);
            // Rose: on the first floor, beside the pipe that carries you on up.
            var a = DyeTapAt(kit, quest, "L1_dye_rose", "Rose", rose, o + new Vector3(-2.6f, 5f, 0.7f), 90f, vat, 0, false);
            // Butter: on the top floor, in the corner behind the reservoir.
            var b = DyeTapAt(kit, quest, "L1_dye_butter", "Butter", butter, o + new Vector3(1.9f, 9f, 4.6f), 180f, vat, 1, false);
            // Sky-blue: the intake, in the canal's east wall at the waterline (swim to it).
            var c = DyeTapAt(kit, quest, "L1_dye_sky", "Sky-blue", sky, new Vector3(CanalEast - 0.25f, CanalSurface, 8f), -90f, vat, 2, true);
            quest.Taps = new[] { a, b, c };
            kit.Hint(new Vector3(CanalEast + 2.6f, 1.5f, 8f), new Vector3(4f, 3f, 6f), "The Works' Intake",
                "The Dye Works draws its sky-blue dye in from the canal here. The tap's down at the waterline: swim to it as Water.", HintTheme.Water);
        }

        static DyeTap DyeTapAt(LevelKit kit, DyeWorksQuest quest, string key, string dye, Color tint, Vector3 feet, float yaw, Vector3 vat, int layer, bool inCanal)
        {
            var copper = PatchworkAssets.Plain(new Color(0.78f, 0.48f, 0.3f), 0.75f);
            Vector3 fwd = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            var glow = PatchworkAssets.Glow(tint, 0.9f);
            var wet = PatchworkAssets.Water(tint);
            Vector3 hub;
            DyeTap tap;
            // What appears once it's running (hidden until then).
            var stream = kit.Group("DyeStream", Vector3.zero);
            if (!inCanal)
            {
                // A standpipe with a wheel on the front.
                kit.Deco(PrimitiveType.Cylinder, feet + Vector3.up * 0.55f, new Vector3(0.24f, 0.55f, 0.24f), copper);
                kit.Deco(PrimitiveType.Sphere, feet + Vector3.up * 1.1f, Vector3.one * 0.38f, copper);
                hub = feet + Vector3.up * 1.1f + fwd * 0.24f;
                tap = kit.Volume<DyeTap>(feet + Vector3.up * 0.9f, new Vector3(1.7f, 2.2f, 1.7f), "DyeTap_" + dye);
                kit.Deco(PrimitiveType.Cylinder, feet + fwd * 0.5f + Vector3.up * 0.5f, new Vector3(0.16f, 0.5f, 0.16f), glow, default(Vector3), stream);
                kit.Deco(PrimitiveType.Cylinder, feet + fwd * 0.5f + Vector3.up * 0.03f, new Vector3(1.1f, 0.012f, 1.1f), wet, default(Vector3), stream);
            }
            else
            {
                // A fat pipe mouth in the canal wall at the waterline, and the wheel on a post
                // on the bank above it.
                var mouth = kit.Deco(PrimitiveType.Cylinder, feet + Vector3.up * 0.2f, new Vector3(1.5f, 0.22f, 1.5f), copper);
                mouth.rotation = Quaternion.FromToRotation(Vector3.up, fwd);
                var dark = kit.Deco(PrimitiveType.Cylinder, feet + Vector3.up * 0.2f + fwd * 0.22f, new Vector3(1.15f, 0.01f, 1.15f), PatchworkAssets.Plain(new Color(0.08f, 0.07f, 0.12f), 0.2f));
                dark.rotation = Quaternion.FromToRotation(Vector3.up, fwd);
                Vector3 post = new Vector3(feet.x - fwd.x * 0.55f, 0f, feet.z - fwd.z * 0.55f);
                kit.Deco(PrimitiveType.Cylinder, post + Vector3.up * 0.55f, new Vector3(0.2f, 0.55f, 0.2f), copper);
                kit.Deco(PrimitiveType.Sphere, post + Vector3.up * 1.1f, Vector3.one * 0.34f, copper);
                hub = post + Vector3.up * 1.1f + fwd * 0.22f;
                tap = kit.Volume<DyeTap>(feet + fwd * 1.15f + Vector3.up * 0.1f, new Vector3(2.5f, 2.2f, 3f), "DyeTap_" + dye);
                var jet = kit.Deco(PrimitiveType.Cylinder, feet + Vector3.up * 0.2f + fwd * 0.5f, new Vector3(0.7f, 0.4f, 0.7f), glow, default(Vector3), stream);
                jet.rotation = Quaternion.FromToRotation(Vector3.up, fwd);
                kit.Deco(PrimitiveType.Cylinder, feet + fwd * 1.5f + Vector3.up * 0.07f, new Vector3(2.8f, 0.01f, 2.4f), wet, default(Vector3), stream);
            }
            var wheel = kit.Group("TapWheel", hub);
            wheel.rotation = Quaternion.FromToRotation(Vector3.up, fwd);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.75f, 0.04f, 0.75f), PatchworkAssets.Plain(tint, 0.6f), default(Vector3), wheel);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.2f, 0.07f, 0.2f), copper, default(Vector3), wheel);
            for (int k = 0; k < 4; k++)
            {
                float ang = k * Mathf.PI * 0.5f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(ang) * 0.37f, 0f, Mathf.Sin(ang) * 0.37f), Vector3.one * 0.15f, copper, default(Vector3), wheel);
            }
            // Its colour pooling in the reservoir, a ring inside the last.
            float d = 2.3f - layer * 0.65f;
            kit.Deco(PrimitiveType.Cylinder, vat + Vector3.up * 0.012f * (layer + 1), new Vector3(d, 0.006f, d), wet, default(Vector3), stream);
            stream.gameObject.SetActive(false);

            tap.Key = key;
            tap.Dye = dye;
            tap.Tint = tint;
            tap.Wheel = wheel;
            tap.Stream = stream;
            tap.Quest = quest;
            return tap;
        }

        // ================================================================ Lost wedding presents

        // A clear bit of ground near a point (or the point itself, if nowhere's free).
        static Vector3 ClearGround(Vector3 near)
        {
            Vector3 p;
            if (Spot(near, 1.6f, out p)) return p;
            foreach (float r in new[] { 5f, 9f })
                for (int k = 0; k < 8; k++)
                {
                    float a = k * Mathf.PI * 0.25f;
                    if (Spot(near + new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r), 1.6f, out p)) return p;
                }
            return near;
        }

        static void WeddingPresents(LevelKit kit)
        {
            Physics.SyncTransforms();
            // Fireworks: out west by the toy soldier's campsite.
            Present(kit, Level1Director.Fireworks, ClearGround(new Vector3(-104f, 0f, -31f)) + Vector3.up * 1.2f, new Color(0.9f, 0.3f, 0.35f), LevelKit.Palette.Gold);
            // Confetti cannons: up in the sky, on the third cloud of the piper's climb.
            Present(kit, Level1Director.Cannons, cloudGiftAt, new Color(0.6f, 0.45f, 0.9f), new Color(1f, 0.85f, 0.4f));
            // Balloons: adrift on a little raft in the canal, a long way south (swim to it).
            Vector3 raft = new Vector3((CanalWest + CanalEast) * 0.5f, CanalSurface, -84f);
            kit.Deco(PrimitiveType.Cube, raft + Vector3.up * 0.03f, new Vector3(1.7f, 0.14f, 1.7f), PatchworkAssets.Planks(new Color(0.7f, 0.52f, 0.36f)), new Vector3(0f, 18f, 0f));
            Present(kit, Level1Director.Balloons, raft + Vector3.up * 0.85f, new Color(0.35f, 0.7f, 0.95f), new Color(1f, 0.95f, 0.9f));
            // Paper doves: down in the ravine, on the far (west) bank near the north end.
            Present(kit, Level1Director.Doves, new Vector3((RavineWest + StreamWest) * 0.5f, -5f + 1.2f, 58f), new Color(0.97f, 0.95f, 0.9f), new Color(0.95f, 0.5f, 0.6f));
            // Bunting: delivered to the wrong end of the toybox, in the far north-east corner.
            Present(kit, Level1Director.Bunting, ClearGround(new Vector3(106f, 0f, 106f)) + Vector3.up * 1.2f, new Color(0.45f, 0.8f, 0.55f), new Color(1f, 0.6f, 0.7f));
        }

        static void Present(LevelKit kit, int index, Vector3 pos, Color wrap, Color ribbon)
        {
            var present = kit.Volume<WeddingPresent>(pos, new Vector3(2.2f, 2.4f, 2.2f), "WeddingPresent_" + index);
            var vis = new GameObject("Visual").transform;
            vis.SetParent(present.transform, false);
            var paper = PatchworkAssets.Fabric(wrap);
            var bow = PatchworkAssets.Felt(ribbon);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.05f, 0f), new Vector3(0.8f, 0.7f, 0.8f), paper, default(Vector3), vis);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.36f, 0f), new Vector3(0.9f, 0.18f, 0.9f), paper, default(Vector3), vis);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.02f, 0f), new Vector3(0.16f, 0.88f, 0.92f), bow, default(Vector3), vis);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.02f, 0f), new Vector3(0.92f, 0.88f, 0.16f), bow, default(Vector3), vis);
            kit.Deco(PrimitiveType.Sphere, new Vector3(-0.2f, 0.56f, 0f), new Vector3(0.36f, 0.22f, 0.18f), bow, new Vector3(0f, 0f, 20f), vis);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.2f, 0.56f, 0f), new Vector3(0.36f, 0.22f, 0.18f), bow, new Vector3(0f, 0f, -20f), vis);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.52f, 0f), Vector3.one * 0.15f, bow, default(Vector3), vis);
            var lightGo = new GameObject("Glow");
            lightGo.transform.SetParent(vis, false);
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.9f, 0.7f);
            l.range = 5f;
            l.intensity = 1.2f;
            l.shadows = LightShadows.None;
            present.Setup(index, vis);
        }

        // ================================================================ What the wedding gains

        // Everything a present adds to the square is built here and kept hidden until the
        // wedding (or, for a present found afterwards, until it's found).
        static void WeddingExtras(LevelKit kit)
        {
            Vector3 arch = director.ArchCenter;

            // ----- The good bunting: strung from two tall poles to the bandstand roof -----
            var bunting = kit.Group("Wedding_Bunting", Vector3.zero);
            var white = PatchworkAssets.Wood(new Color(0.97f, 0.95f, 0.9f));
            Vector3 poleL = new Vector3(-10.5f, 0.06f, -15.6f), poleR = new Vector3(10.5f, 0.06f, -15.6f);
            foreach (var p in new[] { poleL, poleR })
            {
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 2.9f, new Vector3(0.16f, 2.9f, 0.16f), white, default(Vector3), bunting);
                kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 5.9f, Vector3.one * 0.34f, PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f), default(Vector3), bunting);
            }
            Vector3 topL = poleL + Vector3.up * 5.7f, topR = poleR + Vector3.up * 5.7f;
            BuntingInto(kit, bunting, topL, Bandstand + new Vector3(-3.6f, 4.6f, -5.2f));
            BuntingInto(kit, bunting, topR, Bandstand + new Vector3(3.6f, 4.6f, -5.2f));
            BuntingInto(kit, bunting, topL, topR);
            director.GiftScenes[Level1Director.Bunting] = bunting.gameObject;

            // ----- Balloons: bunches tied down beside the chairs and either side of the arch -----
            var balloons = kit.Group("Wedding_Balloons", Vector3.zero);
            Vector3[] bunches = { new Vector3(-6.6f, 0.06f, -13f), new Vector3(6.6f, 0.06f, -13f), arch + new Vector3(-4.8f, 0.06f, 0.6f), arch + new Vector3(4.8f, 0.06f, 0.6f) };
            Color[] cols = { new Color(0.95f, 0.4f, 0.5f), new Color(1f, 0.85f, 0.35f), new Color(0.45f, 0.75f, 1f), new Color(0.7f, 0.55f, 1f), new Color(0.5f, 0.9f, 0.6f) };
            var str = PatchworkAssets.Yarn(LevelKit.Palette.Cream);
            for (int n = 0; n < bunches.Length; n++)
            {
                var g = new GameObject("BalloonBunch").transform;
                g.SetParent(balloons, false);
                g.position = bunches[n];
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.12f, 0f), new Vector3(0.3f, 0.24f, 0.3f), PatchworkAssets.Felt(new Color(0.85f, 0.3f, 0.4f)), new Vector3(0f, 30f, 0f), g);
                var bunch = g.gameObject.AddComponent<BalloonBunch>();
                bunch.Balloons = new Transform[5];
                for (int i = 0; i < 5; i++)
                {
                    float a = i * 1.26f + n;
                    Vector3 at = new Vector3(Mathf.Cos(a) * 0.42f, 2.5f + (i % 3) * 0.42f, Mathf.Sin(a) * 0.42f);
                    Rod(kit, new Vector3(0f, 0.2f, 0f), at + Vector3.down * 0.32f, str, 0.02f, g);
                    bunch.Balloons[i] = kit.Deco(PrimitiveType.Sphere, at, new Vector3(0.6f, 0.72f, 0.6f), PatchworkAssets.Plain(cols[(i + n) % cols.Length], 0.75f), default(Vector3), g);
                }
            }
            director.GiftScenes[Level1Director.Balloons] = balloons.gameObject;

            // ----- Confetti cannons: one either side of the aisle, pointing up over it -----
            var cannons = kit.Group("Wedding_Cannons", Vector3.zero);
            var stripes = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.9f, 0.3f, 0.35f), new Color(0.99f, 0.95f, 0.85f));
            for (int s = -1; s <= 1; s += 2)
            {
                var c = new GameObject("Cannon").transform;
                c.SetParent(cannons, false);
                c.position = arch + new Vector3(s * 4.4f, 0.06f, -2.8f);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.3f, 0f), new Vector3(0.7f, 0.08f, 0.7f), PatchworkAssets.Wood(TownKit.DarkWood), new Vector3(0f, 0f, 90f), c);
                var barrel = kit.Deco(PrimitiveType.Cylinder, new Vector3(-s * 0.15f, 0.75f, 0f), new Vector3(0.5f, 0.6f, 0.5f), stripes, new Vector3(0f, 0f, s * 35f), c);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.98f, 0f), new Vector3(1.16f, 0.06f, 1.16f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f), default(Vector3), barrel);
            }
            director.GiftScenes[Level1Director.Cannons] = cannons.gameObject;

            // ----- Paper doves: wheeling over the square -----
            var doves = kit.Group("Wedding_Doves", Vector3.zero);
            for (int i = 0; i < 6; i++)
            {
                var bird = kit.YarnBird(arch + new Vector3(0f, 7.5f + (i % 3) * 0.9f, 2f), new Color(0.98f, 0.97f, 0.95f), true, 6f + i * 0.8f);
                bird.transform.SetParent(doves, true);
            }
            director.GiftScenes[Level1Director.Doves] = doves.gameObject;

            // ----- More guests, the more of the toybox has been explored -----
            Vector3[] spots =
            {
                new Vector3(-8.4f, 0.06f, -11f), new Vector3(8.4f, 0.06f, -11f), new Vector3(-7.6f, 0.06f, -5.4f), new Vector3(9.8f, 0.06f, -7.4f),
                new Vector3(-1.6f, 0.06f, -10.8f), new Vector3(1.6f, 0.06f, -10.8f), new Vector3(-7f, 0.06f, -14.6f), new Vector3(7f, 0.06f, -14.6f),
            };
            ToyNpc.Kind[] kinds = { ToyNpc.Kind.Bunny, ToyNpc.Kind.RagDoll, ToyNpc.Kind.TinSoldier, ToyNpc.Kind.Teddy, ToyNpc.Kind.JackInBox, ToyNpc.Kind.RagDoll, ToyNpc.Kind.Nutcracker, ToyNpc.Kind.Bunny };
            Color[] mains = { new Color(0.75f, 0.85f, 0.95f), new Color(0.95f, 0.7f, 0.45f), new Color(0.3f, 0.55f, 0.4f), new Color(0.8f, 0.6f, 0.45f),
                              new Color(0.6f, 0.4f, 0.75f), new Color(0.5f, 0.75f, 0.7f), new Color(0.75f, 0.3f, 0.3f), new Color(0.95f, 0.85f, 0.6f) };
            for (int i = 0; i < spots.Length; i++)
            {
                var guest = Toy(kit, spots[i], 0f, kinds[i], "Guest", mains[i], i % 2 == 0 ? LevelKit.Palette.Gold : new Color(0.97f, 0.93f, 0.85f), ToyNpc.Instrument.None, 0.9f + (i % 4) * 0.12f, Color.grey, false);
                director.ExtraGuests.Add(guest);
            }
        }

        // A line of bunting, like LevelKit.Bunting but all under one parent (so it can be hidden).
        static void BuntingInto(LevelKit kit, Transform parent, Vector3 a, Vector3 b)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            float sagMax = Mathf.Min(1.2f, len * 0.07f);
            System.Func<float, Vector3> P = k => Vector3.Lerp(a, b, k) + Vector3.down * (Mathf.Sin(k * Mathf.PI) * sagMax);
            var yarn = PatchworkAssets.Yarn(LevelKit.Palette.Cream);
            int segs = Mathf.Clamp(Mathf.RoundToInt(len / 1.5f), 2, 16);
            for (int i = 0; i < segs; i++)
                Rod(kit, P(i / (float)segs), P((i + 1) / (float)segs), yarn, 0.05f, parent);
            int flags = Mathf.Max(2, Mathf.RoundToInt(len / 1.6f));
            var cols = LevelKit.Palette.Quilt;
            for (int i = 1; i < flags; i++)
            {
                float k = i / (float)flags;
                kit.Deco(PrimitiveType.Cube, P(k) + Vector3.down * 0.45f, new Vector3(0.7f, 0.7f, 0.04f), PatchworkAssets.Fabric(cols[i % cols.Length]),
                    new Vector3(0f, Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg + 90f, 45f), parent);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Toybox Town's secrets: twelve marbles, each found a different way, plus two hidden rooms
    // (inside the Toy Shop, and a glowing grotto reached through a pipe in the canal). The rooms
    // are built out in the nursery past the toybox walls, where you can't see them from town.
    public static partial class Level1Builder
    {
        public static readonly string[] MarbleIds =
        {
            "L1_marble_shop", "L1_marble_grotto", "L1_marble_tune", "L1_marble_hill", "L1_marble_ravine", "L1_marble_peekaboo",
            "L1_marble_rush", "L1_marble_goal", "L1_marble_hatch", "L1_marble_chimes", "L1_marble_clock", "L1_marble_duckling",
            "L1_marble_whack", "L1_marble_trapeze", "L1_marble_wood", "L1_marble_moon"
        };

        static readonly Vector3 ShopRoom = new Vector3(250f, 0f, -200f);
        static readonly Vector3 Grotto = new Vector3(250f, 0f, -262f);
        static readonly Color[] MarbleColors =
        {
            new Color(1f, 0.45f, 0.55f), new Color(0.4f, 0.85f, 1f), new Color(1f, 0.8f, 0.3f), new Color(0.5f, 0.9f, 0.5f),
            new Color(0.75f, 0.55f, 1f), new Color(1f, 0.6f, 0.3f)
        };

        static Color MarbleColor(int i) { return MarbleColors[i % MarbleColors.Length]; }

        static void Secrets(LevelKit kit)
        {
            kit.Info.SecretIds.AddRange(MarbleIds);
            ToyShopInside(kit);
            DrainGrotto(kit);
            TuneSecret(kit);
            DigSpots(kit);
            HideAndSeek(kit);
            HillRush(kit);
            Football(kit);
            BarracksHatch(kit);
            ChimeSecret(kit);
            ClockClimb(kit);
            LostDuckling(kit);
        }

        // A plain room: floor, four walls and a ceiling, none of which cast shadows (so the sun
        // still lights it like an open dolls' house).
        static void Room(LevelKit kit, Vector3 o, Vector3 size, Material floor, Material walls, Material ceiling, string name)
        {
            float t = 0.4f;
            var parts = new List<Transform>
            {
                kit.Block(o + new Vector3(0f, -0.2f, 0f), new Vector3(size.x, 0.4f, size.z), Color.white, name + "Floor", floor),
                kit.Block(o + new Vector3(0f, size.y + 0.2f, 0f), new Vector3(size.x + t * 2f, 0.4f, size.z + t * 2f), Color.white, name + "Ceiling", ceiling),
                kit.Block(o + new Vector3(-size.x * 0.5f - t * 0.5f, size.y * 0.5f, 0f), new Vector3(t, size.y, size.z), Color.white, name + "Wall", walls),
                kit.Block(o + new Vector3(size.x * 0.5f + t * 0.5f, size.y * 0.5f, 0f), new Vector3(t, size.y, size.z), Color.white, name + "Wall", walls),
                kit.Block(o + new Vector3(0f, size.y * 0.5f, -size.z * 0.5f - t * 0.5f), new Vector3(size.x + t * 2f - 0.02f, size.y - 0.02f, t), Color.white, name + "Wall", walls),
                kit.Block(o + new Vector3(0f, size.y * 0.5f, size.z * 0.5f + t * 0.5f), new Vector3(size.x + t * 2f - 0.02f, size.y - 0.02f, t), Color.white, name + "Wall", walls),
            };
            foreach (var p in parts) p.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        static Warp WarpAt(LevelKit kit, Vector3 centre, Vector3 size, Vector3 target, float yaw, bool press, string label)
        {
            var w = kit.Volume<Warp>(centre, size, "Warp");
            w.Target = target;
            w.TargetYaw = yaw;
            w.PressToEnter = press;
            w.Label = label;
            return w;
        }

        // ---------------------------------------------------------------- 1. Inside the Toy Shop

        static void ToyShopInside(LevelKit kit)
        {
            Vector3 o = ShopRoom;
            var size = new Vector3(16f, 7f, 12f);
            Room(kit, o, size, PatchworkAssets.Planks(new Color(0.8f, 0.6f, 0.42f)), PatchworkAssets.Damask(new Color(0.98f, 0.82f, 0.85f)),
                PatchworkAssets.Plaster(new Color(0.98f, 0.95f, 0.9f)), "Shop");
            // The front door in town takes you in; the door inside brings you back out.
            WarpAt(kit, new Vector3(12f, 1.2f, 22.2f), new Vector3(1.8f, 2.4f, 1.2f), o + new Vector3(0f, 0f, -4.2f), 0f, true, "Go inside");
            Vector3 door = o + new Vector3(0f, 0f, -size.z * 0.5f);
            kit.Deco(PrimitiveType.Cube, door + new Vector3(0f, 1.15f, 0.03f), new Vector3(1.6f, 2.3f, 0.08f), PatchworkAssets.Wood(TownKit.DarkWood));
            kit.Deco(PrimitiveType.Cube, door + new Vector3(0f, 1.1f, 0.1f), new Vector3(1.3f, 2.2f, 0.06f), PatchworkAssets.Planks(new Color(0.55f, 0.75f, 0.55f)));
            kit.Deco(PrimitiveType.Sphere, door + new Vector3(0.45f, 1.05f, 0.18f), Vector3.one * 0.12f, PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f));
            WarpAt(kit, door + new Vector3(0f, 1.2f, 0.7f), new Vector3(1.8f, 2.4f, 1f), new Vector3(12f, 0f, 20.6f), 180f, true, "Go outside");

            // A rug, a counter and the shopkeeper.
            kit.Deco(PrimitiveType.Cylinder, o + new Vector3(0f, 0.02f, 0f), new Vector3(7f, 0.02f, 5f), PatchworkAssets.Knit(new Color(0.55f, 0.7f, 0.9f)));
            kit.Block(o + new Vector3(1.5f, 0.55f, 4.4f), new Vector3(6f, 1.1f, 1f), new Color(0.6f, 0.42f, 0.3f), "Counter", PatchworkAssets.Planks(new Color(0.6f, 0.42f, 0.3f)));
            kit.Deco(PrimitiveType.Cube, o + new Vector3(1.5f, 1.14f, 4.4f), new Vector3(6.2f, 0.08f, 1.2f), PatchworkAssets.Wood(new Color(0.85f, 0.7f, 0.5f)));
            var keeper = Toy(kit, o + new Vector3(1.5f, 0f, 5.0f), 180f, ToyNpc.Kind.Bunny, "Mrs Thimble", new Color(0.95f, 0.9f, 0.85f), new Color(0.85f, 0.45f, 0.6f), ToyNpc.Instrument.None, 1.3f, new Color(0.85f, 0.45f, 0.6f));
            int hint = 0;
            keeper.Lines = () =>
            {
                string[][] hints =
                {
                    new[] { "Welcome, welcome! Mind the shelves, everything's for sale. Except the marble on the top shelf. That's a secret.", "There are fourteen secret marbles hidden in Toybox Town, you know. Hardly anyone finds them all." },
                    new[] { "Ever played the music box? Lovely little tune. I hear someone wrote it on a sign by the xylophone bridge..." },
                    new[] { "A little jack-in-the-box keeps hiding round town. Listen for giggling!" },
                    new[] { "Things sparkle in the grass sometimes. Have a dig with your hands!" },
                    new[] { "That old crate in the barracks yard never moves. Wonder what's under it." },
                    new[] { "There's a pipe in the canal wall, half under the water. Only something runny could get in." },
                    new[] { "Mama Duck's lost her duckling again. Last seen in the ravine, poor thing." },
                    new[] { "Ring every set of wind chimes in town and the bandstand gets a surprise." },
                };
                return hints[hint++ % hints.Length];
            };

            // Shelves up the west wall, low to high: a little climb to the marble.
            var shelfMat = PatchworkAssets.Planks(new Color(0.95f, 0.88f, 0.7f));
            float wx = o.x - size.x * 0.5f + 0.75f;
            Vector3[] shelves = { new Vector3(wx, 1.5f, o.z - 2.6f), new Vector3(wx, 3.0f, o.z + 0.4f), new Vector3(wx, 4.5f, o.z + 3.6f) };
            Color[] toyCols = { new Color(0.95f, 0.4f, 0.4f), new Color(0.4f, 0.65f, 0.95f), new Color(1f, 0.8f, 0.3f), new Color(0.5f, 0.8f, 0.5f), new Color(0.8f, 0.55f, 0.9f) };
            for (int i = 0; i < shelves.Length; i++)
            {
                Vector3 s = shelves[i];
                kit.Block(s + Vector3.down * 0.15f, new Vector3(1.5f, 0.3f, 3f), new Color(0.95f, 0.88f, 0.7f), "Shelf", shelfMat);
                for (int b = -1; b <= 1; b += 2)
                    kit.Deco(PrimitiveType.Cube, s + new Vector3(0f, -0.5f, b * 1.3f), new Vector3(1.4f, 0.4f, 0.1f), PatchworkAssets.Wood(TownKit.DarkWood), new Vector3(0f, 0f, 30f));
                // Little toys on each shelf.
                for (int k = 0; k < 3; k++)
                {
                    Vector3 p = s + new Vector3(-0.35f, 0.25f, -1.1f + k * 0.55f);
                    var col = toyCols[(i * 3 + k) % toyCols.Length];
                    if (k == 0) kit.Deco(PrimitiveType.Cube, p, Vector3.one * 0.4f, PatchworkAssets.Wood(col), new Vector3(0f, 20f, 0f));
                    else if (k == 1) kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 0.05f, Vector3.one * 0.45f, PatchworkAssets.Yarn(col));
                    else kit.Deco(PrimitiveType.Cylinder, p, new Vector3(0.3f, 0.2f, 0.3f), PatchworkAssets.Plain(col, 0.7f));
                }
            }
            SecretMarble.Place(kit, shelves[2] + new Vector3(0f, 1.1f, 0.6f), MarbleIds[0], MarbleColor(0));

            // A big music box on the east side. Step on its lid to play its tune.
            Vector3 mb = o + new Vector3(5.5f, 0f, 1f);
            kit.Block(mb + Vector3.up * 0.5f, new Vector3(2.2f, 1f, 1.6f), new Color(0.75f, 0.35f, 0.4f), "MusicBox", PatchworkAssets.Wood(new Color(0.75f, 0.35f, 0.4f)));
            kit.Deco(PrimitiveType.Cube, mb + Vector3.up * 1.02f, new Vector3(2.3f, 0.06f, 1.7f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f));
            var crank = kit.Deco(PrimitiveType.Cylinder, mb + new Vector3(1.2f, 0.55f, 0f), new Vector3(0.12f, 0.2f, 0.12f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f), new Vector3(0f, 0f, 90f));
            crank.name = "Crank";
            var dancer = kit.Group("Dancer", mb + Vector3.up * 1.05f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.25f, 0f), new Vector3(0.06f, 0.25f, 0.06f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f), default(Vector3), dancer);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.65f, 0f), new Vector3(0.6f, 0.18f, 0.6f), PatchworkAssets.Felt(new Color(1f, 0.75f, 0.85f)), default(Vector3), dancer);
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.85f, 0f), new Vector3(0.18f, 0.22f, 0.18f), PatchworkAssets.Felt(new Color(1f, 0.85f, 0.8f)), default(Vector3), dancer);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.15f, 0f), Vector3.one * 0.18f, PatchworkAssets.Felt(new Color(1f, 0.88f, 0.8f)), default(Vector3), dancer);
            var box = kit.Volume<MusicBox>(mb + Vector3.up * 1.6f, new Vector3(2.2f, 1.4f, 1.6f), "MusicBoxLid");
            box.Dancer = dancer;

            // A wind-up mouse doing laps of the rug, and some toys on the floor.
            WindupMouse(kit, PathMover.Oval(o + new Vector3(0f, 0.05f, 0f), 3f, 2f, 24), 0f, new Color(0.7f, 0.7f, 0.75f));
            Grabbable.ToyBlock(kit, o + new Vector3(-3f, 0f, -3.5f), 0.9f, toyCols[0], 15f);
            Grabbable.ToyBlock(kit, o + new Vector3(-2f, 0f, -3.8f), 0.9f, toyCols[2], -10f);
            Grabbable.BeachBall(kit, o + new Vector3(3f, 0f, -3f), 0.5f);
            // Warm lamps.
            var lamp = kit.Group("ShopLight", o + new Vector3(0f, 5.5f, 0f));
            AddGlow(lamp, new Color(1f, 0.85f, 0.65f), 16f, 1.6f);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cylinder, o + new Vector3(s * 3.5f, 6.4f, 0f), new Vector3(0.04f, 0.6f, 0.04f), PatchworkAssets.Plain(TownKit.Iron, 0.5f));
                kit.Deco(PrimitiveType.Sphere, o + new Vector3(s * 3.5f, 5.7f, 0f), new Vector3(0.7f, 0.5f, 0.7f), PatchworkAssets.Glow(new Color(1f, 0.85f, 0.6f), 1.2f));
            }
            for (int i = 0; i < 6; i++)
                ScoreBubbles.Place(kit, o + new Vector3(-4f + i * 1.6f, 1.2f, 1.5f), 10);
        }

        // ---------------------------------------------------------------- 2. The Drain Grotto

        static void DrainGrotto(LevelKit kit)
        {
            Vector3 o = Grotto;
            var size = new Vector3(20f, 9f, 20f);
            Room(kit, o, size, PatchworkAssets.Felt(new Color(0.18f, 0.2f, 0.35f)), PatchworkAssets.Knit(new Color(0.22f, 0.25f, 0.42f)),
                PatchworkAssets.Knit(new Color(0.16f, 0.17f, 0.3f)), "Grotto");
            // The way in: a big pipe mouth in the canal's west wall, half under the water.
            Vector3 pipe = new Vector3(CanalWest + 0.3f, CanalSurface, -20f);
            var tube = PatchworkAssets.Cardboard(new Color(0.65f, 0.6f, 0.55f));
            kit.Deco(PrimitiveType.Cylinder, pipe + Vector3.right * 0.1f, new Vector3(2.6f, 0.2f, 2.6f), tube, new Vector3(0f, 0f, 90f));
            kit.Deco(PrimitiveType.Cylinder, pipe + Vector3.right * 0.22f, new Vector3(2.1f, 0.05f, 2.1f), PatchworkAssets.Plain(new Color(0.05f, 0.05f, 0.08f), 0.2f), new Vector3(0f, 0f, 90f));
            var bolts = PatchworkAssets.Plain(new Color(0.55f, 0.55f, 0.6f), 0.7f);
            for (int i = 0; i < 6; i++)
            {
                float a = i / 6f * Mathf.PI * 2f;
                kit.Deco(PrimitiveType.Sphere, pipe + new Vector3(0.22f, Mathf.Sin(a) * 1.15f, Mathf.Cos(a) * 1.15f), Vector3.one * 0.14f, bolts);
            }
            var inPipe = WarpAt(kit, pipe + new Vector3(0.8f, 0f, 0f), new Vector3(1.4f, 2f, 2.2f), o + new Vector3(-7f, 0f, -7f), 45f, false, "");
            inPipe.WaterOnly = true;
            // The way out: a pipe in the grotto wall that comes out on the canal bank.
            Vector3 outPipe = o + new Vector3(size.x * 0.5f - 0.2f, 1.2f, 6f);
            kit.Deco(PrimitiveType.Cylinder, outPipe, new Vector3(2.4f, 0.2f, 2.4f), tube, new Vector3(0f, 0f, 90f));
            kit.Deco(PrimitiveType.Cylinder, outPipe + Vector3.left * 0.12f, new Vector3(1.9f, 0.05f, 1.9f), PatchworkAssets.Glow(new Color(0.75f, 0.9f, 1f), 0.6f), new Vector3(0f, 0f, 90f));
            WarpAt(kit, outPipe + Vector3.left * 0.8f, new Vector3(1.4f, 2.4f, 2.2f), new Vector3(CanalWest - 1.5f, 0f, -20f), -90f, false, "");
            kit.Hint(o + new Vector3(-7f, 1.5f, -5f), new Vector3(5f, 3f, 5f), "The Drain Grotto",
                "A secret cave under the town, full of sugar crystals. Climb the crystals (or ride the spring as Water) up to the ledge. The glowing pipe takes you back.");

            // Sugar crystals growing up from the floor and down from the ceiling.
            Color[] crystal = { new Color(0.55f, 0.85f, 1f), new Color(1f, 0.6f, 0.85f), new Color(0.75f, 0.6f, 1f) };
            var rng = new System.Random(5);
            for (int i = 0; i < 22; i++)
            {
                float x = (float)(rng.NextDouble() * 2.0 - 1.0) * (size.x * 0.5f - 1.2f);
                float z = (float)(rng.NextDouble() * 2.0 - 1.0) * (size.z * 0.5f - 1.2f);
                if (Mathf.Abs(x + 7f) < 2.5f && Mathf.Abs(z + 7f) < 2.5f) continue;   // keep the arrival spot clear
                bool up = i % 3 != 0;
                float h = 0.8f + (float)rng.NextDouble() * 1.8f;
                var mat = PatchworkAssets.Glow(crystal[i % 3], 0.9f);
                for (int k = 0; k < 3; k++)
                {
                    float tilt = (k - 1) * 18f;
                    Vector3 basePos = o + new Vector3(x + (k - 1) * 0.3f, up ? 0f : size.y, z + (k % 2) * 0.25f);
                    var c = kit.Deco(PrimitiveType.Cube, basePos + Vector3.up * (up ? h * 0.5f : -h * 0.5f) * (1f - k * 0.2f),
                        new Vector3(0.3f, h * (1f - k * 0.2f), 0.3f), mat, new Vector3(tilt, i * 37f, tilt * 0.5f));
                    c.name = "Crystal";
                }
                if (up)
                {
                    // Floor clusters are solid, so you bump into them and can hop on top.
                    var solid = new GameObject("CrystalSolid");
                    solid.transform.SetParent(kit.Root, false);
                    solid.transform.position = o + new Vector3(x, h * 0.45f, z + 0.12f);
                    solid.transform.rotation = Quaternion.Euler(0f, i * 37f, 0f);
                    solid.AddComponent<BoxCollider>().size = new Vector3(0.95f, h * 0.9f, 0.6f);
                }
            }
            // Stepping-stone crystals up to a ledge, and a spring for Water to ride.
            var pillar = PatchworkAssets.Glow(new Color(0.6f, 0.85f, 1f), 0.5f);
            Vector3[] steps = { new Vector3(-3f, 1.2f, 2f), new Vector3(-0.8f, 2.4f, 3.6f), new Vector3(1.6f, 3.6f, 5f), new Vector3(4f, 4.8f, 6.4f) };
            foreach (var s in steps)
                kit.Block(o + new Vector3(s.x, s.y * 0.5f, s.z), new Vector3(1.6f, s.y, 1.6f), new Color(0.6f, 0.85f, 1f), "CrystalPillar", pillar);
            kit.Block(o + new Vector3(6.5f, 2.9f, 8f), new Vector3(5f, 0.4f, 3f), new Color(0.3f, 0.32f, 0.5f), "GrottoLedge", PatchworkAssets.Knit(new Color(0.3f, 0.32f, 0.5f)));
            kit.Fountain(o + new Vector3(2.4f, 0f, 8.6f), 6f, LevelKit.FountainStyle.Spring);
            SecretMarble.Place(kit, o + new Vector3(7.5f, 4.4f, 8f), MarbleIds[1], MarbleColor(1));
            for (int i = 0; i < steps.Length; i++) ScoreBubbles.Place(kit, o + steps[i] + Vector3.up * 1.2f, 50);
            // Blue and pink glows.
            var l1 = kit.Group("GrottoLight", o + new Vector3(-4f, 6f, -2f));
            AddGlow(l1, new Color(0.5f, 0.8f, 1f), 18f, 2f);
            var l2 = kit.Group("GrottoLight", o + new Vector3(5f, 6f, 5f));
            AddGlow(l2, new Color(1f, 0.6f, 0.85f), 16f, 1.6f);
        }

        // ---------------------------------------------------------------- 3. The xylophone tune

        static void TuneSecret(LevelKit kit)
        {
            // The music box's tune: E D C D E E E (keys 3, 2, 1, 2, 3, 3, 3 from the town side).
            int[] tune = { 2, 1, 0, 1, 2, 2, 2 };
            var played = new List<int>();
            Vector3 mid = new Vector3(CanalWest + 4f, 2.6f, 60f);
            var marble = SecretMarble.Place(kit, mid, MarbleIds[2], MarbleColor(2), true);
            bool done = GameState.IsCollected(MarbleIds[2]);
            XyloKey.AnyStruck = idx =>
            {
                if (done) return;
                played.Add(idx);
                if (played.Count > tune.Length) played.RemoveAt(0);
                if (played.Count < tune.Length) return;
                for (int i = 0; i < tune.Length; i++) if (played[i] != tune[i]) return;
                done = true;
                marble.Reveal();
            };
            // A little song sheet on a stand by the bridge: coloured notes in order.
            Vector3 stand = new Vector3(CanalWest - 2.2f, 0f, 57.2f);
            kit.Deco(PrimitiveType.Cylinder, stand + Vector3.up * 0.7f, new Vector3(0.08f, 0.7f, 0.08f), PatchworkAssets.Plain(TownKit.Iron, 0.5f));
            Solid(kit, stand + Vector3.up * 0.7f, new Vector3(0.15f, 1.4f, 0.15f));
            var sheet = kit.Group("SongSheet", stand + Vector3.up * 1.5f);
            sheet.rotation = Quaternion.Euler(-20f, 90f, 0f);
            kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(1.4f, 0.9f, 0.04f), PatchworkAssets.Felt(new Color(0.98f, 0.96f, 0.9f)), default(Vector3), sheet);
            for (int l = 0; l < 5; l++)
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.25f + l * 0.12f, -0.025f), new Vector3(1.3f, 0.01f, 0.005f), PatchworkAssets.Plain(UIKit.Ink, 0.4f), default(Vector3), sheet);
            Color[] keyCols = { new Color(0.92f, 0.3f, 0.35f), new Color(0.98f, 0.55f, 0.25f), new Color(0.98f, 0.82f, 0.3f) };
            for (int i = 0; i < tune.Length; i++)
                kit.Deco(PrimitiveType.Sphere, new Vector3(-0.54f + i * 0.18f, -0.25f + tune[i] * 0.12f, -0.035f), new Vector3(0.12f, 0.1f, 0.02f), PatchworkAssets.Plain(keyCols[tune[i]], 0.6f), default(Vector3), sheet);
            kit.Hint(stand + new Vector3(0f, 1.5f, 0f), new Vector3(3f, 3f, 3f), "A Song Sheet",
                "Seven coloured notes. Red, orange and yellow... just like the first three keys of the bridge. (Jump to play a key twice!)");
        }

        // ---------------------------------------------------------------- 4 & 5. Things to dig up

        static void DigSpots(LevelKit kit)
        {
            Vector3 hill = new Vector3(-72f, 4f, 7f);
            DigSpot.Place(kit, hill, SecretMarble.Place(kit, hill + Vector3.up * 1.3f, MarbleIds[3], MarbleColor(3), true));
            Vector3 ravine = new Vector3(-39.6f, -5f, -2f);
            DigSpot.Place(kit, ravine, SecretMarble.Place(kit, ravine + Vector3.up * 1.3f, MarbleIds[4], MarbleColor(4), true));
        }

        // ---------------------------------------------------------------- 6. Hide and seek

        static void HideAndSeek(LevelKit kit)
        {
            Vector3[] spots = { new Vector3(12f, 0f, 32.8f), new Vector3(-22f, 0f, 69.4f), new Vector3(-84f, 0f, -17.4f) };
            float[] yaws = { 180f, 0f, 0f };
            bool done = GameState.IsCollected(MarbleIds[5]);
            var jack = kit.Volume<Peekaboo>(spots[0] + Vector3.up * 0.7f, new Vector3(1.8f, 2f, 1.8f), "Peekaboo");
            jack.transform.rotation = Quaternion.Euler(0f, yaws[0], 0f);
            jack.Spots = spots;
            jack.Yaws = yaws;
            // The box: striped sides, a crank, and a springy clown head on top.
            var red = PatchworkAssets.Felt(new Color(0.9f, 0.3f, 0.35f));
            var yellow = PatchworkAssets.Felt(new Color(1f, 0.82f, 0.3f));
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.35f, 0f), new Vector3(0.9f, 0.7f, 0.9f), red, default(Vector3), jack.transform);
            for (int i = 0; i < 2; i++)
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.5f + i * 0.3f, 0f), new Vector3(0.92f, 0.08f, 0.92f), yellow, default(Vector3), jack.transform);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.03f, 0.55f), new Vector3(0.9f, 0.06f, 0.4f), red, new Vector3(-60f, 0f, 0f), jack.transform);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.5f, -0.35f, 0f), new Vector3(0.08f, 0.12f, 0.08f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f), new Vector3(0f, 0f, 90f), jack.transform);
            var head = kit.Group("Head", jack.transform.position);
            head.SetParent(jack.transform, true);
            head.localPosition = new Vector3(0f, 0.75f, 0f);
            for (int i = 0; i < 4; i++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.55f + i * 0.14f, 0f), new Vector3(0.28f, 0.03f, 0.28f), PatchworkAssets.Plain(new Color(0.75f, 0.75f, 0.8f), 0.8f), default(Vector3), head);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.55f, PatchworkAssets.Felt(new Color(1f, 0.92f, 0.85f)), default(Vector3), head);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.02f, -0.27f), Vector3.one * 0.12f, PatchworkAssets.Felt(new Color(0.95f, 0.25f, 0.3f)), default(Vector3), head);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.12f, 0.08f, -0.24f), Vector3.one * 0.07f, PatchworkAssets.Plain(UIKit.Ink, 0.7f), default(Vector3), head);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.3f, 0f), new Vector3(0.4f, 0.12f, 0.4f), PatchworkAssets.Felt(new Color(0.4f, 0.6f, 0.95f)), default(Vector3), head);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.45f, 0f), Vector3.one * 0.14f, yellow, default(Vector3), head);
            jack.Head = head;
            jack.Marble = SecretMarble.Place(kit, spots[2] + Vector3.up * 2.4f, MarbleIds[5], MarbleColor(5), true);
            if (done)
            {
                // Already found: it just sits by the spool tower, pleased with itself.
                jack.transform.position = spots[2] + Vector3.up * 0.7f;
                jack.Spots = new Vector3[0];
            }
        }

        // ---------------------------------------------------------------- 7. Bubble rush on Spindle Hill

        static void HillRush(LevelKit kit)
        {
            Vector3 btn = new Vector3(-67f, 0f, 13.8f);
            var rush = kit.Volume<BubbleRush>(btn + Vector3.up * 0.6f, new Vector3(1.8f, 1.4f, 1.8f), "BubbleRush");
            rush.Title = "Bubble Rush";
            rush.Duration = 16f;
            rush.Button = StopwatchButton(kit, btn, rush.transform);
            var purple = PatchworkAssets.Glow(new Color(0.75f, 0.5f, 1f), 0.8f);
            // A loop round the meadow under the lantern gate.
            Vector2[] path = { new Vector2(-76f, 14.5f), new Vector2(-70f, 16f), new Vector2(-64f, 14.5f), new Vector2(-58f, 16f), new Vector2(-58f, 22f),
                               new Vector2(-64f, 24f), new Vector2(-70f, 22.5f), new Vector2(-76f, 24f), new Vector2(-80f, 19f), new Vector2(-67f, 19f) };
            for (int i = 0; i < path.Length; i++)
            {
                var b = kit.Volume<RushBubble>(new Vector3(path[i].x, 1.2f, path[i].y), new Vector3(1.4f, 1.4f, 1.4f), "RushBubble");
                b.Owner = rush;
                var v = kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.7f, purple, default(Vector3), b.transform);
                v.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                b.gameObject.SetActive(false);
                rush.Bubbles.Add(b);
            }
            rush.Marble = SecretMarble.Place(kit, btn + new Vector3(0f, 2.2f, 0f), MarbleIds[6], MarbleColor(6), true);
            kit.Hint(btn + new Vector3(0f, 1.5f, -2.5f), new Vector3(4f, 3f, 3f), "Bubble Rush",
                "Step on the stopwatch, then catch every purple bubble before time runs out!");
        }

        // A big stopwatch you step on. Returns the part that presses down.
        static Transform StopwatchButton(LevelKit kit, Vector3 feet, Transform owner)
        {
            kit.Deco(PrimitiveType.Cylinder, feet + Vector3.up * 0.08f, new Vector3(1.8f, 0.08f, 1.8f), PatchworkAssets.Plain(new Color(0.85f, 0.85f, 0.9f), 0.8f));
            var top = kit.Group("ButtonTop", feet + Vector3.up * 0.18f);
            top.SetParent(owner, true);
            top.localPosition = new Vector3(0f, 0.18f, 0f);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.4f, 0.08f, 1.4f), PatchworkAssets.Plain(new Color(0.75f, 0.5f, 1f), 0.7f), default(Vector3), top);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.09f, 0.22f), new Vector3(0.08f, 0.02f, 0.45f), PatchworkAssets.Plain(Color.white, 0.6f), default(Vector3), top);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.15f, 0.09f, 0f), new Vector3(0.3f, 0.02f, 0.08f), PatchworkAssets.Plain(Color.white, 0.6f), default(Vector3), top);
            ClearGrass(feet, 1.2f);
            return top;
        }

        // ---------------------------------------------------------------- 8. Football

        static void Football(LevelKit kit)
        {
            Vector3 g = new Vector3(85.5f, 0f, -12f);
            var white = PatchworkAssets.Plain(new Color(0.97f, 0.97f, 0.95f), 0.6f);
            // Posts and crossbar on the mouth, a frame at the back, and a string net.
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 post = new Vector3(84.5f, 1.1f, g.z + s * 2.4f);
                kit.Deco(PrimitiveType.Cylinder, post, new Vector3(0.18f, 1.1f, 0.18f), white);
                Solid(kit, post, new Vector3(0.2f, 2.2f, 0.2f));
                kit.Deco(PrimitiveType.Cylinder, new Vector3(86.5f, 1.1f, g.z + s * 2.4f), new Vector3(0.1f, 1.1f, 0.1f), white);
            }
            kit.Deco(PrimitiveType.Cylinder, new Vector3(84.5f, 2.2f, g.z), new Vector3(0.18f, 2.45f, 0.18f), white, new Vector3(90f, 0f, 0f));
            var net = PatchworkAssets.Yarn(new Color(0.95f, 0.95f, 0.95f));
            for (int i = 0; i <= 8; i++)
            {
                float z = g.z - 2.4f + i * 0.6f;
                kit.Deco(PrimitiveType.Cylinder, new Vector3(86.5f, 1.1f, z), new Vector3(0.03f, 1.1f, 0.03f), net);
            }
            for (int i = 0; i <= 4; i++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(86.5f, 0.2f + i * 0.5f, g.z), new Vector3(0.03f, 2.4f, 0.03f), net, new Vector3(90f, 0f, 0f));
            Solid(kit, new Vector3(86.6f, 1.1f, g.z), new Vector3(0.1f, 2.2f, 4.8f));
            Solid(kit, new Vector3(85.5f, 1.1f, g.z - 2.5f), new Vector3(2f, 2.2f, 0.1f));
            Solid(kit, new Vector3(85.5f, 1.1f, g.z + 2.5f), new Vector3(2f, 2.2f, 0.1f));
            Solid(kit, new Vector3(85.5f, 2.3f, g.z), new Vector3(2f, 0.1f, 4.8f));
            // Pitch markings.
            var chalk = PatchworkAssets.Felt(new Color(0.98f, 0.98f, 0.95f));
            kit.Deco(PrimitiveType.Cube, new Vector3(82.5f, 0.02f, g.z), new Vector3(0.12f, 0.02f, 9f), chalk);
            kit.Deco(PrimitiveType.Cube, new Vector3(78.5f, 0.02f, g.z - 4.5f), new Vector3(8f, 0.02f, 0.12f), chalk);
            kit.Deco(PrimitiveType.Cube, new Vector3(78.5f, 0.02f, g.z + 4.5f), new Vector3(8f, 0.02f, 0.12f), chalk);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(74f, 0.02f, g.z), new Vector3(0.3f, 0.01f, 0.3f), chalk);
            ClearGrass(new Vector3(80f, 0f, g.z), 7f);
            var ball = Grabbable.BeachBall(kit, new Vector3(74f, 0f, g.z), 0.6f);
            var goal = new GameObject("GoalNet");
            goal.transform.SetParent(kit.Root, false);
            goal.transform.position = g;
            var gn = goal.AddComponent<GoalNet>();
            gn.Ball = ball;
            gn.Half = new Vector3(1f, 1.1f, 2.3f);
            gn.Marble = SecretMarble.Place(kit, new Vector3(82f, 1.8f, g.z), MarbleIds[7], MarbleColor(7), true);
            kit.Hint(new Vector3(74f, 1.5f, g.z - 3f), new Vector3(5f, 3f, 4f), "Toy Football",
                "Kick the ball into the net! Run into it, push it with {Grab}, or blow it along with Wind's gust.");
        }

        // ---------------------------------------------------------------- 9. Under the crate

        static void BarracksHatch(LevelKit kit)
        {
            Vector3 h = new Vector3(-11.5f, 0.06f, 52f);
            var wood = PatchworkAssets.Planks(new Color(0.55f, 0.4f, 0.28f));
            kit.Deco(PrimitiveType.Cube, h + Vector3.up * 0.01f, new Vector3(1.7f, 0.04f, 1.7f), PatchworkAssets.Plain(new Color(0.12f, 0.1f, 0.12f), 0.2f));
            var hatchGo = new GameObject("Hatch");
            hatchGo.transform.SetParent(kit.Root, false);
            hatchGo.transform.position = h;
            var lid = kit.Group("Lid", h + new Vector3(0f, 0.05f, 0.75f));
            lid.SetParent(hatchGo.transform, true);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, -0.75f), new Vector3(1.36f, 0.08f, 1.36f), wood, default(Vector3), lid);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.06f, -1.3f), new Vector3(0.25f, 0.02f, 0.25f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), default(Vector3), lid);
            var hatch = hatchGo.AddComponent<Hatch>();
            hatch.Lid = lid;
            hatch.Cover = Grabbable.Crate(kit, h, 1.5f, new Color(0.65f, 0.5f, 0.35f));
            hatch.Marble = SecretMarble.Place(kit, h + Vector3.up * 1.1f, MarbleIds[8], MarbleColor(8), true);
        }

        // ---------------------------------------------------------------- 10. Every set of chimes

        static void ChimeSecret(LevelKit kit)
        {
            var marble = SecretMarble.Place(kit, Bandstand + new Vector3(0f, 8.4f, 0f), MarbleIds[9], MarbleColor(9), true);
            var rungSet = new HashSet<WindChime>();
            int total = 6;
            WindChime.FirstRung = c =>
            {
                rungSet.Add(c);
                var game = PatchworkGame.Instance;
                if (rungSet.Count >= total) marble.Reveal();
                else if (game != null) game.ShowToast("Chimes rung: " + rungSet.Count + " / " + total);
            };
        }

        // ---------------------------------------------------------------- 11. Climb to the clock

        static void ClockClimb(LevelKit kit)
        {
            Vector3 btn = new Vector3(6.5f, 0f, 18.5f);
            var path = kit.Volume<StitchPath>(btn + Vector3.up * 0.6f, new Vector3(1.8f, 1.4f, 1.8f), "StitchSwitch");
            path.Title = "Stitched Steps";
            path.Duration = 24f;
            path.RevealMarbleOnWin = false;
            path.Button = StopwatchButton(kit, btn, path.transform);
            var pts = new List<Vector3>();
            for (int i = 0; i < 6; i++) pts.Add(new Vector3(-5f + i * 2f, 1.6f + i * 1.55f, 20.6f));
            for (int i = 6; i < 12; i++) pts.Add(new Vector3(5.8f, 1.6f + i * 1.55f, 21.6f + (i - 5) * 1.6f));
            pts.Add(new Vector3(5.8f, 1.6f + 11 * 1.55f, 31.2f));
            var quilt = new[] { PatchworkAssets.Felt(new Color(1f, 0.82f, 0.4f)), PatchworkAssets.Felt(new Color(0.55f, 0.8f, 1f)), PatchworkAssets.Felt(new Color(1f, 0.6f, 0.75f)) };
            var plats = new Transform[pts.Count];
            for (int i = 0; i < pts.Count; i++)
            {
                var p = kit.Block(pts[i], new Vector3(1.8f, 0.35f, 1.8f), Color.white, "StitchStep", quilt[i % 3]);
                for (int s = 0; s < 4; s++)
                {
                    var stitch = kit.Deco(PrimitiveType.Cube, Vector3.zero, Vector3.one, PatchworkAssets.Felt(Color.white), default(Vector3), p);
                    var face = Quaternion.Euler(0f, s * 90f, 0f);
                    stitch.localPosition = face * new Vector3(0f, 0.52f, 0.42f);
                    stitch.localRotation = face;
                    stitch.localScale = new Vector3(0.8f, 0.05f, 0.04f);
                }
                p.gameObject.SetActive(false);
                plats[i] = p;
            }
            path.Platforms = plats;
            path.Marble = SecretMarble.Place(kit, pts[pts.Count - 1] + Vector3.up * 1.3f, MarbleIds[10], MarbleColor(10));
            kit.Hint(btn + new Vector3(0f, 1.5f, -2.5f), new Vector3(4f, 3f, 3f), "A Funny Button",
                "Stitched steps appear for a little while. Climb them quickly!");
        }

        // ---------------------------------------------------------------- 12. The lost duckling

        static void LostDuckling(LevelKit kit)
        {
            Vector3 pond = new Vector3(14f, 0.1f, 74f);
            var go = new GameObject("Duckling");
            go.transform.SetParent(kit.Root, false);
            go.transform.position = new Vector3(-39.2f, -5f, 14f);
            var body = new GameObject("Body").transform;
            body.SetParent(go.transform, false);
            var yellow = PatchworkAssets.Plain(new Color(1f, 0.88f, 0.3f), 0.75f);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.22f, 0f), new Vector3(0.4f, 0.32f, 0.5f), yellow, default(Vector3), body);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.5f, 0.15f), Vector3.one * 0.28f, yellow, default(Vector3), body);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.47f, 0.31f), new Vector3(0.14f, 0.06f, 0.12f), PatchworkAssets.Plain(new Color(1f, 0.55f, 0.15f), 0.75f), default(Vector3), body);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.08f, 0.56f, 0.26f), Vector3.one * 0.05f, PatchworkAssets.Plain(UIKit.Ink, 0.7f), default(Vector3), body);
            var duck = go.AddComponent<Duckling>();
            duck.Body = body;
            duck.Home = pond;
            duck.Marble = SecretMarble.Place(kit, pond + new Vector3(0f, 2.2f, -6.5f), MarbleIds[11], MarbleColor(11), true);
            // Mama Duck, waiting on the pond.
            var mum = PathMover.Oval(pond, 1.6f, 1.2f, 20);
            Duck(kit, mum, 0f, 2.4f);
            kit.Hint(pond + new Vector3(0f, 1.5f, -7.5f), new Vector3(6f, 3f, 3f), "Mama Duck",
                "Quack! My duckling paddled off towards the ravine and hasn't come back. Could you find it?");
        }
    }
}

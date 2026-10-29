using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Level 1 - Toybox Town.
    //
    // A little Victorian town built inside a giant open toybox: cobbled streets, timber-framed
    // shops with fish-scale roofs, gas lamps, a bandstand in the square and a clock tower. Its
    // people are toys: tin soldiers, rag dolls, a nutcracker, a jack-in-the-box, a bunny or two,
    // and the town's mayor, a teddy bear in a top hat.
    //
    // Story: today is Mayor Bramblebutton's wedding to Lady Lacewing, but the Fray (the grey
    // static that unravels the world) has crept in and spoiled the town band's instruments.
    //   Pip's lute has lost its string     -> it's tangled on the old spinning wheel up Spindle Hill (Yarn)
    //   Brassby's trumpet valves are stuck -> a drop of valve dew in the Dye Works reservoir (Water)
    //   Wisp, the best piper in town, was blown away -> stranded on the weathervane over the rooftops (Wind)
    // Each one brings a new part of the town's music back. With the whole band home, the wedding
    // goes ahead and the music unravels the Fray's knot on the clock tower.
    //
    // Layout (ground level is y = 0 everywhere; nothing here is a floating platform over a void):
    //   Middle - Town Square: bandstand, wedding arch, fountain, shops, the clock tower (north)
    //   North  - the Toy Soldier barracks courtyard, through the clock tower's arch
    //   West   - across the ravine (clothesline swing) to Spindle Hill
    //   East   - over the bridge to the canal promenade and the Dye Works mill
    //   South  - the rooftop lane: steam and chimney smoke up to the weathervane spire
    // Extras: 5 Heartstrings (knots to pop, a thread race, Snarlings, the belfry, a cloud patch)
    // and 3 golden buttons (one per region, off the main path).
    public static partial class Level1Builder
    {
        public const string SceneName = "Level1";
        // Same ids as before so old saves keep their buttons.
        public static readonly string[] CollectibleIds = { "L1_fountain", "L1_swing", "L1_updraft" };
        public static readonly string[] HeartstringIds = { "L1_hs_knots", "L1_hs_race", "L1_hs_tangle", "L1_hs_belfry", "L1_hs_cloud", "L1_hs_bubbles" };
        public const int HeartstringsNeeded = 0;   // all optional in Toybox Town; the band is the goal

        static readonly Vector3 SpawnPoint = new Vector3(0f, 0f, -14f);
        static readonly Vector3 Bandstand = new Vector3(0f, 0f, 7f);
        static readonly Vector3 Tower = new Vector3(0f, 0f, 27f);
        static readonly Vector3 Mill = new Vector3(64f, 0f, 8f);
        const float RavineEast = -37f, RavineWest = -51f, CanalWest = 32f, CanalEast = 40f, Half = 90f;
        const float StreamWest = -46f, StreamEast = -42f, StreamSurface = -5.35f, CanalSurface = -0.6f;

        static readonly Color GrassCol = new Color(0.52f, 0.72f, 0.42f);
        static readonly Color CobbleCol = new Color(0.72f, 0.68f, 0.64f);
        static readonly Color[] Walls = { new Color(0.97f, 0.92f, 0.82f), new Color(0.95f, 0.85f, 0.78f), new Color(0.9f, 0.9f, 0.84f), new Color(0.98f, 0.88f, 0.7f), new Color(0.88f, 0.86f, 0.95f) };
        static readonly Color[] Roofs = { new Color(0.62f, 0.3f, 0.32f), new Color(0.3f, 0.42f, 0.6f), new Color(0.4f, 0.55f, 0.42f), new Color(0.55f, 0.4f, 0.6f), new Color(0.7f, 0.45f, 0.3f) };
        static readonly Color[] SpoolThreads = { LevelKit.Palette.Rose, LevelKit.Palette.Sky, LevelKit.Palette.Butter, LevelKit.Palette.Sage, LevelKit.Palette.Lavender };
        static readonly Color[] FlowerColors = { LevelKit.Palette.Rose, LevelKit.Palette.Butter, LevelKit.Palette.Lavender, LevelKit.Palette.Peach, LevelKit.Palette.Sky, new Color(1f, 0.6f, 0.7f) };

        static Level1Director director;

        public static LevelInfo Build(LevelKit kit)
        {
            var info = kit.Info;
            info.Id = SceneName;
            info.Title = "Level 1 · Toybox Town";
            info.Subtitle = "The Fray spoiled the band on the Mayor's wedding day. Help them play again!";
            info.Spawn = SpawnPoint + Vector3.up * 0.1f;
            info.SpawnYaw = 0f;
            info.KillY = -20f;
            info.FailY = -9f;   // the ravine and the canal are real places you can walk out of
            info.Sky = new Color(0.86f, 0.84f, 0.88f);
            // A big sunny nursery: soft haze far away, so the room beyond the box reads as huge.
            info.FogColor = new Color(0.9f, 0.84f, 0.82f);
            info.FogStart = 90f;
            info.FogEnd = 720f;
            info.FarClip = 900f;
            info.Saturation = 18f;
            info.Warmth = 9f;
            info.Glow = 0.55f;
            info.HeartstringsNeeded = HeartstringsNeeded;
            info.Area.SetMinMax(new Vector3(-Half, -6f, -Half), new Vector3(Half, 40f, Half));
            info.MapArea = new Bounds(Vector3.zero, new Vector3(Half * 2f, 10f, Half * 2f));
            info.MapTitle = "Toybox Town";
            info.MapNote = "Welcome to Toybox Town! The band needs a lute string, a drop of valve dew and their piper back before the Mayor's wedding can start.";
            var P = info.MapPins;
            P.Add(new MapPin("Town Square", new Vector3(-3f, 0f, -6f)));
            P.Add(new MapPin("Bandstand", Bandstand));
            P.Add(new MapPin("Clock Tower", Tower + new Vector3(4f, 0f, 0f)));
            P.Add(new MapPin("Toy Barracks", new Vector3(0f, 0f, 48f)));
            P.Add(new MapPin("Picnic Park", new Vector3(-12f, 0f, 74f)));
            P.Add(new MapPin("Duck Pond", new Vector3(14f, 0f, 74f)));
            P.Add(new MapPin("Thread Ravine", new Vector3((RavineEast + RavineWest) * 0.5f, 0f, -30f)));
            P.Add(new MapPin("Spindle Hill", new Vector3(-67f, 0f, 8f)));
            P.Add(new MapPin("Inkwell Canal", new Vector3((CanalWest + CanalEast) * 0.5f, 0f, 30f)));
            P.Add(new MapPin("Dye Works", Mill + new Vector3(0f, 0f, -6f)));
            P.Add(new MapPin("Tea House", new Vector3(53f, 0f, -30f), MapPin.Kind.Shop));
            P.Add(new MapPin("Toybox Express", new Vector3(70f, 0f, -51f)));
            P.Add(new MapPin("Chimney Lane", new Vector3(0f, 0f, -46f)));
            P.Add(new MapPin("Weathervane", new Vector3(0f, 0f, -72f)));
            P.Add(new MapPin("Lute String", new Vector3(-67f, 0f, 31f), MapPin.Kind.Band, Level1Director.LuteId));
            P.Add(new MapPin("Valve Dew", Mill + new Vector3(2.6f, 0f, 0.6f), MapPin.Kind.Band, Level1Director.TrumpetId));
            P.Add(new MapPin("Wisp the Piper", new Vector3(1.4f, 0f, -71.2f), MapPin.Kind.Band, Level1Director.PiperId));

            YarnGrass.ClearZones.Clear();
            var dirGo = new GameObject("Level1Director");
            dirGo.transform.SetParent(kit.Root, false);
            director = dirGo.AddComponent<Level1Director>();
            director.Ids = HeartstringIds;

            Ground(kit);
            Toybox(kit);
            TownSquare(kit);
            ClockTower(kit);
            Barracks(kit);
            SpindleHill(kit);
            Canal(kit);
            DyeWorks(kit);
            Rooftops(kit);
            Townsfolk(kit);
            Knots(kit);
            YarnBirds(kit);
            Details(kit);

            var sounds = kit.Group("LevelSounds", Vector3.zero);
            for (float z = -60f; z <= 60f; z += 30f)
                AmbientEmitter.Create(sounds, new Vector3(36f, -1.2f, z), Sfx.Ambient(Sfx.AmbientClip.Fountain), 0.22f, 3f, 16f);
            AmbientEmitter.Create(sounds, new Vector3(-44f, -4.5f, 0f), Sfx.Ambient(Sfx.AmbientClip.Fountain), 0.2f, 3f, 14f);
            AmbientEmitter.Create(sounds, Mill + new Vector3(-8.5f, 4f, 0f), Sfx.Ambient(Sfx.AmbientClip.Creak), 0.5f, 3f, 18f);
            AmbientEmitter.Create(sounds, new Vector3(0f, 26f, 27f), Sfx.Ambient(Sfx.AmbientClip.Clock), 0.4f, 4f, 30f);
            AmbientEmitter.Create(sounds, new Vector3(0f, 21f, -72f), Sfx.Ambient(Sfx.AmbientClip.Flag), 0.35f, 2f, 14f);

            director.ApplySavedState();
            IntroProps(kit);
            GroundCover(kit);
            return info;
        }

        // ================================================================ Ground, the toybox

        static void Ground(LevelKit kit)
        {
            var grass = PatchworkAssets.Grass(GrassCol);
            kit.Block(new Vector3((RavineEast + CanalWest) * 0.5f, -1f, 0f), new Vector3(CanalWest - RavineEast, 2f, Half * 2f), GrassCol, "TownGround", grass);
            kit.Block(new Vector3((CanalEast + Half) * 0.5f, -1f, 0f), new Vector3(Half - CanalEast, 2f, Half * 2f), GrassCol, "EastGround", grass);
            kit.Block(new Vector3((-Half + RavineWest) * 0.5f, -1f, 0f), new Vector3(RavineWest + Half, 2f, Half * 2f), GrassCol, "HillGround", grass);
            // The ravine: 5 m deep with cobbled banks, and a deep stream channel down the middle.
            // The canal: 5 m deep, brimming with water. Only Water Form can swim in them.
            var brick = PatchworkAssets.Brick(new Color(0.7f, 0.45f, 0.38f));
            var bankStone = PatchworkAssets.Cobble(new Color(0.6f, 0.58f, 0.55f));
            var deepBed = PatchworkAssets.Felt(new Color(0.12f, 0.2f, 0.42f));
            kit.Block(new Vector3((RavineWest + StreamWest) * 0.5f, -6.5f, 0f), new Vector3(StreamWest - RavineWest, 3f, Half * 2f), GrassCol, "RavineBank", bankStone);
            kit.Block(new Vector3((StreamEast + RavineEast) * 0.5f, -6.5f, 0f), new Vector3(RavineEast - StreamEast, 3f, Half * 2f), GrassCol, "RavineBank", bankStone);
            kit.Block(new Vector3((StreamWest + StreamEast) * 0.5f, -9f, 0f), new Vector3(StreamEast - StreamWest, 2f, Half * 2f), GrassCol, "StreamBed", deepBed);
            kit.Block(new Vector3(RavineEast - 0.1f, -2.5f, 0f), new Vector3(0.2f, 5f, Half * 2f), new Color(0.7f, 0.45f, 0.38f), "RavineWall", brick);
            kit.Block(new Vector3(RavineWest + 0.1f, -2.5f, 0f), new Vector3(0.2f, 5f, Half * 2f), new Color(0.7f, 0.45f, 0.38f), "RavineWall", brick);
            DeepWater(kit, StreamWest, StreamEast, StreamSurface, -8f, 0.05f);
            // Stepping-stone pillars across the stream.
            foreach (float sx in new[] { StreamWest + 1f, StreamEast - 1f })
                kit.Block(new Vector3(sx, -6.55f, -30f), new Vector3(1.2f, 3f, 1.2f), TownKit.Stone, "SteppingStone", bankStone);

            kit.Block(new Vector3((CanalWest + CanalEast) * 0.5f, -6f, 0f), new Vector3(CanalEast - CanalWest, 2f, Half * 2f), GrassCol, "CanalBed", deepBed);
            var stone = PatchworkAssets.Cobble(TownKit.Stone);
            kit.Block(new Vector3(CanalWest + 0.1f, -2.5f, 0f), new Vector3(0.2f, 5f, Half * 2f), TownKit.Stone, "CanalWall", stone);
            kit.Block(new Vector3(CanalEast - 0.1f, -2.5f, 0f), new Vector3(0.2f, 5f, Half * 2f), TownKit.Stone, "CanalWall", stone);
            DeepWater(kit, CanalWest + 0.2f, CanalEast - 0.2f, CanalSurface, -5f, 0.08f);
            // Steps back up out of the ravine (east side).
            TownKit.Stairs(kit, new Vector3(RavineEast - 1.5f, -5f, 6f), 0f, 5, 1f, 1.4f, 2.6f, stone);
            TownKit.Stairs(kit, new Vector3(RavineEast - 1.5f, -5f, -14f), 180f, 5, 1f, 1.4f, 2.6f, stone);
        }

        // Deep, slowly flowing water from x0 to x1 the whole length of the box: a crafted satin
        // surface (see-through, so the dark deep bed shows), lace foam along the edges, and the
        // swimming volume underneath.
        static void DeepWater(LevelKit kit, float x0, float x1, float surface, float bottom, float flow)
        {
            float w = x1 - x0, cx = (x0 + x1) * 0.5f;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = "DeepWater";
            go.transform.SetParent(kit.Root, false);
            go.transform.position = new Vector3(cx, surface - 0.03f, 0f);
            go.transform.localScale = new Vector3(w, 0.06f, Half * 2f);
            var mat = PatchworkAssets.CraftWater(0.86f, new Vector2(Mathf.Max(1f, w / 5f), Half * 2f / 5f));
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var drift = go.AddComponent<PaintLake>();
            drift.Drift = new Vector2(0f, flow);
            drift.Setup(mat);
            // A second, darker layer a little lower gives the water some depth.
            var under = kit.Deco(PrimitiveType.Cube, new Vector3(cx, surface - 0.9f, 0f), new Vector3(w, 0.05f, Half * 2f), PatchworkAssets.Bubble(new Color(0.12f, 0.3f, 0.6f, 0.5f)));
            under.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            // Lace foam where the water meets the walls.
            var lace = PatchworkAssets.Felt(new Color(0.96f, 0.98f, 1f));
            for (int s = -1; s <= 1; s += 2)
            {
                var strip = kit.Deco(PrimitiveType.Cube, new Vector3(cx + s * (w * 0.5f - 0.12f), surface + 0.015f, 0f), new Vector3(0.24f, 0.03f, Half * 2f), lace);
                strip.name = "Foam";
                var batch = new MeshBatch();
                for (float z = -Half + 0.3f; z < Half; z += 0.6f)
                    batch.Ball(new Vector3(cx + s * (w * 0.5f - 0.26f), surface + 0.02f, z), new Vector3(0.28f, 0.05f, 0.28f), 2, 6);
                batch.Build(kit.Root, "FoamScallops", lace);
            }
            var body = kit.Volume<WaterBody>(new Vector3(cx, (bottom + surface + 0.6f) * 0.5f, 0f), new Vector3(w, surface + 0.6f - bottom, Half * 2f), "WaterBody");
            body.SurfaceY = surface;
        }

        // The walls of the giant toybox, papered inside with damask, brass corners, and the lid
        // propped open behind the town.
        static void Toybox(LevelKit kit)
        {
            var paper = PatchworkAssets.Damask(new Color(0.32f, 0.36f, 0.58f));
            var wood = PatchworkAssets.Planks(new Color(0.55f, 0.36f, 0.26f));
            float h = 44f, t = 2f;
            // The walls go down below the ravine and canal beds so there's never a gap under them.
            float wh = h + 8f, wy = (42f + -10f) * 0.5f;
            kit.Block(new Vector3(Half + t * 0.5f, wy, 0f), new Vector3(t, wh, Half * 2f + t * 2f), new Color(0.32f, 0.36f, 0.58f), "BoxWall", paper);
            kit.Block(new Vector3(-Half - t * 0.5f, wy, 0f), new Vector3(t, wh, Half * 2f + t * 2f), new Color(0.32f, 0.36f, 0.58f), "BoxWall", paper);
            kit.Block(new Vector3(0f, wy, Half + t * 0.5f), new Vector3(Half * 2f, wh, t), new Color(0.32f, 0.36f, 0.58f), "BoxWall", paper);
            kit.Block(new Vector3(0f, wy, -Half - t * 0.5f), new Vector3(Half * 2f, wh, t), new Color(0.32f, 0.36f, 0.58f), "BoxWall", paper);
            // Wooden skirting and a brass rim along the top.
            var brass = PatchworkAssets.Plain(new Color(0.9f, 0.72f, 0.38f), 0.8f);
            foreach (var side in new[] { new Vector3(1f, 0f, 0f), new Vector3(-1f, 0f, 0f), new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, -1f) })
            {
                bool alongZ = side.x != 0f;
                Vector3 c = side * (Half - 0.15f);
                Vector3 len = alongZ ? new Vector3(0.3f, 0f, Half * 2f) : new Vector3(Half * 2f, 0f, 0.3f);
                kit.Deco(PrimitiveType.Cube, c + Vector3.up * 0.8f, len + Vector3.up * 1.6f, wood);
                kit.Deco(PrimitiveType.Cube, side * (Half + 1f) + Vector3.up * (h - 1.6f), (alongZ ? new Vector3(2.6f, 0f, Half * 2f + 5f) : new Vector3(Half * 2f + 5f, 0f, 2.6f)) + Vector3.up * 0.8f, brass);
            }
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    kit.Deco(PrimitiveType.Sphere, new Vector3(sx * (Half + 1f), h - 1.2f, sz * (Half + 1f)), Vector3.one * 4f, brass);
            // The lid, propped open behind the north wall.
            var lid = kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(Half * 2f + 4f, 2f, 110f), PatchworkAssets.Damask(new Color(0.4f, 0.28f, 0.42f)));
            lid.rotation = Quaternion.Euler(-72f, 0f, 0f);
            lid.position = new Vector3(0f, h - 2f, Half + 2f) + lid.rotation * new Vector3(0f, 0f, 55f);
            for (int i = 0; i < 6; i++)
            {
                float a = i / 6f * Mathf.PI * 2f;
                kit.Cloud(new Vector3(Mathf.Cos(a) * 60f, 36f + (i % 2) * 3f, Mathf.Sin(a) * 60f), 2.6f);
            }
        }

        // ================================================================ The town square

        static void TownSquare(LevelKit kit)
        {
            var cobble = PatchworkAssets.Cobble(CobbleCol);
            kit.Prism(Octagon(Vector3.zero, 21f), -0.3f, 0.06f, cobble, "TownSquare", 2.5f);
            // Streets out of the square (1 cm lower than the square so the two never flicker).
            kit.Block(new Vector3(0f, -0.1f, -30f), new Vector3(10f, 0.3f, 22f), CobbleCol, "SouthStreet", cobble);
            kit.Block(new Vector3(-28f, -0.1f, 0f), new Vector3(18f, 0.3f, 10f), CobbleCol, "WestStreet", cobble);
            kit.Block(new Vector3(25.5f, -0.1f, 0f), new Vector3(13f, 0.3f, 10f), CobbleCol, "EastStreet", cobble);
            kit.AddCheckpoint(SpawnPoint, 0f);
            kit.Hint(SpawnPoint + new Vector3(0f, 1.5f, 0f), new Vector3(10f, 4f, 6f), "Toybox Town",
                "It's the Mayor's wedding day, but the Fray has spoiled the town band's instruments! Press {Ability} to talk to the toys, and {Map} to open the town map.");

            // ----- The bandstand -----
            Vector3 b = Bandstand;
            var planks = PatchworkAssets.Planks(new Color(0.75f, 0.55f, 0.38f));
            kit.Prism(Octagon(b, 5.6f), 0f, 0.9f, planks, "Bandstand", 2f);
            kit.Block(b + new Vector3(0f, 0.3f, -6f), new Vector3(3f, 0.6f, 1.2f), new Color(0.75f, 0.55f, 0.38f), "BandstandStep", planks);
            var white = PatchworkAssets.Wood(new Color(0.97f, 0.95f, 0.9f));
            var tops = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                float a = (i + 0.5f) / 8f * Mathf.PI * 2f;
                Vector3 p = b + new Vector3(Mathf.Cos(a) * 5.1f, 0.9f, Mathf.Sin(a) * 5.1f);
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 1.6f, new Vector3(0.24f, 1.6f, 0.24f), white);
                Solid(kit, p + Vector3.up * 1.6f, new Vector3(0.3f, 3.2f, 0.3f));
                tops[i] = p + Vector3.up * 3.2f;
            }
            for (int i = 0; i < 8; i++)
            {
                var a = tops[i]; var c = tops[(i + 1) % 8];
                // Railings, except at the front where the steps are.
                if (!(a.z < b.z - 3f && c.z < b.z - 3f))
                    kit.Rope(a + Vector3.down * 2.3f, c + Vector3.down * 2.3f, white, 0.1f);
            }
            var stripeA = PatchworkAssets.Felt(new Color(0.8f, 0.28f, 0.32f));
            var stripeB = PatchworkAssets.Felt(new Color(0.97f, 0.93f, 0.85f));
            kit.Prism(Octagon(b + Vector3.up * 4.1f, 6.3f), 4.1f, 4.5f, stripeA, "BandstandRoof", 2f);
            kit.Prism(Octagon(b, 4.5f), 4.5f, 5.3f, stripeB, "BandstandRoof2", 2f);
            kit.Prism(Octagon(b, 2.6f), 5.3f, 6.2f, stripeA, "BandstandRoof3", 2f);
            kit.Deco(PrimitiveType.Sphere, b + Vector3.up * 6.6f, Vector3.one * 0.8f, PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f));
            for (int i = 0; i < 8; i++) kit.Bunting(tops[i] + Vector3.down * 0.3f, tops[(i + 1) % 8] + Vector3.down * 0.3f, LevelKit.Palette.Quilt);

            // ----- The wedding arch -----
            director.ArchCenter = new Vector3(0f, 0f, -2f);
            TownKit.FlowerArch(kit, director.ArchCenter, 0f, 4.4f);
            director.MayorSpot = new Vector3(-2.2f, 0.06f, -2.4f);
            director.BrideSpot = new Vector3(2.2f, 0.06f, -2.4f);
            // A rose-petal aisle.
            for (int i = 0; i < 8; i++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(((i * 37) % 10) / 10f - 0.5f, 0.07f, -4f - i * 1.1f), new Vector3(0.25f, 0.01f, 0.18f), PatchworkAssets.Felt(LevelKit.Palette.Rose), new Vector3(0f, i * 40f, 0f));

            // ----- The fountain (rides Water up to the pigeon loft) -----
            kit.Fountain(new Vector3(-11f, 0.06f, -8f), 7f, LevelKit.FountainStyle.Stone);
            kit.Block(new Vector3(-11f, 4.75f, -10.8f), new Vector3(3f, 0.5f, 3f), new Color(0.75f, 0.55f, 0.38f), "PigeonLoft", planks);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-11f, 2.25f, -10.8f), new Vector3(0.4f, 2.25f, 0.4f), PatchworkAssets.Wood(TownKit.DarkWood));
            kit.Thread(new Vector3(-10.2f, 5.6f, -11.4f), LevelKit.Palette.Sky);
            kit.Thread(new Vector3(-11.8f, 5.6f, -10.4f), LevelKit.Palette.Sky);
            kit.Hint(new Vector3(-11f, 1.5f, -6f), new Vector3(4f, 3f, 3.4f), "Fountains",
                "Water Form rides fountains up and slips through drains. Teapots and pipes spout too!", HintTheme.Water);

            // ----- Shops round the square -----
            var bakery = TownKit.MakeHouse(kit, new Vector3(-12f, 0f, 27f), new Vector3(9f, 7f, 8f), 180f, Walls[3], Roofs[0], 1, -1, -3f, TownKit.Sign.Bread);
            kit.Updraft(bakery.ChimneyTop, 8f, LevelKit.UpdraftStyle.Chimney);
            TownKit.MakeHouse(kit, new Vector3(12f, 0f, 27f), new Vector3(9f, 7f, 8f), 180f, Walls[0], Roofs[1], 2, 1, 2f, TownKit.Sign.Toy);
            TownKit.MakeHouse(kit, new Vector3(-24f, 0f, 13f), new Vector3(10f, 7f, 9f), 90f, Walls[4], Roofs[3], 3, 1, 2f, TownKit.Sign.Spool);
            TownKit.MakeHouse(kit, new Vector3(-24f, 0f, -14f), new Vector3(10f, 6f, 9f), 90f, Walls[1], Roofs[2], 4, 0, 0f, TownKit.Sign.Teacup);
            TownKit.MakeHouse(kit, new Vector3(24f, 0f, 13f), new Vector3(10f, 7f, 9f), -90f, Walls[2], Roofs[4], 5, -1, 1f, TownKit.Sign.Hat);
            TownKit.MakeHouse(kit, new Vector3(24f, 0f, -13f), new Vector3(10f, 6f, 9f), -90f, Walls[0], Roofs[0], 6, 0, 0f, TownKit.Sign.Clock);
            TownKit.MakeHouse(kit, new Vector3(-13f, 0f, -26f), new Vector3(9f, 6f, 8f), 0f, Walls[3], Roofs[1], 7, 1, -2f, TownKit.Sign.None);
            TownKit.MakeHouse(kit, new Vector3(13f, 0f, -26f), new Vector3(9f, 6f, 8f), 0f, Walls[2], Roofs[3], 8, 0, 0f, TownKit.Sign.Dye);
            // Gas lamps, benches and flowers.
            for (int i = 0; i < 8; i++)
            {
                float a = (i + 0.5f) / 8f * Mathf.PI * 2f;
                TownKit.StreetLamp(kit, new Vector3(Mathf.Cos(a) * 17.5f, 0.06f, Mathf.Sin(a) * 17.5f));
            }
            // (Kept clear of the ring of street lamps.)
            TownKit.Bench(kit, new Vector3(-11f, 0.06f, -14.2f), 0f);
            TownKit.Bench(kit, new Vector3(11f, 0.06f, -14.2f), 0f);
            TownKit.Bench(kit, new Vector3(-15f, 0.06f, 2f), 90f);
            TownKit.Bench(kit, new Vector3(15f, 0.06f, 2f), -90f);
            TownKit.FlowerBed(kit, new Vector3(-12f, 0.06f, 12f), new Vector2(3f, 2f), 1);
            TownKit.FlowerBed(kit, new Vector3(12f, 0.06f, 12f), new Vector2(3f, 2f), 2);
            TownKit.FlowerBed(kit, new Vector3(13f, 0.06f, -9f), new Vector2(2.5f, 2.5f), 3);
            TownKit.ToyCart(kit, new Vector3(8f, 0.06f, 15f), 30f);
            TownKit.Crate(kit, new Vector3(-16.5f, 0.06f, -10.5f), 1.2f, 15f);
            TownKit.Crate(kit, new Vector3(-17.4f, 0.06f, -9.2f), 0.9f, -10f);
            kit.ThreadLine(new Vector3(-3f, 1f, -10f), new Vector3(3f, 1f, -10f), 3);
            // Bunting across the square from lamp to lamp.
            kit.Bunting(new Vector3(-12.4f, 5.5f, -12.4f), new Vector3(12.4f, 5.5f, -12.4f), LevelKit.Palette.Quilt);
            kit.Bunting(new Vector3(-12f, 5.5f, 16f), new Vector3(12f, 5.5f, 16f), LevelKit.Palette.Quilt);

            // The Fray has got into the square too: tears and tendrils that go at the wedding.
            var square = new GameObject("Region_Square").AddComponent<FrayRegion>();
            square.transform.SetParent(kit.Root, false);
            director.SquareRegion = square;
            FrayTear(kit, square, new Vector3(6f, 0.06f, -12f), 2f);
            FrayTear(kit, square, new Vector3(-14f, 0.06f, 2f), 1.8f);
            FrayTendril(kit, square, b + new Vector3(4.4f, 0.9f, 2.6f), 2.6f);
            FrayTendril(kit, square, new Vector3(16f, 0.06f, -4f), 3f);
            Blooms(kit, square, new Vector3(0f, 0.06f, -8f), new Vector2(14f, 4f), 14);
        }

        // ================================================================ Clock tower (north)

        static void ClockTower(LevelKit kit)
        {
            Vector3 t = Tower;
            var brick = PatchworkAssets.Brick(new Color(0.72f, 0.45f, 0.36f));
            var stone = PatchworkAssets.Cobble(TownKit.Stone);
            Color bc = new Color(0.72f, 0.45f, 0.36f);
            // Two piers with an archway between them (the way to the barracks), then the tall tower.
            kit.Block(t + new Vector3(-3f, 3f, 0f), new Vector3(2f, 6f, 8f), bc, "TowerPier", brick);
            kit.Block(t + new Vector3(3f, 3f, 0f), new Vector3(2f, 6f, 8f), bc, "TowerPier", brick);
            kit.Block(t + new Vector3(0f, 5f, 0f), new Vector3(2f, 2f, 8f), bc, "TowerArch", brick);
            kit.Block(t + new Vector3(0f, 14.5f, 0f), new Vector3(8f, 17f, 8f), bc, "Tower", brick);
            kit.Deco(PrimitiveType.Cube, t + new Vector3(0f, 6.1f, 0f), new Vector3(8.4f, 0.4f, 8.4f), stone);
            kit.Deco(PrimitiveType.Cube, t + new Vector3(0f, 23.2f, 0f), new Vector3(8.6f, 0.5f, 8.6f), stone);
            // The belfry: four columns, a bell and a pointed roof.
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    kit.Block(t + new Vector3(sx * 3.4f, 25.7f, sz * 3.4f), new Vector3(1.2f, 4.6f, 1.2f), bc, "BelfryColumn", brick);
            // The bell hangs from a pivot so it can swing; glide into the belfry to ring it.
            var bellPivot = kit.Group("BellPivot", t + new Vector3(0f, 27.6f, 0f));
            var brass = PatchworkAssets.Plain(new Color(0.9f, 0.7f, 0.35f), 0.85f);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -1.6f, 0f), new Vector3(2.4f, 2.8f, 2.4f), brass, default(Vector3), bellPivot);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -2.9f, 0f), new Vector3(2.7f, 0.12f, 2.7f), brass, default(Vector3), bellPivot);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -3.1f, 0f), Vector3.one * 0.45f, PatchworkAssets.Plain(TownKit.Iron, 0.5f), default(Vector3), bellPivot);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.2f, 0f), new Vector3(0.25f, 0.25f, 0.25f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), default(Vector3), bellPivot);
            var ringer = kit.Volume<BellRinger>(t + new Vector3(0f, 25.8f, 0f), new Vector3(8f, 5.6f, 8f), "BellRinger");
            ringer.Bell = bellPivot;
            kit.Block(t + new Vector3(0f, 28.3f, 0f), new Vector3(8.8f, 0.6f, 8.8f), bc, "BelfryRoof", stone);
            var slate = PatchworkAssets.Shingle(new Color(0.32f, 0.4f, 0.55f));
            for (int i = 0; i < 4; i++)
            {
                float s = 7f - i * 1.7f;
                var tier = kit.Deco(PrimitiveType.Cube, t + new Vector3(0f, 29.1f + i * 1.2f, 0f), new Vector3(s, 1.2f, s), slate);
                tier.rotation = Quaternion.Euler(0f, i * 12f, 0f);
            }
            kit.Deco(PrimitiveType.Cylinder, t + new Vector3(0f, 35f, 0f), new Vector3(0.12f, 1.4f, 0.12f), PatchworkAssets.Plain(TownKit.Iron, 0.6f));
            kit.Deco(PrimitiveType.Sphere, t + new Vector3(0f, 36.4f, 0f), Vector3.one * 0.5f, PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f));
            // Clock faces on all four sides.
            for (int i = 0; i < 4; i++)
            {
                var r = Quaternion.Euler(0f, i * 90f, 0f);
                Vector3 face = t + Vector3.up * 19f + r * new Vector3(0f, 0f, -4.08f);
                kit.Deco(PrimitiveType.Cylinder, face, new Vector3(4.6f, 0.08f, 4.6f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f), new Vector3(90f, i * 90f, 0f));
                kit.Deco(PrimitiveType.Cylinder, face + r * new Vector3(0f, 0f, -0.06f), new Vector3(4f, 0.06f, 4f), PatchworkAssets.Felt(new Color(0.98f, 0.95f, 0.85f)), new Vector3(90f, i * 90f, 0f));
                // Working hands: the minute hand goes round once a minute, the hour hand slowly.
                for (int hnd = 0; hnd < 2; hnd++)
                {
                    var pivot = kit.Group("ClockHand", face + r * new Vector3(0f, 0f, -0.15f - hnd * 0.02f));
                    pivot.rotation = r * Quaternion.Euler(0f, 0f, i * 37f + hnd * 120f);
                    float len = hnd == 0 ? 1.4f : 0.9f;
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, len * 0.42f, 0f), new Vector3(hnd == 0 ? 0.12f : 0.2f, len, 0.05f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), default(Vector3), pivot);
                    var turn = pivot.gameObject.AddComponent<Spinner>();
                    turn.Axis = Vector3.back;
                    turn.Speed = hnd == 0 ? 6f : 0.5f;
                }
                kit.Deco(PrimitiveType.Cylinder, face + r * new Vector3(0f, 0f, -0.2f), new Vector3(0.25f, 0.04f, 0.25f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f), new Vector3(90f, i * 90f, 0f));
                for (int n = 0; n < 12; n++)
                {
                    float a = n / 12f * Mathf.PI * 2f;
                    kit.Deco(PrimitiveType.Sphere, face + r * new Vector3(Mathf.Sin(a) * 1.65f, Mathf.Cos(a) * 1.65f, -0.1f), Vector3.one * 0.16f, PatchworkAssets.Plain(TownKit.Iron, 0.5f));
                }
            }
            // A balcony round the tower (the belfry Heartstring is up here).
            var planks = PatchworkAssets.Planks(new Color(0.6f, 0.42f, 0.3f));
            kit.Block(t + new Vector3(0f, 15.75f, -4.7f), new Vector3(8f, 0.5f, 1.4f), bc, "Balcony", planks);
            kit.Block(t + new Vector3(0f, 15.75f, 4.7f), new Vector3(8f, 0.5f, 1.4f), bc, "Balcony", planks);
            kit.Block(t + new Vector3(-4.7f, 15.75f, 0f), new Vector3(1.4f, 0.5f, 10.8f), bc, "Balcony", planks);
            kit.Block(t + new Vector3(4.7f, 15.75f, 0f), new Vector3(1.4f, 0.5f, 10.8f), bc, "Balcony", planks);
            MakeHeartstring(kit, "L1_hs_belfry", "Belfry Thread", t + new Vector3(-4.7f, 17.5f, 3.5f), false);
            kit.Thread(t + new Vector3(-4.7f, 16.8f, -1f), LevelKit.Palette.Gold);
            kit.Thread(t + new Vector3(-4.7f, 16.8f, -3f), LevelKit.Palette.Gold);
            kit.Hint(new Vector3(-9f, 1.5f, 20f), new Vector3(5f, 4f, 4f), "Chimney Smoke",
                "Wind Form can ride the smoke from chimneys and the steam from vents, right up over the rooftops.", HintTheme.Wind);

            // The Fray's knot, wrapped round the south clock face.
            var knot = kit.Group("TowerKnot", t + new Vector3(0f, 19f, -4.9f));
            KnotShape(kit, knot, 2.6f, 24);
            var lightGo = new GameObject("KnotGlow");
            lightGo.transform.SetParent(knot, false);
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = SnarlKnot.FrayGlow;
            l.range = 12f;
            l.intensity = 1.6f;
            l.shadows = LightShadows.None;
            // Grey threads dangling from it down the tower.
            var grey = PatchworkAssets.Yarn(SnarlKnot.FrayGrey);
            for (int i = 0; i < 5; i++)
            {
                var strand = kit.Deco(PrimitiveType.Cylinder, new Vector3(-1.6f + i * 0.8f, -2.5f - (i % 2), 0.2f), new Vector3(0.12f, 2.2f + (i % 3) * 0.5f, 0.12f), grey, new Vector3(0f, 0f, (i - 2) * 6f), knot);
                strand.name = "FrayStrand";
            }
            director.TowerKnot = knot;
        }

        // ================================================================ Barracks (north)

        static void Barracks(LevelKit kit)
        {
            var stone = PatchworkAssets.Cobble(TownKit.Stone);
            kit.Block(new Vector3(0f, -0.1f, 45f), new Vector3(30f, 0.3f, 26f), CobbleCol, "BarracksYard", stone);
            kit.Block(new Vector3(-15.5f, 1.5f, 45f), new Vector3(1f, 3f, 26f), TownKit.Stone, "BarracksWall", stone);
            kit.Block(new Vector3(15.5f, 1.5f, 45f), new Vector3(1f, 3f, 26f), TownKit.Stone, "BarracksWall", stone);
            kit.Block(new Vector3(0f, 1.5f, 58.5f), new Vector3(32f, 3f, 1f), TownKit.Stone, "BarracksWall", stone);
            // Striped sentry boxes, a flagpole and toy cannons.
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 p = new Vector3(s * 5.5f, 0f, 33f);
                kit.Block(p + Vector3.up * 1.4f, new Vector3(1.6f, 2.8f, 1.6f), new Color(0.85f, 0.25f, 0.28f), "SentryBox", PatchworkAssets.Planks(new Color(0.85f, 0.25f, 0.28f)));
                for (int i = 0; i < 3; i++) kit.Deco(PrimitiveType.Cube, p + Vector3.up * (0.6f + i * 0.9f), new Vector3(1.64f, 0.25f, 1.64f), PatchworkAssets.Felt(new Color(0.97f, 0.95f, 0.9f)));
                kit.Deco(PrimitiveType.Cube, p + Vector3.up * 3f, new Vector3(2f, 0.3f, 2f), PatchworkAssets.Planks(new Color(0.25f, 0.3f, 0.5f)));
            }
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-11f, 5f, 54f), new Vector3(0.2f, 5f, 0.2f), PatchworkAssets.Plain(TownKit.Iron, 0.5f));
            var flag = kit.Group("BarracksFlag", new Vector3(-11f, 8.8f, 54f));
            kit.Deco(PrimitiveType.Cube, new Vector3(1.2f, 0f, 0f), new Vector3(2.4f, 1.4f, 0.05f), PatchworkAssets.Felt(new Color(0.85f, 0.25f, 0.28f)), default(Vector3), flag);
            kit.Deco(PrimitiveType.Cube, new Vector3(1.2f, 0f, -0.01f), new Vector3(2.4f, 0.3f, 0.06f), PatchworkAssets.Felt(new Color(0.97f, 0.95f, 0.9f)), default(Vector3), flag);
            flag.gameObject.AddComponent<FlagWave>();
            for (int i = 0; i < 2; i++)
            {
                Vector3 c = new Vector3(8f + i * 3f, 0f, 55f);
                kit.Deco(PrimitiveType.Cylinder, c + new Vector3(0f, 0.9f, -0.4f), new Vector3(0.6f, 1f, 0.6f), PatchworkAssets.Plain(new Color(0.3f, 0.3f, 0.34f), 0.7f), new Vector3(70f, 0f, 0f));
                kit.Deco(PrimitiveType.Cylinder, c + new Vector3(-0.45f, 0.45f, 0f), new Vector3(0.9f, 0.08f, 0.9f), PatchworkAssets.Wood(new Color(0.85f, 0.25f, 0.28f)), new Vector3(0f, 0f, 90f));
                kit.Deco(PrimitiveType.Cylinder, c + new Vector3(0.45f, 0.45f, 0f), new Vector3(0.9f, 0.08f, 0.9f), PatchworkAssets.Wood(new Color(0.85f, 0.25f, 0.28f)), new Vector3(0f, 0f, 90f));
            }
            TownKit.Crate(kit, new Vector3(-12f, 0.06f, 40f), 1.4f, 5f);
            TownKit.Crate(kit, new Vector3(-12.6f, 1.46f, 40.2f), 1f, -20f);
            var foes = new List<Snarling>
            {
                Snarling.Create(kit.Root, new Vector3(-7f, 0.06f, 40f), new Vector3(7f, 0.06f, 40f)),
                Snarling.Create(kit.Root, new Vector3(6f, 0.06f, 46f), new Vector3(-6f, 0.06f, 46f)),
                Snarling.Create(kit.Root, new Vector3(-5f, 0.06f, 52f), new Vector3(5f, 0.06f, 52f))
            };
            Challenge(foes, MakeHeartstring(kit, "L1_hs_tangle", "Tin Tangle", new Vector3(0f, 1.7f, 46f), true));
            kit.Hint(new Vector3(0f, 1.5f, 34.5f), new Vector3(8f, 4f, 3f), "Snarlings!",
                "The Fray's little creatures have taken over the barracks. Jump on their heads to unravel them!");
            kit.AddCheckpoint(new Vector3(0f, 0.06f, 35.5f), 0f);
        }

        // ================================================================ West: Spindle Hill (Yarn)

        static void SpindleHill(LevelKit kit)
        {
            var region = MakeRegion(kit, "SpindleHill", Level1Director.LuteId);
            var stone = PatchworkAssets.Cobble(TownKit.Stone);
            // Houses along the west street.
            TownKit.MakeHouse(kit, new Vector3(-33.3f, 0f, 12f), new Vector3(6f, 6f, 7f), 180f, Walls[2], Roofs[2], 11, 1, 1f);
            TownKit.MakeHouse(kit, new Vector3(-33.3f, 0f, -12f), new Vector3(6f, 7f, 7f), 0f, Walls[0], Roofs[4], 12, -1, -1f);
            TownKit.StreetLamp(kit, new Vector3(-34f, 0f, 5.5f));
            TownKit.StreetLamp(kit, new Vector3(-34f, 0f, -5.5f));
            kit.AddCheckpoint(new Vector3(-34.5f, 0f, 0f), -90f);
            kit.Hint(new Vector3(-34.5f, 1.5f, 0f), new Vector3(4f, 4f, 9f), "Swing on Anything Pink",
                "As Yarn, hold {Ability} near anything wrapped in pink yarn to swing: pegs on a clothesline, lanterns, big knots. Let go to fly!", HintTheme.Yarn);

            // The clothesline over the ravine. The golden button hangs under the middle of the swing.
            TownKit.Clothesline(kit, new Vector3(RavineEast + 0.6f, 0f, 1.5f), new Vector3(RavineWest - 0.6f, 0f, 1.5f), 9.4f, 1.4f, new[] { 0.5f });
            kit.AddCameraZone(new Vector3((RavineEast + RavineWest) * 0.5f, 2f, 0f), new Vector3(20f, 22f, 16f), 2.5f, 6f);
            kit.GoldenButton(CollectibleIds[1], new Vector3((RavineEast + RavineWest) * 0.5f, -0.4f, 1.5f));
            SwingThreadsX(kit, (RavineEast + RavineWest) * 0.5f, 8f, 1.5f);

            // The hill village (ground level): a cottage, yarn trees and a pincushion bounce.
            kit.AddCheckpoint(new Vector3(RavineWest - 3f, 0f, 1.5f), -90f);
            TownKit.MakeHouse(kit, new Vector3(-64f, 0f, -12f), new Vector3(9f, 6f, 8f), 0f, Walls[4], Roofs[0], 13, 1, 2f, TownKit.Sign.Spool);
            kit.YarnTree(new Vector3(-56f, 0f, -6f), 4f, LevelKit.Palette.Sage);
            kit.YarnTree(new Vector3(-80f, 0f, -4f), 5f, LevelKit.Palette.Rose);
            kit.YarnTree(new Vector3(-82f, 0f, 14f), 4.5f, LevelKit.Palette.Butter);
            kit.YarnTree(new Vector3(-56f, 0f, 22f), 3.6f, LevelKit.Palette.Lavender);
            TownKit.FlowerBed(kit, new Vector3(-58f, 0f, -18f), new Vector2(3f, 2f), 4);
            kit.SpoolTower(new Vector3(-84f, 0f, -20f), 6f, SpoolThreads);
            kit.Spool(new Vector3(-78f, 0f, -16f), 1.6f, LevelKit.Palette.Sky);
            kit.AddBouncePad(new Vector3(-67f, 0f, 1.5f), 16f);
            kit.Thread(new Vector3(-67f, 3f, 1.5f), LevelKit.Palette.Gold);
            // Terrace 1 (top 4) and the summit (top 4) across a 14 m gap, with a lantern gate to swing on.
            var hillMat = PatchworkAssets.Knit(new Color(0.55f, 0.72f, 0.45f));
            kit.Block(new Vector3(-67f, 2f, 8f), new Vector3(14f, 4f, 8f), GrassCol, "HillTerrace", hillMat);
            kit.Hint(new Vector3(-67f, 5.5f, 8f), new Vector3(14f, 3f, 8f), "The Lantern Gate",
                "Swing from the lantern to the old spinning wheel. Fall short and you'll just land in the meadow.", HintTheme.Yarn);
            TownKit.LanternGate(kit, new Vector3(-71.5f, 0f, 19f), new Vector3(-62.5f, 0f, 19f), 14.4f, 12f);
            kit.AddCameraZone(new Vector3(-67f, 6f, 19f), new Vector3(16f, 20f, 18f), 2.5f, 6f);
            SwingThreadsZ(kit, -67f, 12f, 19f);
            kit.Block(new Vector3(-67f, 2f, 32f), new Vector3(14f, 4f, 12f), GrassCol, "HillSummit", hillMat);
            SpinningWheel(kit, new Vector3(-67f, 4f, 35.5f));
            BandPieceAt(kit, Level1Director.LuteId, BandPiece.Part.Lute, "The Lute String", new Vector3(-67f, 5.6f, 31f), null);
            MakeHomeHoop(kit, new Vector3(-62.5f, 4f, 29f));
            // The Fray up here.
            FrayTendril(kit, region, new Vector3(-71f, 4f, 30f), 3.2f);
            FrayTendril(kit, region, new Vector3(-63f, 4f, 36f), 2.8f);
            FrayTear(kit, region, new Vector3(-69f, 4f, 28.5f), 2f);
            FrayTear(kit, region, new Vector3(-60f, 0f, -2f), 2.2f);
            FrayTendril(kit, region, new Vector3(-74f, 0f, 6f), 3f);
            Blooms(kit, region, new Vector3(-67f, 4f, 31f), new Vector2(5f, 3f), 8);
            Blooms(kit, region, new Vector3(-66f, 0f, -2f), new Vector2(8f, 6f), 12);
            Snarling.Create(kit.Root, new Vector3(-62f, 0f, -4f), new Vector3(-72f, 0f, -4f));
        }

        static void SwingThreadsX(LevelKit kit, float anchorX, float anchorY, float z)
        {
            const float rope = 8.4f;
            foreach (float a in new[] { -50f, -28f, 28f, 50f })
            {
                float r = a * Mathf.Deg2Rad;
                kit.Thread(new Vector3(anchorX - rope * Mathf.Sin(r), anchorY - rope * Mathf.Cos(r), z), LevelKit.Palette.Rose);
            }
        }

        static void SwingThreadsZ(LevelKit kit, float x, float anchorY, float anchorZ)
        {
            const float rope = 8.4f;
            foreach (float a in new[] { -50f, -28f, 28f, 50f })
            {
                float r = a * Mathf.Deg2Rad;
                kit.Thread(new Vector3(x, anchorY - rope * Mathf.Cos(r), anchorZ + rope * Mathf.Sin(r)), LevelKit.Palette.Rose);
            }
        }

        // The old spinning wheel the lute string got caught in.
        static void SpinningWheel(LevelKit kit, Vector3 feet)
        {
            var wood = PatchworkAssets.Wood(new Color(0.65f, 0.45f, 0.3f));
            kit.Block(feet + new Vector3(0f, 0.6f, 0f), new Vector3(5f, 1.2f, 1.6f), new Color(0.65f, 0.45f, 0.3f), "WheelBench", wood);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, feet + new Vector3(s * 0.5f, 2.6f, 0f), new Vector3(0.25f, 3f, 0.25f), wood);
            var wheel = kit.Group("SpinningWheel", feet + new Vector3(0f, 3.8f, 0f));
            for (int i = 0; i < 20; i++)
            {
                float a = i / 20f * Mathf.PI * 2f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 2.4f, Mathf.Sin(a) * 2.4f, 0f), new Vector3(0.45f, 0.45f, 0.3f), wood, default(Vector3), wheel);
            }
            for (int i = 0; i < 6; i++)
                kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(0.12f, 4.8f, 0.12f), wood, new Vector3(0f, 0f, i * 30f), wheel);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.6f, 0.3f, 0.6f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f), new Vector3(90f, 0f, 0f), wheel);
            var spin = wheel.gameObject.AddComponent<Spinner>();
            spin.Axis = Vector3.forward;
            spin.Speed = 25f;
            // Its spindle, wound with yarn.
            kit.Deco(PrimitiveType.Cylinder, feet + new Vector3(2.2f, 1.9f, 0f), new Vector3(0.5f, 0.7f, 0.5f), PatchworkAssets.Yarn(LevelKit.Palette.Rose));
        }

        // ================================================================ East: the canal

        static void Canal(LevelKit kit)
        {
            var cobble = PatchworkAssets.Cobble(CobbleCol);
            TownKit.Bridge(kit, new Vector3(CanalWest - 0.5f, 0.05f, 0f), new Vector3(CanalEast + 0.5f, 0.05f, 0f), 5f, 1.2f);
            TownKit.Bridge(kit, new Vector3(CanalWest - 0.5f, 0.05f, -40f), new Vector3(CanalEast + 0.5f, 0.05f, -40f), 3.5f, 0.8f);
            TownKit.Bridge(kit, new Vector3(CanalWest - 0.5f, 0.05f, 40f), new Vector3(CanalEast + 0.5f, 0.05f, 40f), 3.5f, 0.8f);
            kit.Block(new Vector3(43.5f, -0.1f, 0f), new Vector3(7f, 0.3f, 120f), CobbleCol, "Promenade", cobble);
            for (float z = -50f; z <= 50f; z += 16f) TownKit.StreetLamp(kit, new Vector3(41f, 0.05f, z));
            TownKit.Bench(kit, new Vector3(46.2f, 0.05f, 18f), -90f);
            TownKit.Bench(kit, new Vector3(46.2f, 0.05f, -22f), -90f);
            kit.AddCheckpoint(new Vector3(43.5f, 0.05f, 0f), 90f);
            kit.Hint(new Vector3(43.5f, 1.5f, 8f), new Vector3(6f, 3f, 4f), "Deep Water",
                "Only Water Form can swim, and it swims fast! Hop in through a gap in the railings, and {Jump} to leap back out.", HintTheme.Water);
            // Threads floating down the canal for swimmers.
            kit.ThreadLine(new Vector3(36f, CanalSurface + 0.45f, -78f), new Vector3(36f, CanalSurface + 0.45f, -52f), 5);
            kit.ThreadLine(new Vector3(36f, CanalSurface + 0.45f, 48f), new Vector3(36f, CanalSurface + 0.45f, 78f), 5);
            // Toy sailboats bobbing on the canal.
            Color[] sails = { new Color(0.97f, 0.95f, 0.9f), LevelKit.Palette.Rose, LevelKit.Palette.Butter };
            for (int i = 0; i < 3; i++)
            {
                var boat = kit.Group("ToyBoat", new Vector3(36f + (i - 1) * 1.6f, CanalSurface + 0.05f, -20f + i * 22f));
                kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(1.2f, 0.5f, 2.6f), PatchworkAssets.Planks(new Color(0.65f, 0.35f, 0.3f)), default(Vector3), boat);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.2f, 0f), new Vector3(0.08f, 1.2f, 0.08f), PatchworkAssets.Wood(TownKit.DarkWood), default(Vector3), boat);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.3f, 0.45f), new Vector3(0.04f, 1.6f, 0.9f), PatchworkAssets.Fabric(sails[i]), default(Vector3), boat);
                var bob = boat.gameObject.AddComponent<Spinner>();
                bob.Speed = 4f;
                bob.BobHeight = 0.12f;
                bob.BobSpeed = 1.1f;
            }

            // The Tea House on the canal, and a toppled teapot that spouts Water up onto its roof.
            var tea = TownKit.MakeHouse(kit, new Vector3(53f, 0f, -30f), new Vector3(9f, 6f, 8f), -90f, Walls[1], Roofs[4], 21, 0, 0f, TownKit.Sign.Teacup);
            kit.Fountain(new Vector3(47.6f, 0f, -30f), 8.5f, LevelKit.FountainStyle.Teapot);
            kit.ThreadLine(new Vector3(53f, tea.RoofTop + 0.8f, -33f), new Vector3(53f, tea.RoofTop + 0.8f, -27f), 3);

            // The Thread Race along the promenade.
            var race = kit.Volume<ThreadRace>(new Vector3(43.5f, 1.25f, -50f), new Vector3(2f, 2.4f, 2f), "ThreadRace");
            var watch = new GameObject("Stopwatch").transform;
            watch.SetParent(race.transform, false);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.3f, 0f), new Vector3(1.3f, 0.1f, 1.3f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f), new Vector3(90f, 0f, 0f), watch);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.3f, -0.07f), new Vector3(1.1f, 0.02f, 1.1f), PatchworkAssets.Felt(LevelKit.Palette.Cream), new Vector3(90f, 0f, 0f), watch);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.5f, -0.1f), new Vector3(0.08f, 0.4f, 0.02f), PatchworkAssets.Felt(UIKit.Ink), default(Vector3), watch);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1f, 0f), Vector3.one * 0.25f, PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f), default(Vector3), watch);
            race.Setup(watch);
            race.TimeLimit = 16f;
            var cyan = PatchworkAssets.Glow(new Color(0.55f, 0.9f, 1f), 0.9f);
            for (int i = 0; i < 10; i++)
            {
                Vector3 p = new Vector3(i % 2 == 0 ? 42f : 45.5f, 1.25f, -42f + i * 8.9f);
                var ringV = kit.Volume<RaceRing>(p, new Vector3(1.8f, 1.8f, 1.8f), "RaceRing");
                var v = new GameObject("Ring").transform;
                v.SetParent(ringV.transform, false);
                for (int j = 0; j < 12; j++)
                {
                    float a = j / 12f * Mathf.PI * 2f;
                    kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.7f, 0f, Mathf.Sin(a) * 0.7f), Vector3.one * 0.16f, cyan, default(Vector3), v);
                }
                ringV.Setup(v);
                race.AddRing(ringV);
            }
            var raceHs = MakeHeartstring(kit, "L1_hs_race", "Race the Canal", new Vector3(45.5f, 1.8f, -53f), true);
            if (GameState.IsCollected(raceHs.Id)) race.MarkDone();
            else race.Won += raceHs.Reveal;
        }

        // ================================================================ East: the Dye Works (Water)

        static void DyeWorks(LevelKit kit)
        {
            var region = MakeRegion(kit, "DyeWorks", Level1Director.TrumpetId);
            Vector3 o = Mill;
            System.Func<float, float, float, Vector3> L = (x, y, z) => o + new Vector3(x, y, z);
            Color bc = new Color(0.68f, 0.42f, 0.34f);
            var brick = PatchworkAssets.Brick(bc);
            const float hw = 7f, hd = 6f, h = 12f, t = 0.6f;
            // Hollow walls, with a drain grate in the south wall as the only way in.
            kit.Block(L(-hw + t * 0.5f, h * 0.5f, 0f), new Vector3(t, h, hd * 2f - t * 2f), bc, "MillWall", brick);
            kit.Block(L(hw - t * 0.5f, h * 0.5f, 0f), new Vector3(t, h, hd * 2f - t * 2f), bc, "MillWall", brick);
            kit.Block(L(0f, h * 0.5f, hd - t * 0.5f), new Vector3(hw * 2f, h, t), bc, "MillWall", brick);
            kit.Block(L(-3.9f, h * 0.5f, -hd + t * 0.5f), new Vector3(6.2f, h, t), bc, "MillWall", brick);
            kit.Block(L(3.9f, h * 0.5f, -hd + t * 0.5f), new Vector3(6.2f, h, t), bc, "MillWall", brick);
            kit.Block(L(0f, (h + 1f) * 0.5f, -hd + t * 0.5f), new Vector3(1.6f, h - 1f, t), bc, "MillWall", brick);
            kit.Grate(L(0f, 0.5f, -hd + t * 0.5f), new Vector3(1.6f, 1f, t));
            TownKit.GableRoof(kit, L(0f, h, 0f), hw * 2f + 0.8f, hd * 2f + 1f, 5f, 0f, PatchworkAssets.Shingle(new Color(0.35f, 0.38f, 0.45f)), brick);
            kit.Block(L(4f, h + 4f, 2f), new Vector3(1.4f, 4f, 1.4f), bc, "MillChimney", brick);
            var footing = PatchworkAssets.Cobble(TownKit.Stone);
            // (The south footing stops either side of the drain, so the opening stays clear.)
            kit.Deco(PrimitiveType.Cube, L(-(hw + 0.16f + 0.9f) * 0.5f, 0.3f, -hd - 0.08f), new Vector3(hw + 0.16f - 0.9f, 0.6f, 0.16f), footing);
            kit.Deco(PrimitiveType.Cube, L((hw + 0.16f + 0.9f) * 0.5f, 0.3f, -hd - 0.08f), new Vector3(hw + 0.16f - 0.9f, 0.6f, 0.16f), footing);
            // A dark mouth behind the bars so the drain reads as a way in.
            kit.Deco(PrimitiveType.Cube, L(0f, 0.5f, -hd + t + 0.05f), new Vector3(1.6f, 1f, 0.04f), PatchworkAssets.Plain(new Color(0.08f, 0.07f, 0.12f), 0.2f));
            kit.Deco(PrimitiveType.Cube, L(0f, 0.3f, hd + 0.08f), new Vector3(hw * 2f + 0.32f, 0.6f, 0.16f), footing);
            kit.Deco(PrimitiveType.Cube, L(-hw - 0.08f, 0.3f, 0f), new Vector3(0.16f, 0.6f, hd * 2f), footing);
            kit.Deco(PrimitiveType.Cube, L(hw + 0.08f, 0.3f, 0f), new Vector3(0.16f, 0.6f, hd * 2f), footing);
            // Windows glowing on the outside, a sign, and a waterwheel turning in its pond.
            var pane = PatchworkAssets.Glow(new Color(1f, 0.86f, 0.6f), 0.55f);
            for (int i = -1; i <= 1; i++)
                for (int f = 0; f < 2; f++)
                {
                    kit.Deco(PrimitiveType.Cube, L(i * 4f, 4f + f * 4f, -hd - 0.04f), new Vector3(1.2f, 1.6f, 0.06f), pane);
                    kit.Deco(PrimitiveType.Cube, L(-hw - 0.04f, 4f + f * 4f, i * 3.5f), new Vector3(0.06f, 1.6f, 1.2f), pane);
                }
            kit.Hint(L(0f, 1.5f, -hd - 2.5f), new Vector3(6f, 3f, 3f), "The Dye Works",
                "The trumpet's valve dew is kept in the reservoir up top. The only way in is the drain. Squeeze through as Water!", HintTheme.Water);
            var wheel = kit.Group("Waterwheel", L(-hw - 1.4f, 4.2f, 0f));
            var wood = PatchworkAssets.Wood(new Color(0.55f, 0.38f, 0.26f));
            for (int i = 0; i < 12; i++)
            {
                float a = i / 12f * Mathf.PI * 2f;
                var paddle = kit.Deco(PrimitiveType.Cube, new Vector3(0f, Mathf.Sin(a) * 3.6f, Mathf.Cos(a) * 3.6f), new Vector3(1.6f, 0.15f, 1f), wood, new Vector3(-a * Mathf.Rad2Deg, 0f, 0f), wheel);
                paddle.name = "Paddle";
            }
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(7.6f, 0.1f, 7.6f), wood, new Vector3(0f, 0f, 90f), wheel);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1f, 1f, 1f), PatchworkAssets.Plain(TownKit.Iron, 0.6f), new Vector3(0f, 0f, 90f), wheel);
            var spin = wheel.gameObject.AddComponent<Spinner>();
            spin.Axis = Vector3.right;
            spin.Speed = 30f;
            kit.Deco(PrimitiveType.Cylinder, L(-hw - 1.4f, 0.05f, 0f), new Vector3(5.5f, 0.05f, 9.5f), PatchworkAssets.Cobble(TownKit.Stone));
            kit.Deco(PrimitiveType.Cylinder, L(-hw - 1.4f, 0.12f, 0f), new Vector3(4.8f, 0.02f, 8.8f), PatchworkAssets.Water(new Color(0.4f, 0.65f, 0.88f)));

            // ----- Inside -----
            var planks = PatchworkAssets.Planks(new Color(0.62f, 0.45f, 0.32f));
            // The drain opens into a clear hall (x -2..2, z -5.4..-2); the floors climb up behind it.
            kit.Deco(PrimitiveType.Cube, L(0f, 0.01f, 0f), new Vector3(hw * 2f - t * 2f, 0.02f, hd * 2f - t * 2f), planks);
            kit.Fountain(L(-4.6f, 0f, -0.8f), 7f, LevelKit.FountainStyle.Pipe);
            kit.Block(L(-1.4f, 2.5f, -0.25f), new Vector3(3.8f, 5f, 3.5f), new Color(0.62f, 0.45f, 0.32f), "MillFloor1", planks);
            kit.Fountain(L(-0.4f, 5f, -0.2f), 6f, LevelKit.FountainStyle.Pipe);
            kit.Block(L(3.8f, 4.5f, 2.2f), new Vector3(5.2f, 9f, 6.4f), new Color(0.62f, 0.45f, 0.32f), "MillFloor2", planks);
            // Ladders painted on the floors' fronts (just for looks) and a railing round the top.
            for (int i = 0; i < 9; i++)
                kit.Deco(PrimitiveType.Cube, L(3.8f, 0.6f + i * 0.95f, -1.04f), new Vector3(0.9f, 0.08f, 0.06f), PatchworkAssets.Wood(TownKit.DarkWood));
            var rail = PatchworkAssets.Plain(new Color(0.78f, 0.48f, 0.3f), 0.75f);
            kit.Rope(L(1.3f, 10f, -0.85f), L(6.3f, 10f, -0.85f), rail, 0.08f);
            foreach (float px in new[] { 1.3f, 3.8f, 6.3f })
            {
                kit.Deco(PrimitiveType.Cylinder, L(px, 9.5f, -0.85f), new Vector3(0.1f, 0.5f, 0.1f), rail);
                kit.Deco(PrimitiveType.Sphere, L(px, 10.05f, -0.85f), Vector3.one * 0.16f, rail);
            }
            // The reservoir: a big copper vat with pipes.
            var copper = PatchworkAssets.Plain(new Color(0.78f, 0.48f, 0.3f), 0.75f);
            kit.Deco(PrimitiveType.Cylinder, L(4.6f, 10.2f, 3.2f), new Vector3(2.6f, 1.2f, 2.6f), copper);
            kit.Deco(PrimitiveType.Cylinder, L(4.6f, 11.42f, 3.2f), new Vector3(2.3f, 0.02f, 2.3f), PatchworkAssets.Water(new Color(0.45f, 0.75f, 0.95f)));
            kit.Rope(L(4.6f, 11.4f, 3.2f), L(4.6f, 11.4f, 5f), copper, 0.3f);
            kit.Rope(L(4.6f, 11.4f, 5f), L(-6f, 11.4f, 5f), copper, 0.3f);
            BandPieceAt(kit, Level1Director.TrumpetId, BandPiece.Part.Trumpet, "Valve Dew", L(2.6f, 10.5f, 0.6f), null);
            MakeHomeHoop(kit, L(4.6f, 9f, 0f));
            // A secret: a third pipe in the back corner up to a high shelf with a golden button.
            kit.Fountain(L(-5.2f, 0f, 2.6f), 11f, LevelKit.FountainStyle.Pipe);
            kit.Block(L(-5.2f, 4.5f, 4.6f), new Vector3(2.4f, 9f, 1.6f), new Color(0.62f, 0.45f, 0.32f), "MillShelf", planks);
            kit.GoldenButton(CollectibleIds[0], L(-5.2f, 10.1f, 4.7f));
            TownKit.Crate(kit, L(-1.6f, 0.02f, 3.4f), 1.2f, 20f);
            TownKit.Crate(kit, L(-1.4f, 1.22f, 3.5f), 0.8f, -15f);
            kit.Lamp(L(-2f, 9.8f, 2.5f), h);
            kit.Lamp(L(1f, 7.5f, -4f), h);
            kit.AddCameraZone(L(0f, 6f, 0f), new Vector3(hw * 2f - 1f, h, hd * 2f - 1f), -3.5f, 12f);
            kit.AddCheckpoint(L(0f, 0f, -4.5f), 0f);
            kit.Thread(L(-4.6f, 3.5f, -0.8f), LevelKit.Palette.Sky);
            kit.Thread(L(-0.4f, 8f, -0.2f), LevelKit.Palette.Sky);
            // Spilled dye and the Fray around the works.
            for (int i = 0; i < 4; i++)
                kit.Deco(PrimitiveType.Cylinder, L(-9f + i * 2.5f, 0.06f, -9f - (i % 2) * 2f), new Vector3(1.8f, 0.02f, 1.4f), PatchworkAssets.Water(FlowerColors[i]));
            FrayTendril(kit, region, L(-9f, 0f, -6f), 3f);
            FrayTendril(kit, region, L(8.5f, 0f, -4f), 2.6f);
            FrayTear(kit, region, L(-3f, 0f, -9f), 2.2f);
            FrayTendril(kit, region, L(3.5f, 9f, 2.5f), 2f);
            Blooms(kit, region, L(0f, 0f, -10f), new Vector2(8f, 2.5f), 10);
            Snarling.Create(kit.Root, L(-6f, 0f, -11f), L(6f, 0f, -11f));
        }

        // ================================================================ South: the rooftops (Wind)

        static void Rooftops(LevelKit kit)
        {
            var region = MakeRegion(kit, "Rooftops", Level1Director.PiperId);
            kit.AddCheckpoint(new Vector3(0f, 0.05f, -36f), 180f);
            // A row of tall terraced houses on each side of the lane.
            var h1 = TownKit.MakeHouse(kit, new Vector3(0f, 0f, -46f), new Vector3(12f, 7f, 8f), 0f, Walls[1], Roofs[1], 31, -1, 4f, TownKit.Sign.None);
            var h2 = TownKit.MakeHouse(kit, new Vector3(0f, 0f, -58f), new Vector3(12f, 6f, 8f), 0f, Walls[3], Roofs[0], 32, -1, -2f, TownKit.Sign.None, false);
            TownKit.MakeHouse(kit, new Vector3(-14f, 0f, -46f), new Vector3(12f, 8f, 8f), 0f, Walls[2], Roofs[3], 33, 1, -2f);
            TownKit.MakeHouse(kit, new Vector3(-14f, 0f, -58f), new Vector3(12f, 7f, 8f), 0f, Walls[0], Roofs[2], 34, -1, 3f, TownKit.Sign.None, false);
            TownKit.MakeHouse(kit, new Vector3(-28f, 0f, -46f), new Vector3(12f, 6f, 8f), 0f, Walls[4], Roofs[4], 35, 1, 0f);
            TownKit.MakeHouse(kit, new Vector3(28f, 0f, -46f), new Vector3(12f, 7f, 8f), 0f, Walls[0], Roofs[3], 36, 1, 2f);
            TownKit.MakeHouse(kit, new Vector3(16f, 0f, -60f), new Vector3(10f, 6f, 8f), 0f, Walls[2], Roofs[1], 37, 0, 0f, TownKit.Sign.None, false);
            // Laundry strung across the back alley.
            TownKit.Clothesline(kit, new Vector3(-20f, 0f, -52f), new Vector3(-8f, 0f, -52f), 5.5f, 0.8f, new float[0]);
            TownKit.Clothesline(kit, new Vector3(8f, 0f, -52f), new Vector3(22f, 0f, -52f), 5.5f, 0.8f, new float[0]);

            // Up: a steam vent in the lane lifts Wind onto the first roof.
            kit.Updraft(new Vector3(0f, 0.05f, -38.5f), 12f, LevelKit.UpdraftStyle.Steam);
            kit.Hint(new Vector3(0f, 1.5f, -38.5f), new Vector3(6f, 4f, 4f), "Over the Rooftops",
                "Steam and chimney smoke lift Wind Form. Float up, then glide from roof to roof. Tap {Ability} mid-glide for a gust.", HintTheme.Wind);
            kit.Thread(new Vector3(0f, 6f, -38.5f), LevelKit.Palette.Cream);
            kit.Thread(new Vector3(0f, 10f, -38.5f), LevelKit.Palette.Cream);
            kit.AddCameraZone(new Vector3(0f, 10f, -57f), new Vector3(20f, 22f, 38f), 3f, 6f);
            // Across the alley to the second roof, then its chimney smoke and a gust to the spire.
            kit.Updraft(h2.ChimneyTop, 8f, LevelKit.UpdraftStyle.Chimney);
            kit.ThreadLine(new Vector3(-2f, 17f, -62f), new Vector3(-0.5f, 16.8f, -67.5f), 3);
            Vector3 spire = new Vector3(0f, 0f, -72f);
            var bc = new Color(0.62f, 0.4f, 0.36f);
            kit.Block(spire + Vector3.up * 8f, new Vector3(6f, 16f, 6f), bc, "Spire", PatchworkAssets.Brick(bc));
            kit.Deco(PrimitiveType.Cube, spire + Vector3.up * 15.8f, new Vector3(6.6f, 0.4f, 6.6f), PatchworkAssets.Cobble(TownKit.Stone));
            Weathervane(kit, spire + new Vector3(-1.6f, 16f, -1.6f));
            // Wisp, the best piper in town, stranded and tangled in the Fray.
            var stranded = ToyNpc.Create(kit, spire + new Vector3(1.4f, 16f, 0.8f), 0f, ToyNpc.Kind.Bird, "Wisp", new Color(0.45f, 0.65f, 0.95f), new Color(0.95f, 0.5f, 0.55f));
            stranded.LookAtPatch = true;
            director.StrandedPiper = stranded;
            BandPieceAt(kit, Level1Director.PiperId, BandPiece.Part.Piper, "Wisp the Piper", spire + new Vector3(1.4f, 17f, 0.8f), stranded);
            FrayTendril(kit, region, spire + new Vector3(-2.6f, 16f, 0.2f), 2.4f);
            FrayTendril(kit, region, spire + new Vector3(2.6f, 16f, 2.6f), 2f);
            MakeHomeHoop(kit, spire + new Vector3(-1.6f, 16f, 1.8f));
            Blooms(kit, region, new Vector3(0f, 0.05f, -38f), new Vector2(4f, 2f), 8);
            FrayTear(kit, region, new Vector3(3f, 0.05f, -36f), 1.8f);
            FrayTendril(kit, region, new Vector3(-4f, 0f, -52f), 2.6f);

            // Secret 1: the humming spool tower (golden button), from the first roof's chimney.
            kit.Updraft(h1.ChimneyTop, 8f, LevelKit.UpdraftStyle.Chimney);
            kit.SpoolTower(new Vector3(14f, 0f, -48f), 16f, SpoolThreads);
            var secret = kit.GoldenButton(CollectibleIds[2], new Vector3(14f, 17.3f, -48f));
            secret.Hums = true;
            // Secret 2: steam on the spire lifts you to a cloud patch with a Heartstring.
            kit.Updraft(spire + new Vector3(1.5f, 16f, -1.5f), 10f, LevelKit.UpdraftStyle.Steam);
            Vector3 cloud = new Vector3(-6f, 22f, -78f);
            // A walkable cloud: a soft flat top ringed and underpinned with puffs.
            var fluff = PatchworkAssets.Felt(new Color(0.99f, 0.97f, 0.94f));
            kit.Prism(Octagon(cloud, 4f), cloud.y - 1f, cloud.y, fluff, "CloudPatch");
            for (int i = 0; i < 10; i++)
            {
                float a = i / 10f * Mathf.PI * 2f;
                float s = 1.9f + (i % 3) * 0.35f;
                kit.Deco(PrimitiveType.Sphere, cloud + new Vector3(Mathf.Cos(a) * 3.7f, -0.55f, Mathf.Sin(a) * 3.7f), new Vector3(s, s * 0.6f, s), fluff);
            }
            for (int i = 0; i < 5; i++)
            {
                float a = i / 5f * Mathf.PI * 2f + 0.4f;
                kit.Deco(PrimitiveType.Sphere, cloud + new Vector3(Mathf.Cos(a) * 1.8f, -1.3f, Mathf.Sin(a) * 1.8f), new Vector3(3.2f, 1.6f, 3.2f), fluff);
            }
            MakeHeartstring(kit, "L1_hs_cloud", "Cloud Patch", cloud + new Vector3(0f, 1.6f, 0f), false);
            MakeHomeHoop(kit, cloud + new Vector3(1.2f, 0f, 1.2f));
        }

        static void Weathervane(LevelKit kit, Vector3 feet)
        {
            var iron = PatchworkAssets.Plain(TownKit.Iron, 0.6f);
            kit.Deco(PrimitiveType.Cylinder, feet + Vector3.up * 2.5f, new Vector3(0.14f, 2.5f, 0.14f), iron);
            var vane = kit.Group("Vane", feet + Vector3.up * 4.6f);
            kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(0.1f, 0.1f, 2.6f), iron, default(Vector3), vane);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, 1.3f), new Vector3(0.05f, 0.5f, 0.5f), iron, new Vector3(45f, 0f, 0f), vane);
            var rooster = new GameObject("Rooster").transform;
            rooster.SetParent(vane, false);
            rooster.localPosition = new Vector3(0f, 0.6f, -0.5f);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.2f, 0.8f, 1f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f), default(Vector3), rooster);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.45f, 0.4f), new Vector3(0.18f, 0.4f, 0.35f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f), default(Vector3), rooster);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.3f, -0.55f), new Vector3(0.06f, 0.7f, 0.5f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f), new Vector3(-30f, 0f, 0f), rooster);
            var spin = vane.gameObject.AddComponent<FlagWave>();
            spin.Amount = 50f;
            spin.Speed = 0.4f;
            kit.Deco(PrimitiveType.Cube, feet + Vector3.up * 3.6f, new Vector3(1.6f, 0.06f, 0.06f), iron);
            kit.Deco(PrimitiveType.Cube, feet + Vector3.up * 3.6f, new Vector3(0.06f, 0.06f, 1.6f), iron);
        }

        // A band piece (the lute string, valve dew, or the stranded piper).
        static void BandPieceAt(LevelKit kit, string id, BandPiece.Part part, string title, Vector3 pos, ToyNpc stranded)
        {
            var piece = kit.Volume<BandPiece>(pos, part == BandPiece.Part.Piper ? new Vector3(2.6f, 3f, 2.6f) : new Vector3(2f, 2.6f, 2f), "BandPiece_" + id);
            Transform vis = null;
            if (part != BandPiece.Part.Piper)
            {
                vis = new GameObject("Visual").transform;
                vis.SetParent(piece.transform, false);
                var glow = PatchworkAssets.Glow(new Color(1f, 0.82f, 0.4f), 0.6f);
                if (part == BandPiece.Part.Lute)
                {
                    // A golden coil of lute string.
                    for (int i = 0; i < 24; i++)
                    {
                        float a = i / 24f * Mathf.PI * 4f;
                        kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.55f, -0.25f + i * 0.022f, Mathf.Sin(a) * 0.55f), Vector3.one * 0.14f, glow, default(Vector3), vis);
                    }
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0f), new Vector3(0.3f, 0.35f, 0.3f), PatchworkAssets.Wood(new Color(0.75f, 0.5f, 0.3f)), default(Vector3), vis);
                }
                else
                {
                    // A glass phial with a glowing drop of valve dew inside.
                    kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.1f, 0f), new Vector3(0.7f, 0.8f, 0.7f), PatchworkAssets.Bubble(new Color(0.8f, 0.95f, 1f, 0.5f)), default(Vector3), vis);
                    kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.15f, 0f), new Vector3(0.38f, 0.45f, 0.38f), PatchworkAssets.Glow(new Color(0.45f, 0.8f, 1f), 1.4f), default(Vector3), vis);
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.42f, 0f), new Vector3(0.22f, 0.15f, 0.22f), PatchworkAssets.Bubble(new Color(0.8f, 0.95f, 1f, 0.5f)), default(Vector3), vis);
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.6f, 0f), new Vector3(0.2f, 0.08f, 0.2f), PatchworkAssets.Felt(new Color(0.65f, 0.48f, 0.32f)), default(Vector3), vis);
                }
                var lightGo = new GameObject("Glow");
                lightGo.transform.SetParent(vis, false);
                var l = lightGo.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = part == BandPiece.Part.Lute ? new Color(1f, 0.8f, 0.5f) : new Color(0.5f, 0.8f, 1f);
                l.range = 7f;
                l.intensity = 1.6f;
                l.shadows = LightShadows.None;
            }
            piece.Setup(id, part, title, vis, stranded);
        }

        // ================================================================ The toys of Toybox Town

        static ToyNpc Toy(LevelKit kit, Vector3 feet, float yaw, ToyNpc.Kind kind, string name, Color main, Color accent, ToyNpc.Instrument inst, float pitch, Color tag, bool talk = true)
        {
            var npc = ToyNpc.Create(kit, feet, yaw, kind, name, main, accent, inst);
            npc.VoicePitch = pitch;
            npc.Tag = tag;
            if (talk)
            {
                var zone = kit.Volume<TalkZone>(feet + Vector3.up * 1.2f, new Vector3(3.8f, 3.2f, 3.8f), "Talk_" + name);
                zone.Npc = npc;
                zone.transform.SetParent(npc.transform, true);
            }
            return npc;
        }

        static void Townsfolk(LevelKit kit)
        {
            Vector3 b = Bandstand + Vector3.up * 0.9f;

            // ----- The band -----
            var pip = Toy(kit, b + new Vector3(-2.6f, 0f, -0.8f), 180f, ToyNpc.Kind.RagDoll, "Pip Plunkett", new Color(0.85f, 0.3f, 0.35f), new Color(0.95f, 0.8f, 0.3f), ToyNpc.Instrument.Lute, 1.15f, new Color(0.75f, 0.3f, 0.35f));
            pip.Lines = () => Level1Director.Has(Level1Director.LuteId)
                ? new[] { "Listen to her sing! A lute's nothing without its string, and now she's got hers back.", "Thank you, little one. Now, has anyone seen Brassby's valve dew? Or Wisp?" }
                : new[] { "Pip Plunkett, lutenist to the Mayor! Or I was, until the Fray snapped my string clean off.", "I saw it blow away west, over the ravine and up Spindle Hill. It'll be caught in the old spinning wheel up there, I'd bet my hat.",
                          "You'll want to swing across. Anything wrapped in pink yarn will hold you!" };
            var brassby = Toy(kit, b + new Vector3(2.6f, 0f, -0.8f), 180f, ToyNpc.Kind.TinSoldier, "Corporal Brassby", new Color(0.8f, 0.22f, 0.25f), LevelKit.Palette.Gold, ToyNpc.Instrument.Trumpet, 0.8f, new Color(0.65f, 0.2f, 0.22f));
            brassby.Lines = () => Level1Director.Has(Level1Director.TrumpetId)
                ? new[] { "Ten-HUT! Ta-ra-ta-raaa! The valves are slick as a sardine. Splendid work!", "I shall play the wedding march with full military gusto!" }
                : new[] { "Corporal Brassby, first trumpet! Ahem. My valves are stuck solid. The Fray dried them right out.", "What they need is a drop of valve dew. The Dye Works by the canal keeps some in its reservoir.",
                          "The mill's all locked up, mind. Only a drain to get in. A wee bubble of Water might manage it." };
            var wisp = Toy(kit, b + new Vector3(0f, 0f, 0.4f), 180f, ToyNpc.Kind.Bird, "Wisp", new Color(0.45f, 0.65f, 0.95f), new Color(0.95f, 0.5f, 0.55f), ToyNpc.Instrument.Pipe, 1.45f, new Color(0.3f, 0.5f, 0.85f));
            wisp.Lines = () => new[] { "Tweedle-dee! You came all the way up the weathervane for me!", "The Fray's gust blew me clean off my perch. I'll pipe the sweetest tune this town's ever heard. Promise!" };
            var bumble = Toy(kit, b + new Vector3(0f, 0f, 2.8f), 180f, ToyNpc.Kind.Bunny, "Bumble", new Color(0.95f, 0.75f, 0.8f), new Color(0.97f, 0.93f, 0.85f), ToyNpc.Instrument.Drum, 1.25f, new Color(0.8f, 0.5f, 0.6f));
            bumble.Lines = () => Level1Director.BandCount() >= 3
                ? new[] { "Ba-dum! We're all here! Go on, tell the Mayor we're ready!" }
                : new[] { "Ba-dum... ba-dum... My drum's the only thing that still works. It's not much of a wedding band on its own.",
                          "We need Pip's lute string, Brassby's valve dew and our piper Wisp. " + (3 - Level1Director.BandCount()) + " to go!" };
            director.Lute = pip;
            director.Trumpet = brassby;
            director.Piper = wisp;
            director.Drummer = bumble;

            // ----- The happy couple -----
            var mayor = Toy(kit, director.MayorSpot, 180f, ToyNpc.Kind.Teddy, "Mayor Bramblebutton", new Color(0.62f, 0.42f, 0.28f), new Color(0.18f, 0.2f, 0.35f), ToyNpc.Instrument.None, 0.72f, new Color(0.25f, 0.28f, 0.45f));
            mayor.Lines = () =>
            {
                if (GameState.IsLevelComplete(SceneName))
                    return new[] { "What a wedding! The finest Toybox Town has ever seen, and the Fray gone from our clock tower.", "You'll always be welcome here, young Patch. Do visit the bandstand. They haven't stopped playing since!" };
                if (Level1Director.BandCount() >= 3)
                    return new[] { "The whole band is back? Then, ahem... places, everyone! Places!", "Lady Lacewing, my dear... shall we?" };
                return new[] { "Mayor Bramblebutton, at your service! Today I marry the loveliest doll in all the toybox.", "But the Fray has ruined the band, and I simply cannot be married without music. It's tradition!",
                               "Pip, Brassby and Bumble will tell you what they need. Oh, and our piper Wisp has gone missing altogether!" };
            };
            mayor.TalkEnded = () => { if (director.CanStartWedding) director.StartWedding(); };
            var bride = Toy(kit, director.BrideSpot, 180f, ToyNpc.Kind.RagDoll, "Lady Lacewing", new Color(0.98f, 0.97f, 0.95f), new Color(0.95f, 0.75f, 0.4f), ToyNpc.Instrument.None, 1.3f, new Color(0.8f, 0.55f, 0.65f));
            bride.Lines = () => GameState.IsLevelComplete(SceneName)
                ? new[] { "I'm Lady Bramblebutton now! Oh, it does have a ring to it." }
                : new[] { "Isn't it a beautiful morning for a wedding? Or it would be, if it weren't so... grey.", "That nasty Fray has wrapped itself round the clock tower. I do hope the music will chase it away." };
            director.Mayor = mayor;
            director.Bride = bride;

            // ----- Townsfolk with tips -----
            var tock = Toy(kit, new Vector3(-6f, 0.06f, 21f), 180f, ToyNpc.Kind.TinSoldier, "Sergeant Tock", new Color(0.25f, 0.35f, 0.65f), LevelKit.Palette.Gold, ToyNpc.Instrument.None, 0.75f, new Color(0.25f, 0.32f, 0.55f));
            tock.Lines = () => new[] { "HALT! Oh. It's you. Carry on.", "Snarlings have taken the barracks, through the arch there. Little grey tangles of Fray.", "Jump on their heads, that's the drill! Stomp, stomp!" };
            var nob = Toy(kit, new Vector3(46f, 0.05f, 4f), -90f, ToyNpc.Kind.Nutcracker, "Nob the Nutcracker", new Color(0.3f, 0.5f, 0.35f), LevelKit.Palette.Gold, ToyNpc.Instrument.None, 0.65f, new Color(0.25f, 0.42f, 0.3f));
            nob.Lines = () => new[] { "Crack! Welcome to the canal. Mind the water, it's deep! Only Water Form can swim in it, and my, how fast it goes.", "The Dye Works is locked tighter than a walnut. But there's a drain in its south wall...", "And that toppled teapot by the Tea House? Still spouting. Water Form could ride it." };
            var jack = Toy(kit, new Vector3(-31f, 0.05f, 4f), 90f, ToyNpc.Kind.JackInBox, "Jack", new Color(0.35f, 0.55f, 0.85f), new Color(0.95f, 0.4f, 0.45f), ToyNpc.Instrument.None, 1.2f, new Color(0.35f, 0.5f, 0.8f));
            jack.Lines = () => new[] { "BOING! Ha! Got you.", "Want to cross the ravine? Swing from the clothesline peg wrapped in pink yarn. Yarn Form only, mind!", "Drop in? No bother. There are steps back up on this side." };
            var thimbleby = Toy(kit, new Vector3(-4f, 0.05f, -34f), 180f, ToyNpc.Kind.RagDoll, "Mrs. Thimbleby", new Color(0.55f, 0.45f, 0.75f), new Color(0.6f, 0.4f, 0.3f), ToyNpc.Instrument.None, 1.05f, new Color(0.5f, 0.4f, 0.7f));
            thimbleby.Lines = () => new[] { "Oh, poor Wisp! A great grey gust took him right up onto the weathervane at the end of the lane.", "My laundry's been up on those roofs twice this week. The steam from the vent carries things right up.", "A little cloud like you would float up there like a feather, dear." };
            var bun = Toy(kit, new Vector3(45.5f, 0.05f, -46.5f), 0f, ToyNpc.Kind.Bunny, "Bun", new Color(0.97f, 0.97f, 0.95f), new Color(0.85f, 0.6f, 0.7f), ToyNpc.Instrument.None, 1.5f, new Color(0.75f, 0.55f, 0.65f));
            bun.Lines = () => new[] { "Ready, steady... RACE! Touch the stopwatch and grab every ring along the promenade!", "My record's twelve seconds. Hop to it!" };
            var ted = Toy(kit, new Vector3(-6f, 0.06f, -11f), 45f, ToyNpc.Kind.Teddy, "Little Ted", new Color(0.85f, 0.65f, 0.4f), new Color(0.4f, 0.6f, 0.85f), ToyNpc.Instrument.None, 1.4f, new Color(0.55f, 0.45f, 0.3f));
            ted.transform.localScale = Vector3.one * 0.7f;
            ted.Lines = () => new[] { "Those grey knots are everywhere! There's one on the bridge, one up on the pigeon loft, one on the Tea House roof...", "Jump on top of them and they go POP! Find all six!" };

            // ----- Wedding guests (just here to watch) -----
            director.Crowd.Add(Toy(kit, new Vector3(-6.5f, 0.06f, -8f), 0f, ToyNpc.Kind.RagDoll, "Guest", new Color(0.6f, 0.8f, 0.6f), new Color(0.45f, 0.3f, 0.2f), ToyNpc.Instrument.None, 1f, Color.grey, false));
            director.Crowd.Add(Toy(kit, new Vector3(6.5f, 0.06f, -8f), 0f, ToyNpc.Kind.Nutcracker, "Guest", new Color(0.7f, 0.25f, 0.5f), LevelKit.Palette.Gold, ToyNpc.Instrument.None, 1f, Color.grey, false));
            director.Crowd.Add(Toy(kit, new Vector3(-4f, 0.06f, -9.5f), 0f, ToyNpc.Kind.Bunny, "Guest", new Color(0.75f, 0.7f, 0.6f), new Color(0.95f, 0.9f, 0.85f), ToyNpc.Instrument.None, 1f, Color.grey, false));
            director.Crowd.Add(Toy(kit, new Vector3(4f, 0.06f, -9.5f), 0f, ToyNpc.Kind.JackInBox, "Guest", new Color(0.95f, 0.7f, 0.3f), new Color(0.4f, 0.7f, 0.5f), ToyNpc.Instrument.None, 1f, Color.grey, false));
            director.Crowd.Add(tock);
            director.Crowd.Add(ted);
        }

        // ================================================================ Knot a Problem (6 knots around town)

        static void Knots(LevelKit kit)
        {
            var hs = MakeHeartstring(kit, "L1_hs_knots", "Knot a Problem", new Vector3(-9f, 1.7f, -14.5f), true);
            var knots = new List<SnarlKnot>
            {
                MakeKnot(kit, new Vector3(8f, 1f, -8f)),
                MakeKnot(kit, new Vector3(-8.5f, 1f, 13f)),
                MakeKnot(kit, new Vector3(-11f, 5.9f, -10.8f)),
                MakeKnot(kit, new Vector3(36f, 2.2f, 0f)),
                MakeKnot(kit, new Vector3(53f, 10.6f, -30f)),
                MakeKnot(kit, new Vector3(0f, 1f, -52f))
            };
            Challenge(knots, hs);
        }

        // ================================================================ Yarn birds

        static void YarnBirds(LevelKit kit)
        {
            Color[] c = { LevelKit.Palette.Rose, LevelKit.Palette.Sky, LevelKit.Palette.Butter, LevelKit.Palette.Lavender, LevelKit.Palette.Peach };
            kit.YarnBird(Bandstand + Vector3.up * 7f, c[0], false);
            kit.YarnBird(new Vector3(0f, 14f, 6f), c[1], true, 12f);
            kit.YarnBird(new Vector3(-8f, 0.06f, -4f), c[2], false);
            kit.YarnBird(new Vector3(9f, 0.06f, 4f), c[3], false);
            kit.YarnBird(Tower + new Vector3(0f, 36.6f, 0f), c[4], false);
            kit.YarnBird(new Vector3(0f, 30f, 27f), c[0], true, 9f);
            kit.YarnBird(new Vector3(-44f, 6f, 10f), c[1], true, 7f);
            kit.YarnBird(new Vector3(-56f, 4.1f, -6f), c[2], false);
            kit.YarnBird(new Vector3(-67f, 7.9f, 35.5f), c[3], false);
            kit.YarnBird(new Vector3(41f, 5.2f, 18f), c[4], false);
            kit.YarnBird(new Vector3(52f, 14f, 0f), c[0], true, 10f);
            kit.YarnBird(new Vector3(0f, 10.6f, -46f), c[1], false);
            kit.YarnBird(new Vector3(-14f, 11.6f, -46f), c[2], false);
            kit.YarnBird(new Vector3(0f, 20f, -62f), c[3], true, 9f);
            kit.YarnBird(new Vector3(-11f, 3f, 54f), c[4], false);
        }

        // ================================================================ Shared pieces

        static Vector2[] Octagon(Vector3 c, float r)
        {
            var pts = new Vector2[8];
            for (int i = 0; i < 8; i++)
            {
                float a = (i + 0.5f) / 8f * Mathf.PI * 2f;
                pts[i] = new Vector2(c.x + Mathf.Cos(a) * r, c.z + Mathf.Sin(a) * r);
            }
            return pts;
        }

        // A Heartstring at pos. Hidden ones appear when their challenge is done.
        static Heartstring MakeHeartstring(LevelKit kit, string id, string title, Vector3 pos, bool hidden)
        {
            kit.Info.HeartstringIds.Add(id);
            var h = kit.Volume<Heartstring>(pos, new Vector3(2.2f, 2.8f, 2.2f), "Heartstring_" + id);
            var vis = new GameObject("Visual").transform;
            vis.SetParent(h.transform, false);
            HeartShape(kit, vis, 0.9f, PatchworkAssets.Glow(new Color(1f, 0.78f, 0.35f), 0.45f));
            // A loop of pink thread circling it.
            var ring = new GameObject("Ring").transform;
            ring.SetParent(vis, false);
            var pink = PatchworkAssets.Glow(new Color(1f, 0.55f, 0.7f), 0.6f);
            for (int i = 0; i < 14; i++)
            {
                float a = i / 14f * Mathf.PI * 2f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 1.05f, 0f, Mathf.Sin(a) * 1.05f), Vector3.one * 0.13f, pink, default(Vector3), ring);
            }
            var lightGo = new GameObject("Glow");
            lightGo.transform.SetParent(h.transform, false);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.8f, 0.5f);
            light.range = 7f;
            light.intensity = 1.6f;
            light.shadows = LightShadows.None;
            h.Setup(id, title, vis, ring, light, hidden);
            return h;
        }

        // A puffy heart: two balls and a diamond, in the x-y plane.
        static void HeartShape(LevelKit kit, Transform parent, float s, Material mat)
        {
            kit.Deco(PrimitiveType.Sphere, new Vector3(-0.3f, 0.2f, 0f) * s, new Vector3(0.75f, 0.75f, 0.5f) * s, mat, default(Vector3), parent);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.3f, 0.2f, 0f) * s, new Vector3(0.75f, 0.75f, 0.5f) * s, mat, default(Vector3), parent);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.12f, 0f) * s, new Vector3(0.68f, 0.68f, 0.46f) * s, mat, new Vector3(0f, 0f, 45f), parent);
        }

        // A Fray knot you pop by landing on it.
        static SnarlKnot MakeKnot(LevelKit kit, Vector3 pos, float size = 1f)
        {
            var k = kit.Volume<SnarlKnot>(pos, new Vector3(2.2f, 2.2f, 2.2f) * size, "SnarlKnot");
            var vis = new GameObject("Visual").transform;
            vis.SetParent(k.transform, false);
            KnotShape(kit, vis, size, 12);
            k.Setup(vis);
            return k;
        }

        static void KnotShape(LevelKit kit, Transform parent, float size, int strands)
        {
            FrayTheme.Knot(kit, parent, size, strands);
        }

        // A tear in the quilt where the Fray has got in, with loose threads around the edge.
        static void FrayTear(LevelKit kit, FrayRegion region, Vector3 pos, float size)
        {
            region.AddFray(FrayTheme.Tear(kit, pos, size));
        }

        // A wavering tendril of Fray thread with a violet glow at the tip, and grey static around it.
        static void FrayTendril(LevelKit kit, FrayRegion region, Vector3 pos, float height)
        {
            region.AddFray(FrayTheme.Tendril(kit, pos, height));
        }

        // Flowers that grow back once a corner is mended.
        static void Blooms(LevelKit kit, FrayRegion region, Vector3 center, Vector2 half, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float x = center.x + (((i * 37) % 100) / 50f - 1f) * half.x;
                float z = center.z + (((i * 61 + 17) % 100) / 50f - 1f) * half.y;
                region.AddBloom(kit.Flower(new Vector3(x, center.y, z), FlowerColors[i % FlowerColors.Length]));
            }
        }

        static FrayRegion MakeRegion(LevelKit kit, string name, string heartstringId)
        {
            var go = new GameObject("Region_" + name);
            go.transform.SetParent(kit.Root, false);
            var r = go.AddComponent<FrayRegion>();
            r.HeartstringId = heartstringId;
            director.Regions[heartstringId] = r;
            return r;
        }

        // A little hoop that takes you back to the Mending Square.
        static void MakeHomeHoop(LevelKit kit, Vector3 feet)
        {
            // An ink hole: a glossy pool of ink with a violet swirl turning in it, droplets bobbing
            // above, a tipped-over inkwell spilling into it and a quill stuck in the edge.
            var hoop = kit.Volume<HomeHoop>(feet + Vector3.up * 0.8f, new Vector3(2f, 1.6f, 2f), "InkHole");
            hoop.Target = SpawnPoint + new Vector3(0f, 0.1f, 2.5f);
            hoop.TargetYaw = 0f;
            var ink = PatchworkAssets.Plain(new Color(0.07f, 0.05f, 0.16f), 0.95f);
            var rim = PatchworkAssets.Plain(new Color(0.14f, 0.1f, 0.3f), 0.9f);
            kit.Deco(PrimitiveType.Cylinder, feet + Vector3.up * 0.02f, new Vector3(2.2f, 0.02f, 2.2f), ink);
            for (int i = 0; i < 14; i++)
            {
                float a = i / 14f * Mathf.PI * 2f;
                float r = 1.1f + Mathf.Sin(i * 2.3f) * 0.08f;
                kit.Deco(PrimitiveType.Sphere, feet + new Vector3(Mathf.Cos(a) * r, 0.04f, Mathf.Sin(a) * r), new Vector3(0.42f, 0.08f, 0.3f), rim, new Vector3(0f, -a * Mathf.Rad2Deg, 0f));
            }
            var ring = new GameObject("Swirl").transform;
            ring.SetParent(hoop.transform, false);
            ring.localPosition = new Vector3(0f, -0.74f, 0f);
            var swirl = PatchworkAssets.Glow(new Color(0.5f, 0.38f, 0.95f), 0.9f);
            var shine = PatchworkAssets.Glow(new Color(0.85f, 0.8f, 1f), 0.8f);
            for (int arm = 0; arm < 3; arm++)
                for (int k = 0; k < 7; k++)
                {
                    float t = k / 7f;
                    float a = arm * 2.094f + t * 4f;
                    float r = Mathf.Lerp(0.95f, 0.12f, t);
                    kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * r, 0.01f, Mathf.Sin(a) * r), new Vector3(0.24f - t * 0.12f, 0.02f, 0.13f), k % 3 == 0 ? shine : swirl, new Vector3(0f, -a * Mathf.Rad2Deg - 70f, 0f), ring);
                }
            for (int i = 0; i < 4; i++)
            {
                float a = i * 1.57f + 0.4f;
                var drop = kit.Deco(PrimitiveType.Sphere, feet + new Vector3(Mathf.Cos(a) * 0.5f, 0.5f + i * 0.15f, Mathf.Sin(a) * 0.5f), new Vector3(0.16f, 0.2f, 0.16f), ink);
                var bob = drop.gameObject.AddComponent<Spinner>();
                bob.Speed = 0f;
                bob.BobHeight = 0.15f;
                bob.BobSpeed = 1.8f + i * 0.35f;
            }
            // The inkwell, on its side, spilling in.
            var well = kit.Group("Inkwell", feet + new Vector3(1.35f, 0.32f, 0.55f));
            well.rotation = Quaternion.Euler(0f, 205f, 82f);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.6f, 0.55f, 0.6f), PatchworkAssets.Bubble(new Color(0.75f, 0.85f, 0.95f, 0.5f)), default(Vector3), well);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.08f, 0f), new Vector3(0.45f, 0.3f, 0.45f), ink, default(Vector3), well);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.36f, 0f), new Vector3(0.22f, 0.1f, 0.22f), PatchworkAssets.Bubble(new Color(0.75f, 0.85f, 0.95f, 0.5f)), default(Vector3), well);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.47f, 0f), new Vector3(0.24f, 0.03f, 0.24f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f), default(Vector3), well);
            kit.Deco(PrimitiveType.Capsule, feet + new Vector3(1.02f, 0.04f, 0.42f), new Vector3(0.22f, 0.32f, 0.04f), ink, new Vector3(90f, 115f, 0f));
            // A quill stuck in the far edge.
            var quill = kit.Group("Quill", feet + new Vector3(-0.95f, 0f, -0.6f));
            quill.rotation = Quaternion.Euler(14f, 30f, -12f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.75f, 0f), new Vector3(0.04f, 0.75f, 0.04f), PatchworkAssets.Plain(new Color(0.95f, 0.92f, 0.85f), 0.6f), default(Vector3), quill);
            kit.Deco(PrimitiveType.Capsule, new Vector3(0.08f, 1.15f, 0f), new Vector3(0.22f, 0.5f, 0.03f), PatchworkAssets.Felt(new Color(0.98f, 0.75f, 0.82f)), new Vector3(0f, 0f, -6f), quill);
            var lightGo = new GameObject("InkGlow");
            lightGo.transform.SetParent(hoop.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, -0.3f, 0f);
            var glowLight = lightGo.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.color = new Color(0.55f, 0.4f, 1f);
            glowLight.range = 4f;
            glowLight.intensity = 1.2f;
            glowLight.shadows = LightShadows.None;
            hoop.Setup(ring);
        }

        // When every one of these is popped, the Heartstring appears.
        static void Challenge(List<SnarlKnot> knots, Heartstring prize)
        {
            if (GameState.IsCollected(prize.Id)) { foreach (var k in knots) k.RemoveQuietly(); return; }
            int left = knots.Count;
            foreach (var k in knots)
                k.Popped += _ =>
                {
                    left--;
                    var game = PatchworkGame.Instance;
                    if (left <= 0) prize.Reveal();
                    else if (game != null) game.ShowToast("Knot popped! " + left + " to go.");
                };
        }

        static void Challenge(List<Snarling> foes, Heartstring prize)
        {
            if (GameState.IsCollected(prize.Id)) { foreach (var f in foes) f.RemoveQuietly(); return; }
            int left = foes.Count;
            foreach (var f in foes)
                f.Popped += _ =>
                {
                    left--;
                    var game = PatchworkGame.Instance;
                    if (left <= 0) prize.Reveal();
                    else if (game != null) game.ShowToast("Snarling unravelled! " + left + " left.");
                };
        }

    }
}

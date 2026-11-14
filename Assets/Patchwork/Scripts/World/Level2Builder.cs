using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Level 2 - The Oceanic Museum.
    //
    // A grand old museum of stone, brass and star-spangled cloth, built round an aquarium the
    // size of a small sea. It's night, and the place is yours. You walk it like a visitor:
    //
    //   The Forecourt (south)      - the Pod lands on the moonlit quay in front of the doors
    //   The Grand Atrium           - a whale's skeleton overhead, the great armillary, a window
    //                                onto the reef as tall as a house, and the grand staircase
    //   The Hall of Deep Time (W)  - fossils, pharaohs, Vikings and the first sea-farers; climb
    //                                the scaffold (or swing from the gallery) to the longship
    //                                hanging in the rafters
    //   The Kelp Galleries (NW)    - a drowned forest growing out of an open pool; cross on the
    //                                floating star-patches, swim it, swing it or glide it
    //   The Deepsea Tunnel (N)     - a glass tube along the sea bed, sharks overhead, and the
    //                                Bell in the middle with its jellyfish
    //   The Sunken Ruins (E)       - a drowned temple: broken columns to hop, channels to swim,
    //                                a wreck to climb, and the lift at the far end
    //   The Hall of Arms (SE)      - knights and samurai, legionaries and hoplites, a castle
    //                                to climb; a side hall off the ruins' south terrace
    //   The Orrery Bridge          - planets going round on brass arms over a starry drop
    //   The Observatory (middle)   - under a glass dome on the sea floor, the great telescope,
    //                                and a balcony looking down over the whole atrium
    //
    // Ground level is y = 0. The main way round is walkable by every form; Water Form and Yarn
    // Form get the most extra routes (pools, fountains and drains; lanterns and knots to swing on).
    // 6 Heartstrings (3 open the Observatory lift), 5 golden buttons and 6 secret marbles
    // (two of them under the water, for Water Form to dive for).
    public static partial class Level2Builder
    {
        public const string SceneName = "Level2";
        public static readonly string[] CollectibleIds = { "L2_b_obelisk", "L2_b_catwalk", "L2_b_crowsnest", "L2_b_library", "L2_b_keep" };
        public static readonly string[] HeartstringIds = { "L2_hs_balcony", "L2_hs_longship", "L2_hs_kelp", "L2_hs_bell", "L2_hs_temple", "L2_hs_orrery" };
        public static readonly string[] MarbleIds = { "L2_marble_desk", "L2_marble_sarcophagus", "L2_marble_seabed", "L2_marble_armoury", "L2_marble_clam", "L2_marble_treasure", "L2_marble_coral", "L2_marble_abyss", "L2_marble_ice", "L2_marble_mangrove" };
        public const int HeartstringsNeeded = 3;

        static readonly Vector3 PodLanding = new Vector3(0f, 0f, -151f);
        static readonly Vector3 SpawnPoint = new Vector3(0f, 0f, -145.6f);

        // The museum's plan (see each part's own file for what's in it).
        const float AtriumHalf = 30f, AtriumZ0 = -108f, AtriumZ1 = -48f, AtriumTop = 30f;
        const float HistX0 = -88f, HistX1 = -32f, HistZ0 = -104f, HistZ1 = -10f, HistTop = 18f;
        const float KelpX0 = -88f, KelpX1 = -32f, KelpZ0 = -4f, KelpZ1 = 76f, KelpTop = 22f;
        const float DeepX0 = -30f, DeepX1 = 42f, DeepZ0 = 20f, DeepZ1 = 100f, DeepFloor = -7f, DeepTop = 26f;
        const float TunnelZ = 66f;
        const float RuinX0 = 44f, RuinX1 = 100f, RuinZ0 = 6f, RuinZ1 = 100f, RuinTop = 24f;
        const float ArmsX0 = 60f, ArmsX1 = 100f, ArmsZ0 = -52f, ArmsZ1 = 4f, ArmsTop = 20f;
        const float DeckY = 14f;
        static readonly Vector3 Observatory = new Vector3(0f, DeckY, -20f);
        const float ObsRadius = 26f;

        static Level2Mood mood;
        static Level2Director director;

        public static LevelInfo Build(LevelKit kit)
        {
            ResetKit();
            var info = kit.Info;
            info.Id = SceneName;
            info.Title = "Level 2 · The Oceanic Museum";
            info.Subtitle = "After hours in the grandest museum under the sea. Find three Heartstrings to open the Observatory lift.";
            info.Spawn = SpawnPoint + Vector3.up * 0.1f;
            info.SpawnYaw = 0f;
            info.KillY = -45f;   // (the Lantern Deep goes down a long way)
            info.FailY = -12f;   // the pools are real places Water Form swims in
            // Night outside; each hall sets its own light as you walk in (see Level2Mood).
            info.Sky = new Color(0.05f, 0.07f, 0.18f);
            info.FogColor = new Color(0.07f, 0.09f, 0.22f);
            info.FogStart = 80f;
            info.FogEnd = 900f;
            info.FarClip = 700f;
            info.SunEuler = new Vector3(48f, 140f, 0f);
            info.SunIntensity = 0.6f;
            info.SunColor = new Color(0.7f, 0.8f, 1f);
            info.ShadowStrength = 0f;   // it's all indoors and lamplit: one sun's shadows would only look wrong
            info.AmbientSky = new Color(0.26f, 0.32f, 0.55f);
            info.AmbientSide = new Color(0.2f, 0.23f, 0.4f);
            info.AmbientGround = new Color(0.12f, 0.12f, 0.22f);
            info.Saturation = 14f;
            info.Contrast = 9f;
            info.Warmth = 2f;
            info.Glow = 0.8f;
            info.Vignette = 0.3f;
            info.Miniature = 0f;
            info.HeartstringsNeeded = HeartstringsNeeded;
            // (Centred on the atrium, so the one snapshot shiny things reflect is taken in there.)
            info.Area.SetMinMax(new Vector3(-110f, -30f, -262f), new Vector3(110f, 70f, 190f));
            info.MapArea = new Bounds(new Vector3(5f, 0f, 2f), new Vector3(210f, 10f, 372f));
            info.MapCameraHeight = 13f;   // under the ceilings, so the map shows the floor plan
            info.MapTitle = "The Oceanic Museum";
            info.MapNote = "Three Heartstrings open the lift to the Observatory. The plaques are worth reading: everything on them is true.";
            info.TitleCenter = new Vector3(0f, 6f, -84f);
            info.TitleRadius = 24f;
            info.TitleHeight = 8f;
            var P = info.MapPins;
            P.Add(new MapPin("The Forecourt", new Vector3(0f, 0f, -134f), MapPin.Kind.Area));
            P.Add(new MapPin("Grand Atrium", new Vector3(0f, 0f, -80f), MapPin.Kind.Area));
            P.Add(new MapPin("Hall of Deep Time", new Vector3(-60f, 0f, -58f), MapPin.Kind.Area));
            P.Add(new MapPin("Kelp Galleries", new Vector3(-60f, 0f, 36f), MapPin.Kind.Area));
            P.Add(new MapPin("Deepsea Tunnel", new Vector3(6f, 0f, TunnelZ + 12f), MapPin.Kind.Area));
            P.Add(new MapPin("Sunken Ruins", new Vector3(72f, 0f, 56f), MapPin.Kind.Area));
            P.Add(new MapPin("The Great Window", new Vector3(26f, 0f, -78f)));
            P.Add(new MapPin("Grand Staircase", new Vector3(0f, 0f, -58f)));
            P.Add(new MapPin("Tyrannosaurus", new Vector3(-60f, 0f, -64f)));
            P.Add(new MapPin("The Longship", new Vector3(-62f, 0f, -30f)));
            P.Add(new MapPin("The Kelp Pool", new Vector3(-60f, 0f, 38f)));
            P.Add(new MapPin("The Bell", new Vector3(6f, 0f, TunnelZ)));
            P.Add(new MapPin("Drowned Temple", new Vector3(72f, 0f, 58f)));
            P.Add(new MapPin("The Wreck", new Vector3(91f, 0f, 40f)));
            P.Add(new MapPin("Observatory Express", new Vector3(48f, 0f, 10f), MapPin.Kind.Shop));
            P.Add(new MapPin("Hall of Arms", new Vector3(80f, 0f, -24f), MapPin.Kind.Area));
            P.Add(new MapPin("Star Corridor", new Vector3(44f, 0f, -22f), MapPin.Kind.Area));
            P.Add(new MapPin("The Sea Gate", new Vector3(-72f, 0f, 27f)));
            P.Add(new MapPin("Drowned Colonnade", new Vector3(97f, 0f, 86f)));
            P.Add(new MapPin("Behind the Glass", new Vector3(-60f, 0f, 140f), MapPin.Kind.Area));
            P.Add(new MapPin("Keepers' Room", new Vector3(-60f, 0f, 86f)));
            P.Add(new MapPin("Coral Nursery", new Vector3(-74.5f, 0f, 112f)));
            P.Add(new MapPin("Lantern Deep", new Vector3(-46.5f, 0f, 112f)));
            P.Add(new MapPin("Frozen Sea", new Vector3(-74.5f, 0f, 154f)));
            P.Add(new MapPin("The Mangrove", new Vector3(-46.5f, 0f, 154f)));
            P.Add(new MapPin("The Undercroft", new Vector3(-60f, 0f, 117f)));

            var moodGo = new GameObject("Level2Mood");
            moodGo.transform.SetParent(kit.Root, false);
            mood = moodGo.AddComponent<Level2Mood>();
            var dirGo = new GameObject("Level2Director");
            dirGo.transform.SetParent(kit.Root, false);
            director = dirGo.AddComponent<Level2Director>();
            director.Mood = mood;
            Moods(info);

            Forecourt(kit);
            Atrium(kit);
            History(kit);
            KelpGalleries(kit);
            GreatDeep(kit);
            Ruins(kit);
            OrreryBridge(kit);
            StarCorridor(kit);
            ObservatoryHall(kit);
            HallOfArms(kit);
            KeepersWing(kit);
            Details(kit);
            Life(kit);
            Rooms(kit);   // (last of all: six more things in every room, fitted in round the rest)
            Quest(kit);   // the story: the three lenses of the Night Glass, and the Fray that has them

            // The hush of the halls, and the deep rumble by the big tanks.
            var sounds = kit.Group("LevelSounds", Vector3.zero);
            foreach (var p in new[] { new Vector3(0f, 6f, -90f), new Vector3(0f, 6f, -62f), new Vector3(-60f, 5f, -84f), new Vector3(-60f, 5f, -50f), new Vector3(-60f, 5f, -22f), new Vector3(0f, 18f, -20f), new Vector3(22f, 16f, 12f) })
                AmbientEmitter.Create(sounds, p, Sfx.Ambient(Sfx.AmbientClip.Hall), 0.5f, 8f, 34f);
            foreach (var p in new[] { new Vector3(34f, 8f, -78f), new Vector3(-84f, 8f, 36f), new Vector3(-36f, 8f, 44f), new Vector3(-16f, 3f, TunnelZ), new Vector3(6f, 3f, TunnelZ), new Vector3(28f, 3f, TunnelZ),
                                      new Vector3(60f, 4f, 76f), new Vector3(84f, 4f, 50f), new Vector3(64f, 4f, 30f), new Vector3(0f, 30f, -20f) })
                AmbientEmitter.Create(sounds, p, Sfx.Ambient(Sfx.AmbientClip.Deep), 0.55f, 7f, 36f);
            AmbientEmitter.Create(sounds, new Vector3(0f, 2f, -150f), Sfx.Ambient(Sfx.AmbientClip.Meadow), 0.12f, 8f, 40f);

            kit.Info.SecretIds.AddRange(MarbleIds);
            ScoreBubbles.Settle(kit);
            // Last of all: settle any surfaces that sit exactly on top of each other (flicker).
            int settled = ZFightFix.Run(kit.Root);
            ZFightFix.OrderSeeThrough(kit.Root);
            if (settled > 0) Debug.Log("[Level2] Settled " + settled + " flickering surfaces.");
            return info;
        }

        // The light and the haze of each part of the museum.
        static void Moods(LevelInfo info)
        {
            mood.Outside = new Level2Mood.Zone
            {
                Name = "The Forecourt",
                AmbientSky = info.AmbientSky, AmbientSide = info.AmbientSide, AmbientGround = info.AmbientGround,
                Fog = info.FogColor, Backdrop = info.Sky, SunColor = info.SunColor, FogStart = info.FogStart, FogEnd = info.FogEnd, Sun = info.SunIntensity
            };
            var Z = mood.Zones;
            KwMoods(Z);   // (Behind the Glass: the tanks first, then the hall round them)
            // (The first box that holds Patch wins, so the small ones come first.)
            Z.Add(new Level2Mood.Zone
            {
                Name = "The Observatory", Line = "And at the very top, or the very bottom, depending how you look at it: the Observatory.",
                Box = Box(-ObsRadius - 4f, ObsRadius + 4f, DeckY - 1f, 70f, Observatory.z - ObsRadius - 1.4f, Observatory.z + ObsRadius + 2f),
                AmbientSky = new Color(0.4f, 0.4f, 0.7f), AmbientSide = new Color(0.33f, 0.28f, 0.5f), AmbientGround = new Color(0.2f, 0.16f, 0.28f),
                Fog = new Color(0.05f, 0.08f, 0.26f), Backdrop = new Color(0.03f, 0.06f, 0.2f), SunColor = new Color(0.75f, 0.8f, 1f), FogStart = 30f, FogEnd = 170f, Sun = 0.55f, Depth = 0.12f
            });
            Z.Add(new Level2Mood.Zone
            {
                Name = "The Star Corridor", Line = "The Star Corridor. The long way round, and the best one.",
                Box = Box(31.5f, 55.5f, DeckY - 1f, DeckY + 8f, -42.5f, 4.5f),
                AmbientSky = new Color(0.4f, 0.36f, 0.62f), AmbientSide = new Color(0.32f, 0.26f, 0.44f), AmbientGround = new Color(0.18f, 0.14f, 0.26f),
                Fog = new Color(0.06f, 0.05f, 0.18f), Backdrop = new Color(0.03f, 0.04f, 0.14f), SunColor = new Color(0.8f, 0.8f, 1f), FogStart = 30f, FogEnd = 160f, Sun = 0.4f
            });
            Z.Add(new Level2Mood.Zone
            {
                Name = "The Orrery Bridge", Line = "The Orrery Bridge. Mind the gap between the planets.",
                Box = Box(-1f, RuinX0 + 0.5f, DeckY - 6f, 40f, 5f, 19f),
                AmbientSky = new Color(0.32f, 0.3f, 0.6f), AmbientSide = new Color(0.24f, 0.2f, 0.42f), AmbientGround = new Color(0.12f, 0.1f, 0.24f),
                Fog = new Color(0.05f, 0.05f, 0.2f), Backdrop = new Color(0.03f, 0.04f, 0.14f), SunColor = new Color(0.7f, 0.75f, 1f), FogStart = 26f, FogEnd = 150f, Sun = 0.4f
            });
            Z.Add(new Level2Mood.Zone
            {
                Name = "The Grand Atrium", Line = "The Grand Atrium. Nobody about but the whale.",
                Box = Box(-AtriumHalf - 1f, AtriumHalf + 0.5f, -3f, 40f, AtriumZ0 - 2.2f, AtriumZ1 + 1.6f),
                AmbientSky = new Color(0.62f, 0.52f, 0.52f), AmbientSide = new Color(0.56f, 0.45f, 0.4f), AmbientGround = new Color(0.32f, 0.25f, 0.24f),
                Fog = new Color(0.2f, 0.15f, 0.22f), Backdrop = new Color(0.04f, 0.05f, 0.12f), SunColor = new Color(1f, 0.88f, 0.72f), FogStart = 55f, FogEnd = 300f, Sun = 0.75f
            });
            Z.Add(new Level2Mood.Zone
            {
                Name = "The Hall of Deep Time", Line = "The Hall of Deep Time, where everything is older than it looks, and it looks very old.",
                Box = Box(HistX0 - 2f, HistX1 + 1f, -3f, 40f, HistZ0 - 2f, HistZ1 + 3f),
                AmbientSky = new Color(0.6f, 0.47f, 0.4f), AmbientSide = new Color(0.52f, 0.4f, 0.32f), AmbientGround = new Color(0.3f, 0.22f, 0.18f),
                Fog = new Color(0.22f, 0.15f, 0.12f), Backdrop = new Color(0.05f, 0.04f, 0.06f), SunColor = new Color(1f, 0.84f, 0.62f), FogStart = 45f, FogEnd = 240f, Sun = 0.65f
            });
            Z.Add(new Level2Mood.Zone
            {
                Name = "The Kelp Galleries", Line = "The Kelp Galleries. A forest that grows up out of the dark, and never once sees the sky.",
                Box = Box(KelpX0 - 2f, KelpX1 + 0.5f, -12f, 40f, HistZ1 + 3f, KelpZ1 + 2f),
                AmbientSky = new Color(0.24f, 0.5f, 0.52f), AmbientSide = new Color(0.18f, 0.36f, 0.42f), AmbientGround = new Color(0.08f, 0.17f, 0.24f),
                Fog = new Color(0.04f, 0.2f, 0.27f), Backdrop = new Color(0.02f, 0.12f, 0.18f), SunColor = new Color(0.6f, 0.95f, 0.9f), FogStart = 16f, FogEnd = 125f, Sun = 0.35f, Depth = 0.3f
            });
            Z.Add(new Level2Mood.Zone
            {
                Name = "The Hall of Arms", Line = "The Hall of Arms. Everybody in here is standing very still, and you can't help feeling they've only just stopped moving.",
                Box = Box(ArmsX0 - 2f, ArmsX1 + 2f, -3f, 40f, ArmsZ0 - 2f, ArmsZ1 + 0.6f),
                AmbientSky = new Color(0.58f, 0.42f, 0.38f), AmbientSide = new Color(0.5f, 0.34f, 0.3f), AmbientGround = new Color(0.28f, 0.18f, 0.17f),
                Fog = new Color(0.22f, 0.1f, 0.1f), Backdrop = new Color(0.06f, 0.03f, 0.04f), SunColor = new Color(1f, 0.8f, 0.6f), FogStart = 40f, FogEnd = 220f, Sun = 0.6f
            });
            Z.Add(new Level2Mood.Zone
            {
                Name = "The Sunken Ruins", Line = "The Sunken Ruins. Somebody built all this to last for ever. The sea had other ideas.",
                Box = Box(RuinX0 - 1.2f, RuinX1 + 2f, -12f, 40f, RuinZ0 - 2f, RuinZ1 + 2f),
                AmbientSky = new Color(0.26f, 0.52f, 0.42f), AmbientSide = new Color(0.18f, 0.36f, 0.32f), AmbientGround = new Color(0.08f, 0.18f, 0.17f),
                Fog = new Color(0.05f, 0.22f, 0.2f), Backdrop = new Color(0.02f, 0.12f, 0.12f), SunColor = new Color(0.7f, 1f, 0.82f), FogStart = 20f, FogEnd = 150f, Sun = 0.45f, Depth = 0.35f
            });
            Z.Add(new Level2Mood.Zone
            {
                Name = "The Deepsea Tunnel", Line = "The Deepsea Tunnel. Down here the only light is the light things make for themselves.",
                Box = Box(DeepX0 - 2.5f, DeepX1 + 1.5f, -12f, 40f, DeepZ0, DeepZ1 + 2f),
                AmbientSky = new Color(0.12f, 0.22f, 0.46f), AmbientSide = new Color(0.08f, 0.14f, 0.32f), AmbientGround = new Color(0.03f, 0.06f, 0.16f),
                Fog = new Color(0.02f, 0.07f, 0.2f), Backdrop = new Color(0.01f, 0.04f, 0.14f), SunColor = new Color(0.5f, 0.7f, 1f), FogStart = 10f, FogEnd = 95f, Sun = 0.18f, Depth = 1f
            });
        }

        static Bounds Box(float x0, float x1, float y0, float y1, float z0, float z1)
        {
            var b = new Bounds();
            b.SetMinMax(new Vector3(x0, y0, z0), new Vector3(x1, y1, z1));
            return b;
        }

        // ================================================================ The Forecourt

        static void Forecourt(LevelKit kit)
        {
            var cobble = PatchworkAssets.Cobble(new Color(0.42f, 0.44f, 0.58f));
            kit.Block(new Vector3(0f, -1f, -136.45f), new Vector3(120f, 2f, 53.1f), StoneCol, "Quay", cobble);
            // The night sea all round the quay, glossy enough to carry the moon.
            var sea = kit.Block(new Vector3(0f, -3.2f, -200f), new Vector3(900f, 1f, 240f), Navy, "NightSea", PatchworkAssets.Plain(new Color(0.05f, 0.08f, 0.2f), 0.93f));
            NoShadow(sea);
            // The quay's edge stones and tall iron railings (with a lamp on every other post).
            var iron = IronMat;
            Vector3[] corners = { new Vector3(-59f, 0f, -110f), new Vector3(-59f, 0f, -162f), new Vector3(59f, 0f, -162f), new Vector3(59f, 0f, -110f) };
            for (int e = 0; e < 3; e++)
            {
                Vector3 a = corners[e], b = corners[e + 1];
                Vector3 d = b - a;
                float len = d.magnitude;
                float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
                kit.Deco(PrimitiveType.Cube, (a + b) * 0.5f + Vector3.up * 0.2f, new Vector3(1.2f, 0.4f, len + 1.2f), PaleStone, new Vector3(0f, yaw, 0f));
                kit.Deco(PrimitiveType.Cube, (a + b) * 0.5f + Vector3.up * 3.1f, new Vector3(0.1f, 0.1f, len), iron, new Vector3(0f, yaw, 0f));
                kit.Deco(PrimitiveType.Cube, (a + b) * 0.5f + Vector3.up * 0.8f, new Vector3(0.1f, 0.1f, len), iron, new Vector3(0f, yaw, 0f));
                var bars = new MeshBatch();
                int n = Mathf.RoundToInt(len / 0.5f);
                for (int i = 0; i <= n; i++)
                {
                    Vector3 p = a + d * (i / (float)n);
                    bars.Cylinder(p + Vector3.up * 0.4f, 0.035f, 3f, 4);
                    bars.Ball(p + Vector3.up * 3.5f, new Vector3(0.1f, 0.3f, 0.1f), 2, 4);
                }
                bars.Build(kit.Root, "Railings", iron);
                int posts = Mathf.RoundToInt(len / 13f);
                for (int i = 0; i <= posts; i++)
                {
                    Vector3 p = a + d * (i / (float)posts);
                    kit.Deco(PrimitiveType.Cube, p + Vector3.up * 2f, new Vector3(0.6f, 4f, 0.6f), PaleStone);
                    NoShadow(kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 4.5f, Vector3.one * 0.9f, PatchworkAssets.Glow(LampGold, 2f)));
                    if (i % 2 == 0) PointLight(kit, p + Vector3.up * 4.5f, LampGold, 16f, 1.2f);
                }
                TownKit.Solid(kit, (a + b) * 0.5f + Vector3.up * 2.4f, new Vector3(0.5f, 4.8f, len + 0.5f), yaw);
            }

            // ----- The front of the museum -----
            var stone = Sandstone;
            WallWithDoor(kit, new Vector3(-61f, 0f, -109f), new Vector3(61f, 0f, -109f), -1f, 36f, 2f, stone, 61f, 9f, 11f);
            // The portico: six great columns, a beam across, and a stepped gable with a brass whale on it.
            foreach (float x in new[] { -18f, -11f, -5.6f, 5.6f, 11f, 18f })
                Column(kit, new Vector3(x, 0f, -115f), 14f, 0.95f);
            kit.Block(new Vector3(0f, 15f, -113f), new Vector3(42f, 2f, 7f), StoneCol, "Entablature", PaleStone);
            for (int i = 0; i < 5; i++)
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 16.8f + i * 1.5f, -113f), new Vector3(40f - i * 8.4f, 1.6f, 6.6f - i * 0.6f), i % 2 == 0 ? stone : PaleStone);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 15f, -116.6f), new Vector3(30f, 1.1f, 0.2f), StarCloth));
            // The whale over the door.
            var emblem = kit.Group("WhaleEmblem", new Vector3(0f, 19.4f, -116.5f));
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(6f, 2.6f, 0.6f), Brass, default(Vector3), emblem);
            kit.Deco(PrimitiveType.Cube, new Vector3(-3.2f, 0.8f, 0f), new Vector3(1.8f, 0.6f, 0.4f), Brass, new Vector3(0f, 0f, -35f), emblem);
            kit.Deco(PrimitiveType.Cube, new Vector3(-3.2f, -0.1f, 0f), new Vector3(1.8f, 0.6f, 0.4f), Brass, new Vector3(0f, 0f, 30f), emblem);
            kit.Deco(PrimitiveType.Sphere, new Vector3(1.7f, 0.35f, -0.25f), Vector3.one * 0.4f, PatchworkAssets.Glow(new Color(0.6f, 0.9f, 1f), 1.5f), default(Vector3), emblem);
            // Lit windows either side, banners between them, and a tower at each end.
            Color warm = new Color(1f, 0.78f, 0.45f);
            foreach (float x in new[] { -48f, -37f, -26f, 26f, 37f, 48f })
            {
                NightWindow(kit, new Vector3(x, 4f, -110f), 180f, 4.4f, 11f, warm, 1.1f);
                NightWindow(kit, new Vector3(x, 20f, -110f), 180f, 3.6f, 7f, warm, 0.8f);
            }
            int em = 0;
            foreach (float x in new[] { -42.5f, -31.5f, 31.5f, 42.5f })
                Banner(kit, new Vector3(x, 17f, -110.3f), 180f, 2.8f, 10f, em % 2 == 0 ? StarCloth : PlumStars, em++);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Block(new Vector3(s * 56f, 22f, -113f), new Vector3(10f, 46f, 10f), StoneCol, "Tower", stone);
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 56f, 45.4f, -113f), new Vector3(11.2f, 0.9f, 11.2f), PaleStone);
                for (int i = 0; i < 6; i++)
                    kit.Deco(PrimitiveType.Cube, new Vector3(s * 56f, 46.6f + i * 1.7f, -113f), new Vector3(9.6f - i * 1.6f, 1.8f, 9.6f - i * 1.6f), PatchworkAssets.Shingle(Navy));
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 56f, 57.6f, -113f), Vector3.one * 1.1f, PatchworkAssets.Glow(new Color(1f, 0.85f, 0.5f), 2f)));
                NightWindow(kit, new Vector3(s * 56f, 28f, -118f), 180f, 3.4f, 8f, warm, 1f);
                NightWindow(kit, new Vector3(s * 56f, 8f, -118f), 180f, 3.4f, 8f, warm, 1f);
            }
            // A cornice along the top of the wall.
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 35.6f, -109.6f), new Vector3(102f, 1.2f, 3.4f), PaleStone);

            // ----- The quay -----
            // A star-cloth runner from the Pod to the doors.
            Rug(kit, new Vector3(0f, 0f, -127f), new Vector2(5f, 30f), StarCloth, 0f, PatchworkAssets.Felt(new Color(1f, 0.82f, 0.4f)));
            for (int i = 0; i < 3; i++)
                for (int s = -1; s <= 1; s += 2)
                    FloorLamp(kit, new Vector3(s * 5.5f, 0f, -139f + i * 8.4f), i % 2 == 0);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Fountain(new Vector3(s * 24f, 0f, -134f), 4.5f, LevelKit.FountainStyle.Stone);
                TownKit.Bench(kit, new Vector3(s * 24f, 0f, -127.5f), 180f);
                TownKit.Bench(kit, new Vector3(s * 17.5f, 0f, -134f), s > 0 ? 270f : 90f);
                // Clipped trees in tubs.
                foreach (float z in new[] { -122f, -146f })
                {
                    Vector3 p = new Vector3(s * 38f, 0f, z);
                    kit.Block(p + Vector3.up * 0.6f, new Vector3(2f, 1.2f, 2f), WoodCol, "Tub", DarkWood);
                    kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 2.2f, new Vector3(0.3f, 1f, 0.3f), PatchworkAssets.Yarn(WoodCol));
                    kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 4f, Vector3.one * 3.2f, PatchworkAssets.Yarn(new Color(0.2f, 0.42f, 0.4f)));
                    kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 6f, Vector3.one * 2.1f, PatchworkAssets.Yarn(new Color(0.24f, 0.48f, 0.44f)));
                }
            }
            NightSky(kit);

            // ----- Arriving -----
            var pod = LandedPod.Build(kit, PodLanding, 0f);
            kit.AddCheckpoint(SpawnPoint, 0f);
            var home = kit.Volume<UseSpot>(pod.DoorStep + new Vector3(0f, 0.4f, 0.4f), new Vector3(2.8f, 3f, 2.4f), "PodHome");
            home.Label = "Head back to the Pod";
            home.PromptPoint = pod.DoorStep + Vector3.up * 2.6f;
            home.Used = p =>
            {
                var g = p.Game;
                g.Ask("Head back to the Pod?", "Fly home to your Pod. Everything you've found is saved.", () =>
                {
                    Sfx.Play(SfxId.PodRocket, 0.7f);
                    g.CameraRig.Kick(0.3f);
                    Haptics.Pulse(0.25f, 0.15f, 0.5f);
                    g.LoadScene(HubBuilder.SceneName);
                });
            };
            var go = new GameObject("PodArrival");
            go.transform.SetParent(kit.Root, false);
            var arrive = go.AddComponent<PodLaunch>();
            arrive.Inside = pod.Inside;
            arrive.Mouth = pod.DoorStep;
            arrive.Land = SpawnPoint;
            arrive.Apex = 0.9f;
            arrive.Duration = 0.85f;
            arrive.Door = pod.Door;
            arrive.Gentle = true;
            arrive.CameraFrom = SpawnPoint + new Vector3(-6f, 3.4f, 7f);
            arrive.CameraAt = PodLanding + new Vector3(0f, 2.2f, 0f);
            kit.Hint(SpawnPoint + new Vector3(0f, 1.5f, 3f), new Vector3(10f, 4f, 8f), "The Oceanic Museum",
                "It's long past closing time, but the doors are open. Three Heartstrings will open the lift to the Observatory. Read the brass plaques as you go: press Ability beside one.");

            Person(kit, new Vector3(6.5f, 0f, -118.5f), 200f, ToyNpc.Kind.Nutcracker, "Ticket-master Tock", new Color(0.2f, 0.25f, 0.55f), new Color(1f, 0.8f, 0.35f), 0.82f, () => new[]
            {
                "Evening! We're closed, strictly speaking. But the fish don't keep hours, and neither do I.",
                "In you go. Atrium first, then the Hall of Deep Time on your left. Follow the bubbles and you can't go wrong.",
                "The Observatory lift wants three Heartstrings before it'll budge. Don't ask me why. It's older than I am."
            });
            Plaque(kit, new Vector3(-6.5f, 0f, -118.5f), 180f, "The Oceanic Museum",
                "Founded so that anyone at all could walk in off the street and stand nose to nose with the sea.",
                "The first public aquarium opened in London in 1853. People queued round the park to look at a tank of fish, because nobody had ever seen under the water before.");
            TrailLine(kit, new Vector3(0f, 1.2f, -136f), new Vector3(0f, 1.2f, -112f), 9);
        }

        // Stars, a big soft moon over the roof, and its glitter on the water.
        static void NightSky(LevelKit kit)
        {
            var rng = new System.Random(88);
            Material[] stars =
            {
                PatchworkAssets.Glow(new Color(1f, 0.97f, 0.9f), 2.4f), PatchworkAssets.Glow(new Color(0.7f, 0.82f, 1f), 2.4f), PatchworkAssets.Glow(new Color(1f, 0.85f, 0.55f), 2.2f)
            };
            var batches = new[] { new MeshBatch(), new MeshBatch(), new MeshBatch() };
            Vector3 mid = new Vector3(0f, 0f, -120f);
            for (int i = 0; i < 420; i++)
            {
                float y = 0.12f + (float)rng.NextDouble() * 0.88f;
                float a = (float)rng.NextDouble() * Mathf.PI * 2f;
                float r = Mathf.Sqrt(1f - y * y);
                Vector3 p = mid + new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r) * 330f;
                float s = 0.9f + (float)rng.NextDouble() * 2.2f;
                batches[i % 3].Ball(p, Vector3.one * s, 2, 5);
            }
            // (Each third of the sky twinkles to its own time.)
            Color[] starTints = { new Color(1f, 0.97f, 0.9f), new Color(0.7f, 0.82f, 1f), new Color(1f, 0.85f, 0.55f) };
            for (int i = 0; i < 3; i++) Twinkling(batches[i], kit.Root, "Stars", starTints[i], 1.3f, 3.2f, 0.8f + i * 0.45f, i * 2.1f);
            // The milky way: a band of fainter stars right across the sky.
            var band = new MeshBatch();
            for (int i = 0; i < 700; i++)
            {
                float a = (float)rng.NextDouble() * Mathf.PI;
                float off = ((float)rng.NextDouble() + (float)rng.NextDouble() - 1f) * 0.22f;
                Vector3 d = Quaternion.Euler(0f, 35f, 0f) * new Vector3(Mathf.Cos(a), Mathf.Sin(a) * 0.9f + 0.1f, off).normalized;
                band.Ball(mid + d * 335f, Vector3.one * (0.6f + (float)rng.NextDouble() * 0.9f), 2, 4);
            }
            Twinkling(band, kit.Root, "MilkyWay", new Color(0.8f, 0.82f, 1f), 1f, 2f, 0.5f, 1f);
            // The moon, with a button's four holes in it.
            Vector3 moon = new Vector3(-150f, 170f, 60f);
            var m = kit.Group("Moon", moon);
            m.rotation = Quaternion.LookRotation((SpawnPoint - moon).normalized, Vector3.up);
            NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(40f, 40f, 6f), PatchworkAssets.Glow(new Color(1f, 0.96f, 0.82f), 1.9f), default(Vector3), m));
            var hole = PatchworkAssets.Glow(new Color(0.7f, 0.7f, 0.68f), 0.9f);
            for (int i = 0; i < 4; i++)
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3((i % 2 == 0 ? -1f : 1f) * 5f, (i < 2 ? -1f : 1f) * 5f, 2.6f), new Vector3(5f, 5f, 1.4f), hole, default(Vector3), m));
            // Moonlight on the sea: a ragged path of glints leading out from the quay.
            var glint = new MeshBatch();
            for (int i = 0; i < 70; i++)
            {
                float z = -166f - i * 2.6f - (float)rng.NextDouble() * 2f;
                float w = 2f + (float)rng.NextDouble() * (3f + i * 0.14f);
                glint.Box(new Vector3(-26f + ((float)rng.NextDouble() - 0.5f) * (6f + i * 0.3f) - i * 0.5f, -2.66f, z), new Vector3(w, 0.04f, 0.4f), Quaternion.identity);
            }
            glint.Build(kit.Root, "MoonGlints", PatchworkAssets.Glow(new Color(0.85f, 0.9f, 1f), 1.2f));
        }

        // ================================================================ The Grand Atrium

        static void Atrium(LevelKit kit)
        {
            var stone = Sandstone;
            float H = AtriumTop;
            Floor(kit, -AtriumHalf, AtriumHalf, -109.9f, AtriumZ1, 0f, FloorQuilt, 5f, "AtriumFloor");

            // ----- Walls -----
            // West: the doorway to the Hall of Deep Time, with the upper gallery's doorway over it.
            Wall(kit, new Vector3(-31f, 0f, AtriumZ0), new Vector3(-31f, 0f, -82f), -1f, H, 2f, stone);
            Wall(kit, new Vector3(-31f, 0f, -74f), new Vector3(-31f, 0f, AtriumZ1), -1f, H, 2f, stone);
            Wall(kit, new Vector3(-31f, 0f, -82f), new Vector3(-31f, 0f, -74f), 6.5f, 8f, 2f, stone);
            Wall(kit, new Vector3(-31f, 0f, -82f), new Vector3(-31f, 0f, -74f), 13.5f, H, 2f, stone);
            ArchTrim(kit, new Vector3(-31f, 0f, -78f), 90f, 8f, 6.5f, 2f, stone, PaleStone);
            ArchTrim(kit, new Vector3(-31f, 8f, -78f), 90f, 8f, 5.5f, 2f, stone, PaleStone);
            // North: solid below, and high up the great arch the Observatory looks down through.
            Wall(kit, new Vector3(-AtriumHalf, 0f, -47f), new Vector3(-10f, 0f, -47f), -1f, H, 2f, stone);
            Wall(kit, new Vector3(10f, 0f, -47f), new Vector3(AtriumHalf, 0f, -47f), -1f, H, 2f, stone);
            Wall(kit, new Vector3(-10f, 0f, -47f), new Vector3(10f, 0f, -47f), -1f, DeckY, 2f, stone);
            Wall(kit, new Vector3(-10f, 0f, -47f), new Vector3(10f, 0f, -47f), DeckY + 11f, H, 2f, stone);
            ArchTrim(kit, new Vector3(0f, DeckY, -47f), 0f, 20f, 11f, 2f, stone, PaleStone);
            // East: stone piers either side of the Great Window, and stone above it.
            Wall(kit, new Vector3(31f, 0f, AtriumZ0), new Vector3(31f, 0f, -102f), -1f, H, 2f, stone);
            Wall(kit, new Vector3(31f, 0f, -54f), new Vector3(31f, 0f, AtriumZ1), -1f, H, 2f, stone);
            Wall(kit, new Vector3(31f, 0f, -102f), new Vector3(31f, 0f, -54f), 22f, H, 2f, stone);

            // ----- The ceiling: a painted night sky, iron ribs and a strip of skylights -----
            kit.Block(new Vector3(0f, H + 0.5f, -78f), new Vector3(64f, 1f, 64f), Navy, "AtriumCeiling", PatchworkAssets.Felt(new Color(0.07f, 0.09f, 0.24f)));
            var rib = IronMat;
            foreach (float z in new[] { -104f, -91f, -78f, -65f, -52f })
            {
                Arc(kit, new Vector3(-AtriumHalf, 21f, z), new Vector3(AtriumHalf, 21f, z), 8.4f, 0.5f, 0.7f, rib, 18);
                Arc(kit, new Vector3(-AtriumHalf, 20f, z), new Vector3(AtriumHalf, 20f, z), 8.4f, 0.16f, 0.9f, Brass, 18);
                for (int s = -1; s <= 1; s += 2)
                {
                    // A stone pier under each rib, except where there's a doorway (west, middle)
                    // or the Great Window (east) to stand in front of.
                    // (None on the east side at all: the Keeper statues stand in those corners.)
                    bool clear = s < 0 ? Mathf.Abs(z + 78f) < 1f : true;
                    if (!clear)
                    {
                        kit.Deco(PrimitiveType.Cube, new Vector3(s * (AtriumHalf - 0.6f), 11f, z), new Vector3(1.2f, 22f, 1.6f), PaleStone);
                        TownKit.Solid(kit, new Vector3(s * (AtriumHalf - 0.6f), 11f, z), new Vector3(1.2f, 22f, 1.6f), 0f);
                    }
                    kit.Deco(PrimitiveType.Cube, new Vector3(s * (AtriumHalf - 0.8f), 21.6f, z), new Vector3(1.8f, 0.8f, 2.2f), Ashlar);
                }
            }
            var moonGlow = PatchworkAssets.Glow(new Color(0.72f, 0.82f, 1f), 1.3f);
            foreach (float z in new[] { -97.5f, -84.5f, -71.5f, -58.5f })
            {
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, H - 0.04f, z), new Vector3(9f, 0.1f, 9f), moonGlow));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, H - 0.1f, z), new Vector3(0.2f, 0.2f, 9f), rib));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, H - 0.1f, z), new Vector3(9f, 0.2f, 0.2f), rib));
            }
            StarCeiling(kit, H);

            // ----- The Great Window -----
            var win = Tank(kit, new Vector3(AtriumHalf, 0f, -78f), 270f, 48f, 22f, 30f, 8, 4, new Color(0.02f, 0.1f, 0.3f), new Color(0.2f, 0.72f, 0.85f), 0f, true, true, 11);
            Reef(win, 30);
            Color[] kelpCols = { new Color(0.55f, 0.6f, 0.2f), new Color(0.7f, 0.62f, 0.2f), new Color(0.4f, 0.55f, 0.25f) };
            for (int i = 0; i < 12; i++)
                Kelp(win, win.R(-22f, 22f), win.R(4f, 26f), win.R(12f, 20f), kelpCols[i % 3]);
            TankShoal(win, -6f, 13f, 14f, 13f, 6f, 13f, 26, 0.5f, new Color(0.85f, 0.9f, 1f));
            TankShoal(win, 10f, 7f, 9f, 9f, 4f, -17f, 18, 0.42f, new Color(1f, 0.8f, 0.35f), 90f);
            TankSwim(win, Swimmer.Shark, 0f, 15f, 16f, 19f, 9f, 9f, 2f, new Color(0.42f, 0.5f, 0.64f), CreamCol);
            TankSwim(win, Swimmer.Shark, 3f, 9f, 18f, 16f, 7f, -11f, 1.5f, new Color(0.5f, 0.48f, 0.6f), CreamCol, 140f);
            TankSwim(win, Swimmer.Ray, -4f, 18f, 13f, 17f, 6f, 8f, 2.2f, new Color(0.2f, 0.24f, 0.44f), CreamCol, 200f);
            TankSwim(win, Swimmer.Turtle, 8f, 5f, 8f, 10f, 3.5f, -7f, 1.5f, new Color(0.35f, 0.58f, 0.4f), new Color(0.8f, 0.75f, 0.5f), 60f);
            Color[] reefFish = { new Color(1f, 0.55f, 0.2f), new Color(1f, 0.85f, 0.25f), new Color(0.35f, 0.6f, 1f), new Color(1f, 0.45f, 0.6f), new Color(0.6f, 0.4f, 0.9f) };
            for (int i = 0; i < 12; i++)
                TankSwim(win, i % 3 == 0 ? Swimmer.Tall : Swimmer.Fish, win.R(-20f, 20f), win.R(2f, 12f), win.R(5f, 13f), win.R(2f, 6f), win.R(1f, 3f), (i % 2 == 0 ? 1f : -1f) * win.R(14f, 34f),
                    win.R(0.6f, 1.1f), reefFish[i % reefFish.Length], i % 2 == 0 ? CreamCol : new Color(0.15f, 0.15f, 0.3f), i * 53f);
            for (int i = 0; i < 4; i++) Bubbles(kit, win.At(win.R(-20f, 20f), 0.5f, win.R(3f, 20f)), 20f);
            // Benches to sit and look, and a rope to keep noses off the glass.
            RopeLine(kit, new Vector3(27.3f, 0f, -98.5f), new Vector3(27.3f, 0f, -59.5f), Plum);
            foreach (float z in new[] { -92f, -78f, -64f }) TownKit.Bench(kit, new Vector3(22f, 0f, z), 90f);
            Plaque(kit, new Vector3(25.6f, 0f, -85f), 270f, "The Great Window",
                "One sheet of glass, as tall as a house, holding back a reef. Giant kelp can grow half a metre in a single day, which makes it one of the fastest-growing living things there is.",
                "The turtle is called Dorothea. She has been here longer than the roof.");

            // ----- The grand staircase and the upper gallery -----
            var marble = PaleStone;
            Steps(kit, new Vector3(0f, 0f, -66f), 0f, 16, 0.25f, 0.75f, 10f, marble, -0.5f);
            kit.Block(new Vector3(0f, 1.75f, -51f), new Vector3(12f, 4.5f, 6f), StoneCol, "Landing", marble);
            Steps(kit, new Vector3(-6f, 4f, -51f), 270f, 16, 0.25f, 0.75f, 5f, marble, -0.5f);
            Steps(kit, new Vector3(6f, 4f, -51f), 90f, 16, 0.25f, 0.75f, 5f, marble, -0.5f);
            // A plum runner up the middle of the stairs.
            for (int i = 0; i < 16; i++)
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.25f * (i + 1) + 0.02f, -66f + 0.75f * (i + 0.5f)), new Vector3(4f, 0.04f, 0.76f), PlumStars));
            Rug(kit, new Vector3(0f, 4f, -51f), new Vector2(4f, 5f), PlumStars);
            for (int s = -1; s <= 1; s += 2)
            {
                FloorLamp(kit, new Vector3(s * 5.8f, 0f, -66.8f));
            }
            // Galleries at y = 8: along the north wall either side of the stairs, and all down the west wall.
            Floor(kit, -AtriumHalf, -18f, -54f, AtriumZ1, 8f, WarmQuilt, 3f, "NorthGalleryW");
            Floor(kit, 18f, AtriumHalf, -54f, AtriumZ1, 8f, WarmQuilt, 3f, "NorthGalleryE");
            Floor(kit, -AtriumHalf, -24f, AtriumZ0, -54f, 8f, WarmQuilt, 3f, "WestGallery");
            Balustrade(kit, new Vector3(-24f, 8f, AtriumZ0 + 0.3f), new Vector3(-24f, 8f, -54f), 1.1f, marble);
            Balustrade(kit, new Vector3(-24f, 8f, -54f), new Vector3(-18.4f, 8f, -54f), 1.1f, marble);
            Balustrade(kit, new Vector3(18.4f, 8f, -54f), new Vector3(AtriumHalf - 0.3f, 8f, -54f), 1.1f, marble);
            for (float z = AtriumZ0 + 4f; z < -55f; z += 8.33f)
                if (Mathf.Abs(z + 78f) > 4.5f) Column(kit, new Vector3(-24.6f, 0f, z), 7f, 0.55f);
            foreach (float x in new[] { -24.6f, -18.6f, 18.6f, 24.6f })
                Column(kit, new Vector3(x, 0f, -54.6f), 7f, 0.55f);
            // Gallery furniture.
            // (Between the piers, not on them.)
            foreach (float z in new[] { -97.5f, -84.5f, -71.5f, -58.5f })
                Sconce(kit, new Vector3(-30f, 10.6f, z), 90f, z == -97.5f || z == -71.5f);
            Trail(kit, 10, new Vector3(-27f, 9.2f, -62f), new Vector3(-27f, 9.2f, -66f), new Vector3(-27f, 9.2f, -70f), new Vector3(-27f, 9.2f, -74f), new Vector3(-28f, 9.2f, -78f));
            kit.ThreadLine(new Vector3(-27f, 9.3f, -86f), new Vector3(-27f, 9.3f, -104f), 6);
            // The east balcony, right up against the Great Window: the first Heartstring.
            Level1Builder.MakeHeartstring(kit, "L2_hs_balcony", "The Whale's Balcony", new Vector3(25f, 9.8f, -51f), false);
            Trail(kit, 10, new Vector3(9f, 5.6f, -51f), new Vector3(12f, 6.6f, -51f), new Vector3(15f, 7.6f, -51f), new Vector3(18.5f, 9.2f, -51f), new Vector3(22f, 9.2f, -51f));
            Plaque(kit, new Vector3(20.5f, 8f, -49.6f), 180f, "Heartstrings",
                "A Heartstring is the thread that holds a place together. Three of them will wake the Observatory lift, at the far end of the Sunken Ruins.");

            // ----- The whale -----
            WhaleSkeleton(kit, new Vector3(0f, 20.5f, -60f), new Vector3(0f, 17f, -92f), H);
            Plaque(kit, new Vector3(-4.6f, 0f, -72f), 0f, "The Blue Whale",
                "The largest animal that has ever lived: bigger than any dinosaur. Its heart is the size of a small car, and its song can cross an entire ocean.",
                "Whales are mammals. Their great-great-grandparents walked about on land on four legs, and then, about fifty million years ago, changed their minds.");

            // ----- The great armillary -----
            Armillary(kit, new Vector3(0f, 0f, -86f));
            Plaque(kit, new Vector3(4.6f, 0f, -91.5f), 180f, "The Armillary Sphere",
                "A model of the sky made of brass hoops. Each ring is a circle you could draw across the heavens: the horizon, the equator, the path of the sun.",
                "Astronomers in Greece and in China were building these more than two thousand years ago, long before anyone had a telescope.");

            // ----- Round the hall -----
            Rug(kit, new Vector3(0f, 0f, -100.5f), new Vector2(6f, 15f), StarCloth, 0f, PatchworkAssets.Felt(new Color(1f, 0.82f, 0.4f)));
            Rug(kit, new Vector3(0f, 0f, -72.5f), new Vector2(6f, 12f), StarCloth, 0f, PatchworkAssets.Felt(new Color(1f, 0.82f, 0.4f)));
            foreach (var p in new[] { new Vector3(-15f, 15f, -96f), new Vector3(15f, 15f, -96f), new Vector3(-15f, 15f, -76f), new Vector3(15f, 15f, -76f), new Vector3(-12f, 16f, -58f), new Vector3(12f, 16f, -58f) })
                HangingLantern(kit, p, H, 26f, 1.7f);
            int e = 0;
            foreach (float z in new[] { -97.5f, -84.5f, -71.5f, -58.5f })
            {
                Banner(kit, new Vector3(-29.6f, 21f, z), 90f, 3f, 9f, e % 2 == 0 ? StarCloth : PlumStars, e);
                e++;
            }
            Banner(kit, new Vector3(-16f, 27f, -47.9f), 180f, 3.4f, 12f, PlumStars, 2);
            Banner(kit, new Vector3(16f, 27f, -47.9f), 180f, 3.4f, 12f, StarCloth, 1);
            // The front desk, and Professor Plume behind it.
            var desk = kit.Group("FrontDesk", new Vector3(-15f, 0f, -101f));
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.6f, 0f), new Vector3(8f, 1.2f, 1.6f), DarkWood, default(Vector3), desk);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.24f, 0f), new Vector3(8.3f, 0.1f, 1.9f), Brass, default(Vector3), desk);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.6f, 0.82f), new Vector3(7.4f, 0.8f, 0.04f), StarCloth, default(Vector3), desk);
            kit.Deco(PrimitiveType.Sphere, new Vector3(-3f, 1.6f, 0f), Vector3.one * 0.6f, PatchworkAssets.Glow(LampGold, 2f), default(Vector3), desk);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(2.6f, 1.36f, 0.1f), new Vector3(0.3f, 0.07f, 0.3f), Brass, default(Vector3), desk);
            kit.Deco(PrimitiveType.Sphere, new Vector3(2.6f, 1.48f, 0.1f), Vector3.one * 0.2f, Brass, default(Vector3), desk);
            var dcol = desk.gameObject.AddComponent<BoxCollider>();
            dcol.center = new Vector3(0f, 0.65f, 0f);
            dcol.size = new Vector3(8f, 1.3f, 1.6f);
            PointLight(kit, new Vector3(-18f, 2.2f, -100f), LampGold, 8f, 1f);
            Person(kit, new Vector3(-9.2f, 0f, -98.6f), 30f, ToyNpc.Kind.Bird, "Professor Plume", new Color(0.36f, 0.42f, 0.7f), new Color(1f, 0.8f, 0.4f), 1.25f, () => new[]
            {
                "Oh! A visitor. At this hour! How perfectly splendid.",
                "The way round is simple. West through the Hall of Deep Time, north to the Kelp Galleries, along the tunnel on the sea bed, and back through the Sunken Ruins to the lift.",
                "Do read the plaques. I wrote every one of them myself, and every word is true. Mostly the whale ones."
            });
            SecretMarble.Place(kit, new Vector3(-10.4f, 1.1f, -104.6f), "L2_marble_desk", new Color(1f, 0.8f, 0.3f));
            // Cases round the hall.
            Vector3 c1 = DisplayCase(kit, new Vector3(-14f, 0f, -70f), new Vector3(2.4f, 2.6f, 2.4f));
            Ammonite(kit, c1 + Vector3.up * 0.75f, 0.6f, new Color(0.85f, 0.7f, 0.5f));
            Plaque(kit, new Vector3(-14f, 0f, -67.8f), 0f, "Ammonite",
                "Not a snail, though it looks like one: a cousin of the squid that lived in a coiled shell. As it grew it built itself a bigger room and moved in, sealing the old one behind it.",
                "Ammonites swam the seas for three hundred million years, and vanished at the same time as the dinosaurs.");
            Vector3 c2 = DisplayCase(kit, new Vector3(14f, 0f, -100f), new Vector3(2.4f, 2.6f, 2.4f));
            ShipInBottle(kit, c2 + Vector3.up * 0.5f);
            Plaque(kit, new Vector3(14f, 0f, -97.8f), 0f, "HMS Challenger",
                "In 1872 this little ship set off to find out what was at the bottom of the sea. She sailed for nearly four years and went right round the world.",
                "Her crew discovered more than four thousand kinds of animal nobody had seen before, and found the deepest place in any ocean, which still carries her name: the Challenger Deep.");
            // The way west, and where you are if you fall.
            kit.AddCheckpoint(new Vector3(0f, 0f, -104f), 0f);
            TrailLine(kit, new Vector3(-6f, 1.2f, -80f), new Vector3(-26f, 1.2f, -78f), 7);
            Person(kit, new Vector3(-22f, 0f, -90f), 60f, ToyNpc.Kind.Bunny, "Thimble", new Color(0.95f, 0.85f, 0.75f), new Color(0.9f, 0.5f, 0.6f), 1.5f, () => new[]
            {
                "I hid when the bell rang for closing and now I've got the WHOLE museum.",
                "You can climb on nearly everything in the Hall of Deep Time. Don't tell the Sergeant I said."
            });
            kit.Hint(new Vector3(0f, 1.5f, -96f), new Vector3(14f, 4f, 8f), "The Grand Atrium",
                "The Hall of Deep Time is through the arch on the left. The grand staircase leads to the galleries: there's a Heartstring up on the balcony by the Great Window.");
        }

        // Little glowing stars and a few stitched constellations on the underside of a ceiling.
        static void PaintedStars(LevelKit kit, Vector3 center, float w, float d, int count, int seed)
        {
            var rng = new System.Random(seed);
            var gold = new MeshBatch();
            var pale = new MeshBatch();
            for (int i = 0; i < count; i++)
            {
                Vector3 p = center + new Vector3(((float)rng.NextDouble() - 0.5f) * w, 0f, ((float)rng.NextDouble() - 0.5f) * d);
                float s = 0.12f + (float)rng.NextDouble() * 0.26f;
                (i % 3 == 0 ? gold : pale).Ball(p, new Vector3(s, 0.08f, s), 2, 5);
            }
            // Constellations: a few stars joined by stitches.
            for (int c = 0; c < 6; c++)
            {
                Vector3 p = center + new Vector3(((float)rng.NextDouble() - 0.5f) * w * 0.8f, -0.02f, ((float)rng.NextDouble() - 0.5f) * d * 0.8f);
                float heading = (float)rng.NextDouble() * 360f;
                for (int k = 0; k < 5; k++)
                {
                    heading += ((float)rng.NextDouble() - 0.5f) * 140f;
                    Vector3 q = p + Quaternion.Euler(0f, heading, 0f) * Vector3.forward * (2.2f + (float)rng.NextDouble() * 2.4f);
                    if (Mathf.Abs(q.x - center.x) > w * 0.48f || Mathf.Abs(q.z - center.z) > d * 0.48f) break;
                    gold.Ball(p, new Vector3(0.6f, 0.1f, 0.6f), 2, 6);
                    gold.Stitches(p, q, 0.3f, 0.2f, 0.07f, 0.03f);
                    p = q;
                }
                gold.Ball(p, new Vector3(0.6f, 0.1f, 0.6f), 2, 6);
            }
            gold.Build(kit.Root, "CeilingStarsGold", PatchworkAssets.Glow(new Color(1f, 0.85f, 0.45f), 1.8f));
            pale.Build(kit.Root, "CeilingStarsPale", PatchworkAssets.Glow(new Color(0.85f, 0.9f, 1f), 1.6f));
        }

        // The blue whale's skeleton, hung from the roof on iron rods, diving towards the doors.
        static void WhaleSkeleton(LevelKit kit, Vector3 tail, Vector3 head, float ceiling)
        {
            var bone = Bone;
            Spine(kit, tail, head, 1.4f, 1f, 4.6f, 0.42f, 0.8f);
            Vector3 fwd = (head - tail).normalized;
            Vector3 side = Vector3.Cross(Vector3.up, fwd).normalized;
            float yaw = Mathf.Atan2(fwd.x, fwd.z) * Mathf.Rad2Deg;
            float pitch = -Mathf.Asin(fwd.y) * Mathf.Rad2Deg;
            // The skull: a long flat beak in three pieces, and the two great bows of the jaw.
            var skull = kit.Group("WhaleSkull", head);
            skull.rotation = Quaternion.Euler(pitch + 6f, yaw, 0f);
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.3f, 0.6f), new Vector3(3.4f, 1.8f, 3f), bone, default(Vector3), skull));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.1f, 3.2f), new Vector3(2.4f, 0.5f, 3.6f), bone, default(Vector3), skull));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, 6.2f), new Vector3(1.5f, 0.4f, 3f), bone, default(Vector3), skull));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.08f, 8.6f), new Vector3(0.8f, 0.3f, 2.2f), bone, default(Vector3), skull));
            for (int s = -1; s <= 1; s += 2)
            {
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 1.5f, 0.1f, 0.9f), new Vector3(0.9f, 0.9f, 1.1f), PatchworkAssets.Felt(new Color(0.75f, 0.7f, 0.6f)), default(Vector3), skull));
                Vector3 prev = new Vector3(s * 1.7f, -0.9f, 0.4f);
                for (int i = 1; i <= 7; i++)
                {
                    float k = i / 7f;
                    Vector3 p = new Vector3(s * (1.7f + Mathf.Sin(k * Mathf.PI) * 1.5f - k * 1.6f), -0.9f - Mathf.Sin(k * Mathf.PI) * 0.5f, 0.4f + k * 9.4f);
                    var seg = kit.Deco(PrimitiveType.Cube, (p + prev) * 0.5f, new Vector3(0.32f, 0.42f, (p - prev).magnitude + 0.1f), bone, default(Vector3), skull);
                    seg.localRotation = Quaternion.LookRotation((p - prev).normalized, Vector3.up);
                    NoShadow(seg);
                    prev = p;
                }
            }
            // Flippers: an arm and a fan of finger bones each side.
            Vector3 shoulder = Vector3.Lerp(tail, head, 0.78f) + Vector3.up * 0.4f;
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 a = shoulder + side * s * 1.6f + Vector3.down * 1.6f;
                Vector3 elbow = a + side * s * 2.4f + Vector3.down * 1.4f - fwd * 0.8f;
                Beam(kit, a, elbow, 0.42f, bone);
                for (int f = 0; f < 4; f++)
                    Beam(kit, elbow, elbow + side * s * (2.2f - f * 0.3f) + Vector3.down * (1.2f + f * 0.5f) - fwd * (0.4f + f * 0.9f), 0.2f, bone);
            }
            // The rods it hangs from.
            for (int i = 0; i < 5; i++)
            {
                float k = 0.08f + i * 0.21f;
                Vector3 p = Vector3.Lerp(tail, head, k) + Vector3.up * (Mathf.Sin(k * Mathf.PI) * 1.4f + 0.5f);
                Beam(kit, p, new Vector3(p.x, ceiling, p.z), 0.08f, IronMat);
            }
        }

        // A great brass armillary sphere on a stone base: three hoops slowly turning round a
        // glass globe of the sea, with little planets on them.
        static void Armillary(LevelKit kit, Vector3 foot)
        {
            Plinth(kit, foot, new Vector3(5.4f, 1.1f, 5.4f), 45f);
            kit.Block(foot + Vector3.up * 1.5f, new Vector3(3f, 0.8f, 3f), StoneCol, "ArmillaryBase", PaleStone);
            Vector3 c = foot + Vector3.up * 6.4f;
            TownKit.Solid(kit, foot + Vector3.up * 2.3f, new Vector3(2.4f, 0.9f, 2.4f), 0f);   // the cradle's feet
            // The cradle: four brass arms curving up to hold the fixed horizon ring.
            for (int i = 0; i < 4; i++)
            {
                var r = Rot(i * 90f + 45f);
                Arc(kit, foot + Vector3.up * 1.9f + r * new Vector3(0f, 0f, 0.8f), c + r * new Vector3(0f, 0f, 4.4f) + Vector3.down * 0.2f, 0.01f, 0.2f, 0.2f, Brass, 6);
            }
            var horizon = kit.Group("HorizonRing", c);
            Hoop(kit, horizon, 4.5f, 0.24f, Brass, 36);
            NoShadow(kit.Deco(PrimitiveType.Sphere, c, Vector3.one * 2.6f, PatchworkAssets.Glow(new Color(0.2f, 0.55f, 0.75f), 0.9f)));
            NoShadow(kit.Deco(PrimitiveType.Sphere, c, Vector3.one * 2.75f, PatchworkAssets.Bubble(new Color(0.7f, 0.9f, 1f, 0.2f))));
            // Continents: felt patches on the globe.
            var land = PatchworkAssets.Glow(new Color(0.85f, 0.75f, 0.45f), 0.5f);
            Vector3[] patches = { new Vector3(0.5f, 0.6f, 0.6f), new Vector3(-0.7f, 0.2f, 0.65f), new Vector3(0.2f, -0.5f, -0.8f), new Vector3(-0.4f, 0.7f, -0.5f), new Vector3(0.9f, -0.1f, -0.3f) };
            foreach (var d in patches)
                NoShadow(kit.Deco(PrimitiveType.Sphere, c + d.normalized * 1.24f, new Vector3(0.9f, 0.7f, 0.9f), land));
            PointLight(kit, c, new Color(0.5f, 0.8f, 1f), 14f, 1.3f);
            float[] radius = { 4.1f, 3.6f, 3.1f };
            Vector3[] tilt = { new Vector3(0f, 0f, 66f), new Vector3(40f, 30f, 0f), new Vector3(90f, 0f, 20f) };
            Color[] planet = { new Color(1f, 0.6f, 0.3f), new Color(0.6f, 0.8f, 1f), new Color(1f, 0.85f, 0.5f) };
            for (int i = 0; i < 3; i++)
            {
                var ring = kit.Group("ArmillaryRing", c);
                ring.rotation = Quaternion.Euler(tilt[i]);
                Hoop(kit, ring, radius[i], 0.16f, i == 1 ? PatchworkAssets.Plain(new Color(0.8f, 0.82f, 0.9f), 0.85f) : Brass, 30);
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(radius[i], 0f, 0f), Vector3.one * (0.7f - i * 0.1f), PatchworkAssets.Glow(planet[i], 1.3f), default(Vector3), ring));
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(-radius[i], 0f, 0f), Vector3.one * 0.3f, Brass, default(Vector3), ring));
                var spin = ring.gameObject.AddComponent<Spinner>();
                spin.Axis = i == 0 ? Vector3.right : i == 1 ? Vector3.forward : new Vector3(1f, 0f, 1f).normalized;
                spin.Speed = 9f + i * 5f;
            }
            RopeLine(kit, foot + new Vector3(-4.6f, 0f, -4.6f), foot + new Vector3(4.6f, 0f, -4.6f), Wine);
            RopeLine(kit, foot + new Vector3(-4.6f, 0f, 4.6f), foot + new Vector3(4.6f, 0f, 4.6f), Wine);
        }

        // A fossil ammonite: a spiral of beads getting bigger, standing on edge.
        static void Ammonite(LevelKit kit, Vector3 center, float size, Color col)
        {
            var m = PatchworkAssets.Felt(col);
            var dark = PatchworkAssets.Felt(Color.Lerp(col, WoodCol, 0.5f));
            for (int i = 0; i < 26; i++)
            {
                float a = i * 0.52f;
                float r = size * (0.08f + i * 0.036f);
                float s = size * (0.14f + i * 0.02f);
                NoShadow(kit.Deco(PrimitiveType.Sphere, center + new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f), new Vector3(s, s, s * 0.8f), i % 2 == 0 ? m : dark));
            }
        }

        // A model ship in a bottle, lying on its side on a little wooden cradle.
        static void ShipInBottle(LevelKit kit, Vector3 center)
        {
            NoShadow(kit.Deco(PrimitiveType.Capsule, center + Vector3.up * 0.25f, new Vector3(0.9f, 0.8f, 0.9f), PatchworkAssets.Bubble(new Color(0.75f, 0.95f, 0.9f, 0.25f)), new Vector3(0f, 0f, 90f)));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, center + new Vector3(0.95f, 0.25f, 0f), new Vector3(0.3f, 0.2f, 0.3f), PatchworkAssets.Bubble(new Color(0.75f, 0.95f, 0.9f, 0.3f)), new Vector3(0f, 0f, 90f)));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, center + new Vector3(1.2f, 0.25f, 0f), new Vector3(0.24f, 0.07f, 0.24f), PatchworkAssets.Felt(new Color(0.7f, 0.5f, 0.3f)), new Vector3(0f, 0f, 90f)));
            NoShadow(kit.Deco(PrimitiveType.Cube, center + new Vector3(-0.05f, 0.08f, 0f), new Vector3(0.9f, 0.14f, 0.24f), DarkWood));
            NoShadow(kit.Deco(PrimitiveType.Cube, center + new Vector3(-0.05f, 0.34f, 0f), new Vector3(0.03f, 0.5f, 0.03f), DarkWood));
            NoShadow(kit.Deco(PrimitiveType.Cube, center + new Vector3(-0.05f, 0.36f, 0f), new Vector3(0.36f, 0.3f, 0.02f), PatchworkAssets.Felt(CreamCol)));
            NoShadow(kit.Deco(PrimitiveType.Cube, center + new Vector3(0.22f, 0.3f, 0f), new Vector3(0.24f, 0.2f, 0.02f), PatchworkAssets.Felt(CreamCol)));
            for (int s = -1; s <= 1; s += 2)
                NoShadow(kit.Deco(PrimitiveType.Cube, center + new Vector3(s * 0.45f, -0.1f, 0f), new Vector3(0.1f, 0.3f, 0.8f), DarkWood));
        }
    }
}

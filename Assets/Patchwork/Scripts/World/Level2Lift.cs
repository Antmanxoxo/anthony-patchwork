using UnityEngine;

namespace Patchwork
{
    // The Observatory Express: a lift made out of a very large cardboard box, wedged into the
    // corner of the Sunken Ruins and disappearing up through the ceiling. Inside it's a cosy
    // little room with a rug, a dial, a panel of felt buttons and a bunny bellhop; the doors
    // slide shut, the lift music starts, and up you go (see CardboardLift for how).
    public static partial class Level2Builder
    {
        // The tower's footprint: x 44..52.4, z 6..14, in the south-west corner of the Ruins.
        const float TowerX0 = 44f, TowerX1 = 52.4f, TowerZ0 = 6f, TowerZ1 = 14f, TowerTop = 24f;
        static readonly Vector3 LiftLower = new Vector3(48.2f, 0f, 10f);
        static readonly Vector3 LiftUpper = new Vector3(48.2f, DeckY, 10f);
        const float LiftDoorW = 3.2f, LiftDoorH = 3.4f, CarHigh = 4.2f;

        static readonly Color Kraft = new Color(0.8f, 0.62f, 0.42f);
        static readonly Color KraftLight = new Color(0.9f, 0.76f, 0.56f);
        static readonly Color TapeCol = new Color(0.86f, 0.78f, 0.6f);

        static void Lift(LevelKit kit)
        {
            var box = PatchworkAssets.Cardboard(Kraft);
            var boxIn = PatchworkAssets.Cardboard(KraftLight);
            float zd0 = 10f - LiftDoorW * 0.5f, zd1 = 10f + LiftDoorW * 0.5f;
            float[] floors = { 0f, DeckY };

            // ----- The tower: four cardboard walls, holes for the doors at both floors -----
            foreach (float x in new[] { TowerX0 + 0.2f, TowerX1 - 0.2f })
            {
                Wall(kit, new Vector3(x, 0f, TowerZ0), new Vector3(x, 0f, zd0), 0f, TowerTop, 0.4f, box, "LiftWall");
                Wall(kit, new Vector3(x, 0f, zd1), new Vector3(x, 0f, TowerZ1), 0f, TowerTop, 0.4f, box, "LiftWall");
                Wall(kit, new Vector3(x, 0f, zd0), new Vector3(x, 0f, zd1), LiftDoorH, DeckY, 0.4f, box, "LiftWall");
                Wall(kit, new Vector3(x, 0f, zd0), new Vector3(x, 0f, zd1), DeckY + LiftDoorH, TowerTop, 0.4f, box, "LiftWall");
            }
            foreach (float z in new[] { TowerZ0 + 0.2f, TowerZ1 - 0.2f })
                Wall(kit, new Vector3(TowerX0, 0f, z), new Vector3(TowerX1, 0f, z), 0f, TowerTop, 0.4f, box, "LiftWall");
            // Ceilings, and the upper car's floor.
            kit.Block(new Vector3(48.2f, CarHigh + 0.2f, 10f), new Vector3(7.6f, 0.4f, 7.2f), Kraft, "LiftCeiling", boxIn);
            kit.Block(new Vector3(48.2f, DeckY - 0.2f, 10f), new Vector3(8.4f, 0.4f, 8f), Kraft, "LiftFloor", boxIn);
            kit.Block(new Vector3(48.2f, DeckY + CarHigh + 0.2f, 10f), new Vector3(7.6f, 0.4f, 7.2f), Kraft, "LiftCeiling", boxIn);

            // ----- The outside: tape, scribbles and arrows, ropes going up into the dark -----
            var tape = PatchworkAssets.Felt(TapeCol);
            var red = PatchworkAssets.Felt(new Color(0.85f, 0.3f, 0.3f));
            var ink = PatchworkAssets.Felt(new Color(0.25f, 0.2f, 0.3f));
            var mb = new MeshBatch();
            // Parcel tape round the corners and across the seams, every few metres.
            foreach (var corner in new[] { new Vector3(TowerX1, 0f, TowerZ1), new Vector3(TowerX1, 0f, TowerZ0 + 0.05f) })
                mb.Box(corner + Vector3.up * TowerTop * 0.5f, new Vector3(0.5f, TowerTop, 0.5f) + Vector3.one * 0.04f, Quaternion.identity);
            for (float y = 6f; y < TowerTop; y += 5.5f)
            {
                mb.Box(new Vector3(TowerX1 + 0.02f, y, 10f), new Vector3(0.04f, 0.4f, 8f), Quaternion.identity);
                mb.Box(new Vector3((TowerX0 + TowerX1) * 0.5f, y, TowerZ1 + 0.02f), new Vector3(8.4f, 0.4f, 0.04f), Quaternion.identity);
            }
            mb.Build(kit.Root, "LiftTape", tape);
            // "This way up": big painted arrows climbing the east face, as if the box itself
            // knows where it's going.
            var arrows = new MeshBatch();
            for (int i = 0; i < 3; i++)
            {
                Vector3 a = new Vector3(TowerX1 + 0.03f, 7f + i * 5.5f, 12.4f);
                arrows.Box(a, new Vector3(0.03f, 1.6f, 0.4f), Quaternion.identity);
                arrows.Box(a + new Vector3(0f, 0.75f, -0.3f), new Vector3(0.03f, 0.9f, 0.32f), Quaternion.Euler(45f, 0f, 0f));
                arrows.Box(a + new Vector3(0f, 0.75f, 0.3f), new Vector3(0.03f, 0.9f, 0.32f), Quaternion.Euler(-45f, 0f, 0f));
                // A wine glass, for "fragile".
                Vector3 g = new Vector3(TowerX1 + 0.03f, 6.4f + i * 5.5f, 8f);
                arrows.Ball(g + Vector3.up * 0.55f, new Vector3(0.04f, 0.5f, 0.55f), 2, 8);
                arrows.Box(g, new Vector3(0.03f, 0.7f, 0.08f), Quaternion.identity);
                arrows.Box(g + Vector3.down * 0.35f, new Vector3(0.03f, 0.08f, 0.4f), Quaternion.identity);
            }
            arrows.Build(kit.Root, "LiftArrows", ink);
            // A big felt label on the north face: a heart and a star, stitched on.
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(49.5f, 8f, TowerZ1 + 0.05f), new Vector3(3f, 2f, 0.06f), PatchworkAssets.Felt(CreamCol)));
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(48.9f, 8.15f, TowerZ1 + 0.1f), new Vector3(0.6f, 0.6f, 0.08f), red));
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(49.3f, 8.15f, TowerZ1 + 0.1f), new Vector3(0.6f, 0.6f, 0.08f), red));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(49.1f, 7.85f, TowerZ1 + 0.1f), new Vector3(0.45f, 0.45f, 0.07f), red, new Vector3(0f, 0f, 45f)));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(50.3f, 8f, TowerZ1 + 0.1f), new Vector3(0.6f, 0.6f, 0.07f), PatchworkAssets.Glow(new Color(1f, 0.85f, 0.45f), 1.2f), new Vector3(0f, 0f, 45f)));
            // Ropes and a pulley wheel, vanishing up into the ceiling.
            for (int i = 0; i < 2; i++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(TowerX1 + 0.5f, 14f, 7f + i * 0.5f), new Vector3(0.09f, 10f, 0.09f), PatchworkAssets.Felt(new Color(0.75f, 0.62f, 0.45f)));
            var wheel = kit.Deco(PrimitiveType.Cylinder, new Vector3(TowerX1 + 0.5f, 22.4f, 7.25f), new Vector3(1.4f, 0.12f, 1.4f), Brass, new Vector3(0f, 0f, 90f));
            wheel.gameObject.AddComponent<Spinner>();
            // A crank handle beside the door (purely for show; nobody's turned it in years).
            kit.Deco(PrimitiveType.Cylinder, new Vector3(TowerX1 + 0.25f, 1.6f, 6.9f), new Vector3(0.5f, 0.1f, 0.5f), IronMat, new Vector3(0f, 0f, 90f));
            kit.Deco(PrimitiveType.Cube, new Vector3(TowerX1 + 0.4f, 1.85f, 6.9f), new Vector3(0.08f, 0.5f, 0.08f), IronMat);
            kit.Deco(PrimitiveType.Sphere, new Vector3(TowerX1 + 0.55f, 2.1f, 6.9f), Vector3.one * 0.18f, red);

            // ----- The cars: the same little room at the bottom and at the top -----
            var lowDoors = new System.Collections.Generic.List<LiftDoor>();
            var highDoors = new System.Collections.Generic.List<LiftDoor>();
            foreach (float fy in floors)
            {
                Vector3 c = new Vector3(48.2f, fy, 10f);
                bool top = fy > 1f;
                // Doors on both sides; only one pair on each floor ever opens.
                for (int side = -1; side <= 1; side += 2)
                {
                    bool works = top ? side < 0 : side > 0;
                    float x = side > 0 ? TowerX1 - 0.48f : TowerX0 + 0.48f;
                    for (int leaf = -1; leaf <= 1; leaf += 2)
                    {
                        var g = kit.Group("LiftDoor", new Vector3(x, fy, 10f + leaf * LiftDoorW * 0.25f));
                        NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, LiftDoorH * 0.5f, 0f), new Vector3(0.14f, LiftDoorH, LiftDoorW * 0.5f + 0.02f), box, default(Vector3), g));
                        // A strip of tape down the meeting edge, and a round porthole doodle.
                        NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, LiftDoorH * 0.5f, -leaf * (LiftDoorW * 0.25f - 0.08f)), new Vector3(0.17f, LiftDoorH, 0.16f), tape, default(Vector3), g));
                        NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 2.3f, 0f), new Vector3(0.6f, 0.09f, 0.6f), PatchworkAssets.Felt(new Color(0.55f, 0.75f, 0.85f)), new Vector3(0f, 0f, 90f), g));
                        var col = g.gameObject.AddComponent<BoxCollider>();
                        col.center = new Vector3(0f, LiftDoorH * 0.5f, 0f);
                        col.size = new Vector3(0.2f, LiftDoorH, LiftDoorW * 0.5f);
                        if (!works) continue;
                        var door = g.gameObject.AddComponent<LiftDoor>();
                        door.Closed = g.localPosition;
                        door.Slide = new Vector3(0f, 0f, leaf * (LiftDoorW * 0.5f - 0.1f));
                        (top ? highDoors : lowDoors).Add(door);
                    }
                }
                CarInside(kit, c, top, boxIn, tape, red);
            }

            // ----- The working parts -----
            var lift = kit.Group("ObservatoryExpress", LiftLower).gameObject.AddComponent<CardboardLift>();
            lift.LowerCenter = LiftLower;
            lift.UpperCenter = LiftUpper;
            lift.CameraOffset = new Vector3(-3.25f, 3.45f, -3.05f);
            lift.LookOffset = new Vector3(1.6f, 1.2f, 2.2f);
            lift.LowerDoorPoint = new Vector3(TowerX1 + 1.5f, 1f, 10f);
            lift.UpperDoorPoint = new Vector3(TowerX0 - 1.5f, DeckY + 1f, 10f);
            lift.LowerDoors = lowDoors.ToArray();
            lift.UpperDoors = highDoors.ToArray();
            lift.HeartsNeeded = HeartstringsNeeded;
            var src = lift.gameObject.AddComponent<AudioSource>();
            src.clip = MusicPlayer.LiftMusic();
            src.loop = true;
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            src.volume = 0f;
            src.ignoreListenerPause = true;
            lift.Music = src;
            for (int i = 0; i < 2; i++)
            {
                bool top = i == 1;
                var spot = kit.Volume<UseSpot>((top ? LiftUpper : LiftLower) + new Vector3(-0.8f, 1.5f, -0.6f), new Vector3(4.2f, 3f, 4.4f), "LiftButton");
                spot.Label = top ? "Ride down" : "Ride up";
                spot.PromptPoint = (top ? LiftUpper : LiftLower) + new Vector3(0.6f, 2.6f, 3f);
                spot.Used = p => lift.Ride(top);
            }

            // ----- The "out of order" sign, until three Heartstrings are found -----
            var gate = kit.Group("LiftGate", new Vector3(TowerX1 + 0.12f, 0f, 10f));
            var card = PatchworkAssets.Felt(CreamCol);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.9f, 0f), new Vector3(0.05f, 1.3f, 2f), card, default(Vector3), gate));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0.03f, 1.9f, 0f), new Vector3(0.04f, 1.42f, 2.12f), red, default(Vector3), gate));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0.07f, 1.9f, 0f), new Vector3(0.03f, 1.2f, 1.9f), card, default(Vector3), gate));
            // Three hearts on it, waiting to be filled.
            var heartGlow = PatchworkAssets.Glow(new Color(1f, 0.6f, 0.4f), 0.9f);
            for (int h = -1; h <= 1; h++)
            {
                Vector3 hc = new Vector3(0.1f, 1.9f, h * 0.6f);
                NoShadow(kit.Deco(PrimitiveType.Sphere, hc + new Vector3(0f, 0.1f, -0.1f), new Vector3(0.05f, 0.24f, 0.24f), heartGlow, default(Vector3), gate));
                NoShadow(kit.Deco(PrimitiveType.Sphere, hc + new Vector3(0f, 0.1f, 0.1f), new Vector3(0.05f, 0.24f, 0.24f), heartGlow, default(Vector3), gate));
                NoShadow(kit.Deco(PrimitiveType.Cube, hc + new Vector3(0f, -0.04f, 0f), new Vector3(0.05f, 0.2f, 0.2f), heartGlow, new Vector3(45f, 0f, 0f), gate));
            }
            // Hung on a string from a drawing pin.
            Beam(kit, new Vector3(TowerX1 + 0.14f, 2.6f, 9.1f), new Vector3(TowerX1 + 0.14f, 3.2f, 10f), 0.02f, ink);
            Beam(kit, new Vector3(TowerX1 + 0.14f, 2.6f, 10.9f), new Vector3(TowerX1 + 0.14f, 3.2f, 10f), 0.02f, ink);
            director.LiftGate = gate.gameObject;
            director.LiftSpot = LiftLower;

            // Floor dials over both outer doors.
            FloorDial(kit, new Vector3(TowerX1 + 0.05f, LiftDoorH + 0.75f, 10f), 90f);
            FloorDial(kit, new Vector3(TowerX0 - 0.05f, DeckY + LiftDoorH + 0.75f, 10f), 270f);
            // A call button by the bottom door, and a lamp over it.
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(TowerX1 + 0.04f, 1.4f, 12.3f), new Vector3(0.06f, 0.7f, 0.42f), Brass));
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(TowerX1 + 0.09f, 1.55f, 12.3f), new Vector3(0.1f, 0.22f, 0.22f), PatchworkAssets.Glow(new Color(1f, 0.75f, 0.4f), 2f)));
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(TowerX1 + 0.09f, 1.25f, 12.3f), new Vector3(0.1f, 0.22f, 0.22f), PatchworkAssets.Glow(new Color(0.6f, 0.85f, 1f), 1.6f)));
            PointLight(kit, new Vector3(TowerX1 + 2.5f, 5.5f, 10f), LampGold, 12f, 1.1f);
            PointLight(kit, new Vector3(TowerX0 - 1.2f, DeckY + 4.5f, 10f), LampGold, 9f, 0.9f);

            kit.Hint(new Vector3(TowerX1 + 4f, 1.5f, 11f), new Vector3(6f, 4f, 8f), "The Observatory Express",
                "The lift to the Observatory, finest cardboard. It wakes up once you've found three Heartstrings: then walk in and press {Ability}.");
            Plaque(kit, new Vector3(TowerX1 + 2.2f, 0f, 15.6f), 120f, "The Lift",
                "Elisha Otis showed off his safety lift in New York in 1854. He rode the platform up high, then had the only rope cut in front of the crowd. The brakes caught, and he raised his hat.",
                "Without lifts nobody would build tall buildings. This one is made of cardboard, which Mr. Otis never tried.");
        }

        // One car's interior: a rug, a ceiling lamp, a dial and a panel of buttons, a little
        // speaker, a potted plant and a bunny bellhop who loves his job.
        static void CarInside(LevelKit kit, Vector3 c, bool top, Material wall, Material tape, Material red)
        {
            Rug(kit, c + new Vector3(0f, 0.02f, 0f), new Vector2(5.6f, 5.2f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, Wine, new Color(1f, 0.8f, 0.55f)), 0f, PatchworkAssets.Felt(new Color(1f, 0.82f, 0.45f)));
            // A stitched skirting of tape round the room.
            var trim = new MeshBatch();
            trim.Box(c + new Vector3(0f, 0.2f, 3.58f), new Vector3(7.5f, 0.4f, 0.04f), Quaternion.identity);
            trim.Box(c + new Vector3(0f, 0.2f, -3.58f), new Vector3(7.5f, 0.4f, 0.04f), Quaternion.identity);
            trim.Box(c + new Vector3(0f, CarHigh - 0.2f, 3.58f), new Vector3(7.5f, 0.3f, 0.04f), Quaternion.identity);
            trim.Box(c + new Vector3(0f, CarHigh - 0.2f, -3.58f), new Vector3(7.5f, 0.3f, 0.04f), Quaternion.identity);
            trim.Stitches(c + new Vector3(-3.6f, 0.45f, 3.55f), c + new Vector3(3.6f, 0.45f, 3.55f));
            trim.Stitches(c + new Vector3(-3.6f, 0.45f, -3.55f), c + new Vector3(3.6f, 0.45f, -3.55f));
            trim.Build(kit.Root, "LiftTrim", tape);
            // The lamp.
            NoShadow(kit.Deco(PrimitiveType.Cylinder, c + new Vector3(0f, CarHigh - 0.05f, 0f), new Vector3(1.2f, 0.05f, 1.2f), Brass));
            NoShadow(kit.Deco(PrimitiveType.Sphere, c + new Vector3(0f, CarHigh - 0.12f, 0f), new Vector3(0.9f, 0.3f, 0.9f), PatchworkAssets.Glow(new Color(1f, 0.85f, 0.6f), 2.2f)));
            PointLight(kit, c + new Vector3(0f, CarHigh - 0.6f, 0f), new Color(1f, 0.82f, 0.6f), 9f, 1.3f);
            // The button panel on the north wall, with a dial above it.
            Vector3 panel = c + new Vector3(1.2f, 1.5f, 3.56f);
            NoShadow(kit.Deco(PrimitiveType.Cube, panel, new Vector3(0.75f, 1.3f, 0.05f), Brass));
            string[] tints = { "#ffcf66", "#9fd6ff", "#ff8f8f" };
            for (int i = 0; i < 3; i++)
            {
                ColorUtility.TryParseHtmlString(tints[i], out Color tc);
                NoShadow(kit.Deco(PrimitiveType.Sphere, panel + new Vector3(0f, 0.38f - i * 0.38f, -0.05f), new Vector3(0.24f, 0.24f, 0.1f), PatchworkAssets.Glow(tc, (top ? i == 1 : i == 0) ? 2.4f : 0.7f)));
            }
            FloorDial(kit, c + new Vector3(0f, CarHigh - 0.75f, 3.56f), 180f);
            // The speaker the music comes out of: a brass grille in the ceiling corner.
            NoShadow(kit.Deco(PrimitiveType.Cylinder, c + new Vector3(-2.8f, CarHigh - 0.04f, 2.7f), new Vector3(0.8f, 0.04f, 0.8f), Brass));
            for (int i = 0; i < 6; i++)
            {
                float a = i * 60f * Mathf.Deg2Rad;
                NoShadow(kit.Deco(PrimitiveType.Sphere, c + new Vector3(-2.8f + Mathf.Cos(a) * 0.22f, CarHigh - 0.08f, 2.7f + Mathf.Sin(a) * 0.22f), Vector3.one * 0.09f, IronMat));
            }
            // A potted cardboard palm in the north-west corner.
            Vector3 pot = c + new Vector3(-3.1f, 0f, 2.9f);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, pot + Vector3.up * 0.35f, new Vector3(0.7f, 0.35f, 0.7f), red));
            for (int i = 0; i < 5; i++)
            {
                float a = i * 72f;
                NoShadow(kit.Deco(PrimitiveType.Cube, pot + Vector3.up * 1.2f + Rot(a) * new Vector3(0f, 0f, 0.3f), new Vector3(0.08f, 0.9f, 0.3f), PatchworkAssets.Felt(new Color(0.4f, 0.7f, 0.45f)), new Vector3(30f, a, 0f)));
            }
            // A framed picture on the south wall: a whale going by (for the view at the top).
            Vector3 pic = c + new Vector3(0.6f, 2.1f, -3.56f);
            NoShadow(kit.Deco(PrimitiveType.Cube, pic, new Vector3(1.6f, 1.1f, 0.06f), Brass));
            NoShadow(kit.Deco(PrimitiveType.Cube, pic + new Vector3(0f, 0f, 0.04f), new Vector3(1.4f, 0.9f, 0.04f), PatchworkAssets.Felt(Navy)));
            NoShadow(kit.Deco(PrimitiveType.Sphere, pic + new Vector3(0f, -0.05f, 0.07f), new Vector3(0.8f, 0.3f, 0.04f), PatchworkAssets.Felt(new Color(0.55f, 0.7f, 0.85f))));
            NoShadow(kit.Deco(PrimitiveType.Sphere, pic + new Vector3(0.4f, 0.28f, 0.07f), Vector3.one * 0.08f, PatchworkAssets.Glow(new Color(1f, 0.9f, 0.6f), 1.5f)));
            // The bellhop: a felt bunny in a pillbox hat, standing ready by the buttons.
            var hop = kit.Group("Bellhop", c + new Vector3(2.85f, 0f, 2.7f));
            hop.rotation = Rot(225f);
            var coat = PatchworkAssets.Felt(new Color(0.75f, 0.2f, 0.28f));
            var fur = PatchworkAssets.Felt(new Color(0.96f, 0.92f, 0.86f));
            NoShadow(kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.7f, 0f), new Vector3(0.6f, 0.55f, 0.5f), coat, default(Vector3), hop));
            for (int b = 0; b < 3; b++) NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.5f + b * 0.2f, 0.25f), Vector3.one * 0.06f, Brass, default(Vector3), hop));
            var head = kit.Group("Head", Vector3.zero);
            head.SetParent(hop, false);
            head.localPosition = new Vector3(0f, 1.45f, 0f);
            NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.5f, 0.46f, 0.46f), fur, default(Vector3), head));
            for (int s = -1; s <= 1; s += 2)
            {
                NoShadow(kit.Deco(PrimitiveType.Capsule, new Vector3(s * 0.12f, 0.42f, -0.02f), new Vector3(0.13f, 0.26f, 0.08f), fur, new Vector3(0f, 0f, s * -12f), head));
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.1f, 0.05f, 0.21f), Vector3.one * 0.06f, PatchworkAssets.Felt(Navy), default(Vector3), head));
            }
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.05f, 0.23f), new Vector3(0.07f, 0.05f, 0.05f), PatchworkAssets.Felt(new Color(1f, 0.6f, 0.65f)), default(Vector3), head));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0.05f, 0.24f, 0f), new Vector3(0.24f, 0.07f, 0.24f), coat, new Vector3(0f, 0f, -10f), head));
            head.gameObject.AddComponent<WatchPlayer>().Range = 7f;
            var bob = hop.gameObject.AddComponent<SeaBob>();
            bob.Height = 0.03f;
            bob.Rock = 2f;
            bob.Speed = 1.4f;
            TownKit.Solid(kit, hop.position + Vector3.up * 0.8f, new Vector3(0.6f, 1.6f, 0.6f), 0f);
        }

        // A semicircular floor dial with a needle (Ground on the left, Top on the right).
        static void FloorDial(LevelKit kit, Vector3 at, float yaw)
        {
            var g = kit.Group("FloorDial", at);
            g.rotation = Rot(yaw);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.1f, 0f), new Vector3(1.1f, 0.03f, 1.1f), Brass, new Vector3(90f, 0f, 0f), g));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.1f, 0.03f), new Vector3(0.95f, 0.02f, 0.95f), PatchworkAssets.Felt(CreamCol), new Vector3(90f, 0f, 0f), g));
            // The ticks: a dot for the ground floor, a star for the top.
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(-0.32f, 0.05f, 0.05f), Vector3.one * 0.09f, PatchworkAssets.Felt(Teal), default(Vector3), g));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0.32f, 0.05f, 0.05f), new Vector3(0.12f, 0.12f, 0.03f), PatchworkAssets.Glow(new Color(1f, 0.85f, 0.45f), 1.4f), new Vector3(0f, 0f, 45f), g));
            var pivot = new GameObject("Needle").transform;
            pivot.SetParent(g, false);
            pivot.localPosition = new Vector3(0f, -0.1f, 0.07f);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.2f, 0f), new Vector3(0.04f, 0.4f, 0.02f), PatchworkAssets.Felt(new Color(0.8f, 0.25f, 0.25f)), default(Vector3), pivot));
            pivot.gameObject.AddComponent<LiftDial>();
        }
    }
}

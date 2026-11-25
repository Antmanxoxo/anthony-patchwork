using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The Common Quarter and the keep: the half of Toybox Town that grew up across the canal,
    // laid out like a real market town (the plan was drawn up from Vernworth's and Kuttenberg's
    // street plans, then shrunk to toybox size).
    //   - Quayside runs the length of the canal; the High Street curves up the middle of the
    //     quarter from the Toybox Gate to the north water gate, three and four storeys tall.
    //   - Cross streets tie the two together: Bridge Row, Tanners' Lane, Mill Street, Spinners'
    //     Lane, Quay Lane and Bell Lane; East Gate Road leads off Mill Square to the East Gate.
    //   - Squares: Gate Square inside the Toybox Gate (the guardhouse and a market), Mill Square
    //     in front of the Dye Works, Chapel Square before St. Stitch's, and Teapot Quay.
    //   - The landmarks that were already here stay where they were: the Dye Works, the Toy
    //     Workshop and the Tea House; the new ones are St. Stitch's chapel and the keep.
    //   - The keep crowns the north of town, past the Tournament Green, in its own walled bailey.
    // Small things that the rest of the level put down where the houses and streets now are get
    // tidied away at the end (see CityClearance).
    public static partial class Level1Builder
    {
        // Generated from the city plan: x, z, front width, depth, wall height, facing, wall colour, roof colour.
        static readonly float[] CityLots =
        {
            49.88f, -54.36f, 4.03f, 5.00f, 6.37f, -90f, 1f, 0f,
            49.88f, -49.99f, 4.22f, 5.00f, 5.79f, -90f, 3f, 1f,
            49.88f, -45.48f, 4.31f, 5.00f, 9.14f, -90f, 4f, 1f,
            55.02f, -50.12f, 5.00f, 4.28f, 8.80f, 180f, 0f, 0f,
            55.02f, -45.53f, 5.00f, 4.41f, 6.32f, 0f, 1f, 1f,
            61.71f, -34.91f, 6.87f, 3.92f, 5.85f, 180f, 1f, 3f,
            61.71f, -30.13f, 6.87f, 5.15f, 8.72f, 180f, 3f, 1f,
            61.71f, -25.11f, 6.87f, 4.38f, 5.91f, 0f, 2f, 2f,
            68.73f, -34.50f, 6.87f, 4.75f, 9.06f, 180f, 2f, 2f,
            68.73f, -29.70f, 4.34f, 6.87f, 9.26f, 90f, 1f, 3f,
            68.73f, -25.10f, 6.87f, 4.36f, 9.26f, 0f, 0f, 2f,
            50.43f, -13.15f, 7.95f, 6.02f, 8.99f, -90f, 4f, 0f,
            55.70f, -13.15f, 4.01f, 7.95f, 8.74f, 180f, 1f, 4f,
            60.73f, -13.15f, 5.54f, 7.95f, 5.80f, 180f, 1f, 3f,
            66.62f, -13.15f, 5.74f, 7.95f, 9.11f, 180f, 4f, 2f,
            71.75f, -13.15f, 4.00f, 7.95f, 12.48f, 180f, 2f, 2f,
            50.95f, -0.85f, 3.85f, 7.15f, 5.97f, -90f, 4f, 0f,
            50.95f, 4.16f, 5.68f, 7.15f, 9.25f, -90f, 2f, 1f,
            50.95f, 9.23f, 3.96f, 7.15f, 6.20f, -90f, 2f, 3f,
            50.95f, 13.69f, 4.46f, 7.15f, 6.28f, -90f, 0f, 4f,
            50.95f, 18.67f, 5.00f, 7.15f, 8.73f, -90f, 4f, 2f,
            57.89f, 18.10f, 5.14f, 6.25f, 6.34f, 0f, 2f, 1f,
            62.93f, 18.10f, 4.43f, 6.25f, 6.49f, 0f, 4f, 1f,
            67.57f, 18.10f, 4.37f, 6.25f, 8.79f, 0f, 3f, 2f,
            71.84f, 18.10f, 3.67f, 6.25f, 9.15f, 0f, 3f, 2f,
            49.54f, 30.75f, 7.55f, 4.23f, 9.25f, -90f, 2f, 3f,
            54.84f, 30.75f, 5.87f, 7.55f, 8.93f, 180f, 3f, 1f,
            60.65f, 30.75f, 5.24f, 7.55f, 6.00f, 180f, 4f, 1f,
            65.66f, 30.75f, 4.29f, 7.55f, 9.39f, 180f, 1f, 0f,
            70.58f, 30.75f, 5.03f, 7.55f, 9.41f, 180f, 4f, 4f,
            51.22f, 63.10f, 4.76f, 7.68f, 6.31f, -90f, 2f, 4f,
            51.22f, 68.56f, 5.66f, 7.68f, 9.37f, -90f, 2f, 2f,
            51.22f, 74.46f, 5.63f, 7.68f, 9.22f, -90f, 4f, 0f,
            59.05f, 63.34f, 7.68f, 5.24f, 9.07f, 180f, 4f, 2f,
            59.05f, 69.16f, 5.90f, 7.68f, 8.92f, 90f, 2f, 3f,
            59.05f, 74.82f, 7.68f, 4.91f, 9.24f, 0f, 1f, 3f,
            49.56f, 85.71f, 5.97f, 4.37f, 6.12f, -90f, 1f, 1f,
            86.79f, -14.62f, 5.50f, 7.42f, 6.19f, -90f, 4f, 4f,
            86.79f, -8.84f, 5.56f, 7.42f, 8.94f, -90f, 4f, 2f,
            86.79f, -3.02f, 5.59f, 7.42f, 5.86f, -90f, 4f, 3f,
            84.73f, 18.54f, 4.00f, 5.40f, 12.14f, -90f, 1f, 3f,
            84.73f, 23.36f, 5.13f, 5.40f, 9.39f, -90f, 3f, 3f,
            84.73f, 28.17f, 4.00f, 5.40f, 9.25f, -90f, 4f, 0f,
            84.73f, 32.95f, 5.05f, 5.40f, 9.36f, -90f, 2f, 0f,
            90.27f, 13.38f, 5.40f, 3.91f, 9.47f, 180f, 4f, 4f,
            90.27f, 18.08f, 5.40f, 4.98f, 8.78f, 180f, 4f, 4f,
            90.27f, 23.23f, 4.81f, 5.40f, 9.46f, -90f, 2f, 2f,
            90.27f, 28.30f, 4.83f, 5.40f, 6.04f, -90f, 4f, 4f,
            90.27f, 33.22f, 4.51f, 5.40f, 9.07f, -90f, 1f, 4f,
            83.84f, -20.30f, 5.25f, 3.71f, 9.15f, -90f, 4f, 0f,
            87.88f, -20.30f, 5.25f, 3.87f, 9.36f, -90f, 0f, 3f,
            84.94f, 69.15f, 6.04f, 6.00f, 9.29f, -90f, 2f, 3f,
        };

        public static readonly Vector2[] HighStreet =
        {
            new Vector2(58f, -69f), new Vector2(64f, -60f), new Vector2(72f, -50f), new Vector2(77f, -36f), new Vector2(78f, -20f), new Vector2(78f, 20f),
            new Vector2(77f, 40f), new Vector2(74f, 60f), new Vector2(68f, 76f), new Vector2(60f, 90f), new Vector2(50f, 102f), new Vector2(44f, 107f),
        };
        // (start x, z, end x, width) for the cross streets, east from the quay.
        static readonly Vector4[] CrossStreets =
        {
            new Vector4(47f, -40f, 76f, 5f), new Vector4(47f, -20f, 78f, 4.5f), new Vector4(47f, -6f, 78f, 5f), new Vector4(47f, 24f, 78f, 4.5f),
            new Vector4(47f, 58f, 75f, 4.5f), new Vector4(47f, 80f, 66f, 4f), new Vector4(81.5f, 8f, 95.2f, 6f),
        };
        static readonly string[] CrossNames = { "Bridge Row", "Tanners' Lane", "Mill Street", "Spinners' Lane", "Quay Lane", "Bell Lane", "East Gate Road" };
        static readonly Vector2 FarmLaneA = new Vector2(76f, 64f), FarmLaneB = new Vector2(84.8f, 93.2f);
        // (x, z, radius)
        static readonly Vector3[] CitySquares = { new Vector3(63f, -60f, 8f), new Vector3(80f, 8f, 7f), new Vector3(47.6f, -30f, 4f), new Vector3(78f, 46f, 6f) };
        static readonly string[] SquareNames = { "Gate Square", "Mill Square", "Teapot Quay", "Chapel Square" };
        // The keep's bailey (x0, z0, x1, z1) and the keep itself.
        static readonly Vector4 Bailey = new Vector4(-27f, 90f, 2f, 111f);
        static readonly Vector3 KeepAt = new Vector3(-13f, 0f, 100.5f);
        // Castle Way: from the far side of the Tournament Green (the green itself stays grass) past
        // the keep's gate to the North Gate.
        static readonly Vector2[] CastleWay = { new Vector2(3f, 88f), new Vector2(6.5f, 93f), new Vector2(8f, 99f), new Vector2(8f, 120f) };
        static readonly Vector4 Chapel = new Vector4(82.5f, 36f, 91.5f, 62f);

        static Vector2[] highStreetSmooth;
        static Vector2[] HighStreetPath { get { if (highStreetSmooth == null) highStreetSmooth = Smooth(HighStreet, 3); return highStreetSmooth; } }

        // Everything the city builds, so the tidy-up at the end leaves it alone.
        static readonly HashSet<Transform> cityThings = new HashSet<Transform>();
        static readonly List<Bounds> cityFootprints = new List<Bounds>();

        static void City(LevelKit kit)
        {
            int first = kit.Root.childCount;
            cityFootprints.Clear();
            cityThings.Clear();
            var root = kit.Group("CommonQuarter", Vector3.zero);
            var cobble = PatchworkAssets.Cobble(CobbleCol);
            var paler = PatchworkAssets.Cobble(new Color(0.8f, 0.76f, 0.7f));
            var setts = PatchworkAssets.Cobble(new Color(0.76f, 0.7f, 0.64f));

            // ---- Streets and squares.
            Ribbon(kit, root, "HighStreet", HighStreetPath, 7f, 0.055f, cobble);
            AddStreetFootprint(HighStreetPath, 7f);
            // Quayside: the old promenade covers the middle; pave the two new ends.
            Ribbon(kit, root, "Quayside", new[] { new Vector2(43.5f, CanalSouthEnd + 1f), new Vector2(43.5f, -60f) }, 7f, 0.04f, cobble);
            Ribbon(kit, root, "Quayside", new[] { new Vector2(43.5f, 60f), new Vector2(43.5f, CanalNorthEnd - 1f) }, 7f, 0.04f, cobble);
            for (int i = 0; i < CrossStreets.Length; i++)
            {
                var c = CrossStreets[i];
                var path = new[] { new Vector2(c.x, c.y), new Vector2(c.z, c.y) };
                Ribbon(kit, root, CrossNames[i].Replace(" ", "").Replace("'", ""), path, c.w, 0.045f, setts);
                AddStreetFootprint(path, c.w);
            }
            {
                var way = Smooth(CastleWay, 3);
                Ribbon(kit, root, "CastleWay", way, 5f, 0.045f, setts);
                AddStreetFootprint(way, 5f);
                var spur = new[] { new Vector2(7f, (Bailey.y + Bailey.w) * 0.5f), new Vector2(Bailey.z + 0.6f, (Bailey.y + Bailey.w) * 0.5f) };
                Ribbon(kit, root, "KeepRoad", spur, 4f, 0.05f, setts);
            }
            {
                var lane = new[] { FarmLaneA, FarmLaneB };
                Ribbon(kit, root, "FarmLane", lane, 3.6f, 0.045f, setts);
                AddStreetFootprint(lane, 3.6f);
            }
            for (int i = 0; i < CitySquares.Length; i++)
            {
                var sq = CitySquares[i];
                var paving = kit.Deco(PrimitiveType.Cylinder, new Vector3(sq.x, 0.065f, sq.y), new Vector3(sq.z * 2f, 0.01f, sq.z * 2f), paler);
                paving.name = "SquarePaving";
                ClearGrass(new Vector3(sq.x, 0f, sq.y), sq.z);
                // (Not Teapot Quay: that's the old Tea House's own corner, already as it should be.)
                if (SquareNames[i] != "Teapot Quay") cityFootprints.Add(new Bounds(new Vector3(sq.x, 0f, sq.y), new Vector3(sq.z * 1.6f, 2f, sq.z * 1.6f)));
                kit.Info.MapPins.Add(new MapPin(SquareNames[i], new Vector3(sq.x, 0f, sq.y)));
            }

            // ---- The houses: tall and narrow, wall to wall, the tallest on the High Street.
            Color[] walls = Walls;
            Color[] roofs = Roofs;
            TownKit.Sign[] shopSigns = { TownKit.Sign.Bread, TownKit.Sign.Hat, TownKit.Sign.Spool, TownKit.Sign.Toy, TownKit.Sign.Clock, TownKit.Sign.Teacup };
            int shops = 0;
            for (int i = 0; i + 7 < CityLots.Length; i += 8)
            {
                var at = new Vector3(CityLots[i], 0f, CityLots[i + 1]);
                float w = CityLots[i + 2], d = CityLots[i + 3], h = CityLots[i + 4], yaw = CityLots[i + 5];
                int wall = (int)CityLots[i + 6], roof = (int)CityLots[i + 7];
                bool onHigh = h >= 8.9f;
                var sign = TownKit.Sign.None;
                if (onHigh && (i / 8) % 3 == 0) sign = shopSigns[shops++ % shopSigns.Length];
                int chimney = (i / 8) % 3 == 0 ? 1 : (i / 8) % 3 == 1 ? -1 : 0;
                TownKit.MakeHouse(kit, at, new Vector3(w, h, d), yaw, walls[wall % walls.Length], roofs[roof % roofs.Length], 500 + i / 8, chimney, (i % 16 == 0 ? -0.25f : 0.25f) * w, sign);
                bool side = Mathf.Abs(Mathf.Repeat(yaw, 180f) - 90f) < 1f;
                cityFootprints.Add(new Bounds(at + Vector3.up * h * 0.5f, side ? new Vector3(d, h, w) : new Vector3(w, h, d)));
            }

            // ---- Lamps, bunting and life along the High Street.
            float run = 0f, next = 6f;
            int alt = 1, bunt = 0;
            var hs = HighStreetPath;
            for (int i = 1; i < hs.Length - 1; i++)
            {
                run += Vector2.Distance(hs[i - 1], hs[i]);
                if (run < next) continue;
                next = run + 13f;
                Vector2 dir = (hs[i + 1] - hs[i - 1]).normalized, n = new Vector2(-dir.y, dir.x);
                Vector2 q = hs[i] + n * alt * 2.9f;
                var lamp = new Vector3(q.x, 0.05f, q.y);
                if (!NearSquare(lamp, 1f)) TownKit.StreetLamp(kit, lamp);
                alt = -alt;
                // Bunting across the street every other lamp, between the upper floors.
                if (bunt++ % 2 == 0)
                {
                    Vector2 l = hs[i] + n * 3.2f, r = hs[i] - n * 3.2f;
                    kit.Bunting(new Vector3(l.x, 6.4f, l.y), new Vector3(r.x, 6.4f, r.y), Heraldic);
                }
            }
            // Gate Square: a market inside the gate, and the guardhouse.
            var gs = CitySquares[0];
            Color[] stripes = { LevelKit.Palette.Rose, LevelKit.Palette.Sky, LevelKit.Palette.Butter, LevelKit.Palette.Sage };
            for (int k = 0; k < 4; k++)
            {
                float a = (k * 90f + 30f) * Mathf.Deg2Rad;
                var p = new Vector3(gs.x + Mathf.Cos(a) * (gs.z - 2.4f), 0.06f, gs.y + Mathf.Sin(a) * (gs.z - 2.4f));
                MarketStall(kit, p, -k * 90f - 30f + 90f, stripes[k], k);
            }
            MarketCross(kit, new Vector3(gs.x, 0.06f, gs.y));
            Guardhouse(kit, new Vector3(73.5f, 0f, -63.2f));
            // Mill Square: a well and benches; Chapel Square: a fountain of thread.
            TownWell(kit, new Vector3(CitySquares[1].x - 3.6f, 0.06f, CitySquares[1].y + 3.4f));
            TownKit.Bench(kit, new Vector3(CitySquares[1].x + 4.2f, 0.06f, CitySquares[1].y - 4.4f), 180f);
            ChapelOfStStitch(kit);
            Keep(kit);

            for (int i = first; i < kit.Root.childCount; i++) cityThings.Add(kit.Root.GetChild(i));
        }

        static void AddStreetFootprint(Vector2[] path, float width)
        {
            for (int i = 0; i < path.Length - 1; i++)
            {
                Vector2 a = path[i], b = path[i + 1];
                int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(a, b) / 2f));
                for (int k = 0; k <= steps; k++)
                {
                    Vector2 p = Vector2.Lerp(a, b, k / (float)steps);
                    cityFootprints.Add(new Bounds(new Vector3(p.x, 0.5f, p.y), new Vector3(width - 0.8f, 1f, width - 0.8f)));
                    ClearGrass(new Vector3(p.x, 0f, p.y), width * 0.5f);
                }
            }
        }

        static bool NearSquare(Vector3 p, float pad)
        {
            foreach (var sq in CitySquares)
                if (new Vector2(p.x - sq.x, p.z - sq.y).magnitude < sq.z + pad) return true;
            return false;
        }

        // A stone market cross on steps, with a lantern on top.
        static void MarketCross(LevelKit kit, Vector3 feet)
        {
            var stone = PatchworkAssets.Cobble(new Color(0.85f, 0.82f, 0.76f));
            var g = kit.Group("MarketCross", feet);
            for (int s = 0; s < 3; s++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.15f + s * 0.3f, 0f), new Vector3(3.2f - s * 0.8f, 0.15f, 3.2f - s * 0.8f), stone, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 2.4f, 0f), new Vector3(0.5f, 3.2f, 0.5f), stone, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 3.4f, 0f), new Vector3(1.6f, 0.4f, 0.4f), stone, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 4.3f, 0f), Vector3.one * 0.6f, PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f), default(Vector3), g);
            TownKit.Solid(kit, feet + Vector3.up * 1.6f, new Vector3(1.4f, 3.2f, 1.4f), 0f);
        }

        // The guardhouse inside the Toybox Gate: a squat stone tower-house with battlements, a
        // striped sentry box and a flag.
        static void Guardhouse(LevelKit kit, Vector3 feet)
        {
            var stone = PatchworkAssets.Cobble(new Color(0.82f, 0.78f, 0.7f));
            var body = kit.Block(feet + Vector3.up * 3.6f, new Vector3(6.4f, 7.2f, 5f), new Color(0.82f, 0.78f, 0.7f), "Guardhouse", stone);
            var g = kit.Group("GuardhouseTrim", feet);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 7.25f, 0f), new Vector3(6.8f, 0.2f, 5.4f), wallTrim, default(Vector3), g);
            for (int i = 0; i < 5; i++)
                for (int s = -1; s <= 1; s += 2)
                {
                    kit.Deco(PrimitiveType.Cube, new Vector3(-2.8f + i * 1.4f, 7.75f, s * 2.4f), new Vector3(0.7f, 0.8f, 0.4f), i % 2 == 0 ? capRed : capBlue, default(Vector3), g);
                    if (i < 4) kit.Deco(PrimitiveType.Cube, new Vector3(s * 3f, 7.75f, -1.6f + i * 1.1f), new Vector3(0.4f, 0.8f, 0.6f), i % 2 == 0 ? capBlue : capRed, default(Vector3), g);
                }
            kit.Deco(PrimitiveType.Cube, new Vector3(-3.21f, 1.3f, 0f), new Vector3(0.06f, 2.6f, 1.4f), PatchworkAssets.Planks(new Color(0.5f, 0.32f, 0.24f)), default(Vector3), g);
            for (int k = 0; k < 2; k++)
                kit.Deco(PrimitiveType.Cube, new Vector3(-3.21f, 4.6f, -1.4f + k * 2.8f), new Vector3(0.05f, 1f, 0.5f), slit, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(2f, 9.4f, 1.2f), new Vector3(0.08f, 1.6f, 0.08f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), default(Vector3), g);
            var flag = kit.Group("Pennant", feet + new Vector3(2f, 10.6f, 1.2f));
            flag.SetParent(g, true);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.7f, 0f, 0f), new Vector3(1.4f, 0.8f, 0.04f), PatchworkAssets.Felt(new Color(0.82f, 0.3f, 0.3f)), default(Vector3), flag);
            flag.gameObject.AddComponent<FlagWave>();
            // The sentry box by the door.
            var box = kit.Group("SentryBox", feet + new Vector3(-4.4f, 0f, 3.2f));
            for (int i = 0; i < 4; i++)
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.2f, -0.6f + i * 0.4f), new Vector3(1.3f, 2.4f, 0.4f), PatchworkAssets.Felt(i % 2 == 0 ? new Color(0.82f, 0.3f, 0.3f) : new Color(0.98f, 0.94f, 0.86f)), default(Vector3), box);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 2.5f, 0f), new Vector3(1.5f, 0.2f, 1.8f), PatchworkAssets.Felt(new Color(0.3f, 0.45f, 0.72f)), default(Vector3), box);
            TownKit.Solid(kit, feet + new Vector3(-4.4f, 1.2f, 3.2f), new Vector3(1.3f, 2.4f, 1.6f), 0f);
            kit.Info.MapPins.Add(new MapPin("Guardhouse", feet));
            cityFootprints.Add(new Bounds(feet + Vector3.up * 3.6f, new Vector3(7f, 7.2f, 5.6f)));
        }

        // St. Stitch's: the town chapel, its long side on Chapel Square, with buttresses, tall
        // windows of coloured glass, a rose window and a bell tower with a slate spire, the
        // second-tallest thing in town after the keep.
        static void ChapelOfStStitch(LevelKit kit)
        {
            var stone = PatchworkAssets.Cobble(new Color(0.9f, 0.88f, 0.84f));
            var dark = PatchworkAssets.Cobble(new Color(0.7f, 0.68f, 0.64f));
            var slate = PatchworkAssets.Shingle(new Color(0.38f, 0.42f, 0.5f));
            float x0 = Chapel.x, z0 = Chapel.y, x1 = Chapel.z, z1 = Chapel.w;
            float cx = (x0 + x1) * 0.5f, w = x1 - x0;
            const float towerSize = 7f, naveH = 11f, towerH = 24f;
            float naveZ0 = z0 + towerSize, naveZc = (naveZ0 + z1) * 0.5f, naveLen = z1 - naveZ0;
            kit.Block(new Vector3(cx, naveH * 0.5f, naveZc), new Vector3(w, naveH, naveLen), new Color(0.9f, 0.88f, 0.84f), "ChapelNave", stone);
            TownKit.GableRoof(kit, new Vector3(cx, naveH, naveZc), naveLen + 0.8f, w + 1f, 5.2f, 90f, slate, stone);
            var g = kit.Group("StStitchs", Vector3.zero);
            // Buttresses and tall coloured windows down both long sides.
            Color[] glass = { new Color(0.9f, 0.3f, 0.35f), new Color(0.3f, 0.5f, 0.9f), new Color(0.95f, 0.78f, 0.3f), new Color(0.45f, 0.75f, 0.45f) };
            int bays = Mathf.FloorToInt(naveLen / 3.6f);
            for (int b = 0; b <= bays; b++)
            {
                float z = naveZ0 + 0.4f + b * (naveLen - 0.8f) / bays;
                for (int s = -1; s <= 1; s += 2)
                {
                    kit.Deco(PrimitiveType.Cube, new Vector3(cx + s * (w * 0.5f + 0.4f), naveH * 0.42f, z), new Vector3(0.8f, naveH * 0.84f, 0.9f), dark, default(Vector3), g);
                    if (b < bays)
                    {
                        float zm = z + (naveLen - 0.8f) / bays * 0.5f;
                        kit.Deco(PrimitiveType.Cube, new Vector3(cx + s * (w * 0.5f + 0.02f), naveH * 0.55f, zm), new Vector3(0.06f, 4.6f, 1.3f), PatchworkAssets.Glow(glass[b % glass.Length], 0.45f), default(Vector3), g);
                        kit.Deco(PrimitiveType.Sphere, new Vector3(cx + s * (w * 0.5f + 0.02f), naveH * 0.55f + 2.3f, zm), new Vector3(0.06f, 1.3f, 1.3f), PatchworkAssets.Glow(glass[(b + 1) % glass.Length], 0.45f), default(Vector3), g);
                    }
                }
            }
            // The rose window in the north gable, and the door on the square (west) side.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(cx, naveH + 1.6f, z1 + 0.42f), new Vector3(3.2f, 0.05f, 3.2f), PatchworkAssets.Glow(new Color(0.85f, 0.4f, 0.6f), 0.5f), new Vector3(90f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(cx, naveH + 1.6f, z1 + 0.46f), new Vector3(1.2f, 0.05f, 1.2f), PatchworkAssets.Glow(new Color(0.95f, 0.8f, 0.35f), 0.6f), new Vector3(90f, 0f, 0f), g);
            float doorZ = (naveZ0 + z1) * 0.5f;
            kit.Deco(PrimitiveType.Cube, new Vector3(x0 - 0.05f, 1.6f, doorZ), new Vector3(0.12f, 3.2f, 2.2f), PatchworkAssets.Planks(new Color(0.5f, 0.32f, 0.24f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(x0 - 0.06f, 3.2f, doorZ), new Vector3(2.2f, 0.06f, 2.2f), PatchworkAssets.Planks(new Color(0.5f, 0.32f, 0.24f)), new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(x0 - 0.3f, 3.6f, doorZ), new Vector3(0.5f, 0.5f, 3.4f), dark, default(Vector3), g);
            TownKit.Stairs(kit, new Vector3(x0 - 1.6f, 0f, doorZ), 90f, 2, 0.15f, 0.7f, 3.6f, dark);
            // The bell tower at the south end: a square tower, a belfry with open arches and a
            // tall slate spire with a gold ball and a weathercock.
            float tz = z0 + towerSize * 0.5f;
            kit.Block(new Vector3(cx, towerH * 0.5f, tz), new Vector3(towerSize, towerH, towerSize), new Color(0.9f, 0.88f, 0.84f), "ChapelTower", stone);
            for (int s = -1; s <= 1; s += 2)
                for (int t = -1; t <= 1; t += 2)
                    kit.Deco(PrimitiveType.Cube, new Vector3(cx + s * towerSize * 0.5f, towerH * 0.5f, tz + t * towerSize * 0.5f), new Vector3(1f, towerH, 1f), dark, default(Vector3), g);
            for (int f = 0; f < 4; f++)
            {
                float a = f * 90f;
                var face = Quaternion.Euler(0f, a, 0f);
                Vector3 c = new Vector3(cx, 0f, tz) + face * new Vector3(0f, 0f, towerSize * 0.5f + 0.03f);
                for (int k = -1; k <= 1; k += 2)
                {
                    kit.Deco(PrimitiveType.Cube, c + Vector3.up * (towerH - 3.2f) + face * new Vector3(k * 1.3f, 0f, 0f), new Vector3(1.1f, 3f, 0.06f), slit, new Vector3(0f, a, 0f), g);
                    kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * (towerH - 1.7f) + face * new Vector3(k * 1.3f, 0f, 0f), new Vector3(1.1f, 0.03f, 1.1f), slit, new Vector3(90f, a, 0f), g);
                }
                // The clock face below the belfry.
                kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * (towerH - 7.4f), new Vector3(2.4f, 0.04f, 2.4f), PatchworkAssets.Felt(LevelKit.Palette.Cream), new Vector3(90f, a, 0f), g);
                kit.Deco(PrimitiveType.Cube, c + Vector3.up * (towerH - 7f) + face * new Vector3(0f, 0f, 0.05f), new Vector3(0.12f, 0.9f, 0.03f), PatchworkAssets.Felt(UIKit.Ink), new Vector3(0f, a, 0f), g);
            }
            kit.Deco(PrimitiveType.Cube, new Vector3(cx, towerH + 0.1f, tz), new Vector3(towerSize + 0.8f, 0.3f, towerSize + 0.8f), wallTrim, default(Vector3), g);
            TowerCone(kit, g, new Vector3(cx, towerH + 0.2f, tz), towerSize * 0.62f, 13f, slate);
            kit.Deco(PrimitiveType.Sphere, new Vector3(cx, towerH + 13.3f, tz), Vector3.one * 0.7f, gold, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(cx, towerH + 14.4f, tz), new Vector3(0.08f, 0.9f, 0.08f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(cx + 0.3f, towerH + 15.2f, tz), new Vector3(1.1f, 0.5f, 0.06f), gold, default(Vector3), g);
            cityFootprints.Add(new Bounds(new Vector3(cx, 6f, (z0 + z1) * 0.5f), new Vector3(w + 2f, 12f, z1 - z0 + 1f)));
            kit.Info.MapPins.Add(new MapPin("St. Stitch's", new Vector3(x0 - 2f, 0f, doorZ)));
        }

        // The keep: a great square tower with round turrets at its corners, a tall slate-and-red
        // roof with a flag on top, banners down its walls, in a walled bailey with its own gate.
        // It's the tallest thing in Toybox Town, and the first thing you see from the road in.
        static void Keep(LevelKit kit)
        {
            var root = kit.Group("TheKeep", Vector3.zero);
            float x0 = Bailey.x, z0 = Bailey.y, x1 = Bailey.z, z1 = Bailey.w;
            // The bailey wall, with towers at the corners and its gate facing Castle Way.
            float gz = (z0 + z1) * 0.5f;
            WallRun(kit, root, new Vector3(x0, 0f, z0), new Vector3(x1, 0f, z0));
            WallRun(kit, root, new Vector3(x0, 0f, z1), new Vector3(x1, 0f, z1));
            WallRun(kit, root, new Vector3(x0, 0f, z0), new Vector3(x0, 0f, z1));
            WallRun(kit, root, new Vector3(x1, 0f, z0), new Vector3(x1, 0f, gz - 4.6f));
            WallRun(kit, root, new Vector3(x1, 0f, gz + 4.6f), new Vector3(x1, 0f, z1));
            WallTower(kit, root, new Vector3(x0, 0f, z0), 2.2f, 7.6f, true);
            WallTower(kit, root, new Vector3(x1, 0f, z0), 2.2f, 7.6f, false);
            WallTower(kit, root, new Vector3(x0, 0f, z1), 2.2f, 7.6f, false);
            WallTower(kit, root, new Vector3(x1, 0f, z1), 2.2f, 7.6f, true);
            Gatehouse(kit, root, new Vector3(x1, 0f, gz), false, 9.2f, "Keep Gate");
            var yard = kit.Deco(PrimitiveType.Cube, new Vector3((x0 + x1) * 0.5f, 0.03f, gz), new Vector3(x1 - x0 - 1f, 0.04f, z1 - z0 - 1f), PatchworkAssets.Cobble(new Color(0.78f, 0.74f, 0.68f)), default(Vector3), root);
            yard.name = "BaileyYard";
            ClearGrass(new Vector3((x0 + x1) * 0.5f, 0f, gz), (z1 - z0) * 0.45f);

            // The keep.
            var stone = PatchworkAssets.Cobble(new Color(0.9f, 0.85f, 0.75f));
            const float size = 12f, h = 24f, turretR = 2.3f, turretH = 29f;
            Vector3 k = KeepAt;
            kit.Block(k + Vector3.up * h * 0.5f, new Vector3(size, h, size), new Color(0.9f, 0.85f, 0.75f), "Keep", stone);
            var g = kit.Group("KeepDressing", k);
            g.SetParent(root, true);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.5f, 0f), new Vector3(size + 0.6f, 1f, size + 0.6f), wallPlinth, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, h * 0.5f, 0f), new Vector3(size + 0.12f, 0.2f, size + 0.12f), wallPlinth, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, h, 0f), new Vector3(size + 0.8f, 0.3f, size + 0.8f), wallTrim, default(Vector3), g);
            // Battlements round the top.
            for (int side = 0; side < 4; side++)
            {
                var face = Quaternion.Euler(0f, side * 90f, 0f);
                for (int i = 0; i < 7; i++)
                {
                    Vector3 p = face * new Vector3(-size * 0.5f + 1.2f + i * (size - 2.4f) / 6f, h + 0.6f, size * 0.5f + 0.1f);
                    kit.Deco(PrimitiveType.Cube, p, new Vector3(0.9f, 1f, 0.5f), i % 2 == 0 ? capRed : capBlue, new Vector3(0f, side * 90f, 0f), g);
                }
                // Tall arched windows, lit warm, and long banners between them.
                for (int fl = 0; fl < 3; fl++)
                    for (int c = -1; c <= 1; c += 2)
                    {
                        Vector3 wp = face * new Vector3(c * 2.6f, 5f + fl * 6.4f, size * 0.5f + 0.03f);
                        kit.Deco(PrimitiveType.Cube, wp, new Vector3(1.1f, 2.2f, 0.06f), PatchworkAssets.Glow(new Color(1f, 0.82f, 0.5f), 0.45f), new Vector3(0f, side * 90f, 0f), g);
                        kit.Deco(PrimitiveType.Cylinder, wp + Vector3.up * 1.1f, new Vector3(1.1f, 0.03f, 1.1f), PatchworkAssets.Glow(new Color(1f, 0.82f, 0.5f), 0.45f), new Vector3(90f, side * 90f, 0f), g);
                    }
                var banner = kit.Deco(PrimitiveType.Cube, face * new Vector3(0f, h - 6.5f, size * 0.5f + 0.08f), new Vector3(2f, 12f, 0.06f), PatchworkAssets.Felt(side % 2 == 0 ? new Color(0.82f, 0.3f, 0.3f) : new Color(0.3f, 0.45f, 0.72f)), new Vector3(0f, side * 90f, 0f), g);
                banner.name = "KeepBanner";
                kit.Deco(PrimitiveType.Cube, face * new Vector3(0f, h - 9f, size * 0.5f + 0.12f), new Vector3(1.4f, 1.4f, 0.04f), gold, new Vector3(0f, side * 90f, 45f), g);
            }
            // The corner turrets, taller than the keep, with pointed roofs and pennants.
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    WallTower(kit, g, k + new Vector3(sx * size * 0.5f, 0f, sz * size * 0.5f), turretR, turretH - TowerExtra, sx == sz);
            // The roof: a tall pyramid in slate with a red-roofed lantern on top, and the flag.
            var slate = PatchworkAssets.Shingle(new Color(0.38f, 0.42f, 0.5f));
            TowerCone(kit, g, new Vector3(0f, h + 0.3f, 0f), size * 0.5f, 9f, slate);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, h + 9.6f, 0f), new Vector3(2.4f, 1.2f, 2.4f), stone, default(Vector3), g);
            TowerCone(kit, g, new Vector3(0f, h + 10.8f, 0f), 1.6f, 3.6f, roofRed);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, h + 14.5f, 0f), Vector3.one * 0.5f, gold, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, h + 15.6f, 0f), new Vector3(0.08f, 1.2f, 0.08f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), default(Vector3), g);
            var flag = kit.Group("KeepFlag", k + new Vector3(0f, h + 16.4f, 0f));
            flag.SetParent(g, true);
            kit.Deco(PrimitiveType.Cube, new Vector3(1.1f, 0f, 0f), new Vector3(2.2f, 1.2f, 0.05f), PatchworkAssets.Felt(new Color(0.82f, 0.3f, 0.3f)), default(Vector3), flag);
            kit.Deco(PrimitiveType.Cube, new Vector3(1.6f, 0f, -0.03f), new Vector3(0.6f, 0.6f, 0.04f), gold, new Vector3(0f, 0f, 45f), flag);
            flag.gameObject.AddComponent<FlagWave>();
            // The great door, up a flight of steps, facing the gate.
            kit.Deco(PrimitiveType.Cube, new Vector3(size * 0.5f + 0.06f, 2.4f, 0f), new Vector3(0.12f, 3.6f, 2.6f), PatchworkAssets.Planks(new Color(0.5f, 0.32f, 0.24f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(size * 0.5f + 0.07f, 4.2f, 0f), new Vector3(2.6f, 0.06f, 2.6f), PatchworkAssets.Planks(new Color(0.5f, 0.32f, 0.24f)), new Vector3(0f, 0f, 90f), g);
            TownKit.Stairs(kit, k + new Vector3(size * 0.5f + 3.2f, 0f, 0f), -90f, 3, 0.25f, 0.8f, 3.6f, PatchworkAssets.Cobble(new Color(0.75f, 0.72f, 0.68f)));
            // Life in the bailey: a well, barrels and a straw practice target.
            TownWell(kit, new Vector3(x0 + 5f, 0.05f, z1 - 4.5f));
            for (int i = 0; i < 3; i++) TownKit.Crate(kit, new Vector3(x0 + 3f + i * 1.2f, 0.05f, z0 + 2.6f), 1f, i * 20f);
            var target = kit.Group("PracticeTarget", new Vector3(x1 - 5f, 0f, z0 + 4f));
            target.SetParent(root, true);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.3f, 0f), new Vector3(1.8f, 0.2f, 1.8f), PatchworkAssets.Felt(new Color(0.95f, 0.85f, 0.5f)), new Vector3(90f, 0f, 0f), target);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.3f, -0.18f), new Vector3(1.1f, 0.05f, 1.1f), PatchworkAssets.Felt(new Color(0.82f, 0.3f, 0.3f)), new Vector3(90f, 0f, 0f), target);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.3f, -0.22f), new Vector3(0.4f, 0.05f, 0.4f), gold, new Vector3(90f, 0f, 0f), target);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.5f, 0.6f, 0.2f), new Vector3(0.12f, 1.4f, 0.12f), PatchworkAssets.Wood(TownKit.DarkWood), new Vector3(-15f, 0f, s * 10f), target);
            cityFootprints.Add(new Bounds(new Vector3((x0 + x1) * 0.5f, 4f, gz), new Vector3(x1 - x0, 8f, z1 - z0)));
            kit.Info.MapPins.Add(new MapPin("The Keep", k + new Vector3(size * 0.5f + 4f, 0f, 0f)));
        }

        // ---------------------------------------------------------------- Tidying up

        // Once the whole level is built: anything small and purely decorative that ended up
        // inside a house, a landmark or a street of the new quarter is taken away (with its
        // invisible bump box). Anything that does something (a collectible, a quest thing)
        // isn't touched; it's logged instead, so it can be moved by hand.
        static void CityClearance(LevelKit kit)
        {
            Physics.SyncTransforms();
            var root = kit.Root;
            var solids = new List<Transform>();
            for (int i = 0; i < root.childCount; i++)
            {
                var t = root.GetChild(i);
                if (t.name == "Solid" && !cityThings.Contains(t) && t.GetComponent<Collider>() != null && t.GetComponent<Renderer>() == null) solids.Add(t);
            }
            int removed = 0;
            for (int i = 0; i < root.childCount; i++)
            {
                var t = root.GetChild(i);
                if (!t.gameObject.activeSelf || cityThings.Contains(t) || t.name == "Solid") continue;
                if (t.GetComponent<MeshFilter>() != null && t.GetComponent<MeshCollider>() != null) continue;   // ground meshes
                Bounds b;
                if (!RendererBounds(t, out b)) continue;
                if (Mathf.Max(b.size.x, b.size.z) > 12f || b.min.y > 3f) continue;   // big things, and things up in the air
                var foot = new Bounds(new Vector3(b.center.x, 0.5f, b.center.z), new Vector3(Mathf.Max(0.2f, b.size.x * 0.7f), 1f, Mathf.Max(0.2f, b.size.z * 0.7f)));
                bool hit = false;
                foreach (var f in cityFootprints)
                {
                    var flat = new Bounds(new Vector3(f.center.x, 0.5f, f.center.z), new Vector3(f.size.x, 1f, f.size.z));
                    if (flat.Intersects(foot)) { hit = true; break; }
                }
                if (!hit && OuterWallDistance(b.center) > 1.4f) continue;
                if (!Plain(t))
                {
                    Debug.LogWarning("[Level1] '" + t.name + "' at " + b.center.ToString("F1") + " is in the way of the new town; move it by hand.");
                    continue;
                }
                t.gameObject.SetActive(false);
                foreach (var s in solids)
                    if (s.gameObject.activeSelf && b.Contains(new Vector3(s.position.x, b.center.y, s.position.z))) s.gameObject.SetActive(false);
                removed++;
            }
            // Invisible bump boxes left standing in a street.
            foreach (var s in solids)
            {
                if (!s.gameObject.activeSelf) continue;
                var p = new Vector3(s.position.x, 0.5f, s.position.z);
                foreach (var f in cityFootprints)
                    if (new Bounds(new Vector3(f.center.x, 0.5f, f.center.z), new Vector3(f.size.x, 1f, f.size.z)).Contains(p)) { s.gameObject.SetActive(false); removed++; break; }
            }
            if (removed > 0) Debug.Log("[Level1] Cleared " + removed + " small things out of the new town's houses and streets.");
        }

        static bool RendererBounds(Transform t, out Bounds b)
        {
            b = new Bounds();
            bool any = false;
            foreach (var r in t.GetComponentsInChildren<Renderer>())
            {
                if (!r.enabled) continue;
                if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
            }
            return any;
        }

        // Nothing but looks (and the little spinners and bobbers that only move it about).
        static bool Plain(Transform t)
        {
            foreach (var mb in t.GetComponentsInChildren<MonoBehaviour>(true))
                if (!(mb is Spinner) && !(mb is Rocker) && !(mb is Blinker) && !(mb is FlagWave) && !(mb is OwnedMesh)) return false;
            return true;
        }
    }
}

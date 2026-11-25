using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The town wall: Toybox Town as a walled medieval town, like a toy fort out of a toybox.
    // Cream stone blocks with painted battlements, round towers with pointed roofs and pennants,
    // and arched gatehouses. The old town's wall still runs:
    //   - west, along the top of the ravine (the ravine is the moat), open in the middle where
    //     the west street, the swing and the steps down are, and now on north past the
    //     Tournament Green to the keep, with posterns at the tape-measure bridge and the button
    //     stones
    //   - north, past the back of the barracks: now an inner wall, with the Castle Gate between
    //     the castle yard and the Tournament Green
    //   - south, behind Chimney Lane, with the old spire as one of its towers, the South Gate
    //     and a postern
    // and the new outer wall (taller, thicker) takes in the Common Quarter across the canal and
    // the keep in the north: over the canal on two water gates, round the east side with the
    // Toybox Gate (the way in from the Pod), the East Gate and a postern to the farm, and back
    // round the keep with the North Gate.
    // Outside it are the outskirts: the fields and farm, the woods, Spindle Hill and the meadows.
    public static partial class Level1Builder
    {
        public const float WallWest = -36.55f, WallEast = 31.55f, WallNorth = 58.5f, WallSouth = -72f;
        const float WallHeight = 5f, WallThick = 1f;
        const float OuterHeight = 6.4f, OuterThick = 1.6f;
        const float TowerExtra = 2f;   // the old towers, raised to stand over the taller walls

        static Material wallStone, wallPlinth, wallTrim, capRed, capBlue, roofRed, roofBlue, gold, slit;

        static void TownWall(LevelKit kit)
        {
            wallStone = PatchworkAssets.Cobble(new Color(0.9f, 0.85f, 0.75f));
            wallPlinth = PatchworkAssets.Cobble(new Color(0.72f, 0.68f, 0.62f));
            wallTrim = PatchworkAssets.Wood(new Color(0.85f, 0.7f, 0.42f));
            capRed = PatchworkAssets.Wood(new Color(0.82f, 0.3f, 0.3f));
            capBlue = PatchworkAssets.Wood(new Color(0.3f, 0.45f, 0.72f));
            roofRed = PatchworkAssets.Felt(new Color(0.8f, 0.3f, 0.32f));
            roofBlue = PatchworkAssets.Felt(new Color(0.3f, 0.44f, 0.72f));
            gold = PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f);
            slit = PatchworkAssets.Plain(new Color(0.18f, 0.14f, 0.2f), 0.2f);
            var root = kit.Group("TownWall", Vector3.zero);

            float W = WallWest, E = WallEast, N = WallNorth, S = WallSouth;

            // West, above the ravine. (Towers on the west and east walls stand just inside the
            // edge, so none of them hangs out over the ravine or the canal.)
            WallRun(kit, root, new Vector3(W, 0f, N), new Vector3(W, 0f, 18.5f));
            WallRun(kit, root, new Vector3(W, 0f, -22.5f), new Vector3(W, 0f, S));
            WallTower(kit, root, new Vector3(W, 0f, N), 2.2f, 7.4f, true);
            WallTower(kit, root, new Vector3(W, 0f, 38f), 1.6f, 5.8f, false);
            WallTower(kit, root, new Vector3(W, 0f, 18.5f), 2f, 6.8f, true);
            WallTower(kit, root, new Vector3(W, 0f, -22.5f), 2f, 6.8f, false);
            WallTower(kit, root, new Vector3(W, 0f, -36f), 1.6f, 5.8f, true);
            WallTower(kit, root, new Vector3(W, 0f, S), 2.2f, 7.4f, false);
            // ...and on north along the ravine to the keep, open at the two ravine crossings.
            float tapeA = TapeZ - 2.6f, tapeB = TapeZ + 2.6f, stonesA = StonesZ - 2.6f, stonesB = StonesZ + 2.6f;
            WallRun(kit, root, new Vector3(W, 0f, N), new Vector3(W, 0f, tapeA));
            WallRun(kit, root, new Vector3(W, 0f, tapeB), new Vector3(W, 0f, stonesA));
            WallRun(kit, root, new Vector3(W, 0f, stonesB), new Vector3(W, 0f, OuterWall[OuterWall.Length - 1].y));
            WallTower(kit, root, new Vector3(W, 0f, tapeA), 1.4f, 5.6f, false);
            WallTower(kit, root, new Vector3(W, 0f, tapeB), 1.4f, 5.6f, true);
            WallTower(kit, root, new Vector3(W, 0f, stonesA), 1.4f, 5.6f, false);
            WallTower(kit, root, new Vector3(W, 0f, stonesB), 1.4f, 5.6f, true);

            // (The old east wall along the canal is gone: the canal runs through the middle of
            // town now. Its south-east corner tower stays, where the outer wall starts.)
            WallTower(kit, root, new Vector3(E, 0f, S), 2.2f, 7.4f, true);

            // North, behind the barracks: the Castle Gate in the middle, a postern by the
            // carousel, and the north-east corner open for the xylophone bridge.
            WallRun(kit, root, new Vector3(-27f, 0f, N), new Vector3(-4.6f, 0f, N));
            WallRun(kit, root, new Vector3(4.6f, 0f, N), new Vector3(24f, 0f, N));
            WallTower(kit, root, new Vector3(-27f, 0f, N), 1.2f, 5.2f, true);
            WallTower(kit, root, new Vector3(-15.5f, 0f, N), 1.6f, 5.8f, false);
            WallTower(kit, root, new Vector3(15.5f, 0f, N), 1.6f, 5.8f, true);
            WallTower(kit, root, new Vector3(24f, 0f, N), 1.8f, 6.4f, false);
            Gatehouse(kit, root, new Vector3(0f, 0f, N), true, 9.2f, "Castle Gate");

            // South, behind Chimney Lane: the old spire stands in the wall as one of its towers,
            // with the South Gate east of it and a postern to the west.
            WallRun(kit, root, new Vector3(W, 0f, S), new Vector3(-25.5f, 0f, S));
            WallRun(kit, root, new Vector3(-18.5f, 0f, S), new Vector3(-3f, 0f, S));
            WallRun(kit, root, new Vector3(3f, 0f, S), new Vector3(5.4f, 0f, S));
            WallRun(kit, root, new Vector3(14.6f, 0f, S), new Vector3(E, 0f, S));
            WallTower(kit, root, new Vector3(-25.5f, 0f, S), 1.2f, 5.2f, true);
            WallTower(kit, root, new Vector3(-18.5f, 0f, S), 1.2f, 5.2f, false);
            WallTower(kit, root, new Vector3(22f, 0f, S), 1.6f, 5.8f, true);
            Gatehouse(kit, root, new Vector3(10f, 0f, S), true, 9.2f, "South Gate");

            // The guide's map and the Storyteller know where the gates are.
            var pins = kit.Info.MapPins;
            pins.Add(new MapPin("Castle Gate", new Vector3(0f, 0f, N), MapPin.Kind.Area));
            pins.Add(new MapPin("South Gate", new Vector3(10f, 0f, S), MapPin.Kind.Area));
            OuterTownWall(kit, root);
        }

        // A straight length of wall between two points on the ground (along x or along z).
        static void WallRun(LevelKit kit, Transform root, Vector3 a, Vector3 b)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            if (len < 0.5f) return;
            bool alongX = Mathf.Abs(d.x) > Mathf.Abs(d.z);
            Vector3 mid = (a + b) * 0.5f;
            Vector3 size = alongX ? new Vector3(len, WallHeight, WallThick) : new Vector3(WallThick, WallHeight, len);
            var body = kit.Block(mid + Vector3.up * WallHeight * 0.5f, size, new Color(0.9f, 0.85f, 0.75f), "TownWallRun", wallStone);
            body.SetParent(root, true);
            // A darker plinth along the bottom, and a painted wooden trim along the top.
            Vector3 plinth = alongX ? new Vector3(len, 0.6f, WallThick + 0.2f) : new Vector3(WallThick + 0.2f, 0.6f, len);
            kit.Deco(PrimitiveType.Cube, mid + Vector3.up * 0.3f, plinth, wallPlinth, default(Vector3), root);
            Vector3 trim = alongX ? new Vector3(len, 0.18f, WallThick + 0.16f) : new Vector3(WallThick + 0.16f, 0.18f, len);
            kit.Deco(PrimitiveType.Cube, mid + Vector3.up * (WallHeight - 0.05f), trim, wallTrim, default(Vector3), root);
            // The battlements: painted blocks, red and blue in turn, with one flat collider over
            // them so the walkway along the top is smooth.
            var red = new MeshBatch();
            var blue = new MeshBatch();
            Vector3 dir = d / len;
            int count = Mathf.Max(1, Mathf.FloorToInt((len - 0.4f) / 1.5f));
            float step = len / count;
            for (int i = 0; i < count; i++)
            {
                Vector3 c = a + dir * (step * (i + 0.5f)) + Vector3.up * (WallHeight + 0.38f);
                Vector3 s = alongX ? new Vector3(0.8f, 0.7f, WallThick + 0.04f) : new Vector3(WallThick + 0.04f, 0.7f, 0.8f);
                (i % 2 == 0 ? red : blue).Box(c, s, Quaternion.identity);
            }
            red.Build(root, "Battlements", capRed, true);
            blue.Build(root, "Battlements", capBlue, true);
            TownKit.Solid(kit, mid + Vector3.up * (WallHeight + 0.38f), alongX ? new Vector3(len, 0.7f, WallThick) : new Vector3(WallThick, 0.7f, len), 0f);
        }

        // A round tower with arrow slits, a crenellated top, a pointed roof and a pennant.
        static void WallTower(LevelKit kit, Transform root, Vector3 feet, float r, float h, bool red)
        {
            // (On the west and east walls, just inside the edge of the ravine or the canal.)
            if (Mathf.Abs(feet.x - WallWest) < 0.01f) feet.x = RavineEast + r - 0.05f;
            else if (Mathf.Abs(feet.x - WallEast) < 0.01f) feet.x = CanalWest - r + 0.05f;
            h += TowerExtra;
            var g = kit.Group("WallTower", feet);
            g.SetParent(root, true);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, h * 0.5f, 0f), new Vector3(r * 2f, h * 0.5f, r * 2f), wallStone, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.35f, 0f), new Vector3(r * 2f + 0.25f, 0.35f, r * 2f + 0.25f), wallPlinth, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, h, 0f), new Vector3(r * 2f + 0.5f, 0.12f, r * 2f + 0.5f), wallTrim, default(Vector3), g);
            var col = g.gameObject.AddComponent<CapsuleCollider>();
            col.radius = r + 0.2f;
            col.height = h + 2f * (r + 0.2f);
            col.center = new Vector3(0f, h * 0.5f, 0f);
            // Crenellations round the top.
            int teeth = Mathf.Max(6, Mathf.RoundToInt(r * 4f));
            for (int i = 0; i < teeth; i++)
            {
                float a = i * Mathf.PI * 2f / teeth;
                kit.Deco(PrimitiveType.Cube, new Vector3(Mathf.Cos(a) * (r + 0.05f), h + 0.45f, Mathf.Sin(a) * (r + 0.05f)), new Vector3(0.55f, 0.7f, 0.45f),
                    i % 2 == 0 ? capRed : capBlue, new Vector3(0f, -a * Mathf.Rad2Deg + 90f, 0f), g);
            }
            // Arrow slits.
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f + 0.4f;
                Vector3 p = new Vector3(Mathf.Cos(a) * (r + 0.01f), h * 0.62f, Mathf.Sin(a) * (r + 0.01f));
                kit.Deco(PrimitiveType.Cube, p, new Vector3(0.14f, 0.9f, 0.05f), slit, new Vector3(0f, -a * Mathf.Rad2Deg + 90f, 0f), g);
            }
            // The roof: a cone, a gold ball and a pennant on a pole.
            TowerCone(kit, g, new Vector3(0f, h + 0.75f, 0f), r + 0.45f, r * 1.9f, red ? roofRed : roofBlue);
            float top = h + 0.75f + r * 1.9f;
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, top, 0f), Vector3.one * 0.32f, gold, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, top + 0.7f, 0f), new Vector3(0.06f, 0.7f, 0.06f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), default(Vector3), g);
            var flag = kit.Group("Pennant", feet + new Vector3(0f, top + 1.15f, 0f));
            flag.SetParent(g, true);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.55f, 0.08f, 0f), new Vector3(1.1f, 0.42f, 0.04f), PatchworkAssets.Felt(red ? new Color(0.3f, 0.45f, 0.72f) : new Color(0.82f, 0.3f, 0.3f)), default(Vector3), flag);
            kit.Deco(PrimitiveType.Cube, new Vector3(1.2f, 0.08f, 0f), new Vector3(0.3f, 0.2f, 0.04f), PatchworkAssets.Felt(LevelKit.Palette.Gold), default(Vector3), flag);
            flag.gameObject.AddComponent<FlagWave>();
        }

        // A gatehouse: two towers either side of the way through, a battlemented arch over it
        // (high enough to walk, run or glide under), the portcullis drawn up, and the town's
        // shield over the arch.
        static void Gatehouse(LevelKit kit, Transform root, Vector3 centre, bool alongX, float span, string name)
        {
            Vector3 across = alongX ? Vector3.right : Vector3.forward;
            Vector3 outward = alongX ? (centre.z > 0f ? Vector3.forward : Vector3.back) : Vector3.right;
            float half = span * 0.5f;
            WallTower(kit, root, centre - across * half, 2.1f, 7.2f, true);
            WallTower(kit, root, centre + across * half, 2.1f, 7.2f, false);
            // (The group sits at the origin, so the positions below are world positions.)
            var g = kit.Group(name.Replace(" ", ""), Vector3.zero);
            g.SetParent(root, true);
            float clear = 4.6f, beam = 1.5f, inner = span - 3.2f;
            Vector3 lintelSize = alongX ? new Vector3(inner, beam, WallThick + 0.6f) : new Vector3(WallThick + 0.6f, beam, inner);
            var lintel = kit.Block(centre + Vector3.up * (clear + beam * 0.5f), lintelSize, new Color(0.9f, 0.85f, 0.75f), "GateArch", wallStone);
            lintel.SetParent(g, true);
            // The arch's curved underside, painted, and its battlements.
            var arch = kit.Deco(PrimitiveType.Cylinder, centre + Vector3.up * clear, alongX ? new Vector3(inner, 0.5f, WallThick + 0.62f) : new Vector3(WallThick + 0.62f, 0.5f, inner), wallTrim, default(Vector3), g);
            arch.rotation = alongX ? Quaternion.Euler(90f, 0f, 0f) : Quaternion.Euler(0f, 0f, 90f);
            arch.localScale = alongX ? new Vector3(inner, (WallThick + 0.62f) * 0.5f, 0.7f) : new Vector3(0.7f, (WallThick + 0.62f) * 0.5f, inner);
            int teeth = Mathf.Max(2, Mathf.FloorToInt(inner / 1.4f));
            for (int i = 0; i < teeth; i++)
            {
                float u = -inner * 0.5f + inner * (i + 0.5f) / teeth;
                kit.Deco(PrimitiveType.Cube, centre + across * u + Vector3.up * (clear + beam + 0.38f), alongX ? new Vector3(0.8f, 0.7f, WallThick + 0.64f) : new Vector3(WallThick + 0.64f, 0.7f, 0.8f), i % 2 == 0 ? capRed : capBlue, default(Vector3), g);
            }
            // The portcullis, drawn up into the arch: just its iron teeth showing.
            var iron = PatchworkAssets.Plain(TownKit.Iron, 0.5f);
            for (float u = -inner * 0.5f + 0.5f; u < inner * 0.5f - 0.3f; u += 0.7f)
                kit.Deco(PrimitiveType.Cube, centre + across * u + Vector3.up * (clear - 0.15f), new Vector3(0.12f, 0.5f, 0.12f), iron, default(Vector3), g);
            // The town's shield on both faces: a spool on blue, with a needle across it.
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 face = centre + outward * s * (WallThick * 0.5f + 0.34f) + Vector3.up * (clear + beam * 0.5f);
                float yaw = Mathf.Atan2(outward.x * s, outward.z * s) * Mathf.Rad2Deg;
                var shield = kit.Group("TownShield", face);
                shield.SetParent(g, true);
                shield.rotation = Quaternion.Euler(0f, yaw, 0f);
                kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(1.3f, 1.2f, 0.12f), gold, default(Vector3), shield);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.02f, 0.05f), new Vector3(1.1f, 1.02f, 0.06f), PatchworkAssets.Felt(new Color(0.3f, 0.45f, 0.72f)), default(Vector3), shield);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.55f, 0.04f), new Vector3(1.1f, 0.5f, 0.12f), PatchworkAssets.Felt(new Color(0.3f, 0.45f, 0.72f)), default(Vector3), shield);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.05f, 0.1f), new Vector3(0.46f, 0.04f, 0.46f), PatchworkAssets.Felt(LevelKit.Palette.Rose), new Vector3(90f, 0f, 0f), shield);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.05f, 0.14f), new Vector3(0.08f, 0.95f, 0.03f), PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.9f), 0.8f), new Vector3(0f, 0f, 35f), shield);
            }
            // Banners hanging from the arch on the way-in side.
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 p = centre + across * s * (inner * 0.5f - 0.7f) + outward * (WallThick * 0.5f + 0.36f) + Vector3.up * (clear + 0.1f);
                float yaw = Mathf.Atan2(outward.x, outward.z) * Mathf.Rad2Deg;
                var banner = kit.Group("GateBanner", p);
                banner.SetParent(g, true);
                banner.rotation = Quaternion.Euler(0f, yaw, 0f);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, -1.1f, 0f), new Vector3(0.8f, 2.2f, 0.04f), PatchworkAssets.Felt(s < 0 ? new Color(0.82f, 0.3f, 0.3f) : new Color(0.3f, 0.45f, 0.72f)), default(Vector3), banner);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, -1.3f, -0.03f), new Vector3(0.8f, 0.22f, 0.03f), PatchworkAssets.Felt(LevelKit.Palette.Gold), default(Vector3), banner);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0f), new Vector3(0.08f, 0.5f, 0.08f), wallTrim, new Vector3(0f, 0f, 90f), banner);
            }
        }

        // The village's colours, for bunting along the streets.
        static readonly Color[] Heraldic =
        {
            new Color(0.82f, 0.3f, 0.3f), new Color(0.98f, 0.94f, 0.86f), new Color(0.3f, 0.45f, 0.72f), new Color(0.95f, 0.75f, 0.3f)
        };

        // A village well: a ring of stone, a little shingled roof on two posts, a winch and a bucket.
        static void TownWell(LevelKit kit, Vector3 feet)
        {
            var g = kit.Group("Well", feet);
            var stone = PatchworkAssets.Cobble(new Color(0.75f, 0.72f, 0.68f));
            var wood = PatchworkAssets.Wood(TownKit.DarkWood);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.45f, 0f), new Vector3(1.7f, 0.45f, 1.7f), stone, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.91f, 0f), new Vector3(1.85f, 0.04f, 1.85f), PatchworkAssets.Cobble(new Color(0.85f, 0.82f, 0.76f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.92f, 0f), new Vector3(1.3f, 0.02f, 1.3f), PatchworkAssets.Plain(new Color(0.15f, 0.2f, 0.32f), 0.3f), default(Vector3), g);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.8f, 1.5f, 0f), new Vector3(0.14f, 1.3f, 0.14f), wood, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.75f, 0f), new Vector3(0.12f, 0.8f, 0.12f), wood, new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.95f, 1.6f, 0f), new Vector3(0.06f, 0.35f, 0.06f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), default(Vector3), g);
            var roof = PatchworkAssets.Shingle(new Color(0.68f, 0.35f, 0.27f));
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 2.35f, s * 0.42f), new Vector3(2f, 0.08f, 1f), roof, new Vector3(s * 35f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.2f, 1.2f, 0f), new Vector3(0.36f, 0.2f, 0.36f), PatchworkAssets.Planks(new Color(0.6f, 0.42f, 0.28f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.2f, 1.6f, 0f), new Vector3(0.02f, 0.25f, 0.02f), PatchworkAssets.Felt(new Color(0.85f, 0.75f, 0.55f)), default(Vector3), g);
            TownKit.Solid(kit, feet + Vector3.up * 0.5f, new Vector3(1.6f, 1f, 1.6f), 0f);
            ClearGrass(feet, 1.2f);
        }

        // ---------------------------------------------------------------- The outer wall

        // The outer wall, from the old wall's south-east corner round to the ravine by the keep.
        public static readonly Vector2[] OuterWall =
        {
            new Vector2(31.55f, -72f), new Vector2(41f, -72.5f), new Vector2(50f, -71.2f), new Vector2(66f, -67f), new Vector2(77f, -59f),
            new Vector2(86f, -46f), new Vector2(92f, -26f), new Vector2(95.2f, 2f), new Vector2(96f, 32f), new Vector2(95f, 62f),
            new Vector2(90f, 86f), new Vector2(80f, 99f), new Vector2(62f, 107f), new Vector2(44f, 110f), new Vector2(30f, 112.5f),
            new Vector2(14f, 120f), new Vector2(-6f, 122f), new Vector2(-24f, 116f), new Vector2(WallWest, 104f),
        };
        public static readonly Vector3 MainGate = new Vector3(58f, 0f, -69.5f);
        static readonly Vector3 EastGateAt = new Vector3(95.36f, 0f, 8f);
        static readonly Vector3 NorthGateAt = new Vector3(8f, 0f, 120.6f);
        static readonly Vector3 FarmGateAt = new Vector3(84.8f, 0f, 92.8f);

        // Is p inside the town (the old town and the Common Quarter)?
        public static bool InTown(Vector3 p)
        {
            var poly = new List<Vector2>(OuterWall);
            poly.Add(new Vector2(WallWest, WallNorth));
            poly.Add(new Vector2(WallWest, WallSouth));
            bool inside = false;
            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                Vector2 a = poly[i], b = poly[j];
                if ((a.y > p.z) != (b.y > p.z) && p.x < (b.x - a.x) * (p.z - a.y) / (b.y - a.y) + a.x) inside = !inside;
            }
            return inside;
        }

        // How far p is from the line of the outer wall.
        public static float OuterWallDistance(Vector3 p)
        {
            var q = new Vector2(p.x, p.z);
            float best = float.MaxValue;
            for (int i = 0; i < OuterWall.Length - 1; i++)
            {
                Vector2 a = OuterWall[i], b = OuterWall[i + 1], ab = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(q - a, ab) / ab.sqrMagnitude);
                best = Mathf.Min(best, Vector2.Distance(q, a + ab * t));
            }
            return best;
        }

        static void OuterTownWall(LevelKit kit, Transform root)
        {
            // The gaps: the gates (and the farm postern), and the two places it crosses the canal.
            var gaps = new List<Vector4>();   // (x, z, half-width, kind) kind: 0 big gate, 1 gate, 2 postern, 3 water
            gaps.Add(new Vector4(MainGate.x, MainGate.z, 7.2f, 0f));
            gaps.Add(new Vector4(EastGateAt.x, EastGateAt.z, 5.6f, 1f));
            gaps.Add(new Vector4(NorthGateAt.x, NorthGateAt.z, 5.6f, 1f));
            gaps.Add(new Vector4(FarmGateAt.x, FarmGateAt.z, 2.4f, 2f));
            foreach (var w in OuterWater()) gaps.Add(w);

            for (int i = 0; i < OuterWall.Length - 1; i++)
            {
                Vector2 a = OuterWall[i], b = OuterWall[i + 1], ab = b - a;
                float len = ab.magnitude;
                Vector2 dir = ab / len;
                // The bits of this stretch that are gaps.
                var cuts = new List<Vector2>();
                foreach (var g in gaps)
                {
                    Vector2 c = new Vector2(g.x, g.y);
                    float t = Vector2.Dot(c - a, dir);
                    if (t < -0.5f || t > len + 0.5f || Vector2.Distance(a + dir * Mathf.Clamp(t, 0f, len), c) > 1.5f) continue;
                    cuts.Add(new Vector2(t - g.z, t + g.z));
                }
                cuts.Sort((x, y) => x.x.CompareTo(y.x));
                float from = 0f;
                foreach (var c in cuts)
                {
                    if (c.x > from + 0.4f) OuterRun(kit, root, a + dir * from, a + dir * Mathf.Min(c.x, len));
                    from = Mathf.Max(from, c.y);
                }
                if (len > from + 0.4f) OuterRun(kit, root, a + dir * from, b);
                // A tower at every corner (not where the wall meets the old one or the ravine;
                // the old towers are there already).
                bool byGate = false;
                foreach (var g in gaps) if (Vector2.Distance(a, new Vector2(g.x, g.y)) < g.z + 4f) byGate = true;
                if (i > 0 && !byGate) WallTower(kit, root, new Vector3(a.x, 0f, a.y), 2.5f, OuterHeight + 2.6f - TowerExtra, i % 2 == 0);
            }
            // The big corner tower where it meets the ravine, by the keep.
            WallTower(kit, root, new Vector3(WallWest, 0f, OuterWall[OuterWall.Length - 1].y), 2.4f, OuterHeight + 2.4f - TowerExtra, false);

            // The gates.
            GrandGate(kit, root, MainGate, WallYawAt(MainGate), 14.4f, 1.35f, "Toybox Gate");
            GrandGate(kit, root, EastGateAt, WallYawAt(EastGateAt), 11.2f, 1f, "East Gate");
            GrandGate(kit, root, NorthGateAt, WallYawAt(NorthGateAt), 11.2f, 1f, "North Gate");
            Postern(kit, root, FarmGateAt, WallYawAt(FarmGateAt));
            foreach (var w in OuterWater()) WaterGate(kit, root, new Vector3(w.x, 0f, w.y), WallYawAt(new Vector3(w.x, 0f, w.y)), w.z);

            var pins = kit.Info.MapPins;
            pins.Add(new MapPin("Toybox Gate", MainGate, MapPin.Kind.Area));
            pins.Add(new MapPin("East Gate", EastGateAt, MapPin.Kind.Area));
            pins.Add(new MapPin("North Gate", NorthGateAt, MapPin.Kind.Area));
            pins.Add(new MapPin("Farm Gate", FarmGateAt));
        }

        // Where the outer wall crosses the canal: the middle of the crossing, and half its width
        // (from quay to quay, plus a little).
        static List<Vector4> OuterWater()
        {
            var list = new List<Vector4>();
            float mid = (CanalWest + CanalEast) * 0.5f;
            for (int i = 0; i < OuterWall.Length - 1; i++)
            {
                Vector2 a = OuterWall[i], b = OuterWall[i + 1];
                if ((a.x - mid) * (b.x - mid) > 0f) continue;
                float t = (mid - a.x) / (b.x - a.x);
                list.Add(new Vector4(mid, Mathf.Lerp(a.y, b.y, t), (CanalEast - CanalWest) * 0.5f + 0.6f, 3f));
            }
            return list;
        }

        // The direction the outer wall runs at p (as a yaw: the wall's length is along local z).
        static float WallYawAt(Vector3 p)
        {
            var q = new Vector2(p.x, p.z);
            float best = float.MaxValue, yaw = 0f;
            for (int i = 0; i < OuterWall.Length - 1; i++)
            {
                Vector2 a = OuterWall[i], b = OuterWall[i + 1], ab = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(q - a, ab) / ab.sqrMagnitude);
                float d = Vector2.Distance(q, a + ab * t);
                if (d < best) { best = d; yaw = Mathf.Atan2(ab.x, ab.y) * Mathf.Rad2Deg; }
            }
            return yaw;
        }

        // A length of the outer wall, at any angle: taller and thicker than the old one, with a
        // plinth, a trim, battlements and a walkway along the top.
        static void OuterRun(LevelKit kit, Transform root, Vector2 a2, Vector2 b2)
        {
            Vector3 a = new Vector3(a2.x, 0f, a2.y), b = new Vector3(b2.x, 0f, b2.y);
            Vector3 d = b - a;
            float len = d.magnitude;
            if (len < 0.4f) return;
            float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            var rot = Quaternion.Euler(0f, yaw, 0f);
            Vector3 mid = (a + b) * 0.5f;
            var body = kit.Block(mid + Vector3.up * OuterHeight * 0.5f, new Vector3(OuterThick, OuterHeight, len), new Color(0.9f, 0.85f, 0.75f), "TownWallRun", wallStone);
            body.rotation = rot;
            body.SetParent(root, true);
            kit.Deco(PrimitiveType.Cube, mid + Vector3.up * 0.4f, new Vector3(OuterThick + 0.3f, 0.8f, len), wallPlinth, new Vector3(0f, yaw, 0f), root);
            kit.Deco(PrimitiveType.Cube, mid + Vector3.up * (OuterHeight - 0.05f), new Vector3(OuterThick + 0.2f, 0.2f, len), wallTrim, new Vector3(0f, yaw, 0f), root);
            // A string course halfway up, and buttresses every so often on the outside.
            kit.Deco(PrimitiveType.Cube, mid + Vector3.up * (OuterHeight * 0.55f), new Vector3(OuterThick + 0.12f, 0.16f, len), wallPlinth, new Vector3(0f, yaw, 0f), root);
            Vector3 dir = d / len, outward = rot * Vector3.right;
            if (InTown(mid + outward * 3f)) outward = -outward;
            var red = new MeshBatch();
            var blue = new MeshBatch();
            var buttress = new MeshBatch();
            int count = Mathf.Max(1, Mathf.FloorToInt((len - 0.4f) / 1.5f));
            float step = len / count;
            for (int i = 0; i < count; i++)
            {
                Vector3 c = a + dir * (step * (i + 0.5f));
                // Merlons on both edges of the walkway (the outer ones taller).
                (i % 2 == 0 ? red : blue).Box(c + outward * (OuterThick * 0.5f - 0.22f) + Vector3.up * (OuterHeight + 0.45f), new Vector3(0.44f, 0.9f, 0.8f), rot);
                if (i % 2 == 0) (i % 4 == 0 ? blue : red).Box(c - outward * (OuterThick * 0.5f - 0.18f) + Vector3.up * (OuterHeight + 0.22f), new Vector3(0.36f, 0.44f, 0.8f), rot);
                if (i % 5 == 2) buttress.Box(c + outward * (OuterThick * 0.5f + 0.35f) + Vector3.up * (OuterHeight * 0.32f), new Vector3(0.7f, OuterHeight * 0.64f, 1.1f), rot);
            }
            red.Build(root, "Battlements", capRed, true);
            blue.Build(root, "Battlements", capBlue, true);
            buttress.Build(root, "Buttresses", wallPlinth, true);
            TownKit.Solid(kit, mid + Vector3.up * (OuterHeight + 0.45f) + outward * (OuterThick * 0.5f - 0.22f), new Vector3(0.44f, 0.9f, len), yaw);
        }

        // A great gatehouse on the outer wall: two big round towers, a tall gate block between
        // them with its own pitched roof, the portcullis drawn up, banners and the town's shield
        // over the arch, and lanterns either side of the way through.
        static void GrandGate(LevelKit kit, Transform root, Vector3 centre, float yaw, float span, float scale, string name)
        {
            var rot = Quaternion.Euler(0f, yaw, 0f);
            Vector3 along = rot * Vector3.forward, outward = rot * Vector3.right;
            if (InTown(centre + outward * 4f)) outward = -outward;
            float half = span * 0.5f;
            float towerR = 2.6f * scale, towerH = OuterHeight + 4.2f * scale;
            for (int s = -1; s <= 1; s += 2)
            {
                // (The towers stand a little proud of the wall on the outside, as gate towers do.)
                WallTower(kit, root, centre + along * s * half + outward * (towerR * 0.35f), towerR, towerH - TowerExtra, s < 0);
            }
            var g = kit.Group(name.Replace(" ", ""), centre);
            g.SetParent(root, true);
            g.rotation = Quaternion.LookRotation(outward, Vector3.up);
            // In g: x along the wall, z out of town.
            float clear = 5.4f * scale, beam = 2.6f * scale, inner = span - towerR * 1.6f, depth = 4.6f * scale;
            float pier = 1.1f * scale, way = inner - pier * 2f;
            System.Func<float, float, float, Vector3> W = (x, y, z) => centre + Quaternion.LookRotation(outward, Vector3.up) * new Vector3(x, y, z);
            for (int s = -1; s <= 1; s += 2)
            {
                var p = kit.Block(W(s * (way * 0.5f + pier * 0.5f), (clear + beam) * 0.5f, 0f), new Vector3(pier, clear + beam, depth), new Color(0.9f, 0.85f, 0.75f), "GatePier", wallStone);
                p.rotation = g.rotation;
                p.SetParent(g, true);
            }
            var lintel = kit.Block(W(0f, clear + beam * 0.5f, 0f), new Vector3(way + 0.02f, beam, depth), new Color(0.9f, 0.85f, 0.75f), "GateArch", wallStone);
            lintel.rotation = g.rotation;
            lintel.SetParent(g, true);
            // The rounded underside of the arch, a trim and the battlements on top.
            var arch = kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, clear, 0f), new Vector3(way, depth * 0.5f + 0.02f, 1.4f * scale), wallTrim, new Vector3(90f, 0f, 0f), g);
            arch.name = "GateArchCurve";
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, clear + beam, 0f), new Vector3(inner + 0.4f, 0.24f, depth + 0.3f), wallTrim, default(Vector3), g);
            int teeth = Mathf.Max(3, Mathf.FloorToInt(inner / 1.3f));
            for (int i = 0; i < teeth; i++)
            {
                float u = -inner * 0.5f + inner * (i + 0.5f) / teeth;
                kit.Deco(PrimitiveType.Cube, new Vector3(u, clear + beam + 0.5f, depth * 0.5f - 0.2f), new Vector3(0.8f, 0.8f, 0.4f), i % 2 == 0 ? capRed : capBlue, default(Vector3), g);
            }
            // A pitched roof over the gate passage, behind the battlements.
            var roof = TownKit.GableRoof(kit, W(0f, clear + beam + 0.1f, -depth * 0.12f), inner + 0.6f, depth * 0.8f, 2.6f * scale, yaw + 90f, roofRed, wallStone);
            if (roof != null) roof.SetParent(g, true);
            // The portcullis, drawn up: its iron teeth showing under the arch.
            var iron = PatchworkAssets.Plain(TownKit.Iron, 0.5f);
            for (float u = -way * 0.5f + 0.45f; u < way * 0.5f - 0.3f; u += 0.62f)
                kit.Deco(PrimitiveType.Cube, new Vector3(u, clear - 0.2f, depth * 0.18f), new Vector3(0.14f, 0.7f, 0.14f), iron, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, clear + 0.12f, depth * 0.18f), new Vector3(way, 0.16f, 0.16f), iron, default(Vector3), g);
            // The shield over the arch, and long banners, on both faces.
            for (int s = -1; s <= 1; s += 2)
            {
                var shield = kit.Group("TownShield", W(0f, clear + beam * 0.55f, s * (depth * 0.5f + 0.08f)));
                shield.SetParent(g, true);
                shield.rotation = Quaternion.LookRotation(outward * s, Vector3.up);
                float k = scale * 1.4f;
                kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(1.3f, 1.2f, 0.12f) * k, gold, default(Vector3), shield);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.02f, 0.05f) * k, new Vector3(1.1f, 1.02f, 0.06f) * k, PatchworkAssets.Felt(new Color(0.3f, 0.45f, 0.72f)), default(Vector3), shield);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.55f, 0.04f) * k, new Vector3(1.1f, 0.5f, 0.12f) * k, PatchworkAssets.Felt(new Color(0.3f, 0.45f, 0.72f)), default(Vector3), shield);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.05f, 0.1f) * k, new Vector3(0.46f, 0.04f, 0.46f) * k, PatchworkAssets.Felt(LevelKit.Palette.Rose), new Vector3(90f, 0f, 0f), shield);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.05f, 0.14f) * k, new Vector3(0.08f, 0.95f, 0.03f) * k, PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.9f), 0.8f), new Vector3(0f, 0f, 35f), shield);
                for (int b = -1; b <= 1; b += 2)
                {
                    var banner = kit.Group("GateBanner", W(b * (way * 0.5f + pier * 0.5f), clear + beam - 0.3f, s * (depth * 0.5f + 0.06f)));
                    banner.SetParent(g, true);
                    banner.rotation = Quaternion.LookRotation(outward * s, Vector3.up);
                    float bl = (clear + beam) * 0.62f;
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, -bl * 0.5f, 0f), new Vector3(1f * scale, bl, 0.04f), PatchworkAssets.Felt(b < 0 ? new Color(0.82f, 0.3f, 0.3f) : new Color(0.3f, 0.45f, 0.72f)), default(Vector3), banner);
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, -bl * 0.72f, -0.03f), new Vector3(1f * scale, 0.24f, 0.03f), PatchworkAssets.Felt(LevelKit.Palette.Gold), default(Vector3), banner);
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, -bl - 0.2f, 0f), new Vector3(0.7f * scale, 0.5f, 0.04f), PatchworkAssets.Felt(b < 0 ? new Color(0.82f, 0.3f, 0.3f) : new Color(0.3f, 0.45f, 0.72f)), new Vector3(0f, 0f, 45f), banner);
                }
            }
            // Lanterns on brackets either side of the way through, on the outside face.
            for (int s = -1; s <= 1; s += 2)
                TownKit.StreetLamp(kit, W(s * (way * 0.5f + pier + 0.9f), 0f, depth * 0.5f + 1.3f));
            ClearGrass(centre, span * 0.5f);
        }

        // A small arched postern through the outer wall.
        static void Postern(LevelKit kit, Transform root, Vector3 centre, float yaw)
        {
            var rot = Quaternion.Euler(0f, yaw, 0f);
            Vector3 along = rot * Vector3.forward;
            for (int s = -1; s <= 1; s += 2)
                WallTower(kit, root, centre + along * s * 2.4f, 1.3f, OuterHeight + 0.4f - TowerExtra, s > 0);
            var lintel = kit.Block(centre + Vector3.up * (OuterHeight - 0.9f), new Vector3(OuterThick, 1.8f, 2.4f), new Color(0.9f, 0.85f, 0.75f), "PosternArch", wallStone);
            lintel.rotation = rot;
            lintel.SetParent(root, true);
            kit.Deco(PrimitiveType.Cube, centre + Vector3.up * (OuterHeight + 0.45f), new Vector3(OuterThick + 0.04f, 0.9f, 0.8f), capRed, new Vector3(0f, yaw, 0f), root);
        }

        // Where the wall crosses the canal: an arch over the water, high enough for a swimmer
        // (or a boat), with the portcullis drawn up and piers on both quays.
        static void WaterGate(LevelKit kit, Transform root, Vector3 centre, float yaw, float halfWidth)
        {
            var rot = Quaternion.Euler(0f, yaw, 0f);
            Vector3 along = rot * Vector3.forward;
            float bottom = 2.8f, len = halfWidth * 2f + 0.4f;
            var body = kit.Block(centre + Vector3.up * ((bottom + OuterHeight) * 0.5f), new Vector3(OuterThick, OuterHeight - bottom, len), new Color(0.9f, 0.85f, 0.75f), "WaterGateArch", wallStone);
            body.rotation = rot;
            body.SetParent(root, true);
            kit.Deco(PrimitiveType.Cube, centre + Vector3.up * (OuterHeight - 0.05f), new Vector3(OuterThick + 0.2f, 0.2f, len), wallTrim, new Vector3(0f, yaw, 0f), root);
            var arch = kit.Deco(PrimitiveType.Cylinder, centre + Vector3.up * bottom, new Vector3(2.4f, (OuterThick + 0.04f) * 0.5f, len - 1.2f), wallTrim, new Vector3(0f, yaw, 90f), root);
            arch.name = "WaterArch";
            var iron = PatchworkAssets.Plain(TownKit.Iron, 0.5f);
            for (float u = -len * 0.5f + 0.6f; u < len * 0.5f - 0.5f; u += 0.6f)
                kit.Deco(PrimitiveType.Cube, centre + along * u + Vector3.up * (bottom - 0.35f), new Vector3(0.12f, 0.8f, 0.12f), iron, new Vector3(0f, yaw, 0f), root);
            for (int i = 0; i < Mathf.FloorToInt(len / 1.5f); i++)
                kit.Deco(PrimitiveType.Cube, centre + along * (-len * 0.5f + 0.75f + i * 1.5f) + Vector3.up * (OuterHeight + 0.45f), new Vector3(OuterThick + 0.04f, 0.9f, 0.8f), i % 2 == 0 ? capRed : capBlue, new Vector3(0f, yaw, 0f), root);
            TownKit.Solid(kit, centre + Vector3.up * (OuterHeight + 0.45f), new Vector3(OuterThick, 0.9f, len), yaw);
            // Piers on the quays (not where an old corner tower already stands).
            var oldCorner = new Vector3(CanalWest - 2.15f, 0f, WallSouth);
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 at = centre + along * s * (halfWidth + 1.2f);
                if (Vector3.Distance(at, oldCorner) < 3f) continue;
                WallTower(kit, root, at, 1.5f, OuterHeight + 1.2f - TowerExtra, s < 0);
            }
        }

        // A cone, point up, for the tower roofs.
        static void TowerCone(LevelKit kit, Transform parent, Vector3 localBase, float radius, float height, Material mat)
        {
            const int segs = 20;
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            for (int i = 0; i <= segs; i++)
            {
                float a = i * Mathf.PI * 2f / segs;
                verts.Add(new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius));
                uvs.Add(new Vector2(i / (float)segs * 3f, 0f));
                verts.Add(new Vector3(0f, height, 0f));
                uvs.Add(new Vector2(i / (float)segs * 3f, 1.5f));
            }
            // (Both windings, so it shows from outside whichever way round Unity takes them.)
            for (int i = 0; i < segs; i++)
            {
                int b = i * 2;
                tris.Add(b); tris.Add(b + 1); tris.Add(b + 2);
                tris.Add(b); tris.Add(b + 2); tris.Add(b + 1);
            }
            // The underside, so it isn't see-through from below.
            int c = verts.Count;
            verts.Add(Vector3.zero);
            uvs.Add(Vector2.zero);
            for (int i = 0; i < segs; i++) { tris.Add(c); tris.Add(i * 2); tris.Add(i * 2 + 2); tris.Add(c); tris.Add(i * 2 + 2); tris.Add(i * 2); }
            var mesh = new Mesh();
            mesh.name = "TowerRoof";
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            var go = new GameObject("TowerRoof");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localBase;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            go.AddComponent<OwnedMesh>().Mesh = mesh;
        }
    }
}

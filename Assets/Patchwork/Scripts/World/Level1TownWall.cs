using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The town wall: Toybox Town as a walled medieval village, like a toy fort out of a toybox.
    // Cream stone blocks with painted battlements, round towers with pointed roofs and pennants,
    // and arched gatehouses. It runs:
    //   - west, along the top of the ravine (the ravine is the moat), open in the middle where
    //     the west street, the swing and the steps down are
    //   - east, along the canal (the canal is the moat), with gates at the bridges and the steps
    //   - north, past the back of the barracks (the castle yard), with the Castle Gate in the
    //     middle, a postern by the carousel, and the corner left open for the xylophone bridge
    //   - south, behind Chimney Lane, with the old spire as one of its towers, the South Gate
    //     and a postern
    // Outside it are the outskirts: the fields and farm, the woods, Spindle Hill and the meadows.
    public static partial class Level1Builder
    {
        public const float WallWest = -36.55f, WallEast = 31.55f, WallNorth = 58.5f, WallSouth = -72f;
        const float WallHeight = 3.6f, WallThick = 1f;

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

            // East, along the canal: the East Gate at the main bridge, gaps at the two small
            // bridges (z +-40) and the steps down to the water (z 12.5 to 18), and the north end
            // left open for the way to the xylophone bridge.
            WallRun(kit, root, new Vector3(E, 0f, S), new Vector3(E, 0f, -43.4f));
            WallRun(kit, root, new Vector3(E, 0f, -36.6f), new Vector3(E, 0f, -7.2f));
            WallRun(kit, root, new Vector3(E, 0f, 7.2f), new Vector3(E, 0f, 12.5f));
            WallRun(kit, root, new Vector3(E, 0f, 18f), new Vector3(E, 0f, 36.6f));
            WallRun(kit, root, new Vector3(E, 0f, 43.4f), new Vector3(E, 0f, 52f));
            WallTower(kit, root, new Vector3(E, 0f, S), 2.2f, 7.4f, true);
            WallTower(kit, root, new Vector3(E, 0f, -43.4f), 1.4f, 5.6f, false);
            WallTower(kit, root, new Vector3(E, 0f, -36.6f), 1.4f, 5.6f, true);
            WallTower(kit, root, new Vector3(E, 0f, 36.6f), 1.4f, 5.6f, false);
            WallTower(kit, root, new Vector3(E, 0f, 43.4f), 1.4f, 5.6f, true);
            WallTower(kit, root, new Vector3(E, 0f, 52f), 1.8f, 6.4f, false);
            Gatehouse(kit, root, new Vector3(E, 0f, 0f), false, 14.4f, "East Gate");

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
            pins.Add(new MapPin("East Gate", new Vector3(E, 0f, 0f), MapPin.Kind.Area));
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

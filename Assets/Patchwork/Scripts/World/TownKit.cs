using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Building blocks for Toybox Town: timber-framed houses with shingle roofs, gas lamps,
    // clotheslines, bridges, stairs, a bandstand, a clock tower and more. Everything is made of
    // the same felt, plaster, brick, wood and brass as the rest of Patchwork.
    // Houses and roofs use yaw in steps of 90 degrees.
    public static class TownKit
    {
        public static readonly Color Iron = new Color(0.16f, 0.15f, 0.18f);
        public static readonly Color DarkWood = new Color(0.36f, 0.24f, 0.17f);
        public static readonly Color Stone = new Color(0.78f, 0.76f, 0.72f);
        public static readonly Color WarmLight = new Color(1f, 0.82f, 0.55f);

        static Quaternion Rot(float yaw) { return Quaternion.Euler(0f, yaw, 0f); }

        static Vector3 RotSize(Vector3 s, float yaw)
        {
            int q = Mathf.RoundToInt(Mathf.Repeat(yaw, 360f) / 90f) % 2;
            return q == 1 ? new Vector3(s.z, s.y, s.x) : s;
        }

        // ------------------------------------------------------------------ Meshes

        class MeshBuild
        {
            readonly List<Vector3> v = new List<Vector3>();
            readonly List<Vector3> n = new List<Vector3>();
            readonly List<Vector2> uv = new List<Vector2>();
            readonly List<int>[] tris;

            public MeshBuild(int subMeshes)
            {
                tris = new List<int>[subMeshes];
                for (int i = 0; i < subMeshes; i++) tris[i] = new List<int>();
            }

            public void Tri(int sub, Vector3 a, Vector3 b, Vector3 c, Vector3 outward, Vector2 ua, Vector2 ub, Vector2 uc)
            {
                if (Vector3.Dot(Vector3.Cross(b - a, c - a), outward) < 0f) { var t = b; b = c; c = t; var tu = ub; ub = uc; uc = tu; }
                int i0 = v.Count;
                v.Add(a); v.Add(b); v.Add(c);
                n.Add(outward); n.Add(outward); n.Add(outward);
                uv.Add(ua); uv.Add(ub); uv.Add(uc);
                tris[sub].Add(i0); tris[sub].Add(i0 + 1); tris[sub].Add(i0 + 2);
            }

            public Mesh Make(string name)
            {
                var m = new Mesh();
                m.name = name;
                m.SetVertices(v);
                m.SetNormals(n);
                m.SetUVs(0, uv);
                m.subMeshCount = tris.Length;
                for (int i = 0; i < tris.Length; i++) m.SetTriangles(tris[i], i);
                m.RecalculateBounds();
                m.RecalculateTangents();
                return m;
            }
        }

        static Transform MeshObject(LevelKit kit, string name, Vector3 pos, float yaw, Mesh mesh, Material[] mats, bool collider)
        {
            var go = new GameObject(name);
            go.transform.SetParent(kit.Root, false);
            go.transform.position = pos;
            go.transform.rotation = Rot(yaw);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials = mats;
            if (collider)
            {
                var c = go.AddComponent<MeshCollider>();
                c.sharedMesh = mesh;
                c.convex = true;
            }
            return go.transform;
        }

        // A gable roof: ridge along local x, sloping down to the eaves at z = +-width/2.
        // The two slopes use "roof", the triangular ends use "gable" (usually the wall plaster).
        public static Transform GableRoof(LevelKit kit, Vector3 eaveCenter, float length, float width, float rise, float yaw, Material roof, Material gable)
        {
            var mb = new MeshBuild(2);
            float L = length * 0.5f, W = width * 0.5f, uvs = 2.5f;
            Vector3 a0 = new Vector3(-L, 0f, -W), a1 = new Vector3(L, 0f, -W), r0 = new Vector3(-L, rise, 0f), r1 = new Vector3(L, rise, 0f);
            Vector3 b0 = new Vector3(-L, 0f, W), b1 = new Vector3(L, 0f, W);
            float slope = Mathf.Sqrt(W * W + rise * rise) / uvs;
            Vector3 outA = new Vector3(0f, W, -rise).normalized, outB = new Vector3(0f, W, rise).normalized;
            mb.Tri(0, a0, a1, r1, outA, new Vector2(0f, 0f), new Vector2(length / uvs, 0f), new Vector2(length / uvs, slope));
            mb.Tri(0, a0, r1, r0, outA, new Vector2(0f, 0f), new Vector2(length / uvs, slope), new Vector2(0f, slope));
            mb.Tri(0, b0, b1, r1, outB, new Vector2(0f, 0f), new Vector2(length / uvs, 0f), new Vector2(length / uvs, slope));
            mb.Tri(0, b0, r1, r0, outB, new Vector2(0f, 0f), new Vector2(length / uvs, slope), new Vector2(0f, slope));
            mb.Tri(1, a0, b0, r0, Vector3.left, new Vector2(-W / uvs, 0f), new Vector2(W / uvs, 0f), new Vector2(0f, rise / uvs));
            mb.Tri(1, a1, b1, r1, Vector3.right, new Vector2(-W / uvs, 0f), new Vector2(W / uvs, 0f), new Vector2(0f, rise / uvs));
            mb.Tri(1, a0, a1, b1, Vector3.down, Vector2.zero, Vector2.right, Vector2.one);
            mb.Tri(1, a0, b1, b0, Vector3.down, Vector2.zero, Vector2.one, Vector2.up);
            return MeshObject(kit, "Roof", eaveCenter, yaw, mb.Make("GableRoof"), new[] { roof, gable }, true);
        }

        // ------------------------------------------------------------------ Houses

        public struct House
        {
            public Transform Body;
            public Vector3 ChimneyTop;   // zero if no chimney
            public float RoofTop;
        }

        public enum Sign { None, Bread, Teacup, Spool, Hat, Toy, Dye, Clock }

        // A timber-framed house. pos is the middle of its footprint at ground level; size is
        // (width, wall height, depth). The front (door) faces local +z (rotated by yaw).
        // chimney: 0 none, 1 on the front slope, -1 on the back slope; chimneyX is along the ridge.
        public static House MakeHouse(LevelKit kit, Vector3 pos, Vector3 size, float yaw, Color wall, Color roof, int seed,
            int chimney = 0, float chimneyX = 0f, Sign sign = Sign.None, bool door = true)
        {
            var r = Rot(yaw);
            System.Func<float, float, float, Vector3> W = (x, y, z) => pos + r * new Vector3(x, y, z);
            float w = size.x, h = size.y, d = size.z;
            var plaster = PatchworkAssets.Plaster(wall);
            var beam = PatchworkAssets.Wood(DarkWood);
            var house = new House();
            house.Body = kit.Block(W(0f, h * 0.5f, 0f), RotSize(size, yaw), wall, "House", plaster);
            // Stone footing.
            kit.Deco(PrimitiveType.Cube, W(0f, 0.3f, 0f), RotSize(new Vector3(w + 0.12f, 0.6f, d + 0.12f), yaw), PatchworkAssets.Cobble(Stone));

            // Timber frame on the front and back.
            for (int side = -1; side <= 1; side += 2)
            {
                float fz = side * (d * 0.5f + 0.04f);
                int posts = Mathf.Max(2, Mathf.RoundToInt(w / 1.7f));
                for (int i = 0; i <= posts; i++)
                {
                    float x = -w * 0.5f + w * i / posts;
                    if (side > 0 && door && Mathf.Abs(x) < 1f) continue;   // no post through the doorway
                    kit.Deco(PrimitiveType.Cube, W(x, (h + 0.6f) * 0.5f, fz), RotSize(new Vector3(0.18f, h - 0.6f, 0.1f), yaw), beam);
                }
                for (float y = 3f; y < h - 0.5f; y += 3f)
                    kit.Deco(PrimitiveType.Cube, W(0f, y, fz), RotSize(new Vector3(w, 0.2f, 0.12f), yaw), beam);
                kit.Deco(PrimitiveType.Cube, W(0f, h - 0.1f, fz), RotSize(new Vector3(w + 0.1f, 0.2f, 0.12f), yaw), beam);
                // A pair of diagonal braces on the upper floor.
                if (h > 4.5f)
                {
                    float by = Mathf.Min(h - 1.6f, 4.5f);
                    var b1 = kit.Deco(PrimitiveType.Cube, W(-w * 0.5f + 0.7f, by, fz), new Vector3(0.14f, 1.9f, 0.1f), beam);
                    b1.rotation = r * Quaternion.Euler(0f, 0f, 35f);
                    var b2 = kit.Deco(PrimitiveType.Cube, W(w * 0.5f - 0.7f, by, fz), new Vector3(0.14f, 1.9f, 0.1f), beam);
                    b2.rotation = r * Quaternion.Euler(0f, 0f, -35f);
                }
            }
            // Corner posts on the sides.
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    kit.Deco(PrimitiveType.Cube, W(sx * (w * 0.5f + 0.02f), (h + 0.6f) * 0.5f, sz * (d * 0.5f + 0.02f)), new Vector3(0.24f, h - 0.6f, 0.24f), beam);

            // Windows on every floor of the front and back (the door takes the front middle).
            var pane = PatchworkAssets.Glow(new Color(1f, 0.86f, 0.6f), 0.55f);
            var frame = PatchworkAssets.Wood(new Color(0.95f, 0.92f, 0.85f));
            Color shutterCol = Color.HSVToRGB(Mathf.Repeat(seed * 0.17f, 1f), 0.35f, 0.75f);
            var shutter = PatchworkAssets.Felt(shutterCol);
            int cols = Mathf.Max(1, Mathf.FloorToInt(w / 2.4f));
            for (int side = -1; side <= 1; side += 2)
            {
                float fz = side * (d * 0.5f + 0.06f);
                for (float y = 1.6f; y < h - 0.8f; y += 3f)
                    for (int c = 0; c < cols; c++)
                    {
                        float x = -w * 0.5f + w * (c + 0.5f) / cols;
                        if (side > 0 && door && y < 2f && Mathf.Abs(x) < 1.3f) continue;
                        Window(kit, W(x, y + 0.2f, fz), yaw + (side > 0 ? 0f : 180f), pane, frame, shutter, side > 0 && (c + seed) % 2 == 0);
                    }
            }
            if (door) Door(kit, W(0f, 0f, d * 0.5f + 0.06f), yaw, Color.HSVToRGB(Mathf.Repeat(0.03f + seed * 0.23f, 1f), 0.5f, 0.55f));

            // Roof with a bit of overhang.
            float rise = d * 0.45f;
            var roofMat = PatchworkAssets.Shingle(roof);
            GableRoof(kit, W(0f, h, 0f), w + 0.9f, d + 1.1f, rise, yaw, roofMat, plaster);
            house.RoofTop = pos.y + h + rise;
            if (chimney != 0)
            {
                float cz = chimney * d * 0.22f;
                float baseY = h + rise * (1f - Mathf.Abs(cz) / (d * 0.55f)) - 0.6f;
                float topY = h + rise + 1.1f;
                var brick = PatchworkAssets.Brick(new Color(0.7f, 0.38f, 0.3f));
                kit.Block(W(chimneyX, (baseY + topY) * 0.5f, cz), new Vector3(1.2f, topY - baseY, 1.2f), new Color(0.7f, 0.38f, 0.3f), "Chimney", brick);
                kit.Deco(PrimitiveType.Cube, W(chimneyX, topY + 0.08f, cz), new Vector3(1.45f, 0.16f, 1.45f), brick);
                house.ChimneyTop = W(chimneyX, topY + 0.16f, cz);
            }
            if (sign != Sign.None) ShopSign(kit, W(w * 0.5f - 0.3f, 3.4f, d * 0.5f + 0.1f), yaw, sign);
            return house;
        }

        static void Window(LevelKit kit, Vector3 p, float yaw, Material pane, Material frame, Material shutter, bool flowers)
        {
            var r = Rot(yaw);
            kit.Deco(PrimitiveType.Cube, p, new Vector3(1.05f, 1.35f, 0.08f), frame, new Vector3(0f, yaw, 0f));
            kit.Deco(PrimitiveType.Cube, p + r * new Vector3(0f, 0f, 0.03f), new Vector3(0.85f, 1.15f, 0.04f), pane, new Vector3(0f, yaw, 0f));
            kit.Deco(PrimitiveType.Cube, p + r * new Vector3(0f, 0f, 0.06f), new Vector3(0.06f, 1.15f, 0.03f), frame, new Vector3(0f, yaw, 0f));
            kit.Deco(PrimitiveType.Cube, p + r * new Vector3(0f, 0.05f, 0.06f), new Vector3(0.85f, 0.06f, 0.03f), frame, new Vector3(0f, yaw, 0f));
            kit.Deco(PrimitiveType.Cube, p + r * new Vector3(-0.75f, 0f, 0.03f), new Vector3(0.42f, 1.3f, 0.05f), shutter, new Vector3(0f, yaw, 0f));
            kit.Deco(PrimitiveType.Cube, p + r * new Vector3(0.75f, 0f, 0.03f), new Vector3(0.42f, 1.3f, 0.05f), shutter, new Vector3(0f, yaw, 0f));
            kit.Deco(PrimitiveType.Cube, p + r * new Vector3(0f, -0.72f, 0.1f), new Vector3(1.2f, 0.08f, 0.22f), frame, new Vector3(0f, yaw, 0f));
            if (flowers)
            {
                kit.Deco(PrimitiveType.Cube, p + r * new Vector3(0f, -0.88f, 0.2f), new Vector3(1.1f, 0.25f, 0.3f), PatchworkAssets.Planks(new Color(0.55f, 0.38f, 0.25f)), new Vector3(0f, yaw, 0f));
                Color[] cols = { LevelKit.Palette.Rose, LevelKit.Palette.Butter, LevelKit.Palette.Lavender, new Color(1f, 0.55f, 0.6f) };
                for (int i = 0; i < 4; i++)
                    kit.Deco(PrimitiveType.Sphere, p + r * new Vector3(-0.38f + i * 0.25f, -0.7f, 0.22f), Vector3.one * 0.2f, PatchworkAssets.Felt(cols[i]));
            }
        }

        static void Door(LevelKit kit, Vector3 p, float yaw, Color col)
        {
            var r = Rot(yaw);
            // Layers from the wall out, each a few cm in front of the last so nothing flickers:
            // the dark frame and its arch, then the door and its arched top, then the knob.
            var wood = PatchworkAssets.Planks(col);
            var frame = PatchworkAssets.Wood(DarkWood);
            kit.Deco(PrimitiveType.Cube, p + r * new Vector3(0f, 1.15f, 0f), new Vector3(1.55f, 2.3f, 0.08f), frame, new Vector3(0f, yaw, 0f));
            kit.Deco(PrimitiveType.Cylinder, p + r * new Vector3(0f, 2.3f, 0f), new Vector3(1.55f, 0.035f, 0.75f), frame, new Vector3(90f, yaw, 0f));
            kit.Deco(PrimitiveType.Cube, p + r * new Vector3(0f, 1.1f, 0.09f), new Vector3(1.25f, 2.2f, 0.08f), wood, new Vector3(0f, yaw, 0f));
            kit.Deco(PrimitiveType.Cylinder, p + r * new Vector3(0f, 2.2f, 0.085f), new Vector3(1.25f, 0.03f, 0.55f), wood, new Vector3(90f, yaw, 0f));
            // Iron strap hinges and a brass knob and knocker.
            var iron = PatchworkAssets.Plain(Iron, 0.5f);
            kit.Deco(PrimitiveType.Cube, p + r * new Vector3(-0.25f, 1.75f, 0.145f), new Vector3(0.7f, 0.07f, 0.03f), iron, new Vector3(0f, yaw, 0f));
            kit.Deco(PrimitiveType.Cube, p + r * new Vector3(-0.25f, 0.45f, 0.145f), new Vector3(0.7f, 0.07f, 0.03f), iron, new Vector3(0f, yaw, 0f));
            kit.Deco(PrimitiveType.Sphere, p + r * new Vector3(0.42f, 1.05f, 0.18f), Vector3.one * 0.12f, PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f));
            kit.Deco(PrimitiveType.Cylinder, p + r * new Vector3(0f, 1.5f, 0.16f), new Vector3(0.22f, 0.015f, 0.22f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f), new Vector3(90f, yaw, 0f));
            kit.Deco(PrimitiveType.Cube, p + r * new Vector3(0f, 0.08f, 0.45f), new Vector3(1.9f, 0.16f, 0.9f), PatchworkAssets.Cobble(Stone), new Vector3(0f, yaw, 0f));
            // A little lamp beside the door.
            Vector3 lamp = p + r * new Vector3(-1.05f, 2.3f, 0.25f);
            kit.Deco(PrimitiveType.Cube, lamp + r * new Vector3(0f, 0.15f, -0.15f), new Vector3(0.06f, 0.06f, 0.35f), PatchworkAssets.Plain(Iron, 0.5f), new Vector3(0f, yaw, 0f));
            kit.Deco(PrimitiveType.Sphere, lamp, new Vector3(0.28f, 0.36f, 0.28f), PatchworkAssets.Glow(WarmLight, 1.2f));
        }

        // A hanging painted sign on an iron bracket, with a little picture of what the shop sells.
        static void ShopSign(LevelKit kit, Vector3 wallPoint, float yaw, Sign sign)
        {
            var r = Rot(yaw);
            var iron = PatchworkAssets.Plain(Iron, 0.5f);
            kit.Deco(PrimitiveType.Cube, wallPoint + r * new Vector3(0f, 0.4f, 0.55f), new Vector3(0.06f, 0.06f, 1.1f), iron, new Vector3(0f, yaw, 0f));
            Vector3 c = wallPoint + r * new Vector3(0f, -0.15f, 0.9f);
            kit.Deco(PrimitiveType.Cube, c, new Vector3(0.08f, 0.85f, 0.95f), PatchworkAssets.Planks(new Color(0.55f, 0.35f, 0.25f)), new Vector3(0f, yaw, 0f));
            kit.Deco(PrimitiveType.Cube, c, new Vector3(0.1f, 0.95f, 1.05f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f), new Vector3(0f, yaw, 0f));
            Vector3 side = r * Vector3.right;
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 f = c + side * (s * 0.07f);
                switch (sign)
                {
                    case Sign.Bread:
                        kit.Deco(PrimitiveType.Capsule, f, new Vector3(0.05f, 0.25f, 0.2f), PatchworkAssets.Felt(new Color(0.85f, 0.6f, 0.3f)), new Vector3(90f, yaw, 0f));
                        break;
                    case Sign.Teacup:
                        kit.Deco(PrimitiveType.Cylinder, f, new Vector3(0.05f, 0.18f, 0.32f), PatchworkAssets.Felt(new Color(0.95f, 0.95f, 0.98f)), new Vector3(0f, yaw, 90f));
                        break;
                    case Sign.Spool:
                        kit.Deco(PrimitiveType.Cylinder, f, new Vector3(0.05f, 0.25f, 0.4f), PatchworkAssets.Felt(LevelKit.Palette.Rose), new Vector3(0f, yaw, 90f));
                        break;
                    case Sign.Hat:
                        kit.Deco(PrimitiveType.Cube, f, new Vector3(0.05f, 0.3f, 0.26f), PatchworkAssets.Felt(new Color(0.15f, 0.12f, 0.16f)), new Vector3(0f, yaw, 0f));
                        break;
                    case Sign.Dye:
                        kit.Deco(PrimitiveType.Sphere, f, new Vector3(0.05f, 0.35f, 0.3f), PatchworkAssets.Felt(new Color(0.4f, 0.6f, 0.9f)), new Vector3(0f, yaw, 0f));
                        break;
                    case Sign.Clock:
                        kit.Deco(PrimitiveType.Cylinder, f, new Vector3(0.38f, 0.05f, 0.38f), PatchworkAssets.Felt(LevelKit.Palette.Cream), new Vector3(0f, yaw, 90f));
                        break;
                    default:
                        kit.Deco(PrimitiveType.Sphere, f, new Vector3(0.05f, 0.3f, 0.3f), PatchworkAssets.Felt(LevelKit.Palette.Butter), new Vector3(0f, yaw, 0f));
                        break;
                }
            }
        }

        // ------------------------------------------------------------------ Street furniture

        // A Victorian gas lamp on an iron post.
        public static void StreetLamp(LevelKit kit, Vector3 feet, bool light = true)
        {
            var iron = PatchworkAssets.Plain(Iron, 0.55f);
            kit.Deco(PrimitiveType.Cylinder, feet + Vector3.up * 0.25f, new Vector3(0.55f, 0.25f, 0.55f), iron);
            kit.Deco(PrimitiveType.Cylinder, feet + Vector3.up * 2f, new Vector3(0.16f, 1.8f, 0.16f), iron);
            kit.Deco(PrimitiveType.Sphere, feet + Vector3.up * 1.2f, Vector3.one * 0.28f, iron);
            kit.Deco(PrimitiveType.Cylinder, feet + Vector3.up * 3.8f, new Vector3(0.36f, 0.06f, 0.36f), iron);
            kit.Deco(PrimitiveType.Cube, feet + Vector3.up * 4.25f, new Vector3(0.42f, 0.7f, 0.42f), PatchworkAssets.Glow(WarmLight, 1.4f));
            var cap = kit.Deco(PrimitiveType.Cube, feet + Vector3.up * 4.75f, new Vector3(0.5f, 0.5f, 0.5f), iron);
            cap.rotation = Quaternion.Euler(45f, 45f, 0f);
            cap.localScale = new Vector3(0.42f, 0.42f, 0.42f);
            kit.Deco(PrimitiveType.Sphere, feet + Vector3.up * 5.1f, Vector3.one * 0.14f, PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f));
            var col = kit.Group("LampPost", feet + Vector3.up * 2f).gameObject.AddComponent<CapsuleCollider>();
            col.radius = 0.2f;
            col.height = 4f;
            if (!light) return;
            var go = new GameObject("LampLight");
            go.transform.SetParent(kit.Root, false);
            go.transform.position = feet + Vector3.up * 4.2f;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = WarmLight;
            l.range = 9f;
            l.intensity = 0.9f;
            l.shadows = LightShadows.None;
        }

        // A clothesline between two poles, sagging in the middle, with laundry pegged on and some
        // of the pegs wrapped in pink yarn for Yarn Form to swing from. Returns the swing pegs.
        public static List<YarnAnchor> Clothesline(LevelKit kit, Vector3 footA, Vector3 footB, float height, float sag, float[] swingAt, float range = 13.5f)
        {
            var wood = PatchworkAssets.Wood(new Color(0.7f, 0.52f, 0.36f));
            foreach (var f in new[] { footA, footB })
            {
                kit.Block(f + Vector3.up * (height * 0.5f + 0.15f), new Vector3(0.35f, height + 0.3f, 0.35f), new Color(0.7f, 0.52f, 0.36f), "LinePole", wood);
                kit.Deco(PrimitiveType.Cube, f + Vector3.up * (height + 0.2f), new Vector3(1.2f, 0.2f, 0.2f), wood, new Vector3(0f, Mathf.Atan2(footB.x - footA.x, footB.z - footA.z) * Mathf.Rad2Deg + 90f, 0f));
            }
            var rope = PatchworkAssets.Yarn(new Color(0.95f, 0.92f, 0.85f));
            System.Func<float, Vector3> P = k => Vector3.Lerp(footA, footB, k) + Vector3.up * (height - Mathf.Sin(k * Mathf.PI) * sag);
            const int segs = 14;
            for (int i = 0; i < segs; i++) kit.Rope(P(i / (float)segs), P((i + 1) / (float)segs), rope, 0.05f);
            // Laundry.
            Color[] cols = { LevelKit.Palette.Sky, LevelKit.Palette.Rose, LevelKit.Palette.Butter, LevelKit.Palette.Sage, LevelKit.Palette.Lavender, new Color(0.95f, 0.95f, 0.95f) };
            float yaw = Mathf.Atan2(footB.x - footA.x, footB.z - footA.z) * Mathf.Rad2Deg + 90f;
            int item = 0;
            for (float k = 0.12f; k < 0.92f; k += 0.11f)
            {
                bool skip = false;
                foreach (float s in swingAt) if (Mathf.Abs(s - k) < 0.07f) skip = true;
                if (skip) continue;
                Vector3 p = P(k);
                var cloth = PatchworkAssets.Fabric(cols[item % cols.Length]);
                if (item % 3 == 0)
                {
                    kit.Deco(PrimitiveType.Cube, p + Vector3.down * 0.55f, new Vector3(0.9f, 1f, 0.05f), cloth, new Vector3(0f, yaw, 0f));
                    kit.Deco(PrimitiveType.Cube, p + Vector3.down * 0.2f, new Vector3(1.5f, 0.3f, 0.05f), cloth, new Vector3(0f, yaw, 0f));
                }
                else if (item % 3 == 1)
                    kit.Deco(PrimitiveType.Capsule, p + Vector3.down * 0.45f, new Vector3(0.22f, 0.4f, 0.12f), PatchworkAssets.Knit(cols[(item + 2) % cols.Length]), new Vector3(0f, yaw, 12f));
                else
                    kit.Deco(PrimitiveType.Cube, p + Vector3.down * 0.4f, new Vector3(0.8f, 0.7f, 0.05f), cloth, new Vector3(0f, yaw, 0f));
                kit.Deco(PrimitiveType.Cube, p, new Vector3(0.08f, 0.22f, 0.12f), wood, new Vector3(0f, yaw, 0f));
                item++;
            }
            var anchors = new List<YarnAnchor>();
            foreach (float s in swingAt) anchors.Add(kit.Anchor(P(s), range, LevelKit.AnchorStyle.Peg));
            return anchors;
        }

        // Two posts with a crossbeam, and a pink-trimmed lantern hanging from the middle to swing on.
        public static YarnAnchor LanternGate(LevelKit kit, Vector3 footA, Vector3 footB, float beamY, float lanternY, float range = 13.5f)
        {
            var wood = PatchworkAssets.Wood(new Color(0.6f, 0.42f, 0.3f));
            foreach (var f in new[] { footA, footB })
                kit.Block(new Vector3(f.x, (f.y + beamY) * 0.5f, f.z), new Vector3(0.6f, beamY - f.y, 0.6f), new Color(0.6f, 0.42f, 0.3f), "GatePost", wood);
            Vector3 a = new Vector3(footA.x, beamY, footA.z), b = new Vector3(footB.x, beamY, footB.z);
            kit.Rope(a, b, wood, 0.5f);
            Vector3 mid = (a + b) * 0.5f;
            kit.Deco(PrimitiveType.Sphere, a, Vector3.one * 0.7f, PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f));
            kit.Deco(PrimitiveType.Sphere, b, Vector3.one * 0.7f, PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f));
            var anchor = kit.Anchor(new Vector3(mid.x, lanternY, mid.z), range, LevelKit.AnchorStyle.Lantern, beamY - lanternY - 0.6f);
            return anchor;
        }

        // A flight of steps going up in direction yaw (bottom step starts at "bottom").
        public static void Stairs(LevelKit kit, Vector3 bottom, float yaw, int steps, float rise, float run, float width, Material mat)
        {
            var r = Rot(yaw);
            for (int i = 0; i < steps; i++)
            {
                float top = rise * (i + 1);
                Vector3 c = bottom + r * new Vector3(0f, 0f, run * (i + 0.5f)) + Vector3.up * (top * 0.5f);
                var s = kit.Block(c, RotSize(new Vector3(width, top, run), yaw), Stone, "Step", mat);
                s.rotation = r;
                s.localScale = new Vector3(width, top, run);
            }
        }

        // An arched wooden footbridge from a to b (both at deck height).
        public static void Bridge(LevelKit kit, Vector3 a, Vector3 b, float width, float arch)
        {
            Vector3 d = b - a;
            float len = new Vector2(d.x, d.z).magnitude;
            float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            var r = Rot(yaw);
            var planks = PatchworkAssets.Planks(new Color(0.72f, 0.52f, 0.36f));
            int segs = 8;
            for (int i = 0; i < segs; i++)
            {
                float k0 = i / (float)segs, k1 = (i + 1) / (float)segs;
                Vector3 p0 = Vector3.Lerp(a, b, k0) + Vector3.up * Mathf.Sin(k0 * Mathf.PI) * arch;
                Vector3 p1 = Vector3.Lerp(a, b, k1) + Vector3.up * Mathf.Sin(k1 * Mathf.PI) * arch;
                Vector3 mid = (p0 + p1) * 0.5f;
                float segLen = Vector3.Distance(p0, p1) + 0.05f;
                var deck = kit.Block(mid + Vector3.down * 0.15f, new Vector3(width, 0.3f, segLen), new Color(0.72f, 0.52f, 0.36f), "BridgeDeck", planks);
                deck.rotation = Quaternion.LookRotation((p1 - p0).normalized, Vector3.up);
                // Railings.
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 off = r * new Vector3(s * (width * 0.5f - 0.1f), 0f, 0f);
                    kit.Rope(p0 + off + Vector3.up * 1f, p1 + off + Vector3.up * 1f, PatchworkAssets.Wood(new Color(0.95f, 0.92f, 0.85f)), 0.12f);
                    kit.Deco(PrimitiveType.Cylinder, p0 + off + Vector3.up * 0.5f, new Vector3(0.14f, 0.5f, 0.14f), PatchworkAssets.Wood(new Color(0.95f, 0.92f, 0.85f)));
                }
            }
        }

        public static void Bench(LevelKit kit, Vector3 feet, float yaw)
        {
            var r = Rot(yaw);
            var wood = PatchworkAssets.Planks(new Color(0.6f, 0.4f, 0.28f));
            var iron = PatchworkAssets.Plain(Iron, 0.5f);
            kit.Deco(PrimitiveType.Cube, feet + r * new Vector3(0f, 0.5f, 0f), new Vector3(2.2f, 0.1f, 0.6f), wood, new Vector3(0f, yaw, 0f));
            kit.Deco(PrimitiveType.Cube, feet + r * new Vector3(0f, 0.95f, -0.28f), new Vector3(2.2f, 0.5f, 0.08f), wood, new Vector3(-10f, yaw, 0f));
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, feet + r * new Vector3(s * 0.95f, 0.35f, 0f), new Vector3(0.08f, 0.7f, 0.6f), iron, new Vector3(0f, yaw, 0f));
        }

        public static void FlowerBed(LevelKit kit, Vector3 center, Vector2 size, int seed)
        {
            kit.Deco(PrimitiveType.Cube, center + Vector3.up * 0.15f, new Vector3(size.x, 0.3f, size.y), PatchworkAssets.Felt(new Color(0.45f, 0.32f, 0.22f)));
            kit.Deco(PrimitiveType.Cube, center + Vector3.up * 0.2f, new Vector3(size.x + 0.2f, 0.4f, size.y + 0.2f), PatchworkAssets.Cobble(Stone));
            Color[] cols = { LevelKit.Palette.Rose, LevelKit.Palette.Butter, LevelKit.Palette.Lavender, LevelKit.Palette.Peach, new Color(1f, 0.55f, 0.6f) };
            int n = Mathf.Max(2, Mathf.RoundToInt(size.x * size.y / 1.2f));
            for (int i = 0; i < n; i++)
            {
                float x = ((i * 37 + seed * 11) % 100) / 100f - 0.5f, z = ((i * 61 + seed * 7) % 100) / 100f - 0.5f;
                kit.Flower(center + new Vector3(x * (size.x - 0.4f), 0.35f, z * (size.y - 0.4f)), cols[(i + seed) % cols.Length]);
            }
        }

        public static void Crate(LevelKit kit, Vector3 feet, float size, float yaw)
        {
            var c = kit.Block(feet + Vector3.up * size * 0.5f, Vector3.one * size, new Color(0.7f, 0.52f, 0.35f), "Crate", PatchworkAssets.Planks(new Color(0.75f, 0.56f, 0.38f)));
            c.rotation = Rot(yaw);
            var trim = PatchworkAssets.Wood(new Color(0.5f, 0.35f, 0.24f));
            var r = Rot(yaw);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, feet + Vector3.up * size * 0.5f + r * new Vector3(0f, 0f, s * size * 0.505f), new Vector3(size * 1.1f, 0.12f, 0.04f), trim, new Vector3(0f, yaw, 45f));
        }

        // A flower arch (for the wedding).
        public static void FlowerArch(LevelKit kit, Vector3 feet, float yaw, float width)
        {
            var r = Rot(yaw);
            var white = PatchworkAssets.Wood(new Color(0.97f, 0.95f, 0.9f));
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, feet + r * new Vector3(s * width * 0.5f, 1.4f, 0f), new Vector3(0.18f, 1.4f, 0.18f), white);
            Color[] cols = { LevelKit.Palette.Rose, new Color(1f, 0.85f, 0.88f), LevelKit.Palette.Butter, new Color(0.97f, 0.97f, 0.97f), LevelKit.Palette.Lavender };
            for (int i = 0; i <= 18; i++)
            {
                float a = i / 18f * Mathf.PI;
                Vector3 p = feet + r * new Vector3(Mathf.Cos(a) * width * 0.5f, 2.8f + Mathf.Sin(a) * width * 0.4f, 0f);
                kit.Deco(PrimitiveType.Sphere, p, Vector3.one * (0.35f + (i % 3) * 0.08f), PatchworkAssets.Felt(cols[i % cols.Length]));
                if (i % 2 == 0) kit.Deco(PrimitiveType.Sphere, p + r * new Vector3(0f, 0f, 0.15f), new Vector3(0.3f, 0.12f, 0.2f), PatchworkAssets.Felt(LevelKit.Palette.Sage));
            }
        }

        // A toy carousel horse on a pole, a lovely bit of set dressing for parks.
        public static void ToyCart(LevelKit kit, Vector3 feet, float yaw)
        {
            var r = Rot(yaw);
            var body = PatchworkAssets.Planks(new Color(0.8f, 0.3f, 0.3f));
            kit.Block(feet + Vector3.up * 0.9f, RotSize(new Vector3(2.4f, 0.8f, 1.4f), yaw), new Color(0.8f, 0.3f, 0.3f), "Cart", body);
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    kit.Deco(PrimitiveType.Cylinder, feet + r * new Vector3(sx * 0.8f, 0.45f, sz * 0.75f), new Vector3(0.8f, 0.06f, 0.8f), PatchworkAssets.Wood(new Color(0.95f, 0.85f, 0.4f)), new Vector3(0f, yaw, 90f));
            kit.Deco(PrimitiveType.Sphere, feet + Vector3.up * 1.5f + r * new Vector3(-0.5f, 0f, 0f), Vector3.one * 0.6f, PatchworkAssets.Felt(new Color(0.95f, 0.6f, 0.3f)));
            kit.Deco(PrimitiveType.Sphere, feet + Vector3.up * 1.45f + r * new Vector3(0.3f, 0f, 0.2f), Vector3.one * 0.5f, PatchworkAssets.Felt(new Color(0.6f, 0.8f, 0.4f)));
            kit.Deco(PrimitiveType.Sphere, feet + Vector3.up * 1.5f + r * new Vector3(0.5f, 0f, -0.3f), Vector3.one * 0.55f, PatchworkAssets.Felt(new Color(0.9f, 0.3f, 0.3f)));
        }
    }
}

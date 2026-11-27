using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The mountains round the edge of the toybox. Standing just inside each wall is a stage set
    // of painted cardboard flats, three deep, so the town seems to sit in a valley:
    //   - at the back, tall blue mountains with snow on their peaks (and, on the west side, a
    //     little painted castle on a far crag)
    //   - in the middle, teal foothills with a fringe of cut-out pine trees
    //   - at the front, low green hills
    // It still looks like a toybox up close: each flat has a cardboard edge you can see along
    // its top, strips of masking tape holding the joins, and little wooden stage braces at its
    // foot. There's a dip in the north range so the crack in the lid is always in view, and gaps
    // where the rivers and the ravine run under the walls.
    public static partial class Level1Builder
    {
        class FlatMesh
        {
            public readonly List<Vector3> V = new List<Vector3>();
            public readonly List<Vector2> UV = new List<Vector2>();
            public readonly List<int> T = new List<int>();
            readonly Vector3 face;
            public FlatMesh(Vector3 face) { this.face = face; }
            // A triangle, wound so it faces `face` (Unity's normal is cross(b - a, c - a)).
            public void Tri(Vector3 a, Vector3 b, Vector3 c)
            {
                int k = V.Count;
                V.Add(a); V.Add(b); V.Add(c);
                UV.Add(new Vector2(a.x + a.z, a.y) / 4f); UV.Add(new Vector2(b.x + b.z, b.y) / 4f); UV.Add(new Vector2(c.x + c.z, c.y) / 4f);
                if (Vector3.Dot(Vector3.Cross(b - a, c - a), face) >= 0f) { T.Add(k); T.Add(k + 1); T.Add(k + 2); }
                else { T.Add(k); T.Add(k + 2); T.Add(k + 1); }
            }
            public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d) { Tri(a, b, c); Tri(a, c, d); }
            public Transform Build(Transform parent, string name, Material mat, bool solid)
            {
                if (T.Count == 0) return null;
                var mesh = new Mesh { name = name };
                if (V.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.SetVertices(V);
                mesh.SetUVs(0, UV);
                mesh.SetTriangles(T, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                var go = new GameObject(name);
                go.transform.SetParent(parent, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = mat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;
                go.AddComponent<OwnedMesh>().Mesh = mesh;
                if (solid)
                {
                    go.AddComponent<MeshCollider>().sharedMesh = mesh;
                    go.AddComponent<NoClimb>();
                }
                return go.transform;
            }
        }

        // One range of flats along one wall. `ridge(a)` is its height at a (along the wall),
        // `snow` the height above which the peaks are white (0 for none).
        static void FlatRange(LevelKit kit, Transform parent, Vector3 wallPoint, Vector3 u, Vector3 n, float inset, System.Func<float, float> bottom,
            System.Func<float, float> ridge, List<Vector2> gaps, Color faceCol, float snow, string name, bool solid, System.Action<Vector3, float> onRidge)
        {
            var face = new FlatMesh(n);
            var rim = new FlatMesh(n);
            var top = new FlatMesh(Vector3.up);
            var cap = new FlatMesh(n);
            Vector3 o = wallPoint + n * inset;
            const float step = 1f, thick = 0.25f;
            Color rimCol = Color.Lerp(faceCol, Color.black, 0.22f);
            for (float a = -Half; a < Half - 0.01f; a += step)
            {
                float b = Mathf.Min(a + step, Half);
                bool gap = false;
                foreach (var g in gaps) if (b > g.x && a < g.y) gap = true;
                if (gap) continue;
                float ya = ridge(a), yb = ridge(b);
                Vector3 A0 = o + u * a + Vector3.up * bottom(a), B0 = o + u * b + Vector3.up * bottom(b);
                Vector3 A1 = o + u * a + Vector3.up * ya, B1 = o + u * b + Vector3.up * yb;
                face.Quad(A0, A1, B1, B0);
                // A painted outline just under the cut edge...
                Vector3 off = n * 0.02f;
                rim.Quad(A1 + off + Vector3.down * 0.35f, A1 + off, B1 + off, B1 + off + Vector3.down * 0.35f);
                // ...and the cardboard's own thickness along the top.
                top.Quad(A1, A1 - n * thick, B1 - n * thick, B1);
                // Snow on the peaks: down to a ragged line under the ridge.
                if (snow > 0f && (ya > snow || yb > snow))
                {
                    float la = Mathf.Min(ya, snow + 1.3f * Mathf.Sin(a * 0.9f) + 0.8f * Mathf.Sin(a * 2.3f));
                    float lb = Mathf.Min(yb, snow + 1.3f * Mathf.Sin(b * 0.9f) + 0.8f * Mathf.Sin(b * 2.3f));
                    Vector3 c = n * 0.04f;
                    cap.Quad(o + u * a + Vector3.up * la + c, A1 + c, B1 + c, o + u * b + Vector3.up * lb + c);
                }
                if (onRidge != null) onRidge(o + u * a, ya);
            }
            face.Build(parent, name, PatchworkAssets.Felt(faceCol), solid);
            rim.Build(parent, name + "Outline", PatchworkAssets.Felt(rimCol), false);
            top.Build(parent, name + "Edge", PatchworkAssets.Felt(new Color(0.78f, 0.62f, 0.45f)), false);
            if (snow > 0f) cap.Build(parent, name + "Snow", PatchworkAssets.Felt(new Color(0.97f, 0.97f, 1f)), false);
        }

        // Smooth peaky noise along a wall: a handful of mountains of different sizes.
        static System.Func<float, float> Peaks(int seed, float baseY, float minH, float maxH, float minW, float maxW, float spacing)
        {
            var rng = new System.Random(seed);
            var peaks = new List<Vector3>();   // (centre, height, half-width)
            for (float a = -Half - 10f; a < Half + 10f; a += spacing * (0.7f + (float)rng.NextDouble() * 0.6f))
                peaks.Add(new Vector3(a, minH + (float)rng.NextDouble() * (maxH - minH), minW + (float)rng.NextDouble() * (maxW - minW)));
            return a =>
            {
                float y = baseY + 1.2f * Mathf.Sin(a * 0.11f + seed) + 0.6f * Mathf.Sin(a * 0.37f + seed * 2f);
                foreach (var p in peaks)
                {
                    float d = Mathf.Abs(a - p.x) / p.z;
                    if (d >= 1f) continue;
                    // A pointy-ish peak: steep near the top, softer at the foot, with a little notch.
                    float h = p.y * Mathf.Pow(1f - d, 1.35f) * (1f - 0.06f * Mathf.Abs(Mathf.Sin(d * 9f)));
                    y = Mathf.Max(y, baseY + h);
                }
                return y;
            };
        }

        static void MountainBackdrop(LevelKit kit, Transform scenery)
        {
            var root = kit.Group("MountainBackdrop", Vector3.zero);
            root.SetParent(scenery, true);
            var pines = new MeshBatch();
            var tape = PatchworkAssets.Felt(new Color(0.95f, 0.9f, 0.74f));
            var brace = PatchworkAssets.Wood(new Color(0.68f, 0.5f, 0.34f));
            var rng = new System.Random(303);
            for (int wall = 0; wall < 4; wall++)
            {
                Vector3 u = wall < 2 ? Vector3.forward : Vector3.right;
                Vector3 n = wall == 0 ? Vector3.left : wall == 1 ? Vector3.right : wall == 2 ? Vector3.back : Vector3.forward;
                Vector3 wallPoint = -n * Half;
                // Where the front ranges stop: the rivers and the ravine going under the wall, and
                // the giant crayons leaning on the walls.
                var gaps = new List<Vector2>();
                if (wall == 0) { gaps.Add(new Vector2(-122f, -102f)); gaps.Add(new Vector2(-45f, -29f)); }
                if (wall == 2) { gaps.Add(new Vector2(RavineWest - 1f, RavineEast + 1f)); gaps.Add(new Vector2(62f, 82f)); gaps.Add(new Vector2(-25f, -1f)); }
                if (wall == 3) gaps.Add(new Vector2(RavineWest - 1f, RavineEast + 1f));
                var noGaps = new List<Vector2>();
                // The back range: tall, blue, snow-capped, dipping in the north so the lid's crack shows.
                var far = Peaks(11 + wall * 7, 20f, 6f, 17f, 16f, 30f, 26f);
                System.Func<float, float> farRidge = far;
                if (wall == 2) farRidge = a => Mathf.Lerp(far(a), 16f, Mathf.Exp(-(a * a) / (16f * 16f)));
                // (Over the ravine it stops short of the ground, so the brook's culvert still shows.)
                bool ravineWall = wall >= 2;
                System.Func<float, float> farFoot = a => ravineWall && a > RavineWest - 0.5f && a < RavineEast + 0.5f ? 1.4f : -5f;
                FlatRange(kit, root, wallPoint, u, n, 0.4f, farFoot, farRidge, noGaps, new Color(0.58f, 0.63f, 0.8f), 28f, "FarMountains", false, null);
                // The foothills, with a fringe of cut-out pines along their tops.
                int pineEvery = 0;
                FlatRange(kit, root, wallPoint, u, n, 1.8f, a => -1f, Peaks(5 + wall * 3, 10f, 3f, 8f, 14f, 26f, 18f), gaps, new Color(0.4f, 0.58f, 0.55f), 0f, "Foothills", false,
                    (p, y) =>
                    {
                        if (pineEvery++ % 3 != 0) return;
                        float s = 1.6f + (float)rng.NextDouble() * 1.4f;
                        var at = p + Vector3.up * (y - 0.4f) + n * 0.1f;
                        pines.Blade(at, s * 2.6f, s, Vector3.zero, Mathf.Atan2(n.x, n.z) * Mathf.Rad2Deg);
                        pines.Blade(at + Vector3.up * s * 0.9f, s * 2f, s * 0.8f, Vector3.zero, Mathf.Atan2(n.x, n.z) * Mathf.Rad2Deg);
                    });
                // The front hills: low and green, and solid (it's the edge of the world).
                float seed = wall * 1.7f;
                System.Func<float, float> near = a => 4.2f + 2.2f * Mathf.Sin(a / 11f + seed) + 1.2f * Mathf.Sin(a / 5.3f + seed * 2f) + 0.8f * Mathf.Sin(a / 2.1f);
                FlatRange(kit, root, wallPoint, u, n, 3f, a => -1f, near, gaps, new Color(0.55f, 0.74f, 0.5f), 0f, "FrontHills", true, null);
                // The toybox showing through: masking tape over the joins, and stage braces at the foot.
                for (float a = -Half + 12f; a < Half - 8f; a += 22f + (float)rng.NextDouble() * 14f)
                {
                    bool inGap = false;
                    foreach (var g in gaps) if (a > g.x - 2f && a < g.y + 2f) inGap = true;
                    if (inGap) continue;
                    Vector3 p = wallPoint + n * 3.03f + u * a;
                    float y = near(a);
                    float yaw = Mathf.Atan2(n.x, n.z) * Mathf.Rad2Deg;
                    kit.Deco(PrimitiveType.Cube, p + Vector3.up * (y - 0.2f), new Vector3(1.6f, 0.45f, 0.02f), tape, new Vector3(0f, yaw, -18f + (float)rng.NextDouble() * 36f), root);
                    kit.Deco(PrimitiveType.Cube, p + n * 0.45f + Vector3.up * 0.35f, new Vector3(0.3f, 0.7f, 0.9f), brace, new Vector3(-35f, yaw, 0f), root);
                }
            }
            pines.Build(root, "PaintedPines", PatchworkAssets.Felt(new Color(0.24f, 0.4f, 0.33f)));

            // A painted castle on a far crag on the west side: the next kingdom over.
            var castle = PatchworkAssets.Felt(new Color(0.44f, 0.48f, 0.66f));
            Vector3 cAt = new Vector3(-Half + 0.45f, 0f, 58f);
            var farWest = Peaks(11 + 1 * 7, 20f, 6f, 17f, 16f, 30f, 26f);
            float cy = farWest(58f);
            var cg = kit.Group("PaintedCastle", cAt + Vector3.up * cy);
            cg.SetParent(root, true);
            cg.rotation = Quaternion.Euler(0f, 90f, 0f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.2f, 0f), new Vector3(5f, 2.4f, 0.05f), castle, default(Vector3), cg);
            for (int k = -1; k <= 1; k++)
            {
                float th = k == 0 ? 5.2f : 3.6f;
                kit.Deco(PrimitiveType.Cube, new Vector3(k * 2.4f, th * 0.5f, 0.02f), new Vector3(1.1f, th, 0.05f), castle, default(Vector3), cg);
                kit.Deco(PrimitiveType.Cube, new Vector3(k * 2.4f, th + 0.5f, 0.02f), new Vector3(0.8f, 0.8f, 0.05f), castle, new Vector3(0f, 0f, 45f), cg);
            }
            kit.Deco(PrimitiveType.Cube, new Vector3(0.3f, 6.4f, 0.03f), new Vector3(0.6f, 0.35f, 0.04f), PatchworkAssets.Felt(new Color(0.82f, 0.3f, 0.3f)), default(Vector3), cg);
        }
    }
}

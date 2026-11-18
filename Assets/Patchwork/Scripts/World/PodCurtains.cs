using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The Pod's curtains: two soft velvet drapes under a scalloped pelmet across the top of the
    // window. Every time you come into the Pod (starting the game, or coming home from a level)
    // they're drawn shut over the glass. The first time you pick up the controller to choose a
    // level they sweep open, bunching into deep folds at the sides of the window, and are tied
    // back with gold rope; they stay open for the rest of your visit.
    //
    // Each drape is one cloth mesh following the curve of the window, with rounded folds rippling
    // across it (deeper and closer together as it gathers), a gold hem along the bottom, and a
    // back face so it shows from outside too.
    public class PodCurtains : MonoBehaviour
    {
        class Drape
        {
            public int Side;
            public Mesh Cloth, Hem;
        }

        readonly List<Drape> drapes = new List<Drape>();
        readonly List<GameObject> ties = new List<GameObject>();
        float t;            // 0 shut, 1 open
        bool opening;
        float edge, bottom, top;

        const int Across = 84, Down = 18, Folds = 9;

        // How far in from the glass they hang: in front of the wall's cardboard struts (which
        // stand 0.6 out from it) all the way down, and further in at the top (in front of the
        // felt piping over the window). The things on the desk are kept in front of them.
        internal static float Off(float y) { return Mathf.Lerp(0.72f, 0.95f, Mathf.InverseLerp(1.3f, 6.8f, y)); }
        static Vector3 Dir(float deg) { float a = deg * Mathf.Deg2Rad; return new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)); }
        static Vector3 On(float deg, float y, float inward) { return Dir(deg) * (HubBuilder.RadiusAt(y) - Off(y) - inward) + Vector3.up * y; }

        public static void Build(LevelKit kit, float halfAngle, float deskTop, float windowTop)
        {
            var root = kit.Group("Curtains", Vector3.zero);
            var c = root.gameObject.AddComponent<PodCurtains>();
            c.edge = halfAngle + 1.5f;
            c.bottom = deskTop + 0.1f;
            c.top = windowTop - 0.3f;
            var velvet = PatchworkAssets.Felt(new Color(0.72f, 0.24f, 0.36f));
            var gold = PatchworkAssets.Felt(new Color(1f, 0.8f, 0.36f));
            for (int s = -1; s <= 1; s += 2)
            {
                var d = new Drape { Side = s, Cloth = new Mesh { name = "Curtain" }, Hem = new Mesh { name = "CurtainHem" } };
                d.Cloth.MarkDynamic();
                d.Hem.MarkDynamic();
                Show(root, "Curtain", d.Cloth, velvet);
                Show(root, "CurtainHem", d.Hem, gold);
                c.drapes.Add(d);

                // The tie-back: a twist of gold rope round the gathered drape, with a tassel.
                float aTie = s * (c.edge - 2.6f);
                const float yTie = 3.6f;
                var tie = kit.Group("CurtainTie", On(aTie, yTie, 0.2f));
                tie.rotation = Quaternion.LookRotation(-Dir(aTie), Vector3.up);
                kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.05f, 0.05f, 0.62f), gold, default(Vector3), tie);
                kit.Deco(PrimitiveType.Sphere, new Vector3(-s * 0.45f, 0f, 0.28f), Vector3.one * 0.14f, gold, default(Vector3), tie);
                kit.Deco(PrimitiveType.Capsule, new Vector3(-s * 0.45f, -0.25f, 0.3f), new Vector3(0.12f, 0.18f, 0.12f), gold, default(Vector3), tie);
                tie.gameObject.SetActive(false);
                c.ties.Add(tie.gameObject);
            }
            c.Pelmet(kit, root);
            c.Apply();
        }

        static void Show(Transform root, string name, Mesh mesh, Material mat)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
        }

        // The pelmet: a stiff padded band across the top of the window, its bottom edge cut in
        // scallops with a pom-pom at the bottom of each, hiding where the drapes hang from.
        void Pelmet(LevelKit kit, Transform root)
        {
            var mat = PatchworkAssets.Felt(new Color(0.6f, 0.2f, 0.32f));
            var trim = PatchworkAssets.Felt(new Color(1f, 0.8f, 0.36f));
            const int n = 96, scallops = 14;
            float from = -edge - 2.5f, to = edge + 2.5f;
            float y1 = top + 0.18f, depth = 0.62f;
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            var trimVerts = new List<Vector3>();
            var trimTris = new List<int>();
            for (int i = 0; i <= n; i++)
            {
                float u = i / (float)n;
                float deg = Mathf.Lerp(from, to, u);
                float scallop = Mathf.Abs(Mathf.Sin(u * scallops * Mathf.PI));
                float y0 = top - 0.32f - scallop * 0.22f;
                verts.Add(On(deg, y1, depth));
                verts.Add(On(deg, y0, depth));
                uvs.Add(new Vector2(u * 12f, 1f));
                uvs.Add(new Vector2(u * 12f, 0f));
                trimVerts.Add(On(deg, y0 + 0.07f, depth + 0.012f));
                trimVerts.Add(On(deg, y0 - 0.01f, depth + 0.012f));
                if (i == n) continue;
                int k = i * 2;
                tris.AddRange(new[] { k, k + 2, k + 1, k + 1, k + 2, k + 3 });
                trimTris.AddRange(new[] { k, k + 2, k + 1, k + 1, k + 2, k + 3 });
            }
            // The pelmet's top: a shelf back to the wall, so it reads as a solid box.
            int topStart = verts.Count;
            for (int i = 0; i <= n; i++)
            {
                float deg = Mathf.Lerp(from, to, i / (float)n);
                verts.Add(On(deg, y1, depth));
                verts.Add(On(deg, y1, -0.15f));
                uvs.Add(new Vector2(i / (float)n * 12f, 0f));
                uvs.Add(new Vector2(i / (float)n * 12f, 1f));
                if (i == n) continue;
                int k = topStart + i * 2;
                tris.AddRange(new[] { k, k + 1, k + 2, k + 2, k + 1, k + 3 });
            }
            var mesh = new Mesh { name = "Pelmet" };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(DoubleSided(tris, verts, mesh), 0);
            Finish(mesh);
            Show(root, "Pelmet", mesh, mat);
            var tm = new Mesh { name = "PelmetTrim" };
            tm.SetVertices(trimVerts);
            tm.SetTriangles(DoubleSided(trimTris, trimVerts, tm), 0);
            Finish(tm);
            Show(root, "PelmetTrim", tm, trim);
            for (int k = 0; k < scallops; k++)
            {
                float deg = Mathf.Lerp(from, to, (k + 0.5f) / scallops);
                kit.Deco(PrimitiveType.Sphere, On(deg, top - 0.6f, depth + 0.04f), Vector3.one * 0.13f, trim, default(Vector3), root);
            }
        }

        // Adds the same triangles the other way round (with their own copies of the points), so
        // a sheet shows from both sides with its light right on each.
        static List<int> DoubleSided(List<int> tris, List<Vector3> verts, Mesh mesh)
        {
            int n = verts.Count;
            var uv = new List<Vector2>();
            mesh.GetUVs(0, uv);
            for (int i = 0; i < n; i++) { verts.Add(verts[i]); if (uv.Count > i) uv.Add(uv[i]); }
            var all = new List<int>(tris);
            for (int i = 0; i < tris.Count; i += 3) all.AddRange(new[] { tris[i] + n, tris[i + 2] + n, tris[i + 1] + n });
            mesh.SetVertices(verts);
            if (uv.Count == verts.Count) mesh.SetUVs(0, uv);
            return all;
        }

        static void Finish(Mesh m)
        {
            m.RecalculateNormals();
            m.RecalculateBounds();
        }

        // Rebuilds both drapes for how far open they are.
        void Apply()
        {
            float k = t * t * (3f - 2f * t);
            foreach (var d in drapes) Shape(d, k);
        }

        void Shape(Drape d, float k)
        {
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            var hem = new List<Vector3>();
            var hemTris = new List<int>();
            float gathered = 5.5f;   // how wide (in degrees) each drape is once bunched up
            for (int j = 0; j <= Down; j++)
            {
                float v = j / (float)Down;
                float y = Mathf.Lerp(bottom, top, v);
                for (int i = 0; i <= Across; i++)
                {
                    float u = i / (float)Across;   // 0 at the middle of the window, 1 at its edge
                    float shutA = -0.8f + u * (edge + 0.8f);
                    // Gathered: squeezed towards the edge, a little looser at the bottom (where
                    // the tie-back lets it flare) and pinched in at the tie.
                    float pinch = 1f - 0.45f * Mathf.Exp(-Mathf.Pow((y - 3.6f) / 0.9f, 2f));
                    float openA = edge - gathered * pinch * (1f - u) - 0.4f;
                    float a = d.Side * Mathf.Lerp(shutA, openA, k);
                    float folds = Folds * (1f + u * 0.15f);
                    float amp = Mathf.Lerp(0.13f, 0.28f, k) * Mathf.Lerp(0.85f, 1.15f, 1f - v);
                    float ripple = amp * (0.5f + 0.5f * Mathf.Sin(u * folds * Mathf.PI * 2f + v * 0.6f));
                    verts.Add(On(a, y, ripple));
                    uvs.Add(new Vector2(u * 9f, v * 4f));
                    if (j == 0)
                    {
                        hem.Add(On(a, y + 0.16f, ripple + 0.012f));
                        hem.Add(On(a, y - 0.01f, ripple + 0.012f));
                    }
                }
            }
            int row = Across + 1;
            for (int j = 0; j < Down; j++)
                for (int i = 0; i < Across; i++)
                {
                    int a0 = j * row + i, a1 = a0 + 1, b0 = a0 + row, b1 = b0 + 1;
                    if (d.Side > 0) tris.AddRange(new[] { a0, b0, a1, a1, b0, b1 });
                    else tris.AddRange(new[] { a0, a1, b0, a1, b1, b0 });
                }
            for (int i = 0; i < Across; i++)
            {
                int h = i * 2;
                if (d.Side > 0) hemTris.AddRange(new[] { h + 1, h, h + 3, h + 3, h, h + 2 });
                else hemTris.AddRange(new[] { h + 1, h + 3, h, h + 3, h + 2, h });
            }
            d.Cloth.Clear();
            d.Cloth.SetVertices(verts);
            d.Cloth.SetUVs(0, uvs);
            d.Cloth.SetTriangles(DoubleSided(tris, verts, d.Cloth), 0);
            Finish(d.Cloth);
            d.Hem.Clear();
            d.Hem.SetVertices(hem);
            d.Hem.SetTriangles(DoubleSided(hemTris, hem, d.Hem), 0);
            Finish(d.Hem);
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (!opening && game != null && game.Mode == UIMode.LevelSelect)
            {
                opening = true;
                Sfx.Play(SfxId.Whoosh, 0.35f);
            }
            if (!opening || t >= 1f) return;
            t = Mathf.MoveTowards(t, 1f, Time.unscaledDeltaTime / 1.4f);
            Apply();
            if (t >= 1f) foreach (var g in ties) if (g != null) g.SetActive(true);
        }
    }
}

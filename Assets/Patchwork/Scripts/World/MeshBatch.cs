using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Patchwork
{
    // Collects lots of little shapes (grass blades, pebbles, stitches, fence pickets...) into one
    // mesh, so a field of thousands of them is drawn in one go instead of one object each.
    // Everything is added in world space; Build() makes one renderer (no collider).
    public class MeshBatch
    {
        readonly List<Vector3> verts = new List<Vector3>();
        readonly List<Vector3> norms = new List<Vector3>();
        readonly List<Vector2> uvs = new List<Vector2>();
        readonly List<int> tris = new List<int>();

        void Tri(Vector3 a, Vector3 b, Vector3 c, Vector2 ua, Vector2 ub, Vector2 uc)
        {
            Vector3 n = Vector3.Cross(b - a, c - a);
            if (n.sqrMagnitude < 1e-12f) return;
            n.Normalize();
            int i = verts.Count;
            verts.Add(a); verts.Add(b); verts.Add(c);
            norms.Add(n); norms.Add(n); norms.Add(n);
            uvs.Add(ua); uvs.Add(ub); uvs.Add(uc);
            tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
        }

        void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float u0, float u1, float v0, float v1)
        {
            // a-b-c-d anticlockwise seen from the outside.
            Tri(a, c, b, new Vector2(u0, v0), new Vector2(u1, v1), new Vector2(u1, v0));
            Tri(a, d, c, new Vector2(u0, v0), new Vector2(u0, v1), new Vector2(u1, v1));
        }

        // A box (all six sides).
        public void Box(Vector3 center, Vector3 size, Quaternion rot)
        {
            Vector3 h = size * 0.5f;
            Vector3[] c = new Vector3[8];
            for (int i = 0; i < 8; i++)
                c[i] = center + rot * new Vector3((i & 1) == 0 ? -h.x : h.x, (i & 2) == 0 ? -h.y : h.y, (i & 4) == 0 ? -h.z : h.z);
            float ux = size.x, uy = size.y, uz = size.z;
            Quad(c[0], c[1], c[3], c[2], 0f, ux, 0f, uy);   // -z
            Quad(c[5], c[4], c[6], c[7], 0f, ux, 0f, uy);   // +z
            Quad(c[4], c[0], c[2], c[6], 0f, uz, 0f, uy);   // -x
            Quad(c[1], c[5], c[7], c[3], 0f, uz, 0f, uy);   // +x
            Quad(c[2], c[3], c[7], c[6], 0f, ux, 0f, uz);   // +y
            Quad(c[4], c[5], c[1], c[0], 0f, ux, 0f, uz);   // -y
        }

        // A thin three-sided blade from root to a tip that leans by "lean" (9 vertices).
        public void Blade(Vector3 root, float height, float width, Vector3 lean, float yaw)
        {
            var r = Quaternion.Euler(0f, yaw, 0f);
            Vector3 a = root + r * new Vector3(-width * 0.5f, 0f, -width * 0.29f);
            Vector3 b = root + r * new Vector3(width * 0.5f, 0f, -width * 0.29f);
            Vector3 c = root + r * new Vector3(0f, 0f, width * 0.58f);
            Vector3 tip = root + Vector3.up * height + lean;
            Tri(a, tip, b, new Vector2(0f, 0f), new Vector2(0.05f, 1f), new Vector2(0.1f, 0f));
            Tri(b, tip, c, new Vector2(0f, 0f), new Vector2(0.05f, 1f), new Vector2(0.1f, 0f));
            Tri(c, tip, a, new Vector2(0f, 0f), new Vector2(0.05f, 1f), new Vector2(0.1f, 0f));
        }

        // A low-poly ball (squashed by scale), for pebbles, pompoms and berries.
        public void Ball(Vector3 center, Vector3 scale, int rings = 4, int segs = 6)
        {
            for (int ri = 0; ri < rings; ri++)
            {
                float t0 = Mathf.PI * ri / rings, t1 = Mathf.PI * (ri + 1) / rings;
                for (int si = 0; si < segs; si++)
                {
                    float p0 = Mathf.PI * 2f * si / segs, p1 = Mathf.PI * 2f * (si + 1) / segs;
                    Vector3 a = Pt(center, scale, t0, p0), b = Pt(center, scale, t0, p1);
                    Vector3 c = Pt(center, scale, t1, p1), d = Pt(center, scale, t1, p0);
                    float u0 = si / (float)segs, u1 = (si + 1f) / segs, v0 = ri / (float)rings, v1 = (ri + 1f) / rings;
                    if (ri > 0) Tri(a, b, c, new Vector2(u0, v0), new Vector2(u1, v0), new Vector2(u1, v1));
                    if (ri < rings - 1) Tri(a, c, d, new Vector2(u0, v0), new Vector2(u1, v1), new Vector2(u0, v1));
                }
            }
        }

        static Vector3 Pt(Vector3 c, Vector3 s, float theta, float phi)
        {
            return c + new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi) * s.x * 0.5f, Mathf.Cos(theta) * s.y * 0.5f, Mathf.Sin(theta) * Mathf.Sin(phi) * s.z * 0.5f);
        }

        // An upright cylinder (with a top; the bottom is left off since it sits on something).
        public void Cylinder(Vector3 bottom, float radius, float height, int segs = 8)
        {
            Vector3 top = bottom + Vector3.up * height;
            for (int i = 0; i < segs; i++)
            {
                float a0 = Mathf.PI * 2f * i / segs, a1 = Mathf.PI * 2f * (i + 1) / segs;
                Vector3 o0 = new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)) * radius, o1 = new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * radius;
                Quad(bottom + o0, bottom + o1, top + o1, top + o0, i / (float)segs, (i + 1f) / segs, 0f, height);
                Tri(top, top + o1, top + o0, new Vector2(0.5f, 0.5f), new Vector2(0.5f + o1.x, 0.5f + o1.z), new Vector2(0.5f + o0.x, 0.5f + o0.z));
            }
        }

        // A run of little stitches along a line (each a tiny flat dash), like a seam.
        public void Stitches(Vector3 a, Vector3 b, float dash = 0.22f, float gap = 0.14f, float width = 0.05f, float thick = 0.02f)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            if (len < 0.01f) return;
            var rot = Quaternion.LookRotation(d / len, Vector3.up);
            for (float s = gap * 0.5f; s + dash <= len; s += dash + gap)
                Box(a + d / len * (s + dash * 0.5f), new Vector3(width, thick, dash), rot);
        }

        public Transform Build(Transform parent, string name, Material mat, bool castShadows = false)
        {
            if (verts.Count == 0) return null;
            var mesh = new Mesh();
            mesh.name = name;
            if (verts.Count > 65000) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            go.AddComponent<OwnedMesh>().Mesh = mesh;
            return go.transform;
        }
    }
}

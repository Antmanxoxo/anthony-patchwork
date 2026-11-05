using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Makes a built level cheaper to draw without losing anything you can actually see:
    //  - Small round things (Unity's spheres and capsules are 768 and 832 triangles each, even for
    //    a pea-sized eye) get a lighter mesh sized to how big they are. Big ones keep full detail.
    //  - Tiny things (under half a metre) stop casting shadows; their shadows were a few pixels.
    //  - Small and medium decorations are only drawn out to a sensible distance (well past where
    //    the haze softens them), so the camera isn't drawing thousands of far-off pebbles.
    // Runs once, right after a level is built.
    public static class RenderOptimizer
    {
        public const int SmallLayer = 8, MediumLayer = 9;
        static Mesh hiSphere, hiCapsule;
        static Mesh[] spheres, capsules;   // low, medium, high

        public static void Run(Transform root, Camera cam, float farClip, bool distanceCull)
        {
            Prepare();
            int swapped = 0, unshadowed = 0, layered = 0;
            var filters = root.GetComponentsInChildren<MeshFilter>(true);
            foreach (var mf in filters)
            {
                var mesh = mf.sharedMesh;
                if (mesh == null) continue;
                var r = mf.GetComponent<MeshRenderer>();
                if (r == null) continue;
                Vector3 s = Vector3.Scale(mesh.bounds.size, mf.transform.lossyScale);
                float size = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));

                if (mesh == hiSphere && size < 2.6f) { mf.sharedMesh = spheres[size < 0.45f ? 0 : size < 1.3f ? 1 : 2]; swapped++; }
                else if (mesh == hiCapsule && size < 3.2f) { mf.sharedMesh = capsules[size < 0.6f ? 0 : size < 1.6f ? 1 : 2]; swapped++; }

                if (size < 0.5f && r.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On)
                {
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    unshadowed++;
                }

                // Distance culling layers, only for plain decorations (nothing you can touch).
                var go = mf.gameObject;
                if (distanceCull && go.layer == 0 && go.GetComponent<Collider>() == null && go.GetComponent<Light>() == null)
                {
                    if (size < 0.8f) { go.layer = SmallLayer; layered++; }
                    else if (size < 3f) { go.layer = MediumLayer; layered++; }
                }
            }
            if (cam != null)
            {
                var d = new float[32];
                d[SmallLayer] = Mathf.Min(farClip, 80f);
                d[MediumLayer] = Mathf.Min(farClip, 180f);
                cam.layerCullDistances = d;
                cam.layerCullSpherical = true;
            }
            Debug.Log("Patchwork: render tidy-up - " + swapped + " lighter round meshes, " + unshadowed + " tiny shadows off, " + layered + " decorations distance-culled.");
        }

        static void Prepare()
        {
            if (spheres != null) return;
            var a = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var b = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hiSphere = a.GetComponent<MeshFilter>().sharedMesh;
            hiCapsule = b.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(a);
            Object.Destroy(b);
            spheres = new[] { Round("LowSphere", 10, 7, 0f), Round("MidSphere", 16, 10, 0f), Round("HighSphere", 20, 14, 0f) };
            capsules = new[] { Round("LowCapsule", 10, 8, 0.5f), Round("MidCapsule", 16, 10, 0.5f), Round("HighCapsule", 20, 14, 0.5f) };
        }

        // A UV sphere of radius 0.5 (stretch > 0 makes a capsule of total height 1 + 2 * stretch,
        // like Unity's: the top half moves up and the bottom half down).
        static Mesh Round(string name, int seg, int rings, float stretch)
        {
            if (rings % 2 == 1) rings++;
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            float totalH = 1f + stretch * 2f;
            for (int j = 0; j <= rings; j++)
            {
                float lat = Mathf.PI * j / rings;               // 0 at the top, pi at the bottom
                float y = Mathf.Cos(lat) * 0.5f, rr = Mathf.Sin(lat) * 0.5f;
                float off = j < rings / 2 ? stretch : j > rings / 2 ? -stretch : 0f;
                bool equator = stretch > 0f && j == rings / 2;
                int copies = equator ? 2 : 1;                   // a capsule's straight middle
                for (int c = 0; c < copies; c++)
                {
                    float oy = equator ? (c == 0 ? stretch : -stretch) : off;
                    for (int i = 0; i <= seg; i++)
                    {
                        float lon = 2f * Mathf.PI * i / seg;
                        var n = new Vector3(Mathf.Cos(lon) * Mathf.Sin(lat), Mathf.Cos(lat), Mathf.Sin(lon) * Mathf.Sin(lat));
                        verts.Add(new Vector3(Mathf.Cos(lon) * rr, y + oy, Mathf.Sin(lon) * rr));
                        normals.Add(n.normalized);
                        uvs.Add(new Vector2(1f - i / (float)seg, (y + oy + totalH * 0.5f) / totalH));
                    }
                }
            }
            int rowCount = verts.Count / (seg + 1);
            for (int j = 0; j < rowCount - 1; j++)
                for (int i = 0; i < seg; i++)
                {
                    int a = j * (seg + 1) + i, b = a + seg + 1;
                    tris.Add(a); tris.Add(a + 1); tris.Add(b);
                    tris.Add(a + 1); tris.Add(b + 1); tris.Add(b);
                }
            // Make sure the faces point outwards.
            for (int t = 0; t < tris.Count; t += 3)
            {
                Vector3 p0 = verts[tris[t]], p1 = verts[tris[t + 1]], p2 = verts[tris[t + 2]];
                Vector3 fn = Vector3.Cross(p1 - p0, p2 - p0);
                if (fn.sqrMagnitude < 1e-10f) continue;
                if (Vector3.Dot(fn, normals[tris[t]] + normals[tris[t + 1]] + normals[tris[t + 2]]) < 0f)
                    for (int k = 0; k < tris.Count; k += 3) { int tmp = tris[k + 1]; tris[k + 1] = tris[k + 2]; tris[k + 2] = tmp; }
                break;
            }
            var m = new Mesh { name = name };
            m.SetVertices(verts);
            m.SetNormals(normals);
            m.SetUVs(0, uvs);
            m.SetTriangles(tris, 0);
            m.RecalculateTangents();
            m.RecalculateBounds();
            m.UploadMeshData(false);
            return m;
        }
    }
}

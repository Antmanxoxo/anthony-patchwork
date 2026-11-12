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
        public const int SmallLayer = 8, MediumLayer = 9, GrassLayer = 10;
        static Mesh hiSphere, hiCapsule, hiCube, hiCylinder, cube, cylinder;
        static Mesh[] spheres, capsules;   // low, medium, high

        // Scales how far small and medium decorations are drawn (set by the Graphics setting).
        public static float CullScale = 1f;
        static Camera cullCamera;
        static float cullFar;

        static bool farView;

        // When on, big blocks wearing a tiling texture (brick, stone, plaster, planks, roof
        // tiles, wallpaper) get their texture laid on at real size, measured in the world, instead
        // of one copy stretched over each face: a 30 m wall gets 30 m of bricks, not eight giant
        // ones, and the courses line up from one block to the next.
        public static bool WorldTiling;
        static Dictionary<Texture, float> tileSize;

        // Cutscenes that fly over the whole level switch this on, so small things are drawn at
        // any distance for a while instead of popping in as the camera gets near.
        public static void SetFarView(bool on)
        {
            farView = on;
            ApplyCullDistances();
        }

        public static void ApplyCullDistances()
        {
            if (cullCamera == null) return;
            var d = new float[32];
            if (farView)
            {
                cullCamera.layerCullDistances = d;   // all 0: everything out to the far clip
                return;
            }
            d[SmallLayer] = Mathf.Min(cullFar, 80f * CullScale);
            d[MediumLayer] = Mathf.Min(cullFar, 180f * CullScale);
            d[GrassLayer] = Mathf.Min(cullFar, 70f * CullScale);
            cullCamera.layerCullDistances = d;
            cullCamera.layerCullSpherical = true;
        }

        public static void Run(Transform root, Camera cam, float farClip, bool distanceCull)
        {
            Prepare();
            farView = false;
            tileSize = null;
            int swapped = 0, unshadowed = 0, layered = 0, tiled = 0;
            var filters = root.GetComponentsInChildren<MeshFilter>(true);
            foreach (var mf in filters)
            {
                var mesh = mf.sharedMesh;
                if (mesh == null) continue;
                var r = mf.GetComponent<MeshRenderer>();
                if (r == null) continue;
                Vector3 s = Vector3.Scale(mesh.bounds.size, mf.transform.lossyScale);
                float size = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));

                // Our own copies of the cube and cylinder (same shape, but always mergeable).
                if (mesh == hiCube) { mf.sharedMesh = cube; mesh = cube; }
                else if (mesh == hiCylinder) { mf.sharedMesh = cylinder; mesh = cylinder; }
                if (WorldTiling && mesh == cube)
                {
                    float tile = TileFor(r.sharedMaterial);
                    if (tile > 0f)
                    {
                        // (Blocks already carry a material copy with its own repeat count baked in;
                        // undo that so the world-size repeat is the only one.)
                        Vector2 rep = r.sharedMaterial.GetTextureScale("_BaseMap");
                        if (rep.x < 0.01f || rep.y < 0.01f) rep = Vector2.one;
                        mf.sharedMesh = WorldCube(mf.transform, tile, rep);
                        tiled++;
                    }
                }
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
                cullCamera = cam;
                cullFar = farClip;
                ApplyCullDistances();
            }
            Debug.Log("Patchwork: render tidy-up - " + swapped + " lighter round meshes, " + unshadowed + " tiny shadows off, " + layered + " decorations distance-culled, " + tiled + " blocks tiled at real size.");
        }

        static void Prepare()
        {
            // Play starts without reloading scripts, so these can be left over from the last
            // Play press with the shapes themselves already thrown away by Unity. Anything given
            // a thrown-away shape draws nothing at all (that's how the whole Pod once vanished),
            // so check the shapes are really still there, not just that we made them once.
            if (spheres != null && capsules != null && cube != null && cylinder != null && hiCube != null
                && spheres[0] != null && spheres[1] != null && spheres[2] != null
                && capsules[0] != null && capsules[1] != null && capsules[2] != null) return;
            var a = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var b = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            var cu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var cy = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hiSphere = a.GetComponent<MeshFilter>().sharedMesh;
            hiCapsule = b.GetComponent<MeshFilter>().sharedMesh;
            hiCube = cu.GetComponent<MeshFilter>().sharedMesh;
            hiCylinder = cy.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(a);
            Object.Destroy(b);
            Object.Destroy(cu);
            Object.Destroy(cy);
            cube = MakeCube();
            cylinder = MakeCylinder(20);
            spheres = new[] { Round("LowSphere", 10, 7, 0f), Round("MidSphere", 16, 10, 0f), Round("HighSphere", 20, 14, 0f) };
            capsules = new[] { Round("LowCapsule", 10, 8, 0.5f), Round("MidCapsule", 16, 10, 0.5f), Round("HighCapsule", 20, 14, 0.5f) };
        }

        // How big one repeat of a material's texture is in the world (0 = leave it alone).
        static float TileFor(Material m)
        {
            if (m == null || !m.HasProperty("_BaseMap")) return 0f;
            var t = m.GetTexture("_BaseMap");
            if (t == null) return 0f;
            if (tileSize == null)
            {
                tileSize = new Dictionary<Texture, float>();
                PatchworkAssets.Brick(Color.white);   // makes sure the textures exist
                System.Action<PatchworkTextures.Pair, float> add = (pair, s) => { if (pair != null && pair.Albedo != null) tileSize[pair.Albedo] = s; };
                add(PatchworkTextures.Brick, 3f);     // 8 courses a repeat: stone blocks about 0.4 m tall
                add(PatchworkTextures.Cobble, 3f);
                add(PatchworkTextures.Plaster, 4f);
                add(PatchworkTextures.Planks, 1.6f);
                add(PatchworkTextures.Shingle, 2f);
                add(PatchworkTextures.Damask, 2.4f);
            }
            float s2;
            return tileSize.TryGetValue(t, out s2) ? s2 : 0f;
        }

        // A copy of the cube whose UVs come from where each corner really is in the world:
        // tops and bottoms map x and z, the sides map along the wall and up.
        static Mesh WorldCube(Transform t, float tile, Vector2 rep)
        {
            var verts = cube.vertices;
            var normals = cube.normals;
            var uvs = new Vector2[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 w = t.TransformPoint(verts[i]);
                Vector3 n = t.TransformDirection(normals[i]);
                Vector2 uv;
                if (Mathf.Abs(n.y) > 0.7f) uv = new Vector2(w.x, w.z);
                else if (Mathf.Abs(n.x) > Mathf.Abs(n.z)) uv = new Vector2(n.x > 0f ? -w.z : w.z, w.y);
                else uv = new Vector2(n.z > 0f ? w.x : -w.x, w.y);
                uvs[i] = new Vector2(uv.x / (tile * rep.x), uv.y / (tile * rep.y));
            }
            var m = new Mesh { name = "PatchworkTiledCube" };
            m.vertices = verts;
            m.normals = normals;
            m.uv = uvs;
            m.triangles = cube.triangles;
            m.RecalculateTangents();
            m.RecalculateBounds();
            m.UploadMeshData(false);
            return m;
        }

        // A unit cube like Unity's (each face its own 0..1 UVs).
        static Mesh MakeCube()
        {
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            Vector3[] n = { Vector3.forward, Vector3.back, Vector3.up, Vector3.down, Vector3.right, Vector3.left };
            foreach (var f in n)
            {
                Vector3 u = Mathf.Abs(f.y) > 0.5f ? Vector3.right : Vector3.Cross(Vector3.up, f);
                Vector3 v = Vector3.Cross(f, u);
                int i = verts.Count;
                for (int k = 0; k < 4; k++)
                {
                    float su = (k == 1 || k == 2) ? 0.5f : -0.5f, sv = k >= 2 ? 0.5f : -0.5f;
                    verts.Add(f * 0.5f + u * su + v * sv);
                    normals.Add(f);
                    uvs.Add(new Vector2(su + 0.5f, sv + 0.5f));
                }
                tris.Add(i); tris.Add(i + 2); tris.Add(i + 1);
                tris.Add(i); tris.Add(i + 3); tris.Add(i + 2);
            }
            return Finish("PatchworkCube", verts, normals, uvs, tris);
        }

        // A cylinder like Unity's: radius 0.5, from y -1 to 1, smooth sides and flat caps.
        static Mesh MakeCylinder(int seg)
        {
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            for (int i = 0; i <= seg; i++)
            {
                float a = Mathf.PI * 2f * i / seg;
                var d = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                verts.Add(d * 0.5f + Vector3.down); normals.Add(d); uvs.Add(new Vector2(i / (float)seg, 0f));
                verts.Add(d * 0.5f + Vector3.up); normals.Add(d); uvs.Add(new Vector2(i / (float)seg, 1f));
            }
            for (int i = 0; i < seg; i++)
            {
                int b0 = i * 2, t0 = b0 + 1, b1 = b0 + 2, t1 = b0 + 3;
                tris.Add(b0); tris.Add(t0); tris.Add(t1);
                tris.Add(b0); tris.Add(t1); tris.Add(b1);
            }
            for (int cap = 0; cap < 2; cap++)
            {
                float y = cap == 0 ? 1f : -1f;
                var nrm = cap == 0 ? Vector3.up : Vector3.down;
                int centre = verts.Count;
                verts.Add(new Vector3(0f, y, 0f)); normals.Add(nrm); uvs.Add(new Vector2(0.5f, 0.5f));
                for (int i = 0; i <= seg; i++)
                {
                    float a = Mathf.PI * 2f * i / seg;
                    verts.Add(new Vector3(Mathf.Cos(a) * 0.5f, y, Mathf.Sin(a) * 0.5f)); normals.Add(nrm);
                    uvs.Add(new Vector2(Mathf.Cos(a) * 0.5f + 0.5f, Mathf.Sin(a) * 0.5f + 0.5f));
                }
                for (int i = 0; i < seg; i++)
                {
                    if (cap == 0) { tris.Add(centre); tris.Add(centre + i + 2); tris.Add(centre + i + 1); }
                    else { tris.Add(centre); tris.Add(centre + i + 1); tris.Add(centre + i + 2); }
                }
            }
            return Finish("PatchworkCylinder", verts, normals, uvs, tris);
        }

        // Builds the mesh and makes sure every face points outwards (matching its normals).
        static Mesh Finish(string name, List<Vector3> verts, List<Vector3> normals, List<Vector2> uvs, List<int> tris)
        {
            for (int t = 0; t < tris.Count; t += 3)
            {
                Vector3 p0 = verts[tris[t]], p1 = verts[tris[t + 1]], p2 = verts[tris[t + 2]];
                Vector3 fn = Vector3.Cross(p1 - p0, p2 - p0);
                if (Vector3.Dot(fn, normals[tris[t]] + normals[tris[t + 1]] + normals[tris[t + 2]]) < 0f) { int tmp = tris[t + 1]; tris[t + 1] = tris[t + 2]; tris[t + 2] = tmp; }
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

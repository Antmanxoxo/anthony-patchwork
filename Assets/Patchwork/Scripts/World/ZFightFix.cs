using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Flickering surfaces ("z-fighting") happen when two solid things have a face in exactly
    // the same place: a rug the same height as another rug, a strip of trim flush with the
    // front of a wall, two floor slabs overlapping. The graphics card can't tell which is in
    // front, so they shimmer. This pass, run once after a level is built, finds those pairs
    // among the level's blocks and discs and nudges the smaller one out by a couple of
    // centimetres, which is too little to see but plenty to settle the flicker.
    public static class ZFightFix
    {
        const float Same = 0.006f;     // faces closer than this count as "in the same place"
        const float Nudge = 0.02f;
        const float MinOverlap = 0.004f;   // and they must overlap by at least this much area

        struct Face
        {
            public Transform T;
            public int Axis, Side;      // which way the face looks: axis 0/1/2 = x/y/z, side +1/-1
            public float Plane;         // where the face is along that axis
            public Vector2 Min, Max;    // its extent along the other two axes
            public float Area;
        }

        public static int Run(Transform root)
        {
            var faces = new Dictionary<long, List<Face>>();
            var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var r in renderers)
            {
                if (r == null || !r.enabled) continue;
                var mf = r.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;
                var mat = r.sharedMaterial;
                if (mat == null || mat.renderQueue >= 2500) continue;   // see-through things sort differently
                string mesh = mf.sharedMesh.name;
                bool cube = mesh == "Cube", cyl = mesh == "Cylinder";
                if (!cube && !cyl) continue;
                var t = r.transform;
                if (!Aligned(t, cyl)) continue;
                Bounds b = r.bounds;
                for (int axis = 0; axis < 3; axis++)
                {
                    // A disc (cylinder) only has flat faces top and bottom.
                    if (cyl && axis != 1) continue;
                    for (int side = -1; side <= 1; side += 2)
                    {
                        var f = new Face { T = t, Axis = axis, Side = side };
                        f.Plane = side > 0 ? b.max[axis] : b.min[axis];
                        int a1 = (axis + 1) % 3, a2 = (axis + 2) % 3;
                        f.Min = new Vector2(b.min[a1], b.min[a2]);
                        f.Max = new Vector2(b.max[a1], b.max[a2]);
                        f.Area = (f.Max.x - f.Min.x) * (f.Max.y - f.Min.y);
                        if (cyl) { f.Min += (f.Max - f.Min) * 0.15f; f.Max -= (f.Max - f.Min) * 0.15f; }   // roughly the disc, not its corners
                        long key = Key(axis, side, Mathf.RoundToInt(f.Plane / Same));
                        List<Face> list;
                        if (!faces.TryGetValue(key, out list)) faces[key] = list = new List<Face>();
                        list.Add(f);
                    }
                }
            }

            var moved = new HashSet<Transform>();
            int fixedCount = 0;
            foreach (var kv in faces)
            {
                var list = kv.Value;
                long baseKey = kv.Key;
                // Compare with the faces in this bucket and the next one up (so a pair that falls
                // either side of a bucket edge is still found, without counting it twice).
                for (int pass = 0; pass < 2; pass++)
                {
                    List<Face> other = list;
                    if (pass == 1 && !faces.TryGetValue(baseKey + 8, out other)) continue;   // the same axis and side, one bucket up
                    for (int i = 0; i < list.Count; i++)
                        for (int j = pass == 0 ? i + 1 : 0; j < other.Count; j++)
                        {
                            Face a = list[i], c = other[j];
                            if (a.T == c.T || Mathf.Abs(a.Plane - c.Plane) > Same) continue;
                            float ox = Mathf.Min(a.Max.x, c.Max.x) - Mathf.Max(a.Min.x, c.Min.x);
                            float oy = Mathf.Min(a.Max.y, c.Max.y) - Mathf.Max(a.Min.y, c.Min.y);
                            if (ox <= 0.02f || oy <= 0.02f || ox * oy < MinOverlap) continue;
                            // Move the smaller one outward (towards where its face looks).
                            Face small = a.Area <= c.Area ? a : c;
                            if (moved.Contains(small.T)) continue;
                            Vector3 dir = Vector3.zero;
                            dir[small.Axis] = small.Side;
                            small.T.position += dir * Nudge;
                            moved.Add(small.T);
                            fixedCount++;
                        }
                }
            }
            return fixedCount;
        }

        static long Key(int axis, int side, int cell)
        {
            return ((long)cell << 3) | (long)(axis * 2 + (side > 0 ? 1 : 0));
        }

        // Is it lined up with the world (so its box is its shape)? Cubes may be turned in steps
        // of 90 degrees; discs must stand upright.
        static bool Aligned(Transform t, bool cylinder)
        {
            Vector3 up = t.up, right = t.right, fwd = t.forward;
            if (cylinder) return Mathf.Abs(up.y) > 0.9995f;
            return Axis(up) && Axis(right) && Axis(fwd);
        }

        static bool Axis(Vector3 v)
        {
            return Mathf.Abs(v.x) > 0.9995f || Mathf.Abs(v.y) > 0.9995f || Mathf.Abs(v.z) > 0.9995f;
        }
    }
}

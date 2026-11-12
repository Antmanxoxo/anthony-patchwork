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
            public int Frame;           // the turn it's measured in (half degrees)
            public float Plane;         // where the face is along that axis
            public Vector2 Min, Max;    // its extent along the other two axes
            public float Area;
        }

        // Faces are compared inside a "frame": tops and bottoms all share one (they're all
        // level), while the sides of a block turned to some angle are compared with the sides
        // of other blocks turned the same way.
        public static int Run(Transform root)
        {
            var faces = new Dictionary<long, List<Face>>();
            var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            var corner = new Vector3[8];
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
                if (Mathf.Abs(t.up.y) < 0.9995f) continue;   // only things standing level
                // The block's own turn, folded into 0..90 (a block turned 90 is the same shape).
                float yaw = Mathf.Repeat(t.eulerAngles.y, 90f);
                if (yaw > 89.75f) yaw = 0f;
                int frame = cube ? Mathf.RoundToInt(yaw * 2f) : 0;   // half-degree steps
                Quaternion inv = Quaternion.Euler(0f, -frame * 0.5f, 0f);
                // Its box in that frame (exact for a cube; near enough for a disc's top).
                Bounds mb = mf.sharedMesh.bounds;
                int n = 0;
                for (int ix = 0; ix < 2; ix++)
                    for (int iy = 0; iy < 2; iy++)
                        for (int iz = 0; iz < 2; iz++)
                            corner[n++] = inv * t.TransformPoint(new Vector3(ix == 0 ? mb.min.x : mb.max.x, iy == 0 ? mb.min.y : mb.max.y, iz == 0 ? mb.min.z : mb.max.z));
                Vector3 lo = corner[0], hi = corner[0];
                for (int i = 1; i < 8; i++) { lo = Vector3.Min(lo, corner[i]); hi = Vector3.Max(hi, corner[i]); }
                for (int axis = 0; axis < 3; axis++)
                {
                    if (cyl && axis != 1) continue;   // a disc only has flat faces top and bottom
                    int fr = axis == 1 ? -1 : frame;    // tops and bottoms all compare together
                    for (int side = -1; side <= 1; side += 2)
                    {
                        var f = new Face { T = t, Axis = axis, Side = side, Frame = frame };
                        f.Plane = side > 0 ? hi[axis] : lo[axis];
                        int a1 = (axis + 1) % 3, a2 = (axis + 2) % 3;
                        if (axis == 1)
                        {
                            // Level faces: compare in world space (a rough box is fine here).
                            Bounds wb = r.bounds;
                            f.Min = new Vector2(wb.min[a1], wb.min[a2]);
                            f.Max = new Vector2(wb.max[a1], wb.max[a2]);
                        }
                        else
                        {
                            f.Min = new Vector2(lo[a1], lo[a2]);
                            f.Max = new Vector2(hi[a1], hi[a2]);
                        }
                        f.Area = (f.Max.x - f.Min.x) * (f.Max.y - f.Min.y);
                        if (cyl || (axis == 1 && frame != 0)) { Vector2 sz = f.Max - f.Min; f.Min += sz * 0.15f; f.Max -= sz * 0.15f; }
                        long key = Key(fr, axis, side, Mathf.RoundToInt(f.Plane / Same));
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
                if (list.Count == 0) continue;
                Face first = list[0];
                int fr0 = first.Axis == 1 ? -1 : first.Frame;
                int cell = Mathf.RoundToInt(first.Plane / Same);
                for (int pass = 0; pass < 2; pass++)
                {
                    List<Face> other = list;
                    if (pass == 1 && !faces.TryGetValue(Key(fr0, first.Axis, first.Side, cell + 1), out other)) continue;
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
                            if (small.Axis != 1) dir = Quaternion.Euler(0f, small.Frame * 0.5f, 0f) * dir;
                            small.T.position += dir * Nudge;
                            moved.Add(small.T);
                            fixedCount++;
                        }
                }
            }
            return fixedCount;
        }

        // See-through things are drawn back to front by their middles, so two big ones in the
        // same place (glass and the water behind it, a dome and the jellyfish round it) keep
        // swapping as you move: flicker. Give them a fixed order instead: glass first, then
        // everything else, then light beams.
        public static void OrderSeeThrough(Transform root)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var m = r.sharedMaterial;
                if (m == null || m.renderQueue < 2500 || r.sortingOrder != 0) continue;
                if (m.name.StartsWith("PW_glass")) r.sortingOrder = -20;
                else if (m.name.StartsWith("PW_beam")) r.sortingOrder = 20;
            }
        }

        static long Key(int frame, int axis, int side, int cell)
        {
            // cell in the high bits, then the frame (-1..180), then axis and side.
            return ((long)cell << 12) | ((long)(frame + 1) << 3) | (long)(axis * 2 + (side > 0 ? 1 : 0));
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

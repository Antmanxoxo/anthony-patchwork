using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Runs once a level is built, before anything is merged: different parts of the level are
    // built by different code (the town, the wood, the outskirts, the corners) and sometimes
    // they put things in the same spot. This tidies that up:
    // - two trees growing right into each other: the smaller one goes
    // - a small prop (a mushroom, a pot, a sign, a yarn ball...) inside a tree trunk, inside
    //   another small prop, or sunk into something bigger (a tent, a gate): it's moved just clear
    // It only touches plain decorations (nothing you can use, collect or that runs a game rule),
    // never moves anything more than 2 m, and leaves things sitting on top of other things alone.
    public static class Declutter
    {
        const float MaxMove = 2f;

        static readonly HashSet<string> Trees = new HashSet<string>
        { "SpookyPine", "PomTree", "Birch", "GnarledTree", "ButtonTree", "YarnTree", "GiantOak", "HollowOak", "WishingWillow" };
        // Special trees that are landmarks: never removed (others move out of their way instead).
        static readonly HashSet<string> Landmarks = new HashSet<string> { "GiantOak", "HollowOak", "WishingWillow" };
        // Little filler pieces: when two things overlap, these are the ones that move.
        static readonly HashSet<string> Fillers = new HashSet<string>
        { "Fern", "Mushroom", "MossyRock", "Stump", "FlowerPot", "LanternPost", "Signpost", "Crystals", "Barrel", "Lantern" };

        public static bool IsTree(string name) { return Trees.Contains(name); }

        class Thing
        {
            public Transform T;
            public Bounds B;
            public bool Tree, Pure, Filler;
            public float Radius;     // rough round footprint
            public float Size => B.size.x * B.size.y * B.size.z;
        }

        public static void Run(Transform root)
        {
            if (root == null) return;
            var things = new List<Thing>();
            for (int i = 0; i < root.childCount; i++)
            {
                var t = root.GetChild(i);
                if (!t.gameObject.activeSelf || t.GetComponent<MeshFilter>() != null) continue;
                Bounds b;
                if (!MeshBounds(t, out b)) continue;
                float foot = Mathf.Max(b.size.x, b.size.z);
                if (foot > 12f || b.size.y < 0.25f) continue;          // areas, paths and flat patches
                var th = new Thing { T = t, B = b, Tree = Trees.Contains(t.name), Pure = Pure(t), Filler = Fillers.Contains(t.name) };
                th.Radius = foot * 0.48f;   // most props are round-ish (balls, caps, pots)
                things.Add(th);
            }
            var trees = things.FindAll(x => x.Tree);
            int removed = 0, moved = 0;
            // Many props aren't solid themselves: an invisible box (a "Solid", see TownKit.Solid)
            // was put down beside them. When a prop is moved or taken out, its box has to go with
            // it, or it's left behind as an invisible wall (a gate you can't walk through, a tree
            // that isn't there but still stops you).
            var boxes = new List<Transform>();
            for (int i = 0; i < root.childCount; i++)
            {
                var t = root.GetChild(i);
                if (t.name == "Solid" && t.GetComponent<Collider>() != null && t.GetComponent<Renderer>() == null) boxes.Add(t);
            }

            // 1. Trees growing into each other.
            for (int i = 0; i < trees.Count; i++)
                for (int j = i + 1; j < trees.Count; j++)
                {
                    Thing a = trees[i], b = trees[j];
                    if (!a.T.gameObject.activeSelf || !b.T.gameObject.activeSelf) continue;
                    float ca = Mathf.Max(a.B.size.x, a.B.size.z) * 0.5f, cb = Mathf.Max(b.B.size.x, b.B.size.z) * 0.5f;
                    if (Flat(a.T.position - b.T.position) >= (ca + cb) * 0.55f) continue;
                    Thing go = a.Size < b.Size ? a : b;
                    if (Landmarks.Contains(go.T.name) || !go.Pure) go = go == a ? b : a;
                    if (Landmarks.Contains(go.T.name) || !go.Pure) continue;
                    go.T.gameObject.SetActive(false);
                    // (Only its trunk's box: the canopy may well spread over other things' boxes.)
                    foreach (var box in BoxesOf(go, boxes))
                        if (Flat(box.position - go.T.position) < Mathf.Max(1f, Mathf.Max(go.B.size.x, go.B.size.z) * 0.15f)) box.gameObject.SetActive(false);
                    removed++;
                }

            // 2. Small props clear of trunks, of bigger things and of each other.
            var small = things.FindAll(x => !x.Tree && x.Pure && Mathf.Max(x.B.size.x, x.B.size.z) < 4f && x.B.size.y < 5f);
            var solid = things.FindAll(x => !x.Tree && Mathf.Max(x.B.size.x, x.B.size.z) >= 4f && Mathf.Max(x.B.size.x, x.B.size.z) <= 9f
                                            && x.T.GetComponentInChildren<Collider>() != null);
            for (int pass = 0; pass < 3; pass++)
                foreach (var s in small)
                {
                    if (!s.T.gameObject.activeSelf) continue;
                    Vector3 push = Vector3.zero;
                    // Tree trunks (gnarled trees have big roots).
                    foreach (var tr in trees)
                    {
                        if (!tr.T.gameObject.activeSelf) continue;
                        float trunk = Mathf.Max(tr.B.size.x, tr.B.size.z) * (tr.T.name == "GnarledTree" ? 0.3f : 0.14f);
                        Vector3 d = Flat3(s.B.center - tr.T.position);
                        float need = trunk + s.Radius;
                        if (d.magnitude < need) push += Away(d, s) * (need - d.magnitude + 0.05f);
                    }
                    // Bigger solid things: out of their box, unless sitting on top.
                    foreach (var o in solid)
                    {
                        if (!o.T.gameObject.activeSelf || s.B.min.y >= o.B.max.y - 0.35f || o.B.min.y >= s.B.max.y - 0.2f) continue;
                        var box = o.B;
                        box.Expand(new Vector3(s.Radius * 2f, 0f, s.Radius * 2f));
                        Vector3 c = s.B.center;
                        if (c.x <= box.min.x || c.x >= box.max.x || c.z <= box.min.z || c.z >= box.max.z) continue;
                        float l = c.x - box.min.x, r = box.max.x - c.x, bk = c.z - box.min.z, f = box.max.z - c.z;
                        float m = Mathf.Min(Mathf.Min(l, r), Mathf.Min(bk, f));
                        push += m == l ? new Vector3(-l, 0f, 0f) : m == r ? new Vector3(r, 0f, 0f) : m == bk ? new Vector3(0f, 0f, -bk) : new Vector3(0f, 0f, f);
                    }
                    // Each other: the filler (or the smaller one) gives way.
                    foreach (var o in small)
                    {
                        if (o == s || !o.T.gameObject.activeSelf) continue;
                        if (s.B.min.y >= o.B.center.y || o.B.min.y >= s.B.center.y) continue;     // stacked on purpose
                        bool sGives = s.Filler != o.Filler ? s.Filler : s.Size < o.Size || (s.Size == o.Size && s.T.GetSiblingIndex() < o.T.GetSiblingIndex());
                        if (!sGives) continue;
                        Vector3 d = Flat3(s.B.center - o.B.center);
                        float need = s.Radius + o.Radius;
                        if (d.magnitude < need) push += Away(d, s) * (need - d.magnitude + 0.05f);
                    }
                    if (push.sqrMagnitude < 0.0004f) continue;
                    if (push.magnitude > MaxMove) push = push.normalized * MaxMove;
                    foreach (var box in BoxesOf(s, boxes)) box.position += push;
                    s.T.position += push;
                    s.B.center += push;
                    moved++;
                }
            int orphans = RemoveOrphanBoxes(root, boxes);
            if (removed > 0 || moved > 0 || orphans > 0)
                Debug.Log("Patchwork: tidied the level (" + removed + " crowded trees taken out, " + moved + " nudges to keep props out of each other, "
                    + orphans + " invisible walls with nothing there taken away).");
        }

        // A box that makes a prop solid, with no prop where it stands (the prop was never built
        // there, was taken out, or was moved by something else): an invisible wall. Take it away.
        static int RemoveOrphanBoxes(Transform root, List<Transform> boxes)
        {
            const float Cell = 8f;
            var grid = new Dictionary<long, List<Bounds>>();
            var big = new List<Bounds>();     // batched meshes spread over a big area (checked for every box)
            foreach (var r in root.GetComponentsInChildren<MeshRenderer>())
            {
                if (!r.enabled) continue;
                var b = r.bounds;
                if (b.size.x > 20f && b.size.z > 20f && b.size.y < 1f) continue;   // the ground, paths and rugs
                int x0 = Mathf.FloorToInt(b.min.x / Cell), x1 = Mathf.FloorToInt(b.max.x / Cell);
                int z0 = Mathf.FloorToInt(b.min.z / Cell), z1 = Mathf.FloorToInt(b.max.z / Cell);
                if ((x1 - x0 + 1) * (z1 - z0 + 1) > 16) { big.Add(b); continue; }
                for (int x = x0; x <= x1; x++)
                    for (int z = z0; z <= z1; z++)
                    {
                        long key = ((long)x << 32) ^ (uint)z;
                        List<Bounds> list;
                        if (!grid.TryGetValue(key, out list)) grid[key] = list = new List<Bounds>();
                        list.Add(b);
                    }
            }
            int gone = 0;
            foreach (var box in boxes)
            {
                if (box == null || !box.gameObject.activeSelf) continue;
                var col = box.GetComponent<Collider>();
                if (col == null) continue;
                var test = col.bounds;
                // Only prop-sized boxes (the big ones are walls and floors that are meant to be
                // invisible, like the edges of the toybox).
                if (Mathf.Max(test.size.x, test.size.z) > 6f) continue;
                test.Expand(0.2f);
                bool seen = false;
                foreach (var b in big) if (b.Intersects(test)) { seen = true; break; }
                int x0 = Mathf.FloorToInt(test.min.x / Cell), x1 = Mathf.FloorToInt(test.max.x / Cell);
                int z0 = Mathf.FloorToInt(test.min.z / Cell), z1 = Mathf.FloorToInt(test.max.z / Cell);
                for (int x = x0; x <= x1 && !seen; x++)
                    for (int z = z0; z <= z1 && !seen; z++)
                    {
                        List<Bounds> list;
                        if (!grid.TryGetValue(((long)x << 32) ^ (uint)z, out list)) continue;
                        foreach (var b in list) if (b.Intersects(test)) { seen = true; break; }
                    }
                if (seen) continue;
                box.gameObject.SetActive(false);
                gone++;
            }
            return gone;
        }

        // The invisible boxes standing inside a prop's footprint (its own, that is).
        static List<Transform> BoxesOf(Thing th, List<Transform> boxes)
        {
            var mine = new List<Transform>();
            var b = th.B;
            foreach (var box in boxes)
            {
                if (box == null || !box.gameObject.activeSelf) continue;
                Vector3 p = box.position;
                if (p.x < b.min.x || p.x > b.max.x || p.z < b.min.z || p.z > b.max.z || p.y < b.min.y - 0.5f || p.y > b.max.y + 0.5f) continue;
                mine.Add(box);
            }
            return mine;
        }

        // Plain decoration: nothing on it but spinning, swaying or blinking.
        static bool Pure(Transform t)
        {
            foreach (var mb in t.GetComponentsInChildren<MonoBehaviour>(true))
                if (!(mb is Spinner) && !(mb is Rocker) && !(mb is Blinker)) return false;
            return true;
        }

        static Vector3 Away(Vector3 d, Thing s)
        {
            if (d.sqrMagnitude > 0.0001f) return d.normalized;
            float a = (s.T.GetSiblingIndex() * 137.5f) * Mathf.Deg2Rad;    // right on top: any direction
            return new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
        }

        static float Flat(Vector3 v) { return new Vector2(v.x, v.z).magnitude; }
        static Vector3 Flat3(Vector3 v) { return new Vector3(v.x, 0f, v.z); }

        static bool MeshBounds(Transform t, out Bounds b)
        {
            b = new Bounds();
            bool any = false;
            foreach (var r in t.GetComponentsInChildren<MeshRenderer>())
            {
                if (!r.enabled) continue;
                var rb = r.bounds;
                if (rb.size.y < 0.25f) continue;   // flat bits (a trailing strand, a rug) don't count towards its size
                if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
            }
            return any;
        }
    }
}

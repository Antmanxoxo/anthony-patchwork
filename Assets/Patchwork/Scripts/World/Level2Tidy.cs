using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Keeping the Oceanic Museum tidy: doorways kept clear, things kept a little apart, and a
    // last look round once everything is built for anything still standing in the way.
    //
    // Doorways are found from the walls themselves: two wall blocks on the same line with a gap
    // between them (2 to 14 metres) is a way through. In front of and behind every one, a
    // space as wide as the gap and three metres deep is kept clear: nothing that finds its own
    // room (room details, the quest's props, exhibits) will be put there, and the last look
    // round takes away any loose decoration that was put there by hand.
    public static partial class Level2Builder
    {
        static readonly List<Bounds> doorways = new List<Bounds>();
        const float DoorClear = 3f;        // how far into each room a doorway is kept clear
        const float Spacing = 0.45f;       // the least gap left round anything placed

        static void FindDoorways(LevelKit kit)
        {
            doorways.Clear();
            var walls = new List<Transform>();
            foreach (Transform t in kit.Root)
                if (t.name == "Wall" && t.GetComponent<BoxCollider>() != null) walls.Add(t);

            for (int i = 0; i < walls.Count; i++)
                for (int j = 0; j < walls.Count; j++)
                {
                    if (i == j) continue;
                    Transform a = walls[i], b = walls[j];
                    Vector3 sa = a.localScale, sb = b.localScale;
                    bool ax = sa.x > sa.z, bx = sb.x > sb.z;
                    if (ax != bx) continue;
                    float aPerp = ax ? a.position.z : a.position.x, bPerp = bx ? b.position.z : b.position.x;
                    float thick = ax ? sa.z : sa.x;
                    if (Mathf.Abs(aPerp - bPerp) > 0.4f || Mathf.Abs(thick - (bx ? sb.z : sb.x)) > 0.6f) continue;
                    // a ends, then the gap, then b begins.
                    float aEnd = (ax ? a.position.x : a.position.z) + (ax ? sa.x : sa.z) * 0.5f;
                    float bStart = (bx ? b.position.x : b.position.z) - (bx ? sb.x : sb.z) * 0.5f;
                    float gap = bStart - aEnd;
                    if (gap < 2f || gap > 14f) continue;
                    float y0 = Mathf.Max(a.position.y - sa.y * 0.5f, b.position.y - sb.y * 0.5f);
                    float y1 = Mathf.Min(a.position.y + sa.y * 0.5f, b.position.y + sb.y * 0.5f);
                    if (y1 - y0 < 3f) continue;
                    // (Nothing else on the same line filling the gap.)
                    float mid = (aEnd + bStart) * 0.5f;
                    bool filled = false;
                    foreach (var c in walls)
                    {
                        if (c == a || c == b) continue;
                        Vector3 sc = c.localScale;
                        if ((sc.x > sc.z) != ax) continue;
                        if (Mathf.Abs((ax ? c.position.z : c.position.x) - aPerp) > 0.4f) continue;
                        float c0 = (ax ? c.position.x : c.position.z) - (ax ? sc.x : sc.z) * 0.5f, c1 = c0 + (ax ? sc.x : sc.z);
                        if (mid > c0 && mid < c1 && c.position.y - sc.y * 0.5f < y0 + 1f) { filled = true; break; }
                    }
                    if (filled) continue;

                    // The floor of the doorway: the highest surface in the gap with head room above it.
                    Vector3 centre = ax ? new Vector3(mid, 0f, aPerp) : new Vector3(aPerp, 0f, mid);
                    float floor = float.NaN;
                    var hits = Physics.RaycastAll(new Vector3(centre.x, y1 - 0.2f, centre.z), Vector3.down, y1 - y0 + 0.5f, RmMask, QueryTriggerInteraction.Ignore);
                    System.Array.Sort(hits, (p, q) => p.distance.CompareTo(q.distance));
                    foreach (var h in hits)
                    {
                        if (h.normal.y < 0.7f) continue;
                        if (Physics.Raycast(h.point + Vector3.up * 0.1f, Vector3.up, 2.4f, RmMask, QueryTriggerInteraction.Ignore)) continue;
                        floor = h.point.y;
                        break;
                    }
                    if (float.IsNaN(floor)) continue;

                    float depth = thick + DoorClear * 2f;
                    var zone = new Bounds(new Vector3(centre.x, floor + 1.7f, centre.z), ax ? new Vector3(gap, 3.2f, depth) : new Vector3(depth, 3.2f, gap));
                    bool dup = false;
                    foreach (var d in doorways) if ((d.center - zone.center).sqrMagnitude < 1f) { dup = true; break; }
                    if (!dup) doorways.Add(zone);
                }
            Debug.Log("[Level2] Doorways kept clear: " + doorways.Count + ".");
        }

        static bool InDoorway(Bounds b)
        {
            foreach (var d in doorways) if (d.Intersects(b)) return true;
            return false;
        }

        // The last look round. Anything that is only decoration (nothing solid, nothing that
        // does anything) and stands in a doorway is taken away; anything else in a doorway is
        // named in the log, so it can be moved by hand.
        static void TidyUp(LevelKit kit)
        {
            int removed = 0, left = 0;
            var check = new List<Transform>();
            foreach (Transform t in kit.Root) check.Add(t);
            foreach (var t in check)
            {
                if (t == null || !t.gameObject.activeSelf) continue;
                string n = t.name;
                if (n == "Wall" || n == "Lintel" || n == "Sill" || n.StartsWith("Floor") || n.StartsWith("Checkpoint") || n.StartsWith("Arch")) continue;
                var rs = t.GetComponentsInChildren<Renderer>();
                if (rs.Length == 0) continue;
                Bounds b = rs[0].bounds;
                for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
                Vector3 s = b.size;
                if ((s.x > 7f ? 1 : 0) + (s.y > 7f ? 1 : 0) + (s.z > 7f ? 1 : 0) >= 2) continue;   // (part of the building)
                // Only what stands at walking height in a doorway's clear space.
                Bounds low = new Bounds(b.center, new Vector3(Mathf.Max(0.01f, s.x - 0.1f), s.y, Mathf.Max(0.01f, s.z - 0.1f)));
                bool inWay = false;
                foreach (var d in doorways)
                {
                    var core = new Bounds(d.center, new Vector3(Mathf.Max(0.5f, d.size.x - 0.6f), d.size.y - 0.6f, Mathf.Max(0.5f, d.size.z - 0.6f)));
                    if (core.Intersects(low) && b.min.y < d.min.y + 2.4f && b.max.y > d.min.y + 0.15f) { inWay = true; break; }
                }
                if (!inWay) continue;
                bool solid = false, alive = false;
                foreach (var c in t.GetComponentsInChildren<Collider>()) if (!c.isTrigger) { solid = true; break; }
                foreach (var m in t.GetComponentsInChildren<MonoBehaviour>()) if (m != null) { alive = true; break; }
                if (!solid && !alive)
                {
                    t.gameObject.SetActive(false);
                    removed++;
                    Debug.Log("[Level2] Tidy: took " + n + " out of a doorway at " + b.center.ToString("F1") + ".");
                }
                else
                {
                    left++;
                    Debug.Log("[Level2] Tidy: " + n + " stands in a doorway at " + b.center.ToString("F1") + ".");
                }
            }
            Debug.Log("[Level2] Tidy: " + removed + " removed from doorways, " + left + " still in the way.");
        }

        // Which way to face something that stands against a wall: away from the nearest wall
        // (looked for straight ahead, behind, left and right), so its front is to the room.
        static float FaceTheRoom(Vector3 foot, float fallback)
        {
            float best = 7f, yaw = fallback;
            for (int k = 0; k < 4; k++)
            {
                float a = k * 90f;
                RaycastHit hit;
                if (Physics.Raycast(foot + Vector3.up * 1.3f, Rot(a) * Vector3.forward, out hit, best, RmMask, QueryTriggerInteraction.Ignore))
                {
                    best = hit.distance;
                    yaw = a + 180f;
                }
            }
            return yaw;
        }
    }
}

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Patchwork
{
    // Editor only: a second after a level loads in Play mode, this looks for common building
    // mistakes and writes them to Logs/PatchworkAudit_<level>.txt. It checks for:
    // - props you can walk straight through (on the ground, big enough to bump into, no collider)
    // - boxes whose faces sit exactly on top of each other (they flicker)
    // - collectibles stuck inside something solid, and a blocked spawn point
    // - missing or broken (pink) materials
    // - props sunk into each other, and (in the Pod) anything poking out through the wall
    // It never runs in a build.
    public static class LevelAudit
    {
        public static void Schedule(PatchworkGame game)
        {
            game.StartCoroutine(Run(game));
        }

        static IEnumerator Run(PatchworkGame game)
        {
            yield return new WaitForSeconds(1.5f);
            if (game == null || game.Level == null) yield break;
            var sb = new StringBuilder();
            var player = game.Player != null ? game.Player.transform : null;
            var renderers = new List<MeshRenderer>();
            foreach (var r in Object.FindObjectsByType<MeshRenderer>())
            {
                if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
                if (player != null && r.transform.IsChildOf(player)) continue;
                renderers.Add(r);
            }

            Stats(sb, renderers);
            sb.AppendLine("== Bubbles settled: moved " + ScoreBubbles.Moved + ", dropped " + ScoreBubbles.Dropped);
            foreach (var line in ScoreBubbles.Report) sb.AppendLine("   " + line);
            Broken(sb, renderers);
            Blocked(sb, game);
            WalkThrough(sb, renderers);
            Flicker(sb, renderers);
            Clipping(sb, game);

            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs"));
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "PatchworkAudit_" + game.Level.Id + ".txt");
            File.WriteAllText(path, sb.ToString());
            Debug.Log("Patchwork: level check written to " + path);
        }

        static void Stats(StringBuilder sb, List<MeshRenderer> renderers)
        {
            var mats = new HashSet<Material>();
            long tris = 0;
            foreach (var r in renderers)
            {
                foreach (var m in r.sharedMaterials) if (m != null) mats.Add(m);
                var mf = r.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null && mf.sharedMesh.isReadable)
                    tris += mf.sharedMesh.triangles.Length / 3;
            }
            int colliders = Object.FindObjectsByType<Collider>().Length;
            int lights = Object.FindObjectsByType<Light>().Length;
            sb.AppendLine("== Stats");
            sb.AppendLine("renderers " + renderers.Count + ", materials " + mats.Count + ", triangles " + tris
                + ", colliders " + colliders + ", lights " + lights);
        }

        static void Broken(StringBuilder sb, List<MeshRenderer> renderers)
        {
            sb.AppendLine("== Broken materials");
            int n = 0;
            foreach (var r in renderers)
                foreach (var m in r.sharedMaterials)
                    if (m == null || m.shader == null || !m.shader.isSupported || m.shader.name.Contains("InternalError"))
                    {
                        if (n++ < 40) sb.AppendLine(PathOf(r.transform) + " at " + V(r.bounds.center));
                        break;
                    }
            sb.AppendLine("count " + n);
        }

        static void Blocked(StringBuilder sb, PatchworkGame game)
        {
            sb.AppendLine("== Blocked spots");
            Vector3 s = game.Level.Spawn;
            if (Solid(Physics.OverlapCapsule(s + Vector3.up * 0.5f, s + Vector3.up * 1.2f, 0.4f, ~0, QueryTriggerInteraction.Ignore)))
                sb.AppendLine("spawn " + V(s) + " is inside something solid");
            var things = new List<Component>();
            things.AddRange(Object.FindObjectsByType<ThreadSpool>());
            things.AddRange(Object.FindObjectsByType<Heartstring>());
            things.AddRange(Object.FindObjectsByType<BandPiece>());
            things.AddRange(Object.FindObjectsByType<Collectible>());
            things.AddRange(Object.FindObjectsByType<Checkpoint>());
            foreach (var t in things)
            {
                Vector3 p = t.transform.position;
                if (Solid(Physics.OverlapSphere(p, 0.3f, ~0, QueryTriggerInteraction.Ignore)))
                    sb.AppendLine(t.GetType().Name + " " + PathOf(t.transform) + " at " + V(p) + " is inside something solid");
            }
        }

        // Anything hit that isn't Patch himself.
        static bool Solid(Collider[] hits)
        {
            foreach (var h in hits)
                if (h.GetComponentInParent<PatchworkPlayer>() == null) return true;
            return false;
        }

        // Grounded props at least waist-ish high and wider than a post, with nothing solid in them.
        static void WalkThrough(StringBuilder sb, List<MeshRenderer> renderers)
        {
            sb.AppendLine("== Walk-through props (grouped by spot)");
            var spots = new Dictionary<Vector3Int, List<string>>();
            foreach (var r in renderers)
            {
                var b = r.bounds;
                if (b.size.x > 14f || b.size.z > 14f) continue;      // ground, walls, merged batches
                if (b.size.y < 0.9f || b.size.x < 0.5f || b.size.z < 0.5f) continue;
                if (r.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.Off && IsSeeThrough(r)) continue;
                if (r.GetComponentInParent<Snarling>() != null || r.GetComponentInParent<SnarlKnot>() != null) continue;   // they push you away
                RaycastHit hit;
                if (!Physics.Raycast(new Vector3(b.center.x, b.min.y + 0.35f, b.center.z), Vector3.down, out hit, 1.2f, ~0, QueryTriggerInteraction.Ignore))
                    continue;                                        // not standing on anything
                if (b.min.y - hit.point.y > 0.5f) continue;          // up in the air
                var probe = new Vector3(b.extents.x * 0.6f, Mathf.Min(b.extents.y, 1f) * 0.6f, b.extents.z * 0.6f);
                var probeCentre = new Vector3(b.center.x, Mathf.Max(hit.point.y + 0.6f, b.min.y + probe.y), b.center.z);
                if (Physics.CheckBox(probeCentre, probe, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore)) continue;
                var key = new Vector3Int(Mathf.RoundToInt(b.center.x / 2f), 0, Mathf.RoundToInt(b.center.z / 2f));
                List<string> list;
                if (!spots.TryGetValue(key, out list)) spots[key] = list = new List<string>();
                list.Add(PathOf(r.transform) + " at " + V(b.center) + " size " + V(b.size));
            }
            foreach (var kv in spots)
            {
                sb.AppendLine("near (" + kv.Key.x * 2 + ", " + kv.Key.z * 2 + "): " + kv.Value.Count + " piece(s)");
                for (int i = 0; i < kv.Value.Count && i < 4; i++) sb.AppendLine("   " + kv.Value[i]);
            }
            sb.AppendLine("spots " + spots.Count);
        }

        // Axis-aligned cubes with different materials whose faces lie in the same plane, facing
        // the same way, and overlap: those flicker.
        static void Flicker(StringBuilder sb, List<MeshRenderer> renderers)
        {
            sb.AppendLine("== Flickering faces");
            var boxes = new List<KeyValuePair<Bounds, MeshRenderer>>();
            foreach (var r in renderers)
            {
                var mf = r.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null || mf.sharedMesh.name != "Cube") continue;
                if (!AxisAligned(r.transform.rotation)) continue;
                boxes.Add(new KeyValuePair<Bounds, MeshRenderer>(r.bounds, r));
            }
            boxes.Sort((a, b) => a.Key.min.x.CompareTo(b.Key.min.x));
            int n = 0;
            const float eps = 0.004f;
            for (int i = 0; i < boxes.Count; i++)
            {
                var a = boxes[i].Key;
                for (int j = i + 1; j < boxes.Count && boxes[j].Key.min.x <= a.max.x + eps; j++)
                {
                    var b = boxes[j].Key;
                    if (boxes[i].Value.sharedMaterial == boxes[j].Value.sharedMaterial) continue;
                    for (int k = 0; k < 3; k++)
                    {
                        bool top = Mathf.Abs(a.max[k] - b.max[k]) < eps;
                        bool bottom = Mathf.Abs(a.min[k] - b.min[k]) < eps && k != 1;   // undersides never show
                        if (!top && !bottom) continue;
                        int u = (k + 1) % 3, v = (k + 2) % 3;
                        float ou = Mathf.Min(a.max[u], b.max[u]) - Mathf.Max(a.min[u], b.min[u]);
                        float ov = Mathf.Min(a.max[v], b.max[v]) - Mathf.Max(a.min[v], b.min[v]);
                        if (ou > 0.05f && ov > 0.05f)
                        {
                            // Is the shared face out in the open? Look just in front of its middle.
                            Vector3 mid = Vector3.zero, normal = Vector3.zero;
                            mid[u] = (Mathf.Max(a.min[u], b.min[u]) + Mathf.Min(a.max[u], b.max[u])) * 0.5f;
                            mid[v] = (Mathf.Max(a.min[v], b.min[v]) + Mathf.Min(a.max[v], b.max[v])) * 0.5f;
                            mid[k] = top ? a.max[k] : a.min[k];
                            normal[k] = top ? 1f : -1f;
                            Vector3 front = mid + normal * 0.03f;
                            if (Covered(front, boxes, i, j)) break;
                            if (n++ < 60)
                                sb.AppendLine(PathOf(boxes[i].Value.transform) + " / " + PathOf(boxes[j].Value.transform)
                                    + " at " + V(mid) + " facing " + V(normal));
                            break;
                        }
                    }
                }
            }
            sb.AppendLine("count " + n);
        }

        // Props pushed into each other. A "prop" is one built thing (a bench, a tree, a lamp...):
        // a group of shapes with no shape of its own. It reads the meshes directly, so it still
        // works after the level's still decorations have been merged together.
        static void Clipping(StringBuilder sb, PatchworkGame game)
        {
            sb.AppendLine("== Props sunk into each other");
            Transform levelRoot = game.LevelRoot;
            if (levelRoot == null) { sb.AppendLine("no level found"); return; }

            var props = new List<KeyValuePair<Transform, Bounds>>();
            Gather(levelRoot, props, 0);
            var cells = new Dictionary<Vector2Int, List<int>>();
            for (int i = 0; i < props.Count; i++)
            {
                var b = props[i].Value;
                for (int x = Mathf.FloorToInt(b.min.x / 8f); x <= Mathf.FloorToInt(b.max.x / 8f); x++)
                    for (int z = Mathf.FloorToInt(b.min.z / 8f); z <= Mathf.FloorToInt(b.max.z / 8f); z++)
                    {
                        List<int> l;
                        var k = new Vector2Int(x, z);
                        if (!cells.TryGetValue(k, out l)) cells[k] = l = new List<int>();
                        l.Add(i);
                    }
            }
            var done = new HashSet<long>();
            int n = 0;
            foreach (var l in cells.Values)
                for (int a = 0; a < l.Count; a++)
                    for (int c = a + 1; c < l.Count; c++)
                    {
                        int i = Mathf.Min(l[a], l[c]), j = Mathf.Max(l[a], l[c]);
                        if (!done.Add((long)i * 100000 + j)) continue;
                        Transform ti = props[i].Key, tj = props[j].Key;
                        if (ti.IsChildOf(tj) || tj.IsChildOf(ti)) continue;
                        Bounds bi = props[i].Value, bj = props[j].Value;
                        float ox = Mathf.Min(bi.max.x, bj.max.x) - Mathf.Max(bi.min.x, bj.min.x);
                        float oz = Mathf.Min(bi.max.z, bj.max.z) - Mathf.Max(bi.min.z, bj.min.z);
                        float oy = Mathf.Min(bi.max.y, bj.max.y) - Mathf.Max(bi.min.y, bj.min.y);
                        if (ox <= 0.15f || oz <= 0.15f || oy <= 0.3f) continue;
                        float smaller = Mathf.Min(bi.size.x * bi.size.z, bj.size.x * bj.size.z);
                        if (ox * oz < smaller * 0.3f) continue;
                        // Something sitting on top of something else (a lamp on a table) is fine.
                        if (bi.min.y >= bj.max.y - 0.35f || bj.min.y >= bi.max.y - 0.35f) continue;
                        if (n++ < 120)
                            sb.AppendLine(PathOf(ti) + " " + V(bi.center) + " size " + V(bi.size) + "  <>  " + PathOf(tj) + " " + V(bj.center) + " size " + V(bj.size));
                    }
            sb.AppendLine("pairs " + n + " (of " + props.Count + " props)");

            if (!game.Level.IsHub) return;
            sb.AppendLine("== Through the Pod wall");
            int w = 0;
            foreach (var mf in Object.FindObjectsByType<MeshFilter>())
            {
                if (mf.sharedMesh == null || !mf.gameObject.activeInHierarchy) continue;
                if (mf.GetComponentInParent<PatchworkPlayer>() != null) continue;
                var b = WorldBounds(mf);
                if (b.size.x > 15f || b.size.z > 15f) continue;      // the shell, floor and sky
                if (b.min.y < -0.5f || b.max.y > 14f) continue;      // outside bits above / below
                float dc = new Vector2(b.center.x, b.center.z).magnitude;
                if (dc > 11f) continue;                               // things out in space
                // The wall itself (panels, ribs, the window) is the edge, not something poking through it.
                bool shell = false;
                for (var t = mf.transform; t != null && !shell; t = t.parent)
                    shell = t.name == "PodWall" || t.name == "Rib" || t.name == "WindowPane" || t.name == "PodExterior" || t.name == "Porthole";
                if (shell) continue;
                // Test the real points of the shape (a box round a slanted piece would exaggerate).
                float worst = 0f;
                var m = mf.transform.localToWorldMatrix;
                var verts = mf.sharedMesh.isReadable ? mf.sharedMesh.vertices : null;
                if (verts != null && verts.Length > 0)
                {
                    int step = Mathf.Max(1, verts.Length / 300);
                    for (int i = 0; i < verts.Length; i += step)
                    {
                        var p = m.MultiplyPoint3x4(verts[i]);
                        float over = new Vector2(p.x, p.z).magnitude - HubBuilder.RadiusAt(Mathf.Clamp(p.y, 0f, 13.4f));
                        if (over > worst) worst = over;
                    }
                }
                if (worst > 0.2f && w++ < 80)
                    sb.AppendLine(PathOf(mf.transform) + " at " + V(b.center) + " sticks out " + worst.ToString("0.00") + " m");
            }
            sb.AppendLine("count " + w);
        }

        static void Gather(Transform t, List<KeyValuePair<Transform, Bounds>> props, int depth)
        {
            if (!t.gameObject.activeInHierarchy || depth > 8) return;
            if (t.GetComponentInParent<PatchworkPlayer>() != null || t.name == "CombinedStatic") return;
            bool group = t.childCount > 0 && t.GetComponent<MeshFilter>() == null;
            if (group && depth >= 2)
            {
                bool any = false;
                var b = new Bounds();
                foreach (var mf in t.GetComponentsInChildren<MeshFilter>())
                {
                    if (mf.sharedMesh == null) continue;
                    var wb = WorldBounds(mf);
                    if (!any) { b = wb; any = true; } else b.Encapsulate(wb);
                }
                // One whole thing, between a stool and a house. Bigger groups are areas: look inside.
                if (any && b.size.x < 12f && b.size.z < 12f && b.size.y > 0.4f && (b.size.x > 0.6f || b.size.z > 0.6f))
                {
                    props.Add(new KeyValuePair<Transform, Bounds>(t, b));
                    return;
                }
            }
            for (int i = 0; i < t.childCount; i++) Gather(t.GetChild(i), props, depth + 1);
        }

        static Bounds WorldBounds(MeshFilter mf)
        {
            var lb = mf.sharedMesh.bounds;
            var m = mf.transform.localToWorldMatrix;
            var b = new Bounds(m.MultiplyPoint3x4(lb.center), Vector3.zero);
            for (int i = 0; i < 8; i++)
                b.Encapsulate(m.MultiplyPoint3x4(lb.center + Vector3.Scale(lb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1))));
            return b;
        }

        static bool Covered(Vector3 p, List<KeyValuePair<Bounds, MeshRenderer>> boxes, int skipA, int skipB)
        {
            if (Physics.CheckSphere(p, 0.01f, ~0, QueryTriggerInteraction.Ignore)) return true;
            for (int i = 0; i < boxes.Count; i++)
                if (i != skipA && i != skipB && boxes[i].Key.Contains(p)) return true;
            return false;
        }

        static bool AxisAligned(Quaternion q)
        {
            Vector3 e = q.eulerAngles;
            return Near90(e.x) && Near90(e.y) && Near90(e.z);
        }

        static bool Near90(float a)
        {
            float m = Mathf.Repeat(a, 90f);
            return m < 0.05f || m > 89.95f;
        }

        static bool IsSeeThrough(Renderer r)
        {
            var m = r.sharedMaterial;
            return m != null && m.renderQueue >= 3000;
        }

        static string PathOf(Transform t)
        {
            string s = t.name;
            if (t.parent != null) s = t.parent.name + "/" + s;
            if (t.parent != null && t.parent.parent != null) s = t.parent.parent.name + "/" + s;
            return s;
        }

        static string V(Vector3 v)
        {
            return "(" + v.x.ToString("0.0") + ", " + v.y.ToString("0.0") + ", " + v.z.ToString("0.0") + ")";
        }
    }
}
#endif

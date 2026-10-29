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
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
                if (player != null && r.transform.IsChildOf(player)) continue;
                renderers.Add(r);
            }

            Stats(sb, renderers);
            Broken(sb, renderers);
            Blocked(sb, game);
            WalkThrough(sb, renderers);
            Flicker(sb, renderers);

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
            int colliders = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length;
            int lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Length;
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
            if (Physics.CheckCapsule(s + Vector3.up * 0.5f, s + Vector3.up * 1.2f, 0.4f, ~0, QueryTriggerInteraction.Ignore))
                sb.AppendLine("spawn " + V(s) + " is inside something solid");
            var things = new List<Component>();
            things.AddRange(Object.FindObjectsByType<ThreadSpool>(FindObjectsSortMode.None));
            things.AddRange(Object.FindObjectsByType<Heartstring>(FindObjectsSortMode.None));
            things.AddRange(Object.FindObjectsByType<BandPiece>(FindObjectsSortMode.None));
            things.AddRange(Object.FindObjectsByType<Collectible>(FindObjectsSortMode.None));
            things.AddRange(Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None));
            foreach (var t in things)
            {
                Vector3 p = t.transform.position;
                if (Physics.CheckSphere(p, 0.3f, ~0, QueryTriggerInteraction.Ignore))
                    sb.AppendLine(t.GetType().Name + " " + PathOf(t.transform) + " at " + V(p) + " is inside something solid");
            }
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
                list.Add(PathOf(r.transform) + " size " + V(b.size));
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
                        bool same = Mathf.Abs(a.max[k] - b.max[k]) < eps || Mathf.Abs(a.min[k] - b.min[k]) < eps;
                        if (!same) continue;
                        int u = (k + 1) % 3, v = (k + 2) % 3;
                        float ou = Mathf.Min(a.max[u], b.max[u]) - Mathf.Max(a.min[u], b.min[u]);
                        float ov = Mathf.Min(a.max[v], b.max[v]) - Mathf.Max(a.min[v], b.min[v]);
                        if (ou > 0.05f && ov > 0.05f)
                        {
                            if (n++ < 60)
                                sb.AppendLine(PathOf(boxes[i].Value.transform) + " / " + PathOf(boxes[j].Value.transform)
                                    + " at " + V(a.center) + " axis " + "XYZ"[k]);
                            break;
                        }
                    }
                }
            }
            sb.AppendLine("count " + n);
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

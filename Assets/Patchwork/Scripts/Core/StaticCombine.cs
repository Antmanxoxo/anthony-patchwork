using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace Patchwork
{
    // Makes a built level much cheaper for the computer: Toybox Town is made of tens of thousands
    // of little shapes, and each one costs the CPU time every frame (culling, sorting, a draw).
    // This merges the ones that never change into one mesh per material per patch of the map
    // (32 x 32 m), so the GPU draws exactly the same picture from a few hundred objects.
    //
    // It leaves alone anything that might move, change colour, hide or be picked up. It finds
    // those by looking at every script in the level: the object a script is on, and every
    // object, renderer or transform that a script (or a callback it holds) refers to, are kept
    // separate along with everything under them. Glass and bubbles (transparent things) are
    // kept separate too, so they still sort properly. Colliders stay exactly where they were.
    public static class StaticCombine
    {
        const float Cell = 32f;
        const int MaxMeshVerts = 2500;     // big meshes (already batched grass, the ground) are left as they are

        struct Key
        {
            public int Material, Layer, Cx, Cz;
            public ShadowCastingMode Shadows;
            public bool Receive;
            public override int GetHashCode() { return ((Material * 397 ^ Layer) * 31 + Cx * 7919 + Cz) * 3 + (int)Shadows * 2 + (Receive ? 1 : 0); }
            public override bool Equals(object o)
            {
                if (!(o is Key)) return false;
                var k = (Key)o;
                return k.Material == Material && k.Layer == Layer && k.Cx == Cx && k.Cz == Cz && k.Shadows == Shadows && k.Receive == Receive;
            }
        }

        // The originals hidden behind merged meshes (the editor's level check still looks at them).
        public static readonly List<MeshRenderer> Hidden = new List<MeshRenderer>();

        public static void Run(Transform root)
        {
            Hidden.Clear();
            if (root == null) return;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            matIds.Clear();
            var live = new HashSet<Transform>();
            var seen = new HashSet<object>();

            // 1. Everything a script is on or refers to (in the level, or anywhere in the scene).
            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
            {
                if (mb == null) continue;
                if (mb.transform.IsChildOf(root) && mb.transform != root) live.Add(mb.transform);
                Collect(mb, live, root, seen, 0);
            }
            // Static lists and dictionaries in the game's own code (e.g. the Pod's decorations).
            System.Type[] types;
            try { types = typeof(StaticCombine).Assembly.GetTypes(); }
            catch (ReflectionTypeLoadException e) { types = e.Types; }
            foreach (var type in types)
            {
                if (type == null) continue;
                if (type.Namespace != "Patchwork" || type.ContainsGenericParameters) continue;
                foreach (var f in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (f.IsLiteral || !Interesting(f.FieldType)) continue;
                    object v;
                    try { v = f.GetValue(null); } catch { continue; }
                    CollectValue(v, live, root, seen, 1);
                }
            }

            // 2. Gather what's left by material and map cell.
            var groups = new Dictionary<Key, List<CombineInstance>>();
            var mats = new Dictionary<int, Material>();
            var hidden = new List<MeshRenderer>();
            var isLive = new Dictionary<Transform, bool>();
            foreach (var mf in root.GetComponentsInChildren<MeshFilter>(false))
            {
                var mesh = mf.sharedMesh;
                if (mesh == null || !mesh.isReadable || mesh.vertexCount > MaxMeshVerts || mesh.subMeshCount != 1) continue;
                var r = mf.GetComponent<MeshRenderer>();
                if (r == null || !r.enabled || r.sharedMaterials.Length != 1) continue;
                var mat = r.sharedMaterial;
                if (mat == null || mat.renderQueue >= 2450) continue;          // transparent / cutout: keep sorting right
                var t = mf.transform;
                Vector3 s = t.lossyScale;
                if (s.x * s.y * s.z <= 0f) continue;                             // mirrored: would turn inside out
                if (Live(t, root, live, isLive)) continue;
                Vector3 c = r.bounds.center;
                var key = new Key
                {
                    Material = MatId(mat), Layer = mf.gameObject.layer,
                    Cx = Mathf.FloorToInt(c.x / Cell), Cz = Mathf.FloorToInt(c.z / Cell),
                    Shadows = r.shadowCastingMode, Receive = r.receiveShadows
                };
                List<CombineInstance> list;
                if (!groups.TryGetValue(key, out list)) { list = new List<CombineInstance>(); groups[key] = list; mats[key.Material] = mat; }
                list.Add(new CombineInstance { mesh = mesh, subMeshIndex = 0, transform = t.localToWorldMatrix });
                hidden.Add(r);
            }

            // 3. Build the merged meshes (only where it actually saves something).
            var holder = new GameObject("CombinedStatic").transform;
            holder.SetParent(root, false);
            holder.position = Vector3.zero;
            holder.rotation = Quaternion.identity;
            holder.localScale = Vector3.one;
            int made = 0, merged = 0;
            foreach (var kv in groups)
            {
                var list = kv.Value;
                if (list.Count < 3) continue;   // too few to bother; leave them be
                // Split very big groups so no single mesh gets huge.
                int start = 0;
                while (start < list.Count)
                {
                    int verts = 0, end = start;
                    while (end < list.Count && (verts < 200000 || end == start)) { verts += list[end].mesh.vertexCount; end++; }
                    var part = list.GetRange(start, end - start).ToArray();
                    var mesh = new Mesh { name = "Combined", indexFormat = verts > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                    mesh.CombineMeshes(part, true, true, false);
                    mesh.RecalculateBounds();
                    mesh.UploadMeshData(true);
                    var go = new GameObject("Combined");
                    go.layer = kv.Key.Layer;
                    go.transform.SetParent(holder, false);
                    go.AddComponent<MeshFilter>().sharedMesh = mesh;
                    var mr = go.AddComponent<MeshRenderer>();
                    mr.sharedMaterial = mats[kv.Key.Material];
                    mr.shadowCastingMode = kv.Key.Shadows;
                    mr.receiveShadows = kv.Key.Receive;
                    mr.lightProbeUsage = LightProbeUsage.Off;
                    mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
                    made++;
                    merged += part.Length;
                    start = end;
                }
            }
            // Hide the originals that went into a merged mesh (their colliders stay).
            foreach (var r in hidden)
            {
                Vector3 c = r.bounds.center;
                var key = new Key
                {
                    Material = MatId(r.sharedMaterial), Layer = r.gameObject.layer,
                    Cx = Mathf.FloorToInt(c.x / Cell), Cz = Mathf.FloorToInt(c.z / Cell),
                    Shadows = r.shadowCastingMode, Receive = r.receiveShadows
                };
                if (groups[key].Count >= 3) { r.enabled = false; Hidden.Add(r); }
            }
            Debug.Log("Patchwork: merged " + merged + " still decorations into " + made + " meshes (" + live.Count + " moving or changing things kept separate) in " + watch.ElapsedMilliseconds + " ms.");
        }

        // A number for each material (Unity's own object ids aren't available in this version).
        static readonly Dictionary<Material, int> matIds = new Dictionary<Material, int>();
        static int MatId(Material m)
        {
            int id;
            if (!matIds.TryGetValue(m, out id)) { id = matIds.Count + 1; matIds[m] = id; }
            return id;
        }

        static bool Live(Transform t, Transform root, HashSet<Transform> live, Dictionary<Transform, bool> cache)
        {
            bool result;
            if (cache.TryGetValue(t, out result)) return result;
            if (t == null || t == root) result = false;
            else if (live.Contains(t)) result = true;
            else if (t.GetComponent<Rigidbody>() != null || t.GetComponent<Animator>() != null) result = true;
            else result = Live(t.parent, root, live, cache);
            cache[t] = result;
            return result;
        }

        static bool Interesting(System.Type t)
        {
            if (t.IsPrimitive || t.IsEnum || t == typeof(string)) return false;
            if (typeof(Material).IsAssignableFrom(t) || typeof(Mesh).IsAssignableFrom(t) || typeof(Texture).IsAssignableFrom(t) || typeof(AudioClip).IsAssignableFrom(t) || typeof(Shader).IsAssignableFrom(t)) return false;
            if (t.IsValueType && !t.IsGenericType) return false;   // Vector3, Color, ...
            return true;
        }

        static void Collect(object o, HashSet<Transform> live, Transform root, HashSet<object> seen, int depth)
        {
            if (o == null || depth > 3 || !seen.Add(o)) return;
            var type = o.GetType();
            while (type != null && type != typeof(MonoBehaviour) && type != typeof(object))
            {
                foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (!Interesting(f.FieldType)) continue;
                    object v;
                    try { v = f.GetValue(o); } catch { continue; }
                    CollectValue(v, live, root, seen, depth + 1);
                }
                type = type.BaseType;
            }
        }

        static void CollectValue(object v, HashSet<Transform> live, Transform root, HashSet<object> seen, int depth)
        {
            if (v == null || depth > 4) return;
            if (v is Object)
            {
                var uo = (Object)v;
                if (uo == null) return;   // destroyed
                Transform t = null;
                var comp = uo as Component;
                if (comp != null) t = comp.transform;
                var go = uo as GameObject;
                if (go != null) t = go.transform;
                if (t != null && t != root && t.IsChildOf(root)) live.Add(t);
                return;
            }
            var del = v as System.Delegate;
            if (del != null)
            {
                foreach (var d in del.GetInvocationList())
                    if (d.Target != null) { if (d.Target is Object) CollectValue(d.Target, live, root, seen, depth); else Collect(d.Target, live, root, seen, depth); }
                return;
            }
            var dict = v as IDictionary;
            if (dict != null)
            {
                foreach (var x in dict.Values) CollectValue(x, live, root, seen, depth + 1);
                foreach (var x in dict.Keys) CollectValue(x, live, root, seen, depth + 1);
                return;
            }
            var list = v as IEnumerable;
            if (list != null)
            {
                if (!seen.Add(v)) return;
                foreach (var x in list) CollectValue(x, live, root, seen, depth + 1);
                return;
            }
            // A plain helper object (e.g. a LandedPod record): look inside it.
            Collect(v, live, root, seen, depth);
        }
    }
}

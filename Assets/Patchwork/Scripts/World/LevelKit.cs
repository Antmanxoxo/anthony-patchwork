using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    public enum LevelId
    {
        Hub = 0,
        Level1 = 1
    }

    public class LevelInfo
    {
        public string Id;
        public string Title;
        public string Subtitle;
        public bool IsHub;
        public Vector3 Spawn;
        public float SpawnYaw;
        public float KillY = -12f;
        // Falling this far below the floor (y = 0) counts as a fall, so you respawn right away
        // instead of waiting to drop all the way to KillY. Ignored while swinging on yarn.
        public float FailY = -3f;
        // The play space (set by each level's builder). Sizes the reflection probe and the map.
        public Bounds Area = new Bounds(Vector3.zero, Vector3.zero);
        public Color Sky = new Color(0.78f, 0.86f, 0.95f);
        // Haze and draw distance. FogColor's alpha 0 means "same as Sky".
        public Color FogColor = new Color(0f, 0f, 0f, 0f);
        public float FogStart = 55f, FogEnd = 190f, FarClip = 600f;
        // The stylised look (see LevelLook).
        public float Saturation = 14f, Contrast = 10f, Warmth = 6f, Glow = 0.45f, Vignette = 0.22f;
        public readonly List<string> CollectibleIds = new List<string>();
        public int ThreadTotal;
        // Heartstrings: the main things to find in a level (some open the way to the finale).
        public readonly List<string> HeartstringIds = new List<string>();
        public int HeartstringsNeeded;
        public int HeartstringCount()
        {
            int n = 0;
            foreach (var id in HeartstringIds) if (GameState.IsCollected(id)) n++;
            return n;
        }
        // The pull-up map: pins, a title, a little note, the area to paint and how high the
        // painting camera floats (it must sit below clouds and anything that would cover the view).
        public readonly List<MapPin> MapPins = new List<MapPin>();
        public string MapTitle = "Map";
        public string MapNote = "";
        public Bounds MapArea = new Bounds(Vector3.zero, Vector3.zero);
        public float MapCameraHeight = 34f;
        // The Skiff's level-select globe (hub only).
        public LevelGlobe Globe;

        public int CollectedCount()
        {
            int n = 0;
            foreach (var id in CollectibleIds) if (GameState.IsCollected(id)) n++;
            return n;
        }
    }

    // Helpers for building handcrafted levels from code: quilted platforms, walls, and all the
    // interactive pieces (anchors, grates, fountains, updrafts, checkpoints).
    public class LevelKit
    {
        public static class Palette
        {
            public static readonly Color Cream = new Color(0.96f, 0.9f, 0.78f);
            public static readonly Color Sage = new Color(0.62f, 0.78f, 0.6f);
            public static readonly Color Peach = new Color(0.98f, 0.72f, 0.6f);
            public static readonly Color Lavender = new Color(0.75f, 0.68f, 0.9f);
            // Rose and Sky are toned down for decoration, so the bright pink of yarn anchors and the
            // bright blue of water things are only ever on objects you can use.
            public static readonly Color Sky = new Color(0.64f, 0.74f, 0.84f);
            public static readonly Color Butter = new Color(0.98f, 0.88f, 0.55f);
            public static readonly Color Rose = new Color(0.84f, 0.64f, 0.68f);
            public static readonly Color AnchorPink = new Color(0.95f, 0.45f, 0.62f);
            public static readonly Color Walnut = new Color(0.55f, 0.4f, 0.3f);
            public static readonly Color Moss = new Color(0.45f, 0.62f, 0.45f);
            public static readonly Color Gold = new Color(1f, 0.8f, 0.25f);
            public static readonly Color[] Quilt = { Cream, Sage, Peach, Lavender, Butter, Rose, Sky };
            public static readonly Color[] Meadow = { Sage, Moss, Cream, Butter, new Color(0.7f, 0.85f, 0.62f) };
        }

        public readonly Transform Root;
        public readonly LevelInfo Info = new LevelInfo();
        int patchSeed;

        public LevelKit(string name)
        {
            Root = new GameObject(name).transform;
        }

        public Transform Group(string name, Vector3 position)
        {
            var t = new GameObject(name).transform;
            t.SetParent(Root, false);
            t.position = position;
            return t;
        }

        // ------------------------------------------------------------------ Geometry

        // A solid box with a stitched-fabric look.
        public Transform Block(Vector3 center, Vector3 size, Color color, string name = "Block", Material material = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            var t = go.transform;
            t.SetParent(Root, false);
            t.position = center;
            t.localScale = size;
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = material != null ? material : PatchworkAssets.Fabric(color);
            SetTiling(r, Mathf.Max(1f, Mathf.Round(size.x / 2.5f)), Mathf.Max(1f, Mathf.Round(Mathf.Max(size.y, size.z) / 2.5f)));
            return t;
        }

        // A walkable platform: one invisible collider plus a grid of coloured quilt patches on top.
        // "center"/"size" describe the whole slab.
        public Transform Quilt(Vector3 center, Vector3 size, Color[] palette, float cell = 3.5f, string name = "Platform", bool grass = false)
        {
            var slab = new GameObject(name).transform;
            slab.SetParent(Root, false);
            slab.position = center;
            var col = slab.gameObject.AddComponent<BoxCollider>();
            col.size = size;

            // Side/bottom "batting" layer.
            var under = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(under.GetComponent<Collider>());
            under.name = "Batting";
            under.transform.SetParent(slab, false);
            under.transform.localPosition = new Vector3(0f, -0.06f, 0f);
            under.transform.localScale = new Vector3(size.x - 0.02f, size.y - 0.1f, size.z - 0.02f);
            var ur = under.GetComponent<MeshRenderer>();
            ur.sharedMaterial = PatchworkAssets.Felt(Color.Lerp(palette[0], Palette.Walnut, 0.35f));

            int nx = Mathf.Max(1, Mathf.RoundToInt(size.x / cell));
            int nz = Mathf.Max(1, Mathf.RoundToInt(size.z / cell));
            float cx = size.x / nx, cz = size.z / nz;
            for (int ix = 0; ix < nx; ix++)
                for (int iz = 0; iz < nz; iz++)
                {
                    var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Object.DestroyImmediate(p.GetComponent<Collider>());
                    p.name = "Patch";
                    p.transform.SetParent(slab, false);
                    p.transform.localPosition = new Vector3(-size.x * 0.5f + cx * (ix + 0.5f), size.y * 0.5f - 0.05f, -size.z * 0.5f + cz * (iz + 0.5f));
                    p.transform.localScale = new Vector3(cx - 0.04f, 0.12f, cz - 0.04f);
                    patchSeed++;
                    int pick = (ix * 7 + iz * 13 + patchSeed * 3) % palette.Length;
                    p.GetComponent<MeshRenderer>().sharedMaterial = grass ? PatchworkAssets.Grass(palette[pick]) : PatchworkAssets.Fabric(palette[pick]);
                }
            return slab;
        }

        // Texture repeats are baked into a shared copy of the material (one per material + repeat
        // count) instead of a per-object override, so Unity can still batch the level's draws.
        static readonly Dictionary<Material, Dictionary<Vector2, Material>> tiledMaterials =
            new Dictionary<Material, Dictionary<Vector2, Material>>();

        static void SetTiling(Renderer r, float x, float y)
        {
            var source = r.sharedMaterial;
            if (source == null) return;
            if (Mathf.Approximately(x, 1f) && Mathf.Approximately(y, 1f)) return;
            Dictionary<Vector2, Material> byTiling;
            if (!tiledMaterials.TryGetValue(source, out byTiling))
            {
                byTiling = new Dictionary<Vector2, Material>();
                tiledMaterials[source] = byTiling;
            }
            var tiling = new Vector2(x, y);
            Material m;
            if (!byTiling.TryGetValue(tiling, out m) || m == null)
            {
                m = new Material(source);
                m.name = source.name + "_x" + x + "y" + y;
                m.SetTextureScale("_BaseMap", tiling);
                byTiling[tiling] = m;
            }
            r.sharedMaterial = m;
        }

        // Decoration without collision.
        public Transform Deco(PrimitiveType type, Vector3 position, Vector3 scale, Material mat, Vector3 euler = default(Vector3), Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            var t = go.transform;
            t.SetParent(parent != null ? parent : Root, false);
            if (parent != null) t.localPosition = position; else t.position = position;
            t.localRotation = Quaternion.Euler(euler);
            t.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return t;
        }

        public T Volume<T>(Vector3 center, Vector3 size, string name) where T : PlayerVolume
        {
            var go = new GameObject(name);
            go.transform.SetParent(Root, false);
            go.transform.position = center;
            var v = go.AddComponent<T>();
            v.Size = size;
            return v;
        }

        // ------------------------------------------------------------------ Decorations

        public void Spool(Vector3 basePos, float height, Color thread)
        {
            var g = Group("Spool", basePos);
            var wood = PatchworkAssets.Felt(new Color(0.8f, 0.62f, 0.42f));
            Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.08f * height, 0f), new Vector3(1.4f, 0.08f, 1.4f) * height, wood, default(Vector3), g);
            Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.5f * height, 0f), new Vector3(1.05f, 0.36f, 1.05f) * height, PatchworkAssets.Yarn(thread), default(Vector3), g);
            Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.92f * height, 0f), new Vector3(1.4f, 0.08f, 1.4f) * height, wood, default(Vector3), g);
            var col = g.gameObject.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.5f * height, 0f);
            col.radius = 0.7f * height;
            col.height = height;
        }

        public void YarnTree(Vector3 basePos, float height, Color leaves)
        {
            var g = Group("YarnTree", basePos);
            Deco(PrimitiveType.Cylinder, new Vector3(0f, height * 0.3f, 0f), new Vector3(0.35f, height * 0.3f, 0.35f), PatchworkAssets.Yarn(Palette.Walnut), default(Vector3), g);
            var leaf = PatchworkAssets.Yarn(leaves);
            Deco(PrimitiveType.Sphere, new Vector3(0f, height * 0.75f, 0f), Vector3.one * height * 0.55f, leaf, default(Vector3), g);
            Deco(PrimitiveType.Sphere, new Vector3(height * 0.22f, height * 0.62f, 0.1f), Vector3.one * height * 0.38f, leaf, default(Vector3), g);
            Deco(PrimitiveType.Sphere, new Vector3(-height * 0.2f, height * 0.66f, -0.1f), Vector3.one * height * 0.36f, leaf, default(Vector3), g);
            var col = g.gameObject.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, height * 0.5f, 0f);
            col.radius = 0.25f;
            col.height = height;
        }

        public Transform Flower(Vector3 pos, Color petals)
        {
            var g = Group("Flower", pos);
            Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.25f, 0f), new Vector3(0.05f, 0.25f, 0.05f), PatchworkAssets.Felt(Palette.Moss), default(Vector3), g);
            var pm = PatchworkAssets.Felt(petals);
            for (int i = 0; i < 5; i++)
            {
                float a = i * Mathf.PI * 2f / 5f;
                Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.14f, 0.55f, Mathf.Sin(a) * 0.14f), new Vector3(0.16f, 0.05f, 0.16f), pm, default(Vector3), g);
            }
            Deco(PrimitiveType.Sphere, new Vector3(0f, 0.57f, 0f), Vector3.one * 0.12f, PatchworkAssets.Felt(Palette.Butter), default(Vector3), g);
            return g;
        }

        public void SewingButton(Vector3 pos, float size, Color color, Vector3 euler)
        {
            var g = Group("Button", pos);
            g.rotation = Quaternion.Euler(euler);
            Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(size, 0.08f * size, size), PatchworkAssets.Felt(color), default(Vector3), g);
            var hole = PatchworkAssets.Plain(Color.Lerp(color, Color.black, 0.5f), 0.2f);
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * size * 0.14f;
                float z = (i < 2 ? -1f : 1f) * size * 0.14f;
                Deco(PrimitiveType.Cylinder, new Vector3(x, 0.05f * size, z), new Vector3(size * 0.12f, 0.02f, size * 0.12f), hole, default(Vector3), g);
            }
        }

        public Transform Cloud(Vector3 pos, float size)
        {
            var g = Group("Cloud", pos);
            // Warm cream, not white, so scenery clouds never look like Wind Form or an updraft.
            var m = PatchworkAssets.Felt(new Color(0.98f, 0.91f, 0.84f));
            Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(2f, 1.1f, 1.3f) * size, m, default(Vector3), g);
            Deco(PrimitiveType.Sphere, new Vector3(0.9f, 0.3f, 0f) * size, new Vector3(1.4f, 1.2f, 1.1f) * size, m, default(Vector3), g);
            Deco(PrimitiveType.Sphere, new Vector3(-0.9f, 0.15f, 0.1f) * size, new Vector3(1.3f, 0.9f, 1f) * size, m, default(Vector3), g);
            var s = g.gameObject.AddComponent<Spinner>();
            s.Speed = 0f;
            s.BobHeight = 0.4f * size;
            s.BobSpeed = 0.4f;
            return g;
        }

        // ------------------------------------------------------------------ Cardboard, rope, paint, birds

        // A solid box of brown corrugated cardboard.
        public Transform Cardboard(Vector3 center, Vector3 size, Color color, string name = "Cardboard")
        {
            return Block(center, size, color, name, PatchworkAssets.Cardboard(color));
        }

        // A solid slab shaped like a flat polygon (given as x,z corners, convex) from yBottom to yTop.
        // Used where boxes would have to overlap (like a ship's pointed bow), since overlapping
        // faces at the same height flicker.
        public Transform Prism(Vector2[] corners, float yBottom, float yTop, Material mat, string name = "Prism", float uvScale = 2.5f)
        {
            int n = corners.Length;
            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            Vector2 centre = Vector2.zero;
            foreach (var c in corners) centre += c;
            centre /= n;

            // Adds a triangle facing "outward".
            System.Action<Vector3, Vector3, Vector3, Vector3, Vector2, Vector2, Vector2> tri = (a, b, c, outward, ua, ub, uc) =>
            {
                Vector3 nrm = Vector3.Cross(b - a, c - a);
                if (Vector3.Dot(nrm, outward) < 0f) { var tv = b; b = c; c = tv; var tu = ub; ub = uc; uc = tu; }
                int i0 = verts.Count;
                verts.Add(a); verts.Add(b); verts.Add(c);
                norms.Add(outward); norms.Add(outward); norms.Add(outward);
                uvs.Add(ua); uvs.Add(ub); uvs.Add(uc);
                tris.Add(i0); tris.Add(i0 + 1); tris.Add(i0 + 2);
            };

            Vector3 ctTop = new Vector3(centre.x, yTop, centre.y), ctBot = new Vector3(centre.x, yBottom, centre.y);
            for (int i = 0; i < n; i++)
            {
                Vector2 p = corners[i], q = corners[(i + 1) % n];
                Vector3 pt = new Vector3(p.x, yTop, p.y), qt = new Vector3(q.x, yTop, q.y);
                Vector3 pb = new Vector3(p.x, yBottom, p.y), qb = new Vector3(q.x, yBottom, q.y);
                tri(ctTop, pt, qt, Vector3.up, centre / uvScale, p / uvScale, q / uvScale);
                tri(ctBot, pb, qb, Vector3.down, centre / uvScale, p / uvScale, q / uvScale);
                Vector2 edge = q - p;
                Vector3 outward = new Vector3(edge.y, 0f, -edge.x).normalized;
                if (Vector2.Dot(new Vector2(outward.x, outward.z), (p + q) * 0.5f - centre) < 0f) outward = -outward;
                float len = edge.magnitude / uvScale;
                float h0 = yBottom / uvScale, h1 = yTop / uvScale;
                tri(pb, qb, qt, outward, new Vector2(0f, h0), new Vector2(len, h0), new Vector2(len, h1));
                tri(pb, qt, pt, outward, new Vector2(0f, h0), new Vector2(len, h1), new Vector2(0f, h1));
            }

            var mesh = new Mesh();
            mesh.name = name;
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            var go = new GameObject(name);
            go.transform.SetParent(Root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            var col = go.AddComponent<MeshCollider>();
            col.sharedMesh = mesh;
            col.convex = true;
            go.AddComponent<OwnedMesh>().Mesh = mesh;
            return go.transform;
        }

        // A strip of shiny brown packing tape (decoration).
        public Transform Tape(Vector3 center, Vector3 size, Vector3 euler = default(Vector3))
        {
            return NoShadow(Deco(PrimitiveType.Cube, center, size, PatchworkAssets.Plain(new Color(0.9f, 0.76f, 0.5f), 0.6f), euler));
        }

        // A straight rope (or stick) from a to b.
        public Transform Rope(Vector3 a, Vector3 b, Material mat, float thickness = 0.08f)
        {
            Vector3 d = b - a;
            var t = Deco(PrimitiveType.Cylinder, (a + b) * 0.5f, new Vector3(thickness, d.magnitude * 0.5f, thickness), mat);
            t.rotation = Quaternion.FromToRotation(Vector3.up, d.normalized);
            return NoShadow(t);
        }

        // A flat lake of swirling marbled paint and ink (no collision). size is x by z in metres.
        public PaintLake PaintLake(Vector3 center, Vector2 size, float cell = 14f)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = "PaintLake";
            go.transform.SetParent(Root, false);
            go.transform.position = center;
            go.transform.localScale = new Vector3(size.x / 10f, 1f, size.y / 10f);
            var mat = PatchworkAssets.PaintLake();
            mat.SetTextureScale("_BaseMap", new Vector2(Mathf.Max(1f, Mathf.Round(size.x / cell)), Mathf.Max(1f, Mathf.Round(size.y / cell))));
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var lake = go.AddComponent<PaintLake>();
            lake.Setup(mat);
            return lake;
        }

        // A yarn bird: circling overhead (radius > 0 and circling), or sitting at pos.
        public YarnBird YarnBird(Vector3 pos, Color color, bool circling, float radius = 4f)
        {
            var go = new GameObject("YarnBird");
            go.transform.SetParent(Root, false);
            var b = go.AddComponent<YarnBird>();
            b.Setup(circling ? Patchwork.YarnBird.Mode.Circle : Patchwork.YarnBird.Mode.Perch, pos, radius, color);
            return b;
        }

        // ------------------------------------------------------------------ Gameplay pieces

        public Checkpoint AddCheckpoint(Vector3 feet, float yaw)
        {
            var cp = Volume<Checkpoint>(feet + Vector3.up * 1.5f, new Vector3(12f, 4f, 3f), "Checkpoint");
            cp.SpawnPoint = feet + Vector3.up * 0.1f;
            cp.SpawnYaw = yaw;
            // A giant sewing pin with a felt flag.
            Vector3 pinPos = feet + Quaternion.Euler(0f, yaw, 0f) * new Vector3(-1.6f, 0f, 0f);
            Deco(PrimitiveType.Cylinder, pinPos + Vector3.up * 1.2f, new Vector3(0.07f, 1.2f, 0.07f), PatchworkAssets.Plain(new Color(0.85f, 0.85f, 0.9f), 0.8f));
            Deco(PrimitiveType.Sphere, pinPos + Vector3.up * 2.5f, Vector3.one * 0.4f, PatchworkAssets.Felt(Palette.Rose));
            var flag = Deco(PrimitiveType.Cube, pinPos + Vector3.up * 1.95f + Quaternion.Euler(0f, yaw, 0f) * new Vector3(0.45f, 0f, 0f),
                new Vector3(0.8f, 0.5f, 0.04f), PatchworkAssets.Fabric(Palette.Cream), new Vector3(0f, yaw, 0f));
            cp.Setup(flag.GetComponent<Renderer>());
            return cp;
        }

        public Collectible GoldenButton(string id, Vector3 pos)
        {
            Info.CollectibleIds.Add(id);
            var c = Volume<Collectible>(pos, new Vector3(1.8f, 2.4f, 1.8f), "GoldenButton_" + id);
            var vis = new GameObject("Visual").transform;
            vis.SetParent(c.transform, false);
            var gold = PatchworkAssets.Glow(Palette.Gold, 0.5f);
            Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.9f, 0.06f, 0.9f), gold, new Vector3(90f, 0f, 0f), vis);
            var hole = PatchworkAssets.Plain(new Color(0.5f, 0.32f, 0.05f), 0.4f);
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * 0.13f;
                float y = (i < 2 ? -1f : 1f) * 0.13f;
                Deco(PrimitiveType.Cylinder, new Vector3(x, y, 0f), new Vector3(0.1f, 0.07f, 0.1f), hole, new Vector3(90f, 0f, 0f), vis);
            }
            c.Setup(id, vis);
            return c;
        }

        // Things Yarn Form can tie onto. Whatever it looks like, the part you hook is wrapped in
        // bright pink yarn and glows when it's in reach, so it always reads the same way.
        public enum AnchorStyle
        {
            Button,     // a big felt button hanging on a thread
            Peg,        // a wooden clothes peg clipped to a line
            Lantern,    // a paper lantern hanging from a rope
            Spool,      // a pulley-like spool of pink thread on a hook
            Knot        // a big pink yarn knot tied round a branch, beam or pole
        }

        // hangLength: how far up the thread it hangs from goes (0 = no thread, e.g. a peg on a
        // line or a knot on a beam that the caller builds). Build what it hangs from yourself, so
        // the thread always ends on something real.
        public YarnAnchor Anchor(Vector3 pos, float range = 13f, AnchorStyle style = AnchorStyle.Button, float hangLength = 0f)
        {
            var g = Group("YarnAnchor", pos);
            var anchor = g.gameObject.AddComponent<YarnAnchor>();
            anchor.Range = range;
            var vis = new GameObject("Visual").transform;
            vis.SetParent(g, false);
            var pink = PatchworkAssets.Felt(Palette.AnchorPink);
            var pinkYarn = PatchworkAssets.Yarn(Palette.AnchorPink);
            var glow = new List<Renderer>();
            switch (style)
            {
                case AnchorStyle.Peg:
                {
                    var wood = PatchworkAssets.Wood(new Color(0.88f, 0.72f, 0.5f));
                    Deco(PrimitiveType.Cube, new Vector3(-0.12f, -0.45f, 0f), new Vector3(0.18f, 1.1f, 0.32f), wood, new Vector3(0f, 0f, 4f), vis);
                    Deco(PrimitiveType.Cube, new Vector3(0.12f, -0.45f, 0f), new Vector3(0.18f, 1.1f, 0.32f), wood, new Vector3(0f, 0f, -4f), vis);
                    var band = Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.35f, 0f), new Vector3(0.62f, 0.14f, 0.5f), pinkYarn, default(Vector3), vis);
                    glow.Add(band.GetComponent<Renderer>());
                    Deco(PrimitiveType.Sphere, new Vector3(0f, 0.05f, 0f), new Vector3(0.36f, 0.2f, 0.36f), PatchworkAssets.Plain(new Color(0.7f, 0.72f, 0.78f), 0.8f), default(Vector3), vis);
                    break;
                }
                case AnchorStyle.Lantern:
                {
                    var body = Deco(PrimitiveType.Sphere, new Vector3(0f, -0.9f, 0f), new Vector3(1.1f, 1.3f, 1.1f), PatchworkAssets.Glow(new Color(1f, 0.82f, 0.6f), 1.2f), default(Vector3), vis);
                    var topCap = Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.25f, 0f), new Vector3(0.7f, 0.08f, 0.7f), pink, default(Vector3), vis);
                    var botCap = Deco(PrimitiveType.Cylinder, new Vector3(0f, -1.55f, 0f), new Vector3(0.7f, 0.08f, 0.7f), pink, default(Vector3), vis);
                    var wrap = Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.9f, 0f), new Vector3(1.16f, 0.08f, 1.16f), pinkYarn, default(Vector3), vis);
                    Deco(PrimitiveType.Cylinder, new Vector3(0f, -1.75f, 0f), new Vector3(0.08f, 0.15f, 0.08f), pinkYarn, default(Vector3), vis);
                    glow.Add(topCap.GetComponent<Renderer>());
                    glow.Add(botCap.GetComponent<Renderer>());
                    glow.Add(wrap.GetComponent<Renderer>());
                    body.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    break;
                }
                case AnchorStyle.Spool:
                {
                    var wood = PatchworkAssets.Wood(new Color(0.8f, 0.62f, 0.42f));
                    Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.5f, -0.4f), new Vector3(1.4f, 0.06f, 1.4f), wood, new Vector3(90f, 0f, 0f), vis);
                    Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.5f, 0.4f), new Vector3(1.4f, 0.06f, 1.4f), wood, new Vector3(90f, 0f, 0f), vis);
                    var thread = Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.5f, 0f), new Vector3(1.05f, 0.38f, 1.05f), pinkYarn, new Vector3(90f, 0f, 0f), vis);
                    glow.Add(thread.GetComponent<Renderer>());
                    Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.05f, 0f), new Vector3(0.08f, 0.1f, 0.08f), PatchworkAssets.Plain(new Color(0.55f, 0.55f, 0.6f), 0.8f), default(Vector3), vis);
                    break;
                }
                case AnchorStyle.Knot:
                {
                    var k1 = Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.9f, 0.7f, 0.9f), pinkYarn, new Vector3(20f, 30f, 0f), vis);
                    var k2 = Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.0f, 0.16f, 1.0f), pinkYarn, new Vector3(90f, 0f, 0f), vis);
                    var tailA = Deco(PrimitiveType.Capsule, new Vector3(0.2f, -0.6f, 0.1f), new Vector3(0.14f, 0.45f, 0.14f), pinkYarn, new Vector3(0f, 0f, 12f), vis);
                    var tailB = Deco(PrimitiveType.Capsule, new Vector3(-0.15f, -0.65f, -0.1f), new Vector3(0.14f, 0.5f, 0.14f), pinkYarn, new Vector3(0f, 0f, -10f), vis);
                    glow.Add(k1.GetComponent<Renderer>());
                    glow.Add(k2.GetComponent<Renderer>());
                    glow.Add(tailA.GetComponent<Renderer>());
                    glow.Add(tailB.GetComponent<Renderer>());
                    break;
                }
                default:
                {
                    var disc = Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.5f, 0.14f, 1.5f), pink, new Vector3(90f, 0f, 0f), vis);
                    var rim = Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0f), new Vector3(1.75f, 0.1f, 1.75f), pink, new Vector3(90f, 0f, 0f), vis);
                    var hole = PatchworkAssets.Plain(new Color(0.45f, 0.12f, 0.25f), 0.3f);
                    for (int i = 0; i < 4; i++)
                    {
                        float x = (i % 2 == 0 ? -1f : 1f) * 0.22f;
                        float y = (i < 2 ? -1f : 1f) * 0.22f;
                        Deco(PrimitiveType.Cylinder, new Vector3(x, y, 0f), new Vector3(0.18f, 0.16f, 0.18f), hole, new Vector3(90f, 0f, 0f), vis);
                    }
                    glow.Add(disc.GetComponent<Renderer>());
                    glow.Add(rim.GetComponent<Renderer>());
                    break;
                }
            }
            // The thread it hangs from, up to whatever is holding it.
            if (hangLength > 0f)
                NoShadow(Deco(PrimitiveType.Cylinder, pos + Vector3.up * (hangLength * 0.5f + 0.4f), new Vector3(0.05f, hangLength * 0.5f, 0.05f), pinkYarn));
            anchor.Setup(vis, glow.ToArray(), new Color(1f, 0.45f, 0.65f));
            anchor.Swings = style != AnchorStyle.Knot && style != AnchorStyle.Spool;
            return anchor;
        }

        // A blue drain grate filling an opening. "size" is the opening size.
        public WaterGrate Grate(Vector3 center, Vector3 size)
        {
            var g = Group("DrainGrate", center);
            var col = g.gameObject.AddComponent<BoxCollider>();
            col.size = size;
            var grate = g.gameObject.AddComponent<WaterGrate>();
            grate.Collider = col;

            var barMat = PatchworkAssets.Plain(new Color(0.3f, 0.5f, 0.85f), 0.85f);
            int bars = Mathf.Max(3, Mathf.RoundToInt(size.x / 0.22f));
            for (int i = 0; i <= bars; i++)
            {
                float x = -size.x * 0.5f + size.x * i / bars;
                Deco(PrimitiveType.Cylinder, new Vector3(x, 0f, 0f), new Vector3(0.07f, size.y * 0.5f, 0.07f), barMat, default(Vector3), g);
            }
            Deco(PrimitiveType.Cube, new Vector3(0f, size.y * 0.5f, 0f), new Vector3(size.x + 0.15f, 0.1f, 0.15f), barMat, default(Vector3), g);
            Deco(PrimitiveType.Cube, new Vector3(0f, -size.y * 0.5f + 0.03f, 0f), new Vector3(size.x + 0.15f, 0.08f, 0.15f), barMat, default(Vector3), g);
            // Big water-drop emblem above the opening so the route reads at a glance.
            var sign = new GameObject("DropSign").transform;
            sign.SetParent(g, false);
            sign.localPosition = new Vector3(0f, size.y * 0.5f + 1.0f, -0.3f);
            var water = PatchworkAssets.Water(FormLibrary.Get(FormType.Water).PrimaryColor);
            Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, 0f), new Vector3(0.6f, 0.6f, 0.3f), water, default(Vector3), sign);
            Deco(PrimitiveType.Sphere, new Vector3(0f, 0.35f, 0f), new Vector3(0.3f, 0.45f, 0.2f), water, default(Vector3), sign);
            var spin = sign.gameObject.AddComponent<Spinner>();
            spin.Speed = 0f;
            spin.BobHeight = 0.12f;
            return grate;
        }

        // What a fountain or a geyser comes out of. The water column itself always looks the same
        // (bright, glassy, bubbling, with a foamy crown), so Water Form always knows it can ride it.
        public enum FountainStyle
        {
            Stone,      // a round carved-stone basin with a fish spout (town squares, gardens)
            Pipe,       // a copper pipe sticking out of the ground (mills, workshops, sewers)
            Teapot,     // the spout of a giant toppled teapot
            Spring,     // just a bubbling patch of water (canals, ponds)
            Barrel      // a ship's water barrel with brass hoops (the Skiff)
        }

        public WaterCurrent Fountain(Vector3 basePos, float height, FountainStyle style = FountainStyle.Stone)
        {
            var current = Volume<WaterCurrent>(basePos + Vector3.up * height * 0.5f, new Vector3(2.4f, height, 2.4f), "Fountain");
            Color waterCol = FormLibrary.Get(FormType.Water).PrimaryColor;
            var surface = PatchworkAssets.Water(waterCol);
            var glass = PatchworkAssets.Bubble(new Color(0.62f, 0.85f, 1f, 0.5f));
            var core = PatchworkAssets.Bubble(new Color(0.85f, 0.95f, 1f, 0.65f));
            var foam = PatchworkAssets.Felt(new Color(0.94f, 0.98f, 1f));

            // ----- What it comes out of -----
            switch (style)
            {
                case FountainStyle.Pipe:
                {
                    var copper = PatchworkAssets.Plain(new Color(0.78f, 0.48f, 0.3f), 0.75f);
                    NoShadow(Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.35f, new Vector3(1.3f, 0.35f, 1.3f), copper));
                    Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.72f, new Vector3(1.55f, 0.05f, 1.55f), copper);
                    for (int i = 0; i < 6; i++)
                    {
                        float a = i / 6f * Mathf.PI * 2f;
                        Deco(PrimitiveType.Sphere, basePos + new Vector3(Mathf.Cos(a) * 0.72f, 0.72f, Mathf.Sin(a) * 0.72f), Vector3.one * 0.14f, PatchworkAssets.Plain(new Color(0.6f, 0.4f, 0.28f), 0.6f));
                    }
                    Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.06f, new Vector3(2.6f, 0.04f, 2.6f), surface);
                    break;
                }
                case FountainStyle.Teapot:
                {
                    var china = PatchworkAssets.Plain(new Color(0.95f, 0.95f, 0.98f), 0.85f);
                    var blue = PatchworkAssets.Plain(new Color(0.35f, 0.5f, 0.85f), 0.8f);
                    var pot = Group("Teapot", basePos + new Vector3(-2.1f, 1.1f, 0f));
                    Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(2.6f, 2.1f, 2.4f), china, new Vector3(0f, 0f, -12f), pot);
                    Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0f), new Vector3(2.66f, 0.08f, 2.46f), blue, new Vector3(0f, 0f, -12f), pot);
                    Deco(PrimitiveType.Cylinder, new Vector3(-0.3f, 1.1f, 0f), new Vector3(1f, 0.1f, 1f), china, new Vector3(0f, 0f, -12f), pot);
                    Deco(PrimitiveType.Sphere, new Vector3(-0.35f, 1.25f, 0f), Vector3.one * 0.3f, blue, default(Vector3), pot);
                    var handle = Deco(PrimitiveType.Capsule, new Vector3(-1.45f, 0.1f, 0f), new Vector3(0.22f, 0.7f, 0.22f), china, new Vector3(0f, 0f, -20f), pot);
                    handle.name = "Handle";
                    // The spout curves up to the jet.
                    Rope(basePos + new Vector3(-1f, 0.7f, 0f), basePos + new Vector3(-0.2f, 0.55f, 0f), china, 0.55f);
                    Deco(PrimitiveType.Cylinder, basePos + new Vector3(0f, 0.55f, 0f), new Vector3(0.6f, 0.12f, 0.6f), blue);
                    var col = pot.gameObject.AddComponent<SphereCollider>();
                    col.radius = 1.05f;
                    Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.04f, new Vector3(2.8f, 0.03f, 2.8f), surface);
                    break;
                }
                case FountainStyle.Spring:
                    Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.04f, new Vector3(3f, 0.03f, 3f), surface);
                    break;
                case FountainStyle.Barrel:
                {
                    // A squat water butt: bulging staves, brass hoops, a little tap, and the
                    // water spouting out of the open top.
                    var staves = PatchworkAssets.Planks(new Color(0.62f, 0.42f, 0.28f));
                    var brass = PatchworkAssets.Plain(new Color(0.88f, 0.7f, 0.38f), 0.8f);
                    NoShadow(Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.45f, new Vector3(1.9f, 0.45f, 1.9f), staves));
                    Deco(PrimitiveType.Sphere, basePos + Vector3.up * 0.45f, new Vector3(2.05f, 0.7f, 2.05f), staves);
                    for (int i = 0; i < 3; i++)
                        Deco(PrimitiveType.Cylinder, basePos + Vector3.up * (0.12f + i * 0.33f), new Vector3(i == 1 ? 2.1f : 1.96f, 0.05f, i == 1 ? 2.1f : 1.96f), brass);
                    Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.88f, new Vector3(1.7f, 0.02f, 1.7f), surface);
                    // Solid, so you hop up onto it instead of walking through the staves.
                    var solid = new GameObject("BarrelSolid");
                    solid.transform.SetParent(Root, false);
                    solid.transform.position = basePos + Vector3.up * 0.45f;
                    solid.AddComponent<BoxCollider>().size = new Vector3(1.5f, 0.9f, 1.5f);
                    var tap = Group("Tap", basePos + new Vector3(0f, 0.3f, -1.05f));
                    Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.12f, 0.12f, 0.12f), brass, new Vector3(90f, 0f, 0f), tap);
                    Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.08f, -0.1f), new Vector3(0.08f, 0.08f, 0.08f), brass, default(Vector3), tap);
                    Deco(PrimitiveType.Cube, new Vector3(0f, 0.1f, -0.03f), new Vector3(0.2f, 0.04f, 0.04f), brass, default(Vector3), tap);
                    // A puddle where it's dripped on the deck.
                    Deco(PrimitiveType.Cylinder, basePos + new Vector3(0.1f, 0.01f, -1.25f), new Vector3(0.7f, 0.01f, 0.45f), surface);
                    break;
                }
                default:
                {
                    // A Victorian stone basin: a fat lip, a carved band and a little fish on top.
                    var stone = PatchworkAssets.Plaster(new Color(0.86f, 0.84f, 0.8f));
                    var band = PatchworkAssets.Cobble(new Color(0.75f, 0.74f, 0.72f));
                    NoShadow(Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.3f, new Vector3(3.6f, 0.3f, 3.6f), stone));
                    Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.62f, new Vector3(3.9f, 0.06f, 3.9f), band);
                    Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.63f, new Vector3(3.2f, 0.05f, 3.2f), surface);
                    Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.75f, new Vector3(0.9f, 0.15f, 0.9f), stone);
                    var fish = Group("Fish", basePos + Vector3.up * 1f);
                    var gold = PatchworkAssets.Plain(new Color(0.95f, 0.65f, 0.3f), 0.7f);
                    Deco(PrimitiveType.Sphere, new Vector3(0f, 0.05f, 0f), new Vector3(0.5f, 0.42f, 0.3f), gold, new Vector3(0f, 0f, 70f), fish);
                    Deco(PrimitiveType.Cube, new Vector3(0f, -0.3f, 0f), new Vector3(0.3f, 0.25f, 0.05f), gold, new Vector3(0f, 0f, 45f), fish);
                    break;
                }
            }

            // ----- The water column -----
            float r0 = 0.62f;
            var column = Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(r0 * 2f, height * 0.5f, r0 * 2f), glass, default(Vector3), current.transform);
            column.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var jet = Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.45f, height * 0.5f, 0.45f), core, default(Vector3), current.transform);
            jet.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            // Bubbles rising inside it.
            var bubbles = new Transform[10];
            for (int i = 0; i < bubbles.Length; i++)
            {
                bubbles[i] = Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.25f, core, default(Vector3), current.transform);
                bubbles[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            // Foamy crown at the top and droplets arcing out from it.
            var crown = new Transform[8];
            for (int i = 0; i < crown.Length; i++)
                crown[i] = Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.45f, foam, default(Vector3), current.transform);
            var drops = new Transform[12];
            for (int i = 0; i < drops.Length; i++)
            {
                drops[i] = Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.16f, glass, default(Vector3), current.transform);
                drops[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            current.SetupVisuals(column, jet, bubbles, crown, drops, height, style != FountainStyle.Spring && style != FountainStyle.Pipe ? 1.6f : 1.2f);
            AmbientEmitter.Create(current.transform, basePos + Vector3.up * 0.5f, Sfx.Ambient(Sfx.AmbientClip.Fountain), 0.25f, 1.5f, 12f);
            return current;
        }

        // What an updraft rises out of. Wind Form can always ride the white swirl above it.
        public enum UpdraftStyle
        {
            Vent,       // a round felt vent in the ground (the original)
            Chimney,    // smoke from a brick chimney (put basePos on the chimney top)
            Steam       // a brass grate breathing warm steam
        }

        public WindUpdraft Updraft(Vector3 basePos, float height, UpdraftStyle style = UpdraftStyle.Vent)
        {
            var up = Volume<WindUpdraft>(basePos + Vector3.up * height * 0.5f, new Vector3(2.6f, height, 2.6f), "Updraft");
            switch (style)
            {
                case UpdraftStyle.Chimney:
                {
                    var brick = PatchworkAssets.Brick(new Color(0.72f, 0.4f, 0.32f));
                    var stack = Block(basePos + Vector3.down * 1.6f, new Vector3(1.6f, 3.2f, 1.6f), new Color(0.72f, 0.4f, 0.32f), "Chimney", brick);
                    SetTiling(stack.GetComponent<Renderer>(), 1f, 2f);
                    Deco(PrimitiveType.Cube, basePos + Vector3.up * 0.1f, new Vector3(1.9f, 0.2f, 1.9f), brick);
                    Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.4f, new Vector3(0.7f, 0.3f, 0.7f), PatchworkAssets.Plain(new Color(0.75f, 0.45f, 0.3f), 0.4f));
                    Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.71f, new Vector3(0.5f, 0.02f, 0.5f), PatchworkAssets.Plain(new Color(0.1f, 0.08f, 0.08f), 0.2f));
                    break;
                }
                case UpdraftStyle.Steam:
                {
                    var brass = PatchworkAssets.Plain(new Color(0.85f, 0.68f, 0.35f), 0.75f);
                    Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.05f, new Vector3(2.6f, 0.05f, 2.6f), brass);
                    for (int i = -2; i <= 2; i++)
                        Deco(PrimitiveType.Cube, basePos + new Vector3(i * 0.42f, 0.1f, 0f), new Vector3(0.12f, 0.04f, 2.1f), PatchworkAssets.Plain(new Color(0.25f, 0.2f, 0.18f), 0.4f));
                    break;
                }
                default:
                    Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.12f, new Vector3(2.6f, 0.12f, 2.6f), PatchworkAssets.Felt(new Color(0.85f, 0.92f, 0.95f)));
                    Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.25f, new Vector3(2.0f, 0.02f, 2.0f), PatchworkAssets.Plain(new Color(0.3f, 0.35f, 0.4f), 0.2f));
                    break;
            }
            var ribbon = PatchworkAssets.Glow(new Color(0.85f, 1f, 0.97f), 0.4f);
            var ribbons = new Transform[8];
            for (int i = 0; i < ribbons.Length; i++)
                ribbons[i] = Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(0.1f, 0.9f, 0.03f), ribbon, default(Vector3), up.transform);
            up.SetupRibbons(ribbons, height);
            if (style != UpdraftStyle.Vent)
            {
                // Soft puffs of smoke or steam rising and spreading.
                var puffMat = PatchworkAssets.Felt(style == UpdraftStyle.Chimney ? new Color(0.88f, 0.88f, 0.9f) : new Color(0.97f, 0.97f, 1f));
                var puffs = new Transform[7];
                for (int i = 0; i < puffs.Length; i++)
                {
                    puffs[i] = Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.6f, puffMat, default(Vector3), up.transform);
                    puffs[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
                up.SetupPuffs(puffs);
            }
            AmbientEmitter.Create(up.transform, basePos + Vector3.up * 0.8f, Sfx.Ambient(Sfx.AmbientClip.Vent), 0.18f, 1.5f, 9f);
            return up;
        }

        // A box where the camera backs out (and optionally tilts down) a little more.
        public CameraZone AddCameraZone(Vector3 center, Vector3 size, float extraDistance, float extraPitch = 0f)
        {
            var z = Volume<CameraZone>(center, size, "CameraZone");
            z.ExtraDistance = extraDistance;
            z.ExtraPitch = extraPitch;
            return z;
        }

        public HintZone Hint(Vector3 center, Vector3 size, string title, string body, HintTheme theme = HintTheme.Neutral)
        {
            var h = Volume<HintZone>(center, size, "Hint_" + title);
            h.Title = title;
            h.Body = body;
            h.Theme = theme;
            return h;
        }

        // ------------------------------------------------------------------ Interactive pieces

        // A squishy pin-cushion bounce pad, low enough to walk onto.
        // A big toy drum to bounce on: painted shell, gold zig-zag cords, a white skin that wobbles.
        public BouncePad AddDrumPad(Vector3 feet, float launchSpeed, Color shell)
        {
            const float h = 1.1f;
            var pad = Volume<BouncePad>(feet + Vector3.up * (h + 0.4f), new Vector3(2.4f, 1.4f, 2.4f), "DrumPad");
            pad.LaunchSpeed = launchSpeed;
            pad.Drum = true;
            var g = Group("ToyDrum", feet);
            var paint = PatchworkAssets.Plain(shell, 0.6f);
            var gold = PatchworkAssets.Plain(Palette.Gold, 0.75f);
            Deco(PrimitiveType.Cylinder, new Vector3(0f, h * 0.5f, 0f), new Vector3(2.4f, h * 0.5f, 2.4f), paint, default(Vector3), g);
            Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.08f, 0f), new Vector3(2.55f, 0.08f, 2.55f), PatchworkAssets.Plain(new Color(0.95f, 0.92f, 0.85f), 0.5f), default(Vector3), g);
            Deco(PrimitiveType.Cylinder, new Vector3(0f, h - 0.04f, 0f), new Vector3(2.55f, 0.08f, 2.55f), PatchworkAssets.Plain(new Color(0.95f, 0.92f, 0.85f), 0.5f), default(Vector3), g);
            for (int i = 0; i < 10; i++)
            {
                float a = i / 10f * 360f;
                var cord = Deco(PrimitiveType.Cube, Quaternion.Euler(0f, a, 0f) * new Vector3(0f, h * 0.5f, 1.23f), new Vector3(0.06f, h * 0.95f, 0.04f), gold, new Vector3(0f, a, i % 2 == 0 ? 28f : -28f), g);
                cord.name = "Cord";
            }
            var skin = Deco(PrimitiveType.Cylinder, new Vector3(0f, h + 0.02f, 0f), new Vector3(2.3f, 0.04f, 2.3f), PatchworkAssets.Felt(new Color(0.98f, 0.96f, 0.9f)), default(Vector3), g);
            var col = g.gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, h * 0.5f, 0f);
            col.size = new Vector3(2.1f, h, 2.1f);
            pad.Setup(skin);
            return pad;
        }

        public BouncePad AddBouncePad(Vector3 feet, float launchSpeed = 14f)
        {
            var pad = Volume<BouncePad>(feet + Vector3.up * 0.7f, new Vector3(2.2f, 1.4f, 2.2f), "BouncePad");
            pad.LaunchSpeed = launchSpeed;
            var g = Group("PinCushion", feet);
            Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.06f, 0f), new Vector3(2.3f, 0.06f, 2.3f), PatchworkAssets.Wood(new Color(0.75f, 0.55f, 0.35f)), default(Vector3), g);
            var cushion = Deco(PrimitiveType.Sphere, new Vector3(0f, 0.14f, 0f), new Vector3(2f, 0.42f, 2f), PatchworkAssets.Felt(new Color(0.9f, 0.25f, 0.3f)), default(Vector3), g);
            Color[] heads = { Palette.Gold, Palette.Sky, Palette.Cream, Palette.Sage, Palette.Lavender };
            for (int i = 0; i < 5; i++)
            {
                float a = i * Mathf.PI * 2f / 5f + 0.3f;
                Vector3 basePos = new Vector3(Mathf.Cos(a) * 0.55f, 0.3f, Mathf.Sin(a) * 0.55f);
                var pin = Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.03f, 0.22f, 0.03f), PatchworkAssets.Plain(new Color(0.85f, 0.85f, 0.9f), 0.9f), default(Vector3), cushion);
                pin.localPosition = new Vector3(basePos.x / 2f, 0.9f, basePos.z / 2f);
                pin.localRotation = Quaternion.Euler(Mathf.Sin(a) * 25f, 0f, -Mathf.Cos(a) * 25f);
                pin.localScale = new Vector3(0.015f, 0.6f, 0.015f);
                var head = Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one, PatchworkAssets.Plain(heads[i], 0.6f), default(Vector3), pin);
                head.localPosition = new Vector3(0f, 1f, 0f);
                head.localScale = new Vector3(5f, 0.6f, 5f);
            }
            pad.Setup(cushion);
            return pad;
        }

        // A floating thread spool to collect.
        public ThreadSpool Thread(Vector3 pos, Color color)
        {
            Info.ThreadTotal++;
            var t = Volume<ThreadSpool>(pos, new Vector3(1.3f, 1.5f, 1.3f), "Thread");
            var vis = new GameObject("Visual").transform;
            vis.SetParent(t.transform, false);
            var wood = PatchworkAssets.Wood(new Color(0.85f, 0.68f, 0.45f));
            Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.2f, 0f), new Vector3(0.42f, 0.03f, 0.42f), wood, default(Vector3), vis);
            Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.32f, 0.17f, 0.32f), PatchworkAssets.Glow(color, 0.35f), default(Vector3), vis);
            Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.2f, 0f), new Vector3(0.42f, 0.03f, 0.42f), wood, default(Vector3), vis);
            t.Setup(vis, color);
            return t;
        }

        public void ThreadLine(Vector3 from, Vector3 to, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float k = count == 1 ? 0.5f : i / (float)(count - 1);
                Thread(Vector3.Lerp(from, to, k), Palette.Quilt[(i + Info.ThreadTotal) % Palette.Quilt.Length]);
            }
        }

        // ------------------------------------------------------------------ Garden decorations

        // A giant ball of yarn (solid) with a loose strand trailing on the ground.
        public void YarnBall(Vector3 groundPos, float radius, Color color)
        {
            var g = Group("YarnBall", groundPos + Vector3.up * radius * 0.9f);
            Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * radius * 2f, PatchworkAssets.Yarn(color), new Vector3(20f, 40f, 0f), g);
            var col = g.gameObject.AddComponent<SphereCollider>();
            col.radius = radius;
            var strand = PatchworkAssets.Yarn(color);
            Vector3 prev = groundPos + new Vector3(radius * 0.8f, 0.05f, 0f);
            for (int i = 1; i < 6; i++)
            {
                Vector3 next = prev + new Vector3(0.7f, 0f, Mathf.Sin(i * 1.3f) * 0.5f);
                Vector3 d = next - prev;
                var seg = Deco(PrimitiveType.Cylinder, (prev + next) * 0.5f, new Vector3(0.12f, d.magnitude * 0.5f, 0.12f), strand);
                seg.rotation = Quaternion.FromToRotation(Vector3.up, d.normalized);
                prev = next;
            }
        }

        // A giant knitting needle leaning against the scenery (decorative).
        public void KnittingNeedle(Vector3 a, Vector3 b, Color knob)
        {
            Vector3 d = b - a;
            var shaft = Deco(PrimitiveType.Cylinder, (a + b) * 0.5f, new Vector3(0.28f, d.magnitude * 0.5f, 0.28f), PatchworkAssets.Wood(new Color(0.85f, 0.7f, 0.5f)));
            shaft.rotation = Quaternion.FromToRotation(Vector3.up, d.normalized);
            Deco(PrimitiveType.Sphere, b, Vector3.one * 0.7f, PatchworkAssets.Plain(knob, 0.6f));
        }

        public void Thimble(Vector3 groundPos, float size)
        {
            var g = Group("Thimble", groundPos);
            var metal = PatchworkAssets.Plain(new Color(0.78f, 0.78f, 0.84f), 0.85f);
            Deco(PrimitiveType.Cylinder, new Vector3(0f, size * 0.5f, 0f), new Vector3(size, size * 0.5f, size), metal, default(Vector3), g);
            Deco(PrimitiveType.Sphere, new Vector3(0f, size, 0f), new Vector3(size, size * 0.4f, size), metal, default(Vector3), g);
            Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.05f * size, 0f), new Vector3(size * 1.06f, 0.05f * size, size * 1.06f), PatchworkAssets.Plain(Palette.Gold, 0.8f), default(Vector3), g);
            var col = g.gameObject.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, size * 0.6f, 0f);
            col.radius = size * 0.5f;
            col.height = size * 1.2f;
        }

        // ------------------------------------------------------------------ Set pieces

        // A tall stack of thread spools (solid). The top is flat so you can stand on it.
        public void SpoolTower(Vector3 groundPos, float height, Color[] threads)
        {
            int count = Mathf.Max(1, Mathf.RoundToInt(height / 2f));
            float h = height / count;
            var wood = PatchworkAssets.Wood(new Color(0.75f, 0.58f, 0.4f));
            for (int i = 0; i < count; i++)
            {
                float y = groundPos.y + i * h;
                Deco(PrimitiveType.Cylinder, new Vector3(groundPos.x, y + h * 0.5f, groundPos.z), new Vector3(2.2f, h * 0.5f - 0.15f, 2.2f), PatchworkAssets.Yarn(threads[i % threads.Length]));
                Deco(PrimitiveType.Cylinder, new Vector3(groundPos.x, y + 0.08f, groundPos.z), new Vector3(3f, 0.08f, 3f), wood);
            }
            Deco(PrimitiveType.Cylinder, new Vector3(groundPos.x, groundPos.y + height - 0.08f, groundPos.z), new Vector3(3f, 0.08f, 3f), wood);
            var col = Group("SpoolTowerCollider", groundPos + Vector3.up * height * 0.5f).gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(2.6f, height, 2.6f);
        }

        // ------------------------------------------------------------------ Enclosure

        static Transform NoShadow(Transform t)
        {
            var r = t.GetComponent<Renderer>();
            if (r != null) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return t;
        }

        public void Bunting(Vector3 a, Vector3 b, Color[] colors)
        {
            var g = Group("Bunting", a);
            Vector3 d = b - a;
            float len = d.magnitude;
            float sagMax = Mathf.Min(1.2f, len * 0.08f);
            System.Func<float, Vector3> P = k => Vector3.Lerp(a, b, k) + Vector3.down * (Mathf.Sin(k * Mathf.PI) * sagMax);
            var yarn = PatchworkAssets.Yarn(Palette.Cream);
            int segs = Mathf.Clamp(Mathf.RoundToInt(len / 1.5f), 2, 16);
            for (int i = 0; i < segs; i++)
            {
                Vector3 p0 = P(i / (float)segs), p1 = P((i + 1) / (float)segs);
                var str = Deco(PrimitiveType.Cylinder, (p0 + p1) * 0.5f, new Vector3(0.05f, (p1 - p0).magnitude * 0.5f + 0.02f, 0.05f), yarn);
                str.rotation = Quaternion.FromToRotation(Vector3.up, (p1 - p0).normalized);
                NoShadow(str);
            }
            int flags = Mathf.Max(2, Mathf.RoundToInt(len / 2.2f));
            for (int i = 1; i < flags; i++)
            {
                float k = i / (float)flags;
                Vector3 p = P(k) + Vector3.down * 0.45f;
                var flag = Deco(PrimitiveType.Cube, p, new Vector3(0.75f, 0.75f, 0.04f), PatchworkAssets.Fabric(colors[i % colors.Length]),
                    new Vector3(0f, Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg + 90f, 45f));
                NoShadow(flag);
            }
        }

        public void Lamp(Vector3 pos, float ceilingY)
        {
            NoShadow(Deco(PrimitiveType.Cylinder, new Vector3(pos.x, (pos.y + ceilingY) * 0.5f, pos.z), new Vector3(0.05f, (ceilingY - pos.y) * 0.5f, 0.05f),
                PatchworkAssets.Yarn(Palette.Walnut)));
            NoShadow(Deco(PrimitiveType.Sphere, pos + Vector3.up * 0.35f, new Vector3(1.6f, 0.7f, 1.6f), PatchworkAssets.Fabric(Palette.Butter)));
            NoShadow(Deco(PrimitiveType.Sphere, pos, Vector3.one * 0.7f, PatchworkAssets.Glow(new Color(1f, 0.85f, 0.6f), 2.5f)));
            var go = new GameObject("LampLight");
            go.transform.SetParent(Root, false);
            go.transform.position = pos + Vector3.down * 0.5f;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.82f, 0.6f);
            l.range = 22f;
            l.intensity = 1.4f;
            l.shadows = LightShadows.None;
        }
    }
}

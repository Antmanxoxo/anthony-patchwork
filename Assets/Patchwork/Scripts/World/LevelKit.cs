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
        public Color Sky = new Color(0.78f, 0.86f, 0.95f);
        public readonly List<string> CollectibleIds = new List<string>();

        public int CollectedCount()
        {
            int n = 0;
            foreach (var id in CollectibleIds) if (GameState.IsCollected(id)) n++;
            return n;
        }
    }

    /// <summary>
    /// Helpers for building handcrafted levels from code: quilted platforms, walls, and all the
    /// interactive pieces (anchors, grates, fountains, updrafts, checkpoints, buttons, goal, portal).
    /// </summary>
    public class LevelKit
    {
        public static class Palette
        {
            public static readonly Color Cream = new Color(0.96f, 0.9f, 0.78f);
            public static readonly Color Sage = new Color(0.62f, 0.78f, 0.6f);
            public static readonly Color Peach = new Color(0.98f, 0.72f, 0.6f);
            public static readonly Color Lavender = new Color(0.75f, 0.68f, 0.9f);
            public static readonly Color Sky = new Color(0.6f, 0.78f, 0.95f);
            public static readonly Color Butter = new Color(0.98f, 0.88f, 0.55f);
            public static readonly Color Rose = new Color(0.95f, 0.62f, 0.7f);
            public static readonly Color Denim = new Color(0.38f, 0.5f, 0.72f);
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

        /// <summary>A solid box with a stitched-fabric look.</summary>
        public Transform Block(Vector3 center, Vector3 size, Color color, string name = "Block")
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            var t = go.transform;
            t.SetParent(Root, false);
            t.position = center;
            t.localScale = size;
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = PatchworkAssets.Fabric(color);
            SetTiling(r, Mathf.Max(1f, Mathf.Round(size.x / 2.5f)), Mathf.Max(1f, Mathf.Round(Mathf.Max(size.y, size.z) / 2.5f)));
            return t;
        }

        /// <summary>
        /// A walkable platform: one invisible collider plus a grid of coloured quilt patches on top.
        /// "center"/"size" describe the whole slab.
        /// </summary>
        public Transform Quilt(Vector3 center, Vector3 size, Color[] palette, float cell = 3.5f, string name = "Platform")
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
                    p.GetComponent<MeshRenderer>().sharedMaterial = PatchworkAssets.Fabric(palette[pick]);
                }
            return slab;
        }

        static void SetTiling(Renderer r, float x, float y)
        {
            var mpb = new MaterialPropertyBlock();
            mpb.SetVector("_BaseMap_ST", new Vector4(x, y, 0f, 0f));
            r.SetPropertyBlock(mpb);
        }

        /// <summary>Decoration without collision.</summary>
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

        public void Flower(Vector3 pos, Color petals)
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

        public void Pinwheel(Vector3 basePos, Color a, Color b)
        {
            var g = Group("Pinwheel", basePos);
            Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.5f, 0f), new Vector3(0.08f, 1.5f, 0.08f), PatchworkAssets.Felt(Palette.Walnut), default(Vector3), g);
            var head = new GameObject("Head").transform;
            head.SetParent(g, false);
            head.localPosition = new Vector3(0f, 3.1f, 0.1f);
            for (int i = 0; i < 4; i++)
            {
                var blade = Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(0.35f, 0.9f, 0.03f), PatchworkAssets.Fabric(i % 2 == 0 ? a : b), new Vector3(0f, 0f, i * 90f), head);
                blade.localPosition = Quaternion.Euler(0f, 0f, i * 90f) * new Vector3(0f, 0.45f, 0f);
            }
            var spin = head.gameObject.AddComponent<Spinner>();
            spin.Axis = Vector3.forward;
            spin.Speed = 220f;
        }

        public void Cloud(Vector3 pos, float size)
        {
            var g = Group("Cloud", pos);
            var m = PatchworkAssets.Felt(new Color(1f, 1f, 1f));
            Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(2f, 1.1f, 1.3f) * size, m, default(Vector3), g);
            Deco(PrimitiveType.Sphere, new Vector3(0.9f, 0.3f, 0f) * size, new Vector3(1.4f, 1.2f, 1.1f) * size, m, default(Vector3), g);
            Deco(PrimitiveType.Sphere, new Vector3(-0.9f, 0.15f, 0.1f) * size, new Vector3(1.3f, 0.9f, 1f) * size, m, default(Vector3), g);
            var s = g.gameObject.AddComponent<Spinner>();
            s.Speed = 0f;
            s.BobHeight = 0.4f * size;
            s.BobSpeed = 0.4f;
        }

        /// <summary>Huge quilt far below the level so gaps feel deep (no collision).</summary>
        public void Backdrop(float y)
        {
            var g = Group("Backdrop", new Vector3(0f, y, 40f));
            int n = 8;
            float cell = 30f;
            for (int ix = 0; ix < n; ix++)
                for (int iz = 0; iz < n; iz++)
                {
                    Color c = Palette.Quilt[(ix * 3 + iz * 5) % Palette.Quilt.Length];
                    Deco(PrimitiveType.Cube, new Vector3((ix - n / 2 + 0.5f) * cell, 0f, (iz - n / 2 + 0.5f) * cell),
                        new Vector3(cell - 0.6f, 1f, cell - 0.6f), PatchworkAssets.Fabric(Color.Lerp(c, Palette.Lavender, 0.35f)), default(Vector3), g);
                }
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

        public YarnAnchor Anchor(Vector3 pos, float range = 13f)
        {
            var g = Group("YarnAnchor", pos);
            var anchor = g.gameObject.AddComponent<YarnAnchor>();
            anchor.Range = range;
            var vis = new GameObject("Visual").transform;
            vis.SetParent(g, false);
            var disc = Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.5f, 0.14f, 1.5f), PatchworkAssets.Felt(Palette.Rose), new Vector3(90f, 0f, 0f), vis);
            var rim = Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0f), new Vector3(1.75f, 0.1f, 1.75f), PatchworkAssets.Felt(Palette.Rose), new Vector3(90f, 0f, 0f), vis);
            var hole = PatchworkAssets.Plain(new Color(0.45f, 0.12f, 0.25f), 0.3f);
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * 0.22f;
                float y = (i < 2 ? -1f : 1f) * 0.22f;
                Deco(PrimitiveType.Cylinder, new Vector3(x, y, 0f), new Vector3(0.18f, 0.16f, 0.18f), hole, new Vector3(90f, 0f, 0f), vis);
            }
            // The thread it hangs from, disappearing into the sky.
            Deco(PrimitiveType.Cylinder, pos + Vector3.up * 20f, new Vector3(0.05f, 19.2f, 0.05f), PatchworkAssets.Yarn(Palette.Rose));
            anchor.Setup(vis, new Renderer[] { disc.GetComponent<Renderer>(), rim.GetComponent<Renderer>() }, new Color(1f, 0.45f, 0.65f));
            return anchor;
        }

        /// <summary>A blue drain grate filling an opening. "size" is the opening size.</summary>
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

        public WaterCurrent Fountain(Vector3 basePos, float height)
        {
            var current = Volume<WaterCurrent>(basePos + Vector3.up * height * 0.5f, new Vector3(2.4f, height, 2.4f), "Fountain");
            var water = PatchworkAssets.Water(FormLibrary.Get(FormType.Water).PrimaryColor);
            var light = PatchworkAssets.Water(FormLibrary.Get(FormType.Water).SecondaryColor);
            Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.15f, new Vector3(3f, 0.15f, 3f), PatchworkAssets.Felt(Palette.Denim));
            Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.3f, new Vector3(2.6f, 0.04f, 2.6f), water);
            var rings = new Transform[6];
            for (int i = 0; i < rings.Length; i++)
                rings[i] = Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(2.2f, 0.04f, 2.2f), i % 2 == 0 ? water : light, default(Vector3), current.transform);
            // A thin central jet.
            Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.35f, height * 0.5f, 0.35f), light, default(Vector3), current.transform);
            current.SetupRings(rings, height);
            return current;
        }

        public WindUpdraft Updraft(Vector3 basePos, float height)
        {
            var up = Volume<WindUpdraft>(basePos + Vector3.up * height * 0.5f, new Vector3(2.6f, height, 2.6f), "Updraft");
            Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.12f, new Vector3(2.6f, 0.12f, 2.6f), PatchworkAssets.Felt(new Color(0.85f, 0.92f, 0.95f)));
            Deco(PrimitiveType.Cylinder, basePos + Vector3.up * 0.25f, new Vector3(2.0f, 0.02f, 2.0f), PatchworkAssets.Plain(new Color(0.3f, 0.35f, 0.4f), 0.2f));
            var ribbon = PatchworkAssets.Glow(new Color(0.85f, 1f, 0.97f), 0.4f);
            var ribbons = new Transform[8];
            for (int i = 0; i < ribbons.Length; i++)
                ribbons[i] = Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(0.1f, 0.9f, 0.03f), ribbon, default(Vector3), up.transform);
            up.SetupRibbons(ribbons, height);
            return up;
        }

        public HintZone Hint(Vector3 center, Vector3 size, string title, string body)
        {
            var h = Volume<HintZone>(center, size, "Hint_" + title);
            h.Title = title;
            h.Body = body;
            return h;
        }

        public GoalZone Goal(Vector3 feet)
        {
            var goal = Volume<GoalZone>(feet + Vector3.up * 1.5f, new Vector3(4.5f, 4f, 2.5f), "Goal");
            // Stitched archway with a banner.
            Block(feet + new Vector3(-2.6f, 2.2f, 0f), new Vector3(0.6f, 4.4f, 0.6f), Palette.Rose, "ArchPost");
            Block(feet + new Vector3(2.6f, 2.2f, 0f), new Vector3(0.6f, 4.4f, 0.6f), Palette.Rose, "ArchPost");
            Block(feet + new Vector3(0f, 4.6f, 0f), new Vector3(6.2f, 0.6f, 0.6f), Palette.Butter, "ArchTop");
            Deco(PrimitiveType.Cube, feet + new Vector3(0f, 3.9f, 0.1f), new Vector3(4.2f, 0.8f, 0.05f), PatchworkAssets.Glow(Palette.Gold, 0.3f));
            Spool(feet + new Vector3(0f, 0f, 3f), 2f, Palette.Rose);
            return goal;
        }

        public Portal AddPortal(Vector3 feet, float yaw, string targetScene, bool completed)
        {
            Quaternion rot = Quaternion.Euler(0f, yaw, 0f);
            var portal = Volume<Portal>(feet + Vector3.up * 1.6f, new Vector3(3f, 3.2f, 1.6f), "Portal_" + targetScene);
            portal.TargetScene = targetScene;
            Block(feet + rot * new Vector3(-2f, 2f, 0f), new Vector3(0.7f, 4f, 0.7f), Palette.Denim, "PortalPost");
            Block(feet + rot * new Vector3(2f, 2f, 0f), new Vector3(0.7f, 4f, 0.7f), Palette.Denim, "PortalPost");
            Block(feet + rot * new Vector3(0f, 4.3f, 0f), new Vector3(4.7f, 0.7f, 0.7f), Palette.Lavender, "PortalTop");
            portal.Swirl = Deco(PrimitiveType.Cylinder, feet + Vector3.up * 2f, new Vector3(3.2f, 0.05f, 3.6f),
                PatchworkAssets.Glow(Palette.Lavender, 0.8f), new Vector3(90f, yaw, 0f));
            portal.Swirl.GetComponent<Renderer>().sharedMaterial.SetTexture("_BaseMap", PatchworkAssets.YarnTex);
            if (completed)
            {
                var star = Deco(PrimitiveType.Sphere, feet + Vector3.up * 5.3f, Vector3.one * 0.7f, PatchworkAssets.Glow(Palette.Gold, 1.2f));
                var s = star.gameObject.AddComponent<Spinner>();
                s.BobHeight = 0.2f;
            }
            return portal;
        }
    }
}

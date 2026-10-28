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
        // The room's walls (set by Enclosure). Used to size the reflection probe.
        public Bounds Area = new Bounds(Vector3.zero, Vector3.zero);
        public Color Sky = new Color(0.78f, 0.86f, 0.95f);
        public readonly List<string> CollectibleIds = new List<string>();
        public int ThreadTotal;

        public int CollectedCount()
        {
            int n = 0;
            foreach (var id in CollectibleIds) if (GameState.IsCollected(id)) n++;
            return n;
        }
    }

    // Helpers for building handcrafted levels from code: quilted platforms, walls, and all the
    // interactive pieces (anchors, grates, fountains, updrafts, checkpoints, buttons, goal, portal).
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

        static void SetTiling(Renderer r, float x, float y)
        {
            var mpb = new MaterialPropertyBlock();
            mpb.SetVector("_BaseMap_ST", new Vector4(x, y, 0f, 0f));
            r.SetPropertyBlock(mpb);
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
            // Warm cream, not white, so scenery clouds never look like Wind Form or an updraft.
            var m = PatchworkAssets.Felt(new Color(0.98f, 0.91f, 0.84f));
            Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(2f, 1.1f, 1.3f) * size, m, default(Vector3), g);
            Deco(PrimitiveType.Sphere, new Vector3(0.9f, 0.3f, 0f) * size, new Vector3(1.4f, 1.2f, 1.1f) * size, m, default(Vector3), g);
            Deco(PrimitiveType.Sphere, new Vector3(-0.9f, 0.15f, 0.1f) * size, new Vector3(1.3f, 0.9f, 1f) * size, m, default(Vector3), g);
            var s = g.gameObject.AddComponent<Spinner>();
            s.Speed = 0f;
            s.BobHeight = 0.4f * size;
            s.BobSpeed = 0.4f;
        }

        // Huge quilt far below the level so gaps feel deep (no collision).
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
            var disc = Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.5f, 0.14f, 1.5f), PatchworkAssets.Felt(Palette.AnchorPink), new Vector3(90f, 0f, 0f), vis);
            var rim = Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0f), new Vector3(1.75f, 0.1f, 1.75f), PatchworkAssets.Felt(Palette.AnchorPink), new Vector3(90f, 0f, 0f), vis);
            var hole = PatchworkAssets.Plain(new Color(0.45f, 0.12f, 0.25f), 0.3f);
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * 0.22f;
                float y = (i < 2 ? -1f : 1f) * 0.22f;
                Deco(PrimitiveType.Cylinder, new Vector3(x, y, 0f), new Vector3(0.18f, 0.16f, 0.18f), hole, new Vector3(90f, 0f, 0f), vis);
            }
            // The thread it hangs from, disappearing into the sky.
            Deco(PrimitiveType.Cylinder, pos + Vector3.up * 20f, new Vector3(0.05f, 19.2f, 0.05f), PatchworkAssets.Yarn(Palette.AnchorPink));
            anchor.Setup(vis, new Renderer[] { disc.GetComponent<Renderer>(), rim.GetComponent<Renderer>() }, new Color(1f, 0.45f, 0.65f));
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

        // A swirling embroidery-hoop portal: a wooden hoop with stitched trim, four counter-rotating
        // glowing rings, sparkles spiralling into the centre and a soft pulsing light.
        public Portal AddPortal(Vector3 feet, float yaw, string targetScene, bool completed)
        {
            Quaternion rot = Quaternion.Euler(0f, yaw, 0f);
            Vector3 center = feet + Vector3.up * 2.3f;
            var portal = Volume<Portal>(feet + Vector3.up * 1.6f, new Vector3(3f, 3.2f, 1.6f), "Portal_" + targetScene);
            portal.TargetScene = targetScene;

            var g = Group("PortalVisual", center);
            g.rotation = rot;

            // Embroidery hoop: ring of wooden segments with a stitched (alternating) trim and a brass clasp.
            var wood = PatchworkAssets.Felt(new Color(0.78f, 0.58f, 0.38f));
            var trim = PatchworkAssets.Felt(Palette.Rose);
            int segments = 28;
            float radius = 2.15f;
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                Vector3 pos = new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f);
                float len = 2f * Mathf.PI * radius / segments * 1.08f;
                Deco(PrimitiveType.Cube, pos, new Vector3(0.32f, len, 0.42f), wood, new Vector3(0f, 0f, a * Mathf.Rad2Deg), g);
                if (i % 2 == 0)
                    Deco(PrimitiveType.Cube, pos * 0.9f + Vector3.back * 0.22f, new Vector3(0.08f, len * 0.6f, 0.03f), trim, new Vector3(0f, 0f, a * Mathf.Rad2Deg + 90f), g);
            }
            Deco(PrimitiveType.Cube, new Vector3(0f, radius + 0.25f, 0f), new Vector3(0.6f, 0.45f, 0.6f), PatchworkAssets.Plain(new Color(0.9f, 0.75f, 0.35f), 0.8f), default(Vector3), g);
            Deco(PrimitiveType.Cylinder, new Vector3(0f, radius + 0.25f, 0f), new Vector3(0.18f, 0.45f, 0.18f), PatchworkAssets.Plain(new Color(0.75f, 0.6f, 0.25f), 0.8f), new Vector3(0f, 0f, 90f), g);

            // Feet: two little spools holding the hoop up.
            Spool(feet + rot * new Vector3(-1.5f, 0f, 0f), 0.8f, Palette.Lavender);
            Spool(feet + rot * new Vector3(1.5f, 0f, 0f), 0.8f, Palette.Sky);

            // Vortex rings (cylinders turned to face forward), largest at the back.
            Color[] colors = { Palette.Lavender, Palette.Rose, Palette.Sky, new Color(1f, 0.95f, 0.85f) };
            float[] sizes = { 3.9f, 3.0f, 2.1f, 1.1f };
            float[] speeds = { 50f, -85f, 140f, -220f };
            var rings = new Transform[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                var mat = PatchworkAssets.Unique(PatchworkAssets.Glow(colors[i], 0.9f + i * 0.3f));
                mat.SetTexture("_BaseMap", PatchworkTextures.Wrap.Albedo);
                rings[i] = Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.05f - i * 0.05f), new Vector3(sizes[i], 0.02f, sizes[i]), mat, new Vector3(90f, 0f, 0f), g);
                rings[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            // Sparkles that spiral inward forever.
            var motes = new Transform[14];
            var moteMat = PatchworkAssets.Glow(Color.white, 2f);
            for (int i = 0; i < motes.Length; i++)
            {
                motes[i] = Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.1f, moteMat, default(Vector3), g);
                motes[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            var lightGo = new GameObject("PortalLight");
            lightGo.transform.SetParent(g, false);
            lightGo.transform.localPosition = new Vector3(0f, 0f, -1.2f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.8f, 0.6f, 1f);
            light.range = 9f;
            light.intensity = 2f;
            light.shadows = LightShadows.None;

            portal.Setup(g, rings, speeds, motes, light);

            if (completed)
            {
                var star = Deco(PrimitiveType.Sphere, feet + Vector3.up * 5.2f, Vector3.one * 0.6f, PatchworkAssets.Glow(Palette.Gold, 1.2f));
                var s = star.gameObject.AddComponent<Spinner>();
                s.BobHeight = 0.2f;
            }
            return portal;
        }

        // ------------------------------------------------------------------ Interactive pieces

        // A squishy pin-cushion bounce pad, low enough to walk onto.
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

        // A big felt button that slides between two points. a/b are the top-centre positions.
        public MovingPlatform MovingButton(Vector3 a, Vector3 b, float phase, Color color, float travel = 3f)
        {
            var go = new GameObject("MovingButton");
            go.transform.SetParent(Root, false);
            var col = go.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, -0.25f, 0f);
            col.size = new Vector3(3.4f, 0.5f, 3.4f);
            Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.25f, 0f), new Vector3(3.6f, 0.25f, 3.6f), PatchworkAssets.Felt(color), default(Vector3), go.transform);
            Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.01f, 0f), new Vector3(2.8f, 0.02f, 2.8f), PatchworkAssets.Felt(Color.Lerp(color, Color.white, 0.25f)), default(Vector3), go.transform);
            var hole = PatchworkAssets.Plain(Color.Lerp(color, Color.black, 0.5f), 0.2f);
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * 0.45f;
                float z = (i < 2 ? -1f : 1f) * 0.45f;
                Deco(PrimitiveType.Cylinder, new Vector3(x, 0.02f, z), new Vector3(0.35f, 0.02f, 0.35f), hole, default(Vector3), go.transform);
            }
            var mp = go.AddComponent<MovingPlatform>();
            mp.PointA = a;
            mp.PointB = b;
            mp.Phase = phase;
            mp.TravelTime = travel;
            mp.Init();
            return mp;
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

        public void Mushroom(Vector3 groundPos, float height, Color cap)
        {
            var g = Group("FeltMushroom", groundPos);
            Deco(PrimitiveType.Cylinder, new Vector3(0f, height * 0.4f, 0f), new Vector3(height * 0.3f, height * 0.4f, height * 0.3f), PatchworkAssets.Felt(Palette.Cream), default(Vector3), g);
            var capT = Deco(PrimitiveType.Sphere, new Vector3(0f, height * 0.85f, 0f), new Vector3(height, height * 0.55f, height), PatchworkAssets.Felt(cap), default(Vector3), g);
            for (int i = 0; i < 5; i++)
            {
                float a = i * 1.3f;
                Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.3f, 0.35f, Mathf.Sin(a) * 0.3f), new Vector3(0.12f, 0.08f, 0.12f), PatchworkAssets.Felt(Color.white), default(Vector3), capT);
            }
        }

        // A big stitched felt sun pinned to a wall, facing direction "facing".
        public void FeltSun(Vector3 pos, float radius, Vector3 facing)
        {
            var g = Group("FeltSun", pos);
            g.rotation = Quaternion.LookRotation(facing, Vector3.up);
            var yellow = PatchworkAssets.Glow(new Color(1f, 0.85f, 0.35f), 0.35f);
            NoShadow(Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(radius * 2f, 0.1f, radius * 2f), yellow, new Vector3(90f, 0f, 0f), g));
            for (int i = 0; i < 12; i++)
            {
                float a = i * 30f;
                var ray = Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(radius * 0.25f, radius * 0.55f, 0.08f), PatchworkAssets.Felt(new Color(1f, 0.7f, 0.3f)), new Vector3(0f, 0f, a), g);
                ray.localPosition = Quaternion.Euler(0f, 0f, a) * new Vector3(0f, radius * 1.3f, 0.05f);
                NoShadow(ray);
            }
            // Sleepy happy face.
            var dark = PatchworkAssets.Plain(new Color(0.45f, 0.25f, 0.1f), 0.3f);
            for (int s = -1; s <= 1; s += 2)
                NoShadow(Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(radius * 0.3f, radius * 0.06f, 0.06f), dark, new Vector3(0f, 0f, s * 15f), g)).localPosition = new Vector3(s * radius * 0.35f, radius * 0.2f, 0.08f);
            for (int i = 0; i < 5; i++)
            {
                float k = i / 4f * 2f - 1f;
                var dot = Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * radius * 0.09f, dark, default(Vector3), g);
                dot.localPosition = new Vector3(k * radius * 0.35f, -radius * 0.25f - (1f - k * k) * radius * 0.12f, 0.08f);
            }
        }

        // ------------------------------------------------------------------ Enclosure

        // Walls and a ceiling around the whole play space so the world feels like a cosy sewing room
        // instead of an endless sky. Walls are striped quilt bands with giant buttons; the ceiling has
        // bunting and warm hanging lamps. Nothing here casts shadows, so the sunlight still reaches in.
        public void Enclosure(Vector3 min, Vector3 max, Color[] bands, Color ceiling, bool wicker = false)
        {
            Info.Area.SetMinMax(min, max);
            var g = Group("Enclosure", Vector3.zero);
            float t = 2f;
            float height = max.y - min.y;
            float bandH = height / bands.Length;
            float sx = max.x - min.x, sz = max.z - min.z;
            float cx = (min.x + max.x) * 0.5f, cz = (min.z + max.z) * 0.5f;

            for (int b = 0; b < bands.Length; b++)
            {
                float y = min.y + bandH * (b + 0.5f);
                Color c = bands[b];
                NoShadow(Block(new Vector3(min.x - t * 0.5f, y, cz), new Vector3(t, bandH, sz + t * 2f), c, "WallW", wicker ? PatchworkAssets.Wicker(c) : null));
                NoShadow(Block(new Vector3(max.x + t * 0.5f, y, cz), new Vector3(t, bandH, sz + t * 2f), c, "WallE", wicker ? PatchworkAssets.Wicker(c) : null));
                NoShadow(Block(new Vector3(cx, y, min.z - t * 0.5f), new Vector3(sx + t * 2f, bandH, t), c, "WallS", wicker ? PatchworkAssets.Wicker(c) : null));
                NoShadow(Block(new Vector3(cx, y, max.z + t * 0.5f), new Vector3(sx + t * 2f, bandH, t), c, "WallN", wicker ? PatchworkAssets.Wicker(c) : null));
                // Stitched seam between bands.
                if (b > 0)
                {
                    float sy = min.y + bandH * b;
                    var seam = PatchworkAssets.Felt(new Color(1f, 0.96f, 0.88f));
                    NoShadow(Deco(PrimitiveType.Cube, new Vector3(min.x + 0.05f, sy, cz), new Vector3(0.1f, 0.25f, sz), seam));
                    NoShadow(Deco(PrimitiveType.Cube, new Vector3(max.x - 0.05f, sy, cz), new Vector3(0.1f, 0.25f, sz), seam));
                    NoShadow(Deco(PrimitiveType.Cube, new Vector3(cx, sy, min.z + 0.05f), new Vector3(sx, 0.25f, 0.1f), seam));
                    NoShadow(Deco(PrimitiveType.Cube, new Vector3(cx, sy, max.z - 0.05f), new Vector3(sx, 0.25f, 0.1f), seam));
                }
            }
            NoShadow(Block(new Vector3(cx, max.y + t * 0.5f, cz), new Vector3(sx + t * 2f, t, sz + t * 2f), ceiling, "Ceiling", PatchworkAssets.Felt(ceiling)));

            // Giant sewing buttons pinned to the walls.
            Color[] buttonColors = { Palette.Gold, Palette.Rose, Palette.Sky, Palette.Peach, Palette.Lavender };
            int bi = 0;
            for (float z = min.z + 18f; z < max.z - 10f; z += 26f)
            {
                SewingButton(new Vector3(min.x + 0.2f, 9f + (bi % 2) * 6f, z), 4.5f, buttonColors[bi % buttonColors.Length], new Vector3(0f, 0f, -90f));
                SewingButton(new Vector3(max.x - 0.2f, 12f - (bi % 2) * 5f, z + 9f), 3.8f, buttonColors[(bi + 2) % buttonColors.Length], new Vector3(0f, 0f, 90f));
                bi++;
            }
            for (float x = min.x + 12f; x < max.x - 6f; x += 20f)
            {
                SewingButton(new Vector3(x, 10f, max.z - 0.2f), 4f, buttonColors[bi % buttonColors.Length], new Vector3(-90f, 0f, 0f));
                SewingButton(new Vector3(x + 6f, 13f, min.z + 0.2f), 3.4f, buttonColors[(bi + 1) % buttonColors.Length], new Vector3(90f, 0f, 0f));
                bi++;
            }

            // Bunting strings and lamps under the ceiling.
            float lampY = max.y - 4f;
            int li = 0;
            for (float z = min.z + 14f; z < max.z - 6f; z += 24f)
            {
                Bunting(new Vector3(min.x, max.y - 2.5f, z), new Vector3(max.x, max.y - 2.5f, z + 6f), Palette.Quilt);
                Lamp(new Vector3(cx + (li % 2 == 0 ? -6f : 6f), lampY, z + 10f), max.y);
                li++;
            }
        }

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
            var str = Deco(PrimitiveType.Cylinder, (a + b) * 0.5f, new Vector3(0.05f, len * 0.5f, 0.05f), PatchworkAssets.Yarn(Palette.Cream));
            str.rotation = Quaternion.FromToRotation(Vector3.up, d.normalized);
            NoShadow(str);
            int flags = Mathf.Max(2, Mathf.RoundToInt(len / 2.2f));
            for (int i = 1; i < flags; i++)
            {
                float k = i / (float)flags;
                float sag = Mathf.Sin(k * Mathf.PI) * 1.2f;
                Vector3 p = Vector3.Lerp(a, b, k) + Vector3.down * (sag + 0.45f);
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

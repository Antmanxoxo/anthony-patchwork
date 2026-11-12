using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Building blocks for the Oceanic Museum: stone walls with arched doorways, columns, vault
    // ribs, lanterns, banners, display cases and plaques, and everything that goes in a tank
    // (the glass, kelp, coral, fish, sharks, rays, turtles, jellyfish, bubbles, shafts of light).
    public static partial class Level2Builder
    {
        // ------------------------------------------------------------------ Colours and cloth

        static readonly Color StoneCol = new Color(0.8f, 0.7f, 0.56f);       // warm sandstone
        static readonly Color StoneDark = new Color(0.56f, 0.46f, 0.4f);
        static readonly Color StonePale = new Color(0.93f, 0.87f, 0.76f);
        static readonly Color Navy = new Color(0.13f, 0.16f, 0.36f);
        static readonly Color Plum = new Color(0.4f, 0.24f, 0.48f);
        static readonly Color Wine = new Color(0.5f, 0.18f, 0.3f);
        static readonly Color Teal = new Color(0.18f, 0.48f, 0.55f);
        static readonly Color CreamCol = new Color(0.96f, 0.91f, 0.8f);
        static readonly Color BoneCol = new Color(0.94f, 0.9f, 0.78f);
        static readonly Color BrassCol = new Color(0.9f, 0.72f, 0.38f);
        static readonly Color WoodCol = new Color(0.42f, 0.28f, 0.2f);
        static readonly Color[] FloorQuilt = { Navy, Plum, new Color(0.2f, 0.25f, 0.5f), new Color(0.3f, 0.2f, 0.42f), new Color(0.16f, 0.2f, 0.42f), Wine };
        static readonly Color[] WarmQuilt = { new Color(0.62f, 0.4f, 0.3f), new Color(0.72f, 0.52f, 0.36f), new Color(0.5f, 0.3f, 0.3f), new Color(0.66f, 0.46f, 0.4f), new Color(0.56f, 0.36f, 0.26f) };
        static readonly Color[] SeaQuilt = { new Color(0.1f, 0.2f, 0.34f), new Color(0.12f, 0.28f, 0.4f), new Color(0.08f, 0.16f, 0.3f), new Color(0.14f, 0.24f, 0.44f), new Color(0.12f, 0.33f, 0.38f) };

        static Material Sandstone { get { return PatchworkAssets.Brick(StoneCol); } }
        static Material Ashlar { get { return PatchworkAssets.Cobble(StoneCol); } }
        static Material DarkStone { get { return PatchworkAssets.Cobble(StoneDark); } }
        static Material PaleStone { get { return PatchworkAssets.Plaster(StonePale); } }
        static Material Brass { get { return PatchworkAssets.Plain(BrassCol, 0.82f); } }
        static Material IronMat { get { return PatchworkAssets.Plain(new Color(0.15f, 0.15f, 0.2f), 0.5f); } }
        static Material DarkWood { get { return PatchworkAssets.Planks(WoodCol); } }
        static Material Bone { get { return PatchworkAssets.Felt(BoneCol); } }
        static Material StarCloth { get { return PatchworkAssets.Print(PatchworkAssets.PrintKind.Stars, Navy, new Color(1f, 0.82f, 0.4f)); } }
        static Material PlumStars { get { return PatchworkAssets.Print(PatchworkAssets.PrintKind.Stars, Plum, new Color(1f, 0.85f, 0.5f)); } }
        static Material GlassMat { get { return Glass(new Color(0.6f, 0.85f, 0.95f, 0.13f), false); } }
        static Material CaseGlass { get { return PatchworkAssets.Bubble(new Color(0.85f, 0.95f, 1f, 0.12f)); } }

        static Quaternion Rot(float yaw) { return Quaternion.Euler(0f, yaw, 0f); }

        static Transform NoShadow(Transform t)
        {
            if (t == null) return null;
            var r = t.GetComponent<Renderer>();
            if (r != null) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return t;
        }

        // Materials made just for this level (rebuilt each time the level is).
        static readonly Dictionary<string, Material> special = new Dictionary<string, Material>();
        static readonly List<Texture2D> madeTextures = new List<Texture2D>();

        static void ResetKit()
        {
            special.Clear();
            madeTextures.Clear();
        }

        // A soft glowing sheet that fades from one colour at the bottom to another at the top:
        // the far wall of a tank (the water going on for ever), or the sea beyond a window.
        static Material Backdrop(Color bottom, Color top, float glow)
        {
            string key = "back" + ColorUtility.ToHtmlStringRGB(bottom) + ColorUtility.ToHtmlStringRGB(top) + glow.ToString("0.00");
            Material m;
            if (special.TryGetValue(key, out m) && m != null) return m;
            const int w = 64, h = 128;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                float v = y / (float)(h - 1);
                for (int x = 0; x < w; x++)
                {
                    float u = x / (float)w;
                    // Faint slanting streaks of light near the top, like sun through the surface.
                    float streak = Mathf.Pow(Mathf.Max(0f, Mathf.Sin((u * 5f + v * 0.9f) * Mathf.PI * 2f)), 6f) * v * v * 0.22f;
                    Color c = Color.Lerp(bottom, top, Mathf.SmoothStep(0f, 1f, v));
                    px[y * w + x] = c + new Color(streak, streak, streak, 0f);
                }
            }
            tex.SetPixels(px);
            tex.Apply(false);
            madeTextures.Add(tex);
            m = PatchworkAssets.Unique(PatchworkAssets.Glow(Color.white, 1f));
            m.name = "PW_" + key;
            m.SetColor("_BaseColor", new Color(0.04f, 0.05f, 0.08f));
            m.SetTexture("_BaseMap", tex);
            m.EnableKeyword("_EMISSION");
            m.SetTexture("_EmissionMap", tex);
            m.SetColor("_EmissionColor", Color.white * glow);
            m.SetFloat("_Smoothness", 0f);
            special[key] = m;
            return m;
        }

        // A see-through sheet of light (sunbeams in a tank, moonlight through a skylight).
        static Material Shaft(Color c, float alpha)
        {
            string key = "shaft" + ColorUtility.ToHtmlStringRGB(c) + alpha.ToString("0.00");
            Material m;
            if (special.TryGetValue(key, out m) && m != null) return m;
            m = PatchworkAssets.Unique(PatchworkAssets.Bubble(new Color(c.r, c.g, c.b, alpha)));
            m.name = "PW_" + key;
            m.SetTexture("_BaseMap", null);
            m.SetColor("_EmissionColor", c * 0.85f);
            m.SetFloat("_Smoothness", 0f);
            m.SetFloat("_EnvironmentReflections", 0f);
            m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            m.SetFloat("_SpecularHighlights", 0f);
            m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            m.SetFloat("_Cull", 0f);
            special[key] = m;
            return m;
        }

        // Tank and tunnel glass. It doesn't mirror the room (the game keeps one snapshot of the
        // level for shiny things to reflect, taken in the atrium, and gold lamplight sliding
        // over the glass of a tunnel on the sea bed would look very wrong).
        static Material Glass(Color c, bool bothSides)
        {
            string key = "glass" + ColorUtility.ToHtmlStringRGBA(c) + (bothSides ? "2" : "1");
            Material m;
            if (special.TryGetValue(key, out m) && m != null) return m;
            m = PatchworkAssets.Unique(PatchworkAssets.Bubble(c));
            m.name = "PW_" + key;
            m.SetFloat("_Smoothness", 0.8f);
            m.SetFloat("_EnvironmentReflections", 0f);
            m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            if (bothSides) m.SetFloat("_Cull", 0f);
            special[key] = m;
            return m;
        }

        // Glass you can see from both sides (domes).
        static Material DomeGlass(Color c) { return Glass(c, true); }

        // ------------------------------------------------------------------ Walls, doors, floors

        // A straight wall from a to b (along x or along z), from y0 up to y1.
        static Transform Wall(LevelKit kit, Vector3 a, Vector3 b, float y0, float y1, float thick, Material mat, string name = "Wall")
        {
            bool alongX = Mathf.Abs(b.x - a.x) > Mathf.Abs(b.z - a.z);
            float len = alongX ? Mathf.Abs(b.x - a.x) : Mathf.Abs(b.z - a.z);
            if (len < 0.05f || y1 - y0 < 0.05f) return null;
            Vector3 c = new Vector3((a.x + b.x) * 0.5f, (y0 + y1) * 0.5f, (a.z + b.z) * 0.5f);
            return kit.Block(c, alongX ? new Vector3(len, y1 - y0, thick) : new Vector3(thick, y1 - y0, len), StoneCol, name, mat);
        }

        // A wall with an arched doorway in it. "at" is how far along from a the middle of the
        // door is. The doorway is doorW wide and doorH high at the top of its arch, and sits on
        // floorY (so it can open onto an upper gallery).
        static void WallWithDoor(LevelKit kit, Vector3 a, Vector3 b, float y0, float y1, float thick, Material mat, float at, float doorW, float doorH, float floorY = 0f, Material trim = null)
        {
            bool alongX = Mathf.Abs(b.x - a.x) > Mathf.Abs(b.z - a.z);
            Vector3 dir = (b - a).normalized;
            float len = (b - a).magnitude;
            Vector3 d0 = a + dir * (at - doorW * 0.5f), d1 = a + dir * (at + doorW * 0.5f);
            Wall(kit, a, d0, y0, y1, thick, mat);
            Wall(kit, d1, b, y0, y1, thick, mat);
            Wall(kit, d0, d1, floorY + doorH, y1, thick, mat, "Lintel");
            if (floorY > y0 + 0.05f) Wall(kit, d0, d1, y0, floorY, thick, mat, "Sill");
            if (at < 0f || at > len) return;
            ArchTrim(kit, a + dir * at + Vector3.up * floorY, alongX ? 0f : 90f, doorW, doorH, thick, mat, trim != null ? trim : PaleStone);
        }

        // The stonework round an arched opening: fills the corners above the curve and rings it
        // with pale stones. yaw 0 = the wall runs along x.
        static void ArchTrim(LevelKit kit, Vector3 sill, float yaw, float w, float h, float thick, Material wall, Material trim)
        {
            var r = Rot(yaw);
            float rad = w * 0.5f;
            float spring = h - rad;
            // Corner fills, in slices.
            const int slices = 6;
            for (int s = -1; s <= 1; s += 2)
                for (int i = 0; i < slices; i++)
                {
                    float u0 = rad * i / slices, u1 = rad * (i + 1) / slices;
                    float um = (u0 + u1) * 0.5f;
                    float arc = Mathf.Sqrt(Mathf.Max(0f, rad * rad - u0 * u0));
                    if (rad - arc < 0.03f) continue;
                    kit.Deco(PrimitiveType.Cube, sill + r * new Vector3(s * um, spring + arc + (rad - arc) * 0.5f, 0f), new Vector3(u1 - u0 + 0.01f, rad - arc, thick + 0.02f), wall, new Vector3(0f, yaw, 0f));
                }
            // The ring of arch stones and the two jambs, standing a little proud of the wall.
            const int stones = 11;
            for (int i = 0; i < stones; i++)
            {
                float ang = Mathf.PI * (i + 0.5f) / stones;
                Vector3 p = new Vector3(Mathf.Cos(ang) * (rad + 0.3f), spring + Mathf.Sin(ang) * (rad + 0.3f), 0f);
                kit.Deco(PrimitiveType.Cube, sill + r * p, new Vector3(Mathf.PI * (rad + 0.3f) / stones + 0.04f, 0.62f, thick + 0.3f), i % 2 == 0 ? trim : wall,
                    new Vector3(0f, yaw, ang * Mathf.Rad2Deg - 90f));
            }
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cube, sill + r * new Vector3(s * (rad + 0.3f), spring * 0.5f, 0f), new Vector3(0.62f, spring, thick + 0.3f), trim, new Vector3(0f, yaw, 0f));
                kit.Deco(PrimitiveType.Cube, sill + r * new Vector3(s * (rad + 0.3f), 0.25f, 0f), new Vector3(0.9f, 0.5f, thick + 0.5f), trim, new Vector3(0f, yaw, 0f));
            }
            // Keystone.
            kit.Deco(PrimitiveType.Cube, sill + r * new Vector3(0f, h + 0.45f, 0f), new Vector3(0.7f, 1f, thick + 0.45f), trim, new Vector3(0f, yaw, 0f));
        }

        // A quilted floor: one solid slab with patchwork on top (top surface at y = top).
        static Transform Floor(LevelKit kit, float x0, float x1, float z0, float z1, float top, Color[] palette, float cell = 4.5f, string name = "Floor")
        {
            return kit.Quilt(new Vector3((x0 + x1) * 0.5f, top - 0.5f, (z0 + z1) * 0.5f), new Vector3(x1 - x0, 1f, z1 - z0), palette, cell, name);
        }

        // A flat rug or runner lying on the floor.
        static void Rug(LevelKit kit, Vector3 center, Vector2 size, Material mat, float yaw = 0f, Material border = null)
        {
            if (border != null) NoShadow(kit.Deco(PrimitiveType.Cube, center + Vector3.up * 0.08f, new Vector3(size.x + 0.5f, 0.03f, size.y + 0.5f), border, new Vector3(0f, yaw, 0f)));
            NoShadow(kit.Deco(PrimitiveType.Cube, center + Vector3.up * 0.1f, new Vector3(size.x, 0.04f, size.y), mat, new Vector3(0f, yaw, 0f)));
        }

        // A round column with a square base and a carved capital. Solid.
        static void Column(LevelKit kit, Vector3 foot, float height, float radius, Material mat = null, Material trim = null)
        {
            mat = mat != null ? mat : PaleStone;
            trim = trim != null ? trim : Ashlar;
            var g = kit.Group("Column", foot);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.3f, 0f), new Vector3(radius * 2.9f, 0.6f, radius * 2.9f), trim, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.72f, 0f), new Vector3(radius * 2.5f, 0.12f, radius * 2.5f), mat, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, height * 0.5f, 0f), new Vector3(radius * 2f, height * 0.5f, radius * 2f), mat, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, height - 0.75f, 0f), new Vector3(radius * 2.4f, 0.1f, radius * 2.4f), Brass, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, height - 0.45f, 0f), new Vector3(radius * 2.7f, 0.2f, radius * 2.7f), mat, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, height - 0.13f, 0f), new Vector3(radius * 3.1f, 0.26f, radius * 3.1f), trim, default(Vector3), g);
            var col = g.gameObject.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, height * 0.5f, 0f);
            col.radius = radius * 1.1f;
            col.height = height;
        }

        // A flight of steps going up in direction yaw from "bottom" (the foot of the first step).
        // Each step is solid all the way down to baseY, so there's never a gap underneath.
        static void Steps(LevelKit kit, Vector3 bottom, float yaw, int steps, float rise, float run, float width, Material mat, float baseY)
        {
            var r = Rot(yaw);
            for (int i = 0; i < steps; i++)
            {
                float top = bottom.y + rise * (i + 1);
                Vector3 c = bottom + r * new Vector3(0f, 0f, run * (i + 0.5f));
                c.y = (baseY + top) * 0.5f;
                var s = kit.Block(c, new Vector3(width, top - baseY, run), StoneCol, "Step", mat);
                s.rotation = r;
            }
        }

        // A ring of little blocks lying flat round "parent" (an armillary's hoop, an orrery's track).
        static void Hoop(LevelKit kit, Transform parent, float radius, float thick, Material mat, int segs = 28)
        {
            for (int i = 0; i < segs; i++)
            {
                float a = (i + 0.5f) / segs * Mathf.PI * 2f;
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius), new Vector3(thick, thick, Mathf.PI * 2f * radius / segs + thick * 0.5f), mat,
                    new Vector3(0f, -a * Mathf.Rad2Deg, 0f), parent));
            }
        }

        // A curved rib from a up over to b (an arch between columns, or a vault rib across a hall).
        static void Arc(LevelKit kit, Vector3 a, Vector3 b, float rise, float thick, float depth, Material mat, int segs = 12)
        {
            Vector3 prev = a;
            for (int i = 1; i <= segs; i++)
            {
                float k = i / (float)segs;
                Vector3 p = Vector3.Lerp(a, b, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * rise;
                Vector3 d = p - prev;
                var seg = kit.Deco(PrimitiveType.Cube, (p + prev) * 0.5f, new Vector3(depth, thick, d.magnitude + thick * 0.4f), mat);
                seg.rotation = Quaternion.LookRotation(d.normalized, Vector3.up);
                NoShadow(seg);
                prev = p;
            }
        }

        // A straight bar from a to b with a square section.
        static Transform Beam(LevelKit kit, Vector3 a, Vector3 b, float thick, Material mat)
        {
            Vector3 d = b - a;
            if (d.sqrMagnitude < 1e-6f) return null;
            var t = kit.Deco(PrimitiveType.Cube, (a + b) * 0.5f, new Vector3(thick, thick, d.magnitude), mat);
            t.rotation = Quaternion.LookRotation(d.normalized, Mathf.Abs(d.normalized.y) > 0.95f ? Vector3.forward : Vector3.up);
            return NoShadow(t);
        }

        // A low stone balustrade from a to b: a rail on little pillars, with a post at each end. Solid.
        static void Balustrade(LevelKit kit, Vector3 a, Vector3 b, float height = 1.1f, Material mat = null)
        {
            mat = mat != null ? mat : PaleStone;
            Vector3 d = b - a;
            float len = d.magnitude;
            if (len < 0.2f) return;
            float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            var r = Rot(yaw);
            Vector3 mid = (a + b) * 0.5f;
            kit.Deco(PrimitiveType.Cube, mid + Vector3.up * height, new Vector3(0.36f, 0.16f, len + 0.3f), mat, new Vector3(0f, yaw, 0f));
            kit.Deco(PrimitiveType.Cube, mid + Vector3.up * 0.1f, new Vector3(0.36f, 0.2f, len), mat, new Vector3(0f, yaw, 0f));
            int n = Mathf.Max(1, Mathf.RoundToInt(len / 0.7f));
            for (int i = 0; i < n; i++)
            {
                Vector3 p = a + d * ((i + 0.5f) / n);
                NoShadow(kit.Deco(PrimitiveType.Sphere, p + Vector3.up * (height * 0.5f), new Vector3(0.24f, height * 0.75f, 0.24f), mat));
            }
            foreach (var p in new[] { a, b })
            {
                kit.Deco(PrimitiveType.Cube, p + Vector3.up * (height * 0.55f), new Vector3(0.46f, height * 1.1f, 0.46f), mat, new Vector3(0f, yaw, 0f));
                NoShadow(kit.Deco(PrimitiveType.Sphere, p + Vector3.up * (height * 1.1f + 0.2f), Vector3.one * 0.44f, Brass));
            }
            TownKit.Solid(kit, mid + Vector3.up * (height * 0.5f), new Vector3(0.36f, height, len), yaw);
        }

        // ------------------------------------------------------------------ Lights and trimmings

        static Light PointLight(LevelKit kit, Vector3 pos, Color c, float range, float intensity)
        {
            var go = new GameObject("Lamp");
            go.transform.SetParent(kit.Root, false);
            go.transform.position = pos;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = c;
            l.range = range;
            l.intensity = intensity;
            l.shadows = LightShadows.None;
            return l;
        }

        static readonly Color LampGold = new Color(1f, 0.8f, 0.52f);

        // A brass lantern hanging on a chain from "ceilingY".
        static void HangingLantern(LevelKit kit, Vector3 pos, float ceilingY, float range = 18f, float intensity = 1.5f, bool light = true)
        {
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(pos.x, (pos.y + 0.6f + ceilingY) * 0.5f, pos.z), new Vector3(0.05f, (ceilingY - pos.y - 0.6f) * 0.5f, 0.05f), IronMat));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, pos + Vector3.up * 0.55f, new Vector3(0.7f, 0.06f, 0.7f), Brass));
            NoShadow(kit.Deco(PrimitiveType.Sphere, pos + Vector3.up * 0.75f, new Vector3(0.36f, 0.3f, 0.36f), Brass));
            NoShadow(kit.Deco(PrimitiveType.Sphere, pos, new Vector3(0.8f, 0.95f, 0.8f), PatchworkAssets.Glow(LampGold, 2.2f)));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, pos + Vector3.down * 0.5f, new Vector3(0.5f, 0.05f, 0.5f), Brass));
            for (int i = 0; i < 4; i++)
            {
                float a = i * 90f + 45f;
                NoShadow(kit.Deco(PrimitiveType.Cube, pos + Rot(a) * new Vector3(0f, 0f, 0.4f), new Vector3(0.04f, 1f, 0.04f), Brass));
            }
            if (light) PointLight(kit, pos + Vector3.down * 0.3f, LampGold, range, intensity);
        }

        // A lamp on a bracket sticking out of a wall (the wall is behind it; yaw faces out).
        static void Sconce(LevelKit kit, Vector3 wallPos, float yaw, bool light = true, float range = 13f)
        {
            var r = Rot(yaw);
            NoShadow(kit.Deco(PrimitiveType.Cube, wallPos + r * new Vector3(0f, 0f, 0.3f), new Vector3(0.08f, 0.08f, 0.6f), Brass, new Vector3(0f, yaw, 0f)));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, wallPos + r * new Vector3(0f, 0.05f, 0.6f), new Vector3(0.36f, 0.05f, 0.36f), Brass));
            NoShadow(kit.Deco(PrimitiveType.Sphere, wallPos + r * new Vector3(0f, 0.4f, 0.6f), new Vector3(0.5f, 0.6f, 0.5f), PatchworkAssets.Glow(LampGold, 2f)));
            if (light) PointLight(kit, wallPos + r * new Vector3(0f, 0.4f, 1.1f), LampGold, range, 1.2f);
        }

        // A lamp standing on the floor: an iron post with a glowing globe. Solid.
        static void FloorLamp(LevelKit kit, Vector3 foot, bool light = true, float height = 3.4f)
        {
            var g = kit.Group("FloorLamp", foot);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.12f, 0f), new Vector3(0.7f, 0.12f, 0.7f), IronMat, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, height * 0.5f, 0f), new Vector3(0.12f, height * 0.5f, 0.12f), IronMat, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, height, 0f), new Vector3(0.5f, 0.05f, 0.5f), Brass, default(Vector3), g);
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, height + 0.4f, 0f), new Vector3(0.7f, 0.8f, 0.7f), PatchworkAssets.Glow(LampGold, 2.2f), default(Vector3), g));
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, height + 0.85f, 0f), Vector3.one * 0.18f, Brass, default(Vector3), g);
            var col = g.gameObject.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, height * 0.5f, 0f);
            col.radius = 0.2f;
            col.height = height;
            if (light) PointLight(kit, foot + Vector3.up * (height + 0.4f), LampGold, 15f, 1.4f);
        }

        // A long banner hanging from a gold rod, with a swallow-tail and a big felt emblem.
        // "top" is the middle of the rod; yaw is the way its face points.
        static void Banner(LevelKit kit, Vector3 top, float yaw, float width, float length, Material cloth, int emblem = 0)
        {
            var g = kit.Group("Banner", top);
            g.rotation = Rot(yaw);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.12f, width * 0.5f + 0.25f, 0.12f), Brass, new Vector3(0f, 0f, 90f), g));
            for (int s = -1; s <= 1; s += 2)
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * (width * 0.5f + 0.3f), 0f, 0f), Vector3.one * 0.26f, Brass, default(Vector3), g));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, -length * 0.5f, 0f), new Vector3(width, length, 0.06f), cloth, default(Vector3), g));
            // Swallow-tail: two points at the bottom.
            float tail = width * 0.36f;
            for (int s = -1; s <= 1; s += 2)
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * width * 0.25f, -length, 0f), new Vector3(tail, tail, 0.06f), cloth, new Vector3(0f, 0f, 45f), g));
            // A gold border stitch and the emblem.
            var gold = PatchworkAssets.Felt(new Color(1f, 0.82f, 0.4f));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.35f, 0.04f), new Vector3(width * 0.86f, 0.1f, 0.02f), gold, default(Vector3), g));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, -length + 0.25f, 0.04f), new Vector3(width * 0.86f, 0.1f, 0.02f), gold, default(Vector3), g));
            Vector3 e = new Vector3(0f, -length * 0.45f, 0.045f);
            float s2 = width * 0.3f;
            switch (emblem)
            {
                case 1:   // a crescent moon
                    NoShadow(kit.Deco(PrimitiveType.Cylinder, e, new Vector3(s2 * 2f, 0.012f, s2 * 2f), gold, new Vector3(90f, 0f, 0f), g));
                    NoShadow(kit.Deco(PrimitiveType.Cylinder, e + new Vector3(s2 * 0.42f, s2 * 0.2f, 0.01f), new Vector3(s2 * 1.6f, 0.014f, s2 * 1.6f), cloth, new Vector3(90f, 0f, 0f), g));
                    break;
                case 2:   // a whale
                    NoShadow(kit.Deco(PrimitiveType.Sphere, e, new Vector3(s2 * 2.6f, s2 * 1.2f, 0.03f), gold, default(Vector3), g));
                    NoShadow(kit.Deco(PrimitiveType.Cube, e + new Vector3(-s2 * 1.4f, s2 * 0.35f, 0f), new Vector3(s2 * 0.7f, s2 * 0.25f, 0.03f), gold, new Vector3(0f, 0f, -35f), g));
                    NoShadow(kit.Deco(PrimitiveType.Cube, e + new Vector3(-s2 * 1.4f, -s2 * 0.1f, 0f), new Vector3(s2 * 0.7f, s2 * 0.25f, 0.03f), gold, new Vector3(0f, 0f, 30f), g));
                    NoShadow(kit.Deco(PrimitiveType.Sphere, e + new Vector3(s2 * 0.75f, s2 * 0.15f, 0.02f), Vector3.one * s2 * 0.16f, PatchworkAssets.Felt(Navy), default(Vector3), g));
                    break;
                case 3:   // a shell spiral
                    for (int i = 0; i < 9; i++)
                    {
                        float a = i * 0.8f, rr = s2 * (0.12f + i * 0.1f);
                        NoShadow(kit.Deco(PrimitiveType.Sphere, e + new Vector3(Mathf.Cos(a) * rr, Mathf.Sin(a) * rr, 0f), new Vector3(s2 * (0.24f + i * 0.05f), s2 * (0.24f + i * 0.05f), 0.03f), gold, default(Vector3), g));
                    }
                    break;
                default:  // a star: two crossed diamonds
                    NoShadow(kit.Deco(PrimitiveType.Cube, e, new Vector3(s2 * 1.5f, s2 * 1.5f, 0.02f), gold, new Vector3(0f, 0f, 45f), g));
                    NoShadow(kit.Deco(PrimitiveType.Cube, e, new Vector3(s2 * 0.5f, s2 * 2.6f, 0.02f), gold, new Vector3(0f, 0f, 0f), g));
                    NoShadow(kit.Deco(PrimitiveType.Cube, e, new Vector3(s2 * 2.6f, s2 * 0.5f, 0.02f), gold, new Vector3(0f, 0f, 0f), g));
                    break;
            }
        }

        // A tall arched window showing the night outside (or deep water), with a stone frame.
        // "foot" is the middle of its sill on the wall's face; yaw faces into the room.
        static void NightWindow(LevelKit kit, Vector3 foot, float yaw, float w, float h, Color glow, float strength = 0.9f)
        {
            var r = Rot(yaw);
            var pane = PatchworkAssets.Glow(glow, strength);
            float rectH = h - w * 0.5f;
            NoShadow(kit.Deco(PrimitiveType.Cube, foot + r * new Vector3(0f, rectH * 0.5f, 0.06f), new Vector3(w, rectH, 0.06f), pane, new Vector3(0f, yaw, 0f)));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, foot + r * new Vector3(0f, rectH, 0.05f), new Vector3(w, 0.03f, w), pane, new Vector3(90f, yaw, 0f)));
            // Lead bars.
            var lead = IronMat;
            NoShadow(kit.Deco(PrimitiveType.Cube, foot + r * new Vector3(0f, h * 0.5f, 0.12f), new Vector3(0.1f, h, 0.06f), lead, new Vector3(0f, yaw, 0f)));
            for (float y = rectH; y > 0.5f; y -= Mathf.Max(1.5f, rectH / 3f))
                NoShadow(kit.Deco(PrimitiveType.Cube, foot + r * new Vector3(0f, y, 0.12f), new Vector3(w, 0.1f, 0.06f), lead, new Vector3(0f, yaw, 0f)));
            // Frame.
            var stone = PaleStone;
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, foot + r * new Vector3(s * (w * 0.5f + 0.15f), rectH * 0.5f, 0.1f), new Vector3(0.3f, rectH, 0.3f), stone, new Vector3(0f, yaw, 0f));
            kit.Deco(PrimitiveType.Cube, foot + r * new Vector3(0f, -0.12f, 0.15f), new Vector3(w + 0.8f, 0.24f, 0.5f), stone, new Vector3(0f, yaw, 0f));
            const int stones = 9;
            for (int i = 0; i < stones; i++)
            {
                float ang = Mathf.PI * (i + 0.5f) / stones;
                float rad = w * 0.5f + 0.15f;
                kit.Deco(PrimitiveType.Cube, foot + r * new Vector3(Mathf.Cos(ang) * rad, rectH + Mathf.Sin(ang) * rad, 0.1f), new Vector3(Mathf.PI * rad / stones + 0.03f, 0.3f, 0.3f), stone,
                    new Vector3(0f, yaw, ang * Mathf.Rad2Deg - 90f));
            }
        }

        // Brass posts with a velvet rope between them, from a to b. (You can hop it.)
        static void RopeLine(LevelKit kit, Vector3 a, Vector3 b, Color rope)
        {
            Vector3 d = b - a;
            int n = Mathf.Max(1, Mathf.RoundToInt(d.magnitude / 2.4f));
            var velvet = PatchworkAssets.Yarn(rope);
            for (int i = 0; i <= n; i++)
            {
                Vector3 p = a + d * (i / (float)n);
                var g = kit.Group("Stanchion", p);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.04f, 0f), new Vector3(0.5f, 0.04f, 0.5f), Brass, default(Vector3), g);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.5f, 0f), new Vector3(0.09f, 0.5f, 0.09f), Brass, default(Vector3), g);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.05f, 0f), Vector3.one * 0.2f, Brass, default(Vector3), g);
                if (i < n)
                {
                    Vector3 q = a + d * ((i + 1) / (float)n);
                    Vector3 m = (p + q) * 0.5f + Vector3.up * 0.72f;
                    kit.Rope(p + Vector3.up * 0.95f, m, velvet, 0.09f);
                    kit.Rope(m, q + Vector3.up * 0.95f, velvet, 0.09f);
                }
            }
        }

        // ------------------------------------------------------------------ Exhibits

        // A glass display case on a wooden plinth. Returns the middle of the plinth's top (put
        // the exhibit there). Solid, and flat on top so you can stand on it.
        static Vector3 DisplayCase(LevelKit kit, Vector3 foot, Vector3 size, float yaw = 0f, bool lit = true)
        {
            var r = Rot(yaw);
            const float plinth = 0.9f;
            var g = kit.Group("DisplayCase", foot);
            g.rotation = r;
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, plinth * 0.5f, 0f), new Vector3(size.x, plinth, size.z), DarkWood, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, plinth + 0.03f, 0f), new Vector3(size.x + 0.1f, 0.06f, size.z + 0.1f), Brass, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.08f, 0f), new Vector3(size.x + 0.14f, 0.16f, size.z + 0.14f), Brass, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, plinth + 0.07f, 0f), new Vector3(size.x - 0.2f, 0.03f, size.z - 0.2f), PatchworkAssets.Felt(Wine), default(Vector3), g);
            float gh = size.y - plinth;
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, plinth + gh * 0.5f, 0f), new Vector3(size.x - 0.08f, gh, size.z - 0.08f), CaseGlass, default(Vector3), g));
            // Brass edges.
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(sx * (size.x * 0.5f - 0.04f), plinth + gh * 0.5f, sz * (size.z * 0.5f - 0.04f)), new Vector3(0.07f, gh, 0.07f), Brass, default(Vector3), g));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, size.y, 0f), new Vector3(size.x, 0.08f, size.z), Brass, default(Vector3), g));
            var col = g.gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, size.y * 0.5f, 0f);
            col.size = new Vector3(size.x, size.y + 0.08f, size.z);
            if (lit) PointLight(kit, foot + Vector3.up * (size.y - 0.3f), new Color(1f, 0.9f, 0.7f), 5f, 0.9f);
            return foot + Vector3.up * (plinth + 0.09f);
        }

        // A stone plinth to stand a big exhibit on. Solid. Returns the middle of its top.
        static Vector3 Plinth(LevelKit kit, Vector3 foot, Vector3 size, float yaw = 0f, Material mat = null)
        {
            mat = mat != null ? mat : Ashlar;
            var b = kit.Block(foot + Vector3.up * size.y * 0.5f, size, StoneCol, "Plinth", mat);
            b.rotation = Rot(yaw);
            // (The cap band stands 3 cm proud of the top, so the two tops never flicker.)
            kit.Deco(PrimitiveType.Cube, foot + Vector3.up * (size.y - 0.03f), new Vector3(size.x + 0.2f, 0.12f, size.z + 0.2f), PaleStone, new Vector3(0f, yaw, 0f));
            kit.Deco(PrimitiveType.Cube, foot + Vector3.up * 0.1f, new Vector3(size.x + 0.2f, 0.2f, size.z + 0.2f), PaleStone, new Vector3(0f, yaw, 0f));
            return foot + Vector3.up * size.y;
        }

        // A brass plaque on a little stand. Walk up and press Ability to read it.
        // yaw is the way the plaque faces (towards the reader).
        static void Plaque(LevelKit kit, Vector3 foot, float yaw, string title, params string[] text)
        {
            var g = kit.Group("Plaque", foot);
            g.rotation = Rot(yaw);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.04f, 0f), new Vector3(0.5f, 0.04f, 0.5f), Brass, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.5f, 0f), new Vector3(0.08f, 0.5f, 0.08f), Brass, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.05f, 0.02f), new Vector3(0.95f, 0.62f, 0.06f), Brass, new Vector3(-35f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.07f, 0.045f), new Vector3(0.82f, 0.5f, 0.04f), PatchworkAssets.Felt(new Color(0.16f, 0.14f, 0.2f)), new Vector3(-35f, 0f, 0f), g);
            var ink = PatchworkAssets.Felt(new Color(1f, 0.86f, 0.5f));
            for (int i = 0; i < 4; i++)
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(i == 0 ? 0f : -0.05f, 1.2f - i * 0.1f, 0.19f - i * 0.07f), new Vector3(i == 0 ? 0.5f : 0.62f - (i % 2) * 0.12f, 0.035f, 0.02f), ink, new Vector3(-35f, 0f, 0f), g));
            var col = g.gameObject.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.6f, 0f);
            col.radius = 0.16f;
            col.height = 1.2f;
            var plaque = g.gameObject.AddComponent<ExhibitPlaque>();
            plaque.Title = title;
            plaque.Text = text;
            plaque.Head = foot + Vector3.up * 0.5f;
            Vector3 stand = foot + Rot(yaw) * new Vector3(0f, 0f, 0.9f);
            var zone = kit.Volume<TalkZone>(stand + Vector3.up * 1.2f, new Vector3(2.6f, 3f, 2.6f), "Read_" + title);
            zone.Npc = plaque;
        }

        static ToyNpc Person(LevelKit kit, Vector3 feet, float yaw, ToyNpc.Kind kind, string name, Color main, Color accent, float pitch, System.Func<string[]> lines)
        {
            var npc = Level1Builder.Toy(kit, feet, yaw, kind, name, main, accent, ToyNpc.Instrument.None, pitch, Color.Lerp(main, Navy, 0.45f));
            npc.Lines = lines;
            return npc;
        }

        // A line (or curve) of score bubbles that counts as one set.
        static void Trail(LevelKit kit, int value, params Vector3[] points)
        {
            ScoreBubbles.BeginSet(kit);
            foreach (var p in points) ScoreBubbles.Place(kit, p, value);
            ScoreBubbles.EndSet();
        }

        static void TrailLine(LevelKit kit, Vector3 a, Vector3 b, int count, int value = 10)
        {
            var pts = new Vector3[count];
            for (int i = 0; i < count; i++) pts[i] = Vector3.Lerp(a, b, count == 1 ? 0.5f : i / (float)(count - 1));
            Trail(kit, value, pts);
        }

        // A round patchwork platform on a brass-ringed drum. "top" is the middle of its top. Solid.
        static Transform RoundPlatform(LevelKit kit, Vector3 top, float radius, float thick, Material cloth, Material side = null, bool star = true)
        {
            var g = kit.Group("RoundPlatform", top);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -thick * 0.5f, 0f), new Vector3(radius * 2f, thick * 0.5f, radius * 2f), side != null ? side : PatchworkAssets.Felt(Navy), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.02f, 0f), new Vector3(radius * 1.9f, 0.03f, radius * 1.9f), cloth, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.1f, 0f), new Vector3(radius * 2.08f, 0.07f, radius * 2.08f), Brass, default(Vector3), g);
            if (star)
            {
                var gold = PatchworkAssets.Felt(new Color(1f, 0.82f, 0.4f));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.015f, 0f), new Vector3(radius * 0.9f, 0.012f, radius * 0.9f), gold, new Vector3(0f, 45f, 0f), g));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.017f, 0f), new Vector3(radius * 0.3f, 0.012f, radius * 1.5f), gold, default(Vector3), g));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.017f, 0f), new Vector3(radius * 1.5f, 0.012f, radius * 0.3f), gold, default(Vector3), g));
            }
            // An eight-sided solid, a touch inside the rim.
            var mesh = PrismMesh(radius * 0.97f, thick, 10);
            var col = g.gameObject.AddComponent<MeshCollider>();
            col.sharedMesh = mesh;
            col.convex = true;
            g.gameObject.AddComponent<OwnedMesh>().Mesh = mesh;
            return g;
        }

        // A plain many-sided drum mesh (top at y = 0, going down "thick"), for round colliders.
        static Mesh PrismMesh(float radius, float thick, int sides)
        {
            var v = new List<Vector3>();
            var tris = new List<int>();
            for (int i = 0; i < sides; i++)
            {
                float a = i / (float)sides * Mathf.PI * 2f;
                v.Add(new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius));
                v.Add(new Vector3(Mathf.Cos(a) * radius, -thick, Mathf.Sin(a) * radius));
            }
            for (int i = 0; i < sides; i++)
            {
                int a = i * 2, b = ((i + 1) % sides) * 2;
                tris.Add(a); tris.Add(b); tris.Add(a + 1);
                tris.Add(b); tris.Add(b + 1); tris.Add(a + 1);
                if (i > 0 && i < sides - 1)
                {
                    tris.Add(0); tris.Add(b); tris.Add(a);
                    tris.Add(1); tris.Add(a + 1); tris.Add(b + 1);
                }
            }
            var m = new Mesh();
            m.name = "Drum";
            m.SetVertices(v);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        // Half a ball, open at the bottom: a glass dome (seen from both sides) or, turned inside
        // out, the painted inside of a sky. v runs from 0 at the rim to 1 at the top. az0..az1
        // (degrees, 0 = east, 90 = north) leaves a slice of it out.
        static Transform Dome(LevelKit kit, Vector3 center, Vector3 radii, Material mat, bool inward, string name = "Dome", int rings = 10, int segs = 28, float az0 = 0f, float az1 = 360f)
        {
            var v = new List<Vector3>();
            var nrm = new List<Vector3>();
            var uv = new List<Vector2>();
            var tris = new List<int>();
            for (int ri = 0; ri <= rings; ri++)
            {
                float el = ri / (float)rings * Mathf.PI * 0.5f;
                for (int si = 0; si <= segs; si++)
                {
                    float az = Mathf.Lerp(az0, az1, si / (float)segs) * Mathf.Deg2Rad;
                    Vector3 d = new Vector3(Mathf.Cos(el) * Mathf.Cos(az), Mathf.Sin(el), Mathf.Cos(el) * Mathf.Sin(az));
                    v.Add(new Vector3(d.x * radii.x, d.y * radii.y, d.z * radii.z));
                    nrm.Add(inward ? -d : d);
                    uv.Add(new Vector2(si / (float)segs, ri / (float)rings));
                }
            }
            int row = segs + 1;
            for (int ri = 0; ri < rings; ri++)
                for (int si = 0; si < segs; si++)
                {
                    int a = ri * row + si, b = a + 1, c = a + row, d = c + 1;
                    if (inward) { tris.Add(a); tris.Add(b); tris.Add(c); tris.Add(b); tris.Add(d); tris.Add(c); }
                    else { tris.Add(a); tris.Add(c); tris.Add(b); tris.Add(b); tris.Add(c); tris.Add(d); }
                }
            var mesh = new Mesh();
            mesh.name = name;
            mesh.SetVertices(v);
            mesh.SetNormals(nrm);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            var go = new GameObject(name);
            go.transform.SetParent(kit.Root, false);
            go.transform.position = center;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.AddComponent<OwnedMesh>().Mesh = mesh;
            return go.transform;
        }

        // ------------------------------------------------------------------ Tanks

        // One aquarium tank, looked into through a wall of glass.
        class TankSpace
        {
            public LevelKit Kit;
            public Vector3 Origin;       // the middle of the bottom edge of the glass
            public float Yaw;            // the way the glass faces (towards whoever's looking)
            public float Width, Height, Depth, FloorY;
            public System.Random Rng;

            // Tank space to world: x along the glass, y up from the tank floor, z back into the tank.
            public Vector3 At(float x, float y, float z)
            {
                return new Vector3(Origin.x, FloorY, Origin.z) + Quaternion.Euler(0f, Yaw, 0f) * new Vector3(x, y, -z);
            }

            public float R(float a, float b) { return a + (float)Rng.NextDouble() * (b - a); }
            public bool Turned { get { return Mathf.RoundToInt(Mathf.Repeat(Yaw, 360f) / 90f) % 2 == 1; } }
        }

        // Builds the glass (with its iron frame), the dim walls and sandy floor behind it, the
        // glowing far wall, the bright surface overhead, slanting shafts of light and a soft
        // light that spills out onto the floor in front. floorDrop: how far below the bottom of
        // the glass the tank floor is.
        static TankSpace Tank(LevelKit kit, Vector3 glassFoot, float yaw, float width, float height, float depth, int panesX, int panesY,
            Color deep, Color shallow, float floorDrop = 0f, bool frame = true, bool glass = true, int seed = 1)
        {
            var t = new TankSpace { Kit = kit, Origin = glassFoot, Yaw = yaw, Width = width, Height = height + floorDrop, Depth = depth, FloorY = glassFoot.y - floorDrop, Rng = new System.Random(seed * 7919 + 13) };
            var r = Rot(yaw);
            if (glass)
            {
                var pane = kit.Block(glassFoot + Vector3.up * height * 0.5f, new Vector3(width, height, 0.3f), Color.white, "TankGlass", GlassMat);
                pane.rotation = r;
                NoShadow(pane);
            }
            if (frame)
            {
                var iron = IronMat;
                for (int i = 0; i <= panesX; i++)
                {
                    float x = -width * 0.5f + width * i / panesX;
                    bool edge = i == 0 || i == panesX;
                    NoShadow(kit.Deco(PrimitiveType.Cube, glassFoot + r * new Vector3(x, height * 0.5f, 0.1f), new Vector3(edge ? 0.5f : 0.26f, height, 0.5f), iron, new Vector3(0f, yaw, 0f)));
                }
                for (int j = 0; j <= panesY; j++)
                {
                    float y = height * j / panesY;
                    bool edge = j == 0 || j == panesY;
                    NoShadow(kit.Deco(PrimitiveType.Cube, glassFoot + r * new Vector3(0f, y, 0.1f), new Vector3(width + 0.5f, edge ? 0.5f : 0.22f, 0.5f), iron, new Vector3(0f, yaw, 0f)));
                }
                // A brass kick-rail along the bottom.
                NoShadow(kit.Deco(PrimitiveType.Cube, glassFoot + r * new Vector3(0f, 0.32f, 0.42f), new Vector3(width, 0.1f, 0.1f), Brass, new Vector3(0f, yaw, 0f)));
            }
            // The far wall glows (deep at the bottom, light at the top); the sides are darker.
            var far = Backdrop(deep, shallow, 0.95f);
            var sides = Backdrop(deep * 0.7f, Color.Lerp(deep, shallow, 0.45f), 0.7f);
            float h = t.Height;
            NoShadow(kit.Deco(PrimitiveType.Cube, t.At(0f, h * 0.5f, depth), new Vector3(width + 1f, h, 0.2f), far, new Vector3(0f, yaw, 0f)));
            for (int s = -1; s <= 1; s += 2)
                NoShadow(kit.Deco(PrimitiveType.Cube, t.At(s * (width * 0.5f + 0.1f), h * 0.5f, depth * 0.5f), new Vector3(0.2f, h, depth), sides, new Vector3(0f, yaw, 0f)));
            // The top of the tank, like a real one: the water stops a little below the top of the
            // glass (its surface seen from underneath, silvery and rippled), then a band of air,
            // then the dark roof of the tank with a row of lamps hung under it.
            float waterline = Mathf.Max(h * 0.86f, h - 1.6f);
            NoShadow(kit.Deco(PrimitiveType.Cube, t.At(0f, waterline, depth * 0.5f), new Vector3(width + 0.3f, 0.08f, depth - 0.1f), PatchworkAssets.Water(Color.Lerp(shallow, Color.white, 0.25f)), new Vector3(0f, yaw, 0f)));
            var dry = PatchworkAssets.Plaster(new Color(0.16f, 0.2f, 0.24f));
            NoShadow(kit.Deco(PrimitiveType.Cube, t.At(0f, (waterline + h) * 0.5f + 0.02f, depth - 0.15f), new Vector3(width + 0.9f, h - waterline + 0.04f, 0.08f), dry, new Vector3(0f, yaw, 0f)));
            for (int s = -1; s <= 1; s += 2)
                NoShadow(kit.Deco(PrimitiveType.Cube, t.At(s * (width * 0.5f - 0.03f), (waterline + h) * 0.5f + 0.02f, depth * 0.5f), new Vector3(0.08f, h - waterline + 0.04f, depth - 0.2f), dry, new Vector3(0f, yaw, 0f)));
            var lampGlow = PatchworkAssets.Glow(new Color(1f, 0.92f, 0.75f), 2f);
            int lamps = Mathf.Max(3, Mathf.RoundToInt(width / 7f));
            for (int i = 0; i < lamps; i++)
            {
                float x = -width * 0.5f + width * (i + 0.5f) / lamps;
                NoShadow(kit.Deco(PrimitiveType.Cylinder, t.At(x, h - 0.12f, depth * 0.35f), new Vector3(1.1f, 0.1f, 1.1f), IronMat));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, t.At(x, h - 0.26f, depth * 0.35f), new Vector3(0.8f, 0.06f, 0.8f), lampGlow));
            }
            NoShadow(kit.Deco(PrimitiveType.Cylinder, t.At(0f, h - 0.35f, depth * 0.7f), new Vector3(0.3f, width * 0.5f, 0.3f), IronMat, new Vector3(0f, yaw, 90f)));
            // Sand.
            NoShadow(kit.Deco(PrimitiveType.Cube, t.At(0f, -0.25f, depth * 0.5f), new Vector3(width + 1f, 0.5f, depth), PatchworkAssets.Felt(Color.Lerp(new Color(0.75f, 0.68f, 0.5f), deep, 0.45f)), new Vector3(0f, yaw, 0f)));
            // A lid on the tank: the dark stone roof above the water, so looking up through the
            // glass you never see over the top of it.
            NoShadow(kit.Deco(PrimitiveType.Cube, t.At(0f, h + 0.5f, depth * 0.5f), new Vector3(width + 1.6f, 1f, depth + 0.6f), PatchworkAssets.Plaster(new Color(0.12f, 0.14f, 0.18f)), new Vector3(0f, yaw, 0f)));
            NoShadow(kit.Deco(PrimitiveType.Cube, t.At(0f, h + 0.22f, -0.05f), new Vector3(width + 1.6f, 0.44f, 0.5f), IronMat, new Vector3(0f, yaw, 0f)));
            // The glow that spills out through the glass onto the floor.
            PointLight(kit, t.At(0f, Mathf.Min(h * 0.45f, floorDrop + 6f), -2.5f), Color.Lerp(shallow, Color.white, 0.2f), Mathf.Max(14f, width * 0.5f), 1.5f);
            PointLight(kit, t.At(0f, h * 0.6f, depth * 0.45f), shallow, Mathf.Max(depth, width * 0.6f), 1.6f);
            return t;
        }

        // A clump of kelp: three stalks with leaves, swaying.
        static void Kelp(TankSpace t, float x, float z, float height, Color col, float y = 0f)
        {
            var kit = t.Kit;
            int segs = Mathf.Clamp(Mathf.RoundToInt(height / 3.2f), 3, 6);
            float segLen = height / segs;
            var mat = PatchworkAssets.Glow(col, 0.28f);
            var root = kit.Group("Kelp", t.At(x, y, z));
            var chain = new Transform[segs];
            Transform parent = root;
            float[] ox = { 0f, t.R(0.25f, 0.5f), -t.R(0.25f, 0.5f) }, oz = { 0f, t.R(-0.4f, 0.4f), t.R(-0.4f, 0.4f) };
            for (int i = 0; i < segs; i++)
            {
                var seg = new GameObject("Seg").transform;
                seg.SetParent(parent, false);
                seg.localPosition = i == 0 ? Vector3.zero : new Vector3(0f, segLen, 0f);
                var mb = new MeshBatch();
                for (int k = 0; k < 3; k++)
                {
                    if (k > 0 && i == segs - 1) continue;   // the side stalks are a little shorter
                    Vector3 b = new Vector3(ox[k], 0f, oz[k]);
                    mb.Cylinder(b, 0.07f, segLen, 5);
                    // Leaves: flat blades angled up and out, with a little float at the stem.
                    for (int leaf = 0; leaf < 3; leaf++)
                    {
                        float ly = segLen * (leaf + 0.5f) / 3f;
                        float la = (i * 3 + leaf + k) * 137.5f;
                        var lr = Quaternion.Euler(0f, la, 0f) * Quaternion.Euler(-55f, 0f, 0f);
                        mb.Box(b + new Vector3(0f, ly, 0f) + lr * new Vector3(0f, 0f, 0.55f), new Vector3(0.34f, 0.03f, 1.1f), lr);
                        mb.Ball(b + new Vector3(0f, ly, 0f) + lr * new Vector3(0f, 0f, 0.1f), Vector3.one * 0.16f, 2, 5);
                    }
                }
                mb.Build(seg, "KelpMesh", mat);
                chain[i] = seg;
                parent = seg;
            }
            var sway = root.gameObject.AddComponent<KelpSway>();
            sway.Segments = chain;
            sway.Amount = t.R(3.5f, 6f);
            sway.Speed = t.R(0.5f, 0.8f);
            sway.Phase = t.R(0f, 6f);
        }

        // Rocks, coral, sea fans and anemones scattered over the tank floor.
        static void Reef(TankSpace t, int count, float zMin = 2f, float zMax = -1f)
        {
            var kit = t.Kit;
            if (zMax < 0f) zMax = t.Depth - 2f;
            var rock = new MeshBatch();
            var rockMat = PatchworkAssets.Felt(new Color(0.28f, 0.32f, 0.42f));
            Color[] corals = { new Color(1f, 0.45f, 0.5f), new Color(1f, 0.7f, 0.3f), new Color(0.75f, 0.5f, 1f), new Color(0.4f, 0.9f, 0.8f), new Color(1f, 0.55f, 0.8f) };
            var coral = new MeshBatch[corals.Length];
            for (int i = 0; i < coral.Length; i++) coral[i] = new MeshBatch();
            for (int i = 0; i < count; i++)
            {
                float x = t.R(-t.Width * 0.47f, t.Width * 0.47f), z = t.R(zMin, zMax);
                Vector3 p = t.At(x, 0f, z);
                float s = t.R(0.8f, 2.4f);
                rock.Ball(p + Vector3.up * s * 0.25f, new Vector3(s * 1.5f, s * 0.9f, s * 1.2f), 4, 7);
                if (i % 3 == 0) rock.Ball(p + new Vector3(s * 0.6f, s * 0.15f, s * 0.3f), new Vector3(s * 0.8f, s * 0.5f, s * 0.7f), 3, 6);
                var mb = coral[i % coral.Length];
                Vector3 top = p + Vector3.up * s * 0.6f;
                switch (i % 4)
                {
                    case 0:   // brain coral: a bumpy dome
                        mb.Ball(top, new Vector3(s * 0.9f, s * 0.7f, s * 0.9f), 4, 8);
                        break;
                    case 1:   // branching coral
                        for (int b = 0; b < 5; b++)
                        {
                            var br = Quaternion.Euler(t.R(-35f, 35f), t.R(0f, 360f), t.R(-35f, 35f));
                            mb.Box(top + br * new Vector3(0f, s * 0.45f, 0f), new Vector3(0.14f, s * 0.9f, 0.14f), br);
                            mb.Ball(top + br * new Vector3(0f, s * 0.9f, 0f), Vector3.one * 0.26f, 2, 5);
                        }
                        break;
                    case 2:   // a sea fan
                    {
                        var fr = Quaternion.Euler(0f, t.R(0f, 180f), 0f);
                        mb.Ball(top + Vector3.up * s * 0.5f, new Vector3(s * 1.1f, s * 1.1f, 0.08f), 4, 8);
                        mb.Box(top + Vector3.up * s * 0.1f, new Vector3(0.1f, s * 0.4f, 0.1f), fr);
                        break;
                    }
                    default:  // an anemone: a ring of fat fingers
                        for (int b = 0; b < 7; b++)
                        {
                            var br = Quaternion.Euler(0f, b * 51.4f, 0f) * Quaternion.Euler(28f, 0f, 0f);
                            mb.Ball(top + br * new Vector3(0f, s * 0.3f, 0f), new Vector3(0.2f, s * 0.6f, 0.2f), 3, 5);
                        }
                        break;
                }
            }
            rock.Build(kit.Root, "TankRocks", rockMat);
            for (int i = 0; i < coral.Length; i++) coral[i].Build(kit.Root, "TankCoral", PatchworkAssets.Glow(corals[i], 0.4f));
        }

        // What kind of swimmer to stitch.
        enum Swimmer { Fish, Tall, Shark, Ray, Turtle, Whale, Angler }

        // Stitches a sea creature (nose pointing along +z) under "parent". Returns its tail if it has
        // one that swings.
        static Transform StitchSwimmer(LevelKit kit, Transform parent, Swimmer kind, float s, Color body, Color trim, bool eyes)
        {
            var main = new MeshBatch();
            var patch = new MeshBatch();
            var tailMesh = new MeshBatch();
            Vector3 tailAt = Vector3.zero;
            Vector3 eye = Vector3.zero;
            float eyeSize = 0.1f * s;
            switch (kind)
            {
                case Swimmer.Tall:   // a tall flat reef fish with stripes
                    main.Ball(Vector3.zero, new Vector3(0.22f, 0.9f, 0.95f) * s, 5, 9);
                    main.Box(new Vector3(0f, 0.55f, -0.1f) * s, new Vector3(0.03f, 0.5f, 0.3f) * s, Quaternion.Euler(-25f, 0f, 0f));
                    main.Box(new Vector3(0f, -0.55f, -0.1f) * s, new Vector3(0.03f, 0.5f, 0.3f) * s, Quaternion.Euler(25f, 0f, 0f));
                    patch.Box(new Vector3(0f, 0f, 0.12f) * s, new Vector3(0.24f, 0.8f, 0.12f) * s, Quaternion.identity);
                    patch.Box(new Vector3(0f, 0f, -0.22f) * s, new Vector3(0.23f, 0.66f, 0.1f) * s, Quaternion.identity);
                    tailAt = new Vector3(0f, 0f, -0.45f) * s;
                    tailMesh.Box(new Vector3(0f, 0.14f, -0.2f) * s, new Vector3(0.03f, 0.3f, 0.36f) * s, Quaternion.Euler(-30f, 0f, 0f));
                    tailMesh.Box(new Vector3(0f, -0.14f, -0.2f) * s, new Vector3(0.03f, 0.3f, 0.36f) * s, Quaternion.Euler(30f, 0f, 0f));
                    eye = new Vector3(0.11f, 0.12f, 0.3f) * s;
                    eyeSize = 0.13f * s;
                    break;
                case Swimmer.Shark:
                    main.Ball(Vector3.zero, new Vector3(0.62f, 0.6f, 2.9f) * s, 6, 10);
                    main.Ball(new Vector3(0f, -0.02f, 1.25f) * s, new Vector3(0.4f, 0.3f, 0.8f) * s, 4, 8);
                    main.Box(new Vector3(0f, 0.5f, 0.1f) * s, new Vector3(0.05f, 0.7f, 0.55f) * s, Quaternion.Euler(-28f, 0f, 0f));
                    main.Box(new Vector3(0.45f, -0.18f, 0.35f) * s, new Vector3(0.75f, 0.05f, 0.42f) * s, Quaternion.Euler(0f, -25f, -22f));
                    main.Box(new Vector3(-0.45f, -0.18f, 0.35f) * s, new Vector3(0.75f, 0.05f, 0.42f) * s, Quaternion.Euler(0f, 25f, 22f));
                    main.Box(new Vector3(0f, 0.22f, -0.75f) * s, new Vector3(0.04f, 0.26f, 0.24f) * s, Quaternion.Euler(-28f, 0f, 0f));
                    patch.Ball(new Vector3(0f, -0.12f, 0.15f) * s, new Vector3(0.52f, 0.44f, 2.4f) * s, 5, 9);
                    tailAt = new Vector3(0f, 0f, -1.25f) * s;
                    tailMesh.Ball(new Vector3(0f, 0f, -0.1f) * s, new Vector3(0.2f, 0.24f, 0.6f) * s, 3, 6);
                    tailMesh.Box(new Vector3(0f, 0.34f, -0.48f) * s, new Vector3(0.05f, 0.9f, 0.3f) * s, Quaternion.Euler(-38f, 0f, 0f));
                    tailMesh.Box(new Vector3(0f, -0.2f, -0.4f) * s, new Vector3(0.05f, 0.55f, 0.26f) * s, Quaternion.Euler(40f, 0f, 0f));
                    eye = new Vector3(0.22f, 0.08f, 1.05f) * s;
                    eyeSize = 0.09f * s;
                    break;
                case Swimmer.Ray:
                    main.Ball(Vector3.zero, new Vector3(1.3f, 0.3f, 1.7f) * s, 5, 10);
                    main.Box(new Vector3(1.05f, 0.02f, 0f) * s, new Vector3(1.5f, 0.06f, 1.1f) * s, Quaternion.Euler(0f, -22f, 10f));
                    main.Box(new Vector3(-1.05f, 0.02f, 0f) * s, new Vector3(1.5f, 0.06f, 1.1f) * s, Quaternion.Euler(0f, 22f, -10f));
                    main.Box(new Vector3(0.3f, 0f, 0.95f) * s, new Vector3(0.14f, 0.1f, 0.5f) * s, Quaternion.Euler(0f, 12f, 0f));
                    main.Box(new Vector3(-0.3f, 0f, 0.95f) * s, new Vector3(0.14f, 0.1f, 0.5f) * s, Quaternion.Euler(0f, -12f, 0f));
                    patch.Ball(new Vector3(0f, -0.06f, 0f) * s, new Vector3(1.1f, 0.26f, 1.4f) * s, 4, 8);
                    tailAt = new Vector3(0f, 0f, -0.8f) * s;
                    tailMesh.Box(new Vector3(0f, 0f, -0.9f) * s, new Vector3(0.05f, 0.05f, 1.8f) * s, Quaternion.identity);
                    eye = new Vector3(0.42f, 0.1f, 0.6f) * s;
                    eyeSize = 0.1f * s;
                    break;
                case Swimmer.Turtle:
                    main.Ball(Vector3.zero, new Vector3(1.1f, 0.55f, 1.35f) * s, 5, 9);
                    patch.Ball(new Vector3(0f, -0.1f, 0f) * s, new Vector3(1f, 0.4f, 1.25f) * s, 4, 8);
                    patch.Ball(new Vector3(0f, 0f, 0.85f) * s, new Vector3(0.4f, 0.36f, 0.5f) * s, 4, 7);
                    patch.Box(new Vector3(0.75f, -0.1f, 0.4f) * s, new Vector3(0.9f, 0.07f, 0.34f) * s, Quaternion.Euler(0f, -30f, -8f));
                    patch.Box(new Vector3(-0.75f, -0.1f, 0.4f) * s, new Vector3(0.9f, 0.07f, 0.34f) * s, Quaternion.Euler(0f, 30f, 8f));
                    patch.Box(new Vector3(0.5f, -0.1f, -0.6f) * s, new Vector3(0.45f, 0.06f, 0.26f) * s, Quaternion.Euler(0f, 35f, 0f));
                    patch.Box(new Vector3(-0.5f, -0.1f, -0.6f) * s, new Vector3(0.45f, 0.06f, 0.26f) * s, Quaternion.Euler(0f, -35f, 0f));
                    // Shell patches.
                    for (int i = 0; i < 5; i++)
                    {
                        float a = i * 72f * Mathf.Deg2Rad;
                        main.Ball(new Vector3(Mathf.Cos(a) * 0.3f, 0.2f, Mathf.Sin(a) * 0.38f) * s, new Vector3(0.36f, 0.16f, 0.4f) * s, 3, 6);
                    }
                    eye = new Vector3(0.17f, 0.08f, 1f) * s;
                    eyeSize = 0.09f * s;
                    break;
                case Swimmer.Whale:
                    main.Ball(Vector3.zero, new Vector3(2.3f, 2.1f, 8.6f) * s, 8, 14);
                    main.Ball(new Vector3(0f, 0.1f, 3f) * s, new Vector3(2.1f, 1.7f, 3.6f) * s, 6, 12);
                    main.Box(new Vector3(1.5f, -0.6f, 1.6f) * s, new Vector3(2.4f, 0.14f, 0.9f) * s, Quaternion.Euler(0f, -30f, -25f));
                    main.Box(new Vector3(-1.5f, -0.6f, 1.6f) * s, new Vector3(2.4f, 0.14f, 0.9f) * s, Quaternion.Euler(0f, 30f, 25f));
                    main.Box(new Vector3(0f, 1.05f, -1.6f) * s, new Vector3(0.1f, 0.5f, 0.7f) * s, Quaternion.Euler(-30f, 0f, 0f));
                    patch.Ball(new Vector3(0f, -0.4f, 0.6f) * s, new Vector3(2.05f, 1.7f, 7.4f) * s, 6, 12);
                    tailAt = new Vector3(0f, 0f, -3.9f) * s;
                    tailMesh.Ball(new Vector3(0f, 0f, -0.5f) * s, new Vector3(0.7f, 0.6f, 1.8f) * s, 4, 8);
                    tailMesh.Box(new Vector3(0.95f, 0f, -1.5f) * s, new Vector3(2f, 0.14f, 0.85f) * s, Quaternion.Euler(0f, 28f, 0f));
                    tailMesh.Box(new Vector3(-0.95f, 0f, -1.5f) * s, new Vector3(2f, 0.14f, 0.85f) * s, Quaternion.Euler(0f, -28f, 0f));
                    eye = new Vector3(0.98f, -0.05f, 3.2f) * s;
                    eyeSize = 0.22f * s;
                    break;
                case Swimmer.Angler:
                    main.Ball(Vector3.zero, new Vector3(1.1f, 1.05f, 1.3f) * s, 5, 9);
                    main.Box(new Vector3(0f, 0.75f, 0.35f) * s, new Vector3(0.05f, 0.7f, 0.05f) * s, Quaternion.Euler(35f, 0f, 0f));
                    main.Box(new Vector3(0.5f, 0f, -0.2f) * s, new Vector3(0.4f, 0.05f, 0.3f) * s, Quaternion.Euler(0f, 30f, 0f));
                    main.Box(new Vector3(-0.5f, 0f, -0.2f) * s, new Vector3(0.4f, 0.05f, 0.3f) * s, Quaternion.Euler(0f, -30f, 0f));
                    // A wide mouth of little felt teeth.
                    for (int i = -3; i <= 3; i++)
                        patch.Box(new Vector3(i * 0.12f, -0.2f + Mathf.Abs(i) * 0.03f, 0.58f - Mathf.Abs(i) * 0.03f) * s, new Vector3(0.08f, 0.2f, 0.06f) * s, Quaternion.Euler(0f, 0f, i * 4f));
                    tailAt = new Vector3(0f, 0f, -0.6f) * s;
                    tailMesh.Box(new Vector3(0f, 0.12f, -0.2f) * s, new Vector3(0.04f, 0.34f, 0.4f) * s, Quaternion.Euler(-30f, 0f, 0f));
                    tailMesh.Box(new Vector3(0f, -0.12f, -0.2f) * s, new Vector3(0.04f, 0.34f, 0.4f) * s, Quaternion.Euler(30f, 0f, 0f));
                    eye = new Vector3(0.38f, 0.25f, 0.45f) * s;
                    eyeSize = 0.14f * s;
                    break;
                default:   // an ordinary fish
                    main.Ball(Vector3.zero, new Vector3(0.36f, 0.56f, 1.1f) * s, 5, 9);
                    main.Box(new Vector3(0f, 0.34f, -0.05f) * s, new Vector3(0.03f, 0.26f, 0.4f) * s, Quaternion.Euler(-18f, 0f, 0f));
                    main.Box(new Vector3(0.2f, -0.1f, 0.15f) * s, new Vector3(0.26f, 0.03f, 0.16f) * s, Quaternion.Euler(0f, -30f, -30f));
                    main.Box(new Vector3(-0.2f, -0.1f, 0.15f) * s, new Vector3(0.26f, 0.03f, 0.16f) * s, Quaternion.Euler(0f, 30f, 30f));
                    patch.Ball(new Vector3(0f, -0.1f, 0.05f) * s, new Vector3(0.32f, 0.4f, 0.9f) * s, 4, 8);
                    tailAt = new Vector3(0f, 0f, -0.5f) * s;
                    tailMesh.Box(new Vector3(0f, 0.12f, -0.2f) * s, new Vector3(0.03f, 0.3f, 0.36f) * s, Quaternion.Euler(-32f, 0f, 0f));
                    tailMesh.Box(new Vector3(0f, -0.12f, -0.2f) * s, new Vector3(0.03f, 0.3f, 0.36f) * s, Quaternion.Euler(32f, 0f, 0f));
                    eye = new Vector3(0.15f, 0.08f, 0.36f) * s;
                    break;
            }
            var bodyMat = PatchworkAssets.Glow(body, 0.3f);
            var trimMat = PatchworkAssets.Glow(trim, 0.3f);
            main.Build(parent, "Body", bodyMat);
            patch.Build(parent, "Patch", trimMat);
            Transform tail = null;
            var tailGo = new GameObject("Tail").transform;
            tailGo.SetParent(parent, false);
            tailGo.localPosition = tailAt;
            if (tailMesh.Build(tailGo, "TailMesh", bodyMat) != null) tail = tailGo;
            if (eyes)
            {
                // Button eyes.
                var white = PatchworkAssets.Glow(new Color(1f, 0.98f, 0.9f), 0.5f);
                var dark = PatchworkAssets.Plain(new Color(0.08f, 0.07f, 0.1f), 0.8f);
                for (int sgn = -1; sgn <= 1; sgn += 2)
                {
                    Vector3 p = new Vector3(eye.x * sgn, eye.y, eye.z);
                    NoShadow(kit.Deco(PrimitiveType.Sphere, p, new Vector3(eyeSize * 0.6f, eyeSize * 1.6f, eyeSize * 1.6f), white, default(Vector3), parent));
                    NoShadow(kit.Deco(PrimitiveType.Sphere, p + new Vector3(sgn * eyeSize * 0.22f, 0f, 0f), new Vector3(eyeSize * 0.4f, eyeSize * 0.8f, eyeSize * 0.8f), dark, default(Vector3), parent));
                }
            }
            return tail;
        }

        // A swimmer going round an oval. center is in world space; rx and rz are the oval's
        // size along world x and z.
        static FishSwim Swim(LevelKit kit, Swimmer kind, Vector3 center, float rx, float rz, float speed, float size, Color body, Color trim, float startAngle = 0f)
        {
            var g = kit.Group(kind.ToString(), center);
            var tail = StitchSwimmer(kit, g, kind, size, body, trim, true);
            var f = g.gameObject.AddComponent<FishSwim>();
            f.Center = center;
            f.RadiusX = rx;
            f.RadiusZ = rz;
            f.Speed = speed;
            f.Angle = startAngle;
            f.Tail = tail;
            bool big = kind == Swimmer.Whale || kind == Swimmer.Shark || kind == Swimmer.Ray || kind == Swimmer.Turtle;
            f.Wiggle = big ? 2.5f : 7f;
            f.WiggleSpeed = kind == Swimmer.Whale ? 0.9f : big ? 2f : 6f;
            f.TailSwing = kind == Swimmer.Whale ? 9f : big ? 16f : 26f;
            f.Bob = big ? 0.8f : 0.35f;
            f.BobSpeed = big ? 0.3f : 0.7f;
            f.Bank = kind == Swimmer.Ray ? 14f : 6f;
            if (kind == Swimmer.Whale || kind == Swimmer.Ray || kind == Swimmer.Turtle) f.TailSwing *= 0.5f;
            return f;
        }

        // The same, placed in a tank (x along the glass, y above the tank floor, z back from
        // the glass; "along" and "back" are the oval's size those ways).
        static FishSwim TankSwim(TankSpace t, Swimmer kind, float x, float y, float z, float along, float back, float speed, float size, Color body, Color trim, float startAngle = 0f)
        {
            bool turned = t.Turned;
            return Swim(t.Kit, kind, t.At(x, y, z), turned ? back : along, turned ? along : back, speed, size, body, trim, startAngle);
        }

        // A shoal: a loose cloud of little fish stitched as one, all swimming round together.
        static FishSwim Shoal(LevelKit kit, Vector3 center, float rx, float rz, float speed, int count, float fishSize, Color col, int seed, float startAngle = 0f)
        {
            var g = kit.Group("Shoal", center);
            var rng = new System.Random(seed);
            var mb = new MeshBatch();
            for (int i = 0; i < count; i++)
            {
                Vector3 p = new Vector3((float)rng.NextDouble() - 0.5f, ((float)rng.NextDouble() - 0.5f) * 0.7f, (float)rng.NextDouble() - 0.5f);
                p = Vector3.Scale(p, new Vector3(2.6f, 2.2f, 5f)) * (0.6f + count * 0.02f);
                float s = fishSize * (0.8f + (float)rng.NextDouble() * 0.4f);
                mb.Ball(p, new Vector3(0.3f, 0.5f, 1f) * s, 3, 6);
                mb.Box(p + new Vector3(0f, 0.1f, -0.62f) * s, new Vector3(0.03f, 0.26f, 0.3f) * s, Quaternion.Euler(-32f, 0f, 0f));
                mb.Box(p + new Vector3(0f, -0.1f, -0.62f) * s, new Vector3(0.03f, 0.26f, 0.3f) * s, Quaternion.Euler(32f, 0f, 0f));
            }
            mb.Build(g, "ShoalMesh", PatchworkAssets.Glow(col, 0.45f));
            var f = g.gameObject.AddComponent<FishSwim>();
            f.Center = center;
            f.RadiusX = rx;
            f.RadiusZ = rz;
            f.Speed = speed;
            f.Angle = startAngle;
            f.Wiggle = 3f;
            f.WiggleSpeed = 2.4f;
            f.Bob = 0.9f;
            f.BobSpeed = 0.35f;
            f.Bank = 0f;
            return f;
        }

        static FishSwim TankShoal(TankSpace t, float x, float y, float z, float along, float back, float speed, int count, float fishSize, Color col, float startAngle = 0f)
        {
            bool turned = t.Turned;
            return Shoal(t.Kit, t.At(x, y, z), turned ? back : along, turned ? along : back, speed, count, fishSize, col, t.Rng.Next(), startAngle);
        }

        // A glowing jellyfish, bobbing where it is.
        static Transform Jelly(LevelKit kit, Vector3 pos, float size, Color col, float phase, bool light = false)
        {
            var g = kit.Group("Jelly", pos);
            var bellMat = PatchworkAssets.Glow(col, 1.1f);
            var skirt = PatchworkAssets.Glow(Color.Lerp(col, Color.white, 0.5f), 0.8f);
            var bell = NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(1f, 0.75f, 1f) * size, bellMat, default(Vector3), g));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.14f * size, 0f), new Vector3(0.96f, 0.05f, 0.96f) * size, skirt, default(Vector3), g));
            var arms = new MeshBatch();
            for (int i = 0; i < 7; i++)
            {
                float a = i / 7f * Mathf.PI * 2f;
                float len = size * (1f + (i % 3) * 0.35f);
                Vector3 top = new Vector3(Mathf.Cos(a) * 0.3f, -0.15f, Mathf.Sin(a) * 0.3f) * size;
                arms.Box(top + Vector3.down * len * 0.5f, new Vector3(0.045f * size, len, 0.045f * size), Quaternion.Euler(Mathf.Sin(a) * 6f, 0f, Mathf.Cos(a) * 6f));
                arms.Ball(top + Vector3.down * len, Vector3.one * 0.1f * size, 2, 5);
            }
            arms.Build(g, "Arms", skirt);
            var bob = g.gameObject.AddComponent<JellyBob>();
            bob.Bell = bell;
            bob.Phase = phase;
            bob.Rise = size * 0.7f;
            bob.Speed = 0.5f + (phase % 1f) * 0.3f;
            bob.Drift = size * 0.3f;
            if (light)
            {
                var l = PointLight(kit, pos, col, size * 7f, 1.2f);
                l.transform.SetParent(g, true);
            }
            return g;
        }

        // Bubbles trickling up from a spot.
        static void Bubbles(LevelKit kit, Vector3 from, float height, int count = 7)
        {
            var g = kit.Group("Bubbles", from);
            var mat = PatchworkAssets.Glow(new Color(0.8f, 0.95f, 1f), 0.7f);
            var list = new Transform[count];
            for (int i = 0; i < count; i++) list[i] = NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.15f, mat, default(Vector3), g));
            var s = g.gameObject.AddComponent<BubbleStream>();
            s.Bubbles = list;
            s.Height = height;
            s.Speed = 1f + (from.x * 0.13f + from.z * 0.07f) % 0.6f;
        }

        // ------------------------------------------------------------------ Bones and old things

        // A skeleton hung or stood along a line: a spine of beads from "tail" to "head" (curving
        // up by "arch" in the middle), with pairs of ribs hanging under the middle of it.
        static void Spine(LevelKit kit, Vector3 tail, Vector3 head, float arch, float bead, float ribLen, float ribFrom = 0.3f, float ribTo = 0.75f)
        {
            var bone = Bone;
            Vector3 d = head - tail;
            float len = d.magnitude;
            int n = Mathf.Max(6, Mathf.RoundToInt(len / (bead * 0.85f)));
            Vector3 side = Vector3.Cross(Vector3.up, d.normalized).normalized;
            float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            for (int i = 0; i <= n; i++)
            {
                float k = i / (float)n;
                Vector3 p = Vector3.Lerp(tail, head, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * arch;
                float fat = bead * Mathf.Lerp(0.45f, 1f, Mathf.Sin(Mathf.Clamp01(k * 1.15f) * Mathf.PI));
                NoShadow(kit.Deco(PrimitiveType.Sphere, p, new Vector3(fat * 1.1f, fat, fat * 0.8f), bone, new Vector3(0f, yaw, 0f)));
                if (i % 2 == 0) NoShadow(kit.Deco(PrimitiveType.Cube, p + Vector3.up * fat * 0.6f, new Vector3(0.08f, fat * 0.7f, 0.12f), bone, new Vector3(0f, yaw, 0f)));
                if (k >= ribFrom && k <= ribTo && i % 2 == 0)
                {
                    float rk = (k - ribFrom) / (ribTo - ribFrom);
                    float rl = ribLen * (0.55f + 0.45f * Mathf.Sin(rk * Mathf.PI));
                    for (int s = -1; s <= 1; s += 2)
                    {
                        Vector3 a = p + side * s * fat * 0.4f;
                        Vector3 mid = p + side * s * rl * 0.62f + Vector3.down * rl * 0.5f;
                        Vector3 end = p + side * s * rl * 0.3f + Vector3.down * rl;
                        Beam(kit, a, mid, 0.14f, bone);
                        Beam(kit, mid, end, 0.11f, bone);
                    }
                }
            }
        }
    }
}

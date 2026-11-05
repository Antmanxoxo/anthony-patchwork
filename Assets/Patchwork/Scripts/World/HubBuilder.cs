using UnityEngine;

namespace Patchwork
{
    // The hub - "The Pod". A cosy egg-shaped cardboard pod floating in a bright paper sky, with a
    // big window at the front. It's quiet in here: walk to the giant controller on the desk under
    // the window and pick it up, and the music comes in while you turn the patchwork planet outside
    // to choose a level. The pod is yours to decorate: stickers stick to its walls, the pattern
    // book by the window changes the wallpaper, and decorations you win turn up on its shelves.
    // The little Cardboard Skiff sails round outside, and Captain Yarny has come aboard for tea.
    //
    // Layout (floor is y = 0, the window faces +z):
    //   the window and the desk with the controller  z  6 .. 9.4
    //   the pattern book (west of the window), Yarny (east)
    //   shelves round the back-left wall, a clock on the right wall
    //   the Button Box: a little room through a doorway in the back wall, z -8.6 .. -14.6
    public static class HubBuilder
    {
        public const string SceneName = "Hub";

        static readonly Color Kraft = new Color(0.8f, 0.62f, 0.42f);
        static readonly Color KraftDark = new Color(0.62f, 0.45f, 0.3f);
        static readonly Color KraftLight = new Color(0.88f, 0.74f, 0.55f);

        // The pod's egg shape: the wall's height and radius at each ring, floor to roof.
        static readonly float[] ProfileY = { 0f, 1.3f, 4f, 7f, 9.5f, 11.8f, 13.4f };
        static readonly float[] ProfileR = { 8.8f, 9.3f, 9.4f, 8.7f, 7f, 4.4f, 1.6f };
        const int Columns = 28;
        const float WindowHalfAngle = 40f;      // columns within this of +z are the window
        const float DoorHalfAngle = 13f;        // and within this of -z, the Button Box doorway
        const float DeskTop = 1.3f, WindowTop = 7f, DoorTop = 4f;

        // Outside: the patchwork planet you pick levels on.
        static readonly Vector3 GlobeCenter = new Vector3(0f, 4.4f, 16.5f);
        const float GlobeRadius = 2.8f;

        public static LevelInfo Build(LevelKit kit)
        {
            PodTheme.ResetStatics();
            PodDecor.ResetStatics();
            var info = kit.Info;
            info.Id = SceneName;
            info.IsHub = true;
            info.Title = LevelCatalog.Skiff.Title;
            info.Subtitle = "Pick up the controller to choose where to go.";
            info.Spawn = LandPoint + Vector3.up * 0.1f;
            info.SpawnYaw = 0f;
            info.KillY = -10f;
            // Deep space outside.
            info.Sky = new Color(0.01f, 0.01f, 0.025f);
            info.FogStart = 400f;
            info.FogEnd = 900f;
            info.CameraRumble = 0.07f;
            info.Saturation = 18f;
            info.Area.SetMinMax(new Vector3(-11f, -1f, -16f), new Vector3(11f, 14f, 11f));
            // Sunlight pours in through the window.
            info.SunEuler = new Vector3(38f, 165f, 0f);
            info.SunIntensity = 1.1f;
            info.TitleCenter = new Vector3(0f, 0f, 0.5f);
            info.TitleRadius = 5.6f;
            info.TitleHeight = 3.4f;
            info.TitleLookOffset = new Vector3(0f, 2.6f, 2f);
            info.QuietMusic = true;

            Shell(kit);
            Window(kit);
            Floor(kit);
            Shelves(kit);
            Controller(kit);
            Globe(kit);
            PatternBook(kit);
            ButtonBox(kit);
            Furnishings(kit);
            Decorations(kit);
            MeritWall(kit);
            PodDecor.ApplyAll();
            Outside(kit);
            MiniSkiff(kit);
            Sounds(kit);
            LaunchTube(kit);
            PodTheme.Apply();

            kit.AddCheckpoint(info.Spawn, 0f);
            kit.AddCameraZone(new Vector3(0f, 6f, 0f), new Vector3(19f, 13f, 19f), -2.4f, 3f);
            kit.Hint(new Vector3(0f, 1.5f, -2f), new Vector3(5f, 4f, 4f), "Your Pod",
                "Pick up the controller under the window to choose a level. Make the pod yours: {Sticker} sticks stickers on the walls, and the pattern book changes the wallpaper.");

            // Captain Yarny's come aboard (his Skiff's parked outside).
            var yarny = NpcYarny.Create(kit, new Vector3(5.6f, 0f, 1.6f), -100f);
            var talk = kit.Volume<TalkZone>(new Vector3(4.6f, 1.2f, 1.6f), new Vector3(3.4f, 3f, 3.6f), "TalkToYarny");
            talk.Npc = yarny;
            return info;
        }

        // Where Patch lands after being popped out of the launch tube.
        static readonly Vector3 LandPoint = new Vector3(0.8f, 0f, -1.2f);

        // Builds something and gathers everything it made under one group, registered with the
        // decorating menu so it can be switched on and off.
        static void Collect(LevelKit kit, string key, System.Action build)
        {
            int before = kit.Root.childCount;
            build();
            var g = new GameObject("Item_" + key).transform;
            g.SetParent(kit.Root, false);
            var made = new System.Collections.Generic.List<Transform>();
            for (int i = before; i < kit.Root.childCount; i++)
            {
                var c = kit.Root.GetChild(i);
                if (c != g) made.Add(c);
            }
            foreach (var c in made) c.SetParent(g, true);
            PodDecor.Register(key, g.gameObject);
        }

        // The furniture you can switch on and off in the decorating menu.
        static void BaseItems(LevelKit kit)
        {
            Collect(kit, "stool", () =>
            {
                Vector3 st = new Vector3(6.2f, 0f, -3.4f);
                kit.Block(st + Vector3.up * 0.45f, new Vector3(1.4f, 0.9f, 1.4f), Color.white, "Stool").GetComponent<Renderer>().enabled = false;
                kit.Deco(PrimitiveType.Cylinder, st + Vector3.up * 0.35f, new Vector3(0.7f, 0.35f, 0.7f), PatchworkAssets.Felt(new Color(0.98f, 0.94f, 0.85f)));
                kit.Deco(PrimitiveType.Sphere, st + Vector3.up * 0.82f, new Vector3(1.7f, 0.6f, 1.7f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(0.9f, 0.25f, 0.25f), new Color(0.99f, 0.96f, 0.9f)));
            });
            Collect(kit, "cushion", () => kit.Deco(PrimitiveType.Sphere, new Vector3(-6.4f, 0.3f, -2.4f), new Vector3(1.9f, 0.6f, 1.9f),
                PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, new Color(0.2f, 0.25f, 0.55f), new Color(1f, 0.8f, 0.28f))));
            Collect(kit, "bouncer", () => kit.AddBouncePad(new Vector3(-4.6f, 0f, -5.2f)));
            Collect(kit, "ball", () => Grabbable.BeachBall(kit, new Vector3(3f, 0.05f, -4.5f), 0.6f));
            Collect(kit, "blocks", () =>
            {
                Grabbable.ToyBlock(kit, new Vector3(-2.6f, 0.05f, -5.6f), 0.8f, new Color(0.95f, 0.45f, 0.45f), 15f);
                Grabbable.ToyBlock(kit, new Vector3(-1.6f, 0.05f, -6.2f), 0.8f, new Color(0.45f, 0.65f, 0.95f), -20f);
                Grabbable.ToyBlock(kit, new Vector3(-2.1f, 0.85f, -5.9f), 0.8f, new Color(1f, 0.85f, 0.35f), 40f);
            });
            // A squashy bean bag by the window.
            Collect(kit, "beanbag", () =>
            {
                Vector3 b = new Vector3(4.2f, 0f, 5.2f);
                kit.Block(b + Vector3.up * 0.35f, new Vector3(1.6f, 0.7f, 1.6f), Color.white, "BeanBag").GetComponent<Renderer>().enabled = false;
                kit.Deco(PrimitiveType.Sphere, b + Vector3.up * 0.42f, new Vector3(2f, 0.9f, 1.9f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stars, new Color(0.55f, 0.35f, 0.75f), new Color(1f, 0.85f, 0.4f)));
                kit.Deco(PrimitiveType.Sphere, b + new Vector3(0f, 0.9f, 0.55f), new Vector3(1.5f, 0.8f, 0.7f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stars, new Color(0.55f, 0.35f, 0.75f), new Color(1f, 0.85f, 0.4f)), new Vector3(-20f, 0f, 0f));
            });
            // A striped toy chest under the shelves.
            Collect(kit, "chest", () =>
            {
                Vector3 c = new Vector3(-7.4f, 0f, 1.2f);
                var g = kit.Group("ToyChest", c);
                g.rotation = Quaternion.Euler(0f, 90f, 0f);
                kit.Block(c + Vector3.up * 0.55f, new Vector3(1.2f, 1.1f, 2f), Color.white, "ToyChestSolid").GetComponent<Renderer>().enabled = false;
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.5f, 0f), new Vector3(2f, 1f, 1.2f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.4f, 0.7f, 1f), new Color(0.99f, 0.96f, 0.9f)), default(Vector3), g);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.08f, -0.25f), new Vector3(2.05f, 0.16f, 1.25f), PatchworkAssets.Felt(new Color(1f, 0.8f, 0.3f)), new Vector3(-25f, 0f, 0f), g);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0.5f, 1.15f, 0.1f), new Vector3(0.5f, 0.5f, 0.5f), PatchworkAssets.Plain(new Color(1f, 0.4f, 0.5f), 0.7f), default(Vector3), g);
                kit.Deco(PrimitiveType.Cube, new Vector3(-0.4f, 1.2f, 0.1f), new Vector3(0.45f, 0.45f, 0.45f), PatchworkAssets.Felt(new Color(0.55f, 0.85f, 0.45f)), new Vector3(10f, 30f, 0f), g);
            });
            // A potted felt plant.
            Collect(kit, "plant", () =>
            {
                Vector3 p = new Vector3(-6.9f, 0f, 5.6f);
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.4f, new Vector3(0.9f, 0.4f, 0.9f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(0.95f, 0.55f, 0.3f), new Color(0.99f, 0.96f, 0.9f)));
                var leaf = PatchworkAssets.Felt(new Color(0.4f, 0.75f, 0.4f));
                for (int k = 0; k < 7; k++)
                {
                    float a = k * 51f;
                    kit.Deco(PrimitiveType.Sphere, p + Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 1.25f + (k % 3) * 0.25f, 0.3f), new Vector3(0.35f, 1f, 0.12f), leaf, new Vector3(25f, a, 0f));
                }
            });
            // A lava lamp on the desk, its blobs drifting up and down.
            Collect(kit, "lavalamp", () =>
            {
                Vector3 p = new Vector3(2.8f, DeskTop, 8.2f);
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.12f, new Vector3(0.45f, 0.12f, 0.45f), PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.9f), 0.8f));
                var glass = kit.Deco(PrimitiveType.Capsule, p + Vector3.up * 0.75f, new Vector3(0.38f, 0.5f, 0.38f), PatchworkAssets.Bubble(new Color(1f, 0.5f, 0.75f, 0.4f)));
                glass.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                for (int k = 0; k < 3; k++)
                {
                    var blob = kit.Deco(PrimitiveType.Sphere, p + Vector3.up * (0.5f + k * 0.25f), Vector3.one * (0.14f + k * 0.03f), PatchworkAssets.Glow(new Color(1f, 0.6f, 0.3f), 1.4f));
                    var s = blob.gameObject.AddComponent<Spinner>();
                    s.Speed = 0f;
                    s.BobHeight = 0.18f;
                    s.BobSpeed = 0.6f + k * 0.25f;
                }
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 1.28f, new Vector3(0.3f, 0.06f, 0.3f), PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.9f), 0.8f));
            });
            // A felt mobile of stars and moons turning under the roof.
            Collect(kit, "mobile", () =>
            {
                var m = kit.Group("StarMobile", new Vector3(0f, 11f, -4f));
                var cord = PatchworkAssets.Yarn(new Color(0.95f, 0.9f, 0.8f));
                kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(4f, 0.04f, 0.06f), cord, new Vector3(0f, 0f, 90f), m);
                kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(4f, 0.04f, 0.06f), cord, new Vector3(90f, 0f, 90f), m);
                Color[] cols = { new Color(1f, 0.85f, 0.35f), new Color(1f, 0.55f, 0.72f), new Color(0.5f, 0.8f, 1f), new Color(0.7f, 0.55f, 0.95f) };
                for (int k = 0; k < 4; k++)
                {
                    Vector3 end = Quaternion.Euler(0f, k * 90f, 0f) * new Vector3(0f, 0f, 2f);
                    kit.Deco(PrimitiveType.Cylinder, end + Vector3.down * 0.6f, new Vector3(0.03f, 0.6f, 0.03f), cord, default(Vector3), m);
                    for (int pnt = 0; pnt < 5; pnt++)
                        kit.Deco(PrimitiveType.Sphere, end + Vector3.down * 1.4f + Quaternion.Euler(0f, k * 90f, pnt * 72f) * new Vector3(0f, 0.22f, 0f), new Vector3(0.18f, 0.48f, 0.1f), PatchworkAssets.Felt(cols[k]), new Vector3(0f, k * 90f, pnt * 72f), m);
                }
                var spin = m.gameObject.AddComponent<Spinner>();
                spin.Speed = 12f;
            });
            // Fairy lights looped round the walls.
            Collect(kit, "fairylights", () =>
            {
                var cord = PatchworkAssets.Yarn(new Color(0.3f, 0.5f, 0.3f));
                Color[] bulbs = { new Color(1f, 0.85f, 0.4f), new Color(1f, 0.5f, 0.6f), new Color(0.5f, 0.8f, 1f), new Color(0.6f, 0.95f, 0.5f) };
                const int n = 40;
                for (int k = 0; k < n; k++)
                {
                    float a0 = 45f + k * 270f / n, a1 = 45f + (k + 1) * 270f / n;
                    float y0 = 8.2f - Mathf.Abs(Mathf.Sin(k * 0.8f)) * 0.5f, y1 = 8.2f - Mathf.Abs(Mathf.Sin((k + 1) * 0.8f)) * 0.5f;
                    Vector3 p0 = Dir(a0) * (RadiusAt(y0) - 0.3f) + Vector3.up * y0, p1 = Dir(a1) * (RadiusAt(y1) - 0.3f) + Vector3.up * y1;
                    kit.Rope(p0, p1, cord, 0.03f);
                    var b = kit.Deco(PrimitiveType.Sphere, p0 + Vector3.down * 0.12f, new Vector3(0.14f, 0.2f, 0.14f), PatchworkAssets.Glow(bulbs[k % bulbs.Length], 1.8f));
                    b.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            });
        }

        // ---------------------------------------------------------------- Helpers

        static Vector3 Dir(float deg)
        {
            float a = deg * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a));
        }

        // -180..180, 0 = straight out of the window.
        static float Signed(float deg) { return Mathf.DeltaAngle(0f, deg); }

        static float ColumnAngle(int i) { return (i + 0.5f) * 360f / Columns; }

        // Radius of the wall at height y.
        static float RadiusAt(float y)
        {
            for (int k = 0; k < ProfileY.Length - 1; k++)
                if (y <= ProfileY[k + 1])
                    return Mathf.Lerp(ProfileR[k], ProfileR[k + 1], Mathf.InverseLerp(ProfileY[k], ProfileY[k + 1], y));
            return ProfileR[ProfileR.Length - 1];
        }

        // A piece of the curved wall: one column, one ring. The inner face sits on the egg shape.
        static Transform Panel(LevelKit kit, float deg, int tier, float thickness, float inset, Vector3 size, Material mat, bool solid, string name)
        {
            Vector3 d = Dir(deg);
            float r0 = ProfileR[tier], r1 = ProfileR[tier + 1], y0 = ProfileY[tier], y1 = ProfileY[tier + 1];
            Vector3 s = d * (r1 - r0) + Vector3.up * (y1 - y0);
            Vector3 n = (d * (y1 - y0) - Vector3.up * (r1 - r0)).normalized;
            Vector3 mid = d * ((r0 + r1) * 0.5f) + Vector3.up * ((y0 + y1) * 0.5f);
            Vector3 c = mid + n * (thickness * 0.5f - inset);
            Transform t;
            if (solid) t = kit.Block(c, size, Color.white, name, mat);
            else t = kit.Deco(PrimitiveType.Cube, c, size, mat);
            t.name = name;
            t.rotation = Quaternion.LookRotation(n, s.normalized);
            return t;
        }

        static float TierLength(int tier)
        {
            float dr = ProfileR[tier + 1] - ProfileR[tier], dy = ProfileY[tier + 1] - ProfileY[tier];
            return Mathf.Sqrt(dr * dr + dy * dy);
        }

        static bool InWindow(float deg, int tier) { return Mathf.Abs(Signed(deg)) < WindowHalfAngle && (tier == 1 || tier == 2); }
        static bool InDoor(float deg, int tier) { return Mathf.Abs(Signed(deg - 180f)) < DoorHalfAngle && (tier == 0 || tier == 1); }

        // A thin rod between two local points (ropes and masts on the little Skiff).
        static void Rod(LevelKit kit, Vector3 a, Vector3 b, Material mat, float thick, Transform parent)
        {
            Vector3 d = b - a;
            var t = kit.Deco(PrimitiveType.Cylinder, (a + b) * 0.5f, new Vector3(thick, d.magnitude * 0.5f, thick), mat, default(Vector3), parent);
            t.localRotation = Quaternion.FromToRotation(Vector3.up, d.normalized);
        }

        // ---------------------------------------------------------------- The shell

        static void Shell(LevelKit kit)
        {
            const float thick = 0.4f;
            var wall = PodTheme.Material(PodTheme.Current);
            var rib = PatchworkAssets.Cardboard(KraftDark);
            var glass = PatchworkAssets.Bubble(new Color(0.75f, 0.88f, 1f, 0.1f));
            var shine = PatchworkAssets.Bubble(new Color(1f, 1f, 1f, 0.32f));
            for (int i = 0; i < Columns; i++)
            {
                float deg = ColumnAngle(i);
                for (int tier = 0; tier < ProfileY.Length - 1; tier++)
                {
                    if (InDoor(deg, tier)) continue;
                    float rm = (ProfileR[tier] + ProfileR[tier + 1]) * 0.5f;
                    var size = new Vector3(rm * 0.235f, TierLength(tier) * 1.03f, thick);
                    var t = Panel(kit, deg, tier, thick, 0f, size, wall, true, "PodWall");
                    if (InWindow(deg, tier))
                    {
                        // The window: a pane of faintly tinted glass with a few glints on it.
                        t.name = "WindowPane";
                        var pr = t.GetComponent<Renderer>();
                        pr.sharedMaterial = glass;
                        pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        int col = Mathf.RoundToInt(Signed(deg) / (360f / Columns) - 0.5f);
                        if (tier == 2 && (col == -3 || col == 1) || tier == 1 && col == 2)
                        {
                            for (int g = 0; g < 2; g++)
                            {
                                var glint = kit.Deco(PrimitiveType.Cube, t.position - t.forward * 0.24f + t.right * (g * 0.35f - 0.3f), new Vector3(g == 0 ? 0.16f : 0.07f, TierLength(tier) * 0.6f, 0.01f), shine);
                                glint.rotation = t.rotation * Quaternion.Euler(0f, 0f, 28f);
                                glint.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                            }
                        }
                    }
                    else PodTheme.Register(t.GetComponent<Renderer>());
                }
            }

            // Cardboard ribs inside, like the struts of a cut-out cardboard model.
            for (int j = 0; j < Columns; j++)
            {
                float deg = j * 360f / Columns;
                bool windowEdge = Mathf.Abs(Mathf.Abs(Signed(deg)) - 38.57f) < 0.1f;
                bool doorEdge = Mathf.Abs(Mathf.Abs(Signed(deg - 180f)) - 12.86f) < 0.1f;
                if (j % 2 != 0 && !windowEdge && !doorEdge) continue;
                for (int tier = 0; tier < ProfileY.Length - 2; tier++)
                {
                    float sd = Mathf.Abs(Signed(deg));
                    if ((tier == 1 || tier == 2) && sd < 38f) continue;          // not across the window
                    if (tier <= 1 && Mathf.Abs(Signed(deg - 180f)) < 12f) continue;  // or the doorway
                    Panel(kit, deg, tier, 0.6f, 0.6f, new Vector3(0.14f, TierLength(tier), 0.6f), rib, false, "Rib");
                }
            }

            // The roof cap and a little cardboard chimney, like the top of the egg.
            var cap = PatchworkAssets.Cardboard(KraftLight);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 13.4f, 0f), new Vector3(3.6f, 0.25f, 3.6f), cap);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 14.4f, 0f), new Vector3(0.9f, 1f, 0.9f), cap);
            // A ring of horizontal shelves round the outside, like the box-art pod's ribs.
            for (int i = 0; i < Columns; i++)
            {
                float deg = ColumnAngle(i);
                if (Mathf.Abs(Signed(deg)) < WindowHalfAngle) continue;
                float r = RadiusAt(9.5f);
                kit.Deco(PrimitiveType.Cube, Dir(deg) * (r + 0.9f) + Vector3.up * 9.5f, new Vector3(r * 0.25f, 0.12f, 1.4f), cap, new Vector3(0f, deg, 0f));
            }
        }

        // The window frame, the desk under it, and its zigzag trim.
        static void Window(LevelKit kit)
        {
            var desk = PatchworkAssets.Planks(new Color(0.86f, 0.7f, 0.5f));
            var trim = PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, new Color(0.2f, 0.25f, 0.55f), new Color(1f, 0.8f, 0.28f));
            for (int i = 0; i < Columns; i++)
            {
                float deg = ColumnAngle(i);
                if (Mathf.Abs(Signed(deg)) >= WindowHalfAngle) continue;
                Vector3 d = Dir(deg);
                float rDesk = RadiusAt(DeskTop), rTop = RadiusAt(WindowTop);
                // The desk: a deep ledge under the window.
                var top = kit.Block(d * (rDesk - 0.75f) + Vector3.up * (DeskTop - 0.15f), new Vector3(rDesk * 0.24f, 0.3f, 1.6f), Color.white, "Desk", desk);
                top.rotation = Quaternion.Euler(0f, deg, 0f);
                kit.Deco(PrimitiveType.Cube, d * (rDesk - 1.58f) + Vector3.up * (DeskTop - 0.15f), new Vector3(rDesk * 0.24f, 0.36f, 0.08f), trim, new Vector3(0f, deg, 0f));
                // Felt piping along the top of the window.
                kit.Deco(PrimitiveType.Cube, d * (rTop - 0.2f) + Vector3.up * WindowTop, new Vector3(rTop * 0.25f, 0.4f, 0.4f), trim, new Vector3(0f, deg, 0f));
            }
            // Desk legs under the front edge.
            var leg = PatchworkAssets.Wood(new Color(0.7f, 0.55f, 0.4f));
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, Dir(s * 30f) * (RadiusAt(DeskTop) - 1.4f) + Vector3.up * (DeskTop * 0.5f - 0.15f), new Vector3(0.18f, DeskTop * 0.5f - 0.15f, 0.18f), leg);
        }

        static void Floor(LevelKit kit)
        {
            var floor = kit.Block(new Vector3(0f, -0.5f, 0f), new Vector3(22f, 1f, 22f), Color.white, "Floor");
            floor.GetComponent<Renderer>().enabled = false;
            var boards = kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.06f, 0f), new Vector3(19.2f, 0.05f, 19.2f), PatchworkAssets.Planks(new Color(0.84f, 0.66f, 0.48f)));
            // A round rug: patterned felt with a rose felt border (both picked in the decorating menu).
            var border = kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.5f), new Vector3(9.4f, 0.01f, 9.4f), PatchworkAssets.Felt(new Color(0.92f, 0.45f, 0.5f)));
            var rug = kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.01f, 0.5f), new Vector3(8.6f, 0.01f, 8.6f),
                PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, new Color(0.45f, 0.75f, 0.95f), new Color(1f, 0.9f, 0.55f)));
            PodDecor.RegisterFloor(boards.GetComponent<Renderer>(), rug.GetComponent<Renderer>(), border.GetComponent<Renderer>());
        }

        // Two curved shelves round the back-left wall: hop up them; prizes sit on them.
        static void Shelves(LevelKit kit)
        {
            var wood = PatchworkAssets.Planks(new Color(0.9f, 0.76f, 0.56f));
            float[] heights = { 2.4f, 4.4f };
            for (int i = 0; i < Columns; i++)
            {
                float deg = ColumnAngle(i);
                float sd = Signed(deg);
                foreach (float h in heights)
                {
                    bool lower = h < 3f;
                    if (lower ? (sd > -60f || sd < -125f) : (sd > -75f || sd < -150f)) continue;
                    float r = RadiusAt(h);
                    var t = kit.Block(Dir(deg) * (r - 0.6f) + Vector3.up * (h - 0.1f), new Vector3(r * 0.24f, 0.2f, 1.25f), Color.white, "Shelf", wood);
                    t.rotation = Quaternion.Euler(0f, deg, 0f);
                }
            }
        }

        // ---------------------------------------------------------------- The controller and the planet

        static void Controller(LevelKit kit)
        {
            Vector3 at = new Vector3(0f, DeskTop + 0.18f, 8.25f);
            var pad = kit.Group("Controller", at);
            var body = PatchworkAssets.Plain(new Color(0.16f, 0.15f, 0.19f), 0.55f);
            kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(1.3f, 0.3f, 0.6f), body, default(Vector3), pad);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.62f, 0f, 0.02f), new Vector3(0.62f, 0.36f, 0.62f), body, default(Vector3), pad);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.74f, -0.03f, -0.3f), new Vector3(0.4f, 0.32f, 0.72f), body, new Vector3(0f, s * -18f, 0f), pad);
                // Thumbsticks.
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.24f, 0.18f, -0.14f), new Vector3(0.22f, 0.06f, 0.22f), PatchworkAssets.Felt(new Color(0.3f, 0.3f, 0.34f)), default(Vector3), pad);
            }
            // D-pad (a felt cross) and four coloured buttons.
            var grey = PatchworkAssets.Felt(new Color(0.55f, 0.55f, 0.6f));
            kit.Deco(PrimitiveType.Cube, new Vector3(-0.62f, 0.17f, 0.06f), new Vector3(0.3f, 0.04f, 0.09f), grey, default(Vector3), pad);
            kit.Deco(PrimitiveType.Cube, new Vector3(-0.62f, 0.17f, 0.06f), new Vector3(0.09f, 0.04f, 0.3f), grey, default(Vector3), pad);
            Color[] cols = { new Color(0.4f, 0.85f, 0.65f), new Color(1f, 0.45f, 0.45f), new Color(0.45f, 0.65f, 1f), new Color(0.95f, 0.6f, 0.9f) };
            Vector3[] offs = { new Vector3(0f, 0f, 0.11f), new Vector3(0.11f, 0f, 0f), new Vector3(0f, 0f, -0.11f), new Vector3(-0.11f, 0f, 0f) };
            for (int k = 0; k < 4; k++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0.62f, 0.17f, 0.06f) + offs[k], new Vector3(0.09f, 0.03f, 0.09f), PatchworkAssets.Glow(cols[k], 0.35f), default(Vector3), pad);
            // A curly cable trailing off the desk.
            var cable = PatchworkAssets.Yarn(new Color(0.2f, 0.2f, 0.24f));
            Vector3 prev = at + new Vector3(0f, -0.05f, 0.35f);
            for (int k = 1; k <= 10; k++)
            {
                float f = k / 10f;
                Vector3 p = at + new Vector3(Mathf.Sin(f * 9f) * 0.25f, -0.12f - f * 0.02f, 0.35f + f * 0.9f);
                kit.Rope(prev, p, cable, 0.05f);
                prev = p;
            }
            var bob = pad.gameObject.AddComponent<Spinner>();
            bob.Speed = 0f;
            bob.BobHeight = 0.04f;
            bob.BobSpeed = 1.2f;

            var station = kit.Volume<HelmStation>(new Vector3(0f, 1.2f, 6.5f), new Vector3(4.4f, 3f, 2.6f), "Controller");
            station.PromptPoint = at + new Vector3(0f, 1f, 0f);
            station.Label = "Pick up the controller";
            controllerStation = station;
        }

        static HelmStation controllerStation;

        static void Globe(LevelKit kit)
        {
            var root = kit.Group("Globe", GlobeCenter);
            var spin = new GameObject("Spin").transform;
            spin.SetParent(root, false);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * GlobeRadius * 2f, PatchworkAssets.GlobeMap(), default(Vector3), spin);
            var globe = root.gameObject.AddComponent<LevelGlobe>();
            globe.Spin = spin;
            globe.Radius = GlobeRadius;
            // Seen from just inside the window, with the card on the right.
            globe.ViewOffset = new Vector3(0.4f, 0.5f, -8.6f);

            foreach (var e in LevelCatalog.Levels)
            {
                Vector3 dir = LevelGlobe.Direction(e.Longitude, e.Latitude);
                var pin = new GameObject("Pin_" + e.Number).transform;
                pin.SetParent(spin, false);
                pin.localPosition = dir * GlobeRadius;
                pin.localRotation = Quaternion.FromToRotation(Vector3.up, dir);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.35f, 0f), new Vector3(0.06f, 0.35f, 0.06f), PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.9f), 0.9f), default(Vector3), pin);
                Color head = e.Playable ? (GameState.IsLevelComplete(e.Scene) ? LevelKit.Palette.Gold : LevelKit.Palette.AnchorPink) : new Color(0.6f, 0.6f, 0.65f);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.75f, 0f), Vector3.one * 0.34f,
                    e.Playable ? PatchworkAssets.Glow(head, 0.4f) : PatchworkAssets.Plain(head, 0.5f), default(Vector3), pin);
                kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.6f, 0.02f, 0.6f), PatchworkAssets.Felt(e.Tint), default(Vector3), pin);
                globe.AddPin(pin);
            }
            kit.Info.Globe = globe;
            if (controllerStation != null) controllerStation.Globe = globe;

            // A warm light on its front.
            var lightGo = new GameObject("GlobeLight");
            lightGo.transform.SetParent(kit.Root, false);
            lightGo.transform.position = GlobeCenter + new Vector3(0f, 2.5f, -5f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.9f, 0.75f);
            light.range = 10f;
            light.intensity = 1.1f;
            light.shadows = LightShadows.None;
        }

        // ---------------------------------------------------------------- Making it yours

        // A big fabric swatch book on a stand: flip it to change the wallpaper.
        static void PatternBook(LevelKit kit)
        {
            Vector3 p = new Vector3(-5.6f, 0f, 3.8f);
            var stand = PatchworkAssets.Wood(new Color(0.7f, 0.55f, 0.4f));
            kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.55f, new Vector3(0.14f, 0.55f, 0.14f), stand);
            kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.04f, new Vector3(0.8f, 0.04f, 0.8f), stand);
            var book = kit.Group("PatternBook", p + Vector3.up * 1.2f);
            book.rotation = Quaternion.Euler(-35f, 135f, 0f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, 0f), new Vector3(1.5f, 1.05f, 0.08f), PatchworkAssets.Felt(new Color(0.85f, 0.3f, 0.35f)), default(Vector3), book);
            // Open pages, each a fabric swatch.
            var pageL = kit.Deco(PrimitiveType.Cube, new Vector3(-0.36f, 0f, -0.06f), new Vector3(0.66f, 0.92f, 0.03f), PodTheme.Material(1), new Vector3(0f, 6f, 0f), book);
            var pageR = kit.Deco(PrimitiveType.Cube, new Vector3(0.36f, 0f, -0.06f), new Vector3(0.66f, 0.92f, 0.03f), PodTheme.Material(2), new Vector3(0f, -6f, 0f), book);
            var spot = kit.Volume<UseSpot>(new Vector3(-4.8f, 1.2f, 3.1f), new Vector3(2.8f, 3f, 2.8f), "PatternBook");
            spot.Label = "Decorate the Pod";
            spot.PromptPoint = p + Vector3.up * 2.3f;
            var left = pageL.GetComponent<Renderer>();
            var right = pageR.GetComponent<Renderer>();
            System.Action showPages = () =>
            {
                int n = PodTheme.Names.Length;
                left.sharedMaterial = PodTheme.Material((PodTheme.Current + 1) % n);
                right.sharedMaterial = PodTheme.Material((PodTheme.Current + 2) % n);
            };
            showPages();
            PodTheme.Changed = showPages;
            spot.Used = player =>
            {
                Sfx.Play(SfxId.PageTurn, 0.6f);
                if (player.Game != null) player.Game.OpenPodEditor();
            };
        }

        // The Button Box: a little room through a doorway at the back, stitched shut until every
        // golden button in Level 1 is found. Inside is the Golden Crown.
        static void ButtonBox(LevelKit kit)
        {
            int found = GameState.GoldenButtonCount();
            int needed = Level1Builder.CollectibleIds.Length;
            bool open = found >= needed;
            const float half = 2.1f, z0 = -14.6f, z1 = -8.6f, h = DoorTop;
            float cz = (z0 + z1) * 0.5f;

            kit.Cardboard(new Vector3(-half - 0.2f, h * 0.5f, cz), new Vector3(0.4f, h, z1 - z0), Kraft, "BoxWall");
            kit.Cardboard(new Vector3(half + 0.2f, h * 0.5f, cz), new Vector3(0.4f, h, z1 - z0), Kraft, "BoxWall");
            kit.Cardboard(new Vector3(0f, h * 0.5f, z0 - 0.2f), new Vector3(half * 2f + 0.8f, h, 0.4f), Kraft, "BoxWall");
            kit.Cardboard(new Vector3(0f, h + 0.2f, cz - 0.2f), new Vector3(half * 2f + 0.8f, 0.4f, z1 - z0 + 0.4f), KraftLight, "BoxRoof");
            var floor = kit.Block(new Vector3(0f, -0.5f, cz), new Vector3(half * 2f, 1f, z1 - z0), Color.white, "BoxFloor");
            floor.GetComponent<Renderer>().enabled = false;
            kit.Quilt(new Vector3(0f, -0.04f, cz), new Vector3(half * 2f, 0.08f, z1 - z0), new[] { LevelKit.Palette.Butter, LevelKit.Palette.Peach, LevelKit.Palette.Cream }, 2f, "BoxRug");

            // Three button sockets over the doorway, gold for each one found.
            var gold = PatchworkAssets.Glow(LevelKit.Palette.Gold, 0.12f);
            var empty = PatchworkAssets.Felt(new Color(0.35f, 0.3f, 0.4f));
            float doorZ = -RadiusAt(0f) + 0.05f;
            for (int i = 0; i < needed; i++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(-0.8f + i * 0.8f, h + 0.55f, -RadiusAt(h + 0.5f) + 0.3f), new Vector3(0.55f, 0.08f, 0.55f), i < found ? gold : empty, new Vector3(90f, 0f, 0f));

            if (!open)
            {
                kit.Block(new Vector3(0f, h * 0.5f, doorZ - 0.3f), new Vector3(half * 2f + 0.3f, h, 0.3f), LevelKit.Palette.Rose, "BoxDoor");
                var thread = PatchworkAssets.Yarn(new Color(0.95f, 0.9f, 0.8f));
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, h * 0.5f, doorZ - 0.12f), new Vector3(0.12f, h * 1.15f, 0.05f), thread, new Vector3(0f, 0f, 35f));
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, h * 0.5f, doorZ - 0.12f), new Vector3(0.12f, h * 1.15f, 0.05f), thread, new Vector3(0f, 0f, -35f));
            }
            kit.Hint(new Vector3(0f, 1.5f, doorZ + 1.6f), new Vector3(4.4f, 3f, 2.4f), "The Button Box",
                open ? "Unstitched! All the golden buttons are home. Step inside."
                     : "Stitched shut. It opens when you've found all " + needed + " golden buttons in Level 1. Found so far: " + found + ".");

            // Inside: the Golden Crown on a pedestal.
            Vector3 ped = new Vector3(0f, 0f, cz - 0.8f);
            kit.Deco(PrimitiveType.Cylinder, ped + Vector3.up * 0.4f, new Vector3(1.4f, 0.4f, 1.4f), PatchworkAssets.Felt(LevelKit.Palette.Cream));
            kit.Deco(PrimitiveType.Cylinder, ped + Vector3.up * 0.84f, new Vector3(1.6f, 0.04f, 1.6f), PatchworkAssets.Felt(LevelKit.Palette.Gold));
            var pedestal = kit.Volume<HatPedestal>(ped + Vector3.up * 1.4f, new Vector3(2f, 2.8f, 2f), "CrownPedestal");
            var vis = new GameObject("Crown").transform;
            vis.SetParent(pedestal.transform, false);
            var crownGold = PatchworkAssets.Glow(new Color(1f, 0.8f, 0.25f), 0.1f);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.9f, 0.16f, 0.9f), crownGold, default(Vector3), vis);
            for (int i = 0; i < 5; i++)
            {
                float a = i * Mathf.PI * 2f / 5f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Sin(a) * 0.4f, 0.25f, Mathf.Cos(a) * 0.4f), new Vector3(0.18f, 0.36f, 0.18f), crownGold, default(Vector3), vis);
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Sin(a) * 0.4f, 0.46f, Mathf.Cos(a) * 0.4f), Vector3.one * 0.12f, crownGold, default(Vector3), vis);
            }
            pedestal.Setup(4, FormVisuals.HatNames[4], vis);
            kit.Lamp(new Vector3(0f, 3.2f, cz), h);
        }

        // Things to play with and sit on.
        static void Furnishings(LevelKit kit)
        {
            // A hanging light bulb in the middle of the pod, swinging gently as it flies.
            var swing = kit.Group("BulbSwing", new Vector3(0f, 13.4f, 0f));
            var cord = PatchworkAssets.Yarn(new Color(0.25f, 0.22f, 0.25f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -3.1f, 0f), new Vector3(0.05f, 3.1f, 0.05f), cord, default(Vector3), swing);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -6.05f, 0f), new Vector3(0.22f, 0.12f, 0.22f), PatchworkAssets.Plain(new Color(0.75f, 0.72f, 0.68f), 0.7f), default(Vector3), swing);
            var bulb = kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -6.4f, 0f), new Vector3(0.5f, 0.62f, 0.5f), PatchworkAssets.Glow(new Color(1f, 0.86f, 0.5f), 2.2f), default(Vector3), swing);
            bulb.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var lgo = new GameObject("BulbLight");
            lgo.transform.SetParent(swing, false);
            lgo.transform.localPosition = new Vector3(0f, -6.8f, 0f);
            var l = lgo.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.84f, 0.62f);
            l.range = 18f;
            l.intensity = 2f;
            l.shadows = LightShadows.None;
            var sway = swing.gameObject.AddComponent<Rocker>();
            sway.Axis = new Vector3(1f, 0f, 0.4f);
            sway.Amount = 3f;
            sway.Speed = 1.1f;

            PodDecor.RegisterBulb(l, bulb.GetComponent<Renderer>());
            BaseItems(kit);

            // A felt clock on the right wall (you can hear it tick).
            Vector3 d = Dir(90f);
            Vector3 c = d * (RadiusAt(3.6f) - 0.12f) + Vector3.up * 3.6f;
            kit.Deco(PrimitiveType.Cylinder, c, new Vector3(1.6f, 0.06f, 1.6f), PatchworkAssets.Felt(new Color(0.99f, 0.95f, 0.85f)), new Vector3(0f, 0f, 90f));
            kit.Deco(PrimitiveType.Cylinder, c + d * 0.03f, new Vector3(1.8f, 0.04f, 1.8f), PatchworkAssets.Felt(new Color(0.3f, 0.6f, 0.85f)), new Vector3(0f, 0f, 90f));
            var hands = kit.Group("ClockHands", c - d * 0.06f);
            hands.rotation = Quaternion.LookRotation(-d, Vector3.up);
            var ink = PatchworkAssets.Felt(UIKit.Ink);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.25f, 0f), new Vector3(0.07f, 0.5f, 0.02f), ink, default(Vector3), hands);
            var minute = kit.Group("MinuteHand", c - d * 0.07f);
            minute.rotation = Quaternion.LookRotation(-d, Vector3.up);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.32f, 0f), new Vector3(0.05f, 0.64f, 0.02f), ink, default(Vector3), minute);
            var tick = minute.gameObject.AddComponent<Spinner>();
            tick.Axis = Vector3.forward;
            tick.Speed = 6f;
        }

        // Decorations won from prize bubbles turn up round the pod.
        static void Decorations(LevelKit kit)
        {
            // Duck Captain: a big rubber duck in a captain's hat, on the top shelf.
            if (Prizes.HasDecoration(0)) Collect(kit, "deco0", () =>
            {
                float deg = -105f;
                Vector3 at = Dir(deg) * (RadiusAt(4.4f) - 0.7f) + Vector3.up * 4.4f;
                var d = kit.Group("DuckCaptain", at);
                d.rotation = Quaternion.Euler(0f, deg + 180f, 0f);
                d.localScale = Vector3.one * 0.6f;
                var yellow = PatchworkAssets.Plain(new Color(1f, 0.85f, 0.2f), 0.75f);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.45f, 0f), new Vector3(1.4f, 1f, 1.8f), yellow, default(Vector3), d);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.5f, -0.8f), new Vector3(0.7f, 0.9f, 0.6f), yellow, new Vector3(-30f, 0f, 0f), d);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.25f, 0.55f), Vector3.one * 0.9f, yellow, default(Vector3), d);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.15f, 1.05f), new Vector3(0.45f, 0.18f, 0.45f), PatchworkAssets.Plain(new Color(1f, 0.55f, 0.15f), 0.75f), default(Vector3), d);
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.28f, 1.38f, 0.92f), Vector3.one * 0.13f, PatchworkAssets.Plain(UIKit.Ink, 0.7f), default(Vector3), d);
                var navy = PatchworkAssets.Felt(new Color(0.2f, 0.24f, 0.4f));
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.68f, 0.55f), new Vector3(0.75f, 0.06f, 0.75f), navy, default(Vector3), d);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.82f, 0.55f), new Vector3(0.6f, 0.12f, 0.6f), PatchworkAssets.Felt(Color.white), default(Vector3), d);
                var rock = d.gameObject.AddComponent<Rocker>();
                rock.Axis = Vector3.forward;
                rock.Amount = 4f;
                rock.Speed = 1.2f;
            });
            // Paper Lanterns strung across the pod.
            if (Prizes.HasDecoration(1)) Collect(kit, "deco1", () =>
            {
                Vector3 a = Dir(-70f) * (RadiusAt(7.5f) - 0.3f) + Vector3.up * 7.5f;
                Vector3 b = Dir(70f) * (RadiusAt(7.5f) - 0.3f) + Vector3.up * 7.5f;
                var cord = PatchworkAssets.Yarn(new Color(0.95f, 0.9f, 0.8f));
                const int segs = 14;
                System.Func<float, Vector3> P = k => Vector3.Lerp(a, b, k) + Vector3.down * Mathf.Sin(k * Mathf.PI) * 1.4f + Vector3.back * Mathf.Sin(k * Mathf.PI) * 3f;
                for (int i = 0; i < segs; i++) kit.Rope(P(i / (float)segs), P((i + 1) / (float)segs), cord, 0.04f);
                Color[] cols = { new Color(1f, 0.55f, 0.55f), new Color(1f, 0.85f, 0.45f), new Color(0.6f, 0.85f, 1f), new Color(0.75f, 0.95f, 0.6f) };
                for (int i = 1; i < 9; i++)
                {
                    Vector3 p = P(i / 9f) + Vector3.down * 0.35f;
                    kit.Deco(PrimitiveType.Sphere, p, new Vector3(0.42f, 0.55f, 0.42f), PatchworkAssets.Glow(cols[i % cols.Length], 0.8f));
                    kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.3f, new Vector3(0.24f, 0.04f, 0.24f), PatchworkAssets.Felt(new Color(0.3f, 0.25f, 0.3f)));
                }
            });
            // Snow Globe: a tiny Skiff in a glass dome, on the desk.
            if (Prizes.HasDecoration(2)) Collect(kit, "deco2", () =>
            {
                Vector3 g = new Vector3(-2.6f, DeskTop, 8.2f);
                kit.Deco(PrimitiveType.Cylinder, g + Vector3.up * 0.15f, new Vector3(0.9f, 0.15f, 0.9f), PatchworkAssets.Wood(new Color(0.5f, 0.32f, 0.22f)));
                kit.Deco(PrimitiveType.Cube, g + Vector3.up * 0.45f, new Vector3(0.3f, 0.14f, 0.5f), PatchworkAssets.Cardboard(Kraft));
                kit.Deco(PrimitiveType.Sphere, g + Vector3.up * 0.75f, new Vector3(0.34f, 0.24f, 0.34f), PatchworkAssets.Felt(new Color(0.95f, 0.6f, 0.65f)));
                var glass = kit.Deco(PrimitiveType.Sphere, g + Vector3.up * 0.68f, Vector3.one * 0.8f, PatchworkAssets.Bubble(new Color(0.85f, 0.92f, 1f, 0.28f)));
                glass.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                var snow = kit.Group("Snow", g + Vector3.up * 0.68f);
                for (int i = 0; i < 12; i++)
                {
                    float a = i * 2.4f;
                    kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.25f, Mathf.Sin(i * 1.7f) * 0.2f, Mathf.Sin(a) * 0.25f), Vector3.one * 0.04f, PatchworkAssets.Glow(Color.white, 0.6f), default(Vector3), snow);
                }
                var spin = snow.gameObject.AddComponent<Spinner>();
                spin.Speed = 25f;
                spin.BobHeight = 0.04f;
            });
            // The Grumblewool Plush: a soft, friendly copy of the Big Top's boss, on the lower shelf.
            if (Prizes.HasDecoration(4)) Collect(kit, "deco4", () =>
            {
                float deg = -95f;
                Vector3 at = Dir(deg) * (RadiusAt(2.4f) - 0.7f) + Vector3.up * 2.4f;
                var plush = kit.Group("GrumblewoolPlush", at);
                plush.rotation = Quaternion.Euler(0f, deg + 180f, 0f);
                var grey = PatchworkAssets.Knit(new Color(0.62f, 0.6f, 0.68f));
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.5f, 0f), new Vector3(1.1f, 0.95f, 1f), grey, default(Vector3), plush);
                for (int s = -1; s <= 1; s += 2)
                {
                    kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.2f, 0.62f, 0.44f), new Vector3(0.24f, 0.28f, 0.12f), PatchworkAssets.Felt(Color.white), default(Vector3), plush);
                    kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.2f, 0.6f, 0.5f), Vector3.one * 0.11f, PatchworkAssets.Plain(UIKit.Ink, 0.7f), default(Vector3), plush);
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.32f, 1.05f, 0f), new Vector3(0.06f, 0.2f, 0.06f), PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.9f), 0.8f), new Vector3(0f, 0f, -s * 25f), plush);
                }
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.36f, 0.5f), new Vector3(0.36f, 0.12f, 0.08f), PatchworkAssets.Felt(new Color(0.95f, 0.5f, 0.6f)), default(Vector3), plush);
                var wob = plush.gameObject.AddComponent<Rocker>();
                wob.Axis = Vector3.forward;
                wob.Amount = 5f;
                wob.Speed = 1.6f;
            });
            // Pinwheels in two flowerpots either side of the Button Box doorway.
            if (Prizes.HasDecoration(3)) Collect(kit, "deco3", () =>
            {
                Color[] cols = { LevelKit.Palette.Rose, LevelKit.Palette.Sky, LevelKit.Palette.Butter, LevelKit.Palette.Sage };
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 pot = new Vector3(s * 3.3f, 0f, -7.7f);
                    kit.Deco(PrimitiveType.Cylinder, pot + Vector3.up * 0.3f, new Vector3(0.7f, 0.3f, 0.7f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(0.95f, 0.55f, 0.3f), new Color(0.99f, 0.96f, 0.9f)));
                    for (int k = 0; k < 2; k++)
                    {
                        Vector3 p = pot + new Vector3((k - 0.5f) * 0.25f, 0.6f, 0f);
                        float hgt = 0.9f + k * 0.35f;
                        kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * hgt * 0.5f, new Vector3(0.05f, hgt * 0.5f, 0.05f), PatchworkAssets.Wood(new Color(0.85f, 0.75f, 0.6f)));
                        var hub = kit.Group("Pinwheel", p + Vector3.up * hgt + Vector3.forward * 0.06f);
                        for (int b = 0; b < 4; b++)
                            kit.Deco(PrimitiveType.Cube, Quaternion.Euler(0f, 0f, b * 90f) * new Vector3(0.16f, 0.06f, 0f), new Vector3(0.32f, 0.16f, 0.01f),
                                PatchworkAssets.Felt(b % 2 == 0 ? cols[(k + (s > 0 ? 2 : 0)) % cols.Length] : Color.white), new Vector3(0f, 0f, b * 90f + 20f), hub);
                        var turn = hub.gameObject.AddComponent<Spinner>();
                        turn.Axis = Vector3.forward;
                        turn.Speed = 140f + k * 40f;
                    }
                }
            });
        }

        // Every Merit Patch you've earned, stitched onto the back-right wall of the pod.
        static void MeritWall(LevelKit kit)
        {
            const int cols = 7;
            for (int i = 0; i < Merits.All.Length; i++)
            {
                if (!Merits.Has((Merits.Id)i)) continue;
                int cx = i % cols, cy = i / cols;
                float deg = 108f + cx * 9f;
                float y = 6.2f - cy * 1.0f;
                Vector3 d = Dir(deg);
                Vector3 p = d * (RadiusAt(y) - 0.25f) + Vector3.up * y;
                var g = kit.Group("MeritPatch", p);
                g.rotation = Quaternion.LookRotation(-d, Vector3.up);
                var def = Merits.All[i];
                kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.9f, 0.03f, 0.9f), PatchworkAssets.Felt(def.Col), new Vector3(90f, 0f, 0f), g);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, -0.02f), new Vector3(0.98f, 0.02f, 0.98f), PatchworkAssets.Felt(Color.white), new Vector3(90f, 0f, 0f), g);
                var q = kit.Deco(PrimitiveType.Quad, new Vector3(0f, 0f, 0.045f), Vector3.one * 0.6f, PatchworkAssets.Sticker(StickerArt.Get(def.Icon)), new Vector3(0f, 180f, 0f), g);
                q.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        // ---------------------------------------------------------------- Outside the window

        // A puffy five-pointed felt star.
        static Transform Star(LevelKit kit, Vector3 pos, float size, Color col)
        {
            var g = kit.Group("Star", pos);
            var m = PatchworkAssets.Felt(col);
            for (int k = 0; k < 5; k++)
            {
                var q = Quaternion.Euler(0f, 0f, k * 72f);
                kit.Deco(PrimitiveType.Sphere, q * new Vector3(0f, 0.42f, 0f) * size, new Vector3(0.36f, 0.95f, 0.3f) * size, m, new Vector3(0f, 0f, k * 72f), g);
            }
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.6f, 0.6f, 0.36f) * size, m, default(Vector3), g);
            var spin = g.gameObject.AddComponent<Spinner>();
            spin.Axis = Vector3.forward;
            spin.Speed = 10f + size * 5f;
            spin.BobHeight = 0.15f;
            spin.BobSpeed = 0.6f;
            return g;
        }

        // Outer space, after the toy-box collage on the game's box art: felt stars, bold
        // patterned planets, a doughnut and a toadstool drifting by, and some woolly aliens.
        static void Outside(LevelKit kit)
        {
            // A field of tiny stars streaming slowly past, so the pod feels like it's flying.
            var field = kit.Group("Starfield", Vector3.zero);
            field.gameObject.AddComponent<Starfield>().Build(kit, 260);

            // A trail of felt stars sweeping down from the top left, red to yellow.
            for (int i = 0; i < 9; i++)
            {
                float f = i / 8f;
                Vector3 p = new Vector3(Mathf.Lerp(-13f, -3.6f, f), Mathf.Lerp(12.5f, 7.2f, f) + Mathf.Sin(f * Mathf.PI) * 1.2f, Mathf.Lerp(25f, 19f, f));
                Color c = Color.Lerp(new Color(0.95f, 0.3f, 0.3f), new Color(1f, 0.85f, 0.3f), f);
                Star(kit, p, Mathf.Lerp(0.5f, 1.1f, f), c);
            }
            // A few big felt stars further out.
            Star(kit, new Vector3(16f, 13f, 34f), 2.2f, new Color(1f, 0.9f, 0.4f));
            Star(kit, new Vector3(-20f, -6f, 30f), 1.6f, new Color(0.55f, 0.85f, 1f));
            Star(kit, new Vector3(9f, -9f, 24f), 1.2f, new Color(1f, 0.55f, 0.75f));

            // Planets: a polka-dot one with a zigzag ring, a striped giant far off, a little red one.
            var p1 = kit.Group("RingPlanet", new Vector3(-12f, 9.5f, 30f));
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 6f, PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(0.25f, 0.55f, 0.95f), new Color(1f, 0.85f, 0.35f)), default(Vector3), p1);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(12.5f, 0.05f, 12.5f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, new Color(0.9f, 0.25f, 0.3f), new Color(1f, 0.85f, 0.35f)), new Vector3(18f, 0f, -24f), p1);
            var p1s = p1.gameObject.AddComponent<Spinner>();
            p1s.Speed = 4f;
            var p2 = kit.Group("GiantPlanet", new Vector3(26f, 15f, 80f));
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 24f, PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.98f, 0.6f, 0.25f), new Color(0.99f, 0.93f, 0.8f)), new Vector3(0f, 0f, 75f), p2);
            var p3 = kit.Group("RedPlanet", new Vector3(12f, -4f, 27f));
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 4f, PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(0.88f, 0.22f, 0.27f), new Color(0.99f, 0.95f, 0.88f)), default(Vector3), p3);
            // A woolly alien sitting on top of it, waving.
            Alien(kit, p3.position + Vector3.up * 2.5f, new Color(0.7f, 0.45f, 0.9f), 1f, true);

            // A polka-dot toadstool floating by.
            var shroom = kit.Group("Toadstool", new Vector3(-8.6f, 5.6f, 20f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.7f, 0f), new Vector3(0.9f, 0.8f, 0.9f), PatchworkAssets.Felt(new Color(0.99f, 0.95f, 0.86f)), default(Vector3), shroom);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.2f, 0f), new Vector3(2.6f, 1.4f, 2.6f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(0.9f, 0.22f, 0.25f), new Color(0.99f, 0.96f, 0.9f)), default(Vector3), shroom);
            shroom.rotation = Quaternion.Euler(0f, 0f, 14f);
            var sb = shroom.gameObject.AddComponent<Spinner>();
            sb.Speed = 8f;
            sb.BobHeight = 0.3f;
            sb.BobSpeed = 0.5f;

            // A frosted doughnut with sprinkles.
            var donut = kit.Group("Doughnut", new Vector3(8.4f, 8f, 21f));
            donut.rotation = Quaternion.Euler(-60f, 0f, 20f);
            var dough = PatchworkAssets.Felt(new Color(0.9f, 0.68f, 0.42f));
            var icing = PatchworkAssets.Felt(new Color(1f, 0.6f, 0.75f));
            Color[] sprinkles = { new Color(0.4f, 0.75f, 1f), new Color(1f, 0.9f, 0.4f), new Color(0.6f, 0.9f, 0.5f), Color.white };
            for (int k = 0; k < 14; k++)
            {
                float a = k * Mathf.PI * 2f / 14f;
                Vector3 p = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 1.05f;
                kit.Deco(PrimitiveType.Sphere, p, Vector3.one * 0.95f, dough, default(Vector3), donut);
                kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 0.18f, new Vector3(0.95f, 0.7f, 0.95f), icing, default(Vector3), donut);
                kit.Deco(PrimitiveType.Cube, p * 1.05f + Vector3.up * 0.52f, new Vector3(0.22f, 0.06f, 0.06f), PatchworkAssets.Felt(sprinkles[k % 4]), new Vector3(0f, k * 47f, 0f), donut);
            }
            var ds = donut.gameObject.AddComponent<Spinner>();
            ds.Speed = 12f;
            ds.BobHeight = 0.35f;
            ds.BobSpeed = 0.45f;

            // Clockwork gears turning beside the planet.
            Gear(kit, new Vector3(5.6f, 1.2f, 21f), 1.6f, new Color(0.55f, 0.35f, 0.75f), 20f);
            Gear(kit, new Vector3(7.7f, 2.9f, 22.6f), 1.1f, new Color(0.95f, 0.65f, 0.25f), -29f);

            // A little felt moon with craters.
            var moon = kit.Group("Moon", new Vector3(-7.4f, 1.6f, 21f));
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 2.2f, PatchworkAssets.Felt(new Color(0.85f, 0.82f, 0.75f)), default(Vector3), moon);
            for (int k = 0; k < 5; k++)
            {
                Vector3 n = Quaternion.Euler(k * 37f - 60f, k * 71f + 150f, 0f) * Vector3.forward;
                var cr = kit.Deco(PrimitiveType.Cylinder, n * 1.05f, new Vector3(0.5f, 0.03f, 0.5f) * (0.6f + (k % 3) * 0.3f), PatchworkAssets.Felt(new Color(0.7f, 0.67f, 0.62f)), default(Vector3), moon);
                cr.localRotation = Quaternion.FromToRotation(Vector3.up, n);
            }
            var ms = moon.gameObject.AddComponent<Spinner>();
            ms.Speed = 6f;
            ms.BobHeight = 0.2f;

            // A flying saucer with a woolly alien at the controls, looping round the planet.
            var ufoOrbit = kit.Group("SaucerOrbit", Vector3.zero);
            var uo = ufoOrbit.gameObject.AddComponent<Orbiter>();
            uo.Center = GlobeCenter + new Vector3(0f, 5.5f, 4f);
            uo.Radius = 10f;
            uo.RadiusZ = 6f;
            uo.Speed = -14f;
            uo.Angle = 200f;
            uo.Lean = 12f;
            uo.Shake = 0.05f;
            var ufo = kit.Group("Saucer", Vector3.zero);
            ufo.SetParent(ufoOrbit, false);
            ufo.localPosition = Vector3.zero;
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(3.6f, 0.7f, 3.6f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.35f, 0.85f, 0.75f), new Color(0.99f, 0.96f, 0.9f)), default(Vector3), ufo);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.25f, 0f), new Vector3(1.6f, 0.5f, 1.6f), PatchworkAssets.Felt(new Color(0.95f, 0.45f, 0.55f)), default(Vector3), ufo);
            Color[] bulbs = { new Color(1f, 0.85f, 0.3f), new Color(1f, 0.45f, 0.5f), new Color(0.5f, 0.85f, 1f) };
            for (int k = 0; k < 12; k++)
            {
                float a = k * Mathf.PI * 2f / 12f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 1.75f, 0f, Mathf.Sin(a) * 1.75f), Vector3.one * 0.22f, PatchworkAssets.Glow(bulbs[k % 3], 1.5f), default(Vector3), ufo);
            }
            var pilot = Alien(kit, Vector3.zero, new Color(0.45f, 0.85f, 0.4f), 0.55f, false);
            pilot.SetParent(ufo, false);
            pilot.localPosition = new Vector3(0f, 0.55f, 0f);
            pilot.localRotation = Quaternion.identity;
            var dome = kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.55f, 0f), new Vector3(1.6f, 1.4f, 1.6f), PatchworkAssets.Bubble(new Color(0.8f, 0.95f, 1f, 0.25f)), default(Vector3), ufo);
            dome.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // More woolly aliens: one peeking in at the window, one drifting by on a balloon.
            Alien(kit, new Vector3(5.6f, 6.6f, 12.6f), new Color(1f, 0.6f, 0.3f), 0.7f, true);
            var floater = Alien(kit, new Vector3(-15f, 3f, 24f), new Color(1f, 0.55f, 0.75f), 0.9f, true);
            kit.Rope(floater.position + Vector3.up * 0.9f, floater.position + new Vector3(0.6f, 3.4f, 0f), PatchworkAssets.Yarn(Color.white), 0.04f);
            kit.Deco(PrimitiveType.Sphere, floater.position + new Vector3(0.6f, 4f, 0f), new Vector3(1.2f, 1.4f, 1.2f), PatchworkAssets.Plain(new Color(1f, 0.85f, 0.3f), 0.7f));

            // A comet with a felt tail.
            var comet = kit.Group("Comet", new Vector3(-24f, 18f, 46f));
            comet.rotation = Quaternion.Euler(0f, 0f, -28f);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 1.6f, PatchworkAssets.Glow(new Color(1f, 0.95f, 0.75f), 1.6f), default(Vector3), comet);
            Color[] tail = { new Color(1f, 0.85f, 0.35f), new Color(1f, 0.55f, 0.4f), new Color(0.95f, 0.4f, 0.6f), new Color(0.6f, 0.45f, 0.9f) };
            for (int k = 0; k < 4; k++)
                kit.Deco(PrimitiveType.Sphere, new Vector3(-1.6f - k * 1.9f, (k % 2 == 0 ? 0.15f : -0.15f), 0f), new Vector3(2.6f, 1.2f - k * 0.2f, 0.4f), PatchworkAssets.Felt(tail[k]), default(Vector3), comet);
        }

        // A little woolly alien: a knitted ball with big eyes, pom-pom antennae and stubby arms.
        static Transform Alien(LevelKit kit, Vector3 pos, Color col, float size, bool facePod)
        {
            var g = kit.Group("WoolAlien", pos);
            if (facePod)
            {
                Vector3 look = new Vector3(0f, 3f, 0f) - pos;
                look.y = 0f;
                if (look.sqrMagnitude > 0.01f) g.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);
            }
            var body = new GameObject("Body").transform;
            body.SetParent(g, false);
            body.localScale = Vector3.one * size;
            var knit = PatchworkAssets.Knit(col);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(1.5f, 1.35f, 1.4f), knit, default(Vector3), body);
            // Big eyes (three of them, for an alien).
            var white = PatchworkAssets.Felt(Color.white);
            var black = PatchworkAssets.Plain(new Color(0.08f, 0.07f, 0.1f), 0.8f);
            float[] ex = { -0.3f, 0.3f, 0f };
            float[] ey = { 0.18f, 0.18f, 0.5f };
            float[] es = { 0.42f, 0.42f, 0.3f };
            for (int k = 0; k < 3; k++)
            {
                kit.Deco(PrimitiveType.Sphere, new Vector3(ex[k], ey[k], 0.58f), Vector3.one * es[k], white, default(Vector3), body);
                kit.Deco(PrimitiveType.Sphere, new Vector3(ex[k], ey[k], 0.58f + es[k] * 0.38f), Vector3.one * es[k] * 0.45f, black, default(Vector3), body);
            }
            // A happy mouth.
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.2f, 0.66f), new Vector3(0.36f, 0.12f, 0.1f), PatchworkAssets.Felt(new Color(0.55f, 0.15f, 0.25f)), default(Vector3), body);
            // Antennae with glowing pom-poms.
            var yarn = PatchworkAssets.Yarn(Color.Lerp(col, Color.black, 0.25f));
            for (int s = -1; s <= 1; s += 2)
            {
                Rod(kit, new Vector3(s * 0.25f, 0.6f, 0f), new Vector3(s * 0.45f, 1.15f, 0.05f), yarn, 0.06f, body);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.45f, 1.18f, 0.05f), Vector3.one * 0.2f, PatchworkAssets.Glow(new Color(1f, 0.9f, 0.4f), 1.2f), default(Vector3), body);
            }
            // Stubby arms (one waves) and three little feet.
            for (int s = -1; s <= 1; s += 2)
            {
                var arm = new GameObject("Arm").transform;
                arm.SetParent(body, false);
                arm.localPosition = new Vector3(s * 0.68f, 0f, 0f);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.2f, 0.12f, 0f), new Vector3(0.5f, 0.24f, 0.24f), knit, new Vector3(0f, 0f, s * 30f), arm);
                if (s > 0)
                {
                    var wave = arm.gameObject.AddComponent<Rocker>();
                    wave.Axis = Vector3.forward;
                    wave.Amount = 25f;
                    wave.Speed = 4f;
                }
            }
            for (int k = 0; k < 3; k++)
            {
                float a = (k * 120f + 30f) * Mathf.Deg2Rad;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.35f, -0.62f, Mathf.Sin(a) * 0.35f), new Vector3(0.3f, 0.2f, 0.3f), knit, default(Vector3), body);
            }
            var bob = body.gameObject.AddComponent<Spinner>();
            bob.Speed = 0f;
            bob.BobHeight = 0.12f * size;
            bob.BobSpeed = 1.3f + size;
            return g;
        }

        static void Gear(LevelKit kit, Vector3 pos, float r, Color col, float speed)
        {
            var g = kit.Group("Gear", pos);
            g.rotation = Quaternion.Euler(0f, 160f, 0f);
            var turn = new GameObject("Turn").transform;
            turn.SetParent(g, false);
            var m = PatchworkAssets.Felt(col);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(r * 2f, 0.2f, r * 2f), m, new Vector3(90f, 0f, 0f), turn);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(r * 0.6f, 0.24f, r * 0.6f), PatchworkAssets.Felt(Color.Lerp(col, Color.white, 0.5f)), new Vector3(90f, 0f, 0f), turn);
            int teeth = Mathf.RoundToInt(r * 8f);
            for (int k = 0; k < teeth; k++)
            {
                float a = k * 360f / teeth;
                var q = Quaternion.Euler(0f, 0f, a);
                kit.Deco(PrimitiveType.Cube, q * new Vector3(0f, r + 0.12f, 0f), new Vector3(0.3f, 0.32f, 0.2f), m, new Vector3(0f, 0f, a), turn);
            }
            var s = turn.gameObject.AddComponent<Spinner>();
            s.Axis = Vector3.forward;
            s.Speed = speed;
        }

        // The Cardboard Skiff, a little toy-sized version, sailing round and round the pod.
        static void MiniSkiff(LevelKit kit)
        {
            var orbit = kit.Group("Skiff", Vector3.zero);
            var o = orbit.gameObject.AddComponent<Orbiter>();
            o.Center = new Vector3(0f, 0f, 4f);
            o.Radius = 26f;
            o.RadiusZ = 22f;
            o.Height = 2.5f;
            o.Speed = 7f;
            o.Angle = -30f;
            o.Shake = 0.08f;
            var ship = kit.Group("SkiffModel", Vector3.zero);
            ship.SetParent(orbit, false);
            ship.localPosition = Vector3.zero;
            ship.localScale = Vector3.one * 0.32f;

            var hull = PatchworkAssets.Cardboard(Kraft);
            var light = PatchworkAssets.Cardboard(KraftLight);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.9f, 0f), new Vector3(7f, 1.8f, 14f), hull, default(Vector3), ship);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.9f, 7f), new Vector3(4.95f, 1.8f, 4.95f), hull, new Vector3(0f, 45f, 0f), ship);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.05f, 0f), new Vector3(6.6f, 0.1f, 13.6f), PatchworkAssets.Planks(new Color(0.8f, 0.64f, 0.46f)), default(Vector3), ship);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.6f, -4.5f), new Vector3(5f, 3.2f, 4f), light, default(Vector3), ship);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 3.3f, -4.5f), new Vector3(5.4f, 0.3f, 4.4f), hull, default(Vector3), ship);
            var pole = PatchworkAssets.Plain(new Color(0.98f, 0.8f, 0.3f), 0.4f);
            Rod(kit, new Vector3(0f, 0f, 2f), new Vector3(0f, 9f, 2f), pole, 0.35f, ship);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 8f, 1.2f), new Vector3(0.05f, 1.1f, 1.8f), PatchworkAssets.Felt(new Color(0.18f, 0.15f, 0.2f)), default(Vector3), ship);
            // Its patched balloon and ropes.
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 15f, 0f), new Vector3(14f, 9f, 20f), PatchworkAssets.BalloonQuilt(), default(Vector3), ship);
            var rope = PatchworkAssets.Yarn(new Color(0.96f, 0.92f, 0.82f));
            for (int s = -1; s <= 1; s += 2)
                for (int z = -1; z <= 1; z++)
                    Rod(kit, new Vector3(s * 3.3f, 0.3f, z * 5.5f), new Vector3(s * 4.8f, 11.5f, z * 6.5f), rope, 0.18f, ship);
            // Propellers at the stern.
            for (int s = -1; s <= 1; s += 2)
            {
                var prop = kit.Group("Propeller", Vector3.zero);
                prop.SetParent(ship, false);
                prop.localPosition = new Vector3(s * 2f, -0.8f, -7.4f);
                for (int b = 0; b < 3; b++)
                    kit.Deco(PrimitiveType.Cube, Quaternion.Euler(0f, 0f, b * 120f) * new Vector3(0f, 0.8f, 0f), new Vector3(0.5f, 1.6f, 0.08f), PatchworkAssets.Felt(LevelKit.Palette.Rose), new Vector3(0f, 0f, b * 120f), prop);
                var sp = prop.gameObject.AddComponent<Spinner>();
                sp.Axis = Vector3.forward;
                sp.Speed = 300f * s;
            }
            foreach (var r in ship.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // The launch tube Patch arrives through: a candy-striped tube poking out of the wall.
        static void LaunchTube(LevelKit kit)
        {
            const float deg = 140f, y = 2.3f;
            Vector3 d = Dir(deg);
            var tube = kit.Group("LaunchTube", d * 8.7f + Vector3.up * y);
            tube.rotation = Quaternion.FromToRotation(Vector3.up, -d);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.7f, 1.2f, 1.7f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.9f, 0.3f, 0.35f), new Color(0.99f, 0.95f, 0.88f)), default(Vector3), tube);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.2f, 0f), new Vector3(1.95f, 0.14f, 1.95f), PatchworkAssets.Felt(LevelKit.Palette.Gold), default(Vector3), tube);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.23f, 0f), new Vector3(1.4f, 0.14f, 1.4f), PatchworkAssets.Plain(new Color(0.08f, 0.06f, 0.1f), 0.2f), default(Vector3), tube);
            // A little brace under it.
            kit.Deco(PrimitiveType.Cube, d * 8.2f + Vector3.up * (y * 0.5f - 0.2f), new Vector3(0.3f, y - 0.4f, 0.3f), PatchworkAssets.Cardboard(KraftDark), new Vector3(0f, deg, 0f));

            var go = new GameObject("PodLaunch");
            go.transform.SetParent(kit.Root, false);
            var l = go.AddComponent<PodLaunch>();
            l.Tube = tube;
            l.Inside = d * 9.9f + Vector3.up * (y - 0.65f);
            l.Mouth = d * 7.3f + Vector3.up * (y - 0.65f);
            l.Land = LandPoint;
            l.CameraFrom = new Vector3(-3.6f, 3.4f, 4.2f);
            l.CameraAt = new Vector3(3.2f, 2.2f, -3.6f);
        }

        static void Sounds(LevelKit kit)
        {
            var root = kit.Group("PodSounds", Vector3.zero);
            // The hum of the pod drifting through space (no ticking clock any more).
            AmbientEmitter.Create(root, new Vector3(0f, 5f, 0f), Sfx.Ambient(Sfx.AmbientClip.Space), 0.55f, 30f, 60f);
        }
    }
}

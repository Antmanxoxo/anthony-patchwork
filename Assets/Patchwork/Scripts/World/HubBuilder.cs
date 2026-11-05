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
            var info = kit.Info;
            info.Id = SceneName;
            info.IsHub = true;
            info.Title = LevelCatalog.Skiff.Title;
            info.Subtitle = "Pick up the controller to choose where to go.";
            info.Spawn = new Vector3(0f, 0.1f, -2f);
            info.SpawnYaw = 0f;
            info.KillY = -10f;
            // A bright paper-white sky, like the box art's background.
            info.Sky = new Color(0.98f, 0.97f, 0.94f);
            info.FogStart = 45f;
            info.FogEnd = 160f;
            info.Saturation = 18f;
            info.Area.SetMinMax(new Vector3(-11f, -1f, -16f), new Vector3(11f, 14f, 11f));
            // Sunlight pours in through the window.
            info.SunEuler = new Vector3(38f, 165f, 0f);
            info.SunIntensity = 1.35f;
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
            Outside(kit);
            MiniSkiff(kit);
            Sounds(kit);
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
                        // The window: an invisible pane so you can't fall out.
                        t.name = "WindowPane";
                        t.GetComponent<Renderer>().enabled = false;
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
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.06f, 0f), new Vector3(19.2f, 0.05f, 19.2f), PatchworkAssets.Planks(new Color(0.84f, 0.66f, 0.48f)));
            // A round rug: zigzag felt with a rose felt border.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.5f), new Vector3(9.4f, 0.01f, 9.4f), PatchworkAssets.Felt(new Color(0.92f, 0.45f, 0.5f)));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.01f, 0.5f), new Vector3(8.6f, 0.01f, 8.6f),
                PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, new Color(0.45f, 0.75f, 0.95f), new Color(1f, 0.9f, 0.55f)));
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
            pad.rotation = Quaternion.Euler(0f, 180f, 0f);   // handles towards you
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

            // A puff of cloud under the planet, and a warm light on its front.
            SketchCloud(kit, GlobeCenter + new Vector3(0.6f, -3.4f, 0.8f), 1.5f);
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
            spot.Label = "Change the wallpaper";
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
            spot.Used = player =>
            {
                PodTheme.Next();
                showPages();
                Sfx.Play(SfxId.PageTurn, 0.6f);
                FxBurst.Spawn(FxBurst.Kind.Scraps, p + Vector3.up * 1.4f, new Color(1f, 0.85f, 0.6f));
                if (player.Game != null) player.Game.ShowToast("Wallpaper: " + PodTheme.Names[PodTheme.Current]);
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
            // A hanging light bulb in the middle of the pod.
            var cord = PatchworkAssets.Yarn(new Color(0.25f, 0.22f, 0.25f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 10.3f, 0f), new Vector3(0.05f, 3.1f, 0.05f), cord);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 7.35f, 0f), new Vector3(0.22f, 0.12f, 0.22f), PatchworkAssets.Plain(new Color(0.75f, 0.72f, 0.68f), 0.7f));
            var bulb = kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 7f, 0f), new Vector3(0.5f, 0.62f, 0.5f), PatchworkAssets.Glow(new Color(1f, 0.86f, 0.5f), 2.2f));
            bulb.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var lgo = new GameObject("BulbLight");
            lgo.transform.SetParent(kit.Root, false);
            lgo.transform.position = new Vector3(0f, 6.6f, 0f);
            var l = lgo.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.84f, 0.62f);
            l.range = 16f;
            l.intensity = 1.5f;
            l.shadows = LightShadows.None;

            // A polka-dot toadstool stool and a zigzag cushion.
            Vector3 st = new Vector3(6.2f, 0f, -3.4f);
            kit.Block(st + Vector3.up * 0.45f, new Vector3(1.4f, 0.9f, 1.4f), Color.white, "Stool").GetComponent<Renderer>().enabled = false;
            kit.Deco(PrimitiveType.Cylinder, st + Vector3.up * 0.35f, new Vector3(0.7f, 0.35f, 0.7f), PatchworkAssets.Felt(new Color(0.98f, 0.94f, 0.85f)));
            kit.Deco(PrimitiveType.Sphere, st + Vector3.up * 0.82f, new Vector3(1.7f, 0.6f, 1.7f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(0.9f, 0.25f, 0.25f), new Color(0.99f, 0.96f, 0.9f)));
            kit.Deco(PrimitiveType.Sphere, new Vector3(-6.4f, 0.3f, -2.4f), new Vector3(1.9f, 0.6f, 1.9f),
                PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, new Color(0.2f, 0.25f, 0.55f), new Color(1f, 0.8f, 0.28f)));

            // A bouncy pin-cushion, a beach ball and some building blocks.
            kit.AddBouncePad(new Vector3(-4.6f, 0f, -5.2f));
            Grabbable.BeachBall(kit, new Vector3(3f, 0.05f, -4.5f), 0.6f);
            Grabbable.ToyBlock(kit, new Vector3(-2.6f, 0.05f, -5.6f), 0.8f, new Color(0.95f, 0.45f, 0.45f), 15f);
            Grabbable.ToyBlock(kit, new Vector3(-1.6f, 0.05f, -6.2f), 0.8f, new Color(0.45f, 0.65f, 0.95f), -20f);
            Grabbable.ToyBlock(kit, new Vector3(-2.1f, 0.85f, -5.9f), 0.8f, new Color(1f, 0.85f, 0.35f), 40f);

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
            if (Prizes.HasDecoration(0))
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
            }
            // Paper Lanterns strung across the pod.
            if (Prizes.HasDecoration(1))
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
            }
            // Snow Globe: a tiny Skiff in a glass dome, on the desk.
            if (Prizes.HasDecoration(2))
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
            }
            // Pinwheels in two flowerpots either side of the Button Box doorway.
            if (Prizes.HasDecoration(3))
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
            }
        }

        // ---------------------------------------------------------------- Outside the window

        // A soft white cloud (the ink outlines make it look hand-drawn).
        static Transform SketchCloud(LevelKit kit, Vector3 pos, float size)
        {
            var g = kit.Group("Cloud", pos);
            var m = PatchworkAssets.Felt(new Color(1f, 1f, 0.99f));
            var shade = PatchworkAssets.Felt(new Color(0.86f, 0.86f, 0.88f));
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(2.2f, 1.1f, 1.1f) * size, m, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.8f, 0.45f, 0f) * size, new Vector3(1.3f, 1.2f, 1f) * size, m, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(-0.7f, 0.3f, 0.05f) * size, new Vector3(1.1f, 0.95f, 0.95f) * size, m, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.1f, -0.3f, 0.12f) * size, new Vector3(2f, 0.5f, 0.9f) * size, shade, default(Vector3), g);
            var s = g.gameObject.AddComponent<Spinner>();
            s.Speed = 0f;
            s.BobHeight = 0.3f * size;
            s.BobSpeed = 0.35f;
            return g;
        }

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

        // The bright, busy world outside, after the toy-box collage on the game's box art.
        static void Outside(LevelKit kit)
        {
            // Clouds above, below and to the sides.
            SketchCloud(kit, new Vector3(-14f, -2f, 26f), 3f);
            SketchCloud(kit, new Vector3(12f, 0.5f, 30f), 2.6f);
            SketchCloud(kit, new Vector3(-5f, 11f, 34f), 2f);
            SketchCloud(kit, new Vector3(15f, 10f, 23f), 1.6f);
            SketchCloud(kit, new Vector3(2f, -7f, 26f), 3.5f);
            SketchCloud(kit, new Vector3(-22f, 5f, 18f), 2.2f);
            SketchCloud(kit, new Vector3(22f, -3f, 15f), 2.5f);
            SketchCloud(kit, new Vector3(-30f, -8f, 40f), 4f);
            SketchCloud(kit, new Vector3(28f, 14f, 44f), 4f);

            // A trail of felt stars sweeping down from the top left, red to yellow.
            for (int i = 0; i < 9; i++)
            {
                float f = i / 8f;
                Vector3 p = new Vector3(Mathf.Lerp(-13f, -3.6f, f), Mathf.Lerp(12.5f, 7.2f, f) + Mathf.Sin(f * Mathf.PI) * 1.2f, Mathf.Lerp(25f, 19f, f));
                Color c = Color.Lerp(new Color(0.95f, 0.3f, 0.3f), new Color(1f, 0.85f, 0.3f), f);
                Star(kit, p, Mathf.Lerp(0.5f, 1.1f, f), c);
            }

            // A polka-dot toadstool floating by.
            var shroom = kit.Group("Toadstool", new Vector3(-8.6f, 6.4f, 20f));
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

            // Purple clockwork gears turning beside the planet.
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

            // A yarn bird flapping round the planet.
            kit.YarnBird(GlobeCenter + new Vector3(0f, 4.5f, 1f), LevelKit.Palette.Butter, true, 6.5f);
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

        static void Sounds(LevelKit kit)
        {
            var root = kit.Group("PodSounds", Vector3.zero);
            AmbientEmitter.Create(root, Dir(90f) * 8.5f + Vector3.up * 3.6f, Sfx.Ambient(Sfx.AmbientClip.Clock), 0.35f, 1.5f, 12f);
            AmbientEmitter.Create(root, new Vector3(0f, 4f, 11f), Sfx.Ambient(Sfx.AmbientClip.Balloon), 0.18f, 3f, 14f);
        }
    }
}

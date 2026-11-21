using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Patchwork
{
    // A photo tour of Toybox Town, the same kind of check the museum and the Pod had: asked for
    // by a file called take_tour1.txt in the project's Screenshots folder, the next time Level 1
    // loads the camera goes everywhere and saves the pictures into Screenshots/Level1Tour:
    //   - from high up all round the toybox, and straight down;
    //   - the landmarks (the square, the bandstand, the clock tower, the mill and the Dye Works,
    //     the barracks, the canal and the ravine, the wood, the farm, the bouncy castle, the
    //     clouds over the rooftops, the hidden rooms);
    //   - and a walk across the whole map on a grid, at a walking camera's height, two ways at
    //     each spot.
    // It also writes clipping_report.txt there: solid things sunk into each other, small things
    // floating off the ground, and things buried in it. Then it gives the camera back.
    [DefaultExecutionOrder(10000)]
    public class Level1PhotoTour : MonoBehaviour
    {
        static string Folder { get { return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), "Screenshots"); } }
        static string AskFile { get { return System.IO.Path.Combine(Folder, "take_tour1.txt"); } }
        static bool running;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Hook()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (s, m) => Check();
            Check();
        }

        static void Check()
        {
            if (!Application.isEditor || running || !System.IO.File.Exists(AskFile)) return;
            running = true;
            new GameObject("Level1PhotoTour").AddComponent<Level1PhotoTour>();
        }

        struct Shot
        {
            public string Name;
            public Vector3 From, To;
            public Shot(string name, Vector3 from, Vector3 to) { Name = name; From = from; To = to; }
        }

        // The ground (or whatever's on it) under a spot.
        static float GroundAt(float x, float z, float fallback = 0f)
        {
            RaycastHit hit;
            int mask = ~((1 << 2) | (1 << RenderOptimizer.GrassLayer));
            if (Physics.Raycast(new Vector3(x, 90f, z), Vector3.down, out hit, 140f, mask, QueryTriggerInteraction.Ignore)) return hit.point.y;
            return fallback;
        }

        // Looking from a spot at walking-camera height towards another.
        static Shot Walk(string name, float x, float z, float tx, float tz)
        {
            float g = GroundAt(x, z), tg = GroundAt(tx, tz, g);
            return new Shot(name, new Vector3(x, g + 2.6f, z), new Vector3(tx, tg + 1.2f, tz));
        }

        static Shot Look(string name, Vector3 from, Vector3 at) { return new Shot(name, from, at); }

        static List<Shot> Shots()
        {
            var s = new List<Shot>();
            // From high up, all round, and straight down.
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                s.Add(Look("Air_" + i * 45, new Vector3(Mathf.Sin(a) * 150f, 70f, Mathf.Cos(a) * 150f), Vector3.zero));
            }
            s.Add(Look("Air_top", new Vector3(0f, 190f, -1f), Vector3.zero));

            // The landmarks.
            s.Add(Look("Pod_landing", new Vector3(58f, 4f, -90f), new Vector3(50f, 1.5f, -103f)));
            s.Add(Look("Spawn_view", new Vector3(50f, 2.6f, -96f), new Vector3(30f, 2f, -60f)));
            s.Add(Look("Square_n", new Vector3(0f, 3f, -34f), new Vector3(0f, 1.5f, -14f)));
            s.Add(Look("Square_s", new Vector3(0f, 3f, 2f), new Vector3(0f, 1.5f, -20f)));
            s.Add(Look("Square_e", new Vector3(18f, 3f, -14f), new Vector3(-6f, 1.5f, -14f)));
            s.Add(Look("Square_w", new Vector3(-18f, 3f, -14f), new Vector3(6f, 1.5f, -14f)));
            s.Add(Look("Bandstand", new Vector3(9f, 3.5f, -3f), new Vector3(0f, 2f, 7f)));
            s.Add(Look("ClockTower", new Vector3(14f, 6f, 10f), new Vector3(0f, 10f, 27f)));
            s.Add(Look("ClockTower_top", new Vector3(10f, 28f, 14f), new Vector3(0f, 22f, 27f)));
            s.Add(Look("Mill", new Vector3(50f, 5f, -6f), new Vector3(64f, 4f, 8f)));
            s.Add(Look("DyeWorks_in", new Vector3(60f, 3f, 3f), new Vector3(70f, 1.5f, 12f)));
            s.Add(Look("Barracks", new Vector3(0f, 4f, 30f), new Vector3(0f, 2f, 45f)));
            s.Add(Look("Barracks_side", new Vector3(-22f, 3f, 38f), new Vector3(-12f, 1.5f, 46f)));
            s.Add(Look("Canal_n", new Vector3(36f, 2.5f, -60f), new Vector3(36f, -0.5f, -20f)));
            s.Add(Look("Canal_s", new Vector3(36f, 2.5f, 60f), new Vector3(36f, -0.5f, 20f)));
            s.Add(Look("Canal_wall", new Vector3(30f, 1.5f, 0f), new Vector3(40f, -2f, 0f)));
            s.Add(Look("Ravine_n", new Vector3(-44f, 3f, -60f), new Vector3(-44f, -4f, -20f)));
            s.Add(Look("Ravine_s", new Vector3(-44f, 3f, 60f), new Vector3(-44f, -4f, 20f)));
            s.Add(Look("Ravine_down", new Vector3(-36f, 4f, 0f), new Vector3(-48f, -5f, 0f)));
            s.Add(Look("SpindleHill", new Vector3(-30f, 14f, 6f), new Vector3(-73f, 8f, 30f)));
            s.Add(Look("Rooftops", new Vector3(14f, 14f, -44f), new Vector3(0f, 6f, -60f)));
            s.Add(Look("Weathervane_clouds", new Vector3(24f, 22f, -70f), new Vector3(12f, 26f, -92f)));
            s.Add(Look("Wood_gate", new Vector3(-64f, 3f, -72f), new Vector3(-71.5f, 2f, -79f)));
            s.Add(Look("Wood_moonglade", new Vector3(-80f, 3f, -42f), new Vector3(-90f, 1.5f, -50f)));
            s.Add(Look("Wood_willow", new Vector3(-98f, 3f, -48f), new Vector3(-108f, 2f, -56f)));
            s.Add(Look("Wood_camp", new Vector3(-90f, 3f, -10f), new Vector3(-99f, 1.2f, -16f)));
            s.Add(Look("Wood_hollowtree", new Vector3(-90f, 3f, -90f), new Vector3(-99f, 2f, -99f)));
            s.Add(Look("Katamari", new Vector3(-92f, 4f, 70f), new Vector3(-103f, 1f, 80f)));
            s.Add(Look("BouncyCastle", new Vector3(20f, 5f, 86f), new Vector3(20f, 3f, 102f)));
            s.Add(Look("Farm", new Vector3(90f, 4f, 90f), new Vector3(106f, 1.5f, 106f)));
            s.Add(Look("Meadow_east", new Vector3(80f, 3f, 20f), new Vector3(100f, 1.5f, 30f)));
            s.Add(Look("Meadow_south", new Vector3(60f, 3f, -90f), new Vector3(75f, 1.5f, -100f)));
            // The rooms out of sight (under the map): the shop, the grotto, the Big Top.
            s.Add(Look("Room_shop", new Vector3(250f, 3f, -192f), new Vector3(250f, 1.5f, -206f)));
            s.Add(Look("Room_grotto", new Vector3(250f, 3f, -254f), new Vector3(250f, 1.5f, -268f)));
            s.Add(Look("Room_bigtop", new Vector3(250f, 8f, -334f), new Vector3(250f, 3f, -350f)));

            // A walk across the whole map, every 30 metres, looking two ways at each spot (turned
            // a quarter each row, so every way gets looked at somewhere).
            int n = 0;
            for (float z = -100f; z <= 101f; z += 30f)
                for (float x = -100f; x <= 101f; x += 30f)
                {
                    int turn = n++ % 4;
                    Vector2[] dirs = { new Vector2(0f, 1f), new Vector2(1f, 0f), new Vector2(0f, -1f), new Vector2(-1f, 0f) };
                    Vector2 d1 = dirs[turn], d2 = dirs[(turn + 2) % 4];
                    string at = "Grid_" + (int)x + "_" + (int)z;
                    s.Add(Walk(at + "_a", x, z, x + d1.x * 25f, z + d1.y * 25f));
                    s.Add(Walk(at + "_b", x, z, x + d2.x * 25f, z + d2.y * 25f));
                }
            return s;
        }

        Camera shotCam;
        Vector3 camPos;
        Quaternion camRot;

        void LateUpdate()
        {
            if (shotCam != null) shotCam.transform.SetPositionAndRotation(camPos, camRot);
        }

        void Start() { StartCoroutine(Run()); }

        IEnumerator Run()
        {
            var game = PatchworkGame.Instance;
            while (game == null || game.CameraRig == null || game.Level == null || game.LevelRoot == null || game.Mode != UIMode.Gameplay)
            {
                yield return null;
                game = PatchworkGame.Instance;
            }
            if (game.Level.Id != Level1Builder.SceneName) { running = false; Destroy(gameObject); yield break; }   // (waits for Toybox Town)
            yield return new WaitForSecondsRealtime(2f);
            var cam = game.CameraRig.GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
            if (cam == null) { Finish(); yield break; }
            Debug.Log("[Level1] Photo tour starting.");

            string dir = System.IO.Path.Combine(Folder, "Level1Tour");
            System.IO.Directory.CreateDirectory(dir);
            foreach (var old in System.IO.Directory.GetFiles(dir, "*.png")) System.IO.File.Delete(old);
            System.IO.File.WriteAllText(System.IO.Path.Combine(dir, "clipping_report.txt"), Report(game.LevelRoot));

            game.CameraRig.enabled = false;
            if (game.Hud != null) game.Hud.enabled = false;
            float fov = cam.fieldOfView, far = cam.farClipPlane;
            cam.fieldOfView = 65f;
            cam.farClipPlane = Mathf.Max(far, 600f);
            shotCam = cam;
            var shots = Shots();
            for (int i = 0; i < shots.Count; i++)
            {
                var sh = shots[i];
                camPos = sh.From;
                camRot = Quaternion.LookRotation(sh.To - sh.From, Vector3.up);
                // (Long enough for the distance culling and the grass to catch up.)
                float until = Time.realtimeSinceStartup + 0.6f;
                while (Time.realtimeSinceStartup < until)
                {
                    game.CameraRig.enabled = false;
                    yield return null;
                }
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(dir, (i + 1).ToString("000") + "_" + sh.Name + ".png"));
                yield return new WaitForSecondsRealtime(0.2f);
            }
            shotCam = null;
            cam.fieldOfView = fov;
            cam.farClipPlane = far;
            game.CameraRig.enabled = true;
            if (game.Hud != null) game.Hud.enabled = true;
            Debug.Log("[Level1] Photo tour done: " + shots.Count + " pictures in " + dir);
            game.ShowToast("Toybox Town tour done: " + shots.Count + " pictures saved.");
            Finish();
        }

        void Finish()
        {
            if (System.IO.File.Exists(AskFile)) System.IO.File.Delete(AskFile);
            running = false;
            Destroy(gameObject);
        }

        static string PathOf(Transform t, Transform stop)
        {
            var parts = new List<string>();
            for (var x = t; x != null && x != stop; x = x.parent) parts.Insert(0, x.name);
            return string.Join("/", parts.ToArray());
        }

        static string V(Vector3 v) { return "(" + v.x.ToString("0.0") + ", " + v.y.ToString("0.00") + ", " + v.z.ToString("0.0") + ")"; }

        // Things that are meant to be off the ground.
        static readonly string[] Airborne = { "Cloud", "Bird", "Balloon", "Bunting", "Flag", "Lamp", "Lantern", "Light", "Star", "Mobile", "Kite", "Butterfly", "Bubble", "Anchor", "Hoop", "Ring", "Sign", "Banner", "Clock", "Roof", "Spool", "Heartstring", "Button", "Marble", "Prize", "Puff", "Fluff", "Wisp", "Rope", "Wire", "Bridge", "Platform", "Shelf", "Window", "Door", "Wall", "Chimney", "Vane" };

        static bool MeantToFloat(string path)
        {
            foreach (var a in Airborne) if (path.IndexOf(a, System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        // What pokes through what, judged by boxes, so the pictures say which really do.
        static string Report(Transform root)
        {
            var sb = new StringBuilder();
            Physics.SyncTransforms();
            var rends = root.GetComponentsInChildren<Renderer>(false);
            sb.AppendLine("Toybox Town clipping report. " + rends.Length + " renderers.");
            sb.AppendLine();

            sb.AppendLine("== Small things floating (bottom more than 0.3 above what's under them) ==");
            int floating = 0, buried = 0;
            var buriedLines = new StringBuilder();
            int mask = ~((1 << 2) | (1 << RenderOptimizer.GrassLayer));
            foreach (var r in rends)
            {
                if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
                var b = r.bounds;
                if (b.size.x > 3f || b.size.z > 3f || b.size.y > 4f) continue;
                if (Mathf.Abs(b.center.x) > 120f || Mathf.Abs(b.center.z) > 120f) continue;   // (the hidden rooms)
                string n = PathOf(r.transform, root);
                // What's directly under it (not counting itself or anything in its own group).
                Transform group = r.transform.parent != null && r.transform.parent != root ? r.transform.parent : r.transform;
                RaycastHit[] hits = Physics.RaycastAll(new Vector3(b.center.x, b.min.y + 0.05f, b.center.z), Vector3.down, 40f, mask, QueryTriggerInteraction.Ignore);
                float below = float.NegativeInfinity;
                foreach (var h in hits)
                {
                    if (h.collider.transform.IsChildOf(group)) continue;
                    below = Mathf.Max(below, h.point.y);
                }
                if (float.IsNegativeInfinity(below)) continue;
                float gap = b.min.y - below;
                if (gap > 0.3f && gap < 12f && !MeantToFloat(n) && floating < 300)
                {
                    sb.AppendLine(n + "  centre " + V(b.center) + "  " + gap.ToString("0.00") + " above what's under it");
                    floating++;
                }
                // Buried: its top is under the surface below its middle.
                RaycastHit top;
                if (Physics.Raycast(new Vector3(b.center.x, b.max.y + 30f, b.center.z), Vector3.down, out top, 60f, mask, QueryTriggerInteraction.Ignore)
                    && !top.collider.transform.IsChildOf(group) && top.point.y > b.max.y + 0.02f && top.point.y - b.max.y < 3f && buried < 200)
                {
                    buriedLines.AppendLine(n + "  centre " + V(b.center) + "  top " + b.max.y.ToString("0.00") + " under " + PathOf(top.collider.transform, root) + " at " + top.point.y.ToString("0.00"));
                    buried++;
                }
            }
            sb.AppendLine("(" + floating + ")");
            sb.AppendLine();
            sb.AppendLine("== Buried (top hidden under a surface) ==");
            sb.Append(buriedLines);
            sb.AppendLine("(" + buried + ")");
            sb.AppendLine();

            sb.AppendLine("== Solid things sinking into each other (colliders, deeper than 0.05) ==");
            var cols = new List<Collider>();
            foreach (var c in root.GetComponentsInChildren<Collider>(false)) if (!c.isTrigger && c.enabled) cols.Add(c);
            int found = 0;
            for (int i = 0; i < cols.Count && found < 400; i++)
                for (int j = i + 1; j < cols.Count && found < 400; j++)
                {
                    var a = cols[i]; var c = cols[j];
                    if (!a.bounds.Intersects(c.bounds)) continue;
                    if (a.transform.IsChildOf(c.transform) || c.transform.IsChildOf(a.transform)) continue;
                    if (a is MeshCollider && !((MeshCollider)a).convex && c is MeshCollider && !((MeshCollider)c).convex) continue;
                    string na = PathOf(a.transform, root), nc = PathOf(c.transform, root);
                    // (The ground pieces meet each other on purpose.)
                    if (na.Contains("Ground") && nc.Contains("Ground") || na.Contains("Bank") && nc.Contains("Bank")) continue;
                    Vector3 dir; float dist;
                    if (Physics.ComputePenetration(a, a.transform.position, a.transform.rotation, c, c.transform.position, c.transform.rotation, out dir, out dist) && dist > 0.05f)
                    {
                        sb.AppendLine(na + "  <->  " + nc + "  by " + dist.ToString("0.00") + " at " + V(a.bounds.center));
                        found++;
                    }
                }
            sb.AppendLine("(" + found + ")");
            sb.AppendLine();
            sb.AppendLine("== Everything, by top-level group (name, centre, size) ==");
            foreach (Transform t in root)
            {
                var rs = t.GetComponentsInChildren<Renderer>(false);
                if (rs.Length == 0) continue;
                var b = rs[0].bounds;
                foreach (var r in rs) b.Encapsulate(r.bounds);
                sb.AppendLine(t.name + "  centre " + V(b.center) + " size " + V(b.size) + "  (" + rs.Length + " pieces)");
            }
            return sb.ToString();
        }
    }
}

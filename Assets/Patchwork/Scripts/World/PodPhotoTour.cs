using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Patchwork
{
    // A photo tour of the Pod and the space round it, for checking that everything sits neatly
    // and nothing pokes through anything else. Asked for by a file called take_pod_tour.txt in
    // the project's Screenshots folder: the next time the Pod is loaded, the camera goes all
    // round the inside (from the middle, from the edges, low down and up high, the Trophy Room)
    // and all round the outside, and saves the pictures into Screenshots/PodTour. It also writes
    // clipping_report.txt there: anything that pokes through the pod's wall or under its floor,
    // and solid things that sink into each other. Then it gives the camera back.
    [DefaultExecutionOrder(10000)]
    public class PodPhotoTour : MonoBehaviour
    {
        static string Folder { get { return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), "Screenshots"); } }
        static string AskFile { get { return System.IO.Path.Combine(Folder, "take_pod_tour.txt"); } }
        static bool running;

        // The pod's egg shape (as HubBuilder builds it): the wall's radius at each height.
        static readonly float[] ProfileY = { 0f, 1.3f, 4f, 7f, 9.5f, 11.8f, 13.4f };
        static readonly float[] ProfileR = { 8.8f, 9.3f, 9.4f, 8.7f, 7f, 4.4f, 1.6f };

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
            new GameObject("PodPhotoTour").AddComponent<PodPhotoTour>();
        }

        struct Shot
        {
            public string Name;
            public Vector3 From, To;
            public Shot(string name, Vector3 from, Vector3 to) { Name = name; From = from; To = to; }
        }

        static float WallR(float y)
        {
            if (y <= ProfileY[0]) return ProfileR[0];
            for (int i = 1; i < ProfileY.Length; i++)
                if (y <= ProfileY[i]) return Mathf.Lerp(ProfileR[i - 1], ProfileR[i], (y - ProfileY[i - 1]) / (ProfileY[i] - ProfileY[i - 1]));
            return 0f;
        }

        static Vector3 Round(float deg, float r, float y) { float a = deg * Mathf.Deg2Rad; return new Vector3(Mathf.Sin(a) * r, y, Mathf.Cos(a) * r); }

        static List<Shot> Shots()
        {
            var s = new List<Shot>();
            // Inside, from the middle: all the way round at eye height, every 22.5 degrees.
            for (int i = 0; i < 16; i++)
            {
                float d = i * 22.5f;
                s.Add(new Shot("In_mid_" + (int)d, new Vector3(0f, 1.7f, 0f), Round(d, 8f, 1.6f)));
            }
            // Inside, from near the wall looking across, at two heights.
            for (int i = 0; i < 12; i++)
            {
                float d = i * 30f;
                s.Add(new Shot("In_edge_" + (int)d, Round(d, 6.6f, 2.4f), Round(d + 180f, 4f, 1.2f)));
                s.Add(new Shot("In_high_" + (int)d, Round(d, 5.2f, 6.5f), Round(d + 180f, 3f, 0.6f)));
            }
            // Low down, along the floor, to catch anything sinking into it or floating over it.
            for (int i = 0; i < 8; i++)
            {
                float d = i * 45f + 22.5f;
                s.Add(new Shot("In_floor_" + (int)d, Round(d, 2.5f, 0.45f), Round(d, 8.5f, 0.6f)));
            }
            // Close to the wall, side on, to catch things poking through it.
            for (int i = 0; i < 12; i++)
            {
                float d = i * 30f + 15f;
                s.Add(new Shot("In_wallside_" + (int)d, Round(d - 14f, 7.2f, 2.2f), Round(d + 14f, 7.8f, 2.2f)));
                s.Add(new Shot("In_wallup_" + (int)d, Round(d, 5.5f, 1.5f), Round(d, 8.3f, 5.5f)));
            }
            // Up at the roof, and down from it.
            s.Add(new Shot("In_roof", new Vector3(0f, 1.5f, 0.5f), new Vector3(0f, 13f, 0f)));
            s.Add(new Shot("In_topdown", new Vector3(0f, 11.5f, -0.5f), new Vector3(0f, 0f, 0f)));
            // The desk and window, the Trophy Room.
            s.Add(new Shot("Desk", new Vector3(0f, 2.6f, 2f), new Vector3(0f, 1.3f, 7.5f)));
            s.Add(new Shot("Desk_left", new Vector3(-4f, 2.2f, 3.5f), new Vector3(1f, 1.3f, 7.5f)));
            s.Add(new Shot("Desk_right", new Vector3(4f, 2.2f, 3.5f), new Vector3(-1f, 1.3f, 7.5f)));
            s.Add(new Shot("Window_up", new Vector3(0f, 1.6f, 3f), new Vector3(0f, 6f, 9f)));
            s.Add(new Shot("Trophy_door", new Vector3(0f, 2f, -4f), new Vector3(0f, 1.6f, -14f)));
            s.Add(new Shot("Trophy_in", new Vector3(0f, 2.4f, -10.2f), new Vector3(0f, 1.6f, -16.6f)));
            s.Add(new Shot("Trophy_quilt", new Vector3(0f, 2.2f, -12.4f), new Vector3(0f, 2.2f, -16.6f)));
            s.Add(new Shot("Trophy_shelf", new Vector3(1.8f, 2.2f, -13.6f), new Vector3(-3.4f, 1.8f, -13.6f)));
            s.Add(new Shot("Trophy_fame", new Vector3(-1.8f, 2.2f, -13.6f), new Vector3(3.4f, 2f, -13.6f)));
            s.Add(new Shot("Trophy_plinth", new Vector3(1.2f, 2f, -11.4f), new Vector3(0f, 1.4f, -13.2f)));
            s.Add(new Shot("Trophy_back", new Vector3(0f, 2.4f, -16f), new Vector3(0f, 1.4f, -8f)));
            // Outside: all round, close and far, from below, level and above.
            Vector3 c = new Vector3(0f, 6f, 0f);
            for (int i = 0; i < 12; i++)
            {
                float d = i * 30f;
                s.Add(new Shot("Out_near_" + (int)d, Round(d, 20f, 6f), c));
                s.Add(new Shot("Out_above_" + (int)d, Round(d, 20f, 18f), c));
                s.Add(new Shot("Out_below_" + (int)d, Round(d, 18f, -6f), c));
            }
            for (int i = 0; i < 8; i++)
            {
                float d = i * 45f + 15f;
                s.Add(new Shot("Out_far_" + (int)d, Round(d, 45f, 12f), c));
            }
            // Outside: the globe, the window from outside, the launch tube side.
            s.Add(new Shot("Out_globe", new Vector3(6f, 6f, 24f), new Vector3(0f, 4.4f, 16.5f)));
            s.Add(new Shot("Out_window", new Vector3(0f, 5f, 26f), new Vector3(0f, 4f, 7f)));
            s.Add(new Shot("Out_windowside", new Vector3(12f, 4f, 16f), new Vector3(0f, 4f, 8f)));
            s.Add(new Shot("Out_trophyroom", new Vector3(7f, 5f, -24f), new Vector3(0f, 2f, -13f)));
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
            if (!game.Level.IsHub) { running = false; Destroy(gameObject); yield break; }   // (waits for the Pod)
            yield return new WaitForSecondsRealtime(2f);
            var cam = game.CameraRig.GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
            if (cam == null) { Finish(); yield break; }
            Debug.Log("[Pod] Photo tour starting.");

            string dir = System.IO.Path.Combine(Folder, "PodTour");
            System.IO.Directory.CreateDirectory(dir);
            foreach (var old in System.IO.Directory.GetFiles(dir, "*.png")) System.IO.File.Delete(old);
            System.IO.File.WriteAllText(System.IO.Path.Combine(dir, "clipping_report.txt"), Report(game.LevelRoot));

            game.CameraRig.enabled = false;
            if (game.Hud != null) game.Hud.enabled = false;
            float fov = cam.fieldOfView;
            cam.fieldOfView = 70f;
            shotCam = cam;
            var shots = Shots();
            for (int i = 0; i < shots.Count; i++)
            {
                var sh = shots[i];
                camPos = sh.From;
                camRot = Quaternion.LookRotation(sh.To - sh.From, Vector3.up);
                float until = Time.realtimeSinceStartup + 0.5f;
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
            game.CameraRig.enabled = true;
            if (game.Hud != null) game.Hud.enabled = true;
            Debug.Log("[Pod] Photo tour done: " + shots.Count + " pictures in " + dir);
            game.ShowToast("Pod tour done: " + shots.Count + " pictures saved.");
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

        static string V(Vector3 v) { return "(" + v.x.ToString("0.00") + ", " + v.y.ToString("0.00") + ", " + v.z.ToString("0.00") + ")"; }

        // What pokes through what. Everything is judged by its box (the smallest upright box
        // round it), so round things near the wall can show up when they don't quite touch:
        // the pictures say which ones really do.
        static string Report(Transform root)
        {
            var sb = new StringBuilder();
            var rends = root.GetComponentsInChildren<Renderer>(false);
            sb.AppendLine("Pod clipping report. " + rends.Length + " renderers.");
            sb.AppendLine();
            sb.AppendLine("== Through the wall (inside things whose box reaches past the wall by more than 0.15) ==");
            foreach (var r in rends)
            {
                if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
                string n = PathOf(r.transform, root);
                if (n.Contains("PodWall") || n.Contains("Window") || n.Contains("Shell") || n.Contains("Rib") || n.Contains("Roof")) continue;
                var b = r.bounds;
                Vector3 cen = b.center;
                if (cen.y < 0f || cen.y > 13.4f || cen.z < -8.4f) continue;   // (outside, or in the Trophy Room)
                float cr = new Vector2(cen.x, cen.z).magnitude;
                if (cr > WallR(cen.y) - 0.3f) continue;                        // (part of the wall, or outside)
                float worst = 0f;
                for (int k = 0; k < 8; k++)
                {
                    Vector3 p = new Vector3((k & 1) == 0 ? b.min.x : b.max.x, (k & 2) == 0 ? b.min.y : b.max.y, (k & 4) == 0 ? b.min.z : b.max.z);
                    float over = new Vector2(p.x, p.z).magnitude - WallR(Mathf.Clamp(p.y, 0f, 13.4f));
                    worst = Mathf.Max(worst, over);
                }
                if (worst > 0.15f) sb.AppendLine(n + "  centre " + V(cen) + " size " + V(b.size) + "  past the wall by " + worst.ToString("0.00"));
            }
            sb.AppendLine();
            sb.AppendLine("== Under the floor (inside things whose bottom is below the floor by more than 0.05) ==");
            foreach (var r in rends)
            {
                string n = PathOf(r.transform, root);
                var b = r.bounds;
                if (b.center.y < 0f || b.center.y > 13f) continue;
                if (new Vector2(b.center.x, b.center.z).magnitude > 8.6f && b.center.z > -8.4f) continue;
                if (n.Contains("Floor") || n.Contains("Rug") || n.Contains("Board")) continue;
                if (b.min.y < -0.05f) sb.AppendLine(n + "  bottom " + b.min.y.ToString("0.00") + " size " + V(b.size));
            }
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
                    Vector3 dir; float dist;
                    if (Physics.ComputePenetration(a, a.transform.position, a.transform.rotation, c, c.transform.position, c.transform.rotation, out dir, out dist) && dist > 0.05f)
                    {
                        string na = PathOf(a.transform, root), nc = PathOf(c.transform, root);
                        if (na.Contains("PodWall") && nc.Contains("PodWall")) continue;
                        sb.AppendLine(na + "  <->  " + nc + "  by " + dist.ToString("0.00") + " at " + V(a.bounds.center));
                        found++;
                    }
                }
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

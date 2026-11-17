using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // A photo tour of the Oceanic Museum, for checking the whole level over at a glance
    // (Patchwork > Level 2 Photo Tour in the editor). The camera goes round every room, four
    // corners each plus a few special views, and saves a picture of each into the project's
    // Screenshots/Level2Tour folder. Then it gives the camera back.
    public static partial class Level2Builder
    {
        public const string TourPref = "pw_l2_tour";

        // Asked for from the menu, or by a file called take_tour.txt in the project's
        // Screenshots folder (so it can be asked for from outside Unity too).
        static string TourFile { get { return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), "Screenshots", "take_tour.txt"); } }

        static bool TourWanted()
        {
            if (!Application.isEditor) return false;
            return PlayerPrefs.GetInt(TourPref, 0) == 1 || System.IO.File.Exists(TourFile);
        }

        struct TourShot
        {
            public string Name;
            public Vector3 From, To;
            public TourShot(string name, Vector3 from, Vector3 to) { Name = name; From = from; To = to; }
        }

        static void AddCorners(List<TourShot> shots, string room, float x0, float x1, float z0, float z1, float floor, float height)
        {
            float inset = 2.5f, up = Mathf.Min(6f, height * 0.45f);
            Vector3[] c =
            {
                new Vector3(x0 + inset, floor + up, z0 + inset), new Vector3(x1 - inset, floor + up, z0 + inset),
                new Vector3(x1 - inset, floor + up, z1 - inset), new Vector3(x0 + inset, floor + up, z1 - inset)
            };
            for (int i = 0; i < 4; i++)
            {
                Vector3 far = c[(i + 2) % 4];
                shots.Add(new TourShot(room + "_" + (i + 1), c[i], new Vector3(far.x, floor + 1f, far.z)));
            }
        }

        static List<TourShot> TourShots()
        {
            var s = new List<TourShot>();
            s.Add(new TourShot("Arrival_pod", PodLanding + new Vector3(7f, 4f, -8f), PodLanding + Vector3.up * 1.5f));
            s.Add(new TourShot("Arrival_toDoors", SpawnPoint + new Vector3(0f, 3f, -4f), new Vector3(0f, 3f, -109f)));
            AddCorners(s, "Atrium", -AtriumHalf, AtriumHalf, AtriumZ0, AtriumZ1, 0f, AtriumTop);
            s.Add(new TourShot("Atrium_doors", new Vector3(0f, 4f, -96f), new Vector3(0f, 2f, -109f)));
            s.Add(new TourShot("Atrium_WicksPost", new Vector3(8f, 3f, -98f), new Vector3(11f, 1.5f, -107f)));
            AddCorners(s, "DeepTime", HistX0, HistX1, HistZ0, HistZ1, 0f, HistTop);
            AddCorners(s, "Kelp", KelpX0, KelpX1, KelpZ0, KelpZ1, 0f, KelpTop);
            s.Add(new TourShot("Tunnel_east", new Vector3(-27f, 1.8f, TunnelZ), new Vector3(10f, 1.4f, TunnelZ)));
            s.Add(new TourShot("Tunnel_west", new Vector3(17f, 1.8f, TunnelZ), new Vector3(-20f, 1.4f, TunnelZ)));
            AddCorners(s, "Ruins", RuinX0, RuinX1, RuinZ0, RuinZ1, 0f, RuinTop);
            AddCorners(s, "HallOfArms", ArmsX0, ArmsX1, ArmsZ0, ArmsZ1, 0f, ArmsTop);
            s.Add(new TourShot("HallOfArms_door", new Vector3(80f, 3f, -6f), new Vector3(80f, 2f, 6f)));
            s.Add(new TourShot("Gallery_north", new Vector3(36f, DeckY + 2.5f, -38.5f), new Vector3(36f, DeckY + 1.5f, -10f)));
            s.Add(new TourShot("Gallery_south", new Vector3(36f, DeckY + 2.5f, -11f), new Vector3(36f, DeckY + 1.5f, -41f)));
            // The Star Corridor at a walking camera's height, every few yards, both ways.
            foreach (float z in new[] { -36f, -31f, -26f, -21f, -16f })
            {
                s.Add(new TourShot("Corridor_walk_n" + (int)-z, new Vector3(36f, DeckY + 2.2f, z - 2f), new Vector3(36f, DeckY + 1.8f, z + 6f)));
                s.Add(new TourShot("Corridor_walk_s" + (int)-z, new Vector3(36f, DeckY + 2.2f, z + 2f), new Vector3(36f, DeckY + 1.8f, z - 6f)));
            }
            s.Add(new TourShot("Corridor_clocks", new Vector3(37.5f, DeckY + 2.2f, -38f), new Vector3(47f, DeckY + 1.6f, -38f)));
            s.Add(new TourShot("Corridor_planets", new Vector3(38f, DeckY + 2.2f, -13f), new Vector3(52f, DeckY + 1.8f, -13f)));
            s.Add(new TourShot("Corridor_moon", new Vector3(51f, DeckY + 2.2f, -14f), new Vector3(51f, DeckY + 1.8f, -2f)));
            s.Add(new TourShot("Corridor_comets", new Vector3(52f, DeckY + 2.2f, 0f), new Vector3(36f, DeckY + 1.8f, 0f)));
            s.Add(new TourShot("Bridge_in", new Vector3(36f, DeckY + 2.4f, 6f), new Vector3(30f, DeckY + 1.6f, 12f)));
            s.Add(new TourShot("Bridge_doorway", new Vector3(36f, DeckY + 2.2f, 1f), new Vector3(36f, DeckY + 1.8f, 12f)));
            s.Add(new TourShot("Bridge_landing", new Vector3(31f, DeckY + 2.2f, 12f), new Vector3(42f, DeckY + 1.4f, 11f)));
            s.Add(new TourShot("Bridge_across", new Vector3(30f, DeckY + 3f, 12.5f), new Vector3(2f, DeckY + 1.5f, 11f)));
            s.Add(new TourShot("Bridge_west", new Vector3(6f, DeckY + 2.4f, 13f), new Vector3(1f, DeckY + 1.5f, 5f)));
            s.Add(new TourShot("Atrium_middle_n", new Vector3(0f, 3f, -95f), new Vector3(0f, 2f, -50f)));
            s.Add(new TourShot("Atrium_middle_w", new Vector3(10f, 3f, -78f), new Vector3(-30f, 2f, -78f)));
            s.Add(new TourShot("HallOfArms_aisle", new Vector3(80f, 2.6f, 1f), new Vector3(80f, 1.6f, -45f)));
            Vector3 obs = new Vector3(0f, DeckY, -20f);
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f + 0.4f;
                s.Add(new TourShot("Observatory_" + (i + 1), obs + new Vector3(Mathf.Cos(a) * 19f, 6f, Mathf.Sin(a) * 19f), obs + Vector3.up * 2f));
            }
            AddCorners(s, "KeepersRoom", -88f, -34f, 78f, 94f, 0f, 8f);
            AddCorners(s, "TankHall", -88f, -34f, 96f, 178f, 8f, 16f);
            Vector3[] tanks = { new Vector3(-74.5f, -5f, 104f), new Vector3(-46.5f, -14f, 104f), new Vector3(-74.5f, -6f, 140f), new Vector3(-46.5f, -4f, 140f) };
            string[] names = { "Tank_Coral", "Tank_Lantern", "Tank_Frozen", "Tank_Mangrove" };
            for (int i = 0; i < 4; i++) s.Add(new TourShot(names[i], tanks[i], tanks[i] + new Vector3(0f, -1f, 14f)));
            return s;
        }

        public static IEnumerator RunTour()
        {
            // (It doesn't wait for any opening scene to finish: it simply takes the camera.)
            var game = PatchworkGame.Instance;
            while (game == null || game.CameraRig == null) { yield return null; game = PatchworkGame.Instance; }
            yield return new WaitForSecondsRealtime(2f);
            Debug.Log("[Level2] Photo tour starting.");
            var cam = game.CameraRig.GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
            if (cam == null) yield break;
            string dir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), "Screenshots", "Level2Tour");
            System.IO.Directory.CreateDirectory(dir);
            foreach (var old in System.IO.Directory.GetFiles(dir, "*.png")) System.IO.File.Delete(old);
            game.CameraRig.enabled = false;
            if (game.Hud != null) game.Hud.enabled = false;
            var shots = TourShots();
            for (int i = 0; i < shots.Count; i++)
            {
                var sh = shots[i];
                // Held in place for a moment (at the end of every frame, so nothing else that moves
                // the camera, a cutscene say, can take it back), then the picture.
                float until = Time.realtimeSinceStartup + 0.6f;
                while (Time.realtimeSinceStartup < until)
                {
                    game.CameraRig.enabled = false;
                    cam.transform.position = sh.From;
                    cam.transform.rotation = Quaternion.LookRotation(sh.To - sh.From, Vector3.up);
                    yield return new WaitForEndOfFrame();
                }
                cam.transform.position = sh.From;
                cam.transform.rotation = Quaternion.LookRotation(sh.To - sh.From, Vector3.up);
                ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(dir, (i + 1).ToString("00") + "_" + sh.Name + ".png"));
                yield return new WaitForSecondsRealtime(0.25f);
            }
            game.CameraRig.enabled = true;
            if (game.Hud != null) game.Hud.enabled = true;
            PlayerPrefs.SetInt(TourPref, 0);
            PlayerPrefs.Save();
            if (System.IO.File.Exists(TourFile)) System.IO.File.Delete(TourFile);
            Debug.Log("[Level2] Photo tour done: " + shots.Count + " pictures in " + dir);
            game.ShowToast("Photo tour done: " + shots.Count + " pictures saved.");
        }
    }

    public class Level2PhotoTour : MonoBehaviour
    {
        void Start() { StartCoroutine(Level2Builder.RunTour()); }
    }
}

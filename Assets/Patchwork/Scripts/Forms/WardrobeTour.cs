using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // A photo tour of the wardrobe, for checking every look at a glance. Asked for by a file
    // called take_wardrobe.txt in the project's Screenshots folder: the next time a level is
    // being played, the camera turns to Patch and takes pictures of every outfit (front and
    // back), colour scheme and fabric on each of his three forms, saved into
    // Screenshots/Wardrobe. Then it puts his own look back and gives the camera back.
    public class WardrobeTour : MonoBehaviour
    {
        static string Folder { get { return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), "Screenshots"); } }
        static string AskFile { get { return System.IO.Path.Combine(Folder, "take_wardrobe.txt"); } }
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
            new GameObject("WardrobeTour").AddComponent<WardrobeTour>();
        }

        void Start() { StartCoroutine(Run()); }

        IEnumerator Run()
        {
            var game = PatchworkGame.Instance;
            while (game == null || game.Player == null || game.CameraRig == null || game.Mode != UIMode.Gameplay)
            {
                yield return null;
                game = PatchworkGame.Instance;
            }
            yield return new WaitForSecondsRealtime(2f);
            var cam = game.CameraRig.GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
            if (cam == null) { Finish(); yield break; }
            Debug.Log("[Wardrobe] Photo tour starting.");

            string dir = System.IO.Path.Combine(Folder, "Wardrobe");
            System.IO.Directory.CreateDirectory(dir);
            foreach (var old in System.IO.Directory.GetFiles(dir, "*.png")) System.IO.File.Delete(old);

            int myColour = GameState.ColourScheme, myFabric = GameState.Fabric, myOutfit = GameState.Outfit;
            var player = game.Player;
            var vis = player.Visuals;
            game.CameraRig.enabled = false;
            if (game.Hud != null) game.Hud.enabled = false;

            FormType[] forms = { FormType.Yarn, FormType.Water, FormType.Wind };
            int n = 0;
            foreach (var f in forms)
            {
                vis.SetForm(f, false);

                // Every outfit, plain colours, from the front and from behind.
                GameState.ColourScheme = 0;
                GameState.Fabric = 0;
                for (int o = 0; o < Wardrobe.OutfitNames.Length; o++)
                {
                    GameState.Outfit = o;
                    vis.ApplyLook();
                    string nm = f + "_Outfit" + o + "_" + Clean(Wardrobe.OutfitNames[o]);
                    yield return Shot(game, cam, player.transform, 20f, 1f, dir, (++n).ToString("000") + "_" + nm + "_front.png");
                    yield return Shot(game, cam, player.transform, 155f, 1f, dir, (++n).ToString("000") + "_" + nm + "_back.png");
                    if (o == 6) yield return Shot(game, cam, player.transform, 70f, 1f, dir, (++n).ToString("000") + "_" + nm + "_side.png");
                }

                // Every colour scheme, no outfit.
                GameState.Outfit = 0;
                for (int c = 0; c < Wardrobe.ColourNames.Length; c++)
                {
                    GameState.ColourScheme = c;
                    vis.ApplyLook();
                    yield return Shot(game, cam, player.transform, 25f, 1f, dir, (++n).ToString("000") + "_" + f + "_Colour" + c + "_" + Clean(Wardrobe.ColourNames[c]) + ".png");
                }

                // Every fabric, close up, in his own colours and in a second scheme.
                for (int fab = 0; fab < Wardrobe.FabricNames.Length; fab++)
                {
                    GameState.Fabric = fab;
                    GameState.ColourScheme = 0;
                    vis.ApplyLook();
                    yield return Shot(game, cam, player.transform, 30f, 0.55f, dir, (++n).ToString("000") + "_" + f + "_Fabric" + fab + "_" + Clean(Wardrobe.FabricNames[fab]) + ".png");
                    GameState.ColourScheme = 4;
                    vis.ApplyLook();
                    yield return Shot(game, cam, player.transform, 30f, 0.55f, dir, (++n).ToString("000") + "_" + f + "_Fabric" + fab + "_" + Clean(Wardrobe.FabricNames[fab]) + "_dyed.png");
                }
                GameState.Fabric = 0;
                GameState.ColourScheme = 0;
            }

            GameState.ColourScheme = myColour;
            GameState.Fabric = myFabric;
            GameState.Outfit = myOutfit;
            vis.ApplyLook();
            vis.SetForm(player.Forms.Current, false);
            pinned = null;
            shotCam = null;
            game.CameraRig.enabled = true;
            if (game.Hud != null) game.Hud.enabled = true;
            Debug.Log("[Wardrobe] Photo tour done: " + n + " pictures in " + dir);
            game.ShowToast("Wardrobe tour done: " + n + " pictures saved.");
            Finish();
        }

        static string Clean(string s)
        {
            var b = new System.Text.StringBuilder();
            foreach (char ch in s) if (char.IsLetterOrDigit(ch)) b.Append(ch);
            return b.ToString();
        }

        void Finish()
        {
            pinned = null;
            shotCam = null;
            if (System.IO.File.Exists(AskFile)) System.IO.File.Delete(AskFile);
            running = false;
            Destroy(gameObject);
        }

        // Keeps him still and facing the same way while the pictures are taken (after
        // everything else has moved him each frame).
        static Transform pinned;
        static Vector3 pinPos;
        static Quaternion pinRot;
        static Camera shotCam;
        static Vector3 camPos;
        static Quaternion camRot;

        void LateUpdate()
        {
            if (pinned != null) pinned.SetPositionAndRotation(pinPos, pinRot);
            if (shotCam != null) shotCam.transform.SetPositionAndRotation(camPos, camRot);
        }

        // Looks at Patch from an angle round him (0 = straight in front), held for a moment
        // so the new look settles, then takes the picture. zoom < 1 comes in closer.
        static IEnumerator Shot(PatchworkGame game, Camera cam, Transform who, float angle, float zoom, string dir, string file)
        {
            if (pinned != who) { pinned = who; pinPos = who.position; pinRot = who.rotation; }
            shotCam = cam;
            var vis = game.Player.Visuals;
            float until = Time.realtimeSinceStartup + 0.5f;
            while (true)
            {
                // (Framed by how tall the form is, not by the box round everything on him:
                // a stray bubble or a mesh with an out-of-date box once sent the camera out
                // into space.)
                float size = Mathf.Clamp(vis.FormHeight, 0.4f, 2.5f);
                Bounds b = new Bounds(vis.Root.position + vis.Root.up * size * 0.5f, Vector3.one * size);
                Vector3 front = Vector3.ProjectOnPlane(vis.Root.forward, Vector3.up).normalized;
                Vector3 dirOut = Quaternion.AngleAxis(angle, Vector3.up) * front;
                // (A 60 degree view fits him with a little room round him.)
                float fov = cam.fieldOfView > 1f ? cam.fieldOfView : 60f;
                float dist = size * 0.5f / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * 1.25f * zoom + cam.nearClipPlane;
                Vector3 look = b.center + Vector3.up * size * (zoom < 0.8f ? 0.12f : 0f);
                camPos = look + dirOut * dist + Vector3.up * size * 0.18f;
                camRot = Quaternion.LookRotation(look - camPos, Vector3.up);
                game.CameraRig.enabled = false;
                cam.transform.SetPositionAndRotation(camPos, camRot);
                if (Time.realtimeSinceStartup >= until) break;
                yield return null;
            }
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(dir, file));
            yield return new WaitForSecondsRealtime(0.25f);
        }
    }
}

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Patchwork.EditorTools
{
    // Patchwork menu: one-click Windows build, and the movement Test Gym.
    public static class PatchworkBuild
    {
        const string BuildFolder = "Builds/Windows";

        [MenuItem("Patchwork/Build for Windows", false, 60)]
        static void BuildWindows()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                EditorUtility.DisplayDialog("Patchwork build", "No scenes are ticked in File > Build Profiles.", "OK");
                return;
            }

            string exe = Path.Combine(BuildFolder, PlayerSettings.productName + ".exe");
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = exe,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Patchwork: Windows build finished (" + (summary.totalSize / 1048576UL) + " MB) in " + BuildFolder);
                EditorUtility.RevealInFinder(exe);
            }
            else
            {
                Debug.LogError("Patchwork: Windows build " + summary.result + " with " + summary.totalErrors + " error(s). See the Console.");
            }
        }

        [MenuItem("Patchwork/Play Test Gym", false, 21)]
        static void PlayGym()
        {
            if (EditorApplication.isPlaying) return;
            SessionState.SetBool("Patchwork.PlayGym", true);
            EditorApplication.isPlaying = true;
        }
    }
}

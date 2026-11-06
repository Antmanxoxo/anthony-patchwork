using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace Patchwork.EditorTools
{
    // Runs before every build (any build profile, or Patchwork > Build for Windows).
    //
    // The game builds its look (bloom, colour grading, tint, vignette) out of Volume profiles made
    // in code while it runs, so URP can't see them when it decides which post-processing shaders
    // to keep. With "strip unused post-processing" on it throws some away, and a built game that's
    // missing any of them switches post-processing off completely: the build looks flat and the
    // Player.log fills with "has been stripped from the build" errors. This keeps that setting off.
    //
    // It also:
    //  - keeps the fog shaders (the haze is turned on from code, so Unity can't see it's used),
    //  - puts DirectX 11 first for Windows (it works on far more graphics cards and drivers than
    //    DirectX 12; DirectX 12 is still there as a fallback),
    //  - stamps a build number the title screen shows, so you can tell which build is running,
    //  - and after the build, zips it up ready to send (without the "don't ship" folders), as
    //    "Patchwork Build N.zip" next to the build folder.
    public class PatchworkBuildChecks : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        const string StampPath = "Assets/Patchwork/Resources/PatchworkBuild.txt";
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:UniversalRenderPipelineGlobalSettings"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string full = System.IO.Path.GetFullPath(path);
                if (!System.IO.File.Exists(full)) continue;
                string text = System.IO.File.ReadAllText(full);
                string fixedText = text
                    .Replace("m_StripUnusedPostProcessingVariantsAndResources: 1", "m_StripUnusedPostProcessingVariantsAndResources: 0")
                    .Replace("m_StripUnusedPostProcessingVariants: 1", "m_StripUnusedPostProcessingVariants: 0");
                if (fixedText == text) continue;
                System.IO.File.WriteAllText(full, fixedText);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                Debug.Log("Patchwork: turned off URP's 'strip unused post-processing' so the build keeps its look (" + path + ").");
            }
            KeepFog();
            if (report.summary.platform == BuildTarget.StandaloneWindows64 || report.summary.platform == BuildTarget.StandaloneWindows)
            {
                var target = report.summary.platform;
                var apis = PlayerSettings.GetGraphicsAPIs(target);
                if (PlayerSettings.GetUseDefaultGraphicsAPIs(target) || apis.Length == 0 || apis[0] != GraphicsDeviceType.Direct3D11)
                {
                    PlayerSettings.SetUseDefaultGraphicsAPIs(target, false);
                    PlayerSettings.SetGraphicsAPIs(target, new[] { GraphicsDeviceType.Direct3D11, GraphicsDeviceType.Direct3D12 });
                    Debug.Log("Patchwork: Windows builds now use DirectX 11 first (more graphics cards supported).");
                }
            }
            StampBuild();
        }

        static void KeepFog()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
            if (assets == null || assets.Length == 0) return;
            var so = new SerializedObject(assets[0]);
            bool changed = false;
            changed |= SetInt(so, "m_FogStripping", 1);
            changed |= SetInt(so, "m_FogKeepLinear", 1);
            changed |= SetInt(so, "m_FogKeepExp", 1);
            changed |= SetInt(so, "m_FogKeepExp2", 1);
            if (changed)
            {
                so.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
                Debug.Log("Patchwork: kept the fog shaders in the build.");
            }
        }

        static bool SetInt(SerializedObject so, string name, int value)
        {
            var p = so.FindProperty(name);
            if (p == null) return false;
            if (p.propertyType == SerializedPropertyType.Boolean) { if (p.boolValue == (value != 0)) return false; p.boolValue = value != 0; return true; }
            if (p.intValue == value) return false;
            p.intValue = value;
            return true;
        }

        static void StampBuild()
        {
            int n = 0;
            string full = Path.GetFullPath(StampPath);
            if (File.Exists(full))
            {
                string old = File.ReadAllText(full).Replace("Build", "").Trim();
                int.TryParse(old, out n);
            }
            n++;
            File.WriteAllText(full, "Build " + n);
            AssetDatabase.ImportAsset(StampPath, ImportAssetOptions.ForceUpdate);
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.result == BuildResult.Failed || report.summary.result == BuildResult.Cancelled) return;
            string exe = report.summary.outputPath;
            string folder = Path.GetDirectoryName(exe);
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return;
            string stamp = File.Exists(Path.GetFullPath(StampPath)) ? File.ReadAllText(Path.GetFullPath(StampPath)).Trim() : "Build";
            string zip = Path.Combine(Path.GetDirectoryName(folder) ?? folder, PlayerSettings.productName + " " + stamp + ".zip");
            try
            {
                if (File.Exists(zip)) File.Delete(zip);
                using (var fs = new FileStream(zip, FileMode.Create))
                using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
                {
                    string top = new DirectoryInfo(folder).Name;
                    foreach (var file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
                    {
                        string rel = file.Substring(folder.Length).TrimStart('/', '\\');
                        if (rel.Contains("DoNotShip") || rel.Contains("DontShip") || rel.EndsWith("Patchwork Log.txt")) continue;
                        var entry = archive.CreateEntry(top + "/" + rel.Replace('\\', '/'), System.IO.Compression.CompressionLevel.Optimal);
                        using (var es = entry.Open())
                        using (var src = File.OpenRead(file)) src.CopyTo(es);
                    }
                }
                Debug.Log("Patchwork: build zipped and ready to send: " + zip);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Patchwork: couldn't zip the build (" + e.Message + "). Zip the whole build folder yourself.");
            }
        }
    }
}

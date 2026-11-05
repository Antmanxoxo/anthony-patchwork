using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Patchwork.EditorTools
{
    // Runs before every build (any build profile, or Patchwork > Build for Windows).
    //
    // The game builds its look (bloom, colour grading, tint, vignette) out of Volume profiles made
    // in code while it runs, so URP can't see them when it decides which post-processing shaders
    // to keep. With "strip unused post-processing" on it throws some away, and a built game that's
    // missing any of them switches post-processing off completely: the build looks flat and the
    // Player.log fills with "has been stripped from the build" errors. This keeps that setting off.
    public class PatchworkBuildChecks : IPreprocessBuildWithReport
    {
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
        }
    }
}

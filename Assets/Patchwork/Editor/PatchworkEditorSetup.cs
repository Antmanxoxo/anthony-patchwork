using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Patchwork.EditorTools
{
    // Editor helpers so I can just hit Play:
    // Play always starts in the Hub, the Hub opens the first time the project loads,
    // and a URP material gets saved in Resources so the Lit shader isn't stripped from builds.
    // There's also a Patchwork menu with shortcuts.
    [InitializeOnLoad]
    public static class PatchworkEditorSetup
    {
        public const string HubPath = "Assets/Patchwork/Scenes/Hub.unity";
        public const string Level1Path = "Assets/Patchwork/Scenes/Level1.unity";
        const string StartFromHubKey = "Patchwork.PlayFromHub";
        const string OpenedHubKey = "Patchwork.OpenedHubOnce";
        const string MaterialFolder = "Assets/Patchwork/Resources";
        const string MaterialPath = "Assets/Patchwork/Resources/PW_Lit.mat";

        static PatchworkEditorSetup()
        {
            EditorApplication.delayCall += Setup;
        }

        static void Setup()
        {
            ApplyPlayModeStartScene();
            EnsureLitMaterial();
            EnsureVariantMaterials();

            if (!SessionState.GetBool(OpenedHubKey, false))
            {
                SessionState.SetBool(OpenedHubKey, true);
                var active = EditorSceneManager.GetActiveScene();
                bool inPatchworkScene = active.path == HubPath || active.path == Level1Path;
                if (!inPatchworkScene && !EditorApplication.isPlayingOrWillChangePlaymode && !active.isDirty
                    && AssetDatabase.LoadAssetAtPath<SceneAsset>(HubPath) != null)
                {
                    EditorSceneManager.OpenScene(HubPath);
                }
            }
        }

        static void ApplyPlayModeStartScene()
        {
            bool fromHub = EditorPrefs.GetBool(StartFromHubKey, true);
            EditorSceneManager.playModeStartScene = fromHub ? AssetDatabase.LoadAssetAtPath<SceneAsset>(HubPath) : null;
        }

        static void EnsureLitMaterial()
        {
            if (AssetDatabase.LoadAssetAtPath<Material>(MaterialPath) != null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) return;
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
                AssetDatabase.CreateFolder("Assets/Patchwork", "Resources");
            var mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, MaterialPath);
            AssetDatabase.SaveAssets();
        }

        // The game turns on shader features at runtime (bumpy textures, glow, see-through bubble).
        // Builds only keep shader versions some material in the build uses, so these three
        // materials in Resources make sure those versions are kept.
        static void EnsureVariantMaterials()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) return;
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
                AssetDatabase.CreateFolder("Assets/Patchwork", "Resources");
            bool made = false;

            string bumpPath = MaterialFolder + "/PW_Keep_Bumpy.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(bumpPath) == null)
            {
                var m = new Material(shader);
                m.EnableKeyword("_NORMALMAP");
                AssetDatabase.CreateAsset(m, bumpPath);
                made = true;
            }

            string glowPath = MaterialFolder + "/PW_Keep_Glow.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(glowPath) == null)
            {
                var m = new Material(shader);
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", Color.white);
                AssetDatabase.CreateAsset(m, glowPath);
                made = true;
            }

            string seeThroughPath = MaterialFolder + "/PW_Keep_SeeThrough.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(seeThroughPath) == null)
            {
                var m = new Material(shader);
                m.SetFloat("_Surface", 1f);
                m.SetFloat("_Blend", 0f);
                m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetFloat("_ZWrite", 0f);
                m.SetOverrideTag("RenderType", "Transparent");
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", Color.white);
                m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                AssetDatabase.CreateAsset(m, seeThroughPath);
                made = true;
            }

            if (made) AssetDatabase.SaveAssets();
        }

        [MenuItem("Patchwork/Open Hub Scene", false, 1)]
        static void OpenHub()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(HubPath);
        }

        [MenuItem("Patchwork/Open Level 1 Scene", false, 2)]
        static void OpenLevel1()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(Level1Path);
        }

        [MenuItem("Patchwork/Play From Hub (toggle)", false, 20)]
        static void TogglePlayFromHub()
        {
            bool now = !EditorPrefs.GetBool(StartFromHubKey, true);
            EditorPrefs.SetBool(StartFromHubKey, now);
            ApplyPlayModeStartScene();
            Debug.Log(now ? "Patchwork: Play now always starts from the Hub." : "Patchwork: Play now starts from the open scene.");
        }

        [MenuItem("Patchwork/Play From Hub (toggle)", true)]
        static bool TogglePlayFromHubValidate()
        {
            Menu.SetChecked("Patchwork/Play From Hub (toggle)", EditorPrefs.GetBool(StartFromHubKey, true));
            return true;
        }

        [MenuItem("Patchwork/Reset Save Data", false, 40)]
        static void ResetSave()
        {
            if (EditorUtility.DisplayDialog("Reset Patchwork save?",
                "This clears golden buttons, level completion, current form and hat.", "Reset", "Cancel"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("Patchwork: save data cleared.");
            }
        }
    }
}

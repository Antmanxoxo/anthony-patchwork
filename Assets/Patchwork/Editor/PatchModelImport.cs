using UnityEditor;
using UnityEngine;

namespace Patchwork.EditorTools
{
    // Import settings for Patch's Blender model and its baked textures
    // (Assets/Patchwork/Resources/Characters): generic rig, no auto materials,
    // looping clips marked as loops, and the normal map imported as a normal map.
    public class PatchModelImport : AssetPostprocessor
    {
        public const string Folder = "Assets/Patchwork/Resources/Characters";
        static readonly string[] LoopingClips = { "Idle", "Walk", "Run", "Fall", "Swing", "Celebrate" };

        bool InFolder { get { return assetPath.Replace('\\', '/').StartsWith(Folder + "/"); } }

        void OnPreprocessModel()
        {
            if (!InFolder) return;
            var mi = (ModelImporter)assetImporter;
            mi.animationType = ModelImporterAnimationType.Generic;
            mi.importAnimation = true;
            mi.materialImportMode = ModelImporterMaterialImportMode.None;
            mi.globalScale = 1f;
            mi.useFileScale = true;
            mi.importCameras = false;
            mi.importLights = false;
            mi.importBlendShapes = false;
            mi.importTangents = ModelImporterTangents.CalculateMikk;
        }

        void OnPreprocessAnimation()
        {
            if (!InFolder) return;
            var mi = (ModelImporter)assetImporter;
            var clips = mi.defaultClipAnimations;
            foreach (var c in clips)
            {
                c.loopTime = false;
                foreach (var name in LoopingClips)
                    if (c.name == name || c.name.EndsWith("|" + name)) c.loopTime = true;
            }
            mi.clipAnimations = clips;
        }

        void OnPreprocessTexture()
        {
            if (!InFolder) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = assetPath.EndsWith("_Normal.png") ? TextureImporterType.NormalMap : TextureImporterType.Default;
            ti.textureShape = TextureImporterShape.Texture2D;
            ti.maxTextureSize = 2048;
            ti.mipmapEnabled = true;
        }

        // The model may have been imported before this script existed; re-import it once if so.
        public static void EnsureImported()
        {
            string fbx = Folder + "/Patch.fbx";
            var mi = AssetImporter.GetAtPath(fbx) as ModelImporter;
            if (mi != null && (mi.animationType != ModelImporterAnimationType.Generic ||
                               mi.materialImportMode != ModelImporterMaterialImportMode.None ||
                               mi.clipAnimations.Length == 0))
                AssetDatabase.ImportAsset(fbx, ImportAssetOptions.ForceUpdate);

            foreach (var name in new[] { "Patch_Albedo", "Patch_Normal" })
            {
                string path = Folder + "/" + name + ".png";
                var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti == null) continue;
                bool wantNormal = name.EndsWith("_Normal");
                if (ti.textureShape != TextureImporterShape.Texture2D ||
                    (ti.textureType == TextureImporterType.NormalMap) != wantNormal)
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}

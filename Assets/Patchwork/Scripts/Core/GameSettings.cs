using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Patchwork
{
    // Player options from the Settings page (saved in PlayerPrefs, so they survive restarts
    // and are separate from game progress).
    public static class GameSettings
    {
        const string CameraSpeedKey = "pw_cam_speed";
        const string InvertYKey = "pw_cam_invert_y";
        const string InvertXKey = "pw_cam_invert_x";
        const string SwingToggleKey = "pw_swing_toggle";
        const string VSyncKey = "pw_vsync";
        const string LookKey = "pw_look";

        public static readonly string[] LookNames = { "Plain", "Storybook", "Stitched" };

        // The screen look: 0 plain, 1 Storybook (soft ink outlines), 2 Stitched (dashed seams).
        public static int Look
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(LookKey, 1), 0, 2); }
            set { PlayerPrefs.SetInt(LookKey, Mathf.Clamp(value, 0, 2)); }
        }

        public static void ApplyLook()
        {
            Shader.SetGlobalFloat("_PatchworkLook", Look);
        }

        // 1..10, 5 is the normal speed.
        public static int CameraSpeed
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(CameraSpeedKey, 5), 1, 10); }
            set { PlayerPrefs.SetInt(CameraSpeedKey, Mathf.Clamp(value, 1, 10)); }
        }

        // 1 -> about half speed, 5 -> normal, 10 -> about one and a half times.
        public static float CameraSpeedMultiplier { get { return 0.4f + CameraSpeed * 0.12f; } }

        public static bool InvertY
        {
            get { return PlayerPrefs.GetInt(InvertYKey, 0) == 1; }
            set { PlayerPrefs.SetInt(InvertYKey, value ? 1 : 0); }
        }

        public static bool InvertX
        {
            get { return PlayerPrefs.GetInt(InvertXKey, 0) == 1; }
            set { PlayerPrefs.SetInt(InvertXKey, value ? 1 : 0); }
        }

        // Yarn swing: false = hold Ability to stay on, true = tap to grab and tap again to let go.
        public static bool SwingToggle
        {
            get { return PlayerPrefs.GetInt(SwingToggleKey, 0) == 1; }
            set { PlayerPrefs.SetInt(SwingToggleKey, value ? 1 : 0); }
        }

        public static bool VSync
        {
            get { return PlayerPrefs.GetInt(VSyncKey, 1) == 1; }
            set { PlayerPrefs.SetInt(VSyncKey, value ? 1 : 0); }
        }

        public static void ApplyVSync()
        {
            QualitySettings.vSyncCount = VSync ? 1 : 0;
        }

        // ------------------------------------------------------------------ Graphics

        const string GraphicsKey = "pw_graphics";
        public static readonly string[] GraphicsNames = { "Fast", "Balanced", "Best" };

        // 0 Fast, 1 Balanced, 2 Best. The first time the game runs it picks one to suit the
        // computer's graphics card (and the player can change it in Settings).
        public static int Graphics
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(GraphicsKey, AutoGraphics()), 0, 2); }
            set { PlayerPrefs.SetInt(GraphicsKey, Mathf.Clamp(value, 0, 2)); }
        }

        public static int AutoGraphics()
        {
            int vram = SystemInfo.graphicsMemorySize;
            string gpu = (SystemInfo.graphicsDeviceName ?? "").ToLowerInvariant();
            bool integrated = (gpu.Contains("intel") && !gpu.Contains("arc")) || gpu.Contains("uhd") || gpu.Contains("iris") || (gpu.Contains("radeon") && gpu.Contains("graphics") && vram < 4000);
            if (integrated || vram < 2500) return 0;
            if (vram < 6000 || SystemInfo.processorCount < 6) return 1;
            return 2;
        }

        static UniversalRenderPipelineAsset runtimePipeline;

        // Applies the graphics level: resolution scale (with FSR upscaling below full), anti-
        // aliasing, shadow distance and detail, and how far away small decorations are drawn.
        // In a built game the pipeline settings are changed on a copy made at startup, so the
        // project's own settings asset is never touched; in the editor only the draw distance
        // changes.
        public static void ApplyGraphics()
        {
            int g = Graphics;
            RenderOptimizer.CullScale = g == 0 ? 0.6f : g == 1 ? 0.8f : 1f;
            RenderOptimizer.ApplyCullDistances();
#if !UNITY_EDITOR
            if (runtimePipeline == null)
            {
                var source = (QualitySettings.renderPipeline ?? GraphicsSettings.defaultRenderPipeline) as UniversalRenderPipelineAsset;
                if (source == null) return;
                runtimePipeline = Object.Instantiate(source);
                runtimePipeline.name = source.name + " (Patchwork)";
                QualitySettings.renderPipeline = runtimePipeline;
            }
            var a = runtimePipeline;
            a.renderScale = g == 0 ? 0.75f : g == 1 ? 0.9f : 1f;
            a.upscalingFilter = g == 2 ? UpscalingFilterSelection.Auto : UpscalingFilterSelection.FSR;
            a.msaaSampleCount = g == 0 ? 1 : g == 1 ? 2 : 4;
            a.shadowDistance = g == 0 ? 28f : g == 1 ? 40f : 50f;
            a.shadowCascadeCount = g == 0 ? 1 : g == 1 ? 2 : 4;
#endif
            Debug.Log("Patchwork: graphics set to " + GraphicsNames[g] + " (" + SystemInfo.graphicsDeviceName + ", " + SystemInfo.graphicsMemorySize + " MB, " + SystemInfo.graphicsDeviceType + ")");
        }
    }
}

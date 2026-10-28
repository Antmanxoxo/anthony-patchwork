using UnityEngine;

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
    }
}

using System.IO;
using UnityEngine;

namespace Patchwork
{
    // In a built game (not the editor), writes "Patchwork Log.txt" next to Patchwork.exe: which
    // build it is, the computer's graphics card and settings, and any warnings or errors while
    // playing. Easy to find and send when the game misbehaves on someone else's computer.
    // (Unity's own Player.log is still written too, in AppData/LocalLow/Little Colossus/Patchwork.)
    public static class BuildLog
    {
        static StreamWriter writer;
        static int lines;

        // The build number, stamped by the editor before each build (see PatchworkBuildChecks).
        static string buildName;
        public static string BuildName
        {
            get
            {
                if (buildName == null)
                {
                    var t = Resources.Load<TextAsset>("PatchworkBuild");
                    buildName = t != null ? t.text.Trim() : "Dev";
                    if (Application.isEditor) buildName = "Editor (last build: " + buildName + ")";
                }
                return buildName;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Start()
        {
            if (Application.isEditor) return;
            try
            {
                string folder = Path.GetDirectoryName(Application.dataPath);
                string path = Path.Combine(folder, "Patchwork Log.txt");
                writer = new StreamWriter(path, false) { AutoFlush = true };
            }
            catch
            {
                try { writer = new StreamWriter(Path.Combine(Application.persistentDataPath, "Patchwork Log.txt"), false) { AutoFlush = true }; }
                catch { writer = null; }
            }
            if (writer == null) return;
            writer.WriteLine("Patchwork " + BuildName);
            writer.WriteLine("Computer: " + SystemInfo.operatingSystem + ", " + SystemInfo.processorType + " x" + SystemInfo.processorCount + ", " + SystemInfo.systemMemorySize + " MB RAM");
            writer.WriteLine("Graphics: " + SystemInfo.graphicsDeviceName + " (" + SystemInfo.graphicsMemorySize + " MB), " + SystemInfo.graphicsDeviceType + ", " + SystemInfo.graphicsDeviceVersion + ", shader level " + SystemInfo.graphicsShaderLevel);
            writer.WriteLine("Screen: " + Screen.width + " x " + Screen.height + " @ " + Screen.currentResolution.refreshRateRatio.value.ToString("0") + " Hz, quality " + QualitySettings.names[QualitySettings.GetQualityLevel()]);
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            writer.WriteLine("URP Lit shader: " + (lit == null ? "MISSING" : lit.isSupported ? "ok" : "NOT SUPPORTED on this card"));
            writer.WriteLine();
            Application.logMessageReceived += OnLog;
        }

        static void OnLog(string message, string stack, LogType type)
        {
            if (writer == null || lines > 3000) return;
            bool ours = message.StartsWith("Patchwork");
            if (type == LogType.Log && !ours) return;
            lines++;
            try
            {
                writer.WriteLine("[" + Time.realtimeSinceStartup.ToString("0.0") + "s " + type + "] " + message);
                if (type == LogType.Exception || type == LogType.Error) writer.WriteLine(stack);
            }
            catch { }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The Storyteller's real voice. Each recorded line is an audio clip named by its ID (L1_003,
    // L2_041, ...) in Resources/Voice/Level1 or Level2, and Resources/Voice/NarratorLines.txt
    // says which text each ID is. When a line comes up on the narrator strip and there's a
    // recording of it, that plays instead of the murmur. Lines with no recording yet still get
    // the murmur, so the game is fine with any number of them.
    public static class NarratorVoice
    {
        const string VolumeKey = "pw_voice_volume";

        static Dictionary<string, string> idByText;
        static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        static AudioSource source;
        static string playingText;

        // Narrator voice volume, 0..10 (saved).
        public static int Volume
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(VolumeKey, 10), 0, 10); }
            set { PlayerPrefs.SetInt(VolumeKey, Mathf.Clamp(value, 0, 10)); if (source != null) source.volume = Loudness(); }
        }

        static float Loudness() { return Volume / 10f; }

        public static void ResetStatics()
        {
            source = null;
            playingText = null;
            clips.Clear();
        }

        // True while a recorded line is being read out.
        public static bool Speaking { get { return source != null && source.isPlaying; } }

        // Seconds of the current recording still to go (0 if none).
        public static float Remaining
        {
            get { return Speaking && source.clip != null ? Mathf.Max(0f, source.clip.length - source.time) : 0f; }
        }

        static string Key(string text)
        {
            if (text == null) return null;
            var sb = new System.Text.StringBuilder(text.Length);
            bool space = false;
            foreach (char ch in text.Trim())
            {
                char c = ch == '’' || ch == '‘' ? '\'' : ch == '“' || ch == '”' ? '"' : ch == '…' ? '.' : ch;
                if (char.IsWhiteSpace(c)) { space = true; continue; }
                if (space && sb.Length > 0) sb.Append(' ');
                space = false;
                sb.Append(c);
            }
            return sb.ToString();
        }

        static void LoadLines()
        {
            idByText = new Dictionary<string, string>();
            var asset = Resources.Load<TextAsset>("Voice/NarratorLines");
            if (asset == null) return;
            foreach (var raw in asset.text.Split('\n'))
            {
                string line = raw.TrimEnd('\r');
                if (line.Length == 0 || line[0] == '#') continue;
                int bar = line.IndexOf('|');
                if (bar <= 0) continue;
                string k = Key(line.Substring(bar + 1));
                if (!idByText.ContainsKey(k)) idByText[k] = line.Substring(0, bar).Trim();
            }
        }

        // The recording of this text, if there is one.
        public static AudioClip ClipFor(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            if (idByText == null) LoadLines();
            string id;
            if (!idByText.TryGetValue(Key(text), out id)) return null;
            AudioClip clip;
            if (clips.TryGetValue(id, out clip)) return clip;
            string folder = id.StartsWith("L2") ? "Level2" : "Level1";
            clip = Resources.Load<AudioClip>("Voice/" + folder + "/" + id);
            clips[id] = clip;
            return clip;
        }

        // Read this line out, if it has a recording (stopping whatever was being read). Returns
        // true if it's being spoken, so the murmur can stay quiet.
        public static bool Speak(string text)
        {
            if (text == playingText && Speaking) return true;
            var clip = ClipFor(text);
            if (clip == null) { Stop(); return false; }
            if (source == null)
            {
                var go = new GameObject("Narrator Voice");
                Object.DontDestroyOnLoad(go);
                source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                source.ignoreListenerPause = true;   // the story goes on while the game's paused
                source.priority = 0;
            }
            source.Stop();
            source.clip = clip;
            source.volume = Loudness();
            source.Play();
            playingText = text;
            return true;
        }

        public static void Stop()
        {
            if (source != null && source.isPlaying) source.Stop();
            playingText = null;
        }

        // Called every frame (by the narrator strip): the music ducks a little under the voice.
        public static float MusicDuck { get { return Speaking ? 0.55f : 1f; } }
    }
}

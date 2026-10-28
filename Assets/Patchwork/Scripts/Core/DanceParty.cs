using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Easter egg: mash the face buttons on the D-pad and Patch breaks into a dance with his own
    // song for the form he's in. It keeps going as long as you keep tapping, and stops a moment
    // after you stop. The game turns it into a little cutscene (camera orbit + letterbox).
    public class DanceParty
    {
        const int TapsToStart = 6;
        const float TapWindow = 2.2f;       // those taps have to land within this many seconds
        const float StopAfter = 1.6f;       // stop this long after the last tap
        const float FadeIn = 0.4f, FadeOut = 0.6f;
        const float Loudness = 0.42f;

        readonly AudioSource source;
        readonly Queue<float> taps = new Queue<float>();
        float lastTap = -10f;
        float volume;
        FormType form;
        int lastBeat = -1;

        public bool Active { get; private set; }
        public FormType Form { get { return form; } }

        // Song position in beats (for keeping the dance on the music).
        public float Beat
        {
            get
            {
                if (source == null || source.clip == null) return 0f;
                return source.time * MusicPlayer.DanceBpm(form) / 60f;
            }
        }

        public bool TimedOut { get { return Active && Time.unscaledTime - lastTap > StopAfter; } }

        public DanceParty(GameObject host)
        {
            source = host.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;
            source.ignoreListenerPause = true;
        }

        // Call on every face-button press. Returns true when enough quick taps have built up to start.
        public bool RegisterTap()
        {
            float now = Time.unscaledTime;
            lastTap = now;
            taps.Enqueue(now);
            while (taps.Count > 0 && now - taps.Peek() > TapWindow) taps.Dequeue();
            return !Active && taps.Count >= TapsToStart;
        }

        public void Start(FormType f)
        {
            form = f;
            Active = true;
            taps.Clear();
            lastBeat = -1;
            source.clip = MusicPlayer.DanceSong(f);
            source.time = 0f;
            source.volume = 0f;
            volume = 0f;
            source.Play();
        }

        public void Stop()
        {
            Active = false;
            taps.Clear();
        }

        // Returns true on the frame a new beat starts (for little rumbles and hops).
        public bool Update(float udt)
        {
            float level = Mathf.Max(MusicPlayer.Volume, Sfx.Volume) / 10f * Loudness;
            float target = Active ? level : 0f;
            volume = Mathf.MoveTowards(volume, target, udt * level / (Active ? FadeIn : FadeOut) + 0.0001f);
            source.volume = volume;
            if (!Active && volume <= 0.0001f && source.isPlaying) source.Stop();

            if (!Active) return false;
            int beat = Mathf.FloorToInt(Beat);
            bool newBeat = beat != lastBeat;
            lastBeat = beat;
            return newBeat;
        }
    }
}

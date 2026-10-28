using UnityEngine;
using UnityEngine.InputSystem;

namespace Patchwork
{
    // Controller rumble. Short pulses fade out on their own; Hold() is for continuous rumble
    // (swinging) and has to be called every frame. Runs on real time so slow-motion doesn't
    // stretch it, goes quiet while paused or when the window loses focus, and always stops on quit.
    public static class Haptics
    {
        const string StrengthKey = "pw_rumble";

        static float pulseLow, pulseHigh, pulseTime, pulseLength;
        static float holdLow, holdHigh;
        static bool silenced;
        static float previewTime;
        static bool motorsOn;

        // Rumble strength, 0..10 (saved). 0 turns it off.
        public static int Strength
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(StrengthKey, 10), 0, 10); }
            set { PlayerPrefs.SetInt(StrengthKey, Mathf.Clamp(value, 0, 10)); }
        }

        // low = big heavy motor, high = small buzzy motor (0..1), seconds = how long it takes to fade out.
        public static void Pulse(float low, float high, float seconds)
        {
            // A new pulse only replaces the current one if it's at least as strong right now.
            float k = pulseLength > 0f ? 1f - Mathf.Clamp01(pulseTime / pulseLength) : 0f;
            if (Mathf.Max(low, high) < Mathf.Max(pulseLow, pulseHigh) * k) return;
            pulseLow = low;
            pulseHigh = high;
            pulseLength = Mathf.Max(0.01f, seconds);
            pulseTime = 0f;
        }

        // A short buzz that plays even while paused (so the Rumble setting can be felt as you change it).
        public static void Preview()
        {
            Pulse(0.4f, 0.3f, 0.18f);
            previewTime = 0.18f;
        }

        // Continuous rumble for this frame only.
        public static void Hold(float low, float high)
        {
            holdLow = Mathf.Max(holdLow, low);
            holdHigh = Mathf.Max(holdHigh, high);
        }

        // Paused, in a menu, or the window lost focus.
        public static void SetSilenced(bool on)
        {
            silenced = on;
        }

        // Called once per frame with unscaled time.
        public static void Tick(float udt)
        {
            pulseTime += udt;
            float k = pulseLength > 0f ? 1f - Mathf.Clamp01(pulseTime / pulseLength) : 0f;
            float strength = Strength / 10f;
            float low = Mathf.Max(pulseLow * k, holdLow) * strength;
            float high = Mathf.Max(pulseHigh * k, holdHigh) * strength;
            holdLow = holdHigh = 0f;

            previewTime -= udt;
            if ((silenced && previewTime <= 0f) || !Application.isFocused) low = high = 0f;
            var pad = Gamepad.current;
            if (pad == null) return;
            if (low <= 0.001f && high <= 0.001f)
            {
                if (motorsOn) { pad.SetMotorSpeeds(0f, 0f); motorsOn = false; }
                return;
            }
            pad.SetMotorSpeeds(Mathf.Clamp01(low), Mathf.Clamp01(high));
            motorsOn = true;
        }

        // Stop everything right now (scene change, quitting, leaving Play mode).
        public static void StopAll()
        {
            pulseLow = pulseHigh = holdLow = holdHigh = 0f;
            pulseLength = 0f;
            motorsOn = false;
            if (Gamepad.current != null) Gamepad.current.ResetHaptics();
        }

        public static void ResetStatics()
        {
            pulseLow = pulseHigh = holdLow = holdHigh = 0f;
            pulseTime = pulseLength = 0f;
            silenced = false;
            previewTime = 0f;
            motorsOn = false;
        }
    }
}

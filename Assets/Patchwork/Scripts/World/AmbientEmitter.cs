using UnityEngine;

namespace Patchwork
{
    // A looping sound that comes from one spot: louder as you get close, silent when you're far.
    // Follows the sound effects volume and gets muffled with everything else in menus.
    public class AmbientEmitter : MonoBehaviour
    {
        AudioSource source;
        AudioLowPassFilter filter;
        float volume;
        float fade;

        public static AmbientEmitter Create(Transform parent, Vector3 position, AudioClip clip, float volume, float near, float far)
        {
            var go = new GameObject("Sound_" + (clip != null ? clip.name : "none"));
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var e = go.AddComponent<AmbientEmitter>();
            e.volume = volume;
            e.source = go.AddComponent<AudioSource>();
            e.source.clip = clip;
            e.source.loop = true;
            e.source.playOnAwake = false;
            e.source.spatialBlend = 1f;
            e.source.rolloffMode = AudioRolloffMode.Linear;
            e.source.minDistance = near;
            e.source.maxDistance = far;
            e.source.dopplerLevel = 0f;
            e.source.spread = 60f;
            e.source.volume = 0f;
            e.filter = go.AddComponent<AudioLowPassFilter>();
            e.filter.cutoffFrequency = 22000f;
            if (clip != null)
            {
                // Start each one somewhere different so copies of the same sound don't line up.
                e.source.time = Random.value * clip.length * 0.95f;
                e.source.Play();
            }
            return e;
        }

        void Update()
        {
            if (source == null) return;
            var game = PatchworkGame.Instance;
            bool leaving = game != null && game.Mode == UIMode.Loading;
            fade = Mathf.MoveTowards(fade, leaving ? 0f : 1f, Time.unscaledDeltaTime * (leaving ? 2f : 0.8f));
            source.volume = volume * fade * Sfx.Volume / 10f;
            filter.cutoffFrequency = Sfx.LoopCutoff;
        }
    }
}

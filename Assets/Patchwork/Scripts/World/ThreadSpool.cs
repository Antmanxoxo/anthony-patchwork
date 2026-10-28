using UnityEngine;

namespace Patchwork
{
    // A little floating spool of thread. Collect them along the way (counted per visit).
    public class ThreadSpool : PlayerVolume
    {
        Transform visual;
        Color color;
        float phase;

        public void Setup(Transform v, Color c)
        {
            visual = v;
            color = c;
            phase = Random.value * 10f;
        }

        void Update()
        {
            if (visual == null) return;
            phase += Time.deltaTime;
            visual.localRotation = Quaternion.Euler(0f, phase * 140f, 12f);
            visual.localPosition = new Vector3(0f, Mathf.Sin(phase * 3f) * 0.1f, 0f);
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            player.Game.CollectThread();
            Sfx.PlayPickup(SfxId.Thread, 0.5f);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position, color);
            gameObject.SetActive(false);
        }
    }
}

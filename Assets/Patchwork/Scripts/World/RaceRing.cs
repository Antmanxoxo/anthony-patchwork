using UnityEngine;

namespace Patchwork
{
    // One glowing ring in a Thread Race.
    public class RaceRing : PlayerVolume
    {
        public ThreadRace Race;
        Transform visual;

        public void Setup(Transform v) { visual = v; }

        void Update()
        {
            if (visual != null) visual.localRotation = Quaternion.Euler(90f, Time.time * 120f, 0f);
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (Race == null || ThreadRace.Running != Race) return;
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position, new Color(0.6f, 0.9f, 1f));
            gameObject.SetActive(false);
            Race.Collect(this);
        }
    }
}

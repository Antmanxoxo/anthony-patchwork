using UnityEngine;

namespace Patchwork
{
    // A bell you can ring by getting right up to it (the clock tower's belfry).
    public class BellRinger : PlayerVolume
    {
        public Transform Bell;
        float swing, cooldown, t;
        bool rungOnce;

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (cooldown > 0f) return;
            cooldown = 1.5f;
            swing = 1f;
            Sfx.Play(SfxId.BigBell, 0.8f);
            if (Bell != null) FxBurst.Spawn(FxBurst.Kind.Sparkle, Bell.position, new Color(1f, 0.85f, 0.5f));
            if (!rungOnce && Bell != null) { rungOnce = true; ScoreBubbles.BurstAt(Bell.position + Vector3.down * 2f, 10); }
        }

        void Update()
        {
            cooldown -= Time.deltaTime;
            t += Time.deltaTime;
            swing = Mathf.MoveTowards(swing, 0f, Time.deltaTime * 0.35f);
            if (Bell != null) Bell.localRotation = Quaternion.Euler(Mathf.Sin(t * 5f) * 22f * swing, 0f, 0f);
        }
    }
}

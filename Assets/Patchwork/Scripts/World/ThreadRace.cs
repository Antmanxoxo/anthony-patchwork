using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // A timed challenge: touch the stopwatch pin and a trail of glowing rings appears. Grab them
    // all before time runs out to make a Heartstring appear.
    public class ThreadRace : PlayerVolume
    {
        public static ThreadRace Running { get; private set; }
        public float TimeLimit = 16f;
        public System.Action Won;
        public float TimeLeft { get; private set; }
        public int Total { get { return rings.Count; } }
        public int Got { get; private set; }
        readonly List<RaceRing> rings = new List<RaceRing>();
        bool done;
        float cooldown;
        Transform visual;

        public static void ResetStatics() { Running = null; }

        public void Setup(Transform v) { visual = v; }

        public void AddRing(RaceRing r)
        {
            r.Race = this;
            rings.Add(r);
            r.gameObject.SetActive(false);
        }

        public void MarkDone()
        {
            done = true;
            if (visual != null) visual.gameObject.SetActive(false);
        }

        void OnDestroy() { if (Running == this) Running = null; }

        void Update()
        {
            cooldown -= Time.deltaTime;
            if (visual != null && !done) visual.localRotation = Quaternion.Euler(0f, Time.time * 50f, 0f);
            if (Running != this) return;
            TimeLeft -= Time.deltaTime;
            if (TimeLeft <= 0f)
            {
                Running = null;
                foreach (var r in rings) r.gameObject.SetActive(false);
                cooldown = 1.5f;
                Sfx.Play(SfxId.TransformFail, 0.5f);
                if (PatchworkGame.Instance != null) PatchworkGame.Instance.ShowToast("Out of time! Touch the stopwatch to try again.");
            }
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (done || Running == this || cooldown > 0f) return;
            Running = this;
            TimeLeft = TimeLimit;
            Got = 0;
            foreach (var r in rings) r.gameObject.SetActive(true);
            Sfx.Play(SfxId.MenuConfirm, 0.6f);
            player.Game.ShowToast("Thread Race! Grab every ring before time runs out!");
        }

        public void Collect(RaceRing r)
        {
            if (Running != this) return;
            Got++;
            Sfx.PlayPickup(SfxId.Thread, 0.6f);
            if (Got >= rings.Count)
            {
                Running = null;
                MarkDone();
                if (Won != null) Won();
            }
        }
    }
}

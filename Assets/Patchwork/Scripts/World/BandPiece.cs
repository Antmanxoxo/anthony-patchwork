using UnityEngine;

namespace Patchwork
{
    // Something the Toybox Town band needs back: the lute's string, oil for the trumpet's valves,
    // or the piper himself (stranded up on the weathervane). Touch it to send it home.
    public class BandPiece : PlayerVolume
    {
        public enum Part { Lute, Trumpet, Piper }

        public string Id;
        public Part Kind;
        public string Title;
        Transform visual;
        ToyNpc stranded;   // the piper waiting to be rescued
        float t;
        bool taken;

        // Locked: it's there (or hidden) but can't be picked up yet, because something has to
        // happen first (the pirate wants his treasure, the dye taps aren't running, the piper is
        // still flying away). Touching a locked one can show a line saying why.
        public bool Locked { get; private set; }
        public bool HideWhenLocked = true;
        public string LockedToast;
        public bool Taken { get { return taken; } }

        public void SetLocked(bool locked)
        {
            if (taken) return;
            bool was = Locked;
            Locked = locked;
            if (visual != null) visual.gameObject.SetActive(!(locked && HideWhenLocked));
            if (was && !locked)
            {
                FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position, new Color(1f, 0.85f, 0.4f));
                FxBurst.Spawn(FxBurst.Kind.Poof, transform.position, Color.white);
                Sfx.Play(SfxId.Twinkle, 0.5f);
            }
        }

        public void Setup(string id, Part kind, string title, Transform visualRoot, ToyNpc strandedNpc)
        {
            Id = id;
            Kind = kind;
            Title = title;
            visual = visualRoot;
            stranded = strandedNpc;
            if (GameState.IsCollected(id))
            {
                taken = true;
                if (visual != null) visual.gameObject.SetActive(false);
                if (stranded != null) stranded.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            if (taken || visual == null) return;
            t += Time.deltaTime;
            visual.localRotation = Quaternion.Euler(0f, t * 70f, 0f);
            visual.localPosition = new Vector3(0f, Mathf.Sin(t * 2f) * 0.18f, 0f);
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (taken) return;
            if (Locked)
            {
                if (!string.IsNullOrEmpty(LockedToast) && player.Game != null) player.Game.ShowToast(LockedToast);
                return;
            }
            Take(player);
        }

        // (If it's unlocked while Patch is already standing in it.)
        protected override void OnPlayerStay(PatchworkPlayer player, float dt)
        {
            if (!taken && !Locked) Take(player);
        }

        void Take(PatchworkPlayer player)
        {
            taken = true;
            GameState.SetCollected(Id);
            ScoreKeeper.Bonus(1000, transform.position);
            ScoreBubbles.BurstAt(transform.position, 15);
            GameState.Save();
            Sfx.Play(SfxId.Complete, 0.8f);
            Sfx.Voice(SfxId.VoiceYay, player.Forms.Current, 0.55f);
            Haptics.Pulse(0.6f, 0.4f, 0.3f);
            Vector3 p = transform.position;
            FxBurst.Spawn(FxBurst.Kind.Sparkle, p, new Color(1f, 0.85f, 0.4f));
            FxBurst.Spawn(FxBurst.Kind.Scraps, p, new Color(1f, 0.6f, 0.7f));
            if (visual != null) visual.gameObject.SetActive(false);
            if (stranded != null)
            {
                FxBurst.Spawn(FxBurst.Kind.Puff, stranded.transform.position + Vector3.up, Color.white);
                stranded.gameObject.SetActive(false);
            }
            if (Level1Director.Instance != null) Level1Director.Instance.OnBandPiece(this);
        }
    }
}

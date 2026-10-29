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
            taken = true;
            GameState.SetCollected(Id);
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

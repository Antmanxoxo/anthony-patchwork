using UnityEngine;

namespace Patchwork
{
    // A Heartstring: a golden, heart-shaped spool of the thread that holds a place together.
    // Mending a level means finding them. Some sit out in the open at the end of a climb; others
    // only appear once you finish a challenge (popping every knot, beating a race...).
    public class Heartstring : PlayerVolume
    {
        public string Id;
        public string Title;
        Transform visual;
        Transform ring;
        float t;
        float appear = 1f;
        bool hidden;
        bool taken;
        Light glow;

        public bool Hidden { get { return hidden; } }

        public void Setup(string id, string title, Transform visualRoot, Transform ringRoot, Light light, bool startHidden)
        {
            Id = id;
            Title = title;
            visual = visualRoot;
            ring = ringRoot;
            glow = light;
            if (GameState.IsCollected(id))
            {
                // Already mended on an earlier visit: leave a faint outline where it was.
                taken = true;
                SetGhost();
                return;
            }
            if (startHidden) { hidden = true; appear = 0f; visual.gameObject.SetActive(false); if (glow != null) glow.enabled = false; }
        }

        void SetGhost()
        {
            foreach (var r in visual.GetComponentsInChildren<Renderer>())
                r.sharedMaterial = PatchworkAssets.Plain(new Color(1f, 0.92f, 0.75f), 0.3f);
            visual.localScale = Vector3.one * 0.6f;
            if (glow != null) glow.enabled = false;
        }

        // Makes a hidden Heartstring appear (with a shower of sparkles and a chime).
        public void Reveal()
        {
            if (!hidden || taken) return;
            hidden = false;
            appear = 0f;
            visual.gameObject.SetActive(true);
            if (glow != null) glow.enabled = true;
            Sfx.Play(SfxId.Checkpoint, 0.7f);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position, new Color(1f, 0.85f, 0.4f));
            if (PatchworkGame.Instance != null) PatchworkGame.Instance.ShowToast("A Heartstring appeared!");
        }

        void Update()
        {
            if (visual == null || !visual.gameObject.activeSelf) return;
            t += Time.deltaTime;
            appear = Mathf.MoveTowards(appear, 1f, Time.deltaTime * 1.5f);
            float s = taken ? 0.6f : UIKit.EaseOutBack(appear);
            visual.localScale = Vector3.one * s;
            visual.localPosition = new Vector3(0f, Mathf.Sin(t * 1.8f) * 0.2f, 0f);
            visual.localRotation = Quaternion.Euler(0f, t * 60f, 0f);
            if (ring != null) ring.localRotation = Quaternion.Euler(70f, -t * 90f, 0f);
            if (glow != null) glow.intensity = 1.6f + Mathf.Sin(t * 3f) * 0.4f;
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (taken || hidden) return;
            taken = true;
            GameState.SetCollected(Id);
            Sfx.Play(SfxId.Complete, 0.75f);
            Sfx.Voice(SfxId.VoiceYay, player.Forms.Current, 0.55f);
            Haptics.Pulse(0.6f, 0.4f, 0.3f);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position, new Color(1f, 0.85f, 0.4f));
            FxBurst.Spawn(FxBurst.Kind.Threads, transform.position, new Color(1f, 0.75f, 0.35f));
            FxBurst.Spawn(FxBurst.Kind.Scraps, transform.position, new Color(1f, 0.55f, 0.65f));
            visual.gameObject.SetActive(false);
            if (glow != null) glow.enabled = false;
            if (Level1Director.Instance != null) Level1Director.Instance.OnHeartstring(this);
        }
    }
}

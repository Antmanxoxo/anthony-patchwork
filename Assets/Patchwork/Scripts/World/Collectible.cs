using UnityEngine;

namespace Patchwork
{
    // A golden sewing button. Optional; placed where a form ability is needed to reach it.
    // Already-collected buttons appear as pale "ghost" buttons.
    public class Collectible : PlayerVolume
    {
        public string Id;
        // Secret buttons softly chime when you're close, so you can hunt them by ear.
        public bool Hums;
        const float HumRange = 16f;
        Transform visual;
        float phase;
        float humTimer;
        bool taken;

        public void Setup(string id, Transform visualRoot)
        {
            Id = id;
            visual = visualRoot;
            phase = Random.value * 10f;
            if (GameState.IsCollected(id))
            {
                foreach (var r in visual.GetComponentsInChildren<Renderer>())
                    r.sharedMaterial = PatchworkAssets.Plain(new Color(1f, 1f, 1f, 1f), 0.2f);
            }
        }

        void Update()
        {
            if (visual == null) return;
            phase += Time.deltaTime;
            visual.localRotation = Quaternion.Euler(0f, phase * 90f, 0f);
            visual.localPosition = new Vector3(0f, Mathf.Sin(phase * 2.2f) * 0.15f, 0f);

            if (Hums && !taken && !GameState.IsCollected(Id) && PatchworkGame.Instance != null && PatchworkGame.Instance.Player != null)
            {
                humTimer -= Time.deltaTime;
                float d = Vector3.Distance(PatchworkGame.Instance.Player.Motor.Center, transform.position);
                if (d < HumRange && humTimer <= 0f)
                {
                    // Closer = louder and quicker.
                    float k = 1f - d / HumRange;
                    Sfx.PlayPitched(SfxId.HintNeutral, 0.08f + 0.3f * k, 1.6f);
                    humTimer = Mathf.Lerp(1.6f, 0.7f, k);
                }
            }
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (taken) return;
            taken = true;
            bool isNew = !GameState.IsCollected(Id);
            GameState.SetCollected(Id);
            Sfx.PlayPickup(SfxId.Collect, 0.8f);
            if (isNew) Sfx.Voice(SfxId.VoiceYay, player.Forms.Current, 0.5f);
            FxBurst.Spawn(FxBurst.Kind.Scraps, transform.position, new Color(1f, 0.82f, 0.25f));
            player.Game.ShowToast(isNew ? "Golden button found!" : "Button already in your satchel.");
            gameObject.SetActive(false);
        }
    }
}

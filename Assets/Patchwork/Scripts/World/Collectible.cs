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
            FxBurst.Spawn(FxBurst.Kind.Scraps, transform.position, new Color(1f, 0.82f, 0.25f));
            var game = player.Game;
            if (!isNew)
            {
                game.ShowToast("Button already in your satchel.");
                gameObject.SetActive(false);
                return;
            }
            // A proper moment for a proper find: everything holds still for a heartbeat, the
            // button leaps up over Patch's head spinning and shining, then bursts, and a card
            // says how many of the level's buttons that makes.
            Sfx.Voice(SfxId.VoiceYay, player.Forms.Current, 0.5f);
            Sfx.Play(SfxId.PrizeFanfare, 0.55f);
            game.HitStop(0.12f);
            if (game.CameraRig != null) game.CameraRig.Kick(0.15f);
            Haptics.Pulse(0.4f, 0.6f, 0.3f);
            ScoreKeeper.Bonus(1000, transform.position + Vector3.up);
            int found = game.Level.CollectedCount(), total = game.Level.CollectibleIds.Count;
            PatchworkHUD.ShowGet("GOLDEN BUTTON!", found >= total ? "That's every golden button in " + game.Level.Title + "!" : found + " of " + total + " found in " + game.Level.Title,
                new Color(1f, 0.78f, 0.25f), 0);
            if (visual != null)
            {
                visual.SetParent(null, true);
                var fly = visual.gameObject.AddComponent<PrizeLeap>();
                fly.Over = player.transform;
            }
            gameObject.SetActive(false);
        }
    }

    // Something just collected leaping up over Patch's head: up, a spin and a grow, a hold,
    // then a burst of sparkles and it's gone.
    public class PrizeLeap : MonoBehaviour
    {
        public Transform Over;
        Vector3 from, scale;
        float t;

        void Start()
        {
            from = transform.position;
            scale = transform.localScale;
        }

        void Update()
        {
            t += Time.unscaledDeltaTime;
            Vector3 top = Over != null ? Over.position + Vector3.up * 2.6f : from + Vector3.up * 2f;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.35f));
            transform.position = Vector3.Lerp(from, top, k) + Vector3.up * Mathf.Sin(t * 4f) * 0.08f;
            transform.rotation = Quaternion.Euler(0f, t * 540f * Mathf.Max(0.25f, 1f - t), 0f);
            transform.localScale = scale * (1f + UIKit.EaseOutBack(Mathf.Clamp01(t / 0.35f)) * 0.5f);
            if (t < 1.2f) return;
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position, new Color(1f, 0.85f, 0.4f));
            FxBurst.Spawn(FxBurst.Kind.Pompoms, transform.position, new Color(1f, 0.8f, 0.3f));
            Sfx.Play(SfxId.Pop, 0.4f);
            Destroy(gameObject);
        }
    }
}

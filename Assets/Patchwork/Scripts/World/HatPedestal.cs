using UnityEngine;

namespace Patchwork
{
    // A hat floating over a pedestal. Walking into it unlocks the hat and puts it on.
    public class HatPedestal : PlayerVolume
    {
        public int Hat;
        public string HatName;
        Transform visual;
        float t;

        public void Setup(int hat, string hatName, Transform visualRoot)
        {
            Hat = hat;
            HatName = hatName;
            visual = visualRoot;
            if (GameState.IsHatUnlocked(hat) && visual != null) visual.gameObject.SetActive(false);
        }

        void Update()
        {
            if (visual == null || !visual.gameObject.activeSelf) return;
            t += Time.deltaTime;
            visual.localRotation = Quaternion.Euler(0f, t * 40f, 0f);
            visual.localPosition = new Vector3(0f, Mathf.Sin(t * 2f) * 0.12f, 0f);
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (visual == null || !visual.gameObject.activeSelf) return;
            GameState.UnlockHat(Hat);
            GameState.HatIndex = Hat;
            GameState.Save();
            player.Visuals.SetHat(Hat);
            player.Visuals.PunchScale(new Vector3(0.7f, 1.35f, 0.7f));
            Sfx.Play(SfxId.Complete, 0.7f);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position, new Color(1f, 0.85f, 0.3f));
            FxBurst.Spawn(FxBurst.Kind.Scraps, transform.position, new Color(1f, 0.8f, 0.3f));
            player.Game.ShowToast("New hat: " + HatName + "!");
            visual.gameObject.SetActive(false);
        }
    }
}

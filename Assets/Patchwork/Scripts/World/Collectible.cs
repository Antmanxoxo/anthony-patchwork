using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// A golden sewing button. Optional; placed where a form ability is needed to reach it.
    /// Already-collected buttons appear as pale "ghost" buttons.
    /// </summary>
    public class Collectible : PlayerVolume
    {
        public string Id;
        Transform visual;
        float phase;
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
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (taken) return;
            taken = true;
            bool isNew = !GameState.IsCollected(Id);
            GameState.SetCollected(Id);
            Sfx.Play(SfxId.Collect, 0.8f);
            TransformBurst.Spawn(transform.position, new Color(1f, 0.82f, 0.25f), Color.white, 14, 0.8f);
            player.Game.ShowToast(isNew ? "Golden button found!" : "Button already in your satchel.");
            gameObject.SetActive(false);
        }
    }
}

using UnityEngine;

namespace Patchwork
{
    /// <summary>A stitched doorway that loads another scene.</summary>
    public class Portal : PlayerVolume
    {
        public string TargetScene;
        public Transform Swirl;

        void Update()
        {
            if (Swirl != null) Swirl.Rotate(0f, 90f * Time.deltaTime, 0f, Space.Self);
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            player.Game.LoadScene(TargetScene);
        }
    }
}

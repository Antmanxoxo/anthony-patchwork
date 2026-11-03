using UnityEngine;

namespace Patchwork
{
    // A squeaky toy (rubber ducks): touch it and it squeaks and squishes.
    public class SqueakToy : PlayerVolume
    {
        public Transform Body;
        public float Pitch = 1f;
        float squish;
        bool squeaked;
        Vector3 baseScale;

        void Start() { if (Body != null) baseScale = Body.localScale; }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            squish = 1f;
            Sfx.PlayPitched(SfxId.Squeak, 0.5f, Pitch * Random.Range(0.95f, 1.08f));
            if (!squeaked) { squeaked = true; ScoreBubbles.BurstAt(transform.position + Vector3.up, 2); }
        }

        void Update()
        {
            if (Body == null) return;
            squish = Mathf.MoveTowards(squish, 0f, Time.deltaTime * 3f);
            float w = Mathf.Sin(squish * 18f) * squish * 0.25f;
            Body.localScale = new Vector3(baseScale.x * (1f + w), baseScale.y * (1f - w), baseScale.z * (1f + w));
        }
    }
}

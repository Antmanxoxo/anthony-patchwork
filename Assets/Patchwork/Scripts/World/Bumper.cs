using UnityEngine;

namespace Patchwork
{
    // For moving toys (the train, marching soldiers): a solid box you can't walk through, and if
    // it runs into Patch it gives him a friendly shove out of the way instead of overlapping him.
    public class Bumper : MonoBehaviour
    {
        public Vector3 HalfSize = new Vector3(0.8f, 0.8f, 1.6f);
        public Vector3 Offset = new Vector3(0f, 0.8f, 0f);
        float cooldown;

        public static Bumper Add(Transform t, Vector3 size, Vector3 centre)
        {
            var b = t.gameObject.AddComponent<Bumper>();
            b.HalfSize = size * 0.5f;
            b.Offset = centre;
            var col = t.gameObject.AddComponent<BoxCollider>();
            col.size = size;
            col.center = centre;
            return b;
        }

        void Update()
        {
            cooldown -= Time.deltaTime;
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null || cooldown > 0f) return;
            var player = game.Player;
            Vector3 local = transform.InverseTransformPoint(player.Motor.Center) - Offset;
            const float pad = 0.5f;
            if (Mathf.Abs(local.x) < HalfSize.x + pad && Mathf.Abs(local.y) < HalfSize.y + pad && Mathf.Abs(local.z) < HalfSize.z + pad)
            {
                // Standing on top is fine; only shove from the sides.
                if (local.y > HalfSize.y) return;
                cooldown = 0.6f;
                SnarlKnot.Bump(player, transform.TransformPoint(Offset));
            }
        }
    }
}

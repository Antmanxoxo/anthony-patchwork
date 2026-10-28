using UnityEngine;

namespace Patchwork
{
    // Clouds sliding past the Skiff from bow to stern, so the ship feels like it's sailing.
    // When one drifts too far behind, it wraps back round in front.
    public class CloudDrift : MonoBehaviour
    {
        public float Speed = 4f;
        public float BackZ = -140f;
        public float FrontZ = 170f;

        void Update()
        {
            Vector3 p = transform.position;
            p.z -= Speed * Time.deltaTime;
            if (p.z < BackZ) p.z = FrontZ;
            transform.position = p;
        }
    }
}

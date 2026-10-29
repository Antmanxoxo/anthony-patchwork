using UnityEngine;

namespace Patchwork
{
    // Frees a mesh made at runtime when its object goes away (so restarting a level doesn't leak).
    public class OwnedMesh : MonoBehaviour
    {
        public Mesh Mesh;
        void OnDestroy() { if (Mesh != null) Destroy(Mesh); }
    }
}

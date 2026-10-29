using UnityEngine;
using UnityEngine.Rendering;

namespace Patchwork
{
    // Frees a look profile made at runtime when the level unloads.
    public class OwnedProfile : MonoBehaviour
    {
        public VolumeProfile Profile;
        void OnDestroy() { if (Profile != null) Destroy(Profile); }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // A blue drain grate. Solid for every form except Water.
    public class WaterGrate : MonoBehaviour
    {
        public static readonly List<WaterGrate> All = new List<WaterGrate>();
        public Collider Collider;

        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }

        public static bool IsGrate(Collider c)
        {
            for (int i = 0; i < All.Count; i++)
                if (All[i] != null && All[i].Collider == c) return true;
            return false;
        }
    }
}

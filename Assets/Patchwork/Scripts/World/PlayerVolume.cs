using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// An axis-aligned box that reacts when the player's centre is inside it. Checked manually each
    /// frame (no physics triggers), which is simple and completely predictable.
    /// </summary>
    public abstract class PlayerVolume : MonoBehaviour
    {
        public static readonly List<PlayerVolume> All = new List<PlayerVolume>();
        static readonly List<PlayerVolume> scratch = new List<PlayerVolume>();

        public Vector3 Size = Vector3.one * 2f;
        bool inside;

        protected virtual void OnEnable() { All.Add(this); }
        protected virtual void OnDisable()
        {
            All.Remove(this);
            inside = false;
        }

        public bool Contains(Vector3 point)
        {
            return new Bounds(transform.position, Size).Contains(point);
        }

        public static void UpdateAll(PatchworkPlayer player, float dt)
        {
            Vector3 c = player.Motor.Center;
            scratch.Clear();
            scratch.AddRange(All);
            for (int i = 0; i < scratch.Count; i++)
            {
                var v = scratch[i];
                if (v == null || !v.isActiveAndEnabled) continue;
                bool now = v.Contains(c);
                if (now && !v.inside) { v.inside = true; v.OnPlayerEnter(player); }
                if (now && v.isActiveAndEnabled) v.OnPlayerStay(player, dt);
                if (!now && v.inside) { v.inside = false; v.OnPlayerExit(player); }
            }
        }

        protected virtual void OnPlayerEnter(PatchworkPlayer player) { }
        protected virtual void OnPlayerStay(PatchworkPlayer player, float dt) { }
        protected virtual void OnPlayerExit(PatchworkPlayer player) { }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.35f);
            Gizmos.DrawWireCube(transform.position, Size);
        }
    }
}

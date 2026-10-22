using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// A big felt button that glides back and forth between two points, pausing at each end.
    /// Moved by the game before the player each frame; the player is carried by <see cref="Delta"/>.
    /// </summary>
    public class MovingPlatform : MonoBehaviour
    {
        public static readonly List<MovingPlatform> All = new List<MovingPlatform>();

        public Vector3 PointA;
        public Vector3 PointB;
        public float TravelTime = 3f;
        public float PauseTime = 0.7f;
        public float Phase;            // 0..1 offset so several platforms can alternate

        /// <summary>How far the platform moved this frame.</summary>
        public Vector3 Delta { get; private set; }

        float clock;

        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }

        /// <summary>Call after setting the points/timing; places the platform at its starting phase.</summary>
        public void Init()
        {
            clock = Phase * Cycle;
            transform.position = Evaluate(clock);
            Delta = Vector3.zero;
        }

        float Cycle { get { return 2f * (TravelTime + PauseTime); } }

        Vector3 Evaluate(float time)
        {
            float t = Mathf.Repeat(time, Cycle);
            float k;
            if (t < PauseTime) k = 0f;
            else if (t < PauseTime + TravelTime) k = (t - PauseTime) / TravelTime;
            else if (t < 2f * PauseTime + TravelTime) k = 1f;
            else k = 1f - (t - 2f * PauseTime - TravelTime) / TravelTime;
            k = k * k * (3f - 2f * k); // ease in/out
            return Vector3.Lerp(PointA, PointB, k);
        }

        public static void TickAll(float dt)
        {
            for (int i = 0; i < All.Count; i++)
            {
                var p = All[i];
                if (p == null) continue;
                Vector3 before = p.transform.position;
                p.clock += dt;
                Vector3 after = p.Evaluate(p.clock);
                p.transform.position = after;
                p.Delta = after - before;
            }
        }
    }
}

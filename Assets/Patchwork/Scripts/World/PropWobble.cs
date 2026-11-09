using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Makes the little things in a level notice Patch. Flowers, mushrooms, pots, signs and
    // lantern posts are soft toys on springs: they lean away as he brushes past, then wobble
    // back, and when he lands close by they all jump and jiggle. One of these looks after every
    // prop in the level (only the ones near Patch are touched each frame).
    public class PropWobble : MonoBehaviour
    {
        const float Near = 12f, Stiffness = 70f, Damping = 5.5f, MaxTilt = 24f;

        class Prop
        {
            public Transform T;
            public Quaternion Rest;
            public Vector3 RestScale;
            public float Reach;        // how close Patch has to be to brush it
            public Vector2 Tilt, Vel;  // lean in degrees (x, z) and how fast it's changing
            public float Hop, HopVel;
            public bool Moving;
        }

        readonly List<Prop> props = new List<Prop>();
        float airTime;

        // Looks after every direct child of the level whose name is in the list.
        public static PropWobble Attach(Transform root, params string[] names)
        {
            var w = root.gameObject.AddComponent<PropWobble>();
            var set = new HashSet<string>(names);
            for (int i = 0; i < root.childCount; i++)
            {
                var t = root.GetChild(i);
                if (!t.gameObject.activeSelf || !set.Contains(t.name)) continue;
                // Only soft, plain things: nothing with a job to do.
                if (t.GetComponentInChildren<PlayerVolume>() != null || t.GetComponentInChildren<Rigidbody>() != null) continue;
                bool any = false;
                var b = new Bounds();
                foreach (var r in t.GetComponentsInChildren<MeshRenderer>())
                {
                    if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
                }
                if (!any || b.size.y > 5f) continue;
                w.props.Add(new Prop { T = t, Rest = t.localRotation, RestScale = t.localScale, Reach = Mathf.Max(b.size.x, b.size.z) * 0.5f + 0.75f });
            }
            return w;
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null || game.Mode != UIMode.Gameplay) return;
            var motor = game.Player.Motor;
            float dt = Mathf.Min(Time.deltaTime, 0.033f);
            if (dt <= 0f) return;
            Vector3 p = game.Player.transform.position;
            Vector3 v = motor.Velocity;
            float speed = new Vector2(v.x, v.z).magnitude;
            bool thump = motor.JustLanded && airTime > 0.35f;
            airTime = motor.Grounded ? 0f : airTime + dt;

            for (int i = 0; i < props.Count; i++)
            {
                var pr = props[i];
                if (pr.T == null) continue;
                Vector3 d = pr.T.position - p;
                float flat = d.x * d.x + d.z * d.z;
                if (flat > Near * Near && !pr.Moving) continue;
                float dist = Mathf.Sqrt(flat);
                Vector2 away = dist > 0.01f ? new Vector2(d.x, d.z) / dist : Vector2.up;

                // Brushing past: pushed over, harder the faster he's going.
                Vector2 target = Vector2.zero;
                if (dist < pr.Reach && Mathf.Abs(d.y) < 2.2f)
                    target = away * (1f - dist / pr.Reach) * MaxTilt * Mathf.Lerp(0.45f, 1f, Mathf.Clamp01(speed / 6f));
                // A landing close by: a kick outwards and a hop.
                if (thump && dist < 4.5f && Mathf.Abs(d.y) < 2.5f)
                {
                    float k = 1f - dist / 4.5f;
                    pr.Vel += away * 170f * k;
                    pr.HopVel += 2.6f * k;
                }

                pr.Vel += ((target - pr.Tilt) * Stiffness - pr.Vel * Damping) * dt;
                pr.Tilt += pr.Vel * dt;
                pr.HopVel += (-pr.Hop * 160f - pr.HopVel * 9f) * dt;
                pr.Hop += pr.HopVel * dt;

                bool moving = pr.Tilt.sqrMagnitude > 0.02f || pr.Vel.sqrMagnitude > 0.5f || Mathf.Abs(pr.Hop) > 0.002f || Mathf.Abs(pr.HopVel) > 0.02f;
                if (!moving && !pr.Moving) continue;
                pr.Moving = moving;
                if (!moving)
                {
                    pr.Tilt = pr.Vel = Vector2.zero;
                    pr.Hop = pr.HopVel = 0f;
                }
                // Leaning away along (x, z) means turning about the axis across it.
                Vector2 tilt = Vector2.ClampMagnitude(pr.Tilt, MaxTilt * 1.4f);
                pr.T.localRotation = Quaternion.Euler(tilt.y, 0f, -tilt.x) * pr.Rest;
                float s = 1f + pr.Hop;
                pr.T.localScale = new Vector3(pr.RestScale.x / Mathf.Sqrt(Mathf.Max(0.5f, s)), pr.RestScale.y * s, pr.RestScale.z / Mathf.Sqrt(Mathf.Max(0.5f, s)));
            }
        }
    }
}

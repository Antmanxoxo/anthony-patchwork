using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The quilted globe on the Skiff. It turns slowly on its own; while you're choosing a level it
    // swings round to show that level's pin to the camera.
    public class LevelGlobe : MonoBehaviour
    {
        public Transform Spin;     // the part that turns (globe + pins)
        public float Radius = 2.4f;
        readonly List<Transform> pins = new List<Transform>();
        readonly List<Vector3> pinScales = new List<Vector3>();
        float yaw;
        float yawVelocity;
        int focus = -1;
        Vector3 focusFrom;
        float pulse;

        public void AddPin(Transform pin)
        {
            pins.Add(pin);
            pinScales.Add(pin.localScale);
        }

        // Direction from the globe's centre for a longitude / latitude (degrees), before turning.
        public static Vector3 Direction(float longitude, float latitude)
        {
            float lo = longitude * Mathf.Deg2Rad, la = latitude * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(lo) * Mathf.Cos(la), Mathf.Sin(la), Mathf.Cos(lo) * Mathf.Cos(la));
        }

        // Turn so the pin at "index" faces "viewer". Pass -1 to go back to slowly spinning.
        public void Focus(int index, Vector3 viewer)
        {
            focus = index;
            focusFrom = viewer;
            pulse = 0f;
        }

        public Vector3 PinWorldPosition(int index)
        {
            if (index < 0 || index >= pins.Count || pins[index] == null) return transform.position;
            return pins[index].position;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (focus >= 0 && focus < LevelCatalog.Levels.Length)
            {
                var e = LevelCatalog.Levels[focus];
                Vector3 to = focusFrom - transform.position;
                float viewAngle = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
                float target = viewAngle - e.Longitude;
                yaw = Mathf.SmoothDampAngle(yaw, target, ref yawVelocity, 0.35f, 720f, dt);
                pulse += dt;
            }
            else
            {
                yawVelocity = 0f;
                yaw += 9f * dt;
            }
            if (Spin != null) Spin.localRotation = Quaternion.Euler(0f, yaw, 0f);

            // The focused pin bounces gently.
            for (int i = 0; i < pins.Count; i++)
            {
                if (pins[i] == null) continue;
                float s = i == focus ? 1f + 0.18f * Mathf.Abs(Mathf.Sin(pulse * 4f)) : 1f;
                pins[i].localScale = pinScales[i] * s;
            }
        }
    }
}

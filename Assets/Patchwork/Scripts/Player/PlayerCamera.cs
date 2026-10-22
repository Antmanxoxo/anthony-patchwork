using UnityEngine;

namespace Patchwork
{
    // Third-person orbit camera. Right stick / mouse orbits; it gently follows the player and
    // pulls in when something is between it and the player.
    public class PlayerCamera : MonoBehaviour
    {
        public Transform Target;
        public float Distance = 7.5f;
        public float TargetHeight = 1.3f;
        public float StickSpeed = 170f;   // degrees per second at full tilt
        public float MouseSpeed = 0.12f;  // degrees per pixel
        public float MinPitch = -25f;
        public float MaxPitch = 65f;

        public Camera Cam { get; private set; }

        float yaw;
        float pitch = 18f;
        Vector3 focus;
        float currentDistance;
        int obstacleMask;

        public float Yaw { get { return yaw; } }

        void Awake()
        {
            Cam = GetComponent<Camera>();
            currentDistance = Distance;
            // Ignore the player (Ignore Raycast layer) and UI.
            obstacleMask = ~((1 << 2) | (1 << 5));
        }

        public void SnapBehind(float targetYaw)
        {
            yaw = targetYaw;
            pitch = 18f;
            if (Target != null) focus = Target.position + Vector3.up * TargetHeight;
            currentDistance = Distance;
            Apply(1f);
        }

        public void Tick(Vector2 stick, Vector2 mouse, float dt)
        {
            if (Target == null) return;

            yaw += stick.x * StickSpeed * dt + mouse.x * MouseSpeed;
            pitch -= stick.y * StickSpeed * 0.7f * dt + mouse.y * MouseSpeed;
            pitch = Mathf.Clamp(pitch, MinPitch, MaxPitch);

            Vector3 desiredFocus = Target.position + Vector3.up * TargetHeight;
            // Follow horizontally tightly, vertically a little softer (less bouncy when jumping).
            float fh = 1f - Mathf.Exp(-14f * dt);
            float fv = 1f - Mathf.Exp(-6f * dt);
            focus = new Vector3(Mathf.Lerp(focus.x, desiredFocus.x, fh),
                                Mathf.Lerp(focus.y, desiredFocus.y, fv),
                                Mathf.Lerp(focus.z, desiredFocus.z, fh));
            Apply(dt);
        }

        void Apply(float dt)
        {
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 back = rot * Vector3.back;

            float wanted = Distance;
            RaycastHit hit;
            if (Physics.SphereCast(focus, 0.3f, back, out hit, Distance, obstacleMask, QueryTriggerInteraction.Ignore))
                wanted = Mathf.Max(1.2f, hit.distance - 0.1f);

            // Pull in instantly, ease back out.
            if (wanted < currentDistance) currentDistance = wanted;
            else currentDistance = Mathf.Lerp(currentDistance, wanted, 1f - Mathf.Exp(-4f * dt));

            transform.position = focus + back * currentDistance;
            transform.rotation = rot;
        }

        // Converts stick input into a world direction relative to the camera.
        public Vector3 ToWorld(Vector2 input)
        {
            Quaternion flat = Quaternion.Euler(0f, yaw, 0f);
            Vector3 v = flat * new Vector3(input.x, 0f, input.y);
            return Vector3.ClampMagnitude(v, 1f);
        }
    }
}

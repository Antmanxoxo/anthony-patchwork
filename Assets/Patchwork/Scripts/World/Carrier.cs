using UnityEngine;

namespace Patchwork
{
    // Put on anything that moves and can be stood on (the toy train). It remembers how far it
    // moved and turned last frame, and Patch is carried along by the same amount.
    [DefaultExecutionOrder(200)]
    public class Carrier : MonoBehaviour
    {
        Vector3 lastPos, prevPos;
        float lastYaw, prevYaw;
        bool started;
        public float DeltaYaw { get; private set; }
        public bool HasDelta { get; private set; }

        void OnEnable()
        {
            started = false;
            HasDelta = false;
        }

        void LateUpdate()
        {
            Vector3 p = transform.position;
            float yaw = transform.eulerAngles.y;
            if (started)
            {
                prevPos = lastPos;
                prevYaw = lastYaw;
                DeltaYaw = Mathf.DeltaAngle(prevYaw, yaw);
                HasDelta = true;
            }
            lastPos = p;
            lastYaw = yaw;
            started = true;
        }

        // Where a point riding on this ended up after last frame's move and turn.
        public Vector3 CarryPoint(Vector3 point)
        {
            if (!HasDelta) return point;
            Vector3 local = point - prevPos;
            return lastPos + Quaternion.Euler(0f, DeltaYaw, 0f) * local;
        }
    }
}

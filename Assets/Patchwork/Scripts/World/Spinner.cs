using UnityEngine;

namespace Patchwork
{
    // Decorative spin + bob (pinwheels, spools, floating buttons).
    public class Spinner : MonoBehaviour
    {
        public Vector3 Axis = Vector3.up;
        public float Speed = 45f;
        public float BobHeight;
        public float BobSpeed = 1.5f;
        Vector3 basePos;
        float t;

        void Start()
        {
            basePos = transform.localPosition;
            t = Random.value * 10f;
        }

        void Update()
        {
            t += Time.deltaTime;
            transform.Rotate(Axis, Speed * Time.deltaTime, Space.Self);
            if (BobHeight > 0f)
                transform.localPosition = basePos + Vector3.up * Mathf.Sin(t * BobSpeed) * BobHeight;
        }
    }
}

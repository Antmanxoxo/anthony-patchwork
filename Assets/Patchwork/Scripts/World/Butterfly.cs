using UnityEngine;

namespace Patchwork
{
    // A little felt butterfly that flutters in loops around a flower bed, flapping its wings.
    public class Butterfly : MonoBehaviour
    {
        Transform left, right;
        Vector3 home;
        float t, speed, radius;

        public void Setup(Transform leftWing, Transform rightWing, float roam)
        {
            left = leftWing;
            right = rightWing;
            home = transform.position;
            radius = roam;
            t = Random.value * 20f;
            speed = 0.5f + Random.value * 0.4f;
        }

        void Update()
        {
            t += Time.deltaTime;
            float a = t * speed;
            Vector3 p = home + new Vector3(Mathf.Sin(a) * radius, 0.6f + Mathf.Sin(a * 2.3f) * 0.45f + Mathf.Sin(t * 7f) * 0.06f, Mathf.Sin(a * 0.7f + 1.3f) * radius * 0.8f);
            Vector3 dir = p - transform.position;
            transform.position = p;
            dir.y = 0f;
            if (dir.sqrMagnitude > 1e-6f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized), Time.deltaTime * 6f);
            float flap = Mathf.Sin(t * 16f) * 55f;
            if (left != null) left.localRotation = Quaternion.Euler(0f, 0f, 20f + flap);
            if (right != null) right.localRotation = Quaternion.Euler(0f, 0f, -20f - flap);
        }
    }
}

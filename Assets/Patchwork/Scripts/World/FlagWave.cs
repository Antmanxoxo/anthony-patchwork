using UnityEngine;

namespace Patchwork
{
    // Swings a flag back and forth on its pole (rotation only, so it never comes off the pole).
    public class FlagWave : MonoBehaviour
    {
        public float Amount = 18f;
        public float Speed = 2.2f;
        Quaternion baseRot;
        float t;

        void Start()
        {
            baseRot = transform.localRotation;
            t = Random.value * 5f;
        }

        void Update()
        {
            t += Time.deltaTime;
            float a = Mathf.Sin(t * Speed) * Amount + Mathf.Sin(t * Speed * 2.7f) * Amount * 0.3f;
            transform.localRotation = baseRot * Quaternion.Euler(0f, a, 0f);
        }
    }
}

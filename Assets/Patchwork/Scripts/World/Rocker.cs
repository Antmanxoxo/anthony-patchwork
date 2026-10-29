using UnityEngine;

namespace Patchwork
{
    // Rocks something back and forth about one axis (the rocking horse, a wobbling spinning top,
    // a kite tugging on its string).
    public class Rocker : MonoBehaviour
    {
        public Vector3 Axis = Vector3.right;
        public float Amount = 10f;
        public float Speed = 1.5f;
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
            transform.localRotation = baseRot * Quaternion.AngleAxis(Mathf.Sin(t * Speed) * Amount, Axis);
        }
    }
}

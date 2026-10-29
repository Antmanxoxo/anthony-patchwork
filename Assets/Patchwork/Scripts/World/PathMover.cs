using UnityEngine;

namespace Patchwork
{
    // Moves something round a closed loop of points at a steady speed, facing the way it goes
    // (the toy train's cars, rubber ducks paddling up the canal, the balloon drifting overhead).
    public class PathMover : MonoBehaviour
    {
        public Vector3[] Points;
        public float Speed = 2f;
        public float Distance;        // how far round the loop it starts
        public float BobHeight;       // bobbing up and down (ducks)
        public float BobSpeed = 2f;
        public float Sway;            // rocking side to side, in degrees
        float[] lengths;
        float total;

        public void Setup(Vector3[] points, float speed, float startDistance)
        {
            Points = points;
            Speed = speed;
            Distance = startDistance;
            lengths = new float[points.Length];
            total = 0f;
            for (int i = 0; i < points.Length; i++)
            {
                lengths[i] = Vector3.Distance(points[i], points[(i + 1) % points.Length]);
                total += lengths[i];
            }
            Place();
        }

        void Update()
        {
            if (Points == null || total <= 0f) return;
            Distance += Speed * Time.deltaTime;
            Place();
        }

        void Place()
        {
            if (Points == null || total <= 0f) return;
            float d = Mathf.Repeat(Distance, total);
            int i = 0;
            while (i < lengths.Length - 1 && d > lengths[i]) { d -= lengths[i]; i++; }
            Vector3 a = Points[i], b = Points[(i + 1) % Points.Length];
            float k = lengths[i] > 0f ? d / lengths[i] : 0f;
            Vector3 p = Vector3.Lerp(a, b, k);
            float t = Time.time + Distance * 0.37f;
            if (BobHeight > 0f) p += Vector3.up * Mathf.Sin(t * BobSpeed) * BobHeight;
            transform.position = p;
            Vector3 dir = b - a;
            dir.y = 0f;
            if (dir.sqrMagnitude > 1e-4f)
                transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up) * Quaternion.Euler(0f, 0f, Sway > 0f ? Mathf.Sin(t * 1.3f) * Sway : 0f);
        }

        // A smooth oval of points (for loops of track and the like).
        public static Vector3[] Oval(Vector3 center, float rx, float rz, int count)
        {
            var pts = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                float a = i / (float)count * Mathf.PI * 2f;
                pts[i] = center + new Vector3(Mathf.Cos(a) * rx, 0f, Mathf.Sin(a) * rz);
            }
            return pts;
        }
    }
}

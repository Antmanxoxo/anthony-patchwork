using UnityEngine;

namespace Patchwork
{
    // A giant bubble wand that keeps blowing bubbles. They drift up slowly; land on one and it
    // pops, bouncing you up, so you can hop from bubble to bubble to somewhere high.
    public class BubbleBlower : MonoBehaviour
    {
        public float Interval = 1.25f, RiseSpeed = 1.6f, Life = 9f;
        public Vector3 Drift = new Vector3(0.12f, 0f, 0.06f);
        Transform[] bubbles;
        float[] age;
        Vector3 mouth;
        float timer;
        int next;

        public void Setup(Vector3 mouthPos, Transform[] pool)
        {
            mouth = mouthPos;
            bubbles = pool;
            age = new float[pool.Length];
            for (int i = 0; i < pool.Length; i++) { pool[i].gameObject.SetActive(false); age[i] = -1f; }
        }

        void Update()
        {
            if (bubbles == null) return;
            float dt = Time.deltaTime;
            timer -= dt;
            if (timer <= 0f)
            {
                timer = Interval;
                int i = next;
                next = (next + 1) % bubbles.Length;
                age[i] = 0f;
                bubbles[i].position = mouth;
                bubbles[i].gameObject.SetActive(true);
            }
            for (int i = 0; i < bubbles.Length; i++)
            {
                if (age[i] < 0f) continue;
                age[i] += dt;
                var b = bubbles[i];
                float a = age[i];
                b.position += (Vector3.up * RiseSpeed + Drift) * dt + new Vector3(Mathf.Sin(a * 1.7f + i) * 0.4f, 0f, Mathf.Cos(a * 1.3f + i) * 0.3f) * dt;
                float grow = Mathf.Clamp01(a / 0.6f);
                float wob = 1f + Mathf.Sin(a * 6f + i) * 0.04f;
                b.localScale = new Vector3(wob, 2f - wob, wob) * Mathf.Lerp(0.3f, 1f, grow);
                if (a > Life) Pop(i, false);
            }
        }

        public void Pop(int i, bool loud)
        {
            if (age == null || i < 0 || i >= age.Length || age[i] < 0f) return;
            age[i] = -1f;
            FxBurst.Spawn(FxBurst.Kind.Splash, bubbles[i].position, new Color(0.85f, 0.95f, 1f));
            Sfx.PlayPitched(SfxId.Pop, loud ? 0.6f : 0.25f, Random.Range(0.9f, 1.2f));
            bubbles[i].gameObject.SetActive(false);
        }

        public int IndexOf(Transform t)
        {
            for (int i = 0; i < bubbles.Length; i++) if (bubbles[i] == t) return i;
            return -1;
        }
    }
}

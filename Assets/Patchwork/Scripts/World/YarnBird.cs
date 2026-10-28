using UnityEngine;

namespace Patchwork
{
    // Little birds made of a ball of yarn with felt wings. Some circle overhead; others sit, hop and
    // peck, and flutter off if Patch runs up to them, then come back later. Each one chirps from
    // its own spot in the world, so you can hear where they are.
    public class YarnBird : MonoBehaviour
    {
        public enum Mode { Circle, Perch }
        enum State { Sit, FlyOff, Loiter, Return }

        Mode mode;
        State state;
        Vector3 home;
        float radius;
        float speed;
        float angle;
        float t;
        float stateTime;
        float chirpTimer;
        float hopTimer;
        Vector3 from, to;

        Transform body, head, wingL, wingR;
        AudioSource voice;

        const float FlushDistance = 4.5f;
        const float HearDistance = 30f;

        public void Setup(Mode m, Vector3 homePos, float circleRadius, Color color)
        {
            mode = m;
            home = homePos;
            radius = Mathf.Max(1.5f, circleRadius);
            speed = Random.Range(3.5f, 5.5f) * (Random.value < 0.5f ? -1f : 1f);
            angle = Random.value * Mathf.PI * 2f;
            t = Random.value * 10f;
            chirpTimer = Random.Range(1f, 6f);
            hopTimer = Random.Range(1f, 3f);
            state = State.Sit;
            transform.position = mode == Mode.Perch ? home : CirclePoint(0f);
            transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Build(color);

            voice = gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.spatialBlend = 1f;
            voice.rolloffMode = AudioRolloffMode.Linear;
            voice.minDistance = 2f;
            voice.maxDistance = HearDistance;
            voice.dopplerLevel = 0f;
        }

        void Build(Color color)
        {
            var yarn = PatchworkAssets.Yarn(color);
            var felt = PatchworkAssets.Felt(Color.Lerp(color, Color.white, 0.35f));
            var dark = PatchworkAssets.Plain(new Color(0.12f, 0.1f, 0.14f), 0.7f);
            var beak = PatchworkAssets.Felt(new Color(1f, 0.7f, 0.25f));

            body = new GameObject("Body").transform;
            body.SetParent(transform, false);
            body.localPosition = new Vector3(0f, 0.22f, 0f);
            Part(PrimitiveType.Sphere, body, Vector3.zero, new Vector3(0.42f, 0.36f, 0.5f), yarn, Vector3.zero);
            Part(PrimitiveType.Cube, body, new Vector3(0f, 0.08f, -0.3f), new Vector3(0.2f, 0.04f, 0.24f), felt, new Vector3(-25f, 0f, 0f));

            head = new GameObject("Head").transform;
            head.SetParent(body, false);
            head.localPosition = new Vector3(0f, 0.2f, 0.2f);
            Part(PrimitiveType.Sphere, head, Vector3.zero, Vector3.one * 0.28f, yarn, Vector3.zero);
            Part(PrimitiveType.Cube, head, new Vector3(0f, -0.02f, 0.16f), new Vector3(0.07f, 0.07f, 0.12f), beak, new Vector3(0f, 0f, 45f));
            Part(PrimitiveType.Sphere, head, new Vector3(0.09f, 0.04f, 0.1f), Vector3.one * 0.05f, dark, Vector3.zero);
            Part(PrimitiveType.Sphere, head, new Vector3(-0.09f, 0.04f, 0.1f), Vector3.one * 0.05f, dark, Vector3.zero);

            wingL = Wing(body, -1f, felt);
            wingR = Wing(body, 1f, felt);
        }

        Transform Wing(Transform parent, float side, Material mat)
        {
            var pivot = new GameObject(side < 0f ? "WingL" : "WingR").transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = new Vector3(0.17f * side, 0.06f, 0f);
            Part(PrimitiveType.Sphere, pivot, new Vector3(0.18f * side, 0f, -0.02f), new Vector3(0.36f, 0.05f, 0.26f), mat, Vector3.zero);
            return pivot;
        }

        static void Part(PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Material mat, Vector3 euler)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        Vector3 CirclePoint(float bob)
        {
            return home + new Vector3(Mathf.Cos(angle) * radius, bob, Mathf.Sin(angle) * radius);
        }

        Vector3 PlayerPosition(out bool has)
        {
            var g = PatchworkGame.Instance;
            has = g != null && g.Player != null;
            return has ? g.Player.Motor.Center : Vector3.zero;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            t += dt;
            stateTime += dt;
            bool hasPlayer;
            Vector3 player = PlayerPosition(out hasPlayer);
            bool flying = mode == Mode.Circle || state != State.Sit;

            if (mode == Mode.Circle) FlyCircle(dt);
            else
            {
                switch (state)
                {
                    case State.Sit:
                        SitAround(dt);
                        if (hasPlayer && (player - home).sqrMagnitude < FlushDistance * FlushDistance)
                        {
                            state = State.FlyOff;
                            stateTime = 0f;
                            from = transform.position;
                            Vector3 away = home - player;
                            away.y = 0f;
                            if (away.sqrMagnitude < 0.01f) away = Vector3.forward;
                            to = home + away.normalized * 5f + Vector3.up * 6f;
                            chirpTimer = 0.05f; // a startled chirp
                        }
                        break;
                    case State.FlyOff:
                    {
                        float k = Mathf.Clamp01(stateTime / 1.2f);
                        Vector3 p = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, k));
                        Face(p - transform.position, dt);
                        transform.position = p;
                        if (k >= 1f) { state = State.Loiter; stateTime = 0f; angle = Mathf.Atan2(to.z - home.z, to.x - home.x); }
                        break;
                    }
                    case State.Loiter:
                    {
                        angle += 4f / 5f * dt * Mathf.Sign(speed);
                        Vector3 p = home + new Vector3(Mathf.Cos(angle) * 5f, 6f + Mathf.Sin(t * 1.3f) * 0.4f, Mathf.Sin(angle) * 5f);
                        Face(p - transform.position, dt);
                        transform.position = Vector3.Lerp(transform.position, p, 1f - Mathf.Exp(-6f * dt));
                        bool clear = !hasPlayer || (player - home).sqrMagnitude > 49f;
                        if (stateTime > 6f && clear) { state = State.Return; stateTime = 0f; from = transform.position; }
                        break;
                    }
                    case State.Return:
                    {
                        float k = Mathf.Clamp01(stateTime / 1.4f);
                        Vector3 p = Vector3.Lerp(from, home, Mathf.SmoothStep(0f, 1f, k)) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.2f;
                        Face(p - transform.position, dt);
                        transform.position = p;
                        if (k >= 1f) { state = State.Sit; stateTime = 0f; transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f); }
                        break;
                    }
                }
            }

            Animate(flying, dt);

            // Chirp now and then when Patch is close enough to hear it.
            chirpTimer -= dt;
            if (chirpTimer <= 0f)
            {
                bool startled = state == State.FlyOff;
                chirpTimer = startled ? Random.Range(1.5f, 3f) : Random.Range(3f, 9f);
                if (hasPlayer && (player - transform.position).sqrMagnitude < HearDistance * HearDistance && voice != null)
                {
                    voice.pitch = Random.Range(0.85f, 1.25f) * (startled ? 1.15f : 1f);
                    voice.PlayOneShot(Sfx.BirdChirp, 0.45f * Sfx.Volume / 10f);
                }
            }
        }

        void FlyCircle(float dt)
        {
            angle += speed / radius * dt;
            Vector3 p = CirclePoint(Mathf.Sin(t * 0.8f) * 0.6f);
            Face(p - transform.position, dt);
            transform.position = p;
        }

        // Sitting: hop to a new spot nearby every so often, and peck in between.
        void SitAround(float dt)
        {
            hopTimer -= dt;
            if (hopTimer <= 0f)
            {
                hopTimer = Random.Range(1.2f, 3.5f);
                float turn = Random.Range(-90f, 90f);
                transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + turn, 0f);
                stateTime = 0f;
            }
            // A little hop arc right after turning.
            float hop = stateTime < 0.25f ? Mathf.Sin(stateTime / 0.25f * Mathf.PI) * 0.15f : 0f;
            transform.position = home + Vector3.up * hop;
        }

        void Face(Vector3 dir, float dt)
        {
            if (dir.sqrMagnitude < 0.00001f) return;
            Quaternion want = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, want, 1f - Mathf.Exp(-8f * dt));
        }

        void Animate(bool flying, float dt)
        {
            if (wingL == null) return;
            float flap = flying ? Mathf.Sin(t * 22f) * 55f : 0f;
            if (!flying) flap = -8f;
            wingL.localRotation = Quaternion.Euler(0f, 0f, flap);
            wingR.localRotation = Quaternion.Euler(0f, 0f, -flap);
            // Pecking when sitting, a little head bob when flying.
            float peck = !flying ? Mathf.Max(0f, Mathf.Sin(t * 3.1f)) * Mathf.Max(0f, Mathf.Sin(t * 0.7f)) * 40f : Mathf.Sin(t * 11f) * 4f;
            head.localRotation = Quaternion.Euler(peck, Mathf.Sin(t * 0.9f) * 20f, 0f);
            body.localRotation = Quaternion.Euler(flying ? -8f : 0f, 0f, 0f);
        }
    }
}

using UnityEngine;

namespace Patchwork
{
    // A little creature of the Fray: a scuttling grey tangle with two glowing eyes. It patrols back
    // and forth, turns to chase Patch when he's close, and pushes him away if he walks into it.
    // Land on its head (or crash into it fast) to unravel it.
    public class Snarling : MonoBehaviour
    {
        public System.Action<Snarling> Popped;
        public bool IsPopped { get; private set; }

        Vector3 a, b;
        float speed = 2.2f;
        float t;
        float phase;
        float bumpCooldown;
        Transform body, eyeL, eyeR;
        Vector3 pos;
        Vector3 facing = Vector3.forward;
        const float ChaseRange = 7f;
        const float HitRadius = 1.1f;

        public Vector3 Center { get { return pos + Vector3.up * 0.7f; } }

        public static Snarling Create(Transform parent, Vector3 from, Vector3 to)
        {
            var go = new GameObject("Snarling");
            go.transform.SetParent(parent, false);
            var s = go.AddComponent<Snarling>();
            s.a = from;
            s.b = to;
            s.pos = from;
            s.phase = Random.value;
            s.Build();
            go.transform.position = from;
            return s;
        }

        void Build()
        {
            body = new GameObject("Body").transform;
            body.SetParent(transform, false);
            body.localPosition = new Vector3(0f, 0.7f, 0f);
            var grey = PatchworkAssets.Yarn(SnarlKnot.FrayGrey);
            var dark = PatchworkAssets.Yarn(SnarlKnot.FrayDark);
            Part(PrimitiveType.Sphere, body, Vector3.zero, new Vector3(1.2f, 1f, 1.1f), dark, Vector3.zero);
            for (int i = 0; i < 9; i++)
            {
                float ang = i * 40f;
                Part(PrimitiveType.Capsule, body, Quaternion.Euler(i * 23f, ang, 0f) * new Vector3(0f, 0f, 0.42f),
                    new Vector3(0.12f, 0.45f, 0.12f), grey, new Vector3(i * 31f, ang, i * 17f));
            }
            var glow = PatchworkAssets.Glow(SnarlKnot.FrayGlow, 1.4f);
            eyeL = Part(PrimitiveType.Sphere, body, new Vector3(-0.2f, 0.12f, 0.5f), new Vector3(0.18f, 0.24f, 0.1f), glow, Vector3.zero);
            eyeR = Part(PrimitiveType.Sphere, body, new Vector3(0.2f, 0.12f, 0.5f), new Vector3(0.18f, 0.24f, 0.1f), glow, Vector3.zero);
            // Stubby legs.
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * 0.32f, z = (i < 2 ? -1f : 1f) * 0.25f;
                Part(PrimitiveType.Capsule, transform, new Vector3(x, 0.15f, z), new Vector3(0.16f, 0.18f, 0.16f), dark, Vector3.zero);
            }
        }

        static Transform Part(PrimitiveType type, Transform parent, Vector3 p, Vector3 s, Material m, Vector3 e)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.Destroy(go.GetComponent<Collider>());
            go.layer = 2;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = p;
            go.transform.localRotation = Quaternion.Euler(e);
            go.transform.localScale = s;
            go.GetComponent<Renderer>().sharedMaterial = m;
            return go.transform;
        }

        void Update()
        {
            if (IsPopped) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            t += dt;
            bumpCooldown -= dt;

            var game = PatchworkGame.Instance;
            PatchworkPlayer player = game != null ? game.Player : null;

            // Patrol between a and b; scurry toward Patch if he comes close (but stay near home).
            Vector3 target;
            float k = Mathf.PingPong(t * speed / Mathf.Max(1f, Vector3.Distance(a, b)) + phase, 1f);
            target = Vector3.Lerp(a, b, k);
            if (player != null)
            {
                Vector3 pp = player.transform.position;
                Vector3 home = (a + b) * 0.5f;
                if ((pp - pos).sqrMagnitude < ChaseRange * ChaseRange && (pp - home).sqrMagnitude < 144f && Mathf.Abs(pp.y - pos.y) < 2.5f)
                    target = new Vector3(pp.x, pos.y, pp.z);
            }
            Vector3 step = Vector3.MoveTowards(pos, target, speed * 1.2f * dt) - pos;
            if (step.sqrMagnitude > 0.000001f) facing = Vector3.Slerp(facing, step.normalized, 1f - Mathf.Exp(-8f * dt));
            pos += step;
            transform.position = pos + Vector3.up * Mathf.Abs(Mathf.Sin(t * 9f)) * 0.08f;
            transform.rotation = Quaternion.LookRotation(new Vector3(facing.x, 0f, facing.z).normalized, Vector3.up);
            body.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 9f) * 6f);
            float blink = Mathf.Repeat(t, 3.3f) < 0.12f ? 0.2f : 1f;
            eyeL.localScale = new Vector3(0.18f, 0.24f * blink, 0.1f);
            eyeR.localScale = new Vector3(0.18f, 0.24f * blink, 0.1f);

            if (player == null) return;
            Vector3 d = player.Motor.Center - Center;
            if (new Vector2(d.x, d.z).magnitude < HitRadius && Mathf.Abs(d.y) < 1.4f)
            {
                if (SnarlKnot.HitHard(player, Center)) Pop(player);
                else if (bumpCooldown <= 0f)
                {
                    bumpCooldown = 0.7f;
                    SnarlKnot.Bump(player, Center);
                }
            }
        }

        public void Pop(PatchworkPlayer player)
        {
            if (IsPopped) return;
            IsPopped = true;
            if (player != null && player.Motor.Velocity.y < 0f) player.Motor.Bounce(9f);
            Haptics.Pulse(0.5f, 0.3f, 0.12f);
            Sfx.Play(SfxId.Unravel, 0.6f);
            Sfx.Play(SfxId.Boing, 0.4f);
            FxBurst.Spawn(FxBurst.Kind.Threads, Center, SnarlKnot.FrayGrey);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, Center, new Color(1f, 0.85f, 0.5f));
            gameObject.SetActive(false);
            if (Popped != null) Popped(this);
        }

        public void RemoveQuietly()
        {
            IsPopped = true;
            gameObject.SetActive(false);
        }
    }
}

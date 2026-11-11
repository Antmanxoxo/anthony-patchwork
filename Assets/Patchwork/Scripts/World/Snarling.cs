using UnityEngine;

namespace Patchwork
{
    // A little creature of the Fray: a dome of ash-grey fur striding about on two long scaly
    // legs, with its eyes out on stalks (what it looks like is in FrayBodies.Walker). It patrols
    // back and forth, turns to chase Patch when he's close, and nips him if he walks into it.
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
        Transform body, eyeL, eyeR, legL, legR;
        Vector3 eyeSize;
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
            var fc = go.AddComponent<FrayCreature>();
            fc.Center = () => s.Center;
            fc.Pop = p => s.Pop(p);
            go.transform.position = from;
            return s;
        }

        void Build()
        {
            body = new GameObject("Body").transform;
            body.SetParent(transform, false);
            body.localPosition = new Vector3(0f, 0.7f, 0f);
            FrayBodies.Walker(transform, body, out eyeL, out eyeR, out legL, out legR, phase * 7f);
            eyeSize = eyeL.localScale;
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
            // Long strides (faster when it's after him), and eyes that never blink together.
            float stride = Mathf.Sin(t * 9f) * Mathf.Clamp01(step.magnitude / Mathf.Max(0.0001f, dt) / speed) * 30f;
            legL.localRotation = Quaternion.Euler(stride, 0f, 0f);
            legR.localRotation = Quaternion.Euler(-stride, 0f, 0f);
            eyeL.localScale = new Vector3(eyeSize.x, eyeSize.y * (Mathf.Repeat(t + phase * 3f, 3.3f) < 0.12f ? 0.15f : 1f), eyeSize.z);
            eyeR.localScale = new Vector3(eyeSize.x, eyeSize.y * (Mathf.Repeat(t * 0.83f + phase * 5f, 4.1f) < 0.12f ? 0.15f : 1f), eyeSize.z);

            if (player == null) return;
            Vector3 d = player.Motor.Center - Center;
            if (new Vector2(d.x, d.z).magnitude < HitRadius && Mathf.Abs(d.y) < 1.4f)
            {
                if (SnarlKnot.HitHard(player, Center)) Pop(player);
                else if (bumpCooldown <= 0f)
                {
                    // A nip: Patch loses a heart (and gets knocked back).
                    bumpCooldown = 0.7f;
                    if (game != null) game.Hurt(Center);
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
            ScoreKeeper.Bonus(150, Center);
            ScoreBubbles.BurstAt(Center, 6);
            Monsters.Popped(Center);
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

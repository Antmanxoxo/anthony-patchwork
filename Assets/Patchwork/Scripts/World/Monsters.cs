using UnityEngine;

namespace Patchwork
{
    // Shared bits for the Fray's creatures: touching Patch (stomped, or a hit on him), stomp
    // combos, and the odd heart they drop.
    public static class Monsters
    {
        static int combo;
        static float comboAge;

        public static void ResetStatics() { combo = 0; comboAge = 0f; }

        // Call every frame a creature overlaps Patch. Returns true if Patch popped it.
        public static bool Touch(PatchworkPlayer player, Vector3 center)
        {
            if (SnarlKnot.HitHard(player, center)) return true;
            var game = player.Game;
            if (game != null) game.Hurt(center);
            return false;
        }

        // Every creature calls this as it pops: stomp combos and heart drops.
        public static void Popped(Vector3 at)
        {
            var game = PatchworkGame.Instance;
            var player = game != null ? game.Player : null;
            // Chains of stomps without touching the ground score more and more.
            if (player != null && !player.Motor.Grounded && Time.time - comboAge < 3f) combo++;
            else combo = 1;
            comboAge = Time.time;
            Merits.Add("pops");
            if (game != null)
            {
                game.HitStop(0.06f);
                if (game.CameraRig != null) game.CameraRig.Kick(0.18f);
            }
            if (combo >= 3) Merits.Earn(Merits.Id.ComboCrafter);
            if (combo >= 2)
            {
                int bonus = 100 * (1 << Mathf.Min(combo - 1, 5));
                ScoreKeeper.Bonus(bonus, at + Vector3.up * 1.2f);
                if (game != null) game.ShowToast("Stomp combo x" + combo + "!");
                Sfx.PlayPitched(SfxId.Collect, 0.5f, 1f + combo * 0.08f);
            }
            if (game != null && game.Hearts < PatchworkGame.MaxHearts && Random.value < 0.4f)
                HeartPickup.Spawn(at + Vector3.up * 0.3f, true);
        }

        // Builds a body part with no collider (the creatures check distances themselves).
        public static Transform Part(PrimitiveType type, Transform parent, Vector3 p, Vector3 s, Material m, Vector3 e = default(Vector3))
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

        // Ground height under a point (or the point's own height if there's nothing there).
        public static float Ground(Vector3 p)
        {
            RaycastHit hit;
            if (Physics.Raycast(p + Vector3.up * 2f, Vector3.down, out hit, 12f, ~(1 << 2), QueryTriggerInteraction.Ignore)) return hit.point.y;
            return p.y;
        }

        // Wind's gust unravels any creature right in front of it.
        public static void Gust(PatchworkPlayer player, Vector3 origin, Vector3 dir)
        {
            var list = new System.Collections.Generic.List<FrayCreature>(FrayCreature.All);
            foreach (var c in list)
            {
                if (c == null || c.Center == null || c.Pop == null) continue;
                Vector3 d = c.Center() - origin;
                if (d.magnitude > 3.8f || Vector3.Dot(d.normalized, dir) < 0.2f) continue;
                c.Pop(player);
            }
        }

        public static void PopFx(Vector3 c)
        {
            Haptics.Pulse(0.5f, 0.3f, 0.12f);
            Sfx.Play(SfxId.Unravel, 0.6f);
            Sfx.Play(SfxId.Boing, 0.4f);
            FxBurst.Spawn(FxBurst.Kind.Threads, c, SnarlKnot.FrayGrey);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, c, new Color(1f, 0.85f, 0.5f));
        }

        static Material grey, dark, glow;
        public static Material Grey { get { if (grey == null) grey = PatchworkAssets.Yarn(SnarlKnot.FrayGrey); return grey; } }
        public static Material Dark { get { if (dark == null) dark = PatchworkAssets.Yarn(SnarlKnot.FrayDark); return dark; } }
        public static Material Glow { get { if (glow == null) glow = PatchworkAssets.Glow(SnarlKnot.FrayGlow, 1.4f); return glow; } }
    }

    // Marks something as a creature of the Fray (so Wind's gust can find it).
    public class FrayCreature : MonoBehaviour
    {
        public static readonly System.Collections.Generic.List<FrayCreature> All = new System.Collections.Generic.List<FrayCreature>();
        public System.Func<Vector3> Center;
        public System.Action<PatchworkPlayer> Pop;
        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }
    }

    // A felt heart that patches Patch up by one heart. Spins and bobs; dropped ones hop out first.
    public class HeartPickup : PlayerVolume
    {
        Transform visual;
        Vector3 home, vel;
        bool dropping;
        float t, life = -1f;

        public static HeartPickup Spawn(Vector3 pos, bool dropped)
        {
            var go = new GameObject("HeartPickup");
            go.transform.position = pos;
            var h = go.AddComponent<HeartPickup>();
            h.Size = Vector3.one * 1.3f;
            h.home = pos;
            h.visual = new GameObject("Heart").transform;
            h.visual.SetParent(go.transform, false);
            var red = PatchworkAssets.Felt(new Color(0.95f, 0.3f, 0.4f));
            Monsters.Part(PrimitiveType.Sphere, h.visual, new Vector3(-0.17f, 0.12f, 0f), new Vector3(0.42f, 0.42f, 0.26f), red);
            Monsters.Part(PrimitiveType.Sphere, h.visual, new Vector3(0.17f, 0.12f, 0f), new Vector3(0.42f, 0.42f, 0.26f), red);
            Monsters.Part(PrimitiveType.Cube, h.visual, new Vector3(0f, -0.06f, 0f), new Vector3(0.42f, 0.42f, 0.24f), red, new Vector3(0f, 0f, 45f));
            Monsters.Part(PrimitiveType.Sphere, h.visual, new Vector3(-0.2f, 0.2f, -0.12f), new Vector3(0.12f, 0.08f, 0.04f), PatchworkAssets.Glow(Color.white, 0.6f));
            if (dropped)
            {
                h.dropping = true;
                h.vel = new Vector3(Random.Range(-1.5f, 1.5f), 6f, Random.Range(-1.5f, 1.5f));
                h.life = 12f;
            }
            return h;
        }

        protected override void OnPlayerStay(PatchworkPlayer player, float dt)
        {
            var game = player.Game;
            if (game == null || dropping) return;
            if (!game.Heal(1))
            {
                ScoreKeeper.Bonus(50, transform.position);
            }
            Sfx.Play(SfxId.Collect, 0.6f);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position, new Color(1f, 0.5f, 0.6f));
            Destroy(gameObject);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            t += dt;
            if (dropping)
            {
                vel.y -= 20f * dt;
                Vector3 p = transform.position + vel * dt;
                float g = Monsters.Ground(transform.position) + 0.6f;
                if (p.y <= g && vel.y < 0f) { p.y = g; dropping = false; home = p; }
                transform.position = p;
            }
            else transform.position = home + Vector3.up * Mathf.Sin(t * 2.5f) * 0.12f;
            if (visual != null) visual.localRotation = Quaternion.Euler(0f, t * 120f, 0f);
            if (life > 0f)
            {
                life -= dt;
                if (life < 3f && visual != null) visual.gameObject.SetActive(Mathf.Repeat(life * 6f, 1f) > 0.3f);
                if (life <= 0f) Destroy(gameObject);
            }
        }
    }

    // A Static Moth: a grey felt moth that flutters about, and every few seconds swoops down at
    // Patch. Jump on it (or gust it, or swing into it) to unravel it.
    public class StaticMoth : MonoBehaviour
    {
        Vector3 home, pos;
        Transform wingL, wingR, body;
        float t, swoopTimer = 3f, swoop = -1f;
        Vector3 swoopFrom, swoopTo;
        bool popped;
        const float Hover = 2.6f;

        public static StaticMoth Create(Transform parent, Vector3 ground)
        {
            var go = new GameObject("StaticMoth");
            go.transform.SetParent(parent, false);
            var m = go.AddComponent<StaticMoth>();
            m.home = ground + Vector3.up * Hover;
            m.pos = m.home;
            m.t = Random.value * 10f;
            go.transform.position = m.pos;
            m.body = new GameObject("Body").transform;
            m.body.SetParent(go.transform, false);
            Monsters.Part(PrimitiveType.Sphere, m.body, Vector3.zero, new Vector3(0.45f, 0.42f, 0.8f), Monsters.Dark);
            Monsters.Part(PrimitiveType.Sphere, m.body, new Vector3(0f, 0.05f, 0.42f), Vector3.one * 0.38f, Monsters.Grey);
            Monsters.Part(PrimitiveType.Sphere, m.body, new Vector3(-0.1f, 0.1f, 0.58f), new Vector3(0.12f, 0.15f, 0.08f), Monsters.Glow);
            Monsters.Part(PrimitiveType.Sphere, m.body, new Vector3(0.1f, 0.1f, 0.58f), new Vector3(0.12f, 0.15f, 0.08f), Monsters.Glow);
            for (int s = -1; s <= 1; s += 2)
                Monsters.Part(PrimitiveType.Cylinder, m.body, new Vector3(s * 0.1f, 0.3f, 0.6f), new Vector3(0.03f, 0.18f, 0.03f), Monsters.Dark, new Vector3(-40f, 0f, s * -20f));
            var wing = PatchworkAssets.Felt(new Color(0.62f, 0.6f, 0.66f));
            m.wingL = new GameObject("WingL").transform;
            m.wingL.SetParent(m.body, false);
            Monsters.Part(PrimitiveType.Sphere, m.wingL, new Vector3(-0.55f, 0f, 0f), new Vector3(1f, 0.06f, 0.75f), wing);
            var fc = go.AddComponent<FrayCreature>();
            fc.Center = () => m.pos;
            fc.Pop = p => m.Pop(p);
            m.wingR = new GameObject("WingR").transform;
            m.wingR.SetParent(m.body, false);
            Monsters.Part(PrimitiveType.Sphere, m.wingR, new Vector3(0.55f, 0f, 0f), new Vector3(1f, 0.06f, 0.75f), wing);
            return m;
        }

        void Update()
        {
            if (popped) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            t += dt;
            var game = PatchworkGame.Instance;
            var player = game != null ? game.Player : null;

            Vector3 want = home + new Vector3(Mathf.Sin(t * 0.7f) * 3f, Mathf.Sin(t * 1.9f) * 0.3f, Mathf.Sin(t * 1.4f) * 1.5f);
            if (swoop >= 0f)
            {
                swoop += dt / 1.3f;
                float k = Mathf.Clamp01(swoop);
                // Down to Patch's height and back up again, in one swoop.
                want = Vector3.Lerp(swoopFrom, swoopTo, k) + Vector3.down * Mathf.Sin(k * Mathf.PI) * (Hover - 0.9f);
                if (k >= 1f) { swoop = -1f; swoopTimer = Random.Range(2.5f, 4f); }
                pos = want;
            }
            else
            {
                pos = Vector3.Lerp(pos, want, 1f - Mathf.Exp(-2f * dt));
                if (player != null)
                {
                    Vector3 pp = player.transform.position;
                    float d = new Vector2(pp.x - pos.x, pp.z - pos.z).magnitude;
                    if (d < 10f && Mathf.Abs(pp.y + Hover - pos.y) < 4f && (pp - home).sqrMagnitude < 400f)
                    {
                        swoopTimer -= dt;
                        if (swoopTimer <= 0f)
                        {
                            swoop = 0f;
                            swoopFrom = pos;
                            Vector3 over = new Vector3(pp.x - pos.x, 0f, pp.z - pos.z);
                            swoopTo = pos + over * 2f;
                            swoopTo.y = pos.y;
                            Sfx.Play(SfxId.Gust, 0.25f);
                        }
                    }
                }
            }
            transform.position = pos;

            Vector3 face = player != null ? player.transform.position - pos : Vector3.forward;
            face.y = 0f;
            if (face.sqrMagnitude > 0.01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(face.normalized, Vector3.up), 1f - Mathf.Exp(-4f * dt));
            float flap = Mathf.Sin(t * (swoop >= 0f ? 30f : 18f)) * 35f;
            wingL.localRotation = Quaternion.Euler(0f, 0f, -flap);
            wingR.localRotation = Quaternion.Euler(0f, 0f, flap);

            if (player == null) return;
            Vector3 dd = player.Motor.Center - pos;
            if (new Vector2(dd.x, dd.z).magnitude < 1.1f && Mathf.Abs(dd.y) < 1.1f)
            {
                if (Monsters.Touch(player, pos)) Pop(player);
            }
        }

        public void Pop(PatchworkPlayer player)
        {
            if (popped) return;
            popped = true;
            if (player.Motor.Velocity.y < 0f) player.Motor.Bounce(10f);
            Monsters.PopFx(pos);
            ScoreKeeper.Bonus(200, pos);
            Monsters.Popped(pos);
            gameObject.SetActive(false);
        }
    }

    // A Tangle Hopper: a springy grey yarn ball that bounces after Patch when he's near.
    public class TangleHopper : MonoBehaviour
    {
        Vector3 home, pos, hopFrom, hopTo;
        float t, hop = -1f, rest = 1f, squash;
        Transform body;
        bool popped;

        public static TangleHopper Create(Transform parent, Vector3 ground)
        {
            var go = new GameObject("TangleHopper");
            go.transform.SetParent(parent, false);
            var h = go.AddComponent<TangleHopper>();
            h.home = ground;
            h.pos = ground;
            h.rest = Random.Range(0.5f, 1.5f);
            go.transform.position = ground;
            h.body = new GameObject("Body").transform;
            h.body.SetParent(go.transform, false);
            h.body.localPosition = Vector3.up * 0.55f;
            var fc = go.AddComponent<FrayCreature>();
            fc.Center = () => h.pos + Vector3.up * 0.55f;
            fc.Pop = p => h.Pop(p, h.pos + Vector3.up * 0.55f);
            Monsters.Part(PrimitiveType.Sphere, h.body, Vector3.zero, Vector3.one * 1f, Monsters.Dark);
            for (int i = 0; i < 7; i++)
                Monsters.Part(PrimitiveType.Capsule, h.body, Quaternion.Euler(i * 31f, i * 51f, 0f) * new Vector3(0f, 0f, 0.38f), new Vector3(0.1f, 0.4f, 0.1f), Monsters.Grey, new Vector3(i * 31f, i * 51f, i * 17f));
            Monsters.Part(PrimitiveType.Sphere, h.body, new Vector3(-0.18f, 0.12f, 0.45f), new Vector3(0.16f, 0.22f, 0.1f), Monsters.Glow);
            Monsters.Part(PrimitiveType.Sphere, h.body, new Vector3(0.18f, 0.12f, 0.45f), new Vector3(0.16f, 0.22f, 0.1f), Monsters.Glow);
            // A spring underneath.
            for (int i = 0; i < 4; i++)
                Monsters.Part(PrimitiveType.Cylinder, go.transform, new Vector3(0f, 0.06f + i * 0.1f, 0f), new Vector3(0.36f, 0.02f, 0.36f), PatchworkAssets.Plain(new Color(0.7f, 0.72f, 0.76f), 0.8f));
            return h;
        }

        void Update()
        {
            if (popped) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            t += dt;
            squash = Mathf.MoveTowards(squash, 0f, dt * 4f);
            var game = PatchworkGame.Instance;
            var player = game != null ? game.Player : null;

            if (hop >= 0f)
            {
                hop += dt / 0.6f;
                float k = Mathf.Clamp01(hop);
                pos = Vector3.Lerp(hopFrom, hopTo, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.6f;
                if (k >= 1f) { hop = -1f; rest = 0.55f; squash = 1f; Sfx.Play(SfxId.Boing, 0.15f); }
            }
            else
            {
                rest -= dt;
                if (rest <= 0f)
                {
                    Vector3 target = home + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
                    if (player != null)
                    {
                        Vector3 pp = player.transform.position;
                        if ((pp - pos).sqrMagnitude < 81f && (pp - home).sqrMagnitude < 196f && Mathf.Abs(pp.y - pos.y) < 2.5f)
                            target = pos + Vector3.ClampMagnitude(new Vector3(pp.x - pos.x, 0f, pp.z - pos.z), 2.8f);
                    }
                    if ((target - home).sqrMagnitude > 196f) target = home;
                    target.y = Monsters.Ground(target);
                    if (Mathf.Abs(target.y - pos.y) > 1.2f) target = pos;   // don't hop off ledges
                    hopFrom = pos;
                    hopTo = target;
                    hop = 0f;
                }
            }
            transform.position = pos;
            if (player != null)
            {
                Vector3 face = player.transform.position - pos;
                face.y = 0f;
                if (face.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(face.normalized, Vector3.up);
            }
            float sq = Mathf.Sin(squash * Mathf.PI) * 0.3f;
            body.localScale = new Vector3(1f + sq, 1f - sq, 1f + sq);

            if (player == null) return;
            Vector3 c = pos + Vector3.up * 0.55f;
            Vector3 dd = player.Motor.Center - c;
            if (new Vector2(dd.x, dd.z).magnitude < 1.05f && Mathf.Abs(dd.y) < 1.2f)
            {
                if (Monsters.Touch(player, c)) Pop(player, c);
            }
        }

        public void Pop(PatchworkPlayer player, Vector3 c)
        {
            if (popped) return;
            popped = true;
            if (player.Motor.Velocity.y < 0f) player.Motor.Bounce(11f);
            Monsters.PopFx(c);
            ScoreKeeper.Bonus(150, c);
            Monsters.Popped(c);
            gameObject.SetActive(false);
        }
    }

    // A Static Spitter: a grumpy grey knot on a little cushion that lobs crackly fuzzballs at
    // Patch from a distance. Dodge them, then stomp it.
    public class StaticSpitter : MonoBehaviour
    {
        Vector3 pos;
        Transform body, mouth;
        float t, cooldown = 2f, charge;
        bool popped;

        public static StaticSpitter Create(Transform parent, Vector3 ground)
        {
            var go = new GameObject("StaticSpitter");
            go.transform.SetParent(parent, false);
            var s = go.AddComponent<StaticSpitter>();
            s.pos = ground;
            go.transform.position = ground;
            Monsters.Part(PrimitiveType.Cylinder, go.transform, new Vector3(0f, 0.18f, 0f), new Vector3(1.4f, 0.18f, 1.4f), PatchworkAssets.Felt(new Color(0.45f, 0.42f, 0.5f)));
            s.body = new GameObject("Body").transform;
            s.body.SetParent(go.transform, false);
            s.body.localPosition = Vector3.up * 0.95f;
            Monsters.Part(PrimitiveType.Sphere, s.body, Vector3.zero, new Vector3(1.2f, 1.1f, 1.2f), Monsters.Dark);
            for (int i = 0; i < 10; i++)
                Monsters.Part(PrimitiveType.Capsule, s.body, Quaternion.Euler(i * 27f, i * 37f, 0f) * new Vector3(0f, 0f, 0.48f), new Vector3(0.12f, 0.5f, 0.12f), Monsters.Grey, new Vector3(i * 27f, i * 37f, i * 13f));
            Monsters.Part(PrimitiveType.Sphere, s.body, new Vector3(-0.22f, 0.2f, 0.52f), new Vector3(0.2f, 0.12f, 0.1f), Monsters.Glow, new Vector3(0f, 0f, -15f));
            Monsters.Part(PrimitiveType.Sphere, s.body, new Vector3(0.22f, 0.2f, 0.52f), new Vector3(0.2f, 0.12f, 0.1f), Monsters.Glow, new Vector3(0f, 0f, 15f));
            var fc = go.AddComponent<FrayCreature>();
            fc.Center = () => s.pos + Vector3.up * 0.95f;
            fc.Pop = p => s.Pop(p);
            s.mouth = Monsters.Part(PrimitiveType.Sphere, s.body, new Vector3(0f, -0.15f, 0.55f), new Vector3(0.3f, 0.12f, 0.1f), PatchworkAssets.Plain(new Color(0.08f, 0.06f, 0.1f), 0.2f));
            return s;
        }

        void Update()
        {
            if (popped) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            t += dt;
            var game = PatchworkGame.Instance;
            var player = game != null ? game.Player : null;
            if (player == null) return;
            Vector3 c = pos + Vector3.up * 0.95f;
            Vector3 to = player.Motor.Center - c;
            float d = to.magnitude;
            Vector3 flat = new Vector3(to.x, 0f, to.z);
            if (flat.sqrMagnitude > 0.01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat.normalized, Vector3.up), 1f - Mathf.Exp(-5f * dt));

            if (d < 16f && d > 2.5f && game.Mode == UIMode.Gameplay)
            {
                cooldown -= dt;
                charge = cooldown < 0.6f ? 1f - cooldown / 0.6f : 0f;
                if (cooldown <= 0f)
                {
                    cooldown = Random.Range(2.4f, 3.2f);
                    FuzzBall.Launch(c + transform.forward * 0.7f, player.transform.position + player.Motor.Velocity * 0.35f);
                    Sfx.Play(SfxId.Steam, 0.35f);
                    charge = 0f;
                }
            }
            else charge = Mathf.MoveTowards(charge, 0f, dt * 3f);
            float puff = 1f + charge * 0.25f + Mathf.Sin(t * 3f) * 0.03f;
            body.localScale = Vector3.one * puff;
            mouth.localScale = new Vector3(0.3f, 0.12f + charge * 0.2f, 0.1f);

            Vector3 dd = player.Motor.Center - c;
            if (new Vector2(dd.x, dd.z).magnitude < 1.2f && Mathf.Abs(dd.y) < 1.4f)
            {
                if (Monsters.Touch(player, c)) Pop(player);
            }
        }

        public void Pop(PatchworkPlayer player)
        {
            if (popped) return;
            popped = true;
            Vector3 c = pos + Vector3.up * 0.95f;
            if (player.Motor.Velocity.y < 0f) player.Motor.Bounce(10f);
            Monsters.PopFx(c);
            ScoreKeeper.Bonus(250, c);
            Monsters.Popped(c);
            gameObject.SetActive(false);
        }
    }

    // A crackly ball of static, lobbed in an arc. Hurts if it hits Patch; fizzles on the ground.
    public class FuzzBall : MonoBehaviour
    {
        Vector3 vel;
        float life = 4f;

        public static void Launch(Vector3 from, Vector3 target)
        {
            var go = new GameObject("FuzzBall");
            go.transform.position = from;
            var f = go.AddComponent<FuzzBall>();
            Monsters.Part(PrimitiveType.Sphere, go.transform, Vector3.zero, Vector3.one * 0.5f, Monsters.Grey);
            Monsters.Part(PrimitiveType.Sphere, go.transform, Vector3.zero, Vector3.one * 0.3f, Monsters.Glow);
            // Aim a 1.1 second arc at where Patch is heading.
            const float T = 1.1f, g = 14f;
            Vector3 d = target + Vector3.up * 0.6f - from;
            f.vel = new Vector3(d.x / T, (d.y + 0.5f * g * T * T) / T, d.z / T);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            vel.y -= 14f * dt;
            transform.position += vel * dt;
            transform.Rotate(300f * dt, 200f * dt, 0f);
            life -= dt;
            var game = PatchworkGame.Instance;
            if (game != null && game.Player != null)
            {
                if ((game.Player.Motor.Center - transform.position).sqrMagnitude < 0.8f)
                {
                    game.Hurt(transform.position);
                    Fizzle();
                    return;
                }
            }
            if (life <= 0f || (vel.y < 0f && transform.position.y < Monsters.Ground(transform.position) + 0.2f)) Fizzle();
        }

        void Fizzle()
        {
            FxBurst.Spawn(FxBurst.Kind.Poof, transform.position, new Color(0.7f, 0.7f, 0.75f));
            Sfx.Play(SfxId.Pop, 0.25f);
            Destroy(gameObject);
        }
    }
}

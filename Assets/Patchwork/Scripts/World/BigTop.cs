using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Flies Patch along a scripted arc (the Big Top's cannon). Input is locked while it runs.
    public class ScriptedHop : MonoBehaviour
    {
        public static bool Busy { get; private set; }
        Vector3 from, to;
        float apex, duration, t, yaw;
        System.Action done;

        public static void ResetStatics() { Busy = false; }

        public static void Fly(Vector3 from, Vector3 to, float apex, float duration, System.Action done = null)
        {
            var go = new GameObject("ScriptedHop");
            var h = go.AddComponent<ScriptedHop>();
            h.from = from;
            h.to = to;
            h.apex = apex;
            h.duration = duration;
            h.done = done;
            Vector3 d = to - from;
            h.yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            Busy = true;
        }

        void OnDestroy() { Busy = false; }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) { Destroy(gameObject); return; }
            var motor = game.Player.Motor;
            t += Time.deltaTime / duration;
            float k = Mathf.Clamp01(t);
            Vector3 mid = (from + to) * 0.5f + Vector3.up * apex * 2f;
            Vector3 p = Vector3.Lerp(Vector3.Lerp(from, mid, k), Vector3.Lerp(mid, to, k), k);
            motor.ExternalControl = true;
            motor.Teleport(p, yaw);
            if (k >= 1f)
            {
                motor.ExternalControl = false;
                game.Player.Visuals.PunchScale(new Vector3(1.3f, 0.7f, 1.3f));
                Sfx.Play(SfxId.Land, 0.7f);
                FxBurst.Spawn(FxBurst.Kind.Scraps, to + Vector3.up * 0.3f, new Color(1f, 0.85f, 0.5f));
                if (done != null) done();
                Destroy(gameObject);
            }
        }
    }

    // The Big Top's audience: little felt toys on the bleachers who bob along and leap up and
    // cheer when something good happens in the ring.
    public class Crowd : MonoBehaviour
    {
        static Crowd instance;
        readonly List<Transform> fans = new List<Transform>();
        readonly List<Vector3> homes = new List<Vector3>();
        float cheer, t;

        public static void Cheer(float seconds = 2f)
        {
            if (instance == null) return;
            instance.cheer = Mathf.Max(instance.cheer, seconds);
            Sfx.Play(SfxId.Collect, 0.35f);
        }

        public void Add(Transform fan) { fans.Add(fan); homes.Add(fan.localPosition); }

        void OnEnable() { instance = this; }
        void OnDisable() { if (instance == this) instance = null; }

        void Update()
        {
            float dt = Time.deltaTime;
            t += dt;
            cheer -= dt;
            for (int i = 0; i < fans.Count; i++)
            {
                float ph = i * 1.7f;
                float hop = cheer > 0f ? Mathf.Abs(Mathf.Sin(t * 9f + ph)) * 0.6f : Mathf.Abs(Mathf.Sin(t * 2.2f + ph)) * 0.06f;
                fans[i].localPosition = homes[i] + Vector3.up * hop;
                fans[i].localRotation = Quaternion.Euler(0f, 0f, cheer > 0f ? Mathf.Sin(t * 12f + ph) * 12f : 0f);
            }
        }
    }

    // Whack-a-Snarl: grumpy little Snarl heads pop up out of nine holes. Jump on them before
    // they duck back down. Whack 15 in 30 seconds to win.
    public class WhackASnarl : TimedChallenge
    {
        public readonly List<Transform> Heads = new List<Transform>();
        public readonly List<Vector3> Holes = new List<Vector3>();
        float[] up;
        float[] life;
        float spawnTimer;
        public System.Action OnWin;

        protected override void OnBegin()
        {
            Total = 15;
            up = new float[Heads.Count];
            life = new float[Heads.Count];
            spawnTimer = 0.4f;
            var game = PatchworkGame.Instance;
            if (game != null) game.ShowToast("Jump on the Snarls as they pop up!");
        }

        protected override void OnEnd(bool won)
        {
            for (int i = 0; i < Heads.Count; i++) { life[i] = 0f; }
            if (won)
            {
                Crowd.Cheer(3f);
                Merits.Earn(Merits.Id.WhackChampion);
                ScoreKeeper.Bonus(1000, transform.position + Vector3.up * 2f);
                if (OnWin != null) OnWin();
            }
        }

        protected override void Update()
        {
            base.Update();
            if (up == null) return;
            float dt = Time.deltaTime;
            bool running = Running == this;
            if (running)
            {
                spawnTimer -= dt;
                // Faster as the clock runs down.
                float pace = Mathf.Lerp(0.55f, 0.9f, TimeLeft / Duration);
                if (spawnTimer <= 0f)
                {
                    spawnTimer = pace;
                    int tries = 0;
                    int i = Random.Range(0, Heads.Count);
                    while (life[i] > 0f && tries++ < 9) i = (i + 1) % Heads.Count;
                    if (life[i] <= 0f) { life[i] = Mathf.Lerp(0.9f, 1.5f, TimeLeft / Duration); Sfx.PlayPitched(SfxId.Pop, 0.25f, 1.3f); }
                }
            }
            var game = PatchworkGame.Instance;
            var player = game != null ? game.Player : null;
            for (int i = 0; i < Heads.Count; i++)
            {
                if (life[i] > 0f) life[i] -= dt;
                up[i] = Mathf.MoveTowards(up[i], life[i] > 0f ? 1f : 0f, dt * 7f);
                var h = Heads[i];
                h.localPosition = Holes[i] + Vector3.up * (-0.6f + up[i] * 0.75f);
                h.localRotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * 6f + i) * 15f, 0f);
                if (!running || player == null || up[i] < 0.6f) continue;
                Vector3 hp = h.position + Vector3.up * 0.4f;
                Vector3 pf = player.transform.position;
                if (new Vector2(pf.x - hp.x, pf.z - hp.z).magnitude < 0.85f && pf.y < hp.y + 0.5f && pf.y > hp.y - 0.6f && player.Motor.Velocity.y < -0.5f)
                {
                    life[i] = 0f;
                    up[i] = 0.2f;
                    player.Motor.Bounce(8f);
                    FxBurst.Spawn(FxBurst.Kind.Threads, hp, SnarlKnot.FrayGrey);
                    Sfx.Play(SfxId.Boing, 0.45f);
                    Sfx.PlayPitched(SfxId.Collect, 0.4f, 1f + Got * 0.03f);
                    ScoreKeeper.Bonus(50, hp);
                    Count();
                }
            }
        }
    }

    // The Grumblewool: a huge, grumpy ball of grey Fray yarn with knitting-needle horns. It paws
    // the ground and charges; dodge so it crashes into the ring wall, then jump on its head
    // while it's dizzy. Three hits. Later it leaps (jump the shockwave) and spits fuzzballs.
    public class Grumblewool : MonoBehaviour
    {
        public static Grumblewool Active { get; private set; }
        public const int MaxHp = 3;
        public int Hp { get; private set; } = MaxHp;
        public string Name = "The Grumblewool";
        public Vector3 Center;
        public float Radius = 9.2f;
        public System.Action Defeated, Started, ResetFight;

        enum S { Asleep, Intro, Idle, Telegraph, Charge, Dizzy, Hurt, Leap, Spit, Dead }
        S s = S.Asleep;
        float st;
        Vector3 pos, chargeDir, leapFrom, leapTo;
        Vector3 facing = Vector3.back;
        Transform body, stars;
        Renderer[] eyes;
        Material eyeCalm, eyeAngry;
        int volley;
        readonly List<Shockwave> waves = new List<Shockwave>();
        Vector3 home;

        public static void ResetStatics() { Active = null; }

        public static Grumblewool Create(LevelKit kit, Vector3 center, Vector3 start)
        {
            var go = new GameObject("Grumblewool");
            go.transform.SetParent(kit.Root, false);
            var g = go.AddComponent<Grumblewool>();
            g.Center = center;
            g.home = start;
            g.pos = start;
            go.transform.position = start;
            g.Build();
            return g;
        }

        void Build()
        {
            body = new GameObject("Body").transform;
            body.SetParent(transform, false);
            var dark = PatchworkAssets.Yarn(SnarlKnot.FrayDark);
            var grey = PatchworkAssets.Yarn(SnarlKnot.FrayGrey);
            Monsters.Part(PrimitiveType.Sphere, body, new Vector3(0f, 2f, 0f), new Vector3(4f, 3.6f, 3.8f), dark);
            for (int i = 0; i < 26; i++)
            {
                var q = Quaternion.Euler(i * 23f, i * 47f, 0f);
                Monsters.Part(PrimitiveType.Capsule, body, new Vector3(0f, 2f, 0f) + q * new Vector3(0f, 0f, 1.65f), new Vector3(0.28f, 1.1f, 0.28f), grey, (q * Quaternion.Euler(90f, 0f, 0f)).eulerAngles);
            }
            // A big grumpy face.
            eyeCalm = PatchworkAssets.Glow(SnarlKnot.FrayGlow, 1.4f);
            eyeAngry = PatchworkAssets.Glow(new Color(1f, 0.25f, 0.2f), 2.2f);
            eyes = new Renderer[2];
            for (int k = 0; k < 2; k++)
            {
                float x = k == 0 ? -0.65f : 0.65f;
                Monsters.Part(PrimitiveType.Sphere, body, new Vector3(x, 2.5f, 1.72f), new Vector3(0.9f, 0.75f, 0.35f), PatchworkAssets.Felt(new Color(0.95f, 0.95f, 0.9f)));
                eyes[k] = Monsters.Part(PrimitiveType.Sphere, body, new Vector3(x, 2.48f, 1.88f), new Vector3(0.42f, 0.42f, 0.2f), eyeCalm).GetComponent<Renderer>();
                // Cross brows.
                Monsters.Part(PrimitiveType.Cube, body, new Vector3(x, 3.02f, 1.75f), new Vector3(0.8f, 0.16f, 0.2f), dark, new Vector3(0f, 0f, k == 0 ? -18f : 18f));
                // Knitting-needle horns.
                Monsters.Part(PrimitiveType.Cylinder, body, new Vector3(x * 1.6f, 3.9f, 0.2f), new Vector3(0.14f, 0.9f, 0.14f), PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.92f), 0.85f), new Vector3(10f, 0f, k == 0 ? 25f : -25f));
                Monsters.Part(PrimitiveType.Sphere, body, new Vector3(x * 2.05f, 4.7f, 0.35f), Vector3.one * 0.32f, PatchworkAssets.Plain(new Color(0.95f, 0.35f, 0.4f), 0.6f));
            }
            // A jagged felt mouth with two big teeth.
            Monsters.Part(PrimitiveType.Sphere, body, new Vector3(0f, 1.6f, 1.75f), new Vector3(1.5f, 0.45f, 0.3f), PatchworkAssets.Felt(new Color(0.2f, 0.12f, 0.2f)));
            for (int k = -1; k <= 1; k += 2)
                Monsters.Part(PrimitiveType.Cube, body, new Vector3(k * 0.32f, 1.72f, 1.86f), new Vector3(0.28f, 0.3f, 0.12f), PatchworkAssets.Felt(Color.white));
            // Stubby legs.
            for (int k = 0; k < 4; k++)
                Monsters.Part(PrimitiveType.Capsule, transform, new Vector3((k % 2 == 0 ? -1f : 1f) * 1.1f, 0.3f, (k < 2 ? -1f : 1f) * 0.9f), new Vector3(0.9f, 0.5f, 0.9f), dark);
            // Dizzy stars (hidden until it crashes).
            stars = new GameObject("Stars").transform;
            stars.SetParent(transform, false);
            stars.localPosition = Vector3.up * 3.2f;
            for (int k = 0; k < 4; k++)
            {
                var star = new GameObject("Star").transform;
                star.SetParent(stars, false);
                float a = k * Mathf.PI * 0.5f;
                star.localPosition = new Vector3(Mathf.Cos(a) * 1.6f, 0f, Mathf.Sin(a) * 1.6f);
                for (int p = 0; p < 5; p++)
                    Monsters.Part(PrimitiveType.Sphere, star, Quaternion.Euler(0f, 0f, p * 72f) * new Vector3(0f, 0.16f, 0f), new Vector3(0.14f, 0.36f, 0.1f), PatchworkAssets.Glow(new Color(1f, 0.85f, 0.3f), 1.2f), new Vector3(0f, 0f, p * 72f));
            }
            stars.gameObject.SetActive(false);
        }

        public bool Asleep { get { return s == S.Asleep; } }

        public void Begin()
        {
            if (s != S.Asleep) return;
            Hp = MaxHp;
            Active = this;
            Go(S.Intro);
            Sfx.Play(SfxId.Unravel, 0.8f);
            Sfx.Play(SfxId.PlatformThunk, 0.7f);
            var game = PatchworkGame.Instance;
            if (game != null)
            {
                game.CameraRig.Kick(0.5f);
                game.ShowToast("Dodge its charge, then jump on its head while it's dizzy!");
            }
            Crowd.Cheer(1.5f);
            if (Started != null) Started();
        }

        void Go(S next) { s = next; st = 0f; }

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            st += dt;
            var game = PatchworkGame.Instance;
            var player = game != null ? game.Player : null;

            // Patch unravelled mid-fight: everything resets for another go.
            if (game != null && game.Unravelling && s != S.Asleep && s != S.Dead)
            {
                s = S.Asleep;
                Active = null;
                pos = home;
                Hp = MaxHp;
                foreach (var w in waves) if (w != null) Destroy(w.gameObject);
                waves.Clear();
                stars.gameObject.SetActive(false);
                body.localScale = Vector3.one;
                if (ResetFight != null) ResetFight();
            }

            Vector3 toPlayer = player != null ? player.transform.position - pos : Vector3.forward;
            toPlayer.y = 0f;
            float squashY = 1f;
            bool angryEyes = false;

            switch (s)
            {
                case S.Asleep:
                    // Snoozing: slow breathing.
                    squashY = 1f + Mathf.Sin(Time.time * 1.3f) * 0.04f;
                    facing = Vector3.back;
                    break;
                case S.Intro:
                    squashY = 1f + Mathf.Sin(st * 20f) * 0.08f * Mathf.Clamp01(1.5f - st);
                    Face(toPlayer, dt, 6f);
                    if (st > 1.6f) Go(S.Idle);
                    break;
                case S.Idle:
                    Face(toPlayer, dt, 6f);
                    pos = Vector3.MoveTowards(pos, Center, dt * 2f);
                    if (st > (Hp == MaxHp ? 1.0f : 0.6f))
                    {
                        if (Hp <= 1 && volley < 1) { volley = 3; Go(S.Spit); }
                        else if (Hp <= 2 && Random.value < 0.5f && toPlayer.magnitude > 4f) LeapAt(player);
                        else Go(S.Telegraph);
                    }
                    break;
                case S.Spit:
                    Face(toPlayer, dt, 8f);
                    squashY = 1f + Mathf.Sin(st * 14f) * 0.05f;
                    if (st > 0.7f && player != null)
                    {
                        FuzzBall.Launch(pos + Vector3.up * 2.2f + facing * 2f, player.transform.position + player.Motor.Velocity * 0.4f + Random.insideUnitSphere * 1.2f);
                        Sfx.Play(SfxId.Steam, 0.45f);
                        volley--;
                        st = 0.2f;
                        if (volley <= 0) { volley = -1; Go(S.Telegraph); }
                    }
                    break;
                case S.Telegraph:
                    // Pawing the ground, eyes flashing red.
                    Face(toPlayer, dt, 10f);
                    angryEyes = Mathf.Repeat(st * 6f, 1f) < 0.6f;
                    squashY = 1f - Mathf.Abs(Mathf.Sin(st * 14f)) * 0.08f;
                    if (Mathf.Repeat(st, 0.3f) < dt) Sfx.Step(SfxId.StepWood, 0.4f);
                    if (st > (Hp == 1 ? 0.6f : 0.85f))
                    {
                        chargeDir = facing;
                        Go(S.Charge);
                        Sfx.Play(SfxId.Gust, 0.6f);
                    }
                    break;
                case S.Charge:
                {
                    angryEyes = true;
                    float speed = Hp == 1 ? 15f : 13f;
                    pos += chargeDir * speed * dt;
                    squashY = 1f + Mathf.Sin(st * 30f) * 0.05f;
                    if (Mathf.Repeat(st, 0.18f) < dt) FxBurst.Spawn(FxBurst.Kind.Puff, pos + Vector3.up * 0.3f, new Color(0.85f, 0.75f, 0.6f), -chargeDir);
                    Vector3 off = pos - Center;
                    off.y = 0f;
                    if (off.magnitude > Radius - 2.2f)
                    {
                        pos = Center + off.normalized * (Radius - 2.2f);
                        Go(S.Dizzy);
                        Sfx.Play(SfxId.PlatformThunk, 0.9f);
                        Sfx.Play(SfxId.Boing, 0.6f);
                        if (game != null) { game.CameraRig.Kick(0.45f); Haptics.Pulse(0.5f, 0.5f, 0.2f); }
                        FxBurst.Spawn(FxBurst.Kind.Sparkle, pos + Vector3.up * 3f, new Color(1f, 0.9f, 0.4f));
                        Crowd.Cheer(1f);
                    }
                    break;
                }
                case S.Dizzy:
                {
                    float dizzyTime = Hp == 1 ? 2.4f : 3f;
                    squashY = Mathf.Lerp(1f, 0.55f, Mathf.Clamp01(st / 0.25f));
                    stars.gameObject.SetActive(true);
                    stars.localPosition = Vector3.up * (3.8f * squashY + 0.3f);
                    stars.Rotate(0f, 240f * dt, 0f, Space.Self);
                    body.localRotation = Quaternion.Euler(Mathf.Sin(st * 5f) * 6f, 0f, Mathf.Sin(st * 4f) * 8f);
                    if (st > dizzyTime) { stars.gameObject.SetActive(false); body.localRotation = Quaternion.identity; Go(S.Idle); }
                    break;
                }
                case S.Hurt:
                    squashY = 0.55f + Mathf.Clamp01(st / 0.6f) * 0.45f;
                    body.localRotation = Quaternion.Euler(0f, st * 720f * Mathf.Clamp01(1f - st), 0f);
                    if (st > 1.1f) { body.localRotation = Quaternion.identity; Go(Hp <= 0 ? S.Dead : S.Idle); if (Hp <= 0) Die(); }
                    break;
                case S.Leap:
                {
                    float k = Mathf.Clamp01(st / 1.0f);
                    pos = Vector3.Lerp(leapFrom, leapTo, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 6f;
                    squashY = 1f + Mathf.Sin(k * Mathf.PI) * 0.15f;
                    if (k >= 1f)
                    {
                        pos = leapTo;
                        var w = Shockwave.Spawn(new Vector3(pos.x, Center.y, pos.z), Radius + 1f);
                        waves.Add(w);
                        Sfx.Play(SfxId.PlatformThunk, 1f);
                        if (game != null) { game.CameraRig.Kick(0.6f); Haptics.Pulse(0.7f, 0.6f, 0.25f); }
                        FxBurst.Spawn(FxBurst.Kind.Puff, pos + Vector3.up * 0.3f, new Color(0.85f, 0.75f, 0.6f));
                        Go(S.Telegraph);
                        st = -0.4f;
                    }
                    break;
                }
                case S.Dead:
                    break;
            }

            if (s == S.Dead) return;
            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(new Vector3(facing.x, 0f, facing.z).normalized, Vector3.up);
            float wide = 1f + (1f - squashY) * 0.5f;
            body.localScale = new Vector3(wide, squashY, wide);
            for (int k = 0; k < 2; k++) eyes[k].sharedMaterial = angryEyes ? eyeAngry : eyeCalm;

            if (player == null || s == S.Asleep || s == S.Hurt) return;
            Vector3 pf = player.transform.position;
            Vector3 d = pf - pos;
            float flat = new Vector2(d.x, d.z).magnitude;
            float top = 3.8f * squashY;
            if (s == S.Dizzy)
            {
                // Landing on its head while it's dizzy.
                if (flat < 2.1f && d.y > top - 0.8f && d.y < top + 1f && player.Motor.Velocity.y < 0f) TakeHit(player);
                else if (flat < 1.9f && d.y < top - 0.8f) Shove(player);
                return;
            }
            if (flat < 2.3f && d.y < top) game.Hurt(pos + Vector3.up * 1.5f);
        }

        void Face(Vector3 dir, float dt, float speed)
        {
            if (dir.sqrMagnitude < 0.01f) return;
            facing = Vector3.Slerp(facing, dir.normalized, 1f - Mathf.Exp(-speed * dt));
        }

        void LeapAt(PatchworkPlayer player)
        {
            leapFrom = pos;
            Vector3 target = player != null ? player.transform.position : Center;
            Vector3 off = target - Center;
            off.y = 0f;
            if (off.magnitude > Radius - 2.5f) off = off.normalized * (Radius - 2.5f);
            leapTo = new Vector3(Center.x + off.x, pos.y, Center.z + off.z);
            Go(S.Leap);
            Sfx.Play(SfxId.Jump, 0.8f);
        }

        // Pushes Patch gently off its sides while it's dizzy (so he doesn't sink into it).
        void Shove(PatchworkPlayer player)
        {
            Vector3 away = player.transform.position - pos;
            away.y = 0f;
            if (away.sqrMagnitude < 0.01f) away = Vector3.forward;
            away.Normalize();
            player.Motor.Velocity = new Vector3(away.x * 4f, Mathf.Max(player.Motor.Velocity.y, 0f), away.z * 4f);
        }

        void TakeHit(PatchworkPlayer player)
        {
            Hp--;
            Go(S.Hurt);
            stars.gameObject.SetActive(false);
            player.Motor.Bounce(15f);
            var game = player.Game;
            if (game != null) { game.HitStop(0.12f); game.CameraRig.Kick(0.6f); }
            Haptics.Pulse(0.8f, 0.6f, 0.25f);
            Sfx.Play(SfxId.Unravel, 0.9f);
            Sfx.Play(SfxId.Boing, 0.7f);
            FxBurst.Spawn(FxBurst.Kind.Threads, pos + Vector3.up * 2.5f, SnarlKnot.FrayGrey);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, pos + Vector3.up * 3f, new Color(1f, 0.85f, 0.5f));
            ScoreKeeper.Bonus(500, pos + Vector3.up * 4f);
            Crowd.Cheer(2.5f);
            if (game != null && Hp > 0) game.ShowToast(Hp == 2 ? "It's furious! Watch out for its big jump!" : "One more! Now it's spitting fuzzballs!");
        }

        void Die()
        {
            s = S.Dead;
            Active = null;
            foreach (var w in waves) if (w != null) Destroy(w.gameObject);
            waves.Clear();
            Vector3 c = pos + Vector3.up * 2f;
            for (int i = 0; i < 4; i++) FxBurst.Spawn(FxBurst.Kind.Threads, c + Random.insideUnitSphere * 1.5f, SnarlKnot.FrayGrey);
            FxBurst.Spawn(FxBurst.Kind.Scraps, c, new Color(1f, 0.6f, 0.7f));
            FxBurst.Spawn(FxBurst.Kind.Scraps, c, new Color(0.5f, 0.8f, 1f));
            FxBurst.Spawn(FxBurst.Kind.Sparkle, c, new Color(1f, 0.9f, 0.5f));
            Sfx.Play(SfxId.Complete, 0.8f);
            ScoreKeeper.Bonus(3000, c + Vector3.up * 2f);
            Crowd.Cheer(5f);
            Merits.Earn(Merits.Id.Ringmaster);
            var game = PatchworkGame.Instance;
            if (game != null) game.ShowToast("The Grumblewool came all undone! The crowd goes wild!");
            if (Defeated != null) Defeated();
            gameObject.SetActive(false);
        }

        void OnDisable() { if (Active == this) Active = null; }
    }

    // A ring of dust that spreads out across the floor from the Grumblewool's big jump. Jump
    // over it!
    public class Shockwave : MonoBehaviour
    {
        float radius, maxRadius, t;
        readonly List<Transform> puffs = new List<Transform>();

        public static Shockwave Spawn(Vector3 at, float maxRadius)
        {
            var go = new GameObject("Shockwave");
            go.transform.position = at;
            var w = go.AddComponent<Shockwave>();
            w.maxRadius = maxRadius;
            var m = PatchworkAssets.Felt(new Color(0.88f, 0.8f, 0.66f));
            for (int i = 0; i < 28; i++)
                w.puffs.Add(Monsters.Part(PrimitiveType.Sphere, go.transform, Vector3.zero, new Vector3(0.7f, 0.5f, 0.7f), m));
            return w;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            t += dt;
            radius += dt * 7f;
            for (int i = 0; i < puffs.Count; i++)
            {
                float a = i * Mathf.PI * 2f / puffs.Count;
                puffs[i].localPosition = new Vector3(Mathf.Cos(a) * radius, 0.25f + Mathf.Sin(t * 20f + i) * 0.05f, Mathf.Sin(a) * radius);
            }
            var game = PatchworkGame.Instance;
            if (game != null && game.Player != null)
            {
                Vector3 p = game.Player.transform.position - transform.position;
                float d = new Vector2(p.x, p.z).magnitude;
                if (Mathf.Abs(d - radius) < 0.6f && p.y < 0.55f) game.Hurt(game.Player.transform.position - new Vector3(p.x, 0f, p.z).normalized);
            }
            if (radius > maxRadius) Destroy(gameObject);
        }
    }
}

namespace Patchwork
{
    // Calls back when Patch steps into it (once until he leaves).
    public class TouchZone : PlayerVolume
    {
        public System.Action<PatchworkPlayer> Entered;
        protected override void OnPlayerEnter(PatchworkPlayer player) { if (Entered != null) Entered(player); }
    }
}

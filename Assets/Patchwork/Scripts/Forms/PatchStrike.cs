using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Patch's strikes. Press Attack (Circle / left click) for a hit; press again in time and
    // the next one follows, three in a row, and the third is the big one. Every form hits its
    // own way:
    //   Yarn:  a lash of the yarn arm, a backhand with the other, then a knot-hammer slam.
    //   Water: a wet slap, a squirt that reaches a long way, then a belly-flop splash all round.
    //   Wind:  a puff that shoves, a whirl that hits all round, then a gale burst straight through.
    // Small Fray pop in one hit. Bigger ones take several, and some can only be hurt by the
    // third hit (it knocks a shield aside, or tips a thing in armour over).
    public class PatchStrike
    {
        public const int Moves = 3;
        static readonly float[] Duration = { 0.26f, 0.28f, 0.5f };
        static readonly float[] HitAt = { 0.1f, 0.11f, 0.3f };
        const float ChainWindow = 0.55f;

        readonly PatchworkPlayer player;
        readonly List<FrayCreature> struck = new List<FrayCreature>();
        readonly List<FrayCreature> scratch = new List<FrayCreature>();
        int next;
        float t, chain, rest;
        bool landed, queued;
        FormType moveForm;
        Vector3 aim = Vector3.forward;

        // Which hit of the three is under way (0, 1, 2), or -1 for none.
        public int Move { get; private set; }
        public bool Active { get { return Move >= 0; } }
        public float Progress { get { return Move >= 0 ? Mathf.Clamp01(t / Duration[Move]) : 0f; } }
        // The hit that the next press will throw (for the HUD).
        public int NextMove { get { return next; } }

        public PatchStrike(PatchworkPlayer p)
        {
            player = p;
            Move = -1;
        }

        public void Cancel()
        {
            Move = -1;
            next = 0;
            queued = false;
            chain = 0f;
            if (player.Visuals != null) player.Visuals.SetStrike(-1, 0f);
        }

        bool CanStrike()
        {
            var motor = player.Motor;
            if (motor.ExternalControl) return false;
            if (player.Hand != null && player.Hand.Held != null) return false;
            var yarn = player.Forms.Ability as YarnAbility;
            if (yarn != null && yarn.Attached) return false;
            return true;
        }

        public void Tick(PlayerInputFrame input, float dt)
        {
            if (rest > 0f) rest -= dt;
            if (!Active)
            {
                if (chain > 0f) { chain -= dt; if (chain <= 0f) next = 0; }
                if (input.AttackPressed && rest <= 0f && CanStrike()) Begin(input);
                return;
            }

            // Changing form (or being grabbed by something) stops a hit half way.
            if (player.Forms.Current != moveForm || player.Motor.ExternalControl) { Cancel(); return; }

            t += dt;
            float dur = Duration[Move];
            if (input.AttackPressed && t > dur * 0.3f) queued = true;
            if (t < dur * 0.7f) player.Motor.FacingOverride = aim;
            player.Visuals.SetStrike(Move, Mathf.Clamp01(t / dur));

            if (!landed && t >= HitAt[Move]) { landed = true; Land(); }
            // The gale burst hits whatever it passes through, all the way along.
            if (landed && moveForm == FormType.Wind && Move == 2 && player.Motor.IsDashing) Sweep(2.1f, -1f, 2, true);

            if (t >= dur)
            {
                bool last = Move == Moves - 1;
                Move = -1;
                player.Visuals.SetStrike(-1, 0f);
                if (last) { next = 0; chain = 0f; rest = 0.22f; queued = false; }
                else
                {
                    chain = ChainWindow;
                    if (queued && CanStrike()) Begin(input);
                }
            }
        }

        void Begin(PlayerInputFrame input)
        {
            Move = next;
            next = (next + 1) % Moves;
            moveForm = player.Forms.Current;
            t = 0f;
            landed = false;
            queued = false;
            chain = 0f;
            struck.Clear();

            var motor = player.Motor;
            // Which way: at the nearest Fray if there's one close and roughly ahead, else where
            // the stick's pointing, else straight ahead.
            Vector3 fwd = player.transform.forward;
            if (input.MoveWorld.sqrMagnitude > 0.04f) fwd = input.MoveWorld.normalized;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
            fwd.Normalize();
            aim = fwd;
            float best = 6.5f;
            foreach (var c in FrayCreature.All)
            {
                if (c == null || c.Center == null) continue;
                Vector3 d = c.Center() - motor.Center;
                if (Mathf.Abs(d.y) > 3f) continue;
                d.y = 0f;
                float dist = d.magnitude - c.Radius;
                if (dist > best || d.sqrMagnitude < 0.0001f) continue;
                if (Vector3.Dot(d.normalized, fwd) < -0.2f && dist > 2f) continue;
                best = dist;
                aim = d.normalized;
            }
            motor.FacingOverride = aim;

            bool wet = motor.Swimming || motor.Submerged;
            float pitch = Move == 0 ? 1f : Move == 1 ? 1.18f : 0.82f;
            switch (moveForm)
            {
                case FormType.Yarn:
                    Sfx.PlayPitched(SfxId.Thread, 0.4f, pitch * 1.2f);
                    if (Move == 2 && motor.Grounded && !wet) motor.Bounce(5.5f);
                    break;
                case FormType.Water:
                    Sfx.PlayPitched(SfxId.Splash, 0.3f, pitch * 1.35f);
                    if (Move == 2 && motor.Grounded && !wet) motor.Bounce(6.5f);
                    break;
                default:
                    Sfx.PlayPitched(SfxId.Gust, 0.35f, pitch * 1.3f);
                    break;
            }
            if (Move == 2) Sfx.Voice(SfxId.VoiceHup, moveForm, 0.35f);
            Haptics.Pulse(0.1f, 0.2f, 0.06f);
            player.Visuals.SetStrike(Move, 0f);
        }

        // The moment the hit lands: work out what's in the way, and show it.
        void Land()
        {
            var motor = player.Motor;
            Vector3 c = motor.Center;
            Color col = FormLibrary.Get(moveForm).PrimaryColor;
            Color pale = Color.Lerp(col, Color.white, 0.5f);
            bool wet = motor.Swimming || motor.Submerged;
            int hits = 0;

            switch (moveForm)
            {
                case FormType.Yarn:
                    if (Move == 0)
                    {
                        hits = Sweep(2.9f, 0.1f, 1, false);
                        StrikeFx.Arc(c + Vector3.up * 0.15f, aim, 100f, -70f, 2.5f, col, 0.2f);
                    }
                    else if (Move == 1)
                    {
                        hits = Sweep(2.9f, 0.1f, 1, false);
                        StrikeFx.Arc(c + Vector3.up * 0.3f, aim, -100f, 70f, 2.5f, col, 0.2f);
                    }
                    else
                    {
                        // The knot hammer: both arms knotted into one ball and brought down in front.
                        Vector3 spot = c + aim * 1.5f;
                        hits = Burst(spot, 2.3f, 2, true);
                        StrikeFx.Ring(spot + Vector3.down * 0.45f, 2.2f, col, 0.24f);
                        FxBurst.Spawn(FxBurst.Kind.Pompoms, spot + Vector3.down * 0.5f, col);
                        FxBurst.Spawn(FxBurst.Kind.Threads, spot, pale);
                        Sfx.Play(SfxId.PlatformThunk, 0.45f);
                        Shake(0.22f);
                    }
                    break;

                case FormType.Water:
                    if (Move == 0)
                    {
                        hits = Sweep(2.3f, -0.05f, 1, false);
                        StrikeFx.Arc(c, aim, 95f, -65f, 1.9f, pale, 0.22f);
                        FxBurst.Spawn(FxBurst.Kind.Splash, c + aim * 1.2f, pale);
                    }
                    else if (Move == 1)
                    {
                        // The squirt: a thin jet, a long way.
                        hits = Jet(6.5f, 1.1f, 1);
                        StrikeFx.Line(c + aim * 0.5f + Vector3.up * 0.1f, aim, 6f, pale, 0.24f);
                        FxBurst.Spawn(FxBurst.Kind.Drips, c + aim * 3f, pale);
                    }
                    else
                    {
                        hits = Burst(c, 3.5f, 2, true);
                        StrikeFx.Ring(c + Vector3.down * 0.5f, 3.4f, pale, 0.26f);
                        FxBurst.Spawn(FxBurst.Kind.Splash, c + Vector3.down * 0.4f, pale);
                        Sfx.Play(SfxId.Splash, 0.6f);
                        Shake(0.22f);
                    }
                    break;

                default:
                    if (Move == 0)
                    {
                        hits = Sweep(3.4f, 0.25f, 1, false);
                        FxBurst.Spawn(FxBurst.Kind.Puff, c + aim * 0.8f, Color.white, aim);
                        StrikeFx.Line(c + aim * 0.6f, aim, 2.8f, Color.white, 0.5f);
                    }
                    else if (Move == 1)
                    {
                        hits = Burst(c, 2.9f, 1, false);
                        StrikeFx.Ring(c, 2.7f, Color.white, 0.24f);
                        FxBurst.Spawn(FxBurst.Kind.Puff, c, Color.white);
                    }
                    else
                    {
                        // The gale burst: he goes straight through whatever's in front of him.
                        motor.StartDash(aim * 11.5f, 0.2f);
                        hits = Sweep(2.4f, -1f, 2, true);
                        FxBurst.Spawn(FxBurst.Kind.Puff, c, Color.white, -aim);
                        StrikeFx.Line(c, aim, 3.4f, Color.white, 0.6f);
                        Sfx.Play(SfxId.Whoosh, 0.4f);
                        Shake(0.16f);
                    }
                    break;
            }
            if (wet) FxBurst.Spawn(FxBurst.Kind.Sparkle, c + aim, pale);
            if (hits > 0) Connected();
        }

        void Shake(float amount)
        {
            var game = player.Game;
            if (game != null && game.CameraRig != null) game.CameraRig.Kick(amount);
            player.Visuals.PunchScale(new Vector3(1.25f, 0.78f, 1.25f));
        }

        void Connected()
        {
            var game = player.Game;
            if (game != null) game.HitStop(Move == 2 ? 0.07f : 0.035f);
            Haptics.Pulse(Move == 2 ? 0.6f : 0.3f, 0.3f, 0.1f);
            Merits.Add("strikes");
        }

        // Everything within "reach" and ahead (how far ahead: "cone" is the least dot product
        // with the aim; -1 for all round). Under water it all ripples out in a ball instead.
        int Sweep(float reach, float cone, int power, bool finisher)
        {
            var motor = player.Motor;
            bool wet = motor.Swimming || motor.Submerged;
            Vector3 o = motor.Center;
            int n = 0;
            scratch.Clear();
            scratch.AddRange(FrayCreature.All);
            foreach (var c in scratch)
            {
                if (c == null || c.Center == null || struck.Contains(c)) continue;
                Vector3 d = c.Center() - o;
                if (wet)
                {
                    if (d.magnitude - c.Radius > reach) continue;
                }
                else
                {
                    if (Mathf.Abs(d.y) > 1.7f + c.Radius) continue;
                    d.y = 0f;
                    float dist = d.magnitude;
                    if (dist - c.Radius > reach) continue;
                    if (cone > -1f && dist > c.Radius + 0.5f && Vector3.Dot(d / Mathf.Max(0.001f, dist), aim) < cone) continue;
                }
                struck.Add(c);
                if (c.TakeHit(player, power, o, finisher)) n++;
            }
            return n;
        }

        // Everything within a ball round a point.
        int Burst(Vector3 at, float radius, int power, bool finisher)
        {
            int n = 0;
            scratch.Clear();
            scratch.AddRange(FrayCreature.All);
            foreach (var c in scratch)
            {
                if (c == null || c.Center == null || struck.Contains(c)) continue;
                Vector3 d = c.Center() - at;
                if (Mathf.Abs(d.y) > 2f + c.Radius && d.magnitude - c.Radius > radius) continue;
                d.y *= 0.6f;
                if (d.magnitude - c.Radius > radius) continue;
                struck.Add(c);
                if (c.TakeHit(player, power, player.Motor.Center, finisher)) n++;
            }
            return n;
        }

        // Everything along a straight line ahead.
        int Jet(float length, float width, int power)
        {
            Vector3 o = player.Motor.Center;
            int n = 0;
            scratch.Clear();
            scratch.AddRange(FrayCreature.All);
            foreach (var c in scratch)
            {
                if (c == null || c.Center == null || struck.Contains(c)) continue;
                Vector3 d = c.Center() - o;
                float along = Vector3.Dot(d, aim);
                if (along < -0.3f || along - c.Radius > length) continue;
                Vector3 off = d - aim * along;
                off.y *= 0.5f;
                if (off.magnitude > width + c.Radius) continue;
                struck.Add(c);
                if (c.TakeHit(player, power, o, false)) n++;
            }
            return n;
        }
    }

    // The swish a strike leaves in the air: a few glowing beads along an arc, a ring or a
    // line, that shrink away in a blink.
    public class StrikeFx : MonoBehaviour
    {
        static readonly Dictionary<int, Material> mats = new Dictionary<int, Material>();
        Transform[] bits;
        Vector3[] drift;
        float life, age, size;

        public static void ResetStatics() { mats.Clear(); }

        static Material Mat(Color c)
        {
            int key = Mathf.RoundToInt(c.r * 15f) * 256 + Mathf.RoundToInt(c.g * 15f) * 16 + Mathf.RoundToInt(c.b * 15f);
            Material m;
            if (!mats.TryGetValue(key, out m) || m == null)
            {
                m = PatchworkAssets.Glow(Color.Lerp(c, Color.white, 0.25f), 1.5f);
                mats[key] = m;
            }
            return m;
        }

        static StrikeFx Make(Vector3 at, int count, Color col, float size, float life)
        {
            var go = new GameObject("StrikeFx");
            go.transform.position = at;
            var fx = go.AddComponent<StrikeFx>();
            fx.bits = new Transform[count];
            fx.drift = new Vector3[count];
            fx.life = life;
            fx.size = size;
            var m = Mat(col);
            for (int i = 0; i < count; i++)
            {
                fx.bits[i] = Monsters.Part(PrimitiveType.Sphere, go.transform, Vector3.zero, Vector3.one * size, m);
                fx.bits[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            return fx;
        }

        // A sweep from one angle to another (degrees, to the right of "forward" is positive).
        public static void Arc(Vector3 center, Vector3 forward, float fromDeg, float toDeg, float radius, Color col, float size)
        {
            const int N = 9;
            var fx = Make(center, N, col, size, 0.24f);
            for (int i = 0; i < N; i++)
            {
                float k = i / (float)(N - 1);
                Vector3 d = Quaternion.Euler(0f, Mathf.Lerp(fromDeg, toDeg, k), 0f) * forward;
                fx.bits[i].localPosition = d * radius;
                // Thin at the start of the sweep, fat at the end, like the mark of a brush.
                fx.bits[i].localScale = Vector3.one * size * (0.4f + k * 0.9f);
                fx.drift[i] = d * 1.5f;
            }
        }

        public static void Ring(Vector3 center, float radius, Color col, float size)
        {
            const int N = 14;
            var fx = Make(center, N, col, size, 0.3f);
            for (int i = 0; i < N; i++)
            {
                float a = i * Mathf.PI * 2f / N;
                Vector3 d = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                fx.bits[i].localPosition = d * radius * 0.45f;
                fx.drift[i] = d * radius * 2f;
            }
        }

        public static void Line(Vector3 from, Vector3 dir, float length, Color col, float size)
        {
            const int N = 8;
            var fx = Make(from, N, col, size, 0.26f);
            for (int i = 0; i < N; i++)
            {
                float k = i / (float)(N - 1);
                fx.bits[i].localPosition = dir * length * k * 0.6f;
                fx.bits[i].localScale = Vector3.one * size * (1f - k * 0.5f);
                fx.drift[i] = dir * length * (0.6f + k) * 1.6f;
            }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            age += dt;
            float k = Mathf.Clamp01(age / life);
            for (int i = 0; i < bits.Length; i++)
            {
                bits[i].localPosition += drift[i] * dt * (1f - k);
                bits[i].localScale = Vector3.Lerp(bits[i].localScale, Vector3.zero, 1f - Mathf.Exp(-9f * dt * (0.4f + k)));
            }
            if (age >= life) Destroy(gameObject);
        }
    }
}

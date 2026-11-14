using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The Fray in the Oceanic Museum. It has no shapes of its own here: it climbs inside the
    // exhibits and wears them. Suits of armour that step down off their plinths (the Hollow
    // Guard), grey jellyfish and knotted clots in the tanks, chessmen that hop, and a brass
    // diving helmet that has grown legs and run off with the eyepiece.
    //
    // Unlike the little Fray of Toybox Town, most of these take more than one hit (see
    // PatchStrike), and some shrug off everything but the third hit of the three.
    public abstract class MuseumFoe : MonoBehaviour
    {
        public System.Action<MuseumFoe> Beaten;
        public bool IsBeaten { get; protected set; }
        public int MaxHealth = 3;
        public int Health { get; protected set; }
        public int Score = 200;

        protected Vector3 pos, knock;
        protected float t, stun, flinch, touchRest;
        protected float centreY = 1f;
        protected float knockScale = 1f;
        protected Transform rig;
        protected FrayCreature creature;

        public Vector3 Center { get { return pos + Vector3.up * centreY; } }
        public Vector3 Feet { get { return pos; } }

        protected void Register(float radius)
        {
            Health = MaxHealth;
            creature = gameObject.AddComponent<FrayCreature>();
            creature.Radius = radius;
            creature.Center = () => Center;
            creature.Struck = OnStruck;
            creature.Pop = p => OnStruck(p, 1, p != null ? p.Motor.Center : Center, false);
        }

        protected virtual bool Blocks(Vector3 from, bool finisher) { return false; }
        protected virtual void OnBlocked(bool finisher) { }
        protected virtual void OnHurt(bool finisher) { }
        protected virtual void OnBeaten() { }

        bool OnStruck(PatchworkPlayer player, int power, Vector3 from, bool finisher)
        {
            if (IsBeaten) return false;
            if (Blocks(from, finisher))
            {
                // Clang! Off the shield (or the brass).
                Sfx.PlayNear(SfxId.Clack, Center, 0.5f, 30f);
                Sfx.PlayNear(SfxId.PadClick, Center, 0.4f, 30f);
                FxBurst.Spawn(FxBurst.Kind.Sparkle, Vector3.Lerp(from, Center, 0.7f), new Color(1f, 0.95f, 0.7f));
                flinch = 0.5f;
                OnBlocked(finisher);
                return false;
            }
            Health -= Mathf.Max(1, power);
            flinch = 1f;
            stun = Mathf.Max(stun, finisher ? 0.9f : 0.3f);
            Vector3 away = Center - from;
            away.y = 0f;
            if (away.sqrMagnitude < 0.001f) away = transform.forward;
            knock = away.normalized * (finisher ? 8f : 4.5f) * knockScale;
            FxBurst.Spawn(FxBurst.Kind.Threads, Center, SnarlKnot.FrayGrey);
            if (Health <= 0) { Fall(); return true; }
            Sfx.PlayNear(SfxId.Pop, Center, 0.45f, 30f);
            Sfx.PlayPitched(SfxId.Boing, 0.3f, 1.3f);
            OnHurt(finisher);
            return true;
        }

        void Fall()
        {
            IsBeaten = true;
            Vector3 c = Center;
            Monsters.PopFx(c);
            ScoreKeeper.Bonus(Score, c);
            ScoreBubbles.BurstAt(c, 5);
            Monsters.Popped(c);
            OnBeaten();
            gameObject.SetActive(false);
            if (Beaten != null) Beaten(this);
        }

        // Put back as it was (the fight's starting again).
        public virtual void Restore(Vector3 at)
        {
            IsBeaten = false;
            Health = MaxHealth;
            pos = at;
            knock = Vector3.zero;
            stun = 0f;
            flinch = 0f;
            transform.position = at;
        }

        // Every frame: time, the flinch when it's hit, and being knocked back. False while it's
        // too dazed to do anything.
        protected bool Common(float dt)
        {
            t += dt;
            touchRest -= dt;
            flinch = Mathf.MoveTowards(flinch, 0f, dt * 3.5f);
            if (knock.sqrMagnitude > 0.01f)
            {
                pos += knock * dt;
                knock = Vector3.Lerp(knock, Vector3.zero, 1f - Mathf.Exp(-6f * dt));
            }
            if (rig != null) rig.localScale = Vector3.one * (1f + Mathf.Sin(flinch * Mathf.PI * 3f) * 0.12f * flinch);
            if (stun > 0f) { stun -= dt; return false; }
            return true;
        }

        // Patch walking into it. Landing on it (or crashing through it) counts as a hit;
        // otherwise, if it's the kind of thing that stings, he loses a heart.
        protected void Contact(PatchworkPlayer player, float radius, float height, bool stings)
        {
            if (player == null || touchRest > 0f) return;
            Vector3 d = player.Motor.Center - Center;
            if (new Vector2(d.x, d.z).magnitude > radius || Mathf.Abs(d.y) > height) return;
            if (SnarlKnot.HitHard(player, Center))
            {
                touchRest = 0.45f;
                if (player.Motor.Velocity.y < 0f) player.Motor.Bounce(9.5f);
                creature.TakeHit(player, 1, player.Motor.Center + Vector3.up, false);
            }
            else if (stings)
            {
                touchRest = 0.8f;
                if (player.Game != null) player.Game.Hurt(Center);
            }
        }

        protected static PatchworkPlayer ThePlayer()
        {
            var game = PatchworkGame.Instance;
            return game != null && game.Mode == UIMode.Gameplay ? game.Player : null;
        }

        protected static Transform Pivot(Transform parent, string name, Vector3 at)
        {
            var p = new GameObject(name).transform;
            p.SetParent(parent, false);
            p.localPosition = at;
            return p;
        }

        protected static Transform P(PrimitiveType type, Transform parent, Vector3 p, Vector3 s, Material m, Vector3 e = default(Vector3))
        {
            return Monsters.Part(type, parent, p, s, m, e);
        }

        // A tuft of loose grey thread poking out of something it shouldn't be in.
        protected static void Tuft(Transform parent, Vector3 at, float size, float seed)
        {
            for (int i = 0; i < 3; i++)
                P(PrimitiveType.Capsule, parent, at, new Vector3(0.05f, 0.16f + i * 0.04f, 0.05f) * size, i == 1 ? Monsters.Dark : Monsters.Grey,
                    new Vector3(Mathf.Sin(seed + i * 2.1f) * 50f, seed * 40f + i * 120f, Mathf.Cos(seed + i * 1.3f) * 50f));
        }
    }

    // ------------------------------------------------------------------ The Hollow Guard

    // A suit of armour off its plinth, with nobody in it but the Fray: grey threads at every
    // joint, two violet lights in the dark of the helmet, and strings going up from its
    // shoulders into nothing, like a puppet's. Each one fights the way its armour was made to.
    //   Knight, Legionary, Viking, Hoplite: behind a shield. Only the third hit knocks it aside
    //     (or get round the back, or land on its head, or hit it while it's swinging).
    //   Samurai: a fast dash straight through you.  Eagle warrior: a great leap down on you.
    //   Steppe archer: keeps away and looses sucker arrows.  Terracotta: slow, and very hard.
    public class HollowGuard : MuseumFoe
    {
        public enum Suit { Knight, Hoplite, Legionary, Viking, Samurai, Terracotta, Mongol, Eagle }
        enum Style { Blade, Spear, Archer, Leaper, Dasher, Heavy }
        enum State { Wake, Stalk, Windup, Swing, Recover }

        static readonly List<HollowGuard> live = new List<HollowGuard>();
        static float lastSwing;

        public Suit Kind { get; private set; }
        public bool Captain { get; private set; }
        // Keeps it inside the hall and out of the furniture (set by the fight).
        public System.Func<Vector3, Vector3> Fence;
        public Vector3 PlinthTop { get; private set; }

        Style style;
        State state;
        bool shield, struckOnce, strafeLeft;
        float st, cooldown, shieldDown, size = 1f, floorY, walk, moved;
        Vector3 facing = Vector3.forward, from, to;
        Transform legL, legR, armL, armR, head, eyes, strings;

        public static HollowGuard Create(Transform parent, Vector3 plinthTop, float yaw, Suit kind, bool captain)
        {
            var go = new GameObject("Hollow" + kind);
            go.transform.SetParent(parent, false);
            var g = go.AddComponent<HollowGuard>();
            g.Kind = kind;
            g.Captain = captain;
            g.PlinthTop = plinthTop;
            g.pos = plinthTop;
            g.facing = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            g.size = captain ? 1.3f : 1f;
            switch (kind)
            {
                case Suit.Hoplite: g.style = Style.Spear; g.shield = true; g.MaxHealth = 3; break;
                case Suit.Legionary: g.style = Style.Blade; g.shield = true; g.MaxHealth = 3; break;
                case Suit.Viking: g.style = Style.Blade; g.shield = true; g.MaxHealth = 4; break;
                case Suit.Samurai: g.style = Style.Dasher; g.MaxHealth = 4; break;
                case Suit.Terracotta: g.style = Style.Heavy; g.MaxHealth = 6; g.knockScale = 0.25f; break;
                case Suit.Mongol: g.style = Style.Archer; g.MaxHealth = 2; break;
                case Suit.Eagle: g.style = Style.Leaper; g.MaxHealth = 3; break;
                default: g.style = Style.Blade; g.shield = true; g.MaxHealth = captain ? 9 : 4; break;
            }
            if (captain) g.knockScale = 0.4f;
            g.Score = captain ? 1000 : 300;
            g.centreY = 1.45f * g.size;
            g.Build();
            g.Register(0.75f * g.size);
            go.transform.position = plinthTop;
            go.transform.rotation = Quaternion.LookRotation(g.facing, Vector3.up);
            go.SetActive(false);
            return g;
        }

        void OnEnable() { if (!live.Contains(this)) live.Add(this); }
        void OnDisable() { live.Remove(this); }

        // Steps down off the plinth to a spot on the floor.
        public void Wake(Vector3 landing)
        {
            Restore(PlinthTop);
            gameObject.SetActive(true);
            floorY = landing.y;
            from = PlinthTop;
            to = landing;
            state = State.Wake;
            st = 0f;
            shieldDown = 0f;
            cooldown = Random.Range(0.6f, 1.6f);
            strafeLeft = Random.value < 0.5f;
            FxBurst.Spawn(FxBurst.Kind.Threads, PlinthTop + Vector3.up * 1.4f, SnarlKnot.FrayGrey);
            Sfx.PlayNear(SfxId.Lever, PlinthTop, 0.5f, 40f);
        }

        public void Sleep()
        {
            IsBeaten = true;
            gameObject.SetActive(false);
        }

        void Build()
        {
            Color main, trim, cloth;
            switch (Kind)
            {
                case Suit.Hoplite: main = new Color(0.8f, 0.6f, 0.3f); trim = new Color(0.75f, 0.2f, 0.2f); cloth = new Color(0.96f, 0.91f, 0.8f); break;
                case Suit.Legionary: main = new Color(0.72f, 0.75f, 0.8f); trim = new Color(0.75f, 0.15f, 0.15f); cloth = new Color(0.7f, 0.15f, 0.15f); break;
                case Suit.Viking: main = new Color(0.5f, 0.52f, 0.56f); trim = new Color(0.55f, 0.4f, 0.25f); cloth = new Color(0.3f, 0.4f, 0.5f); break;
                case Suit.Samurai: main = new Color(0.55f, 0.1f, 0.12f); trim = new Color(1f, 0.8f, 0.35f); cloth = new Color(0.13f, 0.16f, 0.36f); break;
                case Suit.Terracotta: main = new Color(0.7f, 0.52f, 0.4f); trim = new Color(0.6f, 0.44f, 0.34f); cloth = new Color(0.66f, 0.5f, 0.38f); break;
                case Suit.Mongol: main = new Color(0.4f, 0.3f, 0.25f); trim = new Color(0.85f, 0.65f, 0.3f); cloth = new Color(0.25f, 0.4f, 0.6f); break;
                case Suit.Eagle: main = new Color(0.9f, 0.85f, 0.7f); trim = new Color(0.2f, 0.65f, 0.55f); cloth = new Color(0.85f, 0.3f, 0.2f); break;
                default: main = new Color(0.72f, 0.75f, 0.8f); trim = new Color(0.9f, 0.72f, 0.38f); cloth = new Color(0.13f, 0.16f, 0.36f); break;
            }
            // (A little of the colour's gone out of them already.)
            Color grey = new Color(0.5f, 0.48f, 0.54f);
            main = Color.Lerp(main, grey, 0.22f);
            cloth = Color.Lerp(cloth, grey, 0.3f);
            bool metal = Kind == Suit.Knight || Kind == Suit.Legionary || Kind == Suit.Hoplite || Kind == Suit.Viking;
            var m = metal ? PatchworkAssets.Plain(main, 0.78f) : PatchworkAssets.Felt(main);
            var tm = Kind == Suit.Terracotta ? PatchworkAssets.Felt(trim) : PatchworkAssets.Plain(trim, 0.7f);
            var cm = PatchworkAssets.Fabric(cloth);
            var wood = PatchworkAssets.Wood(new Color(0.3f, 0.2f, 0.14f));
            var steel = PatchworkAssets.Plain(new Color(0.8f, 0.82f, 0.88f), 0.9f);
            var dark = PatchworkAssets.Plain(new Color(0.05f, 0.04f, 0.08f), 0.3f);

            rig = Pivot(transform, "Rig", Vector3.zero);
            var body = Pivot(rig, "Body", Vector3.zero);
            body.localScale = Vector3.one * size;

            // Legs (they swing from the hip).
            legL = Pivot(body, "LegL", new Vector3(-0.26f, 1.08f, 0f));
            legR = Pivot(body, "LegR", new Vector3(0.26f, 1.08f, 0f));
            foreach (var leg in new[] { legL, legR })
            {
                P(PrimitiveType.Capsule, leg, new Vector3(0f, -0.5f, 0f), new Vector3(0.34f, 0.52f, 0.34f), Kind == Suit.Knight ? m : cm);
                P(PrimitiveType.Cube, leg, new Vector3(0f, -0.98f, 0.1f), new Vector3(0.3f, 0.2f, 0.52f), Kind == Suit.Knight ? m : wood);
                Tuft(leg, new Vector3(0f, -0.05f, 0f), 1.2f, leg == legL ? 1f : 4f);
            }

            // Body and skirt.
            P(PrimitiveType.Capsule, body, new Vector3(0f, 1.6f, 0f), new Vector3(0.82f, 0.56f, 0.62f), m);
            P(PrimitiveType.Cylinder, body, new Vector3(0f, 1.18f, 0f), new Vector3(0.86f, 0.06f, 0.68f), tm);
            if (Kind == Suit.Knight) P(PrimitiveType.Sphere, body, new Vector3(0f, 1.05f, 0f), new Vector3(0.9f, 0.5f, 0.7f), m);
            else
                for (int i = 0; i < 7; i++)
                {
                    float a = (i / 7f) * Mathf.PI * 2f;
                    P(PrimitiveType.Cube, body, new Vector3(Mathf.Sin(a) * 0.4f, 0.86f, Mathf.Cos(a) * 0.32f), new Vector3(0.3f, 0.6f, 0.07f),
                        i % 2 == 0 ? (Kind == Suit.Samurai || Kind == Suit.Mongol ? m : cm) : tm, new Vector3(-10f, a * Mathf.Rad2Deg, 0f));
                }
            if (Kind == Suit.Legionary)
                for (int r = 0; r < 4; r++) P(PrimitiveType.Cylinder, body, new Vector3(0f, 1.34f + r * 0.17f, 0f), new Vector3(0.88f, 0.03f, 0.68f), tm);
            if (Captain)
            {
                // The Captain wears the stolen lens on his chest like a medal.
                P(PrimitiveType.Cylinder, body, new Vector3(0f, 1.66f, 0.32f), new Vector3(0.34f, 0.02f, 0.34f), PatchworkAssets.Plain(new Color(0.85f, 0.65f, 0.3f), 0.85f), new Vector3(90f, 0f, 0f));
                P(PrimitiveType.Cylinder, body, new Vector3(0f, 1.66f, 0.335f), new Vector3(0.27f, 0.02f, 0.27f), PatchworkAssets.Glow(new Color(0.6f, 0.9f, 1f), 1.6f), new Vector3(90f, 0f, 0f));
                P(PrimitiveType.Cube, body, new Vector3(0f, 1.5f, -0.36f), new Vector3(0.9f, 1.5f, 0.05f), PatchworkAssets.Fabric(new Color(0.45f, 0.12f, 0.16f)), new Vector3(8f, 0f, 0f));
            }

            // Arms (they swing from the shoulder). The right one has the weapon; the left the shield or bow.
            armL = Pivot(body, "ArmL", new Vector3(-0.62f, 2f, 0f));
            armR = Pivot(body, "ArmR", new Vector3(0.62f, 2f, 0f));
            foreach (var arm in new[] { armL, armR })
            {
                P(PrimitiveType.Sphere, arm, new Vector3(0f, -0.02f, 0f), new Vector3(0.5f, 0.36f, 0.5f), m);
                P(PrimitiveType.Capsule, arm, new Vector3(0f, -0.45f, 0f), new Vector3(0.28f, 0.46f, 0.28f), Kind == Suit.Knight || Kind == Suit.Samurai ? m : cm);
                P(PrimitiveType.Sphere, arm, new Vector3(0f, -0.9f, 0f), Vector3.one * 0.26f, Kind == Suit.Knight ? m : dark);
                Tuft(arm, new Vector3(0f, -0.2f, 0f), 1.1f, arm == armL ? 2f : 5f);
            }
            if (Kind == Suit.Samurai)
                foreach (var arm in new[] { armL, armR })
                    for (int r = 0; r < 3; r++)
                        P(PrimitiveType.Cube, arm, new Vector3(arm == armL ? -0.16f : 0.16f, -0.05f - r * 0.2f, 0f), new Vector3(0.1f, 0.19f, 0.6f), r % 2 == 0 ? m : tm);
            if (Kind == Suit.Eagle)
                foreach (var arm in new[] { armL, armR })
                    for (int f = 0; f < 4; f++)
                        P(PrimitiveType.Cube, arm, new Vector3((arm == armL ? -1f : 1f) * (0.18f + f * 0.05f), -0.2f - f * 0.2f, -0.1f), new Vector3(0.5f, 0.07f, 0.24f),
                            f % 2 == 0 ? tm : PatchworkAssets.Felt(new Color(0.96f, 0.91f, 0.8f)));

            // The weapon, in the right hand, pointing forward out of the fist.
            var hand = Pivot(armR, "Hand", new Vector3(0f, -0.9f, 0f));
            hand.localRotation = Quaternion.Euler(90f, 0f, 0f);   // "up" out of the fist is now straight ahead of a hanging arm
            switch (style)
            {
                case Style.Spear:
                case Style.Heavy:
                    P(PrimitiveType.Cylinder, hand, new Vector3(0f, 0.5f, 0f), new Vector3(0.07f, 1.5f, 0.07f), wood);
                    P(PrimitiveType.Cube, hand, new Vector3(0f, 2.12f, 0f), new Vector3(style == Style.Heavy ? 0.3f : 0.14f, 0.42f, 0.04f), steel, new Vector3(0f, 0f, 0f));
                    if (style == Style.Heavy) P(PrimitiveType.Cube, hand, new Vector3(0.16f, 1.86f, 0f), new Vector3(0.34f, 0.1f, 0.04f), steel, new Vector3(0f, 0f, 30f));
                    break;
                case Style.Leaper:
                    // A wooden paddle edged with black glass (obsidian) teeth.
                    P(PrimitiveType.Cylinder, hand, new Vector3(0f, 0.2f, 0f), new Vector3(0.07f, 0.3f, 0.07f), wood);
                    P(PrimitiveType.Cube, hand, new Vector3(0f, 0.95f, 0f), new Vector3(0.22f, 1f, 0.06f), wood);
                    for (int i = 0; i < 5; i++)
                        for (int s = -1; s <= 1; s += 2)
                            P(PrimitiveType.Cube, hand, new Vector3(s * 0.13f, 0.58f + i * 0.19f, 0f), new Vector3(0.08f, 0.14f, 0.03f), dark, new Vector3(0f, 0f, 45f));
                    break;
                case Style.Archer:
                    break;
                default:
                    bool axe = Kind == Suit.Viking;
                    P(PrimitiveType.Cylinder, hand, new Vector3(0f, axe ? 0.5f : 0.05f, 0f), new Vector3(0.07f, axe ? 0.7f : 0.16f, 0.07f), wood);
                    if (axe) P(PrimitiveType.Cube, hand, new Vector3(0.18f, 1.05f, 0f), new Vector3(0.42f, 0.4f, 0.05f), steel);
                    else
                    {
                        P(PrimitiveType.Cube, hand, new Vector3(0f, 0.2f, 0f), new Vector3(Kind == Suit.Samurai ? 0.16f : 0.4f, 0.05f, 0.08f), tm);
                        P(PrimitiveType.Cube, hand, new Vector3(0f, 0.95f, 0f), new Vector3(Kind == Suit.Samurai ? 0.07f : 0.12f, 1.45f, 0.03f), steel);
                    }
                    break;
            }
            if (shield)
            {
                var sh = Pivot(armL, "Shield", new Vector3(-0.1f, -0.7f, 0.3f));
                if (Kind == Suit.Legionary) P(PrimitiveType.Cube, sh, Vector3.zero, new Vector3(0.9f, 1.4f, 0.08f), PatchworkAssets.Felt(Color.Lerp(trim, grey, 0.2f)));
                else if (Kind == Suit.Knight)
                {
                    P(PrimitiveType.Cube, sh, new Vector3(0f, 0.15f, 0f), new Vector3(0.9f, 0.9f, 0.08f), cm);
                    P(PrimitiveType.Cube, sh, new Vector3(0f, -0.3f, 0f), new Vector3(0.64f, 0.64f, 0.08f), cm, new Vector3(0f, 0f, 45f));
                    P(PrimitiveType.Cube, sh, new Vector3(0f, 0.1f, 0.05f), new Vector3(0.9f, 0.16f, 0.02f), tm);
                }
                else P(PrimitiveType.Cylinder, sh, Vector3.zero, new Vector3(1.15f, 0.04f, 1.15f), Kind == Suit.Hoplite ? m : wood, new Vector3(90f, 0f, 0f));
                P(PrimitiveType.Sphere, sh, new Vector3(0f, 0f, 0.05f), new Vector3(0.28f, 0.28f, 0.14f), tm);
            }
            if (style == Style.Archer)
            {
                // A short bow in the left fist.
                for (int i = -3; i <= 3; i++)
                    P(PrimitiveType.Cube, armL, new Vector3(0f, -0.9f + i * 0.2f, 0.12f - Mathf.Abs(i) * 0.06f), new Vector3(0.05f, 0.24f, 0.07f), wood, new Vector3(i * -14f, 0f, 0f));
                P(PrimitiveType.Cylinder, armL, new Vector3(0f, -0.9f, -0.07f), new Vector3(0.012f, 0.62f, 0.012f), PatchworkAssets.Felt(new Color(0.96f, 0.91f, 0.8f)));
            }

            // The helmet: empty, and looking at you.
            head = Pivot(body, "Helm", new Vector3(0f, 2.36f, 0f));
            P(PrimitiveType.Sphere, head, Vector3.zero, new Vector3(0.56f, 0.6f, 0.6f), Kind == Suit.Terracotta || Kind == Suit.Eagle ? PatchworkAssets.Felt(Kind == Suit.Eagle ? trim : main) : m);
            P(PrimitiveType.Cube, head, new Vector3(0f, 0.02f, 0.2f), new Vector3(0.4f, 0.16f, 0.24f), dark);
            eyes = Pivot(head, "Eyes", new Vector3(0f, 0.03f, 0.31f));
            for (int s = -1; s <= 1; s += 2)
                P(PrimitiveType.Sphere, eyes, new Vector3(s * 0.1f, 0f, 0f), new Vector3(0.09f, 0.09f, 0.05f), Monsters.Glow);
            switch (Kind)
            {
                case Suit.Hoplite:
                    for (int i = 0; i < 7; i++) P(PrimitiveType.Cube, head, new Vector3(0f, 0.34f + Mathf.Sin(i / 6f * Mathf.PI) * 0.12f, 0.24f - i * 0.09f), new Vector3(0.06f, 0.26f, 0.1f), tm);
                    break;
                case Suit.Legionary:
                    for (int i = -3; i <= 3; i++) P(PrimitiveType.Cube, head, new Vector3(i * 0.08f, 0.4f - Mathf.Abs(i) * 0.03f, 0f), new Vector3(0.07f, 0.22f, 0.1f), tm);
                    break;
                case Suit.Viking:
                    P(PrimitiveType.Sphere, head, new Vector3(0f, 0.2f, 0f), new Vector3(0.5f, 0.42f, 0.54f), m);
                    P(PrimitiveType.Cube, head, new Vector3(0f, -0.02f, 0.3f), new Vector3(0.06f, 0.26f, 0.04f), m);
                    break;
                case Suit.Samurai:
                    for (int s = -1; s <= 1; s += 2) P(PrimitiveType.Cube, head, new Vector3(s * 0.2f, 0.42f, 0.2f), new Vector3(0.05f, 0.42f, 0.03f), tm, new Vector3(0f, 0f, s * -28f));
                    P(PrimitiveType.Cylinder, head, new Vector3(0f, 0.05f, -0.05f), new Vector3(0.8f, 0.04f, 0.8f), m, new Vector3(-14f, 0f, 0f));
                    break;
                case Suit.Terracotta:
                    P(PrimitiveType.Sphere, head, new Vector3(0.1f, 0.34f, -0.08f), new Vector3(0.2f, 0.22f, 0.2f), PatchworkAssets.Felt(trim));
                    break;
                case Suit.Mongol:
                    P(PrimitiveType.Sphere, head, new Vector3(0f, 0.3f, 0f), new Vector3(0.3f, 0.5f, 0.3f), tm);
                    P(PrimitiveType.Cylinder, head, new Vector3(0f, 0.14f, 0f), new Vector3(0.66f, 0.07f, 0.68f), PatchworkAssets.Knit(new Color(0.36f, 0.3f, 0.26f)));
                    break;
                case Suit.Eagle:
                    P(PrimitiveType.Cube, head, new Vector3(0f, 0.16f, 0.36f), new Vector3(0.16f, 0.12f, 0.3f), PatchworkAssets.Felt(new Color(0.95f, 0.75f, 0.3f)), new Vector3(24f, 0f, 0f));
                    for (int i = -2; i <= 2; i++) P(PrimitiveType.Cube, head, new Vector3(i * 0.1f, 0.4f, -0.14f), new Vector3(0.07f, 0.4f, 0.03f), i % 2 == 0 ? tm : cm, new Vector3(-20f, 0f, i * -9f));
                    break;
                default:
                    P(PrimitiveType.Cube, head, new Vector3(0f, 0.38f, -0.06f), new Vector3(0.06f, 0.3f, 0.34f), PatchworkAssets.Felt(Color.Lerp(Captain ? new Color(0.7f, 0.15f, 0.2f) : cloth, grey, 0.1f)), new Vector3(-20f, 0f, 0f));
                    break;
            }
            Tuft(head, new Vector3(0f, -0.3f, 0f), 1.4f, 3f);

            // The strings it hangs from.
            strings = Pivot(body, "Strings", new Vector3(0f, 2.1f, 0f));
            for (int s = -1; s <= 1; s += 2)
                P(PrimitiveType.Cylinder, strings, new Vector3(s * 0.55f, 0.95f, 0f), new Vector3(0.018f, 0.95f, 0.018f), Monsters.Dark);
            P(PrimitiveType.Cylinder, strings, new Vector3(0f, 1.2f, 0f), new Vector3(0.018f, 0.7f, 0.018f), Monsters.Dark);
        }

        float Reach { get { return (style == Style.Spear ? 3.3f : style == Style.Heavy ? 3f : 2.5f) * (Captain ? 1.2f : 1f); } }
        float WindupTime { get { return style == Style.Heavy ? 0.95f : style == Style.Archer ? 0.8f : style == Style.Leaper ? 0.6f : style == Style.Dasher ? 0.55f : Captain ? 0.75f : 0.6f; } }
        float SwingTime { get { return style == Style.Leaper ? 0.75f : style == Style.Dasher ? 0.3f : 0.3f; } }
        float RecoverTime { get { return style == Style.Heavy ? 1.3f : style == Style.Leaper ? 1.1f : style == Style.Dasher ? 1f : style == Style.Archer ? 0.5f : 0.8f; } }
        bool Spin { get { return Captain && Health <= MaxHealth / 2; } }

        protected override bool Blocks(Vector3 from, bool finisher)
        {
            if (!shield || shieldDown > 0f) return false;
            if (state != State.Stalk && state != State.Windup) return false;
            if (from.y > Center.y + 0.6f) return false;   // from above: over the top of it
            Vector3 d = from - pos;
            d.y = 0f;
            if (d.sqrMagnitude < 0.01f || Vector3.Dot(d.normalized, facing) < 0.25f) return false;   // from behind
            if (finisher) { shieldDown = Captain ? 2.6f : 3.6f; return false; }   // the third hit knocks it aside
            return true;
        }

        protected override void OnHurt(bool finisher)
        {
            // A hit stops most of them part way through a swing (not the heavy ones).
            if (state == State.Windup && style != Style.Heavy && !Captain) { state = State.Recover; st = 0f; }
        }

        protected override void OnBeaten()
        {
            FxBurst.Spawn(FxBurst.Kind.Scraps, Center, new Color(0.7f, 0.7f, 0.76f));
            Sfx.PlayNear(SfxId.Clack, Center, 0.6f, 40f);
            Sfx.PlayNear(SfxId.PlatformThunk, Center, 0.5f, 40f);
        }

        void Update()
        {
            if (IsBeaten) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            var player = ThePlayer();
            bool free = Common(dt);
            st += dt;
            shieldDown -= dt;
            Vector3 before = pos;

            Vector3 toP = player != null ? player.transform.position - pos : facing;
            toP.y = 0f;
            float dist = toP.magnitude;
            Vector3 dirP = dist > 0.01f ? toP / dist : facing;

            switch (state)
            {
                case State.Wake:
                {
                    float k = Mathf.Clamp01(st / 0.75f);
                    pos = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.5f;
                    if (k >= 1f)
                    {
                        pos = to;
                        state = State.Stalk;
                        st = 0f;
                        Sfx.PlayNear(SfxId.PlatformThunk, pos, 0.5f, 40f);
                        FxBurst.Spawn(FxBurst.Kind.Poof, pos + Vector3.up * 0.2f, new Color(0.8f, 0.8f, 0.85f));
                    }
                    break;
                }
                case State.Stalk:
                    if (!free || player == null) break;
                    facing = Vector3.Slerp(facing, dirP, 1f - Mathf.Exp(-6f * dt));
                    float speed = (style == Style.Heavy ? 1.7f : style == Style.Dasher ? 3.4f : 2.7f) * (Captain ? 1.1f : 1f);
                    Vector3 side = Vector3.Cross(Vector3.up, dirP) * (strafeLeft ? -1f : 1f);
                    if (style == Style.Archer)
                    {
                        // Keeps its distance, sidling.
                        if (dist < 7f) pos -= dirP * speed * dt;
                        else if (dist > 12f) pos += dirP * speed * dt;
                        else pos += side * speed * 0.5f * dt;
                    }
                    else if (style == Style.Leaper || style == Style.Dasher)
                    {
                        if (dist > 7.5f) pos += dirP * speed * dt;
                        else if (dist < 4f) pos -= dirP * speed * 0.6f * dt;
                        else pos += side * speed * 0.6f * dt;
                    }
                    else if (dist > Reach * 0.8f) pos += dirP * speed * dt;
                    else pos += side * speed * 0.3f * dt;
                    if (Random.value < dt * 0.25f) strafeLeft = !strafeLeft;

                    cooldown -= dt;
                    float range = style == Style.Archer ? 18f : style == Style.Leaper ? 9f : style == Style.Dasher ? 8.5f : Reach;
                    if (cooldown <= 0f && dist < range && Time.time - lastSwing > 0.55f)
                    {
                        lastSwing = Time.time;
                        state = State.Windup;
                        st = 0f;
                        Sfx.PlayNear(SfxId.Lever, Center, 0.4f, 30f);
                        FxBurst.Spawn(FxBurst.Kind.Sparkle, head.position, SnarlKnot.FrayGlow);
                    }
                    break;

                case State.Windup:
                    if (!free) break;
                    facing = Vector3.Slerp(facing, dirP, 1f - Mathf.Exp(-(style == Style.Heavy ? 1.5f : 4f) * dt));
                    if (st >= WindupTime)
                    {
                        state = State.Swing;
                        st = 0f;
                        struckOnce = false;
                        from = pos;
                        if (style == Style.Dasher) to = pos + facing * Mathf.Min(dist + 3.5f, 10f);
                        else if (style == Style.Leaper) to = player != null ? new Vector3(player.transform.position.x, floorY, player.transform.position.z) : pos;
                        else to = pos;
                        if (Fence != null) to = Fence(to);
                        if (style == Style.Archer && player != null)
                        {
                            Vector3 aim = player.Motor.Center + player.Motor.Velocity * 0.25f - (Center + Vector3.up * 0.4f);
                            ToyArrow.Loose(Center + Vector3.up * 0.4f + facing * 0.6f, aim.normalized);
                            Sfx.PlayNear(SfxId.Release, Center, 0.5f, 40f);
                        }
                        else Sfx.PlayNear(SfxId.Whoosh, Center, 0.3f, 30f);
                    }
                    break;

                case State.Swing:
                {
                    float k = Mathf.Clamp01(st / SwingTime);
                    if (style == Style.Dasher)
                    {
                        pos = Vector3.Lerp(from, to, k);
                        if (!struckOnce && player != null && Near(player, pos, 1.3f)) Hit(player);
                    }
                    else if (style == Style.Leaper)
                    {
                        pos = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 2.8f;
                        if (k >= 1f)
                        {
                            pos = to;
                            Sfx.PlayNear(SfxId.PlatformThunk, pos, 0.6f, 40f);
                            StrikeFx.Ring(pos + Vector3.up * 0.2f, 2f, SnarlKnot.FrayGlow, 0.2f);
                            if (player != null && Near(player, pos, 2f)) Hit(player);
                        }
                    }
                    else if (style != Style.Archer && !struckOnce && k >= 0.4f)
                    {
                        struckOnce = true;   // (one swing, one chance)
                        if (player != null)
                        {
                            bool inReach;
                            if (Spin) { inReach = Near(player, pos, 3.2f); StrikeFx.Ring(Center, 3f, SnarlKnot.FrayGlow, 0.22f); }
                            else if (style == Style.Heavy)
                            {
                                Vector3 spot = pos + facing * 1.9f;
                                inReach = Near(player, spot, 2.1f);
                                StrikeFx.Ring(spot + Vector3.up * 0.2f, 2f, SnarlKnot.FrayGlow, 0.22f);
                                Sfx.PlayNear(SfxId.PlatformThunk, spot, 0.6f, 40f);
                            }
                            else
                            {
                                float cone = style == Style.Spear ? 0.85f : 0.3f;
                                inReach = dist < Reach && Vector3.Dot(dirP, facing) > cone && Mathf.Abs(player.Motor.Center.y - Center.y) < 1.9f;
                                StrikeFx.Arc(Center, facing, style == Style.Spear ? 8f : 70f, style == Style.Spear ? -8f : -60f, Reach * 0.85f, SnarlKnot.FrayGlow, 0.18f);
                            }
                            if (inReach) player.Game.Hurt(Center);
                        }
                    }
                    if (k >= 1f) { state = State.Recover; st = 0f; }
                    break;
                }
                case State.Recover:
                    if (st >= RecoverTime)
                    {
                        state = State.Stalk;
                        st = 0f;
                        cooldown = Random.Range(0.7f, 1.7f) * (Captain ? 0.7f : 1f);
                    }
                    break;
            }

            // Stay in the hall, and out of each other.
            if (state != State.Wake)
            {
                foreach (var o in live)
                {
                    if (o == this || o == null || o.IsBeaten) continue;
                    Vector3 d = pos - o.pos;
                    d.y = 0f;
                    float m = d.magnitude;
                    if (m < 1.7f && m > 0.001f) pos += d / m * (1.7f - m) * 0.5f;
                }
                if (Fence != null) pos = Fence(pos);
                if (!(state == State.Swing && style == Style.Leaper)) pos.y = floorY;
            }

            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(new Vector3(facing.x, 0f, facing.z).normalized, Vector3.up);
            moved = Mathf.Lerp(moved, new Vector2(pos.x - before.x, pos.z - before.z).magnitude / dt, 1f - Mathf.Exp(-10f * dt));
            Pose(dt);
            if (state != State.Wake) Contact(player, 0.85f * size, 2.1f * size, false);
        }

        static bool Near(PatchworkPlayer player, Vector3 feet, float radius)
        {
            Vector3 d = player.transform.position - feet;
            return new Vector2(d.x, d.z).magnitude < radius && d.y > -1.2f && d.y < 2.2f;
        }

        void Hit(PatchworkPlayer player)
        {
            struckOnce = true;
            if (player.Game != null) player.Game.Hurt(Center);
        }

        void Pose(float dt)
        {
            float ease = 1f - Mathf.Exp(-14f * dt);
            walk += moved * dt * 2.6f;
            float stride = Mathf.Sin(walk) * 30f * Mathf.Clamp01(moved / 2f);
            legL.localRotation = Quaternion.Euler(stride, 0f, 0f);
            legR.localRotation = Quaternion.Euler(-stride, 0f, 0f);

            // It hangs from its strings: a jerky sway, never quite upright.
            float jerk = Mathf.Sin(t * 5.3f) * 2.5f + Mathf.Sin(t * 11f) * 1.2f;
            float lean = 0f, crouch = 0f, spinYaw = 0f;
            Quaternion r = Quaternion.Euler(-28f, 0f, 6f), l = Quaternion.Euler(shield ? -70f : -10f, shield ? 25f : 0f, -6f);
            float wind = state == State.Windup ? Mathf.Clamp01(st / WindupTime) : 0f;
            float sw = state == State.Swing ? Mathf.Clamp01(st / SwingTime) : 0f;
            switch (style)
            {
                case Style.Spear:
                    if (state == State.Windup) { r = Quaternion.Euler(-75f + wind * 10f, 0f, 0f); lean = -12f * wind; }
                    else if (state == State.Swing) { r = Quaternion.Euler(-95f, 0f, 0f); lean = 22f; }
                    else r = Quaternion.Euler(-60f, 0f, 4f);
                    break;
                case Style.Archer:
                    l = Quaternion.Euler(-90f, 0f, 0f);
                    r = state == State.Windup ? Quaternion.Euler(-90f + wind * 20f, -30f * wind, 0f) : Quaternion.Euler(-40f, 0f, 6f);
                    lean = state == State.Windup ? -6f : 0f;
                    break;
                case Style.Leaper:
                    if (state == State.Windup) { crouch = wind * 0.45f; r = Quaternion.Euler(-150f * wind, 0f, 0f); }
                    else if (state == State.Swing) { r = Quaternion.Euler(-170f + sw * 150f, 0f, 0f); lean = 20f; }
                    else if (state == State.Recover) crouch = 0.25f * (1f - Mathf.Clamp01(st / RecoverTime));
                    break;
                case Style.Dasher:
                    if (state == State.Windup) { r = Quaternion.Euler(-20f, 0f, 60f * wind); lean = 18f * wind; crouch = 0.15f * wind; }
                    else if (state == State.Swing) { r = Quaternion.Euler(-90f, -70f + sw * 140f, 0f); lean = 30f; }
                    break;
                default:
                    if (Spin && state != State.Stalk)
                    {
                        r = Quaternion.Euler(-90f, -80f, 0f);
                        if (state == State.Windup) spinYaw = -wind * 60f;
                        else if (state == State.Swing) spinYaw = -60f + sw * 420f;
                    }
                    else if (state == State.Windup) { r = Quaternion.Euler(-165f * wind, 0f, 10f); lean = -10f * wind; }
                    else if (state == State.Swing) { r = Quaternion.Euler(-165f + sw * 185f, 0f, 0f); lean = 16f * sw; }
                    else if (state == State.Recover) { r = Quaternion.Euler(10f, 0f, 8f); lean = 12f; }
                    break;
            }
            if (shield && shieldDown > 0f) l = Quaternion.Euler(20f, 0f, -115f);   // shield knocked wide
            armR.localRotation = Quaternion.Slerp(armR.localRotation, r, ease);
            armL.localRotation = Quaternion.Slerp(armL.localRotation, l, ease);
            if (stun > 0f) lean = -20f;
            rig.localRotation = Quaternion.Slerp(rig.localRotation, Quaternion.Euler(lean + jerk, spinYaw, jerk * 0.6f), state == State.Swing && Spin ? 1f : ease);
            rig.localPosition = Vector3.Lerp(rig.localPosition, Vector3.down * crouch * size, ease);
            head.localRotation = Quaternion.Euler(Mathf.Sin(t * 3.1f) * 5f, Mathf.Sin(t * 1.7f) * 10f * (state == State.Stalk ? 1f : 0f), jerk * 2f);
            // The lights in the helmet flare just before it strikes: that's your warning.
            float flare = state == State.Windup ? 1.6f + Mathf.Sin(st * 30f) * 0.5f : 1f;
            eyes.localScale = Vector3.Lerp(eyes.localScale, Vector3.one * flare, ease);
            strings.localRotation = Quaternion.Inverse(rig.localRotation);
        }
    }

    // A toy arrow with a rubber sucker on the end. Still stings.
    public class ToyArrow : MonoBehaviour
    {
        Vector3 vel;
        float life = 2.6f;

        public static void Loose(Vector3 from, Vector3 dir)
        {
            var go = new GameObject("ToyArrow");
            go.transform.position = from;
            go.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            var a = go.AddComponent<ToyArrow>();
            a.vel = dir * 13f;
            Monsters.Part(PrimitiveType.Cylinder, go.transform, Vector3.zero, new Vector3(0.05f, 0.45f, 0.05f), PatchworkAssets.Wood(new Color(0.6f, 0.45f, 0.3f)), new Vector3(90f, 0f, 0f));
            Monsters.Part(PrimitiveType.Sphere, go.transform, new Vector3(0f, 0f, 0.48f), new Vector3(0.2f, 0.2f, 0.1f), PatchworkAssets.Felt(new Color(0.8f, 0.25f, 0.25f)));
            for (int i = 0; i < 3; i++)
                Monsters.Part(PrimitiveType.Cube, go.transform, new Vector3(0f, 0f, -0.4f), new Vector3(0.02f, 0.16f, 0.2f), Monsters.Grey, new Vector3(0f, 0f, i * 60f));
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            Vector3 step = vel * dt;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, vel.normalized, out hit, step.magnitude + 0.3f, ~(1 << 2), QueryTriggerInteraction.Ignore)) { Stop(); return; }
            transform.position += step;
            life -= dt;
            var game = PatchworkGame.Instance;
            if (game != null && game.Player != null && (game.Player.Motor.Center - transform.position).sqrMagnitude < 0.7f)
            {
                game.Hurt(transform.position - vel.normalized);
                Stop();
                return;
            }
            if (life <= 0f) Stop();
        }

        void Stop()
        {
            FxBurst.Spawn(FxBurst.Kind.Poof, transform.position, new Color(0.75f, 0.75f, 0.8f));
            Sfx.PlayNear(SfxId.Pop, transform.position, 0.25f, 20f);
            Destroy(gameObject);
        }
    }

    // ------------------------------------------------------------------ In the tanks

    // An ink jelly: what's left of a jellyfish once the Fray has drunk the colour out of it.
    // It drifts, and drifts toward you. One hit pops it; touching it stings.
    public class InkJelly : MuseumFoe
    {
        Vector3 home;
        float minY, maxY, seed;
        Transform bell;
        Transform[] arms;

        public static InkJelly Create(Transform parent, Vector3 at, float minY, float maxY)
        {
            var go = new GameObject("InkJelly");
            go.transform.SetParent(parent, false);
            go.transform.position = at;
            var j = go.AddComponent<InkJelly>();
            j.home = at;
            j.pos = at;
            j.minY = minY;
            j.maxY = maxY;
            j.seed = at.x * 1.7f + at.z * 0.9f + at.y;
            j.MaxHealth = 1;
            j.Score = 150;
            j.centreY = 0f;
            j.rig = Pivot(go.transform, "Rig", Vector3.zero);
            j.bell = Pivot(j.rig, "Bell", Vector3.zero);
            P(PrimitiveType.Sphere, j.bell, new Vector3(0f, 0.1f, 0f), new Vector3(1.1f, 0.8f, 1.1f), Monsters.Grey);
            P(PrimitiveType.Sphere, j.bell, new Vector3(0f, -0.02f, 0f), new Vector3(1.2f, 0.3f, 1.2f), Monsters.Dark);
            P(PrimitiveType.Sphere, j.bell, new Vector3(0f, 0.12f, 0.42f), new Vector3(0.4f, 0.4f, 0.24f), PatchworkAssets.Plain(new Color(0.05f, 0.04f, 0.07f), 0.95f));
            P(PrimitiveType.Sphere, j.bell, new Vector3(0f, 0.12f, 0.52f), new Vector3(0.17f, 0.17f, 0.1f), Monsters.Glow);
            j.arms = new Transform[6];
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3f;
                j.arms[i] = Pivot(j.rig, "Arm", new Vector3(Mathf.Cos(a) * 0.32f, -0.1f, Mathf.Sin(a) * 0.32f));
                P(PrimitiveType.Capsule, j.arms[i], new Vector3(0f, -0.45f, 0f), new Vector3(0.07f, 0.45f, 0.07f), i % 2 == 0 ? Monsters.Grey : Monsters.Dark);
                P(PrimitiveType.Sphere, j.arms[i], new Vector3(0f, -0.92f, 0f), Vector3.one * 0.11f, Monsters.Glow);
            }
            j.Register(0.7f);
            return j;
        }

        void Update()
        {
            if (IsBeaten) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            var player = ThePlayer();
            bool free = Common(dt);
            Vector3 want = home + new Vector3(Mathf.Sin(t * 0.5f + seed) * 2.2f, Mathf.Sin(t * 0.8f + seed * 2f) * 0.9f, Mathf.Cos(t * 0.4f + seed) * 2.2f);
            if (free && player != null)
            {
                Vector3 pc = player.Motor.Center;
                if ((pc - pos).sqrMagnitude < 64f && (pc - home).sqrMagnitude < 121f) want = pc;
            }
            // It swims in pulses, like the real thing: a squeeze and a glide.
            float pulse = Mathf.Repeat(t * 0.9f + seed, 1f);
            float push = pulse < 0.35f ? 2.4f : 0.5f;
            if (free) pos = Vector3.MoveTowards(pos, want, push * dt);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
            float squeeze = pulse < 0.35f ? Mathf.Sin(pulse / 0.35f * Mathf.PI) : 0f;
            bell.localScale = new Vector3(1f - squeeze * 0.22f, 1f + squeeze * 0.25f, 1f - squeeze * 0.22f);
            for (int i = 0; i < arms.Length; i++)
                arms[i].localRotation = Quaternion.Euler(Mathf.Sin(t * 2f + i) * 18f + squeeze * 20f, 0f, Mathf.Cos(t * 1.6f + i * 2f) * 18f);
            if (player != null)
            {
                Vector3 f = player.Motor.Center - pos;
                f.y = 0f;
                if (f.sqrMagnitude > 0.05f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(f.normalized, Vector3.up), 1f - Mathf.Exp(-2f * dt));
            }
            Contact(player, 0.95f, 1.1f, true);
        }
    }

    // A grey clot: a great knot of the Fray's thread tied round the heart of a tank, drinking
    // the colour out of the water. It can't move, but every few seconds it swells and lets go
    // a shock all round it: get your hits in, then get clear.
    public class GreyClot : MuseumFoe
    {
        Transform knot, eye, murk;
        Transform[] ropes;
        float pulse = 3f, swell = -1f;
        const float ShockRadius = 3.9f;

        public static GreyClot Create(Transform parent, Vector3 at, float seed)
        {
            var go = new GameObject("GreyClot");
            go.transform.SetParent(parent, false);
            go.transform.position = at;
            var c = go.AddComponent<GreyClot>();
            c.pos = at;
            c.MaxHealth = 6;
            c.Score = 600;
            c.centreY = 0f;
            c.knockScale = 0f;
            c.t = seed;
            c.rig = Pivot(go.transform, "Rig", Vector3.zero);
            c.knot = Pivot(c.rig, "Knot", Vector3.zero);
            P(PrimitiveType.Sphere, c.knot, Vector3.zero, Vector3.one * 1.9f, Monsters.Grey);
            for (int i = 0; i < 9; i++)
            {
                Vector3 d = (Quaternion.Euler(i * 67f + seed * 30f, i * 131f, 0f) * Vector3.forward).normalized;
                P(PrimitiveType.Sphere, c.knot, d * 0.75f, Vector3.one * (0.7f + (i % 3) * 0.18f), i % 2 == 0 ? Monsters.Dark : Monsters.Grey);
            }
            for (int i = 0; i < 4; i++)
                P(PrimitiveType.Cylinder, c.knot, Vector3.zero, new Vector3(2.02f, 0.06f, 2.02f), Monsters.Dark, new Vector3(i * 47f + 20f, i * 80f, i * 31f));
            for (int i = 0; i < 6; i++) Tuft(c.knot, (Quaternion.Euler(i * 55f, i * 97f, 0f) * Vector3.up) * 0.98f, 3.2f, i * 1.3f);
            c.eye = Pivot(c.rig, "Eye", Vector3.zero);
            P(PrimitiveType.Sphere, c.eye, new Vector3(0f, 0f, 0.8f), new Vector3(0.8f, 0.8f, 0.5f), PatchworkAssets.Plain(new Color(0.05f, 0.04f, 0.07f), 0.95f));
            P(PrimitiveType.Sphere, c.eye, new Vector3(0f, 0f, 1f), new Vector3(0.36f, 0.36f, 0.2f), Monsters.Glow);
            // Ropes of thread, tying it to the tank.
            c.ropes = new Transform[5];
            for (int i = 0; i < 5; i++)
            {
                c.ropes[i] = Pivot(go.transform, "Rope", Vector3.zero);
                c.ropes[i].localRotation = Quaternion.Euler(i == 0 ? 180f : 62f + (i % 2) * 40f, i * 90f + seed * 20f, 0f);
                P(PrimitiveType.Cylinder, c.ropes[i], new Vector3(0f, 2.9f, 0f), new Vector3(0.09f, 2.1f, 0.09f), i % 2 == 0 ? Monsters.Dark : Monsters.Grey);
                for (int k = 0; k < 3; k++) P(PrimitiveType.Sphere, c.ropes[i], new Vector3(0f, 1.6f + k * 1.1f, 0f), Vector3.one * 0.2f, Monsters.Grey);
            }
            // The murk it's made of the water round it.
            c.murk = P(PrimitiveType.Sphere, go.transform, Vector3.zero, Vector3.one * 9f, PatchworkAssets.Bubble(new Color(0.3f, 0.28f, 0.36f, 0.3f)));
            c.murk.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            c.Register(1.15f);
            return c;
        }

        protected override void OnHurt(bool finisher)
        {
            // A hit sets it off sooner.
            if (swell < 0f) pulse = Mathf.Min(pulse, 1.1f);
        }

        protected override void OnBeaten()
        {
            FxBurst.Spawn(FxBurst.Kind.Scraps, pos, new Color(0.6f, 0.9f, 1f));
            FxBurst.Spawn(FxBurst.Kind.Sparkle, pos + Vector3.up, new Color(1f, 0.9f, 0.6f));
            StrikeFx.Ring(pos, 4f, new Color(0.6f, 0.9f, 1f), 0.4f);
            Sfx.Play(SfxId.PrizeFanfare, 0.4f);
        }

        void Update()
        {
            if (IsBeaten) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            var player = ThePlayer();
            Common(dt);
            transform.position = pos;
            knot.localRotation = Quaternion.Euler(Mathf.Sin(t * 0.5f) * 12f, t * 14f, Mathf.Cos(t * 0.4f) * 12f);
            murk.localScale = Vector3.one * (9f + Mathf.Sin(t * 0.9f) * 0.5f) * Mathf.Lerp(0.6f, 1f, Health / (float)MaxHealth);
            for (int i = 0; i < ropes.Length; i++)
                ropes[i].localScale = new Vector3(1f, 1f + Mathf.Sin(t * 1.3f + i) * 0.04f, 1f);

            bool near = false;
            if (player != null)
            {
                Vector3 d = player.Motor.Center - pos;
                near = d.sqrMagnitude < 100f;
                if (d.sqrMagnitude > 0.1f) eye.rotation = Quaternion.Slerp(eye.rotation, Quaternion.LookRotation(d.normalized, Vector3.up), 1f - Mathf.Exp(-3f * dt));
            }
            if (swell >= 0f)
            {
                // Swelling up... and bang.
                swell += dt / 1.15f;
                float k = Mathf.Clamp01(swell);
                knot.localScale = Vector3.one * (1f + k * 0.3f + Mathf.Sin(k * 40f) * 0.03f * k);
                eye.localScale = Vector3.one * (1f + k * 0.5f);
                if (swell >= 1f)
                {
                    swell = -1f;
                    pulse = Random.Range(3.4f, 4.6f);
                    StrikeFx.Ring(pos, ShockRadius, SnarlKnot.FrayGlow, 0.3f);
                    StrikeFx.Ring(pos + Vector3.up * 1.2f, ShockRadius * 0.7f, SnarlKnot.FrayGlow, 0.24f);
                    StrikeFx.Ring(pos + Vector3.down * 1.2f, ShockRadius * 0.7f, SnarlKnot.FrayGlow, 0.24f);
                    Sfx.PlayNear(SfxId.Steam, pos, 0.6f, 40f);
                    if (player != null && (player.Motor.Center - pos).magnitude < ShockRadius && player.Game != null) player.Game.Hurt(pos);
                }
            }
            else
            {
                knot.localScale = Vector3.Lerp(knot.localScale, Vector3.one * (1f + Mathf.Sin(t * 2.2f) * 0.03f), 1f - Mathf.Exp(-8f * dt));
                eye.localScale = Vector3.Lerp(eye.localScale, Vector3.one, 1f - Mathf.Exp(-8f * dt));
                if (near)
                {
                    pulse -= dt;
                    if (pulse <= 0f) { swell = 0f; Sfx.PlayNear(SfxId.Lever, pos, 0.4f, 40f); }
                }
            }
            Contact(player, 1.5f, 1.6f, true);
        }
    }

    // ------------------------------------------------------------------ Loose in the galleries

    // A chessling: one of the little walrus-ivory chessmen, out of its case and hopping mad.
    // One hit (or a stomp) pops it.
    public class Chessling : MuseumFoe
    {
        Vector3 home, hopFrom, hopTo;
        float hop = -1f, rest, squash;
        Transform body;

        public static Chessling Create(Transform parent, Vector3 ground, bool bishop)
        {
            var go = new GameObject("Chessling");
            go.transform.SetParent(parent, false);
            go.transform.position = ground;
            var c = go.AddComponent<Chessling>();
            c.home = ground;
            c.pos = ground;
            c.rest = Random.Range(0.3f, 1.2f);
            c.MaxHealth = 1;
            c.Score = 150;
            c.centreY = 0.6f;
            var ivory = PatchworkAssets.Felt(new Color(0.86f, 0.82f, 0.72f));
            c.rig = Pivot(go.transform, "Rig", Vector3.zero);
            c.body = Pivot(c.rig, "Body", Vector3.zero);
            P(PrimitiveType.Cylinder, c.body, new Vector3(0f, 0.08f, 0f), new Vector3(0.7f, 0.08f, 0.7f), ivory);
            P(PrimitiveType.Capsule, c.body, new Vector3(0f, 0.5f, 0f), new Vector3(0.5f, 0.42f, 0.5f), ivory);
            P(PrimitiveType.Cylinder, c.body, new Vector3(0f, 0.86f, 0f), new Vector3(0.52f, 0.04f, 0.52f), ivory);
            P(PrimitiveType.Sphere, c.body, new Vector3(0f, 1.08f, 0f), Vector3.one * 0.44f, ivory);
            if (bishop) P(PrimitiveType.Sphere, c.body, new Vector3(0f, 1.36f, 0f), new Vector3(0.22f, 0.4f, 0.22f), ivory);
            else P(PrimitiveType.Cube, c.body, new Vector3(0f, 1.32f, 0f), new Vector3(0.34f, 0.08f, 0.34f), ivory);
            // The worried face they all have, and the Fray looking out of it.
            for (int s = -1; s <= 1; s += 2)
            {
                P(PrimitiveType.Sphere, c.body, new Vector3(s * 0.1f, 1.1f, 0.19f), new Vector3(0.12f, 0.14f, 0.06f), PatchworkAssets.Plain(new Color(0.05f, 0.04f, 0.07f), 0.9f));
                P(PrimitiveType.Sphere, c.body, new Vector3(s * 0.1f, 1.1f, 0.215f), new Vector3(0.06f, 0.06f, 0.04f), Monsters.Glow);
            }
            Tuft(c.body, new Vector3(0f, 0.86f, 0f), 2.2f, ground.x);
            Tuft(c.body, new Vector3(0.1f, 0.2f, -0.2f), 1.8f, ground.z);
            c.Register(0.55f);
            return c;
        }

        void Update()
        {
            if (IsBeaten) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            var player = ThePlayer();
            bool free = Common(dt);
            squash = Mathf.MoveTowards(squash, 0f, dt * 4f);
            if (hop >= 0f)
            {
                hop += dt / 0.5f;
                float k = Mathf.Clamp01(hop);
                pos = Vector3.Lerp(hopFrom, hopTo, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.3f;
                if (k >= 1f) { hop = -1f; rest = Random.Range(0.3f, 0.6f); squash = 1f; Sfx.PlayNear(SfxId.Clack, pos, 0.2f, 16f); }
            }
            else if (free)
            {
                rest -= dt;
                if (rest <= 0f)
                {
                    Vector3 target = home + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
                    if (player != null)
                    {
                        Vector3 pp = player.transform.position;
                        if ((pp - pos).sqrMagnitude < 121f && (pp - home).sqrMagnitude < 256f && Mathf.Abs(pp.y - pos.y) < 2.5f)
                            target = pos + Vector3.ClampMagnitude(new Vector3(pp.x - pos.x, 0f, pp.z - pos.z), 2.6f);
                    }
                    target.y = Monsters.Ground(target + Vector3.up * 0.3f);
                    if (Mathf.Abs(target.y - pos.y) > 1f || Physics.CheckSphere(target + Vector3.up * 0.8f, 0.4f, ~(1 << 2), QueryTriggerInteraction.Ignore)) target = pos;
                    hopFrom = pos;
                    hopTo = target;
                    hop = 0f;
                }
            }
            transform.position = pos;
            if (player != null)
            {
                Vector3 f = player.transform.position - pos;
                f.y = 0f;
                if (f.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(f.normalized, Vector3.up);
            }
            float sq = Mathf.Sin(squash * Mathf.PI) * 0.3f;
            body.localScale = new Vector3(1f + sq, 1f - sq, 1f + sq);
            Contact(player, 0.9f, 1.2f, true);
        }
    }

    // The Helmet Crab: an old brass diving helmet with the Fray inside it, up on six scaly
    // legs, with the telescope's eyepiece held tight in one claw. It runs. Corner it and it
    // turns and charges; that's the moment. Nothing gets through the brass but the third hit
    // of the three, which tips it on its back, and then anything does.
    public class HelmetCrab : MuseumFoe
    {
        enum State { Lurk, Startle, Circle, Rear, Charge, Dizzy }
        public System.Action<HelmetCrab> Fled;
        public bool Standing { get; private set; }   // true at the last place: no more running

        State state;
        float st, cooldown = 2f, walk;
        Vector3 facing = Vector3.forward, spot, chargeDir;
        bool strafeLeft, bit;
        Transform shell, eyes, clawL, clawR;
        Transform[] legs;

        public static HelmetCrab Create(Transform parent)
        {
            var go = new GameObject("HelmetCrab");
            go.transform.SetParent(parent, false);
            var c = go.AddComponent<HelmetCrab>();
            c.MaxHealth = 6;
            c.Score = 1000;
            c.centreY = 0.95f;
            c.knockScale = 0.5f;
            var brass = PatchworkAssets.Plain(new Color(0.78f, 0.58f, 0.28f), 0.85f);
            var copper = PatchworkAssets.Plain(new Color(0.7f, 0.42f, 0.28f), 0.8f);
            var dark = PatchworkAssets.Plain(new Color(0.05f, 0.04f, 0.07f), 0.9f);
            var scale = PatchworkAssets.Plain(new Color(0.5f, 0.45f, 0.42f), 0.7f);
            c.rig = Pivot(go.transform, "Rig", Vector3.zero);
            c.shell = Pivot(c.rig, "Shell", new Vector3(0f, 0.95f, 0f));
            P(PrimitiveType.Sphere, c.shell, Vector3.zero, new Vector3(1.3f, 1.25f, 1.3f), brass);
            P(PrimitiveType.Cylinder, c.shell, new Vector3(0f, -0.52f, 0f), new Vector3(1.5f, 0.1f, 1.5f), copper);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI / 4f;
                P(PrimitiveType.Sphere, c.shell, new Vector3(Mathf.Cos(a) * 0.7f, -0.46f, Mathf.Sin(a) * 0.7f), Vector3.one * 0.12f, brass);
            }
            // Three round windows: front and both sides. The front one has the lights in it.
            P(PrimitiveType.Cylinder, c.shell, new Vector3(0f, 0.05f, 0.6f), new Vector3(0.62f, 0.06f, 0.62f), copper, new Vector3(90f, 0f, 0f));
            P(PrimitiveType.Cylinder, c.shell, new Vector3(0f, 0.05f, 0.635f), new Vector3(0.48f, 0.04f, 0.48f), dark, new Vector3(90f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                P(PrimitiveType.Cylinder, c.shell, new Vector3(s * 0.6f, 0.05f, 0f), new Vector3(0.46f, 0.05f, 0.46f), copper, new Vector3(0f, 0f, 90f));
                P(PrimitiveType.Cylinder, c.shell, new Vector3(s * 0.63f, 0.05f, 0f), new Vector3(0.34f, 0.04f, 0.34f), dark, new Vector3(0f, 0f, 90f));
            }
            for (int i = -1; i <= 1; i++) P(PrimitiveType.Cube, c.shell, new Vector3(i * 0.13f, 0.05f, 0.67f), new Vector3(0.03f, 0.46f, 0.03f), brass);
            c.eyes = Pivot(c.shell, "Eyes", new Vector3(0f, 0.07f, 0.66f));
            for (int s = -1; s <= 1; s += 2) P(PrimitiveType.Sphere, c.eyes, new Vector3(s * 0.11f, 0f, 0f), new Vector3(0.1f, 0.12f, 0.05f), Monsters.Glow);
            // The air pipe on top, with a grey tangle growing out of it.
            P(PrimitiveType.Cylinder, c.shell, new Vector3(0f, 0.66f, -0.1f), new Vector3(0.2f, 0.1f, 0.2f), copper);
            Tuft(c.shell, new Vector3(0f, 0.8f, -0.1f), 3f, 2f);
            Tuft(c.shell, new Vector3(0f, -0.5f, -0.5f), 2.4f, 5f);
            // Six legs, three a side.
            c.legs = new Transform[6];
            for (int i = 0; i < 6; i++)
            {
                float sx = i < 3 ? -1f : 1f;
                int n = i % 3;
                c.legs[i] = Pivot(c.rig, "Leg", new Vector3(sx * 0.55f, 0.6f, (n - 1) * 0.42f));
                P(PrimitiveType.Capsule, c.legs[i], new Vector3(sx * 0.3f, 0.06f, 0f), new Vector3(0.12f, 0.34f, 0.12f), scale, new Vector3(0f, 0f, sx * -62f));
                P(PrimitiveType.Capsule, c.legs[i], new Vector3(sx * 0.62f, -0.26f, 0f), new Vector3(0.1f, 0.36f, 0.1f), scale, new Vector3(0f, 0f, sx * 14f));
                P(PrimitiveType.Sphere, c.legs[i], new Vector3(sx * 0.68f, -0.58f, 0.04f), new Vector3(0.2f, 0.1f, 0.26f), scale);
            }
            // Claws: the right one has the eyepiece.
            c.clawL = Pivot(c.rig, "ClawL", new Vector3(-0.5f, 0.7f, 0.55f));
            c.clawR = Pivot(c.rig, "ClawR", new Vector3(0.5f, 0.7f, 0.55f));
            foreach (var claw in new[] { c.clawL, c.clawR })
            {
                P(PrimitiveType.Capsule, claw, new Vector3(0f, 0f, 0.22f), new Vector3(0.13f, 0.26f, 0.13f), scale, new Vector3(90f, 0f, 0f));
                P(PrimitiveType.Sphere, claw, new Vector3(0f, 0.06f, 0.52f), new Vector3(0.2f, 0.12f, 0.34f), scale);
                P(PrimitiveType.Sphere, claw, new Vector3(0f, -0.06f, 0.5f), new Vector3(0.16f, 0.09f, 0.28f), scale);
            }
            P(PrimitiveType.Cylinder, c.clawR, new Vector3(0f, 0.2f, 0.56f), new Vector3(0.3f, 0.07f, 0.3f), brass);
            P(PrimitiveType.Cylinder, c.clawR, new Vector3(0f, 0.2f, 0.56f), new Vector3(0.22f, 0.075f, 0.22f), PatchworkAssets.Glow(new Color(0.6f, 0.9f, 1f), 1.6f));
            c.Register(0.9f);
            go.SetActive(false);
            return c;
        }

        // Turns up somewhere. "stand" is the last place: there it stops running and fights.
        public void Appear(Vector3 at, bool stand)
        {
            Restore(at);
            spot = at;
            Standing = stand;
            state = State.Lurk;
            st = 0f;
            cooldown = 1.5f;
            gameObject.SetActive(true);
            FxBurst.Spawn(FxBurst.Kind.Threads, at + Vector3.up, SnarlKnot.FrayGrey);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        protected override bool Blocks(Vector3 from, bool finisher)
        {
            if (state == State.Dizzy) return false;
            if (finisher) return false;
            return true;
        }

        protected override void OnBlocked(bool finisher)
        {
            // Being rapped on the helmet makes it bolt (or, if it's cornered, cross).
            if (!Standing && state == State.Lurk) { state = State.Startle; st = 0f; }
            else if (Standing && state == State.Lurk) { state = State.Circle; st = 0f; }
            else cooldown = Mathf.Min(cooldown, 0.4f);
        }

        protected override void OnHurt(bool finisher)
        {
            if (!Standing) { state = State.Startle; st = 0f; stun = 0f; return; }
            if (finisher && state != State.Dizzy) { state = State.Dizzy; st = 0f; Sfx.PlayNear(SfxId.Clack, pos, 0.6f, 30f); }
        }

        bool TryMove(Vector3 next)
        {
            Vector3 flat = next - spot;
            flat.y = 0f;
            if (flat.magnitude > 11f) return false;
            float g = Monsters.Ground(next + Vector3.up * 0.4f);
            if (Mathf.Abs(g - pos.y) > 0.7f) return false;
            if (Physics.CheckSphere(new Vector3(next.x, g + 1.1f, next.z), 0.7f, ~(1 << 2), QueryTriggerInteraction.Ignore)) return false;
            pos = new Vector3(next.x, g, next.z);
            return true;
        }

        void Update()
        {
            if (IsBeaten) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            var player = ThePlayer();
            Vector3 keep = pos;
            bool free = Common(dt);
            // (Being knocked back mustn't push it through a wall.)
            if (pos != keep) { Vector3 want = pos; pos = keep; TryMove(want); }
            st += dt;
            Vector3 toP = player != null ? player.transform.position - pos : facing;
            float dy = toP.y;
            toP.y = 0f;
            float dist = toP.magnitude;
            Vector3 dirP = dist > 0.01f ? toP / dist : facing;
            float tilt = 0f, roll = 0f, lift = 0f;
            float legSpeed = 0f;

            switch (state)
            {
                case State.Lurk:
                    facing = Vector3.Slerp(facing, Quaternion.Euler(0f, Mathf.Sin(t * 0.7f) * 70f, 0f) * (dist < 14f ? dirP : facing), 1f - Mathf.Exp(-3f * dt));
                    if (player != null && dist < (Standing ? 9f : 6.5f) && Mathf.Abs(dy) < 4f)
                    {
                        state = Standing ? State.Circle : State.Startle;
                        st = 0f;
                        Sfx.PlayNear(SfxId.Squeak, pos, 0.5f, 30f);
                        FxBurst.Spawn(FxBurst.Kind.Sparkle, pos + Vector3.up * 1.8f, SnarlKnot.FrayGlow);
                    }
                    break;

                case State.Startle:
                    // A jump, a scrabble backwards, and it's gone through a seam in the floor.
                    lift = Mathf.Sin(Mathf.Clamp01(st / 0.3f) * Mathf.PI) * 0.6f;
                    facing = Vector3.Slerp(facing, dirP, 1f - Mathf.Exp(-12f * dt));
                    if (st > 0.3f) { TryMove(pos - dirP * 7f * dt); legSpeed = 3f; }
                    if (st > 1.1f)
                    {
                        FxBurst.Spawn(FxBurst.Kind.Threads, pos + Vector3.up * 0.6f, SnarlKnot.FrayGrey);
                        FxBurst.Spawn(FxBurst.Kind.Poof, pos + Vector3.up * 0.6f, new Color(0.6f, 0.58f, 0.66f));
                        Sfx.PlayNear(SfxId.Unravel, pos, 0.5f, 30f);
                        gameObject.SetActive(false);
                        if (Fled != null) Fled(this);
                        return;
                    }
                    break;

                case State.Circle:
                    if (!free || player == null) break;
                    facing = Vector3.Slerp(facing, dirP, 1f - Mathf.Exp(-7f * dt));
                    Vector3 side = Vector3.Cross(Vector3.up, dirP) * (strafeLeft ? -1f : 1f);
                    Vector3 step = side * 3.2f + dirP * (dist > 6.5f ? 2.5f : dist < 4f ? -2.5f : 0f);
                    if (!TryMove(pos + step * dt)) strafeLeft = !strafeLeft;
                    legSpeed = 2.4f;
                    cooldown -= dt;
                    if (dist > 16f) { state = State.Lurk; st = 0f; }
                    else if (cooldown <= 0f && dist < 11f) { state = State.Rear; st = 0f; Sfx.PlayNear(SfxId.Lever, pos, 0.5f, 30f); }
                    break;

                case State.Rear:
                    // Up on its back legs, claws wide: here it comes.
                    facing = Vector3.Slerp(facing, dirP, 1f - Mathf.Exp(-5f * dt));
                    tilt = -30f * Mathf.Clamp01(st / 0.3f);
                    if (st >= 0.7f) { state = State.Charge; st = 0f; chargeDir = facing; bit = false; Sfx.PlayNear(SfxId.Whoosh, pos, 0.4f, 30f); }
                    break;

                case State.Charge:
                    tilt = 14f;
                    legSpeed = 5f;
                    bool blocked = !TryMove(pos + chargeDir * 12f * dt);
                    if (!bit && player != null && dist < 1.5f && Mathf.Abs(dy) < 1.6f && !SnarlKnot.HitHard(player, Center))
                    {
                        bit = true;
                        player.Game.Hurt(Center);
                    }
                    if (blocked || st > 0.85f)
                    {
                        // It runs out of floor (or into something) and goes over on its back.
                        state = State.Dizzy;
                        st = 0f;
                        Sfx.PlayNear(SfxId.Clack, pos, blocked ? 0.7f : 0.4f, 30f);
                        if (blocked) FxBurst.Spawn(FxBurst.Kind.Sparkle, pos + chargeDir + Vector3.up, new Color(1f, 0.95f, 0.7f));
                    }
                    break;

                case State.Dizzy:
                    roll = 165f * Mathf.Clamp01(st / 0.25f);
                    legSpeed = 6f;
                    if (st > 2.2f) { state = State.Circle; st = 0f; cooldown = Random.Range(1.6f, 2.6f); }
                    break;
            }

            transform.position = pos + Vector3.up * lift;
            transform.rotation = Quaternion.LookRotation(new Vector3(facing.x, 0f, facing.z).normalized, Vector3.up);
            float ease = 1f - Mathf.Exp(-12f * dt);
            shell.localRotation = Quaternion.Slerp(shell.localRotation, Quaternion.Euler(tilt + Mathf.Sin(t * 2f) * 3f, 0f, roll), ease);
            shell.localPosition = new Vector3(0f, 0.95f + Mathf.Sin(t * 4f) * 0.03f - (state == State.Dizzy ? 0.2f : 0f), 0f);
            walk += legSpeed * dt * 6f;
            for (int i = 0; i < legs.Length; i++)
            {
                float ph = walk + i * 2.1f;
                bool up = state == State.Dizzy;
                legs[i].localRotation = Quaternion.Euler(Mathf.Sin(ph) * (legSpeed > 0f ? 26f : 3f), 0f, up ? (i < 3 ? -140f : 140f) + Mathf.Sin(ph * 1.7f) * 22f : Mathf.Cos(ph) * (legSpeed > 0f ? 10f : 2f));
            }
            float snap = state == State.Rear ? 50f : state == State.Charge ? 20f : 6f + Mathf.Sin(t * 3f) * 6f;
            clawL.localRotation = Quaternion.Slerp(clawL.localRotation, Quaternion.Euler(-snap, -snap * 0.8f, 0f), ease);
            clawR.localRotation = Quaternion.Slerp(clawR.localRotation, Quaternion.Euler(-snap, snap * 0.8f, 0f), ease);
            float flare = state == State.Rear ? 1.7f + Mathf.Sin(st * 30f) * 0.4f : state == State.Dizzy ? 0.4f : 1f;
            eyes.localScale = Vector3.Lerp(eyes.localScale, Vector3.one * flare, ease);
            if (state != State.Startle) Contact(player, 1f, 1.5f, false);
        }
    }

    // ------------------------------------------------------------------ The lenses

    // One of the three lenses of the Night Glass: a disc of blue glass in a brass ring, the
    // size of a dinner plate. It turns slowly and hums until Patch picks it up.
    public class LensPickup : PlayerVolume
    {
        public System.Action Taken;
        Transform visual;
        Vector3 home, vel;
        bool dropping;
        float t;

        public static LensPickup Spawn(Transform parent, Vector3 at, bool hop)
        {
            var go = new GameObject("LensPickup");
            go.transform.SetParent(parent, false);
            go.transform.position = at;
            var l = go.AddComponent<LensPickup>();
            l.Size = Vector3.one * 2f;
            l.home = at;
            l.visual = new GameObject("Lens").transform;
            l.visual.SetParent(go.transform, false);
            var brass = PatchworkAssets.Plain(new Color(0.85f, 0.65f, 0.3f), 0.85f);
            Monsters.Part(PrimitiveType.Cylinder, l.visual, Vector3.zero, new Vector3(1f, 0.05f, 1f), brass, new Vector3(90f, 0f, 0f));
            Monsters.Part(PrimitiveType.Cylinder, l.visual, Vector3.zero, new Vector3(0.84f, 0.06f, 0.84f), PatchworkAssets.Glow(new Color(0.55f, 0.85f, 1f), 1.3f), new Vector3(90f, 0f, 0f));
            Monsters.Part(PrimitiveType.Sphere, l.visual, new Vector3(-0.16f, 0.18f, 0.04f), new Vector3(0.2f, 0.12f, 0.03f), PatchworkAssets.Glow(Color.white, 1.2f), new Vector3(0f, 0f, 35f));
            Monsters.Part(PrimitiveType.Sphere, l.visual, new Vector3(-0.16f, 0.18f, -0.04f), new Vector3(0.2f, 0.12f, 0.03f), PatchworkAssets.Glow(Color.white, 1.2f), new Vector3(0f, 0f, 35f));
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI / 4f;
                Monsters.Part(PrimitiveType.Sphere, l.visual, new Vector3(Mathf.Cos(a) * 0.5f, Mathf.Sin(a) * 0.5f, 0f), Vector3.one * 0.09f, brass);
            }
            if (hop)
            {
                l.dropping = true;
                l.vel = new Vector3(0f, 7f, 0f);
            }
            return l;
        }

        protected override void OnPlayerStay(PatchworkPlayer player, float dt)
        {
            if (dropping) return;
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position, new Color(0.6f, 0.9f, 1f));
            var taken = Taken;
            Destroy(gameObject);
            if (taken != null) taken();
        }

        void Update()
        {
            float dt = Time.deltaTime;
            t += dt;
            if (dropping)
            {
                vel.y -= 16f * dt;
                transform.position += vel * dt;
                if (vel.y < 0f && transform.position.y <= home.y) { transform.position = home; dropping = false; }
            }
            else transform.position = home + Vector3.up * Mathf.Sin(t * 2.2f) * 0.15f;
            visual.localRotation = Quaternion.Euler(0f, t * 90f, 0f);
        }
    }
}

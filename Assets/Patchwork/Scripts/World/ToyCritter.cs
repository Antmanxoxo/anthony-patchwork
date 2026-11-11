using UnityEngine;

namespace Patchwork
{
    // Toy wildlife on the outskirts of town: wind-up mice, felt bunnies, wooden deer on wheels,
    // pull-along ducks, woolly sheep, pincushion hedgehogs, button snails and felt frogs. They
    // potter about near home, stop to nibble or look around, and scurry off (with a squeak) if
    // Patch runs up to them. Bump right into one and it gives a startled little hop.
    public class ToyCritter : MonoBehaviour
    {
        public enum Kind { Mouse, Bunny, Deer, Duck, Sheep, Hedgehog, Snail, Frog, Fox }

        public Kind Type;
        public Vector3 Home;
        public float Radius = 8f, Speed = 1.2f, HopHeight, HopRate = 6f, Shy = 4.5f;
        public Transform Body, Head, Key;
        public Transform[] Wheels, Legs, Ears;
        public float Pitch = 1f;

        Vector3 target;
        float wait, phase, fleeing, squeakCool, startle, headT, groundY, yaw;
        int mask;

        void Start()
        {
            mask = ~(1 << 2);
            target = transform.position;
            groundY = transform.position.y;
            yaw = transform.eulerAngles.y;
            wait = Random.Range(0.5f, 3f);
            phase = Random.value * 10f;
            headT = Random.value * 10f;
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) return;
            Vector3 me = transform.position;
            Vector3 pp = game.Player.Motor.Center;
            float far = (pp - me).sqrMagnitude;
            if (far > 90f * 90f) return;   // too far away to see: don't bother
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            squeakCool -= dt;
            headT += dt;

            Vector3 flat = new Vector3(pp.x - me.x, 0f, pp.z - me.z);
            float dist = flat.magnitude;
            // Startled when bumped.
            if (dist < 0.9f && Mathf.Abs(pp.y - me.y) < 1.6f && startle <= 0f)
            {
                startle = 1f;
                Squeak(1.25f);
                FxBurst.Spawn(FxBurst.Kind.Puff, me + Vector3.up * 0.3f, Color.white);
            }
            // Scurry away from Patch if he comes running up.
            if (Shy > 0f && dist < Shy && fleeing <= 0f)
            {
                Vector3 away = dist > 0.01f ? -flat / dist : Random.insideUnitSphere;
                away.y = 0f;
                Vector3 t = me + away.normalized * Random.Range(4f, 6f);
                Vector3 fromHome = t - Home;
                fromHome.y = 0f;
                if (fromHome.magnitude > Radius * 1.4f) t = Home + fromHome.normalized * Radius * 1.4f;
                target = new Vector3(t.x, me.y, t.z);
                fleeing = 1.6f;
                wait = 0f;
                if (squeakCool <= 0f) Squeak(1f);
            }
            fleeing -= dt;

            Vector3 to = target - me;
            to.y = 0f;
            bool moving = false;
            if (wait > 0f)
            {
                wait -= dt;
                if (wait <= 0f) PickTarget();
            }
            else if (to.magnitude < 0.3f)
            {
                wait = Random.Range(1.5f, 5f);
            }
            else
            {
                Vector3 dir = to.normalized;
                float spd = Speed * (fleeing > 0f ? 2.3f : 1f);
                // Bumped into something, or about to walk off an edge (a ravine, the canal, a
                // drop)? Go somewhere else. They only ever step where there's ground close under
                // the next step.
                if (Physics.SphereCast(me + Vector3.up * 0.35f, 0.25f, dir, out RaycastHit hit, 0.7f, mask, QueryTriggerInteraction.Ignore)
                    || !GroundAhead(me + dir * 0.6f))
                {
                    PickTarget();
                }
                else
                {
                    me += dir * spd * dt;
                    moving = true;
                    float want = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                    yaw = Mathf.MoveTowardsAngle(yaw, want, 360f * dt);
                }
            }
            // Follow the ground.
            if (Physics.Raycast(me + Vector3.up * 1.5f, Vector3.down, out RaycastHit g, 4f, mask, QueryTriggerInteraction.Ignore))
            {
                if (g.point.y - groundY < 0.5f) groundY = Mathf.Lerp(groundY, g.point.y, 1f - Mathf.Exp(-12f * dt));
                else PickTarget();   // a step too high for a little toy
            }
            else
            {
                // Nothing underneath at all (it was pushed or fled over an edge): back to where
                // it was standing last.
                me = transform.position;
                PickTarget();
            }
            me.y = groundY;
            transform.position = me;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            Animate(moving, dt);
        }

        bool GroundAhead(Vector3 p)
        {
            RaycastHit g;
            if (!Physics.Raycast(p + Vector3.up * 1.5f, Vector3.down, out g, 2.4f, mask, QueryTriggerInteraction.Ignore)) return false;
            return g.point.y > groundY - 0.6f && g.normal.y > 0.6f;
        }

        void PickTarget()
        {
            Vector2 r = Random.insideUnitCircle * Radius;
            target = new Vector3(Home.x + r.x, transform.position.y, Home.z + r.y);
            wait = 0f;
        }

        // A little squeak (or a frog's boing), soft, and softer the further away Patch is.
        void Squeak(float pitchMul)
        {
            squeakCool = Random.Range(5f, 8f);
            var game = PatchworkGame.Instance;
            float dist = game != null && game.Player != null ? Vector3.Distance(game.Player.Motor.Center, transform.position) : 0f;
            float near = Mathf.Clamp01(1f - dist / 10f);
            if (near <= 0.05f) return;
            var clip = Type == Kind.Duck ? SfxId.Squeak : Type == Kind.Frog ? SfxId.Boing : SfxId.Critter;
            float vol = (Type == Kind.Frog ? 0.05f : 0.07f) * near;
            Sfx.PlayPitched(clip, vol, Pitch * pitchMul * Random.Range(0.92f, 1.08f));
        }

        void Animate(bool moving, float dt)
        {
            startle = Mathf.MoveTowards(startle, 0f, dt * 2.5f);
            if (moving) phase += dt * HopRate * (fleeing > 0f ? 1.6f : 1f);
            float hop = moving && HopHeight > 0f ? Mathf.Abs(Mathf.Sin(phase)) * HopHeight : 0f;
            hop += Mathf.Sin(startle * Mathf.PI) * 0.45f;
            if (Body != null)
            {
                float breathe = 1f + Mathf.Sin(Time.time * 2.2f + Home.x) * 0.02f;
                float squash = HopHeight > 0f && moving ? 1f + Mathf.Sin(phase * 2f) * 0.06f : 1f;
                Body.localPosition = new Vector3(0f, hop, 0f);
                Body.localScale = new Vector3(1f / Mathf.Sqrt(squash), squash * breathe, 1f / Mathf.Sqrt(squash));
                // A little waddle for wheeled and wind-up toys.
                float roll = moving && HopHeight <= 0f ? Mathf.Sin(phase * 2f) * 4f : 0f;
                Body.localRotation = Quaternion.Euler(0f, 0f, roll);
            }
            if (Wheels != null)
                foreach (var w in Wheels) if (w != null) w.Rotate(Vector3.right, (moving ? Speed * (fleeing > 0f ? 2.3f : 1f) : 0f) * 360f * dt / 0.9f, Space.Self);
            if (Key != null) Key.Rotate(Vector3.forward, (moving ? 420f : 60f) * dt, Space.Self);
            if (Legs != null)
                for (int i = 0; i < Legs.Length; i++)
                    if (Legs[i] != null) Legs[i].localRotation = Quaternion.Euler(moving ? Mathf.Sin(phase + i * Mathf.PI) * 28f : 0f, 0f, 0f);
            if (Head != null)
            {
                // Grazing / looking about while standing still.
                float nibble = !moving && Type == Kind.Sheep || !moving && Type == Kind.Deer ? Mathf.Max(0f, Mathf.Sin(headT * 0.7f)) * 35f : 0f;
                float look = !moving ? Mathf.Sin(headT * 0.9f) * 25f : 0f;
                Head.localRotation = Quaternion.Euler(nibble, look, 0f);
            }
            if (Ears != null)
                for (int i = 0; i < Ears.Length; i++)
                    if (Ears[i] != null) Ears[i].localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 3f + i) * 8f + startle * -30f, 0f, (i == 0 ? -1f : 1f) * 8f);
        }

        // ------------------------------------------------------------------ Builders

        static ToyCritter Make(LevelKit kit, Kind kind, Vector3 feet, float radius, out Transform body)
        {
            var g = kit.Group("Critter_" + kind, feet);
            g.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            body = new GameObject("Body").transform;
            body.SetParent(g, false);
            var c = g.gameObject.AddComponent<ToyCritter>();
            c.Type = kind;
            c.Home = feet;
            c.Radius = radius;
            c.Body = body;
            return c;
        }

        static Transform Pivot(Transform parent, Vector3 local)
        {
            var t = new GameObject("Pivot").transform;
            t.SetParent(parent, false);
            t.localPosition = local;
            return t;
        }

        static void Eyes(LevelKit kit, Transform head, Vector3 at, float spacing, float size)
        {
            var white = PatchworkAssets.Felt(Color.white);
            var black = PatchworkAssets.Plain(new Color(0.08f, 0.07f, 0.1f), 0.85f);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Sphere, at + new Vector3(s * spacing, 0f, 0f), Vector3.one * size, white, default(Vector3), head);
                kit.Deco(PrimitiveType.Sphere, at + new Vector3(s * spacing, 0.01f, size * 0.32f), Vector3.one * size * 0.55f, black, default(Vector3), head);
            }
        }

        // A clockwork mouse: a grey felt teardrop with round ears, a pink tail and a turning key.
        public static ToyCritter Mouse(LevelKit kit, Vector3 feet)
        {
            var c = Make(kit, Kind.Mouse, feet, 6f, out Transform b);
            c.Speed = 2.2f; c.Shy = 5f; c.Pitch = 1.35f; c.HopRate = 14f;
            var grey = PatchworkAssets.Felt(new Color(0.68f, 0.66f, 0.7f));
            var pink = PatchworkAssets.Felt(new Color(1f, 0.65f, 0.72f));
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.2f, 0f), new Vector3(0.38f, 0.32f, 0.6f), grey, default(Vector3), b);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.2f, 0.32f), new Vector3(0.16f, 0.16f, 0.2f), grey, default(Vector3), b);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.21f, 0.43f), Vector3.one * 0.06f, pink, default(Vector3), b);
            var ears = new Transform[2];
            for (int s = 0; s < 2; s++)
            {
                ears[s] = Pivot(b, new Vector3((s == 0 ? -1f : 1f) * 0.1f, 0.34f, 0.18f));
                kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.16f, 0.01f, 0.16f), pink, new Vector3(80f, 0f, 0f), ears[s]);
            }
            c.Ears = ears;
            Eyes(kit, b, new Vector3(0f, 0.27f, 0.3f), 0.06f, 0.06f);
            for (int k = 0; k < 4; k++)
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.16f + Mathf.Sin(k) * 0.03f, -0.32f - k * 0.08f), Vector3.one * 0.04f, pink, default(Vector3), b);
            c.Key = Pivot(b, new Vector3(0f, 0.38f, -0.08f));
            var brass = PatchworkAssets.Plain(new Color(0.95f, 0.75f, 0.35f), 0.85f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.04f, 0f), new Vector3(0.03f, 0.05f, 0.03f), brass, default(Vector3), c.Key);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.07f, 0.1f, 0f), new Vector3(0.1f, 0.08f, 0.02f), brass, default(Vector3), c.Key);
            c.Key.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            return c;
        }

        // A felt bunny: round body, long ears, a pom-pom tail. Hops.
        public static ToyCritter Bunny(LevelKit kit, Vector3 feet, Color col)
        {
            var c = Make(kit, Kind.Bunny, feet, 9f, out Transform b);
            c.Speed = 1.8f; c.HopHeight = 0.35f; c.HopRate = 7f; c.Shy = 5.5f; c.Pitch = 1.2f;
            var felt = PatchworkAssets.Felt(col);
            var inner = PatchworkAssets.Felt(new Color(1f, 0.75f, 0.8f));
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.32f, 0f), new Vector3(0.55f, 0.5f, 0.65f), felt, default(Vector3), b);
            var head = Pivot(b, new Vector3(0f, 0.55f, 0.28f));
            c.Head = head;
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.42f, felt, default(Vector3), head);
            Eyes(kit, head, new Vector3(0f, 0.05f, 0.17f), 0.1f, 0.08f);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.04f, 0.21f), Vector3.one * 0.06f, inner, default(Vector3), head);
            var ears = new Transform[2];
            for (int s = 0; s < 2; s++)
            {
                ears[s] = Pivot(head, new Vector3((s == 0 ? -1f : 1f) * 0.09f, 0.16f, -0.02f));
                kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.2f, 0f), new Vector3(0.12f, 0.2f, 0.06f), felt, default(Vector3), ears[s]);
                kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.2f, 0.025f), new Vector3(0.07f, 0.15f, 0.03f), inner, default(Vector3), ears[s]);
            }
            c.Ears = ears;
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.36f, -0.34f), Vector3.one * 0.18f, PatchworkAssets.Yarn(Color.white), default(Vector3), b);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.16f, 0.08f, -0.08f), new Vector3(0.16f, 0.12f, 0.3f), felt, default(Vector3), b);
            return c;
        }

        // A wooden deer on a wheeled base, painted with white spots. Rolls along slowly.
        public static ToyCritter Deer(LevelKit kit, Vector3 feet)
        {
            var c = Make(kit, Kind.Deer, feet, 12f, out Transform b);
            c.Speed = 1.1f; c.Shy = 6.5f; c.Pitch = 0.85f;
            var wood = PatchworkAssets.Wood(new Color(0.78f, 0.52f, 0.32f));
            var dark = PatchworkAssets.Wood(new Color(0.5f, 0.32f, 0.2f));
            var spot = PatchworkAssets.Felt(new Color(0.99f, 0.95f, 0.88f));
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.22f, 0f), new Vector3(0.7f, 0.1f, 1.5f), dark, default(Vector3), b);
            var wheels = new Transform[4];
            int wi = 0;
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    var w = Pivot(b, new Vector3(sx * 0.4f, 0.15f, sz * 0.55f));
                    kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.3f, 0.04f, 0.3f), PatchworkAssets.Plain(new Color(0.9f, 0.3f, 0.35f), 0.6f), new Vector3(0f, 0f, 90f), w);
                    wheels[wi++] = w;
                }
            c.Wheels = wheels;
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(sx * 0.18f, 0.6f, sz * 0.42f), new Vector3(0.12f, 0.33f, 0.12f), wood, default(Vector3), b);
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 1.05f, 0f), new Vector3(0.5f, 0.6f, 0.5f), wood, new Vector3(90f, 0f, 0f), b);
            for (int k = 0; k < 5; k++)
                kit.Deco(PrimitiveType.Sphere, new Vector3((k % 2 == 0 ? 0.2f : -0.2f), 1.2f, -0.35f + k * 0.17f), new Vector3(0.1f, 0.06f, 0.1f), spot, default(Vector3), b);
            var head = Pivot(b, new Vector3(0f, 1.3f, 0.5f));
            c.Head = head;
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.25f, 0.05f), new Vector3(0.22f, 0.28f, 0.22f), wood, new Vector3(20f, 0f, 0f), head);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.52f, 0.18f), new Vector3(0.32f, 0.3f, 0.42f), wood, default(Vector3), head);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.5f, 0.4f), Vector3.one * 0.1f, PatchworkAssets.Plain(new Color(0.1f, 0.08f, 0.1f), 0.8f), default(Vector3), head);
            Eyes(kit, head, new Vector3(0f, 0.6f, 0.28f), 0.1f, 0.07f);
            var antler = PatchworkAssets.Wood(new Color(0.95f, 0.88f, 0.72f));
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.1f, 0.85f, 0.12f), new Vector3(0.04f, 0.18f, 0.04f), antler, new Vector3(0f, 0f, -s * 20f), head);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.2f, 0.98f, 0.12f), new Vector3(0.035f, 0.1f, 0.035f), antler, new Vector3(0f, 0f, -s * 60f), head);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.19f, 0.62f, 0.1f), new Vector3(0.14f, 0.08f, 0.04f), wood, new Vector3(0f, 0f, s * 30f), head);
            }
            // A pull-string trailing behind.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.2f, -1.05f), new Vector3(0.02f, 0.3f, 0.02f), PatchworkAssets.Yarn(new Color(0.9f, 0.3f, 0.35f)), new Vector3(80f, 0f, 0f), b);
            return c;
        }

        // A pull-along duck on wheels, with a red cord.
        public static ToyCritter Duck(LevelKit kit, Vector3 feet)
        {
            var c = Make(kit, Kind.Duck, feet, 7f, out Transform b);
            c.Speed = 1.3f; c.Shy = 4f; c.Pitch = 1.1f;
            var yellow = PatchworkAssets.Plain(new Color(1f, 0.85f, 0.25f), 0.7f);
            var orange = PatchworkAssets.Plain(new Color(1f, 0.55f, 0.15f), 0.7f);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.38f, 0f), new Vector3(0.55f, 0.45f, 0.75f), yellow, default(Vector3), b);
            var head = Pivot(b, new Vector3(0f, 0.72f, 0.26f));
            c.Head = head;
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.36f, yellow, default(Vector3), head);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.04f, 0.2f), new Vector3(0.18f, 0.07f, 0.18f), orange, default(Vector3), head);
            Eyes(kit, head, new Vector3(0f, 0.06f, 0.14f), 0.09f, 0.07f);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.5f, -0.36f), new Vector3(0.2f, 0.2f, 0.16f), yellow, new Vector3(-35f, 0f, 0f), b);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.25f, 0.42f, -0.02f), new Vector3(0.08f, 0.24f, 0.38f), yellow, default(Vector3), b);
            var wheels = new Transform[2];
            for (int s = 0; s < 2; s++)
            {
                wheels[s] = Pivot(b, new Vector3((s == 0 ? -1f : 1f) * 0.26f, 0.12f, 0f));
                kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.24f, 0.03f, 0.24f), PatchworkAssets.Plain(new Color(0.3f, 0.55f, 0.9f), 0.6f), new Vector3(0f, 0f, 90f), wheels[s]);
            }
            c.Wheels = wheels;
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.3f, 0.75f), new Vector3(0.02f, 0.35f, 0.02f), PatchworkAssets.Yarn(new Color(0.9f, 0.25f, 0.3f)), new Vector3(70f, 0f, 0f), b);
            return c;
        }

        // A woolly sheep: a cloud of knitted bobbles on four little peg legs. Grazes.
        public static ToyCritter Sheep(LevelKit kit, Vector3 feet)
        {
            var c = Make(kit, Kind.Sheep, feet, 10f, out Transform b);
            c.Speed = 0.7f; c.Shy = 3.2f; c.Pitch = 0.7f; c.HopRate = 5f;
            var wool = PatchworkAssets.Knit(new Color(0.98f, 0.96f, 0.92f));
            var face = PatchworkAssets.Felt(new Color(0.25f, 0.22f, 0.26f));
            for (int k = 0; k < 9; k++)
            {
                float a = k * 40f * Mathf.Deg2Rad;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.32f, 0.72f + Mathf.Sin(k * 1.7f) * 0.1f, Mathf.Sin(a) * 0.42f), Vector3.one * 0.48f, wool, default(Vector3), b);
            }
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.8f, 0f), new Vector3(0.8f, 0.65f, 1f), wool, default(Vector3), b);
            var legs = new Transform[4];
            int li = 0;
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    legs[li] = Pivot(b, new Vector3(sx * 0.22f, 0.45f, sz * 0.3f));
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.2f, 0f), new Vector3(0.1f, 0.22f, 0.1f), face, default(Vector3), legs[li]);
                    li++;
                }
            c.Legs = legs;
            var head = Pivot(b, new Vector3(0f, 0.85f, 0.5f));
            c.Head = head;
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, 0.1f), new Vector3(0.3f, 0.34f, 0.36f), face, default(Vector3), head);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.15f, 0.02f), Vector3.one * 0.3f, wool, default(Vector3), head);
            Eyes(kit, head, new Vector3(0f, 0.04f, 0.25f), 0.08f, 0.07f);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.2f, 0.04f, 0.06f), new Vector3(0.18f, 0.06f, 0.1f), face, new Vector3(0f, 0f, s * 20f), head);
            return c;
        }

        // A pincushion hedgehog: a red tomato-cushion back stuck full of coloured pins.
        public static ToyCritter Hedgehog(LevelKit kit, Vector3 feet)
        {
            var c = Make(kit, Kind.Hedgehog, feet, 5f, out Transform b);
            c.Speed = 0.9f; c.Shy = 3f; c.Pitch = 1.4f; c.HopRate = 12f;
            var cushion = PatchworkAssets.Felt(new Color(0.88f, 0.28f, 0.3f));
            var face = PatchworkAssets.Felt(new Color(0.9f, 0.78f, 0.62f));
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.25f, 0f), new Vector3(0.6f, 0.42f, 0.7f), cushion, default(Vector3), b);
            for (int k = 0; k < 6; k++)
            {
                float a = k * 60f;
                kit.Deco(PrimitiveType.Cube, Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0.3f, 0.15f), new Vector3(0.015f, 0.4f, 0.01f), PatchworkAssets.Felt(new Color(0.45f, 0.65f, 0.3f)), new Vector3(0f, a, 0f), b);
            }
            Color[] heads = { new Color(1f, 0.85f, 0.3f), new Color(0.5f, 0.75f, 1f), Color.white, new Color(1f, 0.55f, 0.75f) };
            for (int k = 0; k < 14; k++)
            {
                Vector3 dir = Quaternion.Euler(-20f - (k % 3) * 22f, k * 51f, 0f) * Vector3.back;
                if (dir.z > 0.35f) dir.z = -dir.z;
                Vector3 at = new Vector3(dir.x * 0.3f, 0.25f + Mathf.Abs(dir.y) * 0.21f, dir.z * 0.35f);
                var pin = kit.Deco(PrimitiveType.Cylinder, at + dir * 0.08f, new Vector3(0.012f, 0.1f, 0.012f), PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.9f), 0.9f), default(Vector3), b);
                pin.localRotation = Quaternion.FromToRotation(Vector3.up, (dir + Vector3.up * 0.6f).normalized);
                kit.Deco(PrimitiveType.Sphere, at + (dir + Vector3.up * 0.6f).normalized * 0.19f, Vector3.one * 0.05f, PatchworkAssets.Plain(heads[k % heads.Length], 0.8f), default(Vector3), b);
            }
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.18f, 0.34f), new Vector3(0.24f, 0.2f, 0.28f), face, default(Vector3), b);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.18f, 0.5f), Vector3.one * 0.06f, PatchworkAssets.Plain(new Color(0.1f, 0.08f, 0.1f), 0.8f), default(Vector3), b);
            Eyes(kit, b, new Vector3(0f, 0.25f, 0.42f), 0.06f, 0.05f);
            return c;
        }

        // A snail with a big button for a shell. Very slow, not shy at all.
        public static ToyCritter Snail(LevelKit kit, Vector3 feet, Color shell)
        {
            var c = Make(kit, Kind.Snail, feet, 3f, out Transform b);
            c.Speed = 0.25f; c.Shy = 0f; c.Pitch = 1.6f;
            var body = PatchworkAssets.Felt(new Color(0.75f, 0.85f, 0.55f));
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.08f, 0.05f), new Vector3(0.18f, 0.25f, 0.12f), body, new Vector3(90f, 0f, 0f), b);
            var head = Pivot(b, new Vector3(0f, 0.12f, 0.3f));
            c.Head = head;
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.16f, body, default(Vector3), head);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.04f, 0.12f, 0f), new Vector3(0.02f, 0.07f, 0.02f), body, new Vector3(0f, 0f, -s * 15f), head);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.06f, 0.2f, 0f), Vector3.one * 0.04f, PatchworkAssets.Plain(new Color(0.1f, 0.08f, 0.1f), 0.8f), default(Vector3), head);
            }
            var btn = PatchworkAssets.Plain(shell, 0.75f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.28f, -0.02f), new Vector3(0.4f, 0.05f, 0.4f), btn, new Vector3(0f, 0f, 90f), b);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.28f, -0.02f), new Vector3(0.3f, 0.055f, 0.3f), PatchworkAssets.Plain(Color.Lerp(shell, Color.black, 0.15f), 0.75f), new Vector3(0f, 0f, 90f), b);
            for (int k = 0; k < 4; k++)
            {
                float a = k * Mathf.PI * 0.5f + 0.78f;
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.06f, 0.28f + Mathf.Sin(a) * 0.06f, -0.02f + Mathf.Cos(a) * 0.06f), new Vector3(0.04f, 0.02f, 0.04f), PatchworkAssets.Plain(new Color(0.15f, 0.12f, 0.15f), 0.5f), new Vector3(0f, 0f, 90f), b);
            }
            return c;
        }

        // A felt frog. Hops in big boings near water.
        public static ToyCritter Frog(LevelKit kit, Vector3 feet)
        {
            var c = Make(kit, Kind.Frog, feet, 5f, out Transform b);
            c.Speed = 2f; c.HopHeight = 0.6f; c.HopRate = 5f; c.Shy = 4f; c.Pitch = 1.6f;
            var green = PatchworkAssets.Felt(new Color(0.45f, 0.78f, 0.35f));
            var belly = PatchworkAssets.Felt(new Color(0.85f, 0.95f, 0.6f));
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.22f, 0f), new Vector3(0.5f, 0.32f, 0.5f), green, default(Vector3), b);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.18f, 0.1f), new Vector3(0.4f, 0.22f, 0.36f), belly, default(Vector3), b);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.13f, 0.42f, 0.14f), Vector3.one * 0.17f, green, default(Vector3), b);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.13f, 0.44f, 0.2f), Vector3.one * 0.12f, PatchworkAssets.Felt(Color.white), default(Vector3), b);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.13f, 0.45f, 0.25f), Vector3.one * 0.06f, PatchworkAssets.Plain(new Color(0.08f, 0.07f, 0.1f), 0.85f), default(Vector3), b);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.27f, 0.08f, -0.1f), new Vector3(0.22f, 0.1f, 0.34f), green, default(Vector3), b);
            }
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.24f, 0.25f), new Vector3(0.22f, 0.03f, 0.05f), PatchworkAssets.Felt(new Color(0.55f, 0.15f, 0.25f)), default(Vector3), b);
            return c;
        }

        // A felt fox with a bushy striped tail. Lives at the forest's edge.
        public static ToyCritter Fox(LevelKit kit, Vector3 feet)
        {
            var c = Make(kit, Kind.Fox, feet, 9f, out Transform b);
            c.Speed = 1.6f; c.Shy = 7f; c.Pitch = 0.95f; c.HopRate = 9f;
            var orange = PatchworkAssets.Felt(new Color(0.95f, 0.5f, 0.2f));
            var cream = PatchworkAssets.Felt(new Color(0.99f, 0.95f, 0.88f));
            var dark = PatchworkAssets.Felt(new Color(0.25f, 0.18f, 0.18f));
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.5f, 0f), new Vector3(0.38f, 0.42f, 0.38f), orange, new Vector3(90f, 0f, 0f), b);
            var legs = new Transform[4];
            int li = 0;
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    legs[li] = Pivot(b, new Vector3(sx * 0.12f, 0.36f, sz * 0.25f));
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.17f, 0f), new Vector3(0.09f, 0.18f, 0.09f), dark, default(Vector3), legs[li]);
                    li++;
                }
            c.Legs = legs;
            var head = Pivot(b, new Vector3(0f, 0.72f, 0.42f));
            c.Head = head;
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.36f, 0.32f, 0.34f), orange, default(Vector3), head);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.05f, 0.2f), new Vector3(0.16f, 0.14f, 0.24f), cream, default(Vector3), head);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.02f, 0.32f), Vector3.one * 0.06f, dark, default(Vector3), head);
            Eyes(kit, head, new Vector3(0f, 0.06f, 0.15f), 0.08f, 0.06f);
            var ears = new Transform[2];
            for (int s = 0; s < 2; s++)
            {
                ears[s] = Pivot(head, new Vector3((s == 0 ? -1f : 1f) * 0.1f, 0.16f, 0f));
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.06f, 0f), new Vector3(0.1f, 0.14f, 0.04f), orange, new Vector3(0f, 0f, 45f), ears[s]);
            }
            c.Ears = ears;
            for (int k = 0; k < 4; k++)
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.55f + k * 0.08f, -0.42f - k * 0.13f), Vector3.one * (0.24f - k * 0.02f), k == 3 ? cream : orange, default(Vector3), b);
            return c;
        }
    }
}

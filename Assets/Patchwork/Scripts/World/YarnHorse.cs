using UnityEngine;

namespace Patchwork
{
    // A wild yarn horse: a knitted pony with a looped yarn mane and a tassel tail, roaming the
    // open country in little herds. Walk up and press Ability to hop on: Patch rides on its
    // back, much faster, with clip-clopping hooves. Press Ability again (or jump into water,
    // or warp anywhere) to hop off; it stays where you left it and wanders off grazing.
    public class YarnHorse : MonoBehaviour
    {
        public static YarnHorse Ridden { get; private set; }
        public static void ResetStatics() { Ridden = null; }

        public const float BackHeight = 1.02f;
        const float RideBoost = 0.9f;   // +90% speed

        public Vector3 Home;
        public float Radius = 10f;
        public Transform Body, Head, Neck, Tail;
        public Transform[] Legs;
        public float Pitch = 1f;

        UseSpot spot;
        PatchworkPlayer rider;
        Vector3 target;
        float wait, phase, yaw, groundY, rideT, clop, idleT, nickerCool;
        int mask;
        static bool toldHowToHopOff;

        public void Setup(UseSpot useSpot)
        {
            spot = useSpot;
            spot.Label = "Ride";
            spot.Used = Mount;
        }

        void Start()
        {
            mask = ~(1 << 2);
            target = transform.position;
            groundY = transform.position.y;
            yaw = transform.eulerAngles.y;
            wait = Random.Range(1f, 4f);
            phase = Random.value * 10f;
            idleT = Random.value * 10f;
        }

        void Mount(PatchworkPlayer p)
        {
            if (rider != null || Ridden != null) return;
            rider = p;
            Ridden = this;
            rideT = 0f;
            spot.gameObject.SetActive(false);
            p.Visuals.RideHeight = BackHeight;
            PatchModel.Riding = true;
            p.Motor.Velocity = new Vector3(0f, 4f, 0f);
            Sfx.PlayPitched(SfxId.Neigh, 0.5f, Pitch);
            FxBurst.Spawn(FxBurst.Kind.Puff, transform.position + Vector3.up * 0.4f, Color.white);
            Haptics.Pulse(0.2f, 0.3f, 0.12f);
            var game = p.Game;
            if (game != null && !toldHowToHopOff)
            {
                toldHowToHopOff = true;
                game.ShowToast("Giddy-up! Press the same button again to hop off.");
            }
        }

        public void Dismount(bool hop)
        {
            if (rider == null) return;
            var p = rider;
            rider = null;
            if (Ridden == this) Ridden = null;
            PatchModel.Riding = false;
            p.Visuals.RideHeight = 0f;
            // Step aside so we're not standing in each other.
            Vector3 side = p.transform.right * 1.3f;
            Vector3 at = p.transform.position + side;
            if (Physics.Raycast(at + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 4f, mask, QueryTriggerInteraction.Ignore)) at.y = hit.point.y;
            else at = p.transform.position;
            transform.position = at;
            groundY = at.y;
            Home = at;
            target = at;
            wait = Random.Range(2f, 4f);
            spot.gameObject.SetActive(true);
            if (hop && !p.Motor.ExternalControl) p.Motor.Bounce(6f);
            Sfx.PlayPitched(SfxId.Neigh, 0.35f, Pitch * 1.08f);
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) return;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            if (rider != null) Ride(game, dt);
            else Roam(game, dt);
            if (spot != null)
            {
                spot.transform.position = transform.position + Vector3.up * 1.1f;
                spot.PromptPoint = transform.position + Vector3.up * 2.9f;
            }
        }

        void LateUpdate()
        {
            // The speed-up is a per-step modifier the motor clears after each step, so set it
            // again ready for the next one.
            if (rider != null && rider.Motor.Grounded) rider.Motor.SpeedBoost = Mathf.Max(rider.Motor.SpeedBoost, RideBoost);
        }

        void Ride(PatchworkGame game, float dt)
        {
            rideT += dt;
            var m = rider.Motor;
            // Things that knock you off.
            if (m.Swimming || m.ExternalControl || game.Mode == UIMode.Loading || !rider.gameObject.activeInHierarchy)
            {
                Dismount(false);
                return;
            }
            if (rideT > 0.4f && game.Mode == UIMode.Gameplay && game.Input.Ability.WasPressedThisFrame() && UseSpot.Active == null)
            {
                Dismount(true);
                return;
            }
            transform.position = rider.transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, rider.transform.rotation, 1f - Mathf.Exp(-14f * dt));
            yaw = transform.eulerAngles.y;
            Vector3 v = m.Velocity;
            float speed = new Vector3(v.x, 0f, v.z).magnitude;
            // Hoofbeats.
            if (m.Grounded && speed > 1f)
            {
                clop -= dt * Mathf.Lerp(2.5f, 7f, Mathf.Clamp01(speed / 12f));
                if (clop <= 0f)
                {
                    clop = 1f;
                    Sfx.PlayPitched(SfxId.Clop, 0.22f, Random.Range(0.92f, 1.08f));
                }
            }
            Animate(speed, m.Grounded, dt);
        }

        void Roam(PatchworkGame game, float dt)
        {
            Vector3 me = transform.position;
            if ((game.Player.Motor.Center - me).sqrMagnitude > 100f * 100f) return;
            Vector3 to = target - me;
            to.y = 0f;
            float speed = 0f;
            if (wait > 0f)
            {
                wait -= dt;
                if (wait <= 0f)
                {
                    Vector2 r = Random.insideUnitCircle * Radius;
                    target = new Vector3(Home.x + r.x, me.y, Home.z + r.y);
                }
            }
            else if (to.magnitude < 0.4f) wait = Random.Range(3f, 8f);
            else
            {
                Vector3 dir = to.normalized;
                if (Physics.SphereCast(me + Vector3.up * 0.7f, 0.45f, dir, out RaycastHit block, 1.4f, mask, QueryTriggerInteraction.Ignore))
                    wait = 0.5f;
                else
                {
                    speed = 1.4f;
                    me += dir * speed * dt;
                    yaw = Mathf.MoveTowardsAngle(yaw, Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg, 120f * dt);
                }
            }
            if (Physics.Raycast(me + Vector3.up * 1.5f, Vector3.down, out RaycastHit g, 4f, mask, QueryTriggerInteraction.Ignore))
            {
                if (g.point.y - groundY < 0.6f) groundY = Mathf.Lerp(groundY, g.point.y, 1f - Mathf.Exp(-10f * dt));
                else wait = 0.5f;
            }
            me.y = groundY;
            transform.position = me;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            // A friendly nicker when Patch comes close.
            nickerCool -= dt;
            float d = (game.Player.Motor.Center - me).sqrMagnitude;
            if (d < 9f && nickerCool <= 0f)
            {
                nickerCool = 12f;
                Sfx.PlayPitched(SfxId.Neigh, 0.25f, Pitch * 1.15f);
            }
            Animate(speed, true, dt);
        }

        void Animate(float speed, bool grounded, float dt)
        {
            idleT += dt;
            float gait = Mathf.Clamp01(speed / 10f);
            phase += dt * (speed > 0.2f ? Mathf.Lerp(6f, 14f, gait) : 0f);
            float swing = speed > 0.2f ? Mathf.Lerp(22f, 40f, gait) : 0f;
            if (Legs != null)
            {
                // Front-left with back-right, front-right with back-left (a trot / canter).
                float[] offs = { 0f, Mathf.PI, Mathf.PI, 0f };
                for (int i = 0; i < Legs.Length && i < 4; i++)
                    if (Legs[i] != null) Legs[i].localRotation = Quaternion.Euler(grounded ? Mathf.Sin(phase + offs[i]) * swing : (i < 2 ? -35f : 30f), 0f, 0f);
            }
            if (Body != null)
            {
                float bob = speed > 0.2f ? Mathf.Abs(Mathf.Sin(phase)) * Mathf.Lerp(0.05f, 0.14f, gait) : Mathf.Sin(idleT * 1.8f) * 0.012f;
                Body.localPosition = new Vector3(0f, bob, 0f);
                Body.localRotation = Quaternion.Euler(speed > 0.2f ? Mathf.Sin(phase * 2f) * 3f * gait : 0f, 0f, 0f);
            }
            if (Neck != null)
            {
                // Head nods with the stride; grazes now and then when standing.
                float graze = speed < 0.2f && rider == null ? Mathf.Clamp01(Mathf.Sin(idleT * 0.35f) * 2f - 0.6f) * 55f : 0f;
                float nod = speed > 0.2f ? Mathf.Sin(phase * 2f) * 6f : 0f;
                Neck.localRotation = Quaternion.Euler(graze + nod, 0f, 0f);
            }
            if (Tail != null) Tail.localRotation = Quaternion.Euler(-25f - gait * 30f, Mathf.Sin(idleT * 2.3f) * 18f, 0f);
        }

        void OnDisable()
        {
            if (rider != null) Dismount(false);
        }

        // ------------------------------------------------------------------ Builder

        public static YarnHorse Build(LevelKit kit, Vector3 feet, Color coat, Color mane, float yawDeg)
        {
            var g = kit.Group("YarnHorse", feet);
            g.rotation = Quaternion.Euler(0f, yawDeg, 0f);
            var horse = g.gameObject.AddComponent<YarnHorse>();
            horse.Home = feet;
            horse.Pitch = 0.9f + (coat.r + coat.g) * 0.1f;
            var body = new GameObject("Body").transform;
            body.SetParent(g, false);
            horse.Body = body;
            var knit = PatchworkAssets.Knit(coat);
            var dark = PatchworkAssets.Felt(Color.Lerp(coat, Color.black, 0.45f));
            var yarn = PatchworkAssets.Yarn(mane);
            var ink = PatchworkAssets.Plain(new Color(0.08f, 0.07f, 0.1f), 0.85f);

            // Barrel body with a darker knitted rib along the back.
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 1.1f, 0f), new Vector3(0.82f, 0.9f, 0.82f), knit, new Vector3(90f, 0f, 0f), body);
            for (int k = 0; k < 5; k++)
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.5f, -0.6f + k * 0.3f), new Vector3(0.08f, 0.04f, 0.18f), dark, default(Vector3), body);
            // A little patchwork blanket where Patch sits.
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.47f, -0.05f), new Vector3(0.86f, 0.06f, 0.75f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, new Color(0.99f, 0.96f, 0.9f), mane), new Vector3(0f, 0f, 0f), body);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.43f, 1.3f, -0.05f), new Vector3(0.04f, 0.4f, 0.75f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, new Color(0.99f, 0.96f, 0.9f), mane), new Vector3(0f, 0f, s * 8f), body);

            // Legs: a pivot at each hip so they can swing, a knitted leg and a felt hoof.
            var legs = new Transform[4];
            Vector3[] hips = { new Vector3(-0.24f, 0.95f, 0.55f), new Vector3(0.24f, 0.95f, 0.55f), new Vector3(-0.24f, 0.95f, -0.55f), new Vector3(0.24f, 0.95f, -0.55f) };
            for (int i = 0; i < 4; i++)
            {
                var hip = new GameObject("Leg").transform;
                hip.SetParent(body, false);
                hip.localPosition = hips[i];
                kit.Deco(PrimitiveType.Capsule, new Vector3(0f, -0.42f, 0f), new Vector3(0.22f, 0.42f, 0.22f), knit, default(Vector3), hip);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.86f, 0.02f), new Vector3(0.26f, 0.08f, 0.28f), dark, default(Vector3), hip);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.72f, 0f), new Vector3(0.25f, 0.03f, 0.25f), yarn, default(Vector3), hip);
                legs[i] = hip;
            }
            horse.Legs = legs;

            // Neck (pivots at the shoulders so it can graze), head, ears, eyes and nostrils.
            var neck = new GameObject("Neck").transform;
            neck.SetParent(body, false);
            neck.localPosition = new Vector3(0f, 1.3f, 0.65f);
            horse.Neck = neck;
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.32f, 0.16f), new Vector3(0.4f, 0.42f, 0.42f), knit, new Vector3(30f, 0f, 0f), neck);
            var head = new GameObject("Head").transform;
            head.SetParent(neck, false);
            head.localPosition = new Vector3(0f, 0.72f, 0.36f);
            horse.Head = head;
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, -0.05f, 0.2f), new Vector3(0.34f, 0.34f, 0.36f), knit, new Vector3(100f, 0f, 0f), head);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.14f, 0.5f), new Vector3(0.32f, 0.26f, 0.26f), PatchworkAssets.Felt(Color.Lerp(coat, Color.white, 0.35f)), default(Vector3), head);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.07f, -0.12f, 0.62f), Vector3.one * 0.05f, ink, default(Vector3), head);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.17f, 0.08f, 0.2f), new Vector3(0.11f, 0.02f, 0.11f), ink, new Vector3(0f, 0f, 90f + s * -10f), head);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.175f, 0.09f, 0.21f), Vector3.one * 0.035f, PatchworkAssets.Felt(Color.white), default(Vector3), head);
                kit.Deco(PrimitiveType.Capsule, new Vector3(s * 0.11f, 0.28f, 0.02f), new Vector3(0.08f, 0.12f, 0.05f), knit, new Vector3(-10f, 0f, s * -15f), head);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.15f, -0.12f, 0.32f), new Vector3(0.1f, 0.05f, 0.05f), PatchworkAssets.Felt(new Color(1f, 0.65f, 0.7f)), default(Vector3), head);
            }
            // The mane: a row of chunky yarn loops down the neck, and a forelock.
            for (int k = 0; k < 7; k++)
            {
                float u = k / 6f;
                Vector3 p = Vector3.Lerp(new Vector3(0f, 0.75f, 0.32f), new Vector3(0f, 0.12f, -0.08f), u);
                kit.Deco(PrimitiveType.Sphere, p, new Vector3(0.14f, 0.24f, 0.18f), yarn, new Vector3(20f + k * 9f, 0f, (k % 2 == 0 ? 12f : -12f)), neck);
            }
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.2f, 0.28f), new Vector3(0.16f, 0.14f, 0.2f), yarn, new Vector3(-20f, 0f, 0f), head);

            // A tassel tail.
            var tail = new GameObject("Tail").transform;
            tail.SetParent(body, false);
            tail.localPosition = new Vector3(0f, 1.3f, -0.85f);
            horse.Tail = tail;
            for (int k = 0; k < 5; k++)
            {
                Vector3 a = Vector3.zero, b = new Vector3((k - 2) * 0.05f, -0.75f, -0.15f - (k % 2) * 0.05f);
                var t = kit.Deco(PrimitiveType.Cylinder, (a + b) * 0.5f, new Vector3(0.06f, (b - a).magnitude * 0.5f, 0.06f), yarn, default(Vector3), tail);
                t.localRotation = Quaternion.FromToRotation(Vector3.up, (b - a).normalized);
            }
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.16f, yarn, default(Vector3), tail);

            var use = kit.Volume<UseSpot>(feet + Vector3.up * 1.1f, new Vector3(2.4f, 2.4f, 2.8f), "HorseRide");
            horse.Setup(use);
            return horse;
        }
    }
}

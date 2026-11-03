using UnityEngine;

namespace Patchwork
{
    // A big wooden lever. Stand next to it and press Grab to pull it. Each pull fires Pulled.
    public class Lever : PlayerVolume
    {
        public static Lever Active { get; private set; }
        public Transform Handle;
        public System.Action Pulled;
        public float Cooldown = 1.2f;
        public string Label = "Pull";
        float cool, swing;
        bool flipped;

        protected override void OnPlayerEnter(PatchworkPlayer player) { Active = this; }
        protected override void OnPlayerExit(PatchworkPlayer player) { if (Active == this) Active = null; }
        protected override void OnDisable()
        {
            base.OnDisable();
            if (Active == this) Active = null;
        }

        protected override void OnPlayerStay(PatchworkPlayer player, float dt)
        {
            Active = this;
            var game = player.Game;
            if (game == null || game.Mode != UIMode.Gameplay || cool > 0f) return;
            if (!game.Input.Grab.WasPressedThisFrame()) return;
            if (player.Forms.Current == FormType.Wind)
            {
                game.ShowToast("Wind can't pull levers. Try Yarn or Water!");
                return;
            }
            cool = Cooldown;
            flipped = !flipped;
            swing = 1f;
            Sfx.Play(SfxId.Lever, 0.55f);
            Haptics.Pulse(0.3f, 0.15f, 0.08f);
            player.Visuals.PunchScale(new Vector3(0.9f, 1.1f, 0.9f));
            if (Pulled != null) Pulled();
        }

        void Update()
        {
            cool -= Time.deltaTime;
            swing = Mathf.MoveTowards(swing, 0f, Time.deltaTime * 2.5f);
            if (Handle == null) return;
            float target = flipped ? 35f : -35f;
            float wobble = Mathf.Sin(swing * 18f) * swing * 8f;
            Handle.localRotation = Quaternion.Euler(target + wobble, 0f, 0f);
        }
    }

    // A round button in the floor. Pressed while something heavy enough sits on it: a crate, or
    // Patch himself if it's a light pad. Fires Changed when it goes down or comes back up.
    public class PressurePad : MonoBehaviour
    {
        public Transform Top;
        public Renderer Lamp;
        public float Radius = 1.1f;
        public float MinMass = 20f;     // crates weigh about 40; Patch counts as 10
        public bool Pressed { get; private set; }
        public System.Action<bool> Changed;
        float sink, toastCooldown;
        Material lampOn, lampOff;
        static readonly Collider[] hits = new Collider[16];

        void Start()
        {
            lampOn = PatchworkAssets.Glow(new Color(0.45f, 0.95f, 0.5f), 1.4f);
            lampOff = PatchworkAssets.Glow(new Color(0.95f, 0.4f, 0.35f), 0.8f);
            if (Lamp != null) Lamp.sharedMaterial = lampOff;
        }

        void Update()
        {
            float weight = 0f;
            Vector3 c = transform.position + Vector3.up * 0.5f;
            int n = Physics.OverlapBoxNonAlloc(c, new Vector3(Radius, 0.6f, Radius), hits, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                var rb = hits[i].attachedRigidbody;
                if (rb != null && !rb.isKinematic) weight += rb.mass;
            }
            bool patchOn = false;
            var game = PatchworkGame.Instance;
            if (game != null && game.Player != null && game.Player.Motor.Grounded)
            {
                Vector3 feet = game.Player.transform.position - transform.position;
                if (Mathf.Abs(feet.x) < Radius && Mathf.Abs(feet.z) < Radius && Mathf.Abs(feet.y) < 0.6f) { patchOn = true; weight += 10f; }
            }
            bool now = weight >= MinMass;
            toastCooldown -= Time.deltaTime;
            if (patchOn && !now && toastCooldown <= 0f && game != null)
            {
                game.ShowToast("Patch is too light to press this. Find something heavy!");
                toastCooldown = 6f;
            }
            if (now != Pressed)
            {
                Pressed = now;
                Sfx.Play(SfxId.PadClick, 0.5f);
                if (Lamp != null) Lamp.sharedMaterial = now ? lampOn : lampOff;
                if (Changed != null) Changed(now);
            }
            sink = Mathf.MoveTowards(sink, Pressed ? 1f : (weight > 0f ? 0.3f : 0f), Time.deltaTime * 6f);
            if (Top != null) Top.localPosition = new Vector3(0f, 0.16f - sink * 0.12f, 0f);
        }
    }

    // A door that slides down into the ground when opened.
    public class Gate : MonoBehaviour
    {
        public Transform Door;
        public float Drop = 3f;
        public bool Open { get; private set; }
        Vector3 closed;
        float k;

        void Start() { if (Door != null) closed = Door.localPosition; }

        public void SetOpen(bool open)
        {
            if (open == Open) return;
            Open = open;
            Sfx.Play(SfxId.GateRumble, 0.55f);
        }

        void Update()
        {
            if (Door == null) return;
            k = Mathf.MoveTowards(k, Open ? 1f : 0f, Time.deltaTime * 1.4f);
            float e = k * k * (3f - 2f * k);
            Door.localPosition = closed + Vector3.down * Drop * e + Vector3.right * Mathf.Sin(Time.time * 40f) * 0.02f * (k > 0f && k < 1f ? 1f : 0f);
        }
    }

    // A see-saw catapult. A weight hangs over the raised end; pull the lever and it drops, the
    // board flips and anything standing on the low end is flung up towards Target.
    public class Seesaw : MonoBehaviour
    {
        public Transform Board;        // pivot; tilts around its local X
        public Transform Weight;       // drops onto the raised end
        public Vector3 LowEnd;         // world centre of the end you stand on
        public Vector3 Target;         // where a launched Patch should land
        public float Tilt = 14f;
        public float LaunchSpeed = 20f;
        Vector3 weightHome, weightDown;
        enum State { Ready, Dropping, Flipped, Resetting }
        State state;
        float t;

        void Start()
        {
            if (Weight != null)
            {
                weightHome = Weight.position;
                weightDown = weightHome + Vector3.down * 3.2f;
            }
            SetTilt(1f);
        }

        void SetTilt(float k)
        {
            // k = 1: the low (launch) end is down. k = -1: flipped.
            if (Board != null) Board.localRotation = Quaternion.Euler(Tilt * k, 0f, 0f);
        }

        public void Drop()
        {
            if (state != State.Ready) return;
            state = State.Dropping;
            t = 0f;
            Sfx.Play(SfxId.Release, 0.5f);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            t += dt;
            switch (state)
            {
                case State.Dropping:
                {
                    float k = Mathf.Clamp01(t / 0.45f);
                    if (Weight != null) Weight.position = Vector3.Lerp(weightHome, weightDown, k * k);
                    if (k >= 1f) Flip();
                    break;
                }
                case State.Flipped:
                {
                    float k = Mathf.Clamp01(t / 0.12f);
                    SetTilt(Mathf.Lerp(1f, -1f, k));
                    if (t > 2.6f) { state = State.Resetting; t = 0f; }
                    break;
                }
                case State.Resetting:
                {
                    float k = Mathf.Clamp01(t / 1.6f);
                    float e = k * k * (3f - 2f * k);
                    if (Weight != null) Weight.position = Vector3.Lerp(weightDown, weightHome, e);
                    SetTilt(Mathf.Lerp(-1f, 1f, e));
                    if (k >= 1f) state = State.Ready;
                    break;
                }
            }
        }

        void Flip()
        {
            state = State.Flipped;
            t = 0f;
            Sfx.Play(SfxId.Drum, 0.8f);
            Sfx.Play(SfxId.Boing, 0.6f);
            Haptics.Pulse(0.6f, 0.3f, 0.15f);
            FxBurst.Spawn(FxBurst.Kind.Puff, LowEnd + Vector3.up * 0.3f, Color.white, Vector3.up);
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) return;
            var player = game.Player;
            Vector3 feet = player.transform.position;
            Vector3 d = feet - LowEnd;
            if (Mathf.Abs(d.x) > 1.6f || Mathf.Abs(d.z) > 1.6f || Mathf.Abs(d.y) > 1.2f) return;
            // Work out the sideways speed that lands him on Target.
            var motor = player.Motor;
            float g = PlayerMotor.BaseGravity * (motor.Params != null ? motor.Params.GravityScale : 1f);
            float h = Target.y - feet.y;
            float vy = LaunchSpeed;
            float disc = vy * vy - 2f * g * h;
            float time = disc > 0f ? (vy + Mathf.Sqrt(disc)) / g : vy / g;
            Vector3 flat = Target - feet;
            flat.y = 0f;
            Vector3 carry = flat / Mathf.Max(0.3f, time);
            player.Hand.Release();
            motor.Velocity = new Vector3(carry.x, 0f, carry.z);
            motor.Bounce(vy);
            motor.AirCarry = carry;
            player.Visuals.PunchScale(new Vector3(0.6f, 1.5f, 0.6f));
            Sfx.Voice(SfxId.VoiceYay, player.Forms.Current, 0.5f);
            Sfx.Play(SfxId.Whoosh, 0.6f);
        }
    }

    // Props that squish and wobble when Patch bumps into them (trees, flower pots, lamps).
    public class Jiggle : PlayerVolume
    {
        public Transform Body;
        public SfxId Sound = SfxId.Rustle;
        public float Volume = 0.3f;
        Vector3 baseScale;
        Quaternion baseRot;
        float amount, cooldown;
        Vector3 away;

        void Start()
        {
            if (Body == null) return;
            baseScale = Body.localScale;
            baseRot = Body.localRotation;
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (Body == null || cooldown > 0f) return;
            amount = 1f;
            cooldown = 0.35f;
            away = Body.position - player.transform.position;
            away.y = 0f;
            away = away.sqrMagnitude > 0.001f ? away.normalized : Vector3.forward;
            Sfx.PlayPitched(Sound, Volume, Random.Range(0.9f, 1.15f));
        }

        void Update()
        {
            cooldown -= Time.deltaTime;
            if (Body == null || amount <= 0f) return;
            amount = Mathf.MoveTowards(amount, 0f, Time.deltaTime * 1.4f);
            float w = Mathf.Sin(amount * 22f) * amount;
            Body.localScale = new Vector3(baseScale.x * (1f + w * 0.08f), baseScale.y * (1f - w * 0.08f), baseScale.z * (1f + w * 0.08f));
            Vector3 axis = Vector3.Cross(Vector3.up, away);
            Body.localRotation = Quaternion.AngleAxis(w * 7f, Body.parent != null ? Body.parent.InverseTransformDirection(axis) : axis) * baseRot;
        }
    }
}

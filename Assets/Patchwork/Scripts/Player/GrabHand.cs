using UnityEngine;

namespace Patchwork
{
    // Grab: hold the Grab button next to a crate, block or ball to take hold of it, then walk to
    // push it or back away to pull it. Let go, jump or change to Wind to drop it. Yarn and Water
    // can grab; Wind is a cloud with no hands, so it blows things with its gust instead.
    public class GrabHand : MonoBehaviour
    {
        PatchworkPlayer player;
        public Grabbable Held { get; private set; }
        public Grabbable Target { get; private set; }
        Vector3 offset;
        Collider[] heldColliders;
        float toastCooldown, scuff;
        static readonly Collider[] hits = new Collider[24];

        public void Init(PatchworkPlayer p)
        {
            player = p;
        }

        bool CanGrab
        {
            get
            {
                if (player.Forms.Current == FormType.Wind) return false;
                if (!player.Motor.Grounded || player.Motor.ExternalControl) return false;
                var yarn = player.Forms.Ability as YarnAbility;
                return yarn == null || !yarn.Attached;
            }
        }

        public void Tick(PlayerInputFrame input, float dt)
        {
            toastCooldown -= dt;
            if (Held != null)
            {
                bool keep = input.GrabHeld && CanGrab && !input.JumpPressed && Held.Body != null && Held.isActiveAndEnabled;
                if (!keep) { Release(); return; }
                var motor = player.Motor;
                Vector3 to = Held.Body.worldCenterOfMass - player.transform.position;
                to.y = 0f;
                if (to.sqrMagnitude > 0.01f) motor.FacingOverride = to.normalized;
                motor.SpeedBoost = Mathf.Min(motor.SpeedBoost, Held.SpeedFactor - 1f);
                player.Visuals.SetGrip(true, Held.Body.worldCenterOfMass);
                // Dragged into a wall, or the toy's caught on something: let go.
                Vector3 err = player.transform.position + offset - Held.Body.position;
                err.y = 0f;
                if (err.magnitude > 1.5f)
                {
                    Release();
                    if (toastCooldown <= 0f) { player.Game.ShowToast("It's stuck!"); toastCooldown = 3f; }
                    return;
                }
                // A soft scuff now and then while it's sliding.
                float speed = new Vector2(motor.Velocity.x, motor.Velocity.z).magnitude;
                scuff += speed * dt;
                if (scuff > 1.1f) { scuff = 0f; Sfx.Step(SfxId.StepCardboard, 0.18f); }
                Target = null;
                return;
            }

            Target = CanGrab && Lever.Active == null ? FindTarget() : null;
            if (!input.GrabPressed) return;
            if (player.Forms.Current == FormType.Wind)
            {
                if (toastCooldown <= 0f) { player.Game.ShowToast("Wind can't hold things. Blow them along with a gust!"); toastCooldown = 3f; }
                return;
            }
            if (Target != null) Grab(Target);
        }

        Grabbable FindTarget()
        {
            Vector3 c = player.Motor.Center;
            Vector3 probe = c + player.transform.forward * 0.6f;
            int n = Physics.OverlapSphereNonAlloc(probe, 1.0f, hits, ~0, QueryTriggerInteraction.Ignore);
            Grabbable best = null;
            float bestD = 99f;
            for (int i = 0; i < n; i++)
            {
                var g = hits[i].GetComponentInParent<Grabbable>();
                if (g == null || g.Held || g.NoGrab) continue;
                float d = Vector3.Distance(hits[i].ClosestPoint(c), c);
                if (d < bestD && d < player.Motor.Controller.radius + 0.75f) { best = g; bestD = d; }
            }
            return best;
        }

        void Grab(Grabbable g)
        {
            Held = g;
            g.Held = true;
            offset = g.Body.position - player.transform.position;
            offset.y = 0f;
            heldColliders = g.GetComponentsInChildren<Collider>();
            foreach (var col in heldColliders) Physics.IgnoreCollision(player.Motor.Controller, col, true);
            g.Body.WakeUp();
            Sfx.Play(SfxId.Grab, 0.45f);
            Haptics.Pulse(0.2f, 0.1f, 0.06f);
            player.Visuals.PunchScale(new Vector3(1.1f, 0.92f, 1.1f));
        }

        public void Release()
        {
            if (Held == null) return;
            if (heldColliders != null)
                foreach (var col in heldColliders)
                    if (col != null) Physics.IgnoreCollision(player.Motor.Controller, col, false);
            Held.Held = false;
            Held = null;
            heldColliders = null;
            player.Visuals.SetGrip(false, Vector3.zero);
            Sfx.Play(SfxId.Release, 0.3f);
        }

        void FixedUpdate()
        {
            if (Held == null || Held.Body == null) return;
            var rb = Held.Body;
            Vector3 want = transform.position + offset;
            Vector3 d = want - rb.position;
            d.y = 0f;
            Vector3 v = d / Time.fixedDeltaTime;
            if (v.magnitude > 9f) v = v.normalized * 9f;
            rb.linearVelocity = new Vector3(v.x, rb.linearVelocity.y, v.z);
            rb.angularVelocity = new Vector3(0f, rb.angularVelocity.y * 0.5f, 0f);
        }
    }
}

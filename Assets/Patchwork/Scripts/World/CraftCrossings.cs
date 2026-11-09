using UnityEngine;

namespace Patchwork
{
    // A giant sewing button hanging on a thread, to hop across. It bobs gently on its own,
    // dips and tips under Patch's weight when he lands on it, and springs back up when he
    // leaves. (It has a Carrier on it too, so he rides the dip instead of floating above it.)
    public class WobbleStone : MonoBehaviour
    {
        public Collider Top;
        public float Phase;
        public float Radius = 1.1f;

        Vector3 rest;
        Quaternion restRot;
        float y, vy, t;
        Vector2 tilt, tiltVel;
        bool wasOn;

        void Start()
        {
            rest = transform.localPosition;
            restRot = transform.localRotation;
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            float dt = Mathf.Min(Time.deltaTime, 0.033f);
            if (game == null || game.Player == null || dt <= 0f) return;
            var motor = game.Player.Motor;
            bool on = motor.Grounded && motor.GroundCollider == Top;
            t += dt;

            float targetY = on ? -0.3f : 0f;
            Vector2 targetTilt = Vector2.zero;
            if (on)
            {
                Vector3 d = game.Player.transform.position - transform.position;
                targetTilt = Vector2.ClampMagnitude(new Vector2(d.x, d.z) / Radius, 1f) * 7f;
                if (!wasOn)
                {
                    vy -= 1.6f;   // the thump of landing
                    Sfx.Play(SfxId.PadClick, 0.22f);
                }
            }
            wasOn = on;

            vy += ((targetY - y) * 60f - vy * 8f) * dt;
            y += vy * dt;
            tiltVel += ((targetTilt - tilt) * 50f - tiltVel * 7f) * dt;
            tilt += tiltVel * dt;

            float bob = Mathf.Sin(t * 1.3f + Phase) * 0.05f;
            transform.localPosition = rest + Vector3.up * (y + bob);
            transform.localRotation = Quaternion.Euler(tilt.y, 0f, -tilt.x) * restRot;
        }
    }

    // A tape measure that works as a bridge on a timer. Stomp the red button at either end and
    // the tape shoots out across the gap; a few seconds later it starts to shiver, then snaps
    // back into its case, whether or not Patch is still on it.
    public class TapeBridge : MonoBehaviour
    {
        public Transform Tape, Hook;
        public Renderer[] Ticks;
        public float[] TickAt;
        public Vector3 Anchor;          // where the tape comes out (it runs towards -x from here)
        public float MaxLength = 14.8f, Width = 2.2f, HoldTime = 4.5f;
        public Vector3[] Plates;
        public Transform[] PlateTops;

        const float Thick = 0.16f, OutSpeed = 11f, BackSpeed = 24f, Warn = 1.5f;
        float length, timer, tick;
        bool wasPressed, wasOut;
        float[] plateY;

        void Start()
        {
            plateY = new float[PlateTops.Length];
            for (int i = 0; i < PlateTops.Length; i++) plateY[i] = PlateTops[i].localPosition.y;
            Apply(0f);
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            float dt = Mathf.Min(Time.deltaTime, 0.033f);
            if (game == null || game.Player == null || dt <= 0f) return;
            var motor = game.Player.Motor;
            Vector3 p = game.Player.transform.position;

            bool pressed = false;
            for (int i = 0; i < Plates.Length; i++)
            {
                Vector3 d = p - Plates[i];
                bool down = motor.Grounded && d.x * d.x + d.z * d.z < 0.95f * 0.95f && Mathf.Abs(d.y) < 0.6f;
                pressed |= down;
                var lp = PlateTops[i].localPosition;
                lp.y = Mathf.MoveTowards(lp.y, plateY[i] - (down ? 0.07f : 0f), dt * 0.8f);
                PlateTops[i].localPosition = lp;
            }
            if (pressed)
            {
                if (!wasPressed) Sfx.Play(SfxId.PadClick, 0.5f);
                if (timer <= 0f) Sfx.Play(SfxId.Whoosh, 0.3f);
                timer = HoldTime;
            }
            wasPressed = pressed;

            Vector3 mid = Anchor + Vector3.left * MaxLength * 0.5f;
            if (timer > 0f)
            {
                length = Mathf.MoveTowards(length, MaxLength, OutSpeed * dt);
                if (length >= MaxLength - 0.01f && !pressed)
                {
                    timer -= dt;
                    // Ticking, faster as time runs out.
                    tick -= dt;
                    if (tick <= 0f)
                    {
                        tick = timer < Warn ? 0.16f : 0.5f;
                        Sfx.PlayNear(SfxId.Clack, mid, timer < Warn ? 0.3f : 0.16f, 26f);
                    }
                    if (timer <= 0f) Sfx.PlayNear(SfxId.Whoosh, mid, 0.5f, 30f);
                }
            }
            else length = Mathf.MoveTowards(length, 0f, BackSpeed * dt);

            bool isOut = length > 0.05f;
            if (wasOut && !isOut) Sfx.PlayNear(SfxId.PlatformThunk, Anchor, 0.5f, 30f);
            wasOut = isOut;

            float shiver = timer > 0f && timer < Warn && length >= MaxLength - 0.01f ? Mathf.Sin(Time.time * 38f) * 2.4f * (1f - timer / Warn) : 0f;
            Apply(shiver);
        }

        void Apply(float roll)
        {
            bool isOut = length > 0.05f;
            if (Tape.gameObject.activeSelf != isOut) Tape.gameObject.SetActive(isOut);
            if (isOut)
            {
                Tape.position = Anchor + Vector3.left * (length * 0.5f) + Vector3.down * (Thick * 0.5f - 0.05f);
                Tape.rotation = Quaternion.Euler(roll, 0f, 0f);
                Tape.localScale = new Vector3(length, Thick, Width);
            }
            Hook.position = Anchor + Vector3.left * length + Vector3.up * 0.05f;
            for (int i = 0; i < Ticks.Length; i++)
            {
                bool show = TickAt[i] < length - 0.08f;
                if (Ticks[i].enabled != show) Ticks[i].enabled = show;
            }
        }
    }
}

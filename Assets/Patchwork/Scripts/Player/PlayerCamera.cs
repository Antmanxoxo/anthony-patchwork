using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Third-person orbit camera. Right stick / mouse orbits; it gently follows the player and
    // pulls in when something is between it and the player.
    // Platformer touches:
    // - holds its height while you jump, so normal jumps don't bob the view (it still follows if
    //   you go well above or below where you took off)
    // - looks a little ahead of where you're running
    // - backs out while swinging (and leans toward the anchor) or gliding, so you can see more
    // - after a few seconds without touching the camera, slowly swings round behind you
    // - camera zones in the level can add distance or tilt for a section
    // - it stays out of round things it can't see through (the landed Pod's egg): see AddBlocker
    // - when a level starts, it rises until it has a clear view of Patch over anything behind
    public class PlayerCamera : MonoBehaviour
    {
        public Transform Target;
        public float Distance = 7.5f;
        public float TargetHeight = 1.3f;
        public float StickSpeed = 170f;   // degrees per second at full tilt
        public float MouseSpeed = 0.12f;  // degrees per pixel
        public float MinPitch = -25f;
        public float MaxPitch = 65f;

        // Height band (around where you left the ground) the camera ignores while you're in the air.
        const float HoldAbove = 2.5f;
        const float HoldBelow = 1f;
        const float LookAheadTime = 0.35f;
        const float LookAheadMax = 2f;
        const float RecenterDelay = 2.5f;
        const float RecenterSpeed = 45f;   // degrees per second at full run

        public Camera Cam { get; private set; }

        // Set by the game every frame before Tick.
        [System.NonSerialized] public bool TargetGrounded = true;
        // The camera never dips below this height (used to keep it above water while swimming).
        [System.NonSerialized] public float MinY = float.NegativeInfinity;
        [System.NonSerialized] public Vector3 TargetVelocity;
        [System.NonSerialized] public bool Swinging;
        [System.NonSerialized] public Vector3 SwingAnchor;
        [System.NonSerialized] public bool Gliding;
        // Dance easter egg: a slow, low orbit around Patch like a little cutscene.
        [System.NonSerialized] public bool Cutscene;
        public float CutsceneBlend { get; private set; }
        const float CutsceneDistance = 4.2f;
        const float CutscenePitch = 6f;
        const float CutsceneOrbitSpeed = 22f;   // degrees per second

        // Showcase shots (title screen, level select): the game places the camera directly and it
        // glides between that shot and the normal follow camera.
        [System.NonSerialized] public bool Showcase;
        [System.NonSerialized] public Vector3 ShowcasePosition;
        [System.NonSerialized] public Vector3 ShowcaseLookAt;
        [System.NonSerialized] public float ShowcaseBlendSpeed = 0.8f;
        public float ShowcaseBlend { get; private set; }

        float yaw;
        float pitch = 18f;
        Vector3 focus;
        float currentDistance;
        int obstacleMask;

        float heldGroundY;
        Vector3 lookAhead;
        float idleLookTime;
        float extraDistance, extraPitch;
        CameraZone zone;

        public float Yaw { get { return yaw; } }

        void Awake()
        {
            Cam = GetComponent<Camera>();
            currentDistance = Distance;
            // Ignore the player (Ignore Raycast layer) and UI.
            obstacleMask = ~((1 << 2) | (1 << 5));
        }

        // Round shapes the camera must stay outside of, as egg shapes (ellipsoids) on a transform.
        struct Blocker { public Transform T; public Vector3 Center, Radii; }
        static readonly List<Blocker> blockers = new List<Blocker>();

        // Keeps the camera out of an egg shape: `center` and `radii` in `t`'s own frame. (For
        // things whose solid box sits inside them, so the box alone would let the camera in.)
        public static void AddBlocker(Transform t, Vector3 center, Vector3 radii)
        {
            blockers.RemoveAll(b => b.T == null);
            blockers.Add(new Blocker { T = t, Center = center, Radii = radii });
        }

        // How far along a ray from `from` the first blocker is (or `max` if none is closer).
        // A blocker the ray starts inside doesn't count (Patch is in the Pod).
        static float BlockerDistance(Vector3 from, Vector3 dir, float max)
        {
            float best = max;
            for (int i = 0; i < blockers.Count; i++)
            {
                var bl = blockers[i];
                if (bl.T == null || !bl.T.gameObject.activeInHierarchy) continue;
                Vector3 o = bl.T.InverseTransformPoint(from) - bl.Center;
                Vector3 d = bl.T.InverseTransformDirection(dir);
                o = new Vector3(o.x / bl.Radii.x, o.y / bl.Radii.y, o.z / bl.Radii.z);
                d = new Vector3(d.x / bl.Radii.x, d.y / bl.Radii.y, d.z / bl.Radii.z);
                float a = Vector3.Dot(d, d), b = 2f * Vector3.Dot(o, d), c = Vector3.Dot(o, o) - 1f;
                if (c <= 0f || a < 1e-8f) continue;
                float disc = b * b - 4f * a * c;
                if (disc < 0f) continue;
                float s = (-b - Mathf.Sqrt(disc)) / (2f * a);
                if (s > 0f && s < best) best = s;
            }
            return best;
        }

        public void SnapBehind(float targetYaw)
        {
            yaw = targetYaw;
            pitch = 18f;
            if (Target != null)
            {
                heldGroundY = Target.position.y;
                focus = Target.position + Vector3.up * TargetHeight;
                // Rise until nothing big is between the camera and Patch (the Pod Patch has just
                // climbed out of, a wall behind a checkpoint), so the first view is a clear one.
                for (float p = 18f; p <= 56f; p += 4f)
                {
                    Vector3 back = Quaternion.Euler(p, yaw, 0f) * Vector3.back;
                    RaycastHit h;
                    bool blocked = BlockerDistance(focus, back, Distance) < Distance * 0.85f
                        || Physics.SphereCast(focus, 0.3f, back, out h, Distance * 0.85f, obstacleMask, QueryTriggerInteraction.Ignore);
                    if (!blocked) { pitch = p; break; }
                }
            }
            lookAhead = Vector3.zero;
            idleLookTime = 0f;
            currentDistance = Distance;
            Apply(1f);
        }

        public void EnterZone(CameraZone z) { zone = z; }
        public void ExitZone(CameraZone z) { if (zone == z) zone = null; }

        public void Tick(Vector2 stick, Vector2 mouse, float dt)
        {
            if (Target == null) return;

            // Player settings: speed and inverting.
            float speedMul = GameSettings.CameraSpeedMultiplier;
            stick *= speedMul;
            mouse *= speedMul;
            if (GameSettings.InvertX) { stick.x = -stick.x; mouse.x = -mouse.x; }
            if (GameSettings.InvertY) { stick.y = -stick.y; mouse.y = -mouse.y; }

            yaw += stick.x * StickSpeed * dt + mouse.x * MouseSpeed;
            pitch -= stick.y * StickSpeed * 0.7f * dt + mouse.y * MouseSpeed;
            pitch = Mathf.Clamp(pitch, MinPitch, MaxPitch);

            CutsceneBlend = Mathf.MoveTowards(CutsceneBlend, Cutscene ? 1f : 0f, dt * 1.6f);
            if (Cutscene) yaw += CutsceneOrbitSpeed * CutsceneBlend * dt;

            bool touched = stick.sqrMagnitude > 0.01f || mouse.sqrMagnitude > 0.01f;
            idleLookTime = touched ? 0f : idleLookTime + dt;

            Vector3 flatVel = new Vector3(TargetVelocity.x, 0f, TargetVelocity.z);
            float speed = flatVel.magnitude;

            // Slowly come round behind the player after a while without touching the camera.
            // Not while running toward the camera (that would spin it all the way round).
            if (idleLookTime > RecenterDelay && speed > 2f && TargetGrounded && !Swinging && CutsceneBlend <= 0f)
            {
                float moveYaw = Mathf.Atan2(flatVel.x, flatVel.z) * Mathf.Rad2Deg;
                float diff = Mathf.DeltaAngle(yaw, moveYaw);
                if (Mathf.Abs(diff) < 120f)
                {
                    float rate = RecenterSpeed * Mathf.Clamp01(speed / 6.5f);
                    yaw = Mathf.MoveTowardsAngle(yaw, moveYaw, rate * dt);
                }
            }

            // Hold the height you jumped from unless you go well above or below it.
            float feetY = Target.position.y;
            if (TargetGrounded || Swinging) heldGroundY = feetY;
            else heldGroundY = Mathf.Clamp(heldGroundY, feetY - HoldAbove, feetY + HoldBelow);

            // Look a little ahead of where you're running (never up or down).
            Vector3 wantAhead = Vector3.ClampMagnitude(flatVel * LookAheadTime, LookAheadMax);
            lookAhead = Vector3.Lerp(lookAhead, wantAhead, 1f - Mathf.Exp(-3f * dt));

            Vector3 desiredFocus = new Vector3(Target.position.x, heldGroundY, Target.position.z) + Vector3.up * TargetHeight + lookAhead;
            if (Swinging) desiredFocus = Vector3.Lerp(Target.position + Vector3.up * TargetHeight, SwingAnchor, 0.3f);
            desiredFocus.y -= TargetHeight * 0.25f * CutsceneBlend; // frame the whole body while dancing

            // Follow horizontally tightly, vertically a little softer.
            float fh = 1f - Mathf.Exp(-12f * dt);
            float fv = 1f - Mathf.Exp(-5f * dt);
            focus = new Vector3(Mathf.Lerp(focus.x, desiredFocus.x, fh),
                                Mathf.Lerp(focus.y, desiredFocus.y, fv),
                                Mathf.Lerp(focus.z, desiredFocus.z, fh));

            // Back out a bit for swings, glides and camera zones.
            float wantExtraDist = (Swinging ? 1.5f : 0f) + (Gliding ? 1f : 0f) + (zone != null ? zone.ExtraDistance : 0f);
            float wantExtraPitch = (Gliding ? 5f : 0f) + (zone != null ? zone.ExtraPitch : 0f);
            float fz = 1f - Mathf.Exp(-2.5f * dt);
            extraDistance = Mathf.Lerp(extraDistance, wantExtraDist, fz);
            extraPitch = Mathf.Lerp(extraPitch, wantExtraPitch, fz);
            Apply(dt);
        }

        void Apply(float dt)
        {
            float cut = Mathf.SmoothStep(0f, 1f, CutsceneBlend);
            float p = Mathf.Lerp(Mathf.Clamp(pitch + extraPitch, MinPitch, MaxPitch), CutscenePitch, cut);
            Quaternion rot = Quaternion.Euler(p, yaw, 0f);
            Vector3 back = rot * Vector3.back;

            float dist = Mathf.Lerp(Distance + extraDistance, CutsceneDistance, cut);
            float wanted = dist;
            RaycastHit hit;
            if (Physics.SphereCast(focus, 0.3f, back, out hit, dist, obstacleMask, QueryTriggerInteraction.Ignore))
                wanted = Mathf.Max(1.2f, hit.distance - 0.1f);
            float egg = BlockerDistance(focus, back, dist);
            if (egg < dist) wanted = Mathf.Min(wanted, Mathf.Max(1.2f, egg - 0.4f));

            // Pull in instantly, ease back out.
            if (wanted < currentDistance) currentDistance = wanted;
            else currentDistance = Mathf.Lerp(currentDistance, wanted, 1f - Mathf.Exp(-4f * dt));

            Vector3 pos = focus + back * currentDistance;
            if (pos.y < MinY)
            {
                pos.y = MinY;
                Vector3 look = focus - pos;
                if (look.sqrMagnitude > 0.01f) rot = Quaternion.LookRotation(look.normalized, Vector3.up);
            }
            ShowcaseBlend = Mathf.MoveTowards(ShowcaseBlend, Showcase ? 1f : 0f, dt * ShowcaseBlendSpeed);
            if (ShowcaseBlend > 0f)
            {
                float s = Mathf.SmoothStep(0f, 1f, ShowcaseBlend);
                Quaternion showRot = Quaternion.LookRotation((ShowcaseLookAt - ShowcasePosition).normalized, Vector3.up);
                pos = Vector3.Lerp(pos, ShowcasePosition, s);
                rot = Quaternion.Slerp(rot, showRot, s);
            }
            // A gentle hum and wobble, like a little craft flying (the Pod).
            if (Rumble > 0f)
            {
                float tt = Time.unscaledTime;
                pos += new Vector3(Mathf.PerlinNoise(tt * 1.7f, 0.3f) - 0.5f, Mathf.PerlinNoise(0.7f, tt * 1.9f) - 0.5f, 0f) * Rumble;
                rot *= Quaternion.Euler((Mathf.PerlinNoise(tt * 0.9f, 5.1f) - 0.5f) * Rumble * 12f, 0f, (Mathf.PerlinNoise(9.3f, tt * 0.8f) - 0.5f) * Rumble * 20f);
            }
            // A quick shake from a big hit (stomps, crashes, getting nipped).
            kick = Mathf.MoveTowards(kick, 0f, Time.unscaledDeltaTime * 1.6f);
            if (kick > 0f)
            {
                float tt = Time.unscaledTime * 30f;
                pos += new Vector3(Mathf.PerlinNoise(tt, 2.2f) - 0.5f, Mathf.PerlinNoise(6.6f, tt) - 0.5f, 0f) * kick * 0.9f;
            }
            transform.position = pos;
            transform.rotation = rot;
        }

        [System.NonSerialized] public float Rumble;
        float kick;
        public void Kick(float amount) { kick = Mathf.Max(kick, amount); }

        // Jump straight to the showcase shot (no glide), e.g. when the game starts on the title screen.
        public void SnapShowcase()
        {
            ShowcaseBlend = Showcase ? 1f : 0f;
        }

        // Converts stick input into a world direction relative to the camera.
        public Vector3 ToWorld(Vector2 input)
        {
            Quaternion flat = Quaternion.Euler(0f, yaw, 0f);
            Vector3 v = flat * new Vector3(input.x, 0f, input.y);
            return Vector3.ClampMagnitude(v, 1f);
        }
    }
}

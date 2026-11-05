using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Something small lying about Toybox Town that a Tape Ball can pick up.
    public class Clumpable : MonoBehaviour
    {
        public static readonly List<Clumpable> All = new List<Clumpable>();
        public float Size = 0.5f;     // roughly how big it is across (metres)
        public string Label = "thing";

        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }
    }

    // A ball of sticky tape. Roll it (run into it) over small things and they stick to it, and it
    // grows; the bigger it gets, the bigger the things it can pick up. Grow it big enough and it
    // floats up into the sky as a new star, and a fresh little ball appears.
    public class TapeBall : MonoBehaviour
    {
        public static TapeBall Active { get; private set; }
        public const float StartRadius = 0.45f, StarRadius = 1.8f;
        static readonly float[] Milestones = { 0.7f, 1.0f, 1.35f };
        static readonly string[] MilestoneNames = { "a teacup", "a teddy bear", "an armchair" };

        public float Radius { get; private set; }
        public Vector3 Home;
        public static int StarsMade;

        Rigidbody body;
        SphereCollider col;
        Transform core;
        int nextMilestone;
        float bumpCooldown, starTime = -1f;
        LevelKit kit;
        Vector3 starStart;

        public static void ResetStatics() { Active = null; StarsMade = 0; }

        public static TapeBall Create(LevelKit kit, Vector3 feet)
        {
            var go = new GameObject("TapeBall");
            go.transform.SetParent(kit.Root, false);
            go.transform.position = feet + Vector3.up * StartRadius;
            var b = go.AddComponent<TapeBall>();
            b.kit = kit;
            b.Home = feet;
            b.Radius = StartRadius;

            b.core = kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * StartRadius * 2f,
                PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.85f, 0.9f, 0.95f), new Color(0.98f, 0.98f, 1f)), default(Vector3), go.transform);
            // Little coloured knobs all over it, so it reads as a ball you roll.
            Color[] knobs = { new Color(1f, 0.4f, 0.6f), new Color(1f, 0.85f, 0.3f), new Color(0.4f, 0.8f, 1f), new Color(0.55f, 0.9f, 0.45f), new Color(0.75f, 0.5f, 1f), new Color(1f, 0.6f, 0.3f) };
            for (int i = 0; i < 14; i++)
            {
                float y = 1f - (i + 0.5f) / 14f * 2f;
                float r = Mathf.Sqrt(1f - y * y);
                float a = i * 2.39996f;
                Vector3 n = new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r);
                kit.Deco(PrimitiveType.Sphere, n * 0.5f, Vector3.one * 0.26f, PatchworkAssets.Plain(knobs[i % knobs.Length], 0.6f), default(Vector3), b.core);
            }

            b.col = go.AddComponent<SphereCollider>();
            b.col.radius = StartRadius;
            b.col.material = new PhysicsMaterial("Tape") { bounciness = 0.1f, dynamicFriction = 0.9f, staticFriction = 0.9f, frictionCombine = PhysicsMaterialCombine.Maximum };
            b.body = go.AddComponent<Rigidbody>();
            b.body.mass = 1.5f;
            b.body.linearDamping = 0.4f;
            b.body.angularDamping = 0.6f;
            b.body.interpolation = RigidbodyInterpolation.Interpolate;
            var g = go.AddComponent<Grabbable>();
            g.KickOnWalk = true;
            g.Rolls = true;
            g.SpeedFactor = 0.85f;
            return b;
        }

        void FixedUpdate()
        {
            if (starTime >= 0f || body == null) return;
            Vector3 c = transform.position;
            for (int i = Clumpable.All.Count - 1; i >= 0; i--)
            {
                var item = Clumpable.All[i];
                if (item == null) continue;
                float reach = Radius + item.Size * 0.45f;
                Vector3 ip = item.transform.position + Vector3.up * item.Size * 0.3f;
                if ((ip - c).sqrMagnitude > reach * reach) continue;
                if (item.Size > Radius * 1.15f)
                {
                    if (bumpCooldown <= 0f && body.linearVelocity.magnitude > 1f)
                    {
                        bumpCooldown = 2.5f;
                        Sfx.Play(SfxId.PlatformThunk, 0.4f);
                        var game = PatchworkGame.Instance;
                        if (game != null) game.ShowToast("Too big to pick up! Grow the Tape Ball first.");
                    }
                    continue;
                }
                Stick(item, c);
            }
        }

        void Stick(Clumpable item, Vector3 c)
        {
            item.enabled = false;   // leaves the list
            foreach (var cl in item.GetComponentsInChildren<Collider>()) Destroy(cl);
            Transform t = item.transform;
            Vector3 dir = (t.position + Vector3.up * item.Size * 0.3f - c);
            if (dir.sqrMagnitude < 0.0001f) dir = Vector3.up;
            dir.Normalize();
            t.SetParent(transform, true);
            t.position = c + dir * Radius * 0.92f;
            float s = item.Size;
            Radius = Mathf.Pow(Radius * Radius * Radius + 0.5f * s * s * s, 1f / 3f);
            col.radius = Radius;
            core.localScale = Vector3.one * Radius * 2f;
            body.mass = 1.5f + Radius * Radius * Radius * 2f;
            // Lift a little so the bigger ball doesn't sink into the ground.
            body.position += Vector3.up * 0.02f;
            float pitch = Mathf.Lerp(1.5f, 0.8f, Mathf.InverseLerp(0.2f, 1.5f, s));
            Sfx.PlayPitched(SfxId.Pop, 0.45f, pitch);
            ScoreKeeper.Bonus(Mathf.RoundToInt(5f + s * 20f), t.position);
            Haptics.Pulse(0.1f, 0.15f, 0.05f);

            var game = PatchworkGame.Instance;
            if (nextMilestone < Milestones.Length && Radius >= Milestones[nextMilestone])
            {
                if (game != null) game.ShowToast("The Tape Ball is as big as " + MilestoneNames[nextMilestone] + "!");
                ScoreKeeper.Bonus(150 * (nextMilestone + 1), c + Vector3.up * Radius);
                Sfx.Play(SfxId.Collect, 0.5f);
                nextMilestone++;
            }
            if (Radius >= StarRadius) BecomeStar();
        }

        void BecomeStar()
        {
            starTime = 0f;
            starStart = transform.position;
            body.isKinematic = true;
            col.enabled = false;
            var g = GetComponent<Grabbable>();
            if (g != null) g.enabled = false;
            StarsMade++;
            ScoreKeeper.Bonus(1500, starStart + Vector3.up * Radius);
            Sfx.Play(SfxId.PrizeFanfare, 0.7f);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, starStart, new Color(1f, 0.9f, 0.5f));
            var game = PatchworkGame.Instance;
            if (game != null) game.ShowToast("Your Tape Ball is big enough to be a star!");
        }

        void Update()
        {
            bumpCooldown -= Time.deltaTime;
            var game = PatchworkGame.Instance;
            if (game != null && game.Player != null && starTime < 0f)
            {
                float d = Vector3.Distance(game.Player.Motor.Center, transform.position);
                if (d < Radius + 9f) Active = this;
                else if (Active == this) Active = null;
            }

            if (starTime >= 0f)
            {
                // Float up into the sky, spinning, shrinking into a star.
                starTime += Time.deltaTime;
                float k = Mathf.Clamp01(starTime / 3.5f);
                float e = k * k * (3f - 2f * k);
                Vector3 sky = new Vector3(starStart.x * 0.5f, 62f, starStart.z * 0.5f + 20f);
                transform.position = Vector3.Lerp(starStart, sky, e);
                transform.Rotate(Vector3.up, 360f * Time.deltaTime, Space.World);
                transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.15f, e);
                if (Active == this) Active = null;
                if (k >= 1f)
                {
                    MakeSkyStar(sky);
                    var fresh = Create(kit, Home);
                    FxBurst.Spawn(FxBurst.Kind.Poof, Home + Vector3.up * 0.6f, Color.white);
                    Sfx.Play(SfxId.Pop, 0.6f);
                    fresh.gameObject.SetActive(true);
                    Destroy(gameObject);
                }
                return;
            }

            // Rolled into the canal or the ravine: bounce back home (keeping what it's picked up).
            if (transform.position.y < -2.5f)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.position = Home + Vector3.up * (Radius + 0.3f);
                transform.position = body.position;
                FxBurst.Spawn(FxBurst.Kind.Poof, transform.position, Color.white);
            }
        }

        // A felt star hung in the sky over town, one for each Tape Ball you finish.
        void MakeSkyStar(Vector3 at)
        {
            if (kit == null) return;
            var g = kit.Group("TapeStar", at);
            Color[] cols = { new Color(1f, 0.85f, 0.3f), new Color(1f, 0.55f, 0.7f), new Color(0.5f, 0.85f, 1f), new Color(0.6f, 0.95f, 0.5f) };
            var m = PatchworkAssets.Glow(cols[StarsMade % cols.Length], 1.4f);
            for (int k = 0; k < 5; k++)
                kit.Deco(PrimitiveType.Sphere, Quaternion.Euler(0f, 0f, k * 72f) * new Vector3(0f, 1.5f, 0f), new Vector3(1.2f, 3.2f, 0.9f), m, new Vector3(0f, 0f, k * 72f), g);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(2f, 2f, 1.2f), m, default(Vector3), g);
            var spin = g.gameObject.AddComponent<Spinner>();
            spin.Speed = 20f;
            spin.BobHeight = 0.6f;
            spin.BobSpeed = 0.5f;
            FxBurst.Spawn(FxBurst.Kind.Sparkle, at, new Color(1f, 0.95f, 0.6f));
        }

        // For the HUD: how far to the next size.
        public string NextGoal
        {
            get { return nextMilestone < Milestones.Length ? "Next: as big as " + MilestoneNames[nextMilestone] : "Next: a star!"; }
        }

        public float Progress { get { return Mathf.InverseLerp(StartRadius, StarRadius, Radius); } }

        void OnDestroy() { if (Active == this) Active = null; }
    }
}

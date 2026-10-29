using UnityEngine;

namespace Patchwork
{
    // A knot of the Fray: a grey tangle of frayed threads with a faint violet glow inside.
    // Jump on it (land on top) to pop it, or crash into it fast (swinging, gusting, bouncing).
    // Bumping into it slowly just pushes you off.
    public class SnarlKnot : PlayerVolume
    {
        public System.Action<SnarlKnot> Popped;
        public bool IsPopped { get; private set; }
        Transform visual;
        float t;
        float bumpCooldown;
        float wobble;

        public static readonly Color FrayGrey = new Color(0.42f, 0.39f, 0.48f);
        public static readonly Color FrayDark = new Color(0.18f, 0.15f, 0.22f);
        public static readonly Color FrayGlow = new Color(0.62f, 0.35f, 0.9f);

        public void Setup(Transform visualRoot)
        {
            visual = visualRoot;
            t = Random.value * 5f;
        }

        // Is the player hitting this hard enough to pop it? (Shared with Snarlings.)
        public static bool HitHard(PatchworkPlayer player, Vector3 center)
        {
            var m = player.Motor;
            var yarn = player.Forms.Ability as YarnAbility;
            if (yarn != null && yarn.Attached) return true;
            bool fromAbove = m.Center.y > center.y + 0.1f && m.Velocity.y < -2f;
            float flat = new Vector2(m.Velocity.x, m.Velocity.z).magnitude;
            return fromAbove || flat > 9f || m.IsDashing;
        }

        // Pushes the player away (and up a little).
        public static void Bump(PatchworkPlayer player, Vector3 center)
        {
            var m = player.Motor;
            Vector3 away = m.Center - center;
            away.y = 0f;
            if (away.sqrMagnitude < 0.01f) away = -player.transform.forward;
            away.Normalize();
            m.Bounce(5f);
            m.Velocity = new Vector3(away.x * 7f, m.Velocity.y, away.z * 7f);
            player.Visuals.PunchScale(new Vector3(1.2f, 0.8f, 1.2f));
            Sfx.Play(SfxId.TransformFail, 0.45f);
            Haptics.Pulse(0.35f, 0.15f, 0.12f);
        }

        void Update()
        {
            if (visual == null) return;
            t += Time.deltaTime;
            bumpCooldown -= Time.deltaTime;
            wobble = Mathf.MoveTowards(wobble, 0f, Time.deltaTime * 3f);
            visual.localRotation = Quaternion.Euler(Mathf.Sin(t * 0.7f) * 15f, t * 25f, Mathf.Cos(t * 0.9f) * 12f);
            float s = 1f + Mathf.Sin(t * 3.1f) * 0.04f + Mathf.Sin(wobble * 25f) * wobble * 0.25f;
            visual.localScale = Vector3.one * s;
        }

        protected override void OnPlayerStay(PatchworkPlayer player, float dt)
        {
            if (IsPopped) return;
            if (HitHard(player, transform.position)) { Pop(player); return; }
            if (bumpCooldown > 0f) return;
            bumpCooldown = 0.5f;
            wobble = 1f;
            Bump(player, transform.position);
        }

        public void Pop(PatchworkPlayer player)
        {
            if (IsPopped) return;
            IsPopped = true;
            if (player != null)
            {
                // A little rebound, like stomping in a Mario game.
                if (player.Motor.Velocity.y < 0f) player.Motor.Bounce(9f);
                Haptics.Pulse(0.5f, 0.3f, 0.12f);
            }
            Sfx.Play(SfxId.Unravel, 0.6f);
            Sfx.Play(SfxId.Boing, 0.35f);
            FxBurst.Spawn(FxBurst.Kind.Threads, transform.position, FrayGrey);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position, new Color(1f, 0.85f, 0.5f));
            gameObject.SetActive(false);
            if (Popped != null) Popped(this);
        }

        // Taken away without the fanfare (already beaten on an earlier visit).
        public void RemoveQuietly()
        {
            IsPopped = true;
            gameObject.SetActive(false);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // A secret marble: a glass marble with a coloured swirl, hidden somewhere clever. Found ones
    // stay saved; going back shows a pale "already found" one worth a few points.
    public class SecretMarble : PlayerVolume
    {
        public static readonly List<SecretMarble> All = new List<SecretMarble>();
        public string Id;
        Transform visual, swirl;
        Vector3 home;
        bool had;
        float phase;

        public static SecretMarble Place(LevelKit kit, Vector3 pos, string id, Color swirlColor, bool hidden = false)
        {
            var m = kit.Volume<SecretMarble>(pos, Vector3.one * 1.9f, "SecretMarble");
            m.Id = id;
            m.home = pos;
            m.phase = pos.x * 0.21f + pos.z * 0.13f;
            m.had = GameState.IsCollected(id);
            m.visual = kit.Group("Visual", pos);
            m.visual.SetParent(m.transform, true);
            var glass = PatchworkAssets.Bubble(m.had ? new Color(0.85f, 0.88f, 0.95f, 0.2f) : new Color(0.9f, 0.95f, 1f, 0.38f));
            var shell = kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 1.0f, glass, default(Vector3), m.visual);
            shell.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m.swirl = kit.Group("Swirl", pos);
            m.swirl.SetParent(m.visual, true);
            var glow = PatchworkAssets.Glow(m.had ? Color.Lerp(swirlColor, Color.white, 0.6f) : swirlColor, m.had ? 0.4f : 1.3f);
            for (int i = 0; i < 3; i++)
            {
                var s = kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.18f, 0.72f, 0.28f), glow, new Vector3(i * 60f, i * 40f, 30f), m.swirl);
                s.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            var shine = kit.Deco(PrimitiveType.Sphere, new Vector3(-0.2f, 0.24f, -0.24f), new Vector3(0.2f, 0.12f, 0.06f), PatchworkAssets.Glow(Color.white, 1.2f), new Vector3(0f, 0f, 35f), m.visual);
            shine.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            if (hidden) m.gameObject.SetActive(false);
            return m;
        }

        protected override void OnEnable() { base.OnEnable(); All.Add(this); }
        protected override void OnDisable() { base.OnDisable(); All.Remove(this); }

        public void Reveal()
        {
            if (gameObject.activeSelf) return;
            gameObject.SetActive(true);
            FxBurst.Spawn(FxBurst.Kind.Poof, transform.position, Color.white);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position, new Color(0.85f, 0.95f, 1f));
            Sfx.Play(SfxId.PrizeFanfare, 0.45f);
            var game = PatchworkGame.Instance;
            if (game != null && !had) game.ShowToast("A secret marble appeared!");
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            var game = player.Game;
            Vector3 p = transform.position;
            if (!had)
            {
                GameState.SetCollected(Id);
                ScoreKeeper.Bonus(1500, p);
                ScoreBubbles.BurstAt(p, 20);
                Sfx.Play(SfxId.PrizeFanfare, 0.75f);
                Sfx.Play(SfxId.Complete, 0.5f);
                Sfx.Voice(SfxId.VoiceYay, player.Forms.Current, 0.55f);
                Haptics.Pulse(0.5f, 0.5f, 0.3f);
                if (game != null) game.ShowSecret();
            }
            else
            {
                ScoreKeeper.Bonus(200, p);
                Sfx.Play(SfxId.Pop, 0.6f);
            }
            FxBurst.Spawn(FxBurst.Kind.Sparkle, p, new Color(0.85f, 0.95f, 1f));
            gameObject.SetActive(false);
        }

        void Update()
        {
            float t = Time.time + phase;
            transform.position = home + Vector3.up * Mathf.Sin(t * 2f) * 0.15f;
            if (swirl != null) swirl.localRotation = Quaternion.Euler(t * 40f, t * 120f, 0f);
        }

        // Moves it (used when settling things so they don't clip into props).
        public void MoveHome(Vector3 p) { home = p; transform.position = p; }
    }

    // A door, a painting, a pipe: go in and come out somewhere else. Doors need a press of
    // Ability; pipes and holes take you as soon as you touch them (some only let Water through).
    public class Warp : PlayerVolume
    {
        public static Warp Active { get; private set; }
        public Vector3 Target;
        public float TargetYaw;
        public bool PressToEnter;
        public bool WaterOnly;
        public string Label = "Enter";
        static float lastWarp = -10f;
        float toastCooldown;

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (!PressToEnter) Go(player);
        }

        protected override void OnPlayerStay(PatchworkPlayer player, float dt)
        {
            toastCooldown -= dt;
            if (!PressToEnter) return;
            Active = this;
            var game = player.Game;
            if (game != null && game.Mode == UIMode.Gameplay && game.Input.Ability.WasPressedThisFrame()) Go(player);
        }

        protected override void OnPlayerExit(PatchworkPlayer player) { if (Active == this) Active = null; }
        protected override void OnDisable()
        {
            base.OnDisable();
            if (Active == this) Active = null;
        }

        void Go(PatchworkPlayer player)
        {
            if (lastWarp > Time.time + 5f) lastWarp = -10f;   // a new Play session
            if (Time.time - lastWarp < 1f) return;
            var game = player.Game;
            if (game == null) return;
            if (WaterOnly && player.Forms.Current != FormType.Water)
            {
                if (toastCooldown <= 0f) { game.ShowToast("Only something runny could squeeze in there..."); toastCooldown = 4f; }
                return;
            }
            lastWarp = Time.time;
            if (Active == this) Active = null;
            player.Hand.Release();
            Sfx.Play(SfxId.PortalWarp, 0.55f);
            game.WarpPlayer(Target, TargetYaw);
        }
    }

    // A glinting spot on the ground. Stand on it and press Grab to dig something up.
    public class DigSpot : PlayerVolume
    {
        public static DigSpot Active { get; private set; }
        public SecretMarble Marble;
        readonly List<Transform> glints = new List<Transform>();
        bool dug;

        public static DigSpot Place(LevelKit kit, Vector3 feet, SecretMarble marble)
        {
            var d = kit.Volume<DigSpot>(feet + Vector3.up * 0.8f, new Vector3(1.8f, 1.8f, 1.8f), "DigSpot");
            d.Marble = marble;
            kit.Deco(PrimitiveType.Sphere, feet + Vector3.up * 0.02f, new Vector3(1.1f, 0.14f, 0.9f), PatchworkAssets.Felt(new Color(0.55f, 0.4f, 0.28f)));
            var g = PatchworkAssets.Glow(new Color(1f, 0.95f, 0.7f), 1.6f);
            for (int i = 0; i < 4; i++)
            {
                float a = i * 1.6f;
                var s = kit.Deco(PrimitiveType.Sphere, feet + new Vector3(Mathf.Cos(a) * 0.3f, 0.14f + (i % 2) * 0.08f, Mathf.Sin(a) * 0.25f), Vector3.one * 0.08f, g, default(Vector3), d.transform);
                s.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                d.glints.Add(s);
            }
            return d;
        }

        protected override void OnPlayerStay(PatchworkPlayer player, float dt)
        {
            if (dug) return;
            Active = this;
            var game = player.Game;
            if (game == null || game.Mode != UIMode.Gameplay || !player.Motor.Grounded) return;
            if (!game.Input.Grab.WasPressedThisFrame()) return;
            dug = true;
            Active = null;
            FxBurst.Spawn(FxBurst.Kind.Scraps, transform.position, new Color(0.55f, 0.4f, 0.28f));
            Sfx.Play(SfxId.Rustle, 0.6f);
            Sfx.Play(SfxId.Pop, 0.5f);
            player.Visuals.PunchScale(new Vector3(1.2f, 0.8f, 1.2f));
            foreach (var s in glints) if (s != null) s.gameObject.SetActive(false);
            if (Marble != null) Marble.Reveal();
        }

        protected override void OnPlayerExit(PatchworkPlayer player) { if (Active == this) Active = null; }
        protected override void OnDisable()
        {
            base.OnDisable();
            if (Active == this) Active = null;
        }

        void Update()
        {
            if (dug) return;
            float t = Time.time;
            for (int i = 0; i < glints.Count; i++)
            {
                float k = Mathf.Max(0f, Mathf.Sin(t * 3f + i * 1.7f));
                glints[i].localScale = Vector3.one * 0.08f * (0.3f + k * k * 1.4f);
            }
        }
    }

    // A shy jack-in-the-box playing hide and seek. Find it and it pops out, giggles, and hides
    // somewhere else. Find it at every spot and it gives you a marble. You can hear it giggling
    // when you're close.
    public class Peekaboo : PlayerVolume
    {
        public Vector3[] Spots;
        public float[] Yaws;
        public SecretMarble Marble;
        public Transform Head;
        int found;
        float pop, giggle, moveTimer = -1f;
        AudioSource voice;

        void Start()
        {
            voice = gameObject.AddComponent<AudioSource>();
            voice.spatialBlend = 1f;
            voice.minDistance = 2f;
            voice.maxDistance = 22f;
            voice.rolloffMode = AudioRolloffMode.Linear;
            voice.playOnAwake = false;
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (Spots == null || found >= Spots.Length || moveTimer > 0f) return;
            found++;
            pop = 1f;
            Sfx.Play(SfxId.Boing, 0.6f);
            Laugh(1.5f);
            var game = player.Game;
            if (found < Spots.Length)
            {
                if (game != null) game.ShowToast(found == 1 ? "Peekaboo! You found me! Bet you can't do it again..." : "Eek! Found again! One more time...");
                moveTimer = 1.4f;
            }
            else
            {
                if (game != null) game.ShowToast("You win! You're the best seeker in Toybox Town. Here!");
                if (Marble != null) { Marble.transform.position = transform.position + Vector3.up * 2.4f; Marble.MoveHome(transform.position + Vector3.up * 2.4f); Marble.Reveal(); }
            }
        }

        void Laugh(float pitch)
        {
            if (voice == null) return;
            var clip = Sfx.Clip(SfxId.VoiceYay);
            if (clip == null) return;
            voice.pitch = pitch * Random.Range(0.95f, 1.08f);
            voice.PlayOneShot(clip, 0.5f * Sfx.Volume / 10f);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            pop = Mathf.MoveTowards(pop, 0f, dt * 1.2f);
            if (Head != null) Head.localPosition = new Vector3(0f, 0.75f + Mathf.Sin(pop * 14f) * pop * 0.6f + pop * 0.5f, 0f);
            if (moveTimer > 0f)
            {
                moveTimer -= dt;
                if (moveTimer <= 0f)
                {
                    FxBurst.Spawn(FxBurst.Kind.Poof, transform.position + Vector3.up * 0.6f, Color.white);
                    transform.position = Spots[found] + Vector3.up * 0.7f;
                    if (Yaws != null && found < Yaws.Length) transform.rotation = Quaternion.Euler(0f, Yaws[found], 0f);
                }
                return;
            }
            // Giggle now and then while Patch is near and it's still hiding.
            if (found >= (Spots != null ? Spots.Length : 0)) return;
            giggle -= dt;
            var game = PatchworkGame.Instance;
            if (giggle <= 0f && game != null && game.Player != null)
            {
                giggle = Random.Range(3f, 5f);
                if (Vector3.Distance(game.Player.transform.position, transform.position) < 20f) Laugh(1.8f);
            }
        }
    }

    // A button that starts a race against the clock. The HUD shows the time left.
    public abstract class TimedChallenge : PlayerVolume
    {
        public static TimedChallenge Running { get; private set; }
        public string Title = "Hurry!";
        public float Duration = 15f;
        public SecretMarble Marble;
        public bool RevealMarbleOnWin = true;
        public Transform Button;
        public float TimeLeft { get; private set; }
        public int Got { get; protected set; }
        public int Total { get; protected set; }
        protected bool Won;
        float press, lastTick;

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (Running != null || Won) return;
            Running = this;
            TimeLeft = Duration;
            Got = 0;
            press = 1f;
            lastTick = Duration;
            Sfx.Play(SfxId.PadClick, 0.6f);
            Sfx.Play(SfxId.MenuConfirm, 0.5f);
            Haptics.Pulse(0.3f, 0.2f, 0.1f);
            OnBegin();
        }

        protected abstract void OnBegin();
        protected abstract void OnEnd(bool won);
        protected virtual void OnTick(float timeLeft) { }

        protected void Count()
        {
            if (Running != this) return;
            Got++;
            if (Got >= Total) Win();
        }

        protected void Win()
        {
            if (Running != this) return;
            Running = null;
            Won = true;
            OnEnd(true);
            Sfx.Play(SfxId.Complete, 0.6f);
            if (RevealMarbleOnWin && Marble != null) Marble.Reveal();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (Running == this) Running = null;
        }

        protected virtual void Update()
        {
            press = Mathf.MoveTowards(press, Running == this ? 1f : 0f, Time.deltaTime * 4f);
            if (Button != null) Button.localPosition = new Vector3(0f, 0.18f - press * 0.12f, 0f);
            if (Running != this) return;
            TimeLeft -= Time.deltaTime;
            if (TimeLeft < 5f && Mathf.Ceil(TimeLeft) < Mathf.Ceil(lastTick)) Sfx.Play(SfxId.Clack, 0.5f);
            lastTick = TimeLeft;
            OnTick(TimeLeft);
            if (TimeLeft <= 0f)
            {
                Running = null;
                OnEnd(false);
                Sfx.Play(SfxId.TransformFail, 0.5f);
                var game = PatchworkGame.Instance;
                if (game != null) game.ShowToast("Out of time! Step on the button to try again.");
            }
        }
    }

    // Purple bubbles appear for a few seconds: grab them all before time runs out.
    public class BubbleRush : TimedChallenge
    {
        public readonly List<RushBubble> Bubbles = new List<RushBubble>();

        protected override void OnBegin()
        {
            Total = Bubbles.Count;
            foreach (var b in Bubbles) { b.gameObject.SetActive(true); b.Pop = 1f; }
        }

        protected override void OnEnd(bool won)
        {
            foreach (var b in Bubbles) if (b != null) b.gameObject.SetActive(false);
        }

        public void Collect(RushBubble b)
        {
            if (Running != this) return;
            b.gameObject.SetActive(false);
            Sfx.PlayPitched(SfxId.ScoreBubble, 0.45f, Mathf.Pow(2f, (Got % 12) / 12f) * 0.9f);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, b.transform.position, new Color(0.8f, 0.55f, 1f));
            ScoreKeeper.Bonus(50, b.transform.position);
            Count();
        }
    }

    public class RushBubble : PlayerVolume
    {
        public BubbleRush Owner;
        public float Pop;
        Vector3 baseScale = Vector3.one;
        bool init;

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (Owner != null) Owner.Collect(this);
        }

        void Update()
        {
            if (!init) { baseScale = transform.localScale; init = true; }
            Pop = Mathf.MoveTowards(Pop, 0f, Time.deltaTime * 3f);
            float w = 1f + Mathf.Sin(Time.time * 6f) * 0.06f + Pop * 0.4f;
            transform.localScale = baseScale * w;
        }
    }

    // Stitched platforms appear for a while, leading up to something.
    public class StitchPath : TimedChallenge
    {
        public Transform[] Platforms;
        float showTime;

        protected override void OnBegin()
        {
            Total = 1;
            showTime = 0f;
            Sfx.Play(SfxId.Unravel, 0.6f);
        }

        protected override void OnTick(float timeLeft)
        {
            showTime += Time.deltaTime;
            for (int i = 0; i < Platforms.Length; i++)
            {
                var p = Platforms[i];
                if (p == null) continue;
                bool on = showTime > i * 0.08f;
                if (on && !p.gameObject.activeSelf)
                {
                    p.gameObject.SetActive(true);
                    Sfx.PlayPitched(SfxId.Pop, 0.3f, 1f + i * 0.05f);
                }
                // Blink near the end.
                var r = p.GetComponent<Renderer>();
                if (r != null) r.enabled = timeLeft > 4f || Mathf.Repeat(timeLeft * 5f, 1f) > 0.35f;
            }
            if (Marble != null && !Marble.gameObject.activeSelf) Win();
        }

        protected override void OnEnd(bool won)
        {
            foreach (var p in Platforms) if (p != null) p.gameObject.SetActive(false);
            Won = false;   // you can always run it again
        }
    }

    // A toy football goal. Get the ball into the net.
    public class GoalNet : MonoBehaviour
    {
        public Grabbable Ball;
        public Vector3 Half = new Vector3(1.2f, 1f, 2f);
        public SecretMarble Marble;
        float resetTimer = -1f;
        bool scoredOnce;

        void Update()
        {
            if (Ball == null) return;
            if (resetTimer > 0f)
            {
                resetTimer -= Time.deltaTime;
                if (resetTimer <= 0f) Ball.ResetHome();
                return;
            }
            Vector3 d = transform.InverseTransformPoint(Ball.transform.position);
            if (Mathf.Abs(d.x) < Half.x && d.y > -0.2f && d.y < Half.y * 2f && Mathf.Abs(d.z) < Half.z)
            {
                resetTimer = 2.5f;
                Sfx.Play(SfxId.Complete, 0.6f);
                Sfx.Play(SfxId.Drum, 0.7f);
                Color[] confetti = { new Color(1f, 0.5f, 0.55f), new Color(1f, 0.85f, 0.35f), new Color(0.5f, 0.75f, 1f) };
                foreach (var c in confetti) FxBurst.Spawn(FxBurst.Kind.Scraps, Ball.transform.position + Vector3.up, c);
                ScoreKeeper.Bonus(300, Ball.transform.position);
                var game = PatchworkGame.Instance;
                if (game != null) game.ShowToast(scoredOnce ? "GOAL! Again!" : "GOOOAL!");
                if (!scoredOnce && Marble != null) Marble.Reveal();
                scoredOnce = true;
            }
        }
    }

    // A trapdoor hidden under a crate. Push the crate off and the lid flips open.
    public class Hatch : MonoBehaviour
    {
        public Grabbable Cover;
        public Transform Lid;
        public SecretMarble Marble;
        bool open;
        float k;

        void Update()
        {
            if (!open && Cover != null)
            {
                Vector3 d = Cover.transform.position - transform.position;
                d.y = 0f;
                if (d.magnitude > 1.8f)
                {
                    open = true;
                    Sfx.Play(SfxId.Lever, 0.55f);
                    if (Marble != null) Marble.Reveal();
                }
            }
            if (open && k < 1f)
            {
                k = Mathf.MoveTowards(k, 1f, Time.deltaTime * 2f);
                if (Lid != null) Lid.localRotation = Quaternion.Euler(-110f * (k * k * (3f - 2f * k)), 0f, 0f);
            }
        }
    }

    // A lost duckling. Touch it and it follows you, waddling along your path. Bring it to its
    // mother at Home.
    public class Duckling : MonoBehaviour
    {
        public Vector3 Home;
        public float HomeRadius = 7f;
        public SecretMarble Marble;
        public Transform Body;
        enum State { Lost, Following, Home }
        State state;
        readonly List<Vector3> crumbs = new List<Vector3>();
        float quack, waddle;
        AudioSource voice;

        void Start()
        {
            voice = gameObject.AddComponent<AudioSource>();
            voice.spatialBlend = 1f;
            voice.minDistance = 2f;
            voice.maxDistance = 20f;
            voice.rolloffMode = AudioRolloffMode.Linear;
            voice.playOnAwake = false;
            if (Marble != null && GameState.IsCollected(Marble.Id))
            {
                state = State.Home;
                transform.position = Home + Vector3.right * 2f;
            }
        }

        void Peep(float pitch)
        {
            var clip = Sfx.Clip(SfxId.Squeak);
            if (voice == null || clip == null) return;
            voice.pitch = pitch * Random.Range(0.95f, 1.1f);
            voice.PlayOneShot(clip, 0.45f * Sfx.Volume / 10f);
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) return;
            float dt = Time.deltaTime;
            var player = game.Player;
            Vector3 pp = player.transform.position;
            quack -= dt;
            switch (state)
            {
                case State.Lost:
                    if (quack <= 0f) { quack = Random.Range(2.5f, 4f); if (Vector3.Distance(pp, transform.position) < 18f) Peep(1.9f); }
                    if (Vector3.Distance(player.Motor.Center, transform.position + Vector3.up * 0.3f) < 1.3f)
                    {
                        state = State.Following;
                        crumbs.Clear();
                        Peep(2.1f);
                        game.ShowToast("Peep! The duckling wants to follow you. Take it home to its mum at the duck pond!");
                    }
                    Bob(0f);
                    break;
                case State.Following:
                {
                    if (crumbs.Count == 0 || Vector3.Distance(crumbs[crumbs.Count - 1], pp) > 0.35f) crumbs.Add(pp);
                    if (crumbs.Count > 60) crumbs.RemoveAt(0);
                    Vector3 target = crumbs.Count > 6 ? crumbs[crumbs.Count - 7] : transform.position;
                    float far = Vector3.Distance(transform.position, pp);
                    if (far > 14f)
                    {
                        transform.position = pp - player.transform.forward * 1.5f;
                        crumbs.Clear();
                        FxBurst.Spawn(FxBurst.Kind.Poof, transform.position + Vector3.up * 0.3f, Color.white);
                    }
                    else
                    {
                        Vector3 step = Vector3.MoveTowards(transform.position, target, Mathf.Max(3f, far * 1.6f) * dt);
                        Vector3 flat = step - transform.position;
                        flat.y = 0f;
                        if (flat.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat), dt * 8f);
                        transform.position = step;
                        Bob(flat.magnitude / Mathf.Max(dt, 0.001f));
                    }
                    if (quack <= 0f) { quack = Random.Range(2f, 3.5f); Peep(2f); }
                    Vector3 h = Home - transform.position;
                    h.y = 0f;
                    if (h.magnitude < HomeRadius)
                    {
                        state = State.Home;
                        Peep(2.2f);
                        Sfx.Play(SfxId.Squeak, 0.5f);
                        game.ShowToast("The duckling's home! Mum's so happy she found something for you.");
                        if (Marble != null) Marble.Reveal();
                    }
                    break;
                }
                case State.Home:
                {
                    // Paddle in little circles near mum.
                    float a = Time.time * 0.6f;
                    Vector3 want = Home + new Vector3(Mathf.Cos(a) * 2f, 0f, Mathf.Sin(a) * 2f);
                    Vector3 flat = want - transform.position;
                    flat.y = 0f;
                    transform.position = Vector3.MoveTowards(transform.position, new Vector3(want.x, Home.y, want.z), dt * 3f);
                    if (flat.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(flat);
                    Bob(0.5f);
                    break;
                }
            }
        }

        void Bob(float speed)
        {
            if (Body == null) return;
            waddle += Time.deltaTime * (3f + speed * 2f);
            Body.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(waddle) * Mathf.Clamp01(speed) * 12f);
            Body.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(waddle)) * Mathf.Clamp01(speed) * 0.06f, 0f);
        }
    }

    // A music box. Step onto its lid and it plays its little tune while the dancer turns.
    public class MusicBox : PlayerVolume
    {
        public Transform Dancer;
        // Semitones above C5 and how long each note lasts.
        public int[] Notes = { 4, 2, 0, 2, 4, 4, 4 };
        public float[] Beats = { 1f, 1f, 1f, 1f, 1f, 1f, 2f };
        float t = -1f, spin;
        int next;

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (t >= 0f) return;
            t = 0f;
            next = 0;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            spin = Mathf.MoveTowards(spin, t >= 0f ? 1f : 0f, dt * 2f);
            if (Dancer != null) Dancer.Rotate(Vector3.up, 160f * spin * dt, Space.Self);
            if (t < 0f) return;
            t += dt;
            float when = 0f;
            for (int i = 0; i < next; i++) when += Beats[i] * 0.42f;
            if (next < Notes.Length && t >= when)
            {
                Sfx.PlayPitched(SfxId.Chime, 0.4f, Mathf.Pow(2f, Notes[next] / 12f) * 0.5f);
                next++;
            }
            if (next >= Notes.Length && t > when + 1.5f) t = -1f;
        }
    }
}

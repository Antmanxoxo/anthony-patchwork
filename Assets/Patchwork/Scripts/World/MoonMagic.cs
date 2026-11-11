using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The Whispering Wood's deepest secret, in the Moonglow Glade. The Gloom (little grey
    // tangles of shadow with glowing eyes) has smothered the moon pool. Find the Moonlit Needle
    // under the wishing willow, bring it to the glade and raise it by the pool: a guardian
    // spirit, a fox made of moonlight yarn, leaps out, races round the standing stones chasing
    // the Gloom away and dives into the pool, which fills with silver light. A secret marble
    // and the Starry Wizard Hat rise out of it.
    public class MoonGlade : MonoBehaviour
    {
        public const string NeedleId = "L1_moon_needle";

        public Vector3 Center;
        public float StoneRadius = 8f;
        public Renderer Pool;
        public Material PoolDark, PoolLit;
        public readonly List<Renderer> Runes = new List<Renderer>();
        public Material RuneDim, RuneLit;
        public readonly List<Transform> Gloom = new List<Transform>();
        public GameObject Moonbeam;
        public Transform Guardian;
        public SecretMarble Marble;
        public PrizeBubble Prize;
        public UseSpot Spot;

        bool solved;
        float t = -1f;
        int popped;

        void Start()
        {
            if (Marble != null && GameState.IsCollected(Marble.Id))
            {
                Solve(true);
                Marble.Reveal();
                if (Prize != null) Prize.Reveal();
            }
            if (Guardian != null) Guardian.gameObject.SetActive(solved);
            if (solved && Guardian != null) Guardian.position = Center + new Vector3(2.2f, 0.05f, -2.4f);
            if (Spot != null) Spot.Used = Use;
        }

        void Use(PatchworkPlayer player)
        {
            var game = player.Game;
            if (solved || t >= 0f) return;
            if (!GameState.IsCollected(NeedleId))
            {
                if (game != null) game.ShowToast("The Gloom is too thick here... Maybe the Moonlit Needle could help. It's said to rest beneath the Wishing Willow, just west of here.");
                Sfx.Play(SfxId.TransformFail, 0.35f);
                return;
            }
            // Raise the needle!
            t = 0f;
            popped = 0;
            if (Spot != null) Spot.gameObject.SetActive(false);
            Sfx.Play(SfxId.Spell, 0.7f);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, player.Motor.Center + Vector3.up * 1.2f, new Color(0.8f, 0.92f, 1f));
            Haptics.Pulse(0.3f, 0.5f, 0.4f);
            if (game != null)
            {
                Vector3 back = player.transform.position - Center;
                back.y = 0f;
                back = back.sqrMagnitude > 0.01f ? back.normalized : Vector3.back;
                game.StartScriptedCutscene(Center + back * 14f + Vector3.up * 7f, Center + Vector3.up, 1.6f);
                game.CutsceneCaption = "A guardian of moonlight!";
            }
            if (Guardian != null)
            {
                Guardian.gameObject.SetActive(true);
                // It bursts out of the needle a few steps in front of Patch, towards the pool
                // (not right on top of him).
                Vector3 toPool = Center - player.transform.position;
                toPool.y = 0f;
                toPool = toPool.sqrMagnitude > 0.01f ? toPool.normalized : Vector3.forward;
                Guardian.position = player.transform.position + toPool * 2.6f + Vector3.up * 0.2f;
                Guardian.rotation = Quaternion.LookRotation(-toPool, Vector3.up);
                Guardian.localScale = Vector3.one * 0.1f;
            }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            // The Gloom drifts and puffs out little bits of shadow.
            if (!solved)
                foreach (var g in Gloom)
                    if (g != null && g.gameObject.activeSelf && Random.value < dt * 0.5f)
                        FxBurst.Spawn(FxBurst.Kind.Steam, g.position, new Color(0.3f, 0.28f, 0.36f));

            if (t < 0f) return;
            t += dt;
            var game = PatchworkGame.Instance;
            if (Guardian == null) { Finish(game); return; }

            // 0-0.6 s: the guardian bursts out and grows. 0.6-3.8 s: two laps of the stones,
            // popping the Gloom. 3.8-4.6 s: a leap into the pool. Then the pool lights up.
            Vector3 p;
            float yaw;
            if (t < 0.6f)
            {
                float k = t / 0.6f;
                Guardian.localScale = Vector3.one * Mathf.Lerp(0.1f, 1f, 1f - (1f - k) * (1f - k));
                p = Guardian.position + Vector3.up * dt * 2f;
                yaw = Guardian.eulerAngles.y + dt * 360f;
            }
            else if (t < 3.8f)
            {
                float k = (t - 0.6f) / 3.2f;
                float a = k * Mathf.PI * 4f;
                float r = StoneRadius - 2.2f + Mathf.Sin(k * Mathf.PI * 6f) * 0.5f;
                p = Center + new Vector3(Mathf.Cos(a) * r, 0.4f + Mathf.Abs(Mathf.Sin(a * 3f)) * 0.6f, Mathf.Sin(a) * r);
                Vector3 vel = new Vector3(-Mathf.Sin(a), 0f, Mathf.Cos(a));
                yaw = Mathf.Atan2(vel.x, vel.z) * Mathf.Rad2Deg;
                Guardian.localScale = Vector3.one;
                // Chase the Gloom away.
                foreach (var g in Gloom)
                {
                    if (g == null || !g.gameObject.activeSelf) continue;
                    if ((g.position - p).sqrMagnitude < 9f || k > 0.9f)
                    {
                        g.gameObject.SetActive(false);
                        popped++;
                        FxBurst.Spawn(FxBurst.Kind.Poof, g.position, new Color(0.5f, 0.48f, 0.6f));
                        FxBurst.Spawn(FxBurst.Kind.Sparkle, g.position, new Color(0.8f, 0.92f, 1f));
                        Sfx.PlayPitched(SfxId.Pop, 0.5f, 1f + popped * 0.08f);
                    }
                }
            }
            else if (t < 4.6f)
            {
                float k = (t - 3.8f) / 0.8f;
                Vector3 from = Center + new Vector3(StoneRadius - 2.2f, 0.4f, 0f);
                p = Vector3.Lerp(from, Center, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 4f;
                yaw = 270f;
                Guardian.localScale = Vector3.one * Mathf.Lerp(1f, 0.2f, k * k);
            }
            else
            {
                Finish(game);
                return;
            }
            Guardian.position = p;
            Guardian.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (Random.value < dt * 30f) FxBurst.Spawn(FxBurst.Kind.Sparkle, p + Vector3.up * 0.5f, new Color(0.75f, 0.9f, 1f));
            if (game != null && game.Mode == UIMode.Cutscene) game.SetCutsceneCamera(Center + (Center - p).normalized * 2f + new Vector3(0f, 7f, 0f) + Quaternion.Euler(0f, t * 20f, 0f) * new Vector3(0f, 0f, -13f), Vector3.Lerp(Center, p, 0.5f) + Vector3.up);
        }

        void Finish(PatchworkGame game)
        {
            t = -1f;
            Solve(false);
            if (Guardian != null)
            {
                Guardian.localScale = Vector3.one;
                Guardian.position = Center + new Vector3(2.2f, 0.05f, -2.4f);
                Guardian.rotation = Quaternion.LookRotation(-new Vector3(2.2f, 0f, -2.4f).normalized, Vector3.up);
            }
            FxBurst.Spawn(FxBurst.Kind.Splash, Center + Vector3.up * 0.3f, new Color(0.8f, 0.9f, 1f));
            FxBurst.Spawn(FxBurst.Kind.Sparkle, Center + Vector3.up * 1.5f, Color.white);
            Sfx.Play(SfxId.PrizeFanfare, 0.6f);
            Sfx.Play(SfxId.Twinkle, 0.5f);
            if (Marble != null) Marble.Reveal();
            if (Prize != null) Prize.Reveal();
            if (game != null)
            {
                game.CutsceneCaption = null;
                if (game.Mode == UIMode.Cutscene) game.EndCutscene();
                game.ShowToast("The moon pool shines again!");
            }
        }

        void Solve(bool quiet)
        {
            solved = true;
            if (Pool != null) Pool.sharedMaterial = PoolLit;
            foreach (var r in Runes) if (r != null) r.sharedMaterial = RuneLit;
            foreach (var g in Gloom) if (g != null) g.gameObject.SetActive(false);
            if (Moonbeam != null) Moonbeam.SetActive(true);
            if (Spot != null) Spot.gameObject.SetActive(false);
        }
    }

    // The Moonlit Needle: a silver knitting needle with a star on top, floating under the
    // wishing willow. Touch it to take it.
    public class MoonNeedle : PlayerVolume
    {
        public Transform Visual;
        float t;

        void Start()
        {
            if (GameState.IsCollected(MoonGlade.NeedleId)) gameObject.SetActive(false);
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            GameState.SetCollected(MoonGlade.NeedleId);
            GameState.Save();
            Sfx.Play(SfxId.PrizeFanfare, 0.6f);
            Sfx.Play(SfxId.Spell, 0.4f);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position, new Color(0.8f, 0.92f, 1f));
            Haptics.Pulse(0.3f, 0.3f, 0.2f);
            if (player.Game != null) player.Game.ShowToast("You found the Moonlit Needle! Raise it where the Gloom gathers, in the Moonglow Glade.");
            gameObject.SetActive(false);
        }

        void Update()
        {
            t += Time.deltaTime;
            if (Visual != null)
            {
                Visual.localPosition = new Vector3(0f, Mathf.Sin(t * 1.6f) * 0.15f, 0f);
                Visual.localRotation = Quaternion.Euler(0f, t * 60f, 18f);
            }
            if (Random.value < Time.deltaTime * 3f) FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position + Random.insideUnitSphere * 0.4f, new Color(0.8f, 0.92f, 1f));
        }
    }

    // Something floating gently up and down (the glade's candles), each at its own pace.
    public class Floater : MonoBehaviour
    {
        public float Height = 0.3f, Speed = 0.8f;
        Vector3 home;
        float seed;
        void Start() { home = transform.localPosition; seed = Random.value * 10f; }
        void Update() { transform.localPosition = home + Vector3.up * Mathf.Sin(Time.time * Speed + seed) * Height; }
    }
}

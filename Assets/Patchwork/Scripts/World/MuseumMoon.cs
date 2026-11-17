using UnityEngine;

namespace Patchwork
{
    // An Easter egg: the Moon That Wouldn't Wait.
    //
    // On the quay stands a little wind-up clock tower, a toy. Wind it, and the button moon over
    // the museum turns round, grows a face (the top two buttonholes are its eyes, glaring, with
    // a stitched grimace under them), and begins to come down. Three stitches of the night are
    // left, and the screen says so at the start of each: "Dawn of the First Stitch", and so on.
    // The ground rumbles, harder as it gets closer, and on the Final Stitch the music stops.
    //   - Get back to the little clock tower and wind it backwards in time, and the whole night
    //     goes back three stitches: the moon zooms home, looking put out.
    //   - Or don't, and when time runs out the moon stops just over the roof, blinks, yawns
    //     enormously, and goes home: it only wanted a closer look at the whale.
    // Either way is the secret "moon" (the first time).
    public class ButtonMoonEgg : MonoBehaviour
    {
        public static ButtonMoonEgg Instance { get; private set; }
        public Transform Moon, Face, Mouth, Toy, KeyTurn;
        public Transform[] Eyes;
        public UseSpot Spot;

        enum State { Idle, Falling, Rewinding, Arrived, Leaving }
        State state;
        float t, rumble, banner = -1f, bannerAge;
        string bannerTitle, bannerSub;
        int stage = -1;
        Vector3 home, from;
        Quaternion homeRot;
        bool silenced;

        const float Duration = 90f;
        static readonly float[] StageAt = { 0f, 36f, 66f };
        static readonly string[] StageTitle = { "Dawn of the First Stitch", "Dawn of the Second Stitch", "Dawn of the Final Stitch" };
        static readonly string[] StageSub = { "- 3 Stitches Remain -", "- 2 Stitches Remain -", "- 1 Stitch Remains -" };
        static readonly Vector3 Over = new Vector3(0f, 78f, -70f);   // right over the museum, filling the sky above the doors

        void Awake() { Instance = this; }
        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (silenced) MusicPlayer.Silence = false;
        }

        void Start()
        {
            if (Moon == null) { enabled = false; return; }
            home = Moon.position;
            homeRot = Moon.rotation;
            if (Face != null) Face.gameObject.SetActive(false);
            if (Spot != null) Spot.Used = p => Wind();
        }

        void Wind()
        {
            var game = PatchworkGame.Instance;
            if (game == null) return;
            if (KeyTurn != null) KeyTurn.localRotation *= Quaternion.Euler(0f, 0f, 90f);
            if (state == State.Idle)
            {
                state = State.Falling;
                t = 0f;
                stage = -1;
                rumble = 1f;
                if (Face != null) Face.gameObject.SetActive(true);
                Sfx.Play(SfxId.BigBell, 0.6f);
                FxBurst.Spawn(FxBurst.Kind.Sparkle, Toy != null ? Toy.position + Vector3.up * 1.6f : game.Player.transform.position, new Color(1f, 0.8f, 0.4f));
                if (Spot != null) Spot.Label = "Wind it backwards!";
            }
            else if (state == State.Falling)
            {
                // Wound back in time: the night goes back three stitches.
                state = State.Rewinding;
                t = 0f;
                from = Moon.position;
                Sfx.Play(SfxId.Whoosh, 0.8f);
                Sfx.Play(SfxId.BigBell, 0.6f);
                game.Blackout(0.6f);
                ShowBanner("Dawn of the First Stitch", "- Again -");
                Unsilence();
                if (Level2Quest.Instance != null) Level2Quest.Instance.Secret("moon");
                Narrator.Say("Patch wound the little clock tower backwards, and the whole night went back with it, three stitches' worth. The moon went home looking, if anything, rather put out.");
                if (Spot != null) Spot.Label = "Wind the little clock tower";
            }
        }

        void ShowBanner(string title, string sub)
        {
            bannerTitle = title;
            bannerSub = sub;
            banner = 0f;
            bannerAge = 0f;
        }

        void Unsilence()
        {
            if (!silenced) return;
            silenced = false;
            MusicPlayer.Silence = false;
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) return;
            float dt = Time.deltaTime;
            if (banner >= 0f)
            {
                bannerAge += Time.unscaledDeltaTime;
                if (bannerAge > 4.2f) banner = -1f;
            }
            // (The night stands still while you're talking, or watching something.)
            bool running = game.Mode == UIMode.Gameplay;
            Vector3 eye = game.CameraRig != null ? game.CameraRig.transform.position : game.Player.transform.position;
            switch (state)
            {
                case State.Falling:
                {
                    if (running) t += dt;
                    int now = t >= StageAt[2] ? 2 : t >= StageAt[1] ? 1 : 0;
                    if (now != stage)
                    {
                        stage = now;
                        ShowBanner(StageTitle[now], StageSub[now]);
                        Sfx.Play(SfxId.GateRumble, 0.8f);
                        if (now == 2 && !silenced) { silenced = true; MusicPlayer.Silence = true; }
                    }
                    float k = Mathf.Clamp01(t / Duration);
                    float ease = k * k * (3f - 2f * k) * 0.35f + k * k * 0.65f;   // slow at first, then faster
                    Moon.position = Vector3.Lerp(home, Over, ease);
                    Moon.rotation = Quaternion.LookRotation((eye - Moon.position).normalized, Vector3.up);
                    Moon.localScale = Vector3.one * Mathf.Lerp(1f, 1.35f, k);
                    // The ground shakes, more and more often, harder and harder.
                    rumble -= running ? dt : 0f;
                    if (rumble <= 0f)
                    {
                        rumble = Mathf.Lerp(7f, 1.6f, k);
                        Sfx.Play(SfxId.GateRumble, Mathf.Lerp(0.25f, 0.9f, k));
                        if (game.CameraRig != null) game.CameraRig.Kick(Mathf.Lerp(0.06f, 0.4f, k));
                    }
                    // A glare that gets angrier.
                    foreach (var e in Eyes) if (e != null) e.localScale = new Vector3(1f, 1f + Mathf.Sin(Time.time * (2f + k * 6f)) * 0.08f * k, 1f);
                    if (t >= Duration)
                    {
                        state = State.Arrived;
                        t = 0f;
                        if (Level2Quest.Instance != null) Level2Quest.Instance.Secret("moon");
                        if (Spot != null) Spot.Label = "Wind the little clock tower";
                    }
                    break;
                }
                case State.Arrived:
                {
                    t += dt;
                    Moon.rotation = Quaternion.LookRotation((eye - Moon.position).normalized, Vector3.up);
                    // A long look. A blink. An enormous yawn.
                    float blink = t > 1.4f && t < 1.7f ? 0.08f : 1f;
                    foreach (var e in Eyes) if (e != null) e.localScale = new Vector3(1f, blink, 1f);
                    float yawn = Mathf.Sin(Mathf.PI * Mathf.Clamp01((t - 2.2f) / 2.4f));
                    if (Mouth != null) Mouth.localScale = new Vector3(1f - yawn * 0.35f, 1f + yawn * 2.4f, 1f);
                    if (t > 2.2f && t - dt <= 2.2f)
                    {
                        Sfx.Play(SfxId.Whoosh, 0.6f);
                        Unsilence();
                        Narrator.Say("And then the moon, which had come all that way, only blinked, and yawned enormously, and said it had just wanted a closer look at the whale. Then it went home, rather sheepishly.");
                    }
                    if (t > 5.5f)
                    {
                        state = State.Leaving;
                        t = 0f;
                        from = Moon.position;
                    }
                    break;
                }
                case State.Rewinding:
                case State.Leaving:
                {
                    t += dt;
                    float len = state == State.Rewinding ? 2.4f : 7f;
                    float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / len));
                    Moon.position = Vector3.Lerp(from, home, k);
                    Moon.localScale = Vector3.one * Mathf.Lerp(Moon.localScale.x, 1f, k);
                    Moon.rotation = Quaternion.Slerp(Quaternion.LookRotation((eye - Moon.position).normalized, Vector3.up), homeRot, k);
                    if (t >= len)
                    {
                        Moon.position = home;
                        Moon.rotation = homeRot;
                        Moon.localScale = Vector3.one;
                        if (Face != null) Face.gameObject.SetActive(false);
                        if (Mouth != null) Mouth.localScale = Vector3.one;
                        foreach (var e in Eyes) if (e != null) e.localScale = Vector3.one;
                        state = State.Idle;
                    }
                    break;
                }
            }
        }

        // The banners (drawn by the HUD, over the game).
        public static void DrawOverlay()
        {
            var egg = Instance;
            if (egg == null) return;
            if (egg.banner >= 0f && egg.bannerTitle != null)
            {
                float age = egg.bannerAge;
                float a = Mathf.Clamp01(age / 0.6f) * Mathf.Clamp01((4.2f - age) / 0.8f);
                float cx = UIKit.Width * 0.5f, cy = UIKit.Height * 0.36f;
                var title = UIKit.Style(64, true, TextAnchor.MiddleCenter, new Color(1f, 0.97f, 0.9f, a));
                var sub = UIKit.Style(34, true, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.6f, a));
                UIKit.Fill(new Rect(cx - 520f, cy - 70f, 1040f, 150f), new Color(0f, 0f, 0f, 0.35f * a), 30f);
                UIKit.Label(new Rect(cx - 520f, cy - 64f, 1040f, 84f), egg.bannerTitle, title, true);
                UIKit.Label(new Rect(cx - 520f, cy + 18f, 1040f, 48f), egg.bannerSub, sub, true);
            }
            if (egg.state == State.Falling)
            {
                // How long is left, top and centre.
                float left = Mathf.Max(0f, Duration - egg.t);
                int s = Mathf.CeilToInt(left);
                string clock = (s / 60) + ":" + (s % 60).ToString("00");
                float cx = UIKit.Width * 0.5f;
                var r = new Rect(cx - 120f, 22f, 240f, 64f);
                bool urgent = egg.stage >= 2;
                UIKit.Panel(r, new Color(0.14f, 0.1f, 0.2f, 0.9f), urgent ? new Color(1f, 0.4f, 0.35f, 0.95f) : new Color(1f, 0.85f, 0.5f, 0.9f), 20f);
                UIKit.Circle(new Vector2(r.x + 34f, r.center.y), 40f, new Color(1f, 0.95f, 0.8f));
                for (int i = 0; i < 4; i++)
                    UIKit.Circle(new Vector2(r.x + 34f + (i % 2 == 0 ? -6f : 6f), r.center.y + (i < 2 ? -6f : 6f)), 8f, new Color(0.72f, 0.7f, 0.66f));
                float pulse = urgent ? 1f + Mathf.Sin(Time.unscaledTime * 8f) * 0.06f : 1f;
                UIKit.Label(new Rect(r.x + 60f, r.y, 170f, r.height), clock, UIKit.Style(Mathf.RoundToInt(36 * pulse), true, TextAnchor.MiddleCenter, urgent ? new Color(1f, 0.55f, 0.5f) : new Color(1f, 0.95f, 0.85f)));
            }
        }
    }

    public static partial class Level2Builder
    {
        static Transform skyMoon;

        // The little wind-up clock tower on the quay, and the face the moon keeps to itself.
        static void MoonEgg(LevelKit kit)
        {
            if (skyMoon == null) return;
            Vector3 foot;
            // Right beside the path from the Pod, where you can't miss it (just ahead and to the right).
            if (!RmFloor("the wind-up clock tower", new Vector3(4.6f, 0f, -141f), new Vector3(1.6f, 2.6f, 1.6f), 0f, 5f, out foot)) return;
            Debug.Log("[Level2] Wind-up clock tower on the quay at " + foot);
            var egg = kit.Root.gameObject.AddComponent<ButtonMoonEgg>();
            egg.Moon = skyMoon;

            // ----- The toy: a clock tower about as tall as Patch, on a little plinth -----
            var toy = kit.Group("WindUpClockTower", foot);
            toy.rotation = Rot(270f);   // its clock face toward the path up to the doors
            egg.Toy = toy;
            var cream = PatchworkAssets.Felt(CreamCol);
            var red = PatchworkAssets.Felt(new Color(0.78f, 0.3f, 0.32f));
            var roof = PatchworkAssets.Felt(new Color(0.3f, 0.36f, 0.62f));
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.25f, 0f), new Vector3(1.3f, 0.5f, 1.3f), PaleStone, default(Vector3), toy);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.15f, 0f), new Vector3(0.8f, 1.3f, 0.8f), cream, default(Vector3), toy);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.72f, 0.41f), new Vector3(0.3f, 0.44f, 0.04f), red, default(Vector3), toy);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.86f, 0f), new Vector3(0.92f, 0.14f, 0.92f), red, default(Vector3), toy);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 2.18f, 0f), new Vector3(0.62f, 0.62f, 0.62f), roof, new Vector3(45f, 45f, 0f), toy);
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 2.66f, 0f), Vector3.one * 0.14f, PatchworkAssets.Glow(new Color(1f, 0.85f, 0.4f), 1.6f), default(Vector3), toy));
            // A warm little light on it, so it stands out on the dark quay.
            PointLight(kit, foot + new Vector3(0f, 3.2f, 0f), new Color(1f, 0.82f, 0.5f), 6f, 1.6f);
            // The clock face, at ten to twelve.
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.42f, 0.41f), new Vector3(0.56f, 0.02f, 0.56f), PatchworkAssets.Glow(new Color(1f, 0.96f, 0.85f), 0.6f), new Vector3(90f, 0f, 0f), toy));
            var ink = PatchworkAssets.Felt(new Color(0.15f, 0.12f, 0.18f));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.52f, 0.43f), new Vector3(0.03f, 0.2f, 0.01f), ink, default(Vector3), toy));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(-0.06f, 1.46f, 0.43f), new Vector3(0.025f, 0.15f, 0.01f), ink, new Vector3(0f, 0f, 60f), toy));
            // The big brass key on its side.
            var key = kit.Group("WindingKey", toy.TransformPoint(new Vector3(0.44f, 1.1f, 0f)));
            key.SetParent(toy, true);
            key.localRotation = Quaternion.Euler(0f, 90f, 0f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.12f), new Vector3(0.08f, 0.12f, 0.08f), Brass, new Vector3(90f, 0f, 0f), key);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.16f, 0f, 0.26f), new Vector3(0.24f, 0.16f, 0.05f), Brass, default(Vector3), key);
            egg.KeyTurn = key;
            TownKit.Solid(kit, foot + Vector3.up * 1.2f, new Vector3(1.3f, 2.4f, 1.3f), 0f);
            var spot = kit.Volume<UseSpot>(foot + Vector3.up * 1.2f, new Vector3(3.2f, 2.6f, 3.2f), "ClockTowerSpot");
            spot.Label = "Wind the little clock tower";
            spot.PromptPoint = foot + Vector3.up * 3f;
            egg.Spot = spot;
            Plaque(kit, foot + Rot(270f) * new Vector3(1.4f, 0f, 0.6f), 270f, "The Clock Tower",
                "The very first clocks in church towers had no faces at all. They simply rang a bell on the hour, and the word clock comes from an old word for bell.",
                "Clock faces with hands came later, so that people could tell the time between the bells.");

            // ----- The moon's face (hidden till it's needed) -----
            var face = kit.Group("MoonFace", skyMoon.position);
            face.SetParent(skyMoon, false);
            face.localPosition = Vector3.zero;
            face.localRotation = Quaternion.identity;
            egg.Face = face;
            var glare = PatchworkAssets.Glow(new Color(1f, 0.62f, 0.2f), 2.2f);
            var dark = PatchworkAssets.Plain(new Color(0.1f, 0.06f, 0.08f), 0.4f);
            var thread = PatchworkAssets.Felt(new Color(0.35f, 0.22f, 0.25f));
            egg.Eyes = new Transform[2];
            for (int s = -1; s <= 1; s += 2)
            {
                // The top two buttonholes are its eyes: a fiery iris, a tiny pupil, and a brow
                // stitched down hard at the inside end.
                var eye = new GameObject("MoonEye").transform;
                eye.SetParent(face, false);
                eye.localPosition = new Vector3(s * 5f, 5f, 3.2f);
                NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(4.2f, 4.2f, 0.9f), glare, default(Vector3), eye));
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(-s * 0.4f, -0.3f, 0.35f), new Vector3(1.3f, 1.3f, 0.5f), dark, default(Vector3), eye));
                egg.Eyes[s < 0 ? 0 : 1] = eye;
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 5.4f, 9.2f, 3.3f), new Vector3(7.5f, 1.3f, 0.6f), thread, new Vector3(0f, 0f, s * 22f), face));
            }
            // A grimace sewn across the bottom: a dark band with a row of big cream stitches.
            var mouth = new GameObject("MoonMouth").transform;
            mouth.SetParent(face, false);
            mouth.localPosition = new Vector3(0f, -11.5f, 3.15f);
            egg.Mouth = mouth;
            NoShadow(kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(24f, 3.4f, 0.5f), dark, default(Vector3), mouth));
            for (int i = 0; i < 9; i++)
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(-10.4f + i * 2.6f, 0f, 0.3f), new Vector3(1.5f, 2.8f, 0.3f), cream, default(Vector3), mouth));
            for (int s = -1; s <= 1; s += 2)
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 12.6f, 1.2f, 0.2f), new Vector3(2.4f, 0.8f, 0.4f), thread, new Vector3(0f, 0f, -s * 30f), mouth));
        }
    }
}

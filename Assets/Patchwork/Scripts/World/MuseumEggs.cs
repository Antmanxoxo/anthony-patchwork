using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Two things hidden in the Oceanic Museum for people who like that sort of thing.
    //
    // THE LONG GALLERY (a nod to a certain famous looping hallway). In the Night Sky Gallery,
    // on the way to the Observatory, hangs a crooked portrait of the museum's Founder.
    // Straighten it and it goes crooked again as soon as you walk off. Straighten it a third
    // time and the lamps go out, the music stops, Professor Hoot is gone, and the gallery has no
    // end: walk as far as you like, it is always the same six yards. It gets stranger the
    // further you go (the Founder's eyes follow you; every picture becomes the same picture;
    // then they're upside down; then there are ducks; then the ducks are all looking at you).
    // At last somebody tall is waiting at the end of the hall. Walk up to them, the lights go
    // out... and it's a coat stand under a dust sheet, in a party hat, holding a cake. The ducks
    // came for the cake. You get the Party Hat.
    //
    // THE DUST SHEET FIVE (a nod to a certain nervous plumber's brother and his vacuum cleaner).
    // In the History hall five pieces of furniture are under dust sheets, and something giggles
    // under each. Only wind shifts a sheet: a Wind strike (or the gust) blows it off, and out
    // shoots a dust bunny, which has to be hoovered up with another Wind strike before it gets
    // back under cover. Catch all five and Mrs. Sudsbury has something to say about it.
    public static partial class Level2Builder
    {
        static void Eggs(LevelKit kit)
        {
            var root = kit.Group("Level2Eggs", Vector3.zero);

            // ----- The Long Gallery -----
            var hall = root.gameObject.AddComponent<LongGallery>();
            hall.Y = DeckY;
            var spot = kit.Volume<UseSpot>(new Vector3(34.4f, DeckY + 1.3f, LongGallery.PortraitZ), new Vector3(2.8f, 2.6f, 2.6f), "FounderPortraitSpot");
            spot.Label = "Straighten the picture";
            spot.PromptPoint = new Vector3(33.7f, DeckY + 2.3f, LongGallery.PortraitZ);
            spot.Used = p => hall.Straighten();
            hall.Spot = spot;

            // ----- The Dust Sheet Five -----
            var hunt = root.gameObject.AddComponent<DustyHunt>();
            Vector3[] want = { new Vector3(-70f, 0f, -30f), new Vector3(-48f, 0f, -40f), new Vector3(-76f, 0f, -58f), new Vector3(-52f, 0f, -66f), new Vector3(-64f, 0f, -88f) };
            hunt.Spots = new Vector3[want.Length];
            hunt.Yaws = new float[want.Length];
            for (int i = 0; i < want.Length; i++)
            {
                Vector3 foot;
                hunt.Yaws[i] = i * 67f;
                hunt.Spots[i] = RmFloor("a dust sheet", want[i], new Vector3(2.2f, 2.2f, 2.2f), 0f, 8f, out foot) ? foot : want[i];
            }
        }
    }

    // ====================================================================== The Long Gallery

    public class LongGallery : MonoBehaviour
    {
        public const float PortraitZ = -26.2f;
        const float Period = 6f, LoopLo = -29f, LoopHi = -23f;
        const float WallX = 33.07f, MidX = 36f;

        public float Y;
        public UseSpot Spot;

        enum State { Idle, Starting, Looping, Reveal, Done }
        State state;
        int straightened, wraps, stage;
        bool straight;
        float tilt = 9f, timer, nextNoise = 3f, t;
        Transform[] frames = new Transform[3], tilts = new Transform[3];
        Transform[][] eyes = new Transform[3][];
        readonly List<Transform> ducks = new List<Transform>(), duckHats = new List<Transform>();
        Transform figure, shroud, party;
        Light[] own;
        readonly List<Light> dimmed = new List<Light>();
        readonly List<float> dimmedWas = new List<float>();
        ToyNpc hoot;

        static bool Solved { get { return Level2Quest.Instance != null && Level2Quest.Instance.HasSecret("hall"); } }

        void Start()
        {
            for (int i = 0; i < 3; i++)
            {
                frames[i] = BuildPortrait(i, new Vector3(WallX, Y + 2.75f, PortraitZ + (i - 1) * Period));
                if (i != 1) frames[i].gameObject.SetActive(false);
            }
            foreach (var n in FindObjectsByType<ToyNpc>(FindObjectsInactive.Exclude))
                if (n != null && n.Name == "Professor Hoot") hoot = n;

            // Three lamps of our own, one every six yards, for when the real ones are out.
            own = new Light[3];
            for (int i = 0; i < 3; i++)
            {
                var go = new GameObject("LongGalleryLamp");
                go.transform.SetParent(transform, false);
                go.transform.position = new Vector3(MidX, Y + 4.4f, -32f + i * Period);
                own[i] = go.AddComponent<Light>();
                own[i].type = LightType.Point;
                own[i].range = 7f;
                own[i].shadows = LightShadows.None;
                own[i].enabled = false;
            }

            if (Solved)
            {
                // Been and done: the picture hangs straight, and the party's still going.
                state = State.Done;
                straight = true;
                tilt = 0f;
                if (Spot != null) Spot.gameObject.SetActive(false);
                BuildParty(new Vector3(37.7f, Y, -24.6f));
                for (int i = 0; i < 4; i++) BuildDuck(new Vector3(36.4f + (i % 2) * 0.7f, Y, -25.6f + i * 0.6f), true).rotation = Quaternion.Euler(0f, 70f + i * 15f, 0f);
            }
        }

        void OnDestroy() { if (state == State.Looping || state == State.Starting || state == State.Reveal) MusicPlayer.LiftHush = false; }

        static Transform Part(PrimitiveType type, Transform parent, Vector3 p, Vector3 s, Material m, Vector3 e = default(Vector3))
        {
            var part = Monsters.Part(type, parent, p, s, m, e);
            part.gameObject.layer = 0;
            return part;
        }

        // The Founder: a very stern teddy bear in an admiral's hat. (Local +z is out of the wall.)
        Transform BuildPortrait(int which, Vector3 at)
        {
            var g = new GameObject("FounderPortrait").transform;
            g.SetParent(transform, false);
            g.position = at;
            g.rotation = Quaternion.Euler(0f, 90f, 0f);
            // The nail it hangs from, and the pale patch on the wall where it has hung straight.
            Part(PrimitiveType.Sphere, g, new Vector3(0f, 0.95f, 0.02f), Vector3.one * 0.06f, PatchworkAssets.Plain(new Color(0.3f, 0.3f, 0.34f), 0.6f));
            var tl = new GameObject("Tilt").transform;
            tl.SetParent(g, false);
            tl.localPosition = new Vector3(0f, 0.95f, 0f);
            tilts[which] = tl;
            var gold = PatchworkAssets.Plain(new Color(0.85f, 0.65f, 0.3f), 0.7f);
            var fur = PatchworkAssets.Knit(new Color(0.62f, 0.45f, 0.3f));
            var navy = PatchworkAssets.Felt(new Color(0.12f, 0.15f, 0.34f));
            var black = PatchworkAssets.Plain(new Color(0.05f, 0.04f, 0.06f), 0.8f);
            Part(PrimitiveType.Cube, tl, new Vector3(0f, -0.95f, 0.04f), new Vector3(1.5f, 1.9f, 0.08f), gold);
            Part(PrimitiveType.Cube, tl, new Vector3(0f, -0.95f, 0.085f), new Vector3(1.26f, 1.66f, 0.02f), PatchworkAssets.Felt(new Color(0.16f, 0.26f, 0.22f)));
            // Shoulders, epaulettes, a row of buttons.
            Part(PrimitiveType.Sphere, tl, new Vector3(0f, -1.55f, 0.1f), new Vector3(1.1f, 0.8f, 0.06f), navy);
            for (int s = -1; s <= 1; s += 2) Part(PrimitiveType.Sphere, tl, new Vector3(s * 0.46f, -1.3f, 0.12f), new Vector3(0.26f, 0.12f, 0.05f), gold);
            for (int i = 0; i < 3; i++) Part(PrimitiveType.Sphere, tl, new Vector3(0f, -1.4f - i * 0.14f, 0.13f), Vector3.one * 0.06f, gold);
            // Head, ears, muzzle, and a frown you could hang a coat on.
            Part(PrimitiveType.Sphere, tl, new Vector3(0f, -0.82f, 0.11f), new Vector3(0.72f, 0.66f, 0.07f), fur);
            for (int s = -1; s <= 1; s += 2) Part(PrimitiveType.Sphere, tl, new Vector3(s * 0.32f, -0.52f, 0.1f), new Vector3(0.22f, 0.22f, 0.05f), fur);
            Part(PrimitiveType.Sphere, tl, new Vector3(0f, -0.92f, 0.13f), new Vector3(0.3f, 0.24f, 0.05f), PatchworkAssets.Knit(new Color(0.85f, 0.75f, 0.6f)));
            Part(PrimitiveType.Sphere, tl, new Vector3(0f, -0.86f, 0.15f), new Vector3(0.1f, 0.07f, 0.03f), black);
            Part(PrimitiveType.Cube, tl, new Vector3(0f, -1f, 0.15f), new Vector3(0.16f, 0.02f, 0.01f), black);
            for (int s = -1; s <= 1; s += 2)
            {
                Part(PrimitiveType.Cube, tl, new Vector3(s * 0.16f, -0.66f, 0.15f), new Vector3(0.16f, 0.03f, 0.01f), black, new Vector3(0f, 0f, s * -18f));
                Part(PrimitiveType.Sphere, tl, new Vector3(s * 0.16f, -0.75f, 0.14f), new Vector3(0.12f, 0.12f, 0.02f), PatchworkAssets.Felt(new Color(0.95f, 0.93f, 0.88f)));
            }
            eyes[which] = new Transform[2];
            for (int s = 0; s < 2; s++)
                eyes[which][s] = Part(PrimitiveType.Sphere, tl, new Vector3((s == 0 ? -1f : 1f) * 0.16f, -0.75f, 0.155f), new Vector3(0.06f, 0.06f, 0.02f), black);
            // The admiral's hat.
            Part(PrimitiveType.Sphere, tl, new Vector3(0f, -0.46f, 0.13f), new Vector3(0.95f, 0.24f, 0.06f), navy);
            Part(PrimitiveType.Sphere, tl, new Vector3(0f, -0.36f, 0.13f), new Vector3(0.5f, 0.3f, 0.06f), navy);
            Part(PrimitiveType.Sphere, tl, new Vector3(0f, -0.4f, 0.16f), new Vector3(0.12f, 0.12f, 0.03f), gold);
            // A little brass plate on the frame.
            Part(PrimitiveType.Cube, tl, new Vector3(0f, -1.84f, 0.09f), new Vector3(0.5f, 0.1f, 0.02f), gold);
            return g;
        }

        Transform BuildDuck(Vector3 at, bool hat)
        {
            var d = new GameObject("GalleryDuck").transform;
            d.SetParent(transform, false);
            d.position = at;
            var yellow = PatchworkAssets.Plain(new Color(1f, 0.84f, 0.2f), 0.6f);
            var orange = PatchworkAssets.Plain(new Color(0.95f, 0.5f, 0.15f), 0.5f);
            var black = PatchworkAssets.Plain(new Color(0.05f, 0.04f, 0.06f), 0.8f);
            Part(PrimitiveType.Sphere, d, new Vector3(0f, 0.17f, -0.03f), new Vector3(0.36f, 0.3f, 0.46f), yellow);
            Part(PrimitiveType.Sphere, d, new Vector3(0f, 0.22f, -0.26f), new Vector3(0.14f, 0.14f, 0.16f), yellow, new Vector3(-30f, 0f, 0f));
            Part(PrimitiveType.Sphere, d, new Vector3(0f, 0.42f, 0.1f), Vector3.one * 0.24f, yellow);
            Part(PrimitiveType.Sphere, d, new Vector3(0f, 0.4f, 0.23f), new Vector3(0.13f, 0.05f, 0.14f), orange);
            for (int s = -1; s <= 1; s += 2) Part(PrimitiveType.Sphere, d, new Vector3(s * 0.07f, 0.46f, 0.19f), Vector3.one * 0.04f, black);
            // (A party hat, for later.)
            var h = new GameObject("Hat").transform;
            h.SetParent(d, false);
            h.localPosition = new Vector3(0f, 0.53f, 0.08f);
            Color[] stripes = { new Color(0.95f, 0.35f, 0.45f), new Color(0.95f, 0.93f, 0.88f), new Color(0.35f, 0.65f, 0.95f) };
            for (int i = 0; i < 4; i++)
                Part(PrimitiveType.Cylinder, h, new Vector3(0f, 0.03f + i * 0.055f, 0f), new Vector3(0.16f - i * 0.04f, 0.028f, 0.16f - i * 0.04f), PatchworkAssets.Felt(stripes[i % 3]));
            Part(PrimitiveType.Sphere, h, new Vector3(0f, 0.26f, 0f), Vector3.one * 0.06f, PatchworkAssets.Glow(new Color(1f, 0.85f, 0.4f), 1.2f));
            h.gameObject.SetActive(hat);
            ducks.Add(d);
            duckHats.Add(h);
            return d;
        }

        // The somebody at the end of the hall: tall, draped, two pale eyes. Under it, a coat stand.
        void BuildFigure(Vector3 at)
        {
            figure = new GameObject("TheSomebody").transform;
            figure.SetParent(transform, false);
            figure.position = at;
            shroud = new GameObject("Shroud").transform;
            shroud.SetParent(figure, false);
            var dark = PatchworkAssets.Plain(new Color(0.03f, 0.03f, 0.05f), 0.1f);
            Part(PrimitiveType.Capsule, shroud, new Vector3(0f, 1.35f, 0f), new Vector3(0.95f, 1.35f, 0.8f), dark);
            Part(PrimitiveType.Sphere, shroud, new Vector3(0f, 2.55f, 0f), new Vector3(0.7f, 0.75f, 0.7f), dark);
            Part(PrimitiveType.Cylinder, shroud, new Vector3(0f, 3.05f, 0f), new Vector3(0.3f, 0.2f, 0.3f), dark);
            Part(PrimitiveType.Capsule, shroud, new Vector3(0.6f, 1.5f, 0.1f), new Vector3(0.24f, 0.6f, 0.24f), dark, new Vector3(0f, 0f, 14f));
            for (int s = -1; s <= 1; s += 2)
                Part(PrimitiveType.Sphere, shroud, new Vector3(s * 0.14f, 2.6f, 0.33f), new Vector3(0.1f, 0.13f, 0.04f), PatchworkAssets.Glow(new Color(0.9f, 0.95f, 1f), 2.2f));
        }

        void BuildParty(Vector3 at)
        {
            if (figure == null)
            {
                figure = new GameObject("TheSomebody").transform;
                figure.SetParent(transform, false);
                figure.position = at;
            }
            party = new GameObject("PartyCoatStand").transform;
            party.SetParent(figure, false);
            var wood = PatchworkAssets.Wood(new Color(0.4f, 0.27f, 0.18f));
            var sheet = PatchworkAssets.Fabric(new Color(0.95f, 0.93f, 0.88f));
            // The coat stand: a pole on three feet with pegs at the top.
            Part(PrimitiveType.Cylinder, party, new Vector3(0f, 1.25f, 0f), new Vector3(0.09f, 1.25f, 0.09f), wood);
            for (int i = 0; i < 3; i++)
            {
                float a = i * Mathf.PI * 2f / 3f;
                Part(PrimitiveType.Capsule, party, new Vector3(Mathf.Cos(a) * 0.24f, 0.1f, Mathf.Sin(a) * 0.24f), new Vector3(0.08f, 0.28f, 0.08f), wood, new Vector3(Mathf.Sin(a) * 70f, 0f, -Mathf.Cos(a) * 70f));
            }
            // The dust sheet over it, pushed back like a hood, with two eyeholes cut in it.
            Part(PrimitiveType.Capsule, party, new Vector3(0f, 1.3f, -0.08f), new Vector3(0.9f, 1.2f, 0.7f), sheet);
            Part(PrimitiveType.Sphere, party, new Vector3(0f, 2.45f, -0.05f), new Vector3(0.66f, 0.66f, 0.62f), sheet);
            for (int s = -1; s <= 1; s += 2)
                Part(PrimitiveType.Sphere, party, new Vector3(s * 0.14f, 2.5f, 0.27f), new Vector3(0.12f, 0.15f, 0.05f), PatchworkAssets.Plain(new Color(0.08f, 0.07f, 0.1f), 0.5f));
            Part(PrimitiveType.Sphere, party, new Vector3(0f, 2.34f, 0.29f), new Vector3(0.24f, 0.1f, 0.04f), PatchworkAssets.Plain(new Color(0.08f, 0.07f, 0.1f), 0.5f));
            // The party hat.
            Color[] stripes = { new Color(0.95f, 0.35f, 0.45f), new Color(0.95f, 0.93f, 0.88f), new Color(0.35f, 0.65f, 0.95f), new Color(1f, 0.8f, 0.3f) };
            for (int i = 0; i < 6; i++)
                Part(PrimitiveType.Cylinder, party, new Vector3(0f, 2.82f + i * 0.09f, -0.05f), new Vector3(0.42f - i * 0.065f, 0.045f, 0.42f - i * 0.065f), PatchworkAssets.Felt(stripes[i % 4]));
            Part(PrimitiveType.Sphere, party, new Vector3(0f, 3.4f, -0.05f), Vector3.one * 0.13f, PatchworkAssets.Glow(new Color(1f, 0.85f, 0.4f), 1.4f));
            // One wooden arm, a plate, and a cake with a candle on it.
            Part(PrimitiveType.Capsule, party, new Vector3(0.36f, 1.45f, 0.38f), new Vector3(0.09f, 0.34f, 0.09f), wood, new Vector3(70f, 30f, 0f));
            Part(PrimitiveType.Cylinder, party, new Vector3(0.52f, 1.52f, 0.66f), new Vector3(0.52f, 0.015f, 0.52f), PatchworkAssets.Plain(new Color(0.95f, 0.95f, 0.98f), 0.8f));
            Part(PrimitiveType.Cylinder, party, new Vector3(0.52f, 1.63f, 0.66f), new Vector3(0.4f, 0.09f, 0.4f), PatchworkAssets.Felt(new Color(0.95f, 0.75f, 0.8f)));
            Part(PrimitiveType.Cylinder, party, new Vector3(0.52f, 1.73f, 0.66f), new Vector3(0.42f, 0.02f, 0.42f), PatchworkAssets.Felt(new Color(0.98f, 0.96f, 0.9f)));
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3f;
                Part(PrimitiveType.Sphere, party, new Vector3(0.52f + Mathf.Cos(a) * 0.16f, 1.77f, 0.66f + Mathf.Sin(a) * 0.16f), Vector3.one * 0.06f, PatchworkAssets.Felt(new Color(0.85f, 0.2f, 0.25f)));
            }
            Part(PrimitiveType.Cylinder, party, new Vector3(0.52f, 1.84f, 0.66f), new Vector3(0.03f, 0.09f, 0.03f), PatchworkAssets.Felt(new Color(0.4f, 0.7f, 0.95f)));
            Part(PrimitiveType.Sphere, party, new Vector3(0.52f, 1.97f, 0.66f), new Vector3(0.05f, 0.09f, 0.05f), PatchworkAssets.Glow(new Color(1f, 0.8f, 0.35f), 3f));
        }

        // Patch has put the picture straight (the "Straighten the picture" spot).
        public void Straighten()
        {
            if (state != State.Idle || straight) return;
            straight = true;
            straightened++;
            Sfx.Play(SfxId.Lever, 0.35f);
            if (straightened == 2) Narrator.Say("It had gone crooked again the moment Patch's back was turned. Pictures do that. Don't they?");
            if (straightened >= 3)
            {
                state = State.Starting;
                timer = 0f;
            }
        }

        void Begin(PatchworkGame game)
        {
            state = State.Looping;
            wraps = 0;
            stage = 0;
            timer = 0f;
            if (Spot != null) Spot.gameObject.SetActive(false);
            if (hoot != null) hoot.gameObject.SetActive(false);
            // Every lamp in the gallery goes out.
            dimmed.Clear();
            dimmedWas.Clear();
            var box = new Bounds(new Vector3(MidX, Y + 3.5f, -25.5f), new Vector3(9f, 8f, 34f));
            foreach (var l in FindObjectsByType<Light>(FindObjectsInactive.Exclude))
            {
                if (l == null || l.type == LightType.Directional || !box.Contains(l.transform.position)) continue;
                if (System.Array.IndexOf(own, l) >= 0) continue;
                dimmed.Add(l);
                dimmedWas.Add(l.intensity);
                l.intensity = 0f;
            }
            foreach (var l in own) l.enabled = true;
            foreach (var f in frames) f.gameObject.SetActive(true);
            game.Blackout(1f);
            Sfx.Play(SfxId.PlatformThunk, 0.5f);
            Narrator.Say("The lamps went out, all but three. The music stopped as if somebody had put a hand over it. And the gallery was a good deal longer than it had been.");
        }

        void End()
        {
            for (int i = 0; i < dimmed.Count; i++) if (dimmed[i] != null) dimmed[i].intensity = dimmedWas[i];
            dimmed.Clear();
            dimmedWas.Clear();
            foreach (var l in own) l.enabled = false;
            frames[0].gameObject.SetActive(false);
            frames[2].gameObject.SetActive(false);
            if (hoot != null) hoot.gameObject.SetActive(true);
            MusicPlayer.LiftHush = false;
        }

        void Shift(PatchworkGame game, float dz)
        {
            Vector3 d = new Vector3(0f, 0f, dz);
            var motor = game.Player.Motor;
            motor.Controller.enabled = false;
            motor.transform.position += d;
            motor.Controller.enabled = true;
            if (game.CameraRig != null) game.CameraRig.Shift(d);
            if (figure != null) figure.position += d;
            wraps++;
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) return;
            float dt = Time.deltaTime;
            t += dt;
            Vector3 pp = game.Player.transform.position;

            // The picture: crooked, or straight, or (later) the wrong way up.
            float wantTilt = state == State.Looping && stage >= 2 ? 180f : straight ? 0f : 9f;
            tilt = Mathf.MoveTowards(tilt, wantTilt, dt * (wantTilt > 90f || tilt > 90f ? 240f : 40f));
            for (int i = 0; i < 3; i++)
            {
                tilts[i].localRotation = Quaternion.Euler(0f, 0f, tilt + (straight ? 0f : Mathf.Sin(t * 0.7f) * 0.4f));
                // His eyes: straight ahead, until the gallery goes wrong. Then they follow.
                float look = 0f;
                if ((state == State.Looping && stage >= 1) || state == State.Reveal)
                    look = Mathf.Clamp((pp.z - frames[i].position.z) * -0.012f, -0.03f, 0.03f);
                for (int s = 0; s < 2; s++)
                {
                    Vector3 e = eyes[i][s].localPosition;
                    e.x = Mathf.Lerp(e.x, (s == 0 ? -1f : 1f) * 0.16f + look, 1f - Mathf.Exp(-6f * dt));
                    eyes[i][s].localPosition = e;
                }
            }

            switch (state)
            {
                case State.Idle:
                    // It slips again once Patch has walked off (the first two times).
                    if (straight && straightened < 3 && Mathf.Abs(pp.z - PortraitZ) > 7f)
                    {
                        straight = false;
                        Sfx.PlayNear(SfxId.Lever, frames[1].position, 0.3f, 30f);
                    }
                    break;

                case State.Starting:
                    timer += dt;
                    if (timer > 1.3f && game.Mode == UIMode.Gameplay) Begin(game);
                    break;

                case State.Looping:
                    MusicPlayer.LiftHush = true;
                    if (game.Mode != UIMode.Gameplay) break;
                    // (If he's somehow got out of the gallery, let him go.)
                    if (pp.y < Y - 3f || pp.x < 31f || pp.x > 41f || pp.z < -43f || pp.z > -8f) { End(); state = State.Idle; straight = false; straightened = 0; if (Spot != null) Spot.gameObject.SetActive(true); break; }
                    if (pp.z > LoopHi) Shift(game, -Period);
                    else if (pp.z < LoopLo) Shift(game, Period);
                    Stages(game);
                    Noises(dt, pp);
                    for (int i = 0; i < own.Length; i++)
                    {
                        own[i].color = stage >= 4 ? new Color(1f, 0.35f, 0.3f) : new Color(0.6f, 0.65f, 1f);
                        own[i].intensity = (stage >= 4 ? 0.5f : 0.7f) * (0.85f + Mathf.Sin(t * 13f + i * 2f) * 0.08f + (Mathf.Sin(t * 1.7f + i) > 0.96f ? -0.6f : 0f));
                    }
                    // The ducks: at first they just sit. Later they all look at you.
                    if (stage >= 4)
                        foreach (var d in ducks)
                        {
                            Vector3 to = pp - d.position;
                            to.y = 0f;
                            if (to.sqrMagnitude > 0.01f) d.rotation = Quaternion.Slerp(d.rotation, Quaternion.LookRotation(to.normalized, Vector3.up), 1f - Mathf.Exp(-3f * dt));
                        }
                    // And the somebody.
                    if (figure != null && shroud != null)
                    {
                        float away = figure.position.z - pp.z;
                        if (Mathf.Abs(away) > 12.5f)
                        {
                            // Always ahead of you, whichever way you go.
                            float ahead = game.Player.transform.forward.z >= 0f ? 1f : -1f;
                            figure.position = new Vector3(MidX + 0.5f, Y, pp.z + ahead * 9.5f);
                        }
                        Vector3 face = pp - figure.position;
                        face.y = 0f;
                        if (face.sqrMagnitude > 0.01f) figure.rotation = Quaternion.LookRotation(face.normalized, Vector3.up);
                        shroud.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.8f) * 2f);
                        if (face.magnitude < 3.3f) { state = State.Reveal; timer = 0f; Sfx.Play(SfxId.PlatformThunk, 0.6f); }
                    }
                    break;

                case State.Reveal:
                    MusicPlayer.LiftHush = true;
                    timer += dt;
                    if (timer < 1.25f) game.Blackout(1f);
                    if (timer >= 1f && party == null)
                    {
                        // In the dark: off comes the shroud.
                        shroud.gameObject.SetActive(false);
                        BuildParty(figure.position);
                        foreach (var h in duckHats) h.gameObject.SetActive(true);
                        End();
                    }
                    if (timer >= 1.3f)
                    {
                        state = State.Done;
                        MusicPlayer.LiftHush = false;
                        Vector3 c = figure.position + Vector3.up * 2f;
                        FxBurst.Spawn(FxBurst.Kind.Pompoms, c, new Color(0.95f, 0.35f, 0.45f));
                        FxBurst.Spawn(FxBurst.Kind.Pompoms, c + Vector3.up * 0.6f, new Color(1f, 0.8f, 0.3f));
                        FxBurst.Spawn(FxBurst.Kind.Pompoms, c + Vector3.right * 0.5f, new Color(0.35f, 0.65f, 0.95f));
                        FxBurst.Spawn(FxBurst.Kind.Sparkle, c, Color.white);
                        Sfx.Play(SfxId.PrizeFanfare, 0.7f);
                        Sfx.PlayPitched(SfxId.Squeak, 0.6f, 0.7f);
                        Narrator.Say("It was a coat stand. It had been a coat stand the whole time. Somebody had put a dust sheet over it, and a party hat on top, and in its one wooden hand it was holding a cake. The ducks, it turned out, had come for the cake.");
                        game.Heal(PatchworkGame.MaxHearts);
                        if (!GameState.IsHatUnlocked(5))
                        {
                            GameState.UnlockHat(5);
                            game.ShowToast("The coat stand gave you its Party Hat! (Hats are in your Satchel.)");
                        }
                        else ScoreKeeper.Bonus(5000, game.Player.Motor.Center);
                        if (Level2Quest.Instance != null) Level2Quest.Instance.Secret("hall");
                    }
                    break;
            }
        }

        // What changes, the further Patch walks.
        void Stages(PatchworkGame game)
        {
            int want = wraps >= 15 ? 5 : wraps >= 12 ? 4 : wraps >= 9 ? 3 : wraps >= 6 ? 2 : wraps >= 3 ? 1 : 0;
            if (want <= stage) return;
            stage = want;
            Vector3 pp = game.Player.transform.position;
            switch (stage)
            {
                case 1:
                    Narrator.Say("Professor Hoot was not at his perch. Nobody was anywhere. Only the Founder, in his frame, and he seemed to be looking.");
                    break;
                case 2:
                    Sfx.Play(SfxId.Clack, 0.5f);
                    Narrator.Say("Every picture in the gallery was the same picture now. And every one of them was the wrong way up.");
                    break;
                case 3:
                    // Ducks, one every two yards, in a pattern that repeats with the gallery.
                    for (int k = 0; k < 9; k++)
                        BuildDuck(new Vector3(35.1f + (k % 3) * 0.9f, Y, -34f + k * 2f), false).rotation = Quaternion.Euler(0f, k * 77f, 0f);
                    Sfx.PlayPitched(SfxId.Squeak, 0.35f, 0.6f);
                    Narrator.Say("There had not been ducks before. Patch was almost sure there had not been ducks.");
                    break;
                case 4:
                    game.Blackout(0.7f);
                    Narrator.Say("The ducks were all facing the same way now. It was the way Patch was.");
                    break;
                case 5:
                    float ahead = game.Player.transform.forward.z >= 0f ? 1f : -1f;
                    BuildFigure(new Vector3(MidX + 0.5f, Y, pp.z + ahead * 9.5f));
                    Narrator.Say("And at the end of the hall, where the end of the hall ought to have been, somebody very tall stood waiting.");
                    break;
            }
        }

        // Small sounds, from further down the hall than there is any hall.
        void Noises(float dt, Vector3 pp)
        {
            nextNoise -= dt;
            if (nextNoise > 0f) return;
            nextNoise = Random.Range(3.5f, 7f);
            Vector3 from = pp + new Vector3(Random.Range(-1.5f, 1.5f), 1f, (Random.value < 0.5f ? -1f : 1f) * Random.Range(7f, 11f));
            int pick = Random.Range(0, stage >= 3 ? 4 : 3);
            if (pick == 0) Sfx.PlayNear(SfxId.Clack, from, 0.35f, 30f);
            else if (pick == 1) Sfx.PlayNear(SfxId.Lever, from, 0.3f, 30f);
            else if (pick == 2) Sfx.PlayNear(SfxId.PadClick, from, 0.3f, 30f);
            else Sfx.PlayNear(SfxId.Squeak, from, 0.25f, 30f);
        }

        // The dark, and the fog that hides how short the gallery really is. (After the museum's
        // own moods have set theirs for the frame.)
        void LateUpdate()
        {
            if (state != State.Looping && state != State.Reveal) return;
            if (state == State.Reveal && party != null) return;
            bool red = stage >= 4;
            RenderSettings.fogColor = red ? new Color(0.05f, 0.012f, 0.012f) : new Color(0.012f, 0.012f, 0.03f);
            RenderSettings.fogStartDistance = 1f;
            RenderSettings.fogEndDistance = red ? 7f : 8.5f;
            RenderSettings.ambientSkyColor = new Color(0.04f, 0.04f, 0.08f);
            RenderSettings.ambientEquatorColor = red ? new Color(0.08f, 0.04f, 0.05f) : new Color(0.05f, 0.05f, 0.09f);
            RenderSettings.ambientGroundColor = new Color(0.02f, 0.02f, 0.04f);
        }
    }

    // ====================================================================== The Dust Sheet Five

    // Keeps count of the dust bunnies: which have been caught (ever), and what happens at five.
    public class DustyHunt : MonoBehaviour
    {
        public Vector3[] Spots;
        public float[] Yaws;
        public const int Count = 5;

        static string Key(int i) { return "L2_q_dusty" + i; }
        public int Caught { get { int n = 0; for (int i = 0; i < Count; i++) if (GameState.IsHintSeen(Key(i))) n++; return n; } }

        void Start()
        {
            for (int i = 0; i < Spots.Length && i < Count; i++)
            {
                Vector3 at = Spots[i];
                at.y = Monsters.Ground(at + Vector3.up * 0.4f);
                DustSheet.Create(transform, at, Yaws[i], i, this, !GameState.IsHintSeen(Key(i)));
            }
        }

        public void Got(int which)
        {
            GameState.SetHintSeen(Key(which));
            int n = Caught;
            var game = PatchworkGame.Instance;
            if (game == null) return;
            if (n >= Count)
            {
                game.Heal(PatchworkGame.MaxHearts);
                Narrator.Say("Five dust bunnies, in one satchel, all blowing raspberries at once. It sounded like a very small motorcycle.");
                if (Level2Quest.Instance != null) Level2Quest.Instance.Secret("dusties");
            }
            else game.ShowToast("Hoovered up a dust bunny!  (" + n + " of " + Count + ")");
        }
    }

    // A piece of furniture under a dust sheet. Hit it and it wobbles (and, if there's somebody
    // under there, giggles). Only wind gets the sheet off.
    public class DustSheet : MonoBehaviour
    {
        DustyHunt hunt;
        int index;
        Transform sheet;
        Dusty dusty;
        bool bare;
        float wobble, t, off, twitch = 4f;

        public Vector3 Top { get { return transform.position + Vector3.up * 1.5f; } }

        public static DustSheet Create(Transform parent, Vector3 ground, float yaw, int index, DustyHunt hunt, bool occupied)
        {
            var go = new GameObject("DustSheet");
            go.transform.SetParent(parent, false);
            go.transform.position = ground;
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            var d = go.AddComponent<DustSheet>();
            d.hunt = hunt;
            d.index = index;
            d.t = index * 1.7f;

            // What's under it: a different old thing each time.
            var wood = PatchworkAssets.Wood(new Color(0.42f, 0.28f, 0.18f));
            var brass = PatchworkAssets.Plain(new Color(0.85f, 0.65f, 0.3f), 0.8f);
            var under = new GameObject("Furniture").transform;
            under.SetParent(go.transform, false);
            switch (index % 3)
            {
                case 0:   // an armchair
                    var plush = PatchworkAssets.Damask(new Color(0.55f, 0.2f, 0.25f));
                    Part(under, PrimitiveType.Cube, new Vector3(0f, 0.4f, 0f), new Vector3(1.1f, 0.5f, 0.9f), plush);
                    Part(under, PrimitiveType.Cube, new Vector3(0f, 0.95f, -0.38f), new Vector3(1.1f, 0.9f, 0.2f), plush);
                    for (int s = -1; s <= 1; s += 2) Part(under, PrimitiveType.Capsule, new Vector3(s * 0.52f, 0.72f, 0f), new Vector3(0.2f, 0.42f, 0.2f), plush, new Vector3(90f, 0f, 0f));
                    break;
                case 1:   // a chest of drawers
                    Part(under, PrimitiveType.Cube, new Vector3(0f, 0.6f, 0f), new Vector3(1.2f, 1.2f, 0.7f), wood);
                    for (int r = 0; r < 3; r++)
                        for (int s = -1; s <= 1; s += 2) Part(under, PrimitiveType.Sphere, new Vector3(s * 0.25f, 0.3f + r * 0.32f, 0.36f), Vector3.one * 0.08f, brass);
                    break;
                default:  // a rocking horse
                    var dapple = PatchworkAssets.Felt(new Color(0.85f, 0.83f, 0.8f));
                    Part(under, PrimitiveType.Capsule, new Vector3(0f, 0.75f, 0f), new Vector3(0.45f, 0.5f, 0.45f), dapple, new Vector3(90f, 0f, 0f));
                    Part(under, PrimitiveType.Capsule, new Vector3(0f, 1.1f, 0.42f), new Vector3(0.24f, 0.32f, 0.24f), dapple, new Vector3(35f, 0f, 0f));
                    Part(under, PrimitiveType.Sphere, new Vector3(0f, 1.34f, 0.62f), new Vector3(0.24f, 0.26f, 0.4f), dapple);
                    for (int s = -1; s <= 1; s += 2) Part(under, PrimitiveType.Cube, new Vector3(s * 0.26f, 0.08f, 0f), new Vector3(0.08f, 0.1f, 1.5f), wood);
                    for (int k = 0; k < 4; k++) Part(under, PrimitiveType.Cylinder, new Vector3((k % 2 == 0 ? -0.2f : 0.2f), 0.36f, k < 2 ? -0.3f : 0.3f), new Vector3(0.08f, 0.3f, 0.08f), dapple);
                    break;
            }

            // The sheet: a lumpy white shape with folds down the sides.
            d.sheet = new GameObject("Sheet").transform;
            d.sheet.SetParent(go.transform, false);
            var cloth = PatchworkAssets.Fabric(new Color(0.93f, 0.91f, 0.86f));
            var shade = PatchworkAssets.Fabric(new Color(0.82f, 0.8f, 0.76f));
            Part(d.sheet, PrimitiveType.Cube, new Vector3(0f, 0.7f, 0f), new Vector3(1.5f, 1.4f, 1.2f), cloth);
            Part(d.sheet, PrimitiveType.Sphere, new Vector3(0f, 1.4f, 0f), new Vector3(1.6f, 0.7f, 1.3f), cloth);
            for (int k = 0; k < 10; k++)
            {
                float a = k * Mathf.PI / 5f;
                Part(d.sheet, PrimitiveType.Capsule, new Vector3(Mathf.Cos(a) * 0.78f, 0.6f, Mathf.Sin(a) * 0.63f), new Vector3(0.22f, 0.62f, 0.22f), k % 2 == 0 ? shade : cloth, new Vector3(Mathf.Sin(a) * 6f, 0f, -Mathf.Cos(a) * 6f));
            }
            Part(d.sheet, PrimitiveType.Cube, new Vector3(0.5f, 0.5f, 0.615f), new Vector3(0.24f, 0.14f, 0.01f), PatchworkAssets.Felt(new Color(0.9f, 0.8f, 0.55f)));

            // (So that strikes and gusts can find it.)
            var fc = go.AddComponent<FrayCreature>();
            fc.Radius = 1f;
            fc.Quiet = true;
            fc.Center = () => go.transform.position + Vector3.up * 0.9f;
            fc.Struck = (p, power, from, finisher) => d.Hit(p);

            if (occupied) d.dusty = Dusty.Create(parent, d, index, hunt);
            return d;
        }

        static void Part(Transform parent, PrimitiveType type, Vector3 p, Vector3 s, Material m, Vector3 e = default(Vector3))
        {
            Monsters.Part(type, parent, p, s, m, e).gameObject.layer = 0;
        }

        bool Hit(PatchworkPlayer player)
        {
            if (bare) return false;
            wobble = 1f;
            bool windy = player != null && player.Forms.Current == FormType.Wind;
            bool somebody = dusty != null && dusty.Hiding;
            if (somebody) Sfx.PlayPitched(SfxId.Squeak, 0.4f, 1.5f);
            if (!windy)
            {
                var game = PatchworkGame.Instance;
                if (game != null) game.ShowToast(somebody ? "Something giggled under the sheet. It would take a good puff of wind to shift it." : "A dust sheet. Only a good puff of wind would shift it.");
                return true;
            }
            // Whoosh: off it comes.
            bare = true;
            off = 0f;
            FxBurst.Spawn(FxBurst.Kind.Puff, Top, new Color(0.9f, 0.88f, 0.82f), Vector3.up);
            Sfx.Play(SfxId.Whoosh, 0.4f);
            if (somebody) dusty.PopOut();
            return true;
        }

        // Its dust bunny has got back under (or been caught): the sheet settles again.
        public void Cover()
        {
            bare = false;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            t += dt;
            wobble = Mathf.MoveTowards(wobble, 0f, dt * 2f);
            // The sheet flies up and away, and floats back down when it's wanted again.
            off = Mathf.MoveTowards(off, bare ? 1f : 0f, dt * (bare ? 3f : 1.2f));
            // (An empty one comes back by itself after a bit.)
            if (bare && (dusty == null || dusty.Gone)) { twitch -= dt; if (twitch < -6f) { twitch = 4f; bare = false; } }
            float k = Mathf.SmoothStep(0f, 1f, off);
            sheet.localPosition = Vector3.up * k * 2.6f;
            sheet.localScale = new Vector3(1f + Mathf.Sin(wobble * 18f) * 0.06f * wobble, 1f - Mathf.Sin(wobble * 18f) * 0.06f * wobble, 1f) * (1f - k);
            sheet.localRotation = Quaternion.Euler(0f, k * 200f, Mathf.Sin(wobble * 15f) * 5f * wobble);

            // Somebody under there can't keep still.
            if (!bare && dusty != null && dusty.Hiding)
            {
                twitch -= dt;
                var game = PatchworkGame.Instance;
                if (twitch <= 0f && game != null && game.Player != null && (game.Player.transform.position - transform.position).sqrMagnitude < 64f)
                {
                    twitch = Random.Range(2.5f, 5f);
                    wobble = 0.6f;
                    Sfx.PlayNear(SfxId.Squeak, Top, 0.2f, 14f);
                }
            }
        }
    }

    // A dust bunny: the museum sort, which grow rather large. Round, grey, fluffy, with a
    // handkerchief on its head, and no manners. Out in the open it runs from Patch, blowing
    // raspberries, and dives back under its sheet if it's left alone. A Wind strike hoovers it up.
    public class Dusty : MonoBehaviour
    {
        enum State { Hide, Out, Sucked, Caught }
        State state;
        DustSheet home;
        DustyHunt hunt;
        int index;
        Transform body;
        Transform[] fluff;
        Vector3 pos, vel;
        float t, outFor, puff;
        PatchworkPlayer catcher;

        public bool Hiding { get { return state == State.Hide; } }
        public bool Gone { get { return state == State.Caught; } }

        public static Dusty Create(Transform parent, DustSheet home, int index, DustyHunt hunt)
        {
            var go = new GameObject("DustBunny");
            go.transform.SetParent(parent, false);
            var d = go.AddComponent<Dusty>();
            d.home = home;
            d.hunt = hunt;
            d.index = index;
            d.t = index * 2.3f;
            d.body = new GameObject("Body").transform;
            d.body.SetParent(go.transform, false);
            var grey = PatchworkAssets.Knit(new Color(0.72f, 0.7f, 0.76f));
            var pale = PatchworkAssets.Knit(new Color(0.84f, 0.82f, 0.86f));
            var black = PatchworkAssets.Plain(new Color(0.06f, 0.05f, 0.08f), 0.8f);
            Monsters.Part(PrimitiveType.Sphere, d.body, Vector3.zero, Vector3.one * 0.8f, grey);
            d.fluff = new Transform[9];
            for (int i = 0; i < 9; i++)
            {
                Vector3 dir = (Quaternion.Euler(i * 67f, i * 131f, 0f) * Vector3.forward).normalized;
                if (dir.z > 0.6f) dir.z = -dir.z;   // (keep its face clear)
                d.fluff[i] = Monsters.Part(PrimitiveType.Sphere, d.body, dir * 0.36f, Vector3.one * (0.3f + (i % 3) * 0.07f), i % 2 == 0 ? pale : grey);
            }
            for (int s = -1; s <= 1; s += 2)
            {
                Monsters.Part(PrimitiveType.Sphere, d.body, new Vector3(s * 0.15f, 0.08f, 0.34f), new Vector3(0.2f, 0.24f, 0.1f), PatchworkAssets.Felt(new Color(0.98f, 0.97f, 0.95f)));
                Monsters.Part(PrimitiveType.Sphere, d.body, new Vector3(s * 0.14f, 0.06f, 0.385f), new Vector3(0.09f, 0.11f, 0.05f), black);
                Monsters.Part(PrimitiveType.Capsule, d.body, new Vector3(s * 0.4f, -0.12f, 0.1f), new Vector3(0.1f, 0.12f, 0.1f), grey, new Vector3(0f, 0f, s * 50f));
            }
            // The raspberry.
            Monsters.Part(PrimitiveType.Sphere, d.body, new Vector3(0f, -0.12f, 0.38f), new Vector3(0.1f, 0.06f, 0.12f), PatchworkAssets.Felt(new Color(0.95f, 0.5f, 0.6f)));
            // A handkerchief, knotted at the corners, worn as a hat.
            var hanky = PatchworkAssets.Fabric(new Color(0.95f, 0.93f, 0.88f));
            Monsters.Part(PrimitiveType.Sphere, d.body, new Vector3(0f, 0.36f, -0.02f), new Vector3(0.6f, 0.22f, 0.6f), hanky);
            for (int k = 0; k < 4; k++)
            {
                float a = k * Mathf.PI / 2f + 0.78f;
                Monsters.Part(PrimitiveType.Sphere, d.body, new Vector3(Mathf.Cos(a) * 0.3f, 0.36f, Mathf.Sin(a) * 0.3f - 0.02f), Vector3.one * 0.1f, hanky);
            }

            var fc = go.AddComponent<FrayCreature>();
            fc.Radius = 0.6f;
            fc.Quiet = true;
            fc.Center = () => d.pos;
            fc.Struck = (p, power, from, finisher) => d.Hit(p);
            d.body.gameObject.SetActive(false);
            fc.enabled = false;
            return d;
        }

        public void PopOut()
        {
            state = State.Out;
            outFor = 0f;
            pos = home.Top + Vector3.up * 0.3f;
            vel = new Vector3(Mathf.Sin(t * 7f), 0f, Mathf.Cos(t * 7f)) * 4f + Vector3.up * 3f;
            body.gameObject.SetActive(true);
            GetComponent<FrayCreature>().enabled = true;
            Sfx.PlayPitched(SfxId.Squeak, 0.5f, 1.2f);
            if (!GameState.IsHintSeen("L2_q_hint_dusty"))
            {
                GameState.SetHintSeen("L2_q_hint_dusty");
                Narrator.Say("Out from under the sheet shot something round and grey and fluffy, blowing a raspberry. A dust bunny: the museum sort, which grow rather large. Only a good draught would catch it.");
            }
        }

        bool Hit(PatchworkPlayer player)
        {
            if (state != State.Out) return false;
            if (player == null || player.Forms.Current != FormType.Wind)
            {
                var game = PatchworkGame.Instance;
                if (game != null) game.ShowToast("Your hand goes right through the fluff. It wants hoovering: try Wind.");
                return false;
            }
            state = State.Sucked;
            outFor = 0f;
            catcher = player;
            Sfx.Play(SfxId.Whoosh, 0.5f);
            Sfx.PlayPitched(SfxId.Squeak, 0.5f, 1.8f);
            return true;
        }

        void Hide()
        {
            state = State.Hide;
            body.gameObject.SetActive(false);
            GetComponent<FrayCreature>().enabled = false;
            home.Cover();
        }

        void Update()
        {
            if (state == State.Hide || state == State.Caught) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            t += dt;
            outFor += dt;
            var game = PatchworkGame.Instance;
            var player = game != null ? game.Player : null;

            if (state == State.Sucked)
            {
                // Round and round and in.
                float k = Mathf.Clamp01(outFor / 0.55f);
                Vector3 to = catcher != null ? catcher.Motor.Center : pos;
                pos = Vector3.Lerp(pos, to + Quaternion.Euler(0f, outFor * 900f, 0f) * Vector3.forward * (1f - k) * 0.8f, 1f - Mathf.Exp(-12f * dt));
                transform.position = pos;
                body.localScale = Vector3.one * (1f - k * 0.9f);
                body.localRotation = Quaternion.Euler(0f, outFor * 1400f, 0f);
                if (k >= 1f)
                {
                    state = State.Caught;
                    body.gameObject.SetActive(false);
                    GetComponent<FrayCreature>().enabled = false;
                    FxBurst.Spawn(FxBurst.Kind.Puff, pos, new Color(0.85f, 0.83f, 0.88f));
                    ScoreKeeper.Bonus(400, pos);
                    home.Cover();
                    hunt.Got(index);
                }
                return;
            }

            // Out: keep away from Patch, keep near home, keep off the floor.
            Vector3 homeAt = home.Top;
            Vector3 want = Vector3.zero;
            if (player != null)
            {
                Vector3 from = pos - player.Motor.Center;
                from.y = 0f;
                float d = from.magnitude;
                if (d < 7f && d > 0.01f) want += from / d * (7f - d) * 1.6f;
            }
            Vector3 back = homeAt - pos;
            back.y = 0f;
            if (back.magnitude > 6f) want += back.normalized * (back.magnitude - 6f) * 2f;
            want += new Vector3(Mathf.Sin(t * 1.3f + index), 0f, Mathf.Cos(t * 1.7f + index * 2f)) * 1.5f;
            // After a while it loses its nerve and goes home.
            bool homing = outFor > 9f;
            if (homing) want = back.normalized * 6f;
            vel = Vector3.Lerp(vel, Vector3.ClampMagnitude(new Vector3(want.x, 0f, want.z), 5.2f), 1f - Mathf.Exp(-3f * dt));
            Vector3 step = new Vector3(vel.x, 0f, vel.z) * dt;
            if (!Physics.CheckSphere(pos + step, 0.4f, ~(1 << 2), QueryTriggerInteraction.Ignore)) pos += step;
            else vel = -vel * 0.5f;
            float floor = Monsters.Ground(pos);
            pos.y = Mathf.Lerp(pos.y, floor + 1.25f + Mathf.Sin(t * 3f) * 0.2f, 1f - Mathf.Exp(-4f * dt));
            transform.position = pos;
            if (new Vector2(vel.x, vel.z).sqrMagnitude > 0.05f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(vel.x, 0f, vel.z).normalized, Vector3.up), 1f - Mathf.Exp(-8f * dt));
            float squish = Mathf.Sin(t * 9f) * 0.06f;
            body.localScale = new Vector3(1f + squish, 1f - squish, 1f + squish);
            body.localRotation = Quaternion.Euler(Mathf.Sin(t * 4f) * 8f, 0f, Mathf.Sin(t * 5f) * 10f);
            for (int i = 0; i < fluff.Length; i++) fluff[i].localScale = Vector3.one * (0.3f + (i % 3) * 0.07f) * (1f + Mathf.Sin(t * 6f + i) * 0.1f);
            puff -= dt;
            if (puff <= 0f)
            {
                puff = 0.45f;
                FxBurst.Spawn(FxBurst.Kind.Poof, pos + Vector3.down * 0.3f, new Color(0.85f, 0.83f, 0.88f));
                if (Random.value < 0.3f) Sfx.PlayNear(SfxId.Squeak, pos, 0.2f, 20f);
            }
            if (homing && back.magnitude < 0.9f) Hide();
        }
    }
}

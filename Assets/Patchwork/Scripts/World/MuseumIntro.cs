using UnityEngine;

namespace Patchwork
{
    // The first time you visit the Oceanic Museum: the opening of Chapter Two, told by the
    // museum's own Storyteller (an old owl; see Narrator.Owl).
    //   1. The museum from out at sea, under the button moon. The chapter card.
    //   2. The Observatory, and the Night Glass, the great telescope.
    //   3. The Great Whale going over the dome: she sings when she's seen, and her song keeps
    //      the colour in the place.
    //   4. Tonight, at dusk, the Fray came up through the floor and took the telescope's three
    //      lenses: they pop out of it and fly off.
    //   5. Where each went: the Hall of Arms, the keepers' tanks, and the Atrium (in the claws
    //      of a diving helmet with legs), each with its name.
    //   6. Wick's clock, at eleven: an hour to midnight.
    //   7. Patch, just up the steps from the sea. Find Wick.
    // Hold Confirm or Pause to skip. It plays once (saved), and never once the story has begun.
    public class MuseumIntro : MonoBehaviour
    {
        public const string SeenKey = "L2_intro_seen";
        public Level2Director Director;

        bool active, finished;
        float t = -1f, wait, skipHold;
        int beat = -1;
        Transform[] lenses;
        Vector3[] lensFrom, lensTo;
        bool sang;
        readonly bool[] popped = new bool[3];

        static readonly Vector3 C = new Vector3(0f, 14f, -20f);                // the Observatory
        static readonly Vector3 Tube = C + new Vector3(0f, 5.6f, 0.6f);        // the middle of the telescope
        static readonly Vector3 WickClock = new Vector3(7f, 2.4f, -107.5f);
        const float TDome = 6f, TWhale = 12f, TLenses = 18f, TArms = 24f, TTanks = 28.5f, TCrab = 33f, TClock = 37.5f, TPatch = 42.5f, TEnd = 49f;

        void Start()
        {
            var quest = Level2Quest.Instance;
            bool begun = quest != null && (quest.Met || quest.LensCount > 0);
            if (Director == null || Level2Builder.TouringNow || GameState.IsHintSeen(SeenKey) || GameState.IsLevelComplete(Level2Builder.SceneName) || begun)
            {
                finished = true;
                return;
            }
            active = true;
            // Three lenses, back in the telescope for the start of the story.
            lenses = new Transform[3];
            lensFrom = new Vector3[3];
            lensTo = new Vector3[3];
            var glass = PatchworkAssets.Glow(new Color(0.55f, 0.85f, 1f), 1.8f);
            var rim = PatchworkAssets.Plain(new Color(0.9f, 0.72f, 0.36f), 0.75f);
            var tilt = Quaternion.Euler(-52f, 0f, 0f);
            float[] along = { 6.3f, 1.8f, -2.6f };
            Vector3[] away = { new Vector3(80f, 6f, -10f), new Vector3(-60f, 4f, 112f), new Vector3(0f, 4f, -96f) };
            for (int i = 0; i < 3; i++)
            {
                var lens = new GameObject("IntroLens" + i).transform;
                lens.SetParent(transform, false);
                lensFrom[i] = C + new Vector3(0f, 5.6f, 0.6f) + tilt * new Vector3(0f, 0f, along[i]) + Vector3.left * 1.1f;
                lensTo[i] = away[i];
                lens.position = lensFrom[i];
                lens.rotation = tilt;
                Part(lens, PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.9f, 0.05f, 0.9f), glass, new Vector3(90f, 0f, 0f));
                Part(lens, PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.02f, 0.04f, 1.02f), rim, new Vector3(90f, 0f, 0f));
                lens.gameObject.SetActive(false);
                lenses[i] = lens;
            }
        }

        static void Part(Transform parent, PrimitiveType type, Vector3 p, Vector3 s, Material m, Vector3 e)
        {
            var go = GameObject.CreatePrimitive(type);
            Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = p;
            go.transform.localScale = s;
            go.transform.localRotation = Quaternion.Euler(e);
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = m;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        void Update()
        {
            if (!active || finished) return;
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) return;
            if (t < 0f)
            {
                // Take the camera straight away, behind the loading screen.
                if (game.Mode != UIMode.Gameplay) return;
                t = 0f;
                var arrival = FindAnyObjectByType<PodLaunch>();
                if (arrival != null) arrival.SkipToLanded(game);
                game.StartScriptedCutscene(new Vector3(30f, 22f, -215f), new Vector3(0f, 16f, -108f), 0.9f);
                game.CutsceneSkippable = true;
                game.CameraRig.SnapShowcase();
                RenderOptimizer.SetFarView(true);
            }
            if (game.Mode != UIMode.Cutscene) return;
            if (LoadingScreen.Busy || game.Fade > 0.02f && t <= 0f) { wait = 0f; return; }
            if (wait < 0.6f)
            {
                wait += Time.unscaledDeltaTime;
                Narrator.Card("Chapter Two", "The Night Glass");
                return;
            }

            t += Time.deltaTime;
            bool held = t > 0.6f && (game.Input.Confirm.IsPressed() || game.Input.Pause.IsPressed());
            skipHold = held ? skipHold + Time.unscaledDeltaTime : Mathf.Max(0f, skipHold - Time.unscaledDeltaTime * 2f);
            game.CutsceneSkipFill = skipHold / 0.9f;
            if (skipHold >= 0.9f) { Finish(game); return; }

            int now = t < TDome ? 0 : t < TWhale ? 1 : t < TLenses ? 2 : t < TArms ? 3 : t < TTanks ? 4 : t < TCrab ? 5 : t < TClock ? 6 : t < TPatch ? 7 : t < TEnd ? 8 : 9;
            if (now != beat)
            {
                // A dip to black whenever the story jumps somewhere else.
                if (beat >= 0 && now != 2 && now != 3 && now != 9) game.Blackout(1f);
                beat = now;
            }
            var quest = Level2Quest.Instance;
            switch (now)
            {
                case 0:
                {
                    Narrator.Card("Chapter Two", "The Night Glass");
                    Caption(game, "Out past the end of the pier, where the sea comes right up to the steps, stands the Oceanic Museum.");
                    float k = Ease(t / TDome);
                    game.SetCutsceneCamera(Vector3.Lerp(new Vector3(30f, 22f, -215f), new Vector3(12f, 13f, -178f), k), Vector3.Lerp(new Vector3(0f, 16f, -108f), new Vector3(0f, 12f, -110f), k));
                    break;
                }
                case 1:
                {
                    Narrator.Card(null, null);
                    Narrator.Place("The Observatory");
                    Caption(game, "At the very top of it is the Observatory, and in the Observatory is the Night Glass: the oldest telescope in the world that still does its job.");
                    float k = Ease((t - TDome) / (TWhale - TDome));
                    game.SetCutsceneCamera(Vector3.Lerp(C + new Vector3(-6f, 3f, 21f), C + new Vector3(8f, 5f, 15f), k), Tube);
                    break;
                }
                case 2:
                {
                    Narrator.Place(null);
                    if (!sang && t > TWhale + 0.5f)
                    {
                        sang = true;
                        if (Director.SkyWhaleLife != null) Director.SkyWhaleLife.Sing();
                    }
                    Caption(game, t < TWhale + 3f ? "Every night at midnight, the keepers point it up through the dome, and find the Great Whale as she goes over."
                                                  : "She sings when somebody sees her. And her song is what keeps the colour in this place: in every tank, every tapestry, every rocking horse.");
                    Vector3 whale = Director.SkyWhale != null ? Director.SkyWhale.transform.position : C + Vector3.up * 30f;
                    float k = Ease((t - TWhale) / (TLenses - TWhale));
                    game.SetCutsceneCamera(Vector3.Lerp(C + new Vector3(4f, 6f, -6f), C + new Vector3(0f, 13f, -2f), k), whale);
                    break;
                }
                case 3:
                {
                    float c = t - TLenses;
                    Caption(game, c < 2.6f ? "But tonight, at dusk, the Fray came up through the floor..."
                                           : "...and unscrewed the Night Glass's three lenses, neat as you like, and took them away.");
                    for (int i = 0; i < 3; i++)
                    {
                        float k = Mathf.Clamp01((c - 1.6f - i * 0.7f) / 1.8f);
                        var lens = lenses[i];
                        if (lens == null) continue;
                        bool show = c > 0.2f && k < 1f;
                        if (lens.gameObject.activeSelf != show) lens.gameObject.SetActive(show);
                        if (k > 0f && !popped[i])
                        {
                            popped[i] = true;
                            Sfx.Play(SfxId.Unravel, 0.4f);
                            FxBurst.Spawn(FxBurst.Kind.Sparkle, lensFrom[i], new Color(0.6f, 0.85f, 1f));
                        }
                        // Out of the tube, a little hop, then off and away (it's out of sight by then).
                        Vector3 p = Vector3.Lerp(lensFrom[i], lensFrom[i] + (lensTo[i] - lensFrom[i]).normalized * 30f + Vector3.up * 6f, Ease(k)) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 4f;
                        lens.position = p;
                        lens.Rotate(0f, 0f, Time.deltaTime * 540f, Space.Self);
                    }
                    float kc = Ease(c / (TArms - TLenses));
                    game.SetCutsceneCamera(Vector3.Lerp(C + new Vector3(-7f, 6f, -4f), C + new Vector3(-9f, 9f, -9f), kc), Vector3.Lerp(Tube, Tube + Vector3.up * 5f, kc));
                    break;
                }
                case 4:
                {
                    foreach (var l in lenses) if (l != null && l.gameObject.activeSelf) l.gameObject.SetActive(false);
                    Narrator.Place("The Hall of Arms");
                    Caption(game, "One went to the Hall of Arms, where the old suits of armour have woken up wrong, and are standing guard over it.");
                    float k = Ease((t - TArms) / (TTanks - TArms));
                    game.SetCutsceneCamera(Vector3.Lerp(new Vector3(80f, 3.2f, -1f), new Vector3(80f, 3.8f, -8f), k), new Vector3(80f, 2.2f, -24f));
                    break;
                }
                case 5:
                {
                    Narrator.Place("The Keepers' Tanks");
                    Caption(game, "One sank into the keepers' tanks, wrapped up in a knot of grey as big as a barrel...");
                    Vector3 clot = quest != null && quest.ClotSpots != null && quest.ClotSpots.Length > 0 ? quest.ClotSpots[0] : new Vector3(-74.5f, -4f, 112f);
                    float k = Ease((t - TTanks) / (TCrab - TTanks));
                    game.SetCutsceneCamera(new Vector3(-59.6f, Mathf.Lerp(2.4f, 1.6f, k), clot.z + Mathf.Lerp(-5f, -2f, k)), clot);
                    break;
                }
                case 6:
                {
                    Narrator.Place("The Grand Atrium");
                    Caption(game, "...and the third was carried off by a brass diving helmet that had grown legs. Nobody had expected that. Nobody ever does.");
                    Vector3 crab = quest != null && quest.Crab != null ? quest.Crab.transform.position : quest != null && quest.CrabSpots != null ? quest.CrabSpots[0] : new Vector3(-8f, 0f, -80f);
                    float k = Ease((t - TCrab) / (TClock - TCrab));
                    game.SetCutsceneCamera(crab + Vector3.Lerp(new Vector3(5f, 3f, -6f), new Vector3(3f, 1.8f, -4f), k), crab + Vector3.up * 0.8f);
                    break;
                }
                case 7:
                {
                    Narrator.Place(null);
                    Caption(game, "It was eleven o'clock. If midnight came and nobody was looking, the whale would go over unsung, and by morning the whole museum would be grey.");
                    float k = Ease((t - TClock) / (TPatch - TClock));
                    game.SetCutsceneCamera(WickClock + Vector3.Lerp(new Vector3(4f, 0.6f, 6.5f), new Vector3(2.6f, 0.3f, 4.2f), k), WickClock);
                    break;
                }
                case 8:
                {
                    Narrator.Place("The Forecourt");
                    Caption(game, "Fortunately, somebody small and woolly had just come up the steps from the sea. Find Wick, the night watchman, Patch. He'll be just inside the doors.");
                    Vector3 patch = game.Player.transform.position;
                    Vector3 fwd = game.Player.transform.forward;
                    float k = Ease((t - TPatch) / (TEnd - TPatch));
                    game.SetCutsceneCamera(patch + Quaternion.AngleAxis(Mathf.Lerp(150f, 180f, k), Vector3.up) * fwd * -Mathf.Lerp(9f, 6f, k) + Vector3.up * Mathf.Lerp(5f, 2.6f, k), patch + Vector3.up * 1.1f);
                    break;
                }
                default:
                    Finish(game);
                    break;
            }
        }

        void Finish(PatchworkGame game)
        {
            finished = true;
            active = false;
            if (lenses != null) foreach (var l in lenses) if (l != null) Destroy(l.gameObject);
            GameState.SetHintSeen(SeenKey);
            // The arrival shot would only tell it all again.
            GameState.SetHintSeen("L2_cine_arrive");
            game.CutsceneCaption = null;
            RenderOptimizer.SetFarView(false);
            Narrator.Card(null, null);
            Narrator.Place(null);
            game.CutsceneSkipFill = 0f;
            game.EndCutscene();
            Narrator.Say("Three lenses, three corners of the museum, and one hour to midnight. Hoo. No pressure.");
        }

        static void Caption(PatchworkGame game, string text)
        {
            if (game.CutsceneCaption != text) game.CutsceneCaption = text;
        }

        static float Ease(float k)
        {
            return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(k));
        }
    }

    public static partial class Level2Builder
    {
        // Set while the photo tour is going to run (the opening story stays out of its way).
        public static bool TouringNow;

        static void Intro(LevelKit kit)
        {
            var go = new GameObject("MuseumIntro");
            go.transform.SetParent(kit.Root, false);
            go.AddComponent<MuseumIntro>().Director = director;
        }
    }
}

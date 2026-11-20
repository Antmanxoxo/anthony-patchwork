using UnityEngine;

namespace Patchwork
{
    // The first time you visit the Oceanic Museum: the opening of Chapter Two, told by the
    // museum's own Storyteller (an old owl; see Narrator.Owl). Like Toybox Town's opening, it
    // shows the Fray doing the damage, one thing at a time:
    //   1. The museum from out at sea, under the button moon (the chapter card).
    //   2. The Observatory and the Night Glass, the great telescope.
    //   3. The Great Whale going over the dome, singing, and why that matters.
    //   4. Dusk: a seam splits in the Atrium floor and the Fray climbs out of it.
    //   5. It goes up to the Observatory, wraps itself round the telescope and unscrews the
    //      three lenses, then splits into three to carry them off.
    //   6. One climbs into the oldest armour in the Hall of Arms (which wakes up), one dives
    //      into the keepers' tanks and balls up into a grey clot, and one hides in a brass
    //      diving helmet, which grows legs.
    //   7. Wick's clock at eleven: an hour to midnight. 8. Patch at the Pod: go and find Wick.
    // Every caption stays up long enough to be written out and read (the shot lasts as long as
    // its line needs). Hold Confirm or Pause to skip. Plays once, never once the story's begun.
    public class MuseumIntro : MonoBehaviour
    {
        public const string SeenKey = "L2_intro_seen";
        public Level2Director Director;
        public Transform Blob;          // the Fray, a big grey knot
        public Transform[] Wisps;       // the three bits it splits into

        enum Shot { Sea, Glass, Whale, WhaleSong, Seam, Rises, Telescope, Unscrew, Arms, Tanks, Helmet, Grey, Clock, Patch }
        struct Beat { public Shot Shot; public string Line; public float Min; }
        Beat[] beats;
        float[] starts;
        float total;

        bool active, finished;
        float t = -1f, wait, skipHold;
        int beat = -1;
        Transform[] lenses;
        Vector3[] lensFrom;
        Vector3 wrapFrom, armsAt, clotAt, crabAt, seamAt;
        Transform suit, clot, crab;
        Quaternion suitRot;
        Vector3 clotScale, crabHome;
        FrayCreep creep;
        bool sang, popped, armsWoke, clotWoke, crabWoke;

        static readonly Vector3 C = new Vector3(0f, 14f, -20f);                // the Observatory
        static readonly Vector3 Tube = C + new Vector3(0f, 5.6f, 0.6f);        // the middle of the telescope
        static readonly Vector3 WickClock = new Vector3(7f, 2.4f, -107.5f);
        const float WriteSpeed = 34f;   // (the same as the Storyteller's)

        void Start()
        {
            var quest = Level2Quest.Instance;
            bool begun = quest != null && (quest.Met || quest.LensCount > 0);
            if (Director == null || Level2Builder.TouringNow || GameState.IsHintSeen(SeenKey) || GameState.IsLevelComplete(Level2Builder.SceneName) || begun)
            {
                finished = true;
                Hide();
                return;
            }
            active = true;
            beats = new[]
            {
                new Beat { Shot = Shot.Sea, Min = 6.5f, Line = "Out past the end of the pier, where the sea comes right up to the steps, stands the Oceanic Museum." },
                new Beat { Shot = Shot.Glass, Min = 6f, Line = "At the very top of it is the Observatory, and in the Observatory is the Night Glass: the great telescope." },
                new Beat { Shot = Shot.Whale, Min = 6f, Line = "Every night at midnight the keepers point it up through the dome, and find the Great Whale as she goes over." },
                new Beat { Shot = Shot.WhaleSong, Min = 6.5f, Line = "She sings when somebody sees her. And her song is what keeps the colour in this place: in every tank, every tapestry, every painted fish." },
                new Beat { Shot = Shot.Seam, Min = 5.5f, Line = "But tonight, just after dusk, a seam split open in the floor of the Grand Atrium..." },
                new Beat { Shot = Shot.Rises, Min = 6f, Line = "...and up through it came the Fray: grey, tangled, hungry things that unpick the colour out of whatever they touch." },
                new Beat { Shot = Shot.Telescope, Min = 5.5f, Line = "It went straight to the top of the house, and wrapped itself round the Night Glass..." },
                new Beat { Shot = Shot.Unscrew, Min = 6f, Line = "...and unscrewed all three of its lenses, neat as you like. Then it split itself in three, to carry them off." },
                new Beat { Shot = Shot.Arms, Min = 6f, Line = "One took its lens into the Hall of Arms, and climbed inside the oldest suit of armour there. The armour woke up." },
                new Beat { Shot = Shot.Tanks, Min = 6f, Line = "One dived into the keepers' tanks, and wound itself into a grey knot as big as a barrel, with the lens inside." },
                new Beat { Shot = Shot.Helmet, Min = 6f, Line = "And the last one hid inside a brass diving helmet, which promptly grew legs and scuttled off with it." },
                new Beat { Shot = Shot.Grey, Min = 5f, Line = "And all over the museum, the colour began to drain away." },
                new Beat { Shot = Shot.Clock, Min = 6f, Line = "It was eleven o'clock. If midnight came and nobody was looking, the whale would go over unsung, and by morning the museum would be grey for good." },
                new Beat { Shot = Shot.Patch, Min = 6.5f, Line = "Fortunately, somebody small and woolly had just come up the steps from the sea. Go and find Wick, the night watchman, Patch. He'll be just inside the doors." },
            };
            starts = new float[beats.Length];
            total = 0f;
            for (int i = 0; i < beats.Length; i++)
            {
                starts[i] = total;
                // Long enough to write the whole line out, and then to read it.
                total += Mathf.Max(beats[i].Min, beats[i].Line.Length / WriteSpeed + 2.8f);
            }

            // Where things are.
            seamAt = quest != null && quest.SeamSpots != null && quest.SeamSpots.Length > 0 ? quest.SeamSpots[0] : new Vector3(-8f, 0f, -97f);
            if (quest != null && quest.Fight != null && quest.Fight.Suits != null && quest.Fight.Suits.Length > 0) suit = quest.Fight.Suits[0];
            armsAt = suit != null ? suit.position : new Vector3(67f, 0f, -4f);
            if (suit != null) suitRot = suit.rotation;
            if (quest != null && quest.Clots != null && quest.Clots.Length > 0 && quest.Clots[0] != null) { clot = quest.Clots[0].transform; clotScale = clot.localScale; }
            clotAt = clot != null ? clot.position : quest != null && quest.ClotSpots != null && quest.ClotSpots.Length > 0 ? quest.ClotSpots[0] : new Vector3(-74.5f, -4f, 112f);
            if (quest != null && quest.Crab != null) { crab = quest.Crab.transform; crabHome = crab.position; }
            crabAt = crab != null ? crab.position : quest != null && quest.CrabSpots != null ? quest.CrabSpots[0] : new Vector3(-8f, 0f, -80f);

            // The three lenses, back in the telescope for the start of the story.
            lenses = new Transform[3];
            lensFrom = new Vector3[3];
            var glass = PatchworkAssets.Glow(new Color(0.55f, 0.85f, 1f), 1.8f);
            var rim = PatchworkAssets.Plain(new Color(0.9f, 0.72f, 0.36f), 0.75f);
            var tilt = Quaternion.Euler(-52f, 0f, 0f);
            float[] along = { 5.6f, 1.8f, -2f };
            for (int i = 0; i < 3; i++)
            {
                var lens = new GameObject("IntroLens" + i).transform;
                lens.SetParent(transform, false);
                lensFrom[i] = Tube + tilt * new Vector3(0f, 0f, along[i]);
                lens.position = lensFrom[i];
                lens.rotation = tilt;
                Part(lens, PrimitiveType.Cylinder, new Vector3(0.9f, 0.05f, 0.9f), glass);
                Part(lens, PrimitiveType.Cylinder, new Vector3(1.02f, 0.04f, 1.02f), rim);
                lens.gameObject.SetActive(false);
                lenses[i] = lens;
            }
            Hide();
        }

        void Hide()
        {
            if (Blob != null) Blob.gameObject.SetActive(false);
            if (Wisps != null) foreach (var w in Wisps) if (w != null) w.gameObject.SetActive(false);
        }

        static void Part(Transform parent, PrimitiveType type, Vector3 s, Material m)
        {
            var go = GameObject.CreatePrimitive(type);
            Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localScale = s;
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
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
            if (t <= 0f && (LoadingScreen.Busy || game.Fade > 0.02f)) { wait = 0f; return; }
            if (wait < 0.8f)
            {
                wait += Time.unscaledDeltaTime;
                Narrator.Card("Chapter Two", "The Night Glass");
                return;
            }

            // Each shot runs at the pace of the Storyteller's reading (as in Level 1's opening).
            t += Time.deltaTime * StoryPace();
            bool held = t > 0.6f && (game.Input.Confirm.IsPressed() || game.Input.Pause.IsPressed());
            skipHold = held ? skipHold + Time.unscaledDeltaTime : Mathf.Max(0f, skipHold - Time.unscaledDeltaTime * 2f);
            game.CutsceneSkipFill = skipHold / 0.9f;
            if (skipHold >= 0.9f || t >= total) { Finish(game); return; }

            int now = beats.Length - 1;
            for (int i = 0; i < beats.Length - 1; i++) if (t < starts[i + 1]) { now = i; break; }
            float len = (now + 1 < beats.Length ? starts[now + 1] : total) - starts[now];
            float lt = t - starts[now], k = Ease(lt / len);
            var shot = beats[now].Shot;
            if (now != beat)
            {
                // A dip to black whenever the story jumps somewhere else.
                bool sameRoom = shot == Shot.WhaleSong || shot == Shot.Rises || shot == Shot.Unscrew || shot == Shot.Whale && beats[Mathf.Max(0, now - 1)].Shot == Shot.Glass;
                if (beat >= 0 && !sameRoom) game.Blackout(1f);
                beat = now;
                if (now > 0) Narrator.Card(null, null);
            }
            Caption(game, beats[now].Line);
            Vector3 whale = Director.SkyWhale != null ? Director.SkyWhale.transform.position : C + Vector3.up * 30f;

            switch (shot)
            {
                case Shot.Sea:
                    Narrator.Card("Chapter Two", "The Night Glass");
                    game.SetCutsceneCamera(Vector3.Lerp(new Vector3(30f, 22f, -215f), new Vector3(12f, 13f, -178f), k), Vector3.Lerp(new Vector3(0f, 16f, -108f), new Vector3(0f, 12f, -110f), k));
                    break;
                case Shot.Glass:
                    Narrator.Place("The Observatory");
                    game.SetCutsceneCamera(Vector3.Lerp(C + new Vector3(-6f, 3f, 21f), C + new Vector3(8f, 5f, 15f), k), Tube);
                    break;
                case Shot.Whale:
                    Narrator.Place(null);
                    game.SetCutsceneCamera(Vector3.Lerp(C + new Vector3(8f, 5f, 15f), C + new Vector3(4f, 7f, -4f), k), Vector3.Lerp(Tube, whale, Mathf.Clamp01(lt / 2f)));
                    break;
                case Shot.WhaleSong:
                    if (!sang)
                    {
                        sang = true;
                        if (Director.SkyWhaleLife != null) Director.SkyWhaleLife.Sing();
                    }
                    game.SetCutsceneCamera(Vector3.Lerp(C + new Vector3(4f, 7f, -4f), C + new Vector3(0f, 13f, -2f), k), whale);
                    break;
                case Shot.Seam:
                {
                    // Low on the Atrium floor, by the seam, as it splits and something stirs.
                    Narrator.Place("The Grand Atrium");
                    if (lt < Time.deltaTime * 1.5f) { FxBurst.Spawn(FxBurst.Kind.Threads, seamAt + Vector3.up * 0.3f, SnarlKnot.FrayDark); Sfx.Play(SfxId.Unravel, 0.7f); }
                    if (creep == null && lt > 1f) creep = FrayCreep.Make(transform, seamAt, 2.4f, 3.3f);
                    if (Blob != null)
                    {
                        if (!Blob.gameObject.activeSelf && lt > 2.2f) { Blob.gameObject.SetActive(true); FxBurst.Spawn(FxBurst.Kind.Threads, seamAt + Vector3.up, SnarlKnot.FrayDark); Sfx.Play(SfxId.GateRumble, 0.5f); }
                        Blob.position = seamAt + Vector3.up * Mathf.Lerp(-1.2f, 0.4f, Ease((lt - 2.2f) / 2f));
                        Blob.localScale = Vector3.one * Mathf.Lerp(0.4f, 1f, Ease((lt - 2.2f) / 2f));
                    }
                    game.SetCutsceneCamera(seamAt + Vector3.Lerp(new Vector3(-6f, 2.2f, -7f), new Vector3(-4.5f, 1.6f, -5f), k), seamAt + Vector3.up * 0.6f);
                    break;
                }
                case Shot.Rises:
                {
                    // Up it comes, looking about, with the grey spreading out under it.
                    Narrator.Place(null);
                    if (Blob != null)
                    {
                        Blob.gameObject.SetActive(true);
                        Blob.position = seamAt + Vector3.up * Mathf.Lerp(0.4f, 3.6f, k) + new Vector3(Mathf.Sin(lt * 2.4f) * 0.4f, Mathf.Sin(lt * 3.7f) * 0.2f, 0f);
                        Blob.rotation = Quaternion.Euler(0f, lt * 70f, Mathf.Sin(lt * 3f) * 12f);
                        Blob.localScale = Vector3.one * Mathf.Lerp(1f, 1.5f, k);
                    }
                    if (creep != null) creep.transform.localScale = Vector3.one * Mathf.Lerp(1f, 2.4f, k);
                    Vector3 b = Blob != null ? Blob.position : seamAt;
                    game.SetCutsceneCamera(seamAt + Vector3.Lerp(new Vector3(-5f, 1.6f, -6f), new Vector3(-8f, 3f, -10f), k), b);
                    break;
                }
                case Shot.Telescope:
                {
                    // In through the dome, round and round the telescope.
                    if (Blob != null)
                    {
                        if (lt < Time.deltaTime * 1.5f) wrapFrom = C + new Vector3(-14f, 14f, -10f);
                        Blob.gameObject.SetActive(true);
                        float come = Ease(lt / 2.2f);
                        float a = lt * 2.6f;
                        Vector3 orbit = Tube + new Vector3(Mathf.Cos(a) * 2.2f, Mathf.Sin(lt * 1.3f) * 0.8f, Mathf.Sin(a) * 2.2f);
                        Blob.position = Vector3.Lerp(wrapFrom, orbit, come);
                        Blob.rotation = Quaternion.Euler(lt * 140f, lt * 90f, 0f);
                        Blob.localScale = Vector3.one * 1.3f;
                    }
                    foreach (var l in lenses) if (l != null && !l.gameObject.activeSelf) l.gameObject.SetActive(true);
                    game.SetCutsceneCamera(Vector3.Lerp(C + new Vector3(-9f, 6f, -6f), C + new Vector3(-8f, 7.5f, -3f), k), Tube);
                    break;
                }
                case Shot.Unscrew:
                {
                    // Pop, pop, pop: out come the lenses, and the knot comes apart in three, each
                    // bit grabbing one and making off.
                    if (!popped && lt > 0.6f)
                    {
                        popped = true;
                        Sfx.Play(SfxId.Unravel, 0.8f);
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        float out1 = Mathf.Clamp01((lt - 0.5f - i * 0.6f) / 0.5f);
                        float away = Mathf.Clamp01((lt - 2.6f) / 2.4f);
                        Vector3 popOut = lensFrom[i] + Vector3.left * 1.6f * out1 + Vector3.up * Mathf.Sin(out1 * Mathf.PI) * 0.8f;
                        Vector3 dir = i == 0 ? new Vector3(1f, 0.4f, 0.3f) : i == 1 ? new Vector3(-0.6f, 0.4f, 1f) : new Vector3(0.1f, 0.6f, -1f);
                        Vector3 p = popOut + dir.normalized * Ease(away) * 26f;
                        if (lenses[i] != null)
                        {
                            lenses[i].position = p;
                            lenses[i].Rotate(0f, 0f, Time.deltaTime * 400f * out1, Space.Self);
                            if (out1 > 0f && out1 < 0.1f) FxBurst.Spawn(FxBurst.Kind.Sparkle, lensFrom[i], new Color(0.6f, 0.85f, 1f));
                        }
                        var w = Wisps != null && i < Wisps.Length ? Wisps[i] : null;
                        if (w != null)
                        {
                            bool show = lt > 2.2f;
                            if (w.gameObject.activeSelf != show) w.gameObject.SetActive(show);
                            w.position = p + Vector3.up * 0.7f;
                            w.rotation = Quaternion.Euler(lt * 200f, lt * 130f * (i + 1), 0f);
                        }
                    }
                    if (Blob != null)
                    {
                        float split = Mathf.Clamp01((lt - 2f) / 0.4f);
                        if (split >= 1f && Blob.gameObject.activeSelf) { Blob.gameObject.SetActive(false); FxBurst.Spawn(FxBurst.Kind.Threads, Tube, SnarlKnot.FrayDark); }
                        else Blob.position = Tube + new Vector3(Mathf.Cos(lt * 2.6f) * 2.2f, 0f, Mathf.Sin(lt * 2.6f) * 2.2f);
                    }
                    game.SetCutsceneCamera(Vector3.Lerp(C + new Vector3(-8f, 7.5f, -3f), C + new Vector3(-10f, 10f, -10f), k), Vector3.Lerp(Tube, Tube + Vector3.up * 6f, Ease((lt - 2.6f) / 2.4f)));
                    break;
                }
                case Shot.Arms:
                {
                    foreach (var l in lenses) if (l != null && l.gameObject.activeSelf) l.gameObject.SetActive(false);
                    Narrator.Place("The Hall of Arms");
                    Vector3 head = armsAt + Vector3.up * 2.2f;
                    WispInto(0, head + new Vector3(8f, 6f, 6f), head, lt, 2.4f);
                    if (!armsWoke && lt > 2.4f)
                    {
                        armsWoke = true;
                        FxBurst.Spawn(FxBurst.Kind.Threads, head, SnarlKnot.FrayDark);
                        Sfx.Play(SfxId.GateRumble, 0.6f);
                    }
                    // It shudders, and turns its head to look.
                    if (suit != null && armsWoke)
                    {
                        float s = Mathf.Clamp01((lt - 2.4f) / 1.6f);
                        suit.rotation = suitRot * Quaternion.Euler(Mathf.Sin(lt * 30f) * 3f * (1f - s), Mathf.Lerp(0f, -25f, Ease(s)), 0f);
                    }
                    game.SetCutsceneCamera(armsAt + Vector3.Lerp(new Vector3(7f, 3.2f, 6f), new Vector3(5f, 2.6f, 4f), k), head);
                    break;
                }
                case Shot.Tanks:
                {
                    if (suit != null) suit.rotation = suitRot;
                    Narrator.Place("The Keepers' Tanks");
                    WispInto(1, clotAt + new Vector3(0f, 9f, -2f), clotAt, lt, 2.2f);
                    if (!clotWoke && lt > 2.2f)
                    {
                        clotWoke = true;
                        FxBurst.Spawn(FxBurst.Kind.Threads, clotAt, SnarlKnot.FrayDark);
                        Sfx.Play(SfxId.Unravel, 0.6f);
                    }
                    if (clot != null) clot.localScale = clotScale * (clotWoke ? UIKit.EaseOutBack(Mathf.Clamp01((lt - 2.2f) / 0.8f)) : 0.01f);
                    game.SetCutsceneCamera(new Vector3(-59.6f, Mathf.Lerp(2.4f, 1.6f, k), clotAt.z + Mathf.Lerp(-6f, -3f, k)), clotAt);
                    break;
                }
                case Shot.Helmet:
                {
                    if (clot != null) clot.localScale = clotScale;
                    Narrator.Place("The Grand Atrium");
                    WispInto(2, crabAt + new Vector3(-3f, 6f, 3f), crabAt + Vector3.up * 0.6f, lt, 2f);
                    if (!crabWoke && lt > 2f)
                    {
                        crabWoke = true;
                        FxBurst.Spawn(FxBurst.Kind.Puff, crabAt + Vector3.up * 0.5f, new Color(0.8f, 0.7f, 0.5f));
                        Sfx.Play(SfxId.Boing, 0.6f);
                    }
                    // Up it hops on its new legs, and off it trots.
                    if (crab != null && crabWoke)
                    {
                        float h = lt - 2f;
                        crab.position = crabHome + Vector3.up * Mathf.Abs(Mathf.Sin(h * 6f)) * 0.5f * Mathf.Clamp01(2.5f - h) + Vector3.right * Mathf.Clamp01((h - 1.2f) / 2f) * 3f;
                    }
                    game.SetCutsceneCamera(crabAt + Vector3.Lerp(new Vector3(5f, 3f, -6f), new Vector3(4f, 2f, -4.5f), k), (crab != null ? crab.position : crabAt) + Vector3.up * 0.8f);
                    break;
                }
                case Shot.Grey:
                {
                    if (crab != null) crab.position = crabHome;
                    Narrator.Place(null);
                    game.SetCutsceneCamera(Vector3.Lerp(new Vector3(-20f, 16f, -102f), new Vector3(-14f, 20f, -96f), k), new Vector3(0f, 6f, -70f));
                    break;
                }
                case Shot.Clock:
                    game.SetCutsceneCamera(WickClock + Vector3.Lerp(new Vector3(4f, 0.6f, 6.5f), new Vector3(2.6f, 0.3f, 4.2f), k), WickClock);
                    break;
                default:
                {
                    Narrator.Place("The Forecourt");
                    Vector3 patch = game.Player.transform.position;
                    Vector3 fwd = game.Player.transform.forward;
                    game.SetCutsceneCamera(patch + Quaternion.AngleAxis(Mathf.Lerp(150f, 180f, k), Vector3.up) * fwd * -Mathf.Lerp(9f, 6f, k) + Vector3.up * Mathf.Lerp(5f, 2.6f, k), patch + Vector3.up * 1.1f);
                    break;
                }
            }
        }

        // One of the three bits of Fray, flying in with its lens and vanishing into something.
        void WispInto(int i, Vector3 from, Vector3 to, float lt, float arrive)
        {
            var w = Wisps != null && i < Wisps.Length ? Wisps[i] : null;
            var lens = lenses != null && i < lenses.Length ? lenses[i] : null;
            float k = Mathf.Clamp01(lt / arrive);
            bool show = k < 1f;
            Vector3 p = Vector3.Lerp(from, to, Ease(k)) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.5f;
            if (w != null)
            {
                if (w.gameObject.activeSelf != show) w.gameObject.SetActive(show);
                w.position = p + Vector3.up * 0.6f;
                w.rotation = Quaternion.Euler(lt * 200f, lt * 160f, 0f);
                w.localScale = Vector3.one * Mathf.Lerp(1f, 0.3f, Mathf.Clamp01((k - 0.8f) / 0.2f));
            }
            if (lens != null)
            {
                if (lens.gameObject.activeSelf != show) lens.gameObject.SetActive(show);
                lens.position = p;
                lens.Rotate(0f, 0f, Time.deltaTime * 300f, Space.Self);
            }
        }

        int paceShot = -1;
        bool paceSet;
        float pace = 1f;

        float StoryPace()
        {
            int shot = 0;
            for (int i = 1; i < starts.Length; i++) if (t >= starts[i]) shot = i;
            float next = shot + 1 < starts.Length ? starts[shot + 1] : total;
            if (shot != paceShot) { paceShot = shot; paceSet = false; pace = 1f; }
            float left = next - t;
            if (!paceSet && NarratorVoice.Speaking)
            {
                pace = Mathf.Clamp(left / (NarratorVoice.Remaining + 0.6f), 0.4f, 1f);
                paceSet = true;
            }
            if (NarratorVoice.Speaking && left < 0.1f) return 0f;
            return pace;
        }

        void Finish(PatchworkGame game)
        {
            NarratorVoice.Stop();
            finished = true;
            active = false;
            if (lenses != null) foreach (var l in lenses) if (l != null) Destroy(l.gameObject);
            if (Blob != null) Destroy(Blob.gameObject);
            if (Wisps != null) foreach (var w in Wisps) if (w != null) Destroy(w.gameObject);
            if (creep != null) Destroy(creep.gameObject);
            if (suit != null) suit.rotation = suitRot;
            if (clot != null) clot.localScale = clotScale;
            if (crab != null) crab.position = crabHome;
            GameState.SetHintSeen(SeenKey);
            // The arrival shot would only tell it all again.
            GameState.SetHintSeen("L2_cine_arrive");
            game.CutsceneCaption = null;
            RenderOptimizer.SetFarView(false);
            Narrator.Card(null, null);
            Narrator.Place(null);
            game.CutsceneSkipFill = 0f;
            game.EndCutscene();
            Narrator.Say("Three lenses, three corners of the museum, and one hour to midnight. Hoo. Wick will know where to start.");
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
            var intro = go.AddComponent<MuseumIntro>();
            intro.Director = director;
            // The Fray, as the story shows it: one big grey knot, and the three bits it splits into.
            var blob = kit.Group("IntroFray", Vector3.down * 200f);
            FrayTheme.Knot(kit, blob, 1.6f, 22);
            blob.SetParent(go.transform, true);
            intro.Blob = blob;
            intro.Wisps = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                var w = kit.Group("IntroWisp" + i, Vector3.down * 200f);
                FrayTheme.Knot(kit, w, 0.7f, 10);
                w.SetParent(go.transform, true);
                intro.Wisps[i] = w;
            }
        }
    }
}

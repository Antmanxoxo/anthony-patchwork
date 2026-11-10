using UnityEngine;

namespace Patchwork
{
    // The first time you visit Toybox Town: the opening chapter of the storybook, told by the
    // Storyteller (see Narrator).
    //   1. A chapter card over the toybox seen from outside, in the nursery.
    //   2. Down over the wall into town on the morning of the wedding.
    //   3. The band practising on the bandstand (with all their music).
    //   4. A crack opens in the toybox lid and the Fray creeps out and swoops onto the bandstand.
    //      The music stops.
    //   5. The camera follows each thing the band has lost to where it ends up, with the
    //      place's name, so you know the three places to go: the lute string to Spindle Hill,
    //      the Dye Works (for the trumpet's cure), and Wisp the piper to the weathervane.
    //   6. The Fray ties itself round the clock tower and grey threads spread over town.
    //   7. The Mayor and his bride, with no band for their wedding.
    //   8. Patch, who has just landed. Over to you!
    // Hold Confirm or Pause to skip it. It only plays once (saved), and never once you've found
    // any of the band's things.
    public class ToyboxIntro : MonoBehaviour
    {
        public const string SeenKey = "L1_intro_seen";
        public const string ReplayPref = "Patchwork_ReplayToyboxIntro";   // editor only: play it again once
        public Level1Director Director;
        public Transform Crack, Blob, Streak;

        bool active, finished;
        float t = -1f, wait;
        Vector3 knotScale, crackScale;
        Vector3 wispFrom, wispTo, streakFrom, streakTo, blobHover;
        bool snapped, wispGone, knotTied;
        float skipHold;

        static readonly Vector3 CrackPos = new Vector3(0f, 40.8f, Level1Builder.BoxHalf - 0.15f);

        void Start()
        {
            blobHover = new Vector3(0f, 7.9f, 7f);
            bool replay = false;
#if UNITY_EDITOR
            // Patchwork > Replay Toybox Town Opening.
            replay = UnityEditor.EditorPrefs.GetBool(ReplayPref, false);
            if (replay) UnityEditor.EditorPrefs.SetBool(ReplayPref, false);
#endif
            if (Director == null || !replay && (GameState.IsHintSeen(SeenKey) || GameState.IsLevelComplete(Level1Builder.SceneName) || Level1Director.BandCount() > 0))
            {
                finished = true;
                // The crack the Fray came in by stays until the town is mended.
                if (Crack != null) Crack.gameObject.SetActive(!GameState.IsLevelComplete(Level1Builder.SceneName));
                return;
            }
            active = true;
            // The town as it was before the Fray came: band whole and playing, no grey anywhere.
            if (Director.Lute != null) Director.Lute.SetInstrumentFixed(true);
            if (Director.Trumpet != null) Director.Trumpet.SetInstrumentFixed(true);
            if (Director.Piper != null) Director.Piper.gameObject.SetActive(true);
            if (Director.StrandedPiper != null) Director.StrandedPiper.gameObject.SetActive(false);
            foreach (var n in new[] { Director.Lute, Director.Trumpet, Director.Piper, Director.Drummer }) if (n != null) n.Playing = true;
            foreach (var kv in Director.Regions) kv.Value.HideFray();
            if (Director.SquareRegion != null) Director.SquareRegion.HideFray();
            if (Director.TowerKnot != null)
            {
                knotScale = Director.TowerKnot.localScale;
                Director.TowerKnot.gameObject.SetActive(false);
            }
            if (Crack != null) crackScale = Crack.localScale;
            MusicPlayer.SetBand(true, true, true);
            MusicPlayer.SnapBand();
        }

        void Update()
        {
            if (!active || finished) return;
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) return;
            if (t < 0f)
            {
                // Take over the camera straight away, behind the loading screen, so the first
                // thing you see is the chapter card (not Patch, then a jump cut).
                if (game.Mode != UIMode.Gameplay) return;
                t = 0f;
                // Patch has already hopped out of the Pod by the time the story gets to him.
                var arrival = FindAnyObjectByType<PodLaunch>();
                if (arrival != null) arrival.SkipToLanded(game);
                game.StartScriptedCutscene(new Vector3(0f, 150f, -200f), new Vector3(0f, 26f, 0f), 0.9f);
                game.CutsceneSkippable = true;
                game.CameraRig.SnapShowcase();
                // The whole town is on show from far off: draw every little thing at any
                // distance while the story plays, so nothing pops in as the camera flies about.
                RenderOptimizer.SetFarView(true);
                LevelLook.SetMiniature(false);   // the whole town from far off: keep it sharp
            }
            if (game.Mode != UIMode.Cutscene) return;

            // Hold on the opening shot until the loading screen has gone and the picture has
            // settled for a moment, then start the story.
            if (LoadingScreen.Busy || game.Fade > 0.02f) { wait = 0f; return; }
            if (wait < 0.6f)
            {
                wait += Time.unscaledDeltaTime;
                Narrator.Card("Chapter One", "The Wedding That Nearly Wasn't");
                return;
            }

            float dt = Time.deltaTime;
            t += dt;
            // Hold Confirm (or Pause) to skip, so a stray jump press can't skip it by accident.
            bool held = t > 0.6f && (game.Input.Confirm.IsPressed() || game.Input.Pause.IsPressed());
            skipHold = held ? skipHold + Time.unscaledDeltaTime : Mathf.Max(0f, skipHold - Time.unscaledDeltaTime * 2f);
            game.CutsceneSkipFill = skipHold / 0.9f;
            if (skipHold >= 0.9f)
            {
                Finish(game);
                return;
            }

            var band = Bandstand();
            Vector3 hill = new Vector3(-67f, 6f, 31f);
            Vector3 dye = Level1Builder.DyeWorksPoint;
            Vector3 outside = new Vector3(0f, 150f, -200f), overWall = new Vector3(0f, 44f, -82f);
            if (t < TTown)
            {
                // The toybox from outside, in the nursery, under its mobile.
                Narrator.Card("Chapter One", "The Wedding That Nearly Wasn't");
                Caption(game, "Once upon a bedtime, at the foot of a perfectly ordinary bed, there sat a most extraordinary toybox.");
                float k = Ease(t / TTown);
                game.SetCutsceneCamera(outside + new Vector3(Mathf.Sin(k * 1.2f) * 30f, -k * 14f, k * 18f), new Vector3(0f, 26f, 0f));
            }
            else if (t < TBand)
            {
                // Down over the wall and into town.
                Narrator.Card(null, null);
                Caption(game, "Inside it lay Toybox Town, which was having the finest morning of its life. The Mayor was getting married.");
                float k = Ease((t - TTown) / (TBand - TTown));
                Vector3 from = outside + new Vector3(Mathf.Sin(1.2f) * 30f, -14f, 18f);
                game.SetCutsceneCamera(Vector3.Lerp(from, overWall, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 10f, Vector3.Lerp(new Vector3(0f, 26f, 0f), band + Vector3.up * 3f, k));
            }
            else if (t < TCrack)
            {
                Caption(game, "The town band had been practising for weeks. They had very nearly learned the tune.");
                float k = Ease((t - TBand) / (TCrack - TBand));
                game.SetCutsceneCamera(Vector3.Lerp(new Vector3(7f, 3.5f, -3f), new Vector3(-7f, 3.8f, -2f), k), band + new Vector3(0f, 2.2f, 0f));
            }
            else if (t < TSnap)
            {
                // The crack opens, and the Fray comes creeping out.
                Caption(game, "But then, as things so often do at the worst possible moment, something came unstitched.");
                if (Crack != null)
                {
                    if (!Crack.gameObject.activeSelf) { Crack.gameObject.SetActive(true); Sfx.Play(SfxId.Unravel, 0.7f); }
                    Crack.localScale = crackScale * UIKit.EaseOutBack(Mathf.Clamp01((t - TCrack) / 0.7f));
                }
                Vector3 above = band + new Vector3(0f, 7f, 0f);
                blobHover = above;
                Vector3 bp = CrackPos + Vector3.back * 2f;
                if (t > TCrack + 0.8f)
                {
                    if (Blob != null && !Blob.gameObject.activeSelf)
                    {
                        Blob.gameObject.SetActive(true);
                        FxBurst.Spawn(FxBurst.Kind.Threads, CrackPos, SnarlKnot.FrayDark);
                    }
                    float k = Ease((t - TCrack - 0.8f) / (TSnap - TCrack - 0.8f));
                    // A swooping curve down from the lid to over the bandstand.
                    Vector3 mid = new Vector3(14f, 30f, 45f);
                    bp = Vector3.Lerp(Vector3.Lerp(CrackPos + Vector3.back * 2f, mid, k), Vector3.Lerp(mid, above, k), k);
                    if (Blob != null) Blob.position = bp;
                }
                Vector3 camPos = Vector3.Lerp(new Vector3(-3f, 5f, -5f), new Vector3(0f, 6f, -9f), Ease((t - TCrack) / (TSnap - TCrack)));
                game.SetCutsceneCamera(camPos, Vector3.Lerp(CrackPos, bp, Mathf.Clamp01((t - TCrack - 0.8f) / 2f)));
            }
            else if (t < THill)
            {
                // The Fray over the bandstand: the music stops.
                Hover();
                if (!snapped && t > TSnap + 0.5f) Snap(game);
                Caption(game, "The Fray. It has never cared for music.");
                game.SetCutsceneCamera(Vector3.Lerp(new Vector3(0f, 5.5f, -5.5f), new Vector3(1.5f, 6.5f, -7.5f), Ease((t - TSnap) / (THill - TSnap))), band + Vector3.up * 3.6f);
            }
            else if (t < TDye)
            {
                // Follow the lute string all the way to Spindle Hill.
                Hover();
                Caption(game, "It flung the lute's only string clean across town to the top of Spindle Hill...");
                float k = Mathf.Clamp01((t - THill) / 3f);
                Vector3 p = Vector3.Lerp(streakFrom, streakTo, Ease(k)) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 16f;
                if (Streak != null && Streak.gameObject.activeSelf)
                {
                    if (k > 0f) Streak.rotation = Quaternion.LookRotation((p - Streak.position).normalized + Vector3.forward * 0.001f);
                    Streak.position = p;
                    if (k >= 1f)
                    {
                        Streak.gameObject.SetActive(false);
                        FxBurst.Spawn(FxBurst.Kind.Sparkle, streakTo, new Color(1f, 0.85f, 0.5f));
                    }
                }
                if (k >= 0.85f) Narrator.Place("Spindle Hill");
                // The camera chases it, then settles looking up at where it came down.
                Vector3 chase = p + new Vector3(9f, 4f, -11f);
                Vector3 settle = hill + new Vector3(15f, 5f, -16f);
                game.SetCutsceneCamera(Vector3.Lerp(chase, settle, Ease((k - 0.6f) / 0.4f)), Vector3.Lerp(p, hill, Ease((k - 0.6f) / 0.4f)));
            }
            else if (t < TVane)
            {
                // The Dye Works, where the cure for the trumpet is.
                Hover();
                Narrator.Place("The Dye Works");
                Caption(game, "...gummed up the trumpet so thoroughly that only valve dew from the Dye Works will ever cure it...");
                float k = Ease((t - TDye) / (TVane - TDye));
                game.SetCutsceneCamera(dye + Vector3.Lerp(new Vector3(-20f, 13f, -22f), new Vector3(-12f, 9f, -19f), k), dye + new Vector3(0f, 5f, 0f));
            }
            else if (t < TKnot)
            {
                // Wisp is blown away south to the weathervane; the camera goes with him.
                Hover();
                Caption(game, "...and blew poor Wisp the piper up onto the weathervane, where he is being very brave about it.");
                float k = Mathf.Clamp01((t - TVane - 0.3f) / 3f);
                Vector3 look = wispTo;
                if (Director.Piper != null && Director.Piper.gameObject.activeSelf && !wispGone)
                {
                    Vector3 p = Vector3.Lerp(wispFrom, wispTo, Ease(k)) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 10f;
                    Director.Piper.transform.position = p;
                    Director.Piper.transform.rotation = Quaternion.Euler(t * 400f, t * 220f, 0f);
                    look = p;
                    if (k >= 1f)
                    {
                        wispGone = true;
                        Director.Piper.gameObject.SetActive(false);
                        if (Director.StrandedPiper != null) Director.StrandedPiper.gameObject.SetActive(true);
                        FxBurst.Spawn(FxBurst.Kind.Puff, wispTo, Color.white);
                    }
                }
                if (k >= 0.85f) Narrator.Place("The Weathervane");
                else Narrator.Place(null);
                Vector3 chase = look + new Vector3(-8f, 3f, 12f);
                Vector3 settle = wispTo + new Vector3(-7f, 2.5f, 9f);
                game.SetCutsceneCamera(Vector3.Lerp(chase, settle, Ease((k - 0.6f) / 0.4f)), look);
            }
            else if (t < TCouple)
            {
                Narrator.Place(null);
                if (t < TKnot + 2.4f) Caption(game, "Then it tied itself in a smug little knot around the clock tower...");
                else Caption(game, "...and the colour drained out of everything it touched.");
                Vector3 knotPos = Director.TowerKnot != null ? Director.TowerKnot.position : new Vector3(0f, 19f, 22.1f);
                float k = Ease(Mathf.Clamp01((t - TKnot) / 2f));
                if (Blob != null && Blob.gameObject.activeSelf)
                {
                    Blob.position = Vector3.Lerp(blobHover, knotPos, k);
                    if (t > TKnot + 2f)
                    {
                        float sc = 1f - Mathf.Clamp01((t - TKnot - 2f) / 0.6f);
                        Blob.localScale = Vector3.one * Mathf.Max(0.01f, sc);
                        if (sc <= 0f) Blob.gameObject.SetActive(false);
                    }
                }
                if (!knotTied && t > TKnot + 2f)
                {
                    knotTied = true;
                    if (Director.TowerKnot != null) { Director.TowerKnot.gameObject.SetActive(true); Director.TowerKnot.localScale = Vector3.zero; }
                    FxBurst.Spawn(FxBurst.Kind.Threads, knotPos, SnarlKnot.FrayDark);
                    Sfx.Play(SfxId.Unravel, 0.8f);
                    foreach (var kv in Director.Regions) kv.Value.GrowFray();
                    if (Director.SquareRegion != null) Director.SquareRegion.GrowFray();
                }
                if (knotTied && Director.TowerKnot != null)
                    Director.TowerKnot.localScale = knotScale * UIKit.EaseOutBack(Mathf.Clamp01((t - TKnot - 2f) / 0.8f));
                // Pulling back and up, so you see the grey spreading out over the town.
                float pull = Ease((t - TKnot) / (TCouple - TKnot));
                game.SetCutsceneCamera(Vector3.Lerp(new Vector3(7f, 7f, -2f), new Vector3(10f, 24f, -30f), pull), Vector3.Lerp(blobHover, knotPos, Mathf.Clamp01(k * 1.3f)));
            }
            else if (t < TPatch)
            {
                Caption(game, "No band, no music. No music, no wedding. It was all a dreadful pickle.");
                Vector3 couple = (Director.MayorSpot + Director.BrideSpot) * 0.5f;
                game.SetCutsceneCamera(Vector3.Lerp(couple + new Vector3(0f, 2.4f, -6f), couple + new Vector3(0f, 2.1f, -4.8f), Ease((t - TCouple) / (TPatch - TCouple))), couple + new Vector3(0f, 1.4f, 0f));
            }
            else if (t < TEnd)
            {
                // Patch, who has only just landed.
                Narrator.Place("Landing Meadow");
                Caption(game, "Fortunately, somebody small and woolly had just dropped in. Go and talk to the toys, Patch. They'll know what the band needs.");
                Vector3 patch = game.Player.transform.position;
                Vector3 fwd = game.Player.transform.forward;
                float k = Ease((t - TPatch) / (TEnd - TPatch));
                game.SetCutsceneCamera(patch + Quaternion.AngleAxis(Mathf.Lerp(150f, 180f, k), Vector3.up) * fwd * -Mathf.Lerp(9f, 6f, k) + Vector3.up * Mathf.Lerp(5f, 2.6f, k), patch + Vector3.up * 1.1f);
            }
            else Finish(game);
        }

        // When each part of the story starts, in seconds.
        const float TTown = 5f, TBand = 10.5f, TCrack = 15.5f, TSnap = 19.5f, THill = 22f, TDye = 26.5f, TVane = 30.5f, TKnot = 35.5f, TCouple = 40.5f, TPatch = 44.5f, TEnd = 50f;

        // The Fray hanging over the bandstand, twitching.
        void Hover()
        {
            if (Blob != null) Blob.position = blobHover + new Vector3(Mathf.Sin(t * 5f) * 0.4f, Mathf.Sin(t * 3.3f) * 0.3f, 0f);
        }

        Vector3 Bandstand()
        {
            return Director.Drummer != null ? Director.Drummer.transform.position + new Vector3(0f, 0f, -2.8f) : new Vector3(0f, 0.9f, 7f);
        }

        // The moment the music stops.
        void Snap(PatchworkGame game)
        {
            snapped = true;
            MusicPlayer.SetBand(false, false, false);
            MusicPlayer.SnapBand();
            Sfx.Play(SfxId.Unravel, 1f);
            Sfx.Play(SfxId.TransformFail, 0.7f);
            if (Blob != null) FxBurst.Spawn(FxBurst.Kind.Threads, Blob.position, SnarlKnot.FrayDark);
            if (Director.Lute != null)
            {
                Director.Lute.SetInstrumentFixed(false);
                Director.Lute.Playing = false;
                streakFrom = Director.Lute.transform.position + Vector3.up * 1.2f;
                streakTo = new Vector3(-67f, 6f, 31f);
                if (Streak != null) { Streak.position = streakFrom; Streak.gameObject.SetActive(true); }
            }
            if (Director.Trumpet != null)
            {
                Director.Trumpet.SetInstrumentFixed(false);
                Director.Trumpet.Playing = false;
                FxBurst.Spawn(FxBurst.Kind.Puff, Director.Trumpet.transform.position + Vector3.up * 1.2f, new Color(0.6f, 0.58f, 0.62f));
            }
            if (Director.Piper != null)
            {
                Director.Piper.Playing = false;
                wispFrom = Director.Piper.transform.position;
                wispTo = Director.StrandedPiper != null ? Director.StrandedPiper.transform.position : new Vector3(1.4f, 16f, -71.2f);
            }
        }

        void Finish(PatchworkGame game)
        {
            finished = true;
            active = false;
            if (Blob != null) Blob.gameObject.SetActive(false);
            if (Streak != null) Streak.gameObject.SetActive(false);
            if (Crack != null) { Crack.gameObject.SetActive(true); Crack.localScale = crackScale; }
            if (Director.TowerKnot != null) { Director.TowerKnot.gameObject.SetActive(true); Director.TowerKnot.localScale = knotScale; }
            foreach (var kv in Director.Regions) kv.Value.ShowFrayNow();
            if (Director.SquareRegion != null) Director.SquareRegion.ShowFrayNow();
            if (Director.Piper != null)
            {
                Director.Piper.transform.position = Director.Drummer != null ? Director.Drummer.transform.position + new Vector3(0f, 0f, -2.4f) : new Vector3(0f, 0.9f, 7.4f);
                Director.Piper.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            if (Director.StrandedPiper != null) Director.StrandedPiper.gameObject.SetActive(true);
            // Back to how the town really is now (broken instruments, the band's music gone).
            Director.ApplySavedState();
            GameState.SetHintSeen(SeenKey);
            game.CutsceneCaption = null;
            RenderOptimizer.SetFarView(false);
            LevelLook.SetMiniature(true);
            Narrator.Card(null, null);
            Narrator.Place(null);
            game.CutsceneSkipFill = 0f;
            game.EndCutscene();
            Narrator.Say("Three things lost, in three corners of town. I do hope you brought sensible shoes.");
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
}

using UnityEngine;

namespace Patchwork
{
    // The first time you visit Toybox Town: a short cutscene of the Fray arriving.
    //   1. The town on the morning of the wedding, from high above.
    //   2. The band practising on the bandstand (with all their music).
    //   3. A crack opens in the toybox lid and the Fray creeps out.
    //   4. It swoops onto the bandstand: the lute string snaps off and flies west, the trumpet
    //      seizes up, and Wisp the piper is blown onto the weathervane. The music stops.
    //   5. It ties itself round the clock tower and grey threads spread over town.
    //   6. The Mayor and his bride, with no band for their wedding. Over to you!
    // Confirm or Pause skips it. It only plays once (saved), and never once you've found any of
    // the band's things.
    public class ToyboxIntro : MonoBehaviour
    {
        public const string SeenKey = "L1_intro_seen";
        public Level1Director Director;
        public Transform Crack, Blob, Streak;

        bool active, finished;
        float t = -1f, wait;
        Vector3 knotScale, crackScale;
        Vector3 wispFrom, wispTo, streakFrom, streakTo, blobHover;
        bool snapped, wispGone, knotTied;
        float skipHold;

        static readonly Vector3 CrackPos = new Vector3(0f, 40.8f, 89.85f);

        void Start()
        {
            blobHover = new Vector3(0f, 7.9f, 7f);
            if (Director == null || GameState.IsHintSeen(SeenKey) || GameState.IsLevelComplete(Level1Builder.SceneName) || Level1Director.BandCount() > 0)
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
                // Wait for the loading fade, then roll.
                if (game.Mode != UIMode.Gameplay || game.Fade > 0.02f || LoadingScreen.Busy) return;
                wait += Time.unscaledDeltaTime;
                if (wait < 0.4f) return;
                t = 0f;
                game.StartScriptedCutscene(new Vector3(0f, 62f, -120f), new Vector3(0f, 8f, 10f), 0.9f);
                game.CutsceneSkippable = true;
                game.CameraRig.SnapShowcase();
            }
            if (game.Mode != UIMode.Cutscene) return;

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
            if (t < 5f)
            {
                Caption(game, "Toybox Town, on the morning of the Mayor's wedding...");
                float k = Ease(t / 5f);
                game.SetCutsceneCamera(Vector3.Lerp(new Vector3(0f, 62f, -120f), new Vector3(0f, 40f, -76f), k), Vector3.Lerp(new Vector3(0f, 8f, 10f), new Vector3(0f, 6f, 12f), k));
            }
            else if (t < 9.5f)
            {
                Caption(game, "The town band was practising for the big day.");
                float k = Ease((t - 5f) / 4.5f);
                game.SetCutsceneCamera(Vector3.Lerp(new Vector3(7f, 3.5f, -3f), new Vector3(-7f, 3.8f, -2f), k), band + new Vector3(0f, 2.2f, 0f));
            }
            else if (t < 13.5f)
            {
                // The crack opens, and the Fray comes creeping out.
                Caption(game, "But then the Fray crept in, through a crack in the lid!");
                if (Crack != null)
                {
                    if (!Crack.gameObject.activeSelf) { Crack.gameObject.SetActive(true); Sfx.Play(SfxId.Unravel, 0.7f); }
                    Crack.localScale = crackScale * UIKit.EaseOutBack(Mathf.Clamp01((t - 9.5f) / 0.7f));
                }
                Vector3 above = band + new Vector3(0f, 7f, 0f);
                blobHover = above;
                Vector3 bp = CrackPos + Vector3.back * 2f;
                if (t > 10.3f)
                {
                    if (Blob != null && !Blob.gameObject.activeSelf)
                    {
                        Blob.gameObject.SetActive(true);
                        FxBurst.Spawn(FxBurst.Kind.Threads, CrackPos, SnarlKnot.FrayDark);
                    }
                    float k = Ease((t - 10.3f) / 3.2f);
                    // A swooping curve down from the lid to over the bandstand.
                    Vector3 mid = new Vector3(14f, 30f, 45f);
                    bp = Vector3.Lerp(Vector3.Lerp(CrackPos + Vector3.back * 2f, mid, k), Vector3.Lerp(mid, above, k), k);
                    if (Blob != null) Blob.position = bp;
                }
                Vector3 camPos = Vector3.Lerp(new Vector3(-3f, 5f, -5f), new Vector3(0f, 6f, -9f), Ease((t - 9.5f) / 4f));
                game.SetCutsceneCamera(camPos, Vector3.Lerp(CrackPos, bp, Mathf.Clamp01((t - 10.3f) / 2f)));
            }
            else if (t < 17.5f)
            {
                if (Blob != null) Blob.position = blobHover + new Vector3(Mathf.Sin(t * 5f) * 0.4f, Mathf.Sin(t * 3.3f) * 0.3f, 0f);
                if (!snapped && t > 14f) Snap(game);
                if (t < 15.2f) Caption(game, "It snapped Pip's lute string and seized up Brassby's trumpet...");
                else Caption(game, "...and blew poor Wisp the piper right up onto the weathervane!");
                // The lute string flies off west to Spindle Hill.
                if (Streak != null && Streak.gameObject.activeSelf)
                {
                    float k = Mathf.Clamp01((t - 14f) / 2.2f);
                    Vector3 p = Vector3.Lerp(streakFrom, streakTo, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 14f;
                    if (k > 0f) Streak.rotation = Quaternion.LookRotation((p - Streak.position).normalized + Vector3.forward * 0.001f);
                    Streak.position = p;
                    if (k >= 1f) Streak.gameObject.SetActive(false);
                }
                // Wisp tumbles away south to the weathervane.
                if (Director.Piper != null && Director.Piper.gameObject.activeSelf && t > 14.4f)
                {
                    float k = Mathf.Clamp01((t - 14.4f) / 2.6f);
                    Vector3 p = Vector3.Lerp(wispFrom, wispTo, Ease(k)) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 10f;
                    Director.Piper.transform.position = p;
                    Director.Piper.transform.rotation = Quaternion.Euler(t * 400f, t * 220f, 0f);
                    if (k >= 1f && !wispGone)
                    {
                        wispGone = true;
                        Director.Piper.gameObject.SetActive(false);
                        if (Director.StrandedPiper != null) Director.StrandedPiper.gameObject.SetActive(true);
                        FxBurst.Spawn(FxBurst.Kind.Puff, wispTo, Color.white);
                    }
                }
                Vector3 look = Director.Piper != null && Director.Piper.gameObject.activeSelf && t > 14.6f ? Director.Piper.transform.position : band + Vector3.up * 3f;
                float c = Ease(Mathf.Clamp01((t - 14.8f) / 2.5f));
                game.SetCutsceneCamera(Vector3.Lerp(new Vector3(0f, 5.5f, -5.5f), new Vector3(2f, 12f, -32f), c), look);
            }
            else if (t < 22f)
            {
                if (t < 20f) Caption(game, "Then it tied itself in a knot round the clock tower...");
                else Caption(game, "...and every toy it touched turned grey and broken.");
                Vector3 knotPos = Director.TowerKnot != null ? Director.TowerKnot.position : new Vector3(0f, 19f, 22.1f);
                float k = Ease(Mathf.Clamp01((t - 17.5f) / 2f));
                if (Blob != null && Blob.gameObject.activeSelf)
                {
                    Blob.position = Vector3.Lerp(blobHover, knotPos, k);
                    if (t > 19.5f)
                    {
                        float s = 1f - Mathf.Clamp01((t - 19.5f) / 0.6f);
                        Blob.localScale = Vector3.one * Mathf.Max(0.01f, s);
                        if (s <= 0f) Blob.gameObject.SetActive(false);
                    }
                }
                if (!knotTied && t > 19.5f)
                {
                    knotTied = true;
                    if (Director.TowerKnot != null) { Director.TowerKnot.gameObject.SetActive(true); Director.TowerKnot.localScale = Vector3.zero; }
                    FxBurst.Spawn(FxBurst.Kind.Threads, knotPos, SnarlKnot.FrayDark);
                    Sfx.Play(SfxId.Unravel, 0.8f);
                    foreach (var kv in Director.Regions) kv.Value.GrowFray();
                    if (Director.SquareRegion != null) Director.SquareRegion.GrowFray();
                }
                if (knotTied && Director.TowerKnot != null)
                    Director.TowerKnot.localScale = knotScale * UIKit.EaseOutBack(Mathf.Clamp01((t - 19.5f) / 0.8f));
                game.SetCutsceneCamera(Vector3.Lerp(new Vector3(7f, 7f, -2f), new Vector3(4f, 9f, 6f), Ease((t - 17.5f) / 4.5f)), Vector3.Lerp(blobHover, knotPos, Mathf.Clamp01(k * 1.3f)));
            }
            else if (t < 26.5f)
            {
                if (t < 24.2f) Caption(game, "No band means no music, and no music means no wedding!");
                else Caption(game, "Talk to the toys, Patch. They'll tell you what the band needs.");
                Vector3 couple = (Director.MayorSpot + Director.BrideSpot) * 0.5f;
                game.SetCutsceneCamera(Vector3.Lerp(couple + new Vector3(0f, 2.4f, -6f), couple + new Vector3(0f, 2.1f, -4.8f), Ease((t - 22f) / 4.5f)), couple + new Vector3(0f, 1.4f, 0f));
            }
            else Finish(game);
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
            game.CutsceneSkipFill = 0f;
            game.EndCutscene();
            game.ShowToast("Talk to the toys of Toybox Town to find out what the band needs!");
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

using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // A big rainbow-sheened bubble with a prize turning inside it: a sticker, or a wrapped gift
    // (hats and Skiff decorations). Touch it to pop it. Ones you already have are pale, and
    // just give some points.
    public class PrizeBubble : PlayerVolume
    {
        public string PrizeId;
        Transform visual, content;
        bool owned;
        float phase;
        Vector3 home;

        public static PrizeBubble Place(LevelKit kit, Vector3 pos, string prizeId, bool startHidden = false)
        {
            var def = Prizes.Get(prizeId);
            var b = kit.Volume<PrizeBubble>(pos, Vector3.one * 2.4f, "PrizeBubble");
            b.PrizeId = prizeId;
            b.home = pos;
            b.phase = pos.x * 0.3f;
            b.owned = GameState.HasPrize(prizeId);
            b.visual = kit.Group("Visual", pos);
            b.visual.SetParent(b.transform, true);
            var film = PatchworkAssets.Bubble(b.owned ? new Color(0.9f, 0.9f, 0.95f, 0.25f) : new Color(0.95f, 0.8f, 1f, 0.42f));
            var shell = kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 2.1f, film, default(Vector3), b.visual);
            shell.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            // A rainbow sheen: a few tinted glints round the top.
            Color[] sheen = { new Color(1f, 0.6f, 0.7f), new Color(1f, 0.9f, 0.5f), new Color(0.6f, 0.9f, 1f) };
            for (int i = 0; i < 3; i++)
            {
                var g = kit.Deco(PrimitiveType.Sphere, Quaternion.Euler(0f, i * 40f - 40f, 0f) * new Vector3(-0.45f, 0.62f, -0.55f), new Vector3(0.38f, 0.16f, 0.08f),
                    PatchworkAssets.Glow(sheen[i], b.owned ? 0.3f : 0.9f), new Vector3(0f, i * 40f - 40f, 30f), b.visual);
                g.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            b.content = kit.Group("Prize", pos);
            b.content.SetParent(b.visual, true);
            if (def != null && def.Kind == PrizeKind.Sticker)
            {
                var mat = PatchworkAssets.Sticker(StickerArt.Get((StickerId)def.Value));
                for (int s = 0; s < 2; s++)
                {
                    var q = kit.Deco(PrimitiveType.Quad, Vector3.zero, Vector3.one * 1.1f, mat, new Vector3(0f, s * 180f, 0f), b.content);
                    q.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
            else
            {
                // A wrapped gift with a ribbon and bow.
                Color paper = def != null && def.Kind == PrizeKind.Hat ? new Color(0.45f, 0.65f, 0.95f) : new Color(0.95f, 0.45f, 0.55f);
                kit.Deco(PrimitiveType.Cube, Vector3.zero, Vector3.one * 0.8f, PatchworkAssets.Felt(paper), default(Vector3), b.content);
                var ribbon = PatchworkAssets.Felt(new Color(1f, 0.85f, 0.35f));
                kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(0.84f, 0.84f, 0.16f), ribbon, default(Vector3), b.content);
                kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(0.16f, 0.84f, 0.84f), ribbon, default(Vector3), b.content);
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.14f, 0.5f, 0f), new Vector3(0.24f, 0.16f, 0.12f), ribbon, new Vector3(0f, 0f, s * 30f), b.content);
            }
            if (b.owned) b.content.localScale = Vector3.one * 0.7f;
            if (startHidden) b.gameObject.SetActive(false);
            return b;
        }

        public void MoveHome(Vector3 p) { home = p; transform.position = p; }

        // Shows a hidden bubble with a little puff (a reward appearing).
        public void Reveal()
        {
            if (gameObject.activeSelf) return;
            gameObject.SetActive(true);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position, new Color(1f, 0.85f, 0.5f));
            FxBurst.Spawn(FxBurst.Kind.Poof, transform.position, Color.white);
            Sfx.Play(SfxId.Pop, 0.5f);
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            var def = Prizes.Get(PrizeId);
            var game = player.Game;
            Vector3 p = transform.position;
            if (!owned && def != null)
            {
                Prizes.Grant(def);
                if (game != null) game.ShowPrize(def);
                ScoreKeeper.Bonus(500, p);
                Sfx.Play(SfxId.PrizeFanfare, 0.7f);
                Sfx.Voice(SfxId.VoiceYay, player.Forms.Current, 0.5f);
                Haptics.Pulse(0.4f, 0.4f, 0.25f);
                if (def.Kind == PrizeKind.Hat && game != null) game.ShowToast("Wear it from the Satchel!");
            }
            else
            {
                ScoreKeeper.Bonus(100, p);
                Sfx.Play(SfxId.Pop, 0.6f);
            }
            FxBurst.Spawn(FxBurst.Kind.Sparkle, p, new Color(1f, 0.85f, 0.6f));
            FxBurst.Spawn(FxBurst.Kind.Scraps, p, new Color(0.95f, 0.7f, 0.9f));
            gameObject.SetActive(false);
        }

        void Update()
        {
            float t = Time.time + phase;
            transform.position = home + Vector3.up * Mathf.Sin(t * 1.6f) * 0.2f;
            if (content != null) content.localRotation = Quaternion.Euler(Mathf.Sin(t) * 10f, t * 70f, 0f);
            if (visual != null)
            {
                float w = Mathf.Sin(t * 3.1f) * 0.035f;
                visual.localScale = new Vector3(1f + w, 1f - w, 1f + w);
            }
        }
    }

    // A spot on a board that wants a particular sticker ("stick a star here"). Put the right one
    // on it and something happens.
    public class StickerSwitch : MonoBehaviour
    {
        public static readonly List<StickerSwitch> All = new List<StickerSwitch>();
        public StickerId Wanted;
        public string SaveId;
        public float Radius = 1.1f;
        // Called with true the moment it's done, or with false on load if it was done before.
        public System.Action<bool> Done;
        public bool IsDone { get; private set; }

        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }

        void Start()
        {
            if (!string.IsNullOrEmpty(SaveId) && GameState.IsCollected(SaveId))
            {
                IsDone = true;
                if (Done != null) Done(false);
            }
        }

        // A sticker was just placed at point. Returns true if it set this switch off.
        public static bool TryStick(StickerId id, Vector3 point)
        {
            foreach (var s in All)
            {
                if (s.IsDone || Vector3.Distance(s.transform.position, point) > s.Radius) continue;
                if (s.Wanted != id)
                {
                    var game = PatchworkGame.Instance;
                    if (game != null) game.ShowToast("This spot wants a " + StickerArt.Name(s.Wanted) + " sticker.");
                    return false;
                }
                s.IsDone = true;
                if (!string.IsNullOrEmpty(s.SaveId)) GameState.SetCollected(s.SaveId);
                Sfx.Play(SfxId.PrizeFanfare, 0.55f);
                FxBurst.Spawn(FxBurst.Kind.Sparkle, point, new Color(1f, 0.9f, 0.5f));
                ScoreBubbles.BurstAt(point, 12);
                if (s.Done != null) s.Done(true);
                return true;
            }
            return false;
        }
    }
}

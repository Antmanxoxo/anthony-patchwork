using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Sticker mode (Triangle / F): aim with the camera, pick a sticker with the D-pad, turn it with
    // L1 / R1, press X to slap it onto whatever's in the middle of the screen, Grab to peel one
    // off. Stickers on the level stay there (saved); ones on toys ride along until you leave.
    public class StickerSystem
    {
        const float StickerSize = 0.95f;
        const float Reach = 8f;

        readonly PatchworkGame game;
        readonly List<StickerId> owned = new List<StickerId>();
        readonly List<Placed> placed = new List<Placed>();
        Transform root;
        Transform ghost;
        Renderer ghostRenderer;
        MaterialPropertyBlock mpb;
        int layerCount;

        public int Selected { get; private set; }
        public float Spin { get; private set; }
        public bool HasAim { get; private set; }
        public Vector3 AimPoint { get; private set; }
        public IList<StickerId> Owned { get { return owned; } }
        public bool PeelTarget { get; private set; }

        Vector3 aimNormal;
        Transform aimParent;
        bool aimMoves;
        float openTime;

        class Placed
        {
            public Transform T;
            public GameState.StickerSave Save;
        }

        public StickerSystem(PatchworkGame game)
        {
            this.game = game;
            mpb = new MaterialPropertyBlock();
        }

        // Puts back every sticker saved for this level.
        public void LoadLevel(string levelId)
        {
            root = new GameObject("Stickers").transform;
            foreach (var s in GameState.StickersIn(levelId))
            {
                var t = Spawn((StickerId)s.id, s.pos, s.normal, s.spin, root);
                placed.Add(new Placed { T = t, Save = s });
            }
        }

        public void Open()
        {
            owned.Clear();
            foreach (StickerId id in System.Enum.GetValues(typeof(StickerId)))
                if (Prizes.OwnsSticker(id)) owned.Add(id);
            Selected = Mathf.Clamp(Selected, 0, Mathf.Max(0, owned.Count - 1));
            openTime = 0f;
            if (ghost == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Object.DestroyImmediate(go.GetComponent<Collider>());
                go.name = "StickerGhost";
                go.layer = 2;
                ghost = go.transform;
                ghost.localScale = Vector3.one * StickerSize;
                ghostRenderer = go.GetComponent<Renderer>();
                ghostRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            ghost.gameObject.SetActive(false);
        }

        public void Close()
        {
            if (ghost != null) ghost.gameObject.SetActive(false);
        }

        // Runs while sticker mode is open. Returns false when it should close.
        public bool Update(float udt)
        {
            openTime += udt;
            var input = game.Input;
            if (openTime > 0.1f && (input.Cancel.WasPressedThisFrame() || input.Sticker.WasPressedThisFrame())) return false;
            if (owned.Count == 0) return true;

            if (input.NavLeft.WasPressedThisFrame()) { Selected = (Selected + owned.Count - 1) % owned.Count; Sfx.Play(SfxId.MenuMove, 0.35f); }
            if (input.NavRight.WasPressedThisFrame()) { Selected = (Selected + 1) % owned.Count; Sfx.Play(SfxId.MenuMove, 0.35f); }
            if (input.RotateLeft.WasPressedThisFrame()) Spin += 30f;
            if (input.RotateRight.WasPressedThisFrame()) Spin -= 30f;

            Aim();
            Placed peel = HasAim ? Nearest(AimPoint, 0.55f) : null;
            PeelTarget = peel != null;

            if (HasAim && openTime > 0.15f && (input.Confirm.WasPressedThisFrame() || input.Ability.WasPressedThisFrame()))
                Place();
            else if (peel != null && input.Grab.WasPressedThisFrame())
                Peel(peel);
            return true;
        }

        void Aim()
        {
            var cam = game.CameraRig.Cam;
            var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;
            HasAim = false;
            if (Physics.Raycast(ray, out hit, 30f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
                && Vector3.Distance(hit.point, game.Player.Motor.Center) <= Reach)
            {
                HasAim = true;
                AimPoint = hit.point;
                aimNormal = hit.normal;
                aimMoves = hit.rigidbody != null;
                aimParent = hit.rigidbody != null ? hit.rigidbody.transform : null;
            }
            if (ghost == null) return;
            ghost.gameObject.SetActive(HasAim);
            if (!HasAim) return;
            var id = owned[Selected];
            ghostRenderer.sharedMaterial = PatchworkAssets.Sticker(StickerArt.Get(id));
            ghostRenderer.GetPropertyBlock(mpb);
            float pulse = 0.55f + Mathf.Sin(Time.unscaledTime * 6f) * 0.15f;
            mpb.SetColor("_BaseColor", new Color(1f, 1f, 1f, pulse));
            ghostRenderer.SetPropertyBlock(mpb);
            ghost.SetPositionAndRotation(AimPoint + aimNormal * 0.03f, Facing(aimNormal, Spin));
        }

        Quaternion Facing(Vector3 normal, float spin)
        {
            Vector3 up = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.9f ? Vector3.ProjectOnPlane(game.CameraRig.Cam.transform.forward, normal) : Vector3.up;
            if (up.sqrMagnitude < 0.001f) up = Vector3.forward;
            return Quaternion.LookRotation(-normal, up.normalized) * Quaternion.Euler(0f, 0f, spin);
        }

        Transform Spawn(StickerId id, Vector3 pos, Vector3 normal, float spin, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = "Sticker_" + id;
            var t = go.transform;
            layerCount = (layerCount + 1) % 12;
            t.SetPositionAndRotation(pos + normal * (0.015f + layerCount * 0.0015f), Facing(normal, spin));
            t.localScale = Vector3.one * StickerSize;
            t.SetParent(parent, true);
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = PatchworkAssets.Sticker(StickerArt.Get(id));
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return t;
        }

        void Place()
        {
            var id = owned[Selected];
            var parent = aimMoves && aimParent != null ? aimParent : root;
            var t = Spawn(id, AimPoint, aimNormal, Spin, parent);
            var p = new Placed { T = t };
            if (!aimMoves)
            {
                p.Save = new GameState.StickerSave { level = game.Level.Id, id = (int)id, pos = AimPoint, normal = aimNormal, spin = Spin };
                GameState.AddSticker(p.Save);
            }
            Merits.Add("stickers");
            placed.Add(p);
            // The save keeps the newest few dozen; drop the oldest one we're showing to match.
            int saved = 0;
            foreach (var q in placed) if (q.Save != null) saved++;
            if (saved > GameState.MaxStickersPerLevel)
                for (int i = 0; i < placed.Count; i++)
                    if (placed[i].Save != null) { if (placed[i].T != null) Object.Destroy(placed[i].T.gameObject); placed.RemoveAt(i); break; }
            Sfx.Play(SfxId.StickerSlap, 0.55f);
            Haptics.Pulse(0.15f, 0.1f, 0.05f);
            t.localScale *= 1.25f;
            game.StartCoroutine(Settle(t));
            StickerSwitch.TryStick(id, AimPoint);
        }

        System.Collections.IEnumerator Settle(Transform t)
        {
            Vector3 end = t.localScale / 1.25f;
            Vector3 start = t.localScale;
            for (float k = 0f; k < 1f; k += Time.unscaledDeltaTime / 0.12f)
            {
                if (t == null) yield break;
                t.localScale = Vector3.Lerp(start, end, k);
                yield return null;
            }
            if (t != null) t.localScale = end;
        }

        Placed Nearest(Vector3 point, float within)
        {
            Placed best = null;
            float bd = within;
            foreach (var p in placed)
            {
                if (p.T == null) continue;
                float d = Vector3.Distance(p.T.position, point);
                if (d < bd) { bd = d; best = p; }
            }
            return best;
        }

        void Peel(Placed p)
        {
            if (p.Save != null) GameState.RemoveSticker(p.Save);
            placed.Remove(p);
            if (p.T != null) Object.Destroy(p.T.gameObject);
            Sfx.Play(SfxId.StickerPeel, 0.5f);
        }
    }
}

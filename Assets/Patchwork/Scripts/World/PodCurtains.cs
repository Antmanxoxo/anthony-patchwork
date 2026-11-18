using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The Pod's curtains: two pleated fabric drapes hanging from a wooden rod across the top of
    // the window. Every time you come into the Pod (starting the game, or coming home from a
    // level) they're drawn shut over the glass. The first time you pick up the controller to
    // choose a level they sweep open, gather at the sides of the window and are tied back, and
    // they stay open for the rest of your visit.
    public class PodCurtains : MonoBehaviour
    {
        struct Pose { public Vector3 Pos; public Quaternion Rot; public Vector3 Scale; }

        readonly List<Transform> pleats = new List<Transform>();
        readonly List<Pose> shut = new List<Pose>(), open = new List<Pose>();
        readonly List<GameObject> ties = new List<GameObject>();
        float t;            // 0 shut, 1 open
        bool opening;

        const int PerSide = 13;
        // How far in from the glass they hang: close at the bottom (behind the things on the
        // desk) and further in at the top (in front of the felt piping over the window).
        static float Off(float y) { return Mathf.Lerp(0.2f, 0.52f, Mathf.InverseLerp(1.3f, 6.8f, y)); }

        static Vector3 Dir(float deg) { float a = deg * Mathf.Deg2Rad; return new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)); }
        static Vector3 On(float deg, float y) { return Dir(deg) * (HubBuilder.RadiusAt(y) - Off(y)) + Vector3.up * y; }

        public static void Build(LevelKit kit, float halfAngle, float deskTop, float windowTop)
        {
            var root = kit.Group("Curtains", Vector3.zero);
            var c = root.gameObject.AddComponent<PodCurtains>();
            float bottom = deskTop + 0.08f, mid = 4f, top = windowTop - 0.38f;
            Color[] cloth = { new Color(0.8f, 0.32f, 0.42f), new Color(0.68f, 0.25f, 0.35f) };
            var mats = new[] { PatchworkAssets.Fabric(cloth[0]), PatchworkAssets.Fabric(cloth[1]) };
            float edge = halfAngle + 1f;
            for (int s = -1; s <= 1; s += 2)
            {
                for (int i = 0; i < PerSide; i++)
                {
                    // Shut: the pleats spread from the middle of the window out to its edge.
                    float aShut = s * ((i + 0.5f) / PerSide * edge);
                    // Open: bunched up at the window's edge, each fold close behind the last.
                    float aOpen = s * (edge - 3.2f + (i + 0.5f) / PerSide * 3.4f);
                    float slice = edge / PerSide;
                    bool fold = i % 2 == 1;
                    foreach (var seg in new[] { new Vector2(bottom, mid), new Vector2(mid, top) })
                    {
                        var piece = kit.Deco(PrimitiveType.Cube, Vector3.zero, Vector3.one, mats[fold ? 1 : 0], default(Vector3), root);
                        piece.name = "CurtainPleat";
                        piece.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        c.pleats.Add(piece);
                        c.shut.Add(PoseFor(aShut, seg.x, seg.y, slice, fold, 1f, s));
                        c.open.Add(PoseFor(aOpen, seg.x, seg.y, slice, fold, 0.26f, s));
                    }
                }
                // The tie-back, round the gathered curtain (shown once it's open).
                float aTie = s * (edge - 1.5f);
                float yTie = 3.7f;
                var tie = kit.Group("CurtainTie", On(aTie, yTie) + Vector3.zero);
                tie.rotation = Quaternion.LookRotation(-Dir(aTie), Vector3.up);
                kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.95f, 0.06f, 0.42f), PatchworkAssets.Felt(new Color(1f, 0.82f, 0.35f)), default(Vector3), tie);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, 0.2f), new Vector3(0.22f, 0.22f, 0.12f), PatchworkAssets.Felt(new Color(0.98f, 0.7f, 0.3f)), default(Vector3), tie);
                tie.gameObject.SetActive(false);
                c.ties.Add(tie.gameObject);
            }
            // The rod across the top, in short straight lengths round the curve, with a
            // wooden knob at each end.
            var wood = PatchworkAssets.Wood(new Color(0.62f, 0.42f, 0.28f));
            float rodY = top + 0.06f;
            const int rodBits = 16;
            for (int k = 0; k < rodBits; k++)
            {
                float a0 = Mathf.Lerp(-edge - 2f, edge + 2f, k / (float)rodBits), a1 = Mathf.Lerp(-edge - 2f, edge + 2f, (k + 1) / (float)rodBits);
                Vector3 p0 = On(a0, rodY), p1 = On(a1, rodY);
                var rod = kit.Deco(PrimitiveType.Cylinder, (p0 + p1) * 0.5f, new Vector3(0.09f, (p1 - p0).magnitude * 0.52f, 0.09f), wood, default(Vector3), root);
                rod.rotation = Quaternion.FromToRotation(Vector3.up, (p1 - p0).normalized);
            }
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Sphere, On(s * (edge + 2.2f), rodY), Vector3.one * 0.2f, wood, default(Vector3), root);
            c.Apply();
        }

        // One fold of curtain from height y0 to y1 at angle deg, hanging just in from the glass.
        static Pose PoseFor(float deg, float y0, float y1, float slice, bool fold, float bunch, int side)
        {
            float inset = fold ? 0.07f : 0f;
            Vector3 a = On(deg, y0), b = On(deg, y1);
            Vector3 n = -Dir(deg);
            a += n * inset; b += n * inset;
            Vector3 up = (b - a).normalized;
            // Each fold is turned a little, alternately, so the drape ripples.
            Quaternion rot = Quaternion.LookRotation(n, up) * Quaternion.Euler(0f, (fold ? 18f : -18f) * side * (bunch < 1f ? 2.2f : 1f), 0f);
            float r = HubBuilder.RadiusAt((y0 + y1) * 0.5f) - Off((y0 + y1) * 0.5f);
            float wide = 2f * r * Mathf.Tan(slice * 0.5f * Mathf.Deg2Rad) * 1.18f;
            return new Pose { Pos = (a + b) * 0.5f, Rot = rot, Scale = new Vector3(wide * bunch, (b - a).magnitude + 0.03f, 0.05f) };
        }

        void Apply()
        {
            float k = t * t * (3f - 2f * t);
            for (int i = 0; i < pleats.Count; i++)
            {
                var p = pleats[i];
                if (p == null) continue;
                var a = shut[i]; var b = open[i];
                p.position = Vector3.Lerp(a.Pos, b.Pos, k);
                p.rotation = Quaternion.Slerp(a.Rot, b.Rot, k);
                p.localScale = Vector3.Lerp(a.Scale, b.Scale, k);
            }
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (!opening && game != null && game.Mode == UIMode.LevelSelect)
            {
                opening = true;
                Sfx.Play(SfxId.Whoosh, 0.35f);
            }
            if (!opening || t >= 1f) return;
            t = Mathf.MoveTowards(t, 1f, Time.unscaledDeltaTime / 1.3f);
            Apply();
            if (t >= 1f) foreach (var g in ties) if (g != null) g.SetActive(true);
        }
    }
}

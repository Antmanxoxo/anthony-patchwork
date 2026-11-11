using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // A little floating score bubble. Touch it to pop it for points; grab them one after another
    // to build a chain. Bubbles drift towards Patch when he's close, so a trail feels like
    // hoovering them up.
    public class ScoreBubble : PlayerVolume
    {
        public int Value = 10;
        public bool FromBurst;
        internal Vector3 Home;
        internal float Phase;
        internal Vector3 Velocity;
        internal float Age;
        internal Transform Visual;
        internal Color Tint;
        internal bool Pulled;
        internal int Set = -1;     // the trail or ring it belongs to (-1: none)

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            var game = PatchworkGame.Instance;
            Vector3 at = transform.position;
            if (game != null && game.Score != null) game.Score.AddChained(Value, at);
            var all = ScoreBubbles.Instance;
            if (all != null) all.PopAt(at, Tint);
            gameObject.SetActive(false);
            // A gold bubble is a pinata: it bursts into a shower of little ones.
            if (Value >= 100 && !FromBurst && all != null)
            {
                all.Burst(at, 6);
                FxBurst.Spawn(FxBurst.Kind.Sparkle, at, new Color(1f, 0.85f, 0.4f));
                Sfx.Play(SfxId.Pop, 0.5f);
            }
            if (Set >= 0 && all != null) all.SetPopped(Set, at);
        }
    }

    // Moves every score bubble (bobbing, being pulled in, flying out of bursts) from one Update,
    // keeps a pool for bursts, and plays the little pop rings.
    public class ScoreBubbles : MonoBehaviour
    {
        public static ScoreBubbles Instance { get; private set; }

        readonly List<ScoreBubble> all = new List<ScoreBubble>();
        readonly List<ScoreBubble> pool = new List<ScoreBubble>();
        readonly List<Transform> rings = new List<Transform>();
        readonly List<float> ringAge = new List<float>();
        readonly List<Color> ringColor = new List<Color>();
        MaterialPropertyBlock mpb;
        LevelKit kit;

        const float MagnetRadius = 2.6f;

        // Trails and rings: bubbles placed between BeginSet and EndSet belong together, and
        // getting every one of them pays a bonus (a "perfect trail").
        static int placingSet = -1;
        readonly List<int> setSize = new List<int>(), setLeft = new List<int>();

        public static void BeginSet(LevelKit kit)
        {
            var m = Ensure(kit);
            placingSet = m.setSize.Count;
            m.setSize.Add(0);
            m.setLeft.Add(0);
        }

        public static void EndSet() { placingSet = -1; }

        public void SetPopped(int set, Vector3 at)
        {
            if (set < 0 || set >= setLeft.Count || setLeft[set] <= 0) return;
            setLeft[set]--;
            if (setLeft[set] > 0 || setSize[set] < 4) return;
            int bonus = setSize[set] * 25;
            ScoreKeeper.Bonus(bonus, at + Vector3.up * 0.8f);
            var game = PatchworkGame.Instance;
            if (game != null && game.Score != null) game.Score.Shout(setSize[set] >= 10 ? "PERFECT RING!" : "PERFECT TRAIL!", new Color(0.55f, 0.9f, 1f));
            FxBurst.Spawn(FxBurst.Kind.Sparkle, at, new Color(0.7f, 0.9f, 1f));
            FxBurst.Spawn(FxBurst.Kind.Pompoms, at, new Color(0.6f, 0.85f, 1f));
            Sfx.PlayPitched(SfxId.Chime, 0.4f, 1.5f);
            Sfx.PlayPitched(SfxId.Chime, 0.3f, 2f);
        }

        public static ScoreBubbles Ensure(LevelKit kit)
        {
            if (Instance != null && Instance.kit == kit) return Instance;
            var go = new GameObject("ScoreBubbles");
            go.transform.SetParent(kit.Root, false);
            var m = go.AddComponent<ScoreBubbles>();
            m.kit = kit;
            Instance = m;
            m.mpb = new MaterialPropertyBlock();
            for (int i = 0; i < 40; i++) m.pool.Add(m.Make(Vector3.zero, 10, true));
            var ringMat = PatchworkAssets.Bubble(new Color(1f, 1f, 1f, 0.5f));
            for (int i = 0; i < 10; i++)
            {
                var r = kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one, ringMat, default(Vector3), go.transform);
                r.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.gameObject.SetActive(false);
                m.rings.Add(r);
                m.ringAge.Add(9f);
                m.ringColor.Add(Color.white);
            }
            return m;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // A bubble placed in the level. 10 = small blue, 50 = big pink, 100 = gold.
        public static ScoreBubble Place(LevelKit kit, Vector3 pos, int value = 10)
        {
            var m = Ensure(kit);
            var b = m.Make(pos, value, false);
            kit.Info.ScorePossible += value;
            if (placingSet >= 0 && placingSet < m.setSize.Count)
            {
                b.Set = placingSet;
                m.setSize[placingSet]++;
                m.setLeft[placingSet]++;
            }
            return b;
        }

        ScoreBubble Make(Vector3 pos, int value, bool forPool)
        {
            float r = value >= 50 ? 0.5f : 0.33f;
            var b = kit.Volume<ScoreBubble>(pos, Vector3.one * (value >= 50 ? 1.7f : 1.35f), value >= 100 ? "GoldBubble" : "ScoreBubble");
            b.transform.SetParent(transform, true);
            b.Value = value;
            b.FromBurst = forPool;
            b.Home = pos;
            b.Phase = (pos.x * 0.37f + pos.z * 0.61f) % 6.28f;
            Color tint = value >= 100 ? new Color(1f, 0.82f, 0.3f) : value >= 50 ? new Color(1f, 0.62f, 0.78f) : new Color(0.6f, 0.85f, 1f);
            b.Tint = tint;
            var vis = new GameObject("Visual").transform;
            vis.SetParent(b.transform, false);
            b.Visual = vis;
            var skin = value >= 100 ? PatchworkAssets.Glow(tint, 0.9f) : PatchworkAssets.Bubble(new Color(tint.r, tint.g, tint.b, 0.55f));
            var shell = kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * r * 2f, skin, default(Vector3), vis);
            shell.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var core = kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * r * 0.9f, PatchworkAssets.Glow(tint, 1.2f), default(Vector3), vis);
            core.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var shine = kit.Deco(PrimitiveType.Sphere, new Vector3(-r * 0.4f, r * 0.45f, -r * 0.4f), new Vector3(r * 0.4f, r * 0.25f, r * 0.12f), PatchworkAssets.Glow(Color.white, 1f), new Vector3(0f, 0f, 35f), vis);
            shine.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            if (forPool) b.gameObject.SetActive(false);
            else all.Add(b);
            return b;
        }

        // Throws a handful of bubbles out of a spot; they fly back to Patch on their own.
        public void Burst(Vector3 at, int count, int value = 10)
        {
            for (int n = 0; n < count; n++)
            {
                ScoreBubble b = null;
                foreach (var p in pool) if (!p.gameObject.activeSelf) { b = p; break; }
                if (b == null)
                {
                    b = Make(at, value, true);
                    pool.Add(b);
                }
                b.Value = value;
                b.transform.position = at;
                b.Age = 0f;
                float a = Random.value * Mathf.PI * 2f;
                b.Velocity = new Vector3(Mathf.Cos(a) * 4.5f, 5f + Random.value * 3f, Mathf.Sin(a) * 4.5f);
                b.gameObject.SetActive(true);
            }
        }

        public static void BurstAt(Vector3 at, int count, int value = 10)
        {
            if (Instance != null) Instance.Burst(at, count, value);
        }

        public void PopAt(Vector3 at, Color c)
        {
            int best = 0;
            for (int i = 1; i < rings.Count; i++) if (ringAge[i] > ringAge[best]) best = i;
            if (rings.Count == 0) return;
            rings[best].position = at;
            rings[best].gameObject.SetActive(true);
            ringAge[best] = 0f;
            ringColor[best] = c;
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) return;
            float dt = Time.deltaTime;
            float t = Time.time;
            Vector3 target = game.Player.Motor.Center;

            for (int i = 0; i < all.Count; i++)
            {
                var b = all[i];
                if (b == null || !b.gameObject.activeSelf) continue;
                Vector3 p = b.transform.position;
                Vector3 to = target - p;
                if (b.Pulled || to.sqrMagnitude < MagnetRadius * MagnetRadius)
                {
                    b.Pulled = true;
                    b.Age += dt;
                    b.transform.position = Vector3.MoveTowards(p, target, (6f + b.Age * 30f) * dt);
                }
                else
                {
                    b.transform.position = b.Home + Vector3.up * Mathf.Sin(t * 2.2f + b.Phase) * 0.14f;
                }
                Wobble(b, t);
            }

            foreach (var b in pool)
            {
                if (!b.gameObject.activeSelf) continue;
                b.Age += dt;
                Vector3 p = b.transform.position;
                if (b.Age < 0.45f)
                {
                    b.Velocity *= Mathf.Exp(-3f * dt);
                    b.transform.position = p + b.Velocity * dt;
                }
                else
                {
                    b.transform.position = Vector3.MoveTowards(p, target, (8f + (b.Age - 0.45f) * 30f) * dt);
                }
                Wobble(b, t);
            }

            for (int i = 0; i < rings.Count; i++)
            {
                if (ringAge[i] > 0.3f) continue;
                ringAge[i] += dt;
                float k = Mathf.Clamp01(ringAge[i] / 0.3f);
                rings[i].localScale = Vector3.one * Mathf.Lerp(0.4f, 1.6f, k);
                Color c = ringColor[i];
                c.a = 0.5f * (1f - k);
                var r = rings[i].GetComponent<Renderer>();
                r.GetPropertyBlock(mpb);
                mpb.SetColor("_BaseColor", c);
                r.SetPropertyBlock(mpb);
                if (ringAge[i] > 0.3f) rings[i].gameObject.SetActive(false);
            }
        }

        // ----- Settling: nothing may sit inside a wall, a roof or a prop -----

        // How many bubbles/prizes had to move, and how many were dropped (for the editor's level check).
        public static int Moved { get; private set; }
        public static int Dropped { get; private set; }
        public static readonly List<string> Report = new List<string>();

        // Checks every score bubble, prize bubble and marble against everything solid and every
        // prop's bounds, and nudges it up or sideways until it's clear. Bubbles that can't be
        // freed are removed. Run once the whole level is built.
        public static void Settle(LevelKit kit)
        {
            Physics.SyncTransforms();
            Moved = 0;
            Dropped = 0;
            Report.Clear();
            var props = new List<Bounds>();
            foreach (var r in kit.Root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
                if (r.GetComponentInParent<ScoreBubble>() != null || r.GetComponentInParent<PrizeBubble>() != null
                    || r.GetComponentInParent<SecretMarble>() != null || r.GetComponentInParent<ScoreBubbles>() != null) continue;
                var b = r.bounds;
                if (b.size.x > 9f || b.size.z > 9f || b.size.y > 12f) continue;     // ground, walls, merged batches: their colliders count instead
                var m = r.sharedMaterial;
                if (m != null && m.renderQueue >= 3000) continue;                     // see-through things (water, glass)
                props.Add(b);
            }
            if (Instance != null)
            {
                foreach (var s in Instance.all)
                {
                    if (s == null) continue;
                    float rad = s.Value >= 50 ? 0.5f : 0.33f;
                    Vector3 p;
                    if (FindClear(s.Home, rad, props, true, out p))
                    {
                        if ((p - s.Home).sqrMagnitude > 0.0001f) { Moved++; Note("bubble", s.Home, p); }
                        s.Home = p;
                        s.transform.position = p;
                    }
                    else
                    {
                        Dropped++;
                        if (s.Set >= 0 && s.Set < Instance.setLeft.Count) { Instance.setLeft[s.Set]--; Instance.setSize[s.Set]--; }
                        Note("bubble dropped", s.Home, s.Home);
                        kit.Info.ScorePossible -= s.Value;
                        s.gameObject.SetActive(false);
                    }
                }
            }
            foreach (var pb in kit.Root.GetComponentsInChildren<PrizeBubble>(true))
            {
                Vector3 p;
                if (FindClear(pb.transform.position, 0.9f, props, false, out p) && (p - pb.transform.position).sqrMagnitude > 0.0001f)
                {
                    Moved++;
                    Note("prize " + pb.PrizeId, pb.transform.position, p);
                    pb.MoveHome(p);
                }
            }
            foreach (var mb in kit.Root.GetComponentsInChildren<SecretMarble>(true))
            {
                Vector3 p;
                if (FindClear(mb.transform.position, 0.45f, props, false, out p) && (p - mb.transform.position).sqrMagnitude > 0.0001f)
                {
                    Moved++;
                    Note("marble " + mb.Id, mb.transform.position, p);
                    mb.MoveHome(p);
                }
            }
        }

        static void Note(string what, Vector3 from, Vector3 to)
        {
            if (Report.Count < 80)
                Report.Add(what + " " + from.ToString("F1") + " -> " + to.ToString("F1"));
        }

        static readonly Vector3[] Nudges =
        {
            Vector3.zero, Vector3.up * 0.4f, Vector3.up * 0.8f, Vector3.up * 1.2f, Vector3.up * 1.6f, Vector3.up * 2f,
            Vector3.right, Vector3.left, Vector3.forward, Vector3.back,
            new Vector3(1.5f, 0.4f, 0f), new Vector3(-1.5f, 0.4f, 0f), new Vector3(0f, 0.4f, 1.5f), new Vector3(0f, 0.4f, -1.5f),
            Vector3.up * 2.8f, Vector3.up * 3.6f
        };

        static bool FindClear(Vector3 start, float rad, List<Bounds> props, bool allowSide, out Vector3 result)
        {
            for (int i = 0; i < Nudges.Length; i++)
            {
                Vector3 n = Nudges[i];
                if (!allowSide && (n.x != 0f || n.z != 0f)) continue;
                Vector3 q = start + n;
                if (Clear(q, rad, props)) { result = q; return true; }
            }
            result = start;
            return false;
        }

        static bool Clear(Vector3 p, float rad, List<Bounds> props)
        {
            float r = rad + 0.05f;
            if (Physics.CheckSphere(p, r, ~0, QueryTriggerInteraction.Ignore)) return false;
            float r2 = r * r;
            for (int i = 0; i < props.Count; i++)
                if (props[i].SqrDistance(p) < r2) return false;
            return true;
        }

        static void Wobble(ScoreBubble b, float t)
        {
            float w = Mathf.Sin(t * 5f + b.Phase) * 0.06f;
            b.Visual.localScale = new Vector3(1f + w, 1f - w, 1f + w);
        }
    }
}

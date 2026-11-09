using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Puffball dandelions: tall fluffy seed heads growing in the meadows. Run or jump through
    // one and it bursts into drifting fluff with a soft chime; burst several in a row and the
    // chime climbs the scale, with a sparkle on every fifth. They grow back after a while.
    // One of these looks after every puffball in the level.
    public class Puffballs : MonoBehaviour
    {
        class Puff
        {
            public Transform Root, Head;
            public float Height, Gone, Sway;
        }

        readonly List<Puff> puffs = new List<Puff>();
        int chain;
        float chainTimer;
        static readonly float[] Scale = { 1f, 1.122f, 1.26f, 1.498f, 1.682f, 2f, 2.245f, 2.52f };

        public static Puffballs Ensure(LevelKit kit)
        {
            var p = kit.Root.GetComponent<Puffballs>();
            return p != null ? p : kit.Root.gameObject.AddComponent<Puffballs>();
        }

        public void Add(LevelKit kit, Vector3 feet, float height)
        {
            var g = kit.Group("Puffball", feet);
            var stem = PatchworkAssets.Felt(new Color(0.62f, 0.8f, 0.5f));
            var fluff = PatchworkAssets.Felt(new Color(0.99f, 0.98f, 0.94f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, height * 0.5f, 0f), new Vector3(0.05f, height * 0.5f, 0.05f), stem, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.1f, 0.06f, 0f), new Vector3(0.34f, 0.07f, 0.16f), stem, new Vector3(0f, 30f, 0f), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(-0.09f, 0.06f, 0.05f), new Vector3(0.3f, 0.07f, 0.15f), stem, new Vector3(0f, -50f, 0f), g);
            var head = kit.Group("Head", Vector3.zero);
            head.SetParent(g, false);
            head.localPosition = new Vector3(0f, height, 0f);
            NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.5f, fluff, default(Vector3), head));
            for (int i = 0; i < 7; i++)
            {
                Vector3 d = Random.onUnitSphere;
                NoShadow(kit.Deco(PrimitiveType.Sphere, d * 0.2f, Vector3.one * (0.16f + (i % 3) * 0.04f), fluff, default(Vector3), head));
            }
            puffs.Add(new Puff { Root = g, Head = head, Height = height, Sway = Random.value * 10f });
        }

        static void NoShadow(Transform t)
        {
            t.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null || game.Mode != UIMode.Gameplay) return;
            float dt = Time.deltaTime;
            chainTimer -= dt;
            if (chainTimer <= 0f) chain = 0;
            Vector3 p = game.Player.Motor.Center;
            for (int i = 0; i < puffs.Count; i++)
            {
                var pf = puffs[i];
                if (pf.Root == null) continue;
                Vector3 at = pf.Head.position;
                float dx = at.x - p.x, dz = at.z - p.z;
                float flat = dx * dx + dz * dz;
                if (flat > 40f * 40f) continue;
                if (pf.Gone > 0f)
                {
                    // Growing back: nothing for a while, then the head swells out again.
                    pf.Gone -= dt;
                    float back = Mathf.Clamp01(1f - pf.Gone / 3f);
                    pf.Head.localScale = Vector3.one * (pf.Gone > 3f ? 0f : UIKit.EaseOutBack(back));
                    continue;
                }
                // A gentle sway in the breeze.
                pf.Sway += dt;
                pf.Root.localRotation = Quaternion.Euler(Mathf.Sin(pf.Sway * 1.1f) * 4f, 0f, Mathf.Cos(pf.Sway * 0.8f) * 4f);
                if (flat < 0.75f * 0.75f && Mathf.Abs(at.y - p.y) < 1.3f)
                {
                    pf.Gone = 22f;
                    pf.Head.localScale = Vector3.zero;
                    chain = chainTimer > 0f ? chain + 1 : 0;
                    chainTimer = 2.5f;
                    Vector3 v = game.Player.Motor.Velocity;
                    FxBurst.Spawn(FxBurst.Kind.Steam, at + Vector3.down * 0.3f, new Color(0.99f, 0.98f, 0.94f));
                    FxBurst.Spawn(FxBurst.Kind.Puff, at, new Color(0.99f, 0.98f, 0.94f), new Vector3(v.x, 2f, v.z));
                    Sfx.PlayPitched(SfxId.Chime, 0.32f, Scale[Mathf.Min(chain, Scale.Length - 1)]);
                    if (chain > 0 && chain % 5 == 4)
                    {
                        FxBurst.Spawn(FxBurst.Kind.Sparkle, at, new Color(1f, 0.85f, 0.4f));
                        Sfx.Play(SfxId.ScoreBubble, 0.35f);
                    }
                    Haptics.Pulse(0.1f, 0.1f, 0.05f);
                }
            }
        }
    }

    // Soft pompom puffs of smoke drifting up out of a chimney, one after another: a sign that
    // somebody's home. Only moves while Patch is near enough to see it.
    public class ChimneySmoke : MonoBehaviour
    {
        const int Count = 5;
        const float Life = 4.5f;
        Transform[] puff;
        float[] age;
        float[] drift;

        public static void Add(LevelKit kit, Vector3 top)
        {
            var g = kit.Group("ChimneySmoke", top);
            var s = g.gameObject.AddComponent<ChimneySmoke>();
            s.puff = new Transform[Count];
            s.age = new float[Count];
            s.drift = new float[Count];
            var m = PatchworkAssets.Felt(new Color(0.97f, 0.95f, 0.93f));
            for (int i = 0; i < Count; i++)
            {
                s.puff[i] = kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.zero, m, default(Vector3), g);
                var r = s.puff[i].GetComponent<Renderer>();
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
                s.age[i] = i * (Life / Count);
                s.drift[i] = Random.value * 6.28f;
            }
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null) return;
            if ((game.Player.transform.position - transform.position).sqrMagnitude > 70f * 70f) return;
            float dt = Time.deltaTime;
            for (int i = 0; i < Count; i++)
            {
                age[i] += dt;
                if (age[i] > Life) { age[i] -= Life; drift[i] = Random.value * 6.28f; }
                float k = age[i] / Life;
                // Rises, swells, wanders sideways a little, then thins away.
                float size = Mathf.Sin(Mathf.Clamp01(k * 1.15f) * Mathf.PI) * (0.5f + k * 0.7f);
                puff[i].localPosition = new Vector3(Mathf.Sin(drift[i] + k * 2.2f) * 0.5f * k + k * 0.9f, k * 3.6f, Mathf.Cos(drift[i] * 1.3f + k * 1.7f) * 0.4f * k);
                puff[i].localScale = Vector3.one * Mathf.Max(0f, size);
            }
        }
    }
}

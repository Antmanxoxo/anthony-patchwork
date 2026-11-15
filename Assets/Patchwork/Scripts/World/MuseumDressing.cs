using UnityEngine;

namespace Patchwork
{
    // Things that dress the Oceanic Museum for the night the Fray got in, and change as Patch
    // puts it right: patches of grey creep that clear away, and the night lamps on Wick's round.

    // A patch of creep: where the Fray has been, the floor goes the colour of old dishwater,
    // loose threads stand up out of it like grass, and a few long ones feel about in the air.
    // It does nothing but look bad. When whatever it belongs to is beaten, it shrivels and goes.
    public class FrayCreep : MonoBehaviour
    {
        Transform[] feelers;
        float t, going = -1f;

        public static FrayCreep Make(Transform parent, Vector3 ground, float radius, float seed)
        {
            // Only where there's floor to put it on.
            RaycastHit hit;
            if (!Physics.Raycast(ground + Vector3.up * 1.5f, Vector3.down, out hit, 3.5f, ~(1 << 2), QueryTriggerInteraction.Ignore)) return null;
            var go = new GameObject("FrayCreep");
            go.transform.SetParent(parent, false);
            go.transform.position = hit.point;
            var c = go.AddComponent<FrayCreep>();
            c.t = seed;
            var blot = PatchworkAssets.Plain(new Color(0.34f, 0.32f, 0.38f), 0.15f);
            var dark = PatchworkAssets.Plain(new Color(0.2f, 0.18f, 0.25f), 0.15f);
            int blobs = 4 + Mathf.RoundToInt(radius);
            for (int i = 0; i < blobs; i++)
            {
                float a = seed * 1.3f + i * 2.4f;
                float d = i == 0 ? 0f : radius * (0.25f + 0.5f * Mathf.Abs(Mathf.Sin(seed + i * 1.9f)));
                float s = radius * (i == 0 ? 1.1f : 0.45f + 0.35f * Mathf.Abs(Mathf.Cos(seed * 2f + i)));
                var b = Monsters.Part(PrimitiveType.Sphere, go.transform, new Vector3(Mathf.Cos(a) * d, 0.025f + i * 0.004f, Mathf.Sin(a) * d),
                    new Vector3(s, 0.02f, s * (0.7f + 0.3f * Mathf.Sin(i + seed))), i % 3 == 2 ? dark : blot, new Vector3(0f, a * 57f, 0f));
                b.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            // Loose threads standing up out of it.
            int tufts = 6 + Mathf.RoundToInt(radius * 3f);
            for (int i = 0; i < tufts; i++)
            {
                float a = seed + i * 1.71f;
                float d = radius * 0.5f * Mathf.Sqrt((i + 0.5f) / tufts) * 1.6f;
                float h = 0.12f + 0.16f * Mathf.Abs(Mathf.Sin(seed * 3f + i * 2.3f));
                Monsters.Part(PrimitiveType.Capsule, go.transform, new Vector3(Mathf.Cos(a) * d, h, Mathf.Sin(a) * d), new Vector3(0.035f, h, 0.035f),
                    i % 2 == 0 ? Monsters.Grey : Monsters.Dark, new Vector3(Mathf.Sin(a * 3f) * 22f, a * 57f, Mathf.Cos(a * 2f) * 22f));
            }
            // And two or three long ones, feeling about.
            int n = radius > 1.6f ? 3 : 2;
            c.feelers = new Transform[n];
            for (int i = 0; i < n; i++)
            {
                float a = seed * 0.7f + i * 2.1f;
                var f = new GameObject("Feeler").transform;
                f.SetParent(go.transform, false);
                f.localPosition = new Vector3(Mathf.Cos(a) * radius * 0.3f, 0f, Mathf.Sin(a) * radius * 0.3f);
                float len = 0.5f + 0.25f * i;
                Monsters.Part(PrimitiveType.Capsule, f, new Vector3(0f, len, 0f), new Vector3(0.05f, len, 0.05f), Monsters.Grey);
                Monsters.Part(PrimitiveType.Sphere, f, new Vector3(0f, len * 2f, 0f), Vector3.one * 0.09f, Monsters.Glow);
                c.feelers[i] = f;
            }
            return c;
        }

        // Shrivel up and go (with a little colour where it was).
        public void Clear()
        {
            if (going >= 0f) return;
            going = 0f;
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position + Vector3.up * 0.4f, new Color(1f, 0.9f, 0.6f));
        }

        void Update()
        {
            float dt = Time.deltaTime;
            t += dt;
            for (int i = 0; i < feelers.Length; i++)
                feelers[i].localRotation = Quaternion.Euler(Mathf.Sin(t * 1.3f + i * 2f) * 24f, 0f, Mathf.Cos(t * 1.1f + i * 1.4f) * 24f);
            if (going < 0f) return;
            going += dt / 0.9f;
            float k = Mathf.Clamp01(going);
            transform.localScale = new Vector3(1f - k, Mathf.Max(0.01f, 1f - k * 1.4f), 1f - k);
            if (k >= 1f) Destroy(gameObject);
        }
    }

    // One of the night lamps on Wick's round: an iron post with a glass lantern on a hook.
    // They all went out at dusk. Walk up to one and Patch turns it up again: a pool of warm
    // light, and a heart back the first time.
    public class NightLamp : PlayerVolume
    {
        public System.Action<NightLamp> Lit;
        public bool IsLit { get; private set; }
        Light lamp;
        Transform flame;
        Transform[] moths;
        float t, glow;

        public static NightLamp Create(LevelKit kit, Vector3 foot, Material iron, Material brass)
        {
            var g = kit.Group("NightLamp", foot);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.06f, 0f), new Vector3(0.6f, 0.06f, 0.6f), iron, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.3f, 0f), new Vector3(0.1f, 1.3f, 0.1f), iron, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 2.62f, 0f), Vector3.one * 0.18f, brass, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.3f, 2.5f, 0f), new Vector3(0.62f, 0.06f, 0.06f), iron, new Vector3(0f, 0f, 8f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.56f, 2.38f, 0f), new Vector3(0.015f, 0.12f, 0.015f), iron, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.56f, 2.24f, 0f), new Vector3(0.34f, 0.03f, 0.34f), brass, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.56f, 1.82f, 0f), new Vector3(0.34f, 0.03f, 0.34f), brass, default(Vector3), g);
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI / 2f + 0.78f;
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0.56f + Mathf.Cos(a) * 0.15f, 2.03f, Mathf.Sin(a) * 0.15f), new Vector3(0.02f, 0.2f, 0.02f), brass, default(Vector3), g);
            }
            var glass = kit.Deco(PrimitiveType.Cylinder, new Vector3(0.56f, 2.03f, 0f), new Vector3(0.28f, 0.19f, 0.28f), PatchworkAssets.Bubble(new Color(0.75f, 0.8f, 0.9f, 0.3f)), default(Vector3), g);
            glass.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var l = g.gameObject.AddComponent<NightLamp>();
            l.Size = new Vector3(2.4f, 3.4f, 2.4f);
            l.flame = kit.Deco(PrimitiveType.Sphere, new Vector3(0.56f, 2f, 0f), new Vector3(0.14f, 0.22f, 0.14f), PatchworkAssets.Glow(new Color(1f, 0.8f, 0.4f), 3f), default(Vector3), g);
            l.flame.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            l.flame.gameObject.SetActive(false);
            var lg = new GameObject("Glow");
            lg.transform.SetParent(g, false);
            lg.transform.localPosition = new Vector3(0.56f, 2f, 0f);
            l.lamp = lg.AddComponent<Light>();
            l.lamp.type = LightType.Point;
            l.lamp.color = new Color(1f, 0.78f, 0.45f);
            l.lamp.range = 9f;
            l.lamp.intensity = 0f;
            l.lamp.shadows = LightShadows.None;
            l.lamp.enabled = false;
            // Two little moths that find it once it's burning (the ordinary, harmless kind).
            l.moths = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                var m = new GameObject("Moth").transform;
                m.SetParent(g, false);
                var cream = PatchworkAssets.Felt(new Color(0.93f, 0.89f, 0.78f));
                Monsters.Part(PrimitiveType.Sphere, m, Vector3.zero, new Vector3(0.05f, 0.05f, 0.1f), cream);
                Monsters.Part(PrimitiveType.Cube, m, new Vector3(-0.07f, 0.01f, 0f), new Vector3(0.12f, 0.01f, 0.09f), cream, new Vector3(0f, 0f, 20f));
                Monsters.Part(PrimitiveType.Cube, m, new Vector3(0.07f, 0.01f, 0f), new Vector3(0.12f, 0.01f, 0.09f), cream, new Vector3(0f, 0f, -20f));
                m.gameObject.SetActive(false);
                l.moths[i] = m;
            }
            l.t = foot.x + foot.z;
            return l;
        }

        // Already burning (from an earlier visit).
        public void SetLit()
        {
            IsLit = true;
            glow = 1f;
            flame.gameObject.SetActive(true);
            lamp.enabled = true;
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            if (IsLit) return;
            SetLit();
            glow = 0f;
            Sfx.Play(SfxId.Lever, 0.4f);
            Sfx.Play(SfxId.Checkpoint, 0.3f);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, flame.position, new Color(1f, 0.85f, 0.5f));
            if (player.Game != null) player.Game.Heal(1);
            if (Lit != null) Lit(this);
        }

        void Update()
        {
            if (!IsLit) return;
            t += Time.deltaTime;
            glow = Mathf.MoveTowards(glow, 1f, Time.deltaTime * 1.5f);
            float flick = 1f + Mathf.Sin(t * 9f) * 0.05f + Mathf.Sin(t * 23f) * 0.03f;
            lamp.intensity = 2.1f * glow * flick;
            for (int i = 0; i < moths.Length; i++)
            {
                if (!moths[i].gameObject.activeSelf) moths[i].gameObject.SetActive(true);
                float a = t * (1.7f + i * 0.6f) + i * 3f;
                float r = 0.42f + Mathf.Sin(t * 2.3f + i) * 0.14f;
                moths[i].localPosition = new Vector3(0.56f + Mathf.Cos(a) * r, 2.02f + Mathf.Sin(t * 3.1f + i * 2f) * 0.22f, Mathf.Sin(a) * r);
                moths[i].localRotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, Mathf.Sin(t * 30f + i) * 30f);
            }
            flame.localScale = new Vector3(0.14f, 0.22f * flick, 0.14f) * (0.4f + 0.6f * glow);
        }
    }
}

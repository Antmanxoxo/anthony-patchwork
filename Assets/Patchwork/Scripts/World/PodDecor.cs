using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Everything the Pod's decorating menu can change: the floor, the rug, the light, the
    // decorations you've won (on or off) and the furniture (on or off). The wallpaper is
    // PodTheme. All saved in PlayerPrefs and applied live.
    public static class PodDecor
    {
        public struct Item
        {
            public string Key, Name;
            public bool DefaultOn;
            public Item(string key, string name, bool on) { Key = key; Name = name; DefaultOn = on; }
        }

        public static readonly Item[] Furniture =
        {
            new Item("stool", "Toadstool Stool", true),
            new Item("cushion", "Zigzag Cushion", true),
            new Item("bouncer", "Bouncy Pin-Cushion", true),
            new Item("ball", "Beach Ball", true),
            new Item("blocks", "Building Blocks", true),
            new Item("beanbag", "Starry Bean Bag", false),
            new Item("chest", "Toy Chest", false),
            new Item("plant", "Felt Plant", false),
            new Item("lavalamp", "Lava Lamp", false),
            new Item("mobile", "Star Mobile", false),
            new Item("fairylights", "Fairy Lights", false),
        };

        public static readonly string[] FloorNames = { "Warm Planks", "Dark Planks", "Sky Gingham", "Mint Dots", "Starry Felt", "Cosy Knit" };
        public static readonly string[] RugNames = { "Zigzag", "Polka Dots", "Starry", "Picnic Check", "Candy Stripes", "No Rug" };
        public static readonly string[] LightNames = { "Warm Glow", "Candy Pink", "Moonlight", "Mint", "Disco!" };
        static readonly Color[] LightColors = { new Color(1f, 0.84f, 0.62f), new Color(1f, 0.55f, 0.75f), new Color(0.6f, 0.75f, 1f), new Color(0.6f, 1f, 0.75f), Color.white };

        static readonly Dictionary<string, GameObject> items = new Dictionary<string, GameObject>();
        static Renderer floor, rug, rugBorder, bulb;
        static Light bulbLight;

        public static void ResetStatics() { items.Clear(); floor = rug = rugBorder = bulb = null; bulbLight = null; haveBase = false; }

        // The room's own light (ambient and the sun through the window), remembered once the
        // Pod's lighting is set up, so the chosen light can tint the whole room, not just the bulb.
        static bool haveBase;
        static Color baseSky, baseEquator, baseGround, baseSun;
        static float baseSunIntensity;
        static readonly float[] LightBrightness = { 1f, 1f, 0.7f, 1f, 1f };

        public static void CaptureRoomLight()
        {
            haveBase = true;
            baseSky = RenderSettings.ambientSkyColor;
            baseEquator = RenderSettings.ambientEquatorColor;
            baseGround = RenderSettings.ambientGroundColor;
            if (RenderSettings.sun != null) { baseSun = RenderSettings.sun.color; baseSunIntensity = RenderSettings.sun.intensity; }
            ApplyLight();
        }

        // Tints the whole room towards a colour (strength 0..1), and dims or brightens it.
        public static void TintRoom(Color c, float strength, float brightness)
        {
            if (!haveBase) return;
            RenderSettings.ambientSkyColor = Color.Lerp(baseSky, c * 0.55f, strength) * brightness;
            RenderSettings.ambientEquatorColor = Color.Lerp(baseEquator, c * 0.8f, strength) * brightness;
            RenderSettings.ambientGroundColor = Color.Lerp(baseGround, c * 0.5f, strength) * brightness;
            if (RenderSettings.sun != null)
            {
                RenderSettings.sun.color = Color.Lerp(baseSun, c, strength * 0.7f);
                RenderSettings.sun.intensity = baseSunIntensity * Mathf.Lerp(1f, brightness, 0.8f);
            }
        }

        public static void Register(string key, GameObject go) { items[key] = go; }
        public static void RegisterFloor(Renderer f, Renderer r, Renderer border) { floor = f; rug = r; rugBorder = border; }
        public static void RegisterBulb(Light l, Renderer r) { bulbLight = l; bulb = r; }
        public static bool Exists(string key) { return items.ContainsKey(key) && items[key] != null; }

        // The middle of an item (for a sparkle when it's switched), or null if it isn't there.
        public static Vector3? Find(string key)
        {
            GameObject go;
            if (!items.TryGetValue(key, out go) || go == null) return null;
            var rs = go.GetComponentsInChildren<Renderer>(true);
            if (rs.Length == 0) return go.transform.position;
            var b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            return b.center;
        }

        public static bool IsOn(string key, bool def) { return PlayerPrefs.GetInt("pw_pod_item_" + key, def ? 1 : 0) == 1; }

        public static void Set(string key, bool on)
        {
            PlayerPrefs.SetInt("pw_pod_item_" + key, on ? 1 : 0);
            PlayerPrefs.Save();
            GameObject go;
            if (items.TryGetValue(key, out go) && go != null) go.SetActive(on);
        }

        public static int Floor { get { return Mathf.Clamp(PlayerPrefs.GetInt("pw_pod_floor", 0), 0, FloorNames.Length - 1); } set { PlayerPrefs.SetInt("pw_pod_floor", value); ApplyFloor(); } }
        public static int Rug { get { return Mathf.Clamp(PlayerPrefs.GetInt("pw_pod_rug", 0), 0, RugNames.Length - 1); } set { PlayerPrefs.SetInt("pw_pod_rug", value); ApplyFloor(); } }
        public static int LightMode { get { return Mathf.Clamp(PlayerPrefs.GetInt("pw_pod_light", 0), 0, LightNames.Length - 1); } set { PlayerPrefs.SetInt("pw_pod_light", value); ApplyLight(); } }

        static Material FloorMaterial(int i)
        {
            Color cream = new Color(0.99f, 0.96f, 0.9f);
            switch (i)
            {
                case 1: return PatchworkAssets.Planks(new Color(0.55f, 0.38f, 0.28f));
                case 2: return PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, cream, new Color(0.5f, 0.75f, 0.95f));
                case 3: return PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(0.6f, 0.88f, 0.78f), cream);
                case 4: return PatchworkAssets.Print(PatchworkAssets.PrintKind.Stars, new Color(0.18f, 0.2f, 0.4f), new Color(1f, 0.85f, 0.4f));
                case 5: return PatchworkAssets.Knit(new Color(0.92f, 0.82f, 0.7f));
                default: return PatchworkAssets.Planks(new Color(0.84f, 0.66f, 0.48f));
            }
        }

        static Material RugMaterial(int i)
        {
            Color cream = new Color(0.99f, 0.96f, 0.9f);
            switch (i)
            {
                case 1: return PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(1f, 0.55f, 0.72f), cream);
                case 2: return PatchworkAssets.Print(PatchworkAssets.PrintKind.Stars, new Color(0.2f, 0.25f, 0.55f), new Color(1f, 0.8f, 0.28f));
                case 3: return PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, cream, new Color(0.9f, 0.3f, 0.35f));
                case 4: return PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.6f, 0.88f, 0.8f), cream);
                default: return PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, new Color(0.45f, 0.75f, 0.95f), new Color(1f, 0.9f, 0.55f));
            }
        }

        static void ApplyFloor()
        {
            if (floor != null) floor.sharedMaterial = FloorMaterial(Floor);
            bool none = Rug == RugNames.Length - 1;
            if (rug != null) { rug.enabled = !none; if (!none) rug.sharedMaterial = RugMaterial(Rug); }
            if (rugBorder != null) rugBorder.enabled = !none;
        }

        static void ApplyLight()
        {
            if (bulbLight == null) return;
            int m = LightMode;
            var disco = bulbLight.GetComponent<DiscoLight>();
            bulbLight.intensity = 3.4f;
            bulbLight.range = 22f;
            if (m == LightNames.Length - 1)
            {
                if (disco == null) disco = bulbLight.gameObject.AddComponent<DiscoLight>();
                disco.Bulb = bulb;
                disco.enabled = true;
                return;
            }
            if (disco != null) disco.enabled = false;
            bulbLight.color = LightColors[m];
            TintRoom(LightColors[m], m == 0 ? 0.25f : 0.55f, LightBrightness[m]);
            if (bulb != null) bulb.sharedMaterial = PatchworkAssets.Glow(Color.Lerp(LightColors[m], Color.white, 0.25f), 2.2f);
        }

        public static void ApplyAll()
        {
            foreach (var f in Furniture) Set(f.Key, IsOn(f.Key, f.DefaultOn));
            for (int i = 0; i < Prizes.DecorationNames.Length; i++)
            {
                string key = "deco" + i;
                if (Exists(key)) Set(key, IsOn(key, true));
            }
            ApplyFloor();
            ApplyLight();
        }
    }

    // The Pod's "Disco!" light: the bulb cycles through the rainbow.
    public class DiscoLight : MonoBehaviour
    {
        public Renderer Bulb;
        Light l;
        float t;
        Material[] mats;

        void OnEnable() { l = GetComponent<Light>(); }

        void Update()
        {
            t += Time.deltaTime;
            Color c = Color.HSVToRGB(Mathf.Repeat(t * 0.25f, 1f), 0.55f, 1f);
            if (l != null) l.color = c;
            PodDecor.TintRoom(c, 0.5f, 1f);
            if (Bulb != null)
            {
                if (mats == null)
                {
                    mats = new Material[6];
                    for (int i = 0; i < mats.Length; i++) mats[i] = PatchworkAssets.Glow(Color.HSVToRGB(i / 6f, 0.5f, 1f), 2.2f);
                }
                Bulb.sharedMaterial = mats[Mathf.FloorToInt(Mathf.Repeat(t * 1.5f, 1f) * mats.Length) % mats.Length];
            }
        }
    }
}

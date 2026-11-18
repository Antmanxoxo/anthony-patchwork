using UnityEngine;

namespace Patchwork
{
    // Patch's wardrobe: what points buy. Every point scored anywhere goes into the purse (see
    // GameState.Purse), and in the Satchel you can spend it on:
    //   Colours - a dye for the whole of him (in every form)
    //   Fabric  - what he's made of: felt, velvet, satin, corduroy, glitter...
    //   Outfit  - something to wear under the hat: a scarf, a bow tie, a cape...
    // The first of each is free and always yours. Hats are still found out in the world.
    public static class Wardrobe
    {
        public const int Colours = 0, Fabrics = 1, Outfits = 2;

        public static readonly string[] ColourNames = { "Classic", "Sky", "Mint", "Sunflower", "Lavender", "Coral", "Cocoa", "Midnight", "Seafoam", "Rosewood" };
        public static readonly Color[] ColourTints =
        {
            Color.white, new Color(0.62f, 0.8f, 1f), new Color(0.62f, 1f, 0.78f), new Color(1f, 0.92f, 0.45f), new Color(0.82f, 0.7f, 1f),
            new Color(1f, 0.62f, 0.55f), new Color(0.72f, 0.55f, 0.45f), new Color(0.42f, 0.46f, 0.78f), new Color(0.5f, 0.92f, 0.88f), new Color(0.78f, 0.42f, 0.5f)
        };
        public static readonly int[] ColourPrices = { 0, 1500, 1500, 2000, 2000, 2500, 2500, 3500, 3500, 5000 };
        // For the Yarn form's own colours: the gold of his knit becomes the first colour, and
        // the teal of his patches the second. (Red stitches, cream and the dark bits stay.)
        public static readonly Color[] SecondTints =
        {
            Color.white, new Color(1f, 0.85f, 0.4f), new Color(0.95f, 0.55f, 0.65f), new Color(0.45f, 0.35f, 0.8f), new Color(0.55f, 0.85f, 0.75f),
            new Color(0.35f, 0.65f, 0.85f), new Color(0.92f, 0.82f, 0.65f), new Color(0.95f, 0.8f, 0.35f), new Color(1f, 0.6f, 0.45f), new Color(0.95f, 0.85f, 0.7f)
        };

        public static readonly string[] FabricNames = { "Knit", "Felt", "Velvet", "Satin", "Corduroy", "Patent Leather", "Glitter" };
        // Smoothness, metallic, and how deep the stitches and bumps look.
        public static readonly float[] FabricSmooth = { 0.12f, 0.04f, 0.32f, 0.62f, 0.08f, 0.86f, 0.78f };
        public static readonly float[] FabricMetal = { 0f, 0f, 0f, 0.05f, 0f, 0.08f, 0.55f };
        static readonly float[] FabricBump = { 1f, 0.45f, 0.7f, 0.35f, 2f, 0.2f, 1.2f };
        static readonly float[] FabricShade = { 1f, 1f, 0.86f, 1.04f, 0.92f, 0.95f, 1.05f };
        public static readonly int[] FabricPrices = { 0, 1200, 3000, 3000, 2500, 5000, 8000 };

        public static readonly string[] OutfitNames = { "Nothing", "Striped Scarf", "Bow Tie", "Explorer's Backpack", "Hero Cape", "Flower Garland", "Star Sash", "Lifebuoy" };
        public static readonly int[] OutfitPrices = { 0, 2500, 2000, 4000, 6000, 3000, 4500, 3500 };

        public static string[] Names(int cat) { return cat == Colours ? ColourNames : cat == Fabrics ? FabricNames : OutfitNames; }
        public static int[] Prices(int cat) { return cat == Colours ? ColourPrices : cat == Fabrics ? FabricPrices : OutfitPrices; }
        static string Key(int cat, int i) { return (cat == Colours ? "colour" : cat == Fabrics ? "fabric" : "outfit") + i; }

        public static bool Has(int cat, int i) { return i == 0 || GameState.Owns(Key(cat, i)); }
        public static bool Buy(int cat, int i) { return Has(cat, i) || GameState.Buy(Key(cat, i), Prices(cat)[i]); }

        public static int Current(int cat) { return cat == Colours ? GameState.ColourScheme : cat == Fabrics ? GameState.Fabric : GameState.Outfit; }
        public static void Wear(int cat, int i)
        {
            if (cat == Colours) GameState.ColourScheme = i;
            else if (cat == Fabrics) GameState.Fabric = i;
            else GameState.Outfit = i;
            GameState.Save();
        }

        public static Color Tint
        {
            get
            {
                int c = Mathf.Clamp(GameState.ColourScheme, 0, ColourTints.Length - 1);
                int f = Mathf.Clamp(GameState.Fabric, 0, FabricNames.Length - 1);
                Color t = ColourTints[c] * FabricShade[f];
                t.a = 1f;
                return t;
            }
        }

        // ------------------------------------------------------------------ Textures

        // Patch's own texture, re-dyed for the colour scheme and woven in the fabric: the gold
        // knit takes the first colour and the teal patches the second (their light and shade
        // kept), and the fabric's weave is worked into it. Only the latest one is kept.
        // A few are kept (the Store shows other colours while Patch keeps his own); the one used
        // longest ago goes first.
        static readonly System.Collections.Generic.List<int> dyedKeys = new System.Collections.Generic.List<int>();
        static readonly System.Collections.Generic.List<Texture2D> dyedTex = new System.Collections.Generic.List<Texture2D>();
        const int DyedKept = 4;

        // The size the dye works at, for a texture.
        public static int DyeSize(Texture source) { return source == null ? 0 : Mathf.Min(Mathf.Max(source.width, source.height), 1024); }

        // keepHead: one per pixel at DyeSize; true for the bits of his head, where only the gold
        // knit takes the dye (his button eyes stay their own colours).
        static bool[] headMask;
        public static void SetHeadMask(bool[] mask)
        {
            if (headMask == mask) return;
            headMask = mask;
            foreach (var t in dyedTex) if (t != null) Object.Destroy(t);
            dyedKeys.Clear();
            dyedTex.Clear();
        }

        public static Texture2D DyedAlbedo(Texture source)
        {
            int c = Mathf.Clamp(GameState.ColourScheme, 0, ColourTints.Length - 1);
            int f = Mathf.Clamp(GameState.Fabric, 0, FabricNames.Length - 1);
            if (source == null || c == 0 && Weave(f) == 0) return null;
            int key = c * 100 + f;
            int at = dyedKeys.IndexOf(key);
            if (at >= 0 && dyedTex[at] != null)
            {
                // Most recently used goes to the end.
                var hit = dyedTex[at];
                dyedKeys.RemoveAt(at); dyedTex.RemoveAt(at);
                dyedKeys.Add(key); dyedTex.Add(hit);
                return hit;
            }
            if (at >= 0) { dyedKeys.RemoveAt(at); dyedTex.RemoveAt(at); }
            // (Worked at up to 1024 across, so choosing in the wardrobe stays quick.)
            int w = DyeSize(source), h = w;
            var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var was = RenderTexture.active;
            Graphics.Blit(source, rt);
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            RenderTexture.active = was;
            RenderTexture.ReleaseTemporary(rt);
            var px = tex.GetPixels32();
            float h1, s1, v1, h2, s2, v2;
            Color.RGBToHSV(ColourTints[c], out h1, out s1, out v1);
            Color.RGBToHSV(SecondTints[c], out h2, out s2, out v2);
            for (int i = 0; i < px.Length; i++)
            {
                var p = px[i];
                float hh, ss, vv;
                Color.RGBToHSV(new Color(p.r / 255f, p.g / 255f, p.b / 255f), out hh, out ss, out vv);
                if (c != 0)
                {
                    if (ss > 0.3f && hh > 0.045f && hh < 0.17f) { hh = h1; ss = Mathf.Clamp01(ss * s1 / 0.75f); vv = Mathf.Clamp01(vv * Mathf.Lerp(1f, v1 / 0.85f, 0.85f)); }
                    else if (ss > 0.22f && hh > 0.4f && hh < 0.6f && (headMask == null || headMask.Length != px.Length || !headMask[i])) { hh = h2; ss = Mathf.Clamp01(ss * s2 / 0.7f); vv = Mathf.Clamp01(vv * Mathf.Lerp(1f, v2 / 0.6f, 0.75f)); }
                }
                vv = Mathf.Clamp01(vv * WeaveAt(f, i % w, i / w));
                if (Weave(f) == 4 && Speck(i % w, i / w)) { ss *= 0.15f; vv = 1f; }   // glitter: little white-gold specks
                var o = Color.HSVToRGB(hh, ss, vv);
                px[i] = new Color32((byte)(o.r * 255f), (byte)(o.g * 255f), (byte)(o.b * 255f), 255);
            }
            tex.SetPixels32(px);
            tex.Apply(true);
            tex.name = "PatchDyed";
            while (dyedTex.Count >= DyedKept)
            {
                if (dyedTex[0] != null) Object.Destroy(dyedTex[0]);
                dyedKeys.RemoveAt(0); dyedTex.RemoveAt(0);
            }
            dyedKeys.Add(key);
            dyedTex.Add(tex);
            return tex;
        }

        // 0: no weave of its own (the knit, satin, patent leather); otherwise how it's woven.
        static int Weave(int f) { return f == 1 ? 1 : f == 2 ? 2 : f == 4 ? 3 : f == 6 ? 4 : 0; }

        // How much a fabric lightens or darkens each spot (x, y in texture pixels).
        static float WeaveAt(int f, int x, int y)
        {
            switch (Weave(f))
            {
                case 1: { uint n = (uint)((x / 3) * 73856093 ^ (y / 3) * 19349663); return 0.9f + (n % 1000) / 1000f * 0.14f; }   // felt: fuzzy speckle
                case 2: return 0.82f + 0.2f * (0.5f + 0.5f * Mathf.Sin(x * 0.021f + Mathf.Sin(y * 0.013f) * 2f) * Mathf.Sin(y * 0.017f));   // velvet: crushed sheen
                case 3: return 0.72f + 0.3f * Mathf.Abs(Mathf.Sin(x * Mathf.PI / 7f));   // corduroy: ribs
                case 4: return Speck(x, y) ? 1.5f : 0.9f;   // glitter: specks on a slightly darker ground
                default: return f == 3 ? 1.06f : f == 5 ? 0.92f : 1f;
            }
        }

        static bool Speck(int x, int y)
        {
            uint n = (uint)((x / 2) * 73856093 ^ (y / 2) * 19349663 ^ 0x5bd1e995);
            n ^= n >> 13; n *= 0x5bd1e995; n ^= n >> 15;
            return n % 23 == 0;
        }

        // A small tiling weave for the Water and Wind forms (null: keep their own look).
        static readonly Texture2D[] weaves = new Texture2D[7];
        public static Texture2D WeaveTexture(int f)
        {
            if (Weave(f) == 0) return null;
            if (weaves[f] != null) return weaves[f];
            const int S = 128;
            var t = new Texture2D(S, S, TextureFormat.RGBA32, true);
            t.wrapMode = TextureWrapMode.Repeat;
            var px = new Color32[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float k = Mathf.Clamp01(WeaveAt(f, x * 6, y * 6) * 0.95f);
                    byte b = (byte)(k * 255f);
                    px[y * S + x] = new Color32(b, b, b, 255);
                }
            t.SetPixels32(px);
            t.Apply(true);
            weaves[f] = t;
            return t;
        }

        // Dresses a material for the current fabric.
        public static void Finish(Material m)
        {
            if (m == null) return;
            int f = Mathf.Clamp(GameState.Fabric, 0, FabricNames.Length - 1);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", FabricSmooth[f]);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", FabricMetal[f]);
            if (m.HasProperty("_BumpScale")) m.SetFloat("_BumpScale", FabricBump[f]);
        }

        public static string Describe(int cat, int i)
        {
            if (cat == Colours) return i == 0 ? "Patch's own colours." : "Dyes every form of him.";
            if (cat == Fabrics)
            {
                switch (i)
                {
                    case 0: return "Soft, chunky knit. The original.";
                    case 1: return "Flat and fuzzy, like a puppet.";
                    case 2: return "Deep and soft, with a sheen.";
                    case 3: return "Smooth and shiny, catches the light.";
                    case 4: return "Thick ribs you can almost feel.";
                    case 5: return "Glossy as a new pair of shoes.";
                    default: return "Sparkles like a party.";
                }
            }
            return i == 0 ? "Just Patch (and his hat)." : "Worn in every form, under the hat.";
        }
    }
}

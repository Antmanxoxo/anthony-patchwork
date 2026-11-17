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
        public static readonly string[] Categories = { "Colours", "Fabric", "Outfit" };

        public static readonly string[] ColourNames = { "Classic", "Sky", "Mint", "Sunflower", "Lavender", "Coral", "Cocoa", "Midnight", "Seafoam", "Rosewood" };
        public static readonly Color[] ColourTints =
        {
            Color.white, new Color(0.62f, 0.8f, 1f), new Color(0.62f, 1f, 0.78f), new Color(1f, 0.92f, 0.45f), new Color(0.82f, 0.7f, 1f),
            new Color(1f, 0.62f, 0.55f), new Color(0.72f, 0.55f, 0.45f), new Color(0.42f, 0.46f, 0.78f), new Color(0.5f, 0.92f, 0.88f), new Color(0.78f, 0.42f, 0.5f)
        };
        public static readonly int[] ColourPrices = { 0, 1500, 1500, 2000, 2000, 2500, 2500, 3500, 3500, 5000 };

        public static readonly string[] FabricNames = { "Knit", "Felt", "Velvet", "Satin", "Corduroy", "Patent Leather", "Glitter" };
        // Smoothness, metallic, and how deep the stitches and bumps look.
        static readonly float[] FabricSmooth = { 0.12f, 0.04f, 0.32f, 0.62f, 0.08f, 0.86f, 0.78f };
        static readonly float[] FabricMetal = { 0f, 0f, 0f, 0.05f, 0f, 0.08f, 0.55f };
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

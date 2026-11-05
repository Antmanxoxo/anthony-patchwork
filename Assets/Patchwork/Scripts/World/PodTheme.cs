using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The pod's wallpaper. Flip through the pattern book by the window to change it; it's saved.
    public static class PodTheme
    {
        const string Key = "pw_pod_theme";
        public static readonly string[] Names =
            { "Cardboard", "Zigzag", "Polka Dots", "Denim", "Cosy Knit", "Gingham", "Starry Night", "Candy Stripes" };

        static readonly List<Renderer> walls = new List<Renderer>();

        public static int Current
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(Key, 0), 0, Names.Length - 1); }
            set { PlayerPrefs.SetInt(Key, ((value % Names.Length) + Names.Length) % Names.Length); PlayerPrefs.Save(); }
        }

        public static void ResetStatics() { walls.Clear(); }

        public static void Register(Renderer r) { if (r != null) walls.Add(r); }

        public static Material Material(int theme)
        {
            Color cream = new Color(0.99f, 0.96f, 0.9f);
            switch (theme)
            {
                case 1: return PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, new Color(0.2f, 0.25f, 0.55f), new Color(1f, 0.8f, 0.28f));
                case 2: return PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(0.88f, 0.25f, 0.27f), cream);
                case 3: return PatchworkAssets.Fabric(new Color(0.4f, 0.5f, 0.75f));
                case 4: return PatchworkAssets.Knit(new Color(0.95f, 0.87f, 0.74f));
                case 5: return PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, cream, new Color(0.93f, 0.55f, 0.66f));
                case 6: return PatchworkAssets.Print(PatchworkAssets.PrintKind.Stars, new Color(0.17f, 0.18f, 0.38f), new Color(1f, 0.85f, 0.4f));
                case 7: return PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.6f, 0.88f, 0.8f), cream);
                default: return PatchworkAssets.Cardboard(new Color(0.8f, 0.62f, 0.42f));
            }
        }

        public static void Apply()
        {
            var m = Material(Current);
            walls.RemoveAll(r => r == null);
            foreach (var r in walls) r.sharedMaterial = m;
        }

        public static void Next()
        {
            Current = Current + 1;
            Apply();
            Merits.Earn(Merits.Id.HomeMaker);
        }
    }
}

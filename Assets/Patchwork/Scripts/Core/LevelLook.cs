using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Patchwork
{
    // The stylised "storybook" look over everything: soft glow on bright and glowing things,
    // richer colour, warm highlights and cool shadows, and a gentle vignette. Each level can
    // nudge it through its LevelInfo (warmth, saturation, glow, vignette).
    public static class LevelLook
    {
        public static Volume Apply(LevelInfo level)
        {
            var go = new GameObject("Look");
            var vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 10f;
            var p = ScriptableObject.CreateInstance<VolumeProfile>();
            p.name = "PatchworkLook";

            var tone = p.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.Neutral);

            var bloom = p.Add<Bloom>(true);
            bloom.threshold.Override(1.15f);
            bloom.intensity.Override(level.Glow);
            bloom.scatter.Override(0.68f);
            bloom.tint.Override(new Color(1f, 0.93f, 0.86f));
            bloom.highQualityFiltering.Override(true);

            var adjust = p.Add<ColorAdjustments>(true);
            adjust.postExposure.Override(0.1f);
            adjust.contrast.Override(level.Contrast);
            adjust.saturation.Override(level.Saturation);

            // Warm sunny highlights, slightly cool violet shadows: a painted, storybook feel.
            var smh = p.Add<ShadowsMidtonesHighlights>(true);
            smh.shadows.Override(new Vector4(0.96f, 0.97f, 1.08f, 0f));
            smh.midtones.Override(new Vector4(1.02f, 1f, 0.98f, 0f));
            smh.highlights.Override(new Vector4(1.06f, 1.02f, 0.94f, 0f));

            var wb = p.Add<WhiteBalance>(true);
            wb.temperature.Override(level.Warmth);

            var vig = p.Add<Vignette>(true);
            vig.intensity.Override(level.Vignette);
            vig.smoothness.Override(0.5f);
            vig.color.Override(new Color(0.16f, 0.08f, 0.2f));

            vol.profile = p;
            go.AddComponent<OwnedProfile>().Profile = p;
            return vol;
        }
    }
}

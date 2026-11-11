using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Patchwork
{
    // The stylised "storybook" look over everything: soft glow on bright and glowing things,
    // richer colour, warm highlights and cool shadows, and a gentle vignette. Each level can
    // nudge it through its LevelInfo (warmth, saturation, glow, vignette, miniature).
    public static class LevelLook
    {
        static DepthOfField miniature;
        static ColorAdjustments colours;
        static float baseSaturation, baseContrast;

        // The Fray drains the colour out of things. amount: 0 (the level's own colours) to 1
        // (nearly grey, flatter and a touch dimmer). flash: a moment of extra colour as it comes
        // back (when a creature is unravelled).
        public static void SetDrain(float amount, float flash = 0f)
        {
            if (colours == null) return;
            amount = Mathf.Clamp01(amount);
            // (Only part of the way to grey even at its strongest: you should still be able to
            // tell what colour things are, just that it's being sucked out of them.)
            colours.saturation.Override(Mathf.Lerp(baseSaturation, -38f, amount) + flash * 18f);
            colours.contrast.Override(Mathf.Lerp(baseContrast, baseContrast - 5f, amount));
            colours.postExposure.Override(0.1f - amount * 0.07f + flash * 0.06f);
        }

        // Switch the soft-focus distance off for a while (a cutscene that shows the whole town
        // from far away wants it sharp), and back on afterwards.
        public static void SetMiniature(bool on)
        {
            if (miniature != null) miniature.active = on;
        }

        public static Volume Apply(LevelInfo level)
        {
            miniature = null;
            colours = null;
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
            colours = adjust;
            baseSaturation = level.Saturation;
            baseContrast = level.Contrast;

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

            // The miniature look: like a close-up photo of a model on a table, the far distance
            // goes softly out of focus, so the world reads as small and handmade. (Skipped on
            // the Fast graphics setting.)
            if (level.Miniature > 0f && GameSettings.Graphics > 0)
            {
                var dof = p.Add<DepthOfField>(true);
                miniature = dof;
                dof.mode.Override(DepthOfFieldMode.Gaussian);
                dof.gaussianStart.Override(42f);
                dof.gaussianEnd.Override(150f);
                dof.gaussianMaxRadius.Override(Mathf.Lerp(0.5f, 1.1f, Mathf.Clamp01(level.Miniature)));
                dof.highQualitySampling.Override(GameSettings.Graphics > 1);
            }

            vol.profile = p;
            go.AddComponent<OwnedProfile>().Profile = p;
            return vol;
        }
    }
}

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
        static Vignette vignette;
        static float baseSaturation, baseContrast, baseVignette, lastDrain, gloom;
        static Color baseVignetteColour;

        // The Fray drains the colour out of things. amount: 0 (the level's own colours) to 1
        // (nearly grey, flatter and a touch dimmer). flash: a moment of extra colour as it comes
        // back (when a creature is unravelled).
        public static void SetDrain(float amount, float flash = 0f)
        {
            if (colours == null) return;
            amount = Mathf.Clamp01(amount);
            lastDrain = amount;
            // (Only part of the way to grey even at its strongest: you should still be able to
            // tell what colour things are, just that it's being sucked out of them.)
            colours.saturation.Override(Mathf.Lerp(baseSaturation, -38f, amount) + flash * 18f - gloom * 55f);
            colours.contrast.Override(Mathf.Lerp(baseContrast, baseContrast - 5f, amount) + gloom * 14f);
            colours.postExposure.Override(0.1f - amount * 0.07f + flash * 0.06f - gloom * 0.25f);
        }

        // Somewhere it's frightening to be: the colour goes out, the shadows deepen and the edges
        // of the screen close in (0 = none, 1 = as dark as it gets).
        public static void SetGloom(float k)
        {
            gloom = Mathf.Clamp01(k);
            if (vignette != null)
            {
                vignette.intensity.Override(Mathf.Lerp(baseVignette, 0.52f, gloom));
                vignette.smoothness.Override(Mathf.Lerp(0.5f, 0.85f, gloom));
                vignette.color.Override(Color.Lerp(baseVignetteColour, Color.black, gloom));
            }
            SetDrain(lastDrain);
        }

        // Switch the soft-focus distance off for a while (a cutscene that shows the whole town
        // from far away wants it sharp), and back on afterwards.
        // Under the water: everything goes a little blue-green (0 = dry, 1 = right under).
        public static void SetUnderwater(float k)
        {
            if (colours == null) return;
            colours.colorFilter.Override(Color.Lerp(Color.white, new Color(0.68f, 0.92f, 1f), Mathf.Clamp01(k)));
        }

        public static void SetMiniature(bool on)
        {
            if (miniature != null) miniature.active = on;
        }

        const float MiniatureStart = 42f, MiniatureEnd = 150f;

        // A cutscene's own focus, like a camera on a model town: whatever's about dist away
        // stays sharp and the background melts softly away behind it. k: 0 = the everyday
        // miniature look, 1 = focused on the subject. (ClearCutsceneFocus snaps straight back.)
        public static void SetCutsceneFocus(float dist, float k)
        {
            if (miniature == null) return;
            k = Mathf.Clamp01(k);
            float near = Mathf.Max(6f, dist * 1.7f + 3f);
            miniature.gaussianStart.Override(Mathf.Lerp(MiniatureStart, near, k));
            miniature.gaussianEnd.Override(Mathf.Lerp(MiniatureEnd, near * 2.6f + 12f, k));
        }

        public static void ClearCutsceneFocus()
        {
            if (miniature == null) return;
            miniature.gaussianStart.Override(MiniatureStart);
            miniature.gaussianEnd.Override(MiniatureEnd);
        }

        public static Volume Apply(LevelInfo level)
        {
            miniature = null;
            colours = null;
            vignette = null;
            gloom = 0f;
            lastDrain = 0f;
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
            vignette = vig;
            baseVignette = level.Vignette;
            baseVignetteColour = new Color(0.16f, 0.08f, 0.2f);

            // The miniature look: like a close-up photo of a model on a table, the far distance
            // goes softly out of focus, so the world reads as small and handmade. (Skipped on
            // the Fast graphics setting.)
            if (level.Miniature > 0f && GameSettings.Graphics > 0)
            {
                var dof = p.Add<DepthOfField>(true);
                miniature = dof;
                dof.mode.Override(DepthOfFieldMode.Gaussian);
                dof.gaussianStart.Override(MiniatureStart);
                dof.gaussianEnd.Override(MiniatureEnd);
                dof.gaussianMaxRadius.Override(Mathf.Lerp(0.5f, 1.1f, Mathf.Clamp01(level.Miniature)));
                dof.highQualitySampling.Override(GameSettings.Graphics > 1);
            }

            vol.profile = p;
            go.AddComponent<OwnedProfile>().Profile = p;
            return vol;
        }
    }
}

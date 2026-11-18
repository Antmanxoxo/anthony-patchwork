using UnityEngine;

namespace Patchwork
{
    // The streak left behind a full sprint, a different one for each form:
    //   Strand  (Yarn)  - a loose end of his own yarn unspooling behind him, wiggling as it goes.
    //   Slick   (Water) - a shiny wet streak along the ground where he belly-slides.
    //   Ribbons (Wind)  - two little ribbons of breeze corkscrewing round each other.
    // It remembers where Patch has been a few times a second and draws a line through those
    // spots. It fades by thinning out, so it needs no see-through material.
    public class SprintTrail
    {
        public enum Style { Strand, Slick, Ribbons }
        const int Count = 22;
        const float Every = 0.025f;

        readonly Style style;
        readonly LineRenderer[] lines;
        readonly Vector3[] hist = new Vector3[Count];
        readonly Vector3[] draw = new Vector3[Count];
        readonly float baseWidth;
        int filled;
        float clock, time, width;

        public SprintTrail(Transform parent, Style style, Material a, Material b, float baseWidth)
        {
            this.style = style;
            this.baseWidth = baseWidth;
            lines = new LineRenderer[style == Style.Ribbons ? 2 : 1];
            var taper = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.6f, 0.7f), new Keyframe(1f, 0f));
            for (int i = 0; i < lines.Length; i++)
            {
                var go = new GameObject("SprintTrail_" + style);
                go.transform.SetParent(parent, false);
                go.layer = 2;
                var lr = go.AddComponent<LineRenderer>();
                lr.useWorldSpace = true;
                lr.positionCount = 0;
                lr.sharedMaterial = i == 0 || b == null ? a : b;
                lr.numCapVertices = 3;
                lr.numCornerVertices = 2;
                lr.widthCurve = taper;
                lr.widthMultiplier = 0f;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.enabled = false;
                lines[i] = lr;
            }
        }

        // anchor: where the trail starts this frame. weight: 0..1, how much of a sprint it is.
        public void Tick(Vector3 anchor, Vector3 right, float weight, float dt)
        {
            time += dt;
            width = Mathf.MoveTowards(width, weight * baseWidth, dt * baseWidth * 4f);
            if (width <= 0.0005f && weight <= 0.001f)
            {
                if (filled > 0)
                {
                    filled = 0;
                    foreach (var lr in lines) { lr.positionCount = 0; lr.enabled = false; }
                }
                return;
            }
            // Remember a new spot every so often (the newest is always where he is now).
            clock += dt;
            if (filled == 0) { for (int i = 0; i < Count; i++) hist[i] = anchor; filled = 1; }
            if (clock >= Every)
            {
                clock = 0f;
                for (int i = Count - 1; i > 0; i--) hist[i] = hist[i - 1];
                filled = Mathf.Min(filled + 1, Count);
            }
            hist[0] = anchor;

            for (int l = 0; l < lines.Length; l++)
            {
                var lr = lines[l];
                if (filled < 2) { lr.enabled = false; continue; }
                for (int i = 0; i < filled; i++)
                {
                    float k = i / (float)(Count - 1);
                    Vector3 p = hist[i];
                    switch (style)
                    {
                        case Style.Strand:
                            // A wiggle that runs down the strand, and the loose end drooping.
                            p += right * Mathf.Sin(time * 15f - i * 0.8f) * 0.07f * k;
                            p += Vector3.down * k * k * 0.3f;
                            break;
                        case Style.Ribbons:
                        {
                            float a = time * 10f + i * 0.75f + l * Mathf.PI;
                            float r = 0.1f + k * 0.28f;
                            p += (right * Mathf.Cos(a) + Vector3.up * Mathf.Sin(a)) * r;
                            break;
                        }
                    }
                    draw[i] = p;
                }
                lr.positionCount = filled;
                for (int i = 0; i < filled; i++) lr.SetPosition(i, draw[i]);
                lr.widthMultiplier = width;
                lr.enabled = true;
            }
        }
    }
}

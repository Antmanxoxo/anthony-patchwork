// The Patchwork look: a full-screen pass (drawn before bloom and colour grading).
// _PatchworkLook (global): 0 = plain, 1 = Storybook, 2 = Stitched.
// Storybook: clean, thin plum ink lines on the near side of each shape's edge and creases, with
// very gentle shading steps. Stitched: thicker dashed outlines like the seams on a plush toy, with
// light thread over a dark seam, and a stronger weave.
Shader "Patchwork/Look"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off Blend Off

        Pass
        {
            Name "PatchworkLook"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            float _PatchworkLook;

            float EyeDepth(float2 uv)
            {
                return LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // Raw depth, flipped if needed so near is big and the sky is 0. Raw depth changes
            // evenly across any flat surface on screen, so its second difference is ~0 on floors
            // and walls at any angle, and only jumps at real silhouettes.
            float RawDepth(float2 uv)
            {
                float r = SampleSceneDepth(uv);
            #if !UNITY_REVERSED_Z
                r = 1.0 - r;
            #endif
                return r;
            }

            // Storybook: clean, thin ink lines drawn only on the near side of each edge.
            float3 Storybook(float2 uv, float3 col, float scale)
            {
                float r = RawDepth(uv);
                if (r <= 1e-5) return col;   // the sky is left exactly as it is
                // Very gentle shading steps, with wide soft transitions so textures don't speckle.
                float lum = dot(col, float3(0.299, 0.587, 0.114));
                float lb = lum * 4.0;
                float stepped = (floor(lb) + smoothstep(0.15, 0.85, frac(lb))) / 4.0;
                col *= lerp(1.0, stepped / max(lum, 0.001), 0.12);

                float2 o = scale / _ScreenParams.xy;
                float rl = RawDepth(uv - float2(o.x, 0.0));
                float rr = RawDepth(uv + float2(o.x, 0.0));
                float rd = RawDepth(uv - float2(0.0, o.y));
                float ru = RawDepth(uv + float2(0.0, o.y));
                // Only where this pixel is nearer than its neighbours (the object's own rim).
                float lap = max(0.0, 2.0 * r - rl - rr) + max(0.0, 2.0 * r - rd - ru);
                float depthEdge = smoothstep(0.1, 0.25, lap / max(r, 1e-5));

                // Creases: compare with the right and upper neighbours only, so lines stay 1px.
                float3 n = SampleSceneNormals(uv);
                float3 nr = SampleSceneNormals(uv + float2(o.x, 0.0));
                float3 nu = SampleSceneNormals(uv + float2(0.0, o.y));
                // (A distance, not a dot product, so the empty sky, which has no normals, stays clean.)
                float crease = max(length(n - nr), length(n - nu));
                float normalEdge = smoothstep(0.5, 0.8, crease);

                float edge = max(depthEdge, normalEdge * 0.85);
                // Lines thin out with distance so the far nursery stays soft.
                float eye = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
                edge *= saturate(1.0 - (eye - 35.0) / 80.0);

                float3 ink = lerp(col * 0.35, float3(0.2, 0.13, 0.24), 0.4);
                return lerp(col, ink, edge * 0.75);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float3 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;
                if (_PatchworkLook < 0.5)
                    return half4(col, 1.0);

                bool stitched = _PatchworkLook > 1.5;
                float2 px = uv * _ScreenParams.xy;
                float scale = max(1.0, _ScreenParams.y / 1080.0);
                if (!stitched)
                    return half4(Storybook(uv, col, scale), 1.0);

                // ---- Soft cel bands: flatten the shading a little into steps.
                float lum = dot(col, float3(0.299, 0.587, 0.114));
                float bands = stitched ? 5.0 : 4.0;
                float lb = lum * bands;
                float stepped = (floor(lb) + smoothstep(0.3, 0.7, frac(lb))) / bands;
                col *= lerp(1.0, stepped / max(lum, 0.001), stitched ? 0.4 : 0.3);

                // ---- Outlines from depth and normals.
                float2 o = (stitched ? 2.0 : 1.3) * scale / _ScreenParams.xy;
                float d = EyeDepth(uv);
                float dl = EyeDepth(uv - float2(o.x, 0.0));
                float dr = EyeDepth(uv + float2(o.x, 0.0));
                float dd = EyeDepth(uv - float2(0.0, o.y));
                float du = EyeDepth(uv + float2(0.0, o.y));
                float depthEdge = (abs(dl + dr - 2.0 * d) + abs(dd + du - 2.0 * d)) / max(d, 0.5);
                float3 nl = SampleSceneNormals(uv - float2(o.x, 0.0));
                float3 nr = SampleSceneNormals(uv + float2(o.x, 0.0));
                float3 nd = SampleSceneNormals(uv - float2(0.0, o.y));
                float3 nu = SampleSceneNormals(uv + float2(0.0, o.y));
                float normalEdge = length(nl - nr) + length(nd - nu);
                float edge = saturate(smoothstep(0.06, 0.2, depthEdge) + smoothstep(0.55, 1.2, normalEdge));
                // Fade lines out into the distance so the far nursery stays soft.
                edge *= saturate(1.0 - (d - 45.0) / 110.0);

                // Ink is the colour underneath, darkened and nudged towards plum (never pure black).
                float3 ink = lerp(col * 0.32, float3(0.17, 0.11, 0.2), 0.35);
                if (stitched)
                {
                    // A darker seam, with light dashed thread running along it.
                    float dash = step(0.5, frac((px.x + px.y) / (10.0 * scale)));
                    float3 thread = lerp(col, float3(1.0, 0.97, 0.92), 0.65);
                    col = lerp(col, ink, edge * 0.6);
                    col = lerp(col, thread, edge * dash * 0.75);
                }
                else
                {
                    col = lerp(col, ink, edge * 0.8);
                }

                // ---- Fabric weave over everything (very faint), plus a little grain.
                float2 w = px / ((stitched ? 3.0 : 4.0) * scale);
                float weave = sin(w.x * 3.14159) * sin(w.y * 3.14159);
                float grain = (Hash21(floor(px / (2.0 * scale))) - 0.5);
                col *= 1.0 + weave * (stitched ? 0.04 : 0.022) + grain * 0.025;

                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}

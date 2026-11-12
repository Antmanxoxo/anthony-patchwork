using UnityEngine;

namespace Patchwork
{
    // A last layer of wonder over the whole museum: paper lanterns rising off the quay into the
    // night, a slow river of stars winding up through the atrium, stardust, sunbeams through
    // the kelp, a whale calf keeping close to its mother in the Great Deep, fireflies over the
    // Ruins, and heraldic pennants under the Hall of Arms' roof.
    public static partial class Level2Builder
    {
        static void Grandeur(LevelKit kit)
        {
            SkyLanterns(kit);
            StarRiver(kit);
            KelpLight(kit);
            DeepCalf(kit);
            RuinFireflies(kit);
            ArmsPennants(kit);
        }

        // Paper lanterns floating up from the sea beyond the quay, each one drifting and
        // swaying on its own, fading away high up, then starting again from the water.
        static void SkyLanterns(LevelKit kit)
        {
            var rng = new System.Random(2207);
            Color[] paper = { new Color(1f, 0.62f, 0.3f), new Color(1f, 0.78f, 0.4f), new Color(1f, 0.5f, 0.42f), new Color(1f, 0.86f, 0.55f) };
            for (int i = 0; i < 22; i++)
            {
                float x = -70f + (float)rng.NextDouble() * 140f;
                float z = -205f + (float)rng.NextDouble() * 32f;
                var g = kit.Group("SkyLantern", new Vector3(x, 0.4f, z));
                var glow = PatchworkAssets.Glow(paper[i % paper.Length], 1.6f + (float)rng.NextDouble() * 0.8f);
                float s = 0.7f + (float)rng.NextDouble() * 0.5f;
                NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.55f * s, 0f), new Vector3(0.8f * s, 0.55f * s, 0.8f * s), glow, default(Vector3), g));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.12f * s, 0f), new Vector3(0.62f * s, 0.04f * s, 0.62f * s), glow, default(Vector3), g));
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.1f * s, 0f), Vector3.one * 0.22f * s, PatchworkAssets.Glow(new Color(1f, 0.9f, 0.6f), 3f), default(Vector3), g));
                var sl = g.gameObject.AddComponent<SkyLantern>();
                sl.From = g.position;
                sl.Rise = 55f + (float)rng.NextDouble() * 25f;
                sl.Speed = 0.9f + (float)rng.NextDouble() * 0.7f;
                sl.Phase = (float)rng.NextDouble();
                sl.Drift = new Vector3(((float)rng.NextDouble() - 0.5f) * 14f, 0f, -18f - (float)rng.NextDouble() * 20f);
            }
        }

        // The river of stars: three slow turns of little golden stars rising round the middle
        // of the atrium, from above the armillary up towards the vault, the whole ribbon gently
        // turning. And stardust drifting down through the lamplight.
        static void StarRiver(LevelKit kit)
        {
            var g = kit.Group("StarRiver", new Vector3(0f, 0f, -80f));
            var mb = new MeshBatch();
            var fine = new MeshBatch();
            var rng = new System.Random(808);
            const int n = 170;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                float a = t * Mathf.PI * 2f * 3f;
                float r = 15f + Mathf.Sin(t * 9f) * 1.6f;
                float y = 12f + t * 15f;
                // Each star wanders a little off the line, so it reads as a river, not a wire.
                Vector3 p = new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r) + new Vector3((float)rng.NextDouble() - 0.5f, ((float)rng.NextDouble() - 0.5f) * 0.8f, (float)rng.NextDouble() - 0.5f) * 1.6f;
                if (i % 3 == 0)
                {
                    float sz = 0.16f + (float)rng.NextDouble() * 0.14f;
                    var spin = Quaternion.Euler((float)rng.NextDouble() * 360f, (float)rng.NextDouble() * 360f, 0f);
                    mb.Box(p, new Vector3(sz * 2.4f, sz * 0.3f, sz * 0.3f), spin);
                    mb.Box(p, new Vector3(sz * 0.3f, sz * 2.4f, sz * 0.3f), spin);
                    mb.Ball(p, Vector3.one * sz * 0.9f, 2, 5);
                }
                else fine.Ball(p, Vector3.one * (0.07f + (float)rng.NextDouble() * 0.07f), 2, 4);
            }
            var big = Twinkling(mb, g, "StarRiverStars", new Color(1f, 0.85f, 0.5f), 1.2f, 3.4f, 0.8f, 0f);
            var small = Twinkling(fine, g, "StarRiverDust", new Color(0.8f, 0.9f, 1f), 0.8f, 2.6f, 1.3f, 1.7f);
            foreach (var t in new[] { big, small })
            {
                if (t == null) continue;
                var sp = t.gameObject.AddComponent<Spinner>();
                sp.Speed = t == big ? 3.2f : -2.2f;
            }
            Motes(kit, new Vector3(0f, 15f, -80f), new Vector3(48f, 22f, 48f), 50, new Color(1f, 0.85f, 0.55f), 2.2f, 0.1f, new Vector3(0.05f, -0.18f, 0.03f));
        }

        // Sunbeams (moonbeams, at this hour) slanting down through the kelp from the skylights.
        static void KelpLight(LevelKit kit)
        {
            // Plankton glittering in the beams.
            Motes(kit, new Vector3(-60f, 10f, 38f), new Vector3(50f, 18f, 72f), 40, new Color(0.6f, 1f, 0.85f), 2f, 0.08f, new Vector3(0.08f, 0.06f, 0.05f));
        }

        // A whale calf, swimming just behind and below its mother on her long loop of the Deep.
        static void DeepCalf(LevelKit kit)
        {
            float cx = (DeepX0 + DeepX1) * 0.5f;
            var calf = Swim(kit, Swimmer.Whale, new Vector3(cx, 12.5f, 60f), 25f, 30f, 4.6f, 0.75f, new Color(0.22f, 0.32f, 0.6f), new Color(0.82f, 0.9f, 0.98f), 22f);
            calf.Bob = 1.1f;
        }

        // Fireflies over the Ruins' pool and terrace.
        static void RuinFireflies(LevelKit kit)
        {
            Motes(kit, new Vector3(72f, 5f, 50f), new Vector3(52f, 8f, 86f), 45, new Color(0.85f, 1f, 0.5f), 2.6f, 0.09f, new Vector3(0.12f, 0.05f, 0.1f));
            Motes(kit, new Vector3(70f, 2.5f, 14f), new Vector3(40f, 4f, 14f), 18, new Color(1f, 0.9f, 0.5f), 2.4f, 0.08f, new Vector3(0.1f, 0.06f, 0.1f));
        }

        // Pennants of the old houses hung in two rows under the roof of the Hall of Arms, between
        // the trusses.
        static void ArmsPennants(LevelKit kit)
        {
            float cx = (ArmsX0 + ArmsX1) * 0.5f;
            Material[] cloth =
            {
                PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, Wine, new Color(1f, 0.82f, 0.4f)),
                PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, Navy, CreamCol),
                PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, new Color(0.25f, 0.45f, 0.3f), new Color(1f, 0.85f, 0.5f)),
                StarCloth,
                PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, Plum, CreamCol),
            };
            int e = 0;
            for (float z = -40.5f; z <= -4f; z += 9f)
                for (int s = -1; s <= 1; s += 2)
                {
                    Banner(kit, new Vector3(cx + s * 11f, ArmsTop - 0.6f, z), 90f, 1.6f, 3.6f, cloth[e % cloth.Length], e % 7);
                    e++;
                }
        }
    }

    // A paper lantern rising from the sea into the night sky, then starting again.
    public class SkyLantern : MonoBehaviour
    {
        public Vector3 From, Drift;
        public float Rise = 60f, Speed = 1f, Phase;
        Vector3 scale;

        void Start() { scale = transform.localScale; }

        void Update()
        {
            float t = Mathf.Repeat(Time.time * Speed / Rise + Phase, 1f);
            float sway = Mathf.Sin(Time.time * 0.7f + Phase * 20f) * 1.2f;
            transform.position = From + Vector3.up * (t * Rise) + Drift * t + new Vector3(sway, 0f, sway * 0.5f);
            // Grow in off the water, shrink to a point of light high up.
            float k = Mathf.Clamp01(t / 0.06f) * (1f - Mathf.SmoothStep(0.75f, 1f, t) * 0.85f);
            transform.localScale = scale * Mathf.Max(0.01f, k);
            transform.rotation = Quaternion.Euler(Mathf.Sin(Time.time * 1.1f + Phase * 9f) * 5f, Time.time * 8f + Phase * 360f, 0f);
        }
    }
}

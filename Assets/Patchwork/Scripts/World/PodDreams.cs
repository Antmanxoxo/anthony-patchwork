using UnityEngine;

namespace Patchwork
{
    // The dreamy side of the Pod, to go with its floating song: glowing motes drifting about
    // the room, a clockwork mouse who lives aboard, and outside the window felt nebulas,
    // constellations stitched from star buttons and thread, and the odd shooting star.
    public static class PodDreams
    {
        public static void Build(LevelKit kit)
        {
            Nebulas(kit);
            StarRiver(kit);
            Auroras(kit);
            Constellations(kit);
            var sky = kit.Group("ShootingStars", Vector3.zero);
            sky.gameObject.AddComponent<ShootingStars>().Build(kit);
            var motes = kit.Group("Motes", new Vector3(0f, 4.5f, 0f));
            motes.gameObject.AddComponent<Motes>().Build(kit, 16);
            // A clockwork mouse pottering about the floor. He's shy: he scurries off if Patch
            // runs at him.
            var mouse = ToyCritter.Mouse(kit, new Vector3(-3.2f, 0f, -3.4f));
            mouse.Home = new Vector3(-0.5f, 0f, -1.5f);
            mouse.Radius = 4.2f;
        }

        // ---------------------------------------------------------------- Nebulas

        // Great soft swirls of felt far off in the dark, each a curl of overlapping patches that
        // shade from one colour to the next, glowing faintly.
        static void Nebulas(LevelKit kit)
        {
            var rng = new System.Random(11);
            Vector3[] where = { new Vector3(-70f, 34f, 120f), new Vector3(95f, -30f, 110f), new Vector3(-120f, -25f, -40f), new Vector3(60f, 80f, -90f), new Vector3(10f, -90f, 60f) };
            Color[] from = { new Color(0.85f, 0.3f, 0.65f), new Color(0.25f, 0.6f, 0.75f), new Color(0.5f, 0.35f, 0.85f), new Color(0.9f, 0.5f, 0.35f), new Color(0.3f, 0.7f, 0.6f) };
            Color[] to = { new Color(0.4f, 0.3f, 0.8f), new Color(0.45f, 0.35f, 0.8f), new Color(0.25f, 0.55f, 0.8f), new Color(0.8f, 0.3f, 0.6f), new Color(0.3f, 0.45f, 0.8f) };
            for (int nb = 0; nb < where.Length; nb++)
            {
                var g = kit.Group("Nebula", where[nb]);
                g.rotation = Quaternion.LookRotation(-where[nb].normalized, Vector3.up) * Quaternion.Euler(0f, 0f, (float)rng.NextDouble() * 360f);
                const int puffs = 11;
                for (int i = 0; i < puffs; i++)
                {
                    // A spiral arm: each patch a little further round and further out.
                    float k = i / (float)(puffs - 1);
                    float a = k * 4.4f, r = 6f + k * 34f;
                    Color c = Color.Lerp(from[nb], to[nb], k) * 0.62f;
                    float size = Mathf.Lerp(30f, 13f, k) * (0.85f + (float)rng.NextDouble() * 0.3f);
                    var t = kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, i * 0.6f), new Vector3(size, size * 0.7f, 1.2f),
                        PatchworkAssets.Glow(new Color(c.r, c.g, c.b), 1.25f), new Vector3(0f, 0f, a * Mathf.Rad2Deg + 60f), g);
                    Plain(t);
                    // A second, fainter arm curling the other way round.
                    var t2 = kit.Deco(PrimitiveType.Sphere, new Vector3(-Mathf.Cos(a) * r, -Mathf.Sin(a) * r, i * 0.6f + 0.3f), new Vector3(size * 0.8f, size * 0.55f, 1.2f),
                        PatchworkAssets.Glow(new Color(c.r * 0.7f, c.g * 0.7f, c.b * 0.7f), 1f), new Vector3(0f, 0f, a * Mathf.Rad2Deg + 60f), g);
                    Plain(t2);
                }
                // A bright heart to the swirl.
                Plain(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, -0.5f), new Vector3(11f, 11f, 1.4f), PatchworkAssets.Glow(Color.Lerp(from[nb], Color.white, 0.55f), 2.2f), default(Vector3), g));
                var spin = g.gameObject.AddComponent<Spinner>();
                spin.Axis = Vector3.forward;
                spin.Speed = (nb % 2 == 0 ? 1f : -1f) * 0.8f;
            }
        }

        // ---------------------------------------------------------------- The river of stars

        // A band of glowing dust right round the sky, like the Milky Way sewn in sequins: lots
        // of tiny bright specks, and soft pools of colour glowing among them.
        static void StarRiver(LevelKit kit)
        {
            var g = kit.Group("StarRiver", Vector3.up * 5f);
            g.rotation = Quaternion.Euler(24f, 0f, 38f);
            var rng = new System.Random(5);
            Material[] specks =
            {
                PatchworkAssets.Glow(new Color(1f, 0.96f, 0.85f), 2.6f), PatchworkAssets.Glow(new Color(0.7f, 0.85f, 1f), 2.6f),
                PatchworkAssets.Glow(new Color(1f, 0.75f, 0.9f), 2.4f), PatchworkAssets.Glow(new Color(0.75f, 1f, 0.9f), 2.4f),
            };
            for (int i = 0; i < 260; i++)
            {
                float a = (float)rng.NextDouble() * Mathf.PI * 2f;
                // Bunched towards the middle of the band.
                float off = ((float)rng.NextDouble() + (float)rng.NextDouble() + (float)rng.NextDouble() - 1.5f) * 22f;
                float d = 120f + (float)rng.NextDouble() * 30f;
                float s = 0.35f + (float)rng.NextDouble() * (float)rng.NextDouble() * 1.3f;
                Plain(kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * d, off, Mathf.Sin(a) * d), Vector3.one * s, specks[i % specks.Length], default(Vector3), g));
            }
            Color[] pools = { new Color(0.3f, 0.2f, 0.55f), new Color(0.15f, 0.35f, 0.5f), new Color(0.5f, 0.2f, 0.45f), new Color(0.2f, 0.3f, 0.6f) };
            for (int i = 0; i < 18; i++)
            {
                float a = i / 18f * Mathf.PI * 2f + (float)rng.NextDouble() * 0.2f;
                Vector3 p = new Vector3(Mathf.Cos(a) * 158f, ((float)rng.NextDouble() - 0.5f) * 14f, Mathf.Sin(a) * 158f);
                var t = kit.Deco(PrimitiveType.Sphere, p, new Vector3(44f + (float)rng.NextDouble() * 22f, 20f + (float)rng.NextDouble() * 10f, 1.5f), PatchworkAssets.Glow(pools[i % pools.Length], 0.9f), default(Vector3), g);
                t.rotation = Quaternion.LookRotation(-(g.TransformPoint(p) - g.position).normalized, g.up);
                Plain(t);
            }
            var spin = g.gameObject.AddComponent<Spinner>();
            spin.Speed = 0.35f;
        }

        // ---------------------------------------------------------------- Auroras

        // Ribbons of light hanging in the dark: curtains of thin glowing strips, each a slightly
        // different height and shade, that sway very slowly.
        static void Auroras(LevelKit kit)
        {
            Vector3[] where = { new Vector3(-30f, 46f, 95f), new Vector3(70f, 30f, 60f), new Vector3(-90f, 50f, -30f) };
            Color[] low = { new Color(0.2f, 0.9f, 0.7f), new Color(0.45f, 0.6f, 1f), new Color(0.9f, 0.45f, 0.8f) };
            Color[] high = { new Color(0.5f, 0.45f, 1f), new Color(0.85f, 0.4f, 0.9f), new Color(0.45f, 0.7f, 1f) };
            for (int n = 0; n < where.Length; n++)
            {
                var g = kit.Group("Aurora", where[n]);
                g.rotation = Quaternion.LookRotation(-where[n].normalized, Vector3.up);
                const int strips = 34;
                for (int i = 0; i < strips; i++)
                {
                    float k = i / (float)(strips - 1);
                    // The curtain hangs in a lazy S.
                    float x = (k - 0.5f) * 70f, z = Mathf.Sin(k * 6.3f) * 9f;
                    float tall = 10f + Mathf.Sin(k * 9f + n) * 4f + Mathf.Sin(k * 23f) * 2f;
                    Color c = Color.Lerp(low[n], high[n], Mathf.Sin(k * Mathf.PI)) * (0.35f + 0.25f * Mathf.Sin(k * 17f + n * 2f));
                    Plain(kit.Deco(PrimitiveType.Cube, new Vector3(x, tall * 0.5f, z), new Vector3(1.5f, tall, 0.15f), PatchworkAssets.Glow(new Color(c.r, c.g, c.b), 1.8f), new Vector3(0f, Mathf.Cos(k * 6.3f) * 35f, 0f), g));
                }
                var sway = g.gameObject.AddComponent<Rocker>();
                sway.Axis = Vector3.up;
                sway.Amount = 3f;
                sway.Speed = 0.25f + n * 0.05f;
            }
        }

        // ---------------------------------------------------------------- Constellations

        // Pictures in the stars, sewn the way you'd embroider them: a glowing button at each
        // point and a line of running stitches from one to the next.
        static void Constellations(LevelKit kit)
        {
            // Points are on a little grid (x right, y up); lines join them in order, and a
            // negative number lifts the needle (starts a new line).
            float[][] shapes =
            {
                new[] { 0f, 0f,  -2f, 2f,  -3f, 4f,  -1.5f, 5.2f,  0f, 4f,  1.5f, 5.2f,  3f, 4f,  2f, 2f,  0f, 0f },                       // a heart
                new[] { -3f, -3f,  3f, 3f,  -99f, 0f,  3f, 3f,  3.6f, 4.4f,  2.4f, 5.2f,  1.2f, 4.4f,  0.2f, 2.6f,  -1.4f, 2.2f },         // a needle and its thread
                new[] { 0f, -3f,  -2.4f, -1f,  -2.4f, 2f,  0f, 4f,  2.4f, 2f,  2.4f, -1f,  0f, -3f,  -99f, 0f,  -2.4f, 2f,  -3.4f, 3.6f,  -1.6f, 3.4f,  -99f, 0f,  2.4f, 2f,  3.4f, 3.6f,  1.6f, 3.4f },   // a teddy's head
                new[] { 0f, 5f,  -1.6f, 2f,  -1.6f, -2f,  1.6f, -2f,  1.6f, 2f,  0f, 5f,  -99f, 0f,  -1.6f, -2f,  -3f, -4f,  -99f, 0f,  1.6f, -2f,  3f, -4f },   // a rocket
                new[] { -3f, 0f,  -1f, 1.6f,  1f, 0f,  3f, 1.6f,  -99f, 0f,  -3f, -2f,  -1f, -0.4f,  1f, -2f,  3f, -0.4f },                // waves
            };
            Vector3[] dirs = { new Vector3(-0.55f, 0.5f, 0.67f), new Vector3(0.62f, 0.42f, 0.66f), new Vector3(-0.8f, 0.3f, -0.5f), new Vector3(0.7f, 0.55f, -0.45f), new Vector3(0.1f, 0.85f, 0.5f) };
            Color[] cols = { new Color(1f, 0.6f, 0.75f), new Color(1f, 0.9f, 0.5f), new Color(1f, 0.78f, 0.5f), new Color(0.6f, 0.85f, 1f), new Color(0.6f, 1f, 0.85f) };
            for (int s = 0; s < shapes.Length; s++)
            {
                Vector3 dir = dirs[s].normalized;
                var g = kit.Group("Constellation", dir * 85f + Vector3.up * 5f);
                g.rotation = Quaternion.LookRotation(dir, Vector3.up);
                var star = PatchworkAssets.Glow(cols[s], 2.2f);
                var thread = PatchworkAssets.Glow(Color.Lerp(cols[s], Color.white, 0.5f), 1.1f);
                const float unit = 3.4f;
                var pts = shapes[s];
                bool have = false;
                Vector3 last = Vector3.zero;
                for (int i = 0; i + 1 < pts.Length; i += 2)
                {
                    if (pts[i] < -50f) { have = false; continue; }
                    Vector3 p = new Vector3(pts[i], pts[i + 1], 0f) * unit;
                    Plain(kit.Deco(PrimitiveType.Cylinder, p, new Vector3(1.5f, 0.2f, 1.5f), star, new Vector3(90f, 0f, 0f), g));
                    if (have)
                    {
                        // Running stitches from the last button to this one.
                        Vector3 d = p - last;
                        int dashes = Mathf.Max(2, Mathf.RoundToInt(d.magnitude / 1.7f));
                        float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
                        for (int k = 0; k < dashes; k++)
                            Plain(kit.Deco(PrimitiveType.Cube, last + d * ((k + 0.5f) / dashes), new Vector3(d.magnitude / dashes * 0.55f, 0.22f, 0.1f), thread, new Vector3(0f, 0f, ang), g));
                    }
                    last = p;
                    have = true;
                }
            }
        }

        static void Plain(Transform t)
        {
            var r = t.GetComponent<Renderer>();
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            t.gameObject.layer = 1;   // never distance-culled: it's always a long way off
        }
    }

    // Little glowing specks drifting slowly round the room, like dust in a sunbeam or sleepy
    // fireflies. Each wanders on its own lazy loop and pulses gently.
    public class Motes : MonoBehaviour
    {
        Transform[] mote;
        Vector3[] centre, reach;
        float[] phase, size;

        public void Build(LevelKit kit, int count)
        {
            mote = new Transform[count];
            centre = new Vector3[count];
            reach = new Vector3[count];
            phase = new float[count];
            size = new float[count];
            Color[] cols = { new Color(1f, 0.92f, 0.6f), new Color(0.7f, 0.9f, 1f), new Color(1f, 0.75f, 0.9f) };
            for (int i = 0; i < count; i++)
            {
                float a = Random.value * Mathf.PI * 2f, r = 1.5f + Random.value * 5.5f;
                centre[i] = new Vector3(Mathf.Cos(a) * r, -3.3f + Random.value * 6.5f, Mathf.Sin(a) * r);
                reach[i] = new Vector3(0.6f + Random.value * 1.2f, 0.3f + Random.value * 0.6f, 0.6f + Random.value * 1.2f);
                phase[i] = Random.value * 20f;
                size[i] = 0.05f + Random.value * 0.05f;
                mote[i] = kit.Deco(PrimitiveType.Sphere, centre[i], Vector3.one * size[i], PatchworkAssets.Glow(cols[i % cols.Length], 2.4f), default(Vector3), transform);
                var rd = mote[i].GetComponent<Renderer>();
                rd.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                rd.receiveShadows = false;
            }
        }

        void Update()
        {
            if (mote == null) return;
            float t = Time.time;
            for (int i = 0; i < mote.Length; i++)
            {
                float p = phase[i] + t * (0.18f + (i % 5) * 0.03f);
                mote[i].localPosition = centre[i] + new Vector3(Mathf.Sin(p) * reach[i].x, Mathf.Sin(p * 1.7f + 1f) * reach[i].y, Mathf.Cos(p * 0.8f) * reach[i].z);
                mote[i].localScale = Vector3.one * size[i] * (0.7f + 0.5f * Mathf.Sin(p * 3.1f));
            }
        }
    }

    // Every so often a shooting star streaks across the dark outside: a bright head with a
    // tapering tail, gone in a second.
    public class ShootingStars : MonoBehaviour
    {
        Transform star, tail;
        float wait = 2f, age = -1f, life;
        Vector3 from, to;

        public void Build(LevelKit kit)
        {
            var g = kit.Group("ShootingStar", Vector3.zero);
            g.SetParent(transform, false);
            star = g;
            var head = kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 1.3f, PatchworkAssets.Glow(new Color(1f, 0.97f, 0.85f), 4f), default(Vector3), g);
            tail = kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, -6f), new Vector3(0.6f, 0.6f, 12f), PatchworkAssets.Glow(new Color(1f, 0.85f, 0.55f), 2.6f), default(Vector3), g);
            foreach (var t in new[] { head, tail })
            {
                var r = t.GetComponent<Renderer>();
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
                t.gameObject.layer = 1;
            }
            g.gameObject.SetActive(false);
        }

        void Update()
        {
            if (star == null) return;
            float dt = Time.deltaTime;
            if (age < 0f)
            {
                wait -= dt;
                if (wait > 0f) return;
                // Somewhere out in the dark, more often in front of the window than behind.
                Vector3 dir = Random.onUnitSphere;
                if (Random.value < 0.75f) dir = new Vector3(Random.Range(-0.6f, 0.6f), Random.Range(0f, 0.5f), 1f).normalized;
                Vector3 mid = dir * Random.Range(45f, 70f) + Vector3.up * 5f;
                Vector3 across = Vector3.Cross(dir, Random.onUnitSphere).normalized;
                across.y = -Mathf.Abs(across.y) - 0.25f;   // always falling a little
                across.Normalize();
                from = mid - across * 28f;
                to = mid + across * 28f;
                life = Random.Range(1.1f, 1.7f);
                age = 0f;
                star.rotation = Quaternion.LookRotation(across, Vector3.up);
                star.gameObject.SetActive(true);
            }
            age += dt;
            float k = age / life;
            if (k >= 1f)
            {
                star.gameObject.SetActive(false);
                age = -1f;
                wait = Random.Range(2.5f, 6f);
                return;
            }
            star.position = Vector3.Lerp(from, to, k);
            // Swells in, burns out.
            float s = Mathf.Sin(k * Mathf.PI);
            star.localScale = Vector3.one * Mathf.Max(0.01f, s);
        }
    }
}

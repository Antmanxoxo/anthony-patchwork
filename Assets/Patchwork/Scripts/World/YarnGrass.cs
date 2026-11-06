using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Soft ground cover for grassy levels: tufts of yarn grass, felt patches sewn onto the ground
    // with running stitches, and little pebbles, clover and pompom flowers in the grass.
    // Everything is batched into a few meshes per 16 m square, so it's cheap to draw.
    // Call it last when building a level: it only grows on open grass (it looks down from above
    // at each spot, and skips anything that isn't grass at the right height: streets, houses,
    // patches, platforms).
    public static class YarnGrass
    {
        const float Chunk = 16f;

        // Spots to keep bare (x, z, radius): paths, ponds, train track, flower beds.
        // Cleared at the start of each level build.
        public static readonly List<Vector3> ClearZones = new List<Vector3>();

        static bool OnGrass(Vector3 p, float groundY, out float y)
        {
            y = groundY;
            for (int i = 0; i < ClearZones.Count; i++)
            {
                var c = ClearZones[i];
                float dx = p.x - c.x, dz = p.z - c.y;
                if (dx * dx + dz * dz < c.z * c.z) return false;
            }
            RaycastHit hit;
            if (!Physics.Raycast(new Vector3(p.x, groundY + 40f, p.z), Vector3.down, out hit, 41f, ~0, QueryTriggerInteraction.Ignore)) return false;
            if (Mathf.Abs(hit.point.y - groundY) > 0.06f) return false;
            var r = hit.collider.GetComponent<Renderer>();
            if (r == null || r.sharedMaterial == null) return false;
            string n = r.sharedMaterial.name;
            if (!(n.Contains("grass") || n.Contains("knit"))) return false;
            y = hit.point.y;
            return true;
        }

        class Bins
        {
            readonly Dictionary<long, MeshBatch> map = new Dictionary<long, MeshBatch>();
            public MeshBatch Get(float x, float z, int kind)
            {
                long cx = Mathf.FloorToInt(x / Chunk) + 1000, cz = Mathf.FloorToInt(z / Chunk) + 1000;
                long key = (cx * 4000L + cz) * 16L + kind;
                MeshBatch b;
                if (!map.TryGetValue(key, out b)) { b = new MeshBatch(); map[key] = b; }
                return b;
            }
            public IEnumerable<KeyValuePair<long, MeshBatch>> All { get { return map; } }
        }

        // Thick, colourful yarn grass over all the open grass in [min, max] (x, z), like a meadow
        // knitted from odds and ends: big drifts of one colour fading into the next. density is
        // tufts per m² (scaled down on the lower graphics settings). Built in 16 m squares on its
        // own layer, so the camera stops drawing it further than ~70 m away.
        public static void Scatter(LevelKit kit, Vector2 min, Vector2 max, float groundY, float density, Color[] colours, Color[] flowers, int seed)
        {
            Physics.SyncTransforms();
            var rng = new System.Random(seed);
            System.Func<float> R = () => (float)rng.NextDouble();
            var bins = new Bins();
            float quality = GameSettings.Graphics == 0 ? 0.45f : GameSettings.Graphics == 1 ? 0.7f : 1f;
            float step = 1f / Mathf.Sqrt(Mathf.Max(0.05f, density * quality));
            float ox = seed * 3.17f, oz = seed * 1.91f;
            int flowerKind = colours.Length;
            int pebbleKind = flowerKind + 1;
            int cloverKind = flowerKind + 2;
            for (float x = min.x; x < max.x; x += step)
                for (float z = min.y; z < max.y; z += step)
                {
                    float px = x + (R() - 0.5f) * step, pz = z + (R() - 0.5f) * step;
                    // Nearly everywhere, with just the odd bare spot.
                    float clump = Mathf.PerlinNoise(px * 0.08f + ox, pz * 0.08f + oz);
                    if (clump < 0.2f && R() < 0.7f) continue;
                    float y;
                    if (!OnGrass(new Vector3(px, 0f, pz), groundY, out y)) continue;
                    Vector3 root = new Vector3(px, y - 0.02f, pz);
                    // Colour drifts: a slow noise picks the colour, a faster one blends the edges.
                    float drift = Mathf.PerlinNoise(px * 0.035f + 11f + ox, pz * 0.035f + 5f + oz) * 1.35f - 0.1f;
                    drift += (Mathf.PerlinNoise(px * 0.4f + oz, pz * 0.4f + ox) - 0.5f) * 0.18f;
                    int main = Mathf.Clamp(Mathf.FloorToInt(Mathf.Repeat(drift, 1f) * colours.Length), 0, colours.Length - 1);
                    int blades = 5 + (int)(R() * 3f);
                    float tall = 0.34f + clump * 0.26f + R() * 0.14f;
                    for (int i = 0; i < blades; i++)
                    {
                        // Most strands the drift's colour, a few from the next skein along.
                        int c = R() < 0.8f ? main : (main + 1 + (int)(R() * 2f)) % colours.Length;
                        float a = R() * Mathf.PI * 2f;
                        Vector3 off = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (0.03f + R() * 0.16f);
                        Vector3 lean = off.normalized * (0.08f + R() * 0.18f) + new Vector3(0.05f, 0f, 0.03f);
                        bins.Get(px, pz, c).Blade(root + off, tall * (0.65f + R() * 0.55f), 0.11f, lean, R() * 360f);
                    }
                    float extra = R();
                    if (extra < 0.02f && flowers != null && flowers.Length > 0)
                    {
                        // A pompom flower on a stalk poking out of the tuft.
                        int fc = (int)(R() * flowers.Length) % flowers.Length;
                        bins.Get(px, pz, cloverKind).Blade(root, tall + 0.22f, 0.05f, Vector3.zero, 0f);
                        bins.Get(px, pz, flowerKind + 3 + fc).Ball(root + Vector3.up * (tall + 0.26f), new Vector3(0.18f, 0.15f, 0.18f));
                    }
                    else if (extra < 0.035f)
                    {
                        bins.Get(px, pz, pebbleKind).Ball(root + new Vector3(0.2f, 0.04f, 0.1f), new Vector3(0.22f + R() * 0.15f, 0.1f, 0.18f + R() * 0.1f), 3, 5);
                    }
                }

            var parent = kit.Group("YarnGrass", Vector3.zero);
            var pebble = PatchworkAssets.Cobble(new Color(0.8f, 0.78f, 0.74f));
            var clover = PatchworkAssets.Felt(new Color(0.38f, 0.6f, 0.35f));
            foreach (var kv in bins.All)
            {
                int kind = (int)(kv.Key % 16L);
                Material m;
                if (kind < colours.Length) m = PatchworkAssets.Yarn(colours[kind]);
                else if (kind == pebbleKind) m = pebble;
                else if (kind == cloverKind) m = clover;
                else m = PatchworkAssets.Felt(flowers[(kind - flowerKind - 3) % flowers.Length]);
                var t = kv.Value.Build(parent, "Grass", m, false);
                if (t != null) t.gameObject.layer = RenderOptimizer.GrassLayer;
            }
        }

        // Felt and fabric patches sewn onto the grass with cream running stitches, like a quilt
        // laid over the ground. Each patch is a few mm higher than the last so overlaps never flicker.
        public static void Patches(LevelKit kit, Vector2 min, Vector2 max, float groundY, int count, Color[] colors, int seed)
        {
            Physics.SyncTransforms();
            var rng = new System.Random(seed);
            System.Func<float> R = () => (float)rng.NextDouble();
            var stitches = new MeshBatch();
            int made = 0;
            for (int tries = 0; tries < count * 6 && made < count; tries++)
            {
                Vector3 c = new Vector3(Mathf.Lerp(min.x, max.x, R()), 0f, Mathf.Lerp(min.y, max.y, R()));
                float size = 1.4f + R() * 2.2f;
                float y;
                if (!OnGrass(c, groundY, out y)) continue;
                bool ok = true;
                for (int i = 0; i < 4 && ok; i++)
                {
                    float a = i * Mathf.PI * 0.5f + 0.4f, dummy;
                    ok = OnGrass(c + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * size, groundY, out dummy);
                }
                if (!ok) continue;
                int n = 5 + (int)(R() * 3f);
                float spin = R() * 360f;
                var pts = new Vector2[n];
                for (int i = 0; i < n; i++)
                {
                    float a = (i / (float)n * 360f + spin) * Mathf.Deg2Rad;
                    float rr = size * (0.8f + R() * 0.25f);
                    pts[i] = new Vector2(c.x + Mathf.Cos(a) * rr, c.z + Mathf.Sin(a) * rr);
                }
                float top = groundY + 0.012f + (made % 6) * 0.004f;
                Color col = colors[(int)(R() * colors.Length) % colors.Length];
                Material mat = made % 2 == 0 ? PatchworkAssets.Felt(col) : PatchworkAssets.Fabric(col);
                kit.Prism(pts, groundY - 0.05f, top, mat, "GroundPatch", 1.5f);
                // Stitches just inside the edge.
                for (int i = 0; i < n; i++)
                {
                    Vector2 p = Vector2.Lerp(pts[i], new Vector2(c.x, c.z), 0.12f), q = Vector2.Lerp(pts[(i + 1) % n], new Vector2(c.x, c.z), 0.12f);
                    stitches.Stitches(new Vector3(p.x, top + 0.008f, p.y), new Vector3(q.x, top + 0.008f, q.y));
                }
                made++;
            }
            stitches.Build(kit.Group("PatchStitches", Vector3.zero), "Stitches", PatchworkAssets.Yarn(new Color(0.97f, 0.93f, 0.84f)));
        }
    }
}

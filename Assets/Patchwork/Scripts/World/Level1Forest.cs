using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The Whispering Wood now fills the whole south-west of the toybox (west of the ravine),
    // in three bands that get deeper and stranger the further in you go:
    //   Birch Edge (north)      - white stitched birches, bramble bushes with berries, bluebells
    //                             and the sleepy soldier's campsite. Lighter, friendlier.
    //   The Middle Wood         - pines and twisted trees, glowing crystals, drifting seed-lights,
    //                             the old forgotten toys, the Wishing Willow (where the Moonlit
    //                             Needle rests) and, at its heart, the Moonglow Glade.
    //   The Deep Wood (corner)  - the darkest part: the owl's hollow oak and its spirit lanterns.
    // Ways in: stairs up out of the ravine's west bank, and a footbridge from the orchard.
    public static partial class Level1Builder
    {
        static readonly Vector3 MoonGladeCenter = new Vector3(-90f, 0f, -50f);
        static readonly Vector3 WillowSpot = new Vector3(-108f, 0f, -56f);
        static readonly Vector3 CampSpot = new Vector3(-99f, 0f, -16f);
        static readonly Bounds ForestArea = new Bounds(new Vector3(-83f, 8f, -69.5f), new Vector3(64f, 36f, 91f));
        static readonly List<Vector3[]> lanes = new List<Vector3[]>();

        static bool InForest(Vector3 p) { return p.x < -51f && p.z < -8f; }

        // Paths inside the wood (drawn with mossy stones; kept clear of trees).
        static readonly Vector3[][] ForestPaths =
        {
            new[] { new Vector3(-53f, 0f, -29f), new Vector3(-62f, 0f, -31f), new Vector3(-75f, 0f, -38f), new Vector3(-85f, 0f, -45.5f) },
            new[] { new Vector3(-62f, 0f, -31f), new Vector3(-74f, 0f, -24f), new Vector3(-86f, 0f, -20f), new Vector3(-95f, 0f, -18f) },
            new[] { new Vector3(-96f, 0f, -52f), new Vector3(-101f, 0f, -54f), new Vector3(-104f, 0f, -55.5f) },
            new[] { new Vector3(-91f, 0f, -60f), new Vector3(-93f, 0f, -68f), new Vector3(-90f, 0f, -77f), new Vector3(-85f, 0f, -87f) },
            new[] { new Vector3(-75f, 0f, -38f), new Vector3(-70f, 0f, -48f), new Vector3(-68f, 0f, -57f) },
        };

        static void Lane(Vector3[] line) { lanes.Add(line); }

        static bool NearLane(Vector3 p, float r)
        {
            foreach (var line in lanes)
                for (int i = 0; i < line.Length - 1; i++)
                {
                    Vector3 a = line[i], b = line[i + 1];
                    Vector2 ab = new Vector2(b.x - a.x, b.z - a.z), ap = new Vector2(p.x - a.x, p.z - a.z);
                    float k = Mathf.Clamp01(Vector2.Dot(ap, ab) / Mathf.Max(0.001f, ab.sqrMagnitude));
                    if ((ap - ab * k).magnitude < r + 1.5f) return true;
                }
            return false;
        }

        // ---------------------------------------------------------------- Getting in

        static void RavineCrossings(LevelKit kit)
        {
            var stone = PatchworkAssets.Cobble(TownKit.Stone);
            // Stairs up the ravine's west wall to the Birch Edge (cross the stream on the
            // stepping stones a little further south).
            TownKit.Stairs(kit, new Vector3(RavineWest + 1.5f, -5f, -22f), 180f, 5, 1f, 1.4f, 2.6f, stone);
            // A rope-railed footbridge from the orchard to the wood.
            TownKit.Bridge(kit, new Vector3(RavineEast + 0.5f, 0.05f, -92f), new Vector3(RavineWest - 0.5f, 0.05f, -92f), 3.5f, 1.4f);
            // Lanterns at both ends of the bridge.
            foreach (var x in new[] { RavineEast + 1.8f, RavineWest - 1.8f })
                for (int s = -1; s <= 1; s += 2)
                    LanternPost(kit, new Vector3(x, 0f, -92f + s * 2.4f));
            LanternPost(kit, new Vector3(-54.5f, 0f, -26f));
            // And a little footbridge over the canal, so the Pod's meadow joins the orchard.
            TownKit.Bridge(kit, new Vector3(CanalWest - 0.5f, 0.05f, -97f), new Vector3(CanalEast + 0.5f, 0.05f, -97f), 3f, 0.7f);
        }

        static void LanternPost(LevelKit kit, Vector3 feet)
        {
            var g = kit.Group("LanternPost", feet);
            var wood = PatchworkAssets.Wood(new Color(0.42f, 0.32f, 0.25f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.1f, 0f), new Vector3(0.14f, 1.1f, 0.14f), wood, default(Vector3), g);
            Rod(kit, new Vector3(0f, 2.1f, 0f), new Vector3(0.5f, 2.25f, 0f), wood, 0.07f, g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.5f, 1.9f, 0f), new Vector3(0.3f, 0.38f, 0.3f), PatchworkAssets.Glow(new Color(1f, 0.75f, 0.4f), 1.8f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.5f, 2.1f, 0f), new Vector3(0.2f, 0.03f, 0.2f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), default(Vector3), g);
            Trunk(g, 0.15f, 2.2f);
        }

        // ---------------------------------------------------------------- The wood, all of it

        static void ForestExpansion(LevelKit kit)
        {
            var rng = new System.Random(404);
            // Keep room for the clearings and the old toys that already live here.
            Take(MoonGladeCenter, 12.5f);
            Take(WillowSpot, 8.5f);
            Take(CampSpot, 6f);
            Vector3[] toys = { new Vector3(-66f, 0f, -52f), new Vector3(-74f, 0f, -60f), new Vector3(-58f, 0f, -66f), new Vector3(-70f, 0f, -63f), new Vector3(-79.5f, 0f, -72f), new Vector3(-77f, 0f, -68.5f) };
            float[] toyR = { 3.5f, 2.5f, 2.5f, 2.5f, 4.5f, 2.5f };
            for (int i = 0; i < toys.Length; i++) Take(toys[i], toyR[i]);

            var wood = kit.Group("ForestFloor", Vector3.zero);
            // Floors: lighter at the edge, darker the deeper you go (the deep corner has its own).
            kit.Deco(PrimitiveType.Cube, new Vector3(-83.1f, 0.01f, -23f), new Vector3(63.8f, 0.02f, 30f), PatchworkAssets.Felt(new Color(0.42f, 0.56f, 0.4f)), default(Vector3), wood);
            kit.Deco(PrimitiveType.Cube, new Vector3(-83.1f, 0.01f, -54.75f), new Vector3(63.8f, 0.02f, 33.5f), PatchworkAssets.Felt(new Color(0.31f, 0.43f, 0.35f)), default(Vector3), wood);
            kit.Deco(PrimitiveType.Cube, new Vector3(-61.6f, 0.01f, -93f), new Vector3(20.8f, 0.02f, 43f), PatchworkAssets.Felt(new Color(0.28f, 0.39f, 0.32f)), default(Vector3), wood);
            // Mossy patches and fallen leaves to break the floor up.
            Color[] moss = { new Color(0.36f, 0.5f, 0.36f), new Color(0.26f, 0.36f, 0.3f), new Color(0.55f, 0.48f, 0.3f) };
            for (int i = 0; i < 26; i++)
            {
                Vector3 m = new Vector3(-113f + (float)rng.NextDouble() * 60f, 0.024f, -110f + (float)rng.NextDouble() * 100f);
                kit.Deco(PrimitiveType.Cylinder, m, new Vector3(3f + i % 4, 0.01f, 2.5f + i % 3), PatchworkAssets.Felt(moss[i % moss.Length]), new Vector3(0f, i * 53f, 0f), wood);
            }
            var leafCols = new[] { new Color(0.9f, 0.55f, 0.25f), new Color(0.85f, 0.35f, 0.3f), new Color(0.95f, 0.78f, 0.35f) };
            for (int i = 0; i < 60; i++)
            {
                Vector3 p = new Vector3(-113f + (float)rng.NextDouble() * 60f, 0.035f, -40f + (float)rng.NextDouble() * 31f);
                kit.Deco(PrimitiveType.Sphere, p, new Vector3(0.3f, 0.01f, 0.18f), PatchworkAssets.Felt(leafCols[i % 3]), new Vector3(0f, i * 37f, 0f), wood);
            }

            // Mossy stepping stones along the paths inside the wood.
            var mossStone = new[] { PatchworkAssets.Cobble(new Color(0.55f, 0.6f, 0.55f)), PatchworkAssets.Cobble(new Color(0.48f, 0.52f, 0.5f)) };
            int n = 0;
            foreach (var line in ForestPaths)
                for (int s = 0; s < line.Length - 1; s++)
                {
                    int steps = Mathf.CeilToInt(Vector3.Distance(line[s], line[s + 1]) / 2.2f);
                    for (int k = 0; k < steps; k++)
                    {
                        Vector3 p = Vector3.Lerp(line[s], line[s + 1], k / (float)steps) + new Vector3(Mathf.Sin(n * 1.7f) * 0.35f, 0.03f, Mathf.Cos(n * 1.3f) * 0.35f);
                        n++;
                        if (Taken(p, 0.3f)) continue;
                        kit.Deco(PrimitiveType.Cylinder, p, new Vector3(1.25f, 0.03f, 1f), mossStone[n % 2], new Vector3(0f, n * 43f, 0f), wood);
                        if (n % 5 == 0) kit.Deco(PrimitiveType.Sphere, p + new Vector3(0.55f, 0.02f, 0.2f), new Vector3(0.5f, 0.06f, 0.4f), PatchworkAssets.Felt(moss[0]), default(Vector3), wood);
                    }
                }

            // A sign arch where the stairs come up from the ravine.
            Gate(kit, new Vector3(-56f, 0f, -29.5f), 95f);
            kit.Hint(new Vector3(-58f, 1.5f, -29.5f), new Vector3(5f, 4f, 5f), "The Whispering Wood",
                "The old wood is full of magic. Deep inside, the Gloom has smothered the Moonglow Glade... and something glimmers under the wishing willow.");

            BuildMoonGlade(kit);
            WishingWillow(kit);

            // Trees, band by band.
            for (float x = -113f; x <= -53f; x += 5f)
                for (float z = -112f; z <= -11f; z += 5f)
                {
                    if (x < -72f && z < -72f) continue;   // the deep corner has its own trees
                    Vector3 near = new Vector3(x + (float)(rng.NextDouble() - 0.5) * 3f, 0f, z + (float)(rng.NextDouble() - 0.5) * 3f);
                    bool edge = near.z > -38f;
                    // Thinner at the north edge and along the ravine, so it fades in.
                    if (near.z > -16f && rng.NextDouble() < 0.5) continue;
                    if (near.x > -57f && rng.NextDouble() < 0.4) continue;
                    if (NearLane(near, 1.6f)) continue;
                    // Pick the tree first, then find it a spot with room for its crown.
                    double roll = rng.NextDouble();
                    float h = (float)rng.NextDouble();
                    Vector3 p;
                    if (edge)
                    {
                        if (roll < 0.55) { if (Spot(near, BirchSpread, out p)) Birch(kit, p, 10f + h * 5f, rng); }
                        else if (roll < 0.8) { if (Spot(near, BrambleSpread, out p)) Bramble(kit, p, rng); }
                        else { float ph = 11f + h * 4f; if (Spot(near, PineSpread(ph), out p)) SpookyPine(kit, p, ph, rng); }
                    }
                    else
                    {
                        if (roll < 0.55) { float ph = 13f + h * 6f; if (Spot(near, PineSpread(ph), out p)) SpookyPine(kit, p, ph, rng); }
                        else if (roll < 0.8) { if (Spot(near, GnarlSpread, out p)) GnarledTree(kit, p, 8.5f + h * 3.5f, rng, rng.NextDouble() < 0.3); }
                        else if (roll < 0.9) { if (Spot(near, BirchSpread, out p)) Birch(kit, p, 11f + h * 4f, rng); }
                        else { if (Spot(near, OakSpread, out p)) GiantOak(kit, p, 19f + h * 4f, rng); }
                    }
                }

            // Bluebells at the edge, crystals and seed-lights in the middle.
            for (int i = 0; i < 26; i++)
            {
                Vector3 near = new Vector3(-112f + (float)rng.NextDouble() * 58f, 0f, -36f + (float)rng.NextDouble() * 26f);
                if (Taken(near, 1f) || NearLane(near, 0.5f)) continue;
                Take(near, 1f);
                for (int k = 0; k < 4; k++)
                    kit.Flower(near + new Vector3(Mathf.Cos(k * 1.6f) * 0.7f, 0f, Mathf.Sin(k * 1.6f) * 0.7f), k % 3 == 0 ? new Color(0.75f, 0.68f, 1f) : new Color(0.45f, 0.55f, 0.95f));
            }
            Color[] crystal = { new Color(0.7f, 0.5f, 1f), new Color(0.45f, 0.9f, 0.95f), new Color(1f, 0.55f, 0.85f) };
            for (int i = 0; i < 16; i++)
            {
                Vector3 near = new Vector3(-112f + (float)rng.NextDouble() * 58f, 0f, -71f + (float)rng.NextDouble() * 33f);
                if (NearLane(near, 0.5f) || !Spot(near, 1f, out Vector3 p)) continue;
                Crystals(kit, p, crystal[i % crystal.Length], i);
            }
            var seeds = kit.Group("SeedLights", Vector3.zero);
            var seedMat = PatchworkAssets.Glow(new Color(0.8f, 1f, 0.9f), 2.4f);
            for (int i = 0; i < 40; i++)
            {
                Vector3 p = new Vector3(-112f + (float)rng.NextDouble() * 58f, 0.8f + (float)rng.NextDouble() * 4f, -72f + (float)rng.NextDouble() * 60f);
                var m = kit.Deco(PrimitiveType.Sphere, p, Vector3.one * 0.12f, seedMat, default(Vector3), seeds);
                m.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            seeds.gameObject.AddComponent<FireflySwarm>().Wander = 1.8f;
            for (int i = 0; i < 6; i++)
            {
                Vector3 p = new Vector3(-111f + (float)rng.NextDouble() * 55f, 0.6f + (float)rng.NextDouble() * 1.8f, -70f + (float)rng.NextDouble() * 30f);
                if (!Taken(p, 0.6f)) EyesInTheDark(kit, p, (float)rng.NextDouble() * 360f, new Color(1f, 0.92f, 0.45f));
            }
            Cobweb(kit, new Vector3(-70f, 2.6f, -44f), 1.6f, 30f);
            Cobweb(kit, new Vector3(-104f, 2.8f, -66f), 1.8f, 160f);

            // Music and look for the whole wood (but not the sunny Birch Edge's northern rim).
            MusicPlayer.SetSpookyArea(SceneName, ForestArea);
            SpookyLook.Create(kit.Root);
            var sounds = kit.Group("ForestSounds", Vector3.zero);
            AmbientEmitter.Create(sounds, new Vector3(-84f, 2f, -52f), Sfx.Ambient(Sfx.AmbientClip.Woods), 0.5f, 10f, 30f);
            AmbientEmitter.Create(sounds, new Vector3(-90f, 2f, -28f), Sfx.Ambient(Sfx.AmbientClip.Woods), 0.35f, 8f, 26f);
        }

        // A white felt birch with black stitch marks, thin branches and pale, light leaves.
        static void Birch(LevelKit kit, Vector3 feet, float h, System.Random rng)
        {
            var g = kit.Group("Birch", feet);
            g.rotation = Quaternion.Euler((float)(rng.NextDouble() - 0.5) * 6f, (float)rng.NextDouble() * 360f, (float)(rng.NextDouble() - 0.5) * 6f);
            var bark = PatchworkAssets.Felt(new Color(0.95f, 0.93f, 0.88f));
            var mark = PatchworkAssets.Felt(new Color(0.18f, 0.16f, 0.18f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, h * 0.4f, 0f), new Vector3(0.46f, h * 0.4f, 0.46f), bark, default(Vector3), g);
            for (int k = 0; k < 8; k++)
            {
                float y = 0.5f + k * h * 0.09f;
                float a = k * 77f;
                kit.Deco(PrimitiveType.Cube, Quaternion.Euler(0f, a, 0f) * new Vector3(0f, y, 0.17f), new Vector3(0.18f + (k % 3) * 0.06f, 0.04f, 0.03f), mark, new Vector3(0f, a, 0f), g);
            }
            Color[] leaf = { new Color(0.7f, 0.85f, 0.45f), new Color(0.82f, 0.88f, 0.5f), new Color(0.95f, 0.82f, 0.4f) };
            for (int k = 0; k < 5; k++)
            {
                float a = k * 72f + (float)rng.NextDouble() * 30f;
                Vector3 from = new Vector3(0f, h * (0.55f + k * 0.06f), 0f);
                Vector3 to = from + Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0.6f, 1.1f);
                Rod(kit, from, to, bark, 0.08f, g);
                kit.Deco(PrimitiveType.Sphere, to + Vector3.up * 0.2f, new Vector3(1.6f, 1.1f, 1.5f), PatchworkAssets.Felt(leaf[(k + (int)(h * 3f)) % leaf.Length]), new Vector3(0f, a, 0f), g);
            }
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, h * 0.9f, 0f), new Vector3(1.8f, 1.5f, 1.8f), PatchworkAssets.Felt(leaf[0]), default(Vector3), g);
            Trunk(g, 0.25f, h);
        }

        // A round bramble bush with berries.
        static void Bramble(LevelKit kit, Vector3 feet, System.Random rng)
        {
            var g = kit.Group("Bramble", feet);
            var leaf = PatchworkAssets.Knit(new Color(0.3f, 0.5f, 0.32f));
            var berry = PatchworkAssets.Plain(rng.NextDouble() < 0.5 ? new Color(0.55f, 0.18f, 0.35f) : new Color(0.85f, 0.2f, 0.25f), 0.85f);
            for (int k = 0; k < 4; k++)
            {
                float a = k * 1.6f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.5f, 0.6f + (k % 2) * 0.2f, Mathf.Sin(a) * 0.5f), Vector3.one * (1.2f + (k % 2) * 0.3f), leaf, default(Vector3), g);
            }
            for (int k = 0; k < 9; k++)
            {
                Vector3 d = Quaternion.Euler(-20f - (k % 3) * 25f, k * 40f, 0f) * Vector3.forward;
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.7f, 0f) + d * 0.95f, Vector3.one * 0.13f, berry, default(Vector3), g);
            }
            Trunk(g, 0.7f, 1.4f);
        }

        // A cluster of glowing felt crystals.
        static void Crystals(LevelKit kit, Vector3 feet, Color col, int seed)
        {
            var g = kit.Group("Crystals", feet);
            var glow = PatchworkAssets.Glow(col, 1.3f);
            for (int k = 0; k < 5; k++)
            {
                float a = k * 72f + seed * 13f;
                float h = 0.5f + (k % 3) * 0.35f;
                var c = kit.Deco(PrimitiveType.Cube, Quaternion.Euler(0f, a, 0f) * new Vector3(0f, h * 0.5f, 0.25f), new Vector3(0.22f, h, 0.22f), glow, default(Vector3), g);
                c.localRotation = Quaternion.Euler(0f, a, 0f) * Quaternion.Euler(18f + k * 4f, 0f, 45f);
            }
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.05f, 0f), new Vector3(1.1f, 0.2f, 1.1f), PatchworkAssets.Felt(new Color(0.4f, 0.4f, 0.45f)), default(Vector3), g);
            Trunk(g, 0.5f, 1f);
        }

        // ---------------------------------------------------------------- Undergrowth

        // Now the trees are tall there's room underneath: ferns, fallen hollow logs, old stumps,
        // mossy rocks, and lanterns hung along the paths.
        static void ForestUndergrowth(LevelKit kit)
        {
            var rng = new System.Random(515);
            int mask = ~(1 << 2);
            System.Func<float, float, Vector3> random = (zMin, zMax) => new Vector3(-113.5f + (float)rng.NextDouble() * 61f, 0f, zMin + (float)rng.NextDouble() * (zMax - zMin));
            System.Func<Vector3, float, bool> free = (p, r) =>
                !Taken(p, r) && !NearLane(p, 0.3f) && !Physics.CheckSphere(p + Vector3.up * (r * 0.6f + 0.3f), r * 0.6f, mask, QueryTriggerInteraction.Ignore);
            var fern = new[] { PatchworkAssets.Felt(new Color(0.28f, 0.5f, 0.32f)), PatchworkAssets.Felt(new Color(0.34f, 0.56f, 0.34f)), PatchworkAssets.Felt(new Color(0.24f, 0.42f, 0.34f)) };
            for (int i = 0; i < 140; i++)
            {
                Vector3 p = random(-113.5f, -10f);
                if (!free(p, 0.7f)) continue;
                Take(p, 0.7f);
                Fern(kit, p, 0.8f + (float)rng.NextDouble() * 0.6f, fern[i % fern.Length], i);
            }
            for (int i = 0; i < 16; i++)
            {
                Vector3 p = random(-112f, -14f);
                if (!free(p, 2.4f)) continue;
                Take(p, 2.4f);
                FallenLog(kit, p, (float)rng.NextDouble() * 180f, 3.5f + (float)rng.NextDouble() * 1.5f, i);
            }
            for (int i = 0; i < 22; i++)
            {
                Vector3 p = random(-112f, -14f);
                if (!free(p, 1f)) continue;
                Take(p, 1f);
                Stump(kit, p, 0.6f + (float)rng.NextDouble() * 0.4f, i);
            }
            var rock = PatchworkAssets.Cobble(new Color(0.55f, 0.56f, 0.58f));
            var mossTop = PatchworkAssets.Felt(new Color(0.38f, 0.55f, 0.36f));
            for (int i = 0; i < 34; i++)
            {
                Vector3 p = random(-112f, -12f);
                float s = 0.5f + (float)rng.NextDouble() * 1.1f;
                if (!free(p, s)) continue;
                Take(p, s);
                var g = kit.Group("MossyRock", p);
                g.rotation = Quaternion.Euler(0f, i * 61f, 0f);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, s * 0.3f, 0f), new Vector3(s * 1.6f, s * 0.9f, s * 1.3f), rock, default(Vector3), g);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0.1f, s * 0.62f, 0f), new Vector3(s * 1.2f, s * 0.3f, s * 1f), mossTop, default(Vector3), g);
                if (s > 1f) Trunk(g, s * 0.6f, s * 0.8f);
            }
            // Lanterns hung along the paths every so often, alternating sides.
            int n = 0;
            foreach (var line in ForestPaths)
                for (int s = 0; s < line.Length - 1; s++)
                {
                    Vector3 a = line[s], b = line[s + 1];
                    Vector3 side = Vector3.Cross(Vector3.up, (b - a).normalized);
                    float len = Vector3.Distance(a, b);
                    for (float u = 6f; u < len; u += 13f)
                    {
                        Vector3 p = Vector3.Lerp(a, b, u / len) + side * ((n++ % 2 == 0) ? 1.9f : -1.9f);
                        if (Taken(p, 0.5f) || Physics.CheckSphere(p + Vector3.up, 0.4f, mask, QueryTriggerInteraction.Ignore)) continue;
                        Take(p, 0.6f);
                        LanternPost(kit, p);
                    }
                }
        }

        static void Fern(LevelKit kit, Vector3 feet, float size, Material mat, int seed)
        {
            var g = kit.Group("Fern", feet);
            g.localScale = Vector3.one * size;
            g.rotation = Quaternion.Euler(0f, seed * 47f, 0f);
            var rib = PatchworkAssets.Felt(new Color(0.5f, 0.68f, 0.42f));
            for (int k = 0; k < 5; k++)
            {
                float a = k * 72f + (seed % 3) * 10f;
                var frond = kit.Group("Frond", Vector3.zero);
                frond.SetParent(g, false);
                frond.localRotation = Quaternion.Euler(0f, a, 0f) * Quaternion.Euler(55f - (k % 2) * 15f, 0f, 0f);
                // A wide leafy frond with a paler stitched midrib.
                kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.45f, 0f), new Vector3(0.3f, 0.45f, 0.04f), mat, default(Vector3), frond);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.42f, 0.022f), new Vector3(0.03f, 0.7f, 0.01f), rib, default(Vector3), frond);
            }
        }

        static void FallenLog(LevelKit kit, Vector3 c, float yaw, float len, int seed)
        {
            var g = kit.Group("FallenLog", c);
            g.rotation = Quaternion.Euler(0f, yaw, 0f);
            var bark = PatchworkAssets.Knit(new Color(0.42f, 0.32f, 0.25f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.55f, 0f), new Vector3(1.1f, len * 0.5f, 1.1f), bark, new Vector3(0f, 0f, 90f), g);
            // The hollow end and the rings on the other.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(len * 0.5f + 0.01f, 0.55f, 0f), new Vector3(0.8f, 0.02f, 0.8f), PatchworkAssets.Plain(new Color(0.08f, 0.06f, 0.07f), 0.1f), new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-len * 0.5f - 0.01f, 0.55f, 0f), new Vector3(1f, 0.02f, 1f), PatchworkAssets.Wood(new Color(0.85f, 0.7f, 0.5f)), new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-len * 0.5f - 0.02f, 0.55f, 0f), new Vector3(0.55f, 0.02f, 0.55f), PatchworkAssets.Wood(new Color(0.7f, 0.55f, 0.38f)), new Vector3(0f, 0f, 90f), g);
            // Moss along the top and shelf mushrooms on the side.
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 1.05f, 0.1f), new Vector3(0.6f, len * 0.35f, 0.3f), PatchworkAssets.Felt(new Color(0.38f, 0.55f, 0.36f)), new Vector3(0f, 0f, 90f), g);
            for (int k = 0; k < 3; k++)
                kit.Deco(PrimitiveType.Sphere, new Vector3(-len * 0.3f + k * 0.45f, 0.6f + k * 0.08f, 0.56f), new Vector3(0.4f, 0.08f, 0.3f), PatchworkAssets.Felt(seed % 2 == 0 ? new Color(0.95f, 0.75f, 0.45f) : new Color(0.85f, 0.85f, 0.8f)), default(Vector3), g);
            var col = g.gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.55f, 0f);
            col.size = new Vector3(len, 1.1f, 1.1f);
        }

        static void Stump(LevelKit kit, Vector3 feet, float r, int seed)
        {
            var g = kit.Group("Stump", feet);
            g.rotation = Quaternion.Euler(0f, seed * 53f, 0f);
            var bark = PatchworkAssets.Knit(new Color(0.4f, 0.31f, 0.25f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.35f, 0f), new Vector3(r * 2f, 0.35f, r * 2f), bark, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.71f, 0f), new Vector3(r * 1.8f, 0.02f, r * 1.8f), PatchworkAssets.Wood(new Color(0.85f, 0.7f, 0.5f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.72f, 0f), new Vector3(r * 1f, 0.02f, r * 1f), PatchworkAssets.Wood(new Color(0.72f, 0.56f, 0.4f)), default(Vector3), g);
            for (int k = 0; k < 3; k++)
            {
                float a = k * 120f + 30f;
                Rod(kit, new Vector3(0f, 0.3f, 0f), Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0.02f, r + 0.5f), bark, 0.22f, g);
            }
            if (seed % 3 == 0) Mushroom(kit, feet + new Vector3(r * 0.7f, 0f, r * 0.5f), 0.3f, new Color(0.9f, 0.3f, 0.32f), false);
            Trunk(g, r, 0.75f);
        }

        // ---------------------------------------------------------------- The Moonglow Glade

        static void BuildMoonGlade(LevelKit kit)
        {
            Vector3 c = MoonGladeCenter;
            var g = kit.Group("MoonGlade", Vector3.zero);   // children use world positions
            var gladeGo = new GameObject("MoonGladeMagic");
            gladeGo.transform.SetParent(kit.Root, false);
            var glade = gladeGo.AddComponent<MoonGlade>();
            glade.Center = c;
            ClearGrass(c, 11f);

            // A soft mossy floor and a ring of pale pebbles.
            kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.02f, new Vector3(23f, 0.01f, 23f), PatchworkAssets.Felt(new Color(0.33f, 0.46f, 0.44f)), default(Vector3), g);
            for (int k = 0; k < 40; k++)
            {
                float a = k * Mathf.PI * 2f / 40f;
                kit.Deco(PrimitiveType.Sphere, c + new Vector3(Mathf.Cos(a) * 11.2f, 0.06f, Mathf.Sin(a) * 11.2f), new Vector3(0.45f, 0.18f, 0.38f), PatchworkAssets.Felt(new Color(0.82f, 0.84f, 0.86f)), new Vector3(0f, k * 23f, 0f), g);
            }
            // The moon pool: dark until the Gloom is chased off.
            for (int k = 0; k < 22; k++)
            {
                float a = k * Mathf.PI * 2f / 22f;
                kit.Deco(PrimitiveType.Sphere, c + new Vector3(Mathf.Cos(a) * 3.4f, 0.12f, Mathf.Sin(a) * 3.4f), new Vector3(0.9f, 0.4f, 0.7f), PatchworkAssets.Cobble(new Color(0.62f, 0.64f, 0.68f)), new Vector3(0f, -a * Mathf.Rad2Deg, 0f), g);
            }
            glade.PoolDark = PatchworkAssets.Plain(new Color(0.06f, 0.08f, 0.16f), 0.95f);
            glade.PoolLit = PatchworkAssets.Glow(new Color(0.78f, 0.9f, 1f), 1.4f);
            glade.Pool = kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.07f, new Vector3(6.4f, 0.02f, 6.4f), glade.PoolDark, default(Vector3), g).GetComponent<Renderer>();
            // A crescent moon reflected in it.
            kit.Deco(PrimitiveType.Cylinder, c + new Vector3(0.6f, 0.085f, 0.8f), new Vector3(1.1f, 0.01f, 1.1f), PatchworkAssets.Glow(new Color(0.9f, 0.95f, 1f), 0.6f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, c + new Vector3(0.85f, 0.09f, 1f), new Vector3(1f, 0.012f, 1f), glade.PoolDark, default(Vector3), g);

            // Eight standing stones with runes that wake up.
            glade.RuneDim = PatchworkAssets.Glow(new Color(0.35f, 0.45f, 0.6f), 0.25f);
            glade.RuneLit = PatchworkAssets.Glow(new Color(0.6f, 0.9f, 1f), 2.6f);
            var stoneMat = PatchworkAssets.Cobble(new Color(0.55f, 0.56f, 0.6f));
            for (int k = 0; k < 8; k++)
            {
                float a = k * 45f + 22.5f;
                var q = Quaternion.Euler(0f, a, 0f);
                Vector3 at = c + q * new Vector3(0f, 0f, glade.StoneRadius);
                var st = kit.Group("StandingStone", at);
                st.rotation = q * Quaternion.Euler(k % 2 == 0 ? 4f : -3f, 180f, (k % 3 - 1) * 3f);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.6f, 0f), new Vector3(1.2f, 3.2f + (k % 3) * 0.4f, 0.6f), stoneMat, default(Vector3), st);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 3.3f + (k % 3) * 0.2f, 0f), new Vector3(1.2f, 0.5f, 0.6f), stoneMat, default(Vector3), st);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0.3f, 2.6f, -0.2f), new Vector3(0.5f, 0.35f, 0.3f), PatchworkAssets.Felt(new Color(0.36f, 0.52f, 0.36f)), default(Vector3), st);
                // A little rune: three glowing strokes on the face towards the pool.
                for (int r = 0; r < 3; r++)
                {
                    var rune = kit.Deco(PrimitiveType.Cube, new Vector3((r - 1) * 0.22f, 1.8f + (r == 1 ? 0.25f : 0f), 0.31f), new Vector3(0.07f, 0.5f - (r == 1 ? 0.15f : 0f), 0.02f), glade.RuneDim, new Vector3(0f, 0f, (r - 1) * (k % 2 == 0 ? 25f : -30f)), st);
                    glade.Runes.Add(rune.GetComponent<Renderer>());
                }
                TownKit.Solid(kit, at + Vector3.up * 1.7f, new Vector3(1.2f, 3.4f, 0.7f), a);
            }

            // Floating candles over the glade.
            var candles = kit.Group("FloatingCandles", Vector3.zero);
            var wax = PatchworkAssets.Felt(new Color(0.98f, 0.95f, 0.86f));
            var flame = PatchworkAssets.Glow(new Color(1f, 0.82f, 0.45f), 2.6f);
            var rng = new System.Random(808);
            for (int k = 0; k < 18; k++)
            {
                float a = (float)rng.NextDouble() * Mathf.PI * 2f, r = 2f + (float)rng.NextDouble() * 8f;
                var cg = kit.Group("Candle", Vector3.zero);
                cg.SetParent(candles, false);
                cg.localPosition = c + new Vector3(Mathf.Cos(a) * r, 4.5f + (float)rng.NextDouble() * 3f, Mathf.Sin(a) * r);
                float len = 0.25f + (float)rng.NextDouble() * 0.25f;
                kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.14f, len, 0.14f), wax, default(Vector3), cg);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, len + 0.1f, 0f), new Vector3(0.09f, 0.16f, 0.09f), flame, default(Vector3), cg);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0.04f, len - 0.05f, 0.04f), new Vector3(0.05f, 0.1f, 0.05f), wax, default(Vector3), cg);
                foreach (var rr in cg.GetComponentsInChildren<Renderer>()) rr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                var f = cg.gameObject.AddComponent<Floater>();
                f.Height = 0.25f + (float)rng.NextDouble() * 0.2f;
                f.Speed = 0.6f + (float)rng.NextDouble() * 0.5f;
            }

            // The moonbeam (only once the pool is lit).
            var beam = kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 20f, new Vector3(6f, 20f, 6f), PatchworkAssets.Bubble(new Color(0.8f, 0.9f, 1f, 0.07f)), default(Vector3), g);
            beam.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            beam.gameObject.SetActive(false);
            glade.Moonbeam = beam.gameObject;

            // The Gloom: grey tangles with glowing eyes, circling the pool.
            for (int k = 0; k < 7; k++)
            {
                var orbit = kit.Group("GloomOrbit", Vector3.zero);
                var o = orbit.gameObject.AddComponent<Orbiter>();
                o.Center = c + Vector3.up * (1.4f + (k % 3) * 0.6f);
                o.Radius = 4.6f + (k % 2) * 1.2f;
                o.RadiusZ = o.Radius;
                o.Speed = (k % 2 == 0 ? 1f : -1f) * (22f + k * 3f);
                o.Angle = k * 51f;
                o.Bob = 0.4f;
                o.Lean = 0f;
                var wisp = kit.Group("GloomWisp", Vector3.zero);
                wisp.SetParent(orbit, false);
                wisp.localPosition = Vector3.zero;
                KnotShape(kit, wisp, 0.9f, 8);
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.15f, 0.12f, 0.4f), new Vector3(0.14f, 0.1f, 0.05f), PatchworkAssets.Glow(new Color(1f, 0.35f, 0.45f), 2.2f), default(Vector3), wisp);
                glade.Gloom.Add(orbit);
            }

            // The guardian: a fox made of moonlight yarn (hidden until it's called).
            glade.Guardian = GuardianFox(kit, c);

            // Where to raise the needle, the hint, and the rewards.
            var spot = kit.Volume<UseSpot>(c + Vector3.up * 1.2f, new Vector3(10f, 3f, 10f), "MoonPoolSpot");
            spot.Label = "Raise the Moonlit Needle";
            spot.PromptPoint = c + Vector3.up * 2.6f;
            glade.Spot = spot;
            Signpost(kit, c + new Vector3(-6.5f, 0f, -3.5f), new[] { 255f, 160f, 45f }, new[] { SignWood, SignWood, SignTown });
            kit.Hint(c + new Vector3(4.5f, 1.5f, 5f), new Vector3(5f, 3f, 5f), "Moonglow Glade",
                "The Gloom has smothered the moon pool. Only a guardian of moonlight can chase it away... and that needs the Moonlit Needle.");
            glade.Marble = SecretMarble.Place(kit, c + Vector3.up * 1.6f, MarbleIds[15], MarbleColor(15), true);
            glade.Prize = PrizeBubble.Place(kit, c + new Vector3(2f, 1.8f, -1.2f), "prize_wizardhat", true);
        }

        static Transform GuardianFox(LevelKit kit, Vector3 at)
        {
            var g = kit.Group("MoonlightGuardian", at);
            var light = PatchworkAssets.Glow(new Color(0.72f, 0.88f, 1f), 2.2f);
            var core = PatchworkAssets.Glow(new Color(0.92f, 0.97f, 1f), 3f);
            var aura = PatchworkAssets.Bubble(new Color(0.75f, 0.9f, 1f, 0.18f));
            g.localScale = Vector3.one;
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.75f, 0f), new Vector3(0.5f, 0.6f, 0.5f), light, new Vector3(90f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.1f, 0.65f), new Vector3(0.5f, 0.45f, 0.5f), light, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.02f, 0.92f), new Vector3(0.2f, 0.18f, 0.3f), core, default(Vector3), g);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.15f, 1.38f, 0.6f), new Vector3(0.14f, 0.22f, 0.05f), light, new Vector3(0f, 0f, s * -20f), g);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.11f, 1.16f, 0.88f), new Vector3(0.06f, 0.08f, 0.03f), PatchworkAssets.Plain(new Color(0.2f, 0.3f, 0.5f), 0.9f), default(Vector3), g);
                for (int z = -1; z <= 1; z += 2)
                    kit.Deco(PrimitiveType.Capsule, new Vector3(s * 0.16f, 0.3f, z * 0.4f), new Vector3(0.12f, 0.3f, 0.12f), light, new Vector3(z * 15f, 0f, 0f), g);
            }
            for (int k = 0; k < 5; k++)
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.85f + k * 0.12f, -0.65f - k * 0.2f), Vector3.one * (0.42f - k * 0.04f), k == 4 ? core : light, default(Vector3), g);
            var a = kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.85f, 0f), new Vector3(1.6f, 1.6f, 2.4f), aura, default(Vector3), g);
            a.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            foreach (var r in g.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var bob = g.gameObject.AddComponent<Spinner>();
            bob.Speed = 0f;
            bob.BobHeight = 0.1f;
            bob.BobSpeed = 1.6f;
            g.gameObject.SetActive(false);
            return g;
        }

        // ---------------------------------------------------------------- The Wishing Willow

        static void WishingWillow(LevelKit kit)
        {
            Vector3 w = WillowSpot;
            var g = kit.Group("WishingWillow", w);
            g.localScale = Vector3.one * 1.55f;   // the biggest tree in the wood, so you can spot it
            var bark = PatchworkAssets.Knit(new Color(0.4f, 0.32f, 0.26f));
            Rod(kit, new Vector3(0f, 0f, 0f), new Vector3(0.4f, 3.5f, -0.3f), bark, 1.3f, g);
            Rod(kit, new Vector3(0.4f, 3.5f, -0.3f), new Vector3(0f, 6.2f, 0f), bark, 1f, g);
            for (int k = 0; k < 5; k++)
            {
                float a = k * 72f + 15f;
                Rod(kit, new Vector3(0f, 0.5f, 0f), Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0.05f, 1.8f), bark, 0.45f, g);
                Rod(kit, new Vector3(0.2f, 5.6f, 0f), Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 7f, 2.6f), bark, 0.3f, g);
            }
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 7f, 0f), new Vector3(7.5f, 2.6f, 7.5f), PatchworkAssets.Felt(new Color(0.3f, 0.5f, 0.38f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.8f, 7.8f, -0.5f), new Vector3(4.5f, 2f, 4.5f), PatchworkAssets.Felt(new Color(0.36f, 0.56f, 0.42f)), default(Vector3), g);
            // The weeping curtain of glowing strands.
            var strand = PatchworkAssets.Glow(new Color(0.55f, 0.95f, 0.7f), 0.55f);
            var leaf = PatchworkAssets.Felt(new Color(0.42f, 0.66f, 0.45f));
            for (int k = 0; k < 34; k++)
            {
                float a = k * Mathf.PI * 2f / 34f;
                float r = 3.1f + (k % 3) * 0.25f;
                Vector3 top = new Vector3(Mathf.Cos(a) * r, 6.3f, Mathf.Sin(a) * r);
                float len = 3.6f + (k % 4) * 0.6f;
                // Leave a doorway facing the glade.
                if (Mathf.Abs(Mathf.DeltaAngle(a * Mathf.Rad2Deg, 15f)) < 22f) len = 1.6f;
                Rod(kit, top, top + Vector3.down * len, k % 2 == 0 ? strand : leaf, 0.07f, g);
                kit.Deco(PrimitiveType.Sphere, top + Vector3.down * len, new Vector3(0.14f, 0.2f, 0.14f), strand, default(Vector3), g);
            }
            Trunk(g, 0.75f, 6f);
            ClearGrass(w, 6f);
            // Glowing toadstools round its roots and fireflies drifting in its curtain.
            for (int k = 0; k < 12; k++)
            {
                float a = k * Mathf.PI * 2f / 12f + 0.2f;
                Mushroom(kit, w + new Vector3(Mathf.Cos(a) * 3.2f, 0f, Mathf.Sin(a) * 3.2f), 0.4f + (k % 3) * 0.1f, k % 2 == 0 ? new Color(0.55f, 1f, 0.75f) : new Color(0.75f, 0.85f, 1f), true);
            }
            var flies = kit.Group("WillowFireflies", Vector3.zero);
            var flyMat = PatchworkAssets.Glow(new Color(0.75f, 1f, 0.8f), 3f);
            for (int k = 0; k < 16; k++)
            {
                float a = k * 2.4f;
                var m = kit.Deco(PrimitiveType.Sphere, w + new Vector3(Mathf.Cos(a) * (2f + k % 4), 1.5f + (k % 5) * 1.6f, Mathf.Sin(a) * (2f + k % 4)), Vector3.one * 0.12f, flyMat, default(Vector3), flies);
                m.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            flies.gameObject.AddComponent<FireflySwarm>();
            // The needle, floating over a mossy stump in front of the trunk, towards the glade.
            Vector3 toGlade = (MoonGladeCenter - w);
            toGlade.y = 0f;
            toGlade.Normalize();
            Vector3 stump = w + toGlade * 3.4f;
            kit.Deco(PrimitiveType.Cylinder, stump + Vector3.up * 0.25f, new Vector3(0.9f, 0.25f, 0.9f), PatchworkAssets.Wood(new Color(0.45f, 0.34f, 0.26f)), default(Vector3));
            kit.Deco(PrimitiveType.Sphere, stump + Vector3.up * 0.52f, new Vector3(0.85f, 0.12f, 0.85f), PatchworkAssets.Felt(new Color(0.4f, 0.6f, 0.4f)), default(Vector3));
            var needle = kit.Volume<MoonNeedle>(stump + Vector3.up * 1.3f, new Vector3(1.6f, 2f, 1.6f), "MoonNeedle");
            var vis = kit.Group("NeedleVisual", stump + Vector3.up * 1.3f);
            vis.SetParent(needle.transform, true);
            var silver = PatchworkAssets.Glow(new Color(0.85f, 0.92f, 1f), 1.8f);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.05f, 0.45f, 0.05f), silver, default(Vector3), vis);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.47f, 0f), new Vector3(0.05f, 0.1f, 0.05f), silver, default(Vector3), vis);
            for (int s = 0; s < 2; s++)
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.52f, 0f), new Vector3(0.18f, 0.18f, 0.03f), PatchworkAssets.Glow(new Color(1f, 0.92f, 0.55f), 2.4f), new Vector3(0f, s * 90f, 45f), vis);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.1f, 0f), Vector3.one * 0.22f, PatchworkAssets.Yarn(new Color(0.55f, 0.65f, 0.95f)), default(Vector3), vis);
            needle.Visual = vis;
            kit.Hint(w + toGlade * 5f + Vector3.up * 1.5f, new Vector3(4f, 3f, 4f), "The Wishing Willow",
                "Its strands glow softly, like it's keeping something safe...");
        }
    }
}

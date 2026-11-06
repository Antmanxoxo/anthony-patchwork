using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The outskirts of Toybox Town: the quiet corners of the toybox that used to be open grass.
    //   Whispering Wood (south-west)  - a dark felt forest with button-eyed trees, glowing
    //                                    mushrooms, fireflies, cobwebs and lost toys. Its own spooky
    //                                    music and a darker look. Light the three spirit lanterns
    //                                    and the owl's hollow tree opens (a secret marble + a prize)
    //   Button Orchard (south)         - rows of knitted trees with buttons for fruit, ladders and
    //                                    baskets, on the trail from the Pod to the wood
    //   Patchwork Farm (north-east)    - quilted fields, a red toy barn, a sheep pen, a scarecrow,
    //                                    hay bales and a pinwheel windmill
    //   Toadstool Dell (north-west)    - a lily pond with frogs, cattails, a little jetty, a fairy
    //                                    ring and giant bouncy toadstools
    //   Wildflower Meadow (east)       - long grass and flowers where wooden deer roll about
    // And toy wildlife everywhere out here (see ToyCritter).
    public static partial class Level1Builder
    {
        static readonly Vector3 WoodGate = new Vector3(-71.5f, 0f, -79f);
        static readonly Vector3 HollowTree = new Vector3(-99f, 0f, -99f);
        static readonly List<Vector3> taken = new List<Vector3>();   // x, z, radius of things placed here

        static void Outskirts(LevelKit kit)
        {
            taken.Clear();
            lanes.Clear();
            foreach (var t in TrailLines()) Lane(t);
            foreach (var t in ForestPaths) Lane(t);
            Physics.SyncTransforms();
            RavineCrossings(kit);
            Physics.SyncTransforms();
            ForestExpansion(kit);
            WhisperingWood(kit);
            Physics.SyncTransforms();
            ForestUndergrowth(kit);
            Biomes(kit);
            ButtonOrchard(kit);
            PatchworkFarm(kit);
            ToadstoolDell(kit);
            WildflowerMeadow(kit);
            Vignettes(kit);
            Physics.SyncTransforms();
            Corners(kit);
            Trails(kit);
            Physics.SyncTransforms();
            Wildlife(kit);

            var P = kit.Info.MapPins;
            P.Add(new MapPin("Whispering Wood", new Vector3(-80f, 0f, -30f), MapPin.Kind.Area));
            P.Add(new MapPin("Moonglow Glade", MoonGladeCenter));
            P.Add(new MapPin("Wishing Willow", WillowSpot));
            P.Add(new MapPin("Deep Wood", new Vector3(-95f, 0f, -95f), MapPin.Kind.Area));
            P.Add(new MapPin("Button Orchard", new Vector3(-6f, 0f, -100f), MapPin.Kind.Area));
            P.Add(new MapPin("Patchwork Farm", new Vector3(100f, 0f, 98f), MapPin.Kind.Area));
            P.Add(new MapPin("Toadstool Dell", new Vector3(-62f, 0f, 102f), MapPin.Kind.Area));
            P.Add(new MapPin("Wildflower Meadow", new Vector3(102f, 0f, 32f), MapPin.Kind.Area));
        }

        // ---------------------------------------------------------------- Placement helpers

        static bool Taken(Vector3 p, float r)
        {
            foreach (var t in taken)
            {
                float dx = p.x - t.x, dz = p.z - t.y;
                if (dx * dx + dz * dz < (r + t.z) * (r + t.z)) return true;
            }
            return false;
        }

        static void Take(Vector3 p, float r) { taken.Add(new Vector3(p.x, p.z, r)); }

        // A clear, flat spot near `near` (nothing solid there, nothing of ours either).
        static bool Spot(Vector3 near, float r, out Vector3 p)
        {
            p = near;
            if (!FindSpot(near, Mathf.Min(r, 1.2f), out p)) return false;
            if (Taken(p, r) || NearLane(p, r * 0.6f)) return false;
            Take(p, r);
            return true;
        }

        static void Rod(LevelKit kit, Vector3 a, Vector3 b, Material mat, float thick, Transform parent = null)
        {
            Vector3 d = b - a;
            var t = kit.Deco(PrimitiveType.Cylinder, (a + b) * 0.5f, new Vector3(thick, d.magnitude * 0.5f, thick), mat, default(Vector3), parent);
            t.localRotation = Quaternion.FromToRotation(Vector3.up, d.normalized);
        }

        static void Trunk(Transform g, float radius, float height, float centreY = -1f)
        {
            var col = g.gameObject.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, centreY >= 0f ? centreY : height * 0.5f, 0f);
            col.radius = radius;
            col.height = height;
        }

        // ================================================================ Whispering Wood

        static void WhisperingWood(LevelKit kit)
        {
            var rng = new System.Random(66);
            var wood = kit.Group("WhisperingWood", Vector3.zero);

            // The darker, mossy forest floor.
            kit.Deco(PrimitiveType.Cube, new Vector3(-93f, 0.012f, -93f), new Vector3(43f, 0.02f, 43f), PatchworkAssets.Felt(new Color(0.24f, 0.33f, 0.28f)), default(Vector3), wood);
            for (int i = 0; i < 9; i++)
            {
                Vector3 m = new Vector3(-110f + (float)rng.NextDouble() * 34f, 0.026f, -110f + (float)rng.NextDouble() * 34f);
                kit.Deco(PrimitiveType.Cylinder, m, new Vector3(5f + i % 3 * 2f, 0.01f, 4f + i % 2 * 2f), PatchworkAssets.Felt(new Color(0.2f, 0.29f, 0.3f)), new Vector3(0f, i * 40f, 0f), wood);
            }

            // The path in: dark flagstones winding from the gate to the hollow tree.
            Vector3[] path = { WoodGate, new Vector3(-79f, 0f, -82f), new Vector3(-85f, 0f, -87f), new Vector3(-92f, 0f, -91f), new Vector3(-95f, 0f, -96f) };
            var flag = new[] { PatchworkAssets.Cobble(new Color(0.42f, 0.42f, 0.48f)), PatchworkAssets.Cobble(new Color(0.36f, 0.38f, 0.42f)) };
            int n = 0;
            for (int s = 0; s < path.Length - 1; s++)
            {
                Vector3 a = path[s], b = path[s + 1];
                int steps = Mathf.CeilToInt(Vector3.Distance(a, b) / 2.1f);
                for (int k = 0; k < steps; k++)
                {
                    Vector3 p = Vector3.Lerp(a, b, k / (float)steps) + new Vector3(Mathf.Sin(n * 1.3f) * 0.4f, 0.03f, Mathf.Cos(n * 0.9f) * 0.4f);
                    kit.Deco(PrimitiveType.Cylinder, p, new Vector3(1.3f, 0.03f, 1.0f), flag[n % 2], new Vector3(0f, n * 47f, 0f), wood);
                    Take(p, 1.6f);
                    n++;
                }
            }
            Take(HollowTree, 8f);
            ClearGrass(HollowTree, 5f);

            // The gate: two crooked posts, a sagging crossbar, a hanging lantern and a sign with
            // painted eyes. Pumpkins with glowing faces either side.
            Gate(kit, WoodGate, 68f);
            Take(WoodGate, 3.5f);
            Pumpkin(kit, WoodGate + new Vector3(1.6f, 0f, -3.4f), 0.9f, 20f);
            Pumpkin(kit, WoodGate + new Vector3(-1.5f, 0f, 3.3f), 0.7f, -30f);
            Pumpkin(kit, new Vector3(-86.5f, 0f, -84.5f), 0.6f, 60f);
            Pumpkin(kit, new Vector3(-90f, 0f, -94f), 0.8f, 150f);
            kit.Hint(WoodGate + new Vector3(-2f, 1.5f, -2f), new Vector3(6f, 4f, 6f), "The Whispering Wood",
                "It's awfully dark in here... Somewhere among the trees, three spirit lanterns are waiting to be lit.");

            // The hollow tree and its owl, and the three spirit lanterns.
            var secretGo = new GameObject("WoodSecret");
            secretGo.transform.SetParent(kit.Root, false);
            var secret = secretGo.AddComponent<WoodSecret>();
            HollowOak(kit, secret);
            Vector3[] lanterns = { new Vector3(-109f, 0f, -79f), new Vector3(-80f, 0f, -109f), new Vector3(-111f, 0f, -108f) };
            float[] pitch = { 1f, 1.26f, 1.5f };
            Color[] glowCol = { new Color(0.55f, 1f, 0.85f), new Color(0.75f, 0.7f, 1f), new Color(1f, 0.75f, 0.9f) };
            for (int i = 0; i < lanterns.Length; i++)
            {
                SpiritLanternAt(kit, secret, lanterns[i], pitch[i], glowCol[i]);
                Take(lanterns[i], 2.5f);
                // A trail of tiny glowing mushrooms from the path towards it.
                Vector3 from = path[1 + i];
                int crumbs = Mathf.FloorToInt(Vector3.Distance(from, lanterns[i]) / 2.6f);
                for (int k = 1; k < crumbs; k++)
                {
                    Vector3 p = Vector3.Lerp(from, lanterns[i], k / (float)crumbs) + new Vector3(Mathf.Sin(k * 2.1f + i) * 0.9f, 0f, Mathf.Cos(k * 1.7f) * 0.9f);
                    Mushroom(kit, p, 0.35f + (k % 3) * 0.08f, glowCol[i], true);
                    Take(p, 0.5f);
                }
            }

            // Trees: tall dark felt pines, gnarled bare trees with grey yarn moss, and a few
            // giant oaks whose dark crowns shade the whole wood.
            Vector3[] oaks = { new Vector3(-108f, 0f, -96f), new Vector3(-88f, 0f, -106f), new Vector3(-104f, 0f, -86f), new Vector3(-84f, 0f, -96f), new Vector3(-96f, 0f, -80f), new Vector3(-110f, 0f, -110f) };
            foreach (var o in oaks)
                if (Spot(o, OakSpread, out Vector3 p)) GiantOak(kit, p, 19f + (float)rng.NextDouble() * 4f, rng);
            for (float x = -112f; x <= -72f; x += 5f)
                for (float z = -112f; z <= -72f; z += 5f)
                {
                    Vector3 near = new Vector3(x + (float)(rng.NextDouble() - 0.5) * 3f, 0f, z + (float)(rng.NextDouble() - 0.5) * 3f);
                    // Thin out towards the gate so the edge feels like a treeline, not a wall.
                    float edge = Mathf.Max(near.x + 72f, near.z + 72f);
                    if (edge > -2f && rng.NextDouble() < 0.6) continue;
                    if (rng.NextDouble() < 0.7)
                    {
                        float h = 13f + (float)rng.NextDouble() * 6f;
                        if (Spot(near, PineSpread(h), out Vector3 p)) SpookyPine(kit, p, h, rng);
                    }
                    else if (Spot(near, GnarlSpread, out Vector3 q)) GnarledTree(kit, q, 8.5f + (float)rng.NextDouble() * 3.5f, rng, rng.NextDouble() < 0.4);
                }

            // Glowing mushrooms in clumps, and a fairy ring round the hollow tree's clearing.
            Color[] shroomCols = { new Color(0.45f, 0.95f, 0.9f), new Color(0.8f, 0.55f, 1f), new Color(1f, 0.6f, 0.85f), new Color(0.7f, 1f, 0.55f) };
            for (int i = 0; i < 18; i++)
            {
                Vector3 near = new Vector3(-111f + (float)rng.NextDouble() * 37f, 0f, -111f + (float)rng.NextDouble() * 37f);
                if (!Spot(near, 1f, out Vector3 p)) continue;
                int count = 2 + i % 3;
                for (int k = 0; k < count; k++)
                    Mushroom(kit, p + new Vector3(Mathf.Cos(k * 2.3f) * 0.5f, 0f, Mathf.Sin(k * 2.3f) * 0.5f), 0.4f + k * 0.15f, shroomCols[(i + k) % shroomCols.Length], true);
            }
            for (int k = 0; k < 16; k++)
            {
                float a = k * Mathf.PI * 2f / 16f;
                Mushroom(kit, HollowTree + new Vector3(Mathf.Cos(a) * 6.8f, 0f, Mathf.Sin(a) * 6.8f), 0.45f, shroomCols[k % 2], true);
            }

            // Fireflies drifting through the dark.
            var flies = kit.Group("Fireflies", Vector3.zero);
            var flyMat = PatchworkAssets.Glow(new Color(1f, 0.95f, 0.55f), 3f);
            for (int i = 0; i < 34; i++)
            {
                Vector3 p = new Vector3(-111f + (float)rng.NextDouble() * 37f, 0.6f + (float)rng.NextDouble() * 3.5f, -111f + (float)rng.NextDouble() * 37f);
                var m = kit.Deco(PrimitiveType.Sphere, p, Vector3.one * 0.12f, flyMat, default(Vector3), flies);
                m.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            flies.gameObject.AddComponent<FireflySwarm>();

            // Eyes peeking out of the dark between the trees.
            for (int i = 0; i < 9; i++)
            {
                Vector3 p = new Vector3(-111f + (float)rng.NextDouble() * 36f, 0.5f + (float)rng.NextDouble() * 2.2f, -111f + (float)rng.NextDouble() * 36f);
                if (Taken(p, 0.6f)) continue;
                EyesInTheDark(kit, p, (float)rng.NextDouble() * 360f, i % 3 == 0 ? new Color(1f, 0.4f, 0.4f) : new Color(1f, 0.92f, 0.45f));
            }

            // Cobwebs (with their spiders), lost toys and low fog.
            Cobweb(kit, new Vector3(-82.5f, 2.4f, -91f), 1.6f, 70f);
            Cobweb(kit, new Vector3(-101f, 3f, -84f), 2f, 10f);
            Cobweb(kit, new Vector3(-90f, 2.2f, -103.5f), 1.4f, 120f);
            LostTeddy(kit, new Vector3(-88.6f, 0f, -98.5f), 210f);
            SprungJack(kit, new Vector3(-104.5f, 0f, -92f), 60f);
            var fog = PatchworkAssets.Bubble(new Color(0.85f, 0.9f, 1f, 0.12f));
            for (int i = 0; i < 7; i++)
            {
                Vector3 p = new Vector3(-108f + i % 3 * 12f + (float)rng.NextDouble() * 4f, 0.35f, -108f + i / 3 * 13f + (float)rng.NextDouble() * 4f);
                var f = kit.Deco(PrimitiveType.Sphere, p, new Vector3(11f, 0.7f, 9f), fog, new Vector3(0f, i * 30f, 0f), wood);
                f.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            // Sounds, music and the darker look.
            var sounds = kit.Group("WoodSounds", Vector3.zero);
            AmbientEmitter.Create(sounds, new Vector3(-93f, 2f, -93f), Sfx.Ambient(Sfx.AmbientClip.Woods), 0.6f, 10f, 34f);
        }

        static void Gate(LevelKit kit, Vector3 at, float yaw)
        {
            var g = kit.Group("WoodGate", at);
            g.rotation = Quaternion.Euler(0f, yaw, 0f);   // facing out along the path
            var bark = PatchworkAssets.Wood(new Color(0.32f, 0.24f, 0.2f));
            Rod(kit, new Vector3(-2.2f, 0f, 0f), new Vector3(-2.5f, 4.6f, 0.2f), bark, 0.4f, g);
            Rod(kit, new Vector3(2.2f, 0f, 0f), new Vector3(2.4f, 4.3f, -0.2f), bark, 0.4f, g);
            Rod(kit, new Vector3(-2.9f, 4.4f, 0.1f), new Vector3(2.8f, 4.1f, -0.1f), bark, 0.32f, g);
            // Twiggy bits.
            Rod(kit, new Vector3(-2.4f, 3.6f, 0.1f), new Vector3(-3.4f, 4.4f, 0.3f), bark, 0.12f, g);
            Rod(kit, new Vector3(2.3f, 3.2f, 0f), new Vector3(3.3f, 3.9f, -0.3f), bark, 0.12f, g);
            // The sign: a wonky plank with two big painted eyes.
            var sign = kit.Deco(PrimitiveType.Cube, new Vector3(0f, 3.5f, 0.05f), new Vector3(2.4f, 0.8f, 0.12f), PatchworkAssets.Planks(new Color(0.55f, 0.42f, 0.32f)), new Vector3(0f, 0f, -6f), g);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.45f, 3.52f + s * 0.05f, 0.13f), new Vector3(0.42f, 0.02f, 0.42f), PatchworkAssets.Felt(new Color(0.98f, 0.95f, 0.85f)), new Vector3(90f, 0f, 0f), g);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.45f + 0.06f, 3.5f + s * 0.05f, 0.15f), new Vector3(0.2f, 0.02f, 0.2f), PatchworkAssets.Plain(new Color(0.08f, 0.07f, 0.1f), 0.6f), new Vector3(90f, 0f, 0f), g);
            }
            Rod(kit, new Vector3(-0.9f, 4.2f, 0f), new Vector3(-0.8f, 3.9f, 0.05f), PatchworkAssets.Yarn(new Color(0.85f, 0.8f, 0.7f)), 0.03f, g);
            Rod(kit, new Vector3(0.9f, 4.15f, 0f), new Vector3(0.85f, 3.9f, 0.05f), PatchworkAssets.Yarn(new Color(0.85f, 0.8f, 0.7f)), 0.03f, g);
            // The lantern, swinging.
            var hang = kit.Group("Lantern", Vector3.zero);
            hang.SetParent(g, false);
            hang.localPosition = new Vector3(1.6f, 4.1f, 0f);
            Rod(kit, Vector3.zero, new Vector3(0f, -0.6f, 0f), PatchworkAssets.Yarn(new Color(0.3f, 0.3f, 0.32f)), 0.03f, hang);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.65f, 0f), new Vector3(0.36f, 0.04f, 0.36f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), default(Vector3), hang);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.95f, 0f), new Vector3(0.36f, 0.48f, 0.36f), PatchworkAssets.Glow(new Color(1f, 0.6f, 0.25f), 2.2f), default(Vector3), hang);
            var swing = hang.gameObject.AddComponent<Rocker>();
            swing.Axis = Vector3.forward;
            swing.Amount = 9f;
            swing.Speed = 1.3f;
            TownKit.Solid(kit, at + g.rotation * new Vector3(-2.3f, 2f, 0f), new Vector3(0.5f, 4f, 0.5f), yaw);
            TownKit.Solid(kit, at + g.rotation * new Vector3(2.3f, 2f, 0f), new Vector3(0.5f, 4f, 0.5f), yaw);
        }

        // A felt pumpkin with a carved, glowing face.
        static void Pumpkin(LevelKit kit, Vector3 feet, float size, float yaw)
        {
            var g = kit.Group("Pumpkin", feet);
            g.rotation = Quaternion.Euler(0f, yaw, 0f);
            g.localScale = Vector3.one * size;
            var orange = PatchworkAssets.Felt(new Color(0.95f, 0.52f, 0.18f));
            var dark = PatchworkAssets.Felt(new Color(0.85f, 0.42f, 0.14f));
            for (int k = 0; k < 6; k++)
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.55f, 0f), new Vector3(0.75f, 1f, 1.25f), k % 2 == 0 ? orange : dark, new Vector3(0f, k * 30f, 0f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.05f, 1.1f, 0f), new Vector3(0.14f, 0.16f, 0.14f), PatchworkAssets.Felt(new Color(0.4f, 0.55f, 0.25f)), new Vector3(0f, 0f, 12f), g);
            var glow = PatchworkAssets.Glow(new Color(1f, 0.8f, 0.3f), 2.4f);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.2f, 0.72f, 0.6f), new Vector3(0.16f, 0.16f, 0.06f), glow, new Vector3(0f, 0f, 45f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.38f, 0.58f), new Vector3(0.42f, 0.09f, 0.06f), glow, new Vector3(0f, 0f, 0f), g);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.14f, 0.45f, 0.59f), new Vector3(0.08f, 0.08f, 0.05f), glow, default(Vector3), g);
            Trunk(g, 0.55f, 1.1f);
        }

        // A tall dark felt pine: stacked felt tiers with a lighter blanket-stitch edge.
        // How much room each kind of tree needs (its crown's radius), so crowns don't overlap.
        const float GnarlSpread = 2.6f, BirchSpread = 2.3f, OakSpread = 5.6f, BrambleSpread = 1.4f;
        static float PineBase(float h) { return Mathf.Clamp(h * 0.15f, 1.7f, 2.7f); }
        static float PineSpread(float h) { return PineBase(h) + 0.3f; }

        // A tall dark felt pine: a bare lower trunk, then stacked felt tiers with a lighter
        // blanket-stitch edge, narrowing to a point.
        static void SpookyPine(LevelKit kit, Vector3 feet, float h, System.Random rng)
        {
            var g = kit.Group("SpookyPine", feet);
            g.rotation = Quaternion.Euler((float)(rng.NextDouble() - 0.5) * 6f, (float)rng.NextDouble() * 360f, (float)(rng.NextDouble() - 0.5) * 6f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, h * 0.3f, 0f), new Vector3(0.55f, h * 0.3f, 0.55f), PatchworkAssets.Felt(new Color(0.3f, 0.22f, 0.2f)), default(Vector3), g);
            // Bark stitches and a few stubby old branches on the bare trunk.
            var barkStitch = PatchworkAssets.Felt(new Color(0.42f, 0.32f, 0.28f));
            for (int s = 0; s < 4; s++)
                kit.Deco(PrimitiveType.Cube, Quaternion.Euler(0f, s * 97f, 0f) * new Vector3(0f, 1f + s * h * 0.07f, 0.27f), new Vector3(0.05f, 0.5f, 0.03f), barkStitch, new Vector3(0f, s * 97f, 0f), g);
            for (int s = 0; s < 2; s++)
                Rod(kit, new Vector3(0f, h * (0.2f + s * 0.1f), 0f), Quaternion.Euler(0f, s * 150f + 40f, 0f) * new Vector3(0f, h * (0.24f + s * 0.1f), 0.8f), PatchworkAssets.Felt(new Color(0.3f, 0.22f, 0.2f)), 0.1f, g);
            Color[] tiers = { new Color(0.16f, 0.3f, 0.32f), new Color(0.13f, 0.24f, 0.3f), new Color(0.18f, 0.27f, 0.36f) };
            var stitch = PatchworkAssets.Felt(new Color(0.42f, 0.55f, 0.55f));
            float rb = PineBase(h);
            const int count = 6;
            for (int t = 0; t < count; t++)
            {
                float k = t / (float)(count - 1);
                float y = h * Mathf.Lerp(0.4f, 0.9f, k);
                float r = Mathf.Lerp(rb, 0.4f, k);
                var tier = kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, y, 0f), new Vector3(r * 2f, h * 0.045f, r * 2f), PatchworkAssets.Felt(tiers[t % tiers.Length]), new Vector3(0f, t * 23f, (t % 2 == 0 ? 3f : -3f)), g);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, y - h * 0.042f, 0f), new Vector3(r * 2f + 0.12f, 0.03f, r * 2f + 0.12f), stitch, new Vector3(0f, t * 23f, (t % 2 == 0 ? 3f : -3f)), g);
                tier.name = "Tier";
            }
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, h * 0.97f, 0f), new Vector3(0.45f, 0.9f, 0.45f), PatchworkAssets.Felt(tiers[1]), default(Vector3), g);
            Trunk(g, 0.35f, h);
        }

        // A bare, twisty tree with hanging grey yarn moss. Some have a knot-hole with eyes.
        static void GnarledTree(LevelKit kit, Vector3 feet, float h, System.Random rng, bool eyes)
        {
            var g = kit.Group("GnarledTree", feet);
            g.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
            var bark = PatchworkAssets.Knit(new Color(0.33f, 0.27f, 0.26f));
            var moss = PatchworkAssets.Yarn(new Color(0.62f, 0.66f, 0.62f));
            Vector3 a = Vector3.zero, b = new Vector3(0.4f, h * 0.45f, 0.1f), c = new Vector3(-0.2f, h * 0.8f, -0.2f);
            Rod(kit, a, b, bark, 0.7f, g);
            Rod(kit, b, c, bark, 0.5f, g);
            kit.Deco(PrimitiveType.Sphere, b, Vector3.one * 0.62f, bark, default(Vector3), g);
            // Roots.
            for (int k = 0; k < 4; k++)
            {
                float ang = k * 90f + 30f;
                Rod(kit, new Vector3(0f, 0.4f, 0f), Quaternion.Euler(0f, ang, 0f) * new Vector3(0f, 0f, 1.1f), bark, 0.3f, g);
            }
            // Branches: up and out, then a crooked tip, with moss dangling.
            for (int k = 0; k < 4; k++)
            {
                float ang = k * 90f + (float)rng.NextDouble() * 40f;
                Vector3 from = Vector3.Lerp(b, c, 0.3f + k * 0.2f);
                Vector3 mid = from + Quaternion.Euler(0f, ang, 0f) * new Vector3(0f, 0.6f, 1.4f);
                Vector3 tip = mid + Quaternion.Euler(0f, ang + 40f, 0f) * new Vector3(0f, 0.7f, 0.9f);
                Rod(kit, from, mid, bark, 0.22f, g);
                Rod(kit, mid, tip, bark, 0.12f, g);
                for (int m = 0; m < 2; m++)
                {
                    Vector3 hang = Vector3.Lerp(mid, tip, 0.3f + m * 0.4f);
                    Rod(kit, hang, hang + Vector3.down * (0.7f + m * 0.4f), moss, 0.06f, g);
                }
            }
            if (eyes)
            {
                kit.Deco(PrimitiveType.Sphere, new Vector3(0.2f, h * 0.32f, 0.33f), new Vector3(0.6f, 0.75f, 0.2f), PatchworkAssets.Plain(new Color(0.06f, 0.05f, 0.08f), 0.2f), default(Vector3), g);
                var e = kit.Group("Eyes", Vector3.zero);
                e.SetParent(g, false);
                e.localPosition = new Vector3(0.2f, h * 0.34f, 0.42f);
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.12f, 0f, 0f), new Vector3(0.12f, 0.1f, 0.05f), PatchworkAssets.Glow(new Color(1f, 0.9f, 0.4f), 2.5f), default(Vector3), e);
                e.gameObject.AddComponent<PeekingEyes>();
            }
            Trunk(g, 0.4f, h);
        }

        // A huge old oak whose wide dark crown shades the forest floor.
        static void GiantOak(LevelKit kit, Vector3 feet, float h, System.Random rng)
        {
            var g = kit.Group("GiantOak", feet);
            g.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
            var bark = PatchworkAssets.Knit(new Color(0.3f, 0.24f, 0.22f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, h * 0.35f, 0f), new Vector3(1.6f, h * 0.35f, 1.6f), bark, default(Vector3), g);
            for (int k = 0; k < 5; k++)
            {
                float ang = k * 72f;
                Rod(kit, new Vector3(0f, 0.8f, 0f), Quaternion.Euler(0f, ang, 0f) * new Vector3(0f, 0f, 1.8f), bark, 0.55f, g);
                Rod(kit, new Vector3(0f, h * 0.62f, 0f), Quaternion.Euler(0f, ang + 20f, 0f) * new Vector3(0f, h * 0.8f, 3.2f), bark, 0.4f, g);
            }
            Color[] crown = { new Color(0.12f, 0.2f, 0.22f), new Color(0.15f, 0.22f, 0.28f), new Color(0.1f, 0.17f, 0.2f) };
            for (int k = 0; k < 4; k++)
            {
                float ang = k * 90f + 45f;
                Vector3 p = new Vector3(0f, h * 0.86f + (k % 2) * 0.8f, 0f) + Quaternion.Euler(0f, ang, 0f) * new Vector3(0f, 0f, 2.6f);
                kit.Deco(PrimitiveType.Sphere, p, new Vector3(6f, 3.2f, 5.4f), PatchworkAssets.Felt(crown[k % crown.Length]), new Vector3(0f, ang, 0f), g);
            }
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, h * 0.95f, 0f), new Vector3(7f, 3.6f, 7f), PatchworkAssets.Felt(crown[0]), default(Vector3), g);
            Trunk(g, 0.85f, h);
        }

        // A mushroom: a cream stem and a round cap (glowing ones light up the dark).
        static void Mushroom(LevelKit kit, Vector3 feet, float size, Color cap, bool glow)
        {
            var g = kit.Group("Mushroom", feet);
            g.localScale = Vector3.one * size;
            g.rotation = Quaternion.Euler(0f, feet.x * 37f, (feet.z * 13f) % 12f - 6f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.4f, 0f), new Vector3(0.32f, 0.4f, 0.32f), PatchworkAssets.Felt(new Color(0.95f, 0.92f, 0.85f)), default(Vector3), g);
            var capMat = glow ? PatchworkAssets.Glow(cap, 1.5f) : PatchworkAssets.Felt(cap);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.85f, 0f), new Vector3(1.1f, 0.6f, 1.1f), capMat, default(Vector3), g);
            for (int k = 0; k < 3; k++)
            {
                float a = k * 2.1f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.3f, 1.05f, Mathf.Sin(a) * 0.3f), new Vector3(0.18f, 0.08f, 0.18f), PatchworkAssets.Felt(Color.white), default(Vector3), g);
            }
        }

        static void EyesInTheDark(LevelKit kit, Vector3 pos, float yaw, Color col)
        {
            var e = kit.Group("EyesInTheDark", pos);
            e.rotation = Quaternion.Euler(0f, yaw, 0f);
            var m = PatchworkAssets.Glow(col, 2.6f);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.13f, 0f, 0f), new Vector3(0.13f, 0.09f, 0.05f), m, new Vector3(0f, 0f, s * -12f), e);
            e.gameObject.AddComponent<PeekingEyes>();
        }

        // A cobweb strung between branches, and its felt spider bobbing on a thread.
        static void Cobweb(LevelKit kit, Vector3 c, float r, float yaw)
        {
            var silk = PatchworkAssets.Yarn(new Color(0.9f, 0.9f, 0.92f));
            var q = Quaternion.Euler(0f, yaw, 0f);
            const int spokes = 8;
            for (int k = 0; k < spokes; k++)
            {
                float a = k * Mathf.PI * 2f / spokes;
                kit.Rope(c, c + q * new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f), silk, 0.02f);
            }
            for (int ring = 1; ring <= 3; ring++)
            {
                float rr = r * ring / 3.4f;
                for (int k = 0; k < spokes; k++)
                {
                    float a0 = k * Mathf.PI * 2f / spokes, a1 = (k + 1) * Mathf.PI * 2f / spokes;
                    kit.Rope(c + q * new Vector3(Mathf.Cos(a0) * rr, Mathf.Sin(a0) * rr, 0f), c + q * new Vector3(Mathf.Cos(a1) * rr, Mathf.Sin(a1) * rr, 0f), silk, 0.015f);
                }
            }
            var spider = kit.Group("Spider", c + Vector3.down * 0.9f + q * new Vector3(0.4f, 0f, 0f));
            kit.Rope(spider.position + Vector3.up * 0.15f, c + q * new Vector3(0.4f, 0f, 0f), silk, 0.012f);
            var black = PatchworkAssets.Felt(new Color(0.12f, 0.1f, 0.14f));
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.32f, 0.28f, 0.36f), black, default(Vector3), spider);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, 0.2f), Vector3.one * 0.18f, black, default(Vector3), spider);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.05f, 0.03f, 0.28f), Vector3.one * 0.06f, PatchworkAssets.Glow(new Color(1f, 0.4f, 0.45f), 2f), default(Vector3), spider);
                for (int k = 0; k < 4; k++)
                {
                    Vector3 hip = new Vector3(s * 0.1f, 0f, 0.1f - k * 0.08f);
                    Rod(kit, hip, hip + new Vector3(s * 0.28f, -0.12f, (1.5f - k) * 0.08f), black, 0.025f, spider);
                }
            }
            var bob = spider.gameObject.AddComponent<Spinner>();
            bob.Speed = 20f;
            bob.BobHeight = 0.25f;
            bob.BobSpeed = 0.7f;
        }

        // A lost teddy sitting against a tree: one button eye, the other stitched shut.
        static void LostTeddy(LevelKit kit, Vector3 feet, float yaw)
        {
            if (!Spot(feet, 0.8f, out Vector3 p)) p = feet;
            var g = kit.Group("LostTeddy", p);
            g.rotation = Quaternion.Euler(-12f, yaw, 4f);
            var fur = PatchworkAssets.Knit(new Color(0.6f, 0.48f, 0.4f));
            var muzzle = PatchworkAssets.Felt(new Color(0.85f, 0.75f, 0.62f));
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.45f, 0f), new Vector3(0.75f, 0.8f, 0.65f), fur, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.05f, 0.05f), Vector3.one * 0.6f, fur, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.98f, 0.3f), new Vector3(0.28f, 0.22f, 0.18f), muzzle, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.02f, 0.4f), Vector3.one * 0.08f, PatchworkAssets.Plain(new Color(0.1f, 0.08f, 0.1f), 0.8f), default(Vector3), g);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.25f, 1.32f, 0f), Vector3.one * 0.22f, fur, default(Vector3), g);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.4f, 0.55f, 0.15f), new Vector3(0.22f, 0.45f, 0.22f), fur, new Vector3(30f, 0f, s * 25f), g);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.22f, 0.12f, 0.35f), new Vector3(0.26f, 0.22f, 0.4f), fur, default(Vector3), g);
            }
            // The button eye, and the stitched-shut one.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-0.12f, 1.14f, 0.27f), new Vector3(0.11f, 0.02f, 0.11f), PatchworkAssets.Plain(new Color(0.3f, 0.5f, 0.85f), 0.8f), new Vector3(80f, 0f, 0f), g);
            var thread = PatchworkAssets.Felt(new Color(0.2f, 0.15f, 0.15f));
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(0.12f, 1.14f, 0.28f), new Vector3(0.12f, 0.02f, 0.02f), thread, new Vector3(0f, 0f, s * 40f), g);
            // A patch on its tummy.
            kit.Deco(PrimitiveType.Cube, new Vector3(0.05f, 0.5f, 0.32f), new Vector3(0.28f, 0.26f, 0.03f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, new Color(0.99f, 0.96f, 0.9f), new Color(0.6f, 0.35f, 0.65f)), new Vector3(-10f, 0f, 8f), g);
            // Solid, so Patch bumps into him instead of walking through.
            var col = g.gameObject.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.75f, 0f);
            col.radius = 0.42f;
            col.height = 1.5f;
        }

        // A jack-in-the-box that sprang open long ago: the clown droops sadly on his spring.
        static void SprungJack(LevelKit kit, Vector3 feet, float yaw)
        {
            if (!Spot(feet, 1f, out Vector3 p)) p = feet;
            var g = kit.Group("SprungJack", p);
            g.rotation = Quaternion.Euler(0f, yaw, 0f);
            kit.Block(p + Vector3.up * 0.5f, Vector3.one * 1f, Color.white, "JackBox", PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.45f, 0.35f, 0.55f), new Color(0.75f, 0.7f, 0.65f))).rotation = g.rotation;
            var lid = kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.05f, -0.7f), new Vector3(1f, 0.08f, 1f), PatchworkAssets.Felt(new Color(0.55f, 0.45f, 0.6f)), new Vector3(-100f, 0f, 0f), g);
            lid.localPosition = new Vector3(0f, 1.45f, -0.55f);
            var spring = PatchworkAssets.Plain(new Color(0.7f, 0.7f, 0.74f), 0.8f);
            Vector3 prev = new Vector3(0f, 1f, 0f);
            for (int k = 0; k < 7; k++)
            {
                Vector3 next = new Vector3(Mathf.Sin(k * 0.5f) * 0.2f, 1f + (k + 1) * 0.16f, k * 0.1f);
                kit.Deco(PrimitiveType.Cylinder, next, new Vector3(0.36f, 0.02f, 0.36f), spring, new Vector3(k * 8f, 0f, 0f), g);
                prev = next;
            }
            var head = kit.Group("ClownHead", Vector3.zero);
            head.SetParent(g, false);
            head.localPosition = prev + new Vector3(0f, 0.2f, 0.25f);
            head.localRotation = Quaternion.Euler(45f, 0f, 20f);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.55f, PatchworkAssets.Felt(new Color(0.95f, 0.9f, 0.85f)), default(Vector3), head);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, 0.27f), Vector3.one * 0.14f, PatchworkAssets.Felt(new Color(0.8f, 0.25f, 0.3f)), default(Vector3), head);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.11f, 0.08f, 0.24f), Vector3.one * 0.07f, PatchworkAssets.Plain(new Color(0.1f, 0.08f, 0.1f), 0.8f), default(Vector3), head);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.3f, 0.1f, 0f), new Vector3(0.18f, 0.22f, 0.18f), PatchworkAssets.Yarn(new Color(0.55f, 0.4f, 0.65f)), default(Vector3), head);
            }
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.13f, 0.25f), new Vector3(0.2f, 0.03f, 0.02f), PatchworkAssets.Felt(new Color(0.5f, 0.15f, 0.2f)), new Vector3(0f, 0f, 0f), head);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.32f, 0f), new Vector3(0.38f, 0.14f, 0.38f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(0.45f, 0.35f, 0.55f), new Color(0.85f, 0.8f, 0.75f)), new Vector3(0f, 0f, 15f), head);
            var wobble = head.gameObject.AddComponent<Rocker>();
            wobble.Axis = Vector3.forward;
            wobble.Amount = 6f;
            wobble.Speed = 0.9f;
        }

        // The owl's hollow oak: a huge knotted trunk with a round wooden door at its foot and the
        // old owl peering out of a knot-hole above.
        static void HollowOak(LevelKit kit, WoodSecret secret)
        {
            Vector3 c = HollowTree;
            Vector3 toPath = new Vector3(-95f, 0f, -96f) - c;
            float yaw = Mathf.Atan2(toPath.x, toPath.z) * Mathf.Rad2Deg;
            var g = kit.Group("HollowOak", c);
            g.rotation = Quaternion.Euler(0f, yaw, 0f);
            var bark = PatchworkAssets.Knit(new Color(0.36f, 0.28f, 0.24f));
            var barkDark = PatchworkAssets.Knit(new Color(0.26f, 0.2f, 0.18f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 5f, 0f), new Vector3(5.2f, 5f, 5.2f), bark, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.9f, 0f), new Vector3(6.2f, 0.9f, 6.2f), bark, default(Vector3), g);
            // Bark ridges and big roots.
            for (int k = 0; k < 12; k++)
            {
                float a = k * 30f;
                if (Mathf.Abs(Mathf.DeltaAngle(a, 0f)) < 25f) continue;   // not over the door
                kit.Deco(PrimitiveType.Cube, Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 5f, 2.62f), new Vector3(0.35f, 9.5f, 0.2f), barkDark, new Vector3(0f, a, 0f), g);
            }
            for (int k = 0; k < 6; k++)
            {
                float a = k * 60f + 30f;
                Rod(kit, Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 1.6f, 2.4f), Quaternion.Euler(0f, a + 10f, 0f) * new Vector3(0f, 0.1f, 4.6f), bark, 0.9f, g);
            }
            // The doorway, the round door (hinged on the left) with iron straps and a ring.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.3f, 2.63f), new Vector3(2.3f, 0.04f, 2.3f), barkDark, new Vector3(90f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.3f, 2.66f), new Vector3(1.9f, 0.04f, 1.9f), PatchworkAssets.Plain(new Color(0.05f, 0.04f, 0.06f), 0.1f), new Vector3(90f, 0f, 0f), g);
            var hinge = kit.Group("OakDoorHinge", Vector3.zero);
            hinge.SetParent(g, false);
            hinge.localPosition = new Vector3(-0.92f, 1.3f, 2.72f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.92f, 0f, 0f), new Vector3(1.84f, 0.08f, 1.84f), PatchworkAssets.Planks(new Color(0.55f, 0.36f, 0.24f)), new Vector3(90f, 0f, 0f), hinge);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(0.7f, s * 0.45f, 0.06f), new Vector3(1.4f, 0.12f, 0.03f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), default(Vector3), hinge);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(1.5f, 0f, 0.1f), new Vector3(0.25f, 0.03f, 0.25f), PatchworkAssets.Plain(new Color(0.95f, 0.75f, 0.35f), 0.85f), new Vector3(90f, 0f, 0f), hinge);
            secret.Door = hinge;
            secret.DoorAngle = -115f;
            // The knot-hole and the old owl.
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.4f, 6.6f, 2.5f), new Vector3(1.6f, 1.9f, 0.6f), PatchworkAssets.Plain(new Color(0.05f, 0.04f, 0.06f), 0.1f), default(Vector3), g);
            var owl = kit.Group("Owl", Vector3.zero);
            owl.SetParent(g, false);
            owl.localPosition = new Vector3(0.4f, 6.3f, 2.65f);
            var feathers = PatchworkAssets.Knit(new Color(0.55f, 0.42f, 0.32f));
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(1f, 1.15f, 0.8f), feathers, default(Vector3), owl);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.15f, 0.3f), new Vector3(0.85f, 0.6f, 0.3f), PatchworkAssets.Felt(new Color(0.92f, 0.85f, 0.72f)), default(Vector3), owl);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.3f, 0.6f, 0.1f), new Vector3(0.14f, 0.3f, 0.1f), feathers, new Vector3(0f, 0f, -s * 25f), owl);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.02f, 0.47f), new Vector3(0.12f, 0.16f, 0.12f), PatchworkAssets.Felt(new Color(0.95f, 0.7f, 0.25f)), default(Vector3), owl);
            var eyes = kit.Group("OwlEyes", Vector3.zero);
            eyes.SetParent(owl, false);
            eyes.localPosition = new Vector3(0f, 0.2f, 0.42f);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.2f, 0f, 0f), new Vector3(0.26f, 0.26f, 0.08f), PatchworkAssets.Glow(new Color(1f, 0.85f, 0.3f), 2.2f), default(Vector3), eyes);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.2f, 0f, 0.04f), new Vector3(0.12f, 0.12f, 0.05f), PatchworkAssets.Plain(new Color(0.06f, 0.05f, 0.08f), 0.9f), default(Vector3), eyes);
            }
            eyes.gameObject.AddComponent<PeekingEyes>();
            secret.OwlEyes = owl;
            var turn = owl.gameObject.AddComponent<Rocker>();
            turn.Axis = Vector3.up;
            turn.Amount = 30f;
            turn.Speed = 0.5f;
            // Branches and a dark crown, and a little lantern hung by the door.
            for (int k = 0; k < 5; k++)
            {
                float a = k * 72f + 20f;
                Rod(kit, new Vector3(0f, 8.6f, 0f), Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 11.5f, 4.5f), bark, 0.6f, g);
                kit.Deco(PrimitiveType.Sphere, Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 12f, 4.8f), new Vector3(6f, 3f, 5f), PatchworkAssets.Felt(new Color(0.12f, 0.2f, 0.22f)), new Vector3(0f, a, 0f), g);
            }
            kit.Deco(PrimitiveType.Sphere, new Vector3(1.9f, 3f, 2.3f), new Vector3(0.36f, 0.48f, 0.36f), PatchworkAssets.Glow(new Color(0.55f, 1f, 0.85f), 1.6f), default(Vector3), g);
            TownKit.Solid(kit, c + Vector3.up * 5f, new Vector3(4.6f, 10f, 4.6f), yaw);

            var q = g.rotation;
            secret.Marble = SecretMarble.Place(kit, c + q * new Vector3(0f, 1.3f, 3.6f), MarbleIds[14], MarbleColor(14), true);
            secret.Prize = PrizeBubble.Place(kit, c + q * new Vector3(2.6f, 1.6f, 4.4f), "deco_owl", true);
        }

        static void SpiritLanternAt(LevelKit kit, WoodSecret secret, Vector3 feet, float pitch, Color glowCol)
        {
            var g = kit.Group("SpiritLantern", feet);
            var stick = PatchworkAssets.Wood(new Color(0.35f, 0.27f, 0.22f));
            Rod(kit, Vector3.zero, new Vector3(0.2f, 2.4f, 0f), stick, 0.12f, g);
            Rod(kit, new Vector3(0.2f, 2.4f, 0f), new Vector3(0.9f, 2.6f, 0f), stick, 0.08f, g);
            Rod(kit, new Vector3(0.85f, 2.6f, 0f), new Vector3(0.85f, 2.25f, 0f), PatchworkAssets.Yarn(new Color(0.3f, 0.3f, 0.32f)), 0.02f, g);
            var dark = PatchworkAssets.Felt(new Color(0.3f, 0.32f, 0.4f));
            var lit = PatchworkAssets.Glow(glowCol, 1.4f);
            var paper = kit.Deco(PrimitiveType.Sphere, new Vector3(0.85f, 1.9f, 0f), new Vector3(0.55f, 0.7f, 0.55f), dark, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.85f, 2.26f, 0f), new Vector3(0.3f, 0.04f, 0.3f), PatchworkAssets.Felt(new Color(0.2f, 0.2f, 0.24f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.85f, 1.53f, 0f), new Vector3(0.3f, 0.04f, 0.3f), PatchworkAssets.Felt(new Color(0.2f, 0.2f, 0.24f)), default(Vector3), g);
            // The little ghost-flame that wakes up inside.
            var flame = kit.Group("GhostFlame", Vector3.zero);
            flame.SetParent(g, false);
            flame.localPosition = new Vector3(0.85f, 1.9f, 0f);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, 0f), new Vector3(0.24f, 0.3f, 0.24f), PatchworkAssets.Glow(Color.white, 2.4f), default(Vector3), flame);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.18f, 0f), new Vector3(0.12f, 0.2f, 0.12f), PatchworkAssets.Glow(Color.white, 2.4f), default(Vector3), flame);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.05f, 0.02f, 0.12f), Vector3.one * 0.04f, PatchworkAssets.Plain(new Color(0.1f, 0.08f, 0.12f), 0.8f), default(Vector3), flame);
            flame.gameObject.SetActive(false);
            var vol = kit.Volume<SpiritLantern>(feet + new Vector3(0.6f, 1.3f, 0f), new Vector3(2.2f, 2.8f, 2.2f), "SpiritLanternTouch");
            vol.Secret = secret;
            vol.Paper = paper.GetComponent<Renderer>();
            vol.Flame = flame;
            vol.DarkMat = dark;
            vol.LitMat = lit;
            vol.Pitch = pitch;
            secret.Lanterns.Add(vol);
            TownKit.Solid(kit, feet + Vector3.up * 1.2f, new Vector3(0.25f, 2.4f, 0.25f), 0f);
        }

        // ================================================================ Button Orchard

        static void ButtonOrchard(LevelKit kit)
        {
            var rng = new System.Random(31);
            Color[] leaves = { new Color(0.45f, 0.7f, 0.4f), new Color(0.55f, 0.75f, 0.42f), new Color(0.4f, 0.62f, 0.45f) };
            Color[] buttons = { new Color(0.95f, 0.35f, 0.4f), new Color(1f, 0.8f, 0.3f), new Color(0.45f, 0.65f, 1f), new Color(0.95f, 0.55f, 0.75f), new Color(0.6f, 0.85f, 0.5f) };
            // Rows of knitted trees, with a grassy aisle down the middle.
            for (int row = 0; row < 3; row++)
                for (int col = 0; col < 7; col++)
                {
                    Vector3 near = new Vector3(-34f + col * 8.5f + (row % 2) * 4f, 0f, -109f + row * 7.5f);
                    if (!Spot(near, 2f, out Vector3 p)) continue;
                    ButtonTree(kit, p, 5f + (float)rng.NextDouble() * 1.5f, leaves[(row + col) % leaves.Length], buttons, rng);
                }
            // A ladder against one tree, baskets of buttons, fallen buttons, a cart and a bench.
            Ladder(kit, new Vector3(-16.5f, 0f, -101.5f), 20f);
            for (int i = 0; i < 4; i++)
            {
                Vector3 near = new Vector3(-30f + i * 13f, 0f, (i % 2 == 0 ? -99.6f : -95.4f));
                if (Spot(near, 0.8f, out Vector3 p)) Basket(kit, p, buttons, i);
            }
            for (int i = 0; i < 26; i++)
            {
                Vector3 p = new Vector3(-36f + (float)rng.NextDouble() * 62f, 0.03f, -112f + (float)rng.NextDouble() * 22f);
                if (Taken(p, 0.3f)) continue;
                FlatButton(kit, p, 0.3f + (float)rng.NextDouble() * 0.2f, buttons[i % buttons.Length], new Vector3(0f, i * 31f, 0f));
            }
            if (Spot(new Vector3(22f, 0f, -100f), 2f, out Vector3 cart)) TownKit.ToyCart(kit, cart, 75f);
            if (Spot(new Vector3(-6f, 0f, -99.6f), 1.6f, out Vector3 bench)) TownKit.Bench(kit, bench, 0f);
            // A little signpost where the trail to the wood leaves the orchard.
            Signpost(kit, new Vector3(-38f, 0f, -97f), new[] { 135f, 45f, -60f }, new[] { new Color(0.4f, 0.35f, 0.55f), new Color(0.9f, 0.45f, 0.5f), new Color(1f, 0.8f, 0.35f) });
            kit.Hint(new Vector3(-6f, 1.5f, -99f), new Vector3(10f, 4f, 6f), "Button Orchard",
                "The town's buttons grow here! The trail west leads to the Whispering Wood... if you dare.");
        }

        static void ButtonTree(LevelKit kit, Vector3 feet, float h, Color leaf, Color[] buttons, System.Random rng)
        {
            var g = kit.Group("ButtonTree", feet);
            g.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, h * 0.28f, 0f), new Vector3(0.45f, h * 0.28f, 0.45f), PatchworkAssets.Wood(new Color(0.55f, 0.38f, 0.26f)), default(Vector3), g);
            for (int s = -1; s <= 1; s += 2)
                Rod(kit, new Vector3(0f, h * 0.45f, 0f), new Vector3(s * 0.8f, h * 0.62f, 0.1f), PatchworkAssets.Wood(new Color(0.55f, 0.38f, 0.26f)), 0.2f, g);
            var knit = PatchworkAssets.Knit(leaf);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, h * 0.72f, 0f), new Vector3(h * 0.62f, h * 0.5f, h * 0.62f), knit, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(h * 0.22f, h * 0.82f, 0.2f), Vector3.one * h * 0.32f, PatchworkAssets.Knit(Color.Lerp(leaf, Color.white, 0.15f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(-h * 0.2f, h * 0.78f, -0.3f), Vector3.one * h * 0.3f, knit, default(Vector3), g);
            // Buttons hanging on little threads round the bottom of the crown.
            var thread = PatchworkAssets.Yarn(new Color(0.95f, 0.92f, 0.85f));
            for (int k = 0; k < 7; k++)
            {
                float a = k * Mathf.PI * 2f / 7f + (float)rng.NextDouble() * 0.4f;
                float r = h * 0.28f;
                Vector3 top = new Vector3(Mathf.Cos(a) * r, h * 0.6f, Mathf.Sin(a) * r);
                Vector3 bottom = top + Vector3.down * (0.35f + k % 3 * 0.15f);
                Rod(kit, top, bottom, thread, 0.02f, g);
                var b = kit.Group("Button", Vector3.zero);
                b.SetParent(g, false);
                b.localPosition = bottom + Vector3.down * 0.18f;
                b.localRotation = Quaternion.Euler(90f, a * Mathf.Rad2Deg, 0f);
                var col = buttons[k % buttons.Length];
                kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.36f, 0.04f, 0.36f), PatchworkAssets.Plain(col, 0.75f), default(Vector3), b);
                kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.28f, 0.045f, 0.28f), PatchworkAssets.Plain(Color.Lerp(col, Color.black, 0.12f), 0.75f), default(Vector3), b);
                for (int hgt = 0; hgt < 4; hgt++)
                {
                    float ha = hgt * Mathf.PI * 0.5f + 0.78f;
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(Mathf.Cos(ha) * 0.06f, 0f, Mathf.Sin(ha) * 0.06f), new Vector3(0.04f, 0.05f, 0.04f), PatchworkAssets.Plain(new Color(0.15f, 0.12f, 0.15f), 0.4f), default(Vector3), b);
                }
                var sway = b.gameObject.AddComponent<Rocker>();
                sway.Axis = Vector3.forward;
                sway.Amount = 12f;
                sway.Speed = 1.2f + k * 0.13f;
            }
            Trunk(g, 0.3f, h);
        }

        static void FlatButton(LevelKit kit, Vector3 pos, float size, Color col, Vector3 euler)
        {
            var b = kit.Group("FallenButton", pos);
            b.rotation = Quaternion.Euler(euler);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(size, 0.03f, size), PatchworkAssets.Plain(col, 0.75f), default(Vector3), b);
            for (int h = 0; h < 2; h++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3((h - 0.5f) * size * 0.3f, 0.005f, 0f), new Vector3(size * 0.12f, 0.03f, size * 0.12f), PatchworkAssets.Plain(new Color(0.15f, 0.12f, 0.15f), 0.4f), default(Vector3), b);
        }

        static void Ladder(LevelKit kit, Vector3 feet, float yaw)
        {
            var g = kit.Group("Ladder", feet);
            g.rotation = Quaternion.Euler(0f, yaw, 0f) * Quaternion.Euler(-16f, 0f, 0f);
            var wood = PatchworkAssets.Wood(new Color(0.85f, 0.7f, 0.5f));
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.35f, 1.7f, 0f), new Vector3(0.1f, 3.4f, 0.1f), wood, default(Vector3), g);
            for (int k = 0; k < 7; k++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.3f + k * 0.45f, 0f), new Vector3(0.06f, 0.36f, 0.06f), wood, new Vector3(0f, 0f, 90f), g);
        }

        static void Basket(LevelKit kit, Vector3 feet, Color[] buttons, int seed)
        {
            var g = kit.Group("ButtonBasket", feet);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.3f, 0f), new Vector3(0.9f, 0.3f, 0.9f), PatchworkAssets.Wicker(new Color(0.85f, 0.65f, 0.4f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.6f, 0f), new Vector3(0.96f, 0.04f, 0.96f), PatchworkAssets.Wicker(new Color(0.75f, 0.55f, 0.35f)), default(Vector3), g);
            for (int k = 0; k < 7; k++)
            {
                float a = k * 2.4f + seed;
                FlatButtonChild(kit, g, new Vector3(Mathf.Cos(a) * 0.22f * (k % 3) * 0.6f, 0.6f + k * 0.015f, Mathf.Sin(a) * 0.22f * (k % 3) * 0.6f), buttons[(k + seed) % buttons.Length], new Vector3(k * 9f, a * 50f, k * 7f));
            }
            var handle = PatchworkAssets.Wicker(new Color(0.75f, 0.55f, 0.35f));
            for (int k = 0; k < 6; k++)
            {
                float a0 = k / 6f * Mathf.PI, a1 = (k + 1) / 6f * Mathf.PI;
                Rod(kit, new Vector3(Mathf.Cos(a0) * 0.42f, 0.6f + Mathf.Sin(a0) * 0.5f, 0f), new Vector3(Mathf.Cos(a1) * 0.42f, 0.6f + Mathf.Sin(a1) * 0.5f, 0f), handle, 0.05f, g);
            }
            Trunk(g, 0.45f, 0.7f);
        }

        static void FlatButtonChild(LevelKit kit, Transform parent, Vector3 local, Color col, Vector3 euler)
        {
            var b = kit.Group("Button", Vector3.zero);
            b.SetParent(parent, false);
            b.localPosition = local;
            b.localRotation = Quaternion.Euler(euler);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.3f, 0.03f, 0.3f), PatchworkAssets.Plain(col, 0.75f), default(Vector3), b);
        }

        // A wooden signpost with arrow-shaped boards pointing different ways.
        static void Signpost(LevelKit kit, Vector3 feet, float[] yaws, Color[] cols)
        {
            if (!Spot(feet, 0.8f, out Vector3 p)) p = feet;
            var g = kit.Group("Signpost", p);
            var wood = PatchworkAssets.Wood(new Color(0.6f, 0.45f, 0.32f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.4f, 0f), new Vector3(0.18f, 1.4f, 0.18f), wood, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 2.85f, 0f), Vector3.one * 0.26f, PatchworkAssets.Felt(new Color(1f, 0.85f, 0.35f)), default(Vector3), g);
            for (int i = 0; i < yaws.Length; i++)
            {
                var arm = kit.Group("Arrow", Vector3.zero);
                arm.SetParent(g, false);
                arm.localPosition = new Vector3(0f, 2.4f - i * 0.45f, 0f);
                arm.localRotation = Quaternion.Euler(0f, yaws[i], (i % 2 == 0 ? 3f : -4f));
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, 0.6f), new Vector3(0.06f, 0.32f, 1.1f), PatchworkAssets.Felt(cols[i % cols.Length]), default(Vector3), arm);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, 1.18f), new Vector3(0.06f, 0.24f, 0.24f), PatchworkAssets.Felt(cols[i % cols.Length]), new Vector3(45f, 0f, 0f), arm);
            }
            Trunk(g, 0.2f, 2.8f);
        }

        // ================================================================ Patchwork Farm

        static void PatchworkFarm(LevelKit kit)
        {
            var rng = new System.Random(52);
            Color cream = new Color(0.99f, 0.96f, 0.9f);
            // Quilted fields: squares of different prints, with rows of felt crops in some.
            Material[] fields =
            {
                PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, new Color(0.65f, 0.82f, 0.5f), cream),
                PatchworkAssets.Felt(new Color(0.62f, 0.45f, 0.3f)),
                PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(0.95f, 0.85f, 0.45f), cream),
                PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.5f, 0.72f, 0.42f), new Color(0.6f, 0.8f, 0.5f)),
                PatchworkAssets.Felt(new Color(0.55f, 0.4f, 0.28f)),
                PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, new Color(0.72f, 0.85f, 0.55f), new Color(0.95f, 0.9f, 0.6f)),
            };
            var stitch = PatchworkAssets.Felt(new Color(0.95f, 0.9f, 0.75f));
            for (int fx = 0; fx < 4; fx++)
                for (int fz = 0; fz < 2; fz++)
                {
                    Vector3 c = new Vector3(91f + fx * 6.2f, 0.02f, 89f + fz * 6.2f);
                    int m = (fx + fz * 3) % fields.Length;
                    kit.Deco(PrimitiveType.Cube, c, new Vector3(6f, 0.02f, 6f), fields[m]);
                    // Dashed stitching round each square.
                    for (int k = 0; k < 8; k++)
                    {
                        float u = -2.6f + k * 0.75f;
                        kit.Deco(PrimitiveType.Cube, c + new Vector3(u, 0.015f, 3.02f), new Vector3(0.4f, 0.01f, 0.06f), stitch);
                        kit.Deco(PrimitiveType.Cube, c + new Vector3(3.02f, 0.015f, u), new Vector3(0.06f, 0.01f, 0.4f), stitch);
                    }
                    if (m == 1 || m == 4)
                    {
                        // Ploughed rows with little crops: pom-pom cabbages or felt carrot tops.
                        for (int r = 0; r < 4; r++)
                        {
                            Vector3 rc = c + new Vector3(0f, 0.08f, -2.1f + r * 1.4f);
                            kit.Deco(PrimitiveType.Capsule, rc, new Vector3(0.45f, 2.6f, 0.3f), PatchworkAssets.Felt(new Color(0.48f, 0.33f, 0.22f)), new Vector3(0f, 0f, 90f));
                            for (int k = 0; k < 5; k++)
                            {
                                Vector3 cp = rc + new Vector3(-2.2f + k * 1.1f, 0.18f, 0f);
                                if (m == 1) kit.Deco(PrimitiveType.Sphere, cp + Vector3.up * 0.12f, new Vector3(0.5f, 0.4f, 0.5f), PatchworkAssets.Knit(new Color(0.55f, 0.8f, 0.45f)), new Vector3(0f, k * 40f, 0f));
                                else
                                    for (int l = -1; l <= 1; l++)
                                        kit.Deco(PrimitiveType.Cube, cp + new Vector3(l * 0.06f, 0.2f, 0f), new Vector3(0.05f, 0.4f, 0.12f), PatchworkAssets.Felt(new Color(0.4f, 0.7f, 0.35f)), new Vector3(0f, 0f, l * 18f));
                            }
                        }
                    }
                    Take(c, 4f);
                }
            Scarecrow(kit, new Vector3(100.3f, 0f, 92.1f), 200f);

            // The red toy barn.
            Vector3 barn = new Vector3(105.5f, 0f, 105.5f);
            var red = PatchworkAssets.Planks(new Color(0.82f, 0.28f, 0.27f));
            kit.Block(barn + Vector3.up * 3f, new Vector3(9f, 6f, 8f), Color.white, "Barn", red);
            TownKit.GableRoof(kit, barn + Vector3.up * 6f, 9.6f, 8.8f, 3.2f, 0f, PatchworkAssets.Shingle(new Color(0.45f, 0.42f, 0.48f)), red);
            var white = PatchworkAssets.Felt(cream);
            // The big doors with their white X braces, and a hayloft door above.
            Vector3 door = barn + new Vector3(0f, 1.9f, -4.03f);
            kit.Deco(PrimitiveType.Cube, door, new Vector3(3.6f, 3.8f, 0.06f), PatchworkAssets.Planks(new Color(0.7f, 0.22f, 0.22f)));
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cube, door + new Vector3(s * 0.9f, 0f, -0.04f), new Vector3(0.16f, 4.2f, 0.04f), white, new Vector3(0f, 0f, s * 25f));
                kit.Deco(PrimitiveType.Cube, door + new Vector3(-s * 0.9f, 0f, -0.04f), new Vector3(0.16f, 4.2f, 0.04f), white, new Vector3(0f, 0f, -s * 25f));
                kit.Deco(PrimitiveType.Cube, door + new Vector3(s * 1.8f, 0f, -0.04f), new Vector3(0.18f, 3.9f, 0.05f), white);
            }
            kit.Deco(PrimitiveType.Cube, door + new Vector3(0f, 1.9f, -0.04f), new Vector3(3.8f, 0.18f, 0.05f), white);
            kit.Deco(PrimitiveType.Cube, door + new Vector3(0f, -1.9f, -0.04f), new Vector3(3.8f, 0.18f, 0.05f), white);
            kit.Deco(PrimitiveType.Cube, barn + new Vector3(0f, 6.9f, -4.4f), new Vector3(1.4f, 1.2f, 0.06f), PatchworkAssets.Plain(new Color(0.12f, 0.08f, 0.1f), 0.2f));
            kit.Deco(PrimitiveType.Sphere, barn + new Vector3(0f, 6.6f, -4.45f), new Vector3(0.9f, 0.5f, 0.6f), PatchworkAssets.Yarn(new Color(0.95f, 0.82f, 0.4f)));
            // A rooster weathervane.
            var vane = kit.Group("Rooster", barn + new Vector3(0f, 9.6f, 0f));
            Rod(kit, new Vector3(0f, -0.6f, 0f), new Vector3(0f, 0.3f, 0f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), 0.06f, vane);
            var rooster = kit.Group("RoosterTurn", Vector3.zero);
            rooster.SetParent(vane, false);
            rooster.localPosition = new Vector3(0f, 0.6f, 0f);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.12f, 0.5f, 0.7f), PatchworkAssets.Plain(new Color(0.15f, 0.12f, 0.15f), 0.6f), default(Vector3), rooster);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.32f, 0.3f), new Vector3(0.12f, 0.3f, 0.26f), PatchworkAssets.Plain(new Color(0.15f, 0.12f, 0.15f), 0.6f), default(Vector3), rooster);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.15f, -0.4f), new Vector3(0.08f, 0.6f, 0.35f), PatchworkAssets.Plain(new Color(0.15f, 0.12f, 0.15f), 0.6f), new Vector3(-30f, 0f, 0f), rooster);
            var spin = rooster.gameObject.AddComponent<Rocker>();
            spin.Axis = Vector3.up;
            spin.Amount = 40f;
            spin.Speed = 0.3f;
            Take(barn, 7f);

            // The sheep pen: a popsicle-stick fence (solid, with a gap for a gate).
            Vector3 pen0 = new Vector3(88f, 0f, 100f), pen1 = new Vector3(99f, 0f, 112f);
            PopsicleFence(kit, new Vector3(pen0.x, 0f, pen0.z), new Vector3(pen1.x, 0f, pen0.z), 3f);
            PopsicleFence(kit, new Vector3(pen1.x, 0f, pen0.z), new Vector3(pen1.x, 0f, pen1.z), -1f);
            PopsicleFence(kit, new Vector3(pen0.x, 0f, pen0.z), new Vector3(pen0.x, 0f, pen1.z), -1f);
            Take((pen0 + pen1) * 0.5f, 5.5f);
            // A water trough and hay bales.
            kit.Block(new Vector3(96.5f, 0.35f, 109.5f), new Vector3(2.4f, 0.7f, 0.9f), Color.white, "Trough", PatchworkAssets.Planks(new Color(0.6f, 0.45f, 0.32f)));
            kit.Deco(PrimitiveType.Cube, new Vector3(96.5f, 0.66f, 109.5f), new Vector3(2.1f, 0.04f, 0.6f), PatchworkAssets.Water(new Color(0.45f, 0.7f, 0.95f)));
            Vector3[] bales = { new Vector3(102f, 0f, 98.5f), new Vector3(104.2f, 0f, 98.8f), new Vector3(103.1f, 1.7f, 98.6f), new Vector3(110f, 0f, 99f) };
            foreach (var b in bales)
            {
                kit.YarnBall(b, 0.9f, new Color(0.95f, 0.82f, 0.4f));
                Take(b, 1f);
            }
            // A pinwheel windmill.
            Vector3 mill = new Vector3(110f, 0f, 90f);
            if (Spot(mill, 1.5f, out Vector3 mp))
            {
                var post = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.9f, 0.3f, 0.35f), cream);
                kit.Deco(PrimitiveType.Cylinder, mp + Vector3.up * 4.5f, new Vector3(0.4f, 4.5f, 0.4f), post);
                TownKit.Solid(kit, mp + Vector3.up * 4.5f, new Vector3(0.5f, 9f, 0.5f), 0f);
                var hub = kit.Group("Pinwheel", mp + new Vector3(0f, 9f, -0.35f));
                hub.rotation = Quaternion.Euler(0f, 200f, 0f);
                Color[] bladeCols = { LevelKit.Palette.Rose, LevelKit.Palette.Butter, LevelKit.Palette.Sky, LevelKit.Palette.Sage };
                for (int b = 0; b < 4; b++)
                {
                    kit.Deco(PrimitiveType.Cube, Quaternion.Euler(0f, 0f, b * 90f) * new Vector3(0.9f, 0.45f, 0f), new Vector3(1.8f, 0.9f, 0.04f), PatchworkAssets.Felt(bladeCols[b]), new Vector3(0f, 0f, b * 90f + 18f), hub);
                }
                kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.4f, PatchworkAssets.Felt(cream), default(Vector3), hub);
                var turn = hub.gameObject.AddComponent<Spinner>();
                turn.Axis = Vector3.forward;
                turn.Speed = 80f;
            }
            kit.Hint(new Vector3(100f, 1.5f, 92f), new Vector3(8f, 4f, 6f), "Patchwork Farm",
                "A tiny toy farm, stitched together from leftover fabric. Mind the sheep!");
        }

        static void Scarecrow(LevelKit kit, Vector3 feet, float yaw)
        {
            var g = kit.Group("Scarecrow", feet);
            g.rotation = Quaternion.Euler(0f, yaw, 0f);
            var wood = PatchworkAssets.Wood(new Color(0.6f, 0.45f, 0.32f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.3f, 0f), new Vector3(0.14f, 1.3f, 0.14f), wood, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 2f, 0f), new Vector3(0.1f, 1.1f, 0.1f), wood, new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.75f, 0f), new Vector3(1f, 0.9f, 0.4f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, new Color(0.95f, 0.92f, 0.85f), new Color(0.35f, 0.5f, 0.85f)), default(Vector3), g);
            var straw = PatchworkAssets.Yarn(new Color(0.95f, 0.82f, 0.4f));
            for (int s = -1; s <= 1; s += 2)
                for (int k = 0; k < 3; k++)
                    Rod(kit, new Vector3(s * 1.05f, 2f, 0f), new Vector3(s * 1.25f, 1.85f - k * 0.1f, (k - 1) * 0.08f), straw, 0.04f, g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 2.55f, 0f), new Vector3(0.6f, 0.6f, 0.55f), PatchworkAssets.Fabric(new Color(0.92f, 0.85f, 0.7f)), default(Vector3), g);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.13f, 2.62f, 0.27f), new Vector3(0.1f, 0.02f, 0.1f), PatchworkAssets.Plain(new Color(0.15f, 0.12f, 0.15f), 0.6f), new Vector3(80f, 0f, 0f), g);
            }
            for (int k = 0; k < 5; k++)
                kit.Deco(PrimitiveType.Cube, new Vector3(-0.15f + k * 0.075f, 2.42f, 0.27f), new Vector3(0.03f, 0.08f, 0.02f), PatchworkAssets.Felt(new Color(0.3f, 0.2f, 0.2f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 2.85f, 0f), new Vector3(1.1f, 0.03f, 1.1f), straw, new Vector3(0f, 0f, 6f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 3.02f, 0f), new Vector3(0.55f, 0.18f, 0.55f), straw, new Vector3(0f, 0f, 6f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 2.92f, 0f), new Vector3(0.57f, 0.05f, 0.57f), PatchworkAssets.Felt(new Color(0.9f, 0.3f, 0.35f)), new Vector3(0f, 0f, 6f), g);
            kit.YarnBird(feet + Vector3.up * 3.2f, LevelKit.Palette.Sky, false);
            TownKit.Solid(kit, feet + Vector3.up * 1.4f, new Vector3(0.4f, 2.8f, 0.4f), 0f);
        }

        // A fence of giant lolly sticks between two points (solid). gapAt > 0 leaves a gap for a
        // gate that many metres along.
        static void PopsicleFence(LevelKit kit, Vector3 a, Vector3 b, float gapAt)
        {
            var stick = PatchworkAssets.Wood(new Color(0.92f, 0.82f, 0.62f));
            Vector3 d = b - a;
            float len = d.magnitude;
            Vector3 dir = d / len;
            float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            int posts = Mathf.CeilToInt(len / 1.1f);
            for (int k = 0; k <= posts; k++)
            {
                float u = k * len / posts;
                if (gapAt > 0f && u > gapAt - 0.4f && u < gapAt + 2.4f) continue;
                kit.Deco(PrimitiveType.Capsule, a + dir * u + Vector3.up * 0.65f, new Vector3(0.32f, 0.6f, 0.08f), stick, new Vector3(0f, yaw + 90f, 0f));
            }
            for (int r = 0; r < 2; r++)
            {
                float y = 0.45f + r * 0.5f;
                if (gapAt > 0f)
                {
                    Rail(kit, a, a + dir * (gapAt - 0.5f), y, stick);
                    Rail(kit, a + dir * (gapAt + 2.5f), b, y, stick);
                }
                else Rail(kit, a, b, y, stick);
            }
            if (gapAt > 0f)
            {
                SolidBetween(kit, a, a + dir * (gapAt - 0.5f));
                SolidBetween(kit, a + dir * (gapAt + 2.5f), b);
            }
            else SolidBetween(kit, a, b);
        }

        static void Rail(LevelKit kit, Vector3 a, Vector3 b, float y, Material mat)
        {
            Vector3 d = b - a;
            float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            kit.Deco(PrimitiveType.Capsule, (a + b) * 0.5f + Vector3.up * y, new Vector3(0.22f, d.magnitude * 0.5f, 0.07f), mat, new Vector3(90f, yaw, 0f));
        }

        static void SolidBetween(LevelKit kit, Vector3 a, Vector3 b)
        {
            Vector3 d = b - a;
            float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            TownKit.Solid(kit, (a + b) * 0.5f + Vector3.up * 0.65f, new Vector3(0.2f, 1.3f, d.magnitude), yaw);
        }

        // ================================================================ Toadstool Dell

        static void ToadstoolDell(LevelKit kit)
        {
            Vector3 pond = new Vector3(-62f, 0f, 103f);
            var rng = new System.Random(77);
            // The pond: satin water in a ring of pebbles.
            kit.Deco(PrimitiveType.Cylinder, pond + Vector3.up * 0.03f, new Vector3(14f, 0.02f, 11f), PatchworkAssets.Water(new Color(0.4f, 0.65f, 0.9f)));
            kit.Deco(PrimitiveType.Cylinder, pond + Vector3.up * 0.025f, new Vector3(15f, 0.02f, 12f), PatchworkAssets.Felt(new Color(0.55f, 0.48f, 0.4f)));
            for (int k = 0; k < 30; k++)
            {
                float a = k * Mathf.PI * 2f / 30f;
                Vector3 p = pond + new Vector3(Mathf.Cos(a) * 7.4f, 0.08f, Mathf.Sin(a) * 5.9f);
                kit.Deco(PrimitiveType.Sphere, p, new Vector3(0.7f + k % 3 * 0.2f, 0.3f, 0.6f), PatchworkAssets.Felt(k % 2 == 0 ? new Color(0.75f, 0.72f, 0.68f) : new Color(0.65f, 0.62f, 0.6f)), new Vector3(0f, a * 57f, 0f));
            }
            Take(pond, 7.6f);
            ClearGrass(pond, 7.2f);
            // Lily pads, some with a felt flower, and cattails round the edge.
            for (int k = 0; k < 9; k++)
            {
                Vector3 p = pond + new Vector3(Mathf.Cos(k * 2.3f) * (1.5f + k % 4 * 1.2f), 0.06f, Mathf.Sin(k * 2.3f) * (1.2f + k % 4 * 0.95f));
                var pad = kit.Group("LilyPad", p);
                pad.rotation = Quaternion.Euler(0f, k * 47f, 0f);
                kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.2f, 0.02f, 1.2f), PatchworkAssets.Felt(new Color(0.4f, 0.7f, 0.35f)), default(Vector3), pad);
                kit.Deco(PrimitiveType.Cube, new Vector3(0.45f, 0.01f, 0f), new Vector3(0.5f, 0.03f, 0.12f), PatchworkAssets.Water(new Color(0.4f, 0.65f, 0.9f)), new Vector3(0f, 0f, 0f), pad);
                if (k % 3 == 0)
                    for (int f = 0; f < 5; f++)
                        kit.Deco(PrimitiveType.Sphere, new Vector3(-0.1f + Mathf.Cos(f * 1.26f) * 0.15f, 0.1f, Mathf.Sin(f * 1.26f) * 0.15f), new Vector3(0.18f, 0.08f, 0.1f), PatchworkAssets.Felt(new Color(1f, 0.7f, 0.82f)), new Vector3(0f, f * 72f, 20f), pad);
                var bob = pad.gameObject.AddComponent<Spinner>();
                bob.Speed = 3f;
                bob.BobHeight = 0.02f;
            }
            for (int k = 0; k < 12; k++)
            {
                float a = 0.4f + k * 0.42f;
                Vector3 p = pond + new Vector3(Mathf.Cos(a) * 7.9f, 0f, Mathf.Sin(a) * 6.4f);
                float h = 1.4f + k % 3 * 0.35f;
                Rod(kit, p, p + new Vector3(Mathf.Sin(k) * 0.15f, h, 0f), PatchworkAssets.Felt(new Color(0.45f, 0.65f, 0.35f)), 0.05f);
                kit.Deco(PrimitiveType.Capsule, p + new Vector3(Mathf.Sin(k) * 0.12f, h * 0.85f, 0f), new Vector3(0.14f, 0.2f, 0.14f), PatchworkAssets.Felt(new Color(0.5f, 0.32f, 0.22f)), default(Vector3));
            }
            // A little jetty.
            Vector3 jetty = pond + new Vector3(5.5f, 0f, -4.5f);
            var planks = PatchworkAssets.Planks(new Color(0.75f, 0.58f, 0.42f));
            kit.Block(jetty + new Vector3(-1.3f, 0.25f, 1.1f), new Vector3(1.6f, 0.15f, 4f), Color.white, "Jetty", planks).rotation = Quaternion.Euler(0f, -50f, 0f);
            // Giant bouncy toadstools to hop between.
            Vector3[] stools = { pond + new Vector3(-10f, 0f, -2f), pond + new Vector3(-13f, 0f, 4f), pond + new Vector3(10.5f, 0f, 4.5f) };
            float[] heights = { 1.8f, 3f, 2.2f };
            for (int i = 0; i < stools.Length; i++)
            {
                if (!Spot(stools[i], 2f, out Vector3 p)) continue;
                BouncyToadstool(kit, p, heights[i], i);
            }
            // A fairy ring of tiny red toadstools, and a few spotted ones scattered about.
            Vector3 ring = pond + new Vector3(3f, 0f, -8.8f);
            for (int k = 0; k < 12; k++)
            {
                float a = k * Mathf.PI * 2f / 12f;
                Mushroom(kit, ring + new Vector3(Mathf.Cos(a) * 2.2f, 0f, Mathf.Sin(a) * 2.2f), 0.42f, new Color(0.9f, 0.25f, 0.3f), false);
            }
            Take(ring, 2.6f);
            // Score bubbles dancing in the ring (stand in the middle for luck).
            for (int k = 0; k < 8; k++)
            {
                float a = k * Mathf.PI * 0.25f;
                ScoreBubbles.Place(kit, ring + new Vector3(Mathf.Cos(a) * 1.2f, 1.1f + (k % 2) * 0.4f, Mathf.Sin(a) * 1.2f), 20);
            }
            for (int i = 0; i < 10; i++)
            {
                Vector3 near = pond + new Vector3((float)(rng.NextDouble() - 0.5) * 34f, 0f, (float)(rng.NextDouble() - 0.5) * 16f);
                if (Spot(near, 0.6f, out Vector3 p)) Mushroom(kit, p, 0.5f + (float)rng.NextDouble() * 0.4f, i % 2 == 0 ? new Color(0.9f, 0.3f, 0.32f) : new Color(1f, 0.75f, 0.35f), false);
            }
            for (int i = 0; i < 4; i++)
                if (Spot(pond + new Vector3(-20f + i * 13f, 0f, (i % 2 == 0 ? -9f : 8f)), 2f, out Vector3 tp)) kit.YarnTree(tp, 5.5f + i % 2, FlowerColors[(i + 2) % FlowerColors.Length]);
            kit.Hint(pond + new Vector3(0f, 1.5f, -9f), new Vector3(10f, 4f, 4f), "Toadstool Dell",
                "A quiet little pond where the frogs live. The big toadstools are ever so bouncy!");
        }

        static void BouncyToadstool(LevelKit kit, Vector3 feet, float h, int seed)
        {
            var g = kit.Group("BouncyToadstool", feet);
            g.rotation = Quaternion.Euler(0f, seed * 70f, 0f);
            var cream = PatchworkAssets.Felt(new Color(0.97f, 0.94f, 0.86f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, h * 0.5f, 0f), new Vector3(0.9f, h * 0.5f, 0.9f), cream, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, h * 0.62f, 0f), new Vector3(1.25f, 0.08f, 1.25f), cream, default(Vector3), g);
            var cap = PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, seed % 2 == 0 ? new Color(0.9f, 0.25f, 0.3f) : new Color(0.95f, 0.6f, 0.25f), new Color(0.99f, 0.96f, 0.9f));
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, h, 0f), new Vector3(3.2f, 1.3f, 3.2f), cap, default(Vector3), g);
            kit.Block(feet + Vector3.up * (h * 0.5f), new Vector3(0.9f, h, 0.9f), Color.white, "ToadstoolStem").GetComponent<Renderer>().enabled = false;
            kit.Block(feet + Vector3.up * (h + 0.3f), new Vector3(2.4f, 0.4f, 2.4f), Color.white, "ToadstoolCap").GetComponent<Renderer>().enabled = false;
            var pad = kit.Volume<BouncePad>(feet + Vector3.up * (h + 0.9f), new Vector3(2.2f, 0.8f, 2.2f), "ToadstoolBounce");
            pad.LaunchSpeed = 15f + h;
        }

        // ================================================================ Wildflower Meadow

        static void WildflowerMeadow(LevelKit kit)
        {
            var rng = new System.Random(88);
            // Long felt grass tufts and drifts of flowers.
            for (int i = 0; i < 70; i++)
            {
                Vector3 p = new Vector3(92f + (float)rng.NextDouble() * 21f, 0f, 14f + (float)rng.NextDouble() * 40f);
                if (Taken(p, 0.4f)) continue;
                if (i % 2 == 0) kit.Flower(p, FlowerColors[i % FlowerColors.Length]);
                else
                {
                    var tuft = kit.Group("Tuft", p);
                    for (int k = 0; k < 3; k++)
                        kit.Deco(PrimitiveType.Cube, new Vector3((k - 1) * 0.08f, 0.35f, 0f), new Vector3(0.06f, 0.7f + k % 2 * 0.2f, 0.12f), PatchworkAssets.Felt(new Color(0.5f, 0.72f, 0.4f)), new Vector3(0f, k * 50f, (k - 1) * 14f), tuft);
                }
            }
            // A beehive (a striped yarn skep) on a stand, with toy bees looping round it.
            if (Spot(new Vector3(106f, 0f, 44f), 1.5f, out Vector3 hive))
            {
                var g = kit.Group("Beehive", hive);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.5f, 0f), new Vector3(0.9f, 0.5f, 0.9f), PatchworkAssets.Wood(new Color(0.6f, 0.45f, 0.32f)), default(Vector3), g);
                for (int k = 0; k < 5; k++)
                {
                    float r = Mathf.Lerp(1.3f, 0.5f, k / 4f);
                    kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.2f + k * 0.28f, 0f), new Vector3(r, 0.4f, r), PatchworkAssets.Yarn(k % 2 == 0 ? new Color(1f, 0.82f, 0.35f) : new Color(0.95f, 0.7f, 0.3f)), default(Vector3), g);
                }
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.25f, 0.55f), new Vector3(0.3f, 0.22f, 0.1f), PatchworkAssets.Plain(new Color(0.12f, 0.08f, 0.1f), 0.2f), default(Vector3), g);
                Trunk(g, 0.7f, 2.4f);
                for (int b = 0; b < 4; b++)
                {
                    var orbit = kit.Group("BeeOrbit", Vector3.zero);
                    var o = orbit.gameObject.AddComponent<Orbiter>();
                    o.Center = hive + Vector3.up * (1.6f + b * 0.3f);
                    o.Radius = 1.6f + b * 0.5f;
                    o.RadiusZ = 1.2f + b * 0.4f;
                    o.Speed = (b % 2 == 0 ? 1f : -1f) * (70f + b * 15f);
                    o.Angle = b * 90f;
                    o.Bob = 0.25f;
                    o.Lean = 0f;
                    var bee = kit.Group("Bee", Vector3.zero);
                    bee.SetParent(orbit, false);
                    bee.localPosition = Vector3.zero;
                    bee.localScale = Vector3.one * 0.6f;
                    kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.3f, 0.28f, 0.42f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(1f, 0.8f, 0.25f), new Color(0.15f, 0.12f, 0.15f)), new Vector3(0f, 90f, 0f), bee);
                    for (int s = -1; s <= 1; s += 2)
                        kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.15f, 0.15f, 0f), new Vector3(0.22f, 0.04f, 0.14f), PatchworkAssets.Bubble(new Color(0.9f, 0.95f, 1f, 0.5f)), new Vector3(0f, 0f, s * 30f), bee);
                }
            }
            // A picnic for the deer: a gingham blanket and a toppled basket of felt apples.
            if (Spot(new Vector3(100f, 0f, 24f), 2.4f, out Vector3 pic))
            {
                kit.Deco(PrimitiveType.Cube, pic + Vector3.up * 0.02f, new Vector3(3.4f, 0.02f, 2.6f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, new Color(0.99f, 0.96f, 0.9f), new Color(0.45f, 0.65f, 0.95f)), new Vector3(0f, -12f, 0f));
                kit.Deco(PrimitiveType.Cylinder, pic + new Vector3(0.8f, 0.3f, 0.4f), new Vector3(0.8f, 0.3f, 0.8f), PatchworkAssets.Wicker(new Color(0.85f, 0.65f, 0.4f)), new Vector3(0f, 0f, 75f));
                for (int k = 0; k < 4; k++)
                    kit.Deco(PrimitiveType.Sphere, pic + new Vector3(-0.2f - k * 0.35f, 0.14f, 0.2f + (k % 2) * 0.3f), Vector3.one * 0.28f, PatchworkAssets.Felt(new Color(0.9f, 0.25f, 0.28f)), default(Vector3));
            }
            kit.Hint(new Vector3(100f, 1.5f, 32f), new Vector3(10f, 4f, 10f), "Wildflower Meadow",
                "Shh... the wooden deer are very shy. Walk slowly and they might let you get close.");
        }

        // ================================================================ Biomes

        // Each area gets its own ground colour, a proper edge and an entrance, and the main
        // junctions get signposts (colour-coded: purple wood, gold orchard, red farm, lime meadow,
        // teal dell, cream town, pink Pod).
        static readonly Color SignWood = new Color(0.55f, 0.45f, 0.75f), SignOrchard = new Color(1f, 0.78f, 0.3f), SignFarm = new Color(0.88f, 0.32f, 0.3f),
            SignMeadow = new Color(0.6f, 0.82f, 0.4f), SignDell = new Color(0.35f, 0.72f, 0.7f), SignTown = new Color(0.97f, 0.92f, 0.82f), SignPod = new Color(1f, 0.55f, 0.72f);

        static void Biomes(LevelKit kit)
        {
            var tint = kit.Group("BiomeGround", Vector3.zero);
            // Ground colours (just above the grass, under everything else).
            kit.Deco(PrimitiveType.Cube, new Vector3(-5f, 0.012f, -101f), new Vector3(62f, 0.02f, 26f), PatchworkAssets.Grass(new Color(0.62f, 0.72f, 0.4f)), default(Vector3), tint);
            kit.Deco(PrimitiveType.Cube, new Vector3(78.3f, 0.012f, -100.2f), new Vector3(73.2f, 0.02f, 28.6f), PatchworkAssets.Grass(new Color(0.58f, 0.78f, 0.44f)), default(Vector3), tint);
            kit.Deco(PrimitiveType.Cube, new Vector3(101.4f, 0.012f, 33f), new Vector3(27.2f, 0.02f, 50f), PatchworkAssets.Grass(new Color(0.68f, 0.8f, 0.48f)), default(Vector3), tint);
            kit.Deco(PrimitiveType.Cube, new Vector3(-62f, 0.012f, 102.4f), new Vector3(52f, 0.02f, 25.2f), PatchworkAssets.Grass(new Color(0.38f, 0.6f, 0.4f)), default(Vector3), tint);
            kit.Deco(PrimitiveType.Cube, new Vector3(99.4f, 0.012f, 98.4f), new Vector3(31.2f, 0.02f, 33.2f), PatchworkAssets.Grass(new Color(0.62f, 0.74f, 0.4f)), default(Vector3), tint);

            // The orchard's hedge along its north side, with a flower arch where the lane comes in.
            var hedge = PatchworkAssets.Knit(new Color(0.34f, 0.55f, 0.34f));
            var hedgeLight = PatchworkAssets.Knit(new Color(0.42f, 0.62f, 0.38f));
            for (float x = -35f; x <= 25f; x += 1.6f)
            {
                if (Mathf.Abs(x - 1f) < 3f) continue;
                Vector3 p = new Vector3(x, 0f, -88.6f);
                if (NearLane(p, 0.2f)) continue;
                kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 0.65f, new Vector3(1.9f, 1.4f, 1.3f), ((int)(x * 2f)) % 2 == 0 ? hedge : hedgeLight, new Vector3(0f, x * 31f, 0f));
                if (((int)(x * 10f)) % 3 == 0) kit.Deco(PrimitiveType.Sphere, p + new Vector3(0.3f, 1.3f, -0.4f), Vector3.one * 0.16f, PatchworkAssets.Felt(FlowerColors[((int)(x * 3f) + 99) % FlowerColors.Length]), default(Vector3));
            }
            TownKit.Solid(kit, new Vector3(-16.5f, 0.7f, -88.6f), new Vector3(37f, 1.4f, 1.2f), 0f);
            TownKit.Solid(kit, new Vector3(14.5f, 0.7f, -88.6f), new Vector3(21f, 1.4f, 1.2f), 0f);
            TownKit.FlowerArch(kit, new Vector3(1f, 0f, -88.6f), 0f, 4f);
            Take(new Vector3(1f, 0f, -88.6f), 2.5f);

            // Signposts at the junctions.
            Signpost(kit, new Vector3(-55.5f, 0f, -24f), new[] { 90f, 225f, 300f }, new[] { SignTown, SignWood, SignWood });
            Signpost(kit, new Vector3(4.5f, 0f, -85.5f), new[] { 0f, 270f, 115f }, new[] { SignTown, SignWood, SignPod });
            Signpost(kit, new Vector3(-5f, 0f, -94f), new[] { 270f, 90f, 0f }, new[] { SignWood, SignPod, SignTown });
            Signpost(kit, new Vector3(64f, 0f, 55f), new[] { 30f, 95f, 270f }, new[] { SignFarm, SignMeadow, SignTown });
            Signpost(kit, new Vector3(-14f, 0f, 84.5f), new[] { 290f, 45f, 180f }, new[] { SignDell, SignTown, SignTown });
            Signpost(kit, new Vector3(52f, 0f, -88f), new[] { 270f, 0f, 60f }, new[] { SignOrchard, SignTown, SignMeadow });
        }

        // ================================================================ Little scenes in the gaps

        // Felt butterflies fluttering about an area.
        static void Butterflies(LevelKit kit, Vector3 c, int count, float spread, int seed)
        {
            var rng = new System.Random(seed);
            var swarm = kit.Group("Butterflies", Vector3.zero);
            Color[] cols = { LevelKit.Palette.Rose, LevelKit.Palette.Butter, LevelKit.Palette.Sky, LevelKit.Palette.Lavender, new Color(1f, 0.6f, 0.3f) };
            var body = PatchworkAssets.Felt(new Color(0.2f, 0.16f, 0.2f));
            for (int i = 0; i < count; i++)
            {
                var b = kit.Group("Butterfly", Vector3.zero);
                b.SetParent(swarm, false);
                b.localPosition = c + new Vector3((float)(rng.NextDouble() - 0.5) * spread, 0.8f + (float)rng.NextDouble() * 1.6f, (float)(rng.NextDouble() - 0.5) * spread);
                kit.Deco(PrimitiveType.Capsule, Vector3.zero, new Vector3(0.04f, 0.09f, 0.04f), body, new Vector3(90f, 0f, 0f), b);
                var wing = PatchworkAssets.Felt(cols[i % cols.Length]);
                var spot = PatchworkAssets.Felt(cols[(i + 2) % cols.Length]);
                for (int s = -1; s <= 1; s += 2)
                {
                    var w = kit.Group("Wing", Vector3.zero);
                    w.SetParent(b, false);
                    kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.11f, 0f, 0.03f), new Vector3(0.2f, 0.01f, 0.16f), wing, default(Vector3), w);
                    kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.09f, 0f, -0.08f), new Vector3(0.14f, 0.01f, 0.12f), wing, default(Vector3), w);
                    kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.12f, 0.006f, 0.03f), new Vector3(0.06f, 0.01f, 0.06f), spot, default(Vector3), w);
                    var flap = w.gameObject.AddComponent<Rocker>();
                    flap.Axis = Vector3.forward;
                    flap.Amount = 55f * s;
                    flap.Speed = 13f + (float)rng.NextDouble() * 3f;
                }
                foreach (var r in b.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            var fs = swarm.gameObject.AddComponent<FireflySwarm>();
            fs.Pulse = false;
            fs.Wander = 2.5f;
        }

        static void Vignettes(LevelKit kit)
        {
            Butterflies(kit, new Vector3(-6f, 0f, -101f), 8, 40f, 1);
            Butterflies(kit, new Vector3(97f, 0f, 92f), 6, 16f, 2);
            Butterflies(kit, new Vector3(-62f, 0f, 103f), 6, 22f, 3);
            Butterflies(kit, new Vector3(102f, 0f, 32f), 8, 30f, 4);
            Butterflies(kit, new Vector3(66f, 0f, -98f), 6, 30f, 5);
            Campsite(kit, new Vector3(-99f, 0f, -16f));
            KiteHill(kit, new Vector3(-6f, 0f, 106f));
            Sandpit(kit, new Vector3(101f, 0f, -48f));
        }

        // A toy soldier's campsite: a striped felt tent, a crackling campfire of glowing felt
        // flames, log seats, a kettle, and the soldier fast asleep on his bedroll.
        static void Campsite(LevelKit kit, Vector3 c)
        {
            // Its clearing in the Birch Edge was kept free when the wood was planted.
            if (!FindSpot(c, 1.2f, out Vector3 p)) p = c;
            var g = kit.Group("Campsite", p);
            var canvas = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.35f, 0.55f, 0.4f), new Color(0.95f, 0.9f, 0.75f));
            // The tent: two sloped panels and a dark doorway.
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.85f, 1f, 3f), new Vector3(0.06f, 2.3f, 3f), canvas, new Vector3(0f, 0f, -s * 36f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.75f, 1.52f), new Vector3(1.2f, 1.4f, 0.04f), PatchworkAssets.Plain(new Color(0.1f, 0.08f, 0.1f), 0.2f), new Vector3(0f, 0f, 0f), g);
            Rod(kit, new Vector3(0f, 0f, 1.45f), new Vector3(0f, 1.95f, 1.45f), PatchworkAssets.Wood(new Color(0.6f, 0.45f, 0.32f)), 0.06f, g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 2.15f, 1.4f), new Vector3(0.02f, 0.3f, 0.4f), PatchworkAssets.Felt(new Color(0.9f, 0.3f, 0.35f)), default(Vector3), g);
            TownKit.Solid(kit, p + new Vector3(0f, 0.9f, 3f), new Vector3(2.6f, 1.8f, 3f), 0f);
            // The campfire.
            var rocks = PatchworkAssets.Felt(new Color(0.65f, 0.62f, 0.6f));
            for (int k = 0; k < 8; k++)
            {
                float a = k * Mathf.PI * 0.25f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.65f, 0.1f, Mathf.Sin(a) * 0.65f - 1f), new Vector3(0.32f, 0.22f, 0.28f), rocks, default(Vector3), g);
            }
            for (int k = 0; k < 3; k++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.15f, -1f), new Vector3(0.16f, 0.45f, 0.16f), PatchworkAssets.Wood(new Color(0.45f, 0.32f, 0.22f)), new Vector3(70f, k * 60f, 0f), g);
            var fire = kit.Group("Flames", Vector3.zero);
            fire.SetParent(g, false);
            fire.localPosition = new Vector3(0f, 0.3f, -1f);
            Color[] flame = { new Color(1f, 0.55f, 0.2f), new Color(1f, 0.8f, 0.3f), new Color(1f, 0.4f, 0.25f) };
            for (int k = 0; k < 5; k++)
            {
                float a = k * 1.26f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.15f, 0.25f + (k % 2) * 0.15f, Mathf.Sin(a) * 0.15f), new Vector3(0.22f, 0.55f - k * 0.05f, 0.22f), PatchworkAssets.Glow(flame[k % 3], 2.4f), new Vector3(0f, 0f, (k - 2) * 8f), fire);
            }
            var flick = fire.gameObject.AddComponent<Rocker>();
            flick.Axis = Vector3.forward;
            flick.Amount = 6f;
            flick.Speed = 7f;
            var smoke = new GameObject("Smoke");
            smoke.transform.SetParent(g, false);
            smoke.transform.localPosition = new Vector3(0f, 1.2f, -1f);
            var sm = smoke.AddComponent<Smoker>();
            sm.Every = 1.8f;
            sm.Kind = FxBurst.Kind.Puff;
            sm.Tint = new Color(0.8f, 0.8f, 0.82f);
            // Log seats and a kettle.
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 1.7f, 0.25f, -1f), new Vector3(0.5f, 0.6f, 0.5f), PatchworkAssets.Wood(new Color(0.55f, 0.38f, 0.26f)), new Vector3(90f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.9f, 0.25f, -1.9f), new Vector3(0.4f, 0.32f, 0.4f), PatchworkAssets.Plain(new Color(0.3f, 0.45f, 0.7f), 0.7f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(1.1f, 0.3f, -1.9f), new Vector3(0.06f, 0.12f, 0.06f), PatchworkAssets.Plain(new Color(0.3f, 0.45f, 0.7f), 0.7f), new Vector3(0f, 0f, -60f), g);
            // The sleepy tin soldier on his bedroll, snoring little bubbles.
            kit.Deco(PrimitiveType.Capsule, new Vector3(-2.4f, 0.12f, 0.6f), new Vector3(0.7f, 0.9f, 0.3f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, new Color(0.9f, 0.3f, 0.35f), new Color(0.99f, 0.96f, 0.9f)), new Vector3(90f, 0f, 0f), g);
            var soldier = kit.Group("SleepySoldier", Vector3.zero);
            soldier.SetParent(g, false);
            soldier.localPosition = new Vector3(-2.4f, 0.35f, 0.6f);
            soldier.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0f), new Vector3(0.36f, 0.35f, 0.36f), PatchworkAssets.Plain(new Color(0.85f, 0.2f, 0.25f), 0.6f), default(Vector3), soldier);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.55f, 0f), Vector3.one * 0.32f, PatchworkAssets.Felt(new Color(1f, 0.85f, 0.72f)), default(Vector3), soldier);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.85f, 0f), new Vector3(0.3f, 0.2f, 0.3f), PatchworkAssets.Felt(new Color(0.15f, 0.12f, 0.18f)), default(Vector3), soldier);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.5f, 0f), new Vector3(0.34f, 0.18f, 0.34f), PatchworkAssets.Plain(new Color(0.2f, 0.25f, 0.45f), 0.6f), default(Vector3), soldier);
            var breathe = soldier.gameObject.AddComponent<Rocker>();
            breathe.Axis = Vector3.right;
            breathe.Amount = 3f;
            breathe.Speed = 1.1f;
            var zz = kit.Group("Snores", Vector3.zero);
            zz.SetParent(g, false);
            zz.localPosition = new Vector3(-2.4f, 1.2f, 0.9f);
            for (int k = 0; k < 3; k++)
                kit.Deco(PrimitiveType.Sphere, new Vector3(k * 0.18f, k * 0.25f, 0f), Vector3.one * (0.1f + k * 0.05f), PatchworkAssets.Bubble(new Color(0.85f, 0.95f, 1f, 0.45f)), default(Vector3), zz);
            var zb = zz.gameObject.AddComponent<Spinner>();
            zb.Speed = 15f;
            zb.BobHeight = 0.15f;
            zb.BobSpeed = 0.9f;
            kit.Hint(p + new Vector3(0f, 1.5f, -3f), new Vector3(6f, 4f, 4f), "Sleepy Campsite", "Shh! Private Plink is having a nap. Even toy soldiers need a day off.");
        }

        // Kites flying high on long strings from little stakes, tugging in the breeze.
        static void KiteHill(LevelKit kit, Vector3 c)
        {
            Color[] cols = { LevelKit.Palette.Rose, LevelKit.Palette.Sky, LevelKit.Palette.Butter };
            for (int i = 0; i < 3; i++)
            {
                Vector3 near = c + new Vector3((i - 1) * 6f, 0f, (i % 2) * 2.5f);
                if (!Spot(near, 1f, out Vector3 stake)) continue;
                kit.Deco(PrimitiveType.Cylinder, stake + Vector3.up * 0.3f, new Vector3(0.1f, 0.3f, 0.1f), PatchworkAssets.Wood(new Color(0.6f, 0.45f, 0.32f)));
                kit.Deco(PrimitiveType.Cylinder, stake + Vector3.up * 0.25f, new Vector3(0.35f, 0.12f, 0.35f), PatchworkAssets.Yarn(Color.white));
                var pivot = kit.Group("KiteString", stake + Vector3.up * 0.5f);
                Vector3 kiteAt = new Vector3((i - 1) * 2.5f, 13f + i * 2.5f, 5f + i);
                Rod(kit, Vector3.zero, kiteAt, PatchworkAssets.Yarn(new Color(0.95f, 0.95f, 0.95f)), 0.02f, pivot);
                var kite = kit.Group("Kite", Vector3.zero);
                kite.SetParent(pivot, false);
                kite.localPosition = kiteAt;
                kite.localRotation = Quaternion.Euler(-20f, 0f, 45f);
                kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(1.6f, 1.6f, 0.04f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, cols[i], new Color(0.99f, 0.96f, 0.9f)), default(Vector3), kite);
                kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(2.2f, 0.06f, 0.06f), PatchworkAssets.Wood(new Color(0.9f, 0.8f, 0.6f)), new Vector3(0f, 0f, 45f), kite);
                kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(2.2f, 0.06f, 0.06f), PatchworkAssets.Wood(new Color(0.9f, 0.8f, 0.6f)), new Vector3(0f, 0f, -45f), kite);
                for (int b = 0; b < 4; b++)
                    kit.Deco(PrimitiveType.Cube, new Vector3(-1.2f - b * 0.45f, -1.2f - b * 0.45f, 0f), new Vector3(0.25f, 0.12f, 0.03f), PatchworkAssets.Felt(cols[(i + b + 1) % 3]), new Vector3(0f, 0f, 45f + (b % 2) * 50f), kite);
                var tug = pivot.gameObject.AddComponent<Rocker>();
                tug.Axis = new Vector3(1f, 0f, 0.4f).normalized;
                tug.Amount = 4f + i;
                tug.Speed = 0.7f + i * 0.2f;
                var flutter = kite.gameObject.AddComponent<Rocker>();
                flutter.Axis = Vector3.forward;
                flutter.Amount = 10f;
                flutter.Speed = 2.2f + i * 0.4f;
            }
        }

        // A sandpit framed in wooden boards: a sandcastle with a flag, a bucket and spade, and a
        // half-buried toy boat.
        static void Sandpit(LevelKit kit, Vector3 c)
        {
            if (!Spot(c, 4.5f, out Vector3 p)) return;
            var sand = PatchworkAssets.Felt(new Color(0.96f, 0.86f, 0.62f));
            var boards = PatchworkAssets.Planks(new Color(0.75f, 0.58f, 0.42f));
            kit.Deco(PrimitiveType.Cube, p + Vector3.up * 0.1f, new Vector3(7f, 0.2f, 7f), sand);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Block(p + new Vector3(s * 3.6f, 0.25f, 0f), new Vector3(0.25f, 0.5f, 7.45f), Color.white, "SandpitBoard", boards);
                kit.Block(p + new Vector3(0f, 0.25f, s * 3.6f), new Vector3(7.45f, 0.5f, 0.25f), Color.white, "SandpitBoard", boards);
            }
            // The sandcastle.
            Vector3 castle = p + new Vector3(-1f, 0.2f, 0.8f);
            kit.Deco(PrimitiveType.Cylinder, castle + Vector3.up * 0.4f, new Vector3(1.6f, 0.4f, 1.6f), sand);
            for (int k = 0; k < 4; k++)
            {
                float a = k * Mathf.PI * 0.5f + 0.78f;
                Vector3 t = castle + new Vector3(Mathf.Cos(a) * 0.8f, 0f, Mathf.Sin(a) * 0.8f);
                kit.Deco(PrimitiveType.Cylinder, t + Vector3.up * 0.55f, new Vector3(0.5f, 0.55f, 0.5f), sand);
                for (int m = 0; m < 4; m++)
                    kit.Deco(PrimitiveType.Cube, t + new Vector3(Mathf.Cos(m * 1.57f) * 0.2f, 1.15f, Mathf.Sin(m * 1.57f) * 0.2f), new Vector3(0.12f, 0.14f, 0.12f), sand);
            }
            kit.Deco(PrimitiveType.Cylinder, castle + Vector3.up * 1.2f, new Vector3(0.7f, 0.4f, 0.7f), sand);
            Rod(kit, castle + Vector3.up * 1.6f, castle + Vector3.up * 2.4f, PatchworkAssets.Wood(new Color(0.9f, 0.8f, 0.6f)), 0.04f);
            var fl = kit.Group("Flag", castle + Vector3.up * 2.25f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.25f, 0f, 0f), new Vector3(0.5f, 0.3f, 0.02f), PatchworkAssets.Felt(LevelKit.Palette.Rose), default(Vector3), fl);
            var wave = fl.gameObject.AddComponent<Rocker>();
            wave.Axis = Vector3.up;
            wave.Amount = 20f;
            wave.Speed = 3f;
            TownKit.Solid(kit, castle + Vector3.up * 0.6f, new Vector3(2f, 1.2f, 2f), 0f);
            // Bucket and spade.
            Vector3 bucket = p + new Vector3(1.8f, 0.2f, -1.4f);
            kit.Deco(PrimitiveType.Cylinder, bucket + Vector3.up * 0.35f, new Vector3(0.7f, 0.35f, 0.7f), PatchworkAssets.Plain(LevelKit.Palette.Sky, 0.6f));
            kit.Deco(PrimitiveType.Cylinder, bucket + Vector3.up * 0.71f, new Vector3(0.6f, 0.02f, 0.6f), sand);
            Rod(kit, p + new Vector3(0.6f, 0.25f, -2.2f), p + new Vector3(1.4f, 1.2f, -2f), PatchworkAssets.Plain(LevelKit.Palette.Butter, 0.6f), 0.06f);
            kit.Deco(PrimitiveType.Cube, p + new Vector3(0.5f, 0.3f, -2.25f), new Vector3(0.35f, 0.04f, 0.45f), PatchworkAssets.Plain(LevelKit.Palette.Butter, 0.6f), new Vector3(30f, 20f, 0f));
            // A toy boat, half buried.
            var boat = kit.Group("SandBoat", p + new Vector3(1.6f, 0.15f, 1.9f));
            boat.rotation = Quaternion.Euler(14f, 30f, -18f);
            kit.Deco(PrimitiveType.Capsule, Vector3.zero, new Vector3(0.6f, 0.7f, 0.35f), PatchworkAssets.Plain(new Color(0.9f, 0.3f, 0.35f), 0.6f), new Vector3(90f, 0f, 0f), boat);
            Rod(kit, Vector3.zero, new Vector3(0f, 1f, 0f), PatchworkAssets.Wood(new Color(0.9f, 0.8f, 0.6f)), 0.04f, boat);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.6f, 0.2f), new Vector3(0.02f, 0.6f, 0.4f), PatchworkAssets.Felt(Color.white), default(Vector3), boat);
            ClearGrass(p, 4f);
        }

        // ================================================================ Trails between it all

        // Every trail between the areas (kept clear of trees and props, then laid with stones).
        static Vector3[][] TrailLines()
        {
            return new[]
            {
                // The Pod's meadow, over the canal footbridge, through the orchard, over the ravine
                // footbridge and into the wood.
                new[] { new Vector3(47f, 0f, -97f), new Vector3(28f, 0f, -97f), new Vector3(0f, 0f, -97.5f), new Vector3(-34f, 0f, -97f), new Vector3(-36f, 0f, -92f), new Vector3(-52.5f, 0f, -92f), new Vector3(-60f, 0f, -86f), WoodGate + new Vector3(4f, 0f, 1.8f) },
                // The weathervane lane down to the orchard.
                new[] { new Vector3(0f, 0f, -80f), new Vector3(1f, 0f, -96f) },
                // The Pod's meadow east to the sandpit and the Sky Hoops.
                new[] { new Vector3(58f, 0f, -86f), new Vector3(80f, 0f, -66f), new Vector3(96f, 0f, -52f) },
                // The canal promenade out to the Wildflower Meadow.
                new[] { new Vector3(47f, 0f, 20f), new Vector3(70f, 0f, 24f), new Vector3(92f, 0f, 28f) },
                // The toy workshop north to the farm.
                new[] { new Vector3(64f, 0f, 52f), new Vector3(70f, 0f, 66f), new Vector3(78f, 0f, 84f), new Vector3(86f, 0f, 92f), new Vector3(93f, 0f, 99f) },
                // The picnic park west to the dell.
                new[] { new Vector3(-16f, 0f, 82f), new Vector3(-24f, 0f, 90f), new Vector3(-30f, 0f, 96f), new Vector3(-44f, 0f, 101f), new Vector3(-52f, 0f, 104f) },
            };
        }

        static void Trails(LevelKit kit)
        {
            var stones = new[] { PatchworkAssets.Felt(new Color(0.9f, 0.84f, 0.74f)), PatchworkAssets.Felt(new Color(0.84f, 0.78f, 0.7f)) };
            var mossy = new[] { PatchworkAssets.Cobble(new Color(0.55f, 0.6f, 0.55f)), PatchworkAssets.Cobble(new Color(0.48f, 0.52f, 0.5f)) };
            foreach (var trail in TrailLines())
            {
                int n = 0;
                for (int s = 0; s < trail.Length - 1; s++)
                {
                    Vector3 a = trail[s], b = trail[s + 1];
                    int steps = Mathf.CeilToInt(Vector3.Distance(a, b) / 2.3f);
                    for (int k = 0; k < steps; k++)
                    {
                        Vector3 p = Vector3.Lerp(a, b, k / (float)steps) + new Vector3(Mathf.Sin(n * 1.1f) * 0.35f, 0f, Mathf.Cos(n * 0.8f) * 0.35f);
                        n++;
                        if (Taken(p, 0.6f)) continue;
                        // Only on open ground (not up on anything, not over water).
                        if (!Physics.Raycast(p + Vector3.up * 6f, Vector3.down, out RaycastHit hit, 12f, ~(1 << 2), QueryTriggerInteraction.Ignore) || Mathf.Abs(hit.point.y) > 0.1f) continue;
                        var mat = InForest(p) ? mossy[n % 2] : stones[n % 2];
                        kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.025f, new Vector3(1.2f, 0.02f, 0.95f), mat, new Vector3(0f, n * 41f, 0f));
                        // Tiny flowers and pebbles edging the path now and then.
                        if (n % 7 == 0 && !InForest(p)) kit.Flower(p + new Vector3(1.1f, 0f, 0.4f), FlowerColors[n % FlowerColors.Length]);
                        else if (n % 9 == 0) kit.Deco(PrimitiveType.Sphere, p + new Vector3(-1f, 0.05f, 0.3f), new Vector3(0.3f, 0.14f, 0.24f), PatchworkAssets.Felt(new Color(0.75f, 0.72f, 0.68f)), default(Vector3));
                    }
                }
            }
        }

        // ================================================================ Toy wildlife

        static void Wildlife(LevelKit kit)
        {
            var rng = new System.Random(99);
            Color[] bunnies = { new Color(0.98f, 0.96f, 0.92f), new Color(0.85f, 0.75f, 0.65f), new Color(0.75f, 0.72f, 0.8f), new Color(1f, 0.85f, 0.88f) };
            Color[] shells = { new Color(0.95f, 0.45f, 0.5f), new Color(0.45f, 0.65f, 1f), new Color(1f, 0.8f, 0.3f) };
            System.Action<Vector3, System.Func<Vector3, ToyCritter>> at = (near, make) =>
            {
                if (FindSpot(near, 0.5f, out Vector3 p)) make(p + Vector3.up * 0.02f);
            };
            // The farm.
            for (int i = 0; i < 4; i++) at(new Vector3(91f + i * 2f, 0f, 104f + (i % 2) * 3f), p => { var c = ToyCritter.Sheep(kit, p); c.Home = new Vector3(93.5f, p.y, 106f); c.Radius = 3.5f; return c; });
            at(new Vector3(98f, 0f, 96f), p => ToyCritter.Duck(kit, p));
            at(new Vector3(104f, 0f, 94f), p => ToyCritter.Duck(kit, p));
            at(new Vector3(95f, 0f, 86f), p => ToyCritter.Bunny(kit, p, bunnies[0]));
            // The orchard.
            at(new Vector3(-20f, 0f, -104f), p => ToyCritter.Bunny(kit, p, bunnies[1]));
            at(new Vector3(6f, 0f, -108f), p => ToyCritter.Bunny(kit, p, bunnies[3]));
            at(new Vector3(-28f, 0f, -92f), p => ToyCritter.Hedgehog(kit, p));
            at(new Vector3(14f, 0f, -93f), p => ToyCritter.Hedgehog(kit, p));
            at(new Vector3(-10f, 0f, -110f), p => ToyCritter.Mouse(kit, p));
            at(new Vector3(18f, 0f, -104f), p => ToyCritter.Mouse(kit, p));
            at(new Vector3(-2f, 0f, -102f), p => ToyCritter.Snail(kit, p, shells[0]));
            at(new Vector3(-30f, 0f, -110f), p => ToyCritter.Snail(kit, p, shells[1]));
            // The dell.
            Vector3 pond = new Vector3(-62f, 0f, 103f);
            for (int i = 0; i < 4; i++) at(pond + new Vector3(Mathf.Cos(i * 1.6f) * 8.6f, 0f, Mathf.Sin(i * 1.6f) * 7f), p => { var c = ToyCritter.Frog(kit, p); c.Home = p; c.Radius = 3.5f; return c; });
            at(pond + new Vector3(-12f, 0f, 7f), p => ToyCritter.Duck(kit, p));
            at(pond + new Vector3(14f, 0f, -6f), p => ToyCritter.Snail(kit, p, shells[2]));
            at(pond + new Vector3(-16f, 0f, -6f), p => ToyCritter.Bunny(kit, p, bunnies[2]));
            // The meadow.
            for (int i = 0; i < 3; i++) at(new Vector3(98f + i * 4f, 0f, 20f + i * 11f), p => ToyCritter.Deer(kit, p));
            at(new Vector3(104f, 0f, 38f), p => ToyCritter.Bunny(kit, p, bunnies[0]));
            at(new Vector3(96f, 0f, 48f), p => ToyCritter.Bunny(kit, p, bunnies[1]));
            at(new Vector3(108f, 0f, 16f), p => ToyCritter.Mouse(kit, p));
            // Landing Meadow and round the edges of town.
            at(new Vector3(70f, 0f, -92f), p => ToyCritter.Bunny(kit, p, bunnies[3]));
            at(new Vector3(88f, 0f, -96f), p => ToyCritter.Deer(kit, p));
            at(new Vector3(58f, 0f, -110f), p => ToyCritter.Hedgehog(kit, p));
            at(new Vector3(-58f, 0f, 70f), p => ToyCritter.Deer(kit, p));
            at(new Vector3(-80f, 0f, 30f), p => ToyCritter.Hedgehog(kit, p));
            at(new Vector3(-90f, 0f, -20f), p => ToyCritter.Mouse(kit, p));
            at(new Vector3(-100f, 0f, 0f), p => ToyCritter.Bunny(kit, p, bunnies[2]));
            at(new Vector3(84f, 0f, -40f), p => ToyCritter.Mouse(kit, p));
            // Foxes at the edge of the wood.
            at(new Vector3(-66f, 0f, -70f), p => ToyCritter.Fox(kit, p));
            at(new Vector3(-74f, 0f, -98f), p => ToyCritter.Fox(kit, p));

            // Wild yarn horses, in ones and twos out in the open country. Hop on to ride!
            Color[] coats = { new Color(0.97f, 0.93f, 0.85f), new Color(0.72f, 0.45f, 0.3f), new Color(0.7f, 0.7f, 0.76f), new Color(1f, 0.75f, 0.82f), new Color(0.6f, 0.75f, 0.95f), new Color(0.3f, 0.26f, 0.3f) };
            Color[] manes = { new Color(1f, 0.6f, 0.7f), new Color(0.98f, 0.9f, 0.7f), new Color(0.4f, 0.4f, 0.5f), new Color(0.75f, 0.55f, 0.95f), new Color(1f, 0.85f, 0.4f), new Color(0.95f, 0.95f, 0.95f) };
            Vector3[] herd = { new Vector3(100f, 0f, 38f), new Vector3(106f, 0f, 22f), new Vector3(104f, 0f, 84f), new Vector3(84f, 0f, -100f), new Vector3(-76f, 0f, -18f), new Vector3(-44f, 0f, 92f), new Vector3(-95f, 0f, 26f), new Vector3(46f, 0f, 104f) };
            for (int i = 0; i < herd.Length; i++)
            {
                if (!FindSpot(herd[i], 1.2f, out Vector3 hp)) continue;
                var h = YarnHorse.Build(kit, hp + Vector3.up * 0.02f, coats[i % coats.Length], manes[i % manes.Length], i * 67f);
                h.Radius = 9f;
            }
        }
    }
}

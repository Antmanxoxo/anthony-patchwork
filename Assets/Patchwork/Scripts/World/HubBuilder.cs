using UnityEngine;

namespace Patchwork
{
    // The hub - "The Cardboard Skiff". A little cardboard ship hanging under a patched-up balloon,
    // sailing over a sea of paint. Stand at the helm and turn the quilted globe to pick a level.
    // The deck has a practice spot for each form, and the captain's cabin at the back is the
    // Button Box (sewn shut until every golden button is found).
    //
    // Layout (deck top is y = 0, the bow points +z):
    //   stern  z -16 .. -7.6  captain's cabin (centre), drain crate (west), air vent (east)
    //   middle z  -7  ..  9   spawn, pin-cushion, mast with crow's nest + fountain, lifeboat swing (west)
    //   bow    z  10  .. 26   the helm and the globe
    public static class HubBuilder
    {
        public const string SceneName = "Hub";

        static readonly Color Kraft = new Color(0.8f, 0.62f, 0.42f);
        static readonly Color KraftDark = new Color(0.62f, 0.45f, 0.3f);
        static readonly Color KraftLight = new Color(0.88f, 0.74f, 0.55f);
        const float DeckHalfWidth = 8f;
        const float SternZ = -16f, MidBowZ = 18f;

        public static LevelInfo Build(LevelKit kit)
        {
            var info = kit.Info;
            info.Id = SceneName;
            info.IsHub = true;
            info.Title = LevelCatalog.Skiff.Title;
            info.Subtitle = "Stand at the helm and turn the globe to choose where to sail.";
            info.Spawn = new Vector3(0f, 0.1f, -4f);
            info.SpawnYaw = 0f;
            info.KillY = -10f;
            info.Sky = new Color(0.72f, 0.84f, 0.96f);
            info.Area.SetMinMax(new Vector3(-30f, -8f, -30f), new Vector3(30f, 30f, 40f));

            Hull(kit);
            Balloon(kit);
            Sky(kit);

            kit.AddCheckpoint(new Vector3(0f, 0f, -4f), 0f);
            kit.Hint(new Vector3(0f, 1.5f, -3f), new Vector3(10f, 4f, 6f), "Welcome aboard the Skiff!",
                "Move, {Jump} to jump, and press {Wheel} to switch between Water, Yarn and Wind. {Satchel} opens the Satchel to swap hats.");

            HelmAndGlobe(kit);
            MastAndFountain(kit);
            Lifeboat(kit);
            Cabin(kit);
            DrainCrate(kit);
            AirVent(kit);

            // A pin-cushion to bounce on, with threads to grab.
            kit.AddBouncePad(new Vector3(4.5f, 0f, -1f));
            kit.Thread(new Vector3(4.5f, 3f, -1f), LevelKit.Palette.Gold);
            kit.Thread(new Vector3(4.5f, 4.6f, -1f), LevelKit.Palette.Gold);
            kit.ThreadLine(new Vector3(-3f, 1f, 9f), new Vector3(3f, 1f, 9f), 3);

            // Deck clutter.
            kit.Spool(new Vector3(-6.2f, 0f, 8.5f), 1.4f, LevelKit.Palette.Rose);
            kit.Spool(new Vector3(-6.6f, 1.4f, 8.5f), 1f, LevelKit.Palette.Sky);
            kit.YarnBall(new Vector3(6.3f, 0f, 9f), 0.8f, new Color(0.95f, 0.55f, 0.6f));
            kit.YarnBall(new Vector3(-6.3f, 0f, -5.5f), 0.6f, new Color(0.55f, 0.7f, 0.95f));
            kit.Cardboard(new Vector3(6.4f, 0.6f, 4.8f), new Vector3(1.6f, 1.2f, 1.6f), KraftLight, "SmallBox");
            kit.Tape(new Vector3(6.4f, 1.21f, 4.8f), new Vector3(1.62f, 0.02f, 0.35f));
            kit.Thimble(new Vector3(6.4f, 1.2f, 4.8f), 0.6f);

            return info;
        }

        // ---------------------------------------------------------------- The ship

        static void Hull(LevelKit kit)
        {
            var kraft = PatchworkAssets.Cardboard(Kraft);
            float len = MidBowZ - SternZ;
            float midZ = (MidBowZ + SternZ) * 0.5f;

            // Deck (rectangle plus a diamond for the pointed bow).
            kit.Cardboard(new Vector3(0f, -0.5f, midZ), new Vector3(DeckHalfWidth * 2f, 1f, len), Kraft, "Deck");
            var bowDeck = kit.Cardboard(new Vector3(0f, -0.5f, MidBowZ), new Vector3(11.31f, 1f, 11.31f), Kraft, "BowDeck");
            bowDeck.rotation = Quaternion.Euler(0f, 45f, 0f);
            // Packing tape across the deck seams.
            for (float z = SternZ + 4f; z < MidBowZ; z += 6f)
                kit.Tape(new Vector3(0f, 0.005f, z), new Vector3(DeckHalfWidth * 2f, 0.02f, 0.5f));
            kit.Tape(new Vector3(0f, 0.005f, MidBowZ + 2f), new Vector3(0.5f, 0.02f, 12f));

            // Hull below the deck, a bit darker.
            kit.Cardboard(new Vector3(0f, -3f, midZ), new Vector3(DeckHalfWidth * 2f - 0.4f, 4f, len - 0.2f), KraftDark, "Hull");
            var bowHull = kit.Cardboard(new Vector3(0f, -3f, MidBowZ), new Vector3(11f, 4f, 11f), KraftDark, "BowHull");
            bowHull.rotation = Quaternion.Euler(0f, 45f, 0f);
            kit.Cardboard(new Vector3(0f, -5.4f, midZ + 2f), new Vector3(6f, 1f, len - 4f), KraftDark, "Keel");

            // Round portholes drawn along both sides.
            var glass = PatchworkAssets.Plain(new Color(0.55f, 0.75f, 0.92f), 0.9f);
            var ring = PatchworkAssets.Plain(new Color(0.25f, 0.2f, 0.25f), 0.3f);
            for (float z = SternZ + 3f; z < MidBowZ; z += 5f)
                for (int s = -1; s <= 1; s += 2)
                {
                    float x = s * (DeckHalfWidth - 0.15f);
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(x, -2.4f, z), new Vector3(1.3f, 0.06f, 1.3f), ring, new Vector3(0f, 0f, 90f));
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(x + s * 0.02f, -2.4f, z), new Vector3(1.05f, 0.07f, 1.05f), glass, new Vector3(0f, 0f, 90f));
                }

            // Rails: low cardboard walls, with a gap on the west side for the lifeboat swing.
            Rail(kit, new Vector3(DeckHalfWidth - 0.15f, 0f, SternZ), new Vector3(DeckHalfWidth - 0.15f, 0f, MidBowZ));
            Rail(kit, new Vector3(-DeckHalfWidth + 0.15f, 0f, SternZ), new Vector3(-DeckHalfWidth + 0.15f, 0f, -1f));
            Rail(kit, new Vector3(-DeckHalfWidth + 0.15f, 0f, 5f), new Vector3(-DeckHalfWidth + 0.15f, 0f, MidBowZ));
            Rail(kit, new Vector3(-DeckHalfWidth, 0f, SternZ + 0.15f), new Vector3(DeckHalfWidth, 0f, SternZ + 0.15f));
            Rail(kit, new Vector3(DeckHalfWidth - 0.15f, 0f, MidBowZ), new Vector3(0f, 0f, MidBowZ + 7.85f));
            Rail(kit, new Vector3(-DeckHalfWidth + 0.15f, 0f, MidBowZ), new Vector3(0f, 0f, MidBowZ + 7.85f));
            kit.Hint(new Vector3(-7f, 1.5f, 2f), new Vector3(3f, 4f, 6f), "Practice: Yarn Swing",
                "In Yarn Form, hold {Ability} near the pink button to swing over to the lifeboat. Swing back the same way.", HintTheme.Yarn);

            // Propellers at the stern: spool hubs with lolly-stick blades.
            Propeller(kit, new Vector3(-4f, -2.5f, SternZ - 0.6f), 1f);
            Propeller(kit, new Vector3(4f, -2.5f, SternZ - 0.6f), -1f);
            // Rudder.
            var rudder = kit.Cardboard(new Vector3(0f, -3.2f, SternZ - 1.4f), new Vector3(0.3f, 3.6f, 2.4f), KraftLight, "Rudder");
            Object.Destroy(rudder.GetComponent<Collider>());
            // A pencil bowsprit poking out the front.
            var pencil = PatchworkAssets.Plain(new Color(0.98f, 0.8f, 0.3f), 0.4f);
            kit.Rope(new Vector3(0f, 0.6f, MidBowZ + 6.5f), new Vector3(0f, 2.4f, MidBowZ + 13f), pencil, 0.35f);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 2.45f, MidBowZ + 13.1f), new Vector3(0.36f, 0.36f, 0.5f), PatchworkAssets.Plain(new Color(0.3f, 0.3f, 0.32f), 0.3f));

            // Bunting from the mast to the bow and the stern.
            kit.Bunting(new Vector3(0f, 12f, 6f), new Vector3(0f, 1.4f, MidBowZ + 7.6f), LevelKit.Palette.Quilt);
            kit.Bunting(new Vector3(0f, 12f, 6f), new Vector3(0f, 7f, SternZ + 1f), LevelKit.Palette.Quilt);
        }

        static void Rail(LevelKit kit, Vector3 a, Vector3 b)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            var rail = kit.Cardboard((a + b) * 0.5f + Vector3.up * 0.5f, new Vector3(0.3f, 1f, len), KraftLight, "Rail");
            rail.rotation = Quaternion.LookRotation(d.normalized, Vector3.up);
            // Rolled-paper cap along the top.
            var cap = kit.Deco(PrimitiveType.Cylinder, (a + b) * 0.5f + Vector3.up * 1.02f, new Vector3(0.36f, len * 0.5f, 0.36f), PatchworkAssets.Felt(new Color(0.95f, 0.9f, 0.8f)));
            cap.rotation = Quaternion.FromToRotation(Vector3.up, d.normalized);
        }

        static void Propeller(LevelKit kit, Vector3 pos, float dir)
        {
            var g = kit.Group("Propeller", pos);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.9f, 0.5f, 0.9f), PatchworkAssets.Yarn(LevelKit.Palette.Rose), new Vector3(90f, 0f, 0f), g);
            var blades = new GameObject("Blades").transform;
            blades.SetParent(g, false);
            blades.localPosition = new Vector3(0f, 0f, -0.55f);
            var stick = PatchworkAssets.Wood(new Color(0.9f, 0.78f, 0.58f));
            for (int i = 0; i < 3; i++)
            {
                var b = kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(0.45f, 2.4f, 0.08f), stick, new Vector3(0f, 0f, i * 120f), blades);
                b.localPosition = Quaternion.Euler(0f, 0f, i * 120f) * new Vector3(0f, 1.1f, 0f);
            }
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, -0.6f), Vector3.one * 0.5f, PatchworkAssets.Felt(LevelKit.Palette.Gold), default(Vector3), g);
            var spin = blades.gameObject.AddComponent<Spinner>();
            spin.Axis = Vector3.forward;
            spin.Speed = 420f * dir;
        }

        // A patched-up felt balloon held on by yarn ropes.
        static void Balloon(LevelKit kit)
        {
            Vector3 c = new Vector3(0f, 22f, 1f);
            Vector3 r = new Vector3(10f, 6f, 15f);
            var g = kit.Group("Balloon", c);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, r * 2f, PatchworkAssets.Fabric(new Color(0.93f, 0.6f, 0.62f)), default(Vector3), g);

            // Patches sewn all over it.
            Color[] patchCols = { LevelKit.Palette.Butter, LevelKit.Palette.Sky, LevelKit.Palette.Cream, LevelKit.Palette.Lavender, LevelKit.Palette.Sage, LevelKit.Palette.Peach };
            int n = 0;
            for (int ring = 0; ring < 4; ring++)
            {
                float theta = Mathf.Lerp(0.35f, 1.75f, ring / 3f);   // from near the top to just under the middle
                int count = 6 + ring * 2;
                for (int i = 0; i < count; i++)
                {
                    float phi = (i + (ring % 2) * 0.5f) / count * Mathf.PI * 2f;
                    Vector3 unit = new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi), Mathf.Cos(theta), Mathf.Sin(theta) * Mathf.Sin(phi));
                    Vector3 p = Vector3.Scale(unit, r);
                    Vector3 normal = new Vector3(p.x / (r.x * r.x), p.y / (r.y * r.y), p.z / (r.z * r.z)).normalized;
                    float size = Random.Range(2.2f, 3.4f);
                    var patch = kit.Deco(PrimitiveType.Cube, p + normal * 0.05f, new Vector3(size, size * Random.Range(0.7f, 1f), 0.12f),
                        PatchworkAssets.Fabric(patchCols[n++ % patchCols.Length]), default(Vector3), g);
                    patch.localRotation = Quaternion.LookRotation(normal, Vector3.up) * Quaternion.Euler(0f, 0f, Random.Range(-20f, 20f));
                }
            }
            // A stitched band round the middle.
            for (int i = 0; i < 28; i++)
            {
                float phi = i / 28f * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Cos(phi) * r.x, -0.6f, Mathf.Sin(phi) * r.z);
                var st = kit.Deco(PrimitiveType.Cube, p * 1.005f, new Vector3(1.4f, 0.18f, 0.12f), PatchworkAssets.Felt(new Color(1f, 0.96f, 0.88f)), default(Vector3), g);
                st.localRotation = Quaternion.LookRotation(new Vector3(p.x / (r.x * r.x), 0f, p.z / (r.z * r.z)).normalized, Vector3.up);
            }
            var bob = g.gameObject.AddComponent<Spinner>();
            bob.Speed = 0f;
            bob.BobHeight = 0.25f;
            bob.BobSpeed = 0.5f;

            // Ropes from the rails up to the balloon's belly.
            var rope = PatchworkAssets.Yarn(new Color(0.96f, 0.92f, 0.82f));
            float[] zs = { SternZ + 1f, 1f, MidBowZ - 1f };
            foreach (float z in zs)
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 deck = new Vector3(s * (DeckHalfWidth - 0.2f), 1f, z);
                    // Where the line from the balloon's centre to the rail meets the balloon.
                    Vector3 d = deck - c;
                    float k = 1f / Mathf.Sqrt((d.x / r.x) * (d.x / r.x) + (d.y / r.y) * (d.y / r.y) + (d.z / r.z) * (d.z / r.z));
                    kit.Rope(deck, c + d * k * 0.98f, rope, 0.09f);
                }
        }

        // Sky all around: drifting clouds above and below, a far sea of paint, and the basket island
        // where Level 1 is, off in the distance.
        static void Sky(LevelKit kit)
        {
            kit.PaintLake(new Vector3(0f, -70f, 20f), new Vector2(900f, 900f), 60f);
            var drift = kit.Group("DriftingClouds", Vector3.zero);
            for (int i = 0; i < 18; i++)
            {
                float side = (i % 2 == 0 ? -1f : 1f);
                float x = side * (16f + (i * 7) % 30);
                float y = (i % 3 == 0) ? -14f - (i % 4) * 4f : (i % 3 == 1 ? 6f + (i % 5) * 3f : -4f - (i % 3));
                float z = -130f + i * 17f;
                var cl = kit.Cloud(new Vector3(x, y, z), 2.2f + (i % 3) * 0.9f);
                cl.SetParent(drift, true);
                var d = cl.gameObject.AddComponent<CloudDrift>();
                d.Speed = 3f + (i % 4) * 0.6f;
            }

            // The Sewing Basket Garden, far away below.
            var island = kit.Group("BasketIsland", new Vector3(70f, -55f, 150f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0f), new Vector3(46f, 4f, 46f), PatchworkAssets.Grass(new Color(0.55f, 0.75f, 0.45f)), default(Vector3), island);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 10f, 0f), new Vector3(28f, 8f, 28f), PatchworkAssets.Wicker(new Color(0.8f, 0.6f, 0.38f)), default(Vector3), island);
            kit.Deco(PrimitiveType.Sphere, new Vector3(-8f, 20f, 4f), Vector3.one * 9f, PatchworkAssets.Yarn(new Color(0.95f, 0.55f, 0.6f)), default(Vector3), island);
            kit.Deco(PrimitiveType.Sphere, new Vector3(6f, 19f, -5f), Vector3.one * 7f, PatchworkAssets.Yarn(new Color(0.55f, 0.7f, 0.95f)), default(Vector3), island);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(4f, 24f, 6f), new Vector3(0.8f, 10f, 0.8f), PatchworkAssets.Plain(new Color(0.8f, 0.82f, 0.88f), 0.9f), new Vector3(20f, 0f, -15f), island);
            // Two mystery islands for the levels still being made.
            var m1 = kit.Group("MysteryIsland", new Vector3(-90f, -50f, 60f));
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(36f, 3f, 36f), PatchworkAssets.Fabric(new Color(0.6f, 0.65f, 0.88f)), default(Vector3), m1);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 6f, 0f), new Vector3(22f, 14f, 22f), PatchworkAssets.Felt(new Color(0.85f, 0.85f, 0.95f)), default(Vector3), m1);
            var m2 = kit.Group("MysteryIsland", new Vector3(-30f, -60f, 220f));
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(40f, 3f, 40f), PatchworkAssets.Fabric(new Color(0.9f, 0.65f, 0.72f)), default(Vector3), m2);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 9f, 0f), new Vector3(14f, 16f, 14f), PatchworkAssets.Knit(new Color(0.95f, 0.85f, 0.6f)), new Vector3(0f, 30f, 0f), m2);
        }

        // ---------------------------------------------------------------- The helm and the globe

        static void HelmAndGlobe(LevelKit kit)
        {
            Vector3 globeCenter = new Vector3(0f, 3.4f, 19f);
            const float radius = 2.4f;

            // Stand: a cardboard tube with a felt cushion, and a brass-coloured half ring.
            kit.Cardboard(new Vector3(0f, 0.45f, globeCenter.z), new Vector3(2.4f, 0.9f, 2.4f), KraftLight, "GlobeStand");
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.95f, globeCenter.z), new Vector3(2.2f, 0.06f, 2.2f), PatchworkAssets.Felt(LevelKit.Palette.Rose));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1f, globeCenter.z), new Vector3(0.18f, 0.4f, 0.18f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f));
            var brass = PatchworkAssets.Plain(new Color(0.95f, 0.75f, 0.35f), 0.85f);
            // The ring sits around the globe's outline as seen from the helm, so it never hides a pin.
            for (int i = 0; i <= 20; i++)
            {
                float a = Mathf.Lerp(-15f, 195f, i / 20f) * Mathf.Deg2Rad;
                Vector3 p = globeCenter + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * (radius + 0.25f);
                kit.Deco(PrimitiveType.Sphere, p, Vector3.one * 0.22f, brass);
            }

            var root = kit.Group("Globe", globeCenter);
            var spin = new GameObject("Spin").transform;
            spin.SetParent(root, false);
            var sphere = kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * radius * 2f, PatchworkAssets.GlobeMap(), default(Vector3), spin);
            sphere.gameObject.AddComponent<SphereCollider>();
            var globe = root.gameObject.AddComponent<LevelGlobe>();
            globe.Spin = spin;
            globe.Radius = radius;

            // A sewing pin for every level (gold for ones you can play, grey for ones being made).
            foreach (var e in LevelCatalog.Levels)
            {
                Vector3 dir = LevelGlobe.Direction(e.Longitude, e.Latitude);
                var pin = new GameObject("Pin_" + e.Number).transform;
                pin.SetParent(spin, false);
                pin.localPosition = dir * radius;
                pin.localRotation = Quaternion.FromToRotation(Vector3.up, dir);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.35f, 0f), new Vector3(0.06f, 0.35f, 0.06f), PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.9f), 0.9f), default(Vector3), pin);
                Color head = e.Playable ? (GameState.IsLevelComplete(e.Scene) ? LevelKit.Palette.Gold : LevelKit.Palette.AnchorPink) : new Color(0.6f, 0.6f, 0.65f);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.75f, 0f), Vector3.one * 0.32f,
                    e.Playable ? PatchworkAssets.Glow(head, 0.4f) : PatchworkAssets.Plain(head, 0.5f), default(Vector3), pin);
                // A little felt patch where the pin goes in.
                kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.6f, 0.02f, 0.6f), PatchworkAssets.Felt(e.Tint), default(Vector3), pin);
                globe.AddPin(pin);
            }

            // The helm: a giant sewing button for a wheel, with pencil handles, on a cardboard post.
            Vector3 helm = new Vector3(0f, 0f, 13.2f);
            kit.Cardboard(helm + new Vector3(0f, 0.7f, 0.3f), new Vector3(0.9f, 1.4f, 0.9f), KraftLight, "HelmPost");
            var wheel = kit.Group("HelmWheel", helm + new Vector3(0f, 1.75f, 0f));
            wheel.rotation = Quaternion.Euler(-20f, 0f, 0f);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.6f, 0.08f, 1.6f), PatchworkAssets.Felt(LevelKit.Palette.Rose), new Vector3(90f, 0f, 0f), wheel);
            var hole = PatchworkAssets.Plain(new Color(0.45f, 0.25f, 0.3f), 0.3f);
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * 0.22f, y = (i < 2 ? -1f : 1f) * 0.22f;
                kit.Deco(PrimitiveType.Cylinder, new Vector3(x, y, -0.05f), new Vector3(0.18f, 0.03f, 0.18f), hole, new Vector3(90f, 0f, 0f), wheel);
            }
            var pencilMat = PatchworkAssets.Plain(new Color(0.98f, 0.8f, 0.3f), 0.4f);
            for (int i = 0; i < 6; i++)
            {
                var spoke = kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.12f, 0.6f, 0.12f), pencilMat, new Vector3(0f, 0f, i * 60f), wheel);
                spoke.localPosition = Quaternion.Euler(0f, 0f, i * 60f) * new Vector3(0f, 1.05f, 0f);
            }

            var station = kit.Volume<HelmStation>(new Vector3(0f, 1.2f, 11.6f), new Vector3(4f, 3f, 3.2f), "Helm");
            station.Globe = globe;
            station.PromptPoint = helm + new Vector3(0f, 3.1f, 0f);
            kit.Info.Globe = globe;

            kit.Hint(new Vector3(0f, 1.5f, 9f), new Vector3(8f, 4f, 2.4f), "The Helm",
                "Stand behind the wheel and press {Ability} to turn the globe and pick where to sail.");
            // A warm light on the globe.
            var lightGo = new GameObject("GlobeLight");
            lightGo.transform.SetParent(kit.Root, false);
            lightGo.transform.position = globeCenter + new Vector3(0f, 3f, -4f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.85f, 0.65f);
            light.range = 10f;
            light.intensity = 1.2f;
            light.shadows = LightShadows.None;
        }

        // ---------------------------------------------------------------- Practice spots

        // The mast (a knitting needle) with a crow's nest you reach by riding a fountain as Water.
        static void MastAndFountain(LevelKit kit)
        {
            Vector3 mast = new Vector3(0f, 0f, 6f);
            var needle = kit.Deco(PrimitiveType.Cylinder, mast + Vector3.up * 8f, new Vector3(0.45f, 8f, 0.45f), PatchworkAssets.Plain(new Color(0.75f, 0.85f, 0.95f), 0.7f));
            var col = needle.gameObject.AddComponent<CapsuleCollider>();
            col.radius = 0.5f;
            col.height = 2f;
            kit.Deco(PrimitiveType.Sphere, mast + Vector3.up * 16.1f, Vector3.one * 0.8f, PatchworkAssets.Felt(LevelKit.Palette.Rose));

            // Crow's nest: a cardboard tub around the mast, top at 5 m.
            kit.Cardboard(mast + new Vector3(0f, 4.75f, 0f), new Vector3(3.4f, 0.5f, 3.4f), KraftLight, "CrowsNest");
            kit.Cardboard(mast + new Vector3(0f, 5.4f, 1.6f), new Vector3(3.4f, 0.8f, 0.2f), Kraft, "NestWall");
            kit.Cardboard(mast + new Vector3(1.6f, 5.4f, 0f), new Vector3(0.2f, 0.8f, 3.4f), Kraft, "NestWall");
            kit.Cardboard(mast + new Vector3(-1.6f, 5.4f, 0f), new Vector3(0.2f, 0.8f, 3.4f), Kraft, "NestWall");
            kit.Thread(mast + new Vector3(0.9f, 5.6f, 0.9f), LevelKit.Palette.Sky);
            kit.Thread(mast + new Vector3(-0.9f, 5.6f, 0.9f), LevelKit.Palette.Sky);
            kit.YarnBird(mast + new Vector3(1.2f, 5.8f, 1.6f), LevelKit.Palette.Butter, false);

            kit.Fountain(new Vector3(0f, 0f, 2.4f), 7f);
            kit.Hint(new Vector3(2.8f, 1.5f, 2.4f), new Vector3(3f, 3f, 3.5f), "Practice: Water",
                "Water Form rides blue fountains up to the crow's nest, and slips through blue drain grates.", HintTheme.Water);
        }

        // A little cardboard lifeboat hanging off the west side. Swing over to it as Yarn.
        static void Lifeboat(LevelKit kit)
        {
            Vector3 c = new Vector3(-17f, 0f, 2f);
            kit.Cardboard(c + new Vector3(0f, -0.5f, 0f), new Vector3(6f, 1f, 6f), KraftLight, "Lifeboat");
            kit.Cardboard(c + new Vector3(0f, -1.3f, 0f), new Vector3(5f, 0.8f, 5.4f), KraftDark, "LifeboatHull");
            kit.Cardboard(c + new Vector3(-2.85f, 0.35f, 0f), new Vector3(0.3f, 0.7f, 6f), Kraft, "LifeboatSide");
            kit.Cardboard(c + new Vector3(0f, 0.35f, 2.85f), new Vector3(6f, 0.7f, 0.3f), Kraft, "LifeboatSide");
            kit.Cardboard(c + new Vector3(0f, 0.35f, -2.85f), new Vector3(6f, 0.7f, 0.3f), Kraft, "LifeboatSide");
            kit.Thread(c + new Vector3(0f, 1f, 0f), LevelKit.Palette.Rose);
            kit.Thread(c + new Vector3(-1.5f, 1f, 1.5f), LevelKit.Palette.Rose);

            // Davits: pencils from the ship's side, with yarn ropes holding the boat.
            var pencil = PatchworkAssets.Plain(new Color(0.98f, 0.8f, 0.3f), 0.4f);
            var rope = PatchworkAssets.Yarn(new Color(0.96f, 0.92f, 0.82f));
            for (int s = -1; s <= 1; s += 2)
            {
                float z = c.z + s * 2.4f;
                Vector3 baseP = new Vector3(-DeckHalfWidth + 0.3f, 0f, z);
                Vector3 bend = new Vector3(-DeckHalfWidth - 1f, 8.5f, z);
                Vector3 end = new Vector3(c.x, 8.5f, z);
                kit.Rope(baseP, bend, pencil, 0.3f);
                kit.Rope(bend, end, pencil, 0.3f);
                kit.Rope(end, new Vector3(c.x - 2.6f, 0.6f, z), rope, 0.07f);
                kit.Rope(end, new Vector3(c.x + 2.6f, 0.6f, z), rope, 0.07f);
            }

            kit.Anchor(new Vector3(-11.5f, 6.5f, 2f), 13f);
            kit.AddCameraZone(new Vector3(-12.5f, 2f, 2f), new Vector3(13f, 20f, 10f), 2f, 5f);
        }

        // The captain's cabin at the stern doubles as the Button Box: sewn shut until all of
        // Level 1's golden buttons are found; inside is the Golden Crown. Its roof is the Wind landing.
        static void Cabin(LevelKit kit)
        {
            int found = GameState.GoldenButtonCount();
            int needed = Level1Builder.CollectibleIds.Length;
            bool open = found >= needed;
            const float x0 = -4f, x1 = 4f, z0 = -15.6f, z1 = -7.6f, h = 4f;
            float cx = (x0 + x1) * 0.5f, cz = (z0 + z1) * 0.5f;

            kit.Cardboard(new Vector3(x0 + 0.2f, h * 0.5f, cz), new Vector3(0.4f, h, z1 - z0), Kraft, "CabinWall");
            kit.Cardboard(new Vector3(x1 - 0.2f, h * 0.5f, cz), new Vector3(0.4f, h, z1 - z0), Kraft, "CabinWall");
            kit.Cardboard(new Vector3(cx, h * 0.5f, z0 + 0.2f), new Vector3(x1 - x0, h, 0.4f), Kraft, "CabinWall");
            kit.Cardboard(new Vector3(cx - 2.6f, h * 0.5f, z1 - 0.2f), new Vector3(2.8f, h, 0.4f), Kraft, "CabinFront");
            kit.Cardboard(new Vector3(cx + 2.6f, h * 0.5f, z1 - 0.2f), new Vector3(2.8f, h, 0.4f), Kraft, "CabinFront");
            kit.Cardboard(new Vector3(cx, 3.5f, z1 - 0.2f), new Vector3(2.4f, 1f, 0.4f), Kraft, "CabinLintel");
            kit.Cardboard(new Vector3(cx, h + 0.2f, cz), new Vector3(x1 - x0 + 0.4f, 0.4f, z1 - z0 + 0.4f), KraftLight, "CabinRoof");
            kit.Tape(new Vector3(cx, h + 0.41f, cz), new Vector3(x1 - x0 + 0.42f, 0.02f, 0.6f));
            kit.Quilt(new Vector3(cx, 0.06f, cz), new Vector3(x1 - x0 - 0.8f, 0.12f, z1 - z0 - 0.8f), new[] { LevelKit.Palette.Butter, LevelKit.Palette.Peach, LevelKit.Palette.Cream }, 2.4f, "CabinRug");
            // Painted sign over the door.
            kit.Cardboard(new Vector3(cx, h - 0.15f, z1 + 0.05f), new Vector3(3.2f, 0.7f, 0.1f), LevelKit.Palette.Cream, "CabinSign");

            // Three button sockets beside the door, gold for each one found.
            var gold = PatchworkAssets.Glow(LevelKit.Palette.Gold, 0.5f);
            var empty = PatchworkAssets.Felt(new Color(0.35f, 0.3f, 0.4f));
            for (int i = 0; i < needed; i++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(cx - 0.8f + i * 0.8f, h - 0.15f, z1 + 0.15f), new Vector3(0.55f, 0.05f, 0.55f), i < found ? gold : empty, new Vector3(90f, 0f, 0f));

            if (!open)
            {
                kit.Block(new Vector3(cx, 1.5f, z1 - 0.2f), new Vector3(2.4f, 3f, 0.3f), LevelKit.Palette.Rose, "CabinDoor");
                var thread = PatchworkAssets.Yarn(new Color(0.95f, 0.9f, 0.8f));
                kit.Deco(PrimitiveType.Cube, new Vector3(cx, 1.5f, z1 - 0.02f), new Vector3(0.12f, 3.4f, 0.05f), thread, new Vector3(0f, 0f, 35f));
                kit.Deco(PrimitiveType.Cube, new Vector3(cx, 1.5f, z1 - 0.02f), new Vector3(0.12f, 3.4f, 0.05f), thread, new Vector3(0f, 0f, -35f));
            }
            kit.Hint(new Vector3(cx, 1.5f, z1 + 1.6f), new Vector3(4f, 3f, 2.4f), "The Button Box",
                open ? "Unstitched! All the golden buttons are home. Step inside."
                     : "The captain's cabin is stitched shut. It opens when you've found all " + needed + " golden buttons in Level 1. Found so far: " + found + ".");

            // Inside: the Golden Crown on a pedestal.
            Vector3 ped = new Vector3(cx, 0f, cz - 0.6f);
            kit.Deco(PrimitiveType.Cylinder, ped + Vector3.up * 0.4f, new Vector3(1.4f, 0.4f, 1.4f), PatchworkAssets.Felt(LevelKit.Palette.Cream));
            kit.Deco(PrimitiveType.Cylinder, ped + Vector3.up * 0.82f, new Vector3(1.6f, 0.04f, 1.6f), gold);
            var pedestal = kit.Volume<HatPedestal>(ped + Vector3.up * 1.4f, new Vector3(2f, 2.8f, 2f), "CrownPedestal");
            var vis = new GameObject("Crown").transform;
            vis.SetParent(pedestal.transform, false);
            var crownGold = PatchworkAssets.Glow(new Color(1f, 0.8f, 0.25f), 0.35f);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.9f, 0.16f, 0.9f), crownGold, default(Vector3), vis);
            for (int i = 0; i < 5; i++)
            {
                float a = i * Mathf.PI * 2f / 5f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Sin(a) * 0.4f, 0.25f, Mathf.Cos(a) * 0.4f), new Vector3(0.18f, 0.36f, 0.18f), crownGold, default(Vector3), vis);
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Sin(a) * 0.4f, 0.46f, Mathf.Cos(a) * 0.4f), Vector3.one * 0.12f, crownGold, default(Vector3), vis);
            }
            pedestal.Setup(4, FormVisuals.HatNames[4], vis);
            kit.Lamp(new Vector3(cx, 3.2f, cz), h);

            // Flagpole on the roof with a felt pennant.
            Vector3 pole = new Vector3(x1 - 0.8f, h + 0.4f, z0 + 0.8f);
            kit.Deco(PrimitiveType.Cylinder, pole + Vector3.up * 2f, new Vector3(0.12f, 2f, 0.12f), PatchworkAssets.Plain(new Color(0.98f, 0.8f, 0.3f), 0.4f));
            var flag = kit.Group("Pennant", pole + new Vector3(0f, 3.5f, 0f));
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, -0.9f), new Vector3(0.05f, 0.9f, 1.8f), PatchworkAssets.Felt(LevelKit.Palette.AnchorPink), default(Vector3), flag);
            var wave = flag.gameObject.AddComponent<Spinner>();
            wave.Speed = 0f;
            wave.BobHeight = 0.08f;
            wave.BobSpeed = 3f;
            kit.Thread(new Vector3(-2f, h + 1f, cz), LevelKit.Palette.Cream);
            kit.Thread(new Vector3(0f, h + 1f, cz), LevelKit.Palette.Cream);
            kit.Thread(new Vector3(2f, h + 1f, cz), LevelKit.Palette.Cream);
        }

        // A shipping crate with a drain grate for a door: only Water gets in.
        static void DrainCrate(LevelKit kit)
        {
            Vector3 c = new Vector3(-6f, 0f, -12f);
            kit.Cardboard(c + new Vector3(0f, 1.25f, -1.4f), new Vector3(3.2f, 2.5f, 0.3f), KraftLight, "CrateBack");
            kit.Cardboard(c + new Vector3(-1.45f, 1.25f, 0f), new Vector3(0.3f, 2.5f, 3f), KraftLight, "CrateSide");
            kit.Cardboard(c + new Vector3(1.45f, 1.25f, 0f), new Vector3(0.3f, 2.5f, 3f), KraftLight, "CrateSide");
            kit.Cardboard(c + new Vector3(0f, 2.65f, 0f), new Vector3(3.4f, 0.3f, 3.2f), Kraft, "CrateLid");
            kit.Tape(new Vector3(c.x, 2.81f, c.z), new Vector3(0.5f, 0.02f, 3.22f));
            kit.Cardboard(c + new Vector3(-0.95f, 1.25f, 1.4f), new Vector3(1.0f, 2.5f, 0.3f), KraftLight, "CrateFront");
            kit.Cardboard(c + new Vector3(0.95f, 1.25f, 1.4f), new Vector3(1.0f, 2.5f, 0.3f), KraftLight, "CrateFront");
            kit.Cardboard(c + new Vector3(0f, 1.75f, 1.4f), new Vector3(0.9f, 1.5f, 0.3f), KraftLight, "CrateFront");
            kit.Grate(c + new Vector3(0f, 0.5f, 1.4f), new Vector3(0.9f, 1f, 0.2f));
            kit.SewingButton(c + new Vector3(0f, 0.05f, -0.2f), 1.1f, LevelKit.Palette.Gold, Vector3.zero);
            kit.Thread(c + new Vector3(0f, 0.45f, 0f), LevelKit.Palette.Sky);
        }

        // An air vent made from a cardboard tube: Wind floats up it to the cabin roof.
        static void AirVent(LevelKit kit)
        {
            Vector3 c = new Vector3(6f, 0f, -11.5f);
            kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.3f, new Vector3(2.4f, 0.3f, 2.4f), PatchworkAssets.Cardboard(KraftLight));
            kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.62f, new Vector3(1.9f, 0.02f, 1.9f), PatchworkAssets.Plain(new Color(0.3f, 0.26f, 0.3f), 0.2f));
            kit.Updraft(c, 8f);
            kit.Hint(new Vector3(6f, 1.5f, -8.2f), new Vector3(3.4f, 3f, 3f), "Practice: Wind",
                "Wind Form floats up white updrafts. Ride this one to the cabin roof. Hold {Jump} to glide, tap {Ability} for a gust.", HintTheme.Wind);
        }
    }
}

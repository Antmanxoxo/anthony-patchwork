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
            SewingBoxSky(kit);
            Comforts(kit);
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

        // ---------------------------------------------------------------- The sewing-box sky

        // The worlds near the Pod all come out of one sewing box: a button moon, a cotton reel
        // trailing its thread, a ball-of-yarn planet with knitting needles through it, a
        // pincushion planet bristling with pins, and a silver thimble with lit windows. A line of
        // glowing stitches runs from the patchwork planet out to each of them, because in this
        // game it's thread that joins one world to the next.
        static void SewingBoxSky(LevelKit kit)
        {
            var steel = PatchworkAssets.Plain(new Color(0.82f, 0.84f, 0.9f), 0.85f);
            var cream = PatchworkAssets.Yarn(new Color(0.98f, 0.94f, 0.86f));

            // The button moon.
            Vector3 buttonAt = new Vector3(8.4f, 8f, 21f);
            var button = kit.Group("ButtonMoon", buttonAt);
            button.rotation = Quaternion.Euler(-62f, 0f, 20f);
            var bCol = new Color(1f, 0.62f, 0.72f);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(3.2f, 0.22f, 3.2f), PatchworkAssets.Plain(bCol, 0.75f), default(Vector3), button);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.06f, 0f), new Vector3(2.3f, 0.22f, 2.3f), PatchworkAssets.Plain(Color.Lerp(bCol, Color.white, 0.25f), 0.75f), default(Vector3), button);
            for (int i = 0; i < 4; i++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(i % 2 == 0 ? -0.42f : 0.42f, 0.08f, i < 2 ? -0.42f : 0.42f), new Vector3(0.4f, 0.24f, 0.4f), PatchworkAssets.Plain(new Color(0.35f, 0.2f, 0.3f), 0.3f), default(Vector3), button);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.3f, 0f), new Vector3(1.3f, 0.1f, 0.16f), cream, new Vector3(0f, 45f, 0f), button);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.31f, 0f), new Vector3(1.3f, 0.1f, 0.16f), cream, new Vector3(0f, -45f, 0f), button);
            Drift(button, 9f, 0.3f);

            // The cotton reel, its loose end trailing behind it.
            Vector3 reelAt = new Vector3(-8.6f, 5.6f, 20f);
            var reel = kit.Group("CottonReel", reelAt);
            reel.rotation = Quaternion.Euler(0f, 0f, 24f);
            var wood = PatchworkAssets.Wood(new Color(0.92f, 0.78f, 0.56f));
            var thread = PatchworkAssets.Yarn(new Color(0.35f, 0.75f, 0.9f));
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.5f, 0.9f, 1.5f), thread, default(Vector3), reel);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1f, 0f), new Vector3(2f, 0.12f, 2f), wood, default(Vector3), reel);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -1f, 0f), new Vector3(2f, 0.12f, 2f), wood, default(Vector3), reel);
            for (int i = 0; i < 9; i++)
            {
                float k = i / 8f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(-0.9f - k * 4.2f, 0.5f - k * 0.4f + Mathf.Sin(k * 7f) * 0.5f, Mathf.Cos(k * 5f) * 0.3f), new Vector3(0.7f, 0.12f, 0.12f), thread, new Vector3(0f, 0f, Mathf.Cos(k * 7f) * 35f), reel);
            }
            Drift(reel, 7f, 0.3f);

            // The ball-of-yarn planet, two knitting needles stuck through it.
            Vector3 yarnAt = new Vector3(17f, 5.5f, 30f);
            var yarn = kit.Group("YarnPlanet", yarnAt);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 7f, PatchworkAssets.Yarn(new Color(0.98f, 0.72f, 0.3f)), new Vector3(20f, 0f, 35f), yarn);
            for (int i = 0; i < 3; i++)
                kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(7.12f, 0.16f, 7.12f), PatchworkAssets.Yarn(new Color(1f, 0.82f, 0.4f)), new Vector3(30f + i * 55f, i * 40f, 15f), yarn);
            for (int s = -1; s <= 1; s += 2)
            {
                var needle = kit.Group("Needle", Vector3.zero);
                needle.SetParent(yarn, false);
                needle.localRotation = Quaternion.Euler(0f, s * 20f, s * 32f);
                kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.22f, 5.6f, 0.22f), steel, default(Vector3), needle);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 5.7f, 0f), Vector3.one * 0.7f, PatchworkAssets.Plain(new Color(0.9f, 0.3f, 0.4f), 0.8f), default(Vector3), needle);
            }
            Drift(yarn, 3f, 0.25f);

            // The pincushion planet: a plump tomato-red cushion bristling with glass-headed pins.
            Vector3 pinAt = new Vector3(-19f, 1.5f, 35f);
            var cushion = kit.Group("PincushionPlanet", pinAt);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(7f, 5.4f, 7f), PatchworkAssets.Felt(new Color(0.9f, 0.3f, 0.3f)), default(Vector3), cushion);
            for (int i = 0; i < 6; i++)
                kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(7.08f, 5.46f, 0.14f), PatchworkAssets.Yarn(new Color(0.4f, 0.65f, 0.35f)), new Vector3(0f, i * 30f, 0f), cushion);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 2.65f, 0f), new Vector3(1.6f, 0.4f, 1.6f), PatchworkAssets.Felt(new Color(0.4f, 0.65f, 0.35f)), default(Vector3), cushion);
            Color[] heads = { new Color(1f, 0.9f, 0.4f), new Color(0.5f, 0.8f, 1f), new Color(0.99f, 0.96f, 0.9f), new Color(0.7f, 0.5f, 0.95f), new Color(0.6f, 0.9f, 0.6f) };
            for (int i = 0; i < 9; i++)
            {
                Vector3 n = (Quaternion.Euler(-25f - (i % 3) * 22f, i * 47f, 0f) * Vector3.forward).normalized;
                Vector3 on = new Vector3(n.x * 3.5f, n.y * 2.7f, n.z * 3.5f);
                var pin = kit.Deco(PrimitiveType.Cylinder, on + n * 1f, new Vector3(0.09f, 1.1f, 0.09f), steel, default(Vector3), cushion);
                pin.localRotation = Quaternion.FromToRotation(Vector3.up, n);
                kit.Deco(PrimitiveType.Sphere, on + n * 2.2f, Vector3.one * 0.5f, PatchworkAssets.Glow(heads[i % heads.Length], 0.9f), default(Vector3), cushion);
            }
            Drift(cushion, -2.5f, 0.25f);

            // The thimble: a little silver space station with lit windows, dimpled on top.
            Vector3 thimbleAt = new Vector3(7f, -9f, 36f);
            var thimble = kit.Group("ThimbleStation", thimbleAt);
            thimble.rotation = Quaternion.Euler(14f, 0f, -18f);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(6f, 3.2f, 6f), steel, default(Vector3), thimble);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 3.2f, 0f), new Vector3(6f, 3f, 6f), steel, default(Vector3), thimble);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -3.1f, 0f), new Vector3(6.7f, 0.25f, 6.7f), PatchworkAssets.Plain(new Color(0.95f, 0.78f, 0.4f), 0.85f), default(Vector3), thimble);
            var window = PatchworkAssets.Glow(new Color(1f, 0.85f, 0.5f), 2f);
            for (int i = 0; i < 10; i++)
            {
                float a = i * Mathf.PI * 2f / 10f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 2.95f, -1f + (i % 2) * 1.8f, Mathf.Sin(a) * 2.95f), new Vector3(0.7f, 0.9f, 0.3f), window, new Vector3(0f, 90f - a * Mathf.Rad2Deg, 0f), thimble);
            }
            for (int i = 0; i < 12; i++)
            {
                Vector3 n = (Quaternion.Euler(-50f - (i % 2) * 20f, i * 30f, 0f) * Vector3.forward).normalized;
                kit.Deco(PrimitiveType.Sphere, new Vector3(n.x * 2.9f, 3.2f + n.y * 1.45f, n.z * 2.9f), Vector3.one * 0.5f, PatchworkAssets.Plain(new Color(0.62f, 0.64f, 0.72f), 0.8f), default(Vector3), thimble);
            }
            Drift(thimble, 2f, 0.2f);

            // The stitches joining the patchwork planet to each of them.
            var path = kit.Group("ThreadPaths", Vector3.zero);
            var glow = PatchworkAssets.Glow(new Color(1f, 0.95f, 0.8f), 1.7f);
            Vector3 hub = HubBuilder.GlobePoint;
            foreach (var target in new[] { buttonAt, reelAt, yarnAt, pinAt, thimbleAt })
            {
                Vector3 d = target - hub;
                float len = d.magnitude;
                Vector3 dir = d / len;
                // Start outside the patchwork planet and stop short of the other one; sag a
                // little in the middle like a slack thread.
                for (float at = 5.2f; at < len - 4.2f; at += 1.1f)
                {
                    float k = at / len;
                    Vector3 p = hub + dir * at + Vector3.down * Mathf.Sin(k * Mathf.PI) * 1.2f;
                    var dash = kit.Deco(PrimitiveType.Cube, p, new Vector3(0.09f, 0.09f, 0.55f), glow, default(Vector3), path);
                    dash.rotation = Quaternion.LookRotation(dir, Vector3.up);
                    dash.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
        }

        static void Drift(Transform t, float speed, float bob)
        {
            var spin = t.gameObject.AddComponent<Spinner>();
            spin.Speed = speed;
            spin.BobHeight = bob;
            spin.BobSpeed = 0.4f;
        }

        // ---------------------------------------------------------------- Comforts

        // Calm things to enjoy inside: a wind chime to jump up and ring, a lava lamp glowing on
        // the window ledge, and a mug of cocoa steaming beside it.
        static void Comforts(LevelKit kit)
        {
            float desk = HubBuilder.DeskHeight;

            // The wind chime, hanging on a long thread from the roof.
            Vector3 hang = new Vector3(-2.9f, 0f, 2.4f);
            var chime = kit.Group("WindChime", hang + Vector3.up * 3.3f);
            var thread = PatchworkAssets.Yarn(new Color(0.97f, 0.94f, 0.88f));
            var cord = kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 4.2f, 0f), new Vector3(0.03f, 4.2f, 0.03f), thread, default(Vector3), chime);
            cord.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.1f, 0.05f, 1.1f), PatchworkAssets.Wood(new Color(0.9f, 0.76f, 0.56f)), default(Vector3), chime);
            Color[] tubeCols = { new Color(0.98f, 0.8f, 0.4f), new Color(0.6f, 0.85f, 1f), new Color(1f, 0.65f, 0.78f), new Color(0.7f, 0.95f, 0.75f), new Color(0.8f, 0.7f, 1f) };
            var tubes = new Transform[5];
            for (int i = 0; i < 5; i++)
            {
                float a = i * Mathf.PI * 2f / 5f, len = 0.55f + i * 0.11f;
                var pivot = kit.Group("Tube", Vector3.zero);
                pivot.SetParent(chime, false);
                pivot.localPosition = new Vector3(Mathf.Cos(a) * 0.4f, 0f, Mathf.Sin(a) * 0.4f);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.14f, 0f), new Vector3(0.015f, 0.14f, 0.015f), thread, default(Vector3), pivot);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.28f - len * 0.5f, 0f), new Vector3(0.1f, len * 0.5f, 0.1f), PatchworkAssets.Plain(tubeCols[i], 0.9f), default(Vector3), pivot);
                tubes[i] = pivot;
            }
            // A felt star as the clapper in the middle.
            var star = kit.Group("Clapper", Vector3.zero);
            star.SetParent(chime, false);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.45f, 0f), new Vector3(0.012f, 0.45f, 0.012f), thread, default(Vector3), star);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.95f, 0f), new Vector3(0.34f, 0.34f, 0.12f), PatchworkAssets.Glow(new Color(1f, 0.9f, 0.5f), 1.2f), default(Vector3), star);
            var wc = chime.gameObject.AddComponent<WindChime>();
            wc.Tubes = tubes;
            wc.Clapper = star;

            // The lava lamp on the window ledge.
            Vector3 lampAt = new Vector3(Mathf.Sin(-27f * Mathf.Deg2Rad), 0f, Mathf.Cos(-27f * Mathf.Deg2Rad)) * 8.35f + Vector3.up * desk;
            var lamp = kit.Group("LavaLamp", lampAt);
            var brass = PatchworkAssets.Plain(new Color(0.95f, 0.78f, 0.4f), 0.85f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.16f, 0f), new Vector3(0.5f, 0.16f, 0.5f), brass, default(Vector3), lamp);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.42f, 0f), new Vector3(0.3f, 0.08f, 0.3f), brass, default(Vector3), lamp);
            var glass = kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.86f, 0f), new Vector3(0.52f, 1.2f, 0.52f), PatchworkAssets.Bubble(new Color(0.6f, 0.5f, 1f, 0.22f)), default(Vector3), lamp);
            glass.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var blobs = new Transform[4];
            var blobMat = PatchworkAssets.Glow(new Color(1f, 0.5f, 0.7f), 1.8f);
            for (int i = 0; i < blobs.Length; i++)
            {
                blobs[i] = kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.6f, 0f), Vector3.one * 0.18f, blobMat, default(Vector3), lamp);
                blobs[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            var lightGo = new GameObject("LavaGlow");
            lightGo.transform.SetParent(lamp, false);
            lightGo.transform.localPosition = new Vector3(0f, 0.9f, -0.2f);
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.55f, 0.8f);
            l.intensity = 1.6f;
            l.range = 4.5f;
            l.shadows = LightShadows.None;
            var lava = lamp.gameObject.AddComponent<LavaLamp>();
            lava.Blobs = blobs;
            lava.Glow = l;

            // A mug of cocoa, steaming.
            Vector3 mugAt = new Vector3(Mathf.Sin(25f * Mathf.Deg2Rad), 0f, Mathf.Cos(25f * Mathf.Deg2Rad)) * 8.2f + Vector3.up * desk;
            var mug = kit.Group("CocoaMug", mugAt);
            var china = PatchworkAssets.Plain(new Color(0.55f, 0.8f, 0.95f), 0.7f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.2f, 0f), new Vector3(0.42f, 0.2f, 0.42f), china, default(Vector3), mug);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.395f, 0f), new Vector3(0.36f, 0.01f, 0.36f), PatchworkAssets.Felt(new Color(0.5f, 0.32f, 0.22f)), default(Vector3), mug);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.06f, 0.41f, -0.04f), new Vector3(0.12f, 0.06f, 0.12f), PatchworkAssets.Felt(new Color(0.99f, 0.97f, 0.94f)), default(Vector3), mug);
            kit.Deco(PrimitiveType.Sphere, new Vector3(-0.07f, 0.41f, 0.05f), new Vector3(0.1f, 0.05f, 0.1f), PatchworkAssets.Felt(new Color(1f, 0.85f, 0.88f)), default(Vector3), mug);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.27f, 0.2f, 0f), new Vector3(0.2f, 0.26f, 0.07f), china, default(Vector3), mug);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.01f, 0f), new Vector3(0.7f, 0.01f, 0.7f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, new Color(0.99f, 0.96f, 0.9f), new Color(0.9f, 0.4f, 0.45f)), default(Vector3), mug);
            var steamRoot = kit.Group("CocoaSteam", mugAt + Vector3.up * 0.45f);
            steamRoot.localScale = Vector3.one * 0.16f;
            ChimneySmoke.Attach(kit, steamRoot);
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

    // A wind chime: five tubes and a felt-star clapper on threads. They stir a little all the
    // time, and when Patch jumps up into them they swing wide and ring, one after another, up a
    // gentle five-note scale.
    public class WindChime : MonoBehaviour
    {
        public Transform[] Tubes;
        public Transform Clapper;
        float[] swing, vel, ringAt;
        float t, cooldown;
        static readonly float[] Notes = { 1f, 1.122f, 1.26f, 1.498f, 1.682f };

        void Start()
        {
            swing = new float[Tubes.Length];
            vel = new float[Tubes.Length];
            ringAt = new float[Tubes.Length];
            for (int i = 0; i < ringAt.Length; i++) ringAt[i] = -1f;
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || game.Player == null || swing == null) return;
            float dt = Mathf.Min(Time.deltaTime, 0.033f);
            t += dt;
            cooldown -= dt;
            Vector3 d = game.Player.Motor.Center - (transform.position + Vector3.down * 0.7f);
            bool touching = d.x * d.x + d.z * d.z < 0.75f * 0.75f && d.y > -1.3f && d.y < 0.6f;
            if (touching && cooldown <= 0f && game.Mode == UIMode.Gameplay)
            {
                cooldown = 1.6f;
                // Each tube is knocked in turn, the nearest first.
                for (int i = 0; i < Tubes.Length; i++)
                {
                    ringAt[i] = t + i * 0.13f;
                    vel[i] += 260f + i * 20f;
                }
            }
            for (int i = 0; i < Tubes.Length; i++)
            {
                if (ringAt[i] >= 0f && t >= ringAt[i])
                {
                    ringAt[i] = -1f;
                    Sfx.PlayPitched(SfxId.Chime, 0.3f, Notes[i % Notes.Length]);
                }
                // A soft spring, plus a breath of air so they're never quite still.
                vel[i] += (-swing[i] * 30f - vel[i] * 1.6f) * dt;
                swing[i] += vel[i] * dt;
                float idle = Mathf.Sin(t * 0.9f + i * 1.3f) * 2.5f;
                float a = i * Mathf.PI * 2f / Tubes.Length;
                // Swinging outwards from the middle.
                Tubes[i].localRotation = Quaternion.AngleAxis(swing[i] + idle, new Vector3(-Mathf.Sin(a), 0f, Mathf.Cos(a)));
            }
            if (Clapper != null) Clapper.localRotation = Quaternion.Euler(Mathf.Sin(t * 0.8f) * 4f + swing[0] * 0.4f, t * 14f, Mathf.Cos(t * 0.7f) * 4f);
        }
    }

    // A lava lamp: glowing blobs that rise slowly, hang at the top, sink again, swelling and
    // shrinking as they go, while the lamp's own light breathes with them.
    public class LavaLamp : MonoBehaviour
    {
        public Transform[] Blobs;
        public Light Glow;

        void Update()
        {
            if (Blobs == null) return;
            float t = Time.time;
            for (int i = 0; i < Blobs.Length; i++)
            {
                float p = t * (0.11f + i * 0.023f) + i * 1.7f;
                float k = Mathf.Sin(p) * 0.5f + 0.5f;                       // 0 at the bottom, 1 at the top
                k = k * k * (3f - 2f * k);                                    // lingers at both ends
                float squash = 1f + Mathf.Cos(p) * 0.25f;                     // stretches while it's moving
                Blobs[i].localPosition = new Vector3(Mathf.Sin(p * 0.7f + i) * 0.06f, 0.42f + k * 0.86f, Mathf.Cos(p * 0.5f + i * 2f) * 0.06f);
                float s = 0.15f + 0.07f * Mathf.Sin(p * 1.3f + i);
                Blobs[i].localScale = new Vector3(s / Mathf.Sqrt(squash), s * squash, s / Mathf.Sqrt(squash));
            }
            if (Glow != null) Glow.intensity = 1.5f + Mathf.Sin(t * 0.5f) * 0.3f;
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

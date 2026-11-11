using UnityEngine;

namespace Patchwork
{
    // Light and moments. A few big lights that throw real shadows and dancing water patterns
    // (the reef's light through the Great Window, moonlight through the Observatory's dome,
    // sun through the kelp pool and the cracks in the Ruins' roof), shafts of light you can
    // see, and the little camera moves the first time you walk into each hall.
    public static partial class Level2Builder
    {
        static readonly Color ReefLight = new Color(0.45f, 0.85f, 1f);
        static readonly Color MoonLight = new Color(0.7f, 0.8f, 1f);

        static void Lighting(LevelKit kit)
        {
            // ----- The Great Window: the reef's light pouring into the atrium -----
            // From high up at the back of the tank, so the kelp and the passing sharks throw
            // moving shadows across the atrium floor through the glass, netted with caustics.
            SpotLight(kit, new Vector3(AtriumHalf + 26f, 20f, -78f), new Vector3(-6f, 0f, -78f), ReefLight, 85f, 62f, 110f, true, true);
            // Visible beams slanting in through the glass.
            var beam = Shaft(new Color(0.55f, 0.85f, 1f), 0.045f);
            foreach (float z in new[] { -96f, -86f, -76f, -66f, -58f })
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(14f, 9f, z), new Vector3(36f, 0.04f, 4.5f), beam, new Vector3(0f, 0f, 24f + (z % 3f))));
            Motes(kit, new Vector3(14f, 7f, -78f), new Vector3(26f, 12f, 44f), 36, new Color(0.7f, 0.9f, 1f), 1.8f, 0.07f, new Vector3(0.03f, 0.05f, 0.04f));

            // ----- The Observatory: moonlight down through the dome -----
            Vector3 C = Observatory;
            SpotLight(kit, C + new Vector3(14f, 40f, -12f), C, MoonLight, 75f, 46f, 90f, true, false);

            // ----- The kelp pool: dancing light on the bed and the foyer -----
            SpotLight(kit, new Vector3(-62f, KelpTop - 1.5f, 34f), new Vector3(-60f, PoolBed, 40f), new Color(0.55f, 1f, 0.9f), 40f, 80f, 45f, true, true);
            SpotLight(kit, new Vector3(-56f, KelpTop - 1.5f, 6f), new Vector3(-56f, 0f, 8f), new Color(0.55f, 1f, 0.9f), 30f, 70f, 18f, false, true).GetComponent<CausticLight>().Phase = 0.4f;

            // ----- The Great Deep: light from the surface falling on the tunnel -----
            foreach (float x in new[] { -16f, 10f, 32f })
                SpotLight(kit, new Vector3(x, DeepTop - 2f, TunnelZ - 3f), new Vector3(x + 2f, 0f, TunnelZ), new Color(0.4f, 0.7f, 1f), 40f, 60f, 30f, false, true).GetComponent<CausticLight>().Phase = x * 0.1f;

            // ----- The Sunken Ruins: green light through the broken roof onto the water -----
            foreach (var p in new[] { new Vector3(75f, 0f, 58f), new Vector3(60f, 0f, 84f), new Vector3(90f, 0f, 42f) })
                SpotLight(kit, new Vector3(p.x, RuinTop - 1f, p.z), new Vector3(p.x + 1f, RuinBed, p.z + 1f), new Color(0.6f, 1f, 0.8f), 36f, 52f, 40f, false, true).GetComponent<CausticLight>().Phase = p.z * 0.03f;

            // ----- The Hall of Arms: the coloured glow of the windows on the floor -----
            float acx = (ArmsX0 + ArmsX1) * 0.5f;
            SpotLight(kit, new Vector3(ArmsX1 - 2f, 16f, -22f), new Vector3(acx - 6f, 0f, -22f), new Color(1f, 0.72f, 0.5f), 45f, 70f, 30f, false, false);
            SpotLight(kit, new Vector3(ArmsX0 + 2f, 16f, -31f), new Vector3(acx + 6f, 0f, -31f), new Color(0.65f, 0.6f, 1f), 45f, 70f, 28f, false, false);

            // ----- The quay: a cool moon wash over the front of the museum -----
            SpotLight(kit, new Vector3(-30f, 40f, -200f), new Vector3(0f, 10f, -112f), MoonLight, 140f, 40f, 350f, false, false);
        }

        // (Spot lights fall off with the square of the distance, so these, hung far from what
        // they light, need big numbers to land about as bright as a lamp a few metres away.)
        static Light SpotLight(LevelKit kit, Vector3 pos, Vector3 target, Color c, float range, float angle, float intensity, bool shadows, bool caustic)
        {
            var go = new GameObject(caustic ? "CausticLight" : "SpotLight");
            go.transform.SetParent(kit.Root, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.LookRotation((target - pos).normalized, Vector3.up);
            var l = go.AddComponent<Light>();
            l.type = LightType.Spot;
            l.color = c;
            l.range = range;
            l.spotAngle = angle;
            l.innerSpotAngle = angle * 0.55f;
            l.intensity = intensity;
            l.shadows = shadows ? LightShadows.Soft : LightShadows.None;
            l.shadowStrength = 0.8f;
            l.renderMode = LightRenderMode.ForcePixel;
            if (caustic) go.AddComponent<CausticLight>().Lamp = l;
            return l;
        }

        // ================================================================ Moments

        static CinematicShot Shot(LevelKit kit, string key, Vector3 at, Vector3 size, float duration, Vector3[] cams, Vector3[] looks, params string[] captions)
        {
            var s = kit.Volume<CinematicShot>(at, size, "Cinematic_" + key);
            s.Key = "L2_cine_" + key;
            s.Cams = cams;
            s.Looks = looks;
            s.Duration = duration;
            s.Captions = captions;
            return s;
        }

        static void Cinematics(LevelKit kit)
        {
            // Arriving: up from the Pod, over the moonlit quay, to the doors.
            Shot(kit, "arrive", SpawnPoint + Vector3.up * 1.5f, new Vector3(10f, 4f, 10f), 9f,
                new[] { SpawnPoint + new Vector3(7f, 2f, -8f), new Vector3(10f, 6f, -136f), new Vector3(0f, 9f, -124f), new Vector3(0f, 5f, -136f) },
                new[] { SpawnPoint + new Vector3(0f, 1.5f, 4f), new Vector3(0f, 10f, -112f), new Vector3(0f, 20f, -108f), SpawnPoint + new Vector3(0f, 1.6f, 0f) },
                "The Oceanic Museum.", "Long past closing time...", "...but somebody has left the doors open.");

            // The Grand Atrium: the reef through the Great Window, then up to the whale and the stars.
            Shot(kit, "atrium", new Vector3(0f, 1.5f, -105f), new Vector3(10f, 4f, 3f), 11f,
                new[] { new Vector3(0f, 2.4f, -106f), new Vector3(8f, 5f, -94f), new Vector3(14f, 6f, -80f), new Vector3(-6f, 16f, -84f), new Vector3(-4f, 6f, -100f) },
                new[] { new Vector3(0f, 6f, -88f), new Vector3(28f, 6f, -84f), new Vector3(32f, 9f, -72f), new Vector3(0f, 22f, -64f), new Vector3(0f, 2f, -96f) },
                "The Grand Atrium.", "A reef behind one sheet of glass...", "...the bones of the largest animal that ever lived...", "...and a sky full of stars, indoors.");

            // The Hall of Deep Time: the great lizard first, then along to the longship.
            Shot(kit, "history", new Vector3(-34f, 1.5f, -78f), new Vector3(3f, 4f, 8f), 9f,
                new[] { new Vector3(-35f, 2.4f, -78f), new Vector3(-46f, 9f, -86f), new Vector3(-76f, 10f, -58f), new Vector3(-40f, 3f, -76f) },
                new[] { new Vector3(-60f, 6f, -66f), new Vector3(-62f, 5f, -62f), new Vector3(-62f, 2f, -30f), new Vector3(-50f, 2f, -74f) },
                "The Hall of Deep Time.", "Everything in here is older than everyone you've ever met.", "Put together.");

            // The Kelp Galleries: down over the pool, to the great tank.
            Shot(kit, "kelp", new Vector3(-60f, 1.5f, -2f), new Vector3(8f, 4f, 3f), 9f,
                new[] { new Vector3(-60f, 2.6f, -2.5f), new Vector3(-50f, 9f, 14f), new Vector3(-66f, 16f, 34f), new Vector3(-58f, 3f, 4f) },
                new[] { new Vector3(-60f, 2f, 24f), new Vector3(-60f, -3f, 36f), new Vector3(-88f, 7f, 40f), new Vector3(-60f, 1f, 14f) },
                "The Kelp Galleries.", "Water Form could swim all day in here.", "Some say there are things at the bottom of the pool.");

            // The tunnel under the sea: the whale passing overhead.
            Shot(kit, "tunnel", new Vector3(-29f, 1.5f, TunnelZ), new Vector3(3f, 4f, 7f), 9f,
                new[] { new Vector3(-27f, 2f, TunnelZ - 1.5f), new Vector3(-12f, 2.6f, TunnelZ + 1.8f), new Vector3(2f, 3.2f, TunnelZ - 1.2f), new Vector3(-25f, 2.4f, TunnelZ) },
                new[] { new Vector3(0f, 6f, TunnelZ), new Vector3(6f, 13f, TunnelZ - 8f), new Vector3(10f, 15f, TunnelZ + 10f), new Vector3(-10f, 2f, TunnelZ) },
                "The Great Deep.", "Mind your head. Something enormous lives up there.");

            // The Sunken Ruins: over the water to the temple, then round to the lift in the corner.
            Shot(kit, "ruins", new Vector3(46f, 1.5f, 66f), new Vector3(4f, 4f, 8f), 10f,
                new[] { new Vector3(46.5f, 3f, 66f), new Vector3(58f, 9f, 72f), new Vector3(64f, 11f, 40f), new Vector3(60f, 6f, 26f), new Vector3(48f, 3f, 68f) },
                new[] { new Vector3(72f, 2f, 58f), new Vector3(72f, 4f, 56f), new Vector3(52f, 9f, 12f), new Vector3(49f, 4f, 10f), new Vector3(60f, 1f, 62f) },
                "The Sunken Ruins.", "A city that slipped under the sea...", "...and in the corner, a very large cardboard box.");

            // The Hall of Arms: up the row of knights to the horseman, then the castle.
            Shot(kit, "arms", new Vector3(80f, 1.5f, 1.5f), new Vector3(8f, 4f, 3f), 9f,
                new[] { new Vector3(80f, 2.6f, 2f), new Vector3(76f, 6f, -12f), new Vector3(90f, 9f, -38f), new Vector3(80f, 3f, -2f) },
                new[] { new Vector3(80f, 3f, -27f), new Vector3(80f, 2.5f, -27f), new Vector3(69f, 3f, -43f), new Vector3(80f, 1.5f, -12f) },
                "The Hall of Arms.", "Knights and samurai, standing guard after hours.");

            // The top of the lift: the Orrery Bridge, with the planets turning.
            Shot(kit, "bridge", new Vector3(36f, DeckY + 1.5f, 12f), new Vector3(10f, 4f, 12f), 9f,
                new[] { new Vector3(40f, DeckY + 3f, 9f), new Vector3(24f, DeckY + 5f, 14f), new Vector3(4f, DeckY + 4f, 10f), new Vector3(38f, DeckY + 3f, 12f) },
                new[] { new Vector3(10f, DeckY + 8f, 12f), new Vector3(0f, DeckY + 10f, 12f), new Vector3(-6f, DeckY + 3f, 12f), new Vector3(30f, DeckY + 1.5f, 12f) },
                "The Orrery Bridge.", "Going up! Well, it was going up. Now it's here.");

            // The lift waking up (played from wherever you are, when the third Heartstring is found).
            var wake = Shot(kit, "liftwakes", new Vector3(0f, -500f, 0f), Vector3.zero, 6.5f,
                new[] { new Vector3(62f, 3f, 20f), new Vector3(58f, 3.2f, 15f), new Vector3(57f, 2.6f, 11.5f) },
                new[] { new Vector3(52f, 4f, 10f), new Vector3(52f, 2.6f, 10f), new Vector3(52f, 2f, 10f) },
                "Meanwhile, in the corner of the Sunken Ruins...", "...something stirs.", "Ding!");
            director.LiftShot = wake;
        }
    }
}

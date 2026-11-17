using UnityEngine;

namespace Patchwork
{
    // The sea outside the Observatory's glass, and the Great Whale herself.
    //   - A rocky ledge round the foot of the drum, outside the windows: kelp, corals, sea fans,
    //     anemones, starfish, a lost anchor and a half-buried chest, with bubbles coming up.
    //   - Turtles, and two more shoals, going round outside the glass at window height; sea
    //     snow drifting down over the dome.
    //   - The Great Whale gets a calf, a stitched seam down each flank (she's been mended
    //     before), barnacles on her head, a light of her own, and a life (see WhaleLife):
    //     she sings, rolls over now and then, and slows down when she's being watched.
    public static partial class Level2Builder
    {
        static void ObservatorySea(LevelKit kit, Vector3 C, float R, FishSwim whale)
        {
            var rng = new System.Random(4242);
            System.Func<float, float, float> Rn = (a, b) => a + (float)rng.NextDouble() * (b - a);
            var rock = new MeshBatch();
            Color[] coralCols = { new Color(1f, 0.45f, 0.55f), new Color(1f, 0.72f, 0.3f), new Color(0.72f, 0.5f, 1f), new Color(0.4f, 0.92f, 0.8f), new Color(1f, 0.6f, 0.85f) };
            var coral = new MeshBatch[coralCols.Length];
            for (int i = 0; i < coral.Length; i++) coral[i] = new MeshBatch();
            var star = new MeshBatch();
            var t = new TankSpace { Kit = kit, Origin = C, Yaw = 0f, FloorY = C.y - 0.5f, Width = 1f, Height = 1f, Depth = 1f, Rng = new System.Random(77) };
            Color[] kelpCols = { new Color(0.32f, 0.62f, 0.4f), new Color(0.4f, 0.7f, 0.36f), new Color(0.28f, 0.55f, 0.45f) };
            int step = 0;
            // (Round the open sea only: not across the door to the bridge, and not on the south
            // side, where the balcony looks out over the atrium.)
            for (float deg = -32f; deg <= 212f; deg += 4f, step++)
            {
                if (Mathf.Abs(Mathf.DeltaAngle(deg, 90f)) < 9f) continue;
                float a = deg * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                // Rocks: low near the glass, piled higher towards the back.
                float near = Rn(R + 1.3f, R + 2.2f), far = Rn(R + 2.8f, R + 4f);
                rock.Ball(C + dir * near + Vector3.up * Rn(-0.2f, 0.3f), new Vector3(Rn(1.6f, 2.6f), Rn(1.4f, 2.2f), Rn(1.6f, 2.6f)), 4, 7);
                rock.Ball(C + dir * far + Vector3.up * Rn(0.5f, 1.6f), new Vector3(Rn(2.2f, 3.4f), Rn(2.6f, 4.6f), Rn(2.2f, 3.4f)), 4, 7);
                // Corals and sea fans along the top of the near rocks, where you can see them.
                Vector3 top = C + dir * (near + Rn(-0.3f, 0.3f)) + Vector3.up * Rn(1.1f, 1.6f);
                int ci = rng.Next(coral.Length);
                switch (step % 4)
                {
                    case 0:   // a branching coral
                        for (int b = 0; b < 4; b++)
                        {
                            var lean = Quaternion.Euler(Rn(-30f, 30f), Rn(0f, 360f), Rn(-30f, 30f));
                            coral[ci].Box(top + lean * new Vector3(0f, 0.45f, 0f), new Vector3(0.14f, 0.9f, 0.14f), lean);
                            coral[ci].Ball(top + lean * new Vector3(0f, 0.92f, 0f), Vector3.one * 0.22f, 2, 5);
                        }
                        break;
                    case 1:   // a sea fan, flat and wide
                        coral[ci].Box(top + Vector3.up * 0.6f, new Vector3(1.3f, 1.2f, 0.06f), Quaternion.LookRotation(dir, Vector3.up));
                        break;
                    case 2:   // an anemone: a fat stalk and a crown of fingers
                        coral[ci].Cylinder(top, 0.26f, 0.4f, 8);
                        for (int f = 0; f < 8; f++)
                        {
                            float fa = f / 8f * Mathf.PI * 2f;
                            var tilt = Quaternion.Euler(Mathf.Sin(fa) * 35f, 0f, -Mathf.Cos(fa) * 35f);
                            coral[ci].Box(top + Vector3.up * 0.4f + tilt * new Vector3(0f, 0.2f, 0f), new Vector3(0.07f, 0.4f, 0.07f), tilt);
                        }
                        break;
                    default:  // brain coral
                        coral[ci].Ball(top + Vector3.up * 0.3f, new Vector3(0.9f, 0.6f, 0.9f), 4, 8);
                        break;
                }
                // Starfish on the glass side of the rocks.
                if (step % 5 == 2)
                {
                    Vector3 sp = C + dir * (R + 1.05f) + Vector3.up * Rn(1.7f, 2.6f);
                    var face = Quaternion.LookRotation(-dir, Vector3.up);
                    for (int arm = 0; arm < 5; arm++)
                    {
                        var r = face * Quaternion.Euler(0f, 0f, arm * 72f);
                        star.Box(sp + r * new Vector3(0f, 0.22f, 0f), new Vector3(0.12f, 0.44f, 0.05f), r);
                    }
                }
                // Kelp, every so often, standing up from the back rocks past the top of the windows.
                if (step % 5 == 0)
                {
                    Vector3 kp = C + dir * (R + 3.1f);
                    Kelp(t, kp.x - C.x, -(kp.z - C.z), Rn(9f, 15f), kelpCols[step % kelpCols.Length], 0.5f);
                }
                // A line of bubbles from a crack in the rocks.
                if (step % 9 == 4) Bubbles(kit, C + dir * (R + 2.4f) + Vector3.up * 1.5f, 22f, 7);
            }
            rock.Build(kit.Root, "ObservatoryLedge", PatchworkAssets.Felt(new Color(0.2f, 0.24f, 0.36f)));
            for (int i = 0; i < coral.Length; i++) coral[i].Build(kit.Root, "LedgeCoral", PatchworkAssets.Glow(coralCols[i], 0.45f));
            star.Build(kit.Root, "LedgeStarfish", PatchworkAssets.Felt(new Color(1f, 0.55f, 0.35f)));

            // A lost anchor leaning on the rocks, and a sea chest with something glowing in it.
            {
                float a = 28f * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var anchor = kit.Group("LostAnchor", C + dir * (R + 2f) + Vector3.up * 2.2f);
                anchor.rotation = Quaternion.LookRotation(-dir, Vector3.up) * Quaternion.Euler(0f, 0f, 18f);
                var iron = PatchworkAssets.Plain(new Color(0.25f, 0.28f, 0.32f), 0.4f);
                kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.22f, 1.5f, 0.22f), iron, default(Vector3), anchor);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.2f, 0f), new Vector3(1.6f, 0.18f, 0.18f), iron, default(Vector3), anchor);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.62f, 0f), new Vector3(0.5f, 0.12f, 0.12f), iron, new Vector3(0f, 0f, 90f), anchor);
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.55f, -1.25f, 0f), new Vector3(1.2f, 0.2f, 0.2f), iron, new Vector3(0f, 0f, s * 35f), anchor);
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0.3f, 0.4f, -0.1f), new Vector3(0.6f, 0.9f, 0.4f), PatchworkAssets.Glow(new Color(0.45f, 0.75f, 0.45f), 0.4f), default(Vector3), anchor));
            }
            {
                float a = 172f * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var chest = kit.Group("SeaChest", C + dir * (R + 1.7f) + Vector3.up * 1.3f);
                chest.rotation = Quaternion.LookRotation(-dir, Vector3.up) * Quaternion.Euler(-8f, 0f, 6f);
                var wood = PatchworkAssets.Wood(new Color(0.42f, 0.3f, 0.2f));
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.35f, 0f), new Vector3(1.4f, 0.7f, 0.9f), wood, default(Vector3), chest);
                var lid = kit.Group("ChestLid", chest.TransformPoint(new Vector3(0f, 0.7f, 0.45f)));
                lid.SetParent(chest, true);
                lid.localRotation = Quaternion.Euler(-55f, 0f, 0f);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, -0.45f), new Vector3(1.44f, 0.12f, 0.92f), wood, default(Vector3), lid);
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.72f, 0f), new Vector3(1f, 0.25f, 0.6f), PatchworkAssets.Glow(new Color(1f, 0.82f, 0.4f), 2f), default(Vector3), chest));
                PointLight(kit, chest.position + Vector3.up * 1.2f - dir * 0.6f, new Color(1f, 0.8f, 0.45f), 6f, 1.2f);
            }
            // A cool light along the ledge, so it reads from inside.
            foreach (float d in new[] { 20f, 150f })
            {
                float a = d * Mathf.Deg2Rad;
                PointLight(kit, C + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (R + 2.5f) + Vector3.up * 5f, new Color(0.45f, 0.85f, 0.95f), 16f, 1.2f);
            }

            // ----- More life outside the glass -----
            Swim(kit, Swimmer.Turtle, C + Vector3.up * 12.5f, 28f, 28f, 5f, 0.9f, new Color(0.35f, 0.55f, 0.4f), new Color(0.85f, 0.78f, 0.55f), 40f).Bob = 0.6f;
            Swim(kit, Swimmer.Turtle, C + Vector3.up * 15.5f, 27.5f, 27.5f, -4f, 0.7f, new Color(0.4f, 0.6f, 0.45f), new Color(0.9f, 0.82f, 0.6f), 150f).Bob = 0.6f;
            Shoal(kit, C + Vector3.up * 4.5f, 28.2f, 28.2f, 11f, 34, 0.32f, new Color(1f, 0.75f, 0.35f), 93, 20f);
            Shoal(kit, C + Vector3.up * 9.5f, 28.4f, 28.4f, -8f, 28, 0.36f, new Color(0.55f, 0.85f, 1f), 94, 120f);
            Motes(kit, C + Vector3.up * 34f, new Vector3(56f, 22f, 56f), 90, new Color(0.85f, 0.95f, 1f), 1.4f, 0.09f, new Vector3(0.05f, -0.22f, 0.02f));

            // ----- The Great Whale -----
            // Her calf, keeping just behind and below her.
            var calf = Swim(kit, Swimmer.Whale, C + Vector3.up * 27.4f, 10.4f, 10.4f, whale.Speed, 0.42f, new Color(0.28f, 0.36f, 0.66f), new Color(0.85f, 0.92f, 1f), whale.Angle - 16f);
            calf.Bob = 0.9f;
            var calfLife = calf.gameObject.AddComponent<WhaleLife>();
            calfLife.Calf = true;
            calfLife.Size = 0.42f;
            // A seam down each of her flanks, where she was mended once, long ago, in gold thread.
            var w = whale.transform;
            float ws = 1.05f;
            var thread = new MeshBatch();
            for (int side = -1; side <= 1; side += 2)
                for (int i = 0; i < 9; i++)
                {
                    float z = (-2.6f + i * 0.62f) * ws;
                    float x = side * 1.14f * ws * Mathf.Sqrt(Mathf.Max(0.2f, 1f - (z / (4.3f * ws)) * (z / (4.3f * ws))));
                    Vector3 p = new Vector3(x, 0.28f * ws, z);
                    for (int c = -1; c <= 1; c += 2)
                        thread.Box(p, new Vector3(0.05f, 0.36f * ws, 0.06f), Quaternion.Euler(c * 42f, 0f, 0f));
                }
            thread.Build(w, "WhaleSeam", PatchworkAssets.Glow(new Color(1f, 0.84f, 0.45f), 1.4f));
            // Barnacles on her head.
            var barn = new MeshBatch();
            var brng = new System.Random(31);
            for (int i = 0; i < 16; i++)
            {
                float bz = (2.6f + (float)brng.NextDouble() * 1.4f) * ws, bx = ((float)brng.NextDouble() - 0.5f) * 1.4f * ws;
                float by = 0.9f * ws * Mathf.Sqrt(Mathf.Max(0.1f, 1f - (bx / (1.1f * ws)) * (bx / (1.1f * ws))));
                barn.Ball(new Vector3(bx, by, bz), Vector3.one * (0.12f + (float)brng.NextDouble() * 0.12f) * ws, 2, 5);
            }
            barn.Build(w, "WhaleBarnacles", PatchworkAssets.Felt(new Color(0.9f, 0.88f, 0.82f)));
            // A patch on her tail, sewn on with big stitches.
            var tailBone = w.Find("Tail");
            if (tailBone != null)
            {
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0.95f * ws, 0.09f * ws, -1.5f * ws), new Vector3(0.9f, 0.03f, 0.55f) * ws, PatchworkAssets.Felt(new Color(0.85f, 0.35f, 0.4f)), new Vector3(0f, 28f, 0f), tailBone));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(-0.95f * ws, 0.09f * ws, -1.5f * ws), new Vector3(0.9f, 0.03f, 0.55f) * ws, PatchworkAssets.Felt(new Color(0.95f, 0.75f, 0.35f)), new Vector3(0f, -28f, 0f), tailBone));
            }
            // Her own light, which swells when she sings.
            var lightGo = new GameObject("WhaleLight");
            lightGo.transform.SetParent(w, false);
            lightGo.transform.localPosition = new Vector3(0f, -2.2f, 0.5f);
            var glow = lightGo.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(0.6f, 0.8f, 1f);
            glow.range = 26f;
            glow.intensity = 0.8f;
            glow.shadows = LightShadows.None;
            var life = whale.gameObject.AddComponent<WhaleLife>();
            life.Stars = w.Find("WhaleStars");
            life.Glow = glow;
            life.Size = ws;
            director.SkyWhale = whale;
            director.SkyWhaleLife = life;
        }
    }
}

using UnityEngine;

namespace Patchwork
{
    // Another layer of little things that make the museum feel looked-after and lived-in:
    //   out at sea:       paper boats with tea-lights, bobbing off the end of the quay
    //   the atrium:       a big painted map of the museum on a stand by the doors, and a
    //                     rack of leaflets
    //   Deep Time:        a trail of dinosaur footprints across the floor towards the T. rex
    //   the Kelp Galleries: old glass fishing floats in nets hung on the wall
    //   the front:        proper stonework - a rusticated base, string courses, pilasters,
    //                     keystones and sills on the windows, quoins up the towers, a row of
    //                     dentils under the cornice, a balustrade with urns along the roof,
    //                     lanterns by the doors, ivy, and planters along the foot of the wall
    public static partial class Level2Builder
    {
        static void Flourish(LevelKit kit)
        {
            PaperBoats(kit);
            MuseumMap(kit);
            Footprints(kit);
            FishingFloats(kit);
            FacadeDetail(kit);
        }

        // The museum's front (the wall's outer face is at z -110, the towers stand at x 51..61).
        static void FacadeDetail(LevelKit kit)
        {
            const float face = -110f;
            var pale = PaleStone;
            var trim = new MeshBatch();     // small pale stonework, merged into one mesh
            var groove = new MeshBatch();   // the dark joints cut into the base
            // Spans of wall either side of the doors and the portico.
            float[][] low = { new[] { -51f, -4.9f }, new[] { 4.9f, 51f } };
            float[][] high = { new[] { -51f, -21.2f }, new[] { 21.2f, 51f } };

            // A rusticated base: deep stone blocks along the foot of the wall, with a cap.
            foreach (var s in low)
            {
                float mid = (s[0] + s[1]) * 0.5f, len = s[1] - s[0];
                kit.Block(new Vector3(mid, 1.3f, face - 0.25f), new Vector3(len, 2.6f, 0.5f), StoneCol, "FacadeBase", Ashlar);
                kit.Deco(PrimitiveType.Cube, new Vector3(mid, 2.75f, face - 0.3f), new Vector3(len + 0.2f, 0.3f, 0.6f), pale);
                kit.Deco(PrimitiveType.Cube, new Vector3(mid, 0.1f, face - 0.32f), new Vector3(len + 0.2f, 0.2f, 0.64f), pale);
                foreach (float y in new[] { 0.95f, 1.8f })
                    groove.Box(new Vector3(mid, y, face - 0.51f), new Vector3(len, 0.08f, 0.03f), Quaternion.identity);
                for (float x = s[0] + 1.2f; x < s[1] - 0.5f; x += 2.4f)
                {
                    float row = Mathf.Repeat(x, 4.8f) < 2.4f ? 0f : 1f;   // the joints stagger course to course
                    groove.Box(new Vector3(x + row * 1.2f, 0.55f, face - 0.51f), new Vector3(0.08f, 0.8f, 0.03f), Quaternion.identity);
                    groove.Box(new Vector3(x + (1f - row) * 1.2f, 1.38f, face - 0.51f), new Vector3(0.08f, 0.76f, 0.03f), Quaternion.identity);
                    groove.Box(new Vector3(x + row * 1.2f, 2.2f, face - 0.51f), new Vector3(0.08f, 0.7f, 0.03f), Quaternion.identity);
                }
            }
            groove.Build(kit.Root, "FacadeJoints", PatchworkAssets.Plain(new Color(0.3f, 0.26f, 0.24f), 0.1f));

            // String courses: a moulded band between the two rows of windows, and a frieze under
            // the cornice.
            foreach (var s in high)
            {
                float mid = (s[0] + s[1]) * 0.5f, len = s[1] - s[0];
                kit.Deco(PrimitiveType.Cube, new Vector3(mid, 17.9f, face - 0.2f), new Vector3(len, 0.6f, 0.4f), pale);
                trim.Box(new Vector3(mid, 18.3f, face - 0.27f), new Vector3(len, 0.2f, 0.54f), Quaternion.identity);
            }
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 33.8f, face - 0.15f), new Vector3(102f, 0.9f, 0.3f), pale);
            // Dentils: a row of little square blocks under the cornice, like teeth.
            for (float x = -50.6f; x <= 50.6f; x += 0.9f)
                trim.Box(new Vector3(x, 34.75f, face - 0.22f), new Vector3(0.45f, 0.5f, 0.44f), Quaternion.identity);

            // Pilasters: flat columns standing out from the wall, with a base and a capital.
            System.Action<float, float, float> pilaster = (x, y0, y1) =>
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(x, (y0 + y1) * 0.5f, face - 0.17f), new Vector3(1.3f, y1 - y0, 0.34f), pale);
                trim.Box(new Vector3(x, y0 + 0.2f, face - 0.24f), new Vector3(1.6f, 0.4f, 0.48f), Quaternion.identity);
                trim.Box(new Vector3(x, y1 - 0.25f, face - 0.26f), new Vector3(1.7f, 0.5f, 0.52f), Quaternion.identity);
                // Fluting: three shallow grooves down the face.
                for (int f = -1; f <= 1; f++)
                    groove.Box(new Vector3(x + f * 0.36f, (y0 + y1) * 0.5f, face - 0.35f), new Vector3(0.1f, y1 - y0 - 1.4f, 0.02f), Quaternion.identity);
            };
            groove = new MeshBatch();
            foreach (float x in new[] { -42.5f, -31.5f, 31.5f, 42.5f })
                pilaster(x, 18.6f, 33.35f);
            foreach (float x in new[] { -22.2f, 22.2f })
                pilaster(x, 2.9f, 33.35f);
            groove.Build(kit.Root, "PilasterFlutes", PatchworkAssets.Plain(new Color(0.62f, 0.58f, 0.52f), 0.1f));

            // The windows: a keystone at the top of each arch, a sill with two brackets under it.
            foreach (float x in new[] { -48f, -37f, -26f, 26f, 37f, 48f })
            {
                foreach (var win in new[] { new Vector3(4f, 4.4f, 11f), new Vector3(20f, 3.6f, 7f) })
                {
                    float y = win.x, w = win.y, h = win.z;
                    float top = y + h - w * 0.5f + w * 0.5f + 0.15f;
                    trim.Box(new Vector3(x, top + 0.2f, face - 0.3f), new Vector3(0.8f, 1.2f, 0.6f), Quaternion.identity);
                    trim.Box(new Vector3(x, top + 0.86f, face - 0.33f), new Vector3(1.1f, 0.14f, 0.66f), Quaternion.identity);
                    trim.Box(new Vector3(x, y - 0.32f, face - 0.36f), new Vector3(w + 1.2f, 0.18f, 0.72f), Quaternion.identity);
                    for (int sd = -1; sd <= 1; sd += 2)
                        trim.Box(new Vector3(x + sd * (w * 0.5f + 0.1f), y - 0.68f, face - 0.24f), new Vector3(0.36f, 0.56f, 0.46f), Quaternion.identity);
                }
            }

            // A balustrade along the roof, on top of the cornice, with an urn on every post.
            const float roofY = 36.2f, rz = -110.4f;
            for (float x = -50f; x <= 50.01f; x += 0.75f)
            {
                float rp = Mathf.Repeat(x + 50f, 10f);
                if (Mathf.Min(rp, 10f - rp) < 0.55f) continue;
                trim.Ball(new Vector3(x, roofY + 0.62f, rz), new Vector3(0.36f, 0.62f, 0.36f), 3, 6);
                trim.Box(new Vector3(x, roofY + 0.18f, rz), new Vector3(0.3f, 0.3f, 0.3f), Quaternion.identity);
                trim.Box(new Vector3(x, roofY + 1.06f, rz), new Vector3(0.3f, 0.24f, 0.3f), Quaternion.identity);
            }
            trim.Box(new Vector3(0f, roofY + 0.04f, rz), new Vector3(101f, 0.1f, 0.5f), Quaternion.identity);
            trim.Box(new Vector3(0f, roofY + 1.28f, rz), new Vector3(101f, 0.22f, 0.55f), Quaternion.identity);
            for (float x = -50f; x <= 50.01f; x += 10f)
            {
                trim.Box(new Vector3(x, roofY + 0.7f, rz), new Vector3(0.75f, 1.4f, 0.75f), Quaternion.identity);
                trim.Box(new Vector3(x, roofY + 1.45f, rz), new Vector3(0.9f, 0.14f, 0.9f), Quaternion.identity);
                trim.Ball(new Vector3(x, roofY + 1.95f, rz), new Vector3(0.6f, 0.75f, 0.6f), 4, 8);
                trim.Box(new Vector3(x, roofY + 2.42f, rz), new Vector3(0.22f, 0.3f, 0.22f), Quaternion.identity);
            }

            // Quoins: big pale corner stones up the towers, long and short in turn.
            for (int t = -1; t <= 1; t += 2)
            {
                float cx = t * 56f;
                foreach (var corner in new[] { new Vector2(-1f, -1f), new Vector2(1f, -1f), new Vector2(t, 1f) })
                {
                    float x0 = cx + corner.x * 5f, z0 = -113f + corner.y * 5f;
                    for (int i = 0; i < 27; i++)
                    {
                        float y = 0.45f + i * 1.65f;
                        bool longX = i % 2 == 0;
                        float lx = longX ? 1.8f : 1f, lz = longX ? 1f : 1.8f;
                        trim.Box(new Vector3(x0 - corner.x * (lx * 0.5f - 0.12f), y, z0 - corner.y * (lz * 0.5f - 0.12f)), new Vector3(lx, 1.5f, lz), Quaternion.identity);
                    }
                }
                // A band of stone round each tower at the height of the wall's string course.
                trim.Box(new Vector3(cx, 17.9f, -113f), new Vector3(10.44f, 0.6f, 10.44f), Quaternion.identity);
            }
            trim.Build(kit.Root, "FacadeStonework", pale, true);

            // Lanterns either side of the doors, on curly iron brackets.
            var iron = IronMat;
            for (int sd = -1; sd <= 1; sd += 2)
            {
                Vector3 at = new Vector3(sd * 7.2f, 7.6f, face - 1.1f);
                kit.Deco(PrimitiveType.Cube, new Vector3(at.x, at.y + 1.05f, face - 0.55f), new Vector3(0.12f, 0.12f, 1.1f), iron);
                kit.Deco(PrimitiveType.Cube, new Vector3(at.x, at.y + 0.5f, face - 0.15f), new Vector3(0.4f, 1.4f, 0.3f), iron);
                kit.Deco(PrimitiveType.Cube, at + Vector3.up * 0.95f, new Vector3(0.75f, 0.12f, 0.75f), iron);
                kit.Deco(PrimitiveType.Cube, at + Vector3.up * 1.12f, new Vector3(0.45f, 0.22f, 0.45f), iron, new Vector3(0f, 45f, 0f));
                NoShadow(kit.Deco(PrimitiveType.Cube, at + Vector3.up * 0.35f, new Vector3(0.56f, 1.1f, 0.56f), PatchworkAssets.Glow(new Color(1f, 0.8f, 0.48f), 2f)));
                for (int c = 0; c < 4; c++)
                {
                    Vector3 o = new Vector3((c % 2 == 0 ? -1f : 1f) * 0.29f, 0.35f, (c < 2 ? -1f : 1f) * 0.29f);
                    NoShadow(kit.Deco(PrimitiveType.Cube, at + o, new Vector3(0.06f, 1.14f, 0.06f), iron));
                }
                kit.Deco(PrimitiveType.Cube, at + Vector3.down * 0.25f, new Vector3(0.66f, 0.1f, 0.66f), iron);
                PointLight(kit, at + new Vector3(0f, 0.35f, -0.5f), new Color(1f, 0.78f, 0.48f), 9f, 1.2f);
            }

            // Planters along the foot of the wall, under the banners, with yarn flowers.
            Color[] bloom = { new Color(0.95f, 0.45f, 0.55f), new Color(1f, 0.82f, 0.35f), new Color(0.7f, 0.55f, 0.95f), new Color(0.95f, 0.95f, 0.9f) };
            var leaves = new MeshBatch();
            var flowers = new[] { new MeshBatch(), new MeshBatch(), new MeshBatch(), new MeshBatch() };
            var rng = new System.Random(707);
            foreach (float x in new[] { -42.5f, -31.5f, 31.5f, 42.5f })
            {
                kit.Block(new Vector3(x, 0.55f, face - 1.25f), new Vector3(3f, 1.1f, 1.3f), StoneCol, "Planter", pale);
                for (int i = 0; i < 9; i++)
                {
                    Vector3 p = new Vector3(x - 1.2f + i * 0.3f, 1.25f + (float)rng.NextDouble() * 0.15f, face - 1.25f + ((float)rng.NextDouble() - 0.5f) * 0.7f);
                    leaves.Ball(p, new Vector3(0.6f, 0.45f, 0.6f), 3, 6);
                    flowers[(i + (int)x) & 3].Ball(p + new Vector3(0f, 0.3f, -0.1f), Vector3.one * 0.24f, 2, 5);
                }
            }
            leaves.Build(kit.Root, "PlanterLeaves", PatchworkAssets.Yarn(new Color(0.24f, 0.46f, 0.32f)));
            for (int i = 0; i < 4; i++) flowers[i].Build(kit.Root, "PlanterFlowers", PatchworkAssets.Felt(bloom[i]));

            // Ivy climbing the front corners of the towers, thinning out as it goes up.
            var ivy = new MeshBatch();
            for (int t = -1; t <= 1; t += 2)
            {
                float x0 = t * 56f - t * 5f;   // the inner front corner
                for (int i = 0; i < 70; i++)
                {
                    float y = (float)rng.NextDouble() * (float)rng.NextDouble() * 16f;
                    float spread = 2.4f * (1f - y / 18f);
                    bool onFront = rng.NextDouble() < 0.6;
                    Vector3 p = onFront
                        ? new Vector3(x0 + t * (float)rng.NextDouble() * spread, y + 0.3f, -118.12f)
                        : new Vector3(x0 - t * 0.12f, y + 0.3f, -118f + (float)rng.NextDouble() * spread);
                    ivy.Ball(p, new Vector3(0.7f, 0.55f, 0.7f) * (0.7f + (float)rng.NextDouble() * 0.5f), 2, 5);
                }
            }
            ivy.Build(kit.Root, "TowerIvy", PatchworkAssets.Yarn(new Color(0.2f, 0.4f, 0.26f)));
        }

        // Paper boats, folded from old museum leaflets, each carrying a tea-light, bobbing on
        // the sea off the end of the quay.
        static void PaperBoats(LevelKit kit)
        {
            var rng = new System.Random(5151);
            Color[] paper = { CreamCol, new Color(0.95f, 0.85f, 0.7f), new Color(0.85f, 0.9f, 1f), new Color(1f, 0.85f, 0.85f) };
            for (int i = 0; i < 16; i++)
            {
                Vector3 p = new Vector3(-55f + (float)rng.NextDouble() * 110f, -2.62f, -168f - (float)rng.NextDouble() * 18f);
                var g = kit.Group("PaperBoat", p);
                g.rotation = Rot((float)rng.NextDouble() * 360f);
                var m = PatchworkAssets.Felt(paper[i % paper.Length]);
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.12f, 0f), new Vector3(0.5f, 0.16f, 1.1f), m, default(Vector3), g));
                for (int s = -1; s <= 1; s += 2)
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.16f, 0.3f, 0f), new Vector3(0.04f, 0.32f, 0.9f), m, new Vector3(0f, 0f, s * -28f), g));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.42f, 0f), new Vector3(0.04f, 0.4f, 0.45f), m, default(Vector3), g));
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.3f, 0.3f), new Vector3(0.14f, 0.2f, 0.14f), PatchworkAssets.Glow(new Color(1f, 0.8f, 0.45f), 2.6f), default(Vector3), g));
                var bob = g.gameObject.AddComponent<SeaBob>();
                bob.Height = 0.08f;
                bob.Rock = 6f;
                bob.Speed = 0.7f + (float)rng.NextDouble() * 0.6f;
            }
        }

        // A big painted map of the museum on an easel by the front doors, and a rack of leaflets.
        static void MuseumMap(LevelKit kit)
        {
            Vector3 foot = new Vector3(-7.5f, 0f, -104.6f);
            var g = kit.Group("MuseumMapBoard", foot);
            g.rotation = Rot(20f);
            var wood = DarkWood;
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 1.1f, 1.2f, 0f), new Vector3(0.12f, 2.4f, 0.12f), wood, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.6f, 0.04f), new Vector3(2.4f, 1.6f, 0.1f), Brass, default(Vector3), g);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.6f, 0.1f), new Vector3(2.2f, 1.4f, 0.03f), PatchworkAssets.Felt(new Color(0.95f, 0.9f, 0.78f)), default(Vector3), g));
            // The halls, as felt patches roughly where they really are (south at the bottom).
            var hall = new System.Collections.Generic.List<(Vector2 c, Vector2 s, Color col)>
            {
                (new Vector2(0f, -0.35f), new Vector2(0.45f, 0.42f), new Color(0.75f, 0.55f, 0.45f)),   // atrium
                (new Vector2(-0.6f, -0.2f), new Vector2(0.4f, 0.55f), new Color(0.75f, 0.65f, 0.4f)),   // Deep Time
                (new Vector2(-0.6f, 0.35f), new Vector2(0.4f, 0.45f), new Color(0.4f, 0.7f, 0.55f)),    // kelp
                (new Vector2(0.05f, 0.4f), new Vector2(0.55f, 0.4f), new Color(0.3f, 0.45f, 0.8f)),     // Great Deep
                (new Vector2(0.72f, 0.35f), new Vector2(0.4f, 0.5f), new Color(0.45f, 0.65f, 0.5f)),    // Ruins
                (new Vector2(0.75f, -0.15f), new Vector2(0.32f, 0.38f), new Color(0.7f, 0.45f, 0.5f)),  // Arms
                (new Vector2(0f, 0.05f), new Vector2(0.3f, 0.3f), new Color(0.35f, 0.35f, 0.7f)),       // Observatory
            };
            foreach (var h in hall)
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(h.c.x, 1.6f + h.c.y, 0.125f), new Vector3(h.s.x, h.s.y, 0.02f), PatchworkAssets.Felt(h.col), default(Vector3), g));
            // "You are here": a red pin at the doors.
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.6f - 0.62f, 0.15f), Vector3.one * 0.12f, PatchworkAssets.Glow(new Color(1f, 0.3f, 0.3f), 1.6f), default(Vector3), g));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 2.48f, 0.08f), new Vector3(1.4f, 0.22f, 0.04f), PatchworkAssets.Glow(new Color(1f, 0.82f, 0.45f), 1f), default(Vector3), g));
            TownKit.Solid(kit, foot + Vector3.up * 1.2f, new Vector3(2.4f, 2.4f, 0.4f), 20f);
            // The leaflet rack beside it.
            Vector3 rack = foot + Rot(20f) * new Vector3(2.1f, 0f, 0.2f);
            kit.Block(rack + Vector3.up * 0.65f, new Vector3(0.7f, 1.3f, 0.35f), WoodCol, "LeafletRack", DarkWood);
            Color[] leaflets = { new Color(0.4f, 0.7f, 1f), new Color(1f, 0.6f, 0.5f), new Color(0.6f, 0.9f, 0.6f), new Color(1f, 0.85f, 0.4f) };
            for (int i = 0; i < 4; i++)
                NoShadow(kit.Deco(PrimitiveType.Cube, rack + Rot(20f) * new Vector3((i % 2 - 0.5f) * 0.3f, 0.5f + (i / 2) * 0.45f, 0.2f), new Vector3(0.24f, 0.34f, 0.03f), PatchworkAssets.Felt(leaflets[i]), new Vector3(-8f, 20f, 0f)));
        }

        // Footprints of a big three-toed dinosaur, walking in from the door towards the T. rex.
        static void Footprints(LevelKit kit)
        {
            var mb = new MeshBatch();
            Vector3 from = new Vector3(-36f, 0f, -79f), to = new Vector3(-55f, 0f, -67f);
            Vector3 dir = (to - from).normalized;
            Vector3 side = Vector3.Cross(Vector3.up, dir);
            float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            int steps = Mathf.FloorToInt(Vector3.Distance(from, to) / 1.9f);
            for (int i = 0; i < steps; i++)
            {
                Vector3 p = from + dir * (i * 1.9f) + side * (i % 2 == 0 ? 0.55f : -0.55f) + Vector3.up * 0.05f;
                var q = Quaternion.Euler(0f, yaw, 0f);
                mb.Ball(p, new Vector3(0.5f, 0.02f, 0.45f), 2, 6);
                for (int t = -1; t <= 1; t++)
                {
                    var tq = Quaternion.Euler(0f, yaw + t * 28f, 0f);
                    mb.Box(p + tq * new Vector3(0f, 0f, 0.45f), new Vector3(0.16f, 0.02f, 0.55f), tq);
                }
            }
            mb.Build(kit.Root, "DinoFootprints", PatchworkAssets.Felt(new Color(0.3f, 0.22f, 0.18f)));
        }

        // Old glass fishing floats in rope nets, hung along the Kelp Galleries' east wall.
        static void FishingFloats(LevelKit kit)
        {
            Color[] glass = { new Color(0.4f, 0.8f, 0.7f), new Color(0.35f, 0.6f, 0.9f), new Color(0.55f, 0.85f, 0.5f), new Color(0.75f, 0.6f, 0.9f) };
            var rope = PatchworkAssets.Felt(new Color(0.7f, 0.58f, 0.4f));
            int n = 0;
            foreach (float z in new[] { 2f, 7f, 12f, 17f })
            {
                Vector3 hook = new Vector3(-32.2f, 13.5f, z);
                kit.Deco(PrimitiveType.Cylinder, hook, new Vector3(0.08f, 0.15f, 0.08f), IronMat, new Vector3(0f, 0f, 90f));
                for (int i = 0; i < 3; i++)
                {
                    Vector3 c = hook + new Vector3(-0.4f, -1.1f - i * 0.95f, (i - 1) * 0.35f);
                    float r = 0.38f + (n % 3) * 0.08f;
                    NoShadow(kit.Deco(PrimitiveType.Sphere, c, Vector3.one * r * 2f, PatchworkAssets.Glow(glass[n % glass.Length], 0.55f)));
                    // The net: a few rope bands round each float.
                    for (int b = 0; b < 3; b++)
                    {
                        var band = kit.Group("FloatNet", c);
                        band.rotation = Quaternion.Euler(b * 60f, 0f, 90f);
                        NoShadow(kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(r * 2.08f, 0.02f, r * 2.08f), rope, default(Vector3), band));
                    }
                    Beam(kit, c + Vector3.up * r, hook, 0.03f, rope);
                    n++;
                }
            }
        }
    }
}

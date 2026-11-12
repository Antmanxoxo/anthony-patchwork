using UnityEngine;

namespace Patchwork
{
    // Another layer of little things that make the museum feel looked-after and lived-in:
    //   out at sea:       paper boats with tea-lights, bobbing off the end of the quay
    //   the atrium:       a big painted map of the museum on a stand by the doors, and a
    //                     rack of leaflets
    //   Deep Time:        a trail of dinosaur footprints across the floor towards the T. rex
    //   the Kelp Galleries: old glass fishing floats in nets hung on the wall
    public static partial class Level2Builder
    {
        static void Flourish(LevelKit kit)
        {
            PaperBoats(kit);
            MuseumMap(kit);
            Footprints(kit);
            FishingFloats(kit);
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

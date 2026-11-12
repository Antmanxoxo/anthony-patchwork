using UnityEngine;

namespace Patchwork
{
    // Finishing touches round the Star Corridor:
    //   The Hall of Clocks    - Harrison's little H4 watch under a glass dome, beside its plaque
    //   The Night Sky Gallery - a brass telescope on its tripod, pointed out of a night window
    //   The Planet Walk       - a brass roundel on the wall behind each planet, in its colour
    //   The Moon Steps        - a piece of the Moon in a case
    public static partial class Level2Builder
    {
        static void CorridorTouches(LevelKit kit, float Y)
        {
            // ----- H4, under glass -----
            Vector3 top = Plinth(kit, new Vector3(40.9f, Y, -36.2f), new Vector3(0.9f, 1.1f, 0.9f), 0f, DarkWood);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, top + Vector3.up * 0.04f, new Vector3(0.7f, 0.04f, 0.7f), PatchworkAssets.Felt(Wine)));
            var watch = kit.Group("H4", top + Vector3.up * 0.3f);
            watch.rotation = Quaternion.Euler(-60f, 160f, 0f);
            var silver = PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.88f), 0.9f);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.42f, 0.06f, 0.42f), silver, new Vector3(90f, 0f, 0f), watch);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.06f), new Vector3(0.36f, 0.01f, 0.36f), PatchworkAssets.Felt(CreamCol), new Vector3(90f, 0f, 0f), watch));
            for (int i = 0; i < 12; i++)
            {
                float a = i * 30f * Mathf.Deg2Rad;
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(Mathf.Sin(a) * 0.14f, Mathf.Cos(a) * 0.14f, 0.07f), new Vector3(0.01f, 0.03f, 0.005f), PatchworkAssets.Felt(new Color(0.15f, 0.12f, 0.15f)), new Vector3(0f, 0f, -i * 30f), watch));
            }
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.25f, 0f), new Vector3(0.08f, 0.03f, 0.08f), silver, default(Vector3), watch));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.3f, 0f), new Vector3(0.12f, 0.02f, 0.12f), silver, new Vector3(0f, 0f, 0f), watch));
            for (int h = 0; h < 2; h++)
            {
                var hand = new GameObject("Hand").transform;
                hand.SetParent(watch, false);
                hand.localPosition = new Vector3(0f, 0f, 0.075f + h * 0.004f);
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.05f, 0f), new Vector3(0.012f, 0.1f - h * 0.03f, 0.003f), PatchworkAssets.Felt(new Color(0.1f, 0.1f, 0.3f)), default(Vector3), hand));
                var sp = hand.gameObject.AddComponent<Spinner>();
                sp.Axis = Vector3.forward;
                sp.Speed = h == 0 ? -6f : -0.5f;
            }
            NoShadow(kit.Deco(PrimitiveType.Sphere, top + Vector3.up * 0.42f, new Vector3(0.8f, 0.9f, 0.8f), CaseGlass));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, top + Vector3.up * 0.05f, new Vector3(0.86f, 0.05f, 0.86f), Brass));
            PointLight(kit, top + new Vector3(0f, 1.6f, -0.6f), new Color(1f, 0.9f, 0.75f), 4f, 1f);

            // ----- A brass telescope on its tripod at a night window -----
            Vector3 tf = new Vector3(34.5f, Y, -26.4f);
            var scope = kit.Group("Telescope", tf);
            foreach (float a in new[] { 0f, 120f, 240f })
            {
                var leg = Rot(a) * new Vector3(0f, 0f, 0.55f);
                Beam(kit, tf + Vector3.up * 1.3f, tf + leg, 0.06f, DarkWood);
                NoShadow(kit.Deco(PrimitiveType.Sphere, tf + leg + Vector3.up * 0.03f, Vector3.one * 0.08f, Brass));
            }
            NoShadow(kit.Deco(PrimitiveType.Cylinder, tf + Vector3.up * 1.32f, new Vector3(0.22f, 0.05f, 0.22f), Brass));
            var tube = kit.Group("TelescopeTube", tf + Vector3.up * 1.42f);
            tube.SetParent(scope, true);
            tube.rotation = Quaternion.LookRotation(new Vector3(-1f, 0.55f, -0.45f).normalized);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.35f), new Vector3(0.18f, 0.7f, 0.18f), Brass, new Vector3(90f, 0f, 0f), tube);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 1.08f), new Vector3(0.24f, 0.08f, 0.24f), Brass, new Vector3(90f, 0f, 0f), tube);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 1.17f), new Vector3(0.2f, 0.01f, 0.2f), PatchworkAssets.Glow(new Color(0.5f, 0.6f, 1f), 0.6f), new Vector3(90f, 0f, 0f), tube));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, -0.42f), new Vector3(0.08f, 0.1f, 0.08f), DarkWood, new Vector3(90f, 0f, 0f), tube);
            for (int r = 0; r < 3; r++)
                NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, -0.1f + r * 0.45f), new Vector3(0.2f, 0.02f, 0.2f), DarkWood, new Vector3(90f, 0f, 0f), tube));
            TownKit.Solid(kit, tf + Vector3.up * 0.8f, new Vector3(0.9f, 1.6f, 0.9f), 0f);

            // ----- The Planet Walk's roundels, on the wall behind each planet -----
            string[] names = { "Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune" };
            float[] size = { 0.32f, 0.55f, 0.58f, 0.42f, 1.5f, 1.25f, 0.85f, 0.82f };
            Color[] col =
            {
                new Color(0.7f, 0.65f, 0.6f), new Color(0.95f, 0.8f, 0.5f), new Color(0.35f, 0.6f, 1f), new Color(0.9f, 0.45f, 0.3f),
                new Color(0.9f, 0.7f, 0.5f), new Color(0.95f, 0.85f, 0.55f), new Color(0.6f, 0.9f, 0.95f), new Color(0.35f, 0.5f, 1f)
            };
            float x = 41.8f;
            for (int i = 0; i < names.Length; i++)
            {
                x += size[i] * 0.5f + 0.35f;
                float px = x;
                x += size[i] * 0.5f + 0.35f;
                // On the south wall (its face at z -16), under the gilt line, facing into the room.
                Vector3 c = new Vector3(px, Y + 2.3f, -15.94f);
                NoShadow(kit.Deco(PrimitiveType.Cylinder, c, new Vector3(0.62f, 0.03f, 0.62f), Brass, new Vector3(90f, 0f, 0f)));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, c + Vector3.forward * 0.035f, new Vector3(0.5f, 0.01f, 0.5f), PatchworkAssets.Felt(Navy), new Vector3(90f, 0f, 0f)));
                NoShadow(kit.Deco(PrimitiveType.Sphere, c + Vector3.forward * 0.05f, Vector3.one * Mathf.Lerp(0.12f, 0.36f, size[i] / 1.5f), PatchworkAssets.Glow(col[i], 0.8f)));
                // A dot for each moon it's known to have (one for every few, past four).
                int[] moons = { 0, 0, 1, 2, 4, 4, 3, 2 };
                for (int m = 0; m < moons[i]; m++)
                {
                    float a = (m / (float)Mathf.Max(1, moons[i])) * Mathf.PI * 2f + 0.6f;
                    NoShadow(kit.Deco(PrimitiveType.Sphere, c + new Vector3(Mathf.Cos(a) * 0.21f, Mathf.Sin(a) * 0.21f, 0.05f), Vector3.one * 0.04f, PatchworkAssets.Glow(CreamCol, 0.8f)));
                }
            }

            // ----- A piece of the Moon -----
            Vector3 mt = Plinth(kit, new Vector3(49.2f, Y, -4.3f), new Vector3(0.9f, 1.1f, 0.9f), 0f, PaleStone);
            var rockMb = new MeshBatch();
            rockMb.Ball(mt + new Vector3(0f, 0.16f, 0f), new Vector3(0.36f, 0.24f, 0.3f), 3, 7);
            rockMb.Ball(mt + new Vector3(0.1f, 0.22f, 0.05f), new Vector3(0.2f, 0.18f, 0.18f), 3, 6);
            rockMb.Build(kit.Root, "MoonRock", PatchworkAssets.Felt(new Color(0.45f, 0.45f, 0.48f)));
            NoShadow(kit.Deco(PrimitiveType.Cube, mt + Vector3.up * 0.34f, new Vector3(0.7f, 0.66f, 0.7f), CaseGlass));
            NoShadow(kit.Deco(PrimitiveType.Cube, mt + Vector3.up * 0.69f, new Vector3(0.74f, 0.04f, 0.74f), Brass));
            PointLight(kit, mt + new Vector3(0f, 1.2f, 0.4f), new Color(0.9f, 0.92f, 1f), 3.5f, 1f);
            Plaque(kit, new Vector3(50.4f, Y, -4.2f), 180f, "A Piece of the Moon",
                "Between 1969 and 1972, twelve astronauts walked on the Moon, and between them they brought 382 kilograms of it home: rocks, pebbles and dust.",
                "Some of the rocks are older than almost anything that's ever been found on Earth.");
        }
    }
}

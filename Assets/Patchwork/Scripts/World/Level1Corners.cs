using UnityEngine;

namespace Patchwork
{
    // Little areas for the patches of the toybox that were still bare grass (found with the
    // layout survey):
    //   Pinwheel Hill (east, between the meadow and the farm) - a field of giant spinning
    //                  pinwheel flowers and a tethered hot-air balloon
    //   Paper Boat Pond (east, below Sky Hoops) - paper boats sailing round a thread-spool
    //                  lighthouse, a family of rubber ducks and a jetty
    //   The Giant's Picnic (south-east) - a huge picnic you can climb: biscuit, sandwich,
    //                  cupcake, teapot, with toy ants marching to it
    //   Pom-Pom Grove (north-west, below the dell) - pastel pom-pom trees, a rope swing, a
    //                  hammock and a toy tea party
    public static partial class Level1Builder
    {
        // Somewhere near `near` with nothing solid in a square of half-size r, clear of our
        // own things and the trails.
        static bool ClearArea(Vector3 near, float r, out Vector3 at)
        {
            int mask = ~(1 << 2);
            for (int ring = 0; ring <= 4; ring++)
            {
                int steps = ring == 0 ? 1 : 8;
                for (int k = 0; k < steps; k++)
                {
                    float a = k * Mathf.PI * 2f / steps;
                    Vector3 p = near + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * ring * 3f;
                    if (Mathf.Abs(p.x) > BoxHalf - r - 1f || Mathf.Abs(p.z) > BoxHalf - r - 1f) continue;
                    if (Physics.CheckBox(new Vector3(p.x, 1.6f, p.z), new Vector3(r, 1.3f, r), Quaternion.identity, mask, QueryTriggerInteraction.Ignore)) continue;
                    if (Taken(p, r) || NearLane(p, r * 0.8f)) continue;
                    if (!Physics.Raycast(p + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 8f, mask, QueryTriggerInteraction.Ignore) || Mathf.Abs(hit.point.y) > 0.1f) continue;
                    at = new Vector3(p.x, 0f, p.z);
                    Take(at, r);
                    return true;
                }
            }
            at = near;
            return false;
        }

        static void Corners(LevelKit kit)
        {
            var P = kit.Info.MapPins;
            if (ClearArea(new Vector3(94f, 0f, 69f), 9f, out Vector3 hill)) { PinwheelHill(kit, hill); P.Add(new MapPin("Pinwheel Hill", hill)); }
            if (ClearArea(new Vector3(80f, 0f, -34f), 8f, out Vector3 pond)) { PaperBoatPond(kit, pond); P.Add(new MapPin("Paper Boat Pond", pond)); }
            if (ClearArea(new Vector3(95f, 0f, -75f), 8f, out Vector3 picnic)) { GiantsPicnic(kit, picnic); P.Add(new MapPin("Giant's Picnic", picnic)); }
            if (ClearArea(new Vector3(-72f, 0f, 62f), 10f, out Vector3 grove)) { PomPomGrove(kit, grove); P.Add(new MapPin("Pom-Pom Grove", grove)); }
        }

        // ---------------------------------------------------------------- Pinwheel Hill

        static void PinwheelHill(LevelKit kit, Vector3 c)
        {
            var rng = new System.Random(61);
            Color[] petal = { LevelKit.Palette.Rose, LevelKit.Palette.Butter, LevelKit.Palette.Sky, LevelKit.Palette.Lavender, new Color(1f, 0.6f, 0.35f), new Color(0.6f, 0.85f, 0.5f) };
            kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.014f, new Vector3(19f, 0.01f, 17f), PatchworkAssets.Grass(new Color(0.6f, 0.8f, 0.46f)));
            var placed = new System.Collections.Generic.List<Vector3>();
            for (int i = 0; i < 26; i++)
            {
                // Pick a spot that keeps clear of the flowers already planted (so heads don't overlap).
                Vector3 p = c;
                for (int tries = 0; tries < 25; tries++)
                {
                    float a = (float)rng.NextDouble() * Mathf.PI * 2f, r = 1.5f + (float)rng.NextDouble() * 8f;
                    p = c + new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r * 0.85f);
                    bool clear = true;
                    foreach (var q in placed) if ((q - p).sqrMagnitude < 1.6f * 1.6f) { clear = false; break; }
                    if (clear) break;
                }
                placed.Add(p);
                float h = 1.4f + (float)rng.NextDouble() * 2.8f;
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * h * 0.5f, new Vector3(0.08f, h * 0.5f, 0.08f), PatchworkAssets.Felt(new Color(0.45f, 0.68f, 0.38f)));
                kit.Deco(PrimitiveType.Sphere, p + new Vector3(0.12f, h * 0.45f, 0f), new Vector3(0.36f, 0.06f, 0.16f), PatchworkAssets.Felt(new Color(0.45f, 0.7f, 0.4f)), new Vector3(0f, 0f, -25f));
                var hub = kit.Group("PinwheelFlower", p + Vector3.up * h);
                hub.rotation = Quaternion.Euler(0f, 200f + (float)rng.NextDouble() * 40f, 0f);
                float size = 0.5f + h * 0.12f;
                Color col = petal[i % petal.Length], col2 = petal[(i + 2) % petal.Length];
                for (int b = 0; b < 4; b++)
                {
                    kit.Deco(PrimitiveType.Cube, Quaternion.Euler(0f, 0f, b * 90f) * new Vector3(size * 0.5f, size * 0.25f, 0f), new Vector3(size, size * 0.5f, 0.02f), PatchworkAssets.Felt(b % 2 == 0 ? col : col2), new Vector3(0f, 0f, b * 90f + 18f), hub);
                }
                kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * size * 0.3f, PatchworkAssets.Felt(Color.white), default(Vector3), hub);
                var spin = hub.gameObject.AddComponent<Spinner>();
                spin.Axis = Vector3.forward;
                spin.Speed = (i % 2 == 0 ? 1f : -1f) * (60f + (float)rng.NextDouble() * 90f);
            }
            // A hot-air balloon tied down, bobbing on its ropes.
            Vector3 stake = c + new Vector3(4f, 0f, 3f);
            var bal = kit.Group("TetheredBalloon", stake + Vector3.up * 13f);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(5f, 6f, 5f), PatchworkAssets.BalloonQuilt(), default(Vector3), bal);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -4.6f, 0f), new Vector3(1.6f, 1.2f, 1.6f), PatchworkAssets.Wicker(new Color(0.85f, 0.65f, 0.4f)), default(Vector3), bal);
            var cord = PatchworkAssets.Yarn(new Color(0.96f, 0.92f, 0.82f));
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    Rod(kit, new Vector3(sx * 0.7f, -4f, sz * 0.7f), new Vector3(sx * 1.6f, -2.2f, sz * 1.6f), cord, 0.05f, bal);
            var bob = bal.gameObject.AddComponent<Spinner>();
            bob.Speed = 4f;
            bob.BobHeight = 0.5f;
            bob.BobSpeed = 0.4f;
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 peg = stake + new Vector3(s * 2f, 0f, 0f);
                kit.Deco(PrimitiveType.Cylinder, peg + Vector3.up * 0.3f, new Vector3(0.15f, 0.3f, 0.15f), PatchworkAssets.Wood(new Color(0.6f, 0.45f, 0.32f)));
                kit.Rope(peg + Vector3.up * 0.5f, stake + new Vector3(s * 0.7f, 8.4f, 0f), cord, 0.04f);
            }
            TownKit.Bench(kit, c + new Vector3(-5f, 0f, -4f), 30f);
            kit.Hint(c + Vector3.up * 1.5f, new Vector3(8f, 4f, 8f), "Pinwheel Hill", "Every flower here spins in the breeze. Try a gust as Wind and watch them go!");
        }

        // ---------------------------------------------------------------- Paper Boat Pond

        static void PaperBoatPond(LevelKit kit, Vector3 c)
        {
            kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.03f, new Vector3(15f, 0.02f, 13f), PatchworkAssets.Water(new Color(0.42f, 0.66f, 0.92f)));
            kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.025f, new Vector3(16f, 0.02f, 14f), PatchworkAssets.Felt(new Color(0.85f, 0.78f, 0.6f)));
            for (int k = 0; k < 32; k++)
            {
                float a = k * Mathf.PI * 2f / 32f;
                kit.Deco(PrimitiveType.Sphere, c + new Vector3(Mathf.Cos(a) * 7.9f, 0.08f, Mathf.Sin(a) * 6.9f), new Vector3(0.7f + k % 3 * 0.2f, 0.3f, 0.6f), PatchworkAssets.Felt(k % 2 == 0 ? new Color(0.78f, 0.75f, 0.7f) : new Color(0.68f, 0.65f, 0.62f)), new Vector3(0f, a * 57f, 0f));
            }
            ClearGrass(c, 7.8f);
            // The spool lighthouse on its little island.
            Vector3 isl = c + new Vector3(1.5f, 0f, 0.5f);
            kit.Deco(PrimitiveType.Cylinder, isl + Vector3.up * 0.15f, new Vector3(3f, 0.15f, 2.6f), PatchworkAssets.Felt(new Color(0.9f, 0.82f, 0.6f)));
            Color[] thread = { new Color(0.9f, 0.3f, 0.35f), new Color(0.99f, 0.96f, 0.9f) };
            var wood = PatchworkAssets.Wood(new Color(0.85f, 0.7f, 0.5f));
            for (int k = 0; k < 3; k++)
            {
                float y = 0.3f + k * 1.5f, r = 1.1f - k * 0.18f;
                kit.Deco(PrimitiveType.Cylinder, isl + Vector3.up * (y + 0.08f), new Vector3(r * 2.3f, 0.08f, r * 2.3f), wood);
                kit.Deco(PrimitiveType.Cylinder, isl + Vector3.up * (y + 0.75f), new Vector3(r * 2f, 0.6f, r * 2f), PatchworkAssets.Yarn(thread[k % 2]));
                kit.Deco(PrimitiveType.Cylinder, isl + Vector3.up * (y + 1.42f), new Vector3(r * 2.3f, 0.08f, r * 2.3f), wood);
            }
            Vector3 lampAt = isl + Vector3.up * 5.3f;
            kit.Deco(PrimitiveType.Sphere, lampAt, Vector3.one * 0.9f, PatchworkAssets.Glow(new Color(1f, 0.9f, 0.55f), 2.4f));
            kit.Deco(PrimitiveType.Cylinder, lampAt + Vector3.up * 0.6f, new Vector3(1.1f, 0.12f, 1.1f), PatchworkAssets.Felt(new Color(0.9f, 0.3f, 0.35f)));
            var beam = kit.Group("LighthouseBeam", lampAt);
            var b = kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, 3f), new Vector3(0.5f, 0.5f, 6f), PatchworkAssets.Bubble(new Color(1f, 0.95f, 0.7f, 0.18f)), default(Vector3), beam);
            b.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var turn = beam.gameObject.AddComponent<Spinner>();
            turn.Speed = 45f;
            TownKit.Solid(kit, isl + Vector3.up * 2.4f, new Vector3(2.2f, 4.8f, 2.2f), 0f);
            // Paper boats sailing round it.
            Color[] paper = { new Color(0.99f, 0.97f, 0.92f), new Color(1f, 0.85f, 0.4f), new Color(0.6f, 0.8f, 1f), new Color(1f, 0.65f, 0.72f) };
            for (int i = 0; i < 5; i++)
            {
                var orbit = kit.Group("BoatOrbit", Vector3.zero);
                var o = orbit.gameObject.AddComponent<Orbiter>();
                o.Center = c + Vector3.up * 0.12f;
                o.Radius = 4f + (i % 3) * 1f;
                o.RadiusZ = 3.4f + (i % 3) * 0.9f;
                o.Speed = (i % 2 == 0 ? 1f : -1f) * (9f + i * 2f);
                o.Angle = i * 72f;
                o.Bob = 0.06f;
                o.Lean = 4f;
                var boat = kit.Group("PaperBoat", Vector3.zero);
                boat.SetParent(orbit, false);
                boat.localPosition = Vector3.zero;
                boat.localScale = Vector3.one * (0.8f + (i % 2) * 0.3f);
                var m = PatchworkAssets.Plain(paper[i % paper.Length], 0.4f);
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.22f, 0.2f, 0f), new Vector3(0.04f, 0.42f, 1.4f), m, new Vector3(0f, 0f, s * 30f), boat);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.02f, 0f), new Vector3(0.36f, 0.04f, 1.1f), m, default(Vector3), boat);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.6f, 0f), new Vector3(0.03f, 0.7f, 0.7f), m, new Vector3(45f, 0f, 0f), boat);
            }
            // A family of rubber ducks.
            for (int i = 0; i < 4; i++)
            {
                Vector3 p = c + new Vector3(-4.5f + i * 0.9f, 0.1f, -2.5f - (i % 2) * 0.4f);
                float s = i == 0 ? 1f : 0.55f;
                var d = kit.Group("RubberDuck", p);
                d.rotation = Quaternion.Euler(0f, 80f, 0f);
                var y = PatchworkAssets.Plain(new Color(1f, 0.85f, 0.2f), 0.75f);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.2f * s, 0f), new Vector3(0.7f, 0.5f, 0.9f) * s, y, default(Vector3), d);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.55f * s, 0.25f * s), Vector3.one * 0.45f * s, y, default(Vector3), d);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.5f * s, 0.5f * s), new Vector3(0.22f, 0.08f, 0.2f) * s, PatchworkAssets.Plain(new Color(1f, 0.55f, 0.15f), 0.75f), default(Vector3), d);
                var bob = d.gameObject.AddComponent<Rocker>();
                bob.Axis = Vector3.forward;
                bob.Amount = 6f;
                bob.Speed = 1.4f + i * 0.2f;
            }
            // A jetty with a toy fishing rod.
            Vector3 j = c + new Vector3(-7f, 0f, 3.5f);
            kit.Block(j + new Vector3(1.4f, 0.25f, 0f), new Vector3(3.6f, 0.15f, 1.5f), Color.white, "Jetty", PatchworkAssets.Planks(new Color(0.75f, 0.58f, 0.42f)));
            Rod(kit, j + new Vector3(2.8f, 0.35f, 0.4f), j + new Vector3(4.6f, 2.2f, 1.2f), PatchworkAssets.Plain(new Color(0.9f, 0.3f, 0.35f), 0.6f), 0.05f);
            kit.Rope(j + new Vector3(4.6f, 2.2f, 1.2f), j + new Vector3(5f, 0.08f, 1.6f), PatchworkAssets.Yarn(Color.white), 0.015f);
            kit.Deco(PrimitiveType.Sphere, j + new Vector3(5f, 0.1f, 1.6f), Vector3.one * 0.18f, PatchworkAssets.Plain(new Color(0.9f, 0.25f, 0.3f), 0.7f));
            kit.Hint(c + new Vector3(-6f, 1.5f, -5f), new Vector3(6f, 4f, 4f), "Paper Boat Pond", "The paper boats sail round and round the spool lighthouse. Nobody knows who folded them.");
        }

        // ---------------------------------------------------------------- The Giant's Picnic

        static void GiantsPicnic(LevelKit kit, Vector3 c)
        {
            kit.Deco(PrimitiveType.Cube, c + Vector3.up * 0.02f, new Vector3(15f, 0.02f, 11f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, new Color(0.99f, 0.96f, 0.9f), new Color(0.9f, 0.3f, 0.35f)), new Vector3(0f, 8f, 0f));
            var q = Quaternion.Euler(0f, 8f, 0f);
            // A climbing route: biscuit, sandwich, cupcake, teapot.
            Vector3 biscuit = c + q * new Vector3(-5f, 0f, -2.5f);
            kit.Block(biscuit + Vector3.up * 0.2f, new Vector3(2.4f, 0.4f, 2.4f), Color.white, "Biscuit").GetComponent<Renderer>().enabled = false;
            kit.Deco(PrimitiveType.Cylinder, biscuit + Vector3.up * 0.2f, new Vector3(2.6f, 0.2f, 2.6f), PatchworkAssets.Felt(new Color(0.88f, 0.68f, 0.42f)));
            for (int k = 0; k < 6; k++)
                kit.Deco(PrimitiveType.Sphere, biscuit + new Vector3(Mathf.Cos(k) * 0.7f, 0.42f, Mathf.Sin(k) * 0.7f), Vector3.one * 0.14f, PatchworkAssets.Felt(new Color(0.55f, 0.35f, 0.22f)));
            Vector3 sandwich = c + q * new Vector3(-1.5f, 0f, -1f);
            kit.Block(sandwich + Vector3.up * 0.6f, new Vector3(3.2f, 1.2f, 3.2f), Color.white, "Sandwich").GetComponent<Renderer>().enabled = false;
            Color[] layers = { new Color(0.95f, 0.85f, 0.6f), new Color(0.5f, 0.8f, 0.4f), new Color(1f, 0.85f, 0.35f), new Color(0.95f, 0.55f, 0.55f), new Color(0.95f, 0.85f, 0.6f) };
            float[] th = { 0.35f, 0.12f, 0.12f, 0.16f, 0.35f };
            float yy = 0f;
            for (int k = 0; k < layers.Length; k++)
            {
                float extra = (k == 1 || k == 2) ? 0.25f : 0f;
                kit.Deco(PrimitiveType.Cube, sandwich + Vector3.up * (yy + th[k] * 0.5f), new Vector3(3.2f + extra, th[k], 3.2f + extra * 0.6f), PatchworkAssets.Felt(layers[k]), new Vector3(0f, 8f + k * 3f, 0f));
                yy += th[k];
            }
            Vector3 cupcake = c + q * new Vector3(2f, 0f, 1.5f);
            kit.Block(cupcake + Vector3.up * 1.1f, new Vector3(2.4f, 2.2f, 2.4f), Color.white, "Cupcake").GetComponent<Renderer>().enabled = false;
            kit.Deco(PrimitiveType.Cylinder, cupcake + Vector3.up * 0.7f, new Vector3(2.6f, 0.7f, 2.6f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.55f, 0.75f, 1f), new Color(0.99f, 0.96f, 0.9f)));
            kit.Deco(PrimitiveType.Sphere, cupcake + Vector3.up * 1.7f, new Vector3(3f, 1.4f, 3f), PatchworkAssets.Felt(new Color(1f, 0.75f, 0.85f)));
            kit.Deco(PrimitiveType.Sphere, cupcake + Vector3.up * 2.45f, Vector3.one * 0.6f, PatchworkAssets.Plain(new Color(0.9f, 0.15f, 0.2f), 0.85f));
            for (int k = 0; k < 10; k++)
                kit.Deco(PrimitiveType.Cube, cupcake + new Vector3(Mathf.Cos(k * 0.63f) * 1f, 2.05f + (k % 2) * 0.1f, Mathf.Sin(k * 0.63f) * 1f), new Vector3(0.22f, 0.06f, 0.06f), PatchworkAssets.Felt(FlowerColors[k % FlowerColors.Length]), new Vector3(0f, k * 57f, 0f));
            Vector3 pot = c + q * new Vector3(5.2f, 0f, -1f);
            kit.Block(pot + Vector3.up * 1.6f, new Vector3(3f, 3.2f, 3f), Color.white, "Teapot").GetComponent<Renderer>().enabled = false;
            var china = PatchworkAssets.Plain(new Color(0.98f, 0.97f, 0.95f), 0.85f);
            kit.Deco(PrimitiveType.Sphere, pot + Vector3.up * 1.6f, new Vector3(3.6f, 3.2f, 3.6f), china);
            kit.Deco(PrimitiveType.Cylinder, pot + Vector3.up * 1.6f, new Vector3(3.65f, 0.3f, 3.65f), PatchworkAssets.Plain(new Color(0.45f, 0.6f, 0.95f), 0.85f));
            kit.Deco(PrimitiveType.Cylinder, pot + Vector3.up * 3.25f, new Vector3(1.8f, 0.12f, 1.8f), china);
            kit.Deco(PrimitiveType.Sphere, pot + Vector3.up * 3.5f, Vector3.one * 0.5f, PatchworkAssets.Plain(new Color(0.45f, 0.6f, 0.95f), 0.85f));
            Rod(kit, pot + q * new Vector3(-1.6f, 1.6f, 0f), pot + q * new Vector3(-2.8f, 2.8f, 0f), china, 0.35f);
            for (int k = 0; k < 6; k++)
            {
                float a = Mathf.Lerp(-80f, 80f, k / 5f) * Mathf.Deg2Rad;
                kit.Deco(PrimitiveType.Sphere, pot + q * new Vector3(1.8f + Mathf.Cos(a) * 0.7f, 1.7f + Mathf.Sin(a) * 0.8f, 0f), Vector3.one * 0.28f, china);
            }
            // Steam curling out of the spout.
            var steam = new GameObject("TeaSteam");
            steam.transform.SetParent(kit.Root, false);
            steam.transform.position = pot + q * new Vector3(-2.9f, 3f, 0f);
            steam.AddComponent<Smoker>().Every = 1.6f;
            // A treat on top for whoever climbs up.
            for (int k = 0; k < 6; k++)
            {
                float a = k * Mathf.PI / 3f;
                ScoreBubbles.Place(kit, pot + new Vector3(Mathf.Cos(a) * 0.8f, 4.5f, Mathf.Sin(a) * 0.8f), 25);
            }
            // Felt apple slices and biscuits scattered about, and toy ants marching in a loop.
            for (int k = 0; k < 4; k++)
            {
                Vector3 p = c + q * new Vector3(-6f + k * 3.5f, 0.06f, 4f);
                kit.Deco(PrimitiveType.Cylinder, p, new Vector3(0.9f, 0.06f, 0.9f), PatchworkAssets.Felt(new Color(0.88f, 0.68f, 0.42f)), new Vector3(0f, k * 40f, 0f));
            }
            var ants = kit.Group("Ants", Vector3.zero);
            for (int k = 0; k < 10; k++)
            {
                var orbit = kit.Group("AntOrbit", Vector3.zero);
                orbit.SetParent(ants, false);
                var o = orbit.gameObject.AddComponent<Orbiter>();
                o.Center = c + Vector3.up * 0.05f;
                o.Radius = 8.4f;
                o.RadiusZ = 6.4f;
                o.Speed = 10f;
                o.Angle = k * 6f;
                o.Bob = 0.02f;
                o.Lean = 0f;
                var ant = kit.Group("Ant", Vector3.zero);
                ant.SetParent(orbit, false);
                ant.localPosition = Vector3.zero;
                var black = PatchworkAssets.Felt(new Color(0.12f, 0.1f, 0.12f));
                for (int s = 0; s < 3; s++)
                    kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.12f, (s - 1) * 0.18f), Vector3.one * (s == 1 ? 0.14f : 0.18f), black, default(Vector3), ant);
                if (k % 3 == 0) kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.3f, 0f), Vector3.one * 0.2f, PatchworkAssets.Felt(new Color(0.9f, 0.25f, 0.28f)), default(Vector3), ant);
            }
            kit.Hint(c + q * new Vector3(-6f, 1.5f, -4f), new Vector3(5f, 4f, 5f), "The Giant's Picnic", "Somebody very big left their lunch out. Climb the biscuit, the sandwich and the cupcake to reach the teapot lid!");
        }

        // ---------------------------------------------------------------- Pom-Pom Grove

        static void PomPomGrove(LevelKit kit, Vector3 c)
        {
            Color[] pastel = { new Color(1f, 0.72f, 0.8f), new Color(0.75f, 0.85f, 1f), new Color(1f, 0.9f, 0.6f), new Color(0.75f, 0.95f, 0.75f), new Color(0.88f, 0.78f, 1f) };
            kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.014f, new Vector3(22f, 0.01f, 20f), PatchworkAssets.Grass(new Color(0.55f, 0.76f, 0.5f)));
            Vector3[] trees = new Vector3[9];
            for (int i = 0; i < trees.Length; i++)
            {
                float a = i * Mathf.PI * 2f / trees.Length + 0.3f;
                float r = i % 2 == 0 ? 7.5f : 5f;
                trees[i] = c + new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
                PomTree(kit, trees[i], 5.5f + (i % 3), pastel[i % pastel.Length]);
            }
            // A rope swing hanging from a branch between two trees.
            Vector3 sw = c + new Vector3(0f, 0f, 2.5f);
            var post = PatchworkAssets.Wood(new Color(0.6f, 0.45f, 0.32f));
            Rod(kit, sw + new Vector3(-1.6f, 0f, 0f), sw + new Vector3(-1.2f, 4.2f, 0f), post, 0.25f);
            Rod(kit, sw + new Vector3(1.6f, 0f, 0f), sw + new Vector3(1.2f, 4.2f, 0f), post, 0.25f);
            Rod(kit, sw + new Vector3(-1.5f, 4.2f, 0f), sw + new Vector3(1.5f, 4.2f, 0f), post, 0.22f);
            TownKit.Solid(kit, sw + new Vector3(-1.4f, 2.1f, 0f), new Vector3(0.4f, 4.2f, 0.4f), 0f);
            TownKit.Solid(kit, sw + new Vector3(1.4f, 2.1f, 0f), new Vector3(0.4f, 4.2f, 0.4f), 0f);
            var swing = kit.Group("Swing", sw + Vector3.up * 4.2f);
            var rope = PatchworkAssets.Yarn(new Color(0.95f, 0.9f, 0.8f));
            for (int s = -1; s <= 1; s += 2) Rod(kit, new Vector3(s * 0.45f, 0f, 0f), new Vector3(s * 0.45f, -3.4f, 0f), rope, 0.04f, swing);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -3.45f, 0f), new Vector3(1.1f, 0.1f, 0.5f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, pastel[0], new Color(0.99f, 0.96f, 0.9f)), default(Vector3), swing);
            var push = swing.gameObject.AddComponent<Rocker>();
            push.Axis = Vector3.right;
            push.Amount = 18f;
            push.Speed = 1.2f;
            // A hammock strung between two of the trees.
            Vector3 h0 = trees[1] + Vector3.up * 1.6f, h1 = trees[3] + Vector3.up * 1.6f;
            Vector3 mid = (h0 + h1) * 0.5f + Vector3.down * 0.6f;
            kit.Rope(h0, Vector3.Lerp(h0, mid, 0.5f) + Vector3.down * 0.1f, rope, 0.04f);
            kit.Rope(h1, Vector3.Lerp(h1, mid, 0.5f) + Vector3.down * 0.1f, rope, 0.04f);
            var ham = kit.Deco(PrimitiveType.Capsule, mid, new Vector3(0.9f, Vector3.Distance(h0, h1) * 0.25f, 0.15f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, pastel[1], new Color(0.99f, 0.96f, 0.9f)));
            ham.rotation = Quaternion.FromToRotation(Vector3.up, (h1 - h0).normalized);
            // A toy tea party: a little table, stools, cups and a teddy guest.
            Vector3 t = c + new Vector3(-2.5f, 0f, -2f);
            kit.Block(t + Vector3.up * 0.4f, new Vector3(1.6f, 0.8f, 1.6f), Color.white, "TeaTable").GetComponent<Renderer>().enabled = false;
            kit.Deco(PrimitiveType.Cylinder, t + Vector3.up * 0.78f, new Vector3(1.8f, 0.04f, 1.8f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, new Color(0.99f, 0.96f, 0.9f), pastel[4]));
            kit.Deco(PrimitiveType.Cylinder, t + Vector3.up * 0.4f, new Vector3(0.2f, 0.4f, 0.2f), PatchworkAssets.Wood(new Color(0.9f, 0.8f, 0.65f)));
            for (int k = 0; k < 3; k++)
            {
                float a = k * Mathf.PI * 2f / 3f;
                Vector3 cp = t + new Vector3(Mathf.Cos(a) * 0.5f, 0.88f, Mathf.Sin(a) * 0.5f);
                kit.Deco(PrimitiveType.Cylinder, cp, new Vector3(0.18f, 0.08f, 0.18f), PatchworkAssets.Plain(pastel[k], 0.8f));
                Vector3 stool = t + new Vector3(Mathf.Cos(a) * 1.4f, 0f, Mathf.Sin(a) * 1.4f);
                kit.Deco(PrimitiveType.Cylinder, stool + Vector3.up * 0.25f, new Vector3(0.6f, 0.25f, 0.6f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(0.9f, 0.3f, 0.35f), new Color(0.99f, 0.96f, 0.9f)));
            }
            Vector3 bear = t + new Vector3(Mathf.Cos(0f) * 1.4f, 0.5f, 0f);
            var fur = PatchworkAssets.Knit(new Color(0.75f, 0.55f, 0.4f));
            kit.Deco(PrimitiveType.Sphere, bear + Vector3.up * 0.35f, new Vector3(0.6f, 0.65f, 0.55f), fur);
            kit.Deco(PrimitiveType.Sphere, bear + Vector3.up * 0.9f, Vector3.one * 0.5f, fur);
            for (int s = -1; s <= 1; s += 2) kit.Deco(PrimitiveType.Sphere, bear + new Vector3(0f, 1.15f, s * 0.2f), Vector3.one * 0.18f, fur);
            kit.Deco(PrimitiveType.Sphere, bear + new Vector3(-0.2f, 0.88f, 0f), new Vector3(0.12f, 0.18f, 0.22f), PatchworkAssets.Felt(new Color(0.9f, 0.8f, 0.68f)));
            kit.Hint(c + new Vector3(0f, 1.5f, -6f), new Vector3(8f, 4f, 4f), "Pom-Pom Grove", "The softest trees in the whole toybox. Somebody's having a tea party!");
        }
    }
}

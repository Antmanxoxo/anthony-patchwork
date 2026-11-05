using UnityEngine;

namespace Patchwork
{
    // The Big Top: a striped circus tent near the start of town opens into a whole circus inside.
    //   The ring       - ring the bell to wake the Grumblewool, the boss (it charges, crashes
    //                    into the ring wall, and you jump on its head while it's dizzy)
    //   Whack-a-Snarl  - a booth where Snarl heads pop out of nine holes (west side)
    //   The cannon     - fires you up to the trapeze; swing across as Yarn to a secret marble (east)
    //   The crowd      - felt toys on the bleachers who cheer you on
    //   Ringmaster Rufus explains it all.
    public static partial class Level1Builder
    {
        static readonly Vector3 BigTopRoom = new Vector3(250f, 0f, -350f);
        public const string BossBeatenId = "L1_bigtop_boss";
        static readonly Color CircusRed = new Color(0.9f, 0.22f, 0.27f);
        static readonly Color CircusCream = new Color(0.99f, 0.95f, 0.86f);
        static readonly Color CircusGold = new Color(1f, 0.8f, 0.28f);
        static readonly Color CircusNavy = new Color(0.16f, 0.18f, 0.4f);

        static void BigTop(LevelKit kit)
        {
            Physics.SyncTransforms();
            Vector3 o = BigTopRoom;
            var size = new Vector3(36f, 14f, 36f);
            var stripes = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, CircusRed, CircusCream);
            Room(kit, o, size, PatchworkAssets.Felt(new Color(0.86f, 0.72f, 0.5f)), stripes,
                PatchworkAssets.Print(PatchworkAssets.PrintKind.Stars, CircusNavy, CircusGold), "BigTop");

            // ---- The tent in town, and the way in and out.
            Vector3[] spots = { new Vector3(-12f, 0f, -20f), new Vector3(12f, 0f, -20f), new Vector3(-16f, 0f, -8f), new Vector3(16f, 0f, -8f), new Vector3(-10f, 0f, -28f), new Vector3(10f, 0f, -28f) };
            Vector3 tent = Vector3.zero;
            bool found = false;
            foreach (var s in spots)
                if (FindSpot(s, 3.2f, out tent)) { found = true; break; }
            if (!found) tent = new Vector3(-12f, 0f, -20f);
            Vector3 toward = SquareSpot - tent;
            toward.y = 0f;
            Vector3 face = toward.sqrMagnitude > 0.01f ? toward.normalized : Vector3.back;
            float faceYaw = Mathf.Atan2(face.x, face.z) * Mathf.Rad2Deg;
            TownTent(kit, tent, faceYaw);
            Vector3 doorFront = tent + face * 2.9f;
            WarpAt(kit, doorFront + Vector3.up * 1.2f, new Vector3(2.4f, 2.6f, 1.6f), o + new Vector3(0f, 0f, -15f), 0f, true, "Enter the Big Top");
            kit.Hint(doorFront + face * 1.5f + Vector3.up * 1.5f, new Vector3(5f, 3f, 4f), "The Big Top",
                "The circus is in town! Step inside for a boss fight, a game of Whack-a-Snarl and a trapeze run.");

            Vector3 door = o + new Vector3(0f, 0f, -size.z * 0.5f);
            kit.Deco(PrimitiveType.Cube, door + new Vector3(0f, 1.5f, 0.05f), new Vector3(2.6f, 3f, 0.08f), PatchworkAssets.Plain(new Color(0.1f, 0.08f, 0.14f), 0.2f));
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, door + new Vector3(s * 1.15f, 1.5f, 0.12f), new Vector3(0.7f, 3.1f, 0.08f), PatchworkAssets.Felt(CircusGold), new Vector3(0f, 0f, s * -8f));
            WarpAt(kit, door + new Vector3(0f, 1.2f, 0.8f), new Vector3(2.4f, 2.6f, 1.2f), doorFront + face * 1.2f, faceYaw, true, "Go outside");
            kit.AddCheckpoint(o + new Vector3(0f, 0.05f, -14.5f), 0f);
            kit.AddCameraZone(o + new Vector3(0f, 7f, 0f), size, 2.5f, 6f);

            // ---- Lights: a warm one over the ring and coloured ones round the edges.
            CircusLight(o + new Vector3(0f, 12.5f, 2f), new Color(1f, 0.88f, 0.7f), 30f, 1.8f);
            CircusLight(o + new Vector3(-13f, 9f, -10f), new Color(1f, 0.55f, 0.75f), 16f, 1.2f);
            CircusLight(o + new Vector3(13f, 9f, -10f), new Color(0.55f, 0.75f, 1f), 16f, 1.2f);
            CircusLight(o + new Vector3(0f, 9f, 15f), new Color(1f, 0.8f, 0.45f), 18f, 1f);

            Vector3 rc = o + new Vector3(0f, 0f, 2f);
            CircusRing(kit, rc, 10f);
            Bleachers(kit, o);
            Bunting(kit, o, size);
            CircusBoss(kit, o, rc);
            WhackBooth(kit, o);
            Trapeze(kit, o);

            // Ringmaster Rufus by the door, who explains it all.
            var rufus = Toy(kit, o + new Vector3(-3.5f, 0f, -13f), 160f, ToyNpc.Kind.Teddy, "Ringmaster Rufus", new Color(0.7f, 0.5f, 0.35f), CircusRed, ToyNpc.Instrument.None, 0.9f, CircusRed);
            int line = 0;
            rufus.Lines = () =>
            {
                string[][] lines =
                {
                    new[] { "Roll up, roll up! Welcome to the greatest little show in the toybox!", "Ring the bell by the ring and the Grumblewool wakes up. Big, grumpy, and all made of Fray.",
                            "When it charges, get out of the way! Let it crash into the ring wall, then jump on its head while it's seeing stars." },
                    new[] { "Fancy a game? Step on the red button by the booth for Whack-a-Snarl.", "Jump on the Snarls as they pop up. Fifteen in thirty seconds wins a prize!" },
                    new[] { "See the cannon? Hop in and it'll fire you up to the trapeze.", "Swing across as Yarn. There's something shiny on the far platform." },
                    new[] { "Every clever thing you do earns a Merit Patch. Have a look in the pause menu!", "They get stitched onto the wall of your Pod, too. Very smart." },
                };
                return lines[line++ % lines.Length];
            };
        }

        static void CircusLight(Vector3 pos, Color c, float range, float intensity)
        {
            var go = new GameObject("CircusLight");
            go.transform.position = pos;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = c;
            l.range = range;
            l.intensity = intensity;
            l.shadows = LightShadows.None;
        }

        // A little striped circus tent in town, facing the way you come from.
        static void TownTent(LevelKit kit, Vector3 feet, float yaw)
        {
            var g = kit.Group("BigTopTent", feet);
            g.rotation = Quaternion.Euler(0f, yaw, 0f);
            var red = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, CircusRed, CircusCream);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.6f, 0f), new Vector3(5.2f, 1.6f, 5.2f), red, default(Vector3), g);
            // A cone-ish roof from stacked rings.
            for (int i = 0; i < 6; i++)
            {
                float r = Mathf.Lerp(5.8f, 0.6f, i / 5f);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 3.4f + i * 0.55f, 0f), new Vector3(r, 0.3f, r), i % 2 == 0 ? PatchworkAssets.Felt(CircusRed) : PatchworkAssets.Felt(CircusCream), default(Vector3), g);
            }
            // Scalloped gold trim round the eaves.
            for (int i = 0; i < 16; i++)
            {
                float a = i * Mathf.PI * 2f / 16f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Sin(a) * 2.85f, 3.15f, Mathf.Cos(a) * 2.85f), new Vector3(0.7f, 0.45f, 0.2f), PatchworkAssets.Felt(CircusGold), new Vector3(0f, a * Mathf.Rad2Deg, 0f), g);
            }
            // A pole and pennant on top.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 7.2f, 0f), new Vector3(0.12f, 1f, 0.12f), PatchworkAssets.Plain(CircusGold, 0.7f), default(Vector3), g);
            var flag = kit.Group("Pennant", Vector3.zero);
            flag.SetParent(g, false);
            flag.localPosition = new Vector3(0f, 7.9f, 0f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.6f, 0f, 0f), new Vector3(1.2f, 0.55f, 0.04f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, CircusNavy, CircusGold), default(Vector3), flag);
            flag.gameObject.AddComponent<FlagWave>();
            // The doorway: a dark opening with gold flaps tied back.
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.3f, 2.55f), new Vector3(1.9f, 2.6f, 0.1f), PatchworkAssets.Plain(new Color(0.1f, 0.08f, 0.14f), 0.2f), default(Vector3), g);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 1.15f, 1.4f, 2.62f), new Vector3(0.55f, 2.8f, 0.08f), PatchworkAssets.Felt(CircusGold), new Vector3(0f, 0f, s * -10f), g);
            // A big star over the door.
            var star = kit.Group("DoorStar", Vector3.zero);
            star.SetParent(g, false);
            star.localPosition = new Vector3(0f, 3.0f, 2.75f);
            for (int p = 0; p < 5; p++)
                kit.Deco(PrimitiveType.Sphere, Quaternion.Euler(0f, 0f, p * 72f) * new Vector3(0f, 0.22f, 0f), new Vector3(0.2f, 0.5f, 0.1f), PatchworkAssets.Glow(CircusGold, 0.8f), new Vector3(0f, 0f, p * 72f), star);
            var spin = star.gameObject.AddComponent<Spinner>();
            spin.Axis = Vector3.forward;
            spin.Speed = 30f;
            // Solid, so you walk round it.
            var solid = kit.Block(feet + Vector3.up * 1.6f, new Vector3(4.6f, 3.2f, 4.6f), Color.white, "TentSolid");
            solid.rotation = g.rotation;
            solid.GetComponent<Renderer>().enabled = false;
        }

        static readonly System.Collections.Generic.List<GateBar> ringGates = new System.Collections.Generic.List<GateBar>();

        // The ring: a sawdust arena inside a tall colosseum wall, with arched niches, pennants
        // on top, and two gates (south and north) that drop shut when the fight starts.
        static void CircusRing(LevelKit kit, Vector3 c, float r)
        {
            ringGates.Clear();
            kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.01f, new Vector3(r * 2f, 0.01f, r * 2f), PatchworkAssets.Felt(new Color(0.93f, 0.8f, 0.58f)));
            kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.02f, new Vector3(r * 0.5f, 0.01f, r * 0.5f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stars, CircusRed, CircusGold));
            var wall = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, CircusCream, new Color(0.95f, 0.85f, 0.7f));
            var band = PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, CircusRed, CircusGold);
            var niche = PatchworkAssets.Plain(new Color(0.2f, 0.12f, 0.22f), 0.2f);
            var rim = PatchworkAssets.Plain(CircusGold, 0.7f);
            const int segs = 40;
            const float h = 3.6f;
            float wseg = 2f * Mathf.PI * (r + 0.4f) / segs;
            for (int i = 0; i < segs; i++)
            {
                float a = (i + 0.5f) * 360f / segs;
                Vector3 d = Quaternion.Euler(0f, a, 0f) * Vector3.forward;
                var rot = Quaternion.Euler(0f, a, 0f);
                // Gate gaps to the south and north.
                bool gateS = Mathf.Abs(Mathf.DeltaAngle(a, 180f)) < 20f, gateN = Mathf.Abs(Mathf.DeltaAngle(a, 0f)) < 20f;
                if (gateS || gateN) continue;
                Vector3 p = c + d * (r + 0.4f);
                var t = kit.Block(p + Vector3.up * h * 0.5f, new Vector3(wseg * 1.03f, h, 0.8f), Color.white, "ColosseumWall", wall);
                t.rotation = rot;
                kit.Deco(PrimitiveType.Cube, p + Vector3.up * 0.45f, new Vector3(wseg * 1.04f, 0.9f, 0.86f), band, new Vector3(0f, a, 0f));
                kit.Deco(PrimitiveType.Cube, p + Vector3.up * (h + 0.1f), new Vector3(wseg * 1.05f, 0.2f, 1f), rim, new Vector3(0f, a, 0f));
                // An arched niche on the inside face, every other section.
                if (i % 2 == 0)
                {
                    kit.Deco(PrimitiveType.Cube, p - d * 0.41f + Vector3.up * 2.1f, new Vector3(wseg * 0.5f, 1.4f, 0.04f), niche, new Vector3(0f, a, 0f));
                    kit.Deco(PrimitiveType.Cylinder, p - d * 0.41f + Vector3.up * 2.8f, new Vector3(wseg * 0.5f, 0.02f, wseg * 0.5f), niche, new Vector3(90f, a, 0f));
                }
                // Pennant poles along the top.
                if (i % 4 == 1)
                {
                    kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * (h + 1.1f), new Vector3(0.1f, 1f, 0.1f), rim);
                    var flag = kit.Group("RingPennant", p + Vector3.up * (h + 1.8f));
                    flag.rotation = rot;
                    kit.Deco(PrimitiveType.Cube, new Vector3(0.45f, 0f, 0f), new Vector3(0.9f, 0.45f, 0.03f), PatchworkAssets.Felt(Bright[i % Bright.Length]), default(Vector3), flag);
                    flag.gameObject.AddComponent<FlagWave>();
                }
            }
            // The two gates: striped portcullis bars that drop when the bell rings.
            foreach (float a in new[] { 180f, 0f })
            {
                Vector3 d = Quaternion.Euler(0f, a, 0f) * Vector3.forward;
                Vector3 p = c + d * (r + 0.4f);
                float gw = wseg * 4f - 0.1f;
                // Gateposts and an arch over the top.
                Vector3 side = Quaternion.Euler(0f, 90f, 0f) * d;
                for (int s = -1; s <= 1; s += 2)
                {
                    kit.Block(p + side * s * (gw * 0.5f + 0.35f) + Vector3.up * (h + 0.6f) * 0.5f, new Vector3(0.8f, h + 0.6f, 1.1f), Color.white, "GatePost", band).rotation = Quaternion.Euler(0f, a, 0f);
                    kit.Deco(PrimitiveType.Sphere, p + side * s * (gw * 0.5f + 0.35f) + Vector3.up * (h + 0.9f), Vector3.one * 0.7f, rim);
                }
                kit.Deco(PrimitiveType.Cube, p + Vector3.up * (h + 0.35f), new Vector3(gw + 1.4f, 0.5f, 1.1f), band, new Vector3(0f, a, 0f));
                var bars = kit.Group("Gate", p + Vector3.up * (h + 2.2f));
                bars.rotation = Quaternion.Euler(0f, a, 0f);
                var barMat = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, CircusRed, CircusCream);
                for (int k = 0; k < 7; k++)
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(-gw * 0.5f + 0.3f + k * (gw - 0.6f) / 6f, 0f, 0f), new Vector3(0.22f, h * 0.5f, 0.22f), barMat, default(Vector3), bars);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, -h * 0.25f, 0f), new Vector3(gw, 0.25f, 0.25f), rim, default(Vector3), bars);
                var col = bars.gameObject.AddComponent<BoxCollider>();
                col.size = new Vector3(gw, h, 0.5f);
                var gate = bars.gameObject.AddComponent<GateBar>();
                gate.Closed = p + Vector3.up * h * 0.5f;
                gate.Open = p + Vector3.up * (h + 2.2f);
                ringGates.Add(gate);
            }
        }

        static void ShutGates(bool shut)
        {
            foreach (var g in ringGates)
                if (g != null) g.Shut = shut;
            Sfx.Play(SfxId.PlatformThunk, 0.6f);
        }

        // Three rows of benches along the back with a cheering felt crowd on them.
        static void Bleachers(LevelKit kit, Vector3 o)
        {
            var wood = PatchworkAssets.Planks(new Color(0.85f, 0.6f, 0.4f));
            float[] z = { 14.5f, 15.9f, 17.3f };
            float[] h = { 0.7f, 1.4f, 2.1f };
            var crowdRoot = kit.Group("Crowd", o);
            var crowd = crowdRoot.gameObject.AddComponent<Crowd>();
            var rng = new System.Random(9);
            for (int row = 0; row < 3; row++)
            {
                kit.Block(o + new Vector3(0f, h[row] * 0.5f, z[row]), new Vector3(30f, h[row], 1.4f), Color.white, "Bleacher", wood);
                for (int i = 0; i < 11; i++)
                {
                    float x = -13.5f + i * 2.7f + (row % 2) * 1.2f;
                    var fan = new GameObject("Fan").transform;
                    fan.SetParent(crowdRoot, false);
                    fan.localPosition = new Vector3(x, h[row], z[row]);
                    Color col = Bright[rng.Next(Bright.Length)];
                    var felt = PatchworkAssets.Felt(col);
                    kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.4f, 0f), new Vector3(0.75f, 0.8f, 0.65f), felt, default(Vector3), fan);
                    kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1f, 0f), Vector3.one * 0.6f, felt, default(Vector3), fan);
                    for (int s = -1; s <= 1; s += 2)
                    {
                        kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.13f, 1.05f, -0.26f), new Vector3(0.15f, 0.17f, 0.08f), PatchworkAssets.Felt(Color.white), default(Vector3), fan);
                        kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.13f, 1.04f, -0.3f), Vector3.one * 0.07f, PatchworkAssets.Plain(UIKit.Ink, 0.7f), default(Vector3), fan);
                        kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.4f, 0.65f, -0.05f), new Vector3(0.18f, 0.4f, 0.18f), felt, new Vector3(0f, 0f, s * 30f), fan);
                    }
                    // Some wave little pennants.
                    if (rng.Next(3) == 0)
                        kit.Deco(PrimitiveType.Cube, new Vector3(0.5f, 1.2f, 0f), new Vector3(0.05f, 0.35f, 0.5f), PatchworkAssets.Felt(Bright[rng.Next(Bright.Length)]), new Vector3(0f, 0f, -15f), fan);
                    crowd.Add(fan);
                }
            }
        }

        // Strings of felt flags criss-crossing under the roof.
        static void Bunting(LevelKit kit, Vector3 o, Vector3 size)
        {
            var cord = PatchworkAssets.Yarn(CircusCream);
            Vector3[,] lines =
            {
                { new Vector3(-17.5f, 12.5f, -17.5f), new Vector3(17.5f, 12.5f, 17.5f) },
                { new Vector3(17.5f, 12.5f, -17.5f), new Vector3(-17.5f, 12.5f, 17.5f) },
                { new Vector3(-17.5f, 11.5f, 0f), new Vector3(17.5f, 11.5f, 0f) },
            };
            for (int l = 0; l < lines.GetLength(0); l++)
            {
                Vector3 a = o + lines[l, 0], b = o + lines[l, 1];
                const int segs = 12;
                System.Func<float, Vector3> P = k => Vector3.Lerp(a, b, k) + Vector3.down * Mathf.Sin(k * Mathf.PI) * 2f;
                for (int i = 0; i < segs; i++) kit.Rope(P(i / (float)segs), P((i + 1) / (float)segs), cord, 0.05f);
                Vector3 dir = (b - a).normalized;
                float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                for (int i = 1; i < 24; i++)
                {
                    Vector3 p = P(i / 24f) + Vector3.down * 0.35f;
                    kit.Deco(PrimitiveType.Cube, p, new Vector3(0.05f, 0.55f, 0.55f), PatchworkAssets.Felt(Bright[(i + l) % Bright.Length]), new Vector3(45f, yaw, 0f));
                }
            }
        }

        // The boss: the Grumblewool asleep in the ring, and the bell that wakes it.
        static void CircusBoss(LevelKit kit, Vector3 o, Vector3 rc)
        {
            bool beaten = GameState.IsCollected(BossBeatenId);
            var prize = PrizeBubble.Place(kit, rc + Vector3.up * 2.2f, "deco_plush", !beaten);
            kit.Hint(rc + new Vector3(0f, 1.5f, -8f), new Vector3(6f, 3f, 3f), "The Grumblewool",
                beaten ? "You beat the Grumblewool! The crowd still talks about it."
                       : "Ring the bell to wake it. Dodge its charge so it crashes into the ring wall, then jump on its head while it's dizzy!");

            Vector3 bell = rc + new Vector3(4f, 0f, -6f);
            kit.Deco(PrimitiveType.Cylinder, bell + Vector3.up * 1.1f, new Vector3(0.18f, 1.1f, 0.18f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, CircusRed, CircusCream));
            kit.Deco(PrimitiveType.Cube, bell + Vector3.up * 2.25f, new Vector3(1f, 0.12f, 0.12f), PatchworkAssets.Wood(new Color(0.6f, 0.4f, 0.25f)));
            var swing = kit.Group("Bell", bell + Vector3.up * 2.2f);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.35f, 0f), new Vector3(0.6f, 0.65f, 0.6f), PatchworkAssets.Plain(CircusGold, 0.9f), default(Vector3), swing);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.62f, 0f), new Vector3(0.7f, 0.06f, 0.7f), PatchworkAssets.Plain(CircusGold, 0.9f), default(Vector3), swing);
            if (beaten) return;

            var boss = Grumblewool.Create(kit, rc, rc + new Vector3(0f, 0f, 4f));
            var spot = kit.Volume<UseSpot>(bell + new Vector3(0f, 1.2f, -0.6f), new Vector3(2.6f, 3f, 2.6f), "RingTheBell");
            spot.Label = "Ring the bell!";
            spot.PromptPoint = bell + Vector3.up * 3.2f;
            spot.Used = player =>
            {
                if (!boss.Asleep) return;
                Sfx.Play(SfxId.Checkpoint, 0.8f);
                var rock = swing.gameObject.GetComponent<Rocker>();
                if (rock == null) rock = swing.gameObject.AddComponent<Rocker>();
                rock.Axis = Vector3.forward;
                rock.Amount = 25f;
                rock.Speed = 9f;
                boss.Begin();
            };
            boss.Started = () => { spot.gameObject.SetActive(false); ShutGates(true); };
            boss.ResetFight = () => { spot.gameObject.SetActive(true); ShutGates(false); };
            boss.Defeated = () =>
            {
                ShutGates(false);
                GameState.SetCollected(BossBeatenId);
                GameState.Save();
                prize.Reveal();
                var rock = swing.gameObject.GetComponent<Rocker>();
                if (rock != null) rock.Amount = 6f;
            };
        }

        // Whack-a-Snarl: a polka-dot stage with nine holes and a big red start button.
        static void WhackBooth(LevelKit kit, Vector3 o)
        {
            Vector3 stage = o + new Vector3(-12f, 0f, -8f);
            kit.Block(stage + Vector3.up * 0.25f, new Vector3(6.2f, 0.5f, 6.2f), Color.white, "WhackStage", PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(0.3f, 0.6f, 0.95f), CircusCream));
            // A striped awning on two posts behind it.
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, stage + new Vector3(s * 3f, 2.2f, 3.2f), new Vector3(0.2f, 2.2f, 0.2f), PatchworkAssets.Plain(CircusGold, 0.6f));
            kit.Deco(PrimitiveType.Cube, stage + new Vector3(0f, 4.3f, 2.8f), new Vector3(6.6f, 0.15f, 1.6f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, CircusRed, CircusCream), new Vector3(-20f, 0f, 0f));
            for (int i = 0; i < 9; i++)
                kit.Deco(PrimitiveType.Sphere, stage + new Vector3(-3f + i * 0.75f, 3.9f, 2.1f), new Vector3(0.45f, 0.4f, 0.1f), PatchworkAssets.Felt(i % 2 == 0 ? CircusRed : CircusCream));

            Vector3 pad = o + new Vector3(-12f, 0f, -12.4f);
            var game = kit.Volume<WhackASnarl>(pad + Vector3.up * 1f, new Vector3(1.8f, 2f, 1.8f), "WhackASnarl");
            game.Title = "Whack-a-Snarl";
            game.Duration = 30f;
            kit.Deco(PrimitiveType.Cylinder, pad + Vector3.up * 0.06f, new Vector3(1.6f, 0.06f, 1.6f), PatchworkAssets.Felt(CircusCream));
            var buttonRoot = kit.Group("WhackButton", pad);
            var button = kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.18f, 0f), new Vector3(1.1f, 0.12f, 1.1f), PatchworkAssets.Plain(CircusRed, 0.7f), default(Vector3), buttonRoot);
            game.Button = button;
            game.Marble = SecretMarble.Place(kit, stage + new Vector3(0f, 2.4f, 0f), MarbleIds[12], MarbleColor(12), true);

            var dark = PatchworkAssets.Plain(new Color(0.08f, 0.06f, 0.1f), 0.2f);
            for (int gx = -1; gx <= 1; gx++)
                for (int gz = -1; gz <= 1; gz++)
                {
                    Vector3 hole = stage + new Vector3(gx * 1.8f, 0.5f, gz * 1.8f);
                    kit.Deco(PrimitiveType.Cylinder, hole + Vector3.up * 0.005f, new Vector3(1.1f, 0.005f, 1.1f), dark);
                    var head = new GameObject("SnarlHead").transform;
                    head.SetParent(game.transform, false);
                    Vector3 local = hole - game.transform.position;
                    head.localPosition = local + Vector3.down * 0.6f;
                    Monsters.Part(PrimitiveType.Sphere, head, Vector3.zero, Vector3.one * 0.9f, Monsters.Dark);
                    for (int k = 0; k < 6; k++)
                        Monsters.Part(PrimitiveType.Capsule, head, Quaternion.Euler(k * 30f - 30f, k * 60f, 0f) * new Vector3(0f, 0.2f, 0.35f), new Vector3(0.09f, 0.3f, 0.09f), Monsters.Grey, new Vector3(k * 30f, k * 60f, 0f));
                    Monsters.Part(PrimitiveType.Sphere, head, new Vector3(-0.16f, 0.1f, -0.38f), new Vector3(0.14f, 0.18f, 0.08f), Monsters.Glow);
                    Monsters.Part(PrimitiveType.Sphere, head, new Vector3(0.16f, 0.1f, -0.38f), new Vector3(0.14f, 0.18f, 0.08f), Monsters.Glow);
                    game.Heads.Add(head);
                    game.Holes.Add(local);
                }
            kit.Hint(pad + new Vector3(0f, 1.5f, -1.8f), new Vector3(4f, 3f, 2.4f), "Whack-a-Snarl",
                "Step on the red button to start. Jump on the Snarls as they pop up: 15 in 30 seconds wins!");
        }

        // The cannon fires you up to the trapeze; swing across the yarn buttons as Yarn to the
        // far platform and a secret marble.
        static void Trapeze(LevelKit kit, Vector3 o)
        {
            Vector3 cannon = o + new Vector3(14f, 0f, -10f);
            var barrel = kit.Group("Cannon", cannon + Vector3.up * 1.2f);
            barrel.rotation = Quaternion.Euler(-40f, 0f, 0f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.6f), new Vector3(1.3f, 1.6f, 1.3f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stars, CircusNavy, CircusGold), new Vector3(90f, 0f, 0f), barrel);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 2.2f), new Vector3(1.5f, 0.15f, 1.5f), PatchworkAssets.Plain(CircusGold, 0.8f), new Vector3(90f, 0f, 0f), barrel);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 2.28f), new Vector3(1.05f, 0.1f, 1.05f), PatchworkAssets.Plain(new Color(0.08f, 0.06f, 0.1f), 0.2f), new Vector3(90f, 0f, 0f), barrel);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cylinder, cannon + new Vector3(s * 0.85f, 0.7f, 0f), new Vector3(1.4f, 0.12f, 1.4f), PatchworkAssets.Wood(new Color(0.85f, 0.3f, 0.3f)), new Vector3(0f, 0f, 90f));
                kit.Deco(PrimitiveType.Cylinder, cannon + new Vector3(s * 0.92f, 0.7f, 0f), new Vector3(0.35f, 0.12f, 0.35f), PatchworkAssets.Plain(CircusGold, 0.8f), new Vector3(0f, 0f, 90f));
            }
            Vector3 top = o + new Vector3(13f, 7f, 7f);
            var spot = kit.Volume<UseSpot>(cannon + new Vector3(0f, 1.2f, -2.4f), new Vector3(2.6f, 3f, 2.4f), "Cannon");
            spot.Label = "Climb into the cannon";
            spot.PromptPoint = cannon + Vector3.up * 3.6f;
            Vector3 mouth = cannon + Vector3.up * 1.2f + barrel.forward * 2.2f + Vector3.down * 0.6f;
            spot.Used = player =>
            {
                if (ScriptedHop.Busy) return;
                Sfx.Play(SfxId.PlatformThunk, 0.9f);
                Sfx.Play(SfxId.Gust, 0.8f);
                FxBurst.Spawn(FxBurst.Kind.Puff, mouth, Color.white, barrel.forward);
                FxBurst.Spawn(FxBurst.Kind.Scraps, mouth, CircusGold);
                if (player.Game != null) player.Game.CameraRig.Kick(0.4f);
                Haptics.Pulse(0.6f, 0.4f, 0.15f);
                player.Hand.Release();
                Crowd.Cheer(1.5f);
                ScriptedHop.Fly(mouth, top, 3.5f, 1.3f);
            };
            kit.Hint(cannon + new Vector3(0f, 1.5f, -4.4f), new Vector3(4f, 3f, 2.4f), "The Trapeze",
                "Climb in the cannon to fly up to the trapeze. Then swing across as Yarn: hold {Ability} near the yarn buttons.");

            // The two platforms, on striped poles.
            var plat = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, CircusNavy, CircusGold);
            foreach (float x in new[] { 13f, -13f })
            {
                Vector3 c = o + new Vector3(x, 6.75f, 7f);
                kit.Block(c, new Vector3(4f, 0.5f, 4f), Color.white, "TrapezePlatform", plat);
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(c.x + s * 1.6f, 3.25f, c.z), new Vector3(0.25f, 3.25f, 0.25f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, CircusRed, CircusCream));
            }
            // Three big yarn buttons to swing from.
            kit.Anchor(o + new Vector3(7f, 11.5f, 8f), 13f, LevelKit.AnchorStyle.Button, 1.2f);
            kit.Anchor(o + new Vector3(0f, 11.5f, 9f), 13f, LevelKit.AnchorStyle.Button, 1.2f);
            kit.Anchor(o + new Vector3(-7f, 11.5f, 8f), 13f, LevelKit.AnchorStyle.Button, 1.2f);

            Vector3 far = o + new Vector3(-13f, 7f, 7f);
            SecretMarble.Place(kit, far + Vector3.up * 1.2f, MarbleIds[13], MarbleColor(13));
            var done = kit.Volume<TouchZone>(far + Vector3.up * 1.2f, new Vector3(4f, 2.4f, 4f), "TrapezeDone");
            done.Entered = player =>
            {
                if (Merits.Has(Merits.Id.TrapezeArtist)) return;
                Merits.Earn(Merits.Id.TrapezeArtist);
                Crowd.Cheer(3f);
                ScoreKeeper.Bonus(800, far + Vector3.up * 2f);
            };
        }
    }
}

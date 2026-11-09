using UnityEngine;

namespace Patchwork
{
    // Things made from the sewing box: two ways over the north half of the Thread Ravine (wobbly
    // button stepping stones, and a tape-measure bridge on a timer), and the nursery's mobile
    // turning slowly high above the toybox.
    public static partial class Level1Builder
    {
        const float StonesZ = 80f, TapeZ = 67f;   // north of the hedge that runs along the east edge (it ends at z 60)

        static void CraftCrossings(LevelKit kit)
        {
            ButtonStones(kit);
            TapeMeasureBridge(kit);
            kit.Info.MapPins.Add(new MapPin("Button Stones", new Vector3((RavineEast + RavineWest) * 0.5f, 0f, StonesZ)));
            kit.Info.MapPins.Add(new MapPin("Tape-Measure Bridge", new Vector3((RavineEast + RavineWest) * 0.5f, 0f, TapeZ)));
        }

        // ---------------------------------------------------------------- Puffball dandelions

        // Clusters of puffballs out in the meadows and along the edges of town, to run through.
        static void PuffballMeadows(LevelKit kit)
        {
            var puffs = Puffballs.Ensure(kit);
            var rng = new System.Random(2024);
            Vector3[] centres =
            {
                new Vector3(75f, 0f, -100f), new Vector3(100f, 0f, 30f), new Vector3(100f, 0f, 95f), new Vector3(-6f, 0f, -101f),
                new Vector3(94f, 0f, 69f), new Vector3(20f, 0f, 100f), new Vector3(60f, 0f, 10f), new Vector3(70f, 0f, 50f),
                new Vector3(-20f, 0f, 80f), new Vector3(55f, 0f, -60f), new Vector3(-70f, 0f, 70f), new Vector3(-95f, 0f, 60f),
                new Vector3(0f, 0f, 70f), new Vector3(60f, 0f, -20f), new Vector3(-25f, 0f, -95f), new Vector3(85f, 0f, -30f),
            };
            foreach (var c in centres)
            {
                int made = 0;
                // A loose drift of them, a couple of steps apart, so a run through pops a chain.
                float heading = (float)rng.NextDouble() * Mathf.PI * 2f;
                for (int tries = 0; tries < 26 && made < 7; tries++)
                {
                    float along = (tries - 13) * 1.6f + ((float)rng.NextDouble() - 0.5f);
                    float side = ((float)rng.NextDouble() - 0.5f) * 3f;
                    Vector3 want = c + new Vector3(Mathf.Cos(heading) * along - Mathf.Sin(heading) * side, 0f, Mathf.Sin(heading) * along + Mathf.Cos(heading) * side);
                    Vector3 p;
                    if (!Spot(want, 0.7f, out p)) continue;
                    puffs.Add(kit, p, 0.75f + (float)rng.NextDouble() * 0.45f);
                    made++;
                }
            }
        }

        // ---------------------------------------------------------------- Button stepping stones

        static void ButtonStones(LevelKit kit)
        {
            var steel = PatchworkAssets.Plain(new Color(0.82f, 0.84f, 0.88f), 0.85f);
            var threadMat = PatchworkAssets.Yarn(new Color(0.96f, 0.92f, 0.84f));
            Color[] cols = { new Color(0.93f, 0.36f, 0.4f), new Color(1f, 0.78f, 0.3f), new Color(0.4f, 0.72f, 0.9f), new Color(0.6f, 0.8f, 0.45f), new Color(0.78f, 0.56f, 0.9f) };
            float[] height = { 0.15f, 0.6f, 1.05f, 0.6f, 0.15f };
            float[] offZ = { 0f, 0.9f, 0f, -0.9f, 0f };
            const float lineY = 7.2f, d = 2.3f;

            // A giant dressmaker's pin stuck in each bank, with the thread strung between their heads.
            Vector3 pinW = new Vector3(RavineWest - 1.6f, 0f, StonesZ), pinE = new Vector3(RavineEast + 1.6f, 0f, StonesZ);
            Color[] heads = { new Color(0.93f, 0.36f, 0.4f), new Color(0.4f, 0.72f, 0.9f) };
            int h = 0;
            foreach (var pin in new[] { pinW, pinE })
            {
                var g = kit.Group("GiantPin", pin);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, lineY * 0.5f, 0f), new Vector3(0.16f, lineY * 0.5f, 0.16f), steel, default(Vector3), g);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, lineY + 0.35f, 0f), Vector3.one * 1f, PatchworkAssets.Plain(heads[h++], 0.9f), default(Vector3), g);
                // A little mound of felt where it goes into the ground.
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.02f, 0f), new Vector3(0.9f, 0.22f, 0.9f), PatchworkAssets.Felt(new Color(0.85f, 0.4f, 0.45f)), default(Vector3), g);
                var col = g.gameObject.AddComponent<CapsuleCollider>();
                col.center = new Vector3(0f, lineY * 0.5f, 0f);
                col.radius = 0.14f;
                col.height = lineY;
            }
            kit.Rope(pinW + Vector3.up * lineY, pinE + Vector3.up * lineY, threadMat, 0.07f);

            float span = RavineEast - RavineWest, step = span / height.Length;
            for (int i = 0; i < height.Length; i++)
            {
                Vector3 top = new Vector3(RavineWest + step * (i + 0.5f), height[i], StonesZ + offZ[i]);
                var g = kit.Group("ButtonStone", top);
                // The part you stand on: a round, solid disc (its top is the group's origin).
                var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Object.DestroyImmediate(disc.GetComponent<Collider>());
                disc.name = "Disc";
                disc.transform.SetParent(g, false);
                disc.transform.localPosition = new Vector3(0f, -0.16f, 0f);
                disc.transform.localScale = new Vector3(d, 0.16f, d);
                disc.GetComponent<MeshRenderer>().sharedMaterial = PatchworkAssets.Plain(cols[i], 0.75f);
                var mc = disc.AddComponent<MeshCollider>();
                mc.sharedMesh = disc.GetComponent<MeshFilter>().sharedMesh;
                mc.convex = true;
                // Raised rim, four holes and the cross of thread through them.
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.2f, 0f), new Vector3(d + 0.22f, 0.1f, d + 0.22f), PatchworkAssets.Plain(cols[i] * 0.8f, 0.75f), default(Vector3), g);
                var hole = PatchworkAssets.Plain(new Color(0.2f, 0.15f, 0.2f), 0.2f);
                for (int k = 0; k < 4; k++)
                {
                    float hx = (k % 2 == 0 ? -1f : 1f) * 0.3f, hz = (k < 2 ? -1f : 1f) * 0.3f;
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(hx, 0.004f, hz), new Vector3(0.2f, 0.004f, 0.2f), hole, default(Vector3), g);
                }
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.02f, 0f), new Vector3(0.95f, 0.035f, 0.09f), threadMat, new Vector3(0f, 45f, 0f), g);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.025f, 0f), new Vector3(0.95f, 0.035f, 0.09f), threadMat, new Vector3(0f, -45f, 0f), g);
                // The thread it hangs on, from the cross up to the line (two strands, like a button's).
                float up = lineY - height[i];
                for (int s = -1; s <= 1; s += 2)
                {
                    var strand = kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.06f, up * 0.5f, 0f), new Vector3(0.035f, up * 0.5f, 0.035f), threadMat, default(Vector3), g);
                    strand.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
                g.gameObject.AddComponent<Carrier>();
                var w = g.gameObject.AddComponent<WobbleStone>();
                w.Top = mc;
                w.Phase = i * 1.7f;
                w.Radius = d * 0.5f;
                kit.Thread(top + Vector3.up * 1.5f, cols[(i + 2) % cols.Length]);
            }
            kit.Hint(new Vector3(RavineEast + 3.5f, 1.5f, StonesZ), new Vector3(5f, 3f, 6f), "Button Stones", "They dip and wobble when you land. Keep hopping!");
            kit.Hint(new Vector3(RavineWest - 3.5f, 1.5f, StonesZ), new Vector3(5f, 3f, 6f), "Button Stones", "They dip and wobble when you land. Keep hopping!");
        }

        // ---------------------------------------------------------------- Tape-measure bridge

        static void TapeMeasureBridge(LevelKit kit)
        {
            var yellow = PatchworkAssets.Plain(new Color(1f, 0.82f, 0.25f), 0.7f);
            var ink = PatchworkAssets.Plain(new Color(0.2f, 0.16f, 0.14f), 0.3f);
            var steel = PatchworkAssets.Plain(new Color(0.8f, 0.82f, 0.86f), 0.85f);
            var red = PatchworkAssets.Plain(new Color(0.92f, 0.25f, 0.25f), 0.8f);
            Vector3 anchor = new Vector3(RavineEast + 0.4f, 0f, TapeZ);
            float max = (RavineEast - RavineWest) + 0.8f;
            const float width = 2.2f;

            var g = kit.Group("TapeBridge", anchor);
            // The case: a fat round tape measure standing on the east bank beside the path, with a
            // steel lip on the ground that the tape slides out from under.
            var body = kit.Group("TapeCase", anchor + new Vector3(1.3f, 0f, 2.9f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.5f, 0f), new Vector3(3f, 0.75f, 3f), PatchworkAssets.Plain(new Color(0.95f, 0.4f, 0.3f), 0.75f), new Vector3(90f, 0f, 0f), body);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.5f, -0.76f), new Vector3(1.9f, 0.02f, 1.9f), PatchworkAssets.Plain(new Color(0.99f, 0.95f, 0.86f), 0.5f), new Vector3(90f, 0f, 0f), body);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.5f, -0.8f), new Vector3(0.5f, 0.03f, 0.5f), steel, new Vector3(90f, 0f, 0f), body);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.2f, 0f), new Vector3(2.6f, 0.4f, 1.5f), PatchworkAssets.Plain(new Color(0.95f, 0.4f, 0.3f), 0.75f), default(Vector3), body);
            var bc = body.gameObject.AddComponent<BoxCollider>();
            bc.center = new Vector3(0f, 1.5f, 0f);
            bc.size = new Vector3(2.9f, 3f, 1.5f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.5f, 0.07f, 1.05f), new Vector3(1.8f, 0.06f, 4.6f), steel, default(Vector3), g);

            // The tape itself (stretched out by the bridge as it runs).
            var tape = kit.Block(anchor, new Vector3(1f, 0.16f, width), Color.white, "Tape", yellow);
            var hook = kit.Deco(PrimitiveType.Cube, anchor, new Vector3(0.12f, 0.34f, width + 0.2f), steel);
            hook.name = "TapeHook";
            // Inch marks: a long one every 2 m, short ones between.
            int n = Mathf.FloorToInt(max / 0.5f);
            var ticks = new Renderer[n];
            var at = new float[n];
            var marks = kit.Group("TapeMarks", Vector3.zero);
            for (int i = 0; i < n; i++)
            {
                at[i] = (i + 1) * 0.5f;
                bool big = (i + 1) % 4 == 0;
                var t = kit.Deco(PrimitiveType.Cube, anchor + new Vector3(-at[i], 0.058f, big ? 0f : width * 0.25f), new Vector3(0.06f, 0.012f, big ? width * 0.9f : width * 0.4f), ink, default(Vector3), marks);
                ticks[i] = t.GetComponent<Renderer>();
                ticks[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            // A red stomp button at each end.
            Vector3[] plates = { anchor + new Vector3(2.9f, 0f, 0f), new Vector3(RavineWest - 2.4f, 0f, TapeZ) };
            var tops = new Transform[plates.Length];
            for (int i = 0; i < plates.Length; i++)
            {
                var pg = kit.Group("StompButton", plates[i]);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.02f, 0f), new Vector3(2.1f, 0.03f, 2.1f), steel, default(Vector3), pg);
                tops[i] = kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.05f, 0f), new Vector3(1.7f, 0.05f, 1.7f), red, default(Vector3), pg);
            }

            var bridge = g.gameObject.AddComponent<TapeBridge>();
            bridge.Tape = tape;
            bridge.Hook = hook;
            bridge.Ticks = ticks;
            bridge.TickAt = at;
            bridge.Anchor = anchor;
            bridge.MaxLength = max;
            bridge.Width = width;
            bridge.Plates = plates;
            bridge.PlateTops = tops;

            kit.ThreadLine(new Vector3(RavineEast - 2f, 1.1f, TapeZ), new Vector3(RavineWest + 2f, 1.1f, TapeZ), 5);
            foreach (var p in plates)
                kit.Hint(p + Vector3.up * 1.5f, new Vector3(5f, 3f, 6f), "Tape-Measure Bridge", "Stomp the red button, then run! The tape snaps back after a few seconds.");
        }

        // ---------------------------------------------------------------- The nursery's mobile

        // A crib mobile hanging from the nursery ceiling over the toybox: two crossed arms turning
        // very slowly, with a felt star, moon, cloud, heart, button and paper plane on threads.
        static void NurseryMobile(LevelKit kit)
        {
            const float hubY = 160f, ceiling = 324f, arm = 86f;   // high enough to stay out of the map picture
            var wood = PatchworkAssets.Wood(new Color(0.9f, 0.76f, 0.55f));
            var thread = PatchworkAssets.Yarn(new Color(0.97f, 0.94f, 0.88f));
            var hub = kit.Group("NurseryMobile", new Vector3(18f, hubY, 12f));
            Far(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, (ceiling - hubY) * 0.5f, 0f), new Vector3(0.5f, (ceiling - hubY) * 0.5f, 0.5f), thread, default(Vector3), hub));
            Far(kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 7f, PatchworkAssets.Plain(new Color(0.93f, 0.36f, 0.4f), 0.8f), default(Vector3), hub));
            for (int i = 0; i < 2; i++)
                Far(kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.6f, arm, 1.6f), wood, new Vector3(0f, i * 90f, 90f), hub));
            var spin = hub.gameObject.AddComponent<Spinner>();
            spin.Speed = 1.6f;

            float[] drop = { 44f, 58f, 50f, 64f, 40f, 54f };
            float[] reach = { 1f, 1f, 1f, 1f, 0.5f, 0.5f };
            for (int i = 0; i < 6; i++)
            {
                float a = (i < 4 ? i * 90f : 45f + (i - 4) * 180f) * Mathf.Deg2Rad;
                Vector3 tip = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * arm * reach[i] * (i < 4 ? 1f : 0.9f);
                if (i >= 4)
                    Far(kit.Deco(PrimitiveType.Cylinder, tip * 0.5f, new Vector3(1.2f, tip.magnitude * 0.5f, 1.2f), wood, new Vector3(0f, -a * Mathf.Rad2Deg, 90f), hub));
                Far(kit.Deco(PrimitiveType.Cylinder, tip + Vector3.down * drop[i] * 0.5f, new Vector3(0.3f, drop[i] * 0.5f, 0.3f), thread, default(Vector3), hub));
                var toy = kit.Group("MobileToy", Vector3.zero);
                toy.SetParent(hub, false);
                toy.localPosition = tip + Vector3.down * (drop[i] + 8f);
                MobileToy(kit, toy, i);
                var twirl = toy.gameObject.AddComponent<Spinner>();
                twirl.Speed = (i % 2 == 0 ? 7f : -5f);
                twirl.BobHeight = 1.2f;
                twirl.BobSpeed = 0.5f + i * 0.07f;
            }
        }

        static Transform Far(Transform t)
        {
            var r = t.GetComponent<Renderer>();
            t.gameObject.layer = 1;   // never distance-culled: it's always a long way off
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return t;
        }

        static void MobileToy(LevelKit kit, Transform g, int kind)
        {
            var stitch = PatchworkAssets.Yarn(new Color(0.99f, 0.96f, 0.9f));
            switch (kind)
            {
                case 0:   // a felt star: five fat points round a middle
                {
                    var m = PatchworkAssets.Felt(new Color(1f, 0.82f, 0.32f));
                    Far(kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(11f, 11f, 4f), m, default(Vector3), g));
                    for (int i = 0; i < 5; i++)
                    {
                        float a = i * 72f * Mathf.Deg2Rad;
                        Far(kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Sin(a), Mathf.Cos(a), 0f) * 7.5f, new Vector3(5.5f, 12f, 3.4f), m, new Vector3(0f, 0f, -i * 72f), g));
                    }
                    Face(kit, g, 2.2f);
                    break;
                }
                case 1:   // a sleepy moon
                {
                    Far(kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(20f, 20f, 5f), PatchworkAssets.Felt(new Color(0.98f, 0.93f, 0.7f)), default(Vector3), g));
                    Far(kit.Deco(PrimitiveType.Sphere, new Vector3(4f, 5f, 2f), new Vector3(4f, 4f, 1.4f), PatchworkAssets.Felt(new Color(0.92f, 0.85f, 0.6f)), default(Vector3), g));
                    Far(kit.Deco(PrimitiveType.Sphere, new Vector3(-6f, -3f, 2f), new Vector3(3f, 3f, 1.2f), PatchworkAssets.Felt(new Color(0.92f, 0.85f, 0.6f)), default(Vector3), g));
                    Face(kit, g, 2.7f);
                    break;
                }
                case 2:   // a cloud
                {
                    var m = PatchworkAssets.Felt(new Color(0.99f, 0.97f, 0.95f));
                    Far(kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(22f, 11f, 6f), m, default(Vector3), g));
                    Far(kit.Deco(PrimitiveType.Sphere, new Vector3(-5f, 5f, 0f), new Vector3(11f, 10f, 5.5f), m, default(Vector3), g));
                    Far(kit.Deco(PrimitiveType.Sphere, new Vector3(4.5f, 6f, 0f), new Vector3(13f, 12f, 5.8f), m, default(Vector3), g));
                    break;
                }
                case 3:   // a heart
                {
                    var m = PatchworkAssets.Felt(new Color(0.95f, 0.45f, 0.55f));
                    Far(kit.Deco(PrimitiveType.Sphere, new Vector3(-4.2f, 3f, 0f), new Vector3(10.5f, 10.5f, 4.5f), m, default(Vector3), g));
                    Far(kit.Deco(PrimitiveType.Sphere, new Vector3(4.2f, 3f, 0f), new Vector3(10.5f, 10.5f, 4.5f), m, default(Vector3), g));
                    Far(kit.Deco(PrimitiveType.Cube, new Vector3(0f, -2.2f, 0f), new Vector3(10.2f, 10.2f, 3.6f), m, new Vector3(0f, 0f, 45f), g));
                    break;
                }
                case 4:   // a big sewing button
                {
                    Far(kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(18f, 1.4f, 18f), PatchworkAssets.Plain(new Color(0.4f, 0.72f, 0.9f), 0.8f), new Vector3(90f, 0f, 0f), g));
                    Far(kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(13f, 1.6f, 13f), PatchworkAssets.Plain(new Color(0.5f, 0.8f, 0.95f), 0.8f), new Vector3(90f, 0f, 0f), g));
                    for (int i = 0; i < 4; i++)
                        Far(kit.Deco(PrimitiveType.Cylinder, new Vector3(i % 2 == 0 ? -2.4f : 2.4f, i < 2 ? -2.4f : 2.4f, 0f), new Vector3(2.4f, 1.7f, 2.4f), PatchworkAssets.Plain(new Color(0.2f, 0.3f, 0.45f), 0.3f), new Vector3(90f, 0f, 0f), g));
                    Far(kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(7.4f, 0.9f, 3.6f), stitch, new Vector3(0f, 0f, 45f), g));
                    Far(kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(7.4f, 0.9f, 3.7f), stitch, new Vector3(0f, 0f, -45f), g));
                    break;
                }
                default:  // a paper plane
                {
                    var m = PatchworkAssets.Plain(new Color(0.99f, 0.97f, 0.92f), 0.3f);
                    var body = Far(kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(22f, 3.4f, 0.5f), m, new Vector3(0f, 0f, 8f), g));
                    Far(kit.Deco(PrimitiveType.Cube, new Vector3(-1.5f, 1.4f, 3.6f), new Vector3(19f, 0.4f, 7.5f), m, new Vector3(-14f, 0f, 8f), g));
                    Far(kit.Deco(PrimitiveType.Cube, new Vector3(-1.5f, 1.4f, -3.6f), new Vector3(19f, 0.4f, 7.5f), m, new Vector3(14f, 0f, 8f), g));
                    Far(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.2f, 0f), new Vector3(21f, 0.5f, 0.7f), PatchworkAssets.Plain(new Color(0.93f, 0.36f, 0.4f), 0.4f), new Vector3(0f, 0f, 8f), g));
                    break;
                }
            }
        }

        // Two button eyes and a stitched smile, on both sides.
        static void Face(LevelKit kit, Transform g, float z)
        {
            var dark = PatchworkAssets.Plain(new Color(0.22f, 0.16f, 0.2f), 0.7f);
            for (int side = -1; side <= 1; side += 2)
            {
                Far(kit.Deco(PrimitiveType.Sphere, new Vector3(-2.6f, 1.4f, side * z), Vector3.one * 1.6f, dark, default(Vector3), g));
                Far(kit.Deco(PrimitiveType.Sphere, new Vector3(2.6f, 1.4f, side * z), Vector3.one * 1.6f, dark, default(Vector3), g));
                for (int i = -2; i <= 2; i++)
                    Far(kit.Deco(PrimitiveType.Cube, new Vector3(i * 1.1f, -2.4f + i * i * 0.28f, side * z), new Vector3(0.8f, 0.34f, 0.3f), dark, new Vector3(0f, 0f, i * 14f), g));
            }
        }
    }
}

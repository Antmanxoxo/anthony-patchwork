using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Spindle Hill: a real hill now, not a pair of boxes. A tall, rounded mound of knitted grass
    // in the west of town, too steep to walk up (its sides are marked NoClimb, so Patch slides
    // back down them), with an autumn knit wood all over it: pompom trees in russet, mustard and
    // plum, warm little pines, ferns, toadstools, fallen felt leaves and half-buried cotton reels.
    // The way up is a climb round its east face, in this order:
    //   - stumps stepping up out of the meadow
    //   - a plank ledge, and a pincushion that bounces you up to a branch
    //   - more branches, then a lantern hanging off a giant knitting needle stuck in the hill,
    //     to swing across the gap (or a good jump)
    //   - giant sewing buttons sewn into the hillside, up onto the summit
    // The summit is flat: the old spinning wheel, the lute string, Captain Buttonbeard and his
    // flag are all up there (everything that used to be on the old box summit is moved up with
    // OnHill).
    public static partial class Level1Builder
    {
        public static readonly Vector3 HillCentre = new Vector3(-73f, 0f, 33f);
        public const float HillTop = 16f;
        const float HillRX = 20f, HillRZ = 22f;
        // Where the old box summit was (the middle of its top), so the things that stood on it
        // can be moved up onto the new one by the same amount.
        static readonly Vector3 OldSummit = new Vector3(-67f, 4f, 32f);

        // A spot that was on the old summit, moved to the same place on the new one.
        public static Vector3 OnHill(Vector3 old)
        {
            return old + (new Vector3(HillCentre.x, HillTop, HillCentre.z + 1f) - OldSummit);
        }

        // The hill's shape, from the middle (0) to its foot (1): a flat top, a rounded shoulder,
        // a steep middle and a gentle foot.
        static readonly float[] ProfileS = { 0f, 0.33f, 0.40f, 0.47f, 0.60f, 0.72f, 0.86f, 1.0f };
        static readonly float[] ProfileH = { HillTop, HillTop, 15.2f, 12.5f, 5.2f, 2.8f, 1.0f, -0.05f };
        static float[] profileM;

        static float Profile(float s)
        {
            var S = ProfileS;
            var H = ProfileH;
            int n = S.Length;
            if (profileM == null)
            {
                // Smooth, with no overshoot (monotone cubic), and flat across the top.
                var d = new float[n - 1];
                for (int i = 0; i < n - 1; i++) d[i] = (H[i + 1] - H[i]) / (S[i + 1] - S[i]);
                profileM = new float[n];
                profileM[n - 1] = d[n - 2];
                for (int i = 1; i < n - 1; i++)
                {
                    if (d[i - 1] * d[i] <= 0f) { profileM[i] = 0f; continue; }
                    float w1 = 2f * (S[i + 1] - S[i]) + (S[i] - S[i - 1]);
                    float w2 = (S[i + 1] - S[i]) + 2f * (S[i] - S[i - 1]);
                    profileM[i] = (w1 + w2) / (w1 / d[i - 1] + w2 / d[i]);
                }
                profileM[0] = 0f;
                profileM[1] = 0f;
            }
            if (s <= S[0]) return H[0];
            if (s >= S[n - 1]) return H[n - 1];
            for (int i = 0; i < n - 1; i++)
            {
                if (s > S[i + 1]) continue;
                float h = S[i + 1] - S[i], t = (s - S[i]) / h;
                float t2 = t * t, t3 = t2 * t;
                return (2f * t3 - 3f * t2 + 1f) * H[i] + (t3 - 2f * t2 + t) * h * profileM[i]
                     + (-2f * t3 + 3f * t2) * H[i + 1] + (t3 - t2) * h * profileM[i + 1];
            }
            return H[n - 1];
        }

        // A gently uneven outline, and shoulders a little higher on some sides than others.
        static float Wobble(float a)
        {
            return 1f + 0.045f * Mathf.Sin(3f * a + 0.7f) + 0.025f * Mathf.Sin(7f * a + 2.1f);
        }

        static float HeightAt(float a, float s)
        {
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((s - 0.33f) / 0.17f));
            return Profile(s) * (1f + k * (0.05f * Mathf.Sin(2f * a + 1.3f) + 0.03f * Mathf.Sin(5f * a + 0.4f)));
        }

        // The ground's height on the hill at (x, z) (0 off it).
        public static float HillHeight(float x, float z)
        {
            float dx = (x - HillCentre.x) / HillRX, dz = (z - HillCentre.z) / HillRZ;
            float a = Mathf.Atan2(dz, dx);
            float s = Mathf.Sqrt(dx * dx + dz * dz) / Wobble(a);
            if (s >= 1f) return 0f;
            return Mathf.Max(0f, HeightAt(a, s));
        }

        static Vector3 HillPoint(float a, float s)
        {
            float w = Wobble(a);
            float x = HillCentre.x + Mathf.Cos(a) * HillRX * s * w, z = HillCentre.z + Mathf.Sin(a) * HillRZ * s * w;
            return new Vector3(x, HeightAt(a, s), z);
        }

        // How far out (0..1) the hill is h high, on the side at angle a.
        static float HillSAt(float a, float h)
        {
            float lo = 0.33f, hi = 1f;
            for (int i = 0; i < 40; i++)
            {
                float mid = (lo + hi) * 0.5f;
                if (HeightAt(a, mid) > h) lo = mid; else hi = mid;
            }
            return (lo + hi) * 0.5f;
        }

        // Which way is uphill-to-downhill (outward, flat) at angle a.
        static Vector3 HillOutward(float a)
        {
            Vector3 o = new Vector3(Mathf.Cos(a) / HillRX, 0f, Mathf.Sin(a) / HillRZ);
            return o.normalized;
        }

        static Vector3 HillNormal(float x, float z)
        {
            const float e = 0.3f;
            float hx = HillHeight(x + e, z) - HillHeight(x - e, z), hz = HillHeight(x, z + e) - HillHeight(x, z - e);
            return new Vector3(-hx, 2f * e, -hz).normalized;
        }

        // ---------------------------------------------------------------- Building it

        struct HillStep { public Vector3 Top; public float Width; }
        static readonly List<HillStep> hillRoute = new List<HillStep>();

        static void BuildSpindleHill(LevelKit kit, FrayRegion region)
        {
            hillRoute.Clear();
            var root = kit.Group("SpindleHill", Vector3.zero);
            HillMesh(kit, root);
            HillClimb(kit, root);
            HillWood(kit, root);
            HillCamp(kit, root);
        }

        // Captain Buttonbeard's camp on the summit: he's been up here a while.
        static void HillCamp(LevelKit kit, Transform root)
        {
            Vector3 c = new Vector3(HillCentre.x - 1f, HillTop, HillCentre.z - 5.5f);
            ClearGrass(c, 2f);
            var g = kit.Group("PirateCamp", c);
            g.SetParent(root, true);
            g.rotation = Quaternion.Euler(0f, 20f, 0f);
            // A rolled-up bedroll with red ties.
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.22f, 0f), new Vector3(0.45f, 0.75f, 0.45f), PatchworkAssets.Knit(new Color(0.3f, 0.42f, 0.62f)), new Vector3(0f, 0f, 90f), g);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.4f, 0.22f, 0f), new Vector3(0.47f, 0.04f, 0.47f), PatchworkAssets.Felt(new Color(0.85f, 0.25f, 0.3f)), new Vector3(0f, 0f, 90f), g);
            // A little lantern, a mug, and a treasure map held down with a pebble.
            var lamp = kit.Group("Lantern", c + g.rotation * new Vector3(1.2f, 0f, 0.4f));
            lamp.SetParent(g, true);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.04f, 0f), new Vector3(0.28f, 0.04f, 0.28f), PatchworkAssets.Plain(new Color(0.3f, 0.3f, 0.34f), 0.6f), default(Vector3), lamp);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.22f, 0f), new Vector3(0.2f, 0.15f, 0.2f), PatchworkAssets.Glow(new Color(1f, 0.8f, 0.45f), 1.1f), default(Vector3), lamp);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.4f, 0f), new Vector3(0.26f, 0.03f, 0.26f), PatchworkAssets.Plain(new Color(0.3f, 0.3f, 0.34f), 0.6f), default(Vector3), lamp);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-1.1f, 0.1f, 0.5f), new Vector3(0.18f, 0.1f, 0.18f), PatchworkAssets.Felt(new Color(0.95f, 0.92f, 0.85f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.2f, 0.015f, 0.85f), new Vector3(0.7f, 0.02f, 0.5f), PatchworkAssets.Felt(new Color(0.93f, 0.85f, 0.65f)), new Vector3(0f, 25f, 0f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.25f, 0.03f, 0.88f), new Vector3(0.06f, 0.005f, 0.06f), PatchworkAssets.Felt(new Color(0.85f, 0.2f, 0.25f)), new Vector3(0f, 45f, 0f), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(-0.1f, 0.06f, 0.7f), new Vector3(0.16f, 0.1f, 0.14f), PatchworkAssets.Felt(new Color(0.6f, 0.58f, 0.55f)), default(Vector3), g);
        }

        static void HillMesh(LevelKit kit, Transform root)
        {
            const int segs = 112;
            var grass = PatchworkAssets.Grass(GrassCol);
            // The flat top: a disc, mapped flat like the ground.
            {
                var verts = new List<Vector3>();
                var uvs = new List<Vector2>();
                var tris = new List<int>();
                float[] rings = { 0f, 0.11f, 0.22f, 0.33f };
                verts.Add(HillPoint(0f, 0f));
                uvs.Add(new Vector2(HillCentre.x, HillCentre.z) / 2.5f);
                for (int r = 1; r < rings.Length; r++)
                    for (int i = 0; i < segs; i++)
                    {
                        var p = HillPoint(i * Mathf.PI * 2f / segs, rings[r]);
                        verts.Add(p);
                        uvs.Add(new Vector2(p.x, p.z) / 2.5f);
                    }
                for (int i = 0; i < segs; i++) { tris.Add(0); tris.Add(1 + (i + 1) % segs); tris.Add(1 + i); }
                for (int r = 1; r < rings.Length - 1; r++)
                {
                    int a0 = 1 + (r - 1) * segs, b0 = 1 + r * segs;
                    for (int i = 0; i < segs; i++)
                    {
                        int j = (i + 1) % segs;
                        tris.Add(a0 + i); tris.Add(a0 + j); tris.Add(b0 + i);
                        tris.Add(a0 + j); tris.Add(b0 + j); tris.Add(b0 + i);
                    }
                }
                HillPiece(root, "HillTop", verts, uvs, tris, grass, false);
            }
            // The sides, from the edge of the top down to just under the meadow.
            {
                var verts = new List<Vector3>();
                var uvs = new List<Vector2>();
                var tris = new List<int>();
                var rings = new List<float>();
                for (int r = 0; r <= 40; r++) rings.Add(0.33f + (1.03f - 0.33f) * r / 40f);
                const float around = 32f;   // times the pattern goes round the hill
                for (int i = 0; i <= segs; i++)
                {
                    float a = i * Mathf.PI * 2f / segs;
                    float v = 0f;
                    Vector3 prev = Vector3.zero;
                    for (int r = 0; r < rings.Count; r++)
                    {
                        float s = rings[r];
                        Vector3 p = s <= 1f ? HillPoint(a, s) : new Vector3(HillPoint(a, s).x, -0.4f, HillPoint(a, s).z);
                        if (r > 0) v += Vector3.Distance(prev, p);
                        prev = p;
                        verts.Add(p);
                        uvs.Add(new Vector2(i / (float)segs * around, v / 2.5f));
                    }
                }
                int n = rings.Count;
                for (int i = 0; i < segs; i++)
                    for (int r = 0; r < n - 1; r++)
                    {
                        int a0 = i * n + r, b0 = (i + 1) * n + r;
                        tris.Add(a0); tris.Add(b0); tris.Add(a0 + 1);
                        tris.Add(b0); tris.Add(b0 + 1); tris.Add(a0 + 1);
                    }
                HillPiece(root, "HillSides", verts, uvs, tris, grass, true);
            }
        }

        static void HillPiece(Transform root, string name, List<Vector3> verts, List<Vector2> uvs, List<int> tris, Material mat, bool steep)
        {
            var mesh = new Mesh();
            mesh.name = name;
            if (verts.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            // (Wound so the outside faces up and out; flip if Unity's normals point in.)
            mesh.RecalculateNormals();
            if (mesh.normals.Length > 0 && mesh.normals[0].y < 0f)
            {
                for (int i = 0; i < tris.Count; i += 3) { int t = tris[i]; tris[i] = tris[i + 1]; tris[i + 1] = t; }
                mesh.SetTriangles(tris, 0);
                mesh.RecalculateNormals();
            }
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            go.AddComponent<MeshCollider>().sharedMesh = mesh;
            go.AddComponent<OwnedMesh>().Mesh = mesh;
            if (steep) go.AddComponent<NoClimb>();
        }

        // ---------------------------------------------------------------- The climb

        enum StepKind { Stump, Plank, Button }

        static void HillClimb(LevelKit kit, Transform root)
        {
            // (angle round the hill in degrees, top height, width, kind)
            var stump = StepKind.Stump; var plank = StepKind.Plank; var button = StepKind.Button;
            Step(kit, root, -100f, 4.2f, 2.4f, stump);
            Step(kit, root, -82f, 5.4f, 2.4f, stump);
            Step(kit, root, -64f, 6.6f, 2.4f, stump);
            Step(kit, root, -46f, 7.8f, 4.4f, plank);
            var padStep = Step(kit, root, -26f, 7.8f, 3.2f, plank);
            kit.AddBouncePad(padStep, 17f);
            Step(kit, root, -10f, 11.6f, 3f, plank);
            var swingFrom = Step(kit, root, 7f, 12.6f, 3f, plank);
            var swingTo = Step(kit, root, 40f, 13.2f, 3.4f, plank);
            Step(kit, root, 58f, 14.3f, 2.4f, button);
            Step(kit, root, 75f, 15.4f, 2.4f, button);

            // The lantern on the end of a giant knitting needle stuck into the hill, out over the
            // gap between the two branches.
            float midA = 23.5f * Mathf.Deg2Rad;
            Vector3 lantern = (swingFrom + swingTo) * 0.5f + HillOutward(midA) * 1.6f;
            lantern.y = Mathf.Max(swingFrom.y, swingTo.y) + 4.6f;
            Vector3 tip = lantern + Vector3.up * 1.7f + HillOutward(midA) * 0.2f;
            Vector3 stuck = HillPoint(midA, HillSAt(midA, 15.6f)) + Vector3.down * 0.6f;
            kit.KnittingNeedle(stuck, tip, LevelKit.Palette.Rose);
            kit.Anchor(lantern, 9.5f, LevelKit.AnchorStyle.Lantern, tip.y - 0.35f - lantern.y);

            // Bubbles to show the way, hopping from step to step.
            ScoreBubbles.BeginSet(kit);
            for (int i = 0; i + 1 < hillRoute.Count; i++)
            {
                Vector3 a = hillRoute[i].Top, b = hillRoute[i + 1].Top;
                Vector3 mid = (a + b) * 0.5f + Vector3.up * (1.3f + Mathf.Abs(b.y - a.y) * 0.4f);
                if (i == 6) mid = lantern + Vector3.down * 2.6f;   // under the swing
                ScoreBubbles.Place(kit, mid, 10);
            }
            ScoreBubbles.EndSet();

            // Getting started, and a camera that stands back while you climb.
            float startA = -100f * Mathf.Deg2Rad;
            Vector3 foot = HillPoint(startA, 1.08f);
            foot.y = 0f;
            kit.Hint(foot + Vector3.up * 1.5f, new Vector3(6f, 3f, 6f), "Climb Spindle Hill",
                "It's far too steep to walk up! Hop up the stumps and ledges round its side. The pincushion gives you a boost, and as Yarn you can swing on the lantern hanging off the big knitting needle.", HintTheme.Yarn);
            kit.AddCheckpoint(foot, Mathf.Atan2(-HillOutward(startA).x, -HillOutward(startA).z) * Mathf.Rad2Deg);
            kit.AddCameraZone(new Vector3(-65f, 9f, 29f), new Vector3(20f, 22f, 30f), 2.5f, 5f);
            HillSignpost(kit, foot + HillOutward(startA) * 1.4f + Vector3.right * 1.6f);
        }

        // One step on the way up, sticking out of the hillside. Returns the middle of its top.
        static Vector3 Step(LevelKit kit, Transform root, float angleDeg, float top, float width, StepKind kind)
        {
            float a = angleDeg * Mathf.Deg2Rad;
            Vector3 c = HillPoint(a, HillSAt(a, top - 0.7f));
            c.y = top;
            Vector3 outv = HillOutward(a);
            float yaw = Mathf.Atan2(outv.x, outv.z) * Mathf.Rad2Deg;
            float depth = kind == StepKind.Plank ? 2.6f : width;
            // The part you stand on.
            var solid = new GameObject("HillStepSolid");
            solid.transform.SetParent(root, false);
            solid.transform.position = c + Vector3.down * 0.6f;
            solid.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            solid.AddComponent<BoxCollider>().size = new Vector3(width * (kind == StepKind.Plank ? 1f : 0.86f), 1.2f, depth * 0.86f);

            var g = kit.Group("HillStep", c);
            g.SetParent(root, true);
            g.rotation = Quaternion.Euler(0f, yaw, 0f);
            switch (kind)
            {
                case StepKind.Stump:
                {
                    // A tree stump: knitted bark, the rings on top, a root or two into the hill.
                    var bark = PatchworkAssets.Knit(new Color(0.45f, 0.33f, 0.25f));
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -1.6f, 0f), new Vector3(width, 1.6f, width), bark, default(Vector3), g);
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.01f, 0f), new Vector3(width * 0.9f, 0.02f, width * 0.9f), PatchworkAssets.Wood(new Color(0.86f, 0.7f, 0.5f)), default(Vector3), g);
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.025f, 0f), new Vector3(width * 0.55f, 0.02f, width * 0.55f), PatchworkAssets.Wood(new Color(0.72f, 0.55f, 0.38f)), default(Vector3), g);
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.04f, 0f), new Vector3(width * 0.2f, 0.02f, width * 0.2f), PatchworkAssets.Wood(new Color(0.6f, 0.44f, 0.3f)), default(Vector3), g);
                    for (int k = 0; k < 2; k++)
                        Rod(kit, new Vector3((k - 0.5f) * width * 0.6f, -1.2f, 0f), new Vector3((k - 0.5f) * width * 0.9f, -2.2f, -width * 0.7f), bark, 0.3f, g);
                    if (Mathf.RoundToInt(top) % 2 == 0)
                        Mushroom(kit, c + g.rotation * new Vector3(width * 0.42f, -0.05f, width * 0.15f), 0.28f, new Color(0.92f, 0.45f, 0.2f), false);
                    break;
                }
                case StepKind.Plank:
                {
                    // A wooden shelf on two brackets, like a branch sawn flat.
                    var planks = PatchworkAssets.Planks(new Color(0.72f, 0.52f, 0.34f));
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.18f, 0f), new Vector3(width, 0.36f, depth), planks, default(Vector3), g);
                    var dark = PatchworkAssets.Wood(new Color(0.42f, 0.3f, 0.22f));
                    for (int s = -1; s <= 1; s += 2)
                        Rod(kit, new Vector3(s * width * 0.32f, -0.3f, depth * 0.35f), new Vector3(s * width * 0.32f, -2.4f, -depth * 0.55f), dark, 0.18f, g);
                    // A trim of felt moss along the front edge.
                    kit.Deco(PrimitiveType.Capsule, new Vector3(0f, -0.02f, depth * 0.48f), new Vector3(0.18f, width * 0.45f, 0.14f), PatchworkAssets.Felt(new Color(0.45f, 0.6f, 0.36f)), new Vector3(0f, 0f, 90f), g);
                    break;
                }
                default:
                {
                    // A giant sewing button, sewn into the hillside with thick thread.
                    Color[] cols = { new Color(0.9f, 0.55f, 0.25f), new Color(0.55f, 0.32f, 0.5f), new Color(0.95f, 0.78f, 0.35f) };
                    Color col = cols[Mathf.Abs(Mathf.RoundToInt(top * 3f)) % cols.Length];
                    var face = kit.Group("ButtonStep", c + Vector3.down * 0.2f);
                    face.SetParent(g, true);
                    face.localRotation = Quaternion.identity;
                    kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(width, 0.2f, width), PatchworkAssets.Felt(col), default(Vector3), face);
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.05f, 0f), new Vector3(width * 0.8f, 0.17f, width * 0.8f), PatchworkAssets.Felt(Color.Lerp(col, Color.white, 0.18f)), default(Vector3), face);
                    var hole = PatchworkAssets.Plain(Color.Lerp(col, Color.black, 0.55f), 0.2f);
                    var thread = PatchworkAssets.Yarn(new Color(0.95f, 0.9f, 0.8f));
                    for (int k = 0; k < 4; k++)
                    {
                        float hx = (k % 2 == 0 ? -1f : 1f) * width * 0.13f, hz = (k < 2 ? -1f : 1f) * width * 0.13f;
                        kit.Deco(PrimitiveType.Cylinder, new Vector3(hx, 0.22f, hz), new Vector3(width * 0.1f, 0.02f, width * 0.1f), hole, default(Vector3), face);
                    }
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.23f, 0f), new Vector3(width * 0.4f, 0.04f, 0.08f), thread, new Vector3(0f, 45f, 0f), face);
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.23f, 0f), new Vector3(width * 0.4f, 0.04f, 0.08f), thread, new Vector3(0f, -45f, 0f), face);
                    // Its shank, going into the hill.
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -1.1f, -width * 0.2f), new Vector3(width * 0.35f, 1f, width * 0.35f), PatchworkAssets.Felt(Color.Lerp(col, Color.black, 0.3f)), default(Vector3), g);
                    break;
                }
            }
            hillRoute.Add(new HillStep { Top = c, Width = width });
            return c;
        }

        // A little wooden sign at the bottom of the climb, pointing up.
        static void HillSignpost(LevelKit kit, Vector3 feet)
        {
            var g = kit.Group("Signpost", feet);
            var wood = PatchworkAssets.Wood(new Color(0.6f, 0.42f, 0.28f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.8f, 0f), new Vector3(0.12f, 0.8f, 0.12f), wood, default(Vector3), g);
            var board = kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.35f, 0f), new Vector3(1.1f, 0.32f, 0.06f), PatchworkAssets.Planks(new Color(0.85f, 0.68f, 0.45f)), new Vector3(0f, 0f, 14f), g);
            // An arrow, pointing up the hill.
            kit.Deco(PrimitiveType.Cube, new Vector3(0.1f, 1.38f, -0.04f), new Vector3(0.6f, 0.06f, 0.01f), PatchworkAssets.Felt(new Color(0.85f, 0.3f, 0.32f)), new Vector3(0f, 0f, 14f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.38f, 1.47f, -0.04f), new Vector3(0.2f, 0.06f, 0.01f), PatchworkAssets.Felt(new Color(0.85f, 0.3f, 0.32f)), new Vector3(0f, 0f, 59f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.38f, 1.31f, -0.04f), new Vector3(0.2f, 0.06f, 0.01f), PatchworkAssets.Felt(new Color(0.85f, 0.3f, 0.32f)), new Vector3(0f, 0f, -31f), g);
            g.rotation = Quaternion.Euler(0f, 160f, 0f);
            board.name = "Board";
        }

        // ---------------------------------------------------------------- The hill wood

        static readonly Color[] AutumnLeaves =
        {
            new Color(0.86f, 0.45f, 0.26f), new Color(0.93f, 0.72f, 0.3f), new Color(0.6f, 0.36f, 0.55f),
            new Color(0.62f, 0.66f, 0.34f), new Color(0.8f, 0.3f, 0.28f)
        };

        // Not in the way of the climb, the summit's goings-on, or the meadow under it.
        static bool HillClear(Vector3 p, float r)
        {
            for (int i = 0; i < hillRoute.Count; i++)
            {
                Vector3 a = hillRoute[i].Top;
                if (Flat(p - a) < 3.6f + r) return false;
                if (i + 1 < hillRoute.Count && SegDist(p, a, hillRoute[i + 1].Top) < 3f + r) return false;
            }
            return true;
        }

        static float Flat(Vector3 d) { return new Vector2(d.x, d.z).magnitude; }

        static float SegDist(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector2 P = new Vector2(p.x, p.z), A = new Vector2(a.x, a.z), B = new Vector2(b.x, b.z);
            Vector2 ab = B - A;
            float t = Mathf.Clamp01(Vector2.Dot(P - A, ab) / Mathf.Max(0.001f, ab.sqrMagnitude));
            return (P - (A + ab * t)).magnitude;
        }

        static void HillWood(LevelKit kit, Transform root)
        {
            var rng = new System.Random(707);
            System.Func<float> rnd = () => (float)rng.NextDouble();
            var trees = new List<Vector3>();
            int tries = 0;
            // Trees all over the slopes and round the back of the foot.
            while (trees.Count < 56 && tries++ < 3000)
            {
                float a = rnd() * Mathf.PI * 2f;
                float s = Mathf.Lerp(0.36f, 1.12f, Mathf.Sqrt(rnd()));
                Vector3 p = HillPoint(a, Mathf.Min(s, 0.999f));
                if (s >= 1f)
                {
                    float w = Wobble(a);
                    p = new Vector3(HillCentre.x + Mathf.Cos(a) * HillRX * s * w, 0f, HillCentre.z + Mathf.Sin(a) * HillRZ * s * w);
                    // (Only round the quiet back of the hill, away from town and the meadow.)
                    if (p.x > -60f || p.z < 18f) continue;
                }
                if (s < 0.4f) continue;   // the summit stays open
                if (p.x > -53.5f || p.x < -Level1Builder.BoxHalf + 6f || p.z > Level1Builder.BoxHalf - 6f) continue;
                if (p.z < 14f && p.x > -82f) continue;   // the meadow and the bubble rush
                if (!HillClear(p, 0.8f)) continue;
                bool close = false;
                foreach (var q in trees) if (Flat(p - q) < 3.4f) { close = true; break; }
                if (close) continue;
                trees.Add(p);
            }
            for (int i = 0; i < trees.Count; i++)
            {
                Vector3 f = trees[i];
                f.y = HillHeight(f.x, f.z) - 0.35f;
                float h = 4.6f + rnd() * 3f;
                Color leaf = AutumnLeaves[rng.Next(AutumnLeaves.Length)];
                switch (i % 4)
                {
                    case 0: case 2: PomTree(kit, f, h, leaf); break;
                    case 1: HillPine(kit, f, h + 1f, rng); break;
                    default: ButtonTree(kit, f, h, leaf, new[] { AutumnLeaves[(i + 1) % AutumnLeaves.Length], new Color(0.97f, 0.93f, 0.85f) }, rng); break;
                }
            }

            // Undergrowth: ferns, toadstools and bramble bushes, cotton reels half sunk into the
            // hillside, and the odd ball of yarn rolled to a stop at the bottom.
            var fernMat = PatchworkAssets.Felt(new Color(0.5f, 0.6f, 0.32f));
            var rustFern = PatchworkAssets.Felt(new Color(0.78f, 0.5f, 0.28f));
            for (int i = 0, made = 0; i < 400 && made < 70; i++)
            {
                float a = rnd() * Mathf.PI * 2f, s = Mathf.Lerp(0.38f, 0.98f, rnd());
                Vector3 p = HillPoint(a, s);
                if (p.x > -53.5f || !HillClear(p, 0.2f)) continue;
                bool inTree = false;
                foreach (var q in trees) if (Flat(p - q) < 1.2f) { inTree = true; break; }
                if (inTree) continue;
                p.y = HillHeight(p.x, p.z) - 0.05f;
                int pick = made % 7;
                if (pick < 3) Fern(kit, p, 0.9f + rnd() * 0.6f, made % 3 == 0 ? rustFern : fernMat, made);
                else if (pick < 5) Mushroom(kit, p, 0.25f + rnd() * 0.35f, made % 2 == 0 ? new Color(0.88f, 0.28f, 0.25f) : new Color(0.75f, 0.52f, 0.3f), false);
                else if (pick == 5 && s > 0.72f) Bramble(kit, p + Vector3.down * 0.2f, rng);
                else if (s < 0.9f)
                {
                    var reel = kit.Group("HillReel", p + Vector3.down * 0.5f);
                    reel.rotation = Quaternion.FromToRotation(Vector3.up, HillNormal(p.x, p.z)) * Quaternion.Euler(0f, rnd() * 360f, 70f);
                    var wood = PatchworkAssets.Felt(new Color(0.8f, 0.62f, 0.42f));
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.5f, 0f), new Vector3(1.4f, 0.08f, 1.4f), wood, default(Vector3), reel);
                    kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.05f, 0.45f, 1.05f), PatchworkAssets.Yarn(AutumnLeaves[made % AutumnLeaves.Length]), default(Vector3), reel);
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.5f, 0f), new Vector3(1.4f, 0.08f, 1.4f), wood, default(Vector3), reel);
                }
                made++;
            }
            kit.YarnBall(new Vector3(-88f, 0f, 46f), 1.3f, AutumnLeaves[0]);
            kit.YarnBall(new Vector3(-62f, 0f, 53f), 1.1f, AutumnLeaves[2]);

            // Fallen felt leaves, lying flat on the slopes, and tufts of grass.
            var leafBatches = new MeshBatch[AutumnLeaves.Length];
            for (int i = 0; i < leafBatches.Length; i++) leafBatches[i] = new MeshBatch();
            var tufts = new MeshBatch();
            for (int i = 0; i < 1400; i++)
            {
                float a = rnd() * Mathf.PI * 2f, s = Mathf.Lerp(0.05f, 1f, Mathf.Sqrt(rnd()));
                Vector3 p = HillPoint(a, s);
                if (p.x > -52f) continue;
                Vector3 nrm = HillNormal(p.x, p.z);
                if (i % 3 == 0)
                {
                    if (s < 0.33f && rnd() < 0.6f) continue;   // (sparser on the summit)
                    tufts.Blade(p + Vector3.down * 0.05f, 0.35f + rnd() * 0.3f, 0.06f, new Vector3(rnd() - 0.5f, 0f, rnd() - 0.5f) * 0.25f, rnd() * 360f);
                    tufts.Blade(p + Vector3.down * 0.05f, 0.3f + rnd() * 0.25f, 0.06f, new Vector3(rnd() - 0.5f, 0f, rnd() - 0.5f) * 0.25f, rnd() * 360f);
                    continue;
                }
                var rot = Quaternion.FromToRotation(Vector3.up, nrm) * Quaternion.Euler(0f, rnd() * 360f, 0f);
                leafBatches[i % leafBatches.Length].Box(p + nrm * 0.025f, new Vector3(0.34f, 0.02f, 0.22f), rot);
            }
            for (int i = 0; i < leafBatches.Length; i++) leafBatches[i].Build(root, "FallenLeaves", PatchworkAssets.Felt(AutumnLeaves[i]));
            tufts.Build(root, "HillGrass", PatchworkAssets.Felt(new Color(0.48f, 0.62f, 0.34f)));
        }

        // A warm little pine for the hill: tiers of knitted green with a russet trim.
        static void HillPine(LevelKit kit, Vector3 feet, float h, System.Random rng)
        {
            var g = kit.Group("SpookyPine", feet);
            g.rotation = Quaternion.Euler((float)(rng.NextDouble() - 0.5) * 6f, (float)rng.NextDouble() * 360f, (float)(rng.NextDouble() - 0.5) * 6f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, h * 0.3f, 0f), new Vector3(0.5f, h * 0.3f, 0.5f), PatchworkAssets.Wood(new Color(0.5f, 0.35f, 0.25f)), default(Vector3), g);
            Color[] tiers = { new Color(0.35f, 0.5f, 0.32f), new Color(0.3f, 0.45f, 0.3f), new Color(0.42f, 0.55f, 0.32f) };
            var trim = PatchworkAssets.Felt(new Color(0.85f, 0.5f, 0.28f));
            const int count = 5;
            for (int t = 0; t < count; t++)
            {
                float k = t / (float)(count - 1);
                float y = h * Mathf.Lerp(0.35f, 0.88f, k);
                float r = Mathf.Lerp(h * 0.28f, 0.35f, k);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, y, 0f), new Vector3(r * 2f, h * 0.05f, r * 2f), PatchworkAssets.Knit(tiers[t % tiers.Length]), new Vector3(0f, t * 23f, t % 2 == 0 ? 3f : -3f), g);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, y - h * 0.047f, 0f), new Vector3(r * 2f + 0.1f, 0.03f, r * 2f + 0.1f), trim, new Vector3(0f, t * 23f, t % 2 == 0 ? 3f : -3f), g);
            }
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, h * 0.96f, 0f), Vector3.one * 0.4f, PatchworkAssets.Felt(new Color(0.93f, 0.72f, 0.3f)), default(Vector3), g);
            Trunk(g, 0.32f, h);
        }
    }

    // A surface too steep to climb: standing on it, Patch slides back down (no jumping up it).
    public class NoClimb : MonoBehaviour { }
}

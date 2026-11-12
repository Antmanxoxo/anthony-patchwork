using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Six more things in every room of the museum (the rooms themselves are in Level2RoomsFront,
    // Level2RoomsMiddle and Level2RoomsKeepers).
    //
    // All of it is fitted in last, round what's already standing. Each piece asks for a patch of
    // floor (or wall, or open air, or water) near where it would like to be, and gets the
    // nearest one that's clear of everything else: solid things, and anything you can see. If
    // there's nowhere clear close enough it's left out, and the log says so, so nothing ever
    // ends up standing inside something else.
    public static partial class Level2Builder
    {
        const int RmMask = ~(1 << 2);
        const float RmCell = 4f;
        static List<Bounds> rmTaken;
        static Dictionary<long, List<int>> rmGrid;
        static int rmPlaced, rmMissed;

        static void Rooms(LevelKit kit)
        {
            rmTaken = new List<Bounds>();
            rmGrid = new Dictionary<long, List<int>>();
            rmPlaced = rmMissed = 0;
            foreach (var r in kit.Root.GetComponentsInChildren<Renderer>(true))
            {
                Vector3 s = r.bounds.size;
                int big = (s.x > 7f ? 1 : 0) + (s.y > 7f ? 1 : 0) + (s.z > 7f ? 1 : 0);
                if (big >= 2) continue;   // (walls, floors, ceilings and water: the rooms themselves)
                RmClaim(r.bounds);
            }
            Physics.SyncTransforms();

            RoomsForecourt(kit);
            RoomsAtrium(kit);
            RoomsDeepTime(kit);
            RoomsKelp(kit);
            RoomsTunnel(kit);
            RoomsRuins(kit);
            RoomsArms(kit);
            RoomsBridge(kit);
            RoomsCorridor(kit);
            RoomsObservatory(kit);
            RoomsKeepersRoom(kit);
            RoomsTankHall(kit);
            RoomsUndercroft(kit);
            RoomsCoral(kit);
            RoomsLantern(kit);
            RoomsFrozen(kit);
            RoomsMangrove(kit);

            Debug.Log("[Level2] Room details: " + rmPlaced + " placed" + (rmMissed > 0 ? ", " + rmMissed + " left out for want of room." : "."));
            rmTaken = null;
            rmGrid = null;
        }

        // ------------------------------------------------------------------ Finding room

        static long RmKey(int x, int z) { return ((long)x << 32) ^ (uint)z; }

        static void RmClaim(Bounds b)
        {
            int i = rmTaken.Count;
            rmTaken.Add(b);
            int x0 = Mathf.FloorToInt(b.min.x / RmCell), x1 = Mathf.FloorToInt(b.max.x / RmCell);
            int z0 = Mathf.FloorToInt(b.min.z / RmCell), z1 = Mathf.FloorToInt(b.max.z / RmCell);
            if ((x1 - x0 + 1) * (z1 - z0 + 1) > 400) return;   // (nothing that big gets in the way of anything small)
            for (int x = x0; x <= x1; x++)
                for (int z = z0; z <= z1; z++)
                {
                    List<int> list;
                    long k = RmKey(x, z);
                    if (!rmGrid.TryGetValue(k, out list)) rmGrid[k] = list = new List<int>();
                    list.Add(i);
                }
        }

        static bool RmClear(Bounds b)
        {
            int x0 = Mathf.FloorToInt(b.min.x / RmCell), x1 = Mathf.FloorToInt(b.max.x / RmCell);
            int z0 = Mathf.FloorToInt(b.min.z / RmCell), z1 = Mathf.FloorToInt(b.max.z / RmCell);
            for (int x = x0; x <= x1; x++)
                for (int z = z0; z <= z1; z++)
                {
                    List<int> list;
                    if (!rmGrid.TryGetValue(RmKey(x, z), out list)) continue;
                    foreach (int i in list)
                        if (rmTaken[i].Intersects(b)) return false;
                }
            return true;
        }

        // The box round a turned box.
        static Bounds RmBox(Vector3 center, Vector3 size, Quaternion rot)
        {
            Vector3 ax = rot * new Vector3(size.x * 0.5f, 0f, 0f), ay = rot * new Vector3(0f, size.y * 0.5f, 0f), az = rot * new Vector3(0f, 0f, size.z * 0.5f);
            Vector3 e = new Vector3(Mathf.Abs(ax.x) + Mathf.Abs(ay.x) + Mathf.Abs(az.x), Mathf.Abs(ax.y) + Mathf.Abs(ay.y) + Mathf.Abs(az.y), Mathf.Abs(ax.z) + Mathf.Abs(ay.z) + Mathf.Abs(az.z));
            return new Bounds(center, e * 2f);
        }

        static bool RmFits(Vector3 center, Vector3 size, Quaternion rot)
        {
            Vector3 half = new Vector3(Mathf.Max(0.01f, size.x * 0.5f - 0.02f), Mathf.Max(0.01f, size.y * 0.5f - 0.02f), Mathf.Max(0.01f, size.z * 0.5f - 0.02f));
            if (Physics.CheckBox(center, half, rot, RmMask, QueryTriggerInteraction.Ignore)) return false;
            return RmClear(RmBox(center, size, rot));
        }

        static void RmMiss(string what, Vector3 want)
        {
            rmMissed++;
            Debug.Log("[Level2] No room for " + what + " near " + want.ToString("F1") + ".");
        }

        static void RmGot(Vector3 center, Vector3 size, Quaternion rot)
        {
            rmPlaced++;
            RmClaim(RmBox(center, size, rot));
        }

        // Spots to try, nearest first: rings round the middle, `step` apart, out to `reach`.
        static IEnumerable<Vector3> RmRings(float reach, float step = 0.4f)
        {
            yield return Vector3.zero;
            for (float r = step; r <= reach + 0.001f; r += step)
            {
                int n = Mathf.Max(6, Mathf.RoundToInt(Mathf.PI * 2f * r / step));
                for (int k = 0; k < n; k++)
                {
                    float a = (k + (r / step) * 0.37f) / n * Mathf.PI * 2f;
                    yield return new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
                }
            }
        }

        // A patch of floor near `want` for something `size` big (x across, y high, z deep, in
        // its own frame, turned to `yaw`), within `reach`. The floor has to be level and within
        // `tol` of want's height (so nothing lands on a table, or on the storey below).
        static bool RmFloor(string what, Vector3 want, Vector3 size, float yaw, float reach, out Vector3 foot, float tol = 0.3f)
        {
            var rot = Rot(yaw);
            foreach (var o in RmRings(reach))
            {
                Vector3 p = want + o;
                RaycastHit hit;
                if (!Physics.Raycast(p + Vector3.up * 1.6f, Vector3.down, out hit, 1.6f + tol + 0.05f, RmMask, QueryTriggerInteraction.Ignore)) continue;
                if (hit.normal.y < 0.85f || hit.rigidbody != null || Mathf.Abs(hit.point.y - want.y) > tol) continue;
                // (From a hand's breadth up: anything lying flat on the floor, rugs and runners, is fine to stand on.)
                Vector3 c = hit.point + Vector3.up * (0.15f + size.y) * 0.5f;
                Vector3 s = new Vector3(size.x, Mathf.Max(0.05f, size.y - 0.15f), size.z);
                if (!RmFits(c, s, rot)) continue;
                foot = hit.point;
                RmGot(c, s, rot);
                return true;
            }
            foot = want;
            RmMiss(what, want);
            return false;
        }

        // Something lying flat on the floor (a hopscotch, a mosaic): only has to keep off other
        // flat things and anything standing there.
        static bool RmFlat(string what, Vector3 want, Vector3 size, float yaw, float reach, out Vector3 foot, float tol = 0.3f)
        {
            var rot = Rot(yaw);
            foreach (var o in RmRings(reach))
            {
                Vector3 p = want + o;
                RaycastHit hit;
                if (!Physics.Raycast(p + Vector3.up * 1.6f, Vector3.down, out hit, 1.6f + tol + 0.05f, RmMask, QueryTriggerInteraction.Ignore)) continue;
                if (hit.normal.y < 0.95f || hit.rigidbody != null || Mathf.Abs(hit.point.y - want.y) > tol) continue;
                Vector3 c = hit.point + Vector3.up * (0.015f + size.y * 0.5f);
                if (!RmClear(RmBox(c, size, rot))) continue;
                if (Physics.CheckBox(c + Vector3.up * 0.1f, new Vector3(size.x * 0.5f, 0.08f, size.z * 0.5f), rot, RmMask, QueryTriggerInteraction.Ignore)) continue;
                foot = hit.point;
                RmGot(c, size, rot);
                return true;
            }
            foot = want;
            RmMiss(what, want);
            return false;
        }

        // A clear bit of wall near `want`, for something that stands `size.z` out from it and
        // faces `facing` (out into the room). Slides along the wall (and up and down a little,
        // if `slideUp`) up to `reach`. Returns the point on the wall's face.
        static bool RmWall(string what, Vector3 want, Vector3 facing, Vector3 size, float reach, out Vector3 at, bool slideUp = true)
        {
            facing.y = 0f;
            facing.Normalize();
            var rot = Quaternion.LookRotation(facing, Vector3.up);
            Vector3 along = Vector3.Cross(Vector3.up, facing);
            for (float o = 0f; o <= reach + 0.001f; o += 0.3f)
                for (int side = 0; side < (o == 0f ? 1 : 2); side++)
                    for (int v = 0; v < (slideUp ? 5 : 1); v++)
                    {
                        float up = v == 0 ? 0f : (v % 2 == 1 ? 1f : -1f) * 0.3f * ((v + 1) / 2);
                        Vector3 p = want + along * (side == 0 ? o : -o) + Vector3.up * up;
                        RaycastHit hit;
                        if (!Physics.Raycast(p + facing * 1.2f, -facing, out hit, 2.4f, RmMask, QueryTriggerInteraction.Ignore)) continue;
                        if (Vector3.Dot(hit.normal, facing) < 0.95f || hit.rigidbody != null) continue;
                        Vector3 c = hit.point + facing * (size.z * 0.5f + 0.03f);
                        if (!RmFits(c, size, rot)) continue;
                        at = hit.point;
                        RmGot(c, size, rot);
                        return true;
                    }
            at = want;
            RmMiss(what, want);
            return false;
        }

        // A clear space in the air (or the water) round `want`, within `reach` across and a
        // little up and down.
        static bool RmAir(string what, Vector3 want, Vector3 size, float yaw, float reach, out Vector3 at, float upDown = 0.6f)
        {
            var rot = Rot(yaw);
            foreach (var o in RmRings(reach, 0.5f))
                for (int v = 0; v < 3; v++)
                {
                    Vector3 c = want + o + Vector3.up * (v == 0 ? 0f : v == 1 ? upDown : -upDown);
                    if (v > 0 && upDown <= 0f) break;
                    if (!RmFits(c, size, rot)) continue;
                    at = c;
                    RmGot(c, size, rot);
                    return true;
                }
            at = want;
            RmMiss(what, want);
            return false;
        }

        // Whether a box is clear, and claims it if so (no complaint if not: for the little
        // things scattered by the handful, where one more or less doesn't matter).
        static bool RmTry(Vector3 center, Vector3 size, float yaw)
        {
            var rot = Rot(yaw);
            if (!RmFits(center, size, rot)) return false;
            RmClaim(RmBox(center, size, rot));
            return true;
        }

        // A plaque near `want` (see Plaque), reading `yaw`.
        static void RmPlaque(LevelKit kit, Vector3 want, float yaw, float reach, string title, params string[] text)
        {
            Vector3 foot;
            if (RmFloor("the plaque \"" + title + "\"", want, new Vector3(1f, 1.4f, 0.8f), yaw, reach, out foot)) Plaque(kit, foot, yaw, title, text);
        }

        // ------------------------------------------------------------------ Building

        static Transform RmGroup(LevelKit kit, string name, Vector3 at, float yaw)
        {
            var g = kit.Group(name, at);
            g.rotation = Rot(yaw);
            return g;
        }

        static Transform RmGroup(LevelKit kit, string name, Vector3 at, Quaternion rot)
        {
            var g = kit.Group(name, at);
            g.rotation = rot;
            return g;
        }

        static Material RmFelt(Color c) { return PatchworkAssets.Felt(c); }
        static Material RmGlow(Color c, float k) { return PatchworkAssets.Glow(c, k); }
        static Material RmShine(Color c, float s) { return PatchworkAssets.Plain(c, s); }

        // A round rod from a to b: eight flat slats round its middle (so a batch can carry it).
        static void RmTube(MeshBatch mb, Vector3 a, Vector3 b, float r, int segs = 8)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            if (len < 1e-4f) return;
            Vector3 dir = d / len;
            var q = Quaternion.LookRotation(dir, Mathf.Abs(dir.y) > 0.95f ? Vector3.forward : Vector3.up);
            float w = 2f * r * Mathf.Tan(Mathf.PI / segs) * 1.04f, t = Mathf.Max(0.006f, r * 0.4f);
            Vector3 mid = (a + b) * 0.5f;
            for (int i = 0; i < segs; i++)
            {
                var qi = q * Quaternion.Euler(0f, 0f, i * 360f / segs);
                mb.Box(mid + qi * new Vector3(r - t * 0.5f, 0f, 0f), new Vector3(t, w, len), qi);
            }
        }

        // Parts gathered by material, in a group's own frame, and built as one mesh each.
        sealed class RmBits
        {
            readonly LevelKit kit;
            public readonly Transform Root;
            readonly List<Material> mats = new List<Material>();
            readonly List<MeshBatch> batches = new List<MeshBatch>();

            public RmBits(LevelKit kit, Transform root) { this.kit = kit; Root = root; }

            public MeshBatch this[Material m]
            {
                get
                {
                    int i = mats.IndexOf(m);
                    if (i < 0) { mats.Add(m); batches.Add(new MeshBatch()); i = mats.Count - 1; }
                    return batches[i];
                }
            }

            public void Box(Material m, Vector3 c, Vector3 s) { this[m].Box(c, s, Quaternion.identity); }
            public void Box(Material m, Vector3 c, Vector3 s, Vector3 euler) { this[m].Box(c, s, Quaternion.Euler(euler)); }
            public void Ball(Material m, Vector3 c, Vector3 s, int rings = 4, int segs = 8) { this[m].Ball(c, s, rings, segs); }
            public void Cyl(Material m, Vector3 bottom, float r, float h, int segs = 10) { this[m].Cylinder(bottom, r, h, segs); }
            public void Tube(Material m, Vector3 a, Vector3 b, float r, int segs = 8) { RmTube(this[m], a, b, r, segs); }
            // A primitive of its own (for discs and anything round that isn't upright).
            public Transform Prim(PrimitiveType t, Material m, Vector3 pos, Vector3 scale, Vector3 euler)
            {
                return NoShadow(kit.Deco(t, pos, scale, m, euler, Root));
            }
            // A ring of short straight pieces, lying in the plane its frame's x and y make
            // (turn it with `euler`); from `from` to `to` degrees round, for an arch.
            public void Ring(Material m, Vector3 c, Vector3 euler, float r, float thick, int segs = 16, float from = 0f, float to = 360f)
            {
                var frame = Quaternion.Euler(euler);
                float span = (to - from) * Mathf.Deg2Rad, step = span / segs;
                float piece = 2f * r * Mathf.Sin(step * 0.5f) * 1.08f;
                for (int i = 0; i < segs; i++)
                {
                    float a = from * Mathf.Deg2Rad + step * (i + 0.5f);
                    this[m].Box(c + frame * new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f), new Vector3(thick, piece, thick), frame * Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg));
                }
            }

            // A flat round disc facing along the group's +z (a clock face, a dial, a plate).
            public Transform Disc(Material m, Vector3 c, float diameter, float thick)
            {
                return Prim(PrimitiveType.Cylinder, m, c, new Vector3(diameter, thick * 0.5f, diameter), new Vector3(90f, 0f, 0f));
            }

            public Transform Done(string name, bool shadows = false)
            {
                for (int i = 0; i < mats.Count; i++) batches[i].Build(Root, name, mats[i], shadows);
                mats.Clear();
                batches.Clear();
                return Root;
            }
        }

        // A little exhibit: something on a stand or in a case, with its plaque beside it on
        // the right as you look at it. `stand` is the footprint of the stand alone; the
        // exhibit faces `yaw` (towards the visitor). Returns false if there was no room.
        static bool RmExhibit(LevelKit kit, string what, Vector3 want, float yaw, float reach, Vector3 stand, out Transform g, string title, params string[] text)
        {
            // Stand and plaque side by side: the plaque stands to the visitor's right.
            var size = new Vector3(stand.x + 1.3f, Mathf.Max(stand.y, 1.4f), Mathf.Max(stand.z, 1f));
            Vector3 foot;
            g = null;
            if (!RmFloor(what, want, size, yaw, reach, out foot)) return false;
            var r = Rot(yaw);
            g = RmGroup(kit, what, foot + r * new Vector3(0.65f, 0f, 0f), yaw);
            Plaque(kit, foot + r * new Vector3(-(stand.x * 0.5f + 0.1f), 0f, 0.1f), yaw, title, text);
            return true;
        }

        // A glass case on a dark wooden stand (in the group's frame), lit from above. Returns
        // the middle of the felt inside it.
        static Vector3 RmCase(LevelKit kit, Transform g, Vector3 size, bool lit = true)
        {
            var b = new RmBits(kit, g);
            const float stand = 0.95f;
            b.Box(DarkWood, new Vector3(0f, stand * 0.5f, 0f), new Vector3(size.x, stand, size.z));
            b.Box(Brass, new Vector3(0f, 0.06f, 0f), new Vector3(size.x + 0.12f, 0.12f, size.z + 0.12f));
            b.Box(Brass, new Vector3(0f, stand + 0.03f, 0f), new Vector3(size.x + 0.08f, 0.06f, size.z + 0.08f));
            b.Box(RmFelt(Wine), new Vector3(0f, stand + 0.07f, 0f), new Vector3(size.x - 0.16f, 0.03f, size.z - 0.16f));
            float gh = size.y - stand;
            foreach (int sx in new[] { -1, 1 })
                foreach (int sz in new[] { -1, 1 })
                    b.Box(Brass, new Vector3(sx * (size.x * 0.5f - 0.03f), stand + gh * 0.5f, sz * (size.z * 0.5f - 0.03f)), new Vector3(0.05f, gh, 0.05f));
            b.Box(Brass, new Vector3(0f, size.y, 0f), new Vector3(size.x, 0.06f, size.z));
            b.Done("ExhibitCase", true);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, stand + gh * 0.5f, 0f), new Vector3(size.x - 0.06f, gh - 0.02f, size.z - 0.06f), CaseGlass, default(Vector3), g));
            var col = g.gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, size.y * 0.5f, 0f);
            col.size = size;
            if (lit) PointLight(kit, g.TransformPoint(new Vector3(0f, size.y - 0.2f, 0.3f)), new Color(1f, 0.9f, 0.72f), 4f, 0.8f);
            return new Vector3(0f, stand + 0.09f, 0f);
        }

        // Makes something solid in its group's frame (a box from `center`, `size`).
        static void RmSolid(LevelKit kit, Transform g, Vector3 center, Vector3 size)
        {
            var go = new GameObject("Solid");
            go.transform.SetParent(g, false);
            go.transform.localPosition = center;
            go.AddComponent<BoxCollider>().size = size;
        }

        // Three dots for a pair of eyes and a nose, on a little face looking along +z.
        static void RmEyes(RmBits b, Vector3 between, float apart, float size)
        {
            var black = RmShine(new Color(0.04f, 0.04f, 0.06f), 0.9f);
            b.Ball(black, between + new Vector3(-apart * 0.5f, 0f, 0f), Vector3.one * size, 3, 6);
            b.Ball(black, between + new Vector3(apart * 0.5f, 0f, 0f), Vector3.one * size, 3, 6);
            var glint = RmGlow(Color.white, 0.6f);
            b.Ball(glint, between + new Vector3(-apart * 0.5f + size * 0.15f, size * 0.2f, size * 0.42f), Vector3.one * size * 0.3f, 2, 4);
            b.Ball(glint, between + new Vector3(apart * 0.5f + size * 0.15f, size * 0.2f, size * 0.42f), Vector3.one * size * 0.3f, 2, 4);
        }
    }

    // ---------------------------------------------------------------------- Small behaviours

    // A pufferfish: every so often it gulps water and swells up round, holds it, and lets it out.
    public class Puffer : MonoBehaviour
    {
        public Transform Body;
        public float Every = 9f, Swell = 1.9f, Offset;
        Vector3 rest;

        void Start() { if (Body != null) rest = Body.localScale; }

        void Update()
        {
            if (Body == null) return;
            float t = Mathf.Repeat(Time.time + Offset, Every);
            float k = t < 0.6f ? t / 0.6f : t < 3.2f ? 1f : t < 4.4f ? 1f - (t - 3.2f) / 1.2f : 0f;
            k = Mathf.SmoothStep(0f, 1f, k);
            Body.localScale = Vector3.Scale(rest, Vector3.Lerp(Vector3.one, new Vector3(Swell, Swell * 1.15f, Swell * 0.85f), k));
        }
    }

    // A cuttlefish's skin: rolling slowly from one colour to the next, and flickering stripes.
    public class ColourShift : MonoBehaviour
    {
        public Material Mat;
        public Color[] Colours;
        public float Each = 2.6f, Glow = 0.35f, Offset;

        void Update()
        {
            if (Mat == null || Colours == null || Colours.Length == 0) return;
            float t = (Time.time + Offset) / Each;
            int i = Mathf.FloorToInt(t);
            Color a = Colours[((i % Colours.Length) + Colours.Length) % Colours.Length], b = Colours[(((i + 1) % Colours.Length) + Colours.Length) % Colours.Length];
            Color c = Color.Lerp(a, b, Mathf.SmoothStep(0f, 1f, t - i));
            Mat.SetColor("_BaseColor", c);
            Mat.SetColor("_EmissionColor", c * Glow * (0.75f + 0.25f * Mathf.Sin(Time.time * 7f + Offset)));
        }
    }

    // Something that pops out along local +z every so often and goes back in (a cuckoo).
    public class PopOut : MonoBehaviour
    {
        public float Out = 0.3f, Every = 12f, Hold = 1.6f, Offset;
        public Transform Door;
        Vector3 home;

        void Start() { home = transform.localPosition; }

        void Update()
        {
            float t = Mathf.Repeat(Time.time + Offset, Every);
            float k = t < 0.25f ? t / 0.25f : t < 0.25f + Hold ? 1f : t < 0.5f + Hold ? 1f - (t - 0.25f - Hold) / 0.25f : 0f;
            // (Bobbing as it calls, twice.)
            float bow = (k >= 1f) ? Mathf.Abs(Mathf.Sin((t - 0.25f) * Mathf.PI * 2f / Hold * 2f)) * 18f : 0f;
            transform.localPosition = home + Vector3.forward * Out * k;
            transform.localRotation = Quaternion.Euler(bow, 0f, 0f);
            if (Door != null) Door.localRotation = Quaternion.Euler(0f, -100f * Mathf.Clamp01(k * 1.6f), 0f);
        }
    }

    // A planet going round its sun on an ellipse, the way Kepler found they do: quickest when
    // it's nearest, slowest at the far end. (Solves his equation a few times a frame.)
    public class KeplerOrbit : MonoBehaviour
    {
        public float A = 0.6f, E = 0.55f, Period = 9f;

        void Update()
        {
            float m = Mathf.Repeat(Time.time / Period, 1f) * Mathf.PI * 2f;
            float ea = m;
            for (int i = 0; i < 6; i++) ea = m + E * Mathf.Sin(ea);
            float b = A * Mathf.Sqrt(1f - E * E);
            transform.localPosition = new Vector3(A * (Mathf.Cos(ea) - E), transform.localPosition.y, b * Mathf.Sin(ea));
        }
    }

    // Swaying gently about its own foot, like a plant in the water or a flag in a draught.
    public class RmSway : MonoBehaviour
    {
        public float Amount = 6f, Speed = 0.8f, Phase;
        Quaternion rest;

        void Start() { rest = transform.localRotation; Phase += transform.position.x * 0.7f + transform.position.z * 0.3f; }

        void Update()
        {
            float t = Time.time * Speed + Phase;
            transform.localRotation = rest * Quaternion.Euler(Mathf.Sin(t) * Amount, 0f, Mathf.Sin(t * 0.77f + 1.3f) * Amount * 0.7f);
        }
    }
}

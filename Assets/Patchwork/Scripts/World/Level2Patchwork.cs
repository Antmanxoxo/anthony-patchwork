using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The hand-made touches that make the Oceanic Museum belong in Patchwork's world, laid over
    // everything else once the museum is built:
    //   - Mended patches: squares of felt sewn onto the stone walls here and there, a little
    //     crooked, with a ring of big stitches round each, as if somebody had darned the place.
    //   - Bunting: strings of felt flags across the big halls, sagging between the walls.
    // (The knitted socks on the columns are in Column, in Level2Kit.)
    public static partial class Level2Builder
    {
        static readonly Color[] MendColours =
        {
            new Color(0.72f, 0.36f, 0.38f), new Color(0.88f, 0.68f, 0.36f), new Color(0.34f, 0.52f, 0.6f),
            new Color(0.5f, 0.42f, 0.62f), new Color(0.46f, 0.6f, 0.42f), new Color(0.82f, 0.56f, 0.46f)
        };

        static void PatchworkTouches(LevelKit kit)
        {
            var root = kit.Group("PatchworkTouches", Vector3.zero);
            var felt = new MeshBatch[MendColours.Length];
            for (int i = 0; i < felt.Length; i++) felt[i] = new MeshBatch();
            var thread = new MeshBatch();
            var rng = new System.Random(1351);

            int mends = Mends(kit, felt, thread, rng);
            int flags = Bunting(felt, thread, rng);

            for (int i = 0; i < felt.Length; i++) felt[i].Build(root, "Felt", PatchworkAssets.Felt(MendColours[i]));
            thread.Build(root, "Thread", PatchworkAssets.Yarn(new Color(0.96f, 0.9f, 0.76f)));
            Debug.Log("[Level2] Patchwork touches: " + mends + " mended patches, " + flags + " bunting flags.");
        }

        static float Rand(System.Random rng, float a, float b) { return a + (float)rng.NextDouble() * (b - a); }

        // A dashed line of stitches between two points, lying flat against a face that looks
        // along "normal".
        static void Dashes(MeshBatch b, Vector3 from, Vector3 to, Vector3 normal, float dash, float gap, float width)
        {
            Vector3 d = to - from;
            float len = d.magnitude;
            if (len < 0.01f) return;
            Vector3 dir = d / len;
            // (Facing out of the wall, with local x running along the line.)
            var rot = Quaternion.LookRotation(normal, Vector3.Cross(normal, dir));
            for (float s = gap * 0.5f; s + dash <= len; s += dash + gap)
                b.Box(from + dir * (s + dash * 0.5f), new Vector3(dash, width, 0.012f), rot);
        }

        // ---------------------------------------------------------------- Mended patches

        static int Mends(LevelKit kit, MeshBatch[] felt, MeshBatch thread, System.Random rng)
        {
            int placed = 0;
            var walls = new List<Transform>();
            foreach (Transform t in kit.Root)
                if ((t.name == "Wall" || t.name == "Lintel") && t.GetComponent<BoxCollider>() != null) walls.Add(t);

            foreach (var w in walls)
            {
                Vector3 s = w.localScale;
                bool alongX = s.x > s.z;
                float len = alongX ? s.x : s.z, thick = alongX ? s.z : s.x;
                if (len < 8f || s.y < 4f) continue;
                Vector3 along = alongX ? Vector3.right : Vector3.forward;
                float top = w.position.y + s.y * 0.5f;
                int slots = Mathf.FloorToInt(len / 13f);
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector3 n = alongX ? new Vector3(0f, 0f, side) : new Vector3(side, 0f, 0f);
                    for (int i = 0; i < slots; i++)
                    {
                        if (rng.NextDouble() < 0.5) continue;
                        float u = -len * 0.5f + len * (i + 0.5f) / slots + Rand(rng, -2f, 2f);
                        Vector3 face = w.position + along * u + n * (thick * 0.5f);
                        // Find the floor in front of this bit of wall.
                        RaycastHit fh;
                        if (!Physics.Raycast(new Vector3(face.x, top - 0.3f, face.z) + n * 0.8f, Vector3.down, out fh, s.y, RmMask, QueryTriggerInteraction.Ignore)) continue;
                        if (fh.point.y > top - 3.4f) continue;
                        float py = fh.point.y + Rand(rng, 2.1f, 3.4f);
                        // And make sure it's this wall's face that's there (not a cabinet in front of it).
                        RaycastHit wh;
                        if (!Physics.Raycast(new Vector3(face.x, py, face.z) + n * 1.2f, -n, out wh, 1.5f, RmMask, QueryTriggerInteraction.Ignore)) continue;
                        if (wh.collider == null || wh.collider.transform != w) continue;

                        float pw = Rand(rng, 0.8f, 1.5f), ph = Rand(rng, 0.65f, 1.1f);
                        var rot = Quaternion.LookRotation(n, Vector3.up) * Quaternion.Euler(0f, 0f, Rand(rng, -7f, 7f));
                        Vector3 c = wh.point + n * 0.03f;
                        var size = new Vector3(pw + 0.1f, ph + 0.1f, 0.05f);
                        if (!RmFits(c + n * 0.02f, size, rot)) continue;
                        RmGot(c + n * 0.02f, size, rot);

                        felt[rng.Next(felt.Length)].Box(c, new Vector3(pw, ph, 0.03f), rot);
                        // A ring of big stitches, a little in from the edge.
                        float ix = pw * 0.5f - 0.08f, iy = ph * 0.5f - 0.08f;
                        Vector3 lift = n * 0.018f;
                        Vector3 a = c + rot * new Vector3(-ix, -iy, 0f) + lift, b = c + rot * new Vector3(ix, -iy, 0f) + lift;
                        Vector3 cc = c + rot * new Vector3(ix, iy, 0f) + lift, dd = c + rot * new Vector3(-ix, iy, 0f) + lift;
                        Dashes(thread, a, b, n, 0.1f, 0.07f, 0.03f);
                        Dashes(thread, b, cc, n, 0.1f, 0.07f, 0.03f);
                        Dashes(thread, cc, dd, n, 0.1f, 0.07f, 0.03f);
                        Dashes(thread, dd, a, n, 0.1f, 0.07f, 0.03f);
                        // Now and then, a cross-stitch in the middle, like a darn.
                        if (rng.NextDouble() < 0.35)
                        {
                            float r = Mathf.Min(ix, iy) * 0.5f;
                            Dashes(thread, c + rot * new Vector3(-r, -r, 0f) + lift, c + rot * new Vector3(r, r, 0f) + lift, n, 0.1f, 0.06f, 0.035f);
                            Dashes(thread, c + rot * new Vector3(-r, r, 0f) + lift, c + rot * new Vector3(r, -r, 0f) + lift, n, 0.1f, 0.06f, 0.035f);
                        }
                        placed++;
                    }
                }
            }
            return placed;
        }

        // ---------------------------------------------------------------- Bunting

        static int Bunting(MeshBatch[] felt, MeshBatch thread, System.Random rng)
        {
            // Spans from one wall to the other across each big hall (it's left out wherever the
            // string would pass through something: the whale, the longship, a balcony).
            var spans = new List<Vector3[]>();
            System.Action<float, float, float, float, float> AcrossX = (x0, x1, y, zFrom, zTo) =>
            {
                for (float z = zFrom; z <= zTo + 0.01f; z += 14f) spans.Add(new[] { new Vector3(x0, y, z), new Vector3(x1, y, z) });
            };
            AcrossX(-29.3f, 29.3f, 12.5f, -102f, -60f);                      // the Grand Atrium
            AcrossX(HistX0 + 0.8f, HistX1 - 0.8f, 11f, HistZ0 + 10f, HistZ1 - 8f);   // the Hall of Deep Time
            AcrossX(KelpX0 + 0.8f, KelpX1 - 0.8f, 13f, KelpZ0 + 6f, KelpZ1 - 6f);    // the Kelp Galleries
            AcrossX(RuinX0 + 0.8f, RuinX1 - 0.8f, 12f, RuinZ0 + 12f, RuinZ1 - 10f);  // the Sunken Ruins

            int flags = 0;
            foreach (var sp in spans)
            {
                Vector3 a = sp[0], b = sp[1];
                float sag = (b - a).magnitude * 0.05f;
                // The string must hang free all the way along.
                bool clear = true;
                Vector3 prev = a;
                for (int k = 1; k <= 12 && clear; k++)
                {
                    float t = k / 12f;
                    Vector3 p = Vector3.Lerp(a, b, t) + Vector3.down * sag * 4f * t * (1f - t);
                    if (Physics.Linecast(prev, p, RmMask, QueryTriggerInteraction.Ignore) ||
                        Physics.Linecast(prev + Vector3.down * 0.6f, p + Vector3.down * 0.6f, RmMask, QueryTriggerInteraction.Ignore)) clear = false;
                    prev = p;
                }
                if (!clear) continue;
                if (!RmClear(new Bounds((a + b) * 0.5f + Vector3.down * sag * 0.5f, new Vector3(Mathf.Abs(b.x - a.x), sag + 0.8f, 0.3f)))) continue;

                // The string.
                prev = a;
                const int Segs = 16;
                for (int k = 1; k <= Segs; k++)
                {
                    float t = k / (float)Segs;
                    Vector3 p = Vector3.Lerp(a, b, t) + Vector3.down * sag * 4f * t * (1f - t);
                    Vector3 d = p - prev;
                    thread.Box((prev + p) * 0.5f, new Vector3(0.03f, 0.03f, d.magnitude + 0.02f), Quaternion.LookRotation(d, Vector3.up));
                    prev = p;
                }
                // The flags, a hand's width apart, each hanging a little differently.
                float length = (b - a).magnitude;
                int count = Mathf.FloorToInt(length / 0.9f);
                int colour = rng.Next(felt.Length);
                for (int f = 1; f < count; f++)
                {
                    float t0 = (f - 0.32f) / count, t1 = (f + 0.32f) / count, tm = f / (float)count;
                    Vector3 p0 = Vector3.Lerp(a, b, t0) + Vector3.down * sag * 4f * t0 * (1f - t0);
                    Vector3 p1 = Vector3.Lerp(a, b, t1) + Vector3.down * sag * 4f * t1 * (1f - t1);
                    Vector3 pm = Vector3.Lerp(a, b, tm) + Vector3.down * sag * 4f * tm * (1f - tm);
                    Vector3 tip = pm + Vector3.down * Rand(rng, 0.5f, 0.6f) + new Vector3(0f, 0f, Rand(rng, -0.08f, 0.08f));
                    felt[(colour + f) % felt.Length].Flag(p0 + Vector3.down * 0.02f, p1 + Vector3.down * 0.02f, tip);
                    flags++;
                }
            }
            return flags;
        }
    }
}

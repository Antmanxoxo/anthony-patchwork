using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The patchwork planet outside the Pod's window, in detail. Its map is worked out from
    // longitude and latitude (so the picture, the shape it's wrapped on and the little things
    // stood on it all agree): rolling continents made of quilted fabric patches with stitched
    // coastlines and sandy beaches, paler shallows, wave stitches out at sea, felt mountains
    // with snowy tops and ragged ice caps. Every level's pin sits on land. On top of that:
    // tiny felt trees, cottages and mountains stood on the land, and a layer of cotton-wool
    // clouds drifting round a little slower than the planet turns.
    public static class GlobeDetail
    {
        // ------------------------------------------------------------------ The map

        static float Hash(int x, int y, int z, int seed)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + z * 1274126177 + seed * 1442695040;
                h = (h ^ (h >> 13)) * 1274126177;
                return ((h ^ (h >> 16)) & 0xffffff) / (float)0xffffff;
            }
        }

        static float Noise(Vector3 p, int seed)
        {
            int x = Mathf.FloorToInt(p.x), y = Mathf.FloorToInt(p.y), z = Mathf.FloorToInt(p.z);
            float fx = p.x - x, fy = p.y - y, fz = p.z - z;
            fx = fx * fx * (3f - 2f * fx); fy = fy * fy * (3f - 2f * fy); fz = fz * fz * (3f - 2f * fz);
            float a = Mathf.Lerp(Hash(x, y, z, seed), Hash(x + 1, y, z, seed), fx);
            float b = Mathf.Lerp(Hash(x, y + 1, z, seed), Hash(x + 1, y + 1, z, seed), fx);
            float c = Mathf.Lerp(Hash(x, y, z + 1, seed), Hash(x + 1, y, z + 1, seed), fx);
            float d = Mathf.Lerp(Hash(x, y + 1, z + 1, seed), Hash(x + 1, y + 1, z + 1, seed), fx);
            return Mathf.Lerp(Mathf.Lerp(a, b, fy), Mathf.Lerp(c, d, fy), fz);
        }

        static float Fbm(Vector3 p, int seed, int octaves)
        {
            float v = 0f, amp = 0.5f, total = 0f;
            for (int i = 0; i < octaves; i++)
            {
                v += Noise(p, seed + i * 17) * amp;
                total += amp;
                p *= 2.03f;
                amp *= 0.5f;
            }
            return v / total;
        }

        static List<Vector3> pinDirs;

        static List<Vector3> Pins()
        {
            if (pinDirs != null) return pinDirs;
            pinDirs = new List<Vector3>();
            foreach (var e in LevelCatalog.Levels) pinDirs.Add(LevelGlobe.Direction(e.Longitude, e.Latitude));
            return pinDirs;
        }

        // How far above the sea a point is: below 0 is sea, above 0 land, above 0.2 mountains.
        public static float Height(Vector3 dir)
        {
            float h = Fbm(dir * 1.7f + new Vector3(3.1f, 1.7f, 5.3f), 7, 5) - 0.53f;
            // Raise land round every level's pin so each sits on an island or a coast.
            foreach (var p in Pins())
            {
                float k = Mathf.Clamp01(1f - Vector3.Angle(dir, p) / 26f);
                h = Mathf.Max(h, k * k * 0.16f + (h + 0.04f) * k);
            }
            return h;
        }

        static readonly Color[] Fields =
        {
            new Color(0.56f, 0.76f, 0.46f), new Color(0.68f, 0.82f, 0.48f), new Color(0.45f, 0.66f, 0.42f),
            new Color(0.86f, 0.82f, 0.5f), new Color(0.94f, 0.7f, 0.58f), new Color(0.74f, 0.66f, 0.86f), new Color(0.6f, 0.8f, 0.66f),
        };

        public static Color Paint(float lonDeg, float latDeg)
        {
            Vector3 dir = LevelGlobe.Direction(lonDeg, latDeg);
            float h = Height(dir);
            float alat = Mathf.Abs(latDeg);
            float ice = alat - 66f - (Fbm(dir * 6f, 91, 3) - 0.5f) * 14f;
            Color c;
            if (ice > 0f)
            {
                c = new Color(0.96f, 0.95f, 0.93f);
                if (ice < 1.2f) c = new Color(0.82f, 0.88f, 0.95f);                          // the frozen edge
            }
            else if (h < 0f)
            {
                // Sea: deep blue far out, paler near the coast, with little wave stitches.
                float shallow = Mathf.Clamp01(1f + h / 0.06f);
                c = Color.Lerp(new Color(0.3f, 0.48f, 0.78f), new Color(0.5f, 0.75f, 0.9f), shallow * shallow);
                float wave = Mathf.Repeat(lonDeg * 0.32f + Mathf.Sin(latDeg * 0.25f) * 1.6f, 1f);
                float row = Mathf.Repeat(latDeg * 0.22f, 1f);
                if (shallow < 0.5f && row < 0.1f && wave < 0.45f && Fbm(dir * 9f, 33, 2) > 0.45f)
                    c = Color.Lerp(c, new Color(0.88f, 0.94f, 1f), 0.65f);
                // The coastline itself: a cream running stitch just out from the shore.
                if (h > -0.012f && Mathf.Repeat(lonDeg * 1.4f + latDeg * 1.4f, 1f) < 0.55f) c = new Color(0.98f, 0.95f, 0.88f);
            }
            else if (h < 0.012f)
                c = new Color(0.98f, 0.88f, 0.62f);                                             // beach
            else if (h > 0.2f)
            {
                // Mountains: lilac felt, snowy on the very tops.
                c = h > 0.26f ? new Color(0.97f, 0.96f, 0.98f) : Color.Lerp(new Color(0.62f, 0.52f, 0.66f), new Color(0.78f, 0.7f, 0.82f), (h - 0.2f) / 0.06f);
            }
            else
            {
                // Quilted fields: each patch of land a different fabric, seamed with stitches.
                Vector3 q = dir * 5.5f;
                Vector3 cell = new Vector3(Mathf.Floor(q.x), Mathf.Floor(q.y), Mathf.Floor(q.z));
                c = Fields[Mathf.FloorToInt(Hash((int)cell.x, (int)cell.y, (int)cell.z, 5) * Fields.Length) % Fields.Length];
                // Darker near the coast and inland a little lighter, like light falling on felt.
                c *= 0.92f + Mathf.Clamp01(h / 0.2f) * 0.12f;
                // Seams between the patches.
                Vector3 f = q - cell;
                float edge = Mathf.Min(Mathf.Min(f.x, 1f - f.x), Mathf.Min(Mathf.Min(f.y, 1f - f.y), Mathf.Min(f.z, 1f - f.z)));
                if (edge < 0.05f && Mathf.Repeat((q.x + q.y + q.z) * 3f, 1f) < 0.55f) c = Color.Lerp(c, new Color(1f, 0.97f, 0.9f), 0.7f);
                // Forest dots here and there.
                if (Fbm(dir * 14f, 61, 2) > 0.66f) c = Color.Lerp(c, new Color(0.3f, 0.52f, 0.36f), 0.55f);
            }
            c *= 0.96f + Hash(Mathf.RoundToInt(lonDeg * 4f), Mathf.RoundToInt(latDeg * 4f), 0, 3) * 0.06f;
            c.a = 1f;
            return c;
        }

        // ------------------------------------------------------------------ The shape

        // A sphere whose picture is laid on by longitude and latitude exactly the way
        // LevelGlobe.Direction places the pins.
        static Mesh sphere;

        public static Mesh Sphere()
        {
            if (sphere != null) return sphere;
            const int lonSteps = 72, latSteps = 36;
            var v = new List<Vector3>();
            var uv = new List<Vector2>();
            var tris = new List<int>();
            for (int j = 0; j <= latSteps; j++)
            {
                float lat = -90f + 180f * j / latSteps;
                for (int i = 0; i <= lonSteps; i++)
                {
                    float lon = -180f + 360f * i / lonSteps;
                    v.Add(LevelGlobe.Direction(lon, lat) * 0.5f);
                    uv.Add(new Vector2(i / (float)lonSteps, j / (float)latSteps));
                }
            }
            int row = lonSteps + 1;
            for (int j = 0; j < latSteps; j++)
                for (int i = 0; i < lonSteps; i++)
                {
                    int a = j * row + i, b = a + 1, c = a + row, d = c + 1;
                    tris.Add(a); tris.Add(c); tris.Add(b);
                    tris.Add(b); tris.Add(c); tris.Add(d);
                }
            sphere = new Mesh { name = "PatchworkPlanet" };
            sphere.SetVertices(v);
            sphere.SetUVs(0, uv);
            sphere.SetTriangles(tris, 0);
            // (Pointing out from the middle: a sphere's normals are just its directions.)
            var n = new Vector3[v.Count];
            for (int i = 0; i < n.Length; i++) n[i] = v[i].normalized;
            sphere.normals = n;
            sphere.RecalculateTangents();
            sphere.RecalculateBounds();
            // If the winding comes out inside-out, the planet would vanish: check one face (one
            // round the middle: the ones at the poles are squashed to a point).
            int mid = (latSteps / 2) * lonSteps * 6;
            Vector3 p0 = v[tris[mid]], p1 = v[tris[mid + 1]], p2 = v[tris[mid + 2]];
            if (Vector3.Dot(Vector3.Cross(p1 - p0, p2 - p0), p0 + p1 + p2) < 0f)
            {
                for (int i = 0; i < tris.Count; i += 3) { int t = tris[i + 1]; tris[i + 1] = tris[i + 2]; tris[i + 2] = t; }
                sphere.SetTriangles(tris, 0);
                sphere.RecalculateTangents();
            }
            return sphere;
        }

        public static void ResetStatics() { sphere = null; pinDirs = null; }

        // ------------------------------------------------------------------ Things on it

        public static void Dress(LevelKit kit, Transform spin, Transform root, float radius)
        {
            var rng = new System.Random(23);
            var green = new[] { PatchworkAssets.Felt(new Color(0.32f, 0.6f, 0.38f)), PatchworkAssets.Felt(new Color(0.45f, 0.7f, 0.4f)), PatchworkAssets.Felt(new Color(0.95f, 0.6f, 0.65f)) };
            var trunk = PatchworkAssets.Wood(new Color(0.55f, 0.38f, 0.26f));
            var walls = new[] { PatchworkAssets.Felt(new Color(0.98f, 0.94f, 0.86f)), PatchworkAssets.Felt(new Color(1f, 0.86f, 0.6f)), PatchworkAssets.Felt(new Color(0.8f, 0.88f, 1f)) };
            var roofs = new[] { PatchworkAssets.Felt(new Color(0.88f, 0.32f, 0.35f)), PatchworkAssets.Felt(new Color(0.45f, 0.55f, 0.85f)), PatchworkAssets.Felt(new Color(0.95f, 0.6f, 0.3f)) };
            var lilac = PatchworkAssets.Felt(new Color(0.66f, 0.56f, 0.72f));
            var snow = PatchworkAssets.Felt(new Color(0.98f, 0.97f, 0.99f));
            int trees = 0, houses = 0, peaks = 0, tries = 0;
            while ((trees < 34 || houses < 10 || peaks < 7) && tries++ < 4000)
            {
                Vector3 d = new Vector3((float)rng.NextDouble() * 2f - 1f, (float)rng.NextDouble() * 1.7f - 0.85f, (float)rng.NextDouble() * 2f - 1f);
                if (d.sqrMagnitude < 0.05f) continue;
                d.Normalize();
                if (Mathf.Abs(Mathf.Asin(d.y) * Mathf.Rad2Deg) > 60f) continue;
                bool nearPin = false;
                foreach (var p in Pins()) if (Vector3.Angle(d, p) < 11f) nearPin = true;
                if (nearPin) continue;
                float h = Height(d);
                if (h < 0.03f) continue;
                var spot = new GameObject("PlanetThing").transform;
                spot.SetParent(spin, false);
                spot.localPosition = d * radius * 0.995f;
                spot.localRotation = Quaternion.FromToRotation(Vector3.up, d) * Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
                float s = 0.8f + (float)rng.NextDouble() * 0.4f;
                if (h > 0.2f && peaks < 7)
                {
                    // A felt mountain with a snowy cap.
                    peaks++;
                    Part(kit, spot, PrimitiveType.Sphere, new Vector3(0f, 0.08f, 0f), new Vector3(0.55f, 0.5f, 0.55f) * s, lilac);
                    Part(kit, spot, PrimitiveType.Sphere, new Vector3(0f, 0.25f, 0f), new Vector3(0.24f, 0.18f, 0.24f) * s, snow);
                }
                else if (h < 0.2f && houses < 10 && rng.NextDouble() < 0.3)
                {
                    // A cottage.
                    houses++;
                    int k = rng.Next(3);
                    Part(kit, spot, PrimitiveType.Cube, new Vector3(0f, 0.07f, 0f), new Vector3(0.16f, 0.14f, 0.13f) * s, walls[k]);
                    Part(kit, spot, PrimitiveType.Cube, new Vector3(0f, 0.17f, 0f) * s, new Vector3(0.13f, 0.13f, 0.14f) * s, roofs[k], 45f);
                }
                else if (h < 0.2f && trees < 34)
                {
                    // A lollipop tree.
                    trees++;
                    Part(kit, spot, PrimitiveType.Cylinder, new Vector3(0f, 0.06f, 0f), new Vector3(0.03f, 0.06f, 0.03f) * s, trunk);
                    Part(kit, spot, PrimitiveType.Sphere, new Vector3(0f, 0.17f, 0f) * s, Vector3.one * 0.16f * s, green[rng.Next(green.Length)]);
                }
                else Object.DestroyImmediate(spot.gameObject);
            }

            // Cotton-wool clouds, drifting round a little slower than the planet.
            var clouds = new GameObject("PlanetClouds").transform;
            clouds.SetParent(root, false);
            var drift = clouds.gameObject.AddComponent<Spinner>();
            drift.Axis = new Vector3(0.1f, 1f, 0.05f);
            drift.Speed = 3.5f;
            var cotton = PatchworkAssets.Felt(new Color(1f, 0.99f, 0.97f));
            for (int i = 0; i < 11; i++)
            {
                float lon = i * 360f / 11f + (float)rng.NextDouble() * 20f;
                float lat = ((float)rng.NextDouble() * 2f - 1f) * 50f;
                Vector3 d = LevelGlobe.Direction(lon, lat);
                var puff = new GameObject("Cloud").transform;
                puff.SetParent(clouds, false);
                puff.localPosition = d * (radius + 0.42f);
                puff.localRotation = Quaternion.FromToRotation(Vector3.up, d) * Quaternion.Euler(0f, lon, 0f);
                int bits = 2 + rng.Next(3);
                for (int b = 0; b < bits; b++)
                    Part(kit, puff, PrimitiveType.Sphere, new Vector3((b - (bits - 1) * 0.5f) * 0.22f, (b % 2) * 0.04f, 0f), new Vector3(0.3f, 0.14f, 0.24f) * (b == bits / 2 ? 1.25f : 1f), cotton);
            }
        }

        static void Part(LevelKit kit, Transform parent, PrimitiveType shape, Vector3 at, Vector3 size, Material mat, float yaw = 0f)
        {
            var t = kit.Deco(shape, at, size, mat, new Vector3(0f, yaw, 0f), parent);
            var r = t.GetComponent<Renderer>();
            if (r != null) { r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false; }
        }
    }
}

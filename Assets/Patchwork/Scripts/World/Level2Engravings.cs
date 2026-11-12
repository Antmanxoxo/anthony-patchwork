using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Fine stonework and lettering on the museum, outside and in:
    //   outside: gilt lettering across the portico ("THE OCEANIC MUSEUM"), a motto along the
    //            frieze under the cornice, carved tablets on the towers ("ANNO" / "MDCCCLIII",
    //            the year the first public aquarium opened), scallop-shell roundels on the
    //            towers, stone garlands swagged between the pilasters, and a glowing fanlight
    //            of leaded glass in the top of the great doorway
    //   inside:  a brass compass rose set in the atrium floor round the armillary sphere, and
    //            skirting boards along the atrium's and the Hall of Deep Time's walls
    public static partial class Level2Builder
    {
        static void Engravings(LevelKit kit)
        {
            var gilt = new MeshBatch();
            // The portico: gilt letters on the star-cloth band, with a star at each end.
            Letters(gilt, "THE OCEANIC MUSEUM", new Vector3(0f, 15f, -116.74f), Vector3.back, 0.11f, 0.04f);
            foreach (float x in new[] { -7.6f, 7.6f })
                for (int k = 0; k < 2; k++)
                    gilt.Box(new Vector3(x, 15f, -116.74f), new Vector3(0.5f, 0.12f, 0.04f), Quaternion.Euler(0f, 0f, 45f + k * 90f));
            // The motto, along the frieze under the cornice.
            Letters(gilt, "OPEN TO ALL WHO WONDER", new Vector3(0f, 33.8f, -110.32f), Vector3.back, 0.1f, 0.03f);
            gilt.Build(kit.Root, "FacadeLettering", PatchworkAssets.Glow(new Color(1f, 0.8f, 0.42f), 0.9f));

            var pale = PaleStone;
            var carvedStone = PatchworkAssets.Plain(new Color(0.62f, 0.55f, 0.46f), 0.15f);
            var carve = new MeshBatch();     // pale raised carving
            var cut = new MeshBatch();       // dark cut lettering
            for (int t = -1; t <= 1; t += 2)
            {
                float cx = t * 56f;
                const float tf = -118f;   // the towers' front faces
                // A carved tablet under the lower window.
                kit.Deco(PrimitiveType.Cube, new Vector3(cx, 3.8f, tf - 0.1f), new Vector3(5.6f, 1.3f, 0.2f), pale);
                carve.Box(new Vector3(cx, 3.8f, tf - 0.22f), new Vector3(5.2f, 0.06f, 0.04f), Quaternion.identity);
                foreach (float y in new[] { 3.18f, 4.42f })
                    carve.Box(new Vector3(cx, y, tf - 0.24f), new Vector3(5.8f, 0.1f, 0.1f), Quaternion.identity);
                Letters(cut, t < 0 ? "ANNO" : "MDCCCLIII", new Vector3(cx, 3.8f, tf - 0.21f), Vector3.back, 0.09f, 0.03f);

                // A scallop-shell roundel between the windows.
                Vector3 rc = new Vector3(cx, 23f, tf - 0.08f);
                kit.Deco(PrimitiveType.Cylinder, rc, new Vector3(3.4f, 0.08f, 3.4f), carvedStone, new Vector3(90f, 0f, 0f));
                for (int i = 0; i < 28; i++)
                {
                    float a = i / 28f * Mathf.PI * 2f;
                    carve.Box(rc + new Vector3(Mathf.Cos(a) * 1.72f, Mathf.Sin(a) * 1.72f, -0.1f), new Vector3(0.42f, 0.2f, 0.16f), Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg + 90f));
                }
                Vector3 hinge = rc + new Vector3(0f, -1.05f, -0.12f);
                for (int i = 0; i < 9; i++)
                {
                    float ang = -64f + i * 16f;
                    var q = Quaternion.Euler(0f, 0f, ang);
                    float len = 1.75f - Mathf.Abs(i - 4) * 0.09f;
                    carve.Box(hinge + q * new Vector3(0f, len * 0.5f, 0f), new Vector3(0.16f, len, 0.1f), q);
                }
                for (int i = 0; i < 16; i++)   // the scalloped edge
                {
                    float ang = (-70f + i * (140f / 15f)) * Mathf.Deg2Rad;
                    carve.Ball(hinge + new Vector3(Mathf.Sin(ang) * 1.72f, Mathf.Cos(ang) * 1.72f, 0f), new Vector3(0.26f, 0.26f, 0.14f), 2, 6);
                }
                carve.Box(hinge, new Vector3(0.7f, 0.3f, 0.12f), Quaternion.identity);
            }

            // Stone garlands swagged between the upper pilasters, with ribbons at the ends.
            float[][] spans = { new[] { 43.3f, 50.4f }, new[] { 32.3f, 41.7f }, new[] { 23f, 30.7f } };
            for (int sd = -1; sd <= 1; sd += 2)
                foreach (var sp in spans)
                {
                    float a = sd * sp[0], b = sd * sp[1];
                    const int n = 15;
                    for (int i = 0; i <= n; i++)
                    {
                        float u = i / (float)n;
                        float x = Mathf.Lerp(a, b, u);
                        float y = 32.95f - Mathf.Sin(u * Mathf.PI) * 1.35f;
                        float s = 0.34f + Mathf.Sin(u * Mathf.PI) * 0.16f;
                        carve.Ball(new Vector3(x, y, -110.24f), new Vector3(s * 1.2f, s, s * 0.8f), 3, 6);
                    }
                    foreach (float x in new[] { a, b })
                    {
                        carve.Ball(new Vector3(x, 33.05f, -110.24f), Vector3.one * 0.36f, 3, 6);
                        for (int k = -1; k <= 1; k += 2)
                            carve.Box(new Vector3(x + k * 0.12f, 32.35f, -110.12f), new Vector3(0.12f, 1.1f, 0.04f), Quaternion.Euler(0f, 0f, k * 8f));
                    }
                }
            carve.Build(kit.Root, "FacadeCarving", pale, true);
            cut.Build(kit.Root, "TowerLettering", PatchworkAssets.Plain(new Color(0.3f, 0.24f, 0.2f), 0.2f));

            Fanlight(kit);
            CompassRose(kit);
            Skirting(kit);
        }

        // Leaded glass in the round top of the great doorway (above head height), glowing with
        // the light inside: a half-wheel of panes with a hub, and a bar across the bottom.
        static void Fanlight(LevelKit kit)
        {
            const float r = 4.5f, spring = 6.5f, z = -109f;
            var glass = new MeshBatch();
            const int slices = 18;
            for (int i = 0; i < slices; i++)
            {
                float x0 = -r + 2f * r * i / slices, x1 = -r + 2f * r * (i + 1) / slices;
                float xm = (x0 + x1) * 0.5f;
                float h = Mathf.Sqrt(Mathf.Max(0f, r * r - xm * xm)) - 0.05f;
                if (h <= 0.05f) continue;
                glass.Box(new Vector3(xm, spring + h * 0.5f, z), new Vector3(x1 - x0 + 0.01f, h, 0.04f), Quaternion.identity);
            }
            glass.Build(kit.Root, "Fanlight", PatchworkAssets.Glow(new Color(1f, 0.8f, 0.52f), 0.85f));
            var lead = new MeshBatch();
            lead.Box(new Vector3(0f, spring, z), new Vector3(2f * r, 0.26f, 0.3f), Quaternion.identity);
            for (int i = 1; i < 8; i++)
            {
                var q = Quaternion.Euler(0f, 0f, 90f - i * 22.5f);
                lead.Box(new Vector3(0f, spring, z) + q * new Vector3(0f, (r + 1.1f) * 0.5f, 0f), new Vector3(0.09f, r - 1.1f, 0.12f), q);
            }
            for (int i = 0; i <= 12; i++)
            {
                float a = Mathf.PI * i / 12f;
                lead.Box(new Vector3(Mathf.Cos(a) * 1.1f, spring + Mathf.Sin(a) * 1.1f, z), new Vector3(0.32f, 0.12f, 0.14f), Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg + 90f));
                lead.Box(new Vector3(Mathf.Cos(a) * 2.9f, spring + Mathf.Sin(a) * 2.9f, z), new Vector3(0.8f, 0.08f, 0.12f), Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg + 90f));
            }
            lead.Ball(new Vector3(0f, spring + 0.1f, z), new Vector3(0.9f, 0.9f, 0.2f), 3, 8);
            lead.Build(kit.Root, "FanlightLeading", Brass);
        }

        // A brass compass rose let into the atrium floor round the armillary sphere.
        static void CompassRose(LevelKit kit)
        {
            Vector3 c = new Vector3(0f, 0.03f, -86f);
            var brass = new MeshBatch();
            var dark = new MeshBatch();
            foreach (float rad in new[] { 5.6f, 6.1f })
                for (int i = 0; i < 64; i++)
                {
                    float a = i / 64f * Mathf.PI * 2f;
                    brass.Box(c + new Vector3(Mathf.Cos(a) * rad, 0f, Mathf.Sin(a) * rad), new Vector3(0.12f, 0.02f, rad * Mathf.PI * 2f / 64f + 0.04f), Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f));
                }
            // Points: long to the east and west (the rugs lie north and south), shorter between.
            for (int i = 0; i < 8; i++)
            {
                float yaw = i * 45f;
                bool cardinal = i % 2 == 0;
                bool ew = i == 2 || i == 6;
                float reach = ew ? 9.2f : cardinal ? 6.75f : 7.8f;
                float from = 6.15f, len = reach - from;
                var q = Quaternion.Euler(0f, yaw, 0f);
                // A point tapering to its tip in little steps, half brass and half dark.
                float wdt = cardinal ? 0.6f : 0.42f;
                const int steps = 7;
                for (int k = 0; k < steps; k++)
                {
                    float w = wdt * (1f - k / (float)steps);
                    float along = from + len * (k + 0.5f) / steps;
                    for (int s = -1; s <= 1; s += 2)
                        ((s < 0) == cardinal ? brass : dark).Box(c + q * new Vector3(s * w * 0.25f, 0f, along), new Vector3(w * 0.5f, 0.02f, len / steps + 0.01f), q);
                }
            }
            brass.Build(kit.Root, "CompassRose", Brass);
            dark.Build(kit.Root, "CompassRoseInlay", PatchworkAssets.Plain(Navy, 0.6f));
        }

        // Skirting boards: dark polished marble in the atrium, oak in the Hall of Deep Time, each
        // with a slim brass bead on top, only along the walls (never across a doorway).
        static void Skirting(LevelKit kit)
        {
            var marble = new MeshBatch();
            var oak = new MeshBatch();
            var bead = new MeshBatch();
            System.Action<MeshBatch, Vector3, Vector3, Vector3> run = (mb, a, b, inward) =>
            {
                Vector3 mid = (a + b) * 0.5f;
                bool alongX = Mathf.Abs(b.x - a.x) > Mathf.Abs(b.z - a.z);
                float len = alongX ? Mathf.Abs(b.x - a.x) : Mathf.Abs(b.z - a.z);
                Vector3 size = alongX ? new Vector3(len, 0.4f, 0.08f) : new Vector3(0.08f, 0.4f, len);
                mb.Box(mid + inward * 0.04f + Vector3.up * 0.21f, size, Quaternion.identity);
                Vector3 bs = alongX ? new Vector3(len, 0.05f, 0.11f) : new Vector3(0.11f, 0.05f, len);
                bead.Box(mid + inward * 0.05f + Vector3.up * 0.43f, bs, Quaternion.identity);
            };
            // The atrium (its walls' inner faces are at x +-30, z -108 and z -48).
            run(marble, new Vector3(-30f, 0f, -108f), new Vector3(-30f, 0f, -82f), Vector3.right);
            run(marble, new Vector3(-30f, 0f, -74f), new Vector3(-30f, 0f, -48f), Vector3.right);
            run(marble, new Vector3(30f, 0f, -108f), new Vector3(30f, 0f, -102f), Vector3.left);
            run(marble, new Vector3(30f, 0f, -54f), new Vector3(30f, 0f, -48f), Vector3.left);
            run(marble, new Vector3(-30f, 0f, -48f), new Vector3(-10f, 0f, -48f), Vector3.back);
            run(marble, new Vector3(10f, 0f, -48f), new Vector3(30f, 0f, -48f), Vector3.back);
            run(marble, new Vector3(-30f, 0f, -108f), new Vector3(-4.5f, 0f, -108f), Vector3.forward);
            run(marble, new Vector3(4.5f, 0f, -108f), new Vector3(30f, 0f, -108f), Vector3.forward);
            // The Hall of Deep Time (inner faces at x -88 and -32, z -104 and -10).
            run(oak, new Vector3(-88f, 0f, -104f), new Vector3(-88f, 0f, -10f), Vector3.right);
            run(oak, new Vector3(-88f, 0f, -104f), new Vector3(-32f, 0f, -104f), Vector3.forward);
            run(oak, new Vector3(-88f, 0f, -10f), new Vector3(-64f, 0f, -10f), Vector3.back);
            run(oak, new Vector3(-56f, 0f, -10f), new Vector3(-32f, 0f, -10f), Vector3.back);
            run(oak, new Vector3(-32f, 0f, -104f), new Vector3(-32f, 0f, -82f), Vector3.left);
            run(oak, new Vector3(-32f, 0f, -74f), new Vector3(-32f, 0f, -10f), Vector3.left);
            marble.Build(kit.Root, "AtriumSkirting", PatchworkAssets.Plain(new Color(0.16f, 0.15f, 0.2f), 0.75f));
            oak.Build(kit.Root, "HallSkirting", DarkWood);
            bead.Build(kit.Root, "SkirtingBead", Brass);
        }

        // ---------------------------------------------------------------- Lettering

        // A small blocky alphabet, five dots wide and seven high.
        static readonly Dictionary<char, string[]> Font = new Dictionary<char, string[]>
        {
            { 'A', new[] { ".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" } },
            { 'B', new[] { "####.", "#...#", "#...#", "####.", "#...#", "#...#", "####." } },
            { 'C', new[] { ".###.", "#...#", "#....", "#....", "#....", "#...#", ".###." } },
            { 'D', new[] { "####.", "#...#", "#...#", "#...#", "#...#", "#...#", "####." } },
            { 'E', new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#####" } },
            { 'F', new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#...." } },
            { 'G', new[] { ".###.", "#...#", "#....", "#.###", "#...#", "#...#", ".###." } },
            { 'H', new[] { "#...#", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" } },
            { 'I', new[] { "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "#####" } },
            { 'K', new[] { "#...#", "#..#.", "#.#..", "##...", "#.#..", "#..#.", "#...#" } },
            { 'L', new[] { "#....", "#....", "#....", "#....", "#....", "#....", "#####" } },
            { 'M', new[] { "#...#", "##.##", "#.#.#", "#.#.#", "#...#", "#...#", "#...#" } },
            { 'N', new[] { "#...#", "##..#", "#.#.#", "#..##", "#...#", "#...#", "#...#" } },
            { 'O', new[] { ".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." } },
            { 'P', new[] { "####.", "#...#", "#...#", "####.", "#....", "#....", "#...." } },
            { 'R', new[] { "####.", "#...#", "#...#", "####.", "#.#..", "#..#.", "#...#" } },
            { 'S', new[] { ".####", "#....", "#....", ".###.", "....#", "....#", "####." } },
            { 'T', new[] { "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.." } },
            { 'U', new[] { "#...#", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." } },
            { 'V', new[] { "#...#", "#...#", "#...#", "#...#", "#...#", ".#.#.", "..#.." } },
            { 'W', new[] { "#...#", "#...#", "#...#", "#.#.#", "#.#.#", "##.##", "#...#" } },
            { 'Y', new[] { "#...#", "#...#", ".#.#.", "..#..", "..#..", "..#..", "..#.." } },
        };

        // Lays out a line of text centred on `centre`, on a wall facing `facing`, each dot `px`
        // across and standing `depth` proud of the wall. Rows of dots are merged into strips.
        static void Letters(MeshBatch mb, string text, Vector3 centre, Vector3 facing, float px, float depth)
        {
            var q = Quaternion.LookRotation(-facing, Vector3.up);   // local x runs left to right as you read it
            int cols = text.Length * 6 - 1;
            float left = -cols * px * 0.5f;
            for (int ci = 0; ci < text.Length; ci++)
            {
                string[] g;
                if (!Font.TryGetValue(text[ci], out g)) continue;
                for (int row = 0; row < 7; row++)
                {
                    string line = g[row];
                    int c = 0;
                    while (c < 5)
                    {
                        if (line[c] != '#') { c++; continue; }
                        int start = c;
                        while (c < 5 && line[c] == '#') c++;
                        float x = left + (ci * 6 + (start + c) * 0.5f) * px;
                        float y = (3f - row) * px;
                        mb.Box(centre + q * new Vector3(x, y, -depth * 0.5f), new Vector3((c - start) * px, px, depth), q);
                    }
                }
            }
        }
    }
}

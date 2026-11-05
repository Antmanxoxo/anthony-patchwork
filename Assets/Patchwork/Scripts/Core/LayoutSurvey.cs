using System.Text;
using UnityEngine;

namespace Patchwork
{
    // Editor-only: after a level is built, writes a top-down map of where things are to
    // Logs/patchwork_layout_<level>.txt, so the layout can be checked for empty or crowded
    // patches without flying round it. Each cell is 4 x 4 m. Rows go from north (top) to south.
    //   Detail map:  .  nothing   1-9  how many visible pieces   #  lots (30+)
    //   Solid map:   .  open ground   -  something low to walk over   o  waist high   X  tall
    // Does nothing in a built game.
    public static class LayoutSurvey
    {
        const float Cell = 4f;

        public static void Run(string levelId, Transform root, Bounds area)
        {
#if UNITY_EDITOR
            if (area.size.x <= 0f || area.size.z <= 0f) return;
            int w = Mathf.CeilToInt(area.size.x / Cell), h = Mathf.CeilToInt(area.size.z / Cell);
            if (w > 200 || h > 200) return;
            var detail = new int[w, h];
            var solid = new float[w, h];
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (!r.enabled) continue;
                Vector3 c = r.bounds.center;
                int x = Mathf.FloorToInt((c.x - area.min.x) / Cell), z = Mathf.FloorToInt((c.z - area.min.z) / Cell);
                if (x < 0 || z < 0 || x >= w || z >= h) continue;
                detail[x, z]++;
            }
            int mask = ~(1 << 2);
            for (int x = 0; x < w; x++)
                for (int z = 0; z < h; z++)
                {
                    Vector3 top = new Vector3(area.min.x + (x + 0.5f) * Cell, area.max.y, area.min.z + (z + 0.5f) * Cell);
                    if (Physics.Raycast(top, Vector3.down, out RaycastHit hit, area.size.y + 20f, mask, QueryTriggerInteraction.Ignore))
                        solid[x, z] = hit.point.y;
                    else solid[x, z] = -99f;
                }
            var sb = new StringBuilder();
            sb.AppendLine("Patchwork layout survey: " + levelId + "  cell " + Cell + " m  x " + area.min.x + ".." + area.max.x + "  z " + area.max.z + " (top) .. " + area.min.z + " (bottom)");
            sb.AppendLine();
            sb.AppendLine("DETAIL");
            for (int z = h - 1; z >= 0; z--)
            {
                for (int x = 0; x < w; x++)
                {
                    int n = detail[x, z];
                    sb.Append(n == 0 ? '.' : n >= 30 ? '#' : (char)('0' + Mathf.Min(9, 1 + n / 3)));
                }
                sb.AppendLine("  " + (area.min.z + (z + 0.5f) * Cell).ToString("0"));
            }
            sb.AppendLine();
            sb.AppendLine("SOLID (height of the first thing below the sky)");
            for (int z = h - 1; z >= 0; z--)
            {
                for (int x = 0; x < w; x++)
                {
                    float y = solid[x, z];
                    sb.Append(y < -50f ? ' ' : y < -1f ? '~' : y < 0.15f ? '.' : y < 0.6f ? '-' : y < 1.6f ? 'o' : 'X');
                }
                sb.AppendLine("  " + (area.min.z + (z + 0.5f) * Cell).ToString("0"));
            }
            try
            {
                string path = System.IO.Path.Combine(Application.dataPath, "../Logs/patchwork_layout_" + levelId + ".txt");
                System.IO.File.WriteAllText(path, sb.ToString());
            }
            catch (System.Exception e) { Debug.LogWarning("Patchwork: couldn't write the layout survey (" + e.Message + ")"); }
#endif
        }
    }
}

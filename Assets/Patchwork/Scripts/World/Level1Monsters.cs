using UnityEngine;

namespace Patchwork
{
    // The Fray's creatures roaming Toybox Town (away from the square where you start), and felt
    // hearts to patch Patch up. Three nips and Patch unravels back to the last checkpoint.
    //   Snarlings      - scuttle back and forth, chase you when you're close
    //   Tangle Hoppers - springy yarn balls that bounce after you
    //   Static Moths   - flutter overhead and swoop down at you
    //   Static Spitters - sit still and lob fuzzballs at you from a distance
    // Stomp them, swing into them as Yarn, or blow them away with Wind's gust. Stomping several
    // in a row without landing builds a combo.
    public static partial class Level1Builder
    {
        static void Creatures(LevelKit kit)
        {
            Physics.SyncTransforms();
            var root = kit.Group("Creatures", Vector3.zero);

            Vector3[] snarlings =
            {
                new Vector3(22f, 0f, -22f), new Vector3(-22f, 0f, -24f), new Vector3(52f, 0f, 22f), new Vector3(60f, 0f, -30f),
                new Vector3(-68f, 0f, -28f), new Vector3(16f, 0f, 62f), new Vector3(-30f, 0f, 40f),
            };
            foreach (var s in snarlings)
            {
                Vector3 a;
                if (!FindSpot(s, 0.7f, out a)) continue;
                Vector3 b;
                if (!PatrolEnd(a, 4.5f, out b)) b = a;
                Snarling.Create(root, a + Vector3.up * 0.06f, b + Vector3.up * 0.06f);
            }

            Vector3[] hoppers = { new Vector3(-18f, 0f, 22f), new Vector3(24f, 0f, 30f), new Vector3(54f, 0f, -10f), new Vector3(-66f, 4f, 30f), new Vector3(70f, 0f, 40f) };
            foreach (var h in hoppers)
            {
                Vector3 p;
                if (FindSpot(h, 0.7f, out p)) TangleHopper.Create(root, p);
            }

            Vector3[] moths = { new Vector3(10f, 0f, -32f), new Vector3(-12f, 0f, 50f), new Vector3(62f, 0f, 45f), new Vector3(-76f, 0f, -12f), new Vector3(48f, 0f, -60f) };
            foreach (var m in moths)
            {
                Vector3 p;
                if (FindSpot(m, 1.2f, out p)) StaticMoth.Create(root, p);
            }

            Vector3[] spitters = { new Vector3(30f, 0f, -30f), new Vector3(-30f, 0f, -62f), new Vector3(70f, 0f, -62f), new Vector3(20f, 0f, 74f) };
            foreach (var s in spitters)
            {
                Vector3 p;
                if (FindSpot(s, 1f, out p)) StaticSpitter.Create(root, p);
            }

            // Felt hearts dotted round town.
            Vector3[] hearts = { new Vector3(6f, 0f, -26f), new Vector3(-24f, 0f, -30f), new Vector3(48f, 0f, 10f), new Vector3(-62f, 4f, 10f), new Vector3(4f, 0f, 38f), new Vector3(64f, 0f, -40f), new Vector3(14f, 0f, 70f) };
            foreach (var h in hearts)
            {
                Vector3 p;
                if (FindSpot(h, 0.5f, out p)) HeartPickup.Spawn(p + Vector3.up * 0.8f, false).transform.SetParent(root, true);
            }
        }

        // A clear bit of ground at about the given height, as close to "near" as possible.
        static bool FindSpot(Vector3 near, float clearance, out Vector3 spot)
        {
            int mask = ~(1 << 2);
            for (int ring = 0; ring <= 6; ring++)
            {
                int steps = ring == 0 ? 1 : 8;
                for (int k = 0; k < steps; k++)
                {
                    float a = k * Mathf.PI * 2f / steps;
                    Vector3 p = near + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * ring * 1.5f;
                    RaycastHit hit;
                    if (!Physics.Raycast(new Vector3(p.x, near.y + 20f, p.z), Vector3.down, out hit, 30f, mask, QueryTriggerInteraction.Ignore)) continue;
                    if (hit.normal.y < 0.9f || Mathf.Abs(hit.point.y - near.y) > 0.6f || hit.rigidbody != null) continue;
                    if (Physics.CheckSphere(hit.point + Vector3.up * (clearance + 0.15f), clearance, mask, QueryTriggerInteraction.Ignore)) continue;
                    spot = hit.point;
                    return true;
                }
            }
            spot = near;
            return false;
        }

        // The other end of a short, clear patrol line from a.
        static bool PatrolEnd(Vector3 a, float length, out Vector3 b)
        {
            int mask = ~(1 << 2);
            Vector3[] dirs = { Vector3.right, Vector3.forward, Vector3.left, Vector3.back };
            foreach (var d in dirs)
            {
                Vector3 e = a + d * length;
                if (Physics.CheckCapsule(a + Vector3.up * 0.8f, e + Vector3.up * 0.8f, 0.5f, mask, QueryTriggerInteraction.Ignore)) continue;
                RaycastHit hit;
                if (!Physics.Raycast(e + Vector3.up * 3f, Vector3.down, out hit, 6f, mask, QueryTriggerInteraction.Ignore)) continue;
                if (Mathf.Abs(hit.point.y - a.y) > 0.3f) continue;
                b = hit.point;
                return true;
            }
            b = a;
            return false;
        }
    }
}

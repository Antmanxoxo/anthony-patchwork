using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Toybox Town, with a dash of the roll-everything-up games. Tucked away in the far north-west
    // corner, past Spindle Hill, is the Lost & Found: a heap of bright, oversized everyday things
    // (dice, erasers, sweets, coins, crayons, dominoes, sushi, teacups, toy cars...) and a Tape
    // Ball to roll them up with. And a rainbow over the box.
    public static partial class Level1Builder
    {
        static readonly Color[] Bright =
        {
            new Color(1f, 0.35f, 0.6f), new Color(0.6f, 0.92f, 0.3f), new Color(0.3f, 0.82f, 1f), new Color(1f, 0.86f, 0.25f),
            new Color(1f, 0.55f, 0.2f), new Color(0.65f, 0.42f, 0.95f), new Color(0.95f, 0.25f, 0.3f), new Color(0.45f, 0.9f, 0.75f),
        };
        static readonly Color Ivory = new Color(0.99f, 0.96f, 0.9f);

        delegate void ItemFn(LevelKit kit, Transform g, float s, System.Random r);

        static void Katamari(LevelKit kit)
        {
            Physics.SyncTransforms();
            Rainbow(kit);

            // The Tape Ball, in the Lost & Found corner (a little secret).
            Vector3[] homes = { new Vector3(-72f, 0f, 64f), new Vector3(-74f, 0f, 60f), new Vector3(-70f, 0f, 58f), new Vector3(-78f, 0f, 66f), new Vector3(-68f, 0f, 62f) };
            Vector3 home = homes[0];
            foreach (var h in homes)
            {
                RaycastHit hit;
                if (!Physics.Raycast(h + Vector3.up * 30f, Vector3.down, out hit, 40f, ~(1 << 2), QueryTriggerInteraction.Ignore)) continue;
                if (hit.point.y > 0.3f || hit.point.y < -0.2f) continue;
                if (Physics.CheckSphere(hit.point + Vector3.up * 1.4f, 1.2f, ~(1 << 2), QueryTriggerInteraction.Ignore)) continue;
                home = hit.point;
                break;
            }
            TapeBall.Create(kit, home);
            kit.Hint(home + Vector3.up * 1.5f, new Vector3(7f, 4f, 7f), "The Lost & Found",
                "A Tape Ball! Run into it, or hold {Grab} to push it. Small things stick to it and it grows, so it can pick up bigger things. Make it big enough and it becomes a star!");
            LostAndFoundSign(kit, new Vector3(-58f, 0f, 47f));

            Scatter(kit, home, new Vector2(-87f, 46f), new Vector2(-54f, 87f), 95);
        }

        // A little felt pennant on two crayon posts marking the corner.
        static void LostAndFoundSign(LevelKit kit, Vector3 feet)
        {
            var g = kit.Group("LostAndFoundSign", feet);
            g.rotation = Quaternion.Euler(0f, 220f, 0f);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 1.4f, 1.3f, 0f), new Vector3(0.18f, 1.3f, 0.18f), PatchworkAssets.Plain(s < 0 ? Bright[0] : Bright[2], 0.4f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 2.3f, 0f), new Vector3(3.2f, 0.9f, 0.08f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, Bright[3], Ivory), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.82f, 0f), new Vector3(3.2f, 0.12f, 0.1f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, Bright[6], Ivory), default(Vector3), g);
            // A few things spilling out by the sign, so it's clear what's down here.
            var die = kit.Group("SignDie", feet + new Vector3(1.8f, 0f, -1.2f));
            Die(kit, die, 0.6f, new System.Random(3));
            var cup = kit.Group("SignCup", feet + new Vector3(-2f, 0f, -0.8f));
            Teacup(kit, cup, 0.6f, new System.Random(4));
        }

        static void Scatter(LevelKit kit, Vector3 avoid, Vector2 min, Vector2 max, int target)
        {
            var rng = new System.Random(77);
            ItemFn[] small = { Die, Eraser, Candy, Coin, Thumbtack, CrayonItem, Marble, SewButton, Card, Battery };
            ItemFn[] mid = { Domino, Strawberry, Sushi, Teacup, MahjongTile, Matchbox, Lollipop, RubberDuck, SpinTop, Die, Eraser, Candy };
            ItemFn[] big = { Die, Teacup, ToyCar, Sushi, Strawberry, Matchbox };
            var placed = new List<Vector4>();
            int count = 0;
            int mask = ~(1 << 2);
            for (int attempt = 0; attempt < target * 8 && count < target; attempt++)
            {
                float x = Mathf.Lerp(min.x, max.x, (float)rng.NextDouble());
                float z = Mathf.Lerp(min.y, max.y, (float)rng.NextDouble());
                if (new Vector2(x + 62f, z - 74f).sqrMagnitude < 64f) continue;   // the rocking horse
                double roll = rng.NextDouble();
                float s;
                ItemFn fn;
                bool solid = false;
                if (roll < 0.55) { s = 0.25f + (float)rng.NextDouble() * 0.25f; fn = small[rng.Next(small.Length)]; }
                else if (roll < 0.9) { s = 0.5f + (float)rng.NextDouble() * 0.45f; fn = mid[rng.Next(mid.Length)]; }
                else { s = 1.1f + (float)rng.NextDouble() * 0.5f; fn = big[rng.Next(big.Length)]; solid = true; }

                RaycastHit hit;
                if (!Physics.Raycast(new Vector3(x, 45f, z), Vector3.down, out hit, 60f, mask, QueryTriggerInteraction.Ignore)) continue;
                if (hit.normal.y < 0.95f || hit.point.y > 0.35f || hit.point.y < -0.15f || hit.rigidbody != null) continue;
                Vector3 p = hit.point;
                if (new Vector2(p.x - avoid.x, p.z - avoid.z).sqrMagnitude < 9f) continue;
                float clear = solid ? s * 0.5f + 1.6f : s * 0.5f + 0.1f;
                if (Physics.CheckSphere(p + Vector3.up * (clear + 0.08f), clear, mask, QueryTriggerInteraction.Ignore)) continue;
                bool crowded = false;
                foreach (var q in placed)
                    if (new Vector2(q.x - p.x, q.z - p.z).sqrMagnitude < Mathf.Pow(q.w * 0.5f + s * 0.5f + 0.4f, 2f)) { crowded = true; break; }
                if (crowded) continue;

                var g = kit.Group("Clutter", p);
                fn(kit, g, s, rng);
                foreach (var rr in g.GetComponentsInChildren<Renderer>()) rr.shadowCastingMode = s < 0.5f ? UnityEngine.Rendering.ShadowCastingMode.Off : UnityEngine.Rendering.ShadowCastingMode.On;
                if (solid)
                {
                    // Big things are solid until they're rolled up.
                    var b = new Bounds(p + Vector3.up * s * 0.4f, Vector3.zero);
                    foreach (var rr in g.GetComponentsInChildren<Renderer>()) b.Encapsulate(rr.bounds);
                    var box = g.gameObject.AddComponent<BoxCollider>();
                    box.center = b.center - p;
                    box.size = b.size * 0.9f;
                }
                g.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
                var c = g.gameObject.AddComponent<Clumpable>();
                c.Size = s;
                placed.Add(new Vector4(p.x, p.y, p.z, s));
                count++;
            }
        }

        // ---------------------------------------------------------------- The things

        static Color Pick(System.Random r) { return Bright[r.Next(Bright.Length)]; }

        static void Die(LevelKit kit, Transform g, float s, System.Random r)
        {
            bool red = r.Next(3) == 0;
            kit.Deco(PrimitiveType.Cube, Vector3.up * s * 0.5f, Vector3.one * s, PatchworkAssets.Plain(red ? new Color(0.92f, 0.22f, 0.28f) : Ivory, 0.55f), default(Vector3), g);
            var pip = PatchworkAssets.Plain(red ? Ivory : new Color(0.12f, 0.1f, 0.14f), 0.4f);
            float o = s * 0.27f, ps = s * 0.17f;
            int top = r.Next(1, 7);
            Vector2[] layout = DieLayout(top);
            foreach (var v in layout) kit.Deco(PrimitiveType.Sphere, new Vector3(v.x * o, s, v.y * o), new Vector3(ps, ps * 0.4f, ps), pip, default(Vector3), g);
            foreach (var v in DieLayout(r.Next(1, 7))) kit.Deco(PrimitiveType.Sphere, new Vector3(v.x * o, s * 0.5f + v.y * o, s * 0.5f), new Vector3(ps, ps, ps * 0.4f), pip, default(Vector3), g);
            foreach (var v in DieLayout(r.Next(1, 7))) kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.5f, s * 0.5f + v.y * o, v.x * o), new Vector3(ps * 0.4f, ps, ps), pip, default(Vector3), g);
        }

        static Vector2[] DieLayout(int n)
        {
            switch (n)
            {
                case 1: return new[] { Vector2.zero };
                case 2: return new[] { new Vector2(-1f, -1f), new Vector2(1f, 1f) };
                case 3: return new[] { new Vector2(-1f, -1f), Vector2.zero, new Vector2(1f, 1f) };
                case 4: return new[] { new Vector2(-1f, -1f), new Vector2(1f, 1f), new Vector2(-1f, 1f), new Vector2(1f, -1f) };
                case 5: return new[] { new Vector2(-1f, -1f), new Vector2(1f, 1f), new Vector2(-1f, 1f), new Vector2(1f, -1f), Vector2.zero };
                default: return new[] { new Vector2(-1f, -1f), new Vector2(-1f, 0f), new Vector2(-1f, 1f), new Vector2(1f, -1f), new Vector2(1f, 0f), new Vector2(1f, 1f) };
            }
        }

        static void Eraser(LevelKit kit, Transform g, float s, System.Random r)
        {
            kit.Deco(PrimitiveType.Cube, Vector3.up * s * 0.17f, new Vector3(s, s * 0.34f, s * 0.55f), PatchworkAssets.Felt(r.Next(2) == 0 ? new Color(1f, 0.6f, 0.7f) : new Color(0.98f, 0.95f, 0.92f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.15f, s * 0.17f, 0f), new Vector3(s * 0.5f, s * 0.36f, s * 0.57f), PatchworkAssets.Felt(Pick(r)), default(Vector3), g);
        }

        static void Candy(LevelKit kit, Transform g, float s, System.Random r)
        {
            var wrap = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, Pick(r), Ivory);
            kit.Deco(PrimitiveType.Sphere, Vector3.up * s * 0.22f, new Vector3(s * 0.6f, s * 0.42f, s * 0.42f), wrap, default(Vector3), g);
            for (int k = -1; k <= 1; k += 2)
                kit.Deco(PrimitiveType.Sphere, new Vector3(k * s * 0.4f, s * 0.22f, 0f), new Vector3(s * 0.26f, s * 0.36f, s * 0.36f), wrap, new Vector3(0f, 0f, 90f), g);
        }

        static void Coin(LevelKit kit, Transform g, float s, System.Random r)
        {
            bool silver = r.Next(3) == 0;
            var m = PatchworkAssets.Plain(silver ? new Color(0.82f, 0.84f, 0.88f) : new Color(1f, 0.78f, 0.3f), 0.85f);
            kit.Deco(PrimitiveType.Cylinder, Vector3.up * s * 0.04f, new Vector3(s, s * 0.04f, s), m, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, Vector3.up * s * 0.06f, new Vector3(s * 0.7f, s * 0.04f, s * 0.7f), PatchworkAssets.Plain(silver ? new Color(0.7f, 0.72f, 0.78f) : new Color(0.9f, 0.65f, 0.2f), 0.85f), default(Vector3), g);
        }

        static void Thumbtack(LevelKit kit, Transform g, float s, System.Random r)
        {
            kit.Deco(PrimitiveType.Sphere, Vector3.up * s * 0.12f, new Vector3(s * 0.75f, s * 0.3f, s * 0.75f), PatchworkAssets.Plain(Pick(r), 0.7f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, Vector3.up * s * 0.4f, new Vector3(s * 0.08f, s * 0.22f, s * 0.08f), PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.9f), 0.9f), default(Vector3), g);
        }

        static void CrayonItem(LevelKit kit, Transform g, float s, System.Random r)
        {
            Color c = Pick(r);
            float w = s * 0.22f;
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-s * 0.05f, w * 0.5f, 0f), new Vector3(w, s * 0.38f, w), PatchworkAssets.Plain(c, 0.35f), new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-s * 0.05f, w * 0.5f, 0f), new Vector3(w * 1.05f, s * 0.22f, w * 1.05f), PatchworkAssets.Felt(Ivory), new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.38f, w * 0.5f, 0f), new Vector3(s * 0.22f, w * 0.7f, w * 0.7f), PatchworkAssets.Plain(c, 0.35f), default(Vector3), g);
        }

        static void Marble(LevelKit kit, Transform g, float s, System.Random r)
        {
            kit.Deco(PrimitiveType.Sphere, Vector3.up * s * 0.5f, Vector3.one * s, PatchworkAssets.Plain(Pick(r), 0.95f), default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, Vector3.up * s * 0.5f, new Vector3(s * 1.01f, s * 0.25f, s * 1.01f), PatchworkAssets.Plain(Ivory, 0.95f), new Vector3(30f, 0f, 20f), g);
        }

        static void SewButton(LevelKit kit, Transform g, float s, System.Random r)
        {
            kit.Deco(PrimitiveType.Cylinder, Vector3.up * s * 0.06f, new Vector3(s, s * 0.06f, s), PatchworkAssets.Plain(Pick(r), 0.6f), default(Vector3), g);
            var hole = PatchworkAssets.Plain(new Color(0.15f, 0.12f, 0.16f), 0.2f);
            for (int k = 0; k < 4; k++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3((k % 2 == 0 ? -1f : 1f) * s * 0.13f, s * 0.12f, (k < 2 ? -1f : 1f) * s * 0.13f), new Vector3(s * 0.1f, s * 0.01f, s * 0.1f), hole, default(Vector3), g);
        }

        static void Card(LevelKit kit, Transform g, float s, System.Random r)
        {
            kit.Deco(PrimitiveType.Cube, Vector3.up * 0.012f, new Vector3(s * 0.7f, 0.02f, s), PatchworkAssets.Felt(Ivory), default(Vector3), g);
            bool red = r.Next(2) == 0;
            kit.Deco(PrimitiveType.Sphere, Vector3.up * 0.025f, new Vector3(s * 0.28f, 0.01f, s * 0.32f), PatchworkAssets.Felt(red ? new Color(0.92f, 0.2f, 0.28f) : new Color(0.12f, 0.1f, 0.14f)), new Vector3(0f, 45f, 0f), g);
        }

        static void Battery(LevelKit kit, Transform g, float s, System.Random r)
        {
            float w = s * 0.32f;
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-s * 0.15f, w * 0.5f, 0f), new Vector3(w, s * 0.3f, w), PatchworkAssets.Plain(new Color(0.15f, 0.14f, 0.18f), 0.6f), new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.25f, w * 0.5f, 0f), new Vector3(w, s * 0.15f, w), PatchworkAssets.Plain(new Color(1f, 0.6f, 0.15f), 0.6f), new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.43f, w * 0.5f, 0f), new Vector3(w * 0.35f, s * 0.04f, w * 0.35f), PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.9f), 0.9f), new Vector3(0f, 0f, 90f), g);
        }

        static void Domino(LevelKit kit, Transform g, float s, System.Random r)
        {
            kit.Deco(PrimitiveType.Cube, Vector3.up * s * 0.07f, new Vector3(s * 0.5f, s * 0.14f, s), PatchworkAssets.Plain(new Color(0.12f, 0.1f, 0.14f), 0.6f), default(Vector3), g);
            var dot = PatchworkAssets.Plain(Ivory, 0.6f);
            kit.Deco(PrimitiveType.Cube, Vector3.up * s * 0.145f, new Vector3(s * 0.4f, s * 0.01f, s * 0.03f), dot, default(Vector3), g);
            for (int half = -1; half <= 1; half += 2)
            {
                int n = r.Next(1, 5);
                for (int k = 0; k < n; k++)
                    kit.Deco(PrimitiveType.Sphere, new Vector3((k % 2 == 0 ? -1f : 1f) * s * 0.11f, s * 0.145f, half * s * 0.25f + (k < 2 ? -1f : 1f) * s * 0.1f), new Vector3(s * 0.08f, s * 0.03f, s * 0.08f), dot, default(Vector3), g);
            }
        }

        static void Strawberry(LevelKit kit, Transform g, float s, System.Random r)
        {
            kit.Deco(PrimitiveType.Sphere, Vector3.up * s * 0.45f, new Vector3(s * 0.8f, s * 0.9f, s * 0.8f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(0.92f, 0.18f, 0.25f), new Color(1f, 0.9f, 0.5f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, Vector3.up * s * 0.88f, new Vector3(s * 0.6f, s * 0.12f, s * 0.6f), PatchworkAssets.Felt(new Color(0.35f, 0.75f, 0.3f)), default(Vector3), g);
        }

        static void Sushi(LevelKit kit, Transform g, float s, System.Random r)
        {
            kit.Deco(PrimitiveType.Cube, Vector3.up * s * 0.18f, new Vector3(s, s * 0.36f, s * 0.5f), PatchworkAssets.Felt(Ivory), default(Vector3), g);
            bool tuna = r.Next(2) == 0;
            kit.Deco(PrimitiveType.Cube, Vector3.up * s * 0.42f, new Vector3(s * 1.08f, s * 0.14f, s * 0.56f),
                PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, tuna ? new Color(0.88f, 0.25f, 0.3f) : new Color(1f, 0.55f, 0.3f), tuna ? new Color(0.95f, 0.45f, 0.5f) : new Color(1f, 0.85f, 0.7f)), new Vector3(0f, 0f, 4f), g);
            kit.Deco(PrimitiveType.Cube, Vector3.up * s * 0.3f, new Vector3(s * 0.2f, s * 0.42f, s * 0.58f), PatchworkAssets.Felt(new Color(0.12f, 0.2f, 0.15f)), default(Vector3), g);
        }

        static void Teacup(LevelKit kit, Transform g, float s, System.Random r)
        {
            Color c = Pick(r);
            kit.Deco(PrimitiveType.Cylinder, Vector3.up * s * 0.03f, new Vector3(s, s * 0.03f, s), PatchworkAssets.Plain(Ivory, 0.8f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, Vector3.up * s * 0.3f, new Vector3(s * 0.66f, s * 0.25f, s * 0.66f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, Ivory, c), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, Vector3.up * s * 0.53f, new Vector3(s * 0.68f, s * 0.03f, s * 0.68f), PatchworkAssets.Plain(c, 0.8f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, Vector3.up * s * 0.54f, new Vector3(s * 0.56f, s * 0.02f, s * 0.56f), PatchworkAssets.Plain(new Color(0.55f, 0.32f, 0.18f), 0.9f), default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.4f, s * 0.32f, 0f), new Vector3(s * 0.12f, s * 0.26f, s * 0.08f), PatchworkAssets.Plain(Ivory, 0.8f), default(Vector3), g);
        }

        static void MahjongTile(LevelKit kit, Transform g, float s, System.Random r)
        {
            kit.Deco(PrimitiveType.Cube, Vector3.up * s * 0.2f, new Vector3(s * 0.7f, s * 0.4f, s), PatchworkAssets.Plain(Ivory, 0.7f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, Vector3.up * s * 0.06f, new Vector3(s * 0.71f, s * 0.12f, s * 1.01f), PatchworkAssets.Plain(new Color(0.25f, 0.65f, 0.45f), 0.7f), default(Vector3), g);
            var dot = PatchworkAssets.Plain(r.Next(2) == 0 ? new Color(0.2f, 0.55f, 0.85f) : new Color(0.9f, 0.25f, 0.3f), 0.5f);
            for (int k = 0; k < 3; k++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, s * 0.405f, (k - 1) * s * 0.28f), new Vector3(s * 0.2f, s * 0.01f, s * 0.2f), dot, default(Vector3), g);
        }

        static void Matchbox(LevelKit kit, Transform g, float s, System.Random r)
        {
            kit.Deco(PrimitiveType.Cube, Vector3.up * s * 0.16f, new Vector3(s, s * 0.32f, s * 0.62f), PatchworkAssets.Felt(new Color(0.9f, 0.22f, 0.25f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, Vector3.up * s * 0.32f, new Vector3(s * 0.6f, s * 0.01f, s * 0.4f), PatchworkAssets.Felt(new Color(1f, 0.85f, 0.3f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, Vector3.up * s * 0.325f, new Vector3(s * 0.2f, s * 0.01f, s * 0.2f), PatchworkAssets.Felt(new Color(0.2f, 0.4f, 0.75f)), default(Vector3), g);
        }

        static void Lollipop(LevelKit kit, Transform g, float s, System.Random r)
        {
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-s * 0.15f, s * 0.08f, 0f), new Vector3(s * 0.6f, s * 0.08f, s * 0.6f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, Pick(r), Ivory), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.3f, s * 0.05f, 0f), new Vector3(s * 0.06f, s * 0.3f, s * 0.06f), PatchworkAssets.Felt(Ivory), new Vector3(0f, 0f, 90f), g);
        }

        static void RubberDuck(LevelKit kit, Transform g, float s, System.Random r)
        {
            var y = PatchworkAssets.Plain(new Color(1f, 0.85f, 0.2f), 0.75f);
            kit.Deco(PrimitiveType.Sphere, Vector3.up * s * 0.28f, new Vector3(s * 0.75f, s * 0.55f, s), y, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, s * 0.68f, s * 0.22f), Vector3.one * s * 0.45f, y, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, s * 0.64f, s * 0.48f), new Vector3(s * 0.22f, s * 0.08f, s * 0.2f), PatchworkAssets.Plain(new Color(1f, 0.5f, 0.15f), 0.7f), default(Vector3), g);
            for (int k = -1; k <= 1; k += 2)
                kit.Deco(PrimitiveType.Sphere, new Vector3(k * s * 0.12f, s * 0.76f, s * 0.4f), Vector3.one * s * 0.07f, PatchworkAssets.Plain(UIKit.Ink, 0.7f), default(Vector3), g);
        }

        static void SpinTop(LevelKit kit, Transform g, float s, System.Random r)
        {
            kit.Deco(PrimitiveType.Sphere, Vector3.up * s * 0.4f, new Vector3(s, s * 0.5f, s), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, Pick(r), Pick(r)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, Vector3.up * s * 0.75f, new Vector3(s * 0.1f, s * 0.15f, s * 0.1f), PatchworkAssets.Wood(new Color(0.8f, 0.6f, 0.4f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, Vector3.up * s * 0.12f, new Vector3(s * 0.25f, s * 0.25f, s * 0.25f), PatchworkAssets.Wood(new Color(0.8f, 0.6f, 0.4f)), default(Vector3), g);
        }

        static void ToyCar(LevelKit kit, Transform g, float s, System.Random r)
        {
            Color c = Pick(r);
            kit.Deco(PrimitiveType.Cube, Vector3.up * s * 0.25f, new Vector3(s * 0.5f, s * 0.22f, s), PatchworkAssets.Plain(c, 0.85f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, s * 0.45f, -s * 0.05f), new Vector3(s * 0.44f, s * 0.2f, s * 0.5f), PatchworkAssets.Plain(Color.Lerp(c, Color.white, 0.4f), 0.85f), default(Vector3), g);
            var tyre = PatchworkAssets.Plain(new Color(0.12f, 0.1f, 0.14f), 0.3f);
            for (int k = 0; k < 4; k++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3((k % 2 == 0 ? -1f : 1f) * s * 0.26f, s * 0.13f, (k < 2 ? -1f : 1f) * s * 0.32f), new Vector3(s * 0.26f, s * 0.05f, s * 0.26f), tyre, new Vector3(0f, 0f, 90f), g);
        }

        // ---------------------------------------------------------------- A rainbow over the box

        static void Rainbow(LevelKit kit)
        {
            Color[] bands =
            {
                new Color(0.95f, 0.3f, 0.35f), new Color(1f, 0.6f, 0.25f), new Color(1f, 0.88f, 0.3f), new Color(0.5f, 0.88f, 0.4f),
                new Color(0.35f, 0.7f, 1f), new Color(0.45f, 0.45f, 0.95f), new Color(0.75f, 0.45f, 0.95f),
            };
            Vector3 c = new Vector3(0f, -30f, 170f);
            const int segs = 36;
            var root = kit.Group("Rainbow", c);
            for (int bnd = 0; bnd < bands.Length; bnd++)
            {
                float radius = 120f - bnd * 4f;
                var m = PatchworkAssets.Glow(bands[bnd], 0.3f);
                for (int i = 0; i < segs; i++)
                {
                    float a = Mathf.PI * (i + 0.5f) / segs;
                    Vector3 p = new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f);
                    var t = kit.Deco(PrimitiveType.Cube, p, new Vector3(4.1f, Mathf.PI * radius / segs * 1.04f, 2f), m, new Vector3(0f, 0f, a * Mathf.Rad2Deg), root);
                    t.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
            // Fluffy felt clouds where it lands.
            for (int s = -1; s <= 1; s += 2)
            {
                var cl = kit.Cloud(c + new Vector3(s * 110f, 32f, 0f), 9f);
                cl.SetParent(root, true);
            }
        }
    }
}

using UnityEngine;

namespace Patchwork
{
    // The third pass over the Oceanic Museum: life, and grandeur.
    //   Life:  the night staff and the stowaways (a watchman doing his rounds with a lantern,
    //          another asleep at his post, a cleaner, a painter, a busker, a keeper feeding the
    //          seals, a school party of ducklings), and the animals (gulls, mice, seals,
    //          leaping fish, a heron, a diver cleaning the tunnel glass, lanternfish).
    //   Grand: a sea-creature carousel on the quay, a crystal chandelier over the doors, two
    //          great stone Keepers flanking the Great Window, a gilded wave frieze round the
    //          atrium, signs over every doorway, and the café under the west gallery.
    public static partial class Level2Builder
    {
        static void Life(LevelKit kit)
        {
            Carousel(kit, new Vector3(-38f, 0f, -134f));
            QuayLife(kit);
            AtriumGrandeur(kit);
            AtriumLife(kit);
            Cafe(kit);
            HistoryLife(kit);
            KelpLife(kit);
            DeepLife(kit);
            RuinsLife(kit);
            ArmsLife(kit);
            Signs(kit);
            Grandeur(kit);
            Undersea(kit);
            Lighting(kit);
            Cinematics(kit);
        }

        // ------------------------------------------------------------------ Little figures

        // A toy figure built from felt (for the ones who walk about, so can't be ToyNpcs).
        // Faces +z. Returns the head.
        static Transform Figure(LevelKit kit, Transform g, Color coat, Color hat, float scale, bool cap)
        {
            var c = PatchworkAssets.Felt(coat);
            var skin = PatchworkAssets.Felt(new Color(0.96f, 0.84f, 0.72f));
            var dark = PatchworkAssets.Plain(new Color(0.08f, 0.07f, 0.1f), 0.6f);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Capsule, new Vector3(s * 0.16f, 0.35f, 0f) * scale, new Vector3(0.22f, 0.36f, 0.22f) * scale, PatchworkAssets.Felt(coat * 0.6f), default(Vector3), g);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.16f, 0.06f, 0.06f) * scale, new Vector3(0.22f, 0.12f, 0.32f) * scale, dark, default(Vector3), g);
                kit.Deco(PrimitiveType.Capsule, new Vector3(s * 0.36f, 0.95f, 0.04f) * scale, new Vector3(0.16f, 0.3f, 0.16f) * scale, c, new Vector3(-10f, 0f, s * 10f), g);
            }
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.98f, 0f) * scale, new Vector3(0.6f, 0.46f, 0.46f) * scale, c, default(Vector3), g);
            for (int b = 0; b < 3; b++)
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, (0.82f + b * 0.14f) * scale, 0.23f * scale), Vector3.one * 0.06f * scale, Brass, default(Vector3), g));
            var head = new GameObject("Head").transform;
            head.SetParent(g, false);
            head.localPosition = new Vector3(0f, 1.62f, 0f) * scale;
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.5f * scale, skin, default(Vector3), head);
            for (int s = -1; s <= 1; s += 2)
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.1f, 0.04f, 0.22f) * scale, Vector3.one * 0.07f * scale, dark, default(Vector3), head));
            var hm = PatchworkAssets.Felt(hat);
            if (cap)
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.2f, 0f) * scale, new Vector3(0.52f, 0.1f, 0.52f) * scale, hm, default(Vector3), head);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.13f, 0.24f) * scale, new Vector3(0.36f, 0.03f, 0.2f) * scale, PatchworkAssets.Plain(new Color(0.1f, 0.1f, 0.12f), 0.8f), new Vector3(-10f, 0f, 0f), head);
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.22f, 0.25f) * scale, Vector3.one * 0.08f * scale, Brass, default(Vector3), head));
            }
            else kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.12f, -0.04f) * scale, new Vector3(0.54f, 0.34f, 0.56f) * scale, hm, default(Vector3), head);
            return head;
        }

        // A felt duck (the teacher wears a scarf and carries a flag). Faces +z.
        static Transform Duck(LevelKit kit, Vector3 at, float size, Color body, bool teacher)
        {
            var g = kit.Group(teacher ? "DuckTeacher" : "Duckling", at);
            var m = PatchworkAssets.Felt(body);
            var beak = PatchworkAssets.Felt(new Color(1f, 0.6f, 0.2f));
            var dark = PatchworkAssets.Plain(new Color(0.08f, 0.07f, 0.1f), 0.6f);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.32f, 0f) * size, new Vector3(0.6f, 0.52f, 0.8f) * size, m, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.42f, -0.42f) * size, new Vector3(0.3f, 0.24f, 0.3f) * size, m, new Vector3(-30f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.78f, 0.24f) * size, Vector3.one * 0.4f * size, m, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.74f, 0.48f) * size, new Vector3(0.2f, 0.08f, 0.24f) * size, beak, default(Vector3), g);
            for (int s = -1; s <= 1; s += 2)
            {
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.12f, 0.84f, 0.4f) * size, Vector3.one * 0.07f * size, dark, default(Vector3), g));
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.28f, 0.36f, -0.04f) * size, new Vector3(0.1f, 0.3f, 0.5f) * size, m, new Vector3(0f, 0f, s * 10f), g);
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.12f, 0.03f, 0.1f) * size, new Vector3(0.16f, 0.04f, 0.24f) * size, beak, default(Vector3), g);
            }
            if (teacher)
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.6f, 0.2f) * size, new Vector3(0.44f, 0.06f, 0.44f) * size, PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, Wine, CreamCol), default(Vector3), g);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0.32f, 1.1f, 0.2f) * size, new Vector3(0.03f, 0.6f, 0.03f) * size, DarkWood, default(Vector3), g);
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0.32f, 1.55f, 0.36f) * size, new Vector3(0.02f, 0.26f, 0.32f) * size, PatchworkAssets.Felt(new Color(1f, 0.82f, 0.4f)), default(Vector3), g));
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.0f, 0.24f) * size, new Vector3(0.36f, 0.04f, 0.36f) * size, PatchworkAssets.Felt(Navy), default(Vector3), g);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.08f, 0.24f) * size, new Vector3(0.24f, 0.08f, 0.24f) * size, PatchworkAssets.Felt(Navy), default(Vector3), g);
            }
            return g;
        }

        // A felt cat curled up asleep. Faces +z.
        static void SleepingCat(LevelKit kit, Vector3 at, float yaw, Color fur)
        {
            var g = kit.Group("SleepingCat", at);
            g.rotation = Rot(yaw);
            var m = PatchworkAssets.Yarn(fur);
            var body = kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.18f, 0f), new Vector3(0.62f, 0.36f, 0.5f), m, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.24f, 0.2f, 0.18f), new Vector3(0.3f, 0.26f, 0.28f), m, default(Vector3), g);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(0.24f + s * 0.08f, 0.35f, 0.16f), new Vector3(0.07f, 0.1f, 0.03f), m, new Vector3(0f, 0f, 45f), g);
            kit.Deco(PrimitiveType.Capsule, new Vector3(-0.08f, 0.08f, 0.28f), new Vector3(0.1f, 0.3f, 0.1f), m, new Vector3(0f, 0f, 90f), g);
            g.gameObject.AddComponent<SleepyBreath>().Body = body;
        }

        // A little stream of Zs rising from a sleeper.
        static void Snores(LevelKit kit, Vector3 from)
        {
            var g = kit.Group("Zzz", from);
            var mat = PatchworkAssets.Glow(new Color(0.85f, 0.9f, 1f), 1.4f);
            var zs = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                var z = new GameObject("Z").transform;
                z.SetParent(g, false);
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.9f, 0f), new Vector3(2f, 0.4f, 0.2f), mat, default(Vector3), z));
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.9f, 0f), new Vector3(2f, 0.4f, 0.2f), mat, default(Vector3), z));
                NoShadow(kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(0.4f, 2.4f, 0.2f), mat, new Vector3(0f, 0f, -40f), z));
                zs[i] = z;
            }
            var s = g.gameObject.AddComponent<BubbleStream>();
            s.Bubbles = zs;
            s.Height = 1.8f;
            s.Speed = 0.5f;
            s.Wobble = 0.3f;
        }

        static void Talkative(LevelKit kit, Vector3 head, Vector3 stand, string name, params string[] lines)
        {
            var go = new GameObject("Sleeper_" + name);
            go.transform.SetParent(kit.Root, false);
            go.transform.position = head;
            var npc = go.AddComponent<SleepyNpc>();
            npc.Title = name;
            npc.Text = lines;
            npc.Head = head;
            var zone = kit.Volume<TalkZone>(stand + Vector3.up * 1.2f, new Vector3(2.8f, 3f, 2.8f), "Nudge_" + name);
            zone.Npc = npc;
        }

        // A string of little lamps hanging in a curve from a to b.
        static void LampString(LevelKit kit, Vector3 a, Vector3 b, float sag, int bulbs, MeshBatch[] into)
        {
            var wire = PatchworkAssets.Plain(new Color(0.1f, 0.1f, 0.14f), 0.3f);
            Vector3 prev = a;
            for (int k = 1; k <= bulbs + 1; k++)
            {
                float u = k / (float)(bulbs + 1);
                Vector3 p = Vector3.Lerp(a, b, u) + Vector3.down * Mathf.Sin(u * Mathf.PI) * sag;
                kit.Rope(prev, p, wire, 0.025f);
                if (k <= bulbs) into[k % into.Length].Ball(p + Vector3.down * 0.12f, Vector3.one * 0.2f, 3, 6);
                prev = p;
            }
        }

        // ------------------------------------------------------------------ The carousel

        // A carousel of sea creatures on the quay, turning to its own waltz. You can ride it.
        static void Carousel(LevelKit kit, Vector3 c)
        {
            // The fixed stone step it stands on.
            RoundPlatform(kit, c + Vector3.up * 0.3f, 7.6f, 0.3f, PatchworkAssets.Cobble(new Color(0.6f, 0.6f, 0.7f)), PaleStone, false);
            var turn = kit.Group("Carousel", c + Vector3.up * 0.3f);
            var deck = RoundPlatform(kit, c + Vector3.up * 0.62f, 6.9f, 0.32f, StarCloth, PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, Wine, new Color(1f, 0.82f, 0.4f)), true);
            deck.SetParent(turn, true);
            // The middle: a drum of mirrors and brass, up to the canopy.
            var mirror = PatchworkAssets.Glow(new Color(0.75f, 0.85f, 1f), 0.9f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 2.9f, 0f), new Vector3(2.6f, 2.6f, 2.6f), PatchworkAssets.Fabric(Navy), default(Vector3), turn);
            for (int i = 0; i < 12; i++)
            {
                float a = i / 12f * Mathf.PI * 2f;
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(Mathf.Cos(a) * 1.31f, 2.9f, Mathf.Sin(a) * 1.31f), new Vector3(0.5f, 1.8f, 0.04f), i % 2 == 0 ? mirror : Brass,
                    new Vector3(0f, 90f - a * Mathf.Rad2Deg, 0f), turn));
            }
            foreach (float y in new[] { 0.5f, 1.9f, 3.9f, 5.3f })
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, y, 0f), new Vector3(2.8f, 0.08f, 2.8f), Brass, default(Vector3), turn);
            // The canopy: striped on top, stars underneath, scalloped round the edge.
            var canopyTop = Dome(kit, c + Vector3.up * 5.8f, new Vector3(7.3f, 2.6f, 7.3f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, Wine, CreamCol), false, "CarouselCanopy", 6, 24);
            canopyTop.SetParent(turn, true);
            var canopyUnder = Dome(kit, c + Vector3.up * 5.78f, new Vector3(7.25f, 2.55f, 7.25f), StarCloth, true, "CarouselCanopyUnder", 6, 24);
            canopyUnder.SetParent(turn, true);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 5.5f, 0f), new Vector3(14.8f, 0.3f, 14.8f), Brass, default(Vector3), turn);
            var bulbsA = new MeshBatch();
            var bulbsB = new MeshBatch();
            for (int i = 0; i < 24; i++)
            {
                float a = (i + 0.5f) / 24f * Mathf.PI * 2f;
                var flag = i % 2 == 0 ? PatchworkAssets.Felt(new Color(1f, 0.82f, 0.4f)) : PatchworkAssets.Felt(Navy);
                kit.Deco(PrimitiveType.Cube, new Vector3(Mathf.Cos(a) * 7.42f, 5.05f, Mathf.Sin(a) * 7.42f), new Vector3(1.4f, 0.7f, 0.04f), flag, new Vector3(0f, 90f - a * Mathf.Rad2Deg, 45f), turn);
                float b = i / 24f * Mathf.PI * 2f;
                (i % 2 == 0 ? bulbsA : bulbsB).Ball(new Vector3(Mathf.Cos(b) * 7.5f, 5.55f, Mathf.Sin(b) * 7.5f), Vector3.one * 0.28f, 3, 6);
                // More round the top of the canopy, and on the rays across its underside.
                (i % 2 == 0 ? bulbsB : bulbsA).Ball(new Vector3(Mathf.Cos(b) * 3.6f, 7.65f, Mathf.Sin(b) * 3.6f), Vector3.one * 0.22f, 3, 6);
                if (i % 3 == 0)
                    for (int k = 0; k < 4; k++)
                        (k % 2 == 0 ? bulbsA : bulbsB).Ball(new Vector3(Mathf.Cos(b) * (2.4f + k * 1.2f), 7.4f - k * 0.5f - 0.2f, Mathf.Sin(b) * (2.4f + k * 1.2f)), Vector3.one * 0.18f, 3, 6);
            }
            var lampA = PatchworkAssets.Unique(PatchworkAssets.Glow(new Color(1f, 0.85f, 0.5f), 3f));
            var lampB = PatchworkAssets.Unique(PatchworkAssets.Glow(new Color(1f, 0.85f, 0.5f), 3f));
            var ta = bulbsA.Build(turn, "CarouselBulbs", lampA);
            var tb = bulbsB.Build(turn, "CarouselBulbs", lampB);
            for (int k = 0; k < 2; k++)
            {
                var host = k == 0 ? ta : tb;
                if (host == null) continue;
                var tw = host.gameObject.AddComponent<StarTwinkle>();
                tw.Mat = k == 0 ? lampA : lampB;
                tw.Tint = new Color(1f, 0.85f, 0.5f);
                tw.Low = 0.7f;
                tw.High = 3.4f;
                tw.Speed = 3.2f;
                tw.Phase = k * Mathf.PI;
            }
            // The crown on top: a brass ball and a pennant.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 8.6f, 0f), new Vector3(0.5f, 0.3f, 0.5f), Brass, default(Vector3), turn);
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 9.2f, 0f), Vector3.one * 0.8f, PatchworkAssets.Glow(new Color(1f, 0.85f, 0.45f), 2.4f), default(Vector3), turn));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 10.2f, 0f), new Vector3(0.06f, 0.8f, 0.06f), Brass, default(Vector3), turn);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0.6f, 10.6f, 0f), new Vector3(1.2f, 0.5f, 0.03f), PatchworkAssets.Felt(new Color(0.4f, 0.8f, 0.85f)), default(Vector3), turn));
            // The mounts: sea creatures on twisted brass poles, rising and falling.
            Swimmer[] kinds = { Swimmer.Fish, Swimmer.Turtle, Swimmer.Whale, Swimmer.Tall, Swimmer.Ray, Swimmer.Shark, Swimmer.Fish, Swimmer.Turtle, Swimmer.Whale, Swimmer.Tall };
            float[] sizes = { 1.3f, 0.95f, 0.19f, 1.25f, 0.8f, 0.55f, 1.3f, 0.95f, 0.19f, 1.25f };
            Color[] bodies = { new Color(1f, 0.6f, 0.4f), new Color(0.5f, 0.8f, 0.6f), new Color(0.5f, 0.6f, 0.95f), new Color(1f, 0.8f, 0.4f), new Color(0.75f, 0.6f, 0.95f),
                               new Color(0.6f, 0.75f, 0.85f), new Color(1f, 0.55f, 0.7f), new Color(0.95f, 0.75f, 0.5f), new Color(0.45f, 0.75f, 0.9f), new Color(0.6f, 0.9f, 0.85f) };
            var mounts = new Transform[kinds.Length + 5];
            for (int i = 0; i < kinds.Length; i++)
            {
                float a = i / (float)kinds.Length * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Cos(a) * 5.1f, 0f, Mathf.Sin(a) * 5.1f);
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 2.9f, new Vector3(0.12f, 2.6f, 0.12f), Brass, default(Vector3), turn);
                for (int k = 0; k < 9; k++)
                    kit.Deco(PrimitiveType.Sphere, p + Vector3.up * (0.6f + k * 0.55f), new Vector3(0.18f, 0.12f, 0.18f), Brass, default(Vector3), turn);
                var mount = new GameObject("Mount").transform;
                mount.SetParent(turn, false);
                mount.localPosition = p + Vector3.up * 1.7f;
                mount.localRotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, p).normalized, Vector3.up);
                StitchSwimmer(kit, mount, kinds[i], sizes[i], bodies[i], CreamCol, true);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, kinds[i] == Swimmer.Ray ? 0.3f : 0.5f, 0f), new Vector3(0.5f, 0.12f, 0.6f), PatchworkAssets.Fabric(i % 2 == 0 ? Wine : Navy), default(Vector3), mount);
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, kinds[i] == Swimmer.Ray ? 0.32f : 0.52f, 0f), new Vector3(0.56f, 0.04f, 0.2f), Brass, default(Vector3), mount));
                kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.18f, 0.5f, 0.18f), Brass, default(Vector3), mount);
                mounts[i] = mount;
            }
            // An inner ring of seahorses.
            for (int i = 0; i < 5; i++)
            {
                float a = (i + 0.5f) / 5f * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Cos(a) * 2.9f, 0f, Mathf.Sin(a) * 2.9f);
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 2.9f, new Vector3(0.1f, 2.6f, 0.1f), Brass, default(Vector3), turn);
                var mount = new GameObject("Seahorse").transform;
                mount.SetParent(turn, false);
                mount.localPosition = p + Vector3.up * 1.6f;
                mount.localRotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, p).normalized, Vector3.up);
                var hm = PatchworkAssets.Felt(i % 2 == 0 ? new Color(1f, 0.75f, 0.35f) : new Color(1f, 0.6f, 0.7f));
                for (int b = 0; b < 7; b++)
                    kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.4f + b * 0.2f, Mathf.Sin(b * 0.9f) * 0.12f - (b < 2 ? 0.1f : 0f)), Vector3.one * (0.24f + Mathf.Sin(b / 6f * Mathf.PI) * 0.22f), hm, default(Vector3), mount);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.92f, 0.3f), new Vector3(0.12f, 0.12f, 0.4f), hm, default(Vector3), mount);
                for (int f = 0; f < 4; f++)
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.9f - f * 0.22f, -0.22f), new Vector3(0.04f, 0.16f, 0.2f), PatchworkAssets.Felt(CreamCol), new Vector3(30f, 0f, 0f), mount);
                for (int s = -1; s <= 1; s += 2)
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.12f, 0.86f, 0.12f), Vector3.one * 0.08f, PatchworkAssets.Plain(new Color(0.08f, 0.07f, 0.1f), 0.8f), default(Vector3), mount));
                mounts[kinds.Length + i] = mount;
            }
            var bob = turn.gameObject.AddComponent<CarouselBob>();
            bob.Mounts = mounts;
            var spin = turn.gameObject.AddComponent<Spinner>();
            spin.Speed = 13f;
            turn.gameObject.AddComponent<Carrier>();
            turn.gameObject.AddComponent<Rigidbody>().isKinematic = true;
            PointLight(kit, c + Vector3.up * 4.4f, new Color(1f, 0.82f, 0.55f), 16f, 1.8f);
            Plaque(kit, c + new Vector3(8.8f, 0f, 3f), 90f, "The Carousel",
                "Carousels began as practice for knights: riders galloped in a circle trying to spear a ring. By the 1800s someone had the good sense to swap the real horses for carved ones and add music.",
                "This one has no horses at all. The museum's carpenters thought the sea creatures deserved a turn.");
            kit.Hint(c + new Vector3(9f, 1.5f, -4f), new Vector3(6f, 4f, 6f), "The Carousel", "Hop on and take a ride. It never stops.");
            Trail(kit, 10, c + new Vector3(0f, 1.9f, 6.2f), c + new Vector3(4.4f, 1.9f, 4.4f), c + new Vector3(6.2f, 1.9f, 0f), c + new Vector3(4.4f, 1.9f, -4.4f), c + new Vector3(0f, 1.9f, -6.2f), c + new Vector3(-4.4f, 1.9f, -4.4f));
        }

        // ------------------------------------------------------------------ The quay

        static void QuayLife(LevelKit kit)
        {
            // Gulls wheeling over the quay, and two perched on the railing posts.
            Color gull = new Color(0.96f, 0.96f, 0.98f);
            for (int i = 0; i < 4; i++)
            {
                var bird = Gull(kit, Vector3.zero, i % 2 == 0 ? 1f : 0.85f, gull, true, i);
                var mover = bird.gameObject.AddComponent<PathMover>();
                mover.BobHeight = 1.2f;
                mover.BobSpeed = 0.5f;
                mover.Sway = 14f;
                mover.Setup(PathMover.Oval(new Vector3(i % 2 == 0 ? 0f : -10f, 20f + i * 3.5f, -142f + i * 4f), 32f + i * 5f, 16f + i * 2f, 20), 5f + i * 0.6f, i * 28f);
            }
            Gull(kit, new Vector3(-59f, 4.95f, -123f), 0.9f, gull, false, 7).rotation = Rot(80f);
            Gull(kit, new Vector3(59f, 4.95f, -149f), 0.9f, gull, false, 8).rotation = Rot(-110f);
            // Ducks pottering round the fountains.
            foreach (var p in new[] { new Vector3(20f, 0f, -141f), new Vector3(27.5f, 0f, -128.5f), new Vector3(-20.5f, 0f, -140f) })
            {
                var d = ToyCritter.Duck(kit, p);
                d.Home = p;
                d.Radius = 3.5f;
            }
            // A cat asleep on the crates, and a busker playing for the gulls.
            SleepingCat(kit, new Vector3(32.4f, 2.62f, -152f), 30f, new Color(0.95f, 0.6f, 0.3f));
            var busker = Level1Builder.Toy(kit, new Vector3(-28.5f, 0f, -125.5f), 230f, ToyNpc.Kind.Teddy, "Strummer", new Color(0.55f, 0.35f, 0.3f), new Color(0.9f, 0.75f, 0.35f), ToyNpc.Instrument.Lute, 0.9f, Plum);
            busker.Lines = () => new[]
            {
                "Evening! I play for the carousel. It goes round, I play the tune, everyone's happy.",
                "The gulls don't tip, mind you. Gulls never tip.",
                "Have you ridden it yet? Sit on the whale. The whale's the best one."
            };
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(-27.3f, 0.1f, -124.6f), new Vector3(0.7f, 0.1f, 0.7f), PatchworkAssets.Felt(Wine)));
            for (int i = 0; i < 4; i++)
                NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(-27.3f + (i % 2) * 0.16f, 0.2f + i * 0.02f, -124.6f + (i / 2) * 0.14f), new Vector3(0.14f, 0.01f, 0.14f), Brass));
            // Fountain sounds, and lamps strung round the carousel.
            foreach (float x in new[] { -24f, 24f }) AmbientEmitter.Create(kit.Root, new Vector3(x, 2f, -134f), Sfx.Ambient(Sfx.AmbientClip.Fountain), 0.45f, 4f, 22f);
            Color[] bulbs = { new Color(1f, 0.8f, 0.4f), new Color(1f, 0.5f, 0.5f), new Color(0.5f, 0.8f, 1f), new Color(0.7f, 1f, 0.6f) };
            var batch = new MeshBatch[bulbs.Length];
            for (int i = 0; i < batch.Length; i++) batch[i] = new MeshBatch();
            Vector3 cc = new Vector3(-38f, 0f, -134f);
            var poleAt = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                float a = i / 6f * Mathf.PI * 2f;
                poleAt[i] = cc + new Vector3(Mathf.Cos(a) * 10.5f, 0f, Mathf.Sin(a) * 10.5f);
                kit.Deco(PrimitiveType.Cylinder, poleAt[i] + Vector3.up * 2.4f, new Vector3(0.14f, 2.4f, 0.14f), IronMat);
                kit.Deco(PrimitiveType.Cylinder, poleAt[i] + Vector3.up * 0.1f, new Vector3(0.6f, 0.1f, 0.6f), IronMat);
                NoShadow(kit.Deco(PrimitiveType.Sphere, poleAt[i] + Vector3.up * 4.9f, Vector3.one * 0.3f, Brass));
                TownKit.Solid(kit, poleAt[i] + Vector3.up * 2.4f, new Vector3(0.3f, 4.8f, 0.3f), 0f);
            }
            for (int i = 0; i < 6; i++)
                LampString(kit, poleAt[i] + Vector3.up * 4.7f, poleAt[(i + 1) % 6] + Vector3.up * 4.7f, 1f, 9, batch);
            for (int i = 0; i < batch.Length; i++) batch[i].Build(kit.Root, "CarouselLights", PatchworkAssets.Glow(bulbs[i], 2.6f));
        }

        // A gull: white felt, grey wings, yellow beak. Faces +z.
        static Transform Gull(LevelKit kit, Vector3 at, float size, Color col, bool flying, int seed)
        {
            var g = kit.Group("Gull", at);
            var m = PatchworkAssets.Felt(col);
            var grey = PatchworkAssets.Felt(new Color(0.6f, 0.64f, 0.72f));
            var beak = PatchworkAssets.Felt(new Color(1f, 0.8f, 0.25f));
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.25f, 0f) * size, new Vector3(0.42f, 0.4f, 0.8f) * size, m, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.48f, 0.36f) * size, Vector3.one * 0.3f * size, m, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.44f, 0.58f) * size, new Vector3(0.08f, 0.08f, 0.22f) * size, beak, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.28f, -0.48f) * size, new Vector3(0.26f, 0.04f, 0.26f) * size, grey, new Vector3(0f, 45f, 0f), g);
            for (int s = -1; s <= 1; s += 2)
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.1f, 0.52f, 0.46f) * size, Vector3.one * 0.06f * size, PatchworkAssets.Plain(new Color(0.08f, 0.07f, 0.1f), 0.7f), default(Vector3), g));
            var wings = new Transform[2];
            for (int s = 0; s < 2; s++)
            {
                float side = s == 0 ? -1f : 1f;
                var w = new GameObject("Wing").transform;
                w.SetParent(g, false);
                w.localPosition = new Vector3(side * 0.18f, 0.34f, 0.02f) * size;
                if (flying)
                {
                    kit.Deco(PrimitiveType.Cube, new Vector3(side * 0.55f, 0f, 0f) * size, new Vector3(1.1f, 0.04f, 0.4f) * size, grey, default(Vector3), w);
                    kit.Deco(PrimitiveType.Cube, new Vector3(side * 1.2f, 0f, -0.06f) * size, new Vector3(0.36f, 0.04f, 0.3f) * size, PatchworkAssets.Felt(new Color(0.2f, 0.2f, 0.25f)), default(Vector3), w);
                }
                else kit.Deco(PrimitiveType.Sphere, new Vector3(side * 0.04f, 0f, -0.1f) * size, new Vector3(0.1f, 0.26f, 0.6f) * size, grey, default(Vector3), w);
                wings[s] = w;
            }
            if (flying)
            {
                var flap = g.gameObject.AddComponent<WingFlap>();
                flap.Left = wings[0];
                flap.Right = wings[1];
                flap.Speed = 6f;
                flap.Angle = 26f;
                flap.Phase = seed * 1.7f;
            }
            else
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.08f, 0.02f, 0f) * size, new Vector3(0.04f, 0.06f, 0.04f) * size, beak, default(Vector3), g);
            return g;
        }

        // ------------------------------------------------------------------ The Grand Atrium

        static void AtriumGrandeur(LevelKit kit)
        {
            // ----- The crystal chandelier over the doors -----
            Vector3 ch = new Vector3(0f, 22.6f, -103f);
            var chand = kit.Group("Chandelier", ch);
            var crystal = PatchworkAssets.Glow(new Color(0.8f, 0.9f, 1f), 1.6f);
            var pink = PatchworkAssets.Glow(new Color(1f, 0.85f, 0.95f), 1.5f);
            float[] ringR = { 3.4f, 2.5f, 1.5f };
            float[] ringY = { 0f, 1.3f, 2.5f };
            for (int r = 0; r < 3; r++)
            {
                var hoop = new GameObject("Tier").transform;
                hoop.SetParent(chand, false);
                hoop.localPosition = Vector3.up * ringY[r];
                Hoop(kit, hoop, ringR[r], 0.16f, Brass, 28 - r * 6);
                int arms = 12 - r * 3;
                for (int i = 0; i < arms; i++)
                {
                    float a = i / (float)arms * Mathf.PI * 2f;
                    Vector3 p = new Vector3(Mathf.Cos(a) * ringR[r], ringY[r], Mathf.Sin(a) * ringR[r]);
                    NoShadow(kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.25f, new Vector3(0.16f, 0.2f, 0.16f), PatchworkAssets.Felt(CreamCol), default(Vector3), chand));
                    NoShadow(kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 0.58f, new Vector3(0.14f, 0.26f, 0.14f), PatchworkAssets.Glow(new Color(1f, 0.78f, 0.4f), 3.4f), default(Vector3), chand));
                    NoShadow(kit.Deco(PrimitiveType.Cylinder, p, new Vector3(0.3f, 0.04f, 0.3f), Brass, default(Vector3), chand));
                    // Strings of crystal drops hanging from every arm, longer on the outside.
                    int drops = 3 + (2 - r);
                    for (int d = 0; d < drops; d++)
                        NoShadow(kit.Deco(PrimitiveType.Sphere, p + Vector3.down * (0.3f + d * 0.32f), new Vector3(0.14f, 0.24f, 0.14f), (i + d) % 3 == 0 ? pink : crystal, default(Vector3), chand));
                    if (r < 2)
                    {
                        var rod = Beam(kit, ch + p, ch + Vector3.up * 3.6f, 0.03f, Brass);
                        if (rod != null) rod.SetParent(chand, true);
                    }
                }
            }
            // A swag of crystal beads looping between the bottom tier's arms.
            for (int i = 0; i < 12; i++)
            {
                float a0 = i / 12f * Mathf.PI * 2f, a1 = (i + 1) / 12f * Mathf.PI * 2f;
                for (int k = 1; k < 6; k++)
                {
                    float u = k / 6f;
                    float a = Mathf.Lerp(a0, a1, u);
                    float rr = ringR[0] + 0.05f;
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * rr, -Mathf.Sin(u * Mathf.PI) * 0.8f, Mathf.Sin(a) * rr), Vector3.one * 0.12f, crystal, default(Vector3), chand));
                }
            }
            NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.down * 1.4f, new Vector3(0.6f, 1f, 0.6f), crystal, default(Vector3), chand));
            NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.up * 3.7f, new Vector3(0.9f, 0.5f, 0.9f), Brass, default(Vector3), chand));
            Beam(kit, ch + Vector3.up * 3.8f, new Vector3(ch.x, AtriumTop, ch.z), 0.1f, IronMat);
            chand.gameObject.AddComponent<Spinner>().Speed = 3f;
            var glow = PointLight(kit, ch + Vector3.down * 0.4f, new Color(1f, 0.84f, 0.6f), 30f, 2.2f);
            glow.gameObject.AddComponent<CandleFlicker>().Glow = glow;

            // ----- The Keepers of the Window: two great stone figures either side of the glass -----
            Keeper(kit, new Vector3(26.8f, 0f, -105f), 270f, true);
            Keeper(kit, new Vector3(26.8f, 0f, -57.6f), 270f, false);

            // ----- A gilded frieze of waves round the walls, under the vault -----
            var navy = PatchworkAssets.Felt(new Color(0.1f, 0.12f, 0.3f));
            var goldBatch = new MeshBatch();
            System.Action<Vector3, Vector3> Frieze = (a, b) =>
            {
                Vector3 d = b - a;
                float len = d.magnitude;
                Vector3 dir = d / len;
                Vector3 outN = Vector3.Cross(Vector3.up, dir);
                NoShadow(kit.Deco(PrimitiveType.Cube, (a + b) * 0.5f, new Vector3(0.1f, 1.4f, len), navy, new Vector3(0f, Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg, 0f)));
                int waves = Mathf.RoundToInt(len / 1.6f);
                Vector3 prev = a + Vector3.down * 0.2f + outN * 0.07f;
                for (int i = 1; i <= waves * 6; i++)
                {
                    float u = i / (float)(waves * 6);
                    float ph = u * waves * Mathf.PI * 2f;
                    Vector3 p = a + d * u + Vector3.up * (Mathf.Sin(ph) * 0.35f - 0.1f) + outN * 0.07f;
                    goldBatch.Box((p + prev) * 0.5f, new Vector3(0.09f, 0.09f, (p - prev).magnitude + 0.04f), Quaternion.LookRotation((p - prev).normalized, outN));
                    if (i % 6 == 3) goldBatch.Ball(p + Vector3.up * 0.34f, new Vector3(0.14f, 0.14f, 0.05f), 2, 5);
                    prev = p;
                }
                goldBatch.Box(a + d * 0.5f + Vector3.up * 0.62f + outN * 0.07f, new Vector3(0.05f, 0.08f, len), Quaternion.LookRotation(dir, Vector3.up));
                goldBatch.Box(a + d * 0.5f + Vector3.down * 0.62f + outN * 0.07f, new Vector3(0.05f, 0.08f, len), Quaternion.LookRotation(dir, Vector3.up));
            };
            Frieze(new Vector3(-29.94f, 20f, -107.6f), new Vector3(-29.94f, 20f, -48.4f));
            Frieze(new Vector3(29.94f, 23.2f, -48.6f), new Vector3(29.94f, 23.2f, -107.6f));   // (above the Great Window)
            Frieze(new Vector3(29.6f, 20f, -107.94f), new Vector3(-29.6f, 20f, -107.94f));
            goldBatch.Build(kit.Root, "WaveFrieze", PatchworkAssets.Glow(new Color(1f, 0.8f, 0.4f), 0.9f));

            // ----- Garlands of little lights along the gallery rails -----
            Color[] bulbs = { new Color(1f, 0.85f, 0.5f), new Color(0.8f, 0.85f, 1f) };
            var lights = new MeshBatch[bulbs.Length];
            for (int i = 0; i < lights.Length; i++) lights[i] = new MeshBatch();
            for (float z = AtriumZ0 + 0.5f; z < -55f; z += 8.33f)
                LampString(kit, new Vector3(-24f, 9.15f, z), new Vector3(-24f, 9.15f, Mathf.Min(z + 8.33f, -54.2f)), 0.5f, 7, lights);
            LampString(kit, new Vector3(-24f, 9.15f, -54f), new Vector3(-18.4f, 9.15f, -54f), 0.4f, 5, lights);
            LampString(kit, new Vector3(18.4f, 9.15f, -54f), new Vector3(29.7f, 9.15f, -54f), 0.5f, 9, lights);
            for (int i = 0; i < lights.Length; i++) lights[i].Build(kit.Root, "GalleryLights", PatchworkAssets.Glow(bulbs[i], 2.4f));
        }

        // A colossal stone figure in a long robe on a shell-carved plinth, one hand raised
        // holding up a glowing pearl (or a star). Faces "yaw". Solid at the plinth.
        static void Keeper(LevelKit kit, Vector3 foot, float yaw, bool pearl)
        {
            var stone = PatchworkAssets.Plaster(new Color(0.9f, 0.88f, 0.82f));
            var shade = PatchworkAssets.Plaster(new Color(0.78f, 0.76f, 0.72f));
            Vector3 top = Plinth(kit, foot, new Vector3(3f, 2.2f, 3f), yaw);
            // Scallop shells carved round the plinth.
            for (int i = 0; i < 4; i++)
            {
                var r = Rot(yaw + i * 90f);
                var sh = kit.Group("Shell", foot + r * new Vector3(0f, 1.2f, 1.56f));
                sh.rotation = r;
                for (int k = 0; k < 7; k++)
                    NoShadow(kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(0.18f, 1.1f, 0.06f), shade, new Vector3(0f, 0f, -54f + k * 18f), sh));
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.1f, 0.02f), new Vector3(0.5f, 0.3f, 0.06f), Brass, default(Vector3), sh));
            }
            var g = kit.Group("Keeper", top);
            g.rotation = Rot(yaw);
            // The robe: a long bell of stone, folded.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.6f, 0f), new Vector3(2f, 1.6f, 1.7f), stone, default(Vector3), g);
            for (int i = 0; i < 9; i++)
            {
                float a = i / 9f * Mathf.PI * 2f;
                kit.Deco(PrimitiveType.Capsule, new Vector3(Mathf.Cos(a) * 0.9f, 1.4f, Mathf.Sin(a) * 0.76f), new Vector3(0.3f, 1.5f, 0.3f), i % 2 == 0 ? stone : shade, default(Vector3), g);
            }
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 3.9f, 0f), new Vector3(1.4f, 1.4f, 1.1f), stone, default(Vector3), g);
            // A cloak over the shoulders, falling behind in a long train.
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 3.1f, -0.62f), new Vector3(1.8f, 3.4f, 0.2f), shade, new Vector3(8f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.4f, -1.1f), new Vector3(2f, 0.3f, 1.2f), shade, new Vector3(-6f, 0f, 0f), g);
            // Head, with a cap of curls and a circlet of brass stars.
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 5.25f, 0f), new Vector3(0.8f, 0.9f, 0.82f), stone, default(Vector3), g);
            for (int i = 0; i < 10; i++)
            {
                float a = i / 10f * Mathf.PI * 2f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.34f, 5.5f, Mathf.Sin(a) * 0.34f - 0.05f), Vector3.one * 0.34f, shade, default(Vector3), g);
                if (i % 2 == 0)
                    NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.44f, 5.36f, Mathf.Sin(a) * 0.44f), Vector3.one * 0.12f, Brass, default(Vector3), g));
            }
            // Closed eyes, gently smiling.
            var line = PatchworkAssets.Plaster(new Color(0.55f, 0.52f, 0.48f));
            for (int s = -1; s <= 1; s += 2)
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.15f, 5.28f, 0.4f), new Vector3(0.16f, 0.03f, 0.03f), line, new Vector3(0f, 0f, s * -10f), g));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 5.04f, 0.4f), new Vector3(0.14f, 0.03f, 0.03f), line, default(Vector3), g));
            // One arm down, holding a fold of the robe; the other raised high.
            kit.Deco(PrimitiveType.Capsule, new Vector3(-0.74f, 3.6f, 0.2f), new Vector3(0.36f, 0.9f, 0.36f), stone, new Vector3(-20f, 0f, -8f), g);
            Vector3 shoulder = new Vector3(0.7f, 4.4f, 0f), hand = new Vector3(1.2f, 6.6f, 0.4f);
            var arm = kit.Deco(PrimitiveType.Capsule, (shoulder + hand) * 0.5f, new Vector3(0.36f, Vector3.Distance(shoulder, hand) * 0.55f, 0.36f), stone, default(Vector3), g);
            arm.localRotation = Quaternion.FromToRotation(Vector3.up, (hand - shoulder).normalized);
            kit.Deco(PrimitiveType.Sphere, hand + Vector3.up * 0.1f, new Vector3(0.5f, 0.3f, 0.5f), stone, default(Vector3), g);
            Vector3 orb = hand + Vector3.up * 0.75f;
            if (pearl)
            {
                NoShadow(kit.Deco(PrimitiveType.Sphere, orb, Vector3.one * 1f, PatchworkAssets.Glow(new Color(0.85f, 0.95f, 1f), 2.6f), default(Vector3), g));
                NoShadow(kit.Deco(PrimitiveType.Sphere, orb, Vector3.one * 1.3f, PatchworkAssets.Bubble(new Color(0.8f, 0.95f, 1f, 0.15f)), default(Vector3), g));
            }
            else
            {
                var star = PatchworkAssets.Glow(new Color(1f, 0.85f, 0.45f), 2.8f);
                NoShadow(kit.Deco(PrimitiveType.Cube, orb, new Vector3(0.26f, 1.6f, 0.2f), star, default(Vector3), g));
                NoShadow(kit.Deco(PrimitiveType.Cube, orb, new Vector3(1.6f, 0.26f, 0.2f), star, default(Vector3), g));
                NoShadow(kit.Deco(PrimitiveType.Cube, orb, new Vector3(0.7f, 0.7f, 0.22f), star, new Vector3(0f, 0f, 45f), g));
            }
            // A fish curled at the feet.
            var fish = new GameObject("Fish").transform;
            fish.SetParent(g, false);
            fish.localPosition = new Vector3(-0.9f, 0.4f, 1.1f);
            fish.localRotation = Quaternion.Euler(0f, 60f, 20f);
            StitchSwimmer(kit, fish, Swimmer.Fish, 1f, new Color(0.85f, 0.83f, 0.78f), new Color(0.75f, 0.73f, 0.7f), false);
            var lamp = PointLight(kit, g.TransformPoint(orb), pearl ? new Color(0.8f, 0.9f, 1f) : new Color(1f, 0.85f, 0.5f), 18f, 1.8f);
            lamp.gameObject.AddComponent<GlowPulse>().Lamp = lamp;
            PointLight(kit, foot + Rot(yaw) * new Vector3(0f, 1f, 3.4f), new Color(1f, 0.85f, 0.65f), 12f, 1f);
            // (On the open side of each statue, away from the gallery columns.)
            Plaque(kit, foot + new Vector3(-2.6f, 0f, pearl ? 3.6f : -3.6f), yaw, pearl ? "The Keeper of the Deep" : "The Keeper of the Sky",
                pearl ? "One of the two stone Keepers who guard the Great Window. She holds up the pearl, for everything under the sea." : "One of the two stone Keepers who guard the Great Window. She holds up the star, for everything over it.",
                "The sculptor made their faces from the same model: her own grandmother, asleep in a deckchair at the seaside.");
        }

        static void AtriumLife(LevelKit kit)
        {
            // A school party of ducklings, following their teacher round and round the armillary.
            var loop = PathMover.Oval(new Vector3(0f, 0f, -86f), 7.2f, 6.2f, 24);
            var teacher = Duck(kit, Vector3.zero, 1.15f, new Color(0.95f, 0.95f, 0.92f), true);
            var tm = teacher.gameObject.AddComponent<PathMover>();
            tm.Sway = 6f;
            tm.Setup(loop, 1.3f, 0f);
            Color[] chicks = { new Color(1f, 0.88f, 0.35f), new Color(1f, 0.82f, 0.4f), new Color(1f, 0.9f, 0.5f) };
            for (int i = 0; i < 6; i++)
            {
                var d = Duck(kit, Vector3.zero, 0.55f, chicks[i % 3], false);
                var m = d.gameObject.AddComponent<PathMover>();
                m.BobHeight = 0.05f;
                m.BobSpeed = 9f;
                m.Sway = 10f;
                m.Setup(loop, 1.3f, -1.4f - i * 0.8f);
            }
            // The cleaner, with her mop, her bucket and a sign.
            Person(kit, new Vector3(6.4f, 0f, -97f), 250f, ToyNpc.Kind.RagDoll, "Mrs. Sudsbury", new Color(0.4f, 0.55f, 0.7f), new Color(1f, 0.6f, 0.6f), 1.1f, () => new[]
            {
                "Mind the wet floor, dear. Somebody tracked sand all the way in from the Ruins. Somebody with very small feet.",
                "Forty years I've mopped round that whale. She drips, you know. Nobody believes me.",
                "If you see the Professor, tell him the ducklings have been round the armillary thirty-one times and I'd like my floor back."
            });
            Vector3 mop = new Vector3(5.4f, 0f, -96.2f);
            kit.Deco(PrimitiveType.Cylinder, mop + new Vector3(0f, 1f, 0f), new Vector3(0.06f, 1f, 0.06f), DarkWood, new Vector3(0f, 0f, 12f));
            kit.Deco(PrimitiveType.Sphere, mop + new Vector3(0.2f, 0.12f, 0f), new Vector3(0.6f, 0.24f, 0.6f), PatchworkAssets.Yarn(CreamCol));
            Vector3 bucket = new Vector3(7.6f, 0f, -97.8f);
            kit.Deco(PrimitiveType.Cylinder, bucket + Vector3.up * 0.32f, new Vector3(0.66f, 0.32f, 0.66f), PatchworkAssets.Plain(new Color(0.55f, 0.6f, 0.7f), 0.7f));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, bucket + Vector3.up * 0.62f, new Vector3(0.56f, 0.02f, 0.56f), PatchworkAssets.Glow(new Color(0.6f, 0.85f, 0.9f), 0.6f)));
            for (int i = 0; i < 4; i++)
                NoShadow(kit.Deco(PrimitiveType.Sphere, bucket + new Vector3((i % 2 - 0.5f) * 0.2f, 0.7f + i * 0.04f, (i / 2 - 0.5f) * 0.2f), Vector3.one * 0.12f, PatchworkAssets.Glow(Color.white, 0.8f)));
            TownKit.Solid(kit, bucket + Vector3.up * 0.35f, new Vector3(0.66f, 0.7f, 0.66f), 0f);
            var sign = kit.Group("WetFloor", new Vector3(4.2f, 0f, -94.6f));
            sign.rotation = Rot(160f);
            var yellow = PatchworkAssets.Felt(new Color(1f, 0.85f, 0.2f));
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.5f, s * 0.15f), new Vector3(0.6f, 1f, 0.04f), yellow, new Vector3(-s * 14f, 0f, 0f), sign);   // an A: the tops lean together
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.6f, 0.15f), new Vector3(0.3f, 0.3f, 0.02f), PatchworkAssets.Felt(new Color(0.15f, 0.15f, 0.2f)), new Vector3(-14f, 0f, 45f), sign));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(5f, 0.035f, -95.6f), new Vector3(2.6f, 0.01f, 1.9f), PatchworkAssets.Plain(new Color(0.5f, 0.6f, 0.9f), 0.98f)));
            // The guard asleep at his post by the doors.
            Vector3 chair = new Vector3(9.6f, 0f, -106.4f);
            kit.Block(chair + Vector3.up * 0.25f, new Vector3(1.1f, 0.5f, 1f), WoodCol, "GuardChair", DarkWood);
            kit.Deco(PrimitiveType.Cube, chair + new Vector3(0f, 0.6f, 0.05f), new Vector3(1f, 0.14f, 0.9f), PatchworkAssets.Fabric(Wine));
            kit.Deco(PrimitiveType.Cube, chair + new Vector3(0f, 1.2f, -0.48f), new Vector3(1f, 1.4f, 0.12f), DarkWood, new Vector3(-12f, 0f, 0f));
            var guard = kit.Group("SleepingGuard", chair + Vector3.up * 0.62f);
            var coat = PatchworkAssets.Felt(new Color(0.2f, 0.25f, 0.5f));
            var body = kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.55f, -0.1f), new Vector3(0.62f, 0.5f, 0.5f), coat, new Vector3(-22f, 0f, 0f), guard);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Capsule, new Vector3(s * 0.17f, 0.08f, 0.4f), new Vector3(0.22f, 0.36f, 0.22f), PatchworkAssets.Felt(new Color(0.15f, 0.17f, 0.3f)), new Vector3(80f, 0f, 0f), guard);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.17f, -0.1f, 0.85f), new Vector3(0.2f, 0.2f, 0.32f), PatchworkAssets.Plain(new Color(0.08f, 0.07f, 0.1f), 0.7f), default(Vector3), guard);
                kit.Deco(PrimitiveType.Capsule, new Vector3(s * 0.25f, 0.5f, 0.15f), new Vector3(0.16f, 0.26f, 0.16f), coat, new Vector3(60f, 0f, s * -20f), guard);
            }
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.05f, 1.15f, -0.2f), Vector3.one * 0.44f, PatchworkAssets.Felt(new Color(0.96f, 0.84f, 0.72f)), new Vector3(-20f, 0f, 14f), guard);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.05f, 1.32f, -0.12f), new Vector3(0.5f, 0.12f, 0.5f), coat, new Vector3(28f, 0f, 14f), guard);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.05f, 1.22f, 0.12f), new Vector3(0.36f, 0.03f, 0.2f), PatchworkAssets.Plain(new Color(0.1f, 0.1f, 0.12f), 0.8f), new Vector3(40f, 0f, 14f), guard);
            guard.gameObject.AddComponent<SleepyBreath>().Body = body;
            Snores(kit, chair + new Vector3(0.3f, 2.2f, 0.1f));
            Talkative(kit, chair + Vector3.up * 1.8f, chair + new Vector3(0f, 0f, 1.6f), "Watchman Nod",
                "Mmf... s'all right, Professor... the whale's still there... I counted her...",
                "...one whale...",
                "Zzz.");
            // A painter on the north gallery, painting the whale (badly).
            Vector3 easel = new Vector3(-23.5f, 8f, -50.8f);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, easel + new Vector3(s * 0.4f, 0.9f, 0f), new Vector3(0.05f, 0.95f, 0.05f), DarkWood, new Vector3(-8f, 0f, s * -6f));
            kit.Deco(PrimitiveType.Cylinder, easel + new Vector3(0f, 0.85f, -0.36f), new Vector3(0.05f, 0.9f, 0.05f), DarkWood, new Vector3(24f, 0f, 0f));
            NoShadow(kit.Deco(PrimitiveType.Cube, easel + new Vector3(0f, 1.4f, 0.06f), new Vector3(1.3f, 0.95f, 0.05f), PatchworkAssets.Felt(CreamCol), new Vector3(-8f, 0f, 0f)));
            var sketch = new MeshBatch();
            Vector3 cv = easel + new Vector3(0f, 1.4f, 0.1f);
            for (int i = 0; i < 9; i++) sketch.Ball(cv + new Vector3(-0.45f + i * 0.11f, Mathf.Sin(i * 0.4f) * 0.06f, 0f), new Vector3(0.07f, 0.07f, 0.01f), 2, 4);
            for (int i = 2; i < 7; i++) sketch.Box(cv + new Vector3(-0.45f + i * 0.11f, -0.12f, 0f), new Vector3(0.015f, 0.2f, 0.01f), Quaternion.identity);
            sketch.Build(kit.Root, "Sketch", PatchworkAssets.Felt(new Color(0.25f, 0.25f, 0.35f)));
            NoShadow(kit.Deco(PrimitiveType.Sphere, easel + new Vector3(0.7f, 1f, 0.3f), new Vector3(0.5f, 0.04f, 0.36f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, CreamCol, new Color(0.8f, 0.3f, 0.3f))));
            Person(kit, new Vector3(-23f, 8f, -48.9f), 180f, ToyNpc.Kind.Bunny, "Dabble", new Color(0.95f, 0.9f, 0.85f), new Color(0.5f, 0.6f, 0.9f), 1.35f, () => new[]
            {
                "Shh, she's sitting very still for me. Bless her. She always does.",
                "This is my forty-first whale. They keep coming out looking like a fishbone. I think that's the whale's fault.",
                "Best view in the museum, up here. Apart from the Observatory. And the tunnel. And the Great Window. It's a good museum."
            });
            // A mouse who lives behind the café.
            var mouse = ToyCritter.Mouse(kit, new Vector3(-28.5f, 0f, -88f));
            mouse.Radius = 2.5f;
        }

        // The café under the west gallery.
        static void Cafe(LevelKit kit)
        {
            Color[] cloths = { Wine, Navy, new Color(0.25f, 0.45f, 0.42f), Plum };
            int n = 0;
            foreach (float z in new[] { -96.6f, -91f, -67f, -61.4f })
            {
                Vector3 t = new Vector3(-27.4f, 0f, z);
                kit.Deco(PrimitiveType.Cylinder, t + Vector3.up * 0.5f, new Vector3(0.12f, 0.5f, 0.12f), Brass);
                kit.Deco(PrimitiveType.Cylinder, t + Vector3.up * 0.04f, new Vector3(0.7f, 0.04f, 0.7f), Brass);
                kit.Deco(PrimitiveType.Cylinder, t + Vector3.up * 1.02f, new Vector3(1.4f, 0.04f, 1.4f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, cloths[n % 4], CreamCol));
                kit.Deco(PrimitiveType.Cylinder, t + Vector3.up * 0.9f, new Vector3(1.5f, 0.14f, 1.5f), PatchworkAssets.Felt(cloths[n % 4]));
                TownKit.Solid(kit, t + Vector3.up * 0.55f, new Vector3(1.2f, 1.1f, 1.2f), 0f);
                // Cups, a pot and a little lamp.
                var china = PatchworkAssets.Plain(CreamCol, 0.85f);
                for (int s = -1; s <= 1; s += 2)
                {
                    NoShadow(kit.Deco(PrimitiveType.Cylinder, t + new Vector3(s * 0.32f, 1.13f, 0.1f), new Vector3(0.18f, 0.08f, 0.18f), china));
                    NoShadow(kit.Deco(PrimitiveType.Cylinder, t + new Vector3(s * 0.32f, 1.07f, 0.1f), new Vector3(0.3f, 0.01f, 0.3f), china));
                    // Chairs.
                    Vector3 cpos = t + new Vector3(0f, 0f, s * 1.15f);
                    kit.Deco(PrimitiveType.Cylinder, cpos + Vector3.up * 0.55f, new Vector3(0.7f, 0.06f, 0.7f), PatchworkAssets.Fabric(cloths[(n + 1) % 4]));
                    kit.Deco(PrimitiveType.Cylinder, cpos + Vector3.up * 0.27f, new Vector3(0.06f, 0.27f, 0.06f), Brass);
                    kit.Deco(PrimitiveType.Cube, cpos + new Vector3(0f, 0.95f, s * 0.32f), new Vector3(0.6f, 0.7f, 0.06f), Brass, new Vector3(s * 8f, 0f, 0f));
                    NoShadow(kit.Deco(PrimitiveType.Cube, cpos + new Vector3(0f, 0.95f, s * 0.3f), new Vector3(0.46f, 0.56f, 0.04f), PatchworkAssets.Fabric(cloths[(n + 1) % 4]), new Vector3(s * 8f, 0f, 0f)));
                }
                NoShadow(kit.Deco(PrimitiveType.Sphere, t + new Vector3(0f, 1.24f, -0.25f), new Vector3(0.3f, 0.28f, 0.3f), china));
                NoShadow(kit.Deco(PrimitiveType.Sphere, t + new Vector3(0.05f, 1.28f, 0.32f), new Vector3(0.16f, 0.2f, 0.16f), PatchworkAssets.Glow(LampGold, 2.4f)));
                if (n == 1)
                {
                    // A cake stand.
                    kit.Deco(PrimitiveType.Cylinder, t + new Vector3(-0.1f, 1.3f, -0.3f), new Vector3(0.04f, 0.26f, 0.04f), Brass);
                    for (int k = 0; k < 2; k++)
                    {
                        NoShadow(kit.Deco(PrimitiveType.Cylinder, t + new Vector3(-0.1f, 1.18f + k * 0.28f, -0.3f), new Vector3(0.5f - k * 0.14f, 0.01f, 0.5f - k * 0.14f), china));
                        for (int c = 0; c < 3; c++)
                            NoShadow(kit.Deco(PrimitiveType.Cylinder, t + new Vector3(-0.1f + Mathf.Cos(c * 2.1f) * 0.13f, 1.24f + k * 0.28f, -0.3f + Mathf.Sin(c * 2.1f) * 0.13f), new Vector3(0.12f, 0.05f, 0.12f), PatchworkAssets.Felt(c == 0 ? new Color(1f, 0.7f, 0.75f) : c == 1 ? new Color(0.95f, 0.85f, 0.55f) : new Color(0.6f, 0.4f, 0.3f))));
                    }
                }
                n++;
            }
            // The dessert trolley and a chalk board.
            Vector3 tr = new Vector3(-28.4f, 0f, -85.4f);
            kit.Block(tr + Vector3.up * 0.55f, new Vector3(1.1f, 0.1f, 1.8f), WoodCol, "Trolley", DarkWood);
            kit.Deco(PrimitiveType.Cube, tr + Vector3.up * 1.15f, new Vector3(1.1f, 0.06f, 1.8f), DarkWood);
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    kit.Deco(PrimitiveType.Cylinder, tr + new Vector3(sx * 0.5f, 0.6f, sz * 0.85f), new Vector3(0.05f, 0.6f, 0.05f), Brass);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, tr + new Vector3(0f, 1.6f, 0f), new Vector3(0.6f, 0.4f, 0.6f), PatchworkAssets.Plain(new Color(0.9f, 0.75f, 0.4f), 0.9f)));
            NoShadow(kit.Deco(PrimitiveType.Sphere, tr + new Vector3(0f, 2.05f, 0f), new Vector3(0.4f, 0.2f, 0.4f), PatchworkAssets.Plain(new Color(0.9f, 0.75f, 0.4f), 0.9f)));
            for (int i = 0; i < 4; i++)
                NoShadow(kit.Deco(PrimitiveType.Sphere, tr + new Vector3(0f, 1.3f, -0.6f + i * 0.4f), new Vector3(0.3f, 0.22f, 0.3f), PatchworkAssets.Felt(i % 2 == 0 ? new Color(1f, 0.7f, 0.8f) : new Color(0.8f, 0.6f, 0.4f))));
            var board = kit.Group("CafeBoard", new Vector3(-25.6f, 0f, -86.2f));
            board.rotation = Rot(70f);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.6f, s * 0.18f), new Vector3(0.8f, 1.2f, 0.06f), DarkWood, new Vector3(s * 12f, 0f, 0f), board);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.66f, 0.25f), new Vector3(0.66f, 0.9f, 0.02f), PatchworkAssets.Felt(new Color(0.15f, 0.2f, 0.2f)), new Vector3(12f, 0f, 0f), board));
            for (int i = 0; i < 4; i++)
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.95f - i * 0.18f, 0.27f + i * 0.04f), new Vector3(0.4f - (i % 2) * 0.12f, 0.03f, 0.01f), PatchworkAssets.Felt(CreamCol), new Vector3(12f, 0f, 0f), board));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.15f, 0.25f), new Vector3(0.2f, 0.01f, 0.2f), PatchworkAssets.Felt(new Color(1f, 0.6f, 0.7f)), new Vector3(102f, 0f, 0f), board));
            TownKit.Solid(kit, new Vector3(-25.6f, 0.6f, -86.2f), new Vector3(0.6f, 1.2f, 0.9f), 70f);
            Plaque(kit, new Vector3(-25.4f, 0f, -94f), 90f, "The Whale's Tail Café",
                "Closed. (A note underneath, in pencil: \"Help yourself to a biscuit. Take ONE. I have counted them. Love, Mrs. Sudsbury.\")");
            for (int i = 0; i < 3; i++)
                PointLight(kit, new Vector3(-27.4f, 2.6f, i == 0 ? -94f : i == 1 ? -64f : -85.4f), LampGold, 7f, 0.9f);
        }

        // ------------------------------------------------------------------ The Hall of Deep Time

        static void HistoryLife(LevelKit kit)
        {
            // The night watchman doing his rounds with a lantern, slowly, round the great lizard.
            var g = kit.Group("Watchman", Vector3.zero);
            var body = new GameObject("Body").transform;
            body.SetParent(g, false);
            Figure(kit, body, new Color(0.22f, 0.28f, 0.5f), new Color(0.15f, 0.18f, 0.35f), 1.15f, true);
            var lantern = new GameObject("Lantern").transform;
            lantern.SetParent(g, false);
            lantern.localPosition = new Vector3(0.46f, 0.85f, 0.3f);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.3f, 0.05f, 0.3f), Brass, default(Vector3), lantern);
            NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.down * 0.22f, new Vector3(0.26f, 0.34f, 0.26f), PatchworkAssets.Glow(new Color(1f, 0.8f, 0.45f), 3f), default(Vector3), lantern));
            kit.Deco(PrimitiveType.Cylinder, Vector3.down * 0.44f, new Vector3(0.28f, 0.04f, 0.28f), Brass, default(Vector3), lantern);
            var lamp = PointLight(kit, Vector3.zero, new Color(1f, 0.78f, 0.45f), 11f, 1.6f);
            lamp.transform.SetParent(lantern, false);
            lamp.transform.localPosition = Vector3.down * 0.2f;
            lamp.gameObject.AddComponent<CandleFlicker>().Glow = lamp;
            var walk = g.gameObject.AddComponent<PathMover>();
            walk.Sway = 3f;
            walk.BobHeight = 0.04f;
            walk.BobSpeed = 5.5f;
            walk.Setup(new[] { new Vector3(-53.5f, 0f, -48.4f), new Vector3(-69.5f, 0f, -48.4f), new Vector3(-69.5f, 0f, -82f), new Vector3(-53.5f, 0f, -82f) }, 1.1f, 0f);
            // Mice, of course.
            var m1 = ToyCritter.Mouse(kit, new Vector3(-70f, 0f, -100.6f));
            m1.Radius = 2.6f;
            var m2 = ToyCritter.Mouse(kit, new Vector3(-41.6f, 0f, -28.6f));
            m2.Radius = 2.2f;
            // Oars out of the longship, and a lamp hung off its stern.
            Vector3 ship = new Vector3(-62f, 9.4f, -32f);
            var oar = PatchworkAssets.Wood(new Color(0.8f, 0.66f, 0.45f));
            for (int i = 0; i < 6; i++)
                for (int s = -1; s <= 1; s += 2)
                {
                    float x = -6.25f + i * 2.5f;
                    float w = i == 0 || i == 5 ? 2.8f : 3.6f;
                    Vector3 a = ship + new Vector3(x, 0.3f, s * w * 0.5f), b = ship + new Vector3(x - 0.6f, -2.2f, s * (w * 0.5f + 2.6f));
                    Beam(kit, a, b, 0.1f, oar);
                    NoShadow(kit.Deco(PrimitiveType.Cube, b + new Vector3(-0.1f, -0.1f, s * 0.2f), new Vector3(0.1f, 0.4f, 0.8f), oar, new Vector3(-s * 40f, 0f, 0f)));
                }
            NoShadow(kit.Deco(PrimitiveType.Sphere, ship + new Vector3(-10.6f, 2.3f, 0f), new Vector3(0.4f, 0.5f, 0.4f), PatchworkAssets.Glow(new Color(1f, 0.75f, 0.4f), 3f)));
            // An owl asleep on top of the bookcase by the stair.
            var owl = kit.Group("Owl", new Vector3(-32.5f, 11.6f, -100f));
            owl.rotation = Rot(270f);
            var feather = PatchworkAssets.Yarn(new Color(0.6f, 0.45f, 0.32f));
            var owlBody = kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.3f, 0f), new Vector3(0.5f, 0.6f, 0.46f), feather, default(Vector3), owl);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.32f, 0.16f), new Vector3(0.36f, 0.4f, 0.2f), PatchworkAssets.Felt(new Color(0.92f, 0.84f, 0.7f)), default(Vector3), owl);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.14f, 0.66f, 0f), new Vector3(0.08f, 0.16f, 0.06f), feather, new Vector3(0f, 0f, s * -20f), owl);
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.08f, 0.44f, 0.25f), new Vector3(0.1f, 0.02f, 0.02f), PatchworkAssets.Felt(new Color(0.2f, 0.15f, 0.1f)), default(Vector3), owl));
            }
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.36f, 0.27f), new Vector3(0.05f, 0.08f, 0.06f), PatchworkAssets.Felt(new Color(1f, 0.75f, 0.3f)), default(Vector3), owl);
            owl.gameObject.AddComponent<SleepyBreath>().Body = owlBody;
        }

        // ------------------------------------------------------------------ The Kelp Galleries

        static void KelpLife(LevelKit kit)
        {
            // Two seals, Salt and Pepper, taking turns to leap.
            Leap(kit, Seal(kit, new Color(0.45f, 0.48f, 0.55f)), new Vector3(-46f, PoolSurface - 0.2f, 20.6f), new Vector3(-49.5f, PoolSurface - 0.2f, 24.2f), 2.2f, 6.5f, 0f, 0.35f);
            Leap(kit, Seal(kit, new Color(0.25f, 0.27f, 0.32f)), new Vector3(-84.5f, PoolSurface - 0.2f, 30f), new Vector3(-85.2f, PoolSurface - 0.2f, 35.6f), 2.5f, 8f, 3.2f, 0.35f);
            Person(kit, new Vector3(-48f, 0f, 15.4f), 330f, ToyNpc.Kind.RagDoll, "Keeper Coral", new Color(0.25f, 0.5f, 0.5f), new Color(1f, 0.75f, 0.4f), 1.2f, () => new[]
            {
                "Watch the water. There! That's Salt. Pepper's the dark one, over by the big tank. They're showing off for you.",
                "Seals aren't fish. They breathe air, just like you, and they can hold it for twenty minutes or more.",
                "They're meant to be asleep. I'm meant to be asleep. Nobody in this museum is ever asleep, except Nod."
            });
            Vector3 bucket = new Vector3(-46.8f, 0f, 16.4f);
            kit.Deco(PrimitiveType.Cylinder, bucket + Vector3.up * 0.32f, new Vector3(0.66f, 0.32f, 0.66f), PatchworkAssets.Plain(new Color(0.6f, 0.62f, 0.7f), 0.7f));
            for (int i = 0; i < 3; i++)
            {
                var f = kit.Group("BucketFish", bucket + new Vector3(-0.1f + i * 0.1f, 0.7f, 0f));
                f.rotation = Quaternion.Euler(-60f - i * 10f, i * 50f, 0f);
                StitchSwimmer(kit, f, Swimmer.Fish, 0.34f, new Color(0.7f, 0.75f, 0.85f), CreamCol, false);
            }
            TownKit.Solid(kit, bucket + Vector3.up * 0.35f, new Vector3(0.66f, 0.7f, 0.66f), 0f);
            // Schools of little fish flashing round between the kelp.
            Shoal(kit, new Vector3(-60f, -3.4f, 30f), 9f, 6f, 16f, 22, 0.32f, new Color(0.85f, 0.95f, 1f), 81);
            Shoal(kit, new Vector3(-58f, -2.6f, 48f), 12f, 4f, -13f, 18, 0.3f, new Color(1f, 0.8f, 0.4f), 82, 120f);
        }

        // A felt seal. Faces +z.
        static Transform Seal(LevelKit kit, Color col)
        {
            var g = kit.Group("Seal", Vector3.zero);
            var m = PatchworkAssets.Felt(col);
            kit.Deco(PrimitiveType.Capsule, Vector3.zero, new Vector3(0.7f, 1.1f, 0.7f), m, new Vector3(90f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.08f, 1.1f), new Vector3(0.52f, 0.48f, 0.56f), m, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, 1.36f), new Vector3(0.3f, 0.22f, 0.2f), PatchworkAssets.Felt(Color.Lerp(col, Color.white, 0.3f)), default(Vector3), g);
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.06f, 1.45f), Vector3.one * 0.08f, PatchworkAssets.Plain(new Color(0.05f, 0.05f, 0.07f), 0.8f), default(Vector3), g));
            for (int s = -1; s <= 1; s += 2)
            {
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.15f, 0.2f, 1.3f), Vector3.one * 0.1f, PatchworkAssets.Plain(new Color(0.05f, 0.05f, 0.07f), 0.9f), default(Vector3), g));
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.4f, -0.15f, 0.4f), new Vector3(0.5f, 0.06f, 0.3f), m, new Vector3(0f, s * -30f, s * -20f), g);
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.16f, 0f, -1.05f), new Vector3(0.24f, 0.05f, 0.5f), m, new Vector3(0f, s * 20f, 0f), g);
                for (int w = 0; w < 2; w++)
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.28f, -0.02f + w * 0.05f, 1.34f), new Vector3(0.3f, 0.01f, 0.01f), PatchworkAssets.Felt(CreamCol), new Vector3(0f, s * -15f, 0f), g));
            }
            return g;
        }

        static void Leap(LevelKit kit, Transform who, Vector3 from, Vector3 to, float height, float every, float offset, float splash)
        {
            var l = who.gameObject.AddComponent<Leaper>();
            l.From = from;
            l.To = to;
            l.Height = height;
            l.Every = every;
            l.Offset = offset;
            l.Splash = splash;
            who.position = from;
        }

        // ------------------------------------------------------------------ The Great Deep

        static void DeepLife(LevelKit kit)
        {
            // A diver outside the tunnel, cleaning the glass with a sponge.
            var diver = kit.Group("Diver", new Vector3(-17f, 3.2f, 71f));
            var suit = PatchworkAssets.Felt(new Color(0.75f, 0.6f, 0.4f));
            var brass = Brass;
            kit.Deco(PrimitiveType.Capsule, Vector3.zero, new Vector3(0.7f, 0.8f, 0.6f), suit, new Vector3(90f, 0f, 0f), diver);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.05f, 0.95f), Vector3.one * 0.8f, brass, default(Vector3), diver);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.05f, 1.33f), new Vector3(0.4f, 0.04f, 0.4f), PatchworkAssets.Glow(new Color(1f, 0.9f, 0.6f), 2.2f), new Vector3(90f, 0f, 0f), diver));
            for (int s = -1; s <= 1; s += 2)
            {
                NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.4f, 0.05f, 0.95f), new Vector3(0.2f, 0.03f, 0.2f), PatchworkAssets.Glow(new Color(1f, 0.9f, 0.6f), 1.6f), new Vector3(0f, 0f, 90f), diver));
                kit.Deco(PrimitiveType.Capsule, new Vector3(s * 0.18f, -0.05f, -0.95f), new Vector3(0.22f, 0.4f, 0.22f), suit, new Vector3(90f, 0f, 0f), diver);
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.18f, -0.05f, -1.4f), new Vector3(0.24f, 0.12f, 0.36f), PatchworkAssets.Plain(new Color(0.2f, 0.2f, 0.24f), 0.6f), default(Vector3), diver);
            }
            // One arm reaching out to the glass with the sponge.
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, -0.4f, 0.5f), new Vector3(0.18f, 0.4f, 0.18f), suit, new Vector3(-30f, 0f, 0f), diver);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.72f, 0.75f), new Vector3(0.3f, 0.12f, 0.2f), PatchworkAssets.Felt(new Color(1f, 0.85f, 0.3f)), default(Vector3), diver);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.48f, -0.1f), new Vector3(0.5f, 0.3f, 0.8f), brass, default(Vector3), diver);
            var helmetBubbles = kit.Group("DiverBubbles", Vector3.zero);
            helmetBubbles.SetParent(diver, false);
            helmetBubbles.localPosition = new Vector3(0f, 0.5f, 0.95f);
            var bm = PatchworkAssets.Glow(new Color(0.8f, 0.95f, 1f), 0.8f);
            var bubbles = new Transform[6];
            for (int i = 0; i < bubbles.Length; i++) bubbles[i] = NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.15f, bm, default(Vector3), helmetBubbles));
            var bs = helmetBubbles.gameObject.AddComponent<BubbleStream>();
            bs.Bubbles = bubbles;
            bs.Height = 4f;
            bs.Speed = 1.4f;
            var lamp = PointLight(kit, Vector3.zero, new Color(1f, 0.9f, 0.65f), 8f, 1.4f);
            lamp.transform.SetParent(diver, false);
            lamp.transform.localPosition = new Vector3(0f, 0f, 1.8f);
            var swim = diver.gameObject.AddComponent<FishSwim>();
            swim.Center = new Vector3(-16f, 3.2f, 70.9f);
            swim.RadiusX = 8f;
            swim.RadiusZ = 0.3f;
            swim.Speed = 4f;
            swim.Bob = 0.5f;
            swim.BobSpeed = 0.4f;
            swim.Wiggle = 3f;
            swim.WiggleSpeed = 1.5f;
            swim.Bank = 0f;
            // Two mantas dancing round each other over the Bell.
            Swim(kit, Swimmer.Ray, new Vector3(6f, 11.5f, TunnelZ), 4.5f, 4.5f, 16f, 2.3f, new Color(0.14f, 0.16f, 0.32f), CreamCol, 0f).Bob = 1.2f;
            Swim(kit, Swimmer.Ray, new Vector3(6f, 12.5f, TunnelZ), 4.5f, 4.5f, 16f, 2.1f, new Color(0.2f, 0.22f, 0.4f), CreamCol, 180f).Bob = 1.2f;
            // Lanternfish: little shoals with their own glow, low by the glass.
            foreach (var sp in new[] { new Vector4(-16f, 1.8f, 57.6f, 1f), new Vector4(28f, 1.8f, 74.4f, -1f) })
            {
                var shoal = Shoal(kit, new Vector3(sp.x, sp.y, sp.z), 9f, 1.2f, 11f * sp.w, 24, 0.26f, new Color(0.4f, 1f, 0.9f), Mathf.RoundToInt(sp.x * 7f));
                var glow = PointLight(kit, Vector3.zero, new Color(0.4f, 1f, 0.9f), 7f, 1.4f);
                glow.transform.SetParent(shoal.transform, false);
            }
            // A wave of light running along the tunnel floor.
            const int groups = 6;
            var chase = new MeshBatch[groups];
            for (int i = 0; i < groups; i++) chase[i] = new MeshBatch();
            foreach (var run in new[] { new Vector2(-30f, -3f), new Vector2(15f, 42f) })
                for (float x = run.x; x <= run.y; x += 1f)
                {
                    int gi = Mathf.Abs(Mathf.RoundToInt(x)) % groups;
                    for (int s = -1; s <= 1; s += 2) chase[gi].Ball(new Vector3(x, 0.05f, TunnelZ + s * 3.3f), new Vector3(0.16f, 0.06f, 0.16f), 2, 5);
                }
            for (int i = 0; i < groups; i++) Twinkling(chase[i], kit.Root, "TunnelChase", new Color(0.45f, 0.85f, 1f), 0.15f, 2.8f, 2.2f, -i * (Mathf.PI * 2f / groups));
        }

        // ------------------------------------------------------------------ The Sunken Ruins

        static void RuinsLife(LevelKit kit)
        {
            // Fish leaping out of the pool.
            Color[] fishCols = { new Color(0.85f, 0.9f, 1f), new Color(1f, 0.75f, 0.4f), new Color(0.6f, 1f, 0.8f) };
            Vector3[,] jumps = { { new Vector3(50f, 0f, 32f), new Vector3(52f, 0f, 30f) }, { new Vector3(86f, 0f, 72.4f), new Vector3(88f, 0f, 75f) }, { new Vector3(62f, 0f, 92f), new Vector3(60.5f, 0f, 90f) }, { new Vector3(80f, 0f, 30f), new Vector3(83f, 0f, 31f) } };
            for (int i = 0; i < jumps.GetLength(0); i++)
            {
                var f = kit.Group("JumpingFish", Vector3.zero);
                StitchSwimmer(kit, f, Swimmer.Fish, 0.7f, fishCols[i % 3], CreamCol, true);
                Vector3 a = jumps[i, 0], b = jumps[i, 1];
                a.y = b.y = RuinSurface - 0.1f;
                Leap(kit, f, a, b, 1.3f, 5f + i * 1.3f, i * 1.7f, 0.2f);
            }
            // A heron fishing from a broken column.
            Stump(kit, 56.5f, 49f, 0.35f, 1f, true);
            var heron = kit.Group("Heron", new Vector3(56.5f, 0.35f, 49f));
            heron.rotation = Rot(250f);
            var grey = PatchworkAssets.Felt(new Color(0.65f, 0.7f, 0.78f));
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.1f, 0.6f, 0f), new Vector3(0.05f, 0.6f, 0.05f), PatchworkAssets.Felt(new Color(0.9f, 0.75f, 0.4f)), default(Vector3), heron);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.5f, -0.1f), new Vector3(0.5f, 0.56f, 0.9f), grey, new Vector3(-12f, 0f, 0f), heron);
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 2.1f, 0.22f), new Vector3(0.14f, 0.5f, 0.14f), PatchworkAssets.Felt(CreamCol), new Vector3(20f, 0f, 0f), heron);
            var hhead = new GameObject("HeronHead").transform;
            hhead.SetParent(heron, false);
            hhead.localPosition = new Vector3(0f, 2.55f, 0.36f);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.22f, 0.22f, 0.3f), PatchworkAssets.Felt(CreamCol), default(Vector3), hhead);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.02f, 0.32f), new Vector3(0.06f, 0.06f, 0.5f), PatchworkAssets.Felt(new Color(1f, 0.75f, 0.3f)), default(Vector3), hhead);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.08f, -0.2f), new Vector3(0.03f, 0.03f, 0.4f), PatchworkAssets.Felt(new Color(0.2f, 0.2f, 0.25f)), new Vector3(-20f, 0f, 0f), hhead);
            for (int s = -1; s <= 1; s += 2)
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.1f, 0.04f, 0.08f), Vector3.one * 0.05f, PatchworkAssets.Plain(new Color(0.05f, 0.05f, 0.07f), 0.8f), default(Vector3), hhead));
            hhead.gameObject.AddComponent<WatchPlayer>().MaxTurn = 70f;
            // A turtle sunning itself on the temple steps.
            var turtle = kit.Group("BaskingTurtle", new Vector3(65.4f, 0.6f, 55.9f));
            turtle.rotation = Quaternion.Euler(-4f, 200f, 0f);
            StitchSwimmer(kit, turtle, Swimmer.Turtle, 0.8f, new Color(0.4f, 0.6f, 0.45f), new Color(0.82f, 0.76f, 0.5f), true);
            turtle.gameObject.AddComponent<SleepyBreath>().Body = turtle;
            // Glowing jellyfish drifting in the deep water at the north end.
            Color[] jc = { new Color(0.6f, 1f, 0.85f), new Color(0.75f, 0.8f, 1f) };
            Vector3[] jellies = { new Vector3(63f, -3f, 76f), new Vector3(77f, -2.4f, 80f), new Vector3(93f, -2.8f, 84f), new Vector3(73f, -2.2f, 96f), new Vector3(95f, -3f, 78f), new Vector3(84f, -2.6f, 92f) };
            for (int i = 0; i < jellies.Length; i++)
                Jelly(kit, jellies[i], 0.6f, jc[i % 2], i * 1.4f, false);
        }

        // ------------------------------------------------------------------ The Hall of Arms

        static void ArmsLife(LevelKit kit)
        {
            float cx = (ArmsX0 + ArmsX1) * 0.5f;
            Person(kit, new Vector3(90.4f, 0f, -25.6f), 80f, ToyNpc.Kind.TinSoldier, "Squire Pim", new Color(0.3f, 0.45f, 0.35f), new Color(1f, 0.82f, 0.4f), 1.3f, () => new[]
            {
                "Polishing. Always polishing. You breathe on armour and it goes spotty.",
                "That one's the steppe archer's. He shot from the saddle at a full gallop, backwards. I can barely ride a carousel forwards.",
                "Did you see the helmets turn? They do that. I just say \"good evening\" and carry on."
            });
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(91.2f, 1.1f, -25.2f), new Vector3(0.3f, 0.04f, 0.3f), PatchworkAssets.Felt(CreamCol), new Vector3(20f, 30f, 10f)));
            // The castellan's dog asleep beside the horseman.
            var dog = kit.Group("Hound", new Vector3(76.4f, 0f, -31.6f));
            dog.rotation = Rot(110f);
            var fur = PatchworkAssets.Yarn(new Color(0.55f, 0.4f, 0.28f));
            var dbody = kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.3f, 0f), new Vector3(0.6f, 0.62f, 0.6f), fur, new Vector3(90f, 0f, 0f), dog);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.2f, 0.26f, 0.7f), new Vector3(0.38f, 0.32f, 0.46f), fur, default(Vector3), dog);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.24f, 0.2f, 0.95f), new Vector3(0.2f, 0.16f, 0.22f), PatchworkAssets.Yarn(new Color(0.8f, 0.7f, 0.55f)), default(Vector3), dog);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Sphere, new Vector3(0.2f + s * 0.2f, 0.26f, 0.64f), new Vector3(0.08f, 0.3f, 0.2f), PatchworkAssets.Yarn(new Color(0.35f, 0.25f, 0.18f)), new Vector3(0f, 0f, s * 20f), dog);
                kit.Deco(PrimitiveType.Capsule, new Vector3(s * 0.2f, 0.1f, 0.5f), new Vector3(0.14f, 0.3f, 0.14f), fur, new Vector3(90f, 0f, 0f), dog);
            }
            kit.Deco(PrimitiveType.Capsule, new Vector3(-0.3f, 0.12f, -0.6f), new Vector3(0.1f, 0.3f, 0.1f), fur, new Vector3(90f, 40f, 0f), dog);
            dog.gameObject.AddComponent<SleepyBreath>().Body = dbody;
            Snores(kit, new Vector3(76.6f, 1f, -31f));
            // Candles round the long table of dusty helmets at the north end.
            Vector3 tbl = new Vector3(cx + 7.8f, 0f, -8.5f);
            kit.Block(tbl + Vector3.up * 0.5f, new Vector3(1.2f, 1f, 6f), WoodCol, "HelmTable", DarkWood);
            for (int i = 0; i < 4; i++)
            {
                Vector3 p = tbl + new Vector3(0f, 1.2f, -2.2f + i * 1.45f);
                var helm = PatchworkAssets.Plain(Steel, 0.75f);
                NoShadow(kit.Deco(PrimitiveType.Sphere, p, new Vector3(0.5f, 0.46f, 0.52f), i % 2 == 0 ? helm : PatchworkAssets.Plain(new Color(0.8f, 0.6f, 0.3f), 0.7f)));
                if (i % 2 == 0) NoShadow(kit.Deco(PrimitiveType.Cube, p + new Vector3(0f, 0.32f, 0f), new Vector3(0.06f, 0.3f, 0.36f), PatchworkAssets.Yarn(i == 0 ? Lacquer : Navy)));
            }
            foreach (float z in new[] { -2.9f, 2.9f })
            {
                Vector3 c = tbl + new Vector3(0f, 1f, z);
                kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.3f, new Vector3(0.14f, 0.3f, 0.14f), PatchworkAssets.Felt(CreamCol));
                var flame = NoShadow(kit.Deco(PrimitiveType.Sphere, c + Vector3.up * 0.72f, new Vector3(0.12f, 0.22f, 0.12f), PatchworkAssets.Glow(new Color(1f, 0.7f, 0.3f), 3.2f)));
                var l = PointLight(kit, c + Vector3.up * 0.9f, new Color(1f, 0.72f, 0.4f), 6f, 1.1f);
                var fl = l.gameObject.AddComponent<CandleFlicker>();
                fl.Glow = l;
                fl.Flame = flame;
            }
        }

        // ------------------------------------------------------------------ Signs over the doors

        // A brass-framed sign over a doorway with a stitched picture of what's through it.
        // 0 bones, 1 kelp, 2 fish, 3 column, 4 helmet, 5 star, 6 whale.
        static void DoorSign(LevelKit kit, Vector3 at, float yaw, int icon, float hangTo = 0f)
        {
            var g = kit.Group("DoorSign", at);
            g.rotation = Rot(yaw);
            if (hangTo > at.y)
                for (int s = -1; s <= 1; s += 2)
                    Beam(kit, at + Rot(yaw) * new Vector3(s * 1.3f, 0.6f, 0f), at + Rot(yaw) * new Vector3(s * 1.3f, 0f, 0f) + Vector3.up * (hangTo - at.y), 0.04f, Brass);
            NoShadow(kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(3.2f, 1.3f, 0.1f), Brass, default(Vector3), g));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, 0.06f), new Vector3(2.9f, 1.04f, 0.04f), StarCloth, default(Vector3), g));
            var gold = PatchworkAssets.Glow(new Color(1f, 0.85f, 0.45f), 1.4f);
            var mb = new MeshBatch();
            Vector3 o = new Vector3(0f, 0f, 0.1f);
            switch (icon)
            {
                case 0:
                    mb.Box(o, new Vector3(1.2f, 0.18f, 0.03f), Quaternion.identity);
                    for (int s = -1; s <= 1; s += 2)
                        for (int t = -1; t <= 1; t += 2) mb.Ball(o + new Vector3(s * 0.62f, t * 0.13f, 0f), new Vector3(0.26f, 0.26f, 0.04f), 2, 6);
                    break;
                case 1:
                    for (int k = -1; k <= 1; k++)
                    {
                        mb.Box(o + new Vector3(k * 0.4f, 0f, 0f), new Vector3(0.05f, 0.8f, 0.03f), Quaternion.Euler(0f, 0f, k * 8f));
                        for (int l = 0; l < 3; l++) mb.Ball(o + new Vector3(k * 0.4f + (l % 2 == 0 ? 0.1f : -0.1f), -0.25f + l * 0.24f, 0f), new Vector3(0.2f, 0.1f, 0.03f), 2, 5);
                    }
                    break;
                case 2:
                    mb.Ball(o, new Vector3(0.9f, 0.46f, 0.04f), 3, 8);
                    mb.Box(o + new Vector3(-0.56f, 0f, 0f), new Vector3(0.3f, 0.4f, 0.03f), Quaternion.Euler(0f, 0f, 45f));
                    break;
                case 3:
                    mb.Box(o + new Vector3(0f, 0.36f, 0f), new Vector3(0.8f, 0.12f, 0.03f), Quaternion.identity);
                    mb.Box(o + new Vector3(0f, -0.38f, 0f), new Vector3(0.8f, 0.12f, 0.03f), Quaternion.identity);
                    for (int k = -1; k <= 1; k++) mb.Box(o + new Vector3(k * 0.24f, 0f, 0f), new Vector3(0.1f, 0.62f, 0.03f), Quaternion.identity);
                    break;
                case 4:
                    mb.Ball(o + new Vector3(0f, 0.05f, 0f), new Vector3(0.6f, 0.7f, 0.04f), 3, 8);
                    mb.Box(o + new Vector3(0f, 0.04f, 0.01f), new Vector3(0.4f, 0.06f, 0.03f), Quaternion.identity);
                    mb.Box(o + new Vector3(0f, 0.46f, 0f), new Vector3(0.1f, 0.3f, 0.03f), Quaternion.identity);
                    break;
                case 5:
                    mb.Box(o, new Vector3(0.18f, 0.9f, 0.03f), Quaternion.identity);
                    mb.Box(o, new Vector3(0.9f, 0.18f, 0.03f), Quaternion.identity);
                    mb.Box(o, new Vector3(0.4f, 0.4f, 0.03f), Quaternion.Euler(0f, 0f, 45f));
                    break;
                default:
                    mb.Ball(o, new Vector3(1.1f, 0.45f, 0.04f), 3, 8);
                    mb.Box(o + new Vector3(-0.66f, 0.1f, 0f), new Vector3(0.3f, 0.12f, 0.03f), Quaternion.Euler(0f, 0f, -35f));
                    mb.Box(o + new Vector3(-0.66f, -0.06f, 0f), new Vector3(0.3f, 0.12f, 0.03f), Quaternion.Euler(0f, 0f, 30f));
                    break;
            }
            for (int s = -1; s <= 1; s += 2)
            {
                // Little stars either side.
                mb.Box(o + new Vector3(s * 1.15f, 0f, 0f), new Vector3(0.06f, 0.3f, 0.03f), Quaternion.identity);
                mb.Box(o + new Vector3(s * 1.15f, 0f, 0f), new Vector3(0.3f, 0.06f, 0.03f), Quaternion.identity);
            }
            mb.Build(g, "SignIcon", gold);
        }

        static void Signs(LevelKit kit)
        {
            DoorSign(kit, new Vector3(-27f, 5.6f, -78f), 90f, 0, 7f);    // atrium -> Hall of Deep Time (hung under the gallery)
            DoorSign(kit, new Vector3(-60f, 9.4f, -10.08f), 180f, 1);   // Hall of Deep Time -> Kelp Galleries
            DoorSign(kit, new Vector3(-32.08f, 8.9f, TunnelZ), 270f, 2); // Kelp Galleries -> the tunnel
            DoorSign(kit, new Vector3(44.08f, 8.9f, TunnelZ), 90f, 2);   // the Ruins -> back down the tunnel
            DoorSign(kit, new Vector3(80f, 9.4f, 6.08f), 0f, 4);         // Ruins -> Hall of Arms
            DoorSign(kit, new Vector3(80f, 9.4f, 3.92f), 180f, 3);       // Hall of Arms -> back to the Ruins
            DoorSign(kit, new Vector3(-60f, 9.4f, -3.92f), 0f, 0);       // Kelp Galleries -> back to the Hall of Deep Time
            DoorSign(kit, new Vector3(52.48f, 5.4f, 10f), 90f, 5);       // the lift
            DoorSign(kit, new Vector3(0f, 12.8f, -107.92f), 0f, 6);      // over the front doors, inside
        }
    }
}

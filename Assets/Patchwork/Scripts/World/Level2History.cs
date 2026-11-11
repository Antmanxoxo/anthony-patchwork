using UnityEngine;

namespace Patchwork
{
    // The Hall of Deep Time: one long gallery down the museum's west side. Sands and pharaohs
    // at the south end, the great lizard in the middle with the pendulum beside it, and at the
    // north end the ships and the stars people steered them by, with a Viking longship hung
    // from the rafters. An upper gallery runs the length of the east wall.
    public static partial class Level2Builder
    {
        static void History(LevelKit kit)
        {
            var stone = Sandstone;
            float T = HistTop;
            Floor(kit, HistX0, -30f, HistZ0, KelpZ0, 0f, WarmQuilt, 4.5f, "HistoryFloor");
            Wall(kit, new Vector3(-89f, 0f, -105f), new Vector3(-89f, 0f, -8f), -1f, T + 6f, 2f, stone);
            Wall(kit, new Vector3(-90f, 0f, -105f), new Vector3(-30f, 0f, -105f), -1f, T + 6f, 2f, stone);
            Wall(kit, new Vector3(-31f, 0f, AtriumZ1), new Vector3(-31f, 0f, -4f), -1f, AtriumTop, 2f, stone);
            // The thick wall to the Kelp Galleries, with a tunnel of a doorway through it.
            WallWithDoor(kit, new Vector3(-90f, 0f, -7f), new Vector3(-30f, 0f, -7f), -1f, T + 6f, 6f, stone, 30f, 8f, 7.5f);
            // A dark timber ceiling with beams, like the inside of a ship.
            kit.Block(new Vector3(-60f, T + 0.5f, -57f), new Vector3(60f, 1f, 98f), WoodCol, "HistoryCeiling", PatchworkAssets.Planks(new Color(0.3f, 0.2f, 0.16f)));
            for (float z = HistZ0 + 5f; z < HistZ1; z += 8.4f)
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(-60f, T - 0.4f, z), new Vector3(56f, 0.8f, 0.9f), DarkWood);
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Cube, new Vector3(-60f + s * 26.6f, T - 1.6f, z), new Vector3(2.4f, 0.5f, 0.7f), DarkWood, new Vector3(0f, 0f, s * 38f));
            }
            for (float x = -80f; x <= -40f; x += 20f)
                kit.Deco(PrimitiveType.Cube, new Vector3(x, T - 0.2f, -57f), new Vector3(0.7f, 0.5f, 94f), DarkWood);
            // Tall windows down the west wall, full of night.
            for (float z = -96f; z <= -18f; z += 13f)
                NightWindow(kit, new Vector3(-88f, 6f, z), 90f, 3.6f, 9f, new Color(0.3f, 0.42f, 0.8f), 0.9f);
            // Lamps.
            foreach (float z in new[] { -94f, -76f, -58f, -40f, -22f })
            {
                HangingLantern(kit, new Vector3(-72f, 11.5f, z), T, 22f, 1.5f);
                HangingLantern(kit, new Vector3(-46f, 11.5f, z + 9f), T, 22f, 1.5f, z != -22f);
            }

            HistoryGallery(kit);
            Sands(kit);
            Lizard(kit);
            ShipsAndStars(kit);

            // ----- The way through -----
            kit.AddCheckpoint(new Vector3(-41f, 0f, -78f), 270f);
            Trail(kit, 10, new Vector3(-36f, 1.2f, -78f), new Vector3(-41f, 1.2f, -78f), new Vector3(-46f, 1.2f, -76f), new Vector3(-48.5f, 1.2f, -71f), new Vector3(-49f, 1.2f, -65f),
                new Vector3(-49f, 1.2f, -59f), new Vector3(-49f, 1.2f, -53f));
            Trail(kit, 10, new Vector3(-50f, 1.2f, -45f), new Vector3(-53f, 1.2f, -40f), new Vector3(-56f, 1.2f, -35f), new Vector3(-58f, 1.2f, -29f), new Vector3(-60f, 1.2f, -23f), new Vector3(-60f, 1.2f, -17f));
            kit.ThreadLine(new Vector3(-70f, 1.3f, -84f), new Vector3(-55f, 1.3f, -84f), 6);
            kit.Hint(new Vector3(-42f, 1.5f, -78f), new Vector3(8f, 4f, 10f), "The Hall of Deep Time",
                "The Kelp Galleries are through the arch at the far north end. There's a Heartstring on the longship hanging in the rafters: climb the painters' scaffold beside it, or swing across from the upper gallery.");
            Person(kit, new Vector3(-51f, 0f, -80f), 100f, ToyNpc.Kind.TinSoldier, "Sergeant Stanchion", new Color(0.7f, 0.2f, 0.25f), new Color(1f, 0.82f, 0.4f), 0.8f, () => new[]
            {
                "Halt! Who goes... oh. It's a doll. Carry on.",
                "No touching the exhibits. No climbing the exhibits. No swinging from the exhibits.",
                "...Between you and me, the scaffold by the longship isn't an exhibit. And I shall be looking the other way."
            });
        }

        // The upper gallery along the east wall, its stair, and what's kept up there.
        static void HistoryGallery(LevelKit kit)
        {
            Floor(kit, -38f, -29.98f, HistZ0, HistZ1, 8f, WarmQuilt, 3f, "HistoryGallery");
            for (float z = HistZ0 + 4f; z < HistZ1; z += 9.3f)
                if (Mathf.Abs(z + 78f) > 4.5f) Column(kit, new Vector3(-38.5f, 0f, z), 7f, 0.5f);
            Balustrade(kit, new Vector3(-38f, 8f, HistZ0 + 0.3f), new Vector3(-38f, 8f, -87.8f));
            Balustrade(kit, new Vector3(-38f, 8f, -84f), new Vector3(-38f, 8f, -36.5f));
            Balustrade(kit, new Vector3(-38f, 8f, -27.5f), new Vector3(-38f, 8f, HistZ1 - 0.3f));
            // A broad wooden stair up to it from the south end.
            Steps(kit, new Vector3(-39.6f, 0f, -103.6f), 0f, 32, 0.25f, 0.5f, 3.2f, DarkWood, -0.5f);
            kit.Block(new Vector3(-39.6f, 3.75f, -85.9f), new Vector3(3.2f, 8.5f, 3.4f), WoodCol, "StairHead", DarkWood);
            Balustrade(kit, new Vector3(-41.2f, 8f, -87.6f), new Vector3(-41.2f, 8f, -84.2f), 1.1f, DarkWood);
            Balustrade(kit, new Vector3(-41.2f, 8f, -84.2f), new Vector3(-38.2f, 8f, -84.2f), 1.1f, DarkWood);
            foreach (float z in new[] { -98f, -86f, -64f, -52f, -40f, -20f })
                Sconce(kit, new Vector3(-32f, 10.6f, z), 270f, z == -98f || z == -52f || z == -20f);
            // Bookcases and map chests along the wall.
            for (int i = 0; i < 5; i++)
            {
                float z = -100f + i * 4.4f;
                Bookcase(kit, new Vector3(-32.5f, 8f, z), 270f, 3.8f, 3.4f, i);
            }
            for (int i = 0; i < 3; i++) Bookcase(kit, new Vector3(-32.5f, 8f, -66f + i * 4.4f), 270f, 3.8f, 3.4f, i + 5);
            Vector3 mc = DisplayCase(kit, new Vector3(-34f, 8f, -46f), new Vector3(2f, 2.2f, 3f), 0f, false);
            NoShadow(kit.Deco(PrimitiveType.Cube, mc + Vector3.up * 0.05f, new Vector3(1.5f, 0.04f, 2.3f), PatchworkAssets.Felt(new Color(0.9f, 0.82f, 0.62f))));
            NoShadow(kit.Deco(PrimitiveType.Cube, mc + new Vector3(0.1f, 0.08f, 0.2f), new Vector3(0.7f, 0.03f, 1f), PatchworkAssets.Felt(new Color(0.55f, 0.75f, 0.8f)), new Vector3(0f, 20f, 0f)));
            Plaque(kit, new Vector3(-36f, 8f, -46f), 270f, "The Oldest Map of the Stars",
                "A star chart copied from one drawn in China well over a thousand years ago: more than 1,300 stars, every one inked by hand.",
                "People mapped the sky long before they finished mapping the Earth. The sky, after all, could be seen from anywhere.");
            kit.ThreadLine(new Vector3(-35f, 9.3f, -60f), new Vector3(-35f, 9.3f, -74f), 5);
            Trail(kit, 10, new Vector3(-35f, 9.2f, -24f), new Vector3(-35f, 9.2f, -20f), new Vector3(-35f, 9.2f, -16f), new Vector3(-35f, 9.2f, -12f));
        }

        // A bookcase against a wall (yaw faces out into the room). Solid.
        static void Bookcase(LevelKit kit, Vector3 foot, float yaw, float w, float h, int seed)
        {
            var g = kit.Group("Bookcase", foot);
            g.rotation = Rot(yaw);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, h * 0.5f, 0f), new Vector3(w, h, 0.9f), DarkWood, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, h + 0.08f, 0.05f), new Vector3(w + 0.2f, 0.16f, 1.1f), Brass, default(Vector3), g);
            Color[] spines = { Wine, Navy, Teal, new Color(0.6f, 0.45f, 0.2f), Plum, new Color(0.3f, 0.45f, 0.3f) };
            var rng = new System.Random(seed * 31 + 5);
            var books = new MeshBatch[spines.Length];
            for (int i = 0; i < books.Length; i++) books[i] = new MeshBatch();
            int shelves = Mathf.Max(2, Mathf.FloorToInt(h / 0.85f));
            for (int s = 0; s < shelves; s++)
            {
                float y = 0.2f + s * (h - 0.3f) / shelves;
                float x = -w * 0.5f + 0.2f;
                while (x < w * 0.5f - 0.25f)
                {
                    float bw = 0.1f + (float)rng.NextDouble() * 0.12f, bh = 0.45f + (float)rng.NextDouble() * 0.22f;
                    books[rng.Next(spines.Length)].Box(new Vector3(x + bw * 0.5f, y + bh * 0.5f, 0.38f), new Vector3(bw, bh, 0.3f), Quaternion.identity);
                    x += bw + 0.015f;
                }
            }
            for (int i = 0; i < books.Length; i++) books[i].Build(g, "Books", PatchworkAssets.Felt(spines[i]));
            var col = g.gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, h * 0.5f, 0f);
            col.size = new Vector3(w, h, 1f);
        }

        // ---------------------------------------------------------------- Sands and pharaohs

        static void Sands(LevelKit kit)
        {
            // The obelisk, with a steam vent beside it for anyone who'd like to see the top.
            Vector3 ob = new Vector3(-76f, 0f, -95f);
            Plinth(kit, ob, new Vector3(3.6f, 1.2f, 3.6f));
            var granite = PatchworkAssets.Cobble(new Color(0.75f, 0.5f, 0.42f));
            float y = 1.2f;
            for (int i = 0; i < 5; i++)
            {
                float w = 2.2f - i * 0.22f, h = 2.3f;
                kit.Block(ob + Vector3.up * (y + h * 0.5f), new Vector3(w, h, w), StoneCol, "Obelisk", granite);
                // Carved signs: little gold dashes up each face.
                var gold = PatchworkAssets.Felt(new Color(0.95f, 0.78f, 0.4f));
                for (int k = 0; k < 4; k++)
                    NoShadow(kit.Deco(PrimitiveType.Cube, ob + new Vector3(0f, y + 0.4f + k * 0.5f, -w * 0.5f - 0.01f), new Vector3(0.2f + (k % 2) * 0.3f, 0.16f, 0.03f), gold));
                y += h;
            }
            for (int i = 0; i < 4; i++)
                kit.Deco(PrimitiveType.Cube, ob + Vector3.up * (y + 0.2f + i * 0.4f), new Vector3(1f - i * 0.25f, 0.4f, 1f - i * 0.25f), Brass);
            kit.Updraft(new Vector3(-71f, 0f, -95f), 15f, LevelKit.UpdraftStyle.Steam);
            kit.GoldenButton("L2_b_obelisk", new Vector3(-76f, y + 3.2f, -95f));
            kit.Hint(new Vector3(-71f, 1.5f, -93f), new Vector3(6f, 4f, 6f), "A Warm Draught", "The heating vent by the obelisk lifts Wind Form all the way to the top. Something's shining up there.", HintTheme.Wind);
            Plaque(kit, new Vector3(-76f, 0f, -91.8f), 0f, "Obelisk",
                "Cut from a single piece of red granite and stood on end without a single crane, more than three thousand years ago.",
                "Nobody is completely sure how the Egyptians raised them. The best guess involves a great deal of sand, a great deal of rope, and a great many people pulling.");

            // The mummy case.
            Vector3 mc = DisplayCase(kit, new Vector3(-60f, 0f, -97f), new Vector3(5.6f, 2.5f, 2.4f));
            var stripes = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.15f, 0.25f, 0.6f), new Color(0.98f, 0.8f, 0.3f));
            NoShadow(kit.Deco(PrimitiveType.Capsule, mc + Vector3.up * 0.42f, new Vector3(1.1f, 2f, 0.8f), stripes, new Vector3(0f, 0f, 90f)));
            NoShadow(kit.Deco(PrimitiveType.Sphere, mc + new Vector3(1.9f, 0.5f, 0f), new Vector3(0.9f, 0.85f, 0.85f), PatchworkAssets.Felt(new Color(0.98f, 0.8f, 0.3f))));
            NoShadow(kit.Deco(PrimitiveType.Sphere, mc + new Vector3(1.95f, 0.72f, 0.14f), Vector3.one * 0.13f, PatchworkAssets.Plain(new Color(0.1f, 0.1f, 0.2f), 0.8f)));
            NoShadow(kit.Deco(PrimitiveType.Sphere, mc + new Vector3(1.95f, 0.72f, -0.14f), Vector3.one * 0.13f, PatchworkAssets.Plain(new Color(0.1f, 0.1f, 0.2f), 0.8f)));
            NoShadow(kit.Deco(PrimitiveType.Cube, mc + new Vector3(0.3f, 0.85f, 0f), new Vector3(0.5f, 0.06f, 0.9f), Brass, new Vector3(0f, 45f, 0f)));
            Plaque(kit, new Vector3(-60f, 0f, -94.6f), 0f, "A Painted Coffin",
                "The Egyptians wrapped their dead in linen: hundreds of metres of it, in strips, wound round and round. The very first patchwork people, you might say.",
                "The paint on this lid is over three thousand years old and has never been touched up.");
            SecretMarble.Place(kit, new Vector3(-86.4f, 1.1f, -102.6f), "L2_marble_sarcophagus", new Color(0.4f, 0.85f, 1f));

            // The patient cat.
            Vector3 cat = Plinth(kit, new Vector3(-46f, 0f, -97f), new Vector3(3f, 1.4f, 4f));
            var basalt = PatchworkAssets.Plain(new Color(0.2f, 0.22f, 0.28f), 0.7f);
            var g = kit.Group("StoneCat", cat);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.1f, -0.3f), new Vector3(1.5f, 2.2f, 1.8f), basalt, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.5f, 0.5f), new Vector3(1.2f, 1f, 1.3f), basalt, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 2.5f, 0.2f), new Vector3(1.1f, 1.05f, 1.1f), basalt, default(Vector3), g);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.34f, 3.1f, 0.15f), new Vector3(0.4f, 0.55f, 0.14f), basalt, new Vector3(0f, 0f, s * -14f), g);
                kit.Deco(PrimitiveType.Capsule, new Vector3(s * 0.32f, 0.5f, 0.85f), new Vector3(0.34f, 0.5f, 0.34f), basalt, default(Vector3), g);
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.22f, 2.6f, 0.68f), Vector3.one * 0.16f, PatchworkAssets.Glow(new Color(0.4f, 1f, 0.7f), 1.2f), default(Vector3), g));
            }
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.95f, 0.15f), new Vector3(0.95f, 0.07f, 0.95f), Brass, default(Vector3), g));
            kit.Deco(PrimitiveType.Capsule, new Vector3(0.75f, 0.3f, -0.4f), new Vector3(0.22f, 0.8f, 0.22f), basalt, new Vector3(90f, 30f, 0f), g);
            Plaque(kit, new Vector3(-46f, 0f, -93.8f), 0f, "The Patient Cat",
                "In ancient Egypt cats were treasured: they kept the mice out of the grain. Families shaved their eyebrows in mourning when the household cat died.",
                "This one has sat exactly like this for twenty-six centuries, and would like you to know it is still waiting for its dinner.");

            // Shelves of jars along the south wall.
            var clay = new[] { PatchworkAssets.Felt(new Color(0.78f, 0.5f, 0.36f)), PatchworkAssets.Felt(new Color(0.9f, 0.8f, 0.62f)), PatchworkAssets.Felt(new Color(0.3f, 0.5f, 0.6f)) };
            for (float x = -84f; x <= -66f; x += 6f)
            {
                kit.Block(new Vector3(x, 0.6f, -103.2f), new Vector3(5f, 1.2f, 1.2f), WoodCol, "JarShelf", DarkWood);
                for (int i = 0; i < 4; i++)
                {
                    Vector3 p = new Vector3(x - 1.8f + i * 1.2f, 1.2f, -103.2f);
                    var m = clay[(i + Mathf.Abs(Mathf.RoundToInt(x))) % 3];
                    float s = 0.7f + (i % 2) * 0.25f;
                    kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 0.5f * s, new Vector3(0.75f, 1f, 0.75f) * s, m);
                    kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 1.05f * s, new Vector3(0.3f, 0.18f, 0.3f) * s, m);
                    kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 1.22f * s, new Vector3(0.42f, 0.03f, 0.42f) * s, m);
                }
            }
            // The stone of three scripts.
            Vector3 rs = Plinth(kit, new Vector3(-52f, 0f, -88f), new Vector3(2.6f, 0.7f, 1.6f));
            var slab = kit.Deco(PrimitiveType.Cube, rs + new Vector3(0f, 1.1f, 0f), new Vector3(1.9f, 2.2f, 0.4f), PatchworkAssets.Plain(new Color(0.22f, 0.22f, 0.26f), 0.4f), new Vector3(-8f, 0f, 0f));
            var chalk = PatchworkAssets.Felt(new Color(0.9f, 0.88f, 0.8f));
            for (int i = 0; i < 12; i++)
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.42f - i * 0.075f, 0.52f), new Vector3(i < 4 ? 0.8f : i < 8 ? 0.72f : 0.84f, 0.025f, 0.02f), chalk, default(Vector3), slab));
            Plaque(kit, new Vector3(-52f, 0f, -86.2f), 0f, "The Rosetta Stone (a copy)",
                "One message, carved three times in three different kinds of writing. Because scholars could read the Greek at the bottom, they could at last work out the Egyptian picture-writing at the top.",
                "It was found in 1799. Cracking the code took another twenty-three years.");
            Rug(kit, new Vector3(-62f, 0f, -90f), new Vector2(30f, 5f), PlumStars);
        }

        // ---------------------------------------------------------------- The great lizard

        static void Lizard(LevelKit kit)
        {
            var bone = Bone;
            Plinth(kit, new Vector3(-62f, 0f, -64f), new Vector3(8f, 0.8f, 28f));
            RopeLine(kit, new Vector3(-57f, 0f, -77.5f), new Vector3(-57f, 0f, -50.5f), Wine);
            Vector3 tail = new Vector3(-62f, 2.4f, -77f), head = new Vector3(-62f, 8.6f, -53.6f);
            Spine(kit, tail, head, 2.2f, 0.75f, 3f, 0.42f, 0.7f);
            // Hips and the two great legs.
            Vector3 hip = Vector3.Lerp(tail, head, 0.5f) + Vector3.up * 2.2f;
            NoShadow(kit.Deco(PrimitiveType.Sphere, hip + Vector3.down * 0.4f, new Vector3(2.6f, 1.5f, 2.2f), bone));
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 a = hip + new Vector3(s * 1.1f, -0.6f, 0f);
                Vector3 knee = a + new Vector3(s * 0.3f, -2.8f, 1.5f);
                Vector3 ankle = knee + new Vector3(0f, -2.2f, -1.6f);
                Vector3 toe = new Vector3(ankle.x, 0.95f, ankle.z + 1.6f);
                Beam(kit, a, knee, 0.55f, bone);
                Beam(kit, knee, ankle, 0.42f, bone);
                Beam(kit, ankle, toe, 0.32f, bone);
                NoShadow(kit.Deco(PrimitiveType.Sphere, knee, Vector3.one * 0.7f, bone));
                for (int t = -1; t <= 1; t++)
                    Beam(kit, toe, toe + new Vector3(t * 0.4f, -0.1f, 0.9f), 0.2f, bone);
                // The silly little arms.
                Vector3 sh = Vector3.Lerp(tail, head, 0.74f) + new Vector3(s * 0.8f, 0.2f, 0f);
                Beam(kit, sh, sh + new Vector3(s * 0.2f, -1f, 0.7f), 0.2f, bone);
                Beam(kit, sh + new Vector3(s * 0.2f, -1f, 0.7f), sh + new Vector3(s * 0.15f, -1.2f, 1.5f), 0.16f, bone);
            }
            // The skull: a big box of a head with a jaw full of felt teeth, looking down at you.
            var skull = kit.Group("LizardSkull", head + new Vector3(0f, 0.3f, 0.6f));
            skull.rotation = Quaternion.Euler(24f, 0f, 0f);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.3f, 1.2f), new Vector3(1.5f, 1.2f, 2.8f), bone, default(Vector3), skull));
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.5f, 0f), new Vector3(1.8f, 1.6f, 1.6f), bone, default(Vector3), skull));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.75f, 1f), new Vector3(1.3f, 0.36f, 2.6f), bone, new Vector3(-12f, 0f, 0f), skull));
            var hole = PatchworkAssets.Felt(new Color(0.3f, 0.25f, 0.22f));
            for (int s = -1; s <= 1; s += 2)
            {
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.74f, 0.55f, 0.5f), new Vector3(0.14f, 0.6f, 0.6f), hole, default(Vector3), skull));
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.74f, 0.4f, 1.9f), new Vector3(0.14f, 0.36f, 0.5f), hole, default(Vector3), skull));
                for (int t = 0; t < 7; t++)
                {
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.66f, -0.42f, 0.2f + t * 0.36f), new Vector3(0.1f, 0.34f, 0.16f), PatchworkAssets.Felt(CreamCol), new Vector3(0f, 0f, 0f), skull));
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.56f, -0.5f, 0.3f + t * 0.33f), new Vector3(0.09f, 0.26f, 0.14f), PatchworkAssets.Felt(CreamCol), new Vector3(-12f, 0f, 0f), skull));
                }
            }
            Plaque(kit, new Vector3(-55.6f, 0f, -58f), 90f, "Tyrannosaurus rex",
                "The \"tyrant lizard king\". Twelve metres long, with teeth the size of bananas and arms too short to scratch its own nose.",
                "It lived about sixty-seven million years ago. To put that another way: Tyrannosaurus lived closer in time to you than it did to Stegosaurus.",
                "The first one was dug out of the hills of Montana in 1902.");

            // The sea dragon, hung from the rafters to the north of it.
            Vector3 pt = new Vector3(-76f, 12f, -45f), ph = new Vector3(-48f, 12.6f, -45f);
            Spine(kit, pt, ph, 0.9f, 0.5f, 1.7f, 0.3f, 0.6f);
            NoShadow(kit.Deco(PrimitiveType.Cube, ph + new Vector3(1f, 0f, 0f), new Vector3(1.8f, 0.5f, 0.7f), bone));
            NoShadow(kit.Deco(PrimitiveType.Cube, ph + new Vector3(0.9f, -0.34f, 0f), new Vector3(1.5f, 0.16f, 0.6f), bone, new Vector3(0f, 0f, 8f)));
            foreach (float k in new[] { 0.32f, 0.58f })
            {
                Vector3 p = Vector3.Lerp(pt, ph, k) + Vector3.up * 0.7f;
                for (int s = -1; s <= 1; s += 2)
                {
                    Beam(kit, p + new Vector3(0f, -0.6f, s * 0.8f), p + new Vector3(-0.6f, -1.3f, s * 2.4f), 0.24f, bone);
                    for (int f = 0; f < 4; f++)
                        Beam(kit, p + new Vector3(-0.6f, -1.3f, s * 2.4f), p + new Vector3(-1.2f - f * 0.5f, -1.8f - f * 0.1f, s * (4.4f - f * 0.5f)), 0.13f, bone);
                }
                Beam(kit, p, new Vector3(p.x, HistTop, p.z), 0.06f, IronMat);
            }
            Beam(kit, ph, new Vector3(ph.x, HistTop, ph.z), 0.06f, IronMat);
            Plaque(kit, new Vector3(-66f, 0f, -46f), 180f, "Plesiosaurus",
                "A sea reptile with four paddles and a neck like a snake threaded through a turtle. Not a dinosaur, though it swam while they walked.",
                "The first whole skeleton was found in 1823 by Mary Anning, who had been digging fossils out of the cliffs at Lyme Regis since she was a girl. She found it; for years, grander people took the credit.");

            // The pendulum.
            Vector3 pc = new Vector3(-79f, 0f, -66f);
            kit.Block(pc + Vector3.up * 0.2f, new Vector3(9f, 0.4f, 9f), StoneCol, "PendulumBase", PaleStone).rotation = Rot(45f);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, pc + Vector3.up * 0.42f, new Vector3(7.6f, 0.02f, 7.6f), PatchworkAssets.Felt(new Color(0.9f, 0.82f, 0.62f))));
            for (int i = 0; i < 24; i++)
            {
                float a = i / 24f * Mathf.PI * 2f;
                NoShadow(kit.Deco(PrimitiveType.Cylinder, pc + new Vector3(Mathf.Cos(a) * 3.5f, 0.56f, Mathf.Sin(a) * 3.5f), new Vector3(0.1f, 0.14f, 0.1f), Brass));
            }
            var pivot = kit.Group("Pendulum", new Vector3(pc.x, HistTop - 0.6f, pc.z));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -7.8f, 0f), new Vector3(0.035f, 7.8f, 0.035f), IronMat, default(Vector3), pivot));
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -15.9f, 0f), Vector3.one * 0.95f, Brass, default(Vector3), pivot));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -16.5f, 0f), new Vector3(0.06f, 0.16f, 0.06f), Brass, default(Vector3), pivot));
            var swing = pivot.gameObject.AddComponent<PendulumSwing>();
            swing.Angle = 11f;
            swing.Period = 7.6f;
            Plaque(kit, new Vector3(-73.6f, 0f, -66f), 90f, "Foucault's Pendulum",
                "A weight on a long wire, swinging. Watch it for a while and its line creeps slowly round the circle. But the pendulum isn't turning: the floor is. You are watching the Earth spin.",
                "Léon Foucault first showed this in Paris in 1851. (Ours has been hurried up a little, so you needn't wait all night.)");

            // Fossils in cases along the west wall.
            Vector3 f1 = DisplayCase(kit, new Vector3(-85f, 0f, -84f), new Vector3(2.2f, 2.4f, 2.6f));
            var tri = kit.Group("Trilobite", f1 + Vector3.up * 0.2f);
            var shell = PatchworkAssets.Felt(new Color(0.42f, 0.38f, 0.36f));
            for (int i = 0; i < 8; i++)
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, -0.6f + i * 0.16f), new Vector3(1f - Mathf.Abs(i - 3.5f) * 0.12f, 0.24f, 0.2f), shell, default(Vector3), tri));
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.02f, 0.78f), new Vector3(1f, 0.3f, 0.6f), shell, default(Vector3), tri));
            Plaque(kit, new Vector3(-82.6f, 0f, -84f), 90f, "Trilobite",
                "One of the first animals on Earth to have proper eyes, half a billion years ago. The lenses were made of crystal: it looked out at the world through stone.");
            Vector3 f2 = DisplayCase(kit, new Vector3(-85f, 0f, -52f), new Vector3(2.2f, 2.4f, 3f));
            Swimming(kit, f2 + Vector3.up * 0.7f, Swimmer.Fish, 1.5f, new Color(0.3f, 0.4f, 0.62f), new Color(0.75f, 0.8f, 0.9f), 0f);
            Plaque(kit, new Vector3(-82.6f, 0f, -52f), 90f, "Coelacanth",
                "Everybody knew this fish had died out with the dinosaurs. Then, in 1938, a museum curator called Marjorie Courtenay-Latimer spotted one in a fisherman's catch in South Africa.",
                "It had been quietly swimming about the whole time. Sixty-six million years is a long while to go unnoticed.");
            Rug(kit, new Vector3(-49f, 0f, -64f), new Vector2(5f, 26f), StarCloth);
        }

        // A stitched swimmer standing still as an exhibit (nose towards yaw).
        static void Swimming(LevelKit kit, Vector3 at, Swimmer kind, float size, Color body, Color trim, float yaw)
        {
            var g = kit.Group("Exhibit" + kind, at);
            g.rotation = Rot(yaw);
            StitchSwimmer(kit, g, kind, size, body, trim, true);
        }

        // ---------------------------------------------------------------- Ships and stars

        static void ShipsAndStars(LevelKit kit)
        {
            // ----- The longship in the rafters -----
            Vector3 ship = new Vector3(-62f, 9.4f, -32f);   // the middle of its deck
            Longship(kit, ship);
            Level1Builder.MakeHeartstring(kit, "L2_hs_longship", "The Longship", ship + new Vector3(-3.5f, 1.7f, 0f), false);
            kit.ThreadLine(ship + new Vector3(-7f, 1.2f, 0f), ship + new Vector3(5f, 1.2f, 0f), 5);
            Plaque(kit, new Vector3(-62f, 0f, -37f), 180f, "The Longship",
                "Viking ships were so shallow they could sail up rivers, and so light the crew could pick them up and carry them. In ships like this one, Norse sailors reached North America about the year 1000: nearly five hundred years before Columbus.",
                "They had no compass. They steered by the sun, the stars, the birds and the colour of the sea.");
            // The painters' scaffold beside the bow: the way up for anyone who can jump.
            Vector3 sc = new Vector3(-47.6f, 0f, -32f);
            var pole = PatchworkAssets.Wood(new Color(0.8f, 0.66f, 0.45f));
            var plank = PatchworkAssets.Planks(new Color(0.78f, 0.6f, 0.4f));
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    kit.Deco(PrimitiveType.Cylinder, sc + new Vector3(sx * 3.1f, 5.2f, sz * 1.9f), new Vector3(0.16f, 5.2f, 0.16f), pole);
            for (int i = 0; i < 6; i++)
            {
                float y = 1.5f * (i + 1);
                float x = i % 2 == 0 ? 1.55f : -1.55f;
                if (i == 5) x = -1.2f;
                kit.Block(sc + new Vector3(x, y - 0.12f, 0f), new Vector3(i == 5 ? 3.6f : 2.9f, 0.24f, 3.4f), WoodCol, "ScaffoldPlank", plank);
                for (int sz = -1; sz <= 1; sz += 2)
                    kit.Rope(sc + new Vector3(-3.1f, y - 0.3f, sz * 1.9f), sc + new Vector3(3.1f, y - 0.3f, sz * 1.9f), pole, 0.1f);
                if (i < 5) ScoreBubbles.Place(kit, sc + new Vector3(x, y + 1.1f, 0f), 10);
            }
            // Paint pots and a dust sheet left on the floor.
            NoShadow(kit.Deco(PrimitiveType.Cube, sc + new Vector3(0f, 0.03f, 3.2f), new Vector3(5f, 0.04f, 2.6f), PatchworkAssets.Felt(CreamCol), new Vector3(0f, 8f, 0f)));
            Color[] paints = { new Color(0.8f, 0.25f, 0.25f), new Color(0.95f, 0.8f, 0.3f), new Color(0.25f, 0.4f, 0.75f) };
            for (int i = 0; i < 3; i++)
            {
                Vector3 p = sc + new Vector3(-1.4f + i * 1.2f, 0f, 3.1f + (i % 2) * 0.5f);
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.25f, new Vector3(0.5f, 0.25f, 0.5f), PatchworkAssets.Plain(new Color(0.8f, 0.8f, 0.85f), 0.7f));
                NoShadow(kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.51f, new Vector3(0.44f, 0.01f, 0.44f), PatchworkAssets.Plain(paints[i], 0.6f)));
            }
            Person(kit, sc + new Vector3(2.2f, 0f, 5f), 200f, ToyNpc.Kind.RagDoll, "Dab", new Color(0.4f, 0.6f, 0.75f), new Color(1f, 0.85f, 0.5f), 1.3f, () => new[]
            {
                "I'm touching up the dragon's eyebrows. He's been cross for a thousand years and I think he's earned a rest.",
                "The scaffold's safe as houses. Zig-zag up it and you can hop straight onto the bow."
            });
            // From the upper gallery: a lantern on a long chain to swing across on.
            kit.Deco(PrimitiveType.Cube, new Vector3(-43f, HistTop - 1f, -32f), new Vector3(0.5f, 0.5f, 6f), DarkWood);
            kit.Anchor(new Vector3(-42.5f, 13.4f, -32f), 13.5f, LevelKit.AnchorStyle.Lantern, HistTop - 1.4f - 13.4f - 0.6f);
            kit.Hint(new Vector3(-35f, 9.5f, -32f), new Vector3(6f, 4f, 9f), "Across the Rafters", "Yarn Form can swing from the pink lantern across to the scaffold and the longship's bow.", HintTheme.Yarn);
            // And a fountain by the stern that carries Water Form up to the deck.
            kit.Fountain(new Vector3(-74.6f, 0f, -32f), 10.6f, LevelKit.FountainStyle.Pipe);
            kit.Hint(new Vector3(-74.6f, 1.5f, -29f), new Vector3(6f, 4f, 6f), "The Ship's Pump", "Water Form can ride this jet up to the longship's stern.", HintTheme.Water);

            // ----- Under the ship: how people found their way -----
            // The great globe.
            Vector3 gl = new Vector3(-72f, 0f, -18f);
            Plinth(kit, gl, new Vector3(3.4f, 0.6f, 3.4f), 45f);
            var globe = kit.Group("GreatGlobe", gl + Vector3.up * 3.1f);
            globe.rotation = Quaternion.Euler(0f, 0f, 23.4f);
            NoShadow(kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 3.6f, PatchworkAssets.GlobeMap(), default(Vector3), globe));
            globe.gameObject.AddComponent<Spinner>().Speed = 7f;
            var meridian = kit.Group("Meridian", gl + Vector3.up * 3.1f);
            meridian.rotation = Quaternion.Euler(90f, 0f, 0f);
            Hoop(kit, meridian, 2f, 0.14f, Brass, 26);
            for (int i = 0; i < 3; i++)
                Beam(kit, gl + Vector3.up * 0.6f + Rot(i * 120f) * new Vector3(0f, 0f, 1.3f), gl + Vector3.up * 1.3f, 0.16f, Brass);
            TownKit.Solid(kit, gl + Vector3.up * 2.8f, new Vector3(3f, 4.4f, 3f), 0f);
            Plaque(kit, new Vector3(-68.8f, 0f, -18f), 90f, "Round the World",
                "In 1519 five ships set out from Spain to sail west until they got back to where they started. Three years later one ship limped home with eighteen men aboard.",
                "They had sailed right round the Earth, and so proved, the hard way, how very much of it is sea.");
            // The Antikythera mechanism.
            Vector3 ak = DisplayCase(kit, new Vector3(-84f, 0f, -30f), new Vector3(2.4f, 2.5f, 2.4f));
            var bronze = PatchworkAssets.Plain(new Color(0.35f, 0.55f, 0.45f), 0.5f);
            for (int i = 0; i < 5; i++)
                NoShadow(kit.Deco(PrimitiveType.Cylinder, ak + new Vector3(-0.4f + (i % 3) * 0.4f, 0.5f + (i / 3) * 0.45f - (i % 2) * 0.1f, 0f), new Vector3(0.5f + (i % 2) * 0.3f, 0.03f, 0.5f + (i % 2) * 0.3f), i % 2 == 0 ? bronze : Brass, new Vector3(0f, 0f, 90f)));
            NoShadow(kit.Deco(PrimitiveType.Cube, ak + new Vector3(0.1f, 0.6f, 0f), new Vector3(0.08f, 1.1f, 1.3f), DarkWood));
            Plaque(kit, new Vector3(-81.4f, 0f, -30f), 90f, "The Antikythera Mechanism",
                "Divers found this in a shipwreck off a Greek island in 1901: a lump of green bronze the size of a shoebox. Inside were more than thirty gear wheels.",
                "It is a machine for working out where the sun, the moon and the planets will be, and when the next eclipse is due. It was made two thousand years ago. Nothing else so clever would be built for a thousand years after.");
            // Halley's diving bell, by the door to the galleries.
            Vector3 db = new Vector3(-48f, 0f, -17f);
            Plinth(kit, db, new Vector3(3.2f, 0.5f, 3.2f));
            var lead = PatchworkAssets.Plain(new Color(0.45f, 0.48f, 0.52f), 0.6f);
            var bell = kit.Group("DivingBell", db + Vector3.up * 0.5f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.2f, 0f), new Vector3(2.4f, 1.2f, 2.4f), DarkWood, default(Vector3), bell);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 2.4f, 0f), new Vector3(2.4f, 1.5f, 2.4f), lead, default(Vector3), bell);
            for (int i = 0; i < 3; i++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.3f + i * 0.9f, 0f), new Vector3(2.5f, 0.06f, 2.5f), Brass, default(Vector3), bell);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.5f, -1.18f), new Vector3(0.7f, 0.05f, 0.7f), PatchworkAssets.Glow(new Color(0.6f, 0.9f, 1f), 1.2f), new Vector3(90f, 0f, 0f), bell));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.5f, -1.2f), new Vector3(0.9f, 0.03f, 0.9f), Brass, new Vector3(90f, 0f, 0f), bell));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 3.3f, 0f), new Vector3(0.3f, 0.2f, 0.3f), Brass, default(Vector3), bell);
            Beam(kit, db + Vector3.up * 4f, new Vector3(db.x, HistTop, db.z), 0.1f, IronMat);
            TownKit.Solid(kit, db + Vector3.up * 2f, new Vector3(2.4f, 3.4f, 2.4f), 0f);
            Plaque(kit, new Vector3(-48f, 0f, -20.2f), 180f, "The Diving Bell",
                "Turn a cup upside down and push it into a bowl of water: the air stays inside. Now make the cup big enough to sit in. That is a diving bell.",
                "In 1691 the astronomer Edmond Halley, the one the comet is named after, built one and sat on the sea bed for an hour and a half. Barrels of fresh air were lowered down to him on ropes.");
            // The wayfinders' canoe.
            Vector3 cn = DisplayCase(kit, new Vector3(-84f, 0f, -16f), new Vector3(2.2f, 2.3f, 4.4f));
            NoShadow(kit.Deco(PrimitiveType.Capsule, cn + new Vector3(-0.3f, 0.3f, 0f), new Vector3(0.36f, 1.6f, 0.36f), DarkWood, new Vector3(90f, 0f, 0f)));
            NoShadow(kit.Deco(PrimitiveType.Capsule, cn + new Vector3(0.45f, 0.24f, 0f), new Vector3(0.2f, 1.2f, 0.2f), DarkWood, new Vector3(90f, 0f, 0f)));
            NoShadow(kit.Deco(PrimitiveType.Cube, cn + new Vector3(0.08f, 0.4f, 0f), new Vector3(0.8f, 0.05f, 0.1f), DarkWood));
            NoShadow(kit.Deco(PrimitiveType.Cube, cn + new Vector3(-0.3f, 0.9f, 0f), new Vector3(0.04f, 1.1f, 0.04f), DarkWood));
            NoShadow(kit.Deco(PrimitiveType.Cube, cn + new Vector3(-0.3f, 1f, 0.3f), new Vector3(0.03f, 0.8f, 0.8f), PatchworkAssets.Felt(new Color(0.85f, 0.7f, 0.5f)), new Vector3(20f, 0f, 0f)));
            Plaque(kit, new Vector3(-81.6f, 0f, -16f), 90f, "The Wayfinders",
                "The people of the Pacific crossed thousands of miles of open ocean in canoes like this, and found islands no bigger than a dot on the map.",
                "They carried the whole sky in their heads: which star rises where, how the swells run, which way the birds fly home at dusk. No instruments at all.");
            // An astrolabe on the wall by the north door.
            var astro = kit.Group("Astrolabe", new Vector3(-67f, 5f, -10.05f));
            astro.rotation = Quaternion.Euler(90f, 0f, 0f);
            NoShadow(kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(3.4f, 0.06f, 3.4f), Brass, default(Vector3), astro));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.05f, 0f), new Vector3(2.8f, 0.03f, 2.8f), StarCloth, default(Vector3), astro));
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.1f, 0f), new Vector3(0.12f, 0.04f, 3.2f), Brass, new Vector3(0f, 35f, 0f), astro));
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0f, -1.95f), Vector3.one * 0.5f, Brass, default(Vector3), astro));
            Plaque(kit, new Vector3(-67f, 0f, -11.6f), 180f, "The Astrolabe",
                "Hold it up, line the pointer on a star, and it will tell you the time, how far north you are, and which way is home. A pocket-sized model of the sky.",
                "Scholars in Baghdad, Cairo and Córdoba made the finest ones, a thousand years ago.");
            Banner(kit, new Vector3(-53f, 15f, -10.05f), 180f, 3f, 7f, StarCloth, 2);
            Banner(kit, new Vector3(-67f, 15f, -10.05f), 180f, 3f, 7f, PlumStars, 3);
            Rug(kit, new Vector3(-60f, 0f, -15f), new Vector2(7f, 9f), StarCloth, 0f, PatchworkAssets.Felt(new Color(1f, 0.82f, 0.4f)));
        }

        // A Viking longship hung from the roof on iron rods. "deck" is the middle of its deck;
        // it lies along x with its dragon's head to the east. You can walk its whole length.
        static void Longship(LevelKit kit, Vector3 deck)
        {
            var oak = PatchworkAssets.Planks(new Color(0.5f, 0.33f, 0.22f));
            var dark = DarkWood;
            // The deck (solid) and the hull under it: fat in the middle, narrowing to each end.
            kit.Block(deck + new Vector3(0f, -0.2f, 0f), new Vector3(20f, 0.4f, 3.4f), WoodCol, "ShipDeck", PatchworkAssets.Planks(new Color(0.72f, 0.55f, 0.38f)));
            float[] wide = { 1.4f, 2.6f, 3.4f, 3.8f, 3.8f, 3.4f, 2.6f, 1.4f };
            for (int i = 0; i < 8; i++)
            {
                float x = -8.75f + i * 2.5f;
                float end = Mathf.Abs(i - 3.5f) / 3.5f;
                kit.Deco(PrimitiveType.Cube, deck + new Vector3(x, -0.9f + end * 0.5f, 0f), new Vector3(2.52f, 1.2f - end * 0.5f, wide[i]), oak);
                kit.Deco(PrimitiveType.Cube, deck + new Vector3(x, -1.7f + end * 0.8f, 0f), new Vector3(2.52f, 0.5f, wide[i] * 0.55f), dark);
                // The gunwale: a low rail each side (solid, so you don't roll off) with shields on it.
                for (int s = -1; s <= 1; s += 2)
                {
                    kit.Deco(PrimitiveType.Cube, deck + new Vector3(x, 0.3f, s * (wide[i] * 0.5f - 0.05f)), new Vector3(2.52f, 0.6f, 0.2f), oak);
                    if (i > 0 && i < 7)
                    {
                        var shield = PatchworkAssets.Print(i % 2 == 0 ? PatchworkAssets.PrintKind.Stripes : PatchworkAssets.PrintKind.Polka, i % 3 == 0 ? Wine : i % 3 == 1 ? Navy : new Color(0.25f, 0.45f, 0.35f), new Color(0.98f, 0.85f, 0.5f));
                        NoShadow(kit.Deco(PrimitiveType.Cylinder, deck + new Vector3(x, 0.1f, s * (wide[i] * 0.5f + 0.12f)), new Vector3(1.5f, 0.06f, 1.5f), shield, new Vector3(90f, 0f, 0f)));
                        NoShadow(kit.Deco(PrimitiveType.Sphere, deck + new Vector3(x, 0.1f, s * (wide[i] * 0.5f + 0.2f)), Vector3.one * 0.36f, Brass));
                    }
                }
            }
            for (int s = -1; s <= 1; s += 2)
                TownKit.Solid(kit, deck + new Vector3(0f, 0.3f, s * 1.75f), new Vector3(15f, 0.6f, 0.2f), 0f);
            // The dragon's head rearing up at the bow, and the curl of its tail at the stern.
            Vector3 bow = deck + new Vector3(10f, 0f, 0f);
            Arc(kit, bow + new Vector3(-0.4f, -1f, 0f), bow + new Vector3(1.4f, 3.2f, 0f), 0.01f, 0.5f, 0.5f, oak, 6);
            var headGroup = kit.Group("DragonHead", bow + new Vector3(1.5f, 3.5f, 0f));
            headGroup.rotation = Quaternion.Euler(0f, 90f, 0f) * Quaternion.Euler(-10f, 0f, 0f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, 0.5f), new Vector3(0.7f, 0.7f, 1.5f), oak, default(Vector3), headGroup);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.4f, 0.7f), new Vector3(0.5f, 0.2f, 1.1f), oak, new Vector3(14f, 0f, 0f), headGroup);
            for (int s = -1; s <= 1; s += 2)
            {
                NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.36f, 0.16f, 0.6f), Vector3.one * 0.22f, PatchworkAssets.Glow(new Color(1f, 0.8f, 0.3f), 1.5f), default(Vector3), headGroup));
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.22f, 0.55f, 0f), new Vector3(0.1f, 0.6f, 0.2f), oak, new Vector3(-30f, 0f, s * 14f), headGroup);
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.3f, 0.42f, 0.5f), new Vector3(0.1f, 0.08f, 0.44f), Brass, new Vector3(0f, 0f, s * -20f), headGroup);
            }
            Vector3 stern = deck + new Vector3(-10f, 0f, 0f);
            Arc(kit, stern + new Vector3(0.4f, -1f, 0f), stern + new Vector3(-1.2f, 2.8f, 0f), 0.01f, 0.45f, 0.45f, oak, 6);
            Arc(kit, stern + new Vector3(-1.2f, 2.8f, 0f), stern + new Vector3(-0.3f, 2.9f, 0f), 0.6f, 0.34f, 0.4f, oak, 6);
            // The mast and a striped sail, furled up high so there's room to walk under it.
            kit.Deco(PrimitiveType.Cylinder, deck + new Vector3(0.5f, 3.7f, 0f), new Vector3(0.3f, 3.7f, 0.3f), dark);
            TownKit.Solid(kit, deck + new Vector3(0.5f, 1.5f, 0f), new Vector3(0.4f, 3f, 0.4f), 0f);
            kit.Deco(PrimitiveType.Cylinder, deck + new Vector3(0.5f, 6.4f, 0f), new Vector3(0.18f, 4f, 0.18f), dark, new Vector3(90f, 0f, 0f));
            var sail = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, Wine, CreamCol);
            NoShadow(kit.Deco(PrimitiveType.Cube, deck + new Vector3(0.6f, 4.9f, 0f), new Vector3(0.1f, 3f, 7.4f), sail));
            // Iron rods up to the roof.
            foreach (float x in new[] { -7.5f, -2.5f, 3.5f, 7.5f })
                for (int s = -1; s <= 1; s += 2)
                    Beam(kit, deck + new Vector3(x, 0.5f, s * 1.5f), new Vector3(deck.x + x, HistTop, deck.z + s * 0.6f), 0.07f, IronMat);
            PointLight(kit, deck + new Vector3(-3f, 3f, 0f), LampGold, 12f, 1.1f);
        }
    }
}

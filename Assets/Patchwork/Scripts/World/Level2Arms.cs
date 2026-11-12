using UnityEngine;

namespace Patchwork
{
    // The Hall of Arms: a long, lamplit hall off the south terrace of the Sunken Ruins. Down
    // each side stand the armours of eight different peoples, on plinths, facing each other
    // across a red carpet (their helmets turn to watch you pass). In the middle a knight sits
    // his horse; at the far end are two model castles, one of them big enough to climb.
    // It's a quiet side room for now, a little apart from the way round.
    public static partial class Level2Builder
    {
        static readonly Color Steel = new Color(0.72f, 0.75f, 0.8f);
        static readonly Color Lacquer = new Color(0.55f, 0.1f, 0.12f);

        enum Warrior { Knight, Hoplite, Legionary, Viking, Samurai, Terracotta, Mongol, Eagle }

        static void HallOfArms(LevelKit kit)
        {
            var stone = PatchworkAssets.AgedBrick(new Color(0.62f, 0.5f, 0.46f));
            float T = ArmsTop, cx = (ArmsX0 + ArmsX1) * 0.5f;
            Color[] quilt = { new Color(0.4f, 0.16f, 0.18f), new Color(0.3f, 0.14f, 0.2f), new Color(0.46f, 0.24f, 0.2f), new Color(0.26f, 0.16f, 0.26f), new Color(0.5f, 0.3f, 0.22f) };
            Floor(kit, ArmsX0, ArmsX1, ArmsZ0, 6f, 0f, quilt, 4f, "ArmsFloor");
            Wall(kit, new Vector3(ArmsX0 - 1f, 0f, ArmsZ0 - 2f), new Vector3(ArmsX0 - 1f, 0f, 4f), -1f, T + 4f, 2f, stone);
            Wall(kit, new Vector3(ArmsX1 + 1f, 0f, ArmsZ0 - 2f), new Vector3(ArmsX1 + 1f, 0f, 4f), -1f, T + 4f, 2f, stone);
            Wall(kit, new Vector3(ArmsX0 - 2f, 0f, ArmsZ0 - 1f), new Vector3(ArmsX1 + 2f, 0f, ArmsZ0 - 1f), -1f, T + 4f, 2f, stone);
            // A timber roof on great curved trusses.
            kit.Block(new Vector3(cx, T + 0.5f, -24f), new Vector3(44f, 1f, 60f), WoodCol, "ArmsCeiling", PatchworkAssets.Planks(new Color(0.26f, 0.17f, 0.14f)));
            for (float z = -45f; z <= 0f; z += 9f)
            {
                Arc(kit, new Vector3(ArmsX0, 12f, z), new Vector3(ArmsX1, 12f, z), 7.4f, 0.7f, 0.8f, DarkWood, 16);
                kit.Deco(PrimitiveType.Cube, new Vector3(cx, 12f, z), new Vector3(40f, 0.5f, 0.6f), DarkWood);
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Cube, new Vector3(cx + s * 19.5f, 6f, z), new Vector3(1f, 12f, 1.4f), PaleStone);
            }
            // Stained glass down both sides, lit from behind.
            Color[] glassCols = { new Color(0.9f, 0.25f, 0.25f), new Color(0.25f, 0.45f, 0.95f), new Color(0.95f, 0.75f, 0.25f), new Color(0.3f, 0.75f, 0.45f), new Color(0.7f, 0.35f, 0.85f) };
            int gi = 0;
            for (float z = -40.5f; z <= -4f; z += 9f)
                for (int s = -1; s <= 1; s += 2)
                    StainedGlass(kit, new Vector3(s > 0 ? ArmsX1 - 0.06f : ArmsX0 + 0.06f, 5.5f, z), s > 0 ? 270f : 90f, 4f, 9f, glassCols, gi++);
            // Iron chandeliers down the middle.
            foreach (float z in new[] { -4.5f, -13.5f, -40.5f })
            {
                Vector3 c = new Vector3(cx, 11f, z);
                var ring = kit.Group("Chandelier", c);
                Hoop(kit, ring, 2.4f, 0.16f, IronMat, 20);
                for (int i = 0; i < 8; i++)
                {
                    float a = i / 8f * Mathf.PI * 2f;
                    Vector3 p = c + new Vector3(Mathf.Cos(a) * 2.4f, 0f, Mathf.Sin(a) * 2.4f);
                    NoShadow(kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.3f, new Vector3(0.12f, 0.22f, 0.12f), PatchworkAssets.Felt(CreamCol)));
                    NoShadow(kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 0.66f, new Vector3(0.14f, 0.24f, 0.14f), PatchworkAssets.Glow(new Color(1f, 0.75f, 0.35f), 3f)));
                    if (i % 2 == 0) Beam(kit, p, new Vector3(c.x, c.y + 3f, c.z), 0.05f, IronMat);
                }
                Beam(kit, c + Vector3.up * 3f, new Vector3(c.x, T, c.z), 0.07f, IronMat);
                PointLight(kit, c + Vector3.down * 0.5f, new Color(1f, 0.76f, 0.46f), 24f, 1.8f);
            }

            // ----- The aisle -----
            var red = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stars, Wine, new Color(1f, 0.82f, 0.4f));
            Rug(kit, new Vector3(cx, 0f, -6f), new Vector2(5f, 19f), red, 0f, PatchworkAssets.Felt(new Color(1f, 0.82f, 0.4f)));
            Rug(kit, new Vector3(cx - 6.5f, 0f, -29f), new Vector2(4f, 22f), red);
            Rug(kit, new Vector3(cx + 6.5f, 0f, -29f), new Vector2(4f, 22f), red);
            kit.AddCheckpoint(new Vector3(cx, 0f, 1f), 180f);
            TrailLine(kit, new Vector3(cx, 1.2f, 2f), new Vector3(cx, 1.2f, -14f), 6);
            kit.Hint(new Vector3(cx, 1.5f, 0f), new Vector3(10f, 4f, 7f), "The Hall of Arms",
                "Armour from all over the world, and a model castle at the far end you're allowed to climb. (Mind the helmets. They look.)");

            // ----- The armours: four a side -----
            float wx = cx - 13f, ex = cx + 13f;
            Armour(kit, new Vector3(wx, 0f, -4f), 90f, Warrior.Knight);
            Plaque(kit, new Vector3(wx + 2.6f, 0f, -1.6f), 90f, "A Knight's Harness",
                "A full suit of plate armour, made to measure about 1480. It weighs about as much as a seven-year-old, spread all over the body, and a knight in it could run, climb a ladder and vault onto his horse.",
                "The idea that knights had to be lifted into the saddle with a crane is a joke somebody made up much later.");
            Armour(kit, new Vector3(wx, 0f, -13f), 90f, Warrior.Hoplite);
            Plaque(kit, new Vector3(wx + 2.6f, 0f, -10.6f), 90f, "The Hoplite",
                "A citizen-soldier of ancient Greece. His great round shield covered his own left side and the right side of the man next to him, so the whole line stood or fell together.",
                "The helmet is beaten out of a single sheet of bronze.");
            Armour(kit, new Vector3(wx, 0f, -22f), 90f, Warrior.Legionary);
            Plaque(kit, new Vector3(wx + 2.6f, 0f, -19.6f), 90f, "The Legionary",
                "A Roman soldier carried his armour, his shield, two javelins, a spade, a cooking pot and three days' food, and marched thirty kilometres a day with the lot.",
                "On the order \"testudo\", a squad locked their shields overhead and on every side, and walked forward like a tortoise.");
            Armour(kit, new Vector3(wx, 0f, -31f), 90f, Warrior.Viking);
            Plaque(kit, new Vector3(wx + 2.6f, 0f, -28.6f), 90f, "The Norse Raider",
                "Mail shirt, round wooden shield, an iron cap. Notice what the helmet doesn't have: horns. No Viking helmet with horns has ever been found. An artist added them a thousand years later, for an opera.");
            Armour(kit, new Vector3(ex, 0f, -4f), 270f, Warrior.Samurai);
            Plaque(kit, new Vector3(ex - 2.6f, 0f, -1.6f), 270f, "Samurai Armour",
                "Hundreds of small lacquered plates, laced together with silk cord: metres and metres of it, in the colours of the wearer's house. Light, flexible, and very nearly sewn rather than forged.",
                "The crest on the helmet was so that friends could find you in the crush.");
            Armour(kit, new Vector3(ex, 0f, -13f), 270f, Warrior.Terracotta);
            Plaque(kit, new Vector3(ex - 2.6f, 0f, -10.6f), 270f, "The Terracotta Army",
                "In 1974 some farmers in China, digging a well, struck the head of a clay soldier. He was one of about eight thousand, life-sized, buried in ranks to guard the tomb of the First Emperor.",
                "They had stood in the dark for twenty-two centuries. No two of their faces are the same. (This one is a copy.)");
            Armour(kit, new Vector3(ex, 0f, -22f), 270f, Warrior.Mongol);
            Plaque(kit, new Vector3(ex - 2.6f, 0f, -19.6f), 270f, "The Steppe Archer",
                "The riders of the Mongol empire could shoot from the saddle at full gallop, forwards or backwards, loosing the arrow at the moment all four hooves were off the ground.",
                "Their bow is made of horn, wood and sinew glued in layers: small enough to use on horseback, and stronger than a longbow.");
            Armour(kit, new Vector3(ex, 0f, -31f), 270f, Warrior.Eagle);
            Plaque(kit, new Vector3(ex - 2.6f, 0f, -28.6f), 270f, "The Eagle Warrior",
                "Among the Aztecs, the bravest soldiers earned the right to dress as eagles or jaguars. The armour underneath is quilted cotton, two fingers thick, soaked in salt water to stiffen it.",
                "It worked so well against arrows that the Spanish took off their steel and wore it too. Padded patchwork, saving lives.");

            // ----- The horseman in the middle -----
            Horseman(kit, new Vector3(cx, 0f, -27f));
            Plaque(kit, new Vector3(cx, 0f, -21.4f), 0f, "The Joust",
                "Two riders, a wooden fence between them, and a lance each: the idea was to break yours on the other fellow's shield. The horse wore armour as well, and a quilted coat in its rider's colours.",
                "It was the great sport of its day. People collected favourite champions the way they collect footballers now.");

            // ----- The far end: weapons on the wall, the long embroidery, the castles -----
            WeaponWall(kit, new Vector3(cx, 0f, ArmsZ0 + 0.1f));
            Embroidery(kit, new Vector3(ArmsX0 + 0.12f, 3.4f, -20f), 90f, 26f);
            Plaque(kit, new Vector3(ArmsX0 + 3f, 0f, -20f), 90f, "The Long Embroidery",
                "The Bayeux Tapestry isn't a tapestry at all. It's embroidery: wool stitched onto linen, seventy metres of it, telling the story of the Norman invasion of England in 1066 like a comic strip.",
                "It has 626 people in it, 190 horses, 37 ships, and one comet. It is nearly a thousand years old and you can still see every stitch.");
            ModelKeep(kit, new Vector3(ArmsX0 + 9f, 0f, ArmsZ0 + 9f));
            Tenshu(kit, new Vector3(ArmsX1 - 8f, 0f, ArmsZ0 + 8f));
            Plaque(kit, new Vector3(ArmsX1 - 13.4f, 0f, ArmsZ0 + 8f), 90f, "The White Heron",
                "Himeji Castle in Japan, finished in 1609 and never once taken. The paths up to the keep double back, split, and dead-end on purpose, so that an attacker gets thoroughly lost with people dropping things on him.",
                "Its white plaster walls are meant to look like a heron about to take off.");
            // The chessmen, in a case by the door.
            Vector3 cc = DisplayCase(kit, new Vector3(cx + 9f, 0f, 0.6f), new Vector3(2.6f, 2.3f, 1.8f));
            for (int i = 0; i < 6; i++)
            {
                Vector3 p = cc + new Vector3(-0.9f + i * 0.36f, 0f, (i % 2) * 0.3f - 0.15f);
                var ivory = PatchworkAssets.Felt(i % 3 == 2 ? new Color(0.7f, 0.25f, 0.2f) : new Color(0.93f, 0.88f, 0.74f));
                NoShadow(kit.Deco(PrimitiveType.Capsule, p + Vector3.up * 0.2f, new Vector3(0.22f, 0.2f, 0.2f), ivory));
                NoShadow(kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 0.46f, Vector3.one * 0.17f, ivory));
                if (i % 2 == 0) NoShadow(kit.Deco(PrimitiveType.Cube, p + Vector3.up * 0.58f, new Vector3(0.18f, 0.06f, 0.18f), ivory));
            }
            Plaque(kit, new Vector3(cx + 9f, 0f, -1.2f), 180f, "The Lewis Chessmen",
                "Carved from walrus tusk about 1150 and found in a sand dune on a Scottish island in 1831. The queens are holding their faces in their hands, the rooks are biting their shields, and every one of them looks deeply worried.");
            Person(kit, new Vector3(cx - 8f, 0f, -1f), 140f, ToyNpc.Kind.Nutcracker, "The Castellan", new Color(0.5f, 0.16f, 0.2f), new Color(1f, 0.82f, 0.4f), 0.75f, () => new[]
            {
                "Welcome to the Hall of Arms. Touch nothing sharp. Which is to say, touch nothing.",
                "They turn their heads, you know. I've told the Professor. He says it's the draught.",
                "The castle at the far end is a model, and models are for climbing. There's something shiny on the tallest tower."
            });
            // Banners: tall narrow war flags along the east side, pennants along the west.
            for (int i = 0; i < 4; i++)
            {
                float z = -9f - i * 9f;
                Banner(kit, new Vector3(cx + 10f, 11.7f, z + 0.5f), 0f, 1.8f, 6f, PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, i % 2 == 0 ? Lacquer : Navy, CreamCol), 1 + (i % 3));
                Banner(kit, new Vector3(cx - 10f, 11.7f, z + 0.5f), 0f, 2.6f, 4.6f, PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, i % 2 == 0 ? Navy : Wine, new Color(1f, 0.82f, 0.4f)), i % 4);
            }
            kit.ThreadLine(new Vector3(cx - 6.5f, 1.3f, -20f), new Vector3(cx - 6.5f, 1.3f, -36f), 6);
            Trail(kit, 10, new Vector3(cx + 6.5f, 1.2f, -20f), new Vector3(cx + 6.5f, 1.2f, -24f), new Vector3(cx + 6.5f, 1.2f, -28f), new Vector3(cx + 6.5f, 1.2f, -32f), new Vector3(cx + 6.5f, 1.2f, -36f));
            foreach (var p in new[] { new Vector3(cx, 6f, -10f), new Vector3(cx, 6f, -32f) })
                AmbientEmitter.Create(kit.Root, p, Sfx.Ambient(Sfx.AmbientClip.Hall), 0.5f, 8f, 30f);
        }

        // A tall stained-glass window: coloured panes in lead, glowing, with a pointed top.
        static void StainedGlass(LevelKit kit, Vector3 foot, float yaw, float w, float h, Color[] cols, int seed)
        {
            var g = kit.Group("StainedGlass", foot);
            g.rotation = Rot(yaw);
            var rng = new System.Random(seed * 17 + 3);
            int rows = 6;
            float cellH = (h - w * 0.5f) / rows, cellW = w / 3f;
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < 3; c++)
                {
                    Color col = cols[rng.Next(cols.Length)];
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(-w * 0.5f + cellW * (c + 0.5f), cellH * (r + 0.5f), 0f), new Vector3(cellW - 0.08f, cellH - 0.08f, 0.05f), PatchworkAssets.Glow(col, 1.25f), default(Vector3), g));
                }
            // The pointed head: a rose of five panes.
            float top = h - w * 0.5f;
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, top + w * 0.22f, 0f), new Vector3(w * 0.5f, 0.025f, w * 0.5f), PatchworkAssets.Glow(cols[seed % cols.Length], 1.5f), new Vector3(90f, 0f, 0f), g));
            for (int i = 0; i < 5; i++)
            {
                float a = (i / 5f + 0.05f) * Mathf.PI * 2f;
                NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(Mathf.Cos(a) * w * 0.3f, top + w * 0.22f + Mathf.Sin(a) * w * 0.3f, -0.005f), new Vector3(w * 0.28f, 0.02f, w * 0.28f), PatchworkAssets.Glow(cols[(seed + i + 1) % cols.Length], 1.3f), new Vector3(90f, 0f, 0f), g));
            }
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, h * 0.5f + 0.2f, -0.06f), new Vector3(w + 0.5f, h + 1f, 0.06f), IronMat, default(Vector3), g));
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(s * (w * 0.5f + 0.25f), h * 0.5f, 0.1f), new Vector3(0.4f, h + 0.6f, 0.4f), PaleStone, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.2f, 0.15f), new Vector3(w + 1.2f, 0.36f, 0.6f), PaleStone, default(Vector3), g);
            // Coloured light spilling onto the floor.
            PointLight(kit, foot + Rot(yaw) * new Vector3(0f, 1f, 3f), cols[seed % cols.Length], 10f, 0.9f);
        }

        // A suit of armour on a dummy, on a plinth, facing "yaw". The helmet watches Patch.
        static void Armour(LevelKit kit, Vector3 foot, float yaw, Warrior who)
        {
            Plinth(kit, foot, new Vector3(2.6f, 0.6f, 2.6f), yaw);
            var g = kit.Group("Armour" + who, foot + Vector3.up * 0.6f);
            g.rotation = Rot(yaw);
            Color main, trim, cloth;
            switch (who)
            {
                case Warrior.Hoplite: main = new Color(0.8f, 0.6f, 0.3f); trim = new Color(0.75f, 0.2f, 0.2f); cloth = CreamCol; break;
                case Warrior.Legionary: main = Steel; trim = new Color(0.75f, 0.15f, 0.15f); cloth = new Color(0.7f, 0.15f, 0.15f); break;
                case Warrior.Viking: main = new Color(0.5f, 0.52f, 0.56f); trim = new Color(0.55f, 0.4f, 0.25f); cloth = new Color(0.3f, 0.4f, 0.5f); break;
                case Warrior.Samurai: main = Lacquer; trim = new Color(1f, 0.8f, 0.35f); cloth = Navy; break;
                case Warrior.Terracotta: main = new Color(0.7f, 0.52f, 0.4f); trim = new Color(0.6f, 0.44f, 0.34f); cloth = new Color(0.66f, 0.5f, 0.38f); break;
                case Warrior.Mongol: main = new Color(0.4f, 0.3f, 0.25f); trim = new Color(0.85f, 0.65f, 0.3f); cloth = new Color(0.25f, 0.4f, 0.6f); break;
                case Warrior.Eagle: main = new Color(0.9f, 0.85f, 0.7f); trim = new Color(0.2f, 0.65f, 0.55f); cloth = new Color(0.85f, 0.3f, 0.2f); break;
                default: main = Steel; trim = new Color(0.9f, 0.72f, 0.38f); cloth = Navy; break;
            }
            bool metal = who == Warrior.Knight || who == Warrior.Legionary || who == Warrior.Hoplite || who == Warrior.Viking;
            var m = metal ? PatchworkAssets.Plain(main, 0.78f) : PatchworkAssets.Felt(main);
            var tm = who == Warrior.Terracotta ? PatchworkAssets.Felt(trim) : PatchworkAssets.Plain(trim, 0.7f);
            var cm = PatchworkAssets.Fabric(cloth);
            var wood = DarkWood;
            // Legs, skirt, body, arms.
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Capsule, new Vector3(s * 0.26f, 0.55f, 0f), new Vector3(0.34f, 0.55f, 0.34f), who == Warrior.Knight ? m : cm, default(Vector3), g);
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.26f, 0.1f, 0.1f), new Vector3(0.3f, 0.2f, 0.52f), who == Warrior.Knight ? m : wood, default(Vector3), g);
                kit.Deco(PrimitiveType.Capsule, new Vector3(s * 0.62f, 1.55f, 0f), new Vector3(0.28f, 0.48f, 0.28f), who == Warrior.Knight || who == Warrior.Samurai ? m : cm, new Vector3(0f, 0f, s * 12f), g);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.56f, 1.98f, 0f), new Vector3(0.5f, 0.36f, 0.5f), m, default(Vector3), g);
            }
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 1.6f, 0f), new Vector3(0.82f, 0.56f, 0.62f), m, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.18f, 0f), new Vector3(0.86f, 0.06f, 0.68f), tm, default(Vector3), g);
            // The skirt: plates, a kilt of strips, or a quilted coat.
            int strips = who == Warrior.Knight ? 0 : 7;
            for (int i = 0; i < strips; i++)
            {
                float a = (i / (float)strips) * Mathf.PI * 2f;
                kit.Deco(PrimitiveType.Cube, new Vector3(Mathf.Sin(a) * 0.4f, 0.86f, Mathf.Cos(a) * 0.32f), new Vector3(0.3f, 0.6f, 0.07f), i % 2 == 0 ? (who == Warrior.Samurai || who == Warrior.Mongol ? m : cm) : tm,
                    new Vector3(-10f, a * Mathf.Rad2Deg, 0f), g);
            }
            if (who == Warrior.Knight) kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1.05f, 0f), new Vector3(0.9f, 0.5f, 0.7f), m, default(Vector3), g);
            if (who == Warrior.Samurai)
                for (int s = -1; s <= 1; s += 2)   // the big square shoulder guards
                    for (int r = 0; r < 3; r++)
                        kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.78f, 1.95f - r * 0.2f, 0f), new Vector3(0.1f, 0.19f, 0.6f), r % 2 == 0 ? m : tm, new Vector3(0f, 0f, s * -14f), g);
            if (who == Warrior.Legionary)
                for (int r = 0; r < 4; r++)   // bands of plate round the chest
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.34f + r * 0.17f, 0f), new Vector3(0.88f, 0.03f, 0.68f), tm, default(Vector3), g);
            if (who == Warrior.Eagle)
                for (int s = -1; s <= 1; s += 2)   // feathered wings down the arms
                    for (int f = 0; f < 4; f++)
                        kit.Deco(PrimitiveType.Cube, new Vector3(s * (0.8f + f * 0.05f), 1.8f - f * 0.26f, -0.1f), new Vector3(0.5f, 0.07f, 0.24f), f % 2 == 0 ? tm : PatchworkAssets.Felt(CreamCol), new Vector3(0f, 0f, s * (-50f + f * 6f)), g);

            // The head (which turns), and whatever goes on it.
            var head = new GameObject("Helm").transform;
            head.SetParent(g, false);
            head.localPosition = new Vector3(0f, 2.36f, 0f);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.56f, 0.6f, 0.6f), who == Warrior.Terracotta || who == Warrior.Eagle ? PatchworkAssets.Felt(who == Warrior.Eagle ? trim : main) : m, default(Vector3), head);
            var dark = PatchworkAssets.Plain(new Color(0.06f, 0.06f, 0.08f), 0.3f);
            switch (who)
            {
                case Warrior.Knight:
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.02f, 0.24f), new Vector3(0.5f, 0.3f, 0.2f), m, new Vector3(20f, 0f, 0f), head);
                    NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.06f, 0.33f), new Vector3(0.4f, 0.04f, 0.04f), dark, default(Vector3), head));
                    kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.5f, -0.2f), new Vector3(0.16f, 0.4f, 0.3f), PatchworkAssets.Yarn(new Color(0.8f, 0.2f, 0.25f)), new Vector3(-35f, 0f, 0f), head);
                    break;
                case Warrior.Hoplite:
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.14f, 0.27f), new Vector3(0.1f, 0.34f, 0.08f), m, default(Vector3), head);
                    for (int s = -1; s <= 1; s += 2) kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.2f, -0.2f, 0.2f), new Vector3(0.16f, 0.3f, 0.14f), m, default(Vector3), head);
                    for (int i = 0; i < 7; i++)   // the horsehair crest, front to back
                        kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.34f + Mathf.Sin(i / 6f * Mathf.PI) * 0.26f, 0.3f - i * 0.11f), new Vector3(0.09f, 0.36f, 0.12f), tm, new Vector3(-40f + i * 14f, 0f, 0f), head);
                    break;
                case Warrior.Legionary:
                    for (int s = -1; s <= 1; s += 2) kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.25f, -0.2f, 0.12f), new Vector3(0.08f, 0.3f, 0.26f), m, default(Vector3), head);
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.12f, 0.3f), new Vector3(0.56f, 0.06f, 0.12f), tm, default(Vector3), head);
                    for (int i = 0; i < 7; i++)   // the crest, side to side
                        kit.Deco(PrimitiveType.Cube, new Vector3(-0.3f + i * 0.1f, 0.36f + Mathf.Sin(i / 6f * Mathf.PI) * 0.16f, 0f), new Vector3(0.1f, 0.32f, 0.1f), tm, default(Vector3), head);
                    break;
                case Warrior.Viking:
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.06f, 0f), new Vector3(0.62f, 0.04f, 0.66f), tm, default(Vector3), head);
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.08f, 0.3f), new Vector3(0.07f, 0.3f, 0.06f), m, default(Vector3), head);
                    for (int s = -1; s <= 1; s += 2)
                        NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.13f, -0.02f, 0.29f), new Vector3(0.2f, 0.02f, 0.2f), m, new Vector3(90f, 0f, 0f), head));
                    kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.4f, 0.16f), new Vector3(0.44f, 0.5f, 0.3f), PatchworkAssets.Yarn(new Color(0.75f, 0.5f, 0.25f)), default(Vector3), head);
                    break;
                case Warrior.Samurai:
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -0.06f, -0.06f), new Vector3(0.9f, 0.04f, 0.9f), m, new Vector3(-8f, 0f, 0f), head);
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.2f, 0.26f), new Vector3(0.4f, 0.26f, 0.12f), dark, default(Vector3), head);
                    for (int s = -1; s <= 1; s += 2)   // the crest: a pair of gold horns like a crescent moon
                        kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.26f, 0.5f, 0.2f), new Vector3(0.09f, 0.62f, 0.04f), tm, new Vector3(0f, 0f, s * -28f), head);
                    kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.3f, 0.26f), new Vector3(0.18f, 0.18f, 0.06f), tm, default(Vector3), head);
                    break;
                case Warrior.Terracotta:
                    kit.Deco(PrimitiveType.Sphere, new Vector3(0.08f, 0.36f, -0.08f), new Vector3(0.26f, 0.3f, 0.26f), tm, default(Vector3), head);   // a topknot
                    for (int s = -1; s <= 1; s += 2)
                        NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.12f, 0.04f, 0.27f), new Vector3(0.1f, 0.05f, 0.04f), dark, default(Vector3), head));
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.1f, 0.28f), new Vector3(0.2f, 0.04f, 0.06f), tm, default(Vector3), head);
                    break;
                case Warrior.Mongol:
                    kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.3f, 0f), new Vector3(0.3f, 0.4f, 0.3f), tm, default(Vector3), head);
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.02f, 0f), new Vector3(0.74f, 0.07f, 0.74f), PatchworkAssets.Yarn(new Color(0.5f, 0.38f, 0.28f)), default(Vector3), head);
                    for (int s = -1; s <= 1; s += 2) kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.28f, -0.22f, -0.04f), new Vector3(0.1f, 0.4f, 0.34f), cm, default(Vector3), head);
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.62f, 0f), new Vector3(0.05f, 0.14f, 0.05f), tm, default(Vector3), head);
                    break;
                default:   // the eagle: a beak for a helmet, the face looking out of it
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.2f, 0.36f), new Vector3(0.3f, 0.2f, 0.5f), PatchworkAssets.Plain(new Color(1f, 0.8f, 0.3f), 0.6f), new Vector3(24f, 0f, 0f), head);
                    kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.06f, 0.2f), new Vector3(0.38f, 0.4f, 0.3f), PatchworkAssets.Felt(new Color(0.8f, 0.6f, 0.45f)), default(Vector3), head);
                    for (int i = 0; i < 5; i++)
                        kit.Deco(PrimitiveType.Cube, new Vector3((i - 2) * 0.13f, 0.44f, -0.16f), new Vector3(0.1f, 0.5f, 0.05f), i % 2 == 0 ? tm : PatchworkAssets.Felt(CreamCol), new Vector3(-30f, 0f, (i - 2) * -12f), head);
                    break;
            }
            head.gameObject.AddComponent<WatchPlayer>();

            // Shield and weapon.
            Vector3 hand = new Vector3(0.82f, 1.2f, 0.16f), off = new Vector3(-0.86f, 1.45f, 0.26f);
            switch (who)
            {
                case Warrior.Knight:
                    kit.Deco(PrimitiveType.Cube, off, new Vector3(0.08f, 0.9f, 0.7f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, Navy, new Color(1f, 0.82f, 0.4f)), new Vector3(0f, 0f, 6f), g);
                    kit.Deco(PrimitiveType.Cube, hand + new Vector3(0f, 0.1f, 0.2f), new Vector3(0.07f, 1.9f, 0.14f), PatchworkAssets.Plain(Steel, 0.85f), default(Vector3), g);
                    kit.Deco(PrimitiveType.Cube, hand + new Vector3(0f, 0.86f, 0.2f), new Vector3(0.1f, 0.08f, 0.5f), tm, default(Vector3), g);
                    break;
                case Warrior.Hoplite:
                    kit.Deco(PrimitiveType.Cylinder, off, new Vector3(1.3f, 0.06f, 1.3f), m, new Vector3(0f, 0f, 90f), g);
                    NoShadow(kit.Deco(PrimitiveType.Cylinder, off + new Vector3(-0.07f, 0f, 0f), new Vector3(0.7f, 0.02f, 0.7f), tm, new Vector3(0f, 0f, 90f), g));
                    kit.Deco(PrimitiveType.Cylinder, hand + new Vector3(0f, 0.6f, 0f), new Vector3(0.06f, 1.7f, 0.06f), wood, default(Vector3), g);
                    kit.Deco(PrimitiveType.Sphere, hand + new Vector3(0f, 2.36f, 0f), new Vector3(0.12f, 0.4f, 0.05f), m, default(Vector3), g);
                    break;
                case Warrior.Legionary:
                    kit.Deco(PrimitiveType.Cube, off + new Vector3(0f, -0.2f, 0f), new Vector3(0.1f, 1.5f, 0.9f), PatchworkAssets.Felt(new Color(0.7f, 0.15f, 0.15f)), default(Vector3), g);
                    NoShadow(kit.Deco(PrimitiveType.Sphere, off + new Vector3(-0.06f, -0.2f, 0f), new Vector3(0.12f, 0.3f, 0.3f), PatchworkAssets.Plain(new Color(0.9f, 0.72f, 0.38f), 0.8f), default(Vector3), g));
                    kit.Deco(PrimitiveType.Cylinder, hand + new Vector3(0f, 0.5f, 0f), new Vector3(0.05f, 1.4f, 0.05f), wood, default(Vector3), g);
                    kit.Deco(PrimitiveType.Cylinder, hand + new Vector3(0f, 2.1f, 0f), new Vector3(0.025f, 0.4f, 0.025f), m, default(Vector3), g);
                    break;
                case Warrior.Viking:
                    kit.Deco(PrimitiveType.Cylinder, off, new Vector3(1.2f, 0.05f, 1.2f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.25f, 0.45f, 0.35f), CreamCol), new Vector3(0f, 0f, 90f), g);
                    NoShadow(kit.Deco(PrimitiveType.Sphere, off + new Vector3(-0.08f, 0f, 0f), Vector3.one * 0.3f, m, default(Vector3), g));
                    kit.Deco(PrimitiveType.Cylinder, hand + new Vector3(0f, 0.3f, 0f), new Vector3(0.06f, 0.9f, 0.06f), wood, default(Vector3), g);
                    kit.Deco(PrimitiveType.Cube, hand + new Vector3(0f, 1.1f, 0.2f), new Vector3(0.05f, 0.4f, 0.5f), m, default(Vector3), g);
                    break;
                case Warrior.Samurai:
                    kit.Deco(PrimitiveType.Cube, hand + new Vector3(0f, 0.2f, 0.25f), new Vector3(0.04f, 1.5f, 0.08f), PatchworkAssets.Plain(Steel, 0.9f), new Vector3(8f, 0f, 0f), g);
                    kit.Deco(PrimitiveType.Cylinder, hand + new Vector3(0f, -0.52f, 0.15f), new Vector3(0.2f, 0.02f, 0.2f), tm, new Vector3(8f, 0f, 0f), g);
                    kit.Deco(PrimitiveType.Cylinder, hand + new Vector3(0f, -0.76f, 0.12f), new Vector3(0.06f, 0.22f, 0.06f), cm, new Vector3(8f, 0f, 0f), g);
                    // A war fan in the other hand.
                    kit.Deco(PrimitiveType.Cylinder, off + new Vector3(0f, -0.2f, 0.1f), new Vector3(0.7f, 0.015f, 0.7f), tm, new Vector3(0f, 0f, 80f), g);
                    break;
                case Warrior.Terracotta:
                    kit.Deco(PrimitiveType.Cylinder, hand + new Vector3(0f, 0.6f, 0f), new Vector3(0.05f, 1.6f, 0.05f), wood, default(Vector3), g);
                    kit.Deco(PrimitiveType.Cube, hand + new Vector3(0f, 2.2f, 0.1f), new Vector3(0.04f, 0.14f, 0.5f), tm, default(Vector3), g);
                    break;
                case Warrior.Mongol:
                    Arc(kit, g.TransformPoint(off + new Vector3(0f, -0.7f, 0.1f)), g.TransformPoint(off + new Vector3(0f, 0.7f, 0.1f)), 0.01f, 0.06f, 0.06f, tm, 6);
                    for (int i = 0; i < 4; i++)   // a quiver of arrows on the hip
                        kit.Deco(PrimitiveType.Cylinder, new Vector3(0.5f + i * 0.05f, 1.15f, -0.3f), new Vector3(0.025f, 0.45f, 0.025f), wood, new Vector3(-20f, 0f, -10f + i * 5f), g);
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0.56f, 0.95f, -0.24f), new Vector3(0.22f, 0.3f, 0.14f), cm, new Vector3(-20f, 0f, 0f), g);
                    break;
                default:
                    kit.Deco(PrimitiveType.Cylinder, off, new Vector3(1f, 0.05f, 1f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, trim, new Color(1f, 0.8f, 0.3f)), new Vector3(0f, 0f, 90f), g);
                    for (int i = 0; i < 5; i++)
                        kit.Deco(PrimitiveType.Cube, off + new Vector3(-0.04f, -0.6f, -0.3f + i * 0.15f), new Vector3(0.03f, 0.4f, 0.08f), i % 2 == 0 ? tm : PatchworkAssets.Felt(CreamCol), default(Vector3), g);
                    // The wooden sword edged with black glass.
                    kit.Deco(PrimitiveType.Cube, hand + new Vector3(0f, 0.3f, 0.2f), new Vector3(0.05f, 1.2f, 0.16f), wood, default(Vector3), g);
                    for (int i = 0; i < 5; i++)
                        for (int s = -1; s <= 1; s += 2)
                            kit.Deco(PrimitiveType.Cube, hand + new Vector3(0f, 0.2f + i * 0.2f, 0.2f + s * 0.1f), new Vector3(0.03f, 0.14f, 0.07f), dark, default(Vector3), g);
                    break;
            }
            TownKit.Solid(kit, foot + Vector3.up * 2f, new Vector3(1.5f, 2.8f, 1.1f), yaw);
            PointLight(kit, foot + Rot(yaw) * new Vector3(0f, 3.4f, 2.2f), new Color(1f, 0.85f, 0.65f), 6.5f, 1.1f);
        }

        // A knight on his horse, both in armour and quilted cloth, on a long plinth.
        static void Horseman(LevelKit kit, Vector3 foot)
        {
            Plinth(kit, foot, new Vector3(3.4f, 0.8f, 6.4f));
            var g = kit.Group("Horseman", foot + Vector3.up * 0.8f);
            var coat = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stars, Navy, new Color(1f, 0.82f, 0.4f));
            var steel = PatchworkAssets.Plain(Steel, 0.8f);
            var hide = PatchworkAssets.Felt(new Color(0.35f, 0.25f, 0.2f));
            // The horse: body in its long coat, neck, armoured head, four legs, a yarn tail.
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 1.9f, 0f), new Vector3(1.3f, 1.5f, 1.4f), hide, new Vector3(90f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.6f, -0.2f), new Vector3(1.5f, 1.3f, 2.9f), coat, default(Vector3), g);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.76f, 1.15f, -0.2f), new Vector3(0.05f, 0.9f, 2.8f), coat, new Vector3(0f, 0f, s * 8f), g);
                foreach (float z in new[] { 1.05f, -1.15f })
                {
                    kit.Deco(PrimitiveType.Capsule, new Vector3(s * 0.4f, 0.62f, z), new Vector3(0.3f, 0.64f, 0.3f), hide, new Vector3(z > 0f ? 8f : -6f, 0f, 0f), g);
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.4f, 0.08f, z + (z > 0f ? 0.1f : -0.06f)), new Vector3(0.36f, 0.08f, 0.36f), PatchworkAssets.Plain(new Color(0.15f, 0.12f, 0.1f), 0.5f), default(Vector3), g);
                }
            }
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 2.7f, 1.5f), new Vector3(0.7f, 0.95f, 0.8f), hide, new Vector3(35f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 3.25f, 2.25f), new Vector3(0.5f, 0.6f, 0.56f), steel, new Vector3(70f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 3.62f, 2f), new Vector3(0.1f, 0.4f, 0.1f), PatchworkAssets.Plain(new Color(0.9f, 0.72f, 0.38f), 0.8f), new Vector3(20f, 0f, 0f), g);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.2f, 3.66f, 1.84f), new Vector3(0.1f, 0.26f, 0.06f), hide, new Vector3(0f, 0f, s * -14f), g);
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 1.7f, -1.9f), new Vector3(0.26f, 0.7f, 0.26f), PatchworkAssets.Yarn(new Color(0.2f, 0.15f, 0.12f)), new Vector3(-20f, 0f, 0f), g);
            for (int i = 0; i < 5; i++)
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 3.25f - i * 0.2f, 1.26f + i * 0.02f - i * i * 0.03f), new Vector3(0.16f, 0.34f, 0.3f), PatchworkAssets.Yarn(new Color(0.2f, 0.15f, 0.12f)), default(Vector3), g);
            // The rider.
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 3.2f, -0.1f), new Vector3(0.8f, 0.6f, 0.62f), steel, default(Vector3), g);
            var head = new GameObject("Helm").transform;
            head.SetParent(g, false);
            head.localPosition = new Vector3(0f, 4.05f, -0.1f);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.56f, 0.62f, 0.6f), steel, default(Vector3), head);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.02f, 0.26f), new Vector3(0.5f, 0.3f, 0.22f), steel, new Vector3(20f, 0f, 0f), head);
            kit.Deco(PrimitiveType.Capsule, new Vector3(0f, 0.56f, -0.2f), new Vector3(0.18f, 0.5f, 0.36f), PatchworkAssets.Yarn(new Color(1f, 0.82f, 0.4f)), new Vector3(-35f, 0f, 0f), head);
            head.gameObject.AddComponent<WatchPlayer>();
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Capsule, new Vector3(s * 0.62f, 2.4f, 0.2f), new Vector3(0.3f, 0.6f, 0.34f), steel, new Vector3(-25f, 0f, s * 10f), g);
                kit.Deco(PrimitiveType.Sphere, new Vector3(s * 0.56f, 3.56f, -0.1f), new Vector3(0.48f, 0.34f, 0.48f), steel, default(Vector3), g);
            }
            // Lance (with a pennant) and shield.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.82f, 3.3f, 1.2f), new Vector3(0.09f, 2.9f, 0.09f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, Wine, CreamCol), new Vector3(72f, 0f, 0f), g);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0.82f, 4.3f, 3.5f), new Vector3(0.03f, 0.4f, 0.8f), PatchworkAssets.Felt(Wine), new Vector3(-18f, 0f, 0f), g));
            kit.Deco(PrimitiveType.Cube, new Vector3(-0.78f, 3.1f, 0.3f), new Vector3(0.08f, 0.9f, 0.7f), coat, new Vector3(0f, 0f, 8f), g);
            TownKit.Solid(kit, foot + Vector3.up * 2.6f, new Vector3(1.8f, 3.8f, 4.6f), 0f);
            PointLight(kit, foot + new Vector3(0f, 6.5f, 3f), new Color(1f, 0.85f, 0.65f), 10f, 1.3f);
            RopeLine(kit, foot + new Vector3(-2.6f, 0f, 3.9f), foot + new Vector3(2.6f, 0f, 3.9f), Wine);
        }

        // Racks of weapons along a wall (the wall is at "foot", facing +z), with shields over them.
        static void WeaponWall(LevelKit kit, Vector3 foot)
        {
            var steel = PatchworkAssets.Plain(Steel, 0.85f);
            var wood = DarkWood;
            var gold = Brass;
            for (int r = 0; r < 2; r++)
            {
                Vector3 c = foot + new Vector3(r == 0 ? -5.5f : 5.5f, 0f, 0.5f);
                kit.Block(c + Vector3.up * 0.5f, new Vector3(7f, 1f, 0.8f), WoodCol, "WeaponRack", wood);
                kit.Deco(PrimitiveType.Cube, c + new Vector3(0f, 2.6f, -0.2f), new Vector3(7f, 0.14f, 0.3f), wood);
                for (int i = 0; i < 9; i++)
                {
                    Vector3 p = c + new Vector3(-3.2f + i * 0.8f, 1f, 0f);
                    int kind = (i + r * 2) % 4;
                    if (kind == 0)   // a sword, point down
                    {
                        kit.Deco(PrimitiveType.Cube, p + Vector3.up * 0.9f, new Vector3(0.14f, 1.6f, 0.04f), steel);
                        kit.Deco(PrimitiveType.Cube, p + Vector3.up * 1.72f, new Vector3(0.5f, 0.07f, 0.08f), gold);
                        kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 1.94f, new Vector3(0.07f, 0.2f, 0.07f), wood);
                    }
                    else if (kind == 1)   // a halberd
                    {
                        kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 1.6f, new Vector3(0.06f, 1.6f, 0.06f), wood);
                        kit.Deco(PrimitiveType.Cube, p + new Vector3(0.14f, 2.9f, 0f), new Vector3(0.36f, 0.5f, 0.04f), steel, new Vector3(0f, 0f, 12f));
                        kit.Deco(PrimitiveType.Sphere, p + Vector3.up * 3.36f, new Vector3(0.1f, 0.5f, 0.05f), steel);
                    }
                    else if (kind == 2)   // a curved blade on a long pole
                    {
                        kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 1.4f, new Vector3(0.05f, 1.4f, 0.05f), PatchworkAssets.Plain(Lacquer, 0.7f));
                        kit.Deco(PrimitiveType.Cube, p + new Vector3(0.06f, 3.2f, 0f), new Vector3(0.1f, 0.9f, 0.03f), steel, new Vector3(0f, 0f, -7f));
                    }
                    else   // a bow, unstrung
                        Arc(kit, p + new Vector3(0f, 0.1f, 0f), p + new Vector3(0f, 2f, 0f), 0.01f, 0.06f, 0.06f, wood, 6);
                }
            }
            // Shields in a row above, and a great two-handed sword in the middle.
            for (int i = 0; i < 7; i++)
            {
                if (i == 3) continue;
                Vector3 p = foot + new Vector3(-9f + i * 3f, 6.4f, 0.12f);
                var face = PatchworkAssets.Print(i % 2 == 0 ? PatchworkAssets.PrintKind.Stripes : PatchworkAssets.PrintKind.Stars, i % 3 == 0 ? Wine : i % 3 == 1 ? Navy : new Color(0.2f, 0.42f, 0.35f), new Color(1f, 0.82f, 0.4f));
                if (i % 2 == 0)
                {
                    NoShadow(kit.Deco(PrimitiveType.Cube, p, new Vector3(1.5f, 1.3f, 0.1f), face));
                    NoShadow(kit.Deco(PrimitiveType.Cube, p + Vector3.down * 0.76f, new Vector3(1.06f, 1.06f, 0.1f), face, new Vector3(0f, 0f, 45f)));
                }
                else NoShadow(kit.Deco(PrimitiveType.Cylinder, p, new Vector3(1.7f, 0.05f, 1.7f), face, new Vector3(90f, 0f, 0f)));
                NoShadow(kit.Deco(PrimitiveType.Sphere, p + new Vector3(0f, 0f, 0.08f), Vector3.one * 0.3f, gold));
            }
            NoShadow(kit.Deco(PrimitiveType.Cube, foot + new Vector3(0f, 6f, 0.12f), new Vector3(0.24f, 4.4f, 0.05f), steel));
            NoShadow(kit.Deco(PrimitiveType.Cube, foot + new Vector3(0f, 8f, 0.12f), new Vector3(1.3f, 0.12f, 0.1f), gold));
            NoShadow(kit.Deco(PrimitiveType.Sphere, foot + new Vector3(0f, 8.9f, 0.12f), Vector3.one * 0.3f, gold));
            PointLight(kit, foot + new Vector3(0f, 5f, 4f), new Color(1f, 0.85f, 0.65f), 14f, 1.2f);
        }

        // A long strip of embroidery on a wall: little stitched ships, horses, soldiers and a comet.
        static void Embroidery(LevelKit kit, Vector3 center, float yaw, float length)
        {
            var g = kit.Group("Embroidery", center);
            g.rotation = Rot(yaw);
            NoShadow(kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(length, 2.2f, 0.05f), PatchworkAssets.Felt(new Color(0.9f, 0.84f, 0.68f)), default(Vector3), g));
            var edge = PatchworkAssets.Felt(new Color(0.6f, 0.25f, 0.2f));
            for (int s = -1; s <= 1; s += 2)
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, s * 1f, 0.03f), new Vector3(length, 0.16f, 0.02f), edge, default(Vector3), g));
            NoShadow(kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.25f, 0f), new Vector3(0.1f, length * 0.5f + 0.3f, 0.1f), Brass, new Vector3(0f, 0f, 90f), g));
            Color[] wool = { new Color(0.6f, 0.25f, 0.2f), new Color(0.2f, 0.35f, 0.5f), new Color(0.75f, 0.6f, 0.25f), new Color(0.25f, 0.42f, 0.3f), new Color(0.3f, 0.22f, 0.2f) };
            var batch = new MeshBatch[wool.Length];
            for (int i = 0; i < batch.Length; i++) batch[i] = new MeshBatch();
            var rng = new System.Random(1066);
            int scenes = Mathf.RoundToInt(length / 2.2f);
            for (int i = 0; i < scenes; i++)
            {
                float x = -length * 0.5f + (i + 0.5f) * length / scenes;
                var a = batch[rng.Next(batch.Length)];
                var b = batch[rng.Next(batch.Length)];
                Vector3 o = new Vector3(x, -0.1f, 0.035f);
                switch (i % 4)
                {
                    case 0:   // a ship with a striped sail
                        a.Ball(o + new Vector3(0f, -0.4f, 0f), new Vector3(1.5f, 0.36f, 0.02f), 3, 8);
                        b.Box(o + new Vector3(0f, 0.1f, 0f), new Vector3(0.05f, 0.9f, 0.02f), Quaternion.identity);
                        b.Box(o + new Vector3(0f, 0.2f, 0f), new Vector3(0.9f, 0.5f, 0.02f), Quaternion.identity);
                        for (int k = 0; k < 4; k++) a.Ball(o + new Vector3(-0.5f + k * 0.33f, -0.26f, 0.005f), new Vector3(0.2f, 0.2f, 0.02f), 2, 6);
                        break;
                    case 1:   // a horse and rider
                        a.Ball(o + new Vector3(0f, -0.2f, 0f), new Vector3(1.1f, 0.44f, 0.02f), 3, 8);
                        a.Box(o + new Vector3(0.5f, 0.12f, 0f), new Vector3(0.2f, 0.5f, 0.02f), Quaternion.Euler(0f, 0f, -30f));
                        a.Ball(o + new Vector3(0.7f, 0.36f, 0f), new Vector3(0.36f, 0.2f, 0.02f), 2, 6);
                        for (int k = 0; k < 4; k++) a.Box(o + new Vector3(-0.4f + k * 0.26f, -0.6f, 0f), new Vector3(0.06f, 0.44f, 0.02f), Quaternion.Euler(0f, 0f, (k % 2 == 0 ? 14f : -14f)));
                        b.Ball(o + new Vector3(-0.05f, 0.3f, 0.005f), new Vector3(0.3f, 0.5f, 0.02f), 2, 6);
                        b.Ball(o + new Vector3(-0.05f, 0.64f, 0.005f), new Vector3(0.2f, 0.2f, 0.02f), 2, 6);
                        b.Box(o + new Vector3(0.3f, 0.5f, 0.005f), new Vector3(0.04f, 1f, 0.02f), Quaternion.Euler(0f, 0f, -60f));
                        break;
                    case 2:   // three soldiers with kite shields
                        for (int k = 0; k < 3; k++)
                        {
                            Vector3 p = o + new Vector3(-0.55f + k * 0.55f, 0f, 0f);
                            a.Box(p + new Vector3(0f, -0.3f, 0f), new Vector3(0.2f, 0.8f, 0.02f), Quaternion.identity);
                            a.Ball(p + new Vector3(0f, 0.24f, 0f), new Vector3(0.22f, 0.24f, 0.02f), 2, 6);
                            b.Ball(p + new Vector3(0.12f, -0.2f, 0.005f), new Vector3(0.26f, 0.6f, 0.02f), 2, 6);
                            b.Box(p + new Vector3(-0.14f, 0.1f, 0.005f), new Vector3(0.03f, 1.1f, 0.02f), Quaternion.Euler(0f, 0f, 8f));
                        }
                        break;
                    default:  // a hall, and over it the comet
                        a.Box(o + new Vector3(0f, -0.4f, 0f), new Vector3(1.2f, 0.6f, 0.02f), Quaternion.identity);
                        a.Box(o + new Vector3(0f, 0f, 0f), new Vector3(0.9f, 0.9f, 0.02f), Quaternion.Euler(0f, 0f, 45f));
                        b.Ball(o + new Vector3(0.3f, 0.66f, 0.005f), new Vector3(0.2f, 0.2f, 0.02f), 2, 6);
                        for (int k = 0; k < 4; k++) b.Box(o + new Vector3(0.06f - k * 0.02f, 0.74f + (k - 1.5f) * 0.05f, 0.005f), new Vector3(0.4f, 0.025f, 0.02f), Quaternion.Euler(0f, 0f, -12f + k * 6f));
                        break;
                }
            }
            for (int i = 0; i < batch.Length; i++) batch[i].Build(g, "Wool", PatchworkAssets.Yarn(wool[i]));
            for (float x = -length * 0.5f + 3f; x < length * 0.5f; x += 6.5f)
                PointLight(kit, center + Rot(yaw) * new Vector3(x, 1.2f, 2.2f), new Color(1f, 0.85f, 0.65f), 6f, 1f);
        }

        // A model castle built to be climbed: a curtain wall, corner towers of different
        // heights, and a keep in the middle with a golden button on its turret.
        static void ModelKeep(LevelKit kit, Vector3 c)
        {
            var wall = PatchworkAssets.Cobble(new Color(0.7f, 0.68f, 0.64f));
            var roof = PatchworkAssets.Shingle(new Color(0.55f, 0.2f, 0.22f));
            kit.Block(c + Vector3.up * 0.15f, new Vector3(13f, 0.3f, 13f), StoneCol, "KeepGreen", PatchworkAssets.Felt(new Color(0.36f, 0.5f, 0.32f)));
            // Curtain wall (1.2 high), with a gate in the front.
            kit.Block(c + new Vector3(-5f, 0.9f, 0f), new Vector3(1f, 1.2f, 9f), StoneCol, "KeepWall", wall);
            kit.Block(c + new Vector3(5f, 0.9f, 0f), new Vector3(1f, 1.2f, 9f), StoneCol, "KeepWall", wall);
            kit.Block(c + new Vector3(0f, 0.9f, -5f), new Vector3(9f, 1.2f, 1f), StoneCol, "KeepWall", wall);
            kit.Block(c + new Vector3(-3f, 0.9f, 5f), new Vector3(3f, 1.2f, 1f), StoneCol, "KeepWall", wall);
            kit.Block(c + new Vector3(3f, 0.9f, 5f), new Vector3(3f, 1.2f, 1f), StoneCol, "KeepWall", wall);
            kit.Deco(PrimitiveType.Cube, c + new Vector3(0f, 1.3f, 5f), new Vector3(3f, 0.4f, 1f), wall);
            NoShadow(kit.Deco(PrimitiveType.Cube, c + new Vector3(0f, 0.36f, 6.4f), new Vector3(2.2f, 0.1f, 2.4f), DarkWood, new Vector3(-6f, 0f, 0f)));
            // Corner towers, each a step taller than the last, so they make a stair: 2.4, 3.6, 4.8.
            Vector3[] corner = { new Vector3(5f, 0f, 5f), new Vector3(5f, 0f, -5f), new Vector3(-5f, 0f, -5f), new Vector3(-5f, 0f, 5f) };
            float[] high = { 2.4f, 3.6f, 4.8f, 2.4f };
            for (int i = 0; i < 4; i++)
            {
                kit.Block(c + corner[i] + Vector3.up * (0.3f + high[i] * 0.5f), new Vector3(2.4f, high[i], 2.4f), StoneCol, "KeepTower", wall);
                Battlements(kit, c + corner[i] + Vector3.up * (0.3f + high[i]), 2.4f, wall);
                if (i != 2) ScoreBubbles.Place(kit, c + corner[i] + Vector3.up * (high[i] + 1.5f), 10);
            }
            // Wall-walks between the towers on the east and south sides bridge the gaps.
            kit.Block(c + new Vector3(5f, 1.9f, 0f), new Vector3(1.2f, 1.6f, 7.6f), StoneCol, "KeepWalk", wall);
            kit.Block(c + new Vector3(0f, 3.1f, -5f), new Vector3(7.6f, 1.6f, 1.2f), StoneCol, "KeepWalk", wall);
            // The keep, and its turret.
            kit.Block(c + new Vector3(-1f, 3.3f, 0f), new Vector3(5f, 6f, 5f), StoneCol, "Keep", wall);
            Battlements(kit, c + new Vector3(-1f, 6.3f, 0f), 5f, wall);
            kit.Block(c + new Vector3(-1.6f, 6.9f, 0.6f), new Vector3(2.2f, 1.2f, 2.2f), StoneCol, "KeepTurret", wall);
            for (int i = 0; i < 4; i++)
                kit.Deco(PrimitiveType.Cube, c + new Vector3(-1.6f, 7.75f + i * 0.5f, 0.6f), new Vector3(2.4f - i * 0.56f, 0.5f, 2.4f - i * 0.56f), roof, new Vector3(0f, 45f, 0f));
            NoShadow(kit.Deco(PrimitiveType.Cube, c + new Vector3(-1.6f, 10.9f, 0.6f), new Vector3(0.05f, 1.4f, 0.05f), DarkWood));
            NoShadow(kit.Deco(PrimitiveType.Cube, c + new Vector3(-1.2f, 11.3f, 0.6f), new Vector3(0.8f, 0.5f, 0.03f), PatchworkAssets.Felt(Wine)));
            // Arrow slits and a door, drawn on.
            var dark = PatchworkAssets.Felt(new Color(0.2f, 0.18f, 0.2f));
            foreach (float x in new[] { -2.2f, 0.2f })
                NoShadow(kit.Deco(PrimitiveType.Cube, c + new Vector3(x, 4.6f, 2.52f), new Vector3(0.16f, 0.8f, 0.04f), dark));
            NoShadow(kit.Deco(PrimitiveType.Cube, c + new Vector3(-1f, 1.1f, 2.52f), new Vector3(1.2f, 1.6f, 0.04f), DarkWood));
            kit.GoldenButton("L2_b_keep", c + new Vector3(0.6f, 7.9f, -1.4f));
            SecretMarble.Place(kit, c + new Vector3(-2.6f, 1.2f, -3.4f), "L2_marble_armoury", new Color(0.9f, 0.4f, 0.4f));
            kit.Hint(c + new Vector3(2f, 1.5f, 8f), new Vector3(8f, 4f, 5f), "The Model Castle",
                "Hop onto the wall, then tower to tower: each is a step taller than the last. The golden button is on top of the keep.");
            Plaque(kit, c + new Vector3(4.2f, 0f, 8.2f), 0f, "A Castle",
                "A castle is a machine for making a few people very hard to get at. Every part has a job: the towers stick out so defenders can shoot along the wall, and the stairs wind clockwise so an attacker climbing them has no room to swing his sword arm.",
                "This model was built by the museum's carpenters, who would like it known that it has been climbed on since the day it was finished.");
            PointLight(kit, c + new Vector3(2f, 7f, 5f), new Color(1f, 0.85f, 0.65f), 14f, 1.2f);
        }

        static void Battlements(LevelKit kit, Vector3 top, float size, Material mat)
        {
            int n = Mathf.Max(2, Mathf.RoundToInt(size / 0.8f));
            float step = size / n;
            for (int i = 0; i < n; i += 2)
                for (int side = 0; side < 4; side++)
                {
                    float u = -size * 0.5f + (i + 0.5f) * step;
                    Vector3 p = side == 0 ? new Vector3(u, 0f, size * 0.5f - 0.15f) : side == 1 ? new Vector3(u, 0f, -size * 0.5f + 0.15f) : side == 2 ? new Vector3(size * 0.5f - 0.15f, 0f, u) : new Vector3(-size * 0.5f + 0.15f, 0f, u);
                    kit.Deco(PrimitiveType.Cube, top + p + Vector3.up * 0.2f, new Vector3(side < 2 ? step : 0.3f, 0.4f, side < 2 ? 0.3f : step), mat);
                }
        }

        // A model of a Japanese castle keep: a stone base and five white storeys under sweeping roofs.
        static void Tenshu(LevelKit kit, Vector3 c)
        {
            var stone = PatchworkAssets.Cobble(new Color(0.5f, 0.5f, 0.52f));
            var white = PatchworkAssets.Plaster(new Color(0.96f, 0.95f, 0.9f));
            var tile = PatchworkAssets.Shingle(new Color(0.25f, 0.3f, 0.36f));
            for (int i = 0; i < 3; i++)
                kit.Block(c + Vector3.up * (0.5f + i), new Vector3(8f - i * 0.7f, 1f, 8f - i * 0.7f), StoneCol, "TenshuBase", stone);
            float y = 3f, w = 5.6f;
            for (int i = 0; i < 5; i++)
            {
                kit.Deco(PrimitiveType.Cube, c + Vector3.up * (y + 0.7f), new Vector3(w, 1.4f, w), white);
                // The roof: a wide thin slab and a narrower one on it, corners turned up.
                kit.Deco(PrimitiveType.Cube, c + Vector3.up * (y + 1.5f), new Vector3(w + 1.5f, 0.16f, w + 1.5f), tile);
                kit.Deco(PrimitiveType.Cube, c + Vector3.up * (y + 1.66f), new Vector3(w + 0.7f, 0.2f, w + 0.7f), tile);
                for (int sx = -1; sx <= 1; sx += 2)
                    for (int sz = -1; sz <= 1; sz += 2)
                        kit.Deco(PrimitiveType.Cube, c + new Vector3(sx * (w * 0.5f + 0.75f), y + 1.62f, sz * (w * 0.5f + 0.75f)), new Vector3(0.5f, 0.12f, 0.5f), tile, new Vector3(sz * -18f, 45f, 0f));
                // Windows.
                var dark = PatchworkAssets.Glow(new Color(1f, 0.8f, 0.45f), 1.2f);
                for (float u = -w * 0.5f + 0.6f; u < w * 0.5f - 0.3f; u += 0.9f)
                {
                    NoShadow(kit.Deco(PrimitiveType.Cube, c + new Vector3(u, y + 0.8f, -w * 0.5f - 0.01f), new Vector3(0.3f, 0.5f, 0.03f), dark));
                    NoShadow(kit.Deco(PrimitiveType.Cube, c + new Vector3(-w * 0.5f - 0.01f, y + 0.8f, u), new Vector3(0.03f, 0.5f, 0.3f), dark));
                }
                y += 1.7f;
                w -= 0.9f;
            }
            // Gable and two golden fish on the ridge.
            kit.Deco(PrimitiveType.Cube, c + Vector3.up * (y + 0.2f), new Vector3(w + 1.2f, 0.5f, 0.5f), tile);
            for (int s = -1; s <= 1; s += 2)
                NoShadow(kit.Deco(PrimitiveType.Sphere, c + new Vector3(s * (w * 0.5f + 0.5f), y + 0.7f, 0f), new Vector3(0.24f, 0.6f, 0.3f), PatchworkAssets.Plain(new Color(1f, 0.8f, 0.3f), 0.85f), new Vector3(0f, 0f, s * 20f)));
            TownKit.Solid(kit, c + Vector3.up * 5.5f, new Vector3(6.6f, 11f, 6.6f), 0f);
            PointLight(kit, c + new Vector3(-4f, 6f, 5f), new Color(1f, 0.85f, 0.65f), 14f, 1.3f);
        }
    }
}

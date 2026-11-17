using UnityEngine;

namespace Patchwork
{
    // The Curator's Audit: a reason to read the cards.
    //
    // Twelve small exhibits stand on plinths about the museum, each with a brass card. Seven are
    // real, and what their cards say is true. Five are not exhibits at all: they are the Fray,
    // sitting very still and pretending, and the Fray have written their own cards, and the Fray
    // get their facts wrong. Professor Plume wants them found.
    //
    // Read the card. If you think the thing is a fake, hit it: once to make it rattle, and
    // again (within a few seconds) to say you're sure. A fake jumps up and has to be dealt with;
    // its card is then corrected. Hit a real one twice and the alarm goes, Patch loses a heart,
    // and it counts against you. (For anyone who doesn't know the answer, the fakes can't keep
    // perfectly still: they breathe, there's a loose grey thread at the foot of each, and now
    // and then, when they think nobody's close, they blink.)
    //
    // Every real card is true; every fake card is false, and the truth is told when it's caught.
    public static partial class Level2Builder
    {
        struct AuditEntry
        {
            public string Title, A, B, Truth;
            public bool Fake;
            public Vector3 At;
            public AuditEntry(string title, bool fake, float x, float z, string a, string b, string truth)
            {
                Title = title; Fake = fake; At = new Vector3(x, 0f, z); A = a; B = b; Truth = truth;
            }
        }

        static void Audit(LevelKit kit)
        {
            AuditEntry[] list =
            {
                new AuditEntry("The Sextant", false, -16f, -84f,
                    "With a sextant a sailor measures how high the Sun or a star stands above the horizon.",
                    "From that angle and the exact time, you can work out where on the Earth you are, with no land in sight.", null),
                new AuditEntry("The Ship's Compass", true, 16f, -84f,
                    "The needle of a compass always swings to point at the warmest part of the sea.",
                    "That is why a ship that follows its compass never meets an iceberg.",
                    "A compass needle points to the Earth's magnetic north, whatever the weather. It has never kept anyone clear of an iceberg."),
                new AuditEntry("The Octopus", false, -16f, -60f,
                    "An octopus has three hearts and blue blood.",
                    "Two of the hearts push blood through its gills. The third sends it round the rest of the body.", null),
                new AuditEntry("The Moon", true, 16f, -60f,
                    "The Moon comes much closer to the Earth every winter, and its cold light is what makes winter nights so chilly.",
                    "In summer it drifts away again, and the nights warm up.",
                    "Winter comes from the tilt of the Earth, not from the Moon, and moonlight has no chill in it at all."),
                new AuditEntry("Ship's Biscuit", false, -58f, -22f,
                    "Flour, water and a little salt, baked as hard as a roof tile so that it would keep for months at sea.",
                    "Sailors tapped each one on the table before eating it, to knock the weevils out.", null),
                new AuditEntry("The Jaws of a Shark", true, -42f, -50f,
                    "A shark is born with one set of teeth, which must last it for the whole of its life.",
                    "This is why sharks chew so very carefully.",
                    "A shark grows new teeth all its life, row behind row, and may get through thousands of them."),
                new AuditEntry("A Message in a Bottle", false, -74f, -74f,
                    "The oldest message in a bottle ever found was dropped from a German sailing ship in 1886, as part of an experiment to map the ocean's currents.",
                    "It was picked up on a beach in Western Australia in 2018, nearly 132 years later.", null),
                new AuditEntry("The Starfish", true, -70f, 6f,
                    "A starfish has five brains, one in each arm.",
                    "They take it in turns to decide which way to walk.",
                    "A starfish has no brain at all: only a ring of nerves round its mouth, with a branch running down each arm."),
                new AuditEntry("The Nautilus", false, -46f, 10f,
                    "Animals like the nautilus have lived in the sea for about five hundred million years: far longer than there have been dinosaurs, or even trees.",
                    "It rises and sinks by changing how much gas and water is in the chambers of its shell.", null),
                new AuditEntry("The Seahorse", true, -60f, 66f,
                    "It is the mother seahorse who carries the eggs, in a pouch on her front, until the babies are ready to swim away.",
                    "The father takes no interest.",
                    "It is the father seahorse who carries the eggs in his pouch, and he who gives birth to the young."),
                new AuditEntry("The Mariner's Astrolabe", false, 56f, 12f,
                    "A brass ring for measuring how high the Sun stands at noon, which tells a sailor how far north or south the ship is.",
                    "It was made heavy, with pieces cut out of it, so that it would hang steady in the wind on a rolling deck.", null),
                new AuditEntry("A Narwhal's Tusk", false, 90f, 14f,
                    "It is not a horn but a tooth: one long spiralled tooth that grows out through the narwhal's upper lip, as much as three metres long.",
                    "For hundreds of years they were sold as the horns of unicorns.", null),
            };

            var root = kit.Group("CuratorsAudit", Vector3.zero);
            var audit = root.gameObject.AddComponent<ExhibitAudit>();
            audit.Cases = new ExhibitCase[list.Length];
            var velvet = PatchworkAssets.Fabric(new Color(0.45f, 0.12f, 0.18f));
            for (int i = 0; i < list.Length; i++)
            {
                var e = list[i];
                Vector3 foot;
                if (!RmFloor("an audit exhibit", e.At, new Vector3(2.8f, 2.6f, 3f), 0f, 16f, out foot))
                {
                    // (A real one can simply be left out. A fake can't: the audit needs all five.)
                    if (!e.Fake) continue;
                    foot = e.At;
                }
                // Back to the nearest wall, card to the room.
                float yaw = FaceTheRoom(foot, (i % 4) * 90f);

                var g = kit.Group("AuditCase", foot);
                g.rotation = Rot(yaw);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.1f, 0f), new Vector3(1.2f, 0.2f, 1.2f), DarkWood, default(Vector3), g);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.6f, 0f), new Vector3(0.9f, 0.8f, 0.9f), DarkWood, default(Vector3), g);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.04f, 0f), new Vector3(1.14f, 0.1f, 1.14f), DarkWood, default(Vector3), g);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.11f, 0f), new Vector3(0.96f, 0.05f, 0.96f), velvet, default(Vector3), g);
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Cube, new Vector3(s * 0.46f, 0.6f, 0.46f), new Vector3(0.05f, 0.8f, 0.05f), Brass, default(Vector3), g);
                TownKit.Solid(kit, foot + Vector3.up * 0.55f, new Vector3(1.1f, 1.1f, 1.1f), yaw);

                var card = Plaque(kit, foot + Rot(yaw) * new Vector3(0f, 0f, 1.15f), yaw, e.Title, e.A, e.B);
                var c = g.gameObject.AddComponent<ExhibitCase>();
                c.Setup(audit, i, e.Title, e.Fake, e.Truth, card);
                audit.Cases[i] = c;   // (left empty for a real one that didn't fit)
            }
        }
    }

    // Keeps the score of the audit: how many of the five fakes have been unmasked, and how many
    // real exhibits have been wrongly walloped along the way.
    public class ExhibitAudit : MonoBehaviour
    {
        public static ExhibitAudit Instance { get; private set; }
        public static void ResetStatics() { Instance = null; }

        public ExhibitCase[] Cases;
        public const int Fakes = 5;

        public static string FakeKey(int i) { return "L2_q_fake" + i; }
        public static string TestedKey(int i) { return "L2_q_tested" + i; }
        public int Found { get { int n = 0; for (int i = 0; Cases != null && i < Cases.Length; i++) if (GameState.IsHintSeen(FakeKey(i))) n++; return n; } }
        public int Mistakes { get { int n = 0; for (int i = 0; Cases != null && i < Cases.Length; i++) if (GameState.IsHintSeen(TestedKey(i))) n++; return n; } }

        void Awake() { Instance = this; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Unmasked(int which, string truth)
        {
            GameState.SetHintSeen(FakeKey(which));
            int n = Found;
            var game = PatchworkGame.Instance;
            if (game != null)
            {
                game.ShowToast(n >= Fakes ? "That's all five fakes! Professor Plume will want to hear." : "A fake, unmasked!  (" + n + " of " + Fakes + ")");
                game.Heal(1);
            }
            Narrator.Say("The card was a lie. " + truth);
            if (n < Fakes || Level2Quest.Instance == null) return;
            Level2Quest.Instance.Secret("audit");
            if (Mistakes == 0) Level2Quest.Instance.Secret("connoisseur");
        }

        public void Mistake(int which)
        {
            GameState.SetHintSeen(TestedKey(which));
        }
    }

    // One plinth of the audit: the thing on it, its card, and whether it's what it says it is.
    public class ExhibitCase : MonoBehaviour
    {
        ExhibitAudit audit;
        int index;
        string title, truth;
        bool fake, settled;
        ExhibitPlaque card;
        Transform model, eyes, ribbon;
        float t, lastHit = -99f, rattle, blink = 5f;

        static readonly Vector3 Top = new Vector3(0f, 1.14f, 0f);

        public void Setup(ExhibitAudit audit, int index, string title, bool fake, string truth, ExhibitPlaque card)
        {
            this.audit = audit;
            this.index = index;
            this.title = title;
            this.fake = fake;
            this.truth = truth;
            this.card = card;
            t = index * 1.9f;
        }

        void Start()
        {
            if (fake && GameState.IsHintSeen(ExhibitAudit.FakeKey(index)))
            {
                // Caught on an earlier visit: an empty plinth, and an honest card.
                settled = true;
                Removed();
                return;
            }
            model = new GameObject("Exhibit").transform;
            model.SetParent(transform, false);
            model.localPosition = Top;
            BuildModel(index, model);
            if (fake)
            {
                // The tells: a loose grey thread at its foot, and eyes it mostly keeps shut.
                for (int i = 0; i < 2; i++)
                    Monsters.Part(PrimitiveType.Capsule, transform, new Vector3(0.36f + i * 0.05f, 1.2f + i * 0.03f, 0.3f), new Vector3(0.025f, 0.09f + i * 0.03f, 0.025f), Monsters.Grey, new Vector3(30f + i * 25f, 40f, 20f));
                eyes = new GameObject("Eyes").transform;
                eyes.SetParent(model, false);
                eyes.localPosition = new Vector3(0f, 0.22f, 0.3f);
                for (int s = -1; s <= 1; s += 2)
                    Monsters.Part(PrimitiveType.Sphere, eyes, new Vector3(s * 0.07f, 0f, 0f), new Vector3(0.05f, 0.06f, 0.03f), Monsters.Glow);
                eyes.gameObject.SetActive(false);
            }
            else if (GameState.IsHintSeen(ExhibitAudit.TestedKey(index))) Genuine();

            var fc = gameObject.AddComponent<FrayCreature>();
            fc.Radius = 0.8f;
            fc.Quiet = true;
            fc.Center = () => transform.position + Vector3.up * 1.3f;
            fc.Struck = (p, power, from, finisher) => Hit(p);
        }

        // Every exhibit is a few plain shapes. (Their order matches the list in Audit.)
        static void BuildModel(int kind, Transform m)
        {
            var brass = PatchworkAssets.Plain(new Color(0.85f, 0.65f, 0.3f), 0.85f);
            var dark = PatchworkAssets.Plain(new Color(0.12f, 0.1f, 0.12f), 0.5f);
            var bone = PatchworkAssets.Felt(new Color(0.92f, 0.89f, 0.8f));
            System.Func<PrimitiveType, Vector3, Vector3, Material, Vector3, Transform> P = (type, p, s, mat, e) => Monsters.Part(type, m, p, s, mat, e);
            Vector3 z = Vector3.zero;
            switch (kind)
            {
                case 0:   // Sextant: a wedge of brass with an arc, an arm and a little telescope
                    P(PrimitiveType.Cube, new Vector3(-0.12f, 0.28f, 0f), new Vector3(0.04f, 0.5f, 0.03f), brass, new Vector3(0f, 0f, 28f));
                    P(PrimitiveType.Cube, new Vector3(0.12f, 0.28f, 0f), new Vector3(0.04f, 0.5f, 0.03f), brass, new Vector3(0f, 0f, -28f));
                    for (int i = -3; i <= 3; i++) P(PrimitiveType.Cube, new Vector3(i * 0.075f, 0.06f + Mathf.Abs(i) * 0.012f, 0f), new Vector3(0.09f, 0.04f, 0.03f), brass, new Vector3(0f, 0f, i * -6f));
                    P(PrimitiveType.Cube, new Vector3(0.04f, 0.27f, 0.02f), new Vector3(0.03f, 0.46f, 0.02f), dark, new Vector3(0f, 0f, -8f));
                    P(PrimitiveType.Cylinder, new Vector3(-0.1f, 0.4f, 0.04f), new Vector3(0.05f, 0.12f, 0.05f), brass, new Vector3(0f, 0f, 90f));
                    P(PrimitiveType.Sphere, new Vector3(0f, 0.5f, 0f), Vector3.one * 0.07f, brass, z);
                    break;
                case 1:   // Compass: a brass bowl, a white card, a needle
                    P(PrimitiveType.Cylinder, new Vector3(0f, 0.08f, 0f), new Vector3(0.6f, 0.08f, 0.6f), brass, z);
                    P(PrimitiveType.Cylinder, new Vector3(0f, 0.165f, 0f), new Vector3(0.52f, 0.005f, 0.52f), bone, z);
                    P(PrimitiveType.Cube, new Vector3(0f, 0.18f, 0.1f), new Vector3(0.05f, 0.012f, 0.22f), PatchworkAssets.Felt(new Color(0.8f, 0.2f, 0.2f)), z);
                    P(PrimitiveType.Cube, new Vector3(0f, 0.18f, -0.1f), new Vector3(0.05f, 0.012f, 0.22f), dark, z);
                    P(PrimitiveType.Sphere, new Vector3(0f, 0.19f, 0f), Vector3.one * 0.05f, brass, z);
                    break;
                case 2:   // Octopus: a round head and eight arms
                    var oct = PatchworkAssets.Knit(new Color(0.85f, 0.45f, 0.4f));
                    P(PrimitiveType.Sphere, new Vector3(0f, 0.34f, -0.03f), new Vector3(0.4f, 0.46f, 0.42f), oct, z);
                    for (int i = 0; i < 8; i++)
                    {
                        float a = i * Mathf.PI / 4f;
                        P(PrimitiveType.Capsule, new Vector3(Mathf.Cos(a) * 0.24f, 0.08f, Mathf.Sin(a) * 0.24f), new Vector3(0.09f, 0.2f, 0.09f), oct, new Vector3(Mathf.Sin(a) * 75f, 0f, -Mathf.Cos(a) * 75f));
                    }
                    for (int s = -1; s <= 1; s += 2) P(PrimitiveType.Sphere, new Vector3(s * 0.1f, 0.3f, 0.17f), new Vector3(0.08f, 0.1f, 0.05f), bone, z);
                    break;
                case 3:   // The Moon: a grey globe with craters, on a brass foot
                    P(PrimitiveType.Cylinder, new Vector3(0f, 0.05f, 0f), new Vector3(0.3f, 0.05f, 0.3f), brass, z);
                    P(PrimitiveType.Sphere, new Vector3(0f, 0.38f, 0f), Vector3.one * 0.56f, PatchworkAssets.Felt(new Color(0.8f, 0.8f, 0.78f)), z);
                    for (int i = 0; i < 6; i++)
                    {
                        Vector3 d = (Quaternion.Euler(i * 71f, i * 113f, 0f) * Vector3.forward) * 0.27f;
                        P(PrimitiveType.Sphere, new Vector3(0f, 0.38f, 0f) + d, Vector3.one * (0.08f + (i % 3) * 0.03f), PatchworkAssets.Felt(new Color(0.62f, 0.62f, 0.62f)), z);
                    }
                    break;
                case 4:   // Ship's biscuit: a pale round with holes pricked in it
                    P(PrimitiveType.Cylinder, new Vector3(0f, 0.05f, 0f), new Vector3(0.5f, 0.04f, 0.5f), PatchworkAssets.Felt(new Color(0.86f, 0.74f, 0.52f)), z);
                    for (int i = 0; i < 7; i++)
                    {
                        float a = i * Mathf.PI * 2f / 6f;
                        float r = i == 6 ? 0f : 0.15f;
                        P(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * r, 0.09f, Mathf.Sin(a) * r), new Vector3(0.04f, 0.01f, 0.04f), PatchworkAssets.Felt(new Color(0.6f, 0.48f, 0.32f)), z);
                    }
                    break;
                case 5:   // Shark jaws: a hoop of bone with teeth all round the inside
                    for (int i = 0; i < 14; i++)
                    {
                        float a = i * Mathf.PI * 2f / 14f;
                        P(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.3f, 0.36f + Mathf.Sin(a) * 0.3f, 0f), new Vector3(0.14f, 0.14f, 0.1f), bone, z);
                        P(PrimitiveType.Cube, new Vector3(Mathf.Cos(a) * 0.22f, 0.36f + Mathf.Sin(a) * 0.22f, 0.02f), new Vector3(0.05f, 0.09f, 0.02f), PatchworkAssets.Felt(Color.white), new Vector3(0f, 0f, a * Mathf.Rad2Deg + 90f));
                    }
                    break;
                case 6:   // A bottle on its side, with a roll of paper in it
                    P(PrimitiveType.Capsule, new Vector3(0f, 0.14f, 0f), new Vector3(0.24f, 0.3f, 0.24f), PatchworkAssets.Bubble(new Color(0.5f, 0.8f, 0.65f, 0.45f)), new Vector3(0f, 0f, 90f));
                    P(PrimitiveType.Cylinder, new Vector3(0.36f, 0.14f, 0f), new Vector3(0.1f, 0.1f, 0.1f), PatchworkAssets.Bubble(new Color(0.5f, 0.8f, 0.65f, 0.45f)), new Vector3(0f, 0f, 90f));
                    P(PrimitiveType.Cylinder, new Vector3(0.48f, 0.14f, 0f), new Vector3(0.09f, 0.04f, 0.09f), PatchworkAssets.Felt(new Color(0.6f, 0.45f, 0.3f)), new Vector3(0f, 0f, 90f));
                    P(PrimitiveType.Cylinder, new Vector3(-0.02f, 0.13f, 0f), new Vector3(0.09f, 0.16f, 0.09f), bone, new Vector3(0f, 0f, 90f));
                    break;
                case 7:   // Starfish: five fat arms
                    var star = PatchworkAssets.Knit(new Color(0.95f, 0.55f, 0.3f));
                    P(PrimitiveType.Sphere, new Vector3(0f, 0.06f, 0f), new Vector3(0.26f, 0.1f, 0.26f), star, z);
                    for (int i = 0; i < 5; i++)
                    {
                        float a = i * Mathf.PI * 2f / 5f;
                        P(PrimitiveType.Capsule, new Vector3(Mathf.Sin(a) * 0.2f, 0.05f, Mathf.Cos(a) * 0.2f), new Vector3(0.12f, 0.2f, 0.07f), star, new Vector3(90f, a * Mathf.Rad2Deg, 0f));
                    }
                    break;
                case 8:   // Nautilus: a coiled shell, striped, with a dark opening
                    P(PrimitiveType.Sphere, new Vector3(0f, 0.28f, 0f), new Vector3(0.2f, 0.54f, 0.54f), bone, z);
                    for (int i = 0; i < 7; i++)
                    {
                        float a = i * 0.5f - 0.6f;
                        P(PrimitiveType.Cube, new Vector3(0f, 0.28f + Mathf.Sin(a) * 0.2f, -Mathf.Cos(a) * 0.2f), new Vector3(0.21f, 0.2f, 0.04f), PatchworkAssets.Felt(new Color(0.7f, 0.35f, 0.25f)), new Vector3(-a * Mathf.Rad2Deg, 0f, 0f));
                    }
                    P(PrimitiveType.Sphere, new Vector3(0f, 0.14f, 0.24f), new Vector3(0.16f, 0.2f, 0.16f), dark, z);
                    break;
                case 9:   // Seahorse: a curled tail, a round belly, a long snout
                    var sea = PatchworkAssets.Knit(new Color(0.95f, 0.8f, 0.35f));
                    P(PrimitiveType.Capsule, new Vector3(0f, 0.34f, 0f), new Vector3(0.18f, 0.2f, 0.2f), sea, new Vector3(10f, 0f, 0f));
                    P(PrimitiveType.Sphere, new Vector3(0f, 0.6f, 0.04f), Vector3.one * 0.18f, sea, z);
                    P(PrimitiveType.Capsule, new Vector3(0f, 0.57f, 0.17f), new Vector3(0.06f, 0.1f, 0.06f), sea, new Vector3(80f, 0f, 0f));
                    for (int i = 0; i < 5; i++)
                    {
                        float a = i * 0.9f;
                        P(PrimitiveType.Sphere, new Vector3(0f, 0.12f + Mathf.Cos(a) * 0.08f, 0.02f + Mathf.Sin(a) * 0.08f - i * 0.005f), Vector3.one * (0.11f - i * 0.012f), sea, z);
                    }
                    P(PrimitiveType.Cube, new Vector3(0f, 0.4f, -0.13f), new Vector3(0.02f, 0.16f, 0.1f), PatchworkAssets.Felt(new Color(0.95f, 0.6f, 0.3f)), z);
                    break;
                case 10:  // Astrolabe: a heavy ring with a cross-bar, hung from a loop
                    for (int i = 0; i < 16; i++)
                    {
                        float a = i * Mathf.PI / 8f;
                        P(PrimitiveType.Cube, new Vector3(Mathf.Cos(a) * 0.26f, 0.34f + Mathf.Sin(a) * 0.26f, 0f), new Vector3(0.12f, 0.07f, 0.05f), brass, new Vector3(0f, 0f, a * Mathf.Rad2Deg + 90f));
                    }
                    P(PrimitiveType.Cube, new Vector3(0f, 0.34f, 0f), new Vector3(0.5f, 0.05f, 0.04f), brass, z);
                    P(PrimitiveType.Cube, new Vector3(0f, 0.34f, 0f), new Vector3(0.05f, 0.5f, 0.04f), brass, z);
                    P(PrimitiveType.Cube, new Vector3(0f, 0.34f, 0.03f), new Vector3(0.04f, 0.46f, 0.02f), dark, new Vector3(0f, 0f, 35f));
                    P(PrimitiveType.Sphere, new Vector3(0f, 0.66f, 0f), Vector3.one * 0.08f, brass, z);
                    break;
                default:  // Narwhal tusk: long, thin, spiralled, on two little rests
                    for (int i = 0; i < 12; i++)
                        P(PrimitiveType.Sphere, new Vector3(-0.44f + i * 0.08f, 0.2f + Mathf.Sin(i * 1.1f) * 0.008f, Mathf.Cos(i * 1.1f) * 0.008f), new Vector3(0.12f, 0.1f - i * 0.006f, 0.1f - i * 0.006f), bone, new Vector3(i * 40f, 0f, 0f));
                    for (int s = -1; s <= 1; s += 2) P(PrimitiveType.Cube, new Vector3(s * 0.3f, 0.07f, 0f), new Vector3(0.05f, 0.14f, 0.16f), dark, z);
                    break;
            }
        }

        // A little card on the empty plinth, where a fake used to be.
        void Removed()
        {
            Monsters.Part(PrimitiveType.Cube, transform, Top + new Vector3(0f, 0.09f, 0f), new Vector3(0.4f, 0.18f, 0.02f), PatchworkAssets.Felt(new Color(0.95f, 0.92f, 0.85f)), new Vector3(-12f, 0f, 0f));
            Monsters.Part(PrimitiveType.Cube, transform, Top + new Vector3(0f, 0.1f, 0.012f), new Vector3(0.28f, 0.03f, 0.005f), PatchworkAssets.Felt(new Color(0.75f, 0.2f, 0.2f)), new Vector3(-12f, 0f, 0f));
            if (card != null && truth != null) card.Text = new[] { "(This card was written by the Fray, and was not true. It has been corrected.)", truth };
        }

        // A green rosette on a real one that's been tested the hard way.
        void Genuine()
        {
            settled = true;
            if (ribbon != null) return;
            ribbon = new GameObject("Genuine").transform;
            ribbon.SetParent(transform, false);
            ribbon.localPosition = new Vector3(-0.4f, 0.85f, 0.47f);
            var green = PatchworkAssets.Felt(new Color(0.3f, 0.7f, 0.45f));
            Monsters.Part(PrimitiveType.Cylinder, ribbon, Vector3.zero, new Vector3(0.2f, 0.012f, 0.2f), green, new Vector3(90f, 0f, 0f));
            Monsters.Part(PrimitiveType.Sphere, ribbon, new Vector3(0f, 0f, 0.015f), new Vector3(0.09f, 0.09f, 0.02f), PatchworkAssets.Glow(new Color(1f, 0.9f, 0.5f), 1f));
            Monsters.Part(PrimitiveType.Cube, ribbon, new Vector3(-0.04f, -0.15f, 0f), new Vector3(0.05f, 0.16f, 0.01f), green, new Vector3(0f, 0f, -10f));
            Monsters.Part(PrimitiveType.Cube, ribbon, new Vector3(0.04f, -0.15f, 0f), new Vector3(0.05f, 0.16f, 0.01f), green, new Vector3(0f, 0f, 10f));
        }

        bool Hit(PatchworkPlayer player)
        {
            if (settled || model == null) return false;
            var game = PatchworkGame.Instance;
            rattle = 1f;
            Sfx.PlayNear(SfxId.Clack, transform.position, 0.4f, 20f);
            if (Time.time - lastHit > 4f)
            {
                // The first knock only asks the question.
                lastHit = Time.time;
                if (game != null) game.ShowToast(title + ": read the card. If you're sure it's a FAKE, hit it again.");
                return true;
            }
            lastHit = -99f;
            if (!fake)
            {
                // Oh dear.
                Genuine();
                audit.Mistake(index);
                Sfx.Play(SfxId.TransformFail, 0.6f);
                if (game != null)
                {
                    game.ShowToast(title + " is REAL, and every word on its card is true. The alarm goes off!");
                    game.Hurt(transform.position + Vector3.up);
                }
                return true;
            }
            // Got you.
            settled = true;
            if (eyes != null) eyes.gameObject.SetActive(true);
            var mimic = Mimic.Create(transform.parent, transform.position + transform.rotation * Top, model);
            string said = truth;
            int which = index;
            var book = audit;
            mimic.Beaten = f => book.Unmasked(which, said);
            model = null;
            Removed();
            FxBurst.Spawn(FxBurst.Kind.Threads, transform.position + Vector3.up * 1.4f, SnarlKnot.FrayGrey);
            Sfx.PlayPitched(SfxId.Squeak, 0.6f, 0.7f);
            if (game != null) game.ShowToast("It's a FAKE! " + title + " has grown legs.");
            return true;
        }

        void Update()
        {
            if (model == null) return;
            float dt = Time.deltaTime;
            t += dt;
            rattle = Mathf.MoveTowards(rattle, 0f, dt * 2.5f);
            float shake = Mathf.Sin(rattle * 40f) * 6f * rattle;
            model.localRotation = Quaternion.Euler(0f, 0f, shake);
            if (!fake || settled) return;
            // It can't keep perfectly still.
            model.localScale = Vector3.one * (1f + Mathf.Sin(t * 1.8f) * 0.018f);
            blink -= dt;
            if (blink <= 0f)
            {
                var game = PatchworkGame.Instance;
                bool show = !eyes.gameObject.activeSelf;
                if (show && game != null && game.Player != null)
                {
                    float d = (game.Player.transform.position - transform.position).magnitude;
                    show = d > 3.5f && d < 12f;   // (never while Patch is right in front of it)
                }
                eyes.gameObject.SetActive(show);
                blink = show ? 0.35f : Random.Range(4f, 8f);
            }
        }
    }

    // A fake exhibit, found out: the thing itself, up on four grey legs, hopping mad.
    public class Mimic : MuseumFoe
    {
        Vector3 home, hopFrom, hopTo;
        float hop = 0f, rest, squash;
        Transform body;

        public static Mimic Create(Transform parent, Vector3 plinthTop, Transform exhibit)
        {
            var go = new GameObject("Mimic");
            go.transform.SetParent(parent, false);
            go.transform.position = plinthTop;
            var m = go.AddComponent<Mimic>();
            m.pos = plinthTop;
            m.MaxHealth = 2;
            m.Score = 400;
            m.centreY = 0.55f;
            m.rig = Pivot(go.transform, "Rig", Vector3.zero);
            m.body = Pivot(m.rig, "Body", Vector3.zero);
            P(PrimitiveType.Sphere, m.body, new Vector3(0f, 0.36f, 0f), new Vector3(0.7f, 0.5f, 0.7f), Monsters.Grey);
            for (int i = 0; i < 4; i++)
                P(PrimitiveType.Capsule, m.body, new Vector3(i % 2 == 0 ? -0.24f : 0.24f, 0.12f, i < 2 ? -0.2f : 0.2f), new Vector3(0.1f, 0.14f, 0.1f), Monsters.Dark);
            Tuft(m.body, new Vector3(0.2f, 0.5f, -0.2f), 2.2f, 1.7f);
            Tuft(m.body, new Vector3(-0.22f, 0.46f, 0.1f), 1.8f, 4.2f);
            // What it was pretending to be, still on its back.
            exhibit.SetParent(m.body, true);
            exhibit.localPosition = new Vector3(0f, 0.58f, 0f);
            exhibit.localRotation = Quaternion.identity;
            exhibit.localScale = Vector3.one;
            // First it jumps down off the plinth, away from whoever hit it.
            Vector3 away = Vector3.forward;
            var player = ThePlayer();
            if (player != null)
            {
                away = plinthTop - player.transform.position;
                away.y = 0f;
                away = away.sqrMagnitude > 0.01f ? away.normalized : Vector3.forward;
            }
            Vector3 land = plinthTop + away * 2f;
            land.y = Monsters.Ground(new Vector3(land.x, plinthTop.y - 0.6f, land.z));
            if (Physics.CheckSphere(land + Vector3.up * 0.8f, 0.4f, ~(1 << 2), QueryTriggerInteraction.Ignore))
            {
                land = plinthTop - away * 2f;
                land.y = Monsters.Ground(new Vector3(land.x, plinthTop.y - 0.6f, land.z));
            }
            m.home = land;
            m.hopFrom = plinthTop;
            m.hopTo = land;
            m.Register(0.6f);
            return m;
        }

        void Update()
        {
            if (IsBeaten) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            var player = ThePlayer();
            bool free = Common(dt);
            squash = Mathf.MoveTowards(squash, 0f, dt * 4f);
            if (hop >= 0f)
            {
                hop += dt / 0.45f;
                float k = Mathf.Clamp01(hop);
                pos = Vector3.Lerp(hopFrom, hopTo, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.1f;
                if (k >= 1f) { hop = -1f; rest = Random.Range(0.25f, 0.55f); squash = 1f; Sfx.PlayNear(SfxId.Clack, pos, 0.2f, 16f); }
            }
            else if (free)
            {
                rest -= dt;
                if (rest <= 0f)
                {
                    Vector3 target = home + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
                    if (player != null)
                    {
                        Vector3 pp = player.transform.position;
                        if ((pp - pos).sqrMagnitude < 144f && Mathf.Abs(pp.y - pos.y) < 2.5f)
                            target = pos + Vector3.ClampMagnitude(new Vector3(pp.x - pos.x, 0f, pp.z - pos.z), 2.8f);
                    }
                    target.y = Monsters.Ground(target + Vector3.up * 0.3f);
                    if (Mathf.Abs(target.y - pos.y) > 1f || (target - home).magnitude > 14f || Physics.CheckSphere(target + Vector3.up * 0.8f, 0.4f, ~(1 << 2), QueryTriggerInteraction.Ignore)) target = pos;
                    hopFrom = pos;
                    hopTo = target;
                    hop = 0f;
                }
            }
            transform.position = pos;
            if (player != null)
            {
                Vector3 f = player.transform.position - pos;
                f.y = 0f;
                if (f.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(f.normalized, Vector3.up);
            }
            float sq = Mathf.Sin(squash * Mathf.PI) * 0.3f;
            body.localScale = new Vector3(1f + sq, 1f - sq, 1f + sq);
            Contact(player, 0.9f, 1.2f, true);
        }
    }
}

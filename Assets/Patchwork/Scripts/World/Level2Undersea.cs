using UnityEngine;

namespace Patchwork
{
    // Under the water: things for Water Form to find at the bottom of the two pools.
    //   The kelp pool: the Sea Gate (a drowned stone arch to swim through), a garden of
    //     glowing anemones, starfish and urchins, ripples in the sand, an upturned rowing boat,
    //     and a giant clam that slowly opens and closes over a secret marble.
    //   The Ruins: a drowned colonnade along the east wall, roofed over just under the
    //     surface, so the only way in is to swim. Crystals light the way to a treasure chest
    //     at the far end, which opens when you swim up to it. Also a fallen sphinx, and a
    //     mosaic on the floor of the pool.
    public static partial class Level2Builder
    {
        public const string ClamMarble = "L2_marble_clam", ChestMarble = "L2_marble_treasure";

        static void Undersea(LevelKit kit)
        {
            KelpPoolFloor(kit);
            DrownedColonnade(kit);
            RuinPoolFloor(kit);
        }

        // ================================================================ The kelp pool

        static void KelpPoolFloor(LevelKit kit)
        {
            float bed = PoolBed;
            var rock = PatchworkAssets.Cobble(new Color(0.42f, 0.5f, 0.48f));
            var moss = PatchworkAssets.Felt(new Color(0.3f, 0.55f, 0.45f));

            // ----- The Sea Gate: two drowned pillars and a lintel -----
            Vector3 gate = new Vector3(-72f, bed, 27f);
            foreach (float dz in new[] { -2.6f, 2.6f })
            {
                Vector3 f = gate + new Vector3(0f, 0f, dz);
                kit.Deco(PrimitiveType.Cylinder, f + Vector3.up * 1.7f, new Vector3(1.1f, 1.7f, 1.1f), rock);
                kit.Deco(PrimitiveType.Cylinder, f + Vector3.up * 0.15f, new Vector3(1.5f, 0.15f, 1.5f), rock);
                NoShadow(kit.Deco(PrimitiveType.Sphere, f + new Vector3(0.4f, 2.6f, 0.2f), new Vector3(0.9f, 0.5f, 0.9f), moss));
                TownKit.Solid(kit, f + Vector3.up * 1.7f, new Vector3(1.1f, 3.4f, 1.1f), 0f);
            }
            kit.Block(gate + Vector3.up * 3.7f, new Vector3(1.6f, 0.6f, 7.2f), StoneCol, "SeaGate", rock);
            // Runes along the lintel, glowing faintly.
            var rune = new MeshBatch();
            for (int i = 0; i < 7; i++) rune.Box(gate + new Vector3(-0.82f, 3.7f, -2.7f + i * 0.9f), new Vector3(0.03f, 0.3f, 0.12f), Quaternion.Euler(i * 23f % 45f, 0f, 0f));
            Twinkling(rune, kit.Root, "SeaGateRunes", new Color(0.5f, 1f, 0.9f), 0.8f, 2.4f, 0.7f, 0.3f);
            // Bubbles to swim through the arch for.
            Trail(kit, 10, gate + new Vector3(6f, 1.4f, 0f), gate + new Vector3(3f, 1.6f, 0f), gate + new Vector3(0f, 1.8f, 0f), gate + new Vector3(-3f, 1.6f, 0f), gate + new Vector3(-6f, 1.4f, 0f));

            // ----- The anemone garden, starfish, urchins and sand ripples -----
            var rng = new System.Random(3131);
            var glow = new MeshBatch();
            var stars = new MeshBatch();
            var urchins = new MeshBatch();
            var ripples = new MeshBatch();
            for (int i = 0; i < 26; i++)
            {
                Vector3 p = new Vector3(-84f + (float)rng.NextDouble() * 48f, bed, 20f + (float)rng.NextDouble() * 36f);
                if (Vector3.Distance(p, gate) < 4.5f) continue;
                switch (i % 3)
                {
                    case 0:
                        // An anemone: a crown of little glowing tentacles.
                        for (int t = 0; t < 7; t++)
                        {
                            float a = t / 7f * Mathf.PI * 2f;
                            glow.Box(p + new Vector3(Mathf.Cos(a) * 0.18f, 0.3f, Mathf.Sin(a) * 0.18f), new Vector3(0.07f, 0.5f, 0.07f), Quaternion.Euler(Mathf.Sin(a) * 25f, 0f, -Mathf.Cos(a) * 25f));
                        }
                        glow.Ball(p + Vector3.up * 0.08f, new Vector3(0.45f, 0.18f, 0.45f), 2, 6);
                        break;
                    case 1:
                        for (int arm = 0; arm < 5; arm++)
                            stars.Box(p + Rot(arm * 72f + i * 11f) * new Vector3(0f, 0.04f, 0.18f), new Vector3(0.12f, 0.06f, 0.36f), Rot(arm * 72f + i * 11f));
                        break;
                    default:
                        urchins.Ball(p + Vector3.up * 0.18f, Vector3.one * 0.36f, 3, 6);
                        for (int sp = 0; sp < 8; sp++)
                            urchins.Box(p + Vector3.up * 0.18f, new Vector3(0.03f, 0.03f, 0.7f), Quaternion.Euler(sp * 41f, sp * 67f, 0f));
                        break;
                }
            }
            for (int r = 0; r < 14; r++)
            {
                float z = 21f + r * 2.6f;
                ripples.Box(new Vector3(-50f + (r % 3) * 2f, bed + 0.03f, z), new Vector3(13f, 0.05f, 0.35f), Rot(8f + (r % 4) * 3f));
            }
            Twinkling(glow, kit.Root, "Anemones", new Color(1f, 0.5f, 0.75f), 0.9f, 2.2f, 0.9f, 1.1f);
            stars.Build(kit.Root, "Starfish", PatchworkAssets.Felt(new Color(1f, 0.55f, 0.35f)));
            urchins.Build(kit.Root, "Urchins", PatchworkAssets.Felt(new Color(0.35f, 0.2f, 0.45f)));
            ripples.Build(kit.Root, "SandRipples", PatchworkAssets.Felt(new Color(0.45f, 0.5f, 0.44f)));

            // ----- An upturned rowing boat, with a lantern still lit underneath -----
            Vector3 boat = new Vector3(-44f, bed, 46f);
            var hull = kit.Group("SunkenBoat", boat);
            hull.rotation = Rot(30f);
            var planks = DarkWood;
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.55f, 0f), new Vector3(1.9f, 1.1f, 4.6f), planks, default(Vector3), hull));
            for (int i = -2; i <= 2; i++)
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.05f, i * 0.8f), new Vector3(1.6f, 0.08f, 0.12f), Brass, default(Vector3), hull));
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(1.2f, 0.25f, 0.6f), Vector3.one * 0.35f, PatchworkAssets.Glow(new Color(1f, 0.8f, 0.4f), 2.4f), default(Vector3), hull));
            PointLight(kit, boat + new Vector3(1.2f, 0.6f, 0.6f), new Color(1f, 0.8f, 0.45f), 6f, 1.2f);
            TownKit.Solid(kit, boat + Vector3.up * 0.55f, new Vector3(1.8f, 1.1f, 4.4f), 30f);

            // ----- The giant clam, and the pearl it keeps -----
            Vector3 clamAt = new Vector3(-80f, bed, 52f);
            var clam = kit.Group("GiantClam", clamAt);
            clam.rotation = Rot(120f);
            var shell = PatchworkAssets.Felt(new Color(0.75f, 0.7f, 0.85f));
            var lip = PatchworkAssets.Felt(new Color(0.35f, 0.6f, 0.8f));
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.35f, 0f), new Vector3(3f, 0.9f, 2.2f), shell, default(Vector3), clam));
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.6f, 0f), new Vector3(2.7f, 0.25f, 1.9f), lip, default(Vector3), clam));
            for (int i = -3; i <= 3; i++)
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(i * 0.4f, 0.4f, 1.02f), new Vector3(0.14f, 0.6f, 0.1f), shell, default(Vector3), clam));
            var hinge = new GameObject("Lid").transform;
            hinge.SetParent(clam, false);
            hinge.localPosition = new Vector3(0f, 0.75f, -1.05f);
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.15f, 1.05f), new Vector3(3f, 0.7f, 2.2f), shell, default(Vector3), hinge));
            for (int i = -3; i <= 3; i++)
                NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(i * 0.4f, 0.25f, 2.08f), new Vector3(0.14f, 0.5f, 0.1f), shell, default(Vector3), hinge));
            hinge.gameObject.AddComponent<ClamBreathe>();
            TownKit.Solid(kit, clamAt + Vector3.up * 0.3f, new Vector3(2.6f, 0.6f, 2.6f), 0f);
            SecretMarble.Place(kit, clamAt + Vector3.up * 1.3f, ClamMarble, new Color(1f, 0.85f, 0.95f));
            PointLight(kit, clamAt + Vector3.up * 1.6f, new Color(1f, 0.85f, 1f), 7f, 1.3f);

            kit.Hint(new Vector3(-60f, -0.4f, 38f), new Vector3(54f, 2.4f, 38f), "Diving",
                "In Water Form, hold {Ability} to dive. Let go to float back up, or press {Jump} to kick for the surface. Swim through the old Sea Gate, and keep an eye on the giant clam: it doesn't stay shut for long.", HintTheme.Water);
            // Specks drifting in the water, catching the light.
            Motes(kit, new Vector3(-60f, -3.2f, 38f), new Vector3(54f, 4.6f, 38f), 40, new Color(0.7f, 1f, 0.9f), 1.6f, 0.06f, new Vector3(0.04f, 0.03f, 0.05f));
        }

        // ================================================================ The drowned colonnade

        static void DrownedColonnade(LevelKit kit)
        {
            float bed = RuinBed, roof = -1.5f;
            const float x0 = 93.5f, x1 = 100f, z0 = 72f, z1 = 100f;
            float h = roof - bed;
            var stone = PatchworkAssets.Cobble(new Color(0.38f, 0.46f, 0.42f));
            // The inner wall, lined with half-columns, and the roof just under the surface.
            kit.Block(new Vector3(x0, bed + h * 0.5f, (z0 + z1) * 0.5f), new Vector3(1f, h, z1 - z0), StoneCol, "ColonnadeWall", stone);
            kit.Block(new Vector3((x0 + x1) * 0.5f, roof + 0.2f, (z0 + z1) * 0.5f), new Vector3(x1 - x0 + 1f, 0.4f, z1 - z0), StoneCol, "ColonnadeRoof", stone);
            for (float z = z0 + 1.5f; z < z1; z += 3.5f)
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(x0 - 0.5f, bed + h * 0.5f, z), new Vector3(0.9f, h * 0.5f, 0.9f), RuinStone);
                kit.Deco(PrimitiveType.Cube, new Vector3(x0 - 0.5f, roof - 0.15f, z), new Vector3(1.3f, 0.3f, 1.3f), RuinStone);
            }
            // The way in: a pair of columns and an arch stone at the south end.
            foreach (float x in new[] { x0 + 0.5f, x1 - 0.4f })
                kit.Deco(PrimitiveType.Cylinder, new Vector3(x, bed + h * 0.5f, z0 - 0.4f), new Vector3(0.8f, h * 0.5f, 0.8f), RuinStone);
            kit.Deco(PrimitiveType.Cube, new Vector3((x0 + x1) * 0.5f, roof - 0.2f, z0 - 0.4f), new Vector3(x1 - x0 + 0.6f, 0.5f, 1f), RuinStone);
            // Crystals glowing along the inside, blue-green, then gold near the treasure.
            // (One batch per wall: a single batch across both walls has a bounding box filling the
            // whole passage, which made the marble think the passage was blocked.)
            var crystals = new[] { new MeshBatch(), new MeshBatch() };
            var gold = new[] { new MeshBatch(), new MeshBatch() };
            var rng = new System.Random(919);
            for (float z = z0 + 2f; z < z1 - 1f; z += 2.2f)
            {
                bool near = z > z1 - 8f;
                int si = rng.Next(0, 2);
                var mb = near ? gold[si] : crystals[si];
                float side = si == 0 ? x0 + 0.6f : x1 - 0.3f;
                float y = bed + 0.2f + (float)rng.NextDouble() * 2.2f;
                for (int c = 0; c < 3; c++)
                    mb.Box(new Vector3(side, y + c * 0.12f, z + c * 0.25f), new Vector3(0.16f, 0.55f - c * 0.12f, 0.16f), Quaternion.Euler((float)rng.NextDouble() * 40f - 20f, 0f, (float)rng.NextDouble() * 40f - 20f));
            }
            for (int s = 0; s < 2; s++)
            {
                Twinkling(crystals[s], kit.Root, "ColonnadeCrystals", new Color(0.4f, 1f, 0.9f), 1.2f, 2.8f, 0.8f, 0.2f + s);
                Twinkling(gold[s], kit.Root, "ColonnadeGold", new Color(1f, 0.82f, 0.4f), 1.4f, 3f, 1.1f, 0.6f + s);
            }
            PointLight(kit, new Vector3(96.8f, bed + 2.4f, 80f), new Color(0.4f, 1f, 0.85f), 10f, 1.2f);
            PointLight(kit, new Vector3(96.8f, bed + 2.4f, 90f), new Color(0.4f, 1f, 0.85f), 10f, 1.1f);
            PointLight(kit, new Vector3(96.8f, bed + 2.2f, 97f), new Color(1f, 0.8f, 0.45f), 8f, 1.5f);
            // A row of bubbles leading in.
            for (int i = 0; i < 8; i++)
            {
                float z = z0 + 2f + i * 2.6f;
                Trail(kit, 10, new Vector3(96.8f, bed + 1.4f + Mathf.Sin(i * 0.9f) * 0.4f, z));
            }

            // ----- The treasure chest at the far end -----
            Vector3 at = new Vector3(96.8f, bed, 97.6f);
            var chest = kit.Group("TreasureChest", at);
            var wood = PatchworkAssets.Planks(new Color(0.42f, 0.27f, 0.18f));
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.45f, 0f), new Vector3(1.8f, 0.9f, 1.2f), wood, default(Vector3), chest);
            foreach (float x in new[] { -0.7f, 0.7f })
                kit.Deco(PrimitiveType.Cube, new Vector3(x, 0.45f, 0f), new Vector3(0.14f, 0.94f, 1.24f), Brass, default(Vector3), chest);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.86f, 0f), new Vector3(1.6f, 0.06f, 1f), PatchworkAssets.Glow(new Color(1f, 0.8f, 0.35f), 2.2f), default(Vector3), chest));
            var lid = new GameObject("Lid").transform;
            lid.SetParent(chest, false);
            lid.localPosition = new Vector3(0f, 0.9f, 0.6f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, -0.6f), new Vector3(1.2f, 0.9f, 1.2f), wood, new Vector3(0f, 0f, 90f), lid);
            NoShadow(kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.05f, -1.22f), new Vector3(0.3f, 0.35f, 0.08f), Brass, default(Vector3), lid));
            TownKit.Solid(kit, at + Vector3.up * 0.45f, new Vector3(1.8f, 0.9f, 1.2f), 0f);
            // (Just in front of the chest, clear of it and of the roof, so it stays where it's put.)
            var marble = SecretMarble.Place(kit, at + new Vector3(0f, 1.7f, -2.6f), ChestMarble, new Color(1f, 0.8f, 0.3f));
            var tc = kit.Volume<TreasureChest>(at + new Vector3(0f, 1.2f, -1.5f), new Vector3(6f, 3.4f, 5f), "TreasureChestOpener");
            tc.Lid = lid;
            tc.Prize = marble != null ? marble.gameObject : null;
            tc.MarbleId = ChestMarble;
            kit.Hint(new Vector3(96.8f, -3f, 70f), new Vector3(7f, 4f, 4f), "The Drowned Colonnade",
                "Something glitters at the far end. Swim in with Water Form and see.", HintTheme.Water);
        }

        // ================================================================ The Ruins' pool floor

        static void RuinPoolFloor(LevelKit kit)
        {
            float bed = RuinBed;
            kit.Hint(new Vector3(70f, -0.4f, 40f), new Vector3(50f, 2.4f, 30f), "Diving in the Ruins",
                "Hold {Ability} in Water Form to dive. There's a whole drowned city down there, and a colonnade along the east wall with a roof on it...", HintTheme.Water);
            Motes(kit, new Vector3(72f, -2.8f, 62f), new Vector3(54f, 4f, 74f), 50, new Color(0.75f, 1f, 0.85f), 1.6f, 0.06f, new Vector3(0.05f, 0.03f, 0.04f));
            // A mosaic of the sun on the floor of the pool: rings of coloured felt and gold rays.
            Vector3 m = new Vector3(76f, bed + 0.03f, 76f);
            Color[] rings = { new Color(0.2f, 0.45f, 0.55f), new Color(0.85f, 0.65f, 0.35f), new Color(0.15f, 0.3f, 0.45f), new Color(0.9f, 0.8f, 0.5f) };
            for (int i = 0; i < rings.Length; i++)
                NoShadow(kit.Deco(PrimitiveType.Cylinder, m + Vector3.up * (i * 0.012f), new Vector3(10f - i * 2.2f, 0.012f, 10f - i * 2.2f), PatchworkAssets.Felt(rings[i])));
            var rays = new MeshBatch();
            for (int i = 0; i < 12; i++)
                rays.Box(m + Vector3.up * 0.06f + Rot(i * 30f) * new Vector3(0f, 0f, 3.4f), new Vector3(0.35f, 0.03f, 1.6f), Rot(i * 30f));
            Twinkling(rays, kit.Root, "SunMosaic", new Color(1f, 0.8f, 0.4f), 0.7f, 1.8f, 0.5f, 0.8f);

            // A fallen sphinx, lying on its side in the weed.
            Vector3 s = new Vector3(84f, bed, 84f);
            var sph = kit.Group("Sphinx", s);
            sph.rotation = Rot(200f);
            var sand = RuinStone;
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.8f, 0f), new Vector3(2.2f, 1.6f, 4.6f), sand, new Vector3(0f, 0f, 12f), sph);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.9f, 2.4f), new Vector3(1.4f, 1.6f, 1.3f), sand, new Vector3(0f, 0f, 12f), sph);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 2f, 2.4f), new Vector3(2.2f, 1.5f, 0.6f), PatchworkAssets.Felt(new Color(0.3f, 0.45f, 0.6f)), new Vector3(0f, 0f, 12f), sph);
            foreach (float x in new[] { -0.6f, 0.6f })
                kit.Deco(PrimitiveType.Cube, new Vector3(x, 0.3f, 3.2f), new Vector3(0.5f, 0.5f, 1.8f), sand, default(Vector3), sph);
            NoShadow(kit.Deco(PrimitiveType.Sphere, new Vector3(0.9f, 1.6f, -0.4f), new Vector3(1.2f, 0.4f, 2f), PatchworkAssets.Felt(new Color(0.3f, 0.5f, 0.35f)), default(Vector3), sph));
            TownKit.Solid(kit, s + Vector3.up * 0.9f, new Vector3(2.4f, 1.8f, 5f), 200f);

            // Scattered coins and broken pots in the silt.
            var coins = new MeshBatch();
            var pots = new MeshBatch();
            var rng = new System.Random(4646);
            for (int i = 0; i < 40; i++)
            {
                Vector3 p = new Vector3(62f + (float)rng.NextDouble() * 30f, bed + 0.04f, 64f + (float)rng.NextDouble() * 30f);
                if (Vector3.Distance(p, m) < 5.5f || Vector3.Distance(p, s) < 3.5f || p.x > 92.5f) continue;
                if (i % 4 == 0) pots.Ball(p + Vector3.up * 0.25f, new Vector3(0.5f, 0.7f, 0.5f), 3, 6);
                else coins.Cylinder(p, 0.12f, 0.03f, 6);
            }
            Twinkling(coins, kit.Root, "SunkenCoins", new Color(1f, 0.85f, 0.45f), 0.6f, 1.6f, 1.6f, 0.1f);
            pots.Build(kit.Root, "BrokenPots", PatchworkAssets.Felt(new Color(0.72f, 0.45f, 0.32f)));
        }
    }

    // A giant clam's lid: shut for a while, then slowly open (long enough to swim in for the
    // pearl), then shut again.
    public class ClamBreathe : MonoBehaviour
    {
        public float Open = 42f, Cycle = 9f;

        void Update()
        {
            float t = Mathf.Repeat(Time.time / Cycle, 1f);
            // Shut for the first third, opening, held open, then closing.
            float k = t < 0.3f ? 0f : t < 0.42f ? Mathf.SmoothStep(0f, 1f, (t - 0.3f) / 0.12f) : t < 0.85f ? 1f : 1f - Mathf.SmoothStep(0f, 1f, (t - 0.85f) / 0.15f);
            transform.localRotation = Quaternion.Euler(-k * Open, 0f, 0f);
        }
    }

    // A treasure chest under the water: swim up to it in Water Form and the lid swings open,
    // showing what's inside.
    public class TreasureChest : PlayerVolume
    {
        public Transform Lid;
        public GameObject Prize;
        public string MarbleId;
        bool open;
        float k;

        void Start()
        {
            // Already been opened (the marble's been taken): leave it open and empty.
            if (!string.IsNullOrEmpty(MarbleId) && GameState.IsCollected(MarbleId)) { open = true; k = 1f; Apply(); }
            else if (Prize != null) Prize.SetActive(false);
        }

        protected override void OnPlayerStay(PatchworkPlayer player, float dt)
        {
            if (open) return;
            if (!player.Motor.Swimming && !player.Motor.WasSwimming) return;
            open = true;
            if (Prize != null) Prize.SetActive(true);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position + Vector3.up * 0.5f, new Color(1f, 0.85f, 0.4f));
            Sfx.Play(SfxId.PrizeFanfare, 0.45f);
            if (player.Game != null) player.Game.ShowToast("The chest creaks open!");
        }

        void Update()
        {
            if (!open || k >= 1f) return;
            k = Mathf.MoveTowards(k, 1f, Time.deltaTime / 1.2f);
            Apply();
        }

        void Apply()
        {
            if (Lid != null) Lid.localRotation = Quaternion.Euler(Mathf.SmoothStep(0f, 1f, k) * 105f, 0f, 0f);
        }
    }
}

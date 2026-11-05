using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The bigger toybox's new ground:
    //   Landing Meadow (south-east) - your Pod lands here; a stepping-stone path and a trail of
    //                                 bubbles lead north to the bridge into town
    //   The Bouncy Castle (north)    - an inflatable castle: the whole floor bounces; bounce up
    //                                 the corner towers to the keep for the crown
    //   Sky Hoops (east)             - ride a tall updraft up as Wind and glide through eight
    //                                 felt hoops against the clock
    public static partial class Level1Builder
    {
        // ---------------------------------------------------------------- Landing Meadow

        static void LandingMeadow(LevelKit kit)
        {
            var pod = LandedPod.Build(kit, PodLanding, 0f);
            kit.AddCheckpoint(SpawnPoint, 0f);
            kit.Hint(SpawnPoint + new Vector3(0f, 1.5f, 2f), new Vector3(8f, 4f, 6f), "Landing Meadow",
                "The Pod's landed just outside Toybox Town. Follow the stepping stones (and the bubbles) north to the bridge into town!");

            // Patch hops out of the Pod's door when you arrive.
            var go = new GameObject("PodArrival");
            go.transform.SetParent(kit.Root, false);
            var arrive = go.AddComponent<PodLaunch>();
            arrive.Inside = pod.Inside;
            arrive.Mouth = pod.DoorStep;
            arrive.Land = SpawnPoint;
            arrive.Apex = 0.9f;
            arrive.Duration = 0.85f;
            arrive.Door = pod.Door;
            arrive.Gentle = true;
            arrive.CameraFrom = SpawnPoint + new Vector3(-6f, 3.4f, 7f);
            arrive.CameraAt = PodLanding + new Vector3(0f, 2.2f, 0f);

            // Stepping stones north to the Tea House lane, then west to the south bridge.
            var stones = new[] { PatchworkAssets.Felt(new Color(0.98f, 0.93f, 0.85f)), PatchworkAssets.Felt(new Color(0.98f, 0.75f, 0.8f)), PatchworkAssets.Felt(new Color(0.85f, 0.9f, 1f)) };
            var path = new List<Vector3>();
            for (float z = -95f; z <= -45f; z += 2.4f) path.Add(new Vector3(50f + Mathf.Sin(z * 0.15f) * 1.2f, 0f, z));
            for (float x = 48f; x >= 41.5f; x -= 2.4f) path.Add(new Vector3(x, 0f, -42.5f));
            for (int i = 0; i < path.Count; i++)
            {
                var p = path[i];
                kit.Deco(PrimitiveType.Cylinder, p + Vector3.up * 0.02f, new Vector3(1.5f, 0.02f, 1.2f), stones[i % stones.Length], new Vector3(0f, i * 37f, 0f));
                if (i % 3 == 1) ScoreBubbles.Place(kit, p + Vector3.up * 1.2f, 10);
            }

            // A welcome arch of flowers at the start of the path, and meadow flowers all round.
            TownKit.FlowerArch(kit, SpawnPoint + new Vector3(0f, 0f, 4f), 0f, 4f);
            var rng = new System.Random(21);
            for (int i = 0; i < 60; i++)
            {
                Vector3 p = new Vector3(44f + (float)rng.NextDouble() * 60f, 0f, -112f + (float)rng.NextDouble() * 22f);
                if (Mathf.Abs(p.x - 50f) < 2.5f) continue;   // keep the path clear
                if ((p - PodLanding).sqrMagnitude < 36f) continue;
                kit.Flower(p, FlowerColors[i % FlowerColors.Length]);
            }
            for (int i = 0; i < 6; i++)
                kit.YarnTree(new Vector3(60f + i * 8f + (i % 2) * 3f, 0f, -108f + (i % 3) * 5f), 5f + (i % 3), FlowerColors[i % FlowerColors.Length]);
            // A picnic blanket with a basket, for the view.
            kit.Deco(PrimitiveType.Cube, new Vector3(60f, 0.02f, -94f), new Vector3(4f, 0.02f, 3f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Gingham, new Color(0.99f, 0.96f, 0.9f), new Color(0.9f, 0.3f, 0.35f)), new Vector3(0f, 15f, 0f));
            kit.Deco(PrimitiveType.Cube, new Vector3(60.5f, 0.35f, -94.2f), new Vector3(1f, 0.6f, 0.7f), PatchworkAssets.Wood(new Color(0.8f, 0.6f, 0.4f)), new Vector3(0f, 15f, 0f));
        }

        // ---------------------------------------------------------------- The Bouncy Castle

        static void BouncyCastle(LevelKit kit)
        {
            Vector3 c = new Vector3(20f, 0f, 102f);
            Color pink = new Color(1f, 0.55f, 0.72f), yellow = new Color(1f, 0.85f, 0.35f), blue = new Color(0.4f, 0.7f, 1f), cream = new Color(0.99f, 0.96f, 0.9f);
            var baseMat = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, pink, cream);
            var puff = PatchworkAssets.Plain(yellow, 0.6f);
            var tower = PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, blue, cream);
            // The inflatable base and its two puffy steps.
            kit.Block(c + Vector3.up * 0.6f, new Vector3(18f, 1.2f, 14f), Color.white, "CastleBase", baseMat);
            kit.Block(c + new Vector3(0f, 0.2f, -7.6f), new Vector3(6f, 0.4f, 1.2f), Color.white, "CastleStep", PatchworkAssets.Plain(yellow, 0.6f));
            kit.Block(c + new Vector3(0f, 0.6f, -7.2f), new Vector3(6f, 0.4f, 0.5f), Color.white, "CastleStep", PatchworkAssets.Plain(yellow, 0.6f));
            // The whole floor bounces.
            var floor = kit.Volume<BouncePad>(c + new Vector3(0f, 1.6f, -0.5f), new Vector3(14.5f, 0.8f, 11f), "CastleFloor");
            floor.LaunchSpeed = 12f;
            // Puffy walls round three sides.
            for (int s = -1; s <= 1; s += 2)
            {
                var w = kit.Deco(PrimitiveType.Capsule, c + new Vector3(s * 8.6f, 1.8f, 0f), new Vector3(1.2f, 6.4f, 1.2f), puff, new Vector3(90f, 0f, 0f));
                w.name = "PuffWall";
                kit.Block(c + new Vector3(s * 8.6f, 1.8f, 0f), new Vector3(1f, 1.2f, 12.8f), Color.white, "PuffWallSolid").GetComponent<Renderer>().enabled = false;
            }
            kit.Deco(PrimitiveType.Capsule, c + new Vector3(0f, 1.8f, 6.6f), new Vector3(1.2f, 8.4f, 1.2f), puff, new Vector3(0f, 0f, 90f));
            kit.Block(c + new Vector3(0f, 1.8f, 6.6f), new Vector3(16.8f, 1.2f, 1f), Color.white, "PuffWallSolid").GetComponent<Renderer>().enabled = false;

            // Four corner towers with bouncy tops, and the tall keep at the back.
            Vector3[] corners = { new Vector3(-7f, 0f, -5f), new Vector3(7f, 0f, -5f), new Vector3(-7f, 0f, 5f), new Vector3(7f, 0f, 5f) };
            foreach (var k in corners)
            {
                Vector3 b = c + k;
                kit.Block(b + Vector3.up * 1.8f, new Vector3(2.6f, 3.6f, 2.6f), Color.white, "CastleTower").GetComponent<Renderer>().enabled = false;
                kit.Deco(PrimitiveType.Cylinder, b + Vector3.up * 1.8f, new Vector3(3f, 1.8f, 3f), tower);
                for (int m = 0; m < 6; m++)
                {
                    float a = m * Mathf.PI / 3f;
                    kit.Deco(PrimitiveType.Sphere, b + new Vector3(Mathf.Cos(a) * 1.3f, 3.75f, Mathf.Sin(a) * 1.3f), Vector3.one * 0.7f, puff);
                }
                var top = kit.Volume<BouncePad>(b + Vector3.up * 4.1f, new Vector3(2.2f, 0.8f, 2.2f), "TowerBounce");
                top.LaunchSpeed = 17f;
                kit.Deco(PrimitiveType.Cylinder, b + Vector3.up * 3.62f, new Vector3(2.2f, 0.04f, 2.2f), PatchworkAssets.Plain(pink, 0.5f));
            }
            Vector3 keep = c + new Vector3(0f, 0f, 3.2f);
            kit.Block(keep + Vector3.up * 4.25f, new Vector3(3.2f, 8.5f, 3.2f), Color.white, "Keep").GetComponent<Renderer>().enabled = false;
            kit.Deco(PrimitiveType.Cylinder, keep + Vector3.up * 4.25f, new Vector3(3.8f, 4.25f, 3.8f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, pink, yellow));
            for (int m = 0; m < 8; m++)
            {
                float a = m * Mathf.PI / 4f;
                kit.Deco(PrimitiveType.Sphere, keep + new Vector3(Mathf.Cos(a) * 1.7f, 8.7f, Mathf.Sin(a) * 1.7f), Vector3.one * 0.8f, puff);
            }
            kit.Deco(PrimitiveType.Cylinder, keep + Vector3.up * 10f, new Vector3(0.12f, 1.4f, 0.12f), PatchworkAssets.Plain(yellow, 0.7f));
            var flag = kit.Group("CastleFlag", keep + Vector3.up * 11f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.7f, 0f, 0f), new Vector3(1.4f, 0.7f, 0.04f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stars, blue, yellow), default(Vector3), flag);
            flag.gameObject.AddComponent<FlagWave>();
            // The crown on top of the keep.
            var crown = kit.Group("CastleCrown", keep + Vector3.up * 9.3f);
            var gold = PatchworkAssets.Glow(new Color(1f, 0.82f, 0.3f), 0.25f);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.9f, 0.14f, 0.9f), gold, default(Vector3), crown);
            for (int i = 0; i < 5; i++)
            {
                float a = i * Mathf.PI * 2f / 5f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Sin(a) * 0.4f, 0.24f, Mathf.Cos(a) * 0.4f), new Vector3(0.18f, 0.34f, 0.18f), gold, default(Vector3), crown);
            }
            var spin = crown.gameObject.AddComponent<Spinner>();
            spin.Speed = 60f;
            spin.BobHeight = 0.12f;
            var top2 = kit.Volume<TouchZone>(keep + Vector3.up * 9.6f, new Vector3(3f, 2f, 3f), "KeepTop");
            top2.Entered = player =>
            {
                if (Merits.Has(Merits.Id.KingOfTheCastle)) return;
                Merits.Earn(Merits.Id.KingOfTheCastle);
                ScoreKeeper.Bonus(1000, keep + Vector3.up * 10f);
                FxBurst.Spawn(FxBurst.Kind.Scraps, keep + Vector3.up * 10f, yellow);
                Sfx.Play(SfxId.PrizeFanfare, 0.6f);
                if (player.Game != null) player.Game.ShowToast("King of the Castle!");
            };
            kit.ThreadLine(c + new Vector3(-4f, 4.5f, -2f), c + new Vector3(4f, 4.5f, -2f), 5);
            kit.Hint(c + new Vector3(0f, 1.5f, -10f), new Vector3(8f, 3f, 4f), "The Bouncy Castle",
                "The whole floor bounces! Bounce onto a corner tower, then bounce from there to the top of the keep.");
            kit.AddCameraZone(c + new Vector3(0f, 6f, 0f), new Vector3(22f, 14f, 18f), 3f, 6f);
        }

        // ---------------------------------------------------------------- Sky Hoops

        // A tall spool tower with a big updraft on the east side. Step on its button, float up
        // as Wind and glide down through eight felt hoops before the clock runs out.
        static void SkyHoops(LevelKit kit)
        {
            Vector3 towerFeet = new Vector3(103f, 0f, -20f);
            Color[] threads = { new Color(1f, 0.55f, 0.72f), new Color(1f, 0.85f, 0.35f), new Color(0.4f, 0.7f, 1f) };
            kit.SpoolTower(towerFeet + new Vector3(4.5f, 0f, 0f), 14f, threads);
            kit.Updraft(towerFeet, 22f, LevelKit.UpdraftStyle.Vent);
            var game = kit.Volume<SkyHoopChallenge>(towerFeet + new Vector3(-3.5f, 1f, -1f), new Vector3(1.8f, 2f, 1.8f), "SkyHoops");
            game.Title = "Sky Hoops";
            game.Duration = 40f;
            var btnRoot = kit.Group("SkyHoopsButton", towerFeet + new Vector3(-3.5f, 0f, -1f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.06f, 0f), new Vector3(1.6f, 0.06f, 1.6f), PatchworkAssets.Felt(new Color(0.99f, 0.95f, 0.88f)), default(Vector3), btnRoot);
            game.Button = kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.18f, 0f), new Vector3(1.1f, 0.12f, 1.1f), PatchworkAssets.Plain(new Color(0.4f, 0.7f, 1f), 0.7f), default(Vector3), btnRoot);

            // Eight hoops gliding north, gently descending.
            for (int i = 0; i < 8; i++)
            {
                float z = -8f + i * 9f;
                Vector3 p = new Vector3(103f + Mathf.Sin(i * 0.9f) * 6f, 19f - i * 2f, z);
                var hoop = SkyHoop.Create(kit, p, threads[i % threads.Length], game);
                game.Hoops.Add(hoop);
            }
            kit.Hint(towerFeet + new Vector3(-3.5f, 1.5f, -3.5f), new Vector3(5f, 3f, 3f), "Sky Hoops",
                "Step on the button, then float up the updraft as Wind and glide through all eight hoops before time runs out! Tap {Ability} for a gust if you're dropping too fast.", HintTheme.Wind);
        }
    }

    // The Sky Hoops race: glide through every hoop before the clock runs out.
    public class SkyHoopChallenge : TimedChallenge
    {
        public readonly List<SkyHoop> Hoops = new List<SkyHoop>();

        protected override void OnBegin()
        {
            Total = Hoops.Count;
            foreach (var h in Hoops) h.Lit = false;
            var game = PatchworkGame.Instance;
            if (game != null) game.ShowToast("Up the updraft as Wind, then glide through the hoops!");
        }

        protected override void OnEnd(bool won)
        {
            if (!won) { foreach (var h in Hoops) h.Lit = false; return; }
            Merits.Earn(Merits.Id.SkyGlider);
            ScoreKeeper.Bonus(1500, transform.position + Vector3.up * 2f);
            var game = PatchworkGame.Instance;
            if (game != null) game.ShowToast("Every hoop! What a glide!");
        }

        public void Through(SkyHoop h)
        {
            if (Running != this || h.Lit) return;
            h.Lit = true;
            Sfx.PlayPitched(SfxId.Chime, 0.6f, 1f + Got * 0.08f);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, h.transform.position, new Color(1f, 0.9f, 0.5f));
            ScoreKeeper.Bonus(100, h.transform.position);
            Count();
        }
    }

    // One felt hoop hanging in the sky. Glows gold once you've flown through it.
    public class SkyHoop : PlayerVolume
    {
        public SkyHoopChallenge Game;
        readonly List<Renderer> beads = new List<Renderer>();
        Material off, on;
        bool lit;
        public bool Lit
        {
            get { return lit; }
            set { lit = value; foreach (var r in beads) r.sharedMaterial = lit ? on : off; }
        }

        public static SkyHoop Create(LevelKit kit, Vector3 pos, Color col, SkyHoopChallenge game)
        {
            var h = kit.Volume<SkyHoop>(pos, new Vector3(3.6f, 3.6f, 1.6f), "SkyHoop");
            h.Game = game;
            h.off = PatchworkAssets.Felt(col);
            h.on = PatchworkAssets.Glow(new Color(1f, 0.85f, 0.35f), 1.2f);
            for (int k = 0; k < 18; k++)
            {
                float a = k * Mathf.PI * 2f / 18f;
                var b = kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.62f, h.off, default(Vector3), h.transform);
                b.localPosition = new Vector3(Mathf.Cos(a) * 2.1f, Mathf.Sin(a) * 2.1f, 0f);
                b.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                h.beads.Add(b.GetComponent<Renderer>());
            }
            var bob = h.gameObject.AddComponent<Spinner>();
            bob.Speed = 0f;
            bob.BobHeight = 0.25f;
            bob.BobSpeed = 0.8f;
            return h;
        }

        protected override void OnPlayerEnter(PatchworkPlayer player) { if (Game != null) Game.Through(this); }
    }
}

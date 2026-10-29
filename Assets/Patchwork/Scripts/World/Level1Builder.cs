using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Level 1 - The Sewing Basket Garden.
    //
    // Story: the Fray, a grey static of fraying thread that unravels whatever it touches, has
    // seeped into the garden through a torn seam in the sky. Its knots have choked the Heart Spool
    // in the middle of the basket and its threads are spreading out into every corner. Patch finds
    // the garden's golden Heartstrings and threads them back onto the spool. Each one peels the
    // Fray off a little more; four of them wake the spool, which shoots a golden thread up to the
    // knot the Fray has tied in the sky. Break that knot and the garden's heart is free.
    //
    // Layout: an open garden you can tackle in any order (like a Mario Odyssey kingdom).
    //   Middle - The Mending Square: the Heart Spool, six Fray knots to pop (a Heartstring).
    //   North  - Pincushion Peaks (Yarn): swing, bounce, swing up floating pincushions.
    //   East   - Dye Falls (Water): ride three fountains up the dye cascades; a Thread Race below.
    //   West   - Breezy Bluffs (Wind): glide, updraft, gust across the bluffs to the windmill;
    //            a secret updraft to a floating cloud patch, and the humming spool tower.
    //   South  - Spoolwork Ruins: ride the sliding buttons to the Snarling arena, then slip
    //            through a drain into the old loom's courtyard.
    //   Sky    - The Fray's Knot, reached by the golden lift once four Heartstrings are found.
    // Heartstrings: 8 in all, 4 needed for the finale. Golden buttons: 3 secrets (hats, Button Box).
    // Movement rules from DEV_NOTES / the review: jump ~5 m and 2 m up, glide alone ~8 m,
    // glide + gust ~13.4 m, swing 15 m, bounce pad 16 lifts Yarn ~5 m. Every gap is clearly
    // possible (<= ~80% of the max) or clearly not.
    public static class Level1Builder
    {
        public const string SceneName = "Level1";
        // Same ids as before so old saves keep their buttons.
        public static readonly string[] CollectibleIds = { "L1_fountain", "L1_swing", "L1_updraft" };
        public static readonly string[] HeartstringIds =
        {
            "L1_hs_peaks", "L1_hs_dye", "L1_hs_windmill", "L1_hs_loom",
            "L1_hs_knots", "L1_hs_race", "L1_hs_tangle", "L1_hs_cloud"
        };
        public const int HeartstringsNeeded = 4;

        // Region origins (the middle of each landing).
        static readonly Vector3 North = new Vector3(0f, 0f, 32f);
        static readonly Vector3 East = new Vector3(32f, 0f, 0f);
        static readonly Vector3 West = new Vector3(-32f, 0f, 0f);
        static readonly Vector3 South = new Vector3(0f, 0f, -31f);
        static readonly Vector3 SpawnPoint = new Vector3(0f, 0f, -11f);
        static readonly Vector3 SkyIsland = new Vector3(0f, 30f, 11f);
        const float PlazaRadius = 17f;

        static readonly Color[] Lawn = { new Color(0.55f, 0.75f, 0.45f), new Color(0.48f, 0.68f, 0.42f), new Color(0.62f, 0.8f, 0.5f), new Color(0.5f, 0.72f, 0.5f) };
        static readonly Color[] Peaks = { new Color(0.92f, 0.62f, 0.6f), new Color(0.95f, 0.75f, 0.62f), new Color(0.86f, 0.6f, 0.68f), new Color(0.97f, 0.84f, 0.7f) };
        static readonly Color[] Dye = { new Color(0.55f, 0.75f, 0.82f), new Color(0.62f, 0.8f, 0.74f), new Color(0.7f, 0.82f, 0.9f), new Color(0.82f, 0.86f, 0.7f) };
        static readonly Color[] Breezy = { new Color(0.78f, 0.88f, 0.78f), new Color(0.86f, 0.9f, 0.74f), new Color(0.74f, 0.85f, 0.82f), new Color(0.94f, 0.92f, 0.78f) };
        static readonly Color[] Ruins = { new Color(0.75f, 0.68f, 0.85f), new Color(0.82f, 0.74f, 0.88f), new Color(0.7f, 0.66f, 0.8f), new Color(0.88f, 0.82f, 0.9f) };
        static readonly Color[] SpoolThreads = { LevelKit.Palette.Rose, LevelKit.Palette.Sky, LevelKit.Palette.Butter, LevelKit.Palette.Sage, LevelKit.Palette.Lavender };
        static readonly Color[] FlowerColors = { LevelKit.Palette.Rose, LevelKit.Palette.Butter, LevelKit.Palette.Lavender, LevelKit.Palette.Peach, LevelKit.Palette.Sky, new Color(1f, 0.6f, 0.7f) };

        static Level1Director director;

        public static LevelInfo Build(LevelKit kit)
        {
            var info = kit.Info;
            info.Id = SceneName;
            info.Title = "Level 1 · The Sewing Basket Garden";
            info.Subtitle = "The Fray has tangled the garden's heart. Find the Heartstrings and mend it.";
            info.Spawn = SpawnPoint + Vector3.up * 0.1f;
            info.SpawnYaw = 0f;
            info.KillY = -14f;
            info.Sky = new Color(0.78f, 0.88f, 0.95f);
            info.HeartstringsNeeded = HeartstringsNeeded;

            var dirGo = new GameObject("Level1Director");
            dirGo.transform.SetParent(kit.Root, false);
            director = dirGo.AddComponent<Level1Director>();
            director.Ids = HeartstringIds;
            director.Needed = HeartstringsNeeded;

            // A giant wicker basket around the whole garden, with a felt sky lid and a lake of paint
            // and ink below (that's the gloopy sound you can hear).
            kit.Enclosure(new Vector3(-84f, -45f, -84f), new Vector3(84f, 48f, 84f),
                new[] { new Color(0.62f, 0.45f, 0.28f), new Color(0.75f, 0.56f, 0.34f), new Color(0.82f, 0.64f, 0.4f), new Color(0.88f, 0.72f, 0.48f) },
                new Color(0.62f, 0.78f, 0.95f), true);
            kit.FeltSun(new Vector3(-20f, 34f, 83.6f), 7f, Vector3.back);
            BuildPaintLake(kit);
            for (int i = 0; i < 14; i++)
            {
                float a = i / 14f * Mathf.PI * 2f;
                kit.Cloud(new Vector3(Mathf.Cos(a) * (45f + (i % 3) * 10f), 38f - (i % 2) * 4f, Mathf.Sin(a) * (45f + (i % 3) * 10f)), 1.8f + (i % 3) * 0.5f);
            }

            MendingSquare(kit);
            PincushionPeaks(kit);
            DyeFalls(kit);
            BreezyBluffs(kit);
            SpoolworkRuins(kit);
            FraysKnot(kit);
            YarnBirds(kit);

            // Sounds that come from where they happen.
            var sounds = kit.Group("LevelSounds", Vector3.zero);
            AmbientEmitter.Create(sounds, new Vector3(44f, 5f, 4f), Sfx.Ambient(Sfx.AmbientClip.Fountain), 0.35f, 2f, 16f);
            AmbientEmitter.Create(sounds, new Vector3(0f, 6f, -74f), Sfx.Ambient(Sfx.AmbientClip.Fountain), 0.3f, 2f, 12f);
            AmbientEmitter.Create(sounds, new Vector3(-77.5f, 13f, 5f), Sfx.Ambient(Sfx.AmbientClip.Flag), 0.35f, 2f, 14f);
            AmbientEmitter.Create(sounds, new Vector3(-55f, 6f, 0f), Sfx.Ambient(Sfx.AmbientClip.Vent), 0.3f, 3f, 18f);

            director.ApplySavedState();
            return info;
        }

        // ================================================================ Shared pieces

        static Vector2[] Octagon(Vector3 c, float r)
        {
            var pts = new Vector2[8];
            for (int i = 0; i < 8; i++)
            {
                float a = (i + 0.5f) / 8f * Mathf.PI * 2f;
                pts[i] = new Vector2(c.x + Mathf.Cos(a) * r, c.z + Mathf.Sin(a) * r);
            }
            return pts;
        }

        // A Heartstring at pos. Hidden ones appear when their challenge is done.
        static Heartstring MakeHeartstring(LevelKit kit, string id, string title, Vector3 pos, bool hidden)
        {
            kit.Info.HeartstringIds.Add(id);
            var h = kit.Volume<Heartstring>(pos, new Vector3(2.2f, 2.8f, 2.2f), "Heartstring_" + id);
            var vis = new GameObject("Visual").transform;
            vis.SetParent(h.transform, false);
            HeartShape(kit, vis, 0.9f, PatchworkAssets.Glow(new Color(1f, 0.78f, 0.35f), 0.45f));
            // A loop of pink thread circling it.
            var ring = new GameObject("Ring").transform;
            ring.SetParent(vis, false);
            var pink = PatchworkAssets.Glow(new Color(1f, 0.55f, 0.7f), 0.6f);
            for (int i = 0; i < 14; i++)
            {
                float a = i / 14f * Mathf.PI * 2f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 1.05f, 0f, Mathf.Sin(a) * 1.05f), Vector3.one * 0.13f, pink, default(Vector3), ring);
            }
            var lightGo = new GameObject("Glow");
            lightGo.transform.SetParent(h.transform, false);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.8f, 0.5f);
            light.range = 7f;
            light.intensity = 1.6f;
            light.shadows = LightShadows.None;
            h.Setup(id, title, vis, ring, light, hidden);
            return h;
        }

        // A puffy heart: two balls and a diamond, in the x-y plane.
        static void HeartShape(LevelKit kit, Transform parent, float s, Material mat)
        {
            kit.Deco(PrimitiveType.Sphere, new Vector3(-0.3f, 0.2f, 0f) * s, new Vector3(0.75f, 0.75f, 0.5f) * s, mat, default(Vector3), parent);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0.3f, 0.2f, 0f) * s, new Vector3(0.75f, 0.75f, 0.5f) * s, mat, default(Vector3), parent);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, -0.12f, 0f) * s, new Vector3(0.68f, 0.68f, 0.46f) * s, mat, new Vector3(0f, 0f, 45f), parent);
        }

        // A Fray knot you pop by landing on it.
        static SnarlKnot MakeKnot(LevelKit kit, Vector3 pos, float size = 1f)
        {
            var k = kit.Volume<SnarlKnot>(pos, new Vector3(2.2f, 2.2f, 2.2f) * size, "SnarlKnot");
            var vis = new GameObject("Visual").transform;
            vis.SetParent(k.transform, false);
            KnotShape(kit, vis, size, 12);
            k.Setup(vis);
            return k;
        }

        static void KnotShape(LevelKit kit, Transform parent, float size, int strands)
        {
            var dark = PatchworkAssets.Yarn(SnarlKnot.FrayDark);
            var grey = PatchworkAssets.Yarn(SnarlKnot.FrayGrey);
            var glow = PatchworkAssets.Glow(SnarlKnot.FrayGlow, 1.2f);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 1.1f * size, dark, default(Vector3), parent);
            for (int i = 0; i < strands; i++)
            {
                Vector3 e = new Vector3((i * 47) % 360, (i * 83) % 360, (i * 29) % 360);
                var strand = kit.Deco(PrimitiveType.Capsule, Vector3.zero, new Vector3(0.16f, 0.7f, 0.16f) * size, i % 3 == 0 ? dark : grey, e, parent);
                strand.localPosition = Quaternion.Euler(e) * new Vector3(0f, 0f, 0.3f * size);
            }
            for (int i = 0; i < 3; i++)
                kit.Deco(PrimitiveType.Sphere, Quaternion.Euler(i * 120f, i * 70f, 0f) * new Vector3(0f, 0f, 0.5f * size), Vector3.one * 0.18f * size, glow, default(Vector3), parent);
        }

        // A tear in the quilt where the Fray has got in, with loose threads around the edge.
        static void FrayTear(LevelKit kit, FrayRegion region, Vector3 pos, float size)
        {
            var g = kit.Group("FrayTear", pos);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.04f, 0f), new Vector3(size, 0.03f, size * 0.7f), PatchworkAssets.Felt(SnarlKnot.FrayDark), new Vector3(0f, (pos.x * 13f) % 360f, 0f), g);
            var grey = PatchworkAssets.Yarn(SnarlKnot.FrayGrey);
            for (int i = 0; i < 9; i++)
            {
                float a = i / 9f * Mathf.PI * 2f;
                var t = kit.Deco(PrimitiveType.Capsule, new Vector3(Mathf.Cos(a) * size * 0.45f, 0.12f, Mathf.Sin(a) * size * 0.32f), new Vector3(0.06f, 0.22f + (i % 3) * 0.08f, 0.06f), grey, default(Vector3), g);
                t.localRotation = Quaternion.LookRotation(new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a))) * Quaternion.Euler(60f + (i % 2) * 15f, 0f, 0f);
            }
            region.AddFray(g);
        }

        // A wavering tendril of Fray thread with a violet glow at the tip, and grey static around it.
        static void FrayTendril(LevelKit kit, FrayRegion region, Vector3 pos, float height)
        {
            var g = kit.Group("FrayTendril", pos);
            var dark = PatchworkAssets.Yarn(SnarlKnot.FrayDark);
            var grey = PatchworkAssets.Yarn(SnarlKnot.FrayGrey);
            Vector3 p = Vector3.zero;
            int segs = Mathf.Max(3, Mathf.RoundToInt(height / 0.7f));
            for (int i = 0; i < segs; i++)
            {
                Vector3 next = p + new Vector3(Mathf.Sin(i * 1.7f) * 0.3f, height / segs, Mathf.Cos(i * 1.3f) * 0.3f);
                var seg = kit.Deco(PrimitiveType.Cylinder, (p + next) * 0.5f, new Vector3(0.12f, (next - p).magnitude * 0.5f, 0.12f), i % 2 == 0 ? dark : grey, default(Vector3), g);
                seg.localRotation = Quaternion.FromToRotation(Vector3.up, (next - p).normalized);
                p = next;
            }
            kit.Deco(PrimitiveType.Sphere, p, Vector3.one * 0.3f, PatchworkAssets.Glow(SnarlKnot.FrayGlow, 1.5f), default(Vector3), g);
            for (int i = 0; i < 4; i++)
            {
                var mote = kit.Deco(PrimitiveType.Cube, new Vector3(Mathf.Cos(i * 1.6f) * 0.8f, height * (0.3f + i * 0.15f), Mathf.Sin(i * 1.6f) * 0.8f), Vector3.one * 0.12f, dark, new Vector3(i * 30f, i * 45f, 0f), g);
                var sp = mote.gameObject.AddComponent<Spinner>();
                sp.Axis = new Vector3(1f, 1f, 0f);
                sp.Speed = 120f;
                sp.BobHeight = 0.2f;
                sp.BobSpeed = 2f + i * 0.4f;
            }
            var sway = g.gameObject.AddComponent<FlagWave>();
            sway.Amount = 25f;
            sway.Speed = 0.8f;
            region.AddFray(g);
        }

        // Flowers that grow back once a corner is mended.
        static void Blooms(LevelKit kit, FrayRegion region, Vector3 center, Vector2 half, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float x = center.x + (((i * 37) % 100) / 50f - 1f) * half.x;
                float z = center.z + (((i * 61 + 17) % 100) / 50f - 1f) * half.y;
                region.AddBloom(kit.Flower(new Vector3(x, center.y, z), FlowerColors[i % FlowerColors.Length]));
            }
        }

        static FrayRegion MakeRegion(LevelKit kit, string name, string heartstringId)
        {
            var go = new GameObject("Region_" + name);
            go.transform.SetParent(kit.Root, false);
            var r = go.AddComponent<FrayRegion>();
            r.HeartstringId = heartstringId;
            director.Regions[heartstringId] = r;
            return r;
        }

        // A stitched archway at the start of each region's path, in its colour, with a sign on top.
        static void RegionArch(LevelKit kit, Vector3 pos, float yaw, Color color, int icon)
        {
            var g = kit.Group("RegionArch", pos);
            g.rotation = Quaternion.Euler(0f, yaw, 0f);
            var felt = PatchworkAssets.Felt(color);
            var trim = PatchworkAssets.Felt(Color.Lerp(color, Color.white, 0.5f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-3.6f, 2.4f, 0f), new Vector3(0.6f, 2.4f, 0.6f), felt, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(3.6f, 2.4f, 0f), new Vector3(0.6f, 2.4f, 0.6f), felt, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 5f, 0f), new Vector3(8.4f, 0.7f, 0.7f), trim, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(-3.6f, 4.9f, 0f), Vector3.one * 0.9f, felt, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(3.6f, 4.9f, 0f), Vector3.one * 0.9f, felt, default(Vector3), g);
            var sign = new GameObject("Sign").transform;
            sign.SetParent(g, false);
            sign.localPosition = new Vector3(0f, 6.3f, 0f);
            var signMat = PatchworkAssets.Felt(color);
            switch (icon)
            {
                case 0: // yarn ball
                    kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 1.3f, PatchworkAssets.Yarn(color), default(Vector3), sign);
                    break;
                case 1: // water drop
                    kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.15f, 0f), Vector3.one * 1.1f, PatchworkAssets.Felt(new Color(0.45f, 0.65f, 0.85f)), default(Vector3), sign);
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.35f, 0f), Vector3.one * 0.72f, PatchworkAssets.Felt(new Color(0.45f, 0.65f, 0.85f)), new Vector3(0f, 0f, 45f), sign);
                    break;
                case 2: // cloud
                    kit.Deco(PrimitiveType.Sphere, Vector3.zero, new Vector3(1.6f, 0.9f, 0.9f), PatchworkAssets.Felt(new Color(0.98f, 0.97f, 0.94f)), default(Vector3), sign);
                    kit.Deco(PrimitiveType.Sphere, new Vector3(0.4f, 0.3f, 0f), new Vector3(0.9f, 0.9f, 0.8f), PatchworkAssets.Felt(new Color(0.98f, 0.97f, 0.94f)), default(Vector3), sign);
                    break;
                default: // spool
                    kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.9f, 0.5f, 0.9f), PatchworkAssets.Yarn(color), new Vector3(90f, 0f, 0f), sign);
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.5f), new Vector3(1.3f, 0.08f, 1.3f), signMat, new Vector3(90f, 0f, 0f), sign);
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, -0.5f), new Vector3(1.3f, 0.08f, 1.3f), signMat, new Vector3(90f, 0f, 0f), sign);
                    break;
            }
            var spin = sign.gameObject.AddComponent<Spinner>();
            spin.Speed = 35f;
            spin.BobHeight = 0.15f;
            spin.BobSpeed = 1.5f;
        }

        // A little hoop that takes you back to the Mending Square.
        static void MakeHomeHoop(LevelKit kit, Vector3 feet)
        {
            var hoop = kit.Volume<HomeHoop>(feet + Vector3.up * 1.3f, new Vector3(1.8f, 2.6f, 1.8f), "HomeHoop");
            hoop.Target = SpawnPoint + new Vector3(0f, 0.1f, 2.5f);
            hoop.TargetYaw = 0f;
            var ring = new GameObject("Ring").transform;
            ring.SetParent(hoop.transform, false);
            var wood = PatchworkAssets.Wood(new Color(0.85f, 0.68f, 0.45f));
            var glow = PatchworkAssets.Glow(new Color(0.8f, 0.65f, 1f), 0.8f);
            for (int i = 0; i < 18; i++)
            {
                float a = i / 18f * Mathf.PI * 2f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 1.05f, Mathf.Sin(a) * 1.05f, 0f), Vector3.one * 0.26f, i % 3 == 0 ? glow : wood, default(Vector3), ring);
            }
            kit.Deco(PrimitiveType.Cylinder, feet + Vector3.up * 0.06f, new Vector3(1.6f, 0.06f, 1.6f), PatchworkAssets.Felt(LevelKit.Palette.Lavender));
            hoop.Setup(ring);
        }

        // ================================================================ The Mending Square (middle)

        static void MendingSquare(LevelKit kit)
        {
            kit.Prism(Octagon(Vector3.zero, PlazaRadius), -1.2f, 0f, PatchworkAssets.Grass(new Color(0.56f, 0.76f, 0.46f)), "MendingSquare", 3f);
            // Quilted paths out to the four bridges (stopping short of the spool in the middle).
            kit.Quilt(new Vector3(0f, -0.5f, 10.5f), new Vector3(5f, 1f, 11f), LevelKit.Palette.Quilt, 2.5f, "PathN");
            kit.Quilt(new Vector3(0f, -0.5f, -10.5f), new Vector3(5f, 1f, 11f), LevelKit.Palette.Quilt, 2.5f, "PathS");
            kit.Quilt(new Vector3(10.5f, -0.5f, 0f), new Vector3(11f, 1f, 5f), LevelKit.Palette.Quilt, 2.5f, "PathE");
            kit.Quilt(new Vector3(-10.5f, -0.5f, 0f), new Vector3(11f, 1f, 5f), LevelKit.Palette.Quilt, 2.5f, "PathW");

            kit.AddCheckpoint(SpawnPoint, 0f);
            kit.Hint(SpawnPoint + new Vector3(0f, 1.5f, 0f), new Vector3(10f, 4f, 6f), "The Fray",
                "A grey static called the Fray has crept into the garden, unravelling everything it touches. Find the golden Heartstrings and thread them back onto the Heart Spool to mend it!");

            // ----- The Heart Spool -----
            kit.Prism(Octagon(Vector3.zero, 3.6f), 0f, 1.8f, PatchworkAssets.Yarn(new Color(0.88f, 0.5f, 0.55f)), "HeartSpool", 1.5f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.84f, 0f), new Vector3(7.4f, 0.04f, 7.4f), PatchworkAssets.Wood(new Color(0.82f, 0.64f, 0.42f)));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.12f, 0f), new Vector3(8f, 0.12f, 8f), PatchworkAssets.Wood(new Color(0.75f, 0.58f, 0.38f)));
            var spindle = kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 4.8f, 0f), new Vector3(0.9f, 3f, 0.9f), PatchworkAssets.Wood(new Color(0.85f, 0.7f, 0.5f)));
            spindle.gameObject.AddComponent<CapsuleCollider>();
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 8f, 0f), Vector3.one * 1.3f, PatchworkAssets.Felt(LevelKit.Palette.Rose));

            // A peg for every Heartstring around the rim; each one lights gold when it's found.
            for (int i = 0; i < HeartstringIds.Length; i++)
            {
                float a = i / (float)HeartstringIds.Length * Mathf.PI * 2f;
                var peg = kit.Group("Peg", new Vector3(Mathf.Cos(a) * 2.9f, 1.86f, Mathf.Sin(a) * 2.9f));
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.3f, 0f), new Vector3(0.22f, 0.3f, 0.22f), PatchworkAssets.Felt(new Color(0.55f, 0.5f, 0.6f)), default(Vector3), peg);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.68f, 0f), Vector3.one * 0.36f, PatchworkAssets.Felt(new Color(0.55f, 0.5f, 0.6f)), default(Vector3), peg);
                director.SpoolPegs.Add(peg);
            }
            // The Fray's snarls wound round the spindle: one comes off for each Heartstring (up to four).
            for (int i = 0; i < HeartstringsNeeded; i++)
            {
                var ring = kit.Group("Snarl", new Vector3(0f, 2.7f + i * 1.2f, 0f));
                ring.rotation = Quaternion.Euler(12f * (i % 2 == 0 ? 1f : -1f), i * 40f, 8f * (i - 1.5f));
                var dark = PatchworkAssets.Yarn(SnarlKnot.FrayDark);
                var grey = PatchworkAssets.Yarn(SnarlKnot.FrayGrey);
                for (int j = 0; j < 16; j++)
                {
                    float a = j / 16f * Mathf.PI * 2f;
                    var s = kit.Deco(PrimitiveType.Capsule, new Vector3(Mathf.Cos(a) * 1.2f, Mathf.Sin(j * 2.1f) * 0.15f, Mathf.Sin(a) * 1.2f),
                        new Vector3(0.28f, 0.4f, 0.28f), j % 3 == 0 ? dark : grey, default(Vector3), ring);
                    s.localRotation = Quaternion.LookRotation(new Vector3(-Mathf.Sin(a), 0f, Mathf.Cos(a))) * Quaternion.Euler(90f + (j % 4) * 10f, 0f, 0f);
                }
                kit.Deco(PrimitiveType.Sphere, new Vector3(1.2f, 0f, 0f), Vector3.one * 0.26f, PatchworkAssets.Glow(SnarlKnot.FrayGlow, 1.4f), default(Vector3), ring);
                director.HeartRings.Add(ring);
            }
            kit.Hint(new Vector3(0f, 1.5f, -5.5f), new Vector3(9f, 4f, 2.5f), "The Heart Spool",
                "Every Heartstring you find lights a peg and pulls a snarl off the spool. Thread " + HeartstringsNeeded + " and it'll wake up!");

            // ----- The golden lift (opens at four Heartstrings) -----
            var liftRoot = kit.Group("HeartLift", new Vector3(0f, 1.8f, 0f));
            const float liftHeight = 35f;
            var lift = kit.Volume<HeartLift>(new Vector3(0f, 1.8f + liftHeight * 0.5f, 0f), new Vector3(4.2f, liftHeight, 4.2f), "Lift");
            lift.transform.SetParent(liftRoot, true);
            var goldGlow = PatchworkAssets.Glow(new Color(1f, 0.8f, 0.4f), 0.9f);
            var strands = new Transform[12];
            for (int i = 0; i < strands.Length; i++)
                strands[i] = kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(0.08f, 1.8f, 0.08f), goldGlow, default(Vector3), lift.transform);
            lift.Setup(strands, liftHeight);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, liftHeight * 0.5f, 0f), new Vector3(0.35f, liftHeight * 0.5f, 0.35f), goldGlow, default(Vector3), liftRoot);
            kit.AddCameraZone(new Vector3(0f, 21f, 2f), new Vector3(8f, 28f, 10f), 4f, -8f);
            director.LiftRoot = liftRoot.gameObject;

            // ----- Knot a Problem: six knots of Fray around the square -----
            var hs = MakeHeartstring(kit, "L1_hs_knots", "Knot a Problem", new Vector3(0f, 2.2f, -15f), true);
            var knots = new List<SnarlKnot>
            {
                MakeKnot(kit, new Vector3(8f, 0.9f, -7f)),
                MakeKnot(kit, new Vector3(-8f, 0.9f, -7f)),
                MakeKnot(kit, new Vector3(11.5f, 0.9f, 6.5f)),
                MakeKnot(kit, new Vector3(-11.5f, 0.9f, 6.5f)),
                MakeKnot(kit, new Vector3(12f, 2.9f, -10f)),
                MakeKnot(kit, new Vector3(-12f, 2.9f, -10f))
            };
            kit.Quilt(new Vector3(12f, 1f, -10f), new Vector3(3f, 2f, 3f), LevelKit.Palette.Quilt, 1.5f, "KnotStep");
            kit.Quilt(new Vector3(-12f, 1f, -10f), new Vector3(3f, 2f, 3f), LevelKit.Palette.Quilt, 1.5f, "KnotStep");
            Challenge(knots, hs);
            kit.Hint(new Vector3(8f, 1.5f, -9f), new Vector3(5f, 4f, 5f), "Knots of the Fray",
                "Grey knots like this are the Fray tying the garden up. Jump on top of one to pop it! Bumping into it won't work.");

            // Dressing.
            kit.YarnBall(new Vector3(9f, 0f, 11f), 1.2f, new Color(0.95f, 0.55f, 0.6f));
            kit.YarnBall(new Vector3(-9.5f, 0f, -11.5f), 1f, new Color(0.55f, 0.7f, 0.95f));
            kit.Mushroom(new Vector3(-12.5f, 0f, -4.5f), 1.6f, new Color(0.95f, 0.4f, 0.4f));
            kit.Mushroom(new Vector3(12.5f, 0f, 4f), 1.2f, new Color(1f, 0.7f, 0.3f));
            kit.Thimble(new Vector3(-6f, 0f, 12f), 1.2f);
            kit.SewingPin(new Vector3(7f, 0f, -13f), 3.8f, LevelKit.Palette.Gold, new Vector3(-15f, 0f, 20f));
            for (int i = 0; i < 12; i++)
            {
                float a = (i + 0.5f) / 12f * Mathf.PI * 2f;
                float r = 6.5f + (i % 3) * 2.5f;
                Vector3 p = new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
                if (Mathf.Abs(p.x) < 3f || Mathf.Abs(p.z) < 3f) continue;
                kit.Flower(p, FlowerColors[i % FlowerColors.Length]);
            }
            kit.ThreadLine(new Vector3(-2f, 1f, -6f), new Vector3(2f, 1f, -6f), 3);
            // Bridges.
            kit.Quilt(new Vector3(0f, -0.5f, 21.5f), new Vector3(6f, 1f, 11f), Peaks, 2.5f, "BridgeN");
            kit.Quilt(new Vector3(21.5f, -0.5f, 0f), new Vector3(11f, 1f, 6f), Dye, 2.5f, "BridgeE");
            kit.Quilt(new Vector3(-21.5f, -0.5f, 0f), new Vector3(11f, 1f, 6f), Breezy, 2.5f, "BridgeW");
            kit.Quilt(new Vector3(0f, -0.5f, -21.5f), new Vector3(6f, 1f, 11f), Ruins, 2.5f, "BridgeS");
            RegionArch(kit, new Vector3(0f, 0f, 17f), 0f, new Color(0.92f, 0.6f, 0.6f), 0);
            RegionArch(kit, new Vector3(17f, 0f, 0f), 90f, new Color(0.5f, 0.72f, 0.8f), 1);
            RegionArch(kit, new Vector3(-17f, 0f, 0f), 90f, new Color(0.8f, 0.9f, 0.8f), 2);
            RegionArch(kit, new Vector3(0f, 0f, -17f), 0f, new Color(0.72f, 0.64f, 0.88f), 3);
            kit.ThreadLine(new Vector3(0f, 1f, 18.5f), new Vector3(0f, 1f, 25f), 3);
            kit.ThreadLine(new Vector3(18.5f, 1f, 0f), new Vector3(25f, 1f, 0f), 3);
            kit.ThreadLine(new Vector3(-18.5f, 1f, 0f), new Vector3(-25f, 1f, 0f), 3);
            kit.ThreadLine(new Vector3(0f, 1f, -18.5f), new Vector3(0f, 1f, -25f), 3);
        }

        // When every one of these is popped, the Heartstring appears.
        static void Challenge(List<SnarlKnot> knots, Heartstring prize)
        {
            if (GameState.IsCollected(prize.Id)) { foreach (var k in knots) k.RemoveQuietly(); return; }
            int left = knots.Count;
            foreach (var k in knots)
                k.Popped += _ =>
                {
                    left--;
                    var game = PatchworkGame.Instance;
                    if (left <= 0) prize.Reveal();
                    else if (game != null) game.ShowToast("Knot popped! " + left + " to go.");
                };
        }

        static void Challenge(List<Snarling> foes, Heartstring prize)
        {
            if (GameState.IsCollected(prize.Id)) { foreach (var f in foes) f.RemoveQuietly(); return; }
            int left = foes.Count;
            foreach (var f in foes)
                f.Popped += _ =>
                {
                    left--;
                    var game = PatchworkGame.Instance;
                    if (left <= 0) prize.Reveal();
                    else if (game != null) game.ShowToast("Snarling unravelled! " + left + " left.");
                };
        }

        // ================================================================ North: Pincushion Peaks (Yarn)

        static void PincushionPeaks(LevelKit kit)
        {
            var region = MakeRegion(kit, "Peaks", "L1_hs_peaks");
            Vector3 o = North;
            // Landing (z 27..37).
            kit.Quilt(o + new Vector3(0f, -0.5f, 0f), new Vector3(14f, 1f, 10f), Peaks, 3.5f, "PeaksLanding");
            kit.AddCheckpoint(o + new Vector3(0f, 0f, -3f), 0f);
            kit.Hint(o + new Vector3(0f, 1.5f, 0f), new Vector3(14f, 4f, 6f), "Pincushion Peaks",
                "The peaks float on pincushions. As Yarn, run at a pink button and hold {Ability} to swing. Let go to fly!", HintTheme.Yarn);
            FrayTear(kit, region, o + new Vector3(-4f, 0f, 2f), 2.4f);
            FrayTendril(kit, region, o + new Vector3(5.5f, 0f, 3.5f), 3f);
            Blooms(kit, region, o, new Vector2(6f, 4f), 8);

            // Swing 1: z 37 -> 52 (15 m). The golden button hangs at the bottom of the arc.
            float a1 = o.z + 12.5f;
            kit.Anchor(new Vector3(0f, 8f, a1), 13.5f);
            kit.AddCameraZone(new Vector3(0f, 2f, a1), new Vector3(16f, 24f, 22f), 2.5f, 6f);
            kit.GoldenButton(CollectibleIds[1], new Vector3(0f, -0.4f, a1));
            SwingArcThreads(kit, 8f, a1);

            // Pincushion island (z 52..68) with a bounce up to a ledge 4 m high.
            Vector3 isl = new Vector3(0f, 0f, o.z + 28f);
            kit.Quilt(isl + new Vector3(0f, -0.5f, 0f), new Vector3(10f, 1f, 16f), Peaks, 2.5f, "PincushionIsland");
            NoCollider(kit.Pincushion(isl + new Vector3(0f, -8f, 0f), 6f, new Color(0.9f, 0.35f, 0.35f)));
            kit.AddCheckpoint(isl + new Vector3(-2.5f, 0f, -6f), 0f);
            kit.AddBouncePad(isl + new Vector3(0f, 0f, -1f), 16f);
            kit.Thread(isl + new Vector3(0f, 3f, -1f), LevelKit.Palette.Gold);
            kit.Thread(isl + new Vector3(0f, 4.6f, 0.5f), LevelKit.Palette.Gold);
            kit.Block(isl + new Vector3(0f, 2f, 5f), new Vector3(8f, 4f, 6f), new Color(0.95f, 0.75f, 0.62f), "PeakLedge");
            kit.Hint(isl + new Vector3(0f, 1.5f, -4f), new Vector3(10f, 4f, 4f), "Bounce, then swing again",
                "Bounce up to the ledge, then grab the next pink button with {Ability}.", HintTheme.Yarn);
            FrayTear(kit, region, isl + new Vector3(3f, 0f, -5f), 1.8f);
            FrayTendril(kit, region, isl + new Vector3(-3.5f, 4f, 6.5f), 2.5f);
            Snarling.Create(kit.Root, isl + new Vector3(-3f, 0f, -3f), isl + new Vector3(3f, 0f, -3f));
            Blooms(kit, region, isl + new Vector3(0f, 0f, -4f), new Vector2(3.5f, 2.5f), 6);

            // Swing 2: from the ledge (top 4, z 68) to the summit (top 4, z 83).
            float a2 = o.z + 43.5f;
            kit.Anchor(new Vector3(0f, 12f, a2), 13.5f);
            kit.AddCameraZone(new Vector3(0f, 6f, a2), new Vector3(16f, 24f, 22f), 2.5f, 6f);
            SwingArcThreads(kit, 12f, a2);

            // The summit, on a giant pincushion.
            Vector3 top = new Vector3(0f, 4f, o.z + 56f);
            kit.Quilt(top + new Vector3(0f, -0.5f, 0f), new Vector3(12f, 1f, 10f), Peaks, 3f, "PeakSummit");
            NoCollider(kit.Pincushion(top + new Vector3(0f, -9f, 0f), 7f, new Color(0.85f, 0.4f, 0.5f)));
            MakeHeartstring(kit, "L1_hs_peaks", "Peak Stitch", top + new Vector3(0f, 1.6f, 2f), false);
            MakeHomeHoop(kit, top + new Vector3(4f, 0f, 3f));
            FrayTendril(kit, region, top + new Vector3(-4f, 0f, 3f), 3.5f);
            FrayTear(kit, region, top + new Vector3(2f, 0f, -2.5f), 2f);
            Blooms(kit, region, top, new Vector2(5f, 4f), 8);
            kit.SewingPin(top + new Vector3(-5f, 0f, -3f), 5f, LevelKit.Palette.Gold, new Vector3(-10f, 0f, 15f));
            kit.SewingPin(top + new Vector3(5.2f, 0f, -3.5f), 4.5f, LevelKit.Palette.Sky, new Vector3(10f, 0f, -12f));
        }

        static void NoCollider(Transform t)
        {
            var c = t.GetComponent<Collider>();
            if (c != null) Object.Destroy(c);
        }

        static void SwingArcThreads(LevelKit kit, float anchorY, float anchorZ)
        {
            const float rope = 8.4f;
            float[] angles = { -50f, -28f, 28f, 50f };
            foreach (float a in angles)
            {
                float r = a * Mathf.Deg2Rad;
                kit.Thread(new Vector3(0f, anchorY - rope * Mathf.Cos(r), anchorZ + rope * Mathf.Sin(r)), LevelKit.Palette.Rose);
            }
        }

        // ================================================================ East: Dye Falls (Water)

        static void DyeFalls(LevelKit kit)
        {
            var region = MakeRegion(kit, "DyeFalls", "L1_hs_dye");
            Vector3 o = East;
            // One wide floor (x 27..61) with the tiers stepping up along the north half.
            kit.Quilt(new Vector3(44f, -0.5f, 0f), new Vector3(34f, 1f, 24f), Dye, 3.5f, "DyeFloor", true);
            kit.AddCheckpoint(o + new Vector3(-3f, 0f, 0f), 90f);
            kit.Hint(o + new Vector3(-2f, 1.5f, 0f), new Vector3(6f, 4f, 10f), "Dye Falls",
                "Blue fountains carry Water Form up the falls. Stay as Water at the very top!", HintTheme.Water);

            // Tier 1 (top 4), tier 2 (top 8), tier 3 / summit (top 12), each reached by a fountain.
            kit.Fountain(new Vector3(36f, 0f, 4f), 6f);
            kit.Block(new Vector3(41.1f, 2f, 4f), new Vector3(7.8f, 4f, 8f), new Color(0.55f, 0.75f, 0.85f), "DyeTier1");
            kit.Fountain(new Vector3(43.8f, 4f, 4f), 6f);
            kit.Block(new Vector3(49f, 4f, 4f), new Vector3(8f, 8f, 8f), new Color(0.62f, 0.8f, 0.72f), "DyeTier2");
            kit.Fountain(new Vector3(51.8f, 8f, 4f), 6f);
            kit.Block(new Vector3(57f, 6f, 4f), new Vector3(8f, 12f, 12f), new Color(0.85f, 0.78f, 0.6f), "DyeSummit");
            kit.AddCameraZone(new Vector3(47f, 8f, 4f), new Vector3(28f, 20f, 12f), 3f, 8f);
            kit.Thread(new Vector3(36f, 3f, 4f), LevelKit.Palette.Sky);
            kit.Thread(new Vector3(43.8f, 7f, 4f), LevelKit.Palette.Sky);
            kit.Thread(new Vector3(51.8f, 11f, 4f), LevelKit.Palette.Sky);
            // Dye cascading down the fronts of the tiers.
            Cascade(kit, new Vector3(37.18f, 0f, 1.2f), 4f, new Color(0.95f, 0.5f, 0.65f));
            Cascade(kit, new Vector3(37.18f, 0f, 6.8f), 4f, new Color(0.98f, 0.8f, 0.35f));
            Cascade(kit, new Vector3(44.98f, 4f, 1.2f), 4f, new Color(0.45f, 0.75f, 0.6f));
            Cascade(kit, new Vector3(44.98f, 4f, 6.8f), 4f, new Color(0.7f, 0.55f, 0.9f));
            Cascade(kit, new Vector3(52.98f, 8f, 1.2f), 4f, new Color(0.98f, 0.6f, 0.35f));

            // On the summit: the Heartstring, and a drain hut (only Water gets in) with a golden button.
            Vector3 top = new Vector3(57f, 12f, 4f);
            MakeHeartstring(kit, "L1_hs_dye", "Dye Heart", top + new Vector3(0f, 1.6f, -4f), false);
            DrainHut(kit, top + new Vector3(0f, 0f, 1.2f));
            kit.GoldenButton(CollectibleIds[0], top + new Vector3(0f, 1.1f, 3.2f));
            MakeHomeHoop(kit, top + new Vector3(2.8f, 0f, -4.5f));
            FrayTendril(kit, region, top + new Vector3(-3f, 0f, -4.5f), 3f);
            Blooms(kit, region, top + new Vector3(0f, 0f, -3.5f), new Vector2(3f, 1.5f), 5);

            // Down on the floor: a Thread Race along the south half.
            var race = kit.Volume<ThreadRace>(new Vector3(30f, 1.2f, -8f), new Vector3(2f, 2.4f, 2f), "ThreadRace");
            var watch = new GameObject("Stopwatch").transform;
            watch.SetParent(race.transform, false);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.3f, 0f), new Vector3(1.3f, 0.1f, 1.3f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f), new Vector3(90f, 0f, 0f), watch);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.3f, -0.07f), new Vector3(1.1f, 0.02f, 1.1f), PatchworkAssets.Felt(LevelKit.Palette.Cream), new Vector3(90f, 0f, 0f), watch);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.5f, -0.1f), new Vector3(0.08f, 0.4f, 0.02f), PatchworkAssets.Felt(UIKit.Ink), default(Vector3), watch);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 1f, 0f), Vector3.one * 0.25f, PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f), default(Vector3), watch);
            race.Setup(watch);
            race.TimeLimit = 17f;
            Vector3[] path =
            {
                new Vector3(34f, 1.2f, -9f), new Vector3(38f, 1.2f, -5f), new Vector3(42f, 1.2f, -9.5f), new Vector3(46f, 1.2f, -5f),
                new Vector3(50f, 1.2f, -9.5f), new Vector3(54f, 1.2f, -5f), new Vector3(58f, 4.6f, -8f),
                new Vector3(52f, 1.2f, -11f), new Vector3(44f, 1.2f, -11f), new Vector3(36f, 1.2f, -11f)
            };
            var cyan = PatchworkAssets.Glow(new Color(0.55f, 0.9f, 1f), 0.9f);
            foreach (var p in path)
            {
                var r = kit.Volume<RaceRing>(p, new Vector3(1.8f, 1.8f, 1.8f), "RaceRing");
                var v = new GameObject("Ring").transform;
                v.SetParent(r.transform, false);
                for (int i = 0; i < 12; i++)
                {
                    float a = i / 12f * Mathf.PI * 2f;
                    kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.7f, 0f, Mathf.Sin(a) * 0.7f), Vector3.one * 0.16f, cyan, default(Vector3), v);
                }
                r.Setup(v);
                race.AddRing(r);
            }
            kit.AddBouncePad(new Vector3(58f, 0f, -8f), 14f);
            var raceHs = MakeHeartstring(kit, "L1_hs_race", "Race the Dye", new Vector3(31f, 2.2f, -11f), true);
            if (GameState.IsCollected(raceHs.Id)) race.MarkDone();
            else race.Won += raceHs.Reveal;
            kit.Hint(new Vector3(31f, 1.5f, -8f), new Vector3(4f, 3f, 4f), "Thread Race",
                "Touch the stopwatch and grab every glowing ring before time runs out. Any form will do!");

            // Dressing and the Fray.
            FrayTear(kit, region, new Vector3(33f, 0f, 9f), 2.2f);
            FrayTear(kit, region, new Vector3(48f, 0f, -2f), 1.8f);
            FrayTendril(kit, region, new Vector3(39f, 0f, 10.5f), 3f);
            FrayTendril(kit, region, new Vector3(56f, 0f, -11f), 2.6f);
            Blooms(kit, region, new Vector3(44f, 0f, -3f), new Vector2(14f, 2f), 12);
            Snarling.Create(kit.Root, new Vector3(30f, 0f, 9f), new Vector3(35f, 0f, 9f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(36f, 0.04f, 4f), new Vector3(4.5f, 0.03f, 4.5f), PatchworkAssets.Water(new Color(0.95f, 0.5f, 0.65f)));
            kit.Mushroom(new Vector3(29f, 0f, 10.5f), 1.4f, new Color(0.4f, 0.6f, 0.95f));
            kit.YarnBall(new Vector3(59f, 0f, -11f), 1f, new Color(0.95f, 0.85f, 0.4f));
        }

        // A strip of dye flowing down a wall (scrolling, so it looks like it's running).
        static void Cascade(LevelKit kit, Vector3 baseFront, float height, Color dye)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = "DyeCascade";
            go.transform.SetParent(kit.Root, false);
            go.transform.position = baseFront + new Vector3(-0.06f, height * 0.5f, 0f);
            go.transform.localScale = new Vector3(0.08f, height, 1.6f);
            var mat = PatchworkAssets.Unique(PatchworkAssets.Water(dye));
            mat.SetTextureScale("_BaseMap", new Vector2(1f, height / 2f));
            go.GetComponent<Renderer>().sharedMaterial = mat;
            var flow = go.AddComponent<PaintLake>();
            flow.Drift = new Vector2(0f, 0.6f);
            flow.Setup(mat);
            kit.Deco(PrimitiveType.Sphere, baseFront + new Vector3(-0.6f, 0.05f, 0f), new Vector3(1.8f, 0.15f, 2f), PatchworkAssets.Water(dye));
        }

        // ================================================================ West: Breezy Bluffs (Wind)

        static void BreezyBluffs(LevelKit kit)
        {
            var region = MakeRegion(kit, "Bluffs", "L1_hs_windmill");
            Vector3 o = West;
            // Landing (x -27..-37).
            kit.Quilt(o + new Vector3(0f, -0.5f, 0f), new Vector3(10f, 1f, 14f), Breezy, 3.5f, "BluffsLanding", true);
            kit.AddCheckpoint(o + new Vector3(3f, 0f, 0f), -90f);
            kit.Hint(o + new Vector3(0f, 1.5f, 0f), new Vector3(10f, 4f, 14f), "Breezy Bluffs",
                "Pinwheels mean wind! As Wind, hold {Jump} while falling to glide. Tap {Ability} mid-glide for a gust.", HintTheme.Wind);
            kit.Pinwheel(o + new Vector3(-3f, 0f, 6f), LevelKit.Palette.Sky, LevelKit.Palette.Cream);
            kit.Pinwheel(o + new Vector3(-3f, 0f, -6f), LevelKit.Palette.Rose, LevelKit.Palette.Cream);
            FrayTear(kit, region, o + new Vector3(1f, 0f, -4f), 2f);
            Blooms(kit, region, o, new Vector2(4f, 6f), 8);

            // 7 m glide gap to the second bluff (x -44..-54).
            kit.ThreadLine(new Vector3(-38.5f, 1.6f, 0f), new Vector3(-42.5f, 1f, 0f), 3);
            Vector3 b2 = new Vector3(-49f, 0f, 0f);
            kit.Quilt(b2 + new Vector3(0f, -0.5f, 0f), new Vector3(10f, 1f, 14f), Breezy, 3.5f, "Bluff2", true);
            kit.AddCheckpoint(b2 + new Vector3(4f, 0f, 3f), -90f);
            FrayTendril(kit, region, b2 + new Vector3(3.5f, 0f, -5.5f), 3f);
            Snarling.Create(kit.Root, b2 + new Vector3(-3f, 0f, -2f), b2 + new Vector3(-3f, 0f, -6f));

            // Updraft up to the high bluff (top 9, x -55..-63).
            kit.Updraft(b2 + new Vector3(-2f, 0f, 3f), 12f);
            kit.Block(new Vector3(-59f, 4.5f, 2f), new Vector3(8f, 9f, 12f), new Color(0.8f, 0.86f, 0.74f), "HighBluff");
            kit.Thread(b2 + new Vector3(-2f, 5f, 3f), LevelKit.Palette.Cream);
            kit.Thread(b2 + new Vector3(-2f, 9f, 3f), LevelKit.Palette.Cream);
            kit.AddCameraZone(new Vector3(-58f, 8f, 2f), new Vector3(30f, 22f, 16f), 3f, 6f);
            kit.Hint(b2 + new Vector3(0f, 1.5f, 3f), new Vector3(5f, 4f, 5f), "Updraft",
                "White updrafts lift Wind Form. Float up, then glide over to the high bluff.", HintTheme.Wind);

            // 11 m gust gap to the windmill bluff (x -74..-81).
            kit.ThreadLine(new Vector3(-64f, 10.5f, 2f), new Vector3(-72f, 10f, 2f), 4);
            kit.Block(new Vector3(-77.5f, 4.5f, 2f), new Vector3(7f, 9f, 12f), new Color(0.86f, 0.9f, 0.76f), "WindmillBluff");
            Vector3 mill = new Vector3(-77.5f, 9f, 5f);
            Windmill(kit, mill);
            MakeHeartstring(kit, "L1_hs_windmill", "Windmill Whirl", new Vector3(-76f, 10.6f, 0f), false);
            MakeHomeHoop(kit, new Vector3(-75f, 9f, 6.5f));
            FrayTendril(kit, region, new Vector3(-79.5f, 9f, -2.5f), 3f);
            Blooms(kit, region, new Vector3(-77.5f, 9f, 0f), new Vector2(2.5f, 3f), 6);
            kit.Hint(new Vector3(-59f, 10.5f, 2f), new Vector3(8f, 4f, 12f), "Gust!",
                "Too far to glide! Jump, glide, then tap {Ability} for a burst of speed.", HintTheme.Wind);

            // Secret 1: a side updraft to the humming spool tower (golden button).
            kit.Updraft(b2 + new Vector3(3f, 0f, -5f), 15f);
            kit.SpoolTower(new Vector3(-46f, -6f, -13f), 19f, SpoolThreads);
            var secret = kit.GoldenButton(CollectibleIds[2], new Vector3(-46f, 14.3f, -13f));
            secret.Hums = true;
            kit.Thread(b2 + new Vector3(3f, 7f, -5f), LevelKit.Palette.Cream);

            // Secret 2: an updraft on the windmill bluff to a floating cloud patch (Heartstring).
            kit.Updraft(new Vector3(-76.5f, 9f, -2.5f), 12f);
            Vector3 cloud = new Vector3(-73f, 16.5f, -13f);
            kit.Prism(Octagon(cloud, 4f), cloud.y - 1f, cloud.y, PatchworkAssets.Felt(new Color(0.97f, 0.95f, 0.9f)), "CloudPatch");
            kit.Cloud(cloud + new Vector3(0f, -1.2f, 0f), 2.6f);
            kit.Cloud(cloud + new Vector3(2.5f, -0.8f, 2f), 1.6f);
            MakeHeartstring(kit, "L1_hs_cloud", "Cloud Patch", cloud + new Vector3(0f, 1.6f, 0f), false);
            MakeHomeHoop(kit, cloud + new Vector3(2f, 0f, -2f));
        }

        // A tall cardboard-and-felt windmill with spinning patchwork sails.
        static void Windmill(LevelKit kit, Vector3 feet)
        {
            kit.Deco(PrimitiveType.Cylinder, feet + Vector3.up * 3f, new Vector3(2.4f, 3f, 2.4f), PatchworkAssets.Felt(new Color(0.95f, 0.9f, 0.8f)));
            kit.Deco(PrimitiveType.Sphere, feet + Vector3.up * 6.2f, new Vector3(2.6f, 1.6f, 2.6f), PatchworkAssets.Felt(LevelKit.Palette.Rose));
            var col = kit.Group("WindmillCollider", feet + Vector3.up * 3f).gameObject.AddComponent<CapsuleCollider>();
            col.radius = 1.2f;
            col.height = 6f;
            var hub = kit.Group("Sails", feet + new Vector3(1.35f, 5f, 0f));
            hub.rotation = Quaternion.Euler(0f, 90f, 0f);
            kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.6f, PatchworkAssets.Felt(LevelKit.Palette.Gold), default(Vector3), hub);
            Color[] sail = { LevelKit.Palette.Sky, LevelKit.Palette.Butter, LevelKit.Palette.Lavender, LevelKit.Palette.Sage };
            for (int i = 0; i < 4; i++)
            {
                var s = kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(1f, 3.4f, 0.06f), PatchworkAssets.Fabric(sail[i]), new Vector3(0f, 0f, i * 90f), hub);
                s.localPosition = Quaternion.Euler(0f, 0f, i * 90f) * new Vector3(0.25f, 1.9f, 0f);
            }
            var spin = hub.gameObject.AddComponent<Spinner>();
            spin.Axis = Vector3.forward;
            spin.Speed = 70f;
        }

        // ================================================================ South: Spoolwork Ruins

        static void SpoolworkRuins(LevelKit kit)
        {
            var region = MakeRegion(kit, "Ruins", "L1_hs_loom");
            Vector3 o = South;
            // Landing (z -27..-35).
            kit.Quilt(o + new Vector3(0f, -0.5f, 0f), new Vector3(14f, 1f, 8f), Ruins, 3.5f, "RuinsLanding");
            kit.AddCheckpoint(o + new Vector3(0f, 0f, 2f), 180f);
            kit.Hint(o + new Vector3(0f, 1.5f, 1f), new Vector3(14f, 4f, 5f), "Spoolwork Ruins",
                "The old loom works. Ride the sliding buttons across, and watch out for Snarlings!");
            FrayTear(kit, region, o + new Vector3(4.5f, 0f, -1.5f), 2f);
            Blooms(kit, region, o, new Vector2(6f, 2.5f), 7);

            // Sliding buttons over the pit (z -35..-47).
            kit.MovingButton(new Vector3(-2.5f, 0f, -37.4f), new Vector3(-2.5f, 0f, -44.6f), 0f, new Color(0.95f, 0.5f, 0.55f));
            kit.MovingButton(new Vector3(2.5f, 0f, -37.4f), new Vector3(2.5f, 0f, -44.6f), 0.5f, new Color(0.5f, 0.65f, 0.95f));
            kit.ThreadLine(new Vector3(-2.5f, 1.2f, -38.5f), new Vector3(-2.5f, 1.2f, -43.5f), 3);
            kit.ThreadLine(new Vector3(2.5f, 1.2f, -38.5f), new Vector3(2.5f, 1.2f, -43.5f), 3);
            kit.SewingPin(new Vector3(-8f, -6f, -41f), 9f, LevelKit.Palette.Rose, new Vector3(10f, 0f, -18f));
            kit.SewingPin(new Vector3(8f, -6f, -42f), 9f, LevelKit.Palette.Gold, new Vector3(-10f, 0f, 18f));

            // The arena (z -47..-65): three Snarlings to unravel.
            kit.Quilt(new Vector3(0f, -0.5f, -56f), new Vector3(18f, 1f, 18f), Ruins, 3f, "SnarlingArena");
            kit.AddCheckpoint(new Vector3(0f, 0f, -48.5f), 180f);
            for (int i = 0; i < 8; i++)
            {
                float x = -8.5f + (i % 2) * 17f;
                float z = -49f - (i / 2) * 5f;
                kit.Spool(new Vector3(x, 0f, z), 1.2f, SpoolThreads[i % SpoolThreads.Length]);
            }
            var foes = new List<Snarling>
            {
                Snarling.Create(kit.Root, new Vector3(-5f, 0f, -52f), new Vector3(5f, 0f, -52f)),
                Snarling.Create(kit.Root, new Vector3(5f, 0f, -57f), new Vector3(-5f, 0f, -57f)),
                Snarling.Create(kit.Root, new Vector3(-4f, 0f, -61.5f), new Vector3(4f, 0f, -61.5f))
            };
            var tangle = MakeHeartstring(kit, "L1_hs_tangle", "Tangle Trouble", new Vector3(0f, 1.6f, -56f), true);
            Challenge(foes, tangle);
            kit.Hint(new Vector3(0f, 1.5f, -50f), new Vector3(16f, 4f, 3f), "Snarlings!",
                "Little creatures of the Fray. Jump on their heads to unravel them. If they bump you, they'll push you away.");

            // The back wall, with a drain at the bottom: only Water gets into the loom courtyard.
            var wicker = PatchworkAssets.Wicker(new Color(0.78f, 0.58f, 0.36f));
            kit.Block(new Vector3(-10.8f, 5f, -66f), new Vector3(20f, 10f, 2f), LevelKit.Palette.Walnut, "LoomWall", wicker);
            kit.Block(new Vector3(10.8f, 5f, -66f), new Vector3(20f, 10f, 2f), LevelKit.Palette.Walnut, "LoomWall", wicker);
            kit.Block(new Vector3(0f, 5.5f, -66f), new Vector3(1.6f, 9f, 2f), LevelKit.Palette.Walnut, "LoomWallTop", wicker);
            kit.Grate(new Vector3(0f, 0.5f, -64.9f), new Vector3(1.6f, 1f, 0.2f));
            kit.Hint(new Vector3(0f, 1.5f, -62.5f), new Vector3(6f, 4f, 3f), "A Drain in the Wall",
                "Something's humming on the other side. Only a little bubble of Water could squeeze through here.", HintTheme.Water);

            // The courtyard (z -67..-81): a fountain up the old loom to its Heartstring.
            kit.Quilt(new Vector3(0f, -0.5f, -74f), new Vector3(20f, 1f, 14f), Ruins, 3f, "LoomCourtyard");
            kit.Block(new Vector3(-10.5f, 5f, -74f), new Vector3(1f, 10f, 14f), LevelKit.Palette.Walnut, "CourtyardWall", wicker);
            kit.Block(new Vector3(10.5f, 5f, -74f), new Vector3(1f, 10f, 14f), LevelKit.Palette.Walnut, "CourtyardWall", wicker);
            kit.AddCheckpoint(new Vector3(0f, 0f, -68.5f), 180f);
            kit.Fountain(new Vector3(0f, 0f, -73f), 12f);
            kit.Block(new Vector3(0f, 5f, -77.2f), new Vector3(5f, 10f, 4f), new Color(0.6f, 0.45f, 0.32f), "LoomTower", PatchworkAssets.Wood(new Color(0.65f, 0.5f, 0.35f)));
            Loom(kit, new Vector3(0f, 10f, -79.2f));
            MakeHeartstring(kit, "L1_hs_loom", "Loom Heart", new Vector3(0f, 11.6f, -76.6f), false);
            MakeHomeHoop(kit, new Vector3(1.6f, 10f, -76.2f));
            kit.AddCameraZone(new Vector3(0f, 6f, -74f), new Vector3(20f, 16f, 14f), 2f, 8f);
            FrayTendril(kit, region, new Vector3(-6f, 0f, -77f), 3.5f);
            FrayTendril(kit, region, new Vector3(6.5f, 0f, -71f), 3f);
            FrayTear(kit, region, new Vector3(-4f, 0f, -70f), 2.2f);
            Blooms(kit, region, new Vector3(0f, 0f, -71f), new Vector2(8f, 2.5f), 10);
            kit.ThreadLine(new Vector3(-6f, 1f, -70f), new Vector3(6f, 1f, -70f), 4);
        }

        // An old wooden loom frame with yarn warp threads, on top of the tower.
        static void Loom(LevelKit kit, Vector3 baseCenter)
        {
            var wood = PatchworkAssets.Wood(new Color(0.7f, 0.52f, 0.36f));
            kit.Deco(PrimitiveType.Cube, baseCenter + new Vector3(-2.3f, 2.5f, 0f), new Vector3(0.35f, 5f, 0.35f), wood);
            kit.Deco(PrimitiveType.Cube, baseCenter + new Vector3(2.3f, 2.5f, 0f), new Vector3(0.35f, 5f, 0.35f), wood);
            kit.Deco(PrimitiveType.Cube, baseCenter + new Vector3(0f, 4.9f, 0f), new Vector3(5f, 0.35f, 0.35f), wood);
            kit.Deco(PrimitiveType.Cube, baseCenter + new Vector3(0f, 0.6f, 0f), new Vector3(5f, 0.3f, 0.3f), wood);
            for (int i = 0; i < 9; i++)
            {
                float x = -1.8f + i * 0.45f;
                kit.Deco(PrimitiveType.Cylinder, baseCenter + new Vector3(x, 2.75f, 0f), new Vector3(0.06f, 2.1f, 0.06f), PatchworkAssets.Yarn(SpoolThreads[i % SpoolThreads.Length]));
            }
        }

        // ================================================================ Sky: the Fray's Knot (finale)

        static void FraysKnot(LevelKit kit)
        {
            Vector3 c = SkyIsland;
            kit.Prism(Octagon(c, 8f), c.y - 1.5f, c.y, PatchworkAssets.Fabric(new Color(0.96f, 0.93f, 0.88f)), "SkyIsland");
            kit.Cloud(c + new Vector3(0f, -2.4f, 0f), 3.5f);
            kit.Cloud(c + new Vector3(-5f, -2f, 3f), 2.2f);
            kit.Cloud(c + new Vector3(5f, -2.2f, -2f), 2.4f);
            kit.AddCheckpoint(c + new Vector3(0f, 0f, -5.5f), 0f);
            kit.AddCameraZone(c + new Vector3(0f, 3f, 0f), new Vector3(18f, 12f, 18f), 3f, 8f);
            kit.Hint(c + new Vector3(0f, 1.5f, -5f), new Vector3(12f, 4f, 4f), "The Fray's Knot",
                "This is where the Fray tied the garden up. Pop the four knots holding it together!");

            // The knot itself, floating over the middle of the island.
            Vector3 corePos = c + new Vector3(0f, 6f, 2f);
            var core = kit.Group("FrayCore", corePos);
            KnotShape(kit, core, 3.2f, 26);
            var lightGo = new GameObject("CoreLight");
            lightGo.transform.SetParent(core, false);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = SnarlKnot.FrayGlow;
            light.range = 14f;
            light.intensity = 2f;
            light.shadows = LightShadows.None;
            director.Core = core;

            // Four knots holding it, each tied to it by a dark thread.
            Vector3[] spots = { new Vector3(4.5f, 0.9f, 2f), new Vector3(-4.5f, 0.9f, 2f), new Vector3(0f, 0.9f, 6.5f), new Vector3(0f, 0.9f, -2.5f) };
            var tether = PatchworkAssets.Yarn(SnarlKnot.FrayDark);
            foreach (var s in spots)
            {
                var k = MakeKnot(kit, c + s, 1.1f);
                director.CoreShell.Add(k);
                director.CoreTethers.Add(kit.Rope(c + s, corePos, tether, 0.12f));
            }

            // The garden's heart, freed when the knot breaks. Touch it to finish the level.
            var heart = kit.Group("GardenHeart", c + new Vector3(0f, 2.4f, 2f));
            HeartShape(kit, heart, 2f, PatchworkAssets.Glow(new Color(1f, 0.5f, 0.6f), 0.5f));
            director.GardenHeart = heart;
            var goal = kit.Volume<GoalZone>(c + new Vector3(0f, 1.5f, 2f), new Vector3(3.5f, 3f, 3.5f), "HeartGoal");
            director.CoreGoal = goal.gameObject;
        }

        // ================================================================ The paint lake

        static void BuildPaintLake(LevelKit kit)
        {
            kit.PaintLake(new Vector3(0f, -8f, 0f), new Vector2(168f, 168f), 13f);
            // Art supplies bobbing in the paint, scattered around the garden.
            Color[] paints = { new Color(0.95f, 0.45f, 0.55f), new Color(0.98f, 0.82f, 0.35f), new Color(0.4f, 0.75f, 0.6f), new Color(0.6f, 0.5f, 0.9f), new Color(0.35f, 0.6f, 0.95f), new Color(0.98f, 0.6f, 0.3f) };
            for (int i = 0; i < 18; i++)
            {
                float a = (i + 0.3f) / 18f * Mathf.PI * 2f;
                float r = 24f + (i * 13) % 50;
                Vector3 p = new Vector3(Mathf.Cos(a) * r, -8f, Mathf.Sin(a) * r);
                switch (i % 3)
                {
                    case 0: PaintTube(kit, p, paints[i % paints.Length], i * 37f); break;
                    case 1: InkBottle(kit, p, (i % 5) * 8f - 16f); break;
                    default: Brush(kit, p, paints[(i + 2) % paints.Length], i * 53f); break;
                }
            }
        }

        // A squeezed tube of paint floating on its side, with a blob coming out of it.
        static void PaintTube(LevelKit kit, Vector3 pos, Color paint, float yaw)
        {
            var g = kit.Group("PaintTube", pos + Vector3.up * 0.5f);
            g.rotation = Quaternion.Euler(0f, yaw, 0f);
            var metal = PatchworkAssets.Plain(new Color(0.88f, 0.88f, 0.92f), 0.85f);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.3f, 2f, 1.1f), metal, new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.36f, 1.1f, 1.16f), PatchworkAssets.Plain(paint, 0.4f), new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(-2.1f, 0f, 0f), new Vector3(0.3f, 0.3f, 1.4f), metal, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(2.15f, 0f, 0f), new Vector3(0.55f, 0.25f, 0.55f), PatchworkAssets.Plain(new Color(0.15f, 0.12f, 0.15f), 0.4f), new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(2.9f, -0.2f, 0f), new Vector3(1.4f, 0.5f, 1.1f), PatchworkAssets.Plain(paint, 0.9f), default(Vector3), g);
            var bob = g.gameObject.AddComponent<Spinner>();
            bob.Speed = 2f;
            bob.BobHeight = 0.18f;
            bob.BobSpeed = 0.7f;
        }

        // A bottle of ink, tilted and half sunk.
        static void InkBottle(LevelKit kit, Vector3 pos, float tilt)
        {
            var g = kit.Group("InkBottle", pos);
            g.rotation = Quaternion.Euler(tilt, tilt * 2f, tilt * 0.5f);
            var glass = PatchworkAssets.Plain(new Color(0.12f, 0.13f, 0.3f), 0.95f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.6f, 0f), new Vector3(2.4f, 1.6f, 2.4f), glass, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 2.5f, 0f), new Vector3(1.1f, 0.4f, 1.1f), glass, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 3.05f, 0f), new Vector3(0.95f, 0.25f, 0.95f), PatchworkAssets.Felt(new Color(0.65f, 0.48f, 0.32f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1f, 0f), new Vector3(2.46f, 0.5f, 2.46f), PatchworkAssets.Felt(LevelKit.Palette.Cream), default(Vector3), g);
            var bob = g.gameObject.AddComponent<Spinner>();
            bob.Speed = 0f;
            bob.BobHeight = 0.12f;
            bob.BobSpeed = 0.5f;
        }

        // A big paintbrush sticking up out of the paint at an angle.
        static void Brush(LevelKit kit, Vector3 pos, Color paint, float yaw)
        {
            var g = kit.Group("Paintbrush", pos);
            g.rotation = Quaternion.Euler(0f, yaw, 0f) * Quaternion.Euler(0f, 0f, 30f);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 4f, 0f), new Vector3(0.45f, 3.5f, 0.45f), PatchworkAssets.Plain(new Color(0.85f, 0.3f, 0.3f), 0.6f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.3f, 0f), new Vector3(0.6f, 0.6f, 0.6f), PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.9f), 0.9f), default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, -0.4f, 0f), new Vector3(0.7f, 1.2f, 0.7f), PatchworkAssets.Plain(paint, 0.85f), default(Vector3), g);
        }

        // A tiny stitched hut on the fountain shelf. Its only door is a drain grate, so you have to
        // still be Water when you reach the top. The front wall faces -z at frontBase.z.
        static void DrainHut(LevelKit kit, Vector3 frontBase)
        {
            float x = frontBase.x, y = frontBase.y, z = frontBase.z;
            var felt = LevelKit.Palette.Peach;
            kit.Block(new Vector3(x - 1.35f, y + 1.25f, z), new Vector3(1.3f, 2.5f, 0.4f), felt, "HutFrontL");
            kit.Block(new Vector3(x + 1.35f, y + 1.25f, z), new Vector3(1.3f, 2.5f, 0.4f), felt, "HutFrontR");
            kit.Block(new Vector3(x, y + 1.75f, z), new Vector3(1.4f, 1.5f, 0.4f), felt, "HutFrontTop");
            kit.Grate(new Vector3(x, y + 0.5f, z), new Vector3(1.4f, 1f, 0.2f));
            kit.Block(new Vector3(x - 1.8f, y + 1.25f, z + 1.9f), new Vector3(0.4f, 2.5f, 3.8f), felt, "HutSide");
            kit.Block(new Vector3(x + 1.8f, y + 1.25f, z + 1.9f), new Vector3(0.4f, 2.5f, 3.8f), felt, "HutSide");
            kit.Block(new Vector3(x, y + 1.25f, z + 3.6f), new Vector3(3.6f, 2.5f, 0.4f), felt, "HutBack");
            kit.Block(new Vector3(x, y + 2.7f, z + 1.8f), new Vector3(4.4f, 0.4f, 4.4f), LevelKit.Palette.Rose, "HutRoof");
        }


        // ================================================================ Yarn birds

        static void YarnBirds(LevelKit kit)
        {
            Color[] c = { LevelKit.Palette.Rose, LevelKit.Palette.Sky, LevelKit.Palette.Butter, LevelKit.Palette.Lavender, LevelKit.Palette.Peach };
            // The square
            kit.YarnBird(new Vector3(5f, 0f, -12f), c[0], false);
            kit.YarnBird(new Vector3(-6f, 0f, 9f), c[1], false);
            kit.YarnBird(new Vector3(0f, 8.6f, 0f), c[2], false);
            kit.YarnBird(new Vector3(0f, 12f, 0f), c[3], true, 9f);
            kit.YarnBird(new Vector3(1.5f, 5.35f, 17f), c[4], false);
            // Peaks
            kit.YarnBird(new Vector3(3f, 4f, 66f), c[0], false);
            kit.YarnBird(new Vector3(0f, 14f, 58f), c[1], true, 7f);
            // Dye Falls
            kit.YarnBird(new Vector3(48f, 8f, 1f), c[2], false);
            kit.YarnBird(new Vector3(44f, 9f, -5f), c[3], true, 8f);
            kit.YarnBird(new Vector3(40f, 0f, -2f), c[4], false);
            // Bluffs, and one on the spool tower beside the secret button
            kit.YarnBird(new Vector3(-58f, 9f, -2f), c[0], false);
            kit.YarnBird(new Vector3(-46.8f, 13f, -12.2f), c[1], false);
            kit.YarnBird(new Vector3(-62f, 15f, 2f), c[2], true, 9f);
            // Ruins
            kit.YarnBird(new Vector3(-8.5f, 1.2f, -49f), c[3], false);
            kit.YarnBird(new Vector3(0f, 10f, -56f), c[4], true, 7f);
            // Way up high, near the knot
            kit.YarnBird(new Vector3(0f, 34f, 11f), c[0], true, 12f);
        }
    }
}

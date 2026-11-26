using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Life in the Common Quarter and on the road in: the toys who live and work there, the
    // working quay (a treadwheel crane, a moored barge, bollards and cargo), market clutter in
    // the squares, and a few things along the King's Road and the river.
    public static partial class Level1Builder
    {
        static void CityLife(LevelKit kit)
        {
            // ---- The folk of the quarter.
            var warden = Toy(kit, new Vector3(61.6f, 0.06f, -64.2f), 205f, ToyNpc.Kind.TinSoldier, "Warden Pewter", new Color(0.3f, 0.42f, 0.7f), LevelKit.Palette.Gold, ToyNpc.Instrument.None, 0.78f, new Color(0.28f, 0.38f, 0.62f));
            warden.Lines = () => GameState.IsLevelComplete(SceneName)
                ? new[] { "At ease! The wedding's done and the Fray's gone. Best day on the gate I've ever had.", "Go on up the High Street, little one. The whole town's out." }
                : new[] { "Halt! Who goes th... oh! You're the little patchwork one from the Pod. Welcome to Toybox Town!",
                          "The wedding's over the canal in Wedding Square. Up the High Street, left down Mill Street to the quay, and over the big bridge.",
                          "The Fray's been at the bunting and the band both. If you see a loose thread, give it a tug for us." };
            var marigold = Toy(kit, new Vector3(66.1f, 0.06f, -58.2f), -120f, ToyNpc.Kind.RagDoll, "Mistress Marigold", new Color(0.92f, 0.62f, 0.3f), new Color(0.45f, 0.62f, 0.38f), ToyNpc.Instrument.None, 1.1f, new Color(0.8f, 0.5f, 0.25f));
            marigold.Lines = () => new[] { "Buttons! Bobbins! Felt cabbages, fresh this morning! Ooh, sorry dear, force of habit.",
                                           "Market day's every day in Gate Square. Everyone coming in from the fields passes my stall first.",
                                           "If you're after a view, the bell tower at St. Stitch's has the best one. Just don't tell Brother Bellweather I said you could climb it." };
            var bell = Toy(kit, new Vector3(Chapel.x - 1.4f, 0.06f, (Chapel.y + 7f + Chapel.w) * 0.5f + 2.6f), -90f, ToyNpc.Kind.Nutcracker, "Brother Bellweather", new Color(0.5f, 0.42f, 0.36f), new Color(0.9f, 0.86f, 0.78f), ToyNpc.Instrument.None, 0.9f, new Color(0.45f, 0.38f, 0.32f));
            bell.Lines = () => Level1Director.BandCount() >= 3
                ? new[] { "Is that the band I hear tuning up? Then I'd better get to my bells. A wedding needs bells!" }
                : new[] { "Peace be with your stitches, little one. Welcome to St. Stitch's.",
                          "I ring the bells for every wedding in town. But a wedding needs its band before it needs its bells." };
            var gaunt = Toy(kit, new Vector3(Bailey.z + 2.6f, 0.06f, (Bailey.y + Bailey.w) * 0.5f + 3.4f), 90f, ToyNpc.Kind.TinSoldier, "Sentry Gauntlet", new Color(0.72f, 0.24f, 0.26f), new Color(0.85f, 0.86f, 0.9f), ToyNpc.Instrument.None, 0.7f, new Color(0.62f, 0.2f, 0.22f));
            gaunt.Lines = () => new[] { "This is the keep. The King of the Toybox built it, back before anyone can remember.",
                                        "Nobody goes up while the Fray's about. Orders. But you can see the flag from anywhere in the box.",
                                        "Want my advice? Watch the sky over the north wall. That's where the Fray came from." };
            var wick = Toy(kit, new Vector3(45.6f, 0.06f, 89.2f), -90f, ToyNpc.Kind.Teddy, "Crane-keeper Wick", new Color(0.55f, 0.45f, 0.38f), new Color(0.3f, 0.45f, 0.65f), ToyNpc.Instrument.None, 0.85f, new Color(0.5f, 0.4f, 0.32f));
            wick.Lines = () => new[] { "Mind the rope! Every bale of felt that comes up the canal comes off the barges with this old crane.",
                                       "Two of us walk the wheel when it's busy. Up, up, up she goes!",
                                       "The canal runs right out under the north wall to the River Weft. Only Water could swim that far, mind." };

            // ---- The quay: a treadwheel crane, a moored barge, bollards and cargo.
            QuayCrane(kit, new Vector3(41.8f, 0.06f, 84f));
            Barge(kit, new Vector3(36.4f, CanalSurface + 0.05f, 96f));
            var dark = PatchworkAssets.Plain(new Color(0.25f, 0.22f, 0.22f), 0.4f);
            var rope = PatchworkAssets.Yarn(new Color(0.85f, 0.75f, 0.55f));
            foreach (float z in new[] { -69f, -64f, 64f, 72f, 92f, 100f, 106f })
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(40.5f, 0.4f, z), new Vector3(0.45f, 0.35f, 0.45f), dark);
                kit.Deco(PrimitiveType.Sphere, new Vector3(40.5f, 0.78f, z), new Vector3(0.55f, 0.25f, 0.55f), dark);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(40.5f, 0.5f, z), new Vector3(0.52f, 0.06f, 0.52f), rope);
            }
            for (int i = 0; i < 3; i++)
            {
                TownKit.Crate(kit, new Vector3(45.6f, 0.06f, 79.6f + i * 1.15f), 1f, i * 17f);
                if (i < 2) TownKit.Crate(kit, new Vector3(45.6f, 1.06f, 80.2f + i * 1.15f), 0.9f, i * 29f + 8f);
            }
            Barrel(kit, new Vector3(45.8f, 0.06f, 93f), 0f);
            Barrel(kit, new Vector3(46f, 0.06f, 94.2f), 30f);

            // ---- Market clutter in the squares, and a cart in Mill Square.
            var mill = CitySquares[1];
            TownKit.ToyCart(kit, new Vector3(mill.x + 4.4f, 0.06f, mill.y + 4.2f), 90f);
            Barrel(kit, new Vector3(mill.x - 4.6f, 0.06f, mill.y - 3.4f), 10f);
            var gate = CitySquares[0];
            for (int i = 0; i < 4; i++)
                TownKit.Crate(kit, new Vector3(gate.x - 6.2f + (i % 2) * 1.1f, 0.06f + (i / 2) * 1f, gate.y + 1.4f), 1f, i * 23f);
            // Flower tubs by the chapel door.
            for (int s = -1; s <= 1; s += 2)
            {
                var tub = new Vector3(Chapel.x - 1.1f, 0.06f, (Chapel.y + 7f + Chapel.w) * 0.5f + s * 2.4f);
                kit.Deco(PrimitiveType.Cylinder, tub + Vector3.up * 0.3f, new Vector3(0.9f, 0.3f, 0.9f), PatchworkAssets.Planks(new Color(0.6f, 0.4f, 0.26f)));
                for (int f = 0; f < 3; f++) kit.Flower(tub + new Vector3(Mathf.Cos(f * 2.1f) * 0.25f, 0.55f, Mathf.Sin(f * 2.1f) * 0.25f), FlowerColors[(f + (s > 0 ? 2 : 0)) % FlowerColors.Length]);
            }

            // ---- Banners hung out from the High Street's upper floors (every few houses).
            Color[] banner = { new Color(0.82f, 0.3f, 0.3f), new Color(0.3f, 0.45f, 0.72f), LevelKit.Palette.Butter };
            int hung = 0;
            for (int i = 0; i + 7 < CityLots.Length; i += 8)
            {
                if (CityLots[i + 4] < 8.9f || (i / 8) % 2 == 1) continue;
                float yaw = CityLots[i + 5];
                var rot = Quaternion.Euler(0f, yaw, 0f);
                var at = new Vector3(CityLots[i], 0f, CityLots[i + 1]) + rot * new Vector3(0f, 0f, CityLots[i + 3] * 0.5f + 0.35f);
                var g = kit.Group("HouseBanner", at + Vector3.up * 6.8f);
                g.rotation = rot;
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.4f), new Vector3(0.06f, 0.5f, 0.06f), PatchworkAssets.Wood(TownKit.DarkWood), new Vector3(90f, 0f, 0f), g);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, -1f, 0.8f), new Vector3(0.05f, 2f, 0.7f), PatchworkAssets.Felt(banner[hung % banner.Length]), default(Vector3), g);
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, -2.1f, 0.8f), new Vector3(0.05f, 0.4f, 0.5f), PatchworkAssets.Felt(LevelKit.Palette.Gold), new Vector3(45f, 0f, 0f), g);
                hung++;
            }
        }

        // A wooden treadwheel crane: a timber tower on the quay, the big wheel two toys walk
        // inside to turn it, and a jib out over the water with a bale hanging from its rope.
        static void QuayCrane(LevelKit kit, Vector3 feet)
        {
            var wood = PatchworkAssets.Wood(new Color(0.62f, 0.44f, 0.3f));
            var darkWood = PatchworkAssets.Wood(TownKit.DarkWood);
            var g = kit.Group("TreadwheelCrane", feet);
            // The tower: four posts, braces and a little shingle roof.
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    kit.Deco(PrimitiveType.Cube, new Vector3(sx * 1.1f, 3.2f, sz * 1.1f), new Vector3(0.3f, 6.4f, 0.3f), darkWood, default(Vector3), g);
            for (int y = 0; y < 3; y++)
                for (int s = -1; s <= 1; s += 2)
                {
                    kit.Deco(PrimitiveType.Cube, new Vector3(s * 1.1f, 1.2f + y * 2.2f, 0f), new Vector3(0.2f, 0.2f, 2.4f), darkWood, default(Vector3), g);
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, 1.2f + y * 2.2f, s * 1.1f), new Vector3(2.4f, 0.2f, 0.2f), darkWood, default(Vector3), g);
                }
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 6.5f, 0f), new Vector3(2.8f, 0.2f, 2.8f), wood, default(Vector3), g);
            var roof = TownKit.GableRoof(kit, feet + Vector3.up * 6.6f, 3.2f, 3f, 1.4f, 0f, PatchworkAssets.Shingle(new Color(0.45f, 0.36f, 0.3f)), wood);
            if (roof != null) roof.SetParent(g, true);
            // The treadwheel, beside the tower along the quay.
            var wheel = kit.Group("Treadwheel", feet + new Vector3(0.6f, 2.3f, 2.6f));
            wheel.SetParent(g, true);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0f, s * 0.9f), new Vector3(4.4f, 0.06f, 4.4f), wood, new Vector3(90f, 0f, 0f), wheel);
                for (int k = 0; k < 6; k++)
                    kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0f, s * 0.92f), new Vector3(4.2f, 0.12f, 0.06f), darkWood, new Vector3(0f, 0f, k * 30f), wheel);
            }
            for (int k = 0; k < 16; k++)
            {
                float a = k * Mathf.PI * 2f / 16f;
                kit.Deco(PrimitiveType.Cube, new Vector3(Mathf.Cos(a) * 2.1f, Mathf.Sin(a) * 2.1f, 0f), new Vector3(0.22f, 0.22f, 1.9f), wood, new Vector3(0f, 0f, a * Mathf.Rad2Deg), wheel);
            }
            kit.Deco(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.3f, 1.1f, 0.3f), darkWood, new Vector3(90f, 0f, 0f), wheel);
            var spin = wheel.gameObject.AddComponent<Spinner>();
            spin.Axis = Vector3.forward;
            spin.Speed = 9f;
            // The jib, out over the canal, with its rope and a bale of felt.
            kit.Deco(PrimitiveType.Cube, new Vector3(-2.6f, 5.9f, 0f), new Vector3(5.4f, 0.3f, 0.3f), darkWood, new Vector3(0f, 0f, -8f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(-1.4f, 4.9f, 0f), new Vector3(2.8f, 0.18f, 0.18f), darkWood, new Vector3(0f, 0f, 35f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-5f, 4.5f, 0f), new Vector3(0.05f, 1.4f, 0.05f), PatchworkAssets.Yarn(new Color(0.85f, 0.75f, 0.55f)), default(Vector3), g);
            var bale = kit.Group("Bale", feet + new Vector3(-5f, 2.6f, 0f));
            bale.SetParent(g, true);
            kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(1.1f, 0.8f, 0.9f), PatchworkAssets.Felt(new Color(0.55f, 0.35f, 0.6f)), default(Vector3), bale);
            kit.Deco(PrimitiveType.Cube, Vector3.zero, new Vector3(1.14f, 0.12f, 0.94f), PatchworkAssets.Yarn(new Color(0.85f, 0.75f, 0.55f)), default(Vector3), bale);
            var sway = bale.gameObject.AddComponent<Rocker>();
            sway.Amount = 4f;
            sway.Speed = 0.8f;
            TownKit.Solid(kit, feet + Vector3.up * 1.6f, new Vector3(2.6f, 3.2f, 2.6f), 0f);
            TownKit.Solid(kit, feet + new Vector3(0.6f, 2.3f, 2.6f), new Vector3(4.4f, 4.6f, 2f), 0f);
            kit.Info.MapPins.Add(new MapPin("Treadwheel Crane", feet));
        }

        // A flat-bottomed canal barge tied up at the quay, loaded with bales, barrels and a
        // little tarpaulin shelter at the stern.
        static void Barge(LevelKit kit, Vector3 at)
        {
            var hull = PatchworkAssets.Planks(new Color(0.36f, 0.5f, 0.42f));
            var trim = PatchworkAssets.Planks(new Color(0.85f, 0.72f, 0.36f));
            var g = kit.Group("Barge", at);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.2f, 0f), new Vector3(2.6f, 0.7f, 8.4f), hull, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.58f, 0f), new Vector3(2.75f, 0.1f, 8.6f), trim, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.2f, 4.2f), new Vector3(2.6f, 0.35f, 1.6f), hull, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.2f, -4.2f), new Vector3(2.6f, 0.35f, 1.6f), hull, default(Vector3), g);
            Color[] bales = { new Color(0.55f, 0.35f, 0.6f), new Color(0.85f, 0.4f, 0.35f), new Color(0.4f, 0.55f, 0.8f) };
            for (int i = 0; i < 5; i++)
                kit.Deco(PrimitiveType.Cube, new Vector3((i % 2) * 0.9f - 0.45f, 0.95f + (i / 4) * 0.7f, -2.4f + i * 0.95f), new Vector3(0.9f, 0.7f, 0.85f), PatchworkAssets.Felt(bales[i % bales.Length]), new Vector3(0f, i * 9f, 0f), g);
            for (int i = 0; i < 2; i++)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0.6f - i * 1.2f, 1f, 2.6f), new Vector3(0.7f, 0.42f, 0.7f), PatchworkAssets.Planks(new Color(0.6f, 0.4f, 0.26f)), default(Vector3), g);
            // The stern shelter.
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, new Vector3(s * 1.1f, 1.3f, -3.4f), new Vector3(0.1f, 1.4f, 0.1f), PatchworkAssets.Wood(TownKit.DarkWood), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 2f, -3.4f), new Vector3(2.5f, 0.06f, 1.6f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.95f, 0.92f, 0.85f), new Color(0.82f, 0.3f, 0.3f)), new Vector3(0f, 0f, 6f), g);
            // Tied to the bollards.
            var rope = PatchworkAssets.Yarn(new Color(0.85f, 0.75f, 0.55f));
            kit.Deco(PrimitiveType.Cube, new Vector3(2.2f, 0.7f, 3f), new Vector3(2.4f, 0.06f, 0.06f), rope, new Vector3(0f, 18f, -10f), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(2.2f, 0.7f, -3f), new Vector3(2.4f, 0.06f, 0.06f), rope, new Vector3(0f, -18f, -10f), g);
            var bob = g.gameObject.AddComponent<Rocker>();
            bob.Amount = 1.5f;
            bob.Speed = 0.5f;
        }

        // Along the King's Road: a farmer by the river with his fishing jetty, a wayside shrine,
        // a hay cart, and ducks on the river.
        static void ApproachLife(LevelKit kit, Vector2[] road, Vector2 bridge, Vector2 bridgeDir)
        {
            int mask = ~(1 << 2);
            // The jetty: just downstream of the bridge, on the town side.
            Vector2 side = new Vector2(-bridgeDir.y, bridgeDir.x);
            Vector2 jetty = bridge + side * 15f + bridgeDir * 7f;
            int ni = 1;
            for (int i = 1; i < RiverSouth.Length - 1; i++)
                if ((RiverSouth[i] - jetty).sqrMagnitude < (RiverSouth[ni] - jetty).sqrMagnitude) ni = i;
            Vector2 near = RiverSouth[ni];
            float t;
            PathDistance(near, RiverSouth, out t);
            Vector2 toWater = (near - jetty).normalized;
            float hw = RiverHalfWidth(t);
            Vector2 shore = near - toWater * (hw + 2.6f);
            if (!Physics.CheckSphere(new Vector3(shore.x, 1.2f, shore.y), 1f, mask, QueryTriggerInteraction.Ignore))
            {
                float yaw = Mathf.Atan2(toWater.x, toWater.y) * Mathf.Rad2Deg;
                var rot = Quaternion.Euler(0f, yaw, 0f);
                var deck = PatchworkAssets.Planks(new Color(0.62f, 0.46f, 0.32f));
                for (int i = 0; i < 6; i++)
                {
                    Vector3 p = new Vector3(shore.x, 0.18f, shore.y) + rot * new Vector3(0f, 0f, 0.4f + i * 0.85f);
                    var plank = kit.Block(p, new Vector3(1.8f, 0.14f, 0.8f), new Color(0.62f, 0.46f, 0.32f), "Jetty", deck);
                    plank.rotation = rot;
                }
                for (int s = -1; s <= 1; s += 2)
                    for (int k = 0; k < 2; k++)
                        kit.Deco(PrimitiveType.Cylinder, new Vector3(shore.x, -0.8f, shore.y) + rot * new Vector3(s * 0.85f, 0f, 1.6f + k * 2.8f), new Vector3(0.2f, 1.1f, 0.2f), PatchworkAssets.Wood(TownKit.DarkWood));
                Vector3 seat = new Vector3(shore.x, 0.25f, shore.y) + rot * new Vector3(0f, 0f, 4.4f);
                var tiller = Toy(kit, seat, yaw, ToyNpc.Kind.Teddy, "Old Tiller", new Color(0.7f, 0.52f, 0.32f), new Color(0.82f, 0.7f, 0.4f), ToyNpc.Instrument.None, 0.8f, new Color(0.6f, 0.45f, 0.28f));
                tiller.Lines = () => new[] { "Shh, you'll scare the fish. Not that there are any. They're all buttons in this river.",
                                             "That's Toybox Town up the road. Biggest town in the whole toybox. The keep's older than the wardrobe.",
                                             "The river's the Weft. It comes out of the canal under the town wall. Only a toy made of Water could swim it, mind." };
                // A rod out over the water.
                kit.Deco(PrimitiveType.Cylinder, seat + Vector3.up * 1.2f + rot * new Vector3(0.4f, 0f, 1.4f), new Vector3(0.05f, 1.5f, 0.05f), PatchworkAssets.Wood(new Color(0.7f, 0.55f, 0.35f)), (rot * Quaternion.Euler(55f, 0f, 0f)).eulerAngles);
            }
            // The wayside shrine, at the far end of the bridge: a little gabled box on a post
            // with a candle and a pair of flowers.
            Vector2 sh = bridge + bridgeDir * 15f - side * 4.6f;
            var shrine = new Vector3(sh.x, 0f, sh.y);
            if (!Physics.CheckSphere(shrine + Vector3.up, 0.8f, mask, QueryTriggerInteraction.Ignore) && RiverEdgeDistance(shrine) > 2f)
            {
                var stone = PatchworkAssets.Cobble(new Color(0.85f, 0.82f, 0.76f));
                kit.Block(shrine + Vector3.up * 0.8f, new Vector3(0.5f, 1.6f, 0.5f), TownKit.Stone, "Shrine", stone);
                kit.Deco(PrimitiveType.Cube, shrine + Vector3.up * 2f, new Vector3(0.9f, 0.8f, 0.7f), stone);
                TownKit.GableRoof(kit, shrine + Vector3.up * 2.4f, 1.1f, 0.9f, 0.5f, 0f, roofRed, stone);
                kit.Deco(PrimitiveType.Cylinder, shrine + new Vector3(0f, 2f, -0.36f), new Vector3(0.12f, 0.15f, 0.12f), PatchworkAssets.Felt(LevelKit.Palette.Cream));
                kit.Deco(PrimitiveType.Sphere, shrine + new Vector3(0f, 2.22f, -0.36f), new Vector3(0.08f, 0.12f, 0.08f), PatchworkAssets.Glow(new Color(1f, 0.8f, 0.4f), 0.9f));
                kit.Flower(shrine + new Vector3(0.6f, 0f, -0.4f), FlowerColors[0]);
                kit.Flower(shrine + new Vector3(-0.6f, 0f, -0.4f), FlowerColors[2]);
            }
            // A hay cart pulled onto the verge halfway up.
            Vector2 c = road[Mathf.RoundToInt(road.Length * 0.45f)];
            Vector2 cd = (road[Mathf.RoundToInt(road.Length * 0.45f) + 1] - c).normalized;
            Vector2 cq = c + new Vector2(-cd.y, cd.x) * 4.6f;
            var cart = new Vector3(cq.x, 0f, cq.y);
            if (!Physics.CheckSphere(cart + Vector3.up * 1.2f, 1.6f, mask, QueryTriggerInteraction.Ignore) && RiverEdgeDistance(cart) > 3f)
            {
                TownKit.ToyCart(kit, cart, 0f);
                for (int i = 0; i < 3; i++)
                    kit.Deco(PrimitiveType.Cylinder, cart + new Vector3(-0.6f + i * 0.6f, 1.6f, 0f), new Vector3(0.7f, 0.4f, 0.7f), PatchworkAssets.Felt(new Color(0.95f, 0.85f, 0.5f)), new Vector3(0f, 0f, 90f));
            }
            // Ducks paddling up and down the long bend of the river south of the orchard.
            int from = RiverSouth.Length / 2, to = Mathf.Min(RiverSouth.Length - 2, from + 18);
            var loop = new List<Vector3>();
            for (int i = from; i <= to; i++)
            {
                Vector2 d = (RiverSouth[i + 1] - RiverSouth[i - 1]).normalized, n = new Vector2(-d.y, d.x);
                Vector2 q = RiverSouth[i] + n * 1.6f;
                loop.Add(new Vector3(q.x, CanalSurface, q.y));
            }
            for (int i = to; i >= from; i--)
            {
                Vector2 d = (RiverSouth[i + 1] - RiverSouth[i - 1]).normalized, n = new Vector2(-d.y, d.x);
                Vector2 q = RiverSouth[i] - n * 1.6f;
                loop.Add(new Vector3(q.x, CanalSurface, q.y));
            }
            var pts = loop.ToArray();
            for (int i = 0; i < 3; i++) Duck(kit, pts, i * 23f, i == 0 ? 1.4f : 0.75f);
        }
    }
}

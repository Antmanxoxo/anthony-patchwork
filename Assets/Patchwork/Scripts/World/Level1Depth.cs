using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // More to each part of Toybox Town than the way through it: somewhere to climb, something
    // hidden, someone to talk to, and a reason to stop and look about.
    //   - the Masons' Scaffold: plank ramps zig-zagging up the keep's south face to a top deck
    //   - the Smugglers' Cellar: an old jam cellar under the quay, in through a bent grate in
    //     the canal (Water only), with Captain Treacle and his sweets
    //   - Hob's Burrow: a little round door in Thread Ravine, and the old hermit who used to
    //     mind the seams the Fray come through
    //   - the Bramble Maze: a tall hedge maze west of the barracks, a marble at its heart
    //   - the Squire's Trial: a timed run at the Lists, over hay bales and round the quintain
    //   - the windmill's gallery: plank steps round the back of the mill, clear of the sails
    //   - the Woodcutters' Camp in the west wood, with something buried by a ribboned stump
    //   - a trail of bubbles down the River Weft, for anyone who'd rather swim
    //   - the Lookout Oak by the Pod's meadow: steps round a giant oak to a crow's nest
    //   - the Wishing Well in Gate Hamlet: Water can slip down it to Wellbert's well-bottom
    //   - the Lily Pad Run on the north river: hop upstream from the ford to a willow island
    //   - the Harvest Rush in the cabbage field by the road: a timed run up and down the rows
    //   - Crier Hollerby in Gate Square, who knows a rumour about every one of them
    // Built after the city's tidy-up, so nothing here is cleared away by it.
    public static partial class Level1Builder
    {
        static readonly Vector3 CellarRoom = new Vector3(310f, 0f, -200f);
        static readonly Vector3 BurrowRoom = new Vector3(310f, 0f, -262f);
        static readonly Vector3 WellRoom = new Vector3(310f, 0f, -320f);
        const float CellarZ = 72f, BurrowZ = 40f;

        static void Depth(LevelKit kit)
        {
            Physics.SyncTransforms();
            KeepScaffold(kit);
            SmugglersCellar(kit);
            HobsBurrow(kit);
            BrambleMaze(kit);
            SquiresTrial(kit, ListsAt);
            MillGallery(kit, WindmillAt);
            WoodcuttersCamp(kit);
            RiverTrail(kit);
            LookoutOak(kit);
            WishingWell(kit);
            LilyPadRun(kit);
            HarvestRush(kit);
            TownCrier(kit);
        }

        // Before building somewhere: small, purely decorative things already standing there
        // (a lamp, a flower pot, a puddle) are taken away, with their invisible bump boxes.
        // Anything that does something is left where it is and logged, to be moved by hand.
        static void ClearForDepth(LevelKit kit, Rect area, string what)
        {
            Physics.SyncTransforms();
            var root = kit.Root;
            var solids = new List<Transform>();
            int removed = 0;
            for (int i = 0; i < root.childCount; i++)
            {
                var t = root.GetChild(i);
                if (!t.gameObject.activeSelf) continue;
                if (t.name == "Solid" && t.GetComponent<Renderer>() == null)
                {
                    if (area.Contains(new Vector2(t.position.x, t.position.z))) solids.Add(t);
                    continue;
                }
                if (t.GetComponent<MeshFilter>() != null && t.GetComponent<MeshCollider>() != null) continue;   // ground meshes
                Bounds b;
                if (!RendererBounds(t, out b)) continue;
                if (Mathf.Max(b.size.x, b.size.z) > 12f || b.min.y > 3f) continue;                            // big things, things up high
                if (!area.Contains(new Vector2(b.center.x, b.center.z))) continue;
                if (!Plain(t))
                {
                    Debug.LogWarning("[Level1] '" + t.name + "' at " + b.center.ToString("F1") + " is in the way of " + what + "; move it by hand.");
                    continue;
                }
                t.gameObject.SetActive(false);
                removed++;
            }
            foreach (var box in solids) { box.gameObject.SetActive(false); removed++; }
            if (removed > 0) Debug.Log("[Level1] Cleared " + removed + " small things out of the way of " + what + ".");
        }

        // A point light with no shadows (lanterns, fires, glowing jars).
        static Light WarmLight(Transform parent, Vector3 at, float range, float intensity, Color col)
        {
            var go = new GameObject("Glow");
            go.transform.SetParent(parent, false);
            go.transform.position = at;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = col;
            l.range = range;
            l.intensity = intensity;
            l.shadows = LightShadows.None;
            return l;
        }

        // One purple bubble of a Bubble Rush (hidden until the stopwatch is pressed).
        static void RushBubbleAt(LevelKit kit, BubbleRush rush, Vector3 pos, Material mat)
        {
            var b = kit.Volume<RushBubble>(pos, new Vector3(1.4f, 1.4f, 1.4f), "RushBubble");
            b.Owner = rush;
            var v = kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.7f, mat, default(Vector3), b.transform);
            v.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            b.gameObject.SetActive(false);
            rush.Bubbles.Add(b);
        }

        // A little campfire: a ring of stones, crossed logs and flames that flicker, with a
        // warm light. (Decoration only; it doesn't hurt.)
        static void Campfire(LevelKit kit, Transform g, Vector3 c, float scale)
        {
            var rocks = PatchworkAssets.Felt(new Color(0.62f, 0.6f, 0.58f));
            for (int k = 0; k < 9; k++)
            {
                float a = k * Mathf.PI * 2f / 9f;
                kit.Deco(PrimitiveType.Sphere, c + new Vector3(Mathf.Cos(a), 0.12f, Mathf.Sin(a)) * scale * 0.75f, new Vector3(0.36f, 0.24f, 0.3f) * scale, rocks, new Vector3(0f, k * 40f, 0f), g);
            }
            for (int k = 0; k < 3; k++)
                kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.16f * scale, new Vector3(0.16f, 0.5f, 0.16f) * scale, PatchworkAssets.Wood(new Color(0.45f, 0.32f, 0.22f)), new Vector3(72f, k * 60f, 0f), g);
            var fire = kit.Group("Flames", c + Vector3.up * 0.3f * scale);
            fire.SetParent(g, true);
            Color[] flame = { new Color(1f, 0.55f, 0.2f), new Color(1f, 0.8f, 0.3f), new Color(1f, 0.4f, 0.25f) };
            for (int k = 0; k < 5; k++)
            {
                float a = k * 1.26f;
                kit.Deco(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.15f, 0.25f + (k % 2) * 0.15f, Mathf.Sin(a) * 0.15f) * scale, new Vector3(0.22f, 0.55f - k * 0.05f, 0.22f) * scale, PatchworkAssets.Glow(flame[k % 3], 2.4f), new Vector3(0f, 0f, (k - 2) * 8f), fire);
            }
            var flick = fire.gameObject.AddComponent<CandleFlicker>();
            flick.Flame = fire;
            flick.Glow = WarmLight(g, c + Vector3.up * 1.2f * scale, 9f * scale, 1.5f, new Color(1f, 0.7f, 0.4f));
        }

        // ---------------------------------------------------------------- The Masons' Scaffold

        // The keep is being patched where the Fray chewed the colour out of its stones. Plank
        // ramps zig-zag up its south face in two lanes, a landing at each end, all the way to
        // a top deck eighteen metres up, with a board sticking out over the bailey to glide from.
        static void KeepScaffold(LevelKit kit)
        {
            const float lift = 2f, landing = 1.2f, laneW = 1.4f, top = 18f;
            const int flights = 9;
            float face = KeepAt.z - 6f;                        // the keep's south face
            float xl = KeepAt.x - 3.2f, xr = KeepAt.x + 3.2f;   // between the corner turrets
            float run = xr - xl - landing * 2f;
            float inner = face - 0.1f - laneW * 0.5f, outer = inner - laneW, front = outer - laneW * 0.5f;
            float mid = (xl + xr) * 0.5f;
            var g = kit.Group("MasonsScaffold", Vector3.zero);
            var deckCol = new Color(0.74f, 0.57f, 0.4f);
            var deck = PatchworkAssets.Planks(deckCol);
            var pole = PatchworkAssets.Wood(new Color(0.58f, 0.42f, 0.29f));
            var lash = PatchworkAssets.Yarn(new Color(0.9f, 0.8f, 0.55f));
            float slope = Mathf.Atan2(lift, run) * Mathf.Rad2Deg;
            float rampLen = Mathf.Sqrt(lift * lift + run * run) + 0.15f;
            for (int k = 0; k < flights; k++)
            {
                // Even flights climb west to east up the lane by the wall, odd ones come back
                // east to west up the outer lane, so there's always head room.
                bool east = k % 2 == 0;
                float z = east ? inner : outer, y0 = k * lift;
                var ramp = kit.Block(new Vector3(mid, y0 + lift * 0.5f - 0.1f, z), new Vector3(rampLen, 0.2f, laneW), deckCol, "ScaffoldRamp", deck);
                ramp.rotation = Quaternion.Euler(0f, 0f, east ? slope : -slope);
                // Cleats across it, so it reads as a walkway you could actually climb.
                for (int c = 1; c <= 4; c++)
                {
                    float f = c / 5f;
                    float x = east ? Mathf.Lerp(xl + landing, xr - landing, f) : Mathf.Lerp(xr - landing, xl + landing, f);
                    kit.Deco(PrimitiveType.Cube, new Vector3(x, y0 + lift * f + 0.04f, z), new Vector3(0.1f, 0.08f, laneW - 0.12f), pole, new Vector3(0f, 0f, east ? slope : -slope), g);
                }
                float lx = east ? xr - landing * 0.5f : xl + landing * 0.5f;
                kit.Block(new Vector3(lx, y0 + lift - 0.1f, (inner + outer) * 0.5f), new Vector3(landing, 0.2f, laneW * 2f), deckCol, "ScaffoldLanding", deck);
                // A rope handrail along the front of each landing.
                kit.Deco(PrimitiveType.Cube, new Vector3(lx, y0 + lift + 0.9f, front + 0.05f), new Vector3(landing, 0.06f, 0.06f), lash, default(Vector3), g);
            }
            // The poles, lashed together with ledgers at every landing and braced across the front.
            float[] px = { xl, xl + landing, xr - landing, xr };
            float[] pz = { face - 0.14f, front + 0.06f };
            foreach (var x in px)
                foreach (var z in pz)
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(x, (top + 1.4f) * 0.5f, z), new Vector3(0.16f, (top + 1.4f) * 0.5f, 0.16f), pole, default(Vector3), g);
            for (int k = 1; k <= flights; k++)
            {
                float y = k * lift - 0.25f;
                kit.Deco(PrimitiveType.Cube, new Vector3(mid, y, front + 0.06f), new Vector3(xr - xl + 0.3f, 0.12f, 0.12f), pole, default(Vector3), g);
                for (int i = 0; i < px.Length; i++)
                    kit.Deco(PrimitiveType.Cylinder, new Vector3(px[i], y, front + 0.06f), new Vector3(0.22f, 0.08f, 0.22f), lash, default(Vector3), g);
                if (k % 2 == 1)
                    kit.Deco(PrimitiveType.Cube, new Vector3(mid, y - lift * 0.5f, front + 0.1f), new Vector3(Mathf.Sqrt(run * run + lift * lift), 0.08f, 0.08f), pole,
                        new Vector3(0f, 0f, (k % 4 == 1 ? 1f : -1f) * slope), g);
            }
            // The top deck: the last landing, carried on along the outer lane (there's head room
            // over the flight below it there), and a board out over the bailey.
            kit.Block(new Vector3(xr - landing - 1.2f, top - 0.1f, outer), new Vector3(2.4f, 0.2f, laneW), deckCol, "ScaffoldTop", deck);
            kit.Block(new Vector3(xr - 0.7f, top - 0.08f, front - 1.3f), new Vector3(1f, 0.16f, 2.6f), deckCol, "DivingBoard", deck);
            kit.Deco(PrimitiveType.Cube, new Vector3(xr - 0.7f, top - 0.3f, front - 0.3f), new Vector3(0.14f, 0.3f, 0.6f), pole, default(Vector3), g);
            // A mason's things up top: a tub of mortar, a trowel, a stack of cut stones and a
            // patch of fresh, bright stones in the wall behind.
            var stone = PatchworkAssets.Cobble(new Color(0.95f, 0.9f, 0.8f));
            kit.Deco(PrimitiveType.Cylinder, new Vector3(xr - 0.5f, top + 0.25f, inner), new Vector3(0.7f, 0.25f, 0.7f), PatchworkAssets.Wood(new Color(0.5f, 0.36f, 0.25f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(xr - 0.5f, top + 0.48f, inner), new Vector3(0.6f, 0.02f, 0.6f), PatchworkAssets.Felt(new Color(0.85f, 0.82f, 0.76f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, new Vector3(xr - 0.35f, top + 0.58f, inner + 0.1f), new Vector3(0.05f, 0.05f, 0.4f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), new Vector3(30f, 20f, 0f), g);
            for (int i = 0; i < 3; i++)
                kit.Deco(PrimitiveType.Cube, new Vector3(xr - 3.1f + (i % 2) * 0.3f, top + 0.2f + i * 0.4f, outer - 0.15f), new Vector3(0.8f, 0.38f, 0.5f), stone, new Vector3(0f, i * 12f, 0f), g);
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 4; c++)
                    kit.Deco(PrimitiveType.Cube, new Vector3(mid - 1.6f + c * 1.05f + (r % 2) * 0.5f, 15f + r * 0.55f, face + 0.03f), new Vector3(0.95f, 0.48f, 0.08f),
                        PatchworkAssets.Cobble(Color.Lerp(new Color(0.98f, 0.92f, 0.78f), new Color(1f, 0.78f, 0.65f), (r + c) % 3 * 0.4f)), default(Vector3), g);
            // A hoist: a beam out from the top poles, a rope, and a basket of stones that sways.
            kit.Deco(PrimitiveType.Cube, new Vector3(xl + 0.2f, top + 1.2f, front - 0.6f), new Vector3(0.14f, 0.14f, 1.6f), pole, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(xl + 0.2f, top + 1.05f, front - 1.3f), new Vector3(0.3f, 0.05f, 0.3f), pole, new Vector3(0f, 0f, 90f), g);
            var basket = kit.Group("HoistBasket", new Vector3(xl + 0.2f, top + 1f, front - 1.3f));
            basket.SetParent(g, true);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -5f, 0f), new Vector3(0.03f, 5f, 0.03f), lash, default(Vector3), basket);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -10.3f, 0f), new Vector3(0.8f, 0.3f, 0.8f), PatchworkAssets.Wood(new Color(0.75f, 0.58f, 0.38f)), default(Vector3), basket);
            for (int i = 0; i < 3; i++)
                kit.Deco(PrimitiveType.Sphere, new Vector3((i - 1) * 0.2f, -9.95f, (i % 2) * 0.15f), new Vector3(0.3f, 0.22f, 0.28f), stone, default(Vector3), basket);
            var sway = basket.gameObject.AddComponent<Rocker>();
            sway.Axis = Vector3.right;
            sway.Amount = 3f;
            sway.Speed = 0.8f;
            // The prize, and a few bubbles on the way up.
            SecretMarble.Place(kit, new Vector3(xr - landing - 0.8f, top + 1f, outer), MarbleIds[16], MarbleColor(16));
            for (int k = 2; k < flights; k += 2)   // (the even heights are the west landings)
                ScoreBubbles.Place(kit, new Vector3(xl + landing * 0.5f, k * lift + 1f, (inner + outer) * 0.5f), 10);

            // Down in the bailey: the master mason, a pile of new stone and a tub of mortar.
            // (East of the corner turret, out of its way.)
            Vector3 yard = new Vector3(KeepAt.x + 9.4f, 0.05f, face - 1.9f);
            for (int i = 0; i < 4; i++)
                kit.Block(yard + new Vector3(0.4f + (i % 2) * 1.1f, 0.25f + (i / 2) * 0.5f, 1.2f), new Vector3(1f, 0.5f, 0.6f), new Color(0.95f, 0.9f, 0.8f), "CutStone", stone);
            kit.Deco(PrimitiveType.Cylinder, yard + new Vector3(-1f, 0.3f, 0f), new Vector3(0.9f, 0.3f, 0.9f), PatchworkAssets.Wood(new Color(0.5f, 0.36f, 0.25f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, yard + new Vector3(-1f, 0.58f, 0f), new Vector3(0.78f, 0.02f, 0.78f), PatchworkAssets.Felt(new Color(0.85f, 0.82f, 0.76f)), default(Vector3), g);
            var mason = Toy(kit, yard, -90f, ToyNpc.Kind.Teddy, "Mason Mortarby", new Color(0.82f, 0.68f, 0.5f), new Color(0.45f, 0.5f, 0.62f), ToyNpc.Instrument.None, 0.9f, new Color(0.6f, 0.55f, 0.45f));
            mason.Lines = () => new[]
            {
                "Mortarby's the name, mortar's the game. Somebody has to keep the keep kept!",
                "The Fray had a good chew on the south face last winter. Licked the colour clean out of the stones. We're patching it from the top down.",
                "The ramps go all the way up, back and forth. My apprentice left something shiny on the top deck and won't go back up for it. Heights, he says.",
                "If you do get up there, there's a board sticking out over the bailey. Some folk jump off it on purpose. Some folk glide.",
            };
            kit.Hint(new Vector3(xl - 0.8f, 1.5f, front - 0.6f), new Vector3(4f, 3f, 2.4f), "The Masons' Scaffold",
                "Plank ramps zig-zag all the way up the keep's south face. Something glints on the top deck.");
            kit.AddCameraZone(new Vector3(mid, top * 0.5f + 1f, face - 2.5f), new Vector3(xr - xl + 3f, top + 4f, 6f), 3f, 4f);
        }

        // ---------------------------------------------------------------- The Smugglers' Cellar

        // Under the quay on the town side of the canal there's an old jam cellar the first
        // toymakers dug. The only way in now is a grate in the canal wall with one bar bent
        // aside: a squeeze only something runny can manage. Captain Treacle lives there among
        // barrels of sweets he brings in past the Mayor's toll, by canal, by night.
        static void SmugglersCellar(LevelKit kit)
        {
            Vector3 o = CellarRoom;
            var size = new Vector3(18f, 6f, 14f);
            Room(kit, o, size, PatchworkAssets.Cobble(new Color(0.55f, 0.52f, 0.48f)), PatchworkAssets.Brick(new Color(0.58f, 0.38f, 0.32f)),
                PatchworkAssets.Planks(new Color(0.38f, 0.27f, 0.2f)), "Cellar");
            var g = kit.Group("SmugglersCellar", Vector3.zero);
            var iron = PatchworkAssets.Plain(TownKit.Iron, 0.5f);
            var dark = PatchworkAssets.Plain(new Color(0.05f, 0.05f, 0.08f), 0.2f);
            var stone = PatchworkAssets.Cobble(TownKit.Stone);
            var wood = PatchworkAssets.Planks(new Color(0.6f, 0.42f, 0.28f));

            // The way in: the grate in the canal's west wall, half under the water.
            Vector3 grate = new Vector3(CanalWest + 0.2f, CanalSurface, CellarZ);
            kit.Deco(PrimitiveType.Cylinder, grate + Vector3.right * 0.02f, new Vector3(3.2f, 0.06f, 3.2f), stone, new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Cylinder, grate + Vector3.right * 0.06f, new Vector3(2.5f, 0.04f, 2.5f), dark, new Vector3(0f, 0f, 90f), g);
            for (int i = -2; i <= 2; i++)
            {
                float dz = i * 0.45f, h = Mathf.Sqrt(1.25f * 1.25f - dz * dz);
                if (i == 1)
                {
                    // The bent one.
                    kit.Deco(PrimitiveType.Cylinder, grate + new Vector3(0.3f, 0.2f, dz + 0.25f), new Vector3(0.09f, h * 0.9f, 0.09f), iron, new Vector3(18f, 0f, 0f), g);
                    continue;
                }
                kit.Deco(PrimitiveType.Cylinder, grate + new Vector3(0.14f, 0f, dz), new Vector3(0.09f, h, 0.09f), iron, default(Vector3), g);
            }
            kit.Deco(PrimitiveType.Cube, grate + new Vector3(0.14f, 0.45f, 0f), new Vector3(0.08f, 0.08f, 2.3f), iron, default(Vector3), g);
            var inGrate = WarpAt(kit, grate + new Vector3(0.7f, 0f, 0f), new Vector3(1.4f, 2f, 2.2f), o + new Vector3(-6.6f, 0f, -4f), 90f, false, "");
            inGrate.WaterOnly = true;
            kit.Hint(grate + new Vector3(2.4f, 0.8f, 0f), new Vector3(3.5f, 3f, 4.5f), "A Bent Grate",
                "One of the bars has been bent aside. Something runny could squeeze through...");
            kit.Hint(new Vector3(CanalWest - 1.4f, 1.5f, CellarZ - 3.5f), new Vector3(3f, 3f, 3f), "Under the Quay",
                "Is that someone humming, down under the street? It seems to come from a grate in the canal wall.");

            // Inside: the pipe you came out of, with a puddle under it.
            Vector3 pipe = o + new Vector3(-size.x * 0.5f + 0.05f, 1.3f, -4f);
            kit.Deco(PrimitiveType.Cylinder, pipe, new Vector3(2.4f, 0.1f, 2.4f), stone, new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Cylinder, pipe + Vector3.right * 0.08f, new Vector3(1.9f, 0.04f, 1.9f), dark, new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Cylinder, o + new Vector3(-7.4f, 0.02f, -4f), new Vector3(2.6f, 0.02f, 2f), PatchworkAssets.Bubble(new Color(0.45f, 0.65f, 0.9f, 0.7f)), default(Vector3), g);
            // Barrels of sweets along the north wall, and striped crates of humbugs.
            for (int i = 0; i < 6; i++)
                Barrel(kit, o + new Vector3(-7.6f + i * 1.25f, 0f, 6.2f), i * 37f);
            Barrel(kit, o + new Vector3(-6.95f, 1.1f, 6.2f), 15f);
            Barrel(kit, o + new Vector3(-4.45f, 1.1f, 6.2f), 70f);
            var humbug = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.98f, 0.96f, 0.92f), new Color(0.85f, 0.3f, 0.35f));
            for (int i = 0; i < 3; i++)
            {
                var crate = kit.Block(o + new Vector3(0.2f + i * 1.3f, 0.5f, 6.1f), new Vector3(1.1f, 1f, 1.1f), Color.white, "SweetCrate", humbug);
                crate.rotation = Quaternion.Euler(0f, i * 9f, 0f);
            }
            // Sacks of sherbet and a heap of wrapped toffees spilling out of one.
            var sack = PatchworkAssets.Felt(new Color(0.9f, 0.84f, 0.7f));
            for (int i = 0; i < 3; i++)
                kit.Deco(PrimitiveType.Sphere, o + new Vector3(-7.3f + i * 0.9f, 0.45f, -6f), new Vector3(0.85f, 0.95f, 0.75f), sack, new Vector3(0f, i * 30f, 8f), g);
            Color[] wrappers = { new Color(1f, 0.55f, 0.6f), new Color(0.55f, 0.8f, 1f), new Color(1f, 0.85f, 0.35f), new Color(0.65f, 0.9f, 0.55f) };
            for (int i = 0; i < 14; i++)
            {
                float a = i * 2.4f, r = 0.3f + (i % 4) * 0.18f;
                kit.Deco(PrimitiveType.Capsule, o + new Vector3(-5.2f + Mathf.Cos(a) * r, 0.07f + (i % 3) * 0.03f, -5.6f + Mathf.Sin(a) * r), new Vector3(0.12f, 0.1f, 0.12f),
                    PatchworkAssets.Plain(wrappers[i % wrappers.Length], 0.7f), new Vector3(0f, i * 50f, 90f), g);
            }

            // Somebody's stacked crates up the east end like a staircase, to a high shelf.
            float[] tall = { 1f, 2f, 3f, 4f };
            for (int i = 0; i < tall.Length; i++)
            {
                float w = i == 3 ? 1.8f : 1.4f;
                kit.Block(o + new Vector3(2.6f + i * 1.65f, tall[i] * 0.5f, -5f), new Vector3(w, tall[i], w), new Color(0.6f, 0.42f, 0.28f), "CrateStair", wood);
                kit.Deco(PrimitiveType.Cube, o + new Vector3(2.6f + i * 1.65f, tall[i] - 0.35f, -5f - w * 0.5f - 0.01f), new Vector3(w * 0.7f, 0.3f, 0.02f), humbug, default(Vector3), g);
            }
            kit.Block(o + new Vector3(8.1f, 3.9f, 0.66f), new Vector3(1.6f, 0.2f, 9.5f), new Color(0.6f, 0.42f, 0.28f), "HighShelf", wood);
            for (int i = 0; i < 4; i++)
                kit.Deco(PrimitiveType.Cube, o + new Vector3(8.6f, 1.9f, -3.2f + i * 2.8f), new Vector3(0.12f, 3.8f, 0.12f), PatchworkAssets.Wood(TownKit.DarkWood), default(Vector3), g);
            for (int i = 0; i < 4; i++)
                kit.Deco(PrimitiveType.Cylinder, o + new Vector3(8.2f, 4.2f, -2f + i * 1.1f), new Vector3(0.35f, 0.2f, 0.35f), PatchworkAssets.Glow(wrappers[i], 0.5f), default(Vector3), g);
            SecretMarble.Place(kit, o + new Vector3(8.1f, 4.9f, 4.2f), MarbleIds[17], MarbleColor(17));

            // The captain's table, with a chart of the canal, a candle and his telescope.
            Vector3 table = o + new Vector3(-1.2f, 0f, 0.6f);
            kit.Block(table + new Vector3(0f, 0.9f, 0f), new Vector3(2.4f, 0.12f, 1.4f), new Color(0.5f, 0.34f, 0.24f), "CaptainsTable", PatchworkAssets.Planks(new Color(0.5f, 0.34f, 0.24f)));
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    kit.Deco(PrimitiveType.Cube, table + new Vector3(sx * 1.05f, 0.42f, sz * 0.55f), new Vector3(0.12f, 0.84f, 0.12f), PatchworkAssets.Wood(TownKit.DarkWood), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, table + new Vector3(-0.2f, 0.97f, 0f), new Vector3(1.3f, 0.01f, 0.9f), PatchworkAssets.Felt(new Color(0.95f, 0.9f, 0.76f)), new Vector3(0f, 8f, 0f), g);
            kit.Deco(PrimitiveType.Cube, table + new Vector3(-0.3f, 0.98f, 0.05f), new Vector3(0.12f, 0.01f, 0.85f), PatchworkAssets.Felt(new Color(0.45f, 0.65f, 0.9f)), new Vector3(0f, 8f, 0f), g);
            kit.Deco(PrimitiveType.Cylinder, table + new Vector3(0.8f, 1.08f, -0.3f), new Vector3(0.12f, 0.12f, 0.12f), PatchworkAssets.Felt(new Color(0.98f, 0.95f, 0.88f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, table + new Vector3(0.8f, 1.28f, -0.3f), new Vector3(0.07f, 0.12f, 0.07f), PatchworkAssets.Glow(new Color(1f, 0.75f, 0.35f), 2.2f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, table + new Vector3(0.5f, 1.02f, 0.35f), new Vector3(0.09f, 0.4f, 0.09f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f), new Vector3(0f, 30f, 90f), g);
            WarmLight(g, table + new Vector3(0.8f, 2f, -0.3f), 10f, 1.3f, new Color(1f, 0.78f, 0.5f));
            WarmLight(g, o + new Vector3(5.5f, 4.5f, 0f), 10f, 1f, new Color(1f, 0.8f, 0.55f));
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 lamp = o + new Vector3(s * 4.5f, 3.6f, size.z * 0.5f - 0.3f);
                kit.Deco(PrimitiveType.Cube, lamp + Vector3.up * 0.45f, new Vector3(0.06f, 0.5f, 0.06f), iron, default(Vector3), g);
                kit.Deco(PrimitiveType.Sphere, lamp, new Vector3(0.4f, 0.5f, 0.4f), PatchworkAssets.Glow(new Color(1f, 0.8f, 0.45f), 1.4f), default(Vector3), g);
            }
            var captain = Toy(kit, o + new Vector3(-2.8f, 0.05f, 1.4f), -130f, ToyNpc.Kind.Pirate, "Captain Treacle", new Color(0.55f, 0.32f, 0.28f), new Color(0.95f, 0.8f, 0.4f), ToyNpc.Instrument.None, 0.82f, new Color(0.65f, 0.4f, 0.25f));
            captain.Lines = () => new[]
            {
                "Shh! Close that grate behind you! ...Oh. It doesn't close. Never mind.",
                "Captain Treacle, at your service. Smuggler of the finest sweets, past the Mayor's toll and up the canal, by night.",
                "These old cellars run under half the town. The first toymakers dug them to keep the jam cool. Then the Fray came, and everyone forgot. Except me.",
                "Somebody's stacked my crates up like a staircase over there. There's a marble on the top shelf. Not mine. Probably.",
            };

            // The way out: a stout door up a step, back to the quay.
            Vector3 door = o + new Vector3(0f, 0f, -size.z * 0.5f + 0.05f);
            kit.Deco(PrimitiveType.Cube, door + new Vector3(0f, 1.3f, 0.02f), new Vector3(1.8f, 2.6f, 0.1f), PatchworkAssets.Planks(new Color(0.45f, 0.3f, 0.22f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, door + new Vector3(0f, 2.7f, 0.04f), new Vector3(2.3f, 0.25f, 0.14f), stone, default(Vector3), g);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, door + new Vector3(s * 1.03f, 1.3f, 0.04f), new Vector3(0.25f, 2.6f, 0.14f), stone, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, door + new Vector3(0.55f, 1.2f, 0.1f), new Vector3(0.22f, 0.02f, 0.22f), iron, new Vector3(90f, 0f, 0f), g);
            WarpAt(kit, door + new Vector3(0f, 1.2f, 0.8f), new Vector3(2f, 2.4f, 1.4f), new Vector3(CanalWest - 1.6f, 0f, CellarZ + 2.5f), -90f, true, "Climb out");
            kit.Hint(o + new Vector3(-6f, 1.5f, -4f), new Vector3(4f, 3f, 4f), "The Smugglers' Cellar",
                "An old jam cellar under the town, full of somebody's sweets. The door in the south wall takes you back up to the quay.");
        }

        // ---------------------------------------------------------------- Hob's Burrow

        // A little round door in the west wall of Thread Ravine, with a lamp and toadstools by
        // it. Old Hob lives behind it: he used to mind the seams, the thin places in the world's
        // stitching where the Fray come through, and he knows more about them than anyone.
        static void HobsBurrow(LevelKit kit)
        {
            var g = kit.Group("HobsBurrow", Vector3.zero);
            float wall = RavineWest + 0.2f, floor = -5f;
            var green = PatchworkAssets.Planks(new Color(0.42f, 0.62f, 0.45f));
            var stone = PatchworkAssets.Cobble(new Color(0.7f, 0.66f, 0.6f));
            var gold = PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f);
            var darkWood = PatchworkAssets.Wood(TownKit.DarkWood);
            Vector3 door = new Vector3(wall, floor + 1.35f, BurrowZ);
            kit.Deco(PrimitiveType.Cylinder, door + Vector3.right * 0.03f, new Vector3(3.2f, 0.05f, 3.2f), stone, new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Cylinder, door + Vector3.right * 0.07f, new Vector3(2.6f, 0.05f, 2.6f), green, new Vector3(0f, 0f, 90f), g);
            for (int i = 0; i < 5; i++)
                kit.Deco(PrimitiveType.Cube, door + new Vector3(0.11f, 0f, -1f + i * 0.5f), new Vector3(0.02f, Mathf.Sqrt(1.3f * 1.3f - Mathf.Pow(-1f + i * 0.5f, 2f)) * 2f - 0.1f, 0.04f), darkWood, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, door + new Vector3(0.16f, 0f, 0f), Vector3.one * 0.22f, gold, default(Vector3), g);
            // Round windows either side, lit warm, with little sills of moss.
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 w = new Vector3(wall + 0.06f, floor + 1.9f, BurrowZ + s * 2.4f);
                kit.Deco(PrimitiveType.Cylinder, w, new Vector3(0.9f, 0.04f, 0.9f), stone, new Vector3(0f, 0f, 90f), g);
                kit.Deco(PrimitiveType.Cylinder, w + Vector3.right * 0.03f, new Vector3(0.7f, 0.03f, 0.7f), PatchworkAssets.Glow(new Color(1f, 0.8f, 0.45f), 0.9f), new Vector3(0f, 0f, 90f), g);
                kit.Deco(PrimitiveType.Cube, w + new Vector3(0.15f, -0.5f, 0f), new Vector3(0.3f, 0.12f, 0.9f), PatchworkAssets.Felt(new Color(0.42f, 0.6f, 0.35f)), default(Vector3), g);
            }
            // A step, a doormat, a lamp on a crooked post and toadstools.
            kit.Deco(PrimitiveType.Cube, new Vector3(wall + 0.6f, floor + 0.02f, BurrowZ), new Vector3(1f, 0.04f, 1.6f), PatchworkAssets.Felt(new Color(0.75f, 0.45f, 0.3f)), default(Vector3), g);
            Vector3 lampFoot = new Vector3(wall + 1.1f, floor, BurrowZ + 3.5f);
            kit.Deco(PrimitiveType.Cylinder, lampFoot + Vector3.up * 1.1f, new Vector3(0.1f, 1.1f, 0.1f), darkWood, new Vector3(0f, 0f, -6f), g);
            kit.Deco(PrimitiveType.Cube, lampFoot + new Vector3(-0.1f, 2.15f, 0f), new Vector3(0.5f, 0.06f, 0.06f), darkWood, default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, lampFoot + new Vector3(-0.3f, 1.85f, 0f), new Vector3(0.3f, 0.38f, 0.3f), PatchworkAssets.Glow(new Color(1f, 0.78f, 0.4f), 1.6f), default(Vector3), g);
            WarmLight(g, lampFoot + new Vector3(0.4f, 1.8f, 0f), 7f, 1.1f, new Color(1f, 0.78f, 0.45f));
            Mushroom(kit, new Vector3(wall + 0.7f, floor, BurrowZ - 1.9f), 0.7f, new Color(0.95f, 0.4f, 0.4f), false);
            Mushroom(kit, new Vector3(wall + 1.2f, floor, BurrowZ - 2.6f), 0.45f, new Color(0.7f, 0.85f, 1f), true);
            Mushroom(kit, new Vector3(wall + 0.6f, floor, BurrowZ + 4.4f), 0.55f, new Color(1f, 0.75f, 0.4f), true);
            // A stovepipe poking out of the ravine's lip, the only sign from up top.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(RavineWest - 1.2f, 0.6f, BurrowZ + 0.8f), new Vector3(0.35f, 0.6f, 0.35f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(RavineWest - 1.2f, 1.25f, BurrowZ + 0.8f), new Vector3(0.5f, 0.05f, 0.5f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), default(Vector3), g);
            Solid(kit, new Vector3(RavineWest - 1.2f, 0.6f, BurrowZ + 0.8f), new Vector3(0.4f, 1.2f, 0.4f));
            ClearGrass(new Vector3(RavineWest - 1.2f, 0f, BurrowZ + 0.8f), 0.6f);

            Vector3 o = BurrowRoom;
            var size = new Vector3(16f, 5.5f, 14f);
            WarpAt(kit, new Vector3(wall + 0.7f, floor + 1.2f, BurrowZ), new Vector3(1.4f, 2.4f, 2.4f), o + new Vector3(-5.6f, 0f, -4f), 90f, true, "Knock");
            kit.Hint(new Vector3(wall + 3f, floor + 1.5f, BurrowZ), new Vector3(4f, 3f, 6f), "A Little Round Door",
                "Somebody lives down here, in the ravine wall. Perhaps they're in?");

            // Inside: a snug, low burrow lit by glowing toadstools, roots hanging from the roof.
            Room(kit, o, size, PatchworkAssets.Felt(new Color(0.55f, 0.42f, 0.3f)), PatchworkAssets.Knit(new Color(0.42f, 0.5f, 0.36f)),
                PatchworkAssets.Felt(new Color(0.32f, 0.25f, 0.2f)), "Burrow");
            var gi = kit.Group("BurrowInside", Vector3.zero);
            var rootMat = PatchworkAssets.Wood(new Color(0.48f, 0.36f, 0.26f));
            var rng = new System.Random(77);
            for (int i = 0; i < 16; i++)
            {
                float x = (float)(rng.NextDouble() * 2.0 - 1.0) * (size.x * 0.5f - 1f);
                float z = (float)(rng.NextDouble() * 2.0 - 1.0) * (size.z * 0.5f - 1f);
                float len = 0.6f + (float)rng.NextDouble() * 1.2f;
                kit.Deco(PrimitiveType.Cylinder, o + new Vector3(x, size.y - len * 0.5f, z), new Vector3(0.12f, len * 0.5f, 0.12f), rootMat,
                    new Vector3((float)(rng.NextDouble() * 30.0 - 15.0), 0f, (float)(rng.NextDouble() * 30.0 - 15.0)), gi);
            }
            // The door you came in by (and leave by).
            Vector3 inDoor = o + new Vector3(-size.x * 0.5f + 0.05f, 1.35f, 2f);
            kit.Deco(PrimitiveType.Cylinder, inDoor, new Vector3(3f, 0.05f, 3f), stone, new Vector3(0f, 0f, 90f), gi);
            kit.Deco(PrimitiveType.Cylinder, inDoor + Vector3.right * 0.04f, new Vector3(2.5f, 0.05f, 2.5f), green, new Vector3(0f, 0f, 90f), gi);
            kit.Deco(PrimitiveType.Sphere, inDoor + new Vector3(0.12f, 0f, 0f), Vector3.one * 0.22f, gold, default(Vector3), gi);
            WarpAt(kit, inDoor + new Vector3(0.75f, -0.15f, 0f), new Vector3(1.4f, 2.4f, 2.4f), new Vector3(wall + 2.2f, floor, BurrowZ), 90f, true, "Leave");
            // A hearth on the south wall with a kettle, a mantel of rescued colours in jars,
            // and Hob's armchair.
            Vector3 hearth = o + new Vector3(2f, 0f, -size.z * 0.5f + 0.5f);
            var brick = PatchworkAssets.Brick(new Color(0.62f, 0.4f, 0.34f));
            kit.Block(hearth + new Vector3(0f, 1.1f, 0f), new Vector3(3f, 2.2f, 1f), new Color(0.62f, 0.4f, 0.34f), "Hearth", brick);
            kit.Block(hearth + new Vector3(0f, 3.9f, -0.1f), new Vector3(1.4f, 3.4f, 0.8f), new Color(0.62f, 0.4f, 0.34f), "Chimney", brick);
            kit.Deco(PrimitiveType.Cube, hearth + new Vector3(0f, 0.6f, 0.51f), new Vector3(1.6f, 1.1f, 0.02f), PatchworkAssets.Plain(new Color(0.1f, 0.07f, 0.06f), 0.2f), default(Vector3), gi);
            Campfire(kit, gi, hearth + new Vector3(0f, 0.02f, 0.75f), 0.7f);
            kit.Deco(PrimitiveType.Cube, hearth + new Vector3(0f, 2.28f, 0.15f), new Vector3(3.3f, 0.15f, 1.2f), PatchworkAssets.Planks(new Color(0.5f, 0.34f, 0.24f)), default(Vector3), gi);
            Color[] colours = { new Color(1f, 0.45f, 0.5f), new Color(1f, 0.8f, 0.3f), new Color(0.45f, 0.85f, 0.55f), new Color(0.45f, 0.7f, 1f), new Color(0.8f, 0.55f, 1f) };
            for (int i = 0; i < colours.Length; i++)
            {
                Vector3 jar = hearth + new Vector3(-1.2f + i * 0.6f, 2.6f, 0.4f);
                kit.Deco(PrimitiveType.Cylinder, jar, new Vector3(0.3f, 0.2f, 0.3f), PatchworkAssets.Bubble(new Color(0.85f, 0.95f, 1f, 0.45f)), default(Vector3), gi);
                kit.Deco(PrimitiveType.Sphere, jar, new Vector3(0.2f, 0.26f, 0.2f), PatchworkAssets.Glow(colours[i], 1.8f), default(Vector3), gi);
                kit.Deco(PrimitiveType.Cylinder, jar + Vector3.up * 0.22f, new Vector3(0.22f, 0.04f, 0.22f), PatchworkAssets.Felt(new Color(0.65f, 0.48f, 0.32f)), default(Vector3), gi);
            }
            Vector3 chair = o + new Vector3(0.2f, 0f, -3.8f);
            var cushion = PatchworkAssets.Damask(new Color(0.75f, 0.35f, 0.35f));
            kit.Block(chair + new Vector3(0f, 0.3f, 0f), new Vector3(1.2f, 0.6f, 1.1f), new Color(0.75f, 0.35f, 0.35f), "Armchair", cushion);
            kit.Deco(PrimitiveType.Cube, chair + new Vector3(0f, 1f, 0.5f), new Vector3(1.2f, 1.3f, 0.25f), cushion, default(Vector3), gi);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, chair + new Vector3(s * 0.55f, 0.75f, 0f), new Vector3(0.2f, 0.4f, 1.1f), cushion, default(Vector3), gi);
            // A bookshelf on the east wall, crammed with notes on stitches and seams.
            Vector3 shelf = o + new Vector3(size.x * 0.5f - 0.45f, 0f, -3f);
            kit.Block(shelf + new Vector3(0f, 1.6f, 0f), new Vector3(0.8f, 3.2f, 3f), new Color(0.5f, 0.34f, 0.24f), "Bookshelf", PatchworkAssets.Planks(new Color(0.5f, 0.34f, 0.24f)));
            Color[] spines = { new Color(0.75f, 0.3f, 0.3f), new Color(0.3f, 0.45f, 0.7f), new Color(0.4f, 0.6f, 0.35f), new Color(0.85f, 0.7f, 0.3f), new Color(0.55f, 0.35f, 0.6f) };
            for (int row = 0; row < 4; row++)
                for (int b = 0; b < 9; b++)
                {
                    float h = 0.45f + ((b * 7 + row * 3) % 5) * 0.06f;
                    kit.Deco(PrimitiveType.Cube, shelf + new Vector3(-0.42f, 0.25f + row * 0.78f + h * 0.5f, -1.25f + b * 0.3f), new Vector3(0.06f, h, 0.24f), PatchworkAssets.Felt(spines[(b + row) % spines.Length]), default(Vector3), gi);
                }
            // Glowing toadstools all round, and three big ones that make steps up to the root
            // shelf on the north wall, where Hob keeps the marble he found in a seam.
            Color[] caps = { new Color(0.6f, 0.85f, 1f), new Color(1f, 0.65f, 0.85f), new Color(0.75f, 0.95f, 0.6f), new Color(1f, 0.82f, 0.45f) };
            Vector3[] little = { new Vector3(-6.8f, 0f, 5.8f), new Vector3(-6.2f, 0f, 6.3f), new Vector3(-7.2f, 0f, -1f), new Vector3(6.8f, 0f, 1.2f),
                                 new Vector3(6.2f, 0f, -6.2f), new Vector3(-3.5f, 0f, -6.4f), new Vector3(-2.2f, 0f, 6.4f), new Vector3(7f, 0f, 3.2f) };
            for (int i = 0; i < little.Length; i++)
                Mushroom(kit, o + little[i], 0.4f + (i % 3) * 0.15f, caps[i % caps.Length], true);
            Vector3[] steps = { new Vector3(-1f, 1f, 3.6f), new Vector3(1f, 2f, 4.6f), new Vector3(3.4f, 3f, 5.2f) };
            for (int i = 0; i < steps.Length; i++)
            {
                float s = steps[i].y / 1.15f;
                Vector3 foot = o + new Vector3(steps[i].x, 0f, steps[i].z);
                var m = kit.Group("Toadstool", foot);
                m.localScale = Vector3.one * s;
                m.SetParent(gi, true);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.4f, 0f), new Vector3(0.32f, 0.4f, 0.32f), PatchworkAssets.Felt(new Color(0.95f, 0.92f, 0.85f)), default(Vector3), m);
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.85f, 0f), new Vector3(1.1f, 0.6f, 1.1f), PatchworkAssets.Glow(caps[i], 1.2f), default(Vector3), m);
                kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 1.14f, 0f), new Vector3(0.85f, 0.01f, 0.85f), PatchworkAssets.Glow(Color.Lerp(caps[i], Color.white, 0.5f), 1f), default(Vector3), m);
                var cap = kit.Block(foot + new Vector3(0f, steps[i].y * 0.5f, 0f), new Vector3(0.75f * s, steps[i].y, 0.75f * s), caps[i], "ToadstoolStep");
                cap.GetComponent<Renderer>().enabled = false;
            }
            kit.Block(o + new Vector3(5.9f, 3.2f, 6.2f), new Vector3(2.6f, 0.2f, 1.4f), new Color(0.48f, 0.36f, 0.26f), "RootShelf", rootMat);
            for (int i = 0; i < 3; i++)
                kit.Deco(PrimitiveType.Cylinder, o + new Vector3(4.8f + i * 1.1f, 4.3f, 6.5f), new Vector3(0.16f, 1.1f, 0.16f), rootMat, new Vector3(10f - i * 10f, 0f, 0f), gi);
            SecretMarble.Place(kit, o + new Vector3(6.6f, 4.1f, 6.2f), MarbleIds[18], MarbleColor(18));
            WarmLight(gi, o + new Vector3(-3f, 4f, 3f), 11f, 0.9f, new Color(0.7f, 0.85f, 1f));

            var hob = Toy(kit, o + new Vector3(-1.4f, 0.05f, -4.4f), -85f, ToyNpc.Kind.Teddy, "Old Hob", new Color(0.62f, 0.56f, 0.5f), new Color(0.4f, 0.5f, 0.38f), ToyNpc.Instrument.None, 0.7f, new Color(0.45f, 0.5f, 0.4f));
            hob.Lines = () => new[]
            {
                "Come in, come in, mind the roots. Old Hob doesn't get many visitors. Doesn't get any, truth be told.",
                "I used to mind the seams, back when the town was young. Little thin places where the world's stitching wears through. That's where the Fray come from.",
                "From the Underside, see. The back of the cloth. All knots and loose ends and not a speck of colour anywhere. No wonder they're hungry for ours.",
                "Their King never comes close. Stands off and watches, with our colours in little jars on his coat. I pinched a few back. They're on the mantel.",
                "There's a marble up on my root shelf, found it in a seam. Hop up the toadstools, they're sturdier than they look.",
            };
            kit.Hint(o + new Vector3(-5.6f, 1.5f, -4f), new Vector3(4f, 3f, 4f), "Hob's Burrow",
                "A snug burrow behind the round door, lit by glowing toadstools. The door is the way back out.");
        }

        // ---------------------------------------------------------------- The Bramble Maze

        // A maze of tall hedges on the green west of the barracks, planted by the first Mayor
        // and clipped every week since. The way in is from the street to the south; the marble
        // sits on a little plinth in its heart (the cell furthest from the way in).
        static void BrambleMaze(LevelKit kit)
        {
            const int W = 6, H = 9;
            const float cell = 2.6f, hedgeH = 2.3f, hedgeW = 0.8f;
            const float x0 = -35.4f, z0 = 31f;
            const int gateCell = 3;
            float x1 = x0 + W * cell, z1 = z0 + H * cell;
            var centre = new Vector3((x0 + x1) * 0.5f, 0f, (z0 + z1) * 0.5f);
            ClearForDepth(kit, new Rect(x0 - 0.6f, z0 - 0.6f, W * cell + 1.2f, H * cell + 1.2f), "the Bramble Maze");

            // A maze carved by a random walk that backs up when it's stuck (so every cell can
            // be reached, by exactly one way), then two walls knocked through so there's more
            // than one way round.
            var vWall = new bool[W + 1, H];
            var hWall = new bool[W, H + 1];
            for (int i = 0; i <= W; i++) for (int j = 0; j < H; j++) vWall[i, j] = true;
            for (int i = 0; i < W; i++) for (int j = 0; j <= H; j++) hWall[i, j] = true;
            var rng = new System.Random(1278);   // (a good one: a long way to the heart, and eight dead ends)
            var seen = new bool[W, H];
            var stack = new Stack<Vector2Int>();
            stack.Push(new Vector2Int(gateCell, 0));
            seen[gateCell, 0] = true;
            int[] dx = { 1, -1, 0, 0 }, dz = { 0, 0, 1, -1 };
            var options = new List<int>();
            while (stack.Count > 0)
            {
                var c = stack.Peek();
                options.Clear();
                for (int d = 0; d < 4; d++)
                {
                    int nx = c.x + dx[d], nz = c.y + dz[d];
                    if (nx >= 0 && nx < W && nz >= 0 && nz < H && !seen[nx, nz]) options.Add(d);
                }
                if (options.Count == 0) { stack.Pop(); continue; }
                int dir = options[rng.Next(options.Count)];
                if (dir == 0) vWall[c.x + 1, c.y] = false;
                else if (dir == 1) vWall[c.x, c.y] = false;
                else if (dir == 2) hWall[c.x, c.y + 1] = false;
                else hWall[c.x, c.y] = false;
                var n = new Vector2Int(c.x + dx[dir], c.y + dz[dir]);
                seen[n.x, n.y] = true;
                stack.Push(n);
            }
            for (int k = 0; k < 2; k++)
            {
                int i = 1 + rng.Next(W - 1), j = rng.Next(H);
                vWall[i, j] = false;
            }
            hWall[gateCell, 0] = false;

            // The heart: the cell the most steps from the way in.
            var dist = new int[W, H];
            for (int i = 0; i < W; i++) for (int j = 0; j < H; j++) dist[i, j] = -1;
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(gateCell, 0));
            dist[gateCell, 0] = 0;
            var heart = new Vector2Int(gateCell, 0);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (dist[c.x, c.y] > dist[heart.x, heart.y]) heart = c;
                for (int d = 0; d < 4; d++)
                {
                    int nx = c.x + dx[d], nz = c.y + dz[d];
                    if (nx < 0 || nx >= W || nz < 0 || nz >= H || dist[nx, nz] >= 0) continue;
                    bool open = d == 0 ? !vWall[c.x + 1, c.y] : d == 1 ? !vWall[c.x, c.y] : d == 2 ? !hWall[c.x, c.y + 1] : !hWall[c.x, c.y];
                    if (!open) continue;
                    dist[nx, nz] = dist[c.x, c.y] + 1;
                    queue.Enqueue(new Vector2Int(nx, nz));
                }
            }

            // The hedges, joined up into long runs. (The east-west ones stop just short of the
            // north-south ones and are a hair thinner and lower, so no two faces sit together.)
            for (int i = 0; i <= W; i++)
                for (int j = 0; j < H; )
                {
                    if (!vWall[i, j]) { j++; continue; }
                    int j1 = j;
                    while (j1 + 1 < H && vWall[i, j1 + 1]) j1++;
                    float x = x0 + i * cell;
                    Hedge(kit, new Vector3(x, 0f, z0 + j * cell - hedgeW * 0.5f), new Vector3(x, 0f, z0 + (j1 + 1) * cell + hedgeW * 0.5f), hedgeH, hedgeW);
                    j = j1 + 1;
                }
            for (int j = 0; j <= H; j++)
                for (int i = 0; i < W; )
                {
                    if (!hWall[i, j]) { i++; continue; }
                    int i1 = i;
                    while (i1 + 1 < W && hWall[i1 + 1, j]) i1++;
                    float z = z0 + j * cell;
                    Hedge(kit, new Vector3(x0 + i * cell + hedgeW * 0.5f - 0.02f, 0f, z), new Vector3(x0 + (i1 + 1) * cell - hedgeW * 0.5f + 0.02f, 0f, z), hedgeH - 0.05f, hedgeW - 0.04f);
                    i = i1 + 1;
                }
            // Topiary balls on the gateposts, and a gravel walk inside.
            var g = kit.Group("BrambleMaze", Vector3.zero);
            var topiary = PatchworkAssets.Felt(new Color(0.36f, 0.58f, 0.36f));
            for (int s = 0; s <= 1; s++)
            {
                Vector3 post = new Vector3(x0 + (gateCell + s) * cell, hedgeH + 0.45f, z0);
                kit.Deco(PrimitiveType.Sphere, post, Vector3.one * 0.9f, topiary, default(Vector3), g);
                kit.Deco(PrimitiveType.Sphere, post + Vector3.up * 0.6f, Vector3.one * 0.25f, PatchworkAssets.Felt(new Color(1f, 0.6f, 0.65f)), default(Vector3), g);
            }
            kit.Deco(PrimitiveType.Cube, centre + Vector3.up * 0.02f, new Vector3(W * cell, 0.03f, H * cell), PatchworkAssets.Felt(new Color(0.88f, 0.84f, 0.74f)), default(Vector3), g);
            ClearGrass(centre, H * cell * 0.55f);
            // The heart: a sundial on a plinth under a rose arch, and the marble.
            Vector3 hc = new Vector3(x0 + (heart.x + 0.5f) * cell, 0f, z0 + (heart.y + 0.5f) * cell);
            var plinthStone = PatchworkAssets.Cobble(new Color(0.9f, 0.86f, 0.78f));
            kit.Block(hc + Vector3.up * 0.35f, new Vector3(0.8f, 0.7f, 0.8f), new Color(0.9f, 0.86f, 0.78f), "MazePlinth", plinthStone);
            kit.Deco(PrimitiveType.Cylinder, hc + Vector3.up * 0.73f, new Vector3(0.7f, 0.03f, 0.7f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, hc + new Vector3(0f, 0.86f, 0.05f), new Vector3(0.04f, 0.22f, 0.3f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f), new Vector3(35f, 0f, 0f), g);
            SecretMarble.Place(kit, hc + Vector3.up * 1.6f, MarbleIds[19], MarbleColor(19));
            Color[] roses = { new Color(0.95f, 0.45f, 0.55f), new Color(1f, 0.8f, 0.85f), new Color(0.98f, 0.9f, 0.6f) };
            for (int k = 0; k <= 10; k++)
            {
                float a = Mathf.PI * k / 10f;
                Vector3 p = hc + new Vector3(Mathf.Cos(a) * 1.1f, 0.2f + Mathf.Sin(a) * 2.3f, 0f);
                kit.Deco(PrimitiveType.Sphere, p, Vector3.one * 0.35f, topiary, default(Vector3), g);
                kit.Deco(PrimitiveType.Sphere, p + new Vector3(0f, 0.1f, -0.15f), Vector3.one * 0.16f, PatchworkAssets.Felt(roses[k % roses.Length]), default(Vector3), g);
            }
            // The gardener, at the way in.
            var gardener = Toy(kit, new Vector3(x0 + (gateCell + 1) * cell + 1.6f, 0.05f, z0 - 1.6f), 200f, ToyNpc.Kind.RagDoll, "Mistress Clipwell",
                new Color(0.55f, 0.72f, 0.5f), new Color(0.95f, 0.85f, 0.6f), ToyNpc.Instrument.None, 1.05f, new Color(0.4f, 0.6f, 0.38f));
            gardener.Lines = () => new[]
            {
                "Welcome to the Bramble Maze! Planted by the very first Mayor, and clipped by me every Tuesday for forty years.",
                "There's a marble at the heart of it. The trick is to keep one hand on the hedge and never let go. Or so my granny said. She was in there a week.",
                "Mind the Fray. One wandered in last spring and still hasn't found its way out.",
            };
            kit.Hint(new Vector3(x0 + (gateCell + 0.5f) * cell, 1.5f, z0 - 1.5f), new Vector3(4f, 3f, 3f), "The Bramble Maze",
                "Tall hedges, a dozen dead ends, and a marble somewhere in the middle.");
            kit.AddCameraZone(centre + Vector3.up * 3f, new Vector3(W * cell + 1f, 7f, H * cell + 1f), 2f, 16f);
            kit.Info.MapPins.Add(new MapPin("Bramble Maze", centre));
        }

        // ---------------------------------------------------------------- The Squire's Trial

        // At the Lists: step on the stopwatch at the west end, then up the north side of the
        // tilt barrier, round the quintain and back down the south side, vaulting the hay bales
        // and catching every bubble before the time's up.
        static void SquiresTrial(LevelKit kit, Vector3 c)
        {
            Vector3 btn = c + new Vector3(-23f, 0f, 3.4f);
            var rush = kit.Volume<BubbleRush>(btn + Vector3.up * 0.6f, new Vector3(1.8f, 1.4f, 1.8f), "BubbleRush");
            rush.Title = "The Squire's Trial";
            rush.Duration = 30f;
            rush.Button = StopwatchButton(kit, btn, rush.transform);
            var purple = PatchworkAssets.Glow(new Color(0.75f, 0.5f, 1f), 0.8f);
            var strawCol = new Color(0.93f, 0.8f, 0.45f);
            var straw = PatchworkAssets.Felt(strawCol);
            var twine = PatchworkAssets.Felt(new Color(0.75f, 0.55f, 0.3f));
            Vector2[] bales = { new Vector2(-9f, 3.4f), new Vector2(6f, 3.4f), new Vector2(15f, 3.4f), new Vector2(8f, -4.6f), new Vector2(-7f, -4.6f) };
            foreach (var b in bales)
            {
                Vector3 p = c + new Vector3(b.x, 0.5f, b.y);
                kit.Block(p, new Vector3(1.6f, 1f, 2.2f), strawCol, "HayBale", straw);
                for (int s = -1; s <= 1; s += 2)
                    kit.Deco(PrimitiveType.Cube, p + new Vector3(s * 0.45f, 0f, 0f), new Vector3(0.06f, 1.02f, 2.22f), twine);
                ClearGrass(p, 1.4f);
            }
            // Up the north side, round the end, and back down the south.
            Vector3[] course =
            {
                new Vector3(-18f, 1.2f, 3.4f), new Vector3(-13f, 1.2f, 3.4f), new Vector3(-9f, 2.1f, 3.4f), new Vector3(-4f, 1.2f, 3.4f),
                new Vector3(1f, 1.2f, 3.4f), new Vector3(6f, 2.1f, 3.4f), new Vector3(10.5f, 1.2f, 3.4f), new Vector3(15f, 2.1f, 3.4f),
                new Vector3(21.5f, 1.2f, 3f), new Vector3(23.5f, 1.2f, -0.4f), new Vector3(21.5f, 1.2f, -4f),
                new Vector3(13f, 1.2f, -4.6f), new Vector3(8f, 2.1f, -4.6f), new Vector3(3f, 1.2f, -4.6f), new Vector3(-2f, 1.2f, -4.6f),
                new Vector3(-7f, 2.1f, -4.6f), new Vector3(-12f, 1.2f, -4.6f), new Vector3(-17f, 1.2f, -4.6f), new Vector3(-21f, 1.2f, -4.6f),
            };
            foreach (var p in course) RushBubbleAt(kit, rush, c + p, purple);
            rush.Marble = SecretMarble.Place(kit, btn + new Vector3(0f, 2.2f, 0f), MarbleIds[20], MarbleColor(20), true);
            kit.Hint(btn + new Vector3(0f, 1.5f, 2f), new Vector3(4f, 3f, 2.4f), "The Squire's Trial",
                "Up the lane, round the quintain and back, over the hay bales. Step on the stopwatch and catch every bubble!");
            var squire = Toy(kit, c + new Vector3(-24.6f, 0.05f, 5.8f), 120f, ToyNpc.Kind.TinSoldier, "Squire Bodkin", KnightBlue, new Color(0.95f, 0.92f, 0.85f),
                ToyNpc.Instrument.None, 1.2f, new Color(0.3f, 0.42f, 0.65f));
            squire.Lines = () => new[]
            {
                "Squire Bodkin, of the Blue! Well, squire-in-training. Well, I've applied.",
                "The trial's simple: step on the stopwatch, run up the lane, round the quintain, back down the other side. Catch every bubble on the way.",
                "Over the hay bales, mind. Dame Bobbin did it in nineteen seconds. On a hobby horse.",
                "Beat the clock and there's a marble in it for you. That's what Sir Thimble says, anyway.",
            };
        }

        // ---------------------------------------------------------------- The windmill's gallery

        // Plank steps on brackets wind up round the back of the mill (well clear of the sails)
        // to a gallery round the tower. The miller dropped his best marble up there.
        static void MillGallery(LevelKit kit, Vector3 feet)
        {
            const float deckY = 6.05f, front = 210f;   // the mill faces its sails this way (see Windmill)
            var plankCol = new Color(0.66f, 0.5f, 0.34f);
            var planks = PatchworkAssets.Planks(plankCol);
            var darkWood = PatchworkAssets.Wood(TownKit.DarkWood);
            var g = kit.Group("MillGallery", Vector3.zero);
            const float lastStep = 140f - 7f * 26f;
            for (int i = 0; i < 8; i++)
            {
                float a = 140f - i * 26f, top = 0.7f * (i + 1);
                Vector3 dir = Quaternion.Euler(0f, a, 0f) * Vector3.forward;
                Vector3 p = feet + dir * 5.4f;
                var step = kit.Block(new Vector3(p.x, top - 0.15f, p.z), new Vector3(2.2f, 0.3f, 1.2f), plankCol, "MillStep", planks);
                step.rotation = Quaternion.Euler(0f, a, 0f);
                kit.Deco(PrimitiveType.Cube, feet + dir * 4.1f + Vector3.up * (top - 0.42f), new Vector3(0.14f, 0.14f, 1.8f), darkWood, new Vector3(0f, a, 0f), g);
                kit.Deco(PrimitiveType.Cylinder, p + dir * 0.45f + Vector3.up * (top * 0.5f - 0.15f), new Vector3(0.12f, top * 0.5f - 0.15f, 0.12f), darkWood, default(Vector3), g);
            }
            for (int k = 0; k < 10; k++)
            {
                float a = front + 72f + k * 24f;
                Vector3 dir = Quaternion.Euler(0f, a, 0f) * Vector3.forward;
                Vector3 p = feet + dir * 3.8f;
                var d = kit.Block(new Vector3(p.x, deckY - 0.1f, p.z), new Vector3(1.75f, 0.2f, 1.6f), plankCol, "MillGalleryDeck", planks);
                d.rotation = Quaternion.Euler(0f, a, 0f);
                kit.Deco(PrimitiveType.Cube, feet + dir * 3.4f + Vector3.up * (deckY - 0.45f), new Vector3(0.12f, 0.5f, 0.9f), darkWood, new Vector3(0f, a, 0f), g);
                if (Mathf.Abs(Mathf.DeltaAngle(a, lastStep)) < 20f) continue;   // open where the steps come up
                Vector3 r = feet + dir * 4.5f;
                kit.Deco(PrimitiveType.Cylinder, r + Vector3.up * (deckY + 0.45f), new Vector3(0.1f, 0.45f, 0.1f), darkWood, default(Vector3), g);
                kit.Deco(PrimitiveType.Cube, r + Vector3.up * (deckY + 0.85f), new Vector3(1.85f, 0.08f, 0.08f), darkWood, new Vector3(0f, a, 0f), g);
            }
            // The marble round the back, a few bubbles, and an oil can left by the miller.
            Vector3 back = Quaternion.Euler(0f, front + 180f, 0f) * Vector3.forward;
            SecretMarble.Place(kit, feet + back * 3.8f + Vector3.up * (deckY + 1f), MarbleIds[21], MarbleColor(21));
            foreach (float a in new[] { front + 144f, front + 216f, front + 252f })
                ScoreBubbles.Place(kit, feet + Quaternion.Euler(0f, a, 0f) * Vector3.forward * 3.8f + Vector3.up * (deckY + 0.9f), 10);
            Vector3 can = feet + Quaternion.Euler(0f, front + 120f, 0f) * Vector3.forward * 3.5f + Vector3.up * deckY;
            kit.Deco(PrimitiveType.Cylinder, can + Vector3.up * 0.18f, new Vector3(0.3f, 0.18f, 0.3f), PatchworkAssets.Plain(new Color(0.75f, 0.3f, 0.3f), 0.6f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, can + new Vector3(0.15f, 0.38f, 0f), new Vector3(0.05f, 0.18f, 0.05f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), new Vector3(0f, 0f, -50f), g);
            // The miller, by his door.
            Vector3 mf = feet + Quaternion.Euler(0f, front + 22f, 0f) * Vector3.forward * 5f;
            var miller = Toy(kit, new Vector3(mf.x, 0.05f, mf.z), front + 22f, ToyNpc.Kind.Bunny, "Miller Grist", new Color(0.97f, 0.95f, 0.9f), new Color(0.6f, 0.48f, 0.35f),
                ToyNpc.Instrument.None, 1f, new Color(0.75f, 0.68f, 0.55f));
            miller.Lines = () => new[]
            {
                "Grist's the name, flour's the game! Every loaf in Toybox Town starts as a sack from my mill.",
                "The Fray blew through and spun my sails so fast the whole mill hummed. Shook my best marble right off the gallery. It's still up there, round the back.",
                "Take the steps round the back, and keep well away from the sails. They don't stop for anybody.",
            };
            Vector3 foot = feet + Quaternion.Euler(0f, 140f, 0f) * Vector3.forward * 5.4f;
            kit.Hint(foot + Vector3.up * 1.5f, new Vector3(3f, 3f, 3f), "The Mill Gallery",
                "Plank steps wind up round the back of the mill to a gallery. Keep clear of the sails!");
        }

        // ---------------------------------------------------------------- The Woodcutters' Camp

        // A clearing in the west wood, on the way to the old watchtower: a lean-to, a campfire,
        // a log pile, a chopping block, and Burl, who buried his lucky marble by the stump with
        // the red ribbon and wants someone to have it.
        static void WoodcuttersCamp(LevelKit kit)
        {
            Vector3 c;
            if (!ClearArea(new Vector3(-118f, 0f, 30f), 6f, out c))
            {
                Debug.Log("[Level1] No clear spot in the west wood for the Woodcutters' Camp.");
                return;
            }
            ClearForDepth(kit, new Rect(c.x - 5f, c.z - 5f, 10f, 10f), "the Woodcutters' Camp");
            var g = kit.Group("WoodcuttersCamp", Vector3.zero);
            var bark = PatchworkAssets.Wood(new Color(0.48f, 0.34f, 0.24f));
            var cut = PatchworkAssets.Felt(new Color(0.92f, 0.8f, 0.6f));
            var canvas = PatchworkAssets.Fabric(new Color(0.82f, 0.74f, 0.58f));
            var darkWood = PatchworkAssets.Wood(TownKit.DarkWood);
            kit.Deco(PrimitiveType.Cylinder, c + Vector3.up * 0.02f, new Vector3(9f, 0.02f, 9f), PatchworkAssets.Felt(new Color(0.62f, 0.5f, 0.36f)), default(Vector3), g);
            ClearGrass(c, 5f);
            Campfire(kit, g, c, 1f);
            // Stumps to sit on round the fire.
            for (int k = 0; k < 3; k++)
            {
                float a = 0.6f + k * 2.1f;
                Vector3 s = c + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 2f;
                kit.Deco(PrimitiveType.Cylinder, s + Vector3.up * 0.22f, new Vector3(0.6f, 0.22f, 0.6f), bark, default(Vector3), g);
                kit.Deco(PrimitiveType.Cylinder, s + Vector3.up * 0.45f, new Vector3(0.56f, 0.01f, 0.56f), cut, default(Vector3), g);
            }
            // A lean-to: two posts and a sloping canvas roof, a bedroll under it.
            Vector3 lean = c + new Vector3(-3.4f, 0f, 2.2f);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, lean + new Vector3(s * 1.3f, 0.9f, 0.6f), new Vector3(0.12f, 0.9f, 0.12f), darkWood, default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, lean + new Vector3(0f, 1.05f, 0f), new Vector3(3f, 0.06f, 1.9f), canvas, new Vector3(-35f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Capsule, lean + new Vector3(0f, 0.15f, -0.1f), new Vector3(0.4f, 0.8f, 0.4f), PatchworkAssets.Knit(new Color(0.75f, 0.35f, 0.32f)), new Vector3(0f, 0f, 90f), g);
            Solid(kit, lean + new Vector3(0f, 0.6f, 0f), new Vector3(3f, 1.2f, 1.6f));
            // The log pile: a pyramid of logs with their cut ends showing.
            Vector3 pile = c + new Vector3(3.2f, 0f, 2f);
            for (int row = 0; row < 3; row++)
                for (int i = 0; i < 3 - row; i++)
                {
                    Vector3 p = pile + new Vector3(0f, 0.3f + row * 0.52f, -0.6f + i * 0.6f + row * 0.3f);
                    kit.Deco(PrimitiveType.Cylinder, p, new Vector3(0.55f, 1.2f, 0.55f), bark, new Vector3(0f, 0f, 90f), g);
                    for (int s = -1; s <= 1; s += 2)
                        kit.Deco(PrimitiveType.Cylinder, p + new Vector3(s * 1.2f, 0f, 0f), new Vector3(0.5f, 0.01f, 0.5f), cut, new Vector3(0f, 0f, 90f), g);
                }
            kit.Block(pile + new Vector3(0f, 0.75f, 0f), new Vector3(2.4f, 1.5f, 1.8f), new Color(0.48f, 0.34f, 0.24f), "LogPile", bark).GetComponent<Renderer>().enabled = false;
            // The chopping block with a toy axe in it, and a sawhorse with a half-sawn log.
            Vector3 block = c + new Vector3(2.6f, 0f, -1.8f);
            kit.Deco(PrimitiveType.Cylinder, block + Vector3.up * 0.35f, new Vector3(0.8f, 0.35f, 0.8f), bark, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, block + Vector3.up * 0.71f, new Vector3(0.76f, 0.01f, 0.76f), cut, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, block + new Vector3(0.15f, 1.1f, 0f), new Vector3(0.07f, 0.4f, 0.07f), PatchworkAssets.Wood(new Color(0.85f, 0.65f, 0.4f)), new Vector3(0f, 0f, -25f), g);
            kit.Deco(PrimitiveType.Cube, block + new Vector3(-0.02f, 0.78f, 0f), new Vector3(0.36f, 0.22f, 0.06f), PatchworkAssets.Plain(new Color(0.75f, 0.78f, 0.82f), 0.8f), new Vector3(0f, 0f, -25f), g);
            Solid(kit, block + Vector3.up * 0.35f, new Vector3(0.75f, 0.7f, 0.75f));
            Vector3 horse = c + new Vector3(-0.6f, 0f, -3.4f);
            for (int s = -1; s <= 1; s += 2)
                for (int t = -1; t <= 1; t += 2)
                    kit.Deco(PrimitiveType.Cube, horse + new Vector3(s * 0.6f, 0.4f, t * 0.18f), new Vector3(0.08f, 0.9f, 0.08f), darkWood, new Vector3(t * 14f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Cylinder, horse + Vector3.up * 0.95f, new Vector3(0.4f, 0.9f, 0.4f), bark, new Vector3(0f, 0f, 90f), g);
            kit.Deco(PrimitiveType.Cube, horse + new Vector3(0.3f, 1.25f, 0f), new Vector3(0.02f, 0.25f, 0.7f), PatchworkAssets.Plain(new Color(0.75f, 0.78f, 0.82f), 0.8f), new Vector3(0f, 0f, 10f), g);
            // The ribboned stump, and the glinting ground beside it.
            Vector3 stump = c + new Vector3(-3.2f, 0f, -2.6f);
            kit.Deco(PrimitiveType.Cylinder, stump + Vector3.up * 0.3f, new Vector3(0.9f, 0.3f, 0.9f), bark, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, stump + Vector3.up * 0.61f, new Vector3(0.86f, 0.01f, 0.86f), cut, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, stump + Vector3.up * 0.4f, new Vector3(0.94f, 0.05f, 0.94f), PatchworkAssets.Felt(new Color(0.85f, 0.25f, 0.28f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, stump + new Vector3(0.48f, 0.3f, 0f), new Vector3(0.05f, 0.35f, 0.12f), PatchworkAssets.Felt(new Color(0.85f, 0.25f, 0.28f)), new Vector3(0f, 0f, 15f), g);
            Solid(kit, stump + Vector3.up * 0.3f, new Vector3(0.85f, 0.6f, 0.85f));
            Vector3 dig = stump + new Vector3(1.4f, 0f, -0.4f);
            DigSpot.Place(kit, dig, SecretMarble.Place(kit, dig + Vector3.up * 1.3f, MarbleIds[22], MarbleColor(22), true));
            // Burl.
            Vector3 bf = c + new Vector3(1.7f, 0.05f, 1.3f);
            var burl = Toy(kit, bf, Mathf.Atan2(-1.7f, -1.3f) * Mathf.Rad2Deg, ToyNpc.Kind.Teddy, "Woodcutter Burl", new Color(0.62f, 0.42f, 0.3f), new Color(0.72f, 0.3f, 0.28f),
                ToyNpc.Instrument.None, 0.75f, new Color(0.55f, 0.35f, 0.25f));
            burl.Lines = () => new[]
            {
                "Ho there! Burl's the name. I cut the wood that builds the town, and plant two saplings for every tree I fell.",
                "The old watchtower further west was built when the first toys came. There are steps round the outside, if your legs are springy.",
                "I buried my lucky marble by the stump with the red ribbon, so the Fray wouldn't find it. You have it. Grab where the ground glints, and dig.",
            };
            kit.Info.MapPins.Add(new MapPin("Woodcutters' Camp", c));
        }

        // ---------------------------------------------------------------- Down the river

        // A line of bubbles weaving down the River Weft from the canal's end, for anyone who'd
        // rather swim than walk.
        static void RiverTrail(LevelKit kit)
        {
            var path = RiverSouth;
            float along = 0f, next = 24f;
            int placed = 0;
            const int count = 24;
            for (int i = 1; i < path.Length && placed < count; i++)
            {
                Vector2 a = path[i - 1], b = path[i];
                float seg = (b - a).magnitude;
                if (seg < 0.001f) continue;
                Vector2 d = (b - a) / seg, n = new Vector2(-d.y, d.x);
                while (along + seg >= next && placed < count)
                {
                    Vector2 p = Vector2.Lerp(a, b, (next - along) / seg) + n * Mathf.Sin(next / 7f) * 1.6f;
                    ScoreBubbles.Place(kit, new Vector3(p.x, CanalSurface + 0.45f, p.y), 10);
                    placed++;
                    next += 4.5f;
                }
                along += seg;
            }
        }

        // ---------------------------------------------------------------- The Lookout Oak

        // A giant old oak on the meadow east of where the Pod lands. Plank steps wind up round
        // its trunk to a crow's nest under a striped awning, with a telescope pointed at the
        // town, a sea chest, and Lookout Lark, who sees everything first.
        static void LookoutOak(LevelKit kit)
        {
            Vector3 t;
            if (!ClearArea(new Vector3(130f, 0f, -133f), 6.5f, out t))
            {
                Debug.Log("[Level1] No clear spot by the Pod's meadow for the Lookout Oak.");
                return;
            }
            ClearForDepth(kit, new Rect(t.x - 6.5f, t.z - 6.5f, 13f, 13f), "the Lookout Oak");
            var g = kit.Group("LookoutOak", Vector3.zero);
            var bark = PatchworkAssets.Wood(new Color(0.5f, 0.36f, 0.26f));
            var plankCol = new Color(0.72f, 0.55f, 0.38f);
            var planks = PatchworkAssets.Planks(plankCol);
            var darkWood = PatchworkAssets.Wood(TownKit.DarkWood);
            const float trunkH = 13f, deckY = 8f;
            // The trunk, a little narrower as it goes up, on a spread of roots.
            for (int i = 0; i < 3; i++)
            {
                float r = 1.5f - i * 0.2f;
                kit.Deco(PrimitiveType.Cylinder, t + Vector3.up * (trunkH / 6f + i * trunkH / 3f), new Vector3(r * 2f, trunkH / 6f, r * 2f), bark, default(Vector3), g);
            }
            for (int k = 0; k < 6; k++)
            {
                float a = k * 60f + 15f;
                kit.Deco(PrimitiveType.Sphere, t + Quaternion.Euler(0f, a, 0f) * Vector3.forward * 1.5f + Vector3.up * 0.15f, new Vector3(0.8f, 0.5f, 1.8f), bark, new Vector3(0f, a, 0f), g);
            }
            var trunk = new GameObject("OakTrunk");
            trunk.transform.SetParent(kit.Root, false);
            trunk.transform.position = t;
            var col = trunk.AddComponent<CapsuleCollider>();
            col.radius = 1.35f;
            col.height = trunkH;
            col.center = new Vector3(0f, trunkH * 0.5f, 0f);
            // Ten steps round the trunk: from the east, round by the south and west, to the
            // north, where the last one comes up level with the deck.
            for (int i = 1; i <= 10; i++)
            {
                float a = -(10 - i) * 30f, top = 0.8f * i;
                Vector3 dir = Quaternion.Euler(0f, a, 0f) * Vector3.forward;
                Vector3 p = t + dir * 2.4f;
                var step = kit.Block(new Vector3(p.x, top - 0.125f, p.z), new Vector3(1.3f, 0.25f, 1.6f), plankCol, "OakStep", planks);
                step.rotation = Quaternion.Euler(0f, a, 0f);
                kit.Deco(PrimitiveType.Cube, t + dir * 1.9f + Vector3.up * (top - 0.4f), new Vector3(0.14f, 0.5f, 0.9f), darkWood, new Vector3(-25f, a, 0f), g);
                if (i % 3 == 0) ScoreBubbles.Place(kit, new Vector3(p.x, top + 1f, p.z), 10);
            }
            // The deck, east of the trunk, on joists, with rails round three sides.
            Vector3 deck = t + new Vector3(3.3f, 0f, 0f);
            kit.Block(deck + Vector3.up * (deckY - 0.125f), new Vector3(5.2f, 0.25f, 5.6f), plankCol, "OakDeck", planks);
            for (int k = 0; k < 4; k++)
                kit.Deco(PrimitiveType.Cube, deck + new Vector3(k * 0.9f - 1.35f, deckY - 0.5f, 0f), new Vector3(0.18f, 0.6f, 5.4f), darkWood, default(Vector3), g);
            for (int k = 0; k <= 6; k++)
            {
                float z = -2.7f + k * 0.9f;
                kit.Deco(PrimitiveType.Cylinder, deck + new Vector3(2.5f, deckY + 0.45f, z), new Vector3(0.1f, 0.45f, 0.1f), darkWood, default(Vector3), g);
            }
            kit.Deco(PrimitiveType.Cube, deck + new Vector3(2.5f, deckY + 0.9f, 0f), new Vector3(0.08f, 0.08f, 5.5f), darkWood, default(Vector3), g);
            for (int s = -1; s <= 1; s += 2)
            {
                kit.Deco(PrimitiveType.Cube, deck + new Vector3(0.6f, deckY + 0.9f, s * 2.7f), new Vector3(3.8f, 0.08f, 0.08f), darkWood, default(Vector3), g);
                for (int k = 0; k < 4; k++)
                    kit.Deco(PrimitiveType.Cylinder, deck + new Vector3(-1.1f + k * 1.2f, deckY + 0.45f, s * 2.7f), new Vector3(0.1f, 0.45f, 0.1f), darkWood, default(Vector3), g);
            }
            // A striped awning on four posts, a pennant above it.
            var stripes = PatchworkAssets.Print(PatchworkAssets.PrintKind.Stripes, new Color(0.97f, 0.94f, 0.88f), new Color(0.3f, 0.55f, 0.78f));
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    kit.Deco(PrimitiveType.Cylinder, deck + new Vector3(0.4f + sx * 2f, deckY + 1.35f, sz * 2.4f), new Vector3(0.12f, 1.35f, 0.12f), darkWood, default(Vector3), g);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, deck + new Vector3(0.4f, deckY + 3.05f, s * 1.3f), new Vector3(4.8f, 0.08f, 2.9f), stripes, new Vector3(s * 22f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Cylinder, deck + new Vector3(2.6f, deckY + 4.2f, 0f), new Vector3(0.08f, 1.6f, 0.08f), darkWood, default(Vector3), g);
            var flag = kit.Group("OakPennant", deck + new Vector3(2.6f, deckY + 5.4f, 0f));
            flag.SetParent(g, true);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.7f, 0f, 0f), new Vector3(1.4f, 0.6f, 0.04f), PatchworkAssets.Felt(new Color(0.95f, 0.75f, 0.3f)), default(Vector3), flag);
            flag.gameObject.AddComponent<FlagWave>();
            // A telescope on a tripod, pointed at the town.
            Vector3 scope = deck + new Vector3(1.6f, deckY, -1.4f);
            Vector3 toTown = new Vector3(MainGate.x - scope.x, 0f, MainGate.z - scope.z);
            float scopeYaw = Mathf.Atan2(toTown.x, toTown.z) * Mathf.Rad2Deg;
            for (int k = 0; k < 3; k++)
                kit.Deco(PrimitiveType.Cylinder, scope + Quaternion.Euler(0f, k * 120f, 0f) * new Vector3(0f, 0.55f, 0.25f), new Vector3(0.05f, 0.6f, 0.05f), darkWood, new Vector3(-15f, k * 120f, 0f), g);
            kit.Deco(PrimitiveType.Cylinder, scope + Vector3.up * 1.25f, new Vector3(0.2f, 0.55f, 0.2f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f), new Vector3(80f, scopeYaw, 0f), g);
            kit.Deco(PrimitiveType.Cylinder, scope + Vector3.up * 1.25f + Quaternion.Euler(0f, scopeYaw, 0f) * new Vector3(0f, 0.1f, 0.5f), new Vector3(0.26f, 0.06f, 0.26f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f), new Vector3(80f, scopeYaw, 0f), g);
            // The sea chest, with the marble on its lid.
            Vector3 chest = deck + new Vector3(-0.6f, deckY, -2f);
            kit.Block(chest + Vector3.up * 0.3f, new Vector3(1.1f, 0.6f, 0.7f), new Color(0.55f, 0.35f, 0.25f), "OakChest", PatchworkAssets.Planks(new Color(0.55f, 0.35f, 0.25f)));
            kit.Deco(PrimitiveType.Cylinder, chest + Vector3.up * 0.6f, new Vector3(0.7f, 0.55f, 0.7f), PatchworkAssets.Planks(new Color(0.55f, 0.35f, 0.25f)), new Vector3(0f, 0f, 90f), g);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cube, chest + new Vector3(s * 0.4f, 0.5f, 0f), new Vector3(0.08f, 0.62f, 0.74f), PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.7f), default(Vector3), g);
            SecretMarble.Place(kit, chest + Vector3.up * 1.4f, MarbleIds[23], MarbleColor(23));
            // The canopy: big knitted pompoms of leaves, with felt blossom.
            Color[] leaf = { new Color(0.45f, 0.68f, 0.4f), new Color(0.52f, 0.74f, 0.42f), new Color(0.4f, 0.62f, 0.38f) };
            Vector3[] puffs = { new Vector3(0f, 14.6f, 0f), new Vector3(-2.6f, 12.3f, -1.6f), new Vector3(-1.6f, 12.8f, 2.2f), new Vector3(1f, 13.4f, -3f), new Vector3(1.6f, 14f, 2.6f) };
            float[] size = { 7.5f, 4.6f, 4.2f, 4f, 4.2f };
            for (int i = 0; i < puffs.Length; i++)
            {
                kit.Deco(PrimitiveType.Sphere, t + puffs[i], new Vector3(size[i], size[i] * 0.7f, size[i]), PatchworkAssets.Knit(leaf[i % leaf.Length]), new Vector3(0f, i * 40f, 0f), g);
                for (int b = 0; b < 4; b++)
                {
                    Vector3 o = Quaternion.Euler(0f, b * 90f + i * 25f, 0f) * new Vector3(size[i] * 0.48f, size[i] * 0.12f, 0f);
                    kit.Deco(PrimitiveType.Sphere, t + puffs[i] + o, Vector3.one * 0.35f, PatchworkAssets.Felt(FlowerColors[(i + b) % FlowerColors.Length]), default(Vector3), g);
                }
            }
            // A tyre swing hung from the deck's east edge.
            var swing = kit.Group("TyreSwing", deck + new Vector3(2.2f, deckY - 0.25f, 0f));
            swing.SetParent(g, true);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -3.3f, 0f), new Vector3(0.03f, 3.3f, 0.03f), PatchworkAssets.Yarn(new Color(0.9f, 0.8f, 0.55f)), default(Vector3), swing);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, -6.9f, 0f), new Vector3(1f, 0.16f, 1f), PatchworkAssets.Plain(new Color(0.2f, 0.2f, 0.24f), 0.3f), new Vector3(90f, 0f, 0f), swing);
            var sway = swing.gameObject.AddComponent<Rocker>();
            sway.Axis = Vector3.forward;
            sway.Amount = 4f;
            sway.Speed = 0.7f;
            ClearGrass(t, 4.5f);
            // Lookout Lark, watching the town.
            var lark = Toy(kit, deck + new Vector3(1.4f, deckY + 0.05f, 1.2f), scopeYaw, ToyNpc.Kind.Bird, "Lookout Lark", new Color(0.45f, 0.62f, 0.85f), new Color(0.98f, 0.85f, 0.5f),
                ToyNpc.Instrument.None, 1.4f, new Color(0.35f, 0.5f, 0.75f));
            lark.Lines = () => new[]
            {
                "Ahoy! Lookout Lark, eyes of the meadow! I saw your Pod come down before anyone. Big bump. Lovely landing.",
                "Through the telescope you can see the whole town: the keep, the chapel, the canal, the Common Quarter. On a clear day, the edge of the box.",
                "When the Fray come over the hills, I see them first and ring... well, I shout. I'm getting a bell. Probably.",
                "That chest's for anyone who climbs all the way up. Help yourself.",
            };
            kit.Hint(t + new Vector3(2.4f, 1.5f, 0f), new Vector3(3f, 3f, 3f), "The Lookout Oak",
                "Steps wind up round the old oak to a crow's nest. You can see the whole toybox from up there.");
            kit.Info.MapPins.Add(new MapPin("Lookout Oak", t));
        }

        // ---------------------------------------------------------------- The Wishing Well

        // Gate Hamlet's well grants wishes, they say, to the right sort of visitor: something
        // runny can slip down it to the bottom, where Wellbert keeps every wish anyone ever
        // dropped. A marble somebody dropped in by mistake sits on his top ledge.
        static void WishingWell(LevelKit kit)
        {
            // (Where Hamlet put the well; it's only there if nothing was in the way.)
            var road = RoadIn;
            int wi = Mathf.Clamp(Mathf.RoundToInt(road.Length * 0.8f), 1, road.Length - 2);
            Vector2 w = road[wi];
            Vector2 wd = (road[Mathf.Clamp(Mathf.RoundToInt(road.Length * 0.8f) + 1, 1, road.Length - 1)] - w).normalized;
            Vector2 wq = w + new Vector2(-wd.y, wd.x) * -5.5f;
            var wellAt = new Vector3(wq.x, 0f, wq.y);
            bool found = false;
            for (int i = 0; i < kit.Root.childCount && !found; i++)
            {
                var c = kit.Root.GetChild(i);
                if (c.name == "Well" && c.gameObject.activeSelf && (c.position - wellAt).sqrMagnitude < 0.1f) found = true;
            }
            if (!found)
            {
                Debug.Log("[Level1] Gate Hamlet has no well, so no way down to the Wishing Well.");
                return;
            }
            Vector3 o = WellRoom;
            var size = new Vector3(12f, 7f, 12f);
            var down = WarpAt(kit, wellAt + new Vector3(0f, 1.45f, 0f), new Vector3(1.3f, 0.9f, 1.3f), o + new Vector3(-3.6f, 0f, -3.6f), 45f, false, "");
            down.WaterOnly = true;
            kit.Hint(wellAt + new Vector3(0f, 1.5f, 0f), new Vector3(4.5f, 3f, 4.5f), "The Wishing Well",
                "Folk say if you drop in a wish, something drops out. Only something runny could fit down it, though.");

            Room(kit, o, size, PatchworkAssets.Felt(new Color(0.45f, 0.55f, 0.42f)), PatchworkAssets.Cobble(new Color(0.62f, 0.62f, 0.6f)),
                PatchworkAssets.Cobble(new Color(0.5f, 0.5f, 0.5f)), "WellBottom");
            var g = kit.Group("WellBottom", Vector3.zero);
            // A shaft of daylight down the middle, onto a shallow pool full of wishes.
            kit.Deco(PrimitiveType.Cylinder, o + new Vector3(0f, size.y * 0.5f, 0f), new Vector3(2.2f, size.y * 0.5f, 2.2f), PatchworkAssets.Bubble(new Color(1f, 0.97f, 0.85f, 0.18f)), default(Vector3), g)
                .GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            kit.Deco(PrimitiveType.Cylinder, o + new Vector3(0f, size.y - 0.02f, 0f), new Vector3(2.2f, 0.02f, 2.2f), PatchworkAssets.Glow(new Color(0.85f, 0.93f, 1f), 1.2f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, o + new Vector3(0f, 0.02f, 0f), new Vector3(5f, 0.02f, 5f), PatchworkAssets.Bubble(new Color(0.45f, 0.7f, 0.95f, 0.75f)), default(Vector3), g);
            var gold = PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f);
            var rng = new System.Random(31);
            for (int i = 0; i < 30; i++)
            {
                float a = (float)(rng.NextDouble() * Mathf.PI * 2.0), r = (float)(rng.NextDouble() * 2.2);
                kit.Deco(PrimitiveType.Cylinder, o + new Vector3(Mathf.Cos(a) * r, 0.045f, Mathf.Sin(a) * r), new Vector3(0.22f, 0.01f, 0.22f), gold, new Vector3(0f, 0f, (float)(rng.NextDouble() * 20.0 - 10.0)), g);
            }
            WarmLight(g, o + new Vector3(0f, 4f, 0f), 12f, 1.1f, new Color(0.85f, 0.92f, 1f));
            // Ferns and moss on the walls, and a few lost wishes: a toy boat, a ball, a key.
            var moss = PatchworkAssets.Felt(new Color(0.4f, 0.6f, 0.36f));
            for (int i = 0; i < 10; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * (size.x * 0.5f - 0.15f);
                kit.Deco(PrimitiveType.Sphere, o + new Vector3(x, 0.8f + (i * 0.73f) % 4.5f, -4.5f + i), new Vector3(0.3f, 0.9f, 1.2f), moss, default(Vector3), g);
            }
            kit.Deco(PrimitiveType.Cube, o + new Vector3(1.2f, 0.12f, -1.3f), new Vector3(0.7f, 0.18f, 0.3f), PatchworkAssets.Felt(new Color(0.85f, 0.3f, 0.3f)), new Vector3(0f, 30f, 0f), g);
            kit.Deco(PrimitiveType.Cube, o + new Vector3(1.2f, 0.4f, -1.3f), new Vector3(0.02f, 0.4f, 0.3f), PatchworkAssets.Felt(new Color(0.98f, 0.96f, 0.9f)), new Vector3(0f, 30f, 0f), g);
            kit.Deco(PrimitiveType.Sphere, o + new Vector3(-1.4f, 0.2f, 1.1f), Vector3.one * 0.4f, PatchworkAssets.Felt(new Color(0.4f, 0.6f, 0.95f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, o + new Vector3(0.6f, 0.05f, 1.6f), new Vector3(0.5f, 0.04f, 0.12f), gold, new Vector3(0f, 70f, 0f), g);
            // Ledges up the walls, stone by stone, to the top ledge.
            Vector3[] ledges = { new Vector3(4.6f, 1f, 3f), new Vector3(3.4f, 2f, 4.8f), new Vector3(1.2f, 3f, 4.8f), new Vector3(-1f, 4f, 4.8f), new Vector3(-3.6f, 4.8f, 4.8f) };
            for (int i = 0; i < ledges.Length; i++)
            {
                var l = ledges[i];
                bool last = i == ledges.Length - 1;
                kit.Block(o + new Vector3(l.x, l.y - 0.15f, l.z), new Vector3(last ? 2.6f : 1.5f, 0.3f, last ? 2.2f : 1.5f), new Color(0.62f, 0.62f, 0.6f), "WellLedge",
                    PatchworkAssets.Cobble(new Color(0.66f, 0.65f, 0.62f)));
                kit.Deco(PrimitiveType.Cube, o + new Vector3(l.x, l.y - 0.45f, l.z + 0.3f), new Vector3(0.5f, 0.3f, 0.9f), PatchworkAssets.Cobble(new Color(0.55f, 0.55f, 0.53f)), default(Vector3), g);
            }
            SecretMarble.Place(kit, o + new Vector3(-3.6f, 5.8f, 4.9f), MarbleIds[24], MarbleColor(24));
            // The bucket, on its rope down the shaft: ride it back up.
            Vector3 bucket = o + new Vector3(0f, 0f, 0f);
            kit.Deco(PrimitiveType.Cylinder, bucket + Vector3.up * (size.y * 0.5f + 0.7f), new Vector3(0.04f, size.y * 0.5f - 0.7f, 0.04f), PatchworkAssets.Yarn(new Color(0.9f, 0.8f, 0.55f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, bucket + Vector3.up * 0.75f, new Vector3(0.8f, 0.35f, 0.8f), PatchworkAssets.Planks(new Color(0.6f, 0.42f, 0.28f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, bucket + Vector3.up * 1.08f, new Vector3(0.84f, 0.03f, 0.84f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cube, bucket + Vector3.up * 1.35f, new Vector3(0.8f, 0.04f, 0.04f), PatchworkAssets.Plain(TownKit.Iron, 0.5f), new Vector3(0f, 0f, 0f), g);
            Solid(kit, bucket + Vector3.up * 0.55f, new Vector3(0.75f, 1.1f, 0.75f));
            Vector3 exit;
            if (!FindSpot(wellAt + new Vector3(wd.x, 0f, wd.y) * 2.4f, 0.5f, out exit)) exit = wellAt + new Vector3(wd.x, 0f, wd.y) * 2.4f;
            WarpAt(kit, bucket + new Vector3(0f, 1.2f, -0.9f), new Vector3(1.8f, 2.4f, 1.2f), exit, Mathf.Atan2(wd.x, wd.y) * Mathf.Rad2Deg, true, "Ride the bucket up");
            var wellbert = Toy(kit, o + new Vector3(3.2f, 0.05f, -2.6f), -60f, ToyNpc.Kind.JackInBox, "Wellbert", new Color(0.5f, 0.72f, 0.9f), new Color(0.98f, 0.82f, 0.4f),
                ToyNpc.Instrument.None, 1.25f, new Color(0.4f, 0.6f, 0.85f));
            wellbert.Lines = () => new[]
            {
                "Boing! A visitor! Nobody visits the bottom of a well. They just drop wishes on my head.",
                "I keep every one. That's a wish for a pony, that's a wish for rain, and that one's for a better hat.",
                "Somebody wished for a marble once, then dropped it in by mistake. It's up on the top ledge. You have it; it was never really wished for.",
                "Ride the bucket back up when you're done. Mind your head on the way.",
            };
            kit.Hint(o + new Vector3(-3.6f, 1.5f, -3.6f), new Vector3(4f, 3f, 4f), "The Bottom of the Well",
                "Full of wishes, and somebody who keeps them. The bucket in the middle rides back up.");
        }

        // ---------------------------------------------------------------- The Lily Pad Run

        // Upstream from the Weft Ford, a line of lily pads weaves out along the river to a sandy
        // island with a little willow, where Tilda Tench's lucky float came to rest (and
        // something shinier besides). Hop out from the middle of the ford.
        static void LilyPadRun(LevelKit kit)
        {
            var path = RiverNorth;
            var near = new Vector2(44f, 141f);   // (as Countryside's Ford)
            int ni = 1;
            for (int i = 1; i < path.Length - 1; i++)
                if ((path[i] - near).sqrMagnitude < (path[ni] - near).sqrMagnitude) ni = i;
            var lengths = new float[path.Length];
            for (int i = 1; i < path.Length; i++) lengths[i] = lengths[i - 1] + Vector2.Distance(path[i - 1], path[i]);
            float total = lengths[path.Length - 1], ford = lengths[ni];
            System.Func<float, Vector2> at = s =>
            {
                s = Mathf.Clamp(s, 0f, total);
                int i = 1;
                while (i < path.Length - 1 && lengths[i] < s) i++;
                float seg = lengths[i] - lengths[i - 1];
                return Vector2.Lerp(path[i - 1], path[i], seg > 0f ? (s - lengths[i - 1]) / seg : 0f);
            };
            System.Func<float, Vector2> side = s =>
            {
                Vector2 d = (at(s + 0.5f) - at(s - 0.5f)).normalized;
                return new Vector2(-d.y, d.x);
            };
            var green = PatchworkAssets.Felt(new Color(0.42f, 0.66f, 0.38f));
            var paler = PatchworkAssets.Felt(new Color(0.56f, 0.76f, 0.46f));
            var g = kit.Group("LilyPadRun", Vector3.zero);
            float top = CanalSurface + 0.08f;
            for (int k = 0; k < 10; k++)
            {
                float s = ford - 2.4f - k * 2f;   // (upstream: the river flows out of the box, away from the town)
                Vector2 q = at(s) + side(s) * 1.8f * Mathf.Sin(k * 0.8f);
                float bottom = MeadowHeight(q.x, q.y) - 0.3f;
                var pad = kit.Block(new Vector3(q.x, (top + bottom) * 0.5f, q.y), new Vector3(1.2f, top - bottom, 1.2f), new Color(0.42f, 0.66f, 0.38f), "LilyPad");
                pad.GetComponent<Renderer>().enabled = false;
                kit.Deco(PrimitiveType.Cylinder, new Vector3(q.x, top - 0.03f, q.y), new Vector3(1.45f, 0.03f, 1.45f), k % 2 == 0 ? green : paler, new Vector3(0f, k * 47f, 0f), g);
                kit.Deco(PrimitiveType.Cube, new Vector3(q.x, top + 0.005f, q.y) + Quaternion.Euler(0f, k * 47f, 0f) * new Vector3(0f, 0f, 0.45f), new Vector3(0.06f, 0.02f, 0.55f),
                    PatchworkAssets.Felt(new Color(0.3f, 0.5f, 0.3f)), new Vector3(0f, k * 47f, 0f), g);
                if (k % 3 == 1)
                {
                    Vector3 f = new Vector3(q.x, top + 0.1f, q.y) + Quaternion.Euler(0f, k * 47f + 120f, 0f) * new Vector3(0f, 0f, 0.4f);
                    for (int p = 0; p < 5; p++)
                        kit.Deco(PrimitiveType.Sphere, f + Quaternion.Euler(0f, p * 72f, 0f) * new Vector3(0f, 0f, 0.1f), new Vector3(0.12f, 0.08f, 0.2f), PatchworkAssets.Felt(new Color(1f, 0.72f, 0.8f)), new Vector3(0f, p * 72f, 0f), g);
                    kit.Deco(PrimitiveType.Sphere, f + Vector3.up * 0.03f, Vector3.one * 0.09f, PatchworkAssets.Felt(new Color(1f, 0.88f, 0.4f)), default(Vector3), g);
                }
                if (k % 3 == 2) ScoreBubbles.Place(kit, new Vector3(q.x, top + 1f, q.y), 10);
            }
            // The island: a sandbar with a willow, an old stump and the lost float.
            float si = ford - 2.4f - 9f * 2f - 3.2f;
            Vector2 c2 = at(si);
            Vector3 isle = new Vector3(c2.x, 0f, c2.y);
            float isleTop = 0.15f, isleBottom = MeadowHeight(c2.x, c2.y) - 0.3f;
            var bar = kit.Block(new Vector3(isle.x, (isleTop + isleBottom) * 0.5f, isle.z), new Vector3(3.4f, isleTop - isleBottom, 3.4f), new Color(0.9f, 0.82f, 0.6f), "WillowIsland");
            bar.rotation = Quaternion.Euler(0f, 30f, 0f);
            bar.GetComponent<Renderer>().enabled = false;
            kit.Deco(PrimitiveType.Sphere, isle + Vector3.up * (isleTop - 0.3f), new Vector3(4.4f, 0.6f, 4.1f), PatchworkAssets.Felt(new Color(0.92f, 0.84f, 0.62f)), new Vector3(0f, 30f, 0f), g);
            kit.Deco(PrimitiveType.Sphere, isle + new Vector3(0.6f, isleTop - 0.2f, 0.7f), new Vector3(2.2f, 0.5f, 2f), PatchworkAssets.Felt(new Color(0.55f, 0.75f, 0.45f)), default(Vector3), g);
            kit.YarnTree(isle + new Vector3(0.7f, isleTop, 0.8f), 3.4f, new Color(0.62f, 0.8f, 0.5f));
            var bark = PatchworkAssets.Wood(new Color(0.48f, 0.34f, 0.24f));
            kit.Deco(PrimitiveType.Cylinder, isle + new Vector3(-0.7f, isleTop + 0.3f, -0.5f), new Vector3(0.6f, 0.3f, 0.6f), bark, default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, isle + new Vector3(-0.7f, isleTop + 0.61f, -0.5f), new Vector3(0.56f, 0.01f, 0.56f), PatchworkAssets.Felt(new Color(0.92f, 0.8f, 0.6f)), default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, isle + new Vector3(0.9f, isleTop + 0.1f, -1.2f), new Vector3(0.3f, 0.3f, 0.3f), PatchworkAssets.Plain(new Color(0.9f, 0.25f, 0.25f), 0.7f), default(Vector3), g);
            kit.Deco(PrimitiveType.Sphere, isle + new Vector3(0.9f, isleTop + 0.24f, -1.2f), new Vector3(0.28f, 0.16f, 0.28f), PatchworkAssets.Plain(new Color(0.98f, 0.97f, 0.95f), 0.7f), default(Vector3), g);
            kit.Deco(PrimitiveType.Cylinder, isle + new Vector3(0.9f, isleTop + 0.45f, -1.2f), new Vector3(0.03f, 0.15f, 0.03f), PatchworkAssets.Plain(new Color(0.9f, 0.25f, 0.25f), 0.7f), default(Vector3), g);
            SecretMarble.Place(kit, isle + new Vector3(-0.7f, isleTop + 1.5f, -0.5f), MarbleIds[25], MarbleColor(25));
            // Tilda, on the bank by the ford with her rod and bucket.
            Vector2 fp = path[ni], fn = side(ford);
            float fhw = RiverHalfWidth(total > 0f ? ford / total : 0f);
            Vector2 tq = fp + fn * (fhw + 2.2f) - (at(ford + 1f) - at(ford - 1f)).normalized * 2.5f;
            Vector3 tilda;
            if (FindSpot(new Vector3(tq.x, 0f, tq.y), 0.6f, out tilda))
            {
                float faceYaw = Mathf.Atan2(-fn.x, -fn.y) * Mathf.Rad2Deg;
                var npc = Toy(kit, tilda + Vector3.up * 0.05f, faceYaw, ToyNpc.Kind.RagDoll, "Tilda Tench", new Color(0.4f, 0.55f, 0.7f), new Color(0.95f, 0.88f, 0.7f),
                    ToyNpc.Instrument.None, 1.1f, new Color(0.35f, 0.5f, 0.6f));
                npc.Lines = () => new[]
                {
                    "Shh! You'll scare the fish. Not that there are any. They're all felt, and they don't bite.",
                    "See the little island upstream, with the willow? My lucky float washed up out there. Something shinier too, by the glint of it.",
                    "Hop out along the lily pads from the middle of the ford. They'll hold you up fine.",
                };
                var rodRot = Quaternion.Euler(0f, faceYaw, 0f);
                kit.Deco(PrimitiveType.Cylinder, tilda + rodRot * new Vector3(0.5f, 1.2f, 1f), new Vector3(0.05f, 1.4f, 0.05f), PatchworkAssets.Wood(new Color(0.7f, 0.55f, 0.35f)), new Vector3(55f, faceYaw, 0f), g);
                kit.Deco(PrimitiveType.Cylinder, tilda + rodRot * new Vector3(-0.6f, 0.25f, 0.2f), new Vector3(0.5f, 0.25f, 0.5f), PatchworkAssets.Plain(new Color(0.55f, 0.6f, 0.68f), 0.6f), default(Vector3), g);
            }
            kit.Hint(isle + new Vector3(0f, 1.5f, 0f), new Vector3(4f, 3f, 4f), "Willow Island", "A sandbar in the river, a willow, and a lost fishing float.");
        }

        // ---------------------------------------------------------------- The Harvest Rush

        // The cabbage field beside the King's Road, south of the town (if Fields could fit it
        // in): step on the stopwatch at the gap in the hedge, then run the furrows up and down,
        // catching every bubble before Farmer Furrow's time is up.
        static void HarvestRush(LevelKit kit)
        {
            var f = new Vector4(112f, -104f, 138f, -86f);   // (Fields' first plot)
            var c = new Vector3((f.x + f.z) * 0.5f, 0f, (f.y + f.w) * 0.5f);
            bool found = false;
            for (int i = 0; i < kit.Root.childCount && !found; i++)
            {
                var t = kit.Root.GetChild(i);
                if (t.name == "FieldSoil" && t.gameObject.activeSelf && Mathf.Abs(t.position.x - c.x) < 0.1f && Mathf.Abs(t.position.z - c.z) < 0.1f) found = true;
            }
            if (!found) return;
            Vector3 btn = new Vector3(f.x - 1.6f, 0f, f.w - 2f);   // just outside the gap in the west hedge
            var rush = kit.Volume<BubbleRush>(btn + Vector3.up * 0.6f, new Vector3(1.8f, 1.4f, 1.8f), "BubbleRush");
            rush.Title = "The Harvest Rush";
            rush.Duration = 34f;
            rush.Button = StopwatchButton(kit, btn, rush.transform);
            var purple = PatchworkAssets.Glow(new Color(0.75f, 0.5f, 1f), 0.8f);
            // Up and down the furrows (between the rows), from the gap to the far corner.
            for (int lane = 0; lane < 4; lane++)
            {
                float z = f.w - 2.4f - lane * 4.4f;
                for (int k = 0; k < 5; k++)
                {
                    float u = (k + 0.5f) / 5f;
                    float x = lane % 2 == 0 ? Mathf.Lerp(f.x + 2f, f.z - 2f, u) : Mathf.Lerp(f.z - 2f, f.x + 2f, u);
                    RushBubbleAt(kit, rush, new Vector3(x, 1.2f, z), purple);
                }
            }
            rush.Marble = SecretMarble.Place(kit, btn + new Vector3(0f, 2.2f, 0f), MarbleIds[26], MarbleColor(26), true);
            kit.Hint(btn + new Vector3(-1.6f, 1.5f, 0f), new Vector3(2.4f, 3f, 4f), "The Harvest Rush",
                "Step on the stopwatch, then run the furrows up and down and catch every bubble before the time's up!");
            // A barrow of cabbages by the gap, and the farmer.
            Vector3 barrow = btn + new Vector3(-0.4f, 0f, -2.6f);
            var wood = PatchworkAssets.Wood(new Color(0.62f, 0.44f, 0.3f));
            var g = kit.Group("HarvestBarrow", Vector3.zero);
            kit.Deco(PrimitiveType.Cube, barrow + new Vector3(0f, 0.6f, 0f), new Vector3(1.1f, 0.5f, 0.8f), wood, new Vector3(0f, 0f, -8f), g);
            kit.Deco(PrimitiveType.Cylinder, barrow + new Vector3(0.65f, 0.3f, 0f), new Vector3(0.6f, 0.05f, 0.6f), PatchworkAssets.Wood(TownKit.DarkWood), new Vector3(90f, 0f, 0f), g);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, barrow + new Vector3(-0.9f, 0.55f, s * 0.3f), new Vector3(0.06f, 0.4f, 0.06f), wood, new Vector3(0f, 0f, 75f), g);
            for (int k = 0; k < 4; k++)
                kit.Deco(PrimitiveType.Sphere, barrow + new Vector3(-0.3f + (k % 2) * 0.55f, 0.95f + (k / 2) * 0.2f, (k % 2 == 0 ? -0.15f : 0.15f)), new Vector3(0.5f, 0.42f, 0.5f), PatchworkAssets.Felt(new Color(0.55f, 0.78f, 0.45f)), default(Vector3), g);
            Solid(kit, barrow + Vector3.up * 0.5f, new Vector3(1.4f, 1f, 0.9f));
            Vector3 fp;
            if (FindSpot(btn + new Vector3(-1.4f, 0f, 2.4f), 0.6f, out fp))
            {
                var farmer = Toy(kit, fp + Vector3.up * 0.05f, 110f, ToyNpc.Kind.Teddy, "Farmer Furrow", new Color(0.78f, 0.6f, 0.42f), new Color(0.45f, 0.6f, 0.38f),
                    ToyNpc.Instrument.None, 0.85f, new Color(0.55f, 0.5f, 0.35f));
                farmer.Lines = () => new[]
                {
                    "Furrow's the name, cabbages are the game. Best cabbages in the box, and the only ones, if I'm honest.",
                    "Harvest's in, and the bubbles have got into the rows again. Step on the stopwatch and clear the furrows for me, quick as you can.",
                    "Up one furrow, down the next, all the way to the far hedge. Beat the clock and there's a marble for your trouble.",
                };
            }
        }

        // ---------------------------------------------------------------- Crier Hollerby

        // The town crier in Gate Square, with a rumour for every one of the places above.
        static void TownCrier(LevelKit kit)
        {
            Vector3 spot;
            if (!FindSpot(new Vector3(66f, 0f, -55f), 0.7f, out spot)) return;
            var crier = Toy(kit, spot + Vector3.up * 0.05f, 200f, ToyNpc.Kind.Bird, "Crier Hollerby", new Color(0.85f, 0.35f, 0.32f), new Color(0.95f, 0.82f, 0.35f),
                ToyNpc.Instrument.None, 1.35f, new Color(0.75f, 0.3f, 0.28f));
            crier.Lines = () => new[]
            {
                "Oyez, oyez! News of the town, fresh out of the toybox!",
                "The masons on the keep say their apprentice left something shiny on top of the scaffold. Too frightened to fetch it!",
                "Humming heard under the quay at night! Some say smugglers. Some say frogs. Something runny could find out.",
                "A little round door in Thread Ravine! Nobody's knocked on it in years.",
                "Mistress Clipwell swears there's a marble in the heart of the Bramble Maze. Bring string!",
                "The squires are running their trial at the Lists. Beat the stopwatch and you'll be knighted! Well. Given a marble.",
                "And the miller's lost something up on his windmill, and Burl the woodcutter's buried something in the west wood. Busy town, this!",
                "Out by the Pod's meadow, a lookout in the old oak can see the whole toybox. And in Gate Hamlet, they say the well grants wishes. To the right sort of visitor.",
                "Tilda Tench at the north ford has lost her lucky float on the willow island. Hop the lily pads, she says. Don't dawdle, she says.",
                "Farmer Furrow wants a hand with the harvest, out in the cabbage field on the King's Road. Quick hands, mind!",
            };
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The Fray, as it really is.
    //
    // The Fray isn't from Patch's world at all. It comes from the Underside: the back of the
    // cloth, where every stitch is a knot and a loose end and nothing has ever had a colour.
    // The things that crawl through look almost like toys, and that's what's wrong with them:
    // a ball of ash-grey fur on legs that are far too long, eyes out on stalks that each look
    // a different way, a mouth that's too wide with too few teeth, and a tongue the only
    // coloured thing on them (it's colour they've licked off the world). They eat colour.
    // Where they walk they leave grey behind, and the air round them goes dull.
    //
    // Behind them is the Unpicker: tall as a house, thin as a needle, a thimble for a hat, a
    // seam ripper for a walking stick and a stitched-shut mouth. He never comes close. He
    // stands a long way off and watches, and the stolen colours hang from his coat in little
    // glowing jars. (His world has none; he's collecting ours.)
    //
    // This file has what the creatures and the Unpicker look like (FrayBodies), the grey they
    // leave behind (GreyStain), and the Unpicker's watching (UnpickerWatch).
    public static class FrayBodies
    {
        static Material fur, furDark, scale, eyeBlack, pupil, tongue, tooth, mouthDark, wing, steel;
        static Material[] faded;

        public static void ResetStatics() { fur = null; faded = null; }

        static void Ensure()
        {
            if (fur != null && faded != null && faded[0] != null) return;
            fur = PatchworkAssets.Knit(new Color(0.6f, 0.58f, 0.64f));
            furDark = PatchworkAssets.Knit(new Color(0.3f, 0.28f, 0.34f));
            scale = PatchworkAssets.Plain(new Color(0.5f, 0.45f, 0.42f), 0.7f);        // dull, like old pewter
            eyeBlack = PatchworkAssets.Plain(new Color(0.05f, 0.04f, 0.07f), 0.95f);
            pupil = PatchworkAssets.Glow(SnarlKnot.FrayGlow, 1.6f);
            tongue = PatchworkAssets.Felt(new Color(0.86f, 0.42f, 0.5f));               // the one bright thing
            tooth = PatchworkAssets.Felt(new Color(0.93f, 0.9f, 0.8f));
            mouthDark = PatchworkAssets.Plain(new Color(0.07f, 0.05f, 0.09f), 0.2f);
            wing = PatchworkAssets.Felt(new Color(0.72f, 0.7f, 0.76f));
            steel = PatchworkAssets.Plain(new Color(0.72f, 0.74f, 0.8f), 0.85f);
            // Spots of colour they've eaten, already going grey.
            faded = new[]
            {
                PatchworkAssets.Felt(new Color(0.62f, 0.5f, 0.56f)), PatchworkAssets.Felt(new Color(0.46f, 0.58f, 0.6f)),
                PatchworkAssets.Felt(new Color(0.64f, 0.6f, 0.46f)),
            };
        }

        static Transform P(PrimitiveType type, Transform parent, Vector3 p, Vector3 s, Material m, Vector3 e = default(Vector3))
        {
            return Monsters.Part(type, parent, p, s, m, e);
        }

        static Transform Pivot(Transform parent, string name, Vector3 at)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = at;
            return t;
        }

        // An eye out on a stalk. Returns the eyeball (scale its y to blink). The stalk sways
        // on its own, never quite in time with the other one.
        static Transform EyeStalk(Transform parent, Vector3 from, Vector3 lean, float length, float eye, float seed)
        {
            var stalk = Pivot(parent, "EyeStalk", from);
            stalk.localRotation = Quaternion.FromToRotation(Vector3.up, lean.normalized);
            P(PrimitiveType.Cylinder, stalk, new Vector3(0f, length * 0.5f, 0f), new Vector3(eye * 0.22f, length * 0.5f, eye * 0.22f), furDark);
            var ball = P(PrimitiveType.Sphere, stalk, new Vector3(0f, length + eye * 0.3f, 0f), Vector3.one * eye, eyeBlack);
            // The pupil faces the way the creature does, whatever way the stalk leans.
            var pup = P(PrimitiveType.Sphere, ball, Vector3.zero, Vector3.one * 0.5f, pupil);
            pup.localPosition = Quaternion.Inverse(stalk.localRotation) * new Vector3(0f, 0f, 0.36f);
            var sway = stalk.gameObject.AddComponent<Rocker>();
            sway.Axis = new Vector3(Mathf.Cos(seed * 5f), 0f, Mathf.Sin(seed * 5f));
            sway.Amount = 9f;
            sway.Speed = 0.7f + Mathf.Repeat(seed * 0.37f, 0.9f);
            return ball;
        }

        // A long, thin, scaly leg hanging from a hip pivot, ending in a foot that's too big.
        // Returns the pivot (swing it to walk).
        static Transform Leg(Transform parent, Vector3 hip, float length, float thick, Vector3 foot)
        {
            var leg = Pivot(parent, "Leg", hip);
            P(PrimitiveType.Cylinder, leg, new Vector3(0f, -length * 0.5f, 0f), new Vector3(thick, length * 0.5f, thick), scale);
            int rings = Mathf.Max(2, Mathf.RoundToInt(length / 0.16f));
            for (int i = 0; i < rings; i++)
                P(PrimitiveType.Sphere, leg, new Vector3(0f, -length * (i + 0.5f) / rings, 0f), new Vector3(thick * 1.7f, length / rings * 0.8f, thick * 1.7f), scale);
            P(PrimitiveType.Sphere, leg, new Vector3(0f, -length + foot.y * 0.4f, foot.z * 0.28f), foot, scale);
            return leg;
        }

        // A fringe of fur tufts round a body's lower edge, and a few faded spots on top.
        static void Fuzz(Transform body, float radius, float y, int tufts, int spots, float seed)
        {
            for (int i = 0; i < tufts; i++)
            {
                float a = (i + seed) / tufts * Mathf.PI * 2f;
                P(PrimitiveType.Capsule, body, new Vector3(Mathf.Cos(a) * radius, y, Mathf.Sin(a) * radius), new Vector3(0.12f, 0.16f + (i % 3) * 0.04f, 0.12f) * (radius / 0.45f),
                    i % 4 == 0 ? furDark : fur, new Vector3(Mathf.Sin(a) * 22f, 0f, -Mathf.Cos(a) * 22f));
            }
            for (int i = 0; i < spots; i++)
            {
                Vector3 d = (Quaternion.Euler(-35f - (i % 3) * 18f, i * 97f + seed * 60f, 0f) * Vector3.forward).normalized;
                var s = P(PrimitiveType.Sphere, body, d * radius * 1.02f + Vector3.up * (y + radius * 0.75f), new Vector3(0.16f, 0.16f, 0.03f) * (0.8f + (i % 2) * 0.5f), faded[i % faded.Length]);
                s.localRotation = Quaternion.LookRotation(d);
            }
        }

        // ------------------------------------------------------------------ The creatures

        // The Snarling: a dome of fur striding about on two long legs, staring.
        public static void Walker(Transform root, Transform body, out Transform eyeL, out Transform eyeR, out Transform legL, out Transform legR, float seed)
        {
            Ensure();
            legL = Leg(root, new Vector3(-0.2f, 0.6f, 0f), 0.52f, 0.07f, new Vector3(0.28f, 0.12f, 0.46f));
            legR = Leg(root, new Vector3(0.2f, 0.6f, 0f), 0.52f, 0.07f, new Vector3(0.28f, 0.12f, 0.46f));
            // (body's own origin is 0.7 up)
            P(PrimitiveType.Sphere, body, new Vector3(0f, 0.12f, 0f), new Vector3(1f, 0.86f, 0.96f), fur);
            Fuzz(body, 0.45f, -0.14f, 11, 4, seed);
            // Too wide a mouth, two teeth, and a tongue that never quite goes back in.
            P(PrimitiveType.Sphere, body, new Vector3(0f, 0.0f, 0.4f), new Vector3(0.6f, 0.24f, 0.22f), mouthDark);
            for (int s = -1; s <= 1; s += 2)
                P(PrimitiveType.Capsule, body, new Vector3(s * 0.13f, 0.07f, 0.48f), new Vector3(0.07f, 0.07f, 0.06f), tooth);
            P(PrimitiveType.Capsule, body, new Vector3(0.06f, -0.14f, 0.5f), new Vector3(0.15f, 0.17f, 0.06f), tongue, new Vector3(-18f, 0f, 8f));
            eyeL = EyeStalk(body, new Vector3(-0.15f, 0.48f, 0.08f), new Vector3(-0.3f, 1f, 0.05f), 0.5f + Mathf.Repeat(seed, 0.25f), 0.24f, seed);
            eyeR = EyeStalk(body, new Vector3(0.15f, 0.48f, 0.08f), new Vector3(0.3f, 1f, 0.05f), 0.72f - Mathf.Repeat(seed, 0.25f), 0.24f, seed + 1.3f);
        }

        // The Tangle Hopper: a striped sock of a thing on one fat leg, its mouth on top.
        public static void Hopper(Transform root, Transform body, float seed)
        {
            Ensure();
            P(PrimitiveType.Cylinder, root, new Vector3(0f, 0.22f, 0f), new Vector3(0.18f, 0.22f, 0.18f), scale);
            for (int i = 0; i < 3; i++)
                P(PrimitiveType.Sphere, root, new Vector3(0f, 0.12f + i * 0.13f, 0f), new Vector3(0.3f, 0.11f, 0.3f), scale);
            P(PrimitiveType.Sphere, root, new Vector3(0f, 0.07f, 0.1f), new Vector3(0.5f, 0.14f, 0.66f), scale);
            // (body's own origin is 0.55 up) Bands, like a sock: grey, dark, and one that was once pink.
            Material[] bands = { furDark, fur, faded[0], fur };
            for (int i = 0; i < 4; i++)
                P(PrimitiveType.Sphere, body, new Vector3(0f, -0.08f + i * 0.17f, 0f), new Vector3(0.86f - i * 0.07f, 0.3f, 0.86f - i * 0.07f), bands[i]);
            // The mouth, gaping upwards, with three teeth round the rim and a tongue flopped out.
            P(PrimitiveType.Sphere, body, new Vector3(0f, 0.52f, 0.06f), new Vector3(0.52f, 0.2f, 0.52f), mouthDark);
            for (int i = 0; i < 3; i++)
            {
                float a = (i - 1) * 0.9f;
                P(PrimitiveType.Capsule, body, new Vector3(Mathf.Sin(a) * 0.22f, 0.6f, 0.06f + Mathf.Cos(a) * 0.22f), new Vector3(0.07f, 0.08f, 0.07f), tooth);
            }
            P(PrimitiveType.Capsule, body, new Vector3(-0.1f, 0.5f, 0.34f), new Vector3(0.14f, 0.2f, 0.06f), tongue, new Vector3(70f, -15f, 0f));
            EyeStalk(body, new Vector3(-0.28f, 0.42f, 0f), new Vector3(-0.7f, 1f, 0f), 0.22f, 0.2f, seed);
            EyeStalk(body, new Vector3(0.28f, 0.42f, 0f), new Vector3(0.7f, 1f, 0f), 0.34f, 0.2f, seed + 2.1f);
        }

        // The Static Moth: a puff of fur with one eye, two feelers and ragged wings, its little
        // legs dangling.
        public static void Flit(Transform body, Transform wingL, Transform wingR, float seed)
        {
            Ensure();
            P(PrimitiveType.Sphere, body, Vector3.zero, new Vector3(0.72f, 0.64f, 0.72f), fur);
            Fuzz(body, 0.32f, -0.12f, 7, 2, seed);
            P(PrimitiveType.Sphere, body, new Vector3(0f, 0.04f, 0.28f), Vector3.one * 0.32f, eyeBlack);
            P(PrimitiveType.Sphere, body, new Vector3(0f, 0.04f, 0.41f), Vector3.one * 0.14f, pupil);
            for (int s = -1; s <= 1; s += 2)
            {
                // Feelers with a ball on the end.
                var feeler = Pivot(body, "Feeler", new Vector3(s * 0.12f, 0.28f, 0.05f));
                feeler.localRotation = Quaternion.Euler(-12f, 0f, -s * 20f);
                P(PrimitiveType.Cylinder, feeler, new Vector3(0f, 0.3f, 0f), new Vector3(0.03f, 0.3f, 0.03f), furDark);
                P(PrimitiveType.Sphere, feeler, new Vector3(0f, 0.64f, 0f), Vector3.one * 0.13f, furDark);
                var sway = feeler.gameObject.AddComponent<Rocker>();
                sway.Axis = Vector3.right;
                sway.Amount = 12f;
                sway.Speed = 1.6f + s * 0.3f;
                // Legs that just hang there.
                P(PrimitiveType.Cylinder, body, new Vector3(s * 0.12f, -0.44f, 0f), new Vector3(0.04f, 0.16f, 0.04f), scale);
                P(PrimitiveType.Sphere, body, new Vector3(s * 0.12f, -0.62f, 0.04f), new Vector3(0.12f, 0.06f, 0.18f), scale);
                // Three ragged panels to a wing.
                var w = s < 0 ? wingL : wingR;
                for (int i = 0; i < 3; i++)
                    P(PrimitiveType.Sphere, w, new Vector3(s * (0.5f + i * 0.08f), 0f, 0.22f - i * 0.24f), new Vector3(0.8f - i * 0.12f, 0.04f, 0.4f), i == 1 ? fur : wing, new Vector3(0f, -s * (i - 1) * 22f, 0f));
            }
        }

        // The Static Spitter: nearly all mouth, with its eyes up on top and hands for feet.
        // Returns the mouth (it opens wider as it gets ready to spit).
        public static Transform Gawp(Transform root, Transform body, float seed)
        {
            Ensure();
            for (int s = -1; s <= 1; s += 2)
            {
                P(PrimitiveType.Cylinder, root, new Vector3(s * 0.24f, 0.2f, 0f), new Vector3(0.14f, 0.2f, 0.14f), scale);
                P(PrimitiveType.Sphere, root, new Vector3(s * 0.28f, 0.06f, 0.1f), new Vector3(0.36f, 0.1f, 0.34f), scale);
                for (int f = -1; f <= 1; f++)
                    P(PrimitiveType.Capsule, root, new Vector3(s * 0.28f + f * 0.13f, 0.05f, 0.36f), new Vector3(0.09f, 0.16f, 0.09f), scale, new Vector3(90f, f * 18f, 0f));
            }
            // (body's own origin is 0.95 up)
            P(PrimitiveType.Sphere, body, new Vector3(0f, 0.02f, 0f), new Vector3(1.05f, 1.5f, 0.95f), fur);
            Fuzz(body, 0.46f, -0.52f, 9, 5, seed);
            var mouth = P(PrimitiveType.Sphere, body, new Vector3(0f, -0.08f, 0.38f), new Vector3(0.74f, 0.4f, 0.3f), mouthDark);
            for (int s = -1; s <= 1; s += 2)
            {
                P(PrimitiveType.Capsule, body, new Vector3(s * 0.15f, 0.12f, 0.46f), new Vector3(0.1f, 0.09f, 0.08f), tooth);
                P(PrimitiveType.Capsule, body, new Vector3(s * 0.11f, -0.27f, 0.45f), new Vector3(0.1f, 0.08f, 0.08f), tooth);
            }
            EyeStalk(body, new Vector3(-0.26f, 0.62f, 0.1f), new Vector3(-0.35f, 1f, 0.1f), 0.16f, 0.3f, seed);
            EyeStalk(body, new Vector3(0.26f, 0.62f, 0.1f), new Vector3(0.35f, 1f, 0.1f), 0.28f, 0.3f, seed + 0.7f);
            return mouth;
        }

        // Eyes for a knot of the Fray: a few stalks poking out, looking every which way.
        public static void KnotEyes(Transform knot, float size)
        {
            Ensure();
            for (int i = 0; i < 3; i++)
            {
                Vector3 d = (Quaternion.Euler(-50f + i * 35f, i * 130f, 0f) * Vector3.forward).normalized;
                var ball = EyeStalk(knot, d * 0.45f * size, d, (0.4f + i * 0.12f) * size, 0.24f * size, i * 1.7f);
                ball.GetChild(0).localPosition = new Vector3(0f, 0.36f, 0f);   // these just stare outwards
            }
        }

        // ------------------------------------------------------------------ The Unpicker

        // Built standing at root, facing +z, about five and a half metres tall.
        public static void Unpicker(Transform root)
        {
            Ensure();
            // Legs like a wading bird's.
            Leg(root, new Vector3(-0.3f, 2.6f, 0f), 2.5f, 0.07f, new Vector3(0.5f, 0.16f, 0.9f));
            Leg(root, new Vector3(0.3f, 2.6f, 0f), 2.5f, 0.07f, new Vector3(0.5f, 0.16f, 0.9f));
            // The coat: a tall bell of grey scraps, patched with colours that have died.
            var coat = Pivot(root, "Coat", new Vector3(0f, 2.4f, 0f));
            for (int i = 0; i < 5; i++)
                P(PrimitiveType.Cylinder, coat, new Vector3(0f, 0.22f + i * 0.4f, 0f), new Vector3(1.25f - i * 0.17f, 0.22f, 1.05f - i * 0.14f), i % 2 == 0 ? furDark : fur);
            for (int i = 0; i < 9; i++)
            {
                float a = i * 2.4f, h = 0.2f + (i % 5) * 0.36f, r = 0.62f - (i % 5) * 0.08f;
                var patch = P(PrimitiveType.Cube, coat, new Vector3(Mathf.Sin(a) * r, h, Mathf.Cos(a) * r * 0.84f), new Vector3(0.3f, 0.26f, 0.03f), faded[i % faded.Length]);
                patch.localRotation = Quaternion.Euler(0f, a * Mathf.Rad2Deg, (i % 3 - 1) * 14f);
            }
            // The stolen colours: little jars on threads round his hem, the only bright things
            // about him.
            Color[] stolen = { LevelKit.Palette.Rose, LevelKit.Palette.Butter, LevelKit.Palette.Sky, LevelKit.Palette.Sage, LevelKit.Palette.Lavender, LevelKit.Palette.Gold };
            for (int i = 0; i < stolen.Length; i++)
            {
                float a = i / (float)stolen.Length * Mathf.PI * 2f + 0.4f;
                var jar = Pivot(coat, "Jar", new Vector3(Mathf.Sin(a) * 0.64f, 0.05f, Mathf.Cos(a) * 0.54f));
                P(PrimitiveType.Cylinder, jar, new Vector3(0f, -0.18f, 0f), new Vector3(0.012f, 0.18f, 0.012f), furDark);
                P(PrimitiveType.Sphere, jar, new Vector3(0f, -0.46f, 0f), new Vector3(0.2f, 0.26f, 0.2f), PatchworkAssets.Glow(stolen[i], 1.6f));
                P(PrimitiveType.Cylinder, jar, new Vector3(0f, -0.32f, 0f), new Vector3(0.1f, 0.03f, 0.1f), steel);
                var swing = jar.gameObject.AddComponent<Rocker>();
                swing.Axis = new Vector3(Mathf.Cos(a), 0f, -Mathf.Sin(a));
                swing.Amount = 10f;
                swing.Speed = 0.9f + i * 0.13f;
            }
            // Arms that reach nearly to his knees, three fingers each.
            for (int s = -1; s <= 1; s += 2)
            {
                var arm = Pivot(root, "Arm", new Vector3(s * 0.34f, 4.25f, 0f));
                arm.localRotation = Quaternion.Euler(s > 0 ? -22f : 4f, 0f, s * 9f);
                P(PrimitiveType.Cylinder, arm, new Vector3(0f, -1.05f, 0f), new Vector3(0.07f, 1.05f, 0.07f), furDark);
                P(PrimitiveType.Sphere, arm, new Vector3(0f, -1.05f, 0f), Vector3.one * 0.13f, furDark);
                for (int f = -1; f <= 1; f++)
                    P(PrimitiveType.Capsule, arm, new Vector3(f * 0.07f, -2.3f, 0f), new Vector3(0.05f, 0.22f, 0.05f), scale, new Vector3(0f, 0f, f * 14f));
                if (s < 0) continue;
                // The seam ripper, held like a staff: a pole, a ball, and a forked steel hook.
                var ripper = Pivot(arm, "SeamRipper", new Vector3(0f, -2.15f, 0.08f));
                ripper.localRotation = Quaternion.Euler(22f, 0f, -9f);
                P(PrimitiveType.Cylinder, ripper, new Vector3(0f, 0.2f, 0f), new Vector3(0.09f, 2.4f, 0.09f), furDark);
                P(PrimitiveType.Sphere, ripper, new Vector3(0f, 2.65f, 0f), Vector3.one * 0.2f, steel);
                P(PrimitiveType.Cylinder, ripper, new Vector3(0f, 3.1f, 0f), new Vector3(0.05f, 0.45f, 0.05f), steel);
                P(PrimitiveType.Cylinder, ripper, new Vector3(0.12f, 3.5f, 0f), new Vector3(0.04f, 0.3f, 0.04f), steel, new Vector3(0f, 0f, -28f));
                P(PrimitiveType.Sphere, ripper, new Vector3(-0.02f, 3.58f, 0f), Vector3.one * 0.1f, PatchworkAssets.Glow(new Color(0.9f, 0.25f, 0.3f), 1.2f));
            }
            // The head: a small pale dome with no face on it but a mouth sewn shut. A thimble
            // for a hat. And the eyes: five of them, on stalks, never all looking the same way.
            var head = Pivot(root, "Head", new Vector3(0f, 4.72f, 0.04f));
            P(PrimitiveType.Sphere, head, Vector3.zero, new Vector3(0.74f, 0.66f, 0.7f), wing);
            P(PrimitiveType.Cube, head, new Vector3(0f, -0.12f, 0.33f), new Vector3(0.4f, 0.025f, 0.02f), mouthDark);
            for (int i = -2; i <= 2; i++)
                P(PrimitiveType.Cube, head, new Vector3(i * 0.085f, -0.12f, 0.335f), new Vector3(0.018f, 0.1f, 0.02f), mouthDark, new Vector3(0f, 0f, i % 2 == 0 ? 18f : -18f));
            P(PrimitiveType.Cylinder, head, new Vector3(0f, 0.42f, -0.02f), new Vector3(0.5f, 0.22f, 0.5f), steel);
            P(PrimitiveType.Sphere, head, new Vector3(0f, 0.62f, -0.02f), new Vector3(0.5f, 0.2f, 0.5f), steel);
            P(PrimitiveType.Cylinder, head, new Vector3(0f, 0.22f, -0.02f), new Vector3(0.58f, 0.03f, 0.58f), steel);
            for (int i = 0; i < 5; i++)
            {
                float a = (i - 2) * 0.55f;
                Vector3 lean = new Vector3(Mathf.Sin(a) * 1.1f, 0.55f + (i % 2) * 0.3f, 0.35f);
                EyeStalk(head, new Vector3(Mathf.Sin(a) * 0.3f, 0.1f, 0.12f), lean, 0.45f + (i * 37 % 5) * 0.12f, 0.2f, i * 1.9f);
            }
            var nod = head.gameObject.AddComponent<Rocker>();
            nod.Axis = new Vector3(0.2f, 0f, 1f);
            nod.Amount = 7f;
            nod.Speed = 0.45f;
        }
    }

    // ------------------------------------------------------------------ The grey they leave
    //
    // Wherever a creature of the Fray goes, the colour goes out of the ground: grey blots are
    // left where it has walked, and fade back a while after it has moved on. And the nearer
    // Patch is to any of them, the more the whole picture drains towards grey. Unravel one and
    // its colour comes straight back: the blots round it shrink away in a puff of pompoms.
    public class GreyStain : MonoBehaviour
    {
        const int Count = 110;
        const float Life = 11f, GrowTime = 0.5f, FadeTime = 1.6f, Step = 1f;

        public static GreyStain Instance { get; private set; }

        Transform[] blot;
        float[] born, size;
        int next;
        float scan, drain, flash;
        readonly Dictionary<FrayCreature, Vector3> last = new Dictionary<FrayCreature, Vector3>();
        readonly Dictionary<FrayCreature, float> lastTime = new Dictionary<FrayCreature, float>();
        int mask;

        public static void ResetStatics() { Instance = null; MusicPlayer.FrayDread = 0f; }

        void Awake() { Instance = this; }
        void OnDestroy() { if (Instance == this) Instance = null; LevelLook.SetDrain(0f); MusicPlayer.FrayDread = 0f; }

        void Start()
        {
            mask = ~((1 << 2) | (1 << RenderOptimizer.GrassLayer));
            blot = new Transform[Count];
            born = new float[Count];
            size = new float[Count];
            var a = PatchworkAssets.Felt(new Color(0.6f, 0.59f, 0.62f));
            var b = PatchworkAssets.Felt(new Color(0.5f, 0.49f, 0.53f));
            for (int i = 0; i < Count; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Destroy(go.GetComponent<Collider>());
                go.name = "GreyBlot";
                go.layer = 2;
                go.transform.SetParent(transform, false);
                var r = go.GetComponent<MeshRenderer>();
                r.sharedMaterial = i % 3 == 0 ? b : a;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                go.SetActive(false);
                blot[i] = go.transform;
                born[i] = -100f;
            }
        }

        void Place(Vector3 from)
        {
            RaycastHit hit;
            if (!Physics.Raycast(from + Vector3.up * 0.3f, Vector3.down, out hit, 6f, mask, QueryTriggerInteraction.Ignore)) return;
            if (hit.normal.y < 0.6f || hit.rigidbody != null) return;
            int i = next;
            next = (next + 1) % Count;
            size[i] = Random.Range(1.5f, 2.5f);
            blot[i].SetPositionAndRotation(hit.point + hit.normal * (0.025f + (i % 5) * 0.003f), Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(0f, Random.value * 360f, 0f));
            blot[i].localScale = new Vector3(0.01f, 0.01f, 0.01f);
            born[i] = Time.time;
            blot[i].gameObject.SetActive(true);
        }

        // A creature has been unravelled here: its grey goes, and the colour bounces back.
        public void Restore(Vector3 at)
        {
            float now = Time.time;
            for (int i = 0; i < Count; i++)
            {
                if (now - born[i] > Life - FadeTime || (blot[i].position - at).sqrMagnitude > 49f) continue;
                born[i] = now - (Life - FadeTime * 0.4f);
            }
            Color[] back = { LevelKit.Palette.Rose, LevelKit.Palette.Butter, LevelKit.Palette.Sky, LevelKit.Palette.Sage };
            for (int i = 0; i < 3; i++)
                FxBurst.Spawn(FxBurst.Kind.Pompoms, at + new Vector3(Mathf.Cos(i * 2.1f), 0.3f, Mathf.Sin(i * 2.1f)) * 0.5f, back[Random.Range(0, back.Length)]);
            flash = 1f;
        }

        void Update()
        {
            if (blot == null) return;
            float dt = Time.deltaTime, now = Time.time;
            var game = PatchworkGame.Instance;
            var player = game != null ? game.Player : null;

            // Every so often: where is everything, and how near is the nearest?
            scan -= dt;
            if (scan <= 0f && player != null)
            {
                scan = 0.15f;
                Vector3 pp = player.Motor.Center;
                float nearest = 1000f;
                foreach (var c in FrayCreature.All)
                {
                    if (c == null || c.Center == null) continue;
                    Vector3 at = c.Center();
                    float d = (at - pp).magnitude;
                    nearest = Mathf.Min(nearest, d);
                    if (d > 70f) continue;
                    Vector3 was;
                    float when;
                    bool known = last.TryGetValue(c, out was);
                    lastTime.TryGetValue(c, out when);
                    if (!known || (new Vector2(at.x - was.x, at.z - was.z)).sqrMagnitude > Step * Step || now - when > Life - FadeTime - 1f)
                    {
                        last[c] = at;
                        lastTime[c] = now;
                        Place(at);
                    }
                }
                // Knots of the Fray drain the colour round them too.
                foreach (var v in PlayerVolume.All)
                {
                    var knot = v as SnarlKnot;
                    if (knot == null || knot.IsPopped || !knot.isActiveAndEnabled) continue;
                    nearest = Mathf.Min(nearest, (knot.transform.position - pp).magnitude + 1.5f);
                }
                float want = 0.75f * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(3f, 13f, nearest)));
                drainTarget = game.Mode == UIMode.Gameplay || game.Mode == UIMode.FormWheel ? want : 0f;
            }
            drain = Mathf.MoveTowards(drain, drainTarget, dt * 1.4f);
            flash = Mathf.MoveTowards(flash, 0f, dt * 1.6f);
            LevelLook.SetDrain(drain, flash);
            MusicPlayer.FrayDread = drain;

            for (int i = 0; i < Count; i++)
            {
                float age = now - born[i];
                if (age > Life) { if (blot[i].gameObject.activeSelf) blot[i].gameObject.SetActive(false); continue; }
                float k = age < GrowTime ? UIKit.EaseOutBack(age / GrowTime) : age > Life - FadeTime ? 1f - Mathf.SmoothStep(0f, 1f, (age - (Life - FadeTime)) / FadeTime) : 1f;
                blot[i].localScale = new Vector3(size[i] * k, 0.012f, size[i] * k * 0.82f);
            }
        }

        float drainTarget;
    }

    // ------------------------------------------------------------------ The Unpicker, watching
    //
    // He's never anywhere Patch can reach. He stands on a rooftop or a hilltop a long way off,
    // all his eyes on Patch, perfectly still. Get close, or look away for long enough, and he
    // folds up like an umbrella and is gone, to turn up somewhere else a while later. Once the
    // town is mended he doesn't come back. (He will, somewhere else.)
    public class UnpickerWatch : MonoBehaviour
    {
        public const string SeenKey = "L1_unpicker_seen", NamedKey = "L1_unpicker_named";
        public Vector2[] Spots;          // places he might stand (x, z): he stands on whatever is highest there
        public string DoneScene;         // he's gone for good once this level is complete

        Transform figure;
        float away = 14f, here, fold = -1f;
        bool shown;

        void Start()
        {
            figure = new GameObject("TheUnpicker").transform;
            figure.SetParent(transform, false);
            FrayBodies.Unpicker(figure);
            figure.gameObject.SetActive(false);
        }

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (figure == null || game == null || game.Player == null || Spots == null || Spots.Length == 0) return;
            float dt = Time.deltaTime;
            Vector3 pp = game.Player.transform.position;

            if (fold >= 0f)
            {
                // Folding away: thin as a thread, then nothing.
                fold += dt / 0.45f;
                float k = Mathf.Clamp01(fold);
                figure.localScale = new Vector3(1f - k, 1f + k * 0.25f, 1f - k);
                if (k >= 1f)
                {
                    fold = -1f;
                    shown = false;
                    figure.gameObject.SetActive(false);
                    away = Random.Range(22f, 40f);
                }
                return;
            }
            if (!shown)
            {
                if (game.Mode != UIMode.Gameplay || (!string.IsNullOrEmpty(DoneScene) && GameState.IsLevelComplete(DoneScene))) return;
                away -= dt;
                if (away > 0f) return;
                // Somewhere a good way off, high up.
                for (int tries = 0; tries < 6; tries++)
                {
                    Vector2 s = Spots[Random.Range(0, Spots.Length)];
                    float d = new Vector2(s.x - pp.x, s.y - pp.z).magnitude;
                    if (d < 42f || d > 100f) continue;
                    RaycastHit hit;
                    if (!Physics.Raycast(new Vector3(s.x, 80f, s.y), Vector3.down, out hit, 120f, ~(1 << 2), QueryTriggerInteraction.Ignore)) continue;
                    if (hit.point.y < -1f) continue;
                    figure.position = hit.point;
                    figure.localScale = Vector3.one;
                    figure.gameObject.SetActive(true);
                    shown = true;
                    here = Random.Range(24f, 40f);
                    break;
                }
                if (!shown) away = 4f;
                return;
            }

            // All his eyes on Patch.
            Vector3 to = pp - figure.position;
            float flat = new Vector2(to.x, to.z).magnitude;
            to.y = 0f;
            if (to.sqrMagnitude > 0.01f) figure.rotation = Quaternion.Slerp(figure.rotation, Quaternion.LookRotation(to.normalized, Vector3.up), 1f - Mathf.Exp(-1.5f * dt));
            here -= dt;
            bool tooClose = flat < 26f;
            if (!tooClose && here > 0f && game.Mode == UIMode.Gameplay) return;

            fold = 0f;
            FxBurst.Spawn(FxBurst.Kind.Threads, figure.position + Vector3.up * 3f, SnarlKnot.FrayDark);
            if (!tooClose) return;
            Sfx.PlayNear(SfxId.Unravel, figure.position, 0.5f, 60f);
            // The Storyteller notices, a little at a time.
            if (!GameState.IsHintSeen(SeenKey))
            {
                GameState.SetHintSeen(SeenKey);
                Narrator.Say("Did you see that? Up there. Tall fellow, thimble for a hat. No? ...I'm sure it was nothing.");
            }
            else if (!GameState.IsHintSeen(NamedKey))
            {
                GameState.SetHintSeen(NamedKey);
                Narrator.Say("There he is again. The toys call him the Unpicker. The Fray follows where he points, and the colours it takes end up in his jars.");
            }
        }
    }
}

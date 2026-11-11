using UnityEngine;

namespace Patchwork
{
    // The framed pictures on the Pod's walls: little felt-and-button snapshots of Patch and
    // his gang (his Water and Wind selves) caught at their silliest. Each is built flat, in
    // shallow relief, inside a frame that's already been placed and turned to face the room
    // (local x across, y up, +z out of the picture).
    public static class PodPictures
    {
        static readonly Color Pink = new Color(0.93f, 0.45f, 0.58f), Peach = new Color(1f, 0.85f, 0.6f);
        static readonly Color Jelly = new Color(0.36f, 0.66f, 0.95f), JellyPale = new Color(0.78f, 0.92f, 1f);
        static readonly Color Whirl = new Color(0.86f, 0.95f, 0.97f), Teal = new Color(0.55f, 0.85f, 0.8f);
        static readonly Color Ink = new Color(0.16f, 0.13f, 0.16f);

        public const int Count = 4;
        public static readonly string[] Titles = { "Windy Hair Day", "Bath Time", "The Cake Thief", "Group Photo" };

        public static void Build(LevelKit kit, Transform frame, int which, float w, float h)
        {
            switch (which % Count)
            {
                case 0: WindyHairDay(kit, frame, w, h); break;
                case 1: BathTime(kit, frame, w, h); break;
                case 2: CakeThief(kit, frame, w, h); break;
                default: GroupPhoto(kit, frame, w, h); break;
            }
        }

        // ------------------------------------------------------------------ The scenes

        // Wind sneezes, and Patch's yarn hair shoots straight out sideways in a great tangle,
        // while Water laughs so hard he's wobbling.
        static void WindyHairDay(LevelKit kit, Transform f, float w, float h)
        {
            Backdrop(kit, f, w, h, new Color(0.75f, 0.88f, 1f), new Color(0.55f, 0.78f, 0.45f));
            WindDoll(kit, f, new Vector3(-w * 0.32f, -h * 0.12f, 0f), 0.9f, true);
            // The sneeze: three curly gust lines.
            var gust = PatchworkAssets.Felt(Color.white);
            for (int i = 0; i < 3; i++)
                Flat(kit, f, PrimitiveType.Cube, new Vector3(-w * 0.12f, -h * 0.02f + (i - 1) * 0.09f, 0.03f), new Vector3(0.2f - Mathf.Abs(i - 1) * 0.05f, 0.025f, 0.01f), gust, (i - 1) * 10f);
            var patch = YarnDoll(kit, f, new Vector3(w * 0.12f, -h * 0.16f, 0f), 0.95f, Face.Shocked);
            // Hair blown out to the right in loops.
            var hair = PatchworkAssets.Yarn(Pink);
            for (int i = 0; i < 6; i++)
                Flat(kit, f, PrimitiveType.Sphere, patch + new Vector3(0.12f + i * 0.055f, 0.2f + Mathf.Sin(i * 1.7f) * 0.05f, 0.025f), new Vector3(0.09f, 0.06f, 0.01f), hair, i * 25f);
            JellyDoll(kit, f, new Vector3(w * 0.36f, -h * 0.2f, 0f), 0.8f, Face.Laughing);
            Speech(kit, f, new Vector3(-w * 0.28f, h * 0.27f, 0f), "ACHOO");
        }

        // Water jumps into the bath and the whole tub empties over Patch, who's left standing
        // there dripping with a rubber duck on his head. Wind holds the umbrella. Too late.
        static void BathTime(LevelKit kit, Transform f, float w, float h)
        {
            Backdrop(kit, f, w, h, new Color(1f, 0.92f, 0.82f), new Color(0.6f, 0.82f, 0.9f));
            // The tub, with a great splash leaping out.
            var tub = PatchworkAssets.Plain(new Color(0.98f, 0.98f, 1f), 0.8f);
            Flat(kit, f, PrimitiveType.Sphere, new Vector3(-w * 0.22f, -h * 0.26f, 0.01f), new Vector3(0.5f, 0.24f, 0.01f), tub);
            var splash = PatchworkAssets.Felt(new Color(0.55f, 0.8f, 1f));
            for (int i = 0; i < 5; i++)
                Flat(kit, f, PrimitiveType.Sphere, new Vector3(-w * 0.22f + (i - 2) * 0.09f, -h * 0.06f + Mathf.Abs(i - 2) * -0.03f + 0.06f, 0.02f), new Vector3(0.09f, 0.16f, 0.01f), splash, (i - 2) * 18f);
            JellyDoll(kit, f, new Vector3(-w * 0.22f, h * 0.12f, 0f), 0.65f, Face.Laughing);   // mid-cannonball
            var patch = YarnDoll(kit, f, new Vector3(w * 0.14f, -h * 0.17f, 0f), 0.95f, Face.Flat);
            // Drips all over him, and a duck on his head.
            for (int i = 0; i < 5; i++)
                Flat(kit, f, PrimitiveType.Sphere, patch + new Vector3(-0.12f + i * 0.06f, 0.05f - (i % 2) * 0.12f, 0.035f), new Vector3(0.025f, 0.04f, 0.01f), splash);
            var duck = PatchworkAssets.Felt(new Color(1f, 0.85f, 0.25f));
            Flat(kit, f, PrimitiveType.Sphere, patch + new Vector3(0f, 0.3f, 0.03f), new Vector3(0.1f, 0.07f, 0.01f), duck);
            Flat(kit, f, PrimitiveType.Sphere, patch + new Vector3(0.04f, 0.35f, 0.03f), new Vector3(0.06f, 0.06f, 0.01f), duck);
            Flat(kit, f, PrimitiveType.Cube, patch + new Vector3(0.08f, 0.35f, 0.035f), new Vector3(0.04f, 0.02f, 0.01f), PatchworkAssets.Felt(new Color(1f, 0.5f, 0.2f)));
            // Wind, holding an umbrella over the wrong person entirely.
            WindDoll(kit, f, new Vector3(w * 0.38f, -h * 0.14f, 0f), 0.7f, false);
            var brolly = PatchworkAssets.Felt(new Color(0.9f, 0.35f, 0.4f));
            Flat(kit, f, PrimitiveType.Cube, new Vector3(w * 0.38f, h * 0.08f, 0.02f), new Vector3(0.012f, 0.24f, 0.01f), PatchworkAssets.Felt(Ink));
            Flat(kit, f, PrimitiveType.Sphere, new Vector3(w * 0.38f, h * 0.2f, 0.025f), new Vector3(0.26f, 0.11f, 0.01f), brolly);
        }

        // A clockwork mouse makes off with the wedding cake, and the whole gang gives chase:
        // Patch reaching, Water skidding, Wind with his arms whirling.
        static void CakeThief(LevelKit kit, Transform f, float w, float h)
        {
            Backdrop(kit, f, w, h, new Color(0.98f, 0.86f, 0.9f), new Color(0.85f, 0.72f, 0.55f));
            // The mouse, out in front, carrying a slice almost as big as he is.
            var tin = PatchworkAssets.Plain(new Color(0.72f, 0.74f, 0.8f), 0.8f);
            Vector3 m = new Vector3(w * 0.33f, -h * 0.3f, 0f);
            Flat(kit, f, PrimitiveType.Sphere, m, new Vector3(0.16f, 0.09f, 0.01f), tin);
            Flat(kit, f, PrimitiveType.Sphere, m + new Vector3(0.08f, 0.05f, 0.005f), new Vector3(0.05f, 0.05f, 0.01f), tin);
            Flat(kit, f, PrimitiveType.Cube, m + new Vector3(-0.1f, 0.01f, 0f), new Vector3(0.08f, 0.01f, 0.01f), PatchworkAssets.Felt(Ink), 20f);
            Flat(kit, f, PrimitiveType.Cube, m + new Vector3(0f, 0.12f, 0.02f), new Vector3(0.16f, 0.1f, 0.01f), PatchworkAssets.Felt(new Color(0.99f, 0.95f, 0.88f)), -12f);
            Flat(kit, f, PrimitiveType.Cube, m + new Vector3(0f, 0.17f, 0.025f), new Vector3(0.16f, 0.03f, 0.01f), PatchworkAssets.Felt(new Color(0.95f, 0.5f, 0.6f)), -12f);
            Flat(kit, f, PrimitiveType.Sphere, m + new Vector3(0.02f, 0.21f, 0.03f), Vector3.one * 0.03f, PatchworkAssets.Felt(new Color(0.85f, 0.15f, 0.2f)));
            // Running motion lines behind him.
            for (int i = 0; i < 3; i++)
                Flat(kit, f, PrimitiveType.Cube, m + new Vector3(-0.2f - i * 0.02f, -0.02f + i * 0.04f, 0f), new Vector3(0.08f, 0.012f, 0.01f), PatchworkAssets.Felt(Color.white));
            Vector3 p = YarnDoll(kit, f, new Vector3(w * 0.02f, -h * 0.14f, 0f), 0.85f, Face.Shocked, 18f);
            Flat(kit, f, PrimitiveType.Capsule, p + new Vector3(0.17f, 0.02f, 0.03f), new Vector3(0.05f, 0.12f, 0.01f), PatchworkAssets.Yarn(Pink), -70f);   // reaching
            JellyDoll(kit, f, new Vector3(-w * 0.2f, -h * 0.2f, 0f), 0.7f, Face.Shocked, 14f);
            WindDoll(kit, f, new Vector3(-w * 0.38f, -h * 0.1f, 0f), 0.65f, false);
            Speech(kit, f, new Vector3(-w * 0.05f, h * 0.27f, 0f), "OI!");
        }

        // The three of them stacked into a tower for a photo: Water at the bottom squashed flat,
        // Patch in the middle wobbling, and Wind on top, upside down, grinning.
        static void GroupPhoto(LevelKit kit, Transform f, float w, float h)
        {
            Backdrop(kit, f, w, h, new Color(0.82f, 0.95f, 0.85f), new Color(0.98f, 0.82f, 0.5f));
            var jelly = PatchworkAssets.Felt(Jelly);
            Flat(kit, f, PrimitiveType.Sphere, new Vector3(0f, -h * 0.34f, 0f), new Vector3(0.36f, 0.12f, 0.01f), jelly);            // squashed flat
            Eyes(kit, f, new Vector3(0f, -h * 0.33f, 0f), 0.05f, Face.Flat);
            YarnDoll(kit, f, new Vector3(0.02f, -h * 0.13f, 0f), 0.75f, Face.Grin, -8f);
            // Wind, upside down on top, waving.
            WindDoll(kit, f, new Vector3(0.05f, h * 0.2f, 0f), 0.55f, false, 180f);
            // Wobble lines either side of the tower.
            for (int s = -1; s <= 1; s += 2)
                for (int i = 0; i < 2; i++)
                    Flat(kit, f, PrimitiveType.Cube, new Vector3(s * (0.24f + i * 0.05f), -h * 0.05f + i * 0.1f, 0.02f), new Vector3(0.012f, 0.09f, 0.01f), PatchworkAssets.Felt(Ink), s * 12f);
            // A camera flash in the corner.
            var flash = PatchworkAssets.Glow(new Color(1f, 0.95f, 0.7f), 1.2f);
            Vector3 c = new Vector3(w * 0.36f, h * 0.3f, 0.02f);
            for (int i = 0; i < 4; i++)
                Flat(kit, f, PrimitiveType.Cube, c, new Vector3(0.16f, 0.02f, 0.01f), flash, i * 45f);
        }

        // ------------------------------------------------------------------ Pieces

        enum Face { Grin, Shocked, Laughing, Flat }

        static Transform Flat(LevelKit kit, Transform f, PrimitiveType shape, Vector3 at, Vector3 size, Material mat, float roll = 0f)
        {
            // Everything sits just proud of the picture's surface (0.045 out from the frame).
            var t = kit.Deco(shape, at + new Vector3(0f, 0f, 0.05f), size, mat, default(Vector3), f);
            t.localRotation = Quaternion.Euler(0f, 0f, roll);
            var r = t.GetComponent<Renderer>();
            if (r != null) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return t;
        }

        static void Backdrop(LevelKit kit, Transform f, float w, float h, Color sky, Color ground)
        {
            Flat(kit, f, PrimitiveType.Cube, new Vector3(0f, 0f, -0.015f), new Vector3(w, h, 0.01f), PatchworkAssets.Felt(sky));
            Flat(kit, f, PrimitiveType.Cube, new Vector3(0f, -h * 0.38f, -0.01f), new Vector3(w, h * 0.24f, 0.01f), PatchworkAssets.Felt(ground));
        }

        static void Eyes(LevelKit kit, Transform f, Vector3 at, float r, Face face)
        {
            var ink = PatchworkAssets.Felt(Ink);
            for (int s = -1; s <= 1; s += 2)
            {
                if (face == Face.Laughing)
                    Flat(kit, f, PrimitiveType.Cube, at + new Vector3(s * r, r * 0.5f, 0.02f), new Vector3(r * 0.9f, r * 0.25f, 0.01f), ink, s * 25f);   // squeezed shut
                else
                    Flat(kit, f, PrimitiveType.Cylinder, at + new Vector3(s * r, r * 0.5f, 0.02f), face == Face.Shocked ? new Vector3(r * 0.9f, 0.005f, r * 0.9f) : new Vector3(r * 0.6f, 0.005f, r * 0.6f), ink).localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            Vector3 mouth = at + new Vector3(0f, -r * 0.6f, 0.02f);
            switch (face)
            {
                case Face.Shocked: Flat(kit, f, PrimitiveType.Sphere, mouth, new Vector3(r * 0.6f, r * 0.8f, 0.01f), ink); break;
                case Face.Flat: Flat(kit, f, PrimitiveType.Cube, mouth, new Vector3(r * 1.1f, r * 0.15f, 0.01f), ink); break;
                default:
                    Flat(kit, f, PrimitiveType.Sphere, mouth, new Vector3(r * 1.5f, r * (face == Face.Laughing ? 0.9f : 0.55f), 0.01f), ink);
                    Flat(kit, f, PrimitiveType.Sphere, mouth + new Vector3(0f, r * 0.2f, 0.005f), new Vector3(r * 1.5f, r * 0.4f, 0.01f), PatchworkAssets.Felt(new Color(0.97f, 0.42f, 0.5f)));
                    break;
            }
        }

        // Patch himself: a pink rag doll with a peach face and button eyes. Returns where his
        // head is (so a scene can add to it).
        static Vector3 YarnDoll(LevelKit kit, Transform f, Vector3 at, float s, Face face, float lean = 0f)
        {
            var body = PatchworkAssets.Yarn(Pink);
            var skin = PatchworkAssets.Felt(Peach);
            Quaternion q = Quaternion.Euler(0f, 0f, -lean);
            Flat(kit, f, PrimitiveType.Capsule, at, new Vector3(0.16f, 0.12f, 0.01f) * s, body, -lean);
            for (int k = -1; k <= 1; k += 2)
            {
                Flat(kit, f, PrimitiveType.Capsule, at + q * new Vector3(k * 0.1f, 0.02f, 0f) * s, new Vector3(0.05f, 0.09f, 0.01f) * s, body, -lean + k * 35f);   // arms
                Flat(kit, f, PrimitiveType.Capsule, at + q * new Vector3(k * 0.05f, -0.16f, 0f) * s, new Vector3(0.055f, 0.08f, 0.01f) * s, body, -lean);       // legs
            }
            Vector3 head = at + q * new Vector3(0f, 0.2f, 0f) * s;
            Flat(kit, f, PrimitiveType.Sphere, head, new Vector3(0.22f, 0.2f, 0.01f) * s, skin);
            Flat(kit, f, PrimitiveType.Cube, head + new Vector3(0f, 0.08f * s, 0.01f), new Vector3(0.2f, 0.035f, 0.01f) * s, PatchworkAssets.Yarn(Pink), -lean);     // fringe
            Flat(kit, f, PrimitiveType.Cube, head + new Vector3(0.05f * s, -0.04f * s, 0.015f), new Vector3(0.05f, 0.008f, 0.01f) * s, PatchworkAssets.Felt(Ink), 40f);   // a stitch on his cheek
            Eyes(kit, f, head, 0.045f * s, face);
            return head;
        }

        // Water: a round, wobbly blue jelly doll.
        static void JellyDoll(LevelKit kit, Transform f, Vector3 at, float s, Face face, float lean = 0f)
        {
            var jelly = PatchworkAssets.Felt(Jelly);
            Flat(kit, f, PrimitiveType.Sphere, at, new Vector3(0.24f, 0.26f, 0.01f) * s, jelly, -lean);
            Flat(kit, f, PrimitiveType.Sphere, at + new Vector3(-0.05f, 0.06f, 0.01f) * s, new Vector3(0.06f, 0.08f, 0.01f) * s, PatchworkAssets.Felt(JellyPale));   // shine
            Flat(kit, f, PrimitiveType.Sphere, at + new Vector3(0f, 0.17f, 0f) * s, new Vector3(0.05f, 0.08f, 0.01f) * s, jelly);    // his drip on top
            for (int k = -1; k <= 1; k += 2)
                Flat(kit, f, PrimitiveType.Capsule, at + new Vector3(k * 0.13f, 0f, 0f) * s, new Vector3(0.04f, 0.08f, 0.01f) * s, jelly, k * 50f);
            Eyes(kit, f, at + new Vector3(0f, 0.02f, 0f) * s, 0.04f * s, face);
        }

        // Wind: a little whirlwind of rings with a round head on top.
        static void WindDoll(LevelKit kit, Transform f, Vector3 at, float s, bool sneezing, float roll = 0f)
        {
            Quaternion q = Quaternion.Euler(0f, 0f, roll);
            var ring = PatchworkAssets.Felt(Whirl);
            var trim = PatchworkAssets.Felt(Teal);
            for (int i = 0; i < 4; i++)
                Flat(kit, f, PrimitiveType.Sphere, at + q * new Vector3(Mathf.Sin(i * 1.3f) * 0.015f, -0.14f + i * 0.06f, 0f) * s, new Vector3(0.08f + i * 0.04f, 0.05f, 0.01f) * s, i % 2 == 0 ? ring : trim, roll);
            Vector3 head = at + q * new Vector3(0f, 0.15f, 0f) * s;
            Flat(kit, f, PrimitiveType.Sphere, head, new Vector3(0.2f, 0.19f, 0.01f) * s, ring);
            Eyes(kit, f, head, 0.04f * s, sneezing ? Face.Laughing : Face.Grin);
            if (sneezing)
                Flat(kit, f, PrimitiveType.Sphere, head + new Vector3(0.09f, -0.02f, 0.02f) * s, new Vector3(0.05f, 0.04f, 0.01f) * s, PatchworkAssets.Felt(new Color(0.97f, 0.6f, 0.65f)));
        }

        // A little white speech bubble with lettering made from felt strips.
        static void Speech(LevelKit kit, Transform f, Vector3 at, string word)
        {
            float wide = 0.07f * word.Length + 0.08f;
            Flat(kit, f, PrimitiveType.Sphere, at, new Vector3(wide, 0.13f, 0.01f), PatchworkAssets.Felt(Color.white));
            Flat(kit, f, PrimitiveType.Cube, at + new Vector3(-wide * 0.2f, -0.08f, 0f), new Vector3(0.04f, 0.06f, 0.01f), PatchworkAssets.Felt(Color.white), 30f);
            // Not real letters: a row of bold scribbles, the way a sound looks in a comic.
            var ink = PatchworkAssets.Felt(new Color(0.85f, 0.2f, 0.3f));
            for (int i = 0; i < word.Length; i++)
                if (word[i] != ' ')
                    Flat(kit, f, PrimitiveType.Cube, at + new Vector3((i - (word.Length - 1) * 0.5f) * 0.065f, 0f, 0.01f), new Vector3(0.035f, 0.065f, 0.01f), ink, (i % 2 == 0 ? 10f : -10f));
        }
    }
}

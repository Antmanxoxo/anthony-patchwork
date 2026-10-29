using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Captain Yarny: an old yarn-ball pirate who sails the Cardboard Skiff. Faded red yarn, a
    // scraggly grey yarn beard, an eyepatch, a felt tricorn, a pencil peg leg, a crochet hook for a
    // hand and a little yarn parrot on his shoulder. He turns to watch Patch, sways with the ship,
    // and talks in grumbly little blips from where he stands.
    public class NpcYarny : MonoBehaviour
    {
        public const string DisplayName = "Captain Yarny";
        const string MetKey = "Yarny_met";

        Transform body, hat, beardRoot, parrot, hookArm;
        readonly List<Transform> beard = new List<Transform>();
        AudioSource voice;
        float t;
        float baseYaw;
        float yaw;
        float talkPulse;
        float greetCooldown;
        int chatIndex;
        public bool Talking { get; set; }

        public Vector3 HeadPosition { get { return transform.position + Vector3.up * 1.9f; } }

        public static NpcYarny Create(LevelKit kit, Vector3 feet, float faceYaw)
        {
            var go = new GameObject("Captain Yarny");
            go.transform.SetParent(kit.Root, false);
            go.transform.position = feet;
            go.transform.rotation = Quaternion.Euler(0f, faceYaw, 0f);
            var npc = go.AddComponent<NpcYarny>();
            npc.baseYaw = faceYaw;
            npc.yaw = faceYaw;
            npc.Build();
            // Something solid to bump into.
            var col = go.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 1.1f, 0f);
            col.radius = 0.7f;
            col.height = 2.2f;
            return npc;
        }

        // ------------------------------------------------------------------ Look

        void Build()
        {
            var yarnRed = PatchworkAssets.Yarn(new Color(0.66f, 0.3f, 0.32f));
            var grey = PatchworkAssets.Yarn(new Color(0.8f, 0.78f, 0.76f));
            var black = PatchworkAssets.Felt(new Color(0.16f, 0.13f, 0.17f));
            var gold = PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.8f);
            var white = PatchworkAssets.Felt(new Color(0.97f, 0.95f, 0.9f));
            var pencil = PatchworkAssets.Plain(new Color(0.98f, 0.8f, 0.3f), 0.4f);
            var steel = PatchworkAssets.Plain(new Color(0.8f, 0.82f, 0.88f), 0.9f);

            // Legs: one stubby yarn leg with a boot, one pencil peg leg.
            Part(PrimitiveType.Cylinder, transform, new Vector3(-0.28f, 0.3f, 0f), new Vector3(0.28f, 0.3f, 0.28f), yarnRed, Vector3.zero);
            Part(PrimitiveType.Sphere, transform, new Vector3(-0.28f, 0.08f, 0.08f), new Vector3(0.36f, 0.2f, 0.48f), black, Vector3.zero);
            Part(PrimitiveType.Cylinder, transform, new Vector3(0.28f, 0.32f, 0f), new Vector3(0.12f, 0.32f, 0.12f), pencil, Vector3.zero);
            Part(PrimitiveType.Cylinder, transform, new Vector3(0.28f, 0.04f, 0f), new Vector3(0.08f, 0.04f, 0.08f), PatchworkAssets.Plain(new Color(0.3f, 0.3f, 0.32f), 0.3f), Vector3.zero);

            body = new GameObject("Body").transform;
            body.SetParent(transform, false);
            body.localPosition = new Vector3(0f, 1.2f, 0f);
            // The ball of yarn he's made of.
            Part(PrimitiveType.Sphere, body, Vector3.zero, new Vector3(1.35f, 1.25f, 1.2f), yarnRed, Vector3.zero);
            // Belt with a gold buckle.
            Part(PrimitiveType.Cylinder, body, new Vector3(0f, -0.3f, 0f), new Vector3(1.24f, 0.07f, 1.1f), PatchworkAssets.Felt(new Color(0.35f, 0.25f, 0.2f)), Vector3.zero);
            Part(PrimitiveType.Cube, body, new Vector3(0f, -0.3f, 0.55f), new Vector3(0.24f, 0.18f, 0.06f), gold, Vector3.zero);

            // Face: one button eye, one eyepatch, a pink nose.
            Part(PrimitiveType.Cylinder, body, new Vector3(-0.22f, 0.2f, 0.56f), new Vector3(0.2f, 0.03f, 0.2f), white, new Vector3(80f, 0f, 0f));
            Part(PrimitiveType.Cylinder, body, new Vector3(-0.22f, 0.2f, 0.585f), new Vector3(0.1f, 0.03f, 0.1f), black, new Vector3(80f, 0f, 0f));
            Part(PrimitiveType.Cylinder, body, new Vector3(0.22f, 0.21f, 0.56f), new Vector3(0.24f, 0.03f, 0.22f), black, new Vector3(80f, 0f, 0f));
            Part(PrimitiveType.Cube, body, new Vector3(0f, 0.32f, 0.47f), new Vector3(1.0f, 0.05f, 0.05f), black, new Vector3(0f, 0f, -18f));
            Part(PrimitiveType.Sphere, body, new Vector3(0f, 0.06f, 0.62f), new Vector3(0.18f, 0.15f, 0.15f), PatchworkAssets.Felt(new Color(0.95f, 0.6f, 0.6f)), Vector3.zero);
            // Bushy eyebrow over the button eye.
            Part(PrimitiveType.Cube, body, new Vector3(-0.22f, 0.36f, 0.53f), new Vector3(0.3f, 0.07f, 0.08f), grey, new Vector3(0f, 0f, 12f));

            // Mustache and a scraggly beard of grey yarn strands.
            Part(PrimitiveType.Sphere, body, new Vector3(-0.15f, -0.04f, 0.6f), new Vector3(0.32f, 0.12f, 0.14f), grey, new Vector3(0f, 0f, 20f));
            Part(PrimitiveType.Sphere, body, new Vector3(0.15f, -0.04f, 0.6f), new Vector3(0.32f, 0.12f, 0.14f), grey, new Vector3(0f, 0f, -20f));
            beardRoot = new GameObject("Beard").transform;
            beardRoot.SetParent(body, false);
            beardRoot.localPosition = new Vector3(0f, -0.1f, 0.55f);
            for (int i = 0; i < 9; i++)
            {
                float x = (i - 4) * 0.075f;
                float len = 0.35f + 0.18f * (1f - Mathf.Abs(i - 4) / 4f) + (i % 2) * 0.05f;
                var strand = new GameObject("Strand").transform;
                strand.SetParent(beardRoot, false);
                strand.localPosition = new Vector3(x, 0f, 0.02f * (1f - Mathf.Abs(i - 4) / 4f));
                strand.localRotation = Quaternion.Euler(-12f, 0f, (i - 4) * 4f);
                Part(PrimitiveType.Cylinder, strand, new Vector3(0f, -len * 0.5f, 0f), new Vector3(0.07f, len * 0.5f, 0.07f), grey, Vector3.zero);
                Part(PrimitiveType.Sphere, strand, new Vector3(0f, -len, 0.02f), Vector3.one * 0.09f, grey, Vector3.zero);
                beard.Add(strand);
            }

            // Tricorn hat with gold trim and a button skull.
            hat = new GameObject("Hat").transform;
            hat.SetParent(body, false);
            hat.localPosition = new Vector3(0f, 0.58f, 0f);
            hat.localRotation = Quaternion.Euler(-6f, 0f, 4f);
            Part(PrimitiveType.Cylinder, hat, Vector3.zero, new Vector3(1.25f, 0.04f, 1.1f), black, Vector3.zero);
            Part(PrimitiveType.Cylinder, hat, new Vector3(0f, 0.2f, 0f), new Vector3(0.75f, 0.2f, 0.7f), black, Vector3.zero);
            Part(PrimitiveType.Cylinder, hat, new Vector3(0f, 0.05f, 0f), new Vector3(0.78f, 0.04f, 0.73f), gold, Vector3.zero);
            for (int i = 0; i < 3; i++)
            {
                float a = i * 120f;
                var flap = Part(PrimitiveType.Cube, hat, Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0.16f, 0.5f), new Vector3(0.9f, 0.32f, 0.05f), black, new Vector3(-25f, a, 0f));
                flap.localRotation = Quaternion.Euler(0f, a, 0f) * Quaternion.Euler(-25f, 0f, 0f);
            }
            Part(PrimitiveType.Sphere, hat, new Vector3(0f, 0.2f, 0.6f), new Vector3(0.2f, 0.18f, 0.06f), white, Vector3.zero);
            Part(PrimitiveType.Sphere, hat, new Vector3(-0.04f, 0.22f, 0.63f), Vector3.one * 0.04f, black, Vector3.zero);
            Part(PrimitiveType.Sphere, hat, new Vector3(0.04f, 0.22f, 0.63f), Vector3.one * 0.04f, black, Vector3.zero);

            // Left arm: a stubby yarn arm on his hip. Right arm: a crochet hook.
            Part(PrimitiveType.Capsule, body, new Vector3(-0.66f, -0.15f, 0.05f), new Vector3(0.22f, 0.3f, 0.22f), yarnRed, new Vector3(0f, 0f, -35f));
            hookArm = new GameObject("HookArm").transform;
            hookArm.SetParent(body, false);
            hookArm.localPosition = new Vector3(0.62f, -0.05f, 0.1f);
            Part(PrimitiveType.Capsule, hookArm, new Vector3(0.1f, -0.15f, 0f), new Vector3(0.22f, 0.25f, 0.22f), yarnRed, new Vector3(0f, 0f, 30f));
            Part(PrimitiveType.Cylinder, hookArm, new Vector3(0.28f, 0.12f, 0.1f), new Vector3(0.07f, 0.35f, 0.07f), steel, new Vector3(20f, 0f, -15f));
            Part(PrimitiveType.Sphere, hookArm, new Vector3(0.38f, 0.46f, 0.2f), new Vector3(0.1f, 0.12f, 0.1f), steel, Vector3.zero);

            // A green yarn parrot on his shoulder.
            parrot = new GameObject("Parrot").transform;
            parrot.SetParent(body, false);
            parrot.localPosition = new Vector3(-0.5f, 0.5f, -0.1f);
            var green = PatchworkAssets.Yarn(new Color(0.4f, 0.75f, 0.45f));
            Part(PrimitiveType.Sphere, parrot, Vector3.zero, new Vector3(0.26f, 0.32f, 0.26f), green, Vector3.zero);
            Part(PrimitiveType.Sphere, parrot, new Vector3(0f, 0.2f, 0.04f), Vector3.one * 0.2f, green, Vector3.zero);
            Part(PrimitiveType.Cube, parrot, new Vector3(0f, 0.18f, 0.14f), new Vector3(0.07f, 0.09f, 0.09f), PatchworkAssets.Felt(new Color(0.95f, 0.4f, 0.3f)), new Vector3(30f, 0f, 0f));
            Part(PrimitiveType.Sphere, parrot, new Vector3(0.06f, 0.24f, 0.08f), Vector3.one * 0.04f, black, Vector3.zero);
            Part(PrimitiveType.Sphere, parrot, new Vector3(-0.06f, 0.24f, 0.08f), Vector3.one * 0.04f, black, Vector3.zero);
            Part(PrimitiveType.Cube, parrot, new Vector3(0f, -0.15f, -0.14f), new Vector3(0.1f, 0.03f, 0.28f), PatchworkAssets.Felt(new Color(0.95f, 0.8f, 0.3f)), new Vector3(35f, 0f, 0f));

            voice = gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.spatialBlend = 0.8f;
            voice.rolloffMode = AudioRolloffMode.Linear;
            voice.minDistance = 3f;
            voice.maxDistance = 22f;
            voice.dopplerLevel = 0f;
        }

        static Transform Part(PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Material mat, Vector3 euler)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.Destroy(go.GetComponent<Collider>());
            go.layer = 2;
            var tr = go.transform;
            tr.SetParent(parent, false);
            tr.localPosition = pos;
            tr.localRotation = Quaternion.Euler(euler);
            tr.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return tr;
        }

        // ------------------------------------------------------------------ Life

        void Update()
        {
            float dt = Time.deltaTime;
            t += dt;
            greetCooldown -= dt;
            talkPulse = Mathf.MoveTowards(talkPulse, 0f, dt * 6f);

            // Turn to watch Patch when he's nearby; otherwise drift back to looking out to sea.
            float want = baseYaw;
            var game = PatchworkGame.Instance;
            if (game != null && game.Player != null)
            {
                Vector3 to = game.Player.transform.position - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude < 100f && to.sqrMagnitude > 0.01f) want = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
            }
            yaw = Mathf.MoveTowardsAngle(yaw, want, dt * 120f);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            // Sway with the ship, bob on the peg leg, beard and parrot wobble.
            float sway = Mathf.Sin(t * 1.1f) * 3f;
            float bob = Mathf.Abs(Mathf.Sin(t * 1.1f)) * 0.03f + talkPulse * 0.05f;
            body.localPosition = new Vector3(0f, 1.2f + bob, 0f);
            body.localRotation = Quaternion.Euler(talkPulse * -4f, 0f, sway);
            for (int i = 0; i < beard.Count; i++)
            {
                float s = Mathf.Sin(t * 2.3f + i * 0.7f) * 6f + talkPulse * 10f * ((i % 2) * 2 - 1);
                beard[i].localRotation = Quaternion.Euler(-12f + s * 0.5f, 0f, (i - 4) * 4f + s);
            }
            hookArm.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.8f) * 6f + (Talking ? Mathf.Sin(t * 5f) * 12f : 0f));
            float parrotHop = Mathf.Max(0f, Mathf.Sin(t * 0.6f)) > 0.97f ? 0.06f : 0f;
            parrot.localPosition = new Vector3(-0.5f, 0.5f + parrotHop, -0.1f);
            parrot.localRotation = Quaternion.Euler(0f, Mathf.Sin(t * 0.5f) * 40f, Mathf.Sin(t * 3f) * 4f);
        }

        // A grumbly little syllable, from where he's standing.
        public void Blip()
        {
            if (voice == null) return;
            talkPulse = 1f;
            voice.pitch = Random.Range(0.5f, 0.68f);
            var clip = Sfx.Clip(Random.value < 0.7f ? SfxId.VoiceHup : SfxId.VoiceAww);
            if (clip != null) voice.PlayOneShot(clip, 0.35f * Sfx.Volume / 10f);
        }

        // A hearty "arr" when you walk up to him (not every single time).
        public void Greet()
        {
            if (greetCooldown > 0f || voice == null) return;
            greetCooldown = 12f;
            talkPulse = 1f;
            voice.pitch = 0.55f;
            var clip = Sfx.Clip(SfxId.VoiceYay);
            if (clip != null) voice.PlayOneShot(clip, 0.4f * Sfx.Volume / 10f);
        }

        // ------------------------------------------------------------------ What he says

        public string[] NextLines()
        {
            if (!GameState.IsHintSeen(MetKey))
            {
                GameState.SetHintSeen(MetKey);
                GameState.Save();
                return new[]
                {
                    "Arr! A new deckhand aboard! Name's Captain Yarny, master o' the Cardboard Skiff.",
                    "I've been sailin' these patchwork skies since I were a single loose thread.",
                    "But there be trouble below. A grey static's creepin' over the world. We sailors call it the Fray.",
                    "It slipped in through a tear in the sky, and it unravels whatever it touches. Gardens, rivers, whole kingdoms o' cloth.",
                    "Every place has golden Heartstrings holdin' it together. Find 'em, thread 'em back where they belong, and the Fray lets go.",
                    "See that globe at the bow? Step up to the helm and give it a turn. The Sewing Basket Garden's where it struck first.",
                    "Now off ye go. And mind ye don't unravel!"
                };
            }

            var lines = new List<string[]>();
            int found = GameState.GoldenButtonCount();
            int needed = Level1Builder.CollectibleIds.Length;
            bool done1 = GameState.IsLevelComplete(Level1Builder.SceneName);
            int hearts = 0;
            foreach (var id in Level1Builder.HeartstringIds) if (GameState.IsCollected(id)) hearts++;
            if (!done1 && hearts < Level1Builder.HeartstringsNeeded)
                lines.Add(new[] { "The Fray's got the Sewing Basket Garden all tangled up.", "Find " + Level1Builder.HeartstringsNeeded + " Heartstrings and the Heart Spool will wake. Ye've got " + hearts + " so far.",
                                  "Four corners to that garden: the Peaks, the Falls, the Bluffs and the Ruins. Start wherever ye fancy!" });
            else if (!done1)
                lines.Add(new[] { "The Heart Spool's awake! That golden thread'll carry ye right up to the Fray's knot.", "Pop the knots holdin' it together and set the garden's heart free!" });
            else
                lines.Add(new[] { "Ye mended the garden! The Fray's knot came right undone. Proud o' ye, deckhand.",
                                  "But where there's one tear in the sky, there's bound to be more...", "Mark me words. We'll be needin' that globe again." });
            if (hearts < Level1Builder.HeartstringIds.Length)
                lines.Add(new[] { "There be " + (Level1Builder.HeartstringIds.Length - hearts) + " Heartstrings still loose in the garden.",
                                  "Some only show up when ye beat a challenge. Knots to pop, races to run, Snarlings to sort out." });
            lines.Add(new[] { "Snarlings, they call the Fray's little critters. All grumble, no bite.", "Hop on their heads and they come right undone!" });
            if (found < needed)
                lines.Add(new[] { "Word on the wind is there be " + (needed - found) + " golden button" + (needed - found == 1 ? "" : "s") + " still hidin' in that garden.",
                                  "One of 'em hums a little tune, they say. Keep yer ears open, matey." });
            else
                lines.Add(new[] { "Ye found every golden button! Me cabin door's come unstitched.", "Go on in. There be somethin' shiny in there for a captain-in-trainin'." });
            lines.Add(new[] { "Lost me eye to a runaway sewin' needle, I did.", "Watch yer step 'round the pins, deckhand." });
            lines.Add(new[] { "Water slips through drains, Wind rides the breeze...", "...and Yarn? Yarn swings from the rigging, like any proper pirate! Arr!" });
            lines.Add(new[] { "This old Skiff's held together with tape and stubbornness.", "And a wee bit o' glue. Don't tell the parrot." });
            lines.Add(new[] { "Tap them face buttons fast enough and ye might break into a jig.", "Not that I'd know anythin' about that. Ahem." });
            lines.Add(new[] { "Every hat ye find stays on through every form.", "A pirate's hat is his pride! Mine's older than the ship." });

            var pick = lines[chatIndex % lines.Count];
            chatIndex++;
            return pick;
        }
    }
}

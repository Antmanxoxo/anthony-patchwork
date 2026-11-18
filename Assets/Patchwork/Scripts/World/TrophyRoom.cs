using UnityEngine;

namespace Patchwork
{
    // The middle of the Trophy Room: a grand white plinth under its own spotlight, velvet rope
    // round it, and on top, very small, the Participation Trophy. Walk up to it and it's
    // presented, with all the ceremony it deserves.
    public class ParticipationTrophy : PlayerVolume
    {
        Transform cup;
        float spin;
        bool presented;

        public static void Build(LevelKit kit, Vector3 floor, float ceiling)
        {
            var white = PatchworkAssets.Felt(new Color(0.97f, 0.95f, 0.92f));
            var gold = PatchworkAssets.Glow(new Color(1f, 0.8f, 0.32f), 0.5f);
            var brass = PatchworkAssets.Plain(new Color(0.95f, 0.78f, 0.4f), 0.85f);
            // The plinth: a fluted column on a stepped base, far too grand for what's on it.
            kit.Block(floor + Vector3.up * 0.1f, new Vector3(1.2f, 0.2f, 1.2f), Color.white, "Plinth", white);
            kit.Block(floor + Vector3.up * 0.75f, new Vector3(0.8f, 1.1f, 0.8f), Color.white, "Plinth", white);
            for (int k = 0; k < 4; k++)
            {
                var flute = kit.Deco(PrimitiveType.Cube, floor + Vector3.up * 0.75f, new Vector3(0.82f, 1f, 0.05f), PatchworkAssets.Felt(new Color(0.9f, 0.88f, 0.85f)));
                flute.rotation = Quaternion.Euler(0f, k * 45f, 0f);
            }
            kit.Block(floor + Vector3.up * 1.36f, new Vector3(1f, 0.12f, 1f), Color.white, "Plinth", white);
            kit.Deco(PrimitiveType.Cylinder, floor + Vector3.up * 1.44f, new Vector3(0.5f, 0.03f, 0.5f), PatchworkAssets.Felt(new Color(0.65f, 0.15f, 0.25f)));   // a velvet cushion
            // The trophy itself: tiny.
            var t = kit.Group("ParticipationTrophyCup", floor + Vector3.up * 1.48f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.02f, 0f), new Vector3(0.08f, 0.04f, 0.08f), PatchworkAssets.Wood(new Color(0.5f, 0.32f, 0.22f)), default(Vector3), t);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.06f, 0f), new Vector3(0.015f, 0.02f, 0.015f), gold, default(Vector3), t);
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, 0.1f, 0f), new Vector3(0.06f, 0.05f, 0.06f), gold, default(Vector3), t);
            for (int s = -1; s <= 1; s += 2)
                kit.Deco(PrimitiveType.Cylinder, new Vector3(s * 0.035f, 0.105f, 0f), new Vector3(0.02f, 0.004f, 0.02f), gold, new Vector3(0f, 0f, 90f), t);
            // The brass plate on the front of the column.
            Vector3 plate = floor + new Vector3(0f, 1.05f, 0.42f);
            kit.Deco(PrimitiveType.Cube, plate, new Vector3(0.62f, 0.2f, 0.02f), brass);
            var words = new MeshBatch();
            Level2Builder.Letters(words, "FOR TURNING UP", plate + new Vector3(0f, 0f, 0.012f), Vector3.forward, 0.0068f, 0.004f);
            words.Build(kit.Root, "ParticipationWords", PatchworkAssets.Felt(new Color(0.3f, 0.22f, 0.15f)));
            // A spotlight from the ceiling, just for it.
            var lgo = new GameObject("TrophySpot");
            lgo.transform.SetParent(kit.Root, false);
            lgo.transform.position = floor + Vector3.up * (ceiling - 0.15f);
            lgo.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            var l = lgo.AddComponent<Light>();
            l.type = LightType.Spot;
            l.spotAngle = 28f;
            l.range = ceiling + 1f;
            l.intensity = 3f;
            l.color = new Color(1f, 0.93f, 0.8f);
            l.shadows = LightShadows.None;
            kit.Deco(PrimitiveType.Cylinder, lgo.transform.position + Vector3.up * 0.05f, new Vector3(0.3f, 0.08f, 0.3f), PatchworkAssets.Plain(new Color(0.2f, 0.2f, 0.22f), 0.6f));

            var v = kit.Volume<ParticipationTrophy>(floor + new Vector3(0f, 1f, 0f), new Vector3(2.4f, 2.4f, 2.4f), "ParticipationTrophy");
            v.cup = t;
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            spin = 1f;
            Sfx.Play(SfxId.Boing, 0.5f);
            FxBurst.Spawn(FxBurst.Kind.Sparkle, cup != null ? cup.position + Vector3.up * 0.2f : transform.position, new Color(1f, 0.85f, 0.4f));
            if (presented) return;
            presented = true;
            player.Game.ShowToast("The Participation Trophy. Awarded to Patch, for turning up. Well done, you.");
        }

        void Update()
        {
            if (cup == null || spin <= 0f) return;
            spin = Mathf.MoveTowards(spin, 0f, Time.deltaTime * 0.8f);
            cup.Rotate(0f, 720f * spin * Time.deltaTime, 0f, Space.World);
        }
    }

    // A toast a moment after the level loads (once the HUD is up).
    public class DelayedToast : MonoBehaviour
    {
        string text;
        float wait;

        public static void Show(LevelKit kit, string text, float delay)
        {
            var d = kit.Root.gameObject.AddComponent<DelayedToast>();
            d.text = text;
            d.wait = delay;
        }

        void Update()
        {
            wait -= Time.unscaledDeltaTime;
            if (wait > 0f) return;
            var game = PatchworkGame.Instance;
            if (game == null || game.Mode != UIMode.Gameplay) return;
            game.ShowToast(text);
            Destroy(this);
        }
    }
}

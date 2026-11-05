using UnityEngine;

namespace Patchwork
{
    // Little whimsical noises in the Pod every so often: a robot beep-boop, an alien cooing, a
    // twinkle of bells, a wobbly "wee-oo", a wind chime, a squeaky toy. Only while you're playing
    // or decorating (not in menus), and soft enough to sit under the music.
    public class PodWhimsy : MonoBehaviour
    {
        static readonly float[] Pentatonic = { 1f, 1.125f, 1.26f, 1.5f, 1.68f, 2f };
        float wait = 4f;
        int last = -1;

        void Update()
        {
            var game = PatchworkGame.Instance;
            if (game == null || (game.Mode != UIMode.Gameplay && game.Mode != UIMode.PodEdit)) return;
            wait -= Time.deltaTime;
            if (wait > 0f) return;
            wait = Random.Range(6f, 12f);

            int pick = Random.Range(0, 7);
            if (pick == last) pick = (pick + 1) % 7;
            last = pick;
            float note = Pentatonic[Random.Range(0, Pentatonic.Length)];
            switch (pick)
            {
                case 0: Sfx.PlayPitched(SfxId.Beeper, 0.22f, Random.Range(0.9f, 1.15f)); break;
                case 1: Sfx.PlayPitched(SfxId.AlienCoo, 0.2f, Random.Range(0.9f, 1.2f)); break;
                case 2: Sfx.PlayPitched(SfxId.Twinkle, 0.2f, note > 1.6f ? 1f : note); break;
                case 3: Sfx.PlayPitched(SfxId.Warble, 0.16f, Random.Range(0.85f, 1.2f)); break;
                case 4: Sfx.PlayPitched(SfxId.Chime, 0.18f, note); Sfx.PlayPitched(SfxId.Chime, 0.12f, note * 1.5f); break;
                case 5: Sfx.PlayPitched(SfxId.Squeak, 0.12f, Random.Range(1.1f, 1.4f)); break;
                default: Sfx.PlayPitched(SfxId.Xylo, 0.16f, note); break;
            }
        }
    }
}

using UnityEngine;

namespace Patchwork
{
    /// <summary>Shorthand for a fabric-scrap burst (pickups, respawn, level complete).</summary>
    public static class TransformBurst
    {
        public static void Spawn(Vector3 position, Color a, Color b, int count, float lifetime)
        {
            FxBurst.Spawn(FxBurst.Kind.Scraps, position, a);
            if (count > 16) FxBurst.Spawn(FxBurst.Kind.Sparkle, position, b);
        }
    }
}

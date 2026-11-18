using UnityEngine;

namespace Patchwork
{
    // Six souvenirs for the Pod, hidden about the Oceanic Museum in prize bubbles: a ship in a
    // bottle in the Keeper's room, a jellyfish lamp among the kelp, an ammonite in the Deep Time
    // hall, a brass telescope in the Star Corridor, a star chart in the Hall of Arms and a whale
    // plush in the sunken ruins. Each one turns up in the Pod once found.
    public static partial class Level2Builder
    {
        static void PodTreasures(LevelKit kit)
        {
            Physics.SyncTransforms();
            PlaceTreasure(kit, new Vector3(-82f, 2.2f, 90f), "deco_ship");
            PlaceTreasure(kit, new Vector3(KelpX0 + 8f, 3.2f, 40f), "deco_jelly");
            PlaceTreasure(kit, new Vector3(HistX0 + 8f, 2.5f, HistZ0 + 14f), "deco_ammonite");
            PlaceTreasure(kit, new Vector3(36f, DeckY + 1.8f, -20f), "deco_telescope");
            PlaceTreasure(kit, new Vector3(ArmsX0 + 4f, 2.5f, ArmsZ0 + 4f), "deco_starchart");
            PlaceTreasure(kit, new Vector3(RuinX0 + 6f, 3f, RuinZ1 - 6f), "deco_whale");
        }

        // A bubble at about that spot, lifted until it's clear of anything solid (so it never
        // sits inside an exhibit or a wall).
        static void PlaceTreasure(LevelKit kit, Vector3 at, string prize)
        {
            Vector3 p = at;
            for (int k = 0; k < 24 && Physics.CheckSphere(p, 1.2f, ~0, QueryTriggerInteraction.Ignore); k++) p += Vector3.up * 0.5f;
            if (Physics.CheckSphere(p, 1.2f, ~0, QueryTriggerInteraction.Ignore)) p = at;
            PrizeBubble.Place(kit, p, prize);
        }
    }
}

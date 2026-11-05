using UnityEngine;

namespace Patchwork
{
    public enum PrizeKind { Sticker, Hat, Decoration }

    public class PrizeDef
    {
        public readonly string Id;
        public readonly string Name;
        public readonly PrizeKind Kind;
        public readonly int Value;   // sticker id, hat index or decoration index

        public PrizeDef(string id, string name, PrizeKind kind, int value)
        {
            Id = id; Name = name; Kind = kind; Value = value;
        }

        public string KindText
        {
            get { return Kind == PrizeKind.Sticker ? "New sticker" : Kind == PrizeKind.Hat ? "New hat" : "New Pod decoration"; }
        }
    }

    // Everything a prize bubble can hold. Stickers go in the sticker book, hats in the Satchel,
    // and decorations appear in the Pod.
    public static class Prizes
    {
        public static readonly string[] DecorationNames = { "Duck Captain", "Paper Lanterns", "Snow Globe", "Pinwheels", "Grumblewool Plush" };

        public static readonly PrizeDef[] All =
        {
            new PrizeDef("prize_moon", "Sleepy Moon", PrizeKind.Sticker, (int)StickerId.Moon),
            new PrizeDef("prize_music", "Music Note", PrizeKind.Sticker, (int)StickerId.Music),
            new PrizeDef("prize_crown", "Little Crown", PrizeKind.Sticker, (int)StickerId.Crown),
            new PrizeDef("prize_fish", "Goldfish", PrizeKind.Sticker, (int)StickerId.Fish),
            new PrizeDef("prize_rainbow", "Rainbow", PrizeKind.Sticker, (int)StickerId.Rainbow),
            new PrizeDef("prize_paw", "Paw Print", PrizeKind.Sticker, (int)StickerId.Paw),
            new PrizeDef("prize_partyhat", "Party Hat", PrizeKind.Hat, 5),
            new PrizeDef("prize_piratehat", "Pirate Hat", PrizeKind.Hat, 6),
            new PrizeDef("prize_propeller", "Propeller Cap", PrizeKind.Hat, 7),
            new PrizeDef("deco_duck", "Duck Captain", PrizeKind.Decoration, 0),
            new PrizeDef("deco_lanterns", "Paper Lanterns", PrizeKind.Decoration, 1),
            new PrizeDef("deco_snowglobe", "Snow Globe", PrizeKind.Decoration, 2),
            new PrizeDef("deco_pinwheels", "Pinwheels", PrizeKind.Decoration, 3),
            new PrizeDef("deco_plush", "Grumblewool Plush", PrizeKind.Decoration, 4),
        };

        public static PrizeDef Get(string id)
        {
            foreach (var p in All) if (p.Id == id) return p;
            return null;
        }

        public static void Grant(PrizeDef p)
        {
            if (p == null) return;
            GameState.AddPrize(p.Id);
            if (p.Kind == PrizeKind.Hat) GameState.UnlockHat(p.Value);
        }

        public static bool HasDecoration(int index)
        {
            foreach (var p in All)
                if (p.Kind == PrizeKind.Decoration && p.Value == index) return GameState.HasPrize(p.Id);
            return false;
        }

        public static int Count(PrizeKind kind, out int total)
        {
            int n = 0;
            total = 0;
            foreach (var p in All)
            {
                if (p.Kind != kind) continue;
                total++;
                if (GameState.HasPrize(p.Id)) n++;
            }
            return n;
        }

        // Stickers you can use: the four starters plus every sticker prize found.
        public static bool OwnsSticker(StickerId id)
        {
            foreach (var s in StickerArt.Starters) if (s == id) return true;
            foreach (var p in All)
                if (p.Kind == PrizeKind.Sticker && p.Value == (int)id) return GameState.HasPrize(p.Id);
            return false;
        }
    }
}

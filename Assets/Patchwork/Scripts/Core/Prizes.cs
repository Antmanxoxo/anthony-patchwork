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
        public static readonly string[] DecorationNames = { "Duck Captain", "Paper Lanterns", "Snow Globe", "Pinwheels", "Grumblewool Plush", "Sleepy Owl",
            "Ship in a Bottle", "Jellyfish Lamp", "Ammonite Fossil", "Brass Telescope", "Star Chart", "Whale Plush" };

        public static readonly PrizeDef[] All =
        {
            new PrizeDef("prize_moon", "Sleepy Moon", PrizeKind.Sticker, (int)StickerId.Moon),
            new PrizeDef("prize_music", "Music Note", PrizeKind.Sticker, (int)StickerId.Music),
            new PrizeDef("prize_crown", "Little Crown", PrizeKind.Sticker, (int)StickerId.Crown),
            new PrizeDef("prize_fish", "Goldfish", PrizeKind.Sticker, (int)StickerId.Fish),
            new PrizeDef("prize_rainbow", "Rainbow", PrizeKind.Sticker, (int)StickerId.Rainbow),
            new PrizeDef("prize_paw", "Paw Print", PrizeKind.Sticker, (int)StickerId.Paw),
            // Story stickers: earned for the big moments (see GrantSticker).
            new PrizeDef("sticker_lute", "Lute", PrizeKind.Sticker, (int)StickerId.Lute),
            new PrizeDef("sticker_trumpet", "Trumpet", PrizeKind.Sticker, (int)StickerId.Trumpet),
            new PrizeDef("sticker_feather", "Wisp's Feather", PrizeKind.Sticker, (int)StickerId.Feather),
            new PrizeDef("sticker_cake", "Wedding Cake", PrizeKind.Sticker, (int)StickerId.Cake),
            new PrizeDef("sticker_shield", "Hall of Arms Shield", PrizeKind.Sticker, (int)StickerId.Shield),
            new PrizeDef("sticker_lens", "Night Glass Lens", PrizeKind.Sticker, (int)StickerId.Lens),
            new PrizeDef("sticker_helmet", "Helmet Crab", PrizeKind.Sticker, (int)StickerId.Helmet),
            new PrizeDef("sticker_telescope", "The Night Glass", PrizeKind.Sticker, (int)StickerId.Telescope),
            new PrizeDef("sticker_whale", "The Great Whale", PrizeKind.Sticker, (int)StickerId.Whale),
            new PrizeDef("sticker_buttonmoon", "Button Moon", PrizeKind.Sticker, (int)StickerId.ButtonMoon),
            new PrizeDef("prize_partyhat", "Party Hat", PrizeKind.Hat, 5),
            new PrizeDef("prize_piratehat", "Pirate Hat", PrizeKind.Hat, 6),
            new PrizeDef("prize_propeller", "Propeller Cap", PrizeKind.Hat, 7),
            new PrizeDef("prize_wizardhat", "Starry Wizard Hat", PrizeKind.Hat, 8),
            new PrizeDef("deco_duck", "Duck Captain", PrizeKind.Decoration, 0),
            new PrizeDef("deco_lanterns", "Paper Lanterns", PrizeKind.Decoration, 1),
            new PrizeDef("deco_snowglobe", "Snow Globe", PrizeKind.Decoration, 2),
            new PrizeDef("deco_pinwheels", "Pinwheels", PrizeKind.Decoration, 3),
            new PrizeDef("deco_plush", "Grumblewool Plush", PrizeKind.Decoration, 4),
            new PrizeDef("deco_owl", "Sleepy Owl", PrizeKind.Decoration, 5),
            // From the Oceanic Museum (Level 2).
            new PrizeDef("deco_ship", "Ship in a Bottle", PrizeKind.Decoration, 6),
            new PrizeDef("deco_jelly", "Jellyfish Lamp", PrizeKind.Decoration, 7),
            new PrizeDef("deco_ammonite", "Ammonite Fossil", PrizeKind.Decoration, 8),
            new PrizeDef("deco_telescope", "Brass Telescope", PrizeKind.Decoration, 9),
            new PrizeDef("deco_starchart", "Star Chart", PrizeKind.Decoration, 10),
            new PrizeDef("deco_whale", "Whale Plush", PrizeKind.Decoration, 11),
        };

        // A story sticker, the first time something big happens. Shows what you got.
        public static void GrantSticker(StickerId id)
        {
            foreach (var p in All)
            {
                if (p.Kind != PrizeKind.Sticker || p.Value != (int)id) continue;
                if (GameState.HasPrize(p.Id)) return;
                Grant(p);
                var game = PatchworkGame.Instance;
                if (game != null) game.ShowToast("New sticker: " + p.Name + "!  (Stick it anywhere with the Sticker button.)");
                Sfx.Play(SfxId.StickerSlap, 0.5f);
                return;
            }
        }

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
            if (GameState.DevMode) return true;
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
            if (GameState.DevMode) return true;
            foreach (var s in StickerArt.Starters) if (s == id) return true;
            foreach (var p in All)
                if (p.Kind == PrizeKind.Sticker && p.Value == (int)id) return GameState.HasPrize(p.Id);
            return false;
        }
    }
}

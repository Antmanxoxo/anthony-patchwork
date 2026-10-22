using UnityEngine;

namespace Patchwork
{
    public enum FormType
    {
        Water = 0,
        Yarn = 1,
        Wind = 2
    }

    // Movement parameters, collision size and presentation data for one form.
    // Shared movement code reads these; form-specific behaviour lives in FormAbility subclasses.
    public class FormDefinition
    {
        public FormType Type;
        public string DisplayName;
        public string Tagline;
        public string AbilityHint;
        public Color PrimaryColor;
        public Color SecondaryColor;

        // Movement
        public float MoveSpeed = 6.5f;
        public float GroundAcceleration = 60f;
        public float AirAcceleration = 22f;
        public float JumpHeight = 2.0f;
        public float GravityScale = 1f;
        public float MaxFallSpeed = 30f;

        // Collision capsule (pivot is at the feet)
        public float ControllerHeight = 1.4f;
        public float ControllerRadius = 0.45f;

        // Where cosmetics (hats) sit, relative to the feet
        public float HeadHeight = 1.35f;
    }

    public static class FormLibrary
    {
        public static readonly FormType[] AllForms = { FormType.Water, FormType.Yarn, FormType.Wind };

        static FormDefinition water, yarn, wind;

        public static FormDefinition Get(FormType type)
        {
            if (water == null) Build();
            switch (type)
            {
                case FormType.Water: return water;
                case FormType.Wind: return wind;
                default: return yarn;
            }
        }

        static void Build()
        {
            water = new FormDefinition
            {
                Type = FormType.Water,
                DisplayName = "Water Form",
                Tagline = "A jiggly bubble with big eyes. Slips through drain grates and rides blue fountains.",
                AbilityHint = "Slip through blue drain grates. Step into blue fountains to ride them up.",
                PrimaryColor = new Color(0.36f, 0.66f, 0.95f),
                SecondaryColor = new Color(0.78f, 0.92f, 1f),
                MoveSpeed = 7.2f,
                JumpHeight = 1.3f,
                GravityScale = 1.1f,
                ControllerHeight = 0.8f,
                ControllerRadius = 0.32f,
                HeadHeight = 0.82f
            };

            yarn = new FormDefinition
            {
                Type = FormType.Yarn,
                DisplayName = "Yarn Form",
                Tagline = "Patch's little knitted self. Throws a yarn arm at pink anchors and swings across gaps.",
                AbilityHint = "Near a glowing pink anchor, hold Ability to swing. Let go (or Jump) to launch.",
                PrimaryColor = new Color(0.93f, 0.45f, 0.58f),
                SecondaryColor = new Color(1f, 0.85f, 0.6f),
                MoveSpeed = 6.5f,
                JumpHeight = 2.0f,
                GravityScale = 1f,
                ControllerHeight = 1.4f,
                ControllerRadius = 0.45f,
                HeadHeight = 1.4f
            };

            wind = new FormDefinition
            {
                Type = FormType.Wind,
                DisplayName = "Wind Form",
                Tagline = "A happy floating cloud. Glides, falls slowly and bursts forward on a gust.",
                AbilityHint = "Hold Jump in the air to glide. Press Ability for a gust (once per jump). White updrafts lift you.",
                PrimaryColor = new Color(0.86f, 0.95f, 0.97f),
                SecondaryColor = new Color(0.55f, 0.85f, 0.8f),
                MoveSpeed = 6.5f,
                AirAcceleration = 26f,
                JumpHeight = 2.0f,
                GravityScale = 0.8f,
                ControllerHeight = 1.4f,
                ControllerRadius = 0.45f,
                HeadHeight = 1.2f
            };
        }
    }
}

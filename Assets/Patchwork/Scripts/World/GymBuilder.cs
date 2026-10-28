using UnityEngine;

namespace Patchwork
{
    // A plain test room for checking movement numbers (Patchwork > Play Test Gym in the editor).
    // Floor stripes every metre (gold every 5 m), height steps of 1-4 m, a lane of gaps from 3 m to
    // 9 m, a 15 m swing gap with an anchor, 7 m and 11 m Wind gaps, a bounce pad and an updraft.
    // A readout at the top right shows each jump's distance and height.
    public static class GymBuilder
    {
        public const string Id = "Gym";

        public static LevelInfo Build(LevelKit kit)
        {
            var info = kit.Info;
            info.Id = Id;
            info.IsHub = true; // no return-to-hub option, cards always show
            info.Title = "Test Gym";
            info.Subtitle = "Every stripe is 1 m. Gold stripes are every 5 m.";
            info.Spawn = new Vector3(0f, 0.1f, -2f);
            info.SpawnYaw = 0f;
            info.KillY = -10f;
            info.Sky = new Color(0.85f, 0.85f, 0.9f);

            kit.Backdrop(-30f);
            var plain = new[] { new Color(0.86f, 0.84f, 0.8f), new Color(0.8f, 0.79f, 0.76f) };

            // Measuring floor, z -6 .. 34, with stripes from z = 0.
            kit.Quilt(new Vector3(0f, -0.5f, 14f), new Vector3(12f, 1f, 40f), plain, 4f, "GymFloor");
            var line = PatchworkAssets.Plain(new Color(0.3f, 0.3f, 0.36f), 0.1f);
            var gold = PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.2f);
            for (int z = 0; z <= 30; z++)
                kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.025f, z), new Vector3(12f, 0.02f, z % 5 == 0 ? 0.12f : 0.04f), z % 5 == 0 ? gold : line);

            // Height steps beside the floor: 1, 2, 3 and 4 m tall.
            for (int i = 0; i < 4; i++)
                kit.Block(new Vector3(-7.5f, (i + 1) * 0.5f, 4f + i * 4f), new Vector3(3f, i + 1, 3f), LevelKit.Palette.Lavender, "Step" + (i + 1) + "m");

            // Gap lane (x = 12): platforms 3 m long with gaps of 3, 4, 5, 6, 7, 8 and 9 m.
            kit.Quilt(new Vector3(12f, -0.5f, -3.5f), new Vector3(4f, 1f, 5f), plain, 3f, "GapStart");
            float zEdge = -1f;
            for (int gap = 3; gap <= 9; gap++)
            {
                float start = zEdge + gap;
                kit.Quilt(new Vector3(12f, -0.5f, start + 1.5f), new Vector3(4f, 1f, 3f), plain, 3f, "Gap" + gap + "m");
                zEdge = start + 3f;
            }
            // Walkway back to the start.
            kit.Quilt(new Vector3(8.5f, -0.5f, 14f), new Vector3(3f, 1f, 40f), plain, 4f, "GapWalkway");
            // Strip along the back that joins every lane's start.
            kit.Quilt(new Vector3(3f, -0.5f, -8f), new Vector3(44f, 1f, 4f), plain, 4f, "BackStrip");

            // Swing lane (x = -16): a 15 m gap with an anchor in the middle, like Level 1.
            kit.Quilt(new Vector3(-16f, -0.5f, -1f), new Vector3(6f, 1f, 10f), plain, 4f, "SwingStart");
            kit.Anchor(new Vector3(-16f, 8f, 11.5f), 13.5f);
            kit.Quilt(new Vector3(-16f, -0.5f, 23f), new Vector3(6f, 1f, 8f), plain, 4f, "SwingEnd");

            // Wind lane (x = 22): a 7 m gap (glide only) then an 11 m gap (glide + gust).
            kit.Quilt(new Vector3(22f, -0.5f, -1f), new Vector3(6f, 1f, 10f), plain, 4f, "WindStart");
            kit.Quilt(new Vector3(22f, -0.5f, 15f), new Vector3(6f, 1f, 8f), plain, 4f, "Wind7m");
            kit.Quilt(new Vector3(22f, -0.5f, 34f), new Vector3(6f, 1f, 8f), plain, 4f, "Wind11m");

            kit.AddBouncePad(new Vector3(-3f, 0f, 26f));
            kit.Updraft(new Vector3(3f, 0f, 26f), 10f);
            kit.AddCheckpoint(new Vector3(0f, 0f, -2f), 0f);
            return info;
        }
    }

    // Measures each jump: how far it went across the ground and how high it got.
    public class JumpMeter
    {
        public float LastDistance { get; private set; }
        public float LastHeight { get; private set; }
        public float BestDistance { get; private set; }
        public bool HasJump { get; private set; }

        bool inAir;
        Vector3 takeoff;
        float peak;

        public void Tick(PlayerMotor motor)
        {
            Vector3 p = motor.transform.position;
            if (!motor.Grounded)
            {
                if (!inAir) { inAir = true; takeoff = p; peak = p.y; }
                peak = Mathf.Max(peak, p.y);
            }
            else if (inAir)
            {
                inAir = false;
                LastDistance = new Vector2(p.x - takeoff.x, p.z - takeoff.z).magnitude;
                LastHeight = peak - takeoff.y;
                if (LastDistance > BestDistance) BestDistance = LastDistance;
                HasJump = true;
            }
        }
    }
}

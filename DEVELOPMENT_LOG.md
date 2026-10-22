# Development Log — Patchwork

## 2026-10-22 — Prototype 1 verified
- Opened the project in Unity, it compiled, and the game played.
- Pulled back the files Unity generated on first open (`Resources/PW_Lit.mat`, URP project settings).
- Merged `dev` into `main` and tagged **`prototype-1`** as the restore point.

## 2026-10-22 — Project set-up and Milestone 1 (unverified)
- Confirmed the engine: **Unity 6 (6000.6.7f1), URP, C#** (an empty Unreal project in another folder is unrelated and was not touched).
- Committed the Universal 3D template as the Git baseline and pushed it to `Antmanxoxo/anthony-patchwork`.
- Added a Unity `.gitignore`/`.gitattributes`.
- Built the whole prototype in code so that no hand-authored scenes/prefabs are needed:
  - **Architecture:** one `PlayerMotor` shared by all forms; `FormDefinition` (numbers) + `FormAbility` (behaviour) per form;
    `FormController` handles safe transforms (defer while swinging, refuse when the body wouldn't fit, keep feet position, clear transient state).
  - **Water:** 0.8 m capsule, ignores `WaterGrate` colliders, lifted by `WaterCurrent` fountains.
  - **Yarn:** anchor targeting with line of sight, constrained-pendulum swing moved through the CharacterController, reel-in for energy, hold to swing / release or Jump to launch.
  - **Wind:** glide caps fall speed at 2 m/s while Jump is held, one 14 m/s gust per airtime, lifted by `WindUpdraft`.
  - **UI:** radial Form Wheel (slow-mo), Satchel (forms, golden buttons, hat), pause, HUD with PlayStation glyphs drawn at runtime.
  - **Levels:** Hub ("The Sewing Room") with practice spots, Level 1 ("The Quilted Meadow"): grate wall → fountain room → 15 m yarn gap → 9 m wind gap + updraft shelf → goal.
  - Generated fabric/yarn textures, PS button glyphs, form icons and synthesised sound effects.
- Editor helper: Play always starts from the Hub; **Patchwork** menu (open scenes, toggle play-from-hub, reset save).
- Status: code complete, **not yet compiled**; waiting on the first Play test.

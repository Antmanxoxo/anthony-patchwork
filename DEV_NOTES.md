# Dev Notes

Stuff I need to remember about how Patchwork is put together.

## Setup
- Unity 6 (6000.6.7f1), URP, C#. Input System only (old input manager is off).
- There are no hand-built scenes or prefabs. `Hub.unity` and `Level1.unity` only have a Global Volume
  and a `LevelBootstrap`. When you press Play, `PatchworkGame.Boot()` builds the level, lights,
  camera, player and HUD in code.
- Almost all the art, textures, icons, sounds and music are made in code (`PatchworkAssets`,
  `PatchworkTextures`, `UIArt`, `Sfx`, `MusicPlayer`). The one imported asset is Patch himself
  (Yarn form): `Resources/Characters/Patch.fbx` with baked `Patch_Albedo.png` / `Patch_Normal.png`.
- Patch's source files are in `Art/` next to the project (not inside Assets):
  `Patch_Yarn.blend` is the rigged, animated version I export from, `Patch_Yarn_Parts.blend` has
  every piece separate for design changes. Re-export = bake textures, then FBX into `Resources/Characters`.
- `PatchModelImport` (Editor) sets the import options for that folder: generic rig, no materials,
  looping clips, normal map type.
- UI is OnGUI, drawn at 1080p and scaled (`UIKit`). Might move to UI Toolkit later.
- `PatchworkEditorSetup` makes Play always start in the Hub and saves `Resources/PW_Lit.mat`
  so the URP Lit shader makes it into builds.

## Where things are (`Assets/Patchwork/Scripts`)
- **Core** - `PatchworkGame` (main loop, menus, checkpoints, loading), `GameState` (save data),
  `PatchworkAssets` / `PatchworkTextures` (materials + textures), `Sfx`, `MusicPlayer`
- **Input** - `PatchworkInput` (all controls)
- **Player** - `PatchworkPlayer`, `PlayerMotor` (one movement script for every form), `PlayerCamera`
- **Forms** - `FormDefinition` (numbers per form), `FormController` (switching),
  `WaterAbility` / `YarnAbility` / `WindAbility`, `FormVisuals` (bodies + change animations), `FxBurst`,
  `PatchModel` (loads the Blender model, picks his animation from movement, swaps his three faces)
- **World** - `LevelKit` (building blocks), `HubBuilder`, `Level1Builder`, and the interactive bits
  (`PlayerVolume` and friends, `YarnAnchor`, `WaterGrate`, `BouncePad`, `MovingPlatform`, `ThreadSpool`)
- **UI** - `UIKit`, `UIArt`, `PatchworkHUD`, `FormSelector` (wheel), `SatchelMenu`, `PauseMenu`

## Rules I'm sticking to
- One motor for all forms. Forms only change numbers in `FormDefinition` plus their own ability.
- Order every frame: form controller -> ability PreMove -> volumes -> motor -> ability PostMove -> visuals.
- Switching forms waits if you're swinging, refuses if the new body won't fit, and keeps your feet in place.
- Hats are separate from forms and never reset when you change.
- If `Patch.fbx` is missing, Yarn falls back to the old block-built toy, so the game never breaks.
- Patch's animations: Idle, Walk, Run, Jump, Fall, Land, Swing, Wave (if you stand still a while),
  Celebrate (level complete). Played with the Playables API, no Animator Controller asset needed.
- Only one menu open at a time.

## Numbers the level depends on
- Normal jump goes about 5m. Wind with glide + gust goes about 12m.
- Yarn gap is 15m, Wind gap is 9m.
- Water is 0.8m tall (0.32 radius), tunnels are 1m tall. Other forms are 1.4m tall (0.45 radius).
- If I change movement numbers I need to re-check the gaps in `Level1Builder`.

## Habits
- Test every change with Play and check the Console.
- Keep it to C# 9 and older Unity APIs so it compiles everywhere.
- Never regenerate .meta files - both scenes point at the `LevelBootstrap.cs.meta` GUID.
- Commit each milestone, only push to `main` once it plays.
- Keep `PROJECT_STATUS.md` and `DEVELOPMENT_LOG.md` up to date.

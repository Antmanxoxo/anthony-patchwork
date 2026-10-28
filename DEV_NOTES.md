# Dev Notes

Stuff I need to remember about how Patchwork is put together.

## Setup
- Unity 6 (6000.6.7f1), URP, C#. Input System only (old input manager is off).
- Company name is "Little Colossus", app id `com.LittleColossus.Patchwork`. Don't change these once
  people have the game: saves and settings live under the company + product name
  (registry `HKCU\Software\Little Colossus\Patchwork` on Windows), so a rename resets them.
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
- Dance easter egg (`DanceParty`, `UIMode.Dance`): 6 face taps within 2.2 s while standing starts it,
  it stops 1.6 s after the last tap (or on Pause/Back). Songs are made in `MusicPlayer.DanceSong`,
  dances are in `FormVisuals.ApplyDance`, the camera orbit is `PlayerCamera.Cutscene`.
- Settings are in `GameSettings` (PlayerPrefs). Progress is in `GameState` (save.json).
- `CameraZone` boxes (made with `kit.AddCameraZone`) add camera distance/tilt for a section.
- Music is four stems (base + Water/Yarn/Wind tune) started together with `PlayScheduled`;
  `MusicPlayer.SetForm` crossfades the tune layer.
- Play mode doesn't reload scripts, so any new static value that should start fresh needs a line
  in `StaticReset`.
- Rumble goes through `Haptics` only (never call the gamepad directly), so pausing and quitting
  always stop it.
- Saved settings: `pw_music_volume`, `pw_sfx_volume`, `pw_rumble` (0-10).
- Colour rule: saturated pink (`Palette.AnchorPink`) is only for yarn anchors, bright blue only for
  water things, white only for Wind/updrafts. Decoration uses the softer `Rose` / `Sky`.
- Fonts: `UIKit.Style` uses `Resources/Fonts/Fredoka-Medium`, `Fredoka-SemiBold` (bold) and
  `Sniglet-ExtraBold` (bold, 30px and up). Missing files fall back to Unity's default font.
- Progress is saved in `save.json` (with a `save.json.bak` backup) in Unity's persistent data folder;
  settings stay in PlayerPrefs. Patchwork > Reset Save Data clears progress but keeps settings.
- Level 1's tutorial cards show once (stored in the save). Hub cards always show.
- `BlobShadow` puts the shadow disc under Patch; its material copies the see-through + glow setup
  so the build already keeps that shader version.

## Numbers the level depends on
- Every floor's top is at y = 0. Dropping 3m below that (`LevelInfo.FailY`) counts as a fall,
  except while swinging on yarn. Don't build anything you're meant to stand on below y = -2.
- Normal jump goes about 5m. Wind with glide + gust goes about 13.4m (the gust only gives a small
  fixed lift, so it can't re-launch the glide).
- Yarn gap is 15m, Wind gap is 7m.
- Water is 0.8m tall (0.32 radius), tunnels are 1m tall. Other forms are 1.4m tall (0.45 radius).
- If I change movement numbers I need to re-check the gaps in `Level1Builder`.
- Falling is 1.5x gravity for Yarn and Water (`FallGravityMultiplier`), but not after a launch
  (swing release, gust, bounce pad), so those distances stayed the same.

## Habits
- Test every change with Play and check the Console.
- Keep it to C# 9 and older Unity APIs so it compiles everywhere.
- Never regenerate .meta files - both scenes point at the `LevelBootstrap.cs.meta` GUID.
- Commit each milestone, only push to `main` once it plays.
- Keep `PROJECT_STATUS.md` and `DEVELOPMENT_LOG.md` up to date.

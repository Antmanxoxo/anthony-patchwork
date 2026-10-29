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
- Hint text can use `{Jump}`, `{Ability}`, `{Wheel}`, `{Satchel}` and `{Pause}`; `UIKit.RichText`
  swaps them for the button icon.
- Hats: 0-1 free, 2 = half of Level 1's threads in one run, 3 = all threads, 4 = Golden Crown from the
  Button Box (`HatPedestal`). Best thread counts are saved as "Level1=23/40".
- Footstep sound comes from the ground's material name (wicker / wood, planks, cobble, brick,
  shingle / grass, otherwise felt).
- Ambience loops (`LoopId.Meadow`, `LoopId.Room`) are requested every frame by `PatchworkGame`.
- Start-up: `TitleSequence` plays the studio logo + title the first time the hub loads after
  starting the game. The logo is `Resources/Branding/StudioLogo.png` if it exists, otherwise a
  drawn placeholder badge.
- All scene changes go through `PatchworkGame.LoadScene` -> `LoadingScreen` (async load, min 3.2 s,
  1.2 s for a restart). Level names, blurbs, colours and globe pins live in `LevelCatalog`.
- The hub's level select: `HelmStation` (stand there, press Ability) -> `LevelSelectMenu`, which
  turns the `LevelGlobe` and uses the camera's Showcase mode (`PlayerCamera.Showcase`).
- Yarn birds (`YarnBird`, `kit.YarnBird`) play their chirps from their own 3D AudioSource.
- `PaintLake` scrolls a marbled texture (`PatchworkTextures.PaintMarble`).
- NPCs: `NpcYarny` (looks, lines, 3D voice blips) + `TalkZone` (stand in it, press Ability) ->
  `DialogueBox` (UIMode.Talk). Whether you've met him is saved as the hint id "Yarny_met".
- Positional loops: `AmbientEmitter.Create(parent, pos, Sfx.Ambient(...), volume, near, far)`.
  `kit.Fountain` and `kit.Updraft` add their own.
  The AudioListener is on its own object between the camera and Patch.
- Don't let two boxes overlap with faces at the same height (they flicker). Use `kit.Prism` for
  odd shapes, or make one piece a hair shorter.
- `BlobShadow` puts the shadow disc under Patch; its material copies the see-through + glow setup
  so the build already keeps that shader version.

## Numbers the level depends on
- Every floor's top is at y = 0. Dropping 3m below that (`LevelInfo.FailY`) counts as a fall,
  except while swinging on yarn. Don't build anything you're meant to stand on below y = -2.
- Normal jump goes about 5m. Wind with glide + gust goes about 13.4m (the gust only gives a small
  fixed lift, so it can't re-launch the glide).
- Toybox Town: the square is an octagon (radius 21) round (0,0,0), spawn at (0,0,-14). Clock tower
  at z 27 (barracks behind it), Spindle Hill west over the ravine (x -51..-37, bed at -5), the Dye
  Works east over the canal (x 32..40, bed at -2), rooftops south. Toybox walls at +-90.
  `LevelInfo.FailY` is -9 there so you can climb out of the ravine and canal.
- `Level1Director` runs the wedding story: band pieces are `L1_band_lute/trumpet/piper` in the
  collected list (`BandPiece`), each mends its `FrayRegion` and adds a music stem
  (`MusicPlayer.SetBand`). Talking to the Mayor with all three starts the wedding cutscene
  (`PatchworkGame.StartCutscene`). Bonus Heartstring ids are `L1_hs_*`.
- Toys are `ToyNpc.Create(kit, feet, yaw, kind, name, colours, instrument)`; their lines are a
  `Func<string[]>` so they change with progress. Their `TalkZone` is parented to them.
- The look: `LevelLook.Apply` makes a global post-processing Volume from LevelInfo's
  Saturation / Contrast / Warmth / Glow / Vignette. Fog and draw distance are LevelInfo's
  FogColor / FogStart / FogEnd / FarClip.
- Textures: `PatchworkTextures.Build(size, height, shade, range, relief, cavityColour, edge,
  macro, macroCells, tipColour)`. Cavity colour tints the low points, edge brightens ridges.
- The Fray's look per world: set `FrayTheme.Style` (PatchworkGame.Init does it per level);
  knots, tears, tendrils and Snarlings ask FrayTheme how to build themselves.
- The map: fill `LevelInfo.MapPins` (and MapTitle / MapNote / MapArea / MapCameraHeight).
  `MapScreen` paints the level once from above (camera below any clouds) and stylises it.
- Music: tunes are written as text in `MusicKit` ("C5:0.5 F5:1 r:1"), chords by name ("Gm7").
  Hub shanty: `MusicPlayer.Compose`. Toybox: `MusicToybox.ComposeToybox` (`ToyBpm` 112).
- Toybox Town's dressing is in `Level1Details.cs` (same `Level1Builder` partial class). The
  first-visit cutscene is `ToyboxIntro` (saved as hint id "L1_intro_seen"); it uses
  `PatchworkGame.StartScriptedCutscene` + `SetCutsceneCamera` to move the camera itself.
- Lots of small shapes go through `MeshBatch` (one mesh per material). `YarnGrass.Scatter` /
  `Patches` grow ground cover only where a downward ray hits a grass/knit collider at ground
  height, so build them last. `YarnGrass.ClearZones` keeps spots bare.
- `PatchworkGame.Ask(title, body, onYes)` shows a Yes/No card (`AskBox`, `UIMode.Ask`). Home
  hoops use it.
- Moving decorations: `PathMover` (loops of points), `Rocker` (rock on one axis), `Butterfly`.
- Deep water: `WaterBody` volume (SurfaceY). Water Form gets `Motor.Swimming` + `SpeedBoost`
  each frame (lift to the surface, jump allowed); other forms splash and respawn.
  Toybox Town's canal surface is y -0.6 (bed -5), the stream -5.35 (bed -8).
- `PatchworkAssets.CraftWater(alpha, tiling)` is the crafted water material (scroll it with
  PaintLake). Its shader variant is kept for builds by `Resources/PW_Keep_SeeThroughBumpy.mat`.
- `kit.AddDrumPad` is a bounce pad dressed as a toy drum. `XyloKey` plays a note when stepped on.
- Other toys: `DominoRun` (pivots tip in turn, `Finished` callback), `BubbleBlower` +
  `BubbleBounce` (pool of bubble volumes), `WindChime`, `SqueakToy`, `BellRinger`.
- Decorations don't collide. `Level1Builder.Solid` / `TownKit.Solid` add an invisible box;
  `Bumper.Add` gives a moving prop a collider and pushes Patch away from it.
- Save version is 2 (`GameState.Version`). Older saves drop the Level 1 completion and its
  Heartstrings when they load.
- `PatchworkAssets.Plain` uses the gentle `PatchworkTextures.Paint` brushstroke texture.
- Town pieces are in `TownKit` (houses, lamps, clotheslines, lantern gates, bridges, stairs...).
  `MakeHouse` returns where its chimney top is, for a chimney-smoke updraft.
- Fountain rule: height = shelf top + 2, shelf edge 1.2-2.2 m from the jet. Updraft height = rise.
- `kit.Anchor(pos, range, style, hangLength)`: the thread is only drawn when hangLength > 0, so
  give it something to hang from.
- Fray knots (`SnarlKnot`) and `Snarling`s pop when Patch lands on them moving down, crashes in
  faster than 9 m/s, gusts into them, or swings into them. Otherwise they push him away.
- Yarn gaps are 15m, Wind glide gap is 7m, gust gap is 11m. The rim swing goes from the thimble
  (top 6m) to the rim (top 6m).
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

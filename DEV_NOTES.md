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
  (`PlayerVolume` and friends, `YarnAnchor`, `WaterGrate`, `BouncePad`, `ThreadSpool`)
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
- Saved settings: `pw_music_volume`, `pw_sfx_volume`, `pw_rumble` (0-10), `pw_look` (0 plain,
  1 Storybook, 2 Stitched).
- The look is `Shaders/PatchworkLook.shader`, run by a Full Screen Pass renderer feature called
  "PatchworkLook" that the editor setup adds to every URP renderer (material
  `Resources/PW_Look.mat`). It reads the global `_PatchworkLook`, set by
  `GameSettings.ApplyLook()`. If the shader ever fails to compile, the setup switches the pass
  off and logs a warning, so the game still runs in the plain look.
- Photo Mode is `UIMode.Photo` (`PhotoMode.cs`): time scale 0, the camera driven through the
  showcase shot. The UI is hidden for a couple of frames around the capture so it isn't in the
  picture.
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
- The hub is the Pod (`HubBuilder`): an egg-shaped shell of curved cardboard panels (28 columns,
  rings from `ProfileY` / `ProfileR`), a window at +z with invisible panes, and the Button Box
  room through a doorway at the back. `LevelInfo.QuietMusic` keeps the music silent while you
  walk about; it comes in when the level select opens.
- The hub's level select: `HelmStation` (the controller on the desk; stand there, press Ability)
  -> `LevelSelectMenu`, which turns the `LevelGlobe` floating outside the window and uses the
  camera's Showcase mode (`PlayerCamera.Showcase`). The camera spot is `LevelGlobe.ViewOffset`.
- Pod wallpaper: `PodTheme` (saved as `pw_pod_theme`), flipped by the pattern book, a `UseSpot`
  (stand in it, press Ability; the HUD floats its label). Wall panels register with
  `PodTheme.Register`. Printed felt patterns (zigzag, polka dots, gingham, stars, stripes):
  `PatchworkAssets.Print`.
- Arriving in the Pod: `PodLaunch` holds Patch inside the launch tube (motor `ExternalControl`)
  until the fade and any title are done, then flies him along an arc; `PodLaunch.Busy` blocks
  input. The Pod song is `MusicPod.cs` (`MusicTrack.Pod`, cued from the top by
  `MusicPlayer.CuePod()` when the level select opens). `LevelInfo.CameraRumble` wobbles the camera.
- Toybox Town clutter: `Level1Katamari.cs` scatters `Clumpable` things by raycasting for clear
  ground at y 0; ones 1.1 m and up get a box collider. `TapeBall` picks up anything up to
  1.15x its radius (volume adds as half the item's size cubed) and becomes a star at 1.8 m.
  `Grabbable.Rolls` gives it a smooth push (no hop); `NoGrab` keeps it out of Grab.
- Hearts: `PatchworkGame.Hearts` / `MaxHearts` (3), `Hurt(from)` (knockback + 1.6 s of
  flicker via `FormVisuals.SetHidden`), `Heal(n)`, and the unravel-and-respawn when they run out.
  No damage in the hub, or outside gameplay. Creatures share `Monsters` (`Touch`, `Popped` for
  combos and heart drops, `Gust` for Wind) and tag themselves with `FrayCreature`. Toybox Town's
  creatures and hearts are placed in `Level1Monsters.cs` with `FindSpot` (clear ground near a
  point) and `PatrolEnd`.
- The HUD slides its panels in and out (`PatchworkHUD.UpdateVisibility` / `Shifted`).
- The Big Top (`Level1BigTop.cs`) is a room at (250, 0, -350) reached by a `Warp` at the town
  tent (placed with `FindSpot`). Its parts live in `BigTop.cs`: `Grumblewool` (boss state machine,
  `Active` drives the HUD boss bar, resets when Patch unravels), `Shockwave`, `WhackASnarl` (a
  `TimedChallenge`), `Crowd` (`Crowd.Cheer()`), `ScriptedHop` (the cannon's flight; blocks input
  like `PodLaunch`) and `TouchZone`. Beating the boss saves `L1_bigtop_boss`.
- Merit Patches: `Merits.cs`. Earned ones are saved as hint ids `merit_<Id>`; running counts in
  PlayerPrefs (`pw_mc_*`). `Merits.Earn(id)` from events, `Merits.Tick` polls the rest.
- Juice: `PatchworkGame.HitStop(seconds)` and `PlayerCamera.Kick(amount)`.
- Toybox Town's size is `Level1Builder.BoxHalf` (115). Everything along the walls is placed
  relative to it. The start is `SpawnPoint` in Landing Meadow; `SquareSpot` is the old start
  (the square's checkpoint and the ink hole's target). New areas are in `Level1Expansion.cs`.
- Arriving by Pod: `LandedPod.Build` (the exterior model) and `PodLaunch` with `Door` and
  `Gentle` set (the door swings open, then Patch hops out).
- The Pod's decorating menu: `PodEditor` (`UIMode.PodEdit`) over `PodDecor` (floor, rug, light,
  items on/off; PlayerPrefs `pw_pod_*`) and `PodTheme` (walls). Things that can be switched are
  built inside `HubBuilder.Collect(kit, key, build)`, which groups whatever the build made.
  The editor camera is a `PlayerCamera` showcase shot set in `PodEditor.Open` and must stay
  inside the shell (`ProfileY`/`ProfileR`: about 8.8 m radius at 6.5 m up).
- Pod sounds: `PodWhimsy` (on the PodSounds group) plays a random soft one-shot every 6-12 s in
  Gameplay / PodEdit (`SfxId.Beeper`, `AlienCoo`, `Twinkle`, `Warble`, plus pitched chimes).
  `Sfx.SoftRocket` makes `PodPop` (launch tube) and `PodRocket` (level select launch).
  Outside props from the storybook sky are in `HubBuilder.StorybookSky`.
- `RenderOptimizer.Run` (called in `PatchworkGame` after the level is built): swaps built-in
  sphere/capsule meshes under 2.6 m / 3.2 m for lighter generated ones, turns off shadows for
  things under 0.5 m, and moves plain collider-less decorations to layer 8 (small, culled at
  80 m) or 9 (medium, 180 m) via `Camera.layerCullDistances`. It logs what it did. Objects made
  after the level is built (effects, dropped hearts) aren't touched.
- Per-level sun and title camera: `LevelInfo.SunEuler` / `SunIntensity` and `TitleCenter` /
  `TitleRadius` / `TitleHeight` / `TitleLookOffset`.
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
- Grab: `GrabHand` on the player (`Player.Hand`), `Grabbable` on physics toys (builders:
  `Grabbable.Crate / ToyBlock / BeachBall`). `Motor.FacingOverride` and `Motor.AirCarry` are
  per-frame / per-launch helpers. Standing on anything with a `Carrier` moves Patch with it.
- Contraptions (`Contraptions.cs`): `Lever` (Grab to pull, `Pulled`), `PressurePad`
  (`Changed`, MinMass; crates ~40, Patch counts 10), `Gate`, `Seesaw` (Drop, launches to
  Target), `Jiggle`.
- Score: `PatchworkGame.Score` (`ScoreKeeper`). `ScoreBubbles.Place` adds to
  `LevelInfo.ScorePossible`; medals are 50% / 100% / 160% of it. `ScoreBubbles.BurstAt` for
  rewards, `ScoreKeeper.Bonus` for points.
- Prizes: `Prizes.All` (stickers, hats 5-7, decorations 0-3). `PrizeBubble.Place(kit, pos, id,
  hidden)`, `Reveal()` to show a hidden one. Hub decorations are in `HubBuilder.Decorations`.
- Stickers: `StickerSystem` (UIMode.Sticker), art in `StickerArt`, saved per level in the save
  file. `StickerSwitch` is a spot that wants a sticker (`Done`).
- Toybox Town's playground layer is `Level1Fun.cs` (score trails, prizes, boards, workshop).
- Secrets: `Level1Secrets.cs` places them; the pieces are in `Secrets.cs` (`SecretMarble`,
  `Warp`, `DigSpot`, `Peekaboo`, `TimedChallenge` / `BubbleRush` / `StitchPath`, `GoalNet`,
  `Hatch`, `Duckling`, `MusicBox`). Marble ids are `Level1Builder.MarbleIds`, listed in
  `LevelInfo.SecretIds`. The hidden rooms are built out in the nursery (around x 250,
  z -200 and -262) where the box walls hide them.
- `ScoreBubbles.Settle(kit)` runs after the level is built and moves bubbles, prize bubbles
  and marbles out of anything they overlap. Place things after it and they won't be checked.
- `LevelAudit` (editor only) runs 1.5 s after a level loads in Play mode and writes
  `Logs/PatchworkAudit_<level>.txt`: walk-through props, flickering faces, collectibles stuck in
  solids, broken materials and counts.
- Builds: Patchwork > Build for Windows. Every shader keyword combo the game turns on at runtime
  needs a `PW_Keep_*` material in Resources (the editor setup recreates them). The app icon is
  `Assets/Patchwork/Icons/PatchworkIcon.png`, applied on editor load.
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

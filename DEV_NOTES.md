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
- Hats: `FormVisuals.accessoryRoot`. For Patch's Blender model it's set in `PlaceHatOnHead`
  (subscribed to `PatchModel.PoseApplied`, which runs at the end of its LateUpdate) from
  `PatchModel.HatPose`: the head bone's rotation relative to its rest pose (captured at load)
  and a fit scale from the head size. Don't move it in Update for that model, or it lags a frame.
- Button prompts: use `UIKit.PromptWidth` to lay out text next to `UIKit.Prompt`; keyboard key
  caps are wider than pad glyphs.
- Outskirts: `Level1Outskirts.cs`, built by `Outskirts(kit)` after `ApplySavedState` (before
  ground cover, so `ClearGrass` works). Placement goes through `Spot` (FindSpot plus our own
  `taken` circles). Toy animals are `ToyCritter` (wander near `Home`, flee within `Shy`, skip
  updating beyond 90 m). Trails only lay stones on open ground at y 0.
- Spooky music: `MusicSpooky.cs`. `MusicPlayer.SetSpookyArea(levelId, bounds)` composes it and
  sets the area; it plays (and ducks everything else) while the player is inside.
  `MusicPlayer.SpookyMix` drives `SpookyLook` (a priority-11 global Volume: darker, bluer).
- The wood secret: `WoodSecret` + three `SpiritLantern` volumes; opens the hollow oak and
  reveals `L1_marble_wood` and the `deco_owl` prize (Pod decoration 5, built in
  `HubBuilder.Decorations`). `PeekingEyes`, `FireflySwarm` (also butterflies, `Pulse = false`)
  and `Smoker` are small helpers in `WoodSecret.cs`.
- The landed Pod (`LandedPod.Build`) builds its crater too; the rim is tilted solid slabs
  (`Slope`) so it's walkable. The `PodHome` UseSpot at the hatch asks to go home.
- Level Complete: Cancel returns to Gameplay ("Stay and explore").
- The wood: `Level1Forest.cs` (`ForestExpansion`, `BuildMoonGlade`, `WishingWillow`,
  `RavineCrossings`); the deep corner is still `WhisperingWood` in `Level1Outskirts.cs`.
  `ForestArea` sets the spooky music/look. Trails are reserved first as `lanes`
  (`TrailLines()` + `ForestPaths`) so nothing is planted on them; `Spot` checks `NearLane`.
- Moon secret: `MoonMagic.cs` (`MoonGlade`, `MoonNeedle`, `Floater`). The needle is saved as
  collected id `L1_moon_needle`; the glade reveals `L1_marble_moon` and `prize_wizardhat`
  (hat 8, "Starry Wizard Hat").
- Riding: `YarnHorse` (built by `YarnHorse.Build`). Mounting sets `FormVisuals.RideHeight`
  and `PatchModel.Riding` (idle pose), and the horse adds `PlayerMotor.SpeedBoost` each
  LateUpdate while grounded. It knocks you off when swimming, warping or under external control.
- `LayoutSurvey.Run` (editor only) writes Logs/patchwork_layout_<level>.txt: a 4 m grid of
  how much is drawn per cell, and of solid heights.
- Performance: `StaticCombine.Run(root)` runs after `RenderOptimizer` in levels (not the hub).
  It treats as "live" every transform a MonoBehaviour is on or references (fields, lists,
  dictionaries, delegate closures, static fields in the Patchwork namespace) plus their
  children, and merges everything else that's opaque, single-material and under 2500 verts.
  If a new script changes a plain decoration it doesn't hold a reference to, give it one (or put
  any MonoBehaviour on that object) so it stays separate.
- Graphics setting: `GameSettings.ApplyGraphics` (copies the URP asset at runtime in builds only).
- Build output: build number in Resources/PatchworkBuild.txt, zip and "Patchwork Log.txt"
  (`BuildLog`); DirectX 11 first and fog kept are set by `PatchworkBuildChecks`.
- Builds: keep URP's "strip unused post-processing variants" OFF (Project Settings > Graphics >
  URP). `PatchworkBuildChecks` (IPreprocessBuildWithReport) enforces it. The built game's log is
  %USERPROFILE%/AppData/LocalLow/Little Colossus/Patchwork/Player.log.
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
  solids, broken materials and counts. It also lists props sunk into each other (a prop = a
  group with no mesh of its own, under 12 m across; read from mesh bounds so it still works
  after `StaticCombine`) and, in the Pod, anything sticking out past `HubBuilder.RadiusAt(y)`.
- Pod: `HubBuilder.TidyRoom` runs after `CosyDetails` and pushes `Item_*` groups inside the wall,
  out of the doorway / window desk / landing spot / centre, and apart from each other.
  `PodIdleCam` (on the level root) waits 40 s with no input, shows `PodExterior` (inactive
  otherwise) and orbits the camera with `PlayerCamera.Showcase`. Bulb light: range 10.5,
  shadows on Balanced and Best.
- `Declutter.Run(root)` runs in Level1Builder just before `ScoreBubbles.Settle`. It only looks at
  direct children of the level root, only touches "pure" decorations (no scripts but Spinner /
  Rocker / Blinker), removes the smaller of two trees closer than 0.55 x their crown radii, and
  pushes small props (< 4 m) out of trunks, solid 4-9 m things and each other (max 2 m). Tree
  names and filler names are listed at the top of the file. Add new tree types there.
- Yarn grass: `YarnGrass.Scatter` takes a colour palette (index = mesh kind; colours +
  3 + flower colours must stay <= 16), grows nearly everywhere, scales density by
  `GameSettings.Graphics`, builds 16 m chunks and puts them on `RenderOptimizer.GrassLayer` (10),
  culled at 70 m x CullScale.
- Map: places and shops are numbered in reading order (`MapScreen.Numbered`) and named in the
  key; region (Area) names sit on felt tags on the map. Add a new place with a plain `MapPin`
  and it gets a number and a key row by itself.
- Thin ground decals (biome tints) have no collider: keep them off the ravine and the canal.
- `StitchTrail` (on the player, ticked from `PatchworkPlayer.Tick`): a pool of 72 flat cubes.
  It won't sew onto anything with a `Carrier`, a rigidbody, or a collider named "Tape".
- `PropWobble.Attach(root, names...)` runs after `Declutter` in Level1Builder and springs
  the named top-level groups (their origin must be at their base). Add a name to the list to
  make a new kind of prop react. Wobbling props are not merged by `StaticCombine`, so don't add
  kinds that exist in the hundreds (ferns).
- `WobbleStone` needs a `Carrier` on the same object and its `Top` collider set. `TapeBridge`
  runs along -x from `Anchor`; the hedge on the ravine's east edge runs z 16.5..60, so new
  crossings go north of that or need a gap cut.
- The nursery mobile is on layer 1 so its small parts are never distance-culled; its lowest
  toy must stay above `MapCameraHeight` (70) or it shows up in the map picture.
- Art direction (Toybox Town): bright pastel diorama. Box walls = sky-blue felt with paper
  hills and clouds (`Toybox` in Level1Builder); keep big backdrops light. Texture relief is set
  in `PatchworkTextures.EnsureBuilt` (base shade, shade range, normal strength): keep base
  >= 0.58 so dips never go near black. Grass: `MeshBatch.Strand` (blunt), Felt material, first
  3 palette colours are the greens, the rest are blossom drifts.
- Map picture: `MapScreen.Stylize` blurs, snaps every pixel to `Land` colours or 12 pastel
  hues (`MapColour`), then outlines where kinds meet. If the ground colour changes a lot,
  check it still falls in the "meadow" range (hue 62-170, value > 0.6).
- `LevelInfo.Miniature` (0..1) adds Gaussian depth of field (start 42 m, end 150 m) in
  `LevelLook`. Only Level 1 sets it (0.7), and I like it: keep it.
- All IMGUI text goes through `UIKit.Sharp` (via `UIKit.Label`), which takes the UI scale off
  and draws the font at the real pixel size so letters stay crisp. Don't call `GUI.Label`
  directly (the one exception is the loading-screen title inside a `GUI.BeginGroup`).
- If the whole game looks soft in the editor, check the Game view's Scale slider is at 1x.
- In IMGUI code never use `GUIUtility.RotateAroundPivot` (it turns about a screen pixel and
  breaks under the UI scale); use `MapScreen.Turn` / `UIKit.IconTransformed` style matrices.
- Toybox music: `MusicToybox.ComposeToybox`. Sections come from `ToySection(bar)`; chords,
  tune and horn line are the string tables above it (one bar per entry, 4 beats each). Keep
  tune notes on the beat inside the bar's chord. `AddPad` is the cheap string pad; heavy use of
  `AddStrings` makes the first load of the level noticeably slower.
- `Emote.Show(pos, kind)` pops a felt emote anywhere. `Puffballs.Ensure(kit).Add(...)` adds a
  puffball; `ChimneySmoke.Add` is called by `TownKit` for every chimney.
- `MusicPlayer.Hush()` fades the music out and keeps it silent until the next loading screen
  has finished; the music is also always silent while `LoadingScreen.Busy`.
- `LevelInfo.SunColor`, `AmbientSky/Side/Ground` (alpha 0 = work it out from the sky) and
  `ShadowStrength` set a level's lighting mood. The Pod's extras (space glow, motes, mouse)
  are in `PodDreams`.
- Water form: `FormVisuals.BuildWater` / `AnimateWater` (pivots `wHipL/R`, `wArmL/R`, `wBody`,
  `wHead`, `wFish` under `bubble`, the whole-doll wobble pivot at the feet). Keep the top of
  its head under about 0.8 m: the collider is 0.8 m so it fits through grates.
- Wind form: `BuildWind` / `AnimateWind`. `windRing[0..5]` (tip to top, heights from `RingY`),
  `windArm`, `windHead`, all under `cloud` (the float pivot). `windStream` is 0 still .. 1 gliding.
- Hats: made for a head 0.6 m across with the hat's origin at the top of the head. Water and
  Wind place it with `PlaceHatOnFormHead` using `WaterHeadRadius` / `WindHeadRadius`; if a head
  changes size, change those constants. Things that stand up on a head go under
  `waterTopper` / `windTopper` so `SetHat` can hide them.
- Sounds from things out in the level that the player didn't cause go through
  `Sfx.PlayNear(id, pos, volume, range)` so they fade with distance (plain `Sfx.Play` is
  heard at full volume anywhere).
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

## The Storyteller and the Toybox Town opening
- `UI/Narrator.cs`: `Narrator.Say(line)` for a timed remark in play, `Narrator.Card(kicker, title)`
  and `Narrator.Place(name)` for the chapter card and place tags (pass null to hide), and
  `Narrator.Draw` which the HUD calls. Cutscene captions (`game.CutsceneCaption`) are drawn by
  the Storyteller too. `NarrationZone` is a volume that says its line once (hint-seen key).
- Remarks round town are in `Level1Crafts.StorytellerRemarks` (keys `L1_say_<place>`).
- `World/ToyboxIntro.cs` is one timeline; the `T...` constants at the top are when each shot
  starts. It takes the camera as soon as the level is in Gameplay mode (behind the loading
  screen) and holds the first shot until the fade has gone. It calls
  `PodLaunch.SkipToLanded` so Patch is already outside the Pod, and
  `RenderOptimizer.SetFarView(true)` and `LevelLook.SetMiniature(false)` (both undone in
  `Finish`) so small things aren't distance-culled and the wide shots stay sharp.
- The Storyteller is text plus a murmur sound. If a voice is ever added it has to be my own
  recording or one I have the rights to, never an imitation of a real narrator.
- Play starts without reloading scripts, so anything kept in a `static` that Unity made
  (meshes, materials, textures) must be checked with `== null` before reuse, not just "did I
  make it once". `RenderOptimizer.Prepare` does this now (it's what made the Pod vanish).

## Toybox Town's quests and wedding presents
- `World/Level1Quests.cs` places everything; `World/QuestBits.cs` has the moving parts
  (`PirateQuest`, `ScaredPiper`, `DyeTap` / `DyeWorksQuest`, `WeddingPresent`).
- A `BandPiece` can be `SetLocked(true)` until its story is done (`HideWhenLocked`,
  `LockedToast`). The pirate, the piper and the dye works each unlock theirs.
- Quest progress that isn't a pickup is saved with `GameState.SetHintSeen(key)`:
  `L1_pirate_met`, `L1_pirate_treasure`, `L1_pirate_paid`, `L1_dye_rose` / `_butter` / `_sky`.
- Presents are `Level1Director.GiftIds` (saved as collected). What each adds is built hidden in
  `Level1Builder.WeddingExtras` and kept in `director.GiftScenes`; `director.Explored()`
  (0..1 of all secrets) sets how many of `ExtraGuests` come. To add a present: add an id, a
  name and a hint in the director, a `Present(...)` in `WeddingPresents`, and its scene.
- `DigSpot` can dig up anything now: pass a null marble and set `OnDug`.
- `ToyNpc.Kind.Pirate` is a new toy body.

## Music layers must stay in step
- Never set a music layer's volume to 0: use `MusicPlayer.SilentVolume`. Unity stops really
  playing silent sounds and they come back slightly late.
- Start layers with `StartTogether(sources, sample)`; `KeepInStep` checks them once a second.
- `MusicPlayer.PodBands(float[])` gives the Pod song's loudness from low to high notes and
  `PodBeat` its beat count; `SoundRing` (in PodDreams.cs) draws the ring from them.

## The planet, the pictures and the Merit Quilt
- `World/GlobeDetail.cs`: the planet's map is `GlobeDetail.Paint(lon, lat)` (used by
  `PatchworkTextures.BuildGlobe`), wrapped on `GlobeDetail.Sphere()`, a sphere laid out by
  longitude and latitude the same way `LevelGlobe.Direction` places pins. `Height(dir)` says
  what's land; `Dress` stands trees, cottages and mountains on it and adds the clouds.
- `World/PodPictures.cs`: the four wall pictures, built flat inside their frames. Add a scene
  by adding a case to `Build`.
- `HubBuilder.MeritWall` draws every Merit's square; unearned ones are dashed rings.
- Hub furniture collisions: `TidyRoom` gives wide items a box per part (`BumpBox`).
- `ToyNpc.Focus`: set a point and every toy not playing turns to watch it; clear it after.
- The editor is set to recompile after you stop playing (`PatchworkStoryMenu` sets
  `ScriptCompilationDuringPlay`): recompiling mid-play leaves the code-built level broken.

## The Fray (FrayKin.cs)
- `FrayBodies` builds every creature's look (`Walker`, `Hopper`, `Flit`, `Gawp`), knot eyes and
  the Unpicker. The behaviour scripts (Snarling.cs, Monsters.cs) only move the parts they're
  handed back (legs, eyes, mouth, wings). Shared pieces: `EyeStalk`, `Leg`, `Fuzz`.
- `GreyStain` (one per level, added in `Level1Builder.Creatures`) leaves grey blots under
  everything in `FrayCreature.All`, and calls `LevelLook.SetDrain(amount, flash)` from how
  near the nearest creature or knot is. `Monsters.Popped` calls `GreyStain.Restore`.
- `UnpickerWatch` shows the Unpicker on top of whatever is highest at one of its `Spots`
  (x, z), 42 to 100 m from Patch; he folds away if Patch comes within 26 m. Hint-seen keys
  `L1_unpicker_seen` / `L1_unpicker_named` are the Storyteller's two remarks.
- The creature designs are original. The museum pieces were inspiration for the feel only:
  don't copy a particular sculpture.

## Invisible boxes and collecting
- `TownKit.Solid` boxes belong to whatever prop is around them. `Declutter` moves them with
  the prop, disables them with a removed tree, and switches off any prop-sized one (up to 6 m)
  with nothing visible where it is. A box that's meant to be invisible and alone must be
  bigger than 6 m or not be named "Solid".
- Score bubbles: wrap a line or ring in `ScoreBubbles.BeginSet(kit)` / `EndSet()` to make it
  a set (4+ bubbles pays a "perfect" bonus). `Level1Fun.Trail` and `Ring` already do.
- `ScoreKeeper.Sparks`, `Shout(words, colour)`, `ChainLeft`; the HUD draws them.
- `PatchworkHUD.ShowGet(title, line, colour, icon)` shows the big "got it" card (icon 0 a
  button, 1 a heart). `PrizeLeap` makes a picked-up thing leap over Patch's head.

## Level 2 (The Oceanic Museum)
- Files: `Level2Builder.cs` (plan, moods, Forecourt, Atrium), `Level2History.cs`,
  `Level2Aquarium.cs` (Kelp Galleries, Great Deep, tunnel, Bell), `Level2Ruins.cs` (Ruins,
  lift, Orrery Bridge, Observatory), `Level2Kit.cs` (all the building blocks),
  `MuseumLife.cs` (the moving things and the director), `MusicMuseum.cs`.
- The scene is `Scenes/Level2.unity` (same as Level 1's, with Level = 2). `LevelId.Level2`.
- Room sizes are the constants at the top of `Level2Builder` (AtriumHalf, HistX0, KelpZ0...).
- `Level2Mood` zones set the light, fog, backdrop colour and how "underwater" the music is
  for each hall. The first box that contains Patch wins. No sun shadows in this level
  (`ShadowStrength = 0`): it's lit by point lights and glowing materials.
- Tanks: `Tank(...)` makes the glass, frame, glowing back wall, surface and light shafts and
  returns a `TankSpace`; put things in it with `Kelp`, `Reef`, `TankSwim`, `TankShoal`,
  `Bubbles`. In tank space x runs along the glass, y up from the tank floor, z back.
  Swimmers are stitched by `StitchSwimmer` (Fish, Tall, Shark, Ray, Turtle, Whale, Angler).
- `Plaque(kit, foot, yaw, title, lines...)` is a readable plaque (an `ExhibitPlaque`, which
  is an `Npc` whose prompt says "Read"). Keep what's on them true.
- `Pool(...)` = water surface + `WaterBody`. `RoundPlatform(...)` = a round solid platform;
  add `Glider` + `Carrier` to make it move. `Stump(...)` = a broken column to stand on.
- The lift is the Observatory Express (`Level2Lift.cs` builds it, `CardboardLift.cs` runs
  it): a cardboard tower in the Ruins' south-west corner (x 44..52.4, z 6..14) with two
  identical cars, one at y 0 (doors east) and one at DeckY (doors west, onto the bridge).
  Riding is a scripted cutscene: doors shut, the camera sits in the car's corner, the lift
  music plays (`MusicPlayer.LiftMusic()`, `LiftDing()` in `MusicLift.cs`; `LiftHush` drops
  the other music), and halfway through Patch is teleported to the other car. The two
  cars must stay exactly alike or the swap shows. `LiftDoor` = a sliding door leaf,
  `LiftDial` = a floor dial's needle (reads `CardboardLift.Dial`). The bottom doors stay
  shut until 3 Heartstrings; `Level2Director` hides the "out of order" sign then.
- `Level2Director` sets the music's layers and runs the telescope ending.
- Music layers: `MusicPlayer.SetMuseumLayers(strings, winds, grand)` and
  `SetMuseumDepth(0..1)`.
- `Level2Details.cs` is the second pass (decor only, plus the atrium's `StarCeiling`);
  `Level2Arms.cs` is the Hall of Arms (ArmsX0.. constants). `Armour(kit, foot, yaw,
  Warrior.X)` makes one suit; its helmet has a `WatchPlayer`.
- Little effects in `MuseumLife.cs`: `StarTwinkle`, `ConstellationShow`, `ShootingStar`,
  `DriftMotes`, `StarMobile`, `SeaBob`, `WatchPlayer`. Helpers: `Twinkling`, `Motes`,
  `Shooting`, `Painting`, `Palm`, `Crate`, `Vine`, `Boat`.
- The museum's piano and violins are `AddPiano` / `AddViolins` in `MusicMuseum.cs`.
- `Level2Life.cs` is the third pass: people, animals and the big set pieces (Carousel,
  chandelier, Keepers, frieze, café, door signs). Components for it are in `MuseumLife.cs`:
  `Leaper` (seals and jumping fish), `CarouselBob`, `WingFlap`, `SleepyNpc` (an Npc whose
  prompt says "Nudge"). Walking figures use `PathMover`; they can't be talked to.
- `Level2Grandeur.cs` is the fourth pass (sky lanterns over the sea, the atrium's river of
  stars, kelp moonbeams, the whale calf, fireflies, the Hall of Arms' pennants).
  `SkyLantern` lives there too.
- Checkpoints (all levels) are sewing stations: `LevelKit.AddCheckpoint` builds a tomato
  pincushion with pins, a cotton reel and a needle beside the spawn spot, parented under the
  Checkpoint so it stays live. `Checkpoint` animates it: asleep (needle lying across the reel,
  grey thread) or stitched (needle up and swaying, gold thread, reel winding, lamp on, a
  bounce and sparkles). The map's checkpoint icon is a reel with a needle.
- `Level2Flourish.cs`: paper boats on the sea, the museum map board and leaflet rack by the
  front doors, dinosaur footprints in Deep Time, fishing floats in the Kelp Galleries.
- The lift's two cars aren't stacked: the bottom one is in the Ruins tower (`LiftLower`),
  the top one is at the far end of the Star Corridor (`LiftUpper` = (52, DeckY, -38)).
  Both are built by `CarShell` + `CarInside` and must stay identical. The Ruins' old upper
  doorway onto the bridge is walled up.
- `Level2Corridor.cs`: the Star Corridor (x 32..55, z -42..4, floor DeckY, 6.5 high),
  five rooms: Hall of Clocks, Night Sky Gallery, Planet Walk, Moon Steps, Comet Gallery.
  It comes out through a new arch in the Orrery Bridge's south wall (x 34..38) onto the
  bridge's east landing. Its own mood zone and map area.
- See-through draw order is fixed, not left to distance sorting (that swaps big
  overlapping see-through things as you move: flicker). `Renderer.sortingOrder` (URP
  ignores `rendererPriority` for see-through things): pool haze -15, glass -20 (domes:
  inside shell -21, outside -20; `ZFightFix.OrderSeeThrough` sets all "PW_glass"
  materials), everything else 0, pool surfaces 5, light beams 20.
- A light that casts shadows never moves (`CausticLight` only sways the shadowless ones).
- Tanks are full to the roof: looking up through the glass you see the dark ceiling, its
  beams and lamps.
- Light beams are `LightBeam`s (CinematicShot.cs): a soft-edged additive quad that turns
  round its own axis to face the camera (`LightBeam.Make(parent, top, bottom, width,
  colour, strength)`); placed in `Level2Light.Beams`. Never use flat `Shaft` sheets for
  light again.
- Tanks: the water stops below the top of the glass (`waterline`), with a dark band of
  "air" walls, lamps and the roof above, so the top reads as a closed tank. The Great Deep
  has the same.
- Camera near clip is 0.25 m (it was 0.1, which made distant flat things flicker).
- `Plinth()` caps stand 3 cm proud; `ZFightFix` also handles blocks turned to any angle.
- The lion spouts pour from open mouths in an arc of round stream segments; the springs
  rise from a stone fish's mouth (`SpringMouth`).
- No see-through "light shaft" sheets any more (they read as flat grey panels and flickered
  as they sorted against each other): light through windows is real spot lights only.
  `Shaft()` is still used for the waterfall ribbon, soft aurora blobs and clouds.
- `Tank()` puts a dark stone lid over every tank, so you can't see over the top.
- `ZFightFix.Run(root)` (end of the Level 2 build) nudges apart blocks/discs whose faces sit
  in the same place. Fountains: the stone basin's water sits above its band, and the
  column/jet/bubbles have fixed `rendererPriority` so the see-through parts don't swap.
- `CausticLight` now keeps one pattern and turns/sways it smoothly (swapping patterns made
  lit things flicker). Shadowed spots use a large shadow bias.
- `Level2Light.cs`: `Lighting` (spot lights: caustic cookies via `CausticLight`, a few with
  soft shadows: the Great Window, the Observatory moon, the kelp pool) and `Cinematics`
  (one `CinematicShot` per hall). Spot lights fall off with distance squared, so a spot
  20-40 m from what it lights needs an intensity of roughly 30-110.
- `CinematicShot` (CinematicShot.cs): a PlayerVolume that plays a camera move once (save key
  `L2_cine_<name>`), waits for gameplay / no loading / no lift ride, hold Confirm to skip.
  `Tick(k)` and `Done` hooks; a zero-size one is started by `Trigger()` (the "lift wakes"
  shot, run by `Level2Director.WakeTheLift`, which drops the sign and opens the doors via
  `CardboardLift.ShowOff`).
- Diving: `WaterBody.Divable` (all Level 2 pools; Level 1's water is unchanged). Water Form
  holds Ability to dive, floats up when let go, Jump kicks up. `PlayerMotor.Submerged` /
  `WasSubmerged`; while submerged the camera may go under the surface, and `Level2Mood`
  turns the fog and ambient blue-green and calls `LevelLook.SetUnderwater`.
- God Mode Halo (hat 9, `FormVisuals.GodHat`): a testing hat, unlocked only in the editor
  or a development build. `PlayerMotor.GodMode` is on while it's worn: 3.2x speed, 1.8x
  jump height, jump again in mid-air, hold Jump in the air to fly up, a gentler fall, and
  any form can swim (WaterBody). Pick it in the Satchel's hat row.
- Editor menu: Patchwork > Replay Level 2 Moments (keep what you found) forgets the
  cutscenes, cards, Storyteller lines and map for Level 2; Patchwork > Reset Level 2
  (first visit) also puts back everything found there. Both use `GameState.ForgetLevel`
  with the prefixes `L2_`, `Level2_` and `map_cp_Level2_`, so new Level 2 save keys
  should start with one of those.
- `Level2Undersea.cs`: the kelp pool floor (Sea Gate, anemones, boat, giant clam with
  marble `L2_marble_clam`), the Drowned Colonnade along the Ruins' east wall (x 93.5..100,
  z 72..100, roofed at y -1.5) with a `TreasureChest` that opens for a swimmer and shows
  marble `L2_marble_treasure`, and the Ruins' pool floor (sun mosaic, sphinx, coins).
- `RenderOptimizer.WorldTiling` (on for Level 2 only, set in PatchworkGame before Run): cubes
  wearing Brick/Cobble/Plaster/Planks/Shingle/Damask get their own mesh with world-space UVs
  (tops map x,z; sides map along-wall and y), one texture repeat per 3 m (brick, cobble),
  4 m (plaster), 1.6 m (planks), 2-2.4 m (others). Done before StaticCombine, so it merges.
- Facade detail lives in `Level2Flourish.FacadeDetail` (outer face z -110, towers x 51..61).
- Star Corridor walls are non-overlapping segments (each corner owned by one wall); the
  dado and gilt are added per wall face by the `W` helper, never per room.
- ScoreBubbles.Settle treats any renderer under 9 m as a prop by its bounding box, so one
  MeshBatch spread across a passage blocks the whole passage: split batches per wall.
- Walk-through props from the audit are fixed with `TownKit.Solid` boxes beside the decor.
- `Painting()` now adds its own picture light, liner and title plate (all paintings get them).
- `Level2Flourish.HistoryOutside` dresses the Hall of Deep Time's outer south face (z -106).
- `Level2Engravings.cs`: `Letters(mb, text, centre, facing, px, depth)` draws text in a 5x7
  dot font (capitals only; add glyphs to `Font`). Also the tower tablets/roundels, garlands,
  door fanlight, atrium compass rose and skirting boards. Called from Flourish.
- WorldTiling divides by the material's baked `_BaseMap` scale (LevelKit.Block bakes a repeat
  count into a material copy), so the world repeat is the only one.

## The map (MapScreen.cs)
- Painted once from an overhead camera. Outdoor levels get the felt look (`Stylize`);
  levels with `MapCameraHeight` under 20 (indoors) get `Lamplit`, which levels out the dark
  picture and keeps the real colours.
- Exploring: a 96x96 grid over the map, uncovered within ~16 m of Patch (checked every
  0.3 s in `Tick`), saved in PlayerPrefs as `pw_map_<level id>` and wiped by
  `GameState.DeleteSave`. Region names and place numbers only show once explored.
- Checkpoints you've stood at are remembered as hint ids `map_cp_<level>_<x>_<z>`; you can
  fast-travel to them (and to the Pod) from the map.
- Controls: stick / WASD / mouse moves the cursor, L1 / R1 (Z / X, mouse wheel) zooms,
  Triangle / F finds Patch, Confirm (or a click) travels.

## Habits
- Test every change with Play and check the Console.
- Keep it to C# 9 and older Unity APIs so it compiles everywhere.
- Never regenerate .meta files - both scenes point at the `LevelBootstrap.cs.meta` GUID.
- Commit each milestone, only push to `main` once it plays.
- Keep `PROJECT_STATUS.md` and `DEVELOPMENT_LOG.md` up to date.

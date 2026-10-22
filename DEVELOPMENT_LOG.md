# Development Log — Patchwork

## 2026-10-22 — Cleanup
- Removed unused code: old flat texture makers (fabric, felt, yarn, wind), the unused Wind material, the triangle glyph and soft-circle icon, `TransformBurst` (callers now use `FxBurst` directly), and small unused helpers (`ResetAllInsideFlags`, `AirTime`, `ConsumeCoyote`, `ResetProgress`).
- The Form Wheel button can now be switched between L1 and R1 from the Pause menu (saved); button prompts follow the choice.

## 2026-10-22 — Prototype 3 verified
- Plays well. Merged `dev` into `main` and saved the **`prototype-3`** branch as a restore point.
- Renamed the notes file to `DEV_NOTES.md`.

## 2026-10-22 — Prototype 3 changes
- **Auto-hiding pop-ups:** `PatchworkHUD` slides the form panel out after 4.5 s (kept up while swinging, gliding, gust recharging, transform pending). Tutorial cards (`HintZone`) now last 7 s, slide in/out, and slide away early when you leave.
- **Themed cards:** `HintTheme` (Neutral/Water/Yarn/Wind) picks colours, a tiled pattern (`UIArt`: waves / knit / swirls / dots), animated decorations (rising bubbles, swinging yarn strand, drifting clouds) and a sound (`HintWater` bloops, `HintYarn` Karplus-Strong yarn pluck, `HintWind` breeze + chimes).
- **Pause menu:** drop-in with overshoot, staggered option slide-in, animated highlight with marching stitches, generated icons with per-option animations (pulse, spin, hop, wiggle, dance, sway).
- **Textures:** `PatchworkTextures` builds albedo + normal maps from height functions; materials gain `_BumpMap`. New material helpers: `Knit`, `Wicker`, `Grass`, `Wood`.
- **Level 1:** rebuilt as the Sewing Basket Garden with a Button Bridge section (two `MovingPlatform`s; player carried via `PlayerMotor.GroundCollider`), `BouncePad` pin-cushions (`PlayerMotor.Bounce`), and `ThreadSpool` collectibles (per-visit count). Core Water/Yarn/Wind challenge distances unchanged.

## 2026-10-22 — Prototype 2 verified
- Confirmed the Water eyes fix in Unity; everything compiles and plays.
- Merged `dev` into `main` and saved the **`prototype-2`** branch as a frozen restore point.

## 2026-10-22 — Water eyes fix
- Bug: the water bubble's blink/jiggle code reset the eye pivot's height to ~0 every frame, so the eyes sank to the bottom of the bubble.
  Now the jiggle offsets from the eyes' rest position.
- Eyes rebuilt as cartoon eyes (white eyeball, dark pupil, sparkle) placed on the bubble's outer surface and angled to follow its curve, so they look crisp instead of cloudy through the see-through shell. Smile moved onto the surface too.

## 2026-10-22 — Prototype 2 changes (verified: compiles and runs)
Feedback after Prototype 1, all addressed on `dev`:
- **PS5 prompts missing:** device detection treated tiny mouse movements (e.g. the cursor unlocking) as keyboard use. Now only deliberate input switches, and the Pause menu can force PlayStation prompts.
- **Screechy audio:** rewrote `Sfx` with sines and low-passed noise only; new gust whoosh, bubbling loop for rising in fountains, airy glide loop (`Sfx.RequestLoop`).
- **Music:** `MusicPlayer` synthesises a 72 BPM lo-fi loop (e-piano chords, bass, music box, brushes) once and keeps it across scenes.
- **Characters:** `FormVisuals` rebuilt. Yarn toy with leg/arm walk cycle and a yarn arm that points at the anchor (tether starts at the hand). Water is a transparent jiggling bubble (`PatchworkAssets.Bubble`). Wind is a floating smiling cloud.
- **Transitions:** `FormVisuals.BeginTransition` plays a 0.6 s two-phase animation per element pair with `FxBurst` particles (drips, steam, rain, threads, splash, puff). Gameplay switches instantly; only visuals animate.
- **Menus:** dark pill backings and outlined text wherever white text sat on the scene.
- **Enclosure:** `LevelKit.Enclosure` walls in hub and level (quilt bands, giant buttons, bunting, lamps; no shadows so sunlight still reaches in).
- **Portal:** embroidery-hoop portal with four counter-rotating rings, spiralling sparkles and a pulsing light; `PatchworkGame.EnterPortal` pulls Patch in, spins and shrinks him, then fades to white.

## 2026-10-22 — Prototype 1 verified
- Opened the project in Unity, it compiled, and the game played.
- Pulled back the files Unity generated on first open (`Resources/PW_Lit.mat`, URP project settings).
- Merged `dev` into `main` and saved the **`prototype-1`** branch as a frozen restore point.

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

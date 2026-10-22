# Project Status — Patchwork

_Last updated: 2026-10-22_

## Current milestone
**M1: Form system + all three forms + Hub + Level 1 (code complete, awaiting first Play test)**

Written but **not yet verified in Unity**. This milestone becomes
"verified" only after it compiles with no Console errors and has been played on the PC.

## Done (pending verification)
- [x] Unity 6 URP project baseline in Git (`main`)
- [x] Controller-first input (PS layout) + keyboard fallback, no conflicting actions
- [x] Shared movement motor (coyote time, jump buffer, variable jump), orbit camera
- [x] Form system: definitions, abilities, safe/deferred transforms, room check, availability (unlock) flags
- [x] Radial Form Wheel (L1/Q), slows time, confirm/cancel, PS button prompts
- [x] Water Form: smaller body, passes drain grates, rides fountains
- [x] Yarn Form: anchor highlighting, hold-to-swing pendulum tether, release/jump launch
- [x] Wind Form: glide (hold Jump), one gust per jump, rides updrafts
- [x] Transformation feedback: fabric-scrap burst, squash/stretch, chime
- [x] HUD: current form + ability hints, golden buttons, controls strip, tutorial cards, toasts
- [x] Satchel (character-held HUD): forms, finds, hat cosmetic (independent from form)
- [x] Pause menu, checkpoints, fall respawn, level complete screen
- [x] Hub with practice spots for each form + doorway to Level 1
- [x] Level 1 with Water / Yarn / Wind challenges and 3 optional golden buttons
- [x] Docs: README, DEV_NOTES.md, PROJECT_STATUS, DEVELOPMENT_LOG

## Next
1. First Play test on the PC → fix any Console errors → push a verified checkpoint
2. Tune feel (swing strength, glide distance, camera) from play feedback
3. Polish: nicer form models/animations, real sound effects, UI Toolkit menus
4. Rebinding UI for the Form Wheel button (code hook exists: `PatchworkInput.SetFormWheelBinding`)

## Known limits / risks
- Not compiled yet (written without access to Unity). Expect a first round of small fixes.
- Everything is built from code, so level edits happen in `Level1Builder.cs`/`HubBuilder.cs`, not the Scene view.
- Windows builds are untested; play in the Editor for now.

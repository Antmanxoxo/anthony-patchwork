# Project Status — Patchwork

_Last updated: 2026-10-22_

## Current milestone
**Prototype 2 — verified ✅** (saved as the `prototype-2` branch on GitHub; `prototype-1` is the earlier restore point)

Compiled and played on the PC on 2026-10-22, including the Water eyes fix.

## Done
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

## Prototype 2 additions
- [x] PS5 prompts fixed (mouse drift no longer flips to keyboard) + Pause option: Button Prompts Auto / PlayStation / Keyboard
- [x] Softer sounds: gust whoosh, bubbling loop while Water rises, airy glide loop, gentler chimes
- [x] Chill background music (procedural lo-fi loop) + Pause option: Music On/Off
- [x] New characters: knitted Yarn toy (walks, flails when jumping, yarn arm reaches anchors), jiggly see-through Water bubble with blinking eyes, happy floating Wind cloud
- [x] Element transitions: yarn soaks → water, water steams → wind, wind rains → water, water wrung → yarn, yarn/wind unravel/spin
- [x] Readable menus (dark backings + outlined white text)
- [x] Hub and Level 1 enclosed in quilted rooms (walls, ceiling, bunting, lamps)
- [x] New embroidery-hoop portal with spinning rings, sparkles, light, and a warp-in animation
- [x] Water bubble eyes fixed (sit on the surface, stay at face height while jiggling)

## Next
1. Owner's next round of changes
2. Tune feel (swing strength, glide distance, camera) from play feedback
3. Polish: nicer form models/animations, real sound effects, UI Toolkit menus
4. Rebinding UI for the Form Wheel button (code hook exists: `PatchworkInput.SetFormWheelBinding`)

## Known limits / risks
- Everything is built from code, so level edits happen in `Level1Builder.cs`/`HubBuilder.cs`, not the Scene view.
- Windows builds are untested; play in the Editor for now.

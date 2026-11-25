# Project Status

_Last updated: November 25, 2026_

## Overview

| Area | State |
|---|---|
| The Pod (hub) | Playable |
| Level 1: Toybox Town | Playable from start to finish, fully narrated |
| Level 2: The Oceanic Museum | Playable, with narration in progress |
| Windows build | Working (one-click build from the editor) |

`main` holds the latest stable work. `dev` is where new work lands first.
`first-build` and `prototype-1` to `prototype-3` are kept as snapshots of earlier milestones.

## Finished

**Core play**
- Controller-first controls (PlayStation layout) with full keyboard and mouse support, and button prompts that follow the device in use
- Movement with coyote time, jump buffering, running and sprinting, plus an orbit camera
- Three forms: Water (squeezes through grates, rides fountains, swims), Yarn (swings from anchors and lanterns) and Wind (glides and gusts)
- Form Wheel that slows time, with change animations between forms
- Combat: stomps, dashes and three-hit strike combos

**Story and presentation**
- Narrated storybook cutscenes with voice-over, a hand-held camera, soft depth of field and stitched iris wipes
- Subtitles on a paper strip, with a volume setting for the narrator
- Original music for each area, which grows as you put things right, plus area sound beds

**The Pod**
- Level select by turning the patchwork planet
- The Store, the Wardrobe (dyes, hats and outfits) and the Trophy Room with its Merit cabinet

**Toybox Town**
- The wedding story, the band pieces, threads and Heartstrings
- Side quests, secrets, a bubble rush and lots of interactive toys
- Spindle Hill: a tall knitted hill with its own autumn wood and a climbing route

**The Oceanic Museum**
- The Night Glass story, themed halls from the harbour to the deep sea
- The Curator's Audit, the Museum Passport and hands-on exhibits

**Systems**
- HUD, map, quest menu, Satchel, Photo Mode and stickers
- Saves that upgrade themselves from older versions
- App icon and one-click Windows build

## In progress
- Voicing the rest of Level 2's narration
- Playtesting and tuning the new Spindle Hill climb

## Next
- More levels on the planet
- Models for the Water and Wind forms
- Further tuning of how the swing, glide and camera feel

## Notes for development
- Levels are built in code. Change them in the level builder scripts (for example
  `Level1Builder.cs`, `Level1SpindleHill.cs`, `Level2Builder.cs` and `HubBuilder.cs`), not in the Scene view.
- Pressing Play in the editor writes a level check to `Logs/PatchworkAudit_<level>.txt`.
- Narration clips live in `Assets/Patchwork/Resources/Voice`. Any line without a clip shows as text only.

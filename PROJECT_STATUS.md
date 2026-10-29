# Project Status

## Where it's at
**First build.** The Cardboard Skiff hub and Toybox Town are playable from start to finish.
`prototype-1` to `prototype-3` are older backups.

## Done
- Controller-first controls (PlayStation layout) with keyboard backup
- Movement with coyote time and jump buffering, orbit camera
- Three forms: Water (squeezes through grates, rides fountains, swims), Yarn (swings from
  anchors), Wind (glides and gusts)
- Form Wheel that slows time, change animations between forms
- HUD, themed tutorial cards, Satchel with hats, animated pause menu, pull-up map
- Sea shanty music for the hub and a layered march for Toybox Town that grows as the band
  comes back
- The Cardboard Skiff hub with a level-select globe
- Toybox Town: the wedding story, an opening cutscene, deep water, threads, Heartstrings and
  lots of toys to play with (xylophone bridge, carousel, domino run, bubble wand, wind chimes)
- Saves that upgrade themselves from older versions
- App icon and one-click Windows build

## Next
- More levels on the globe
- Models for the Water and Wind forms
- Play more and tune how the swing, glide and camera feel

## Good to know
- Levels are built in code, so changes go in `Level1Builder.cs` / `Level1Details.cs` /
  `HubBuilder.cs`, not the Scene view.
- Pressing Play in the editor writes a level check to `Logs/PatchworkAudit_<level>.txt`.

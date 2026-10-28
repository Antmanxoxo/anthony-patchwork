# Project Status

## Where it's at
**Prototype 3 works.** Saved as the `prototype-3` branch (`prototype-1` and `prototype-2` are older backups).

## Done
- Controller-first controls (PlayStation layout) with keyboard backup
- Movement with coyote time and jump buffering, orbit camera
- Three forms: Water (squeezes through grates, rides fountains), Yarn (swings from anchors), Wind (glides and gusts)
- Form Wheel that slows time, change animations between forms
- HUD, themed tutorial cards, Satchel with hats, animated pause menu
- Music and sound effects
- Hub (The Sewing Room) and Level 1 (The Sewing Basket Garden) with golden buttons and thread spools
- Bounce pads and moving platforms
- Patch's Yarn form is now a real rigged, animated model made in Blender, with swappable faces

## Next
- Play more and tune how the swing, glide and camera feel
- Models for the Water and Wind forms
- Maybe move menus to UI Toolkit

## Known issues
- Levels are built in code, so changes go in `Level1Builder.cs` / `HubBuilder.cs`, not the Scene view.

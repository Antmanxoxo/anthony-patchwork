# Development Log

## Polish pass 2
- Wind's gust no longer re-launches the glide, so Wind can't skip the Yarn swing any more.
  Made the Wind gap 7m so gliding alone gets you over.
- Shiny things (thimbles, needles, the bubble) now reflect the room instead of looking grey.
- Sounds you hear a lot (jump, land, bounce) change pitch slightly each time, and picking up
  spools in a row climbs a little scale.
- Music goes quiet and muffled when paused, and muffled in the Form Wheel.
- Pause menu has Music Volume, Sound Volume and Rumble settings (left / right to change).
- Controller rumble on jump, land, bounce, gust, catching an anchor and while swinging.
- The HUD no longer makes garbage every frame, and pressing Play twice in a row starts clean.

## Polish pass 1
- Set the company name to Little Colossus and gave the game a proper ID.
- Turned on VSync, with a 120 fps cap as a backup.
- Saved the shader-keeper materials into the project so builds don't lose the bumpy, glow
  and see-through looks.
- Added a soft shadow disc straight under Patch in every form, so it's easier to tell where
  you'll land. It shrinks and fades the higher you go.
- The yarn rope now stays attached to Patch's hand instead of lagging a frame behind.
- Falling into a pit respawns you almost straight away with a quick fade, instead of waiting
  (gliding Wind used to float down for about 6 seconds).
- Compressed Patch's textures (about 43 MB down to about 3 MB) with no visible difference.
- Fixed the hub sign's level name, and Level 1's tutorial cards now only show the first time.

## Patch model
- Made a proper 3D model of Patch in Blender for the Yarn form: yarn-ball head with wound yarn,
  mismatched button eyes, cardboard eyebrows, a paper mouth, felt cheeks, and a stitched knit suit
  with a bowtie, pockets, coat tails and a worn knee patch. His right arm unravels into a yarn rope.
- Rigged him (14 bones) and animated Idle, Walk, Run, Jump, Fall, Land, Swing, Wave and Celebrate.
- Three faces you can swap with the D-pad (or 1 / 2 / 3): happy, sad and angry.
- Baked his materials into one colour texture and one normal map so Unity can use them.
- In game his animation follows what he's doing, and his yarn arm points at the anchor while swinging.

## Build prep
- Added small materials in Resources so the build keeps the shader versions for the bumpy
  textures, glow and the see-through bubble (Unity strips unused ones from builds).
- Gave those materials a real normal texture and the emissive flag, since URP was quietly
  turning their bumpy and glow settings off on import.
- Fixed "Practise" -> "Practice" in the hub title.

## Cleanup
- Took out code that wasn't being used anymore (old flat textures, an unused material, a couple of
  icons, and some small helper functions).
- You can now put the Form Wheel on L1 or R1 from the pause menu. The button prompts update too.
- Rewrote my code comments so they're shorter and easier to read.

## Prototype 3
- The form panel and tutorial cards now slide away on their own after a few seconds.
- Tutorial cards have Water / Yarn / Wind themes, each with its own colours, little animations and sound.
- Pause menu got a proper open animation and every option has an animated icon.
- Added bumpy textures (normal maps) for knitting, felt, cloth, wicker, grass and wood.
- Rebuilt Level 1 as the Sewing Basket Garden: wicker walls, felt sky and sun, yarn balls,
  knitting needles, thimbles and mushrooms.
- New stuff to play with: bouncy pin-cushions, sliding button platforms, and thread spools to collect.

## Prototype 2
- Fixed the PS5 button prompts (mouse wiggles were switching it to keyboard). Added a pause option to force them.
- Made the sounds softer, added a bubbling sound for fountains and a whoosh for the gust.
- Added background music.
- New characters: a little knitted Yarn toy, a jiggly Water bubble with eyes, and a happy Wind cloud.
- Added change animations between forms (yarn soaks into water, water steams into wind, and so on).
- Made menu text easier to read, closed in the maps with walls and a ceiling, and made a new portal.
- Fixed the Water bubble's eyes sinking to the bottom.

## Prototype 1
- Set up the Unity 6 URP project and Git.
- Built the form system with Water, Yarn and Wind, the Form Wheel, HUD, Satchel and pause menu.
- Made the Hub and Level 1.

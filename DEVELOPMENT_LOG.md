# Development Log

## Ready for a first build
- Cleaned out code the game no longer uses: the Test Gym, portals, moving platforms, the old
  Sewing Basket props, a few unused instruments, and Unity's sample scene and readme.
- Builds keep every material look the game makes (no pink objects), anti-aliasing is on, the
  window opens at 1920x1080 and can be resized, and the game has its own icon.
- Pressing Play in the editor now writes a level check to the Logs folder, listing props you
  can walk through, flickering faces and anything stuck inside a wall.
- README and project status updated for the Cardboard Skiff and Toybox Town.
- Fixes from the level check: solid crayons, cannons, belfry roof and the Skiff's water barrel;
  threads moved out of the bandstand roof; flickering bench legs, line poles and spire cap.

## Toys to play with, solid props and fixes
- Fixes:
  - The Mayor said the same thing whether or not you had the band pieces, and the opening
    cutscene never played. An old save from before the town rebuild was marking Toybox Town
    as finished. Saves now upgrade themselves and drop that, and Reset Save Data clears
    everything.
  - Talking was fiddly. Toys are easier to talk to from further away, and when two are close
    you talk to the nearest one.
  - You could walk through benches, the train, wagons, soldiers, flower pots, the signpost,
    chairs, the picnic, the scarecrow, vats and posts. They're solid now, and the moving toys
    push you away.
  - The camera dipped under the water while swimming. It now stays above the surface.
  - The intro cutscene can be skipped by holding Confirm.
- New things to play with:
  - A domino run up the east side of town. Knock over the first domino and they all fall,
    ring a bell and drop threads.
  - A giant bubble wand that keeps blowing bubbles. Bounce from bubble to bubble up to a sponge
    with a new Heartstring on it ("Bubble Bath").
  - Wind chimes on the bandstand, the rooftops and the spire that play notes as you brush
    past.
  - Rubber ducks squeak and squish when you touch them.
  - The clock tower bell swings and rings when you reach the belfry, and the clock hands turn.
- More detail:
  - Houses have lace curtains, gutters, drainpipes and sometimes ivy.
  - Smooth painted toys have a soft brushstroke texture.

## Deep water and ink holes
- The canal and the ravine stream are now deep water (5 m and 3 m).
  - The surface is crafted: glossy, see-through blue satin with stitched white wave lines,
    sequin glints and lace foam along the edges, over a dark deep bed.
  - Water Form floats at the surface, swims nearly twice as fast and can jump back out.
  - Every other form splashes and goes back to the last checkpoint.
  - Threads float along the canal for swimmers.
- The home hoops are now ink holes: glossy pools of ink with a turning violet swirl, bobbing
  droplets, a tipped-over inkwell and a quill. They still ask before sending you home.
- Fixes:
  - The map's painted view came out blank. It's now taken by a camera while the level loads.
  - The Dye Works drain was covered by the stone footing.
  - A copper rail floated in mid-air inside the Dye Works.
  - Cattail heads floated above the canal.
  - The box walls left a gap at the end of the ravine.
  - Street lamps grew through the square's benches.
  - The cloud patch was a flat slab with a lumpy bun under it.
  - The toy train's smoke looked like a mushroom.
  - The park pond can't be walked on any more.

## Stylised Toybox Town, a map and new music
- New look over everything: soft glow on bright and glowing things, richer colour, warm
  highlights with cool shadows, and a gentle vignette. Each level can tune it.
- Textures have much more detail:
  - Crevices take on their own colour (moss in the cobble and brick joints, shade in the
    grass) and raised edges catch the light.
  - Big soft colour patches break up the tiling, and the grass texture is twice the
    resolution, with sunny tips.
- The Fray now looks like the world it's in. In Toybox Town it's made of grey, broken toys:
  - Its knots are stuffed with gears, springs, wind-up keys and button eyes.
  - Snarlings are grey wind-up tin beetles with keys turning in their backs.
  - Tendrils are sprung jack-in-the-boxes with drooping jester heads.
  - Tears are holes ripped in grey wrapping paper.
- The nursery outside the box is a whole room now: walls with wainscoting and pictures, a
  ceiling with a big glowing lamp, and a rug with a border. A long soft haze makes it feel huge.
- A map you can pull up with D-pad left (M on keyboard). It's a hand-painted tourist map of
  the town on squared paper, with red pins and place names, stars for the band's missing
  things, an arrow showing where you are, a little sun and some trees, and a legend of what's
  left to find.
- The Skiff has a new sea shanty: 6/8 in D minor, with accordion, plucked bass, bodhran and
  the ship's bell. The lead changes with your form: fiddle for Yarn, a rolling marimba with
  bubbles for Water, and a tin whistle over a breeze for Wind.
- Toybox Town has a new soundtrack: a bouncy, swung toy-town march in four sections.
  - It goes verse, verse with a wistful turn, a minor middle with a soft chiptune lead, and
    a soaring finish with strings.
  - On its own it's walking bass, brushed drums, toy piano and the tune on music box,
    chiptune and glockenspiel.
  - Each band member you bring back adds a layer: harp and glissandos, brass, or the flute.
- New toys to play with:
  - A xylophone bridge over the canal that plays a scale as you run across it. Play the
    whole scale in one go and threads pop out.
  - Toy drums to bounce on: onto the bandstand roof, up the alphabet blocks and up to the
    rooftops.
  - Wind-up mice scurrying about, tin soldiers drilling in the barracks, and a carousel in
    the park.

## A lived-in Toybox Town
- The first time you visit Toybox Town, a cutscene shows the Fray arriving. The band is
  playing when a crack opens in the toybox lid and the Fray swoops out onto the bandstand. It
  snaps the lute string (which flies off to Spindle Hill), seizes up the trumpet, and blows
  Wisp onto the weathervane, and the music stops. Then it knots itself round the clock tower
  and grey threads spread over town. Confirm skips it, and it only plays once.
- Home hoops now ask "Head back to the town square?" before sending you anywhere (No is picked
  to start with), so you can't fall through one by accident.
- Fixed: the Dye Works floors were solid blocks right behind the drain, so you came through
  into a wall. The drain now opens into a clear hall and the pipes climb up behind it.
- Fixed: doors flickered because each door's front sat at exactly the same depth as its frame.
  Doors are now layered (frame, door, then hinges and knocker), and no timber posts run
  through doorways.
- The Skiff's fountain is now a ship's water barrel with brass hoops instead of a stone basin.
- Yarn grass grows over all the open ground in clumps, with clover, pebbles and pompom
  flowers. Felt and fabric patches are sewn onto the grass with running stitches. All of it
  is batched into a few meshes, so it's cheap to draw.
- Outside the toybox is the nursery: a rug, the box's painted outside, a window full of sky
  with curtains, a bed, a chest of drawers and a lamp. A patchwork balloon drifts round the
  town and a cot mobile turns overhead.
- Giant toys round the edges: alphabet blocks to climb (threads on top), crayons leaning on the
  walls, a rocking horse, a wobbling spinning top, marbles, jacks, and yarn balls with knitting
  needles.
- More of the town:
  - In the square: the wedding cake, white chairs with bows, two market stalls, a pillar box,
    a signpost to each part of town, flower pots by every door, barrels, puddles and a
    penny-farthing.
  - Around town: string lights across the streets, hedges and canal railings, lollipop trees,
    and rubber ducks paddling up the canal past lily pads and reeds.
  - In the ravine: rocks and stepping stones, plus a culvert where the stream comes in.
  - On Spindle Hill: a fenced vegetable patch with a scarecrow, and stepping-stone paths.
  - In the north park: a picnic, a pond with ducklings and a kite.
  - At the Dye Works: drying lines of dyed cloth and vats of dye.
- A toy train runs round a track with a station, a cardboard tunnel and a chatty
  stationmaster.
- Butterflies flutter round the flower beds.

## Toybox Town
- Level 1 is now Toybox Town: a little Victorian town built inside a giant open toybox, with
  cobbled streets, timber-framed shops with fish-scale roofs, gas lamps, a bandstand, a wedding
  arch and a clock tower. Everything stands on real ground: a ravine with a stream to the west, a
  canal with bridges and toy sailboats to the east, and the toybox's papered walls all round.
- The town's people are toys: Mayor Bramblebutton (a teddy in a top hat) is marrying Lady
  Lacewing (a rag doll) today, but the Fray has spoiled the town band. Talk to them all.
  - Pip Plunkett's lute lost its string: swing over the ravine on the clothesline and up Spindle
    Hill to the old spinning wheel (Yarn).
  - Corporal Brassby's trumpet valves are stuck: slip into the Dye Works through its drain and
    ride the pipes up to the reservoir for some valve dew (Water).
  - Wisp the piper was blown onto the weathervane: ride steam and chimney smoke over the rooftops
    (Wind).
  - With the whole band home, talk to the Mayor: the wedding plays out, the music unravels the
    Fray's knot on the clock tower, and the town is mended.
- Level 1 has its own music: a 3/4 waltz on harpsichord with tabor and tambourine. The lute,
  trumpet and pipe join in as you bring each one back, and the musicians play along on stage.
- 5 bonus Heartstrings (pop six knots round town, race along the canal, clear the barracks of
  Snarlings, the clock tower balcony, a cloud patch above the spire) and the 3 golden buttons.
- Fountains look much better: glassy water column, rising bubbles, a foamy crown and droplets.
  They come in stone, pipe and teapot styles. Updrafts can be vents, chimneys or steam grates.
- Yarn can swing from anything wrapped in pink yarn: buttons, clothesline pegs, lanterns, spools.
- New textures (cobbles, brick, plaster, roof shingles, planks, damask wallpaper) and stronger
  relief on the existing ones.
- Fixed the thread that ran up into the sky from the Skiff's lifeboat swing (it now hangs from a
  pencil crossbar). Bunting strings sag with their flags.
- Fountains and updrafts bring their own sounds. Level 1's background is now breeze, distant
  birds and a far-off bell. Cobbles, brick and planks sound like wood underfoot.

## The Fray and an open Level 1
- Story: a grey static called the Fray has seeped into the world through a tear in the sky and
  unravels whatever it touches. Every place is held together by golden Heartstrings; finding
  them and threading them back mends it. Captain Yarny tells you about it on the Skiff.
- Level 1 is now an open garden you can explore in any order, like a Mario Odyssey kingdom.
  The middle is the Mending Square with the Heart Spool; four bridges lead to Pincushion Peaks
  (Yarn swings and bounces), Dye Falls (Water fountains up dye cascades), Breezy Bluffs (Wind
  glides, updrafts and gusts) and the Spoolwork Ruins (sliding buttons, a Snarling arena and a
  drain into the old loom's courtyard).
- 8 Heartstrings: one at the top of each region, plus challenges that make one appear: pop the
  six Fray knots in the square, win the Thread Race at Dye Falls, unravel the Snarlings in the
  arena, and a secret cloud patch above the windmill.
- Each Heartstring lights a peg on the Heart Spool and pulls a snarl off it. Finding a region's
  Heartstring peels the Fray off that region and flowers grow back.
- Four Heartstrings wake the spool: a golden thread lifts any form up to the Fray's knot in the
  sky. Pop the four knots holding it, and the garden's heart is freed (that finishes the level).
- New: Fray knots and Snarlings (jump on them to pop them; walking into them pushes you back),
  homeward hoops that whisk you back to the square, a Heartstring counter on the HUD, and a
  countdown for the Thread Race.
- The 3 golden buttons are still secrets: bottom of the first swing in the Peaks, inside the
  drain hut on top of Dye Falls, and the humming spool tower in the Bluffs.
- Settings menu rows are easier to read when they're not highlighted (solid cards, darker text,
  values in their own coloured chips).
- The gold in the captain's cabin no longer shimmers.

## Captain Yarny and a tidier Skiff
- Captain Yarny, an old yarn-ball pirate with an eyepatch, a grey yarn beard, a tricorn, a pencil
  peg leg, a crochet-hook hand and a yarn parrot, stands by the helm. Press Ability next to him to
  talk; he has an introduction the first time and different chat after that, depending on what
  you've found.
- Ship sounds now come from where they happen (propellers, flag, creaking hull, the balloon,
  a clock in the cabin, the fountain and the air vent), so they get louder as you walk up to them
  instead of playing at full volume everywhere. Sounds are heard from near Patch, not the camera.
- Fixed flickering: the bow is one solid piece instead of overlapping boxes, rail corners,
  the cabin walls, the drain crate, the lifeboat and the crow's nest no longer overlap, and tape
  strips sit just above the surface.
- The balloon is one smooth shape with the patchwork painted on (the stuck-on patches floated),
  it no longer bobs away from its ropes, the flag swings on its pole instead of sliding off it,
  propellers have shafts into the hull, the rudder is taped on, and the globe's ring has legs.
- More pirate ship: cardboard-tube cannons with yarn cannonballs, a treasure chest of golden
  buttons, a skull-and-needles flag, paper lanterns on the rails, tin-can barrels, rope coils,
  a painted stripe on the hull, cabin windows, and a desk, map, candle, clock and hammock inside.

## The Cardboard Skiff
- Starting the game now shows the Little Colossus studio logo (with its own chime), then the
  Patchwork title screen over a slow fly-around of the hub. Any button starts.
- The hub is now the Cardboard Skiff: a little cardboard ship under a patched felt balloon,
  sailing over a sea of paint with clouds drifting past and the Level 1 basket island in the distance.
- Levels are picked at the ship's helm: press Ability and the camera moves in on a quilted globe
  that turns to each level's pin, with a card showing buttons found and best threads. Levels 2 and 3
  are on the globe as "Coming soon".
- New loading screen: quilt patches pop in, the level's name gets stitched across by a needle,
  a ball of yarn rolls along as the loading bar, and there's a tip. Used for every level change.
- Practice spots moved onto the ship: a fountain up to the crow's nest, a swing to the lifeboat,
  an air vent up to the cabin roof, and a drain crate. The Button Box is now the captain's cabin.
- Level 1 has yarn birds: some circle overhead, others hop and peck and flutter off when you run
  up to them. Their chirps come from where they are.
- A lake of swirling paint and ink now sits under Level 1, with paint tubes, ink bottles and
  brushes floating in it. The background sound is now a light breeze with paint gloops and drips
  (instead of the old sound that was like waves).
- Footsteps on cardboard.

## Level 1 rebuild
- Level 1 is now eight sections that climb out of the sewing basket: the garden, a drain in the
  wicker wall, the dye pond fountain, Button Bridge, Pincushion Canyon (two swings in a row with
  a pincushion island between), the Breezy Meadow (a glide gap, then a gust gap), a tall wicker
  wall you float over as Wind, and a finale up to the basket rim that uses all three forms.
- New set pieces: giant sewing pins, a tomato pincushion, scissors, a measuring tape and a tall
  stack of spools.
- Golden buttons moved to better hiding spots. The fountain one is inside a little hut on the shelf
  whose only door is a drain, and the secret one on top of the spool tower hums when you're close.
- Tutorial cards show the real button icons for your controller or keyboard.
- Your best thread count is saved. Half the threads unlocks the Felt Top Hat, all of them the
  Daisy Clip. Locked hats show what you need in the Satchel.
- The Button Box in the Sewing Room is sewn shut until all three golden buttons are found. Inside
  is the Golden Crown.
- Changing form now has a quick cloud poof that covers the swap.
- Footsteps that change with the ground (felt, grass, wicker, wood) and squelches for Water.
- Background sounds: a breeze with birds in Level 1, a quiet ticking clock in the Sewing Room.
- Patch has a little voice: a "hup" on some jumps, a "yay" for golden buttons and an "aww" when
  he falls. Higher for Water, highest for Wind.

## Settings, saves and tools
- New Settings page in the pause menu: music, sound and rumble levels, camera speed, invert camera
  up/down and left/right, Yarn swing as hold or tap-on/tap-off, button prompts, Form Wheel button
  and VSync.
- Progress is now saved to a file with a backup copy, so a crash can't wipe it. Old saves move
  over automatically.
- The level's texture repeats no longer stop Unity from batching draws (smoother on slower PCs).
- Patchwork > Play Test Gym: a plain room with metre stripes, height steps and gap lanes, with a
  readout of every jump's distance and height.
- Patchwork > Build for Windows makes a build in one click.
- A secret: tap the face buttons over and over and Patch starts dancing to his own song,
  a different dance and song for each form, with a little cutscene camera.

## Feel upgrades
- The D-pad faces (happy, sad, angry) now work on all three forms: the Water bubble and the
  Wind cloud got their own frowns and eyebrows.
- Jump height is now the same at any frame rate.
- Jumps feel snappier: a quicker drop after the top of a jump and a little hang at the top while
  Jump is held (Yarn and Water; Wind stays floaty). Swings, gusts and bounce pads are unchanged.
- Jumping off a moving button keeps its speed.
- Camera: stays level during normal jumps, looks a little ahead, backs out while swinging or
  gliding, slowly swings round behind you if you leave it alone, and pulls back over the swing gaps.
- Patch's walk, run and standing now blend smoothly by speed, his air pose follows whether he's
  rising or falling, and he leans into starts, stops and turns.
- Music has a layer for each form that crossfades when you change: music box for Water,
  plucked strings for Yarn, a breathy flute for Wind.

## Polish pass 3
- Changing Music, Sound or Rumble in the pause menu plays a tick that goes up in pitch as the
  setting goes up, and down as it goes down.
- Form Wheel: the description moved to its own card above the wheel, so the text no longer runs
  into the form pictures. Only the form's name sits in the middle.
- Restart, Return to the Sewing Room and Quit now ask "Are you sure?" first (starts on No).
- Bigger text on tutorial cards, the form panel, the Form Wheel and the Satchel.
- Tutorial cards stay up while you're standing in their area, and slide away 1.5 seconds after
  you leave.
- Decorations use softer pink and blue, so bright pink means a yarn anchor and bright blue means
  water. Scenery clouds are cream so they don't look like Wind Form.
- New crafted fonts: Fredoka (soft and rounded, like felt letters) for normal text and Sniglet
  (puffy cut-out letters) for big titles.

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

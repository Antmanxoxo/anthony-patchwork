# Patchwork

A small handmade-looking 3D platformer I'm making in **Unity 6 (URP, C#)**.
You play **Patch**, a little yarn creature who can switch between three forms:

| Form | What it does |
|---|---|
| 💧 **Water** | Turns into a jiggly bubble that slips through drain grates and rides fountains up. |
| 🧶 **Yarn** | A knitted toy with a yarn-ball head, button eyes and a stitched suit. Throws his yarn arm at pink anchors and swings across big gaps. |
| 🌬️ **Wind** | A happy cloud that glides, falls slowly, and gets one gust per jump. |

Right now there's a hub (**The Cardboard Skiff**, a little pirate ship you steer with a globe) and
one level (**Toybox Town**), where the Fray has spoiled the band on the Mayor's wedding day.
Find the three missing band pieces, collect threads and Heartstrings, and play with the toys
around town.

## How to play it
1. Open the project in **Unity 6000.6.7f1** (Unity Hub → Add → this folder).
2. Wait for Unity to finish importing/compiling.
3. Press **Play**. (The game always starts from the Hub.)

## Controls (PlayStation controller first, keyboard fallback)
| Action | PlayStation | Keyboard / Mouse |
|---|---|---|
| Move | Left stick | WASD |
| Camera | Right stick | Mouse |
| Jump (hold in air as Wind to glide) | ✕ Cross | Space |
| Ability (Yarn swing / Wind gust) | ▢ Square | E or Left click |
| Form Wheel | L1 (or R1, set in Pause) | Q |
| Confirm / Cancel in menus | ✕ / ◯ | Enter or Space / Esc or Backspace |
| Satchel (forms, buttons, hats) | Touchpad (or Create) | Tab |
| Map | D-pad Left | M |
| Pause | Options | Esc |
| Yarn's face (happy / sad / angry) | D-pad Up / Down / Right | 1 / 2 / 3 |

In the Form Wheel, point either stick (or the D-pad) at a form: Yarn is at the top, Water is
lower-left, Wind is lower-right. Time slows while the wheel is open.

## Project layout
- `Assets/Patchwork/Scripts`: all game code (see `DEV_NOTES.md` for a map)
- `Assets/Patchwork/Scenes`: `Hub` and `Level1` (built from code when you press Play)
- `Assets/Patchwork/Editor`: editor helpers (**Patchwork** menu in the Unity menu bar)

## Making a build
**Patchwork → Build for Windows**. The game ends up in `Builds/Windows/Patchwork.exe`.
Saves go in the player's AppData folder, so a new build starts fresh on another PC.

`PROJECT_STATUS.md` has what's working, `DEVELOPMENT_LOG.md` has the history.

## Fonts
- Fredoka and Sniglet, both under the SIL Open Font License 1.1 (licence files are next to the fonts in `Assets/Patchwork/Resources/Fonts`).

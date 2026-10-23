# Patchwork

A small handmade-looking 3D platformer I'm making in **Unity 6 (URP, C#)**.
You play **Patch**, a little yarn creature who can switch between three forms:

| Form | What it does |
|---|---|
| 💧 **Water** | Turns into a jiggly bubble that slips through drain grates and rides fountains up. |
| 🧶 **Yarn** | A knitted toy with a yarn-ball head, button eyes and a stitched suit. Throws his yarn arm at pink anchors and swings across big gaps. |
| 🌬️ **Wind** | A happy cloud that glides, falls slowly, and gets one gust per jump. |

Right now there's a hub (**The Sewing Room**) and one level (**The Sewing Basket Garden**) with
golden buttons and thread spools to find.

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
| Pause | Options | Esc |
| Yarn's face (happy / sad / angry) | D-pad Up / Down / Right | 1 / 2 / 3 |

In the Form Wheel, point either stick (or the D-pad) at a form: Yarn is at the top, Water is
lower-left, Wind is lower-right. Time slows while the wheel is open.

## Project layout
- `Assets/Patchwork/Scripts`: all game code (see `DEV_NOTES.md` for a map)
- `Assets/Patchwork/Scenes`: `Hub` and `Level1` (built from code when you press Play)
- `Assets/Patchwork/Editor`: editor helpers (**Patchwork** menu in the Unity menu bar)

`PROJECT_STATUS.md` has what's working, `DEVELOPMENT_LOG.md` has the history.

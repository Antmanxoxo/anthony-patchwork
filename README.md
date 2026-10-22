# Patchwork

A small, handmade-looking 3D platformer prototype made in **Unity 6 (URP, C#)**.
You play **Patch**, a little yarn creature who can transform between three forms:

| Form | What it does |
|---|---|
| 💧 **Water** | Shrinks into a droplet that slips through blue drain grates and rides blue fountains upward. |
| 🧶 **Yarn** | Patch's handmade self. Tethers to glowing pink button-anchors and swings across big gaps. |
| 🌬️ **Wind** | A breezy puff that glides, falls slowly, and bursts forward with one gust per jump. |

The prototype has a hub (**The Sewing Room**) and one level (**The Quilted Meadow**) with three
optional golden buttons to find.

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
| Form Wheel | L1 | Q |
| Confirm / Cancel in menus | ✕ / ◯ | Enter or Space / Esc or Backspace |
| Satchel (forms, buttons, hats) | Touchpad (or Create) | Tab |
| Pause | Options | Esc |

In the Form Wheel, point either stick (or the D-pad) at a form: Yarn is at the top, Water is
lower-left, Wind is lower-right. Time slows while the wheel is open.

## Project layout
- `Assets/Patchwork/Scripts`: all game code (see `DEV_NOTES.md` for a map)
- `Assets/Patchwork/Scenes`: `Hub` and `Level1` (built from code when you press Play)
- `Assets/Patchwork/Editor`: editor helpers (**Patchwork** menu in the Unity menu bar)

See `PROJECT_STATUS.md` for what works now and `DEVELOPMENT_LOG.md` for history.

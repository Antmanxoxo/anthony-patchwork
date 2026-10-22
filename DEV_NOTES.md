# DEV_NOTES.md — development notes for Patchwork

## What this is
**Patchwork** is a Unity 6 (6000.6.7f1) + URP + C# prototype: a controller-first 3D platformer
where Patch, a handmade yarn creature, transforms between **Water**, **Yarn** and **Wind** forms.
Goal of the prototype: prove the core loop is fun with one hub and one complete level.

Changes are tested by opening Unity, pressing Play, and checking the Console and how it feels.

## How the project is built
- **No hand-made scenes or prefabs.** Each scene (`Assets/Patchwork/Scenes/Hub.unity`,
  `Level1.unity`) contains only a Global Volume and a `LevelBootstrap` component. On Play,
  `PatchworkGame.Boot()` builds the level, lights, camera, player and HUD from code.
- All materials, textures, icons and sounds are generated at runtime
  (`PatchworkAssets`, `Sfx`). No imported art yet.
- UI is IMGUI (`OnGUI`) drawn in a virtual 1080p space (`UIKit`). Simple and dependable;
  can be swapped for UI Toolkit later.
- Input uses the Input System package, with actions created in code (`PatchworkInput`).
  The project's `activeInputHandler` is the new Input System only.
- `Assets/Patchwork/Editor/PatchworkEditorSetup.cs` makes Play always start from the Hub,
  opens the Hub on first load, and creates `Resources/PW_Lit.mat` (keeps URP Lit in builds).

## Code map (`Assets/Patchwork/Scripts`)
| Folder | Key types | Role |
|---|---|---|
| Core | `PatchworkGame`, `LevelBootstrap`, `GameState`, `PatchworkAssets`, `Sfx` | Boot, frame order, UI modes, checkpoints, scene loads, save data, generated assets |
| Input | `PatchworkInput` | All bindings (PS controller + keyboard) |
| Player | `PatchworkPlayer`, `PlayerMotor`, `PlayerCamera` | One shared CharacterController motor for every form; orbit camera |
| Forms | `FormDefinition`/`FormLibrary`, `FormController`, `FormAbility` + `WaterAbility`/`YarnAbility`/`WindAbility`, `FormVisuals`, `TransformBurst` | Form state, safe transforms, abilities, appearance, cosmetics |
| World | `LevelKit`, `HubBuilder`, `Level1Builder`, `PlayerVolume` + volumes, `YarnAnchor`, `WaterGrate` | Level construction and interactive pieces |
| UI | `UIKit`, `PatchworkHUD`, `FormSelector`, `SatchelMenu`, `PauseMenu` | HUD, radial Form Wheel, character-held Satchel, pause |

### Form system rules (keep these true)
- One motor (`PlayerMotor`) for all forms. Forms differ only by `FormDefinition` numbers and a `FormAbility`.
- Per frame order (in `PatchworkGame.Update` → `PatchworkPlayer.Tick`): form controller tick → ability `PreMove` → volumes → motor `Step` → ability `PostMove` → visuals.
- `FormController` defers transforms while `Ability.BlocksTransform` (yarn tether), refuses if the new capsule wouldn't fit (`HasRoomFor`), keeps the feet position, and clears transient motor state.
- Cosmetics (`GameState.HatIndex`, `FormVisuals` accessory root) are independent from form.
- Only one UI mode at a time (`UIMode`), so Form Wheel / Satchel / Pause can't conflict.

### Tuning facts the level depends on
- Normal jump ≈ 5 m horizontal. Wind (jump + glide + gust) ≈ 12 m. Yarn gap is 15 m, Wind gap 9 m.
- Water capsule 0.8 m tall × 0.32 m radius; tunnels are 1.0 m tall. Other forms 1.4 m × 0.45 m.
Change movement numbers → re-check gap sizes in `Level1Builder`.

## Workflow rules
- Every change must be checked by pressing Play in Unity. Write
  conservative C# (no newer language features than C# 9, prefer explicit types and APIs that
  have existed since Unity 2021).
- Keep `.meta` files committed; never regenerate GUIDs for existing files
  (`LevelBootstrap.cs.meta` GUID is referenced by both scenes).
- Commit at each milestone; push to `origin/main` only after it compiles and plays.
- Update `PROJECT_STATUS.md` and `DEVELOPMENT_LOG.md` with every milestone.

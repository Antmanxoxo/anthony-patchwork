using UnityEngine;
using UnityEngine.InputSystem;

namespace Patchwork
{
    /// <summary>
    /// All input bindings in one place. Controller-first (PlayStation layout), with keyboard/mouse fallback.
    ///
    ///   Action          PlayStation        Keyboard / Mouse
    ///   Move            Left stick         WASD
    ///   Camera          Right stick        Mouse
    ///   Jump / Glide    Cross              Space
    ///   Ability         Square             E / Left mouse
    ///   Form wheel      L1                 Q
    ///   Confirm (menus) Cross              Enter / Space
    ///   Cancel (menus)  Circle             Backspace / Esc
    ///   Satchel (HUD)   Touchpad / Create  Tab
    ///   Pause           Options            Esc
    ///
    /// The form wheel button is configurable via <see cref="SetFormWheelBinding"/>.
    /// </summary>
    public class PatchworkInput
    {
        public const string DefaultFormWheelGamepadPath = "<Gamepad>/leftShoulder";

        readonly InputActionMap map;

        public readonly InputAction Move;
        public readonly InputAction LookStick;
        public readonly InputAction LookMouse;
        public readonly InputAction Jump;
        public readonly InputAction Ability;
        public readonly InputAction FormWheel;
        public readonly InputAction Confirm;
        public readonly InputAction Cancel;
        public readonly InputAction Satchel;
        public readonly InputAction Pause;
        public readonly InputAction NavUp;
        public readonly InputAction NavDown;
        public readonly InputAction NavLeft;
        public readonly InputAction NavRight;
        public readonly InputAction Pick1;
        public readonly InputAction Pick2;
        public readonly InputAction Pick3;

        public enum PromptMode { Auto = 0, PlayStation = 1, Keyboard = 2 }
        const string PromptModeKey = "pw_prompt_mode";

        bool detectedKeyboard;

        /// <summary>Which button prompts to show. Auto follows the last device used.</summary>
        public PromptMode Prompts
        {
            get { return (PromptMode)Mathf.Clamp(PlayerPrefs.GetInt(PromptModeKey, 0), 0, 2); }
            set { PlayerPrefs.SetInt(PromptModeKey, (int)value); PlayerPrefs.Save(); }
        }

        /// <summary>True when keyboard prompts should be shown (and the mouse may steer the camera).</summary>
        public bool UsingKeyboard
        {
            get
            {
                var mode = Prompts;
                if (mode == PromptMode.PlayStation) return false;
                if (mode == PromptMode.Keyboard) return true;
                return detectedKeyboard;
            }
        }

        public PatchworkInput()
        {
            map = new InputActionMap("Patchwork");

            Move = map.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            Move.AddBinding("<Gamepad>/leftStick");
            Move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            LookStick = map.AddAction("LookStick", InputActionType.Value, "<Gamepad>/rightStick", expectedControlLayout: "Vector2");
            LookMouse = map.AddAction("LookMouse", InputActionType.Value, "<Mouse>/delta", expectedControlLayout: "Vector2");

            Jump = Button("Jump", "<Gamepad>/buttonSouth", "<Keyboard>/space");
            Ability = Button("Ability", "<Gamepad>/buttonWest", "<Keyboard>/e", "<Mouse>/leftButton");
            FormWheel = Button("FormWheel", DefaultFormWheelGamepadPath, "<Keyboard>/q");
            Confirm = Button("Confirm", "<Gamepad>/buttonSouth", "<Keyboard>/enter", "<Keyboard>/space", "<Keyboard>/numpadEnter");
            Cancel = Button("Cancel", "<Gamepad>/buttonEast", "<Keyboard>/backspace", "<Keyboard>/escape");
            Satchel = Button("Satchel", "<DualShockGamepad>/touchpadButton", "<Gamepad>/select", "<Keyboard>/tab");
            Pause = Button("Pause", "<Gamepad>/start", "<Keyboard>/escape");
            NavUp = Button("NavUp", "<Gamepad>/dpad/up", "<Keyboard>/upArrow");
            NavDown = Button("NavDown", "<Gamepad>/dpad/down", "<Keyboard>/downArrow");
            NavLeft = Button("NavLeft", "<Gamepad>/dpad/left", "<Keyboard>/leftArrow");
            NavRight = Button("NavRight", "<Gamepad>/dpad/right", "<Keyboard>/rightArrow");
            Pick1 = Button("Pick1", "<Keyboard>/1");
            Pick2 = Button("Pick2", "<Keyboard>/2");
            Pick3 = Button("Pick3", "<Keyboard>/3");

            map.Enable();
            detectedKeyboard = Gamepad.current == null;
        }

        InputAction Button(string name, params string[] paths)
        {
            var action = map.AddAction(name, InputActionType.Button);
            foreach (var p in paths) action.AddBinding(p);
            return action;
        }

        /// <summary>Rebind the gamepad button that opens the form wheel (e.g. "&lt;Gamepad&gt;/rightShoulder").</summary>
        public void SetFormWheelBinding(string gamepadPath)
        {
            map.Disable();
            FormWheel.ApplyBindingOverride(0, gamepadPath);
            map.Enable();
        }

        /// <summary>
        /// Call once per frame to track which device the player is using.
        /// Only deliberate input counts (buttons, sticks, keys, clicks); mouse drift is ignored so
        /// the prompts don't flicker back to keyboard when the cursor unlocks.
        /// </summary>
        public void UpdateDeviceKind()
        {
            var pad = Gamepad.current;
            if (pad != null)
            {
                if (pad.buttonSouth.wasPressedThisFrame || pad.buttonEast.wasPressedThisFrame ||
                    pad.buttonWest.wasPressedThisFrame || pad.buttonNorth.wasPressedThisFrame ||
                    pad.leftShoulder.wasPressedThisFrame || pad.rightShoulder.wasPressedThisFrame ||
                    pad.leftTrigger.wasPressedThisFrame || pad.rightTrigger.wasPressedThisFrame ||
                    pad.startButton.wasPressedThisFrame || pad.selectButton.wasPressedThisFrame ||
                    pad.dpad.up.wasPressedThisFrame || pad.dpad.down.wasPressedThisFrame ||
                    pad.dpad.left.wasPressedThisFrame || pad.dpad.right.wasPressedThisFrame ||
                    pad.leftStick.ReadValue().sqrMagnitude > 0.3f || pad.rightStick.ReadValue().sqrMagnitude > 0.3f)
                {
                    detectedKeyboard = false;
                    return;
                }
            }

            var kb = Keyboard.current;
            if (kb != null && kb.anyKey.wasPressedThisFrame)
            {
                detectedKeyboard = true;
                return;
            }

            var mouse = Mouse.current;
            if (mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame))
            {
                // A click right after a controller was in use is usually just focusing the Game view.
                if (pad == null) detectedKeyboard = true;
            }

            if (pad == null) detectedKeyboard = true;
        }

        public void Dispose()
        {
            map.Disable();
            map.Dispose();
        }
    }
}

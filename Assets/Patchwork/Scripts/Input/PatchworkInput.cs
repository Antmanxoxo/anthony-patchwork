using UnityEngine;
using UnityEngine.InputSystem;

namespace Patchwork
{
    // All the controls in one place. PlayStation controller first, keyboard as backup.
    // Move: left stick / WASD      Camera: right stick / mouse
    // Jump: X / Space              Ability: Square / E or left click
    // Form Wheel: L1 or R1 / Q     Satchel: touchpad / Tab     Pause: Options / Esc
    // Yarn's face: D-pad up / down / right (or 1 / 2 / 3) for happy / sad / angry
    // Menus: X / Enter to confirm, Circle / Esc to back out
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
        public readonly InputAction Map;
        public readonly InputAction NavUp;
        public readonly InputAction NavDown;
        public readonly InputAction NavLeft;
        public readonly InputAction NavRight;
        public readonly InputAction Pick1;
        public readonly InputAction Pick2;
        public readonly InputAction Pick3;
        public readonly InputAction FaceHappy;
        public readonly InputAction FaceSad;
        public readonly InputAction FaceAngry;

        public enum PromptMode { Auto = 0, PlayStation = 1, Keyboard = 2 }
        const string PromptModeKey = "pw_prompt_mode";

        bool detectedKeyboard;

        // Which button prompts to show. Auto follows the last device used.
        public PromptMode Prompts
        {
            get { return (PromptMode)Mathf.Clamp(PlayerPrefs.GetInt(PromptModeKey, 0), 0, 2); }
            set { PlayerPrefs.SetInt(PromptModeKey, (int)value); PlayerPrefs.Save(); }
        }

        // True when keyboard prompts should be shown (and the mouse may steer the camera).
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
            Map = Button("Map", "<Gamepad>/dpad/left", "<Keyboard>/m");
            NavUp = Button("NavUp", "<Gamepad>/dpad/up", "<Keyboard>/upArrow");
            NavDown = Button("NavDown", "<Gamepad>/dpad/down", "<Keyboard>/downArrow");
            NavLeft = Button("NavLeft", "<Gamepad>/dpad/left", "<Keyboard>/leftArrow");
            NavRight = Button("NavRight", "<Gamepad>/dpad/right", "<Keyboard>/rightArrow");
            Pick1 = Button("Pick1", "<Keyboard>/1");
            Pick2 = Button("Pick2", "<Keyboard>/2");
            Pick3 = Button("Pick3", "<Keyboard>/3");
            FaceHappy = Button("FaceHappy", "<Gamepad>/dpad/up", "<Keyboard>/1");
            FaceSad = Button("FaceSad", "<Gamepad>/dpad/down", "<Keyboard>/2");
            FaceAngry = Button("FaceAngry", "<Gamepad>/dpad/right", "<Keyboard>/3");

            map.Enable();
            detectedKeyboard = Gamepad.current == null;
            if (FormWheelOnR1) FormWheelOnR1 = true; // apply the saved choice
            else FormWheelPadLabel = "L1";
        }

        InputAction Button(string name, params string[] paths)
        {
            var action = map.AddAction(name, InputActionType.Button);
            foreach (var p in paths) action.AddBinding(p);
            return action;
        }

        const string WheelOnR1Key = "pw_wheel_r1";

        // Button label shown in prompts for the Form Wheel ("L1" or "R1").
        public static string FormWheelPadLabel = "L1";

        // True when the Form Wheel is on R1 instead of L1 (saved).
        public bool FormWheelOnR1
        {
            get { return PlayerPrefs.GetInt(WheelOnR1Key, 0) == 1; }
            set
            {
                PlayerPrefs.SetInt(WheelOnR1Key, value ? 1 : 0);
                PlayerPrefs.Save();
                SetFormWheelBinding(value ? "<Gamepad>/rightShoulder" : DefaultFormWheelGamepadPath);
                FormWheelPadLabel = value ? "R1" : "L1";
            }
        }

        // Rebind the gamepad button that opens the form wheel (e.g. "<Gamepad>/rightShoulder").
        public void SetFormWheelBinding(string gamepadPath)
        {
            map.Disable();
            FormWheel.ApplyBindingOverride(0, gamepadPath);
            map.Enable();
        }

        // Call once per frame to track which device the player is using.
        // Only deliberate input counts (buttons, sticks, keys, clicks); mouse drift is ignored so
        // the prompts don't flicker back to keyboard when the cursor unlocks.
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

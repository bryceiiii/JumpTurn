using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JumpTurn
{
    public sealed class GameInput : IDisposable
    {
        private readonly InputAction _move;
        private readonly InputAction _jump;
        private readonly InputAction _rotateLeft;
        private readonly InputAction _rotateRight;
        private readonly InputAction _phase;
        private readonly InputAction _pause;
        private readonly InputAction _restart;

        public GameInput()
        {
            _move = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            _move.AddBinding("<Gamepad>/leftStick");
            _move.AddBinding("<Gamepad>/dpad");

            _jump = Button("Jump", "<Keyboard>/space", "<Gamepad>/buttonSouth");
            _rotateLeft = Button("RotateLeft", "<Keyboard>/q", "<Gamepad>/leftShoulder");
            _rotateRight = Button("RotateRight", "<Keyboard>/e", "<Gamepad>/rightShoulder");
            _phase = Button("Phase", "<Keyboard>/leftShift", "<Gamepad>/buttonWest");
            _pause = Button("Pause", "<Keyboard>/escape", "<Gamepad>/start");
            _restart = Button("Restart", "<Keyboard>/r", "<Gamepad>/select");

            _move.Enable();
            _jump.Enable();
            _rotateLeft.Enable();
            _rotateRight.Enable();
            _phase.Enable();
            _pause.Enable();
            _restart.Enable();
        }

        public Vector2 Move => _move.ReadValue<Vector2>();
        public bool JumpPressed => _jump.WasPressedThisFrame();
        public bool JumpHeld => _jump.IsPressed();
        public bool JumpReleased => _jump.WasReleasedThisFrame();
        public bool RotateLeftPressed => _rotateLeft.WasPressedThisFrame();
        public bool RotateRightPressed => _rotateRight.WasPressedThisFrame();
        public bool PhasePressed => _phase.WasPressedThisFrame();
        public bool PausePressed => _pause.WasPressedThisFrame();
        public bool RestartPressed => _restart.WasPressedThisFrame();

        public void Dispose()
        {
            _move.Dispose();
            _jump.Dispose();
            _rotateLeft.Dispose();
            _rotateRight.Dispose();
            _phase.Dispose();
            _pause.Dispose();
            _restart.Dispose();
        }

        private static InputAction Button(string name, string keyboardPath, string gamepadPath)
        {
            var action = new InputAction(name, InputActionType.Button);
            action.AddBinding(keyboardPath);
            action.AddBinding(gamepadPath);
            return action;
        }
    }
}

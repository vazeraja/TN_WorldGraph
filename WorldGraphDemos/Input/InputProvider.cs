using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace ThunderNut.WorldGraph.Demos {

    [CreateAssetMenu(fileName = "InputReader", menuName = "InputData/Input Reader")]
    public class InputProvider : ScriptableObject, IInputProvider, GameInput.IGameplayActions {
        private void OnEnable() {
            if (GameInput == null) {
                GameInput = new GameInput();
                GameInput.Gameplay.SetCallbacks(this);
            }

            GameInput.Gameplay.Enable();
        }

        private void OnDisable() => DisableAllInput();
        
        private GameInput GameInput { get; set; }
        
        private Vector2 movementDirection;
        private bool isCrouching;
        public event Action<float> onJump;
        public event Action<float> onDash;

        public InputState GetState() =>
            new InputState {
                movementDirection = movementDirection,
                isCrouching = isCrouching,
            };

        public static implicit operator InputState(InputProvider provider) => provider.GetState();

        public void OnMove(InputAction.CallbackContext context) {
            movementDirection = context.ReadValue<Vector2>();
        }

        public void OnCrouch(InputAction.CallbackContext context) {
            if (context.phase == InputActionPhase.Performed) {
                isCrouching = context.ReadValue<float>() > 0.5f;
            }
        }

        public void OnJump(InputAction.CallbackContext context) {
            if (context.phase == InputActionPhase.Performed) {
                onJump?.Invoke(context.ReadValue<float>());
            }
        }

        public void OnDash(InputAction.CallbackContext context) {
            if (context.phase == InputActionPhase.Performed)
                onDash?.Invoke(context.ReadValue<float>());
        }

        public void OnInteract(InputAction.CallbackContext context) {
        }


        public void EnableGameplayInput() {
            GameInput.Gameplay.Enable();
        }

        public void DisableAllInput() {
            GameInput.Gameplay.Disable();
        }
    }

}
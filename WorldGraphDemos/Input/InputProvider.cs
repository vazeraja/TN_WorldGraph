using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "InputReader", menuName = "InputData/Input Reader")]
public class InputProvider : ScriptableObject, IInputProvider, GameInput.IGameplayActions, GameInput.IDialoguesActions,
    GameInput.IMenuActions {

    private Vector2 movementDirection;
    private bool isCrouching;
    public event UnityAction<float> onJump;
    public event UnityAction<float> onDash;


    #region Other

    public event UnityAction InteractionStartedEvent;
    public event UnityAction InteractionCancelledEvent;
    public event UnityAction HatEvent;
    public event UnityAction OpenDevConsole;
    public event UnityAction AdvanceDialogueEvent;

    #endregion

    #region Menu UnityActions

    public event UnityAction OpenMenuWindow;
    public event UnityAction TabRightButtonEvent;
    public event UnityAction TabLeftButtonEvent;
    public event UnityAction CloseMenuWindow;

    #endregion

    private GameInput GameInput { get; set; }

    private void OnEnable()
    {
        if (GameInput == null) {
            GameInput = new GameInput();
            GameInput.Gameplay.SetCallbacks(this);
            GameInput.Dialogues.SetCallbacks(this);
            GameInput.Menu.SetCallbacks(this);
        }

        EnableGameplayInput();
    }

    private void OnDisable() => DisableAllInput();

    #region Gameplay Actions


    public InputState GetState() =>
        new InputState {
            movementDirection = movementDirection,
            isCrouching = isCrouching
        };

    public static implicit operator InputState(InputProvider provider) => provider.GetState();

    public void OnMove(InputAction.CallbackContext context)
    {
        movementDirection = context.ReadValue<Vector2>();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed) {
            isCrouching = context.ReadValue<float>() > 0.5f;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed) {
            onJump?.Invoke(context.ReadValue<float>());
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            onDash?.Invoke(context.ReadValue<float>());
    }

    public void OnHat(InputAction.CallbackContext context)
    {
        if (HatEvent != null && context.phase == InputActionPhase.Started)
            HatEvent?.Invoke();
    }


    public void OnOpenDevConsole(InputAction.CallbackContext context)
    {
        if (OpenDevConsole != null && context.phase == InputActionPhase.Performed)
            OpenDevConsole.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (InteractionStartedEvent != null && context.phase == InputActionPhase.Started)
            InteractionStartedEvent.Invoke();

        if (InteractionCancelledEvent != null && context.phase == InputActionPhase.Canceled)
            InteractionCancelledEvent.Invoke();
    }

    public void OnMenu(InputAction.CallbackContext context)
    {
        if (OpenMenuWindow != null && context.phase == InputActionPhase.Started)
            OpenMenuWindow?.Invoke();
    }

    #endregion

    #region Dialogue Actions

    public void OnAdvanceDialogue(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            AdvanceDialogueEvent?.Invoke();
    }

    #endregion

    #region Menu Actions

    public void OnTabLeft(InputAction.CallbackContext context)
    {
        if (TabLeftButtonEvent != null && context.phase == InputActionPhase.Started) {
            TabLeftButtonEvent?.Invoke();
        }
    }

    public void OnTabRight(InputAction.CallbackContext context)
    {
        if (TabRightButtonEvent != null && context.phase == InputActionPhase.Started)
            TabRightButtonEvent?.Invoke();
    }

    public void OnCloseMenu(InputAction.CallbackContext context)
    {
        if (CloseMenuWindow != null && context.phase == InputActionPhase.Started)
            CloseMenuWindow?.Invoke();
    }

    #endregion

    #region Helpful Methods
    public void EnableGameplayInput()
    {
        GameInput.Gameplay.Enable();
        GameInput.Dialogues.Disable();
        GameInput.Menu.Disable();
    }

    public void EnableDialogueInput()
    {
        GameInput.Dialogues.Enable();
        GameInput.Gameplay.Disable();
        GameInput.Menu.Disable();
    }

    public void EnableMenuInput()
    {
        GameInput.Menu.Enable();
        GameInput.Gameplay.Disable();
        GameInput.Dialogues.Disable();
    }

    public void DisableAllInput()
    {
        GameInput.Gameplay.Disable();
        GameInput.Dialogues.Disable();
        GameInput.Menu.Disable();
    }
    #endregion
    
}
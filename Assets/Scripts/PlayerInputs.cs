using System;
using UnityEngine;
using UnityEngine.UIElements;


class InGameActions
{
    bool enabled = false;
    public Action<Vector2> Move;
    public Action JumpPressed;
    public Action JumpRelease;
    public Action Attack;
    public Action Interact;
    public Action RunPressed;
    public Action RunRelease;
    public Action Pause;

    public void Enable()
    {
        enabled = true;
    }

    public void Disable()
    {
        enabled = false;
    }

    public void OnMove(Vector2 val)
    {
        if (!enabled) return;
        Move?.Invoke(val);
    }

    public void OnJumpPressed()
    {
        if (!enabled) return;

        JumpPressed?.Invoke();
    }

    public void OnJumpReleased()
    {
        if (!enabled) return;

        JumpRelease?.Invoke();
    }

    public void OnAttack()
    {
        if (!enabled) return;

        Attack?.Invoke();
    }

    public void OnInteract()
    {
        if (!enabled) return;

        Interact?.Invoke();
    }

    public void OnRunPressed()
    {
        if (!enabled) return;

        RunPressed?.Invoke();
    }

    public void OnRunReleased()
    {
        if (!enabled) return;

        RunRelease?.Invoke();
    }

    public void OnPause()
    {
        if (!enabled) return;
        Pause?.Invoke();
    }
}

class UIActions
{
    bool enabled = false;

    public void Enable()
    {
        enabled = true;
    }

    public void Disable()
    {
        enabled = false;
    }

    public Action<Vector2> Navigate;
    public Action Approve;
    public Action Cancel;
    public Action<VisualElement> Pressed;

    public void OnNavigate(Vector2 nav)
    {
        if (!enabled) return;
        Navigate?.Invoke(nav);
    }
    public void OnApprove()
    {
        if (!enabled) return;
        Approve?.Invoke();
    }
    public void OnCancel()
    {
        if (!enabled) return;
        Cancel?.Invoke();
    }

    public void OnPressed(VisualElement elem)
    {
        if (!enabled) return;
        Pressed?.Invoke(elem);
    }
}

class PlayerInputs : MonoBehaviour
{
    private static PlayerInputs instance;
    public static PlayerInputs Instance
    {
        get => instance;
    }

    //private CustomControls customControls;
    private InputSystem_Actions defaultInputs;

    public InGameActions inGameActions;
    public UIActions uiActions;

    InputRepeatHandler inGameMoveRepeat;
    InputRepeatHandler uiNavigateRepeat;

    private bool isUpdatingMove = false;
    private bool isAttackActive = false;
    private bool isInteractActive = false;
    private bool enableUiInputNextFrame = false;
    private float moveDeadzone = 0.2f;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize both input systems
        //customControls = new CustomControls();
        defaultInputs = new InputSystem_Actions();

        inGameActions = new InGameActions();
        uiActions = new UIActions();

        inGameMoveRepeat = new InputRepeatHandler(this, 0.2f);
        uiNavigateRepeat = new InputRepeatHandler(this, 0.4f);
        //customControls.Enable();
        defaultInputs.Enable();

        // customControls.Player.Drop.started += ctx => TriggerIfNotActive(ref isAttackActive, inGameActions.OnDrop);
        // customControls.Player.Drop.canceled += ctx => isAttackActive = false;
        // customControls.Player.RotateClockwise.started += ctx => TriggerIfNotActive(ref isRotateClockwiseActive, inGameActions.OnRotateClockwise);
        // customControls.Player.RotateClockwise.canceled += ctx => isRotateClockwiseActive = false;
        // customControls.Player.RotateCounterClockwise.started += ctx => TriggerIfNotActive(ref isInteractActive, inGameActions.OnRotateCounterClockwise);
        // customControls.Player.RotateCounterClockwise.canceled += ctx => isInteractActive = false;
        // customControls.Player.Hold.started += ctx => TriggerIfNotActive(ref isHoldActive, inGameActions.OnHold);
        // customControls.Player.Hold.canceled += ctx => isHoldActive = false;
        // customControls.Player.Move.performed += ctx => HandleMoveValueChanged();
        // customControls.Player.Move.canceled += ctx => inGameMoveRepeat.Stop();

        defaultInputs.Player.Attack.started += ctx => TriggerIfNotActive(ref isAttackActive, inGameActions.OnAttack);
        defaultInputs.Player.Attack.canceled += ctx => isAttackActive = false;
        defaultInputs.Player.Jump.started += ctx => inGameActions.OnJumpPressed();
        defaultInputs.Player.Jump.canceled += ctx => inGameActions.OnJumpReleased();
        defaultInputs.Player.Interact.started += ctx => TriggerIfNotActive(ref isInteractActive, inGameActions.OnInteract);
        defaultInputs.Player.Interact.canceled += ctx => isInteractActive = false;
        defaultInputs.Player.Sprint.started += ctx => inGameActions.OnRunPressed();
        defaultInputs.Player.Sprint.canceled += ctx => inGameActions.OnRunReleased();
        defaultInputs.Player.Move.performed += ctx => HandleMoveValueChanged();
        defaultInputs.Player.Move.canceled += ctx => inGameMoveRepeat.Stop();

        inGameMoveRepeat.Repeat += TriggerMoveAction;

        defaultInputs.UI.Submit.started += ctx => uiActions.OnApprove();
        defaultInputs.UI.Cancel.started += ctx => uiActions.OnCancel();
        defaultInputs.UI.Navigate.started += ctx => uiNavigateRepeat.Start();
        defaultInputs.UI.Navigate.canceled += ctx => uiNavigateRepeat.Stop();
        uiNavigateRepeat.Repeat += TriggerNav;

        RegisterMouseClickHandler();
    }

    // public CustomControls GetCustomControls()
    // {
    //     return customControls;
    // }

    public InputSystem_Actions GetDefaultInputs()
    {
        return defaultInputs;
    }

    private void TriggerIfNotActive(ref bool isActive, Action action)
    {
        if (isActive) return;
        isActive = true;
        action?.Invoke();
    }

    private void HandleMoveValueChanged()
    {
        if (isUpdatingMove) return;

        isUpdatingMove = true;
        // float moveValue = customControls.Player.Move.IsPressed()
        //     ? customControls.Player.Move.ReadValue<float>()
        //     : defaultInputs.Player.Move.ReadValue<float>();

        Vector2 moveValue = defaultInputs.Player.Move.ReadValue<Vector2>();

        if (moveValue.magnitude >= moveDeadzone)
        {
            if (!inGameMoveRepeat.IsRunning())
            {
                inGameMoveRepeat.Start();
            }
            if (!uiNavigateRepeat.IsRunning())
            {
                uiNavigateRepeat.Start();
            }
        }
        else
        {
            if (inGameMoveRepeat.IsRunning())
            {
                inGameMoveRepeat.Stop();
            }
            if (uiNavigateRepeat.IsRunning())
            {
                uiNavigateRepeat.Stop();
            }
        }
        isUpdatingMove = false;
    }

    private void TriggerMoveAction()
    {
        // float moveValue = customControls.Player.Move.IsPressed()
        //     ? customControls.Player.Move.ReadValue<float>()
        //     : defaultInputs.Player.Move.ReadValue<float>();
        Vector2 moveValue = defaultInputs.Player.Move.ReadValue<Vector2>();

        if (moveValue.magnitude >= moveDeadzone)
        {
            inGameActions.OnMove(moveValue);
        }
    }

    private void TriggerNav()
    {
        Vector2 moveValue =
            defaultInputs.UI.Navigate.ReadValue<Vector2>();

        uiActions.OnNavigate(moveValue);
    }

    public void RegisterMouseClickHandler()
    {
        UIDocument uiDoc = FindFirstObjectByType<UIDocument>();
        if (uiDoc == null) return;

        var root = uiDoc.rootVisualElement;

        root.RegisterCallback<PointerDownEvent>(OnPointerClick, TrickleDown.TrickleDown);
    }

    private void OnPointerClick(PointerDownEvent evt)
    {
        VisualElement clickedElement = evt.target as VisualElement;
        if (clickedElement == null) return;
        uiActions.OnPressed(clickedElement);
    }

    void Update()
    {
        if (enableUiInputNextFrame)
        {
            enableUiInputNextFrame = false;
            uiActions.Enable();
        }
    }

    public void EnableUiInputNextFrame()
    {
        enableUiInputNextFrame = true;
    }

}

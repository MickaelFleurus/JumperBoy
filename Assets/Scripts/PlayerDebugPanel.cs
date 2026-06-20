using UnityEngine;
using UnityEngine.UIElements;

public class PlayerDebugPanel : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    [SerializeField] private Player player;
    [SerializeField] private bool ignoreKeyboardInput = true;

    private Label positionField;
    private Label velocityField;
    private VectorArrowElement velocityVector;
    private Label stateLabel;
    private IntegerField jumpRepeat;
    private Label jumpLeftLabel;

    void Start()
    {
        // Set up UI toggle
        PlayerInputs.Instance.inGameActions.ToggleDebugUI += OnToggleDebugUI;

        // Initialize UI element references
        if (uiDocument != null)
        {
            InitializeUIElements();
        }
    }

    void InitializeUIElements()
    {
        var root = uiDocument.rootVisualElement;

        if (ignoreKeyboardInput)
            root.pickingMode = PickingMode.Ignore;

        positionField = root.Q<Label>("PositionValue");
        velocityField = root.Q<Label>("VelocityValue");
        stateLabel = root.Q<Label>("StateValue");
        velocityVector = root.Q<VectorArrowElement>("VelocityVector");
        jumpRepeat = root.Q<IntegerField>("JumpAmountRoot");
        jumpLeftLabel = root.Q<Label>("JumpLeftValue");
    }

    void Update()
    {
        // Skip update if debug panel is hidden
        if (uiDocument == null || uiDocument.rootVisualElement.style.display == DisplayStyle.None)
            return;

        UpdateUIWithPlayerData();
    }
    string FormatVector(Vector2 v) => $"({v.x}, {v.y})";
    void UpdateUIWithPlayerData()
    {

        positionField.text = FormatVector(player.transform.position);

        velocityField.text = FormatVector(player.GetVelocity());
        velocityVector.Vector = player.GetVelocity();

        string state = player.IsGrounded() ? "Grounded" : "Falling";
        if (player.GetJumpHandler().IsJumping)
            state = "Jumping";
        stateLabel.text = state;

        jumpLeftLabel.text = player.GetJumpHandler().JumpLeft.ToString();
    }

    void OnToggleDebugUI()
    {
        if (uiDocument == null)
            return;

        if (uiDocument.rootVisualElement.style.display == DisplayStyle.None)
            uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        else
            uiDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    public void SetIgnoreInput(bool ignoreInput)
    {
        ignoreKeyboardInput = ignoreInput;
        if (uiDocument != null)
        {
            uiDocument.rootVisualElement.pickingMode = ignoreInput ? PickingMode.Ignore : PickingMode.Position;
        }
    }
}

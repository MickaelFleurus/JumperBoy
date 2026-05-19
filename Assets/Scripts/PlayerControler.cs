using UnityEngine;

public class PlayerControler : MonoBehaviour
{
    enum PlayerState { Idle, Walk, Run, Hanging, Rising, Falling };

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Vector2 velocity = Vector2.zero;
    private float gravityScale = 9.8f; // Adjust to feel right
    private Vector2 currentDirection = new Vector2(0f, 0f);
    private bool isGrounded = false;
    private LayerMask groundLayer;
    private float groundCheckDistance = 0.01f;

    private PlayerState currentState = PlayerState.Idle;
    void Start()
    {
        groundLayer = LayerMask.GetMask("Ground");
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0; // Turn off automatic gravity
        PlayerInputs.Instance.inGameActions.Enable();
        PlayerInputs.Instance.inGameActions.Move += OnMove;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 raycastOrigin = new Vector2(
            transform.position.x,
            playerCollider.bounds.min.y
        );

        RaycastHit2D hit = Physics2D.Raycast(
            raycastOrigin,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        isGrounded = hit.collider != null;

        if (isGrounded)
            currentState = PlayerState.Idle;
        else
            currentState = PlayerState.Falling;

        if (!isGrounded)
            velocity.y -= gravityScale * Time.deltaTime;
        else
            velocity.y = 0.0f;

        float speed = currentState == PlayerState.Run ? runSpeed : walkSpeed;
        velocity.x = currentDirection.x * speed;
        Debug.Log(currentDirection);

        rb.linearVelocity = velocity;
    }

    private void OnMove(Vector2 direction)
    {
        currentDirection = direction;
    }
}

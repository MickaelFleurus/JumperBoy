using UnityEngine;

public class PlayerControler : MonoBehaviour
{
    enum PlayerState { Idle, Walk, Run, Hanging, Rising, Falling };

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float accelerationRate = 50f;
    [SerializeField] private float decelerationRate = 30f;
    [SerializeField] private float jumpPower = 0.5f;
    [SerializeField] private float jumpingMore = 4f;
    [SerializeField] private float maxJumpTime = 1f;
    private float timeSinceJumpStart = 0f;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Vector2 velocity = Vector2.zero;
    private float gravityScale = 9.8f;
    private Vector2 currentDirection = new Vector2(0f, 0f);
    private bool isGrounded = false;
    private LayerMask groundLayer;
    private float groundCheckDistance = 0.01f;

    private PlayerState currentState = PlayerState.Idle;
    private bool isJumping = false; // Track if jump button is held

    void Start()
    {
        groundLayer = LayerMask.GetMask("Ground");
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0; // Turn off automatic gravity
        PlayerInputs.Instance.inGameActions.Enable();
        PlayerInputs.Instance.inGameActions.Move += OnMove;
        PlayerInputs.Instance.inGameActions.JumpPressed += OnJumpStart;
        PlayerInputs.Instance.inGameActions.JumpRelease += OnJumpStop;

    }


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

        if (isGrounded && !isJumping)
        {
            currentState = PlayerState.Idle;
            velocity.y = 0.0f;
        }
        else
        {
            float jumpingAttenuation = 0f;
            if (isJumping)
            {
                timeSinceJumpStart += Time.deltaTime;
                if (timeSinceJumpStart < maxJumpTime)
                {
                    jumpingAttenuation = jumpingMore;
                }
                else
                {
                    isJumping = false;
                }
            }
            velocity.y -= (gravityScale - jumpingAttenuation) * Time.deltaTime;

        }
        Debug.Log(velocity.y);

        if (Mathf.Abs(currentDirection.x) > 0.1f)
        {
            float targetSpeed = currentDirection.x * (currentState == PlayerState.Run ? runSpeed : walkSpeed);
            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, accelerationRate * Time.deltaTime);
        }
        else
        {
            velocity.x = Mathf.MoveTowards(velocity.x, 0, decelerationRate * Time.deltaTime);
        }

        rb.linearVelocity = velocity;
    }

    private void OnMove(Vector2 direction)
    {
        currentDirection = direction;
    }

    private void OnJumpStart()
    {
        if (!isGrounded) return;

        isJumping = true;
        currentState = PlayerState.Rising;
        velocity.y = jumpPower;
        timeSinceJumpStart = 0.0f;
    }

    private void OnJumpStop()
    {
        isJumping = false;
    }
}

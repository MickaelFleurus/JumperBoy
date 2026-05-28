using System;
using UnityEngine;

public class Player : MonoBehaviour
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
    private float groundCheckDistance = 0.2f;
    private float lateralCheckDistance = 0.25f;

    private PlayerState currentState = PlayerState.Idle;
    private bool isJumping = false;

    private RaycastHit2D[] hitResults = new RaycastHit2D[3];

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


    void FixedUpdate()
    {
        isGrounded = CheckVecticalCollision();
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
                bool hitCeiling = CheckVecticalCollision();
                if (!hitCeiling)
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
                else
                {
                    isJumping = false;
                    velocity.y = 0f;
                }

            }
            velocity.y -= Math.Max((gravityScale - jumpingAttenuation) * Time.deltaTime, -10f);
        }

        if (Mathf.Abs(currentDirection.x) > 0.1f)
        {
            float targetSpeed = currentDirection.x * (currentState == PlayerState.Run ? runSpeed : walkSpeed);

            if ((currentDirection.x > 0 && !CheckLateralCollision(Vector2.right)) || (currentDirection.x < 0 && !CheckLateralCollision(Vector2.left)))
            {
                velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, accelerationRate * Time.deltaTime);
            }
            else
            {
                velocity.x = 0; // Stop if hitting a wall
            }
        }
        else
        {
            velocity.x = Mathf.MoveTowards(velocity.x, 0, decelerationRate * Time.deltaTime);
        }

        Debug.Log(velocity);
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

    private bool CheckLateralCollision(Vector2 direction)
    {
        float xOrigin = direction.x > 0 ? playerCollider.bounds.max.x : playerCollider.bounds.min.x;
        Span<Vector2> origins = stackalloc Vector2[3]{new Vector2(
            xOrigin,
            playerCollider.bounds.min.y + 0.1f
        ), new Vector2(
            xOrigin,
            playerCollider.bounds.center.y
        ), new Vector2(
            xOrigin,
            playerCollider.bounds.max.y - 0.1f
        )};

        var interaction = new InteractionInitiator();
        interaction.angle = InteractionInitiator.EInteractionAngle.Sideway;
        interaction.go = this.gameObject;
        interaction.hitStrenght = 1;

        foreach (var origin in origins)
        {
            int hits = Physics2D.RaycastNonAlloc(
                        origin,
                        direction, hitResults,
                        lateralCheckDistance,
                        groundLayer
                    );

            for (int i = 0; i < hits; ++i)
            {
                var ground = hitResults[i].transform.gameObject.GetComponent<Ground>();

                if (hitResults[i].distance > 0)
                {
                    var result = ground.OnInteraction(interaction);
                    if (result.stopMovement)
                        return true;
                }
            }

        }

        return false;
    }

    private bool CheckVecticalCollision()
    {
        bool fromBelow = velocity.y > 0;
        float yOrigin = fromBelow ? playerCollider.bounds.max.y : playerCollider.bounds.min.y;
        Span<Vector2> origins = stackalloc Vector2[3]{new Vector2(
            playerCollider.bounds.min.x, yOrigin
        ), new Vector2(
            playerCollider.bounds.center.x, yOrigin
        ), new Vector2(
            playerCollider.bounds.max.x , yOrigin
        )};

        var interaction = new InteractionInitiator();
        interaction.angle = fromBelow ? InteractionInitiator.EInteractionAngle.FromBeneath : InteractionInitiator.EInteractionAngle.FromAbove;
        interaction.go = this.gameObject;
        interaction.hitStrenght = 1;

        foreach (var origin in origins)
        {
            int hits = Physics2D.RaycastNonAlloc(
                    origin,
                    Vector2.down, hitResults,
                    groundCheckDistance,
                    groundLayer);


            for (int i = 0; i < hits; ++i)
            {
                if (hitResults[i].distance < 0f) continue;
                var ground = hitResults[i].transform.gameObject.GetComponent<Ground>();
                var result = ground.OnInteraction(interaction);
                if (result.stopMovement)
                    return true;
            }
        }

        return false;
    }
}

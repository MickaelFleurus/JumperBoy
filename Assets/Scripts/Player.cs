using System;
using NUnit.Framework;
using UnityEngine;

public class Player : MonoBehaviour
{
    enum PlayerState { Idle, Walk, Run, Hanging, Rising, Falling };

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float accelerationRate = 50f;
    [SerializeField] private float decelerationRate = 30f;
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

    private JumpHandler jumpHandler;
    private DashHandler dashHandler;
    private SpriteRenderer spriteRenderer;
    private bool ignoreNextWalkThroughCollision = false;

    private RaycastHit2D[] hitResults = new RaycastHit2D[3];

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        jumpHandler = ScriptableObject.CreateInstance<JumpHandler>();
        dashHandler = ScriptableObject.CreateInstance<DashHandler>();

        groundLayer = LayerMask.GetMask("Ground");
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        PlayerInputs.Instance.inGameActions.Enable();
        PlayerInputs.Instance.inGameActions.Move += OnMove;
        PlayerInputs.Instance.inGameActions.JumpPressed += OnJumpPressed;
        PlayerInputs.Instance.inGameActions.JumpRelease += OnJumpReleased;
        PlayerInputs.Instance.inGameActions.Dash += OnDash;

    }

    void Update()
    {
        jumpHandler.Updated(Time.deltaTime);
        dashHandler.Updated(Time.deltaTime);
    }


    void FixedUpdate()
    {
        if (!dashHandler.IsDashing)
        {
            if (velocity.y <= 0f)
            {
                bool isNowGrounded = HandleGroundCollision();

                if (!isGrounded && isNowGrounded)
                {
                    velocity.y = 0.0f;
                    ignoreNextWalkThroughCollision = false;
                    jumpHandler.OnJumpReset();
                    dashHandler.OnDashReset();
                }
                else if (!isNowGrounded)
                {
                    velocity.y -= Math.Max(gravityScale * Time.deltaTime, -10f);
                }
                isGrounded = isNowGrounded;
            }
            else  // velocity.y > 0f
            {
                if (CheckVecticalCollision()) // Stop ascending
                {
                    jumpHandler.OnJumpingStop();
                    velocity.y = 0f;
                }
                else
                {
                    float jumpingAttenuation = 0f;
                    if (jumpHandler.IsJumping)
                    {
                        timeSinceJumpStart += Time.deltaTime;
                        if (timeSinceJumpStart < jumpHandler.MaxJumpTime)
                        {
                            jumpingAttenuation = jumpHandler.JumpingMore;
                        }
                        else
                        {
                            jumpHandler.OnJumpingStop();
                        }
                    }
                    velocity.y -= Math.Max((gravityScale - jumpingAttenuation) * Time.deltaTime, -10f);
                }
            }
        }
        if (!dashHandler.IsDashing)
        {
            if (Mathf.Abs(currentDirection.x) > 0.1f)
            {
                float targetSpeed = currentDirection.x * (true ? runSpeed : walkSpeed);

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
        }
        else
        {
            if ((currentDirection.x > 0 && CheckLateralCollision(Vector2.right)) || (currentDirection.x < 0 && CheckLateralCollision(Vector2.left)))
            {
                velocity.x = 0f;
            }
        }


        rb.linearVelocity = velocity;
    }

    private void OnMove(Vector2 direction)
    {
        currentDirection = direction;
    }

    private void OnJumpPressed()
    {
        if (isGrounded && currentDirection.y < -0.5f)
        {
            ignoreNextWalkThroughCollision = true;
        }
        else
        {
            ignoreNextWalkThroughCollision = false;
            if (!jumpHandler.TryJump()) return;

            velocity.y = jumpHandler.JumpPower;
            timeSinceJumpStart = 0.0f;
        }
    }

    private void OnJumpReleased()
    {
        jumpHandler.OnJumpingStop();
    }

    private void OnDash()
    {
        if (Math.Abs(currentDirection.x) > 0.1f && dashHandler.TryDash())
        {
            velocity.x = currentDirection.x > 0f ? dashHandler.DashPower : -dashHandler.DashPower;
            velocity.y = 0f;
            isGrounded = false;
        }
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
                if (hitResults[i].distance < 0f) continue;
                var ground = hitResults[i].transform.gameObject.GetComponent<Ground>();
                var result = ground.OnInteraction(interaction);
                if (result.stopMovement)
                {
                    float skinSize = direction.x > 0 ? -spriteRenderer.bounds.extents.x : spriteRenderer.bounds.extents.x;
                    transform.position = new Vector3(
                        hitResults[i].point.x + skinSize,
                        transform.position.y,
                        transform.position.z);
                    return true;
                }
            }
        }

        return false;
    }

    private bool HandleGroundCollision()
    {
        if (velocity.y > 0f)
            return false;
        float yOrigin = playerCollider.bounds.min.y;
        Span<Vector2> origins = stackalloc Vector2[3]{new Vector2(
            playerCollider.bounds.min.x, yOrigin
        ), new Vector2(
            playerCollider.bounds.center.x, yOrigin
        ), new Vector2(
            playerCollider.bounds.max.x , yOrigin
        )};

        var interaction = new InteractionInitiator();
        interaction.angle = InteractionInitiator.EInteractionAngle.FromAbove;
        interaction.go = this.gameObject;
        interaction.hitStrenght = 1;
        interaction.ignoreWalkThroughCollision = ignoreNextWalkThroughCollision;


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
                {
                    transform.position = new Vector3(
                        transform.position.x,
                        hitResults[i].point.y + spriteRenderer.bounds.extents.y,
                        transform.position.z);
                    return true;
                }
            }
        }

        return false;
    }

    private bool CheckVecticalCollision()
    {
        float yOrigin = playerCollider.bounds.max.y;
        Span<Vector2> origins = stackalloc Vector2[3]{new Vector2(
            playerCollider.bounds.min.x, yOrigin
        ), new Vector2(
            playerCollider.bounds.center.x, yOrigin
        ), new Vector2(
            playerCollider.bounds.max.x , yOrigin
        )};

        var interaction = new InteractionInitiator();
        interaction.angle = InteractionInitiator.EInteractionAngle.FromBeneath;
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
                {
                    return true;
                }
            }
        }

        return false;
    }
}

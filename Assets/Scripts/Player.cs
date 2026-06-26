using System;
using NUnit.Framework;
using UnityEngine;


// BUG WHEN KEEPING THE JUMP BUTTON PRESSED WHILE WALL JUMPING
public class Player : MonoBehaviour, IDynamicInteractible
{
    enum PlayerState { Idle, Walk, Run, Hanging, Rising, Falling };

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;


    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Vector2 velocity = Vector2.zero;
    private Vector2 currentDirection = new Vector2(0f, 0f);
    private bool isGrounded = false;

    private JumpHandler jumpHandler;
    private DashHandler dashHandler;
    private WallJumpHandler wallJumpHandler;
    private SpriteRenderer spriteRenderer;
    private bool ignoreNextWalkThroughCollision = false;

    void Awake()
    {
        ContactHandler.Instance.AddDynamicObject(this);
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        jumpHandler = ScriptableObject.CreateInstance<JumpHandler>();
        dashHandler = ScriptableObject.CreateInstance<DashHandler>();
        wallJumpHandler = ScriptableObject.CreateInstance<WallJumpHandler>();

        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        PlayerInputs.Instance.inGameActions.Enable();
        PlayerInputs.Instance.inGameActions.Move += OnMove;
        PlayerInputs.Instance.inGameActions.JumpPressed += OnJumpPressed;
        PlayerInputs.Instance.inGameActions.JumpRelease += OnJumpReleased;
        PlayerInputs.Instance.inGameActions.Dash += OnDash;


#if !UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!Debug.isDebugBuild)
            return;
#endif

        GameObject debugPanelGO = new GameObject("PlayerDebugPanel");
        PlayerDebugPanel debugPanel = debugPanelGO.AddComponent<PlayerDebugPanel>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = velocity;
    }

    void Update()
    {
        jumpHandler.Updated(Time.deltaTime);
        wallJumpHandler.Updated(Time.deltaTime);
        dashHandler.Updated(Time.deltaTime);
    }

    private void OnMove(Vector2 direction)
    {
        currentDirection.x = Math.Sign(direction.x);
        currentDirection.y = Math.Sign(direction.y);
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
            if (wallJumpHandler.canWallJump)
            {
                wallJumpHandler.StartWallJumping();
                velocity = wallJumpHandler.JumpPower;
                if (currentDirection.x > 0f)
                {
                    velocity.x = -velocity.x;
                }
            }
            else if (jumpHandler.TryJump())
            {
                velocity.y = jumpHandler.JumpPower;
            }
        }
    }

    private void OnJumpReleased()
    {
        wallJumpHandler.OnJumpReleased();
        jumpHandler.OnJumpReleased();
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

    public InteractionResult OnInteraction(InteractionInitiator other)
    {
        throw new NotImplementedException();
    }

    public void OnHit(int strength)
    {
        throw new NotImplementedException();
    }

    public Vector3 GetPosition() => transform.position;
    public Vector2 GetVelocity() => velocity;
    public float GetSpeed() => runSpeed;
    public Vector2 GetCurrentDirection() => currentDirection;
    public bool IsGrounded() => isGrounded;
    public JumpHandler GetJumpHandler() => jumpHandler;
    public DashHandler GetDashHandler() => dashHandler;
    public WallJumpHandler GetWallJumpHandler() => wallJumpHandler;

    public Collider2D GetCollider() => playerCollider;
    public bool GetIgnoreNextWalkThroughCollision() => ignoreNextWalkThroughCollision;

    public void SetIgnoreNextWalkThroughCollision(bool value) => ignoreNextWalkThroughCollision = value;

    public void SetIsGrounded(bool value) => isGrounded = value;
    public void SetPosition(Vector3 position) => transform.position = position;
    public void SetVelocity(Vector2 value) { velocity = value; rb.linearVelocity = velocity; }


}

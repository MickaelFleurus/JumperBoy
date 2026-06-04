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
    private WallJumpHandler wallJumpHandler;
    private SpriteRenderer spriteRenderer;
    private bool ignoreNextWalkThroughCollision = false;

    private RaycastHit2D[] hitResults = new RaycastHit2D[3];

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        jumpHandler = ScriptableObject.CreateInstance<JumpHandler>();
        dashHandler = ScriptableObject.CreateInstance<DashHandler>();
        wallJumpHandler = ScriptableObject.CreateInstance<WallJumpHandler>();

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
        wallJumpHandler.Updated(Time.deltaTime);
        dashHandler.Updated(Time.deltaTime);
    }

    void FixedUpdate()
    {
        UpdateHorizontalVelocity();
        UpdateVerticalVelocity();
        rb.linearVelocity = velocity;
    }

    private void UpdateVerticalVelocity()
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
                    jumpHandler.OnJumpStop();
                    wallJumpHandler.OnJumpStop();
                    velocity.y = 0f;
                }
                else
                {
                    float jumpingAttenuation = jumpHandler.IsJumping
                        ? jumpHandler.JumpPower
                        : wallJumpHandler.IsWallJumping
                            ? wallJumpHandler.JumpPower.y
                            : 0f;
                    velocity.y -= Math.Max((gravityScale - jumpingAttenuation) * Time.deltaTime, -10f);
                }
            }
        }
    }

    private void UpdateHorizontalVelocity()
    {
        wallJumpHandler.canWallJump = false;
        if (!dashHandler.IsDashing && !wallJumpHandler.IsWallJumping)
        {
            if (Mathf.Abs(currentDirection.x) > 0.1f)
            {
                float targetSpeed = currentDirection.x * (true ? runSpeed : walkSpeed);

                bool hasCollision = currentDirection.x > 0f ? CheckLateralCollision(Vector2.right) : CheckLateralCollision(Vector2.left);
                if (!hasCollision)
                {
                    velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, accelerationRate * Time.deltaTime);
                }
                else
                {
                    velocity.x = 0; // Stop if hitting a wall
                    wallJumpHandler.canWallJump = true;
                }
            }
            else
            {
                velocity.x = Mathf.MoveTowards(velocity.x, 0, decelerationRate * Time.deltaTime);
            }
        }
        else if (wallJumpHandler.IsWallJumping)
        {
            bool hasCollision = velocity.x > 0f ? CheckLateralCollision(Vector2.right) : CheckLateralCollision(Vector2.left);
            if (hasCollision)
            {
                velocity.x = 0f;
                wallJumpHandler.OnJumpStop();
            }
            else
            {
                float targetSpeed = velocity.x * (true ? runSpeed : walkSpeed);
                velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, decelerationRate * Time.deltaTime);
            }
        }
        else if (dashHandler.IsDashing)
        {
            bool hasCollision = currentDirection.x > 0f ? CheckLateralCollision(Vector2.right) : CheckLateralCollision(Vector2.left);
            if (hasCollision)
            {
                velocity.x = 0f;
            }
            else
            {
                velocity.x = dashHandler.DashPower;
            }
        }
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
            if (wallJumpHandler.canWallJump)
            {
                Debug.Log("WallJump!");
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

    private bool showDebugPanel = true;
    private bool showVelocitySection = true;
    private bool showJumpHandlerSection = true;
    private bool showDashHandlerSection = true;
    private bool showWallJumpHandlerSection = true;
    private Vector2 scrollPosition = Vector2.zero;

    private bool DrawFoldableSection(ref bool isOpen, string title)
    {
        string arrow = isOpen ? "▼ " : "► ";
        bool newState = GUILayout.Toggle(isOpen, arrow + title, GUI.skin.box);
        return newState;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 500));

        showDebugPanel = GUILayout.Toggle(showDebugPanel, "▼ Player Debug Panel", GUI.skin.box);

        if (showDebugPanel)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            GUILayout.Space(5);

            // Velocity Section
            showVelocitySection = DrawFoldableSection(ref showVelocitySection, "Velocity");
            if (showVelocitySection)
            {
                GUILayout.Label($"Velocity X: {velocity.x:F2}", GUILayout.Height(25));
                GUILayout.Label($"Velocity Y: {velocity.y:F2}", GUILayout.Height(25));
                GUILayout.Label($"Magnitude: {velocity.magnitude:F2}", GUILayout.Height(25));
                GUILayout.Label($"Direction: {velocity.normalized}", GUILayout.Height(25));
                GUILayout.Label($"Grounded: {isGrounded}", GUILayout.Height(25));
            }

            GUILayout.Space(10);

            // Jump Handler Section
            showJumpHandlerSection = DrawFoldableSection(ref showJumpHandlerSection, "Jump Handler");
            if (showJumpHandlerSection)
            {
                GUILayout.Label($"Is Jumping: {jumpHandler.IsJumping}", GUILayout.Height(25));

                GUILayout.BeginHorizontal();
                GUILayout.Label("Jump Power:", GUILayout.Width(120));
                float jumpPower = GUILayout.HorizontalSlider(jumpHandler.JumpPower, 1f, 15f);
                GUILayout.Label($"{jumpPower:F2}", GUILayout.Width(50));
                if (jumpPower != jumpHandler.JumpPower)
                    jumpHandler.SetJumpPower(jumpPower);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Max Jump Time:", GUILayout.Width(120));
                float maxJumpTime = GUILayout.HorizontalSlider(jumpHandler.MaxJumpTime, 0.1f, 2f);
                GUILayout.Label($"{maxJumpTime:F2}", GUILayout.Width(50));
                if (maxJumpTime != jumpHandler.MaxJumpTime)
                    jumpHandler.SetMaxJumpTime(maxJumpTime);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Jump Amount:", GUILayout.Width(120));
                int jumpAmount = (int)GUILayout.HorizontalSlider(jumpHandler.JumpAmount, 1f, 10f);
                GUILayout.Label($"{jumpAmount}", GUILayout.Width(50));
                if (jumpAmount != jumpHandler.JumpAmount)
                    jumpHandler.SetJumpAmount(jumpAmount);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Jump Cooldown:", GUILayout.Width(120));
                float jumpCooldown = GUILayout.HorizontalSlider(jumpHandler.JumpCooldown, 0.1f, 3f);
                GUILayout.Label($"{jumpCooldown:F2}", GUILayout.Width(50));
                if (jumpCooldown != jumpHandler.JumpCooldown)
                    jumpHandler.SetJumpCooldown(jumpCooldown);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            // Dash Handler Section
            showDashHandlerSection = DrawFoldableSection(ref showDashHandlerSection, "Dash Handler");
            if (showDashHandlerSection)
            {
                GUILayout.Label($"Is Dashing: {dashHandler.IsDashing}", GUILayout.Height(25));

                GUILayout.BeginHorizontal();
                GUILayout.Label("Dash Power:", GUILayout.Width(120));
                float dashPower = GUILayout.HorizontalSlider(dashHandler.DashPower, 5f, 30f);
                GUILayout.Label($"{dashPower:F2}", GUILayout.Width(50));
                if (dashPower != dashHandler.DashPower)
                    dashHandler.SetDashPower(dashPower);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Dash Amount:", GUILayout.Width(120));
                int dashAmount = (int)GUILayout.HorizontalSlider(dashHandler.DashAmount, 1f, 10f);
                GUILayout.Label($"{dashAmount}", GUILayout.Width(50));
                if (dashAmount != dashHandler.DashAmount)
                    dashHandler.SetDashAmount(dashAmount);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Dash Duration:", GUILayout.Width(120));
                float dashDuration = GUILayout.HorizontalSlider(dashHandler.DashDuration, 0.1f, 1f);
                GUILayout.Label($"{dashDuration:F2}", GUILayout.Width(50));
                if (dashDuration != dashHandler.DashDuration)
                    dashHandler.SetDashDuration(dashDuration);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Dash Cooldown:", GUILayout.Width(120));
                float dashCooldown = GUILayout.HorizontalSlider(dashHandler.DashCooldown, 0.1f, 3f);
                GUILayout.Label($"{dashCooldown:F2}", GUILayout.Width(50));
                if (dashCooldown != dashHandler.DashCooldown)
                    dashHandler.SetDashCooldown(dashCooldown);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            // Wall Jump Handler Section
            showWallJumpHandlerSection = DrawFoldableSection(ref showWallJumpHandlerSection, "Wall Jump Handler");
            if (showWallJumpHandlerSection)
            {
                GUILayout.Label($"Is Wall Jumping: {wallJumpHandler.IsWallJumping}", GUILayout.Height(25));

                GUILayout.BeginHorizontal();
                GUILayout.Label("Jump Power X:", GUILayout.Width(120));
                Vector2 jumpPowerWall = wallJumpHandler.JumpPower;
                float jumpPowerX = GUILayout.HorizontalSlider(jumpPowerWall.x, 0f, 10f);
                GUILayout.Label($"{jumpPowerX:F2}", GUILayout.Width(50));
                if (jumpPowerX != jumpPowerWall.x)
                    wallJumpHandler.SetJumpPower(new Vector2(jumpPowerX, jumpPowerWall.y));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Jump Power Y:", GUILayout.Width(120));
                float jumpPowerY = GUILayout.HorizontalSlider(jumpPowerWall.y, 0f, 15f);
                GUILayout.Label($"{jumpPowerY:F2}", GUILayout.Width(50));
                if (jumpPowerY != jumpPowerWall.y)
                    wallJumpHandler.SetJumpPower(new Vector2(jumpPowerWall.x, jumpPowerY));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Extension Duration:", GUILayout.Width(120));
                float extDuration = GUILayout.HorizontalSlider(wallJumpHandler.JumpExtensionDuration, 0.1f, 1f);
                GUILayout.Label($"{extDuration:F2}", GUILayout.Width(50));
                if (extDuration != wallJumpHandler.JumpExtensionDuration)
                    wallJumpHandler.SetJumpExtensionDuration(extDuration);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Mandatory Duration:", GUILayout.Width(120));
                float mandDuration = GUILayout.HorizontalSlider(wallJumpHandler.JumpMandatoryDuration, 0.1f, 2f);
                GUILayout.Label($"{mandDuration:F2}", GUILayout.Width(50));
                if (mandDuration != wallJumpHandler.JumpMandatoryDuration)
                    wallJumpHandler.SetJumpMandatoryDuration(mandDuration);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        GUILayout.EndArea();
    }
}

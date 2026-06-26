using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContactHandler : MonoBehaviour
{
    private static ContactHandler instance;
    public static ContactHandler Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("ContactHandler");
                instance = obj.AddComponent<ContactHandler>();
            }
            return instance;
        }
    }

    private float accelerationRate = 50f;
    private float decelerationRate = 30f;
    private float groundCheckDistance = 0.2f;
    private float lateralCheckDistance = 0.25f;
    private float gravityScale = 9.8f;
    private RaycastHit2D[] hitResults = new RaycastHit2D[3];

    private readonly List<IDynamicInteractible> dynamicInteractibles = new();

    public void AddDynamicObject(IDynamicInteractible dynamic)
    {
        dynamicInteractibles.Add(dynamic);
    }

    void FixedUpdate()
    {
        foreach (var dynamic in dynamicInteractibles)
        {
            UpdateHorizontalVelocity(dynamic);
            UpdateVerticalVelocity(dynamic);
        }
    }

    private bool HandleGroundCollision(IDynamicInteractible dynamic)
    {
        Vector3 position = dynamic.GetPosition();
        Bounds bounds = dynamic.GetCollider().bounds;

        float yOrigin = bounds.min.y;
        Span<Vector2> origins = stackalloc Vector2[3]{new Vector2(bounds.min.x, yOrigin
        ), new Vector2(bounds.center.x, yOrigin
        ), new Vector2(bounds.max.x , yOrigin
        )};

        var interaction = new InteractionInitiator();
        interaction.angle = InteractionInitiator.EInteractionAngle.FromAbove;
        interaction.go = this.gameObject;
        interaction.hitStrenght = 1;
        interaction.ignoreWalkThroughCollision = dynamic.GetIgnoreNextWalkThroughCollision();


        LayerMask groundLayer = LayerMask.GetMask("Ground");
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



            }
        }

        return false;
    }

    private void UpdateVerticalVelocity(IDynamicInteractible dynamic)
    {
        Vector2 velocity = dynamic.GetVelocity();
        DashHandler dashHandler = dynamic.GetDashHandler();
        JumpHandler jumpHandler = dynamic.GetJumpHandler();
        WallJumpHandler wallJumpHandler = dynamic.GetWallJumpHandler();
        if (dashHandler == null || !dashHandler.IsDashing)
        {
            if (velocity.y <= 0f)
            {
                bool isNowGrounded = HandleGroundCollision(dynamic);

                if (!dynamic.IsGrounded() && isNowGrounded)
                {
                    velocity.y = 0.0f;
                    dynamic.SetIgnoreNextWalkThroughCollision(false);
                    jumpHandler?.OnJumpReset();
                    dashHandler?.OnDashReset();
                }
                else if (!isNowGrounded)
                {
                    velocity.y -= Math.Max(gravityScale * Time.fixedDeltaTime, -10f);
                }
                dynamic.SetIsGrounded(isNowGrounded);
            }
            else  // velocity.y > 0f
            {
                dynamic.SetIsGrounded(false);
                if (CheckVecticalCollision(dynamic)) // Stop ascending
                {
                    jumpHandler?.OnJumpStop();
                    wallJumpHandler?.OnJumpStop();
                    velocity.y = 0f;
                }
                else
                {
                    float jumpingAttenuation = jumpHandler && jumpHandler.IsJumping
                        ? jumpHandler.JumpPower
                        : wallJumpHandler && wallJumpHandler.IsWallJumping
                            ? wallJumpHandler.JumpPower.y
                            : 0f;
                    velocity.y -= Math.Max((gravityScale - jumpingAttenuation) * Time.deltaTime, -10f);
                }
            }
            dynamic.SetVelocity(velocity);
        }
    }


    private bool CheckVecticalCollision(IDynamicInteractible dynamic)
    {
        Bounds bounds = dynamic.GetCollider().bounds;
        float yOrigin = bounds.max.y;
        Span<Vector2> origins = stackalloc Vector2[3]{new Vector2(
            bounds.min.x, yOrigin
        ), new Vector2(bounds.center.x, yOrigin
        ), new Vector2(bounds.max.x , yOrigin
        )};

        var interaction = new InteractionInitiator();
        interaction.angle = InteractionInitiator.EInteractionAngle.FromBeneath;
        interaction.go = this.gameObject;
        interaction.hitStrenght = 1;

        LayerMask groundLayer = LayerMask.GetMask("Ground");
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
                var ground = hitResults[i].transform.gameObject.GetComponent<IInteractible>();
                var result = ground.OnInteraction(interaction);
                if (result.stopMovement)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void UpdateHorizontalVelocity(IDynamicInteractible dynamic)
    {
        Vector2 velocity = dynamic.GetVelocity();
        Vector2 currentDirection = dynamic.GetCurrentDirection();
        float speed = dynamic.GetSpeed();
        DashHandler dashHandler = dynamic.GetDashHandler();
        WallJumpHandler wallJumpHandler = dynamic.GetWallJumpHandler();

        wallJumpHandler.canWallJump = false;

        if (!dashHandler.IsDashing && !wallJumpHandler.IsWallJumping)
        {
            if (Mathf.Abs(velocity.x) > 0.1f || Mathf.Abs(currentDirection.x) > 0.1f)
            {
                float targetSpeed = currentDirection.x * speed;

                float checkValue = Mathf.Abs(velocity.x) > 0.01f ? velocity.x : currentDirection.x;
                bool hasCollision = checkValue > 0f ? CheckLateralCollision(Vector2.right, dynamic) : CheckLateralCollision(Vector2.left, dynamic);
                if (!hasCollision)
                {
                    velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, accelerationRate * Time.deltaTime);
                }
                else
                {
                    velocity.x = 0;
                    wallJumpHandler.canWallJump = !dynamic.IsGrounded();
                }
            }
            else
            {
                velocity.x = Mathf.MoveTowards(velocity.x, 0, decelerationRate * Time.deltaTime);
            }
        }
        else if (wallJumpHandler.IsWallJumping)
        {
            bool hasCollision = velocity.x > 0f ? CheckLateralCollision(Vector2.right, dynamic) : CheckLateralCollision(Vector2.left, dynamic);
            if (hasCollision)
            {
                velocity.x = 0f;
                wallJumpHandler.OnJumpStop();
            }
            else
            {
                float targetSpeed = speed;
                targetSpeed *= Math.Sign(velocity.x);
                velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, decelerationRate * Time.deltaTime);
            }
        }
        else if (dashHandler.IsDashing)
        {
            bool hasCollision = velocity.x > 0f ? CheckLateralCollision(Vector2.right, dynamic) : CheckLateralCollision(Vector2.left, dynamic);
            if (hasCollision)
            {
                velocity.x = 0f;
            }
            else
            {
                velocity.x = dashHandler.DashPower * Math.Sign(velocity.x);
            }
        }
        dynamic.SetVelocity(velocity);
    }

    private bool CheckLateralCollision(Vector2 direction, IDynamicInteractible dynamic)
    {
        Bounds bounds = dynamic.GetCollider().bounds;
        Vector3 position = dynamic.GetPosition();

        float xOrigin = direction.x > 0 ? bounds.max.x : bounds.min.x;
        Span<Vector2> origins = stackalloc Vector2[3]{new Vector2(
            xOrigin, bounds.min.y + 0.1f
        ), new Vector2(
            xOrigin, bounds.center.y
        ), new Vector2(
            xOrigin, bounds.max.y - 0.1f
        )};

        var interaction = new InteractionInitiator();
        interaction.angle = InteractionInitiator.EInteractionAngle.Sideway;
        interaction.go = this.gameObject;
        interaction.hitStrenght = 1;

        LayerMask groundLayer = LayerMask.GetMask("Ground");
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
                var ground = hitResults[i].transform.gameObject.GetComponent<IInteractible>();
                if (ground == null) continue;

                var result = ground.OnInteraction(interaction);
                if (result.stopMovement)
                {
                    float skinSize = direction.x > 0 ? -bounds.extents.x : bounds.extents.x;
                    position.x =
                        hitResults[i].point.x + skinSize;
                    dynamic.SetPosition(position);
                    return true;
                }
            }
        }

        return false;
    }
}

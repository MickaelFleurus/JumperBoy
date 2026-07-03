
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public interface IDynamicInteractible
{
    public bool IsGrounded();

    public float GetSpeed();
    public Vector2 GetVelocity();
    public Vector2 GetCurrentDirection();
    public JumpHandler GetJumpHandler();
    public DashHandler GetDashHandler();
    public WallJumpHandler GetWallJumpHandler();
    public Collider2D GetCollider();
    public bool GetIgnoreNextWalkThroughCollision();

    public void SetIgnoreNextWalkThroughCollision(bool value);

    public void SetIsGrounded(bool value);
    public void SetPosition(Vector3 position);
    public void SetVelocity(Vector2 velocity);
}

using UnityEngine;
public struct CollisionResult
{
    public Vector2 AllowedMovement;

    public bool Grounded;

    public bool HitWall;

    public Vector2 GroundNormal;

    public SurfaceProperties Surface;
}

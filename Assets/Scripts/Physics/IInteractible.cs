using UnityEngine;
public struct InteractionInitiator
{
    public enum EInteractionAngle { FromBeneath, FromAbove, Sideway };
    public bool ignoreWalkThroughCollision;
    public EInteractionAngle angle;
    public int hitStrenght;
    public GameObject go;
    public string tag;
    public int layer;
}
public struct InteractionResult
{
    public bool stopMovement;
    public float yPosition;
    public Environment.EGroundType groundType;
    public bool kill;
}

public interface IInteractible
{
    public Vector3 GetPosition();
}

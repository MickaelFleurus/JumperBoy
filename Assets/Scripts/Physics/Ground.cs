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
    public Ground.EGroundType groundType;

}

public class Ground : MonoBehaviour
{
    public enum EWallType { Slippy, Normal };
    public enum EGroundType { Solid, WalkThrough, Breakable };
    [SerializeField] EGroundType groundType = EGroundType.Solid;
    [SerializeField] EWallType wallType = EWallType.Normal;
    [SerializeField] int strength = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnHit(int strength)
    {
        this.strength -= strength;
        if (this.strength <= 0)
        {
            Destroy(gameObject);
        }
    }

    public InteractionResult OnInteraction(InteractionInitiator other)
    {
        InteractionResult result = new InteractionResult();
        if (other.go.GetComponent<Player>())
        {
            if (other.angle == InteractionInitiator.EInteractionAngle.FromAbove)
            {
                if (other.ignoreWalkThroughCollision && groundType == EGroundType.WalkThrough)
                {
                    result.stopMovement = false;
                }
                else
                {
                    result.stopMovement = true;
                }
            }
            else if (other.angle == InteractionInitiator.EInteractionAngle.Sideway)
            {
                result.stopMovement = this.groundType != EGroundType.WalkThrough;
            }
            else if (other.angle == InteractionInitiator.EInteractionAngle.FromBeneath)
            {
                result.stopMovement = this.groundType != EGroundType.WalkThrough;
                if (this.groundType == EGroundType.Breakable)
                {
                    OnHit(other.hitStrenght);
                }
            }
        }
        return result;
    }
}

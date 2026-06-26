using UnityEngine;

public class Environment : MonoBehaviour, IInteractible
{
    public enum EWallType { Slippy, Normal };
    public enum EGroundType { Solid, WalkThrough, Breakable };

    [SerializeField] EGroundType groundType = EGroundType.Solid;
    [SerializeField] EWallType wallType = EWallType.Normal;
    [SerializeField] int strength = 1;

    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;


    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();


        boxCollider = gameObject.AddComponent<BoxCollider2D>();
        boxCollider.size = spriteRenderer.size;
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

    public Vector3 GetPosition()
    {
        throw new System.NotImplementedException();
    }
}

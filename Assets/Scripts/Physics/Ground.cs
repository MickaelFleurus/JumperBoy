using UnityEngine;

public struct InteractionInitiator
{
    public enum EInteractionAngle { FromBeneath, FromAbove, Sideway };
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
}

public class Ground : MonoBehaviour
{
    public enum EGroundType { Solid, WalkThrough, Breakable };
    [SerializeField] EGroundType groundType = EGroundType.Solid;
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
            Debug.Log("DESTROY!");
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
                result.stopMovement = true;
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

using UnityEngine;

public readonly struct CollisionContext
{
    public readonly Component Sender;

    // Direction the sender was moving.
    public readonly Vector2 Direction;

    // Surface normal.
    public readonly Vector2 Normal;

    public readonly float Speed;

    public CollisionContext(
        Component sender,
        Vector2 direction,
        Vector2 normal,
        float speed)
    {
        Sender = sender;
        Direction = direction;
        Normal = normal;
        Speed = speed;
    }
}

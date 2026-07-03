using UnityEngine;

public readonly struct DamageContext
{
    public readonly Component Sender;

    public readonly float Damage;

    public readonly Vector2 HitDirection;

    public readonly int Strength;

    public DamageContext(
        Component sender,
        float damage,
        Vector2 hitDirection,
        int strength)
    {
        Sender = sender;
        Damage = damage;
        HitDirection = hitDirection;
        Strength = strength;
    }
}

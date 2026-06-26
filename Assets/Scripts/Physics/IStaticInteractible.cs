
using UnityEngine;


public interface IStaticInteractible : IInteractible
{
    public bool IsSolid();
    public bool CanJumpFrom();
    public float Friction();

    public void OnImpact() { }
}

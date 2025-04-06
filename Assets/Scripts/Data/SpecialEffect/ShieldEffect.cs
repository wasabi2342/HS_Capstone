using UnityEngine;

[CreateAssetMenu(fileName = "ShieldEffect", menuName = "Scriptable Objects/ShieldEffect")]
public class ShieldEffect : BaseSpecialEffect
{
    public override void ApplyEffect()
    {
        playerController.AddShield(value, duration);
    }

    public override void Init(float value, float duration, ParentPlayerController playerController)
    {
        base.Init(value, duration, playerController);

        isInstant = true;
    }
}

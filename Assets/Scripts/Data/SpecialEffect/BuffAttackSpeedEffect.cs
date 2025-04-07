using UnityEngine;

[CreateAssetMenu(fileName = "BuffAttackSpeedEffect", menuName = "Scriptable Objects/BuffAttackSpeedEffect")]
public class BuffAttackSpeedEffect : BaseSpecialEffect
{
    public override void ApplyEffect()
    {
        playerController.BuffAttackSpeed(value, duration);
    }

    public override void Init(float value, float duration, ParentPlayerController playerController)
    {
        base.Init(value, duration, playerController);
        
        isInstant = true;
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "BuffMoveSpeed", menuName = "Scriptable Objects/BuffMoveSpeed")]
public class BuffMoveSpeed : BaseSpecialEffect
{
    public override void ApplyEffect()
    {
        Debug.Log("이동속도 버프 가호");
        playerController.BuffMoveSpeed(value, duration);
    }

    public override void Init(float value, float duration, ParentPlayerController playerController)
    {
        base.Init(value, duration, playerController);

        isInstant = true;
    }
}

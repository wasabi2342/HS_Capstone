using UnityEngine;

[CreateAssetMenu(fileName = "BindingEffect", menuName = "Scriptable Objects/BindingEffect")]
public class BindingEffect : BaseSpecialEffect
{
    public override void ApplyEffect()
    {
        // 속박 시키는 코드 추가하기
        Debug.Log("적 속박");
    }

    public override void Init(float value, float duration, ParentPlayerController playerController)
    {
        base.Init(value, duration, playerController);

        isInstant = false;
    }
}

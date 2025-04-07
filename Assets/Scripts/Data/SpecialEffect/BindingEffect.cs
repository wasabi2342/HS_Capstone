using UnityEngine;

[CreateAssetMenu(fileName = "BindingEffect", menuName = "Scriptable Objects/BindingEffect")]
public class BindingEffect : BaseSpecialEffect
{
    public override void ApplyEffect()
    {
        // �ӹ� ��Ű�� �ڵ� �߰��ϱ�
        Debug.Log("�� �ӹ�");
    }

    public override void Init(float value, float duration, ParentPlayerController playerController)
    {
        base.Init(value, duration, playerController);

        isInstant = false;
    }
}

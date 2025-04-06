using UnityEngine;

[CreateAssetMenu(fileName = "SlowEffect", menuName = "Scriptable Objects/SlowEffect")]
public class SlowEffect : BaseSpecialEffect
{
    public override void ApplyEffect()
    {
        // �� ���ο� �ڵ� �߰�
        Debug.Log("�� ���ο�");
    }

    public override void Init(float value, float duration, ParentPlayerController playerController)
    {
        base.Init(value, duration, playerController);

        isInstant = false;
    }
}

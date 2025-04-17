using UnityEngine;

[CreateAssetMenu(fileName = "DOTDamage", menuName = "Scriptable Objects/DOTDamage")]
public class DOTDamage : BaseSpecialEffect
{
    public override void ApplyEffect()
    {
        // ��Ʈ������ �ִ� ��� �߰��ϱ�
        Debug.Log("�� ��Ʈ������");
        collider.GetComponent<DebuffController>().ApplyDebuff(SpecialEffectType.Dot, duration, value);
    }

    public override void Init(float value, float duration, ParentPlayerController playerController)
    {
        base.Init(value, duration, playerController);

        isInstant = false;
    }
}

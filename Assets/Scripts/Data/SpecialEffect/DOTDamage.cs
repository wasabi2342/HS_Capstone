using UnityEngine;

[CreateAssetMenu(fileName = "DOTDamage", menuName = "Scriptable Objects/DOTDamage")]
public class DOTDamage : BaseSpecialEffect
{
    public override void ApplyEffect()
    {
        // 도트데미지 주는 기능 추가하기
        Debug.Log("적 도트데미지");
        collider.GetComponent<DebuffController>().ApplyDebuff(SpecialEffectType.Dot, duration, value);
    }

    public override void Init(float value, float duration, ParentPlayerController playerController)
    {
        base.Init(value, duration, playerController);

        isInstant = false;
    }
}

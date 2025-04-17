using UnityEngine;

[CreateAssetMenu(fileName = "SlowEffect", menuName = "Scriptable Objects/SlowEffect")]
public class SlowEffect : BaseSpecialEffect
{
    public override void ApplyEffect()
    {
        // 적 슬로우 코드 추가
        Debug.Log("적 슬로우");
        collider.GetComponent<DebuffController>().ApplyDebuff(SpecialEffectType.Slow, duration, value);

    }

    public override void Init(float value, float duration, ParentPlayerController playerController)
    {
        base.Init(value, duration, playerController);

        isInstant = false;
    }
}

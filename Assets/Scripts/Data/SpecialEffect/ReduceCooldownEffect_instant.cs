using UnityEngine;

[CreateAssetMenu(fileName = "ReduceCooldownEffect_instant", menuName = "Scriptable Objects/ReduceCooldownEffect_instant")]
public class ReduceCooldownEffect_instant : BaseSpecialEffect
{
    public override void ApplyEffect()
    {
        int intKey = (int)duration;
        string strKey = intKey.ToString();

        foreach (char c in strKey)
        {
            if (char.IsDigit(c))
            {
                playerController.cooldownCheckers[int.Parse(c.ToString())].ReduceCooldown(value);
                Debug.Log((Skills)int.Parse(c.ToString()) + "스킬" + value + "만큼 쿨타임 감소");
            }
        }
    }

    public override void Init(float value, float duration, ParentPlayerController playerController)
    {
        base.Init(value, duration, playerController);
        isInstant = true;
    }
}

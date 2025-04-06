using UnityEngine;

[CreateAssetMenu(fileName = "ReduceCooldownEffect", menuName = "Scriptable Objects/ReduceCooldownEffect")]
public class ReduceCooldownEffect : BaseSpecialEffect
{
    public override void ApplyEffect()
    {
        if (!isFirstTime)
        {
            return;
        }

        int intKey = (int)duration;
        string strKey = intKey.ToString();

        foreach (char c in strKey)
        {
            if (char.IsDigit(c))
                playerController.cooldownCheckers[int.Parse(c.ToString())].ReduceCooldown(value);
        }

        isFirstTime = false;
    }

    public override void Init(float value, float duration, ParentPlayerController playerController)
    {
        base.Init(value, duration, playerController);

        isFirstTime = true;
        isInstant = false;
    }
}

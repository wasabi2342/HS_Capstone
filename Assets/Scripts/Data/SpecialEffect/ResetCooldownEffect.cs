using UnityEngine;

[CreateAssetMenu(fileName = "ResetCooldownEffect", menuName = "Scriptable Objects/ResetCooldownEffect")]
public class ResetCooldownEffect : BaseSpecialEffect
{
    public override void ApplyEffect()
    {
        int intKey = (int)duration;
        string strKey = intKey.ToString();

        foreach (char c in strKey)
        {
            if (char.IsDigit(c))
                playerController.cooldownCheckers[int.Parse(c.ToString())].ResetCooldown(playerController);
        }
    }

    public override void Init(float value, float duration, ParentPlayerController playerController)
    {
        base.Init(value, duration, playerController);

        isInstant = true;
    }
}

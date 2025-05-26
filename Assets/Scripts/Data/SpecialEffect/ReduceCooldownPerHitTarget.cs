using UnityEngine;

[CreateAssetMenu(fileName = "ReduceCooldownPerHitTarget", menuName = "Scriptable Objects/ReduceCooldownPerHitTarget")]
public class ReduceCooldownPerHitTarget : BaseSpecialEffect
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
                Debug.Log($"{int.Parse(c.ToString())}≥l≈∏¿”∞®º“");
            }
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

using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    protected Transform target;
    protected float damage;
    private float maxCooldownTime;
    private float currentCooldownTime = 0f;
    private bool isSkillAvailable = true;

    public void Setup(Transform target, float damage, float cooldownTime)
    {
        this.target = target;
        this.damage = damage;
        maxCooldownTime = cooldownTime;
    }

    private void Update()
    {
        if (!isSkillAvailable && Time.time - currentCooldownTime > maxCooldownTime)
        {
            isSkillAvailable = true;
        }
    }

    public void TryAttack()
    {
        if (isSkillAvailable)
        {
            OnAttack();
            isSkillAvailable = false;
            currentCooldownTime = Time.time;
        }
    }

    public abstract void OnAttack();
}

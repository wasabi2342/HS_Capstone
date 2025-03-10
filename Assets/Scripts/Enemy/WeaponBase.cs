using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField]
    protected GameObject projectilePrefab;
    [SerializeField]
    protected Transform projectileSpawnPoint;

    protected Transform target;
    protected float damage;
    private float maxCooldownTime;
    private float currentCooldownTime = 0f;
    private bool isSkillAvailable = true;

    public void Setup(Transform target, float damage, float cooldowmTime)
    {
        this.target = target;
        this.damage = damage;
        maxCooldownTime = cooldowmTime;
    }

    private void Update()
    {
        if (isSkillAvailable == false && Time.time - currentCooldownTime > maxCooldownTime)
        {
            isSkillAvailable = true;
        }
    }

    public void TryAttack()
    {
        if (isSkillAvailable == true)
        {
            OnAttack();
            isSkillAvailable = false;
            currentCooldownTime = Time.time;
        }
    }

    public abstract void OnAttack();
}

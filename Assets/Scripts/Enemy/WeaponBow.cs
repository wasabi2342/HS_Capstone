using UnityEngine;

public class WeaponBow : WeaponBase
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    public override void OnAttack()
    {
        if (projectilePrefab != null && projectileSpawnPoint != null && target != null)
        {
            GameObject clone = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            clone.GetComponent<EnemyProjectile>()?.Setup(target, damage);
        }
    }
}

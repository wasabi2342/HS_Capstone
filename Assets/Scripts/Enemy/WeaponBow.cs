using UnityEngine;

public class WeaponBow : WeaponBase
{
    public override void OnAttack()
    {
        GameObject clone = GameObject.Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        clone.GetComponent<EnemyProjectile>().Setup(target, damage);
    }
}

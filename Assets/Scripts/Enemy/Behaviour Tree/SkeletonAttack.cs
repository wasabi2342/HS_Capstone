using UnityEngine;

public class SkeletonAttack : MonoBehaviour, IMonsterAttack
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 15f;

    public void Attack(Transform target)
    {
        if (target != null && firePoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();

            Vector3 direction = new Vector3(target.position.x - firePoint.position.x, 0, target.position.z - firePoint.position.z).normalized;
            rb.linearVelocity = direction * projectileSpeed;
        }
    }
}

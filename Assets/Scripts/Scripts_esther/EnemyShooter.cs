using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("총알 발사 설정")]
    public GameObject bulletPrefab;  // BulletController가 붙은 총알 프리팹
    public Transform muzzle;         // 총구 역할을 하는 자식 Transform
    public float fireRate = 1f;        // 발사 간격 (초)

    private float fireTimer = 0f;

    void Update()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate)
        {
            FireBullet();
            fireTimer = 0f;
        }
    }

    void FireBullet()
    {
        if (bulletPrefab != null && muzzle != null)
        {
            Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);
        }
    }
}

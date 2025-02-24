using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject bulletPrefab;  // HomingBulletController가 붙은 프리팹
    public Transform muzzle;         // 총구
    public float fireRate = 1f;

    private float fireTimer = 0f;
    public Transform playerTransform; // 플레이어 Transform (Inspector에서 할당)

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
        if (bulletPrefab != null && muzzle != null && playerTransform != null)
        {
            // 총알 생성
            GameObject bulletObj = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);

            // 총알의 HomingBulletController.target에 플레이어 Transform 연결
            HomingBulletController bulletCtrl = bulletObj.GetComponent<HomingBulletController>();
            if (bulletCtrl != null)
            {
                bulletCtrl.target = playerTransform;
            }
        }
    }
}

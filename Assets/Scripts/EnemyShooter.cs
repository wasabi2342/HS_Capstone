using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("�Ѿ� �߻� ����")]
    public GameObject bulletPrefab;  // BulletController�� ���� �Ѿ� ������
    public Transform muzzle;         // �ѱ� ������ �ϴ� �ڽ� Transform
    public float fireRate = 1f;        // �߻� ���� (��)

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

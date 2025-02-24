using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject bulletPrefab;  // HomingBulletController�� ���� ������
    public Transform muzzle;         // �ѱ�
    public float fireRate = 1f;

    private float fireTimer = 0f;
    public Transform playerTransform; // �÷��̾� Transform (Inspector���� �Ҵ�)

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
            // �Ѿ� ����
            GameObject bulletObj = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);

            // �Ѿ��� HomingBulletController.target�� �÷��̾� Transform ����
            HomingBulletController bulletCtrl = bulletObj.GetComponent<HomingBulletController>();
            if (bulletCtrl != null)
            {
                bulletCtrl.target = playerTransform;
            }
        }
    }
}

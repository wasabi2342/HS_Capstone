using UnityEngine;

public class Weapon : MonoBehaviour
{
    private Collider weaponCollider;
    private int currentDamage;

    void Start()
    {
        weaponCollider = GetComponent<Collider>();
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
    }

    public void ActivateCollider(int damage)
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
            currentDamage = damage;
        }
    }

    public void DeactivateCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
            currentDamage = 0;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (currentDamage > 0 && other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponentInParent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(currentDamage);
                Debug.Log($"[���� ����] ������ {currentDamage}�� �������� �������ϴ�. ���� ü��: {enemy.GetCurrentHealth()}");
                currentDamage = 0;
            }
        }
    }
}

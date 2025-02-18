using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("�� �ִ� ü��")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("���� ���� ���� ����")]
    public float attackRange = 2f;        // �÷��̾� ����/���� ����
    public int meleeDamage = 10;          // ���� ���� ������
    public LayerMask playerLayerMask;     // �÷��̾ ���� ���̾� (ex: "Player")

    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log($"�� ������. ���� ü��: {currentHealth}/{maxHealth}");
    }

    void Update()
    {
        // ����) �� �����Ӹ��� OverlapSphere�� �÷��̾� ���� �� ����
        CheckPlayerInRange();
    }

    private void CheckPlayerInRange()
    {
        // ���� ��ġ�� �������� attackRange ���� �ȿ� �÷��̾ �ִ��� �˻�
        Collider[] cols = Physics.OverlapSphere(transform.position, attackRange, playerLayerMask);
        if (cols.Length > 0)
        {
            // ù ��° �浹ü�� �÷��̾��� ����
            PlayerController player = cols[0].GetComponent<PlayerController>();
            if (player != null)
            {
                // ���� ���� ������
                Debug.Log("���� ���� ���� �÷��̾�� �������� �ݴϴ�.");
                player.TakeDamage(meleeDamage);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log($"���� ���� ü��: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    void Die()
    {
        Debug.Log("���� ����߽��ϴ�.");
        Destroy(gameObject);
    }
}

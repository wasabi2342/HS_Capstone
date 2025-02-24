using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("적 최대 체력")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("근접 공격 범위 설정")]
    public float attackRange = 2f;        // 플레이어 감지/공격 범위
    public int meleeDamage = 10;          // 근접 공격 데미지
    public LayerMask playerLayerMask;     // 플레이어가 속한 레이어 (ex: "Player")

    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log($"적 생성됨. 현재 체력: {currentHealth}/{maxHealth}");
    }

    void Update()
    {
        // 예시) 매 프레임마다 OverlapSphere로 플레이어 감지 후 공격
        CheckPlayerInRange();
    }

    private void CheckPlayerInRange()
    {
        // 적의 위치를 기준으로 attackRange 범위 안에 플레이어가 있는지 검사
        Collider[] cols = Physics.OverlapSphere(transform.position, attackRange, playerLayerMask);
        if (cols.Length > 0)
        {
            // 첫 번째 충돌체가 플레이어라고 가정
            PlayerController player = cols[0].GetComponent<PlayerController>();
            if (player != null)
            {
                // 근접 공격 데미지
                Debug.Log("근접 범위 안의 플레이어에게 데미지를 줍니다.");
                player.TakeDamage(meleeDamage);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log($"적의 남은 체력: {currentHealth}");

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
        Debug.Log("적이 사망했습니다.");
        Destroy(gameObject);
    }
}

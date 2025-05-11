using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ScarecrowAttackCollider : MonoBehaviour
{
    [SerializeField] float damage = 0f;
    [SerializeField] AttackerType attackerType = AttackerType.Enemy;

    /* ---------- 데미지 판정 ---------- */
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        other.GetComponent<IDamageable>()
             ?.TakeDamage(damage, transform.position, attackerType);
    }
}

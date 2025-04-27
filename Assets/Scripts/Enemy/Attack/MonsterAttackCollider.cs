using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class MonsterAttackCollider : MonoBehaviourPun
{   
    private EnemyAI enemyAI;
    private Vector3 defaultCenter;    // 기본 Center 저장

    private void Awake()
    {
        if (enemyAI == null)
            enemyAI = GetComponentInParent<EnemyAI>();
    }

    // ─────────────────────────────
    // 충돌 판정
    // ─────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable") || other.gameObject.name.Contains("Interactable"))
            return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null && enemyAI != null)
        {
            damageable.TakeDamage(enemyAI.status.damage);
        }
    }
}

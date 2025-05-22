using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class MonsterAttackCollider : MonoBehaviourPun
{
    private EnemyFSM fsm;
    private Vector3 defaultCenter;    // 기본 Center 저장

    private void Awake()
    {
        if (fsm == null)
            fsm = GetComponentInParent<EnemyFSM>();
    }

    // ─────────────────────────────
    // 충돌 판정
    // ─────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable") || other.gameObject.name.Contains("Interactable"))
            return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null && fsm != null)
        {
            damageable.TakeDamage(fsm.EnemyStatusRef.attackDamage, fsm.transform.position);
        }
    }
}

using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class MonsterAttackCollider : MonoBehaviourPun
{
    // 부모 객체에서 EnemyAI를 찾습니다.
    private EnemyAI enemyAI;

    private void Awake()
    {
        enemyAI = GetComponentInParent<EnemyAI>();
        if (enemyAI == null)
        {
            Debug.LogError("MonsterAttackCollider: 부모 객체에서 EnemyAI를 찾을 수 없습니다.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("늑대가 니 샛길 공격함");

        // 충돌한 오브젝트가 IDamageable 인터페이스를 구현하고 있으면 데미지 적용
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // enemyAI.status.damage 값을 사용하여 데미지 전달
            damageable.TakeDamage(enemyAI.status.damage);
        }
    }
}

using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class MonsterAttackCollider : MonoBehaviourPun
{
    // 부모 객체의 EnemyAI 컴포넌트에서 enemyStatus의 damage 값을 사용합니다.
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
        // 충돌한 객체가 Player 태그인 경우
        if (other.CompareTag("Player"))
        {
            PhotonView targetPV = other.GetComponent<PhotonView>();
            if (targetPV != null && enemyAI != null)
            {
                // EnemyStatus에 정의된 damage 값을 사용합니다.
                float damage = enemyAI.status.damage;
                // 플레이어 측의 IDamageable 인터페이스를 구현한 DamageToMaster RPC를 호출합니다.
                targetPV.RPC("DamageToMaster", RpcTarget.MasterClient, damage);
            }
        }
    }
}

using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class MonsterAttackCollider : MonoBehaviourPun
{
    // 부모 객체에서 EnemyAI를 찾아 enemyStatus를 통해 공격 데미지를 가져온다.
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
        if (other.CompareTag("Player"))
        {
            PhotonView targetView = other.GetComponent<PhotonView>();
            if (targetView != null && enemyAI != null)
            {
                // enemyStatus에서 데미지 값을 가져옴 (enemyStatus에 damage 필드가 있다고 가정)
                float damage = enemyAI.status.damage;
                // 플레이어에게 Master Client가 데미지를 적용하도록 RPC 호출
                targetView.RPC("DamageToMaster", RpcTarget.MasterClient, damage);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class EnemyFSM : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyStatus enemyStatus; // 몬스터의 스탯 참조

    private Transform player;
    private NavMeshAgent navMeshAgent;
    private int currentHP;

    private void Start()
    {
        if (enemyStatus == null)
        {
            Debug.LogError("EnemyStatus가 할당되지 않았습니다!");
            return;
        }

        currentHP = enemyStatus.hp; // 몬스터 체력 초기화
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent 컴포넌트가 없습니다.");
        }
    }

    public void Setup(Transform player, GameObject[] wayPoints)
    {
        this.player = player;

        if (navMeshAgent == null) return;

        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        navMeshAgent.angularSpeed = 0f;

        // 몬스터 공격력 설정 (예: 무기 클래스가 있다면 설정 가능)
        Debug.Log($"{enemyStatus.name}의 공격력: {enemyStatus.dmg}");
    }

    // IDamageable 구현 - 네트워크 RPC를 통해 데미지 처리
    [PunRPC]
    public void DamageToMaster(float damage)
    {
        currentHP -= (int)damage;
        Debug.Log($"{enemyStatus.name} 체력 감소: {currentHP} HP 남음");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    // 체력 업데이트 (클라이언트에서 호출)
    [PunRPC]
    public void UpdateHP(float damage)
    {
        currentHP -= (int)damage;
        if (currentHP <= 0)
        {
            Die();
        }
    }

    // 직접 호출 가능한 데미지 처리
    public void TakeDamage(float damage)
    {
        PhotonView photonView = GetComponent<PhotonView>();
        if (photonView != null && photonView.IsMine)
        {
            photonView.RPC(nameof(DamageToMaster), RpcTarget.All, damage);
        }
    }

    private void Die()
    {
        Debug.Log($"{enemyStatus.name} 사망!");
        PhotonNetwork.Destroy(gameObject); // 네트워크 상에서 객체 제거
    }
}

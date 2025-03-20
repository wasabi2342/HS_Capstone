using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class EnemyFSM : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyStatus enemyStatus; // ������ ���� ����

    private Transform player;
    private NavMeshAgent navMeshAgent;
    private int currentHP;

    private void Start()
    {
        if (enemyStatus == null)
        {
            Debug.LogError("EnemyStatus�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        currentHP = enemyStatus.hp; // ���� ü�� �ʱ�ȭ
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent ������Ʈ�� �����ϴ�.");
        }
    }

    public void Setup(Transform player, GameObject[] wayPoints)
    {
        this.player = player;

        if (navMeshAgent == null) return;

        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        navMeshAgent.angularSpeed = 0f;

        // ���� ���ݷ� ���� (��: ���� Ŭ������ �ִٸ� ���� ����)
        Debug.Log($"{enemyStatus.name}�� ���ݷ�: {enemyStatus.dmg}");
    }

    // IDamageable ���� - ��Ʈ��ũ RPC�� ���� ������ ó��
    [PunRPC]
    public void DamageToMaster(float damage)
    {
        currentHP -= (int)damage;
        Debug.Log($"{enemyStatus.name} ü�� ����: {currentHP} HP ����");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    // ü�� ������Ʈ (Ŭ���̾�Ʈ���� ȣ��)
    [PunRPC]
    public void UpdateHP(float damage)
    {
        currentHP -= (int)damage;
        if (currentHP <= 0)
        {
            Die();
        }
    }

    // ���� ȣ�� ������ ������ ó��
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
        Debug.Log($"{enemyStatus.name} ���!");
        PhotonNetwork.Destroy(gameObject); // ��Ʈ��ũ �󿡼� ��ü ����
    }
}

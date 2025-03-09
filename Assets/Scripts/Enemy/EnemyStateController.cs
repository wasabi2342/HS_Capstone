using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections;

public class EnemyStateController : MonoBehaviourPun, IPunObservable
{
    [Header("Common")]
    public EnemyStatus baseStatus;
    [HideInInspector]
    public EnemyStatus status;
    private Transform player;
    private NavMeshAgent agent;
    private Vector3 spawnPoint;
    private PhotonView photonView;
    public GameObject attackBox;

    public bool isChasing = false;
    public bool isReturning = false;
    public bool isDying = false;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        status = Instantiate(baseStatus);
        agent.speed = status.speed;
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        player = GameObject.FindWithTag("Player")?.transform;
        attackBox.SetActive(false);
        spawnPoint = transform.position;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (isDying) return;

        if (isChasing) agent.SetDestination(player.position);

        if (isReturning)
        {
            agent.SetDestination(spawnPoint);

            if (Vector3.Distance(transform.position, spawnPoint) < 0.5f)
            {
                isReturning = false;
                agent.isStopped = true;
                photonView.RPC("SyncState", RpcTarget.All, false, false);
            }
        }

        if (status.hp <= 0 && !isDying) Die();
    }

    public void DetectPlayer()
    {
        if (isReturning) return; // 복귀 중이면 감지 X

        isChasing = true;
        agent.isStopped = false;
        photonView.RPC("SyncState", RpcTarget.All, true, false);
        Debug.Log("[추적] 플레이어 감지 - 추적 시작!");
    }

    public void LostPlayer()
    {
        if (isChasing)
        {
            isChasing = false;
            isReturning = true;
            agent.SetDestination(spawnPoint);
            photonView.RPC("SyncState", RpcTarget.All, false, true);
            Debug.Log("[해제] 플레이어 놓침 - 복귀 중...");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDying) return; // 이미 죽은 상태면 처리 X

        photonView.RPC("SyncDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    void SyncDamage(int damage)
    {
        status.hp -= damage;
        if (status.hp <= 0 && !isDying)
        {
            Die();
        }
    }

    void Die()
    {
        isDying = true;
        agent.isStopped = true;
        attackBox.SetActive(false);
        photonView.RPC("SyncDeath", RpcTarget.All);
        Destroy(gameObject, 2f);
    }

    [PunRPC]
    void SyncState(bool chasing, bool returning)
    {
        isChasing = chasing;
        isReturning = returning;
    }

    [PunRPC]
    void SyncDeath()
    {
        isDying = true;
        attackBox.SetActive(false);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isChasing);
            stream.SendNext(isReturning);
            stream.SendNext(transform.position);
        }
        else
        {
            isChasing = (bool)stream.ReceiveNext();
            isReturning = (bool)stream.ReceiveNext();
            transform.position = (Vector3)stream.ReceiveNext();
        }
    }
}

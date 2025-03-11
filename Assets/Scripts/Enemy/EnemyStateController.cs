using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections;
using System.Linq;
using Unity.Behavior;

public class EnemyStateController : MonoBehaviourPun, IPunObservable
{
    [Header("Common")]
    public EnemyStatus baseStatus;
    [HideInInspector]
    public EnemyStatus status;
    private NavMeshAgent agent;
    private PhotonView photonView;
    public GameObject attackBox;
    private BehaviorGraphAgent behaviorAgent;

    private Vector3 spawnPoint;
    private Transform player;
    public bool isDying = false;

    void Start()
    {
        status = Instantiate(baseStatus);
        agent = GetComponent<NavMeshAgent>();
        agent.speed = status.speed;
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        spawnPoint = transform.position;
        // 기타 HP, Photon 로직 등은 기존처럼
    }

    public void Setup(Transform player, GameObject[] wayPoints)
    {
        this.player = player;

        // BehaviorGraphAgent 붙어있는지 확인
        behaviorAgent = GetComponent<BehaviorGraphAgent>();
        if (behaviorAgent != null)
        {
            behaviorAgent.SetVariableValue("PatrolPoints", wayPoints.ToList());
            behaviorAgent.SetVariableValue("Player", player.gameObject);
        }
    }

    void Update()
    {
        // 마스터 클라이언트만 진행
        if (!PhotonNetwork.IsMasterClient) return;
        // 이미 사망 처리 중이면 로직 중단
        if (isDying) return;

        // HP가 0 이하일 때 사망 처리
        if (status.hp <= 0 && !isDying)
        {
            Die();
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDying) return;
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

        photonView.RPC("SyncDeath", RpcTarget.All);

        // 2초 후 오브젝트 제거
        Destroy(gameObject, 2f);
    }

    [PunRPC]
    void SyncDeath()
    {
        isDying = true;

    }

    // PUN 동기화
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 여기서는 필요 시 위치/회전/상태 등을 보낼 수 있음
            stream.SendNext(transform.position);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
        }
    }

}

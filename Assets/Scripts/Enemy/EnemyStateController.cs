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
        // ��Ÿ HP, Photon ���� ���� ����ó��
    }

    public void Setup(Transform player, GameObject[] wayPoints)
    {
        this.player = player;

        // BehaviorGraphAgent �پ��ִ��� Ȯ��
        behaviorAgent = GetComponent<BehaviorGraphAgent>();
        if (behaviorAgent != null)
        {
            behaviorAgent.SetVariableValue("PatrolPoints", wayPoints.ToList());
            behaviorAgent.SetVariableValue("Player", player.gameObject);
        }
    }

    void Update()
    {
        // ������ Ŭ���̾�Ʈ�� ����
        if (!PhotonNetwork.IsMasterClient) return;
        // �̹� ��� ó�� ���̸� ���� �ߴ�
        if (isDying) return;

        // HP�� 0 ������ �� ��� ó��
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

        // 2�� �� ������Ʈ ����
        Destroy(gameObject, 2f);
    }

    [PunRPC]
    void SyncDeath()
    {
        isDying = true;

    }

    // PUN ����ȭ
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // ���⼭�� �ʿ� �� ��ġ/ȸ��/���� ���� ���� �� ����
            stream.SendNext(transform.position);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
        }
    }

}

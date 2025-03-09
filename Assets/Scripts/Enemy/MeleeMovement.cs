using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class MeleeMovement : MonoBehaviourPun, IPunObservable
{
    [Header("Common")]
    public EnemyStatus baseStatus;
    [HideInInspector]
    public EnemyStatus status;
    private Transform player;
    private NavMeshAgent agent;
    private Vector3 spawnPoint;
    private PhotonView photonView;

    [Header("State")]
    public bool isReturningToPatrol = false;
    public bool isChasing = false;
    public bool isPatrolling = true;

    [Header("Patrol & Chase Settings")]
    public Transform patrolArea;
    private Vector3 patrolCenter;
    private float patrolRadius;
    public Transform chaseRange; // 하위 오브젝트로 `ChaseRange` 탐색

    private EnemyStateController stateController;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        agent = GetComponent<NavMeshAgent>();
        stateController = GetComponent<EnemyStateController>();
    }

    void Start()
    {
        status = Instantiate(baseStatus);
        agent.speed = status.speed;
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        player = GameObject.FindWithTag("Player")?.transform;
        spawnPoint = transform.position;

        if (patrolArea != null) SetPatrolArea(patrolArea);
        else Debug.LogError("[오류] PatrolArea가 설정되지 않음!");

        if (chaseRange == null) // 하위 오브젝트에서 ChaseRange 자동 탐색
        {
            Transform chaseObject = transform.Find("ChaseRange");
            if (chaseObject != null)
            {
                chaseRange = chaseObject;
            }
            else
            {
                Debug.LogError("[오류] ChaseRange 하위 오브젝트가 없습니다!");
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            SetRandomStartPosition();
            StartCoroutine(Patrol());
        }
    }

    void SetRandomStartPosition()
    {
        Vector3 startPosition = GetRandomPointInPatrolArea();
        transform.position = startPosition;
        agent.Warp(startPosition);
    }

    public void SetPatrolArea(Transform area)
    {
        patrolArea = area;
        patrolCenter = area.position;
        SphereCollider collider = area.GetComponent<SphereCollider>();
        patrolRadius = collider != null ? collider.radius : 5f;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (stateController.isDying || player == null) return;

        float distanceFromPatrolCenter = Vector3.Distance(transform.position, patrolCenter);
        float distanceFromPlayer = Vector3.Distance(transform.position, player.position);
        float chaseRadius = chaseRange.GetComponent<SphereCollider>().radius; // ChaseRange의 반경 가져오기

        //플레이어를 추격 중인데 PatrolArea를 벗어나거나 ChaseRange 밖으로 나가면 추격 해제 후 PatrolArea로 복귀
        if (isChasing && (distanceFromPatrolCenter > patrolRadius || distanceFromPlayer > chaseRadius))
        {
            StopChasingAndReturnToPatrol();
        }

        if (isChasing)
        {
            agent.SetDestination(player.position);
        }

        if (isReturningToPatrol && distanceFromPatrolCenter < patrolRadius * 0.8f)
        {
            isReturningToPatrol = false;
            isPatrolling = true;
            StartCoroutine(Patrol());
            photonView.RPC("SyncState", RpcTarget.All, false);
        }
    }

    public void StartChasing()
    {
        isChasing = true;
        isPatrolling = false;
        agent.SetDestination(player.position);
        photonView.RPC("SyncChaseState", RpcTarget.All, true);
    }

    public void StopChasingAndReturnToPatrol()
    {
        isChasing = false;
        isReturningToPatrol = true;
        agent.SetDestination(GetRandomPointInPatrolArea());
        photonView.RPC("SyncChaseState", RpcTarget.All, false);
    }

    IEnumerator Patrol()
    {
        while (isPatrolling)
        {
            Vector3 randomPoint = GetRandomPointInPatrolArea();
            agent.SetDestination(randomPoint);
            yield return new WaitForSeconds(3f);
        }
    }

    Vector3 GetRandomPointInPatrolArea()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomPoint = patrolCenter + Random.insideUnitSphere * patrolRadius;
            randomPoint.y = transform.position.y;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 2.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return patrolCenter;
    }

    [PunRPC]
    void SyncState(bool returning)
    {
        isReturningToPatrol = returning;
    }

    [PunRPC]
    void SyncChaseState(bool chasing)
    {
        isChasing = chasing;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isReturningToPatrol);
            stream.SendNext(isChasing);
            stream.SendNext(transform.position);
        }
        else
        {
            isReturningToPatrol = (bool)stream.ReceiveNext();
            isChasing = (bool)stream.ReceiveNext();
            transform.position = (Vector3)stream.ReceiveNext();
        }
    }
}

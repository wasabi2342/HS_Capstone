using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GhoulMovement : MonoBehaviour
{
    [Header("Common")]
    public EnemyStatus baseStatus;
    [HideInInspector]
    public EnemyStatus status;
    private Transform player;
    private NavMeshAgent agent;

    [Header("State")]
    public bool isDetecting = false;

    private EnemyStateController stateController;
    private MeleeAttack meleeAttack;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stateController = GetComponent<EnemyStateController>();
        meleeAttack = GetComponent<MeleeAttack>();
    }

    void Start()
    {
        status = Instantiate(baseStatus);
        agent.speed = status.speed;
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("[오류] 플레이어를 찾을 수 없습니다! 'Player' 태그가 있는지 확인하세요.");
        }
    }

    void Update()
    {
        if (stateController.isDying || player == null) return; // 사망 시 또는 플레이어가 없으면 이동 중단

        DetectPlayer();
    }

    void DetectPlayer()
    {
        if (player == null) return; // 플레이어가 없으면 탐지 중단

        float distance = Vector3.Distance(transform.position, player.position);
        isDetecting = distance <= status.detectionSize.x;
    }

    public void SetMoveSpeed(float speed)
    {
        agent.speed = speed;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MeleeMovement : MonoBehaviour
{
    [Header("Common")]
    public EnemyStatus baseStatus;
    [HideInInspector]
    public EnemyStatus status;
    private Transform player;
    private NavMeshAgent agent;
    private Vector3 spawnPoint; // 스폰 위치 저장

    [Header("State")]
    public bool isDetecting = false;
    public bool isAttacking = false;
    public bool isReturningToSpawn = false;

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

        transform.rotation = Quaternion.Euler(45f, 0f, 0f);

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("[오류] 플레이어를 찾을 수 없습니다! 'Player' 태그가 있는지 확인하세요.");
        }

        spawnPoint = transform.position; // 초기 스폰 위치 저장
    }

    void Update()
    {
        if (stateController.isDying || player == null) return; // 사망 시 또는 플레이어가 없으면 이동 중단

        float distance = Vector3.Distance(transform.position, player.position);
        float spawnDistance = Vector3.Distance(transform.position, spawnPoint);

        // 플레이어 감지
        if (distance <= status.detectionSize.x && !isDetecting)
        {
            isDetecting = true;
            Debug.Log("[탐지] 몬스터가 플레이어를 감지하고 추적을 시작합니다.");
            agent.speed = status.speed * 1.5f; // 감지 시 속도 증가
        }

        // 공격 로직
        if (isDetecting && distance <= status.attackSize.x && !isAttacking)
        {
            StartCoroutine(AttackPlayer());
        }

        // 추적 로직
        if (isDetecting && !isAttacking)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        // 스폰 포인트에서 너무 멀어지면 복귀
        if (spawnDistance > 7f)
        {
            isDetecting = false;
            isReturningToSpawn = true;
            agent.SetDestination(spawnPoint);
            Debug.Log("[이동] 몬스터가 스폰포인트로 복귀 중...");
        }

        // 복귀 완료 시 상태 초기화
        if (isReturningToSpawn && spawnDistance < 0.5f)
        {
            isReturningToSpawn = false;
            agent.isStopped = true;
            Debug.Log("[대기] 몬스터가 스폰 포인트에 도착하여 대기 중...");
        }
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;
        agent.isStopped = true;

        Debug.Log("[공격] 몬스터가 공격을 준비 중...");
        yield return new WaitForSeconds(status.waitCool);

        Debug.Log("[공격] 몬스터가 공격을 실행합니다!");
        meleeAttack.GiveDamage();

        yield return new WaitForSeconds(status.attackCool);

        isAttacking = false;
        agent.isStopped = false;
    }

    public void SetMoveSpeed(float speed)
    {
        agent.speed = speed;
    }
}

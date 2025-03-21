using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("AI Parameters")]
    public float detectRange = 10f;   // 플레이어 탐지 거리
    public float attackRange = 2f;    // 공격 가능 거리
    public float wanderSpeed = 2f;    // 방랑 이동 속도
    public float chaseSpeed = 5f;     // 추적 이동 속도

    [SerializeField] private SpawnArea spawnArea;
    private Transform targetPlayer;
    private NavMeshAgent agent;
    private BehaviorTreeRunner behaviorTree;
    private IMonsterAttack attackStrategy;

    // [Wander] 상태 (도착→대기→다음 목적지)
    private bool isWaiting = false;
    private float waitTime = 0f;
    private float waitTimer = 0f;
    private Vector3 wanderTarget = Vector3.zero;

    // [애니메이션] 이동 방향
    public Animator animator;
    private float lastMoveX = 1f;         // 마지막 이동 방향
    private string currentMoveAnim = "";  // 현재 이동/정지 애니메이션 이름

    // [공격 관련] 쿨다운 & 애니메이션
    private bool canAttack = true;                // 쿨다운이 풀렸을 때만 공격 가능
    public float attackCooldown = 1.0f;           // 공격 이후 대기 시간
    private float cooldownTimer = 0f;             // 쿨다운 타이머

    private bool isAttackAnimationPlaying = false; // 공격 애니메이션이 실제 재생 중인지
    private float attackAnimTime = 0f;             // 현재 공격 애니메이션 진행 시간
    public float attackAnimDuration = 0.7f;        // 공격 애니메이션 길이(예: 0.7초 등)

    private string prevAnimBeforeAttack = "";      // 공격 전에 재생 중이던 애니메이션 이름

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.updateRotation = false;
        agent.angularSpeed = 500f;
        agent.stoppingDistance = 0.1f;

        if (spawnArea == null)
        {
            spawnArea = GetComponentInParent<SpawnArea>();
            Debug.Log($"[INIT] spawnArea auto-assigned: {spawnArea}");
        }

        attackStrategy = GetComponent<IMonsterAttack>();

        // Behavior Tree 생성
        behaviorTree = new BehaviorTreeRunner(SettingBT());
    }

    private void Update()
    {
        // 1) 쿨다운 처리
        if (!canAttack)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= attackCooldown)
            {
                // 쿨다운 종료
                canAttack = true;
                cooldownTimer = 0f;
            }
        }

        // 2) Behavior Tree 실행
        behaviorTree.Operate();

        // 3) 공격 애니메이션 진행 체크
        if (isAttackAnimationPlaying)
        {
            attackAnimTime += Time.deltaTime;

            // 공격 애니메이션이 끝났으면
            if (attackAnimTime >= attackAnimDuration)
            {
                // 애니메이션 재생을 이전 상태로 복귀
                isAttackAnimationPlaying = false;
                attackAnimTime = 0f;

                // 공격 전에 하고 있던 애니메이션으로 다시 재생
                if (!string.IsNullOrEmpty(prevAnimBeforeAttack))
                {
                    animator.Play(prevAnimBeforeAttack);
                }
            }
            // 공격 애니메이션 끝나기 전에는 이동/Idle 애니메이션 스킵
            return;
        }

        // 4) 공격 애니메이션이 아닌 경우 → 이동/Idle 애니메이션
        float vx = agent.velocity.x;
        if (!Mathf.Approximately(vx, 0f))
        {
            lastMoveX = vx; // 이동 방향 갱신
        }

        if (!agent.isStopped)
        {
            // 이동 중
            if (vx > 0f)
            {
                PlayMoveAnim("Run_Right");
            }
            else if (vx < 0f)
            {
                PlayMoveAnim("Run_Left");
            }
            else
            {
                if (lastMoveX >= 0f) PlayMoveAnim("Run_Right");
                else PlayMoveAnim("Run_Left");
            }
        }
        else
        {
            // 정지 상태
            PlayMoveAnim("Idle");
        }
    }

    // 단순히 이동/정지 애니메이션을 재생하는 헬퍼 함수
    void PlayMoveAnim(string animName)
    {
        // 같은 애니메이션이면 중복 재생 안 하도록
        if (currentMoveAnim == animName)
            return;

        animator.Play(animName);
        currentMoveAnim = animName;
    }

    // ─────────────────────────────────────────────────────────────────
    // Behavior Tree
    // ─────────────────────────────────────────────────────────────────
    INode SettingBT()
    {
        return new SelectorNode(
            new List<INode>()
            {
                new SequenceNode(
                    new List<INode>()
                    {
                        new ActionNode(CheckEnemyWithinAttackRange),
                        new ActionNode(DoAttack),
                    }
                ),
                new SequenceNode(
                    new List<INode>()
                    {
                        new ActionNode(CheckDetectEnemy),
                        new ActionNode(MoveToEnemy),
                    }
                ),
                // Wander
                new ActionNode(WanderInsideSpawnArea),
            }
        );
    }

    // ─────────────────────────────────────────────────────────────────
    // 1) 공격 범위 체크
    // ─────────────────────────────────────────────────────────────────
    INode.NodeState CheckEnemyWithinAttackRange()
    {
        if (!canAttack) // 쿨다운 중이면 실패
            return INode.NodeState.Failure;

        if (targetPlayer != null)
        {
            float dist = Vector3.Distance(transform.position, targetPlayer.position);
            if (dist < attackRange)
            {
                return INode.NodeState.Success;
            }
        }
        return INode.NodeState.Failure;
    }

    // ─────────────────────────────────────────────────────────────────
    // 2) 공격 실행
    // ─────────────────────────────────────────────────────────────────
    INode.NodeState DoAttack()
    {
        // 실제 공격
        if (targetPlayer != null && attackStrategy != null && canAttack)
        {
            // 공격 전에 하던 애니메이션 이름을 기억
            prevAnimBeforeAttack = currentMoveAnim;

            // 방향에 따라 공격 애니메이션 재생
            float targetX = targetPlayer.position.x;
            float selfX = transform.position.x;
            if (targetX >= selfX)
            {
                animator.Play("Attack_Right");
            }
            else
            {
                animator.Play("Attack_Left");
            }

            // 타격 로직 (Animation Event로 맞춰도 됨)
            attackStrategy.Attack(targetPlayer);

            // 공격 애니메이션 진행 상태 ON
            isAttackAnimationPlaying = true;
            attackAnimTime = 0f;

            // 쿨다운 시작
            canAttack = false;
            cooldownTimer = 0f;

            return INode.NodeState.Success;
        }

        return INode.NodeState.Failure;
    }

    // ─────────────────────────────────────────────────────────────────
    // 3) 플레이어 탐지 & 추적
    // ─────────────────────────────────────────────────────────────────
    INode.NodeState CheckDetectEnemy()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDist = detectRange;
        Transform nearestPlayer = null;

        foreach (GameObject player in players)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearestPlayer = player.transform;
            }
        }

        targetPlayer = nearestPlayer;
        return (targetPlayer != null) ? INode.NodeState.Success : INode.NodeState.Failure;
    }

    INode.NodeState MoveToEnemy()
    {
        if (targetPlayer != null)
        {
            Vector3 targetPos = new Vector3(
                targetPlayer.position.x,
                transform.position.y,
                targetPlayer.position.z
            );
            agent.speed = chaseSpeed;
            agent.isStopped = false;
            agent.SetDestination(targetPos);
            return INode.NodeState.Running;
        }
        return INode.NodeState.Failure;
    }

    // ─────────────────────────────────────────────────────────────────
    // 4) 방랑(Wander): 목적지 도착 시 3~5초 대기
    // ─────────────────────────────────────────────────────────────────
    INode.NodeState WanderInsideSpawnArea()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                waitTimer = 0f;
                waitTime = 0f;
                agent.isStopped = false;
                ChooseNextWanderDestination();
            }
            return INode.NodeState.Running;
        }

        if (agent.pathPending)
            return INode.NodeState.Running;

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            isWaiting = true;
            agent.isStopped = true;
            waitTime = Random.Range(3f, 5f);
            waitTimer = 0f;
            Debug.Log($"[Wander] 목적지 도착. {waitTime:F2}초 대기 시작.");
        }

        return INode.NodeState.Running;
    }

    private void ChooseNextWanderDestination()
    {
        if (spawnArea == null)
        {
            spawnArea = GetComponentInParent<SpawnArea>();
            Debug.LogWarning("[BT] WanderInsideSpawnArea: spawnArea was null, attempted reassignment.");
        }

        if (spawnArea == null || agent == null)
        {
            Debug.LogWarning("[BT] WanderInsideSpawnArea: Missing spawnArea or agent");
            return;
        }

        const int maxTries = 10;
        const float minDistToOthers = 2.5f;
        const float minWanderDistance = 1.5f;

        for (int i = 0; i < maxTries; i++)
        {
            Vector3 candidate = spawnArea.GetRandomPointInsideArea();
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                float dist = Vector3.Distance(transform.position, hit.position);
                if (dist < minWanderDistance)
                    continue;

                if (!IsNearOtherMonsters(hit.position, minDistToOthers))
                {
                    wanderTarget = hit.position;
                    wanderTarget.y = transform.position.y;
                    if (agent.SetDestination(wanderTarget))
                    {
                        agent.speed = wanderSpeed;
                        Debug.Log($"[Wander] 새 목적지: {wanderTarget} (dist: {dist:F2}m)");
                        return;
                    }
                }
            }
        }

        Debug.LogWarning("[Wander] Failed to find valid destination after retries");
    }

    private bool IsNearOtherMonsters(Vector3 pos, float minDist)
    {
        foreach (var other in FindObjectsOfType<EnemyAI>())
        {
            if (other != this && Vector3.Distance(other.transform.position, pos) < minDist)
                return true;
        }
        return false;
    }

    // ─────────────────────────────────────────────────────────────────
    // (선택) 애니메이션 이벤트로 타격 시점 맞추는 함수
    // ─────────────────────────────────────────────────────────────────
    public void OnAttackHitEvent()
    {
        if (targetPlayer != null && attackStrategy != null)
        {
            attackStrategy.Attack(targetPlayer);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        if (wanderTarget != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(wanderTarget, 0.4f);
            Gizmos.DrawLine(transform.position, wanderTarget);
        }
    }
}

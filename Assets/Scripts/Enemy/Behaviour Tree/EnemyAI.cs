/*
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public EnemyStatus status;

    [SerializeField] private SpawnArea spawnArea;
    private Transform targetPlayer;
    private NavMeshAgent agent;
    private BehaviorTreeRunner behaviorTree;
    private IMonsterAttack attackStrategy;

    private bool isWaiting = false;
    private float waitTime = 0f;
    private float waitTimer = 0f;
    private Vector3 wanderTarget = Vector3.zero;

    public Animator animator;
    private float lastMoveX = 1f;
    private string currentMoveAnim = "";

    private bool canAttack = true;
    private float cooldownTimer = 0f;

    private bool isAttackAnimationPlaying = false;
    private float attackAnimTime = 0f;
    private float attackAnimDuration = 0.7f; // status로부터 설정

    private string prevAnimBeforeAttack = "";

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
        attackAnimDuration = status.animDuration;

        behaviorTree = new BehaviorTreeRunner(SettingBT());
    }

    private void Update()
    {
        if (!canAttack)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= status.attackCool)
            {
                canAttack = true;
                cooldownTimer = 0f;
            }
        }

        behaviorTree.Operate();

        if (isAttackAnimationPlaying)
        {
            attackAnimTime += Time.deltaTime;
            if (attackAnimTime >= attackAnimDuration)
            {
                isAttackAnimationPlaying = false;
                attackAnimTime = 0f;

                if (!string.IsNullOrEmpty(prevAnimBeforeAttack))
                    animator.Play(prevAnimBeforeAttack);
            }
            return;
        }

        float vx = agent.velocity.x;
        if (!Mathf.Approximately(vx, 0f))
        {
            lastMoveX = vx;
        }

        if (!agent.isStopped)
        {
            if (vx > 0f) PlayMoveAnim("Run_Right");
            else if (vx < 0f) PlayMoveAnim("Run_Left");
            else PlayMoveAnim(lastMoveX >= 0f ? "Run_Right" : "Run_Left");
        }
        else
        {
            PlayMoveAnim("Idle");
        }
    }

    void PlayMoveAnim(string animName)
    {
        if (currentMoveAnim == animName) return;

        animator.Play(animName);
        currentMoveAnim = animName;
    }

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
                new ActionNode(WanderInsideSpawnArea),
            }
        );
    }

    INode.NodeState CheckEnemyWithinAttackRange()
    {
        if (!canAttack) return INode.NodeState.Failure;

        if (targetPlayer != null)
        {
            float dist = Vector3.Distance(transform.position, targetPlayer.position);
            if (dist < status.attackRange)
                return INode.NodeState.Success;
        }

        return INode.NodeState.Failure;
    }

    INode.NodeState DoAttack()
    {
        if (targetPlayer != null && attackStrategy != null && canAttack)
        {
            prevAnimBeforeAttack = currentMoveAnim;

            float targetX = targetPlayer.position.x;
            float selfX = transform.position.x;

            animator.Play(targetX >= selfX ? "Attack_Right" : "Attack_Left");
            attackStrategy.Attack(targetPlayer);

            isAttackAnimationPlaying = true;
            attackAnimTime = 0f;

            canAttack = false;
            cooldownTimer = 0f;

            return INode.NodeState.Success;
        }

        return INode.NodeState.Failure;
    }

    INode.NodeState CheckDetectEnemy()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDist = status.detectRange;
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
        return targetPlayer != null ? INode.NodeState.Success : INode.NodeState.Failure;
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
            agent.speed = status.chaseSpeed;
            agent.isStopped = false;
            agent.SetDestination(targetPos);
            return INode.NodeState.Running;
        }

        return INode.NodeState.Failure;
    }

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

        if (agent.pathPending) return INode.NodeState.Running;

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
                        agent.speed = status.wanderSpeed;
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
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviourPun
{
    public EnemyStatus status;  // enemyStatus에 공격 데미지, 속도, 범위 등 값들이 정의되어 있음

    [SerializeField] private SpawnArea spawnArea;
    private Transform targetPlayer;
    private NavMeshAgent agent;
    private BehaviorTreeRunner behaviorTree;
    private IMonsterAttack attackStrategy;

    private bool isWaiting = false;
    private float waitTime = 0f;
    private float waitTimer = 0f;
    private Vector3 wanderTarget = Vector3.zero;

    public Animator animator;
    private float lastMoveX = 1f;
    private string currentMoveAnim = "";
    private bool canAttack = true;
    private float cooldownTimer = 0f;
    private bool isAttackAnimationPlaying = false;
    private float attackAnimTime = 0f;
    private float attackAnimDuration = 0.7f; // 기본값, status.animDuration으로 덮어씀
    private string prevAnimBeforeAttack = "";

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
        attackAnimDuration = status.animDuration;
        behaviorTree = new BehaviorTreeRunner(SettingBT());
    }

    private void Update()
    {
        // 오직 Master Client에서만 AI 로직 실행
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!canAttack)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= status.attackCool)
            {
                canAttack = true;
                cooldownTimer = 0f;
            }
        }

        behaviorTree.Operate();

        if (isAttackAnimationPlaying)
        {
            attackAnimTime += Time.deltaTime;
            if (attackAnimTime >= attackAnimDuration)
            {
                isAttackAnimationPlaying = false;
                attackAnimTime = 0f;
                if (!string.IsNullOrEmpty(prevAnimBeforeAttack))
                    animator.Play(prevAnimBeforeAttack);
            }
            return;
        }

        float vx = agent.velocity.x;
        if (!Mathf.Approximately(vx, 0f))
        {
            lastMoveX = vx;
        }

        if (!agent.isStopped)
        {
            if (vx > 0f) PlayMoveAnim("Run_Right");
            else if (vx < 0f) PlayMoveAnim("Run_Left");
            else PlayMoveAnim(lastMoveX >= 0f ? "Run_Right" : "Run_Left");
        }
        else
        {
            PlayMoveAnim("Idle");
        }
    }

    void PlayMoveAnim(string animName)
    {
        if (currentMoveAnim == animName) return;
        animator.Play(animName);
        currentMoveAnim = animName;
    }

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
                new ActionNode(WanderInsideSpawnArea),
            }
        );
    }

    INode.NodeState CheckEnemyWithinAttackRange()
    {
        if (!canAttack)
            return INode.NodeState.Failure;

        if (targetPlayer != null)
        {
            float dist = Vector3.Distance(transform.position, targetPlayer.position);
            if (dist < status.attackRange)
                return INode.NodeState.Success;
        }
        return INode.NodeState.Failure;
    }

    INode.NodeState DoAttack()
    {
        if (targetPlayer != null && attackStrategy != null && canAttack)
        {
            prevAnimBeforeAttack = currentMoveAnim;
            float targetX = targetPlayer.position.x;
            float selfX = transform.position.x;
            animator.Play(targetX >= selfX ? "Attack_Right" : "Attack_Left");

            // 공격은 Master Client 기준으로 실행
            attackStrategy.Attack(targetPlayer);

            isAttackAnimationPlaying = true;
            attackAnimTime = 0f;
            canAttack = false;
            cooldownTimer = 0f;

            return INode.NodeState.Success;
        }
        return INode.NodeState.Failure;
    }

    INode.NodeState CheckDetectEnemy()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDist = status.detectRange;
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
        return targetPlayer != null ? INode.NodeState.Success : INode.NodeState.Failure;
    }

    INode.NodeState MoveToEnemy()
    {
        if (targetPlayer != null)
        {
            Vector3 targetPos = new Vector3(targetPlayer.position.x, transform.position.y, targetPlayer.position.z);
            agent.speed = status.chaseSpeed;
            agent.isStopped = false;
            agent.SetDestination(targetPos);
            return INode.NodeState.Running;
        }
        return INode.NodeState.Failure;
    }

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
                        agent.speed = status.wanderSpeed;
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

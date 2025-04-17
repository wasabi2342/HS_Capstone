using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

/// <summary>
/// 플레이어를 추적 및 공격하는 몬스터 AI.
/// 죽은 플레이어(WhitePlayerState.Death)는 타겟에서 제외하여 Wander 상태로 돌아가도록 수정.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviourPun, IDamageable
{
    public EnemyStatus status;  // EnemyStatus에 공격 데미지, 속도, 범위, 체력 등이 정의
    public static int ActiveMonsterCount = 0;

    [SerializeField] private SpawnArea spawnArea;
    public GameObject damageTextPrefab;
    private SpriteRenderer spriteRenderer;
    public Animator animator;
    private Transform targetPlayer;
    public DebuffController debuffController;
    public NavMeshAgent agent { get; private set; }
    private BehaviorTreeRunner behaviorTree;
    private IMonsterAttack attackStrategy;

    private bool isWaiting = false;
    private float waitTime = 0f;
    private float waitTimer = 0f;
    private Vector3 wanderTarget = Vector3.zero;

    private float lastMoveX = 1f;
    private string currentMoveAnim = "";
    private bool canAttack = true;
    private float cooldownTimer = 0f;

    private bool isAttackAnimationPlaying = false;
    private float attackAnimTime = 0f;
    private float attackAnimDuration = 0.7f; // 기본값, status.animDuration으로 덮어씀
    private string prevAnimBeforeAttack = "";
    private bool isPreparingAttack = false;
    private float attackPrepareTimer = 0f;

    // 체력 관련 변수
    private float currentHP;
    private bool isDead = false;             // 사망 플래그
    private float deathAnimDuration = 1.5f;  // 사망 애니메이션 지속 시간

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        debuffController = GetComponent<DebuffController>();

        agent.updateRotation = false;
        agent.angularSpeed = 500f;
        agent.stoppingDistance = 0.1f;

        // 마스터 클라이언트일 때만 카운트 증가
        if (PhotonNetwork.IsMasterClient)
        {
            ActiveMonsterCount++;
        }
        Debug.Log("EnemyAI Awake: " + gameObject.name + " ActiveMonsterCount: " + ActiveMonsterCount);

        // SpawnArea 자동 할당
        if (spawnArea == null)
        {
            spawnArea = GetComponentInParent<SpawnArea>();
            Debug.Log($"[INIT] spawnArea auto-assigned: {spawnArea}");
        }

        // IMonsterAttack 전략 찾기
        attackStrategy = GetComponent<IMonsterAttack>();

        // EnemyStatus에 정의된 애니메이션 길이 사용
        attackAnimDuration = status.animDuration;

        // Behavior Tree 구성
        behaviorTree = new BehaviorTreeRunner(SettingBT());

        // EnemyStatus에 정의된 초기 체력 사용
        currentHP = status.hp;
    }

    private void Update()
    {
        // 마스터 클라이언트만 AI 로직
        if (!PhotonNetwork.IsMasterClient || isDead)
            return;

        // 공격 쿨타임
        if (!canAttack)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= status.attackCool)
            {
                canAttack = true;
                cooldownTimer = 0f;
            }
        }

        // [추가 1] targetPlayer가 죽었는지 실시간 확인 (Optional)
        if (targetPlayer != null)
        {
            // WhitePlayerController (또는 유사 스크립트)에서 Death 상태 확인
            // (코드에서는 PlayerController + PlayerState로 수정)
            var pc = targetPlayer.GetComponent<PlayerController>();
            if (pc == null || pc.CurrentState == PlayerState.Death)
            {
                targetPlayer = null;
                ResetAttackPreparation();
                return;
            }
        }

        // Behavior Tree 실행
        behaviorTree.Operate();

        // 공격 애니메이션 진행 중이면 대기
        if (isAttackAnimationPlaying)
        {
            attackAnimTime += Time.deltaTime;
            if (attackAnimTime >= attackAnimDuration)
            {
                isAttackAnimationPlaying = false;
                attackAnimTime = 0f;
                if (!string.IsNullOrEmpty(prevAnimBeforeAttack))
                    PlayMoveAnim(prevAnimBeforeAttack);
            }
            return;
        }

        // 이동/Idle 애니메이션 처리
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
            PlayMoveAnim(lastMoveX >= 0f ? "Right_Idle" : "Left_Idle");
        }
    }

    // ─────────────────────────────────────────
    // 애니메이션 재생 및 RPC 동기화
    // ─────────────────────────────────────────
    void PlayMoveAnim(string animName)
    {
        if (currentMoveAnim == animName)
            return;

        animator.Play(animName);
        currentMoveAnim = animName;
        photonView.RPC("RPC_PlayAnimation", RpcTarget.Others, animName);
    }

    [PunRPC]
    public void RPC_PlayAnimation(string animName)
    {
        animator.Play(animName);
        currentMoveAnim = animName;
    }

    // ─────────────────────────────────────────
    // Behavior Tree 구성
    // ─────────────────────────────────────────
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

    // ─────────────────────────────────────────
    // [CheckEnemyWithinAttackRange]
    // 사정거리 안에 플레이어가 있는지, 살아있는지 확인
    // ─────────────────────────────────────────
    INode.NodeState CheckEnemyWithinAttackRange()
    {
        if (!canAttack)
            return INode.NodeState.Failure;

        if (targetPlayer != null)
        {
            // 여기서도 원래는 WhitePlayerController / WhitePlayerState 체크
            // 현재는 PlayerController / PlayerState로 변경
            var pc = targetPlayer.GetComponent<PlayerController>();
            if (pc == null || pc.CurrentState == PlayerState.Death)
            {
                // 죽은 플레이어 무효 처리
                targetPlayer = null;
                return INode.NodeState.Failure;
            }

            float dist = Vector3.Distance(transform.position, targetPlayer.position);
            if (dist < status.attackRange)
                return INode.NodeState.Success;
        }
        return INode.NodeState.Failure;
    }

    // ─────────────────────────────────────────
    // [DoAttack]
    // 실제 공격 애니메이션 및 데미지 처리
    // ─────────────────────────────────────────
    INode.NodeState DoAttack()
    {
        if (targetPlayer != null && attackStrategy != null && canAttack)
        {
            // 원래 WhitePlayerController → PlayerController
            var pc = targetPlayer.GetComponent<PlayerController>();
            if (pc == null || pc.CurrentState == PlayerState.Death)
            {
                targetPlayer = null;
                ResetAttackPreparation();
                return INode.NodeState.Failure;
            }

            float targetX = targetPlayer.position.x;
            string idleAnim = targetX >= transform.position.x ? "Right_Idle" : "Left_Idle";

            // 공격 준비 중?
            if (isPreparingAttack)
            {
                attackPrepareTimer += Time.deltaTime;
                if (attackPrepareTimer >= status.waitCool)
                {
                    // 대기 완료 → 공격 실행
                    isPreparingAttack = false;
                    attackPrepareTimer = 0f;

                    prevAnimBeforeAttack = currentMoveAnim;
                    string attackAnim = targetX >= transform.position.x ? "Attack_Right" : "Attack_Left";
                    PlayMoveAnim(attackAnim);

                    attackStrategy.Attack(targetPlayer);

                    isAttackAnimationPlaying = true;
                    attackAnimTime = 0f;
                    canAttack = false;
                    cooldownTimer = 0f;

                    return INode.NodeState.Success;
                }

                // 준비 중 애니메이션 유지
                PlayMoveAnim(idleAnim);
                agent.isStopped = true;
                return INode.NodeState.Running;
            }
            else
            {
                // 공격 준비 시작
                isPreparingAttack = true;
                attackPrepareTimer = 0f;
                agent.isStopped = true;
                PlayMoveAnim(idleAnim);
                return INode.NodeState.Running;
            }
        }

        ResetAttackPreparation();
        return INode.NodeState.Failure;
    }

    private void ResetAttackPreparation()
    {
        isPreparingAttack = false;
        attackPrepareTimer = 0f;
    }

    // ─────────────────────────────────────────
    // [CheckDetectEnemy]
    // 탐지 범위 내에서 살아있는 플레이어를 찾음
    // ─────────────────────────────────────────
    INode.NodeState CheckDetectEnemy()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDist = status.detectRange;
        Transform nearestPlayer = null;

        foreach (GameObject playerObj in players)
        {
            // 원래는 WhitePlayerController / WhitePlayerState로 체크
            // now PlayerController / PlayerState
            var pc = playerObj.GetComponent<PlayerController>();
            if (pc == null || pc.CurrentState == PlayerState.Death)
                continue;  // 죽은 플레이어는 무시

            float dist = Vector3.Distance(transform.position, playerObj.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearestPlayer = playerObj.transform;
            }
        }

        targetPlayer = nearestPlayer;
        return targetPlayer != null ? INode.NodeState.Success : INode.NodeState.Failure;
    }

    // ─────────────────────────────────────────
    // [MoveToEnemy]
    // 플레이어 추격
    // ─────────────────────────────────────────
    INode.NodeState MoveToEnemy()
    {
        if (targetPlayer != null)
        {
            var pc = targetPlayer.GetComponent<PlayerController>();
            if (pc == null || pc.CurrentState == PlayerState.Death)
            {
                targetPlayer = null;
                return INode.NodeState.Failure;
            }

            Vector3 targetPos = new Vector3(targetPlayer.position.x, transform.position.y, targetPlayer.position.z);
            agent.speed = status.chaseSpeed;
            agent.isStopped = false;
            agent.SetDestination(targetPos);

            return INode.NodeState.Running;
        }
        return INode.NodeState.Failure;
    }

    // ─────────────────────────────────────────
    // [WanderInsideSpawnArea]
    // 목적지에 도착하면 랜덤 위치로 배회
    // ─────────────────────────────────────────
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
            //Debug.Log($"[Wander] 목적지 도착. {waitTime:F2}초 대기 시작.");
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
        // (주석으로 기존 로직 비활성화)
        /*
        foreach (var other in FindObjectsOfType<EnemyAI>())
        {
            if (other != this && Vector3.Distance(other.transform.position, pos) < minDist)
                return true;
        }
        */
        return false;
    }

    public void OnAttackHitEvent()
    {
        if (targetPlayer != null && attackStrategy != null)
        {
            attackStrategy.Attack(targetPlayer);
        }
    }

    // ---------------------------
    // IDamageable 인터페이스 구현
    // ---------------------------
    public void TakeDamage(float damage)
    {
        photonView.RPC("DamageToMaster", RpcTarget.MasterClient, damage);
    }

    [PunRPC]
    public void DamageToMaster(float damage)
    {
        if (!PhotonNetwork.IsMasterClient || isDead)
            return;

        string hitAnim = lastMoveX >= 0 ? "Right_Hit" : "Left_Hit";
        photonView.RPC("RPC_PlayAnimation", RpcTarget.All, hitAnim);

        currentHP -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage, current HP: {currentHP}");
        photonView.RPC("UpdateHP", RpcTarget.AllBuffered, currentHP);

        photonView.RPC("RPC_FlashSprite", RpcTarget.All);
        SpawnDamageText(damage);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    [PunRPC]
    public void RPC_FlashSprite()
    {
        StartCoroutine(FlashSpriteCoroutine());
    }

    private IEnumerator FlashSpriteCoroutine()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            Color flashColor;
            if (!ColorUtility.TryParseHtmlString("#F6CECE", out flashColor))
            {
                flashColor = Color.red;
            }

            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    private void SpawnDamageText(float damage)
    {
        if (damageTextPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0, 1.5f, 0);
            GameObject dmgText = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

            TextMesh textMesh = dmgText.GetComponent<TextMesh>();
            if (textMesh != null)
            {
                textMesh.text = damage.ToString();
            }
        }
    }

    [PunRPC]
    public void UpdateHP(float hp)
    {
        currentHP = hp;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        if (PhotonNetwork.IsMasterClient)
        {
            ActiveMonsterCount--;
            StageManager stageManager = FindObjectOfType<StageManager>();
            if (stageManager != null)
            {
                stageManager.AreAllMonstersCleared();
            }
        }

        string deathAnim = lastMoveX >= 0 ? "Right_Death" : "Left_Death";
        photonView.RPC("RPC_PlayAnimation", RpcTarget.All, deathAnim);

        StartCoroutine(DelayedDestroy());
    }

    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(deathAnimDuration);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
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

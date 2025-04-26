using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviourPun, IDamageable
{
    /* ────────────── Editor 노출 ────────────── */
    public EnemyStatus status;                       // ScriptableObject (hp, damage, headOffset …)
    [SerializeField] private SpawnArea spawnArea;    // 배회 영역
    public GameObject damageTextPrefab;              // 데미지 숫자 프리팹

    /* ────────────── 런타임 필드 ────────────── */
    public NavMeshAgent agent { get; private set; }
    public Animator animator;
    private SpriteRenderer spriteRenderer;
    public DebuffController debuffController { get; private set; }
    private IMonsterAttack attackStrategy;
    private BehaviorTreeRunner behaviorTree;

    private MonsterTargeting targeting;              // ★ 몬스터 타입 확인용

    // 타깃 & 이동
    private Transform targetPlayer;
    private float lastMoveX = 1f;
    private string currentMoveAnim = "";
    private bool canAttack = true;
    private float cooldownTimer;
    private bool isAttackAnimPlaying;
    private float attackAnimTime;
    private float attackAnimDuration = 0.7f;   // status.animDuration 로 덮어씀
    private bool isPreparingAttack;
    private float attackPrepareTimer;

    // 배회
    private bool isWaiting;
    private float waitTime, waitTimer;
    private Vector3 wanderTarget;

    // HP·Shield
    private float maxHP, currentHP;
    private float maxShield, currentShield;

    // 상태
    private bool isDead;
    private float deathAnimDuration = 1.5f;
    public static int ActiveMonsterCount;

    // UI
    private UIEnemyHealthBar uiBar;

    /* ───────────────────────────── Awake ───────────────────────────── */
    void Awake()
    {
        /* 필수 컴포넌트 캐시 */
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        debuffController = GetComponent<DebuffController>();
        targeting = GetComponent<MonsterTargeting>();    // ★

        agent.updateRotation = false;
        agent.angularSpeed = 500f;
        agent.stoppingDistance = 0.1f;

        if (PhotonNetwork.IsMasterClient) ActiveMonsterCount++;

        /* 스탯 초기화 */
        maxHP = status.hp;
        currentHP = status.hp;
        maxShield = status.maxShield;
        currentShield = status.maxShield;
        attackAnimDuration = status.animDuration;

        /* 체력바 UI 인스턴스 */
        GameObject prefab = Resources.Load<GameObject>("UIEnemyHealthBar");
        if (prefab != null)
        {
            GameObject go = Instantiate(prefab);          // 부모 지정 X ‒ 스케일 영향 안 받음
            uiBar = go.GetComponent<UIEnemyHealthBar>();
            uiBar.Init(transform, new Vector3(0f, status.headOffset, 0f));
            uiBar.SetHP(1f);
            uiBar.SetShield(maxShield > 0 ? 1f : 0f);
        }

        /* AI 세팅 */
        attackStrategy = GetComponent<IMonsterAttack>();
        behaviorTree = new BehaviorTreeRunner(BuildBehaviorTree());

        EnsureSpawnArea();                                // Awake에서도 한 번 시도
    }

    /* ─────────────────── 부모 변경 시 SpawnArea 재확인 ─────────────────── */
    void OnTransformParentChanged() => EnsureSpawnArea();

    /* SpawnArea 확보 함수 (성공 시 true) */
    bool EnsureSpawnArea()
    {
        if (spawnArea) return true;
        spawnArea = GetComponentInParent<SpawnArea>();
        return spawnArea != null;
    }

    /* ───────────────────────────── Update ───────────────────────────── */
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient || isDead) return;

        /* 공격 쿨다운 */
        if (!canAttack)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= status.attackCool)
            { canAttack = true; cooldownTimer = 0f; }
        }

        behaviorTree.Operate();           // 행동 트리 실행

        /* 공격 애니메이션 진행 중이면 대기 */
        if (isAttackAnimPlaying)
        {
            attackAnimTime += Time.deltaTime;
            if (attackAnimTime >= attackAnimDuration)
            {
                isAttackAnimPlaying = false;
                PlayAnim(lastMoveX >= 0 ? "Right_Idle" : "Left_Idle");
            }
            return;
        }

        /* 이동/Idle 애니메이션 */
        float vx = agent.velocity.x;
        if (!Mathf.Approximately(vx, 0f)) lastMoveX = vx;

        if (!agent.isStopped)
        {
            if (vx > 0) PlayAnim("Run_Right");
            else if (vx < 0) PlayAnim("Run_Left");
            else PlayAnim(lastMoveX >= 0 ? "Run_Right" : "Run_Left");
        }
        else
            PlayAnim(lastMoveX >= 0 ? "Right_Idle" : "Left_Idle");
    }

    /* ─────────────────── 애니메이션 & RPC ─────────────────── */
    void PlayAnim(string anim)
    {
        if (currentMoveAnim == anim) return;
        animator.Play(anim);
        currentMoveAnim = anim;
        photonView.RPC(nameof(RPC_PlayAnim), RpcTarget.Others, anim);
    }
    [PunRPC] void RPC_PlayAnim(string anim) { animator.Play(anim); currentMoveAnim = anim; }

    /* ─────────────────── Behavior Tree ─────────────────── */
    INode BuildBehaviorTree()
    {
        return new SelectorNode(new List<INode>()
        {
            new SequenceNode(new List<INode>()
            {
                new ActionNode(CheckEnemyInAttackRange),
                new ActionNode(DoAttack),
            }),
            new SequenceNode(new List<INode>()
            {
                new ActionNode(CheckDetectEnemy),
                new ActionNode(MoveToEnemy),
            }),
            new ActionNode(WanderInsideSpawnArea),
        });
    }

    /* ───── 조건 노드 ───── */
    INode.NodeState CheckEnemyInAttackRange()
    {
        if (!canAttack || targetPlayer == null) return INode.NodeState.Failure;

        var pc = targetPlayer.GetComponent<PlayerController>();
        if (pc == null || pc.CurrentState == PlayerState.Death)
        { targetPlayer = null; return INode.NodeState.Failure; }

        return Vector3.Distance(transform.position, targetPlayer.position) < status.attackRange
            ? INode.NodeState.Success : INode.NodeState.Failure;
    }

    INode.NodeState CheckDetectEnemy()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDist = status.detectRange;
        Transform nearest = null;

        foreach (var p in players)
        {
            var pc = p.GetComponent<PlayerController>();
            if (pc == null || pc.CurrentState == PlayerState.Death) continue;

            float d = Vector3.Distance(transform.position, p.transform.position);
            if (d < minDist) { minDist = d; nearest = p.transform; }
        }

        targetPlayer = nearest;
        return nearest ? INode.NodeState.Success : INode.NodeState.Failure;
    }

    /* ───── 행동 노드 ───── */
    INode.NodeState MoveToEnemy()
    {
        if (!targetPlayer) return INode.NodeState.Failure;

        /* ★ Ranged 타입은 사정거리 안에 들어오면 추적 중단 */
        if (targeting && targeting.monsterType == MonsterType.Ranged)
        {
            float dist = Vector3.Distance(transform.position, targetPlayer.position);
            if (dist <= status.attackRange)
            {
                agent.isStopped = true;
                agent.ResetPath();
                return INode.NodeState.Failure;   // 이동 노드 종료 → 공격 시퀀스로
            }
        }

        agent.speed = status.chaseSpeed;
        agent.isStopped = false;
        agent.SetDestination(new Vector3(targetPlayer.position.x,
                                         transform.position.y,
                                         targetPlayer.position.z));
        return INode.NodeState.Running;
    }

    INode.NodeState DoAttack()
    {
        if (!canAttack || attackStrategy == null || !targetPlayer)
            return INode.NodeState.Failure;

        var pc = targetPlayer.GetComponent<PlayerController>();
        if (pc == null || pc.CurrentState == PlayerState.Death)
        { targetPlayer = null; ResetPrep(); return INode.NodeState.Failure; }

        if (isPreparingAttack)
        {
            attackPrepareTimer += Time.deltaTime;
            if (attackPrepareTimer >= status.waitCool)
            {
                isPreparingAttack = false; attackPrepareTimer = 0f;

                string atkAnim = targetPlayer.position.x >= transform.position.x ? "Attack_Right" : "Attack_Left";
                PlayAnim(atkAnim);

                attackStrategy.Attack(targetPlayer);
                isAttackAnimPlaying = true; attackAnimTime = 0f;
                canAttack = false; cooldownTimer = 0f;
                return INode.NodeState.Success;
            }

            PlayAnim(targetPlayer.position.x >= transform.position.x ? "Right_Idle" : "Left_Idle");
            agent.isStopped = true;
            return INode.NodeState.Running;
        }
        else
        {
            isPreparingAttack = true; attackPrepareTimer = 0f;
            agent.isStopped = true;
            PlayAnim(targetPlayer.position.x >= transform.position.x ? "Right_Idle" : "Left_Idle");
            return INode.NodeState.Running;
        }
    }
    void ResetPrep() { isPreparingAttack = false; attackPrepareTimer = 0f; }

    /* ───── Wander 노드 ───── */
    INode.NodeState WanderInsideSpawnArea()
    {
        if (!EnsureSpawnArea()) return INode.NodeState.Failure;

        if (isWaiting)
        {
            if ((waitTimer += Time.deltaTime) >= waitTime)
            { isWaiting = false; agent.isStopped = false; PickWanderPoint(); }
            return INode.NodeState.Running;
        }

        if (agent.pathPending) return INode.NodeState.Running;

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + .1f)
        {
            isWaiting = true; agent.isStopped = true;
            waitTime = Random.Range(3f, 5f); waitTimer = 0f;
        }
        return INode.NodeState.Running;
    }

    void PickWanderPoint()
    {
        if (!EnsureSpawnArea()) return;

        const int MAX_TRIES = 10;
        const float MIN_DIST = 1.5f;

        for (int i = 0; i < MAX_TRIES; i++)
        {
            Vector3 cand = spawnArea.GetRandomPointInsideArea();
            if (NavMesh.SamplePosition(cand, out var hit, 2f, NavMesh.AllAreas))
            {
                if (Vector3.Distance(transform.position, hit.position) < MIN_DIST) continue;
                wanderTarget = hit.position;
                agent.speed = status.wanderSpeed;
                agent.SetDestination(wanderTarget);
                return;
            }
        }
    }

    /* ───── 애니메이션 이벤트 ───── */
    public void OnAttackHitEvent()
    {
        if (targetPlayer && attackStrategy != null)
            attackStrategy.Attack(targetPlayer);
    }

    /* ───── IDamageable 구현 ───── */
    public void TakeDamage(float dmg)
    {
        photonView.RPC(nameof(DamageToMaster), RpcTarget.MasterClient, dmg);
    }

    [PunRPC]
    public void DamageToMaster(float dmg)
    {
        if (!PhotonNetwork.IsMasterClient || isDead) return;

        /* 쉴드 선감소 */
        float beforeShield = currentShield;
        if (currentShield > 0)
        {
            currentShield = Mathf.Max(0, currentShield - dmg);
            dmg = Mathf.Max(0, dmg - beforeShield);
            photonView.RPC(nameof(UpdateShield), RpcTarget.AllBuffered, currentShield);
        }

        /* HP 감소 */
        currentHP = Mathf.Max(0, currentHP - dmg);
        photonView.RPC(nameof(UpdateHP), RpcTarget.AllBuffered, currentHP);

        /* 피격 애니메이션 */
        if (currentShield <= 0)
            photonView.RPC(nameof(RPC_PlayAnim), RpcTarget.All, lastMoveX >= 0 ? "Right_Hit" : "Left_Hit");

        photonView.RPC(nameof(RPC_Flash), RpcTarget.All);
        SpawnDamageText(dmg);

        if (currentHP <= 0) Die();
    }

    /* ───── UI RPC ───── */
    [PunRPC]
    public void UpdateHP(float hp)
    {
        currentHP = hp;
        if (uiBar)
        {
            float n = currentHP / maxHP;
            uiBar.SetHP(n);
            uiBar.CheckThreshold(n, false);
        }
    }

    [PunRPC]
    public void UpdateShield(float shd)
    {
        currentShield = shd;
        if (uiBar)
        {
            float n = maxShield == 0 ? 0 : currentShield / maxShield;
            uiBar.SetShield(n);
            uiBar.CheckThreshold(n, true);
        }
    }

    /* ───── 피격 점멸 ───── */
    [PunRPC] void RPC_Flash() => StartCoroutine(CoFlash());
    IEnumerator CoFlash()
    {
        Color org = spriteRenderer.color;
        spriteRenderer.color = new Color(1f, 0.3f, 0.3f);
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = org;
    }

    /* ───── 데미지 숫자 ───── */
    void SpawnDamageText(float dmg)
    {
        if (!damageTextPrefab) return;
        var go = Instantiate(damageTextPrefab,
                             transform.position + Vector3.up * 1.5f,
                             Quaternion.identity);
        if (go.TryGetComponent(out TextMesh tm))
            tm.text = dmg.ToString();
    }

    /* ───── 사망 처리 ───── */
    void Die()
    {
        if (isDead) return;
        isDead = true;
        agent.isStopped = true;
        if (uiBar) Destroy(uiBar.gameObject);

        if (PhotonNetwork.IsMasterClient)
        {
            ActiveMonsterCount--;
            StageManager sm = Object.FindAnyObjectByType<StageManager>();
            sm?.AreAllMonstersCleared();
        }

        photonView.RPC(nameof(RPC_PlayAnim), RpcTarget.All,
                       lastMoveX >= 0 ? "Right_Death" : "Left_Death");
        StartCoroutine(CoDestroyLater());
    }

    IEnumerator CoDestroyLater()
    {
        yield return new WaitForSeconds(deathAnimDuration);
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
    }

    /* ───── Gizmos ───── */
    void OnDrawGizmosSelected()
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

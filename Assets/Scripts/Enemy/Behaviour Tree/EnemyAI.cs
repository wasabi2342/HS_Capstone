/*******************************************************
 * EnemyAI.cs – Shield 우선차감 + Hit-Stun 복귀 2025-04-28
 *******************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviourPun, IDamageable
{
    /* ───── Inspector ───── */
    public EnemyStatus status;                         // hp, damage, headOffset, maxShield…
    [SerializeField] private SpawnArea spawnArea;      // Wander 영역
    public GameObject damageTextPrefab;

    /* ───── Runtime ───── */
    public NavMeshAgent agent { get; private set; }
    public Animator animator;
    private SpriteRenderer sr;
    private DebuffController debuff;
    public DebuffController debuffController { get => debuff; private set => debuff = value; }
    private IMonsterAttack attackStrategy;
    private BehaviorTreeRunner bt;

    Transform targetPlayer;
    float lastMoveX = 1f;
    string currentAnim = "";

    bool canAttack = true; float atkCoolT;
    bool atkAnim; float atkT;
    bool prepping; float prepT;
    float atkDur = .7f;

    bool waiting; float waitT, waitDur;
    Vector3 wanderTarget;
    Vector3 lastTargetPos;
    float updateTargetThreshold = 0.5f;
    float detectCooldown = 0.5f; 
    float detectTimer = 0f;
    float destinationUpdateInterval = 0.2f;
    float destinationUpdateTimer = 0f;

    /* Hit-Stun */
    bool stunned;
    Vector3 storedDest;
    bool hadPath;
    private Vector3 knockbackVelocity;
    private float knockbackTimer;
    private const float KNOCKBACK_DURATION = 0.1f;
    [SerializeField] private GameObject bloodEffectPrefab_Left;
    [SerializeField] private GameObject bloodEffectPrefab_Right;


    /* HP & Shield */
    float maxHP, hp;
    float maxShield, shield;          // 실드
    bool dead;
    const float DIE_DUR = 1.5f;

    public static int ActiveMonsterCount = 0;

    /* UI */
    UIEnemyHealthBar uiBar;

    /* ───────────────────────── Awake ───────────────────────── */
    void Awake()
    {
        /* 컴포넌트 */
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        debuff = GetComponent<DebuffController>();
        attackStrategy = GetComponent<IMonsterAttack>();

        agent.updateRotation = false;
        agent.stoppingDistance = .1f;
        agent.avoidancePriority = Random.Range(10, 90);

        /* 스탯 초기화 */
        maxHP = hp = status.hp;
        maxShield = shield = status.maxShield;
        atkDur = status.animDuration;

        if (PhotonNetwork.IsMasterClient) ActiveMonsterCount++;

        EnsureSpawnArea();

        /* HP / Shield 바 */
        var prefab = Resources.Load<GameObject>("UIEnemyHealthBar");
        if (prefab != null)
        {
            var go = Instantiate(prefab);
            uiBar = go.GetComponent<UIEnemyHealthBar>();
            uiBar.Init(transform, Vector3.up * status.headOffset);
            uiBar.SetHP(1f);
            uiBar.SetShield(maxShield > 0 ? 1f : 0f);
        }

        /* Behavior Tree */
        bt = new BehaviorTreeRunner(BuildBT());

        /* 클라이언트 제어 분기 */
        if (PhotonNetwork.InRoom && (!photonView.IsMine || !PhotonNetwork.IsMasterClient))
            agent.enabled = false;
    }

    /* ───────── SpawnArea 확보 ───────── */
    void OnTransformParentChanged() => EnsureSpawnArea();
    bool EnsureSpawnArea()
    {
        if (spawnArea) return true;
        spawnArea = GetComponentInParent<SpawnArea>();
        return spawnArea != null;
    }
    public void SetSpawnArea(SpawnArea sa)
    {
        spawnArea = sa;
        if (agent && agent.enabled && agent.isOnNavMesh && !dead)
            PickWanderPoint();
    }

    /* ───────────────────────── Update ───────────────────────── */
    void Update()
    {
        if (dead || stunned || !agent.enabled) return;
        if (knockbackTimer > 0f)
        {
            float knockbackProgress = 1f - (knockbackTimer / KNOCKBACK_DURATION);
            transform.position += knockbackVelocity * (1f - knockbackProgress) * Time.deltaTime;
            knockbackTimer -= Time.deltaTime;
            return;
        }
        if (prepping || atkAnim)
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
        }
        /* 공격 쿨다운 동안 정지 + Idle */
        if (!canAttack && !prepping && !atkAnim)
        {
            agent.isStopped = true;
            agent.ResetPath();
            PlayAnim(lastMoveX >= 0 ? "Right_Idle" : "Left_Idle");

            atkCoolT += Time.deltaTime;
            if (atkCoolT >= status.attackCool) { canAttack = true; atkCoolT = 0f; }
            return;
        }

        bt.Operate();   // Behavior Tree 실행

        /* 공격 애니 진행 중 */
        if (atkAnim)
        {
            atkT += Time.deltaTime;
            if (atkT >= atkDur)
            {
                atkAnim = false;
                PlayAnim(lastMoveX >= 0 ? "Right_Idle" : "Left_Idle");
            }
            return;
        }

        /* 이동/Idle 애니 */
        float vx = agent.velocity.x;
        if (!Mathf.Approximately(vx, 0f)) lastMoveX = vx;

        if (!agent.isStopped)
        {
            if (vx > 0) PlayAnim("Run_Right");
            else if (vx < 0) PlayAnim("Run_Left");
            else PlayAnim(lastMoveX >= 0 ? "Run_Right" : "Run_Left");
        }
        else PlayAnim(lastMoveX >= 0 ? "Right_Idle" : "Left_Idle");
    }

    /* ───────── Behavior Tree 구축 ───────── */
    INode BuildBT() => new SelectorNode(new List<INode>()
    {
        new SequenceNode(new List<INode>()
        {
            new ActionNode(CheckEnemyInRange),
            new ActionNode(DoAttack),
        }),
        new SequenceNode(new List<INode>()
        {
            new ActionNode(DetectEnemy),
            new ActionNode(MoveToEnemy),
        }),
        new ActionNode(WanderInsideArea),
    });

    /* ─── BT 노드들 ─── */
    INode.NodeState CheckEnemyInRange()
    {
        if (!canAttack || targetPlayer == null) return INode.NodeState.Failure;
        return Vector3.Distance(transform.position, targetPlayer.position) < status.attackRange
               ? INode.NodeState.Success : INode.NodeState.Failure;
    }

    INode.NodeState DetectEnemy()
    {
        detectTimer -= Time.deltaTime;
        if (detectTimer > 0f && targetPlayer != null)
            return INode.NodeState.Success; // 이미 타겟이 있을 때는 그대로 유지

        detectTimer = detectCooldown;

        Transform near = null; float min = status.detectRange;
        foreach (var p in GameObject.FindGameObjectsWithTag("Player"))
        {
            float d = Vector3.Distance(transform.position, p.transform.position);
            if (d < min) { min = d; near = p.transform; }
        }
        targetPlayer = near;
        return near ? INode.NodeState.Success : INode.NodeState.Failure;
    }

    INode.NodeState MoveToEnemy()
    {
        if (!targetPlayer) return INode.NodeState.Failure;

        agent.isStopped = false;
        agent.speed = status.chaseSpeed;

        destinationUpdateTimer -= Time.deltaTime;

        if (destinationUpdateTimer <= 0f)
        {
            destinationUpdateTimer = destinationUpdateInterval;

            if (Vector3.Distance(lastTargetPos, targetPlayer.position) > updateTargetThreshold)
            {
                agent.SetDestination(new Vector3(targetPlayer.position.x, transform.position.y, targetPlayer.position.z));
                lastTargetPos = targetPlayer.position;
            }
        }

        return INode.NodeState.Running;
    }

    INode.NodeState DoAttack()
    {
        if (!canAttack || attackStrategy == null || !targetPlayer)
        {
            ResetAttackState();
            return INode.NodeState.Failure;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

        // 플레이어가 공격 범위를 벗어나면 공격 준비를 취소하고 초기화
        if (distanceToPlayer > status.attackRange)
        {
            ResetAttackState();
            targetPlayer = null; // 타겟도 초기화
            agent.isStopped = false;
            return INode.NodeState.Failure;
        }

        if (prepping)
        {
            prepT += Time.deltaTime;

            if (prepT >= status.waitCool)
            {
                prepping = false;
                prepT = 0f;
                string anim = targetPlayer.position.x >= transform.position.x ? "Attack_Right" : "Attack_Left";
                PlayAnim(anim);
                attackStrategy.Attack(targetPlayer);

                atkAnim = true;
                atkT = 0f;
                canAttack = false;
                atkCoolT = 0f;

                return INode.NodeState.Success;
            }

            lastMoveX = targetPlayer.position.x >= transform.position.x ? 1f : -1f;
            PlayAnim(lastMoveX >= 0 ? "Right_Idle" : "Left_Idle");
            return INode.NodeState.Running;
        }
        else if (distanceToPlayer <= status.attackRange)
        {
            prepping = true;
            prepT = 0f;
            agent.isStopped = true;
            agent.ResetPath();
            return INode.NodeState.Running;
        }

        ResetAttackState();
        return INode.NodeState.Failure;
    }

    // 공격 상태 초기화 메소드 추가
    void ResetAttackState()
    {
        prepping = false;
        prepT = 0f;
        atkAnim = false;
        atkT = 0f;
        agent.isStopped = false;
        agent.ResetPath();
    }



    INode.NodeState WanderInsideArea()
    {
        if (!EnsureSpawnArea()) return INode.NodeState.Failure;

        if (waiting)
        {
            if ((waitT += Time.deltaTime) >= waitDur)
            { waiting = false; agent.isStopped = false; PickWanderPoint(); }
            return INode.NodeState.Running;
        }

        if (agent.pathPending) return INode.NodeState.Running;

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + .1f)
        {
            waiting = true; agent.isStopped = true;
            waitDur = Random.Range(3f, 5f); waitT = 0f;
        }
        return INode.NodeState.Running;
    }

    void PickWanderPoint()
    {
        if (!EnsureSpawnArea()) return;
        bool found = false;
        for (int i = 0; i < 10; i++)
        {
            Vector3 p = spawnArea.GetRandomPointInsideArea();
            if (NavMesh.SamplePosition(p, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                wanderTarget = hit.position;
                agent.speed = status.wanderSpeed;
                agent.SetDestination(wanderTarget);
                found = true;
                break;
            }
        }
        if (!found)
        {
            // 실패했을 때, SpawnArea 근처 랜덤 이동
            Vector3 fallback = spawnArea.transform.position + Random.insideUnitSphere * 2f;
            fallback.y = transform.position.y;
            agent.SetDestination(fallback);
        }
    }
    /* ───────── Damage & Shield ───────── */
    public void TakeDamage(float dmg, AttackerType attackerType = AttackerType.Default) =>
        photonView.RPC(nameof(DamageToMaster), RpcTarget.MasterClient, dmg);

    [PunRPC]
    public void DamageToMaster(float dmg)
    {
        if (!PhotonNetwork.IsMasterClient || dead) return;

        /* 실드 먼저 차감 */
        float before = shield;
        if (shield > 0)
        {
            shield = Mathf.Max(0, shield - dmg);
            dmg = Mathf.Max(0, dmg - before);
            photonView.RPC(nameof(UpdateShield), RpcTarget.AllBuffered, shield / maxShield);

            if (shield == 0 && before > 0)
                photonView.RPC(nameof(RPC_ShieldBreakFx), RpcTarget.All);
        }

        /* HP 차감 */
        hp = Mathf.Max(0, hp - dmg);
        photonView.RPC(nameof(UpdateHP), RpcTarget.AllBuffered, hp / maxHP);

        // 현재 facing 방향 결정
        bool faceRight = lastMoveX >= 0f;
        // blood effect용 RPC: 위치 + facing 정보 전달
        photonView.RPC(nameof(RPC_SpawnBloodEffect), RpcTarget.All, transform.position, faceRight);

        /* 피격 연출 (실드가 없을 때만 경직) */
        if (shield <= 0)
        {
            photonView.RPC(nameof(RPC_PlayAnim), RpcTarget.All,
                           lastMoveX >= 0 ? "Right_Hit" : "Left_Hit");

            photonView.RPC(nameof(RPC_Stun), RpcTarget.All, status.hitRecoverTime);
        }

        photonView.RPC(nameof(RPC_Flash), RpcTarget.All);
        SpawnDamageText(dmg);

        if (hp <= 0) Die();
        else ApplyKnockback();  // 죽지 않은 경우에만 넉백

    }
    // ───────── Helper / RPC ─────────
    [PunRPC]
    void RPC_SpawnBloodEffect(Vector3 pos, bool faceRight)
    {
        // Facing 플래그로 프리팹 선택
        GameObject prefabToUse = faceRight ? bloodEffectPrefab_Right : bloodEffectPrefab_Left;
        if (prefabToUse == null)
        {
            Debug.LogError("[EnemyAI] BloodEffect prefab is null!");
            return;
        }

        // 이펙트 생성
        GameObject fx = Instantiate(prefabToUse, pos + Vector3.up * 1f, Quaternion.identity);

        // 애니메이터 있으면 랜덤 애니 재생
        if (fx.TryGetComponent<Animator>(out var fxAnim))
        {
            string[] animNames = { "BloodEffect_1", "BloodEffect_2", "BloodEffect_3" };
            fxAnim.Play(animNames[Random.Range(0, animNames.Length)]);
        }

        Destroy(fx, 2f);
    }

    void ApplyKnockback()
    {
        if (targetPlayer == null) return;

        Vector3 dir = (transform.position - targetPlayer.position).normalized;
        dir.y = 0f; // 수평으로만 밀리게
        knockbackVelocity = dir * 4.5f; // 밀리는 속도
        knockbackTimer = KNOCKBACK_DURATION;
    }

    /* ───────── Stun / Recover ───────── */
    [PunRPC]
    void RPC_Stun(float t)
    {
        if (stunned || dead) return;
        StartCoroutine(CoStun(t));
    }
    IEnumerator CoStun(float t)
    {
        stunned = true;

        /* 추격 상태 보존 */
        hadPath = agent.hasPath;
        if (hadPath) storedDest = agent.destination;

        agent.isStopped = true;
        agent.ResetPath();

        yield return new WaitForSeconds(t);

        stunned = false;
        if (dead) yield break;
        agent.isStopped = false;

        /* 즉시 재추격 */
        if (hadPath && targetPlayer)
            agent.SetDestination(storedDest);

        PlayAnim(lastMoveX >= 0 ? "Right_Idle" : "Left_Idle");
    }

    /* ───────── UI RPC ───────── */
    [PunRPC] public void UpdateHP(float n) => uiBar?.SetHP(n);
    [PunRPC] public void UpdateShield(float n) => uiBar?.SetShield(n);
    [PunRPC] void RPC_ShieldBreakFx() { /* 파티클·사운드 */ }

    /* ───────── Death & Cleanup ───────── */
    void Die()
    {
        if (dead) return;
        dead = true; agent.isStopped = true;

        photonView.RPC(nameof(RPC_DestroyHPBar), RpcTarget.AllBuffered);
        photonView.RPC(nameof(RPC_PlayAnim), RpcTarget.All,
                       lastMoveX >= 0 ? "Right_Death" : "Left_Death");

        if (PhotonNetwork.IsMasterClient)
        {
            ActiveMonsterCount--;
            StartCoroutine(CoDestroyLater());
        }
    }
    [PunRPC] void RPC_DestroyHPBar() { if (uiBar) Destroy(uiBar.gameObject); }
    IEnumerator CoDestroyLater()
    {
        yield return new WaitForSeconds(DIE_DUR);
        if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(gameObject);
    }

    /* ───────── Helper / RPC ───────── */
    void PlayAnim(string anim)
    {
        if (currentAnim == anim) return;
        animator.Play(anim); currentAnim = anim;
        photonView.RPC(nameof(RPC_PlayAnim), RpcTarget.Others, anim);
    }
    [PunRPC] void RPC_PlayAnim(string a) { animator.Play(a); currentAnim = a; }
    [PunRPC] void RPC_Flash() => StartCoroutine(CoFlash());
    IEnumerator CoFlash()
    {
        var c = sr.color; sr.color = new Color(1, .3f, .3f);
        yield return new WaitForSeconds(.1f); sr.color = c;
    }

    void SpawnDamageText(float dmg)
    {
        if (!damageTextPrefab) return;
        var g = Instantiate(damageTextPrefab,
                            transform.position + Vector3.up * 1.5f, Quaternion.identity);
        if (g.TryGetComponent(out TextMesh tm)) tm.text = dmg.ToString("F0");
    }

    /* ───────── Gizmos ───────── */
    void OnDrawGizmos()
    {
        if (agent && agent.enabled && agent.isOnNavMesh && agent.velocity.sqrMagnitude > .01f)
        {
            Gizmos.color = Color.blue;
            Vector3 d = agent.velocity.normalized * 1.2f;
            Gizmos.DrawLine(transform.position, transform.position + d);
            Gizmos.DrawSphere(transform.position + d, .07f);
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, .25f);
        if (wanderTarget != Vector3.zero)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(wanderTarget, .25f);
            Gizmos.DrawLine(transform.position, wanderTarget);
        }
    }
}

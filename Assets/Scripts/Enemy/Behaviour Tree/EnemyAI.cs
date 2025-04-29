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

    bool canAttack = true;
    float atkCoolT;

    bool atkAnim;
    bool prepping;
    float prepT;

    bool waiting;
    float waitT, waitDur;
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
    private const float KNOCKBACK_DURATION = 0.5f;
    [SerializeField] private GameObject bloodEffectPrefab_Left;
    [SerializeField] private GameObject bloodEffectPrefab_Right;

    [SerializeField] private GameObject slashEffectPrefab_Left;
    [SerializeField] private GameObject slashEffectPrefab_Right;
    /* HP & Shield */
    float maxHP, hp;
    float maxShield, shield;
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

        // 1) 넉백 처리
        if (knockbackTimer > 0f)
        {
            float knockbackProgress = 1f - (knockbackTimer / KNOCKBACK_DURATION);
            transform.position += knockbackVelocity * (1f - knockbackProgress) * Time.deltaTime;
            knockbackTimer -= Time.deltaTime;
            return;
        }

        // 2) 공격 준비(prepping) 또는 공격 애니 진행 중일 때 멈춤
        if (prepping || atkAnim)
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
        }

        // 3) 공격 쿨다운 처리
        if (!canAttack && !prepping && !atkAnim)
        {
            atkCoolT += Time.deltaTime;
            if (atkCoolT >= status.attackCool)
            {
                canAttack = true;
                atkCoolT = 0f;
            }
            // 이동/Idle 분기까지 내려가서 자연스럽게 모션 전환
        }

        // 4) Behavior Tree 실행
        bt.Operate();

        // 5) 런타임 애니 상태 종료 감지 (이제 Animation Event로도 처리되지만, 안전망으로 유지)
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if ((stateInfo.IsName("Right_Attack") || stateInfo.IsName("Left_Attack")))
        {
            if (stateInfo.normalizedTime < 1f)
            {
                return;
            }
            // 애니가 완전히 끝난 시점
            atkAnim = false;
            PlayAnim(lastMoveX >= 0 ? "Right_Idle" : "Left_Idle");
            ResetAttackState();
        }

        // 6) 이동 방향 갱신
        if (agent.hasPath && !agent.isStopped)
        {
            Vector3 dv = agent.desiredVelocity;
            if (dv.sqrMagnitude > 0.01f)
            {
                lastMoveX = Mathf.Abs(dv.x) > 0.1f
                    ? Mathf.Sign(dv.x)
                    : (agent.destination.x - transform.position.x >= 0 ? 1f : -1f);
            }
        }

        // 7) 이동/Idle 애니메이션 분기
        if (agent.hasPath && agent.remainingDistance > agent.stoppingDistance + .1f)
        {
            PlayAnim(lastMoveX >= 0 ? "Right_Run" : "Left_Run");
        }
        else
        {
            PlayAnim(lastMoveX >= 0 ? "Right_Idle" : "Left_Idle");
        }
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
            return INode.NodeState.Success;

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
        if (Vector3.Distance(transform.position, targetPlayer.position) <= status.attackRange)
            return INode.NodeState.Failure;

        agent.isStopped = false;
        agent.speed = status.chaseSpeed;

        destinationUpdateTimer -= Time.deltaTime;
        if (destinationUpdateTimer <= 0f)
        {
            destinationUpdateTimer = destinationUpdateInterval;
            if (Vector3.Distance(lastTargetPos, targetPlayer.position) > updateTargetThreshold)
            {
                agent.SetDestination(new Vector3(
                    targetPlayer.position.x,
                    transform.position.y,
                    targetPlayer.position.z));
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

        float dist = Vector3.Distance(transform.position, targetPlayer.position);
        if (dist > status.attackRange)
        {
            ResetAttackState();
            targetPlayer = null;
            agent.isStopped = false;
            return INode.NodeState.Failure;
        }

        if (!prepping)
        {
            prepping = true;
            prepT = 0f;
            agent.isStopped = true;
            agent.ResetPath();
            return INode.NodeState.Running;
        }
        else
        {
            prepT += Time.deltaTime;
            if (prepT >= status.waitCool)
            {
                prepping = false;
                prepT = 0f;
                // 공격 애니 실행
                lastMoveX = (targetPlayer.position.x >= transform.position.x) ? 1f : -1f;
                string anim = lastMoveX >= 0 ? "Right_Attack" : "Left_Attack";
                PlayAnim(anim);
                attackStrategy.Attack(targetPlayer);

                atkAnim = true;
                canAttack = false;
                atkCoolT = 0f;
                return INode.NodeState.Success;
            }
            // 준비 중에는 Idle 유지
            PlayAnim(lastMoveX >= 0 ? "Right_Idle" : "Left_Idle");
            return INode.NodeState.Running;
        }
    }

    // 공격 상태 초기화
    void ResetAttackState()
    {
        prepping = false;
        atkAnim = false;
        agent.isStopped = false;
        agent.ResetPath();
        PlayAnim(lastMoveX >= 0 ? "Right_Idle" : "Left_Idle");
    }

    // ───────── Animation Event 콜백 ─────────
    // 애니메이터에서 Attack 클립 끝에 Animation Event로 이 함수를 호출하세요.
    public void OnAttackAnimationEndEvent()
    {
        // 모든 클라이언트에서 공격 상태 해제 및 Idle 전환
        photonView.RPC(nameof(RPC_HandleAttackEnd), RpcTarget.All);
    }

    [PunRPC]
    void RPC_HandleAttackEnd()
    {
        atkAnim = false;
        ResetAttackState();
    }

    INode.NodeState WanderInsideArea()
    {
        if (!EnsureSpawnArea()) return INode.NodeState.Failure;

        if (waiting)
        {
            if ((waitT += Time.deltaTime) >= waitDur)
            {
                waiting = false;
                agent.isStopped = false;
                PickWanderPoint();
            }
            return INode.NodeState.Running;
        }
        if (agent.pathPending) return INode.NodeState.Running;

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + .1f)
        {
            waiting = true;
            agent.isStopped = true;
            waitDur = Random.Range(3f, 5f);
            waitT = 0f;
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

        float beforeShield = shield;
        bool shieldWasPresent = shield > 0f;
        if (shield > 0f)
        {
            shield = Mathf.Max(0f, shield - dmg);
            dmg = Mathf.Max(0f, dmg - beforeShield);
            photonView.RPC(nameof(UpdateShield), RpcTarget.AllBuffered, shield / maxShield);

            if (shield == 0f && beforeShield > 0f)
                photonView.RPC(nameof(RPC_ShieldBreakFx), RpcTarget.All);
        }

        // Stun 및 넉백 동기화
        Vector3 attackerPosition = targetPlayer ? targetPlayer.position : transform.position;
        photonView.RPC(nameof(RPC_ApplyKnockback), RpcTarget.All, attackerPosition, shieldWasPresent);

        // HP 감소
        hp = Mathf.Max(0f, hp - dmg);
        photonView.RPC(nameof(UpdateHP), RpcTarget.AllBuffered, hp / maxHP);

        // Blood Effect 동기화
        bool faceRight = (attackerPosition.x >= transform.position.x);
        photonView.RPC(nameof(RPC_SpawnBloodEffect), RpcTarget.All, transform.position, faceRight);

        photonView.RPC(nameof(RPC_PlayAnim), RpcTarget.All,
                       lastMoveX >= 0 ? "Right_Hit" : "Left_Hit");
        photonView.RPC(nameof(RPC_Flash), RpcTarget.All);
        SpawnDamageText(dmg);

        if (hp <= 0f) Die();
    }

    [PunRPC]
    void RPC_ApplyKnockback(Vector3 attackerPosition, bool shieldWasPresent)
    {
        float baseStrength = 5f;
        float strength = shieldWasPresent ? baseStrength * 0.75f : baseStrength;

        Vector3 dir = (transform.position - attackerPosition).normalized;
        dir.y = 0f;
        knockbackVelocity = dir * strength;
        knockbackTimer = KNOCKBACK_DURATION;
    }

    [PunRPC]
    void RPC_SpawnBloodEffect(Vector3 pos, bool faceRight)
    {
        GameObject bloodFx = Instantiate(
            faceRight ? bloodEffectPrefab_Right : bloodEffectPrefab_Left,
            pos + Vector3.up * 1f,
            Quaternion.identity, null);
        if (bloodFx.TryGetComponent<Animator>(out var bloodFXAnim))
        {
            string[] animNames = { "BloodEffect_1", "BloodEffect_2", "BloodEffect_3" };
            bloodFXAnim.Play(animNames[Random.Range(0, animNames.Length)]);
        }
        Destroy(bloodFx, 2f);

        GameObject slashFX = Instantiate(
            faceRight ? slashEffectPrefab_Right : slashEffectPrefab_Left,
            pos,
            Quaternion.identity, null);
        if (slashFX.TryGetComponent<Animator>(out var slashFXAnim))
        {
            string[] animNames = { "Slash1", "Slash2", "Slash3", "Slash4" };
            slashFXAnim.Play(animNames[Random.Range(0, animNames.Length)]);
        }
        Destroy(slashFX, 0.4f);
    }

    [PunRPC]
    void RPC_Stun(float t)
    {
        if (stunned || dead) return;
        StartCoroutine(CoStun(t));
    }
    IEnumerator CoStun(float t)
    {
        stunned = true;
        hadPath = agent.hasPath;
        if (hadPath) storedDest = agent.destination;

        agent.isStopped = true;
        agent.ResetPath();
        yield return new WaitForSeconds(t);

        stunned = false;
        if (dead) yield break;
        agent.isStopped = false;
        if (hadPath && targetPlayer)
            agent.SetDestination(storedDest);
        PlayAnim(lastMoveX >= 0 ? "Right_Idle" : "Left_Idle");
    }

    [PunRPC] public void UpdateHP(float n) => uiBar?.SetHP(n);
    [PunRPC] public void UpdateShield(float n) => uiBar?.SetShield(n);
    [PunRPC] void RPC_ShieldBreakFx() { /* 파티클·사운드 */ }

    void Die()
    {
        if (dead) return;
        dead = true;
        agent.isStopped = true;
        photonView.RPC(nameof(RPC_DestroyHPBar), RpcTarget.AllBuffered);
        photonView.RPC(nameof(RPC_PlayAnim), RpcTarget.All,
                       lastMoveX >= 0 ? "Right_Death" : "Left_Death");

        if (PhotonNetwork.IsMasterClient)
        {
            ActiveMonsterCount--;
            if (ActiveMonsterCount == 0)
            {
                StageManager.Instance.OnAllMonsterCleared();
            }
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
        animator.Play(anim);
        currentAnim = anim;
        photonView.RPC(nameof(RPC_PlayAnim), RpcTarget.Others, anim);
    }
    [PunRPC]
    void RPC_PlayAnim(string a)
    {
        animator.Play(a);
        currentAnim = a;
    }
    [PunRPC] void RPC_Flash() => StartCoroutine(CoFlash());
    IEnumerator CoFlash()
    {
        var c = sr.color;
        sr.color = new Color(1, .3f, .3f);
        yield return new WaitForSeconds(.1f);
        sr.color = c;
    }

    void SpawnDamageText(float dmg)
    {
        if (!damageTextPrefab) return;
        var g = Instantiate(damageTextPrefab,
                            transform.position + Vector3.up * 1.5f,
                            Quaternion.identity);
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

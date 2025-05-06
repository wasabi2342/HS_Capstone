using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

[RequireComponent(typeof(NavMeshAgent), typeof(PhotonView))]
public class EnemyFSM : MonoBehaviourPun, IPunObservable, IDamageable
{
    /* ───────── Components ───────── */
    public NavMeshAgent Agent { get; private set; }
    public Animator Anim { get; private set; }
    PhotonView pv;

    /* ───────── Data ───────── */
    [SerializeField] EnemyStatusSO enemyStatus;
    public EnemyStatusSO EnemyStatusRef => enemyStatus;

    /* ───────── Status ───────── */
    float maxHP, hp;
    float maxShield, shield;
    public float currentHP => hp;

    /* ───────── Facing ───────── */
    float lastMoveX = 1f;                       // +1 ⇒ Right , -1 ⇒ Left
    public float CurrentFacing => lastMoveX;
    public void ForceFacing(float s) => lastMoveX = s >= 0 ? 1f : -1f;

    /* ───────── UI ───────── */
    UIEnemyHealthBar uiHP;                      // ★ EnemyAI 식 Resources 스폰

    /* ───────── Visual / FX ───────── */
    [Header("Visual / FX")]
    [SerializeField] SpriteRenderer sr;
    [SerializeField] GameObject bloodFxRight, bloodFxLeft;
    [SerializeField] GameObject slashFxRight, slashFxLeft;

    public SpriteRenderer SR => sr;
    public GameObject BloodFxRight => bloodFxRight;
    public GameObject BloodFxLeft => bloodFxLeft;
    public GameObject SlashFxRight => slashFxRight;
    public GameObject SlashFxLeft => slashFxLeft;

    /* ───────── Attack Helper ───────── */
    public IMonsterAttack AttackComponent { get; private set; }

    /* ───────── SpawnArea ───────── */
    SpawnArea spawnArea;
    public void SetSpawnArea(SpawnArea a) => spawnArea = a;
    public SpawnArea CurrentSpawnArea => spawnArea;

    /* ───────── FSM ───────── */
    readonly Dictionary<System.Type, IState> states = new();
    public IState CurrentState { get; private set; }
    EnemyState currentEnum;

    /* ───────── Runtime ───────── */
    public Transform Target { get; set; }
    public bool LastAttackSuccessful { get; set; }

    /* ───────── Net Lerp ───────── */
    Vector3 netPos; Quaternion netRot; float netFacing;
    const float NET_SMOOTH = 10f;

    /* ───────── Knock‑back ───────── */
    Vector3 knockVel; float knockTime;
    const float KNOCK_DUR = .5f;

    /* ───────── Debug 옵션 ───────── */
    [Header("Debug")] public bool debugMode = false;
    const float GIZMO_Y = .05f;

    /* ───────── Attack Alignment ───────── */
    [Header("Attack Alignment")] public float zAlignTolerance = .4f;

    /* ───────── Static Counter ───────── */
    public static int ActiveMonsterCount = 0;

    /* ───────────────────────── Unity Flow ───────────────────────── */
    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Anim = GetComponentInChildren<Animator>();
        pv = GetComponent<PhotonView>();

        Agent.updateRotation = false;
        Agent.updatePosition = Agent.updateRotation = PhotonNetwork.IsMasterClient;

        /* FSM 상태 등록 */
        states[typeof(WanderState)] = new WanderState(this);
        states[typeof(IdleState)] = new IdleState(this);
        states[typeof(ChaseState)] = new ChaseState(this);
        states[typeof(WaitCoolState)] = new WaitCoolState(this);
        states[typeof(AttackState)] = new AttackState(this);
        states[typeof(AttackCoolState)] = new AttackCoolState(this);
        states[typeof(HitState)] = new HitState(this);
        states[typeof(DeadState)] = new DeadState(this);

        /* 스탯 초기화 */
        maxHP = hp = enemyStatus.maxHealth;
        maxShield = shield = enemyStatus.maxShield;

        /* 캐싱 */
        AttackComponent = GetComponent<IMonsterAttack>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>(true);

        /* ★ EnemyAI 방식 HP/Shield UI 스폰 */
        var prefab = Resources.Load<GameObject>("UI/UIEnemyHealthBar");
        if (prefab)
        {
            var go = Instantiate(prefab);                       // 월드 공간에 독립 인스턴스
            uiHP = go.GetComponent<UIEnemyHealthBar>();
            float y = enemyStatus.headOffset;
            if (Mathf.Approximately(y, 0f) && sr)
                y = sr.bounds.size.y * 0.9f;                  
            uiHP.Init(transform, Vector3.up * y);
            uiHP.SetHP(1f);
            uiHP.SetShield(maxShield > 0 ? 1f : 0f);
        }

        if (PhotonNetwork.IsMasterClient) ActiveMonsterCount++;
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ApplyStatusToAgent();
            TransitionToState(typeof(WanderState));
        }
    }

    void Update()
    {
        /* 넉백 이동 */
        if (knockTime > 0f)
        {
            float t = Time.deltaTime;
            knockTime -= t;
            transform.position += knockVel * t;
        }

        /* FSM 실행 / 네트워크 보간 */
        if (PhotonNetwork.IsMasterClient) CurrentState?.Execute();
        else
        {
            transform.position = Vector3.Lerp(transform.position, netPos, Time.deltaTime * NET_SMOOTH);
            transform.rotation = Quaternion.Slerp(transform.rotation, netRot, Time.deltaTime * NET_SMOOTH);
            lastMoveX = netFacing;
        }

        UpdateFacingFromVelocity();
    }

    /* ────────────────────────── Facing / Anim ───────────────────── */
    void UpdateFacingFromVelocity()
    {
        if (PhotonNetwork.IsMasterClient &&
            Agent.enabled && Agent.velocity.sqrMagnitude > .0001f)
            lastMoveX = Agent.velocity.x >= 0 ? 1f : -1f;
    }

    public void PlayDirectionalAnim(string action)
    {
        string clip = (lastMoveX >= 0 ? "Right_" : "Left_") + action;
        if (Anim.GetCurrentAnimatorStateInfo(0).IsName(clip)) return;

        if (pv.IsMine)
            pv.RPC(nameof(RPC_PlayClip), RpcTarget.Others, clip);

        Anim.Play(clip, 0);
    }
    [PunRPC] void RPC_PlayClip(string c) => Anim.Play(c, 0);

    /* ────────────────────────── FSM 핼퍼 ───────────────────────── */
    public void TransitionToState(System.Type t)
    {
        if (!states.TryGetValue(t, out var next)) return;
        if (CurrentState?.GetType() == t) return;

        CurrentState?.Exit();
        CurrentState = next;
        CurrentState.Enter();
        currentEnum = TypeToEnum(t);
    }
    EnemyState TypeToEnum(System.Type t) => t switch
    {
        _ when t == typeof(WanderState) => EnemyState.Wander,
        _ when t == typeof(IdleState) => EnemyState.Idle,
        _ when t == typeof(ChaseState) => EnemyState.Chase,
        _ when t == typeof(WaitCoolState) => EnemyState.WaitCool,
        _ when t == typeof(AttackState) => EnemyState.Attack,
        _ when t == typeof(AttackCoolState) => EnemyState.AttackCool,
        _ when t == typeof(HitState) => EnemyState.Hit,
        _ when t == typeof(DeadState) => EnemyState.Dead,
        _ => EnemyState.Idle
    };
    System.Type EnumToType(EnemyState e) => e switch
    {
        EnemyState.Wander => typeof(WanderState),
        EnemyState.Idle => typeof(IdleState),
        EnemyState.Chase => typeof(ChaseState),
        EnemyState.WaitCool => typeof(WaitCoolState),
        EnemyState.Attack => typeof(AttackState),
        EnemyState.AttackCool => typeof(AttackCoolState),
        EnemyState.Hit => typeof(HitState),
        EnemyState.Dead => typeof(DeadState),
        _ => typeof(IdleState)
    };

    /* ────────────────────────── Detect Player ──────────────────── */
    float detT; const float DET_INT = .2f;
    public void DetectPlayer()
    {
        detT += Time.deltaTime;
        if (detT < DET_INT) return; detT = 0f;

        Collider[] cols = new Collider[8];
        int n = Physics.OverlapSphereNonAlloc(transform.position, enemyStatus.detectRange,
                                              cols, enemyStatus.playerLayerMask, QueryTriggerInteraction.Ignore);
        Transform closest = null; float minSq = float.PositiveInfinity;
        for (int i = 0; i < n; ++i)
            if (cols[i].CompareTag("Player"))
            {
                float d = (cols[i].transform.position - transform.position).sqrMagnitude;
                if (d < minSq) { minSq = d; closest = cols[i].transform; }
            }
        Target = closest;
    }

    /* ────────────────────────── IDamageable ─────────────────────── */
    public void TakeDamage(float damage, AttackerType t = AttackerType.Default)
        => pv.RPC(nameof(DamageToMaster), RpcTarget.MasterClient, damage);

    [PunRPC]
    public void DamageToMaster(float damage)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        /* 실드 → HP 차감 */
        if (shield > 0f)
        {
            float prev = shield;
            shield = Mathf.Max(0f, shield - damage);
            damage = Mathf.Max(0f, damage - prev);
            pv.RPC(nameof(UpdateShield), RpcTarget.AllBuffered, maxShield == 0 ? 0 : shield / maxShield);
            if (shield == 0f && prev > 0f)
                pv.RPC(nameof(RPC_ShieldBreakFx), RpcTarget.All);
        }

        hp = Mathf.Max(0f, hp - damage);
        pv.RPC(nameof(UpdateHP), RpcTarget.AllBuffered, hp / maxHP);

        /* 넉백 & FX */
        Vector3 atkPos = Target ? Target.position : transform.position;
        bool fromRight = atkPos.x < transform.position.x;
        pv.RPC(nameof(RPC_ApplyKnockback), RpcTarget.All, atkPos);
        pv.RPC(nameof(RPC_SpawnHitFx), RpcTarget.All, transform.position, fromRight);

        TransitionToState(hp <= 0f ? typeof(DeadState) : typeof(HitState));
    }

    /* ─ HP / Shield UI 동기화 ─ */
    [PunRPC]
    public void UpdateHP(float r)
    { if (uiHP) { uiHP.SetHP(r); uiHP.CheckThreshold(r, false); } }

    [PunRPC]
    public void UpdateShield(float r)
    { if (uiHP) { uiHP.SetShield(r); uiHP.CheckThreshold(r, true); } }

    /* ─ 넉백 / FX ─ */
    [PunRPC]
    void RPC_ApplyKnockback(Vector3 atkPos)
    {
        Vector3 dir = (transform.position - atkPos).normalized;
        dir.y = dir.z = 0f;
        knockVel = dir * enemyStatus.hitKnockbackStrength;
        knockTime = KNOCK_DUR;
    }
    [PunRPC]
    void RPC_SpawnHitFx(Vector3 pos, bool spawnRight)
    {
        GameObject b = spawnRight ? bloodFxRight : bloodFxLeft;
        GameObject s = spawnRight ? slashFxRight : slashFxLeft;
        OneShotFx(b, pos, .7f); OneShotFx(s, pos, .4f);
    }
    void OneShotFx(GameObject prefab, Vector3 pos, float life)
    {
        if (!prefab) return;
        var go = Instantiate(prefab, pos + Vector3.down * 3f, Quaternion.identity);
        Destroy(go, life);
    }
    [PunRPC]
    void RPC_ShieldBreakFx()
    {
        var fx = Resources.Load<GameObject>("ShieldBreakFx");
        if (fx) Destroy(Instantiate(fx, transform.position + Vector3.up * 1.2f, Quaternion.identity), 2f);
    }
    public void DestroyUI() { if (uiHP) Destroy(uiHP.gameObject); }

    /* ────────────────────────── Networking ─────────────────────── */
    public void OnPhotonSerializeView(PhotonStream s, PhotonMessageInfo i)
    {
        if (s.IsWriting)
        {
            s.SendNext((int)currentEnum);
            s.SendNext(transform.position);
            s.SendNext(transform.rotation);
            s.SendNext(lastMoveX);
        }
        else
        {
            var inc = (EnemyState)(int)s.ReceiveNext();
            netPos = (Vector3)s.ReceiveNext();
            netRot = (Quaternion)s.ReceiveNext();
            netFacing = (float)s.ReceiveNext();
            if (inc != currentEnum) TransitionToState(EnumToType(inc));
        }
    }

    /* ────────────────────────── 기타 유틸 ──────────────────────── */
    void ApplyStatusToAgent()
    {
        Agent.speed = enemyStatus.moveSpeed;
        Agent.stoppingDistance = enemyStatus.attackRange;
        Agent.enabled = true;
    }
    public float GetZDiffAbs() => Target ? Mathf.Abs(Target.position.z - transform.position.z) : float.PositiveInfinity;
    public float GetTarget2DDistSq()
    {
        if (!Target) return float.PositiveInfinity;
        Vector3 a = Target.position; Vector3 b = transform.position;
        a.y = b.y = 0f; return (a - b).sqrMagnitude;
    }
    public bool IsTargetInAttackRange()
        => GetTarget2DDistSq() <= enemyStatus.attackRange * enemyStatus.attackRange;
    public bool IsAlignedAndInRange()
        => IsTargetInAttackRange() && GetZDiffAbs() <= zAlignTolerance;

    /* ───────── Scene Gizmo ───────── */
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!debugMode || enemyStatus == null) return;
        Gizmos.color = new Color(1f, .9f, 0f, .4f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * GIZMO_Y, enemyStatus.detectRange);
        Gizmos.color = new Color(1f, 0f, 0f, .4f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * GIZMO_Y, enemyStatus.attackRange);
    }
#endif
}

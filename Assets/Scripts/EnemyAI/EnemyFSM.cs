using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using Unity.AppUI.UI;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent), typeof(PhotonView))]
public class EnemyFSM : MonoBehaviourPun, IPunObservable, IDamageable
{
    /* ───────── Components ───────── */
    public NavMeshAgent Agent { get; private set; }
    public Animator Anim { get; private set; }
    public DebuffController debuff;
    PhotonView pv;

    /* ───────── Data ───────── */
    [SerializeField] EnemyStatusSO enemyStatus;
    public EnemyStatusSO EnemyStatusRef => enemyStatus;
    public Vector3 spawnPosition;        // Start()에서 기록


    /* ───────── Status ───────── */
    float maxHP, hp;    
    public float currentHP => hp;
    float maxShield, shield;
    private bool isDead;
    /* ────────── Detect ──────────── */
    [SerializeField] LayerMask playerMask;      // Player 레이어만
    [SerializeField] LayerMask servantMask;     // Servant 레이어만

    /* Facing */
    float lastMoveX = 1f;
    public float CurrentFacing => lastMoveX;
    public void ForceFacing(float s) => lastMoveX = s >= 0 ? 1f : -1f;

    /* ★ 추가: 플레이어 기준 좌/우 선호 라인  */
    [HideInInspector] public float preferredSide = 0f;   // -1 왼쪽, +1 오른쪽
    private Vector3 lastHitPos;
    public Vector3 LastHitPos => lastHitPos; // 공격자 위치

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
    public IMonsterAttack[] AttackPatterns { get; private set; }
    public IMonsterAttack AttackComponent { get; private set; }
    int attackIdx = -1;

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
        debuff = GetComponent<DebuffController>();
        Agent.updatePosition = PhotonNetwork.IsMasterClient;
        Agent.updateRotation = false;
        Agent.updateUpAxis = false;  // 2D 평면일 땐 선택

        /* FSM 상태 등록 */
        states[typeof(WanderState)] = new WanderState(this);
        states[typeof(IdleState)] = new IdleState(this);
        states[typeof(ChaseState)] = new ChaseState(this);
        states[typeof(DetourState)] = new DetourState(this);
        states[typeof(ReturnState)] = new ReturnState(this);
        states[typeof(WaitCoolState)] = new WaitCoolState(this);
        states[typeof(AttackState)] = new AttackState(this);
        states[typeof(AttackCoolState)] = new AttackCoolState(this);
        states[typeof(HitState)] = new HitState(this);
        states[typeof(DeadState)] = new DeadState(this);

        /* 스탯 초기화 */
        maxHP = hp = enemyStatus.maxHealth;
        maxShield = shield = enemyStatus.maxShield;

        /* 캐싱 */
        AttackPatterns = GetComponents<IMonsterAttack>();
        AttackComponent = AttackPatterns.Length > 0 ? AttackPatterns[0] : null;
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>(true);

        /* ★ EnemyAI 방식 HP/Shield UI 스폰 */
        var prefab = Resources.Load<GameObject>("UI/UIEnemyHealthBar");
        if (prefab)
        {
            var go = Instantiate(prefab);                       // 월드 공간에 독립 인스턴스
            uiHP = go.GetComponent<UIEnemyHealthBar>();
            float y = enemyStatus.headOffset;
            if (Mathf.Approximately(y, 0f) && sr)
                y = sr.bounds.size.y;                  
            uiHP.Init(transform, Vector3.up * y);
            uiHP.SetHP(1f);
            uiHP.SetShield(maxShield > 0 ? 1f : 0f);
        }
         
        if (PhotonNetwork.IsMasterClient) ActiveMonsterCount++;


        enemyStatus = Instantiate(enemyStatus);
        int pCnt = PhotonNetwork.CurrentRoom.PlayerCount;
        var diff = DifficultyManager.Instance;
        maxHP = hp = enemyStatus.maxHealth * diff.HpMul(pCnt);
        enemyStatus.attackDamage *= diff.AtkMul(pCnt);
        maxShield = shield = enemyStatus.maxShield * diff.ShieldMul(pCnt);


    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ApplyStatusToAgent();
            spawnPosition = transform.position;
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

        /* FSM 실행 / 네트워크 보간 */
        if (PhotonNetwork.IsMasterClient) 
        {
            CurrentState?.Execute();
            
            // 체력이 0 이하이고 아직 DeadState가 아니면 죽음 처리
            if (hp <= 0f && !(CurrentState is DeadState))
            {
                TransitionToState(typeof(DeadState));
            }
            // DeadState.cs에는 이미 DestroyLater 코루틴이 구현되어 있으므로 
            // 여기서 추가 파괴 로직이 필요하지 않음
        }
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
        _ when t == typeof(DetourState) => EnemyState.Detour,
        _ when t == typeof(ReturnState) => EnemyState.Return,
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
        EnemyState.Detour => typeof(DetourState),
        EnemyState.Return => typeof(ReturnState),
        EnemyState.WaitCool => typeof(WaitCoolState),
        EnemyState.Attack => typeof(AttackState),
        EnemyState.AttackCool => typeof(AttackCoolState),
        EnemyState.Hit => typeof(HitState),
        EnemyState.Dead => typeof(DeadState),
        _ => typeof(IdleState)
    };

    /* ────────────────── Detect Target (Taunt > Player > Servant) ──────────────────── */
    float detT; 
    const float DET_INT = .2f;
    public void DetectTarget()
    {
        detT += Time.deltaTime;
        if (detT < DET_INT) return;
        detT = 0f;
        /* 0) 활성 도발 확인 */
        ITauntable[] taunts = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                                    .OfType<ITauntable>()
                                    .Where(t => t.IsActive)
                                    .ToArray();
        if (taunts.Length > 0)
        {
            Target = taunts[0].TauntPoint;          // 하나만 선택
            return;
        }

        /* 1) 플레이어 찾기 */
        Collider[] cols = Physics.OverlapSphere(
                transform.position, enemyStatus.detectRange,
                playerMask, QueryTriggerInteraction.Ignore);

        Transform nearest = FindNearest(cols);
        if (nearest) { Target = nearest; return; }

        /* 2) 소환수 찾기 */
        cols = Physics.OverlapSphere(
                transform.position, enemyStatus.detectRange,
                servantMask, QueryTriggerInteraction.Ignore);

        nearest = FindNearest(cols);
        Target = nearest;          // 없으면 null
    }
    void TrySwitchTargetToAttacker(Vector3 hitPos)
    {
        const float PICK_RADIUS = 2f;              // 피격 지점 2 m 안
        Collider[] cols = Physics.OverlapSphere(
                            hitPos, PICK_RADIUS,
                            playerMask,                // EnemyFSM에 이미 존재
                            QueryTriggerInteraction.Ignore);

        Transform nearest = FindNearest(cols);      // EnemyFSM에 이미 구현돼 있음
        if (nearest && nearest != Target)
            Target = nearest;
    }
    Transform FindNearest(Collider[] arr)
    {
        Transform best = null;
        float minSq = float.PositiveInfinity;
        foreach (var c in arr)
        {
            float d = (c.transform.position - transform.position).sqrMagnitude;
            if (d < minSq) { minSq = d; best = c.transform; }
        }
        return best;
    }

    /* ────────────────────────── IDamageable ─────────────────────── */
    public void TakeDamage(float damage, Vector3 atkPos, AttackerType t = AttackerType.Default)
    {
        if (isDead) return;
        pv.RPC(nameof(DamageToMaster_RPC), RpcTarget.MasterClient, damage, atkPos, (int)t);
    }
      
    public void DamageToMaster(float damage, Vector3 attackerPos)
    {
        // ‘Default’ 타입으로 실제 RPC 호출
        DamageToMaster_RPC(damage, attackerPos, (int)AttackerType.Default);
    }
    [PunRPC]
    public void DamageToMaster_RPC(float damage, Vector3 atkPos, int t = 0)
    {
        if (isDead) return;
        var atkType = (AttackerType)t;
        if (!PhotonNetwork.IsMasterClient) return;
        lastHitPos = atkPos;
        TrySwitchTargetToAttacker(atkPos);
        float rawDamage = damage;
        /* 실드 → HP 차감 */
        if (shield > 0f)
        {
            float prevShield = shield;
            shield = Mathf.Max(0f, shield - damage);
            damage = Mathf.Max(0f, damage - prevShield);
            pv.RPC(nameof(UpdateShield), RpcTarget.AllBuffered,
                   maxShield == 0f ? 0f : shield / maxShield);
            if (shield == 0f && prevShield > 0f)
                pv.RPC(nameof(RPC_ShieldBreakFx), RpcTarget.All);
        }

        float prevHP = hp;
        hp = Mathf.Max(0f, hp - damage);
        float deltaHP = prevHP - hp;
        pv.RPC(nameof(UpdateHP), RpcTarget.AllBuffered, hp / maxHP);
        pv.RPC(nameof(RPC_ShowDamage), RpcTarget.All, rawDamage);
        if(atkType != AttackerType.Debuff)
        {
            bool fromRight = atkPos.x < transform.position.x;
            //pv.RPC(nameof(RPC_ApplyKnockback), RpcTarget.All, atkPos);
            pv.RPC(nameof(RPC_SpawnHitFx), RpcTarget.All, transform.position, fromRight);
            TransitionToState(hp <= 0f ? typeof(DeadState) : typeof(HitState));
        }
        else if (hp <= 0f)
        {
            isDead = true;
            TransitionToState(typeof(DeadState));
        }
    }
    public void SelectNextAttackPattern()
    {
        if (AttackPatterns == null || AttackPatterns.Length == 0) return;
        if (AttackPatterns.Length == 1) { AttackComponent = AttackPatterns[0]; return; }

        /* 1) SO가중치 → 총합 계산 */
        float total = 0f;
        foreach (var w in enemyStatus.attackWeights) total += w.weight;

        /* 2) 룰렛 */
        float r = Random.Range(0f, total);
        float acc = 0f;
        for (int i = 0; i < enemyStatus.attackWeights.Length; i++)
        {
            acc += enemyStatus.attackWeights[i].weight;
            if (r <= acc)
            {
                string want = enemyStatus.attackWeights[i].scriptName;
                AttackComponent = AttackPatterns.First(p => p.GetType().Name == want);
                attackIdx = i;
                return;
            }
        }
        AttackComponent = AttackPatterns[0];      // 안전장치
    }
    /* ─ HP / Shield UI 동기화 ─ */
    [PunRPC]
    public void UpdateHP(float r)
    { if (uiHP != null) uiHP.SetHP(r); }
    [PunRPC]
    void RPC_ShowDamage(float damage)
    {
        if (uiHP != null)
        {
            uiHP.ShowDamage((int)damage);
        }
    }
    [PunRPC]
    public void UpdateShield(float r)
    { if (uiHP) { uiHP.SetShield(r); uiHP.CheckThreshold(r, true); } }

    /* ─ 넉백 / FX ─ */
    //[PunRPC]
    //void RPC_ApplyKnockback(Vector3 atkPos)
    //{
    //    Vector3 dir = (transform.position - atkPos).normalized;
    //    dir.y = dir.z = 0f;
    //    knockVel = dir * enemyStatus.hitKnockbackStrength;
    //    knockTime = KNOCK_DUR;
    //}
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

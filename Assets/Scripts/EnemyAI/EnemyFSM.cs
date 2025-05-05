/************************************************
 * EnemyFSM.cs  –  NavMesh + PUN2 네트워크 FSM
 *   (2025‑05‑05 : 방향 API · RPC 애니 동기화 완성)
 ************************************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

[RequireComponent(typeof(NavMeshAgent), typeof(PhotonView))]
public class EnemyFSM : MonoBehaviourPun, IPunObservable
{
    /* ───────── Components ───────── */
    public NavMeshAgent Agent { get; private set; }
    public Animator Anim { get; private set; }

    private PhotonView pv;

    /* ───────── Data ───────── */
    [SerializeField] private EnemyStatusSO enemyStatus;
    public EnemyStatusSO EnemyStatusRef => enemyStatus;

    /* ───────── SpawnArea (Spawner RPC로 주입) ───────── */
    private SpawnArea spawnArea;
    public void SetSpawnArea(SpawnArea a) => spawnArea = a;
    public SpawnArea CurrentSpawnArea => spawnArea;

    /* ───────── FSM ───────── */
    private readonly Dictionary<System.Type, IState> states = new();
    public IState CurrentState { get; private set; }

    /* ───────── Runtime ───────── */
    public Transform Target { get; set; }
    public bool LastAttackSuccessful { get; set; }
    public float currentHP { get; private set; }

    /* ───────── Facing ───────── */
    float lastMoveX = 1f;                       // +1 ⇒ Right , -1 ⇒ Left
    public float CurrentFacing => lastMoveX;
    public void ForceFacing(float dirSign) => lastMoveX = dirSign >= 0 ? 1f : -1f;

    /* ───────── Network Lerp ───────── */
    Vector3 netPos;
    Quaternion netRot;
    float netLastMoveX;
    const float NET_SMOOTH = 10f;

    EnemyState currentEnum;
    /* ───────── Debug 옵션 ───────── */
    [Header("Debug")]
    [SerializeField] public bool debugMode = true;             // 필요 없으면 false
    const float GIZMO_Y_OFFSET = 0.05f;                 // 겹침 방지
    /* ───────── 공격 조건 ───────── */
    [Header("Attack Alignment")]
    [SerializeField] public float zAlignTolerance = 0.4f;   // 허용 Z 오차 (m)
    /* ─────────────────────────────────────────── */
    #region Unity Flow
    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        pv = GetComponent<PhotonView>();
        Anim = GetComponentInChildren<Animator>();

        Agent.updateRotation = false;
        Agent.updatePosition = PhotonNetwork.IsMasterClient;
        Agent.updateRotation = PhotonNetwork.IsMasterClient;

        /* 상태 인스턴스 등록 */
        states[typeof(WanderState)] = new WanderState(this);
        states[typeof(IdleState)] = new IdleState(this);
        states[typeof(ChaseState)] = new ChaseState(this);
        states[typeof(WaitCoolState)] = new WaitCoolState(this);
        states[typeof(AttackState)] = new AttackState(this);
        states[typeof(AttackCoolState)] = new AttackCoolState(this);
        states[typeof(HitState)] = new HitState(this);
        states[typeof(DeadState)] = new DeadState(this);
    }

    void Start()
    {
        currentHP = enemyStatus.maxHealth;

        if (PhotonNetwork.IsMasterClient)
        {
            ApplyStatusToAgent();
            TransitionToState(typeof(WanderState));
        }
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
            CurrentState?.Execute();
        else
        {
            transform.position = Vector3.Lerp(transform.position, netPos, Time.deltaTime * NET_SMOOTH);
            transform.rotation = Quaternion.Slerp(transform.rotation, netRot, Time.deltaTime * NET_SMOOTH);
            lastMoveX = netLastMoveX;
        }

        UpdateFacingFromVelocity();
    }
    #endregion

    #region Facing / Animation
    void UpdateFacingFromVelocity()
    {
        if (PhotonNetwork.IsMasterClient &&
            Agent.enabled && Agent.velocity.sqrMagnitude > 0.0001f)
        {
            lastMoveX = Agent.velocity.x >= 0 ? 1f : -1f;
        }
    }

    public void PlayDirectionalAnim(string action)
    {
        string clip = (lastMoveX >= 0 ? "Right_" : "Left_") + action;

        if (Anim.GetCurrentAnimatorStateInfo(0).IsName(clip)) return;

        if (pv.IsMine)
            pv.RPC(nameof(RPC_PlayClip), RpcTarget.Others, clip);

        Anim.Play(clip, 0);
    }

    [PunRPC] void RPC_PlayClip(string clip) => Anim.Play(clip, 0);
    #endregion

    #region FSM Helpers
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
    #endregion

    #region Detect Player (다른 상태에서 호출)
    float detectT;
    const float DETECT_INTERVAL = .2f;

    public void DetectPlayer()
    {
        detectT += Time.deltaTime;
        if (detectT < DETECT_INTERVAL) return;
        detectT = 0f;

        Collider[] cols = new Collider[8];
        int n = Physics.OverlapSphereNonAlloc(transform.position,
                                              enemyStatus.detectRange,
                                              cols, enemyStatus.playerLayerMask,
                                              QueryTriggerInteraction.Ignore);

        Transform closest = null;
        float minSq = float.PositiveInfinity;
        for (int i = 0; i < n; ++i)
            if (cols[i].CompareTag("Player"))
            {
                float d = (cols[i].transform.position - transform.position).sqrMagnitude;
                if (d < minSq) { minSq = d; closest = cols[i].transform; }
            }

        Target = closest;
    }
    #endregion

    #region Networking
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
            var incoming = (EnemyState)(int)s.ReceiveNext();
            netPos = (Vector3)s.ReceiveNext();
            netRot = (Quaternion)s.ReceiveNext();
            netLastMoveX = (float)s.ReceiveNext();

            if (incoming != currentEnum)
                TransitionToState(EnumToType(incoming));
        }
    }
    #endregion

    void ApplyStatusToAgent()
    {
        Agent.speed = enemyStatus.moveSpeed;
        Agent.stoppingDistance = enemyStatus.attackRange;
        Agent.enabled = true;
    }
    /* ───────── 유틸: 평면‑거리 계산 ───────── */
    public float GetZDiffAbs()
    {
        if (Target == null) return float.PositiveInfinity;
        return Mathf.Abs(Target.position.z - transform.position.z);
    }

    public float GetTarget2DDistSq()
    {
        if (Target == null) return float.PositiveInfinity;
        Vector3 a = Target.position;
        Vector3 b = transform.position;
        a.y = b.y = 0f;                                 // 평면 거리
        return (a - b).sqrMagnitude;
    }

    public bool IsTargetInAttackRange()
    {
        float range = enemyStatus.attackRange;
        return GetTarget2DDistSq() <= range * range;
    }

#if UNITY_EDITOR
    /* ───────── Scene 뷰 시각화 ───────── */
    void OnDrawGizmosSelected()
    {
        if (!debugMode || enemyStatus == null) return;

        // 탐지 범위 = 노랑
        Gizmos.color = new Color(1f, 0.9f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * GIZMO_Y_OFFSET,
                              enemyStatus.detectRange);

        // 공격 범위 = 빨강 (Agent.radius 포함)
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        float atkR = enemyStatus.attackRange;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * GIZMO_Y_OFFSET,
                              atkR);

        // 타깃까지 선 & 텍스트
        if (Target)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position + Vector3.up * GIZMO_Y_OFFSET,
                            Target.position + Vector3.up * GIZMO_Y_OFFSET);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.2f,
                $"dist={Mathf.Sqrt(GetTarget2DDistSq()):0.00}");
        }
    }
#endif
    public Vector2 GetXZToTarget()
    {
        if (Target == null) return Vector2.positiveInfinity;
        Vector3 d = Target.position - transform.position;
        return new Vector2(d.x, d.z);                // (x, z)
    }

    /* 새 함수: 범위 + Z 맞춤 모두 만족? */
    public bool IsAlignedAndInRange()
    {
        float zOk = GetZDiffAbs() <= zAlignTolerance ? 1f : 0f;
        return IsTargetInAttackRange() && zOk > 0f;
    }
}

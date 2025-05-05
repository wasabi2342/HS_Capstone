// ==================== EnemyFSM.cs
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent), typeof(PhotonView))]
public class EnemyFSM : MonoBehaviourPun, IPunObservable
{
    /* ── Components ── */
    public NavMeshAgent Agent { get; private set; }
    public Animator Anim { get; private set; }
    private PhotonView pv;

    /* ── Data ── */
    [SerializeField] private EnemyStatusSO enemyStatus;
    public EnemyStatusSO EnemyStatusRef => enemyStatus;
    public SpawnArea SpawnAreaRef { get; private set; }

    /* ── FSM ── */
    public Dictionary<System.Type, IState> states;
    public IState CurrentState { get; private set; }
    private EnemyState currentEnum;

    /* ── Target & Anim ── */
    public Transform Target { get; set; }
    public bool LastAttackSuccessful { get; set; }
    float lastMoveX = 1f;         // + 오른쪽, – 왼쪽

    public float currentHP { get; private set; }
    /* ── Unity ── */
    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Agent.updateRotation = false;          // 회전 차단

#if UNITY_2022_2_OR_NEWER
        Agent.updateUpAxis = false;          // 2D 평면용
#endif
        pv = GetComponent<PhotonView>();
        Anim = GetComponentInChildren<Animator>();

        states = new Dictionary<System.Type, IState>
        {
            { typeof(WanderState)  , new WanderState(this)   },
            { typeof(IdleState)    , new IdleState(this)     },
            { typeof(ChaseState)   , new ChaseState(this)    },
            { typeof(WaitCoolState), new WaitCoolState(this) },
            { typeof(AttackState)  , new AttackState(this)   },
            { typeof(AttackCoolState), new AttackCoolState(this) },
            { typeof(DeadState)   , new DeadState(this)      },
            { typeof(HitState)   , new HitState(this)       }
        };
    }

    void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Agent.enabled = false;
            if (TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            return;
        }
        currentHP = enemyStatus.maxHealth;
        ApplyStatusToAgent();
        TransitionToState(typeof(WanderState));
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        CurrentState?.Execute();
        detectTimer += Time.deltaTime;
        if (detectTimer >= DETECT_INTERVAL)
        {
            DetectPlayer();
            detectTimer = 0f;
        }
    }

    /* ── Animation Helper ── */
    public void PlayDirectionalAnim(string action)
    {
        if (Agent.enabled && Agent.velocity.sqrMagnitude > .0001f)
            lastMoveX = Agent.velocity.x >= 0 ? 1f : -1f;


        string stateName = (lastMoveX >= 0 ? "Right_" : "Left_") + action;
        if (!Anim.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            Anim.Play(stateName, 0);
    }

    [PunRPC] public void PlayAttackAnimRPC() => PlayDirectionalAnim("Attack");

    /* ── State control ── */
    public void TransitionToState(System.Type t)
    {
        if (!states.TryGetValue(t, out IState next)) return;
        if (CurrentState?.GetType() == t) return;

        CurrentState?.Exit();
        CurrentState = next;
        CurrentState.Enter();
        currentEnum = TypeToEnum(t);
    }

    /* ── Detect player ── */
    float detectTimer;
    const float DETECT_INTERVAL = .2f;

    public void DetectPlayer()
    {
        if (!enemyStatus) return;

        Collider[] cols = new Collider[8];
        int n = Physics.OverlapSphereNonAlloc(transform.position, enemyStatus.detectRange,
                                              cols, enemyStatus.playerLayerMask,
                                              QueryTriggerInteraction.Ignore);
        Transform closest = null;
        float min = float.PositiveInfinity;
        for (int i = 0; i < n; ++i)
            if (cols[i].CompareTag("Player"))
            {
                float d = (cols[i].transform.position - transform.position).sqrMagnitude;
                if (d < min) { min = d; closest = cols[i].transform; }
            }
        Target = closest;
    }

    /* ── Helpers ── */
    void ApplyStatusToAgent()
    {
        if (!enemyStatus) return;
        Agent.speed = enemyStatus.moveSpeed;
        Agent.stoppingDistance = enemyStatus.attackRange * .9f;
        Agent.enabled = true;
    }
    public void SetSpawnArea(SpawnArea sa) => SpawnAreaRef = sa;

    /* ── Networking ── */
    public void OnPhotonSerializeView(PhotonStream s, PhotonMessageInfo i)
    {
        if (s.IsWriting) s.SendNext((int)currentEnum);
        else TransitionToState(EnumToType((EnemyState)(int)s.ReceiveNext()));
    }

    EnemyState TypeToEnum(System.Type t)
    {
        if (t == typeof(WanderState)) return EnemyState.Wander;
        if (t == typeof(IdleState)) return EnemyState.Idle;
        if (t == typeof(ChaseState)) return EnemyState.Chase;
        if (t == typeof(WaitCoolState)) return EnemyState.WaitCool;
        if (t == typeof(AttackState)) return EnemyState.Attack;
        if (t == typeof(AttackCoolState)) return EnemyState.AttackCool;
        if (t == typeof(DeadState)) return EnemyState.Dead;
        if (t == typeof(HitState)) return EnemyState.Hit;
        return EnemyState.Idle;
    }
    System.Type EnumToType(EnemyState e) => e switch
    {
        EnemyState.Wander => typeof(WanderState),
        EnemyState.Idle => typeof(IdleState),
        EnemyState.Chase => typeof(ChaseState),
        EnemyState.WaitCool => typeof(WaitCoolState),
        EnemyState.Attack => typeof(AttackState),
        EnemyState.AttackCool => typeof(AttackCoolState),
        EnemyState.Dead => typeof(DeadState),
        EnemyState.Hit => typeof(HitState),
        _ => typeof(IdleState)
    };
}

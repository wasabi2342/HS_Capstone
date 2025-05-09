using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class ServantFSM : MonoBehaviourPun, IPunObservable, IDamageable
{
    // ─── Components & References ─────────────────────────────
    public NavMeshAgent Agent { get; private set; }
    public Animator Anim { get; private set; }
    public IMonsterAttack Attack { get; private set; }
    public PhotonView pv;

    public Transform OwnerPlayer { get; private set; }
    public Transform TargetEnemy { get; private set; }

    // ─── Stats & Timers ───────────────────────────────────────
    [Header("Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public float chaseMultiplier = 1.2f;
    public float attackDamage = 10f;
    public float detectRange = 3f;
    public float attackRange = 0.5f;
    public float attackDuration = 1f;

    [Header("Timing")]
    public float waitCoolTime = 0.5f;
    public float attackCoolTime = 1f;

    [Header("Detection")]
    public LayerMask enemyLayerMask;

    float currentHP;

    // ─── Invincibility ───────────────────────────────────────
    public bool IsInvincible { get; set; } = false;

    // ─── Facing ───────────────────────────────────────────────
    float lastMoveX = 1f;  // +1 = 오른쪽, -1 = 왼쪽
    public float CurrentFacing => lastMoveX;
    public void ForceFacing(float dx) => lastMoveX = dx >= 0f ? 1f : -1f;

    // ─── FSM States ───────────────────────────────────────────
    Dictionary<Type, ServantBaseState> states = new Dictionary<Type, ServantBaseState>();
    public ServantBaseState CurrentState { get; private set; }

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Anim = GetComponentInChildren<Animator>();
        pv = GetComponent<PhotonView>();
        Attack = GetComponent<IMonsterAttack>();

        currentHP = maxHealth;

        // 상태 등록
        states[typeof(ServantSpawnState)] = new ServantSpawnState(this);
        states[typeof(ServantIdleState)] = new ServantIdleState(this);
        states[typeof(ServantWanderState)] = new ServantWanderState(this);
        states[typeof(ServantChaseState)] = new ServantChaseState(this);
        states[typeof(ServantWaitCoolState)] = new ServantWaitCoolState(this);
        states[typeof(ServantAttackState)] = new ServantAttackState(this);
        states[typeof(ServantAttackCoolState)] = new ServantAttackCoolState(this);
        states[typeof(ServantHitState)] = new ServantHitState(this);
        states[typeof(ServantDeadState)] = new ServantDeadState(this);
    }

    void Start()
    {
        // 마스터 클라이언트만 FSM 시작
        if (PhotonNetwork.IsMasterClient)
            TransitionToState(typeof(ServantSpawnState));
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CurrentState?.Execute();
            // ▶ 이동 속도 기반으로 Facing 자동 갱신
            UpdateFacingFromVelocity();
        }
    }

    /// <summary>상태 전환</summary>
    public void TransitionToState(Type next)
    {
        if (CurrentState?.GetType() == next) return;
        CurrentState?.Exit();
        CurrentState = states[next];
        CurrentState.Enter();
    }

    /// <summary>가장 가까운 적 탐지</summary>
    public void DetectEnemy()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, detectRange, enemyLayerMask);
        float best = float.MaxValue;
        Transform pick = null;
        foreach (var c in cols)
            if (c.CompareTag("Enemy"))
            {
                float d = (c.transform.position - transform.position).sqrMagnitude;
                if (d < best) { best = d; pick = c.transform; }
            }

        TargetEnemy = pick;
    }

    // ─── Animation 동기화 ─────────────────────────────────────
    public void PlayDirectionalAnim(string action)
    {
        // ex) "Right_Attack" 또는 "Left_Walk"
        string clip = (lastMoveX >= 0f ? "Right_" : "Left_") + action;
        Anim.Play(clip, 0);

        // 네트워크 동기화: 다른 클라이언트에게도 재생 요청
        if (pv.IsMine)
            pv.RPC(nameof(RPC_PlayClip), RpcTarget.Others, clip);
    }

    [PunRPC]
    public void RPC_PlayClip(string clip)
    {
        Anim.Play(clip, 0);
    }

    // ─── Facing 업데이트 ──────────────────────────────────────
    void UpdateFacingFromVelocity()
    {
        if (Agent.enabled && Agent.velocity.sqrMagnitude > 0.0001f)
            lastMoveX = Agent.velocity.x >= 0f ? 1f : -1f;
    }

    // ─── IDamageable 구현 ─────────────────────────────────────
    [PunRPC]
    public void RPC_TakeDamage(float dmg, int attackerViewID)
    {
        DamageToMaster(dmg, attackerViewID);
    }

    [PunRPC]
    public void DamageToMaster(float dmg, int attacker)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        TakeDamage(dmg);
    }

    public void TakeDamage(float damage, AttackerType at = AttackerType.Default)
    {
        if (IsInvincible || !PhotonNetwork.IsMasterClient) return;

        currentHP -= damage;
        float ratio = Mathf.Clamp01(currentHP / maxHealth);
        pv.RPC(nameof(UpdateHP), RpcTarget.AllBuffered, ratio);

        if (currentHP <= 0f)
            TransitionToState(typeof(ServantDeadState));
        else
            TransitionToState(typeof(ServantHitState));
    }

    [PunRPC]
    public void UpdateHP(float ratio)
    {
        // TODO: UI 업데이트
    }

    // ─── 네트워크 위치/회전 동기화 ─────────────────────────────
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
    [PunRPC]
    public void ForceKill()
    {
        // 이미 DeadState가 아니면 전환
        if (!(CurrentState is ServantDeadState))
            TransitionToState(typeof(ServantDeadState));
    }
    // ─── 오너 설정 RPC ─────────────────────────────────────────
    [PunRPC]
    public void RPC_SetOwner(int viewID)
    {
        var opv = PhotonView.Find(viewID);
        if (opv != null) OwnerPlayer = opv.transform;
    }

    void OnEnable()
    {
        if (OwnerPlayer != null)
            OwnerPlayer.GetComponent<PinkPlayerController>()
                       ?.AddServantToList(this);
    }

    void OnDestroy()
    {
        if (OwnerPlayer != null)
            OwnerPlayer.GetComponent<PinkPlayerController>()
                       ?.RemoveServantFromList(this);
    }
}

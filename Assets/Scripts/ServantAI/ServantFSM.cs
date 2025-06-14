using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ServantFSM : MonoBehaviourPun, IPunObservable, IDamageable, ITauntable
{
    // ─── Components & References ─────────────────────────────
    public NavMeshAgent Agent { get; private set; }
    public Animator Anim { get; private set; }
    public IMonsterAttack Attack { get; private set; }
    public PhotonView pv;

    public Transform OwnerPlayer { get; private set; }
    public Transform TargetEnemy { get; private set; }

    // PinkPlayerController 참조
    private PinkPlayerController ownerController =>
        OwnerPlayer != null
            ? OwnerPlayer.GetComponent<PinkPlayerController>()
            : null;

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
    [SerializeField] LayerMask enemyLayerMask;
    [SerializeField] LayerMask playerLayerMask;

    [Header("Taunt")]
    public bool tauntActive;
    public float tauntEndTime;

    float currentHP;
    bool isPvpScene;

    // ─── Invincibility ───────────────────────────────────────
    public bool IsInvincible { get; set; } = false;

    // ─── Facing ───────────────────────────────────────────────
    float lastMoveX = 1f;  // +1 = 오른쪽, -1 = 왼쪽
    public float CurrentFacing => lastMoveX;
    public void ForceFacing(float dx) => lastMoveX = dx >= 0f ? 1f : -1f;

    // ─── FSM States ───────────────────────────────────────────
    Dictionary<Type, ServantBaseState> states = new Dictionary<Type, ServantBaseState>();
    public ServantBaseState CurrentState { get; private set; }
    // ── Taunt ─────────────────────────────────────────────────
    public bool IsActive => tauntActive && Time.time < tauntEndTime;
    public Transform TauntPoint => transform;
    void EnsureAgentOnNavMesh(float searchRadius = 5f)
    {
        if (Agent == null || Agent.isOnNavMesh) return;

        // 가장 가까운 NavMesh 지점 샘플링
        if (NavMesh.SamplePosition(transform.position,
                                   out var hit, searchRadius,
                                   NavMesh.AllAreas))
        {
            Agent.Warp(hit.position);     // 올바른 좌표로 순간이동
            Agent.enabled = true;
        }
        else
        {
            Debug.LogWarning("[ServantFSM] NavMesh를 찾지 못했습니다. Agent 비활성화");
            Agent.enabled = false;        // 문제 씬(PVP 등)에서 오류 차단
        }
    }
    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Anim = GetComponentInChildren<Animator>();
        pv = GetComponent<PhotonView>();
        Attack = GetComponent<IMonsterAttack>();

        currentHP = maxHealth;
        EnsureAgentOnNavMesh();
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
        isPvpScene = SceneManager.GetActiveScene().name.Contains("PVP");
        TransitionToState(typeof(ServantSpawnState));
        Debug.Log($"[Servant] 소환수 정보 - Name: {name}, Layer: {gameObject.layer} ({LayerMask.LayerToName(gameObject.layer)}), Tag: {tag}");

        // 모든 자식 오브젝트의 레이어도 확인
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            Debug.Log($"[Servant] 자식 '{child.name}' Layer: {child.gameObject.layer} ({LayerMask.LayerToName(child.gameObject.layer)})");
        }
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
    /// <summary>상태 전환</summary>
    [PunRPC]
    public void RPC_EnableTaunt(float dur)
    {
        tauntActive = true;
        tauntEndTime = Time.time + dur;
    }
    /// <summary>가장 가까운 적 탐지</summary>
    public void DetectEnemy()
    {
        // 1) 어떤 레이어를 볼지 결정
        LayerMask mask = isPvpScene
            ? enemyLayerMask | playerLayerMask   // PVP = 전부
            : enemyLayerMask;                    // PVE = 몬스터만

        // 2) 가장 가까운 IDamageable 찾기
        Collider[] cols = Physics.OverlapSphere(
            transform.position,
            detectRange,
            mask,
            QueryTriggerInteraction.Collide);
        float closest = float.MaxValue;
        Transform pick = null;

        foreach (var c in cols)
        {
            // Owner 자기 자신은 절대 타깃이 되면 안 됨
            if (OwnerPlayer != null && c.transform.IsChildOf(OwnerPlayer)) continue;

            // IDamageable 있는가?
            if (c.GetComponentInParent<IDamageable>() == null) continue;

            float d = (c.transform.position - transform.position).sqrMagnitude;
            if (d < closest)
            {
                closest = d;
                pick = c.transform;
            }
        }
        TargetEnemy = pick;
    }

    // 외부에서 읽을 수 있게 프로퍼티 하나 노출
    public bool IsPvpScene => isPvpScene;


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
    public void RPC_TakeDamage(float dmg, int attackerViewID, Vector3 attackerPos)
    {
        DamageToMaster(dmg, attackerPos);
    }

    [PunRPC]
    public void DamageToMaster(float dmg, Vector3 attackerPos)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        TakeDamage(dmg, attackerPos);
    }

    public void TakeDamage(float damage, Vector3 attackerPos, AttackerType at = AttackerType.Default)
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

    // 도발

    public void TauntEnemy(float tauntDur)
    {
        //float tauntDur = 5f; //도발 시간 전달
        photonView.RPC("RPC_EnableTaunt", RpcTarget.AllBuffered, tauntDur);
        tauntActive = true;
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

    // ─── 소환수 죽을 때 데미지 호출 ─────────────────
    /// <summary>
    /// Animation Event로 호출.
    /// Devil 레벨이 3일 때만 사망 이펙트를 생성·동기화합니다.
    /// </summary>
    public void CreateServantDeathEffect()
    {
        if (ownerController == null) return;

        int devil = ownerController.runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.Devil;
        if (devil != 3) return;


        Debug.Log("지옥의 수호자");
        // 데미지 계산
        float adc = ownerController.runTimeData.skillWithLevel[(int)Skills.R].skillData.AttackDamageCoefficient;
        float apc = ownerController.runTimeData.skillWithLevel[(int)Skills.R].skillData.AbilityPowerCoefficient;
        float damage = (adc * ownerController.runTimeData.attackPower
                      + apc * ownerController.runTimeData.abilityPower)
                       * ownerController.damageBuff;

        // 이펙트 경로 & 위치
        string side = lastMoveX >= 0f ? "right" : "left";
        string path = $"SkillEffect/PinkPlayer/pink_servant_death_{side}_{devil}";
        Vector3 pos = transform.position;

        // 1) 로컬 인스턴스
        InstantiateDeathEffect(path, pos, damage, /*isMine*/ pv.IsMine);

        // 2) 원격 동기화
        if (pv.IsMine)
            pv.RPC(nameof(RPC_CreateServantDeathEffectOnRemote), RpcTarget.OthersBuffered, path, pos, damage);
    }

    /// <summary>사망 이펙트를 실제 생성하는 헬퍼</summary>
    private void InstantiateDeathEffect(string path, Vector3 pos, float dmg, bool isMine)
    {
        // 1) 이펙트 로드 & 인스턴스
        var fx = Instantiate(Resources.Load<SkillEffect>(path), pos, Quaternion.identity);

        // 2) PlayerBlessing 컴포넌트에서 SpecialEffect 가져오기
        var blessingComp = ownerController.GetComponent<PlayerBlessing>();
        var specialEffect = blessingComp != null
            ? blessingComp.FindSkillEffect(
                ownerController.runTimeData.skillWithLevel[(int)Skills.R].skillData.ID,
                ownerController
              )
            : null;

        // 3) Init 호출
        fx.Init(
            dmg,
            ownerController.StartHitlag,
            isMine,
            specialEffect
        );

        // 4) 부모 설정
        fx.transform.parent = transform;
    }

    [PunRPC]
    private void RPC_CreateServantDeathEffectOnRemote(string path, Vector3 pos, float dmg)
    {
        InstantiateDeathEffect(path, pos, dmg, /*isMine*/ false);
    }
}


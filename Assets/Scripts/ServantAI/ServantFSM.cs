using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class ServantFSM : MonoBehaviourPun, IPunObservable, IDamageable, ITauntable
{
    // ������ Components & References ����������������������������������������������������������
    public NavMeshAgent Agent { get; private set; }
    public Animator Anim { get; private set; }
    public IMonsterAttack Attack { get; private set; }
    public PhotonView pv;

    public Transform OwnerPlayer { get; private set; }
    public Transform TargetEnemy { get; private set; }

    // PinkPlayerController ����
    private PinkPlayerController ownerController =>
        OwnerPlayer != null
            ? OwnerPlayer.GetComponent<PinkPlayerController>()
            : null;

    // ������ Stats & Timers ������������������������������������������������������������������������������
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
    [Header("Taunt")]
    public bool tauntActive;
    public float tauntEndTime;

    float currentHP;

    // ������ Invincibility ������������������������������������������������������������������������������
    public bool IsInvincible { get; set; } = false;

    // ������ Facing ����������������������������������������������������������������������������������������������
    float lastMoveX = 1f;  // +1 = ������, -1 = ����
    public float CurrentFacing => lastMoveX;
    public void ForceFacing(float dx) => lastMoveX = dx >= 0f ? 1f : -1f;

    // ������ FSM States ��������������������������������������������������������������������������������������
    Dictionary<Type, ServantBaseState> states = new Dictionary<Type, ServantBaseState>();
    public ServantBaseState CurrentState { get; private set; }
    // ���� Taunt ��������������������������������������������������������������������������������������������������
    public bool IsActive => tauntActive && Time.time < tauntEndTime;
    public Transform TauntPoint => transform;

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Anim = GetComponentInChildren<Animator>();
        pv = GetComponent<PhotonView>();
        Attack = GetComponent<IMonsterAttack>();

        currentHP = maxHealth;

        // ���� ���
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
        // ������ Ŭ���̾�Ʈ�� FSM ����
        if (PhotonNetwork.IsMasterClient)
            TransitionToState(typeof(ServantSpawnState));
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CurrentState?.Execute();
            // �� �̵� �ӵ� ������� Facing �ڵ� ����
            UpdateFacingFromVelocity();
        }
    }

    /// <summary>���� ��ȯ</summary>
    public void TransitionToState(Type next)
    {
        if (CurrentState?.GetType() == next) return;
        CurrentState?.Exit();
        CurrentState = states[next];
        CurrentState.Enter();
    }
    /// <summary>���� ��ȯ</summary>
    [PunRPC]
    public void RPC_EnableTaunt(float dur)
    {
        tauntActive = true;
        tauntEndTime = Time.time + dur;
    }
    /// <summary>���� ����� �� Ž��</summary>
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

    // ������ Animation ����ȭ ��������������������������������������������������������������������������
    public void PlayDirectionalAnim(string action)
    {
        // ex) "Right_Attack" �Ǵ� "Left_Walk"
        string clip = (lastMoveX >= 0f ? "Right_" : "Left_") + action;
        Anim.Play(clip, 0);

        // ��Ʈ��ũ ����ȭ: �ٸ� Ŭ���̾�Ʈ���Ե� ��� ��û
        if (pv.IsMine)
            pv.RPC(nameof(RPC_PlayClip), RpcTarget.Others, clip);
    }

    [PunRPC]
    public void RPC_PlayClip(string clip)
    {
        Anim.Play(clip, 0);
    }

    // ������ Facing ������Ʈ ����������������������������������������������������������������������������
    void UpdateFacingFromVelocity()
    {
        if (Agent.enabled && Agent.velocity.sqrMagnitude > 0.0001f)
            lastMoveX = Agent.velocity.x >= 0f ? 1f : -1f;
    }

    // ������ IDamageable ���� ��������������������������������������������������������������������������
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
        // TODO: UI ������Ʈ
    }

    // ����

    public void TauntEnemy(float tauntDur)
    {
        //float tauntDur = 5f; //���� �ð� ����
        photonView.RPC("RPC_EnableTaunt", RpcTarget.AllBuffered, tauntDur);
        tauntActive = true;
    }

    // ������ ��Ʈ��ũ ��ġ/ȸ�� ����ȭ ����������������������������������������������������������
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
        // �̹� DeadState�� �ƴϸ� ��ȯ
        if (!(CurrentState is ServantDeadState))
            TransitionToState(typeof(ServantDeadState));
    }
    // ������ ���� ���� RPC ����������������������������������������������������������������������������������
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

    // ������ ���⿡ �߰�: ��ȯ�� Death Animation Event�� ȣ�� ����������������������������������
    /// <summary>
    /// Animation Event�� ȣ��.
    /// Devil ������ 3�� ���� ��� ����Ʈ�� ����������ȭ�մϴ�.
    /// </summary>
    public void CreateServantDeathEffect()
    {
        if (ownerController == null) return;

        int devil = ownerController.runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil;
        if (devil != 3) return;


        Debug.Log("������ ��ȣ��");
        // ������ ���
        float adc = ownerController.runTimeData.skillWithLevel[(int)Skills.R].skillData.AttackDamageCoefficient;
        float apc = ownerController.runTimeData.skillWithLevel[(int)Skills.R].skillData.AbilityPowerCoefficient;
        float damage = (adc * ownerController.runTimeData.attackPower
                      + apc * ownerController.runTimeData.abilityPower)
                       * ownerController.damageBuff;

        // ����Ʈ ��� & ��ġ
        string side = lastMoveX >= 0f ? "right" : "left";
        string path = $"SkillEffect/PinkPlayer/pink_servant_death_{side}_{devil}";
        Vector3 pos = transform.position;

        // 1) ���� �ν��Ͻ�
        InstantiateDeathEffect(path, pos, damage, /*isMine*/ pv.IsMine);

        // 2) ���� ����ȭ
        if (pv.IsMine)
            pv.RPC(nameof(RPC_CreateServantDeathEffectOnRemote), RpcTarget.OthersBuffered, path, pos, damage);
    }

    /// <summary>��� ����Ʈ�� ���� �����ϴ� ����</summary>
    private void InstantiateDeathEffect(string path, Vector3 pos, float dmg, bool isMine)
    {
        // 1) ����Ʈ �ε� & �ν��Ͻ�
        var fx = Instantiate(Resources.Load<SkillEffect>(path), pos, Quaternion.identity);

        // 2) PlayerBlessing ������Ʈ���� SpecialEffect ��������
        var blessingComp = ownerController.GetComponent<PlayerBlessing>();
        var specialEffect = blessingComp != null
            ? blessingComp.FindSkillEffect(
                ownerController.runTimeData.skillWithLevel[(int)Skills.R].skillData.ID,
                ownerController
              )
            : null;

        // 3) Init ȣ��
        fx.Init(
            dmg,
            ownerController.StartHitlag,
            isMine,
            specialEffect
        );

        // 4) �θ� ����
        fx.transform.parent = transform;
    }

    [PunRPC]
    private void RPC_CreateServantDeathEffectOnRemote(string path, Vector3 pos, float dmg)
    {
        InstantiateDeathEffect(path, pos, dmg, /*isMine*/ false);
    }
}


using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;          // [PunRPC] �Ӽ��� ���


/* ���������������������������������������������������������������������������������������������������������������������� */
[RequireComponent(typeof(Animator))]
public class ScarecrowFSM : MonoBehaviour, IDamageable
{
    /* ������[Inspector] ���Ȧ����� */
    [Header("Stats")]
    [SerializeField] public float maxHealth = 50000000f;
    [SerializeField] public float attackDur = 1f;   // Attack �ִ� ����
    [SerializeField] public float hitStunTime = 0.3f;
    [SerializeField] public float attackInterval = 2f;  // �� ���� �߰�
    /* ������[Inspector] ����� ��ۦ����� */
    [SerializeField] bool debugAttack;
    /* ������ ��Ÿ�� ������ */
    public Animator Anim { get; private set; }
    public float HP { get; private set; }

    /* ���� ON/OFF */
    bool attackEnabled;
    public bool AttackEnabled => attackEnabled;
    public void SetAttackEnabled(bool on)
    {
        attackEnabled = on;
        if (!on) attackTimer = 0f;      // OFF �� Ÿ�̸� �ʱ�ȭ
    }

    /* FSM */
    readonly Dictionary<System.Type, IState> states = new();
    IState currentState;

    /* ���� Ÿ�̸� (Idle ���¿��� ���) */
    [HideInInspector] public float attackTimer = 0f;

    /* ������ Unity Flow ������ */
    void Awake()
    {
        Anim = GetComponent<Animator>();
        HP = maxHealth;

        states[typeof(ScarecrowIdle)] = new ScarecrowIdle(this);
        states[typeof(ScarecrowAttack)] = new ScarecrowAttack(this);
        states[typeof(ScarecrowHit)] = new ScarecrowHit(this);
    }
    void Start() => TransitionTo(typeof(ScarecrowIdle));
    void Update()
    {
        /* ���� �ν����� ��� <-> ���� FSM ���� ��� �ݿ� ���� */
        if (debugAttack && !attackEnabled)
        {
            // ��� ON > ���� Ȱ�� + ��� AttackState ����
            SetAttackEnabled(true);
            ForceAttack();                     // �� �ż� ��ȯ
        }
        else if (!debugAttack && attackEnabled)
        {
            // ��� OFF >  ���� ��Ȱ�� + ��� IdleState ����
            SetAttackEnabled(false);
            TransitionTo(typeof(ScarecrowIdle));
        }

        currentState?.Execute();
    }
    #region Debug helpers
    /// <summary>
    /// ����ס�Ʃ�丮���: ���� ���¿� ������� ��� AttackState�� ���Խ�Ų��.
    /// </summary>
    public void ForceAttack()
    {
        // �̹� ���� ���̸� ����
        if (currentState is ScarecrowAttack)
            return;

        attackTimer = 0f;                              // ���� Ÿ�̸� ����
        TransitionTo(typeof(ScarecrowAttack));         // Attack ���·� ���� ��ȯ
    }
    #endregion
    /* ������ ���� ������ */
    public void PlayAnim(string clip)
    {
        if (!Anim.GetCurrentAnimatorStateInfo(0).IsName(clip))
            Anim.Play(clip, 0);
    }

    public void TransitionTo(System.Type t)
    {
        if (currentState?.GetType() == t) return;
        currentState?.Exit();
        currentState = states[t];
        currentState.Enter();
    }

    /* ������ IDamageable ������ */
    public void TakeDamage(float dmg, Vector3 attackerPos,
                           AttackerType type = AttackerType.Default)
    {
        /* ���� �����: ���Ⱑ IDamageable ȣ���ߴ��� Ȯ�� ���� */
        Debug.Log($"[ScarecrowFSM] TakeDamage() called  ��  dmg={dmg}, from={type}");
        DamageToMaster(dmg, attackerPos);
    }

    [PunRPC]
    public void DamageToMaster(float dmg, Vector3 attackerPos)
    {
        /* ���� �����: HP ���� ���� & Hit ��ȯ ���� */
        Debug.Log($"[ScarecrowFSM] DamageToMaster() ��  -{dmg} HP  (before={HP})");

        HP = Mathf.Max(0f, HP - dmg);
        attackTimer = 0f;                     // �ǰ� �� ���� Ÿ�̸� �ʱ�ȭ
        TransitionTo(typeof(ScarecrowHit));

        UpdateHP(dmg);
    }
    [PunRPC] public void UpdateHP(float dmg) { /* ü�¹� ���� �� */ }

}

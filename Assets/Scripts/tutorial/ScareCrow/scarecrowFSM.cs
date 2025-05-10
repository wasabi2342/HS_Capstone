using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;          // [PunRPC] 속성만 사용


/* ─────────────────────────────────────────────────────────── */
[RequireComponent(typeof(Animator))]
public class ScarecrowFSM : MonoBehaviour, IDamageable
{
    /* ───[Inspector] 스탯─── */
    [Header("Stats")]
    [SerializeField] public float maxHealth = 50000000f;
    [SerializeField] public float attackDur = 1f;   // Attack 애니 길이
    [SerializeField] public float hitStunTime = 0.3f;
    [SerializeField] public float attackInterval = 2f;  // ← 새로 추가
    /* ───[Inspector] 디버그 토글─── */
    [SerializeField] bool debugAttack;
    /* ─── 런타임 ─── */
    public Animator Anim { get; private set; }
    public float HP { get; private set; }

    /* 공격 ON/OFF */
    bool attackEnabled;
    public bool AttackEnabled => attackEnabled;
    public void SetAttackEnabled(bool on)
    {
        attackEnabled = on;
        if (!on) attackTimer = 0f;      // OFF 시 타이머 초기화
    }

    /* FSM */
    readonly Dictionary<System.Type, IState> states = new();
    IState currentState;

    /* 내부 타이머 (Idle 상태에서 사용) */
    [HideInInspector] public float attackTimer = 0f;

    /* ─── Unity Flow ─── */
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
        /* ── 인스펙터 토글 <-> 실제 FSM 상태 즉시 반영 ── */
        if (debugAttack && !attackEnabled)
        {
            // 토글 ON > 공격 활성 + 즉시 AttackState 진입
            SetAttackEnabled(true);
            ForceAttack();                     // ← 신속 전환
        }
        else if (!debugAttack && attackEnabled)
        {
            // 토글 OFF >  공격 비활성 + 즉시 IdleState 복귀
            SetAttackEnabled(false);
            TransitionTo(typeof(ScarecrowIdle));
        }

        currentState?.Execute();
    }
    #region Debug helpers
    /// <summary>
    /// 디버그·튜토리얼용: 현재 상태와 관계없이 즉시 AttackState로 진입시킨다.
    /// </summary>
    public void ForceAttack()
    {
        // 이미 공격 중이면 무시
        if (currentState is ScarecrowAttack)
            return;

        attackTimer = 0f;                              // 간격 타이머 리셋
        TransitionTo(typeof(ScarecrowAttack));         // Attack 상태로 강제 전환
    }
    #endregion
    /* ─── 헬퍼 ─── */
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

    /* ─── IDamageable ─── */
    public void TakeDamage(float dmg, Vector3 attackerPos,
                           AttackerType type = AttackerType.Default)
    {
        /* ── 디버그: 무기가 IDamageable 호출했는지 확인 ── */
        Debug.Log($"[ScarecrowFSM] TakeDamage() called  ▶  dmg={dmg}, from={type}");
        DamageToMaster(dmg, attackerPos);
    }

    [PunRPC]
    public void DamageToMaster(float dmg, Vector3 attackerPos)
    {
        /* ── 디버그: HP 실제 차감 & Hit 전환 ── */
        Debug.Log($"[ScarecrowFSM] DamageToMaster() ▶  -{dmg} HP  (before={HP})");

        HP = Mathf.Max(0f, HP - dmg);
        attackTimer = 0f;                     // 피격 시 공격 타이머 초기화
        TransitionTo(typeof(ScarecrowHit));

        UpdateHP(dmg);
    }
    [PunRPC] public void UpdateHP(float dmg) { /* 체력바 갱신 등 */ }

}

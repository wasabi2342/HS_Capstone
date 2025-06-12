using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using UnityEngine.UI;

#region Shared Player Types
/// <summary>
/// 모든 플레이어 공통 상태. EnemyAI 등은 이 값을 기준으로 생존 여부를 판단합니다.
/// </summary>
public enum PlayerState
{
    Alive,
    Stun,
    Death
}
#endregion

/// <summary>
/// White / Pink 등 모든 플레이어가 상속할 공통 컨트롤러.
/// 체력, 쿨타임, 네트워크 동기화 로직을 여기서 처리하고
/// 각 파생 클래스는 스킬·애니메이션만 구현합니다.
/// </summary>
public class PlayerController : MonoBehaviourPun
{
    // ───────────────────────────────────────── I N S P E C T O R ──────
    [Header("Hit‑lag Settings")]
    [SerializeField] protected float hitlagTime = 0.117f;

    [Header("UI References")]
    public Image stunOverlay;
    public Image stunSlider;
    public Image hpBar;

    #region Cooldown UI Events
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent<float, float> OnAttackCooldownUpdate;
    public UnityEvent<float, float> OnDashCooldownUpdate;
    public UnityEvent<float, float> ShiftCoolDownUpdate;
    public UnityEvent<float, float> UltimateCoolDownUpdate;
    public UnityEvent<float, float> MouseRightSkillCoolDownUpdate;
    public UnityEvent<float> AttackStackUpdate;
    public UnityEvent<float, float> ShieldUpdate;
    public UnityEvent<UIIcon, Color> SkillOutlineUpdate;
    #endregion

    // ───────────────────────────────────────── S T A T E ──────────────
    private PlayerState currentState;
    public virtual PlayerState CurrentState
    {
        get => currentState;
        protected set => currentState = value;
    }

    protected bool isInvincible = false;
    protected bool isSuperArmor = false;

    // ───────────────────────────────────────── S T A T S ──────────────
    [SerializeField] protected CharacterStats characterBaseStats;
    protected PlayerRunTimeData runTimeData;

    protected readonly List<Shield> shields = new();
    private const float maxShield = 100f;

    protected PlayerBlessing playerBlessing;
    protected Animator animator;
    public int attackStack = 0;

    public CooldownChecker[] cooldownCheckers = new CooldownChecker[(int)Skills.Max];

    // ───────────────────────────────────────── U N I T Y ──────────────
    protected virtual void Awake()
    {
        runTimeData = new PlayerRunTimeData(characterBaseStats);

        if (PhotonNetwork.IsConnected)
        {
            if (photonView.IsMine)
            {
                runTimeData.LoadFromJsonFile();
                photonView.RPC(nameof(UpdateHP), RpcTarget.Others, runTimeData.currentHealth);
            }
            else
            {
                RoomManager.Instance.AddPlayerDic(photonView.Owner.ActorNumber, gameObject);
            }
        }

        BindCooldown();

        runTimeData.currentHealth = characterBaseStats.maxHP;
        OnHealthChanged?.Invoke(1f);

        playerBlessing = GetComponent<PlayerBlessing>();

        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("Animator not found! (PlayerController)");

        animator.speed = runTimeData.attackSpeed;
    }

    // ───────────────────────────────────────── P U B L I C  A P I ────
    #region State Helpers
    public virtual void EnterStunState()
    {
        CurrentState = PlayerState.Stun;
        if (stunOverlay != null) stunOverlay.enabled = true;
    }

    public virtual void ExitStunState()
    {
        CurrentState = PlayerState.Alive;
        if (stunOverlay != null) stunOverlay.enabled = false;
    }

    public virtual void Die()
    {
        if (CurrentState == PlayerState.Death) return;
        CurrentState = PlayerState.Death;
        // 파생 클래스에서 사망 애니메이션 / 처리 추가
    }
    #endregion

    // ───────────────────────────────────────── C O O L D O W N ────────
    public virtual void BindCooldown()
    {
        cooldownCheckers[(int)Skills.Mouse_L] = new CooldownChecker(runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Cooldown, OnAttackCooldownUpdate, runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Stack);
        cooldownCheckers[(int)Skills.Mouse_R] = new CooldownChecker(runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Cooldown, MouseRightSkillCoolDownUpdate, runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Stack);
        cooldownCheckers[(int)Skills.Space] = new CooldownChecker(runTimeData.skillWithLevel[(int)Skills.Space].skillData.Cooldown, OnDashCooldownUpdate, runTimeData.skillWithLevel[(int)Skills.Space].skillData.Stack);
        cooldownCheckers[(int)Skills.Shift_L] = new CooldownChecker(runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.Cooldown, ShiftCoolDownUpdate, runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.Stack);
        cooldownCheckers[(int)Skills.R] = new CooldownChecker(runTimeData.skillWithLevel[(int)Skills.R].skillData.Cooldown, UltimateCoolDownUpdate, runTimeData.skillWithLevel[(int)Skills.R].skillData.Stack);
    }

    // ───────────────────────────────────────── D A M A G E ────────────
    public virtual void AddShield(float amount, float duration)
    {
        float totalShield = GetTotalShield();
        if (totalShield + amount > maxShield) amount = maxShield - totalShield;
        if (amount <= 0) return;

        Shield s = new(amount);
        shields.Add(s);
        StartCoroutine(RemoveShieldAfterTime(s, duration));
        ShieldUpdate?.Invoke(GetTotalShield(), maxShield);
    }

    private IEnumerator RemoveShieldAfterTime(Shield s, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (shields.Remove(s))
            ShieldUpdate?.Invoke(GetTotalShield(), maxShield);
    }

    public float GetTotalShield() => shields.Sum(x => x.amount);

    public virtual void TakeDamage(float damage, AttackerType attackerType = AttackerType.Default)
    {
        if (CurrentState == PlayerState.Death) return;

        // 실드 먼저 차감
        while (damage > 0 && shields.Count > 0)
        {
            if (shields[0].amount > damage)
            {
                shields[0].amount -= damage;
                ShieldUpdate?.Invoke(GetTotalShield(), maxShield);
                return;
            }
            damage -= shields[0].amount;
            shields.RemoveAt(0);
        }

        if (damage <= 0) return;

        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            // MasterClient로 전송
            photonView.RPC(nameof(DamageToMaster), RpcTarget.MasterClient, damage);
            return;
        }

        ApplyDamage(damage);
    }

    private void ApplyDamage(float damage)
    {
        runTimeData.currentHealth -= damage;
        runTimeData.currentHealth = Mathf.Clamp(runTimeData.currentHealth, 0, characterBaseStats.maxHP);
        OnHealthChanged?.Invoke(runTimeData.currentHealth / characterBaseStats.maxHP);

        if (runTimeData.currentHealth <= 0)
            Die();

        if (PhotonNetwork.InRoom)
            photonView.RPC(nameof(UpdateHP), RpcTarget.Others, runTimeData.currentHealth);
    }

    [PunRPC]
    public virtual void DamageToMaster(float damage)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ApplyDamage(damage);
    }

    [PunRPC]
    public virtual void UpdateHP(float hp)
    {
        runTimeData.currentHealth = Mathf.Clamp(hp, 0, characterBaseStats.maxHP);
        OnHealthChanged?.Invoke(runTimeData.currentHealth / characterBaseStats.maxHP);
        if (runTimeData.currentHealth <= 0) Die();
    }

    // ───────────────────────────────────────── C O O L D O W N  A P I ─
    public void StartAttackCooldown() => cooldownCheckers[(int)Skills.Mouse_L].Use(this);
    public void StartSpaceCooldown() => cooldownCheckers[(int)Skills.Space].Use(this);
    public void StartShiftCoolDown() => cooldownCheckers[(int)Skills.Shift_L].Use(this);
    public void StartUltimateCoolDown() => cooldownCheckers[(int)Skills.R].Use(this);
    public void StartMouseRCoolDown() => cooldownCheckers[(int)Skills.Mouse_R].Use(this);

    // ───────────────────────────────────────── B U F F S ──────────────
    public virtual void BuffAttackSpeed(float multiplier, float duration) => StartCoroutine(BuffAttackSpeedTimer(multiplier, duration));
    private IEnumerator BuffAttackSpeedTimer(float multiplier, float duration)
    {
        animator.speed = runTimeData.attackSpeed * multiplier;
        yield return new WaitForSeconds(duration);
        animator.speed = runTimeData.attackSpeed;
    }

    // Hit‑lag
    private IEnumerator HitlagCoroutine()
    {
        animator.speed = 0;
        yield return new WaitForSeconds(hitlagTime);
        animator.speed = 1;
    }
    public void StartHitlag() => StartCoroutine(HitlagCoroutine());

    // ───────────────────────────────────────── S A V E / L O A D ─────
    public virtual void SaveRunTimeData() => runTimeData.SaveToJsonFile();
    protected virtual void OnApplicationQuit()
    {
        if (photonView.IsMine)
            runTimeData.DeleteRunTimeData();
    }

    // ───────────────────────────────────────── A N I M  RPC ───────────
    [PunRPC] public virtual void SyncBoolParameter(string p, bool v) => animator.SetBool(p, v);
    public virtual void SetBoolParameter(string p, bool v) => photonView.RPC(nameof(SyncBoolParameter), RpcTarget.Others, p, v);

    [PunRPC] public virtual void SyncIntParameter(string p, int v) => animator.SetInteger(p, v);
    public virtual void SetIntParameter(string p, int v) => photonView.RPC(nameof(SyncIntParameter), RpcTarget.Others, p, v);

    // ───────────────────────────────────────── U T I L S ──────────────
    public float ReturnAttackPower() => runTimeData.attackPower;
    public float ReturnAbilityPower() => runTimeData.abilityPower;
}

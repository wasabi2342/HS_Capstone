using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class ParentPlayerController : MonoBehaviourPun, IDamageable
{

    // 죽음, 기절 관련 ui, 체력바 ui

    public Image stunOverlay;
    public Image stunSlider;
    public Image hpBar;

    #region Cooldown UI Events

    [Header("Cooldown Settings")]
    // 공격/대쉬 쿨타임 (임의 값)
    protected float attackCooldown = 1f;
    protected float dashCooldown = 1f;
    protected float shiftCoolDown = 3f; // 캐릭터의 능력치, 쿨타임 등 캐릭터 정보를 가진 scriptableobject 또는 구조체, 클래스 중 1개를 만들어 Start에서 능력치 연결 해줘야함
    protected float ultimateCoolDown = 30f; // 추후에 로그라이크 강화 요소 또한 불러와야 함
    protected float mouseRightCoolDown = 4f;

    // 체력 UI 업데이트 -> 체력바 갱신, 인자로 0~1의 정규화된 값을 전송
    public UnityEvent<float> OnHealthChanged;
    // 공격/대쉬 쿨타임 UI 갱신 이벤트 (0~1의 진행률 전달)
    public UnityEvent<float> OnAttackCooldownUpdate;
    public UnityEvent<float> OnDashCooldownUpdate;
    public UnityEvent<float> ShiftCoolDownUpdate;
    public UnityEvent<float> UltimateCoolDownUpdate;
    public UnityEvent<float> MouseRightSkillCoolDownUpdate;
    public UnityEvent<float> AttackStackUpdate;
    public UnityEvent<float, float> ShieldUpdate;
    public UnityEvent<UIIcon, Color> SkillOutlineUpdate;

    // 스킬 사용 가능 여부
    protected bool isShiftReady = true;
    protected bool isUltimateReady = true;
    protected bool isMouseRightSkillReady = true;
    protected bool isDashReady = true;

    protected bool isInvincible = false; // 무적 상태 체크용
    protected bool isSuperArmor = false; // 슈퍼아머 상태 체크용
    #endregion

    [SerializeField]
    protected CharacterStats characterBaseStats;
    protected PlayerRunTimeData runTimeData;

    // 실드
    protected List<Shield> shields = new List<Shield>();
    private readonly float maxShield = 100f;

    public int attackStack = 0;

    #region Unity Lifecycle

    protected virtual void Awake()
    {
        runTimeData = new PlayerRunTimeData(characterBaseStats.attackPower, characterBaseStats.attackSpeed, characterBaseStats.moveSpeed, characterBaseStats.cooldownReductionPercent, characterBaseStats.abilityPower, characterBaseStats.maxHP);
        if (PhotonNetwork.IsConnected)
        {
            if (photonView.IsMine)
            {
                runTimeData.LoadFromJsonFile();
                photonView.RPC("UpdateHP", RpcTarget.Others, runTimeData.currentHealth);
            }
            else
            {
                RoomManager.Instance.AddPlayerDic(photonView.Owner.ActorNumber, gameObject);
            }
        }

        attackCooldown = characterBaseStats.mouseLeftCooldown;
        dashCooldown = characterBaseStats.spaceCooldown;
        shiftCoolDown = characterBaseStats.shiftCooldown;
        ultimateCoolDown = characterBaseStats.ultimateCooldown;
        mouseRightCoolDown = characterBaseStats.mouseRightCooldown;

        runTimeData.currentHealth = characterBaseStats.maxHP;
        // 시작 시 체력 UI 업데이트
        OnHealthChanged?.Invoke(runTimeData.currentHealth / characterBaseStats.maxHP);
    }

    #endregion

    #region Damage & Health Synchronization

    public virtual void AddShield(float amount, float duration)
    {
        float totalShield = GetTotalShield(); // 현재 실드 총량

        // 최대 실드를 초과하지 않도록 조정
        if (totalShield + amount > maxShield)
        {
            amount = maxShield - totalShield; // 초과분 조정
        }
        if (amount > 0)
        {
            Shield newShield = new Shield(amount);
            shields.Add(newShield); // 실드 추가
            StartCoroutine(RemoveShieldAfterTime(newShield, duration)); // 코루틴 시작
            Debug.Log($"실드 추가! {amount} HP 실드 생성");
            ShieldUpdate?.Invoke(GetTotalShield(), maxShield);
        }
    }

    protected IEnumerator RemoveShieldAfterTime(Shield amount, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (shields.Contains(amount)) // 만료된 실드가 아직 남아 있으면 제거
        {
            shields.Remove(amount);
            ShieldUpdate?.Invoke(GetTotalShield(), maxShield);
            Debug.Log($"실드 {amount} 시간이 지나 제거됨");
        }
    }

    public float GetTotalShield()
    {
        float totalShield = 0;
        foreach (var shield in shields)
        {
            totalShield += shield.amount;
        }
        return totalShield;
    }

    // 2) 추가 파라미터 useRPC를 사용한 데미지 처리
    public virtual void TakeDamage(float damage)
    {

        if (PhotonNetwork.InRoom)
        {
            if (!photonView.IsMine) return;

            if (PhotonNetwork.IsMasterClient)
            {
                while (damage > 0 && shields.Count > 0)
                {
                    if (shields[0].amount > damage)
                    {
                        shields[0].amount -= damage;
                        Debug.Log($"실드로 {damage} 피해 흡수! 남은 실드: {shields[0].amount}");
                        ShieldUpdate?.Invoke(GetTotalShield(), maxShield);
                        return;
                    }
                    else
                    {
                        damage -= shields[0].amount;
                        Debug.Log($"실드 {shields[0]} 소진 후 파괴됨");
                        shields.RemoveAt(0);
                        ShieldUpdate?.Invoke(GetTotalShield(), maxShield);
                    }
                }
                if (damage == 0)
                {
                    return;
                }

                // Master Client는 직접 체력 계산
                runTimeData.currentHealth -= damage;
                runTimeData.currentHealth = Mathf.Clamp(runTimeData.currentHealth, 0, characterBaseStats.maxHP);

                // 체력바 UI 업데이트
                OnHealthChanged?.Invoke(runTimeData.currentHealth / characterBaseStats.maxHP);

                // 모든 클라이언트에 체력 동기화
                photonView.RPC("UpdateHP", RpcTarget.Others, runTimeData.currentHealth);
            }
            else
            {
                // Master Client가 아니라면, 피해량을 Master에 전송
                photonView.RPC("DamageToMaster", RpcTarget.MasterClient, damage);
            }
        }

        else
        {
            while (damage > 0 && shields.Count > 0)
            {
                if (shields[0].amount > damage)
                {
                    shields[0].amount -= damage;
                    Debug.Log($"실드로 {damage} 피해 흡수! 남은 실드: {shields[0].amount}");
                    return;
                }
                else
                {
                    damage -= shields[0].amount;
                    Debug.Log($"실드 {shields[0]} 소진 후 파괴됨");
                    shields.RemoveAt(0);
                }
            }
            if (damage == 0)
            {
                return;
            }

            // Master Client는 직접 체력 계산
            runTimeData.currentHealth -= damage;
            runTimeData.currentHealth = Mathf.Clamp(runTimeData.currentHealth, 0, characterBaseStats.maxHP);

            // 체력바 UI 업데이트
            OnHealthChanged?.Invoke(runTimeData.currentHealth / characterBaseStats.maxHP);
        }
    }



    [PunRPC]
    public virtual void DamageToMaster(float damage)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        runTimeData.currentHealth -= damage;
        runTimeData.currentHealth = Mathf.Clamp(runTimeData.currentHealth, 0, characterBaseStats.maxHP);

        // 체력바 UI 업데이트
        OnHealthChanged?.Invoke(runTimeData.currentHealth / characterBaseStats.maxHP);

        // 모든 클라이언트에 체력 동기화
        photonView.RPC("UpdateHP", RpcTarget.Others, runTimeData.currentHealth);
    }

    [PunRPC]
    public virtual void UpdateHP(float hp)
    {
        runTimeData.currentHealth = hp;
        runTimeData.currentHealth = Mathf.Clamp(runTimeData.currentHealth, 0, characterBaseStats.maxHP);
        OnHealthChanged?.Invoke(runTimeData.currentHealth / characterBaseStats.maxHP);
    }

    #endregion
    /// <summary>
    /// 무적상태 시작
    /// </summary>
    public virtual void EnterInvincibleState()
    {
        isInvincible = true;
    }

    /// <summary>
    /// 무적상태 종료
    /// </summary>
    public virtual void ExitInvincibleState()
    {
        isInvincible = false;
    }

    /// <summary>
    /// 슈퍼아머상태 시작
    /// </summary>
    public virtual void EnterSuperArmorState()
    {
        isSuperArmor = true;
    }

    /// <summary>
    /// 슈퍼아머 상태 종료
    /// </summary>
    public virtual void ExitSuperArmorState()
    {
        isSuperArmor = true;
    }

    /// <summary>
    /// 런타임데이터 json으로 저장
    /// </summary>
    public virtual void SaveRunTimeData()
    {
        runTimeData.SaveToJsonFile();
    }

    public virtual void UpdateBlessingRunTimeData(Dictionary<Skills, BlessingInfo> playerBlessingDic)
    {
        foreach (var data in playerBlessingDic)
        {
            runTimeData.blessingInfo[(int)data.Key] = data.Value;
        }
    }

    public virtual BlessingInfo[] ReturnBlessingRunTimeData()
    {
        return runTimeData.blessingInfo;
    }

    protected virtual void OnApplicationQuit()
    {
        if (!photonView.IsMine) // 내 데이터만 삭제 하도록
        {
            return;
        }
        Debug.Log("게임 종료됨!");
        runTimeData.DeleteRunTimeData();
    }

    /// <summary>
    /// value만큼 체력 회복
    /// </summary>
    /// <param name="value"></param>
    public virtual void RecoverHealth(float value)
    {
        if (!photonView.IsMine)
        {
            return;
        }
        runTimeData.currentHealth += value;
        runTimeData.currentHealth = Mathf.Clamp(runTimeData.currentHealth, 0, characterBaseStats.maxHP);
        OnHealthChanged?.Invoke(runTimeData.currentHealth / characterBaseStats.maxHP);
        photonView.RPC("UpdateHP", RpcTarget.Others, runTimeData.currentHealth);
    }

    #region Cooldown Handling (UI Update)

    public virtual void StartAttackCooldown()
    {
        StartCoroutine(AttackCooldownCoroutine());
    }

    private IEnumerator AttackCooldownCoroutine()
    {
        float timer = 0f;
        while (timer < attackCooldown)
        {
            OnAttackCooldownUpdate?.Invoke(timer / attackCooldown);
            timer += Time.deltaTime;
            yield return null;
        }
        OnAttackCooldownUpdate?.Invoke(1f);
    }

    public virtual void StartDashCooldown()
    {
        StartCoroutine(DashCooldownCoroutine());
    }

    private IEnumerator DashCooldownCoroutine()
    {
        float timer = 0f;
        while (timer < dashCooldown)
        {
            OnDashCooldownUpdate?.Invoke(timer / dashCooldown);
            timer += Time.deltaTime;
            yield return null;
        }
        OnDashCooldownUpdate?.Invoke(1f);
    }

    #endregion
}
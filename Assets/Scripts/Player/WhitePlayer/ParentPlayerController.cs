using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using System.Collections.Generic;

public class ParentPlayerController : MonoBehaviourPun, IDamageable
{
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

    public int attackStack = 0;

    #region Unity Lifecycle

    protected virtual void Awake()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        runTimeData = new PlayerRunTimeData(characterBaseStats.attackPower, characterBaseStats.attackSpeed, characterBaseStats.moveSpeed, characterBaseStats.cooldownReductionPercent, characterBaseStats.abilityPower, characterBaseStats.maxHP);

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



    // 2) 추가 파라미터 useRPC를 사용한 데미지 처리
    public virtual void TakeDamage(float damage)
    {

        if (PhotonNetwork.InRoom)
        {
            if (!photonView.IsMine) return;

            if (PhotonNetwork.IsMasterClient)
            {
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
        foreach(var data in playerBlessingDic)
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
        Debug.Log("게임 종료됨!");
        runTimeData.DeleteRunTimeData();
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
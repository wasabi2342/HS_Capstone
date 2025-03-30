using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using System.Collections.Generic;

public class ParentPlayerController : MonoBehaviourPun, IDamageable
{
    #region Cooldown UI Events

    [Header("Cooldown Settings")]
    protected CooldownManager cooldownManager;

    // 체력 UI 이벤트 -> 체력바 표시, 인자로 0~1로 정규화된 값을 전달
    public UnityEvent<float> OnHealthChanged;
    // 공격/대시 쿨타임 UI 관련 이벤트 (0~1의 정규화 값들)
    public UnityEvent<float> OnAttackCooldownUpdate;
    public UnityEvent<float> OnDashCooldownUpdate;
    public UnityEvent<float> ShiftCoolDownUpdate;
    public UnityEvent<float> UltimateCoolDownUpdate;
    public UnityEvent<float> MouseRightSkillCoolDownUpdate;
    public UnityEvent<float> AttackStackUpdate;
    public UnityEvent<UIIcon, Color> SkillOutlineUpdate;

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
        runTimeData = new PlayerRunTimeData(characterBaseStats.attackPower, characterBaseStats.attackSpeed, characterBaseStats.moveSpeed, characterBaseStats.cooldownReductionPercent, characterBaseStats.abilityPower, characterBaseStats.maxHP);
        if (photonView.IsMine)
        {
            runTimeData.LoadFromJsonFile();
            photonView.RPC("UpdateHP", RpcTarget.Others, runTimeData.currentHealth);
        }
        
        // 쿨다운 매니저 초기화
        cooldownManager = gameObject.AddComponent<CooldownManager>();
        cooldownManager.Initialize(characterBaseStats);
        
        // 쿨다운 이벤트 연결
        cooldownManager.OnCooldownUpdate.AddListener(HandleCooldownUpdate);
        cooldownManager.OnCooldownComplete.AddListener(HandleCooldownComplete);

        runTimeData.currentHealth = characterBaseStats.maxHP;
        // 초기 값 체력 UI 업데이트
        OnHealthChanged?.Invoke(runTimeData.currentHealth / characterBaseStats.maxHP);
    }

    protected virtual void OnDestroy()
    {
        if (cooldownManager != null)
        {
            cooldownManager.OnCooldownUpdate.RemoveListener(HandleCooldownUpdate);
            cooldownManager.OnCooldownComplete.RemoveListener(HandleCooldownComplete);
        }
    }

    protected void HandleCooldownUpdate(Skills skillType, float progress)
    {
        switch (skillType)
        {
            case Skills.Mouse_L:
                OnAttackCooldownUpdate?.Invoke(progress);
                break;
            case Skills.Mouse_R:
                MouseRightSkillCoolDownUpdate?.Invoke(progress);
                break;
            case Skills.Space:
                OnDashCooldownUpdate?.Invoke(progress);
                break;
            case Skills.Shift_L:
                ShiftCoolDownUpdate?.Invoke(progress);
                break;
            case Skills.R:
                UltimateCoolDownUpdate?.Invoke(progress);
                break;
        }
    }

    protected void HandleCooldownComplete(Skills skillType)
    {
        Debug.Log($"Skill {skillType} cooldown completed");
    }

    #endregion

    #region Damage & Health Synchronization

    public virtual void TakeDamage(float damage)
    {

        if (PhotonNetwork.InRoom)
        {
            if (!photonView.IsMine) return;

            if (PhotonNetwork.IsMasterClient)
            {
                // Master Client에서 체력 계산
                runTimeData.currentHealth -= damage;
                runTimeData.currentHealth = Mathf.Clamp(runTimeData.currentHealth, 0, characterBaseStats.maxHP);

                // 체력 UI 업데이트
                OnHealthChanged?.Invoke(runTimeData.currentHealth / characterBaseStats.maxHP);

                // 다른 클라이언트에 체력 동기화
                photonView.RPC("UpdateHP", RpcTarget.Others, runTimeData.currentHealth);
            }
            else
            {
                // Master Client가 아니면, 데미지를 Master에 전달
                photonView.RPC("DamageToMaster", RpcTarget.MasterClient, damage);
            }
        }

        else
        {
            // Master Client에서 체력 계산
            runTimeData.currentHealth -= damage;
            runTimeData.currentHealth = Mathf.Clamp(runTimeData.currentHealth, 0, characterBaseStats.maxHP);

            // 체력 UI 업데이트
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

        // 체력 UI 업데이트
        OnHealthChanged?.Invoke(runTimeData.currentHealth / characterBaseStats.maxHP);

        // 다른 클라이언트에 체력 동기화
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
    /// 무적상태 설정
    /// </summary>
    public virtual void EnterInvincibleState()
    {
        isInvincible = true;
    }

    /// <summary>
    /// 무적상태 해제
    /// </summary>
    public virtual void ExitInvincibleState()
    {
        isInvincible = false;
    }

    /// <summary>
    /// 슈퍼아머상태 설정
    /// </summary>
    public virtual void EnterSuperArmorState()
    {
        isSuperArmor = true;
    }

    /// <summary>
    /// 슈퍼아머 상태 해제
    /// </summary>
    public virtual void ExitSuperArmorState()
    {
        isSuperArmor = false;
    }

    /// <summary>
    /// 런타임데이터를 json으로 저장
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
        if (!photonView.IsMine) // 내 데이터만 저장 하도록
        {
            return;
        }
        Debug.Log("게임 종료됨!");
        runTimeData.DeleteRunTimeData();
    }

    #region Cooldown Handling

    public virtual void StartAttackCooldown()
    {
        cooldownManager.StartCooldown(Skills.Mouse_L);
    }

    public virtual void StartDashCooldown()
    {
        cooldownManager.StartCooldown(Skills.Space);
    }

    public virtual void StartShiftCooldown()
    {
        cooldownManager.StartCooldown(Skills.Shift_L);
    }

    public virtual void StartUltimateCooldown()
    {
        cooldownManager.StartCooldown(Skills.R);
    }

    public virtual void StartMouseRightCooldown()
    {
        cooldownManager.StartCooldown(Skills.Mouse_R);
    }

    public virtual bool IsSkillReady(Skills skillType)
    {
        return cooldownManager.IsSkillReady(skillType);
    }

    public virtual void ResetSkillCooldown(Skills skillType)
    {
        cooldownManager.ResetCooldown(skillType);
    }

    #endregion
}
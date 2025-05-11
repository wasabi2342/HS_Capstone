using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class ParentPlayerController : MonoBehaviourPun, IDamageable
{
    [SerializeField]
    protected float hitlagTime = 0.117f;
    [SerializeField]
    protected SpriteRenderer shadow;
    [SerializeField]
    protected TextMeshProUGUI nicknameText;

    public Transform footPivot;

    // 죽음, 기절 관련 ui, 체력바 ui

    public Image stunOverlay;
    public Image stunSlider;
    public Image hpBar;

    #region Cooldown UI Events

    // 체력 UI 업데이트 -> 체력바 갱신, 인자로 0~1의 정규화된 값을 전송
    public UnityEvent<float> OnHealthChanged;
    // 공격/대쉬 쿨타임 UI 갱신 이벤트 (0~1의 진행률 전달)
    public UnityEvent<float, float> OnAttackCooldownUpdate;
    public UnityEvent<float, float> OnDashCooldownUpdate;
    public UnityEvent<float, float> ShiftCoolDownUpdate;
    public UnityEvent<float, float> UltimateCoolDownUpdate;
    public UnityEvent<float, float> MouseRightSkillCoolDownUpdate;
    public UnityEvent<float> AttackStackUpdate;
    public UnityEvent<float, float> ShieldUpdate;
    public UnityEvent<UIIcon, Color> SkillOutlineUpdate;
    public UnityEvent OnHitEvent;


    public CooldownChecker[] cooldownCheckers = new CooldownChecker[(int)Skills.Max];

    protected bool isInvincible = false; // 무적 상태 체크용
    protected bool isSuperArmor = false; // 슈퍼아머 상태 체크용
    #endregion

    [SerializeField]
    protected CharacterStats characterBaseStats;
    protected PlayerRunTimeData runTimeData;

    protected Rigidbody rb;

    // 실드
    protected List<Shield> shields = new List<Shield>();
    private readonly float maxShield = 100f;

    protected PlayerBlessing playerBlessing;

    public Animator animator;

    public int attackStack = 0;

    public float damageBuff = 1;

    private bool isInPVPArea;

    #region Unity Lifecycle

    protected virtual void Awake()
    {
        runTimeData = new PlayerRunTimeData(characterBaseStats);

        BindCooldown();

        playerBlessing = GetComponent<PlayerBlessing>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator 컴포넌트를 찾을 수 없습니다! (WhitePlayerController)");
        }

        // 애니메이터 속도는 미리 설정
        animator.speed = runTimeData.attackSpeed;
    }

    protected virtual void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (photonView.IsMine)
            {
                runTimeData.LoadFromJsonFile();

                if (isInPVPArea)
                    runTimeData.currentHealth = characterBaseStats.maxHP;

                // 내 체력으로 동기화
                photonView.RPC("UpdateHP", RpcTarget.OthersBuffered, runTimeData.currentHealth);
                nicknameText.text = PhotonNetwork.CurrentRoom.Players[photonView.Owner.ActorNumber].NickName;
                nicknameText.color = new Color32(102, 204, 255, 255);

                // UI 갱신용 invoke
                OnHealthChanged?.Invoke(runTimeData.currentHealth / characterBaseStats.maxHP);

                // pvp 테스트 임시 코드
                //SetTeamId(PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                RoomManager.Instance.AddPlayerDic(photonView.Owner.ActorNumber, gameObject);
                nicknameText.text = PhotonNetwork.CurrentRoom.Players[photonView.Owner.ActorNumber].NickName;

                // 나와 팀 ID 비교
                object myTeamIdObj, otherTeamIdObj;
                PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("TeamId", out myTeamIdObj);
                photonView.Owner.CustomProperties.TryGetValue("TeamId", out otherTeamIdObj);

                if (myTeamIdObj != null && otherTeamIdObj != null && !myTeamIdObj.Equals(otherTeamIdObj))
                {
                    // 팀 ID 다르면 빨간색
                    nicknameText.color = Color.red;
                }
                else
                {
                    // 같은 팀 또는 TeamId 없음
                    nicknameText.color = new Color32(102, 255, 102, 255);
                }
            }
        }
    }

    #endregion

    public void SetIsInPVPArea(bool value)
    {
        isInPVPArea = value;
    }

    public void UpdateHP()
    {
        OnHealthChanged?.Invoke(runTimeData.currentHealth / characterBaseStats.maxHP);
    }

    public virtual void BindCooldown()
    {
        cooldownCheckers[(int)Skills.Mouse_L] = new CooldownChecker(runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Cooldown, OnAttackCooldownUpdate, runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Stack);
        cooldownCheckers[(int)Skills.Mouse_R] = new CooldownChecker(runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Cooldown, MouseRightSkillCoolDownUpdate, runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Stack);
        cooldownCheckers[(int)Skills.Space] = new CooldownChecker(runTimeData.skillWithLevel[(int)Skills.Space].skillData.Cooldown, OnDashCooldownUpdate, runTimeData.skillWithLevel[(int)Skills.Space].skillData.Stack);
        cooldownCheckers[(int)Skills.Shift_L] = new CooldownChecker(runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.Cooldown, ShiftCoolDownUpdate, runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.Stack);
        cooldownCheckers[(int)Skills.R] = new CooldownChecker(runTimeData.skillWithLevel[(int)Skills.R].skillData.Cooldown, UltimateCoolDownUpdate, runTimeData.skillWithLevel[(int)Skills.R].skillData.Stack);
    }

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

    [PunRPC]
    public void TakeDamageRPC(float damage, Vector3 pos, int attackerTypeInt)
    {
        TakeDamage(damage, pos, (AttackerType)attackerTypeInt);
    }

    // 2) 추가 파라미터 useRPC를 사용한 데미지 처리
    public virtual void TakeDamage(float damage, Vector3 attackerPos, AttackerType attackerType = AttackerType.Default)
    {

        if (PhotonNetwork.InRoom)
        {
            if (!photonView.IsMine) return;

            OnHitEvent?.Invoke();

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
                photonView.RPC("DamageToMaster", RpcTarget.MasterClient, damage, attackerPos);
            }
        }

        else
        {
            OnHitEvent?.Invoke();

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
    public virtual void DamageToMaster(float damage, Vector3 attackerPos)
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
        isSuperArmor = false;
    }

    /// <summary>
    /// 런타임데이터 json으로 저장
    /// </summary>
    public virtual void SaveRunTimeData()
    {
        runTimeData.SaveToJsonFile();
    }

    public virtual void UpdateBlessingRunTimeData(SkillWithLevel newData)
    {
        runTimeData.skillWithLevel[newData.skillData.Bind_Key] = newData;
        BindCooldown();
    }

    public virtual SkillWithLevel[] ReturnBlessingRunTimeData()
    {
        return runTimeData.skillWithLevel;
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
        cooldownCheckers[(int)Skills.Mouse_L].Use(this);
    }

    public virtual void StartSpaceCooldown()
    {
        cooldownCheckers[(int)Skills.Space].Use(this);
    }

    public virtual void StartShiftCoolDown() // 이벤트 클립으로 쿨타임 체크
    {
        cooldownCheckers[(int)Skills.Shift_L].Use(this);
    }

    public virtual void StartUltimateCoolDown() // 이벤트 클립으로 쿨타임 체크
    {
        cooldownCheckers[(int)Skills.R].Use(this);
    }

    public virtual void StartMouseRCoolDown() // 이벤트 클립으로 쿨타임 체크
    {
        cooldownCheckers[(int)Skills.Mouse_R].Use(this);
    }

    #endregion

    public virtual void BuffAttackSpeed(float value, float duration)
    {
        StartCoroutine(BuffAttackSpeedTimer(value, duration));
    }

    private IEnumerator BuffAttackSpeedTimer(float value, float duration)
    {
        animator.speed = runTimeData.attackSpeed * value;

        yield return new WaitForSeconds(duration);

        animator.speed = runTimeData.attackSpeed;
    }

    private IEnumerator PauseForSeconds()
    {
        animator.speed = 0;
        yield return new WaitForSeconds(hitlagTime);
        animator.speed = 1;
    }

    public void StartHitlag()
    {
        StartCoroutine(PauseForSeconds());
    }

    public float ReturnAttackPower()
    {
        return runTimeData.attackPower;
    }

    public float ReturnAbilityPower()
    {
        return runTimeData.abilityPower;
    }

    [PunRPC]
    public virtual void SyncBoolParameter(string parameter, bool value)
    {
        animator.SetBool(parameter, value);
    }

    public virtual void SetBoolParameter(string parameter, bool value)
    {
        photonView.RPC("SyncBoolParameter", RpcTarget.Others, parameter, value);
    }

    [PunRPC]
    public virtual void SyncIntParameter(string parameter, int value)
    {
        animator.SetInteger(parameter, value);
    }

    public virtual void SetIntParameter(string parameter, int value)
    {
        photonView.RPC("SyncIntParameter", RpcTarget.Others, parameter, value);
    }

    public string ReturnCharacterName()
    {
        return characterBaseStats.name;
    }

    [PunRPC]
    public virtual void CreateAnimation(string name, Vector3 pos, bool isChild)
    {
        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>(name), pos, Quaternion.identity);
        if(isChild)
            skillEffect.transform.parent = transform;
    }

    public virtual void ShadowOff()
    {
        shadow.enabled = false;
    }

    public virtual void ShadowOn()
    {
        shadow.enabled = true;
    }

    public void DeleteRuntimeData()
    {
        runTimeData.DeleteRunTimeData();
    }

    public void SetTeamId(int teamId)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "TeamId", teamId }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        Debug.Log($"TeamId가 {teamId}로 설정되었습니다.");
    }

}
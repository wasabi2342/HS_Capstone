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

    // ����, ���� ���� ui, ü�¹� ui

    public Image stunOverlay;
    public Image stunSlider;
    public Image hpBar;

    #region Cooldown UI Events

    // ü�� UI ������Ʈ -> ü�¹� ����, ���ڷ� 0~1�� ����ȭ�� ���� ����
    public UnityEvent<float> OnHealthChanged;
    // ����/�뽬 ��Ÿ�� UI ���� �̺�Ʈ (0~1�� ����� ����)
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

    protected bool isInvincible = false; // ���� ���� üũ��
    protected bool isSuperArmor = false; // ���۾Ƹ� ���� üũ��
    #endregion

    [SerializeField]
    protected CharacterStats characterBaseStats;
    protected PlayerRunTimeData runTimeData;

    protected Rigidbody rb;

    // �ǵ�
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
            Debug.LogError("Animator ������Ʈ�� ã�� �� �����ϴ�! (WhitePlayerController)");
        }

        // �ִϸ����� �ӵ��� �̸� ����
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

                // �� ü������ ����ȭ
                photonView.RPC("UpdateHP", RpcTarget.OthersBuffered, runTimeData.currentHealth);
                nicknameText.text = PhotonNetwork.CurrentRoom.Players[photonView.Owner.ActorNumber].NickName;
                nicknameText.color = new Color32(102, 204, 255, 255);

                // UI ���ſ� invoke
                OnHealthChanged?.Invoke(runTimeData.currentHealth / characterBaseStats.maxHP);

                // pvp �׽�Ʈ �ӽ� �ڵ�
                //SetTeamId(PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                RoomManager.Instance.AddPlayerDic(photonView.Owner.ActorNumber, gameObject);
                nicknameText.text = PhotonNetwork.CurrentRoom.Players[photonView.Owner.ActorNumber].NickName;

                // ���� �� ID ��
                object myTeamIdObj, otherTeamIdObj;
                PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("TeamId", out myTeamIdObj);
                photonView.Owner.CustomProperties.TryGetValue("TeamId", out otherTeamIdObj);

                if (myTeamIdObj != null && otherTeamIdObj != null && !myTeamIdObj.Equals(otherTeamIdObj))
                {
                    // �� ID �ٸ��� ������
                    nicknameText.color = Color.red;
                }
                else
                {
                    // ���� �� �Ǵ� TeamId ����
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
        float totalShield = GetTotalShield(); // ���� �ǵ� �ѷ�

        // �ִ� �ǵ带 �ʰ����� �ʵ��� ����
        if (totalShield + amount > maxShield)
        {
            amount = maxShield - totalShield; // �ʰ��� ����
        }
        if (amount > 0)
        {
            Shield newShield = new Shield(amount);
            shields.Add(newShield); // �ǵ� �߰�
            StartCoroutine(RemoveShieldAfterTime(newShield, duration)); // �ڷ�ƾ ����
            Debug.Log($"�ǵ� �߰�! {amount} HP �ǵ� ����");
            ShieldUpdate?.Invoke(GetTotalShield(), maxShield);
        }
    }

    protected IEnumerator RemoveShieldAfterTime(Shield amount, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (shields.Contains(amount)) // ����� �ǵ尡 ���� ���� ������ ����
        {
            shields.Remove(amount);
            ShieldUpdate?.Invoke(GetTotalShield(), maxShield);
            Debug.Log($"�ǵ� {amount} �ð��� ���� ���ŵ�");
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

    // 2) �߰� �Ķ���� useRPC�� ����� ������ ó��
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
                    Debug.Log($"�ǵ�� {damage} ���� ���! ���� �ǵ�: {shields[0].amount}");
                    ShieldUpdate?.Invoke(GetTotalShield(), maxShield);
                    return;
                }
                else
                {
                    damage -= shields[0].amount;
                    Debug.Log($"�ǵ� {shields[0]} ���� �� �ı���");
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

                // Master Client�� ���� ü�� ���
                runTimeData.currentHealth -= damage;
                runTimeData.currentHealth = Mathf.Clamp(runTimeData.currentHealth, 0, characterBaseStats.maxHP);

                // ü�¹� UI ������Ʈ
                OnHealthChanged?.Invoke(runTimeData.currentHealth / characterBaseStats.maxHP);

                // ��� Ŭ���̾�Ʈ�� ü�� ����ȭ
                photonView.RPC("UpdateHP", RpcTarget.Others, runTimeData.currentHealth);
            }
            else
            {
                // Master Client�� �ƴ϶��, ���ط��� Master�� ����
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
                    Debug.Log($"�ǵ�� {damage} ���� ���! ���� �ǵ�: {shields[0].amount}");
                    return;
                }
                else
                {
                    damage -= shields[0].amount;
                    Debug.Log($"�ǵ� {shields[0]} ���� �� �ı���");
                    shields.RemoveAt(0);
                }
            }
            if (damage == 0)
            {
                return;
            }

            // Master Client�� ���� ü�� ���
            runTimeData.currentHealth -= damage;
            runTimeData.currentHealth = Mathf.Clamp(runTimeData.currentHealth, 0, characterBaseStats.maxHP);

            // ü�¹� UI ������Ʈ
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

        // ü�¹� UI ������Ʈ
        OnHealthChanged?.Invoke(runTimeData.currentHealth / characterBaseStats.maxHP);

        // ��� Ŭ���̾�Ʈ�� ü�� ����ȭ
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
    /// �������� ����
    /// </summary>
    public virtual void EnterInvincibleState()
    {
        isInvincible = true;
    }

    /// <summary>
    /// �������� ����
    /// </summary>
    public virtual void ExitInvincibleState()
    {
        isInvincible = false;
    }

    /// <summary>
    /// ���۾Ƹӻ��� ����
    /// </summary>
    public virtual void EnterSuperArmorState()
    {
        isSuperArmor = true;
    }

    /// <summary>
    /// ���۾Ƹ� ���� ����
    /// </summary>
    public virtual void ExitSuperArmorState()
    {
        isSuperArmor = false;
    }

    /// <summary>
    /// ��Ÿ�ӵ����� json���� ����
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
        if (!photonView.IsMine) // �� �����͸� ���� �ϵ���
        {
            return;
        }
        Debug.Log("���� �����!");
        runTimeData.DeleteRunTimeData();
    }

    /// <summary>
    /// value��ŭ ü�� ȸ��
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

    public virtual void StartShiftCoolDown() // �̺�Ʈ Ŭ������ ��Ÿ�� üũ
    {
        cooldownCheckers[(int)Skills.Shift_L].Use(this);
    }

    public virtual void StartUltimateCoolDown() // �̺�Ʈ Ŭ������ ��Ÿ�� üũ
    {
        cooldownCheckers[(int)Skills.R].Use(this);
    }

    public virtual void StartMouseRCoolDown() // �̺�Ʈ Ŭ������ ��Ÿ�� üũ
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
        Debug.Log($"TeamId�� {teamId}�� �����Ǿ����ϴ�.");
    }

}
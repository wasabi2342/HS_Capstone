using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using System.Collections.Generic;

public class ParentPlayerController : MonoBehaviourPun, IDamageable
{
    #region Cooldown UI Events

    [Header("Cooldown Settings")]
    // ����/�뽬 ��Ÿ�� (���� ��)
    protected float attackCooldown = 1f;
    protected float dashCooldown = 1f;
    protected float shiftCoolDown = 3f; // ĳ������ �ɷ�ġ, ��Ÿ�� �� ĳ���� ������ ���� scriptableobject �Ǵ� ����ü, Ŭ���� �� 1���� ����� Start���� �ɷ�ġ ���� �������
    protected float ultimateCoolDown = 30f; // ���Ŀ� �α׶���ũ ��ȭ ��� ���� �ҷ��;� ��
    protected float mouseRightCoolDown = 4f;

    // ü�� UI ������Ʈ -> ü�¹� ����, ���ڷ� 0~1�� ����ȭ�� ���� ����
    public UnityEvent<float> OnHealthChanged;
    // ����/�뽬 ��Ÿ�� UI ���� �̺�Ʈ (0~1�� ����� ����)
    public UnityEvent<float> OnAttackCooldownUpdate;
    public UnityEvent<float> OnDashCooldownUpdate;
    public UnityEvent<float> ShiftCoolDownUpdate;
    public UnityEvent<float> UltimateCoolDownUpdate;
    public UnityEvent<float> MouseRightSkillCoolDownUpdate;
    public UnityEvent<float> AttackStackUpdate;

    // ��ų ��� ���� ����
    protected bool isShiftReady = true;
    protected bool isUltimateReady = true;
    protected bool isMouseRightSkillReady = true;
    protected bool isDashReady = true;

    protected bool isInvincible = false; // ���� ���� üũ��
    protected bool isSuperArmor = false; // ���۾Ƹ� ���� üũ��
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
        // ���� �� ü�� UI ������Ʈ
        OnHealthChanged?.Invoke(runTimeData.currentHealth / characterBaseStats.maxHP);
       
    }

    #endregion

    #region Damage & Health Synchronization



    // 2) �߰� �Ķ���� useRPC�� ����� ������ ó��
    public virtual void TakeDamage(float damage)
    {

        if (PhotonNetwork.InRoom)
        {
            if (!photonView.IsMine) return;

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
                photonView.RPC("DamageToMaster", RpcTarget.MasterClient, damage);
            }
        }

        else
        {
            // Master Client�� ���� ü�� ���
            runTimeData.currentHealth -= damage;
            runTimeData.currentHealth = Mathf.Clamp(runTimeData.currentHealth, 0, characterBaseStats.maxHP);

            // ü�¹� UI ������Ʈ
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
        isSuperArmor = true;
    }

    /// <summary>
    /// ��Ÿ�ӵ����� json���� ����
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
        Debug.Log("���� �����!");
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
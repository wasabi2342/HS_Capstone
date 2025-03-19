using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using System;

public class ParentPlayerController : MonoBehaviourPun, IDamageable
{
    #region Health Settings

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    // ü�� UI ������Ʈ -> ü�¹� ����, ���ڷ� 0~1�� ����ȭ�� ���� ����
    public UnityEvent<float> OnHealthChanged;

    #endregion

    #region Cooldown UI Events

    [Header("Cooldown Settings")]
    // ����/�뽬 ��Ÿ�� (���� ��)
    protected float attackCooldown = 1f;
    protected float dashCooldown = 1f;
    protected float shiftCoolDown = 3f; // ĳ������ �ɷ�ġ, ��Ÿ�� �� ĳ���� ������ ���� scriptableobject �Ǵ� ����ü, Ŭ���� �� 1���� ����� Start���� �ɷ�ġ ���� �������
    protected float ultimateCoolDown = 30f; // ���Ŀ� �α׶���ũ ��ȭ ��� ���� �ҷ��;� ��
    protected float guardCoolDown = 4f;


    // ����/�뽬 ��Ÿ�� UI ���� �̺�Ʈ (0~1�� ����� ����)
    public UnityEvent<float> OnAttackCooldownUpdate;
    public UnityEvent<float> OnDashCooldownUpdate;
    public UnityEvent<float> ShiftCoolDownUpdate;
    public UnityEvent<float> UltimateCoolDownUpdate;
    public UnityEvent<float> MouseRightSkillCoolDownUpdate;

    // ��ų ��� ���� ����
    protected bool isShiftReady = true;
    protected bool isUltimateReady = true;
    protected bool isMouseRightSkillReady = true;

    protected bool isInvincible = false; // ���� ���� üũ��
    protected bool isSuperArmor = false; // ���۾Ƹ� ���� üũ��
    #endregion

    #region Unity Lifecycle

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        // ���� �� ü�� UI ������Ʈ
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
        
        DontDestroyOnLoad(gameObject);
        
        if (PhotonNetwork.IsConnected && photonView.ViewID == 0)
        {
            PhotonNetwork.Destroy(gameObject);
        }
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
                currentHealth -= damage;
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

                // ü�¹� UI ������Ʈ
                OnHealthChanged?.Invoke(currentHealth / maxHealth);

                // ��� Ŭ���̾�Ʈ�� ü�� ����ȭ
                photonView.RPC("UpdateHP", RpcTarget.Others, currentHealth);
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
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            // ü�¹� UI ������Ʈ
            OnHealthChanged?.Invoke(currentHealth / maxHealth);
        }
    }



    [PunRPC]
    public virtual void DamageToMaster(float damage)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // ü�¹� UI ������Ʈ
        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        // ��� Ŭ���̾�Ʈ�� ü�� ����ȭ
        photonView.RPC("UpdateHP", RpcTarget.Others, currentHealth);
    }

    [PunRPC]
    public virtual void UpdateHP(float hp)
    {
        currentHealth = hp;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    #endregion

    public virtual void EnterInvincibleState()
    {
        isInvincible = true;
    }

    public virtual void ExitInvincibleState()
    {
        isInvincible = false;
    }

    public virtual void EnterSuperArmorState()
    {
        isSuperArmor = true;
    }

    public virtual void ExitSuperArmorState()
    {
        isSuperArmor = true;
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
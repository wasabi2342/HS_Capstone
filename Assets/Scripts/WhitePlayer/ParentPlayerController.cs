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
    protected float currentHealth;

    // 체력 UI 업데이트 -> 체력바 갱신, 인자로 0~1의 정규화된 값을 전송
    public UnityEvent<float> OnHealthChanged;

    #endregion

    #region Cooldown UI Events

    [Header("Cooldown Settings")]
    // 공격/대쉬 쿨타임 (임의 값)
    public float attackCooldown = 1f;
    public float dashCooldown = 1f;

    // 공격/대쉬 쿨타임 UI 갱신 이벤트 (0~1의 진행률 전달)
    public UnityEvent<float> OnAttackCooldownUpdate;
    public UnityEvent<float> OnDashCooldownUpdate;

    #endregion

    #region Unity Lifecycle

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        // 시작 시 체력 UI 업데이트
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
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
                currentHealth -= damage;
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

                // 체력바 UI 업데이트
                OnHealthChanged?.Invoke(currentHealth / maxHealth);

                // 모든 클라이언트에 체력 동기화
                photonView.RPC("UpdateHP", RpcTarget.Others, currentHealth);
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
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            // 체력바 UI 업데이트
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

        // 체력바 UI 업데이트
        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        // 모든 클라이언트에 체력 동기화
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
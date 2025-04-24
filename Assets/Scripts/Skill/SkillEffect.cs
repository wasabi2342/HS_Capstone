using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

public class SkillEffect : MonoBehaviourPun
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private float hitlagTime = 0.117f;

    private BaseSpecialEffect specialEffect;

    private float damage;
    private Action triggerEvent;
    private BoxCollider boxCollider;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage"> ������ </param>
    /// <param name="triggerEvent"> ������ �̺�Ʈ </param>
    public void Init(float damage, Action triggerEvent, BaseSpecialEffect specialEffect = null)
    {
        this.damage = damage;
        this.triggerEvent += triggerEvent;
        this.specialEffect = specialEffect;

        if (specialEffect != null && specialEffect.IsInstant())
        {
            specialEffect.ApplyEffect();
        }
    }

    public void OnAttackCollider()
    {
        boxCollider.enabled = true;
    }

    public void OffAttackCollider()
    {
        boxCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null && !other.CompareTag("Player"))
            {
                if (specialEffect != null)
                {
                    specialEffect.InjectCollider(other);
                }

                if (specialEffect != null && !specialEffect.IsInstant())
                {
                    specialEffect.ApplyEffect();
                }

                damageable.TakeDamage(damage);
                triggerEvent?.Invoke();
                StartCoroutine(PauseForSeconds());
            }
        }
    }

    private IEnumerator PauseForSeconds()
    {
        animator.speed = 0;
        yield return new WaitForSeconds(hitlagTime);
        animator.speed = 1;
    }

    private void OnDisable()
    {
        triggerEvent = null;
    }

    // ����Ʈ �� null ���� ������ִ� �̺�Ʈ �Լ�

    public void OnEffectEnd()
    {
        gameObject.SetActive(false);
    }
}

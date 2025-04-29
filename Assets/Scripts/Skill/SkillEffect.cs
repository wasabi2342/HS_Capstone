using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

public class SkillEffect : MonoBehaviourPun
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private float hitlagTime = 0.078f;
    [SerializeField]
    private AttackerType attackerType;
    [SerializeField]
    private float animationSpeed = 1f;

    private BaseSpecialEffect specialEffect;

    private float damage;
    private Action triggerEvent;
    private BoxCollider boxCollider;
    private bool isMine = false;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider>();
        animator.speed = animationSpeed;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage"> ������ </param>
    /// <param name="triggerEvent"> ������ �̺�Ʈ </param>
    public void Init(float damage, Action triggerEvent,bool isMine = false, BaseSpecialEffect specialEffect = null)
    {
        this.damage = damage;
        this.triggerEvent += triggerEvent;
        this.specialEffect = specialEffect;
        this.isMine = isMine;

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
        if (!PhotonNetwork.IsConnected || isMine)
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

                damageable.TakeDamage(damage, attackerType);
                triggerEvent?.Invoke();
                // Ÿ����
                switch (attackerType)
                {
                    case AttackerType.WhitePlayer:
                        AudioManager.Instance.PlayOneShot("event:/Character/Character-sword/katana_attack", transform.position);
                        break;
                    case AttackerType.PinkPlayer:

                        break;
                }
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

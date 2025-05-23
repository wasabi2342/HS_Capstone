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

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage"> 데미지 </param>
    /// <param name="triggerEvent"> 역경직 이벤트 </param>
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

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null && !other.CompareTag("Player"))
            {
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
}

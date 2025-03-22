using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

public class SkillEffect : MonoBehaviourPun
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private float hitlagTime = 0.13f;

    private float damage;
    private Action triggerEvent;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage"> 데미지 </param>
    /// <param name="triggerEvent"> 역경직 이벤트 </param>
    public void Init(float damage, Action triggerEvent)
    {
        this.damage = damage;
        this.triggerEvent += triggerEvent;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null && !other.CompareTag("Player"))
            {
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

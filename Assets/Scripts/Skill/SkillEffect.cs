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
    [SerializeField]
    private GameObject childEffect;

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
    public void Init(float damage, Action triggerEvent, bool isMine = false, BaseSpecialEffect specialEffect = null)
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

    public void CreateChildEffect()
    {
        Instantiate(childEffect, transform.position, transform.rotation, transform);
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
        if (!isMine)
            return;

        PhotonView otherView = other.GetComponent<PhotonView>();
        IDamageable damageable = other.GetComponent<IDamageable>();

        if (other.CompareTag("Player"))
        {
            if (other.gameObject == RoomManager.Instance.ReturnLocalPlayer())
                return;

            PhotonView localView = RoomManager.Instance.ReturnLocalPlayer().GetPhotonView();

            if (!TryGetTeamId(localView, out int myTeamId) || !TryGetTeamId(otherView, out int otherTeamId))
            {
                Debug.Log("TeamId�� �������� �ʾҽ��ϴ�.");
                return;
            }

            if (myTeamId != otherTeamId && otherView != null)
            {
                // �����ڿ��� �������� �����ϴ� RPC ����
                otherView.RPC("TakeDamageRPC", otherView.Owner, damage, (int)attackerType);

                ApplyAttackEffectOnly(other); // ����Ʈ �� ����
            }
        }
        else if (damageable != null)
        {
            ApplyAttackEffectOnly(other);
            damageable.TakeDamage(damage, attackerType);
        }
    }

    private bool TryGetTeamId(PhotonView view, out int teamId)
    {
        if (view.Owner.CustomProperties.TryGetValue("TeamId", out object value))
        {
            teamId = (int)value;
            return true;
        }
        teamId = -1;
        return false;
    }

    private void ApplyAttackEffectOnly(Collider other)
    {
        if (specialEffect != null)
        {
            specialEffect.InjectCollider(other);
            if (!specialEffect.IsInstant())
            {
                specialEffect.ApplyEffect();
            }
        }

        triggerEvent?.Invoke();

        switch (attackerType)
        {
            case AttackerType.WhitePlayer:
                AudioManager.Instance.PlayOneShot("event:/Character/Character-sword/katana_attack", transform.position);
                break;
            case AttackerType.PinkPlayer:
                AudioManager.Instance.PlayOneShot("event:/Character/Character-pink/mace_attack", transform.position);
                break;
        }

        StartCoroutine(PauseForSeconds());
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

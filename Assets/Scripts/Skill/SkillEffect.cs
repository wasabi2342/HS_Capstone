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
    private string soundClip = null;
    [SerializeField]
    private float animationSpeed = 1f;
    [SerializeField]
    private GameObject childEffect;

    private BaseSpecialEffect specialEffect;

    private float damage;
    private float knockbackMultiplier = 1f;  // ������ ��� �˹� ���� ����
    private Action triggerEvent;
    private BoxCollider boxCollider;
    private bool isMine = false;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider>();
        animator.speed = animationSpeed;
    }

    private Vector3 pushDirection = Vector3.zero;

    public void OnSetPushDirection()
    {
        // ����Ʈ ��������Ʈ/���� �ٶ󺸴� ������ ��������
        pushDirection = transform.forward;
    }

    /// (����) �˹� ���� ����
    public void SetKnockbackMultiplier(float multiplier)
    {
        this.knockbackMultiplier = multiplier;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage"> ������ </param>
    /// <param name="triggerEvent"> ������ �̺�Ʈ </param>
    public void Init(float damage, Action triggerEvent = null, bool isMine = false, BaseSpecialEffect specialEffect = null)
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

            ParentPlayerController parentPlayerController = other.GetComponent<ParentPlayerController>();

            if (parentPlayerController != null && parentPlayerController.IsStunState())
            {
                parentPlayerController.ReduceReviveTime();

                ApplySoundEffect();

                return;
            }

            PhotonView localView = RoomManager.Instance.ReturnLocalPlayer().GetPhotonView();

            if (!TryGetTeamId(localView, out int myTeamId) || !TryGetTeamId(otherView, out int otherTeamId))
            {
                Debug.Log("TeamId�� �������� �ʾҽ��ϴ�.");
                return;
            }

            if (myTeamId != otherTeamId && otherView != null)
            {
                // �����ڿ��� �������� �����ϴ� RPC ����
                otherView.RPC("TakeDamageRPC", otherView.Owner, damage, transform.position, (int)attackerType);

                ApplyAttackEffectOnly(other); // ����Ʈ �� ����

                // �˹� ����
                // ApplyKnockback(other);
            }
        }
        else if (damageable != null)
        {
            ApplyAttackEffectOnly(other);
            damageable.TakeDamage(damage, transform.position, attackerType);
            // �˹� ����
            ApplyKnockback(other);
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

        ApplySoundEffect();

        StartCoroutine(PauseForSeconds());
    }

    private void ApplySoundEffect()
    {
        if (soundClip == "")
        {
            switch (attackerType)
            {
                case AttackerType.WhitePlayer:
                    AudioManager.Instance.PlayOneShot("event:/Character/Character-sword/katana_attack", transform.position, RpcTarget.All);
                    break;
                case AttackerType.PinkPlayer:
                    AudioManager.Instance.PlayOneShot("event:/Character/Character-pink/mace_attack", transform.position, RpcTarget.All);
                    break;
            }
        }
        else
        {
            switch (attackerType)
            {
                case AttackerType.WhitePlayer:
                    AudioManager.Instance.PlayOneShot($"event:/Character/Character-sword/{soundClip}", transform.position, RpcTarget.All);
                    break;
                case AttackerType.PinkPlayer:
                    AudioManager.Instance.PlayOneShot($"event:/Character/Character-pink/{soundClip}", transform.position, RpcTarget.All);
                    break;
            }
        }
    }

    private IEnumerator PauseForSeconds()
    {
        animator.speed = 0;
        yield return new WaitForSeconds(hitlagTime);
        animator.speed = animationSpeed;
    }

    // ���ݼӵ� ����

    public void SetAttackSpeed(float value)
    {
        animationSpeed = value;
        animator.speed = animationSpeed;
    }


    // �˹�

    private void ApplyKnockback(Collider other)
    {
        // 1) Rigidbody ��������
        Rigidbody rb = other.attachedRigidbody ?? other.GetComponent<Rigidbody>();
        if (rb == null) return;

        // �˹� ���� ���
        Vector3 dir;
        // �ִϸ��̼� �̺�Ʈ�� ������ ���� ������ ������ �켱 ���
        if (pushDirection.sqrMagnitude > Mathf.Epsilon)
        {
            dir = pushDirection.normalized;
        }
        else
        {
            //  �浹 ������ �÷��̾�� ���� ��ġ ����
            Vector3 delta = other.transform.position - transform.position;
            delta.y = 0f;                    // ���� ���� ����
            if (delta.sqrMagnitude > Mathf.Epsilon)
            {
                dir = delta.normalized;      // �Ÿ��� 0�� �ƴϸ� �� ����
            }
            else
            {
                // ������ ��ģ ���¸�, ����Ʈ ����(�Ǵ� ���� ����)����
                dir = transform.forward;
            }
        }

        //�ּ� �и� �Ÿ� Ȯ�� (�о��)
        const float minDistance = 1f;
        float currentDist = Vector3.Distance(other.transform.position, transform.position);
        if (currentDist < minDistance)
        {
            Vector3 pushOut = dir * (minDistance - currentDist);
            rb.MovePosition(rb.position + pushOut);
        }

        //�ݵ�� �˹� �� ���� (�Ÿ� 0�̾ ������� ��)
        float force = damage * knockbackMultiplier;
        rb.AddForce(dir * force, ForceMode.VelocityChange);

        //���� �浹�� ���� ���� ���� �ʱ�ȭ
        pushDirection = Vector3.zero;
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

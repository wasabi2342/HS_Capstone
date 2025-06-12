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
    private float knockbackMultiplier = 1f;  // 데미지 대비 넉백 세기 비율
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
        // 이펙트 스프라이트/모델이 바라보는 앞쪽을 기준으로
        pushDirection = transform.forward;
    }

    /// (기존) 넉백 세기 설정
    public void SetKnockbackMultiplier(float multiplier)
    {
        this.knockbackMultiplier = multiplier;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage"> 데미지 </param>
    /// <param name="triggerEvent"> 역경직 이벤트 </param>
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
                Debug.Log("TeamId가 설정되지 않았습니다.");
                return;
            }

            if (myTeamId != otherTeamId && otherView != null)
            {
                // 피해자에게 데미지를 적용하는 RPC 전송
                otherView.RPC("TakeDamageRPC", otherView.Owner, damage, transform.position, (int)attackerType);

                ApplyAttackEffectOnly(other); // 이펙트 및 사운드

                // 넉백 적용
                // ApplyKnockback(other);
            }
        }
        else if (damageable != null)
        {
            ApplyAttackEffectOnly(other);
            damageable.TakeDamage(damage, transform.position, attackerType);
            // 넉백 적용
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

    // 공격속도 조절

    public void SetAttackSpeed(float value)
    {
        animationSpeed = value;
        animator.speed = animationSpeed;
    }


    // 넉백

    private void ApplyKnockback(Collider other)
    {
        // 1) Rigidbody 가져오기
        Rigidbody rb = other.attachedRigidbody ?? other.GetComponent<Rigidbody>();
        if (rb == null) return;

        // 넉백 방향 계산
        Vector3 dir;
        // 애니메이션 이벤트로 설정된 전진 방향이 있으면 우선 사용
        if (pushDirection.sqrMagnitude > Mathf.Epsilon)
        {
            dir = pushDirection.normalized;
        }
        else
        {
            //  충돌 시점의 플레이어와 몬스터 위치 차이
            Vector3 delta = other.transform.position - transform.position;
            delta.y = 0f;                    // 수직 성분 제거
            if (delta.sqrMagnitude > Mathf.Epsilon)
            {
                dir = delta.normalized;      // 거리가 0이 아니면 그 방향
            }
            else
            {
                // 완전히 겹친 상태면, 이펙트 앞쪽(또는 공격 방향)으로
                dir = transform.forward;
            }
        }

        //최소 분리 거리 확보 (밀어내기)
        const float minDistance = 1f;
        float currentDist = Vector3.Distance(other.transform.position, transform.position);
        if (currentDist < minDistance)
        {
            Vector3 pushOut = dir * (minDistance - currentDist);
            rb.MovePosition(rb.position + pushOut);
        }

        //반드시 넉백 힘 적용 (거리 0이어도 여기까지 옴)
        float force = damage * knockbackMultiplier;
        rb.AddForce(dir * force, ForceMode.VelocityChange);

        //다음 충돌을 위해 전진 방향 초기화
        pushDirection = Vector3.zero;
    }




    private void OnDisable()
    {
        triggerEvent = null;
    }

    // 이펙트 값 null 값을 만들어주는 이벤트 함수

    public void OnEffectEnd()
    {
        gameObject.SetActive(false);
    }



}

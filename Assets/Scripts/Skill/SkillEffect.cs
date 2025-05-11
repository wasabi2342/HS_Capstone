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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage"> 데미지 </param>
    /// <param name="triggerEvent"> 역경직 이벤트 </param>
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
                Debug.Log("TeamId가 설정되지 않았습니다.");
                return;
            }

            if (myTeamId != otherTeamId && otherView != null)
            {
                // 피해자에게 데미지를 적용하는 RPC 전송
                otherView.RPC("TakeDamageRPC", otherView.Owner, damage, transform.position, (int)attackerType);

                ApplyAttackEffectOnly(other); // 이펙트 및 사운드

                // 넉백 적용
                ApplyKnockback(other);
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

    // 넉백

    private void ApplyKnockback(Collider other)
    {
        // 1) Rigidbody 가져오기
        Rigidbody rb = other.attachedRigidbody ?? other.GetComponent<Rigidbody>();
        if (rb == null) return;

        // 2) 수평 벡터만 사용 (Y축 차이는 무시)
        Vector3 delta3D = other.transform.position - transform.position;
        Vector3 delta = new Vector3(delta3D.x, 0f, delta3D.z);
        float dist = delta.magnitude;

        // 3) 방향 계산 (거리가 0에 매우 가깝지 않을 때만 실제 벡터 사용)
        Vector3 dir;
        if (delta.sqrMagnitude > Mathf.Epsilon)  // 거의 “0,0,0”이 아닐 때
        {
            dir = delta / dist;
        }
        else
        {
            // 완전히 겹친 상태일 땐 오른쪽으로 밀어냄(필요시 -transform.right 으로 바꾸세요)
            dir = transform.right;
        }

        // 4) 최소 분리 거리 확보 (MovePosition 사용)
        const float minDistance = 1f;
        if (dist < minDistance)
        {
            Vector3 pushOut = dir * (minDistance - dist);
            rb.MovePosition(rb.position + pushOut);
        }

        // 5) 넉백 힘 적용
        float force = damage * knockbackMultiplier;
        rb.AddForce(dir * force, ForceMode.VelocityChange);
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

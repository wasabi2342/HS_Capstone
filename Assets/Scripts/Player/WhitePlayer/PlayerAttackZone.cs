using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;

[RequireComponent(typeof(SphereCollider))]
public class WhitePlayerAttackZone : MonoBehaviourPun
{
    [Header("공격 범위 설정")]
    [Tooltip("이 영역의 반지름이 플레이어의 공격 범위입니다.")]
    public float attackRange = 2f;

    // 범위 내에 있는 적 오브젝트 목록
    public List<GameObject> enemiesInRange = new List<GameObject>();

    // SphereCollider 캐싱
    private SphereCollider sphereCollider;

    [SerializeField]
    private BoxCollider skillCollider;
    [SerializeField]
    private BoxCollider counterCollider;

    private Animator animator;

    public float Damage;

    [SerializeField]
    private float hitlagTime = 0.13f;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        animator = GetComponentInParent<Animator>();
        if (sphereCollider != null)
        {
            sphereCollider.isTrigger = true;
            sphereCollider.radius = attackRange;
        }
    }

    // 공격 콜라이더를 켜거나 끄는 메서드
    public void EnableAttackCollider(bool enable)
    {
        if (sphereCollider != null)
        {
            sphereCollider.enabled = enable;
        }
    }

    public void EnableSkillAttackCollider(bool enable, bool isRight = true)
    {
        if (enable)
        {
            if (isRight)
            {
                skillCollider.center = new Vector3(3, 0, 0);
            }
            else
            {
                skillCollider.center = new Vector3(-3, 0, 0);
            }
        }

        if (skillCollider != null)
        {
            skillCollider.enabled = enable;
        }
    }

    public void EnableCounterAttackCollider(bool enable, bool isRight = true)
    {
        if (enable)
        {
            if (isRight)
            {
                counterCollider.center = new Vector3(3.5f, 0, 0);
            }
            else
            {
                counterCollider.center = new Vector3(-3.5f, 0, 0);
            }
        }

        if (counterCollider != null)
        {
            counterCollider.enabled = enable;
        }
    }

    private IEnumerator PauseForSeconds()
    {
        animator.speed = 0;
        yield return new WaitForSeconds(hitlagTime);
        animator.speed = 1;
    }

    public void StartHitlag()
    {
        StartCoroutine(PauseForSeconds());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }
        if (other.transform.parent == transform.parent)
        {
            return; // 같은 부모를 공유하는 경우 공격하지 않음
        }

        //other.transform.parent.GetComponentInChildren<IDamageable>().TakeDamage(Damage); 플레이어 때리기

        if (other.GetComponent<IDamageable>() != null)
        {
            other.GetComponent<IDamageable>().TakeDamage(Damage);
            StartHitlag();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (enemiesInRange.Contains(other.gameObject))
            {
                enemiesInRange.Remove(other.gameObject);
                Debug.Log("[WhitePlayerAttackZone] 적 제거: " + other.gameObject.name);
            }
        }
    }
}

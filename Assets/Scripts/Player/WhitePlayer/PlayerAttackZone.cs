using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;
using System;

[RequireComponent(typeof(SphereCollider))]
public class WhitePlayerAttackZone : MonoBehaviourPun
{
    [Header("���� ���� ����")]
    [Tooltip("�� ������ �������� �÷��̾��� ���� �����Դϴ�.")]
    public float attackRange = 2f;

    // ���� ���� �ִ� �� ������Ʈ ���
    public List<GameObject> enemiesInRange = new List<GameObject>();

    // SphereCollider ĳ��
    private SphereCollider sphereCollider;

    [SerializeField]
    private BoxCollider skillCollider;
    [SerializeField]
    private BoxCollider counterCollider;
    [SerializeField]
    private BoxCollider ultimateCollider;

    private Action<Collider> abilityAction;
    private bool onlyOnce;
    private bool isFirstHit;
    private Animator animator;

    public float Damage;

    [SerializeField]
    private float hitlagTime = 0.117f;

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

    // ���� �ݶ��̴��� �Ѱų� ���� �޼���
    public void EnableAttackCollider(bool enable)
    {
        if (sphereCollider != null)
        {
            sphereCollider.enabled = enable;
        }
    }

    public void EnableSkillAttackCollider(bool enable, bool isRight = false, bool onlyOnce = false, Action<Collider> ability = null)
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
            abilityAction = ability;
            this.onlyOnce = onlyOnce;
            isFirstHit = true;
        }
        else
        {
            abilityAction = null;
        }

        if (skillCollider != null)
        {
            skillCollider.enabled = enable;
        }
    }

    public void EnableCounterAttackCollider(bool enable, bool isRight = false, bool onlyOnce = false, Action<Collider> ability = null)
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
            abilityAction = ability;
            this.onlyOnce = onlyOnce;
            isFirstHit = true;
        }
        else
        {
            abilityAction = null;
        }

        if (counterCollider != null)
        {
            counterCollider.enabled = enable;
        }
    }

    public void EnableUltimateCollider(bool enable, bool isRight = false, bool onlyOnce = false, Action<Collider> ability = null)
    {
        if (enable)
        {
            if (isRight)
            {
                ultimateCollider.center = new Vector3(-8.5f, 0, 0);
            }
            else
            {
                ultimateCollider.center = new Vector3(8.5f, 0, 0);
            }
            abilityAction = ability;
            this.onlyOnce = onlyOnce;
            isFirstHit = true;
        }
        else
        {
            abilityAction = null;
        }

        if (ultimateCollider != null)
        {
            ultimateCollider.enabled = enable;
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
            return; // ���� �θ� �����ϴ� ��� �������� ����
        }

        //other.transform.parent.GetComponentInChildren<IDamageable>().TakeDamage(Damage); �÷��̾� ������

        if (other.GetComponent<IDamageable>() != null)
        {
            if (!onlyOnce || isFirstHit)
            {
                abilityAction?.Invoke(other);
                isFirstHit = false;
            }
            other.GetComponent<IDamageable>().TakeDamage(Damage, transform.position);
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
                Debug.Log("[WhitePlayerAttackZone] �� ����: " + other.gameObject.name);
            }
        }
    }
}

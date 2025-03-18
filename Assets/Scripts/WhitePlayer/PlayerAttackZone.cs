using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class WhitePlayerAttackZone : MonoBehaviour
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

    // ���� �ݶ��̴��� �Ѱų� ���� �޼���
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
        if (other.GetComponent<IDamageable>() != null)
        {
            other.GetComponent<IDamageable>().TakeDamage(Damage);
            StartHitlag();
        }
    }

    private void OnTriggerExit(Collider other)
    {
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

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

    private Animator animator;

    public float Damage;

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

    private IEnumerator PauseForSeconds(float seconds)
    {
        animator.speed = 0; 
        yield return new WaitForSeconds(seconds); 
        animator.speed = 1; 
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IDamageable>() != null)
        {
            other.GetComponent<IDamageable>().TakeDamage(Damage);
            StartCoroutine(PauseForSeconds(0.13f));
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

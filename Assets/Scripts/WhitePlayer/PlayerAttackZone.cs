using UnityEngine;
using System.Collections.Generic;

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

    public float Damage;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
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

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<IDamageable>() != null)
        {
            other.GetComponent<IDamageable>().TakeDamage(Damage);
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

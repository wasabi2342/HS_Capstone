using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class PlayerAttackZone : MonoBehaviour
{
    [Header("���� ���� ����")]
    [Tooltip("�� ������ �������� �÷��̾��� ���� �����Դϴ�.")]
    public float attackRange = 2f;

    // ���� ���� �ִ� �� ������Ʈ ���
    public List<GameObject> enemiesInRange = new List<GameObject>();

    private void Awake()
    {
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = attackRange;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // "Enemy" ���̾ �ִ� ������Ʈ�� �߰�
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (!enemiesInRange.Contains(other.gameObject))
            {
                enemiesInRange.Add(other.gameObject);
                Debug.Log("[PlayerAttackZone] �� �߰�: " + other.gameObject.name);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (enemiesInRange.Contains(other.gameObject))
            {
                enemiesInRange.Remove(other.gameObject);
                Debug.Log("[PlayerAttackZone] �� ����: " + other.gameObject.name);
            }
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class PlayerAttackZone : MonoBehaviour
{
    [Header("공격 범위 설정")]
    [Tooltip("이 영역의 반지름이 플레이어의 공격 범위입니다.")]
    public float attackRange = 2f;

    // 범위 내에 있는 적 오브젝트 목록
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
        // "Enemy" 레이어에 있는 오브젝트만 추가
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (!enemiesInRange.Contains(other.gameObject))
            {
                enemiesInRange.Add(other.gameObject);
                Debug.Log("[PlayerAttackZone] 적 추가: " + other.gameObject.name);
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
                Debug.Log("[PlayerAttackZone] 적 제거: " + other.gameObject.name);
            }
        }
    }
}

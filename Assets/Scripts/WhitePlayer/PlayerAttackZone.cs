using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class WhitePlayerAttackZone : MonoBehaviour
{
    [Header("공격 범위 설정")]
    [Tooltip("이 영역의 반지름이 플레이어의 공격 범위입니다.")]
    public float attackRange = 2f;

    // 범위 내에 있는 적 오브젝트 목록
    public List<GameObject> enemiesInRange = new List<GameObject>();

    // SphereCollider 캐싱
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

    // 공격 콜라이더를 켜거나 끄는 메서드
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
                Debug.Log("[WhitePlayerAttackZone] 적 제거: " + other.gameObject.name);
            }
        }
    }
}

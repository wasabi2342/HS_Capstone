using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PlayerDamageZone : MonoBehaviour
{
    [Header("이 영역에 들어온 총알에게 받는 데미지")]
    public int bulletDamage = 10;

    private PlayerController playerController;

    private void Awake()
    {
        // 자동으로 SphereCollider를 Trigger로 설정
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // 상위에 있는 PlayerController를 찾는다
        playerController = GetComponentInParent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("[PlayerDamageZone] 상위에 PlayerController가 없습니다!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[DamageZone] 충돌한 오브젝트: " + other.gameObject.name);

        if (other.CompareTag("Bullet"))
        {
            Debug.Log("[DamageZone] 총알 충돌 감지됨.");
            if (playerController != null)
            {
                playerController.TakeDamage(bulletDamage);
                Debug.Log("[DamageZone] 플레이어가 " + bulletDamage + " 데미지를 입었습니다.");
            }
            Destroy(other.gameObject);
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PlayerReceiveDamageZone : MonoBehaviour
{
    [Header("총알 데미지 설정")]
    public int bulletDamage = 10;

    private PlayerController playerController;

    private void Awake()
    {
        // SphereCollider를 자동으로 Trigger로 설정
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // 상위에 있는 PlayerController를 찾음
        playerController = GetComponentInParent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("[PlayerReceiveDamageZone] 상위에 PlayerController가 없습니다!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[PlayerReceiveDamageZone] 충돌한 오브젝트: " + other.gameObject.name);
        // 예시로 "Bullet" 태그가 있는 총알이 충돌하면 데미지 적용
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("[PlayerReceiveDamageZone] 총알 충돌 감지됨.");
            if (playerController != null)
            {
                playerController.TakeDamage(bulletDamage);
                Debug.Log("[PlayerReceiveDamageZone] 플레이어가 " + bulletDamage + " 데미지를 입었습니다.");
            }
            Destroy(other.gameObject);
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class WhitePlayerReceiveDamageZone : MonoBehaviour
{
    [Header("총알 데미지 설정")]
    public int bulletDamage = 10;

    private WhitePlayerController whitePlayerController;

    private void Awake()
    {
        // SphereCollider를 자동으로 Trigger로 설정
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // 상위에 있는 WhitePlayerController를 찾음
        whitePlayerController = GetComponentInParent<WhitePlayerController>();
        if (whitePlayerController == null)
        {
            Debug.LogWarning("[WhitePlayerReceiveDamageZone] 상위에 WhitePlayerController가 없습니다!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[WhitePlayerReceiveDamageZone] 충돌한 오브젝트: " + other.gameObject.name);
        // "Bullet" 태그가 있는 총알이 충돌하면 데미지 적용
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("[WhitePlayerReceiveDamageZone] 총알 충돌 감지됨.");
            if (whitePlayerController != null)
            {
                whitePlayerController.TakeDamage(bulletDamage);
                Debug.Log("[WhitePlayerReceiveDamageZone] 플레이어가 " + bulletDamage + " 데미지를 입었습니다.");
            }
            Destroy(other.gameObject);
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PlayerDamageZone : MonoBehaviour
{
    [Header("이 영역에 들어온 총알에게 받는 데미지")]
    public int bulletDamage = 10;

    
    public PlayerController playerController;

    private void Awake()
    {
        
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null) col.isTrigger = true;
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

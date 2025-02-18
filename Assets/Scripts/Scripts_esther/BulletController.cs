using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BulletController : MonoBehaviour
{
    [Header("총알 데미지 설정")]
    public int damage = 10;       // Inspector에서 설정 가능
    [Header("총알 이동 속도")]
    public float speed = 10f;     // 총알 이동 속도
    [Header("총알 수명 (초)")]
    public float lifeTime = 5f;   // lifeTime 후 자동 삭제

    void Start()
    {
        // 일정 시간 후 파괴
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 전방 이동
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Player 태그를 가진 오브젝트와 충돌 시
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // 플레이어에게 데미지
                player.TakeDamage(damage);

                // (원하면 추가 로그)
                Debug.Log($"[Bullet] 플레이어에게 {damage} 데미지!");
            }

            // 총알 파괴
            Destroy(gameObject);
        }
    }
}

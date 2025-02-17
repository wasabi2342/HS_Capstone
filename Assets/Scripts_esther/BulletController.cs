using UnityEngine;

public class BulletController : MonoBehaviour
{
    public int damage = 10;      // Inspector에서 설정 가능한 총알 데미지
    public float speed = 10f;    // 총알 이동 속도
    public float lifeTime = 5f;  // 총알 수명 (lifeTime 후 자동 삭제)

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}

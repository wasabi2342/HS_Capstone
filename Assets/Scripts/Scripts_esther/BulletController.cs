using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BulletController : MonoBehaviour
{
    [Header("총알 데미지 설정")]
    public int damage = 10;

    [Header("총알 이동 속도")]
    public float speed = 10f;

    [Header("총알 수명 (초)")]
    public float lifeTime = 5f;

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
                Debug.Log($"[Bullet] 플레이어에게 {damage} 데미지!");
            }
            Destroy(gameObject);
        }
       
    }
}

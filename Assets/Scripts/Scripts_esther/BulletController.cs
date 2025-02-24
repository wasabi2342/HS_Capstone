using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BulletController : MonoBehaviour
{
    [Header("�Ѿ� ������ ����")]
    public int damage = 10;

    [Header("�Ѿ� �̵� �ӵ�")]
    public float speed = 10f;

    [Header("�Ѿ� ���� (��)")]
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
                Debug.Log($"[Bullet] �÷��̾�� {damage} ������!");
            }
            Destroy(gameObject);
        }
       
    }
}

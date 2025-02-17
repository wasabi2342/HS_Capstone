using UnityEngine;

public class BulletController : MonoBehaviour
{
    public int damage = 10;      // Inspector���� ���� ������ �Ѿ� ������
    public float speed = 10f;    // �Ѿ� �̵� �ӵ�
    public float lifeTime = 5f;  // �Ѿ� ���� (lifeTime �� �ڵ� ����)

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

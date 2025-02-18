using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BulletController : MonoBehaviour
{
    [Header("�Ѿ� ������ ����")]
    public int damage = 10;       // Inspector���� ���� ����
    [Header("�Ѿ� �̵� �ӵ�")]
    public float speed = 10f;     // �Ѿ� �̵� �ӵ�
    [Header("�Ѿ� ���� (��)")]
    public float lifeTime = 5f;   // lifeTime �� �ڵ� ����

    void Start()
    {
        // ���� �ð� �� �ı�
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // ���� �̵�
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Player �±׸� ���� ������Ʈ�� �浹 ��
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // �÷��̾�� ������
                player.TakeDamage(damage);

                // (���ϸ� �߰� �α�)
                Debug.Log($"[Bullet] �÷��̾�� {damage} ������!");
            }

            // �Ѿ� �ı�
            Destroy(gameObject);
        }
    }
}

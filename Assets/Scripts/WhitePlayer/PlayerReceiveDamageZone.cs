using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PlayerReceiveDamageZone : MonoBehaviour
{
    [Header("�Ѿ� ������ ����")]
    public int bulletDamage = 10;

    private PlayerController playerController;

    private void Awake()
    {
        // SphereCollider�� �ڵ����� Trigger�� ����
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // ������ �ִ� PlayerController�� ã��
        playerController = GetComponentInParent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("[PlayerReceiveDamageZone] ������ PlayerController�� �����ϴ�!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[PlayerReceiveDamageZone] �浹�� ������Ʈ: " + other.gameObject.name);
        // ���÷� "Bullet" �±װ� �ִ� �Ѿ��� �浹�ϸ� ������ ����
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("[PlayerReceiveDamageZone] �Ѿ� �浹 ������.");
            if (playerController != null)
            {
                playerController.TakeDamage(bulletDamage);
                Debug.Log("[PlayerReceiveDamageZone] �÷��̾ " + bulletDamage + " �������� �Ծ����ϴ�.");
            }
            Destroy(other.gameObject);
        }
    }
}

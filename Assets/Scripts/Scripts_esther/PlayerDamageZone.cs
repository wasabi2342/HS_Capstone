using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PlayerDamageZone : MonoBehaviour
{
    [Header("�� ������ ���� �Ѿ˿��� �޴� ������")]
    public int bulletDamage = 10;

    private PlayerController playerController;

    private void Awake()
    {
        // �ڵ����� SphereCollider�� Trigger�� ����
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // ������ �ִ� PlayerController�� ã�´�
        playerController = GetComponentInParent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("[PlayerDamageZone] ������ PlayerController�� �����ϴ�!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[DamageZone] �浹�� ������Ʈ: " + other.gameObject.name);

        if (other.CompareTag("Bullet"))
        {
            Debug.Log("[DamageZone] �Ѿ� �浹 ������.");
            if (playerController != null)
            {
                playerController.TakeDamage(bulletDamage);
                Debug.Log("[DamageZone] �÷��̾ " + bulletDamage + " �������� �Ծ����ϴ�.");
            }
            Destroy(other.gameObject);
        }
    }
}

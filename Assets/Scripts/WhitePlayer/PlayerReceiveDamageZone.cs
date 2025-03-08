using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class WhitePlayerReceiveDamageZone : MonoBehaviour
{
    [Header("�Ѿ� ������ ����")]
    public int bulletDamage = 10;

    private WhitePlayerController whitePlayerController;

    private void Awake()
    {
        // SphereCollider�� �ڵ����� Trigger�� ����
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // ������ �ִ� WhitePlayerController�� ã��
        whitePlayerController = GetComponentInParent<WhitePlayerController>();
        if (whitePlayerController == null)
        {
            Debug.LogWarning("[WhitePlayerReceiveDamageZone] ������ WhitePlayerController�� �����ϴ�!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[WhitePlayerReceiveDamageZone] �浹�� ������Ʈ: " + other.gameObject.name);
        // "Bullet" �±װ� �ִ� �Ѿ��� �浹�ϸ� ������ ����
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("[WhitePlayerReceiveDamageZone] �Ѿ� �浹 ������.");
            if (whitePlayerController != null)
            {
                whitePlayerController.TakeDamage(bulletDamage);
                Debug.Log("[WhitePlayerReceiveDamageZone] �÷��̾ " + bulletDamage + " �������� �Ծ����ϴ�.");
            }
            Destroy(other.gameObject);
        }
    }
}

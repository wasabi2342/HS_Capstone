using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PlayerDamageZone : MonoBehaviour
{
    [Header("�� ������ ���� �Ѿ˿��� �޴� ������")]
    public int bulletDamage = 10;

    
    public PlayerController playerController;

    private void Awake()
    {
        
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null) col.isTrigger = true;
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

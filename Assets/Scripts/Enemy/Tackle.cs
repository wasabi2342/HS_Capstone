using UnityEngine;

public class Tackle : MonoBehaviour
{
    public EnemyStatus status;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().TakeDamage(1);
            Debug.Log("[�浹] �÷��̾�� �浹�߽��ϴ�.");
        }
    }
}

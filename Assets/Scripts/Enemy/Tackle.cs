using UnityEngine;

public class Tackle : MonoBehaviour
{
    public EnemyStatus status;

    private void OntTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        { 
            Debug.Log("[�浹] �÷��̾�� �浹�߽��ϴ�.");
        }
    }
}

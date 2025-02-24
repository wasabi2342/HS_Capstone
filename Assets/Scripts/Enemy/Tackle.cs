using UnityEngine;

public class Tackle : MonoBehaviour
{
    public EnemyStatus status;

    private void OntTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        { 
            Debug.Log("[충돌] 플레이어와 충돌했습니다.");
        }
    }
}

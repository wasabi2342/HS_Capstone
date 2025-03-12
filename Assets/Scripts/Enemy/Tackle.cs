using UnityEngine;

public class Tackle : MonoBehaviour
{
    public EnemyStatus status;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().TakeDamage(1);
            Debug.Log("[충돌] 플레이어와 충돌했습니다.");
        }
    }
}

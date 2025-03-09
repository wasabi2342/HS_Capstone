using UnityEngine;

public class ChaseRange : MonoBehaviour
{
    private MeleeMovement meleeMovement;

    void Start()
    {
        meleeMovement = GetComponentInParent<MeleeMovement>();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            meleeMovement.StopChasingAndReturnToPatrol(); //플레이어가 ChaseRange 벗어나면 추격 해제
        }
    }
}

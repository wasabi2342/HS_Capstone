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
            meleeMovement.StopChasingAndReturnToPatrol(); //�÷��̾ ChaseRange ����� �߰� ����
        }
    }
}

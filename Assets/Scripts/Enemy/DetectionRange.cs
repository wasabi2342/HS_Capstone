using UnityEngine;

public class DetectionRange : MonoBehaviour
{
    private EnemyStateController enemyController;

    void Start()
    {
        enemyController = GetComponentInParent<EnemyStateController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemyController.DetectPlayer(); // �÷��̾� ����
        }
    }
}

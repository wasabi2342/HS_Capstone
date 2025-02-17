using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject enemyPrefab; // ������ ���� ������
    public float spawnInterval = 5f; // ���� ���� ����
    public int maxEnemies = 5; // ���ÿ� ������ �ִ� ���� ��
    private int currentEnemyCount = 0;

    void Start()
    {
        InvokeRepeating("SpawnEnemy", 0f, spawnInterval); // ���� �������� ���� ����
    }

    void SpawnEnemy()
    {
        if (currentEnemyCount >= maxEnemies) return;

        GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        enemy.GetComponent<EnemyStateController>().SetSpawnPoint(transform.position); // Spawn Point ����
        currentEnemyCount++;
    }

    public void OnEnemyDestroyed()
    {
        currentEnemyCount--; // ���Ͱ� ������ ī��Ʈ ����
    }
}

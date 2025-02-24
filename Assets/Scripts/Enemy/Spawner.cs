using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval;
    public int maxEnemies;
    private int currentEnemyCount = 0;

    void Start()
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Enemy"), true);
        InvokeRepeating(nameof(SpawnEnemy), 0f, spawnInterval);
    }

    void SpawnEnemy()
    {
        if (currentEnemyCount >= maxEnemies) return;

        GameObject enemy = Instantiate(enemyPrefab, transform.position, enemyPrefab.transform.rotation);
        enemy.GetComponent<EnemyStateController>().SetSpawnPoint(transform.position);
        currentEnemyCount++;
    }

    public void OnEnemyDestroyed()
    {
        currentEnemyCount--;
    }
}

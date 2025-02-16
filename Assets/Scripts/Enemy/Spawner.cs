using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject enemyPrefab; // 스폰할 몬스터 프리팹
    public float spawnInterval = 5f; // 몬스터 스폰 간격
    public int maxEnemies = 5; // 동시에 존재할 최대 몬스터 수
    private int currentEnemyCount = 0;

    void Start()
    {
        InvokeRepeating("SpawnEnemy", 0f, spawnInterval); // 일정 간격으로 몬스터 생성
    }

    void SpawnEnemy()
    {
        if (currentEnemyCount >= maxEnemies) return;

        GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        enemy.GetComponent<EnemyStateController>().SetSpawnPoint(transform.position); // Spawn Point 설정
        currentEnemyCount++;
    }

    public void OnEnemyDestroyed()
    {
        currentEnemyCount--; // 몬스터가 죽으면 카운트 감소
    }
}

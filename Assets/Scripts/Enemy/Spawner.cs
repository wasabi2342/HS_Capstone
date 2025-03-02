using Photon.Pun;
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
        if (PhotonNetwork.InRoom)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;
            else
            {
                if (currentEnemyCount >= maxEnemies) return;
                GameObject enemy = PhotonNetwork.Instantiate(enemyPrefab.name, transform.position, Quaternion.Euler(45f, 0f, 0f));
                enemy.GetComponent<EnemyStateController>().SetSpawnPoint(transform.position);
                currentEnemyCount++;
            }
        }
        else
        {
            if (currentEnemyCount >= maxEnemies) return;
            GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.Euler(45f, 0f, 0f));
            enemy.GetComponent<EnemyStateController>().SetSpawnPoint(transform.position);
            currentEnemyCount++;
        }
        //if (currentEnemyCount >= maxEnemies) return;

        //GameObject enemy = Instantiate(enemyPrefab, transform.position, enemyPrefab.transform.rotation);
    }

    public void OnEnemyDestroyed()
    {
        currentEnemyCount--;
    }
}

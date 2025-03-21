/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterSpawner : MonoBehaviourPun
{
    public GameObject monsterPrefab;
    public SpawnArea spawnArea;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnMonsters(3); // 예제: 3마리 몬스터 스폰
        }
    }

    public void SpawnMonsters(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPoint = spawnArea.GetRandomPointInsideArea();
            PhotonNetwork.Instantiate(monsterPrefab.name, spawnPoint, Quaternion.identity);
        }
    }
}
*/

using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject[] monsterPrefabs;
    public SpawnArea spawnArea;
    public int spawnCount = 3;

    private void Start()
    {
        SpawnMonsters();
    }

    public void SpawnMonsters()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            foreach (var prefab in monsterPrefabs)
            {
                Vector3 spawnPoint = spawnArea.GetRandomPointInsideArea();
                GameObject monster = Instantiate(prefab, spawnPoint, Quaternion.identity);

                monster.transform.SetParent(spawnArea.transform);
            }
        }
    }

}


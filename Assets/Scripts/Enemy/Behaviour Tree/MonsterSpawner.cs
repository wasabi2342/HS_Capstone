/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterSpawner : MonoBehaviourPun
{
    public GameObject[] monsterPrefabs;  // 네트워크로 생성할 몬스터 프리팹들
    public SpawnArea spawnArea;
    public int spawnCount = 3;           // 스폰 반복 횟수

    private void Start()
    {
        // 마스터 클라이언트(방장)만 몬스터를 스폰하도록 예시
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnMonsters();
        }
    }

    public void SpawnMonsters()
    {
        // spawnCount만큼 반복
        for (int i = 0; i < spawnCount; i++)
        {
            // monsterPrefabs에 등록된 각 몬스터를 순회하면서 생성
            foreach (var prefab in monsterPrefabs)
            {
                // 랜덤 스폰 지점
                Vector3 spawnPoint = spawnArea.GetRandomPointInsideArea();

                // 네트워크 객체로 생성
                // (prefab.name이 PhotonNetwork의 리소스 풀에서 인식 가능한 이름이어야 함)
                GameObject monster = PhotonNetwork.Instantiate(prefab.name, spawnPoint, Quaternion.identity);

                // MonsterSpawner 오브젝트 하위로 이동시켜 하이어라키에서 구조를 정리
                monster.transform.SetParent(this.transform);
            }
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


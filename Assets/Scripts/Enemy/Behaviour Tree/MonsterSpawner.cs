using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterSpawner : MonoBehaviourPun
{
    public GameObject[] monsterPrefabs;  // 네트워크로 생성할 몬스터 프리팹들
    public SpawnArea spawnArea;          // 스폰 영역 (미리 배치된 SpawnArea 내의 MonsterSpawner에 연결)
    public int spawnCount;           // 스폰 반복 횟수

    private void Awake()
    {

        if (spawnArea == null)
        {
            GameObject spawnAreaInstance = PhotonNetwork.Instantiate("SpawnArea", Vector3.zero, Quaternion.identity);
            spawnArea = spawnAreaInstance.GetComponent<SpawnArea>();

            // MonsterSpawner(이 스크립트가 붙은 객체)를 생성된 SpawnArea의 자식으로 재배치
            transform.SetParent(spawnAreaInstance.transform);
        }
    }

    private void Start()
    {
        // 마스터 클라이언트만 몬스터를 스폰
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnMonsters();
        }
    }

    public void SpawnMonsters()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            foreach (var prefab in monsterPrefabs)
            {
                // SpawnArea 내에서 랜덤한 스폰 위치 결정
                Vector3 spawnPoint = spawnArea.GetRandomPointInsideArea();

                // 네트워크 객체로 몬스터 생성 (Resources 폴더 내에 해당 프리팹이 있어야 함)
                GameObject monster = PhotonNetwork.Instantiate(prefab.name, spawnPoint, Quaternion.identity);

                // Master Client에서는 MonsterSpawner의 자식으로 부모 설정
                monster.transform.SetParent(this.transform);

                // 다른 클라이언트에도 부모 설정을 동기화하기 위한 RPC 호출
                int monsterViewID = monster.GetComponent<PhotonView>().ViewID;
                int spawnerViewID = this.photonView.ViewID;
                photonView.RPC("RPC_SetParent", RpcTarget.OthersBuffered, monsterViewID, spawnerViewID);
            }
        }
    }

    [PunRPC]
    void RPC_SetParent(int monsterViewID, int spawnerViewID)
    {
        PhotonView monsterPV = PhotonView.Find(monsterViewID);
        PhotonView spawnerPV = PhotonView.Find(spawnerViewID);
        if (monsterPV != null && spawnerPV != null)
        {
            // 다른 클라이언트에서도 MonsterSpawner 하위로 부모 설정
            monsterPV.gameObject.transform.SetParent(spawnerPV.gameObject.transform);
        }
    }
}




/*
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
*/

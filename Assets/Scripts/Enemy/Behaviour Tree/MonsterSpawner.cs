
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
                Vector3 spawnPoint = spawnArea.GetRandomPointInsideArea();

                // 네트워크 객체로 생성
                // (prefab.name은 PhotonNetwork의 리소스 풀에서 인식 가능한 이름이어야 함)
                GameObject monster = PhotonNetwork.Instantiate(prefab.name, spawnPoint, Quaternion.identity);
                monster.transform.SetParent(this.transform);

                // 다른 클라이언트에도 부모 설정을 동기화하기 위한 RPC 호출
                int monsterViewID = monster.GetComponent<PhotonView>().ViewID;
                int parentViewID = this.photonView.ViewID;
                photonView.RPC("RPC_SetParent", RpcTarget.OthersBuffered, monsterViewID, parentViewID);
            }
        }
    }

    [PunRPC]
    void RPC_SetParent(int monsterViewID, int parentViewID)
    {
        PhotonView monsterPV = PhotonView.Find(monsterViewID);
        PhotonView parentPV = PhotonView.Find(parentViewID);
        if (monsterPV != null && parentPV != null)
        {
            // 다른 클라이언트에서도 MonsterSpawner 하위로 부모 설정
            monsterPV.gameObject.transform.SetParent(parentPV.gameObject.transform);
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

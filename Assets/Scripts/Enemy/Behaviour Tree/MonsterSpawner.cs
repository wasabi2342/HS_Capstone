using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterSpawner : MonoBehaviourPun
{
    public GameObject[] monsterPrefabs;  // 네트워크로 생성할 몬스터 프리팹들
    public SpawnArea spawnArea;          // 자동 할당: MonsterSpawner가 SpawnArea 하위에 있으면 해당 SpawnArea 사용
    public int spawnCount = 3;           // 스폰 반복 횟수

    private void Awake()
    {
        // Inspector에 spawnArea가 할당되어 있지 않으면 부모에서 찾아 자동 할당
        if (spawnArea == null)
        {
            spawnArea = GetComponentInParent<SpawnArea>();
            if (spawnArea == null)
            {
                Debug.LogError("MonsterSpawner: 부모 SpawnArea를 찾을 수 없습니다.");
            }
            else
            {
                Debug.Log("[INIT] spawnArea 자동 할당됨: " + spawnArea);
            }
        }
    }

    private void Start()
    {
        // 마스터 클라이언트(방장)만 몬스터를 스폰하도록 실행
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
            // monsterPrefabs에 등록된 각 프리팹을 순회하며 생성
            foreach (var prefab in monsterPrefabs)
            {
                // 랜덤 스폰 지점 계산
                Vector3 spawnPoint = spawnArea.GetRandomPointInsideArea();
                // 네트워크 객체로 생성 (prefab.name은 Resources 폴더 내 프리팹 이름과 일치해야 함)
                GameObject monster = PhotonNetwork.Instantiate(prefab.name, spawnPoint, Quaternion.identity);
                monster.transform.SetParent(this.transform);

                // 부모 설정은 네트워크 상에서 자동 전파되지 않으므로, RPC를 이용해 동기화
                int monsterViewID = monster.GetComponent<PhotonView>().ViewID;
                int parentViewID = this.photonView.ViewID;
                photonView.RPC("RPC_SetParent", RpcTarget.OthersBuffered, monsterViewID, parentViewID);
            }
        }
    }

    // RPC를 통해 다른 클라이언트에서 부모-자식 관계를 설정하는 함수
    [PunRPC]
    void RPC_SetParent(int monsterViewID, int parentViewID)
    {
        PhotonView monsterPV = PhotonView.Find(monsterViewID);
        PhotonView parentPV = PhotonView.Find(parentViewID);
        if (monsterPV != null && parentPV != null)
        {
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterSpawner : MonoBehaviourPun
{
    [Header("몬스터 프리팹 (Resources 폴더 내에 있어야 함)")]
    public GameObject[] monsterPrefabs;
    [Header("SpawnArea 참조 (미지정 시 부모에서 자동 할당)")]
    public SpawnArea spawnArea;
    [Header("스폰 설정")]
    public int spawnCount = 3;  // 스폰 반복 횟수

    private void Awake()
    {
        // 만약 Inspector에 할당되어 있지 않다면, MonsterSpawner의 부모에서 SpawnArea 컴포넌트를 자동으로 할당
        if (spawnArea == null)
        {
            spawnArea = GetComponentInParent<SpawnArea>();
            if (spawnArea == null)
            {
                Debug.LogError("MonsterSpawner: SpawnArea를 찾을 수 없습니다.");
            }
        }
    }

    private void Start()
    {
        // 마스터 클라이언트에서만 몬스터 스폰
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
            foreach (var prefab in monsterPrefabs)
            {
                // SpawnArea 내에서 랜덤한 스폰 위치 결정
                Vector3 spawnPoint = spawnArea.GetRandomPointInsideArea();

                // 네트워크 객체로 몬스터 생성 (prefab.name은 Resources 폴더 내 프리팹 이름과 일치해야 함)
                GameObject monster = PhotonNetwork.Instantiate(prefab.name, spawnPoint, Quaternion.identity);

                // Master Client에서는 SpawnArea의 자식으로 부모 설정
                monster.transform.SetParent(spawnArea.transform);

                // 다른 클라이언트에서도 동일한 부모 관계를 갖도록 RPC 호출
                int monsterViewID = monster.GetComponent<PhotonView>().ViewID;
                int parentViewID = spawnArea.photonView.ViewID;
                photonView.RPC("RPC_SetParent", RpcTarget.OthersBuffered, monsterViewID, parentViewID);
            }
        }
    }

    // RPC: 다른 클라이언트에서 몬스터의 부모를 SpawnArea로 재설정
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

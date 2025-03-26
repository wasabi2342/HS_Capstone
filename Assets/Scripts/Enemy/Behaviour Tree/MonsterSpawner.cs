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

    private void Awake()
    {
        if (spawnArea == null)
        {
            spawnArea = GetComponentInParent<SpawnArea>();
            if (spawnArea == null)
            {
                Debug.LogError("MonsterSpawner: SpawnArea를 찾을 수 없습니다.");
            }
        }
    }

    // 외부에서 스폰할 몬스터 수를 전달받아 몬스터를 생성
    public void SpawnMonsters(int spawnCount)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        for (int i = 0; i < spawnCount; i++)
        {
            foreach (var prefab in monsterPrefabs)
            {
                // SpawnArea 내에서 랜덤한 스폰 위치 결정
                Vector3 spawnPoint = spawnArea.GetRandomPointInsideArea();

                // 네트워크 객체로 몬스터 생성 (prefab.name은 Resources 폴더 내 프리팹 이름과 일치해야 함)
                GameObject monster = PhotonNetwork.Instantiate(prefab.name, spawnPoint, Quaternion.identity);

                // 생성된 몬스터를 SpawnArea의 자식으로 설정
                monster.transform.SetParent(spawnArea.transform);

                // 다른 클라이언트에서도 동일한 부모 관계를 갖도록 RPC 호출
                int monsterViewID = monster.GetComponent<PhotonView>().ViewID;
                int parentViewID = spawnArea.photonView.ViewID;
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
            monsterPV.gameObject.transform.SetParent(parentPV.gameObject.transform);
        }
    }
}

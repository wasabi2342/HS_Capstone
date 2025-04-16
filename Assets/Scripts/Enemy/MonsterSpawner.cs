using UnityEngine;
using Photon.Pun;

public class MonsterSpawner : MonoBehaviourPun
{
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

    // [변경] MonsterSpawnInfo 배열을 받아서 각 몬스터를 개별적으로 스폰
    public void SpawnMonsters(MonsterSpawnInfo[] monsterSpawnInfos)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        foreach (var info in monsterSpawnInfos)
        {
            // info.count만큼 반복해서 스폰
            for (int i = 0; i < info.count; i++)
            {
                Vector3 spawnPoint = spawnArea.GetRandomPointInsideArea();

                // Resources 폴더 내 prefabName으로 네트워크 Instantiate
                GameObject monster = PhotonNetwork.Instantiate(info.monsterPrefabName, spawnPoint, Quaternion.identity);

                // SpawnArea를 부모로 설정
                monster.transform.SetParent(spawnArea.transform);

                // 다른 클라이언트에서도 동일하게 부모 설정
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

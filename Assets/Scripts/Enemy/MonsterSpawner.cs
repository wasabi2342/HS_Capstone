using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

/// <summary>
/// SpawnArea 안에서 몬스터 프리팹을 네트워크 Instantiate 하고
/// ─ 부모(SpawnArea) 설정 (RPC_SetParent)  
/// ─ SpawnArea 참조를 EnemyAI 에 확정 (RPC_LinkSpawnArea)  
/// ─ NavMesh 밖에 떨어졌으면 Warp 로 보정
/// 마스터 클라이언트에서만 실행된다.
/// </summary>
public class MonsterSpawner : MonoBehaviourPun
{
    public SpawnArea spawnArea;           // 인스펙터에서 연결 or 부모 자동 검색

    void Awake()
    {
        if (spawnArea == null)
        {
            spawnArea = GetComponentInParent<SpawnArea>();
            if (spawnArea == null)
                Debug.LogError("[MonsterSpawner] SpawnArea not found!");
        }
    }

    public void SpawnMonsters(MonsterSpawnInfo[] infos)
    {
        if (!PhotonNetwork.IsMasterClient || spawnArea == null) return;

        foreach (var info in infos)
        {
            for (int i = 0; i < info.count; i++)
            {
                /* 1. 위치 선정 */
                Vector3 pos = spawnArea.GetRandomPointInsideArea();

                /* 2. 네트워크 Instantiate */
                GameObject monster =
                    PhotonNetwork.Instantiate(info.monsterPrefabName, pos, Quaternion.identity);

                /* 3. 부모 지정 (로컬) */
                monster.transform.SetParent(spawnArea.transform);

                /* 4. 다른 클라이언트에도 부모 지정 */
                int mViewID = monster.GetComponent<PhotonView>().ViewID;
                int aViewID = spawnArea.photonView.ViewID;
                photonView.RPC(nameof(RPC_SetParent), RpcTarget.OthersBuffered, mViewID, aViewID);
                photonView.RPC(nameof(RPC_LinkSpawnArea), RpcTarget.AllBuffered, mViewID, aViewID);

                /* 5. NavMesh 보정 (마스터 자신에게만) */
                if (monster.TryGetComponent(out NavMeshAgent ag) && !ag.isOnNavMesh)
                {
                    if (NavMesh.SamplePosition(ag.transform.position, out var hit, 3f, NavMesh.AllAreas))
                        ag.Warp(hit.position);
                }
            }
        }
    }

    /* ───────── RPC 1 : transform 부모 동기화 ───────── */
    [PunRPC]
    void RPC_SetParent(int monsterVid, int areaVid)
    {
        PhotonView mPV = PhotonView.Find(monsterVid);
        PhotonView aPV = PhotonView.Find(areaVid);
        if (mPV != null && aPV != null)
            mPV.transform.SetParent(aPV.transform);
    }

    /* ───────── RPC 2 : EnemyAI 에 SpawnArea 참조 확정 ───────── */
    [PunRPC]
    void RPC_LinkSpawnArea(int monsterVid, int areaVid)
    {
        PhotonView mPV = PhotonView.Find(monsterVid);
        PhotonView aPV = PhotonView.Find(areaVid);

        if (mPV != null && aPV != null &&
            mPV.TryGetComponent(out EnemyAI ai) &&
            aPV.TryGetComponent(out SpawnArea sa))
        {
            ai.SetSpawnArea(sa);   // EnemyAI 내부에서 PickWanderPoint() 호출
        }
    }
}

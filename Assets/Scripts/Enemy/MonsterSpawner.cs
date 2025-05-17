// =========================================================== MonsterSpawner.cs
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

/// <summary>
/// • SpawnArea 안에서 MonsterSpawnInfo 배열대로 몬스터를 Instantiate  
/// • 부모‑자식 관계 · SpawnArea 참조 · NavMesh 워프를 전 클라이언트에 맞춰 동기화  
/// • **StageManager**가 wave 마다 SpawnMonsters() 를 호출한다.
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class MonsterSpawner : MonoBehaviourPun
{
    [SerializeField] private SpawnArea spawnArea;   // 없어도 부모에서 자동 탐색


    void Awake()
    {
        if (!spawnArea)
            spawnArea = GetComponentInParent<SpawnArea>();

        if (!spawnArea)
            Debug.LogError("[MonsterSpawner] SpawnArea not found!", this);
    }

    public void SpawnMonsters(MonsterSpawnInfo[] infos)
    {
        if (!PhotonNetwork.IsMasterClient || spawnArea == null) return;
        int pCnt = PhotonNetwork.CurrentRoom.PlayerCount;
        float countMul = DifficultyManager.Instance.GetCountMultiplier(pCnt);
        foreach (var info in infos)
        {
            int spawnCount = Mathf.CeilToInt(info.count * countMul);
            for (int i = 0; i < spawnCount; i++)
            {
                /* 1️⃣ 위치 선정 */
                Vector3 pos = spawnArea.GetRandomPointInsideArea();

                /* 2️⃣ 네트워크 Instantiate (마스터가 Owner) */
                GameObject m = PhotonNetwork.Instantiate(info.monsterPrefabName, pos, Quaternion.identity);

                /* 3️⃣ 로컬 부모 지정 */
                m.transform.SetParent(spawnArea.transform, true);

                /* 4️⃣ 다른 클라이언트에도 부모 & SpawnArea 참조 적용 */
                int mVid = m.GetComponent<PhotonView>().ViewID;
                int aVid = spawnArea.GetComponent<PhotonView>().ViewID;
                photonView.RPC(nameof(RPC_SetParent), RpcTarget.OthersBuffered, mVid, aVid);
                photonView.RPC(nameof(RPC_LinkSpawnArea), RpcTarget.AllBuffered, mVid, aVid);

                /* 5️⃣ NavMesh 워프(마스터만) */
                if (m.TryGetComponent(out NavMeshAgent ag) && !ag.isOnNavMesh)
                    if (NavMesh.SamplePosition(pos, out var hit, 3f, NavMesh.AllAreas))
                        ag.Warp(hit.position);
            }
        }
    }

    /* ---------- RPC : transform 부모 동기화 ---------- */
    [PunRPC]
    void RPC_SetParent(int monsterVid, int areaVid)
    {
        var mPV = PhotonView.Find(monsterVid);
        var aPV = PhotonView.Find(areaVid);
        if (mPV && aPV) mPV.transform.SetParent(aPV.transform, true);
    }

    /* ---------- RPC : EnemyAI/FSM 에 SpawnArea 주입 ---------- */
    [PunRPC]
    void RPC_LinkSpawnArea(int monsterVid, int areaVid)
    {
        var mPV = PhotonView.Find(monsterVid);
        var aPV = PhotonView.Find(areaVid);
        if (mPV == null || aPV == null) return;
        if (!aPV.TryGetComponent(out SpawnArea sa)) return;

        if (mPV.TryGetComponent(out EnemyAI ai)) ai.SetSpawnArea(sa);
        if (mPV.TryGetComponent(out EnemyFSM fsm)) fsm.SetSpawnArea(sa);
    }
}

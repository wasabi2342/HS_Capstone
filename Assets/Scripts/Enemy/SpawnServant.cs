using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

public class SpawnServant : MonoBehaviourPun
{
    [Header("Servant Settings")]
    public string servantPrefabName = "Servant";   // Resources/Servant.prefab
    public float spawnDelay = 10f;                 // 몇 초 후에 소환할지
    public Vector3 spawnOffset = new Vector3(-1.5f, 0f, 0f); // 플레이어 기준 상대 위치

    private void Start()
    {
        if (photonView.IsMine)
        {
            Invoke(nameof(Spawn), spawnDelay);
        }
    }

    private void Spawn()
    {
        Vector3 spawnPos = transform.position + spawnOffset;

        // 스폰 지점을 NavMesh 위로 보정
        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            spawnPos = hit.position;

        GameObject servant = PhotonNetwork.Instantiate(servantPrefabName, spawnPos, Quaternion.identity);

        // NavMeshAgent가 아직 Off‑Mesh 에 있을 경우 강제로 워프
        NavMeshAgent agent = servant.GetComponent<NavMeshAgent>();
        if (agent != null && !agent.isOnNavMesh)
            agent.Warp(spawnPos);

        // masterPlayer 지정
        if (servant.TryGetComponent(out ServantAI servantAI))
            servantAI.masterPlayer = transform;

        servant.transform.SetParent(transform);   // (선택) 부모 설정
    }

}

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

        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            spawnPos = hit.position;

        GameObject servant = PhotonNetwork.Instantiate(servantPrefabName, spawnPos, Quaternion.identity);

        // masterPlayer 동기화
        int masterID = GetComponent<PhotonView>().ViewID;
        servant.GetComponent<PhotonView>()
               .RPC(nameof(ServantAI.RPC_SetMaster), RpcTarget.AllBuffered, masterID);

        /*  부모 설정 제거
        servant.transform.SetParent(this.transform);
        */
    }

}

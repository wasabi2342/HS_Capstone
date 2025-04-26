using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

/// <summary>
/// 몬스터 배회·스폰 영역. 반경 값은 PUN RPC 로 모든 클라이언트에 동기화된다.
/// </summary>
public class SpawnArea : MonoBehaviourPun
{
    [SerializeField] private float spawnRadius = 8f;   // 기본 반경

    /* ───── public API ───── */
    public void SetRadius(float r)
    {
        if (r <= 0.01f)
        {
            Debug.LogWarning(
                $"[SpawnArea] radius({r}) ≤ 0  → 기본값 8로 대체 : {name}");
            r = 8f;
        }

        spawnRadius = r;

        // 룸 안일 때만 RPC 전송(오프라인 모드에서는 불필요)
        if (PhotonNetwork.InRoom)
            photonView.RPC(nameof(RPC_SetRadius), RpcTarget.OthersBuffered, r);
    }

    [PunRPC] public void RPC_SetRadius(float r) => spawnRadius = r;
    public float GetRadius() => spawnRadius;

    public Vector3 GetRandomPointInsideArea()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 p = Random.insideUnitSphere * spawnRadius + transform.position;
            p.y = transform.position.y;

            if (NavMesh.SamplePosition(p, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                return hit.position;
        }
        return transform.position;   // 실패 시 원점
    }

    /* ───── Gizmos ───── */
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}

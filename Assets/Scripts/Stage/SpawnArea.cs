// =========================================================== SpawnArea.cs
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

/// <summary>
/// ‘몬스터 배회 & 스폰’ 구역. 반경 값은 PUN RPC로 동기화된다.
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class SpawnArea : MonoBehaviourPun
{
    [SerializeField] private float radius = 8f;

    /* ------------ public API ------------ */
    public void SetRadius(float r)
    {
        r = Mathf.Max(0.1f, r);
        radius = r;

        if (PhotonNetwork.InRoom)
            photonView.RPC(nameof(RPC_SetRadius), RpcTarget.OthersBuffered, r);
    }
    public float GetRadius() => radius;

    public Vector3 GetRandomPointInsideArea()
    {
        for (int i = 0; i < 10; i++)
        {
            var p = Random.insideUnitSphere * radius + transform.position;
            p.y = transform.position.y;

            if (NavMesh.SamplePosition(p, out var hit, 3f, NavMesh.AllAreas))
                return hit.position;
        }
        return transform.position; // 실패 시 중심 반환
    }

    /* ------------ RPC ------------ */
    [PunRPC] void RPC_SetRadius(float r) => radius = r;

    /* ------------ Gizmos ------------ */
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0.9f, 0, 0.25f);
        Gizmos.DrawSphere(transform.position, radius);
    }
#endif
}

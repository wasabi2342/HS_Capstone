using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Photon.Pun;

public class SpawnArea : MonoBehaviourPun
{
    private float spawnRadius; // 배회할 수 있는 반경

    public void SetRadius(float radius)
    {
        spawnRadius = radius;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

    public Vector3 GetRandomPointInsideArea()
    {
        for (int i = 0; i < 10; i++) // 최대 10회 시도
        {
            Vector3 randomPos = Random.insideUnitSphere * spawnRadius;
            randomPos += transform.position;
            randomPos.y = transform.position.y;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 0f, NavMesh.AllAreas))
                return hit.position;
        }
        return transform.position; // 실패 시 원점 반환
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}

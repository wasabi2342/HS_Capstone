using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class SpawnArea : MonoBehaviour
{
    public float radius = 10f; // 배회할 수 있는 반경

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public Vector3 GetRandomPointInsideArea()
    {
        for (int i = 0; i < 10; i++) // 최대 10회 시도
        {
            Vector3 randomPos = Random.insideUnitSphere * radius;
            randomPos += transform.position;
            randomPos.y = transform.position.y;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                return hit.position;
        }
        return transform.position; // 실패 시 원점 반환
    }

}

using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("몬스터 프리팹")]
    [SerializeField]
    private GameObject enemyPrefab;

    [Header("스폰할 몬스터 수")]
    [SerializeField]
    private int enemyCount = 10;

    [Header("몬스터가 추적할 대상(플레이어 등)")]
    [SerializeField]
    private Transform target;

    [Header("스폰할 Plane 오브젝트(바닥)")]
    [SerializeField]
    private GameObject planeObject; // MeshRenderer나 Collider가 있는 Plane

    [System.Serializable]
    private struct WayPointData
    {
        public GameObject[] wayPoints;  // 순찰 경로로 쓸 웨이포인트들
    }

    [Header("순찰 경로 세트 (여러 개 중 랜덤 선택)")]
    [SerializeField]
    private WayPointData[] wayPointData;

    private void Awake()
    {
        // Plane의 Renderer를 통해 Bounds 가져오기
        Renderer planeRenderer = planeObject.GetComponent<Renderer>();
        if (planeRenderer == null)
        {
            Debug.LogError("planeObject에 Renderer가 없습니다. Plane MeshRenderer를 확인하세요.");
            return;
        }
        Bounds planeBounds = planeRenderer.bounds;

        // enemyCount만큼 반복하여 Plane 영역 내 임의 위치에 몬스터 스폰
        for (int i = 0; i < enemyCount; i++)
        {
            // 1) Plane 영역에서 랜덤 위치 뽑기 (x, z)
            float randX = Random.Range(planeBounds.min.x, planeBounds.max.x);
            float randZ = Random.Range(planeBounds.min.z, planeBounds.max.z);
            Vector3 spawnPos = new Vector3(randX, planeBounds.min.y, randZ);

            // 2) WayPointData 중 하나를 임의 선택
            int wayIndex = Random.Range(0, wayPointData.Length);
            GameObject[] selectedWayPoints = wayPointData[wayIndex].wayPoints;

            // 3) 몬스터 생성
            GameObject clone = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, transform);

            // 4) EnemyStateController에 순찰 경로와 타겟 할당
            EnemyStateController controller = clone.GetComponent<EnemyStateController>();
            if (controller != null)
            {
                controller.Setup(target, selectedWayPoints);
            }
        }
    }
}

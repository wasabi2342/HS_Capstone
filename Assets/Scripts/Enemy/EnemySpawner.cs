using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("���� ������")]
    [SerializeField]
    private GameObject enemyPrefab;

    [Header("������ ���� ��")]
    [SerializeField]
    private int enemyCount = 10;

    [Header("���Ͱ� ������ ���(�÷��̾� ��)")]
    [SerializeField]
    private Transform target;

    [Header("������ Plane ������Ʈ(�ٴ�)")]
    [SerializeField]
    private GameObject planeObject; // MeshRenderer�� Collider�� �ִ� Plane

    [System.Serializable]
    private struct WayPointData
    {
        public GameObject[] wayPoints;  // ���� ��η� �� ��������Ʈ��
    }

    [Header("���� ��� ��Ʈ (���� �� �� ���� ����)")]
    [SerializeField]
    private WayPointData[] wayPointData;

    private void Awake()
    {
        // Plane�� Renderer�� ���� Bounds ��������
        Renderer planeRenderer = planeObject.GetComponent<Renderer>();
        if (planeRenderer == null)
        {
            Debug.LogError("planeObject�� Renderer�� �����ϴ�. Plane MeshRenderer�� Ȯ���ϼ���.");
            return;
        }
        Bounds planeBounds = planeRenderer.bounds;

        // enemyCount��ŭ �ݺ��Ͽ� Plane ���� �� ���� ��ġ�� ���� ����
        for (int i = 0; i < enemyCount; i++)
        {
            // 1) Plane �������� ���� ��ġ �̱� (x, z)
            float randX = Random.Range(planeBounds.min.x, planeBounds.max.x);
            float randZ = Random.Range(planeBounds.min.z, planeBounds.max.z);
            Vector3 spawnPos = new Vector3(randX, planeBounds.min.y, randZ);

            // 2) WayPointData �� �ϳ��� ���� ����
            int wayIndex = Random.Range(0, wayPointData.Length);
            GameObject[] selectedWayPoints = wayPointData[wayIndex].wayPoints;

            // 3) ���� ����
            GameObject clone = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, transform);

            // 4) EnemyStateController�� ���� ��ο� Ÿ�� �Ҵ�
            EnemyStateController controller = clone.GetComponent<EnemyStateController>();
            if (controller != null)
            {
                controller.Setup(target, selectedWayPoints);
            }
        }
    }
}

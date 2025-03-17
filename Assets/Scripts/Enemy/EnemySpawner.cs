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
    private Transform player;

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

        // 80% ���� ������ ���� ����
        Vector3 minBound = planeBounds.min;
        Vector3 maxBound = planeBounds.max;
        Vector3 size = planeBounds.size;

        float minX = minBound.x + size.x * 0.1f;
        float maxX = maxBound.x - size.x * 0.1f;
        float minZ = minBound.z + size.z * 0.1f;
        float maxZ = maxBound.z - size.z * 0.1f;

        // enemyCount��ŭ �ݺ��Ͽ� Plane ���� �� ���� ����
        for (int i = 0; i < enemyCount; i++)
        {
            // 1) �߾� 80% �������� ���� ��ġ ���� (x, z)
            float randX = Random.Range(minX, maxX);
            float randZ = Random.Range(minZ, maxZ);
            Vector3 spawnPos = new Vector3(randX, planeBounds.min.y, randZ);

            // 2) WayPointData �� �ϳ��� ���� ����
            int wayIndex = Random.Range(0, wayPointData.Length);
            GameObject[] selectedWayPoints = wayPointData[wayIndex].wayPoints;

            // 3) ���� ����
            GameObject clone = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, transform);

            // 4) EnemyStateController�� ���� ��ο� �÷��̾� �Ҵ�
            EnemyFSM fsm = clone.GetComponent<EnemyFSM>();
            if (fsm != null)
            {
                fsm.Setup(player, selectedWayPoints);
            }
        }
    }
}

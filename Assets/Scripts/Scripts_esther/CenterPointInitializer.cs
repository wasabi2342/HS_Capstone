
using UnityEngine;

public class CenterPointInitializer : MonoBehaviour
{
    public Transform playerTransform;
    public float interactionRadius = 2.5f; // 플레이어 기준 상호작용 범위
    public Transform[] centerPoints; // 8개 중심점

    void Start()
    {
        if (playerTransform == null || centerPoints == null || centerPoints.Length != 8)
        {
            Debug.LogError("플레이어 Transform 또는 CenterPoints가 올바르게 설정되지 않았습니다.");
            return;
        }

        // 8방향 각도 설정 (360도를 8개 방향으로 분할)
        float[] angles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };

        for (int i = 0; i < centerPoints.Length; i++)
        {
            float radians = angles[i] * Mathf.Deg2Rad; // 각도를 라디안으로 변환
            Vector3 offset = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)) * interactionRadius;
            centerPoints[i].position = playerTransform.position + offset;
        }
    }

    void Update()
    {
        // 플레이어가 이동하면 중심점도 따라 이동
        for (int i = 0; i < centerPoints.Length; i++)
        {
            float radians = i * 45f * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)) * interactionRadius;
            centerPoints[i].position = playerTransform.position + offset;
        }
    }
}

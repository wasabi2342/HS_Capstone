using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // 따라갈 대상 (플레이어)
    public Transform target;
    // 플레이어와 카메라 간의 오프셋 (예: 높이와 거리 조정)
    public Vector3 offset = new Vector3(0, 5, -10);
    // 부드러운 따라감 정도 (0~1 사이, 낮을수록 더 부드럽게 따라감)
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target != null)
        {
            // 플레이어 위치에 오프셋을 더한 위치를 목표 위치로 설정
            Vector3 desiredPosition = target.position + offset;
            // 현재 카메라 위치와 목표 위치를 Lerp로 보간하여 부드럽게 이동
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
            // 카메라가 항상 플레이어를 바라보도록 설정
            transform.LookAt(target);
        }
    }
}

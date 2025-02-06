using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // 따라갈 대상 (플레이어)
    public Transform target;
    // 카메라와 대상 간의 오프셋 (원하는 위치 조정)
    public Vector3 offset = new Vector3(0, 20, -10);
    // 부드러운 이동을 위한 속도
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target == null)
            return;

        // 타겟의 위치에 오프셋을 더한 목표 위치 계산
        Vector3 desiredPosition = target.position + offset;
        // 부드러운 이동(Lerp)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // 필요에 따라 카메라가 타겟을 바라보도록
        transform.LookAt(target);
    }
}

using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform target; // 플레이어 위치
    public float smoothSpeed = 0.1f;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 newPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        transform.position = Vector3.Lerp(transform.position, newPosition, smoothSpeed);
    }
}

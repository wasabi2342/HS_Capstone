using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform target; // �÷��̾� ��ġ
    public float smoothSpeed = 0.1f;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 newPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        transform.position = Vector3.Lerp(transform.position, newPosition, smoothSpeed);
    }
}

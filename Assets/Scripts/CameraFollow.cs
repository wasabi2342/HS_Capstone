using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // ���� ��� (�÷��̾�)
    public Transform target;
    // ī�޶�� ��� ���� ������ (���ϴ� ��ġ ����)
    public Vector3 offset = new Vector3(0, 20, -10);
    // �ε巯�� �̵��� ���� �ӵ�
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target == null)
            return;

        // Ÿ���� ��ġ�� �������� ���� ��ǥ ��ġ ���
        Vector3 desiredPosition = target.position + offset;
        // �ε巯�� �̵�(Lerp)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // �ʿ信 ���� ī�޶� Ÿ���� �ٶ󺸵���
        transform.LookAt(target);
    }
}

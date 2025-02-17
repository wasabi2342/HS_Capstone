using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // ���� ��� (�÷��̾�)
    public Transform target;
    // �÷��̾�� ī�޶� ���� ������ (��: ���̿� �Ÿ� ����)
    public Vector3 offset = new Vector3(0, 5, -10);
    // �ε巯�� ���� ���� (0~1 ����, �������� �� �ε巴�� ����)
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target != null)
        {
            // �÷��̾� ��ġ�� �������� ���� ��ġ�� ��ǥ ��ġ�� ����
            Vector3 desiredPosition = target.position + offset;
            // ���� ī�޶� ��ġ�� ��ǥ ��ġ�� Lerp�� �����Ͽ� �ε巴�� �̵�
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
            // ī�޶� �׻� �÷��̾ �ٶ󺸵��� ����
            transform.LookAt(target);
        }
    }
}

using UnityEngine;
using Unity.Cinemachine;

public class SideViewCamera : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform player; // �÷��̾��� Transform
    private Vector3 initialPosition;

    private void Start()
    {
        if (cinemachineCamera == null || player == null)
        {
            Debug.LogError("CinemachineVirtualCamera �Ǵ� Player�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        // �ʱ� ī�޶� ��ġ ���� (Y, Z ����)
        initialPosition = cinemachineCamera.transform.position;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        // X ���� �÷��̾ ���󰡰� Y, Z�� ����
        cinemachineCamera.transform.position = new Vector3(player.position.x, initialPosition.y, initialPosition.z);
    }
}

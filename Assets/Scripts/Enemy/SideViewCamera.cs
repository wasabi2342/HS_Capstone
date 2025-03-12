using UnityEngine;
using Unity.Cinemachine;

public class SideViewCamera : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform player; // 플레이어의 Transform
    private Vector3 initialPosition;

    private void Start()
    {
        if (cinemachineCamera == null || player == null)
        {
            Debug.LogError("CinemachineVirtualCamera 또는 Player가 할당되지 않았습니다.");
            return;
        }

        // 초기 카메라 위치 저장 (Y, Z 고정)
        initialPosition = cinemachineCamera.transform.position;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        // X 값만 플레이어를 따라가고 Y, Z는 고정
        cinemachineCamera.transform.position = new Vector3(player.position.x, initialPosition.y, initialPosition.z);
    }
}

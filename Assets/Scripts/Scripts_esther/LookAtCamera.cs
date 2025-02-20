using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class LookAtCamera : MonoBehaviourPun
{
    private Camera mainCamera;

    void Start()
    {
        if (!photonView.IsMine)
        {
            enabled = false;
            return;
        }

        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning("[LookAtCamera] 메인 카메라를 찾을 수 없습니다.");
        }
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // 카메라를 바라보도록 설정 (X축 회전은 고정)
            Vector3 lookDirection = mainCamera.transform.forward;
            lookDirection.y = 0; // Y축 회전을 고정 (필요에 따라 조절)
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}

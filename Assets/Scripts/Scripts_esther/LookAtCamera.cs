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
            Debug.LogWarning("[LookAtCamera] ���� ī�޶� ã�� �� �����ϴ�.");
        }
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // ī�޶� �ٶ󺸵��� ���� (X�� ȸ���� ����)
            Vector3 lookDirection = mainCamera.transform.forward;
            lookDirection.y = 0; // Y�� ȸ���� ���� (�ʿ信 ���� ����)
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}

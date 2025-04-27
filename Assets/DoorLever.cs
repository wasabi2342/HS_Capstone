using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoorLever : MonoBehaviourPun, IInteractable
{
    [SerializeField]
    private Canvas canvas;

    private bool canInteract = true;

    private Transform lastPlayer;

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if (canInteract)
            {
                canInteract = false;

                if (PhotonNetwork.IsConnected)
                {
                    if (photonView != null)
                    {
                        photonView.RPC("SyncCanInteract", RpcTarget.Others, false);
                    }
                }

                var panel = UIManager.Instance.OpenPopupPanel<UISpaceInterationPanel>();

                panel.Init(OnLeverInteractionComplete);

                // 🔹 위치 설정
                if (lastPlayer != null && panel != null)
                {
                    Vector3 dir = lastPlayer.position - transform.position;
                    Vector3 uiWorldPos;

                    if (dir.x < 0)
                        uiWorldPos = transform.position + new Vector3(2f, 1f, 0f);
                    else
                        uiWorldPos = transform.position + new Vector3(-2f, 1f, 0f);
                    panel.SetPanelPosition(uiWorldPos);
                }
            }
            else
            {
                UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText("상호작용이 불가능합니다.");
            }
        }
    }

    public void OnLeverInteractionComplete(bool success)
    {
        if (success)
        {
            if (PhotonNetwork.IsConnected)
            {
                if (photonView != null)
                {
                    photonView.RPC("SuccessLeverInteraction", RpcTarget.All);
                }
            }
            else
            {
                DoorInteractionManager.instance.SuccessLeverInteraction();
            }
        }
        else
        {
            canInteract = true;

            if (PhotonNetwork.IsConnected)
            {
                if (photonView != null)
                {
                    photonView.RPC("SyncCanInteract", RpcTarget.Others, true);
                }
            }
        }
    }

    [PunRPC]
    public void SuccessLeverInteraction()
    {
        DoorInteractionManager.instance.SuccessLeverInteraction();
    }

    [PunRPC]
    public void SyncCanInteract(bool value)
    {
        canInteract = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable") && canInteract && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            if (canvas != null)
            {
                canvas.gameObject.SetActive(true);
            }

            lastPlayer = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable") && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            if (canvas != null)
            {
                canvas.gameObject.SetActive(false);
            }
        }
    }
}

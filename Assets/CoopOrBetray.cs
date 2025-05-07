using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class CoopOrBetray : MonoBehaviourPun, IInteractable
{
    [SerializeField]
    private Canvas canvas;

    private bool canInteract = true;

    private void Start()
    {
        canvas.gameObject.SetActive(false);

        if (!PhotonNetwork.IsConnected || PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            canInteract = false;
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if (canInteract)
            {
                canInteract = false;
                photonView.RPC("Interact", RpcTarget.All);
            }
            else
            {
                UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIDialogPanel>().SetInfoText("��ȣ�ۿ��� �Ұ����մϴ�.");
            }
        }
    }

    [PunRPC]
    public void Interact()
    {
        canInteract = false;
        UIManager.Instance.OpenPopupPanelInOverlayCanvas<UICoopOrBetrayPanel>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable") && canInteract && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            canvas.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable") && canInteract && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            canvas.gameObject.SetActive(false);
        }
    }
}

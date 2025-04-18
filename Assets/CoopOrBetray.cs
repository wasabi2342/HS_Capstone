using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class CoopOrBetray : MonoBehaviourPun, IInteractable
{
    [SerializeField]
    private Canvas canvas;

    private bool canInteract = true;

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
                UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText("이미 상호작용이 완료되었습니다.");
            }
        }
    }

    [PunRPC]
    public void Interact()
    {
        canInteract = false;
        UIManager.Instance.OpenPopupPanel<UICoopOrBetrayPanel>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canInteract && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            canvas.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && canInteract && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            canvas.gameObject.SetActive(false);
        }
    }
}

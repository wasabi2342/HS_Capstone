using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class CoopOrBetray : MonoBehaviourPun, IInteractable
{
    [SerializeField]
    private Canvas canvas;

    public bool isInTutorial = false;

    public bool isEndStage = false;

    private bool canInteract = true;

    [SerializeField]
    private CoopType coopType = CoopType.defaultType;

    private void Start()
    {
        canvas.gameObject.SetActive(false);

        if (!PhotonNetwork.IsConnected || PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            canInteract = false;
        }

        if (isInTutorial || isEndStage)
        {
            canInteract = true;
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if (canInteract)
            {
                canInteract = false;

                //if(PhotonNetwork.CurrentRoom.PlayerCount < 2 || isInTutorial)
                photonView.RPC("Interact", RpcTarget.All);
                //else if (PhotonNetwork.OfflineMode)
                //{
                //     UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIDialogPanel>().SetInfoText("알파버전 클리어하셨습니다");
                //}
                //else if (!PhotonNetwork.OfflineMode)
                //{
                //
                //}
            }
            else
            {
                UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIDialogPanel>().SetInfoText("상호작용이 불가능합니다.");
            }
        }
    }

    [PunRPC]
    public void Interact()
    {
        canInteract = false;
        UIManager.Instance.OpenPopupPanelInOverlayCanvas<UICoopOrBetrayPanel>().Init(coopType);
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

using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class CoopOrBetray : MonoBehaviourPun, IInteractable
{    [SerializeField]
    private Canvas canvas;

    public bool isInTutorial = false;

    private bool canInteract = true;
    
    // 캔버스 활성화를 위한 public 메서드 추가
    public void ActivateCanvas(bool activate)
    {
        if (canvas != null)
        {
            canvas.gameObject.SetActive(activate);
        }
    }

    [SerializeField]
    private CoopType coopType = CoopType.defaultType;

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
            if (canInteract || isInTutorial)
            {
                canInteract = false;
                photonView.RPC("Interact", RpcTarget.All);
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

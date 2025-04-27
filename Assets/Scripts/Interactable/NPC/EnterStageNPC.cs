using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnterStageNPC : MonoBehaviour, IInteractable
{
    [SerializeField]
    private Canvas canvas;

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if (!RoomManager.Instance.isEnteringStage)
            {
                if (!RoomManager.Instance.IsPlayerInRestrictedArea())
                {
                    RoomManager.Instance.InteractWithDungeonNPC(); //.onClose += () => canControl = true;
                    //RoomManager.Instance.ReturnLocalPlayer().canControl = false;
                }
                else
                {
                    UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText("��� �÷��̾ ������ ���;� �մϴ�.");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable") && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            canvas.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable") && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            canvas.gameObject.SetActive(false);
        }
    }
}

using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectDoor : MonoBehaviourPun, IInteractable
{
    [SerializeField]
    private bool canInteract = true;
    [SerializeField]
    private Canvas canvas;

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        Debug.Log($"Reward Door 상호작용 함수 호출 ctx: {ctx.phase} canInteract:{canInteract}");
        if (canInteract) 
        {
            Debug.Log("Reward Door 상호작용 시작 - RewardUI 호출");
            canInteract = false;

            if (PhotonNetwork.IsConnected)
            {
                if (photonView != null)
                {
                    photonView.RPC("SyncCanInteract", RpcTarget.Others, false);
                }
            }

            PhotonNetworkManager.Instance.photonView.RPC("RPC_SaveRunTimeData", RpcTarget.All);
            // PhotonNetworkManager의 PhotonView를 사용하여 모든 클라이언트에 보상 UI를 열도록 RPC 호출
            //PhotonNetworkManager.Instance.photonView.RPC("RPC_OpenRewardUIForAll", RpcTarget.All);
            //InputManager.Instance.ChangeDefaultMap(InputDefaultMap.UI);
            PhotonNetworkManager.Instance.photonView.RPC("RPC_NextStage",RpcTarget.All);

        }
    }

    [PunRPC]
    public void SyncCanInteract(bool value)
    {
        canInteract = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (canInteract && (other.CompareTag("Interactable") && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine))))
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

using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectRewardDoor : MonoBehaviourPun, IInteractable
{
    [SerializeField]
    private bool canInteract = true;
    [SerializeField]
    private Canvas canvas;

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        Debug.Log("Reward Door 상호작용 함수 호출");
        if (ctx.started && canInteract)
        {
            Debug.Log("Reward Door 상호작용 시작 - RewardUI 호출");
            canInteract = false;
            // PhotonNetworkManager의 PhotonView를 사용하여 모든 클라이언트에 보상 UI를 열도록 RPC 호출
            PhotonNetworkManager.Instance.photonView.RPC("RPC_OpenRewardUIForAll", RpcTarget.All);
            InputManager.Instance.ChangeDefaultMap(InputDefaultMap.UI);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (canInteract && (other.transform.parent.CompareTag("Player") && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine))))
        {
            canvas.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.parent.CompareTag("Player") && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            canvas.gameObject.SetActive(false);
        }
    }
}

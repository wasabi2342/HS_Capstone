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
            // 자신의 PhotonView를 통해 RPC 호출: 모든 클라이언트에서 UIRewardPanel을 열도록 함.
            photonView.RPC("RPC_OpenRewardUIForAll", RpcTarget.All);
            InputManager.Instance.ChangeDefaultMap("UI");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (canInteract && (other.CompareTag("Player") && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine))))
        {
            canvas.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            canvas.gameObject.SetActive(false);
        }
    }

    [PunRPC]
    private void RPC_OpenRewardUIForAll()
    {
        // 모든 클라이언트에서 UIManager를 통해 UIRewardPanel 프리팹을 로드해 보상 UI를 엽니다.
        UIManager.Instance.OpenPopupPanel<UIRewardPanel>();
    }
}

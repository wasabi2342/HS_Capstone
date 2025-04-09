using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectRewardDoor : MonoBehaviour, IInteractable
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
            // UIManager의 OpenPopupPanel을 사용해 Reward UI 팝업을 띄웁니다.
            UIManager.Instance.OpenPopupPanel<UIRewardPanel>();
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
}

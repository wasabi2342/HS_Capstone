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
        Debug.Log("Reward Door ��ȣ�ۿ� �Լ� ȣ��");
        if (ctx.started && canInteract)
        {
            Debug.Log("Reward Door ��ȣ�ۿ� ���� - RewardUI ȣ��");
            canInteract = false;
            PhotonNetworkManager.Instance.photonView.RPC("RPC_SaveRunTimeData", RpcTarget.All);
            // PhotonNetworkManager�� PhotonView�� ����Ͽ� ��� Ŭ���̾�Ʈ�� ���� UI�� ������ RPC ȣ��
            //PhotonNetworkManager.Instance.photonView.RPC("RPC_OpenRewardUIForAll", RpcTarget.All);
            //InputManager.Instance.ChangeDefaultMap(InputDefaultMap.UI);
            PhotonNetworkManager.Instance.photonView.RPC("RPC_NextStage",RpcTarget.All);

        }
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

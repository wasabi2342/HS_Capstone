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
        Debug.Log($"Reward Door ��ȣ�ۿ� �Լ� ȣ�� ctx: {ctx.phase} canInteract:{canInteract}");
        if (canInteract) 
        {
            Debug.Log("Reward Door ��ȣ�ۿ� ���� - RewardUI ȣ��");
            canInteract = false;

            if (PhotonNetwork.IsConnected)
            {
                if (photonView != null)
                {
                    photonView.RPC("SyncCanInteract", RpcTarget.Others, false);
                }
            }

            PhotonNetworkManager.Instance.photonView.RPC("RPC_SaveRunTimeData", RpcTarget.All);
            // PhotonNetworkManager�� PhotonView�� ����Ͽ� ��� Ŭ���̾�Ʈ�� ���� UI�� ������ RPC ȣ��
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

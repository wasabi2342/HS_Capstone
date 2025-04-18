using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectBlessingNPC : MonoBehaviour, IInteractable
{
    [SerializeField]
    private bool canIneract = true;
    [SerializeField]
    private Canvas canvas;

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        Debug.Log("��ȣ�ۿ� �Լ� ȣ��");
        if (ctx.started && canIneract)
        {
            Debug.Log("��ȣ�ۿ� �Լ� ȣ�� if�� ����");
            canIneract = false;
            UIManager.Instance.OpenPopupPanel<UISelectBlessingPanel>();
            InputManager.Instance.ChangeDefaultMap(InputDefaultMap.UI);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (canIneract && (other.CompareTag("Player") && (!PhotonNetwork.InRoom ||
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

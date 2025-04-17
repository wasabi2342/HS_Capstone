using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChangeCharacterNPC : MonoBehaviour, IInteractable
{

    [SerializeField]
    private Canvas canvas;

    private GameObject player;

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            UIManager.Instance.OpenPopupPanel<UIChangeCharacterPanel>().GetCharacter(player);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            canvas.gameObject.SetActive(true);
            player = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            canvas.gameObject.SetActive(false);
            player = null;
        }
    }
}

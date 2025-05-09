using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Mannequin : GaugeInteraction
{
    [SerializeField]
    private GameObject characterPrefab;

    private GameObject player;

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable") && (!PhotonNetwork.InRoom ||
                   (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            if (other.GetComponentInParent<ParentPlayerController>().ReturnCharacterName() != characterPrefab.name) // 프리펩에서 이름 가져와야 함
            {
                player = other.transform.root.gameObject;
                base.OnTriggerEnter(other);
            }
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable") && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            player = null;
            base.OnTriggerExit(other);
        }
    }

    public override void OnInteract(InputAction.CallbackContext ctx)
    {
        base.OnInteract(ctx);
    }

    protected override void OnPerformedEvent()
    {
        if (player == null)
        {
            return;
        }
        base.OnPerformedEvent();
        CancelGaugeCoroutine(true);
        RoomManager.Instance.CreateCharacter(characterPrefab.name, player.transform.position, Quaternion.identity, true);
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.Destroy(player);
        }
        else
        {
            Destroy(player);
        }

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { "SelectCharacter", characterPrefab.name }
            });
        }
        else
        {
            PlayerPrefs.SetString("SelectCharacter", characterPrefab.name);
            PlayerPrefs.Save();
        }
    }

    protected override void OnCanceledEvent()
    {
        base.OnCanceledEvent();
    }

    protected override void OnStartedEvent()
    {
        base.OnStartedEvent();
    }
}

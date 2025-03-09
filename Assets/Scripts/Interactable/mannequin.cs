using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Mannequin : MonoBehaviour, IInteractable
{
    [SerializeField]
    private Image gauge;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private GameObject characterPrefab;

    private GameObject player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            player = other.transform.root.gameObject;
            canvas.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            player = null;
            canvas.gameObject.SetActive(false);
        }
    }

    public void StartGaugeCoroutine()
    {
        StartCoroutine(FillGauge());
    }

    public void CancelGaugeCoroutine(bool isComplete)
    {
        StopAllCoroutines();
        gauge.fillAmount = 0;
        if (isComplete)
        {
            canvas.gameObject.SetActive(false);
        }
    }

    private IEnumerator FillGauge()
    {
        float timeCount = 0;
        while (timeCount < 1)
        {
            timeCount += Time.deltaTime;
            gauge.fillAmount = timeCount;
            yield return null;
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            StartGaugeCoroutine();
        }
        else if (ctx.canceled)
        {
            CancelGaugeCoroutine(false);
        }
        else if (ctx.performed)
        {
            CancelGaugeCoroutine(true);
            RoomManager.Instance.CreateCharacter(characterPrefab, transform.position, transform.rotation, true);
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.Destroy(player);
            }
            else
            {
                Destroy(player);
            }
        }
    }
}

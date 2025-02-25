using Photon.Pun;
using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class Mannequin : Interactable
{
    [SerializeField]
    private Image gauge;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private GameObject characterPrefab;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<PhotonView>().IsMine || !PhotonNetwork.InRoom)
            {
                canvas.gameObject.SetActive(true);
                RoomMovement roomMovement = other.GetComponent<RoomMovement>();
                roomMovement.startFillGauge += StartGaugeCoroutine;
                roomMovement.canelFillGauge += CancelGaugeCoroutine;
                roomMovement.changeCharacterPrefab = characterPrefab;
            }
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<PhotonView>().IsMine || !PhotonNetwork.InRoom)
            {
                canvas.gameObject.SetActive(false);
                RoomMovement roomMovement = other.GetComponent<RoomMovement>();
                roomMovement.startFillGauge -= StartGaugeCoroutine;
                roomMovement.canelFillGauge -= CancelGaugeCoroutine;
                roomMovement.changeCharacterPrefab = null;
            }
        }
    }

    public void StartGaugeCoroutine()
    {
        StartCoroutine(FillGauge());
    }

    public void CancelGaugeCoroutine()
    {
        StopAllCoroutines();
        gauge.fillAmount = 0;
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
}

using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.InputSystem;
using System.Collections;

public class GaugeInteraction : MonoBehaviourPun, IInteractable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    protected Image gauge;
    [SerializeField]
    protected Canvas canvas;
    [SerializeField]
    public float holdTime = 1f;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable") && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            canvas.gameObject.SetActive(true);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable") && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
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
        while (timeCount < holdTime)
        {
            timeCount += Time.deltaTime;
            gauge.fillAmount = timeCount / holdTime;
            yield return null;
        }
        gauge.fillAmount = 1f;
        OnPerformedEvent();
    }

    public virtual void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            OnStartedEvent();
        }
        else if (ctx.canceled)
        {
            OnCanceledEvent();
        }
        //else if (ctx.performed)
        //{
        //    OnPerformedEvent();
        //}
    }

    protected virtual void OnPerformedEvent()
    {
        canvas.gameObject.SetActive(false);
    }
    protected virtual void OnCanceledEvent()
    {
        CancelGaugeCoroutine(false);
    }

    protected virtual void OnStartedEvent()
    {
        StartGaugeCoroutine();
    }
}

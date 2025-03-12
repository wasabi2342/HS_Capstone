using Photon.Pun;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    private InteractableObject thisPlace;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<BasePlayerController>().UpdateNowInteractable(thisPlace);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<BasePlayerController>().UpdateNowInteractable(InteractableObject.none);
        }
    }
}

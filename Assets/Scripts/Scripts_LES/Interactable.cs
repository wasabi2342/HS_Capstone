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
            other.GetComponent<RoomMovement>().UpdateNowInteractable(thisPlace);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<RoomMovement>().UpdateNowInteractable(InteractableObject.none);
        }
    }
}

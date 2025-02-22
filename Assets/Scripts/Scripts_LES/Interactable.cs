using Photon.Pun;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    private InteractableObject thisPlace;

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<RoomMovement>().UpdateNowInteractable(thisPlace);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<RoomMovement>().UpdateNowInteractable(InteractableObject.none);
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<RoomMovement>().UpdateNowInteractable(thisPlace);
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<RoomMovement>().UpdateNowInteractable(InteractableObject.none);
        }
    }
}

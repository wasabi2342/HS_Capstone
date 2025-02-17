using Photon.Pun;
using UnityEngine;

public class RestrictedArea : MonoBehaviour
{
    [SerializeField]
    private InteractablePlace thisPlace;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<RoomMovement>().UpdateNowPlace(thisPlace);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<RoomMovement>().UpdateNowPlace(InteractablePlace.none);
        }
    }
}

using UnityEngine;

public class Portal : Interactable
{
    [SerializeField]
    private Transform point;

    public Vector3 PortalExitPos()
    {
        return point.position;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<RoomMovement>().GetPortalExitPosition(point.position);
        }   
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
    }
}

using UnityEngine;

public class Portal : Interactable
{
    [SerializeField]
    private Transform point;

    public Vector3 PortalExitPos()
    {
        return point.position;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (other.CompareTag("Player"))
        {
            other.GetComponent<BasePlayerController>().GetPortalExitPosition(point.position);
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
    }
}

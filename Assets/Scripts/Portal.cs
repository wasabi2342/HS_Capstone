using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField]
    private Transform point;

    public Vector3 PortalExitPos()
    {
        return point.position;
    }
}

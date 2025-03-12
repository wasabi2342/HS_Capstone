using UnityEngine;

public class MovementRigidbody : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;
    private Rigidbody rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    public void MoveTo(Vector3 direction)
    {
        rigid.linearVelocity = direction * moveSpeed;
    }
}

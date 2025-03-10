using UnityEngine;

public class MovementRigidbody2D : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;
    private Rigidbody2D rigid2D;

    private void Awake()
    {
        rigid2D = GetComponent<Rigidbody2D>();
    }

    public void MoveTo(Vector3 direction)
    {
        rigid2D.linearVelocity = direction * moveSpeed;
    }
}

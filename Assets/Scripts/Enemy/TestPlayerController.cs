using UnityEngine;

public class TestPlayerController : MonoBehaviour
{
    private MovementRigidbody movement;

    private void Awake()
    {
        movement = GetComponent<MovementRigidbody>();
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        movement.MoveTo(new Vector3(x,y,0));
    }
}
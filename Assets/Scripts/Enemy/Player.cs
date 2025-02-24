using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody rb;
    public float moveSpeed = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal"); // InputManager의 Horizontal 축
        float moveZ = Input.GetAxis("Vertical");   // InputManager의 Vertical 축

        // 방향이 반대로 이동되면 여기에서 부호를 변경하세요.
        Vector3 moveDirection = new Vector3(-moveX, 0f, -moveZ).normalized;

        rb.MovePosition(transform.position + moveDirection * moveSpeed * Time.deltaTime);
    }
}

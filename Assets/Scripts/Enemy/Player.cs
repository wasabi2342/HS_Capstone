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
        float moveX = Input.GetAxis("Horizontal"); // InputManager�� Horizontal ��
        float moveZ = Input.GetAxis("Vertical");   // InputManager�� Vertical ��

        // ������ �ݴ�� �̵��Ǹ� ���⿡�� ��ȣ�� �����ϼ���.
        Vector3 moveDirection = new Vector3(-moveX, 0f, -moveZ).normalized;

        rb.MovePosition(transform.position + moveDirection * moveSpeed * Time.deltaTime);
    }
}
